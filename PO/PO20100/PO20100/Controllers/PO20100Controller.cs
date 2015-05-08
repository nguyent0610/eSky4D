using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using System.Text;

namespace PO20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class PO20100Controller : Controller
    {
        private string _screenNbr = "PO20100";
        private string _userName = Current.UserName;

        PO20100Entities _db = Util.CreateObjectContext<PO20100Entities>(false);

        public ActionResult Index()
        {  
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult GetPO_Price(string PriceID)
        {
            return this.Store(_db.PO20100_pgGetPOPrice(PriceID).ToList());
        }

        public ActionResult GetPO_PriceCpny(string PriceID)
        {
            return this.Store(_db.PO20100_pgGetPOPriceCpny(PriceID).ToList());
        }

        public ActionResult GetPOPriceHeader(string PriceID)
        {
            return this.Store(_db.PO_PriceHeader.Where(p => p.PriceID == PriceID));
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            //string PriceID = data["cboPriceID"];
            try
            {
                string PriceID = data["cboPriceID"];

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstPOPriceHeader"]);
                ChangeRecords<PO_PriceHeader> lstPOPriceHeader = dataHandler.BatchObjectData<PO_PriceHeader>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstPO_Price"]);
                ChangeRecords<PO20100_pgGetPOPrice_Result> lstPO_Price = dataHandler1.BatchObjectData<PO20100_pgGetPOPrice_Result>();

                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstPO_PriceCpny"]);
                ChangeRecords<PO20100_pgGetPOPriceCpny_Result> lstPO_PriceCpny = dataHandler2.BatchObjectData<PO20100_pgGetPOPriceCpny_Result>();
                
                #region Save header
                lstPOPriceHeader.Created.AddRange(lstPOPriceHeader.Updated);

                foreach (PO_PriceHeader curHeader in lstPOPriceHeader.Created)
                {
                   if (PriceID.PassNull() == "") continue;

                    var header = _db.PO_PriceHeader.FirstOrDefault(p => p.PriceID == PriceID);

                    if (header != null)
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(header, curHeader, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        header = new PO_PriceHeader();
                        header.PriceID = PriceID;

                        UpdatingHeader(header, curHeader, true);
                        _db.PO_PriceHeader.AddObject(header);
                    }
                }
                #endregion

                #region Save PO_Price
                foreach (PO20100_pgGetPOPrice_Result deleted in lstPO_Price.Deleted)
                {
                    var del = _db.PO_Price.Where(p => p.PriceID == PriceID && p.InvtID == deleted.InvtID && p.UOM==deleted.UOM).FirstOrDefault();
                    if (del != null)
                    {
                        _db.PO_Price.DeleteObject(del);
                    }
                }

                lstPO_Price.Created.AddRange(lstPO_Price.Updated);

                foreach (PO20100_pgGetPOPrice_Result curLang in lstPO_Price.Created)
                {
                    if (curLang.InvtID.PassNull() == "" || PriceID.PassNull()=="" || curLang.UOM.PassNull()=="") continue;

                    var lang = _db.PO_Price.FirstOrDefault(p => p.PriceID.ToLower() == PriceID.ToLower() && p.InvtID.ToLower() == curLang.InvtID.ToLower() && p.UOM.ToLower()== curLang.UOM.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_PO_Price(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new PO_Price();
                        lang.PriceID = PriceID;
                        Update_PO_Price(lang, curLang, true);
                        _db.PO_Price.AddObject(lang);
                    }
                }
                #endregion

                #region Save PO_PriceCpny
                foreach (PO20100_pgGetPOPriceCpny_Result deleted in lstPO_PriceCpny.Deleted)
                {
                    var objDelete = _db.PO_PriceCpny.Where(p => p.PriceID == deleted.PriceID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                    if (objDelete != null)
                    {
                        _db.PO_PriceCpny.DeleteObject(objDelete);
                    }
                }

                lstPO_PriceCpny.Created.AddRange(lstPO_PriceCpny.Updated);

                foreach (PO20100_pgGetPOPriceCpny_Result curLang in lstPO_PriceCpny.Created)
                {
                    if (curLang.CpnyID.PassNull() == "") continue;

                    var lang = _db.PO_PriceCpny.FirstOrDefault(p => p.PriceID.ToLower() == curLang.PriceID.ToLower()  && p.CpnyID.ToLower()  == curLang.CpnyID.ToLower());

                    if (lang != null)
                    {
                        lang.CpnyID = curLang.CpnyID;
                    }
                    else
                    {
                        lang = new PO_PriceCpny();
                        lang.PriceID = PriceID;
                        lang.CpnyID=curLang.CpnyID;
                        _db.PO_PriceCpny.AddObject(lang);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(PO_PriceHeader t, PO_PriceHeader s,bool isNew)
        {
            if (isNew)
            {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;
            t.Disc = s.Disc;
            t.EffDate = s.EffDate;
            t.HOCreate = s.HOCreate;
            t.Public = s.Public;
            t.Status = s.Status;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_PO_Price(PO_Price t,PO20100_pgGetPOPrice_Result s,bool isNew)
        {
            if (isNew)
            {
                t.VendID = "*";
                t.InvtID = s.InvtID;
                t.UOM = s.UOM;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Price = s.Price;
            t.Descr = s.Descr;
            t.QtyBreak = s.QtyBreak;
            t.Disc = s.Disc;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string PriceID = data["cboPriceID"];
                var obj = _db.PO_PriceHeader.FirstOrDefault(p => p.PriceID == PriceID);
                if (obj != null)
                {
                    _db.PO_PriceHeader.DeleteObject(obj);
                }

                var lstPO_Price = _db.PO_Price.Where(p => p.PriceID == PriceID).ToList();
                foreach (var item in lstPO_Price)
                {
                    _db.PO_Price.DeleteObject(item);
                }

                var lstPO_PriceCpny = _db.PO_PriceCpny.Where(p => p.PriceID == PriceID).ToList();
                foreach (var item in lstPO_PriceCpny)
                {
                    _db.PO_PriceCpny.DeleteObject(item);
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
    }
}

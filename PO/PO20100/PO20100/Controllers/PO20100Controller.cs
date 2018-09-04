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
            bool noPriceCalculation = false;
            bool hidebtnCopy = false;
            bool hideChkPublic = false;
            var objConfig = _db.PO20100_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();            
            if (objConfig != null)
            {
                noPriceCalculation = objConfig.NoPriceCalculation.HasValue && objConfig.NoPriceCalculation.Value;
                hidebtnCopy = objConfig.hidebtnCopy.HasValue && objConfig.hidebtnCopy.Value;
                hideChkPublic = objConfig.hideChkPublic.HasValue && objConfig.hideChkPublic.Value;
            }
            ViewBag.hidebtnCopy = hidebtnCopy;
            ViewBag.noPriceCalculation = noPriceCalculation;
            ViewBag.hideChkPublic = hideChkPublic;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
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
        public ActionResult Save(FormCollection data,bool copy)
        {
            //string PriceID = data["cboPriceID"];
            try
            {
                string PriceID = data["cboPriceID"];
                
                //var discInfoHandler = new StoreDataHandler(data["lstDiscInfo"]);
                //var inputDisc = discInfoHandler.ObjectData<OM_Discount>()
                //            .FirstOrDefault(p => p.DiscID == discID);
                bool noPriceCalculation = false;
                var objconfig = _db.PO20100_pdConfig(Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault();
                if (objconfig != null)
                {
                    noPriceCalculation = objconfig.NoPriceCalculation.HasValue && objconfig.NoPriceCalculation.Value;
                }

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstPOPriceHeader"]);
                var curHeader = dataHandler.ObjectData<PO_PriceHeader>().FirstOrDefault();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstPO_Price"]);
                ChangeRecords<PO20100_pgGetPOPrice_Result> lstPO_Price = dataHandler1.BatchObjectData<PO20100_pgGetPOPrice_Result>();

                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstPO_PriceCpny"]);
                ChangeRecords<PO20100_pgGetPOPriceCpny_Result> lstPO_PriceCpny = dataHandler2.BatchObjectData<PO20100_pgGetPOPriceCpny_Result>();

                StoreDataHandler dataHandler3 = new StoreDataHandler(data["lstPO_PriceCopy"]);
                List<PO20100_pgGetPOPrice_Result> lstPO_PriceCopy = dataHandler3.ObjectData<PO20100_pgGetPOPrice_Result>();

                StoreDataHandler dataHandler4 = new StoreDataHandler(data["lstPO_PriceCpnyCopy"]);
                List<PO20100_pgGetPOPriceCpny_Result> lstPO_PriceCpnyCopy = dataHandler4.ObjectData<PO20100_pgGetPOPriceCpny_Result>();
                #region Save header
                // lstPOPriceHeader.Created.AddRange(lstPOPriceHeader.Updated);

                //foreach (PO_PriceHeader curHeader in lstPOPriceHeader.Created)
                //{
                //  if (PriceID.PassNull() == "") continue;

                var header = _db.PO_PriceHeader.FirstOrDefault(p => p.PriceID == PriceID);
                bool isNew = false;
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        isNew = false;
                        //UpdatingHeader(header, curHeader, false);
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
                    isNew = true;
                    
                    _db.PO_PriceHeader.AddObject(header);
                }
                UpdatingHeader(header, curHeader, isNew);
                #region Save PO_PriceCpny
                if (header.Public)
                {
                    var lstDelCpny = _db.PO_PriceCpny.Where(p => p.PriceID == PriceID).ToList();
                    foreach (var objDelete in lstDelCpny)
                    {
                        if (objDelete != null)
                        {
                            _db.PO_PriceCpny.DeleteObject(objDelete);
                        }
                    }                        
                }
                else
                {
                    lstPO_PriceCpny.Created.AddRange(lstPO_PriceCpny.Updated);
                    foreach (var deleted in lstPO_PriceCpny.Deleted)
                    {
                        //neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                        if (lstPO_PriceCpny.Created.Where(p => p.CpnyID == deleted.CpnyID).Count() > 0)
                        {
                            // lstPO_PriceCpny.Created.Where(p => p.CpnyID == deleted.CpnyID).FirstOrDefault().tt = del.tstamp;
                        }
                        else
                        {
                            var objDelete = _db.PO_PriceCpny.Where(p => p.PriceID == header.PriceID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                            if (objDelete != null)
                            {
                                _db.PO_PriceCpny.DeleteObject(objDelete);
                            }
                        }
                    }

                    if (copy)
                    {
                        foreach(PO20100_pgGetPOPriceCpny_Result item in lstPO_PriceCpnyCopy)
                        {
                            if (item.CpnyID.PassNull() == "" || PriceID.ToLower() != header.PriceID.ToLower()) continue;
                            var lang = _db.PO_PriceCpny.FirstOrDefault(p => p.PriceID.ToLower() == header.PriceID.ToLower() && p.CpnyID.ToLower() == item.CpnyID.ToLower());

                            if (lang != null)
                            {
                                lang.CpnyID = item.CpnyID;
                            }
                            else
                            {
                                lang = new PO_PriceCpny();
                                lang.PriceID = PriceID;
                                lang.CpnyID = item.CpnyID;
                                _db.PO_PriceCpny.AddObject(lang);
                            }
                        }
                    }
                    else
                    {
                        foreach (PO20100_pgGetPOPriceCpny_Result curLang in lstPO_PriceCpny.Created)
                        {
                            if (curLang.CpnyID.PassNull() == "" || PriceID.ToLower() != header.PriceID.ToLower()) continue;

                            var lang = _db.PO_PriceCpny.FirstOrDefault(p => p.PriceID.ToLower() == header.PriceID.ToLower() && p.CpnyID.ToLower() == curLang.CpnyID.ToLower());

                            if (lang != null)
                            {
                                lang.CpnyID = curLang.CpnyID;
                            }
                            else
                            {
                                lang = new PO_PriceCpny();
                                lang.PriceID = PriceID;
                                lang.CpnyID = curLang.CpnyID;
                                _db.PO_PriceCpny.AddObject(lang);
                            }
                        }
                    }

                    
                }
                #endregion

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

                if (copy)
                {
                    foreach (PO20100_pgGetPOPrice_Result item in lstPO_PriceCopy)
                    {
                        if (item.InvtID.PassNull() == "" || PriceID.PassNull() == "" || item.UOM.PassNull() == "") continue;

                        var lang = _db.PO_Price.FirstOrDefault(p => p.PriceID.ToLower() == PriceID.ToLower() && p.InvtID.ToLower() == item.InvtID.ToLower() && p.UOM.ToLower() == item.UOM.ToLower());

                        if (lang != null)
                        {
                            if (lang.tstamp.ToHex() == item.tstamp.ToHex())
                            {
                                Update_PO_Price(lang, item, noPriceCalculation, false);
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
                            Update_PO_Price(lang, item, noPriceCalculation, true);
                            _db.PO_Price.AddObject(lang);
                        }
                    }
                }
                else
                {
                    foreach (PO20100_pgGetPOPrice_Result curLang in lstPO_Price.Created)
                    {
                        if (curLang.InvtID.PassNull() == "" || PriceID.PassNull() == "" || curLang.UOM.PassNull() == "") continue;

                        var lang = _db.PO_Price.FirstOrDefault(p => p.PriceID.ToLower() == PriceID.ToLower() && p.InvtID.ToLower() == curLang.InvtID.ToLower() && p.UOM.ToLower() == curLang.UOM.ToLower());

                        if (lang != null)
                        {
                            if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                            {
                                Update_PO_Price(lang, curLang, noPriceCalculation, false);
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
                            Update_PO_Price(lang, curLang, noPriceCalculation, true);
                            _db.PO_Price.AddObject(lang);
                        }
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

        private void Update_PO_Price(PO_Price t,PO20100_pgGetPOPrice_Result s,bool noPriceCalculation, bool isNew)
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
            t.Descr = s.Descr;
            t.QtyBreak = s.QtyBreak;
            t.Disc = s.Disc;           
            if (noPriceCalculation)
            {
                t.Price = s.Price;
            }
            else
            {
                t.Price = s.Price + ((s.Price * s.Disc.ToDouble()) / 100);
            }
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

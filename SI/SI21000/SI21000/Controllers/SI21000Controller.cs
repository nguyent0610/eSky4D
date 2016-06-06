using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace SI21000.Controllers
{
    [DirectController]
    public class SI21000Controller : Controller
    {
        private string _screenNbr = "SI21000";
        SI21000Entities _db = Util.CreateObjectContext<SI21000Entities>(false);

        // GET: /SI21000/
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        // Get collection of Vendor for binding data (Ajax)
        public ActionResult GetTax(string TaxID)
        {
            var vendor = _db.SI_Tax.FirstOrDefault(p => p.TaxID == TaxID);
            return this.Store(vendor);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                // Get params from data that's sent from client (Ajax)
                string taxID = data["cboTaxID"].PassNull();
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstTax"]);
                //ChangeRecords<SI_Tax> lstTax = dataHandler.BatchObjectData<SI_Tax>();
                #region Old
                //foreach (SI_Tax createdTax in lstTax.Updated)
                //{
                //    var objHeader = _db.SI_Tax.FirstOrDefault(p => p.TaxID == taxID);
                //    if (isNew)//new record
                //    {
                //        if (objHeader != null)
                //            return Json(new { success = false, msgCode = 2000, msgParam = taxID });//quang message ma nha cung cap da ton tai ko the them
                //        else
                //        {
                //            objHeader = new SI_Tax();
                //            objHeader.TaxID = taxID;
                //            objHeader.Crtd_DateTime = DateTime.Now;
                //            objHeader.Crtd_Prog = _screenNbr;
                //            objHeader.Crtd_User = Current.UserName;
                //            UpdatingHeader(createdTax, ref objHeader);
                //            // Add data to SI_Tax
                //            _db.SI_Tax.AddObject(objHeader);
                //            _db.SaveChanges();
                //        }
                //    }
                //    else if (objHeader != null)//update record
                //    {
                //        if (objHeader.tstamp.ToHex() == createdTax.tstamp.ToHex())
                //        {
                //            UpdatingHeader(createdTax, ref objHeader);
                //        }
                //        else
                //        {
                //            throw new MessageException(MessageType.Message, "19");
                //        }
                //        _db.SaveChanges();

                //    }
                //    else
                //    {
                //        throw new MessageException(MessageType.Message, "19");
                //    }
                //}
                #endregion
                #region New Save
                var curHeader = dataHandler.ObjectData<SI_Tax>().FirstOrDefault();
                var header = _db.SI_Tax.FirstOrDefault(p => p.TaxID == taxID);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(curHeader,ref header ,false);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new SI_Tax();
                    header.ResetET();
                    header.TaxID = taxID;
                    
                    UpdatingHeader(curHeader,ref header ,true);
                    _db.SI_Tax.AddObject(header);
                }
                #endregion
                _db.SaveChanges();

                return Json(new { success = true, TaxID = taxID }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        // Delete a SI_Tax
        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string taxID = data["cboTaxID"].PassNull();
                var cpny = _db.SI_Tax.FirstOrDefault(p => p.TaxID == taxID);
                if (cpny != null)
                {
                    _db.SI_Tax.DeleteObject(cpny);

                }

                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }

        }

        private void UpdatingHeader(SI_Tax s, ref SI_Tax t, bool isNew)
        {
            if(isNew){

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
            }
            t.Descr = s.Descr;
            t.TaxRate = s.TaxRate;
            t.TaxBasis = s.TaxBasis;
            t.TaxCalcType = s.TaxCalcType;
            t.TaxCalcLvl = s.TaxCalcLvl;
            t.TxblMax = s.TxblMax;
            t.TxblMin = s.TxblMin;
            t.InclFrt = s.InclFrt == "True" ? "1" : "0";
            t.Inclmisc = s.Inclmisc == "True" ? "1" : "0";
            t.PrcTaxIncl = s.PrcTaxIncl == "True" ? "1" : "0";
            t.Lvl2Exmpt = s.Lvl2Exmpt;
            t.InclInDocTot = s.InclInDocTot == "True" ? "1" : "0";
            t.ApplTermDisc = s.ApplTermDisc == "True" ? "1" : "0";
            t.ApplTermsDiscTax = s.ApplTermsDiscTax;
            t.AdjByTermsDisc = s.AdjByTermsDisc == "True" ? "1" : "0";
            t.ARTaxPtDate = s.ARTaxPtDate;
            t.APTaxPtDate = s.APTaxPtDate;
            t.OPTaxPtDate = s.OPTaxPtDate;
            t.POTaxPtDate = s.POTaxPtDate;
            t.CatFlg = s.CatFlg;
            t.CatExcept00 = s.CatExcept00;
            t.CatExcept01 = s.CatExcept01;
            t.CatExcept02 = s.CatExcept02;
            t.CatExcept03 = s.CatExcept03;
            t.CatExcept04 = s.CatExcept04;
            t.CatExcept05 = s.CatExcept05;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
        }
    }
}

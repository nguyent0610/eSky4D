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
namespace AP20200.Controllers
{
    [DirectController]
    public class AP20200Controller : Controller
    {
        private string _screenNbr = "AP20200";
        AP20200Entities _db = Util.CreateObjectContext<AP20200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
      
        // GET: /AP20200/
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
        public ActionResult GetAPVendorHeader(string vendID)
        {
            var vendor = _db.AP_Vendor.FirstOrDefault(p => p.VendID == vendID);
            return this.Store(vendor);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(FormCollection data, bool isNew)
        {
            try
            {
                // Get params from data that's sent from client (Ajax)
                string vendID = data["cboVendID"];
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAPVendorHeader"]);
                ChangeRecords<AP_Vendor> lstAPVendorHeader = dataHandler.BatchObjectData<AP_Vendor>();
                foreach (AP_Vendor createdVendor in lstAPVendorHeader.Updated)
                {
                    var objHeader = _db.AP_Vendor.FirstOrDefault(p => p.VendID == vendID);
                    if (isNew)//new record
                    {
                        if (objHeader != null)
                            return Json(new { success = false, msgCode = 2000, msgParam = vendID });//quang message ma nha cung cap da ton tai ko the them
                        else
                        {
                            objHeader = new AP_Vendor();
                            objHeader.VendID = vendID;
                            objHeader.Crtd_DateTime = DateTime.Now;
                            objHeader.Crtd_Prog = _screenNbr;
                            objHeader.Crtd_User = Current.UserName;
                            UpdatingHeader(createdVendor, ref objHeader);
                            // Add data to AP_Vendor
                            _db.AP_Vendor.AddObject(objHeader);
                            _db.SaveChanges();                        
                        }
                    }
                    else if (objHeader != null)//update record
                    {
                        if (objHeader.tstamp.ToHex() == createdVendor.tstamp.ToHex())
                        {
                            UpdatingHeader(createdVendor, ref objHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }               
                        _db.SaveChanges();
                       
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        // Delete a AP_Vendor
        [HttpPost]
        public ActionResult Delete(string vendID)
        {
            try
            {
                var cpny = _db.AP_Vendor.FirstOrDefault(p => p.VendID == vendID);
                if (cpny != null)
                {
                    _db.AP_Vendor.DeleteObject(cpny);

                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
           
        }

        private void UpdatingHeader(AP_Vendor s, ref AP_Vendor d)
        {
            d.Addr1 = s.Addr1;
            d.Addr2 = s.Addr2;
            d.Attn = s.Attn;
            d.City = s.City;
            d.ClassID = s.ClassID;
            d.Country = s.Country;
            d.DfltOrdFromId = s.DfltOrdFromId;
            d.EMailAddr = s.EMailAddr;
            d.ExpAcct = s.ExpAcct;
            d.ExpSub = s.ExpSub;
            d.Fax = s.Fax;
            d.Name = s.Name;
            d.Phone = s.Phone;
            d.RemitAddr1 = s.RemitAddr1;
            d.RemitAddr2 = s.RemitAddr2;
            d.RemitAttn = s.RemitAttn;
            d.RemitCity = s.RemitCity;
            d.RemitCountry = s.RemitCountry;
            d.RemitFax = s.RemitFax;
            d.RemitName = s.RemitName;
            d.RemitPhone = s.RemitPhone;
            d.RemitSalut = s.RemitSalut;
            d.RemitState = s.RemitState;
            d.RemitZip = s.RemitZip;
            d.Salut = s.Salut;
            d.State = s.State;
            d.Status = s.Status;
            d.TaxDflt = s.TaxDflt;
            d.TaxId00 = s.TaxId00;
            d.TaxId01 = s.TaxId01;
            d.TaxId02 = s.TaxId02;
            d.TaxId03 = s.TaxId03;
            d.TaxLocId = s.TaxLocId;
            d.TaxRegNbr = s.TaxRegNbr;
            d.Terms = s.Terms;
            d.Zip = s.Zip;
            d.CrLmt = s.CrLmt;
            d.PmtMethod = s.PmtMethod;
            d.MOQVal = s.MOQVal;
            d.MOQType = s.MOQType;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
        }   
    }
}

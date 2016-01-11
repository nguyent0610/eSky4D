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
        private string _userName = Current.UserName;

        AP20200Entities _db = Util.CreateObjectContext<AP20200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

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
        public ActionResult GetAPVendorHeader(string VendID)
        {
            var vendor = _db.AP_Vendor.FirstOrDefault(p => p.VendID == VendID);
            return this.Store(vendor);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string VendID = data["cboVendID"].PassNull();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstVendor"]);
                var curHeader = dataHandler1.ObjectData<AP_Vendor>().FirstOrDefault();

                #region Save AP_Vendor
                var header = _db.AP_Vendor.FirstOrDefault(p => p.VendID == VendID);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new AP_Vendor();
                    header.ResetET();
                    header.VendID = VendID;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader);
                    _db.AP_Vendor.AddObject(header);
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true , VendID = VendID }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string VendID = data["cboVendID"].PassNull();

                var obj = _db.AP_Vendor.FirstOrDefault(p => p.VendID == VendID);
                if (obj != null)
                {
                    _db.AP_Vendor.DeleteObject(obj);
                }

                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(ref AP_Vendor t, AP_Vendor s)
        {
            t.Addr1 = s.Addr1;
            t.Addr2 = s.Addr2;
            t.Attn = s.Attn;
            t.City = s.City;
            t.ClassID = s.ClassID;
            t.Country = s.Country;
            t.DfltOrdFromId = s.DfltOrdFromId;
            t.EMailAddr = s.EMailAddr;
            t.ExpAcct = s.ExpAcct;
            t.ExpSub = s.ExpSub;
            t.Fax = s.Fax;
            t.Name = s.Name;
            t.Phone = s.Phone;
            t.RemitAddr1 = s.RemitAddr1;
            t.RemitAddr2 = s.RemitAddr2;
            t.RemitAttn = s.RemitAttn;
            t.RemitCity = s.RemitCity;
            t.RemitCountry = s.RemitCountry;
            t.RemitFax = s.RemitFax;
            t.RemitName = s.RemitName;
            t.RemitPhone = s.RemitPhone;
            t.RemitSalut = s.RemitSalut;
            t.RemitState = s.RemitState;
            t.RemitZip = s.RemitZip;
            t.Salut = s.Salut;
            t.State = s.State;
            t.Status = s.Status;
            t.TaxDflt = s.TaxDflt;
            t.TaxId00 = s.TaxId00;
            t.TaxId01 = s.TaxId01;
            t.TaxId02 = s.TaxId02;
            t.TaxId03 = s.TaxId03;
            t.TaxLocId = s.TaxLocId;
            t.TaxRegNbr = s.TaxRegNbr;
            t.Terms = s.Terms;
            t.Zip = s.Zip;
            t.CrLmt = s.CrLmt;
            t.PmtMethod = s.PmtMethod;
            t.MOQVal = s.MOQVal;
            t.MOQType = s.MOQType;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

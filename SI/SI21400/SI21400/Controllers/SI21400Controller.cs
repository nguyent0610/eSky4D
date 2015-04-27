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
namespace SI21400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21400Controller : Controller
    {
        private string _screenNbr = "SI21400";
        private string _userName = Current.UserName;
        SI21400Entities _db = Util.CreateObjectContext<SI21400Entities>(false);

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

        public ActionResult GetSI_Address(string AddrID)
        {
            return this.Store(_db.SI_Address.FirstOrDefault(p => p.AddrID == AddrID));
        }

        #region Save & Update
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string AddrID = data["cboAddrID"];

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Address"]);
                ChangeRecords<SI_Address> lstSI_Address = dataHandler.BatchObjectData<SI_Address>();

                #region Save Header Company
                lstSI_Address.Created.AddRange(lstSI_Address.Updated);
                foreach (SI_Address curHeader in lstSI_Address.Created)
                {
                    if (AddrID.PassNull() == "") continue;

                    var header = _db.SI_Address.FirstOrDefault(p => p.AddrID == AddrID);
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
                        //string images = getPathThenUploadImage(curHeader, UserID);
                        header = new SI_Address();
                        header.AddrID = AddrID;
                        header.Crtd_DateTime = DateTime.Now;
                        header.Crtd_Prog = _screenNbr;
                        header.Crtd_User = Current.UserName;
                        UpdatingHeader(ref header, curHeader);
                        _db.SI_Address.AddObject(header);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, AddrID = AddrID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        private void UpdatingHeader(ref SI_Address t, SI_Address s)
        {
            t.Addr1 = s.Addr1;
            t.Addr2 = s.Addr2;
            t.Attn = s.Attn;
            t.Country = s.Country;
            t.Fax = s.Fax;
            t.Name = s.Name;
            t.Phone = s.Phone;
            t.Salut = s.Salut;
            t.State = s.State;
            t.TaxId00 = s.TaxId00;
            t.TaxId01 = s.TaxId01;
            t.TaxId02 = s.TaxId02;
            t.TaxId03 = s.TaxId03;
            t.TaxLocId = s.TaxLocId;
            t.TaxRegNbr = s.TaxRegNbr;
            t.Zip = s.Zip;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        #region Delete information Company
        //Delete information Company
        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string AddrID = data["cboAddrID"];
                var obj = _db.SI_Address.FirstOrDefault(p => p.AddrID == AddrID);
                if (obj != null)
                {
                    _db.SI_Address.DeleteObject(obj);
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
        #endregion
    }
}

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
namespace AP00000.Controllers
{
    [DirectController]
    public class AP00000Controller : Controller
    {
        private string _screenNbr = "AP00000";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;

        AP00000Entities _db = Util.CreateObjectContext<AP00000Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        // Get collection of Vendor for binding data (Ajax)
        public ActionResult GetAP00000Header(string branchId, string setupID)
        {
            var setupData = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupID == setupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string BranchID = _cpnyID;
                string setupID = "AP";

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSetup"]);

                var curHeader = dataHandler1.ObjectData<AP_Setup>().FirstOrDefault();
                
                #region Save AP_Setup

                var header = _db.AP_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupID == setupID);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader,false);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new AP_Setup();
                    header.ResetET();
                    //header.BranchID = BranchID;
                    //header.Crtd_DateTime = DateTime.Now;
                    //header.Crtd_Prog = _screenNbr;
                    //header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader,true);
                    _db.AP_Setup.AddObject(header);
                }
                #endregion
                
                _db.SaveChanges();
                return Json(new { success = true , VendID = BranchID }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }



        private void UpdatingHeader(ref AP_Setup t, AP_Setup s, bool isNew)
        {
            if (isNew)
            {
                //t.CpnyID = s.CpnyID;
                t.BranchID = _cpnyID;
                t.SetupID = "AP";
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.ClassID = s.ClassID;
            t.DfltBankAcct = s.DfltBankAcct;
            t.LastBatNbr = s.LastBatNbr;
            t.LastRefNbr = s.LastRefNbr;
            t.LastPaymentNbr = s.LastPaymentNbr;
            t.PreFixBat = s.PreFixBat;
            t.TranDescDflt = s.TranDescDflt;
            t.terms = s.terms;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

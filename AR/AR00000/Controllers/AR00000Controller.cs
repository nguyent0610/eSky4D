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
namespace AR00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR00000Controller : Controller
    {
        private string _screenNbr = "AR00000";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;

        AR00000Entities _db = Util.CreateObjectContext<AR00000Entities>(false);
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


        public ActionResult GetAR00000Header(string branchId, string setupID)
        {
            var setupData = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupId == setupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
           
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAR00000Header"]);
                //ChangeRecords<AR_Setup> lstSetup = dataHandler.BatchObjectData<AR_Setup>();

                var curHeader = dataHandler.ObjectData<AR_Setup>().FirstOrDefault();

                //lstSetup.Created.AddRange(lstSetup.Updated);

                string branchId = data["BranchID"];
                string setupID = "AR";
                #region Save AP_Setup

                var header = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupId == setupID);

                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        Update_Setup(ref header, curHeader, false);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new AR_Setup();
                    header.ResetET();
                    Update_Setup(ref header, curHeader, true);
                    _db.AR_Setup.AddObject(header);
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_Setup(ref AR_Setup s, AR_Setup d, bool isNew)
        {
            if (isNew)
            {
                s.BranchID = _cpnyID;
                s.SetupId = "AR";
                s.Crtd_DateTime = DateTime.Now;
                s.Crtd_Prog = _screenNbr;
                s.Crtd_User = _userName;
            }

            s.LastBatNbr = d.LastBatNbr;
            s.AutoCustID = d.AutoCustID == null ? false : d.AutoCustID;
            s.LastRefNbr = d.LastRefNbr;
            s.AutoSlsperID = d.AutoSlsperID == null ? false : d.AutoSlsperID;
            s.LastReceiptNbr = d.LastReceiptNbr;
            s.HiddenHierarchy = d.AutoSlsperID == null ? false : d.HiddenHierarchy;

            s.LastCustID = d.LastCustID;
            s.LastSlsID = d.LastSlsID;
            s.DfltShipViaID = d.DfltShipViaID;
            s.PrefixBat = d.PrefixBat;
            s.TranDescDflt = d.TranDescDflt;
            s.AddressLevel = d.AddressLevel;
            s.DfltBankAcct = d.DfltBankAcct;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
            s.LUpd_User = _userName;
        }

    }
}

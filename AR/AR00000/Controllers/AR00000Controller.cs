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
namespace AR00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR00000Controller : Controller
    {
        private string _screenNbr = "AR00000";
        private string _userName = Current.UserName;

        AR00000Entities _db = Util.CreateObjectContext<AR00000Entities>(false);

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

     
        public ActionResult GetAR00000Header(string branchId, string setupID)
        {
            var setupData = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupId == setupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            string branchId = Current.CpnyID;         
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAR00000Header"]);
                ChangeRecords<AR_Setup> lstSetup = dataHandler.BatchObjectData<AR_Setup>();
                

                lstSetup.Created.AddRange(lstSetup.Updated);

                foreach (AR_Setup curSetup in lstSetup.Created)
                {
                    if (curSetup.BranchID.PassNull() == "") continue;

                    var Setup = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupId == "AR");

                    if (Setup != null)
                    {
                        if (Setup.tstamp.ToHex() == curSetup.tstamp.ToHex())
                        {
                            Update_Setup(Setup, curSetup, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Setup = new AR_Setup();
                        Update_Setup(Setup, curSetup, true);
                        _db.AR_Setup.AddObject(Setup);
                    }
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
        [DirectMethod]
        public ActionResult Delete(string branchId)
        {
            branchId = Current.CpnyID;
            var cpny = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId);
            if (cpny != null)
            {
                _db.AR_Setup.DeleteObject(cpny);

            }   
            _db.SaveChanges();
            return this.Direct();
        }
        private void Update_Setup(AR_Setup s, AR_Setup d, bool isNew)
        {
            if (isNew)
            {
                s.BranchID = d.BranchID;
                s.SetupId = "AR";
                s.Crtd_DateTime = DateTime.Now;
                s.Crtd_Prog = _screenNbr;
                s.Crtd_User = _userName;
            }

            s.LastBatNbr =d.LastBatNbr;
            s.AutoCustID = d.AutoCustID == null ? false : d.AutoCustID;
            s.LastRefNbr = d.LastRefNbr;
            s.AutoSlsperID = d.AutoSlsperID == null ? false : d.AutoSlsperID;
            s.LastReceiptNbr =d.LastReceiptNbr;
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

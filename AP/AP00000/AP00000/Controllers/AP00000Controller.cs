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
namespace AP00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AP00000Controller : Controller
    {
        private string _screenNbr = "AP00000";
        private string _userName = Current.UserName;

        AP00000Entities _db = Util.CreateObjectContext<AP00000Entities>(false);

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

        //public ActionResult GetLanguage()
        //{
        //    return this.Store(_db.SYS_Language.ToList());
        //}
        public ActionResult GetAP00000Header(string branchId, string setupID)
        {
            var setupData = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupID == setupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            string branchId = Current.CpnyID;
            string dfltBankAcct = data["cboBankAcct"];
            string classID = data["cboClassID"];
            string lastBatNbr = data["txtLastBatNbr"];
            string lastRefNbr = data["txtLastRefNbr"];
            string lastPaymentNbr = data["txtLastPaymentNbr"];
            string preFixBat = data["txtPreFixBat"];
            string tranDescDef = data["cboTranDescDef"];
            string terms = data["cboTermsID"];


            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAP00000Header"]);
                ChangeRecords<AP_Setup> lstSetup = dataHandler.BatchObjectData<AP_Setup>();
                

                lstSetup.Created.AddRange(lstSetup.Updated);

                foreach (AP_Setup curSetup in lstSetup.Created)
                {
                    if (curSetup.BranchID.PassNull() == "") continue;

                    var Setup = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupID == "AP");

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
                        Setup = new AP_Setup();
                        Setup.DfltBankAcct = dfltBankAcct; // ???
                        Setup.ClassID = classID;
                        Setup.LastBatNbr = lastBatNbr;
                        Setup.LastRefNbr = lastRefNbr;
                        Setup.LastPaymentNbr = lastPaymentNbr;
                        Setup.PreFixBat = preFixBat;
                        Setup.TranDescDflt = tranDescDef;
                        Setup.terms = terms;

                        Update_Setup(Setup, curSetup, true);
                        _db.AP_Setup.AddObject(Setup);
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
            var cpny = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId);
            if (cpny != null)
            {
                _db.AP_Setup.DeleteObject(cpny);

            }   
            _db.SaveChanges();
            return this.Direct();
        }
        private void Update_Setup(AP_Setup s, AP_Setup d, bool isNew)
        {
            if (isNew)
            {
                s.BranchID = d.BranchID;
                s.SetupID = "AP";
                s.Crtd_DateTime = DateTime.Now;
                s.Crtd_Prog = _screenNbr;
                s.Crtd_User = _userName;
            }
            s.ClassID = d.ClassID;
            s.DfltBankAcct = d.DfltBankAcct;
            s.LastBatNbr = d.LastBatNbr;
            s.LastRefNbr = d.LastRefNbr;
            s.LastPaymentNbr = d.LastPaymentNbr;
            s.PreFixBat = d.PreFixBat;
            s.TranDescDflt = d.TranDescDflt;
            s.terms = d.terms;
            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
            s.LUpd_User = _userName;
        }
        
    }
}

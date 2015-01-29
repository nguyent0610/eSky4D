using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using eBiz4DWebFrame;
using eBiz4DWebSys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using HQSendMailApprove;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace AP00000.Controllers
{
    [DirectController]
    public class AP00000Controller : Controller
    {
        private string _screenName = "AP00000";
        
        AP00000Entities _db = Util.CreateObjectContext<AP00000Entities>(false);
        eBiz4DWebSysEntities _sys = Util.CreateObjectContext<eBiz4DWebSysEntities>(true);

        string test = Current.CpnyID;

        

        //
        // GET: /AP00000/
        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        // Get collection of Sales Person in a branch for binding data (Ajax)
        public ActionResult GetAP00000Header(string branchId, string setupID)
        {
            var setupData = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupID == setupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            // Get params from data that's sent from client (Ajax)
            
            string branchId = Current.CpnyID;
            string dfltBankAcct = data["cboBankAcct"];
            string classID = data["cboClassID"];
            string lastBatNbr = data["txtLastBatNbr"];
            string lastRefNbr = data["txtLastRefNbr"];
            string lastPaymentNbr = data["txtLastPaymentNbr"];
            string preFixBat = data["txtPreFixBat"];
            string tranDescDef = data["cboTranDescDef"];
            string terms = data["cboTermsID"];
            

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstAP00000Header"]);
            

            ChangeRecords<AP_Setup> lstAP00000Header = dataHandler.BatchObjectData<AP_Setup>();

            foreach (AP_Setup updatedSlsperson in lstAP00000Header.Updated)
            {
                // Get the image path
               

                var objHeader = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupID == "AP");
                if (objHeader != null)
                {
                    //updating
                    
                    if(objHeader.tstamp.ToHex() != updatedSlsperson.tstamp.ToHex())
                    {
                        return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    }else
                    {
                        UpdatingHeader(updatedSlsperson, ref objHeader);
                    }
                }
                
                
                    
                    
                

                // If there is a change in handling status (keepStatus is False),
                // add a new pending task with an approved handle.
                
                    _db.SaveChanges();
                
                // ===============================================================

                // Get out of the loop (only update the first data)
                break;
            }
            foreach (AP_Setup updatedSlsperson in lstAP00000Header.Created)
            {
                var objHeader = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupID == "AP");
                if (objHeader == null)
                {
                    objHeader = new AP_Setup();
                    
                    objHeader.BranchID = branchId;
                    objHeader.SetupID = "AP";
                    objHeader.DfltBankAcct = dfltBankAcct; // ???
                    objHeader.ClassID = classID;
                    objHeader.LastBatNbr = lastBatNbr;
                    objHeader.LastRefNbr = lastRefNbr;
                    objHeader.LastPaymentNbr = lastPaymentNbr;
                    objHeader.PreFixBat = preFixBat;
                    objHeader.TranDescDflt = tranDescDef;
                    objHeader.terms = terms;


                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = _screenName;
                    objHeader.Crtd_User = Current.UserName;
                    objHeader.tstamp = new byte[0];
                    UpdatingHeader(updatedSlsperson, ref objHeader);
                    _db.AP_Setup.AddObject(objHeader);
                    _db.SaveChanges();
                    
                }
            }
            //_db.SaveChanges();

            return Json(new { success = true });
        }

        // Delete a Sales Person and his picture file
        [DirectMethod]
        public ActionResult Delete(string branchId)
        {
            branchId = Current.CpnyID;
            var cpny = _db.AP_Setup.FirstOrDefault(p => p.BranchID == branchId);
            if (cpny != null )
            {
                _db.AP_Setup.DeleteObject(cpny);
               
            }

            _db.SaveChanges();
            return this.Direct();
        }

        private void UpdatingHeader(AP_Setup s, ref AP_Setup d)
        {
            
            d.DfltBankAcct = s.DfltBankAcct;
            d.ClassID = s.ClassID;
            d.LastBatNbr = s.LastBatNbr;
            d.LastRefNbr = s.LastRefNbr;
            d.LastPaymentNbr = s.LastPaymentNbr;
            d.PreFixBat = s.PreFixBat;
            d.TranDescDflt = s.TranDescDflt;
            d.terms = s.terms;
                 

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenName;
            d.LUpd_User = Current.UserName;
        }

        

        // Upload and preview the image.
       

       
    }
}

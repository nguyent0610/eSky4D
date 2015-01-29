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
namespace AR00000.Controllers
{
    [DirectController]
    public class AR00000Controller : Controller
    {
        private string _screenName = "AR00000";
        
        AR00000Entities _db = Util.CreateObjectContext<AR00000Entities>(false);
        eBiz4DWebSysEntities _sys = Util.CreateObjectContext<eBiz4DWebSysEntities>(true);

        string test = Current.CpnyID;

        

        //
        // GET: /AR00000/
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
        public ActionResult GetAR00000Header(string branchId, string setupID)
        {
            var setupData = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupId == setupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            // Get params from data that's sent from client (Ajax)
            
            string branchId = Current.CpnyID;
            //string dfltBankAcct = data["cboBankAcct"];
            //string classID = data["cboClassID"];
            //string lastBatNbr = data["txtLastBatNbr"];
            //string lastRefNbr = data["txtLastRefNbr"];
            //string lastPaymentNbr = data["txtLastPaymentNbr"];
            //string preFixBat = data["txtPreFixBat"];
            //string tranDescDef = data["cboTranDescDef"];
            //string terms = data["cboTermsID"];
            

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstAR00000Header"]);
            

            ChangeRecords<AR_Setup> lstAR00000Header = dataHandler.BatchObjectData<AR_Setup>();

            foreach (AR_Setup updatedSlsperson in lstAR00000Header.Updated)
            {
                // Get the image path


                var objHeader = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupId == "AR");
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
            foreach (AR_Setup createdAR00000 in lstAR00000Header.Created)
            {
                var objHeader = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId && p.SetupId == "AR");
                if (objHeader == null)
                {
                    objHeader = new AR_Setup();
                    
                    objHeader.BranchID = branchId;
                    objHeader.SetupId = "AR";
                    objHeader.LastBatNbr = createdAR00000.LastBatNbr; // ???
                    objHeader.AutoCustID = createdAR00000.AutoCustID;
                    objHeader.LastRefNbr = createdAR00000.LastRefNbr;
                    objHeader.AutoSlsperID = createdAR00000.AutoSlsperID;
                    objHeader.LastReceiptNbr = createdAR00000.LastReceiptNbr;
                    objHeader.HiddenHierarchy = createdAR00000.HiddenHierarchy;
                    objHeader.LastCustID = createdAR00000.LastCustID;
                    objHeader.DfltShipViaID = createdAR00000.DfltShipViaID;
                    objHeader.PrefixBat = createdAR00000.PrefixBat;
                    objHeader.TranDescDflt = createdAR00000.TranDescDflt;
                    objHeader.AddressLevel = createdAR00000.AddressLevel;
                    objHeader.DfltBankAcct = createdAR00000.DfltBankAcct;


                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = _screenName;
                    objHeader.Crtd_User = Current.UserName;
                    objHeader.tstamp = new byte[0];
                    UpdatingHeader(createdAR00000, ref objHeader);
                    _db.AR_Setup.AddObject(objHeader);
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
            var cpny = _db.AR_Setup.FirstOrDefault(p => p.BranchID == branchId);
            if (cpny != null )
            {
                _db.AR_Setup.DeleteObject(cpny);
               
            }

            _db.SaveChanges();
            return this.Direct();
        }

        private void UpdatingHeader(AR_Setup s, ref AR_Setup d)
        {

            d.LastBatNbr = s.LastBatNbr;
            d.AutoCustID = s.AutoCustID == null ? false : s.AutoCustID;
            d.LastRefNbr = s.LastRefNbr;
            d.AutoSlsperID = s.AutoSlsperID == null ? false : s.AutoSlsperID;
            d.LastReceiptNbr = s.LastReceiptNbr;
            d.HiddenHierarchy = s.AutoSlsperID == null ? false : s.HiddenHierarchy;


            d.LastCustID = s.LastCustID;
            d.DfltShipViaID = s.DfltShipViaID;
            d.PrefixBat = s.PrefixBat;
            d.TranDescDflt = s.TranDescDflt;
            d.AddressLevel = s.AddressLevel;
            d.DfltBankAcct = s.DfltBankAcct;
                 

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenName;
            d.LUpd_User = Current.UserName;
        }

        

        // Upload and preview the image.
       

       
    }
}

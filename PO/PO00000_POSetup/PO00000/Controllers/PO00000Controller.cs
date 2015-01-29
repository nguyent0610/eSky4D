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

namespace PO00000.Controllers
{
    [DirectController]
    public class PO00000Controller : Controller
    {



        PO00000Entities _Sys = Util.CreateObjectContext<PO00000Entities>(false);
        
        public ActionResult Index()
        {

         
            return View(_Sys.PO_Setup.Where(p=>p.BranchID==Current.CpnyID));
           
        }
        public ActionResult GetPO_Setup()
        {

            return this.Store(_Sys.PO_Setup.Where(p => p.BranchID == Current.CpnyID));
           
        }
         [HttpPost]
        public ActionResult Save(FormCollection data )     
        {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstPO_Setup"]);
            ChangeRecords<PO_Setup> lstLang = dataHandler.BatchObjectData<PO_Setup>();

            foreach (PO_Setup updated in lstLang.Created)
            {
                var record = _Sys.PO_Setup.Where(p => p.BranchID == updated.BranchID).FirstOrDefault();
                if (record == null)
                {
                    record = new PO_Setup();
                    record.Crtd_DateTime = DateTime.Now;
                    record.Crtd_Prog = "PO00000";
                    record.Crtd_User = "ADMIN";
                    record.BranchID = Current.CpnyID;
                    record.SetupID = "PO";
                    UpdatingPOSetup(updated,ref record);
                    _Sys.PO_Setup.AddObject(record);
                }
                UpdatingPOSetup(updated, ref record);

            }

            foreach (PO_Setup deleted in lstLang.Deleted)
            {
                if (_Sys.PO_Setup.Where(p => p.BranchID == deleted.BranchID).Count() > 0)
                {
                    _Sys.PO_Setup.DeleteObject(_Sys.PO_Setup.Where(p => p.BranchID == deleted.BranchID).FirstOrDefault());
                }
            }

            foreach (PO_Setup updated in lstLang.Updated)
            {


                var record = _Sys.PO_Setup.Where(p => p.BranchID == updated.BranchID).FirstOrDefault();
                if (record == null)
                {
                    record = new PO_Setup();
                    record.Crtd_DateTime = DateTime.Now;
                    record.Crtd_Prog = "PO00000";
                    record.Crtd_User = "ADMIN";
                    record.BranchID = Current.CpnyID;
                    record.SetupID = "PO";
                    UpdatingPOSetup(updated, ref record);
                    _Sys.PO_Setup.AddObject(record);
                }
                UpdatingPOSetup(updated, ref record);
            }
            _Sys.SaveChanges();
         
            return Json(new { success = true });
            
        }
        private void UpdatingPOSetup(PO_Setup updated,ref PO_Setup record)
        {
            record.AutoRef = updated.AutoRef;
            record.AutoReleaseAP = updated.AutoReleaseAP;
            record.BillAddr1 = updated.BillAddr1;
            record.BillAddr2 = updated.BillAddr2;
            record.BillAttn = updated.BillAttn;

            record.BillCity = updated.BillCity;
            record.BillCountry = updated.BillCountry;
            record.BillEmail = updated.BillEmail;
            record.BillFax = updated.BillFax;
            record.BillName = updated.BillName;

            record.BillPhone = updated.BillPhone;
            record.BillState = updated.BillState;
            record.BillZip = updated.BillZip;
            record.DfltLstUnitCost = updated.DfltLstUnitCost;
            record.DfltRcptFrom = updated.DfltRcptFrom;

            record.DfltRcptUnitFromIN = updated.DfltRcptUnitFromIN;
            record.EditablePOPrice = updated.EditablePOPrice;
            record.LastBatNbr = updated.LastBatNbr;
            record.LastPONbr = updated.LastPONbr;
            record.LastRcptNbr = updated.LastRcptNbr;

            record.PreFixBat = updated.PreFixBat;
            record.ShipAddr1 = updated.ShipAddr1;
            record.ShipAddr2 = updated.ShipAddr2;
            record.ShipAttn = updated.ShipAttn;
            record.ShipCity = updated.ShipCity;

            record.ShipCountry = updated.ShipCountry;
            record.ShipEmail = updated.ShipEmail;
            record.ShipFax = updated.ShipFax;
            record.ShipName = updated.ShipName;
            record.ShipPhone = updated.ShipPhone;

            record.ShipState = updated.ShipState;
            record.ShipZip = updated.ShipZip;
            record.UseAP = updated.UseAP;
            record.UseBarCode = updated.UseBarCode;
            record.UseIN = updated.UseIN;


            record.LUpd_DateTime = DateTime.Now;
            record.LUpd_Prog = "PO00000";
            record.LUpd_User = "ADMIN";
        }
        public ActionResult Refresh()
        {
            X.GetCmp<Store>("storePO_Setup").Reload();  
            return this.Direct();
            
        }
        [DirectMethod]
        public ActionResult AskClose()
        {
            Message.Show(5, null, new JFunction() { Fn = "askClose" });
            return this.Direct();
        }

       
      
    }
}
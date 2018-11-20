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
namespace AR10400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10400Controller : Controller
    {
        string screenNbr = "AR10400";
        AR10400Entities _db = Util.CreateObjectContext<AR10400Entities>(false);
        

        public ActionResult Index()
        {
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetDataFormTop(String branchID, String batNbr)
        {
            var rptCtrl = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.Module == "AR" && p.BatNbr == batNbr);
            return this.Store(rptCtrl);
        }
        public ActionResult GetDataFormBot(String branchID, String batNbr, String refNbr)
        {
            var lst = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
            return this.Store(lst);
        }

        public ActionResult GetDataGrid1(String batNbr,String branchID, String custID,String docType)
        {
            var lst = _db.AR10400_pgLoadGridAdjg(batNbr, branchID, custID, docType);

            return this.Store(lst);
        }

        public ActionResult GetDataGrid2(String batNbr, String branchID, String custID)
        {
            var lst = _db.AR10400_pgLoadGridAdjd(batNbr, branchID, custID);

            return this.Store(lst);
        }

        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string branchID, string handle, string batNbr,string docType, string custID)
        {
            StoreDataHandler dataHandlerTop = new StoreDataHandler(data["lstheaderTop"]);
            ChangeRecords<Batch> lstheaderTop = dataHandlerTop.BatchObjectData<Batch>();

            StoreDataHandler dataHandlerGrid1 = new StoreDataHandler(data["lstgrd1"]);
            ChangeRecords<AR10400_pgLoadGridAdjg_Result> lstgrd1 = dataHandlerGrid1.BatchObjectData<AR10400_pgLoadGridAdjg_Result>();
            StoreDataHandler dataHandlerGrid2 = new StoreDataHandler(data["lstgrd2"]);
            ChangeRecords<AR10400_pgLoadGridAdjd_Result> lstgrd2 = dataHandlerGrid2.BatchObjectData<AR10400_pgLoadGridAdjd_Result>();


            var docDate = data["txtDocDate"];
            var toAmt = data["txtCuryCrTot"];
            //var tmpGridChangeOrNot = 0;
            ////var invcDate = data["txtInvcDate"];


            //var batNbrIfNull = 0;
            var tmpBatNbr = "";



            var tmpcatchHandle = "";
            double tmpAdjAmt = 0;
            double tmpAdgAmt = 0;
            bool tmpUsed = false;
            var tmpAdjdRefNbr = "";
            var tmpAdjgRefNbr = "";
            var tmpStatus = "";
                
            if (lstgrd1.Updated.Count != 0 && lstgrd2.Updated.Count != 0)
            {
                var recordDelete = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                for (int i = 0; i < recordDelete.Count; i++)
                {
                    _db.AR_Adjust.DeleteObject(recordDelete[i]);
                    _db.SaveChanges();
                }
            }

            foreach (Batch createdForm in lstheaderTop.Created)
            {
                
                var objHeaderBatNbr = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.BatNbr == batNbr).FirstOrDefault();
                if (objHeaderBatNbr == null)
                {
                    objHeaderBatNbr = new Batch();
                    objHeaderBatNbr.BranchID = branchID;
                    objHeaderBatNbr.Module = "AR";
                    objHeaderBatNbr.BatNbr = functionBatNbrIfNull(branchID); //ham tu dong tao ra BatNbr
                    objHeaderBatNbr.Descr = createdForm.Descr;


                    tmpBatNbr = objHeaderBatNbr.BatNbr;

                    objHeaderBatNbr.TotAmt = Convert.ToDouble(toAmt);
                    objHeaderBatNbr.DateEnt = Convert.ToDateTime(docDate);
                    objHeaderBatNbr.EditScrnNbr = "AR10400";
                    objHeaderBatNbr.JrnlType = "AR";
                    objHeaderBatNbr.OrigBranchID = "";
                    if (handle == "R")
                    {
                        objHeaderBatNbr.Rlsed = 1;
                        objHeaderBatNbr.Status = "C"; // sua lai sau
                        tmpStatus = "C";
                    }
                    else if (handle == "N")
                    {
                        objHeaderBatNbr.Rlsed = 0;
                        objHeaderBatNbr.Status = "H";
                        tmpStatus = "H";
                    }
                    objHeaderBatNbr.NoteID = 0;
                    objHeaderBatNbr.IntRefNbr = "";
                    objHeaderBatNbr.ReasonCD = "";

                    objHeaderBatNbr.LUpd_DateTime = DateTime.Now;
                    objHeaderBatNbr.LUpd_Prog = screenNbr;
                    objHeaderBatNbr.LUpd_User = Current.UserName;
                    objHeaderBatNbr.Crtd_DateTime = DateTime.Now;
                    objHeaderBatNbr.Crtd_Prog = screenNbr;
                    objHeaderBatNbr.Crtd_User = Current.UserName;
                    objHeaderBatNbr.tstamp = new byte[0];

                    _db.Batches.AddObject(objHeaderBatNbr);
                    //_db.SaveChanges();
                }
            }

            foreach (Batch updatedForm in lstheaderTop.Updated)
            {
                var objHeaderBatNbr = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.BatNbr == batNbr).FirstOrDefault();
                if (objHeaderBatNbr != null)
                {
                    objHeaderBatNbr.Descr = updatedForm.Descr;
                    objHeaderBatNbr.TotAmt = Convert.ToDouble(toAmt);

                }

            }



            foreach (AR10400_pgLoadGridAdjg_Result updatedGrid1 in lstgrd1.Updated)
            {
                //if (branchID != "")
                //{
                    tmpAdjAmt = Convert.ToDouble(updatedGrid1.Payment);
                    foreach (AR10400_pgLoadGridAdjd_Result updatedGrid2 in lstgrd2.Updated)
                    {

                        if (tmpAdjAmt > 0)
                        {
                            if (tmpAdgAmt == 0)
                            {
                                if (tmpUsed == false)
                                {
                                    tmpAdgAmt = Convert.ToDouble(updatedGrid2.Payment);
                                    if (tmpAdjAmt >= tmpAdgAmt)
                                    {
                                        var record = new AR_Adjust();
                                        record.BranchID = branchID;
                                        if(batNbr == ""){
                                           record.BatNbr = tmpBatNbr;
                                        }else{
                                            record.BatNbr = batNbr;
                                        }
                                        record.AdjdRefNbr = updatedGrid2.RefNbr;
                                        record.AdjdBatNbr = updatedGrid2.BatNbr;


                                        record.AdjAmt = tmpAdgAmt;
                                        tmpAdjAmt = tmpAdjAmt - tmpAdgAmt;
                                        tmpAdgAmt = 0;
                                        tmpUsed = false;

                                        record.AdjdDocType = updatedGrid2.DocType;
                                        record.AdjDiscAmt = 0;
                                        record.AdjgDocDate = Convert.ToDateTime(docDate);
                                        record.AdjgDocType = docType;
                                        record.AdjgBatNbr = updatedGrid1.BatNbr;
                                        record.AdjgRefNbr = updatedGrid1.RefNbr;
                                        record.CustID = custID;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = screenNbr;
                                        record.LUpd_User = Current.UserName;
                                        record.Crtd_DateTime = DateTime.Now;
                                        record.Crtd_Prog = screenNbr;
                                        record.Crtd_User = Current.UserName;
                                        _db.AR_Adjust.AddObject(record);
                                        _db.SaveChanges();
                                    }
                                    else if (tmpAdjAmt < tmpAdgAmt)
                                    {
                                        var record = new AR_Adjust();
                                        record.BranchID = branchID;
                                        if(batNbr == ""){
                                           record.BatNbr = tmpBatNbr;
                                        }else{
                                            record.BatNbr = batNbr;
                                        }
                                        record.AdjdRefNbr = updatedGrid2.RefNbr;
                                        record.AdjdBatNbr = updatedGrid2.BatNbr;


                                        record.AdjAmt = tmpAdjAmt;
                                        tmpAdjAmt = 0;
                                        tmpAdgAmt = tmpAdgAmt - tmpAdjAmt;
                                        tmpUsed = true;
                                        tmpAdjdRefNbr = record.AdjdRefNbr;
                                        tmpAdjgRefNbr = updatedGrid1.RefNbr;


                                        record.AdjdDocType = updatedGrid2.DocType;
                                        record.AdjDiscAmt = 0;
                                        record.AdjgDocDate = Convert.ToDateTime(docDate);
                                        record.AdjgDocType = docType;
                                        record.AdjgBatNbr = updatedGrid1.BatNbr;
                                        record.AdjgRefNbr = updatedGrid1.RefNbr;
                                        record.CustID = custID;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = screenNbr;
                                        record.LUpd_User = Current.UserName;
                                        record.Crtd_DateTime = DateTime.Now;
                                        record.Crtd_Prog = screenNbr;
                                        record.Crtd_User = Current.UserName;
                                        _db.AR_Adjust.AddObject(record);
                                        _db.SaveChanges();
                                    }
                                }

                                // ngoac dong vong if tmpAdgAmt = 0
                            }
                            else if (tmpAdgAmt > 0)
                            {
                                if (tmpUsed == true)
                                {
                                    var recordContain = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.AdjdRefNbr == tmpAdjdRefNbr && p.AdjgRefNbr == tmpAdjgRefNbr).FirstOrDefault();
                                    if (recordContain != null)
                                    {
                                        if (tmpAdjAmt >= tmpAdgAmt)
                                        {
                                            var record = new AR_Adjust();
                                            record.BranchID = branchID;
                                            if(batNbr == ""){
                                               record.BatNbr = tmpBatNbr;
                                            }else{
                                                record.BatNbr = batNbr;
                                            }
                                            record.AdjdRefNbr = updatedGrid2.RefNbr;
                                            record.AdjdBatNbr = updatedGrid2.BatNbr;


                                            record.AdjAmt = tmpAdgAmt - recordContain.AdjAmt;
                                            tmpAdjAmt = tmpAdjAmt - record.AdjAmt;
                                            tmpAdgAmt = 0;
                                            tmpUsed = false;


                                            record.AdjdDocType = updatedGrid2.DocType;
                                            record.AdjDiscAmt = 0;
                                            record.AdjgDocDate = Convert.ToDateTime(docDate);
                                            record.AdjgDocType = docType;
                                            record.AdjgBatNbr = updatedGrid1.BatNbr;
                                            record.AdjgRefNbr = updatedGrid1.RefNbr;
                                            record.CustID = custID;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.AR_Adjust.AddObject(record);
                                            _db.SaveChanges();
                                        }
                                        else if (tmpAdjAmt < tmpAdgAmt)
                                        {
                                            var record = new AR_Adjust();
                                            record.BranchID = branchID;
                                            if(batNbr == ""){
                                               record.BatNbr = tmpBatNbr;
                                            }else{
                                                record.BatNbr = batNbr;
                                            }
                                            record.AdjdRefNbr = updatedGrid2.RefNbr;
                                            record.AdjdBatNbr = updatedGrid2.BatNbr;


                                            record.AdjAmt = tmpAdjAmt;
                                            tmpAdjAmt = 0;
                                            tmpAdgAmt = tmpAdgAmt - tmpAdjAmt;
                                            tmpUsed = true;
                                            tmpAdjdRefNbr = record.AdjdRefNbr;
                                            tmpAdjgRefNbr = updatedGrid1.RefNbr;


                                            record.AdjdDocType = updatedGrid2.DocType;
                                            record.AdjDiscAmt = 0;
                                            record.AdjgDocDate = Convert.ToDateTime(docDate);
                                            record.AdjgDocType = docType;
                                            record.AdjgBatNbr = updatedGrid1.BatNbr;
                                            record.AdjgRefNbr = updatedGrid1.RefNbr;
                                            record.CustID = custID;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.AR_Adjust.AddObject(record);
                                            _db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            //dong ngoac vong if tmpAdjAmt > 0
                        }
                    }
               ////ngoac dong dk branchID != null
               // }
               // else  // branchID == null tuc tao moi
               // {


               // }

            }









            if (handle == "R")
            {
                var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10400" && p.BatNbr == batNbr).FirstOrDefault();
                recordBatNbrUpdate.Rlsed = 1;
                recordBatNbrUpdate.Status = "C";
                tmpcatchHandle = "1";
                tmpStatus = "C";
            }

            _db.SaveChanges();

            return Json(new { success = true, value1 = tmpBatNbr, value2 = tmpStatus }, JsonRequestBehavior.AllowGet);


        }

        [DirectMethod]
        public ActionResult DeleteFormTopBatch(string batNbr, string branchID)
        {
            var recordTopBatch = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.Module == "AR");
            if (recordTopBatch != null)
            {

                _db.Batches.DeleteObject(recordTopBatch);
                var recordDelete = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                for (int i = 0; i < recordDelete.Count; i++)
                {
                    _db.AR_Adjust.DeleteObject(recordDelete[i]);
                    _db.SaveChanges();
                }
            }

            _db.SaveChanges();
            return this.Direct();
        }

        //[DirectMethod]
        //public ActionResult DeleteFormBotAP_Doc(string refNbr, string batNbr, string branchID)
        //{
        //    var recordBotAP_Doc = _db.AP_Doc.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr);

        //    if (recordBotAP_Doc != null)
        //    {
        //        _db.AP_Doc.DeleteObject(recordBotAP_Doc);
        //        var recordGridTrans = _db.AP_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
        //        if (recordGridTrans != null)
        //        {
        //            for (int i = 0; i < recordGridTrans.Count; i++)
        //            {
        //                _db.AP_Trans.DeleteObject(recordGridTrans[i]);
        //            }
        //        }
        //    }
        //    _db.SaveChanges();
        //    return this.Direct();
        //}





        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AR10400_ppARNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }


    }
}











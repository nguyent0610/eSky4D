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
namespace AR10200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10200Controller : Controller
    {
        string screenNbr = "AR10200";
        AR10200Entities _db = Util.CreateObjectContext<AR10200Entities>(false);
        

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
        public ActionResult GetDataFormBotRight(String branchID, String batNbr, String refNbr)
        {
            var lst = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
            return this.Store(lst);
        }

        public ActionResult GetDataGrid(String batNbr, String branchID, String custID, String slsperId, String deliveryId, String refNbr, DateTime fromDate, DateTime toDate, String dateType, String isGridF3)
        {
            var lst = _db.AR10200_pgBindingGrid(batNbr, branchID, custID, slsperId, deliveryId, refNbr, fromDate, toDate, dateType, isGridF3);

            return this.Store(lst);
        }

        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string branchID, string handle, string batNbr, string refNbr,string reasonCD)
        {
            StoreDataHandler dataHandlerTop = new StoreDataHandler(data["lstheaderTop"]);
            ChangeRecords<Batch> lstheaderTop = dataHandlerTop.BatchObjectData<Batch>();
            StoreDataHandler dataHandlerBot = new StoreDataHandler(data["lstheaderBotRight"]);
            ChangeRecords<AR_Doc> lstheaderBotRight = dataHandlerBot.BatchObjectData<AR_Doc>();
            StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR10200_pgBindingGrid_Result> lstgrd = dataHandlerGrid.BatchObjectData<AR10200_pgBindingGrid_Result>();

            var docDate = data["dteDocDate"];
           
            var toAmt = data["txtCuryCrTot"];
            var custID = data["cboCustID"];
            var slsperID = data["cboSlsperID"];
            var delieveryID = data["cboDeliveryID"];
            var descr = data["txtDescr"];
            var tmpGridChangeOrNot = 0;
            //var invcDate = data["txtInvcDate"];


            var batNbrIfNull = 0;
            var tmpBatNbr = "";
            var tmpRefNbr = "";
            var tmpcatchHandle = "";



            //ap_doc phuc tap neu nhu de trong BatNbr thi ngoai viec them moi ap_doc thi phai them moi ca Batch
            foreach (AR_Doc created in lstheaderBotRight.Created)
            {
                var objHeader = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).FirstOrDefault();
                //neu objHeader nghia la hien tai BatNbr van chua co cai nao ca , neu vay ta se tao. ra 1 cai Nbr moi
                if (objHeader == null)
                {
                    objHeader = new AR_Doc();
                    //bat dau xet cac dieu kien batNbr va refNbr
                    objHeader.BranchID = branchID;
                    if (batNbr != "")
                    {
                        objHeader.BatNbr = batNbr;
                    }
                    else
                    {
                        batNbrIfNull = 1;
                        objHeader.BatNbr = functionBatNbrIfNull(branchID);
                    }
                    if (refNbr != "")
                    {
                        objHeader.RefNbr = refNbr;

                    }
                    else
                    {
                        //neu batNbr khac Null thi ta dang khoi tao cai RefNbr thu 2 cho cai batNbr nay
                        if (batNbrIfNull == 0)
                        {

                            objHeader.RefNbr = functionRefNbrIfNull(branchID);
                        }
                        //truong hop nguoc lai neu batNbr cung null thi ta tao ca cai batNbr moi va RefNbr moi
                        else
                        {
                            tmpBatNbr = objHeader.BatNbr;
                            objHeader.RefNbr = functionRefNbrIfNull(branchID);
                        }
                    }

                    if (handle == "R")
                    {
                        objHeader.Rlsed = 1;
                    }
                    else if (handle == "N")
                    {
                        objHeader.Rlsed = 0;
                    }

                    tmpRefNbr = objHeader.RefNbr;
                    objHeader.CustId = custID;
                    objHeader.SlsperId = slsperID;
                    objHeader.DeliveryID = delieveryID;
                    UpdatingFormBotAR_Doc(created, ref objHeader);

                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    objHeader.tstamp = new byte[0];



                    _db.AR_Doc.AddObject(objHeader);
                    //_db.SaveChanges();

                    //Add object Batch moi neu nhu tmpBatNbr khac "" co nghia la truong hop nay chi tao moi doc o FormBot chu ko chon 1 cai BatNbr san co
                    if (tmpBatNbr != "")
                    {
                        var objHeaderBatNbr = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.BatNbr == batNbr).FirstOrDefault();
                        if (objHeaderBatNbr == null)
                        {
                            objHeaderBatNbr = new Batch();
                            objHeaderBatNbr.BranchID = branchID;
                            objHeaderBatNbr.Module = "AR";
                            objHeaderBatNbr.BatNbr = tmpBatNbr;
                            if (toAmt != "")
                            {
                                objHeaderBatNbr.TotAmt = Convert.ToDouble(toAmt);
                            }
                            else
                            {
                                objHeaderBatNbr.TotAmt = 0;
                            }
                            objHeaderBatNbr.DateEnt = Convert.ToDateTime(docDate);
                            objHeaderBatNbr.EditScrnNbr = "AR10200";
                            objHeaderBatNbr.JrnlType = "AR";
                            objHeaderBatNbr.OrigBranchID = "";
                            if (handle == "R")
                            {
                                objHeaderBatNbr.Rlsed = 1;
                                objHeaderBatNbr.Status = "C"; // sua lai sau
                            }
                            else if (handle == "N")
                            {
                                objHeaderBatNbr.Rlsed = 0;
                                objHeaderBatNbr.Status = "H";
                            }

                            objHeaderBatNbr.ReasonCD = reasonCD;
                            objHeaderBatNbr.Descr = descr;
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

                }
            }


            foreach (AR_Doc updated in lstheaderBotRight.Updated)
            {
                // Get the image path


                var objHeader = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).FirstOrDefault();
                if (objHeader != null)
                {
                    //updating
                    if (handle == "R")
                    {
                        objHeader.Rlsed = 1;
                    }
                    else if (handle == "N")
                    {
                        objHeader.Rlsed = 0;
                    }

                    objHeader.CustId = custID;
                    objHeader.SlsperId = slsperID;
                    objHeader.DeliveryID = delieveryID;

                    UpdatingFormBotAR_Doc(updated, ref objHeader);

                }
                else
                {

                }


            }

            foreach (AR10200_pgBindingGrid_Result created in lstgrd.Created)
            {
                var record = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.AdjdRefNbr == created.RefNbr && p.AdjgRefNbr == refNbr).FirstOrDefault();
                if (record == null)
                {
                    record = new AR_Adjust();
                    record.BranchID = branchID;
                    if (batNbr == "")
                    {
                        record.AdjgBatNbr = tmpBatNbr;
                        record.BatNbr = tmpBatNbr;
                    }
                    else
                    {
                        record.BatNbr = batNbr;
                        record.AdjgBatNbr = batNbr;
                    }
                    if (refNbr == "")
                    {
                        record.AdjgRefNbr = tmpRefNbr;
                    }
                    else
                    {
                        record.AdjgRefNbr = refNbr;
                    }

                    UpdatingGridAd_Adjust(created, ref record);
                    record.Crtd_DateTime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;
                    _db.AR_Adjust.AddObject(record);
                }
            }

            foreach (AR10200_pgBindingGrid_Result updated in lstgrd.Updated)
            {
                var record = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.AdjdRefNbr == updated.RefNbr && p.AdjgRefNbr == refNbr).FirstOrDefault();
                if (record != null)
                {
                    if (batNbr == "")
                    {
                        record.AdjgBatNbr = tmpBatNbr;
                        record.BatNbr = tmpBatNbr;
                    }
                    if (refNbr == "")
                    {
                        record.AdjgRefNbr = tmpRefNbr;
                    }
                   UpdatingGridAd_Adjust(updated, ref record);
                }
                else
                {
                    record = new AR_Adjust();
                    record.BranchID = branchID;
                    
                   
                    if (batNbr == "")
                    {
                        record.AdjgBatNbr = tmpBatNbr;
                        record.BatNbr = tmpBatNbr;
                    }
                    else
                    {
                        record.BatNbr = batNbr;
                        record.AdjgBatNbr = batNbr;
                    }
                    if (refNbr == "")
                    {
                        record.AdjgRefNbr = tmpRefNbr;
                    }
                    else
                    {
                        record.AdjgRefNbr = refNbr;
                    }

                    UpdatingGridAd_Adjust(updated, ref record);
                    record.Crtd_DateTime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;
                    _db.AR_Adjust.AddObject(record);
                }
            }

            foreach (AR10200_pgBindingGrid_Result deleted in lstgrd.Deleted)
            {
                var record = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.AdjdRefNbr == deleted.RefNbr && p.AdjgRefNbr == refNbr).FirstOrDefault();
                if (record != null)
                {
                    _db.AR_Adjust.DeleteObject(record);
                }

            }

            _db.SaveChanges();

            var justUpdateBatNbr = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10200" && p.BatNbr == batNbr).FirstOrDefault();
            if (justUpdateBatNbr != null)
            {
                justUpdateBatNbr.Descr = descr;
                justUpdateBatNbr.TotAmt = Convert.ToDouble(toAmt);
                justUpdateBatNbr.ReasonCD = reasonCD;
                _db.SaveChanges();
            }

            if (handle == "R")
            {
                var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10200" && p.BatNbr == batNbr).FirstOrDefault();
                var recordRefNbrUpdate = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).FirstOrDefault();
                recordBatNbrUpdate.Rlsed = 1;
                recordBatNbrUpdate.Status = "C";
                recordRefNbrUpdate.Rlsed = 1;
                tmpcatchHandle = "1";
            }


            _db.SaveChanges();



            return Json(new { success = true, value = batNbrIfNull, value1 = tmpRefNbr, value2 = tmpBatNbr, value3 = tmpcatchHandle, value4 = tmpGridChangeOrNot }, JsonRequestBehavior.AllowGet);


        }

        [DirectMethod]
        public ActionResult DeleteFormTopBatch(string batNbr, string branchID)
        {
            var recordTopBatch = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.Module == "AR");
            if (recordTopBatch != null)
            {
                var recordBotAP_Doc = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                if (recordBotAP_Doc != null)
                {
                    for (int k = 0; k < recordBotAP_Doc.Count; k++)
                    {
                        _db.AR_Doc.DeleteObject(recordBotAP_Doc[k]);
                   
                    }
                    var recordGridAP_Adjust = _db.AR_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                    if (recordGridAP_Adjust != null)
                    {
                        for (int l = 0; l < recordGridAP_Adjust.Count; l++)
                        {
                            _db.AR_Adjust.DeleteObject(recordGridAP_Adjust[l]);

                        }
                    }


                }




                _db.Batches.DeleteObject(recordTopBatch);
            }

            _db.SaveChanges();
            return this.Direct();
        }



        private void UpdatingGridAR_Trans(AR_Trans s, ref AR_Trans d)
        {
            d.LineRef = s.LineRef;
            d.TranAmt = s.TranAmt;
            d.TranDesc = s.TranDesc;
            //d.InvcDate = DateTime.Now;
            d.TranDate = DateTime.Now;
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void UpdatingFormBotAR_Doc(AR_Doc s, ref AR_Doc d)
        {
      
            d.DocBal = s.DocBal;
            d.DocDate = s.DocDate;
            d.DocDesc = s.DocDesc;

            d.SlsperId = s.SlsperId;
            d.InvcNbr = s.InvcNbr;
            d.DiscDate = s.DocDate;
            d.DueDate = s.DocDate;
            
            d.InvcNote = "";
            d.Terms = "";
            d.TaxId00 = "";
            d.TaxId01 = "";
            d.TaxId02 = "";
            d.TaxId03 = "";
            //d.RcptNbr = "";
            d.DocType = "HC";
            //d.VendID = "";
            d.OrigDocAmt = s.OrigDocAmt;
            //d.PONbr = s.PONbr;


            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void UpdatingGridAd_Adjust(AR10200_pgBindingGrid_Result s, ref AR_Adjust d)
        {
            d.AdjdBatNbr = s.BatNbr;
            d.AdjdRefNbr = s.RefNbr;
            d.AdjAmt = Convert.ToDouble(s.Payment);
            d.AdjgDocDate = s.DocDate;
            d.AdjgDocType = "HC";
            d.CustID = s.CustId;
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }


        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AR10200_ppARNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AR10200_ppARNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }
       

    }
}











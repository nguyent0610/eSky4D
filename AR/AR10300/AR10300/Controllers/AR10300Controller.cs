using eBiz4DWebFrame;
using Ext.Net;
using Ext.Net.MVC;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace AR10300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10300Controller : Controller
    {
        string screenNbr = "AR10300";
        AR10300Entities _db = Util.CreateObjectContext<AR10300Entities>(false);
        

        public ActionResult Index()
        {
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
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
            var lst = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
            return this.Store(lst);
        }

        public ActionResult GetDataGrid(String branchID, String batNbr, String refNbr)
        {
            var lst = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();

            return this.Store(lst);
        }

        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string branchID, string handle, string batNbr, string refNbr, string docType, string intRefNbr, string reasonCD)
        {
            StoreDataHandler dataHandlerTop = new StoreDataHandler(data["lstheaderTop"]);
            ChangeRecords<Batch> lstheaderTop = dataHandlerTop.BatchObjectData<Batch>();
            StoreDataHandler dataHandlerBot = new StoreDataHandler(data["lstheaderBot"]);
            ChangeRecords<AR_Doc> lstheaderBot = dataHandlerBot.BatchObjectData<AR_Doc>();
            StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_Trans> lstgrd = dataHandlerGrid.BatchObjectData<AR_Trans>();

            var docDate = data["txtDocDate"];
           
            var toAmt = data["txtCuryCrTot"];
            var tmpGridChangeOrNot = 0;
            //var invcDate = data["txtInvcDate"];


            var batNbrIfNull = 0;
            var tmpBatNbr = "";
            var tmpRefNbr = "";
            var tmpcatchHandle = "";



            //AR_Doc phuc tap neu nhu de trong BatNbr thi ngoai viec them moi AR_Doc thi phai them moi ca Batch
            foreach (AR_Doc created in lstheaderBot.Created)
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
                    objHeader.DocType = docType;
                    //objHeader.VendID = vendID;
                   
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
                            objHeaderBatNbr.EditScrnNbr = "AR10300";
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

                            objHeaderBatNbr.IntRefNbr = intRefNbr;
                            objHeaderBatNbr.ReasonCD = reasonCD;

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


            foreach (AR_Doc updated in lstheaderBot.Updated)
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

                    UpdatingFormBotAR_Doc(updated, ref objHeader);

                }
                else
                {

                }


            }










            foreach (AR_Trans created in lstgrd.Created)
            {
                tmpGridChangeOrNot = 1;
                var record = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr &&
                                             p.LineRef == created.LineRef).FirstOrDefault();
                //if (created.tstamp.ToHex() == "")
                //{
                if (record == null && created.LineRef != "")
                {
                    record = new AR_Trans();
                    record.BranchID = branchID;
                    //bat dau xet cac dieu kien batNbr va refNbr
                    if (batNbr != "")
                    {
                        record.BatNbr = batNbr;
                    }
                    else
                    {
                    
                        record.BatNbr = tmpBatNbr;
                    }
                    if (refNbr != "")
                    {
                        record.RefNbr = refNbr;

                    }
                    else
                    {
                        //neu batNbr khac Null thi ta dang khoi tao cai RefNbr thu 2 cho cai batNbr nay
                        if (batNbrIfNull == 0)
                        {
            
                            record.RefNbr = tmpRefNbr;
                        }
                        //truong hop nguoc lai neu batNbr cung null thi ta tao ca cai batNbr moi va RefNbr moi
                        else
                        {

                            record.BatNbr = tmpBatNbr;
                            record.RefNbr = tmpRefNbr;
                        }
                    }

                    //var recordVendor = _db.AP_Vendor.Where(p => p.VendID == vendID).FirstOrDefault();
                    //record.Addr = recordVendor.Addr1;
                    
                    record.JrnlType = "AR";
                  
                    UpdatingGridAR_Trans(created, ref record);

                    //record.VendID = vendID;
                    //record.VendName = recordVendor.Name;
                    record.Crtd_DateTime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;

                    _db.AR_Trans.AddObject(record);
                    _db.SaveChanges();

                }
                else
                {
                    //return Json(new
                    //{
                    //    success = false,
                    //    code = "151",
                    //    //colName = Util.GetLang("ReportViewID"),
                    //    //value = created.ReportViewID
                    //}, JsonRequestBehavior.AllowGet);
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }
                // }
            }



            foreach (AR_Trans updated in lstgrd.Updated)
            {
                tmpGridChangeOrNot = 1;
                var record = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr &&
                                             p.LineRef == updated.LineRef).FirstOrDefault();

                if (record != null)
                {

                    //if (record.tstamp.ToHex() != updated.tstamp.ToHex())
                    //{
                    //    return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    //}

                    UpdatingGridAR_Trans(updated, ref record);
                    var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10300" && p.BatNbr == record.BatNbr).FirstOrDefault();
                    var recordRefNbrUpdate = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == record.BatNbr && p.RefNbr == record.RefNbr).FirstOrDefault();
                    if (recordBatNbrUpdate != null)
                    {
                        recordBatNbrUpdate.TotAmt = Convert.ToDouble(toAmt);
                    }
                    if (recordRefNbrUpdate != null)
                    {
                        recordRefNbrUpdate.DocBal = Convert.ToDouble(toAmt);
                        recordRefNbrUpdate.OrigDocAmt = Convert.ToDouble(toAmt);
                    }
                }
                else
                {
                    if (updated.tstamp.ToHex() == "")
                    {
                        if (record == null)
                        {
                            record = new AR_Trans();
                            record.BranchID = branchID;

                            if (batNbr != "")
                            {
                                record.BatNbr = batNbr;
                            }
                            else
                            {
                                record.BatNbr = tmpBatNbr;
                            }
                            if (refNbr != "")
                            {
                                record.RefNbr = refNbr;

                            }
                            else
                            {
                                //neu batNbr khac Null thi ta dang khoi tao cai RefNbr thu 2 cho cai batNbr nay
                                if (batNbrIfNull == 0)
                                {
                                       record.RefNbr = tmpRefNbr;
                                }
                                //truong hop nguoc lai neu batNbr cung null thi ta tao ca cai batNbr moi va RefNbr moi
                                else
                                {
   
                                    record.BatNbr = tmpBatNbr;
                                    record.RefNbr = tmpRefNbr;
                                }
                            }

                            //var recordVendor = _db.AP_Vendor.Where(p => p.VendID == vendID).FirstOrDefault();
                            //record.Addr = recordVendor.Addr1;
                            
                            record.JrnlType = "AR";
                         
                            UpdatingGridAR_Trans(updated, ref record);

                            //record.VendID = vendID;
                            //record.VendName = recordVendor.Name;
                            record.Crtd_DateTime = DateTime.Now;
                            record.Crtd_Prog = screenNbr;
                            record.Crtd_User = Current.UserName;

                            _db.AR_Trans.AddObject(record);
                            //_db.SaveChanges();

                        }

                    }
                    else
                    {

                        return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }


            foreach (AR_Trans deleted in lstgrd.Deleted)
            {

                var del = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr &&
                                             p.LineRef == deleted.LineRef).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_Trans.DeleteObject(del);

                }
                tmpGridChangeOrNot = 1;
            }

            _db.SaveChanges();
            var justUpdateBatNbr = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10300" && p.BatNbr == batNbr).FirstOrDefault();
            if (justUpdateBatNbr != null)
            {
                justUpdateBatNbr.IntRefNbr = intRefNbr;
                justUpdateBatNbr.ReasonCD = reasonCD;
                justUpdateBatNbr.TotAmt = Convert.ToDouble(toAmt);
                _db.SaveChanges();
            }

            if (handle == "R")
            {
                var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10300" && p.BatNbr == batNbr).FirstOrDefault();
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
                        var recordGridTrans = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                        if (recordGridTrans != null)
                        {
                            for (int i = 0; i < recordGridTrans.Count; i++)
                            {
                                _db.AR_Trans.DeleteObject(recordGridTrans[i]);
                            }
                        }
                    }
                }
                _db.Batches.DeleteObject(recordTopBatch);
            }

            _db.SaveChanges();
            return this.Direct();
        }

        //[DirectMethod]
        //public ActionResult DeleteFormBotAP_Doc(string refNbr, string batNbr, string branchID)
        //{
        //    var recordBotAP_Doc = _db.AR_Doc.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr);

        //    if (recordBotAP_Doc != null)
        //    {
        //        _db.AR_Doc.DeleteObject(recordBotAP_Doc);
        //        var recordGridTrans = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
        //        if (recordGridTrans != null)
        //        {
        //            for (int i = 0; i < recordGridTrans.Count; i++)
        //            {
        //                _db.AR_Trans.DeleteObject(recordGridTrans[i]);
        //            }
        //        }
        //    }
        //    _db.SaveChanges();
        //    return this.Direct();
        //}



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
            d.CustId = s.CustId;


            d.InvcNbr = s.InvcNbr;
            d.DiscDate = s.DocDate;
            d.DueDate = s.DocDate;
            //d.InvcDate = s.DocDate;
            d.InvcNote = "";
            d.Terms = "";
            d.TaxId00 = "";
            d.TaxId01 = "";
            d.TaxId02 = "";
            d.TaxId03 = "";
            //d.RcptNbr = "";
            d.OrigDocAmt = s.OrigDocAmt;
            //d.PONbr = s.PONbr;


            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }




        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.ARNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.ARNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }
       

    }
}











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
namespace AR10800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10800Controller : Controller
    {
        string screenNbr = "AR10800";
        AR10800Entities _db = Util.CreateObjectContext<AR10800Entities>(false);
        

        public ActionResult Index()
        {
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult GetDataFormTop(String branchID, String invcNbr)
        {
            var rptCtrl = _db.AR_RedInvoiceDoc.Where(p => p.BranchID == branchID && p.InvcNbr == invcNbr).FirstOrDefault();
            return this.Store(rptCtrl);
        }
        //public ActionResult GetDataFormBot(String branchID, String batNbr, String refNbr)
        //{
        //    var lst = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
        //    return this.Store(lst);
        //}

        //public ActionResult GetDataGrid(String branchID, String invcNbr, String invcNote)
        //{
        //    var lst = _db.AR_TransLoadRedInvoice(branchID, invcNbr, invcNote).ToList();

        //    return this.Store(lst);
        //}

        public ActionResult GetDataGrid(String branchID, String taxID, String invcNbr, String tmpInvcNote, DateTime startDate, DateTime endDate, String custID, String slsperID, String deliveryID, Boolean includeFee,String tmpLoadGrid, String invcNote)
        {
            short intIncludeFee = 0;
            if (includeFee == false)
            {
                intIncludeFee = 0;
            }else{
                 intIncludeFee = 1;
            }
            if (tmpLoadGrid == "0")
            {
                var lst = _db.AR_TransLoadRedInvoice(branchID, invcNbr, tmpInvcNote).ToList();
                var lstFilter = _db.AR_GetDocForRedInvoice(branchID, taxID, invcNbr, tmpInvcNote, startDate, endDate, custID, slsperID, deliveryID, intIncludeFee).ToList();
                var lstRedInvoiceDoc = _db.AR_RedInvoiceDoc.Where(p => p.BranchID == branchID && p.InvcNbr == invcNbr).FirstOrDefault();
                if (invcNote == "")
                {
                    if (lstRedInvoiceDoc != null)
                    {
                        return this.Store(lst);
                    }
                    else
                    {
                        return this.Store(lstFilter);
                    }

                }
                else
                {

                    return this.Store(lstFilter);
                }
            }
            else
            {
                var lst = _db.AR_TransLoadRedInvoice(branchID, invcNbr, invcNote).ToList();
                var lstFilter = _db.AR_GetDocForRedInvoice(branchID, taxID, invcNbr, invcNote, startDate, endDate, custID, slsperID, deliveryID, intIncludeFee).ToList();
                var lstRedInvoiceDoc = _db.AR_RedInvoiceDoc.Where(p => p.BranchID == branchID && p.InvcNbr == invcNbr).FirstOrDefault();
                if (invcNote == "0")
                {
                    if (lstRedInvoiceDoc != null)
                    {
                        return this.Store(lst);
                    }
                    else
                    {
                        return this.Store(lstFilter);
                    }

                }
                else
                {

                    return this.Store(lstFilter);
                }
            }
            
            //return this.Store(lst);
        }

        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data,string invcNbr ,string branchID, string handle,string invcNote)
        {
            StoreDataHandler dataHandlerTop = new StoreDataHandler(data["lstheaderTop"]);
            ChangeRecords<AR_RedInvoiceDoc> lstheaderTop = dataHandlerTop.BatchObjectData<AR_RedInvoiceDoc>();
            StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_GetDocForRedInvoice_Result> lstgrd = dataHandlerGrid.BatchObjectData<AR_GetDocForRedInvoice_Result>();

            foreach (AR_RedInvoiceDoc created in lstheaderTop.Created)
            {
                var objHeader = _db.AR_RedInvoiceDoc.Where(p => p.BranchID == branchID && p.InvcNbr == created.InvcNbr).FirstOrDefault();
              
                if (objHeader == null)
                {
                    objHeader = new AR_RedInvoiceDoc();
                    if (handle == "R")
                    {
                        objHeader.Status = "C";
                    }
                    else if (handle == "N" || handle == "")
                    {
                        objHeader.Status = "H";
                    }
                    objHeader.BranchID = branchID;
                    objHeader.InvcNbr = invcNbr;
                    objHeader.Crtd_Datetime = DateTime.Now;
                    objHeader.Crtd_Prog = screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    Updating_AR_RedInvoiceDoc(created, ref objHeader);
                    if (objHeader.InvcNbr != "")
                    {
                        _db.AR_RedInvoiceDoc.AddObject(objHeader);
                    }
                }
                _db.SaveChanges();
            }


            foreach (AR_RedInvoiceDoc updated in lstheaderTop.Updated)
            {
                // Get the image path


                var objHeader = _db.AR_RedInvoiceDoc.Where(p => p.BranchID == branchID && p.InvcNbr == invcNbr).FirstOrDefault();
                if (objHeader != null)
                {
                    //updating
                    if (handle == "R")
                    {
                        objHeader.Status = "C";
                    }
                    else if (handle == "N" || handle == "")
                    {
                        objHeader.Status = "H";
                    }

                    

                    Updating_AR_RedInvoiceDoc(updated, ref objHeader);

                }
                else
                {
                    objHeader = new AR_RedInvoiceDoc();
                    if (handle == "R")
                    {
                        objHeader.Status = "C";
                    }
                    else if (handle == "N" || handle == "")
                    {
                        objHeader.Status = "H";
                    }
                    objHeader.BranchID = branchID;
                    objHeader.InvcNbr = invcNbr;
                    objHeader.Crtd_Datetime = DateTime.Now;
                    objHeader.Crtd_Prog = screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    Updating_AR_RedInvoiceDoc(updated, ref objHeader);
                    if (objHeader.InvcNbr != "")
                    {
                        _db.AR_RedInvoiceDoc.AddObject(objHeader);
                    }
                }
                _db.SaveChanges();


            }




            foreach(AR_GetDocForRedInvoice_Result deleted in lstgrd.Deleted)
            {
                var del = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == deleted.BatNbr && p.RefNbr == deleted.RefNbr && p.LineRef == deleted.LineRef).FirstOrDefault();
                if (del != null)
                {
                    del.InvcNbr = "";
                    del.InvcNote = "";
                }

            }
      



            foreach (AR_GetDocForRedInvoice_Result updated in lstgrd.Updated)
            {
                var record = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == updated.BatNbr && p.RefNbr == updated.RefNbr && p.LineRef == updated.LineRef).FirstOrDefault();

                if (record != null)
                {
                    if (record.tstamp.ToHex() != updated.tstamp.ToHex())
                    {
                        return Json(new { success = false });
                    }
                    if (updated.Selected == true)
                    {
                        record.InvcNbr = invcNbr;
                        record.InvcNote = invcNote;
                    }
                    else
                    {
                        record.InvcNbr = "";
                        record.InvcNote = "";
                    }
                }
                else
                {
               
                }


            }

            _db.SaveChanges();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [DirectMethod]
        public ActionResult DeleteFormTopAndGridIncluded(string branchID,string invcNbr)
        {
            var recordAR_RedInvoiceDoc = _db.AR_RedInvoiceDoc.FirstOrDefault(p => p.BranchID == branchID && p.InvcNbr == invcNbr);

            if (recordAR_RedInvoiceDoc != null)
            {
                _db.AR_RedInvoiceDoc.DeleteObject(recordAR_RedInvoiceDoc);
                var recordGridTrans = _db.AR_Trans.Where(p => p.BranchID == branchID && p.InvcNbr == invcNbr).ToList();
                if (recordGridTrans != null)
                {
                    for (int i = 0; i < recordGridTrans.Count; i++)
                    {
                        recordGridTrans[i].InvcNbr = "";
                        recordGridTrans[i].InvcNote = "";
                        _db.SaveChanges();
                    }
                }
            }
            _db.SaveChanges();
            return this.Direct();
        }



        private void Updating_AR_RedInvoiceDoc(AR_RedInvoiceDoc s, ref AR_RedInvoiceDoc d)
        {
            d.InvcNote = s.InvcNote;
            
            //d.Status = s.Status;
            d.PerPost = "";
            d.CustID = s.CustID;
            d.DocDate =Convert.ToDateTime(s.DocDate).Short();
            d.DocDesc = s.DocDesc;
            d.TaxID = s.TaxID;
            d.CuryTaxAmt = s.CuryTaxAmt;
            d.CuryTxblAmt = s.CuryTxblAmt;
            d.DiscAmt = s.DiscAmt;
            d.SOFee = s.SOFee;
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        //private void UpdatingAR_Trans(AR_GetDocForRedInvoice_Result s, ref AR_Trans d)
        //{
        //    d.InvcNote = s.InvcNote;

        //    d.Status = s.Status;
        //    d.CustID = s.CustID;
        //    d.DocDate = Convert.ToDateTime(s.DocDate).Short();
        //    d.DocDesc = s.DocDesc;
        //    d.TaxID = s.TaxID;
        //    d.CuryTaxAmt = s.CuryTaxAmt;
        //    d.CuryTxblAmt = s.CuryTxblAmt;
        //    d.DiscAmt = s.DiscAmt;
        //    d.SOFee = s.SOFee;

        //    d.LUpd_DateTime = DateTime.Now;
        //    d.LUpd_Prog = screenNbr;
        //    d.LUpd_User = Current.UserName;
        //}

    




        //private string functionBatNbrIfNull(string branchID)
        //{
        //    var recordLastBatNbr = _db.ARNumbering(branchID, "BatNbr").FirstOrDefault();
        //    return recordLastBatNbr;
        //}

        //private string functionRefNbrIfNull(string branchID)
        //{
        //    var recordLastBatNbr = _db.ARNumbering(branchID, "RefNbr").FirstOrDefault();
        //    return recordLastBatNbr;
        //}
       

    }
}











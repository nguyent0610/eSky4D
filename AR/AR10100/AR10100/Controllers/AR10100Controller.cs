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
namespace AR10100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10100Controller : Controller
    {
        string screenNbr = "AR10100";
        AR10100Entities _db = Util.CreateObjectContext<AR10100Entities>(false);
        

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
            var lst = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).FirstOrDefault();
            return this.Store(lst);
        }

        public ActionResult GetDataGrid(String branchID, String batNbr, String refNbr)
        {
            var lst = _db.AR10100_pgLoadGridTrans(branchID,batNbr,refNbr).ToList();

            return this.Store(lst);
        }

        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string branchID, string handle, string batNbr, string refNbr, string docType, string intRefNbr)
        {
            StoreDataHandler dataHandlerTop = new StoreDataHandler(data["lstheaderTop"]);
            ChangeRecords<Batch> lstheaderTop = dataHandlerTop.BatchObjectData<Batch>();
            StoreDataHandler dataHandlerBot = new StoreDataHandler(data["lstheaderBot"]);
            ChangeRecords<AR_Doc> lstheaderBot = dataHandlerBot.BatchObjectData<AR_Doc>();
            StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR10100_pgLoadGridTrans_Result> lstgrd = dataHandlerGrid.BatchObjectData<AR10100_pgLoadGridTrans_Result>();

            var docDate = data["txtDocDate"];
            var invcDate = data["txtInvcDate"];
            var invcNbr = data["txtInvcNbr"];
            var invcNote = data["txtInvcNote"];
            var toAmt = data["txtCuryCrTot"];
            var tmpGridChangeOrNot = 0;
            //var invcDate = data["txtInvcDate"];


            var batNbrIfNull = 0;
            var tmpBatNbr = "";
            var tmpRefNbr = "";
            var tmpcatchHandle = "";
            if (handle == "R")
            {
                var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10100" && p.BatNbr == batNbr).FirstOrDefault();
                var recordRefNbrUpdate = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).FirstOrDefault();
                recordBatNbrUpdate.Rlsed = 1;
                recordBatNbrUpdate.Status = "C";
                recordRefNbrUpdate.Rlsed = 1;
                tmpcatchHandle = "1";
            }


            //ap_doc phuc tap neu nhu de trong BatNbr thi ngoai viec them moi ap_doc thi phai them moi ca Batch
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
                    objHeader.TaxId00 = "";
                    objHeader.TaxId01 = "";
                    objHeader.TaxId02 = "";
                    objHeader.TaxId03 = "";
                    UpdatingFormBotAP_Doc(created, ref objHeader);

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
                            objHeaderBatNbr.EditScrnNbr = "AR10100";
                            objHeaderBatNbr.JrnlType = "AR";
                            objHeaderBatNbr.OrigBranchID = "";
                            objHeaderBatNbr.IntRefNbr = intRefNbr;
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

                    objHeader.TaxId00 = "";
                    objHeader.TaxId01 = "";
                    objHeader.TaxId02 = "";
                    objHeader.TaxId03 = "";

                    UpdatingFormBotAP_Doc(updated, ref objHeader);

                }
                else
                {

                }


            }










            foreach (AR10100_pgLoadGridTrans_Result created in lstgrd.Created)
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
                    //record.InvcDate = Convert.ToDateTime(invcDate);
                    record.JrnlType = "AR";
                    record.InvcNbr = invcNbr;
                    record.InvcNote = invcNote;
                    UpdatingGridAP_Trans(created, ref record);

                    //record.VendID = vendID;
                    //record.VendName = recordVendor.Name;
                    record.Crtd_DateTime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;
                    if (record.BranchID != "" && record.BatNbr != "" && record.RefNbr != "" && record.InvtId != "")
                    {
                        _db.AR_Trans.AddObject(record);
                    }
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



            foreach (AR10100_pgLoadGridTrans_Result updated in lstgrd.Updated)
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

                    UpdatingGridAP_Trans(updated, ref record);
                    var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10100" && p.BatNbr == record.BatNbr).FirstOrDefault();
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
                        if (record == null && updated.LineRef != "")
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
                            //record.InvcDate = Convert.ToDateTime(invcDate);
                            record.JrnlType = "AR";
                            record.InvcNbr = invcNbr;
                            record.InvcNote = invcNote;
                            UpdatingGridAP_Trans(updated, ref record);

                            //record.VendID = vendID;
                            //record.VendName = recordVendor.Name;
                            record.Crtd_DateTime = DateTime.Now;
                            record.Crtd_Prog = screenNbr;
                            record.Crtd_User = Current.UserName;
                            if (record.BranchID != "" && record.BatNbr != "" && record.RefNbr != "" && record.InvtId != "")
                            {
                                _db.AR_Trans.AddObject(record);
                            }
                            //_db.SaveChanges();

                        }

                    }
                    else
                    {

                        return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }


            foreach (AR10100_pgLoadGridTrans_Result deleted in lstgrd.Deleted)
            {

                var del = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr &&
                                             p.LineRef == deleted.LineRef).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_Trans.DeleteObject(del);

                }
                tmpGridChangeOrNot = 1;
            }


            var recordBatchChange = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.EditScrnNbr == "AR10100" && p.BatNbr == batNbr).FirstOrDefault();
            if (recordBatchChange != null)
            {
                recordBatchChange.TotAmt = Convert.ToDouble(toAmt);
                recordBatchChange.IntRefNbr = intRefNbr;
            }


            _db.SaveChanges();
            return Json(new { success = true, value = batNbrIfNull, value1 = tmpRefNbr, value2 = tmpBatNbr, value3 = tmpcatchHandle, value4 = tmpGridChangeOrNot }, JsonRequestBehavior.AllowGet);


        }

        [DirectMethod]
        public ActionResult DeleteFormTopBatch(string batNbr, string branchID)
        {
            var recordTopBatch = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.Module == "AP");
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

        [DirectMethod]
        public ActionResult DeleteFormBotAP_Doc(string refNbr, string batNbr,string branchID)
        {
            var recordBotAP_Doc = _db.AR_Doc.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr);

            if (recordBotAP_Doc != null)
            {
                _db.AR_Doc.DeleteObject(recordBotAP_Doc);
                var recordGridTrans = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
                if (recordGridTrans != null)
                {
                    for (int i = 0; i < recordGridTrans.Count; i++)
                    {
                        _db.AR_Trans.DeleteObject(recordGridTrans[i]);
                    }
                }
            }
            _db.SaveChanges();
            return this.Direct();
        }



        private void UpdatingGridAP_Trans(AR10100_pgLoadGridTrans_Result s, ref AR_Trans d)
        {
            d.LineRef = s.LineRef;
            d.InvtId = s.InvtID;
            d.LineType = s.LineType;



            d.Qty = s.Qty;
            d.TaxAmt00 = s.TaxAmt00;
            d.TaxAmt01 = s.TaxAmt01;
            d.TaxAmt02 = s.TaxAmt02;
            d.TaxAmt03 = s.TaxAmt03;
            d.TaxCat = s.TaxCat;
            d.TaxId00 = s.TaxId00;
            d.TaxId01 = s.TaxId01;
            d.TaxId02 = s.TaxId02;
            d.TaxId03 = s.TaxId03;
            d.TranAmt = s.TranAmt;
            d.TranDate = DateTime.Today;
            d.TranDesc = s.TranDesc;
            d.TranType = s.TranType;
            d.TxblAmt00 = s.TxblAmt00;
            d.TxblAmt01 = s.TxblAmt01;
            d.TxblAmt02 = s.TxblAmt02;
            d.TxblAmt03 = s.TxblAmt03;
            d.UnitPrice = s.UnitPrice;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void UpdatingFormBotAP_Doc(AR_Doc s, ref AR_Doc d)
        {
            d.DiscDate = s.DiscDate;
            d.DocBal = s.DocBal;
            d.DocDate = s.DocDate;
            d.DocDesc = s.DocDesc;
            d.DueDate = s.DueDate;
            //d.InvcDate = s.InvcDate;

            d.SlsperId = s.SlsperId;
            d.CustId = s.CustId;
            d.TxblTot00 = s.TxblTot00;
            d.TaxTot00 = s.TaxTot00;

            d.InvcNbr = s.InvcNbr;
            d.InvcNote = s.InvcNote;
            d.OrigDocAmt = s.OrigDocAmt;
            //d.PONbr = s.PONbr;
            //d.RcptNbr = s.RcptNbr;
            d.Terms = s.Terms;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }




        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AR10100_ppARNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AR10100_ppARNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }
        //private string functionBatNbrIfNull(int batNbrIfNull)
        //{
        //    //batNbrIfNull = Convert.ToInt32(recordBatNbr.BatNbr) + 1;
        //    var stringbatNbr = "";
        //    if (batNbrIfNull < 10)
        //    {
        //        stringbatNbr = "00000" + Convert.ToString(batNbrIfNull);
        //        return stringbatNbr;
        //    }
        //    else if (batNbrIfNull >= 10 && batNbrIfNull < 100)
        //    {
        //        stringbatNbr = "0000" + Convert.ToString(batNbrIfNull);
        //        return stringbatNbr;
        //    }
        //    else if (batNbrIfNull >= 100 && batNbrIfNull < 1000)
        //    {
        //        stringbatNbr = "000" + Convert.ToString(batNbrIfNull);
        //        return stringbatNbr;
        //    }
        //    else if (batNbrIfNull >= 1000 && batNbrIfNull < 10000)
        //    {
        //        stringbatNbr = "00" + Convert.ToString(batNbrIfNull);
        //        return stringbatNbr;
        //    }
        //    else if (batNbrIfNull >= 10000 && batNbrIfNull < 100000)
        //    {
        //        stringbatNbr = "0" + Convert.ToString(batNbrIfNull);
        //        return stringbatNbr;
        //    }
        //    else if (batNbrIfNull >= 100000 && batNbrIfNull < 1000000)
        //    {
        //        stringbatNbr = Convert.ToString(batNbrIfNull);
        //        return stringbatNbr;
        //    }
        //    else
        //    {
        //        return "";
        //    }

        //}

        //private string functionRefNbrIfNull(int refNbrIfNull)
        //{
        //    //batNbrIfNull = Convert.ToInt32(recordBatNbr.BatNbr) + 1;
        //    var stringrefNbr = "";
        //    if (refNbrIfNull < 10)
        //    {
        //        stringrefNbr = "00000" + Convert.ToString(refNbrIfNull);
        //        return stringrefNbr;
        //    }
        //    else if (refNbrIfNull >= 10 && refNbrIfNull < 100)
        //    {
        //        stringrefNbr = "0000" + Convert.ToString(refNbrIfNull);
        //        return stringrefNbr;
        //    }
        //    else if (refNbrIfNull >= 100 && refNbrIfNull < 1000)
        //    {
        //        stringrefNbr = "000" + Convert.ToString(refNbrIfNull);
        //        return stringrefNbr;
        //    }
        //    else if (refNbrIfNull >= 1000 && refNbrIfNull < 10000)
        //    {
        //        stringrefNbr = "00" + Convert.ToString(refNbrIfNull);
        //        return stringrefNbr;
        //    }
        //    else if (refNbrIfNull >= 10000 && refNbrIfNull < 100000)
        //    {
        //        stringrefNbr = "0" + Convert.ToString(refNbrIfNull);
        //        return stringrefNbr;
        //    }
        //    else if (refNbrIfNull >= 100000 && refNbrIfNull < 1000000)
        //    {
        //        stringrefNbr = Convert.ToString(refNbrIfNull);
        //        return stringrefNbr;
        //    }
        //    else
        //    {
        //        return "";
        //    }

        //}

    }
}


//if (batNbrIfNull < 10)
//{
//    record.BatNbr = "00000" + batNbrIfNull.ToString();
//}
//else if (batNbrIfNull >= 10 && batNbrIfNull < 100)
//{
//    record.BatNbr = "0000" + batNbrIfNull.ToString();
//}
//else if (batNbrIfNull >= 100 && batNbrIfNull < 1000)
//{
//    record.BatNbr = "000" + batNbrIfNull.ToString();
//}
//else if (batNbrIfNull >= 1000 && batNbrIfNull < 10000)
//{
//    record.BatNbr = "00" + batNbrIfNull.ToString();
//}
//else if (batNbrIfNull >= 10000 && batNbrIfNull < 100000)
//{
//    record.BatNbr = "0" + batNbrIfNull.ToString();
//}
//else if (batNbrIfNull >= 100000 && batNbrIfNull < 1000000)
//{
//    record.BatNbr = batNbrIfNull.ToString();
//}










//if (RefNbrIfNull < 10)
//{
//    record.RefNbr = "00000" + RefNbrIfNull.ToString();
//}
//else if (RefNbrIfNull >= 10 && RefNbrIfNull < 100)
//{
//    record.RefNbr = "0000" + RefNbrIfNull.ToString();
//}
//else if (RefNbrIfNull >= 100 && RefNbrIfNull < 1000)
//{
//    record.RefNbr = "000" + RefNbrIfNull.ToString();
//}
//else if (RefNbrIfNull >= 1000 && RefNbrIfNull < 10000)
//{
//    record.RefNbr = "00" + RefNbrIfNull.ToString();
//}
//else if (RefNbrIfNull >= 10000 && RefNbrIfNull < 100000)
//{
//    record.RefNbr = "0" + RefNbrIfNull.ToString();
//}
//else if (RefNbrIfNull >= 100000 && RefNbrIfNull < 1000000)
//{
//    record.RefNbr = RefNbrIfNull.ToString();
//}

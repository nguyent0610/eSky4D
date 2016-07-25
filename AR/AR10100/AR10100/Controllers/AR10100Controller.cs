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
using HQFramework.DAL;
using System.IO;
using Aspose.Cells;
using System.Globalization;
using HQ.eSkySys;

namespace AR10100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10100Controller : Controller
    {
        string _screenNbr = "AR10100";
        AR10100Entities _db = Util.CreateObjectContext<AR10100Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;

        public ActionResult Index(string branchID)
        {
            Util.InitRight(_screenNbr);
            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);

            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }

            if (branchID == null) branchID = Current.CpnyID;

            //var userDft = _db.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID == branchID);

            //ViewBag.INSite = userDft == null ? "" : userDft.INSite;
            ViewBag.BranchID = branchID;

            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetAR10100_pdHeader(string branchID, string batNbr)
        {
            var obj = _db.AR10100_pdHeader(branchID,batNbr).FirstOrDefault();
            return this.Store(obj);
        }    
 
        public ActionResult GetData_AR_Trans(string branchID, string batNbr, string refNbr)
        {
            var lst = _db.AR10100_pgLoadInvoiceMemo(branchID,batNbr,refNbr,"%").ToList();

            return this.Store(lst);
        }

        public ActionResult GetAR10100_pgLoadTaxTrans(string branchID, string batNbr, string refNbr)
        {
            return this.Store(_db.AR10100_pgLoadTaxTrans(branchID,batNbr,refNbr).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                var branchID = data["txtBranchID"];
                var batNbr = data["cboBatNbr"];
                var refNbr = data["txtRefNbr"];
                var handle = data["cboHandle"];
                  
                DateTime dpoDate = data["PODate"].ToDateShort();

                var detHeader = new StoreDataHandler(data["lstheader_Batch"]);
                var _header = detHeader.ObjectData<AR10100_pdHeader_Result>().FirstOrDefault();
       
                StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
                ChangeRecords<AR10100_pgLoadInvoiceMemo_Result> lstgrd = dataHandlerGrid.BatchObjectData<AR10100_pgLoadInvoiceMemo_Result>();

                //Kiem tra dong ngay 
                if (_db.AR10100_ppCheckCloseDate(branchID, Convert.ToDateTime(data["dteDocDate"]).ToDateShort(), "AR10100").FirstOrDefault() == "0")
                    throw new MessageException(MessageType.Message, "301");

                var tmpBatNbr = "";
                var tmpRefNbr = "";
                #region save batch
           
                var objBatch = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "AR" && p.BatNbr == batNbr).FirstOrDefault();
                if (batNbr.PassNull()=="")//new record
                {
                    if (objBatch != null)
                        return Json(new { success = false, msgCode = 2000, msgParam = batNbr });//quang message ma nha cung cap da ton tai ko the them
                    else
                    {
                        objBatch = new Batch();
                        objBatch.ResetET();
                        objBatch.BatNbr = _db.ARNumbering(branchID, "BatNbr").FirstOrDefault();
                        tmpBatNbr = objBatch.BatNbr;

                        objBatch.RefNbr = _db.ARNumbering(branchID, "RefNbr").FirstOrDefault();
                        tmpRefNbr = objBatch.RefNbr;
                        objBatch.BranchID = branchID;
                        objBatch.Module = "AR";
                        objBatch.Crtd_DateTime = DateTime.Now;
                        objBatch.Crtd_Prog = _screenNbr;
                        objBatch.Crtd_User = Current.UserName;

                        Updating_Batch(_header, ref objBatch, data);

                        if (objBatch.BatNbr != "" && objBatch.BranchID != "" && objBatch.Module != "")
                        {
                            _db.Batches.AddObject(objBatch);
                                
                        }
                    }
                }
                else if (objBatch != null)//update record
                {
                    if (objBatch.tstamp.ToHex() == _header.tstamp.ToHex())
                    {
                        Updating_Batch(_header, ref objBatch, data);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                #endregion savebatch

                #region save Doc
            
                var objAR_Doc = _db.AR_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).FirstOrDefault();
                if (refNbr.PassNull()=="")//new record
                {
                    if (objAR_Doc != null)
                        return Json(new { success = false, msgCode = 2000, msgParam = batNbr });//quang message ma nha cung cap da ton tai ko the them
                    else
                    {
                        objAR_Doc = new AR_Doc();
                        objAR_Doc.ResetET();
                        objAR_Doc.BranchID = branchID;

                        if (batNbr != "") // neu batNbr da co roi tuc la chi tao moi RefNbr thoi
                        {
                            objAR_Doc.BatNbr = batNbr ;
                        }else{ // neu BatNbr chua co tuc la tao moi ca Bat va Ref
                            objAR_Doc.BatNbr = tmpBatNbr;
                        }
                        if (tmpRefNbr != "") // neu tmpRefNbr != "" tuc la tao moi ca Bat va Ref
                        {
                            objAR_Doc.RefNbr = tmpRefNbr;
                        }
                        else // neu tmpRefNbr = "" tuc la chi tao moi Ref
                        {
                            //tao Ref moi tu ham tu sinh
                            objAR_Doc.RefNbr = _db.ARNumbering(branchID, "RefNbr").FirstOrDefault();
                            tmpRefNbr = objAR_Doc.RefNbr; // gan vao bien tam de cho Grid xai
                        }

                        objAR_Doc.Crtd_DateTime = DateTime.Now;
                        objAR_Doc.Crtd_Prog = _screenNbr;
                        objAR_Doc.Crtd_User = Current.UserName;


                        Updating_AP_Doc(_header, ref objAR_Doc, data);
                        if (objAR_Doc.BatNbr != "" && objAR_Doc.BranchID != "" && objAR_Doc.RefNbr != "")
                        {
                            _db.AR_Doc.AddObject(objAR_Doc);
                        }
                    }
                }
                else if (objAR_Doc != null)//update record
                {                    
                     Updating_AP_Doc(_header, ref objAR_Doc, data);                    
                }
                else
                {
                    throw new MessageException(MessageType.Message, "19");
                }

                #endregion save Doc

                #region save tran
                foreach (AR10100_pgLoadInvoiceMemo_Result deleted in lstgrd.Deleted)
                {
                    var delGrid = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr &&
                                                 p.LineRef == deleted.LineRef).FirstOrDefault();
                    if (delGrid != null)
                    {
                        _db.AR_Trans.DeleteObject(delGrid);
                    }
                }

                //them hoac update cac record tren luoi
                lstgrd.Created.AddRange(lstgrd.Updated);

                foreach (AR10100_pgLoadInvoiceMemo_Result created in lstgrd.Created.Where(p=>p.TranAmt!=0))
                {
                    var objGrid = _db.AR_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr &&
                                                 p.LineRef == created.LineRef).FirstOrDefault();
                    if (created.tstamp.ToHex() == "")//dong nay la dong them moi
                    {
                        if (objGrid == null)
                        {
                            objGrid = new AR_Trans();
                            objGrid.ResetET();
                            objGrid.BranchID = branchID;

                            if (batNbr != "") // neu batNbr da co roi tuc la chi tao moi RefNbr thoi
                            {
                                objGrid.BatNbr = batNbr;
                            }
                            else
                            { // neu BatNbr chua co tuc la tao moi ca Bat va Ref
                                objGrid.BatNbr = tmpBatNbr;
                            }
                            if (refNbr != "") // neu refNbr != "" tuc la tao chi Grid thoi
                            {
                                objGrid.RefNbr = refNbr;
                            }
                            else // neu refNbr = "" tuc la chi tao moi ca Ref va Grid
                            {
                                //tao Ref moi tu bien tam da gan tren AP_Doc
                                objGrid.RefNbr = tmpRefNbr;
                            }
                            objGrid.LineRef = created.LineRef;                       
                            objGrid.Crtd_DateTime = DateTime.Now;
                            objGrid.Crtd_Prog = _screenNbr;
                            objGrid.Crtd_User = Current.UserName;
                            objGrid.tstamp = new byte[0];
                            Updating_AR_Trans(created, ref objGrid,data);
                            if (objGrid.BatNbr != "" && objGrid.BranchID != "" && objGrid.RefNbr != "" && objGrid.LineRef != "" )
                            {
                                _db.AR_Trans.AddObject(objGrid);
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
                        }
                    }
                    else
                    {
                        if (created.tstamp.ToHex() == objGrid.tstamp.ToHex())
                        {
                            Updating_AR_Trans(created, ref objGrid,data);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                } // ngoac ket thuc foreach cua Grid
                #endregion trans


                _db.SaveChanges();

                //xử lý ARProcess
                var batNbrRls = "";
                var refNbrRls = "";
                if (handle != "N" && handle!="")
                {
                    DataAccess dal = Util.Dal();
                    try
                    {
                        ARProcess.AR ar = new ARProcess.AR(Current.UserName, _screenNbr, dal);
                        if (handle == "R")
                        {
                            dal.BeginTrans(IsolationLevel.ReadCommitted);
                        
                            if (batNbr != "")
                            {
                                batNbrRls = batNbr;
                            }else{
                                batNbrRls = tmpBatNbr;
                            }

                            if (!ar.AR10100_Release(branchID, batNbrRls))
                            {
                                dal.RollbackTrans();
                            }
                            else
                            {
                                dal.CommitTrans();

                            }
                        }
                        else if (handle == "C" || handle == "V")
                        {
                            dal.BeginTrans(IsolationLevel.ReadCommitted);
                            if (batNbr != "")
                            {
                                batNbrRls = batNbr;
                            }
                            else
                            {
                                batNbrRls = tmpBatNbr;
                            }

                            if (refNbr != "")
                            {
                                refNbrRls = refNbr;
                            }
                            else
                            {
                                refNbrRls = tmpRefNbr;
                            }

                            if (!ar.AR10100_Cancel(branchID, batNbrRls, refNbrRls, handle))
                            {
                                dal.RollbackTrans();
                            }
                            else
                            {
                                dal.CommitTrans();
                            }                     
                        }
                        ar = null;
                    }
                    catch (Exception)
                    {
                        dal.RollbackTrans();
                        throw;
                    }
                    return Util.CreateMessage(MessageProcess.Process, new { RefNbr = objAR_Doc.RefNbr, BatNbr = objBatch.BatNbr });
                }

                return Util.CreateMessage(MessageProcess.Save, new { RefNbr = objAR_Doc.RefNbr, BatNbr = objBatch.BatNbr });
            }
            catch (Exception ex)
            {

                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult Delete_Batch(string batNbr, string branchID)
        {
            try
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
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
        }

        [HttpPost]
        public ActionResult Delete_AP_Doc(string refNbr, string batNbr,string branchID)
        {
            try
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
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
        }

        private void Updating_Batch(AR10100_pdHeader_Result s, ref Batch d, FormCollection data)
        {
            d.EditScrnNbr = "AR10100";
            d.JrnlType = "AR";
            d.OrigBranchID = "";

            if (data["txtCuryCrTot"] != "")
            {
                d.TotAmt = Convert.ToDouble(data["txtCuryCrTot"]);
            }
            else
            {
                d.TotAmt = 0;
            }
            d.DateEnt = Convert.ToDateTime(data["dteDocDate"]);

            d.IntRefNbr = data["txtOrigRefNbr"];

            if (data["cboHandle"] == "C" || data["cboHandle"] == "V")
                d.Status = "H";
            else
                d.Status = s.Status;
          
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void Updating_AP_Doc(AR10100_pdHeader_Result s, ref AR_Doc d, FormCollection data)
        {
            d.DocType = data["cboDocType"];
            d.DiscDate = s.DiscDate;
            d.DocBal = s.DocBal;
            d.DocDate = s.DocDate;
            d.DocDesc = s.DocDesc;
            d.DueDate = s.DueDate;          

            d.SlsperId = s.SlsperId;
            d.CustId = s.CustId;
            d.TxblTot00 = s.TxblTot00;
            d.TaxTot00 = s.TaxTot00;
            d.InvcNbr = s.InvcNbr;
            d.InvcNote = s.InvcNote;
            d.OrigDocAmt = s.OrigDocAmt;       
            d.Terms = s.Terms;
           
            var obj = _db.AR_Salesperson.Where(p => p.SlsperId == s.SlsperId).FirstOrDefault();
            if(obj!=null)
                d.DeliveryID = obj.DeliveryMan;
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void Updating_AR_Trans(AR10100_pgLoadInvoiceMemo_Result s, ref AR_Trans d, FormCollection data)
        {
            d.LineType = s.LineType;                 
            d.JrnlType = "AR";
            d.InvcNbr = data["txtInvcNbr"];
            d.InvcNote = data["txtInvcNote"];
            d.InvtId = s.InvtId;
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
            d.TranDate = Convert.ToDateTime(data["dteDocDate"]).ToDateShort();
            d.TranDesc = s.TranDesc;
            d.TranType = data["cboDocType"];// s.TranType;
            d.TxblAmt00 = s.TxblAmt00;
            d.TxblAmt01 = s.TxblAmt01;
            d.TxblAmt02 = s.TxblAmt02;
            d.TxblAmt03 = s.TxblAmt03;
            d.UnitPrice = s.UnitPrice;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
        }

        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                List<AR_Doc> lstAR_Doc = new List<AR_Doc>();

                string message = string.Empty;
                string errorDocType = string.Empty;
                string errorCustID = string.Empty;
                string errorCustIDnotExists = string.Empty;
                string errorDocDate = string.Empty;
                string errorDocDesc = string.Empty;
                string errorTranAmt = string.Empty;
                string errorTranAmtNotInput = string.Empty;
                string erorrTranAmtFormat = string.Empty;
                string errorDocDateFormat = string.Empty;
                string errorDuplicate = string.Empty;
                string errorDuplicateDB = string.Empty;
                string errorCloseDate = string.Empty;

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        string CpnyID = data["txtBranchID"].PassNull();
                        string DocType = string.Empty;
                        string CustID = string.Empty;
                        string InvcNbr = string.Empty;
                        string InvcNote = string.Empty;
                        string DocDate = string.Empty; // Ngay Chung Tu 
                        string DueDate = string.Empty; // Ngay Toi Han
                        string DocDesc = string.Empty; // Dien Giai Chung Tu
                        string TranAmt = string.Empty; // Thanh Tien
                        if (CpnyID != "")
                        {
                            var lstCustomer = _db.AR10100_pcCustomerActive(CpnyID, Current.UserName, _screenNbr).ToList();
                            var lstTerms = _db.AR10100_pcterms().ToList();
                            for (int i = 3; i <= workSheet.Cells.MaxDataRow; i++)
                            {
                                var objCust = new AR10100_pcCustomerActive_Result();
                                bool FlagCheck = false;
                                DocType = workSheet.Cells[i, 0].StringValue.PassNull();
                                CustID = workSheet.Cells[i, 1].StringValue.PassNull();
                                InvcNbr = workSheet.Cells[i, 4].StringValue.PassNull();
                                InvcNote = workSheet.Cells[i, 5].StringValue.PassNull();
                                DocDate = workSheet.Cells[i, 6].StringValue.PassNull();
                                DocDesc = workSheet.Cells[i, 7].StringValue.PassNull();
                                TranAmt = workSheet.Cells[i, 8].StringValue.PassNull();

                                if (DocType == "" && CustID == ""
                                    && InvcNbr == "" && InvcNote == "" && DocDate == ""
                                    && DocDesc == "" && TranAmt == "")
                                {
                                    continue;
                                }

                                if (DocType != "DM")
                                {
                                    errorDocType += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                                else if (CustID == "")
                                {
                                    errorCustID += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                                else if (DocDate == "")
                                {
                                    errorDocDate += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                                else if (DocDesc == "")
                                {
                                    errorDocDesc += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                                else if (TranAmt == "")
                                {
                                    errorTranAmt += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                                if (CustID != "")
                                {
                                    if (lstCustomer.FirstOrDefault(p => p.CustID == CustID) == null)
                                    {
                                        errorCustIDnotExists += (i + 1).ToString() + ",";
                                        FlagCheck = true;
                                    }
                                    else
                                        objCust = lstCustomer.FirstOrDefault(p => p.CustID == CustID);
                                }
                                if (DocDate != "")
                                {
                                    DateTime parsed;
                                    bool valid = DateTime.TryParseExact(DocDate, "yyyy/MM/dd",
                                                                        CultureInfo.InvariantCulture,
                                                                        DateTimeStyles.None,
                                                                        out parsed);

                                    if (valid == false)
                                    {
                                        errorDocDateFormat += (i + 1).ToString() + ",";
                                        FlagCheck = true;
                                    }
                                    else
                                    {
                                        if (_db.AR10100_ppCheckCloseDate(CpnyID, parsed.ToDateShort(), "AR10100").FirstOrDefault() == "0")
                                            errorCloseDate += (i + 1).ToString() + ",";
                                    }
                                }
                                if (TranAmt != "")
                                {
                                    float n;
                                    bool isNumeric = float.TryParse(TranAmt, out n);
                                    if (isNumeric == true)
                                    {
                                        if (n == 0 || n < 0)
                                        {
                                            errorTranAmtNotInput += (i + 1).ToString() + ",";
                                            FlagCheck = true;
                                        }
                                    }
                                    else
                                    {
                                        erorrTranAmtFormat += (i + 1).ToString() + ",";
                                        FlagCheck = true;
                                    }
                                }

                                if (FlagCheck == true)
                                {
                                    continue;
                                }

                                //Luu thanh tien TranAmt vao TotAmt, OrigDocAmt, DocBal, TxblTot00
                                //Han Thanh Toan lay field Terms o AR_Customer
                                //Ngay Chiet Khau  lay Ngay Chung Tu
                                //DiscDate = DocDate
                                //DueDate = DocDate + Terms.DueIntrv
                                var tmpBatNbr = "";
                                var tmpRefNbr = "";
                                string[] strDocDate = DocDate.Split('/');
                                DateTime tmpDocDate = new DateTime(int.Parse(strDocDate[0]), int.Parse(strDocDate[1]), int.Parse(strDocDate[2]));

                                #region Save Batch
                                var objBatch = new Batch();
                                objBatch.ResetET();
                                objBatch.BatNbr = _db.ARNumbering(CpnyID, "BatNbr").FirstOrDefault();
                                tmpBatNbr = objBatch.BatNbr;

                                objBatch.RefNbr = _db.ARNumbering(CpnyID, "RefNbr").FirstOrDefault();
                                tmpRefNbr = objBatch.RefNbr;

                                objBatch.BranchID = CpnyID;
                                objBatch.Module = "AR";
                                objBatch.EditScrnNbr = "AR10100";
                                objBatch.JrnlType = "AR";
                                objBatch.OrigBranchID = "";
                                objBatch.TotAmt = Convert.ToDouble(TranAmt);
                                objBatch.DateEnt = tmpDocDate;
                                objBatch.Status = "H";
                                objBatch.Crtd_DateTime = DateTime.Now;
                                objBatch.Crtd_Prog = _screenNbr;
                                objBatch.Crtd_User = Current.UserName;
                                objBatch.LUpd_DateTime = DateTime.Now;
                                objBatch.LUpd_Prog = _screenNbr;
                                objBatch.LUpd_User = Current.UserName;

                                if (objBatch.BatNbr != "" && objBatch.BranchID != "" && objBatch.Module != "")
                                    _db.Batches.AddObject(objBatch);

                                #endregion

                                #region Save Doc
                                var objAR_Doc = new AR_Doc();
                                objAR_Doc.ResetET();
                                objAR_Doc.BranchID = CpnyID;
                                objAR_Doc.BatNbr = tmpBatNbr;
                                objAR_Doc.RefNbr = tmpRefNbr;
                                objAR_Doc.Crtd_DateTime = DateTime.Now;
                                objAR_Doc.Crtd_Prog = _screenNbr;
                                objAR_Doc.Crtd_User = Current.UserName;
                                objAR_Doc.LUpd_DateTime = DateTime.Now;
                                objAR_Doc.LUpd_Prog = _screenNbr;
                                objAR_Doc.LUpd_User = Current.UserName;

                                objAR_Doc.DocType = DocType;
                                objAR_Doc.DiscDate = tmpDocDate;
                                objAR_Doc.DocBal = Convert.ToDouble(TranAmt);
                                objAR_Doc.DocDate = tmpDocDate;
                                objAR_Doc.DocDesc = DocDesc;

                                objAR_Doc.DueDate = tmpDocDate.AddDays(lstTerms.FirstOrDefault(p => p.TermsID == objCust.Terms).DueIntrv);

                                objAR_Doc.SlsperId = "";
                                objAR_Doc.CustId = CustID;
                                objAR_Doc.TxblTot00 = Convert.ToDouble(TranAmt);
                                objAR_Doc.OrigDocAmt = Convert.ToDouble(TranAmt);
                                objAR_Doc.TaxTot00 = 0;
                                objAR_Doc.InvcNbr = InvcNbr;
                                objAR_Doc.InvcNote = InvcNote;
                                objAR_Doc.Terms = objCust.Terms;
                                
                                if (objAR_Doc.BatNbr != "" && objAR_Doc.BranchID != "" && objAR_Doc.RefNbr != "")
                                    _db.AR_Doc.AddObject(objAR_Doc);
                                #endregion

                                #region Save Trans
                                var objGrid = new AR_Trans();
                                objGrid.ResetET();
                                objGrid.BranchID = CpnyID;
                                objGrid.BatNbr = tmpBatNbr;
                                objGrid.RefNbr = tmpRefNbr;
                                objGrid.LineRef = "00001";
                                objGrid.Crtd_DateTime = DateTime.Now;
                                objGrid.Crtd_Prog = _screenNbr;
                                objGrid.Crtd_User = Current.UserName;
                                objGrid.LUpd_DateTime = DateTime.Now;
                                objGrid.LUpd_Prog = _screenNbr;
                                objGrid.LUpd_User = Current.UserName;

                                objGrid.LineType = "N";
                                objGrid.JrnlType = "AR";
                                objGrid.InvcNbr = InvcNbr;
                                objGrid.InvcNote = InvcNote;
                                objGrid.InvtId = "";
                                objGrid.Qty = 0;
                                objGrid.TaxAmt00 = 0;
                                objGrid.TaxAmt01 = 0;
                                objGrid.TaxAmt02 = 0;
                                objGrid.TaxAmt03 = 0;
                                objGrid.TaxCat = "";
                                objGrid.TaxId00 = "";
                                objGrid.TaxId01 = "";
                                objGrid.TaxId02 = "";
                                objGrid.TaxId03 = "";
                                objGrid.TranAmt = Convert.ToDouble(TranAmt);
                                objGrid.TranDate = tmpDocDate.ToDateShort();
                                objGrid.TranDesc = CustID + " - " + objCust.Name;
                                objGrid.TranType = DocType;
                                objGrid.TxblAmt00 = Convert.ToDouble(TranAmt);
                                objGrid.TxblAmt01 = 0;
                                objGrid.TxblAmt02 = 0;
                                objGrid.TxblAmt03 = 0;
                                objGrid.UnitPrice = 0;

                                if (objGrid.BatNbr != "" && objGrid.BranchID != "" && objGrid.RefNbr != "" && objGrid.LineRef != "")
                                    _db.AR_Trans.AddObject(objGrid);
                                #endregion
                            }

                            message = errorCloseDate == "" ? "" : string.Format("Dòng: {0} ngày chứng từ không nằm trong phạm vi cho phép nhập liệu của bạn</br>", errorCloseDate);
                            message += errorDocType == "" ? "" : string.Format("{0} dòng: {1} không thuộc loại Phiếu Báo Nợ(DM) </br>", "Loại Chứng Từ", errorDocType);
                            message += errorCustID == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", "Mã KH", errorCustID);
                            message += errorCustIDnotExists == "" ? "" : string.Format("{0} dòng: {1} không tồn tại</br>", "Mã KH", errorCustIDnotExists);
                            message += errorDocDate == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", "Ngày Chứng Từ", errorDocDate);
                            message += errorDocDesc == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", "Diễn Giải Chứng Từ", errorDocDesc);
                            message += errorTranAmt == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", "Thành Tiền", errorTranAmt);
                            message += errorTranAmtNotInput == "" ? "" : string.Format("{0} dòng: {1} thành tiền phải lớn hơn 0</br>", "Thành Tiền", errorTranAmtNotInput);
                            message += erorrTranAmtFormat == "" ? "" : string.Format("{0} dòng: {1} không đúng định dạng kiểu số</br>", "Thành Tiền", erorrTranAmtFormat);
                            message += errorDocDateFormat == "" ? "" : string.Format("{0} dòng: {1} không đúng định dạng (yyyy/MM/dd)</br>", "Ngày Chứng Từ", errorDocDateFormat);

                            if (message == "" || message == string.Empty)
                            {
                                _db.SaveChanges();
                            }
                            Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                        }
                    }
                    return _logMessage;
                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
            return _logMessage;
        }
    }
}


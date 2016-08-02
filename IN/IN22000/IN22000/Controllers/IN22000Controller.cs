using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
//using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
using Aspose.Cells;
using HQFramework.DAL;
using System.Data;
using System.Drawing;
using System.Web;
namespace IN22000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN22000Controller : Controller
    {
        private string _screenNbr = "IN22000";
        private string _userName = Current.UserName;
        private string _beginStatus = "H";
        IN22000Entities _db = Util.CreateObjectContext<IN22000Entities>(false);
        private JsonResult _logMessage;
        //
        // GET: /IN22000/
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetPosmInfo(string posmID)
        {
            var posm = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID == posmID);
            return this.Store(posm);
        }

        public ActionResult GetBranch(string posmID)
        {
            var dets = _db.IN22000_pgBranch(Current.UserName, posmID).ToList();
            return this.Store(dets);
        }

        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var posmID = data["cboPosmID"];
                if (!string.IsNullOrWhiteSpace(posmID))
                {
                    var posm = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID == posmID);

                    if (posm != null)
                    {
                        var custHandler = new StoreDataHandler(data["lstDetChange"]);
                        var lstDetChange = custHandler.BatchObjectData<IN22000_pgBranch_Result>();

                        foreach (var created in lstDetChange.Created)
                        {
                            if (!string.IsNullOrWhiteSpace(created.BranchID))
                            {
                                created.PosmID = posmID;

                                var createdCpny = _db.IN_POSMCust.FirstOrDefault(
                                    x => x.BranchID == created.BranchID
                                        && x.PosmID == created.PosmID
                                        && x.CustID == "."
                                        && x.PosmCode == created.PosmCode);
                                if (createdCpny == null)
                                {
                                    createdCpny = new IN_POSMCust();
                                    updateBranch(ref createdCpny, created, true);
                                    _db.IN_POSMCust.AddObject(createdCpny);
                                }
                            }
                        }

                        foreach (var updated in lstDetChange.Updated)
                        {
                            if (!string.IsNullOrWhiteSpace(updated.BranchID))
                            {
                                updated.PosmID = posmID;

                                var updatedCpny = _db.IN_POSMCust.FirstOrDefault(
                                    x => x.BranchID == updated.BranchID
                                        && x.PosmID == updated.PosmID
                                        && x.CustID == "."
                                        && x.PosmCode == updated.PosmCode);
                                if (updatedCpny != null)
                                {
                                    updateBranch(ref updatedCpny, updated, true);
                                }
                            }
                        }

                        foreach (var deleted in lstDetChange.Deleted)
                        {
                            if (!string.IsNullOrWhiteSpace(deleted.BranchID))
                            {
                                deleted.PosmID = posmID;

                                var deletedCpny = _db.IN_POSMCust.FirstOrDefault(
                                    x => x.BranchID == deleted.BranchID
                                        && x.PosmID == deleted.PosmID
                                        && x.CustID == "."
                                        && x.PosmCode == deleted.PosmCode);
                                if (deletedCpny != null)
                                {
                                    _db.IN_POSMCust.DeleteObject(deletedCpny);
                                }
                            }
                        }

                        _db.SaveChanges();
                        return Json(new { success = true, msgCode = 201405071 });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "8");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "744");
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        public ActionResult DeletePosm(string posmID)
        {
            try
            {
                var posm = _db.IN_POSMHeader.FirstOrDefault(p => p.PosmID == posmID);
                if (posm != null)
                {
                    var cpnies = _db.IN_POSMCust.Where(c => c.PosmID == posmID).ToList();
                    foreach (var cpny in cpnies)
                    {
                        if (cpny.Status == _beginStatus)
                        {
                            _db.IN_POSMCust.DeleteObject(cpny);
                        }
                        else 
                        {
                            throw new MessageException(MessageType.Message, "20140306");
                        }
                    }
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("PosmID") });
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        private void updateBranch(ref IN_POSMCust createdCpny, IN22000_pgBranch_Result created, bool isNew)
        {
            if (isNew)
            {
                createdCpny.ResetET();
                createdCpny.PosmID = created.PosmID;
                createdCpny.BranchID = created.BranchID;
                createdCpny.CustID = ".";
                createdCpny.SlsperID = "";
                createdCpny.PosmCode = created.PosmCode;
                createdCpny.Status = _beginStatus;
                createdCpny.IsAgree = false;

                createdCpny.Crtd_DateTime = DateTime.Now;
                createdCpny.Crtd_Prog = _screenNbr;
                createdCpny.Crtd_User = Current.UserName;
            }
            createdCpny.Qty = created.Qty;
            createdCpny.AppQty = created.Qty;
            createdCpny.LUpd_DateTime = DateTime.Now;
            createdCpny.LUpd_Prog = _screenNbr;
            createdCpny.LUpd_User = Current.UserName;
        }

        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();

                Worksheet SheetPOSM = workbook.Worksheets[0];
                SheetPOSM.Name = "POSM";
                workbook.Worksheets.Add();
                Worksheet SheetDataMaster = workbook.Worksheets[1];
                SheetDataMaster.Name = "Master";

                DataAccess dal = Util.Dal();
                Cell cell;
                Range range;
                int numRow = 1000;

                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, Current.UserName, ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, Current.CpnyID, ParameterDirection.Input, 30));
                DataTable dtPOSM = dal.ExecDataTable("IN22000_pdPOSMImport", CommandType.StoredProcedure, ref pc);

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, Current.UserName, ParameterDirection.Input, 30));
                DataTable dtBranchID = dal.ExecDataTable("IN22000_pdBranchIDImport", CommandType.StoredProcedure, ref pc);

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, Current.UserName, ParameterDirection.Input, 30));
                DataTable dtProduct = dal.ExecDataTable("IN22000_pdPosmProductImport", CommandType.StoredProcedure, ref pc);

                int iRow = 2;
                SheetDataMaster.Cells["A1"].PutValue("POSMID");
                for (int i = 0; i < dtPOSM.Rows.Count; i++)
                {
                    cell = SheetDataMaster.Cells["A" + iRow];
                    cell.PutValue(dtPOSM.Rows[i]["POSMID"].ToString());
                    iRow++;
                }

                iRow = 2;
                SheetDataMaster.Cells["B1"].PutValue("POSMID");
                SheetDataMaster.Cells["C1"].PutValue("BranchName");
                SheetDataMaster.Cells["D1"].PutValue("BranchID");
                for (int i = 0; i < dtBranchID.Rows.Count; i++)
                {
                    cell = SheetDataMaster.Cells["B" + iRow];
                    cell.PutValue(dtBranchID.Rows[i]["POSMID"].ToString());
                    cell = SheetDataMaster.Cells["C" + iRow];
                    cell.PutValue(dtBranchID.Rows[i]["BranchID"].ToString());
                    cell = SheetDataMaster.Cells["D" + iRow];
                    cell.PutValue(dtBranchID.Rows[i]["BranchName"].ToString());
                    iRow++;
                }

                iRow = 2;
                SheetDataMaster.Cells["E1"].PutValue("InvtName");
                SheetDataMaster.Cells["F1"].PutValue("InvtID");
                for (int i = 0; i < dtProduct.Rows.Count; i++)
                {
                    cell = SheetDataMaster.Cells["E" + iRow];
                    cell.PutValue(dtProduct.Rows[i]["InvtID"].ToString());
                    cell = SheetDataMaster.Cells["F" + iRow];
                    cell.PutValue(dtProduct.Rows[i]["InvtName"].ToString());
                    iRow++;
                }

                // POSMID
                Validation validation = SheetPOSM.Validations[SheetPOSM.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=Master! $A$2:$A$" + (dtPOSM.Rows.Count + 1);
                validation.ShowError = true;
                validation.AlertStyle = ValidationAlertType.Stop;
                validation.ErrorTitle = "Error";
                validation.InputMessage = "Choose POSMID";
                validation.ErrorMessage = "POSMID isn't existed";

                CellArea area;
                area.StartRow = 1;
                area.EndRow = numRow;
                area.StartColumn = 0;
                area.EndColumn = 0;
                validation.AddArea(area);

                //BranchID
                validation = SheetPOSM.Validations[SheetPOSM.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                //validation.Formula1 = "=Master! $D$2:$D$" + (dtBranchID.Rows.Count + 1);
                validation.Formula1 = "=OFFSET(Master! $B$2,IFERROR(MATCH(A2,Master! $B$2:$B$" + (dtBranchID.Rows.Count + 1) + ",0)-1," + (dtBranchID.Rows.Count + 2) + "),1,IF(COUNTIF(Master! $B$2:$B$" + (dtBranchID.Rows.Count + 1) + ",A2)=0,1,COUNTIF(Master! $B$2:$B$" + (dtBranchID.Rows.Count + 1) + ",A2)),1)";
                validation.ShowError = true;
                validation.AlertStyle = ValidationAlertType.Stop;
                validation.ErrorTitle = "Error";
                validation.InputMessage = "Choose Branch";
                validation.ErrorMessage = "Branch isn't existed";

                area.StartRow = 1;
                area.EndRow = numRow;
                area.StartColumn = 1;
                area.EndColumn = 1;
                validation.AddArea(area);

                //InvtID
                validation = SheetPOSM.Validations[SheetPOSM.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=Master! $E$2:$E$" + (dtProduct.Rows.Count + 1);
                validation.ShowError = true;
                validation.AlertStyle = ValidationAlertType.Stop;
                validation.ErrorTitle = "Error";
                validation.InputMessage = "Choose Invt";
                validation.ErrorMessage = "Invt isn't existed";

                area.StartRow = 1;
                area.EndRow = numRow;
                area.StartColumn = 2;
                area.EndColumn = 2;
                validation.AddArea(area);

                //Value Branch
                //string vlkBranchID = @"VLOOKUP(B2,OFFSET(Master! $B$2,IFERROR(MATCH(A2,Master! $B$2:$B$" + (dtBranchID.Rows.Count + 1) + ",0)-1," + (dtBranchID.Rows.Count + 1) + "),1,IF(COUNTIF(Master! $B$2:$B$" + (dtBranchID.Rows.Count + 1) + ",A2)=0,1,COUNTIF(Master! $B$2:$B$" + (dtBranchID.Rows.Count + 1) + ",A2)),2),2,FALSE)";
                //String formularBranchID = "=IFERROR(" + vlkBranchID + ",\"\")";
                //SheetPOSM.Cells["X2"].SetSharedFormula(formularBranchID, numRow, 1);

                //Value InvtName
                String formularInvtName = "=IF(ISERROR(VLOOKUP(C2,Master! $E$2:$F$" + (dtProduct.Rows.Count + 1) + ",2,0)),\"\",VLOOKUP(C2,Master! $E$2:$F$" + (dtProduct.Rows.Count + 1) + ",2,0))";
                SheetPOSM.Cells["D2"].SetSharedFormula(formularInvtName, numRow, 1);

                SheetPOSM.Cells.Columns[0].Width = 30;
                SheetPOSM.Cells.Columns[1].Width = 30;
                SheetPOSM.Cells.Columns[2].Width = 30;
                SheetPOSM.Cells.Columns[3].Width = 30;
                SheetPOSM.Cells.Columns[4].Width = 15;
                SetCellValueGrid(SheetPOSM.Cells["A1"], Util.GetLang("POSMID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetPOSM.Cells["B1"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetPOSM.Cells["C1"], Util.GetLang("PosmCode"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetPOSM.Cells["D1"], Util.GetLang("PosmName"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetPOSM.Cells["E1"], Util.GetLang("Qty"), TextAlignmentType.Center, TextAlignmentType.Left);

                var style1 = SheetPOSM.Cells["X1"].GetStyle();
                style1.Font.Color = Color.White;
                var range1 = SheetPOSM.Cells.CreateRange("X1", "X" + numRow);
                range1.SetStyle(style1);

                SheetDataMaster.IsVisible = false;
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "IN22000_Template.xlsx" };
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        private void SetCellValueGrid(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.IsBold = true;
            style.Font.Size = 10;
            style.Font.Color = Color.Black;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
            
        }

        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                
                string message = string.Empty;
                string errorPOSMID = string.Empty;
                string errorBranchID = string.Empty;
                string errorInvtID = string.Empty;
                string errorPOSMIDnotExists = string.Empty;
                string errorBranchIDnotExists = string.Empty;
                string errorInvtIDnotExists = string.Empty;
                string errorQty = string.Empty;
                string errorQtyNotInput = string.Empty;
                string errorQtyFormat = string.Empty;
                string errorDuplicate = string.Empty;
                string errorDuplicateDB = string.Empty;

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        string POSMID = string.Empty;
                        string BranchID = string.Empty;
                        string InvtID = string.Empty;
                        string Qty = string.Empty;
                        
                        var lstIN_POSMCust = new List<IN_POSMCust>();
                        var lstPOSMID = _db.IN22000_pdPOSMImport(Current.UserName, Current.CpnyID).ToList();
                        var lstBranchID = _db.IN22000_pdBranchIDImport(Current.UserName).ToList();
                        var lstProduct = _db.IN22000_pdPosmProductImport(Current.UserName).ToList();

                        for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                        {
                            bool FlagCheck = false;
                            POSMID = workSheet.Cells[i, 0].StringValue.PassNull();
                            //BranchID = workSheet.Cells[i, 23].StringValue.PassNull();
                            BranchID = workSheet.Cells[i, 1].StringValue.PassNull();
                            InvtID = workSheet.Cells[i, 2].StringValue.PassNull();
                            Qty = workSheet.Cells[i, 4].StringValue.PassNull();

                            if (POSMID == "" && BranchID == "" && InvtID == "" && Qty == "")
                            {
                                continue;
                            }

                            if (POSMID == "")
                            {
                                errorPOSMID += (i + 1).ToString() + ",";
                                FlagCheck = true;
                            }
                            else
                            {
                                if (lstPOSMID.FirstOrDefault(p => p.POSMID == POSMID) == null)
                                {
                                    errorPOSMIDnotExists += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                            }

                            if (BranchID == "")
                            {
                                errorBranchID += (i + 1).ToString() + ",";
                                FlagCheck = true;
                            }
                            else
                            {
                                if (lstBranchID.FirstOrDefault(p => p.POSMID == POSMID && p.BranchID == BranchID) == null)
                                {
                                    errorBranchIDnotExists += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                            }

                            if (InvtID == "")
                            {
                                errorInvtID += (i + 1).ToString() + ",";
                                FlagCheck = true;
                            }
                            else
                            {
                                if (lstProduct.FirstOrDefault(p => p.InvtID == InvtID) == null)
                                {
                                    errorInvtIDnotExists += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                            }

                            if (Qty == "")
                            {
                                errorQty += (i + 1).ToString() + ",";
                                FlagCheck = true;
                            }
                            else
                            {
                                float n;
                                bool isNumeric = float.TryParse(Qty, out n);
                                if (isNumeric == true)
                                {
                                    if (n == 0 || n < 0)
                                    {
                                        errorQtyNotInput += (i + 1).ToString() + ",";
                                        FlagCheck = true;
                                    }
                                }
                                else
                                {
                                    errorQtyFormat += (i + 1).ToString() + ",";
                                    FlagCheck = true;
                                }
                            }

                            if (FlagCheck == true)
                            {
                                continue;
                            }

                            var recordExists = lstIN_POSMCust.FirstOrDefault(p=>p.PosmID == POSMID && p.BranchID == BranchID && p.PosmCode == InvtID && p.CustID == ".");
                            if (recordExists == null)
                            {
                                var record = _db.IN_POSMCust.FirstOrDefault(p=>p.PosmID == POSMID && p.BranchID == BranchID && p.PosmCode == InvtID && p.CustID == ".");
                                if (record == null)
                                {
                                    record = new IN_POSMCust();
                                    record.ResetET();
                                    record.PosmID = POSMID;
                                    record.BranchID = BranchID;
                                    record.PosmCode = InvtID;
                                    record.CustID = ".";
                                    record.SlsperID = "";
                                    record.Status = _beginStatus;
                                    record.IsAgree = false;
                                    record.Qty = Convert.ToInt32(Qty);
                                    record.AppQty = Convert.ToInt32(Qty);
                                    record.Crtd_DateTime = DateTime.Now;
                                    record.Crtd_Prog = _screenNbr;
                                    record.Crtd_User = _userName;
                                    record.LUpd_DateTime = DateTime.Now;
                                    record.LUpd_Prog = _screenNbr;
                                    record.LUpd_User = _userName;

                                    _db.IN_POSMCust.AddObject(record);
                                    lstIN_POSMCust.Add(record);
                                }
                                else
                                {
                                    if (record.Status != _beginStatus)
                                    {
                                        errorDuplicateDB += (i + 1).ToString() + ",";
                                    }
                                    else
                                    {
                                        record.Qty = Convert.ToInt32(Qty);
                                        record.AppQty = Convert.ToInt32(Qty);
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = _userName;
                                    }
                                }
                            }
                            else
                            {
                                errorDuplicate += (i + 1).ToString() + ",";
                            }
                        }

                        message = errorPOSMID == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", Util.GetLang("POSMID"), errorPOSMID);
                        message += errorBranchID == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", Util.GetLang("BranchID"), errorBranchID);
                        message += errorInvtID == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", Util.GetLang("PosmCode"), errorInvtID);
                        message += errorQty == "" ? "" : string.Format("{0} dòng: {1} chưa điền</br>", Util.GetLang("Qty"), errorQty);
                        message += errorPOSMIDnotExists == "" ? "" : string.Format("{0} dòng: {1} không tồn tại</br>", Util.GetLang("POSMID"), errorPOSMIDnotExists);
                        message += errorBranchIDnotExists == "" ? "" : string.Format("{0} dòng: {1} không tồn tại</br>", Util.GetLang("BranchID"), errorBranchIDnotExists);
                        message += errorInvtIDnotExists == "" ? "" : string.Format("{0} dòng: {1} không tồn tại</br>", Util.GetLang("PosmCode"), errorInvtIDnotExists);
                        message += errorQtyNotInput == "" ? "" : string.Format("{0} dòng: {1} phải lớn hơn 0</br>", Util.GetLang("Qty"), errorQtyNotInput);
                        message += errorQtyFormat == "" ? "" : string.Format("{0} dòng: {1} không đúng định dạng kiểu số</br>", Util.GetLang("Qty"), errorQtyFormat);
                        message += errorDuplicate == "" ? "" : string.Format("Dòng: {0} đã trùng lắp trong file Import</br>", errorDuplicate);
                        message += errorDuplicateDB == "" ? "" : string.Format("Dòng: {0} khác trạng thái chờ xét duyệt không thể import</br>", errorDuplicateDB);
                        
                        if (message == "" || message == string.Empty)
                        {
                            _db.SaveChanges();
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                        
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

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
using System.IO;
using System.Text;
using System.Data.Objects;
using System.Data.SqlClient;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using HQFramework.Common;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace OM23600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23600Controller : Controller
    {
        private string _screenNbr = "OM23600";
        private string _userName = Current.UserName;
        OM23600Entities _db = Util.CreateObjectContext<OM23600Entities>(false);
        private static readonly Regex boxNumberRegex = new Regex(@"^\d{2}/\d{4}$");
        private static readonly Regex boxNumberRegex1 = new Regex(@"^\d/\d{4}$");
        private JsonResult _logMessage;
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
        public ActionResult GetData(string posmID, string progType)
        {
            var lstPOSM = _db.OM23600_pgPosmID(posmID, progType, Current.UserName, Current.CpnyID, Current.LangID);
            return this.Store(lstPOSM);
        }        
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string posmID = data["cboPosmID"].PassNull();
                string progType = data["cboProgID"].PassNull();
                // Save Header
                var objH = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID.ToUpper() == posmID.ToUpper());
                if (objH != null)
                {
                    if (objH.ProgType != progType)
                    {
                        objH.ProgType = progType;
                        objH.LUpd_DateTime = DateTime.Now;
                        objH.LUpd_Prog = _screenNbr;
                        objH.LUpd_User = Current.UserName;
                        objH.tstamp = new byte[1];
                    }                    
                }
                else
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<OM23600_pgPosmID_Result> lstData = dataHandler.BatchObjectData<OM23600_pgPosmID_Result>();

                lstData.Created.AddRange(lstData.Updated);
                foreach (OM23600_pgPosmID_Result del in lstData.Deleted)
                {
                    DateTime dt = del.Date.ToDateShort();
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstData.Created.Where(p => 
                        p.BranchID == del.BranchID && 
                        p.CustId == del.CustId && 
                        p.ClassID == del.ClassID &&
                        p.SiteID == del.SiteID && 
                        p.InvtID == del.InvtID &&
                        p.Date.ToDateShort() == dt).Count() > 0)
                    {
                        lstData.Created.Where(p => 
                            p.BranchID == del.BranchID && 
                            p.CustId == del.CustId &&
                            p.ClassID == del.ClassID &&
                            p.SiteID == del.SiteID && 
                            p.InvtID == del.InvtID &&
                            p.Date.ToDateShort() == dt).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        // Xoá dữ liệu
                        var objDel = _db.OM_POSMBranch.ToList().Where(p => 
                            p.PosmID == posmID && 
                            p.BranchID == del.BranchID && 
                            p.CustId == del.CustId &&
                            p.ClassID == del.ClassID &&
                            p.SiteID == del.SiteID && 
                            p.InvtID == del.InvtID &&
                            p.Date.ToDateShort() == dt).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_POSMBranch.DeleteObject(objDel);
                        }
                    }
                }
                // Add or update 
                foreach (OM23600_pgPosmID_Result curItem in lstData.Created)
                {
                    if (curItem.BranchID.PassNull() == ""
                        && curItem.CustId.PassNull() == ""
                        && curItem.SiteID.PassNull() == ""
                        && curItem.ClassID.PassNull() == "" 
                        && (progType == "D4" || 
                            progType == "D3" && curItem.InvtID.PassNull() == "" && curItem.Date.Year == 1))
                    {
                        continue;
                    }

                    var objBranch = _db.OM_POSMBranch.Where(p => p.PosmID == posmID 
                        && p.BranchID == curItem.BranchID 
                        && p.CustId == curItem.CustId
                        && p.SiteID == curItem.SiteID
                        && p.InvtID == curItem.InvtID).FirstOrDefault();

                    if (objBranch != null)
                    {
                        // Update record
                        if (objBranch.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_OM_POSMBranch(objBranch, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        // Add new
                        objBranch = new OM_POSMBranch();
                        objBranch.PosmID = posmID;
                        objBranch.InvtID = progType == "D3" ? curItem.InvtID : "*";
                        objBranch.Date = progType == "D3" ? curItem.Date.ToDateTime() : new DateTime(1900, 1, 1);
                        objBranch.ProgType = progType;
                        Update_OM_POSMBranch(objBranch, curItem, true);
                        _db.OM_POSMBranch.AddObject(objBranch);
                    }
                }
                _db.SaveChanges();

                return Util.CreateMessage(MessageProcess.Save);
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
            }
        }

        private void Update_OM_POSMBranch(OM_POSMBranch t, OM23600_pgPosmID_Result s, bool isNew)
        {
            if (isNew)
            {
                t.BranchID = s.BranchID;
                t.CustId = s.CustId;                
                t.ClassID = s.ClassID;
                t.SiteID = s.SiteID;
                //t.InvtID = s.InvtID;
                //t.Date = s.Date.ToDateTime();   
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CustName = s.CustName;
            t.Descr = s.Descr;                    
            //t.Date = DateTime.TryParseExact("", Current.FormatDate);//,/ s.Date.ToDateTime();
            t.Qty = s.Qty;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        #region -Export- 
        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
                string posmID = data["cboPosmID"].PassNull();
                string format = Current.FormatDate;// WebConfigurationManager.AppSettings["FormatDate"].PassNull();
                if (string.IsNullOrWhiteSpace(format))
                {
                    format = "dd-MM-yyyy";
                }
                string fromDate = DateTime.Parse(data["dteFromDate"]).ToString(format);
                string toDate = DateTime.Parse(data["dteToDate"]).ToString(format);
                string progType = data["cboProgID"].PassNull();

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet sheetDet = workbook.Worksheets[0];
                sheetDet.Name = Util.GetLang("OM23600");
                int numMaxRow = 10010;
                Range range;
                Cell cell;
                DataAccess dal = Util.Dal();
                Validation validation;
                CellArea area;
                #region -Export data-
                                
                //Branch
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@PosmID", DbType.String, clsCommon.GetValueDBNull(posmID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtBranch = dal.ExecDataTable("OM23600_pcBranchID", CommandType.StoredProcedure, ref pc);
                int iRow = 1;
                for (int i = 0; i < dtBranch.Rows.Count; i++)
                {
                    cell = sheetDet.Cells["AA" + iRow];
                    cell.PutValue(dtBranch.Rows[i]["BranchID"].ToString());
                    cell = sheetDet.Cells["AB" + iRow];
                    cell.PutValue(dtBranch.Rows[i]["CpnyName"].ToString());
                    iRow++;
                }
                // CustID
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@PosmID", DbType.String, clsCommon.GetValueDBNull(posmID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtCust = dal.ExecDataTable("OM23600_pdExpCustId", CommandType.StoredProcedure, ref pc);
                iRow = 1;
                for (int i = 0; i < dtCust.Rows.Count; i++)
                {
                    cell = sheetDet.Cells["AD" + iRow];
                    cell.PutValue(dtCust.Rows[i]["CustId"].ToString());                   
                    cell = sheetDet.Cells["AE" + iRow];
                    cell.PutValue(dtCust.Rows[i]["BranchID"].ToString() + dtCust.Rows[i]["CustId"].ToString());
                    cell = sheetDet.Cells["AF" + iRow];
                    cell.PutValue(dtCust.Rows[i]["CustName"].ToString());
                    iRow++;
                }
                // Class ID
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtClass = dal.ExecDataTable("OM23600_pcClassID", CommandType.StoredProcedure, ref pc);
                iRow = 1;
                for (int i = 0; i < dtClass.Rows.Count; i++)
                {
                    cell = sheetDet.Cells["AH" + iRow];
                    cell.PutValue(dtClass.Rows[i]["ClassID"].ToString());
                    cell = sheetDet.Cells["AI" + iRow];
                    cell.PutValue(dtClass.Rows[i]["Descr"].ToString());
                    iRow++;
                }
                // Site
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@PosmID", DbType.String, clsCommon.GetValueDBNull(posmID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtSite = dal.ExecDataTable("OM23600_pdExpSiteID", CommandType.StoredProcedure, ref pc);
                iRow = 1;
                for (int i = 0; i < dtSite.Rows.Count; i++)
                {
                    cell = sheetDet.Cells["AK" + iRow];
                    cell.PutValue(dtSite.Rows[i]["SiteId"].ToString());
                    cell = sheetDet.Cells["AL" + iRow];
                    cell.PutValue(dtSite.Rows[i]["Name"].ToString());
                    iRow++;
                }
                
                #endregion
                
                #region -Header++-

                this.SetRequireCellValueHeader(sheetDet, "C1", Util.GetLang("POSMID"));
                this.SetCellValue(sheetDet, "D1", posmID);
                this.SetRequireCellValueHeader(sheetDet, "B2", Util.GetLang("FromDate"));
                this.SetCellValue(sheetDet, "C2", fromDate);
                this.SetRequireCellValueHeader(sheetDet, "D2", Util.GetLang("ToDate"));
                this.SetCellValue(sheetDet, "E2", toDate);

                this.SetCellValueHeader(sheetDet, "A3", Util.GetLang("BranchID"));
                this.SetCellValueHeader(sheetDet, "B3", Util.GetLang("CustID"));
                this.SetCellValueHeader(sheetDet, "C3", Util.GetLang("CustName"));
                this.SetCellValueHeader(sheetDet, "D3", Util.GetLang("ClassID"));
                this.SetCellValueHeader(sheetDet, "E3", Util.GetLang("SiteId"));
                var style = sheetDet.Cells["A4"].GetStyle();
                if (progType == "D3")
                {
                    this.SetCellValueHeader(sheetDet, "F3", Util.GetLang("InvtID"));
                    this.SetCellValueHeader(sheetDet, "G3", Util.GetLang("InvtName"));
                    this.SetCellValueHeader(sheetDet, "H3", Util.GetLang("ExpDate") + " (MM/yyyy)");
                    this.SetCellValueHeader(sheetDet, "I3", Util.GetLang("Qty"));

                    // InvtID
                    pc = new ParamCollection();
                    pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                    DataTable dtInvt = dal.ExecDataTable("OM23600_pdExpInvtID", CommandType.StoredProcedure, ref pc);
                    iRow = 1;
                    for (int i = 0; i < dtInvt.Rows.Count; i++)
                    {                        
                        cell = sheetDet.Cells["AN" + iRow];
                        cell.PutValue(dtInvt.Rows[i]["InvtID"].ToString());
                        cell = sheetDet.Cells["AO" + iRow];
                        cell.PutValue(dtInvt.Rows[i]["Descr"].ToString());
                        cell = sheetDet.Cells["AP" + iRow];
                        cell.PutValue(dtInvt.Rows[i]["ClassID"].ToString());
                        iRow++;
                    }
                    // ExpDate
                    pc = new ParamCollection();
                    pc.Add(new ParamStruct("@PosmID", DbType.String, clsCommon.GetValueDBNull(posmID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                    //DataTable dtExpDate = dal.ExecDataTable("OM23600_pdExpExpDate4Excel", CommandType.StoredProcedure, ref pc);
                    //iRow = 1;
                    //for (int i = 0; i < dtExpDate.Rows.Count; i++)
                    //{
                    //    cell = sheetDet.Cells["AR" + iRow];
                    //    cell.PutValue(dtExpDate.Rows[i]["ExportDate"].ToString());
                    //    //cell = sheetDet.Cells["AS" + iRow];
                    //    //cell.PutValue(dtExpDate.Rows[i]["InvtID"].ToString());
                    //    //cell = sheetDet.Cells["AT" + iRow];
                    //    //cell.PutValue(dtExpDate.Rows[i]["SiteID"].ToString());
                    //    iRow++;
                    //}                    

                    // InvtID
                    validation = sheetDet.Validations[sheetDet.Validations.Add()];
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.Operator = OperatorType.Between;
                    validation.InCellDropDown = true;
                    validation.Formula1 = "=$AN$1:$AN$" + (dtInvt.Rows.Count);
                    validation.AlertStyle = ValidationAlertType.Stop;

                    area.StartRow = 3;
                    area.EndRow = numMaxRow;
                    area.StartColumn = 5;
                    area.EndColumn = 5;
                    validation.AddArea(area);
                    //// ExpDate
                    //validation = sheetDet.Validations[sheetDet.Validations.Add()];
                    //validation.Type = Aspose.Cells.ValidationType.Date;
                    //validation.Operator = OperatorType.None;
                    //validation.InCellDropDown = true;
                    ////validation.Formula1 = "7";
                    //validation.AlertStyle = ValidationAlertType.Stop;
                    //validation.ErrorTitle = "Invalid Entry";
                    
                    //validation.ErrorMessage = "Bạn phải nhập theo định dạng MM/yyyy!";

                    //area.StartRow = 3;
                    //area.EndRow = numMaxRow;
                    //area.StartColumn = 7;
                    //area.EndColumn = 7;
                    //validation.AddArea(area);

                    //style = sheetDet.Cells["H4"].GetStyle();
                    //style.Number = 17;
                    //range = sheetDet.Cells.CreateRange("H4", "H" + numMaxRow);
                    //range.SetStyle(style);


                    validation = sheetDet.Validations[sheetDet.Validations.Add()];
                    validation.Type = Aspose.Cells.ValidationType.WholeNumber;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = "0.0";
                    validation.Formula2 = float.MaxValue.ToString();
                    validation.ShowError = true;
                    validation.AlertStyle = ValidationAlertType.Stop;
                    validation.ErrorTitle = "Invalid Entry";
                    validation.ErrorMessage = "Bạn phải nhập số nguyên dương!";

                    area.StartRow = 3;
                    area.EndRow = numMaxRow;
                    area.StartColumn = 8;
                    area.EndColumn = 8;
                    validation.AddArea(area);

                    sheetDet.Cells["G4"].SetSharedFormula("=IF(ISERROR(VLOOKUP(F4,AN:AO,2,0)),\"\",VLOOKUP(F4,AN:AO,2,0))", numMaxRow, 1);

                    style.IsLocked = false;
                    style.Number = 49;
                    range = sheetDet.Cells.CreateRange("A4", "I" + numMaxRow);
                    range.SetStyle(style);

                    style = sheetDet.Cells["G4"].GetStyle();
                    style.IsLocked = true;
                    range = sheetDet.Cells.CreateRange("G4", "G" + numMaxRow);
                    range.SetStyle(style);
                    style = sheetDet.Cells["I4"].GetStyle();
                    style.Number = 3;
                    range = sheetDet.Cells.CreateRange("I4", "I" + numMaxRow);
                    range.SetStyle(style);
                }
                else
                {
                    this.SetCellValueHeader(sheetDet, "F3", Util.GetLang("Qty"));

                    validation = sheetDet.Validations[sheetDet.Validations.Add()];
                    validation.Type = Aspose.Cells.ValidationType.WholeNumber;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = "0.0";
                    validation.Formula2 = float.MaxValue.ToString();
                    validation.ShowError = true;
                    validation.AlertStyle = ValidationAlertType.Stop;
                    validation.ErrorTitle = "Invalid Entry";
                    validation.ErrorMessage = "Bạn phải nhập số nguyên dương!";

                    area.StartRow = 3;
                    area.EndRow = numMaxRow;
                    area.StartColumn = 5;
                    area.EndColumn = 5;
                    validation.AddArea(area);

                    style.IsLocked = false;
                    style.Number = 49;
                    range = sheetDet.Cells.CreateRange("A4", "F" + numMaxRow);
                    range.SetStyle(style);

                    style = sheetDet.Cells["F4"].GetStyle();
                    style.Number = 3;
                    range = sheetDet.Cells.CreateRange("F4", "F" + numMaxRow);
                    range.SetStyle(style);
                }

                style = sheetDet.Cells["C4"].GetStyle();
                style.IsLocked = true;
                range = sheetDet.Cells.CreateRange("C4", "C" + numMaxRow);
                range.SetStyle(style);
                #endregion

                #region -Fomular-
                // BranchID
                validation = sheetDet.Validations[sheetDet.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=$AA$1:$AA$" + (dtBranch.Rows.Count);
                validation.AlertStyle = ValidationAlertType.Stop;
               
                area.StartRow = 3;
                area.EndRow = numMaxRow;
                area.StartColumn = 0;
                area.EndColumn = 0;
                validation.AddArea(area);

                // CustID
                validation = sheetDet.Validations[sheetDet.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=$AD$1:$AD$" + (dtCust.Rows.Count);
                validation.AlertStyle = ValidationAlertType.Stop;

                area.StartRow = 3;
                area.EndRow = numMaxRow;
                area.StartColumn = 1;
                area.EndColumn = 1;
                validation.AddArea(area);

                // ClassID
                validation = sheetDet.Validations[sheetDet.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=$AH$1:$AH$" + (dtClass.Rows.Count);
                validation.AlertStyle = ValidationAlertType.Stop;

                area.StartRow = 3;
                area.EndRow = numMaxRow;
                area.StartColumn = 3;
                area.EndColumn = 3;
                validation.AddArea(area);

                // SiteID
                validation = sheetDet.Validations[sheetDet.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=$AK$1:$AK$" + (dtSite.Rows.Count);
                validation.AlertStyle = ValidationAlertType.Stop;

                area.StartRow = 3;
                area.EndRow = numMaxRow;
                area.StartColumn = 4;
                area.EndColumn = 4;
                validation.AddArea(area);

                sheetDet.Cells["C4"].SetSharedFormula("=IF(ISERROR(VLOOKUP(CONCATENATE(A4,B4),AE:AF,2,0)),\"\",VLOOKUP(CONCATENATE(A4,B4),AE:AF,2,0))", numMaxRow, 1);

                #endregion

                sheetDet.AutoFitColumns();
                sheetDet.Cells.SetColumnWidth(0, 15);
                sheetDet.Cells.SetColumnWidth(1, 15);
                sheetDet.Cells.SetColumnWidth(2, 25);
                sheetDet.Cells.SetColumnWidth(3, 15);
                sheetDet.Cells.SetColumnWidth(4, 15);
                sheetDet.Cells.SetColumnWidth(5, 15);
                if (progType == "D3")
                {
                    sheetDet.Cells.SetColumnWidth(6, 30);
                    sheetDet.Cells.SetColumnWidth(8, 15);
                }
                sheetDet.Protect(ProtectionType.All, "HQP@ssw0rd", "HQP@ssw0rd");
                workbook.Save(stream, SaveFormat.Excel97To2003);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = sheetDet.Name + "_" + DateTime.Now.ToString("yyyyMMdd HHmmss") + ".xls" };
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

        private void SetRequireCellValueHeader(Worksheet sheet, string cell, object value)
        {
            var style = sheet.Cells[cell].GetStyle();

            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.Font.IsBold = true;
            style.Font.Size = 10;
            style.Pattern = BackgroundType.Solid;
            //style.ForegroundColor = Color.Yellow;
            style.Font.Color = Color.DarkBlue;

            sheet.Cells[cell].SetStyle(style);
            sheet.Cells[cell].PutValue(value);
            sheet.Cells.SetRowHeight(0, 22);
        }

        private void SetCellValueHeader(Worksheet sheet, string cell, object value)
        {
            var style = sheet.Cells[cell].GetStyle();

            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.Font.IsBold = true;
            style.Font.Size = 10;
            style.Pattern = BackgroundType.Solid;
            style.ForegroundColor = Color.Yellow;
            style.Font.Color = Color.Blue;

            sheet.Cells[cell].SetStyle(style);
            sheet.Cells[cell].PutValue(value);
            sheet.Cells.SetRowHeight(0, 20);
        }
        private void SetCellValue(Worksheet sheet, string cell, object value)
        {
            var style = sheet.Cells[cell].GetStyle();

            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.Font.IsBold = true;
            style.Font.Size = 10;
            //style.Pattern = BackgroundType.Solid;
            //style.ForegroundColor = Color.Yellow;

            sheet.Cells[cell].SetStyle(style);
            sheet.Cells[cell].PutValue(value);
            //sheet.Cells.SetRowHeight(0, 20);
        }

        #endregion

        #region -Import-                
        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {                
                string posmID = data["cboPosmID"].PassNull();
                string progType = data["cboProgID"].PassNull();
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            #region -Table OM_PriceClassTmp-
                            System.Data.DataTable dtOM_DetailsTmp = new System.Data.DataTable() { TableName = "OM_POSMBranch_tmp" };
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "PosmID" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "BranchID" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "CustId" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "CustName" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "ClassID" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "SiteID" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "InvtID" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "Descr" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "Date" });
                            dtOM_DetailsTmp.Columns.Add(new DataColumn() { ColumnName = "Qty" });

                            #endregion
                            DataRow dtRowDetails;
                            List<string> lstDetails = new List<string>();
                            string BranchID = string.Empty;                            
                            string CustID = string.Empty;
                            string CustName = string.Empty;
                            string ClassID = string.Empty;
                            string SiteID = string.Empty;
                            double Qty = 0.0;
                            string errorRows = string.Empty;                            
                            Worksheet workSheet = workbook.Worksheets[0];
                            var impPosmID = workSheet.Cells[0, 3].StringValue.PassNull().Trim();
                            if (impPosmID.ToUpper() != posmID.ToUpper())
                            {
                                var obj = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID.ToUpper() == impPosmID.ToUpper());
                                if (obj != null)
                                {
                                    posmID = impPosmID;
                                    if (!string.IsNullOrWhiteSpace(obj.ProgType))
                                    {
                                        progType = obj.ProgType;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(workSheet.Cells[2, 6].StringValue) &&
                                            !string.IsNullOrWhiteSpace(workSheet.Cells[2, 7].StringValue) &&
                                            !string.IsNullOrWhiteSpace(workSheet.Cells[2, 5].StringValue) &&
                                            !string.IsNullOrWhiteSpace(workSheet.Cells[2, 0].StringValue) &&
                                            !string.IsNullOrWhiteSpace(workSheet.Cells[2, 1].StringValue) &&
                                            !string.IsNullOrWhiteSpace(workSheet.Cells[2, 2].StringValue) &&
                                            !string.IsNullOrWhiteSpace(workSheet.Cells[2, 3].StringValue) &&
                                            !string.IsNullOrWhiteSpace(workSheet.Cells[2, 4].StringValue)
                                            )
                                        {
                                            progType = "D3";
                                        }
                                        else
                                        {
                                            progType = "D4";
                                        }
                                    }
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "2016071101", parm: new[] { impPosmID });
                                }
                            }
                            int maxDataRow = workSheet.Cells.MaxDataRow;
                            var lstBranch = _db.OM23600_pcBranchID(posmID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            var lstCust = _db.OM23600_pdExpCustId(Current.UserName, Current.CpnyID, Current.LangID, posmID).ToList();
                            var lstClass = _db.OM23600_pcClassID(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            var lstSite = _db.OM23600_pdExpSiteID(Current.UserName, Current.CpnyID, Current.LangID, posmID).ToList();

                            string dateFormat = Util.GetLang("OM23600DateFormat");
                            string column = " (" + Util.GetLang("Column") + ": ";
                            if (progType == "D3")
                            {
                                var lstInvt = _db.OM23600_pdExpInvtID(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                                if (string.IsNullOrWhiteSpace(workSheet.Cells[2, 6].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 7].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 8].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 0].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 1].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 2].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 3].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 4].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 5].StringValue) 
                                    )
                                {
                                    throw new MessageException(MessageType.Message, "20407");
                                }
                                string InvtID = string.Empty;
                                string InvtDescr = string.Empty;
                                string ExpDate = string.Empty;
                                DateTime dtExp;
                                for (int i = 3; i <= maxDataRow; i++)
                                {
                                    BranchID = workSheet.Cells[i, 0].StringValue.PassNull().Trim();                                   
                                    CustID = workSheet.Cells[i, 1].StringValue.PassNull().Trim();
                                   // CustName = workSheet.Cells[i, 2].StringValue.PassNull().Trim();                                    
                                    ClassID = workSheet.Cells[i, 3].StringValue.PassNull().Trim();
                                    SiteID = workSheet.Cells[i, 4].StringValue.PassNull().Trim();
                                    InvtID = workSheet.Cells[i, 5].StringValue.PassNull().Trim();
                                    //InvtDescr = workSheet.Cells[i, 6].StringValue.PassNull().Trim();
                                    ExpDate = workSheet.Cells[i, 7].StringValue.PassNull().Trim();
                                    
                                    #region -Validate data-
                                    if (BranchID == string.Empty &&
                                        CustID == string.Empty && 
                                        ClassID == string.Empty &&
                                        SiteID == string.Empty &&
                                        InvtID == string.Empty &&
                                        ExpDate == string.Empty)
                                    {
                                        continue;
                                    }
                                    bool isValidVal = double.TryParse(workSheet.Cells[i, 8].StringValue.PassNull(), out Qty);
                                    if (!isValidVal)
                                    {
                                        Qty = 0;
                                    }
                                    //Qty = double.Parse(workSheet.Cells[i, 8].StringValue.PassNull());
                                    // Check exist company
                                    var objBranch = lstBranch.Where(x => x.BranchID == BranchID).FirstOrDefault();
                                    if (objBranch == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("BranchID") + ")" });
                                    }
                                    // Check exist customer
                                    var objCust = lstCust.Where(x =>
                                                    x.BranchID == BranchID
                                                    && x.CustId == CustID).FirstOrDefault();
                                    if (objCust == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("CustID") + ")" });
                                    }                                                                                   
                                    // Check exist class
                                    var objClass = lstClass.Where(x => x.ClassID == ClassID).FirstOrDefault();
                                    if (objClass == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("ClassID") + ")" });
                                    }
                                    // Check exist siteID
                                    var objSite = lstSite.Where(x => x.SiteId == SiteID).FirstOrDefault();
                                    if (objSite == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("SiteId") + ")" });
                                    }
                                    // Check exist invtID
                                    var objInvt = lstInvt.Where(x => x.InvtID == InvtID).FirstOrDefault();
                                    if (objInvt == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("InvtID") + ")" });
                                    }
                                    // Check exist ExpDate                                   

                                    if (!VerifyBoxNumber(ExpDate))
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("ExpDate") + " " + dateFormat + ")" });
                                    }
                                    else
                                    {
                                        string[] val = ExpDate.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (val.Length > 2 || 
                                            int.Parse(val[0]) < 0 || 
                                            int.Parse(val[0]) > 12 ||
                                            int.Parse(val[1]) <= 1900)
                                        {
                                            throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("ExpDate") + ")" });
                                        }
                                        dtExp = new DateTime(int.Parse(val[1]), int.Parse(val[0]), DateTime.DaysInMonth(int.Parse(val[1]), int.Parse(val[0])));
                                    }
                                    #endregion

                                    #region -Get data from excel-

                                    if (!lstDetails.Contains(posmID + BranchID + CustID + ClassID + SiteID + InvtID + ExpDate))
                                    {
                                        lstDetails.Add(posmID + BranchID + CustID + ClassID + SiteID + InvtID + ExpDate);
                                        dtRowDetails = dtOM_DetailsTmp.NewRow();
                                        dtRowDetails["PosmID"] = posmID;
                                        dtRowDetails["BranchID"] = BranchID;
                                        dtRowDetails["CustId"] = CustID;
                                        dtRowDetails["CustName"] = objCust.CustName;

                                        dtRowDetails["ClassID"] = ClassID;
                                        dtRowDetails["SiteID"] = SiteID;
                                        dtRowDetails["InvtID"] = objInvt.InvtID;
                                        dtRowDetails["Descr"] = objInvt.Descr;

                                        dtRowDetails["Date"] = dtExp;
                                        dtRowDetails["Qty"] = Qty;
                                        dtOM_DetailsTmp.Rows.Add(dtRowDetails);
                                    }
                                    #endregion
                                }// End for loop
                            }
                            else // ProgType = "D4"
                            {
                                if ( 
                                    !string.IsNullOrWhiteSpace(workSheet.Cells[2, 6].StringValue) ||
                                    !string.IsNullOrWhiteSpace(workSheet.Cells[2, 7].StringValue) ||
                                    !string.IsNullOrWhiteSpace(workSheet.Cells[2, 8].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 0].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 1].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 2].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 3].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 4].StringValue) ||
                                    string.IsNullOrWhiteSpace(workSheet.Cells[2, 5].StringValue) )
                                {
                                    throw new MessageException(MessageType.Message, "20407");
                                }
                                for (int i = 3; i <= maxDataRow; i++)
                                {
                                    BranchID = workSheet.Cells[i, 0].StringValue.PassNull().Trim();
                                    CustID = workSheet.Cells[i, 1].StringValue.PassNull().Trim();                                   
                                    ClassID = workSheet.Cells[i, 3].StringValue.PassNull().Trim();
                                    SiteID = workSheet.Cells[i, 4].StringValue.PassNull().Trim();

                                    #region -Validate data-
                                    if (BranchID == string.Empty &&
                                        CustID == string.Empty &&
                                        ClassID == string.Empty &&
                                        SiteID == string.Empty)
                                    {
                                        continue;
                                    }
                                   // Qty = double.Parse(workSheet.Cells[i, 5].StringValue.PassNull());
                                    bool isValidVal = double.TryParse(workSheet.Cells[i, 5].StringValue.PassNull(), out Qty);
                                    if (!isValidVal)
                                    {
                                        Qty = 0;
                                    }
                                    // Check exist company
                                    var objBranch = lstBranch.Where(x => x.BranchID == BranchID).FirstOrDefault();
                                    if (objBranch == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("BranchID") + ")" });
                                    }
                                    // Check exist customer
                                    var objCust = lstCust.Where(x =>
                                                    x.BranchID == BranchID
                                                    && x.CustId == CustID).FirstOrDefault();
                                    if (objCust == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("CustID") + ")" });
                                    }
                                    // Check exist class
                                    var objClass = lstClass.Where(x => x.ClassID == ClassID).FirstOrDefault();
                                    if (objClass == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("ClassID") + ")" });
                                    }
                                    // Check exist siteID
                                    var objSite = lstSite.Where(x => x.SiteId == SiteID).FirstOrDefault();
                                    if (objSite == null)
                                    {
                                        throw new MessageException(MessageType.Message, "201302071", parm: new[] { (i + 1).PassNull() + column + Util.GetLang("SiteId") + ")" });
                                    }
                                    
                                    #endregion

                                    #region -Get data from excel-

                                    if (!lstDetails.Contains(posmID + BranchID + CustID + ClassID + SiteID))
                                    {
                                        lstDetails.Add(posmID + BranchID + CustID + ClassID + SiteID);
                                        dtRowDetails = dtOM_DetailsTmp.NewRow();
                                        dtRowDetails["PosmID"] = posmID;
                                        dtRowDetails["BranchID"] = BranchID;
                                        dtRowDetails["CustId"] = CustID;
                                        dtRowDetails["CustName"] = objCust.CustName;

                                        dtRowDetails["ClassID"] = ClassID;
                                        dtRowDetails["SiteID"] = SiteID;
                                        dtRowDetails["InvtID"] = "*";
                                        dtRowDetails["Descr"] = "";

                                        dtRowDetails["Date"] = new DateTime(1900, 01, 01);
                                        dtRowDetails["Qty"] = Qty;
                                        dtOM_DetailsTmp.Rows.Add(dtRowDetails);
                                    }
                                    #endregion
                                }// End for loop
                            }
                            var dal = Util.Dal();
                            using (SqlConnection dbConnection = new SqlConnection(dal.ConnectionString))
                            {
                                dbConnection.Open();
                                using (SqlTransaction sqlTran = dbConnection.BeginTransaction())
                                {
                                    using (SqlBulkCopy s = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity, sqlTran))
                                    {
                                        try
                                        {
                                            // Insert vào bảng tạm OM_FCSHeader
                                            s.DestinationTableName = dtOM_DetailsTmp.TableName;
                                            foreach (var col in dtOM_DetailsTmp.Columns)
                                                s.ColumnMappings.Add(col.ToString(), col.ToString());
                                            s.WriteToServer(dtOM_DetailsTmp);
                                            ////Gọi store insert, update từ bảng tạm vào bảng chính
                                            SqlCommand cmd1 = new SqlCommand("OM23600_ppImportOM_POSMBranch", dbConnection, sqlTran);
                                            cmd1.CommandType = CommandType.StoredProcedure;
                                            cmd1.Parameters.AddWithValue("@UserID", Current.UserName);
                                            cmd1.Parameters.AddWithValue("@PosmID", posmID);
                                            cmd1.Parameters.AddWithValue("@ProgType", progType);

                                            cmd1.ExecuteScalar();
                                            sqlTran.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            sqlTran.Rollback();
                                            throw ex;
                                        }
                                    }
                                }
                            }
                        }
                        return Json(new { success = true, msgCode = 20121418, posmID = posmID, progType = progType });
                        //string isoJson = JsonConvert.SerializeObject(lstBudget, new IsoDateTimeConverter());
                        //return Json(new { success = true, type = "error", data = isoJson });
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
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
                return _logMessage;
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
        // Verify format
        public static bool VerifyBoxNumber(string boxNumber)
        {
            return (boxNumberRegex.IsMatch(boxNumber) || boxNumberRegex1.IsMatch(boxNumber));
        }
        #endregion
    }
}

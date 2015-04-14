using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Xml;
using System.Xml.Linq;
using System.Data.Objects;
using System.IO;
//using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Runtime.InteropServices;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
//using EasyXLS;
using System.Data;
using System.IO;
using System.Drawing;

using System.Globalization;
using System.Data.SqlClient;
using HQFramework.DAL;
using Aspose.Cells;
using HQFramework.Common;

namespace OM22101.Controllers
{  
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22101Controller : Controller
    {
        string screenNbr = "OM22101";
        OM22101Entities _db = Util.CreateObjectContext<OM22101Entities>(false);
        eSkySysEntities mDb = Util.CreateObjectContext<eSkySysEntities>(true);
        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = mDb.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM22101");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\TempImport");
                }
                return _filePath;
            }
        }
        public ActionResult Index()
        {
            
            return View();
        }
        private JsonResult _logMessage;
        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            //StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
            //ChangeRecords<ppv_OM22101_LoadGrid_Result> lstgrd = dataHandler1.BatchObjectData<ppv_OM22101_LoadGrid_Result>();

            //foreach (ppv_OM22101_LoadGrid_Result deleted in lstgrd.Deleted)
            //{
            //    var del = _db.POS_MasterData.Where(p =>  p.POSCode == deleted.POSCode).FirstOrDefault();
            //    if (del != null)
            //    {
            //        _db.POS_MasterData.DeleteObject(del);

            //    }
            //}
            //foreach (ppv_OM22101_LoadGrid_Result created in lstgrd.Created)
            //{
            //    var record = _db.POS_MasterData.Where(p => p.POSCode == created.POSCode).FirstOrDefault();

            //    if (created.tstamp.ToHex() == "")
            //    {
            //        if (record == null)
            //        {
            //            record = new POS_MasterData();
            //            record.POSCode = created.POSCode;

            //            record.Crtd_DateTime = DateTime.Now;
            //            record.Crtd_Prog = screenNbr;
            //            record.Crtd_User = Current.UserName;

            //            UpdatingPOS_MasterData(created, ref record);
            //            if ((record.SDSM != "" && record.POSCode != "") || (record.DSM != "" && record.POSCode != "") || 
            //                ((record.SDSM != "" && record.DSM != "" && record.POSCode != "")))
            //            {
            //                _db.POS_MasterData.AddObject(record);
            //            }
            //        }
            //        else
            //        {
            //            return Json(new
            //            {
            //                success = false,
            //                code = "151",
            //                colName = Util.GetLang("Type"),

            //            }, JsonRequestBehavior.AllowGet);
            //            //tra ve loi da ton tai ma ngon ngu nay ko the them
            //        }
            //    }
            //}

            //foreach (ppv_OM22101_LoadGrid_Result updated in lstgrd.Updated)
            //{
            //    var record = _db.POS_MasterData.Where(p =>  p.POSCode == updated.POSCode).FirstOrDefault();

            //    if (record != null)
            //    {
            //        if (record.tstamp.ToHex() != updated.tstamp.ToHex())
            //        {
            //            return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
            //        }
            //        UpdatingPOS_MasterData(updated, ref record);
            //    }
            //    else
            //    {
            //        if (updated.tstamp.ToHex() == "")
            //        {
            //            record = new POS_MasterData();
            //            record.AreaCode = updated.AreaCode;
            //            record.SDSM = updated.SDSMCode;
            //            record.DSM = updated.DSMCode;
            //            record.POSCode = updated.POSCode;
            //            record.Crtd_DateTime = DateTime.Now;
            //            record.Crtd_Prog = screenNbr;
            //            record.Crtd_User = Current.UserName;
            //            UpdatingPOS_MasterData(updated, ref record);
            //            if ((record.SDSM != "" && record.POSCode != "") || (record.DSM != "" && record.POSCode != "") ||
            //                ((record.SDSM != "" && record.DSM != "" && record.POSCode != "")))
            //            {
            //                _db.POS_MasterData.AddObject(record);
            //            }
            //        }
            //        else
            //        {
            //            return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
            //        }
            //    }

            //}
            //_db.SaveChanges();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult OM22101Import(FormCollection data)
        {
            try
            {
                var date = DateTime.Now.Date;
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("ImportTemplate");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);

                string messagestrEBranchID = string.Empty;
                string messagestrESlsperId = string.Empty;
                string messagestrECycleNbr = string.Empty;
                string messagestrEInvtID = string.Empty;
                string messagestrECnvFact = string.Empty;
                string messagestrEQty = string.Empty;

                string messageDate = string.Empty;
                string messageerror = string.Empty;
                string messageduplicate = string.Empty;
                string message = string.Empty;

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {

                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        string strCycleNbr = workSheet.Cells[1, 2].StringValue.PassNull().Trim();
                        string strBranchID = workSheet.Cells[2, 2].StringValue.PassNull().Trim();
                        string strSlsperId = "";
                        string strInvtID = "";
                        string strCnvFact = "";
                        string strQty = "";

                        for (int i = 7; i <= workSheet.Cells.MaxDataRow; i++)
                        {
                            strSlsperId = workSheet.Cells[i, 1].StringValue.PassNull().Trim();
                            strInvtID = workSheet.Cells[i, 4].StringValue.PassNull().Trim();
                            strCnvFact = workSheet.Cells[i, 6].StringValue.PassNull().Trim();
                            strQty = workSheet.Cells[i, 7].StringValue.PassNull().Trim();
                            if (strCycleNbr == "") continue;
                            else if (strBranchID == "" || strSlsperId == "" || strInvtID == "" || strCnvFact == "" || strQty=="")
                            {
                                if (strBranchID == "")
                                {
                                    messagestrEBranchID += (i + 1).ToString() + ",";
                                    continue;

                                }
                                if (strSlsperId == "")
                                {
                                    messagestrESlsperId += (i + 1).ToString() + ",";
                                    continue;
                                }
                                if (strInvtID == "")
                                {
                                    messagestrEInvtID += (i + 1).ToString() + ",";
                                    continue;

                                }
                                if (strCnvFact == "")
                                {
                                    messagestrECnvFact += (i + 1).ToString() + ",";
                                    continue;

                                }
                                if (strQty == "")
                                {
                                    messagestrEQty += (i + 1).ToString() + ",";
                                    continue;

                                }
                            }

                            else
                            {
                                var c = _db.OM_KPI_SKU.FirstOrDefault(l => l.BranchID == strBranchID && l.SlsperId == strSlsperId && l.CycleNbr == strCycleNbr && l.InvtID == strInvtID);
                                if (c == null)
                                {
                                    c = new OM_KPI_SKU();
                                    c.ResetET();
                                    c.CycleNbr = strCycleNbr.PassNull().Trim();
                                    c.BranchID = strBranchID.PassNull().Trim();
                                    c.SlsperId = strSlsperId.PassNull().Trim();
                                    c.InvtID = strInvtID.PassNull().Trim();
                                    c.CnvFact = double.Parse(strCnvFact);
                                    c.Qty = double.Parse(strQty);

                                    c.Crtd_DateTime = DateTime.Now;
                                    c.Crtd_Prog = screenNbr;
                                    c.Crtd_User = Current.UserName;
                                    c.LUpd_DateTime = DateTime.Now;
                                    c.LUpd_Prog = screenNbr;
                                    c.LUpd_User = Current.UserName;
                                    _db.OM_KPI_SKU.AddObject(c);
                                }
                                else
                                {
                                    c.CnvFact = double.Parse(strCnvFact);
                                    c.Qty = double.Parse(strQty);

                                    c.LUpd_DateTime = DateTime.Now;
                                    c.LUpd_Prog = screenNbr;
                                    c.LUpd_User = Current.UserName;
                                }
                            }
                        }

                        _db.SaveChanges();

                        message = messagestrECycleNbr == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrECycleNbr, workSheet.Cells[1, 1].StringValue);
                        message += messagestrEBranchID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrEBranchID, workSheet.Cells[2, 1].StringValue);
                        message += messagestrESlsperId == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrESlsperId, workSheet.Cells[5, 1].StringValue);
                        message += messagestrEInvtID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrEInvtID, workSheet.Cells[5, 4].StringValue);
                        message += messagestrECnvFact == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrECnvFact, workSheet.Cells[5, 6].StringValue);
                        message += messagestrEQty == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrEQty, workSheet.Cells[5, 7].StringValue);

                        message += messageerror == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ<br/>", messageerror);
                        message += messageduplicate == "" ? "" : string.Format("Dòng {0} dữ liệu bi trùng<br/>", messageduplicate);

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

                return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
            return _logMessage;
        }

        private string renameFileName(string oldfileName, string newName, bool isPlus)
        {
            var fileName = string.Empty;
            var fileNameNotExt = Path.GetFileNameWithoutExtension(oldfileName);
            var ext = Path.GetExtension(oldfileName);
            if (isPlus)
            {
                fileName = string.Format("{0}{1}{2}", fileNameNotExt, newName, ext);
            }
            else
            {
                fileName = string.Format("{0}{2}", fileNameNotExt, ext);
            }
            return fileName;
        }

        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
       
                string BranchID = data["BranchID"].PassNull().Trim();
                string BranchName = data["BranchName"].PassNull().Trim();
                string CycleNbr = data["CycleNbr"].PassNull().Trim();
                string StartDate = data["StartDate"].PassNull().Trim();
                string EndDate = data["EndDate"].PassNull().Trim();
                var headerRowIdx = 5;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetKPI = workbook.Worksheets[0];
                SheetKPI.Name = Util.GetLang("OM22101_KPI_SKU");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();

                #region header info
                // Title header
                SetCellValue(SheetKPI.Cells["A1"],
                    string.Format("{0} {1}", Util.GetLang("OM22101_ExportHeader") + "(" + BranchID + ")", (string.IsNullOrWhiteSpace(BranchID) ? string.Format("({0})", BranchID) : string.Empty)),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 20,true);
                SheetKPI.Cells.Merge(0,0,1,8);

                // Title info
                SetCellValue(SheetKPI.Cells["B2"], Util.GetLang("OM22100_Cycle"), TextAlignmentType.Center, TextAlignmentType.Right, true, 12, false);
                SetCellValue(SheetKPI.Cells["B3"], Util.GetLang("OM22100_BranchID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 12, false);
                SetCellValue(SheetKPI.Cells["B4"], Util.GetLang("OM22100_BranchName"), TextAlignmentType.Center, TextAlignmentType.Right, true, 12, false);
                SetCellValue(SheetKPI.Cells["B5"], Util.GetLang("OM22100_StartDate"), TextAlignmentType.Center, TextAlignmentType.Right, true, 12, false);
                SetCellValue(SheetKPI.Cells["E5"], Util.GetLang("OM22100_EndDate"), TextAlignmentType.Center, TextAlignmentType.Right, true, 12, false);

                SetCellValue(SheetKPI.Cells["C2"], CycleNbr, TextAlignmentType.Center, TextAlignmentType.Left, true, 12, true);
                SetCellValue(SheetKPI.Cells["C3"], BranchID, TextAlignmentType.Center, TextAlignmentType.Left, true, 12, true);
                SetCellValue(SheetKPI.Cells["C4"], BranchName, TextAlignmentType.Center, TextAlignmentType.Left, true, 12, true);
                SetCellValue(SheetKPI.Cells["C5"], StartDate, TextAlignmentType.Center, TextAlignmentType.Left, true, 12, true);
                SetCellValue(SheetKPI.Cells["F5"], EndDate, TextAlignmentType.Center, TextAlignmentType.Left, true, 12, true);
  
                // Header text columns
                // before of Route column
                var ColTexts = new string[] { "No", "SlsperId", "SlsName", "ClassID", "InvtID", "Descr", "CnvFact", "Qty" };
                for (int i = 0; i < ColTexts.Length; i++)
                {
                    var colIdx = i;
                    SetCellValue(SheetKPI.Cells[5, colIdx], Util.GetLang(ColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 12);
                    SheetKPI.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }

                var allColumns = new List<string>();
                allColumns.AddRange(ColTexts);

                #endregion

                #region export data
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30));
                pc.Add(new ParamStruct("@CycleNbr", DbType.String, clsCommon.GetValueDBNull(CycleNbr), ParameterDirection.Input, 6));
        

                DataTable dtDataExport = dal.ExecDataTable("OM22101_peExportData", CommandType.StoredProcedure, ref pc);
     
                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    SheetKPI.Cells[7 + i, 0].PutValue(i + 1);
                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            SheetKPI.Cells[7 + i, j].PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }
                    }
                }
                #endregion

                SheetKPI.AutoFitColumns();
                //SheetKPI.Cells.Columns[allColumns.IndexOf("CustID")].Width = 30;
                //SheetKPI.Cells.Columns[allColumns.IndexOf("CustName")].Width = 30;
                //SheetKPI.Cells.Columns[allColumns.IndexOf("SlsName")].Width = 30;
                //SheetKPI.Cells.Columns[allColumns.IndexOf("Address")].Width = 30;

                ////SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM22101") + ".xlsx" };
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

        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle = false)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
                style.Font.Color = Color.Blue;
            c.SetStyle(style);
        }

        #region other
        private string Getcell(int column)
        {
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 > 1)
            {
                cell += ABC.Substring(column / 26, 1);
                column += column - 26;
            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            return cell;
        }
        #endregion
    }
}

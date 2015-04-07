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

namespace OM22100.Controllers
{  
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22100Controller : Controller
    {
        string screenNbr = "OM22100";
        OM22100Entities _db = Util.CreateObjectContext<OM22100Entities>(false);
        eSkySysEntities mDb = Util.CreateObjectContext<eSkySysEntities>(true);
        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = mDb.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM22100");
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
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        //[HttpPost]
        //public ActionResult Save(FormCollection data)
        //{
        //    //StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
        //    //ChangeRecords<ppv_OM22100_LoadGrid_Result> lstgrd = dataHandler1.BatchObjectData<ppv_OM22100_LoadGrid_Result>();

        //    //foreach (ppv_OM22100_LoadGrid_Result deleted in lstgrd.Deleted)
        //    //{
        //    //    var del = _db.POS_MasterData.Where(p =>  p.POSCode == deleted.POSCode).FirstOrDefault();
        //    //    if (del != null)
        //    //    {
        //    //        _db.POS_MasterData.DeleteObject(del);

        //    //    }
        //    //}
        //    //foreach (ppv_OM22100_LoadGrid_Result created in lstgrd.Created)
        //    //{
        //    //    var record = _db.POS_MasterData.Where(p => p.POSCode == created.POSCode).FirstOrDefault();

        //    //    if (created.tstamp.ToHex() == "")
        //    //    {
        //    //        if (record == null)
        //    //        {
        //    //            record = new POS_MasterData();
        //    //            record.POSCode = created.POSCode;

        //    //            record.Crtd_DateTime = DateTime.Now;
        //    //            record.Crtd_Prog = screenNbr;
        //    //            record.Crtd_User = Current.UserName;

        //    //            UpdatingPOS_MasterData(created, ref record);
        //    //            if ((record.SDSM != "" && record.POSCode != "") || (record.DSM != "" && record.POSCode != "") || 
        //    //                ((record.SDSM != "" && record.DSM != "" && record.POSCode != "")))
        //    //            {
        //    //                _db.POS_MasterData.AddObject(record);
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            return Json(new
        //    //            {
        //    //                success = false,
        //    //                code = "151",
        //    //                colName = Util.GetLang("Type"),

        //    //            }, JsonRequestBehavior.AllowGet);
        //    //            //tra ve loi da ton tai ma ngon ngu nay ko the them
        //    //        }
        //    //    }
        //    //}

        //    //foreach (ppv_OM22100_LoadGrid_Result updated in lstgrd.Updated)
        //    //{
        //    //    var record = _db.POS_MasterData.Where(p =>  p.POSCode == updated.POSCode).FirstOrDefault();

        //    //    if (record != null)
        //    //    {
        //    //        if (record.tstamp.ToHex() != updated.tstamp.ToHex())
        //    //        {
        //    //            return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
        //    //        }
        //    //        UpdatingPOS_MasterData(updated, ref record);
        //    //    }
        //    //    else
        //    //    {
        //    //        if (updated.tstamp.ToHex() == "")
        //    //        {
        //    //            record = new POS_MasterData();
        //    //            record.AreaCode = updated.AreaCode;
        //    //            record.SDSM = updated.SDSMCode;
        //    //            record.DSM = updated.DSMCode;
        //    //            record.POSCode = updated.POSCode;
        //    //            record.Crtd_DateTime = DateTime.Now;
        //    //            record.Crtd_Prog = screenNbr;
        //    //            record.Crtd_User = Current.UserName;
        //    //            UpdatingPOS_MasterData(updated, ref record);
        //    //            if ((record.SDSM != "" && record.POSCode != "") || (record.DSM != "" && record.POSCode != "") ||
        //    //                ((record.SDSM != "" && record.DSM != "" && record.POSCode != "")))
        //    //            {
        //    //                _db.POS_MasterData.AddObject(record);
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
        //    //        }
        //    //    }

        //    //}
        //    //_db.SaveChanges();

        //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public ActionResult OM22100Import(FormCollection data)
        //{

        //    string BranchID = data["BranchID"].PassNull();
        //    string PJP = BranchID;
        //    var tmpCheckRowAdded = "";
        //    string lstUser = "";
        //    string lstName = "";
        //    string lstPass = "";
        //    var k = 0;
        //    try
        //    {
        //        var date = DateTime.Now.Date;
        //        FileUploadField fileUploadField = X.GetCmp<FileUploadField>("ImportTemplate");
        //        HttpPostedFile file = fileUploadField.PostedFile;
        //        FileInfo fileInfo = new FileInfo(file.FileName);
        //        string messagestrERouteID = string.Empty;
        //        string messagestrECustID = string.Empty;
        //        string messagestrESlsperID = string.Empty;
        //        string messagestrEBeginDate = string.Empty;
        //        string messagestrEEndDate = string.Empty;
        //        string messagestrETBH = string.Empty;
        //        string messagestrETS = string.Empty;
        //        string messageDate = string.Empty;
        //        string messageerror = string.Empty;
        //        string messageduplicate = string.Empty;
        //        string message = string.Empty;
        //        if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
        //        {

        //            Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
        //            if (workbook.Worksheets.Count > 0)
        //            {
        //                Worksheet workSheet = workbook.Worksheets[0];



        //                string strEBanchID = workSheet.Cells[1, 2].StringValue;//dataArray.GetValue(2, 3).PassNull();// w1.Rows[1].Cells[2).PassNull();
        //                string strEPJP = workSheet.Cells[1, 2].StringValue;// dataArray.GetValue(2, 3).PassNull();
        //                string strERouteID = "";
        //                string strECustID = "";
        //                string strESlsperID = "";
        //                string strEBeginDate = "";
        //                string strEEndDate = "";
        //                string strETS = "";
        //                string strETBH = "";
        //                string strESTT = "";
        //                DateTime startDate = DateTime.Now;
        //                DateTime endDate = DateTime.Now;
        //                if (strEPJP.ToUpper().Trim() != PJP.ToUpper().Trim() || BranchID != strEBanchID.ToUpper().Trim())
        //                {
        //                    return Json(new { success = false, messid = 201401221, error = new string[] { strEPJP, strEBanchID, PJP, BranchID } });

        //                }
        //                var objPJP = _db.OM_PJP.Where(p => p.PJPID == PJP).FirstOrDefault();
        //                if (objPJP == null)
        //                {
        //                    objPJP = new OM_PJP();
        //                    objPJP.PJPID = PJP;
        //                    objPJP.BranchID = BranchID;
        //                    objPJP.Descr = "Kế hoạch viếng thăm " + BranchID;
        //                    objPJP.LUpd_DateTime = DateTime.Today;
        //                    objPJP.LUpd_Prog = screenNbr;
        //                    objPJP.LUpd_User = Current.UserName;
        //                    objPJP.Status = true;
        //                    objPJP.StatusHandle = "C";

        //                    objPJP.Crtd_DateTime = DateTime.Today;
        //                    objPJP.Crtd_Prog = screenNbr;
        //                    objPJP.Crtd_User = Current.UserName;

        //                    _db.OM_PJP.AddObject(objPJP);
        //                }

        //                string lstCustomer = "";
        //                string strtmpError = "";
        //                string id = Guid.NewGuid().ToString();
        //                for (int i = 5; i < workSheet.Cells.MaxDataRow; i++)
        //                {


        //                    strESTT = workSheet.Cells[i, 0].StringValue;//dataArray.GetValue(i, 1).PassNull();
        //                    strERouteID = workSheet.Cells[i, 17].StringValue;//dataArray.GetValue(i, 18).PassNull();
        //                    strECustID = workSheet.Cells[i, 1].StringValue;//dataArray.GetValue(i, 2).PassNull();
        //                    strESlsperID = workSheet.Cells[i, 4].StringValue;//dataArray.GetValue(i, 5).PassNull();
        //                    strEBeginDate = workSheet.Cells[i, 6].StringValue;//dataArray.GetValue(i, 7).PassNull();
        //                    strEEndDate = workSheet.Cells[i, 7].StringValue;//dataArray.GetValue(i, 8).PassNull();
        //                    strETS = workSheet.Cells[i, 8].StringValue;//dataArray.GetValue(i, 9).PassNull();
        //                    strETBH = workSheet.Cells[i, 9].StringValue;//dataArray.GetValue(i, 10).PassNull();

        //                    if (strECustID == "") continue;
        //                    else if (strERouteID == ""
        //                         || strECustID == ""
        //                         || strESlsperID == ""
        //                         || strETS == ""
        //                        || strETBH == ""
        //                        || strEBeginDate == ""
        //                        || strEEndDate == "")
        //                    {
        //                        if (strERouteID == "")
        //                        {
        //                            messagestrERouteID += (i + 1).ToString() + ",";
        //                            continue;

        //                        }
        //                        if (strECustID == "")
        //                        {
        //                            messagestrECustID += (i + 1).ToString() + ",";
        //                            continue;
        //                        }

        //                        if (strESlsperID == "")
        //                        {
        //                            messagestrESlsperID += (i + 1).ToString() + ",";
        //                            continue;

        //                        }
        //                        if (strETS == "")
        //                        {
        //                            messagestrETS += (i + 1).ToString() + ",";
        //                            continue;

        //                        }
        //                        if (strETBH == "")
        //                        {
        //                            messagestrETBH += (i + 1).ToString() + ",";
        //                            continue;

        //                        }
        //                        if (strEBeginDate == "")
        //                        {
        //                            messagestrEBeginDate += (i + 1).ToString() + ",";
        //                            continue;

        //                        }
        //                        if (strEEndDate == "")
        //                        {
        //                            messagestrEEndDate += (i + 1).ToString() + ",";
        //                            continue;

        //                        }

        //                    }

        //                    else
        //                    {
        //                        try
        //                        {
        //                            startDate = workSheet.Cells[i, 6].DateTimeValue.ToDateShort();// DateTime.FromOADate(double.Parse(workSheet.Cells[i, 6].StringValue)).Short();
        //                            endDate = workSheet.Cells[i, 7].DateTimeValue.ToDateShort(); //DateTime.FromOADate(double.Parse(workSheet.Cells[i, 7].StringValue)).Short();

        //                        }
        //                        catch
        //                        {
        //                            messageDate += string.Format("Dòng {0} dữ liệu ngày tháng không hợp lệ<br/>", (i + 1).ToString());
        //                            continue;

        //                        }
        //                        OM_SalesRouteMasterImport objImport = new OM_SalesRouteMasterImport();
        //                        bool isNew = false;

        //                        lstCustomer += strECustID + ";";
        //                        if (_db.OM_SalesRouteMasterImport.Where(p => p.ID == id
        //                                                                            && p.BranchID == BranchID
        //                                                                             && p.PJPID == BranchID
        //                                                                              && p.SalesRouteID == strERouteID
        //                                                                              && p.CustID == strECustID
        //                                                                               && p.SlsPerID == strESlsperID).ToList().Count == 0)
        //                        {
        //                            objImport.ID = id;
        //                            objImport.BranchID = BranchID;
        //                            objImport.PJPID = BranchID;
        //                            objImport.SalesRouteID = strERouteID;
        //                            objImport.CustID = strECustID;
        //                            objImport.SlsPerID = strESlsperID;
        //                            objImport.tstamp = new byte[1];
        //                            objImport.StartDate = startDate;
        //                            objImport.EndDate = endDate; ;
        //                            objImport.SlsFreq = workSheet.Cells[i, 8].StringValue;//  dataArray.GetValue(i, 9).ToString().Trim().ToUpper();
        //                            objImport.SlsFreqType = "R";
        //                            objImport.WeekofVisit = workSheet.Cells[i, 9].StringValue;// dataArray.GetValue(i, 10).ToString().Trim().ToUpper();
        //                            objImport.Mon = workSheet.Cells[i, 10].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 11) == null ? false : dataArray.GetValue(i, 11).ToString().Trim().ToUpper() == "X" ? true : false;
        //                            objImport.Tue = workSheet.Cells[i, 11].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 12) == null ? false : dataArray.GetValue(i, 12).ToString().Trim().ToUpper() == "X" ? true : false;
        //                            objImport.Wed = workSheet.Cells[i, 12].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 13) == null ? false : dataArray.GetValue(i, 13).ToString().Trim().ToUpper() == "X" ? true : false;
        //                            objImport.Thu = workSheet.Cells[i, 13].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 14) == null ? false : dataArray.GetValue(i, 14).ToString().Trim().ToUpper() == "X" ? true : false;
        //                            objImport.Fri = workSheet.Cells[i, 14].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 15) == null ? false : dataArray.GetValue(i, 15).ToString().Trim().ToUpper() == "X" ? true : false;
        //                            objImport.Sat = workSheet.Cells[i, 15].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 16) == null ? false : dataArray.GetValue(i, 16).ToString().Trim().ToUpper() == "X" ? true : false;
        //                            objImport.Sun = workSheet.Cells[i, 16].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 17) == null ? false : dataArray.GetValue(i, 17).ToString().Trim().ToUpper() == "X" ? true : false;
        //                            try
        //                            {
        //                                objImport.VisitSort = workSheet.Cells[i, 19].StringValue == "" ? 0 : workSheet.Cells[i, 9].ToInt();// dataArray.GetValue(i, 20) == null ? 0 : dataArray.GetValue(i, 20).ToString().Trim().ToUpper() == "" ? 0 : int.Parse(dataArray.GetValue(i, 20).ToString().Trim().ToUpper());
        //                            }
        //                            catch
        //                            {
        //                                objImport.VisitSort = 0;
        //                            }
        //                            objImport.LUpd_DateTime = objImport.LUpd_DateTime = DateTime.Now;
        //                            objImport.LUpd_Prog = objImport.LUpd_Prog = screenNbr;
        //                            objImport.LUpd_User = objImport.LUpd_User = Current.UserName;
        //                            objImport.Crtd_DateTime = objImport.Crtd_DateTime = DateTime.Now;
        //                            objImport.Crtd_Prog = objImport.Crtd_Prog = screenNbr;
        //                            objImport.Crtd_User = objImport.Crtd_User = Current.UserName;
        //                            if (isValidSelOMSalesRouteMaster(objImport, false))
        //                            {
        //                                if (workSheet.Cells[i, 20].StringValue != null && workSheet.Cells[i, 20].StringValue == "X")
        //                                {
        //                                    objImport.Del = true;

        //                                }
        //                                _db.OM_SalesRouteMasterImport.AddObject(objImport);
        //                            }
        //                            else
        //                            {
        //                                messageerror += (i + 1).ToString() + ",";
        //                                //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu không hợp lệ" + "\r";
        //                            }

        //                        }
        //                        else messageduplicate += (i + 1).ToString() + ",";  //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu bi trùng" + "\r";
        //                    }
        //                }

        //                _db.SaveChanges();
        //                // toi day thoi
        //                Exec(id);
        //                message = messagestrECustID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrECustID, workSheet.Cells[3, 1].StringValue);
        //                message += messagestrESlsperID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrESlsperID, workSheet.Cells[3, 4].StringValue);
        //                message += messagestrETS == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrETS, workSheet.Cells[3, 8].StringValue);
        //                message += messagestrETBH == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrETBH, workSheet.Cells[3, 9].StringValue);
        //                message += messagestrEBeginDate == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrEBeginDate, workSheet.Cells[3, 6].StringValue);
        //                message += messagestrEEndDate == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrEEndDate, workSheet.Cells[3, 7].StringValue);
        //                message += messageDate == "" ? "" : string.Format("Dòng {0} dữ liệu ngày tháng không hợp lệ<br/>", messageDate.ToString());
        //                message += messagestrERouteID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrERouteID, workSheet.Cells[3, 13].StringValue);
        //                message += messageerror == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ<br/>", messageerror);
        //                message += messageduplicate == "" ? "" : string.Format("Dòng {0} dữ liệu bi trùng<br/>", messageduplicate);

        //                Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
        //            }
        //            return _logMessage;

        //        }
        //        else
        //        {
        //            Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
        //    }
        //    return _logMessage;
        //}

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
       
                string BranchID = data["BranchID"].PassNull();
                string BranchName=data["BranchName"].PassNull();
                string CycleNbr = data["CycleNbr"].PassNull();
                string StartDate = data["StartDate"].PassNull();
                string EndDate = data["EndDate"].PassNull();
                var headerRowIdx = 5;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetKPI = workbook.Worksheets[0];
                SheetKPI.Name = Util.GetLang("OM22100_KPI");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                Range range;
                Cell cell;

                #region header info
                // Title header
                SetCellValue(SheetKPI.Cells["A1"],
                    string.Format("{0} {1}", Util.GetLang("OM22100_ExportHeader") + "(" + BranchID + ")", (string.IsNullOrWhiteSpace(BranchID) ? string.Format("({0})", BranchID) : string.Empty)),
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
                var ColTexts = new string[] { "No", "BranchID", "BranchName", "SlsperId", "SlsName", "PC", "LPPC", "ASO"};
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
        

                DataTable dtDataExport = dal.ExecDataTable("OM22100_peExportData", CommandType.StoredProcedure, ref pc);
     
                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
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

                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM22100") + ".xlsx" };
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

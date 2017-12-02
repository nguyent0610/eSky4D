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
using HQ.eSkySys;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
namespace OM28300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM28300Controller : Controller
    {
        private string _screenNbr = "OM28300";
        private string _userName = Current.UserName;
        private JsonResult _logMessage;

        OM28300Entities _db = Util.CreateObjectContext<OM28300Entities>(false);

        public ActionResult Index()
        {
            bool allowImport = false
                , allowExport = false;

            var objConfig = _db.OM28300_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                allowExport = objConfig.AllowExport.HasValue ? objConfig.AllowExport.Value : false;
                allowImport = objConfig.AllowImport.HasValue ? objConfig.AllowImport.Value : false;
            }
            ViewBag.allowExport = allowExport;
            ViewBag.allowImport = allowImport;
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetData()
        {
            return this.Store(_db.OM28300_pgLoadGrid(Current.CpnyID, Current.UserName, Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<OM_EquipmentStatus> lstData = dataHandler.BatchObjectData<OM_EquipmentStatus>();
                foreach (OM_EquipmentStatus deleted in lstData.Deleted)
                {
                    var del = _db.OM_EquipmentStatus.Where(p => p.BranchID.ToUpper().Trim() == deleted.BranchID.ToUpper().Trim() && p.EquipmentID.ToUpper().Trim() == deleted.EquipmentID.ToUpper().Trim()).FirstOrDefault();
                    if (del != null)
                    {
                        _db.OM_EquipmentStatus.DeleteObject(del);
                    }
                }

                //lstData.Created.AddRange(lstData.Updated);
                lstData.Created.AddRange(lstData.Updated);
                foreach (OM_EquipmentStatus del in lstData.Deleted)
                {
                    //neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstData.Created.Where(p => p.BranchID.ToUpper().Trim() == del.BranchID.ToUpper().Trim() && p.EquipmentID.ToUpper().Trim() == del.EquipmentID.ToUpper().Trim()).Count() > 0)
                    {
                        lstData.Created.Where(p => p.BranchID.ToUpper().Trim() == del.BranchID.ToUpper().Trim() && p.EquipmentID.ToUpper().Trim() == del.EquipmentID.ToUpper().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_EquipmentStatus.ToList().Where(p => p.BranchID.ToUpper().Trim() == del.BranchID.ToUpper().Trim() && p.EquipmentID.ToUpper().Trim() == del.EquipmentID.ToUpper().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_EquipmentStatus.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM_EquipmentStatus reco in lstData.Created)
                {
                    if (reco.BranchID.PassNull() == "" || reco.EquipmentID.PassNull() == "") continue;

                    var lang = _db.OM_EquipmentStatus.Where(p => p.BranchID.ToUpper().Trim() == reco.BranchID.ToUpper().Trim() && p.EquipmentID.ToUpper().Trim() == reco.EquipmentID.ToUpper().Trim()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == reco.tstamp.ToHex())
                        {
                            Update_EquipmentStatus(lang, reco, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_EquipmentStatus();
                        Update_EquipmentStatus(lang, reco, true);
                        _db.OM_EquipmentStatus.AddObject(lang);
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_EquipmentStatus(OM_EquipmentStatus t, OM_EquipmentStatus s, bool isNew)
        {
            if (isNew)
            {
                t.BranchID = s.BranchID.ToUpper();
                t.EquipmentID = s.EquipmentID;
                t.IMEI = s.IMEI.ToUpper();
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            if (s.Date == null)
            {
                var datet = "01/01/1900";
                t.Date = datet.ToDateShort();
            }
            else
            {
                t.Date = s.Date.ToDateShort();
            }
            
            t.Status = s.Status;
            t.Lupd_DateTime = DateTime.Now;
            t.Lupd_Prog = _screenNbr;
            t.Lupd_User = _userName;
        }

        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_EquipmentStatus"]);
                var lstOM_EquipmentStatus = dataHandler.ObjectData<OM28300_pgLoadGrid_Result>();

                Cell cell;
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = Util.GetLang("OM28300NameSheet");

                //TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["A1"], Util.GetLang("OM23800BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["B1"], Util.GetLang("EquipmentID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["C1"], Util.GetLang("IMEI"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["D1"], Util.GetLang("OM28300Date"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["E1"], Util.GetLang("Status"), TextAlignmentType.Center, TextAlignmentType.Left);
                SheetData.Cells.SetRowHeight(0, 45);
                Style colStyle = SheetData.Cells.Columns[1].Style;
                Style colStyle1 = SheetData.Cells.Columns[2].Style;
                StyleFlag flag = new StyleFlag();
                flag.NumberFormat = true;
                colStyle.Number = 49;
                colStyle1.Number = 49;
                SheetData.Cells.Columns[0].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[4].ApplyStyle(colStyle, flag);

                SheetData.Cells.SetColumnWidth(0, 15);
                SheetData.Cells.SetColumnWidth(1, 15);
                SheetData.Cells.SetColumnWidth(2, 15);
                SheetData.Cells.SetColumnWidth(3, 15);
                SheetData.Cells.SetColumnWidth(4, 15);

                var style = SheetData.Cells["D2"].GetStyle();
                style.Custom = "MM/dd/yyyy";
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Left;
                var range = SheetData.Cells.CreateRange("D2", "D2000");
                range.SetStyle(style);

                int iRow = 2;
                //foreach (OM28300_pgLoadGrid_Result r in lstOM_EquipmentStatus)
                //{
                //    if(r.BranchID!=null && r.BranchID != "")
                //    {
                //        cell = SheetData.Cells["A" + iRow];
                //        cell.PutValue(r.BranchID.ToString());
                //        cell = SheetData.Cells["B" + iRow];
                //        cell.PutValue(r.EquipmentID.ToString());
                //        cell = SheetData.Cells["C" + iRow];
                //        cell.PutValue(r.IMEI.ToString());
                //        cell = SheetData.Cells["D" + iRow];
                //        cell.PutValue(r.Date.ToDateTime());
                //        cell = SheetData.Cells["E" + iRow];
                //        cell.PutValue(r.Status.ToString());
                //        iRow++;
                //    }
                    
                //}

                //CommentPGType
                int commentIndex = SheetData.Comments.Add("N3");
                Validation validation = SheetData.Validations[SheetData.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;

                var fileName = Util.GetLang("OM28300Template") + DateTime.Now.ToString("yyyyMMddHHmm") + ".xlsx";
                string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);

                workbook.Settings.AutoRecover = true;

                workbook.Save(fullPath, SaveFormat.Xlsx);
                return Json(new { success = true, fileName = fileName, errorMessage = "" });
                //workbook.Save(stream, SaveFormat.Xlsx);
                //stream.Flush();
                //stream.Position = 0;
                //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM28300Template") };
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
        [HttpGet]
        [DeleteFileAttribute] //Action Filter, it will auto delete the file after download,I will explain it later
        public ActionResult DownloadAndDelete(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.ms-excel", file);
        }
        private void SetCellValueGridHeader(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 12;
            style.Font.Color = Color.CornflowerBlue;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }


        private void SetCellValueGrid(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 11;
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
                var access = Session["AR20300"] as AccessRight;

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                List<OM_EquipmentStatus> lstOM_EquipmentStatus = new List<OM_EquipmentStatus>();
                string message = string.Empty;
                string errorBrachIDNull = string.Empty;
                string errorBrachID = string.Empty;
                string errorEquipmentID = string.Empty;
                string errorEquipmentIDLeng = string.Empty;
                string errorStatus = string.Empty;
                string errorDate = string.Empty;
                string okStatus = string.Empty;
                string erroriMEI = string.Empty;
                string erroriMEINull = string.Empty;
                //string okStatus = string.Empty;

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("OM23800BranchID").ToUpper().Trim()
                          || workSheet.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("EquipmentID").ToUpper().Trim()
                          || workSheet.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("IMEI").ToUpper().Trim()
                          || workSheet.Cells[0, 3].StringValue.ToUpper().Trim() != (Util.GetLang("OM28300Date").ToUpper().Trim())
                          || workSheet.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("Status").ToUpper().Trim()
                          )
                        {
                            throw new MessageException(MessageType.Message, "148");
                        }
                        
                        //string Statusimport = string.Empty;
                        var lstBranch = _db.OM28300_pcBranch(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                        var lstStatus = _db.OM28300_pcStatus(Current.CpnyID, Current.UserName, Current.LangID).ToList();

                        foreach (OM28300_pcStatus_Result r in lstStatus)
                        {
                            okStatus += r.Code.ToString() + "(" + r.Descr.ToString() + "), ";
                        }
                        var lstOM28300_pgLoadGrid = _db.OM28300_pgLoadGrid(Current.CpnyID, Current.UserName, Current.LangID).ToList();


                            for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                            {                                
                                bool flagCheck = false;
                                var time = DateTime.Now;
                                string branchIDimport = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper();
                                string equipmentIDimport = workSheet.Cells[i, 1].StringValue.PassNull();
                                string iMEIimport = workSheet.Cells[i, 2].StringValue.PassNull();
                                string dateimpo = workSheet.Cells[i, 3].StringValue.PassNull();
                                if(dateimpo!=null && dateimpo!="")
                                {
                                    try
                                    {
                                        //var dateimport = workSheet.Cells[i, 3].DateTimeValue;
                                        time = DateTime.ParseExact(workSheet.Cells[i, 3].StringValue, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch
                                    {
                                        errorDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                            
                                string status = workSheet.Cells[i, 4].StringValue.PassNull().ToUpper();
                                if (branchIDimport == "" || branchIDimport==null)
                                {
                                    errorBrachIDNull += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (lstBranch.FirstOrDefault(p => p.BranchID.ToUpper() == branchIDimport) == null)
                                    {
                                        errorBrachID += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                
                                if (equipmentIDimport == ""||errorEquipmentID==null)
                                {
                                    errorEquipmentID += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (equipmentIDimport.ToString().Length>50)
                                    {
                                        errorEquipmentIDLeng += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                if(iMEIimport==null|| iMEIimport == "")
                                {
                                    erroriMEINull += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (iMEIimport.Length > 100)
                                    {
                                        erroriMEI += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }                                

                                if (status != "" && status!=null)
                                {
                                    if (lstStatus.FirstOrDefault(p => p.Code.ToUpper() == status) == null)
                                    {
                                        errorStatus += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }                                        
                                }

                                if (flagCheck == true)
                                {
                                    continue;
                                }
                               

                                var recordExists = lstOM_EquipmentStatus.FirstOrDefault(p => p.BranchID.ToUpper()==branchIDimport && p.EquipmentID.ToUpper() == equipmentIDimport.ToUpper());
                                if (recordExists == null)
                                {
                                    var record = _db.OM_EquipmentStatus.FirstOrDefault(p => p.BranchID.ToUpper() == branchIDimport.ToUpper() && p.EquipmentID.ToUpper() == equipmentIDimport.ToUpper());
                                    if (record == null)
                                    {
                                        record = new OM_EquipmentStatus();
                                        record.ResetET();
                                        record.BranchID = branchIDimport;//các key đều upper lên khi save
                                        record.EquipmentID = equipmentIDimport;

                                        if (dateimpo != null && dateimpo != "")
                                        {
                                            record.Date = time.ToDateShort();
                                        }
                                        else
                                        {
                                            var tam = "01/01/1900";
                                            record.Date = tam.ToDateShort();
                                        }
                                        record.Crtd_DateTime = DateTime.Now;
                                        record.Crtd_Prog = _screenNbr;
                                        record.Crtd_User = Current.UserName;
                                        _db.OM_EquipmentStatus.AddObject(record);

                                    }
                                    record.Status = status;
                                    record.IMEI = iMEIimport;
                                    record.Lupd_DateTime = DateTime.Now;
                                    record.Lupd_Prog = _screenNbr;
                                    record.Lupd_User = Current.UserName;
                                    lstOM_EquipmentStatus.Add(record);

                                }

                            }
                        

                        message = errorBrachIDNull == "" ? "" : string.Format(Message.GetString("2017112901", null), Util.GetLang("OM23800BranchID"), errorBrachIDNull);
                        message += errorBrachID == "" ? "" : string.Format(Message.GetString("2017112902", null), Util.GetLang("OM23800BranchID"), errorBrachID);
                        message += errorEquipmentID == "" ? "" : string.Format(Message.GetString("2017112901", null), Util.GetLang("EquipmentID"), errorEquipmentID);
                        message += errorEquipmentIDLeng == "" ? "" : string.Format(Message.GetString("2017112502", null), Util.GetLang("EquipmentID"), errorBrachID,"50");
                        message += erroriMEINull == "" ? "" : string.Format(Message.GetString("2017112901", null), Util.GetLang("IMEI"), erroriMEINull);
                        message += erroriMEI == "" ? "" : string.Format(Message.GetString("2017112502", null), Util.GetLang("IMEI"), erroriMEI,"100");
                        message += errorDate == "" ? "" : string.Format(Message.GetString("2017120101", null), Util.GetLang("OM28300Date"), errorDate);
                        message += errorStatus == "" ? "" : string.Format(Message.GetString("2017112904", null), Util.GetLang("Status"), errorStatus,okStatus);
                        if (message == "" || message == string.Empty)
                        {
                            _db.SaveChanges();
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                    }

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
    public class DeleteFileAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Flush();

            //convert the current filter context to file and get the file path
            string filePath = (filterContext.Result as FilePathResult).FileName;

            //delete the file after download
            System.IO.File.Delete(filePath);
        }
    }
}

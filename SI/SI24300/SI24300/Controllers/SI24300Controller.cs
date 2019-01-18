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
using HQFramework.DAL;
using System.Drawing;
using Aspose.Cells;
using System.Text.RegularExpressions;
namespace SI24300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI24300Controller : Controller
    {
        private string _screenNbr = "SI24300";
        private string _userName = Current.UserName;

        SI24300Entities _db = Util.CreateObjectContext<SI24300Entities>(false);
        private JsonResult _logMessage;
        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetSI_Ward()
        {        
            return this.Store(_db.SI24300_pgSI_Ward(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Ward"]);
                ChangeRecords<SI24300_pgSI_Ward_Result> lstSI_Ward = dataHandler.BatchObjectData<SI24300_pgSI_Ward_Result>();
                foreach (SI24300_pgSI_Ward_Result deleted in lstSI_Ward.Deleted)
                {
                    if (lstSI_Ward.Created.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.District == deleted.District && p.Ward == deleted.Ward).Count() > 0)
                    {
                        lstSI_Ward.Created.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.District == deleted.District && p.Ward == deleted.Ward).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.SI_Ward.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.District == deleted.District && p.Ward == deleted.Ward).FirstOrDefault();
                        if (del != null)
                        {
                            if(_db.SI24300_pdCheckbeforedelete(Current.CpnyID, Current.UserName, Current.LangID, deleted.Ward).FirstOrDefault() == 1)
                            {
                                throw new MessageException(MessageType.Message, "2018091305", "", new string[] { Util.GetLang("ProvinceCode"), del.State, Util.GetLang("SI24300DistrictCode"), del.District, Util.GetLang("SI24300Ward"), del.Ward });
                            }
                            _db.SI_Ward.DeleteObject(del);
                        }
                    }

                }

                lstSI_Ward.Created.AddRange(lstSI_Ward.Updated);

                foreach (SI24300_pgSI_Ward_Result curWard in lstSI_Ward.Created)
                {
                    if (curWard.Country.PassNull() == "" && curWard.State.PassNull() == "" && curWard.District.PassNull() == "" && curWard.Ward.PassNull() == "") continue;

                    var Ward = _db.SI_Ward.Where(p => p.Country.ToLower() == curWard.Country.ToLower() && p.State.ToLower() == curWard.State.ToLower() && p.District.ToLower() == curWard.District.ToLower() && p.Ward.ToLower() == curWard.Ward.ToLower()).FirstOrDefault();

                    if (Ward != null)
                    {
                        if (Ward.tstamp.ToHex() == curWard.tstamp.ToHex())
                        {
                            Update_SI_Ward(Ward, curWard, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Ward = new SI_Ward();
                        Update_SI_Ward(Ward, curWard, true);
                        _db.SI_Ward.AddObject(Ward);
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
        private void Update_SI_Ward(SI_Ward t, SI24300_pgSI_Ward_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.State = s.State;
                t.District = s.District;
                t.Ward = s.Ward;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Name = s.WardName;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
         #region -Import-
        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                List<SI_Ward> lstWard = new List<SI_Ward>();
                string message = string.Empty;
                string errorState = string.Empty;
                string errorStateNotExists = string.Empty;
                string errorDistrict = string.Empty;
                string errorDistrictNotExists = string.Empty;
                string errorWard = string.Empty;
                string errorWardFormat = string.Empty;
                string errorWardUnicode = string.Empty;
                string errorWardName = string.Empty;
                string errorWardNameFormat = string.Empty;
                string errorDuplicateWard = string.Empty;
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            string state = string.Empty;
                            string district = string.Empty;
                            string ward = string.Empty;
                            string wardName = string.Empty;
                            bool flagCheck = false;
                            Worksheet workSheet = workbook.Worksheets[0];
                            var ColTexts = new List<string>() { 
                                  "State", "District", "Ward", "WardName"
                                };

                            for (int i = 3; i <= workSheet.Cells.MaxDataRow; i++)
                            {
                                state = workSheet.Cells[i, ColTexts.IndexOf("State")].StringValue.PassNull();
                                district = workSheet.Cells[i, ColTexts.IndexOf("District")].StringValue.PassNull();
                                ward = workSheet.Cells[i, ColTexts.IndexOf("Ward")].StringValue.PassNull();
                                wardName = workSheet.Cells[i, ColTexts.IndexOf("WardName")].StringValue.PassNull();
                                
                                if (state == "")
                                {
                                    errorState += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if(_db.SI24300_pcState(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p=>p.State == state) == null)
                                    {
                                        errorStateNotExists += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                
                                if (district == "")
                                {
                                    errorDistrict += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (_db.SI24300_pcDistrict(Current.UserName, Current.CpnyID, Current.LangID, state).FirstOrDefault(p => p.State == state && p.District == district) == null)
                                    {
                                        errorDistrictNotExists += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                if (ward == "")
                                {
                                    errorWard += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (ward.Length > 30 )
                                    {
                                        errorWardFormat += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (!IsUniCode(ward))
                                    {
                                        errorWardUnicode += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                if(wardName == "")
                                {
                                    errorWardName += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (wardName.Length > 100)
                                    {
                                        errorWardNameFormat += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                if (flagCheck == true)
                                {
                                    continue;
                                }
                                var recordExistsCust = lstWard.FirstOrDefault(p => p.State == state && p.District == district && p.Ward == ward);
                                if (recordExistsCust == null)
                                {
                                    if (ward != "")
                                    {
                                        var recordItem = _db.SI_Ward.FirstOrDefault(p => p.State == state && p.District == district && p.Ward == ward);
                                        if (recordItem == null)
                                        {
                                            recordItem = new SI_Ward();
                                            recordItem.ResetET();
                                            recordItem.Country = "VN";
                                            recordItem.State = state;
                                            recordItem.District = district;
                                            recordItem.Ward = ward;
                                            recordItem.Name = wardName;
                                            recordItem.Crtd_Datetime = recordItem.LUpd_Datetime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;

                                            _db.SI_Ward.AddObject(recordItem);
                                           
                                        }
                                        else
                                        {
                                            recordItem.Name = wardName;

                                            recordItem.LUpd_Datetime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                        lstWard.Add(recordItem);
                                    }
                                }
                                else
                                {
                                    errorDuplicateWard += (i + 1).ToString() + ",";
                                    continue;
                                }
                            }
                            message = errorState == "" ? "" : string.Format(Message.GetString("2018091301", null), "Mã tỉnh", errorState);
                            message += errorStateNotExists == "" ? "" : string.Format(Message.GetString("2018091302", null), "Mã tỉnh", errorStateNotExists);
                            message += errorDistrict == "" ? "" : string.Format(Message.GetString("2018091301", null), "Mã Quận/Huyện/TP Thuộc Tỉnh", errorDistrict);
                            message += errorDistrictNotExists == "" ? "" : string.Format(Message.GetString("2018091302", null), "Mã Quận/Huyện/TP Thuộc Tỉnh", errorDistrictNotExists);
                            message += errorWard == "" ? "" : string.Format(Message.GetString("2018091301", null), "Mã Phường/Xã", errorWard);
                            message += errorWardFormat == "" ? "" : string.Format(Message.GetString("2018091303", null), "Mã Phường/Xã", errorWardFormat, "30");
                            message += errorWardName == "" ? "" : string.Format(Message.GetString("2018091301", null), "Tên Phường/Xã", errorWardName);
                            message += errorWardNameFormat == "" ? "" : string.Format(Message.GetString("2018091303", null), "Tên Phường/Xã", errorWardNameFormat, "100");
                            message += errorDuplicateWard == "" ? "" : string.Format(Message.GetString("2018091304", null), "Mã Phường/Xã", errorDuplicateWard);
                            message += errorWardUnicode == "" ? "" : string.Format(Message.GetString("2019011760", null), "Mã Phường/Xã", errorWardUnicode, "30");
                            
                            if (message == "" || message == string.Empty)
                            {
                                _db.SaveChanges();
                            }
                            Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                        }
                        return _logMessage;
                    }
                    catch(Exception ex){
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
            }
            catch(Exception ex){
                if (ex is MessageException)
                {

                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
            return _logMessage;
        }
        public bool IsUniCode(string pText)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
            return regex.IsMatch(pText);
        }
        #endregion
        #region -Export-
        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
                var ColTexts = new List<string>() { 
                                  "State", "District", "Ward", "WardName"
                                };

                var allColumns = new List<string>();
                allColumns.AddRange(ColTexts);

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = "SI24300";
                DataAccess dal = Util.Dal();
                StyleFlag flag = new StyleFlag();
            
                #region header info
                // Title header
                SetCellValue(SheetData.Cells["A1"],
                    string.Format("{0}", Util.GetLang("ExSI24300")),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetData.Cells.Merge(0, 0, 1, 6);
                Range range;
                #endregion
                for (int colIdx = 0; colIdx < ColTexts.Count; colIdx++)
                {
                    if (ColTexts[colIdx] == "State")
                    {
                        SetCellValue(SheetData.Cells[2, colIdx], Util.GetLang("ProvinceCode"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, true);
                    }
                    if (ColTexts[colIdx] == "District")
                    {
                        SetCellValue(SheetData.Cells[2, colIdx], Util.GetLang("SI24300DistrictCode"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, true);
                    }
                    if (ColTexts[colIdx] == "Ward")
                    {
                        SetCellValue(SheetData.Cells[2, colIdx], Util.GetLang("SI24300Ward"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, true);
                    }
                    if (ColTexts[colIdx] == "WardName")
                    {
                        SetCellValue(SheetData.Cells[2, colIdx], Util.GetLang("WardName"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, true);
                    }
                 
                }

                StyleFlag flag1 = new StyleFlag();
                Style colStyleCrush = SheetData.Cells[allColumns.IndexOf("State")].GetStyle();
                colStyleCrush.Font.Color = Color.Black;
                colStyleCrush.IsLocked = false;
                colStyleCrush.Number = 49;
                flag1.FontColor = true;
                flag1.NumberFormat = true;
                flag1.Locked = true;
                range = SheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("State")) + 2, Getcell(allColumns.IndexOf("State")) + 10000);
                range.ApplyStyle(colStyleCrush, flag1);

                flag1 = new StyleFlag();
                colStyleCrush = SheetData.Cells[allColumns.IndexOf("District")].GetStyle();
                colStyleCrush.Font.Color = Color.Black;
                colStyleCrush.IsLocked = false;
                colStyleCrush.Number = 49;
                flag1.FontColor = true;
                flag1.NumberFormat = true;
                flag1.Locked = true;
                range = SheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("District")) + 2, Getcell(allColumns.IndexOf("District")) + 10000);
                range.ApplyStyle(colStyleCrush, flag1);

                flag1 = new StyleFlag();
                colStyleCrush = SheetData.Cells[allColumns.IndexOf("Ward")].GetStyle();
                colStyleCrush.Font.Color = Color.Black;
                colStyleCrush.IsLocked = false;
                colStyleCrush.Number = 49;
                flag1.FontColor = true;
                flag1.NumberFormat = true;
                flag1.Locked = true;
                range = SheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("Ward")) + 2, Getcell(allColumns.IndexOf("Ward")) + 10000);
                range.ApplyStyle(colStyleCrush, flag1);

                SheetData.Cells.Columns[allColumns.IndexOf("State")].Width = 20;
                SheetData.Cells.Columns[allColumns.IndexOf("District")].Width = 30;
                SheetData.Cells.Columns[allColumns.IndexOf("Ward")].Width = 20;
                SheetData.Cells.Columns[allColumns.IndexOf("WardName")].Width = 40;

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel")
                {
                    FileDownloadName = "SI24300_Template.xlsx"
                };
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
        #endregion
        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle, bool isBackground = false)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
            {
                style.Font.Color = Color.Red;
            }
            if (isBackground)
            {
                style.Font.Color = Color.Red;
                style.Pattern = BackgroundType.Solid;
                style.ForegroundColor = Color.Yellow;
            }
            c.SetStyle(style);
        }
        private string Getcell(int column)
        {
            bool flag = false;
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 >= 1)
            {
                cell += ABC.Substring((column / 26) - 1, 1);
                column = column - 26;
                flag = true;

            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            else
            {
                if (column % 26 == 0)
                {
                    //if (flag)
                    //{
                    cell += ABC.Substring(0, 1);
                }
            }

            return cell;
        }

    }
}

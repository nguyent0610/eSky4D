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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;

namespace SI23900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI23900Controller : Controller
    {
        private string _screenNbr = "SI23900";
        private string _userName = Current.UserName;
        SI23900Entities _db = Util.CreateObjectContext<SI23900Entities>(false);
        private JsonResult _logMessage;
        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetApp_Market()
        {
            return this.Store(_db.SI23900_pgMarket(_userName, Current.CpnyID, Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstApp_Market"]);
                ChangeRecords<SI23900_pgMarket_Result> lstSI_Market = dataHandler.BatchObjectData<SI23900_pgMarket_Result>();

                lstSI_Market.Created.AddRange(lstSI_Market.Updated);

                foreach (SI23900_pgMarket_Result del in lstSI_Market.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSI_Market.Created.Where(p => p.Market == del.Market).Count() > 0)
                    {
                        lstSI_Market.Created.Where(p => p.Market == del.Market).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Market.ToList().Where(p => p.Market == del.Market).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Market.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SI23900_pgMarket_Result curLang in lstSI_Market.Created)
                {
                    if (curLang.Market.PassNull() == "") continue;

                    var lang = _db.SI_Market.Where(p => p.Market.ToLower() == curLang.Market.ToLower()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Language(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SI_Market();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SI_Market.AddObject(lang);
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

        private void Update_Language(SI_Market t, SI23900_pgMarket_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Market = s.Market;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.State = s.State;
            t.Zone = s.Zone;
            t.Territory = s.Territory;
            t.SubTerritory = s.SubTerritory;
            t.District = s.District;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;



        }


        #region -Export & import-

        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {

                Cell cell;
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = "Market";

                SetCellValueGrid(SheetData.Cells["A1"], Util.GetLang("Zone"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["B1"], Util.GetLang("Territory"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["C1"], Util.GetLang("SubTerritory"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["D1"], Util.GetLang("State"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["E1"], Util.GetLang("District"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["F1"], Util.GetLang("SI23900_MarketID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["G1"], Util.GetLang("SI23900_MarketName"), TextAlignmentType.Center, TextAlignmentType.Left);


                Style colStyle = SheetData.Cells.Columns[2].Style;
                Style colStyle1 = SheetData.Cells.Columns[3].Style;
                StyleFlag flag = new StyleFlag();
                flag.NumberFormat = true;
                colStyle.Number = 49;
                colStyle1.Number = 49;
                SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[4].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[5].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[6].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[7].ApplyStyle(colStyle, flag);


                SheetData.Cells.SetColumnWidth(0, 15);
                SheetData.Cells.SetColumnWidth(1, 15);
                SheetData.Cells.SetColumnWidth(2, 15);
                SheetData.Cells.SetColumnWidth(3, 15);
                SheetData.Cells.SetColumnWidth(4, 15);
                SheetData.Cells.SetColumnWidth(5, 15);
                SheetData.Cells.SetColumnWidth(6, 15);
                SheetData.Cells.SetColumnWidth(7, 15);


                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "Market.xlsx" };
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
        public bool IsNumber(string pValue)
        {
            foreach (Char c in pValue)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }
        public bool IsUnicode(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                int num = (int)s[i];
                if (num > 255)
                    return true;
            }
            return false;
        }
        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                var access = Session["SI23900"] as AccessRight;

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                List<SI_Market> lstSI_Martket = new List<SI_Market>();
                string message = string.Empty;
                string errorZone = string.Empty;
                string errorZonecheck = string.Empty;
                string errorTerritory = string.Empty;
                string errorTerritorycheck = string.Empty;
                string errorSubTerritory = string.Empty;
                string errorSubTerritorycheck = string.Empty;
                string errorState = string.Empty;
                string errorStatecheck = string.Empty;
                string errorDistrict = string.Empty;
                string errorDistrictcheck = string.Empty;
                string errorMarket = string.Empty;
                string errorMarketcheck = string.Empty;
                string errorDescr = string.Empty;
                string errorDescrCheck = string.Empty;
                string oktype = string.Empty;
                string MarketimportKT = string.Empty;
                string errorMarketcheckKT = string.Empty;
                string errorNull = string.Empty;
                string errorMarketImport = string.Empty;
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        if (workSheet.Cells[0, 0].StringValue.Trim() != Util.GetLang("Zone")
                          || workSheet.Cells[0, 1].StringValue.Trim() != Util.GetLang("Territory")
                          || workSheet.Cells[0, 2].StringValue.Trim() != Util.GetLang("SubTerritory")
                          || workSheet.Cells[0, 3].StringValue.Trim() != Util.GetLang("State")
                          || workSheet.Cells[0, 4].StringValue.Trim() != Util.GetLang("District")
                          || workSheet.Cells[0, 5].StringValue.Trim() != Util.GetLang("SI23900_MarketID")
                          || workSheet.Cells[0, 6].StringValue.Trim() != Util.GetLang("SI23900_MarketName")
                          || workSheet.Cells[0, 7].StringValue.Trim() != ""
                          )
                        {
                            throw new MessageException(MessageType.Message, "148");
                        }

                        bool key = false;
                        string Zoneimport = string.Empty;
                        string Territoryimport = string.Empty;
                        string SubTerritoryimport = string.Empty;
                        string Stateimport = string.Empty;
                        string Districtimport = string.Empty;
                        string Marketimport = string.Empty;
                        string Descrimport = string.Empty;
                        if (!key)
                        {

                            var lstZone = _db.SI23900_pcZone(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                            var lstTerritory = _db.SI23900_pcTerritory(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                            var lstSubTerritory = _db.SI23900_pcSubTerritory(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                            var lstState = _db.SI23900_pcState(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                            var lstDistrict = _db.SI23900_pcDistrictByState(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                            var lstMaket = _db.SI23900_pgMarket(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                            string regex = "!$^&=:;><?*%\"#{}[]\\/,`'+.|~";
                            //string regex = "\"/:*?<>|`'";
                            //string regex = "~`!#$%^&*=+/\'\,{}[]:;<>?.|";


                            if (workSheet.Cells.MaxDataRow == 0)
                            {
                                errorNull += "Dữ liệu rỗng";
                                message += errorNull == "" ? "" : string.Format(Message.GetString("14091901", null));
                            }
                            for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                            {
                                bool flagCheck = false;
                                Zoneimport = workSheet.Cells[i, 0].StringValue.PassNull();
                                Territoryimport = workSheet.Cells[i, 1].StringValue.PassNull();
                                SubTerritoryimport = workSheet.Cells[i, 2].StringValue.PassNull();
                                Stateimport = workSheet.Cells[i, 3].StringValue.PassNull();
                                Districtimport = workSheet.Cells[i, 4].StringValue.PassNull();
                                Marketimport = workSheet.Cells[i, 5].StringValue.PassNull();
                                Descrimport = workSheet.Cells[i, 6].StringValue.PassNull();
                                if (Zoneimport == "" && Territoryimport == "" && SubTerritoryimport == "" && Stateimport == "" && Districtimport == "" && Marketimport == "" && Descrimport == "")
                                {
                                    errorNull += (i + 1).ToString() + ",";
                                    message += errorNull == "" ? "" : string.Format(Message.GetString("2018040561", null), errorNull) + " ";
                                    flagCheck = true;
                                }
                                else
                                {
                                    for (int j = i + 1; j <= workSheet.Cells.MaxDataRow; j++)
                                    {
                                        MarketimportKT = workSheet.Cells[j, 5].StringValue.PassNull();
                                        if (Marketimport == MarketimportKT)
                                        {
                                            errorMarketcheckKT += (j + 1).ToString() + ", ";
                                            message += errorMarketcheckKT == "" ? "" : string.Format(Message.GetString("2018040562", null), Util.GetLang("Market"), (i + 1).ToString(), (j + 1).ToString());
                                            flagCheck = true;
                                        }
                                    }
                                }
                                int keyvalues = 0;
                                for (int j = 0; j < regex.Length;j++)
                                {
                                    char Str1 = regex[j];
                                    if (Marketimport.Contains(Str1))
                                    {
                                        keyvalues = 1;
                                    }

                                    if (IsUnicode(Marketimport) == true)
                                    {
                                        keyvalues = 1;
                                    }
                                }
                                if (keyvalues == 1)
                                {
                                    errorMarketImport += (i + 1).ToString() + ", ";
                                    flagCheck = true;
                                }
                            }
                            message += errorMarketImport == "" ? "" : string.Format(Message.GetString("2018040660", null), Util.GetLang("SI23900_MarketID"), errorMarketImport);
                            if (errorMarketcheckKT == "" && errorNull == "" && errorMarketImport == "")
                            {
                                for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                {
                                    //var keysave= 1;
                                    bool flagCheck = false;
                                    Zoneimport = workSheet.Cells[i, 0].StringValue.PassNull();
                                    Territoryimport = workSheet.Cells[i, 1].StringValue.PassNull();
                                    SubTerritoryimport = workSheet.Cells[i, 2].StringValue.PassNull();
                                    Stateimport = workSheet.Cells[i, 3].StringValue.PassNull();
                                    Districtimport = workSheet.Cells[i, 4].StringValue.PassNull();
                                    Marketimport = workSheet.Cells[i, 5].StringValue.PassNull();
                                    Descrimport = workSheet.Cells[i, 6].StringValue.PassNull();
                                    //Market
                                    if (Marketimport == "")
                                    {
                                        errorMarket += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstMaket.FirstOrDefault(p => p.Market == Marketimport) != null)
                                        {
                                            errorMarketcheck += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    //Zone
                                    if (Zoneimport == "")
                                    {
                                        errorZone += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstZone.FirstOrDefault(p => p.Code == Zoneimport) == null)
                                        {
                                            errorZonecheck += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    // Territory
                                    if (Territoryimport == "")
                                    {
                                        errorTerritory += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstTerritory.FirstOrDefault(p => p.Territory == Territoryimport && p.Zone == Zoneimport) == null)
                                        {
                                            errorTerritorycheck += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    //SubTerritory
                                    if (SubTerritoryimport == "")
                                    {
                                        errorSubTerritory += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstSubTerritory.FirstOrDefault(p => p.Code == SubTerritoryimport && p.Territory == Territoryimport) == null)
                                        {
                                            errorSubTerritorycheck += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    //State
                                    if (Stateimport == "")
                                    {
                                        errorState += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstState.FirstOrDefault(p => p.State == Stateimport && p.Territory == Territoryimport) == null)
                                        {
                                            errorStatecheck += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    //District
                                    if (Districtimport == "")
                                    {
                                        errorDistrict += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDistrict.FirstOrDefault(p => p.District == Districtimport && p.State == Stateimport) == null)
                                        {
                                            errorDistrictcheck += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    //Descr
                                    if (Descrimport == "")
                                    {
                                        errorDescr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (flagCheck == true)
                                    {
                                        continue;
                                    }
                                    var recordExists = lstSI_Martket.FirstOrDefault(p => p.Zone.ToUpper().Trim() == Zoneimport.ToUpper().Trim() && p.Territory.ToUpper().Trim() == Territoryimport.ToUpper().Trim() && p.SubTerritory.ToUpper().Trim() == SubTerritoryimport.ToUpper().Trim() && p.State.ToUpper().Trim() == Stateimport.ToUpper().Trim() && p.District.ToUpper().Trim() == Districtimport.ToUpper().Trim());
                                    if (recordExists == null)
                                    {
                                        var record = _db.SI_Market.FirstOrDefault(p => p.Market.ToUpper().Trim() == Marketimport.Trim());
                                        if (record == null)
                                        {
                                            record = new SI_Market();
                                            record.Zone = Zoneimport;
                                            record.Territory = Territoryimport;
                                            record.SubTerritory = SubTerritoryimport;
                                            record.State = Stateimport;
                                            record.District = Districtimport;
                                            record.Market = Marketimport.ToUpper();
                                            record.Descr = Descrimport;
                                            record.Crtd_Datetime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            record.LUpd_Datetime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            _db.SI_Market.AddObject(record);
                                        }
                                        else
                                        {
                                            record.LUpd_Datetime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                        }
                                    }
                                }
                            }
                        }
                        if (errorMarketcheckKT == "" && errorNull == "" && errorMarketImport == "")
                        {
                            message += errorMarket == "" ? "" : string.Format(Message.GetString("2018040560", null), Util.GetLang("SI23900_MarketID"), errorMarket) + " ";
                            message += errorMarketcheck == "" ? "" : string.Format(Message.GetString("2018040460", null), Util.GetLang("SI23900_MarketID"), errorMarketcheck) + " ";
                            message += errorZone == "" ? "" : string.Format(Message.GetString("2018040560", null), Util.GetLang("Zone"), errorZone) + " ";
                            message += errorZonecheck == "" ? "" : string.Format(Message.GetString("2018040461", null), Util.GetLang("Zone"), errorZonecheck) + " ";
                            message += errorTerritory == "" ? "" : string.Format(Message.GetString("2018040560", null), Util.GetLang("Territory"), errorTerritory) + " ";
                            message += errorTerritorycheck == "" ? "" : string.Format(Message.GetString("2018040461", null), Util.GetLang("Territory"), errorTerritorycheck) + " ";
                            message += errorSubTerritory == "" ? "" : string.Format(Message.GetString("2018040560", null), Util.GetLang("SubTerritory"), errorSubTerritory) + " ";
                            message += errorSubTerritorycheck == "" ? "" : string.Format(Message.GetString("2018040461", null), Util.GetLang("SubTerritory"), errorSubTerritorycheck) + " ";
                            message += errorState == "" ? "" : string.Format(Message.GetString("2018040560", null), Util.GetLang("State"), errorState) + " ";
                            message += errorStatecheck == "" ? "" : string.Format(Message.GetString("2018040461", null), Util.GetLang("State"), errorStatecheck) + " ";
                            message += errorDistrict == "" ? "" : string.Format(Message.GetString("2018040560", null), Util.GetLang("District"), errorDistrict) + " ";
                            message += errorDistrictcheck == "" ? "" : string.Format(Message.GetString("2018040461", null), Util.GetLang("District"), errorDistrictcheck) + " ";
                            message += errorDescr == "" ? "" : string.Format(Message.GetString("2018040560", null), Util.GetLang("SI23900_MarketName"), errorDescr) + " ";
                        }

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
        #endregion
    }
}

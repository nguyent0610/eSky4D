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
using System.Text.RegularExpressions;
using System.Drawing;
using HQFramework.DAL;
using HQFramework.Common;
namespace SI21700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21700Controller : Controller
    {
        private string _screenNbr = "SI21700";
        private string _userName = Current.UserName;
        
        SI21700Entities _db = Util.CreateObjectContext<SI21700Entities>(false);
        public JsonResult _logMessage;
        public ActionResult Index()
        {

            Util.InitRight(_screenNbr);
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            bool country = false;
            var obj = _db.SI21700_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (obj != null)
            {
                country = obj.Country.Value && obj.Country.HasValue;
            }
            ViewBag.CountryView = country;
            return View();
        }
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetSI_District()
        {
            return this.Store(_db.SI21700_pgLoadDistrict(Current.CpnyID, Current.UserName, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_District"]);
                ChangeRecords<SI21700_pgLoadDistrict_Result> lstSI_District = dataHandler.BatchObjectData<SI21700_pgLoadDistrict_Result>();
                foreach (SI21700_pgLoadDistrict_Result deleted in lstSI_District.Deleted)
                {
                    if (lstSI_District.Created.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.District == deleted.District).Count() > 0)
                    {
                        lstSI_District.Created.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.District == deleted.District).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.SI_District.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.District == deleted.District).FirstOrDefault();
                        if (del != null)
                        {
                            _db.SI_District.DeleteObject(del);
                        }
                    }
                   
                }

                lstSI_District.Created.AddRange(lstSI_District.Updated);

                foreach (SI21700_pgLoadDistrict_Result curDistrict in lstSI_District.Created)
                {
                    if (curDistrict.Country.PassNull() == "" && curDistrict.State.PassNull() == "" && curDistrict.District.PassNull() == "") continue;

                    var District = _db.SI_District.Where(p => p.Country.ToLower() == curDistrict.Country.ToLower() && p.State.ToLower() == curDistrict.State.ToLower() && p.District.ToLower() == curDistrict.District.ToLower()).FirstOrDefault();

                    if (District != null)
                    {
                        if (District.tstamp.ToHex() == curDistrict.tstamp.ToHex())
                        {
                            Update_SI_District(District, curDistrict, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        District = new SI_District();
                        Update_SI_District(District, curDistrict, true);
                        _db.SI_District.AddObject(District);
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
        private void Update_SI_District(SI_District t, SI21700_pgLoadDistrict_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.State = s.State;
                t.District = s.District;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Name = s.Name;
         
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

#region <!--Import-->
        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);

                List<SI_District> lstSI_District = new List<SI_District>(); // khai báo dữ liệu bảng
                lstSI_District = _db.SI_District.ToList();
                List<SI21700_pgLoadDistrict_Result> lstdata = new List<SI21700_pgLoadDistrict_Result>(); // khai báo dữ liệu store
                lstdata = _db.SI21700_pgLoadDistrict(Current.UserName, Current.CpnyID, Current.LangID).ToList();

                string message = string.Empty;
                string errorCountry = string.Empty;
                string errorCountryduplicate = string.Empty;
                string errorStateduplicate = string.Empty;
                string errorState = string.Empty;
                string errorDistrict = string.Empty;
                string errorDistrictRegex = string.Empty;
                string errorName = string.Empty;

                if(fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if(workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];

                            string Country = string.Empty;
                            string CountryName = string.Empty;
                            string State = string.Empty;
                            string DescrState = string.Empty;
                            string District = string.Empty;
                            string Name = string.Empty;

                            bool country = false;
                            var obj = _db.SI21700_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
                            if (obj != null)
                            {
                                country = obj.Country.Value && obj.Country.HasValue;
                            }

                            var Coltexts = new List<string>();
                            if(country == false)
                            {
                                Coltexts = new List<string>(){"State","DescrState","District","Name"};
                            }
                            else
                            {
                                Coltexts = new List<string>(){"Country","CountryName","State","DescrState","District","Name"};
                            }

                            for (int i = 4 ; i <= workSheet.Cells.MaxColumn ; i++ )
                            {
                                bool flagCheck = false;
                                if(country == false)
                                {
                                    State       = workSheet.Cells[i, Coltexts.IndexOf("State")].StringValue.PassNull();
                                    DescrState  = workSheet.Cells[i, Coltexts.IndexOf("DescrState")].StringValue.PassNull();
                                    District    = workSheet.Cells[i, Coltexts.IndexOf("District")].StringValue.PassNull();
                                    Name        = workSheet.Cells[i, Coltexts.IndexOf("Name")].StringValue.PassNull();
                                }
                                else
                                {
                                    Country     = workSheet.Cells[i, Coltexts.IndexOf("Country")].StringValue.PassNull();
                                    CountryName = workSheet.Cells[i, Coltexts.IndexOf("CountryName")].StringValue.PassNull();
                                    State       = workSheet.Cells[i, Coltexts.IndexOf("State")].StringValue.PassNull();
                                    DescrState  = workSheet.Cells[i, Coltexts.IndexOf("DescrState")].StringValue.PassNull();
                                    District    = workSheet.Cells[i, Coltexts.IndexOf("District")].StringValue.PassNull();
                                    Name        = workSheet.Cells[i, Coltexts.IndexOf("Name")].StringValue.PassNull();
                                }
                                if (State == string.Empty && District == string.Empty)
                                {
                                    continue;
                                }
                                if(country == true)
                                {
                                    if (Country == "")//Country
                                    {
                                        errorCountry += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (_db.SI_Country.FirstOrDefault(p => p.CountryID == Country) == null) // phai trùng
                                    {
                                        errorCountryduplicate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                if (State == "")//Territory
                                {
                                    errorState += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (Name == "") //DescrTerritory
                                {
                                    errorName += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (District == "") //DescrTerritory
                                {
                                    errorDistrict += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    Regex rgx = new Regex("^[a-z0-9A-Z/ ]+$");
                                    if (!rgx.IsMatch(District))
                                    {
                                        errorDistrictRegex += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                                if (_db.SI_State.FirstOrDefault(p => p.State == State) != null)// Phải trùng //State
                                {
                                    errorStateduplicate += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                
                                if (flagCheck == true)
                                {
                                    continue;
                                }
                                var record = lstSI_District.Where(p => p.State == State && p.Country == Country).FirstOrDefault();
                                if (record == null)
                                {
                                    record = new SI_District();
                                    record.ResetET();
                                    record.District = District;
                                    record.Country = Country;
                                    record.Crtd_Datetime = DateTime.Now;
                                    record.Crtd_Prog = _screenNbr;
                                    record.Crtd_User = _userName;
                                    _db.SI_District.AddObject(record);
                                }
                                record.State = State;
                                record.Name = Name;
                                record.LUpd_Datetime = DateTime.Now;
                                record.LUpd_Prog = _screenNbr;
                                record.LUpd_User = _userName;
                                lstSI_District.Add(record);
                            }
                            message = errorCountry == "" ? "" : string.Format(Message.GetString("2018102401", null), Util.GetLang("SI21700_Country"), errorCountry);
                            message += errorState == "" ? "" : string.Format(Message.GetString("2018102401", null), Util.GetLang("SI21700_State"), errorState);
                            message += errorDistrict == "" ? "" : string.Format(Message.GetString("2018102401", null), Util.GetLang("SI21700_District"), errorDistrict);
                            message += errorName == "" ? "" : string.Format(Message.GetString("2018102401", null), Util.GetLang("SI21700_Name"), errorName);
                            message += errorCountryduplicate == "" ? "" : string.Format(Message.GetString("2018102402", null), Util.GetLang("SI21700_Country"), errorCountryduplicate);
                            message += errorStateduplicate == "" ? "" : string.Format(Message.GetString("2018102402", null), Util.GetLang("SI21700_State"), errorStateduplicate);  //không được nhập ký tự đặc biệt
                            message += errorDistrictRegex == "" ? "" : string.Format(Message.GetString("2018102403", null), Util.GetLang("SI21700_District"), errorDistrictRegex);
                                        
                            if (message == "" || message == string.Empty)
                            {
                                _db.SaveChanges();
                            }
                            Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                        }
                        return _logMessage;
                    }
                    catch(Exception ex)
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

            }
            catch(Exception ex)
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
            return _logMessage;
        }
#endregion //end  -Import-

#region -export-


        public ActionResult Export(FormCollection data)
        {
            try
            {
                //StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_CabinetChecking"]);
                //var lstOM_EquipmentStatus = dataHandler.ObjectData<OM29200_pgOM_CabinetChecking_Result>();

                Cell cell;
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = "SI21700NameSheet";
                DataAccess dal = Util.Dal();

                SheetData.Cells.SetRowHeight(0, 40);
                Style colStyle = SheetData.Cells.Columns[1].Style;
                Style colStyle1 = SheetData.Cells.Columns[2].Style;
                Range range;
                StyleFlag flag = new StyleFlag();
                flag.NumberFormat = true;
                colStyle.Number = 49;
                colStyle1.Number = 49;

                bool country = false;
                var obj = _db.SI21700_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
                if (obj != null)
                {
                    country = obj.Country.Value && obj.Country.HasValue;
                }
if (country == false)
                {
                    SetCellValueGridTitle(SheetData.Cells["B1"], Util.GetLang("SI21700Exp"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["A3"], Util.GetLang("SI21700_State"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["B3"], Util.GetLang("SI21700_DescrState"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["C3"], Util.GetLang("SI21700_District"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["D3"], Util.GetLang("SI21700_Name"), TextAlignmentType.Center, TextAlignmentType.Left);

                    SheetData.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);


                    SheetData.Cells.SetColumnWidth(0, 22);
                    SheetData.Cells.SetColumnWidth(1, 25);
                    SheetData.Cells.SetColumnWidth(2, 40);
                    SheetData.Cells.SetColumnWidth(3, 40);



                    #region load mã Tinh - tên Tinh

                    ParamCollection State = new ParamCollection();
                    State = new ParamCollection();
                    State.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                    State.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                    State.Add(new ParamStruct("@LangID", DbType.String, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));
                    DataTable dtStateAll = dal.ExecDataTable("SI21700_pcStateAll", CommandType.StoredProcedure, ref State);
                    SheetData.Cells.ImportDataTable(dtStateAll, true, 0, 43, false); // số dòng lưu trữ

                    Style colStyleState = SheetData.Cells.Columns[3].Style;
                    colStyleState = SheetData.Cells["AR1"].GetStyle();
                    colStyleState.Font.Color = Color.White;
                    flag.FontColor = true;
                    flag.NumberFormat = true;

                    Range rangeterritory;
                    rangeterritory = SheetData.Cells.CreateRange("AR1", "BD" + (dtStateAll.Rows.Count + 100));
                    rangeterritory.ApplyStyle(colStyleState, flag);

                    int commentState = SheetData.Comments.Add("S3");
                    Validation validationState = SheetData.Validations[SheetData.Validations.Add()];
                    validationState.IgnoreBlank = true;
                    validationState.Type = Aspose.Cells.ValidationType.List;
                    validationState.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationState.Operator = OperatorType.Between;

                    string formulaState = "=$AR$2:$AR$" + (dtStateAll.Rows.Count + 2);

                    validationState = SheetData.Validations[SheetData.Validations.Add()];
                    validationState.Type = Aspose.Cells.ValidationType.List;
                    validationState.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationState.Formula1 = formulaState;
                    validationState.InputTitle = "";
                    validationState.IgnoreBlank = true;
                    validationState.Operator = OperatorType.Between;
                    validationState.InputMessage = "Chọn mã tỉnh";
                    validationState.ErrorMessage = "Mã tỉnh không tồn tại";

                    var areaState = new CellArea();
                    areaState.StartRow = 3;
                    areaState.EndRow = dtStateAll.Rows.Count + 100;
                    areaState.StartColumn = 0;
                    areaState.EndColumn = 0;
                    validationState.AddArea(areaState);
                    //load ten vung theo ma vùng
                    string TerritoryName = string.Format("=IF(ISERROR(VLOOKUP({0},AR:AS,2,0)),\"\",VLOOKUP({0},AR:AS,2,0))", "A4");
                    SheetData.Cells["B4"].SetSharedFormula(TerritoryName, (dtStateAll.Rows.Count + 100), 1);


                    Validation validationTer = SheetData.Validations[SheetData.Validations.Add()];
                    validationTer.IgnoreBlank = true;
                    validationTer.Type = Aspose.Cells.ValidationType.List;
                    validationTer.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationTer.Operator = OperatorType.Between;

         #endregion //end mã tỉnh - tên tỉnh

                    Style colStyleAddter = SheetData.Cells.Columns[2].Style;
                    colStyleAddter = SheetData.Cells["B4"].GetStyle();
                    colStyleAddter.Font.Color = Color.Green;
                    flag.FontColor = true;
                    range = SheetData.Cells.CreateRange("B4", "B" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleAddter, flag);

                    Style colStyleClassID = SheetData.Cells.Columns[0].Style;
                    colStyleClassID = SheetData.Cells["A3"].GetStyle();
                    colStyleClassID.IsLocked = false;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = true;
                    range = SheetData.Cells.CreateRange("A3", "A" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleClassID, flag);

                    Style colStyleC = SheetData.Cells.Columns[0].Style;
                    colStyleC = SheetData.Cells["C3"].GetStyle();
                    colStyleC.IsLocked = false;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = true;
                    range = SheetData.Cells.CreateRange("C3", "C" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleC, flag);

                    Style colStyleE = SheetData.Cells.Columns[0].Style;
                    colStyleE = SheetData.Cells["D3"].GetStyle();
                    colStyleE.IsLocked = false;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = true;
                    range = SheetData.Cells.CreateRange("D3", "D" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleE, flag);


                }
else
                {

                    SetCellValueGridTitle(SheetData.Cells["C1"], Util.GetLang("SI20700NameSheet"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["A3"], Util.GetLang("SI21700_Country"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["B3"], Util.GetLang("SI21700_NameCountry"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["C3"], Util.GetLang("SI21700_State"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["D3"], Util.GetLang("SI21700_DescrState"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["E3"], Util.GetLang("SI21700_District"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["F3"], Util.GetLang("SI21700_Name"), TextAlignmentType.Center, TextAlignmentType.Left);

                    SheetData.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[5].ApplyStyle(colStyle, flag);


                    SheetData.Cells.SetColumnWidth(0, 15);
                    SheetData.Cells.SetColumnWidth(1, 25);
                    SheetData.Cells.SetColumnWidth(2, 22);
                    SheetData.Cells.SetColumnWidth(3, 25);
                    SheetData.Cells.SetColumnWidth(4, 40);
                    SheetData.Cells.SetColumnWidth(5, 40);

                    #region load đất nước

                    ParamCollection pc = new ParamCollection();
                    pc = new ParamCollection();
                    DataTable dtCountry = dal.ExecDataTable("SI21700_pcCountryAll", CommandType.StoredProcedure, ref pc);
                    SheetData.Cells.ImportDataTable(dtCountry, true, 0, 40, false);

                    Validation validation = SheetData.Validations[SheetData.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.Between;

                    string formulaCountry = "=$AO$2:$AO$" + (dtCountry.Rows.Count + 2);
                    validation = SheetData.Validations[SheetData.Validations.Add()];
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Formula1 = formulaCountry;
                    validation.InputTitle = "";
                    validation.IgnoreBlank = true;
                    validation.Operator = OperatorType.Between;
                    validation.InputMessage = "Chọn mã đất nước";
                    validation.ErrorMessage = "Mã đất nước này không tồn tại";

                    var area = new CellArea();
                    area.StartRow = 3;
                    area.EndRow = dtCountry.Rows.Count + 100;
                    area.StartColumn = 0;
                    area.EndColumn = 0;
                    validation.AddArea(area);



                    //Load tên Đất nước theo đất nước
                    string CountryName = string.Format("=IF(ISERROR(VLOOKUP({0},AO:AP,2,0)),\"\",VLOOKUP({0},AO:AP,2,0))", "A4");
                    SheetData.Cells["B4"].SetSharedFormula(CountryName, (dtCountry.Rows.Count + 100), 1);
                    Validation validationCountry = SheetData.Validations[SheetData.Validations.Add()];
                    validationCountry.IgnoreBlank = true;
                    validationCountry.Type = Aspose.Cells.ValidationType.List;
                    validationCountry.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationCountry.Operator = OperatorType.Between;



                    #endregion //end đất nước

                    #region load mã vùng - tên vùng

                    ParamCollection state = new ParamCollection();
                    state = new ParamCollection();
                    state.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                    state.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                    state.Add(new ParamStruct("@LangID", DbType.String, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));
                    DataTable dtStateAll = dal.ExecDataTable("SI21700_pcStateAll", CommandType.StoredProcedure, ref state);
                    SheetData.Cells.ImportDataTable(dtStateAll, true, 0, 43, false); // số dòng lưu trữ

                    Style colStyleState = SheetData.Cells.Columns[3].Style;
                    colStyleState = SheetData.Cells["AR1"].GetStyle();
                    colStyleState.Font.Color = Color.White;
                    flag.FontColor = true;
                    flag.NumberFormat = true;

                    Range rangeState;
                    rangeState = SheetData.Cells.CreateRange("AR1", "BD" + (dtStateAll.Rows.Count + 100));
                    rangeState.ApplyStyle(colStyleState, flag);

                    int commentState = SheetData.Comments.Add("S3");
                    Validation validationState = SheetData.Validations[SheetData.Validations.Add()];
                    validationState.IgnoreBlank = true;
                    validationState.Type = Aspose.Cells.ValidationType.List;
                    validationState.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationState.Operator = OperatorType.Between;

                    string formulaTerritory = "=$AR$2:$AR$" + (dtStateAll.Rows.Count + 2);

                    validationState = SheetData.Validations[SheetData.Validations.Add()];
                    validationState.Type = Aspose.Cells.ValidationType.List;
                    validationState.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationState.Formula1 = formulaTerritory;
                    validationState.InputTitle = "";
                    validationState.IgnoreBlank = true;
                    validationState.Operator = OperatorType.Between;
                    validationState.InputMessage = "Chọn mã tỉnh ";
                    validationState.ErrorMessage = "Mã tỉnh không tồn tại";

                    var areaState = new CellArea();
                    areaState.StartRow = 3;
                    areaState.EndRow = dtStateAll.Rows.Count + 100;
                    areaState.StartColumn = 2;
                    areaState.EndColumn = 2;
                    validationState.AddArea(areaState);
                    //load ten vung theo ma vùng
                    string StateName = string.Format("=IF(ISERROR(VLOOKUP({0},AR:AS,2,0)),\"\",VLOOKUP({0},AR:AS,2,0))", "C4");
                    SheetData.Cells["D4"].SetSharedFormula(StateName, (dtStateAll.Rows.Count + 100), 1);


                    Validation validationTer = SheetData.Validations[SheetData.Validations.Add()];
                    validationTer.IgnoreBlank = true;
                    validationTer.Type = Aspose.Cells.ValidationType.List;
                    validationTer.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationTer.Operator = OperatorType.Between;

                    #endregion //end mã vùng - tên vùng

                    Style colStyle2 = SheetData.Cells.Columns[1].Style;
                    colStyle2 = SheetData.Cells["AO1"].GetStyle();
                    colStyle2.Font.Color = Color.White;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = false;
                    range = SheetData.Cells.CreateRange("AO1", "BS" + (dtCountry.Rows.Count + 100));
                    range.ApplyStyle(colStyle2, flag);
                    int commentIndex = SheetData.Comments.Add("R3");

                    StyleFlag flagA = new StyleFlag();
                    Style colStyleA = SheetData.Cells.Columns[2].Style;
                    colStyleA = SheetData.Cells["A4"].GetStyle();
                    flagA.FontColor = true;
                    colStyleA.Font.Color = Color.Black;
                    range = SheetData.Cells.CreateRange("A4", "A" + (dtCountry.Rows.Count + 100));
                    range.ApplyStyle(colStyleA, flagA);

                    StyleFlag flag1 = new StyleFlag();
                    Style colStyleAdd = SheetData.Cells.Columns[2].Style;
                    colStyleAdd = SheetData.Cells["B4"].GetStyle();
                    colStyleAdd.Font.Color = Color.Green;
                    flag1.FontColor = true;
                    range = SheetData.Cells.CreateRange("B4", "B" + (dtCountry.Rows.Count + 100));
                    range.ApplyStyle(colStyleAdd, flag1);

                    StyleFlag flagTER = new StyleFlag();
                    Style colStyleAddter = SheetData.Cells.Columns[2].Style;
                    colStyleAdd = SheetData.Cells["D4"].GetStyle();
                    colStyleAdd.Font.Color = Color.Green;
                    flagTER.FontColor = true;
                    rangeState = SheetData.Cells.CreateRange("D4", "D" + (dtStateAll.Rows.Count + 100));
                    rangeState.ApplyStyle(colStyleAdd, flagTER);

                    Style colStyleClassID = SheetData.Cells.Columns[0].Style;
                    colStyleClassID = SheetData.Cells["A4"].GetStyle();
                    colStyleClassID.IsLocked = false;
                    flag1.FontColor = true;
                    colStyleClassID.Font.Color = Color.Black;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("A4", "A" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleClassID, flag1);

                    Style colStyleC = SheetData.Cells.Columns[0].Style;
                    colStyleC = SheetData.Cells["C4"].GetStyle();
                    colStyleC.IsLocked = false;
                    colStyleC.Font.Color = Color.Black;
                    flag1.FontColor = true;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("C4", "C" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleC, flag1);

                    Style colStyleE = SheetData.Cells.Columns[0].Style;
                    colStyleE = SheetData.Cells["E4"].GetStyle();
                    colStyleE.Font.Color = Color.Black;
                    colStyleE.IsLocked = false;
                    flag1.FontColor = true;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("E4", "E" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleE, flag1);

                    Style colStyleF = SheetData.Cells.Columns[0].Style;
                    colStyleF = SheetData.Cells["F4"].GetStyle();
                    colStyleF.Font.Color = Color.Black;
                    colStyleF.IsLocked = false;
                    flag1.FontColor = true;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("F4", "F" + (dtStateAll.Rows.Count + 100));
                    range.ApplyStyle(colStyleF, flag1);

                }


                #region lưu excel
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel")
                {
                    FileDownloadName = "SI21700_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xlsx"
                };
                #endregion end lưu
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
        private void SetCellValueGridTitle(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 20;
            style.Font.Color = Color.Blue;
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

            style.Font.Color = Color.Blue;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }
        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 11;
            style.Font.Color = Color.White;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }

#endregion //end -export-
    }
}

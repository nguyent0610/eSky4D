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
using Aspose.Cells;
using System.Text;
using HQFramework.DAL;
using System.Drawing;
using System.Text.RegularExpressions;
using HQFramework.Common;
namespace SI20700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20700Controller : Controller
    {
        private string _screenNbr = "SI20700";
        private string _userName = Current.UserName;
        SI20700Entities _db = Util.CreateObjectContext<SI20700Entities>(false);
        private JsonResult _logMessage;
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            bool province = false;
            bool country = false;
            var obj = _db.SI20700_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (obj != null)
            {
                province = obj.Province.Value && obj.Province.HasValue;
                country = obj.Country.Value && obj.Country.HasValue;
            }
            ViewBag.ProvinceView = province;
            ViewBag.CountryView = country;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetData()
        {
            return this.Store(_db.SI20700_pgLoadState(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SI20700_pgLoadState_Result> lstData = dataHandler.BatchObjectData<SI20700_pgLoadState_Result>();

                lstData.Created.AddRange(lstData.Updated);
                foreach (SI20700_pgLoadState_Result del in lstData.Deleted)
                {
                    if (lstData.Created.Any(p => p.Country.ToLower().Trim() == del.Country.ToLower().Trim() && p.State.ToLower().Trim() == del.State.ToLower().Trim()))// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.Country.ToLower().Trim() == del.Country.ToLower().Trim() && p.State.ToLower().Trim() == del.State.ToLower().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_State.ToList().Where(p => p.Country.ToLower().Trim() == del.Country.ToLower().Trim() && p.State.ToLower().Trim() == del.State.ToLower().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_State.DeleteObject(objDel);
                        }
                    }
                }


                foreach (SI20700_pgLoadState_Result curItem in lstData.Created)
                {
                    if (curItem.Country.PassNull() == "" && curItem.State.PassNull() == "") continue;

                    var State = _db.SI_State.Where(p => p.Country.ToLower() == curItem.Country.ToLower() && p.State.ToLower() == curItem.State.ToLower()).FirstOrDefault();

                    if (State != null)
                    {
                        if (State.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_SI_State(State, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        State = new SI_State();
                        Update_SI_State(State, curItem, true);
                        _db.SI_State.AddObject(State);
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

        private void Update_SI_State(SI_State t, SI20700_pgLoadState_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.State = s.State;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Territory = s.Territory;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }


     
#region ExPort

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
                SheetData.Name = "SI20700NameSheet";
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
                var obj = _db.SI20700_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
                if (obj != null)
                {
                    country = obj.Country.Value && obj.Country.HasValue;
                }  
if (country == false)
                {
                    SetCellValueGridTitle(SheetData.Cells["B1"], Util.GetLang("SI20700Exp"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["A2"], Util.GetLang("SI20700_Territory"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["B2"], Util.GetLang("DescrTerritory"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["C2"], Util.GetLang("SI20700_State"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["D2"], Util.GetLang("SI20700_Descr"), TextAlignmentType.Center, TextAlignmentType.Left);

                    SheetData.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);


                    SheetData.Cells.SetColumnWidth(0, 22);
                    SheetData.Cells.SetColumnWidth(1, 25);
                    SheetData.Cells.SetColumnWidth(2, 22);
                    SheetData.Cells.SetColumnWidth(3, 25);



                    #region load mã vùng - tên vùng

                    ParamCollection TER = new ParamCollection();
                    TER = new ParamCollection();
                    TER.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                    TER.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                    TER.Add(new ParamStruct("@LangID", DbType.String, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));
                    DataTable dtTerritory = dal.ExecDataTable("SI20700_pcLoadTerritory", CommandType.StoredProcedure, ref TER);
                    SheetData.Cells.ImportDataTable(dtTerritory, true, 0, 43, false); // số dòng lưu trữ

                    Style colStyleTerritory = SheetData.Cells.Columns[3].Style;
                    colStyleTerritory = SheetData.Cells["AR1"].GetStyle();
                    colStyleTerritory.Font.Color = Color.White;
                    flag.FontColor = true;
                    flag.NumberFormat = true;

                    Range rangeterritory;
                    rangeterritory = SheetData.Cells.CreateRange("AR1", "BD" + (dtTerritory.Rows.Count + 100));
                    rangeterritory.ApplyStyle(colStyleTerritory, flag);

                    int commentTerritory = SheetData.Comments.Add("S3");
                    Validation validationTerritory = SheetData.Validations[SheetData.Validations.Add()];
                    validationTerritory.IgnoreBlank = true;
                    validationTerritory.Type = Aspose.Cells.ValidationType.List;
                    validationTerritory.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationTerritory.Operator = OperatorType.Between;

                    string formulaTerritory = "=$AR$2:$AR$" + (dtTerritory.Rows.Count + 2);

                    validationTerritory = SheetData.Validations[SheetData.Validations.Add()];
                    validationTerritory.Type = Aspose.Cells.ValidationType.List;
                    validationTerritory.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationTerritory.Formula1 = formulaTerritory;
                    validationTerritory.InputTitle = "";
                    validationTerritory.IgnoreBlank = true;
                    validationTerritory.Operator = OperatorType.Between;
                    validationTerritory.InputMessage = "Chọn mã vùng ";
                    validationTerritory.ErrorMessage = "Mã vùng này không tồn tại";

                    var areaTerritory = new CellArea();
                    areaTerritory.StartRow = 2;
                    areaTerritory.EndRow = dtTerritory.Rows.Count + 100;
                    areaTerritory.StartColumn = 0;
                    areaTerritory.EndColumn = 0;
                    validationTerritory.AddArea(areaTerritory);
                    //load ten vung theo ma vùng
                    string TerritoryName = string.Format("=IF(ISERROR(VLOOKUP({0},AR:AS,2,0)),\"\",VLOOKUP({0},AR:AS,2,0))", "A3");
                    SheetData.Cells["B3"].SetSharedFormula(TerritoryName, (dtTerritory.Rows.Count + 100), 1);


                    Validation validationTer = SheetData.Validations[SheetData.Validations.Add()];
                    validationTer.IgnoreBlank = true;
                    validationTer.Type = Aspose.Cells.ValidationType.List;
                    validationTer.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationTer.Operator = OperatorType.Between;

                    #endregion //end mã vùng - tên vùng



                    //  StyleFlag flag1 = new StyleFlag();
                    Style colStyleAddter = SheetData.Cells.Columns[2].Style;
                    colStyleAddter = SheetData.Cells["B3"].GetStyle();
                    colStyleAddter.Font.Color = Color.Green;
                    flag.FontColor = true;
                    range = SheetData.Cells.CreateRange("B3", "B" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleAddter, flag);

                    Style colStyleClassID = SheetData.Cells.Columns[0].Style;
                    colStyleClassID = SheetData.Cells["A3"].GetStyle();
                    colStyleClassID.IsLocked = false;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = true;
                    range = SheetData.Cells.CreateRange("A3", "A" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleClassID, flag);

                    Style colStyleC = SheetData.Cells.Columns[0].Style;
                    colStyleC = SheetData.Cells["C3"].GetStyle();
                    colStyleC.IsLocked = false;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = true;
                    range = SheetData.Cells.CreateRange("C3", "C" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleC, flag);

                    Style colStyleE = SheetData.Cells.Columns[0].Style;
                    colStyleE = SheetData.Cells["D3"].GetStyle();
                    colStyleE.IsLocked = false;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = true;
                    range = SheetData.Cells.CreateRange("D3", "D" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleE, flag);


                }
else
                {

                    SetCellValueGridTitle(SheetData.Cells["C1"], Util.GetLang("SI20700NameSheet"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["A2"], Util.GetLang("SI20700_Country"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["B2"], Util.GetLang("SI20700_NameCountry"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["C2"], Util.GetLang("SI20700_Territory"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["D2"], Util.GetLang("DescrTerritory"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["E2"], Util.GetLang("SI20700_State"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData.Cells["F2"], Util.GetLang("SI20700_Descr"), TextAlignmentType.Center, TextAlignmentType.Left);

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
                    SheetData.Cells.SetColumnWidth(4, 22);
                    SheetData.Cells.SetColumnWidth(5, 25);

                    #region load đất nước

                    ParamCollection pc = new ParamCollection();
                    pc = new ParamCollection();
                    DataTable dtCountry = dal.ExecDataTable("SI20700_pcLoadCountryAll", CommandType.StoredProcedure, ref pc);
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
                    area.StartRow = 2;
                    area.EndRow = dtCountry.Rows.Count + 100;
                    area.StartColumn = 0;
                    area.EndColumn = 0;
                    validation.AddArea(area);



                    //Load tên Đất nước theo đất nước
                    string CountryName = string.Format("=IF(ISERROR(VLOOKUP({0},AO:AP,2,0)),\"\",VLOOKUP({0},AO:AP,2,0))", "A3");
                    SheetData.Cells["B3"].SetSharedFormula(CountryName, (dtCountry.Rows.Count + 100), 1);
                    Validation validationCustID = SheetData.Validations[SheetData.Validations.Add()];
                    validationCustID.IgnoreBlank = true;
                    validationCustID.Type = Aspose.Cells.ValidationType.List;
                    validationCustID.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationCustID.Operator = OperatorType.Between;



                    #endregion //end đất nước

                    #region load mã vùng - tên vùng

                    ParamCollection TER = new ParamCollection();
                    TER = new ParamCollection();
                    TER.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                    TER.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                    TER.Add(new ParamStruct("@LangID", DbType.String, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));
                    DataTable dtTerritory = dal.ExecDataTable("SI20700_pcLoadTerritory", CommandType.StoredProcedure, ref TER);
                    SheetData.Cells.ImportDataTable(dtTerritory, true, 0, 43, false); // số dòng lưu trữ

                    Style colStyleTerritory = SheetData.Cells.Columns[3].Style;
                    colStyleTerritory = SheetData.Cells["AR1"].GetStyle();
                    colStyleTerritory.Font.Color = Color.White;
                    flag.FontColor = true;
                    flag.NumberFormat = true;

                    Range rangeterritory;
                    rangeterritory = SheetData.Cells.CreateRange("AR1", "BD" + (dtTerritory.Rows.Count + 100));
                    rangeterritory.ApplyStyle(colStyleTerritory, flag);

                    int commentTerritory = SheetData.Comments.Add("S3");
                    Validation validationTerritory = SheetData.Validations[SheetData.Validations.Add()];
                    validationTerritory.IgnoreBlank = true;
                    validationTerritory.Type = Aspose.Cells.ValidationType.List;
                    validationTerritory.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationTerritory.Operator = OperatorType.Between;

                    string formulaTerritory = "=$AR$2:$AR$" + (dtTerritory.Rows.Count + 2);

                    validationTerritory = SheetData.Validations[SheetData.Validations.Add()];
                    validationTerritory.Type = Aspose.Cells.ValidationType.List;
                    validationTerritory.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validationTerritory.Formula1 = formulaTerritory;
                    validationTerritory.InputTitle = "";
                    validationTerritory.IgnoreBlank = true;
                    validationTerritory.Operator = OperatorType.Between;
                    validationTerritory.InputMessage = "Chọn mã vùng ";
                    validationTerritory.ErrorMessage = "Mã vùng này không tồn tại";

                    var areaTerritory = new CellArea();
                    areaTerritory.StartRow = 2;
                    areaTerritory.EndRow = dtTerritory.Rows.Count + 100;
                    areaTerritory.StartColumn = 2;
                    areaTerritory.EndColumn = 2;
                    validationTerritory.AddArea(areaTerritory);
                    //load ten vung theo ma vùng
                    string TerritoryName = string.Format("=IF(ISERROR(VLOOKUP({0},AR:AS,2,0)),\"\",VLOOKUP({0},AR:AS,2,0))", "C3");
                    SheetData.Cells["D3"].SetSharedFormula(TerritoryName, (dtTerritory.Rows.Count + 100), 1);


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
                    colStyleA = SheetData.Cells["A3"].GetStyle();
                    flagA.FontColor = true;
                    range = SheetData.Cells.CreateRange("A3", "A" + (dtCountry.Rows.Count + 100));
                    range.ApplyStyle(colStyleA, flagA);

                    StyleFlag flag1 = new StyleFlag();
                    Style colStyleAdd = SheetData.Cells.Columns[2].Style;
                    colStyleAdd = SheetData.Cells["B3"].GetStyle();
                    colStyleAdd.Font.Color = Color.Green;
                    flag1.FontColor = true;
                    range = SheetData.Cells.CreateRange("B3", "B" + (dtCountry.Rows.Count + 100));
                    range.ApplyStyle(colStyleAdd, flag1);

                    StyleFlag flagTER = new StyleFlag();
                    Style colStyleAddter = SheetData.Cells.Columns[2].Style;
                    colStyleAdd = SheetData.Cells["D3"].GetStyle();
                    colStyleAdd.Font.Color = Color.Green;
                    flagTER.FontColor = true;
                    rangeterritory = SheetData.Cells.CreateRange("D3", "D" + (dtTerritory.Rows.Count + 100));
                    rangeterritory.ApplyStyle(colStyleAdd, flagTER);

                    Style colStyleClassID = SheetData.Cells.Columns[0].Style;
                    colStyleClassID = SheetData.Cells["A3"].GetStyle();
                    colStyleClassID.IsLocked = false;
                    flag1.FontColor = true;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("A3", "A" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleClassID, flag1);

                    Style colStyleC = SheetData.Cells.Columns[0].Style;
                    colStyleC = SheetData.Cells["C3"].GetStyle();
                    colStyleC.IsLocked = false;
                    flag1.FontColor = true;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("C3", "C" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleC, flag1);

                    Style colStyleE = SheetData.Cells.Columns[0].Style;
                    colStyleE = SheetData.Cells["E3"].GetStyle();
                    colStyleE.IsLocked = false;
                    flag1.FontColor = true;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("E3", "E" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleE, flag1);

                    Style colStyleF = SheetData.Cells.Columns[0].Style;
                    colStyleF = SheetData.Cells["F3"].GetStyle();
                    colStyleF.IsLocked = false;
                    flag1.FontColor = true;
                    flag1.NumberFormat = true;
                    flag1.Locked = true;
                    range = SheetData.Cells.CreateRange("F3", "F" + (dtTerritory.Rows.Count + 100));
                    range.ApplyStyle(colStyleF, flag1);

                }


#region lưu excel
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel")
                {
                    FileDownloadName = "SI20700_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xlsx"
                };
                //SheetData.Protect(ProtectionType.All);
                //var fileName = SheetData.Name + DateTime.Now.ToString("yyyyMMddHHmm") + ".xlsx";
                //string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
                //workbook.Settings.AutoRecover = true;
                //workbook.Save(fullPath, SaveFormat.Xlsx);
                //return Json(new { success = true, fileName = fileName, errorMessage = "" });
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
         private void SetCellValueGridTitle (Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
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

        #endregion // end export

#region -Import-
        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                List<SI_State> lstSI_State = new List<SI_State>();
                lstSI_State = _db.SI_State.ToList();
                List<SI_Territory> lstSI_Territory = new List<SI_Territory>();                
                List<SI20700_pgLoadState_Result> lstdata = new List<SI20700_pgLoadState_Result>();
                lstdata = _db.SI20700_pgLoadState(Current.UserName,Current.CpnyID,Current.LangID).ToList();  
           
                string message = string.Empty;
                string errorCountry             = string.Empty; 
                string errorTerritory           = string.Empty;
                string errorDescrTerritory      = string.Empty;
                string errorState               = string.Empty;
                string errorStateRegex          = string.Empty; //ký tự tỉnh
                string errorStateDescr          = string.Empty;
                string errorData                = string.Empty; // data not null
                string errorCountryduplicate    = string.Empty;
                string errorTerritoryduplicate  = string.Empty;
                string errorStateduplicate      = string.Empty;


                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];

                            string Country = string.Empty;                  // Mã đất nước
                            string CountryName = string.Empty;             // Tên đất nước
                            string Territory = string.Empty;              // mã vùng
                            string DescrTerritory = string.Empty;        // tên vùng
                            string State = string.Empty;                // mã tỉnh
                            string StateDescr = string.Empty;          // tên tỉnh 

                            bool country = false;
                            var obj = _db.SI20700_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
                            if (obj != null)
                            {
                                country = obj.Country.Value && obj.Country.HasValue;
                            }    
                            var ColTexts = new List<string>();
                            if (country == false)
                            {
                                ColTexts = new List<string>(){ 
                                     "Territory", "DescrTerritory", "State", "StateDescr"
                                    };
                            }
                            else
                            {
                                ColTexts = new List<string>(){ 
                                    "Country","CountryName", "Territory", "DescrTerritory", "State", "StateDescr"
                                    };
                            }
                            for (int i = 2; i <= workSheet.Cells.MaxDataRow; i++)  //workSheet.Cells.MaxDataRow -> đếm từ  0 , 1 , 2 , .... còn i đếm từ  1 , 2 , 3 , ....
                            {
                                
                                bool flagCheck = false;

                               if (country == false)
                                {
                                    Territory = workSheet.Cells[i, ColTexts.IndexOf("Territory")].StringValue.PassNull();
                                    DescrTerritory = workSheet.Cells[i, ColTexts.IndexOf("DescrTerritory")].StringValue.PassNull();
                                    State = workSheet.Cells[i, ColTexts.IndexOf("State")].StringValue.PassNull();
                                    StateDescr = workSheet.Cells[i, ColTexts.IndexOf("StateDescr")].StringValue.PassNull();
                               }
                                else
                                {
                                    Country = workSheet.Cells[i, ColTexts.IndexOf("Country")].StringValue.PassNull();
                                    CountryName = workSheet.Cells[i, ColTexts.IndexOf("CountryName")].StringValue.PassNull();
                                    Territory = workSheet.Cells[i, ColTexts.IndexOf("Territory")].StringValue.PassNull();
                                    DescrTerritory = workSheet.Cells[i, ColTexts.IndexOf("DescrTerritory")].StringValue.PassNull();
                                    State = workSheet.Cells[i, ColTexts.IndexOf("State")].StringValue.PassNull();
                                    StateDescr = workSheet.Cells[i, ColTexts.IndexOf("StateDescr")].StringValue.PassNull();
                                }
                                if (Territory == string.Empty && State == string.Empty)
                                {
                                    continue;
                                }
                                if (Country == "")//Country
                                {
                                    errorCountry += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (_db.SI_Country.FirstOrDefault(p => p.CountryID == Country) == null) // không trùng
                                {
                                    errorCountryduplicate += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (Territory == "")//Territory
                                {
                                    errorTerritory += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (DescrTerritory == "") //DescrTerritory
                                {
                                    errorDescrTerritory += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (_db.SI_Territory.FirstOrDefault(p => p.Territory == Territory && p.Descr == DescrTerritory) == null) // không trùng
                                {
                                    errorTerritoryduplicate += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (_db.SI_State.FirstOrDefault(p => p.State == State) != null)// Phải trùng //State
                                {
                                    errorStateduplicate += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                if (State == "")
                                {
                                    errorState += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    Regex rgx = new Regex("^[a-z0-9A-Z/ ]+$");
                                    if (!rgx.IsMatch(State))
                                    {
                                        errorStateRegex += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                } 
                                if (StateDescr == "")//StateDescr
                                {
                                    errorStateDescr += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                } 
                                if (flagCheck == true)
                                {
                                    continue;
                                }
 #region save SI_State                         
                                var record = lstSI_State.Where(p => p.State == State && p.Country == Country).FirstOrDefault();
                                if (record == null)
                                {
                                    record = new SI_State();
                                    record.ResetET();
                                    record.State = State;
                                    record.Country = Country;
                                    record.Crtd_Datetime = DateTime.Now;
                                    record.Crtd_Prog = _screenNbr;
                                    record.Crtd_User = _userName;
                                    _db.SI_State.AddObject(record);
                                }
                                record.Territory = Territory;
                                record.Descr = StateDescr;
                                record.LUpd_Datetime = DateTime.Now;
                                record.LUpd_Prog = _screenNbr;
                                record.LUpd_User = _userName;
                                lstSI_State.Add(record);
 #endregion
                            }
                            message = errorCountry == "" ? "" : string.Format(Message.GetString("2018101701", null), Util.GetLang("SI20700_Country"), errorCountry);
                            message += errorTerritory == "" ? "" : string.Format(Message.GetString("2018101701", null), Util.GetLang("SI20700_Territory"), errorTerritory);
                            message += errorDescrTerritory == "" ? "" : string.Format(Message.GetString("2018101701", null), Util.GetLang("DescrTerritory"), errorDescrTerritory);  //không được nhập ký tự đặc biệt
                            message += errorState == "" ? "" : string.Format(Message.GetString("2018101701", null), Util.GetLang("SI20700_State"), errorState);
                            message += errorStateRegex == "" ? "" : string.Format(Message.GetString("2018101702", null), Util.GetLang("SI20700_State"), errorStateRegex);
                            message += errorStateDescr == "" ? "" : string.Format(Message.GetString("2018101701", null), Util.GetLang("SI20700_Descr"), errorStateDescr);
                            message += errorData == "" ? "" : string.Format(Message.GetString("2018101703", null), Util.GetLang("SI20700_data"), errorData);
                            message += errorCountryduplicate == "" ? "" : string.Format(Message.GetString("2018101704", null), Util.GetLang("SI20700_Country"), errorCountryduplicate);
                            message += errorTerritoryduplicate == "" ? "" : string.Format(Message.GetString("2018101706", null), Util.GetLang("SI20700_Territory"), errorTerritoryduplicate);
                            message += errorStateduplicate == "" ? "" : string.Format(Message.GetString("2018101705", null), Util.GetLang("SI20700_State"), errorStateduplicate);
                          
                            if (message == "" || message == string.Empty)
                            {
                                _db.SaveChanges();
                            }
                            Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
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
            return _logMessage;
        }
        #endregion

    }
}

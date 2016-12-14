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
using HQFramework.Common;
using HQ.eSkyFramework.HQControl;
using System.Drawing;
using Microsoft.Office.Interop.Excel;
using Aspose.Cells;
using System.Runtime.InteropServices;
using System.Globalization;
namespace IF30100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IF30100Controller : Controller
    {
        public class ParmData
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        private SYS_ReportExport _objReport;
        private string _screenNbr = "IF30100";
        private string _userName = Current.UserName;
        private string _branchID = "";
        IF30100Entities _db = Util.CreateObjectContext<IF30100Entities>(false);
        IF30100SysEntities _eBiz4DSys = Util.CreateObjectContext<IF30100SysEntities>(true,"Sys");
        private JsonResult _logMessage;

        public ActionResult Index(string screenNbr)
        {
            if (screenNbr.PassNull() != string.Empty)
            {
                _screenNbr = screenNbr;
            }
            ViewBag.ScreenNbr = _screenNbr;
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                

               // _objOrder = data.ConvertToObject<OM10100_pcOrder_Result>(false, new string[] { "DoNotCalDisc", "CreditHold", "ApproveOver" });

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }

                return Util.CreateError(ex.ToString());
            }
        }
       
        public ActionResult GetIF30100_pgData(string view)
        {
            return this.Store(_db.IF30100_pgData(view).ToList());
        }

        [DirectMethod]
        public ActionResult IF30100Filter(string reportNbr)
        {

            IF30100SysEntities sys = new IF30100SysEntities(EntityConnectionStringHelper.Build(Current.Server,Current.DBSys, "IF30100SysModel"));
            var lstColumn = sys.SYS_ReportOLAPTemplate.Where(p => p.ReportNbr == reportNbr && p.PivotType == "P").OrderBy(p=>p.PivotOrder).ToList();

            var pnlFilterHeader = this.GetCmp<Panel>("pnlFilterHeader");
            var tabFilterGrid = this.GetCmp<TabPanel>("tabFilterGrid");

            foreach (var col in lstColumn)
            {
                if (col.ParmType.ToUpper() == "TextBox".ToUpper())
                {
                    pnlFilterHeader.Show();
                    string[] settings = col.ParmData.Split('#').ToArray();
                    TextField textField = new TextField()
                    {
                        ID = "Parm_" + col.ColumnName,
                        Name = "Parm_" + col.ColumnName,
                        FieldLabel = Util.GetLang(col.ColumnDescr),
                        LabelWidth = 150,
                        Width = settings[0].ToInt()

                    };

                    textField.Listeners.Change.Handler = GetHandle(lstColumn, col);
                    textField.Listeners.Render.Handler = string.Format("HQ.parm.push({{id:'{0}',type:'text'}})", "Parm_" + col.ColumnName);
                    textField.AddTo(pnlFilterHeader);
                  
                }
                if (col.ParmType.ToUpper() == "Combo".ToUpper())
                {
                    pnlFilterHeader.Show();
                    string[] settings = col.ParmData.Split('#').ToArray();
                    HQCombo comboField = new HQCombo()
                    {
                        ID = "Parm_" + col.ColumnName,
                        Name = "Parm_" + col.ColumnName,
                        HQLangCode = col.ColumnDescr,
                        HQProcedure = settings[0],
                        DisplayField = settings[2],
                        ValueField = settings[1],
                        Width = settings[4].ToInt(),
                        HQColumnShow = settings[3],
                        LabelWidth = 150,
                        MultiSelect = true,
                        ForceSelection = true,
                        HQParam = new StoreParameterCollection()
                    };
                    var filters = col.FilterBy.Split('#').ToList();

                    comboField.HQParam.Add(new StoreParameter() { Name = "@UserID", Value = "HQ.userName", Mode = ParameterMode.Raw });
                    comboField.HQParam.Add(new StoreParameter() { Name = "@LoginCpnyID", Value = "HQ.cpnyID", Mode = ParameterMode.Raw });
                    comboField.HQParam.Add(new StoreParameter() { Name = "@LangID", Value = "HQ.langID", Mode = ParameterMode.Raw });
                    foreach(var item in filters)
                    {
                        var colFilter = lstColumn.FirstOrDefault(p=>p.PivotType == "P" && p.ColumnName == item);
                        if(colFilter!=null)
                        {
                            if(colFilter.ParmType.ToUpper() == "Combo".ToUpper())
                            {
                                comboField.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getComboValue(App.Parm_" + colFilter.ColumnName + ".getValue())", Mode = ParameterMode.Raw });
                            }
                            if(colFilter.ParmType.ToUpper() == "Text".ToUpper())
                            {
                                comboField.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getValue(App.Parm_" + colFilter.ColumnName + ".getValue())", Mode = ParameterMode.Raw });
                            }
                           
                        }
                    }
                    comboField.Listeners.Change.Handler = GetHandle(lstColumn, col);
                    comboField.LoadData();
                    comboField.Listeners.BeforeQuery.Handler = "";
                    comboField.Listeners.Render.Handler = string.Format("HQ.parm.push({{id:'{0}',type:'combo'}})", "Parm_" + col.ColumnName);
                    comboField.AddTo(pnlFilterHeader);
                
               
                }
                if (col.ParmType.ToUpper() == "Date".ToUpper())
                {
                    pnlFilterHeader.Show();
                    string[] settings = col.ParmData.Split('#').ToArray();
                    HQDateField dateField = new HQDateField() { 
                        Name = "Parm_" + col.ColumnName, 
                        ID = "Parm_" + col.ColumnName, 
                        Value = DateTime.Now, 
                        HQLangCode = col.ColumnDescr,
                        Width = settings[0].ToInt(),
                        LabelWidth = 150
                    };
                    dateField.Listeners.Change.Handler = GetHandle(lstColumn, col);
                    dateField.Listeners.Render.Handler = string.Format("HQ.parm.push({{id:'{0}',type:'date'}})", "Parm_" + col.ColumnName);
                    dateField.AddTo(pnlFilterHeader);
                
                }
                if (col.ParmType.ToUpper() == "Month".ToUpper())
                {
                    pnlFilterHeader.Show();
                    string[] settings = col.ParmData.Split('#').ToArray();
                    HQDateField dateField = new HQDateField()
                    {
                        Name = "Parm_" + col.ColumnName,
                        ID = "Parm_" + col.ColumnName,
                        Value = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1),
                        HQLangCode = col.ColumnDescr,
                        Width = settings[0].ToInt(),
                        Type = DatePickerType.Month,
                        LabelWidth = 150,
                        Format = "MM/yyyy",
                    };
                    dateField.Listeners.Change.Handler = GetHandle(lstColumn, col);
                    dateField.Listeners.Render.Handler = string.Format("HQ.parm.push({{id:'{0}',type:'date'}})", "Parm_" + col.ColumnName);
                    dateField.AddTo(pnlFilterHeader);

                }
                if (col.ParmType.ToUpper() == "Year".ToUpper())
                {
                    pnlFilterHeader.Show();
                    string[] settings = col.ParmData.Split('#').ToArray();
                    HQDateField dateField = new HQDateField()
                    {
                        Name = "Parm_" + col.ColumnName,
                        ID = "Parm_" + col.ColumnName,
                        Value = new DateTime(DateTime.Now.Year, 1, 1),
                        HQLangCode = col.ColumnDescr,
                        Width = settings[0].ToInt(),
                        Type = DatePickerType.Month,
                        LabelWidth = 150,
                        Format = "yyyy"
                    };
                    dateField.Listeners.Change.Handler = GetHandle(lstColumn, col);
                    dateField.Listeners.Render.Handler = string.Format("HQ.parm.push({{id:'{0}',type:'date'}})", "Parm_" + col.ColumnName);
                    dateField.AddTo(pnlFilterHeader);

                }
                if (col.ParmType.ToUpper() == "Grid".ToUpper())
                {
                    string[] settings = col.ParmData.Split('#').ToArray();

                    tabFilterGrid.Show();

                    HQGridPanel gridPanel = new HQGridPanel()
                    {
                        ID = "Parm_" + col.ColumnName,
                        Title = Util.GetLang(col.ColumnDescr),
                        HQisPaging = true,
                        HQPageSize = 50,
                        HQAutoLoad = false,
                        BottomBar ={
                            new Ext.Net.PagingToolbar(){
                                ID="PagingTool_" + col.ColumnName,
                                Items = {
                                    new Ext.Net.Label("Page size:"),
                                    new Ext.Net.ToolbarSpacer(10),
                                    new Ext.Net.ComboBox(){
                                        Width = 80,
                                        Items = {
                                            new Ext.Net.ListItem("1"),
                                            new Ext.Net.ListItem("2"),
                                            new Ext.Net.ListItem("10"),
                                            new Ext.Net.ListItem("30"),
                                            new Ext.Net.ListItem("50")
                                        },
                                        SelectedItems = { new Ext.Net.ListItem("50") },
                                        Listeners = { Select = { Fn = "HQ.grid.onPageSelect" }}
                                    }
                                },
                                Plugins = { new Ext.Net.ProgressBarPager()}
                            }
                        },
                        HQParam = new StoreParameterCollection(),
                        HQDBSys = false,
                        HQProcedure = settings[0],
                        Tag = settings[1],
                        Plugins = { new CellEditing() { ClicksToEdit = 1 } }
                       
                    };


                    gridPanel.View.Add(new GridView() { TrackOver = false });

                    var filters = col.FilterBy.Split('#').ToList();

                 

                    gridPanel.HQParam.Add(new StoreParameter() { Name = "@UserID", Value = "HQ.userName", Mode = ParameterMode.Raw });
                    gridPanel.HQParam.Add(new StoreParameter() { Name = "@LoginCpnyID", Value = "HQ.cpnyID", Mode = ParameterMode.Raw });
                    gridPanel.HQParam.Add(new StoreParameter() { Name = "@LangID", Value = "HQ.langID", Mode = ParameterMode.Raw });
                    foreach (var item in filters)
                    {
                        var colFilter = lstColumn.FirstOrDefault(p => p.PivotType == "P" && p.ColumnName == item);
                        if (colFilter != null)
                        {
                            if (colFilter.ParmType.ToUpper() == "Combo".ToUpper())
                            {
                                gridPanel.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getComboValue(App.Parm_" + colFilter.ColumnName + ".getValue())", Mode = ParameterMode.Raw });
                            }
                            if (colFilter.ParmType.ToUpper() == "Text".ToUpper())
                            {
                                gridPanel.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getValue(App.Parm_" + colFilter.ColumnName + ".getValue())", Mode = ParameterMode.Raw });
                            }
                            if (colFilter.ParmType.ToUpper() == "Grid".ToUpper())
                            {
                                gridPanel.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getGridValue(App.Parm_" + colFilter.ColumnName + ")", Mode = ParameterMode.Raw });
                            }
                            if (colFilter.ParmType.ToUpper() == "Date".ToUpper())
                            {
                                gridPanel.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getDate(App.Parm_" + colFilter.ColumnName + ".getValue(),'D')", Mode = ParameterMode.Raw });
                            }
                            if (colFilter.ParmType.ToUpper() == "Month".ToUpper())
                            {
                                gridPanel.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getDate(App.Parm_" + colFilter.ColumnName + ".getValue(),'M')", Mode = ParameterMode.Raw });
                            }
                            if (colFilter.ParmType.ToUpper() == "Year".ToUpper())
                            {
                                gridPanel.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "getDate(App.Parm_" + colFilter.ColumnName + ".getValue(),'Y')", Mode = ParameterMode.Raw });
                            }
                        }
                    }

                    gridPanel.Listeners.Edit.Handler = GetHandle(lstColumn, col);
                    gridPanel.LoadData();
                    gridPanel.Store[0].Model[0].IDProperty = "";
                    var lstField =  gridPanel.Store[0].Model[0].Fields.ToList();
                    gridPanel.Store[0].Model[0].Fields.Clear();
                    gridPanel.Store[0].Model[0].Fields.Add(new ModelField("Sel", ModelFieldType.Boolean) );
                    gridPanel.Store[0].Model[0].Fields.AddRange(lstField);

                    var lstItem = gridPanel.ColumnModel.Items.ToList();
                    gridPanel.ColumnModel.Items.Clear();
                    gridPanel.ColumnModel.Add(new Ext.Net.RowNumbererColumn());
                    var colCheck = new Ext.Net.CheckColumn() { Text = "", DataIndex = "Sel",Editable= true,Width = 70,Sortable=false };
                    colCheck.RenderTpl = new XTemplate()
                    {
                        Html = @"<div id=""{id}-titleEl"" {tipMarkup}class=""x-column-header-inner"">
                                                                <span id=""{id}-textEl"" class=""x-column-header-text"">
                                                                    <input id=""my-header-checkbox-{id}"" type=""checkbox"" class=""my-header-checkbox""></input>
                                                                    {text}
                                                                </span>
                                                                <tpl if=""!menuDisabled"">
                                                                    <div id=""{id}-triggerEl"" class=""x-column-header-trigger""></div>
                                                                </tpl>
                                                            </div>
                                                            {%this.renderContainer(out,values)%}"
                    };
                    
                         
                    gridPanel.ColumnModel.Add(colCheck);
                    
                    gridPanel.ColumnModel.Items.AddRange(lstItem);
                    gridPanel.ColumnModel.Listeners.HeaderClick.Fn = "chkHeader_Change";
                    gridPanel.SelectionModel.Add(new RowSelectionModel() { Mode = SelectionMode.Single });
                    gridPanel.Listeners.Added.Handler = string.Format("HQ.parm.push({{id:'{0}',type:'grid'}})", "Parm_" + col.ColumnName);
                    gridPanel.AddTo(tabFilterGrid);
                   
                }
            }
            return this.Direct();
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
        public ActionResult ExportView(FormCollection data, string view, string name, string proc,bool isReadOnly)
        {
            try
            {
                Stream stream = new MemoryStream();
                Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
                Aspose.Cells.Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = "Data";

                var detHandler = new StoreDataHandler(data["lstDet"]);
                var lstDet = detHandler.ObjectData<IF30100_pgData_Result>().ToList();
                int i = 0;
                foreach (var obj in lstDet)
                {
                    SetCellValueGrid(SheetData.Cells.Rows[0][i], Util.GetLang(obj.Column_Name), TextAlignmentType.Center, TextAlignmentType.Left);
                    i++;
                }
                DataAccess dal = Util.Dal(false, isReadOnly);
                ParamCollection pc = new ParamCollection();
                System.Data.DataTable dtInvtID = dal.ExecDataTable(proc, CommandType.Text, ref pc);

                Cell cell;
                for (int j = 1; j < dtInvtID.Rows.Count; j++)
                {
                    for(int x = 0; x < dtInvtID.Columns.Count; x++){
                        cell = SheetData.Cells[j, x];
                        if (dtInvtID.Columns[x].DataType.ToString().ToUpper().Contains("DATE"))
                        {
                            DateTime tmpValue = DateTime.Parse(dtInvtID.Rows[j][x].ToString());
                            cell.PutValue(tmpValue.ToString(Current.FormatDate));
                        }
                        else
                        {
                            cell.PutValue(dtInvtID.Rows[j][x].ToString());
                        }
                    }
                }

                //SheetData.Cells.ImportDataTable(dtCloned, false, "A2");// du lieu Inventory

                           

                SheetData.AutoFitColumns();

                string fileName = Guid.NewGuid().ToString() + ".xlsx";
                string path = Server.MapPath("~/ExportPivot") + @"\" + fileName;
              
        
                workbook.Save(path, SaveFormat.Xlsx);

                return Json(new { success = true, id = fileName, name = name + ".xlsx" }, JsonRequestBehavior.AllowGet);
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

        public bool ChangeColumnDataType(System.Data.DataTable table, string columnname, Type newtype)
        {
            if (table.Columns.Contains(columnname) == false)
                return false;

            DataColumn column = table.Columns[columnname];
            if (column.DataType == newtype)
                return true;

            try
            {
                DataColumn newcolumn = new DataColumn("temporary", newtype);
                table.Columns.Add(newcolumn);
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        row["temporary"] = Convert.ChangeType(row[columnname], newtype);
                    }
                    catch
                    {
                    }
                }
                table.Columns.Remove(columnname);
                newcolumn.ColumnName = columnname;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        [HttpPost]
        public ActionResult ExportPivot(FormCollection data, string view, string name)
        {
            Application excelApplication = null;
            Microsoft.Office.Interop.Excel.Workbook excelWorkBook = null;
            Microsoft.Office.Interop.Excel.Worksheet targetSheet = null;
            Microsoft.Office.Interop.Excel.Worksheet dataSheet = null;
            PivotTable pivotTable = null;
            Microsoft.Office.Interop.Excel.Range pivotData = null;
            Microsoft.Office.Interop.Excel.Range pivotDestination = null;
            string fileName = "";
           
            try
            {
                string select = "";
                string param = "";
                string cmd = "";
                string reportNbr = data["report"].PassNull();
                var parmHandler = new StoreDataHandler(data["data"]);
                var lstParm = parmHandler.ObjectData<ParmData>().ToList();

                IF30100SysEntities sys = new IF30100SysEntities(EntityConnectionStringHelper.Build(Current.Server, Current.DBSys, "IF30100SysModel"));
                var report = sys.SYS_ReportExport.FirstOrDefault(p => p.ReportNbr == reportNbr);
                DataAccess dal = Util.Dal(false, report.IsReadOnly);
                ParamCollection pc = new ParamCollection();

                var lstColumn = sys.SYS_ReportOLAPTemplate.Where(p => p.ReportNbr == reportNbr && p.PivotType != "P").ToList();
                var lstColReport = sys.SYS_ReportOLAPTemplate.Where(p => p.ReportNbr == reportNbr && p.PivotType == "P").ToList();

                foreach (var item in lstParm)
                {
                    string nameParm = item.Name.Replace("Parm_","");

                    var colFilter = lstColReport.FirstOrDefault(p => p.ColumnName == nameParm);
                    if (colFilter == null) continue;
                    if (colFilter.ParmType.ToUpper() == "Month".ToUpper())
                    {
                        item.Value = item.Value.Substring(0,7) + "-01T00:00:00";
                    }
                    else if (colFilter.ParmType.ToUpper() == "Year".ToUpper())
                    {
                        item.Value = item.Value.Substring(0, 4) + "-01-01T00:00:00";
                    }
                }
                System.Data.DataTable dt = null;
                if (report.SourceType.PassNull() == "V")
                {
                    var filter = sys.SYS_ReportOLAPFilter.FirstOrDefault(p => p.ReportNbr == reportNbr);
                    if (filter != null && filter.FilterData.PassNull() != string.Empty)
                    {
                        foreach (var item in lstParm)
                        {
                            string parmName = item.Name.Replace("Parm_", "");


                            if (filter.FilterData.Contains("IN #" + parmName))
                            {
                                var lstTempValue = item.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                string inValue = "IN (";
                                foreach (var itemValue in lstTempValue)
                                {
                                    inValue += "N'" + itemValue + "',";
                                }
                                if (inValue.Length > 4)
                                {
                                    inValue = inValue.TrimEnd(',') + ")";
                                }
                                else
                                {
                                    inValue += "'')";
                                }
                                filter.FilterData = filter.FilterData.Replace("IN #" + parmName, inValue);
                            }
                            filter.FilterData = filter.FilterData.Replace("#" + parmName, "N'" + item.Value + "'");
                        }
                    }


                  

                    var lstTemp = new List<string>();
                    foreach (var col in lstColumn)
                    {
                        if (!lstTemp.Any(p => p == col.ColumnName))
                        {
                            lstTemp.Add(col.ColumnName);
                            //select += string.Format("N'{0}' = {1},", Util.GetLang(col.ColumnDescr), col.ColumnName);
                            select += string.Format("[{0}],", col.ColumnName);
                        }
                    }

                    cmd = "select " + select.TrimEnd(',') + " from " + view;
                    if (filter.FilterData.PassNull().Length > 0)
                    {
                        cmd += " where " + filter.FilterData;
                    }
                    dt = dal.ExecDataTable(cmd, CommandType.Text, ref pc);
                }
                else
                {
                 

                    string cmdRPT = "insert into RPTRunning(ReportNbr,MachineName,ReportName,ReportCap,ReportDate,StringParm00,StringParm01,StringParm02,StringParm03," +
                        "DateParm00,DateParm01,DateParm02,DateParm03,BooleanParm00,BooleanParm01,BooleanParm02,BooleanParm03,ListParm00,ListParm01,ListParm02,ListParm03,SelectionFormular,UserID,AppPath,ClientName,LoggedCpnyID,CpnyID,LangID) " +
                        "values(@ReportNbr,@MachineName,@ReportName,@ReportCap,@ReportDate,@StringParm00,@StringParm01,@StringParm02,@StringParm03," +
                        "@DateParm00,@DateParm01,@DateParm02,@DateParm03,@BooleanParm00,@BooleanParm01,@BooleanParm02,@BooleanParm03,@ListParm00,@ListParm01,@ListParm02,@ListParm03,@SelectionFormular,@UserID,@AppPath,@ClientName,@LoggedCpnyID,@CpnyID,@LangID) SELECT @@IDENTITY";

                    pc.Add(new ParamStruct("@ReportNbr", DbType.String, report.ReportNbr, ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@MachineName", DbType.String, Environment.MachineName, ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@ReportName", DbType.String, report.Name, ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@ReportCap", DbType.String, report.Name, ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@ReportDate", DbType.DateTime, DateTime.Now, ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@StringParm00", DbType.String, GetRPTParm(lstColReport, lstParm, "StringParm00", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@StringParm01", DbType.String, GetRPTParm(lstColReport, lstParm, "StringParm01", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@StringParm02", DbType.String, GetRPTParm(lstColReport, lstParm, "StringParm02", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@StringParm03", DbType.String, GetRPTParm(lstColReport, lstParm, "StringParm03", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@DateParm00", DbType.DateTime, GetRPTParm(lstColReport, lstParm, "DateParm00", DateTime.Now), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@DateParm01", DbType.DateTime, GetRPTParm(lstColReport, lstParm, "DateParm01", DateTime.Now), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@DateParm02", DbType.DateTime, GetRPTParm(lstColReport, lstParm, "DateParm02", DateTime.Now), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@DateParm03", DbType.DateTime, GetRPTParm(lstColReport, lstParm, "DateParm03", DateTime.Now), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@BooleanParm00", DbType.Boolean, GetRPTParm(lstColReport, lstParm, "BooleanParm00", false), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@BooleanParm01", DbType.Boolean, GetRPTParm(lstColReport, lstParm, "BooleanParm01", false), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@BooleanParm02", DbType.Boolean, GetRPTParm(lstColReport, lstParm, "BooleanParm02", false), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@BooleanParm03", DbType.Boolean, GetRPTParm(lstColReport, lstParm, "BooleanParm03", false), ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@ListParm00", DbType.String, GetRPTParm(lstColReport, lstParm, "ListParm00", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@ListParm01", DbType.String, GetRPTParm(lstColReport, lstParm, "ListParm01", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@ListParm02", DbType.String, GetRPTParm(lstColReport, lstParm, "ListParm02", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@ListParm03", DbType.String, GetRPTParm(lstColReport, lstParm, "ListParm03", ""), ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@SelectionFormular",DbType.String, "", ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@UserID", DbType.String, Current.UserName, ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@AppPath", DbType.String,"", ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@ClientName", DbType.String, HttpContext.Request.UserHostName, ParameterDirection.Input, 200));
                    pc.Add(new ParamStruct("@LoggedCpnyID", DbType.String, Current.CpnyID, ParameterDirection.Input, 200));
                  
                    HQ.eSkySys.eSkySysEntities sys2 = Util.CreateObjectContext<HQ.eSkySys.eSkySysEntities>(true);
                    var user = sys2.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());

                    pc.Add(new ParamStruct("@CpnyID", DbType.String, user !=null ? user.CpnyID : "" , ParameterDirection.Input, Int32.MaxValue));
                    pc.Add(new ParamStruct("@LangID", DbType.Int16, Current.LangID, ParameterDirection.Input, 200));

                    int rptID = Convert.ToInt32(dal.ExecScalar(cmdRPT, CommandType.Text, ref pc));

                    pc = new ParamCollection();
                    pc.Add(new ParamStruct("@RPTID", DbType.Int32, rptID, ParameterDirection.Input, 200));
                    dt = dal.ExecDataTable(report.Proc, CommandType.StoredProcedure, ref pc);

                }
                      
                if (dt.Rows.Count == 0)
                {
                    Util.AppendLog(ref _logMessage, "20100101", "");
                    return _logMessage;
                }
                if (dt.Rows.Count > 999999)
                {
                    Util.AppendLog(ref _logMessage, "201610061", "");
                    return _logMessage;
                }

                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                {
                    var col = lstColumn.FirstOrDefault(p => p.ColumnName == dt.Columns[i].ColumnName);
                    if (col == null)
                    {
                        dt.Columns.RemoveAt(i);
                    }
                }
                dt.AcceptChanges();

                excelApplication = new Application();

              
                excelWorkBook = excelApplication.Workbooks.Add();

                string pivotTableName = "PivotTable";
                string workSheetName = "OLAPReport";

                excelWorkBook.Worksheets.Add();
                excelWorkBook.Worksheets.Add();
                targetSheet = excelWorkBook.Worksheets[1];
                targetSheet.Name = workSheetName;
                dataSheet = excelWorkBook.Worksheets[2];
                dataSheet.Name = "Data";


                string finalColLetter = string.Empty;
                string colChartSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int colChartSetLen = colChartSet.Length;

                if (dt.Columns.Count > colChartSetLen)
                {
                    finalColLetter = colChartSet.Substring((dt.Columns.Count - 1) / colChartSetLen - 1, 1);
                }
                finalColLetter += colChartSet.Substring((dt.Columns.Count - 1) % colChartSetLen, 1);


                object[,] rawData = new object[1, dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var col = lstColumn.FirstOrDefault(p => p.ColumnName == dt.Columns[i].ColumnName);
                    rawData[0, i] = Util.GetLang(col.ColumnDescr);

                    var column = (i / 25 > 0 ? colChartSet[i / 25].ToString() : "") + colChartSet[i % 25].ToString();

                    if (col.DataFormat.PassNull() != string.Empty)
                    {
                        dataSheet.Range[string.Format("{0}{1}:{0}{2}", column, 2, dt.Rows.Count + 1), Type.Missing].NumberFormat = col.DataFormat;
                    }
                    else
                    {
                        dataSheet.Range[string.Format("{0}{1}:{0}{2}", column, 2, dt.Rows.Count + 1), Type.Missing].NumberFormat = "@";
                    }
                }

                var lstMeasure = lstColumn.Where(p => p.PivotType == "M").OrderBy(p => p.PivotOrder).ToList();

                string excelRange = String.Format("A1:{0}{1}", finalColLetter, 1);
                dataSheet.Range[excelRange, Type.Missing].Value2 = rawData;
                (dataSheet.Rows[1, Type.Missing] as Microsoft.Office.Interop.Excel.Range).Font.Bold = true;

              
                for (int i = 0; i < (dt.Rows.Count / 1000); i++)
                {
                    object[,] rawData2 = new object[1000, dt.Columns.Count];
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        if (!lstMeasure.Any(p => Util.GetLang(p.ColumnDescr) == dt.Columns[col].ColumnName))
                        {
                            for (int row = 0; row < 1000; row++)
                            {
                                rawData2[row, col] =  dt.Rows[row + (i * 1000)].ItemArray[col];
                            }
                            
                        }
                        else
                        {
                            for (int row = 0; row < 1000; row++)
                            {
                                rawData2[row, col] = dt.Rows[row + (i * 1000)].ItemArray[col];
                            }
                        }
                        
                    }
                    excelRange = String.Format("A{0}:{1}{2}", i * 1000 + 2, finalColLetter, (i + 1) * 1000 + 1);
                    dataSheet.Range[excelRange, Type.Missing].Value2 = rawData2;

                }

                if (dt.Rows.Count % 1000 > 0)
                {
                    object[,] rawData2 = new object[dt.Rows.Count % 1000, dt.Columns.Count];
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        if (!lstMeasure.Any(p => Util.GetLang(p.ColumnDescr) == dt.Columns[col].ColumnName))
                        {
                            for (int row = 0; row < (dt.Rows.Count % 1000); row++)
                            {
                                rawData2[row, col] =  dt.Rows[(dt.Rows.Count / 1000) * 1000 + row].ItemArray[col];
                            }
                          
                        }
                            
                        else
                        {
                            for (int row = 0; row < (dt.Rows.Count % 1000); row++)
                            {
                                rawData2[row, col] = dt.Rows[(dt.Rows.Count / 1000) * 1000 + row].ItemArray[col];
                            }
                        }
                           

                       
                    }
                    excelRange = String.Format("A{0}:{1}{2}", (dt.Rows.Count / 1000) * 1000 + 2, finalColLetter, (dt.Rows.Count / 1000) * 1000 + dt.Rows.Count % 1000 + 1);
                    dataSheet.Range[excelRange, Type.Missing].Value2 = rawData2;

                }

                var lstFilter = lstColumn.Where(p => p.PivotType == "F").OrderByDescending(p => p.PivotOrder).ToList();

                dataSheet.Columns.AutoFit();
                dataSheet.Protect("Hqs0ft20062099", true, true, true, true, true, true);
                dataSheet.Visible = XlSheetVisibility.xlSheetHidden;

                pivotData = dataSheet.Range[dataSheet.Cells[1, 1], dataSheet.Cells[dt.Rows.Count + 1, dt.Columns.Count]];


                var lstControlFilter = lstColReport.Where(p =>p.ShowFilterInExcel).OrderBy(p => p.PivotOrder).ToList();

                var lstFilterShow = lstFilter.Where(p => p.PivotShow).ToList();
                pivotDestination = targetSheet.Range["A" + (lstControlFilter.Count + 7 + lstFilterShow.Count() / 2).ToString()];

                excelWorkBook.PivotTableWizard(XlPivotTableSourceType.xlDatabase, pivotData, pivotDestination, pivotTableName, true, true, true, true, Type.Missing, Type.Missing, false, false, XlOrder.xlOverThenDown, 2);
                pivotTable = targetSheet.PivotTables(pivotTableName);



                var lstRow = lstColumn.Where(p => p.PivotType == "R").OrderBy(p => p.PivotOrder).ToList();

                foreach (var row in lstRow)
                {
                    try
                    {
                        PivotField pvf = pivotTable.PivotFields(Util.GetLang(row.ColumnDescr));
                        if (row.PivotShow)
                        {
                            pvf.Orientation = XlPivotFieldOrientation.xlRowField;
                            pvf.Subtotals[1] = true;
                            pvf.Subtotals[1] = false;

                        }
                    }
                    catch (Exception)
                    {
                        throw new MessageException("20410", "", new string[] { string.Format("Dòng {0} - {1} bị lỗi", row.ColumnName, Util.GetLang(row.ColumnDescr)) });
                    }
                    //foreach (var item in pvf.PivotItems())
                    //{
                    //    item.ShowDetail = false;
                    //}

                }

                var lstCol = lstColumn.Where(p => p.PivotType == "C").OrderBy(p => p.PivotOrder).ToList();
                foreach (var col in lstCol)
                {
                    try
                    {
                        PivotField pvf = pivotTable.PivotFields(Util.GetLang(col.ColumnDescr));
                        if (col.PivotShow)
                        {
                            pvf.Orientation = XlPivotFieldOrientation.xlColumnField;
                            pvf.Subtotals[1] = true;
                            pvf.Subtotals[1] = false;

                        }
                    }
                    catch (Exception)
                    {
                        
                        throw new MessageException("20410", "", new string[] { string.Format("Cột {0} - {1} bị lỗi", col.ColumnName, Util.GetLang(col.ColumnDescr)) });
                    }
                   
                  
                    //foreach (var item in pvf.PivotItems())
                    //{
                    //    item.ShowDetail = false;
                    //}
                }

                foreach (var colFilter in lstFilter)
                {
                    try
                    {
                        PivotField pvf = pivotTable.PivotFields(Util.GetLang(colFilter.ColumnDescr));
                        if (colFilter.PivotShow)
                        {
                            pvf.Orientation = XlPivotFieldOrientation.xlPageField;
                            pvf.CurrentPage = "(All)";
                            pvf.Subtotals[1] = true;
                            pvf.Subtotals[1] = false;
                        }
                    }
                    catch (Exception)
                    {

                        throw new MessageException("20410", "", new string[] { string.Format("Filter {0} - {1} bị lỗi", colFilter.ColumnName, Util.GetLang(colFilter.ColumnDescr)) });
                    }


                }

              
                foreach (var measure in lstMeasure)
                {
                    try
                    {
                        PivotField pvf = pivotTable.PivotFields(Util.GetLang(measure.ColumnDescr));
                        if (measure.PivotShow)
                        {
                            pvf.Orientation = XlPivotFieldOrientation.xlDataField;

                            pvf.Function = GetFunction(measure.MeasureFunc);
                            if (measure.DataFormat.PassNull() != string.Empty)
                                pvf.NumberFormat = measure.DataFormat;
                            else
                                pvf.NumberFormat = "#,##";

                            foreach (var item in pvf.PivotItems())
                            {
                                item.ShowDetail = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw new MessageException("20410", "", new string[] { string.Format("Measure {0} - {1} bị lỗi", measure.ColumnName, Util.GetLang(measure.ColumnDescr)) });
                        
                    }
                   
                  
               
                    //foreach (var item in pvf.PivotItems())
                    //{
                    //    item.ShowDetail = false;
                    //}
                }
                if (lstMeasure.Where(p => p.PivotShow).Count() > 1)
                {
                    pivotTable.PivotFields("Data").Orientation = XlPivotFieldOrientation.xlColumnField;
                }
              
               
                pivotTable.CacheIndex = 1;
                pivotTable.SaveData = false;
                pivotTable.RefreshTable();
                //pivotTable.CalculateData();

                if (report.ExportImage)
                {
                    string pathImage = Server.MapPath("~/Content/Images/logo.png");

                    if (System.IO.File.Exists(pathImage))
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromFile(pathImage);
                        double rate = (double)image.Height / 40;

                        targetSheet.Shapes.AddPicture(pathImage, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, targetSheet.Range["F1:F1"].Left, 0, (int)((double)image.Width / rate), 30);
                    }
                  
                }



                for (int i = 4 + (lstControlFilter.Count / 2); i <= 6 + (lstControlFilter.Count / 2) + (lstFilterShow.Count / 2); i++)
                {
                    if (!String.IsNullOrWhiteSpace(targetSheet.Cells[i, 1].Value))
                    {
                        targetSheet.Cells[i, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 176, 240));
                    }
                    if (!String.IsNullOrWhiteSpace(targetSheet.Cells[i, 4].Value))
                    {
                        targetSheet.Cells[i, 4].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 176, 240));
                    }
                }

                targetSheet.Range["A1:E1"].Font.Size = 25;
                targetSheet.Cells[1, 5].HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                targetSheet.Cells[1, 5].Value = Util.GetLang(report.Name);
                targetSheet.Cells[1, 5].Font.Bold = true;
                targetSheet.Range["A1:E1"].HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                targetSheet.Range["A1:E1"].Merge();

                targetSheet.Cells[2, 5].Font.Bold = true;
                targetSheet.Cells[2, 5].HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                targetSheet.Cells[2, 5].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " (" + Current.UserName + ")";
                targetSheet.Range["A2:E2"].HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                targetSheet.Range["A2:E2"].Merge();

                targetSheet.Range["A3:A3"].EntireColumn.ColumnWidth = 20;
                targetSheet.Range["B3:B3"].EntireColumn.ColumnWidth = 15;
                targetSheet.Range["C3:C3"].EntireColumn.ColumnWidth = 20;
                targetSheet.Range["D3:D3"].EntireColumn.ColumnWidth = 15;

                int start = 3;
                for (int i = 0; i < lstControlFilter.Count; i++)
                {
                    var ctrFilter = lstControlFilter[i];
                    var parm = lstParm.FirstOrDefault(p => p.Name == "Parm_" + ctrFilter.ColumnName);
                    if (parm == null)
                    {
                        parm = new ParmData();
                    }
                    if (i % 2 == 0)
                    {
                        targetSheet.Cells[start, 1].Value = Util.GetLang(lstControlFilter[i].ColumnDescr);
                        targetSheet.Cells[start, 1].Font.Bold = true;
                        targetSheet.Cells[start, 2].NumberFormat = "@";
     
                        if (ctrFilter.ParmType.ToUpper() == "Month".ToUpper())
                        {
                            targetSheet.Cells[start, 2].Value = parm.Value.Substring(5, 2) + "/" + parm.Value.Substring(0, 4);
                        }
                        else if (ctrFilter.ParmType.ToUpper() == "Year".ToUpper())
                        {
                            targetSheet.Cells[start, 2].Value = parm.Value.Substring(0, 4);
                        }
                        else if (ctrFilter.ParmType.ToUpper() == "Date".ToUpper())
                        {
                            DateTime date = Convert.ToDateTime(parm.Value.Substring(5, 2) + "/" + parm.Value.Substring(8, 2) + "/" + parm.Value.Substring(0, 4), System.Globalization.CultureInfo.InvariantCulture);
                            if (ctrFilter.DataFormat.PassNull() != string.Empty)
                            {
                                targetSheet.Cells[start, 2].Value = date.ToString(ctrFilter.DataFormat);
                            }
                            else
                            {
                                targetSheet.Cells[start, 2].Value = date.ToString("dd/MM/yyyy");
                            }
                            
                        }
                        else
                        {
                            targetSheet.Cells[start, 2].Value = parm.Value;
                        }
                       
                   
                    }
                    else
                    {
                        targetSheet.Cells[start, 3].Value = Util.GetLang(lstControlFilter[i].ColumnDescr);
                        targetSheet.Cells[start, 3].Font.Bold = true;
                        targetSheet.Cells[start, 4].NumberFormat = "@";
                        if (ctrFilter.ParmType.ToUpper() == "Month".ToUpper())
                        {
                            targetSheet.Cells[start, 4].Value = parm.Value.Substring(5, 2) + "/" + parm.Value.Substring(0, 4);
                        }
                        else if (ctrFilter.ParmType.ToUpper() == "Year".ToUpper())
                        {
                            targetSheet.Cells[start, 4].Value = parm.Value.Substring(0, 4);
                        }
                        else if (ctrFilter.ParmType.ToUpper() == "Date".ToUpper())
                        {
                            DateTime date = Convert.ToDateTime(parm.Value.Substring(5, 2) + "/" + parm.Value.Substring(8, 2) + "/" + parm.Value.Substring(0, 4),System.Globalization.CultureInfo.InvariantCulture);
                            if (ctrFilter.DataFormat.PassNull() != string.Empty)
                            {
                                targetSheet.Cells[start, 4].Value = date.ToString(ctrFilter.DataFormat);
                            }
                            else
                            {
                                targetSheet.Cells[start, 4].Value = date.ToString("dd/MM/yyyy");
                            }
                        }
                        else
                        {
                            targetSheet.Cells[start, 4].Value = parm.Value;
                        }
                    
                        start++;
                    }
                }

                fileName =  Guid.NewGuid().ToString() + ".xlsb";
                string path = Server.MapPath("~/ExportPivot");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fullName =   path + @"\" + fileName;
             

                targetSheet.Protect("Hqs0ft20062099", true, false, true, true, true, true, true, true, true, true, true, true, true, true, true);
                excelApplication.DisplayAlerts = false;


                excelWorkBook.SaveAs(fullName, XlFileFormat.xlExcel12, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange);
                //Stream stream = new MemoryStream(buffer);
                //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = name + ".xlsb" };

                return Json(new { success = true, id = fileName, name = name + ".xlsb" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    if (ex.ToString().Contains("0x800A03EC"))
                    {
                        throw new MessageException("20410", "", new string[] { "Vượt quá số lượng dòng cho phép của Microsoft Excel" });
                    }
                    else
                    {
                        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                    }
                }
            }
            finally
            {
                if (excelWorkBook != null)
                {
                    excelWorkBook.Close(false, Type.Missing, Type.Missing);  
                    Marshal.FinalReleaseComObject(excelWorkBook);
                }

                if (excelApplication != null)
                {
                    excelApplication.Application.Quit();
                    excelApplication.Quit();
                    Marshal.FinalReleaseComObject(excelApplication);

                }
                pivotDestination = null;
                pivotData = null;
                pivotTable = null;
                targetSheet = null;
                dataSheet = null;
                excelWorkBook = null;
                excelApplication = null;

                  GC.Collect();
                GC.WaitForPendingFinalizers();

            }
        }
        public ActionResult DownloadFile(string name, string id)
        {
            string path = Server.MapPath("~/ExportPivot");
            string fullName = path + @"\" + id;
            if (System.IO.File.Exists(fullName))
            {
                try
                {
                    byte[] buffer = new byte[1];
                    using (FileStream fs = new FileStream(fullName, FileMode.Open,
                                       FileAccess.Read, FileShare.Read))
                    {
                        buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, (int)fs.Length);

                    }

                    Stream stream = new MemoryStream(buffer);
                    return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = name };

                }
                catch (Exception)
                {
                    
                    throw;
                }
                finally
                {
                    System.IO.File.Delete(fullName);
                }
               
            }
            return new HttpNotFoundResult();
           
        }
        //public ActionResult ExportPivot(FormCollection data, string view, string name)
        //{
        //    try
        //    {
        //        string select = "";
        //        string param = "";
        //        string cmd = "";
        //        string reportNbr = data["report"].PassNull();
        //        var parmHandler = new StoreDataHandler(data["data"]);
        //        var lstParm = parmHandler.ObjectData<ParmData>().ToList();

        //        IF30100SysEntities sys = new IF30100SysEntities(EntityConnectionStringHelper.Build(Current.Server,Current.DBSys, "IF30100SysModel"));
        //        var filter = sys.SYS_ReportOLAPFilter.FirstOrDefault(p => p.ReportNbr == reportNbr);
        //        if (filter != null && filter.FilterData.PassNull() != string.Empty)
        //        {
        //            foreach (var item in lstParm)
        //            {
        //                string parmName = item.Name.Replace("Parm_","");
                        
                            
        //                if (filter.FilterData.Contains("IN #"+parmName))
        //                {
        //                    var lstTempValue = item.Value.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries).ToList();
        //                    string inValue = "IN (";
        //                    foreach (var itemValue in lstTempValue)
        //                    {
        //                        inValue += "N'" + itemValue + "',";
        //                    }
        //                    if(inValue.Length>3)
        //                    {
        //                        inValue = inValue.TrimEnd(',') + ")";
        //                    } 
        //                    else
        //                    {
        //                        inValue= "";
        //                    }
        //                    filter.FilterData = filter.FilterData.Replace("IN #" + parmName, inValue);
        //                }
        //                filter.FilterData = filter.FilterData.Replace("#" + parmName, "N'" + item.Value + "'");
        //            }
        //        }

              
        //        var lstColumn = sys.SYS_ReportOLAPTemplate.Where(p => p.ReportNbr == reportNbr && p.PivotType!="P").ToList();

        //        var lstTemp = new List<string>();
        //        foreach (var col in lstColumn)
        //        {
        //            if (!lstTemp.Any(p => p == col.ColumnName))
        //            {
        //                lstTemp.Add(col.ColumnName);
        //                select += string.Format("N'{0}' = {1},", Util.GetLang(col.ColumnDescr), col.ColumnName);
        //            }
        //        }

        //        cmd = "select " + select.TrimEnd(',') + " from " + view;
        //        if (filter.FilterData.PassNull().Length > 0)
        //        {
        //            cmd += " where " + filter.FilterData;
        //        }
        //        Stream stream = new MemoryStream();
        //        Workbook workbook = new Workbook();

        //        Worksheet sheetPivot = workbook.Worksheets[0];
        //        Worksheet sheetData = workbook.Worksheets[workbook.Worksheets.Add()];

        //        sheetData.Name = "Data";
        //        sheetData.IsVisible = false;

        //        DataAccess dal = Util.Dal();
        //        ParamCollection pc = new ParamCollection();

        //        DataTable dt = dal.ExecDataTable(cmd, CommandType.Text, ref pc);
        //        if (dt.Rows.Count == 0)
        //        {
        //            Util.AppendLog(ref _logMessage, "20100101", "");
        //            return _logMessage;
        //        }
        //        sheetData.Cells.ImportDataTable(dt, true, "A1");// du lieu Inventory
             
        //        sheetData.AutoFitColumns();
               
        //        sheetPivot.Name = name;
        //        //Getting the pivottables collection in the sheet
        //        Aspose.Cells.Pivot.PivotTableCollection pivotTables = sheetPivot.PivotTables;
        //        sheetPivot.Cells[0, 0].Value = name;
        //        var style = sheetPivot.Cells[0, 0].GetStyle();
        //        style.Font.Size = 25;
        //        style.Font.IsBold = true;
        //        sheetPivot.Cells[0, 0].SetStyle(style);
                

        //        var lstFilter = lstColumn.Where(p => p.PivotType == "F").OrderBy(p => p.PivotOrder).ToList();

        //        int index = pivotTables.Add("=Data!A1:" + sheetData.Cells[sheetData.Cells.MaxDataRow, sheetData.Cells.MaxDataColumn].Name, "A" + (lstFilter.Count + 5).ToString(), "PivotTable1");
              
        //        Aspose.Cells.Pivot.PivotTable pivotTable = pivotTables[index];
              
        //        pivotTable.RowGrand = true;
                
        //        pivotTable.ColumnGrand = true;
              
        //        pivotTable.IsAutoFormat = true;
              
        //        pivotTable.AutoFormatType = Aspose.Cells.Pivot.PivotTableAutoFormatType.None;
              
              
              
        //        var lstRow = lstColumn.Where(p => p.PivotType == "R").OrderBy(p => p.PivotOrder).ToList();

        //        foreach (var item in lstRow)
        //        {
        //            var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
        //            if (col != null)
        //            {
        //                pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Row,col.ColumnName);
        //                var field = pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Row)[col.ColumnName];
        //                if (item.DataFormat.PassNull()!=string.Empty)
        //                {
        //                    field.NumberFormat = item.DataFormat;
        //                }
        //            }
                   
        //        }

        //        var lstCol= lstColumn.Where(p => p.PivotType == "C").OrderBy(p => p.PivotOrder).ToList();
        //        foreach (var item in lstCol)
        //        {
        //            var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
        //            if (col != null)
        //            {
        //                pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Column, col.ColumnName);
        //                var field = pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Column)[col.ColumnName];
        //                if (item.DataFormat.PassNull() != string.Empty)
        //                {
        //                    field.NumberFormat = item.DataFormat;
        //                }
        //            }
        //        }

        //        var lstMeasure = lstColumn.Where(p => p.PivotType == "M").OrderBy(p => p.PivotOrder).ToList();
        //        foreach (var item in lstMeasure)
        //        {
        //            var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
        //            if (col != null)
        //            {
        //                pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Data, col.ColumnName);
        //                var field = pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Data)[col.ColumnName];
        //                field.Function = GetFunction(item.MeasureFunc);
        //                if(item.DataFormat.PassNull()!=string.Empty){
        //                    field.NumberFormat = item.DataFormat;
        //                }
                        
        //            }
        //        }

             
        //        foreach (var item in lstFilter)
        //        {
        //            var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
        //            if (col != null)
        //            {
        //                pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Page, col.ColumnName);
        //                pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Page)[col.ColumnName].IsMultipleItemSelectionAllowed = true;
        //            }
        //        }

        //        //pivotTable.CalculateData();
        //        string fileName = Server.MapPath("~/Export") + @"\" +  Guid.NewGuid().ToString() + ".xlsx";
        //        workbook.Save(fileName, SaveFormat.Xlsx);
        //        workbook.Save(stream, SaveFormat.Xlsx);
        //        stream.Flush();
        //        stream.Position = 0;

        //        return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = name + ".xlsx" };

        //    }
        //    catch (Exception ex)
        //    {

        //        if (ex is MessageException)
        //        {
        //            return (ex as MessageException).ToMessage();
        //        }
        //        else
        //        {
        //            return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //        }
        //    }
        //}
        private string GetHandle(List<SYS_ReportOLAPTemplate> lstColumn, SYS_ReportOLAPTemplate col)
        {
            string handler = "";
            foreach (var item in lstColumn)
            {
                if (item.ColumnName != col.ColumnName && item.FilterBy.Split('#').Contains(col.ColumnName))
                {
                    if (item.ParmType.ToUpper() == "Grid".ToUpper())
                    {
                        handler += "App.Parm_" + item.ColumnName + ".store.clearData();" + "App.Parm_" + item.ColumnName + ".view.refresh();";
                    }
                    else
                    {
                        handler += "App.Parm_" + item.ColumnName + ".store.reload();";
                    }
                }
            }
            return handler;
        }
        private object GetRPTParm(List<SYS_ReportOLAPTemplate> lstColumn,List<ParmData> lstParm,string parmName, object defaultVal)
        {
            var col = lstColumn.FirstOrDefault(p => p.RPTParm.ToLower() == parmName.ToLower());
            if (col != null)
            {
                var parm = lstParm.FirstOrDefault(p => p.Name == "Parm_" +  col.ColumnName);
                if(parm!=null)  return parm.Value;
            }
            return defaultVal;
        }
        //private ConsolidationFunction GetFunction(string func)
        //{
        //    switch (func.ToUpper())
        //    {
        //        case "SUM": return ConsolidationFunction.Sum;
        //        case "COUNT": return ConsolidationFunction.Count;
        //        case "AVG": return ConsolidationFunction.Average;
        //        case "MIN": return ConsolidationFunction.Min;
        //        case "MAX": return ConsolidationFunction.Max;
        //        default: return ConsolidationFunction.Sum;
        //    }
        //}
        private XlConsolidationFunction GetFunction(string func)
        {
            switch (func.ToUpper())
            {
                case "SUM": return XlConsolidationFunction.xlSum;
                case "COUNT": return XlConsolidationFunction.xlCount;
                case "AVG": return XlConsolidationFunction.xlAverage;
                case "MIN": return XlConsolidationFunction.xlMin;
                case "MAX": return XlConsolidationFunction.xlMax;
                default: return XlConsolidationFunction.xlSum;
            }
        }

        [DirectMethod]
        public ActionResult IF30100LoadRPTParm(string ReportNbr, string ReportName)
        {

            var obj = _eBiz4DSys.SYS_Configurations.Where(p => p.Code == "isRPTDefault").FirstOrDefault();// (ConfigurationManager.AppSettings["isRPTDefault"].PassNull() == "1") ? true : false;
            bool isRPTDefault = obj == null ? false : (obj.IntVal == 1 ? true : false);
            StoreParameterCollection lstp = new StoreParameterCollection();
            lstp = getStoreParameterCollection(ReportNbr);

            SYS_ReportExportParm objRPTParm = _eBiz4DSys.SYS_ReportExportParm.FirstOrDefault(p => p.ReportNbr.Equals(ReportNbr));

           
            HQCombo StringParm00 = new HQCombo() { HQFirstDefault = isRPTDefault, Name = "StringParm00", ID = "StringParm00", Hidden = true, MarginSpec = "5,0,0,0", LabelWidth = 120, AnchorHorizontal = "100%", HQColumnShow = "*", MultiSelect = true };// this.GetCmp<ComboBox>("cboStringParm00");
            HQCombo StringParm01 = new HQCombo() { HQFirstDefault = isRPTDefault, Name = "StringParm01", ID = "StringParm01", Hidden = true, MarginSpec = "5,0,0,0", LabelWidth = 120, AnchorHorizontal = "100%", HQColumnShow = "*", MultiSelect = true };// this.GetCmp<ComboBox>("cboStringParm01");
            HQCombo StringParm02 = new HQCombo() { HQFirstDefault = isRPTDefault, Name = "StringParm02", ID = "StringParm02", Hidden = true, MarginSpec = "5,0,0,0", LabelWidth = 120, AnchorHorizontal = "100%", HQColumnShow = "*", MultiSelect = true };// this.GetCmp<ComboBox>("cboStringParm02");
            HQCombo StringParm03 = new HQCombo() { HQFirstDefault = isRPTDefault, Name = "StringParm03", ID = "StringParm03", Hidden = true, MarginSpec = "5,0,0,0", LabelWidth = 120, AnchorHorizontal = "100%", HQColumnShow = "*", MultiSelect = true };// this.GetCmp<ComboBox>("cboStringParm03");

            HQDateField cboDate00 = new HQDateField() { Name = "cboDate00", ID = "cboDate00", Hidden = true, Value = DateTime.Now, Width = 230, LabelWidth = 120 };// this.GetCmp<DateField>("cboDate00");
            HQDateField cboDate01 = new HQDateField() { Name = "cboDate01", ID = "cboDate01", Hidden = true, Value = DateTime.Now, Width = 230, LabelWidth = 120 };// this.GetCmp<DateField>("cboDate01");
            HQDateField cboDate02 = new HQDateField() { Name = "cboDate02", ID = "cboDate02", Hidden = true, Value = DateTime.Now, Width = 230, LabelWidth = 120 };//this.GetCmp<DateField>("cboDate02");
            HQDateField cboDate03 = new HQDateField() { Name = "cboDate03", ID = "cboDate03", Hidden = true, Value = DateTime.Now, Width = 230, LabelWidth = 120 };//this.GetCmp<DateField>("cboDate03");


            HQCheckbox chk00 = new HQCheckbox() { Name = "chk00", ID = "chk00", Hidden = true, Checked = false };// this.GetCmp<Checkbox>("chk00");
            HQCheckbox chk01 = new HQCheckbox() { Name = "chk01", ID = "chk01", Hidden = true, Checked = false };// this.GetCmp<Checkbox>("chk01");
            HQCheckbox chk02 = new HQCheckbox() { Name = "chk02", ID = "chk02", Hidden = true, Checked = false };// this.GetCmp<Checkbox>("chk02");
            HQCheckbox chk03 = new HQCheckbox() { Name = "chk03", ID = "chk03", Hidden = true, Checked = false };// this.GetCmp<Checkbox>("chk03");

            var pnlDate = this.GetCmp<Panel>("pnlDate");
            var pnlStringParm = this.GetCmp<Panel>("pnlStringParm");
            var pnlBoaleanParm = this.GetCmp<Panel>("pnlBoaleanParm");
            var tabList = this.GetCmp<TabPanel>("tabList");
            var pnlButton = this.GetCmp<TabPanel>("pnlButton");
          
            Component component = new Component() { ID = "component", Flex = 1 };
            Ext.Net.Button btnLoadParamList = new Ext.Net.Button() { ID = "btnLoadParamList", Text = Util.GetLang("reloadfilterlist"), MarginSpec = "3 0 0 0", Hidden = true };


            HQGridPanel List00 = new HQGridPanel() { ID = "List0", Hidden = true, MultiSelect = true, HQAutoLoad = false, HQColumnSelect = true, RowLines = true, ColumnLines = true };
            HQGridPanel List01 = new HQGridPanel() { ID = "List1", Hidden = true, MultiSelect = true, HQAutoLoad = false, HQColumnSelect = true, RowLines = true, ColumnLines = true };
            HQGridPanel List02 = new HQGridPanel() { ID = "List2", Hidden = true, MultiSelect = true, HQAutoLoad = false, HQColumnSelect = true, RowLines = true, ColumnLines = true };
            HQGridPanel List03 = new HQGridPanel() { ID = "List3", Hidden = true, MultiSelect = true, HQAutoLoad = false, HQColumnSelect = true, RowLines = true, ColumnLines = true };

            HQGridPanel List0 = new HQGridPanel()
            {
                ID = "List0",
                Hidden = true,
                MultiSelect = true,
                HQisPaging = true,
                HQPageSize = 50,
                HQAutoLoad = false,
                HQColumnSelect = true
                    ,
                BottomBar ={
                    new Ext.Net.PagingToolbar(){
                         ID="PagingTool0"
                        ,Items = {
                            new Ext.Net.Label("Page size:"),
                            new Ext.Net.ToolbarSpacer(10),
                            new Ext.Net.ComboBox(){
                                Width = 80,
                                Items = {
                                    new Ext.Net.ListItem("1"),
                                    new Ext.Net.ListItem("2"),
                                    new Ext.Net.ListItem("10"),
                                    new Ext.Net.ListItem("30"),
                                    new Ext.Net.ListItem("50")
                                },
                                SelectedItems = {new Ext.Net.ListItem("50")},
                                Listeners = {
                                    Select = {
                                        Fn = "HQ.grid.onPageSelect"
                                    }
                                }
                            }
                        },
                        Plugins = { new Ext.Net.ProgressBarPager()}
                    }
                },
            };//  this.GetCmp<Panel>("List0");

            HQGridPanel List1 = new HQGridPanel()
            {
                ID = "List1",
                Hidden = true,
                MultiSelect = true,
                HQisPaging = true,
                HQPageSize = 50,
                HQAutoLoad = false,
                HQColumnSelect = true,
                BottomBar ={
                    new Ext.Net.PagingToolbar(){
                         ID="PagingTool1"
                        ,Items = {
                            new Ext.Net.Label("Page size:"),
                            new Ext.Net.ToolbarSpacer(10),
                            new Ext.Net.ComboBox(){
                                Width = 80,
                                Items = {
                                    new Ext.Net.ListItem("1"),
                                    new Ext.Net.ListItem("2"),
                                    new Ext.Net.ListItem("10"),
                                    new Ext.Net.ListItem("30"),
                                    new Ext.Net.ListItem("50")
                                },
                                SelectedItems = {new Ext.Net.ListItem("50")},
                                Listeners = {
                                    Select = {
                                        Fn = "HQ.grid.onPageSelect"
                                    }
                                }
                            }
                        },
                        Plugins = { new Ext.Net.ProgressBarPager()}
                    }
                },
            };//  this.GetCmp<Panel>("List0");
            HQGridPanel List2 = new HQGridPanel()
            {
                ID = "List2",
                Hidden = true,
                MultiSelect = true,
                HQisPaging = true,
                HQPageSize = 50,
                HQAutoLoad = false,
                HQColumnSelect = true
                ,
                BottomBar ={
                    new Ext.Net.PagingToolbar(){
                        Items = {
                            new Ext.Net.Label("Page size:"),
                            new Ext.Net.ToolbarSpacer(10),
                            new Ext.Net.ComboBox(){
                                Width = 80,
                                Items = {
                                    new Ext.Net.ListItem("1"),
                                    new Ext.Net.ListItem("2"),
                                    new Ext.Net.ListItem("10"),
                                    new Ext.Net.ListItem("30"),
                                    new Ext.Net.ListItem("50")
                                },
                                SelectedItems = {new Ext.Net.ListItem("50")},
                                Listeners = {
                                    Select = {
                                        Fn = "HQ.grid.onPageSelect"
                                    }
                                }
                            }
                        },
                        Plugins = { new Ext.Net.ProgressBarPager()}
                    }
                },
            };//  this.GetCmp<Panel>("List2");
            HQGridPanel List3 = new HQGridPanel()
            {
                ID = "List3",
                Hidden = true,
                MultiSelect = true,
                HQisPaging = true,
                HQPageSize = 50,
                HQAutoLoad = false,
                HQColumnSelect = true
                ,
                BottomBar ={
                    new Ext.Net.PagingToolbar(){
                        Items = {
                            new Ext.Net.Label("Page size:"),
                            new Ext.Net.ToolbarSpacer(10),
                            new Ext.Net.ComboBox(){
                                Width = 80,
                                Items = {
                                    new Ext.Net.ListItem("1"),
                                    new Ext.Net.ListItem("2"),
                                    new Ext.Net.ListItem("10"),
                                    new Ext.Net.ListItem("30"),
                                    new Ext.Net.ListItem("50")
                                },
                                SelectedItems = {new Ext.Net.ListItem("50")},
                                Listeners = {
                                    Select = {
                                        Fn = "HQ.grid.onPageSelect"
                                    }
                                }
                            }
                        },
                        Plugins = { new Ext.Net.ProgressBarPager()}
                    }
                },
            };//  this.GetCmp<Panel>("List3");


            if (objRPTParm != null)
            {

                //List Parameter
                List0.Title = Util.GetLang(objRPTParm.ListCap00);
                List1.Title = Util.GetLang(objRPTParm.ListCap01);
                List2.Title = Util.GetLang(objRPTParm.ListCap02);
                List3.Title = Util.GetLang(objRPTParm.ListCap03);
                //Date parameter
                cboDate00.HQLangCode = objRPTParm.DateCap00.Split(';').Length > 0 ? objRPTParm.DateCap00.Split(';')[0] : objRPTParm.DateCap00;
                cboDate01.HQLangCode = objRPTParm.DateCap01.Split(';').Length > 0 ? objRPTParm.DateCap01.Split(';')[0] : objRPTParm.DateCap01;
                cboDate02.HQLangCode = objRPTParm.DateCap02.Split(';').Length > 0 ? objRPTParm.DateCap02.Split(';')[0] : objRPTParm.DateCap02;
                cboDate03.HQLangCode = objRPTParm.DateCap03.Split(';').Length > 0 ? objRPTParm.DateCap03.Split(';')[0] : objRPTParm.DateCap03;
                //String parameter
                StringParm00.HQLangCode = objRPTParm.StringCap00;
                StringParm01.HQLangCode = objRPTParm.StringCap01;
                StringParm02.HQLangCode = objRPTParm.StringCap02;
                StringParm03.HQLangCode = objRPTParm.StringCap03;
                //Boolean Parameter                    
                chk00.HQLangCode = objRPTParm.BooleanCap00;
                chk01.HQLangCode = objRPTParm.BooleanCap01;
                chk02.HQLangCode = objRPTParm.BooleanCap02;
                chk03.HQLangCode = objRPTParm.BooleanCap03;

                #region List Param
                if (!objRPTParm.ListCap00.ToString().Trim().Equals(""))
                {
                    List0.Hidden = false;
                 
                    //List0.Tag = objRPTParm.ListProc00.Trim();
                    List0.HQProcedure = objRPTParm.ListProc00.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                    List0.HQDBSys = false;
                    //List0.SelectionModel.Add(new CheckboxSelectionModel() { Mode = SelectionMode.Multi,});
                    List0.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                    List0.LoadData();

                    //this.List0.Header = objRPTParm.ListCap00;
                    if (!objRPTParm.ListCap01.ToString().Trim().Equals(""))
                    {
                        List1.Hidden = false;
                      
                        //List1.Tag = objRPTParm.ListProc01.Trim();
                        List1.HQProcedure = objRPTParm.ListProc01.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                        List1.HQDBSys = false;
                        //List1.SelectionModel.Add(new CheckboxSelectionModel() { Mode = SelectionMode.Multi });
                        List1.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                        List1.LoadData();
                        //this.List1.Header = objRPTParm.ListCap01;
                        if (!objRPTParm.ListCap02.ToString().Trim().Equals(""))
                        {
                            List2.Hidden = false;
                         
                            //List2.Tag = objRPTParm.ListProc02.Trim();
                            List2.HQProcedure = objRPTParm.ListProc02.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                            List2.HQDBSys = false;
                            //List2.SelectionModel.Add(new CheckboxSelectionModel() { Mode = SelectionMode.Multi });
                            List2.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                            List2.LoadData();
                            //this.List2.Header = objRPTParm.ListCap02;
                            if (!objRPTParm.ListCap03.ToString().Trim().Equals(""))
                            {
                                List3.Hidden = false;
                              
                                //List3.Tag = objRPTParm.ListProc03.Trim();
                                List3.HQProcedure = objRPTParm.ListProc03.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                                List3.HQDBSys = false;
                                //List3.SelectionModel.Add(new CheckboxSelectionModel() { Mode = SelectionMode.Multi });
                                List3.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                                List3.LoadData();
                            }
                        }
                    }
                   
                   
                }
                #endregion
                #region//Date parameter
                if (!objRPTParm.DateCap00.ToString().Trim().Equals(""))
                {
                    if (objRPTParm.DateCap00.Split(';').Length > 1 && objRPTParm.DateCap00.Split(';')[1].ToLower() == "p")
                    {
                        cboDate00.Type = DatePickerType.Month;
                    }

                    cboDate00.Hidden = false;
                    string handler = "";
                    if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@DateParm00"))
                    {
                        handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                    }
                    if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@DateParm00"))
                    {
                        handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                    }
                    if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@DateParm00"))
                    {
                        handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                    }
                    if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@DateParm00"))
                    {
                        handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                    }


                    cboDate00.Listeners.Change.Handler = handler;
                    if (!objRPTParm.DateCap01.ToString().Trim().Equals(""))
                    {
                        if (objRPTParm.DateCap01.Split(';').Length > 1 && objRPTParm.DateCap01.Split(';')[1].ToLower() == "p")
                        {

                            cboDate01.Type = DatePickerType.Month;
                        }

                        cboDate01.Hidden = false;
                        handler = "";
                        if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@DateParm01"))
                        {
                            handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@DateParm01"))
                        {
                            handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@DateParm01"))
                        {
                            handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@DateParm01"))
                        {
                            handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                        }


                        cboDate01.Listeners.Change.Handler = handler;
                        if (!objRPTParm.DateCap02.ToString().Trim().Equals(""))
                        {
                            if (objRPTParm.DateCap02.Split(';').Length > 1 && objRPTParm.DateCap02.Split(';')[1].ToLower() == "p")
                            {
                                cboDate02.Type = DatePickerType.Month;
                            }

                            cboDate02.Hidden = false;
                            handler = "";
                            if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@DateParm02"))
                            {
                                handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@DateParm02"))
                            {
                                handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@DateParm02"))
                            {
                                handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@DateParm02"))
                            {
                                handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                            }



                            cboDate02.Listeners.Change.Handler = handler;
                            if (!objRPTParm.DateCap03.ToString().Trim().Equals(""))
                            {

                                if (objRPTParm.DateCap03.Split(';').Length > 1 && objRPTParm.DateCap03.Split(';')[1].ToLower() == "p")
                                {
                                    cboDate03.Type = DatePickerType.Month;
                                }

                                cboDate03.Hidden = false;
                                handler = "";
                                if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@DateParm03"))
                                {
                                    handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@DateParm03"))
                                {
                                    handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@DateParm03"))
                                {
                                    handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@DateParm03"))
                                {
                                    handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                                }


                                cboDate03.Listeners.Change.Handler = handler;
                            }
                        }
                    }
                }
                #endregion
                #region//String parameter
                if (!objRPTParm.StringCap00.Trim().Equals(""))
                {



                    StringParm00.Hidden = false;
                    if (!objRPTParm.PPV_Proc00.Trim().Equals(""))
                    {
                        StringParm00.Tag = objRPTParm.PPV_Proc00.Trim();
                        StringParm00.HQProcedure = objRPTParm.PPV_Proc00.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                        StringParm00.HQDBSys = false;
                        StringParm00.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                        StringParm00.DisplayField = objRPTParm.PPV_Proc00.Trim().Split(';').Length > 1 ? objRPTParm.PPV_Proc00.Trim().Split(';')[1] : "";
                        StringParm00.LoadData();
                        string handler = "";
                        if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@StringParm00"))
                        {
                            handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@StringParm00"))
                        {
                            handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@StringParm00"))
                        {
                            handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@StringParm00"))
                        {
                            handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                        }
                        StringParm00.Listeners.Change.Handler = handler;

                    }
                    if (!objRPTParm.StringCap01.Trim().Equals(""))
                    {
                        StringParm01.Hidden = false;
                        if (!objRPTParm.PPV_Proc01.Trim().Equals(""))
                        {
                            StringParm01.Tag = objRPTParm.PPV_Proc01.Trim();
                            StringParm01.HQProcedure = objRPTParm.PPV_Proc01.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                            StringParm01.HQDBSys = false;
                            StringParm01.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                            StringParm01.DisplayField = objRPTParm.PPV_Proc01.Trim().Split(';').Length > 1 ? objRPTParm.PPV_Proc01.Trim().Split(';')[1] : "";
                            StringParm01.LoadData();
                            string handler = "";
                            if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@StringParm01"))
                            {
                                handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@StringParm01"))
                            {
                                handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@StringParm01"))
                            {
                                handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@StringParm01"))
                            {
                                handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                            }
                            StringParm01.Listeners.Change.Handler = handler;
                        }

                        if (!objRPTParm.StringCap02.Trim().Equals(""))
                        {
                            StringParm02.Hidden = false;
                            if (!objRPTParm.PPV_Proc02.Trim().Equals(""))
                            {
                                StringParm02.Tag = objRPTParm.PPV_Proc02.Trim();
                                StringParm02.HQProcedure = objRPTParm.PPV_Proc02.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                                StringParm02.HQDBSys = false;
                                StringParm02.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                                StringParm02.DisplayField = objRPTParm.PPV_Proc02.Trim().Split(';').Length > 1 ? objRPTParm.PPV_Proc02.Trim().Split(';')[1] : "";
                                StringParm02.LoadData();
                                string handler = "";
                                if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@StringParm02"))
                                {
                                    handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@StringParm02"))
                                {
                                    handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@StringParm02"))
                                {
                                    handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@StringParm02"))
                                {
                                    handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                                }
                                StringParm02.Listeners.Change.Handler = handler;
                            }

                            if (!objRPTParm.StringCap03.Trim().Equals(""))
                            {

                                StringParm03.Hidden = false;
                                if (!objRPTParm.PPV_Proc03.Trim().Equals(""))
                                {
                                    StringParm03.Tag = objRPTParm.PPV_Proc03.Trim();
                                    StringParm03.HQProcedure = objRPTParm.PPV_Proc03.Trim().Split(';').FirstOrDefault().Split(',').FirstOrDefault();
                                    StringParm03.HQDBSys = false;
                                    StringParm03.HQParam = lstp;// (new StoreParameterCollection { new StoreParameter() { Name = "@ReportNbr", Value = "App.cboRptList.getValue()", Mode = ParameterMode.Raw } });
                                    StringParm03.DisplayField = objRPTParm.PPV_Proc03.Trim().Split(';').Length > 1 ? objRPTParm.PPV_Proc03.Trim().Split(';')[1] : "";
                                    //StringParm03.ValueField = "";
                                    StringParm03.LoadData();
                                    string handler = "";
                                    if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@StringParm03"))
                                    {
                                        handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                                    }
                                    if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@StringParm03"))
                                    {
                                        handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                                    }
                                    if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@StringParm03"))
                                    {
                                        handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                                    }
                                    if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@StringParm03"))
                                    {
                                        handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                                    }
                                    StringParm03.Listeners.Change.Handler = handler;
                                }
                            }
                        }
                    }
                }
                #endregion
                #region//Boolean Parameter
                if (!objRPTParm.BooleanCap00.ToString().Trim().Equals(""))
                {

                    chk00.Hidden = false;
                    chk00.Checked = false;
                    string handler = "";
                    if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@BooleanParm00"))
                    {
                        handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                    }
                    if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@BooleanParm00"))
                    {
                        handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                    }
                    if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@BooleanParm00"))
                    {
                        handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                    }
                    if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@BooleanParm00"))
                    {
                        handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                    }



                    chk00.Listeners.Change.Handler = handler;
                    if (!objRPTParm.BooleanCap01.ToString().Trim().Equals(""))
                    {
                        chk01.Hidden = false;
                        chk01.Checked = false;
                        handler = "";
                        if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@BooleanParm01"))
                        {
                            handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@BooleanParm01"))
                        {
                            handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@BooleanParm01"))
                        {
                            handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                        }
                        if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@BooleanParm01"))
                        {
                            handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                        }



                        chk01.Listeners.Change.Handler = handler;
                        if (!objRPTParm.BooleanCap02.ToString().Trim().Equals(""))
                        {

                            chk02.Hidden = false;
                            chk02.Checked = false;
                            handler = "";
                            if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@BooleanParm02"))
                            {
                                handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@BooleanParm02"))
                            {
                                handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@BooleanParm02"))
                            {
                                handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                            }
                            if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@BooleanParm02"))
                            {
                                handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                            }



                            chk02.Listeners.Change.Handler = handler;
                            if (!objRPTParm.BooleanCap03.ToString().Trim().Equals(""))
                            {

                                chk03.Hidden = false;
                                chk03.Checked = false;
                                handler = "";
                                if (objRPTParm.PPV_Proc00.PassNull().ToString().Contains("@BooleanParm03"))
                                {
                                    handler = "App.StringParm00.clearValue();App.StringParm00.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc01.PassNull().ToString().Contains("@BooleanParm03"))
                                {
                                    handler += "App.StringParm01.clearValue();App.StringParm01.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc02.PassNull().ToString().Contains("@BooleanParm03"))
                                {
                                    handler += "App.StringParm02.clearValue();App.StringParm02.getStore().reload();";
                                }
                                if (objRPTParm.PPV_Proc03.PassNull().ToString().Contains("@BooleanParm03"))
                                {
                                    handler += "App.StringParm03.clearValue();App.StringParm03.getStore().reload();";
                                }

                                chk03.Listeners.Change.Handler = handler;
                            }
                        }
                    }
                }
                #endregion
            }

            cboDate00.AddTo(pnlDate);
            cboDate01.AddTo(pnlDate);
            cboDate02.AddTo(pnlDate);
            cboDate03.AddTo(pnlDate);


            StringParm00.AddTo(pnlStringParm);
            StringParm01.AddTo(pnlStringParm);
            StringParm02.AddTo(pnlStringParm);
            StringParm03.AddTo(pnlStringParm);

            chk00.AddTo(pnlBoaleanParm);
            chk01.AddTo(pnlBoaleanParm);
            chk02.AddTo(pnlBoaleanParm);
            chk03.AddTo(pnlBoaleanParm);
            if (objRPTParm != null)
            {

                if (!objRPTParm.ListCap00.ToString().Trim().Equals(""))
                {
                    List0.AddTo(tabList);
                }
                else
                {
                    List00.AddTo(tabList);
                }

                if (!objRPTParm.ListCap01.ToString().Trim().Equals(""))
                {
                    List1.AddTo(tabList);
                }
                else
                {
                    List01.AddTo(tabList);
                }
                if (!objRPTParm.ListCap02.ToString().Trim().Equals(""))
                {
                    List2.AddTo(tabList);
                }
                else
                {
                    List02.AddTo(tabList);
                }
                if (!objRPTParm.ListCap03.ToString().Trim().Equals(""))
                {
                    List3.AddTo(tabList);
                }
                else
                {
                    List03.AddTo(tabList);
                }

            }
            else
            {
                List00.AddTo(tabList);
                List01.AddTo(tabList);
                List02.AddTo(tabList);
                List03.AddTo(tabList);
            }

            tabList.Hidden = true;
            pnlDate.Hidden = true;
            pnlStringParm.Hidden = true;
            pnlBoaleanParm.Hidden = true;

            if (!List0.Hidden || !List1.Hidden || !List2.Hidden || !List3.Hidden)//ẩn nút load danh sách tham số
            {

                string handler = "HQ.SourceList=0;HQ.common.showBusy(true, HQ.waitMsg);";
                if (!List3.Hidden)
                {
                    handler += "App.List0.getStore().addListener('load', List_Load(4));";
                    handler += "App.List1.getStore().addListener('load', List_Load(4));";
                    handler += "App.List2.getStore().addListener('load', List_Load(4));";
                    handler += "App.List3.getStore().addListener('load', List_Load(4));";

                    handler += "App.List0.getStore().reload();";
                    handler += "App.List1.getStore().reload();";
                    handler += "App.List2.getStore().reload();";
                    handler += "App.List3.getStore().reload();";                  
                }
                else if (!List2.Hidden)
                {
                    handler += "App.List0.getStore().addListener('load', List_Load(3));";
                    handler += "App.List1.getStore().addListener('load', List_Load(3));";
                    handler += "App.List2.getStore().addListener('load', List_Load(3));";

                    handler += "App.List0.getStore().reload();";
                    handler += "App.List1.getStore().reload();";
                    handler += "App.List2.getStore().reload();";                   
                }
                else if (!List1.Hidden)
                {
                    handler += "App.List0.getStore().addListener('load', List_Load(2));";
                    handler += "App.List1.getStore().addListener('load', List_Load(2));";                    
                    handler += "App.List0.getStore().reload();";
                    handler += "App.List1.getStore().reload();";                 
                }
                else if (!List0.Hidden)
                {
                    handler += "App.List0.getStore().addListener('load', List_Load(1));";
                    handler += "App.List0.getStore().reload();";                   
                }
                btnLoadParamList.Listeners.Click.Handler = handler;                                       
                btnLoadParamList.Hidden = false;
                tabList.Hidden = false;
            }

            if (!cboDate00.Hidden || !cboDate01.Hidden || !cboDate02.Hidden || !cboDate03.Hidden)//ẩn nút load danh sách tham số
            {
                pnlDate.Hidden = false;

            }
            if (!StringParm00.Hidden || !StringParm01.Hidden || !StringParm02.Hidden || !StringParm03.Hidden)//ẩn nút load danh sách tham số
            {

                pnlStringParm.Hidden = false;
            }
            if (!chk00.Hidden || !chk01.Hidden || !chk02.Hidden || !chk03.Hidden)//ẩn nút load danh sách tham số
            {

                pnlBoaleanParm.Hidden = false;
            }

            component.AddTo(pnlButton);
            btnLoadParamList.AddTo(pnlButton);
            return this.Direct();
        }

        private StoreParameterCollection getStoreParameterCollection(string ReportNbr)
        {
            StoreParameterCollection lstp = new StoreParameterCollection();

            lstp.Add(new StoreParameter() { Name = "@ReportNbr", Value = ReportNbr, Mode = ParameterMode.Value });
            lstp.Add(new StoreParameter() { Name = "@ReportDate", Value = DateTime.Now.ToString("yyyyMMdd"), Mode = ParameterMode.Value });

            lstp.Add(new StoreParameter() { Name = "@DateParm00", Value = "App.cboDate00.getValue()", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@DateParm01", Value = "App.cboDate01.getValue()", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@DateParm02", Value = "App.cboDate02.getValue()", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@DateParm03", Value = "App.cboDate03.getValue()", Mode = ParameterMode.Raw });

            lstp.Add(new StoreParameter() { Name = "@BooleanParm00", Value = "App.chk00.getValue()==false?0:1", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@BooleanParm01", Value = "App.chk01.getValue()==false?0:1", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@BooleanParm02", Value = "App.chk02.getValue()==false?0:1", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@BooleanParm03", Value = "App.chk03.getValue()==false?0:1", Mode = ParameterMode.Raw });

            lstp.Add(new StoreParameter() { Name = "@StringParm00", Value = "App.StringParm00.valueModels?App.StringParm00.getValue().join(','):''", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@StringParm01", Value = "App.StringParm01.valueModels?App.StringParm01.getValue().join(','):''", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@StringParm02", Value = "App.StringParm02.valueModels?App.StringParm02.getValue().join(','):''", Mode = ParameterMode.Raw });
            lstp.Add(new StoreParameter() { Name = "@StringParm03", Value = "App.StringParm03.valueModels?App.StringParm03.getValue().join(','):''", Mode = ParameterMode.Raw });


            lstp.Add(new StoreParameter() { Name = "@UserID", Value = Current.UserName, Mode = ParameterMode.Value });

            return lstp;
        }

        [HttpPost]
        public ActionResult ExportProc(FormCollection data, string reportNbr, string name, string proc,bool isReadOnly)
        {
            try
            {
                //SelectedRowCollection List0 = JSON.Deserialize<SelectedRowCollection>(data["list0"]);
                //SelectedRowCollection List1 = JSON.Deserialize<SelectedRowCollection>(data["list1"]);
                //SelectedRowCollection List2 = JSON.Deserialize<SelectedRowCollection>(data["list2"]);
                //SelectedRowCollection List3 = JSON.Deserialize<SelectedRowCollection>(data["list3"]);

                var created = new RPTRunning();
                short bit1 = 1;
                short bit0 = 0;
                created.AppPath = "Reports\\";
                created.BooleanParm00 = data["chk00"].PassNull() != "" ? bit1 : bit0;
                created.BooleanParm01 = data["chk01"].PassNull() != "" ? bit1 : bit0;
                created.BooleanParm02 = data["chk02"].PassNull() != "" ? bit1 : bit0;
                created.BooleanParm03 = data["chk03"].PassNull() != "" ? bit1 : bit0;
                created.ClientName = Current.UserName;
                created.CpnyID = Current.CpnyID;
                created.DateParm00 = Convert.ToDateTime(data["cboDate00"]);
                created.DateParm01 = Convert.ToDateTime(data["cboDate01"]);
                created.DateParm02 = Convert.ToDateTime(data["cboDate02"]);
                created.DateParm03 = Convert.ToDateTime(data["cboDate03"]);
                created.LangID = Current.LangID;
                created.LoggedCpnyID = Current.CpnyID;
                created.MachineName = "Web";
                created.ReportCap = reportNbr;
                created.ReportDate = DateTime.Now.ToDateShort();
                created.ReportID = 0;
                created.ReportName = reportNbr;
                created.ReportNbr = reportNbr;
                created.SelectionFormular = "";
                created.StringParm00 = data["StringParm00"];
                created.StringParm01 = data["StringParm01"];
                created.StringParm02 = data["StringParm02"];
                created.StringParm03 = data["StringParm03"];
                created.UserID = Current.UserName;

                _db.RPTRunnings.AddObject(created);
                _db.SaveChanges();



                #region//Insert RptParm0 RPTRunningParm0 parm0;
                int i = 0;
                if (data["list0"] != null)
                    foreach (var ID in data["list0"].PassNull().TrimEnd(',').Split(','))
                    {
                        if (ID.PassNull() != "")
                        {
                            i++;
                            var parm0 = new RPTRunningParm0();
                            parm0.ReportNbr = created.ReportNbr;
                            parm0.ReportID = created.ReportID;
                            parm0.MachineName = "Web";
                            parm0.LineRef = i.ToString();
                            parm0.StringParm = ID;
                            parm0.DateParm = DateTime.Now;
                            parm0.NumericParm = 0;
                            parm0.Crtd_DateTime = DateTime.Now;
                            parm0.Crtd_Prog = created.ReportNbr;
                            parm0.Crtd_User = Current.UserName;
                            parm0.LUpd_DateTime = DateTime.Now;
                            parm0.LUpd_Prog = created.ReportNbr;
                            parm0.LUpd_User = Current.UserName;
                            parm0.tstamp = new byte[1];
                            _db.RPTRunningParm0.AddObject(parm0);
                        }

                    }
                #endregion
                #region//Insert RptParm1 RPTRunningParm1 parm1;
                i = 0;
                if (data["list1"] != null)
                    foreach (var ID in data["list1"].PassNull().TrimEnd(',').Split(','))
                    {
                        if (ID.PassNull() != "")
                        {
                            i++;
                            var parm1 = new RPTRunningParm1();
                            parm1.ReportNbr = created.ReportNbr;
                            parm1.ReportID = created.ReportID;
                            parm1.MachineName = "Web";
                            parm1.LineRef = i.ToString();
                            parm1.StringParm = ID;
                            parm1.DateParm = DateTime.Now;
                            parm1.NumericParm = 0;
                            parm1.Crtd_DateTime = DateTime.Now;
                            parm1.Crtd_Prog = created.ReportNbr;
                            parm1.Crtd_User = Current.UserName;
                            parm1.LUpd_DateTime = DateTime.Now;
                            parm1.LUpd_Prog = created.ReportNbr;
                            parm1.LUpd_User = Current.UserName;
                            parm1.tstamp = new byte[1];
                            _db.RPTRunningParm1.AddObject(parm1);
                        }
                    }
                #endregion
                #region//Insert RptParm2 RPTRunningParm2 parm2;
                i = 0;
                if (data["list2"] != null)
                    foreach (var ID in data["list2"].PassNull().TrimEnd(',').Split(','))
                    {
                        if (ID.PassNull() != "")
                        {
                            i++;
                            var parm2 = new RPTRunningParm2();
                            parm2.ReportNbr = created.ReportNbr;
                            parm2.ReportID = created.ReportID;
                            parm2.MachineName = "Web";
                            parm2.LineRef = i.ToString();
                            parm2.StringParm = ID;
                            parm2.DateParm = DateTime.Now;
                            parm2.NumericParm = 0;
                            parm2.Crtd_DateTime = DateTime.Now;
                            parm2.Crtd_Prog = created.ReportNbr;
                            parm2.Crtd_User = Current.UserName;
                            parm2.LUpd_DateTime = DateTime.Now;
                            parm2.LUpd_Prog = created.ReportNbr;
                            parm2.LUpd_User = Current.UserName;
                            parm2.tstamp = new byte[2];
                            _db.RPTRunningParm2.AddObject(parm2);
                        }
                    }
                #endregion
                #region//Insert RptParm3 RPTRunningParm3 parm3;
                i = 0;
                if (data["list3"] != null)
                    foreach (var ID in data["list3"].PassNull().TrimEnd(',').Split(','))
                    {
                        if (ID.PassNull() != "")
                        {
                            i++;
                            var parm3 = new RPTRunningParm3();
                            parm3.ReportNbr = created.ReportNbr;
                            parm3.ReportID = created.ReportID;
                            parm3.MachineName = "Web";
                            parm3.LineRef = i.ToString();
                            parm3.StringParm = ID;
                            parm3.DateParm = DateTime.Now;
                            parm3.NumericParm = 0;
                            parm3.Crtd_DateTime = DateTime.Now;
                            parm3.Crtd_Prog = created.ReportNbr;
                            parm3.Crtd_User = Current.UserName;
                            parm3.LUpd_DateTime = DateTime.Now;
                            parm3.LUpd_Prog = created.ReportNbr;
                            parm3.LUpd_User = Current.UserName;
                            parm3.tstamp = new byte[3];
                            _db.RPTRunningParm3.AddObject(parm3);
                        }
                    }
                #endregion
                _db.SaveChanges();

                try
                {
                    Stream stream = new MemoryStream();
                    Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
                    Aspose.Cells.Worksheet SheetData = workbook.Worksheets[0];
                    SheetData.Name = "Data";

                    DataAccess dal = Util.Dal(false,isReadOnly);
                    ParamCollection pc = new ParamCollection();
                    pc.Add(new ParamStruct("@RPTID", DbType.Int16, clsCommon.GetValueDBNull(created.ReportID), ParameterDirection.Input, 50));
                    System.Data.DataTable dtInvtID = dal.ExecDataTable(proc, CommandType.StoredProcedure, ref pc);

                    Cell cell;
                    for (int j = 1; j < dtInvtID.Rows.Count; j++)
                    {
                        for (int x = 0; x < dtInvtID.Columns.Count; x++)
                        {
                            if(j==1)
                                SetCellValueGrid(SheetData.Cells.Rows[0][x], Util.GetLang(dtInvtID.Columns[x].ColumnName), TextAlignmentType.Center, TextAlignmentType.Left);
                            cell = SheetData.Cells[j, x];
                            if (dtInvtID.Columns[x].DataType.ToString().ToUpper().Contains("DATE"))
                            {
                               
                                DateTime tmpValue = DateTime.Parse(dtInvtID.Rows[j][x].ToString());
                                cell.PutValue(tmpValue.ToString(Current.FormatDate));
                            }
                            else
                            {
                                cell.PutValue(dtInvtID.Rows[j][x].ToString());
                            }
                        }
                    }

                    //SheetData.Cells.ImportDataTable(dtCloned, false, "A2");// du lieu Inventory



                    SheetData.AutoFitColumns();

                    string fileName = Guid.NewGuid().ToString() + ".xlsx";
                    string path = Server.MapPath("~/ExportPivot") + @"\" + fileName;


                    workbook.Save(path, SaveFormat.Xlsx);

                    return Json(new { success = true, id = fileName, name = name + ".xlsx" }, JsonRequestBehavior.AllowGet);
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
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }

            //return Json(created.ReportName + created.ReportID + ".xls");
        }
    }
}

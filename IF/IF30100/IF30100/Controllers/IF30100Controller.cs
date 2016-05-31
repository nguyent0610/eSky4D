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
        [HttpPost]
        public ActionResult Export(FormCollection data, string view, string name)
        {
            try
            {
                string select = "";
                string param = "";
                string proc="";
                var detHandler = new StoreDataHandler(data["lstDet"]);
                var lstDet = detHandler.ObjectData<IF30100_pgData_Result>().ToList();
                foreach(var obj in lstDet)
                {
                    select += obj.Checked == true ? obj.Column_Name + "," : "";
                    if (obj.Operator.PassNull() != "")
                    {
                        if (obj.Operator.ToUpper().Trim() == "Between".ToUpper())
                        {
                            param += obj.Column_Name + " Between " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' AND " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value2 + "' AND ";
                        }
                        else if (obj.Operator.ToUpper().Trim() == "AND".ToUpper())
                        {
                            param += obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' AND " + obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value2 + "' AND ";
                        }
                        else if (obj.Operator.ToUpper().Trim() == "OR".ToUpper())
                        {
                            param += obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' OR " + obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value2 + "' AND ";
                        }
                        else if (obj.Operator.ToUpper().Trim() == "IN".ToUpper())
                        {

                            param += obj.Column_Name + " IN('"+ obj.Value1.Replace(",","','")+ "') AND ";
                        }
                        else param += obj.Column_Name + " " + obj.Operator + " " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' AND ";
                        
                    }

                }
                param = param.Length > 3 ?  " Where " + param.Substring(0, param.Length - 4) : param;
                proc="select "+select.TrimEnd(',')+" from " + view +param;
                Stream stream = new MemoryStream();
                Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
                Aspose.Cells.Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = name;


                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();
                System.Data.DataTable dtInvtID = dal.ExecDataTable(proc, CommandType.Text, ref pc);
                SheetData.Cells.ImportDataTable(dtInvtID, true, "A1");// du lieu Inventory

                           

                SheetData.AutoFitColumns();

                

                //SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = name + ".xlsx" };

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
        [HttpPost]
        public ActionResult ExportPivot(FormCollection data, string view, string name)
        {
            Application excelApplication = null;
            Microsoft.Office.Interop.Excel.Workbook excelWorkBook = null;
            Microsoft.Office.Interop.Excel.Worksheet targetSheet = null;
            Microsoft.Office.Interop.Excel.Worksheet dataSheet = null;
            PivotTable pivotTable;
            Microsoft.Office.Interop.Excel.Range pivotData;
            Microsoft.Office.Interop.Excel.Range pivotDestination;
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
                DataAccess dal = Util.Dal();
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

                    pc.Add(new ParamStruct("@CpnyID", DbType.String, user !=null ? user.CpnyID : "" , ParameterDirection.Input, 200));
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

                var lstFilter = lstColumn.Where(p => p.PivotType == "F").OrderBy(p => p.PivotOrder).ToList();

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
                    PivotField pvf = pivotTable.PivotFields(Util.GetLang(row.ColumnDescr));
                    if (row.PivotShow)
                    {
                        pvf.Orientation = XlPivotFieldOrientation.xlRowField;
                        pvf.Subtotals[1] = true;
                        pvf.Subtotals[1] = false;
                        
                    }
                   
                    //foreach (var item in pvf.PivotItems())
                    //{
                    //    item.ShowDetail = false;
                    //}

                }

                var lstCol = lstColumn.Where(p => p.PivotType == "C").OrderBy(p => p.PivotOrder).ToList();
                foreach (var col in lstCol)
                {

                    PivotField pvf = pivotTable.PivotFields(Util.GetLang(col.ColumnDescr));
                    if (col.PivotShow)
                    {
                        pvf.Orientation = XlPivotFieldOrientation.xlColumnField;
                        pvf.Subtotals[1] = true;
                        pvf.Subtotals[1] = false;
                       
                    }
                  
                    //foreach (var item in pvf.PivotItems())
                    //{
                    //    item.ShowDetail = false;
                    //}
                }

              
                foreach (var measure in lstMeasure)
                {
                    PivotField pvf = pivotTable.PivotFields(Util.GetLang(measure.ColumnDescr));
                    if (measure.PivotShow)
                    {
                        pvf.Orientation = XlPivotFieldOrientation.xlDataField;
                        pvf.Function = GetFunction(measure.MeasureFunc);
                        if (measure.DataFormat.PassNull()!=string.Empty)
                            pvf.NumberFormat = measure.DataFormat;
                        else
                            pvf.NumberFormat = "#,##";
                    }
                  
               
                    //foreach (var item in pvf.PivotItems())
                    //{
                    //    item.ShowDetail = false;
                    //}
                }


                foreach (var colFilter in lstFilter)
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
                            DateTime date = Convert.ToDateTime(parm.Value.Substring(8, 2) + "/" + parm.Value.Substring(5, 2) + "/" + parm.Value.Substring(0, 4));
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
                            targetSheet.Cells[start, 4].Value = parm.Value.Substring(0, 7);
                        }
                        else if (ctrFilter.ParmType.ToUpper() == "Year".ToUpper())
                        {
                            targetSheet.Cells[start, 4].Value = parm.Value.Substring(0, 4);
                        }
                        else if (ctrFilter.ParmType.ToUpper() == "Date".ToUpper())
                        {
                            targetSheet.Cells[start, 4].Value = parm.Value.Substring(0, 10);
                        }
                        else
                        {
                            targetSheet.Cells[start, 4].Value = parm.Value;
                        }
                    
                        start++;
                    }
                }
                
                string path = Server.MapPath("~/ExportPivot");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                fileName = path + @"\" + Guid.NewGuid().ToString() + ".xlsb";

                targetSheet.Protect("Hqs0ft20062099", true, false, true, true, true, true, true, true, true, true, true, true, true, true, true);
                excelApplication.DisplayAlerts = false;


                excelWorkBook.SaveAs(fileName, XlFileFormat.xlExcel12, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange);
                excelWorkBook.Close(true, Type.Missing, Type.Missing);
                excelApplication.Quit();
                excelApplication = null;

                byte[] buffer = new byte[1];
                using (FileStream fs = new FileStream(fileName, FileMode.Open,
                                   FileAccess.Read, FileShare.Read))
                {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);

                }

                Stream stream = new MemoryStream(buffer);
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = name + ".xlsb" };

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
            finally
            {

                if (System.IO.File.Exists(fileName))
                    System.IO.File.Delete(fileName);

                pivotDestination = null;
                pivotData = null;
                pivotTable = null;
                targetSheet = null;
                if (excelWorkBook != null)
                    excelWorkBook = null;
                if (excelApplication != null)
                {
                    excelApplication.Quit();
                    excelApplication = null;
                }
            }
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
    }
}

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
using HQ.eSkyFramework.HQControl;
using System.Drawing;
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
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
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
                                gridPanel.HQParam.Add(new StoreParameter() { Name = "@" + colFilter.ColumnName, Value = "App.Parm_" + colFilter.ColumnName + ".getValue()", Mode = ParameterMode.Raw });
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
                Workbook workbook = new Workbook();
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = name;


                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();
                DataTable dtInvtID = dal.ExecDataTable(proc, CommandType.Text, ref pc);
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
            try
            {
                string select = "";
                string param = "";
                string cmd = "";
                string reportNbr = data["report"].PassNull();
                var parmHandler = new StoreDataHandler(data["data"]);
                var lstParm = parmHandler.ObjectData<ParmData>().ToList();

                IF30100SysEntities sys = new IF30100SysEntities(EntityConnectionStringHelper.Build(Current.Server,Current.DBSys, "IF30100SysModel"));
                var filter = sys.SYS_ReportOLAPFilter.FirstOrDefault(p => p.ReportNbr == reportNbr);
                if (filter != null && filter.FilterData.PassNull() != string.Empty)
                {
                    foreach (var item in lstParm)
                    {
                        string parmName = item.Name.Replace("Parm_","");
                        
                            
                        if (filter.FilterData.Contains("IN #"+parmName))
                        {
                            var lstTempValue = item.Value.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries).ToList();
                            string inValue = "IN (";
                            foreach (var itemValue in lstTempValue)
	                        {
                                inValue += "N'" + itemValue + "',";
	                        }
                            if(inValue.Length>3)
                            {
                                inValue = inValue.TrimEnd(',') + ")";
                            } 
                            else
                            {
                                inValue= "";
                            }
                            filter.FilterData = filter.FilterData.Replace("IN #" + parmName, inValue);
                        }
                        filter.FilterData = filter.FilterData.Replace("#" + parmName, "N'" + item.Value + "'");
                    }
                }

              
                var lstColumn = sys.SYS_ReportOLAPTemplate.Where(p => p.ReportNbr == reportNbr && p.PivotType!="P").ToList();

                var lstTemp = new List<string>();
                foreach (var col in lstColumn)
                {
                    if (!lstTemp.Any(p => p == col.ColumnName))
                    {
                        lstTemp.Add(col.ColumnName);
                        select += string.Format("N'{0}' = {1},", Util.GetLang(col.ColumnDescr), col.ColumnName);
                    }
                }

                cmd = "select " + select.TrimEnd(',') + " from " + view;
                if (filter.FilterData.PassNull().Length > 0)
                {
                    cmd += " where " + filter.FilterData;
                }
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();

                Worksheet sheetPivot = workbook.Worksheets[0];
                Worksheet sheetData = workbook.Worksheets[workbook.Worksheets.Add()];

                sheetData.Name = "Data";
                sheetData.IsVisible = false;

                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();

                DataTable dt = dal.ExecDataTable(cmd, CommandType.Text, ref pc);
                if (dt.Rows.Count == 0)
                {
                    Util.AppendLog(ref _logMessage, "20100101", "");
                    return _logMessage;
                }
                sheetData.Cells.ImportDataTable(dt, true, "A1");// du lieu Inventory
             
                sheetData.AutoFitColumns();
               
                sheetPivot.Name = name;
                //Getting the pivottables collection in the sheet
                Aspose.Cells.Pivot.PivotTableCollection pivotTables = sheetPivot.PivotTables;
                sheetPivot.Cells[0, 0].Value = name;
                var style = sheetPivot.Cells[0, 0].GetStyle();
                style.Font.Size = 25;
                style.Font.IsBold = true;
                sheetPivot.Cells[0, 0].SetStyle(style);
                

                var lstFilter = lstColumn.Where(p => p.PivotType == "F").OrderBy(p => p.PivotOrder).ToList();

                int index = pivotTables.Add("=Data!A1:" + sheetData.Cells[sheetData.Cells.MaxDataRow, sheetData.Cells.MaxDataColumn].Name, "A" + (lstFilter.Count + 5).ToString(), "PivotTable1");
              
                Aspose.Cells.Pivot.PivotTable pivotTable = pivotTables[index];
              
                pivotTable.RowGrand = true;
                
                pivotTable.ColumnGrand = true;
              
                pivotTable.IsAutoFormat = true;
              
                pivotTable.AutoFormatType = Aspose.Cells.Pivot.PivotTableAutoFormatType.None;
              
              
              
                var lstRow = lstColumn.Where(p => p.PivotType == "R").OrderBy(p => p.PivotOrder).ToList();

                foreach (var item in lstRow)
                {
                    var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
                    if (col != null)
                    {
                        pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Row,col.ColumnName);
                        var field = pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Row)[col.ColumnName];
                        if (item.DataFormat.PassNull()!=string.Empty)
                        {
                            field.NumberFormat = item.DataFormat;
                        }
                    }
                   
                }

                var lstCol= lstColumn.Where(p => p.PivotType == "C").OrderBy(p => p.PivotOrder).ToList();
                foreach (var item in lstCol)
                {
                    var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
                    if (col != null)
                    {
                        pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Column, col.ColumnName);
                        var field = pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Column)[col.ColumnName];
                        if (item.DataFormat.PassNull() != string.Empty)
                        {
                            field.NumberFormat = item.DataFormat;
                        }
                    }
                }

                var lstMeasure = lstColumn.Where(p => p.PivotType == "M").OrderBy(p => p.PivotOrder).ToList();
                foreach (var item in lstMeasure)
                {
                    var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
                    if (col != null)
                    {
                        pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Data, col.ColumnName);
                        var field = pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Data)[col.ColumnName];
                        field.Function = GetFunction(item.MeasureFunc);
                        if(item.DataFormat.PassNull()!=string.Empty){
                            field.NumberFormat = item.DataFormat;
                        }
                        
                    }
                }

             
                foreach (var item in lstFilter)
                {
                    var col = dt.Columns[Util.GetLang(item.ColumnDescr)];
                    if (col != null)
                    {
                        pivotTable.AddFieldToArea(Aspose.Cells.Pivot.PivotFieldType.Page, col.ColumnName);
                        pivotTable.Fields(Aspose.Cells.Pivot.PivotFieldType.Page)[col.ColumnName].IsMultipleItemSelectionAllowed = true;
                    }
                }
           

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
        private ConsolidationFunction GetFunction(string func)
        {
            switch (func.ToUpper())
            {
                case "SUM": return ConsolidationFunction.Sum;
                case "COUNT": return ConsolidationFunction.Count;
                case "AVG": return ConsolidationFunction.Average;
                case "MIN": return ConsolidationFunction.Min;
                case "MAX": return ConsolidationFunction.Max;
                default: return ConsolidationFunction.Sum;
            }
        }
    }
}

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
using HQ.eSkyFramework.HQControl;
using System.Text.RegularExpressions;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using HQFramework.Common;

namespace OM23102.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23102Controller : Controller
    {
        private string _screenNbr = "OM23102";
        private string _userName = Current.UserName;
        OM23102Entities _db = Util.CreateObjectContext<OM23102Entities>(false);
        private JsonResult _logMessage;
        public ActionResult Index()
        {
           
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetOM_PG_FCS(string BranchID, DateTime FCSDate)
        {
            return this.Store(_db.OM23102_pgLoadGrid(BranchID, FCSDate));
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            string BranchID = data["cboDist"];
            string FCSDate_temp = data["dateFcs"];
            DateTime FCSDate = DateTime.Parse(FCSDate_temp);
            DataTable dtUpdated = data["lstOM_PG_FCS"] == "{}" ? new DataTable() : ConvertJSONToDataTable(data["lstOM_PG_FCS"].Replace("{\"Updated\":[{", "{\"Status\":\"Updated\",").Replace("\"Created\":[{", "{\"Status\":\"Created\",").Replace("\"Deleted\":[{", "{\"Status\":\"Deleted\",").Replace("}]}", ""));
            try
            {
                var lstIN_ProductClass = _db.OM23102_getIN_ProductClass(BranchID).ToList();
                var listRecordChange = dtUpdated.Rows;
                for (int i = 0; i < listRecordChange.Count; i++)
                {
                    for (int k = 0; k < lstIN_ProductClass.Count; k++)
                    {
                        var tmpBranchID = BranchID;
                        string tmpSlsperId = listRecordChange[i]["SlsperId"].PassNull();
                        string tmpCustID = listRecordChange[i]["CustID"].PassNull();
                        string tmpClassID = lstIN_ProductClass[k].ClassID;
                        var tmpSellIn = listRecordChange[i]["SellIn_" + tmpClassID];
                        var tmpSellOut = listRecordChange[i]["SellOut_" + tmpClassID];
                        var tmpCoverage = listRecordChange[i]["Coverage_" + tmpClassID];
                        var tmpDNA = listRecordChange[i]["DNA_" + tmpClassID];
                        var tmpForcusedSKU = listRecordChange[i]["ForcusedSKU_" + tmpClassID];
                        var tmpLPPC = listRecordChange[i]["LPPC"];
                        var tmpVisit = listRecordChange[i]["Visit"];
                        var tmpVisitTime = listRecordChange[i]["VisitTime"];
                        var Status = listRecordChange[i]["Status"];

                        var record = _db.OM_PG_FCS.FirstOrDefault(p => p.BranchID == tmpBranchID && p.ClassID == tmpClassID && p.SlsperId == tmpSlsperId && p.CustID == tmpCustID && p.FCSDate.Month == FCSDate.Month && p.FCSDate.Year == FCSDate.Year);
                        if (record != null)
                        {
                            record.SellIn = Convert.ToDouble(tmpSellIn);
                            record.SellOut = Convert.ToDouble(tmpSellOut);
                            record.Coverage = Convert.ToDouble(tmpCoverage);
                            record.DNA = Convert.ToDouble(tmpDNA);
                            record.Visit = Convert.ToDouble(tmpVisit);
                            record.LPPC = Convert.ToDouble(tmpLPPC);
                            record.ForcusedSKU = Convert.ToDouble(tmpForcusedSKU);
                            record.VisitTime = Convert.ToDouble(tmpVisitTime);

                            record.LUpd_DateTime = DateTime.Now;
                            record.LUpd_Prog = _screenNbr;
                            record.LUpd_User = _userName;

                            //if (record.SellIn == 0 && record.Coverage == 0 && record.DNA == 0 && record.Visit == 0 && record.LPPC == 0 && record.ForcusedSKU == 0 && record.VisitTime == 0)
                            //{
                            //    _db.OM_PG_FCS.DeleteObject(record);
                            //}
                            if (Status.Equals("Deleted"))
                            {
                                var lstDeleted = _db.OM_PG_FCS.Where(p => p.SlsperId == tmpSlsperId && p.BranchID == BranchID && p.ClassID == tmpClassID && p.FCSDate.Year==FCSDate.Year && p.FCSDate.Month==FCSDate.Month).ToList();
                                foreach (var item in lstDeleted)
                                {
                                    _db.OM_PG_FCS.DeleteObject(item);
                                }
                            }

                        }
                        else
                        {
                            var recordNew = new OM_PG_FCS();

                            recordNew.BranchID = Convert.ToString(tmpBranchID);
                            recordNew.SlsperId = Convert.ToString(tmpSlsperId);
                            recordNew.CustID = Convert.ToString(tmpCustID);
                            recordNew.ClassID = Convert.ToString(tmpClassID);
                            recordNew.FCSDate = Convert.ToDateTime(FCSDate);

                            recordNew.SellIn = Convert.ToDouble(tmpSellIn);
                            recordNew.SellOut = Convert.ToDouble(tmpSellOut);
                            recordNew.Coverage = Convert.ToDouble(tmpCoverage);
                            recordNew.DNA = Convert.ToDouble(tmpDNA);
                            recordNew.Visit = Convert.ToDouble(tmpVisit);
                            recordNew.LPPC = Convert.ToDouble(tmpLPPC);
                            recordNew.ForcusedSKU = Convert.ToDouble(tmpForcusedSKU);
                            recordNew.VisitTime = Convert.ToDouble(tmpVisitTime);

                            recordNew.Crtd_DateTime = DateTime.Now;
                            recordNew.Crtd_Prog = _screenNbr;
                            recordNew.Crtd_User = _userName;
                            recordNew.LUpd_DateTime = DateTime.Now;
                            recordNew.LUpd_Prog = _screenNbr;
                            recordNew.LUpd_User = _userName;

                            if (recordNew.ClassID != "" && recordNew.SlsperId != "" && recordNew.CustID != "" && recordNew.BranchID != "" && recordNew.FCSDate != null)
                            {
                                _db.OM_PG_FCS.AddObject(recordNew);
                            }
                        }
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

        private DataTable ConvertJSONToDataTable(string jsonString)
        {
          
            DataTable dt = new DataTable();
            //strip out bad characters
            string[] jsonParts = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");

            //hold column names
            List<string> dtColumns = new List<string>();

            //get columns
            foreach (string jp in jsonParts)
            {
                //only loop thru once to get column names
                string[] propData = Regex.Split(jp.Replace("{", "").Replace("}", ""), ",");
                foreach (string rowData in propData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string n = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string v = rowData.Substring(idx + 1);
                        if (!dtColumns.Contains(n))
                        {
                            dtColumns.Add(n);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", rowData));
                    }

                }
                break; // TODO: might not be correct. Was : Exit For
            }
            //build dt
            foreach (string c in dtColumns)
            {
                dt.Columns.Add(c);
            }
            //get table data
            foreach (string jp in jsonParts)
            {
                string[] propData = Regex.Split(jp.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in propData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string n = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string v = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[n] = v;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }

        [DirectMethod]
        public ActionResult OM23102_LoadParm(string BranchID)
        {
            Ext.Net.Column clm;
            NumberColumn nbc;
            var grid = this.GetCmp<HQGridPanel>("grdOM_PG_FCS");
            DataSource ds = new DataSource();
            var lstIN_ProductClass = _db.OM23102_getIN_ProductClass(BranchID).ToList();
            grid.HQisPaging = true;
            grid.HQPageSize = 50;
            //Column Sell In
            clm = new Ext.Net.Column
            {
                Text = Util.GetLang("OM23102_SellIn"),
                ID = "txt_SellIn"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "SellIn_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Format="0,000",
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=0,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column SellOut
            clm = new Ext.Net.Column
            {
                Text = Util.GetLang("OM23102_SellOut"),
                ID = "txt_SellOut"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "SellOut_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Format = "0,000",
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=0,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column Coverage
            clm = new Ext.Net.Column
            {
                Text = Util.GetLang("OM23102_Coverage"),
                ID = "txt_Coverage"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "Coverage_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Format = "0,000",
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=0,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column DNA
            clm = new Ext.Net.Column
            {
                Text = Util.GetLang("OM23102_DNA"),
                ID = "txt_DNA"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "DNA_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Format = "0,000",
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=0,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column ForcusedSKU
            clm = new Ext.Net.Column
            {
                Text = Util.GetLang("OM23102_ForcusedSKU"),
                ID = "txt_ForcusedSKU"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "ForcusedSKU_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Format = "0,000",
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=0,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column Visit
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23102_Visit"),
                ID = "txt_Visit",
                Align = Alignment.Right,
                DataIndex = "Visit",
                Format = "0,000",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=0,
                        MinValue=0
                    }
                }
            };
            grid.AddColumn(nbc);

            //Column LPPC
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23102_LPPC"),
                ID = "txt_LPPC",
                Align = Alignment.Right,
                DataIndex = "LPPC",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=2,
                        MinValue=0,
                        MaxValue=100
                    }
                }
            };
            grid.AddColumn(nbc);

            //Column VisitTime
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23102_VisitTime"),
                ID = "txt_VisitTime",
                Hidden = true,
                Width = 120,
                Align = Alignment.Right,
                DataIndex = "VisitTime",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=0,
                        MinValue=0
                    }
                }
            };
            grid.AddColumn(nbc);

            //Column Reject
            CommandColumn commentclm = new CommandColumn
            {
                Width = 100,
                ID = "CommandReject",
                Commands =
                {
                    new GridCommand
                    {
                        Text = Util.GetLang("Reject"), 
                        ToolTip = 
                        {
                            Text = Util.GetLang("Rejectrowchanges"), 
                        },
                        CommandName = "reject",
                        Icon = Ext.Net.Icon.ArrowUndo
                    }
                },
                PrepareToolbar =
                {
                    Handler = "toolbar.items.get(0).setVisible(record.dirty);"
                },
                Listeners =
                {
                    Command =
                    {
                        Handler = "grdOM_PG_FCS_Reject(record);"
                    }
                }
            };
            grid.AddColumn(commentclm);
            return this.Direct();
        }
        #region import,export, report
        [HttpPost]
        public ActionResult Export(FormCollection data, string type)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                workbook.Worksheets.Add();
                Worksheet SheetTarget = workbook.Worksheets[0];
                Worksheet SheetDataMaster = workbook.Worksheets[1];
                CellArea area;
                Validation validation = SheetTarget.Validations[SheetTarget.Validations.Add()];

                SheetTarget.Name = Util.GetLang("Target");
                SheetDataMaster.Name = "Master";

                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection(); 
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                DataTable dtBranch = dal.ExecDataTable("OM23102_peBranch", CommandType.StoredProcedure, ref pc);
                SheetDataMaster.Cells.ImportDataTable(dtBranch, true, 0, 0, false);// du lieu Inventory

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                DataTable dtSale = dal.ExecDataTable("OM23102_peSale", CommandType.StoredProcedure, ref pc);
                SheetDataMaster.Cells.ImportDataTable(dtSale, true, 0, 2, false);// du lieu Inventory

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                DataTable dtClass = dal.ExecDataTable("OM23102_peProductClass", CommandType.StoredProcedure, ref pc);
                SheetDataMaster.Cells.ImportDataTable(dtClass, true, 0, 6, false);// du lieu Inventory


                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                DataTable dtCust = dal.ExecDataTable("OM23102_peCust", CommandType.StoredProcedure, ref pc);
                SheetDataMaster.Cells.ImportDataTable(dtCust, true, 0, 7, false);// du lieu Inventory

                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                Range range;
                Cell cell;
                //LOCK TRUE

                int commentIndex = SheetTarget.Comments.Add("E4");
                //Accessing the newly added comment
                Comment comment = SheetTarget.Comments[commentIndex];
                //Setting the comment note
                comment.Note = "Input Month(MM/yyyy)";
                comment.WidthCM = 5;

                #region template

                SetCellValueHeader(SheetTarget.Cells["D1"], Util.GetLang("OM23102EHeader"), TextAlignmentType.Center, TextAlignmentType.Center);
                SheetTarget.Cells.Merge(0, 1, 2, 4);
                
                SetCellValueHeader(SheetTarget.Cells["A4"], Util.GetLang("OM23102ESTT"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["B4"], Util.GetLang("OM23102BranchID"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["C4"], Util.GetLang("OM23102SlsPerID"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["D4"], Util.GetLang("OM23102SlsName"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["E4"], Util.GetLang("OM23102Month"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["F4"], Util.GetLang("OM23102Class"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["G4"], Util.GetLang("OM23102SellOut"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["H4"], Util.GetLang("OM23102CustID"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetTarget.Cells["I4"], Util.GetLang("OM23102CustName"), TextAlignmentType.Center, TextAlignmentType.Center);
                
                #endregion

                #region formular


                //BranchID
                string formulaBranch = "=Master! $A$2:$A$" + (dtBranch.Rows.Count + 2);
                validation = SheetTarget.Validations[SheetTarget.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaBranch;
                validation.InputTitle = "";
                validation.InputMessage = "Please choose BranchID";
                validation.ErrorMessage = "BranchID does not exist";

                area = new CellArea();
                area.StartRow = 4;
                area.EndRow =1000;
                area.StartColumn = 1;
                area.EndColumn = 1;
                validation.AddArea(area);

                //SlsperID
                string formulaSlsperName = "=OFFSET(Master! $C$1,IFERROR(MATCH(B5,Master! $C$1:$C$" + (dtSale.Rows.Count + 1) + ",0)-1," + (dtSale.Rows.Count + 1) + "),2,IF(COUNTIF(Master! $C$1:$C$" + (dtSale.Rows.Count + 1) + ",B5)=0,1,COUNTIF(Master! $C$1:$C$" + (dtSale.Rows.Count + 1) + ",B5)),1)";
                validation = SheetTarget.Validations[SheetTarget.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaSlsperName;
                validation.InputTitle = "";
                validation.InputMessage = "Please choose SlsperName";
                validation.ErrorMessage = "SlsperName does not exist";

                area = new CellArea();
                area.StartRow = 4;
                area.EndRow = 1000;
                area.StartColumn = 2;
                area.EndColumn = 2;
                validation.AddArea(area);


                //SlsName
                string formulaSlsName = "=IFERROR(VLOOKUP(B5&C5,Master! $D$1:$F$" + (dtSale.Rows.Count + 2) + ",3,FALSE),\"\")";
                SheetTarget.Cells["D5"].SetSharedFormula(formulaSlsName, 1000, 1);

                //CustID
                string formulaCust = "=OFFSET(Master! $H$1,IFERROR(MATCH(B5&C5,Master! $H$1:$H$" + (dtCust.Rows.Count + 1) + ",0)-1," + (dtCust.Rows.Count + 1) + "),2,IF(COUNTIF(Master! $H$1:$H$" + (dtCust.Rows.Count + 1) + ",B5&C5)=0,1,COUNTIF(Master! $H$1:$H$" + (dtCust.Rows.Count + 1) + ",B5&C5)),1)";
                validation = SheetTarget.Validations[SheetTarget.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaCust;
                validation.InputTitle = "";
                validation.InputMessage = "Please choose CustID";
                validation.ErrorMessage = "CustID does not exist";

                area = new CellArea();
                area.StartRow = 4;
                area.EndRow = 1000;
                area.StartColumn = 7;
                area.EndColumn = 7;
                validation.AddArea(area);

                //SlsName
                string formulaCustName = "=IFERROR(VLOOKUP(B5&C5&H5,Master! $I$1:$K$" + (dtCust.Rows.Count + 2) + ",3,FALSE),\"\")";
                SheetTarget.Cells["I5"].SetSharedFormula(formulaCustName, 1000, 1);

                // Date
                validation = SheetTarget.Validations[SheetTarget.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.Date;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.None;
                validation.InputTitle = "Input Month";
                validation.InputMessage = "Format (MM/yyyy)";
                validation.ErrorMessage = "Error Format (MM/yyyy)";


                area = new CellArea();
                area.StartRow =4;
                area.EndRow =1000;// dtCustomer.Rows.Count + _headerRowIdx + _headerRowCount;
                area.StartColumn = 4;
                area.EndColumn = 4;
                validation.AddArea(area);

                //Class
                string formulaClass = "=Master! $G$2:$G$" + (dtClass.Rows.Count + 2);
                validation = SheetTarget.Validations[SheetTarget.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaClass;
                validation.InputTitle = "";
                validation.InputMessage = "Please choose Class";
                validation.ErrorMessage = "Class does not exist";

                area = new CellArea();
                area.StartRow = 4;
                area.EndRow = 1000;
                area.StartColumn = 5;
                area.EndColumn = 5;
                validation.AddArea(area);

                String formulaSTT = "=IF(ISERROR(IF(B5<>\"\",A4+1 ,\"\")),1,IF(B5<>\"\",A4+1,\"\"))";
                SheetTarget.Cells["A5"].SetSharedFormula(formulaSTT, 1000, 1);


                #endregion



                #region Fomat cell


                cell = SheetTarget.Cells["G5"];
                style = cell.GetStyle();
                style.Custom = "#,##0";
                cell.SetStyle(style);


                var range1 = SheetTarget.Cells.CreateRange("A5", "A1000");
                cell = SheetTarget.Cells["A5"];
                range1.SetStyle(style);

                var range2 = SheetTarget.Cells.CreateRange("G5", "G1000");
                cell = SheetTarget.Cells["G5"];
                range2.SetStyle(style);
              

                cell = SheetTarget.Cells["E5"];
                style = cell.GetStyle();
                style.Custom = "MM/yyyy";
               
                var range4 = SheetTarget.Cells.CreateRange("E5", "E1000");
                range4.SetStyle(style);



                SheetTarget.AutoFilter.Range = "A4:H4";
                style = SheetTarget.Cells["A4"].GetStyle();
                //style.HorizontalAlignment = TextAlignmentType.Right;             
                style.IsLocked = false;
                range = SheetTarget.Cells.CreateRange("A4:H4");
                range.SetStyle(style);

                #endregion

                SheetTarget.AutoFitColumns();


                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM23102NameEx")+".xlsx" };

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
        public ActionResult Import(FormCollection data)
        {
            try
            {               
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile; 
                FileInfo fileInfo = new FileInfo(file.FileName);
                var lsterrDuplicate = new List<string>();
                var lsterrBranch =new List<string>();
                var lsterrSls =new List<string>();
                var lsterrCust = new List<string>();
                var lsterrClass =new List<string>();
                var lsterrEmpty = new List<string>();
                var lsterrStruct = new List<string>();
                string message = "";
                List<OM_PG_FCS> lstImport = new List<OM_PG_FCS>();
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];
                            string stt,branchid,slsperid,classid,custid = string.Empty;
                            double sellin, sellout = 0;
                            DateTime month;                           
                            int lineRef = 1;
                            var lstBranch=_db.OM23102_peBranch(Current.UserName).ToList();
                            var lstSlsper=_db.OM23102_peSale(Current.UserName).ToList();
                            var lstCust = _db.OM23102_peCust(Current.UserName).ToList();
                            var lstClass=_db.OM23102_peProductClass(Current.UserName).ToList();
                            for (int i = 4; i < workSheet.Cells.MaxDataRow; i++)
                            {
                                stt = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper().Trim();

                                if (stt == "") break;

                                try
                                {
                                    branchid = workSheet.Cells[i, 1].StringValue.PassNull().ToUpper().Trim();
                                    slsperid = workSheet.Cells[i, 2].StringValue.PassNull().ToUpper().Trim();
                                    month = workSheet.Cells[i, 4].DateTimeValue.AddMonths(1).AddDays(-1);
                                    classid = workSheet.Cells[i, 5].StringValue.PassNull().ToUpper().Trim();
                                    custid = workSheet.Cells[i, 7].StringValue.PassNull().ToUpper().Trim();
                                    sellout = workSheet.Cells[i, 6].DoubleValue;
                                    sellin = 0;
                                }
                                catch
                                {
                                    lsterrStruct.Add((i + 1) + "");
                                    continue;
                                }
                                if (branchid == "" || slsperid == "" || classid == "" || custid == "")
                                {
                                    lsterrEmpty.Add((i + 1) + "");
                                    continue;
                                }
                                if (lstBranch.Where(p => p.CpnyID.PassNull().ToUpper().Trim() == branchid).Count() == 0)
                                {
                                    if (!lsterrBranch.Contains(branchid))
                                    {
                                        lsterrBranch.Add(branchid);
                                    }
                                    //continue;
                                }
                                if (lstSlsper.Where(p => p.BranchID.PassNull().ToUpper().Trim() == branchid && p.SlsperId.PassNull().ToUpper().Trim() == slsperid).Count() == 0)
                                {
                                    if (!lsterrSls.Contains(slsperid))
                                    {
                                        lsterrSls.Add(slsperid);
                                    }
                                    //continue;
                                }
                                if (lstCust.Where(p => p.BranchIDSlsperIdCustID.PassNull().ToUpper().Trim() == branchid+slsperid+custid).Count() == 0)
                                {
                                    if (!lsterrCust.Contains(custid))
                                    {
                                        lsterrCust.Add(custid);
                                    }
                                    //continue;
                                }
                                if (lstClass.Where(p => p.PassNull().ToUpper().Trim() == classid).Count() == 0)
                                {
                                    if (!lsterrClass.Contains(classid))
                                    {
                                        lsterrClass.Add(classid);
                                    }
                                    //continue;
                                }

                                if (lsterrBranch.Count != 0 || lsterrSls.Count != 0 || lsterrClass.Count != 0 || lsterrCust.Count != 0) continue;
                                if (lstImport.Where(p => p.BranchID == branchid && p.SlsperId == slsperid && p.CustID == custid && p.ClassID == classid && p.FCSDate.Month == month.Month && p.FCSDate.Year == month.Year).Count() != 0) 
                                {
                                    lsterrDuplicate.Add("(" + branchid + "," + slsperid + "," + custid + "," + classid + "," + month.Month + "/" + month.Year + ")");
                                    continue;
                                }
                                var objFCS = _db.OM_PG_FCS.Where(p => p.BranchID == branchid && p.SlsperId == slsperid && p.CustID == custid && p.ClassID == classid && p.FCSDate.Month == month.Month && p.FCSDate.Year == month.Year).FirstOrDefault();
                                var objFCS1 = _db.OM_PG_FCS.Where(p => p.BranchID == branchid && p.SlsperId == slsperid && p.CustID == custid && p.FCSDate.Month == month.Month && p.FCSDate.Year == month.Year).FirstOrDefault();
                                  
                                if (objFCS == null)
                                {
                                    objFCS = new OM_PG_FCS();
                                    objFCS.ResetET();
                                    objFCS.BranchID = branchid;
                                    objFCS.ClassID = classid;
                                    objFCS.CustID = custid;
                                    objFCS.SlsperId = slsperid;
                                    objFCS.FCSDate = month;

                                    objFCS.Coverage = 0;
                                    objFCS.Crtd_DateTime = DateTime.Now;
                                    objFCS.Crtd_Prog = _screenNbr;
                                    objFCS.Crtd_User = _userName;
                                    objFCS.DNA = 0;
                                    objFCS.ForcusedSKU = 0;
                                    objFCS.LPPC = objFCS1 == null ? 0 : objFCS1.LPPC;
                                    objFCS.LUpd_DateTime = DateTime.Now;
                                    objFCS.LUpd_Prog = _screenNbr;
                                    objFCS.LUpd_User = _userName;
                                    objFCS.SellIn = sellin;
                                    objFCS.SellOut = sellout;
                                    objFCS.Visit = objFCS1 == null ? 0 : objFCS1.Visit;
                                    objFCS.VisitTime = objFCS1 == null ? 0 : objFCS1.VisitTime;

                                    _db.OM_PG_FCS.AddObject(objFCS);

                                }
                                else
                                {
                                    objFCS.LUpd_DateTime = DateTime.Now;
                                    objFCS.LUpd_Prog = _screenNbr;
                                    objFCS.LUpd_User = _userName;
                                    //objFCS.SellIn = sellin;
                                    objFCS.SellOut = sellout;
                                }
                                lstImport.Add(objFCS);

                            }
                        }

                        if (lsterrCust.Count == 0 && lsterrBranch.Count == 0 && lsterrClass.Count == 0 && lsterrDuplicate.Count == 0 && lsterrSls.Count == 0 && lsterrEmpty.Count == 0 && lsterrStruct.Count == 0)
                        {
                            _db.SaveChanges();                          
                        }
                        else
                        {
                           
                            if (lsterrStruct.Count > 0)
                            {                                
                                message += string.Format(Util.GetLang("ErrFmatPre")+"<br/>",
                                    lsterrStruct.Count > 5 ? string.Join(", ", lsterrStruct.Take(5)) + ", ..." : string.Join(", ", lsterrStruct));
                            }
                            if (lsterrEmpty.Count > 0)
                            {
                                message += string.Format(Util.GetLang("OM23102ErrEtyPre") + "<br/>", lsterrEmpty.Count > 5 ? string.Join(", ", lsterrEmpty.Take(5)) + ", ..." : string.Join(", ", lsterrEmpty));
                            }
                            if (lsterrBranch.Count > 0)
                            {
                                message += string.Format(Util.GetLang("ErrNExistBranch") + "<br/>",
                                    lsterrBranch.Count > 5 ? string.Join(", ", lsterrBranch.Take(5)) + ", ..." : string.Join(", ", lsterrBranch));
                            }
                            if (lsterrSls.Count > 0)
                            {
                                message += string.Format(Util.GetLang("ErrNExistSales") + "<br/>",
                                    lsterrSls.Count > 5 ? string.Join(", ", lsterrSls.Take(5)) + ", ..." : string.Join(", ", lsterrSls));
                            }
                            if (lsterrCust.Count > 0)
                            {
                                message += string.Format(Util.GetLang("ErrNExistCust") + "<br/>",
                                    lsterrCust.Count > 5 ? string.Join(", ", lsterrCust.Take(5)) + ", ..." : string.Join(", ", lsterrCust));
                            }
                            if (lsterrClass.Count > 0)
                            {
                                message += string.Format(Util.GetLang("ErrNExistClass") + "<br/>",
                                    lsterrClass.Count > 5 ? string.Join(", ", lsterrClass.Take(5)) + ", ..." : string.Join(", ", lsterrClass));
                            }
                            if (lsterrDuplicate.Count > 0)
                            {
                                message += string.Format(Util.GetLang("ErrDuplicate") + "<br/>",
                                    lsterrDuplicate.Count > 5 ? string.Join(", ", lsterrDuplicate.Take(5)) + ", ..." : string.Join(", ", lsterrDuplicate));
                            }
                            return Json(new { success = true, type = "error", message = message });
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "")});                        
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
   
        #endregion
        private void SetCellValueHeader(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = true;
            style.Font.Size = 10;
            style.Font.Color = Color.Blue;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }

    }
}

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

namespace OM23100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23100Controller : Controller
    {
        private string _screenNbr = "OM23100";
        private string _userName = Current.UserName;
        OM23100Entities _db = Util.CreateObjectContext<OM23100Entities>(false);
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

        public ActionResult GetOM_FCS(string BranchID, DateTime FCSDate)
        {
            return this.Store(_db.OM23100_pgLoadGrid(BranchID, FCSDate));
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            string BranchID = data["cboDist"];
            string FCSDate_temp = data["dateFcs"];
            DateTime FCSDate = DateTime.Parse(FCSDate_temp);
            DataTable dtUpdated = data["lstOM_FCS"] == "{}" ? new DataTable() : ConvertJSONToDataTable(data["lstOM_FCS"].Replace("{\"Updated\":[{", "{\"Status\":\"Updated\",").Replace("\"Created\":[{", "{\"Status\":\"Created\",").Replace("\"Deleted\":[{", "{\"Status\":\"Deleted\",").Replace("}]}", ""));
            try
            {
                var lstIN_ProductClass = _db.OM23100_getIN_ProductClass(BranchID).ToList();
                var listRecordChange = dtUpdated.Rows;
                for (int i = 0; i < listRecordChange.Count; i++)
                {
                    for (int k = 0; k < lstIN_ProductClass.Count; k++)
                    {
                        var tmpBranchID = BranchID;
                        var tmpSlsperId = listRecordChange[i]["SlsperId"];
                        var tmpClassID = lstIN_ProductClass[k].ClassID;
                        var tmpSellIn = listRecordChange[i]["SellIn_" + lstIN_ProductClass[k].ClassID];
                        var tmpCoverage = listRecordChange[i]["Coverage_" + lstIN_ProductClass[k].ClassID];
                        var tmpDNA = listRecordChange[i]["DNA_" + lstIN_ProductClass[k].ClassID];
                        var tmpForcusedSKU = listRecordChange[i]["ForcusedSKU_" + lstIN_ProductClass[k].ClassID];
                        var tmpLPPC = listRecordChange[i]["LPPC"];
                        var tmpVisit = listRecordChange[i]["Visit"];
                        var tmpVisitTime = listRecordChange[i]["VisitTime"];
                        var Status = listRecordChange[i]["Status"];

                        var record = _db.OM_FCS.FirstOrDefault(p => p.BranchID == tmpBranchID && p.ClassID == tmpClassID && p.SlsperId == tmpSlsperId && p.FCSDate.Month == FCSDate.Month && p.FCSDate.Year == FCSDate.Year);
                        if (record != null)
                        {
                            record.SellIn = Convert.ToDouble(tmpSellIn);
                            record.Coverage = Convert.ToDouble(tmpCoverage);
                            record.DNA = Convert.ToDouble(tmpDNA);
                            record.Visit = Convert.ToDouble(tmpVisit);
                            record.LPPC = Convert.ToDouble(tmpLPPC);
                            record.ForcusedSKU = Convert.ToDouble(tmpForcusedSKU);
                            record.VisitTime = Convert.ToDouble(tmpVisitTime);

                            record.LUpd_DateTime = DateTime.Now;
                            record.LUpd_Prog = _screenNbr;
                            record.LUpd_User = _userName;

                            if (record.SellIn == 0 && record.Coverage == 0 && record.DNA == 0 && record.Visit == 0 && record.LPPC == 0 && record.ForcusedSKU == 0 && record.VisitTime == 0)
                            {
                                _db.OM_FCS.DeleteObject(record);
                            }
                            if (Status.Equals("Deleted"))
                            {
                                var lstDeleted = _db.OM_FCS.Where(p => p.SlsperId == tmpSlsperId && p.BranchID == BranchID && p.ClassID == tmpClassID && p.FCSDate.Year==FCSDate.Year && p.FCSDate.Month==FCSDate.Month).ToList();
                                foreach (var item in lstDeleted)
                                {
                                    _db.OM_FCS.DeleteObject(item);
                                }
                            }

                        }
                        else
                        {
                            var recordNew = new OM_FCS();

                            recordNew.BranchID = Convert.ToString(tmpBranchID);
                            recordNew.SlsperId = Convert.ToString(tmpSlsperId);
                            recordNew.ClassID = Convert.ToString(tmpClassID);
                            recordNew.FCSDate = Convert.ToDateTime(FCSDate);

                            recordNew.SellIn = Convert.ToDouble(tmpSellIn);
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

                            if (recordNew.ClassID != "" && recordNew.SlsperId != "" && recordNew.BranchID != "" && recordNew.FCSDate != null)
                            {
                                _db.OM_FCS.AddObject(recordNew);
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
        public ActionResult LoadRPTParm(string BranchID)
        {
            Column clm;
            NumberColumn nbc;
            var grid = this.GetCmp<GridPanel>("grdOM_FCS");
            DataSource ds = new DataSource();
            var lstIN_ProductClass = _db.OM23100_getIN_ProductClass(BranchID).ToList();

            //Column Sell In
            clm = new Column
            {
                Text = Util.GetLang("OM23100_SellIn"),
                ID = "txt_SellIn"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "SellIn_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column Coverage
            clm = new Column
            {
                Text = Util.GetLang("OM23100_Coverage"),
                ID = "txt_Coverage"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "Coverage_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column DNA
            clm = new Column
            {
                Text = Util.GetLang("OM23100_DNA"),
                ID = "txt_DNA"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "DNA_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
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
                Text = Util.GetLang("OM23100_Visit"),
                ID = "txt_Visit",
                Align = Alignment.Right,
                DataIndex = "Visit",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=2,
                        MinValue=0
                    }
                }
            };
            grid.AddColumn(nbc);

            //Column LPPC
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23100_LPPC") + "(%)",
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

            //Column ForcusedSKU
            clm = new Column
            {
                Text = Util.GetLang("OM23100_ForcusedSKU"),
                ID = "txt_ForcusedSKU"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "ForcusedSKU_" + lstIN_ProductClass[i].ClassID,
                    Align = Alignment.Right,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column VisitTime
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23100_VisitTime"),
                ID = "txt_VisitTime",
                Width = 120,
                Align = Alignment.Right,
                DataIndex = "VisitTime",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=2,
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
                        Handler = "grd_Reject(record);"
                    }
                }
            };
            grid.AddColumn(commentclm);
            return this.Direct();
        }
    }
}

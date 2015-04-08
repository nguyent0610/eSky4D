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

namespace OM23101.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23101Controller : Controller
    {
        private string _screenNbr = "OM23101";
        private string _userName = Current.UserName;
        OM23101Entities _db = Util.CreateObjectContext<OM23101Entities>(false);

        public ActionResult Index()
        {
            Column clm;
            NumberColumn nbc;
            var grid = this.GetCmp<GridPanel>("grdOM_FCSBranch");
            DataSource ds = new DataSource();
            var lstIN_ProductClass = _db.OM23101_getIN_ProductClass().ToList();

            //Column Sell In
            clm = new Column
            {
                Text = Util.GetLang("OM23101_SellInKPI"),
                ID = "txt_SellInKPI"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    DataIndex = "SellInKPI_" + lstIN_ProductClass[i].ClassID,
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

            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetOM_FCSBranch(string State, DateTime FCSDate)
        {
            return this.Store(_db.OM23101_pgLoadGrid(State, FCSDate));
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
      
            string FCSDate_temp = data["dateFcs"];
            DateTime FCSDate = DateTime.Parse(FCSDate_temp);
            DataTable dtUpdated = data["lstOM_FCSBranch"] == "{}" ? new DataTable() : ConvertJSONToDataTable(data["lstOM_FCSBranch"].Replace("{\"Updated\":[{", "{\"Status\":\"Updated\",").Replace("\"Created\":[{", "{\"Status\":\"Created\",").Replace("\"Deleted\":[{", "{\"Status\":\"Deleted\",").Replace("}]}", ""));
            try
            {
                var lstIN_ProductClass = _db.OM23101_getIN_ProductClass().ToList();
                var listRecordChange = dtUpdated.Rows;
                for (int i = 0; i < listRecordChange.Count; i++)
                {
                    for (int k = 0; k < lstIN_ProductClass.Count; k++)
                    {
                        var tmpBranchID = listRecordChange[i]["BranchID"];
                        var tmpClassID = lstIN_ProductClass[k].ClassID;
                        var tmpSellInKPI = listRecordChange[i]["SellInKPI_" + lstIN_ProductClass[k].ClassID];
                        var Status = listRecordChange[i]["Status"];

                        var record = _db.OM_FCSBranch.FirstOrDefault(p => p.BranchID == tmpBranchID && p.ClassID == tmpClassID && p.FCSDate.Month == FCSDate.Month && p.FCSDate.Year == FCSDate.Year);
                        if (record != null)
                        {
                            record.SellInKPI = Convert.ToDouble(tmpSellInKPI);
 
                            record.LUpd_DateTime = DateTime.Now;
                            record.LUpd_Prog = _screenNbr;
                            record.LUpd_User = _userName;

                            if (Status.Equals("Deleted"))
                            {
                                var lstDeleted = _db.OM_FCSBranch.Where(p => p.BranchID == tmpBranchID && p.ClassID == tmpClassID && p.FCSDate.Year == FCSDate.Year && p.FCSDate.Month == FCSDate.Month).ToList();
                                foreach (var item in lstDeleted)
                                {
                                    _db.OM_FCSBranch.DeleteObject(item);
                                }
                            }
                        }
                        else
                        {
                            var recordNew = new OM_FCSBranch();

                            recordNew.BranchID = Convert.ToString(tmpBranchID);
                            recordNew.ClassID = Convert.ToString(tmpClassID);
                            recordNew.FCSDate = Convert.ToDateTime(FCSDate);
                            recordNew.SellInKPI = Convert.ToDouble(tmpSellInKPI);

                            recordNew.Crtd_DateTime = DateTime.Now;
                            recordNew.Crtd_Prog = _screenNbr;
                            recordNew.Crtd_User = _userName;
                            recordNew.LUpd_DateTime = DateTime.Now;
                            recordNew.LUpd_Prog = _screenNbr;
                            recordNew.LUpd_User = _userName;

                            if (recordNew.ClassID != "" && recordNew.BranchID != "" && recordNew.FCSDate != null)
                            {
                                _db.OM_FCSBranch.AddObject(recordNew);
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

    }
}

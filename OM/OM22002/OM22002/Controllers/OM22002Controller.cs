using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
//using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
using HQFramework.DAL;
using System.Data;
using HQFramework.Common;
using System.Text.RegularExpressions;
using HQ.eSkyFramework.HQControl;

namespace OM22002.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22002Controller : Controller
    {
        private string _screenNbr = "OM22002";

        private string _displayTradeType = "D";
        private string _bonusTradeType = "B";

        OM22002Entities _db = Util.CreateObjectContext<OM22002Entities>(false);
        //
        // GET: /OM22002/
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

        // Grid Display
        public ActionResult GetDet(string cpnyID, string objectID, string tradeType)
        {
            var dets = _db.OM22002_pgCust(cpnyID, objectID, tradeType).ToList();
            return this.Store(dets);
        }

        // Grid Bonus
        public ActionResult GetGridPanel(string containerId, string cpnyID, string objectID, string tradeType)
        {
            this.BuildGridPanel(cpnyID, objectID, tradeType).AddTo(containerId);

            return this.Direct();
        }

        // Save Display
        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var custHandler = new StoreDataHandler(data["lstCustChange"]);
                var lstCustChange = custHandler.BatchObjectData<OM22002_pgCust_Result>();

                foreach (var updated in lstCustChange.Updated)
                {
                    if (!string.IsNullOrWhiteSpace(updated.ObjectID))
                    {
                        if (updated.TradeType == _displayTradeType)
                        {
                            #region DisplayTradeType
                            var regisObj = _db.OM_TDisplayCustomer.FirstOrDefault(p => p.BranchID == updated.BranchID
                                && p.SlsperID == updated.SlsperID && p.CustID == updated.CustID
                                && p.DisplayID == updated.ObjectID);// && p.LevelID != updated.LevelID);
                            if (regisObj != null)
                            {
                                throw new MessageException("1003", "");
                            }
                            else
                            {
                                //regisObj = _db.OM_TDisplayCustomer.FirstOrDefault(p => p.BranchID == updated.BranchID
                                //     && p.SlsperID == updated.SlsperID && p.CustID == updated.CustID
                                //    && p.ObjectID == updated.ObjectID && p.LevelID == updated.LevelID);
                                //if (regisObj == null)
                                //{
                                regisObj = new OM_TDisplayCustomer()
                                {
                                    BranchID = updated.BranchID,
                                    CustID = updated.CustID,
                                    DisplayID = updated.ObjectID,
                                    LevelID = updated.LevelID,
                                    Rate = (double)updated.Rate,
                                    SlsperID = updated.SlsperID,
                                    Crtd_DateTime = DateTime.Now,
                                    Crtd_Prog = _screenNbr,
                                    Crtd_User = Current.UserName,

                                };
                                //}
                                regisObj.LUpd_DateTime = DateTime.Now;
                                regisObj.LUpd_Prog = _screenNbr;
                                regisObj.LUpd_User = Current.UserName;
                                regisObj.Rate = (double)updated.Rate;
                                regisObj.Territory = updated.Territory.PassNull();
                                regisObj.Zone = updated.Zone;
                                regisObj.Status = "H";
                                regisObj.PercentImage = 0;
                                regisObj.PercentSales = 0;
                                _db.OM_TDisplayCustomer.AddObject(regisObj);
                            }
                            #endregion
                        }
                        else if(updated.TradeType == _bonusTradeType){
                            #region BonusTradeType
                            //var regisObj = _db.OM_TBonusCustomer.FirstOrDefault(p => p.BranchID == updated.BranchID
                            //    && p.SlsperID == updated.SlsperID && p.CustID == updated.CustID
                            //    && p.BonusID == updated.ObjectID);// && p.LevelID != updated.LevelID);
                            //if (regisObj != null)
                            //{
                            //    throw new MessageException("1003", "");
                            //}
                            //else
                            //{
                            //    if (regisObj == null)
                            //    {
                            //        regisObj = new OM_TBonusCustomer()
                            //        {
                            //            BranchID = updated.BranchID,
                            //            CustID = updated.CustID,
                            //            BonusID = updated.ObjectID,
                            //            LevelID = updated.LevelID,
                            //            //Rate = (double)updated.Rate,
                            //            SlsperID = updated.SlsperID,
                            //            Crtd_DateTime = DateTime.Now,
                            //            Crtd_Prog = _screenNbr,
                            //            Crtd_User = Current.UserName,

                            //        };
                            //        _db.OM_TBonusCustomer.AddObject(regisObj);

                            //    }
                            //    regisObj.LUpd_DateTime = DateTime.Now;
                            //    regisObj.LUpd_Prog = _screenNbr;
                            //    regisObj.LUpd_User = Current.UserName;
                            //    //regisObj.Rate = (double)updated.Rate;
                            //    regisObj.Territory = updated.Territory.PassNull();
                            //    regisObj.Zone = updated.Zone;
                            //    //regisObj.Status = "H";
                            //    //regisObj.PercentImage = 0;
                            //    //regisObj.PercentSales = 0;

                            //    _db.SaveChanges();
                            //}
                            #endregion
                        }
                    }
                    else
                    {
                        throw new MessageException("15", "", new string[] { 
                            Util.GetLang("DisplayID")
                        });
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true, msgCode = 201405071 });
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

        // Save Bonus
        public ActionResult SaveDataBonus(FormCollection data)
        {
            try
            {
                DataTable dtUpdated = new DataTable();
                if (data["lstCustChange"] != "{}")
                {
                    dtUpdated = ConvertJSONToDataTable(data["lstCustChange"].Replace("{\"Updated\":[{", "{\"Action\":\"Updated\",").Replace("}]}", ""));
                    var lstProductClass = _db.OM22002_pcProductClass(Current.UserName, Current.LangID).ToList();
                    for (int i = 0; i < dtUpdated.Rows.Count; i++)
                    {
                        var branchID = dtUpdated.Rows[i]["BranchID"].ToString();
                        var slsperID = dtUpdated.Rows[i]["SlsperID"].ToString();
                        var custID = dtUpdated.Rows[i]["CustID"].ToString();
                        var bonusID = dtUpdated.Rows[i]["BonusID"].ToString();
                        var levelID = dtUpdated.Rows[i]["LevelID"].ToString();
                        var selected = dtUpdated.Rows[i]["Selected"].ToBool();
                        var registered = dtUpdated.Rows[i]["Registered"].ToBool();

                        if (!registered || (selected && !string.IsNullOrWhiteSpace(levelID)))
                        {
                            for (int j = 0; j < lstProductClass.Count; j++)
                            {
                                #region BonusTradeType
                                var classID = lstProductClass[j].Code;
                                var slsAmt = dtUpdated.Rows[i]["PC_" + classID].ToDouble();

                                var regisObj = _db.OM_TBonusCustomer.FirstOrDefault(p => p.BranchID == branchID
                                    && p.SlsperID == slsperID && p.CustID == custID
                                    && p.BonusID == bonusID && p.ClassID == classID);
                                if (regisObj != null)
                                {
                                    if (slsAmt > 0)
                                    {
                                        regisObj.LevelID = levelID;
                                        regisObj.LUpd_DateTime = DateTime.Now;
                                        regisObj.LUpd_Prog = _screenNbr;
                                        regisObj.LUpd_User = Current.UserName;
                                        regisObj.SlsAmt = slsAmt;
                                        regisObj.Status = selected ? "C" : "H";
                                    }
                                    else
                                    {
                                        _db.OM_TBonusCustomer.DeleteObject(regisObj);
                                    }
                                }
                                else
                                {
                                    if (slsAmt > 0)
                                    {
                                        regisObj = new OM_TBonusCustomer()
                                        {
                                            BranchID = branchID,
                                            CustID = custID,
                                            BonusID = bonusID,
                                            LevelID = levelID,
                                            //Rate = (double)updated.Rate,
                                            SlsperID = slsperID,
                                            ClassID = lstProductClass[j].Code,

                                            Crtd_DateTime = DateTime.Now,
                                            Crtd_Prog = _screenNbr,
                                            Crtd_User = Current.UserName,
                                            LUpd_DateTime = DateTime.Now,
                                            LUpd_Prog = _screenNbr,
                                            LUpd_User = Current.UserName,
                                            SlsAmt = slsAmt,
                                            Territory = dtUpdated.Rows[i]["Territory"].ToString(),
                                            Zone = dtUpdated.Rows[i]["Zone"].ToString(),
                                            Status = selected ? "C" : "H"
                                        };
                                        _db.OM_TBonusCustomer.AddObject(regisObj);
                                    }

                                }
                                #endregion
                            }
                        }
                    }

                    _db.SaveChanges();
                    return Json(new { success = true, msgCode = 201405071 });
                }
                else
                {
                    throw new MessageException("14091901");
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
        }

        private Ext.Net.GridPanel BuildGridPanel(string cpnyID, string objectID, string tradeType)
        {
            var hiddenColumns = new string[] { "Registered", "Rate", "TradeType" };
            DataAccess dal = Util.Dal();
            var pc = new ParamCollection();
            pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(cpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@ObjectID", DbType.String, clsCommon.GetValueDBNull(objectID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@TradeType", DbType.String, clsCommon.GetValueDBNull(tradeType), ParameterDirection.Input, 30));

            DataTable dtDataExport = dal.ExecDataTable("OM22002_pgBonus", CommandType.StoredProcedure, ref pc);
            var lstProductClass = _db.OM22002_pcProductClass(Current.UserName, Current.LangID).ToList();

            var filters = new Ext.Net.GridFilters()
            {
                Local = true
            };

            var grdBonus = new Ext.Net.GridPanel
            {
                ID = "grdBonus",
                Region = Ext.Net.Region.Center,
                Border = false,
                Store =  
                {
                    this.BuildStore(dtDataExport)
                },
                SelectionModel = 
                { 
                    new RowSelectionModel() { ID = "slmBonus", Mode = SelectionMode.Single }
                },
                View =
                {
                   new Ext.Net.GridView()
                   {
                        StripeRows = true,
                        TrackOver = true 
                   }
                },
                Plugins = 
                {
                     new Ext.Net.CellEditing()
                     {
                        ClicksToEdit = 1,
                        Listeners = 
                        {
                            BeforeEdit =
                            {
                                Fn = "Event.Grid.grdBonus_beforeEdit"
                            }
                        }
                     }
                }
            };

            var mergeCol = new Column() { 
                Text = Util.GetLang("SlsAmt")
            };
            foreach (System.Data.DataColumn col in dtDataExport.Columns)
            {
                if (!hiddenColumns.Contains(col.ColumnName))
                {
                    if (col.ColumnName == "Selected")
                    {
                        var column = new CheckColumn()
                        {
                            Text = Util.GetLang("Register"),
                            DataIndex = col.ColumnName,
                            Editable = true,
                            Locked = true,
                            Width = 50
                        };
                        grdBonus.ColumnModel.Columns.Add(column);
                    }
                    else
                    {
                        if (col.ColumnName.StartsWith("PC_"))
                        {
                            var headerText = Util.GetLang(col.ColumnName);
                            var productClass = lstProductClass.FirstOrDefault(p => p.Code == col.ColumnName.Substring(3));
                            if (productClass != null)
                            {
                                headerText = productClass.Descr;
                            }
                            var column = new NumberColumn()
                            {
                                Text = headerText,
                                DataIndex = col.ColumnName,
                                Align = Alignment.Right,
                                Format = "0,000",
                                //Width = 50,
                                Editor = { 
                                    new HQNumberField()
                                    {
                                        MinValue = 0,
                                        DecimalPrecision = 0
                                    }
                                }
                            };
                            mergeCol.Columns.Add(column);
                            filters.Filters.Add(new NumericFilter() { DataIndex = col.ColumnName });
                            //grdTest.ColumnModel.Columns.Add(column);
                        }
                        else
                        {
                            var column = new Column()
                            {
                                Text = Util.GetLang(col.ColumnName),
                                DataIndex = col.ColumnName
                            };

                            if (col.ColumnName == "LevelID")
                            {
                                var cboColLevelIDBonus = new HQCombo()
                                {
                                    ID = "cboColLevelIDBonus",
                                    HQProcedure = "OM22002_pcLevel",
                                    HQColumnShow = "Code",
                                    ValueField = "Code",
                                    DisplayField = "Code",
                                    HQParam = new StoreParameterCollection() { 
                                        new StoreParameter("@ObjectID", 
                                        "App.slmBonus.selected.items[0] ? App.slmBonus.selected.items[0].data.BonusID : Ext.String.empty", 
                                        ParameterMode.Raw) 
                                        , new StoreParameter("@TradeType", "B", ParameterMode.Value) 
                                    }
                                };
                                cboColLevelIDBonus.LoadData();

                                column.Locked = true;
                                column.Width = 50;
                                column.Editor.Add(cboColLevelIDBonus);
                            }

                            grdBonus.ColumnModel.Columns.Add(column);
                            filters.Filters.Add(new StringFilter() { DataIndex = col.ColumnName });
                        }
                    }
                }
            }
            grdBonus.ColumnModel.Columns.Add(mergeCol);
            grdBonus.Features.Add(filters);

            var rejectCol = new CommandColumn()
            {
                Width = 150,
                Commands =
                {
                    new Ext.Net.GridCommand()
                    { 
                        Text = Util.GetLang("Reject"),
                        CommandName = "reject",
                        Icon = Icon.ArrowUndo,
                        ToolTip = {
                            Text = Util.GetLang("Rejectrowchanges")
                        }
                    }
                },
                PrepareToolbar =
                {
                    Handler = "toolbar.items.get(0).setVisible(record.dirty);"
                },
                Listeners =
                {
                    Command = {
                        Handler = "Event.Grid.grd_reject(this, record);"
                    }
                }
            };
            grdBonus.ColumnModel.Columns.Add(rejectCol);

            return grdBonus;
        }

        private Store BuildStore(System.Data.DataTable data)
        {
            Store store = new Store()
            {
                Listeners =
                {
                    Update =
                    {
                        //Handler = "Event.Form.frmMain_fieldChange()"
                    },

                    DataChanged = {
                        //Handler = "Event.Form.frmMain_fieldChange()"
                    }
                }
            };
            Model mdl = new Model();

            foreach (System.Data.DataColumn col in data.Columns)
            {
                ModelFieldType rs;
                if (!Enum.TryParse<ModelFieldType>(col.DataType.Name, out rs))
                {
                    rs = ModelFieldType.Auto;
                }
                var field = new ModelField(col.ColumnName, rs);
                
                mdl.Fields.Add(field);
            }
            store.Model.Add(mdl);

            store.DataSource = data;

            return store;
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

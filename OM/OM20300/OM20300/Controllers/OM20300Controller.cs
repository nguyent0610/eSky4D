using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
//using OM20300.Models;
using System.Xml;
using System.Xml.Linq;
using System.Data.Objects;
using System.Data;
using HQSendMailApprove;
namespace OM20300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20300Controller : Controller
    {
        eSkySysEntities _db = Util.CreateObjectContext<eSkySysEntities>(true);
        OM20300Entities _app = Util.CreateObjectContext<OM20300Entities>(false);
        OM20300_pcBudget_Result _objBudget = null;
        JsonResult _logMessage = null;

        #region Action
        public ActionResult Index()
        {
            var user = _db.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
            ViewBag.BeginStatus = "H";
            ViewBag.EndStatus = "C";
            ViewBag.Roles = user.UserTypes.PassNull();
            return View();

        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public System.Web.Mvc.PartialViewResult Body(string lang)
        {
                  return PartialView();
        }
        #endregion
        
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                _objBudget = Util.ConvertToObject<OM20300_pcBudget_Result>(data, true, new string[] { "Active" });

                var budget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower());
                
                if (budget != null)
                {
                    _app.OM_PPBudget.DeleteObject(budget);
                }

                var lstItem = _app.OM_PPFreeItem.Where(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower()).ToList();
                foreach (var item in lstItem)
                {
                    _app.OM_PPFreeItem.DeleteObject(item);
                }

                var lstCpny = _app.OM_PPCpny.Where(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower()).ToList();
                foreach (var item in lstCpny)
                {
                    _app.OM_PPCpny.DeleteObject(item);
                }

                var lstAlloc = _app.OM_PPAlloc.Where(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower()).ToList();
                foreach (var item in lstAlloc)
                {
                    _app.OM_PPAlloc.DeleteObject(item);
                }
                
                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Delete);
            }
            catch (Exception ex)
            {

                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string Status = data["Status"].PassNull();
                string Handle = data["Handle"].PassNull();

                _objBudget = Util.ConvertToObject<OM20300_pcBudget_Result>(data, true ,new string[]{"Active"});
                _objBudget.Active = data["Active"].PassNull() != string.Empty ? true : false;
                _objBudget.tstamp = data["tstamp"].PassNull();

                StoreDataHandler freeItemHandler = new StoreDataHandler(data["lstInventory"]);
                var lstInvt = freeItemHandler.ObjectData<OM20300_pgFreeItem_Result>().Where(p => p.FreeItemID.PassNull() != string.Empty).ToList();

                StoreDataHandler cpnyHandler = new StoreDataHandler(data["lstCompany"]);
                var lstCpny = cpnyHandler.ObjectData<OM20300_pgCpny_Result>().Where(p => p.CpnyID.PassNull() != string.Empty).ToList();

                StoreDataHandler allocHandler = new StoreDataHandler(data["lstAlloc"]);
                var lstAlloc = allocHandler.ObjectData<OM20300_pgAlloc_Result>().Where(p => p.ObjID.PassNull() != string.Empty).ToList();

                var budget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower());
    
                if (budget == null)
                {
                    budget = new OM_PPBudget();
                    Update_BudgetHeader(budget, data, true, Status, Handle);
                    _app.OM_PPBudget.AddObject(budget);

                }
                else
                {
                    if (_objBudget.tstamp.ToHex() != budget.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "2014071002");
                    }
                    else
                    {
                        Update_BudgetHeader(budget, data, false, Status, Handle);
                    }
                   
                }
                if (_objBudget.ApplyTo == "F")
                {
                    var lstInvtDB = _app.OM_PPFreeItem.Where(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower()).ToList();
                    foreach(var invtDB in lstInvtDB)
                    {
                        if (!lstInvt.Any(p => p.FreeItemID == invtDB.FreeItemID))
                        {
                            _app.OM_PPFreeItem.DeleteObject(invtDB);
                        }
                    }

                    foreach (var item in lstInvt)
                    {
                        var freeItem = _app.OM_PPFreeItem.FirstOrDefault(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower() && p.FreeItemID == item.FreeItemID);
                        if (freeItem == null)
                        {
                            freeItem = new OM_PPFreeItem();
                            Update_Item(freeItem, item, true);
                            _app.OM_PPFreeItem.AddObject(freeItem);
                        }
                        else
                        {
                            if (item.tstamp.ToHex() != freeItem.tstamp.ToHex())
                            {
                                throw new MessageException(MessageType.Message, "2014071002");
                            }
                            Update_Item(freeItem, item, false);
                        }
                    }
                }

                var lstCpnyDB = _app.OM_PPCpny.Where(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower()).ToList();
                foreach (var cpnyDB in lstCpnyDB)
                {
                    if (!lstCpny.Any(p => p.CpnyID == cpnyDB.CpnyID && p.FreeItemID == cpnyDB.FreeItemID))
                    {
                        _app.OM_PPCpny.DeleteObject(cpnyDB);
                    }

                }

                foreach (var item in lstCpny)
                {
                    var cpnyDB = _app.OM_PPCpny.FirstOrDefault(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower() && p.CpnyID == item.CpnyID && p.FreeItemID == item.FreeItemID);
                    if (cpnyDB == null)
                    {
                        cpnyDB = new OM_PPCpny();
                        Update_Cpny(cpnyDB, item, true);
                        _app.OM_PPCpny.AddObject(cpnyDB);
                    }
                    else
                    {
                        if (item.tstamp.ToHex() != cpnyDB.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "2014071002");
                        }
                        Update_Cpny(cpnyDB, item, false);
                    }
                }

                var lstAllocDB = _app.OM_PPAlloc.Where(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower()).ToList();
                foreach (var allocDB in lstAllocDB)
                {
                    if (!lstAlloc.Any(p => p.CpnyID == allocDB.CpnyID && p.FreeItemID == allocDB.FreeItemID && p.ObjID == allocDB.ObjID))
                    {
                        _app.OM_PPAlloc.DeleteObject(allocDB);
                    }

                }

                foreach (var item in lstAlloc)
                {
                    var allocDB = _app.OM_PPAlloc.FirstOrDefault(p => p.BudgetID.ToLower() == _objBudget.BudgetID.ToLower() && p.CpnyID == item.CpnyID && p.FreeItemID == item.FreeItemID && p.ObjID == item.ObjID);
                    if (allocDB == null)
                    {
                        allocDB = new OM_PPAlloc();
                        Update_Alloc(allocDB, item, true);
                        _app.OM_PPAlloc.AddObject(allocDB);
                    }
                    else
                    {
                        if (item.tstamp.ToHex() != allocDB.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "2014071002");
                        }
                        Update_Alloc(allocDB, item, false);
                    }
                }
               
                _app.SaveChanges();

                return Util.CreateMessage(MessageProcess.Save);
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
            
        }

        private void Update_BudgetHeader(OM_PPBudget s, FormCollection t, bool isnew, string Status, string Handle)
        {
            if (isnew)
            {
                s.ResetET();
                s.BudgetID = _objBudget.BudgetID;
                s.Crtd_DateTime = DateTime.Now;
                s.Crtd_Prog = "OM20300";
                s.Crtd_User = Current.UserName;
            }
            if (Handle == string.Empty || Handle == "N")
                s.Status = Status;
            else
                s.Status = Handle;

            s.Active = _objBudget.Active;
            s.Descr = _objBudget.Descr.PassNull();
            s.AllocType = _objBudget.AllocType;
            s.ApplyTo = _objBudget.ApplyTo;
            s.QtyAmtAlloc = _objBudget.QtyAmtAlloc;
            s.QtyAmtFree = _objBudget.QtyAmtFree;
            s.QtyAmtTotal = _objBudget.QtyAmtTotal;
            s.RvsdDate = _objBudget.RvsdDate;
            //s.Status = _objBudget.Status.PassNull();
            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = "OM20300";
            s.LUpd_User = Current.UserName;
        }

        private void Update_Item(OM_PPFreeItem s, OM20300_pgFreeItem_Result t, bool isnew)
        {
            if (isnew)
            {
                s.ResetET();
                s.BudgetID = _objBudget.BudgetID;
                s.FreeItemID = t.FreeItemID;
                s.Crtd_DateTime = DateTime.Now;
                s.Crtd_Prog = "OM20300";
                s.Crtd_User = Current.UserName;
            }
            s.QtyAmtAlloc = t.QtyAmtAlloc;
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.UnitDesc = t.UnitDesc;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = "OM20300";
            s.LUpd_User = Current.UserName;
        }

        private void Update_Cpny(OM_PPCpny s, OM20300_pgCpny_Result t, bool isnew)
        {
            if (isnew)
            {
                s.ResetET();
                s.BudgetID = _objBudget.BudgetID;
                s.FreeItemID = t.FreeItemID;
                s.CpnyID = t.CpnyID;
                s.Crtd_DateTime = DateTime.Now;
                s.Crtd_Prog = "OM20300";
                s.Crtd_User = Current.UserName;
            }
            s.ApplyTo = _objBudget.ApplyTo;
            s.QtyAmtAlloc = t.QtyAmtAlloc;
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.UnitDesc = t.UnitDesc;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = "OM20300";
            s.LUpd_User = Current.UserName;
        }

        private void Update_Alloc(OM_PPAlloc s, OM20300_pgAlloc_Result t, bool isnew)
        {
            if (isnew)
            {
                s.ResetET();
                s.BudgetID = _objBudget.BudgetID;
                s.FreeItemID = t.FreeItemID;
                s.CpnyID = t.CpnyID;
                s.ObjID=t.ObjID;
                s.Crtd_DateTime = DateTime.Now;
                s.Crtd_Prog = "OM20300";
                s.Crtd_User = Current.UserName;
            }
            s.ApplyTo = _objBudget.ApplyTo;
            s.QtyAmtAlloc = t.QtyAmtAlloc;
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.UnitDesc = t.UnitDesc;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = "OM20300";
            s.LUpd_User = Current.UserName;
        }
     
      
        #region Source
        public ActionResult OM20300_TreeBranch(string brandID)
        {
            Panel pnlTree = this.GetCmp<Panel>("pnlTree");

            TreePanel tree = new TreePanel();
            tree.ID = "treeBranch";
            tree.Fields.Add(new ModelField("Descr", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Value", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.AutoScroll = true;
            tree.RootVisible = false;
            tree.Border = false;
            tree.Header = false;
            //tree.Listeners.CheckChange.Fn = "tree_CheckChange";
            Node node = new Node();
            node.NodeID = "tree-node-root-branch";

            var lstTerritory = _app.SI_Territory.ToList();
            foreach (var item in lstTerritory)
            {
                var nodeTerritory = new Node();
            
                nodeTerritory.NodeID = "node-branch-territory" + item.Territory;
                nodeTerritory.Text = item.Territory;
                nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Territory", Mode = ParameterMode.Value });
                nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "Value", Value = item.Territory, Mode = ParameterMode.Value });
                nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = item.Descr, Mode = ParameterMode.Value });

                var lstCpny = _app.OM20300_pcBranch(Current.UserName, Current.CpnyID).Where(p => p.Territory.ToLower() == item.Territory.ToLower()).ToList();
                nodeTerritory.Leaf = lstCpny.Count == 0 ? true : false;
                foreach (var cpny in lstCpny)
                {
                    var nodeCpny = new Node();
                 
                    nodeCpny.NodeID = "node-branch-cnpy-" + cpny.BranchID;
                    nodeCpny.Text = cpny.BranchID + " - " + cpny.BranchName;
                    nodeCpny.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Company", Mode = ParameterMode.Value });
                    nodeCpny.CustomAttributes.Add(new ConfigItem() { Name = "Value", Value = cpny.BranchID, Mode = ParameterMode.Value });
                    nodeCpny.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = cpny.BranchName, Mode = ParameterMode.Value });

                    nodeTerritory.Children.Add(nodeCpny);
                }
                node.Children.Add(nodeTerritory);
            }
            if (lstTerritory.Count == 0) node.Leaf = true;
            tree.Root.Add(node);
            tree.AddTo(pnlTree);
            return this.Direct();
        }
        public ActionResult GetListBudget(string query, int start, int limit, int page)
        {
            query = query ?? string.Empty;
            List<OM20300_pcBudget_Result> lstBudget = new List<OM20300_pcBudget_Result>();
            var user = _db.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
            //int type = user.UserTypes.PassNull().Split(',').Any(p => p.ToLower() == "ho") == true ? 0 : 1;
            int type = 0;
            if (query == string.Empty)
            {
                lstBudget = _app.OM20300_pcBudget(user.CpnyID, type).ToList();
            }
            else
            {
                lstBudget = _app.OM20300_pcBudget(user.CpnyID, type).ToList().Where(p => p.Descr.ToLower().Contains(query.ToLower()) || p.BudgetID.ToLower().Contains(query.ToLower())).ToList();
            }
            var result = lstBudget.Skip(start).Take(limit);
            Paging<OM20300_pcBudget_Result> paging = new Paging<OM20300_pcBudget_Result>(result, lstBudget.Count);
            return this.Store(paging.Data, paging.TotalRecords);
        }

       
        public ActionResult GetListBudgetInventory(string budgetID)
        {
            var lstInvt = _app.OM20300_pgFreeItem(budgetID).ToList();
            return this.Store(lstInvt, lstInvt.Count);
        }
        public ActionResult GetListBudgetInventoryAlloc(string budgetID)
        {
            var lstInvt = _app.OM20300_pgInvtAlloc(budgetID, Current.CpnyID).ToList();
            return this.Store(lstInvt, lstInvt.Count);
        }
        public ActionResult GetListBudgetAlloc(string budgetID)
        {
            var lstAlloc = _app.OM20300_pgAlloc(budgetID).ToList();
            return this.Store(lstAlloc, lstAlloc.Count);
        }
        public ActionResult GetListBudgetCompany(string budgetID)
        {
            var lstCpny = _app.OM20300_pgCpny(budgetID).ToList();
            return this.Store(lstCpny, lstCpny.Count);
        }
       
        #endregion
    }
}

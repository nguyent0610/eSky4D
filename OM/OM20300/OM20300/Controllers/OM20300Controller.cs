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
using Aspose.Cells;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
namespace OM20300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20300Controller : Controller
    {
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        OM20300Entities _app = Util.CreateObjectContext<OM20300Entities>(false);
        OM20300_pcBudget_Result _objBudget = null;
        JsonResult _logMessage = null;
        string _screenNbr = "OM20300";

        #region Action
        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            var user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
            ViewBag.BeginStatus = "H";
            ViewBag.EndStatus = "C";
            ViewBag.Roles = user.UserTypes.PassNull();
            return View();

        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
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
                s.Crtd_Prog = _screenNbr;
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
            s.LUpd_Prog = _screenNbr;
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
                s.Crtd_Prog = _screenNbr;
                s.Crtd_User = Current.UserName;
            }
            s.QtyAmtAlloc = t.QtyAmtAlloc;
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.UnitDesc = t.UnitDesc;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
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
                s.Crtd_Prog = _screenNbr;
                s.Crtd_User = Current.UserName;
            }
            s.ApplyTo = _objBudget.ApplyTo;
            s.QtyAmtAlloc = t.QtyAmtAlloc;
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.UnitDesc = t.UnitDesc;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
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
                s.Crtd_Prog = _screenNbr;
                s.Crtd_User = Current.UserName;
            }
            s.ApplyTo = _objBudget.ApplyTo;
            s.QtyAmtAlloc = t.QtyAmtAlloc;
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.UnitDesc = t.UnitDesc;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
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

                var lstCpny = _app.OM20300_pcBranchID(Current.UserName, Current.CpnyID,Current.LangID).Where(p => p.Territory.ToLower() == item.Territory.ToLower()).ToList();
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
            var user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
            //int type = user.UserTypes.PassNull().Split(',').Any(p => p.ToLower() == "ho") == true ? 0 : 1;
            int type = 0;
            if (query == string.Empty)
            {
                lstBudget = _app.OM20300_pcBudget(user.CpnyID, type, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            }
            else
            {
                lstBudget = _app.OM20300_pcBudget(user.CpnyID, type, Current.UserName, Current.CpnyID, Current.LangID).ToList().Where(p => p.Descr.ToLower().Contains(query.ToLower()) || p.BudgetID.ToLower().Contains(query.ToLower())).ToList();
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
        public ActionResult GetListBudgetAlloc(string budgetID, string allocType)
        {
            var lstAlloc = _app.OM20300_pgAlloc(Current.CpnyID,Current.UserName,Current.LangID,budgetID,allocType).ToList();
            return this.Store(lstAlloc, lstAlloc.Count);
        }
        public ActionResult GetListBudgetCompany(string budgetID)
        {
            var lstCpny = _app.OM20300_pgCpny(budgetID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstCpny, lstCpny.Count);
        }
       
        #endregion


        [ValidateInput(false)]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                Dictionary<string, int> lstOrderReference = new Dictionary<string, int>();
                List<OM20300_pgAlloc_Result> lstresult = new List<OM20300_pgAlloc_Result>(); //_lstImport;
                string allocType = data["AllocType"];
                string message = string.Empty;
                string rowNumInvtNotMapped = "";
                string rowCustIDNotMapped = "";
                bool isFormatQuantity = false;
                bool isFormatAmount = false;
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    try
                    {
                        //LoadOptions loadOptions = new LoadOptions(LoadFormat.CSV);
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];
                            #region Nha Phan Phoi
                            if (allocType == "0")
                            {
                                if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() == "BUDGET ID"
                                   && workSheet.Cells[0, 1].StringValue.ToUpper().Trim() == "BRANCH ID"
                                   && workSheet.Cells[0, 2].StringValue.ToUpper().Trim().StartsWith("TOTAL")
                                   && workSheet.Cells[0, 3].StringValue.ToUpper().Trim() == "FREE ITEM ID"
                                   && workSheet.Cells[0, 4].StringValue.ToUpper().Trim().StartsWith("UNIT")
                                   && workSheet.Cells[0, 5].StringValue.ToUpper().Trim() == "ALLOCATED"
                                   && workSheet.Cells[0, 6].StringValue.ToUpper().Trim() == "DESCRIPTION"
                                   )
                                {
                                    isFormatQuantity = true;
                                }
                                else if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() == "BUDGET ID"
                                   && workSheet.Cells[0, 1].StringValue.ToUpper().Trim() == "BRANCH ID"
                                   && workSheet.Cells[0, 2].StringValue.ToUpper().Trim().StartsWith("TOTAL")
                                   && workSheet.Cells[0, 3].StringValue.ToUpper().Trim() == "ALLOCATED"
                                   && workSheet.Cells[0, 4].StringValue.ToUpper().Trim() == "DESCRIPTION"
                                )
                                {
                                    isFormatAmount = true;
                                }
                                if (!isFormatQuantity && !isFormatAmount)
                                {
                                    throw new MessageException(MessageType.Message, "20407", parm: new[] { fileInfo.Name });
                                }
                                var _lstcpnyCheck = _app.OM20300_pcBranchID(Current.UserName,Current.CpnyID,Current.LangID).ToList();
                                List<OM_PPCpny> lstppcpny = new List<OM_PPCpny>();
                                List<OM_PPBudget> lstppBudget = new List<OM_PPBudget>();
                                List<OM_PPCpny> lstppcpnyDB = new List<OM_PPCpny>();
                                #region Amount
                                if (isFormatAmount)
                                {
                                    for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                    {
                                        string budgetID = ""; string cpnyID = "";
                                        budgetID = workSheet.Cells[i, 0].StringValue.ToUpper();
                                        cpnyID = workSheet.Cells[i, 1].StringValue.ToUpper();
                                        string budgetDescr = workSheet.Cells[i, 4].Value.PassNull().Trim().ToUpper();
                                        double QtyAmtTotal = 0;
                                        try
                                        {
                                            QtyAmtTotal = workSheet.Cells[i, 2].Value == null ? 0 : workSheet.Cells[i, 2].Value.ToDouble();
                                        }
                                        catch
                                        {
                                            throw new MessageException(MessageType.Message, "201707072", parm: new[] { (i + 1).PassNull(), "TOTAL" });
                                        }
                                        double QtyAmtAlloc = 0;
                                        try
                                        {
                                            QtyAmtAlloc=workSheet.Cells[i, 3].Value == null ? 0 : workSheet.Cells[i, 3].Value.ToDouble();
                                        }
                                        catch
                                        {
                                            throw new MessageException(MessageType.Message, "201707072", parm: new[] { (i + 1).PassNull(), "ALLOCATED" });
                                        }
                                        
                                       
                                        //Check budgetDescr ko duoc de trong
                                        if (budgetDescr.PassNull() == "" && (cpnyID != "" || budgetID != ""))
                                        {
                                            throw new MessageException(MessageType.Message, "201707071", parm: new[] { (i + 1).PassNull(), "DESCRIPTION" });
                                        }
                                        //check ky tu dac biet                                 
                                        if (CheckSpecialChar(budgetID))
                                        {
                                            throw new MessageException(MessageType.Message, "201312121", parm: new[] { "Dòng " + (i + 1).PassNull() + " BUDGET ID" });
                                        }
                                        //Check CpnyID co trong danh sach cpny ko
                                        if (_lstcpnyCheck.Where(p => p.BranchID.ToUpper() == cpnyID).Count() == 0 && (cpnyID != "" || budgetID != ""))
                                        {
                                            throw new MessageException(MessageType.Message, "1016", parm: new[] { (i + 1).PassNull(), "BRANCH ID" });
                                        }
                                        //Check trong file excel co row nao bi trung lap 2 dong tro len ko(trung BudgetID,Cpny)
                                        if (lstppcpny.Where(p => p.CpnyID.PassNull().ToUpper().Trim() == cpnyID && p.BudgetID.PassNull().Trim().ToUpper() == budgetID
                                                         && p.FreeItemID.PassNull().Trim().ToUpper() == "").Count() > 0)
                                        {
                                            throw new MessageException(MessageType.Message, "1017", parm: new[] { (i + 1).PassNull() });
                                        }
                                        OM_PPCpny ppcpny = new OM_PPCpny();
                                        ppcpny.ResetET();
                                        ppcpny.BudgetID = budgetID;
                                        ppcpny.CpnyID = cpnyID;
                                        ppcpny.FreeItemID = "";
                                        ppcpny.UnitDesc = "";
                                        ppcpny.ApplyTo = "A";
                                        ppcpny.QtyAmtAlloc = QtyAmtAlloc;// workSheet.Cells[i, 3].Value == null ? 0 : workSheet.Cells[i, 3].Value.ToDouble();
                                        ppcpny.QtyAmtAvail = ppcpny.QtyAmtAlloc;
                                        ppcpny.QtyAmtSpent = 0;
                                        ppcpny.Crtd_DateTime = DateTime.Now;
                                        ppcpny.Crtd_Prog = _screenNbr;
                                        ppcpny.Crtd_User = Current.UserName;
                                        ppcpny.LUpd_DateTime = DateTime.Now;
                                        ppcpny.LUpd_Prog = _screenNbr;
                                        ppcpny.LUpd_User = Current.UserName;
                                        bool isAddCpnyID = false;
                                        if (budgetID != "" && cpnyID != "")
                                        {
                                            lstppcpny.Add(ppcpny);
                                            isAddCpnyID = true;
                                        }

                                        if (lstppBudget.Where(p => p.BudgetID == budgetID).Count() == 0 && budgetID.Trim() != "" && isAddCpnyID == true)
                                        {
                                            OM_PPBudget ppbudget = new OM_PPBudget();
                                            ppbudget.ResetET();
                                            ppbudget.BudgetID = budgetID;
                                            ppbudget.Active = true;
                                            ppbudget.ApplyTo = "A";
                                            ppbudget.Descr = workSheet.Cells[i, 4].Value.PassNull().Trim().ToUpper();
                                            ppbudget.RvsdDate = DateTime.Now.ToDateShort();
                                            ppbudget.QtyAmtTotal = QtyAmtTotal;// workSheet.Cells[i, 2].Value == null ? 0 : workSheet.Cells[i, 2].Value.ToDouble();
                                            ppbudget.AllocType = "0";
                                            ppbudget.Status = "H";
                                            ppbudget.Crtd_DateTime = DateTime.Now;
                                            ppbudget.Crtd_Prog = _screenNbr;
                                            ppbudget.Crtd_User = Current.UserName;
                                            ppbudget.LUpd_DateTime = DateTime.Now;
                                            ppbudget.LUpd_Prog = _screenNbr;
                                            ppbudget.LUpd_User = Current.UserName;
                                            lstppBudget.Add(ppbudget);
                                        }
                                    }
                                    foreach (OM_PPBudget temp in lstppBudget)
                                    {
                                        //TINH LAI SO LUONG PHAN BO         
                                        var lstppcpnyCal = lstppcpny;
                                        lstppcpnyDB = _app.OM_PPCpny.ToList().Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper()).ToList();
                                        foreach (var objDB in lstppcpnyDB)
                                        {
                                            var objImp = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.CpnyID == objDB.CpnyID && p.FreeItemID == objDB.FreeItemID).FirstOrDefault();
                                            if (objImp == null)
                                            {
                                                lstppcpnyCal.Add(objDB);
                                            }
                                            else
                                            {
                                                if (objImp.QtyAmtAlloc < objDB.QtyAmtSpent)
                                                {
                                                    //Nha PP nay co QtyAllo no hon QtySpent
                                                    throw new MessageException(MessageType.Message, "201707102", parm: new[] { temp.BudgetID, objImp.CpnyID });
                                                }
                                                else
                                                    objImp.QtyAmtSpent = objDB.QtyAmtSpent;
                                            }
                                        }
                                        temp.QtyAmtAlloc = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper()).Sum(p => p.QtyAmtAlloc);
                                        //Check tong ngan sach co nho hon tong cap phai ko
                                        if (temp.QtyAmtTotal < temp.QtyAmtAlloc)
                                        {
                                            throw new MessageException(MessageType.Message, "1018", parm: new[] { temp.BudgetID });
                                        }
                                        temp.QtyAmtFree = temp.QtyAmtTotal - temp.QtyAmtAlloc;
                                    }

                                    #region Save Data
                                    foreach (var obj in lstppcpny)
                                    {
                                        var cpny = _app.OM_PPCpny.ToList().Where(p => p.BudgetID.Trim().ToUpper() == obj.BudgetID.Trim().ToUpper()
                                                                            && p.CpnyID.Trim().ToUpper() == obj.CpnyID.Trim().ToUpper()
                                                                            && p.FreeItemID.Trim().ToUpper() == obj.FreeItemID.Trim().ToUpper()).FirstOrDefault();
                                        if (cpny == null)
                                        {
                                            _app.OM_PPCpny.AddObject(obj);
                                        }
                                        else
                                        {
                                            update_OM_PPCpny(cpny, obj);
                                        }
                                    }
                                    foreach (var obj in lstppBudget)
                                    {
                                        var budget = _app.OM_PPBudget.ToList().Where(p => p.BudgetID.Trim().ToUpper() == obj.BudgetID.Trim().ToUpper()).FirstOrDefault();
                                        if (budget == null)
                                        {
                                            _app.OM_PPBudget.AddObject(obj);
                                        }
                                        else
                                        {
                                            if (budget.Status != "H" && budget.Status != "C" && budget.Status != "D")
                                            {
                                                //neu status dang cho xet duyet thi ko import de
                                                throw new MessageException(MessageType.Message, "1029", parm: new[] { obj.BudgetID });
                                            }
                                            if (budget.ApplyTo!=obj.ApplyTo)
                                            {
                                                //không đúng applyto đã có
                                                throw new MessageException(MessageType.Message, "201707101", parm: new[] { obj.BudgetID });
                                            }
                                            
                                            Update_OM_PPBudget(budget, obj);
                                        }
                                    }
                                    #endregion
                                    _app.SaveChanges();
                                }
                                #endregion
                                #region Qty
                                else if (isFormatQuantity)
                                {
                                    List<IN_Inventory> _lstInvtCheck = _app.IN_Inventory.ToList();//OM20300_pcInventory(Current.UserName,Current.CpnyID,Current.LangID).ToList();
                                    List<OM_PPFreeItem> lstppfreeItem = new List<OM_PPFreeItem>();
                                    for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                    {
                                        string budgetID, cpnyID, invtID, unitDescr,budgetDescr = "";
                                        budgetID = workSheet.Cells[i, 0].StringValue.ToUpper();
                                        cpnyID = workSheet.Cells[i, 1].StringValue.ToUpper();
                                        invtID = workSheet.Cells[i, 3].Value.PassNull().Trim().ToUpper();
                                        unitDescr = workSheet.Cells[i, 4].Value.PassNull().Trim().ToUpper();
                                        budgetDescr = workSheet.Cells[i, 6].Value.PassNull().Trim().ToUpper();

                                        double QtyAmtTotal = 0;
                                        try
                                        {
                                            QtyAmtTotal = workSheet.Cells[i, 2].Value == null ? 0 : workSheet.Cells[i, 2].Value.ToDouble();
                                        }
                                        catch
                                        {
                                            throw new MessageException(MessageType.Message, "201707072", parm: new[] { (i + 1).PassNull(), "TOTAL" });
                                        }
                                        double QtyAmtAlloc = 0;
                                        try
                                        {
                                            QtyAmtAlloc = workSheet.Cells[i, 5].Value == null ? 0 : workSheet.Cells[i, 5].Value.ToDouble();
                                        }
                                        catch
                                        {
                                            throw new MessageException(MessageType.Message, "201707072", parm: new[] { (i + 1).PassNull(), "ALLOCATED" });
                                        }
                                        

                                        //Check budgetDescr ko duoc de trong
                                        if (budgetDescr.PassNull()=="" && (invtID != "" || cpnyID != "" || budgetID != ""))
                                        {
                                            throw new MessageException(MessageType.Message, "201707071", parm: new[] { (i + 1).PassNull(), "DESCRIPTION" });
                                        }
                                        //check ky tu dac biet                                 
                                        if (CheckSpecialChar(budgetID))
                                        {
                                            throw new MessageException(MessageType.Message, "201312121", parm: new[]{ "Dòng "+ (i + 1).PassNull()+ " BUDGET ID" });
                                        }
                                        var objInvtID = _lstInvtCheck.Where(p => p.InvtID.ToUpper() == invtID).FirstOrDefault();
                                        //Check invtID co trong danh sach IN_INventory ko
                                        if (objInvtID==null && (invtID != "" || cpnyID != "" || budgetID != ""))
                                        {
                                            throw new MessageException(MessageType.Message, "1016", parm: new[] { (i + 1).PassNull(), "FREE ITEM ID" });
                                        }
                                        //Check CpnyID co trong danh sach cpny ko
                                        if (_lstcpnyCheck.Where(p => p.BranchID.ToUpper() == cpnyID).Count() == 0 && (cpnyID != "" || budgetID != ""))
                                        {
                                            throw new MessageException(MessageType.Message, "1016", parm: new[] { (i + 1).PassNull(), "BRANCH ID" });
                                        }
                                        //Check Unit co trong danh sach Unit ko
                                        if (_app.OM20300_pcUnit(objInvtID.ClassID, invtID).ToList().Where(p => p.FromUnit == unitDescr).Count() == 0 && (invtID != "" || cpnyID != "" || budgetID != ""))
                                        {
                                            throw new MessageException(MessageType.Message, "1016", parm: new[] { (i + 1).PassNull(), "UNIT" });
                                        }
                                        //Check trong file excel co row nao bi trung lap 2 dong tro len ko(trung BudgetID,Cpny)
                                        if (lstppcpny.Where(p => p.CpnyID.PassNull().ToUpper().Trim() == cpnyID && p.BudgetID.PassNull().Trim().ToUpper() == budgetID
                                                         && p.FreeItemID.PassNull().Trim().ToUpper() == invtID).Count() > 0)
                                        {
                                            throw new MessageException(MessageType.Message, "1017", parm: new[] { (i + 1).PassNull() });
                                        }

                                        OM_PPCpny ppcpny = new OM_PPCpny();
                                        ppcpny.ResetET();
                                        ppcpny.BudgetID = budgetID;
                                        ppcpny.CpnyID = cpnyID;
                                        ppcpny.FreeItemID = invtID;
                                        ppcpny.UnitDesc = unitDescr;
                                        ppcpny.ApplyTo = "F";
                                        ppcpny.QtyAmtAlloc = QtyAmtAlloc;
                                        ppcpny.QtyAmtAvail = ppcpny.QtyAmtAlloc;
                                        ppcpny.QtyAmtSpent = 0;
                                        ppcpny.Crtd_DateTime = DateTime.Now;
                                        ppcpny.Crtd_Prog = _screenNbr;
                                        ppcpny.Crtd_User = Current.UserName;
                                        ppcpny.LUpd_DateTime = DateTime.Now;
                                        ppcpny.LUpd_Prog = _screenNbr;
                                        ppcpny.LUpd_User = Current.UserName;
                                        bool isAddCpnyID = false;
                                        if (budgetID != "" && cpnyID != "")
                                        {
                                            lstppcpny.Add(ppcpny);
                                            isAddCpnyID = true;
                                        }
                                        OM_PPFreeItem ppfreeItem =lstppfreeItem.Where(p => p.BudgetID.PassNull().ToUpper().Trim() == budgetID && p.FreeItemID.PassNull().ToUpper().Trim() == invtID).FirstOrDefault();
                                        if (ppfreeItem==null && isAddCpnyID == true)
                                        {
                                            ppfreeItem = new OM_PPFreeItem();
                                            ppfreeItem.ResetET();
                                            ppfreeItem.BudgetID = ppcpny.BudgetID;
                                            ppfreeItem.FreeItemID = ppcpny.FreeItemID;
                                            ppfreeItem.UnitDesc = unitDescr;
                                            ppfreeItem.QtyAmtAlloc = ppcpny.QtyAmtAlloc;
                                            ppfreeItem.QtyAmtAvail = ppfreeItem.QtyAmtAlloc;
                                            ppfreeItem.QtyAmtSpent = 0;
                                            ppfreeItem.Crtd_DateTime = DateTime.Now;
                                            ppfreeItem.Crtd_Prog = _screenNbr;
                                            ppfreeItem.Crtd_User = Current.UserName;
                                            ppfreeItem.LUpd_DateTime = DateTime.Now;
                                            ppfreeItem.LUpd_Prog = _screenNbr;
                                            ppfreeItem.LUpd_User = Current.UserName;
                                            lstppfreeItem.Add(ppfreeItem);
                                        }
                                        else if (ppfreeItem!=null && isAddCpnyID == true)
                                        {
                                            ppfreeItem.QtyAmtAlloc += ppcpny.QtyAmtAlloc;
                                            ppfreeItem.QtyAmtAvail = ppfreeItem.QtyAmtAlloc;
                                        }

                                        if (lstppBudget.Where(p => p.BudgetID == budgetID).Count() == 0 && budgetID.Trim() != "")
                                        {
                                            OM_PPBudget ppbudget = new OM_PPBudget();
                                            ppbudget.ResetET();
                                            ppbudget.BudgetID = budgetID;
                                            ppbudget.Active = true;
                                            ppbudget.ApplyTo = "F";
                                            ppbudget.Descr = workSheet.Cells[i, 6].Value.PassNull().Trim().ToUpper();
                                            ppbudget.RvsdDate = DateTime.Now.ToDateShort();
                                            ppbudget.QtyAmtTotal = QtyAmtTotal;
                                            ppbudget.AllocType = "0";
                                            ppbudget.Status = "H";
                                            ppbudget.Crtd_DateTime = DateTime.Now;
                                            ppbudget.Crtd_Prog = _screenNbr;
                                            ppbudget.Crtd_User = Current.UserName;
                                            ppbudget.LUpd_DateTime = DateTime.Now;
                                            ppbudget.LUpd_Prog = _screenNbr;
                                            ppbudget.LUpd_User = Current.UserName;
                                            lstppBudget.Add(ppbudget);
                                        }

                                    }

                                    foreach (OM_PPBudget temp in lstppBudget)
                                    {
                                        //TINH LAI SO LUONG PHAN BO         
                                        var lstppcpnyCal = lstppcpny;
                                        lstppcpnyDB = _app.OM_PPCpny.ToList().Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper()).ToList();
                                        foreach (var objDB in lstppcpnyDB)
                                        {
                                            var objImp = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.CpnyID == objDB.CpnyID && p.FreeItemID == objDB.FreeItemID).FirstOrDefault();
                                            if (objImp == null)
                                            {
                                                lstppcpnyCal.Add(objDB);
                                            }
                                            else
                                            {
                                                if (objImp.QtyAmtAlloc < objDB.QtyAmtSpent)
                                                {
                                                    //Nha PP nay co QtyAllo no hon QtySpent
                                                    throw new MessageException(MessageType.Message, "201707102", parm: new[] { temp.BudgetID, objImp.CpnyID });
                                                }
                                                else
                                                {
                                                    objImp.QtyAmtSpent = objDB.QtyAmtSpent;                                         
                                                }
                                            }
                                        }

                                        //TINH LAI SO LUONG PHAN BO         
                                        //var lstppfreeItemCal = lstppfreeItem.ToList();
                                        var lstppfreeItemDB = _app.OM_PPFreeItem.ToList().Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper()).ToList();
                                        foreach (var obj in lstppfreeItemDB)
                                        {
                                            var objFreeItem = lstppfreeItem.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.FreeItemID == obj.FreeItemID).FirstOrDefault();
                                            if (objFreeItem == null)
                                            {
                                                obj.QtyAmtAlloc = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.FreeItemID == obj.FreeItemID).Sum(p => p.QtyAmtAlloc);
                                                obj.QtyAmtAvail = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.FreeItemID == obj.FreeItemID).Sum(p => p.QtyAmtAvail);
                                                obj.QtyAmtSpent = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.FreeItemID == obj.FreeItemID).Sum(p => p.QtyAmtSpent);
                                                lstppfreeItem.Add(obj);
                                            }
                                            else
                                            {
                                                objFreeItem.QtyAmtAlloc = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.FreeItemID == obj.FreeItemID).Sum(p => p.QtyAmtAlloc);
                                                objFreeItem.QtyAmtAvail = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.FreeItemID == obj.FreeItemID).Sum(p => p.QtyAmtAvail);
                                                objFreeItem.QtyAmtSpent = lstppcpnyCal.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper() && p.FreeItemID == obj.FreeItemID).Sum(p => p.QtyAmtSpent);
                                            }
                                        }
                                        temp.QtyAmtAlloc = lstppfreeItem.Where(p => p.BudgetID.Trim().ToUpper() == temp.BudgetID.Trim().ToUpper()).Sum(p => p.QtyAmtAlloc);
                                        //Check tong ngan sach co nho hon tong cap phai ko
                                        if (temp.QtyAmtTotal < temp.QtyAmtAlloc)
                                        {
                                            throw new MessageException(MessageType.Message, "1018", parm: new[] { temp.BudgetID });
                                        }
                                        temp.QtyAmtFree = temp.QtyAmtTotal - temp.QtyAmtAlloc;
                                    }
                                    #region Save Data
                                    foreach (var obj in lstppcpny)
                                    {
                                        var cpny = _app.OM_PPCpny.ToList().Where(p => p.BudgetID.Trim().ToUpper() == obj.BudgetID
                                                     && p.CpnyID.Trim().ToUpper() == obj.CpnyID
                                                     && p.FreeItemID.Trim().ToUpper() == obj.FreeItemID).FirstOrDefault();
                                        if (cpny == null)
                                        {
                                            _app.OM_PPCpny.AddObject(obj);
                                        }
                                        else
                                        {
                                            update_OM_PPCpny(cpny, obj);

                                        }
                                    }
                                    foreach (var obj in lstppfreeItem)
                                    {
                                        var freeItem = _app.OM_PPFreeItem.ToList().Where(p => p.BudgetID.Trim().ToUpper() == obj.BudgetID
                                                         && p.FreeItemID.Trim().ToUpper() == obj.FreeItemID).FirstOrDefault();
                                        if (freeItem == null)
                                        {
                                            _app.OM_PPFreeItem.AddObject(obj);
                                        }
                                        else
                                        {
                                            update_OM_PPFreeItem(freeItem, obj);
                                        }
                                    }
                                    foreach (var obj in lstppBudget)
                                    {
                                        var budget = _app.OM_PPBudget.ToList().Where(p => p.BudgetID.Trim().ToUpper() == obj.BudgetID).FirstOrDefault();
                                        if (budget == null)
                                        {
                                            _app.OM_PPBudget.AddObject(obj);
                                        }
                                        else
                                        {
                                            if (budget.Status != "H" && budget.Status != "C" && budget.Status != "D")
                                            {
                                                //neu status dang cho xet duyet thi ko import de
                                                throw new MessageException(MessageType.Message, "1029", parm: new[] { obj.BudgetID });
                                            }
                                            if (budget.ApplyTo != obj.ApplyTo)
                                            {
                                                //không đúng applyto đã có
                                                throw new MessageException(MessageType.Message, "201707101", parm: new[] { obj.BudgetID });
                                            }
                                            Update_OM_PPBudget(budget, obj);
                                        }
                                    }
                                    _app.SaveChanges();
                                    #endregion
                                }
                                #endregion

                            }
                            #endregion
                    
                        }
                        return Json(new { success = true, msgCode = 20121418 }, "text/html");
                    }
                    catch (Exception ex)
                    {
                        if (ex is MessageException)
                        {
                            return (ex as MessageException).ToMessage();
                        }
                        else
                        {
                            return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
                        }
                    }
                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
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
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
                }
            }

        }
        #region Export
        [HttpPost]
        public ActionResult ExportAmount(FormCollection data)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetDataAM = workbook.Worksheets[0];
                SheetDataAM.Name = "AM";
                workbook.Worksheets.Add();
                SetCellValueGrid(SheetDataAM.Cells["A1"], Util.GetLang("BUDGETID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["B1"], Util.GetLang("BRANCHID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["C1"], Util.GetLang("TOTALEx"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["D1"], Util.GetLang("ALLOCATED"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["E1"], Util.GetLang("DESCRIPTION"), TextAlignmentType.Center, TextAlignmentType.Left);               

                SheetDataAM.Cells.SetColumnWidth(0, 15);
                SheetDataAM.Cells.SetColumnWidth(1, 15);
                SheetDataAM.Cells.SetColumnWidth(2, 15);
                SheetDataAM.Cells.SetColumnWidth(3, 15);
                SheetDataAM.Cells.SetColumnWidth(4, 20);
               
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "TemplateImportBudget_Amount.xlsx" };
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
        public ActionResult ExportQuantity(FormCollection data)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetDataAM = workbook.Worksheets[0];
                SheetDataAM.Name = "QT";
                workbook.Worksheets.Add();
                SetCellValueGrid(SheetDataAM.Cells["A1"], Util.GetLang("BUDGETID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["B1"], Util.GetLang("BRANCHID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["C1"], Util.GetLang("TOTALEx"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["D1"], Util.GetLang("FREE ITEMID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["E1"], Util.GetLang("UNITEx"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["F1"], Util.GetLang("ALLOCATED"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetDataAM.Cells["G1"], Util.GetLang("DESCRIPTION"), TextAlignmentType.Center, TextAlignmentType.Left);

                SheetDataAM.Cells.SetColumnWidth(0, 15);
                SheetDataAM.Cells.SetColumnWidth(1, 15);
                SheetDataAM.Cells.SetColumnWidth(2, 15);
                SheetDataAM.Cells.SetColumnWidth(3, 15);
                SheetDataAM.Cells.SetColumnWidth(4, 15);
                SheetDataAM.Cells.SetColumnWidth(5, 15);
                SheetDataAM.Cells.SetColumnWidth(6, 20);

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "TemplateImportBudget_Quantity.xlsx" };
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
        private void SetCellValueGrid(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.VerticalAlignment = TextAlignmentType.Center;
            style.Font.IsBold = true;
            style.Font.Size = 11;
            style.Font.Color = Color.Black;
            c.SetStyle(style);
        }
        #endregion

        private void update_OM_PPAlloc(OM_PPAlloc s, OM_PPAlloc t)
        {
            s.UnitDesc = t.UnitDesc;           
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
            s.LUpd_User = Current.UserName;
        }
        private void update_OM_PPCpny(OM_PPCpny s, OM_PPCpny t)
        {
            s.UnitDesc = t.UnitDesc;
            s.QtyAmtAlloc = t.QtyAmtAlloc;           
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = t.QtyAmtAlloc - t.QtyAmtSpent;
            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
            s.LUpd_User = Current.UserName;
        }
        private void Update_OM_PPBudget(OM_PPBudget s, OM_PPBudget t)
        {

            s.Active = t.Active;
            s.AllocType = t.AllocType;
            s.ApplyTo = t.ApplyTo;
            s.Descr = t.Descr;
            s.Status = t.Status;
            s.RvsdDate = t.RvsdDate;
            s.QtyAmtAlloc = t.QtyAmtAlloc;
            s.QtyAmtFree = t.QtyAmtFree;
            s.QtyAmtTotal = t.QtyAmtTotal;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
            s.LUpd_User = Current.UserName;


        }
        private void update_OM_PPFreeItem(OM_PPFreeItem s, OM_PPFreeItem t)
        {
            s.UnitDesc = t.UnitDesc;
            s.QtyAmtAlloc = t.QtyAmtAlloc;
          
            s.QtyAmtSpent = t.QtyAmtSpent;
            s.QtyAmtAvail = s.QtyAmtAlloc - t.QtyAmtSpent;
            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
            s.LUpd_User = Current.UserName;
        }

        private bool CheckSpecialChar(string value)
        {
            //check ky tu dac biet
            var regex = @"[,;'""@~#%/\\\.\[\{\(\*\+\?\^\$\|\]!]";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.IsMatch(value);
        }
    }
}

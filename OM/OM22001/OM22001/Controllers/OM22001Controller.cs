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
namespace OM22001.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22001Controller : Controller
    {
        private string _screenNbr = "OM22001";
        private string _beginStatus = "H";
        private string _noneStatus = "N";
        OM22001Entities _db = Util.CreateObjectContext<OM22001Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        //
        // GET: /OM22001/
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

        [DirectMethod]
        public ActionResult OM22001GetTreeBranch(string panelID)
        {
            TreePanel tree = new TreePanel();
            tree.ID = "treePanelBranch";
            tree.ItemID = "treePanelBranch";

            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
            tree.Root.Add(node);

            var lstTerritories = _db.OM22001_ptTerritory(Current.UserName).ToList();//tam thoi
            var companies = _db.OM22001_ptCompany(Current.UserName).ToList();

            foreach (var item in lstTerritories)
            {
                var nodeTerritory = new Node();
                nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = item.Territory, Mode = ParameterMode.Value });
                nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Territory", Mode = ParameterMode.Value });
                //nodeTerritory.Cls = "tree-node-parent";
                nodeTerritory.Text = item.Descr;
                nodeTerritory.Checked = false;
                nodeTerritory.NodeID = "territory-" + item.Territory;
                //nodeTerritory.IconCls = "tree-parent-icon";

                var lstCompaniesInTerr = companies.Where(x => x.Territory == item.Territory);
                foreach (var company in lstCompaniesInTerr)
                {
                    var nodeCompany = new Node();
                    nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = company.CpnyID, Mode = ParameterMode.Value });
                    nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Company", Mode = ParameterMode.Value });
                    //nodeCompany.Cls = "tree-node-parent";
                    nodeCompany.Text = company.CpnyName;
                    nodeCompany.Checked = false;
                    nodeCompany.Leaf = true;
                    nodeCompany.NodeID = "territory-company-" + item.Territory + "-" + company.CpnyID;
                    //nodeCompany.IconCls = "tree-parent-icon";

                    nodeTerritory.Children.Add(nodeCompany);

                }
                if (lstCompaniesInTerr.Count() == 0)
                {
                    nodeTerritory.Leaf = true;
                }
                node.Children.Add(nodeTerritory);
            }

            var treeBranch = X.GetCmp<Panel>(panelID);

            //tree.Listeners.ItemClick.Fn = "DiscDefintion.nodeClick";
            tree.Listeners.CheckChange.Fn = "Event.Tree.treePanelBranch_checkChange";

            tree.AddTo(treeBranch);

            return this.Direct();
        }

        public ActionResult GetDisplayById(string displayID)
        {
            var display = _db.OM_TDisplay.FirstOrDefault(x => x.DisplayID == displayID);
            return this.Store(display);
        }

        public ActionResult GetCompany(string displayID)
        {
            var companies = _db.OM22001_pgCompany(displayID, Current.CpnyID).ToList();
            return this.Store(companies);
        }

        public ActionResult GetLevel(string displayID, string displayType)
        {
            var displayLevels = _db.OM22001_pgLevel(displayID, displayType).ToList();
            return this.Store(displayLevels);
        }

        public ActionResult SaveData(FormCollection data, bool isNew)
        {
            try
            {
                string displayID = data["cboDisplayID"];
                string handle = data["cboHandle"];

                var displayInfoHandler = new StoreDataHandler(data["lstDisplay"]);
                var inputDisplay = displayInfoHandler.ObjectData<OM_TDisplay>()
                            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(displayID));
                if (inputDisplay != null)
                {
                    inputDisplay.DisplayID = displayID;

                    var display = _db.OM_TDisplay.FirstOrDefault(p => p.DisplayID == inputDisplay.DisplayID);
                    if (display != null)
                    {
                        if (isNew)
                        {
                            throw new MessageException(MessageType.Message, "8001", "", 
                                new string[]{
                                    Util.GetLang("DisplayID")
                                });
                        }
                        if (display.tstamp.ToHex() == inputDisplay.tstamp.ToHex())
                        {
                            Update_Header(ref display, inputDisplay, false);

                            Save_Cpny(data, display);
                            Save_Level(data, display);

                            // handle here
                            if (handle != _noneStatus && handle != null)
                                Save_Task(data, display, handle);
                            else
                                Submit_Data(display.DisplayID);

                            return Json(new { success = true, msgCode = 201405071 });
                        }
                        else {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        display = new OM_TDisplay();
                        Update_Header(ref display, inputDisplay, true);
                        _db.OM_TDisplay.AddObject(display);

                        Save_Cpny(data, display);
                        Save_Level(data, display);

                        // handle here
                        if (handle != _noneStatus && handle != null)
                            Save_Task(data, display, handle);
                        else
                            Submit_Data(display.DisplayID);

                        return Json(new { success = true, msgCode = 201405071 });
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "744");
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

        public ActionResult DeleteDisplay(string displayID)
        {
            try
            {
                var display = _db.OM_TDisplay.FirstOrDefault(p => p.DisplayID == displayID);
                if (display != null)
                {
                    _db.OM_TDisplay.DeleteObject(display);

                    var cpnies = _db.OM_TDisplayCpny.Where(c => c.DisplayID == displayID).ToList();
                    foreach (var cpny in cpnies)
                    {
                        _db.OM_TDisplayCpny.DeleteObject(cpny);
                    }

                    var levels = _db.OM_TDisplayLevel.Where(l => l.DisplayID == displayID).ToList();
                    foreach (var level in levels)
                    {
                        _db.OM_TDisplayLevel.DeleteObject(level);
                    }

                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else {
                    throw new MessageException(MessageType.Message, "89", "",new string[]{ Util.GetLang("DisplayID") });
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

        private void Save_Task(FormCollection data, OM_TDisplay display, string handle)
        {
            var cpnyHandler = new StoreDataHandler(data["lstCpny"]);
            var lstCpny = cpnyHandler.ObjectData<OM22001_pgCompany_Result>()
                        .Where(p => !string.IsNullOrWhiteSpace(p.CpnyID))
                        .ToList();

            string branch = string.Empty;
            foreach (var cpny in lstCpny)
            {
                branch += cpny.CpnyID + ',';
            }

            if (branch.Length > 0)
            {
                branch = branch.Substring(0, branch.Length - 1);
            }

            var pTask = _db.HO_PendingTasks.FirstOrDefault(x => x.ObjectID == display.DisplayID
                                            && x.EditScreenNbr == _screenNbr
                                            && x.BranchID == branch);
            var flowHandle = _db.SI_ApprovalFlowHandle.FirstOrDefault(x=>x.AppFolID == _screenNbr
                                            && x.Status == display.Status
                                            && x.Handle == handle);

            if (pTask == null && flowHandle != null)
            {
                if (!flowHandle.Param00.PassNull().Split(',').Any(p => p.ToLower() == "notapprove"))
                {
                    HO_PendingTasks newTask = new HO_PendingTasks();
                    newTask.BranchID = branch;
                    newTask.ObjectID = display.DisplayID;
                    newTask.EditScreenNbr = _screenNbr;
                    newTask.Content = string.Format(flowHandle.ContentApprove, display.DisplayID, display.Descr, branch);
                    newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                    newTask.Crtd_Prog = newTask.LUpd_Prog = _screenNbr;
                    newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                    newTask.Status = flowHandle.ToStatus;
                    newTask.tstamp = new byte[1];
                    _db.HO_PendingTasks.AddObject(newTask);

                }
                display.Status = flowHandle.ToStatus;
            }
            Submit_Data(display.DisplayID, flowHandle, branch);
        }

        private void Submit_Data(string displayID, SI_ApprovalFlowHandle handle = null, string lstBranch = null)
        {
            _db.SaveChanges();
            if (handle != null)
            {
                try
                {
                    // send email
                    Approve.Mail_Approve(handle.AppFolID, displayID,
                        handle.RoleID, handle.Status, handle.Handle,
                        Current.LangID.ToString(), Current.UserName, lstBranch, Current.CpnyID,
                        string.Empty, string.Empty, string.Empty);
                }
                catch
                {
                    //throw new MessageException(MessageType.Message, "11032015");
                }
            }
        }

        private void Save_Cpny(FormCollection data, OM_TDisplay display)
        {
            var cpnyChangeHandler = new StoreDataHandler(data["lstCpnyChange"]);
            var lstCpnyChange = cpnyChangeHandler.BatchObjectData<OM22001_pgCompany_Result>();

            foreach (var created in lstCpnyChange.Created)
            {
                var createdCpny = _db.OM_TDisplayCpny.FirstOrDefault(x => x.DisplayID == display.DisplayID && x.CpnyID == created.CpnyID);
                if (!string.IsNullOrWhiteSpace(created.CpnyID) && createdCpny == null)
                {
                    createdCpny = new OM_TDisplayCpny();
                    createdCpny.DisplayID = display.DisplayID;
                    createdCpny.CpnyID = created.CpnyID;
                    update_Cpny(ref createdCpny, created, true);
                    _db.OM_TDisplayCpny.AddObject(createdCpny);
                }
            }

            //foreach (var updated in lstCpnyChange.Updated)
            //{
            //    var createdCpny = _db.OM_TDisplayCpny.FirstOrDefault(x => x.DisplayID == display.DisplayID && x.CpnyID == updated.CpnyID);
            //    if (!string.IsNullOrWhiteSpace(updated.CpnyID) && createdCpny != null)
            //    {
            //        update_Cpny(ref createdCpny, updated, false);
            //    }
            //}

            foreach (var deleted in lstCpnyChange.Deleted)
            {
                var createdCpny = _db.OM_TDisplayCpny.FirstOrDefault(x => x.DisplayID == display.DisplayID && x.CpnyID == deleted.CpnyID);
                if (!string.IsNullOrWhiteSpace(deleted.CpnyID) && createdCpny != null)
                {
                    _db.OM_TDisplayCpny.DeleteObject(createdCpny);
                }
            }
        }

        private void update_Cpny(ref OM_TDisplayCpny createdCpny, OM22001_pgCompany_Result created, bool isNew)
        {
            if (isNew) {
                createdCpny.Crtd_DateTime = DateTime.Now;
                createdCpny.Crtd_Prog = _screenNbr;
                createdCpny.Crtd_User = Current.UserName;
            }
            createdCpny.LUpd_DateTime = DateTime.Now;
            createdCpny.LUpd_Prog = _screenNbr;
            createdCpny.LUpd_User = Current.UserName;
        }

        private void Save_Level(FormCollection data, OM_TDisplay display)
        {
            var levelChangeHandler = new StoreDataHandler(data["lstLevelChange"]);
            var lstLevelChange = levelChangeHandler.BatchObjectData<OM22001_pgLevel_Result>();

            foreach (var created in lstLevelChange.Created)
            {
                var createdLevel = _db.OM_TDisplayLevel.FirstOrDefault(x => x.DisplayID == display.DisplayID 
                    && x.LevelID == created.LevelID && x.DisplayType == display.DisplayType);
                if (!string.IsNullOrWhiteSpace(created.LevelID) && createdLevel == null
                    && !string.IsNullOrWhiteSpace(created.LocID))
                {
                    createdLevel = new OM_TDisplayLevel();
                    createdLevel.DisplayID = display.DisplayID;
                    createdLevel.LevelID = created.LevelID;
                    createdLevel.DisplayType = display.DisplayType;

                    update_Level(ref createdLevel, created, true);
                    _db.OM_TDisplayLevel.AddObject(createdLevel);
                }
            }

            foreach (var updated in lstLevelChange.Updated)
            {
                var createdLevel = _db.OM_TDisplayLevel.FirstOrDefault(x => x.DisplayID == display.DisplayID
                    && x.LevelID == updated.LevelID && x.DisplayType == display.DisplayType);
                if (!string.IsNullOrWhiteSpace(updated.LevelID) && createdLevel != null
                    && !string.IsNullOrWhiteSpace(updated.LocID))
                {
                    update_Level(ref createdLevel, updated, false);
                }
            }

            foreach (var deleted in lstLevelChange.Deleted)
            {
                var createdLevel = _db.OM_TDisplayLevel.FirstOrDefault(x => x.DisplayID == display.DisplayID
                    && x.LevelID == deleted.LevelID && x.DisplayType == display.DisplayType);
                if (!string.IsNullOrWhiteSpace(deleted.LevelID) && createdLevel != null
                    && !string.IsNullOrWhiteSpace(deleted.LocID))
                {
                    _db.OM_TDisplayLevel.DeleteObject(createdLevel);
                }
            }
        }

        private void Update_Header(ref OM_TDisplay display, OM_TDisplay inputDisplay, bool isNew)
        {
            if (isNew)
            {
                display.Status = _beginStatus;

                display.DisplayID = inputDisplay.DisplayID;
                display.Crtd_DateTime = DateTime.Now;
                display.Crtd_Prog = _screenNbr;
                display.Crtd_User = Current.UserName;
            }

            display.Type = inputDisplay.Type;
            display.Descr = inputDisplay.Descr;
            display.ApplyFor = inputDisplay.ApplyFor;
            display.ApplyType = inputDisplay.ApplyType;
            display.FromDate = inputDisplay.FromDate;
            display.ToDate = inputDisplay.ToDate;

            display.DisplayType = inputDisplay.DisplayType;
            //if (cboHandle.ToValue() == "N" || cboHandle.ToValue() == null)
            //    display.Status = cboStatus.ToValue().PassNull();

            display.LUpd_DateTime = DateTime.Now;
            display.LUpd_Prog = _screenNbr;
            display.LUpd_User = Current.UserName;
        }

        private void update_Level(ref OM_TDisplayLevel createdLevel, OM22001_pgLevel_Result created, bool isNew)
        {
            if (isNew)
            {
                createdLevel.Crtd_DateTime = DateTime.Now;
                createdLevel.Crtd_Prog = _screenNbr;
                createdLevel.Crtd_User = Current.UserName;
            }

            createdLevel.Bonus = created.Bonus;
            createdLevel.LocID = created.LocID;
            createdLevel.SoMatTB = created.SoMatTB;
            createdLevel.ChiPhiDauLon = created.ChiPhiDauLon;
            createdLevel.LUpd_DateTime = DateTime.Now;
            createdLevel.LUpd_Prog = _screenNbr;
            createdLevel.LUpd_User = Current.UserName;
        }
    }
}

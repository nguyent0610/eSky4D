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
namespace OM27700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]

    //public class CustExt : OM27700_pcCustomer_Result
    //{
    //    public bool Saved { get; set; }
    //}

    //public class SalesExt : OM27700_pcSalesPerson_Result
    //{
    //    public bool Saved { get; set; }
    //}
    public class OM27700Controller : Controller
    {
        private string _screenNbr = "OM27700";
        private string _beginStatus = "H";
        private string _noneStatus = "N";
        private string _applyTypeQty = "Q";
        OM27700Entities _db = Util.CreateObjectContext<OM27700Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        List<OM27700_pcInventory_Result> lstInventory = new List<OM27700_pcInventory_Result>();
        //
        // GET: /OM27700/
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            ViewBag.dateNow = DateTime.Now.ToDateShort();
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        #region -Get data-
        
        
        [DirectMethod]
        public ActionResult OM27700GetTreeBranch(string panelID)
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

            var lstTerritories = _db.OM27700_ptTerritory(Current.CpnyID, Current.UserName, Current.LangID).ToList();//tam thoi
            var companies = _db.OM27700_ptCompany("", Current.CpnyID, Current.UserName, Current.LangID).ToList();

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
                    nodeCompany.Text = company.CpnyID + "-" + company.CpnyName;
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

        public ActionResult GetAccumulateById(string accumulateID)
        {
            var accumulate = _db.OM_Accumulated.FirstOrDefault(x => x.AccumulateID == accumulateID);
            return this.Store(accumulate);
        }

        public ActionResult GetCompany(string accumulateID)
        {
            var companies = _db.OM27700_pgCompany(accumulateID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(companies);
        }

        public ActionResult GetLevel(string accumulateID)
        {
            var accumulateLevels = _db.OM27700_pgLevel(accumulateID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(accumulateLevels);
        }
        public ActionResult GetInvt(string accumulateID)
        {
            var invts = _db.OM27700_pgInvt(accumulateID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(invts);
        }

        public ActionResult GetCustomer(string accumulateID)
        {
            var lstCustomer = _db.OM27700_pgCustomer(accumulateID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(lstCustomer);
        }
        public ActionResult GetSales(string accumulateID)
        {
            var lstSales = _db.OM27700_pgSalesPerson(accumulateID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(lstSales);
        }

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, string nodeType)
        {
            var node = new Node();
            if (inactiveHierachy.Descr == "Root")
            {
                node.Text = inactiveHierachy.Descr;
            }
            else
            {
                node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Parent", Mode = ParameterMode.Value });
                node.Text = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
                node.NodeID = inactiveHierachy.NodeID + "-" + inactiveHierachy.NodeLevel + "-" + inactiveHierachy.ParentRecordID.ToString() + "-" + inactiveHierachy.RecordID;
                node.Checked = false;
            }

            var childrenInactiveHierachies = _db.SI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.Type == nodeType
                    && p.NodeLevel == level).ToList();

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (SI_Hierarchy childrenInactiveNode in childrenInactiveHierachies)
                {
                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, nodeType));
                }
            }
            else
            {
                if (childrenInactiveHierachies.Count == 0)
                {
                    var invts = lstInventory.Where(i => i.NodeID == inactiveHierachy.NodeID && i.NodeLevel == inactiveHierachy.NodeLevel && i.ParentRecordID == inactiveHierachy.ParentRecordID).ToList();// _db.OM27700_pcInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList()
                    if (invts.Count > 0)
                    {
                        foreach (var invt in invts)
                        {
                            Node invtNode = new Node();

                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Invt", Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "InvtID", Value = invt.InvtID, Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = invt.Descr, Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "CnvFact", Value = invt.CnvFact.ToString(), Mode = ParameterMode.Value });
                            invtNode.Leaf = true;
                            invtNode.Checked = false;
                            invtNode.Text = invt.InvtID + "-" + invt.Descr;
                            invtNode.NodeID = inactiveHierachy.NodeID + "-" + invt.InvtID;

                            node.Children.Add(invtNode);
                        }
                    }
                    else node.Leaf = true;
                }
                else
                {
                    node.Leaf = false;
                }
            }
            System.Diagnostics.Debug.WriteLine(node.Text);
            return node;
        }

        [DirectMethod]
        public ActionResult OM27700GetInvt(string panelID)
        {
            var a = new ItemsCollection<Plugin>();
            a.Add(Html.X().TreeViewDragDrop().DDGroup("InvtID").EnableDrop(false));

            TreeView v = new TreeView();
            //v.Plugins.Add(a);
            v.Copy = true;
            TreePanel tree = new TreePanel()
            {
                ViewConfig = v
            };
            tree.ID = "treePanelInvt";
            tree.ItemID = "treePanelInvt";
            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("NodeLevel", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ParentRecordID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("InvtID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Descr", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CnvFact", ModelFieldType.String));
            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            var root = new Node() { };

            var hierarchy = new SI_Hierarchy()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "Root",
                Type = "I"
            };
            lstInventory = _db.OM27700_pcInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, "I");
            tree.Root.Add(node);


            var treeBranch = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "treePanelInvt_checkChange";
            tree.Listeners.BeforeItemExpand.Handler = "App.treePanelInvt.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "App.treePanelInvt.el.unmask();Ext.resumeLayouts(true);";
            tree.AddTo(treeBranch);
            return this.Direct();
        }

        [DirectMethod]
        public ActionResult OM27700GetTreeCustomer(string panelID, string lstCpny)
        {
            TreePanel tree = new TreePanel();
            tree.ID = "treePanelCustomer";
            tree.ItemID = "treePanelCustomer";

            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CpnyID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CustID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CustName", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
            tree.Root.Add(node);

            if (string.IsNullOrWhiteSpace(lstCpny))
            {
                lstCpny = "@@@@@@@";
            }
            var lstTerritories = _db.OM27700_ptTerritory(Current.CpnyID, Current.UserName, Current.LangID).ToList();
            var companies = _db.OM27700_ptCompany(lstCpny, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            var lstCustomer = _db.OM27700_pcCustomer(Current.UserName, Current.CpnyID, Current.LangID).ToList();

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
                    nodeCompany.Text = company.CpnyID + "-" + company.CpnyName;
                    nodeCompany.Checked = false;
                    nodeCompany.Leaf = false;
                    nodeCompany.NodeID = "territory-company-" + item.Territory + "-" + company.CpnyID;
                    //nodeCompany.IconCls = "tree-parent-icon";
                    var lstCustAdd = lstCustomer.Where(x => x.BranchID == company.CpnyID).ToList();
                    foreach (var cust in lstCustAdd)
                    {
                        var nodeCust = new Node();
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = cust.CustID, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = cust.BranchID, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "CustID", Value = cust.CustID, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "CustName", Value = cust.CustName, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "customer", Mode = ParameterMode.Value });
                        //nodeCust.Cls = "tree-node-parent";
                        nodeCust.Text = cust.CustID + "-" + cust.CustName;
                        nodeCust.Checked = false;
                        nodeCust.Leaf = true;
                        nodeCust.NodeID = "territory-company-customer" + item.Territory + "-" + company.CpnyID + "-" + cust.CustID;
                        nodeCompany.Children.Add(nodeCust);
                    }
                    nodeTerritory.Children.Add(nodeCompany);
                }
                if (lstCompaniesInTerr.Count() == 0)
                {
                    nodeTerritory.Leaf = true;
                }
                node.Children.Add(nodeTerritory);
            }

            var treeCustomer = X.GetCmp<Panel>(panelID);

             tree.Listeners.CheckChange.Fn = "treePanelCustomer_checkChange";
            
            tree.AddTo(treeCustomer);

            return this.Direct();
        }

        [DirectMethod]
        public ActionResult OM27700GetTreeSales(string panelID, string lstCpny)
        {
            TreePanel tree = new TreePanel();
            tree.ID = "treePanelSales";
            tree.ItemID = "treePanelSales";

            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CpnyID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("SlsperID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("SlsName", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
            tree.Root.Add(node);
            if (string.IsNullOrWhiteSpace(lstCpny))
            {
                lstCpny = "@@@@@@@";
            }
            var lstTerritories = _db.OM27700_ptTerritory(Current.CpnyID, Current.UserName, Current.LangID).ToList();//tam thoi
            var companies = _db.OM27700_ptCompany(lstCpny, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            var lstSales = _db.OM27700_pcSalesPerson(Current.UserName, Current.CpnyID, Current.LangID).ToList();

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
                    nodeCompany.Text = company.CpnyID + "-" + company.CpnyName;
                    nodeCompany.Checked = false;
                    nodeCompany.Leaf = false;
                    nodeCompany.NodeID = "territory-company-" + item.Territory + "-" + company.CpnyID;
                    //nodeCompany.IconCls = "tree-parent-icon";
                    var lstSalesAdd = lstSales.Where(x => x.BranchID == company.CpnyID).ToList();
                    foreach (var sales in lstSalesAdd)
                    {
                        var nodeCust = new Node();
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = sales.SlsperID, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = sales.BranchID, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = sales.SlsperID, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "SlsName", Value = sales.SlsName, Mode = ParameterMode.Value });
                        nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "sales", Mode = ParameterMode.Value });
                        //nodeCust.Cls = "tree-node-parent";
                        nodeCust.Text = sales.SlsperID + "-" + sales.SlsName;
                        nodeCust.Checked = false;
                        nodeCust.Leaf = true;
                        nodeCust.NodeID = "territory-company-sales" + item.Territory + "-" + company.CpnyID + "-" + sales.SlsperID;
                        nodeCompany.Children.Add(nodeCust);
                    }
                    nodeTerritory.Children.Add(nodeCompany);
                }
                if (lstCompaniesInTerr.Count() == 0)
                {
                    nodeTerritory.Leaf = true;
                }
                node.Children.Add(nodeTerritory);
            }

            var treeCustomer = X.GetCmp<Panel>(panelID);

            tree.Listeners.CheckChange.Fn = "treePanelSales_checkChange";

            tree.AddTo(treeCustomer);

            return this.Direct();
        }
        #endregion

        #region -Data Processing-               
        public ActionResult SaveData(FormCollection data, bool isNew)
        {
            try
            {
                string accumulateID = data["cboAccumulateID"];
                string handle = data["cboHandle"];

                var accumulateInfoHandler = new StoreDataHandler(data["lstAccumulate"]);
                var inputAccumulate = accumulateInfoHandler.ObjectData<OM_Accumulated>().FirstOrDefault(p => !string.IsNullOrWhiteSpace(accumulateID));
                            
                if (inputAccumulate != null)
                {
                    inputAccumulate.AccumulateID = accumulateID;

                    var accumulate = _db.OM_Accumulated.FirstOrDefault(p => p.AccumulateID.ToUpper() == accumulateID.ToUpper());
                    if (accumulate != null)
                    {
                        if (isNew)
                        {
                            throw new MessageException(MessageType.Message, "8001", "", new string[] { Util.GetLang("ProcID") });
                        }
                        if (accumulate.tstamp.ToHex() == inputAccumulate.tstamp.ToHex())
                        {
                            Update_Header(ref accumulate, inputAccumulate, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        accumulate = new OM_Accumulated();
                        Update_Header(ref accumulate, inputAccumulate, true);
                        _db.OM_Accumulated.AddObject(accumulate);
                    }
                    Save_Cpny(data, accumulateID);
                    Save_Level(data, accumulateID);
                    if (inputAccumulate.ApplyType == _applyTypeQty)
                    {
                        Save_Invt(data, accumulateID);
                    }
                    Save_Customer(data, accumulateID);
                    Save_Sales(data, accumulateID);
                    // handle here
                    if (handle != _noneStatus && handle != null)
                    {
                        Save_Task(data, accumulate, handle);
                    }
                    else
                    {
                        Submit_Data(accumulateID);
                    }
                    return Json(new { success = true, msgCode = 201405071 });
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

        public ActionResult DeleteAccumulate(string accumulateID)
        {
            try
            {
                accumulateID = accumulateID.ToUpper();
                var accumulate = _db.OM_Accumulated.FirstOrDefault(p => p.AccumulateID.ToUpper() == accumulateID);
                if (accumulate != null)
                {
                    _db.OM_Accumulated.DeleteObject(accumulate);

                    var cpnies = _db.OM_AccumulatedCpny.Where(c => c.AccumulateID.ToUpper() == accumulateID).ToList();
                    foreach (var cpny in cpnies)
                    {
                        _db.OM_AccumulatedCpny.DeleteObject(cpny);
                    }

                    var levels = _db.OM_AccumulatedLevel.Where(l => l.AccumulateID.ToUpper() == accumulateID).ToList();
                    foreach (var level in levels)
                    {
                        _db.OM_AccumulatedLevel.DeleteObject(level);
                    }
                    var invts = _db.OM_AccumulatedInvt.Where(l => l.AccumulateID.ToUpper() == accumulateID).ToList();
                    foreach (var it in invts)
                    {
                        _db.OM_AccumulatedInvt.DeleteObject(it);
                    }
                    var lstCust = _db.OM_AccumulatedCustomer.Where(l => l.AccumulateID.ToUpper() == accumulateID).ToList();
                    foreach (var item in lstCust)
                    {
                        _db.OM_AccumulatedCustomer.DeleteObject(item);
                    }

                    var lstSales = _db.OM_AccumulatedSalesPerson.Where(l => l.AccumulateID.ToUpper() == accumulateID).ToList();
                    foreach (var item in lstSales)
                    {
                        _db.OM_AccumulatedSalesPerson.DeleteObject(item);
                    }
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("ProcID") });
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

        private void Save_Task(FormCollection data, OM_Accumulated accumulate, string handle)
        {
            var cpnyHandler = new StoreDataHandler(data["lstCpny"]);
            var lstCpny = cpnyHandler.ObjectData<OM27700_pgCompany_Result>()
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

            var pTask = _db.HO_PendingTasks.FirstOrDefault(x => x.ObjectID == accumulate.AccumulateID
                                            && x.EditScreenNbr == _screenNbr
                                            && x.BranchID == branch);
            var flowHandle = _db.SI_ApprovalFlowHandle.FirstOrDefault(x => x.AppFolID == _screenNbr
                                            && x.Status == accumulate.Status
                                            && x.Handle == handle);

            if (pTask == null && flowHandle != null)
            {
                if (!flowHandle.Param00.PassNull().Split(',').Any(p => p.ToLower() == "notapprove"))
                {
                    HO_PendingTasks newTask = new HO_PendingTasks();
                    newTask.BranchID = branch;
                    newTask.ObjectID = accumulate.AccumulateID;
                    newTask.EditScreenNbr = _screenNbr;
                    newTask.Content = string.Format(flowHandle.ContentApprove, accumulate.AccumulateID, accumulate.Descr, branch);
                    newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                    newTask.Crtd_Prog = newTask.LUpd_Prog = _screenNbr;
                    newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                    newTask.Status = flowHandle.ToStatus;
                    newTask.tstamp = new byte[1];
                    _db.HO_PendingTasks.AddObject(newTask);

                }
                accumulate.Status = flowHandle.ToStatus;
            }
            Submit_Data(accumulate.AccumulateID, flowHandle, branch);
        }

        private void Submit_Data(string accumulateID, SI_ApprovalFlowHandle handle = null, string lstBranch = null)
        {
            _db.SaveChanges();
            if (handle != null)
            {
                try
                {
                    // send email
                    Approve.Mail_Approve(handle.AppFolID, accumulateID,
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

        private void Save_Customer(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();
            var discCustHandler = new StoreDataHandler(data["lstCustomer"]);
            var lstDiscCust = discCustHandler.ObjectData<OM27700_pgCustomer_Result>()
                        .Where(p => Util.PassNull(p.CustID) != string.Empty)
                        .ToList();

            var lstDel = _db.OM_AccumulatedCustomer.Where(p => p.AccumulateID.ToUpper() == AccumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                if (!lstDiscCust.Any(p => p.CpnyID == lstDel[i].CpnyID && p.CustID == lstDel[i].CustID))
                {
                    _db.OM_AccumulatedCustomer.DeleteObject(lstDel[i]);
                }
            }

            foreach (var currentCust in lstDiscCust)
            {
                var cust = (from p in _db.OM_AccumulatedCustomer
                            where
                                p.AccumulateID.ToUpper() == AccumulateID
                                && p.CustID == currentCust.CustID
                                && p.CpnyID == currentCust.CpnyID
                            select p).FirstOrDefault();
                if (cust == null)
                {
                    OM_AccumulatedCustomer newCust = new OM_AccumulatedCustomer();
                    newCust.AccumulateID = accumulateID;
                    newCust.CpnyID = currentCust.CpnyID;
                    newCust.CustID = currentCust.CustID;

                    newCust.LUpd_DateTime = DateTime.Now;
                    newCust.LUpd_Prog = _screenNbr;
                    newCust.LUpd_User = Current.UserName;
                    newCust.Crtd_DateTime = DateTime.Now;
                    newCust.Crtd_Prog = _screenNbr;
                    newCust.Crtd_User = Current.UserName;
                    _db.OM_AccumulatedCustomer.AddObject(newCust);
                }
            }
        }

        private void Save_Sales(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();
            var discCustHandler = new StoreDataHandler(data["lstSales"]);
            var lstDiscCust = discCustHandler.ObjectData<OM27700_pgSalesPerson_Result>()
                        .Where(p => Util.PassNull(p.SlsperID) != string.Empty).ToList();

            var lstDel = _db.OM_AccumulatedSalesPerson.Where(p => p.AccumulateID.ToUpper() == AccumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                if (!lstDiscCust.Any(p => p.CpnyID == lstDel[i].CpnyID && p.SlsperID == lstDel[i].SlsperID))
                {
                    _db.OM_AccumulatedSalesPerson.DeleteObject(lstDel[i]);
                }
            }

            foreach (var currentCust in lstDiscCust)
            {
                var sales = (from p in _db.OM_AccumulatedSalesPerson
                             where
                                 p.AccumulateID.ToUpper() == AccumulateID
                                 && p.SlsperID == currentCust.SlsperID
                                 && p.CpnyID == currentCust.CpnyID
                             select p).FirstOrDefault();
                if (sales == null)
                {
                    OM_AccumulatedSalesPerson newCust = new OM_AccumulatedSalesPerson();
                    newCust.AccumulateID = accumulateID;
                    newCust.CpnyID = currentCust.CpnyID;
                    newCust.SlsperID = currentCust.SlsperID;

                    newCust.LUpd_DateTime = DateTime.Now;
                    newCust.LUpd_Prog = _screenNbr;
                    newCust.LUpd_User = Current.UserName;
                    newCust.Crtd_DateTime = DateTime.Now;
                    newCust.Crtd_Prog = _screenNbr;
                    newCust.Crtd_User = Current.UserName;
                    _db.OM_AccumulatedSalesPerson.AddObject(newCust);
                }
            }
        }

        private void Save_Cpny(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();
            var discCustHandler = new StoreDataHandler(data["lstCpny"]);
            var lstCpny = discCustHandler.ObjectData<OM27700_pgCompany_Result>()
                        .Where(p => Util.PassNull(p.CpnyID) != string.Empty)
                        .ToList();

            var lstDel = _db.OM_AccumulatedCustomer.Where(p => p.AccumulateID.ToUpper() == AccumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                if (!lstCpny.Any(p => p.CpnyID == lstDel[i].CpnyID))
                {
                    _db.OM_AccumulatedCustomer.DeleteObject(lstDel[i]);
                }
            }

            foreach (var currentCust in lstCpny)
            {
                var cust = (from p in _db.OM_AccumulatedCpny
                            where
                                p.AccumulateID.ToUpper() == AccumulateID
                                && p.CpnyID == currentCust.CpnyID
                            select p).FirstOrDefault();
                if (cust == null)
                {
                    OM_AccumulatedCpny newCust = new OM_AccumulatedCpny();
                    newCust.AccumulateID = accumulateID;
                    newCust.CpnyID = currentCust.CpnyID;

                    newCust.LUpd_DateTime = DateTime.Now;
                    newCust.LUpd_Prog = _screenNbr;
                    newCust.LUpd_User = Current.UserName;
                    newCust.Crtd_DateTime = DateTime.Now;
                    newCust.Crtd_Prog = _screenNbr;
                    newCust.Crtd_User = Current.UserName;
                    _db.OM_AccumulatedCpny.AddObject(newCust);
                }
            }
            //var cpnyChangeHandler = new StoreDataHandler(data["lstCpnyChange"]);
            //var lstCpnyChange = cpnyChangeHandler.BatchObjectData<OM27700_pgCompany_Result>();
            
            //lstCpnyChange.Created.AddRange(lstCpnyChange.Updated);
            //foreach (var deleted in lstCpnyChange.Deleted)
            //{
            //    if (lstCpnyChange.Created.Where(p => p.CpnyID == deleted.CpnyID).Count() > 0)
            //    {
            //        lstCpnyChange.Created.Where(p => p.CpnyID == deleted.CpnyID).FirstOrDefault().tstamp = deleted.tstamp;
            //    }
            //    else
            //    {
            //        var createdCpny = _db.OM_AccumulatedCpny.FirstOrDefault(x => x.AccumulateID.ToUpper() == accumulateID.ToUpper() && x.CpnyID == deleted.CpnyID);
            //        if (!string.IsNullOrWhiteSpace(deleted.CpnyID) && createdCpny != null)
            //        {
            //            _db.OM_AccumulatedCpny.DeleteObject(createdCpny);
            //        }
            //    }
            //}

            //foreach (OM27700_pgCompany_Result created in lstCpnyChange.Created)
            //{
            //    if (created.CpnyID == "") continue;

            //    var createdCpny = _db.OM_AccumulatedCpny.FirstOrDefault(x => x.AccumulateID.ToUpper() == accumulateID.ToUpper() && x.CpnyID == created.CpnyID);
            //    if (createdCpny != null)
            //    {
            //        if (createdCpny.tstamp.ToHex() == created.tstamp.ToHex())
            //        {
            //            update_Cpny(createdCpny, created, false);
            //        }
            //        else
            //        {
            //            throw new MessageException(MessageType.Message, "19");
            //        }
            //    }
            //    else
            //    {
            //        createdCpny = new OM_AccumulatedCpny();
            //        update_Cpny(createdCpny, created, true);
            //        createdCpny.AccumulateID = accumulateID;
            //        createdCpny.CpnyID = created.CpnyID;
            //        _db.OM_AccumulatedCpny.AddObject(createdCpny);
            //    }
            //}
        }

        private void update_Cpny(OM_AccumulatedCpny createdCpny, OM27700_pgCompany_Result created, bool isNew)
        {
            if (isNew)
            {
                createdCpny.AccumulateID = created.AccumulateID;
                createdCpny.CpnyID = created.CpnyID;
                createdCpny.Crtd_DateTime = DateTime.Now;
                createdCpny.Crtd_Prog = _screenNbr;
                createdCpny.Crtd_User = Current.UserName;
            }
            createdCpny.LUpd_DateTime = DateTime.Now;
            createdCpny.LUpd_Prog = _screenNbr;
            createdCpny.LUpd_User = Current.UserName;
        }

        private void Save_Level(FormCollection data, string accumulateID)
        {
            var levelChangeHandler = new StoreDataHandler(data["lstLevelChange"]);
            var lstLevelChange = levelChangeHandler.BatchObjectData<OM27700_pgLevel_Result>();

            foreach (var created in lstLevelChange.Created)
            {
                var createdLevel = _db.OM_AccumulatedLevel.FirstOrDefault(x => x.AccumulateID.ToUpper() == accumulateID.ToUpper()
                    && x.LevelID == created.LevelID);
                if (!string.IsNullOrWhiteSpace(created.LevelID) && createdLevel == null
                    && (!string.IsNullOrWhiteSpace(created.LevelDescr) ||
                        created.LevelType != string.Empty))
                {
                    createdLevel = new OM_AccumulatedLevel();
                    createdLevel.AccumulateID = accumulateID;
                    createdLevel.LevelID = created.LevelID;

                    update_Level(ref createdLevel, created, true);
                    _db.OM_AccumulatedLevel.AddObject(createdLevel);
                }
            }

            foreach (var updated in lstLevelChange.Updated)
            {
                var createdLevel = _db.OM_AccumulatedLevel.FirstOrDefault(x => x.AccumulateID.ToUpper() == accumulateID.ToUpper()
                    && x.LevelID == updated.LevelID);
                if (!string.IsNullOrWhiteSpace(updated.LevelID) && createdLevel != null
                    && (!string.IsNullOrWhiteSpace(updated.LevelDescr) ||
                        updated.LevelType != string.Empty))
                {
                    update_Level(ref createdLevel, updated, false);
                }
            }

            foreach (var deleted in lstLevelChange.Deleted)
            {
                var createdLevel = _db.OM_AccumulatedLevel.FirstOrDefault(x => x.AccumulateID.ToUpper() == accumulateID.ToUpper()
                    && x.LevelID == deleted.LevelID);
                if (!string.IsNullOrWhiteSpace(deleted.LevelID) && createdLevel != null)
                {
                    _db.OM_AccumulatedLevel.DeleteObject(createdLevel);
                }
            }
        }

        private void Save_Invt(FormCollection data, string accumulateID)
        {
            var invtChangeHandler = new StoreDataHandler(data["lstInvtChange"]);
            var lstInvtChange = invtChangeHandler.BatchObjectData<OM27700_pgInvt_Result>();
            lstInvtChange.Created.AddRange(lstInvtChange.Updated);

            foreach (var deleted in lstInvtChange.Deleted)
            {
                if (lstInvtChange.Created.Where(x => x.LevelID == deleted.LevelID && x.InvtID == deleted.InvtID).Count() > 0)
                {
                    lstInvtChange.Created.FirstOrDefault(x => x.LevelID == deleted.LevelID && x.InvtID == deleted.InvtID).tstamp = deleted.tstamp;
                }
                else
                {
                    var deletedLevel = _db.OM_AccumulatedInvt.FirstOrDefault(x => 
                            x.AccumulateID == accumulateID
                            && x.LevelID == deleted.LevelID && x.InvtID == deleted.InvtID);
                    if (!string.IsNullOrWhiteSpace(deleted.LevelID) && deletedLevel != null
                        && !string.IsNullOrWhiteSpace(deleted.InvtID))
                    {
                        _db.OM_AccumulatedInvt.DeleteObject(deletedLevel);
                    }
                }
            }

            foreach (var created in lstInvtChange.Created)
            {
                var createdInvt = _db.OM_AccumulatedInvt.FirstOrDefault(x => x.AccumulateID == accumulateID
                    && x.LevelID == created.LevelID && x.InvtID == created.InvtID);
                if (!string.IsNullOrWhiteSpace(created.LevelID)
                    && !string.IsNullOrWhiteSpace(created.InvtID))
                {
                    if (createdInvt != null)
                    {
                        update_Invt(ref createdInvt, created, false);
                    }
                    else
                    {
                        createdInvt = new OM_AccumulatedInvt();
                        createdInvt.AccumulateID= accumulateID;
                        createdInvt.LevelID = created.LevelID;

                        update_Invt(ref createdInvt, created, true);
                        _db.OM_AccumulatedInvt.AddObject(createdInvt);
                    }

                }
            }
        }

        private void update_Invt(ref OM_AccumulatedInvt createdInvt, OM27700_pgInvt_Result created, bool isNew)
        {
            if (isNew)
            {
                createdInvt.InvtID = created.InvtID;
                createdInvt.Crtd_DateTime = DateTime.Now;
                createdInvt.Crtd_Prog = _screenNbr;
                createdInvt.Crtd_User = Current.UserName;
            }
            createdInvt.Qty = created.Qty;
            createdInvt.LUpd_DateTime = DateTime.Now;
            createdInvt.LUpd_Prog = _screenNbr;
            createdInvt.LUpd_User = Current.UserName;
        }

        private void Update_Header(ref OM_Accumulated accumulate, OM_Accumulated inputAccumulate, bool isNew)
        {
            if (isNew)
            {
                accumulate.Status = _beginStatus;

                accumulate.AccumulateID= inputAccumulate.AccumulateID;
                accumulate.Crtd_DateTime = DateTime.Now;
                accumulate.Crtd_Prog = _screenNbr;
                accumulate.Crtd_User = Current.UserName;
            }           
            accumulate.Type = inputAccumulate.Type;
            accumulate.Descr = inputAccumulate.Descr;
            accumulate.ApplyFor = inputAccumulate.ApplyFor;
            accumulate.ApplyType = inputAccumulate.ApplyType;
            accumulate.FromDate = inputAccumulate.FromDate;
            accumulate.ToDate = inputAccumulate.ToDate;
            accumulate.ObjApply = inputAccumulate.ObjApply;
            //if (cboHandle.ToValue() == "N" || cboHandle.ToValue() == null)
            //    accumulate.Status = cboStatus.ToValue().PassNull();

            accumulate.LUpd_DateTime = DateTime.Now;
            accumulate.LUpd_Prog = _screenNbr;
            accumulate.LUpd_User = Current.UserName;
        }

        private void update_Level(ref OM_AccumulatedLevel createdLevel, OM27700_pgLevel_Result created, bool isNew)
        {
            if (isNew)
            {
                createdLevel.Crtd_DateTime = DateTime.Now;
                createdLevel.Crtd_Prog = _screenNbr;
                createdLevel.Crtd_User = Current.UserName;
            }
            createdLevel.PercentBonus = created.PercentBonus;
            createdLevel.LevelDescr = created.LevelDescr;
            createdLevel.LevelFrom = created.LevelFrom;
            createdLevel.LevelTo = created.LevelTo;
            createdLevel.LevelType = created.LevelType;
            createdLevel.LUpd_DateTime = DateTime.Now;
            createdLevel.LUpd_Prog = _screenNbr;
            createdLevel.LUpd_User = Current.UserName;
        }
        #endregion
       
    }
}

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
using HQ.eSkySys;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using System.Text.RegularExpressions;

namespace OM21100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21100Controller : Controller
    {
        private string _screenNbr = "OM21100";
        OM21100Entities _db = Util.CreateObjectContext<OM21100Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        List<OM21100_ptInventory_Result> _lstPtInventory = new List<OM21100_ptInventory_Result>();
        List<OM21100_pdSI_Hierarchy_Result> _lstSI_HierarchyI = new List<OM21100_pdSI_Hierarchy_Result>();

        private const string Channel = "CT"; // Cho Chứng Từ
        private const string CustCate = "CL"; // Loại KH cho Chứng Từ

        private const string ItemChannel = "IC"; // Mặt hàng + Channel
        private const string GItemChannel = "GC"; // Nhóm MH + Channel
        private const string ItemCustCate = "GI"; // Mặt Hàng + Loại KH
        private const string GItemCustCate = "GP"; // Nhóm MH + Loại KH 
        private JsonResult _logMessage;
        List<OM21100_ptTreeNode_Result> lstAllNode = new List<OM21100_ptTreeNode_Result>();
        // GET: /OM21100/
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            var objCheck = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "OM21100SAMEPROMOKIND");
            var sameKind = true;
            if (objCheck != null && objCheck.IntVal == 0)
            {
                sameKind = false;
            }
            bool allowImport = false
                , allowExport = false;

            var objConfig = _db.OM21100_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                allowExport = objConfig.AllowExport.HasValue ? objConfig.AllowExport.Value : false;
                allowImport = objConfig.AllowImport.HasValue ? objConfig.AllowImport.Value : false;
            }
            //var allowExport = true;
            //var allowImport = true;
            ViewBag.allowExport = allowExport;
            ViewBag.allowImport = allowImport;
            ViewBag.addSameKind = sameKind;
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetDiscInfo(string discID)
        {
            var discInfo = _db.OM_Discount.FirstOrDefault(x => x.DiscID == discID);
            return this.Store(discInfo);
        }

        public ActionResult GetDiscSeqInfo(string discID, string discSeq)
        {
            var discSeqInfo = _db.OM_DiscSeq.FirstOrDefault(x => x.DiscID == discID && x.DiscSeq == discSeq);
            return this.Store(discSeqInfo);
        }

        public ActionResult GetDiscBreak(string discID, string discSeq)
        {
            var discBreaks = _db.OM21100_pgDiscBreak(discID, discSeq, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(discBreaks);
        }

        public ActionResult GetFreeItem(string discID, string discSeq)
        {
            var freeItems = _db.OM21100_pgFreeItem(discID, discSeq, "").ToList();
            return this.Store(freeItems);
        }

        [DirectMethod]
        public ActionResult OM21100GetTreeBranch(string panelID)
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
            node.Checked = false;

            lstAllNode = _db.OM21100_ptTreeNode(Current.UserName, Current.CpnyID, Current.LangID).ToList();


            var maxLevel = lstAllNode.Max(x => x.LevelID);
            var lstFirst = lstAllNode.Where(x => x.LevelID == maxLevel).ToList();
            var crrLevel = maxLevel - 1;
            if (lstFirst.Count > 0)
            {
                string crrParent = string.Empty;
                Node parentNode = null;
                bool isAddChild = false;// lstFirst.Where(x => x.ParentID != string.Empty).Count() > 0;
                foreach (var it in lstFirst)
                {
                    var childNode = SetNodeValue(it, Ext.Net.Icon.UserHome);
                    GetChildNode(ref childNode, (int)crrLevel, it.Code);

                    if (it.ParentID != crrParent)
                    {
                        crrParent = it.ParentID;
                        parentNode.Children.Add(childNode);
                        isAddChild = true;
                        node.Children.Add(parentNode);
                    }
                    else
                    {
                        if (it.ParentID != string.Empty)
                        {
                            parentNode.Children.Add(childNode);
                        }
                    }
                    if (!isAddChild)
                    {
                        node.Children.Add(childNode);
                    }
                }
            }

            node.Icon = Ext.Net.Icon.FolderHome;

            tree.Root.Add(node);

            var treeBranch = X.GetCmp<Panel>(panelID);

            //tree.Listeners.ItemClick.Fn = "DiscDefintion.nodeClick";
            tree.Listeners.CheckChange.Fn = "DiscDefintion.Event.treePanelBranch_checkChange";
            tree.Listeners.ItemCollapse.Fn = "tree_ItemCollapse";
            tree.AddTo(treeBranch);
            
            return this.Direct();
        }
        List<string> lst = new List<string>();
        private Node SetNodeValue(OM21100_ptTreeNode_Result objNode, Ext.Net.Icon icon)
        {
            Node node = new Node();

            Random rand = new Random();
            node.NodeID = objNode.Code + objNode.ParentID + (rand.Next(999, 9999) + objNode.LevelID) .ToString();
            node.Checked = false;
            node.Text = objNode.Descr;
            node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = objNode.Type, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = objNode.Code, Mode = Ext.Net.ParameterMode.Value });          
            node.Icon = objNode.LevelID != 0 ? icon : Ext.Net.Icon.Folder;
            node.Leaf = objNode.LevelID == 0;// true;
            node.IconCls = "tree-node-noicon";
            return node;
        }
        private void GetChildNode(ref Node crrNode, int level, string parrentID)
        {
            if (level >= 0)
            {
                var lstSub = lstAllNode.Where(x => x.ParentID == parrentID && x.LevelID == level).ToList();

                if (lstSub.Count > 0)
                {
                    var crrLevel = level - 1;
                    string crrParent = string.Empty;
                    foreach (var it in lstSub)
                    {
                        var childNode = SetNodeValue(it, Ext.Net.Icon.FolderGo);
                        GetChildNode(ref childNode, crrLevel, it.Code);
                        crrNode.Children.Add(childNode);                        
                    }
                }
                else
                {
                    crrNode.Leaf = true;
                }
            }
            else
            {
                crrNode.Leaf = true;
            }
        }

        //[DirectMethod]
        //public ActionResult OM21100GetTreeBranch(string panelID)
        //{
        //    TreePanel tree = new TreePanel();
        //    tree.ID = "treePanelBranch";
        //    tree.ItemID = "treePanelBranch";

        //    tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
        //    tree.Fields.Add(new ModelField("Type", ModelFieldType.String));

        //    tree.Border = false;
        //    tree.RootVisible = true;
        //    tree.Animate = true;

        //    Node node = new Node();
        //    node.NodeID = "Root";
        //    tree.Root.Add(node);

        //    var lstTerritories = _db.OM21100_ptTerritory(Current.UserName).ToList();//tam thoi
        //    var companies = _db.OM21100_ptCompany(Current.UserName).ToList();

        //    if (lstTerritories.Count == 0)
        //    {
        //        node.Leaf = true;
        //    }

        //    foreach (var item in lstTerritories)
        //    {
        //        var nodeTerritory = new Node();
        //        nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = item.Territory, Mode = ParameterMode.Value });
        //        nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Territory", Mode = ParameterMode.Value });
        //        //nodeTerritory.Cls = "tree-node-parent";
        //        nodeTerritory.Text = item.Descr;
        //        nodeTerritory.Checked = false;
        //        nodeTerritory.NodeID = "territory-" + item.Territory;
        //        //nodeTerritory.IconCls = "tree-parent-icon";

        //        var lstCompaniesInTerr = companies.Where(x => x.Territory == item.Territory);
        //        foreach (var company in lstCompaniesInTerr)
        //        {
        //            var nodeCompany = new Node();
        //            nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = company.CpnyID, Mode = ParameterMode.Value });
        //            nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Company", Mode = ParameterMode.Value });
        //            //nodeCompany.Cls = "tree-node-parent";
        //            nodeCompany.Text = company.CpnyName;
        //            nodeCompany.Checked = false;
        //            nodeCompany.Leaf = true;
        //            nodeCompany.NodeID = "territory-company-" + item.Territory + "-" + company.CpnyID;
        //            //nodeCompany.IconCls = "tree-parent-icon";

        //            nodeTerritory.Children.Add(nodeCompany);

        //        }
        //        if (lstCompaniesInTerr.Count() == 0)
        //        {
        //            nodeTerritory.Leaf = true;
        //        }
        //        node.Children.Add(nodeTerritory);
        //    }

        //    var treeBranch = X.GetCmp<Panel>(panelID);

        //    //tree.Listeners.ItemClick.Fn = "DiscDefintion.nodeClick";
        //    tree.Listeners.CheckChange.Fn = "DiscDefintion.Event.treePanelBranch_checkChange";

        //    tree.AddTo(treeBranch);

        //    return this.Direct();
        //}

        // Tree Free Item
        [DirectMethod]
        public ActionResult OM21100LoadTreeFreeItem(string panelID)
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
            tree.ID = "treePanelFreeItem";
            tree.ItemID = "treePanelFreeItem";
            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("NodeLevel", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ParentRecordID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("InvtID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Descr", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CnvFact", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Unit", ModelFieldType.String));
            tree.Fields.Add(new ModelField("InvtType", ModelFieldType.String));
            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            var root = new Node() { };

            var hierarchy = new OM21100_pdSI_Hierarchy_Result()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "Root"
            };
            // Get inventory for load tree
            _lstPtInventory = _db.OM21100_ptInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            _lstSI_HierarchyI = _db.OM21100_pdSI_Hierarchy(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, "I");
            tree.Root.Add(node);


            var treeItem = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "DiscDefintion.Event.treePanelFreeItem_checkChange";
            tree.Listeners.BeforeItemExpand.Handler = "App.treePanelFreeItem.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "App.treePanelFreeItem.el.unmask();Ext.resumeLayouts(true);";
            tree.AddTo(treeItem);
            return this.Direct();
        }

        //OM21100LoadTreeInventory

        [DirectMethod]
        public ActionResult OM21100LoadTreeInventory(string panelID)
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
            tree.Fields.Add(new ModelField("Unit", ModelFieldType.String));
            tree.Fields.Add(new ModelField("InvtType", ModelFieldType.String));
            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            var root = new Node() { };

            var hierarchy = new OM21100_pdSI_Hierarchy_Result()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "Root"
            };
            // Get inventory for load tree
            _lstPtInventory = _db.OM21100_ptInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            _lstSI_HierarchyI = _db.OM21100_pdSI_Hierarchy(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, "I");
            tree.Root.Add(node);

            var treeBranch = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "DiscDefintion.Event.treePanelInvt_checkChange";
            tree.Listeners.BeforeItemExpand.Handler = "App.treePanelInvt.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "App.treePanelInvt.el.unmask();Ext.resumeLayouts(true);";
            tree.AddTo(treeBranch);
            return this.Direct();
        }

        [DirectMethod]
        public ActionResult OM21100LoadTreeBundleItem(string panelID)
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
            tree.ID = "treePanelBundle";
            tree.ItemID = "treePanelBundle";
            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("NodeLevel", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ParentRecordID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("InvtID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Descr", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CnvFact", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Unit", ModelFieldType.String));
            tree.Fields.Add(new ModelField("InvtType", ModelFieldType.String));
            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            var root = new Node() { };

            var hierarchy = new OM21100_pdSI_Hierarchy_Result()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "Root"
            };
            // Get inventory for load tree
            _lstPtInventory = _db.OM21100_ptInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            _lstSI_HierarchyI = _db.OM21100_pdSI_Hierarchy(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, "I");
            tree.Root.Add(node);

            var treeBranch = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "DiscDefintion.Event.treePanelBundle_checkChange";
            tree.Listeners.BeforeItemExpand.Handler = "App.treePanelBundle.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "App.treePanelBundle.el.unmask();Ext.resumeLayouts(true);";
            tree.AddTo(treeBranch);
            return this.Direct();
        }
        
        [DirectMethod]
        public ActionResult OM21100GetTreeCustomer(string panelID)
        {
            TreePanel tree = new TreePanel();
            tree.ID = "treePanelCustomer";
            tree.ItemID = "treePanelCustomer";

            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("BranchID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Territory", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
            tree.Root.Add(node);

            var lstChannel = _db.OM21100_ptChannel(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            var lstCust = _db.OM21100_ptCustomer(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            var lstCustClass = _db.OM21100_ptCustClass(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            if (lstChannel.Count() == 0)
            {
                node.Leaf = true;
            }
            
            foreach (var item in lstChannel)
            {
                string channel = item.Channel;
                var nodeChannel = new Node();
                nodeChannel.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = channel, Mode = ParameterMode.Value });
                nodeChannel.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Channel", Mode = ParameterMode.Value });
                //nodeTerritory.Cls = "tree-node-parent";
                nodeChannel.Text = item.Descr;
                nodeChannel.Checked = false;
                nodeChannel.NodeID = "channel-" + channel;
                //nodeTerritory.IconCls = "tree-parent-icon";
                
                var lstCustClassInChannel = lstCustClass.Where(x => x.Channel == channel);//.ToList();
                foreach (var custClass in lstCustClassInChannel)
                {
                    var nodeCussClass = new Node();
                    nodeCussClass.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = custClass.ClassID, Mode = ParameterMode.Value });
                    nodeCussClass.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "ClassID", Mode = ParameterMode.Value });
                    nodeCussClass.Text = custClass.Descr;
                    nodeCussClass.Checked = false;
                    nodeCussClass.NodeID = "channel-custclassid-" + channel + "-" + custClass.ClassID;

                    string custClassID = custClass.ClassID;
                    var lstCustInCustClass = lstCust.Where(x => x.Channel == channel && x.ClassID == custClassID);//.ToList();
                     foreach (var cust in lstCustInCustClass)
                     {
                         var nodeCust = new Node();
                         nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = cust.CustID, Mode = ParameterMode.Value });
                         nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "CustID", Mode = ParameterMode.Value });
                         nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "BranchID", Value = cust.BranchID, Mode = ParameterMode.Value });
                         nodeCust.CustomAttributes.Add(new ConfigItem() { Name = "Territory", Value = cust.TerritoryName, Mode = ParameterMode.Value });
                         //nodeCompany.Cls = "tree-node-parent";
                         nodeCust.Text = cust.CustName;
                         nodeCust.Checked = false;
                         nodeCust.Leaf = true;
                         nodeCust.NodeID = "channel-custclassid-custid" + channel + "-" + custClassID + "-" + cust.CustID;
                         //nodeCompany.IconCls = "tree-parent-icon";
                         nodeCussClass.Children.Add(nodeCust);
                     }

                     if (lstCustInCustClass.Count() == 0)
                     {
                         nodeCussClass.Leaf = true;
                     }
                     nodeChannel.Children.Add(nodeCussClass);
                }
                if (lstCustClassInChannel.Count() == 0)
                {
                    nodeChannel.Leaf = true;
                }
                node.Children.Add(nodeChannel);
            }

            var treeBranch = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "DiscDefintion.Event.treePanelCustomer_checkChange";

            tree.AddTo(treeBranch);

            return this.Direct();
        }
        public ActionResult GetCompany(string discID, string discSeq)
        {
            var companies = _db.OM21100_pgCompany(discID, discSeq, Current.CpnyID).ToList();
            return this.Store(companies);
        }

        public ActionResult GetDiscItem(string discID, string discSeq)
        {
            var discDiscItems = _db.OM21100_pgDiscItem(discID, discSeq).ToList();
            return this.Store(discDiscItems);
        }

        public ActionResult GetDiscBundle(string discID, string discSeq)
        {
            var discBundles = _db.OM21100_pgDiscBundle(discID, discSeq).ToList();            
            return this.Store(discBundles);
        }

        public ActionResult GetDiscCustClass(string discID, string discSeq)
        {
            var discCustClasses = _db.OM21100_pgDiscCustClass(discID, discSeq).ToList();
            return this.Store(discCustClasses);
        }

        public ActionResult GetDiscCust(string discID, string discSeq)
        {
            var discCusts = _db.OM21100_pgDiscCust(discID, discSeq, Current.CpnyID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(discCusts);
        }

        public ActionResult GetDiscItemClass(string discID, string discSeq)
        {
            var discItems = _db.OM21100_pgDiscItemClass(discID, discSeq).ToList();
            return this.Store(discItems);
        }

        public ActionResult GetDiscCustCate(string discID, string discSeq)
        {
            var custcates = _db.OM21100_pgDiscCustCate(discID, discSeq, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(custcates);
        }

        public ActionResult GetDiscChannel(string discID, string discSeq)
        {
            var channels = _db.OM21100_pgDiscChannel(discID, discSeq, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(channels);
        }

        public ActionResult SaveData(FormCollection data, bool isNewDiscID, bool isNewDiscSeq)
        {
            try
            {
                var discID = data["cboDiscID"];
                var discSeq = data["cboDiscSeq"];

                if (!string.IsNullOrWhiteSpace(discID) && !string.IsNullOrWhiteSpace(discSeq))
                {
                    #region Create/Update discount
                    var disc = _db.OM_Discount.FirstOrDefault(dc => dc.DiscID == discID);

                    var discInfoHandler = new StoreDataHandler(data["lstDiscInfo"]);
                    var inputDisc = discInfoHandler.ObjectData<OM_Discount>()
                                .FirstOrDefault(p => p.DiscID == discID);

                    var discSeqInfoHandler = new StoreDataHandler(data["lstDiscSeqInfo"]);
                    var inputDiscSeq = discSeqInfoHandler.ObjectData<OM_DiscSeq>()
                                .FirstOrDefault(p => p.DiscID == discID && p.DiscSeq==discSeq);
                    if (inputDiscSeq != null)
                    {
                        inputDiscSeq.DiscClass = inputDisc.DiscClass;
                    }

                    var roles = _sys.Users.FirstOrDefault(x=>x.UserName==Current.UserName).UserTypes.Split(',');

                    if (disc != null)
                    {
                        if (isNewDiscID)
                        {
                            throw new MessageException(MessageType.Message, "8001", "", new string[] { Util.GetLang("DiscID") });
                        }
                        else
                        {
                            updateDiscount(ref disc, inputDisc, false, roles);
                            saveDiscSeq(data, disc, inputDiscSeq, isNewDiscSeq);
                            return Json(new { success = true, msgCode = 201405071, tstamp = disc.tstamp.ToHex() });
                        }
                    }
                    else
                    {
                        // Create new discount
                        disc = new OM_Discount();
                        updateDiscount(ref disc, inputDisc, true, roles);
                        _db.OM_Discount.AddObject(disc);
                        saveDiscSeq(data, disc, inputDiscSeq, isNewDiscSeq);

                        return Json(new { success = true, msgCode = 201405071 });
                    }
                    #endregion
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msgCode = 15,
                        msgParam = string.Format("{0} & {1}", Util.GetLang("DiscID"), Util.GetLang("DiscSeq"))
                    });
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

        public ActionResult DeleteDisc(string discID)
        {
            try
            {
                var ordDiscs = _db.OM_OrdDisc.Where(p => p.DiscID == discID).ToList();
                if (ordDiscs.Count() > 0)
                {
                    throw new MessageException(MessageType.Message, "18");
                }
                else
                {
                    var disc = _db.OM_Discount.FirstOrDefault(p => p.DiscID == discID);
                    if (disc != null)
                    {
                        _db.OM_Discount.DeleteObject(disc);

                        var lstSeq = (from p in _db.OM_DiscSeq where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var omDiscSeq in lstSeq)
                            _db.OM_DiscSeq.DeleteObject(omDiscSeq);

                        var lstBreak = (from p in _db.OM_DiscBreak where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var omDiscBreak in lstBreak)
                            _db.OM_DiscBreak.DeleteObject(omDiscBreak);

                        var lstFreeItem = (from p in _db.OM_DiscFreeItem where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var omDiscFreeItem in lstFreeItem)
                            _db.OM_DiscFreeItem.DeleteObject(omDiscFreeItem);

                        var lstDiscItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var omDiscItem in lstDiscItem)
                            _db.OM_DiscItem.DeleteObject(omDiscItem);

                        var lstDiscItemClass = (from p in _db.OM_DiscItemClass where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var omDiscItemClass in lstDiscItemClass)
                            _db.OM_DiscItemClass.DeleteObject(omDiscItemClass);

                        var lstDiscCust = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var omDiscCust in lstDiscCust)
                            _db.OM_DiscCust.DeleteObject(omDiscCust);

                        var lstDiscCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var omDiscCustClass in lstDiscCustClass)
                            _db.OM_DiscCustClass.DeleteObject(omDiscCustClass);

                        var lstCompany = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var item in lstCompany)
                            _db.OM_DiscCpny.DeleteObject(item);

                        var lstDiscCustCate = (from p in _db.OM_DiscCustCate where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var item in lstDiscCustCate)
                        {
                            _db.OM_DiscCustCate.DeleteObject(item);
                        }

                        var lstDiscChannel = (from p in _db.OM_DiscChannel where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var item in lstDiscChannel)
                        {
                            _db.OM_DiscChannel.DeleteObject(item);
                        }
                        Submit_Data(null, "", null);
                        return Json(new { success = true });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("DiscID") });
                    }
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

        public ActionResult DeleteDiscSeq(string discID, string discSeq)
        {
            try
            {
                discID = discID.ToUpper();
                discSeq = discSeq.ToUpper();
                var ordDiscSeqs = _db.OM_OrdDisc.Where(p => p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq).ToList();
                if (ordDiscSeqs.Count() > 0)
                {
                    throw new MessageException(MessageType.Message, "18");
                }
                else
                {
                    var discSeqDel = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq);
                    if (discSeqDel != null)
                    {
                        _db.OM_DiscSeq.DeleteObject(discSeqDel);

                        var lstBreak = (from p in _db.OM_DiscBreak where p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq select p).ToList();
                        foreach (var omDiscBreak in lstBreak)
                            _db.OM_DiscBreak.DeleteObject(omDiscBreak);

                        var lstFreeItem = (from p in _db.OM_DiscFreeItem where p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq select p).ToList();
                        foreach (var omDiscFreeItem in lstFreeItem)
                            _db.OM_DiscFreeItem.DeleteObject(omDiscFreeItem);

                        var lstDiscItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq select p).ToList();
                        foreach (var omDiscItem in lstDiscItem)
                            _db.OM_DiscItem.DeleteObject(omDiscItem);

                        var lstDiscItemClass = (from p in _db.OM_DiscItemClass where p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq select p).ToList();
                        foreach (var omDiscItemClass in lstDiscItemClass)
                            _db.OM_DiscItemClass.DeleteObject(omDiscItemClass);

                        var lstDiscCust = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq select p).ToList();
                        foreach (var omDiscCust in lstDiscCust)
                            _db.OM_DiscCust.DeleteObject(omDiscCust);

                        var lstDiscCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq select p).ToList();
                        foreach (var omDiscCustClass in lstDiscCustClass)
                            _db.OM_DiscCustClass.DeleteObject(omDiscCustClass);

                        var lstCompany = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == discID && p.DiscSeq.ToUpper() == discSeq select p).ToList();
                        foreach (var item in lstCompany)
                            _db.OM_DiscCpny.DeleteObject(item);

                        var lstDiscCustCate = (from p in _db.OM_DiscCustCate where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var item in lstDiscCustCate)
                        {
                            _db.OM_DiscCustCate.DeleteObject(item);
                        }

                        var lstDiscChannel = (from p in _db.OM_DiscChannel where p.DiscID.ToUpper() == discID select p).ToList();
                        foreach (var item in lstDiscChannel)
                        {
                            _db.OM_DiscChannel.DeleteObject(item);
                        }
                        Submit_Data(null, "", null);
                        return Json(new { success = true });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("DiscID") });
                    }
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

        private void saveDiscSeq(FormCollection data, OM_Discount inputDisc, OM_DiscSeq inputDiscSeq, bool isNewDiscSeq)
        {
            var handle = data["cboHandle"];

            var discBreakHandler = new StoreDataHandler(data["lstDiscBreak"]);
            var lstDiscBreak = discBreakHandler.ObjectData<OM21100_pgDiscBreak_Result>()
                        .Where(p => Util.PassNull(p.LineRef) != string.Empty 
                            && (p.BreakAmt > 0 || p.BreakQty > 0 || p.DiscAmt > 0))
                        .ToList();

            var freeItemHandler = new StoreDataHandler(data["lstFreeItem"]);
            var lstFreeItem = freeItemHandler.ObjectData<OM21100_pgFreeItem_Result>()
                        .Where(p => Util.PassNull(p.FreeItemID) != string.Empty)
                        .ToList();

            var discItemHandler = new StoreDataHandler(data["lstDiscItem"]);
            var lstDiscItem = discItemHandler.ObjectData<OM21100_pgDiscItem_Result>()
                        .Where(p => Util.PassNull(p.InvtID) != string.Empty)
                        .ToList();

            var companyHandler = new StoreDataHandler(data["lstCompany"]);
            var lstCompany = companyHandler.ObjectData<OM21100_pgCompany_Result>()
                        .Where(p => Util.PassNull(p.CpnyID) != string.Empty)
                        .ToList();

            var discCustClassHandler = new StoreDataHandler(data["lstDiscCustClass"]);
            var lstDiscCustClass = discCustClassHandler.ObjectData<OM21100_pgDiscCustClass_Result>()
                        .Where(p => Util.PassNull(p.ClassID) != string.Empty)
                        .ToList();

            var discCustHandler = new StoreDataHandler(data["lstDiscCust"]);
            var lstDiscCust = discCustHandler.ObjectData<OM21100_pgDiscCust_Result>()
                        .Where(p => Util.PassNull(p.CustID) != string.Empty)
                        .ToList();

            var freeItemChangeHandler = new StoreDataHandler(data["lstFreeItemChange"]);
            var lstFreeItemChange = freeItemChangeHandler.BatchObjectData<OM21100_pgFreeItem_Result>();

            var roles = _sys.Users.FirstOrDefault(x => x.UserName.ToLower() == Current.UserName.ToLower()).UserTypes.Split(',');
            
            foreach (var item in lstDiscBreak)
            {
                if (item.DiscAmt == 0 
                    && !((_db.OM_DiscFreeItem.Any(p => p.LineRef == item.LineRef && p.DiscID == inputDisc.DiscID && p.DiscSeq == inputDiscSeq.DiscSeq)
                    && !lstFreeItemChange.Deleted.Any(p => p.LineRef == item.LineRef && p.FreeItemID != string.Empty))
                    || lstFreeItem.Any(p => p.LineRef == item.LineRef && p.FreeItemID != string.Empty)
                    || lstFreeItemChange.Created.Any(p => p.LineRef == item.LineRef && p.FreeItemID != string.Empty)
                    || lstFreeItemChange.Updated.Any(p => p.LineRef == item.LineRef && p.FreeItemID != string.Empty)))
                {
                    throw new MessageException(MessageType.Message, "1798");
                }
            } 

            if (!roles.Contains("HO") && !roles.Contains("DIST") && isNewDiscSeq)
            {
                lstCompany.Add(new OM21100_pgCompany_Result() { CpnyID = Current.CpnyID });
            }
            var seq = (from p in _db.OM_DiscSeq where p.DiscID == inputDisc.DiscID && p.DiscSeq == inputDiscSeq.DiscSeq select p).FirstOrDefault();
            if (seq != null)
            {
                if (isNewDiscSeq)
                {
                    throw new MessageException(MessageType.Message, "8001", "", new string[] { Util.GetLang("DiscSeq") });
                }
                if (seq.tstamp.ToHex() != inputDiscSeq.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                updateDiscSeq(ref seq, inputDiscSeq, false, roles, handle);
            }
            else
            {
                seq = new OM_DiscSeq();
                updateDiscSeq(ref seq, inputDiscSeq, true, roles, handle);
                _db.OM_DiscSeq.AddObject(seq);
            }

            #region Upload files
            var files = Request.Files;
            string filePath = GetFilePath();
            if (files.Count > 0) // Co chon file de upload
            {
                if (files[0].ContentLength > 0)
                {
                    string midPath = string.Format("{0}{1}", inputDiscSeq.DiscID, inputDiscSeq.DiscSeq);

                    Random rand = new Random();
                    string newFolder = filePath.TrimEnd(new char[] { '\\' }) + "\\"; //  seq.Crtd_DateTime.ToString("yyyyMM") +
                    if (!System.IO.Directory.Exists(newFolder))
                    {
                        System.IO.Directory.CreateDirectory(newFolder);
                    }
                    string newFileName = midPath + files[0].FileName;
                    Util.UploadFile(newFolder, newFileName, files[0]);
                    try
                    {
                        string oldFile = filePath.TrimEnd(new char[] { '\\' }) + "\\" + seq.Profile;
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }
                    catch
                    {

                    }
                    seq.Profile = newFileName;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(seq.Profile) && string.IsNullOrWhiteSpace(inputDiscSeq.Profile))
                    {

                        Util.UploadFile(filePath, seq.Profile, null);
                        seq.Profile = string.Empty;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(seq.Profile) && string.IsNullOrWhiteSpace(inputDiscSeq.Profile))
                {

                    Util.UploadFile(filePath, seq.Profile, null);
                    seq.Profile = string.Empty;
                }

            }
            #endregion
            // Ktra có check trùng CTKM đang chạy không?
            var checkPromo = true;
            var objConfig = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "OM10100CHECKPROMO");
            if (objConfig != null && objConfig.IntVal == 1)
            {
                checkPromo = false;
            }
            if (seq.Active == 1 && checkPromo) // Không check trùng KM
            {
                var lstSeq = (from p in _db.OM_DiscSeq where p.DiscClass == inputDisc.DiscClass 
                                  && (p.DiscID.ToUpper() != inputDisc.DiscID.ToUpper() 
                                  || (p.DiscID.ToUpper() == inputDisc.DiscID && p.DiscSeq.ToUpper() != inputDiscSeq.DiscSeq.ToUpper())) select p).ToList();
                if (seq.Promo == 1)
                {
                    foreach (var othSeq in lstSeq)
                    {
                        if (othSeq.Active == 1 && othSeq.Promo == 1 
                            && ((DateTime.Compare(seq.StartDate, othSeq.EndDate) <= 0 
                            && DateTime.Compare(seq.StartDate, othSeq.StartDate) >= 0) 
                            || (DateTime.Compare(seq.EndDate, othSeq.EndDate) <= 0 
                            && DateTime.Compare(seq.EndDate, othSeq.StartDate) >= 0) 
                            || (DateTime.Compare(seq.EndDate, othSeq.EndDate) >= 0 
                            && DateTime.Compare(seq.StartDate, othSeq.StartDate) <= 0)))
                        {
                            var lstDisItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper()
                                                  && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            var lstCpny = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();

                            if (inputDisc.DiscClass == "CI" || inputDisc.DiscClass == "II" || inputDisc.DiscClass == "TI")
                            {
                                bool flat = false;
                                if (lstDisItem.Any(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)) 
                                    && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                {
                                    if (inputDisc.DiscClass == "CI")
                                    {
                                        var lstDiscCustCi = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCi.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TI")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] 
                                            { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                lstDisItem.Where(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)).FirstOrDefault().InvtID, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID
                                            });
                                    }
                                }
                            }
                            else if (inputDisc.DiscClass == "BB" || inputDisc.DiscClass == "CB" || inputDisc.DiscClass == "TB")
                            {
                                int match = 0;
                                string bundleItem = string.Empty;
                                foreach (var s in lstDiscItem)
                                {
                                    bundleItem += s.InvtID + ", ";
                                    if (lstDisItem.Any(c => c.InvtID == s.InvtID && c.UnitDesc == c.UnitDesc) 
                                        && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                        match++;
                                }
                                if (match == lstDiscItem.Count)
                                {
                                    bool flat = false;
                                    if (inputDisc.DiscClass == "CB")
                                    {
                                        var lstDiscCustCb = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCb.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TB")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        bundleItem = bundleItem.Substring(0, bundleItem.Length - 2);
                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] 
                                            { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                bundleItem, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID
                                            });
                                    }


                                }
                            }
                        }
                        else if (othSeq.Active == 1 && othSeq.Promo == 0 && DateTime.Compare(seq.EndDate, othSeq.StartDate) >= 0)
                        {
                            var lstDisItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                  && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            var lstCpny = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();

                            if (inputDisc.DiscClass == "CI" || inputDisc.DiscClass == "II" || inputDisc.DiscClass == "TI")
                            {
                                if (lstDisItem.Any(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)) 
                                    && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                {
                                    throw new MessageException(MessageType.Message,"1089","",
                                        new string[] { 
                                            seq.DiscID, 
                                            seq.DiscSeq, 
                                            othSeq.DiscID, 
                                            othSeq.DiscSeq, 
                                            lstDisItem.Where(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)).FirstOrDefault().InvtID, 
                                            lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                        });
                                }
                            }
                            else if (inputDisc.DiscClass == "BB" || inputDisc.DiscClass == "CB" || inputDisc.DiscClass == "TB")
                            {
                                int match = 0;
                                string bundleItem = string.Empty;
                                foreach (var s in lstDiscItem)
                                {
                                    bundleItem += s.InvtID + ", ";
                                    if (lstDisItem.Any(c => c.InvtID == s.InvtID && c.UnitDesc == c.UnitDesc) 
                                        && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                        match++;
                                }
                                bool flat = false;
                                if (match == lstDiscItem.Count)
                                {
                                    if (inputDisc.DiscClass == "CB")
                                    {
                                        var lstDiscCustCb = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCb.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TB")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                                && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        bundleItem = bundleItem.Substring(0, bundleItem.Length - 2);

                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                bundleItem, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var othSeq in lstSeq)
                    {
                        if (othSeq.Active == 1 && ((othSeq.Promo == 1 && DateTime.Compare(othSeq.EndDate, seq.StartDate) >= 0) || othSeq.Promo == 0))
                        {
                            var lstDisItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                  && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            var lstCpny = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            if (inputDisc.DiscClass == "CI" || inputDisc.DiscClass == "II" || inputDisc.DiscClass == "TI")
                            {
                                bool flat = false;
                                if (lstDisItem.Any(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)) 
                                    && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                {
                                    if (inputDisc.DiscClass == "CI")
                                    {
                                        var lstDiscCustCi = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCi.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TI")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                                && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                lstDisItem.Where(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)).FirstOrDefault().InvtID, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                            });
                                    }
                                }
                            }
                            else if (inputDisc.DiscClass == "BB" || inputDisc.DiscClass == "CB" || inputDisc.DiscClass == "TB")
                            {
                                int match = 0;
                                string bundleItem = string.Empty;
                                foreach (var s in lstDiscItem)
                                {
                                    bundleItem += s.InvtID + ", ";
                                    if (lstDisItem.Any(c => c.InvtID == s.InvtID && c.UnitDesc == c.UnitDesc) 
                                        && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                        match++;
                                }
                                bool flat = false;
                                if (match == lstDiscItem.Count)
                                {
                                    if (inputDisc.DiscClass == "CB")
                                    {
                                        var lstDiscCustCb = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCb.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TB")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                                && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;
                                    if (flat)
                                    {
                                        bundleItem = bundleItem.Substring(0, bundleItem.Length - 2);

                                        throw new MessageException(MessageType.Message, "1089", "",
                                        new string[] { 
                                            seq.DiscID, 
                                            seq.DiscSeq, 
                                            othSeq.DiscID, 
                                            othSeq.DiscSeq, 
                                            bundleItem, 
                                            lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                        });
                                    }

                                }
                            }
                        }
                    }
                }

            }

            saveCompany(data,inputDisc, inputDiscSeq, seq, roles, lstCompany);
        }

        private void saveCompany(FormCollection data, 
            OM_Discount inputDisc, 
            OM_DiscSeq inputSeq, 
            OM_DiscSeq seq, 
            string[] roles, 
            List<OM21100_pgCompany_Result> lstCompany)
        {
            var handle = data["cboHandle"];
            string discID = inputDisc.DiscID.ToUpper();
            string discSeq = inputSeq.DiscSeq.ToUpper();
            //if (roles.Contains("HO") || roles.Contains("DIST"))
            //{
                foreach (var cnpy in lstCompany)
                {
                    var sysCom = (from p in _db.OM_DiscCpny
                                  where p.DiscID.ToUpper() == discID
                                      && p.DiscSeq.ToUpper() == discSeq
                                      && p.CpnyID == cnpy.CpnyID select p).FirstOrDefault();
                    if (sysCom == null)
                    {
                        OM_DiscCpny newComp = new OM_DiscCpny();
                        newComp.DiscID = inputDisc.DiscID;
                        newComp.DiscSeq = inputSeq.DiscSeq;
                        newComp.CpnyID = cnpy.CpnyID;

                        _db.OM_DiscCpny.AddObject(newComp);
                    }
                }
            //}
            //else
            //{
            //    var sysCom = (from p in _db.OM_DiscCpny
            //                  where p.DiscID.ToUpper() == discID
            //                      && p.DiscSeq.ToUpper() == discSeq 
            //                      && p.CpnyID == Current.CpnyID select p).FirstOrDefault();
            //    if (sysCom == null)
            //    {
            //        OM_DiscCpny newComp = new OM_DiscCpny();
            //        newComp.DiscID = inputDisc.DiscID;
            //        newComp.DiscSeq = inputSeq.DiscSeq;
            //        newComp.CpnyID = Current.CpnyID;

            //        _db.OM_DiscCpny.AddObject(newComp);
            //        if (lstCompany.Count == 0)
            //        {
            //            lstCompany.Add(new OM21100_pgCompany_Result() { 
            //                DiscID = inputDisc.DiscID,
            //                DiscSeq = inputSeq.DiscSeq,
            //                CpnyID = Current.CpnyID 
            //            });
            //        }
            //    }
            //}

            var cpnyHandler = new StoreDataHandler(data["lstCompanyChange"]);
            var lstCompanyChange = cpnyHandler.BatchObjectData<OM21100_pgCompany_Result>();
            foreach (var deleted in lstCompanyChange.Deleted)
            {
                if (!lstCompany.Any(x => x.CpnyID == deleted.CpnyID))
                {
                    var deletedCpny = _db.OM_DiscCpny.FirstOrDefault(p => p.DiscID == inputDisc.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.CpnyID == deleted.CpnyID);
                    if (deletedCpny != null)
                    {
                        _db.OM_DiscCpny.DeleteObject(deletedCpny);
                    }
                }                
            }

            //if (handle != "N" && handle != null && (roles.Any(c => c.ToUpper() == inputSeq.Crtd_Role)
            //    || roles.Any(c => c.ToUpper() == inputDisc.Crtd_Role.ToUpper()) // khong hieu cai role
            //    || (inputSeq.Crtd_Role.PassNull() == "SUBDIST"
            //    && roles.Any(c => c.ToUpper() == "DIST"))))
            //    Save_Task(data, lstCompany, seq, inputSeq, handle);
            //else
                Save_Break(data, null, null, inputSeq);
        }

        private void Save_Task(FormCollection data, List<OM21100_pgCompany_Result> lstCompany, OM_DiscSeq seq, OM_DiscSeq inputSeq, string cboHandle)
        {
            string branches = string.Empty;
            foreach (var cpny in lstCompany)
            {
                branches += cpny.CpnyID + ',';
            }
            if (branches.Length > 0) branches = branches.Substring(0, branches.Length - 1);
            var handle = (from p in _db.SI_ApprovalFlowHandle where p.AppFolID == _screenNbr 
                              && p.Status == inputSeq.Status
                              && p.Handle == cboHandle
                          select p).FirstOrDefault();
            if (handle != null && handle.Param03.PassNull().Split(',').Any(c => c.ToLower() == "many"))
            {
                foreach (var branch in branches.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var dataTask = (from p in _db.HO_PendingTasks
                                where p.ObjectID == inputSeq.DiscID + "-" + inputSeq.DiscSeq 
                                    && p.EditScreenNbr == _screenNbr
                                    && p.BranchID == branch
                                select p).FirstOrDefault();
                    if (dataTask == null && handle != null)
                    {
                        if (!handle.Param00.PassNull().Split(',').Any(c => c.ToLower() == "notapprove"))
                        {
                            HO_PendingTasks newTask = new HO_PendingTasks();
                            newTask.BranchID = branch;
                            newTask.ObjectID = inputSeq.DiscID + "-" + inputSeq.DiscSeq;
                            newTask.EditScreenNbr = _screenNbr;
                            newTask.Content = string.Format(handle.ContentApprove, inputSeq.DiscID + "-" + inputSeq.DiscSeq, inputSeq.Descr, branch);
                            newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                            newTask.Crtd_Prog = newTask.LUpd_Prog = _screenNbr;
                            newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                            newTask.Status = handle.ToStatus;
                            newTask.tstamp = new byte[1];
                            _db.HO_PendingTasks.AddObject(newTask);
                        }
                        seq.Status = handle.ToStatus;
                    }
                }

            }
            else
            {
                var dataTask = (from p in _db.HO_PendingTasks
                            where p.ObjectID == inputSeq.DiscID + "-" + inputSeq.DiscSeq && p.EditScreenNbr == _screenNbr
                                && p.BranchID == branches
                            select p).FirstOrDefault();
                if (dataTask == null && handle != null)
                {
                    if (!handle.Param00.PassNull().Split(',').Any(c => c.ToLower() == "notapprove"))
                    {
                        HO_PendingTasks newTask = new HO_PendingTasks();
                        newTask.BranchID = branches;
                        newTask.ObjectID = inputSeq.DiscID + "-" + inputSeq.DiscSeq;
                        newTask.EditScreenNbr = _screenNbr;
                        newTask.Content = string.Format(handle.ContentApprove, inputSeq.DiscID + "-" + inputSeq.DiscSeq, inputSeq.Descr, branches);
                        newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                        newTask.Crtd_Prog = newTask.LUpd_Prog = _screenNbr;
                        newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                        newTask.Status = handle.ToStatus;
                        newTask.tstamp = new byte[1];
                        _db.HO_PendingTasks.AddObject(newTask);
                    }
                    seq.Status = handle.ToStatus;
                }
            }
            Save_Break(data, handle, branches, inputSeq);
        }

        private void Save_Break(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discBreakHandler = new StoreDataHandler(data["lstDiscBreak"]);
            var lstDiscBreak = discBreakHandler.ObjectData<OM21100_pgDiscBreak_Result>()
                        .Where(p => Util.PassNull(p.LineRef) != string.Empty
                            && (p.BreakAmt > 0 || p.BreakQty > 0 || p.DiscAmt > 0))
                        .ToList();

            var discBreakChangeHandler = new StoreDataHandler(data["lstDiscBreakChange"]);
            var lstDiscBreakChange = discBreakChangeHandler.BatchObjectData<OM21100_pgDiscBreak_Result>();

            foreach (var currentBreak in lstDiscBreak)
            {
                var discBreak = (from p in _db.OM_DiscBreak
                                 where
                                     p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                     p.LineRef == currentBreak.LineRef
                                 select p).FirstOrDefault();
                if (discBreak != null)
                {
                    Update_Break(discBreak, currentBreak, false);
                }
                else
                {
                    OM_DiscBreak newBreak = new OM_DiscBreak();
                    Update_Break(newBreak, currentBreak, true);
                    _db.OM_DiscBreak.AddObject(newBreak);
                }

            }

            // Xoa cac item khong con tren luoi
            foreach (var deleted in lstDiscBreakChange.Deleted)
            {
                var deletedDiscBreak = _db.OM_DiscBreak.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                                        && p.DiscSeq == inputSeq.DiscSeq && p.LineRef == deleted.LineRef);
                if (deletedDiscBreak != null)
                {
                    _db.OM_DiscBreak.DeleteObject(deletedDiscBreak);
                    // Xoa free item
                    var lstDeletedFreeItem = _db.OM_DiscFreeItem.Where(p => p.DiscID == inputSeq.DiscID
                                        && p.DiscSeq == inputSeq.DiscSeq && p.LineRef == deleted.LineRef).ToList();
                    foreach (var deletedFI in lstDeletedFreeItem)
                    {
                        _db.OM_DiscFreeItem.DeleteObject(deletedFI);
                    }
                }
            }

            Save_FreeItem(data, handle, branches, inputSeq);
        }

        private void Update_Break(OM_DiscBreak t, OM21100_pgDiscBreak_Result s, bool isNew)
        {
            try
            {
                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.LineRef = s.LineRef;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }
                t.MaxLot = s.MaxLot;
                t.BreakAmt = s.BreakAmt;
                t.BreakQty = s.BreakQty;
                t.DiscAmt = s.DiscAmt;
                t.Descr = s.Descr;

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Save_FreeItem(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var freeItemChangeHandler = new StoreDataHandler(data["lstFreeItemChange"]);
            var lstFreeItemChange = freeItemChangeHandler.BatchObjectData<OM21100_pgFreeItem_Result>();

            foreach (var currentFree in lstFreeItemChange.Created)
            {
                var free = (from p in _db.OM_DiscFreeItem
                            where
                                p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                p.LineRef == currentFree.LineRef && p.FreeItemID == currentFree.FreeItemID
                            select p).FirstOrDefault();
                if (free != null)
                {
                    Update_FreeItem(free, currentFree, false);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(currentFree.FreeItemID))
                    {
                        OM_DiscFreeItem newFree = new OM_DiscFreeItem();
                        Update_FreeItem(newFree, currentFree, true);
                        _db.OM_DiscFreeItem.AddObject(newFree);
                    }
                }

            }
            foreach (var currentFree in lstFreeItemChange.Updated)
            {
                var free = (from p in _db.OM_DiscFreeItem
                            where
                                p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                p.LineRef == currentFree.LineRef && p.FreeItemID == currentFree.FreeItemID
                            select p).FirstOrDefault();
                if (free != null)
                {
                    Update_FreeItem(free, currentFree, false);
                }
                else
                {
                    OM_DiscFreeItem newFree = new OM_DiscFreeItem();
                    Update_FreeItem(newFree, currentFree, true);
                    _db.OM_DiscFreeItem.AddObject(newFree);
                }

            }

            foreach (var currentFree in lstFreeItemChange.Deleted)
            {
                var free = (from p in _db.OM_DiscFreeItem
                            where
                                p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                p.LineRef == currentFree.LineRef && p.FreeItemID == currentFree.FreeItemID
                            select p).FirstOrDefault();
                if (free != null)
                    _db.OM_DiscFreeItem.DeleteObject(free);
            }
            if (inputSeq.DiscClass == "II")
                Save_DiscItem(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "BB")
                Save_Bundle(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TT")
                Save_DiscCustClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CC")
                Save_DiscCust(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "PP")
                Save_DiscItemClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CI")
                Save_DiscItem(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TI")
                Save_DiscItem(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TP")
                Save_DiscItemClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TB")
                Save_Bundle(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CB")
                Save_Bundle(data, handle, branches, inputSeq);

            else if (inputSeq.DiscClass == Channel)
            {
                Save_DiscChannel(data, handle, branches, inputSeq);
            }
            else if (inputSeq.DiscClass == CustCate)
            {
                Save_DiscCustCate(data, handle, branches, inputSeq);
            }
            else if (inputSeq.DiscClass == ItemChannel)
            {
                Save_DiscItem(data, handle, branches, inputSeq);
            }
            else if (inputSeq.DiscClass == ItemCustCate)
            {
                Save_DiscItem(data, handle, branches, inputSeq);
            }
            else if (inputSeq.DiscClass == GItemChannel)
            {
                Save_DiscItemClass(data, handle, branches, inputSeq);
            }            
            else if (inputSeq.DiscClass == GItemCustCate)
            {
                Save_DiscItemClass(data, handle, branches, inputSeq);
            }
        }

        private void Save_DiscCustClass(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discCustClassHandler = new StoreDataHandler(data["lstDiscCustClass"]);
            var lstDiscCustClass = discCustClassHandler.ObjectData<OM21100_pgDiscCustClass_Result>()
                        .Where(p => Util.PassNull(p.ClassID) != string.Empty)
                        .ToList();

            var discCustClassChangeHandler = new StoreDataHandler(data["lstDiscCustClassChange"]);
            var lstDiscCustClassChange = discCustClassChangeHandler.BatchObjectData<OM21100_pgDiscCustClass_Result>();

            foreach (var currentCustClass in lstDiscCustClass)
            {
                var custClass = (from p in _db.OM_DiscCustClass
                                 where
                                     p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                     p.ClassID == currentCustClass.ClassID
                                 select p).FirstOrDefault();
                if (custClass != null)
                {
                    Update_DiscCustClass(custClass, currentCustClass, false);
                }
                else
                {
                    OM_DiscCustClass newCustClass = new OM_DiscCustClass();
                    Update_DiscCustClass(newCustClass, currentCustClass, true);
                    _db.OM_DiscCustClass.AddObject(newCustClass);
                }
            }

            foreach (var deleted in lstDiscCustClassChange.Deleted)
            {
                var deletedDiscItem = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.InvtID == deleted.ClassID);
                if (deletedDiscItem != null)
                {
                    _db.OM_DiscItem.DeleteObject(deletedDiscItem);
                }
            }

            if (inputSeq.DiscClass == "TT")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TI")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TP")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TB")
                Submit_Data(handle, branches, inputSeq);
        }

        private void Submit_Data(SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            _db.SaveChanges();
        }

        private void Save_DiscItem(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discType = data["cboDiscType"];
            var discItemHandler = new StoreDataHandler(data["lstDiscItem"]);
            var lstDiscItem = discItemHandler.ObjectData<OM21100_pgDiscItem_Result>()
                        .Where(p => Util.PassNull(p.InvtID) != string.Empty)
                        .ToList();

            var discItemChangeHandler = new StoreDataHandler(data["lstDiscItemChange"]);
            var lstDiscItemChange = discItemChangeHandler.BatchObjectData<OM21100_pgDiscItem_Result>();

            foreach (var deleted in lstDiscItemChange.Deleted)
            {
                if (!lstDiscItem.Any(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.InvtID == deleted.InvtID))
                {
                    var deletedDiscItem = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.InvtID == deleted.InvtID);
                    if (deletedDiscItem != null)
                    {
                        _db.OM_DiscItem.DeleteObject(deletedDiscItem);
                    }
                }
                
            }

            var lstDel = _db.OM_DiscItem.Where(p => p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstDiscItem.FirstOrDefault(p => p.DiscID == lstDel[i].DiscID
                                && p.DiscSeq == lstDel[i].DiscSeq
                                && p.InvtID == lstDel[i].InvtID);
                if (objDel == null)
                {
                    _db.OM_DiscItem.DeleteObject(lstDel[i]);
                }
            }

            foreach (var currentItem in lstDiscItem)
            {
                var discItem = (from p in _db.OM_DiscItem
                                where
                                    p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                    p.InvtID == currentItem.InvtID
                                select p).FirstOrDefault();
                if (discItem != null)
                {
                    currentItem.DiscType = discType;
                    Update_DiscItem(discItem, currentItem, false);
                }
                else
                {
                    OM_DiscItem newItem = new OM_DiscItem();
                    currentItem.DiscType = discType;
                    Update_DiscItem(newItem, currentItem, true);
                    _db.OM_DiscItem.AddObject(newItem);
                }
            }            

            if (inputSeq.DiscClass == "II")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CI")
                Save_DiscCust(data,handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TI")
                Save_DiscCustClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == ItemChannel)
            {
                Save_DiscChannel(data, handle, branches, inputSeq);
            }
            else if (inputSeq.DiscClass == ItemCustCate)
            {
                Save_DiscCustCate(data, handle, branches, inputSeq);
            }
        }

        private void Save_DiscCust(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discCustHandler = new StoreDataHandler(data["lstDiscCust"]);
            var lstDiscCust = discCustHandler.ObjectData<OM21100_pgDiscCust_Result>()
                        .Where(p => Util.PassNull(p.CustID) != string.Empty)
                        .ToList();

            var discCustChangeHandler = new StoreDataHandler(data["lstDiscCustChange"]);
            var lstDiscCustChange = discCustChangeHandler.BatchObjectData<OM21100_pgDiscCust_Result>();

            foreach (var deleted in lstDiscCustChange.Deleted)
            {
                if (!lstDiscCust.Any(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.CustID == deleted.CustID && p.BranchID == deleted.BranchID))
                {
                    var deletedDiscItem = _db.OM_DiscCust.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.CustID == deleted.CustID && p.BranchID == deleted.BranchID);
                    if (deletedDiscItem != null)
                    {
                        _db.OM_DiscCust.DeleteObject(deletedDiscItem);
                    }
                }                
            }

            var lstDel = _db.OM_DiscCust.Where(p => p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstDiscCust.FirstOrDefault(p => p.DiscID == lstDel[i].DiscID
                                && p.DiscSeq == lstDel[i].DiscSeq
                                && p.CustID == lstDel[i].CustID
                                && p.BranchID == lstDel[i].BranchID);
                if (objDel == null)
                {
                    _db.OM_DiscCust.DeleteObject(lstDel[i]);
                }
            }
            foreach (var currentCust in lstDiscCust)
            {
                var cust = (from p in _db.OM_DiscCust
                            where
                                p.DiscID == inputSeq.DiscID 
                                && p.DiscSeq == inputSeq.DiscSeq 
                                && p.CustID == currentCust.CustID
                                && p.BranchID == currentCust.BranchID
                            select p).FirstOrDefault();
                if (cust == null)
                {
                    OM_DiscCust newCust = new OM_DiscCust();
                    Update_DiscCust(newCust, currentCust, true);
                    _db.OM_DiscCust.AddObject(newCust);
                }                                   
                //else
                //{
                //    Update_DiscCust(cust, currentCust, false);
                //}
            }

            

            if (inputSeq.DiscClass == "CC")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CI")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CB")
                Submit_Data(handle, branches, inputSeq);
        }

        private void Update_DiscCust(OM_DiscCust t, OM21100_pgDiscCust_Result s, bool isNew)
        {
            try
            {
                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.CustID = s.CustID;
                    t.BranchID = s.BranchID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Save_DiscItemClass(FormCollection data,SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discItemClassHandler = new StoreDataHandler(data["lstDiscItemClass"]);
            var lstDiscItemClass = discItemClassHandler.ObjectData<OM21100_pgDiscItemClass_Result>()
                        .Where(p => Util.PassNull(p.ClassID) != string.Empty)
                        .ToList();

            var discItemClassChangeHandler = new StoreDataHandler(data["lstDiscItemClassChange"]);
            var lstDiscItemClassChange = discItemClassChangeHandler.BatchObjectData<OM21100_pgDiscItemClass_Result>();

            foreach (var currentItemClass in lstDiscItemClass)
            {
                var itemClass = (from p in _db.OM_DiscItemClass
                                 where
                                     p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                     p.ClassID == currentItemClass.ClassID
                                 select p).FirstOrDefault();
                if (itemClass != null)
                {
                    Update_DiscItemClass(itemClass, currentItemClass, false);
                }
                else
                {
                    OM_DiscItemClass newItemClass = new OM_DiscItemClass();
                    Update_DiscItemClass(newItemClass, currentItemClass, true);
                    _db.OM_DiscItemClass.AddObject(newItemClass);
                }
            }
            ////// TEST HERE
            foreach (var deleted in lstDiscItemClassChange.Deleted)
            {
                var deletedDiscItemClass = _db.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.ClassID == deleted.ClassID);
                if (deletedDiscItemClass != null)
                {
                    _db.OM_DiscItemClass.DeleteObject(deletedDiscItemClass);
                }
            }

            if (inputSeq.DiscClass == "PP")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TP")
                Save_DiscCustClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == GItemChannel)
            {
                Save_DiscChannel(data, handle, branches, inputSeq);
            }
            else if (inputSeq.DiscClass == GItemCustCate)
            {
                Save_DiscCustCate(data, handle, branches, inputSeq);
            }

        }        

        private void Save_Bundle(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discType = data["cboDiscType"];
            var discBundleHandler = new StoreDataHandler(data["lstBundle"]);
            var lstDiscBundle = discBundleHandler.ObjectData<OM21100_pgDiscBundle_Result>()
                        .Where(p => Util.PassNull(p.InvtID) != string.Empty)
                        .ToList();

            var discBundleChangeHandler = new StoreDataHandler(data["lstBundleChange"]);
            var lstBundleChange = discBundleChangeHandler.BatchObjectData<OM21100_pgDiscBundle_Result>();

            foreach (var deleted in lstBundleChange.Deleted)
            {
                if (!lstDiscBundle.Any(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.InvtID == deleted.InvtID))
                {
                    var deletedBundle = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.InvtID == deleted.InvtID);
                    if (deletedBundle != null)
                    {
                        _db.OM_DiscItem.DeleteObject(deletedBundle);
                    }
                }                
            }

            var lstDel = _db.OM_DiscItem.Where(p => p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstDiscBundle.FirstOrDefault(p => p.DiscID == lstDel[i].DiscID
                                && p.DiscSeq == lstDel[i].DiscSeq
                                && p.InvtID == lstDel[i].InvtID);
                if (objDel == null)
                {
                    _db.OM_DiscItem.DeleteObject(lstDel[i]);
                }
            }

            foreach (var currentBundle in lstDiscBundle)
            {
                var discBundle = (from p in _db.OM_DiscItem
                                  where
                                      p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                      p.InvtID == currentBundle.InvtID
                                  select p).FirstOrDefault();
                if (discBundle != null)
                {
                    currentBundle.DiscType = discType;
                    Update_Bundle(discBundle, currentBundle, false);
                }
                else
                {
                    OM_DiscItem newBundle = new OM_DiscItem();
                    currentBundle.DiscType = discType;
                    Update_Bundle(newBundle, currentBundle, true);
                    _db.OM_DiscItem.AddObject(newBundle);
                }
            }            

            if (inputSeq.DiscClass == "BB")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TB")
                Save_DiscCustClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CB")
                Save_DiscCust(data, handle, branches, inputSeq);

        }

        private void Save_DiscCustCate(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discItemHandler = new StoreDataHandler(data["lstDiscCustCate"]);
            var lstCustCate = discItemHandler.ObjectData<OM21100_pgDiscCustCate_Result>()
                        .Where(p => Util.PassNull(p.CustCateID) != string.Empty).ToList();

            var discItemChangeHandler = new StoreDataHandler(data["lstDiscCustCateChange"]);
            var lstCustCateChange = discItemChangeHandler.BatchObjectData<OM21100_pgDiscCustCate_Result>();

            foreach (var deleted in lstCustCateChange.Deleted)
            {
                if (!lstCustCate.Any(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.CustCateID == deleted.CustCateID))
                {
                    var deletedDiscItem = _db.OM_DiscCustCate.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.CustCateID == deleted.CustCateID);
                    if (deletedDiscItem != null)
                    {
                        _db.OM_DiscCustCate.DeleteObject(deletedDiscItem);
                    }
                }
            }

            foreach (var currentItem in lstCustCate)
            {
                var discItem = (from p in _db.OM_DiscCustCate
                                where
                                    p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                    p.CustCateID == currentItem.CustCateID
                                select p).FirstOrDefault();
                if (discItem == null)
                {
                    discItem = new OM_DiscCustCate();
                    discItem.DiscID = inputSeq.DiscID;
                    discItem.DiscSeq = inputSeq.DiscSeq;
                    discItem.CustCateID = currentItem.CustCateID;
                    discItem.LUpd_DateTime = DateTime.Now;
                    discItem.LUpd_Prog = _screenNbr;
                    discItem.LUpd_User = Current.UserName;
                    discItem.Crtd_DateTime = DateTime.Now;
                    discItem.Crtd_Prog = _screenNbr;
                    discItem.Crtd_User = Current.UserName;
                    discItem.tstamp = new byte[1];
                    _db.OM_DiscCustCate.AddObject(discItem);
                }
            }

            Submit_Data(handle, branches, inputSeq);
        }

        private void Save_DiscChannel(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discItemHandler = new StoreDataHandler(data["lstDiscChannel"]);
            var lstChannel = discItemHandler.ObjectData<OM21100_pgDiscChannel_Result>()
                        .Where(p => Util.PassNull(p.ChannelID) != string.Empty).ToList();

            var discItemChangeHandler = new StoreDataHandler(data["lstDiscChannelChange"]);
            var lstChannelChange = discItemChangeHandler.BatchObjectData<OM21100_pgDiscChannel_Result>();

            foreach (var deleted in lstChannelChange.Deleted)
            {
                if (!lstChannel.Any(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.ChannelID == deleted.ChannelID))
                {
                    var deletedDiscItem = _db.OM_DiscChannel.FirstOrDefault(p => p.DiscID == inputSeq.DiscID
                    && p.DiscSeq == inputSeq.DiscSeq && p.ChannelID == deleted.ChannelID);
                    if (deletedDiscItem != null)
                    {
                        _db.OM_DiscChannel.DeleteObject(deletedDiscItem);
                    }
                }
            }

            foreach (var currentItem in lstChannel)
            {
                var discItem = (from p in _db.OM_DiscChannel
                                where
                                    p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                    p.ChannelID == currentItem.ChannelID
                                select p).FirstOrDefault();
                if (discItem == null)
                {
                    discItem = new OM_DiscChannel();
                    discItem.DiscID = inputSeq.DiscID;
                    discItem.DiscSeq = inputSeq.DiscSeq;
                    discItem.ChannelID = currentItem.ChannelID;
                    discItem.LUpd_DateTime = DateTime.Now;
                    discItem.LUpd_Prog = _screenNbr;
                    discItem.LUpd_User = Current.UserName;
                    discItem.Crtd_DateTime = DateTime.Now;
                    discItem.Crtd_Prog = _screenNbr;
                    discItem.Crtd_User = Current.UserName;
                    discItem.tstamp = new byte[1];
                    _db.OM_DiscChannel.AddObject(discItem);
                }
            }

            Submit_Data(handle, branches, inputSeq);
        }
        private void Update_DiscItemClass(OM_DiscItemClass t, OM21100_pgDiscItemClass_Result s, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.ClassID = s.ClassID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }
                t.UnitDesc = s.UnitDesc;
                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Update_DiscItem(OM_DiscItem t, OM21100_pgDiscItem_Result s, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.InvtID = s.InvtID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }
                t.UnitDesc = s.UnitDesc;
                t.BundleAmt = 0;
                t.BundleNbr = 0;
                t.BundleOrItem = "I";
                t.BundleQty = 0;
                t.DiscType = s.DiscType;


                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Update_Bundle(OM_DiscItem t, OM21100_pgDiscBundle_Result s, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.InvtID = s.InvtID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }
                t.BundleAmt = s.BundleAmt;
                t.BundleNbr = s.BundleNbr;
                t.BundleOrItem = "B";
                t.BundleQty = s.BundleQty;
                t.DiscType = s.DiscType;
                t.UnitDesc = s.UnitDesc;

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Update_FreeItem(OM_DiscFreeItem t, OM21100_pgFreeItem_Result s, bool isNew)
        {
            try
            {
                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.LineRef = s.LineRef;
                    t.FreeItemID = s.FreeItemID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }

                t.FreeITemSiteID = s.FreeITemSiteID;
                t.FreeItemBudgetID = s.FreeItemBudgetID;
                t.FreeItemQty = s.FreeItemQty;
                t.UnitDescr = s.UnitDescr;

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }        

        private void Update_DiscCustClass(OM_DiscCustClass t, OM21100_pgDiscCustClass_Result s, bool isNew)
        {
            try
            {
                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.ClassID = s.ClassID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateDiscSeq(ref OM_DiscSeq updatedDiscSeq, OM_DiscSeq inputDiscSeq, bool isNew, string[] roles, string handle)
        {
            if (isNew)
            {
                updatedDiscSeq.ResetET();
                updatedDiscSeq.DiscID = inputDiscSeq.DiscID;
                updatedDiscSeq.DiscSeq = inputDiscSeq.DiscSeq;
                updatedDiscSeq.Crtd_DateTime = DateTime.Now;
                updatedDiscSeq.Crtd_Prog = _screenNbr;
                updatedDiscSeq.Crtd_User = Current.UserName;
                updatedDiscSeq.tstamp = new byte[1];
                updatedDiscSeq.Status = "H";
            }
            if (updatedDiscSeq.Crtd_Role.PassNull() == string.Empty)
            {
                updatedDiscSeq.Crtd_Role = roles.Contains("HO") ? "HO" : roles.Contains("DIST") ? "DIST" : roles.Contains("SUBDIST") ? "SUBDIST" : string.Empty;
            }
            updatedDiscSeq.DiscFor = inputDiscSeq.DiscFor;
            updatedDiscSeq.DiscClass = inputDiscSeq.DiscClass;
            updatedDiscSeq.EndDate = inputDiscSeq.EndDate.ToDateShort();
            updatedDiscSeq.StartDate = inputDiscSeq.StartDate.ToDateShort();
            updatedDiscSeq.Active = inputDiscSeq.Active;
            updatedDiscSeq.AllowEditDisc = inputDiscSeq.AllowEditDisc;
            updatedDiscSeq.AutoFreeItem = inputDiscSeq.AutoFreeItem;
            updatedDiscSeq.BreakBy = inputDiscSeq.BreakBy;
            updatedDiscSeq.BudgetID = inputDiscSeq.BudgetID;
            updatedDiscSeq.Descr = inputDiscSeq.Descr;
            updatedDiscSeq.ProAplForItem = inputDiscSeq.ProAplForItem;
            updatedDiscSeq.Promo = inputDiscSeq.Promo;
            updatedDiscSeq.POUse = inputDiscSeq.POUse;
            updatedDiscSeq.POEndDate = inputDiscSeq.POEndDate.ToDateShort();
            updatedDiscSeq.POStartDate = inputDiscSeq.POStartDate.ToDateShort();
            updatedDiscSeq.ExactQty = inputDiscSeq.ExactQty;
            updatedDiscSeq.ExcludeOtherDisc = inputDiscSeq.ExcludeOtherDisc;
            if (!string.IsNullOrEmpty(handle) && handle != "N" && updatedDiscSeq.Status != handle)
            {
                updatedDiscSeq.Status = handle;
            }

            updatedDiscSeq.LUpd_DateTime = DateTime.Now;
            updatedDiscSeq.LUpd_Prog = _screenNbr;
            updatedDiscSeq.LUpd_User = Current.UserName;
        }

        private void updateDiscount(ref OM_Discount updatedDiscount, OM_Discount inputedDiscount, bool isNew, string[] roles)
        {
            if (isNew)
            {
                updatedDiscount.DiscID = inputedDiscount.DiscID;
                updatedDiscount.Crtd_DateTime = DateTime.Now;
                updatedDiscount.Crtd_Prog = _screenNbr;
                //updatedDiscount.Crtd_Role = Current.UserName;
                updatedDiscount.Crtd_User = Current.UserName;
            }

            updatedDiscount.Descr = inputedDiscount.Descr;
            updatedDiscount.DiscType = inputedDiscount.DiscType;
            updatedDiscount.DiscClass = inputedDiscount.DiscClass;

            if (updatedDiscount.Crtd_Role.PassNull() == string.Empty)
                updatedDiscount.Crtd_Role = roles.Contains("HO") ? "HO" : roles.Contains("DIST") ? "DIST" : roles.Contains("SUBDIST") ? "SUBDIST" : string.Empty;

            updatedDiscount.LUpd_Prog = _screenNbr;
            updatedDiscount.LUpd_DateTime = DateTime.Now;
            //updatedDiscount.Crtd_Role = Current.UserName;
            updatedDiscount.LUpd_User = Current.UserName;
        }

        private Node createNode(Node root, OM21100_pdSI_Hierarchy_Result inactiveHierachy, int level, string nodeType)
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

            var childrenInactiveHierachies = _lstSI_HierarchyI
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.NodeLevel == level).ToList();

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (OM21100_pdSI_Hierarchy_Result childrenInactiveNode in childrenInactiveHierachies)
                {
                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, nodeType));
                }
            }
            else
            {
                if (childrenInactiveHierachies.Count == 0)
                {
                    var invts = _lstPtInventory.Where(i => i.NodeID == inactiveHierachy.NodeID && i.NodeLevel == inactiveHierachy.NodeLevel && i.ParentRecordID == inactiveHierachy.ParentRecordID).ToList();
                    //var invts = _db.OM21100_ptInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList().Where(i => i.NodeID == inactiveHierachy.NodeID && i.NodeLevel == inactiveHierachy.NodeLevel && i.ParentRecordID == inactiveHierachy.ParentRecordID).ToList();
                    if (invts.Count > 0)
                    {
                        foreach (var invt in invts)
                        {
                            Node invtNode = new Node();

                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Invt", Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "InvtID", Value = invt.InvtID, Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = invt.Descr, Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "CnvFact", Value = invt.CnvFact.ToString(), Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "InvtType", Value = invt.InvtType, Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Unit", Value = invt.Unit, Mode = ParameterMode.Value });
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


        #region -Profile-
        
       
        private string GetFilePath()
        {
            var objConfig = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "OM21100PROFILEPATH");
            if (objConfig != null && !string.IsNullOrWhiteSpace(objConfig.TextVal))
            {
                return objConfig.TextVal;
            }
            return Server.MapPath("~\\Images\\");
        }

        public ActionResult ImageToBin(string fileName)
        {
            try
            {
                var imgString64 = Util.ImageToBin(GetFilePath(), fileName);
                var jsonResult = Json(new { success = true, imgSrc = imgString64 }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
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
  

        #region -Export-
        [HttpPost]
        public ActionResult Export(FormCollection data, string templateExport)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                string filename = string.Empty;
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = "Chương trình KM";
                workbook.Worksheets.Add();
                Worksheet SheetData1 = workbook.Worksheets[1];
                SheetData1.Name = "Điều Kiện";
                workbook.Worksheets.Add();
                Worksheet SheetData2 = workbook.Worksheets[2];
                SheetData2.Name = "Sản phẩm bán";
                //Chương trình Khuyến mãi
                SetCellValueGrid(SheetData.Cells["A1"], Util.GetLang("DiscID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["B1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["C1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["D1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["E1"], Util.GetLang("StartDate") + " \r\n" + " (dd/MM/yyyy)", TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["F1"], Util.GetLang("EndDate") + " \r\n" + " (dd/MM/yyyy)", TextAlignmentType.Center, TextAlignmentType.Left);

                SheetData.Cells.SetRowHeight(0, 45);
           
                Style colStyle = SheetData.Cells.Columns[2].Style;
                Style colStyle1 = SheetData.Cells.Columns[3].Style;
                StyleFlag flag = new StyleFlag();
                flag.NumberFormat = true;
                //Set the formating on the as text formating 
                colStyle.Number = 49;
                colStyle1.Number = 49;
                SheetData.Cells.Columns[0].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[4].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[5].ApplyStyle(colStyle, flag);
                SheetData.Cells.SetColumnWidth(0, 15);
                SheetData.Cells.SetColumnWidth(1, 25);
                SheetData.Cells.SetColumnWidth(2, 15);
                SheetData.Cells.SetColumnWidth(3, 25);
                SheetData.Cells.SetColumnWidth(4, 15);
                SheetData.Cells.SetColumnWidth(5, 15);
                var style = SheetData.Cells["E2"].GetStyle();
                style.Custom = "dd/MM/yyyy";
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Left;
                var range = SheetData.Cells.CreateRange("E2", "E1000");
                range.SetStyle(style);
                range = SheetData.Cells.CreateRange("F2", "F1000");
                range.SetStyle(style);
                int commentIndex = SheetData.Comments.Add("N3");
                Comment comment = SheetData.Comments[commentIndex];
                CellArea position = new CellArea();
                position.StartRow = 3;
                position.EndRow = 1000;
                position.StartColumn = 5;
                position.EndColumn = 5;

                SheetData1.Cells.Columns[0].ApplyStyle(colStyle, flag);
                SheetData1.Cells.Columns[1].ApplyStyle(colStyle, flag);
                SheetData1.Cells.Columns[2].ApplyStyle(colStyle, flag);
                SheetData1.Cells.Columns[3].ApplyStyle(colStyle, flag);
                SheetData1.Cells.SetColumnWidth(0, 15);
                SheetData1.Cells.SetColumnWidth(1, 10);
                SheetData1.Cells.SetColumnWidth(2, 15);
                SheetData1.Cells.SetColumnWidth(3, 15);

                ////////////////////////////GIIQA///////////////////////////////////////
                if (templateExport.ToUpper().Trim()== "GIIQA")
                {
                    filename = "GIIQA.xlsx";                    
                    workbook.Worksheets.Add();
                    Worksheet SheetData3 = workbook.Worksheets[3];
                    SheetData3.Name = "NPP áp dụng";                
                    //Điều Kiện
                    SetCellValueGrid(SheetData1.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["C1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["D1"], Util.GetLang("Discount"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["E1"], Util.GetLang("OM21100MaxLot"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["F1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);

                    SheetData1.Cells.SetRowHeight(0, 45);
                    SheetData1.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData1.Cells.Columns[5].ApplyStyle(colStyle, flag);                    
                    SheetData1.Cells.SetColumnWidth(4, 15);
                    SheetData1.Cells.SetColumnWidth(5, 20);
                    //Sản phẩm bán
                    SetCellValueGrid(SheetData2.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["B1"], Util.GetLang("InvtID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["C1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData2.Cells.SetRowHeight(0, 45);
                    SheetData2.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[3].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.SetColumnWidth(0, 15);
                    SheetData2.Cells.SetColumnWidth(1, 15);
                    SheetData2.Cells.SetColumnWidth(2, 15);
                    //NPP áp dụng
                    SetCellValueGrid(SheetData3.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["B1"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["C1"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData3.Cells.SetRowHeight(0, 45);
                    SheetData3.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[3].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.SetColumnWidth(0, 15);
                    SheetData3.Cells.SetColumnWidth(1, 15);
                    SheetData3.Cells.SetColumnWidth(2, 25);

                }


                //////////////////////////////////GIIQF///////////////////////////////////////////////////////
                if (templateExport.ToUpper().Trim() == "GIIQF")
                {
                    filename = "GIIQF.xlsx";
                    workbook.Worksheets.Add();
                    Worksheet SheetData3 = workbook.Worksheets[3]; 
                    SheetData3.Name = "Sản phẩm tặng";
                    workbook.Worksheets.Add();
                    Worksheet SheetData4 = workbook.Worksheets[4]; 
                    SheetData4.Name = "NPP áp dụng";                    
                    //Điều Kiện
                    SetCellValueGrid(SheetData1.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["C1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["D1"], Util.GetLang("OM21100MaxLot"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["E1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData1.Cells.SetRowHeight(0, 45);
                    SheetData1.Cells.Columns[4].ApplyStyle(colStyle, flag);                    
                    SheetData1.Cells.SetColumnWidth(4, 20);
                    //Sản phẩm bán
                    SetCellValueGrid(SheetData2.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["B1"], Util.GetLang("InvtID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["C1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData2.Cells.SetRowHeight(0, 45);
                    SheetData2.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.SetColumnWidth(0, 15);
                    SheetData2.Cells.SetColumnWidth(1, 15);
                    SheetData2.Cells.SetColumnWidth(2, 15);
                    //Sản phẩm tặng
                    SetCellValueGrid(SheetData3.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["C1"], Util.GetLang("FreeItemID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["D1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["E1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData3.Cells.SetRowHeight(0, 45);
                    SheetData3.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[3].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.SetColumnWidth(0, 15);
                    SheetData3.Cells.SetColumnWidth(1, 15);
                    SheetData3.Cells.SetColumnWidth(2, 15);
                    SheetData3.Cells.SetColumnWidth(3, 15);
                    SheetData3.Cells.SetColumnWidth(4, 15);
                    //NPP áp dụng
                    SetCellValueGrid(SheetData4.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData4.Cells["B1"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData4.Cells["C1"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData4.Cells.SetRowHeight(0, 45);
                    SheetData4.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData4.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData4.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData4.Cells.SetColumnWidth(0, 15);
                    SheetData4.Cells.SetColumnWidth(1, 15);
                    SheetData4.Cells.SetColumnWidth(2, 25);
                }

                //////////////////////GIIQP//////////////////////////////
                if (templateExport.ToUpper().Trim() == "GIIQP")
                {
                    filename = "GIIQP.xlsx";
                    workbook.Worksheets.Add();
                    Worksheet SheetData3 = workbook.Worksheets[3];
                    SheetData3.Name = "NPP áp dụng";                   
                    //Điều Kiện
                    SetCellValueGrid(SheetData1.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["C1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["D1"], Util.GetLang("Discount"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["E1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData1.Cells.SetRowHeight(0, 45);
                    SheetData1.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData1.Cells.SetColumnWidth(4, 15);
                    //Sản phẩm bán
                    SetCellValueGrid(SheetData2.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["B1"], Util.GetLang("InvtID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["C1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData2.Cells.SetRowHeight(0, 45);
                    SheetData2.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.SetColumnWidth(0, 15);
                    SheetData2.Cells.SetColumnWidth(1, 15);
                    SheetData2.Cells.SetColumnWidth(2, 15);
                    //NPP áp dụng
                    SetCellValueGrid(SheetData3.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["B1"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["C1"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData3.Cells.SetRowHeight(0, 45);
                    SheetData3.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.SetColumnWidth(0, 15);
                    SheetData3.Cells.SetColumnWidth(1, 15);
                    SheetData3.Cells.SetColumnWidth(2, 25);
                }


                ///////////////////////LIIQA///////////////////////////
                if (templateExport.ToUpper().Trim() == "LIIQA")
                {
                    filename = "LIIQA.xlsx";
                    workbook.Worksheets.Add();
                    Worksheet SheetData3 = workbook.Worksheets[3];
                    SheetData3.Name = "NPP áp dụng";
                    
                    //Điều Kiện
                    SetCellValueGrid(SheetData1.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["C1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["D1"], Util.GetLang("Discount"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["E1"], Util.GetLang("OM21100MaxLot"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["F1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData1.Cells.SetRowHeight(0, 45);
                    SheetData1.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData1.Cells.Columns[5].ApplyStyle(colStyle, flag);
                    SheetData1.Cells.SetColumnWidth(4, 15);
                    SheetData1.Cells.SetColumnWidth(5, 20);

                    //Sản phẩm bán
                    SetCellValueGrid(SheetData2.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["B1"], Util.GetLang("InvtID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["C1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData2.Cells.SetRowHeight(0, 45);
                    SheetData2.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.SetColumnWidth(0, 15);
                    SheetData2.Cells.SetColumnWidth(1, 15);
                    SheetData2.Cells.SetColumnWidth(2, 15);
                    //NPP áp dụng
                    SetCellValueGrid(SheetData3.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["B1"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["C1"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData3.Cells.SetRowHeight(0, 45);
                    SheetData3.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.SetColumnWidth(0, 15);
                    SheetData3.Cells.SetColumnWidth(1, 25);
                    SheetData3.Cells.SetColumnWidth(2, 25);
                }
                /////////////////////////////////LIIQF///////////////////////
                if (templateExport.ToUpper().Trim() == "LIIQF")
                {
                    filename = "LIIQF.xlsx";
                    workbook.Worksheets.Add();
                    Worksheet SheetData3 = workbook.Worksheets[3];
                    SheetData3.Name = "Sản phẩm tặng";
                    workbook.Worksheets.Add();
                    Worksheet SheetData4 = workbook.Worksheets[4];
                    SheetData4.Name = "NPP áp dụng";
                    
                    //Điều Kiện
                    SetCellValueGrid(SheetData1.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["C1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["D1"], Util.GetLang("OM21100MaxLot"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["E1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData1.Cells.SetRowHeight(0, 45);
                    SheetData1.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData1.Cells.SetColumnWidth(4, 15);
                    //Sản phẩm bán
                    SetCellValueGrid(SheetData2.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["B1"], Util.GetLang("InvtID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["C1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData2.Cells.SetRowHeight(0, 45);
                    SheetData2.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.SetColumnWidth(0, 15);
                    SheetData2.Cells.SetColumnWidth(1, 15);
                    SheetData2.Cells.SetColumnWidth(2, 15);
                    //Sản phẩm tặng
                    SetCellValueGrid(SheetData3.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["C1"], Util.GetLang("FreeItemID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["D1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["E1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData3.Cells.SetRowHeight(0, 45);
                    SheetData3.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[3].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.SetColumnWidth(0, 15);
                    SheetData3.Cells.SetColumnWidth(1, 15);
                    SheetData3.Cells.SetColumnWidth(2, 15);
                    SheetData3.Cells.SetColumnWidth(3, 15);
                    SheetData3.Cells.SetColumnWidth(4, 15);
                    //NPP áp dụng
                    SetCellValueGrid(SheetData4.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData4.Cells["B1"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData4.Cells["C1"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData4.Cells.SetRowHeight(0, 45);
                    SheetData4.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData4.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData4.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData4.Cells.SetColumnWidth(0, 15);
                    SheetData4.Cells.SetColumnWidth(1, 15);
                    SheetData4.Cells.SetColumnWidth(2, 25);
                }


                ////////////////LIIQP////////////////////
                if (templateExport.ToUpper().Trim() == "LIIQP")
                {
                    filename = "LIIQP.xlsx";
                    workbook.Worksheets.Add();
                    Worksheet SheetData3 = workbook.Worksheets[3];
                    SheetData3.Name = "NPP áp dụng";
                   
                    //Điều Kiện
                    SetCellValueGrid(SheetData1.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["B1"], Util.GetLang("Level"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["C1"], Util.GetLang("BreakQty"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["D1"], Util.GetLang("Discount"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData1.Cells["E1"], Util.GetLang("Descr"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData1.Cells.SetRowHeight(0, 45);
                    SheetData1.Cells.Columns[4].ApplyStyle(colStyle, flag);
                    SheetData1.Cells.SetColumnWidth(4, 15);
                    //Sản phẩm bán
                    SetCellValueGrid(SheetData2.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["B1"], Util.GetLang("InvtID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData2.Cells["C1"], Util.GetLang("UOM"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData2.Cells.SetRowHeight(0, 45);
                    SheetData2.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData2.Cells.SetColumnWidth(0, 15);
                    SheetData2.Cells.SetColumnWidth(1, 15);
                    SheetData2.Cells.SetColumnWidth(2, 15);
                    //NPP áp dụng
                    SetCellValueGrid(SheetData3.Cells["A1"], Util.GetLang("DiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["B1"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SetCellValueGrid(SheetData3.Cells["C1"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Left);
                    SheetData3.Cells.SetRowHeight(0, 45);
                    SheetData3.Cells.Columns[0].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[1].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.Columns[2].ApplyStyle(colStyle, flag);
                    SheetData3.Cells.SetColumnWidth(0, 15);
                    SheetData3.Cells.SetColumnWidth(1, 25);
                    SheetData3.Cells.SetColumnWidth(2, 25);
                }    
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = filename.ToString() };

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
        private void SetCellValueGridHeader(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 20;
            style.Font.Color = Color.CornflowerBlue;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }
        private void SetCellValueGrid(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 11;
            style.Font.Color = Color.Black;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            style.IsTextWrapped = true;
            c.SetStyle(style);
        }


        public bool IsNumber(string pValue)
        {
            foreach (Char c in pValue)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }


        //Hàm chuyển chuổi có dấu thành không dấu
        public static string convertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }




        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                var access = Session["OM21100"] as AccessRight;

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                
                FileInfo fileInfo = new FileInfo(file.FileName);
                //List<AR_CustomerTD> lstAR_CustomerTD = new List<AR_CustomerTD>();
                string message = string.Empty;
   
                //string okStatus = string.Empty;
                List<OM_Discount> lstOM_Discount = new List<OM_Discount>();
                List<OM_DiscSeq> lstOM_DiscSeq = new List<OM_DiscSeq>();
                List<OM_DiscBreak> lstOM_DiscBreak = new List<OM_DiscBreak>();
                List<OM_DiscItem> lstOM_DiscItem = new List<OM_DiscItem>();
                List<OM_DiscFreeItem> lstOM_DiscFreeItem = new List<OM_DiscFreeItem>();
                List<OM_DiscCpny> lstOM_DiscCpny = new List<OM_DiscCpny>();
                

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    var fileImportName = file.FileName;
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);

                    string namesheet = "Sản phẩm tặng";
                    string namesheetNPP = "NPP áp dụng";
                    string workt_DiscID = string.Empty;
                    string workt_DiscIDDr = string.Empty;
                    string workt_DiscSeq = string.Empty;
                    string workt_DiscSeqDr = string.Empty;
                    string workt_StartDate = string.Empty;
                    string workt_EndDate = string.Empty;
                    string workt1_DiscSeq = string.Empty;
                    string workt1_Level = string.Empty;
                    string workt1_BreakQty = string.Empty;
                    string workt1_Discount = string.Empty;
                    string workt1_OM21100MaxLot = string.Empty;
                    string workt1_Descr = string.Empty;
                    string workt2_DiscSeq = string.Empty;
                    string workt2_InvtID = string.Empty;
                    string workt2_UOM = string.Empty;
                    string workt3_DiscSeq = string.Empty;
                    string workt3_BranchID = string.Empty;
                    Worksheet workSheet = workbook.Worksheets[0];
                    Worksheet workSheet1 = workbook.Worksheets[1];
                    Worksheet workSheet2 = workbook.Worksheets[2];
                    Worksheet workSheet3 = workbook.Worksheets[3];

                    /////////////////khai báo biến để bắt lỗi////////////////////
                    string errorDiscIDNull = string.Empty;
                    string errorDiscIDLength = string.Empty;
                    string errorDiscID = string.Empty;
                    string errorDiscIDDescr = string.Empty;
                    string errorDiscSeqNull = string.Empty;
                    string errorDiscSeqLength = string.Empty;
                    string errorStartDate = string.Empty;
                    string errorEndDate = string.Empty;
                    string errorDate = string.Empty;
                    string errorDiscSeq1Null = string.Empty;
                    string errorDiscSeq1 = string.Empty;
                    string errorLevelNull = string.Empty;
                    string errorLevelLength = string.Empty;
                    string errorLevel = string.Empty;
                    string errorworkt1_BreakQty = string.Empty;
                    string errorworkt1_Discount = string.Empty;
                    string errorworkt1_OM21100MaxLot = string.Empty;
                    string errorworkt1_Descr = string.Empty;
                    string errorworkt2_DiscSeqNull = string.Empty;
                    string errorworkt2_InvtIDNull = string.Empty;
                    string errorworkt2_DiscSep = string.Empty;
                    string errorworkt2_InvtIDLength = string.Empty;
                    string errorworkt2_UOMNull = string.Empty;
                    string errorworkt2_UOM = string.Empty;
                    string errorworkt3_DiscSeq = string.Empty;
                    string errorworkt3_BranchID = string.Empty;
                    string errorworkt3_DiscSeqNull = string.Empty;
                    string errorworkt3_BranchIDNull = string.Empty;
                    string errorDiscSeqFreeItemNull = string.Empty;
                    string errorDiscSeqFreeItem = string.Empty;
                    string errorFreeItemNull = string.Empty;
                    string errorFreeItem = string.Empty;
                    string errorLevelFreeItem = string.Empty;
                    string errorLevelFreeItemNull = string.Empty;
                    string errorLevelFreeItemLength = string.Empty;
                    string errorQtFreeItem = string.Empty;
                    string errorUOMFreeItem = string.Empty;
                    string errorUOMFreeItemNull = string.Empty;

                    bool flagCheck = false;
                    string okUOM = string.Empty;
                    var lstIN_UnitConversion = _db.OM21100_ppIN_UnitConversion(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                    var lstBrachID = _db.OM21100_ppCompanyCheck(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                    var lstDiscSeq = _db.OM21100_ppOM_DiscSeq(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                    var lstIN_Inventory = _db.OM21100_ppIN_Inventory(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                    
                    foreach (OM21100_ppIN_UnitConversion_Result r in lstIN_UnitConversion)
                    {
                        okUOM += r.FromUnit.ToString() + ", ";
                    }

                    if (fileImportName.ToUpper().Trim() == "GIIQA.XLSX" || fileImportName.ToUpper().Trim() == "GIIQA.XLS"
                        || fileImportName.ToUpper().Trim() == "GIIQF.XLSX" || fileImportName.ToUpper().Trim() == "GIIQF.XLS"
                        || fileImportName.ToUpper().Trim() == "GIIQP.XLSX" || fileImportName.ToUpper().Trim() == "GIIQP.XLS"
                        || fileImportName.ToUpper().Trim() == "LIIQA.XLSX" || fileImportName.ToUpper().Trim() == "LIIQA.XLS"
                        || fileImportName.ToUpper().Trim() == "LIIQF.XLSX" || fileImportName.ToUpper().Trim() == "LIIQF.XLS"
                        || fileImportName.ToUpper().Trim() == "LIIQP.XLSX" || fileImportName.ToUpper().Trim() == "LIIQP.XLS")
                    {



                        #region nếu là template GIIQA
                        if (fileImportName.ToUpper().Trim() == "GIIQA.XLSX" || fileImportName.ToUpper().Trim() == "GIIQA.XLS")
                        {

                            if (workbook.Worksheets.Count > 0)
                            {
                                #region kiểm tra template
                                // kiểm tra sheet Chương trình khuyến mãi
                                if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscID").ToUpper().Trim()
                                  || workSheet.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                  || workSheet.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 4].StringValue.ToUpper().Trim() != (Util.GetLang("StartDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  || workSheet.Cells[0, 5].StringValue.ToUpper().Trim() != (Util.GetLang("EndDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sheet Điều kiện
                                if (workSheet1.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet1.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet1.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 || workSheet1.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Discount").ToUpper().Trim()
                                 || workSheet1.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("OM21100MaxLot").ToUpper().Trim()
                                 || workSheet1.Cells[0, 5].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                // kiểm tra sản phẩm bán
                                if (workSheet2.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet2.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("InvtID").ToUpper().Trim()
                                 || workSheet2.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }


                                if (workSheet3.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet3.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("BranchID").ToUpper().Trim()
                                 || workSheet3.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BranchName").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                #endregion

                                #region import vào bảng OM_DiscSeq và OM_Discount
                                for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                {
                                    int check = 0;
                                    workt_DiscID = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt_DiscIDDr = workSheet.Cells[i, 1].StringValue.PassNull();
                                    workt_DiscSeq = workSheet.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    workt_DiscSeqDr = workSheet.Cells[i, 3].StringValue.PassNull();
                                    workt_StartDate = workSheet.Cells[i, 4].StringValue.PassNull();
                                    workt_EndDate = workSheet.Cells[i, 5].StringValue.PassNull();
                                    ///kiểm tra dữ liệu nhập vào
                                    if (workt_DiscID == null || workt_DiscID == "")
                                    {
                                        errorDiscIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscID.Length > 10)
                                        {
                                            errorDiscIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt_DiscIDDr.Length > 50)
                                    {
                                        errorDiscIDDescr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt_DiscSeq == null || workt_DiscSeq == "")
                                    {
                                        errorDiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscSeq.Length > 10)
                                        {
                                            errorDiscSeqLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {

                                        workt_StartDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorStartDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    try
                                    {
                                        workt_EndDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorEndDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    if (check == 0)
                                    {
                                        if (workt_EndDate.ToDateShort() < workt_StartDate.ToDateShort())
                                        {
                                            errorDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    #region import import sheet chương trình khuyến mãi vào OM_Discount
                                    var recordOM_Discount = lstOM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID);
                                    if (recordOM_Discount == null)
                                    {
                                        var record = _db.OM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID.ToUpper().Trim());
                                        if (record == null)
                                        {
                                            record = new OM_Discount();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.Descr = workt_DiscIDDr;
                                            record.DiscClass = "II";
                                            record.DiscType = "G";
                                            record.Crtd_Role = "HO";
                                            record.Accumulate = false;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            _db.OM_Discount.AddObject(record);
                                            lstOM_Discount.Add(record);
                                        }

                                    }
                                    else
                                    {
                                        lstOM_Discount.Add(recordOM_Discount);
                                    }


                                    #endregion

                                    #region import sheet chương trình khuyến mãi vào OM_DiscSeq
                                    var recordlstOM_DiscSeq = lstOM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                    if (recordlstOM_DiscSeq == null)
                                    {
                                        var record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                        if (record == null)
                                        {
                                            record = new OM_DiscSeq();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt_DiscSeq;
                                            record.Active = 1;
                                            record.BreakBy = "Q";
                                            record.BudgetID = null;
                                            record.DiscClass = "II";
                                            record.DiscFor = "A";
                                            record.Promo = 1;
                                            record.Crtd_Role = "HO";
                                            record.Status = "H";
                                            record.ExactQty = false;
                                            record.ExcludeOtherDisc = false;
                                            record.POStartDate = DateTime.Now;
                                            record.POEndDate = DateTime.Now;
                                            record.POUse = false;
                                            record.ProAplForItem = "A";
                                            record.AutoFreeItem = false;
                                            record.AllowEditDisc = false;
                                            record.StartDate = workt_StartDate.ToDateShort();
                                            record.EndDate = workt_EndDate.ToDateShort();
                                            record.Descr = workt_DiscSeqDr;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Profile = null;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscSeq.AddObject(record);
                                            lstOM_DiscSeq.Add(record);
                                        }
                                        else
                                        {
                                            record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq && p.Status == "H");
                                            if (record != null)
                                            {
                                                record.StartDate = workt_StartDate.ToDateShort();
                                                record.EndDate = workt_EndDate.ToDateShort();
                                                record.Descr = workt_DiscSeqDr;
                                                record.LUpd_DateTime = DateTime.Now;
                                                record.LUpd_Prog = _screenNbr;
                                                record.LUpd_User = Current.UserName;
                                            }
                                        }

                                    }
                                    #endregion
                                }
                                //kiểm tra mã khuyến mãi thống nhất hay không
                                if (lstOM_Discount.Where(p => p.DiscID == workt_DiscID).Count() < (lstOM_Discount.Count()))
                                {
                                    errorDiscID = ",";
                                    flagCheck = true;
                                }
                                #endregion

                                #region import Điều Kiện vào  OM_DiscBreak
                                for (int i = 1; i <= workSheet1.Cells.MaxDataRow; i++)
                                {
                                    workt1_DiscSeq = workSheet1.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt1_Level = workSheet1.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt1_BreakQty = workSheet1.Cells[i, 2].StringValue.PassNull();
                                    workt1_Discount = workSheet1.Cells[i, 3].StringValue.PassNull();
                                    workt1_OM21100MaxLot = workSheet1.Cells[i, 4].StringValue.PassNull();
                                    workt1_Descr = workSheet1.Cells[i, 5].StringValue.PassNull();

                                    if (workt1_DiscSeq == null || workt1_DiscSeq == "")
                                    {
                                        errorDiscSeq1Null += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0)
                                        {
                                            errorDiscSeq1 += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt1_Level == null || workt1_Level == "")
                                    {
                                        errorLevelNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt1_Level.Length > 5)
                                        {
                                            errorLevelLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt1_Level);
                                    }
                                    catch
                                    {
                                        errorLevel += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_BreakQty);
                                    }
                                    catch
                                    {
                                        errorworkt1_BreakQty += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_Discount);
                                    }
                                    catch
                                    {
                                        errorworkt1_Discount += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    try
                                    {
                                        Convert.ToDouble(workt1_OM21100MaxLot);
                                    }
                                    catch
                                    {
                                        errorworkt1_OM21100MaxLot += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt1_Descr.Length > 200)
                                    {
                                        errorworkt1_Descr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    for (int l = workt1_Level.Length; workt1_Level.Length < 5;)
                                        workt1_Level = "0" + workt1_Level;
                                    var recordOM_DiscBreak = lstOM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                    if (recordOM_DiscBreak == null)
                                    {
                                        var record = _db.OM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                        if (record == null)
                                        {
                                            record = new OM_DiscBreak();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt1_DiscSeq;
                                            record.LineRef = workt1_Level;
                                            record.BreakAmt = 0;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscBreak.AddObject(record);
                                            lstOM_DiscBreak.Add(record);
                                        }
                                        record.BreakQty = Convert.ToDouble(workt1_BreakQty);
                                        record.DiscAmt = workt1_Discount.ToDouble();
                                        record.MaxLot = workt1_OM21100MaxLot.ToDouble();
                                        record.Descr = workt1_Descr;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }

                                }

                                #endregion

                                #region Sheet Sản Phẩm Bán => OM_DiscItem
                                for (int i = 1; i <= workSheet2.Cells.MaxDataRow; i++)
                                {
                                    workt2_DiscSeq = workSheet2.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt2_InvtID = workSheet2.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt2_UOM = convertToUnSign(workSheet2.Cells[i, 2].StringValue.PassNull().ToUpper());
                                    //var a = convertToUnSign3(workt2_UOM);
                                    if (workt2_DiscSeq == null || workt2_DiscSeq == "")
                                    {
                                        errorworkt2_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt2_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt2_DiscSeq).Count() == 0)
                                        {
                                            errorworkt2_DiscSep += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt2_InvtID == null || workt2_InvtID == "")
                                    {
                                        errorworkt2_InvtIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt2_InvtID).Count() == 0)
                                        {
                                            errorworkt2_InvtIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt2_UOM == null || workt2_UOM == "")
                                    {
                                        errorworkt2_UOMNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt2_UOM).Count() == 0)
                                        {
                                            errorworkt2_UOM += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    var recordOM_DiscItem = lstOM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                    if (recordOM_DiscItem == null)
                                    {
                                        var record = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt2_DiscSeq;
                                            record.InvtID = workt2_InvtID;
                                            record.Active = 0;
                                            record.BundleAmt = 0;
                                            record.BundleNbr = 0;
                                            record.BundleOrItem = "I";
                                            record.BundleQty = 0;
                                            record.DiscType = "G";
                                            record.QtyType = "E";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscItem.AddObject(record);
                                            lstOM_DiscItem.Add(record);
                                        }

                                        record.UnitDesc = workt2_UOM;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }
                                }
                                #endregion

                                #region Sheet NPP Áp Dụng => OM_DiscCpny
                                for (int i = 1; i <= workSheet3.Cells.MaxDataRow; i++)
                                {
                                    workt3_DiscSeq = workSheet3.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt3_BranchID = workSheet3.Cells[i, 1].StringValue.PassNull().ToUpper();

                                    if (workt3_DiscSeq == null || workt3_DiscSeq == "")
                                    {
                                        errorworkt3_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeq).Count() == 0)
                                        {
                                            errorworkt3_DiscSeq += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_BranchID == null || workt3_BranchID == "")
                                    {
                                        errorworkt3_BranchIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstBrachID.Where(p => p.CpnyID.ToUpper() == workt3_BranchID).Count() == 0)
                                        {
                                            errorworkt3_BranchID += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }


                                    var recordOM_DiscCpny = lstOM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                    if (recordOM_DiscCpny == null)
                                    {
                                        var record = _db.OM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscCpny();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeq;
                                            record.CpnyID = workt3_BranchID;
                                            _db.OM_DiscCpny.AddObject(record);
                                            lstOM_DiscCpny.Add(record);
                                        }

                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                        #region Nếu là template GIIQF
                        if (fileImportName.ToUpper().Trim() == "GIIQF.XLSX" || fileImportName.ToUpper().Trim() == "GIIQF.XLS")
                        {

                            #region kiểm tra template
                            if (workbook.Worksheets.Count > 0)
                            {
                                Worksheet workSheet4 = workbook.Worksheets[4];
                                // kiểm tra sheet Chương trình khuyến mãi
                                if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscID").ToUpper().Trim()
                                  || workSheet.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                  || workSheet.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 4].StringValue.ToUpper().Trim() != (Util.GetLang("StartDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  || workSheet.Cells[0, 5].StringValue.ToUpper().Trim() != (Util.GetLang("EndDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sheet Điều kiện
                                if (workSheet1.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet1.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet1.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 || workSheet1.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("OM21100MaxLot").ToUpper().Trim()
                                 || workSheet1.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sản phẩm bán
                                if (workSheet2.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet2.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("InvtID").ToUpper().Trim()
                                 || workSheet2.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                //Kiểm tra sản phẩm tặng
                                if (workSheet3.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet3.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet3.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("FreeItemID").ToUpper().Trim()
                                 || workSheet3.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 || workSheet3.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra NPP app dung
                                if (workSheet4.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet4.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("BranchID").ToUpper().Trim()
                                 || workSheet4.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BranchName").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                #endregion

                                #region import import sheet chương trình khuyến mãi vào OM_Discount và OM_DiscSeq
                                
                                for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                {
                                    //var keysave = 1;
                                    int check = 0;
                                    workt_DiscID = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt_DiscIDDr = workSheet.Cells[i, 1].StringValue.PassNull();
                                    workt_DiscSeq = workSheet.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    workt_DiscSeqDr = workSheet.Cells[i, 3].StringValue.PassNull();
                                    workt_StartDate = workSheet.Cells[i, 4].StringValue.PassNull();
                                    workt_EndDate = workSheet.Cells[i, 5].StringValue.PassNull();

                                    ///kiểm tra dữ liệu nhập vào
                                    if (workt_DiscID == null || workt_DiscID == "")
                                    {
                                        errorDiscIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscID.Length > 10)
                                        {
                                            errorDiscIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt_DiscIDDr.Length > 50)
                                    {
                                        errorDiscIDDescr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt_DiscSeq == null || workt_DiscSeq == "")
                                    {
                                        errorDiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscSeq.Length > 10)
                                        {
                                            errorDiscSeqLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {

                                        workt_StartDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorStartDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    try
                                    {
                                        workt_EndDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorEndDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    if (check == 0)
                                    {
                                        if (workt_EndDate.ToDateShort() < workt_StartDate.ToDateShort())
                                        {
                                            errorDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }


                                    #region import import sheet chương trình khuyến mãi vào OM_Discount
                                    var recordOM_Discount = lstOM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID);
                                    if (recordOM_Discount == null)
                                    {
                                        var record = _db.OM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID.ToUpper().Trim());
                                        if (record == null)
                                        {
                                            record = new OM_Discount();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.Descr = workt_DiscIDDr;
                                            record.DiscClass = "II";
                                            record.DiscType = "G";
                                            record.Crtd_Role = "HO";
                                            record.Accumulate = false;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            _db.OM_Discount.AddObject(record);
                                            lstOM_Discount.Add(record);
                                        }
                                    }
                                    else
                                    {
                                        lstOM_Discount.Add(recordOM_Discount);
                                    }
                                    #endregion

                                    #region import sheet chương trình khuyến mãi vào OM_DiscSeq
                                    var recordlstOM_DiscSeq = lstOM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                    if (recordlstOM_DiscSeq == null)
                                    {
                                        var record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                        if (record == null)
                                        {
                                            record = new OM_DiscSeq();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt_DiscSeq;
                                            record.Active = 1;
                                            record.BreakBy = "Q";
                                            record.BudgetID = null;
                                            record.DiscClass = "II";
                                            record.DiscFor = "P";
                                            record.Promo = 1;
                                            record.Crtd_Role = "HO";
                                            record.Status = "H";
                                            record.ExactQty = false;
                                            record.ExcludeOtherDisc = false;
                                            record.POStartDate = DateTime.Now;
                                            record.POEndDate = DateTime.Now;
                                            record.POUse = false;
                                            record.ProAplForItem = "A";
                                            record.AutoFreeItem = false;
                                            record.AllowEditDisc = false;
                                            record.StartDate = workt_StartDate.ToDateShort();
                                            record.EndDate = workt_EndDate.ToDateShort();
                                            record.Descr = workt_DiscSeqDr;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Profile = null;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscSeq.AddObject(record);
                                            lstOM_DiscSeq.Add(record);
                                        }
                                        else
                                        {
                                            record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq && p.Status == "H");
                                            if (record != null)
                                            {
                                                record.StartDate = workt_StartDate.ToDateShort();
                                                record.EndDate = workt_EndDate.ToDateShort();
                                                record.Descr = workt_DiscSeqDr;
                                                record.LUpd_DateTime = DateTime.Now;
                                                record.LUpd_Prog = _screenNbr;
                                                record.LUpd_User = Current.UserName;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                #endregion

                                #region import Điều Kiện vào  OM_DiscBreak
                                for (int i = 1; i <= workSheet1.Cells.MaxDataRow; i++)
                                {
                                    workt1_DiscSeq = workSheet1.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt1_Level = workSheet1.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt1_BreakQty = workSheet1.Cells[i, 2].StringValue.PassNull();
                                    workt1_OM21100MaxLot = workSheet1.Cells[i, 3].StringValue.PassNull();
                                    workt1_Descr = workSheet1.Cells[i, 4].StringValue.PassNull();

                                    if (workt1_DiscSeq == null || workt1_DiscSeq == "")
                                    {
                                        errorDiscSeq1Null += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0)
                                        {
                                            errorDiscSeq1 += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt1_Level == null || workt1_Level == "")
                                    {
                                        errorLevelNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt1_Level.Length > 5)
                                        {
                                            errorLevelLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt1_Level);
                                    }
                                    catch
                                    {
                                        errorLevel += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_BreakQty);
                                    }
                                    catch
                                    {
                                        errorworkt1_BreakQty += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }


                                    try
                                    {
                                        Convert.ToDouble(workt1_OM21100MaxLot);
                                    }
                                    catch
                                    {
                                        errorworkt1_OM21100MaxLot += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt1_Descr.Length > 200)
                                    {
                                        errorworkt1_Descr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    for (int l = workt1_Level.Length; workt1_Level.Length < 5;)
                                        workt1_Level = "0" + workt1_Level;
                                    var recordOM_DiscBreak = lstOM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                    if (recordOM_DiscBreak == null)
                                    {
                                        var record = _db.OM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                        if (record == null)
                                        {
                                            record = new OM_DiscBreak();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt1_DiscSeq;
                                            record.LineRef = workt1_Level;
                                            record.BreakAmt = 0;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscBreak.AddObject(record);
                                            lstOM_DiscBreak.Add(record);
                                        }
                                        record.BreakQty = workt1_BreakQty.ToInt();
                                        record.DiscAmt = 0;
                                        record.MaxLot = workt1_OM21100MaxLot.ToInt();
                                        record.Descr = workt1_Descr;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }

                                }
                                #endregion

                                #region Sheet Sản Phẩm Bán => OM_DiscItem
                                for (int i = 1; i <= workSheet2.Cells.MaxDataRow; i++)
                                {
                                    workt2_DiscSeq = workSheet2.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt2_InvtID = workSheet2.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt2_UOM = workSheet2.Cells[i, 2].StringValue.PassNull().ToUpper();

                                    if (workt2_DiscSeq == null || workt2_DiscSeq == "")
                                    {
                                        errorworkt2_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt2_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt2_DiscSeq).Count() == 0)
                                        {
                                            errorworkt2_DiscSep += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt2_InvtID == null || workt2_InvtID == "")
                                    {
                                        errorworkt2_InvtIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt2_InvtID).Count() == 0)
                                        {
                                            errorworkt2_InvtIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt2_UOM == null || workt2_UOM == "")
                                    {
                                        errorworkt2_UOMNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt2_UOM).Count() == 0)
                                        {
                                            errorworkt2_UOM += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    var recordOM_DiscItem = lstOM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                    if (recordOM_DiscItem == null)
                                    {
                                        var record = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt2_DiscSeq;
                                            record.InvtID = workt2_InvtID;
                                            record.Active = 0;
                                            record.BundleAmt = 0;
                                            record.BundleNbr = 0;
                                            record.BundleOrItem = "I";
                                            record.BundleQty = 0;
                                            record.DiscType = "G";
                                            record.QtyType = "E";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscItem.AddObject(record);
                                            lstOM_DiscItem.Add(record);
                                        }
                                        record.UnitDesc = workt2_UOM;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }
                                }
                                #endregion


                                #region Sheet Sản phẩm tặng => OM_DiscFreeItem
                                for (int i = 1; i <= workSheet3.Cells.MaxDataRow; i++)
                                {

                                    string workt3_DiscSeqFreeItem = workSheet3.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    string workt3_LevelFreeItem = workSheet3.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    string workt3_InvtIDFreeItem = workSheet3.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    string workt3_UOMFreeItem = workSheet3.Cells[i, 3].StringValue.PassNull().ToUpper();
                                    string workt3_BreakQtFreeItemy = workSheet3.Cells[i, 4].StringValue.PassNull();

                                    if (workt3_DiscSeqFreeItem == null || workt3_DiscSeqFreeItem == "")
                                    {
                                        errorDiscSeqFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeqFreeItem).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeqFreeItem).Count() == 0)
                                        {
                                            errorDiscSeqFreeItem += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt3_LevelFreeItem == null || workt3_LevelFreeItem == "")
                                    {
                                        errorLevelFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt3_LevelFreeItem.Length > 5)
                                        {
                                            errorLevelFreeItemLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt3_LevelFreeItem);
                                    }
                                    catch
                                    {
                                        errorLevelFreeItem += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    if (workt3_InvtIDFreeItem == null || workt3_InvtIDFreeItem == "")
                                    {
                                        errorFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt3_InvtIDFreeItem).Count() == 0)
                                        {
                                            errorFreeItem += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_UOMFreeItem == null || workt3_UOMFreeItem == "")
                                    {
                                        errorUOMFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt3_UOMFreeItem).Count() == 0)
                                        {
                                            errorUOMFreeItem += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt3_BreakQtFreeItemy);
                                    }
                                    catch
                                    {
                                        errorQtFreeItem += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }
                                    for (int l = workt3_LevelFreeItem.Length; workt3_LevelFreeItem.Length < 5;)
                                        workt3_LevelFreeItem = "0" + workt3_LevelFreeItem;
                                    var recordOM_DiscFreeItem = lstOM_DiscFreeItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeqFreeItem && p.LineRef.ToUpper().Trim() == workt3_LevelFreeItem && p.FreeItemID.ToUpper().Trim() == workt3_InvtIDFreeItem);
                                    if (recordOM_DiscFreeItem == null)
                                    {
                                        var record = _db.OM_DiscFreeItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeqFreeItem && p.LineRef.ToUpper().Trim() == workt3_LevelFreeItem && p.FreeItemID.ToUpper().Trim() == workt3_InvtIDFreeItem);
                                        if (record == null)
                                        {
                                            record = new OM_DiscFreeItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeqFreeItem;
                                            record.LineRef = workt3_LevelFreeItem;
                                            record.FreeItemID = workt3_InvtIDFreeItem;
                                            record.FreeItemBudgetID = "";
                                            record.FreeITemSiteID = "";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscFreeItem.AddObject(record);
                                            lstOM_DiscFreeItem.Add(record);
                                        }
                                        record.FreeItemQty = workt3_BreakQtFreeItemy.ToInt();
                                        record.UnitDescr = workt3_UOMFreeItem;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;

                                    }
                                }
                                #endregion

                                #region Sheet NPP Áp Dụng => OM_DiscCpny
                                for (int i = 1; i <= workSheet4.Cells.MaxDataRow; i++)
                                {

                                    workt3_DiscSeq = workSheet4.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt3_BranchID = workSheet4.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    if (workt3_DiscSeq == null || workt3_DiscSeq == "")
                                    {
                                        errorworkt3_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeq).Count() == 0)
                                        {
                                            errorworkt3_DiscSeq += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_BranchID == null || workt3_BranchID == "")
                                    {
                                        errorworkt3_BranchIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstBrachID.Where(p => p.CpnyID.ToUpper() == workt3_BranchID).Count() == 0)
                                        {
                                            errorworkt3_BranchID += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }
                                    var recordOM_DiscCpny = lstOM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                    if (recordOM_DiscCpny == null)
                                    {
                                        var record = _db.OM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscCpny();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeq;
                                            record.CpnyID = workt3_BranchID;
                                            _db.OM_DiscCpny.AddObject(record);
                                            lstOM_DiscCpny.Add(record);
                                        }

                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                        #region Nếu là template GIIQP
                        if (fileImportName.ToUpper().Trim() == "GIIQP.XLSX" || fileImportName.ToUpper().Trim() == "GIIQP.XLS")
                        {

                            if (workbook.Worksheets.Count > 0)
                            {
                                #region kiểm tra template
                                ///kiểm tra sheet Chương trình khuyến mãi
                                if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscID").ToUpper().Trim()
                                  || workSheet.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                  || workSheet.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 4].StringValue.ToUpper().Trim() != (Util.GetLang("StartDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  || workSheet.Cells[0, 5].StringValue.ToUpper().Trim() != (Util.GetLang("EndDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sheet Điều kiện
                                if (workSheet1.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet1.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet1.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 || workSheet1.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Discount").ToUpper().Trim()
                                 || workSheet1.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                // kiểm tra sản phẩm bán
                                if (workSheet2.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet2.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("InvtID").ToUpper().Trim()
                                 || workSheet2.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }


                                if (workSheet3.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet3.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("BranchID").ToUpper().Trim()
                                 || workSheet3.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BranchName").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                #endregion

                                #region nhập dữ liệu vào OM_Discount và OM_DiscSeq
                                for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                {
                                    //var keysave = 1;
                                    int check = 0;
                                    workt_DiscID = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt_DiscIDDr = workSheet.Cells[i, 1].StringValue.PassNull();
                                    workt_DiscSeq = workSheet.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    workt_DiscSeqDr = workSheet.Cells[i, 3].StringValue.PassNull();
                                    workt_StartDate = workSheet.Cells[i, 4].StringValue.PassNull();
                                    workt_EndDate = workSheet.Cells[i, 5].StringValue.PassNull();
                                    ///kiểm tra dữ liệu nhập vào
                                    if (workt_DiscID == null || workt_DiscID == "")
                                    {
                                        errorDiscIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscID.Length > 10)
                                        {
                                            errorDiscIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt_DiscIDDr.Length > 50)
                                    {
                                        errorDiscIDDescr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt_DiscSeq == null || workt_DiscSeq == "")
                                    {
                                        errorDiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscSeq.Length > 10)
                                        {
                                            errorDiscSeqLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {

                                        workt_StartDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorStartDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    try
                                    {
                                        workt_EndDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorEndDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    if (check == 0)
                                    {
                                        if (workt_EndDate.ToDateShort() < workt_StartDate.ToDateShort())
                                        {
                                            errorDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    #region import import sheet chương trình khuyến mãi vào OM_Discount
                                    var recordOM_Discount = lstOM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID);
                                    if (recordOM_Discount == null)
                                    {
                                        var record = _db.OM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID.ToUpper().Trim());
                                        if (record == null)
                                        {
                                            record = new OM_Discount();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.Descr = workt_DiscIDDr;
                                            record.DiscClass = "II";
                                            record.DiscType = "G";
                                            record.Crtd_Role = "HO";
                                            record.Accumulate = false;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            _db.OM_Discount.AddObject(record);
                                            lstOM_Discount.Add(record);
                                        }
                                    }
                                    else
                                    {
                                        lstOM_Discount.Add(recordOM_Discount);
                                    }
                                    #endregion

                                    #region import sheet chương trình khuyến mãi vào OM_DiscSeq
                                    var recordlstOM_DiscSeq = lstOM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                    if (recordlstOM_DiscSeq == null)
                                    {
                                        var record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                        if (record == null)
                                        {
                                            record = new OM_DiscSeq();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt_DiscSeq;
                                            record.Active = 1;
                                            record.BreakBy = "Q";
                                            record.BudgetID = null;
                                            record.DiscClass = "II";
                                            record.DiscFor = "P";
                                            record.Promo = 1;
                                            record.Crtd_Role = "HO";
                                            record.Status = "H";
                                            record.ExactQty = false;
                                            record.ExcludeOtherDisc = false;
                                            record.POStartDate = DateTime.Now;
                                            record.POEndDate = DateTime.Now;
                                            record.POUse = false;
                                            record.ProAplForItem = "A";
                                            record.AutoFreeItem = false;
                                            record.AllowEditDisc = false;
                                            record.StartDate = workt_StartDate.ToDateShort();
                                            record.EndDate = workt_EndDate.ToDateShort();
                                            record.Descr = workt_DiscSeqDr;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Profile = null;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscSeq.AddObject(record);
                                            lstOM_DiscSeq.Add(record);
                                        }
                                        else
                                        {
                                            record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq && p.Status == "H");
                                            if (record != null)
                                            {
                                                record.StartDate = workt_StartDate.ToDateShort();
                                                record.EndDate = workt_EndDate.ToDateShort();
                                                record.Descr = workt_DiscSeqDr;
                                                record.LUpd_DateTime = DateTime.Now;
                                                record.LUpd_Prog = _screenNbr;
                                                record.LUpd_User = Current.UserName;
                                            }
                                        }
                                    }
                                    #endregion

                                }
                                //kiểm tra mã khuyến mãi thống nhất hay không
                                if (lstOM_Discount.Where(p => p.DiscID == workt_DiscID).Count() < (lstOM_Discount.Count()))
                                {
                                    errorDiscID = ",";
                                    flagCheck = true;
                                }
                                #endregion


                                #region import Điều Kiện vào  OM_DiscBreak
                                for (int i = 1; i <= workSheet1.Cells.MaxDataRow; i++)
                                {
                                    workt1_DiscSeq = workSheet1.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt1_Level = workSheet1.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt1_BreakQty = workSheet1.Cells[i, 2].StringValue.PassNull();
                                    workt1_Discount = workSheet1.Cells[i, 3].StringValue.PassNull();
                                    workt1_Descr = workSheet1.Cells[i, 4].StringValue.PassNull();

                                    if (workt1_DiscSeq == null || workt1_DiscSeq == "")
                                    {
                                        errorDiscSeq1Null += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0)
                                        {
                                            errorDiscSeq1 += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt1_Level == null || workt1_Level == "")
                                    {
                                        errorLevelNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt1_Level.Length > 5)
                                        {
                                            errorLevelLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt1_Level);
                                    }
                                    catch
                                    {
                                        errorLevel += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_BreakQty);
                                    }
                                    catch
                                    {
                                        errorworkt1_BreakQty += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_Discount);
                                    }
                                    catch
                                    {
                                        errorworkt1_Discount += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    if (workt1_Descr.Length > 200)
                                    {
                                        errorworkt1_Descr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    for (int l = workt1_Level.Length; workt1_Level.Length < 5;)
                                        workt1_Level = "0" + workt1_Level;
                                    var recordOM_DiscBreak = lstOM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                    if (recordOM_DiscBreak == null)
                                    {
                                        var record = _db.OM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                        if (record == null)
                                        {
                                            record = new OM_DiscBreak();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt1_DiscSeq;
                                            record.LineRef = workt1_Level;
                                            record.BreakAmt = 0;
                                            record.MaxLot = 0;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscBreak.AddObject(record);
                                            lstOM_DiscBreak.Add(record);
                                        }
                                        record.BreakQty = workt1_BreakQty.ToInt();
                                        record.DiscAmt = workt1_Discount.ToInt();
                                        record.MaxLot = 0;
                                        record.Descr = workt1_Descr;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }

                                }
                                #endregion

                                #region Sheet Sản Phẩm Bán => OM_DiscItem
                                for (int i = 1; i <= workSheet2.Cells.MaxDataRow; i++)
                                {
                                    workt2_DiscSeq = workSheet2.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt2_InvtID = workSheet2.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt2_UOM = workSheet2.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    if (workt2_DiscSeq == null || workt2_DiscSeq == "")
                                    {
                                        errorworkt2_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt2_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt2_DiscSeq).Count() == 0)
                                        {
                                            errorworkt2_DiscSep += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt2_InvtID == null || workt2_InvtID == "")
                                    {
                                        errorworkt2_InvtIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt2_InvtID).Count() == 0)
                                        {
                                            errorworkt2_InvtIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt2_UOM == null || workt2_UOM == "")
                                    {
                                        errorworkt2_UOMNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt2_UOM).Count() == 0)
                                        {
                                            errorworkt2_UOM += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    var recordOM_DiscItem = lstOM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                    if (recordOM_DiscItem == null)
                                    {
                                        var record = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt2_DiscSeq;
                                            record.InvtID = workt2_InvtID;
                                            record.Active = 0;
                                            record.BundleAmt = 0;
                                            record.BundleNbr = 0;
                                            record.BundleOrItem = "I";
                                            record.BundleQty = 0;
                                            record.DiscType = "G";
                                            record.QtyType = "E";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscItem.AddObject(record);
                                            lstOM_DiscItem.Add(record);
                                        }
                                        record.UnitDesc = workt2_UOM;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }
                                }
                                #endregion

                                #region Sheet NPP Áp Dụng => OM_DiscCpny
                                for (int i = 1; i <= workSheet3.Cells.MaxDataRow; i++)
                                {
                                    workt3_DiscSeq = workSheet3.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt3_BranchID = workSheet3.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    if (workt3_DiscSeq == null || workt3_DiscSeq == "")
                                    {
                                        errorworkt3_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeq).Count() == 0)
                                        {
                                            errorworkt3_DiscSeq += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_BranchID == null || workt3_BranchID == "")
                                    {
                                        errorworkt3_BranchIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstBrachID.Where(p => p.CpnyID.ToUpper() == workt3_BranchID).Count() == 0)
                                        {
                                            errorworkt3_BranchID += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }
                                    var recordOM_DiscCpny = lstOM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                    if (recordOM_DiscCpny == null)
                                    {
                                        var record = _db.OM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscCpny();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeq;
                                            record.CpnyID = workt3_BranchID;
                                            _db.OM_DiscCpny.AddObject(record);
                                            lstOM_DiscCpny.Add(record);
                                        }

                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                        #region nếu template là LIIQA
                        if (fileImportName.ToUpper().Trim() == "LIIQA.XLSX" || fileImportName.ToUpper().Trim() == "LIIQA.XLS")
                        {
                            if (workbook.Worksheets.Count > 0)
                            {
                                #region kiểm tra template
                                // kiểm tra sheet Chương trình khuyến mãi
                                if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscID").ToUpper().Trim()
                                  || workSheet.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                  || workSheet.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 4].StringValue.ToUpper().Trim() != (Util.GetLang("StartDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  || workSheet.Cells[0, 5].StringValue.ToUpper().Trim() != (Util.GetLang("EndDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sheet Điều kiện
                                if (workSheet1.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet1.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet1.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 || workSheet1.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Discount").ToUpper().Trim()
                                 || workSheet1.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("OM21100MaxLot").ToUpper().Trim()
                                 || workSheet1.Cells[0, 5].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                // kiểm tra sản phẩm bán
                                if (workSheet2.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet2.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("InvtID").ToUpper().Trim()
                                 || workSheet2.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }


                                if (workSheet3.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet3.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("BranchID").ToUpper().Trim()
                                 || workSheet3.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BranchName").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                #endregion

                                #region import vào bảng OM_Discount và OM_DiscSeq
                                for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                {
                                    //var keysave = 1;
                                    int check = 0;
                                    workt_DiscID = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt_DiscIDDr = workSheet.Cells[i, 1].StringValue.PassNull();
                                    workt_DiscSeq = workSheet.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    workt_DiscSeqDr = workSheet.Cells[i, 3].StringValue.PassNull();
                                    workt_StartDate = workSheet.Cells[i, 4].StringValue.PassNull();
                                    workt_EndDate = workSheet.Cells[i, 5].StringValue.PassNull();

                                    ///kiểm tra dữ liệu nhập vào
                                    if (workt_DiscID == null || workt_DiscID == "")
                                    {
                                        errorDiscIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscID.Length > 10)
                                        {
                                            errorDiscIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt_DiscIDDr.Length > 50)
                                    {
                                        errorDiscIDDescr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt_DiscSeq == null || workt_DiscSeq == "")
                                    {
                                        errorDiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscSeq.Length > 10)
                                        {
                                            errorDiscSeqLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {

                                        workt_StartDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorStartDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    try
                                    {
                                        workt_EndDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorEndDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    if (check == 0)
                                    {
                                        if (workt_EndDate.ToDateShort() < workt_StartDate.ToDateShort())
                                        {
                                            errorDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    #region import import sheet chương trình khuyến mãi vào OM_Discount
                                    var recordOM_Discount = lstOM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID);
                                    if (recordOM_Discount == null)
                                    {
                                        var record = _db.OM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID.ToUpper().Trim());
                                        if (record == null)
                                        {
                                            record = new OM_Discount();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.Descr = workt_DiscIDDr;
                                            record.DiscClass = "II";
                                            record.DiscType = "L";
                                            record.Crtd_Role = "HO";
                                            record.Accumulate = false;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            _db.OM_Discount.AddObject(record);
                                            lstOM_Discount.Add(record);
                                        }
                                    }
                                    else
                                    {
                                        lstOM_Discount.Add(recordOM_Discount);
                                    }
                                    #endregion


                                    #region import sheet chương trình khuyến mãi vào OM_DiscSeq
                                    var recordlstOM_DiscSeq = lstOM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                    if (recordlstOM_DiscSeq == null)
                                    {
                                        var record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                        if (record == null)
                                        {
                                            record = new OM_DiscSeq();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt_DiscSeq;
                                            record.Active = 1;
                                            record.BreakBy = "Q";
                                            record.BudgetID = null;
                                            record.DiscClass = "II";
                                            record.DiscFor = "A";
                                            record.Promo = 1;
                                            record.Crtd_Role = "HO";
                                            record.Status = "H";
                                            record.ExactQty = false;
                                            record.ExcludeOtherDisc = false;
                                            record.POStartDate = DateTime.Now;
                                            record.POEndDate = DateTime.Now;
                                            record.POUse = false;
                                            record.ProAplForItem = "A";
                                            record.AutoFreeItem = false;
                                            record.AllowEditDisc = false;
                                            record.StartDate = workt_StartDate.ToDateShort();
                                            record.EndDate = workt_EndDate.ToDateShort();
                                            record.Descr = workt_DiscSeqDr;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Profile = null;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscSeq.AddObject(record);
                                            lstOM_DiscSeq.Add(record);
                                        }
                                        else
                                        {
                                            record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq && p.Status == "H");
                                            if (record != null)
                                            {
                                                record.StartDate = workt_StartDate.ToDateShort();
                                                record.EndDate = workt_EndDate.ToDateShort();
                                                record.Descr = workt_DiscSeqDr;
                                                record.LUpd_DateTime = DateTime.Now;
                                                record.LUpd_Prog = _screenNbr;
                                                record.LUpd_User = Current.UserName;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                //kiểm tra mã khuyến mãi thống nhất hay không
                                if (lstOM_Discount.Where(p => p.DiscID == workt_DiscID).Count() < (lstOM_Discount.Count()))
                                {
                                    errorDiscID = ",";
                                    flagCheck = true;
                                }
                                #endregion


                                #region import Điều Kiện vào  OM_DiscBreak
                                for (int i = 1; i <= workSheet1.Cells.MaxDataRow; i++)
                                {
                                    workt1_DiscSeq = workSheet1.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt1_Level = workSheet1.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt1_BreakQty = workSheet1.Cells[i, 2].StringValue.PassNull();
                                    workt1_Discount = workSheet1.Cells[i, 3].StringValue.PassNull();
                                    workt1_OM21100MaxLot = workSheet1.Cells[i, 4].StringValue.PassNull();
                                    workt1_Descr = workSheet1.Cells[i, 5].StringValue.PassNull();
                                    if (workt1_DiscSeq == null || workt1_DiscSeq == "")
                                    {
                                        errorDiscSeq1Null += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0)
                                        {
                                            errorDiscSeq1 += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt1_Level == null || workt1_Level == "")
                                    {
                                        errorLevelNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt1_Level.Length > 5)
                                        {
                                            errorLevelLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt1_Level);
                                    }
                                    catch
                                    {
                                        errorLevel += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_BreakQty);
                                    }
                                    catch
                                    {
                                        errorworkt1_BreakQty += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_Discount);
                                    }
                                    catch
                                    {
                                        errorworkt1_Discount += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    try
                                    {
                                        Convert.ToDouble(workt1_OM21100MaxLot);
                                    }
                                    catch
                                    {
                                        errorworkt1_OM21100MaxLot += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt1_Descr.Length > 200)
                                    {
                                        errorworkt1_Descr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    for (int l = workt1_Level.Length; workt1_Level.Length < 5;)
                                        workt1_Level = "0" + workt1_Level;
                                    var recordOM_DiscBreak = lstOM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                    if (recordOM_DiscBreak == null)
                                    {
                                        var record = _db.OM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                        if (record == null)
                                        {
                                            record = new OM_DiscBreak();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt1_DiscSeq;
                                            record.LineRef = workt1_Level;
                                            record.BreakAmt = 0;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscBreak.AddObject(record);
                                            lstOM_DiscBreak.Add(record);
                                        }
                                        record.BreakQty = workt1_BreakQty.ToInt();
                                        record.DiscAmt = workt1_Discount.ToInt();
                                        record.MaxLot = workt1_OM21100MaxLot.ToInt();
                                        record.Descr = workt1_Descr;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }

                                }
                                #endregion

                                #region Sheet Sản Phẩm Bán => OM_DiscItem
                                for (int i = 1; i <= workSheet2.Cells.MaxDataRow; i++)
                                {
                                    workt2_DiscSeq = workSheet2.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt2_InvtID = workSheet2.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt2_UOM = workSheet2.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    if (workt2_DiscSeq == null || workt2_DiscSeq == "")
                                    {
                                        errorworkt2_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt2_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt2_DiscSeq).Count() == 0)
                                        {
                                            errorworkt2_DiscSep += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt2_InvtID == null || workt2_InvtID == "")
                                    {
                                        errorworkt2_InvtIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt2_InvtID).Count() == 0)
                                        {
                                            errorworkt2_InvtIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt2_UOM == null || workt2_UOM == "")
                                    {
                                        errorworkt2_UOMNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt2_UOM).Count() == 0)
                                        {
                                            errorworkt2_UOM += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    var recordOM_DiscItem = lstOM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                    if (recordOM_DiscItem == null)
                                    {
                                        var record = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt2_DiscSeq;
                                            record.InvtID = workt2_InvtID;
                                            record.Active = 0;
                                            record.BundleAmt = 0;
                                            record.BundleNbr = 0;
                                            record.BundleOrItem = "I";
                                            record.BundleQty = 0;
                                            record.DiscType = "L";
                                            record.QtyType = "E";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscItem.AddObject(record);
                                            lstOM_DiscItem.Add(record);
                                        }
                                        record.UnitDesc = workt2_UOM;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }
                                }
                                #endregion


                                #region Sheet NPP Áp Dụng => OM_DiscCpny
                                for (int i = 1; i <= workSheet3.Cells.MaxDataRow; i++)
                                {
                                    workt3_DiscSeq = workSheet3.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt3_BranchID = workSheet3.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    if (workt3_DiscSeq == null || workt3_DiscSeq == "")
                                    {
                                        errorworkt3_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeq).Count() == 0)
                                        {
                                            
                                            errorworkt3_DiscSeq += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_BranchID == null || workt3_BranchID == "")
                                    {
                                        errorworkt3_BranchIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstBrachID.Where(p => p.CpnyID.ToUpper() == workt3_BranchID).Count() == 0)
                                        {
                                            errorworkt3_BranchID += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }
                                    var recordOM_DiscCpny = lstOM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                    if (recordOM_DiscCpny == null)
                                    {
                                        var record = _db.OM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscCpny();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeq;
                                            record.CpnyID = workt3_BranchID;
                                            _db.OM_DiscCpny.AddObject(record);
                                            lstOM_DiscCpny.Add(record);
                                        }

                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                        #region nếu chọn teamlate là LIIQF
                        if (fileImportName.ToUpper().Trim() == "LIIQF.XLSX" || fileImportName.ToUpper().Trim() == "LIIQF.XLS")
                        {

                            if (workbook.Worksheets.Count > 0)
                            {
                                #region kiểm tra template
                                Worksheet workSheet4 = workbook.Worksheets[4];
                                // kiểm tra sheet Chương trình khuyến mãi
                                if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscID").ToUpper().Trim()
                                  || workSheet.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                  || workSheet.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 4].StringValue.ToUpper().Trim() != (Util.GetLang("StartDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  || workSheet.Cells[0, 5].StringValue.ToUpper().Trim() != (Util.GetLang("EndDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sheet Điều kiện
                                if (workSheet1.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet1.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet1.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 || workSheet1.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("OM21100MaxLot").ToUpper().Trim()
                                 || workSheet1.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sản phẩm bán
                                if (workSheet2.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet2.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("InvtID").ToUpper().Trim()
                                 || workSheet2.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                //Kiểm tra sản phẩm tặng
                                if (workSheet3.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet3.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet3.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("FreeItemID").ToUpper().Trim()
                                 || workSheet3.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 || workSheet3.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra NPP app dung
                                if (workSheet4.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet4.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("BranchID").ToUpper().Trim()
                                 || workSheet4.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BranchName").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                #endregion


                                #region import import sheet chương trình khuyến mãi vào OM_Discount và OM_DiscSeq
                                for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                {
                                    //var keysave = 1;
                                    int check = 0;
                                    workt_DiscID = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt_DiscIDDr = workSheet.Cells[i, 1].StringValue.PassNull();
                                    workt_DiscSeq = workSheet.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    workt_DiscSeqDr = workSheet.Cells[i, 3].StringValue.PassNull();
                                    workt_StartDate = workSheet.Cells[i, 4].StringValue.PassNull();
                                    workt_EndDate = workSheet.Cells[i, 5].StringValue.PassNull();

                                    ///kiểm tra dữ liệu nhập vào
                                    if (workt_DiscID == null || workt_DiscID == "")
                                    {
                                        errorDiscIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscID.Length > 10)
                                        {
                                            errorDiscIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt_DiscIDDr.Length > 50)
                                    {
                                        errorDiscIDDescr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt_DiscSeq == null || workt_DiscSeq == "")
                                    {
                                        errorDiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscSeq.Length > 10)
                                        {
                                            errorDiscSeqLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {

                                        workt_StartDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorStartDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    try
                                    {
                                        workt_EndDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorEndDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    if (check == 0)
                                    {
                                        if (workt_EndDate.ToDateShort() < workt_StartDate.ToDateShort())
                                        {
                                            errorDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    #region import import sheet chương trình khuyến mãi vào OM_Discount
                                    var recordOM_Discount = lstOM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID);
                                    if (recordOM_Discount == null)
                                    {
                                        var record = _db.OM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID.ToUpper().Trim());
                                        if (record == null)
                                        {
                                            record = new OM_Discount();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.Descr = workt_DiscIDDr;
                                            record.DiscClass = "II";
                                            record.DiscType = "L";
                                            record.Crtd_Role = "HO";
                                            record.Accumulate = false;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            _db.OM_Discount.AddObject(record);
                                            lstOM_Discount.Add(record);
                                        }
                                    }
                                    else
                                    {
                                        lstOM_Discount.Add(recordOM_Discount);
                                    }
                                    #endregion

                                    #region import sheet chương trình khuyến mãi vào OM_DiscSeq
                                    var recordlstOM_DiscSeq = lstOM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                    if (recordlstOM_DiscSeq == null)
                                    {
                                        var record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                        if (record == null)
                                        {
                                            record = new OM_DiscSeq();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt_DiscSeq;
                                            record.Active = 1;
                                            record.BreakBy = "Q";
                                            record.BudgetID = null;
                                            record.DiscClass = "II";
                                            record.DiscFor = "P";
                                            record.Promo = 1;
                                            record.Crtd_Role = "HO";
                                            record.Status = "H";
                                            record.ExactQty = false;
                                            record.ExcludeOtherDisc = false;
                                            record.POStartDate = DateTime.Now;
                                            record.POEndDate = DateTime.Now;
                                            record.POUse = false;
                                            record.ProAplForItem = "A";
                                            record.AutoFreeItem = false;
                                            record.AllowEditDisc = false;
                                            record.StartDate = workt_StartDate.ToDateShort();
                                            record.EndDate = workt_EndDate.ToDateShort();
                                            record.Descr = workt_DiscSeqDr;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Profile = null;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscSeq.AddObject(record);
                                            lstOM_DiscSeq.Add(record);
                                        }
                                        else
                                        {
                                            record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq && p.Status == "H");
                                            if (record != null)
                                            {
                                                record.StartDate = workt_StartDate.ToDateShort();
                                                record.EndDate = workt_EndDate.ToDateShort();
                                                record.Descr = workt_DiscSeqDr;
                                                record.LUpd_DateTime = DateTime.Now;
                                                record.LUpd_Prog = _screenNbr;
                                                record.LUpd_User = Current.UserName;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                //kiểm tra mã khuyến mãi thống nhất hay không
                                if (lstOM_Discount.Where(p => p.DiscID == workt_DiscID).Count() < (lstOM_Discount.Count()))
                                {
                                    errorDiscID = ",";
                                    flagCheck = true;
                                }
                                #endregion

                                #region import Điều Kiện vào  OM_DiscBreak
                                for (int i = 1; i <= workSheet1.Cells.MaxDataRow; i++)
                                {
                                    workt1_DiscSeq = workSheet1.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt1_Level = workSheet1.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt1_BreakQty = workSheet1.Cells[i, 2].StringValue.PassNull();
                                    workt1_OM21100MaxLot = workSheet1.Cells[i, 3].StringValue.PassNull();
                                    workt1_Descr = workSheet1.Cells[i, 4].StringValue.PassNull();

                                    if (workt1_DiscSeq == null || workt1_DiscSeq == "")
                                    {
                                        errorDiscSeq1Null += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0)
                                        {
                                            errorDiscSeq1 += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt1_Level == null || workt1_Level == "")
                                    {
                                        errorLevelNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt1_Level.Length > 5)
                                        {
                                            errorLevelLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt1_Level);
                                    }
                                    catch
                                    {
                                        errorLevel += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_BreakQty);
                                    }
                                    catch
                                    {
                                        errorworkt1_BreakQty += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }


                                    try
                                    {
                                        Convert.ToDouble(workt1_OM21100MaxLot);
                                    }
                                    catch
                                    {
                                        errorworkt1_OM21100MaxLot += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt1_Descr.Length > 200)
                                    {
                                        errorworkt1_Descr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    for (int l = workt1_Level.Length; workt1_Level.Length < 5;)
                                        workt1_Level = "0" + workt1_Level;
                                    var recordOM_DiscBreak = lstOM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                    if (recordOM_DiscBreak == null)
                                    {
                                        var record = _db.OM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                        if (record == null)
                                        {
                                            record = new OM_DiscBreak();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt1_DiscSeq;
                                            record.LineRef = workt1_Level;
                                            record.BreakAmt = 0;
                                            record.DiscAmt = 0;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscBreak.AddObject(record);
                                            lstOM_DiscBreak.Add(record);
                                        }
                                        record.BreakQty = workt1_BreakQty.ToInt();
                                        record.MaxLot = workt1_OM21100MaxLot.ToInt();
                                        record.Descr = workt1_Descr;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }

                                }
                                #endregion

                                #region Sheet Sản Phẩm Bán => OM_DiscItem
                                for (int i = 1; i <= workSheet2.Cells.MaxDataRow; i++)
                                {
                                    workt2_DiscSeq = workSheet2.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt2_InvtID = workSheet2.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt2_UOM = workSheet2.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    if (workt2_DiscSeq == null || workt2_DiscSeq == "")
                                    {
                                        errorworkt2_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt2_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt2_DiscSeq).Count() == 0)
                                        {
                                            errorworkt2_DiscSep += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt2_InvtID == null || workt2_InvtID == "")
                                    {
                                        errorworkt2_InvtIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt2_InvtID).Count() == 0)
                                        {
                                            errorworkt2_InvtIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt2_UOM == null || workt2_UOM == "")
                                    {
                                        errorworkt2_UOMNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt2_UOM).Count() == 0)
                                        {
                                            errorworkt2_UOM += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    var recordOM_DiscItem = lstOM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                    if (recordOM_DiscItem == null)
                                    {
                                        var record = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt2_DiscSeq;
                                            record.InvtID = workt2_InvtID;
                                            record.Active = 0;
                                            record.BundleAmt = 0;
                                            record.BundleNbr = 0;
                                            record.BundleOrItem = "I";
                                            record.BundleQty = 0;
                                            record.DiscType = "L";
                                            record.QtyType = "E";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscItem.AddObject(record);
                                            lstOM_DiscItem.Add(record);
                                        }
                                        record.UnitDesc = workt2_UOM;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }
                                }
                                #endregion


                                #region Sheet Sản phẩm tặng => OM_DiscFreeItem
                                
                                for (int i = 1; i <= workSheet3.Cells.MaxDataRow; i++)
                                {

                                    string workt3_DiscSeqFreeItem = workSheet3.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    string workt3_LevelFreeItem = workSheet3.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    string workt3_InvtIDFreeItem = workSheet3.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    string workt3_UOMFreeItem = workSheet3.Cells[i, 3].StringValue.PassNull().ToUpper();
                                    string workt3_BreakQtFreeItemy = workSheet3.Cells[i, 4].StringValue.PassNull();

                                    if (workt3_DiscSeqFreeItem == null || workt3_DiscSeqFreeItem == "")
                                    {
                                        errorDiscSeqFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeqFreeItem).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeqFreeItem).Count() == 0)
                                        {
                                            errorDiscSeqFreeItem += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt3_LevelFreeItem == null || workt3_LevelFreeItem == "")
                                    {
                                        errorLevelFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt3_LevelFreeItem.Length > 5)
                                        {
                                            errorLevelFreeItemLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt3_LevelFreeItem);
                                    }
                                    catch
                                    {
                                        errorLevelFreeItem += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }


                                    if (workt3_InvtIDFreeItem == null || workt3_InvtIDFreeItem == "")
                                    {
                                        errorFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt3_InvtIDFreeItem).Count() == 0)
                                        {
                                            errorFreeItem += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_UOMFreeItem == null || workt3_UOMFreeItem == "")
                                    {
                                        errorUOMFreeItemNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt3_UOMFreeItem).Count() == 0)
                                        {
                                            errorUOMFreeItem += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt3_BreakQtFreeItemy);
                                    }
                                    catch
                                    {
                                        errorQtFreeItem += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }


                                    for (int l = workt3_LevelFreeItem.Length; workt3_LevelFreeItem.Length < 5;)
                                        workt3_LevelFreeItem = "0" + workt3_LevelFreeItem;
                                    var recordOM_DiscFreeItem = lstOM_DiscFreeItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeqFreeItem && p.LineRef.ToUpper().Trim() == workt3_LevelFreeItem && p.FreeItemID.ToUpper().Trim() == workt3_InvtIDFreeItem);
                                    if (recordOM_DiscFreeItem == null)
                                    {
                                        var record = _db.OM_DiscFreeItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeqFreeItem && p.LineRef.ToUpper().Trim() == workt3_LevelFreeItem && p.FreeItemID.ToUpper().Trim() == workt3_InvtIDFreeItem);
                                        if (record == null)
                                        {
                                            record = new OM_DiscFreeItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeqFreeItem;
                                            record.LineRef = workt3_LevelFreeItem;
                                            record.FreeItemID = workt3_InvtIDFreeItem;
                                            record.FreeItemBudgetID = "";
                                            record.FreeITemSiteID = "";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscFreeItem.AddObject(record);
                                            lstOM_DiscFreeItem.Add(record);
                                        }
                                        record.FreeItemQty = workt3_BreakQtFreeItemy.ToInt();
                                        record.UnitDescr = workt3_UOMFreeItem;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;

                                    }
                                }
                                #endregion

                                #region Sheet NPP Áp Dụng => OM_DiscCpny
                                for (int i = 1; i <= workSheet4.Cells.MaxDataRow; i++)
                                {

                                    workt3_DiscSeq = workSheet4.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt3_BranchID = workSheet4.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    if (workt3_DiscSeq == null || workt3_DiscSeq == "")
                                    {
                                        errorworkt3_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeq).Count() == 0)
                                        {
                                            errorworkt3_DiscSeq += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_BranchID == null || workt3_BranchID == "")
                                    {
                                        errorworkt3_BranchIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstBrachID.Where(p => p.CpnyID.ToUpper() == workt3_BranchID).Count() == 0)
                                        {
                                            errorworkt3_BranchID += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }
                                    var recordOM_DiscCpny = lstOM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                    if (recordOM_DiscCpny == null)
                                    {
                                        var record = _db.OM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscCpny();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeq;
                                            record.CpnyID = workt3_BranchID;
                                            _db.OM_DiscCpny.AddObject(record);
                                            lstOM_DiscCpny.Add(record);
                                        }

                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                        #region Nếu chon template LIIQP
                        if (fileImportName.ToUpper().Trim() == "LIIQP.XLSX" || fileImportName.ToUpper().Trim() == "LIIQP.XLS")
                        {
                            if (workbook.Worksheets.Count > 0)
                            {
                                #region kiểm tra template
                                // kiểm tra sheet Chương trình khuyến mãi
                                if (workSheet.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscID").ToUpper().Trim()
                                  || workSheet.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                  || workSheet.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                  || workSheet.Cells[0, 4].StringValue.ToUpper().Trim() != (Util.GetLang("StartDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  || workSheet.Cells[0, 5].StringValue.ToUpper().Trim() != (Util.GetLang("EndDate") + " \r\n" + " (dd/MM/yyyy)").ToUpper().Trim()
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }

                                // kiểm tra sheet Điều kiện
                                if (workSheet1.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet1.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("Level").ToUpper().Trim()
                                 || workSheet1.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BreakQty").ToUpper().Trim()
                                 || workSheet1.Cells[0, 3].StringValue.ToUpper().Trim() != Util.GetLang("Discount").ToUpper().Trim()
                                 || workSheet1.Cells[0, 4].StringValue.ToUpper().Trim() != Util.GetLang("Descr").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                // kiểm tra sản phẩm bán
                                if (workSheet2.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet2.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("InvtID").ToUpper().Trim()
                                 || workSheet2.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("UOM").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }


                                if (workSheet3.Cells[0, 0].StringValue.ToUpper().Trim() != Util.GetLang("DiscSeq").ToUpper().Trim()
                                 || workSheet3.Cells[0, 1].StringValue.ToUpper().Trim() != Util.GetLang("BranchID").ToUpper().Trim()
                                 || workSheet3.Cells[0, 2].StringValue.ToUpper().Trim() != Util.GetLang("BranchName").ToUpper().Trim()
                                 )
                                {
                                    throw new MessageException(MessageType.Message, "148");
                                }
                                #endregion

                                #region import vào bảng OM_DiscSeq và OM_Discount
                                for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                                {
                                    int check = 0;
                                    workt_DiscID = workSheet.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt_DiscIDDr = workSheet.Cells[i, 1].StringValue.PassNull();
                                    workt_DiscSeq = workSheet.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    workt_DiscSeqDr = workSheet.Cells[i, 3].StringValue.PassNull();
                                    workt_StartDate = workSheet.Cells[i, 4].StringValue.PassNull();
                                    workt_EndDate = workSheet.Cells[i, 5].StringValue.PassNull();

                                    ///kiểm tra dữ liệu nhập vào
                                    if (workt_DiscID == null || workt_DiscID == "")
                                    {
                                        errorDiscIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscID.Length > 10)
                                        {
                                            errorDiscIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt_DiscIDDr.Length > 50)
                                    {
                                        errorDiscIDDescr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (workt_DiscSeq == null || workt_DiscSeq == "")
                                    {
                                        errorDiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (workt_DiscSeq.Length > 10)
                                        {
                                            errorDiscSeqLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {

                                        workt_StartDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorStartDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    try
                                    {
                                        workt_EndDate.ToDateShort();
                                    }
                                    catch
                                    {
                                        errorEndDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        check = 1;
                                    }
                                    if (check == 0)
                                    {
                                        if (workt_EndDate.ToDateShort() < workt_StartDate.ToDateShort())
                                        {
                                            errorDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    #region import import sheet chương trình khuyến mãi vào OM_Discount
                                    var recordOM_Discount = lstOM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID);
                                    if (recordOM_Discount == null)
                                    {
                                        var record = _db.OM_Discount.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID.ToUpper().Trim());
                                        if (record == null)
                                        {
                                            record = new OM_Discount();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.Descr = workt_DiscIDDr;
                                            record.DiscClass = "II";
                                            record.DiscType = "L";
                                            record.Crtd_Role = "HO";
                                            record.Accumulate = false;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            record.LUpd_DateTime = DateTime.Now;
                                            record.LUpd_Prog = _screenNbr;
                                            record.LUpd_User = Current.UserName;
                                            _db.OM_Discount.AddObject(record);
                                            lstOM_Discount.Add(record);
                                        }
                                    }
                                    else
                                    {
                                        lstOM_Discount.Add(recordOM_Discount);
                                    }
                                    #endregion

                                    #region import sheet chương trình khuyến mãi vào OM_DiscSeq
                                    var recordlstOM_DiscSeq = lstOM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                    if (recordlstOM_DiscSeq == null)
                                    {
                                        var record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq);
                                        if (record == null)
                                        {
                                            record = new OM_DiscSeq();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt_DiscSeq;
                                            record.Active = 1;
                                            record.BreakBy = "Q";
                                            record.BudgetID = null;
                                            record.DiscClass = "II";
                                            record.DiscFor = "P";
                                            record.Promo = 1;
                                            record.Crtd_Role = "HO";
                                            record.Status = "H";
                                            record.ExactQty = false;
                                            record.ExcludeOtherDisc = false;
                                            record.POStartDate = DateTime.Now;
                                            record.POEndDate = DateTime.Now;
                                            record.POUse = false;
                                            record.ProAplForItem = "A";
                                            record.AutoFreeItem = false;
                                            record.AllowEditDisc = false;
                                            record.StartDate = workt_StartDate.ToDateShort();
                                            record.EndDate = workt_EndDate.ToDateShort();
                                            record.Descr = workt_DiscSeqDr;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Profile = null;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscSeq.AddObject(record);
                                            lstOM_DiscSeq.Add(record);
                                        }
                                        else
                                        {
                                            record = _db.OM_DiscSeq.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt_DiscSeq && p.Status == "H");
                                            if (record != null)
                                            {
                                                record.StartDate = workt_StartDate.ToDateShort();
                                                record.EndDate = workt_EndDate.ToDateShort();
                                                record.Descr = workt_DiscSeqDr;
                                                record.LUpd_DateTime = DateTime.Now;
                                                record.LUpd_Prog = _screenNbr;
                                                record.LUpd_User = Current.UserName;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                //kiểm tra mã khuyến mãi thống nhất hay không
                                if (lstOM_Discount.Where(p => p.DiscID == workt_DiscID).Count() < (lstOM_Discount.Count()))
                                {
                                    errorDiscID = ",";
                                    flagCheck = true;
                                }
                                #endregion


                                #region import Điều Kiện vào  OM_DiscBreak
                                for (int i = 1; i <= workSheet1.Cells.MaxDataRow; i++)
                                {
                                    workt1_DiscSeq = workSheet1.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt1_Level = workSheet1.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt1_BreakQty = workSheet1.Cells[i, 2].StringValue.PassNull();
                                    workt1_Discount = workSheet1.Cells[i, 3].StringValue.PassNull();
                                    workt1_Descr = workSheet1.Cells[i, 4].StringValue.PassNull();

                                    if (workt1_DiscSeq == null || workt1_DiscSeq == "")
                                    {
                                        errorDiscSeq1Null += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq.ToUpper() == workt1_DiscSeq).Count() == 0)
                                        {
                                            errorDiscSeq1 += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt1_Level == null || workt1_Level == "")
                                    {
                                        errorLevelNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    else
                                    {
                                        if (workt1_Level.Length > 5)
                                        {
                                            errorLevelLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    try
                                    {
                                        Convert.ToInt32(workt1_Level);
                                    }
                                    catch
                                    {
                                        errorLevel += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_BreakQty);
                                    }
                                    catch
                                    {
                                        errorworkt1_BreakQty += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    try
                                    {
                                        Convert.ToDouble(workt1_Discount);
                                    }
                                    catch
                                    {
                                        errorworkt1_Discount += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    if (workt1_Descr.Length > 200)
                                    {
                                        errorworkt1_Descr += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }

                                    for (int l = workt1_Level.Length; workt1_Level.Length < 5;)
                                        workt1_Level = "0" + workt1_Level;
                                    var recordOM_DiscBreak = lstOM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                    if (recordOM_DiscBreak == null)
                                    {
                                        var record = _db.OM_DiscBreak.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt1_DiscSeq && p.LineRef.ToUpper().Trim() == workt1_Level);
                                        if (record == null)
                                        {
                                            record = new OM_DiscBreak();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt1_DiscSeq;
                                            record.LineRef = workt1_Level;
                                            record.BreakAmt = 0;
                                            record.MaxLot = 0;
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscBreak.AddObject(record);
                                            lstOM_DiscBreak.Add(record);
                                        }
                                        record.BreakQty = workt1_BreakQty.ToInt();
                                        record.DiscAmt = workt1_Discount.ToInt();
                                        record.Descr = workt1_Descr;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }

                                }
                                #endregion

                                #region Sheet Sản Phẩm Bán => OM_DiscItem
                                for (int i = 1; i <= workSheet2.Cells.MaxDataRow; i++)
                                {
                                    workt2_DiscSeq = workSheet2.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt2_InvtID = workSheet2.Cells[i, 1].StringValue.PassNull().ToUpper();
                                    workt2_UOM = workSheet2.Cells[i, 2].StringValue.PassNull().ToUpper();
                                    if (workt2_DiscSeq == null || workt2_DiscSeq == "")
                                    {
                                        errorworkt2_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt2_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt2_DiscSeq).Count() == 0)
                                        {
                                            errorworkt2_DiscSep += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (workt2_InvtID == null || workt2_InvtID == "")
                                    {
                                        errorworkt2_InvtIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_Inventory.Where(p => p.InvtID.ToUpper() == workt2_InvtID).Count() > 0)
                                        {
                                            errorworkt2_InvtIDLength += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt2_UOM == null || workt2_UOM == "")
                                    {
                                        errorworkt2_UOMNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstIN_UnitConversion.Where(p => p.FromUnit.ToUpper() == workt2_UOM) == null)
                                        {
                                            errorworkt2_UOM += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (flagCheck)
                                    {
                                        continue;
                                    }
                                    var recordOM_DiscItem = lstOM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                    if (recordOM_DiscItem == null)
                                    {
                                        var record = _db.OM_DiscItem.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt2_DiscSeq && p.InvtID.ToUpper().Trim() == workt2_InvtID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscItem();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt2_DiscSeq;
                                            record.InvtID = workt2_InvtID;
                                            record.Active = 0;
                                            record.BundleAmt = 0;
                                            record.BundleNbr = 0;
                                            record.BundleOrItem = "I";
                                            record.BundleQty = 0;
                                            record.DiscType = "L";
                                            record.QtyType = "E";
                                            record.Crtd_DateTime = DateTime.Now;
                                            record.Crtd_Prog = _screenNbr;
                                            record.Crtd_User = Current.UserName;
                                            _db.OM_DiscItem.AddObject(record);
                                            lstOM_DiscItem.Add(record);
                                        }
                                        record.UnitDesc = workt2_UOM;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = Current.UserName;
                                    }
                                }
                                #endregion

                                #region Sheet NPP Áp Dụng => OM_DiscCpny
                                for (int i = 1; i <= workSheet3.Cells.MaxDataRow; i++)
                                {
                                    workt3_DiscSeq = workSheet3.Cells[i, 0].StringValue.PassNull().ToUpper();
                                    workt3_BranchID = workSheet3.Cells[i, 1].StringValue.PassNull().ToUpper();

                                    if (workt3_DiscSeq == null || workt3_DiscSeq == "")
                                    {
                                        errorworkt3_DiscSeqNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstDiscSeq.Where(p => p.DiscSeq.ToUpper() == workt3_DiscSeq).Count() == 0 && lstOM_DiscSeq.Where(p => p.DiscSeq == workt3_DiscSeq).Count() == 0)
                                        {
                                            errorworkt3_DiscSeq += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }

                                    if (workt3_BranchID == null || workt3_BranchID == "")
                                    {
                                        errorworkt3_BranchIDNull += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstBrachID.Where(p => p.CpnyID.ToUpper() == workt3_BranchID).Count() == 0)
                                        {
                                            errorworkt3_BranchID += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                    if (flagCheck)
                                    {
                                        continue;
                                    }
                                    var recordOM_DiscCpny = lstOM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                    if (recordOM_DiscCpny == null)
                                    {
                                        var record = _db.OM_DiscCpny.FirstOrDefault(p => p.DiscID.ToUpper().Trim() == workt_DiscID && p.DiscSeq.ToUpper().Trim() == workt3_DiscSeq && p.CpnyID.ToUpper().Trim() == workt3_BranchID);
                                        if (record == null)
                                        {
                                            record = new OM_DiscCpny();
                                            record.ResetET();
                                            record.DiscID = workt_DiscID;//các key đều upper lên khi save
                                            record.DiscSeq = workt3_DiscSeq;
                                            record.CpnyID = workt3_BranchID;
                                            _db.OM_DiscCpny.AddObject(record);
                                            lstOM_DiscCpny.Add(record);
                                        }

                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                        #region thông báo lỗi

                        message = errorDiscIDNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("DiscID"), errorDiscIDNull, workSheet.Name);
                        message += errorDiscIDLength == "" ? "" : string.Format(Message.GetString("2017112502", null), Util.GetLang("DiscID"), errorDiscIDLength, "10", workSheet.Name);
                        message += errorDiscID == "" ? "" : string.Format(Message.GetString("2017112503", null), Util.GetLang("DiscID"), workSheet.Name);
                        message += errorDiscIDDescr == "" ? "" : string.Format(Message.GetString("2017112504", null), Util.GetLang("Descr"), Util.GetLang("DiscID"), errorDiscIDDescr, "50", workSheet.Name);
                        message += errorDiscSeqNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("DiscSeq"), errorDiscSeqNull, workSheet.Name);
                        message += errorDiscSeqLength == "" ? "" : string.Format(Message.GetString("2017112504", null), Util.GetLang("DiscSeq"), errorDiscSeqLength, "10", workSheet.Name);
                        message += errorStartDate == "" ? "" : string.Format(Message.GetString("2017112505", null), Util.GetLang("StartDate"), errorStartDate, Util.GetLang("StartDate"), workSheet.Name);
                        message += errorEndDate == "" ? "" : string.Format(Message.GetString("2017112505", null), Util.GetLang("EndDate"), errorEndDate, Util.GetLang("EndDate"), workSheet.Name);
                        message += errorDate == "" ? "" : string.Format(Message.GetString("2017112509", null), errorDate, null, workSheet.Name);
                        message += errorDiscSeq1Null == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("DiscSeq"), errorDiscSeq1Null, workSheet1.Name);
                        message += errorDiscSeq1 == "" ? "" : string.Format(Message.GetString("2017112508", null), Util.GetLang("DiscSeq"), errorDiscSeq1, workSheet1.Name);
                        message += errorLevelNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("Level"), errorLevelNull, workSheet1.Name);
                        message += errorLevelLength == "" ? "" : string.Format(Message.GetString("2017112502", null), Util.GetLang("Level"), errorLevelLength, "5", workSheet1.Name);
                        message += errorLevel == "" ? "" : string.Format(Message.GetString("2017112506", null), Util.GetLang("Level"), errorLevel, Util.GetLang("Level"), workSheet1.Name);
                        message += errorworkt1_BreakQty == "" ? "" : string.Format(Message.GetString("2017112506", null), Util.GetLang("BreakQty"), errorworkt1_BreakQty, Util.GetLang("BreakQty"), workSheet1.Name);
                        message += errorworkt1_Discount == "" ? "" : string.Format(Message.GetString("2017112506", null), Util.GetLang("Discount"), errorworkt1_Discount, Util.GetLang("Discount"), workSheet1.Name);
                        message += errorworkt1_OM21100MaxLot == "" ? "" : string.Format(Message.GetString("2017112506", null), Util.GetLang("OM21100MaxLot"), errorworkt1_OM21100MaxLot, Util.GetLang("OM21100MaxLot"), workSheet1.Name);
                        message += errorworkt1_Descr == "" ? "" : string.Format(Message.GetString("2017112502", null), Util.GetLang("Descr"), errorworkt1_Descr, "200", workSheet1.Name);
                        message += errorworkt2_DiscSeqNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("DiscSeq"), errorworkt2_DiscSeqNull, workSheet2.Name);
                        message += errorworkt2_DiscSep == "" ? "" : string.Format(Message.GetString("2017112508", null), Util.GetLang("DiscSeq"), errorworkt2_DiscSep, workSheet2.Name);
                        message += errorworkt2_InvtIDNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("InvtID"), errorworkt2_InvtIDNull, workSheet2.Name);
                        message += errorworkt2_InvtIDLength == "" ? "" : string.Format(Message.GetString("2017112508", null), Util.GetLang("InvtID"), errorworkt2_InvtIDLength, workSheet2.Name);
                        message += errorworkt2_UOMNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("UOM"), errorworkt2_UOMNull, workSheet2.Name);
                        message += errorworkt2_UOM == "" ? "" : string.Format(Message.GetString("2017112507", null), Util.GetLang("UOM"), errorworkt2_UOM, okUOM, workSheet2.Name);
                        message += errorDiscSeqFreeItemNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("DiscSeq"), errorDiscSeqFreeItemNull, namesheet);
                        message += errorDiscSeqFreeItem == "" ? "" : string.Format(Message.GetString("2017112508", null), Util.GetLang("DiscSeq"), errorDiscSeqFreeItem, namesheet);
                        message += errorLevelFreeItemNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("Level"), errorLevelFreeItemNull, namesheet);
                        message += errorLevelFreeItemLength == "" ? "" : string.Format(Message.GetString("2017112502", null), Util.GetLang("Level"), errorLevelFreeItemLength, "5", namesheet);
                        message += errorLevelFreeItem == "" ? "" : string.Format(Message.GetString("2017112506", null), Util.GetLang("Level"), errorLevelFreeItem, Util.GetLang("Level"), namesheet);
                        message += errorFreeItemNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("FreeItemID"), errorFreeItemNull, namesheet);
                        message += errorFreeItem == "" ? "" : string.Format(Message.GetString("2017112508", null), Util.GetLang("InvtID"), errorFreeItem, namesheet);
                        message += errorUOMFreeItemNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("UOM"), errorUOMFreeItemNull, namesheet);
                        message += errorUOMFreeItem == "" ? "" : string.Format(Message.GetString("2017112507", null), Util.GetLang("UOM"), errorUOMFreeItem, okUOM, namesheet);
                        message += errorQtFreeItem == "" ? "" : string.Format(Message.GetString("2017112506", null), Util.GetLang("BreakQty"), errorQtFreeItem, Util.GetLang("BreakQty"), namesheet);
                        message += errorworkt3_DiscSeqNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("DiscSeq"), errorworkt3_DiscSeqNull, namesheetNPP);
                        message += errorworkt3_DiscSeq == "" ? "" : string.Format(Message.GetString("2017112508", null), Util.GetLang("DiscSeq"), errorworkt3_DiscSeq, namesheetNPP);
                        message += errorworkt3_BranchIDNull == "" ? "" : string.Format(Message.GetString("2017112501", null), Util.GetLang("BranchID"), errorworkt3_BranchIDNull, namesheetNPP);
                        message += errorworkt3_BranchID == "" ? "" : string.Format(Message.GetString("2017112508", null), Util.GetLang("BranchID"), errorworkt3_BranchID, namesheetNPP);

                        #endregion
                        if (message == "" || message == string.Empty)
                        {
                            _db.SaveChanges();
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "2017112510");
                    }                  
                }               
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
            return _logMessage;
        }

    }
}

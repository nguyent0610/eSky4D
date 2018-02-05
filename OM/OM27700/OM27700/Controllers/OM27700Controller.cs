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
        //private string _beginStatus = "H";
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

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
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

        public ActionResult GetSaleInvt(string accumulateID)
        {
            var invts = _db.OM27700_pgSalesInvt(accumulateID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
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

        public ActionResult GetCustomerCombo(string query, int start, int limit, int page, string lstCpnyID)
        {
            query = query ?? string.Empty;
            if (page != 1) query = string.Empty;
            int startFromIndex = start == 0 ? 0 : start + 1;
            var lstCustomer = _db.OM27700_pcCustomer(Current.UserName, Current.CpnyID, Current.LangID, query, startFromIndex, start + limit, lstCpnyID.RemoveLast(), 1).ToList();
            var paging = new Paging<OM27700_pcCustomer_Result>(lstCustomer, lstCustomer.Count > 0 ? lstCustomer[0].TotalRecords.Value : 0);
            return this.Store(paging.Data, paging.TotalRecords);
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
                    var invts = _db.OM27700_pcInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList().Where(i => i.NodeID == inactiveHierachy.NodeID && i.NodeLevel == inactiveHierachy.NodeLevel && i.ParentRecordID == inactiveHierachy.ParentRecordID).ToList();// _db.OM27700_pcInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList()
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
            var lstCustomer = _db.OM27700_pcCustomer(Current.UserName, Current.CpnyID, Current.LangID, string.Empty, 0, 0 + 20, lstCpny, 2).ToList();

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
                    if (lstCustAdd.Count() == 0)
                    {
                        nodeCompany.Leaf = true;
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
            var lstSales = _db.OM27700_pcSalesPerson(lstCpny, Current.UserName, Current.CpnyID, Current.LangID).ToList();

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

                    if (lstSalesAdd.Count == 0)
                    {
                        nodeCompany.Leaf = true;
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
            tree.Fields.Add(new ModelField("Unit", ModelFieldType.String));
            tree.Fields.Add(new ModelField("StkUnit", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ClassID", ModelFieldType.String));
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
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, "I");
            tree.Root.Add(node);


            var treeBranch = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "treePanelInvt_checkChange";
            tree.Listeners.BeforeItemExpand.Handler = "if (App.treePanelInvt) { App.treePanelInvt.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts(); }";
            tree.Listeners.AfterItemExpand.Handler = "if (App.treePanelInvt) {App.treePanelInvt.view.refresh(); App.treePanelInvt.el.unmask();Ext.resumeLayouts(true); } ";
            tree.AddTo(treeBranch);
            return this.Direct();
        }


        public ActionResult OM27700GetSale(string panelID)
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
            tree.ID = "treeInvt";
            tree.ItemID = "treeInvt";
            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("NodeLevel", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ParentRecordID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("InvtID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Descr", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CnvFact", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Unit", ModelFieldType.String));
            tree.Fields.Add(new ModelField("StkUnit", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ClassID", ModelFieldType.String));
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
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, "I");
            tree.Root.Add(node);


            var treeBranch = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "treeSaleInvt_checkChange";
            tree.Listeners.BeforeItemExpand.Handler = "App.treeSaleInvt.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "if (App.treeInvt){App.treeInvt.view.refresh(); }App.treeSaleInvt.el.unmask();Ext.resumeLayouts(true);";
            tree.AddTo(treeBranch);
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
                if (handle != "" && handle != "N")
                    inputAccumulate.Status = handle;
                else
                    inputAccumulate.Status = data["cboStatus"];

                            
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
                    Save_ProductSale(data, accumulateID);
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

                    var lstInvtSetup = _db.OM_AccumulatedInvtSetup.Where(l => l.AccumulateID.ToUpper() == accumulateID).ToList();
                    foreach (var item in lstInvtSetup)
                    {
                        _db.OM_AccumulatedInvtSetup.DeleteObject(item);
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
            var lstCpny = cpnyHandler.BatchObjectData<OM27700_pgCompany_Result>();

            lstCpny.Created.AddRange(lstCpny.Updated);


            string branch = string.Empty;
            foreach (var cpny in lstCpny.Created.Where(p => !string.IsNullOrWhiteSpace(p.CpnyID)))
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

            StoreDataHandler dataHander4 = new StoreDataHandler(data["lstCustomerSave"]);
            ChangeRecords<OM27700_pgCustomer_Result> lstCustDetail = dataHander4.BatchObjectData<OM27700_pgCustomer_Result>();
            lstCustDetail.Created.AddRange(lstCustDetail.Updated);
            foreach (OM27700_pgCustomer_Result deleted in lstCustDetail.Deleted)
            {
                if (lstCustDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.CpnyID == deleted.CpnyID && p.CustID == deleted.CustID).Count() > 0)
                {
                    lstCustDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.CpnyID == deleted.CpnyID && p.CustID == deleted.CustID).FirstOrDefault().tstamp = deleted.tstamp;
                }
                //else
                //{
                //    var del = _db.OM_AccumulatedCustomer.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.CpnyID == deleted.CpnyID && p.CustID == deleted.CustID);
                //    if (del != null)
                //    {
                //        _db.OM_AccumulatedCustomer.DeleteObject(del);
                //    }
                //}
            }
            var srvQuestionCpnyHandler = new StoreDataHandler(data["lstCustomerDetailAll"]);
            var lstsrvQuestionCpny = srvQuestionCpnyHandler.ObjectData<OM27700_pgCustomer_Result>().ToList();

            var lstDel = _db.OM_AccumulatedCustomer.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionCpny.FirstOrDefault(p =>// p.AccumulateID == lstDel[i].AccumulateID &&
                                 p.CpnyID == lstDel[i].CpnyID
                                && p.CustID == lstDel[i].CustID);
                if (objDel == null)
                {
                    _db.OM_AccumulatedCustomer.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM27700_pgCustomer_Result curBranch in lstCustDetail.Created)
            {
                if (curBranch.CustID.PassNull() == "") continue;

                var lang = _db.OM_AccumulatedCustomer.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.CpnyID == curBranch.CpnyID && p.CustID == curBranch.CustID);

                if (lang != null)
                {
                    if (lang.tstamp.ToHex() == curBranch.tstamp.ToHex())
                    {
                        lang.NumRegLot = curBranch.NumRegLot;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                    }
                }
                else
                {
                    var temp = _db.OM_AccumulatedCustomer.FirstOrDefault(p => p.CpnyID == curBranch.CpnyID && p.AccumulateID.ToUpper() == accumulateID.ToUpper() && p.CustID == curBranch.CustID);
                    if (temp == null)
                    {
                        lang = new OM_AccumulatedCustomer();
                        lang.ResetET();
                        OM_AccumulatedCustomer newCust = new OM_AccumulatedCustomer();
                        lang.AccumulateID = accumulateID;
                        lang.CpnyID = curBranch.CpnyID;
                        lang.CustID = curBranch.CustID;
                        lang.NumRegLot = curBranch.NumRegLot;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                        lang.Crtd_DateTime = DateTime.Now;
                        lang.Crtd_Prog = _screenNbr;
                        lang.Crtd_User = Current.UserName;
                        _db.OM_AccumulatedCustomer.AddObject(lang);
                    }
                    else
                    {
                        continue;
                    }
                }
            }              
        }

        private void Save_Sales(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();

            StoreDataHandler dataHander4 = new StoreDataHandler(data["lstSalesSave"]);
            ChangeRecords<OM27700_pgSalesPerson_Result> lstSalesDetail = dataHander4.BatchObjectData<OM27700_pgSalesPerson_Result>();
            lstSalesDetail.Created.AddRange(lstSalesDetail.Updated);
            foreach (OM27700_pgSalesPerson_Result deleted in lstSalesDetail.Deleted)
            {
                if (lstSalesDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.CpnyID == deleted.CpnyID && p.SlsperID == deleted.SlsperID).Count() > 0)
                {
                    lstSalesDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.CpnyID == deleted.CpnyID && p.SlsperID == deleted.SlsperID).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedSalesPerson.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.CpnyID == deleted.CpnyID && p.SlsperID == deleted.SlsperID);
                    if (del != null)
                    {
                        _db.OM_AccumulatedSalesPerson.DeleteObject(del);
                    }
                }
            }
            var srvQuestionCpnyHandler = new StoreDataHandler(data["lstSalesDetailAll"]);
            var lstsrvQuestionCpny = srvQuestionCpnyHandler.ObjectData<OM27700_pgSalesPerson_Result>().ToList();

            var lstDel = _db.OM_AccumulatedSalesPerson.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionCpny.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.CpnyID == lstDel[i].CpnyID
                                && p.SlsperID == lstDel[i].SlsperID);
                if (objDel == null)
                {
                    _db.OM_AccumulatedSalesPerson.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM27700_pgSalesPerson_Result curBranch in lstSalesDetail.Created)
            {
                if (curBranch.SlsperID.PassNull() == "") continue;

                var lang = _db.OM_AccumulatedSalesPerson.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.CpnyID == curBranch.CpnyID && p.SlsperID == curBranch.SlsperID);

                if (lang != null)
                {
                    if (lang.tstamp.ToHex() == curBranch.tstamp.ToHex())
                    {
                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                    }
                }
                else
                {
                    var temp = _db.OM_AccumulatedSalesPerson.FirstOrDefault(p => p.CpnyID == curBranch.CpnyID && p.AccumulateID.ToUpper() == accumulateID.ToUpper() && p.SlsperID == curBranch.SlsperID);
                    if (temp == null)
                    {
                        lang = new OM_AccumulatedSalesPerson();
                        lang.ResetET();
                        OM_AccumulatedSalesPerson newCust = new OM_AccumulatedSalesPerson();
                        lang.AccumulateID = accumulateID;
                        lang.CpnyID = curBranch.CpnyID;
                        lang.SlsperID = curBranch.SlsperID;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                        lang.Crtd_DateTime = DateTime.Now;
                        lang.Crtd_Prog = _screenNbr;
                        lang.Crtd_User = Current.UserName;
                        _db.OM_AccumulatedSalesPerson.AddObject(lang);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        private void Save_Cpny(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();

            StoreDataHandler dataHander4 = new StoreDataHandler(data["lstCpnySave"]);
            ChangeRecords<OM27700_pgCompany_Result> lstBranchDetail = dataHander4.BatchObjectData<OM27700_pgCompany_Result>();
            lstBranchDetail.Created.AddRange(lstBranchDetail.Updated);
            foreach (OM27700_pgCompany_Result deleted in lstBranchDetail.Deleted)
            {
                if (lstBranchDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.CpnyID == deleted.CpnyID).Count() > 0)
                {
                    lstBranchDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.CpnyID == deleted.CpnyID).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedCpny.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.CpnyID == deleted.CpnyID);
                    if (del != null)
                    {
                        _db.OM_AccumulatedCpny.DeleteObject(del);
                    }
                }
            }
            var srvQuestionCpnyHandler = new StoreDataHandler(data["lstCpnyDetailAll"]);
            var lstsrvQuestionCpny = srvQuestionCpnyHandler.ObjectData<OM27700_pgCompany_Result>().ToList();

            var lstDel = _db.OM_AccumulatedCpny.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionCpny.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.CpnyID == lstDel[i].CpnyID);
                if (objDel == null)
                {
                    _db.OM_AccumulatedCpny.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM27700_pgCompany_Result curBranch in lstBranchDetail.Created)
            {
                if (curBranch.CpnyID.PassNull() == "") continue;

                var lang = _db.OM_AccumulatedCpny.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.CpnyID == curBranch.CpnyID);

                if (lang != null)
                {
                    if (lang.tstamp.ToHex() == curBranch.tstamp.ToHex())
                    {
                        lang.CpnyID = curBranch.CpnyID;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                    }
                }
                else
                {
                    var temp = _db.OM_AccumulatedCpny.FirstOrDefault(p => p.CpnyID == curBranch.CpnyID && p.AccumulateID.ToUpper() == accumulateID.ToUpper());
                    if (temp == null)
                    {
                        lang = new OM_AccumulatedCpny();
                        lang.ResetET();
                        OM_AccumulatedCpny newCust = new OM_AccumulatedCpny();
                        lang.AccumulateID = accumulateID;
                        lang.CpnyID = curBranch.CpnyID;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                        lang.Crtd_DateTime = DateTime.Now;
                        lang.Crtd_Prog = _screenNbr;
                        lang.Crtd_User = Current.UserName;
                        _db.OM_AccumulatedCpny.AddObject(lang);
                    }
                    else
                    {
                        continue;
                    }
                }
            }    
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
            string AccumulateID = accumulateID.ToUpper();

            StoreDataHandler dataHander4 = new StoreDataHandler(data["lstInvtSave"]);
            ChangeRecords<OM27700_pgInvt_Result> lstInvtDetail = dataHander4.BatchObjectData<OM27700_pgInvt_Result>();
            lstInvtDetail.Created.AddRange(lstInvtDetail.Updated);
            foreach (OM27700_pgInvt_Result deleted in lstInvtDetail.Deleted)
            {
                if (lstInvtDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.LevelID == deleted.LevelID).Count() > 0)
                {
                    lstInvtDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.LevelID == deleted.LevelID).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedInvt.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.InvtID == deleted.InvtID && p.LevelID == deleted.LevelID);
                    if (del != null)
                    {
                        _db.OM_AccumulatedInvt.DeleteObject(del);
                    }
                }
            }
            var srvQuestionCpnyHandler = new StoreDataHandler(data["lstInvtDetailAll"]);
            var lstsrvQuestionCpny = srvQuestionCpnyHandler.ObjectData<OM27700_pgInvt_Result>().ToList();

            var lstDel = _db.OM_AccumulatedInvt.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionCpny.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.LevelID == lstDel[i].LevelID &&
                                p.InvtID == lstDel[i].InvtID);
                if (objDel == null)
                {
                    _db.OM_AccumulatedInvt.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM27700_pgInvt_Result currentCust in lstInvtDetail.Created)
            {
                if (currentCust.InvtID.PassNull() == "") continue;

                var lang = _db.OM_AccumulatedInvt.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.LevelID == currentCust.LevelID && p.InvtID == currentCust.InvtID);

                if (lang != null)
                {
                    if (lang.tstamp.ToHex() == currentCust.tstamp.ToHex())
                    {
                        lang.Qty = currentCust.Qty;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                    }
                }
                else
                {
                    var temp = _db.OM_AccumulatedInvt.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.LevelID == currentCust.LevelID && p.InvtID == currentCust.InvtID);
                    if (temp == null)
                    {
                        lang = new OM_AccumulatedInvt();
                        lang.ResetET();
                        OM_AccumulatedInvt newCust = new OM_AccumulatedInvt();
                        lang.AccumulateID = accumulateID;

                        lang.LevelID = currentCust.LevelID;
                        lang.InvtID = currentCust.InvtID;
                        lang.Qty = currentCust.Qty;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                        lang.Crtd_DateTime = DateTime.Now;
                        lang.Crtd_Prog = _screenNbr;
                        lang.Crtd_User = Current.UserName;
                        _db.OM_AccumulatedInvt.AddObject(lang);
                    }
                    else
                    {
                        continue;
                    }
                }
            }    




            //string AccumulateID = accumulateID.ToUpper();
            //var discCustHandler = new StoreDataHandler(data["lstInvtChange"]);
            //var lstDiscCust = discCustHandler.ObjectData<OM27700_pgInvt_Result>()
            //            .Where(p => Util.PassNull(p.InvtID) != string.Empty).ToList();

            //var lstDel = _db.OM_AccumulatedInvt.Where(p => p.AccumulateID.ToUpper() == AccumulateID).ToList();
            //for (int i = 0; i < lstDel.Count; i++)
            //{
            //    if (!lstDiscCust.Any(p => p.InvtID == lstDel[i].InvtID && p.AccumulateID == lstDel[i].AccumulateID))
            //    {
            //        _db.OM_AccumulatedInvt.DeleteObject(lstDel[i]);
            //    }
            //}

            //foreach (var currentCust in lstDiscCust)
            //{
            //    var sales = (from p in _db.OM_AccumulatedInvt
            //                 where
            //                     p.AccumulateID.ToUpper() == AccumulateID
            //                     && p.InvtID == currentCust.InvtID
            //                 select p).FirstOrDefault();
            //    if (sales == null)
            //    {
            //        OM_AccumulatedInvt newCust = new OM_AccumulatedInvt();
            //        newCust.AccumulateID = accumulateID;

            //        newCust.LevelID = currentCust.LevelID;
            //        newCust.InvtID = currentCust.InvtID;
            //        newCust.Qty = currentCust.Qty;

            //        newCust.LUpd_DateTime = DateTime.Now;
            //        newCust.LUpd_Prog = _screenNbr;
            //        newCust.LUpd_User = Current.UserName;
            //        newCust.Crtd_DateTime = DateTime.Now;
            //        newCust.Crtd_Prog = _screenNbr;
            //        newCust.Crtd_User = Current.UserName;
            //        _db.OM_AccumulatedInvt.AddObject(newCust);
            //    }
            //    else
            //    {
            //        sales.Qty = currentCust.Qty;

            //        sales.LUpd_DateTime = DateTime.Now;
            //        sales.LUpd_Prog = _screenNbr;
            //        sales.LUpd_User = Current.UserName;
            //    }
            //}
        }

        private void Save_ProductSale(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();

            StoreDataHandler dataHander4 = new StoreDataHandler(data["lstProductSave"]);
            ChangeRecords<OM27700_pgSalesInvt_Result> lstProductDetail = dataHander4.BatchObjectData<OM27700_pgSalesInvt_Result>();
            lstProductDetail.Created.AddRange(lstProductDetail.Updated);
            foreach (OM27700_pgSalesInvt_Result deleted in lstProductDetail.Deleted)
            {
                if (lstProductDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.InvtID == deleted.InvtID).Count() > 0)
                {
                    lstProductDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.InvtID == deleted.InvtID).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedInvtSetup.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.InvtID == deleted.InvtID);
                    if (del != null)
                    {
                        _db.OM_AccumulatedInvtSetup.DeleteObject(del);
                    }
                }
            }
            var srvQuestionCpnyHandler = new StoreDataHandler(data["lstProductDetailAll"]);
            var lstsrvQuestionCpny = srvQuestionCpnyHandler.ObjectData<OM27700_pgSalesInvt_Result>().ToList();

            var lstDel = _db.OM_AccumulatedInvtSetup.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionCpny.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.InvtID == lstDel[i].InvtID);
                if (objDel == null)
                {
                    _db.OM_AccumulatedInvtSetup.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM27700_pgSalesInvt_Result curBranch in lstProductDetail.Created)
            {
                if (curBranch.InvtID.PassNull() == "") continue;

                var lang = _db.OM_AccumulatedInvtSetup.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.InvtID == curBranch.InvtID);

                if (lang != null)
                {
                    if (lang.tstamp.ToHex() == curBranch.tstamp.ToHex())
                    {
                        lang.Qty = curBranch.Qty;
                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                    }
                }
                else
                {
                    var temp = _db.OM_AccumulatedInvtSetup.FirstOrDefault(p => p.InvtID == curBranch.InvtID && p.AccumulateID.ToUpper() == accumulateID.ToUpper());
                    if (temp == null)
                    {
                        lang = new OM_AccumulatedInvtSetup();
                        lang.ResetET();
                        OM_AccumulatedInvtSetup newCust = new OM_AccumulatedInvtSetup();
                        lang.AccumulateID = accumulateID;
                        lang.InvtID = curBranch.InvtID;
                        lang.Qty = curBranch.Qty;

                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;
                        lang.Crtd_DateTime = DateTime.Now;
                        lang.Crtd_Prog = _screenNbr;
                        lang.Crtd_User = Current.UserName;
                        _db.OM_AccumulatedInvtSetup.AddObject(lang);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            //var invtChangeHandler = new StoreDataHandler(data["lstSaleProduct"]);
            //var lstInvtChange = invtChangeHandler.BatchObjectData<OM27700_pgSalesInvt_Result>();
            //lstInvtChange.Created.AddRange(lstInvtChange.Updated);

            //foreach (var deleted in lstInvtChange.Deleted)
            //{
            //    if (lstInvtChange.Created.Where(x =>x.InvtID == deleted.InvtID).Count() > 0)
            //    {
            //        lstInvtChange.Created.FirstOrDefault(x =>x.InvtID == deleted.InvtID).tstamp = deleted.tstamp;
            //    }
            //    else
            //    {
            //        var deletedLevel = _db.OM_AccumulatedInvtSetup.FirstOrDefault(x =>
            //                x.AccumulateID == accumulateID
            //                && x.InvtID == deleted.InvtID);
            //        if (!string.IsNullOrWhiteSpace(deleted.InvtID) && deletedLevel != null
            //            && !string.IsNullOrWhiteSpace(deleted.InvtID))
            //        {
            //            _db.OM_AccumulatedInvtSetup.DeleteObject(deletedLevel);
            //        }
            //    }
            //}

            //foreach (var created in lstInvtChange.Created)
            //{
            //    var createdInvt = _db.OM_AccumulatedInvtSetup.FirstOrDefault(x => x.AccumulateID == accumulateID
            //        && x.InvtID == created.InvtID);
            //    if (!string.IsNullOrWhiteSpace(created.InvtID)
            //        && !string.IsNullOrWhiteSpace(created.InvtID))
            //    {
            //        if (createdInvt != null)
            //        {
            //            update_SaleInvt(ref createdInvt, created, false);
            //        }
            //        else
            //        {
            //            createdInvt = new OM_AccumulatedInvtSetup();
            //            createdInvt.AccumulateID = accumulateID;
            //            update_SaleInvt(ref createdInvt, created, true);
            //            _db.OM_AccumulatedInvtSetup.AddObject(createdInvt);
            //        }

            //    }
            //}
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

        private void update_SaleInvt(ref OM_AccumulatedInvtSetup createdInvt, OM27700_pgSalesInvt_Result created, bool isNew)
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
                //accumulate.Status = _beginStatus;

                accumulate.AccumulateID= inputAccumulate.AccumulateID;
                accumulate.Crtd_DateTime = DateTime.Now;
                accumulate.Crtd_Prog = _screenNbr;
                accumulate.Crtd_User = Current.UserName;
            }

            accumulate.RegisForm = inputAccumulate.RegisForm;
            accumulate.RegisTo = inputAccumulate.RegisTo;
            accumulate.Type = inputAccumulate.Type;
            accumulate.Descr = inputAccumulate.Descr;
            accumulate.ApplyFor = inputAccumulate.ApplyFor;
            accumulate.ApplyType = inputAccumulate.ApplyType;
            accumulate.FromDate = inputAccumulate.FromDate;
            accumulate.ToDate = inputAccumulate.ToDate;
            accumulate.ObjApply = inputAccumulate.ObjApply;
            accumulate.Status = inputAccumulate.Status;

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
            createdLevel.Point = created.Point;
            createdLevel.LUpd_DateTime = DateTime.Now;
            createdLevel.LUpd_Prog = _screenNbr;
            createdLevel.LUpd_User = Current.UserName;
        }
        private void update_Customer(OM_AccumulatedCustomer t, OM27700_pgCustomer_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
            }
            //t.NumRegLot = s.NumRegLot;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
        }
        #endregion
       
    }
}

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

namespace OM20320.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20320Controller : Controller
    {
        //private string _beginStatus = "H";
        private string _noneStatus = "N";
        private string _applyTypeQty = "Q";
        private string _applyTypePoint = "P";
        private string _screenNbr = "OM20320";
        private string _userName = Current.UserName;
        OM20320Entities _db = Util.CreateObjectContext<OM20320Entities>(false);
        List<OM20320_ptTreeNodeCustomer_Result> lstAllNode = new List<OM20320_ptTreeNodeCustomer_Result>();
        List<SI_Hierarchy> _lstSI_Hierarchy = new List<SI_Hierarchy>();
        List<OM20320_ptProduct_Result> _lstAllProduct = new List<OM20320_ptProduct_Result>();
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }
        public ActionResult LoadGrid(string Type, string Status)
        {
            var slsper = _db.OM20320_pgLoadGrid(Current.UserName, Current.CpnyID, Current.LangID, Type, Status).ToList();
            return this.Store(slsper);
        }

        public ActionResult GetAccumulateById(string accumulateID)
        {
            var accumulate = _db.OM_Accumulated.FirstOrDefault(x => x.AccumulateID == accumulateID);
            return this.Store(accumulate);
        }
        public ActionResult LoadGridReality(string AccumulateID)
        {
            var Reality = _db.OM20320_pgReality(Current.UserName, Current.CpnyID, Current.LangID, AccumulateID).ToList();
            return this.Store(Reality);
        }
        public ActionResult LoadGridPgProduct(string AccumulateID)
        {
            var Product = _db.OM20320_pgProduct(Current.UserName, Current.CpnyID, Current.LangID, AccumulateID).ToList();
            return this.Store(Product);
        }
        public ActionResult LoadGridAccuAmt(string AccumulateID)
        {
            var AccuAmt = _db.OM20320_pgAccuAmt(Current.UserName, Current.CpnyID, Current.LangID, AccumulateID).ToList();
            return this.Store(AccuAmt);
        }
        public ActionResult LoadGridApplyCust(string AccumulateID,string typeAccumulated)
        {
            var Reality = _db.OM20320_pgApplyCust(Current.UserName, Current.CpnyID, Current.LangID, AccumulateID, typeAccumulated).ToList();
            return this.Store(Reality);
        }
        public ActionResult LoadGridAccumulatedManage() 
        {
            var Reality = _db.OM20320_pgAccumulatedManage(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(Reality);
        }
        public ActionResult LoadGridAccumulatedReward()
        {
            var Reality = _db.OM20320_pgAccumulatedReward(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(Reality);
        }
       // [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
      
        #region -tree-
        [DirectMethod]
        public ActionResult OM20320GetTreeData(string typeAccumulation)
        {
           
            string panelID = "treeApplyCustommerID";
            TreePanel tree = new TreePanel();
            tree.ID = "treeApplyCustommer";
            tree.ItemID = "treeApplyCustommer";
            //e.AfterRender.ExtraParams.Add(new Parameter("panelID", "treeBranch"));
            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CustID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CustName", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ClassID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Descr", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ContractType", ModelFieldType.String));
            tree.Fields.Add(new ModelField("LevelID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("RefCustID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Chain", ModelFieldType.String));
            tree.Fields.Add(new ModelField("PharmacyType", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
           
            //node.Checked = true;
            lstAllNode = _db.OM20320_ptTreeNodeCustomer(Current.UserName, Current.CpnyID, Current.LangID, typeAccumulation).ToList();

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

            tree.Listeners.ItemClick.Fn = "nodeClick";
            tree.Listeners.CheckChange.Fn = "treeApplyCustommerID_CheckChange";
            //tree.Listeners.AfterItemExpand.Handler = "App.treeApplyCustommer.el.unmask();Ext.resumeLayouts(true);";

            tree.AddTo(treeBranch);

            return this.Direct();
        }
        List<string> lst = new List<string>();
        
            
        private Node SetNodeValue(OM20320_ptTreeNodeCustomer_Result objNode, Ext.Net.Icon icon)
        {
            Node node = new Node();
            _db.CommandTimeout = int.MaxValue;
            Random rand = new Random();
            node.NodeID = objNode.Code + objNode.ParentID + (rand.Next(999, 9999) + objNode.LevelID).ToString();
            node.Checked = false;
            node.Text = objNode.Descr;
            node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = objNode.Type, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = objNode.Descr, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "CustID", Value = objNode.Code, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "ContractType", Value = objNode.ContractType, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "CustName", Value = objNode.Descr, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "ClassID", Value = objNode.ParentID, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = objNode.Type, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "RefCustID", Value = objNode.RefCustID, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "PharmacyType", Value = objNode.PharmacyType, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "Chain", Value = objNode.Chain, Mode = Ext.Net.ParameterMode.Value });

            node.Icon = Ext.Net.Icon.Folder;
            node.Leaf = objNode.LevelID == 0;
            //node.Leaf = objNode.Position == "S";// true;
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
        #endregion

        #region -tree- Product
        public ActionResult OM20320GettreeProduct(string panelID)
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
            tree.ID = "treeProduct";
            tree.ItemID = "treeProduct";
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
            _lstAllProduct = _db.OM20320_ptProduct(Current.UserName, Current.CpnyID, Current.LangID).ToList();
            _lstSI_Hierarchy = _db.SI_Hierarchy.ToList();
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, "I");
            tree.Root.Add(node);


            var treeBranch = X.GetCmp<Panel>(panelID);
            tree.Listeners.CheckChange.Fn = "treeSaleInvt_checkChange";
            tree.Listeners.BeforeItemExpand.Handler = "App.treeProductID.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "if (App.treeProduct){App.treeProduct.view.refresh(); }App.treeProductID.el.unmask();Ext.resumeLayouts(true);";
            tree.AddTo(treeBranch);
            return this.Direct();
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

            var childrenInactiveHierachies = _lstSI_Hierarchy
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
                    var invts = _lstAllProduct.Where(i => i.NodeID == inactiveHierachy.NodeID && i.NodeLevel == inactiveHierachy.NodeLevel && i.ParentRecordID == inactiveHierachy.ParentRecordID).ToList();// _db.OM27700_pcInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList()
                    if (invts.Count > 0)
                    {
                        foreach (var invt in invts)
                        {
                            Node invtNode = new Node();

                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Invt", Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "InvtID", Value = invt.InvtID, Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = invt.Descr, Mode = ParameterMode.Value });
                            invtNode.CustomAttributes.Add(new ConfigItem() { Name = "Unit", Value = invt.Unit, Mode = ParameterMode.Value });
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

        #endregion

        #region -Save-

        #region -Save-data-
        public ActionResult SaveNewData(FormCollection data)
        {
            try
            {
                string accumulateID = data["txtProgramID"];
                string handle = data["cboHandle"];
                Save_NewInfo(data, accumulateID);
                Save_Cpny(data, accumulateID);
                Save_Reality(data, accumulateID);
                Save_RealityAmt(data, accumulateID);
                Save_RealityInvt(data, accumulateID);
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

        public ActionResult SaveEditData(FormCollection data)
        {
            try
            {
                string accumulateID = data["txtProgramID"];
                string handle = data["cboHandle"];
                Save_EditInfo(data, accumulateID);
                Save_Cpny(data, accumulateID);
                Save_Reality(data, accumulateID);
                Save_RealityAmt(data, accumulateID);
                Save_RealityInvt(data, accumulateID);
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
        public ActionResult SaveNewOfNewData(FormCollection data)
        {
            try
            {
                string accumulateID = data["txtProgramID"];
                string handle = data["cboHandle"];
                Save_NewOfNewInfo(data, accumulateID);
                Save_Cpny(data, accumulateID);
                Save_Reality(data, accumulateID);
                Save_RealityAmt(data, accumulateID);
                Save_RealityInvt(data, accumulateID);
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

        private void Update_Header(ref OM_Accumulated accumulate, OM_Accumulated inputAccumulate, bool isNew)
        {
            if (isNew)
            {
                accumulate.AccumulateID = inputAccumulate.AccumulateID;
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
            accumulate.Status = inputAccumulate.Status;
            accumulate.LUpd_DateTime = DateTime.Now;
            accumulate.LUpd_Prog = _screenNbr;
            accumulate.LUpd_User = Current.UserName;
        }
        private void Save_Task(FormCollection data, OM_Accumulated accumulate, string handle)
        {
            var cpnyHandler = new StoreDataHandler(data["lstCpny"]);
            var lstCust = cpnyHandler.BatchObjectData<OM20320_pgApplyCust_Result>();

            lstCust.Created.AddRange(lstCust.Updated);
            string branch = string.Empty;
            foreach (var cust in lstCust.Created.Where(p => !string.IsNullOrWhiteSpace(p.CustID)))
            {
                branch += cust.CustID + ',';
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
        }

        #endregion

        #region -Save- info Khách Hàng
        private void Save_Cpny(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();
            StoreDataHandler dataCust = new StoreDataHandler(data["lstCustSave"]);
            ChangeRecords<OM20320_pgApplyCust_Result> lstCustDetail = dataCust.BatchObjectData<OM20320_pgApplyCust_Result>();
            lstCustDetail.Created.AddRange(lstCustDetail.Updated);
            foreach (OM20320_pgApplyCust_Result deleted in lstCustDetail.Deleted)
            {
                if (lstCustDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.CustID == deleted.CustID).Count() > 0)
                {
                    lstCustDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.CustID == deleted.CustID).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedCustomer.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.CustID == deleted.CustID);
                    if (del != null)
                    {
                        _db.OM_AccumulatedCustomer.DeleteObject(del);
                    }
                }
            }
            var srvQuestionCpnyHandler = new StoreDataHandler(data["lstCustDetailAll"]);
            var lstsrvQuestionCpny = srvQuestionCpnyHandler.ObjectData<OM20320_pgApplyCust_Result>().ToList();

            var lstDel = _db.OM_AccumulatedCustomer.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionCpny.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.CustID == lstDel[i].CustID);
                if (objDel == null)
                {
                    _db.OM_AccumulatedCustomer.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM20320_pgApplyCust_Result curBranch in lstCustDetail.Created)
            {
                if (curBranch.CustID.PassNull() == "") continue;
                var AccumulatedCust = _db.OM_AccumulatedCustomer.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.CustID == curBranch.CustID);
                if(AccumulatedCust == null)
                {
                    AccumulatedCust = new OM_AccumulatedCustomer();
                    AccumulatedCust.ResetET();
                    AccumulatedCust.AccumulateID = accumulateID;
                    AccumulatedCust.CustID = curBranch.CustID;
                    AccumulatedCust.Crtd_DateTime = DateTime.Now;
                    AccumulatedCust.Crtd_Prog = _screenNbr;
                    AccumulatedCust.Crtd_User = Current.UserName;
                    _db.OM_AccumulatedCustomer.AddObject(AccumulatedCust);
                }
                AccumulatedCust.LUpd_DateTime = DateTime.Now;
                AccumulatedCust.LUpd_Prog = _screenNbr;
                AccumulatedCust.LUpd_User = Current.UserName;
            }
        }

        private void Save_NewInfo(FormCollection data, string accumulateID)
        {
            accumulateID = data["txtProgramID"];
            string handle = data["cboHandle"].PassNull();
            string Descr = data["txtDescr"];
            string TypeAccumulation = data["TypeAccumulation"];
            string MonthYear = data["cboMonthYear"];
            string Status = data["cboStatus"];
            string ApplyTo = data["Applyto"];
            DateTime FromDay = Convert.ToDateTime(data["FromDay"]);
            string CheckSalesBy = data["cboCheckSalesBy"];
            DateTime ToDay = Convert.ToDateTime(data["ToDay"]);
            bool Using = data["active"].ToBool();
            var CustInfo = _db.OM_Accumulated.FirstOrDefault(p => p.AccumulateID.ToUpper() == accumulateID.ToUpper());
            if (CustInfo != null)
            {
                throw new MessageException(MessageType.Message, "22112018", "", new string[] { Util.GetLang("OM20320_ProgramID") });

            }
            if (CustInfo == null)
            {
                CustInfo = new OM_Accumulated();
                CustInfo.ResetET();
                CustInfo.AccumulateID = accumulateID;
                
                CustInfo.Crtd_DateTime = DateTime.Now;
                CustInfo.Crtd_Prog = _screenNbr;
                CustInfo.Crtd_User = Current.UserName;
                _db.OM_Accumulated.AddObject(CustInfo);
            }
            CustInfo.Descr = Descr;
            CustInfo.BreakBy = CheckSalesBy;
            CustInfo.Active = Using;
            CustInfo.Type = TypeAccumulation;
            CustInfo.CycleNbr = MonthYear;
            CustInfo.ApplyFor = ApplyTo;
            CustInfo.FromDate = FromDay;
            CustInfo.ToDate = ToDay;
            if(handle == "")
            {
                CustInfo.Status = Status;
            }
            else
            {
                CustInfo.Status = handle;
            }
            CustInfo.LUpd_DateTime = DateTime.Now;
            CustInfo.LUpd_Prog = _screenNbr;
            CustInfo.LUpd_User = Current.UserName;
        }

        private void Save_NewOfNewInfo(FormCollection data, string accumulateID)
        {
            accumulateID = data["txtProgramID"];
            string handle = data["cboHandle"].PassNull();
            string Descr = data["txtDescr"];
            string TypeAccumulation = data["TypeAccumulation"];
            string MonthYear = data["cboMonthYear"];
            string Status = data["cboStatus"];
            string ApplyTo = data["Applyto"];
            DateTime FromDay = Convert.ToDateTime(data["FromDay"]);
            string CheckSalesBy = data["cboCheckSalesBy"];
            DateTime ToDay = Convert.ToDateTime(data["ToDay"]);
            bool Using = data["active"].ToBool();
            var CustInfo = _db.OM_Accumulated.FirstOrDefault(p => p.AccumulateID.ToUpper() == accumulateID.ToUpper());
            //if (CustInfo != null)
            //{
            //    throw new MessageException(MessageType.Message, "22112018", "", new string[] { Util.GetLang("OM20320_ProgramID") }); 

            //}
            if (CustInfo == null)
            {
                CustInfo = new OM_Accumulated();
                CustInfo.ResetET();
                CustInfo.AccumulateID = accumulateID;

                CustInfo.Crtd_DateTime = DateTime.Now;
                CustInfo.Crtd_Prog = _screenNbr;
                CustInfo.Crtd_User = Current.UserName;
                _db.OM_Accumulated.AddObject(CustInfo);
            }
            CustInfo.Descr = Descr;
            CustInfo.BreakBy = CheckSalesBy;
            CustInfo.Active = Using;
            CustInfo.Type = TypeAccumulation;
            CustInfo.CycleNbr = MonthYear;
            CustInfo.ApplyFor = ApplyTo;
            CustInfo.FromDate = FromDay;
            CustInfo.ToDate = ToDay;
            if (handle == "")
            {
                CustInfo.Status = Status;
            }
            else
            {
                CustInfo.Status = handle;
            }
            CustInfo.LUpd_DateTime = DateTime.Now;
            CustInfo.LUpd_Prog = _screenNbr;
            CustInfo.LUpd_User = Current.UserName;
        }
        private void Save_EditInfo(FormCollection data, string accumulateID)
        {
            accumulateID = data["txtProgramID"];
            string handle = data["cboHandle"].PassNull();
            string Descr = data["txtDescr"];
            string TypeAccumulation = data["TypeAccumulation"];
            string MonthYear = data["cboMonthYear"];
            string Status = data["cboStatus"];
            string ApplyTo = data["Applyto"];
            DateTime FromDay = Convert.ToDateTime(data["FromDay"]);
            string CheckSalesBy = data["cboCheckSalesBy"];
            DateTime ToDay = Convert.ToDateTime(data["ToDay"]);
            bool Using = data["active"].ToBool();
            var CustInfo = _db.OM_Accumulated.FirstOrDefault(p => p.AccumulateID.ToUpper() == accumulateID.ToUpper());
            if (CustInfo == null)
            {
                CustInfo = new OM_Accumulated();
                CustInfo.ResetET();
                CustInfo.AccumulateID = accumulateID;

                CustInfo.Crtd_DateTime = DateTime.Now;
                CustInfo.Crtd_Prog = _screenNbr;
                CustInfo.Crtd_User = Current.UserName;
                _db.OM_Accumulated.AddObject(CustInfo);
            }
            CustInfo.Descr = Descr;
            CustInfo.BreakBy = CheckSalesBy;
            CustInfo.Active = Using;
            CustInfo.Type = TypeAccumulation;
            CustInfo.CycleNbr = MonthYear;
            CustInfo.ApplyFor = ApplyTo;
            CustInfo.FromDate = FromDay;
            CustInfo.ToDate = ToDay;
            if (handle == "")
            {
                CustInfo.Status = Status;
            }
            else
            {
                CustInfo.Status = handle;
            }
            CustInfo.LUpd_DateTime = DateTime.Now;
            CustInfo.LUpd_Prog = _screenNbr;
            CustInfo.LUpd_User = Current.UserName;
        }

        private void Save_Reality(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();

            StoreDataHandler dataReality = new StoreDataHandler(data["lstRealitySave"]);
            ChangeRecords<OM20320_pgReality_Result> lstRealtyDetail = dataReality.BatchObjectData<OM20320_pgReality_Result>();
            lstRealtyDetail.Created.AddRange(lstRealtyDetail.Updated);
            foreach (OM20320_pgReality_Result deleted in lstRealtyDetail.Deleted)
            {
                if (lstRealtyDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.LineRef == deleted.LineRef).Count() > 0)
                {
                    lstRealtyDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.LineRef == deleted.LineRef).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedLevel.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.LineRef == deleted.LineRef);
                    if (del != null)
                    {
                        _db.OM_AccumulatedLevel.DeleteObject(del);
                    }
                }
            }
            var srvQuestionCpnyHandler = new StoreDataHandler(data["lstRealityDetailAll"]);
            var lstsrvQuestionReality = srvQuestionCpnyHandler.ObjectData<OM20320_pgReality_Result>().ToList();

            var lstDel = _db.OM_AccumulatedLevel.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionReality.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.LineRef == lstDel[i].LineRef);
                if (objDel == null)
                {
                    _db.OM_AccumulatedLevel.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM20320_pgReality_Result curBranch in lstRealtyDetail.Created)
            {
                if (curBranch.LineRef.PassNull() == "") continue;
                var AccumulatedCust = _db.OM_AccumulatedLevel.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.LineRef == curBranch.LineRef);
                if (curBranch.ToPercent <= curBranch.FromPercent && curBranch.ToPercent != 0)
                {
                    throw new MessageException(MessageType.Message, "23112018", "", new string[] { Util.GetLang("OM20320FromPercent") }); 
                }
                if (AccumulatedCust == null)
                {
                    AccumulatedCust = new OM_AccumulatedLevel();
                    AccumulatedCust.ResetET();
                    AccumulatedCust.AccumulateID = accumulateID;
                    AccumulatedCust.LineRef = curBranch.LineRef;
                    AccumulatedCust.Crtd_DateTime = DateTime.Now;
                    AccumulatedCust.Crtd_Prog = _screenNbr;
                    AccumulatedCust.Crtd_User = Current.UserName;
                    _db.OM_AccumulatedLevel.AddObject(AccumulatedCust);
                }
                AccumulatedCust.FromPercent = curBranch.FromPercent;
                AccumulatedCust.ToPercent = curBranch.ToPercent;
                AccumulatedCust.LUpd_DateTime = DateTime.Now;
                AccumulatedCust.LUpd_Prog = _screenNbr;
                AccumulatedCust.LUpd_User = Current.UserName;
            }
        }

        private void Save_RealityAmt(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();

            StoreDataHandler dataRealityAmt = new StoreDataHandler(data["lstRealityAmtSave"]);
            ChangeRecords<OM20320_pgAccuAmt_Result> lstRealtyAmtDetail = dataRealityAmt.BatchObjectData<OM20320_pgAccuAmt_Result>();
            lstRealtyAmtDetail.Created.AddRange(lstRealtyAmtDetail.Updated);
            foreach (OM20320_pgAccuAmt_Result deleted in lstRealtyAmtDetail.Deleted)
            {
                if (lstRealtyAmtDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.LineRef == deleted.LineRef).Count() > 0)
                {
                    lstRealtyAmtDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.LineRef == deleted.LineRef).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedLevelAmt.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.LineRef == deleted.LineRef);
                    if (del != null)
                    {
                        _db.OM_AccumulatedLevelAmt.DeleteObject(del);
                    }
                }
            }
            var srvQuestionRealityAmtHandler = new StoreDataHandler(data["lstRealityAmtDetailAll"]);
            var lstsrvQuestionRealityAmt = srvQuestionRealityAmtHandler.ObjectData<OM20320_pgAccuAmt_Result>().ToList();

            var lstDel = _db.OM_AccumulatedLevelAmt.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionRealityAmt.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.LineRef == lstDel[i].LineRef);
                if (objDel == null)
                {
                    _db.OM_AccumulatedLevelAmt.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM20320_pgAccuAmt_Result curBranch in lstRealtyAmtDetail.Created)
            {
                if (curBranch.LineRef.PassNull() == "") continue;
                var AccumulatedCust = _db.OM_AccumulatedLevelAmt.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.LineRef == curBranch.LineRef);
                if (curBranch.To <= curBranch.From && curBranch.To != 0)
                {
                    throw new MessageException(MessageType.Message, "2018122050", "", new string[] { Util.GetLang("OM20320BonusLevel") });
                }
                if (AccumulatedCust == null)
                {
                    AccumulatedCust = new OM_AccumulatedLevelAmt();
                    AccumulatedCust.ResetET();
                    AccumulatedCust.AccumulateID = accumulateID;
                    AccumulatedCust.LineRef = curBranch.LineRef;
                    AccumulatedCust.From = curBranch.From;
                    AccumulatedCust.To = curBranch.To;
                    AccumulatedCust.Crtd_DateTime = DateTime.Now;
                    AccumulatedCust.Crtd_Prog = _screenNbr;
                    AccumulatedCust.Crtd_User = Current.UserName;
                    _db.OM_AccumulatedLevelAmt.AddObject(AccumulatedCust);
                }
                AccumulatedCust.Pct = curBranch.Pct;
                AccumulatedCust.LUpd_DateTime = DateTime.Now;
                AccumulatedCust.LUpd_Prog = _screenNbr;
                AccumulatedCust.LUpd_User = Current.UserName;
            }
        }

        private void Save_RealityInvt(FormCollection data, string accumulateID)
        {
            string AccumulateID = accumulateID.ToUpper();

            StoreDataHandler dataRealityInvt = new StoreDataHandler(data["lstRealityInvtSave"]);
            ChangeRecords<OM20320_pgProduct_Result> lstRealtyInvtDetail = dataRealityInvt.BatchObjectData<OM20320_pgProduct_Result>();
            lstRealtyInvtDetail.Created.AddRange(lstRealtyInvtDetail.Updated);
            foreach (OM20320_pgProduct_Result deleted in lstRealtyInvtDetail.Deleted)
            {
                if (lstRealtyInvtDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == AccumulateID.ToUpper().Trim() && p.LineRef == deleted.LineRef).Count() > 0)
                {
                    lstRealtyInvtDetail.Created.Where(p => p.AccumulateID.ToUpper().Trim() == deleted.AccumulateID.ToUpper().Trim() && p.LineRef == deleted.LineRef).FirstOrDefault().tstamp = deleted.tstamp;
                }
                else
                {
                    var del = _db.OM_AccumulatedLevelInvt.FirstOrDefault(p => p.AccumulateID == deleted.AccumulateID && p.LineRef == deleted.LineRef && p.InvtID == deleted.InvtID);
                    if (del != null)
                    {
                        _db.OM_AccumulatedLevelInvt.DeleteObject(del);
                    }
                }
            }
            var srvQuestionRealityInvtHandler = new StoreDataHandler(data["lstRealityInvtDetailAll"]);
            var lstsrvQuestionRealityInvt = srvQuestionRealityInvtHandler.ObjectData<OM20320_pgProduct_Result>().ToList();

            var lstDel = _db.OM_AccumulatedLevelInvt.Where(p => p.AccumulateID == accumulateID).ToList();
            for (int i = 0; i < lstDel.Count; i++)
            {
                var objDel = lstsrvQuestionRealityInvt.FirstOrDefault(p => p.AccumulateID == lstDel[i].AccumulateID
                                && p.LineRef == lstDel[i].LineRef);
                if (objDel == null)
                {
                    _db.OM_AccumulatedLevelInvt.DeleteObject(lstDel[i]);
                }
            }

            foreach (OM20320_pgProduct_Result curBranch in lstRealtyInvtDetail.Created)
            {
                if (curBranch.LineRef.PassNull() == "") continue;
                var AccumulatedCust = _db.OM_AccumulatedLevelInvt.FirstOrDefault(p => p.AccumulateID.ToUpper() == AccumulateID.ToUpper() && p.LineRef == curBranch.LineRef && p.InvtID == curBranch.InvtID);
                if (AccumulatedCust == null)
                {
                    AccumulatedCust = new OM_AccumulatedLevelInvt();
                    AccumulatedCust.ResetET();
                    AccumulatedCust.AccumulateID = accumulateID;
                    AccumulatedCust.LineRef = curBranch.LineRef;
                    AccumulatedCust.InvtID = curBranch.InvtID;
                    AccumulatedCust.Crtd_DateTime = DateTime.Now;
                    AccumulatedCust.Crtd_Prog = _screenNbr;
                    AccumulatedCust.Crtd_User = Current.UserName;
                    _db.OM_AccumulatedLevelInvt.AddObject(AccumulatedCust);
                }
                AccumulatedCust.Unit = curBranch.Unit;
                AccumulatedCust.Pct = curBranch.Pct;
                AccumulatedCust.LUpd_DateTime = DateTime.Now;
                AccumulatedCust.LUpd_Prog = _screenNbr;
                AccumulatedCust.LUpd_User = Current.UserName;
            }
        }

        public ActionResult DeleteAllAccumulateID(string accumulateID,string status)
        {
            try
            {
                if(status == "C")
                {
                    throw new MessageException(MessageType.Message, "2018122051", "", new string[] { accumulateID });
                }
                var display = _db.OM_Accumulated.FirstOrDefault(p => p.AccumulateID == accumulateID);
                if (display != null)
                {
                    _db.OM_Accumulated.DeleteObject(display);

                    var customer = _db.OM_AccumulatedCustomer.Where(c => c.AccumulateID == accumulateID).ToList();
                    foreach (var cus in customer)
                    {
                        _db.OM_AccumulatedCustomer.DeleteObject(cus);
                    }

                    var levels = _db.OM_AccumulatedLevel.Where(l => l.AccumulateID == accumulateID).ToList();
                    foreach (var level in levels)
                    {
                        _db.OM_AccumulatedLevel.DeleteObject(level);
                    }
                    var levelAmt = _db.OM_AccumulatedLevelAmt.Where(l => l.AccumulateID == accumulateID).ToList();
                    foreach (var amt in levelAmt)
                    {
                        _db.OM_AccumulatedLevelAmt.DeleteObject(amt);
                    }
                    var invts = _db.OM_AccumulatedLevelInvt.Where(l => l.AccumulateID == accumulateID).ToList();
                    foreach (var it in invts)
                    {
                        _db.OM_AccumulatedLevelInvt.DeleteObject(it);
                    }
                    var checkhaveManage = _db.OM_AccumulatedManage.Where(l => l.AccumulateID == accumulateID).ToList();
                    var checkhavedata = _db.OM_AccumulatedReward.Where(l => l.AccumulateID == accumulateID).ToList();
                    if (checkhaveManage.Count > 0 || checkhaveManage.Count > 0)
                    {
                        throw new MessageException(MessageType.Message, "2018121750", "", new string[] { accumulateID });
                    }
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("DisplayID") });
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

        #endregion

        #endregion

    } //end controller 
}//end namespace

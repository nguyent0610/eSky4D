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
namespace OM21500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21500Controller : Controller
    {
        private string _screenNbr = "OM21500";
        private string _userName = Current.UserName;
        OM21500Entities _db = Util.CreateObjectContext<OM21500Entities>(false);
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
        // Get Grid DiscDescr
        public ActionResult GetOM_DiscDescr()
        {
            return this.Store(_db.OM21500_pgLoadGrid(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        // Get Grid DiscDescr Company
        public ActionResult GetOM_DiscDescrCpny(string discCode)
        {
            return this.Store(_db.OM21500_pgDiscCompany(discCode, Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        // Get Grid DiscDescr Invt
        public ActionResult GetOM_DiscDescrInvt(string discCode)
        {
            return this.Store(_db.OM21500_pgDiscDescItem(discCode, Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                // List header
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_DiscDescr"]);
                ChangeRecords<OM21500_pgLoadGrid_Result> lstHeader = dataHandler.BatchObjectData<OM21500_pgLoadGrid_Result>();
                // List details
                StoreDataHandler dataDetHandler = new StoreDataHandler(data["lstCpny"]);
                List<OM21500_pgDiscCompany_Result> lstDetails = dataDetHandler.ObjectData<OM21500_pgDiscCompany_Result>().ToList();
                // List details det
                StoreDataHandler dataDetsHandler = new StoreDataHandler(data["lstCpnyDel"]);
                var lstT = dataDetsHandler.ObjectData<OM21500_pgDiscCompany_Result>();
                List<OM21500_pgDiscCompany_Result> lstDetDel = new List<OM21500_pgDiscCompany_Result>();
                if (lstT != null)
                {
                    lstDetDel = new List<OM21500_pgDiscCompany_Result>(lstT);
                }
                // List details
                StoreDataHandler dataItemHandler = new StoreDataHandler(data["lstItem"]);
                List<OM21500_pgDiscDescItem_Result> lstItem = dataItemHandler.ObjectData<OM21500_pgDiscDescItem_Result>().ToList();
                // List details det
                StoreDataHandler dataDelItemHandler = new StoreDataHandler(data["lstItemDel"]);
                var lstItemT = dataDelItemHandler.ObjectData<OM21500_pgDiscDescItem_Result>();
                List<OM21500_pgDiscDescItem_Result> lstDelItem = new List<OM21500_pgDiscDescItem_Result>();
                if (lstItemT != null)
                {
                    lstDelItem = new List<OM21500_pgDiscDescItem_Result>(lstItemT);
                }
                #region -Header-                                
                lstHeader.Created.AddRange(lstHeader.Updated);
                // Delete or update tstamp header
                foreach (OM21500_pgLoadGrid_Result deleted in lstHeader.Deleted)
                {
                    if (lstHeader.Created.Where(p => p.DiscCode == deleted.DiscCode).Count() > 0)
                    {
                        lstHeader.Created.Where(p => p.DiscCode == deleted.DiscCode).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.OM_DiscDescr.Where(p => p.DiscCode == deleted.DiscCode).FirstOrDefault();
                        if (del != null)
                        {
                            // Delete Company
                            var lstDetailsDel1 = _db.OM_DiscDescCpny.Where(x => x.DiscCode == del.DiscCode).ToList();
                            for (int i = 0; i < lstDetailsDel1.Count; i++)
                            {
                                _db.OM_DiscDescCpny.DeleteObject(lstDetailsDel1[i]);
                            }
                            //var lstDetailsDel2 = lstDetails.Where(x => x.DiscCode == del.DiscCode).ToList();
                            //while (lstDetailsDel2.Count > 0)
                            //{
                            //    lstDetails.Remove(lstDetailsDel2.FirstOrDefault());
                            //}
                            // Delete Item
                            var lstItemDel1 = _db.OM_DiscDescrItem.Where(x => x.DiscCode == del.DiscCode).ToList();
                            for (int i = 0; i < lstItemDel1.Count; i++)
                            {
                                _db.OM_DiscDescrItem.DeleteObject(lstItemDel1[i]);
                            }
                            //var lstItemDel2 = lstItem.Where(x => x.DiscCode == del.DiscCode).ToList();
                            //while (lstItemDel2.Count > 0)
                            //{
                            //    lstItem.Remove(lstItemDel2.FirstOrDefault());
                            //}
                            // Delete Header
                            _db.OM_DiscDescr.DeleteObject(del);
                        }
                    }                    
                }
                // Update or add new header
                foreach (OM21500_pgLoadGrid_Result crrHeader in lstHeader.Created)
                {
                    if (crrHeader.DiscCode.PassNull() == "") continue;                    
                    var objHeader = _db.OM_DiscDescr.Where(p => p.DiscCode.ToLower() == crrHeader.DiscCode.ToLower()).FirstOrDefault();

                    if (objHeader != null)
                    {
                        if (objHeader.tstamp.ToHex() == crrHeader.tstamp.ToHex())
                        {
                            Update_DiscDescr(objHeader, crrHeader, false);                            
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {

                            objHeader = new OM_DiscDescr();
                            Update_DiscDescr(objHeader, crrHeader, true);
                            _db.OM_DiscDescr.AddObject(objHeader);
                    }
                }
                #endregion

                #region -Save, Del Cpny-
                // Del Cpny
                for (int i = 0; i < lstDetDel.Count; i++)
                {
                    if (lstDetails.Where(p => p.DiscCode == lstDetDel[i].DiscCode && p.CpnyID == lstDetDel[i].CpnyID).Count() == 0)
                    {
                        var objD = lstDetDel[i];
                        var del = _db.OM_DiscDescCpny.Where(p => p.DiscCode == objD.DiscCode && p.CpnyID == objD.CpnyID).FirstOrDefault();
                        if (del != null)
                        {
                            _db.OM_DiscDescCpny.DeleteObject(del);
                        }
                    }
                }
                // Add cpny
                for (int i = 0; i < lstDetails.Count; i++)
                {
                    var obj = lstDetails[i];
                    var objDet = _db.OM_DiscDescCpny.FirstOrDefault(x =>
                        x.DiscCode.ToLower() == obj.DiscCode.ToLower() &&
                        x.CpnyID == obj.CpnyID);
                    if (objDet == null)
                    {
                        objDet = new OM_DiscDescCpny();
                        objDet.CpnyID = obj.CpnyID;
                        objDet.DiscCode = obj.DiscCode;
                        _db.OM_DiscDescCpny.AddObject(objDet);
                    }
                }
                #endregion
                #region -Save Del Item-
                // Del Item
                for (int i = 0; i < lstDelItem.Count; i++)
                {
                    if (lstItem.Where(p => p.DiscCode == lstDelItem[i].DiscCode && p.InvtID == lstDelItem[i].InvtID && p.InvtType == lstDelItem[i].InvtType).Count() == 0)
                    {
                        var objD = lstDelItem[i];
                        var del = _db.OM_DiscDescrItem.Where(p => p.DiscCode == objD.DiscCode && p.InvtID == objD.InvtID && p.InvtType == objD.InvtType).FirstOrDefault();
                        if (del != null)
                        {
                            _db.OM_DiscDescrItem.DeleteObject(del);
                        }
                    }
                    else
                    {
                        lstItem.Where(p => p.DiscCode == lstDelItem[i].DiscCode && p.InvtID == lstDelItem[i].InvtID && p.InvtType == lstDelItem[i].InvtType).FirstOrDefault().tstamp = lstDelItem[i].tstamp;
                    }
                }
                // Add cpny
                for (int i = 0; i < lstItem.Count; i++)
                {
                    var objD = lstItem[i];
                    var objDet = _db.OM_DiscDescrItem.FirstOrDefault(p => p.DiscCode == objD.DiscCode && p.InvtID == objD.InvtID && p.InvtType == objD.InvtType);
                    if (objDet == null)
                    {
                        objDet = new OM_DiscDescrItem();
                        objDet.InvtID = objD.InvtID;
                        objDet.DiscCode = objD.DiscCode;
                        objDet.InvtType = objD.InvtType;
                        objDet.Crtd_DateTime = DateTime.Now;
                        objDet.Crtd_Prog = _screenNbr;
                        objDet.Crtd_User = Current.UserName;
                        _db.OM_DiscDescrItem.AddObject(objDet);
                    }
                    objDet.Mark = objD.Mark;
                    objDet.LUpd_DateTime = DateTime.Now;
                    objDet.LUpd_Prog = _screenNbr;
                    objDet.LUpd_User = Current.UserName;
                    objDet.tstamp = new byte[1];
                }
                #endregion
                // Save all change
                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
            }
        }
        private void Update_DiscDescr(OM_DiscDescr t, OM21500_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.DiscCode = s.DiscCode;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
                t.BudgetID = "";
            }
            t.PromoType = s.PromoType;
            t.ObjApply = s.ObjApply;
            t.Descr = s.Descr;
            t.Active = s.Active;
            t.DiscType = s.DiscType;
            t.FromDate = s.FromDate;
            t.ToDate = s.ToDate;
            t.ApplyFor = s.ApplyFor;
            if (s.ApplyFor == "" || s.ApplyFor == null)
            {
                throw new MessageException("2018040260", new[] { Util.GetLang("ApplyFor") });
            }
            t.POTime=s.POTime;
            t.OMTime = s.OMTime;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [DirectMethod]
        public ActionResult OM21500GetTreeBranch(string panelID)
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

            var lstTerritories = _db.OM21500_ptTerritory(Current.UserName, Current.CpnyID, Current.LangID).ToList();//tam thoi
            var companies = _db.OM21500_ptCompany(Current.UserName, Current.CpnyID, Current.LangID).ToList();

            if (lstTerritories.Count == 0)
            {
                node.Leaf = true;
            }

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
            tree.Listeners.CheckChange.Fn = "treePanelBranch_checkChange";

            tree.AddTo(treeBranch);

            return this.Direct();
        }

        [DirectMethod]
        public ActionResult OM22001GetInvt(string panelID)
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
            tree.Fields.Add(new ModelField("InvtType", ModelFieldType.String));
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
            tree.Listeners.BeforeItemExpand.Handler = "App.treePanelInvt.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "App.treePanelInvt.el.unmask();Ext.resumeLayouts(true);";
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
                    var invts = _db.OM21500_ptInventory(Current.UserName, Current.CpnyID, Current.LangID).ToList().Where(i => i.NodeID == inactiveHierachy.NodeID && i.NodeLevel == inactiveHierachy.NodeLevel && i.ParentRecordID == inactiveHierachy.ParentRecordID).ToList();
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

    }
}

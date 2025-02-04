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
namespace IN22001.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN22001Controller : Controller
    {
        private string _screenNbr = "IN22001";
        IN22001Entities _db = Util.CreateObjectContext<IN22001Entities>(false);
        List<IN22001_ptTreeNodeCpny_Result> lstAllNodeCpny = new List<IN22001_ptTreeNodeCpny_Result>();
        // GET: /IN22001/
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetPosmInfo(string posmID)
        {
            var posm = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID == posmID);
            return this.Store(posm);
        }

        public ActionResult GetBranch(string posmID)
        {
            var dets = _db.IN22001_pgBranch(Current.UserName, Current.CpnyID, Current.LangID, posmID).ToList();
            return this.Store(dets);
        }

        public ActionResult SaveData(FormCollection data, bool isNew)
        {
            try
            {
                var posmID = data["cboPosmID"];

                var posmHandler = new StoreDataHandler(data["lstPosm"]);
                var posmInput = posmHandler.ObjectData<IN_POSMHeader>().FirstOrDefault();

                if (posmInput != null && !string.IsNullOrWhiteSpace(posmID))
                {
                    posmInput.PosmID = posmID;
                    var posm = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID == posmInput.PosmID);

                    if (posm != null)
                    {
                        if (isNew)
                        {
                            throw new MessageException(MessageType.Message, "8001", "",
                                new string[]{
                                    Util.GetLang("POSMID")
                                });
                        }
                        else
                        {
                            if (posm.tstamp.ToHex() == posmInput.tstamp.ToHex())
                            {
                                updatePosm(ref posm, posmInput, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                    }
                    else
                    {
                        posm = new IN_POSMHeader();
                        updatePosm(ref posm, posmInput, true);
                        _db.IN_POSMHeader.AddObject(posm);
                    }

                    #region Branch
                    var custHandler = new StoreDataHandler(data["lstDetChange"]);
                    var lstDetChange = custHandler.BatchObjectData<IN22001_pgBranch_Result>();

                    foreach (var created in lstDetChange.Created)
                    {
                        if (!string.IsNullOrWhiteSpace(created.BranchID))
                        {
                            created.PosmID = posmInput.PosmID;

                            var createdCpny = _db.IN_POSMBranch.FirstOrDefault(
                                x => x.BranchID == created.BranchID
                                    && x.PosmID == created.PosmID);
                            if (createdCpny == null)
                            {
                                createdCpny = new IN_POSMBranch();
                                updateBranch(ref createdCpny, created, true);
                                _db.IN_POSMBranch.AddObject(createdCpny);
                            }
                        }
                    }

                    foreach (var updated in lstDetChange.Updated)
                    {
                        if (!string.IsNullOrWhiteSpace(updated.BranchID))
                        {
                            updated.PosmID = posmInput.PosmID;

                            var updatedCpny = _db.IN_POSMBranch.FirstOrDefault(
                                x => x.BranchID == updated.BranchID
                                    && x.PosmID == updated.PosmID);
                            if (updatedCpny != null)
                            {
                                updateBranch(ref updatedCpny, updated, true);
                            }
                        }
                    }

                    foreach (var deleted in lstDetChange.Deleted)
                    {
                        if (!string.IsNullOrWhiteSpace(deleted.BranchID))
                        {
                            deleted.PosmID = posmInput.PosmID;

                            var deletedCpny = _db.IN_POSMBranch.FirstOrDefault(
                                x => x.BranchID == deleted.BranchID
                                    && x.PosmID == deleted.PosmID);
                            if (deletedCpny != null)
                            {
                                _db.IN_POSMBranch.DeleteObject(deletedCpny);
                            }
                        }
                    }
                    #endregion

                    _db.SaveChanges();
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

        public ActionResult DeletePosm(string posmID)
        {
            try
            {
                var posm = _db.IN_POSMHeader.FirstOrDefault(p => p.PosmID == posmID);
                if (posm != null)
                {
                    _db.IN_POSMHeader.DeleteObject(posm);

                    var cpnies = _db.IN_POSMBranch.Where(c => c.PosmID == posmID).ToList();
                    foreach (var cpny in cpnies)
                    {
                        _db.IN_POSMBranch.DeleteObject(cpny);
                    }
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("PosmID") });
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

        private void updateBranch(ref IN_POSMBranch createdCpny, IN22001_pgBranch_Result created, bool isNew)
        {
            if (isNew)
            {
                createdCpny.PosmID = created.PosmID;
                createdCpny.BranchID = created.BranchID;
                createdCpny.Zone = "";
                createdCpny.Territory = "";
                createdCpny.FCS = created.FCS;

                createdCpny.Crtd_DateTime = DateTime.Now;
                createdCpny.Crtd_Prog = _screenNbr;
                createdCpny.Crtd_User = Current.UserName;
            }
            createdCpny.LUpd_DateTime = DateTime.Now;
            createdCpny.LUpd_Prog = _screenNbr;
            createdCpny.LUpd_User = Current.UserName;
        }

        private void updatePosm(ref IN_POSMHeader posm, IN_POSMHeader posmInput, bool isNew)
        {
            if (isNew)
            {
                posm.PosmID = posmInput.PosmID;
                posm.Crtd_DateTime = DateTime.Now;
                posm.Crtd_Prog = _screenNbr;
                posm.Crtd_User = Current.UserName;
            }
            posm.FromDate = posmInput.FromDate;
            posm.ToDate = posmInput.ToDate;
            posm.Active = posmInput.Active;
            posm.LUpd_DateTime = DateTime.Now;
            posm.LUpd_Prog = _screenNbr;
            posm.LUpd_User = Current.UserName;
        }
    //---- tree---
        [DirectMethod]
        public ActionResult IN22001GetTreeBranch(string panelID)
        {

            TreePanel tree = new TreePanel();
            tree.ID = "treePanelCompany";
            tree.ItemID = "treePanelConpany";

            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Zone", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Territory", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CpnyID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CpnyName", ModelFieldType.String));
            tree.Fields.Add(new ModelField("CpnyType", ModelFieldType.String));
            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
            node.Checked = false;

            lstAllNodeCpny = _db.IN22001_ptTreeNodeCpny(Current.UserName, Current.CpnyID, Current.LangID).ToList();


            var maxLevel = lstAllNodeCpny.Max(x => x.LevelID);
            var lstFirst = lstAllNodeCpny.Where(x => x.LevelID == maxLevel).ToList();
            var crrLevel = maxLevel - 1;
            if (lstFirst.Count > 0)
            {
                string crrParent = string.Empty;
                Node parentNode = null;
                bool isAddChild = false;
                foreach (var it in lstFirst)
                {
                    var childNode = SetNodeValueCpny(it, Ext.Net.Icon.Folder);
                    GetChildNodeCpny(ref childNode, (int)crrLevel, it.Code);

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

            tree.Listeners.CheckChange.Fn = "treePanelCompany_checkChange";
            tree.Listeners.ItemCollapse.Fn = "tree_ItemCollapse";
            tree.AddTo(treeBranch);

            return this.Direct();
        }
        private Node SetNodeValueCpny(IN22001_ptTreeNodeCpny_Result objNode, Ext.Net.Icon icon)
        {
            Node node = new Node();

            Random rand = new Random();
            node.NodeID = objNode.Code + objNode.ParentID + (rand.Next(999, 9999) + objNode.LevelID).ToString();
            node.Checked = false;
            node.Text = objNode.Descr;
            node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = objNode.Type, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = objNode.Code, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyName", Value = objNode.Descr, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyType", Value = objNode.CpnyType, Mode = Ext.Net.ParameterMode.Value });

            if (objNode.LevelID == 0)
            {
                var objTerritory = lstAllNodeCpny.FirstOrDefault(x => x.Code == objNode.ParentID);
                if (objTerritory != null)
                {
                    node.CustomAttributes.Add(new ConfigItem() { Name = "Territory", Value = objNode.ParentID, Mode = Ext.Net.ParameterMode.Value });
                    node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = objNode.Code, Mode = Ext.Net.ParameterMode.Value });
                    node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyName", Value = objNode.Descr, Mode = Ext.Net.ParameterMode.Value });
                    node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyType", Value = objNode.CpnyType, Mode = Ext.Net.ParameterMode.Value });
                }

            }
            node.Icon = objNode.LevelID != 0 ? icon : Ext.Net.Icon.Folder;
            node.Leaf = objNode.LevelID == 0;// true;
            node.IconCls = "tree-node-noicon";
            return node;
        }
        private void GetChildNodeCpny(ref Node crrNode, int level, string parrentID)
        {
            if (level >= 0)
            {
                var lstSub = lstAllNodeCpny.Where(x => x.ParentID == parrentID && x.LevelID == level).ToList();

                if (lstSub.Count > 0)
                {
                    var crrLevel = level - 1;
                    string crrParent = string.Empty;
                    foreach (var it in lstSub)
                    {
                        var childNode = SetNodeValueCpny(it, Ext.Net.Icon.FolderGo);
                        GetChildNodeCpny(ref childNode, crrLevel, it.Code);
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
    }
}

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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;

namespace SA03001.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA03001Controller : Controller
    {
        private string _screenNbr = "SA03001";
        private string _userName = Current.UserName;
        SA03001Entities _db = Util.CreateObjectContext<SA03001Entities>(true);
        private JsonResult _logMessage;
        private bool isShowUserTypes = false;
        private bool isRequiredCpny = false;
        //List<SA03001_ptTreeNode_Result> lstAllNode = new List<SA03001_ptTreeNode_Result>();
        //bool isChecked = true;
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            var objUserTypes = _db.SA03001_pdConfigHideShow(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if(objUserTypes!= null)
            {
                isShowUserTypes = objUserTypes.UserTypes.ToBool();
                isRequiredCpny = objUserTypes.CpnyID.ToBool();
            }
            ViewBag.IsShowUserTypes = isShowUserTypes;
            ViewBag.IsRequiredCpny = isRequiredCpny;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult LoadGrid()
        {
            return this.Store(_db.SA03001_pgLoadGrid(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }

        public ActionResult LoadBranch(string listBranchID) 
        {
            return this.Store(_db.SA03001_pgBranchAllByUser(Current.CpnyID, Current.UserName, Current.LangID,listBranchID).ToList());
        }

        public ActionResult LoadForm(string userName)
        {
            var item = _db.Users.FirstOrDefault(p => p.UserName == userName);
            item.Password = item.Password == "" ? "" : Encryption.Decrypt(item.Password.ToString(), "1210Hq10s081f359t");
            return this.Store(item);
        }

        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstUser"]);
                User objTemp = new User();
                objTemp = dataHandler.ObjectData<User>().FirstOrDefault();
                string userName = data["txtUserName"].PassNull();
                string firstName = data["txtFirstName"].PassNull();
                string passWord = data["txtPassword"].PassNull();
                string email = data["txtEmail"].PassNull();
                string cpnyID = data["lstCpnyID"].PassNull();
                string userType = data["cboUserTypes"].PassNull();
                string userGroup = data["cboUserGroup"].PassNull();
                string blocked = data["valueBloked"].PassNull();
                string status = data["cboStatus"].PassNull();
                string position = data["cboPosition"].PassNull();
                //string tmpStartDate = data["valueStartDate"].PassNull();
                //string tmpEndDate = data["valueEndDate"].PassNull();
                string valueTstamp = data["valueTstamp"].PassNull();
                //DateTime StartDate = DateTime.Parse(tmpStartDate);
                //DateTime EndDate = DateTime.Parse(tmpEndDate);
                string tmpBeginDay = data["dtpBeginDay"].PassNull();
                DateTime BeginDay = DateTime.Parse(tmpBeginDay);
                int ExpireDay = data["txtExpireDay"].ToInt();
                DateTime StartWork = data["dtpStartWork"].ToDateShort();
                DateTime EndWork = data["dtpEndWork"].ToDateShort();

                var lstdataErro = _db.SA03001_pdCheckSaveUser(userName, cpnyID, userType, Current.CpnyID, Current.UserName, Current.LangID).ToList();
                if (lstdataErro.Count>0)
                {
                    string strUserID = "";
                    string strBranchID = "";
                    string strTypeUser = "";
                    foreach (var item in lstdataErro)
                    {
                        strUserID = strUserID + item.UserID + ",";
                        strBranchID = strBranchID + item.BranchID + ",";
                        strTypeUser = strTypeUser + item.TypeUser + ",";
                    }
                    if (strUserID!="" && strBranchID!="" && strTypeUser!="")
                    {
                        string messageerorr = string.Format(Message.GetString("2018032711", null), strUserID, strBranchID, strTypeUser);
                        throw new MessageException(MessageType.Message, "20410", "", new string[] { messageerorr });
                    }
                }
                bool isNewUser = false;

                if (data["isNewUser"].PassNull() == "true")
                {
                    isNewUser = true;
                }
                bool isBlock = false;
                if (blocked == "true")
                {
                    isBlock = true;
                }
                int failedLoginCount = Convert.ToInt32(data["txtFailedLoginCount"].PassNull() == "" ? "0" : data["txtFailedLoginCount"].PassNull());
                string manager = data["cboManager"].PassNull();

                var objUser = _db.Users.FirstOrDefault(p => p.UserName == userName);
                if (isNewUser == true)
                {
                    if (objUser != null)
                    {
                        throw new MessageException(MessageType.Message, "201308241" ,"", new[] {Util.GetLang("UserName"), userName });
                    }
                }
                bool isNew = false;
                if (objUser == null)
                {
                    isNew = true;
                    objUser = new User();
                    objUser.ResetET();
                    objUser.UserName = userName;
                    objUser.Crtd_Datetime = DateTime.Now;
                    objUser.Crtd_Prog = _screenNbr;
                    objUser.Crtd_User = _userName;
                    objUser.Address = ".";
                    objUser.LastName = ".";
                    objUser.Channel = "DMS";
                    objUser.CheckFirstLogin = false;
                    //objUser.ExpireDay = 90;
                    objUser.BeginDay = DateTime.Now;
                    objUser.LoggedIn = true;
                }
                else
                {
                    if (objUser.tstamp.ToHex() != valueTstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
               
                var listUserGroup = _db.SYS_UserGroup.Where(p => p.UserID == userName).ToList();
                foreach(var item in listUserGroup)
                {
                    if(!userGroup.PassNull().Split(',').Contains(item.GroupID))
                        _db.DeleteObject(item);
                }
                foreach (var userGroupItem in userGroup.PassNull().Split(','))
                {
                    if (!string.IsNullOrEmpty(userGroupItem))
                    {
   
                        var objUserGroup = _db.SYS_UserGroup.FirstOrDefault(p => p.UserID == userName && p.GroupID == userGroupItem);
                        if (objUserGroup == null)
                        {
                            objUserGroup = new SYS_UserGroup();
                            objUserGroup.ResetET();
                            objUserGroup.UserID = userName;
                            objUserGroup.GroupID = userGroupItem;
                            objUserGroup.Crtd_Datetime = DateTime.Now;
                            objUserGroup.Crtd_Prog = _screenNbr;
                            objUserGroup.Crtd_User = _userName;
                            objUserGroup.LUpd_Datetime = DateTime.Now;
                            objUserGroup.LUpd_Prog = _screenNbr;
                            objUserGroup.LUpd_User = _userName;
                            _db.SYS_UserGroup.AddObject(objUserGroup);
                        }
                        else
                        {
                            objUserGroup.LUpd_Datetime = DateTime.Now;
                            objUserGroup.LUpd_Prog = _screenNbr;
                            objUserGroup.LUpd_User = _userName;
                        }
                    }
                }

                objUser.Blocked = isBlock;
                if (!string.IsNullOrEmpty(data["dteBlockedTime"]))
                    objUser.BlockedTime = Convert.ToDateTime(data["dteBlockedTime"]);
                objUser.CpnyID = cpnyID;
                objUser.Email = email;

                objUser.FailedLoginCount = failedLoginCount;
                objUser.FirstName = firstName;
                if(!isShowUserTypes)
                    objUser.UserTypes = userType;
                else
                    objUser.UserTypes = manager;
                objUser.Manager = manager;
                objUser.Status = status;
                objUser.JobTitle = position;
                objUser.StartWork = StartWork;
                objUser.EndWork = EndWork;
                objUser.BeginDay = BeginDay;
                objUser.ExpireDay = ExpireDay;
                //objUser.StartDate = StartDate;
                //objUser.EndDate = EndDate;
                if (string.IsNullOrEmpty(passWord))
                {
                    objUser.Password = Encryption.Encrypt("123456", "1210Hq10s081f359t");
                }
                else
                {
                    if (objUser.Password != passWord)
                    {
                        objUser.Password = Encryption.Encrypt(passWord, "1210Hq10s081f359t");
                    }
                }
                if (isNew == true)
                {
                    _db.Users.AddObject(objUser);
                }
                _db.SaveChanges();
                return Json(new { success = true});
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

        //[DirectMethod]
        //public ActionResult SA03001_GetTreeData(string lstbranchID)
        //{
        //    #region -Declare-
        //    Panel panel = this.GetCmp<Panel>("pnlTreeAVC");
        //    TextField txt = this.GetCmp<TextField>("txtAllCurrentSalesman");
        //    string _allCurrentSalesman = "";
        //    TreePanel tree = new TreePanel();
        //    tree.ID = "treeAVC";
        //    tree.Fields.Add(new ModelField("Data", ModelFieldType.String));
        //    tree.Fields.Add(new ModelField("Color", ModelFieldType.String));
        //    tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
        //    tree.Fields.Add(new ModelField("SlsperID", ModelFieldType.String));
        //    tree.Fields.Add(new ModelField("CpnyID", ModelFieldType.String));
        //    //tree.Fields.Add(new ModelField("Lat", ModelFieldType.Float));
        //    //tree.Fields.Add(new ModelField("Lng", ModelFieldType.Float));

        //    tree.AutoScroll = true;
        //    tree.Scroll = ScrollMode.Both;
        //    tree.RootVisible = true;

        //    tree.Border = false;
        //    tree.Header = false;
        //    //tree.Listeners.CheckChange.Fn = "tree_CheckChange";
        //    //tree.Listeners.Load.Fn = "setValueAllCurrentSalesFirsrLoad";
        //    //tree.Listeners.SelectionChange.Fn = "treeAVC_SelectionChange";
        //    //tree.Listeners.BeforeItemExpand.Fn = "treeAVC_BeforeItemExpand";
        //    //tree.Listeners.AfterItemExpand.Fn = "treeAVC_AfterItemExpand";
        //    //tree.Listeners.ItemCollapse.Fn = "treeAVC_ItemCollapse";
        //    //tree.Listeners.BeforeItemCollapse.Fn = "treeAVC_BeforeItemExpand";
        //    //tree.Listeners.AfterItemCollapse.Fn = "treeAVC_AfterItemExpand";

        //    #endregion

        //    var objBranchFirst = _db.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "SA03001BranchFirst".ToUpper());
        //    var branchFirst = objBranchFirst != null && objBranchFirst.IntVal == 1;
        //    if (branchFirst) // Show Node Branch first
        //    {
        //        Node node = new Node();
        //        node.NodeID = "tree-node-root-company";
        //        Random rand = new Random();
        //        node.Checked = true;
        //        #region -Branch-
        //        var lstBranch = _db.SA03001_pdBranch(Current.UserName, lstbranchID).ToList();//item.CpnyID
        //        string nodeType = "B";
        //        foreach (var branch in lstBranch)
        //        {
        //            var nodeBranch = new Node();
        //            nodeBranch.Checked = true;
        //            //_allCurrentSalesman += "node-branch-sup-sales-positon" + '-' + branch.BranchID + '-' + "" + '-' + "B" + ",";
        //            nodeBranch.NodeID = "node-branch-sup-sales-positon" + '-' + branch.BranchID + '-' + "" + '-' + "B";// Thêm SupID để khỏi trùng
        //            // nodeBranch.NodeID = "node-branch-" + '-' + branch.BranchID;// + item.CpnyID CpnyID
        //            nodeBranch.Text = branch.BranchID + " - " + branch.Name;
        //            nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "B", Mode = Ext.Net.ParameterMode.Value });
        //            nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = "node-branch-sup-sales-positon" + '-' + branch.BranchID + '-' + "" + '-' + "B", Mode = Ext.Net.ParameterMode.Value });// item.CpnyID + "#@#" +
        //            nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = branch.BranchID, Mode = Ext.Net.ParameterMode.Value });
        //            nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = branch.BranchID, Mode = Ext.Net.ParameterMode.Value });

        //            nodeBranch.Qtitle = "";

        //            var lstDSM = _db.SA03001_pdDSM(Current.UserName, branch.BranchID, Current.CpnyID, Current.LangID).ToList();//item.CpnyID
        //            if (lstDSM.Any(x => x.Data.EndsWith("#S")))
        //            {
        //                nodeType = "T";
        //            }
        //            else
        //            {
        //                nodeType = "M";
        //            }
        //            foreach (var dsm in lstDSM)
        //            {
        //                #region -DSM-
        //                string colorDSM = GetRandomColour(rand);
        //                var nodeDSM = new Node();
        //                nodeDSM.Checked = true;
        //                _allCurrentSalesman += dsm.Data + ",";// branch.BranchID + "#" + dsm.SlsperID + "##" + ",";
        //                nodeDSM.NodeID = dsm.Data;// "node-dsm-" + '-' + branch.BranchID + '-' + dsm.SlsperID;//+ item.CpnyID ;background-color:" + colorDSM
        //                //nodeDSM.Text = dsm.SlsperID + " - " + dsm.Name;// @"<span style="" width:15px;height:15px" + @";display:inline-block;vertical-align:middle;margin-right:5px;""></span><span style=""vertical-align: middle;"">" + dsm.SlsperId + " - " + dsm.Name + "</span>";
        //                nodeDSM.Text = @"<span style=""color: #" + dsm.Color + "\">" + dsm.SlsperID + " - " + dsm.Name + "</span>";
        //                nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = nodeType, Mode = Ext.Net.ParameterMode.Value });
        //                nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = dsm.Data, Mode = Ext.Net.ParameterMode.Value });//item.CpnyID + '-' +
        //                nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = colorDSM.Replace("#", ""), Mode = Ext.Net.ParameterMode.Value });
        //                nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = dsm.SlsperID, Mode = Ext.Net.ParameterMode.Value });
        //                nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = dsm.BranchID, Mode = Ext.Net.ParameterMode.Value });

        //                nodeDSM.Qtitle = dsm.SlsperID.ToLower() + " - " + ConvertString(dsm.Name);
        //                nodeDSM.Icon = Ext.Net.Icon.UserSuit;
        //                nodeDSM.IconCls = "tree-node-noicon";
        //                #endregion

        //                var lstDSR = _db.SA03001_pdDSR(Current.UserName, branch.BranchID, dsm.SlsperID).ToList();
        //                var nodeTypeDSR = "S";
        //                if (lstDSR.Any(x => x.Data.EndsWith("#S")))
        //                {
        //                    nodeTypeDSR = "T";
        //                }
        //                foreach (var dsr in lstDSR)
        //                {
        //                    #region -DSR-
        //                    string color = GetRandomColour(rand);
        //                    var nodeDSR = new Node();
        //                    nodeDSR.Checked = true;
        //                    _allCurrentSalesman += dsr.Data + ",";// branch.BranchID + "#" + dsm.SlsperID + '#' + dsr.SlsperID + "#" + ",";
        //                    nodeDSR.NodeID = dsr.Data;// "node-dsr-" + '-' + branch.BranchID + '-' + dsm.SlsperID + '-' + dsr.SlsperID;// + item.CpnyID ;background-color:" + color
        //                    //nodeDSR.Text = dsr.SlsperID + " - " + dsr.Name;// @"<span style="" width:15px;height:15px" + @";display:inline-block;vertical-align:middle;margin-right:5px;""></span><span style=""vertical-align: middle;"">" + dsr.SlsperID + " - " + dsr.Name + "</span>";
        //                    nodeDSR.Text = @"<span style=""color: #" + dsr.Color + "\">" + dsr.SlsperID + " - " + dsr.Name + "</span>";
        //                    nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = nodeTypeDSR, Mode = Ext.Net.ParameterMode.Value });
        //                    nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = dsr.Data, Mode = Ext.Net.ParameterMode.Value });
        //                    nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = color.Replace("#", ""), Mode = Ext.Net.ParameterMode.Value });
        //                    nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = dsr.SlsperID, Mode = Ext.Net.ParameterMode.Value });
        //                    nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = branch.BranchID, Mode = Ext.Net.ParameterMode.Value });


        //                    nodeDSR.Qtitle = dsr.SlsperID.ToLower() + " - " + ConvertString(dsr.Name);
        //                    nodeDSR.Icon = Ext.Net.Icon.UserRed;
        //                    nodeDSR.IconCls = "tree-node-noicon";
        //                    var lstTAA = _db.SA03001_pdTAA(Current.UserName, branch.BranchID, dsr.SlsperID).ToList();

        //                    foreach (var taa in lstTAA)
        //                    {
        //                        #region -TAA-
        //                        string colorTAA = GetRandomColour(rand);
        //                        var nodeTAA = new Node();
        //                        nodeTAA.Checked = true;
        //                        _allCurrentSalesman += taa.Data + ",";// branch.BranchID + "#" + dsm.SlsperID + '#' + dsr.SlsperID + '#' + taa.PGID + ",";
        //                        nodeTAA.NodeID = taa.Data;// "node-dsr-" + '-' + branch.BranchID + '-' + dsm.SlsperID + '-' + dsr.SlsperID + '-' + taa.PGID;// + item.CpnyID ;background-color:" + color
        //                        //nodeTAA.Text = taa.PGID + " - " + taa.PGName;// @"<span style="" width:15px;height:15px" + @";display:inline-block;vertical-align:middle;margin-right:5px;""></span><span style=""vertical-align: middle;"">" + taa.PGID + " - " + taa.PGName + "</span>";
        //                        nodeTAA.Text = @"<span style=""color: #" + taa.Color + "\">" + taa.PGID + " - " + taa.PGName + "</span>";
        //                        nodeTAA.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "T", Mode = Ext.Net.ParameterMode.Value });
        //                        nodeTAA.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = taa.Data, Mode = Ext.Net.ParameterMode.Value });//item.CpnyID + '-' + 
        //                        nodeTAA.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = color.Replace("#", ""), Mode = Ext.Net.ParameterMode.Value });
        //                        nodeTAA.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = taa.PGID, Mode = Ext.Net.ParameterMode.Value });
        //                        nodeTAA.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = branch.BranchID, Mode = Ext.Net.ParameterMode.Value });

        //                        nodeTAA.Qtitle = taa.PGID.ToLower() + " - " + ConvertString(taa.PGName);
        //                        nodeTAA.Icon = Ext.Net.Icon.UserGreen;
        //                        nodeTAA.IconCls = "tree-node-noicon";

        //                        nodeTAA.Leaf = true;
        //                        nodeDSR.Children.Add(nodeTAA);
        //                        #endregion
        //                    }
        //                    nodeDSR.Leaf = lstTAA.Count == 0 ? true : false;
        //                    nodeDSM.Children.Add(nodeDSR);
        //                    //nodeDSR.Leaf = true;
        //                    //nodeDSM.Children.Add(nodeDSR);
        //                    #endregion
        //                }

        //                nodeDSM.Leaf = lstDSR.Count == 0 ? true : false;
        //                nodeBranch.Children.Add(nodeDSM);
        //            }

        //            nodeBranch.Leaf = lstDSM.Count == 0 ? true : false;
        //            nodeBranch.Icon = Ext.Net.Icon.House;
        //            node.Children.Add(nodeBranch);
        //        }
        //        tree.Root.Add(node);
        //        tree.AddTo(panel);
        //        txt.Value = _allCurrentSalesman;
        //        #endregion
        //    }
        //    else // Show NS first
        //    {
        //        Node node = new Node();
        //        node.Checked = true;
        //        node.NodeID = "tree-node-root-ns";
        //        Random rand = new Random();

        //        lstAllNode = _db.SA03001_ptTreeNode(Current.UserName, Current.CpnyID, Current.LangID).ToList();


        //        var maxLevel = lstAllNode.Max(x => x.Level);
        //        var lstFirst = lstAllNode.Where(x => x.Level == maxLevel).ToList();
        //        var crrLevel = maxLevel - 1;
        //        if (lstFirst.Count > 0)
        //        {
        //            string crrParent = string.Empty;
        //            Node parentNode = null;
        //            bool isAddChild = lstFirst.Where(x => x.ParentID != string.Empty).Count() > 0;
        //            foreach (var it in lstFirst)
        //            {
        //                var childNode = SetNodeValue(it, isChecked, Ext.Net.Icon.UserHome);
        //                _allCurrentSalesman += it.Data + ",";
        //                GetChildNode(ref childNode, ref  _allCurrentSalesman, crrLevel, it.SlsperID);

        //                if (it.ParentID != crrParent)
        //                {
        //                    crrParent = it.ParentID;
        //                    parentNode = SetBranchNodeValue(it, isChecked, "B");
        //                    parentNode.Children.Add(childNode);
        //                    isAddChild = true;
        //                    node.Children.Add(parentNode);
        //                }
        //                else
        //                {
        //                    if (it.ParentID != string.Empty)
        //                    {
        //                        parentNode.Children.Add(childNode);
        //                    }
        //                }
        //                if (!isAddChild)
        //                {
        //                    node.Children.Add(childNode);
        //                }
        //            }
        //        }

        //        node.Icon = Ext.Net.Icon.FolderHome;

        //        tree.Root.Add(node);

        //        tree.AddTo(panel);
        //        txt.Value = _allCurrentSalesman;
        //    }
        //    tree.ExpandAll();
        //    return this.Direct();

        //}
        //private void GetChildNode(ref Node crrNode, ref string _allCurrentSalesman, int level, string supID)
        //{
        //    if (level >= 0)
        //    {
        //        var lstSub = lstAllNode.Where(x => x.SupID == supID && x.Level == level).ToList();

        //        if (lstSub.Count > 0)
        //        {
        //            var crrLevel = level - 1;
        //            string crrParent = string.Empty;
        //            Node parentNode = null;
        //            bool isAddChild = lstSub.Where(x => x.ParentID != string.Empty).Count() > 0;
        //            foreach (var it in lstSub)
        //            {
        //                var childNode = SetNodeValue(it, isChecked, Ext.Net.Icon.UserGreen);
        //                _allCurrentSalesman += it.Data + ",";
        //                GetChildNode(ref childNode, ref _allCurrentSalesman, crrLevel, it.SlsperID);

        //                if (it.ParentID != crrParent)
        //                {
        //                    parentNode = null;
        //                    crrParent = it.ParentID;
        //                    parentNode = SetBranchNodeValue(it, isChecked, "B");
        //                    parentNode.Children.Add(childNode);
        //                    isAddChild = true;
        //                    crrNode.Children.Add(parentNode);
        //                }
        //                else
        //                {
        //                    if (it.ParentID != string.Empty)
        //                    {
        //                        parentNode.Children.Add(childNode);
        //                    }
        //                }
        //                if (!isAddChild)
        //                {
        //                    crrNode.Children.Add(childNode);
        //                }
        //            }

        //        }
        //        else
        //        {
        //            crrNode.Leaf = true;
        //        }
        //    }
        //    else
        //    {
        //        crrNode.Leaf = true;
        //    }
        //}

        //private Node SetNodeValue(SA03001_ptTreeNode_Result objNode, bool isChecked, Ext.Net.Icon icon)
        //{
        //    Node node = new Node();
        //    Random rand = new Random();
        //    string color = GetRandomColour(rand);
        //    node.Checked = isChecked;

        //    node.NodeID = objNode.Data;// "node-branch-sup-sales-position-" + '-' + objNode.SupID + '-' + objNode.SlsperID + '-' + position;// + item.CpnyID ;background-color:" + color
        //    node.Text = @"<span style=""color: #" + objNode.Color + "\">" + objNode.SlsperID + " - " + objNode.Name + "</span>";
        //    node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = objNode.NoteType, Mode = Ext.Net.ParameterMode.Value });
        //    node.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = objNode.Data, Mode = Ext.Net.ParameterMode.Value });
        //    node.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = color.Replace("#", ""), Mode = Ext.Net.ParameterMode.Value });
        //    node.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = objNode.SlsperID, Mode = Ext.Net.ParameterMode.Value });
        //    node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = "", Mode = Ext.Net.ParameterMode.Value });

        //    node.Qtitle = objNode.SlsperID.ToLower() + " - " + ConvertString(objNode.Name);
        //    node.Icon = objNode.Level != 0 ? icon : Ext.Net.Icon.UserBrown;
        //    node.Leaf = objNode.Level == 0;// true;
        //    node.IconCls = "tree-node-noicon";
        //    return node;
        //}

        //private Node SetBranchNodeValue(SA03001_ptTreeNode_Result objNode, bool isChecked, string position)
        //{
        //    Node nodeBranch = new Node();
        //    nodeBranch.Checked = isChecked;
        //    nodeBranch.NodeID = "node-branch-sup-sales-positon" + '-' + objNode.ParentID + objNode.SlsperID + '-' + objNode.SupID + '-' + position;// Thêm SupID để khỏi trùng
        //    nodeBranch.Text = objNode.ParentID + " - " + objNode.ParentName;
        //    nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = position, Mode = Ext.Net.ParameterMode.Value });
        //    nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = objNode.ParentID + "###", Mode = Ext.Net.ParameterMode.Value });
        //    nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = objNode.ParentID, Mode = Ext.Net.ParameterMode.Value });
        //    nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = objNode.ParentID, Mode = Ext.Net.ParameterMode.Value });

        //    nodeBranch.Qtitle = objNode.ParentID + " - " + objNode.ParentName;
        //    return nodeBranch;
        //}

        //public string ConvertString(string stringInput)
        //{
        //    string convert = "ĂÂÀẰẦÁẮẤẢẲẨÃẴẪẠẶẬỄẼỂẺÉÊÈỀẾẸỆÔÒỒƠỜÓỐỚỎỔỞÕỖỠỌỘỢƯÚÙỨỪỦỬŨỮỤỰÌÍỈĨỊỲÝỶỸỴĐăâàằầáắấảẳẩãẵẫạặậễẽểẻéêèềếẹệôòồơờóốớỏổởõỗỡọộợưúùứừủửũữụựìíỉĩịỳýỷỹỵđ ";
        //    string To = "AAAAAAAAAAAAAAAAAEEEEEEEEEEEOOOOOOOOOOOOOOOOOUUUUUUUUUUUIIIIIYYYYYDaaaaaaaaaaaaaaaaaeeeeeeeeeeeooooooooooooooooouuuuuuuuuuuiiiiiyyyyyd-";
        //    for (int i = 0; i < To.Length; i++)
        //    {
        //        stringInput = stringInput.Replace(convert[i], To[i]);
        //    }
        //    return stringInput.ToLower();
        //}﻿

        //private string GetRandomColour(Random rand)
        //{
        //    var c = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        //    return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        //}
    }
}

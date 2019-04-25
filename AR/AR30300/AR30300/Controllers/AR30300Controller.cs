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
using System.Globalization;
using HQ.eSkySys;
using System.Net;
using System.IO;

namespace AR30300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR30300Controller : Controller
    {
        private string _screenNbr = "AR30300";
        private string _userName = Current.UserName;
        AR30300Entities _db = Util.CreateObjectContext<AR30300Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        List<AR30300_ptTreeNode_Result> lstAllNode = new List<AR30300_ptTreeNode_Result>();
        private JsonResult _logMessage;
        private string _filePath;
        private string _Path;
        internal string PathImage
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "LocalAR30300");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                    _Path = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\AR30300");
                }
                if (!Directory.Exists(_filePath))
                {
                    Directory.CreateDirectory(_filePath);
                }
                return _filePath;
            }
        }

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

        public ActionResult AR30300_pgCustSearch(string state, string classId, string keysearch)
        {
            return this.Store(_db.AR30300_pgCustSearch(Current.CpnyID, Current.UserName, Current.LangID, state, classId, keysearch).ToList());
        }

        //-----------------------------Upload Image------------------------------------
        public ActionResult UploadFiles(string fileName, string slsperID, string branchID, string albumID,string custID)
        {
            try
            {
                Random rand = new Random();
                string iTmp = DateTime.Now.ToString("yyMMdd") + rand.Next(100000000, 999999999);
                string newFileName = "";
                string exten = branchID+ "/" + slsperID + "/" +DateTime.Now.ToString("yyyyMM") + "/" ;
                var files = Request.Files;
                string dir = PathImage + exten;
                if (!System.IO.Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                // Upload file moi
                newFileName = string.Format("{0}{1}", slsperID + "_" +"ALBUM" + "_" + DateTime.Now.ToString("yyyyMMdd")+ "_" + iTmp, Path.GetExtension(fileName));
                if (files.Count > 0)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        if (files[i].ContentLength > 0)
                        {
                            files[i].SaveAs(string.Format(@"{0}/{1}", dir, newFileName));
                        }
                    }
                }

                var objFile = _db.OM_AlbumImage.FirstOrDefault(p => p.BranchID == branchID && p.SlsperID == slsperID && p.AlbumID == albumID && p.ImageName == iTmp);

                while (objFile == null)
                {
                    string a = DateTime.Now.ToDateTime().ToString("yyyy-MM-dd HH':'mm':'ss");
                    objFile = new OM_AlbumImage();
                    objFile.ResetET();
                    objFile.BranchID = "ECO";
                    objFile.AlbumID = albumID;
                    objFile.SlsperID = slsperID;
                    objFile.ImageCreateDate = a.ToDateTime();//.ToString("yyyy-MM-dd");
                    objFile.ImageName = exten + newFileName;
                    objFile.Crtd_DateTime = DateTime.Now;
                    objFile.Crtd_Prog = _screenNbr;
                    objFile.Crtd_User = _userName;
                    objFile.LUpd_DateTime = DateTime.Now;
                    objFile.LUpd_Prog = _screenNbr;
                    objFile.LUpd_User = _userName;
                    objFile.CustID = custID;
                    _db.OM_AlbumImage.AddObject(objFile);

                }

                
                _db.SaveChanges();
                return Json(new { success = true, Exist = newFileName }, JsonRequestBehavior.AllowGet);

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

        //------------------DeleteImage---------------------------
        [HttpPost, ValidateInput(false)]
        public ActionResult AR30300DeleteImage( string albumID,string branchID,string imageName)
        {
            try
            {
                string newFileName = "";

                var objFile = _db.OM_AlbumImage.Where(p => p.AlbumID == albumID && p.ImageName == imageName).FirstOrDefault();
                    if (objFile != null)
                    {
                        _db.OM_AlbumImage.DeleteObject(objFile);
                        _db.SaveChanges();
                    }
                return Json(new { success = true, Exist = newFileName }, JsonRequestBehavior.AllowGet);

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

        public ActionResult GetAR30300_pdImage(string territory,string state,string branchID,string classID,string slsperID,string custID,string typeAlbum,DateTime startDate,DateTime endDate)
        {
            var lst = _db.AR30300_pdImages(Current.CpnyID, Current.UserName, Current.LangID,territory,state,branchID,classID,slsperID, custID, typeAlbum, startDate, endDate).ToList();
            foreach (var obj in lst)
            {
                obj.Pic = Util.ImageToBin(PathImage, obj.Pic,true);
            }
            return this.Store(lst);
        }
//////------------------TREE----------------------///////////
        [DirectMethod]
        public ActionResult AR30300GetTreeCustomer(string panelID,string territory,string state,string slsperID,string custID,string branchID)
        {
            TreePanel tree = new TreePanel();
            tree.ID = "treePanelCustomer";
            tree.ItemID = "treePanelCustomer";

            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Name", ModelFieldType.String));
            tree.Fields.Add(new ModelField("ParentID", ModelFieldType.String));
            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;
            Node node = new Node();
            node.NodeID = "Root";
            string _territory = territory == "null" ? "" : territory;
            string _state = state == "null" ? "" : state;
            string _slsperID = slsperID == "null" ? "" : slsperID;
            string _custID = custID == "null" ? "" : custID;
            string _branchID = branchID == "null" ? Current.CpnyID : branchID;
            // node.Checked = false;
            lstAllNode = _db.AR30300_ptTreeNode(Current.UserName, _branchID , Current.LangID, _territory, _state, _slsperID,_custID).ToList();


            var maxLevel = lstAllNode.Count == 0 ? -1 : lstAllNode.Max(x => x.Level);
            var lstFirst = lstAllNode.Where(x => x.Level == maxLevel).ToList();
            var crrLevel = maxLevel - 1;
            if (lstFirst.Count > 0)
            {
                string crrParent = string.Empty;
                Node parentNode = null;
                bool isAddChild = false;// lstFirst.Where(x => x.ParentID != string.Empty).Count() > 0;
                foreach (var it in lstFirst)
                {
                    var childNode = SetNodeValue(it, Ext.Net.Icon.UserHome);
                    GetChildNode(ref childNode, (int)crrLevel, it.SlsperId);

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
            // node.Checked = false;
            node.Icon = Ext.Net.Icon.FolderHome;
            var treeBranch = X.GetCmp<Panel>(panelID);
            if (lstAllNode.Count > 0)
            {
                tree.Root.Add(node);
                tree.Listeners.CheckChange.Fn = "treePanelCustomer_checkChange";
                tree.Listeners.SelectionChange.Fn = "treePanelCustomer_SlecheckChange";
                tree.Listeners.ItemCollapse.Fn = "tree_ItemCollapse";
            }
            tree.AddTo(treeBranch);

            return this.Direct();
        }

        private Node SetNodeValue(AR30300_ptTreeNode_Result objNode, Ext.Net.Icon icon)
        {
            Node node = new Node();

            Random rand = new Random();
            node.NodeID = objNode.SlsperId + objNode.ParentID + (rand.Next(999, 9999) + objNode.Level).ToString();
            //node.Checked = false;
            node.Text = objNode.Name;
            node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = objNode.Position, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = objNode.SlsperId, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "Name", Value = objNode.Name, Mode = Ext.Net.ParameterMode.Value });
            node.CustomAttributes.Add(new ConfigItem() { Name = "ParentID", Value = objNode.ParentID, Mode = Ext.Net.ParameterMode.Value });
            node.Icon = objNode.Level == 5 ? icon : Ext.Net.Icon.UserBrown;
            node.Leaf = objNode.Level == 0;
            //node.Leaf = objNode.Position == "S";// true;
            node.IconCls = "tree-node-noicon";
            return node;
        }

        private void GetChildNode(ref Node crrNode, int level, string parrentID)
        {
            crrNode.Checked = false;
            if (level >= 0)
            {
                var lstSub = lstAllNode.Where(x => x.ParentID == parrentID && x.Level == level).ToList();

                if (lstSub.Count > 0)
                {
                    var crrLevel = level - 1;
                    string crrParent = string.Empty;
                    foreach (var it in lstSub)
                    {
                        var childNode = SetNodeValue(it, Ext.Net.Icon.FolderGo);
                        GetChildNode(ref childNode, crrLevel, it.SlsperId);
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
        //public ActionResult AR30300DownloadAlbum(string files)
        //{
        //    string remoteFilePath = "http://mvc2.ext.net/Areas/DataView_Basic/Content/images/thumbs/gangster_zack.jpg";
        //    Uri remoteFilePathUri = new Uri(remoteFilePath);
        //    string remoteFilePathWithoutQuery = remoteFilePathUri.GetLeftPart(UriPartial.Path);
        //    string fileName = Path.GetFileName(remoteFilePathWithoutQuery);
        //    string localPath = Server.MapPath("~") + @"Images\AR30300\Album\" + fileName;
        //    WebClient webClient = new WebClient();
        //    webClient.DownloadFile(remoteFilePath, localPath);
        //    webClient.Dispose();

        //    return this.Direct();
        //}
    }
}

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
using System.Drawing;
using System.Text.RegularExpressions;
using HQ.eSkySys;
//using HQSendMailApprove;
using System.Web.Hosting;
using HQFramework.Common;
using Ionic.Zip;
using HQFramework.DAL;

namespace IN20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20500Controller : Controller
    {
        private string _screenNbr = "IN20500";
        private string _userName = Current.UserName;
        IN20500Entities _db = Util.CreateObjectContext<IN20500Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;

        private string _filePath;
        private string _Path;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadIN20500");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                    _Path = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\IN20500");
                }
                if (!Directory.Exists(_filePath))
                {
                    Directory.CreateDirectory(_filePath);
                }
                return _filePath;
            }
        }
        internal string PathVideo
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "PublicIN20500");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _Path = config.TextVal;
                }
                else
                {
                    _Path = Server.MapPath("~\\Images\\IN20500");
                }
                return _Path;
            }
        }     
        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            var isHideChkPublic = false;
            var DfltValMthd = false;
            var GiftPoint = false;
            var isShowBarCode = false;
            var isShowKitType = false;
            var isShowImport = false;
            var objConfig = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "IN20500CHKPUBLIC");
            var objConfigHideShow = _db.IN20500_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (objConfig != null && objConfig.IntVal == 1)
            {
                isHideChkPublic = true;
            }
            if (objConfigHideShow!= null)
            {
                DfltValMthd = objConfigHideShow.DfltValMthd ?? false;
                GiftPoint = objConfigHideShow.GiftPoint ??  false;
                isShowBarCode = objConfigHideShow.IsShowBarCode ?? false;
                isShowKitType = objConfigHideShow.KitType ?? false;
                isShowImport = objConfigHideShow.IsShowImportExport ?? false;
            }
            ViewBag.isHideChkPublic = isHideChkPublic;
            ViewBag.DfltValMthd = DfltValMthd;
            ViewBag.GiftPoint = GiftPoint;
            ViewBag.IsShowBarCode = isShowBarCode;
            ViewBag.IsShowKitType = isShowKitType;
            ViewBag.IsShowImport = isShowImport;
            return View();
        }
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetIN_Inventory(string InvtID)
        {
            return this.Store(_db.IN20500_pdHeader(InvtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault());
        }
        public ActionResult GetCpny(string InvtID)
        {
            return this.Store(_db.IN20500_pgIN_InvtCpny(InvtID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data, string NodeID, string NodeLevel, string ParentRecordID, string HiddenTree, string Copy)
        {
            try
            {
                string isNew = "false";
                string InvtID = data["cboInvtID"].PassNull();
                string InvtID_Copy = data["txtInvtIDCopy"].PassNull();
                string BranchID = data["cboCpnyID"].PassNull();
                string ClassId = data["cboClassId"].PassNull();
                string Status = data["cboApproveStatus"].PassNull();
                string Handle = data["cboHandle"].PassNull();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstIN_Inventory"]);
                var curHeader = dataHandler1.ObjectData<IN20500_pdHeader_Result>().FirstOrDefault();

                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstCpny"]);
                var lstCpny = dataHandler2.ObjectData<IN20500_pgIN_InvtCpny_Result>();

                #region Save IN_Inventory
                var header = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == InvtID);

                if (Copy == "true")
                {
                    if (header != null)
                    {
                        throw new MessageException(MessageType.Message, "8001",
                          parm: new[] { Util.GetLang("InvtID") + " " + InvtID });
                    }
                }

                if (header == null)
                { 
                    isNew = "true";
                    header = new IN_Inventory();
                    header.ResetET();
                    header.InvtID = InvtID;;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader, NodeID, NodeLevel, ParentRecordID, Status, Handle);
                    _db.IN_Inventory.AddObject(header);
                    var objUnit = _db.IN_UnitConversion.FirstOrDefault(x => x.InvtID == header.InvtID);
                    if (objUnit == null)
                    {
                        objUnit = new IN_UnitConversion();
                        CreateUnit(header,objUnit);
                        _db.IN_UnitConversion.AddObject(objUnit);
                    }
                    
                }
                else
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader, NodeID, NodeLevel, ParentRecordID, Status, Handle);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }

                InvtID = header.InvtID;
                #endregion

                #region Image & Media
                if (Copy == "false")
                {
                    var files = Request.Files;
                    if (files.Count > 0) // Co chon file de upload
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            if (files[i].ContentLength > 0)
                            {
                                if (Path.GetExtension(files[i].FileName).ToLower().Contains("jpg") || Path.GetExtension(files[i].FileName).ToLower().Contains("png") || Path.GetExtension(files[i].FileName).ToLower().Contains("gif"))
                                {
                                    // Xoa file cu di
                                    var oldPath = string.Format("{0}\\{1}", FilePath, header.Picture);
                                    if (System.IO.File.Exists(oldPath))
                                    {
                                        System.IO.File.Delete(oldPath);
                                    }

                                    // Upload file moi
                                    string newFileName = string.Format("{0}{1}", InvtID, Path.GetExtension(files[i].FileName));
                                    files[i].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                                    header.Picture = newFileName;

                                    ZipFiles(FilePath, newFileName);
                                }
                                else if (Path.GetExtension(files[i].FileName).ToLower().Contains("mp4") || Path.GetExtension(files[i].FileName).ToLower().Contains("wmv") ||
                                         Path.GetExtension(files[i].FileName).ToLower().Contains("ppt") || Path.GetExtension(files[i].FileName).ToLower().Contains("pptx") ||
                                         Path.GetExtension(files[i].FileName).ToLower().Contains("pdf") || Path.GetExtension(files[i].FileName).ToLower().Contains("xls") ||
                                         Path.GetExtension(files[i].FileName).ToLower().Contains("xlsx") || Path.GetExtension(files[i].FileName).ToLower().Contains("docx") ||
                                         Path.GetExtension(files[i].FileName).ToLower().Contains("doc"))
                                {
                                    // Xoa file cu di
                                    var oldPath = string.Format("{0}\\{1}", FilePath, header.Media);
                                    if (System.IO.File.Exists(oldPath))
                                    {
                                        System.IO.File.Delete(oldPath);
                                    }

                                    // Upload file moi
                                    string newFileName = string.Format("{0}{1}", InvtID, Path.GetExtension(files[i].FileName));
                                    files[i].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                                    header.Media = newFileName;
                                    
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(header.Picture) && string.IsNullOrWhiteSpace(curHeader.Picture))
                    {
                        // Xoa file cu di
                        var oldPath = string.Format("{0}\\{1}", FilePath, header.Picture);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                        DeleteFileInZip(header.Picture, FilePath);
                        header.Picture = string.Empty;
                    }
                    if (!string.IsNullOrWhiteSpace(header.Media) && string.IsNullOrWhiteSpace(curHeader.Media))
                    {
                        // Xoa file cu di
                        var oldPath = string.Format("{0}\\{1}", FilePath, header.Media);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                        header.Media = string.Empty;
                    }
                }
                else
                {
                    if (InvtID_Copy != "")
                    {
                        var objOld = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == InvtID_Copy);
                        if (objOld != null)
                        {
                            if (objOld.Picture != "")
                            {
                                FileInfo file = new FileInfo(FilePath + "\\" + objOld.Picture);
                                if (file.Exists)
                                {
                                    string urlImage = FilePath + "\\" + (InvtID) + file.Extension;
                                    file.CopyTo(urlImage);
                                    header.Picture = (InvtID) + file.Extension;
                                    ZipFiles(FilePath, header.Picture);
                                }
                            }
                            if (objOld.Media != "")
                            {
                                FileInfo file = new FileInfo(FilePath + "\\" + objOld.Media);
                                if (file.Exists)
                                {
                                    string urlMedia = FilePath + "\\" + (InvtID) + file.Extension;
                                    file.CopyTo(urlMedia);
                                    header.Media = (InvtID) + file.Extension;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Save IN_InvtCpny
                var lstCpny_DB = _db.IN_InvtCpny.Where(p => p.InvtID == InvtID).ToList();

                foreach (var del in lstCpny_DB)
                {
                    if (lstCpny.Where(p => p.CpnyID == del.CpnyID).Count() == 0)
                        _db.IN_InvtCpny.DeleteObject(del);
                }

                foreach (var obj in lstCpny)
                {
                    if (obj.CpnyID.PassNull() == "" || InvtID.PassNull() == "") continue;
                    var objCpny = _db.IN_InvtCpny.FirstOrDefault(p => p.CpnyID == obj.CpnyID && p.InvtID == InvtID);
                    if (objCpny == null)
                    {
                        objCpny = new IN_InvtCpny();
                        objCpny.ResetET();
                        objCpny.InvtID = InvtID;
                        objCpny.CpnyID = obj.CpnyID;
                        _db.IN_InvtCpny.AddObject(objCpny);
                    }
                }
                #endregion

                

                _db.SaveChanges();
                return Json(new { success = true, InvtID = InvtID, isNew = isNew });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(ref IN_Inventory t, IN20500_pdHeader_Result s, string NodeID, string NodeLevel, string ParentRecordID, string Status, string Handle)
        {
            t.NodeID = NodeID;
            t.NodeLevel = short.Parse(NodeLevel.PassNull() == "" ? "0" : NodeLevel);
            t.ParentRecordID = ParentRecordID.PassNull() == "" ? 0 : ParentRecordID.ToInt();

            if (t.Public == false && s.Public == true)
            {
                var del = _db.IN_InvtCpny.Where(p => p.InvtID == s.InvtID).ToList();
                foreach (var t1 in del)
                {
                    _db.IN_InvtCpny.DeleteObject(t1);
                }
            }
            t.Public = s.Public;
            t.BarCode = s.BarCode;
            t.Buyer = s.Buyer;
            t.ClassID = s.ClassID;
            t.Color = s.Color;
            t.Descr = s.Descr;
            t.Descr1 = s.Descr1;
            t.DfltPOUnit = s.DfltPOUnit;
            t.DfltSOUnit = s.DfltSOUnit;
            t.IROverStkQty = s.IROverStkQty;
            t.IRSftyStkDays = s.IRSftyStkDays;
            t.IRSftyStkPct = s.IRSftyStkPct;
            t.IRSftyStkQty = s.IRSftyStkQty;
            t.InvtType = s.InvtType;
            t.LossRate00 = s.LossRate00;
            t.LossRate01 = s.LossRate01;
            t.LossRate02 = s.LossRate02;
            t.LossRate03 = s.LossRate03;
            t.LotSerFxdLen = s.LotSerFxdLen;
            t.LotSerFxdTyp = s.LotSerFxdTyp;
            t.LotSerFxdVal = s.LotSerFxdVal;
            t.LotSerIssMthd = s.LotSerIssMthd;
            t.LotSerNumLen = s.LotSerNumLen;
            t.LotSerNumVal = s.LotSerNumVal;
            t.LotSerTrack = s.LotSerTrack;
            t.MaterialType = s.MaterialType;
            t.POFee = s.POFee;
            t.POPrice = s.POPrice;
            t.PrePayPct = s.PrePayPct;
            t.PriceClassID = s.PriceClassID;
            t.SOFee = s.SOFee;
            t.SOPrice = s.SOPrice;
            t.SerAssign = s.SerAssign;
            t.ShelfLife = s.ShelfLife;
            t.WarrantyDays = s.WarrantyDays;
            t.Size = s.Size;
            t.Source = s.Source;
            t.Status = s.Status;
            t.StkItem = s.StkItem;
            t.StkUnit = s.StkUnit;
            t.StkVol = s.StkVol;
            t.StkWt = s.StkWt;
            t.StkWtUnit = s.StkWtUnit;
            t.Style = s.Style;
            t.TaxCat = s.TaxCat;
            t.KitType = s.KitType;
            t.ValMthd = s.ValMthd;
            t.VendID1 = s.VendID1;
            t.VendID2 = s.VendID2;
            t.LotSerRcptAuto = s.LotSerRcptAuto;
            t.GiftPoint = s.GiftPoint;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;        
            if (Handle == string.Empty || Handle == "N")
                t.ApproveStatus = Status;
            else
                t.ApproveStatus = Handle;
            t.CnvFact = s.CnvFact > 0 ? s.CnvFact : 1;
            t.VideoModifiedDate = s.VideoModifiedDate;
            t.ImageModifiedDate = s.ImageModifiedDate;
        }


        private void CreateUnit(IN_Inventory t, IN_UnitConversion me) // bugID: 20180927_00049 VIETUC
        {
            me.UnitType = "3";
            me.ClassID = "*";
            me.InvtID = t.InvtID;
            me.FromUnit = t.StkUnit;
            me.ToUnit = t.StkUnit;
            me.MultDiv = "M";
            me.CnvFact = 1;
            me.LUpd_DateTime = DateTime.Now;
            me.LUpd_Prog = _screenNbr;
            me.LUpd_User = _userName;
            me.Crtd_DateTime = DateTime.Now;
            me.Crtd_Prog = _screenNbr;
            me.Crtd_User = Current.UserName;
        }

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, int z, List<SI_Hierarchy> lstSI_Hierarchy, List<IN_Inventory> lstIN_Inventory)
        {
            var node = new Node();
            var k = -1;
            if (inactiveHierachy.Descr == "root")
            {
                node.Text = inactiveHierachy.Descr;
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeID", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeLevel", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "ParentRecordID", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "RecordID", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "InvtID", Value = "", Mode = ParameterMode.Value });
            }
            else
            {
                node.Text = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
                node.NodeID = inactiveHierachy.NodeID + "-" + inactiveHierachy.NodeLevel + "-" + inactiveHierachy.ParentRecordID.ToString() + "-" + inactiveHierachy.RecordID;
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeID", Value = inactiveHierachy.NodeID, Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeLevel", Value = inactiveHierachy.NodeLevel.ToString(), Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "ParentRecordID", Value = inactiveHierachy.ParentRecordID.ToString(), Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "RecordID", Value = inactiveHierachy.RecordID.ToString(), Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "InvtID", Value = "", Mode = ParameterMode.Value });
            }

            var tmps = lstIN_Inventory
                .Where(p => p.NodeID == inactiveHierachy.NodeID
                    && p.ParentRecordID == inactiveHierachy.ParentRecordID
                    && p.NodeLevel == level - 1).ToList();

            var childrenInactiveHierachies = lstSI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.NodeLevel == level).ToList();

            if (tmps != null && tmps.Count > 0)
            {
                foreach (IN_Inventory tmp in tmps)
                {
                    k++;
                    Node nodetmp = new Node();
                    nodetmp.Text = tmp.InvtID + "-" + tmp.Descr;
                    nodetmp.NodeID = tmp.InvtID + "-" + "|";
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "NodeID", Value = inactiveHierachy.NodeID, Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "NodeLevel", Value = inactiveHierachy.NodeLevel.ToString(), Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "ParentRecordID", Value = inactiveHierachy.ParentRecordID.ToString(), Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "RecordID", Value = inactiveHierachy.RecordID.ToString(), Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "InvtID", Value = tmp.InvtID.ToString(), Mode = ParameterMode.Value });
                    nodetmp.Leaf = true;
                    node.Children.Add(nodetmp);
                }
            }

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (SI_Hierarchy childrenInactiveNode in childrenInactiveHierachies)
                {
                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, z++, lstSI_Hierarchy, lstIN_Inventory));
                }
            }
            else
            {
                node.Leaf = false;
                if (tmps.Count == 0 && childrenInactiveHierachies.Count == 0)
                {
                    node.Leaf = true;
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
        public ActionResult ReloadTreeIN20500()
        {
            var root = new Node() { };
            var nodeType = "I";
            var hierarchy = new SI_Hierarchy()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "root",
                Type = nodeType
            };
            var z = 0;

            List<SI_Hierarchy> lstSI_Hierarchy = _db.SI_Hierarchy.Where(p => p.Type == nodeType).ToList();
            List<IN_Inventory> lstIN_Inventory = _db.IN_Inventory.ToList();
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, z, lstSI_Hierarchy, lstIN_Inventory);
            //quan trong dung de refresh slmTree
            this.GetCmp<TreePanel>("treeInvt").SetRootNode(node);
            return this.Direct();
        }

        public ActionResult ImageToBin(string fileName)
        {
            try
            {
                if (fileName.ToLower().Contains("mp4") || fileName.ToLower().Contains("wmv")||
                    fileName.ToLower().Contains("mp3") || fileName.ToLower().Contains("pdf")||
                    fileName.ToLower().Contains("docx") || fileName.ToLower().Contains("doc")||
                    fileName.ToLower().Contains("pptx ") || fileName.ToLower().Contains("ppt")||
                    fileName.ToLower().Contains("txt")
                    ) fileName = "Media.png";
                string filename = FilePath + "\\" + fileName;
                if (System.IO.File.Exists(filename))
                {
                    FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    BinaryReader reader = new BinaryReader(fileStream);
                    byte[] imageBytes = reader.ReadBytes((int)fileStream.Length);
                    reader.Close();

                    var imgString64 = Convert.ToBase64String(imageBytes, 0, imageBytes.Length);

                    var jsonResult = Json(new { success = true, imgSrc = @"data:image/jpg;base64," + imgString64 }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                else
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
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

        [DirectMethod]
        public ActionResult PlayMedia(string fileVideo)
        {
            var pathMedia = string.Format(@"{0}{1}", PathVideo, fileVideo);
            string typeMedia = pathMedia.ToLower().Contains("mp4") ? "type='video/mp4'" : "type='video/wmv'";
            Ext.Net.Window win = new Window
            {
                ID = "Window1",
                Title = "Play media",
                Height = 520,
                Width = 640,
                Maximizable = true,
                Closable = true,
                Modal = true,
                Layout = "fit",
                Html = "<video width='640' height='480' controls autoplay> <source src='" + pathMedia + "' " + typeMedia + "> </video>"
            };
            win.Render(RenderMode.RenderTo);
            return this.Direct();
        }

        public void DeleteFileInZip(string fileName, string path)
        {
            try
            {
                string zipFolder = path;
                path += "\\";

                if (System.IO.File.Exists(zipFolder + "/ABC.zip"))
                {
                    using (ZipFile zip = ZipFile.Read(zipFolder + "/ABC.zip"))
                    {
                        string filePath = path + fileName;
                        Delete(filePath);
                        string zipFile = fileName;
                        if (zip.ContainsEntry(zipFile))
                        {
                            zip.RemoveEntry(zipFile);
                            zip.Save();
                        }
                    }
                }
                else
                {
                    string filePath = path + fileName;
                    Delete(filePath);
                }
            }
            catch
            {
            }
        }

        private void Delete(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        public void ZipFiles(string serverPath, string fileName)
        {
            try
            {
                string[] fileNames = fileName.Split(';');
                for (int i = 0; i < fileNames.Count(); i++)
                {
                    fileNames[i] = serverPath + "\\" + fileNames[i];
                }
                string zipFilePath = serverPath + "/ABC.zip";
                FileInfo file = new FileInfo(zipFilePath);
                if (file.Exists)
                {
                    using (ZipFile zip = ZipFile.Read(zipFilePath))
                    {
                        zip.AddFiles(fileNames, "");
                        zip.Save();
                    }
                }
                else
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.UseUnicodeAsNecessary = true;  // utf-8
                        zip.AddFiles(fileNames, "");
                        zip.Save(zipFilePath);
                    }
                }
            }
            catch
            { }
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string InvtID = data["cboInvtID"].PassNull();

                var objInvtID = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == InvtID);
                if (objInvtID != null)
                {
                    var del = _db.IN_InvtCpny.Where(p => p.InvtID == InvtID).ToList();
                    foreach (IN_InvtCpny t in del)
                    {
                        _db.IN_InvtCpny.DeleteObject(t);
                    }
                    var delUnit = _db.IN_UnitConversion.Where(x => x.InvtID == InvtID).ToList();

                    foreach (var item in delUnit)
                    {
                        _db.IN_UnitConversion.DeleteObject(item);
                    }

                    _db.IN_Inventory.DeleteObject(objInvtID);


                    // Xoa file cu di
                    var oldPath = string.Format("{0}\\{1}", FilePath, objInvtID.Picture);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }


                    // Xoa file cu di
                    oldPath = string.Format("{0}\\{1}", FilePath, objInvtID.Media);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = true });
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

        [HttpPost]
        public ActionResult Import()
        {
            var colTexts = HeaderExcel();
            var dataRowIdx = 1;
            FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
            HttpPostedFile file = fileUploadField.PostedFile;
            FileInfo fileInfo = new FileInfo(file.FileName);
            try
            {
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    Worksheet workSheet = workbook.Worksheets[0];

                    if (workSheet.Cells.MaxDataRow < 1)
                    {
                        throw new MessageException(MessageType.Message, "2018062955");
                    }
                    string message = string.Empty;
                    string invtIDNull = string.Empty;
                    string descrNull = string.Empty;
                    string classIDNull = string.Empty;
                    string typeNull = string.Empty;
                    string sourceNull = string.Empty;
                    string valMthdNull = string.Empty;
                    string lotSerTrackNull = string.Empty;
                    string stkUnitNull = string.Empty;
                    string pOUnitNull = string.Empty;
                    string sOUnitNull = string.Empty;
                    string taxCatNull = string.Empty;
                    string invtID = string.Empty;
                    string descr = string.Empty;
                    string classID = string.Empty;
                    string type = string.Empty;
                    string source = string.Empty;
                    string valMthd = string.Empty;
                    string lotSerTrack = string.Empty;
                    string stkUnit = string.Empty;
                    string pOUnit = string.Empty;
                    string sOUnit = string.Empty;
                    string taxCat = string.Empty;

                    for (int i = dataRowIdx; i <= workSheet.Cells.MaxDataRow; i++) //index luôn đi từ 0
                    {
                        #region Get data from excel
                        bool flagCheck = false;
                        invtID = workSheet.Cells[i, colTexts.IndexOf("InvtID")].StringValue.Trim();
                        descr = workSheet.Cells[i, colTexts.IndexOf("Descr")].StringValue.Trim();
                        classID = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("ClassID")].StringValue.Trim());
                        type = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("Type")].StringValue.Trim());
                        source = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("Source")].StringValue.Trim());
                        valMthd = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("ValMthd")].StringValue.Trim());
                        lotSerTrack = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("LotSerTrack")].StringValue.Trim());
                        stkUnit = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("StkUnit")].StringValue.Trim());
                        pOUnit = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("POUnit")].StringValue.Trim());
                        sOUnit = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("SOUnit")].StringValue.Trim());
                        taxCat = GetCodeFromExcel(workSheet.Cells[i, colTexts.IndexOf("TaxCat")].StringValue.Trim());

                        #endregion

                        #region checkdata

                        if ((!string.IsNullOrEmpty(descr) || !string.IsNullOrEmpty(classID) || !string.IsNullOrEmpty(type) || !string.IsNullOrEmpty(source) || !string.IsNullOrEmpty(valMthd) || !string.IsNullOrEmpty(lotSerTrack) || !string.IsNullOrEmpty(stkUnit)) && string.IsNullOrEmpty(invtID))
                        {
                            invtID += (i + 1) + ", ";
                            flagCheck = true;
                        }
                        if (string.IsNullOrEmpty(descr))
                        {
                            descrNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(classID))
                        {
                            classIDNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(type))
                        {
                            typeNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(source))
                        {
                            sourceNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(valMthd))
                        {
                            valMthdNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(lotSerTrack))
                        {
                            lotSerTrackNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(stkUnit))
                        {
                            stkUnitNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(pOUnit))
                        {
                            pOUnitNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(sOUnit))
                        {
                            sOUnitNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(taxCat))
                        {
                            taxCatNull += (i + 1) + ", ";
                            flagCheck = true;
                        }                        

                        #endregion

                        #region Save data 

                        if (!flagCheck)
                        {
                            var obj = _db.IN_Inventory.FirstOrDefault(x => x.InvtID == invtID);
                            if (obj == null)
                            {
                                obj = new IN_Inventory();
                                obj.InvtID = invtID;
                                obj.Descr = descr;
                                obj.ClassID = classID;
                                obj.InvtType = type;
                                obj.Source = source;
                                obj.ValMthd = valMthd;
                                obj.LotSerTrack = lotSerTrack;
                                obj.DfltPOUnit = pOUnit;
                                obj.DfltSOUnit = sOUnit;
                                obj.TaxCat = taxCat;
                                obj.StkUnit = stkUnit;
                                obj.NodeID = classID;
                                obj.NodeLevel = 2;
                                obj.ParentRecordID = 1;
                                obj.Public = true;
                                obj.BarCode = "";
                                obj.Buyer = "";
                                obj.Color = "";
                                obj.Descr1 = "";
                                obj.IROverStkQty = 0;
                                obj.IRSftyStkDays = 0;
                                obj.IRSftyStkPct = 0;
                                obj.IRSftyStkQty = 0;
                                obj.PriceClassID = "";
                                obj.LossRate00 = 0;
                                obj.LossRate01 = 0;
                                obj.LossRate02 = 0;
                                obj.LossRate03 = 0;
                                obj.LotSerFxdLen = 0;
                                obj.LotSerFxdTyp = "";
                                obj.LotSerFxdVal = "";
                                obj.LotSerIssMthd = "";
                                obj.LotSerNumLen = 0;
                                obj.LotSerNumVal = "";
                                obj.MaterialType = "MD";
                                obj.POFee = 0;
                                obj.POPrice = 0;
                                obj.PrePayPct = 0;
                                obj.SOFee = 0;
                                obj.SOPrice = 0;
                                obj.SerAssign = "";
                                obj.ShelfLife = 0;
                                obj.WarrantyDays = 0;
                                obj.Size = "";
                                obj.Status = "AC";
                                obj.StkItem = 1;
                                obj.StkVol = 0;
                                obj.StkWt = 0;
                                obj.StkWtUnit = "";
                                obj.Style = "";
                                obj.VendID1 = "";
                                obj.VendID2 = "";
                                obj.LotSerRcptAuto = false;
                                obj.GiftPoint = 0;
                                obj.ApproveStatus = "H";
                                obj.CnvFact = 1;
                                obj.Category = "";
                                obj.Brand = "";
                                obj.ProGroup = "";
                                obj.ProType = "";
                                obj.LUpd_DateTime = DateTime.Now;
                                obj.LUpd_Prog = _screenNbr;
                                obj.LUpd_User = _userName;
                                obj.Crtd_DateTime = DateTime.Now;
                                obj.Crtd_Prog = _screenNbr;
                                obj.Crtd_User = _userName;
                                _db.IN_Inventory.AddObject(obj);
                            }
                            else
                            {
                                obj.Descr = descr;
                                obj.ClassID = classID;
                                obj.PriceClassID = type;
                                obj.Source = source;
                                obj.ValMthd = valMthd;
                                obj.LotSerTrack = lotSerTrack;
                                obj.DfltPOUnit = pOUnit;
                                obj.DfltSOUnit = sOUnit;
                                obj.TaxCat = taxCat;
                                obj.LUpd_DateTime = DateTime.Now;
                                obj.LUpd_Prog = _screenNbr;
                                obj.LUpd_User = _userName;
                            }
                        }                      

                        #endregion
                    }

                    message = invtIDNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("InvtID"), invtIDNull.TrimEnd(','));
                    message += descrNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("Descr"), descrNull.TrimEnd(','));
                    message += classIDNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("ClassID"), descrNull.TrimEnd(','));
                    message += typeNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("Type"), descrNull.TrimEnd(','));
                    message += sourceNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("Source"), descrNull.TrimEnd(','));
                    message += valMthdNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("ValMthd"), descrNull.TrimEnd(','));
                    message += lotSerTrackNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("LotSerTrack"), descrNull.TrimEnd(','));
                    message += stkUnitNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("StkUnit"), descrNull.TrimEnd(','));
                    message += pOUnitNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("POUnit"), descrNull.TrimEnd(','));
                    message += sOUnitNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("SOUnit"), descrNull.TrimEnd(','));
                    message += taxCatNull == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("TaxCat"), descrNull.TrimEnd(','));

                    if (string.IsNullOrEmpty(message))
                    {
                        _db.SaveChanges();
                    }
                    Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "148");
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

            return _logMessage;
        }

        [HttpPost]
        public ActionResult Export(FormCollection data, string applicationID, string fiCode, string requestBy, string status, DateTime? fromDate, DateTime? toDate)
        {

            var colTexts = HeaderExcel();
           
            Stream stream = new MemoryStream();
            Workbook workbook = new Workbook();
            workbook.Worksheets.Add();
            Cell cell;
            Worksheet sheet = workbook.Worksheets[0];
            Worksheet masterData = workbook.Worksheets[1];
            DataAccess dal = Util.Dal();

            sheet.Name = Util.GetLang("IN20500NameSheet");
            masterData.Name = "MasterData";
            
            #region header info
            // Header text columns
            sheet.AutoFitColumns();

            for (int i = 0; i < colTexts.Count; i++)
            {
                SetCellValue(sheet.Cells[0, i], Util.GetLang(colTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, false, false);
                sheet.Cells.SetColumnWidth(i, 25);
            }
            var style = workbook.GetStyleInPool(0);
            #endregion
            var allColumns = new List<string>();
            allColumns.AddRange(colTexts);

            ParamCollection pc = new ParamCollection();
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtProductClass = dal.ExecDataTable("IN20500_pcProductClassExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtProductClass, true, 0, 0, false);

            string formulaProductClass = "=MasterData!$A$2:$A$" + (dtProductClass.Rows.Count + 2);
            Validation validation = GetValidation(ref sheet, formulaProductClass, "Chọn Nhóm", "Mã Nhóm này không tồn tại");
            validation.AddArea(GetCellArea(1, dtProductClass.Rows.Count + 100, colTexts.IndexOf("ClassID")));   
            
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtType = dal.ExecDataTable("IN20500_pcIvntTypeExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtType, true, 0, 3, false);

            string formulaType = "=MasterData!$D$2:$D$" + (dtType.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaType, "Chọn Loại ", "Mã Loại  này không tồn tại");
            validation.AddArea(GetCellArea(1, dtType.Rows.Count + 100, colTexts.IndexOf("Type")));

            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtSource = dal.ExecDataTable("IN20500_pcSourceExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtSource, true, 0, 6, false);

            string formulaSource = "=MasterData!$G$2:$G$" + (dtSource.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaSource, "Chọn Nguồn", "Mã Nguồn này không tồn tại");
            validation.AddArea(GetCellArea(1, dtSource.Rows.Count + 100, colTexts.IndexOf("Source")));   
            
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtValMthd = dal.ExecDataTable("IN20500_pcValMthdExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtValMthd, true, 0, 9, false);

            string formulaValMthd = "=MasterData!$J$2:$J$" + (dtValMthd.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaValMthd, "Chọn Phương Pháp Tính Giá", "Mã Phương Pháp Tính Giá này không tồn tại");
            validation.AddArea(GetCellArea(1, dtValMthd.Rows.Count + 100, colTexts.IndexOf("ValMthd")));           
            
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtLotSerTrack = dal.ExecDataTable("IN20500_pcLotSerTrackExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtLotSerTrack, true, 0, 11, false);

            string formulaLotSerTrack = "=MasterData!$L$2:$L$" + (dtLotSerTrack.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaLotSerTrack, "", "Mã này không tồn tại");
            validation.AddArea(GetCellArea(1, dtLotSerTrack.Rows.Count + 100, colTexts.IndexOf("LotSerTrack")));             
            
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtStkUnit = dal.ExecDataTable("IN20500_pcgetToUnitExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtStkUnit, true, 0, 13, false);

            string formulaStkUnit = "=MasterData!$N$2:$N$" + (dtStkUnit.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaStkUnit, "Chọn Đơn Vị Lưu Kho", "Mã Đơn Vị Lưu Kho này không tồn tại");
            validation.AddArea(GetCellArea(1, dtStkUnit.Rows.Count + 100, colTexts.IndexOf("StkUnit")));       
            
            
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtDfltPOUnit = dal.ExecDataTable("IN20500_pcDfltPOUnitExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtDfltPOUnit, true, 0, 15, false);

            string formulaDfltPOUnit = "=MasterData!$P$2:$P$" + (dtDfltPOUnit.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaDfltPOUnit, "Chọn Đơn Vị Mua", "Mã Đơn Vị Mua này không tồn tại");
            validation.AddArea(GetCellArea(1, dtDfltPOUnit.Rows.Count + 100, colTexts.IndexOf("POUnit"))); 
            
            
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtSOUnit = dal.ExecDataTable("IN20500_pcDfltPOUnitExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtSOUnit, true, 0, 15, false);

            string formulaSOUnit = "=MasterData!$P$2:$P$" + (dtSOUnit.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaSOUnit, "Chọn Đơn Vị bán", "Mã Đơn Vị Bán này không tồn tại");
            validation.AddArea(GetCellArea(1, dtSOUnit.Rows.Count + 100, colTexts.IndexOf("SOUnit")));
 
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtTax = dal.ExecDataTable("IN20500_pcTaxCatExcel", CommandType.StoredProcedure, ref pc);
            masterData.Cells.ImportDataTable(dtTax, true, 0, 17, false);

            string formulaTax = "=MasterData!$R$2:$R$" + (dtTax.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaTax, "Chọn Loại Thuế", "Mã Loại Thuế Bán này không tồn tại");
            validation.AddArea(GetCellArea(1, dtTax.Rows.Count + 100, colTexts.IndexOf("TaxCat")));

            sheet.Cells.SetRowHeight(0, 30);

            sheet.Protect(ProtectionType.All);
            masterData.Protect(ProtectionType.All);
            masterData.VisibilityType = VisibilityType.Hidden;

            var strFirstRow = 2.ToString();
            style = sheet.Cells["A" + strFirstRow].GetStyle();
            style.IsLocked = false;
            Range range;

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("InvtID")) + 2, GetCell(allColumns.IndexOf("InvtID")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("Descr")) + 2, GetCell(allColumns.IndexOf("Descr")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("ClassID")) + 2, GetCell(allColumns.IndexOf("ClassID")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("Type")) + 2, GetCell(allColumns.IndexOf("Type")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("Source")) + 2, GetCell(allColumns.IndexOf("Source")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("ValMthd")) + 2, GetCell(allColumns.IndexOf("ValMthd")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("LotSerTrack")) + 2, GetCell(allColumns.IndexOf("LotSerTrack")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("StkUnit")) + 2, GetCell(allColumns.IndexOf("StkUnit")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("POUnit")) + 2, GetCell(allColumns.IndexOf("POUnit")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("SOUnit")) + 2, GetCell(allColumns.IndexOf("SOUnit")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(GetCell(allColumns.IndexOf("TaxCat")) + 2, GetCell(allColumns.IndexOf("TaxCat")) + 1000);
            range.SetStyle(style);

            workbook.Save(stream, SaveFormat.Xlsx);
            stream.Flush();
            stream.Position = 0;

            return new FileStreamResult(stream, "application/vnd.ms-excel")
            {
                FileDownloadName = string.Format("{0}.xlsx", Util.GetLang("IN20500_excel"))
            };
        }

        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle, bool isBackground, bool isWrapsTex)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
            {
                style.Font.Color = Color.Red;
            }
            if (isBackground)
            {
                style.Font.Color = Color.Red;
                style.Pattern = BackgroundType.Solid;
                style.ForegroundColor = Color.Yellow;
            }
            if (isWrapsTex)
            {
                style.IsTextWrapped = true;
            }
            c.SetStyle(style);
        }


        private Validation GetValidation(ref Worksheet SheetMCP, string formular1, string inputMess, string errMess)
        {
            var validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
            validation.IgnoreBlank = true;
            validation.Type = Aspose.Cells.ValidationType.List;
            validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
            validation.Operator = OperatorType.Between;
            validation.Formula1 = formular1;
            validation.InputTitle = "";
            validation.InputMessage = inputMess;
            validation.ErrorMessage = errMess;
            return validation;
        }
        private CellArea GetCellArea(int startRow, int endRow, int columnIndex, int endColumnIndex = -1)
        {
            var area = new CellArea();
            area.StartRow = startRow;
            area.EndRow = endRow;
            area.StartColumn = columnIndex;
            area.EndColumn = endColumnIndex == -1 ? columnIndex : endColumnIndex;
            return area;
        }
        private List<string> HeaderExcel()
        {
            return new List<string>() { "InvtID", "Descr", "ClassID", "Type",  "Source", "ValMthd", "LotSerTrack", "StkUnit", "POUnit", "SOUnit", "TaxCat" };
        }
        private string GetCodeFromExcel(string codeDescr)
        {
            int index = codeDescr.IndexOf("-");
            if (index > 0)
            {
                return codeDescr.Substring(0, index).Trim();
            }
            return codeDescr.Trim();
        }
        private string GetCell(int column) // Hàm bị sai khi lấy vị trí column AA
        {
            if (column == 0)
            {
                return "A";
            }
            bool flag = false;
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 >= 1)
            {
                cell += ABC.Substring((column / 26) - 1, 1);
                column = column - 26;
                flag = true;
            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            else
            {
                if (column % 26 == 0 && flag)
                {
                    cell += ABC.Substring(0, 1);
                }
            }
            return cell;
        }        
    }
}

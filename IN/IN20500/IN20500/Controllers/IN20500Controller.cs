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
using System.Drawing;
using HQ.eSkySys;
//using HQSendMailApprove;
using System.Web.Hosting;
using Ionic.Zip;

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
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetIN_Inventory(string InvtID)
        {
            return this.Store(_db.IN20500_pdHeader(InvtID).FirstOrDefault());
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
                //StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstCpny"]);
                //ChangeRecords<IN20500_pgIN_InvtCpny_Result> lstCpny = dataHandler2.BatchObjectData<IN20500_pgIN_InvtCpny_Result>();

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

                //foreach (IN20500_pgIN_InvtCpny_Result deleted in lstCpny.Deleted)
                //{
                //    var objDelete = _db.IN_InvtCpny.Where(p => p.InvtID == InvtID
                //                                            && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                //    if (objDelete != null)
                //    {
                //        _db.IN_InvtCpny.DeleteObject(objDelete);
                //    }
                //}

                //lstCpny.Created.AddRange(lstCpny.Updated);

                //foreach (IN20500_pgIN_InvtCpny_Result curLang in lstCpny.Created)
                //{
                //    if (InvtID.PassNull() == "" || curLang.CpnyID.PassNull() == "") continue;

                //    var lang = _db.IN_InvtCpny.FirstOrDefault(p => p.InvtID.ToLower() == InvtID.ToLower()
                //                                                    && p.CpnyID.ToLower() == curLang.CpnyID.ToLower());

                //    if (lang == null)
                //    {
                //        lang = new IN_InvtCpny();
                //        lang.ResetET();
                //        lang.InvtID = InvtID;
                //        lang.CpnyID = curLang.CpnyID;
                //        _db.IN_InvtCpny.AddObject(lang);
                //    }
                //}
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
            t.ParentRecordID = short.Parse(ParentRecordID.PassNull() == "" ? "0" : ParentRecordID);

            if (t.Public == false && s.Public == true)
            {
                var del = _db.IN_InvtCpny.Where(p => p.InvtID == s.InvtID).ToList();
                for (int i = 0; i < del.Count; i++)
                {
                    _db.IN_InvtCpny.DeleteObject(del[i]);
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
            t.ValMthd = s.ValMthd;
            t.VendID1 = s.VendID1;
            t.VendID2 = s.VendID2;
            t.LotSerRcptAuto = s.LotSerRcptAuto;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        
            if (Handle == string.Empty || Handle == "N")
                t.ApproveStatus = Status;
            else
                t.ApproveStatus = Handle;
        
        }

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, int z, List<SI_Hierarchy> lstSI_Hierarchy, List<IN_Inventory> lstIN_Inventory)
        {
            var node = new Node();
            var k = -1;
            if (inactiveHierachy.Descr == "root")
                node.Text = inactiveHierachy.Descr;
            else
            {
                node.Text = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
                node.NodeID = inactiveHierachy.NodeID + "-" + inactiveHierachy.NodeLevel + "-" + inactiveHierachy.ParentRecordID.ToString() + "-" + inactiveHierachy.RecordID;
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
            //var pathMedia = PathVideo + "\\" + fileVideo;
            var pathMedia = string.Format(@"{0}{1}", PathVideo, fileVideo);
            //var pathMedia = ".mp4";
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
                    for (int i = 0; i < del.Count; i++)
                    {
                        _db.IN_InvtCpny.DeleteObject(del[i]);
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
        
    }
}

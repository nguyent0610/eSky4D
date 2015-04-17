using HQ.eSkyFramework;
using HQ.eSkySys;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using System.Web.Hosting;
using Ionic.Zip;
namespace IN20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20500Controller : Controller
    {
        string screenNbr = "IN20500";
        IN20500Entities _db = Util.CreateObjectContext<IN20500Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        string b = "";
        string tmpChangeTreeDic = "0";
        private string _pathImage;   
        
        IN_Inventory _objHeader = new IN_Inventory();
        private string _filePath;
        private string _Path;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadIN20500");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = HostingEnvironment.ApplicationPhysicalPath+"\\" + config.TextVal;
                    _Path = config.TextVal;
                }
                else
                {
                    _filePath =  HostingEnvironment.ApplicationPhysicalPath+"\\" + "Images\\IN20500";
                 
                }
                return _filePath;
            }
        }
        internal string PathVideo
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadIN20500");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {                 
                    _Path = config.TextVal;
                }
                else
                {
                  
                    _Path = "Images\\IN20500";
                }
                return _Path;
            }
        }
     
        public ActionResult Index()
        {
            //var user =_sys.Users.Where(p=> p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            //ViewBag.Roles = user.UserTypes;
            //var root = new Node() { };
            //var nodeType = "I";

            //var hierarchy = new SI_Hierarchy()
            //{
            //    RecordID = 0,
            //    NodeID = "",
            //    ParentRecordID = 0,
            //    NodeLevel = 1,
            //    Descr = "root",
            //    Type = nodeType
            //};
            //var z = 0 ;
            //ViewData["resultRoot"] = createNode(root, hierarchy, hierarchy.NodeLevel, z);
            return View();
        }

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, int z)
        {
            var node = new Node();
            var k = -1;
            //GetNodeItem(root, inactiveHierachy, level);
            if (inactiveHierachy.Descr == "root")
            {
                node.Text = inactiveHierachy.Descr;
            }
            else
            {
                node.Text = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
                node.NodeID = inactiveHierachy.NodeID + "-" + inactiveHierachy.NodeLevel + "-" + inactiveHierachy.ParentRecordID.ToString();            
            }

            var tmps = _db.IN_Inventory
                .Where(p => p.NodeID == inactiveHierachy.NodeID
                    && p.ParentRecordID == inactiveHierachy.ParentRecordID
                    && p.NodeLevel == level - 1).ToList();
            var childrenInactiveHierachies = _db.SI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.Type == inactiveHierachy.Type
                    && p.NodeLevel == level).ToList();

            if (tmps != null && tmps.Count > 0)
            {
                foreach (IN_Inventory tmp in tmps)
                {

                    k++;

                    Node nodetmp = new Node();
                    nodetmp.Text = tmp.InvtID + "-" + tmp.Descr;
                    nodetmp.NodeID ="InvtID-"+ tmp.InvtID;
                    nodetmp.Leaf = true;                
                    node.Children.Add(nodetmp);
                    //System.Diagnostics.Debug.WriteLine(nodetmp.Text);

                }
            }

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (SI_Hierarchy childrenInactiveNode in childrenInactiveHierachies)
                {

                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, z++));

                }
            }
            else
            {
                if (tmps.Count == 0 && childrenInactiveHierachies.Count == 0)
                {

                    node.Leaf = true;
                    //node.NodeID = Convert.ToString(level) + Convert.ToString(z);
                }
                else
                {

                    node.Leaf = false;
                }
            }
            System.Diagnostics.Debug.WriteLine(node.Text);

            return node;
        }


        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            ViewBag.Roles = user.UserTypes;
            //var root = new Node() { };
            //var nodeType = "I";

            //var hierarchy = new SI_Hierarchy()
            //{
            //    RecordID = 0,
            //    NodeID = "",
            //    ParentRecordID = 0,
            //    NodeLevel = 1,
            //    Descr = "root",
            //    Type = nodeType
            //};
            //var z = 0;
            //ViewData["resultRoot"] = createNode(root, hierarchy, hierarchy.NodeLevel, z);
            return PartialView();
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
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, z);
            this.GetCmp<TreePanel>("IDTree").SetRootNode(node);

            return this.Direct();

        }


        public ActionResult GetProductClass(String classID)
        {
            var rptCtrl = from p in _db.IN_ProductClass
                          where p.ClassID == classID
                          select new
                          {
                              p.ClassID,
                              p.Public,
                              StkItem = p.DfltStkItem,
                              InvtType = p.DfltInvtType,
                              Source = p.DfltSource,
                              ValMthd = p.DfltValMthd,
                              LotSerTrack = p.DfltLotSerTrack,
                              p.Buyer,
                              StkUnit = p.DfltStkUnit,
                              p.DfltPOUnit,
                              p.DfltSOUnit,
                              p.MaterialType,
                              TaxCat = p.DfltSlsTaxCat,
                              SerAssign = p.DfltLotSerAssign,
                              LotSerIssMthd = p.DfltLotSerMthd,
                              ShelfLife = p.DfltLotSerShelfLife,
                              WarrantyDays = p.DfltWarrantyDays,
                              LotSerFxdTyp = p.DfltLotSerFxdTyp,
                              LotSerFxdLen = p.DfltLotSerFxdLen,
                              LotSerFxdVal = p.DfltLotSerFxdVal,
                              LotSerNumLen = p.DfltLotSerNumLen,
                              LotSerNumVal = p.DfltLotSerNumVal

                          };
            return this.Store(rptCtrl);
        }

        public ActionResult GetInventoryClass(String invtID)
        {
            var rptInven = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            return this.Store(rptInven);
        }

        public ActionResult GetCompanyClass(String classID, String invtID, String chooseGrid)
        {
            // var lst = new List<>();
            //var lst = _db.IN20500_pgGetCompanyInvt(invtID).ToList();
            //var lst = from p in _db.IN20500_pgGetCompanyInvt(invtID).ToList()

            //          select new
            //          {
            //              ClassID = p.InvtID,
            //              p.CpnyID,
            //              p.CpnyName,


            //          };
            //var lst1 = _db.IN20500_pgGetCompany(classID).ToList();
            if (chooseGrid == "1")
            {
                var lst = from p in _db.IN20500_pgGetCompanyInvt(invtID).ToList()

                          select new
                          {
                              ClassID = p.InvtID,
                              p.CpnyID,
                              p.CpnyName,


                          };
                //var lst = _db.IN20500_pgGetCompanyInvt(invtID).ToList();

                return this.Store(lst);
            }
            else
            {
                var lst = _db.IN20500_pgGetCompany(classID).ToList();
                return this.Store(lst);
            }
            //return this.Store(lst);
        }

        [ValidateInput(false)]
        public ActionResult Save(FormCollection data, string invtID,bool isNew, string handle, string nodeID, string nodeLevel, string parentRecordID, int hadChild, string approveStatus, bool Public, bool StkItem, string imageChange, int tmpImageDelete, string tmpImageForUpload, int tmpMediaDelete, string tmpSelectedNode, string tmpCopyFormSave, string tmpCopyForm, string tmpCopyFormImageUrl, string tmpCopyFormMedia, string tmpOldFileName, string mediaExist)
        {
            try
            {
                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
                IN_Inventory obj = dataHandler2.ObjectData<IN_Inventory>().FirstOrDefault();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
                ChangeRecords<IN20500_pgGetCompanyInvt_Result> lstgrd = dataHandler1.BatchObjectData<IN20500_pgGetCompanyInvt_Result>();

                string invtIDCopyForm = data["cboInvtID"];
              
                obj.InvtID = invtID;
                obj.NodeID = nodeID;
                obj.NodeLevel = short.Parse(nodeLevel);
                obj.ParentRecordID = int.Parse(parentRecordID);
           
                _objHeader = _db.IN_Inventory.Where(p => p.InvtID == obj.InvtID).FirstOrDefault();
                if (_objHeader != null && !isNew)
                {
                    Updating_Inventory(_objHeader, obj,data);
                    if (obj.Public == true)
                    {
                        var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
                        for (int i = 0; i < del.Count; i++)
                        {
                            _db.IN_InvtCpny.DeleteObject(del[i]);
                        }
                    }

                }
                else if (_objHeader != null && isNew)
                {
                    throw new MessageException(MessageType.Message, "8001",
                          parm: new[] { Util.GetLang("InvtID") + " " + obj.InvtID });

                }
                else
                {
                    _objHeader = new IN_Inventory();
                    _objHeader.ResetET();
                    _objHeader.InvtID = obj.InvtID;
                    if (obj.Public == true)
                    {
                        var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
                        for (int i = 0; i < del.Count; i++)
                        {

                            _db.IN_InvtCpny.DeleteObject(del[i]);

                        }
                    }
                    Updating_Inventory(_objHeader, obj,data);

                    _objHeader.Crtd_DateTime = DateTime.Now;
                    _objHeader.Crtd_Prog = screenNbr;
                    _objHeader.Crtd_User = Current.UserName;
                    _db.IN_Inventory.AddObject(_objHeader);
                }

                var files = Request.Files;
                if (files.Count > 0 ) // Co chon file de upload
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        if (files[i].ContentLength > 0)
                        {
                            if (Path.GetExtension(files[i].FileName).ToLower().Contains("jpg") || Path.GetExtension(files[i].FileName).ToLower().Contains("png") || Path.GetExtension(files[i].FileName).ToLower().Contains("gif"))
                            {
                                // Xoa file cu di
                                var oldPath = string.Format("{0}\\{1}", FilePath, _objHeader.Picture);
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }

                                // Upload file moi
                                string newFileName = string.Format("{0}{1}", invtID, Path.GetExtension(files[i].FileName));
                                files[i].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                                _objHeader.Picture = newFileName;

                                ZipFiles(FilePath, newFileName);
                            }
                            else if (Path.GetExtension(files[i].FileName).ToLower().Contains("mp4") || Path.GetExtension(files[i].FileName).ToLower().Contains("wmv"))
                            {
                                // Xoa file cu di
                                var oldPath = string.Format("{0}\\{1}", FilePath, _objHeader.Media);
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }

                                // Upload file moi
                                string newFileName = string.Format("{0}{1}", invtID, Path.GetExtension(files[i].FileName));
                                files[i].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                                _objHeader.Media = newFileName;
                            }
                        }
                    }
                }
               
                if (!string.IsNullOrWhiteSpace(_objHeader.Picture) && string.IsNullOrWhiteSpace(obj.Picture))
                {
                    // Xoa file cu di
                    var oldPath = string.Format("{0}\\{1}", FilePath, _objHeader.Picture);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                    DeleteFileInZip(_objHeader.Picture, FilePath);
                    _objHeader.Picture = string.Empty;
                    
                }
                if (!string.IsNullOrWhiteSpace(_objHeader.Media) && string.IsNullOrWhiteSpace(obj.Media))
                {
                    // Xoa file cu di
                    var oldPath = string.Format("{0}\\{1}", FilePath, _objHeader.Media);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                    _objHeader.Media = string.Empty;
                }
                // cap nhat Compay
                if (obj.Public != true)
                {
                    foreach (IN20500_pgGetCompanyInvt_Result deleted in lstgrd.Deleted)
                    {
                        if (approveStatus != "H" && approveStatus != "D" && approveStatus != "C")
                        {
                            var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                            if (del != null)
                            {
                                _db.IN_InvtCpny.DeleteObject(del);

                            }
                        }
                    }
                    foreach (IN20500_pgGetCompanyInvt_Result created in lstgrd.Created)
                    {
                        var record = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == created.CpnyID).FirstOrDefault();
                        if (record == null)
                        {
                            record = new IN_InvtCpny();
                            record.InvtID = invtID;
                            record.CpnyID = created.CpnyID;


                            if (record.InvtID != "" && record.CpnyID != "")
                            {

                                _db.IN_InvtCpny.AddObject(record);
                            }
                        }
                    }



                    foreach (IN20500_pgGetCompanyInvt_Result updated in lstgrd.Updated)
                    {
                        var record = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == updated.CpnyID).FirstOrDefault();
                        if (record != null)
                        {
                        }
                        else
                        {
                            record = new IN_InvtCpny();
                            record.InvtID = invtID;
                            record.CpnyID = updated.CpnyID;
                            _db.IN_InvtCpny.AddObject(record);
                        }
                    }
                }
                #region save task
                if (handle != "" && handle != "N")
                {
                    string branch = "";
                    if (obj.Public == true)
                    {
                        var lstCpny = _sys.SYS_Company.ToList();
                        foreach (var item in lstCpny)
                            branch += item.CpnyID + ',';
                    }
                    else
                    {
                        foreach (var cpny in _db.IN_InvtCpny.Where(p => p.InvtID == invtID))
                            branch += cpny.CpnyID + ',';
                    }
                    if (branch.Length > 0) branch = branch.Substring(0, branch.Length - 1);

                    var objHO_Pending = _db.HO_PendingTasks.Where(p => p.ObjectID == obj.InvtID && p.EditScreenNbr == screenNbr && p.BranchID == branch).FirstOrDefault();

                    var objHandle = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == screenNbr && p.Status == obj.ApproveStatus && p.Handle == handle).FirstOrDefault();

                    if (objHO_Pending == null && objHandle != null)
                    {
                        if (!objHandle.Param00.PassNull().Split(',').Any(p => p.ToLower() == "notapprove"))
                        {
                            HO_PendingTasks newTask = new HO_PendingTasks();
                            newTask.BranchID = branch;
                            newTask.ObjectID = _objHeader.InvtID;
                            newTask.EditScreenNbr = screenNbr;
                            newTask.Content = string.Format(objHandle.ContentApprove, _objHeader.InvtID, _objHeader.Descr, branch);
                            newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                            newTask.Crtd_Prog = newTask.LUpd_Prog = screenNbr;
                            newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                            newTask.Status = objHandle.ToStatus;
                            newTask.tstamp = new byte[1];
                            _db.HO_PendingTasks.AddObject(newTask);
                        }
                      
                    }
                    if (objHandle != null) _objHeader.ApproveStatus = objHandle.ToStatus;
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, invtID = _objHeader.InvtID, Descr = _objHeader.Descr }, JsonRequestBehavior.AllowGet);
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


        private void Updating_Inventory(IN_Inventory t, IN_Inventory s,FormCollection data)
        {
           
                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = screenNbr;
                t.LUpd_User = Current.UserName;

                t.Public = s.Public;
                t.BarCode = s.BarCode;
                t.Buyer = data["cboBuyer"];
                t.ClassID = data["cboClassID"];
                t.Color = data["txtColor"];
                t.Descr = data["txtDescr"];
                t.Descr1 = data["txtDescr1"];
                t.DfltPOUnit = data["cboDfltPOUnit"];
                t.DfltSOUnit = data["cboDfltSOUnit"];
                t.IROverStkQty = Convert.ToDouble(data["txtIROverStkQty"]);
                t.IRSftyStkDays = Convert.ToDouble(data["txtIRSftyStkDays"]);
                t.IRSftyStkPct = Convert.ToDouble(data["txtIRSftyStkPct"]);
                t.IRSftyStkQty = Convert.ToDouble(data["txtIRSftyStkQty"]);
                t.InvtType = data["cboInvtType"];
                t.LossRate00 = Convert.ToDouble(data["txtLossRate00"]);
                t.LossRate01 = Convert.ToDouble(data["txtLossRate01"]);
                t.LossRate02 = Convert.ToDouble(data["txtLossRate02"]);
                t.LossRate03 = Convert.ToDouble(data["txtLossRate03"]);
                t.LotSerFxdLen = Convert.ToInt16(data["txtLotSerFxdLen"]);
                t.LotSerFxdTyp = data["cboLotSerFxdTyp"];
                t.LotSerFxdVal = data["txtLotSerFxdVal"];
                t.LotSerIssMthd = data["cboLotSerIssMthd"];
                t.LotSerNumLen = Convert.ToInt16(data["txtLotSerNumLen"]);
                t.LotSerNumVal = data["txtLotSerNumVal"];
                t.LotSerTrack = data["cboLotSerTrack"];
                t.MaterialType = data["cboMaterialType"];
                t.NodeID = s.NodeID;
                t.NodeLevel = s.NodeLevel;
                t.POFee = Convert.ToDouble(data["txtPOFee"]);
                t.POPrice = Convert.ToDouble(data["txtPOPrice"]);
                t.ParentRecordID = s.ParentRecordID;
                t.PrePayPct = Convert.ToDouble(data["txtPrePayPct"]);
                t.PriceClassID = data["cboPriceClassID"];
                t.SOFee = Convert.ToDouble(data["txtSOFee"]);
                t.SOPrice = Convert.ToDouble(data["txtSOPrice"]);
                t.SerAssign = data["cboSerAssign"];
                t.ShelfLife = Convert.ToInt16(data["txtShelfLife"]);
                t.WarrantyDays = Convert.ToInt16(data["txtWarrantyDays"]);
                t.Size = s.Size;
                t.Source = data["cboSource"];
                t.Status = s.Status;
                t.StkItem = s.StkItem;
                t.StkUnit = data["cboStkUnit"];
                t.StkVol = Convert.ToDouble(data["txtStkVol"]);
                t.StkWt = Convert.ToDouble(data["txtStkWt"]);
                t.StkWtUnit = data["cboStkWtUnit"];
                t.Style = data["cboStyle"];
                t.TaxCat = data["cboDfltSlsTaxCat"];
                t.ValMthd = data["cboValMthd"];
                t.VendID2 = data["cboVendor2"];
                t.VendID1 = data["cboVendor1"];
                t.ApproveStatus = s.ApproveStatus;
                t.LotSerRcptAuto = s.LotSerRcptAuto;
        }

        

        [HttpPost]
        public ActionResult Delete(FormCollection data, string invtID)
        {
            try
            {
                var objInvtID = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvtID != null)
                {
                    var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
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
        [DirectMethod]
        public ActionResult PlayMedia(string fileVideo)
        {
            var pathMedia = PathVideo + "\\" + fileVideo;
            //var pathMedia = PathImage.Substring(0, PathImage.Length - 14) + "Media\\" + fileVideo;
            //var pathMedia = "/DevProjects/FrameworkWeb/App/Media/a.mp4";
            //var pathMedia = "file://192.168.130.4/DevProjects/FrameworkWeb/App/Media/a.mp4";
            //var pathMedia = "http://techslides.com/demos/sample-videos/small.mp4";

            Window win = new Window
            {
                ID = "Window1",
                Title = "Media",
                Height = 520,
                Width = 640,
                Closable = true,
                CloseAction = CloseAction.Destroy,
                Modal = true,

                Html = "<video width='640' height='480' controls autoplay><source src='" + pathMedia + (pathMedia.ToLower().Contains("mp4") ? "' type='video/mp4'></video>" : "' type='video/wmv'></video>")

            };

            win.Render(RenderMode.RenderTo);
            //return Json(new { success = true, value = win }, JsonRequestBehavior.AllowGet);
            return this.Direct();

        }
       
       
        public ActionResult ImageToBin(string fileName)
        {
            try
            {
                if (fileName.ToLower().Contains("mp4") || fileName.ToLower().Contains("wmv")) fileName = "anh1.jpg";
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
    }
}

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
        internal string PathImage
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadIN20500");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _pathImage = config.TextVal;
                }
                else
                {
                    _pathImage = string.Empty;
                }
                return _pathImage;
            }
        }
        IN_Inventory _objHeader = new IN_Inventory();
        private bool _isConfig;
        internal bool IsConfig
        {
            get
            {
                _isConfig = string.IsNullOrWhiteSpace(PathImage) ? false : true;
                return _isConfig;
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

        [HttpPost]
        public ActionResult Save(FormCollection data, string invtID,bool isNew, string handle, string nodeID, string nodeLevel, string parentRecordID, int hadChild, string approveStatus, bool Public, bool StkItem, string imageChange, int tmpImageDelete, string tmpImageForUpload, int tmpMediaDelete, string tmpSelectedNode, string tmpCopyFormSave, string tmpCopyForm, string tmpCopyFormImageUrl, string tmpCopyFormMedia, string tmpOldFileName, string mediaExist)
        {
            try
            {
                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
                IN_Inventory obj = dataHandler2.ObjectData<IN_Inventory>().FirstOrDefault();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
                ChangeRecords<IN20500_pgGetCompanyInvt_Result> lstgrd = dataHandler1.BatchObjectData<IN20500_pgGetCompanyInvt_Result>();

                string invtIDCopyForm = data["cboInvtID"];
                string images = getPathThenUploadImage(obj, invtID);
                string media = getPathMedia(obj, invtID, mediaExist);
                obj.InvtID = invtID;
                obj.NodeID = nodeID;
                obj.NodeLevel = short.Parse(nodeLevel);
                obj.ParentRecordID = int.Parse(parentRecordID);
                obj.Picture = images;
                obj.Media = media;

                _objHeader = _db.IN_Inventory.Where(p => p.InvtID == obj.InvtID).FirstOrDefault();
                if (_objHeader != null && !isNew)
                {
                    Updating_Inventory(_objHeader, obj);
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
                    Updating_Inventory(_objHeader, obj);

                    _objHeader.Crtd_DateTime = DateTime.Now;
                    _objHeader.Crtd_Prog = screenNbr;
                    _objHeader.Crtd_User = Current.UserName;
                    _db.IN_Inventory.AddObject(_objHeader);
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


        private void Updating_Inventory(IN_Inventory t, IN_Inventory s)
        {
           
                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = screenNbr;
                t.LUpd_User = Current.UserName;

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
                t.NodeID = s.NodeID;
                t.NodeLevel = s.NodeLevel;
                t.POFee = s.POFee;
                t.POPrice = s.POPrice;
                t.ParentRecordID = s.ParentRecordID;
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
                t.VendID2 = s.VendID2;
                t.VendID1 = s.VendID1;
                t.ApproveStatus = s.ApproveStatus;
        }

       
        [DirectMethod]
        public ActionResult IN20500Delete(string invtID)
        {
            var inv = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            _db.IN_Inventory.DeleteObject(inv);
            _db.SaveChanges();
            return this.Direct();
        }
       
        [DirectMethod]
        public ActionResult GetImages(string Name)
        {
            string typeFile = "";
            if (Name.EndsWith(".jpg"))
            {
                typeFile = "jpg";
            }
            else if (Name.EndsWith(".png"))
            {
                typeFile = "png";
            }
            else if (Name.EndsWith(".gif"))
            {
                typeFile = "gif";
            }
            var Images = this.GetCmp<Image>("imgPPCStorePicReq");
            string a = getStringImage(Name);

            Images.ImageUrl = @"data:image/" + typeFile + ";base64," + a;

            return this.Direct();
        }
        private string getStringImage(string name)
        {
            var a = IN20500ImgHelper.IN20500GetImage(name, PathImage, string.IsNullOrWhiteSpace(PathImage) ? false : true);
            if (a == null)
            {
                return string.Empty;
            }
            else
            {
                return Convert.ToBase64String(a);
            }
        }
        private byte[] getByteImage(string dataIamge)
        {
            return Encoding.ASCII.GetBytes(dataIamge);
        }
        [DirectMethod]
        public ActionResult Upload()
        {
            if (this.GetCmp<FileUploadField>("NamePPCStorePicReq").HasFile)
            {
                var FileUpload1 = this.GetCmp<FileUploadField>("NamePPCStorePicReq");//.PostedFile.InputStream;
                string typeFile = "";
                if (FileUpload1.PostedFile.FileName.EndsWith(".jpg"))
                {
                    typeFile = "jpg";
                }
                else if (FileUpload1.PostedFile.FileName.EndsWith(".png"))
                {
                    typeFile = "png";
                }
                else if (FileUpload1.PostedFile.FileName.EndsWith(".gif"))
                {
                    typeFile = "gif";
                }
                if (typeFile != "")
                {
                    var Images = this.GetCmp<Image>("imgPPCStorePicReq");
                    var txtImages = this.GetCmp<TextField>("PPCStorePicReq");

                    int intLength = Convert.ToInt32(FileUpload1.PostedFile.InputStream.Length);
                    byte[] arrContent = new byte[intLength];
                    string imgType = FileUpload1.PostedFile.ContentType;

                    FileUpload1.PostedFile.InputStream.Read(arrContent, 0, intLength);
                    Images.ImageUrl = @"data:image/" + typeFile + ";base64," + Convert.ToBase64String(arrContent); ;
                    txtImages.Text = FileUpload1.PostedFile.FileName;
                }
                else
                {
                    X.Msg.Show(new MessageBoxConfig
                    {
                        Buttons = MessageBox.Button.OK,
                        Icon = MessageBox.Icon.ERROR,
                        Title = "Fail",
                        Message = "File format .jpg,.png,.gif"
                    });
                }
            }
            else
            {
                X.Msg.Show(new MessageBoxConfig
                {
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.ERROR,
                    Title = "Fail",
                    Message = "No file uploaded"
                });
            }
            DirectResult result = new DirectResult();
            result.IsUpload = true;

            return result;
        }
        private string getPathThenUploadImage(IN_Inventory inventory, string invtID)
        {
            string images = string.Format("{0}.jpg", invtID);

            if (!string.IsNullOrWhiteSpace(inventory.Picture) && !inventory.Picture.Contains(".jpg"))
            {
                string strImage = inventory.Picture
                    .Replace("data:image/jpg;base64,", "")
                    .Replace("data:image/png;base64,", "")
                    .Replace("data:image/gif;base64,", "");

                // Upload a new file.
                IN20500ImgHelper.IN20500UploadImage(images,
                    Convert.FromBase64CharArray(strImage.ToCharArray(), 0, strImage.Length),
                    PathImage, IsConfig);
            }
            else if (!string.IsNullOrWhiteSpace(inventory.Picture) && inventory.Picture.Contains(".jpg"))
            {
                images = inventory.Picture;
            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteFile(images, PathImage, IsConfig);
            }

            return images;
        }
        private string getPathThenUploadImageCopyForm(string tmpCopyFormImageUrl, string invtID)
        {
            string images = string.Format("{0}.jpg", invtID);

            if (!string.IsNullOrWhiteSpace(tmpCopyFormImageUrl) && !tmpCopyFormImageUrl.Contains(".jpg"))
            {
                string strImage = tmpCopyFormImageUrl
                    .Replace("data:image/jpg;base64,", "")
                    .Replace("data:image/png;base64,", "")
                    .Replace("data:image/gif;base64,", "");

                // Upload a new file.
                IN20500ImgHelper.IN20500UploadImage(images,
                    Convert.FromBase64CharArray(strImage.ToCharArray(), 0, strImage.Length),
                    PathImage, IsConfig);
            }
            else if (!string.IsNullOrWhiteSpace(tmpCopyFormImageUrl) && tmpCopyFormImageUrl.Contains(".jpg"))
            {
                images = tmpCopyFormImageUrl;
            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteFile(images, PathImage, IsConfig);
            }

            return images;
        }
        [DirectMethod]
        public ActionResult PlayMedia(string fileVideo)
        {
            var pathMedia = "/eSky4D/Media/" + fileVideo;
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

                Html = "<video width='640' height='480' controls autoplay><source src='" + pathMedia + "' type='video/mp4'></video>"

            };

            win.Render(RenderMode.RenderTo);
            //return Json(new { success = true, value = win }, JsonRequestBehavior.AllowGet);
            return this.Direct();

        }
        [ValidateInput(false)]
        public ActionResult IN20500UploadMedia(string invtID)
        {
            string fullFileName = "";
            if (this.GetCmp<FileUploadField>("NamePPCStoreMediaReq").HasFile)
            {
                var FileUpload1 = this.GetCmp<FileUploadField>("NamePPCStoreMediaReq");//.PostedFile.InputStream;
                string typeFile = "";
                if (FileUpload1.PostedFile.FileName.EndsWith(".mp4"))
                {
                    typeFile = "mp4";
                }
                else if (FileUpload1.PostedFile.FileName.EndsWith(".wmv"))
                {
                    typeFile = "wmv";
                }

                if (typeFile != "")
                {

                    fullFileName = invtID + "." + typeFile;

                    //doi icon anh
                    var Images = this.GetCmp<Image>("imgPPCStoreMediaReq");
                    string a = getStringImage("anh1.jpg");
                    Images.ImageUrl = @"data:image/" + "jpg" + ";base64," + a;
                    b = Images.ImageUrl;

                    // upload media len
                    int intLength = Convert.ToInt32(FileUpload1.PostedFile.InputStream.Length);
                    byte[] arrContent = new byte[intLength];
                    string imgType = FileUpload1.PostedFile.ContentType;

                    FileUpload1.PostedFile.InputStream.Read(arrContent, 0, intLength);
                    IN20500ImgHelper.IN20500UploadMedia(fullFileName, arrContent, PathImage, IsConfig);

                }
                else
                {
                    X.Msg.Show(new MessageBoxConfig
                    {
                        Buttons = MessageBox.Button.OK,
                        Icon = MessageBox.Icon.ERROR,
                        Title = "Fail",
                        Message = "File format .mp4,.wmv"
                    });
                }
            }
            else
            {
                X.Msg.Show(new MessageBoxConfig
                {
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.ERROR,
                    Title = "Fail",
                    Message = "No file uploaded"
                });
            }

            return Json(new { success = true, imageStream = b, fullFileName = fullFileName });

            //return Json(new { success = true, imageStream = "123", fullFileName = "456" });

        }
        [DirectMethod]
        public ActionResult IN20500SetMediaImage()
        {

            // doi icon anh
            var Images = this.GetCmp<Image>("imgPPCStoreMediaReq");
            string a = getStringImage("anh1.jpg");
            Images.ImageUrl = @"data:image/" + "jpg" + ";base64," + a;
            b = Images.ImageUrl;
            return Json(new { success = true, imageStream = b }, JsonRequestBehavior.AllowGet);
        }
        private string getPathMedia(IN_Inventory inventory, string invtID, string mediaExist)
        {
            string media = "";
            if (mediaExist != "")
            {
                media = string.Format("{0}.mp4", invtID);
            }


            if (!string.IsNullOrWhiteSpace(inventory.Media) && inventory.Media.Contains(".mp4"))
            {
                media = inventory.Media;
            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteMedia(media, PathImage, IsConfig);
            }

            return media;
        }
        private string getPathMediaCopyForm(string tmpCopyFormMedia, string invtID, string tmpOldFileName)
        {
            string media = string.Format("{0}.mp4", invtID);


            if (!string.IsNullOrWhiteSpace(media) && media.Contains(".mp4"))
            {



                IN20500ImgHelper.IN20500CopyMedia(media, tmpOldFileName, PathImage, IsConfig);

            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteMedia(media, PathImage, IsConfig);
            }

            return media;
        }

    }
}

using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using eBiz4DWebFrame;
using eBiz4DWebSys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using HQSendMailApprove;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace AR20200.Controllers
{
    [DirectController]
    public class AR20200Controller : Controller
    {
        private string _screenName = "AR20200";
        private string _beginStatus = "H";
        private string _noneStatus = "N";
        AR20200Entities _db = Util.CreateObjectContext<AR20200Entities>(false);
        eBiz4DWebSysEntities _sys = Util.CreateObjectContext<eBiz4DWebSysEntities>(true);

        private string _pathImage;
        internal string PathImage
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadAR20200");
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

        private bool _isConfig;
        internal bool IsConfig
        {
            get 
            {
                _isConfig = string.IsNullOrWhiteSpace(PathImage) ? false : true;
                return _isConfig;
            }
        }

        //
        // GET: /AR20200/
        public ActionResult Index()
        {
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }
        // Get collection of Sales Person in a branch for binding data (Ajax)
        public ActionResult GetARSalesPersonHeader(string branchId)
        {
            return this.Store(_db.AR_Salesperson.Where(p => p.BranchID == branchId));
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            // Get params from data that's sent from client (Ajax)
            string slsperId = data["cboSlsperId"];
            string branchId = data["cboBranchID"];
            string handle = data["cboHandle"];

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstARSalesPersonHeader"]);
            bool keepStatus = (string.IsNullOrWhiteSpace(handle) || handle == _noneStatus) ? true : false;

            ChangeRecords<AR_Salesperson> lstARSalesPersonHeader = dataHandler.BatchObjectData<AR_Salesperson>();

            foreach (AR_Salesperson updatedSlsperson in lstARSalesPersonHeader.Updated)
            {
                // Get the image path
                string images = getPathThenUploadImage(updatedSlsperson, branchId, slsperId);

                var objHeader = _db.AR_Salesperson.FirstOrDefault(p => p.SlsperId == slsperId && p.BranchID == branchId);
                
                if (objHeader == null)
                {
                    // Create new a sales person
                    objHeader = new AR_Salesperson();
                    objHeader.BranchID = branchId;
                    objHeader.SlsperId = slsperId;
                    objHeader.Status = _beginStatus; // ???
                    objHeader.Images = images;

                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = _screenName;
                    objHeader.Crtd_User = Current.UserName;
                    UpdatingHeader(updatedSlsperson, ref objHeader);

                    // Add data to Sales person
                    _db.AR_Salesperson.AddObject(objHeader);

                    // Add data to Sales Per Hist
                    AddSalesPerHist(objHeader);
                }
                else
                {
                    // Update an existing sales person
                    objHeader.Status = keepStatus ? objHeader.Status : handle; // ???
                    objHeader.Images = images;
                    UpdatingHeader(updatedSlsperson, ref objHeader);
                }

                // If there is a change in handling status (keepStatus is False),
                // add a new pending task with an approved handle.
                if (!keepStatus)
                {
                    var task = _db.HO_PendingTasks.FirstOrDefault(p => p.ObjectID == slsperId && p.EditScreenNbr == _screenName && p.BranchID == branchId);

                    var approveHandle = _db.SI_ApprovalFlowHandle
                        .FirstOrDefault(p => p.AppFolID == _screenName
                                            && p.Status == objHeader.Status 
                                            && p.Handle == handle);
                    if (task == null && approveHandle != null)
                    {
                        if (!approveHandle.Param00.PassNull().Split(',').Any(p => p.ToLower() == "notapprove"))
                        {
                            HO_PendingTasks newTask = new HO_PendingTasks();
                            newTask.BranchID = branchId;
                            newTask.ObjectID = slsperId;
                            newTask.EditScreenNbr = _screenName;
                            newTask.Content = string.Format(approveHandle.ContentApprove, objHeader.SlsperId, objHeader.Name, branchId);
                            newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                            newTask.Crtd_Prog = newTask.LUpd_Prog = _screenName;
                            newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                            newTask.Status = approveHandle.ToStatus;
                            newTask.tstamp = new byte[1];
                            newTask.Parm00 = string.Empty;
                            newTask.Parm01 = string.Empty;
                            newTask.Parm02 = string.Empty;
                            _db.HO_PendingTasks.AddObject(newTask);
                        }
                        objHeader.Status = approveHandle.ToStatus; // ???
                    }
                    _db.SaveChanges();
                    if (approveHandle != null)
                    {
                        X.Msg.Show(new MessageBoxConfig() { 
                            Message="Email sent!"
                        });
                        //Approve.Mail_Approve(_screenName, objHeader.SlsperId,
                        //    approveHandle.RoleID, approveHandle.Status, approveHandle.Handle,
                        //    Current.LangID.ToString(), Current.UserName, branchId, Current.CpnyID,
                        //    string.Empty, string.Empty, string.Empty);
                    }

                }
                else
                {
                    _db.SaveChanges();
                }
                // ===============================================================

                // Get out of the loop (only update the first data)
                break;
            }

            //_db.SaveChanges();
            return Json(new { success = true });

        }

        // Delete a Sales Person and his picture file
        [DirectMethod]
        public ActionResult Delete(string slsperId, string branchId)
        {
            var cpny = _db.AR_Salesperson.FirstOrDefault(p => p.SlsperId == slsperId && p.BranchID == branchId);
            if (cpny != null && cpny.Status==_beginStatus)
            {
                _db.AR_Salesperson.DeleteObject(cpny);
                AR20200ImgHelper.DeleteFile(cpny.Images, PathImage, IsConfig);
            }

            _db.SaveChanges();
            return this.Direct();
        }

        private void UpdatingHeader(AR_Salesperson s, ref AR_Salesperson d)
        {
            d.Addr1 = s.Addr1;
            d.Addr2 = s.Addr2;
            d.CmmnPct = s.CmmnPct;
            d.Country = s.Country;
            d.EMailAddr = s.EMailAddr;
            d.Fax = s.Fax;
            d.Name = s.Name;
            d.Phone = s.Phone;
            d.ProductGroup = s.ProductGroup;
            d.State = s.State;
            d.DeliveryMan = s.DeliveryMan;
            d.Position = s.Position;
            d.District = s.District;
            d.SupID = s.SupID;
            d.CrLmt = s.CrLmt;
            d.Active = s.Active;

            // The Silverlight project does NOT encrypt the PPCPassword.
            d.PPCPassword = s.PPCPassword;

            d.PPCStorePicReq = s.PPCStorePicReq;
            //d.VendID = s.VendID;?
            d.PPCAdmin = s.PPCAdmin;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenName;
            d.LUpd_User = Current.UserName;
        }

        private void AddSalesPerHist(AR_Salesperson obj)
        {
            DateTime now = DateTime.Now;
            AR_SalesPerHist objSalesHist = new AR_SalesPerHist();
            var objSalesHist1 = _db.AR_SalesPerHist
                .Where(p => p.BranchID == obj.BranchID && p.SlsPerID == obj.SlsperId)
                .OrderByDescending(p => p.Seq)
                .FirstOrDefault();

            string strSeq = objSalesHist1 == null ? "0000000001" : ("0000000000" + (double.Parse(objSalesHist1.Seq) + 1));
            strSeq = strSeq.Substring(strSeq.Length - 10, 10);

            objSalesHist.Seq = strSeq;
            objSalesHist.BranchID = obj.BranchID;
            objSalesHist.Crtd_DateTime = now;
            objSalesHist.Crtd_Prog = _screenName;
            objSalesHist.Crtd_User = Current.UserName;
            objSalesHist.SlsPerID = obj.SlsperId;
            objSalesHist.LUpd_DateTime = now;
            objSalesHist.LUpd_Prog = _screenName;
            objSalesHist.LUpd_User = Current.UserName;
            objSalesHist.tstamp = new byte[0];
            objSalesHist.Note = "Tạo mới Nhân viên";
            objSalesHist.FromDate = new DateTime(now.Year, now.Month, now.Day);
            objSalesHist.ToDate = (new DateTime(now.Year, now.Month, now.Day)).AddYears(100);

            _db.AR_SalesPerHist.AddObject(objSalesHist);
        }

        // Upload and preview the image.
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

        // Get image path and then upload the image.
        private string getPathThenUploadImage(AR_Salesperson updatedSlsperson, string branchId, string slsperId)
        {
            string images = string.Format("{0}_{1}.jpg", branchId, slsperId);

            if (!string.IsNullOrWhiteSpace(updatedSlsperson.Images) && !updatedSlsperson.Images.Contains(".jpg"))
            {
                string strImage = updatedSlsperson.Images
                    .Replace("data:image/jpg;base64,", "")
                    .Replace("data:image/png;base64,", "")
                    .Replace("data:image/gif;base64,", "");
 
                // Upload a new file.
                AR20200ImgHelper.AR20200UploadImage(images,
                    Convert.FromBase64CharArray(strImage.ToCharArray(), 0, strImage.Length),
                    PathImage, IsConfig);
            }
            else if (!string.IsNullOrWhiteSpace(updatedSlsperson.Images) && updatedSlsperson.Images.Contains(".jpg"))
            {
                images = updatedSlsperson.Images;
            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                AR20200ImgHelper.DeleteFile(images, PathImage, IsConfig);
            }

            return images;
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
            var a = AR20200ImgHelper.AR20200GetImage(name, PathImage, string.IsNullOrWhiteSpace(PathImage) ? false : true);
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
    }
}

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
using OM22003.Models;
namespace OM22003.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22003Controller : Controller
    {
        private string _screenNbr = "OM22003";
        OM22003Entities _db = Util.CreateObjectContext<OM22003Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM22003");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\OM22003");
                }
                return _filePath;
            }
        }
        //
        // GET: /OM22003/
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

        public ActionResult GetDet(string zone, string territory, string cpnyID,
            string displayID, DateTime? fromDate, DateTime? todate, string status)
        {
            var dets = _db.OM22003_pgAppraise(zone, territory, cpnyID, displayID, fromDate, todate, status).ToList();
            return this.Store(dets);
        }

        public ActionResult GetImage(string branchID, string custID, 
            string displayID, string slsperID, string levelID)
        {
            var imgs = _db.OM22003_pgImage(branchID, custID, displayID, slsperID, levelID).ToList();
            //var imgs = new List<OM22003_pgImage_Result>();
            //imgs.Add(new OM22003_pgImage_Result() {
            //    ASM = branchID + custID + displayID + slsperID + levelID,
            //    LineRef = "1",
            //    Pass = false,
            //    AuditImage = "" 
            //});
            for (int i = 0; i < imgs.Count; i++)
            {
                //imgs[i].ImageFileSrc = (FilePath +"\\"+ "Penguins.jpg").ToBase64Thumbnails(200,100, true);
                //imgs[i].AuditImageSrc = (FilePath + "\\" + "Koala.jpg").ToBase64Thumbnails(200, 100, true);

                imgs[i].ImageFileSrc = (FilePath + "\\" + imgs[i].ImageFile).ToBase64Thumbnails(200, 100, true);
                imgs[i].AuditImageSrc = (FilePath + "\\" + imgs[i].AuditImage).ToBase64Thumbnails(200, 100, true);
            }
            return this.Store(imgs);
        }

        [ValidateInput(false)]
        public ActionResult SaveAppraise(FormCollection data)
        {
            try
            {
                var recHandler = new StoreDataHandler(data["record"]);
                var Appraise = recHandler.ObjectData<OM22003_pgAppraise_Result>()
                            .FirstOrDefault();

                var imgHandler = new StoreDataHandler(data["lstImage"]);
                var lstImage = imgHandler.ObjectData<OM22003_pgImage_Result>()
                            .ToList();

                foreach (var item in lstImage)
                {
                    if (item.LineRef == string.Empty)
                    {
                        item.LineRef = LastLineRef(lstImage);
                    }
                    var image = _db.OM_TDisplayImage.FirstOrDefault(p => p.LineRef == item.LineRef 
                        && p.BranchID == Appraise.BranchID 
                        && p.SlsperID == Appraise.SlsperID 
                        && p.DisplayID == Appraise.DisplayID
                        && p.CustID == Appraise.CustID 
                        && p.LevelID == Appraise.LevelID);
                    if (image == null)
                    {
                        image = new OM_TDisplayImage();
                        image.BranchID = Appraise.BranchID;
                        image.SlsperID = Appraise.SlsperID;
                        image.CustID = Appraise.CustID;
                        image.DisplayID = Appraise.DisplayID;
                        image.LevelID = Appraise.LevelID;
                        image.LineRef = item.LineRef;
                        image.LUpd_DateTime = DateTime.Now;
                        image.LUpd_Prog = _screenNbr;
                        image.LUpd_User = Current.UserName;
                        _db.OM_TDisplayImage.AddObject(image);
                    }
                    image.ImageFile = item.ImageFile;
                    image.ASM = item.ASM;
                    image.Pass = item.Pass.ToBool();
                    image.Crtd_DateTime = DateTime.Now;
                    image.Crtd_Prog = _screenNbr;
                    image.Crtd_User = Current.UserName;

                }
                Appraise.PercentImage = Math.Round(((double)lstImage.Count(p => p.Pass == true) / lstImage.Count * 100), 2);
                var cust = _db.OM_TDisplayCustomer.FirstOrDefault(p => p.BranchID == Appraise.BranchID 
                    && p.CustID == Appraise.CustID 
                    && p.DisplayID == Appraise.DisplayID 
                    && p.LevelID == Appraise.LevelID);
                if (cust != null)
                {
                    cust.PercentImage = Appraise.PercentImage.Value;
                    cust.LUpd_DateTime = DateTime.Now;
                    cust.LUpd_Prog = _screenNbr;
                    cust.LUpd_User = Current.UserName;
                }
                _db.SaveChanges();

                return Json(new { success = true });
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

        private string LastLineRef(List<OM22003_pgImage_Result> mImageList)
        {
            int num = 0;

            foreach (var item in mImageList)
            {
                if (item.LineRef.PassNull() != string.Empty && item.LineRef.ToInt() > num)
                    num = item.LineRef.ToInt();
            }
            num++;
            string lineRef = num.ToString();
            int len = lineRef.Length;
            for (int i = 0; i < 5 - len; i++)
            {
                lineRef = "0" + lineRef;
            }
            return lineRef;
        }

        //private string imageToBin(string fileName)
        //{
        //    string bin = string.Empty;
        //    string filename = FilePath + "\\" + fileName;
        //    if (System.IO.File.Exists(filename))
        //    {
        //        FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
        //        BinaryReader reader = new BinaryReader(fileStream);
        //        byte[] imageBytes = reader.ReadBytes((int)fileStream.Length);
        //        reader.Close();

        //        var imgString64 = Convert.ToBase64String(imageBytes, 0, imageBytes.Length);

        //        bin = @"data:image/jpg;base64," + imgString64;
        //    }

        //    return bin;
        //} 
    }
}

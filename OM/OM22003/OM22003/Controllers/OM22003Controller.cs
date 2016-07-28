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

        public ActionResult GetDet(string zone, string territory, string cpnyID, string displayID, DateTime? fromDate, DateTime? toDate)
        {
            _db.CommandTimeout = int.MaxValue;
            var dets = _db.OM22003_pgAppraise(zone, territory, cpnyID, displayID, fromDate, toDate).ToList();
            return this.Store(dets);
        }



        public ActionResult GetImage(string branchID, string custID, string displayID, string slsperID, DateTime? fromDate,DateTime? toDate, int width = 150, int height = 100)
        {
            var imgs = _db.OM22003_pgImage(branchID, custID, displayID, slsperID, fromDate,toDate).ToList();
            //var imgs = new List<OM22003_pgImage_Result>();
            //imgs.Add(new OM22003_pgImage_Result()
            //{
            //    CreateDate = DateTime.Now,
            //    ImageName = "Penguins.jpg",
            //    //ImageSrc = (FilePath + "\\" + "Penguins.jpg").ToBase64Thumbnails(200, 100, true)
            //});
            for (int i = 0; i < imgs.Count; i++)
            {
                try
                {
                    imgs[i].ImageSrc = FilePath.ToUpper().StartsWith("HTTP") ? (FilePath.TrimEnd('/') + "/" + imgs[i].ImageName).ImageURL(width, height) : (FilePath + "\\" + imgs[i].ImageName).ToBase64Thumbnails(width, height, true);
                }
                catch
                {
                    imgs[i].ImageSrc = "A";
                }
            }
            return this.Store(imgs);
        }

        [ValidateInput(false)]
        public ActionResult SaveAppraise(FormCollection data, bool pass,DateTime dateDisplay)
        {
            try
            {
                var recHandler = new StoreDataHandler(data["record"]);
                var Appraise = recHandler.ObjectData<OM22003_pgAppraise_Result>()
                            .FirstOrDefault();

                var recAppraise = _db.OM_TDisplayResult.FirstOrDefault(
                    p => p.BranchID == Appraise.BranchID && p.SlsperID == Appraise.SlsperID
                        && p.CustID == Appraise.CustID && p.DisplayID == Appraise.DisplayID && p.Date == dateDisplay);
                if (recAppraise != null)
                {
                    if (recAppraise.tstamp.ToHex() == Appraise.tstamp.ToHex())
                    {
                        // Chua tuong bao gio cham diem moi dc cap nhat
                        if (string.IsNullOrWhiteSpace(Appraise.Pass))
                        {
                            recAppraise.Pass = pass;
                            recAppraise.Remark = Appraise.Remark;
                            recAppraise.LUpd_DateTime = DateTime.Now;
                            recAppraise.LUpd_Prog = _screenNbr;
                            recAppraise.LUpd_User = Current.UserName;
                          
                        }
                      
                    }
                    else {
                        throw new MessageException("19");
                    }
                }
                else {
                    recAppraise = new OM_TDisplayResult();
                    recAppraise.ResetET();

                    recAppraise.BranchID = Appraise.BranchID;
                    recAppraise.CustID = Appraise.CustID;
                    recAppraise.Date = dateDisplay;
                    recAppraise.DisplayID = Appraise.DisplayID;
                    recAppraise.LevelID = Appraise.LevelID;
                    recAppraise.Rate = 0;
                    recAppraise.SlsperID = Appraise.SlsperID;
                    recAppraise.Territory = Appraise.Territory;
                    recAppraise.Zone = Appraise.Zone;

                    recAppraise.Pass = pass;
                    recAppraise.Remark = Appraise.Remark;

                    recAppraise.Crtd_DateTime = DateTime.Now;
                    recAppraise.Crtd_Prog = _screenNbr;
                    recAppraise.Crtd_User = Current.UserName;

                    recAppraise.LUpd_DateTime = DateTime.Now;
                    recAppraise.LUpd_Prog = _screenNbr;
                    recAppraise.LUpd_User = Current.UserName;

                    _db.OM_TDisplayResult.AddObject(recAppraise);
                    //throw new MessageException("8");
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

            //foreach (var item in mImageList)
            //{
            //    if (item.LineRef.PassNull() != string.Empty && item.LineRef.ToInt() > num)
            //        num = item.LineRef.ToInt();
            //}
            num++;
            string lineRef = num.ToString();
            int len = lineRef.Length;
            for (int i = 0; i < 5 - len; i++)
            {
                lineRef = "0" + lineRef;
            }
            return lineRef;
        }

        public ActionResult Download(string fileName)
        {
            string path = this.FilePath + @"\" + fileName;
            if (System.IO.File.Exists(path))
            {
                FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(input);
                byte[] inArray = reader.ReadBytes((int)input.Length);
                reader.Close();
                string str2 = Convert.ToBase64String(inArray, 0, inArray.Length);
                return new FileContentResult(inArray, "image/jpeg");
            }
            return null;
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

        private string imageToBin(string fileName)
        {
            string str = string.Empty;
            string path = this.FilePath + @"\" + fileName;
            if (System.IO.File.Exists(path))
            {
                FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(input);
                byte[] inArray = reader.ReadBytes((int)input.Length);
                reader.Close();
                string str3 = Convert.ToBase64String(inArray, 0, inArray.Length);
                str = "data:image/jpg;base64," + str3;
            }
            return str;
        }

    }
}

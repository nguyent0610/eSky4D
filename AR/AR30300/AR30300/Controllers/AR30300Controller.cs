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
        private JsonResult _logMessage;

        //private string _Path;
        //internal string PathImage
        //{
        //    get
        //    {
        //        var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "PublicAR30300");
        //        if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
        //        {
        //            _Path = config.TextVal;
        //        }
        //        else
        //        {
        //            throw new MessageException(MessageType.Message, "2016111510");
        //        }
        //        return _Path;
        //    }
        //}

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

        public ActionResult GetAR_Customer(string Zone, string Territory, string BranchID, string ClassID, string SlsperID, string CustID)
        {
            return this.Store(_db.AR30300_pgCustomer(_userName, Zone, Territory, BranchID, ClassID, SlsperID, CustID).ToList());
        }

        public ActionResult AR30300DownloadAlbum(string[] urls)
        {
            try
            {
                string resultAlbum = "";
                string path = Server.MapPath("~") + @"Images\AR30300\Album\";
                if (!Directory.Exists(path))
                {
                    throw new MessageException(MessageType.Message, "2016111510");
                }
                else
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(path);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    for (int i = 0; i < urls.Length; i++)
                    {
                        string remoteFilePath = urls[i];
                        Uri remoteFilePathUri = new Uri(remoteFilePath);
                        string remoteFilePathWithoutQuery = remoteFilePathUri.GetLeftPart(UriPartial.Path);
                        string fileName = Path.GetFileName(remoteFilePathWithoutQuery);
                        string localPath = path + fileName;
                        resultAlbum += fileName + ";";
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(remoteFilePath, localPath);
                        webClient.Dispose();
                    }
                    if (resultAlbum != "")
                        resultAlbum = resultAlbum.TrimEnd(';');
                }
                return Json(new { success = true, strResult = resultAlbum });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
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

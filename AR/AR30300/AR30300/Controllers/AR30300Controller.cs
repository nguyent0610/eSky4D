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

        private string _Path;
        internal string PathImage
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "PublicAR30300");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _Path = config.TextVal;
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2016111510");
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

        public ActionResult GetAR_Customer(string Zone, string Territory, string BranchID, string ClassID, string SlsperID, string CustID)
        {
            return this.Store(_db.AR30300_pgCustomer(_userName, Zone, Territory, BranchID, ClassID, SlsperID, CustID).ToList());
        }

        //[HttpPost]
        //public ActionResult Save(FormCollection data)
        //{
        //    try
        //    {
        //        StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstAR_Customer"]);
        //        var lstAR_Customer = dataHandler1.ObjectData<AR30300_pgAR_Customer_Result>() == null ? new List<AR30300_pgAR_Customer_Result>() : dataHandler1.ObjectData<AR30300_pgAR_Customer_Result>();

        //        foreach (var item in lstAR_Customer)
        //        {
        //            if (item.colCheck == false) continue;
        //            var obj = _db.AR_Customer.FirstOrDefault(p => p.BranchID == item.BranchID && p.CustId == item.CustId);
        //            if (obj != null)
        //            {
        //                bool flag = false;
        //                if (item.ClassId2_S != "")
        //                {
        //                    obj.ClassId2 = item.ClassId2_S;
        //                    flag = true;
        //                }
        //                if (item.Status_S != "")
        //                {
        //                    obj.GiftExchange = item.Status_S == "I" ? true : false;
        //                    flag = true;
        //                }
        //                if (flag)
        //                {
        //                    obj.LUpd_Datetime = DateTime.Now;
        //                    obj.LUpd_Prog = _screenNbr;
        //                    obj.LUpd_User = _userName;
        //                }
        //            }
        //        }

        //        _db.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}

        //[DirectMethod]
        //public ActionResult AR30300DownloadImageSelect(string files)
        //{
        //    string[] strFiles = files.Split(';');
        //    for(int i = 0; i < strFiles.Length; i++){
        //        using (WebClient webClient = new WebClient())
        //        {
        //            webClient.DownloadFile(PathImage + strFiles[i], strFiles[i]);
        //        }
        //    }
        //    return this.Direct();
        //}
    }
}

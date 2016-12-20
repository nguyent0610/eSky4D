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

namespace OM42700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM42700Controller : Controller
    {
        private string _screenNbr = "OM42700";
        private string _userName = Current.UserName;
        OM42700Entities _db = Util.CreateObjectContext<OM42700Entities>(false);
        private JsonResult _logMessage;
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
        public ActionResult GetPDA(string BranchID,string SlsperID,DateTime FromDate,DateTime ToDate)
        {
            return this.Store(_db.OM42700_pgPDA(Current.CpnyID, Current.UserName, Current.LangID,BranchID,SlsperID,FromDate,ToDate).ToList());
        }
        public ActionResult GetOrder(string BranchID, string SlsperID, DateTime FromDate, DateTime ToDate)
        {
            return this.Store(_db.OM42700_pgOrder(Current.CpnyID, Current.UserName, Current.LangID, BranchID, SlsperID, FromDate, ToDate).ToList());
        }
    }
}

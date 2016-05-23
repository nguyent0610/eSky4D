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
namespace IN30200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN30200Controller : Controller
    {
        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN30200";
        private string _userName = Current.UserName;
        IN30200Entities _db = Util.CreateObjectContext<IN30200Entities>(false);
        private JsonResult mLogMessage;
        private FormCollection mForm;
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetDetail(string InvtID, string BranchID)
        {
            return this.Store(_db.IN30200_pgDetail(Current.UserName, Current.CpnyID, Current.LangID, InvtID, BranchID).ToList());
        }

        public ActionResult GetHeader(string InvtID)
        {
            return this.Store(_db.IN30200_LoadText(Current.UserName, Current.CpnyID, Current.LangID, InvtID).ToList());
        }

    }
}

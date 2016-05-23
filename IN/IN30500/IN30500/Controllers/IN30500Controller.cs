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
namespace IN30500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN30500Controller : Controller
    {
        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN30500";
        private string _userName = Current.UserName;
        IN30500Entities _db = Util.CreateObjectContext<IN30500Entities>(false);
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

        public ActionResult GetHeader(string InvtID, string BranchID, string Site, string FirstDate, string LastDate, string Status)
        {
            DateTime FromDate_tmp = DateTime.Parse(FirstDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(LastDate.PassNull());

            return this.Store(_db.IN30500_ppQuickQueryInvt(Current.UserName, Current.CpnyID, Current.LangID, InvtID, BranchID, Site, FromDate_tmp, ToDate_tmp, Status).ToList());
        }
        public ActionResult GetDetail(string InvtID, string BranchID, string Site, string FirstDate, string LastDate, string Status)
        {
            DateTime FromDate_tmp = DateTime.Parse(FirstDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(LastDate.PassNull());

            return this.Store(_db.IN30500_ppQuickQuerySite(Current.UserName, Current.CpnyID, Current.LangID, InvtID, BranchID, Site, FromDate_tmp, ToDate_tmp, Status).ToList());
        }

    }
}

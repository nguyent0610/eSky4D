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
namespace OM30100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM30100Controller : Controller
    {
        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "OM30100";
        private string _userName = Current.UserName;
        OM30100Entities _db = Util.CreateObjectContext<OM30100Entities>(false);
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

        public ActionResult GetDetail(string CustID, string InvenID, string SiteID, string CusPO, string invoice
            , string Status, string BranchID, string FromDate, string ToDate, string OrdNbrBranch)
        {
            DateTime FromDate_tmp = DateTime.Parse(FromDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(ToDate.PassNull());
            return this.Store(_db.OM30100_open_close_Detail(CustID, InvenID, SiteID, CusPO, invoice, Status
                , BranchID, FromDate_tmp, ToDate_tmp,OrdNbrBranch).ToList());
        }

        public ActionResult GetHeader(string CustID, string InvenID, string SiteID, string CusPO, string invoice
            , string Status, string BranchID, string FromDate, string ToDate)
        {
            DateTime FromDate_tmp = DateTime.Parse(FromDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(ToDate.PassNull());
            return this.Store(_db.OM30100_open_close_Header(CustID, InvenID, SiteID, CusPO, invoice, Status
                , BranchID, FromDate_tmp, ToDate_tmp).ToList());
        }

    }
}

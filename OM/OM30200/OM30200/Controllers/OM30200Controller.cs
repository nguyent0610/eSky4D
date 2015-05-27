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
namespace OM30200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM30200Controller : Controller
    {
        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "OM30200";
        private string _userName = Current.UserName;
        OM30200Entities _db = Util.CreateObjectContext<OM30200Entities>(false);
        private JsonResult mLogMessage;
        private FormCollection mForm;
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

        public ActionResult GetDetail(string CustID, string InvtID, string SiteID, string CustPO, string invoice
            , string Status, string BranchID, string FromDate, string ToDate, string SlsRoute)
        {
            DateTime FromDate_tmp = DateTime.Parse(FromDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(ToDate.PassNull());
            return this.Store(_db.OM30200_SalesInquiryDetail(CustID, InvtID, SiteID, CustPO, invoice, Status
                , BranchID, FromDate_tmp, ToDate_tmp, SlsRoute).ToList());
        }

        public ActionResult GetHeader(string CustID, string InvtID, string SiteID, string CustPO, string invoice
            , string Status, string BranchID, string FromDate, string ToDate, string SlsRoute)
        {
            DateTime FromDate_tmp = DateTime.Parse(FromDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(ToDate.PassNull());
            return this.Store(_db.OM30200_SalesInquiryHeader(CustID, InvtID, SiteID, CustPO, invoice, Status
                , BranchID, FromDate_tmp, ToDate_tmp, SlsRoute).ToList());
        }

    }
}

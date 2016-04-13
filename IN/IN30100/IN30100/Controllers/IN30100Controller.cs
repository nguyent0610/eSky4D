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
namespace IN30100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN30100Controller : Controller
    {
        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN30100";
        private string _userName = Current.UserName;
        IN30100Entities _db = Util.CreateObjectContext<IN30100Entities>(false);
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

        public ActionResult GetIN_Transactions(string BranchID, string InvtID, string CustID,string VendID, string SiteID
            , string TranType,string JrnlType, string Status, string FromDate,string ToDate)
        {
            DateTime FromDate_tmp = DateTime.Parse(FromDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(ToDate.PassNull());
            short Status_tmp = short.Parse(Status);

            return this.Store(_db.IN30100_pgTransactionsQuery(Current.UserName,Current.CpnyID,Current.LangID, InvtID, CustID, VendID, SiteID, TranType, JrnlType, Status_tmp, FromDate_tmp, ToDate_tmp).ToList());
        }

        public ActionResult GetIN30100_GetStockBegEndBal(string BranchID, string InvtID, string CustID, string VendID, string SiteID
              , string TranType, string JrnlType, string Status, string FromDate, string ToDate)
        {
            DateTime FromDate_tmp = DateTime.Parse(FromDate.PassNull());
            DateTime ToDate_tmp = DateTime.Parse(ToDate.PassNull());
            short Status_tmp = short.Parse(Status);

            return this.Store(_db.IN30100_GetStockBegEndBal(BranchID, InvtID, CustID, VendID, SiteID, TranType, JrnlType, Status_tmp, FromDate_tmp, ToDate_tmp).ToList());
        }

    }
}

using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using eBiz4DWebFrame;
using eBiz4DWebSys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using HQSendMailApprove;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace AR10200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10200Controller : Controller
    {
        private string _screenName = "AR10200";
        AR10200Entities _db = Util.CreateObjectContext<AR10200Entities>(false);

        //
        // GET: /AR10200/
        public ActionResult Index()
        {
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult GetListAdjust(string branchId, string batNbr
            , string custId, string slsperId, string deliveryId
            , string refNbr, DateTime fromDate, DateTime toDate
            , string dateType, string isGridF3) 
        {
            var gridData = _db.AR10200_BindingGrid(batNbr, branchId, custId, slsperId, deliveryId, refNbr, fromDate, toDate, dateType, isGridF3);
            return this.Store(gridData);
        }
    }
}

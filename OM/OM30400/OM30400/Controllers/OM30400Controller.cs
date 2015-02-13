using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using HQ.eSkyFramework;

namespace OM30400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM30400Controller : Controller
    {
        private string _screenName = "OM30400";
        OM30400Entities _db = Util.CreateObjectContext<OM30400Entities>(false);

        //
        // GET: /OM30400/
        public ActionResult Index()
        {
            Util.InitRight(_screenName);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "name")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult LoadVisitCustomerPlan(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var planVisit = _db.OM30400_pgVisitCustomerPlan(Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(planVisit);
        }

        public ActionResult LoadMCL(string channel, string territory,
            string province, string distributor, string shopType) 
        {
            var mcl = _db.OM30400_pgLoadMCL(Current.CpnyID, Current.UserName, territory, province, distributor, channel, shopType).ToList();
            return this.Store(mcl);
        }

        public ActionResult LoadVisitCustomerActual(string distributor, string slsperId, DateTime visitDate)
        {
            var actualVisit = _db.OM30400_pgVisitCustomerActual(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate).ToList();
            return this.Store(actualVisit);
        }

        public ActionResult LoadAllCurrentSalesman(string distributor, DateTime visitDate)
        {
            var allSlspers = _db.OM30400_pgAllCurrentSalesman(Current.CpnyID, Current.UserName, distributor, visitDate).ToList();
            return this.Store(allSlspers);
        }

        public ActionResult LoadCustHistory(string distributor, string slsperId, string customer, DateTime startDate, DateTime endDate)
        {
            var cusHistory = _db.OM30400_pgHistory(Current.CpnyID, Current.UserName, distributor, slsperId, customer, startDate, endDate).ToList();
            return this.Store(cusHistory);
        }
    }
}

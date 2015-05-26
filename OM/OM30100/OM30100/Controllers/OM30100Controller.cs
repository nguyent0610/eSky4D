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
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        //public ActionResult GetDetail(string InvtID, string BranchID)
        //{
        //    return this.Store(_db.OM30100_open_close_Detail(InvtID, BranchID).ToList());
        //}

        //public ActionResult GetHeader(string InvtID)
        //{
        //    return this.Store(_db.OM30100_open_close_Header(InvtID).ToList());
        //}

    }
}

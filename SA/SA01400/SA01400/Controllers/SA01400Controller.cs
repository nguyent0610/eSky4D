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
namespace SA01400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA01400Controller : Controller
    {
        private string _screenNbr = "SA01400";
        private string _userName = Current.UserName;
        SA01400Entities _db = Util.CreateObjectContext<SA01400Entities>(true);
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

        public ActionResult GetSYS_BuildLog()
        {
            return this.Store(_db.SA01400_pgSYS_BuildLog().ToList());
        }
        
    }
}

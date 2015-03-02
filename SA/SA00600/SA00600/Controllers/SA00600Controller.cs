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
namespace SA00600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00600Controller : Controller
    {
        private string _screenNbr = "SA00600";
        private string _userName = Current.UserName;
        SA00600Entities _db = Util.CreateObjectContext<SA00600Entities>(true);
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
        public ActionResult GetMailHeader(string accessDate)
        {
            //DateTime access;
            //if (accessDate != "")
            //    access = Convert.ToDateTime(accessDate);
            //else
            //    access = DateTime.Now;
            DateTime access;
            if (!string.IsNullOrWhiteSpace(accessDate))
            { 
                access=Convert.ToDateTime(accessDate);
                return this.Store(_db.SA00600_pgLoginHistory(access).ToList());
            }
            else
            {
                return this.Store(_db.SA00600_pgLoginHistory(null).ToList());
            }
            
        }
        
    }
}

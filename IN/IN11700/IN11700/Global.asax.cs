using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using HQ.eSkyFramework;
using System.Configuration;

namespace IN11700
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Session_Start(object sender, EventArgs e)
        {
            Current.Authorize = false;
            Current.Server = "HOANGSAD";// ConfigurationManager.AppSettings["Server"].ToString();
            Current.DBSys = "PhuThai_Test_eSky4DSys";//ConfigurationManager.AppSettings["DBSys"].ToString();
            Current.FormatDate = "MM.dd.yyyy";
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            Session["IN11700"] = acc;
            Session["DBApp"] = Current.DBApp = "PhuThai_Test_eSky4DApp";// "eBiz4DWebApp";
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "LCUS-HCM-0004";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;
        }
    }
}
using HQ.eSkyFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace OM20090
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
            Current.Server = "TRUONGSAD";
            Current.DBSys = "PhuThai_eSky4DSys";
            AccessRight acc = new AccessRight();
            Current.FormatDate = "dd.MM.yyyy";
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            Session["OM20090"] = acc;
            Session["DBApp"] = Current.DBApp = "PhuThai_eSky4DApp";
            Session["UserName"] = Current.UserName = "admin"; //MTR-RSM HCM-MS-01 
            Session["CpnyID"] = Current.CpnyID = "18247";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;

        }
    }
}
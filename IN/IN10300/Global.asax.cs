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

namespace IN10300
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
            Current.FormatDate = "dd/MM/yyyy";
            Current.Authorize = false;
            Current.Server = "TRUONGSAD";////192.168.130.4//EARTHSVR--TRUONGSAD
            Current.DBSys = "PHUTHAI_eSky4DSys";////eBiz4DCloudSysJFV
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            Session["IN10300"] = acc;
            Session["DBApp"] = Current.DBApp = "PHUTHAI_eSky4DApp";//eBiz4DCloudAppJFV
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "CHHCM0007";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;
        }
    }
}
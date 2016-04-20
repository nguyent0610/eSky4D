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

namespace OM23600
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

            Current.Server = "MARSSVR\\SQL2012"; // MARSSVR EARTHSVR
            Current.FormatDate = "dd.MM.yyyy";  // MM/dd/yyyy  dd-MM-yyyy       
            Current.DBSys = "eSky4DSys"; // ND_eSky4DSys
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = false;
            Session["OM23600"] = acc;
            Session["DBApp"] = Current.DBApp = "eSky4DApp"; // ND_eSky4DApp
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "HQHD3110";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;
        }
    }
}
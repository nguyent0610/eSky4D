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

namespace PO10200
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Session_Start(object sender, EventArgs e)
        {
            Current.Authorize = false;
            Current.Server = "MARSSVR\\SQL2012";// ConfigurationManager.AppSettings["Server"].ToString(); //"EARTHSVR\\SQL2012";
            Current.DBSys = "eSky4DSys";// "eBiz4DWebSys";// ConfigurationManager.AppSettings["DBSys"].ToString();
            Current.Theme = "Default";
            Current.FormatDate = "dd.MM.yyyy";
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            acc.Release = true;
            Session["PO10200"] = acc;
            Session["DBApp"] = Current.DBApp = "eSky4DApp";// "eBiz4DWebApp";
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "NDMT";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 0;
           
        }
    }
}
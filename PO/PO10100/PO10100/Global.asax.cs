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

namespace PO10100
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
            Current.Server ="EARTHSVR\\SQL2014";// ConfigurationManager.AppSettings["Server"].ToString();// "EARTHSVR\\SQL2012";//
            Current.DBSys = "eBiz4DCloudSysKAO";// "eBiz4DWebSys"; ND_eSky4DSys// ConfigurationManager.AppSettings["DBSys"].ToString();
            Current.Theme = "Default";
            Current.FormatDate = "dd.MM.yyyy";
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            Session["PO10100"] = acc;
            Session["DBApp"] = Current.DBApp = "eBiz4DCloudAppKAO";// "eBiz4DWebApp";ND_eSky4DApp
            Session["UserName"] = Current.UserName = "ADMIN";
            Session["CpnyID"] = Current.CpnyID = "18591001";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;
           
        }
    }
}
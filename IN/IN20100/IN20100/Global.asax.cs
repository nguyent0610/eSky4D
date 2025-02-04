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

namespace IN20100
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
            Current.Server = "TRUONGSAD";// ConfigurationManager.AppSettings["Server"].ToString(); //"EARTHSVR\\SQL2012";
            Current.DBSys = "HT_eSky4DSys";// "eBiz4DWebSys";// ConfigurationManager.AppSettings["DBSys"].ToString();
            Current.Theme = "Default";
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            acc.Release = true;
            Session["IN20100"] = acc;
            Session["DBApp"] = Current.DBApp = "HT_eSky4DApp";// "eBiz4DWebApp";
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "NDMT";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;
            //Current.Authorize = false;
            //Current.Server = "MARSSVR\\SQL2012";
            //Current.DBSys = "eSky4DSys";
            //Current.FormatDate = "dd-MM-yyyy";
            //AccessRight acc = new AccessRight();
            //acc.Delete = true;
            //acc.Insert = true;
            //acc.Update = true;
            //Session["SI20100"] = acc;
            //Session["DBApp"] = Current.DBApp = "eSky4DApp";
            //Session["UserName"] = Current.UserName = "admin";
            //Session["CpnyID"] = Current.CpnyID = "HQHD3110";
            //Session["Language"] = Current.Language = "vi";
            //Session["LangID"] = 1;
        }
    }
}
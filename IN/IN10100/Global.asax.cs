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

namespace IN10100
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
            //Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
            //Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
            //Session["DBApp"] = Current.DBApp = "eBiz4DWebApp";
            //Session["UserName"] = Current.UserName = "admin";
            //Session["CpnyID"] = Current.CpnyID = "HQH00000";
            //Session["Language"] = Current.Language = "vi";
            //Session["LangID"] = 1;

            Current.Authorize = false;
            //Current.Server = "EARTHSVR\\SQL2012";// ConfigurationManager.AppSettings["Server"].ToString();
            Current.Server = "TRUONGSAD";// TRUONGSAD   HOANGSAD
            Current.DBSys = "PhuThai_eSky4DSys";//ConfigurationManager.AppSettings["DBSys"].ToString(); PhuThai_Test_eSky4DSys PhuThai_eSky4DSys
            Current.FormatDate = "dd-MM-yyyy";
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            Session["IN10100"] = acc;
            Session["DBApp"] = Current.DBApp = "PhuThai_eSky4DApp";// "eBiz4DWebApp"; PhuThai_Test_eSky4DApp  PhuThai_eSky4DApp
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "040100001";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;
        }
    }
}
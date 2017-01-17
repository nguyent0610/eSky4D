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

namespace AR20500
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
            Current.FormatDate = "dd/MM/yyyy";
            Current.Authorize = false;
            Current.Server = "HOANGSAD"; //"EARTHSVR\\SQL2014";//"HQDBDG";//"EARTHSVR\\SQL2012";// ConfigurationManager.AppSettings["Server"].ToString();
            Current.DBSys = "ANVM_Test_eSky4DSys";//"ANVM_eSky4DSys";//"ND_eSky4DSys";//ConfigurationManager.AppSettings["DBSys"].ToString();
            AccessRight acc = new AccessRight();
            acc.Delete = true;
            acc.Insert = true;
            acc.Update = true;
            Session["AR20500"] = acc;
            Session["DBApp"] = Current.DBApp = "ANVM_Test_eSky4DApp";//"ANVM_eSky4DApp";//"ND_eSky4DApp";// "eBiz4DWebApp";
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "HQHD3120";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] =1;
        }
    }
}
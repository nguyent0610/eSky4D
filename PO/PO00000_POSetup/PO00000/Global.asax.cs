﻿using eBiz4DWebFrame;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PO00000
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
            Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
            Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
            Session["DBApp"] = Current.DBApp = "eBiz4DWebApp";
            Session["UserName"] = Current.UserName = "admin";
            Session["CpnyID"] = Current.CpnyID = "HQH00000";
            Session["Language"] = Current.Language = "vi";
            Session["LangID"] = 1;
        }
      
    }
}
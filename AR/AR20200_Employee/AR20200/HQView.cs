using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AR20200
{
    public class HQViewEngine : RazorViewEngine
    {
        public HQViewEngine(): base()
        {
            AreaViewLocationFormats = new[]
            {
                "~/Areas/{2}/HQViews/{1}/{0}.hqview",
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };
            AreaMasterLocationFormats = new[]
            {
                 "~/Areas/{2}/HQViews/{1}/{0}.hqview",
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };
            AreaPartialViewLocationFormats = new[]
            {
                "~/Areas/{2}/HQViews/{1}/{0}.hqview",
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            ViewLocationFormats = new[]
            {
                "~/HQViews/{1}/{0}.hqview",
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };
            MasterLocationFormats = new[]
            {
                "~/HQViews/{1}/{0}.hqview",
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };
            PartialViewLocationFormats = new[]
            {
                "~/HQViews/{1}/{0}.hqview",
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            FileExtensions = new[]
            {
                "hqview",
                "cshtml",
                "vbhtml",
            };
        }
       
        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            if(viewPath.EndsWith(".hqview"))
            {
                var t = controllerContext.RouteData.Values[viewPath].ToString();

                return base.CreateView(controllerContext, t, masterPath);
            
            }
            else 
            {
                 return base.CreateView(controllerContext, viewPath, masterPath);
            }

           
        }
 
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            if (partialPath.EndsWith(".hqview"))
            {

                var t = controllerContext.RouteData.Values[partialPath].ToString();
                //System.IO.File.WriteAllLines(dir, lines);

                return base.CreatePartialView(controllerContext, t);
            }
            else 
            {
                 return base.CreatePartialView(controllerContext, partialPath);
            }
            
        }
 
        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {

            if (virtualPath.EndsWith(".hqview") && File.Exists(controllerContext.HttpContext.Server.MapPath(virtualPath)))
            {
                string newFile=string.Empty;
                try
                {
                    string path = controllerContext.HttpContext.Server.MapPath(virtualPath);
                    string dir = (new DirectoryInfo(path)).Parent.FullName;
                    string name = Guid.NewGuid().ToString().Replace("-", "") + ".hqhtml";

                    newFile=dir + @"\" + name;
                    System.IO.File.WriteAllLines(newFile, File.ReadAllLines(path));

                    var oldVirtualPath = virtualPath;

                    virtualPath = virtualPath.Substring(0,virtualPath.LastIndexOf("/") +1) + name;

                    controllerContext.RouteData.Values.Add(oldVirtualPath, virtualPath);

                }
                catch (Exception)
                {
                }
                finally
                {
                    File.Delete(newFile);
                }
                   

              
            }
            return (base.FileExists(controllerContext, virtualPath));
        }

    }

   
}
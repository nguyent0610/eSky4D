using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using HQ.eSkyFramework;
using System.IO;
using Aspose.Cells;
using HQFramework.DAL;
using System.Drawing;
using System.Data;
using System.Configuration;


namespace AR22300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR22300Controller : Controller
    {
        private string _screenName = "AR22300";
        AR22300Entities _db = Util.CreateObjectContext<AR22300Entities>(false);

        //
        // GET: /AR22300/
        public ActionResult Index(string data)
        {

            if (data != null)//dung cho PVN lay du lieu tu silverlight goi len
            {
                // user;company;langid => ?data=admin;LCUS-HCM-0004;1
                try
                {
                    data = data.Replace(" ", "+");
                    data = Encryption.Decrypt(data, DateTime.Now.ToString("yyyyMMdd"));
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();

                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["UserName"] = Current.UserName = data.Split(';')[0];
                    Session["CpnyID"] = Current.CpnyID = data.Split(';')[1];
                    Session["Language"] = Current.Language = short.Parse(data.Split(';')[2]) == 0 ? "en" : "vi";
                    Session["LangID"] = short.Parse(data.Split(';')[2]);
                    Util.InitRight(_screenName);
                }
                catch
                {
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["Language"] = Current.Language = ConfigurationManager.AppSettings["LangID"].ToString();
                    Session["LangID"] = Current.Language == "vi" ? 1 : 0;
                    ViewBag.Title = Util.GetLang("AR22300");
                    ViewBag.Error = Message.GetString("225", null);
                    return View("Error");
                }
            }
            if (Current.UserName.PassNull() == "")
            {
                AccessRight acc = new AccessRight();
                acc.Delete = false;
                acc.Insert = false;
                acc.Update = false;
                Session["AR22300"] = acc;
                Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                Session["Language"] = Current.Language = ConfigurationManager.AppSettings["LangID"].ToString();
                Session["LangID"] = Current.Language == "vi" ? 1 : 0;
                ViewBag.Title = Util.GetLang("AR22300");
                ViewBag.Error = Message.GetString("225", null);
                return View("Error");
            }
            ViewBag.Title = Util.GetLang("AR22300");
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult LoadMCP(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var planVisit = _db.AR22300_pgMCP(Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(planVisit);
        }

        [ValidateInput(false)]
        public ActionResult ResetGeo(FormCollection data) 
        {
            try
            {
                var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                var lstSelCust = selCustHandler.ObjectData<AR22300_pgMCP_Result>()
                            .Where(p => !string.IsNullOrWhiteSpace(p.CustId)).ToList();

                if (lstSelCust.Count > 0)
                {
                    foreach (var selCust in lstSelCust)
                    {
                        var custLoc = _db.AR_CustomerLocation.FirstOrDefault(p => p.CustID == selCust.CustId
                            && p.BranchID == selCust.BranchID);
                        if (custLoc != null)
                        {
                            if (custLoc.tstamp.ToHex() == selCust.tstamp.ToHex())
                            {
                                if (custLoc.Lat != 0 && custLoc.Lng != 0)
                                {
                                    custLoc.Lat = 0;
                                    custLoc.Lng = 0;
                                }
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "2015052202", "",
                                    new string[] { selCust.CustId });
                        }
                    }

                    _db.SaveChanges();
                    return Json(new { success = true, msgCode = 201405071 });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2015052201", "",
                        new string[] { Util.GetLang("Customer") });
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }
    }
}

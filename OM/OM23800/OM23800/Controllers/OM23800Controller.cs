﻿using System.Web.Mvc;
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
using HQFramework.Common;
using System.Web;
using System.Globalization;
using System.Data.SqlClient;
using HQ.eSkySys;

namespace OM23800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23800Controller : Controller
    {
        private string _screenName = "OM23800";
        OM23800Entities _db = Util.CreateObjectContext<OM23800Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;
        private bool _checkRequireImport = false;
        public ActionResult Index(string data)
        {

            if (data != null)//dung cho PVN lay du lieu tu silverlight goi len
            {
                // user;company;langid => ?data=admin;LCUS-HCM-0004;1
                try
                {
                    //data = data.Replace(" ", "+");
                    //data = Encryption.Decrypt(data, DateTime.Now.ToString("yyyyMMdd"));
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();

                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["UserName"] = Current.UserName = data.Split(';')[0];
                    Session["CpnyID"] = Current.CpnyID = data.Split(';')[1];
                    Session["Language"] = Current.Language = short.Parse(data.Split(';')[2]) == 0 ? "en" : "vi";
                    Session["LangID"] = short.Parse(data.Split(';')[2]);
                    //Util.InitRight(_screenName);
                }
                catch
                {
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["Language"] = Current.Language = ConfigurationManager.AppSettings["LangID"].ToString();
                    Session["LangID"] = Current.Language == "vi" ? 1 : 0;
                    ViewBag.Title = Util.GetLang("OM23800");
                    ViewBag.Error = Message.GetString("225", null);
                    return View("Error");
                }
            }
            if (Current.UserName.PassNull() == "")
            {
                Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                Session["Language"] = Current.Language = ConfigurationManager.AppSettings["LangID"].ToString();
                Session["LangID"] = Current.Language == "vi" ? 1 : 0;
                ViewBag.Title = Util.GetLang("OM23800");
                ViewBag.Error = Message.GetString("225", null);
                return View("Error");
            }
            var objConfig = _sys.SYS_Configurations.FirstOrDefault(p => p.Code.ToUpper() == "OM23800MAXCUST");
            if (objConfig != null)
                ViewBag.CountCust = objConfig.IntVal;
            else
                ViewBag.CountCust = int.MaxValue;
            ViewBag.Title = Util.GetLang("OM23800");
            ViewBag.AllowModifyCust = _db.OM23800_ppAllowModifyCust(Current.UserName, Current.CpnyID).FirstOrDefault();

            var objRequire = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "OM23800RequireImport");
            if (objRequire != null)
                if (objRequire.IntVal == 1)
                    _checkRequireImport = true;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult MCPInfo(string lang)
        {
            return PartialView();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult MCPCusts(string lang)
        {
            return PartialView();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult ImportExport(string lang)
        {
            return PartialView();
        }

        public ActionResult LoadMCP(string routeID,string pJPID, string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit,
            bool hightLight, string colorFor="",
            double? amtFrom = 0, double? amtTo = 0, string brand = "", string markFor = "")
        {
            _db.CommandTimeout = 3600;

            var planVisits = _db.OM23800_pgMCL( routeID, pJPID,Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit,
                amtFrom, amtTo, brand).ToList();

            if (hightLight && !string.IsNullOrWhiteSpace(colorFor))
            {
                if (!string.IsNullOrWhiteSpace(markFor))
                {
                    makeMarkFor(ref planVisits, colorFor, markFor);
                }
                else
                {
                    makeColorFor(ref planVisits, colorFor);
                }
            }

            return this.Store(planVisits);
        }

        private void makeMarkFor(ref List<OM23800_pgMCL_Result> planVisits, string colorFor, string markFor)
        {
            var dayOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            var lstColorFors = new List<string>();

            foreach (var plan in planVisits)
            {
                var property = plan.GetType().GetProperty(colorFor);
                if (property != null)
                {
                    var value = (string)property.GetValue(plan, null);
                    if (value == markFor)
                    {
                        if (!lstColorFors.Contains(value))
                        {
                            lstColorFors.Add(value);
                        }
                    }
                    else
                    {
                        if (!lstColorFors.Contains("#"))
                        {
                            lstColorFors.Add("#");
                        }
                    }
                }
                else if (colorFor == "DOW")
                {
                    for (int i = 0; i < dayOfWeeks.Count(); i++)
                    {
                        var dayProperty = plan.GetType().GetProperty(dayOfWeeks[i]);
                        if (dayProperty != null)
                        {
                            var dayValue = (bool)dayProperty.GetValue(plan, null);
                            if (dayValue && dayOfWeeks[i] == markFor)
                            {
                                lstColorFors.Add(dayOfWeeks[i]);
                            }
                            else if (i == dayOfWeeks.Count() - 1)
                            {
                                if (!lstColorFors.Contains(dayOfWeeks[i]))
                                {
                                    lstColorFors.Add("#");
                                }
                            }
                        }
                    }
                }
            }

            var lstColors = _db.OM23800_ppListColors(Current.UserName, Current.CpnyID, Current.LangID).Select(x => x.Code).ToList();
            var lstPropertyColor = new Dictionary<string, string>();

            if (lstColorFors.Count <= lstColors.Count)
            {
                for (int i = 0; i < lstColorFors.Count; i++)
                {
                    if (!lstPropertyColor.ContainsKey(lstColorFors[i]))
                    {
                        lstPropertyColor.Add(lstColorFors[i], lstColors[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < lstColors.Count; i++)
                {
                    if (!lstPropertyColor.ContainsKey(lstColorFors[i]))
                    {
                        lstPropertyColor.Add(lstColorFors[i], lstColors[i]);
                    }
                }
                for (int i = lstColors.Count; i < lstColorFors.Count; i++)
                {
                    if (!lstPropertyColor.ContainsKey(lstColorFors[i]))
                    {
                        lstPropertyColor.Add(lstColorFors[i], lstColors[i % lstColors.Count]);
                    }
                }
            }

            foreach (var plan in planVisits)
            {
                var property = plan.GetType().GetProperty(colorFor);
                if (property != null)
                {
                    var planValue = (string)property.GetValue(plan, null);
                    if (planValue == markFor)
                    {
                        plan.Color = lstPropertyColor[planValue];
                    }
                    else
                    {
                        plan.Color = lstPropertyColor["#"];
                    }
                }
                else if (colorFor == "DOW")
                {
                    for (int i = 0; i < dayOfWeeks.Count(); i++)
                    {
                        var dayProperty = plan.GetType().GetProperty(dayOfWeeks[i]);
                        if (dayProperty != null)
                        {
                            var dayValue = (bool)dayProperty.GetValue(plan, null);
                            if (dayValue && dayOfWeeks[i] == markFor)
                            {
                                plan.Color = lstPropertyColor[dayOfWeeks[i]];
                            }
                            else if (i == dayOfWeeks.Count() - 1)
                            {
                                if (!lstColorFors.Contains(dayOfWeeks[i]))
                                {
                                    if (lstPropertyColor.ContainsKey(dayOfWeeks[i]))
                                    {
                                        plan.Color = lstPropertyColor[dayOfWeeks[i]];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void makeColorFor(ref List<OM23800_pgMCL_Result> planVisits, string colorFor)
        {
            var dayOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            var lstColorFors = new List<string>();

            foreach (var plan in planVisits)
            {
                var property = plan.GetType().GetProperty(colorFor);
                if (property != null)
                {
                    var value = (string)property.GetValue(plan, null);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (!lstColorFors.Contains(value))
                        {
                            lstColorFors.Add(value);
                        }
                    }
                    else
                    {
                        if (!lstColorFors.Contains("#"))
                        {
                            lstColorFors.Add("#");
                        }
                    }
                }
                else if (colorFor == "DOW")
                {
                    for (int i = 0; i < dayOfWeeks.Count(); i++)
                    {
                        var dayProperty = plan.GetType().GetProperty(dayOfWeeks[i]);
                        if (dayProperty != null)
                        {
                            var dayValue = (bool)dayProperty.GetValue(plan, null);
                            if (dayValue)
                            {
                                lstColorFors.Add(dayOfWeeks[i]);
                            }
                            else if (i == dayOfWeeks.Count() - 1)
                            {
                                if (!lstColorFors.Contains(dayOfWeeks[i]))
                                {
                                    lstColorFors.Add("#");
                                }
                            }
                        }
                    }
                }
            }

            var lstColors = _db.OM23800_ppListColors(Current.UserName, Current.CpnyID, Current.LangID).Select(x => x.Code).ToList();
            var lstSlsperColor = new Dictionary<string, string>();

            if (lstColorFors.Count <= lstColors.Count)
            {
                for (int i = 0; i < lstColorFors.Count; i++)
                {
                    if (!lstSlsperColor.ContainsKey(lstColorFors[i]))
                    {
                        lstSlsperColor.Add(lstColorFors[i], lstColors[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < lstColors.Count; i++)
                {
                    if (!lstSlsperColor.ContainsKey(lstColorFors[i]))
                    {
                        lstSlsperColor.Add(lstColorFors[i], lstColors[i]);
                    }
                }
                for (int i = lstColors.Count; i < lstColorFors.Count; i++)
                {
                    if (!lstSlsperColor.ContainsKey(lstColorFors[i]))
                    {
                        lstSlsperColor.Add(lstColorFors[i], lstColors[i % lstColors.Count]);
                    }
                }
            }

            foreach (var plan in planVisits)
            {
                var property = plan.GetType().GetProperty(colorFor);
                if (property != null)
                {
                    var planValue = (string)property.GetValue(plan, null);
                    if (!string.IsNullOrWhiteSpace(planValue))
                    {
                        plan.Color = lstSlsperColor[planValue];
                    }
                    else
                    {
                        plan.Color = lstSlsperColor["#"];
                    }
                }
                else if (colorFor == "DOW")
                {
                    for (int i = 0; i < dayOfWeeks.Count(); i++)
                    {
                        var dayProperty = plan.GetType().GetProperty(dayOfWeeks[i]);
                        if (dayProperty != null)
                        {
                            var dayValue = (bool)dayProperty.GetValue(plan, null);
                            if (dayValue)
                            {
                                plan.Color = lstSlsperColor[dayOfWeeks[i]];
                            }
                            else if (i == dayOfWeeks.Count() - 1)
                            {
                                if (!lstColorFors.Contains(dayOfWeeks[i]))
                                {
                                    if (lstSlsperColor.ContainsKey(dayOfWeeks[i]))
                                    {
                                        plan.Color = lstSlsperColor[dayOfWeeks[i]];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public ActionResult LoadSalesRouteMaster(string branchID, string custID, string slsPerID, string pJPID)
        {
            var slsRouteMster = _db.OM_SalesRouteMaster.FirstOrDefault(
                                    x => x.BranchID == branchID
                                    && x.CustID == custID
                                    && x.SlsPerID == slsPerID
                                    && x.SalesRouteID == branchID
                                    && x.PJPID == pJPID);
            return this.Store(slsRouteMster);
        }

        public ActionResult LoadOverLays(string BranchID,string PJPID)
        {
            return this.Store(_db.OM23800_pdOverLays(BranchID,PJPID).ToList());
        }

        //public ActionResult SaveMcp1(FormCollection data,
        //    bool custActive, string custID, string slsperID, string branchID, string pJPID, string routeID)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(custID)
        //            && !string.IsNullOrWhiteSpace(slsperID)
        //            && !string.IsNullOrWhiteSpace(branchID))
        //        {

        //            var cust = _db.AR_Customer.FirstOrDefault(c => c.CustId == custID && c.BranchID == branchID);
        //            if (cust != null)
        //            {
        //                var custActived = cust.Status.ToUpper() == "A" ? true : false;
        //                if (custActived != custActive)
        //                {
        //                    cust.Status = custActive ? "A" : "I";
        //                    cust.LUpd_Datetime = DateTime.Now;
        //                    cust.LUpd_Prog = _screenName;
        //                    cust.LUpd_User = Current.UserName;

        //                    _db.SaveChanges();
        //                }

        //                if (custActive)
        //                {
        //                    #region MCP
        //                    var dataHandler = new StoreDataHandler(data["lstMcpInfo"]);
        //                    var lstMcpInfo = dataHandler.BatchObjectData<OM_SalesRouteMaster>();

        //                    foreach (var deleted in lstMcpInfo.Deleted)
        //                    {
        //                        var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == pJPID
        //                            && x.SalesRouteID == routeID
        //                            && x.BranchID == branchID
        //                            && x.SlsPerID == slsperID
        //                            && x.CustID == custID);

        //                        if (obj != null)
        //                        {
        //                            // xoa cu
        //                            _db.OM_SalesRouteMaster.DeleteObject(obj);
        //                            _db.SaveChanges();

        //                            return Json(new
        //                            {
        //                                success = true,
        //                                CustID = custID,
        //                                SlsPerID = slsperID,
        //                                BranchID = branchID,
        //                                RouteID = routeID,
        //                                Color = "CCFF33",
        //                                SlsFreq = "",
        //                                WeekofVisit = "",
        //                                VisitSort = "false",
        //                                Sun = 0,
        //                                Mon = 0,
        //                                Tue = 0,
        //                                Wed = 0,
        //                                Thu = 0,
        //                                Fri = 0,
        //                                Sat = 0,
        //                                Status = cust.Status
        //                            });
        //                        }
        //                        else
        //                        {
        //                            throw new MessageException(MessageType.Message, "8");
        //                        }
        //                    }

        //                    lstMcpInfo.Updated.AddRange(lstMcpInfo.Created);
        //                    foreach (var updated in lstMcpInfo.Updated)
        //                    {
        //                        var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == pJPID
        //                            && x.SalesRouteID == routeID
        //                            && x.BranchID == branchID
        //                            && x.SlsPerID == slsperID
        //                            && x.CustID == custID);

        //                        if (obj != null)
        //                        {
        //                            //if (obj.tstamp.ToHex() == updated.tstamp.ToHex())
        //                            //{
        //                            // xoa cu, insert moi
        //                            //var newObj = new OM_SalesRouteMaster()
        //                            //{
        //                            //    PJPID = obj.PJPID,
        //                            //    SalesRouteID = obj.SalesRouteID,
        //                            //    CustID = obj.CustID,
        //                            //    SlsPerID = obj.SlsPerID,
        //                            //    BranchID = obj.BranchID
        //                            //};

        //                            //_db.OM_SalesRouteMaster.DeleteObject(obj);
        //                            //_db.SaveChanges();

        //                            updateSaleRoutesMaster(ref obj, updated);
        //                            if (isValidSelOMSalesRouteMaster(obj))
        //                                _db.SaveChanges();
        //                            else
        //                            {
        //                                throw new MessageException(MessageType.Message, "20131224", "", new string[] { obj.CustID });
        //                            }

        //                            return Json(new
        //                            {
        //                                success = true,
        //                                CustID = custID,
        //                                SlsPerID = slsperID,
        //                                BranchID = branchID,
        //                                RouteID = routeID,
        //                                Color = "FF0000",
        //                                SlsFreq = obj.SlsFreq,
        //                                WeekofVisit = obj.WeekofVisit,
        //                                VisitSort = obj.VisitSort,
        //                                Sun = obj.Sun ? 1 : 0,
        //                                Mon = obj.Mon ? 1 : 0,
        //                                Tue = obj.Tue ? 1 : 0,
        //                                Wed = obj.Wed ? 1 : 0,
        //                                Thu = obj.Thu ? 1 : 0,
        //                                Fri = obj.Fri ? 1 : 0,
        //                                Sat = obj.Sat ? 1 : 0,
        //                                Status = cust.Status
        //                            });
        //                            //}
        //                            //else
        //                            //{
        //                            //    throw new MessageException(MessageType.Message, "19");
        //                            //}
        //                        }
        //                        else
        //                        {
        //                            // insert moi
        //                            var newObj = new OM_SalesRouteMaster()
        //                            {
        //                                PJPID = pJPID,
        //                                SalesRouteID = routeID,
        //                                CustID = custID,
        //                                SlsPerID = slsperID,
        //                                BranchID = branchID
        //                            };

        //                            updateSaleRoutesMaster(ref newObj, updated);
        //                            if (isValidSelOMSalesRouteMaster(newObj))
        //                                _db.SaveChanges();
        //                            else
        //                            {
        //                                throw new MessageException(MessageType.Message, "22701", "", new string[] { newObj.CustID });
        //                            }
        //                            _db.OM_SalesRouteMaster.AddObject(newObj);
        //                            _db.SaveChanges();

        //                            return Json(new
        //                            {
        //                                success = true,
        //                                CustID = custID,
        //                                SlsPerID = slsperID,
        //                                BranchID = branchID,
        //                                RouteID = routeID,
        //                                Color = "FF0000",
        //                                SlsFreq = newObj.SlsFreq,
        //                                WeekofVisit = newObj.WeekofVisit,
        //                                VisitSort = newObj.VisitSort,
        //                                Sun = newObj.Sun ? 1 : 0,
        //                                Mon = newObj.Mon ? 1 : 0,
        //                                Tue = newObj.Tue ? 1 : 0,
        //                                Wed = newObj.Wed ? 1 : 0,
        //                                Thu = newObj.Thu ? 1 : 0,
        //                                Fri = newObj.Fri ? 1 : 0,
        //                                Sat = newObj.Sat ? 1 : 0,
        //                                Status = cust.Status
        //                            });
        //                        }
        //                    }

        //                    //foreach (var created in lstMcpInfo.Created)
        //                    //{
        //                    //    var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == pJPID
        //                    //        && x.SalesRouteID == routeID
        //                    //        && x.BranchID == branchID
        //                    //        && x.SlsPerID == slsperID
        //                    //        && x.CustID == custID);

        //                    //    if (obj == null)
        //                    //    {
        //                    //        // insert moi
        //                    //        var newObj = new OM_SalesRouteMaster()
        //                    //        {
        //                    //            PJPID = pJPID,
        //                    //            SalesRouteID = routeID,
        //                    //            CustID = custID,
        //                    //            SlsPerID = slsperID,
        //                    //            BranchID = branchID
        //                    //        };

        //                    //        updateSaleRoutesMaster(ref newObj, created);
        //                    //        if (isValidSelOMSalesRouteMaster(newObj))
        //                    //            _db.SaveChanges();
        //                    //        else
        //                    //        {
        //                    //            throw new MessageException(MessageType.Message, "22701", "", new string[] { newObj.CustID });
        //                    //        }
        //                    //        _db.OM_SalesRouteMaster.AddObject(newObj);
        //                    //        _db.SaveChanges();

        //                    //        return Json(new
        //                    //        {
        //                    //            success = true,
        //                    //            CustID = custID,
        //                    //            SlsPerID = slsperID,
        //                    //            BranchID = branchID,
        //                    //            RouteID = routeID,
        //                    //            Color = "FF0000",
        //                    //            SlsFreq = newObj.SlsFreq,
        //                    //            WeekofVisit = newObj.WeekofVisit,
        //                    //            VisitSort = newObj.VisitSort,
        //                    //            Sun = newObj.Sun ? 1 : 0,
        //                    //            Mon = newObj.Mon ? 1 : 0,
        //                    //            Tue = newObj.Tue ? 1 : 0,
        //                    //            Wed = newObj.Wed ? 1 : 0,
        //                    //            Thu = newObj.Thu ? 1 : 0,
        //                    //            Fri = newObj.Fri ? 1 : 0,
        //                    //            Sat = newObj.Sat ? 1 : 0,
        //                    //            Status = cust.Status
        //                    //        });
        //                    //    }
        //                    //    else
        //                    //    {
        //                    //        //return Json(new { success = false, msgCode = 2000, msgParam = Util.GetLang("MCP") });
        //                    //        throw new MessageException(MessageType.Message, "2000", "", new string[] { Util.GetLang("MCP"),pJPID });
        //                    //    }
        //                    //}
        //                    #endregion
        //                }

        //                return Json(new
        //                {
        //                    success = true,
        //                    CustID = custID,
        //                    SlsPerID = slsperID,
        //                    BranchID = branchID,
        //                    RouteID = routeID,
        //                    Status = cust.Status,
        //                    Color = (cust.Status == "I" ? "000000" : "undefined")
        //                });
        //            }
        //            else
        //            {
        //                throw new MessageException(MessageType.Message, "8", "", new string[] { Util.GetLang("MCP") });
        //            }
        //        }
        //        else
        //        {
        //            throw new MessageException(MessageType.Message, "22701");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException)
        //        {
        //            return (ex as MessageException).ToMessage();
        //        }
        //        else
        //        {
        //            return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
        //        }
        //    }
        //}

        public ActionResult SaveMcp(FormCollection data,
            bool custActive, string custID, string slsperID, string branchID, string pJPID, string routeID)
        {
            try
            {
                var salesFreq = data["salesFreq"];
                var weekOfVisit = data["weekOfVisit"];
                var sun = data["sun"].ToLower() == "true" ? true : false;
                var mon = data["mon"].ToLower() == "true" ? true : false;
                var tue = data["tue"].ToLower() == "true" ? true : false;
                var wed = data["wed"].ToLower() == "true" ? true : false;
                var thu = data["thu"].ToLower() == "true" ? true : false;
                var fri = data["fri"].ToLower() == "true" ? true : false;
                var sat = data["sat"].ToLower() == "true" ? true : false;
                DateTime startDate = DateTime.Parse(data["startDate"]);
                DateTime endDate = DateTime.Parse(data["endDate"]);
                var visitSort = data["visitSort"].PassNull() == ""? 1 : int.Parse(data["visitSort"].PassNull());

                if (!string.IsNullOrWhiteSpace(custID)
                    && !string.IsNullOrWhiteSpace(slsperID)
                    && !string.IsNullOrWhiteSpace(branchID))
                {

                    var cust = _db.AR_Customer.FirstOrDefault(c => c.CustId == custID && c.BranchID == branchID);
                    if (cust != null)
                    {
                        var custActived = cust.Status.ToUpper() == "A" ? true : false;
                        if (custActived != custActive)
                        {
                            cust.Status = custActive ? "A" : "I";
                            cust.LUpd_Datetime = DateTime.Now;
                            cust.LUpd_Prog = _screenName;
                            cust.LUpd_User = Current.UserName;

                            _db.SaveChanges();
                        }

                        if (custActive)
                        {
                            string id = Guid.NewGuid().ToString();
                            OM_SalesRouteMasterImport objImport = new OM_SalesRouteMasterImport();

                            if (_db.OM_SalesRouteMasterImport.Where(p => p.ID == id
                                                                            && p.BranchID == branchID
                                                                            && p.PJPID == pJPID
                                                                            && p.SalesRouteID == routeID
                                                                            && p.CustID == custID
                                                                            && p.SlsPerID == slsperID).ToList().Count == 0)
                            {
                                objImport.ID = id;
                                objImport.BranchID = branchID;
                                objImport.PJPID = pJPID;
                                objImport.SalesRouteID = routeID;
                                objImport.CustID = custID;
                                objImport.SlsPerID = slsperID;
                                objImport.StartDate = startDate;
                                objImport.EndDate = endDate; ;
                                objImport.SlsFreq = salesFreq;
                                objImport.SlsFreqType = "R";
                                objImport.WeekofVisit = weekOfVisit;
                                objImport.Mon = mon;
                                objImport.Tue = tue;
                                objImport.Wed = wed;
                                objImport.Thu = thu;
                                objImport.Fri = fri;
                                objImport.Sat = sat;
                                objImport.Sun = sun;

                                objImport.VisitSort = visitSort;

                                objImport.LUpd_DateTime = objImport.LUpd_DateTime = DateTime.Now;
                                objImport.LUpd_Prog = objImport.LUpd_Prog = _screenName;
                                objImport.LUpd_User = objImport.LUpd_User = Current.UserName;
                                objImport.Crtd_DateTime = objImport.Crtd_DateTime = DateTime.Now;
                                objImport.Crtd_Prog = objImport.Crtd_Prog = _screenName;
                                objImport.Crtd_User = objImport.Crtd_User = Current.UserName;
                                if (isValidSelOMSalesRouteMaster(objImport, false))
                                {
                                    _db.OM_SalesRouteMasterImport.AddObject(objImport);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "22701", "", new string[] { objImport.CustID });
                                    //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu không hợp lệ" + "\r";
                                }
                            }

                            _db.SaveChanges();
                            DataAccess dal = Util.Dal();
                            try
                            {

                                PJPProcess.PJP pjp = new PJPProcess.PJP(Current.UserName, "OM23800", dal);
                                dal.BeginTrans(IsolationLevel.ReadCommitted);
                                if (!pjp.OM23800CreateMCP(id))
                                {
                                    dal.RollbackTrans();
                                }
                                else
                                {
                                    dal.CommitTrans();
                                }
                            }
                            catch (Exception ex)
                            {
                                dal.RollbackTrans();
                                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });

                            }
                        }
                        return Json(new
                        {
                            success = true,
                            msgCode = 201405071,
                            CustID = custID,
                            SlsPerID = slsperID,
                            BranchID = branchID,
                            RouteID = routeID,
                            Status = cust.Status,
                            Color = (cust.Status == "I" ? "000000" : "undefined")
                        });
                        
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "8", "", new string[] { Util.GetLang("MCP") });
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "22701");
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

        public ActionResult SaveMcpCusts(FormCollection data)
        {
            try
            {
                var lstMcpCustsHandler = new StoreDataHandler(data["lstMcpCusts"]);
                var lstMcpCusts = lstMcpCustsHandler.ObjectData<OM23800_pgMCL_Result>();
                bool flagCust = false;
                var overLays_LatLng = data["overLays"].PassNull();
                int ID = data["iID"].PassNull()==""?0:int.Parse(data["iID"]);
                var BranchID_Main = data["cboDistributorMCL"].PassNull();
                var PJPID_Main = data["cboPJPIDMCL"].PassNull();   
                var routeID = data["routeID"];
                var salesFreq = data["salesFreq"];
                var weekOfVisit = data["weekOfVisit"];
                var sun = data["sun"].ToLower() == "true" ? true : false;
                var mon = data["mon"].ToLower() == "true" ? true : false;
                var tue = data["tue"].ToLower() == "true" ? true : false;
                var wed = data["wed"].ToLower() == "true" ? true : false;
                var thu = data["thu"].ToLower() == "true" ? true : false;
                var fri = data["fri"].ToLower() == "true" ? true : false;
                var sat = data["sat"].ToLower() == "true" ? true : false;
                DateTime startDate = DateTime.Parse(data["startDate"]);
                DateTime endDate = DateTime.Parse(data["endDate"]);
                
                string id = Guid.NewGuid().ToString();
                string branchID = string.Empty;
                string custID = string.Empty;
                string slsperID = string.Empty;
                string pJPID = string.Empty;
                for (int i = 0; i < lstMcpCusts.Count; i ++ )
                {
                    branchID = lstMcpCusts[i].BranchID;
                    custID = lstMcpCusts[i].CustId;
                    slsperID = lstMcpCusts[i].SlsperId;
                    pJPID = lstMcpCusts[i].PJPID;

                    OM_SalesRouteMasterImport objImport = new OM_SalesRouteMasterImport();

                    if (_db.OM_SalesRouteMasterImport.Where(p => p.ID == id
                                                                    && p.BranchID == branchID
                                                                        && p.PJPID == pJPID
                                                                        && p.SalesRouteID == routeID
                                                                        && p.CustID == custID
                                                                        && p.SlsPerID == slsperID).ToList().Count == 0)
                    {
                        objImport.ID = id;
                        objImport.BranchID = branchID;
                        objImport.PJPID = pJPID;
                        objImport.SalesRouteID = routeID;
                        objImport.CustID = custID;
                        objImport.SlsPerID = slsperID;
                        objImport.StartDate = startDate;
                        objImport.EndDate = endDate; ;
                        objImport.SlsFreq = salesFreq;//  dataArray.GetValue(i, 9).ToString().Trim().ToUpper();
                        objImport.SlsFreqType = "R";
                        objImport.WeekofVisit = weekOfVisit;// dataArray.GetValue(i, 10).ToString().Trim().ToUpper();
                        objImport.Mon = mon;// dataArray.GetValue(i, 11) == null ? false : dataArray.GetValue(i, 11).ToString().Trim().ToUpper() == "X" ? true : false;
                        objImport.Tue = tue;// dataArray.GetValue(i, 12) == null ? false : dataArray.GetValue(i, 12).ToString().Trim().ToUpper() == "X" ? true : false;
                        objImport.Wed = wed;// dataArray.GetValue(i, 13) == null ? false : dataArray.GetValue(i, 13).ToString().Trim().ToUpper() == "X" ? true : false;
                        objImport.Thu = thu;// dataArray.GetValue(i, 14) == null ? false : dataArray.GetValue(i, 14).ToString().Trim().ToUpper() == "X" ? true : false;
                        objImport.Fri = fri;// dataArray.GetValue(i, 15) == null ? false : dataArray.GetValue(i, 15).ToString().Trim().ToUpper() == "X" ? true : false;
                        objImport.Sat = sat;// dataArray.GetValue(i, 16) == null ? false : dataArray.GetValue(i, 16).ToString().Trim().ToUpper() == "X" ? true : false;
                        objImport.Sun = sun;// dataArray.GetValue(i, 17) == null ? false : dataArray.GetValue(i, 17).ToString().Trim().ToUpper() == "X" ? true : false;

                        objImport.VisitSort = i + 1;// dataArray.GetValue(i, 20) == null ? 0 : dataArray.GetValue(i, 20).ToString().Trim().ToUpper() == "" ? 0 : int.Parse(dataArray.GetValue(i, 20).ToString().Trim().ToUpper());
   
                        objImport.LUpd_DateTime = objImport.LUpd_DateTime = DateTime.Now;
                        objImport.LUpd_Prog = objImport.LUpd_Prog = _screenName;
                        objImport.LUpd_User = objImport.LUpd_User = Current.UserName;
                        objImport.Crtd_DateTime = objImport.Crtd_DateTime = DateTime.Now;
                        objImport.Crtd_Prog = objImport.Crtd_Prog = _screenName;
                        objImport.Crtd_User = objImport.Crtd_User = Current.UserName;
                        if (isValidSelOMSalesRouteMaster(objImport, false))
                        {
                            flagCust = true;
                            _db.OM_SalesRouteMasterImport.AddObject(objImport);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "22701", "", new string[] { objImport.CustID });
                            //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu không hợp lệ" + "\r";
                        }

                    }
                }
                var objOverlays = new OM_OverlaysMCP();
                if (flagCust == true)
                {
                    objOverlays = _db.OM_OverlaysMCP.FirstOrDefault(p => p.ID == ID);
                    if (objOverlays == null)
                    {
                        objOverlays = new OM_OverlaysMCP();
                        objOverlays.ResetET();
                        objOverlays.BranchID = BranchID_Main;
                        objOverlays.PJPID = PJPID_Main;
                        objOverlays.LatLng = overLays_LatLng;
                        objOverlays.LUpd_DateTime = DateTime.Now;
                        objOverlays.LUpd_Prog = _screenName;
                        objOverlays.LUpd_User = Current.UserName;
                        objOverlays.Crtd_DateTime = DateTime.Now;
                        objOverlays.Crtd_Prog = _screenName;
                        objOverlays.Crtd_User = Current.UserName;

                        _db.OM_OverlaysMCP.AddObject(objOverlays);
                    }
                }
                if(objOverlays == null)
                    objOverlays.ID = 0;
                _db.SaveChanges();
                DataAccess dal = Util.Dal();
                try
                {
                  
                    PJPProcess.PJP pjp = new PJPProcess.PJP(Current.UserName, "OM23800", dal);
                    dal.BeginTrans(IsolationLevel.ReadCommitted);
                    if (!pjp.OM23800CreateMCP(id))
                    {
                        dal.RollbackTrans();
                    }
                    else
                    {
                        dal.CommitTrans();
                    }
                }
                catch (Exception ex)
                {
                    dal.RollbackTrans();
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });

                }

                return Json(new
                {
                    success = true,
                    msgCode = 201405071,
                    ID = objOverlays.ID
                });
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

        public ActionResult DeleteOverLays(FormCollection data)
        {
            try
            {
                int ID = data["iID"].PassNull() == "" ? 0 : int.Parse(data["iID"]);
                
                var objDelete = _db.OM_OverlaysMCP.FirstOrDefault(p => p.ID == ID);
                if (objDelete != null)
                {
                    _db.OM_OverlaysMCP.DeleteObject(objDelete);
                }

                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    msgCode = 201405071
                });
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

        [DirectMethod]
        public ActionResult ExportSelectedCust_old(string custIDs)
        {
            try
            {
                string[] paraStringsD = JSON.Deserialize<string[]>(custIDs);

                if (custIDs.Length > 0)
                {
                    var headerRowIdx = 3;

                    Stream stream = new MemoryStream();
                    Workbook workbook = new Workbook();
                    Worksheet SheetMCP = workbook.Worksheets[0];
                    SheetMCP.Name = Util.GetLang("MCL");
                    DataAccess dal = Util.Dal();
                    Style style = workbook.GetStyleInPool(0);
                    StyleFlag flag = new StyleFlag();
                    var beforeColTexts = new string[] { "N0", "BranchID", "CpnyName", "SlsperID", "SlsName", "CustID", "CustName", "Address", "WeekofVisit", "DayOfWeek" };

                    #region header info
                    // Title header
                    SetCellValue(SheetMCP.Cells["A2"],
                        string.Format("{0} ({1})", Util.GetLang("OM23800EHeader"), Util.GetLang("DrawingArea")),
                        TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                    SheetMCP.Cells.Merge(1, 0, 1, beforeColTexts.Length);

                    // Header text columns

                    for (int i = 0; i < beforeColTexts.Length; i++)
                    {
                        var cell = SheetMCP.Cells[headerRowIdx, i];
                        SetCellValue(cell, Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                        //SheetMCP.Cells.Merge(headerRowIdx, colIdx, 1, 1);

                        //Apply the border styles to the cell
                        setCellStyle(cell);
                    }
                    var allColumns = new List<string>();
                    allColumns.AddRange(beforeColTexts);

                    #endregion

                    #region export data
                    ParamCollection pc = new ParamCollection();
                    pc.Add(new ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(string.Join(",", paraStringsD)), ParameterDirection.Input, int.MaxValue));
                    pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                    DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportSelectedCust", CommandType.StoredProcedure, ref pc);
                    //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

                    for (int i = 0; i < dtDataExport.Rows.Count; i++)
                    {
                        for (int j = 0; j < allColumns.Count; j++)
                        {
                            var cell = SheetMCP.Cells[headerRowIdx + 1 + i, j];
                            if (allColumns[j] == "N0")
                            {
                                cell.PutValue(i + 1);
                            }
                            else if (dtDataExport.Columns.Contains(allColumns[j]))
                            {
                                cell.PutValue(dtDataExport.Rows[i][allColumns[j]]);
                            }

                            //Apply the border styles to the cell
                            setCellStyle(cell);
                        }
                    }
                    #endregion
                    SheetMCP.AutoFitColumns();

                    //SheetPOSuggest.Protect(ProtectionType.Objects);
                    workbook.Save(stream, SaveFormat.Xlsx);
                    stream.Flush();
                    stream.Position = 0;

                    //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM30400") + ".xlsx" };
                    return File(stream, "application/vnd.ms-excel", string.Format("{0}_{1}.xlsx", Util.GetLang("OM23800"), Util.GetLang("DrawingArea")));
                }
                else
                {
                    throw new MessageException(MessageType.Message, "14091901");
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

        [DirectMethod]
        public ActionResult ExportSelectedCust(string custIDs, string pBranchID, string pBranchName, string pJPID, string routeID)
        {
            try
            {
                string[] paraStringsD = JSON.Deserialize<string[]>(custIDs);

                if (paraStringsD.Length > 0)
                {
                    //string branchID = data["BranchID"].PassNull();
                    string branchID = pBranchID;                  
                    string branchName = pBranchName;
                  
                    string slsperID = string.Empty;
                    var headerRowIdx = 3;

                    Stream stream = new MemoryStream();
                    Workbook workbook = new Workbook();
                    Worksheet SheetMCP = workbook.Worksheets[0];
                    SheetMCP.Name = Util.GetLang("MCP");
                    DataAccess dal = Util.Dal();
                    Style style = workbook.GetStyleInPool(0);
                    StyleFlag flag = new StyleFlag();
                    Range range;
                    Cell cell;
                    #region master data
                    ParamCollection pc = new ParamCollection();
                    pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                    DataTable dtCustomer = dal.ExecDataTable("OM23800_peCustomerMCP", CommandType.StoredProcedure, ref pc);
                    SheetMCP.Cells.ImportDataTable(dtCustomer, true, 0, 26, false);// du lieu AR_Customer


                    pc = new ParamCollection();
                    pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                    DataTable dtSales = dal.ExecDataTable("OM23800_peSalespersonMCP", CommandType.StoredProcedure, ref pc);
                    SheetMCP.Cells.ImportDataTable(dtSales, true, 0, 52, false);// du lieu Salesperson


                    pc = new ParamCollection();
                    pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                    DataTable dtRoute = dal.ExecDataTable("OM23800_peRouteMCP", CommandType.StoredProcedure, ref pc);
                    SheetMCP.Cells.ImportDataTable(dtRoute, true, 0, 78, false);// du lieu SalesRoute


                    SheetMCP.Cells["Z1"].PutValue("W159");
                    SheetMCP.Cells["Z2"].PutValue("W2610");
                    SheetMCP.Cells["Z3"].PutValue("W3711");
                    SheetMCP.Cells["Z4"].PutValue("W4812");
                    SheetMCP.Cells["Z5"].PutValue("OW");
                    SheetMCP.Cells["Z6"].PutValue("EW");
                    SheetMCP.Cells["Z7"].PutValue("NA");
                    for (int i = 0; i < 53; i++)
                    {
                        SheetMCP.Cells["Z" + (i + 8)].PutValue("W" + (i + 1));
                    }

                    #endregion

                    #region header info
                    // Title header
                    SetCellValue(SheetMCP.Cells["B1"],
                        string.Format("{0} ({1})", Util.GetLang("OM23800EHeader"), Util.GetLang("DrawingArea")),
                        TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                    SheetMCP.Cells.Merge(0, 1, 1, 6);

                    // Title info
                    SetCellValue(SheetMCP.Cells["B2"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                    SetCellValue(SheetMCP.Cells["B3"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                    SetCellValue(SheetMCP.Cells["C2"], branchID, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);
                    SetCellValue(SheetMCP.Cells["C3"], branchName, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);

                    // Header text columns
                    // before of Route column
                    var beforeColTexts = new string[] { "N0", "CustID", "CustName", "Address", "SlsperID", "SlsName", "StartDate", "EndDate", "SlsFreq", "WeekofVisit" };
                    for (int i = 0; i < beforeColTexts.Length; i++)
                    {
                        var colIdx = i;
                        SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                        SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                    }

                    //Route column
                    var daysOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                    SetCellValue(SheetMCP.Cells[headerRowIdx, beforeColTexts.Length], Util.GetLang("Route"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    SheetMCP.Cells.Merge(headerRowIdx, beforeColTexts.Length, 1, daysOfWeeks.Length);
                    for (int i = 0; i < daysOfWeeks.Length; i++)
                    {
                        var colIdx = beforeColTexts.Length + i;
                        SetCellValue(SheetMCP.Cells[headerRowIdx + 1, colIdx], Util.GetLang(daysOfWeeks[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    }

                    // after of Route column
                    var afterColTexts = new string[] { "SalesRouteID", "RouteName", "VisitSort", "CustCancel" };
                    for (int i = 0; i < afterColTexts.Length; i++)
                    {
                        var colIdx = beforeColTexts.Length + daysOfWeeks.Length + i;
                        SetCellValue(SheetMCP.Cells[headerRowIdx, colIdx], Util.GetLang(afterColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);



                        SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                    }



                    var allColumns = new List<string>();
                    allColumns.AddRange(beforeColTexts);
                    allColumns.AddRange(daysOfWeeks);
                    allColumns.AddRange(afterColTexts);

                    #endregion

                    #region formular

                    Validation validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.Date;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.GreaterOrEqual;
                    validation.Formula1 = DateTime.Now.ToShortDateString();
                    validation.InputTitle = "Chọn Ngày Bắt Đầu(MM/dd/yyyy)";
                    validation.InputMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");
                    validation.ErrorMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");

                    CellArea area;
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("StartDate");
                    area.EndColumn = allColumns.IndexOf("StartDate");
                    validation.AddArea(area);

                    string formulaDate = "=$" + Getcell(allColumns.IndexOf("StartDate")) + "$6";
                    validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.Date;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.GreaterOrEqual;
                    validation.Formula1 = formulaDate;
                    validation.InputTitle = "Chọn Ngày Kết Thúc(MM/dd/yyyy)";
                    validation.InputMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";
                    validation.ErrorMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";

                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("EndDate");
                    area.EndColumn = allColumns.IndexOf("EndDate");
                    validation.AddArea(area);

                    //custid
                    string formulaCustomer = "=$AA$2:$AA$" + (dtCustomer.Rows.Count + 2);
                    validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = formulaCustomer;
                    validation.InputTitle = "";
                    validation.InputMessage = "Chọn Mã Khách Hàng ";
                    validation.ErrorMessage = "Mã Khách Hàng này không tồn tại";

                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("CustID");
                    area.EndColumn = allColumns.IndexOf("CustID");
                    validation.AddArea(area);

                    //SALES
                    string formulaSales = "=$BA$2:$BA$" + (dtSales.Rows.Count + 2);
                    validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = formulaSales;
                    validation.InputTitle = "";
                    validation.InputMessage = "Chọn Mã Nhân Viên Bán Hàng";
                    validation.ErrorMessage = "Mã Nhân Viên này không tồn tại";

                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("SlsperID");
                    area.EndColumn = allColumns.IndexOf("SlsperID");
                    validation.AddArea(area);

                    //Route
                    string formulaRoutes = "=$CA$2:$CA$" + (dtRoute.Rows.Count + 2);
                    validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = formulaRoutes;
                    validation.InputTitle = "";
                    validation.InputMessage = "Chọn Mã Tuyến Đường";
                    validation.ErrorMessage = "Mã Tuyến Đường này không tồn tại";

                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("SalesRouteID");
                    area.EndColumn = allColumns.IndexOf("SalesRouteID");
                    validation.AddArea(area);
                    //Requency LIST
                    string formulaRequenc = "F1,F2,F4,F4A,F8,F12,F1/2,F1/3,A";
                    validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = formulaRequenc;
                    validation.InputTitle = "";
                    validation.InputMessage = "Chọn Tần Suất Thăm Viếng";
                    validation.ErrorMessage = "Mã Tần Suất này không tồn tại";

                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("SlsFreq");
                    area.EndColumn = allColumns.IndexOf("SlsFreq");
                    validation.AddArea(area);


                    //string formula = "=IF(I6=\"F1\",$Z$1:$Z$4,IF(OR(I6=\"F2\",I6=\"F4A\",I6=\"F8A\"),$Z$5:$Z$6,$Z$7:$Z$7))";// + dtOMRoute.Rows.Count + 2;        
                    string formula = "=IF(I6=\"F1\",$Z$1:$Z$4,IF(OR(I6=\"F1/2\",I6=\"F1/3\"),$Z$8:$Z$61,IF(OR(I6=\"F2\",I6=\"F4A\",I6=\"F8A\"),$Z$5:$Z$6,$Z$7:$Z$7)))";// + dtOMRoute.Rows.Count + 2;                              
                    validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = formula;
                    validation.InputTitle = "";
                    validation.InputMessage = "Chọn Tuần Bán Hàng";
                    validation.ErrorMessage = "Mã Tuần Bán Hàng Không tồn tại";

                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("WeekofVisit");
                    area.EndColumn = allColumns.IndexOf("WeekofVisit");
                    validation.AddArea(area);

                    string formulaCheck = "X";
                    validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                    validation.IgnoreBlank = true;
                    validation.Type = Aspose.Cells.ValidationType.List;
                    validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                    validation.Operator = OperatorType.Between;
                    validation.Formula1 = formulaCheck;
                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("Mon");
                    area.EndColumn = allColumns.IndexOf("Sun");
                    validation.AddArea(area);

                    area = new CellArea();
                    area.StartRow = 5;
                    area.EndRow = dtCustomer.Rows.Count + 5;
                    area.StartColumn = allColumns.IndexOf("CustCancel");
                    area.EndColumn = allColumns.IndexOf("CustCancel");
                    validation.AddArea(area);


                    string formulaCustName = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,2,0)),\"\",VLOOKUP({0},AA:AC,2,0))", "B6");
                    SheetMCP.Cells[Getcell(allColumns.IndexOf("CustName")) + "6"].SetSharedFormula(formulaCustName, (dtCustomer.Rows.Count + 6), 1);


                    string formulaCustAddr = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,3,0)),\"\",VLOOKUP({0},AA:AC,3,0))", "B6");
                    SheetMCP.Cells[Getcell(allColumns.IndexOf("Address")) + "6"].SetSharedFormula(formulaCustAddr, (dtCustomer.Rows.Count + 6), 1);


                    string formulaSalesName = string.Format("=IF(ISERROR(VLOOKUP({0},BA:BC,2,0)),\"\",VLOOKUP({0},BA:BC,2,0))", "E6");
                    SheetMCP.Cells[Getcell(allColumns.IndexOf("SlsName")) + "6"].SetSharedFormula(formulaSalesName, (dtCustomer.Rows.Count + 6), 1);


                    string formulaRoute = string.Format("=IF(ISERROR(VLOOKUP({0},CA:CC,2,0)),\"\",VLOOKUP({0},CA:CC,2,0))", "R6");
                    SheetMCP.Cells[Getcell(allColumns.IndexOf("RouteName")) + "6"].SetSharedFormula(formulaRoute, (dtCustomer.Rows.Count + 6), 1);


                    string formulaSTT = "=IFERROR( IF(B6<>\"\",A5+1 & \"\",\"\"),1)";
                    SheetMCP.Cells["A6"].SetSharedFormula(formulaSTT, (dtCustomer.Rows.Count + 6), 1);



                    #endregion
                    #region export data
                    pc = new ParamCollection();
                    pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));
                    pc.Add(new ParamStruct("@SelectedCusts", DbType.String, clsCommon.GetValueDBNull(string.Join(",",paraStringsD)), ParameterDirection.Input, int.MaxValue));

                    DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportDataCustSelected", CommandType.StoredProcedure, ref pc);
                    //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export


                    for (int i = 0; i < dtDataExport.Rows.Count; i++)
                    {
                        for (int j = 0; j < allColumns.Count; j++)
                        {
                            if (allColumns[j] == "N0" || allColumns[j] == "CustName" || allColumns[j] == "SlsName" || allColumns[j] == "Address" || allColumns[j] == "RouteName")
                            {
                                //SheetMCP.Cells[5 + i, j].PutValue(i + 1);
                            }
                            else if (dtDataExport.Columns.Contains(allColumns[j]))
                            {
                                SheetMCP.Cells[5 + i, j].PutValue(dtDataExport.Rows[i][allColumns[j]]);
                            }
                        }
                    }
                    #endregion
                    #region Fomat cell

                    style = SheetMCP.Cells[allColumns.IndexOf("StartDate")].GetStyle();
                    style.Custom = "MM/dd/yyyy";
                    style.Font.Color = Color.Black;
                    style.HorizontalAlignment = TextAlignmentType.Left;

                    range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("StartDate")) + "5", Getcell(allColumns.IndexOf("StartDate")) + dtCustomer.Rows.Count + 5);
                    range.SetStyle(style);

                    range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("EndDate")) + "5", Getcell(allColumns.IndexOf("EndDate")) + dtCustomer.Rows.Count + 5);
                    range.SetStyle(style);


                    style = SheetMCP.Cells[allColumns.IndexOf("VisitSort")].GetStyle();
                    style.Custom = "#,##0";
                    style.Font.Color = Color.Black;
                    style.HorizontalAlignment = TextAlignmentType.Right;

                    range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("VisitSort")) + "5", Getcell(allColumns.IndexOf("VisitSort")) + dtCustomer.Rows.Count + 5);
                    range.SetStyle(style);


                    style = SheetMCP.Cells["Z1"].GetStyle();
                    style.Font.Color = Color.Transparent;
                    flag.FontColor = true;
                    flag.NumberFormat = true;
                    flag.Locked = true;

                    range = SheetMCP.Cells.CreateRange("Z1", "ZZ" + (dtCustomer.Rows.Count + dtSales.Rows.Count + dtRoute.Rows.Count + 100));
                    range.ApplyStyle(style, flag);


                    #endregion
                    SheetMCP.AutoFitColumns();

                    SheetMCP.Cells.Columns[allColumns.IndexOf("CustID")].Width = 30;
                    SheetMCP.Cells.Columns[allColumns.IndexOf("CustName")].Width = 30;
                    SheetMCP.Cells.Columns[allColumns.IndexOf("SlsName")].Width = 30;
                    SheetMCP.Cells.Columns[allColumns.IndexOf("Address")].Width = 30;

                    //SheetPOSuggest.Protect(ProtectionType.Objects);
                    workbook.Save(stream, SaveFormat.Xlsx);
                    stream.Flush();
                    stream.Position = 0;

                    return new FileStreamResult(stream, "application/vnd.ms-excel")
                    {
                        FileDownloadName = string.Format("{0}_{1}.xlsx", Util.GetLang("OM23800"), Util.GetLang("DrawingArea"))
                    };
                }
                else
                {
                    throw new MessageException(MessageType.Message, "14091901");
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

        [DirectMethod]
        public ActionResult ExportMCL(string routeID,string pjpID, string channel, string channelDescr, 
            string territory, string territoryDescr,
            string province, string provinceDescr,
            string distributor, string distributorDescr, 
            string shopType, string shopTypeDescr,
            string slsperId, string slsperIdDescr,
            string daysOfWeek, string daysOfWeekDescr,
            string weekOfVisit, string weekOfVisitDescr)
        {
            try
            {
                var headerRowIdx = 11;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetMCP = workbook.Worksheets[0];
                SheetMCP.Name = Util.GetLang("MCL");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                var beforeColTexts = new string[] { "N0", "CustId", "CustName", "Addr1", "Name", "Lat", "lng" };

                #region header info
                // Title header
                SetCellValue(SheetMCP.Cells["A2"],
                    string.Format("{0}", Util.GetLang("OM23800EHeader")),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetMCP.Cells.Merge(1, 0, 1, beforeColTexts.Length);

                SetCellValue(SheetMCP.Cells["B3"],
                    string.Format("{0}", Util.GetLang("Area")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C3"].PutValue(string.Format("{0} - {1}", territory, territoryDescr));

                SetCellValue(SheetMCP.Cells["B4"],
                    string.Format("{0}", Util.GetLang("Provice")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C4"].PutValue(string.Format("{0} - {1}", province, provinceDescr));

                SetCellValue(SheetMCP.Cells["B5"],
                    string.Format("{0}", Util.GetLang("Distributor")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C5"].PutValue(string.Format("{0} - {1}", distributor, distributorDescr));

                SetCellValue(SheetMCP.Cells["B6"],
                    string.Format("{0}", Util.GetLang("Channel")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C6"].PutValue(string.Format("{0} - {1}", channel, channelDescr));

                SetCellValue(SheetMCP.Cells["B7"],
                    string.Format("{0}", Util.GetLang("ShopType")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C7"].PutValue(string.Format("{0} - {1}", shopType, shopTypeDescr));

                SetCellValue(SheetMCP.Cells["B8"],
                    string.Format("{0}", Util.GetLang("Salesman")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C8"].PutValue(string.Format("{0} - {1}", slsperId, slsperIdDescr));

                SetCellValue(SheetMCP.Cells["B9"],
                    string.Format("{0}", Util.GetLang("DayOfWeek")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C9"].PutValue(string.Format("{0} - {1}", daysOfWeek, daysOfWeekDescr));

                SetCellValue(SheetMCP.Cells["B10"],
                    string.Format("{0}", Util.GetLang("WeekOfVisit")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C10"].PutValue(string.Format("{0} - {1}", weekOfVisit, weekOfVisitDescr));

                // Header text columns
                for (int i = 0; i < beforeColTexts.Length; i++)
                {
                    var cell = SheetMCP.Cells[headerRowIdx, i];
                    SetCellValue(cell, Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    //SheetMCP.Cells.Merge(headerRowIdx, colIdx, 1, 1);

                    //Apply the border styles to the cell
                    setCellStyle(cell);
                }
                var allColumns = new List<string>();
                allColumns.AddRange(beforeColTexts);

                #endregion

                #region export data
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pjpID), ParameterDirection.Input, 30));            
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@Channel", DbType.String, clsCommon.GetValueDBNull(channel), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@Territory", DbType.String, clsCommon.GetValueDBNull(territory), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@Province", DbType.String, clsCommon.GetValueDBNull(province), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@Distributor", DbType.String, clsCommon.GetValueDBNull(distributor), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@ShopType", DbType.String, clsCommon.GetValueDBNull(shopType), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@SlsperId", DbType.String, clsCommon.GetValueDBNull(slsperId), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@DaysOfWeek", DbType.String, clsCommon.GetValueDBNull(daysOfWeek), ParameterDirection.Input, 3));
                pc.Add(new ParamStruct("@WeekEO", DbType.String, clsCommon.GetValueDBNull(weekOfVisit), ParameterDirection.Input, 2));

                DataTable dtDataExport = dal.ExecDataTable("OM23800_peMCL", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        var cell = SheetMCP.Cells[headerRowIdx + 1 + i, j];
                        if (allColumns[j] == "N0")
                        {
                            cell.PutValue(i + 1);
                        }
                        else if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            cell.PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }

                        //Apply the border styles to the cell
                        setCellStyle(cell);
                    }
                }
                #endregion
                SheetMCP.AutoFitColumns();

                //SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM30400") + ".xlsx" };
                return File(stream, "application/vnd.ms-excel", Util.GetLang("OM23800") + ".xlsx");
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

        [HttpPost]
        public ActionResult ExportMCP(FormCollection data)
        {
            try
            {

                //string branchID = data["BranchID"].PassNull();
                string branchID = data["BranchID"].PassNull();
                string pJPID = data["PJPID"].PassNull();
                string branchName = data["BranchName"].PassNull();
                string routeID = data["RouteID"].PassNull();
                string slsperID = data["SlsperID"].PassNull();
                var headerRowIdx = 3;
                int numberRow = 3000;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetMCP = workbook.Worksheets[0];
                SheetMCP.Name = Util.GetLang("MCP");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                Range range;
                Cell cell;
                #region master data
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtCustomer = dal.ExecDataTable("OM23800_peCustomerMCP", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtCustomer, true, 0, 26, false);// du lieu AR_Customer


                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtSales = dal.ExecDataTable("OM23800_peSalespersonMCP", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtSales, true, 0, 52, false);// du lieu Salesperson


                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtRoute = dal.ExecDataTable("OM23800_peRouteMCP", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtRoute, true, 0, 78, false);// du lieu SalesRoute


                SheetMCP.Cells["Z1"].PutValue("W159");
                SheetMCP.Cells["Z2"].PutValue("W2610");
                SheetMCP.Cells["Z3"].PutValue("W3711");
                SheetMCP.Cells["Z4"].PutValue("W4812");
                SheetMCP.Cells["Z5"].PutValue("OW");
                SheetMCP.Cells["Z6"].PutValue("EW");
                SheetMCP.Cells["Z7"].PutValue("NA");

                for (int i = 0; i < 53; i++)
                {
                    SheetMCP.Cells["Z"+(i+8)].PutValue("W"+(i+1));
                }

                #endregion

                #region header info
                // Title header
                SetCellValue(SheetMCP.Cells["B1"],
                    string.Format("{0} {1}", Util.GetLang("OM23800EHeaderMCP") + "(" + pJPID + ")", (string.IsNullOrWhiteSpace(branchID) ? string.Format("({0})", branchID) : string.Empty)),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetMCP.Cells.Merge(0, 1, 1, 6);

                // Title info
                SetCellValue(SheetMCP.Cells["B2"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["B3"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["C2"], branchID, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);
                SetCellValue(SheetMCP.Cells["C3"], branchName, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);

                SetCellValue(SheetMCP.Cells["D2"], Util.GetLang("PJPID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["E2"], pJPID, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);
                // Header text columns
                // before of Route column
                var beforeColTexts = new string[] { "N0", "CustID", "CustName", "Address", "SlsperID", "SlsName", "StartDate", "EndDate", "SlsFreq", "WeekofVisit" };
                for (int i = 0; i < beforeColTexts.Length; i++)
                {
                    var colIdx = i;
                    SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }

                //Route column
                var daysOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                SetCellValue(SheetMCP.Cells[headerRowIdx, beforeColTexts.Length], Util.GetLang("Route"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                SheetMCP.Cells.Merge(headerRowIdx, beforeColTexts.Length, 1, daysOfWeeks.Length);
                for (int i = 0; i < daysOfWeeks.Length; i++)
                {
                    var colIdx = beforeColTexts.Length + i;
                    SetCellValue(SheetMCP.Cells[headerRowIdx + 1, colIdx], Util.GetLang(daysOfWeeks[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                }

                // after of Route column
                var afterColTexts = new string[] { "SalesRouteID", "RouteName", "VisitSort", "CustCancel" };
                for (int i = 0; i < afterColTexts.Length; i++)
                {
                    var colIdx = beforeColTexts.Length + daysOfWeeks.Length + i;
                    SetCellValue(SheetMCP.Cells[headerRowIdx, colIdx], Util.GetLang(afterColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);



                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }



                var allColumns = new List<string>();
                allColumns.AddRange(beforeColTexts);
                allColumns.AddRange(daysOfWeeks);
                allColumns.AddRange(afterColTexts);

                #endregion

                #region formular

                Validation validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.Date;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.GreaterOrEqual;
                validation.Formula1 = DateTime.Now.ToShortDateString();
                validation.InputTitle = "Chọn Ngày Bắt Đầu(MM/dd/yyyy)";
                validation.InputMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");
                validation.ErrorMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");

                CellArea area;
                area.StartRow = 5;
                area.EndRow = numberRow;// dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("StartDate");
                area.EndColumn = allColumns.IndexOf("StartDate");
                validation.AddArea(area);

                string formulaDate = "=$" + Getcell(allColumns.IndexOf("StartDate")) + "$6";
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.Date;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.GreaterOrEqual;
                validation.Formula1 = formulaDate;
                validation.InputTitle = "Chọn Ngày Kết Thúc(MM/dd/yyyy)";
                validation.InputMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";
                validation.ErrorMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;//  dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("EndDate");
                area.EndColumn = allColumns.IndexOf("EndDate");
                validation.AddArea(area);

                //custid
                string formulaCustomer = "=$AA$2:$AA$" + (dtCustomer.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaCustomer;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Mã Khách Hàng ";
                validation.ErrorMessage = "Mã Khách Hàng này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;//  dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("CustID");
                area.EndColumn = allColumns.IndexOf("CustID");
                validation.AddArea(area);

                //SALES
                string formulaSales = "=$BA$2:$BA$" + (dtSales.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaSales;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Mã Nhân Viên Bán Hàng";
                validation.ErrorMessage = "Mã Nhân Viên này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;//  dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("SlsperID");
                area.EndColumn = allColumns.IndexOf("SlsperID");
                validation.AddArea(area);

                //Route
                string formulaRoutes = "=$CA$2:$CA$" + (dtRoute.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaRoutes;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Mã Tuyến Đường";
                validation.ErrorMessage = "Mã Tuyến Đường này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;//  dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("SalesRouteID");
                area.EndColumn = allColumns.IndexOf("SalesRouteID");
                validation.AddArea(area);
                //Requency LIST
                string formulaRequenc = "F1,F2,F4,F4A,F8,F12,F1/2,F1/3,A";
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaRequenc;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Tần Suất Thăm Viếng";
                validation.ErrorMessage = "Mã Tần Suất này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;//  dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("SlsFreq");
                area.EndColumn = allColumns.IndexOf("SlsFreq");
                validation.AddArea(area);


                string formula = "=IF(I6=\"F1\",$Z$1:$Z$4,IF(OR(I6=\"F1/2\",I6=\"F1/3\"),$Z$8:$Z$61,IF(OR(I6=\"F2\",I6=\"F4A\",I6=\"F8A\"),$Z$5:$Z$6,$Z$7:$Z$7)))";// + dtOMRoute.Rows.Count + 2;               
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formula;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Tuần Bán Hàng";
                validation.ErrorMessage = "Mã Tuần Bán Hàng Không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;// dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("WeekofVisit");
                area.EndColumn = allColumns.IndexOf("WeekofVisit");
                validation.AddArea(area);

                string formulaCheck = "X";
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaCheck;
                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;// dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("Mon");
                area.EndColumn = allColumns.IndexOf("Sun");
                validation.AddArea(area);

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = numberRow;//  dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("CustCancel");
                area.EndColumn = allColumns.IndexOf("CustCancel");
                validation.AddArea(area);


                string formulaCustName = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,2,0)),\"\",VLOOKUP({0},AA:AC,2,0))", "B6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("CustName")) + "6"].SetSharedFormula(formulaCustName, numberRow, 1); // (dtCustomer.Rows.Count + 6)


                string formulaCustAddr = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,3,0)),\"\",VLOOKUP({0},AA:AC,3,0))", "B6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("Address")) + "6"].SetSharedFormula(formulaCustAddr, numberRow, 1);// (dtCustomer.Rows.Count + 6)


                string formulaSalesName = string.Format("=IF(ISERROR(VLOOKUP({0},BA:BC,2,0)),\"\",VLOOKUP({0},BA:BC,2,0))", "E6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("SlsName")) + "6"].SetSharedFormula(formulaSalesName, numberRow, 1);// (dtCustomer.Rows.Count + 6)


                string formulaRoute = string.Format("=IF(ISERROR(VLOOKUP({0},CA:CC,2,0)),\"\",VLOOKUP({0},CA:CC,2,0))", "R6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("RouteName")) + "6"].SetSharedFormula(formulaRoute, numberRow, 1);// (dtCustomer.Rows.Count + 6)


                string formulaSTT = "=IFERROR( IF(B6<>\"\",A5+1 & \"\",\"\"),1)";
                SheetMCP.Cells["A6"].SetSharedFormula(formulaSTT, numberRow, 1);// (dtCustomer.Rows.Count + 6)



                #endregion

                #region export data
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportDataMCP", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export


                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        if (allColumns[j] == "N0" || allColumns[j] == "CustName" || allColumns[j] == "SlsName" || allColumns[j] == "Address" || allColumns[j] == "RouteName")
                        {
                            //SheetMCP.Cells[5 + i, j].PutValue(i + 1);
                        }
                        else if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            SheetMCP.Cells[5 + i, j].PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }
                    }
                }
                #endregion

                #region Fomat cell

                style = SheetMCP.Cells[allColumns.IndexOf("StartDate")].GetStyle();
                style.Custom = "MM/dd/yyyy";
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Left;

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("StartDate")) + "5", Getcell(allColumns.IndexOf("StartDate")) + numberRow); //dtCustomer.Rows.Count + 5
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("EndDate")) + "5", Getcell(allColumns.IndexOf("EndDate")) + numberRow);//dtCustomer.Rows.Count + 5
                range.SetStyle(style);


                style = SheetMCP.Cells[allColumns.IndexOf("VisitSort")].GetStyle();
                style.Custom = "#,##0";
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Right;

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("VisitSort")) + "5", Getcell(allColumns.IndexOf("VisitSort")) + numberRow);//dtCustomer.Rows.Count + 5
                range.SetStyle(style);


                style = SheetMCP.Cells["Z1"].GetStyle();
                style.Font.Color = Color.Transparent;
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;

                range = SheetMCP.Cells.CreateRange("Z1", "ZZ" + (dtCustomer.Rows.Count + dtSales.Rows.Count + dtRoute.Rows.Count + 100));
                range.ApplyStyle(style, flag);

         

                style = SheetMCP.Cells["B6"].GetStyle();
                style.Number = 49;
                range = SheetMCP.Cells.CreateRange("B6", "B"+ numberRow);
                range.SetStyle(style);



                #endregion
                SheetMCP.AutoFitColumns();

                SheetMCP.Cells.Columns[allColumns.IndexOf("CustID")].Width = 30;
                SheetMCP.Cells.Columns[allColumns.IndexOf("CustName")].Width = 30;
                SheetMCP.Cells.Columns[allColumns.IndexOf("SlsName")].Width = 30;
                SheetMCP.Cells.Columns[allColumns.IndexOf("Address")].Width = 30;

                //SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel") {
                    FileDownloadName = string.Format("{0}_{1}.xlsx", Util.GetLang("OM23800"),Util.GetLang("MCP"))
                };

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

        [HttpPost]
        public ActionResult ImportMCP(FormCollection data)
        {
            try
            {
                string BranchID = data["BranchID"].PassNull();
                string slsperID=data["cboSlsPerID_ImExMcp"].PassNull();
                string routeID = data["cboRouteID_ImExMcp"].PassNull();

                 //string slsPerID = data["slsPer"].PassNull();
                string pJPID = data["cboPJPID_ImExMcp"].PassNull(); //BranchID;
                var date = DateTime.Now.Date;
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("fupImport_ImExMcp");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                string errorERouteID = string.Empty;
                string errorESlsperID = string.Empty;

                bool flagCheckError = false;
                string messagestrERouteID = string.Empty;
                string messagestrECustID = string.Empty;
                string messagestrESlsperID = string.Empty;
                string messagestrEBeginDate = string.Empty;
                string messagestrEEndDate = string.Empty;
                string messagestrETBH = string.Empty;
                string messagestrETS = string.Empty;
                string messageDate = string.Empty;
                string messageerror = string.Empty;
                string messageduplicate = string.Empty;
                string message = string.Empty;
                string messageduplicatefile = string.Empty;

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {

                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        string strEBanchID = workSheet.Cells[1, 2].StringValue;//dataArray.GetValue(2, 3).PassNull();// w1.Rows[1].Cells[2).PassNull();
                        string strEPJP = workSheet.Cells[1, 4].StringValue;// dataArray.GetValue(2, 3).PassNull();
                        string strERouteID = "";
                        string strECustID = "";
                        string strESlsperID = "";
                        string strEBeginDate = "";
                        string strEEndDate = "";
                        string strETS = "";
                        string strETBH = "";
                        string strESTT = "";
                        DateTime startDate = DateTime.Now;
                        DateTime endDate = DateTime.Now;
                        var lstOM_SalesRouteMasterImport = new List<OM_SalesRouteMasterImport>();

                        if (strEPJP.ToUpper().Trim() != pJPID.ToUpper().Trim() || BranchID != strEBanchID.ToUpper().Trim())
                        {
                            throw new MessageException(MessageType.Message, "201401221", "", new string[] { strEPJP, strEBanchID, pJPID, BranchID });
                        }
                        var objPJP = _db.OM_PJP.Where(p => p.PJPID == pJPID).FirstOrDefault();
                        if (objPJP == null)
                        {
                            objPJP = new OM_PJP();
                            objPJP.PJPID = pJPID;
                            objPJP.BranchID = BranchID;
                            objPJP.Descr = "Kế hoạch viếng thăm " + BranchID;
                            objPJP.LUpd_DateTime = DateTime.Today;
                            objPJP.LUpd_Prog = _screenName;
                            objPJP.LUpd_User = Current.UserName;
                            objPJP.Status = true;
                            objPJP.StatusHandle = "C";

                            objPJP.Crtd_DateTime = DateTime.Today;
                            objPJP.Crtd_Prog = _screenName;
                            objPJP.Crtd_User = Current.UserName;

                            _db.OM_PJP.AddObject(objPJP);
                        }

                        string lstCustomer = "";
                        string strtmpError = "";
                        string id = Guid.NewGuid().ToString();
                        for (int i = 5; i <= workSheet.Cells.MaxDataRow; i++)
                        {
                            strESTT = workSheet.Cells[i, 0].StringValue;//dataArray.GetValue(i, 1).PassNull();
                            strERouteID = workSheet.Cells[i, 17].StringValue;//dataArray.GetValue(i, 18).PassNull();
                            strECustID = workSheet.Cells[i, 1].StringValue;//dataArray.GetValue(i, 2).PassNull();
                            strESlsperID = workSheet.Cells[i, 4].StringValue;//dataArray.GetValue(i, 5).PassNull();
                            strEBeginDate = workSheet.Cells[i, 6].StringValue;//dataArray.GetValue(i, 7).PassNull();
                            strEEndDate = workSheet.Cells[i, 7].StringValue;//dataArray.GetValue(i, 8).PassNull();
                            strETS = workSheet.Cells[i, 8].StringValue;//dataArray.GetValue(i, 9).PassNull();
                            strETBH = workSheet.Cells[i, 9].StringValue;//dataArray.GetValue(i, 10).PassNull();
                            //if (strESTT.PassNull().Trim() == "") break;
                            if (strESTT.PassNull() == "" && strERouteID.PassNull() == "" &&
                                strECustID.PassNull() == "" && strESlsperID.PassNull() == "" &&
                                strEBeginDate.PassNull() == "" && strEEndDate.PassNull() == "" &&
                                strETS.PassNull() == "" && strETBH.PassNull() == "")
                                continue;

                            if (strESlsperID != slsperID && slsperID != "")
                            {
                                errorESlsperID += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            };
                            if (strERouteID != routeID && routeID != "")
                            {
                                errorERouteID += (i + 1).ToString() + ",";
                                flagCheckError = true;

                            };
                            
                            if (strERouteID == "")
                            {
                                messagestrERouteID += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            }
                            if (strECustID == "")
                            {
                                messagestrECustID += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            }

                            if (strESlsperID == "")
                            {
                                messagestrESlsperID += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            }
                            if (strETS == "")
                            {
                                messagestrETS += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            }
                            if (strETBH == "")
                            {
                                messagestrETBH += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            }
                            if (strEBeginDate == "")
                            {
                                messagestrEBeginDate += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            }
                            if (strEEndDate == "")
                            {
                                messagestrEEndDate += (i + 1).ToString() + ",";
                                flagCheckError = true;
                            }

                            if (flagCheckError == false)
                            {

                                try
                                {
                                    startDate = workSheet.Cells[i, 6].DateTimeValue.ToDateShort();// DateTime.FromOADate(double.Parse(workSheet.Cells[i, 6].StringValue)).Short();
                                    endDate = workSheet.Cells[i, 7].DateTimeValue.ToDateShort(); //DateTime.FromOADate(double.Parse(workSheet.Cells[i, 7].StringValue)).Short();

                                }
                                catch
                                {
                                    messageDate += string.Format(Message.GetString("2016082906", null), (i + 1).ToString());
                                    continue;

                                }

                                OM_SalesRouteMasterImport objImport = new OM_SalesRouteMasterImport();
                                bool isNew = false;
                                var recordExists = lstOM_SalesRouteMasterImport.FirstOrDefault(p => p.ID == id
                                                                                                && p.BranchID == BranchID
                                                                                                && p.PJPID == pJPID
                                                                                                && p.SalesRouteID == strERouteID
                                                                                                && p.CustID == strECustID
                                                                                                && p.SlsPerID == strESlsperID);
                                if (recordExists == null)
                                {
                                    lstCustomer += strECustID + ";";
                                    if (_db.OM_SalesRouteMasterImport.Where(p => p.ID == id
                                                                            && p.BranchID == BranchID
                                                                            && p.PJPID == pJPID
                                                                            && p.SalesRouteID == strERouteID
                                                                            && p.CustID == strECustID
                                                                            && p.SlsPerID == strESlsperID).ToList().Count == 0)
                                    {
                                        objImport.ID = id;
                                        objImport.BranchID = BranchID;
                                        objImport.PJPID = pJPID;
                                        objImport.SalesRouteID = strERouteID;
                                        objImport.CustID = strECustID;
                                        objImport.SlsPerID = strESlsperID;
                                        objImport.StartDate = startDate;
                                        objImport.EndDate = endDate; ;
                                        objImport.SlsFreq = workSheet.Cells[i, 8].StringValue;//  dataArray.GetValue(i, 9).ToString().Trim().ToUpper();
                                        objImport.SlsFreqType = "R";
                                        objImport.WeekofVisit = workSheet.Cells[i, 9].StringValue;// dataArray.GetValue(i, 10).ToString().Trim().ToUpper();
                                        objImport.Mon = workSheet.Cells[i, 10].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 11) == null ? false : dataArray.GetValue(i, 11).ToString().Trim().ToUpper() == "X" ? true : false;
                                        objImport.Tue = workSheet.Cells[i, 11].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 12) == null ? false : dataArray.GetValue(i, 12).ToString().Trim().ToUpper() == "X" ? true : false;
                                        objImport.Wed = workSheet.Cells[i, 12].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 13) == null ? false : dataArray.GetValue(i, 13).ToString().Trim().ToUpper() == "X" ? true : false;
                                        objImport.Thu = workSheet.Cells[i, 13].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 14) == null ? false : dataArray.GetValue(i, 14).ToString().Trim().ToUpper() == "X" ? true : false;
                                        objImport.Fri = workSheet.Cells[i, 14].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 15) == null ? false : dataArray.GetValue(i, 15).ToString().Trim().ToUpper() == "X" ? true : false;
                                        objImport.Sat = workSheet.Cells[i, 15].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 16) == null ? false : dataArray.GetValue(i, 16).ToString().Trim().ToUpper() == "X" ? true : false;
                                        objImport.Sun = workSheet.Cells[i, 16].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 17) == null ? false : dataArray.GetValue(i, 17).ToString().Trim().ToUpper() == "X" ? true : false;
                                        try
                                        {
                                            objImport.VisitSort = workSheet.Cells[i, 19].IntValue;// dataArray.GetValue(i, 20) == null ? 0 : dataArray.GetValue(i, 20).ToString().Trim().ToUpper() == "" ? 0 : int.Parse(dataArray.GetValue(i, 20).ToString().Trim().ToUpper());
                                        }
                                        catch
                                        {
                                            objImport.VisitSort = 0;
                                        }
                                        objImport.LUpd_DateTime = objImport.LUpd_DateTime = DateTime.Now;
                                        objImport.LUpd_Prog = objImport.LUpd_Prog = _screenName;
                                        objImport.LUpd_User = objImport.LUpd_User = Current.UserName;
                                        objImport.Crtd_DateTime = objImport.Crtd_DateTime = DateTime.Now;
                                        objImport.Crtd_Prog = objImport.Crtd_Prog = _screenName;
                                        objImport.Crtd_User = objImport.Crtd_User = Current.UserName;
                                        if (isValidSelOMSalesRouteMaster(objImport, false))
                                        {
                                            if (workSheet.Cells[i, 20].StringValue != null && workSheet.Cells[i, 20].StringValue == "X")
                                            {
                                                objImport.Del = true;

                                            }
                                            _db.OM_SalesRouteMasterImport.AddObject(objImport);
                                            lstOM_SalesRouteMasterImport.Add(objImport);
                                        }
                                        else
                                        {
                                            messageerror += (i + 1).ToString() + ",";
                                            //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu không hợp lệ" + "\r";
                                        }

                                    }
                                    else messageduplicate += (i + 1).ToString() + ",";  //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu bi trùng" + "\r";
                                }
                                else
                                    messageduplicatefile += (i + 1).ToString() + ",";
                            }
                        }

                        _db.SaveChanges();
                    
                        DataAccess dal = Util.Dal();
                        try
                        {

                            PJPProcess.PJP pjp = new PJPProcess.PJP(Current.UserName, "OM23800", dal);
                            dal.BeginTrans(IsolationLevel.ReadCommitted);
                            if (!pjp.OM23800CreateMCP(id))
                            {
                                dal.RollbackTrans();
                            }
                            else
                            {
                                dal.CommitTrans();
                            }
                        }
                        catch (Exception ex)
                        {
                            dal.RollbackTrans();
                            return Json(new { success = false, type = "error", errorMsg = ex.ToString() });

                        }
                        message = messagestrECustID == "" ? "" : string.Format(Message.GetString("2016082912",null), messagestrECustID, workSheet.Cells[3, 1].StringValue);
                        message += messagestrESlsperID == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrESlsperID, workSheet.Cells[3, 4].StringValue);
                        message += messagestrETS == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrETS, workSheet.Cells[3, 8].StringValue);
                        message += messagestrETBH == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrETBH, workSheet.Cells[3, 9].StringValue);
                        message += messagestrEBeginDate == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrEBeginDate, workSheet.Cells[3, 6].StringValue);
                        message += messagestrEEndDate == "" ? "" : string.Format(Message.GetString("2016082912",null), messagestrEEndDate, workSheet.Cells[3, 7].StringValue);
                        message += messageDate == "" ? "" : string.Format(Message.GetString("2016082906", null), messageDate.ToString());
                        message += messagestrERouteID == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrERouteID, workSheet.Cells[3, 17].StringValue);
                        message += messageerror == "" ? "" : string.Format(Message.GetString("2016082913", null), messageerror);
                        message += messageduplicate == "" ? "" : string.Format(Message.GetString("2016082903", null), messageduplicate);
                        message += messageduplicatefile == "" ? "" : string.Format(Message.GetString("2016082903", null), messageduplicatefile);
                        message += errorESlsperID == "" ? "" : string.Format(Message.GetString("2016082904", null), errorESlsperID);
                        message += errorERouteID == "" ? "" : string.Format(Message.GetString("2016082905", null), errorERouteID);
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                    }
                    return _logMessage;

                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
                return _logMessage;
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

        [HttpPost]
        public ActionResult ExportCust(FormCollection data, string[] provinces, string provinceRawValue, bool isUpdated)
        {
            try
            {
                string branchID = data["branchID"].PassNull();
                string branchName = data["branchName"].PassNull();
                string[] provincesDescr = provinceRawValue.Contains(", ") 
                    ? provinceRawValue.Split(new string[] { ", " }, StringSplitOptions.None) 
                    : new string[] { provinceRawValue };

                var headerRowIdx = 3;
                var maxRow = 1000;
                var ColTexts = new List<string>() { "N0", "SlsperID", "SlsName", "ShopID", "ShopName", "Attn", "Addr", "Province", "ProvinceCode", "District", "DistrictCode", "Phone", "CustClass", "ShopType", "SellProduct", "Latitude", "Longitude", "RefCustID", "InActive" };

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetMCP = workbook.Worksheets[0];
                SheetMCP.Name = Util.GetLang("Customer");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                Range range;
                Cell cell;


                #region master data
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                DataTable dtSlsperMaster = dal.ExecDataTable("OM23800_peSlsperIDCust", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtSlsperMaster, true, 0, 26, false);// du lieu Slsperson

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",",provinces)), ParameterDirection.Input, int.MaxValue));

                DataTable dtDistrict = dal.ExecDataTable("OM23800_peDistrictCust", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtDistrict, true, 0, 52, false);// du lieu District

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                DataTable dtCustClass = dal.ExecDataTable("OM23800_peCustClassCust", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtCustClass, true, 0, 78, false);// du lieu SalesRoute

                for (var i = 0; i < provinces.Length; i++)
                {
                    SheetMCP.Cells["DA" + (i + 1).ToString()].PutValue(provinces[i]);
                    SheetMCP.Cells["DB" + (i + 1).ToString()].PutValue(provincesDescr[i]);
                }

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                DataTable dtShopType = dal.ExecDataTable("OM23800_peShopType", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtShopType, true, 0, 130, false);// du lieu ShopType

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                DataTable dtSellProduct = dal.ExecDataTable("OM23800_peSellProduct", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtSellProduct, true, 0, 156, false);// du lieu SellProduct


                SheetMCP.Cells["GA1"].PutValue("X");
                #endregion

                #region header info
                // Title header
                SetCellValue(SheetMCP.Cells["B1"],
                    string.Format("{0}", Util.GetLang("OM23800EHeaderCust")),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetMCP.Cells.Merge(0, 1, 1, ColTexts.Count);

                // Title info
                SetCellValue(SheetMCP.Cells["B2"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["B3"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["C2"], branchID, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);
                SetCellValue(SheetMCP.Cells["C3"], branchName, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);

                // Header text columns
                for (int i = 0; i < ColTexts.Count; i++)
                {
                    var colIdx = i;
                    if(ColTexts[i] == "SellProduct")
                        SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang("OM23800SellProduct"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    else
                        SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(ColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }
                #endregion

                #region export data
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",", provinces)), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@IsUpdated", DbType.String, clsCommon.GetValueDBNull(isUpdated), ParameterDirection.Input, 10));

                DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportDataCust", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    for (int j = 0; j < ColTexts.Count; j++)
                    {
                        if (ColTexts[j] == "N0")
                        {
                            SheetMCP.Cells[headerRowIdx + 2 + i, j].PutValue(i + 1);
                        }
                        else if (dtDataExport.Columns.Contains(ColTexts[j]))
                        {
                            SheetMCP.Cells[headerRowIdx + 2 + i, j].PutValue(dtDataExport.Rows[i][ColTexts[j]]);
                        }
                    }
                }
                #endregion

                #region formular
                //SlsperID
                string formulaSlsper = "=$AA$2:$AA$" + (dtSlsperMaster.Rows.Count + 2);
                Validation validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaSlsper;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn mã nhân viên";
                validation.ErrorMessage = "Mã nhân viên này không tồn tại";

                CellArea area = new CellArea();
                area.StartRow = headerRowIdx + 2;
                area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                area.StartColumn = ColTexts.IndexOf("SlsperID");
                area.EndColumn = ColTexts.IndexOf("SlsperID");
                validation.AddArea(area);

                // ShopType
                string formulaShopType = "=$EA$2:$EA$" + (dtShopType.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaShopType;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn loại hình KD";
                validation.ErrorMessage = "Loại hình KD này không tồn tại";

                area = new CellArea();
                area.StartRow = headerRowIdx + 2;
                area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                area.StartColumn = ColTexts.IndexOf("ShopType");
                area.EndColumn = ColTexts.IndexOf("ShopType");
                validation.AddArea(area);

                // SellProduct
                string formulaSellProduct = "=$FA$2:$FA$" + (dtSellProduct.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaSellProduct;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn cấp độ cửa hàng";
                validation.ErrorMessage = "Cấp độ cửa hàng này không tồn tại";

                area = new CellArea();
                area.StartRow = headerRowIdx + 2;
                area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                area.StartColumn = ColTexts.IndexOf("SellProduct");
                area.EndColumn = ColTexts.IndexOf("SellProduct");
                validation.AddArea(area);

                // InActive
                string formulaInActive = "=$GA$1:$GA$1";
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaInActive;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn giá trị";
                validation.ErrorMessage = "Giá trị này không tồn tại";

                area = new CellArea();
                area.StartRow = headerRowIdx + 2;
                area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                area.StartColumn = ColTexts.IndexOf("InActive");
                area.EndColumn = ColTexts.IndexOf("InActive");
                validation.AddArea(area);

                // State
                string formulaState = "=$DB$1:$DB$" + (provinces.Length + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaState;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Tỉnh";
                validation.ErrorMessage = "Mã Tỉnh này không tồn tại";

                area = new CellArea();
                area.StartRow = headerRowIdx + 2;
                area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                area.StartColumn = ColTexts.IndexOf("Province");
                area.EndColumn = ColTexts.IndexOf("Province");
                validation.AddArea(area);

                // District
                string formula = string.Format("=OFFSET($BB$1,IFERROR(MATCH(I{0},$BC$2:$BC${1},0),{2}),0, IF(COUNTIF($BC$2:$BC${1},I{0})=0,1,COUNTIF($BC$2:$BC${1},I{0})),1)", new string[] { "6", (dtDistrict.Rows.Count + 1).ToString(), (dtDistrict.Rows.Count + 2).ToString() });
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formula;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn quận huyện";
                validation.ErrorMessage = "Mã quận huyện không tồn tại";

                area = new CellArea();
                area.StartRow = headerRowIdx + 2;
                area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                area.StartColumn = ColTexts.IndexOf("District");
                area.EndColumn = ColTexts.IndexOf("District");
                validation.AddArea(area);

                //Cust Class
                string formulaSales = "=$CA$2:$CA$" + (dtCustClass.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaSales;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn nhóm khách hàng";
                validation.ErrorMessage = "Mã nhóm này không tồn tại";

                area = new CellArea();
                area.StartRow = headerRowIdx + 2;
                area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                area.StartColumn = ColTexts.IndexOf("CustClass");
                area.EndColumn = ColTexts.IndexOf("CustClass");
                validation.AddArea(area);

                string formulaSlsName = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,2,0)),\"\",VLOOKUP({0},AA:AC,2,0))", "B6");
                SheetMCP.Cells[Getcell(ColTexts.IndexOf("SlsName")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaSlsName, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);

                string formulaDistrictCode = string.Format("=IF(ISERROR(VLOOKUP({0},CHOOSE({{2,1}},$BA$1:$BA${1},$BB$1:$BB${1}),2,0)),\"\",VLOOKUP({0},CHOOSE({{2,1}},$BA$1:$BA${1},$BB$1:$BB${1}),2,0))", "J6", dtDistrict.Rows.Count + 1);
                SheetMCP.Cells[Getcell(ColTexts.IndexOf("DistrictCode")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaDistrictCode, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);

                string formulaProvinceCode = string.Format("=IF(ISERROR(VLOOKUP({0},CHOOSE({{2,1}},$DA$1:$DA${1},$DB$1:$DB${1}),2,0)),\"\",VLOOKUP({0},CHOOSE({{2,1}},$DA$1:$DA${1},$DB$1:$DB${1}),2,0))", "H6", provinces.Length);
                SheetMCP.Cells[Getcell(ColTexts.IndexOf("ProvinceCode")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaProvinceCode, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);
                #endregion

                #region Fomat cell
                var strFirstRow = (headerRowIdx + 2).ToString();
                var strLastRow = (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2).ToString();

                style = SheetMCP.Cells["A" + strFirstRow].GetStyle();
                style.IsLocked = false;
                range = SheetMCP.Cells.CreateRange("A" + strFirstRow, 
                    "A" + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("SlsperID")) + strFirstRow, 
                    Getcell(ColTexts.IndexOf("SlsperID")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("ShopName")) + strFirstRow, 
                    Getcell(ColTexts.IndexOf("ShopName")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Attn")) + strFirstRow, 
                    Getcell(ColTexts.IndexOf("Attn")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Addr")) + strFirstRow, 
                    Getcell(ColTexts.IndexOf("Addr")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Province")) + strFirstRow, 
                    Getcell(ColTexts.IndexOf("Province")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("District")) + strFirstRow, 
                    Getcell(ColTexts.IndexOf("District")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Phone")) + strFirstRow, 
                    Getcell(ColTexts.IndexOf("Phone")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("CustClass")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("CustClass")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("ShopType")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("ShopType")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("SellProduct")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("SellProduct")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Latitude")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("Latitude")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Longitude")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("Longitude")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("RefCustID")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("RefCustID")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("InActive")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("InActive")) + strLastRow);
                range.SetStyle(style);

                style = SheetMCP.Cells["Z1"].GetStyle();
                style.Font.Color = Color.Transparent;
                style.IsLocked = false;
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;
                range = SheetMCP.Cells.CreateRange("Z1", "ZZ" + (dtSlsperMaster.Rows.Count + dtDistrict.Rows.Count + dtCustClass.Rows.Count + 100));
                range.ApplyStyle(style, flag);
                
                SheetMCP.Protect(ProtectionType.All);

                #endregion
                SheetMCP.AutoFitColumns();

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel")
                {
                    FileDownloadName = string.Format("{0}_{1}.xlsx", Util.GetLang("OM23800"), Util.GetLang("Customer"))
                };
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

        [HttpPost]
        public ActionResult ImportCust(FormCollection data, bool isUpdated)
        {
            try
            {
                string BranchID = data["branchID"].PassNull();
                var dataRowIdx = 5;
                var date = DateTime.Now.Date;
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("fupImport_ImExCust");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    var lineSuccess = new List<string>();
                    var lineBlank = new List<string>();
                    var lineExistRefCustID = new List<string>();
                    var lineExist = new List<string>();
                    var lineNoExist = new List<string>();
                    var lineSlsNoExist = new List<string>();
                    var lineInvalidDistrict = new List<string>();
                    var lineInvalidGeo = new List<string>();
                    var lineCustID = new List<string>();
                    var lineShopType = new List<string>();
                    var lineSlsPerID = new List<string>();
                    var lineShopID = new List<string>();
                    var lineShopName = new List<string>();
                    var lineAttn = new List<string>();
                    var lineAddr = new List<string>();
                    var lineProvince = new List<string>();
                    var lineDistrict = new List<string>();
                    var lineCustClass = new List<string>();
                    var flagCheckError = false;
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        string strEBranchID = workSheet.Cells[1, 2].StringValue.Trim();
                        string strSlsPerID = string.Empty;
                        string strShopID = string.Empty;
                        string strShopName = string.Empty;
                        string strAttn = string.Empty;
                        string strAddr = string.Empty;
                        string strDistrict = string.Empty;
                        string strProvince = string.Empty;
                        string strPhone = string.Empty;
                        string strCustClass = string.Empty;
                        string strLocation = string.Empty;
                        string strRefCustID = string.Empty;
                        string strShopType = string.Empty;
                        string strSellProduct = string.Empty;
                        string strCountry = string.Empty;
                        string strCity = string.Empty;
                        string strTerritory = string.Empty;
                        string strInActive = string.Empty;
                        bool autoCustID = true;
                        double lat = 0;
                        double lng = 0;

                        if (strEBranchID == BranchID)
                        {
                            var objAR_Setup = _db.AR_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupId == "AR");
                            if (objAR_Setup != null)
                                autoCustID = objAR_Setup.AutoCustID;

                            for (int i = dataRowIdx; i <= workSheet.Cells.MaxDataRow; i++)
                            {
                                //if (workSheet.Cells[i,0].StringValue.PassNull().Trim() == "") break;
                                strSlsPerID = workSheet.Cells[i, 1].StringValue.Trim();
                                strShopID = workSheet.Cells[i, 3].StringValue.Trim();
                                strShopName = workSheet.Cells[i, 4].StringValue.Trim();
                                strAttn = workSheet.Cells[i, 5].StringValue.Trim();
                                strAddr = workSheet.Cells[i, 6].StringValue.Trim();
                                strDistrict = workSheet.Cells[i, 10].StringValue.Trim();
                                strProvince = workSheet.Cells[i, 8].StringValue.Trim();
                                strPhone = workSheet.Cells[i, 11].StringValue.Trim();
                                strCustClass = workSheet.Cells[i, 12].StringValue.Trim();
                                strShopType = workSheet.Cells[i, 13].StringValue.Trim();
                                strSellProduct = workSheet.Cells[i, 14].StringValue.Trim();
                                double.TryParse(workSheet.Cells[i, 15].StringValue.Trim(), out lat); //13 
                                double.TryParse(workSheet.Cells[i, 16].StringValue.Trim(), out lng); //14 
                                strRefCustID = workSheet.Cells[i, 17].StringValue.Trim();
                                strInActive = workSheet.Cells[i, 18].StringValue.Trim();

                                if (_checkRequireImport == false)
                                {
                                    if (strSlsPerID == "" && strShopID == "" && strShopName == "" && strAttn == ""
                                        && strAddr == "" && strProvince == "" && strDistrict == "" && strCustClass == "")
                                        continue;
                                }
                                else
                                {
                                    if (strSlsPerID == "" && strShopID == "" && strShopName == "" && strAttn == ""
                                        && strAddr == "" && strProvince == "" && strDistrict == "" && strCustClass == "")
                                        continue;

                                    if (strShopType == "")
                                    {
                                        lineShopType.Add((i - dataRowIdx + 1).ToString());
                                        flagCheckError = true;
                                    }

                                }

                                if (strSlsPerID == "")
                                {
                                    lineSlsPerID.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                //if (strShopID == "")
                                //{
                                //    lineShopID.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
                                if (strShopName == "")
                                {
                                    lineShopName.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (strAttn == "")
                                {
                                    lineAttn.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (strAddr == "")
                                {
                                    lineAddr.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (strProvince == "")
                                {
                                    lineProvince.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (strDistrict == "")
                                {
                                    lineDistrict.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (strCustClass == "")
                                {
                                    lineCustClass.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (flagCheckError == false)
                                {
                                    if (!string.IsNullOrWhiteSpace(strSlsPerID)
                                        || !string.IsNullOrWhiteSpace(strShopID)
                                        || !string.IsNullOrWhiteSpace(strShopName)
                                        || !string.IsNullOrWhiteSpace(strAttn)
                                        || !string.IsNullOrWhiteSpace(strAddr)
                                        || !string.IsNullOrWhiteSpace(strDistrict)
                                        || !string.IsNullOrWhiteSpace(strProvince)
                                        || !string.IsNullOrWhiteSpace(strPhone)
                                        || !string.IsNullOrWhiteSpace(strCustClass)
                                        || !string.IsNullOrWhiteSpace(strLocation)
                                        || lat > 0 || lng > 0)
                                    {
                                        var slsright = true;
                                        if (!string.IsNullOrWhiteSpace(strSlsPerID))
                                        {
                                            var slsper = _db.AR_Salesperson.FirstOrDefault(s => s.SlsperId == strSlsPerID && s.BranchID == strEBranchID);
                                            if (slsper == null)
                                            {
                                                lineSlsNoExist.Add((i - dataRowIdx + 1).ToString());
                                                slsright = false;
                                            }
                                        }
                                        else
                                        {
                                            //lineSlsPerID.Add((i - dataRowIdx + 1).ToString());
                                            //continue;
                                        }
                                        
                                        if (!string.IsNullOrWhiteSpace(strDistrict))
                                        {
                                            var district = _db.SI_District.FirstOrDefault(d => d.District == strDistrict && d.State == strProvince);
                                            if (district == null)
                                            {
                                                lineInvalidDistrict.Add((i - dataRowIdx + 1).ToString());
                                                slsright = false;
                                            }
                                            else
                                            {
                                                strCountry = district.Country;
                                                var state = _db.SI_State.FirstOrDefault(s => s.Country == strCountry && s.State == strProvince);
                                                if (state != null)
                                                {
                                                    strTerritory = state.Territory;
                                                }
                                                var city = _db.SI_City.FirstOrDefault(c => c.Country == strCountry && c.State == strProvince);
                                                if (city != null)
                                                {
                                                    strCity = city.City;
                                                }
                                            }
                                        }

                                        if (isUpdated)
                                        {
                                            if (!string.IsNullOrWhiteSpace(strShopID)
                                                && !string.IsNullOrWhiteSpace(strShopName)
                                                && !string.IsNullOrWhiteSpace(strDistrict)
                                                && !string.IsNullOrWhiteSpace(strProvince)
                                                && !string.IsNullOrWhiteSpace(strCustClass))
                                            {
                                                if (_checkRequireImport == true && string.IsNullOrWhiteSpace(strShopType))
                                                    continue;
                                                if (slsright)
                                                {
                                                    var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustId == strShopID && c.BranchID == strEBranchID);
                                                    if (existCust != null)
                                                    {
                                                        if (existCust.RefCustID != strRefCustID && strRefCustID.PassNull()!="")
                                                        {
                                                            var existRefCustID = _db.AR_Customer.FirstOrDefault(p => p.BranchID == strEBranchID && p.RefCustID == strRefCustID);
                                                            if (existRefCustID == null)
                                                            {
                                                                existCust.CustName = existCust.BillName = strShopName;
                                                                existCust.Attn = existCust.BillAttn = strAttn;
                                                                existCust.Addr1 = existCust.BillAddr1 = strAddr;
                                                                existCust.District = strDistrict;
                                                                existCust.State = existCust.BillState = strProvince;
                                                                existCust.Phone = existCust.BillPhone = strPhone;
                                                                existCust.Country = existCust.BillCountry = strCountry;
                                                                existCust.City = existCust.BillCity = strCity;
                                                                existCust.Territory = strTerritory;
                                                                existCust.ClassId = strCustClass;
                                                                existCust.Location = strLocation;
                                                                existCust.LUpd_Datetime = DateTime.Now;
                                                                existCust.LUpd_Prog = _screenName;
                                                                existCust.LUpd_User = Current.UserName;
                                                                existCust.SlsperId = strSlsPerID.PassNull();
                                                                existCust.ShopType = strShopType;
                                                                existCust.SellProduct = strSellProduct;
                                                                existCust.RefCustID = strRefCustID;
                                                                if (strInActive == "X")
                                                                    existCust.Status = "H";
                                                                else
                                                                    existCust.Status = "A";
                                                                if (lat > 0 && lng > 0)
                                                                {
                                                                    updateCustomerLocation(strEBranchID, strShopID, lat, lng);
                                                                }
                                                                else if (lat > 0 || lng > 0)
                                                                {
                                                                    lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
                                                                }

                                                                lineSuccess.Add((i - dataRowIdx + 1).ToString());
                                                            }
                                                            else
                                                            {
                                                                lineExistRefCustID.Add((i - dataRowIdx + 1).ToString());
                                                            }
                                                        }
                                                        else
                                                        {
                                                            existCust.CustName = existCust.BillName = strShopName;
                                                            existCust.Attn = existCust.BillAttn = strAttn;
                                                            existCust.Addr1 = existCust.BillAddr1 = strAddr;
                                                            existCust.District = strDistrict;
                                                            existCust.State = existCust.BillState = strProvince;
                                                            existCust.Phone = existCust.BillPhone = strPhone;
                                                            existCust.Country = existCust.BillCountry = strCountry;
                                                            existCust.City = existCust.BillCity = strCity;
                                                            existCust.Territory = strTerritory;
                                                            existCust.ClassId = strCustClass;
                                                            existCust.Location = strLocation;
                                                            existCust.LUpd_Datetime = DateTime.Now;
                                                            existCust.LUpd_Prog = _screenName;
                                                            existCust.LUpd_User = Current.UserName;
                                                            existCust.SlsperId = strSlsPerID.PassNull();
                                                            existCust.ShopType = strShopType;
                                                            existCust.SellProduct = strSellProduct;
                                                            existCust.RefCustID = strRefCustID;
                                                            if (strInActive == "X")
                                                                existCust.Status = "H";
                                                            else
                                                                existCust.Status = "A";
                                                            if (lat > 0 && lng > 0)
                                                            {
                                                                updateCustomerLocation(strEBranchID, strShopID, lat, lng);
                                                            }
                                                            else if (lat > 0 || lng > 0)
                                                            {
                                                                lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
                                                            }

                                                            lineSuccess.Add((i - dataRowIdx + 1).ToString());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        lineNoExist.Add((i - dataRowIdx + 1).ToString());
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //lineBlank.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrWhiteSpace(strShopName)
                                                && !string.IsNullOrWhiteSpace(strDistrict)
                                                && !string.IsNullOrWhiteSpace(strProvince)
                                                && !string.IsNullOrWhiteSpace(strCustClass))
                                            {
                                                if (_checkRequireImport == true && string.IsNullOrWhiteSpace(strShopType))
                                                    continue;
                                                if (slsright)
                                                {
                                                    var canInsert = true;
                                                    if (!string.IsNullOrWhiteSpace(strShopID))
                                                    {
                                                        var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustId == strShopID && c.BranchID == strEBranchID);
                                                        if (existCust != null)
                                                        {
                                                            lineExist.Add((i - dataRowIdx + 1).ToString());
                                                            canInsert = false;
                                                        }
                                                        else
                                                        {
                                                            if(autoCustID == true)
                                                                strShopID = _db.OM23800_CustID(strEBranchID, strProvince, strDistrict, strCustClass).FirstOrDefault().ToString();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustName == strShopName && c.Addr1 == strAddr && c.BranchID == strEBranchID);
                                                        if (existCust != null)
                                                        {
                                                            lineExist.Add((i - dataRowIdx + 1).ToString());
                                                            canInsert = false;
                                                        }
                                                        else
                                                        {
                                                            if (autoCustID == true)
                                                                strShopID = _db.OM23800_CustID(strEBranchID, strProvince, strDistrict, strCustClass).FirstOrDefault().ToString();
                                                            else
                                                            {
                                                                lineCustID.Add((i - dataRowIdx + 1).ToString());
                                                                canInsert = false;
                                                            }
                                                                // bao loi thieu custid

                                                        }
                                                    }
                                                    if (canInsert)
                                                    {
                                                        if (strRefCustID.PassNull() != "")
                                                        {
                                                            var existRefCustID = _db.AR_Customer.FirstOrDefault(p => p.BranchID == strEBranchID && p.RefCustID == strRefCustID);
                                                            if (existRefCustID != null)
                                                            {
                                                                lineExistRefCustID.Add((i - dataRowIdx + 1).ToString());
                                                                canInsert = false;
                                                            }
                                                        }
                                                    }
                                                    if (canInsert)
                                                    {
                                                        var newCust = new AR_Customer();
                                                        newCust.ResetET();
                                                        newCust.ExpiryDate = DateTime.Now.ToDateShort();
                                                        newCust.Birthdate = new DateTime(1900, 1, 1).ToDateShort();
                                                        newCust.EstablishDate = new DateTime(1900, 1, 1).ToDateShort();
                                                        newCust.CustId = strShopID;
                                                        newCust.BranchID = strEBranchID;
                                                        newCust.CustName = newCust.BillName = strShopName;
                                                        newCust.Attn = newCust.BillAttn = strAttn;
                                                        newCust.Addr1 = newCust.BillAddr1 = strAddr;
                                                        newCust.District = strDistrict;
                                                        newCust.State = newCust.BillState = strProvince;
                                                        newCust.Phone = newCust.BillPhone = strPhone;
                                                        newCust.Country = newCust.BillCountry = strCountry;
                                                        newCust.City = newCust.BillCity = strCity;
                                                        newCust.Territory = strTerritory;
                                                        newCust.ClassId = strCustClass;
                                                        newCust.Location = strLocation;
                                                        newCust.CrRule = "N";
                                                        newCust.CustType = "R";
                                                        newCust.DfltShipToId = "DEFAULT";
                                                        newCust.NodeLevel = 2;
                                                        newCust.ParentRecordID = 4;
                                                        if (strInActive == "X")
                                                            newCust.Status = "H";
                                                        else
                                                            newCust.Status = "A";
                                                        newCust.SupID = "";
                                                        newCust.TaxDflt = "C";
                                                        newCust.TaxID00 = "OVAT10-00";
                                                        newCust.TaxID01 = "OVAT05-00";
                                                        newCust.TaxID02 = "VAT00";
                                                        newCust.TaxID03 = "NONEVAT";
                                                        newCust.TaxLocId = "";
                                                        newCust.TaxRegNbr = "123456789";
                                                        newCust.Terms = "07";
                                                        newCust.LUpd_Datetime = newCust.Crtd_Datetime = DateTime.Now;
                                                        newCust.LUpd_Prog = newCust.Crtd_Prog = _screenName;
                                                        newCust.LUpd_User = newCust.Crtd_User = Current.UserName;
                                                        newCust.SlsperId = strSlsPerID;
                                                        newCust.ShopType = strShopType;
                                                        newCust.SellProduct = strSellProduct;
                                                        newCust.RefCustID = strRefCustID;
                                                        _db.AR_Customer.AddObject(newCust);

                                                        if (lat > 0 && lng > 0)
                                                        {
                                                            updateCustomerLocation(strEBranchID, strShopID, lat, lng);
                                                        }
                                                        else if (lat > 0 || lng > 0)
                                                        {
                                                            lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
                                                        }

                                                        _db.SaveChanges();
                                                        lineSuccess.Add((i - dataRowIdx + 1).ToString());
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //lineBlank.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                    }
                                }
                            }
                            if (isUpdated)
                            {
                                _db.SaveChanges();
                            }
                            if (lineSuccess.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082907",null),
                                    lineSuccess.Count > 5 ? string.Join(", ", lineSuccess.Take(5)) + ", ..." : string.Join(", ", lineSuccess));
                            }
                            if (lineCustID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineCustID.Count > 5 ? string.Join(", ", lineCustID.Take(5)) + ", ..." : string.Join(", ", lineCustID), workSheet.Cells[3, 3].StringValue);
                            }
                            if (lineSlsPerID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineSlsPerID.Count > 5 ? string.Join(", ", lineSlsPerID.Take(5)) + ", ..." : string.Join(", ", lineSlsPerID), workSheet.Cells[3,1].StringValue);
                            }
                            if (lineShopID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineShopID.Count > 5 ? string.Join(", ", lineShopID.Take(5)) + ", ..." : string.Join(", ", lineShopID), workSheet.Cells[3, 3].StringValue);
                            }
                            if (lineShopName.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineShopName.Count > 5 ? string.Join(", ", lineShopName.Take(5)) + ", ..." : string.Join(", ", lineShopName), workSheet.Cells[3, 4].StringValue);
                            }
                            if (lineAttn.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineAttn.Count > 5 ? string.Join(", ", lineAttn.Take(5)) + ", ..." : string.Join(", ", lineAttn), workSheet.Cells[3, 5].StringValue);
                            }
                            if (lineAddr.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineAddr.Count > 5 ? string.Join(", ", lineAddr.Take(5)) + ", ..." : string.Join(", ", lineAddr), workSheet.Cells[3, 6].StringValue);
                            }
                            if (lineProvince.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineProvince.Count > 5 ? string.Join(", ", lineProvince.Take(5)) + ", ..." : string.Join(", ", lineProvince), workSheet.Cells[3, 8].StringValue);
                            }
                            if (lineDistrict.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineDistrict.Count > 5 ? string.Join(", ", lineDistrict.Take(5)) + ", ..." : string.Join(", ", lineDistrict), workSheet.Cells[3, 10].StringValue);
                            }
                            if (lineCustClass.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineCustClass.Count > 5 ? string.Join(", ", lineCustClass.Take(5)) + ", ..." : string.Join(", ", lineCustClass), workSheet.Cells[3, 12].StringValue);
                            }
                            if (lineShopType.Count > 0 && _checkRequireImport == true)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineShopType.Count > 5 ? string.Join(", ", lineShopType.Take(5)) + ", ..." : string.Join(", ", lineShopType), workSheet.Cells[3, 13].StringValue);
                            }
                            if (lineExist.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082908", null),
                                    lineExist.Count > 5 ? string.Join(", ", lineExist.Take(5)) + ", ..." : string.Join(", ", lineExist));
                            }
                            if (lineNoExist.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082909", null),
                                    lineNoExist.Count > 5 ? string.Join(", ", lineNoExist.Take(5)) + ", ..." : string.Join(", ", lineNoExist));
                            }
                            if (lineSlsNoExist.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082910", null),
                                    lineSlsNoExist.Count > 5 ? string.Join(", ", lineSlsNoExist.Take(5)) + ", ..." : string.Join(", ", lineSlsNoExist));
                            }
                            if (lineExistRefCustID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016090701", null),
                                    lineExistRefCustID.Count > 5 ? string.Join(", ", lineExistRefCustID.Take(5)) + ", ..." : string.Join(", ", lineExistRefCustID));
                            }
                            if (lineInvalidDistrict.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082911", null),
                                    lineInvalidDistrict.Count > 5 ? string.Join(", ", lineInvalidDistrict.Take(5)) + ", ..." : string.Join(", ", lineInvalidDistrict));
                            }
                            Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "20150611", "", new string[] { strEBranchID, BranchID });
                        }
                    }
                    return _logMessage;

                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
                return _logMessage;
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {                   
                    return Json(new { success = false, type = "error", errorMsg = ex.Message.ToString() });
                }
            }
        }

        private void updateCustomerLocation(string branchID, string custID, double lat, double lng)
        {
            var custLoc = _db.AR_CustomerLocation.FirstOrDefault(l => l.BranchID == branchID && l.CustID == custID);
            if (custLoc != null)
            {
                custLoc.Lat = lat;
                custLoc.Lng = lng;

                custLoc.LUpd_Datetime = DateTime.Now;
                custLoc.LUpd_Prog = _screenName;
                custLoc.LUpd_User = Current.UserName;
            }
            else
            {
                custLoc = new AR_CustomerLocation();
                custLoc.ResetET();
                custLoc.CustID = custID;
                custLoc.BranchID = branchID;
                custLoc.Lat = lat;
                custLoc.Lng = lng;
                custLoc.Crtd_Datetime = custLoc.LUpd_Datetime = DateTime.Now;
                custLoc.Crtd_Prog = custLoc.LUpd_Prog = _screenName;
                custLoc.Crtd_User = custLoc.LUpd_User = Current.UserName;
                _db.AR_CustomerLocation.AddObject(custLoc);
            }
        }

        private void updateSaleRoutesMaster(ref OM_SalesRouteMaster updated, OM_SalesRouteMaster inputted)
        {
            updated.SlsFreq = inputted.SlsFreq;
            updated.SlsFreqType = "R";
            updated.VisitSort = inputted.VisitSort;
            updated.WeekofVisit = inputted.WeekofVisit;
            updated.Mon = inputted.Mon;
            updated.Tue = inputted.Tue;
            updated.Wed = inputted.Wed;
            updated.Thu = inputted.Thu;
            updated.Fri = inputted.Fri;
            updated.Sat = inputted.Sat;
            updated.Sun = inputted.Sun;

            updated.Crtd_DateTime = DateTime.Now;
            updated.Crtd_Prog = _screenName;
            updated.Crtd_User = Current.UserName;

            updated.LUpd_DateTime = DateTime.Now;
            updated.LUpd_Prog = _screenName;
            updated.LUpd_User = Current.UserName;

            updated.StartDate = DateTime.Now.Date;
            updated.EndDate = new DateTime(DateTime.Now.Year, 12, 31);
        }

        private bool isValidSelOMSalesRouteMaster(OM_SalesRouteMasterImport OM, bool Message)
        {
            int iVisit = 0;
            try
            {
                if (OM.Mon == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Sat == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Sun == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Fri == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Thu == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Tue == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Wed == true)
                {
                    iVisit = iVisit + 1;
                }

                switch (OM.SlsFreq)
                {
                    case "F1":
                    case "F1/2":
                    case "F1/3":
                    case "F2":
                    case "F4":
                    case "F4A":
                        if (iVisit != 1)
                        {
                            //if (_complete)
                            //    HQMessageBox.Show(53, hqSys.LangID, null, HQmesscomplete);
                            //_complete = false;
                            return false;
                        }
                        break;
                    case "F8":
                    case "F8A":
                        if (iVisit != 2)
                        {
                            return false;
                        }
                        break;
                    case "F12":
                        if (iVisit != 3)
                        {
                            return false;
                        }
                        break;
                    case "F16":
                        if (iVisit != 4)
                        {
                            return false;
                        }
                        break;
                    case "F20":
                        if (iVisit != 5)
                        {
                            return false;
                        }
                        break;
                    case "F24":
                        if (iVisit != 6)
                        {
                            return false;
                        }
                        break;
                    case "A":
                        if (iVisit == 0)
                        {
                            return false;
                        }
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {

                throw ex;
                return false;
            }
        }
        private bool isValidSelOMSalesRouteMaster(OM_SalesRouteMaster OM)
        {
            int iVisit = 0;
            try
            {
                if (OM.Mon == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Sat == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Sun == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Fri == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Thu == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Tue == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Wed == true)
                {
                    iVisit = iVisit + 1;
                }

                switch (OM.SlsFreq)
                {
                    case "F1":
                    case "F1/2":
                    case "F1/3":
                    case "F2":
                    case "F4":
                    case "F4A":
                        if (iVisit != 1)
                        {
                            //if (_complete)
                            //    HQMessageBox.Show(53, hqSys.LangID, null, HQmesscomplete);
                            //_complete = false;
                            return false;
                        }
                        break;
                    case "F8":
                    case "F8A":
                        if (iVisit != 2)
                        {
                            return false;
                        }
                        break;
                    case "F12":
                        if (iVisit != 3)
                        {
                            return false;
                        }
                        break;
                    case "F16":
                        if (iVisit != 4)
                        {
                            return false;
                        }
                        break;
                    case "F20":
                        if (iVisit != 5)
                        {
                            return false;
                        }
                        break;
                    case "F24":
                        if (iVisit != 6)
                        {
                            return false;
                        }
                        break;
                    case "A":
                        if (iVisit == 0)
                        {
                            return false;
                        }
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {

                throw ex;
                return false;
            }
        }
        
        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle = false)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
                style.Font.Color = Color.Red;
            c.SetStyle(style);
        }

        private void setCellStyle(Cell cell)
        {
            //Create a style object
            Style style = cell.GetStyle();

            //Setting the line style of the top border
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the top border
            style.Borders[BorderType.TopBorder].Color = Color.Black;

            //Setting the line style of the bottom border
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the bottom border
            style.Borders[BorderType.BottomBorder].Color = Color.Black;

            //Setting the line style of the left border
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the left border
            style.Borders[BorderType.LeftBorder].Color = Color.Black;

            //Setting the line style of the right border
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the right border
            style.Borders[BorderType.RightBorder].Color = Color.Black;

            cell.SetStyle(style);
        }

        private string Getcell(int column)
        {
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 > 1)
            {
                cell += ABC.Substring(column / 26, 1);
                column += column - 26;
            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            return cell;
        }
    }
}

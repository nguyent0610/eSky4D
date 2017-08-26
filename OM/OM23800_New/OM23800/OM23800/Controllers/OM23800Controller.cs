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
using HQFramework.Common;
using System.Web;
using System.Globalization;
using System.Data.SqlClient;
//using HQ.eSkySys;

namespace OM23800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]

    public class OM23800Controller : Controller
    {
        
        private string _screenName = "OM23800";
        OM23800Entities _db = Util.CreateObjectContext<OM23800Entities>(false);
        HQ.eSkySys.eSkySysEntities _sys = Util.CreateObjectContext<HQ.eSkySys.eSkySysEntities>(true);
        private JsonResult _logMessage;
        private bool _checkRequireImport = false;
        string activeStatus = "A";
        string inActiveStatus = "I";
        List<string> _lstRefCustID = new List<string>();
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

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult SuggestView(string lang)
        {
            return PartialView();
        }

        public ActionResult LoadMCP(string routeID,string pJPID, string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit,
            bool hightLight, bool isnumberingCust, string colorFor = ""            
            , double? amtFrom = 0, double? amtTo = 0, string brand = "", string markFor = "")
        {
            _db.CommandTimeout = 3600;

            var planVisits = _db.OM23800_pgMCL( routeID, pJPID,Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit,
                amtFrom, amtTo, brand, isnumberingCust, Current.CpnyID, Current.LangID).ToList();

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
                            if (!custActive)
                            {
                                cust.SlsperId = "";
                            }
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

        #region - ExportSelectedCust_old Delete ? -
       
        //[DirectMethod]
        //public ActionResult ExportSelectedCust_old(string custIDs)
        //{
        //    try
        //    {
        //        string[] paraStringsD = JSON.Deserialize<string[]>(custIDs);

        //        if (custIDs.Length > 0)
        //        {
        //            var headerRowIdx = 3;

        //            Stream stream = new MemoryStream();
        //            Workbook workbook = new Workbook();
        //            Worksheet SheetMCP = workbook.Worksheets[0];
        //            SheetMCP.Name = Util.GetLang("MCL");
        //            DataAccess dal = Util.Dal();
        //            Style style = workbook.GetStyleInPool(0);
        //            StyleFlag flag = new StyleFlag();
        //            var beforeColTexts = new string[] { "N0", "BranchID", "CpnyName", "SlsperID", "SlsName", "CustID", "CustName", "Address", "WeekofVisit", "DayOfWeek" };

        //            #region header info
        //            // Title header
        //            SetCellValue(SheetMCP.Cells["A2"],
        //                string.Format("{0} ({1})", Util.GetLang("OM23800EHeader"), Util.GetLang("DrawingArea")),
        //                TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
        //            SheetMCP.Cells.Merge(1, 0, 1, beforeColTexts.Length);

        //            // Header text columns

        //            for (int i = 0; i < beforeColTexts.Length; i++)
        //            {
        //                var cell = SheetMCP.Cells[headerRowIdx, i];
        //                SetCellValue(cell, Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
        //                //SheetMCP.Cells.Merge(headerRowIdx, colIdx, 1, 1);

        //                //Apply the border styles to the cell
        //                setCellStyle(cell);
        //            }
        //            var allColumns = new List<string>();
        //            allColumns.AddRange(beforeColTexts);

        //            #endregion

        //            #region export data
        //            ParamCollection pc = new ParamCollection();
        //            pc.Add(new ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(string.Join(",", paraStringsD)), ParameterDirection.Input, int.MaxValue));
        //            pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

        //            DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportSelectedCust", CommandType.StoredProcedure, ref pc);
        //            //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

        //            for (int i = 0; i < dtDataExport.Rows.Count; i++)
        //            {
        //                for (int j = 0; j < allColumns.Count; j++)
        //                {
        //                    var cell = SheetMCP.Cells[headerRowIdx + 1 + i, j];
        //                    if (allColumns[j] == "N0")
        //                    {
        //                        cell.PutValue(i + 1);
        //                    }
        //                    else if (dtDataExport.Columns.Contains(allColumns[j]))
        //                    {
        //                        cell.PutValue(dtDataExport.Rows[i][allColumns[j]]);
        //                    }

        //                    //Apply the border styles to the cell
        //                    setCellStyle(cell);
        //                }
        //            }
        //            #endregion
        //            SheetMCP.AutoFitColumns();

        //            //SheetPOSuggest.Protect(ProtectionType.Objects);
        //            workbook.Save(stream, SaveFormat.Xlsx);
        //            stream.Flush();
        //            stream.Position = 0;

        //            //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM30400") + ".xlsx" };
        //            return File(stream, "application/vnd.ms-excel", string.Format("{0}_{1}.xlsx", Util.GetLang("OM23800"), Util.GetLang("DrawingArea")));
        //        }
        //        else
        //        {
        //            throw new MessageException(MessageType.Message, "14091901");
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
        //            return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //        }
        //    }

        //}
        #endregion

        #region -Export & import MCL-                
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
                   // Cell cell;
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
                        SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                        SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                    }

                    //Route column
                    var daysOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                    SetCellValue(SheetMCP.Cells[headerRowIdx, beforeColTexts.Length], Util.GetLang("Route"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                    SheetMCP.Cells.Merge(headerRowIdx, beforeColTexts.Length, 1, daysOfWeeks.Length);
                    for (int i = 0; i < daysOfWeeks.Length; i++)
                    {
                        var colIdx = beforeColTexts.Length + i;
                        SetCellValue(SheetMCP.Cells[headerRowIdx + 1, colIdx], Util.GetLang(daysOfWeeks[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                    }

                    // after of Route column
                    var afterColTexts = new string[] { "SalesRouteID", "RouteName", "VisitSort", "CustCancel" };
                    for (int i = 0; i < afterColTexts.Length; i++)
                    {
                        var colIdx = beforeColTexts.Length + daysOfWeeks.Length + i;
                        SetCellValue(SheetMCP.Cells[headerRowIdx, colIdx], Util.GetLang(afterColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);



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
                    string formulaRequenc = "F1A,F1B,F1C,F1D,F1,F2,F4,F4A,F8,F12,F1/2,F1/3,A";
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
                    SetCellValue(cell, Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
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
        #endregion

        #region -Export & import MCP-
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
                int numberRow = 500;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetMCP = workbook.Worksheets[0];
                SheetMCP.Name = Util.GetLang("MCP");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                Range range;
                //Cell cell;
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
                    SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }

                //Route column
                var daysOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                SetCellValue(SheetMCP.Cells[headerRowIdx, beforeColTexts.Length], Util.GetLang("Route"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                SheetMCP.Cells.Merge(headerRowIdx, beforeColTexts.Length, 1, daysOfWeeks.Length);
                for (int i = 0; i < daysOfWeeks.Length; i++)
                {
                    var colIdx = beforeColTexts.Length + i;
                    SetCellValue(SheetMCP.Cells[headerRowIdx + 1, colIdx], Util.GetLang(daysOfWeeks[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                }

                // after of Route column
                var afterColTexts = new string[] { "SalesRouteID", "RouteName", "VisitSort", "CustCancel" };
                for (int i = 0; i < afterColTexts.Length; i++)
                {
                    var colIdx = beforeColTexts.Length + daysOfWeeks.Length + i;
                    SetCellValue(SheetMCP.Cells[headerRowIdx, colIdx], Util.GetLang(afterColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }

                var allColumns = new List<string>();
                allColumns.AddRange(beforeColTexts);
                allColumns.AddRange(daysOfWeeks);
                allColumns.AddRange(afterColTexts);

                #endregion

                #region export data
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(pJPID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.String, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportDataMCP", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export
                
                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        if (allColumns[j] == "N0") //  || allColumns[j] == "CustName" || allColumns[j] == "SlsName" || allColumns[j] == "Address" || allColumns[j] == "RouteName"
                        {
                            //SheetMCP.Cells[5 + i, j].PutValue(i + 1);
                        }
                        else if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            SheetMCP.Cells[5 + i, j].PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }
                    }
                }
                if (dtDataExport.Rows.Count == 0)
                {
                    numberRow = 20000;
                }
                else
                {
                    numberRow = dtDataExport.Rows.Count + numberRow;
                }
                
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
                if (dtCustomer.Rows.Count > 0)
                {
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
                }
                if (dtSales.Rows.Count > 0)
                {
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
                }
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
                string formulaRequenc = "F1A,F1B,F1C,F1D,F1,F2,F4,F4A,F8,F12,F1/2,F1/3,A";
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

                if (dtCustomer.Rows.Count > 0)
                {
                    string formulaCustName = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,2,0)),\"\",VLOOKUP({0},AA:AC,2,0))", "B6");
                    SheetMCP.Cells[Getcell(allColumns.IndexOf("CustName")) + "6"].SetSharedFormula(formulaCustName, numberRow, 1); // (dtCustomer.Rows.Count + 6)

                    string formulaCustAddr = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,3,0)),\"\",VLOOKUP({0},AA:AC,3,0))", "B6");
                    SheetMCP.Cells[Getcell(allColumns.IndexOf("Address")) + "6"].SetSharedFormula(formulaCustAddr, numberRow, 1);// (dtCustomer.Rows.Count + 6)
                }
                if (dtSales.Rows.Count > 0)
                {
                    string formulaSalesName = string.Format("=IF(ISERROR(VLOOKUP({0},BA:BC,2,0)),\"\",VLOOKUP({0},BA:BC,2,0))", "E6");
                    SheetMCP.Cells[Getcell(allColumns.IndexOf("SlsName")) + "6"].SetSharedFormula(formulaSalesName, numberRow, 1);// (dtCustomer.Rows.Count + 6)
                }

                string formulaRoute = string.Format("=IF(ISERROR(VLOOKUP({0},CA:CC,2,0)),\"\",VLOOKUP({0},CA:CC,2,0))", "R6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("RouteName")) + "6"].SetSharedFormula(formulaRoute, numberRow, 1);// (dtCustomer.Rows.Count + 6)


                string formulaSTT = "=IFERROR( IF(B6<>\"\",A5+1 & \"\",\"\"),1)";
                SheetMCP.Cells["A6"].SetSharedFormula(formulaSTT, numberRow, 1);// (dtCustomer.Rows.Count + 6)
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
                string messgeWeekOfVistit = string.Empty;
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
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
                        var lstWeekOfVisit = _db.OM23800_piWeekofVisit(Current.UserName, Current.CpnyID, Current.LangID).ToList();
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
                     //   string strtmpError = "";
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
                              //  bool isNew = false;
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

                                            if (!lstWeekOfVisit.Any(x => x.SlsFreq == objImport.SlsFreq && x.WeekofVisit == objImport.WeekofVisit))
                                            {
                                                messgeWeekOfVistit += (i + 1).ToString() + ",";
                                            }
                                            else
                                            {
                                                if (workSheet.Cells[i, 20].StringValue != null && workSheet.Cells[i, 20].StringValue == "X")
                                                {
                                                    objImport.Del = true;

                                                }
                                                _db.OM_SalesRouteMasterImport.AddObject(objImport);
                                                lstOM_SalesRouteMasterImport.Add(objImport);
                                            }
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
                        message += messgeWeekOfVistit == "" ? "" : string.Format(Message.GetString("2017040301", null), messgeWeekOfVistit);
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
        #endregion

        #region -Customer delete ?-
        //[HttpPost]
        //public ActionResult ExportCust(FormCollection data, string[] provinces, string provinceRawValue, string slsperID, string channel, string territory, bool isUpdated)
        //{
        //    try
        //    {
        //        string branchID = data["branchID"].PassNull();
        //        string branchName = data["branchName"].PassNull();
        //        string[] provincesDescr = provinceRawValue.Contains(", ") 
        //            ? provinceRawValue.Split(new string[] { ", " }, StringSplitOptions.None) 
        //            : new string[] { provinceRawValue };

        //        var headerRowIdx = 3;
        //        var maxRow = 1000;
        //        var ColTexts = new List<string>() { "N0", "SlsperID", "SlsName", "ShopID", "ShopName", "Attn", "StreetNo", "StreetName", "Ward", "Territory", "Province", "District", "Phone", "Channel", "CustClass", "Chain", "ShopType", "Classification", "Location", "Latitude", "Longitude", "RefCustID", "InActive" };

        //        Stream stream = new MemoryStream();
        //        Workbook workbook = new Workbook();
        //        Worksheet SheetMCP = workbook.Worksheets[0];
        //        SheetMCP.Name = Util.GetLang("Customer");
        //        DataAccess dal = Util.Dal();
        //        Style style = workbook.GetStyleInPool(0);
        //        StyleFlag flag = new StyleFlag();
        //        Range range;
        //        //Cell cell;


        //        #region master data
        //        ParamCollection pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

        //        DataTable dtSlsperMaster = dal.ExecDataTable("OM23800_peSlsperIDCust", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtSlsperMaster, true, 0, 26, false);// du lieu Slsperson      AA          

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",",provinces)), ParameterDirection.Input, int.MaxValue));

        //        DataTable dtDistrict = dal.ExecDataTable("OM23800_peDistrictCust", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtDistrict, true, 0, 52, false);// du lieu District BA

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

        //        DataTable dtCustClass = dal.ExecDataTable("OM23800_peCustClassCust", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtCustClass, true, 0, 78, false);// du lieu SalesRoute CA
                
        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",", provinces)), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
        //        DataTable dtState = dal.ExecDataTable("OM23800_peExportState", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtState, true, 0, 104, false);// du lieu SellProduct => đổi thành Location FA

        //        //for (var i = 0; i < dtState.Rows.Count; i++)
        //        //{
        //        //    SheetMCP.Cells["DA" + (i + 1).ToString()].PutValue(dtState.Rows[i]["State"].ToString());
        //        //    SheetMCP.Cells["DB" + (i + 1).ToString()].PutValue(provinces[i] + " - " + provincesDescr[i]);
        //        //}

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

        //        DataTable dtShopType = dal.ExecDataTable("OM23800_peShopType", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtShopType, true, 0, 130, false);// du lieu ShopType EA

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
        //        DataTable dtLocation = dal.ExecDataTable("OM23800_peAR_Location", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtLocation, true, 0, 156, false);// du lieu SellProduct => đổi thành Location FA

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",", provinces)), ParameterDirection.Input, int.MaxValue));
        //        pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
        //        DataTable dtTerritory = dal.ExecDataTable("OM23800_peExportDataTerritory", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtTerritory, true, 0, 286, false);// du lieu Territory KA

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
        //        DataTable dtChannel = dal.ExecDataTable("OM23800_peChannel", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtChannel, true, 0, 260, false);// du lieu Channel GD

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
        //        DataTable dtChain = dal.ExecDataTable("OM23800_peCustomerChain", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtChain, true, 0, 208, false);// du lieu Customer Chain HA

        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
        //        DataTable dtClassification = dal.ExecDataTable("OM23800_peShopClassification", CommandType.StoredProcedure, ref pc);
        //        SheetMCP.Cells.ImportDataTable(dtClassification, true, 0, 234, false);// du lieu Classification IA
                
        //        SheetMCP.Cells["GA1"].PutValue("X");
        //        #endregion

        //        #region header info
        //        // Title header
        //        SetCellValue(SheetMCP.Cells["B1"],
        //            string.Format("{0}", Util.GetLang("OM23800EHeaderCust")),
        //            TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
        //        SheetMCP.Cells.Merge(0, 1, 1, ColTexts.Count - 2);

        //        // Title info
        //        SetCellValue(SheetMCP.Cells["B2"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
        //        SetCellValue(SheetMCP.Cells["B3"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
        //        SetCellValue(SheetMCP.Cells["C2"], branchID, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);
        //        SetCellValue(SheetMCP.Cells["C3"], branchName, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);

        //        // Header text columns
        //        for (int i = 0; i < ColTexts.Count; i++)
        //        {
        //            var colIdx = i;
        //            if (ColTexts[i] == "Location")
        //            {
        //                SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang("OM23800CustLocation"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true);
        //            }
        //            else if (ColTexts[i] == "CustClass")
        //            {
        //                SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang("OM23800CustClass"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true);
        //            }
        //            else
        //            {
        //                SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(ColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
        //            }
        //            SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
        //        }
        //        #endregion

        //        #region export data
        //        pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",", provinces)), ParameterDirection.Input, int.MaxValue));
        //        pc.Add(new ParamStruct("@IsUpdated", DbType.String, clsCommon.GetValueDBNull(isUpdated), ParameterDirection.Input, 10));
        //        pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, int.MaxValue));
        //        pc.Add(new ParamStruct("@Channel", DbType.String, clsCommon.GetValueDBNull(channel), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@Territory", DbType.String, clsCommon.GetValueDBNull(territory), ParameterDirection.Input, 30));
        //        pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));

                
        //        DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportDataCust", CommandType.StoredProcedure, ref pc);
        //        //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

        //        for (int i = 0; i < dtDataExport.Rows.Count; i++)
        //        {
        //            for (int j = 0; j < ColTexts.Count; j++)
        //            {
        //                if (ColTexts[j] == "N0")
        //                {
        //                    SheetMCP.Cells[headerRowIdx + 2 + i, j].PutValue(i + 1);
        //                }
        //                else if (dtDataExport.Columns.Contains(ColTexts[j]))
        //                {
        //                    SheetMCP.Cells[headerRowIdx + 2 + i, j].PutValue(dtDataExport.Rows[i][ColTexts[j]]);
        //                }
        //            }
        //        }
        //        #endregion

        //        #region formular
        //        //SlsperID
        //        string formulaSlsper = "=$AA$2:$AA$" + (dtSlsperMaster.Rows.Count + 2);
        //        Validation validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaSlsper;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn mã nhân viên";
        //        validation.ErrorMessage = "Mã nhân viên này không tồn tại";

        //        CellArea area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("SlsperID");
        //        area.EndColumn = ColTexts.IndexOf("SlsperID");
        //        validation.AddArea(area);               

        //        // Zone
        //        string formulaTerritory = "=$KB$2:$KB$" + (dtTerritory.Rows.Count + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaTerritory;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn Zone";
        //        validation.ErrorMessage = "Zone này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("Territory");
        //        area.EndColumn = ColTexts.IndexOf("Territory");
        //        validation.AddArea(area);

        //        // State
        //        string formulaState = string.Format("=OFFSET($DB$1,IFERROR(MATCH(J{0},$DC$2:$DC${1},0),{2}),0, IF(COUNTIF($DC$2:$DC${1},J{0})=0,1,COUNTIF($DC$2:$DC${1},J{0})),1)", new string[] { "6", (dtState.Rows.Count + 1).ToString(), (dtState.Rows.Count + 2).ToString() }); ;//
        //        //string formulaState = "=$DB$2:$DB$" + (provinces.Length + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaState;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn Tỉnh";
        //        validation.ErrorMessage = "Mã Tỉnh này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("Province");
        //        area.EndColumn = ColTexts.IndexOf("Province");
        //        validation.AddArea(area);

        //        // District
        //        string formula = string.Format("=OFFSET($BB$2,IFERROR(MATCH(K{0},$BC$2:$BC${1},0),{2}),0, IF(COUNTIF($BC$2:$BC${1},K{0})=0,1,COUNTIF($BC$2:$BC${1},K{0})),1)", new string[] { "6", (dtDistrict.Rows.Count + 1).ToString(), (dtDistrict.Rows.Count + 2).ToString() });
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formula;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn quận huyện";
        //        validation.ErrorMessage = "Mã quận huyện không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("District");
        //        area.EndColumn = ColTexts.IndexOf("District");
        //        validation.AddArea(area);

        //        // Channel
        //        string formulaChannel = "=$JB$2:$JB$" + (dtChannel.Rows.Count + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaChannel;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn Kênh";
        //        validation.ErrorMessage = "Kênh này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("Channel");
        //        area.EndColumn = ColTexts.IndexOf("Channel");
        //        validation.AddArea(area);

        //        //Cust Class
        //        string formulaSales = "=$CB$2:$CB$" + (dtCustClass.Rows.Count + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaSales;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn nhóm khách hàng";
        //        validation.ErrorMessage = "Mã nhóm này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("CustClass");
        //        area.EndColumn = ColTexts.IndexOf("CustClass");
        //        validation.AddArea(area);

        //        // ShopType
        //        string formulaChain = "=$HB$2:$HB$" + (dtChain.Rows.Count + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaChain;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn Chuỗi";
        //        validation.ErrorMessage = "Chuỗi này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("Chain");
        //        area.EndColumn = ColTexts.IndexOf("Chain");
        //        validation.AddArea(area);

        //        // ShopType
        //        string formulaShopType = "=$EB$2:$EB$" + (dtShopType.Rows.Count + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaShopType;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn Loại cửa hàng";
        //        validation.ErrorMessage = "Loại cửa hàng này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("ShopType");
        //        area.EndColumn = ColTexts.IndexOf("ShopType");
        //        validation.AddArea(area);

        //        // Location
        //        string formulaClassification = "=$IB$2:$IB$" + (dtClassification.Rows.Count + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaClassification;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn Vị trí khách hàng";
        //        validation.ErrorMessage = "Vị trí khách hàng này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("Classification");
        //        area.EndColumn = ColTexts.IndexOf("Classification");
        //        validation.AddArea(area);

        //        // Location
        //        string formulaLocation = "=$FB$2:$FB$" + (dtLocation.Rows.Count + 2);
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaLocation;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn Vị trí khách hàng";
        //        validation.ErrorMessage = "Vị trí khách hàng này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("Location");
        //        area.EndColumn = ColTexts.IndexOf("Location");
        //        validation.AddArea(area);

        //        // InActive
        //        string formulaInActive = "=$GA$1:$GA$1";// "X";
        //        validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        validation.Formula1 = formulaInActive;
        //        validation.InputTitle = "";
        //        validation.InputMessage = "Chọn giá trị";
        //        validation.ErrorMessage = "Giá trị này không tồn tại";

        //        area = new CellArea();
        //        area.StartRow = headerRowIdx + 2;
        //        area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
        //        area.StartColumn = ColTexts.IndexOf("InActive");
        //        area.EndColumn = ColTexts.IndexOf("InActive");
        //        validation.AddArea(area);
        //        string formulaSlsName = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,2,0)),\"\",VLOOKUP({0},AA:AC,2,0))", "B6");
        //        SheetMCP.Cells[Getcell(ColTexts.IndexOf("SlsName")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaSlsName, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);

        //        //string formulaDistrictCode = string.Format("=IF(ISERROR(VLOOKUP({0},CHOOSE({{2,1}},$BA$1:$BA${1},$BB$1:$BB${1}),2,0)),\"\",VLOOKUP({0},CHOOSE({{2,1}},$BA$1:$BA${1},$BB$1:$BB${1}),2,0))", "M6", dtDistrict.Rows.Count + 1);
        //        //SheetMCP.Cells[Getcell(ColTexts.IndexOf("DistrictCode")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaDistrictCode, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);

        //        //string formulaProvinceCode = string.Format("=IF(ISERROR(VLOOKUP({0},CHOOSE({{2,1}},$DA$1:$DA${1},$DB$1:$DB${1}),2,0)),\"\",VLOOKUP({0},CHOOSE({{2,1}},$DA$1:$DA${1},$DB$1:$DB${1}),2,0))", "K6", provinces.Length);
        //        //SheetMCP.Cells[Getcell(ColTexts.IndexOf("ProvinceCode")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaProvinceCode, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);
        //        #endregion

        //        #region Fomat cell
        //        var strFirstRow = (headerRowIdx + 2).ToString();
        //        var strLastRow = (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2).ToString();

        //        style = SheetMCP.Cells["A" + strFirstRow].GetStyle();
        //        style.IsLocked = false;
        //        range = SheetMCP.Cells.CreateRange("A" + strFirstRow, 
        //            "A" + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("SlsperID")) + strFirstRow, 
        //            Getcell(ColTexts.IndexOf("SlsperID")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("ShopName")) + strFirstRow, 
        //            Getcell(ColTexts.IndexOf("ShopName")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Attn")) + strFirstRow, 
        //            Getcell(ColTexts.IndexOf("Attn")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("StreetNo")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("StreetNo")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("StreetName")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("StreetName")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Ward")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Ward")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Territory")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Territory")) + strLastRow);
        //        range.SetStyle(style);
       
        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Province")) + strFirstRow, 
        //            Getcell(ColTexts.IndexOf("Province")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("District")) + strFirstRow, 
        //            Getcell(ColTexts.IndexOf("District")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Phone")) + strFirstRow, 
        //            Getcell(ColTexts.IndexOf("Phone")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Channel")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Channel")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("CustClass")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("CustClass")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Chain")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Chain")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("ShopType")) + strFirstRow,
        //           Getcell(ColTexts.IndexOf("ShopType")) + strLastRow);
        //        range.SetStyle(style);
	
        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Classification")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Classification")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Location")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Location")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Latitude")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Latitude")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Longitude")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("Longitude")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("RefCustID")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("RefCustID")) + strLastRow);
        //        range.SetStyle(style);

        //        range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("InActive")) + strFirstRow,
        //            Getcell(ColTexts.IndexOf("InActive")) + strLastRow);
        //        range.SetStyle(style);

        //        style = SheetMCP.Cells["Z1"].GetStyle();
        //        style.Font.Color = Color.Transparent;
        //        style.IsLocked = false;
        //        flag.FontColor = true;
        //        flag.NumberFormat = true;
        //        flag.Locked = true;
        //        range = SheetMCP.Cells.CreateRange("Z1", "ZZ" + (dtSlsperMaster.Rows.Count + dtDistrict.Rows.Count + dtCustClass.Rows.Count + 100));
        //        range.ApplyStyle(style, flag);

        //        style = SheetMCP.Cells["O6"].GetStyle();
        //        style.Font.Color = Color.Red;
        //        style.IsLocked = false;
        //        flag.FontColor = true;
        //        range = SheetMCP.Cells.CreateRange("O6", "O" + (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2));
        //        range.ApplyStyle(style, flag);

        //        //style = SheetMCP.Cells["U6"].GetStyle();
        //        //style.Font.Color = Color.Red;
        //        //style.IsLocked = false;
        //        //flag.FontColor = true;
        //        range = SheetMCP.Cells.CreateRange("S6", "S" + (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2));
        //        range.ApplyStyle(style, flag);
               
        //        SheetMCP.Protect(ProtectionType.All);

        //        #endregion                
        //        SheetMCP.AutoFitColumns();

        //        workbook.Save(stream, SaveFormat.Xlsx);
        //        stream.Flush();
        //        stream.Position = 0;

        //        return new FileStreamResult(stream, "application/vnd.ms-excel")
        //        {
        //            FileDownloadName = string.Format("{0}_{1}.xlsx", Util.GetLang("OM23800"), Util.GetLang("Customer"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException)
        //        {
        //            return (ex as MessageException).ToMessage();
        //        }
        //        else
        //        {
        //            return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //        }
        //    }
        //}
        

        //[HttpPost]
        //public ActionResult ImportCust(FormCollection data, bool isUpdated)
        //{
        //    try
        //    {
        //        string BranchID = data["branchID"].PassNull();
        //        var dataRowIdx = 5;
        //        var date = DateTime.Now.Date;
        //        FileUploadField fileUploadField = X.GetCmp<FileUploadField>("fupImport_ImExCust");
        //        HttpPostedFile file = fileUploadField.PostedFile;
        //        FileInfo fileInfo = new FileInfo(file.FileName);
        //        string message = string.Empty;
        //        if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
        //        {                   
        //            #region -Variable Line error-                                        
        //            Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
        //            var lineSuccess = new List<string>();
        //            var lineBlank = new List<string>();
        //            var lineExistRefCustID = new List<string>();
        //            var lineExist = new List<string>();
        //            var lineNoExist = new List<string>();
        //            var lineSlsNoExist = new List<string>();
        //            var lineInvalidDistrict = new List<string>();
        //            var lineInvalidGeo = new List<string>();
        //            var lineCustID = new List<string>();
                    
        //            var lineSlsPerID = new List<string>();
        //            var lineShopID = new List<string>();
        //            var lineShopName = new List<string>();
        //            var lineAttn = new List<string>();
        //            var lineStreetNo = new List<string>();
        //            var lineStreetName = new List<string>();
        //            var lineWard = new List<string>();
        //            var lineProvince = new List<string>();
        //            var lineDistrict = new List<string>();
        //            var lineCustClass = new List<string>();

        //            var lineTerritory = new List<string>();
        //            var lineChannel = new List<string>();
        //            var lineClassID = new List<string>();
        //            var lineChain = new List<string>();
        //            var lineShopType = new List<string>();
        //            var lineClassification = new List<string>();
        //            var lineLocation = new List<string>();


        //            var lineTerritoryNotExist = new List<string>();
        //            var lineChannelNotExist = new List<string>();
        //            var lineCustClassNotExist = new List<string>();
        //            var lineChainNotExist = new List<string>();
        //            var lineShopTypeNotExist = new List<string>();
        //            var lineClassificationNotExist = new List<string>();
        //            var lineLocationNotExist = new List<string>();
        //            var flagCheckError = false;
        //            #endregion

        //            if (workbook.Worksheets.Count > 0)
        //            {
        //                #region -Variable data-                                               
        //                Worksheet workSheet = workbook.Worksheets[0];
        //                string strEBranchID = workSheet.Cells[1, 2].StringValue.Trim();
        //                string strSlsPerID = string.Empty;
        //                string strShopID = string.Empty;
        //                string strShopName = string.Empty;
        //                string strAttn = string.Empty;
        //                string strDistrict = string.Empty;
        //                string strProvince = string.Empty;
        //                string strPhone = string.Empty;
        //                string strCustClass = string.Empty;
        //                string strRefCustID = string.Empty;
        //                string strShopType = string.Empty;
        //                string strLocation = string.Empty;
        //                string strCountry = string.Empty;
        //                string strCity = string.Empty;
        //                string strInActive = string.Empty;
                        
        //                string Channel = string.Empty;
        //                string Chain = string.Empty;
        //                string Classification = string.Empty;
        //                string StreetNo = string.Empty;
        //                string StreetName = string.Empty;
        //                string Ward = string.Empty;
        //                string Territory = string.Empty;

        //                bool autoCustID = true;
        //                double lat = 0;
        //                double lng = 0;

        //                var lstTerritory = _db.OM23800_peExportDataTerritory(Current.UserName, Current.CpnyID, Current.LangID).ToList();
        //                var lstChannel = _db.OM23800_peChannel(Current.UserName, Current.CpnyID, Current.LangID).ToList();
        //                var lstCustClass = _db.OM23800_peCustClassCust(Current.UserName, Current.CpnyID, Current.LangID).ToList();
        //                var lstChain = _db.OM23800_peCustomerChain(Current.UserName, Current.CpnyID, Current.LangID).ToList();
        //                var lstShopType = _db.OM23800_peShopType(BranchID, Current.UserName).ToList();
        //                var lstClassification = _db.OM23800_peShopClassification(Current.UserName, Current.CpnyID, Current.LangID).ToList();
        //                var lstLocation = _db.OM23800_peAR_Location(BranchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();

        //                #endregion
                        
        //                if (strEBranchID == BranchID)
        //                {
        //                    var objAR_Setup = _db.AR_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupId == "AR");
        //                    if (objAR_Setup != null) { 
        //                        autoCustID = objAR_Setup.AutoCustID;
        //                    }
        //                    for (int i = dataRowIdx; i <= workSheet.Cells.MaxDataRow; i++)
        //                    {
        //                        #region -Get data from excel-                                                                
        //                        //if (workSheet.Cells[i,0].StringValue.PassNull().Trim() == "") break;
        //                        strSlsPerID = workSheet.Cells[i, 1].StringValue.Trim();
        //                        strShopID = workSheet.Cells[i, 3].StringValue.Trim();
        //                        strShopName = workSheet.Cells[i, 4].StringValue.Trim();
        //                        strAttn = workSheet.Cells[i, 5].StringValue.Trim();
        //                        StreetNo = workSheet.Cells[i, 6].StringValue.Trim();
        //                        StreetName = workSheet.Cells[i, 7].StringValue.Trim();
        //                        Ward = workSheet.Cells[i, 8].StringValue.Trim();
        //                        Territory = GetCodeFromExcel(workSheet.Cells[i, 9].StringValue.Trim());
        //                        strProvince = GetCodeFromExcel(workSheet.Cells[i, 10].StringValue.Trim());
        //                        strDistrict = GetCodeFromExcel(workSheet.Cells[i, 11].StringValue.Trim());
                                
        //                        strPhone = workSheet.Cells[i, 12].StringValue.Trim();

        //                        Channel = GetCodeFromExcel(workSheet.Cells[i, 13].StringValue.Trim());
        //                        strCustClass = GetCodeFromExcel(workSheet.Cells[i, 14].StringValue.Trim());
        //                        Chain = GetCodeFromExcel(workSheet.Cells[i, 15].StringValue.Trim());
        //                        strShopType = GetCodeFromExcel(workSheet.Cells[i, 16].StringValue.Trim());
        //                        Classification = GetCodeFromExcel(workSheet.Cells[i, 17].StringValue.Trim());
        //                        strLocation = GetCodeFromExcel(workSheet.Cells[i, 18].StringValue.Trim());

        //                        double.TryParse(workSheet.Cells[i, 19].StringValue.Trim(), out lat); //13 
        //                        double.TryParse(workSheet.Cells[i, 20].StringValue.Trim(), out lng); //14 
        //                        strRefCustID = workSheet.Cells[i, 21].StringValue.Trim();
        //                        strInActive = workSheet.Cells[i, 22].StringValue.Trim();
        //                        flagCheckError = false;
        //                        #endregion

        //                        // Continue when row is empty
        //                        //if (_checkRequireImport == false)
        //                        //{
        //                        if (strSlsPerID == "" && strShopName == "" && strAttn == "" && strShopID == ""
        //                                && StreetNo == "" && StreetName == "" && Ward == "" && Territory == "" && strProvince == "" && strDistrict == ""
        //                                && Channel == "" && strCustClass == "" && Chain == "" && strShopType == "" && Classification == ""
        //                                && strLocation == "")
        //                        {
        //                            continue;
        //                        }
        //                        //}
        //                        //else
        //                        //{
        //                        //    if (strSlsPerID == "" && strShopID == "" && strShopName == "" && strAttn == ""
        //                        //        && StreetNo == "" && strProvince == "" && strDistrict == "" && strCustClass == "")
        //                        //        continue;

        //                        //    //if (strShopType == "")
        //                        //    //{
        //                        //    //    lineShopType.Add((i - dataRowIdx + 1).ToString());
        //                        //    //    flagCheckError = true;
        //                        //    //}
        //                        //}
        //                        #region -Check required field-                                                                
        //                        if (strSlsPerID == "")
        //                        {
        //                            lineSlsPerID.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }                                
        //                        if (strShopName == "")
        //                        {
        //                            lineShopName.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (strAttn == "")
        //                        {
        //                            lineAttn.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (StreetNo == "")
        //                        {
        //                            lineStreetNo.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (StreetName == "")
        //                        {
        //                            lineStreetName.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (Ward == "")
        //                        {
        //                            lineWard.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (strProvince == "")
        //                        {
        //                            lineProvince.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (strDistrict == "")
        //                        {
        //                            lineDistrict.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (Territory == "")
        //                        {
        //                            lineTerritory.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (Channel == "")
        //                        {
        //                            lineChannel.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (strCustClass == "")
        //                        {
        //                            lineCustClass.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (Chain == "")
        //                        {
        //                            lineChain.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (strShopType == "")
        //                        {
        //                            lineShopType.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (Classification == "")
        //                        {
        //                            lineClassification.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        if (strLocation == "")
        //                        {
        //                            lineLocation.Add((i - dataRowIdx + 1).ToString());
        //                            flagCheckError = true;
        //                        }
        //                        #endregion
                                
        //                        if (flagCheckError == false)
        //                        {
        //                            if (!string.IsNullOrWhiteSpace(strSlsPerID)
        //                                || !string.IsNullOrWhiteSpace(strShopID)
        //                                || !string.IsNullOrWhiteSpace(strShopName)
        //                                || !string.IsNullOrWhiteSpace(strAttn)
        //                                || !string.IsNullOrWhiteSpace(StreetNo)
        //                                || !string.IsNullOrWhiteSpace(StreetName)
        //                                || !string.IsNullOrWhiteSpace(Ward)
        //                                || !string.IsNullOrWhiteSpace(Territory)
        //                                || !string.IsNullOrWhiteSpace(strDistrict)
        //                                || !string.IsNullOrWhiteSpace(strProvince)
        //                                || !string.IsNullOrWhiteSpace(strPhone)
        //                                || !string.IsNullOrWhiteSpace(Channel)
        //                                || !string.IsNullOrWhiteSpace(strCustClass)
        //                                || !string.IsNullOrWhiteSpace(Chain)
        //                                || !string.IsNullOrWhiteSpace(strShopType)
        //                                || !string.IsNullOrWhiteSpace(Classification)
        //                                || !string.IsNullOrWhiteSpace(strLocation)
        //                                || lat > 0 || lng > 0)
        //                            {
        //                                var slsright = true;
        //                                if (!string.IsNullOrWhiteSpace(strSlsPerID))
        //                                {
        //                                    var slsper = _db.AR_Salesperson.FirstOrDefault(s => s.SlsperId == strSlsPerID && s.BranchID == strEBranchID);
        //                                    if (slsper == null)
        //                                    {
        //                                        lineSlsNoExist.Add((i - dataRowIdx + 1).ToString());
        //                                        slsright = false;
        //                                    }
        //                                }

        //                                if (!string.IsNullOrWhiteSpace(Territory))
        //                                {
        //                                    if (lstTerritory.FirstOrDefault(x => x.Code == Territory) == null)
        //                                    {
        //                                        lineTerritoryNotExist.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                if (!string.IsNullOrWhiteSpace(strDistrict))
        //                                {
        //                                    var district = _db.SI_District.FirstOrDefault(d => d.District == strDistrict && d.State == strProvince);
        //                                    if (district == null)
        //                                    {
        //                                        lineInvalidDistrict.Add((i - dataRowIdx + 1).ToString());
        //                                        slsright = false;
        //                                    }
        //                                    else
        //                                    {
        //                                        strCountry = district.Country;
        //                                        //var state = _db.SI_State.FirstOrDefault(s => s.Country == strCountry && s.State == strProvince);
        //                                        //if (state != null)
        //                                        //{
        //                                        //    Territory = state.Territory;
        //                                        //}
        //                                        var city = _db.SI_City.FirstOrDefault(c => c.Country == strCountry && c.State == strProvince);
        //                                        if (city != null)
        //                                        {
        //                                            strCity = city.City;
        //                                        }
        //                                    }
        //                                }
        //                                if (!string.IsNullOrWhiteSpace(Channel))
        //                                {
        //                                    if (lstChannel.FirstOrDefault(x => x.Code == Channel) == null)
        //                                    {
        //                                        lineChannelNotExist.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                if (!string.IsNullOrWhiteSpace(strCustClass))
        //                                {
        //                                    if (lstCustClass.FirstOrDefault(x => x.Code == strCustClass) == null)
        //                                    {
        //                                        lineCustClassNotExist.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                if (!string.IsNullOrWhiteSpace(Chain))
        //                                {
        //                                    if (lstChain.FirstOrDefault(x => x.Code == Chain) == null)
        //                                    {
        //                                        lineChainNotExist.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                if (!string.IsNullOrWhiteSpace(strShopType))
        //                                {
        //                                    if (lstShopType.FirstOrDefault(x => x.Code == strShopType) == null)
        //                                    {
        //                                        lineShopTypeNotExist.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                if (!string.IsNullOrWhiteSpace(Classification))
        //                                {
        //                                    if (lstClassification.FirstOrDefault(x => x.Code == Classification) == null)
        //                                    {
        //                                        lineClassificationNotExist.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                if (!string.IsNullOrWhiteSpace(strLocation))
        //                                {
        //                                    if (lstLocation.FirstOrDefault(x => x.Code == strLocation) == null)
        //                                    {
        //                                        lineLocationNotExist.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                if (isUpdated)
        //                                {
        //                                    if (!string.IsNullOrWhiteSpace(strShopID)
        //                                        && !string.IsNullOrWhiteSpace(strShopName)
        //                                        && !string.IsNullOrWhiteSpace(Territory)
        //                                        && !string.IsNullOrWhiteSpace(strDistrict)
        //                                        && !string.IsNullOrWhiteSpace(strProvince)
        //                                        && !string.IsNullOrWhiteSpace(Channel)                                                
        //                                        && !string.IsNullOrWhiteSpace(strCustClass)
        //                                        && !string.IsNullOrWhiteSpace(Chain)
        //                                        && !string.IsNullOrWhiteSpace(strShopType)
        //                                        && !string.IsNullOrWhiteSpace(Classification)
        //                                        && !string.IsNullOrWhiteSpace(strLocation)
        //                                        )
        //                                    {
        //                                        //if (_checkRequireImport == true && string.IsNullOrWhiteSpace(strShopType)) // Luôn ktra shoptype
        //                                        //    continue;
        //                                        if (slsright)
        //                                        {
        //                                            var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustId == strShopID && c.BranchID == strEBranchID);
        //                                            if (existCust != null)
        //                                            {
        //                                                if (existCust.RefCustID != strRefCustID && strRefCustID.PassNull()!="")
        //                                                {
        //                                                    var existRefCustID = _db.AR_Customer.FirstOrDefault(p => p.BranchID == strEBranchID && p.RefCustID == strRefCustID);
        //                                                    if (existRefCustID == null)
        //                                                    {
        //                                                        existCust.CustName = existCust.BillName = strShopName;
        //                                                        existCust.Attn = existCust.BillAttn = strAttn;
        //                                                        existCust.Addr1 = existCust.BillAddr1 = StreetName;
        //                                                        existCust.Addr2 = existCust.BillAddr2 = StreetNo;
        //                                                        existCust.Ward = Ward;
        //                                                        existCust.District = strDistrict;
        //                                                        existCust.State = existCust.BillState = strProvince;
        //                                                        existCust.Phone = existCust.BillPhone = strPhone;
        //                                                        existCust.Country = existCust.BillCountry = strCountry;
        //                                                        existCust.City = existCust.BillCity = strCity;
        //                                                        existCust.Territory = Territory;
        //                                                        existCust.Channel = Channel;                                                                
        //                                                        existCust.ClassId = strCustClass;
        //                                                        existCust.Chain = Chain;
        //                                                        existCust.ShopType = strShopType;
        //                                                        existCust.Classification = Classification;
        //                                                        existCust.Location = strLocation;                                                                

        //                                                        existCust.LUpd_Datetime = DateTime.Now;
        //                                                        existCust.LUpd_Prog = _screenName;
        //                                                        existCust.LUpd_User = Current.UserName;
        //                                                        existCust.SlsperId = strSlsPerID.PassNull();
                                                               
        //                                                        existCust.SellProduct = "";
        //                                                        existCust.RefCustID = strRefCustID;
        //                                                        if (strInActive == "X")
        //                                                            existCust.Status = inActiveStatus;
        //                                                        else
        //                                                            existCust.Status = activeStatus;
        //                                                        if (lat > 0 && lng > 0)
        //                                                        {
        //                                                            updateCustomerLocation(strEBranchID, strShopID, lat, lng);
        //                                                        }
        //                                                        else if (lat > 0 || lng > 0)
        //                                                        {
        //                                                            lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
        //                                                        }
        //                                                        lineSuccess.Add((i - dataRowIdx + 1).ToString());
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        lineExistRefCustID.Add((i - dataRowIdx + 1).ToString());
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    existCust.CustName = existCust.BillName = strShopName;
        //                                                    existCust.Attn = existCust.BillAttn = strAttn;
        //                                                    existCust.Addr1 = existCust.BillAddr1 = StreetName;
        //                                                    existCust.Addr2 = existCust.BillAddr2 = StreetNo;
        //                                                    existCust.Ward = Ward;
        //                                                    existCust.District = strDistrict;
        //                                                    existCust.State = existCust.BillState = strProvince;
        //                                                    existCust.Phone = existCust.BillPhone = strPhone;
        //                                                    existCust.Country = existCust.BillCountry = strCountry;
        //                                                    existCust.City = existCust.BillCity = strCity;

        //                                                    existCust.Territory = Territory;
        //                                                    existCust.Channel = Channel;
        //                                                    existCust.ClassId = strCustClass;
        //                                                    existCust.Chain = Chain;
        //                                                    existCust.ShopType = strShopType;
        //                                                    existCust.Classification = Classification;
        //                                                    existCust.Location = strLocation;

        //                                                    existCust.LUpd_Datetime = DateTime.Now;
        //                                                    existCust.LUpd_Prog = _screenName;
        //                                                    existCust.LUpd_User = Current.UserName;
        //                                                    existCust.SlsperId = strSlsPerID.PassNull();
        //                                                    existCust.ShopType = strShopType;
        //                                                    existCust.SellProduct =  "";
                                                            
        //                                                    existCust.RefCustID = strRefCustID;
        //                                                    if (strInActive == "X")
        //                                                        existCust.Status = inActiveStatus;
        //                                                    else
        //                                                        existCust.Status = activeStatus;

        //                                                    if (lat > 0 && lng > 0)
        //                                                    {
        //                                                        updateCustomerLocation(strEBranchID, strShopID, lat, lng);
        //                                                    }
        //                                                    else if (lat > 0 || lng > 0)
        //                                                    {
        //                                                        lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
        //                                                    }

        //                                                    lineSuccess.Add((i - dataRowIdx + 1).ToString());
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                lineNoExist.Add((i - dataRowIdx + 1).ToString());
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        //lineBlank.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (!string.IsNullOrWhiteSpace(strShopName)                                                
        //                                        && !string.IsNullOrWhiteSpace(Territory)
        //                                        && !string.IsNullOrWhiteSpace(strDistrict)
        //                                        && !string.IsNullOrWhiteSpace(strProvince)
        //                                        && !string.IsNullOrWhiteSpace(Channel)
        //                                        && !string.IsNullOrWhiteSpace(strCustClass)
        //                                        && !string.IsNullOrWhiteSpace(Chain)
        //                                        && !string.IsNullOrWhiteSpace(strShopType)
        //                                        && !string.IsNullOrWhiteSpace(Classification)
        //                                        && !string.IsNullOrWhiteSpace(strLocation)
        //                                        )
        //                                    {
        //                                        if (_checkRequireImport == true && string.IsNullOrWhiteSpace(strShopType))
        //                                            continue;
        //                                        if (slsright)
        //                                        {
        //                                            var canInsert = true;
        //                                            if (!string.IsNullOrWhiteSpace(strShopID))
        //                                            {
        //                                                var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustId == strShopID && c.BranchID == strEBranchID);
        //                                                if (existCust != null)
        //                                                {
        //                                                    lineExist.Add((i - dataRowIdx + 1).ToString());
        //                                                    canInsert = false;
        //                                                }
        //                                                else
        //                                                {
        //                                                    if(autoCustID == true)
        //                                                        strShopID = _db.OM23800_CustID(strEBranchID, "", Territory, strDistrict, Channel, Chain, "", "", "", "", strCustClass, strProvince, strShopName).FirstOrDefault().ToString();
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustName == strShopName 
        //                                                                        && c.Addr1 == StreetName
        //                                                                        && c.Addr2 == StreetNo
        //                                                                        && c.Ward == Ward
        //                                                                        && c.District == strDistrict
        //                                                                        && c.State == strProvince
        //                                                                        && c.BranchID == strEBranchID);
        //                                                if (existCust != null)
        //                                                {
        //                                                    lineExist.Add((i - dataRowIdx + 1).ToString());
        //                                                    canInsert = false;
        //                                                }
        //                                                else
        //                                                {
        //                                                    if (autoCustID == true)
        //                                                    {
        //                                                        strShopID = _db.OM23800_CustID(strEBranchID, "", Territory, strDistrict, Channel, Chain, "", "", "", "", strCustClass, strProvince, strShopName).FirstOrDefault().ToString();
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        lineCustID.Add((i - dataRowIdx + 1).ToString());
        //                                                        canInsert = false;
        //                                                    }
        //                                                        // bao loi thieu custid

        //                                                }
        //                                            }
        //                                            if (canInsert)
        //                                            {
        //                                                if (strRefCustID.PassNull() != "")
        //                                                {
        //                                                    var existRefCustID = _db.AR_Customer.FirstOrDefault(p => p.BranchID == strEBranchID && p.RefCustID == strRefCustID);
        //                                                    if (existRefCustID != null)
        //                                                    {
        //                                                        lineExistRefCustID.Add((i - dataRowIdx + 1).ToString());
        //                                                        canInsert = false;
        //                                                    }
        //                                                }
        //                                            }
        //                                            if (canInsert)
        //                                            {
        //                                                #region -Insert new Cust-                                                                                                                
        //                                                var newCust = new AR_Customer();
        //                                                newCust.ResetET();
        //                                                newCust.ExpiryDate = DateTime.Now.ToDateShort();
        //                                                newCust.Birthdate = new DateTime(1900, 1, 1).ToDateShort();
        //                                                newCust.EstablishDate = new DateTime(1900, 1, 1).ToDateShort();
        //                                                newCust.CustId = strShopID;
        //                                                newCust.BranchID = strEBranchID;
        //                                                newCust.CustName = newCust.BillName = strShopName;
        //                                                newCust.Attn = newCust.BillAttn = strAttn;
        //                                                newCust.Addr1 = newCust.BillAddr1 = StreetName;
        //                                                newCust.Addr2 = newCust.BillAddr2 = StreetNo;
        //                                                newCust.District = strDistrict;
        //                                                newCust.State = newCust.BillState = strProvince;
        //                                                newCust.Phone = newCust.BillPhone = strPhone;
        //                                                newCust.Country = newCust.BillCountry = strCountry;
        //                                                newCust.City = newCust.BillCity = strCity;
        //                                                newCust.Territory = Territory;
        //                                                newCust.ClassId = strCustClass;
        //                                                newCust.Location = strLocation;
        //                                                newCust.CrRule = "N";
        //                                                newCust.CustType = "R";
        //                                                newCust.DfltShipToId = "DEFAULT";
        //                                                newCust.NodeLevel = 2;
        //                                                newCust.ParentRecordID = 4;
        //                                                if (strInActive == "X")
        //                                                    newCust.Status = inActiveStatus;
        //                                                else
        //                                                    newCust.Status = activeStatus;
        //                                                newCust.SupID = "";
        //                                                newCust.TaxDflt = "C";
        //                                                newCust.TaxID00 = "OVAT10-00";
        //                                                newCust.TaxID01 = "OVAT05-00";
        //                                                newCust.TaxID02 = "VAT00";
        //                                                newCust.TaxID03 = "NONEVAT";
        //                                                newCust.TaxLocId = "";
        //                                                newCust.TaxRegNbr = "123456789";
        //                                                newCust.Terms = "07";
        //                                                newCust.LUpd_Datetime = newCust.Crtd_Datetime = DateTime.Now;
        //                                                newCust.LUpd_Prog = newCust.Crtd_Prog = _screenName;
        //                                                newCust.LUpd_User = newCust.Crtd_User = Current.UserName;
        //                                                newCust.SlsperId = strSlsPerID;
        //                                                newCust.ShopType = strShopType;
        //                                                newCust.SellProduct = "";
        //                                                newCust.Location = strLocation;
        //                                                newCust.RefCustID = strRefCustID;

        //                                                newCust.Ward = Ward;
        //                                                newCust.Territory = Territory;
        //                                                newCust.Channel = Channel;
        //                                                newCust.ClassId = strCustClass;
        //                                                newCust.Chain = Chain;
        //                                                newCust.ShopType = strShopType;
        //                                                newCust.Classification = Classification;
        //                                                newCust.Location = strLocation;

        //                                                _db.AR_Customer.AddObject(newCust);

        //                                                if (lat > 0 && lng > 0)
        //                                                {
        //                                                    updateCustomerLocation(strEBranchID, strShopID, lat, lng);
        //                                                }
        //                                                else if (lat > 0 || lng > 0)
        //                                                {
        //                                                    lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
        //                                                }

        //                                                _db.SaveChanges();
        //                                                lineSuccess.Add((i - dataRowIdx + 1).ToString());
        //                                                #endregion
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        //lineBlank.Add((i - dataRowIdx + 1).ToString());
        //                                    }
        //                                }
        //                            }
        //                        }// flagCheckError = false
        //                    }
        //                    if (isUpdated)
        //                    {
        //                        _db.SaveChanges();
        //                    }
        //                    var numOfLine = 10;
        //                    #region -Error-                                                        
        //                    if (lineSuccess.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082907",null),
        //                            lineSuccess.Count > numOfLine ? string.Join(", ", lineSuccess.Take(numOfLine)) + ", ..." : string.Join(", ", lineSuccess));
        //                    }
        //                    if (lineCustID.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineCustID.Count > numOfLine ? string.Join(", ", lineCustID.Take(numOfLine)) + ", ..." : string.Join(", ", lineCustID), workSheet.Cells[3, 3].StringValue);
        //                    }
        //                    if (lineSlsPerID.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineSlsPerID.Count > numOfLine ? string.Join(", ", lineSlsPerID.Take(numOfLine)) + ", ..." : string.Join(", ", lineSlsPerID), workSheet.Cells[3, 1].StringValue);
        //                    }
        //                    if (lineShopID.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineShopID.Count > numOfLine ? string.Join(", ", lineShopID.Take(numOfLine)) + ", ..." : string.Join(", ", lineShopID), workSheet.Cells[3, 3].StringValue);
        //                    }
        //                    if (lineShopName.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineShopName.Count > numOfLine ? string.Join(", ", lineShopName.Take(numOfLine)) + ", ..." : string.Join(", ", lineShopName), workSheet.Cells[3, 4].StringValue);
        //                    }
        //                    if (lineAttn.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineAttn.Count > numOfLine ? string.Join(", ", lineAttn.Take(numOfLine)) + ", ..." : string.Join(", ", lineAttn), workSheet.Cells[3, 5].StringValue);
        //                    }
        //                    if (lineStreetNo.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineStreetNo.Count > numOfLine ? string.Join(", ", lineStreetNo.Take(numOfLine)) + ", ..." : string.Join(", ", lineStreetNo), workSheet.Cells[3, 6].StringValue);
        //                    }
        //                    if (lineStreetName.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineStreetName.Count > numOfLine ? string.Join(", ", lineStreetName.Take(numOfLine)) + ", ..." : string.Join(", ", lineStreetName), workSheet.Cells[3, 7].StringValue);
        //                    }
        //                    if (lineWard.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineWard.Count > numOfLine ? string.Join(", ", lineWard.Take(numOfLine)) + ", ..." : string.Join(", ", lineWard), workSheet.Cells[3, 8].StringValue);
        //                    }
        //                    if (lineTerritory.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineTerritory.Count > numOfLine ? string.Join(", ", lineTerritory.Take(numOfLine)) + ", ..." : string.Join(", ", lineTerritory), workSheet.Cells[3, 9].StringValue);
        //                    }
        //                    if (lineProvince.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineProvince.Count > numOfLine ? string.Join(", ", lineProvince.Take(numOfLine)) + ", ..." : string.Join(", ", lineProvince), workSheet.Cells[3, 10].StringValue);
        //                    }
        //                    if (lineDistrict.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineDistrict.Count > numOfLine ? string.Join(", ", lineDistrict.Take(numOfLine)) + ", ..." : string.Join(", ", lineDistrict), workSheet.Cells[3, 11].StringValue);
        //                    }
        //                    if (lineChannel.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineChannel.Count > numOfLine ? string.Join(", ", lineChannel.Take(numOfLine)) + ", ..." : string.Join(", ", lineChannel), workSheet.Cells[3, 13].StringValue);
        //                    }
        //                    if (lineCustClass.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineCustClass.Count > numOfLine ? string.Join(", ", lineCustClass.Take(numOfLine)) + ", ..." : string.Join(", ", lineCustClass), workSheet.Cells[3, 14].StringValue);
        //                    }
        //                    if (lineChain.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineChain.Count > numOfLine ? string.Join(", ", lineChain.Take(numOfLine)) + ", ..." : string.Join(", ", lineChain), workSheet.Cells[3, 15].StringValue);
        //                    }
        //                    if (lineShopType.Count > 0 && _checkRequireImport == true)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineShopType.Count > numOfLine ? string.Join(", ", lineShopType.Take(numOfLine)) + ", ..." : string.Join(", ", lineShopType), workSheet.Cells[3, 16].StringValue);
        //                    }
        //                    if (lineClassification.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineClassification.Count > numOfLine ? string.Join(", ", lineClassification.Take(numOfLine)) + ", ..." : string.Join(", ", lineClassification), workSheet.Cells[3, 17].StringValue);
        //                    }
        //                    if (lineLocation.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082912", null),
        //                            lineLocation.Count > numOfLine ? string.Join(", ", lineLocation.Take(numOfLine)) + ", ..." : string.Join(", ", lineLocation), workSheet.Cells[3, 18].StringValue);
        //                    }

        //                    if (lineExist.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082908", null),
        //                            lineExist.Count > numOfLine ? string.Join(", ", lineExist.Take(numOfLine)) + ", ..." : string.Join(", ", lineExist));
        //                    }
        //                    if (lineNoExist.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082909", null),
        //                            lineNoExist.Count > numOfLine ? string.Join(", ", lineNoExist.Take(numOfLine)) + ", ..." : string.Join(", ", lineNoExist));
        //                    }
        //                    if (lineSlsNoExist.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082910", null),
        //                            lineSlsNoExist.Count > numOfLine ? string.Join(", ", lineSlsNoExist.Take(numOfLine)) + ", ..." : string.Join(", ", lineSlsNoExist));
        //                    }

        //                    // 2016091413 {0} Line: {1} have not exists</br>

        //                    if (lineExistRefCustID.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016090701", null),
        //                            lineExistRefCustID.Count > numOfLine ? string.Join(", ", lineExistRefCustID.Take(numOfLine)) + ", ..." : string.Join(", ", lineExistRefCustID));
        //                    }
        //                    if (lineInvalidDistrict.Count > 0)
        //                    {
        //                        message += string.Format(Message.GetString("2016082911", null),
        //                            lineInvalidDistrict.Count > numOfLine ? string.Join(", ", lineInvalidDistrict.Take(numOfLine)) + ", ..." : string.Join(", ", lineInvalidDistrict));
        //                    }
        //                    #endregion
        //                    Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
        //                }
        //                else
        //                {
        //                    throw new MessageException(MessageType.Message, "20150611", "", new string[] { strEBranchID, BranchID });
        //                }
        //            }
        //            return _logMessage;

        //        }
        //        else
        //        {
        //            Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
        //        }
        //        return _logMessage;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException)
        //        {
        //            return (ex as MessageException).ToMessage();
        //        }
        //        else
        //        {                   
        //            return Json(new { success = false, type = "error", errorMsg = ex.Message.ToString() });
        //        }
        //    }
        //}
        #endregion

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
                    case "F1A":
                    case "F1B":
                    case "F1C":
                    case "F1D":
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
                //return false;
            }
        }       
        
        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle, bool isBackground = false)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
            {
                style.Font.Color = Color.Red;
            }
            if (isBackground)
            {
                style.Font.Color = Color.Red;
                style.Pattern = BackgroundType.Solid;
                style.ForegroundColor = Color.Yellow;
            }
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

        private string Getcell(int column) // Hàm bị sai khi lấy vị trí column AA
        {
            bool flag = false;
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 >= 1)
            {
                cell += ABC.Substring((column / 26)-1, 1);
                column = column - 26;
                flag = true;
            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            else
            {
                if (column % 26 == 0 && flag)
                {
                    cell += ABC.Substring(0, 1);
                }
            }
         
            return cell;
        }

        // Get Code from execl 
        private string GetCodeFromExcel(string codeDescr)
        {
            int index = codeDescr.IndexOf(" - ");
            if (index > 0)
            {
                return codeDescr.Substring(0, index);
            }
            return string.Empty;
        }

        #region -New: Export Cust + MCP-
        [HttpPost]
        public ActionResult ExportCustMCP(FormCollection data, string[] provinces, string provinceRawValue, string slsperID, string territory, bool isUpdated)
        {
            try
            {
                string channel = string.Empty;

                string branchID = data["branchID"].PassNull();
                string branchName = data["branchName"].PassNull();
                string[] provincesDescr = provinceRawValue.Contains(", ") 
                    ? provinceRawValue.Split(new string[] { ", " }, StringSplitOptions.None) 
                    : new string[] { provinceRawValue };

                var headerRowIdx = 3;
                var maxRow = 20000;
                var ColTexts = new List<string>() { 
                                        "N0", "SlsperID", "SlsName", "DeliveryUnit", "ShopID", "ShopName", "Attn", "BusinessName"
                                        , "StreetNo", "StreetName", "Ward", "Territory", "Province", "District", "SalesProvince"
                                        , "Phone" , "Channel", "ShopType", "Chain", "CustClass", "Classification" //, "Location"
                                        , "Latitude", "Longitude", "RefCustID", "InActive" //, "StartDate", "EndDate", "SlsFreq", "WeekofVisit"
                                    };
                var allColumns = new List<string>();
                allColumns.AddRange(ColTexts);

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                workbook.Worksheets.Add();
                Worksheet SheetMCP = workbook.Worksheets[0];

                Worksheet MasterData = workbook.Worksheets[1];
                MasterData.Name = "MasterData";
                SheetMCP.Name = "Customer";
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                Range range;
                //Cell cell;

                #region master data
                ParamCollection pc = new ParamCollection();                     
                
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",",provinces)), ParameterDirection.Input, int.MaxValue));

                DataTable dtDistrict = dal.ExecDataTable("OM23800_peDistrictCust", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtDistrict, true, 0, 52, false);// du lieu District BA

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                DataTable dtCustClass = dal.ExecDataTable("OM23800_peCustClassCust", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtCustClass, true, 0, 78, false);// du lieu CustClass CA
                
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",", provinces)), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtState = dal.ExecDataTable("OM23800_peExportState", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtState, true, 0, 104, false);// du lieu State => đổi thành Location FA

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                DataTable dtShopType = dal.ExecDataTable("OM23800_peShopType", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtShopType, true, 0, 130, false);// du lieu ShopType EA

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtLocation = dal.ExecDataTable("OM23800_peAR_Location", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtLocation, true, 0, 156, false);// du lieu SellProduct => đổi thành Location FA
                
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                DataTable dtSlsperMaster = dal.ExecDataTable("OM23800_peSlsperIDCust", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtSlsperMaster, true, 0, 182, false);// du lieu Slsperson      GA                     

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtChain = dal.ExecDataTable("OM23800_peCustomerChain", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtChain, true, 0, 208, false);// du lieu Customer Chain HA

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtClassification = dal.ExecDataTable("OM23800_peShopClassification", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtClassification, true, 0, 234, false);// du lieu Classification IA

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtChannel = dal.ExecDataTable("OM23800_peChannel", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtChannel, true, 0, 260, false);// du lieu Channel JA

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",", provinces)), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtTerritory = dal.ExecDataTable("OM23800_peExportDataTerritory", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtTerritory, true, 0, 286, false);// du lieu Territory KA

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));
                DataTable dtDeliveryUnit = dal.ExecDataTable("OM23800_peDeliveryUnit_ImExCustMCP", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtDeliveryUnit, true, 0, 312, false);// du lieu Channel LA

                //new
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtRoute = dal.ExecDataTable("OM23800_peRouteMCP", CommandType.StoredProcedure, ref pc);
                MasterData.Cells.ImportDataTable(dtRoute, true, 0, 0, false);// du lieu SalesRoute


                MasterData.Cells["AZ1"].PutValue("W159");
                MasterData.Cells["AZ2"].PutValue("W2610");
                MasterData.Cells["AZ3"].PutValue("W3711");
                MasterData.Cells["AZ4"].PutValue("W4812");
                MasterData.Cells["AZ5"].PutValue("OW");
                MasterData.Cells["AZ6"].PutValue("EW");
                MasterData.Cells["AZ7"].PutValue("NA");

                for (int i = 0; i < 53; i++)
                {
                    MasterData.Cells["AZ" + (i + 8)].PutValue("W" + (i + 1));
                }
         
                #endregion

                #region header info
                // Title header
                SetCellValue(SheetMCP.Cells["B1"],
                    string.Format("{0}", Util.GetLang("OM23800EHeaderCust")),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetMCP.Cells.Merge(0, 1, 1, ColTexts.Count - 2);

                // Title info
                SetCellValue(SheetMCP.Cells["B2"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["B3"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["C2"], branchID, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);
                SetCellValue(SheetMCP.Cells["C3"], branchName, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);

                string langUpdate = Util.GetLang("Template") + " "+ (isUpdated ? Util.GetLang("Update") : Util.GetLang("AddNew"));
                SetCellValue(SheetMCP.Cells["E2"], langUpdate, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);

                // Header text columns
                for (int i = 0; i < ColTexts.Count; i++)
                {
                    bool isHeaderColor = CheckSetHeaderColor(i);
                    var colIdx = i;
                    if (ColTexts[i] == "Location")
                    {
                        SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang("OM23800CustLocation"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, isHeaderColor);
                    }
                    else if (ColTexts[i] == "CustClass")
                    {
                        SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang("OM23800CustClass"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, isHeaderColor);
                    }
                    else
                    {
                        SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(ColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false, isHeaderColor);
                    }
                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }

                //// Route column
                //var daysOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                //SetCellValue(SheetMCP.Cells[headerRowIdx, ColTexts.Count], Util.GetLang("Route"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                //SheetMCP.Cells.Merge(headerRowIdx, ColTexts.Count, 1, daysOfWeeks.Length);
                //for (int i = 0; i < daysOfWeeks.Length; i++)
                //{
                //    var colIdx = ColTexts.Count + i;
                //    SetCellValue(SheetMCP.Cells[headerRowIdx + 1, colIdx], Util.GetLang(daysOfWeeks[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                //}

                //// after of Route column
                //var afterColTexts = new string[] { "SalesRouteID", "RouteName", "VisitSort", "CustCancel" };
                //for (int i = 0; i < afterColTexts.Length; i++)
                //{
                //    var colIdx = ColTexts.Count + daysOfWeeks.Length + i;
                    
                //    if (afterColTexts[i] == "CustCancel")
                //    {
                //        SetCellValue(SheetMCP.Cells[headerRowIdx, colIdx], Util.GetLang("DeleteMCP"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                //        SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                //    }
                //    else
                //    {
                //        SetCellValue(SheetMCP.Cells[headerRowIdx, colIdx], Util.GetLang(afterColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                //        SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                //    }
                //}                
                //allColumns.AddRange(daysOfWeeks);
                //allColumns.AddRange(afterColTexts);
                
                #endregion

                #region export data
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@State", DbType.String, clsCommon.GetValueDBNull(string.Join(",", provinces)), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@IsUpdated", DbType.String, clsCommon.GetValueDBNull(isUpdated), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@Channel", DbType.String, clsCommon.GetValueDBNull(channel), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@Territory", DbType.String, clsCommon.GetValueDBNull(territory), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 4));

                
                DataTable dtDataExport = dal.ExecDataTable("OM23800_peExportDataCustMCP", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        if (allColumns[j] == "N0")
                        {
                            SheetMCP.Cells[headerRowIdx + 2 + i, j].PutValue(i + 1);
                        }
                        else if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            SheetMCP.Cells[headerRowIdx + 2 + i, j].PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }
                    }
                }
                #endregion

                #region formular
                //SlsperID
                string formulaSlsper = "=MasterData!$GA$2:$GA$" + (dtSlsperMaster.Rows.Count + 2);
                // Validation validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                Validation validation = GetValidation(ref SheetMCP, formulaSlsper, "Chọn mã nhân viên", "Mã nhân viên này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("SlsperID")));

                // Đơn Vị Giao Hàng
                string formulaDeliveryUnit = "=MasterData!$LB$2:$LB$" + (dtDeliveryUnit.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaDeliveryUnit, "Chọn Đơn Vị Giao Hàng", "Đơn Vị Giao Hàng này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("DeliveryUnit"))); 

                // Zone
                string formulaTerritory = "=MasterData!$KB$2:$KB$" + (dtTerritory.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaTerritory, "Chọn Vùng Bán Hàng", "Vùng Bán Hàng này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("Territory")));

                // State
                string formulaState = string.Format("=OFFSET(MasterData!$DB$1,IFERROR(MATCH(L{0},MasterData!$DC$2:$DC${1},0),{2}),0, IF(COUNTIF(MasterData!$DC$2:$DC${1},L{0})=0,1,COUNTIF(MasterData!$DC$2:$DC${1},L{0})),1)", new string[] { "6", (dtState.Rows.Count + 1).ToString(), (dtState.Rows.Count + 2).ToString() }); ;//
                validation = GetValidation(ref SheetMCP, formulaState, "Chọn Tỉnh", "Tỉnh này không tồn tại"); 
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("Province")));

                // District
                string formulaDistrict = string.Format("=OFFSET(MasterData!$BB$2,IFERROR(MATCH(M{0},MasterData!$BC$2:$BC${1},0),{2}),0, IF(COUNTIF(MasterData!$BC$2:$BC${1},M{0})=0,1,COUNTIF(MasterData!$BC$2:$BC${1},M{0})),1)", new string[] { "6", (dtDistrict.Rows.Count + 1).ToString(), (dtDistrict.Rows.Count + 2).ToString() });
                validation = GetValidation(ref SheetMCP, formulaDistrict, "Chọn quận huyện", "Mã quận huyện không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("District")));


                // SalesProvince
                string formulaSalesProvince = "=MasterData!$DB$2:$DB$" + (dtState.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaSalesProvince, "Chọn Tỉnh", "Tỉnh này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2,ColTexts.IndexOf("SalesProvince")));

                // Channel
                string formulaChannel = "=MasterData!$JB$2:$JB$" + (dtChannel.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaChannel, "Chọn Kênh", "Kênh này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("Channel")));

                //Cust Class
                string formulaSales = "=MasterData!$CB$2:$CB$" + (dtCustClass.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaSales, "Chọn nhóm khách hàng", "Mã nhóm khách hàng này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("CustClass")));

                // ShopType
                string formulaChain = "=MasterData!$HB$2:$HB$" + (dtChain.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaChain, "Chọn Chuỗi", "Chuỗi này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("Chain")));

                // ShopType
                string formulaShopType = "=MasterData!$EB$2:$EB$" + (dtShopType.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaShopType, "Chọn Loại hình kinh doanh", "Loại hình kinh doanh này không tồn tại");
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("ShopType")));

                // Location
                string formulaClassification = "=MasterData!$IB$2:$IB$" + (dtClassification.Rows.Count + 2);
                validation = GetValidation(ref SheetMCP, formulaClassification, "Chọn Phân Cấp", "Phân Cấp này không tồn tại"); 
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("Classification")));

                //// Location
                //string formulaLocation = "=MasterData!$FB$2:$FB$" + (dtLocation.Rows.Count + 2);
                //validation = GetValidation(ref SheetMCP, formulaLocation, "Chọn Vị trí khách hàng", "Vị trí khách hàng này không tồn tại");
                //validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("Location")));

                // InActive
                string formulaInActive = "X";// 
                validation = GetValidation(ref SheetMCP, formulaInActive, "Chọn giá trị", "Giá trị này không tồn tại");  
                validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("InActive")));

                string formulaSlsName = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$GA:$GC,2,0)),\"\",VLOOKUP({0},MasterData!$GA:$GC,2,0))", "B6");
                SheetMCP.Cells[Getcell(ColTexts.IndexOf("SlsName")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaSlsName, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);

                //string formulaDistrictCode = string.Format("=IF(ISERROR(VLOOKUP({0},CHOOSE({{2,1}},$BA$1:$BA${1},$BB$1:$BB${1}),2,0)),\"\",VLOOKUP({0},CHOOSE({{2,1}},$BA$1:$BA${1},$BB$1:$BB${1}),2,0))", "M6", dtDistrict.Rows.Count + 1);
                //SheetMCP.Cells[Getcell(ColTexts.IndexOf("DistrictCode")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaDistrictCode, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);

                //string formulaProvinceCode = string.Format("=IF(ISERROR(VLOOKUP({0},CHOOSE({{2,1}},$DA$1:$DA${1},$DB$1:$DB${1}),2,0)),\"\",VLOOKUP({0},CHOOSE({{2,1}},$DA$1:$DA${1},$DB$1:$DB${1}),2,0))", "K6", provinces.Length);
                //SheetMCP.Cells[Getcell(ColTexts.IndexOf("ProvinceCode")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaProvinceCode, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);
                #endregion
               
                #region formular MCP
                //validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                //validation.IgnoreBlank = true;
                //validation.Type = Aspose.Cells.ValidationType.Date;
                //validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                //validation.Operator = OperatorType.GreaterOrEqual;
                //validation.Formula1 = DateTime.Now.ToShortDateString();
                //validation.InputTitle = "Chọn Ngày Bắt Đầu(MM/dd/yyyy)";
                //validation.InputMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");
                //validation.ErrorMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");
                //validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("StartDate")));

                //string formulaDate = "=$" + Getcell(ColTexts.IndexOf("StartDate")) + "$6";
                //validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                //validation.IgnoreBlank = true;
                //validation.Type = Aspose.Cells.ValidationType.Date;
                //validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                //validation.Operator = OperatorType.GreaterOrEqual;
                //validation.Formula1 = formulaDate;
                //validation.InputTitle = "Chọn Ngày Kết Thúc(MM/dd/yyyy)";
                //validation.InputMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";
                //validation.ErrorMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";
                //validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("EndDate")));

                ////Route
                //string formulaRoutes = "=MasterData!$A$2:$A$" + (dtRoute.Rows.Count + 2);
                //validation = GetValidation(ref SheetMCP, formulaRoutes, "Chọn Mã Tuyến Đường", "Mã Tuyến Đường này không tồn tại");
                //validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, allColumns.IndexOf("SalesRouteID")));

                ////Requency LIST
                //string formulaRequenc = "F1,F2,F4,F8,F12,A";// "F1A,F1B,F1C,F1D,F1,F2,F4,F4A,F8,F12,F1/2,F1/3,A";
                //validation = GetValidation(ref SheetMCP, formulaRequenc, "Chọn Tần Suất Thăm Viếng", "Mã Tần Suất này không tồn tại");
                //validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("SlsFreq")));
  

                //string formula = "=IF(AC6=\"F1\",MasterData!$AZ$1:$AZ$4,IF(OR(AC6=\"F1/2\",AC6=\"F1/3\"),MasterData!$AZ$8:$AZ$61,IF(OR(AC6=\"F2\",AC6=\"F4A\",AC6=\"F8A\"),MasterData!$AZ$5:$AZ$6,MasterData!$AZ$7:$AZ$7)))";// + dtOMRoute.Rows.Count + 2;               
                //validation = GetValidation(ref SheetMCP, formula, "Chọn Tuần Bán Hàng", "Mã Tuần Bán Hàng Không tồn tại");
                //validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, ColTexts.IndexOf("WeekofVisit")));

                //string formulaCheck = "X";
                //validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                //validation.IgnoreBlank = true;
                //validation.Type = Aspose.Cells.ValidationType.List;
                //validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                //validation.Operator = OperatorType.Between;
                //validation.Formula1 = formulaCheck;                 
                //validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, allColumns.IndexOf("Mon"), allColumns.IndexOf("Sun")));
                //area = new CellArea();
                //area.StartRow = headerRowIdx + 2;
                //area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                //area.StartColumn = allColumns.IndexOf("Mon");
                //area.EndColumn = allColumns.IndexOf("Sun");
                //validation.AddArea(area);

                //area = new CellArea();
                //area.StartRow = headerRowIdx + 2;
                //area.EndRow = dtDataExport.Rows.Count + maxRow + headerRowIdx + 2;
                //area.StartColumn = allColumns.IndexOf("CustCancel");
                //area.EndColumn = allColumns.IndexOf("CustCancel");
                //validation.AddArea(area);

               // validation.AddArea(GetCellArea(headerRowIdx + 2, dtDataExport.Rows.Count + maxRow + headerRowIdx + 2, allColumns.IndexOf("CustCancel")));

                //string formulaRoute = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$A:$C,2,0)),\"\",VLOOKUP({0},MasterData!$A:$C,2,0))", "AL6");
                //SheetMCP.Cells[Getcell(allColumns.IndexOf("RouteName")) + (headerRowIdx + 3).ToString()].SetSharedFormula(formulaRoute, (dtDataExport.Rows.Count + maxRow + headerRowIdx + 5), 1);// (dtCustomer.Rows.Count + 6)

                #endregion
                
                #region Fomat cell
                var strFirstRow = (headerRowIdx + 2).ToString();
                var strLastRow = (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2).ToString();
    
                style = SheetMCP.Cells["A" + strFirstRow].GetStyle();
                style.IsLocked = false;
                range = SheetMCP.Cells.CreateRange("A" + strFirstRow, "A" + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("SlsperID")) + strFirstRow, 
                    Getcell(allColumns.IndexOf("SlsperID")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("DeliveryUnit")) + strFirstRow,
                    Getcell(allColumns.IndexOf("DeliveryUnit")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("ShopName")) + strFirstRow, 
                    Getcell(allColumns.IndexOf("ShopName")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Attn")) + strFirstRow, 
                    Getcell(allColumns.IndexOf("Attn")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("BusinessName")) + strFirstRow,
                   Getcell(allColumns.IndexOf("BusinessName")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("StreetNo")) + strFirstRow,
                    Getcell(allColumns.IndexOf("StreetNo")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("StreetName")) + strFirstRow,
                    Getcell(allColumns.IndexOf("StreetName")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Ward")) + strFirstRow,
                    Getcell(allColumns.IndexOf("Ward")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Territory")) + strFirstRow,
                    Getcell(allColumns.IndexOf("Territory")) + strLastRow);
                range.SetStyle(style);
       
                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Province")) + strFirstRow, 
                    Getcell(allColumns.IndexOf("Province")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("District")) + strFirstRow, 
                    Getcell(allColumns.IndexOf("District")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("SalesProvince")) + strFirstRow,
                    Getcell(allColumns.IndexOf("SalesProvince")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Phone")) + strFirstRow, 
                    Getcell(allColumns.IndexOf("Phone")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Channel")) + strFirstRow,
                    Getcell(allColumns.IndexOf("Channel")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("CustClass")) + strFirstRow,
                    Getcell(allColumns.IndexOf("CustClass")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Chain")) + strFirstRow,
                    Getcell(allColumns.IndexOf("Chain")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("ShopType")) + strFirstRow,
                   Getcell(allColumns.IndexOf("ShopType")) + strLastRow);
                range.SetStyle(style);
	
                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Classification")) + strFirstRow,
                    Getcell(allColumns.IndexOf("Classification")) + strLastRow);
                range.SetStyle(style);

                //range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Location")) + strFirstRow,
                //    Getcell(allColumns.IndexOf("Location")) + strLastRow);
                //range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Latitude")) + strFirstRow,
                    Getcell(allColumns.IndexOf("Latitude")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Longitude")) + strFirstRow,
                    Getcell(allColumns.IndexOf("Longitude")) + strLastRow);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("RefCustID")) + strFirstRow,
                    Getcell(allColumns.IndexOf("RefCustID")) + strLastRow);
                range.SetStyle(style);

                //range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("CustCancel")) + strFirstRow,
                //    Getcell(allColumns.IndexOf("CustCancel")) + strLastRow);
                //range.SetStyle(style);

                 range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("InActive")) + strFirstRow,
                    Getcell(allColumns.IndexOf("InActive")) + strLastRow);
                range.SetStyle(style);

                // range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("SlsFreq")) + strFirstRow,
                //    Getcell(allColumns.IndexOf("SlsFreq")) + strLastRow);
                //range.SetStyle(style);

                // range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("WeekofVisit")) + strFirstRow,
                //    Getcell(allColumns.IndexOf("WeekofVisit")) + strLastRow);
                //range.SetStyle(style);

                
                //range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("SalesRouteID")) + strFirstRow,
                //    Getcell(allColumns.IndexOf("SalesRouteID")) + strLastRow);
                //range.SetStyle(style);

                //range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("VisitSort")) + strFirstRow,
                //    Getcell(allColumns.IndexOf("VisitSort")) + strLastRow);
                //range.SetStyle(style);

                //range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("Mon")) + strFirstRow,
                //    Getcell(allColumns.IndexOf("Sun")) + strLastRow);
                //range.SetStyle(style);               

                //style = SheetMCP.Cells[allColumns.IndexOf("VisitSort")].GetStyle();
                //style.Custom = "#,##0";
                //style.IsLocked = false;
                //style.Font.Color = Color.Black;
                //style.HorizontalAlignment = TextAlignmentType.Right;

                //range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("VisitSort")) + strFirstRow, Getcell(allColumns.IndexOf("VisitSort")) + strLastRow);//dtCustomer.Rows.Count + 5
                //range.SetStyle(style);

                //range = SheetMCP.Cells.CreateRange("A" + strFirstRow, "A" + strLastRow);//dtCustomer.Rows.Count + 5
                //range.SetStyle(style);

                //style = SheetMCP.Cells["AO1"].GetStyle();
                //style.Font.Color = Color.Transparent;
                //style.IsLocked = false;
                //flag.FontColor = true;
                //flag.NumberFormat = true;
                //flag.Locked = true;
                //range = SheetMCP.Cells.CreateRange("AO1", "XZ" + (strLastRow + 100));
                //range.ApplyStyle(style, flag);

                //style = SheetMCP.Cells[Getcell(ColTexts.IndexOf("Location")) + strFirstRow].GetStyle();
                //style.Font.Color = Color.Red;
                //style.IsLocked = false;
                //flag.FontColor = true;

                //range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("Location")) + strFirstRow,
                //    Getcell(ColTexts.IndexOf("Location")) + strLastRow);

                //range = SheetMCP.Cells.CreateRange("O6", "O" + (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2));
                range.ApplyStyle(style, flag);

                //style = SheetMCP.Cells["U6"].GetStyle();
                //style.Font.Color = Color.Red;
                //style.IsLocked = false;
                //flag.FontColor = true;
                //range = SheetMCP.Cells.CreateRange("S6", "S" + (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2));                

                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("ShopType")) + strFirstRow,
                    Getcell(ColTexts.IndexOf("ShopType")) + strLastRow);
                range.ApplyStyle(style, flag);

                // Check auto Cust
                var obj = _db.OM23800_ppCheckAutoCustID(branchID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                bool isLock = obj == "1";
                
                var shopIDStyle = SheetMCP.Cells[Getcell(ColTexts.IndexOf("ShopID")) + strFirstRow].GetStyle();
                shopIDStyle.IsLocked = isLock;
                range = SheetMCP.Cells.CreateRange(Getcell(ColTexts.IndexOf("ShopID")) + strFirstRow, Getcell(ColTexts.IndexOf("ShopID")) + strLastRow);
                //range = SheetMCP.Cells.CreateRange("O6", "O" + (dtDataExport.Rows.Count + maxRow + headerRowIdx + 2));
                range.SetStyle(shopIDStyle);


                //string startCol = "AA" + strFirstRow; // Check lại hàm getCell
                //string endCol = "AA" + strLastRow;
                //var dateStyle = SheetMCP.Cells[startCol].GetStyle();
                //dateStyle.IsLocked = false;
                //dateStyle.Custom = "MM/dd/yyyy";
                ////dateStyle.Font.Color = Color.Black;
                ////dateStyle.HorizontalAlignment = TextAlignmentType.Left;
                
                //var dateRange = SheetMCP.Cells.CreateRange(startCol, endCol);// //dtCustomer.Rows.Count + 5
                //dateRange.SetStyle(dateStyle);

                //dateRange = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("EndDate")) + strFirstRow, Getcell(allColumns.IndexOf("EndDate")) + strLastRow);//dtCustomer.Rows.Count + 5
                //dateRange.SetStyle(dateStyle);

                SheetMCP.Protect(ProtectionType.All);
                MasterData.Protect(ProtectionType.All);
                #endregion                
                
                MasterData.VisibilityType = VisibilityType.Hidden;
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
        public ActionResult ImportCustMCP(FormCollection data, bool isUpdated)
        {
            try
            {
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
                string messgeWeekOfVistit = string.Empty;
                var ColTexts = new List<string>() { 
                                        "N0", "SlsperID", "SlsName", "DeliveryUnit", "ShopID", "ShopName", "Attn", "BusinessName"
                                        , "StreetNo", "StreetName", "Ward", "Territory", "Province", "District", "SalesProvince"
                                        , "Phone" , "Channel", "ShopType", "Chain", "CustClass", "Classification" //, "Location"
                                        , "Latitude", "Longitude", "RefCustID", "InActive" //, "StartDate", "EndDate", "SlsFreq", "WeekofVisit" , "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun", "SalesRouteID", "RouteName", "VisitSort", "CustCancel"
                                    };
                string BranchID = data["branchID"].PassNull();
                string pJPID=BranchID;
                var dataRowIdx = 5;
                var date = DateTime.Now.Date;
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImpCustMCP");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
  
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {                    
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    Worksheet workSheet = workbook.Worksheets[0];
                    if (workbook.Worksheets.Count > 0)
                    {
                        string strEBranchID = workSheet.Cells[1, 2].StringValue.Trim();
                        if (strEBranchID == BranchID)
                        {
                            #region -Variable Line error-
                            var lineSuccess = new List<string>();
                            var lineBlank = new List<string>();
                            var lineExistRefCustID = new List<string>();
                            var lineExist = new List<string>();
                            var lineNoExist = new List<string>();
                            var lineSlsNoExist = new List<string>();
                            var lineInvalidDistrict = new List<string>();
                            var lineInvalidGeo = new List<string>();
                            var lineCustID = new List<string>();

                            var lineSlsPerID = new List<string>();
                            var lineShopID = new List<string>();
                            var lineShopName = new List<string>();
                            //var lineAttn = new List<string>();
                            //var lineStreetNo = new List<string>();
                            //var lineStreetName = new List<string>();
                            //var lineWard = new List<string>();
                            var lineProvince = new List<string>();
                            var lineDistrict = new List<string>();
                            var lineCustClass = new List<string>();

                            var lineTerritory = new List<string>();
                            var lineChannel = new List<string>();
                            var lineClassID = new List<string>();
                            //var lineChain = new List<string>();
                            var lineShopType = new List<string>();
                           // var lineClassification = new List<string>();
                           // var lineLocation = new List<string>();


                            var lineTerritoryNotExist = new List<string>();
                            var lineChannelNotExist = new List<string>();
                            var lineCustClassNotExist = new List<string>();
                            var lineChainNotExist = new List<string>();
                            var lineShopTypeNotExist = new List<string>();
                            var lineClassificationNotExist = new List<string>();
                            //var lineLocationNotExist = new List<string>();
                            var flagCheckError = false;
                            #endregion

                            #region -Variable data-                                                        
                            string strSlsPerID = string.Empty;
                            string strShopID = string.Empty;
                            string strShopName = string.Empty;
                            string strAttn = string.Empty;
                            string strBusName = string.Empty;
                            string strDistrict = string.Empty;
                            string strProvince = string.Empty;
                            string strPhone = string.Empty;
                            string strCustClass = string.Empty;
                            string strRefCustID = string.Empty;
                            string strShopType = string.Empty;
                          //  string strLocation = string.Empty;
                            string strCountry = string.Empty;
                            string strCity = string.Empty;
                            string strInActive = string.Empty;
                            
                            string Channel = string.Empty;
                            string Chain = string.Empty;
                            string Classification = string.Empty;
                            string StreetNo = string.Empty;
                            string StreetName = string.Empty;
                            string Ward = string.Empty;
                            string Territory = string.Empty;
                            string DeliveryUnit = string.Empty;
                            string SalesProvince = string.Empty;
                            bool autoCustID = true;
                            double lat = 0;
                            double lng = 0;

                            var lstTerritory = _db.OM23800_peExportDataTerritory(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            var lstChannel = _db.OM23800_peChannel(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            var lstCustClass = _db.OM23800_peCustClassCust(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            var lstChain = _db.OM23800_peCustomerChain(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            var lstShopType = _db.OM23800_peShopType(BranchID, Current.UserName).ToList();
                            var lstClassification = _db.OM23800_peShopClassification(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                           // var lstLocation = _db.OM23800_peAR_Location(BranchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            
                           
                            var objAR_Setup = _db.AR_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupId == "AR");
                            #endregion
                            
                            #region MCP
                            //string strERouteID = "";
                            string strECustID = "";
                            string strESlsperID = "";
                            //string strEBeginDate = "";
                            //string strEEndDate = "";
                            //string strETS = "";
                            //string strETBH = "";
                            //string strESTT = "";
                            DateTime startDate = DateTime.Now;
                            DateTime endDate = DateTime.Now;
                            var lstWeekOfVisit = _db.OM23800_piWeekofVisit(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                            var lstOM_SalesRouteMasterImport = new List<OM_SalesRouteMasterImport>();

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

                            //string lstCustomer = "";
                            //   string strtmpError = "";
                            string id = Guid.NewGuid().ToString();
                            #endregion
                          
                            if (objAR_Setup != null)
                            {
                                autoCustID = objAR_Setup.AutoCustID;
                            }
                           
                            for (int i = dataRowIdx; i <= workSheet.Cells.MaxDataRow; i++)
                            {
                                #region -Get data from excel-
                                //if (workSheet.Cells[i,0].StringValue.PassNull().Trim() == "") break;

                                strESlsperID = strSlsPerID = workSheet.Cells[i, ColTexts.IndexOf("SlsperID")].StringValue.Trim();
                                strECustID=strShopID = workSheet.Cells[i, ColTexts.IndexOf("ShopID")].StringValue.Trim();
                                strShopName = workSheet.Cells[i, ColTexts.IndexOf("ShopName")].StringValue.Trim();
                                strAttn = workSheet.Cells[i, ColTexts.IndexOf("Attn")].StringValue.Trim();
                                strBusName = workSheet.Cells[i, ColTexts.IndexOf("BusinessName")].StringValue.Trim();
                                StreetNo = workSheet.Cells[i, ColTexts.IndexOf("StreetNo")].StringValue.Trim();
                                StreetName = workSheet.Cells[i, ColTexts.IndexOf("StreetName")].StringValue.Trim();
                                Ward = workSheet.Cells[i, ColTexts.IndexOf("Ward")].StringValue.Trim();
                                Territory = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("Territory")].StringValue.Trim());
                                DeliveryUnit = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("DeliveryUnit")].StringValue.Trim());
                                SalesProvince = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("SalesProvince")].StringValue.Trim());

                                strProvince = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("Province")].StringValue.Trim());
                                strDistrict = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("District")].StringValue.Trim());

                                strPhone = workSheet.Cells[i, ColTexts.IndexOf("Phone")].StringValue.Trim();

                                Channel = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("Channel")].StringValue.Trim());
                                strCustClass = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("CustClass")].StringValue.Trim());
                                Chain = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("Chain")].StringValue.Trim());
                                strShopType = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("ShopType")].StringValue.Trim());
                                Classification = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("Classification")].StringValue.Trim());
                              //  strLocation = GetCodeFromExcel(workSheet.Cells[i, ColTexts.IndexOf("Location")].StringValue.Trim());

                                double.TryParse(workSheet.Cells[i, ColTexts.IndexOf("Latitude")].StringValue.Trim(), out lat); //13 
                                double.TryParse(workSheet.Cells[i, ColTexts.IndexOf("Longitude")].StringValue.Trim(), out lng); //14 
                                strRefCustID = workSheet.Cells[i, ColTexts.IndexOf("RefCustID")].StringValue.Trim();
                                strInActive = workSheet.Cells[i, ColTexts.IndexOf("InActive")].StringValue.Trim();
                                flagCheckError = false;
                                #endregion

                                // Continue when row is empty
                                //if (_checkRequireImport == false)
                                //{
                                if (strSlsPerID == "" && strShopName == "" && strAttn == "" && strShopID == "" && strBusName == ""
                                        && StreetNo == "" && StreetName == "" && Ward == "" && Territory == "" && strProvince == "" && strDistrict == ""
                                        && Channel == "" && strCustClass == "" && Chain == "" && strShopType == "" && Classification == "" //&& strLocation == ""
                                        )
                                {
                                    continue;
                                }                            
                        
                                #region -Check required field-
                                if (strSlsPerID == "")
                                {
                                    lineSlsPerID.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (strShopName == "")
                                {
                                    lineShopName.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                //if (strAttn == "")
                                //{
                                //    lineAttn.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
                                //if (StreetNo == "")
                                //{
                                //    lineStreetNo.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
                                //if (StreetName == "")
                                //{
                                //    lineStreetName.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
                                //if (Ward == "")
                                //{
                                //    lineWard.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
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
                                if (Territory == "")
                                {
                                    lineTerritory.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (Channel == "")
                                {
                                    lineChannel.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                if (strCustClass == "")
                                {
                                    lineCustClass.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                //if (Chain == "")
                                //{
                                //    lineChain.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
                                if (strShopType == "")
                                {
                                    lineShopType.Add((i - dataRowIdx + 1).ToString());
                                    flagCheckError = true;
                                }
                                //if (Classification == "")
                                //{
                                //    lineClassification.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
                                //if (strLocation == "")
                                //{
                                //    lineLocation.Add((i - dataRowIdx + 1).ToString());
                                //    flagCheckError = true;
                                //}
                                #endregion

                                if (flagCheckError == false)
                                {                                                                                                          
                                    if (!string.IsNullOrWhiteSpace(strSlsPerID)
                                        || !string.IsNullOrWhiteSpace(strShopID)
                                        || !string.IsNullOrWhiteSpace(strShopName)
                                        || !string.IsNullOrWhiteSpace(strAttn)
                                        || !string.IsNullOrWhiteSpace(StreetNo)
                                        || !string.IsNullOrWhiteSpace(StreetName)
                                        || !string.IsNullOrWhiteSpace(Ward)
                                        || !string.IsNullOrWhiteSpace(Territory)
                                        || !string.IsNullOrWhiteSpace(strDistrict)
                                        || !string.IsNullOrWhiteSpace(strProvince)
                                        || !string.IsNullOrWhiteSpace(strPhone)
                                        || !string.IsNullOrWhiteSpace(Channel)
                                        || !string.IsNullOrWhiteSpace(strCustClass)
                                        || !string.IsNullOrWhiteSpace(Chain)
                                        || !string.IsNullOrWhiteSpace(strShopType)
                                        || !string.IsNullOrWhiteSpace(Classification)
                                      //  || !string.IsNullOrWhiteSpace(strLocation)
                                        || lat > 0 || lng > 0)
                                    {
                                        var slsright = true;

                                        #region -Check data-
                                        if (!string.IsNullOrWhiteSpace(strSlsPerID))
                                        {
                                            var slsper = _db.AR_Salesperson.FirstOrDefault(s => s.SlsperId == strSlsPerID && s.BranchID == strEBranchID);
                                            if (slsper == null)
                                            {
                                                lineSlsNoExist.Add((i - dataRowIdx + 1).ToString());
                                                slsright = false;
                                            }
                                        }

                                        if (!string.IsNullOrWhiteSpace(Territory))
                                        {
                                            if (lstTerritory.FirstOrDefault(x => x.Code == Territory) == null)
                                            {
                                                lineTerritoryNotExist.Add((i - dataRowIdx + 1).ToString());
                                            }
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
                                                var city = _db.SI_City.FirstOrDefault(c => c.Country == strCountry && c.State == strProvince);
                                                if (city != null)
                                                {
                                                    strCity = city.City;
                                                }
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(Channel))
                                        {
                                            if (lstChannel.FirstOrDefault(x => x.Code == Channel) == null)
                                            {
                                                lineChannelNotExist.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(strCustClass))
                                        {
                                            if (lstCustClass.FirstOrDefault(x => x.Code == strCustClass) == null)
                                            {
                                                lineCustClassNotExist.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(Chain))
                                        {
                                            if (lstChain.FirstOrDefault(x => x.Code == Chain) == null)
                                            {
                                                lineChainNotExist.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(strShopType))
                                        {
                                            if (lstShopType.FirstOrDefault(x => x.Code == strShopType) == null)
                                            {
                                                lineShopTypeNotExist.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(Classification))
                                        {
                                            if (lstClassification.FirstOrDefault(x => x.Code == Classification) == null)
                                            {
                                                lineClassificationNotExist.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                        //if (!string.IsNullOrWhiteSpace(strLocation))
                                        //{
                                        //    if (lstLocation.FirstOrDefault(x => x.Code == strLocation) == null)
                                        //    {
                                        //        lineLocationNotExist.Add((i - dataRowIdx + 1).ToString());
                                        //    }
                                        //}
                                        #endregion

                                        #region checkMCP
                                        //strERouteID = workSheet.Cells[i, ColTexts.IndexOf("SalesRouteID")].StringValue.Trim();
                                        //strEBeginDate = workSheet.Cells[i, ColTexts.IndexOf("StartDate")].StringValue.Trim();
                                        //strEEndDate = workSheet.Cells[i, ColTexts.IndexOf("EndDate")].StringValue.Trim();
                                        //strETS = workSheet.Cells[i, ColTexts.IndexOf("SlsFreq")].StringValue.Trim();
                                        //strETBH = workSheet.Cells[i, ColTexts.IndexOf("WeekofVisit")].StringValue.Trim();
                                        ////if (strESTT.PassNull().Trim() == "") break;
                                        //if (strERouteID.PassNull() == "" &&
                                        //    strECustID.PassNull() == "" && strESlsperID.PassNull() == "" &&
                                        //    strEBeginDate.PassNull() == "" && strEEndDate.PassNull() == "" &&
                                        //    strETS.PassNull() == "" && strETBH.PassNull() == "")
                                        //    continue;

                                        //if (strERouteID == "")
                                        //{
                                        //    messagestrERouteID += (i - dataRowIdx + 1).ToString() + ",";
                                        //    slsright = false;
                                        //}

                                        //if (strETS == "")
                                        //{
                                        //    messagestrETS += (i - dataRowIdx + 1).ToString() + ",";
                                        //    slsright = false;
                                        //}
                                        //if (strETBH == "")
                                        //{
                                        //    messagestrETBH += (i - dataRowIdx + 1).ToString() + ",";
                                        //    slsright = false;
                                        //}
                                        //if (strEBeginDate == "")
                                        //{
                                        //    messagestrEBeginDate += (i - dataRowIdx + 1).ToString() + ",";
                                        //    slsright = false;
                                        //}
                                        //if (strEEndDate == "")
                                        //{
                                        //    messagestrEEndDate += (i - dataRowIdx + 1).ToString() + ",";
                                        //    slsright = false;
                                        //}

                                        
                                        #endregion
                                        
                                        if (isUpdated)
                                        {
                                            if (!string.IsNullOrWhiteSpace(strShopID)
                                                && !string.IsNullOrWhiteSpace(strShopName)
                                                && !string.IsNullOrWhiteSpace(Territory)
                                                && !string.IsNullOrWhiteSpace(strDistrict)
                                                && !string.IsNullOrWhiteSpace(strProvince)
                                                && !string.IsNullOrWhiteSpace(Channel)
                                                && !string.IsNullOrWhiteSpace(strCustClass)
                                                && !string.IsNullOrWhiteSpace(Chain)
                                                && !string.IsNullOrWhiteSpace(strShopType)
                                                && !string.IsNullOrWhiteSpace(Classification)
                                                //&& !string.IsNullOrWhiteSpace(strLocation)
                                                )
                                            {
                                                //if (_checkRequireImport == true && string.IsNullOrWhiteSpace(strShopType)) // Luôn ktra shoptype
                                                //    continue;
                                                if (slsright)
                                                {
                                                    var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustId == strShopID && c.BranchID == strEBranchID);
                                                    if (existCust != null)
                                                    {
                                                        if (existCust.RefCustID != strRefCustID && strRefCustID.PassNull() != "")
                                                        {
                                                            var existRefCustID = _db.AR_Customer.FirstOrDefault(p => p.BranchID == strEBranchID && p.RefCustID == strRefCustID);
                                                            if (existRefCustID == null)
                                                            {
                                                                #region -Edit Cust-
                                                                existCust.CustName = existCust.BillName = strShopName;
                                                                existCust.Attn = existCust.BillAttn = strAttn;
                                                                existCust.BusinessName = strBusName;
                                                                existCust.Addr1 = existCust.BillAddr1 = StreetName;
                                                                existCust.Addr2 = existCust.BillAddr2 = StreetNo;
                                                                existCust.Ward = Ward;
                                                                existCust.District = strDistrict;
                                                                existCust.State = existCust.BillState = strProvince;
                                                                existCust.Phone = existCust.BillPhone = strPhone;
                                                                existCust.Country = existCust.BillCountry = strCountry;
                                                                existCust.City = existCust.BillCity = strCity;
                                                                existCust.Territory = Territory;
                                                                existCust.Channel = Channel;
                                                                existCust.ClassId = strCustClass;
                                                                existCust.Chain = Chain;
                                                                existCust.ShopType = strShopType;
                                                                existCust.Classification = Classification;
                                                               // existCust.Location = strLocation;

                                                                existCust.DeliveryUnit = DeliveryUnit;
                                                                existCust.SalesProvince = SalesProvince;

                                                                existCust.LUpd_Datetime = DateTime.Now;
                                                                existCust.LUpd_Prog = _screenName;
                                                                existCust.LUpd_User = Current.UserName;
                                                                existCust.SlsperId = strSlsPerID.PassNull();

                                                                existCust.SellProduct = "";
                                                                existCust.RefCustID = strRefCustID;
                                                                if (strInActive == "X")
                                                                    existCust.Status = inActiveStatus;
                                                                else
                                                                    existCust.Status = activeStatus;
                                                                if (lat > 0 && lng > 0)
                                                                {
                                                                    updateCustomerLocation(strEBranchID, strShopID, lat, lng);
                                                                }
                                                                else if (lat > 0 || lng > 0)
                                                                {
                                                                    lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
                                                                }
                                                                lineSuccess.Add((i - dataRowIdx + 1).ToString());
                                                                #endregion
                                                            }
                                                            else
                                                            {
                                                                lineExistRefCustID.Add((i - dataRowIdx + 1).ToString());
                                                            }
                                                        }
                                                        else
                                                        {
                                                            #region -Edit Cust-
                                                            existCust.CustName = existCust.BillName = strShopName;
                                                            existCust.Attn = existCust.BillAttn = strAttn;
                                                            existCust.BusinessName = strBusName;
                                                            existCust.Addr1 = existCust.BillAddr1 = StreetName;
                                                            existCust.Addr2 = existCust.BillAddr2 = StreetNo;
                                                            existCust.Ward = Ward;
                                                            existCust.District = strDistrict;
                                                            existCust.State = existCust.BillState = strProvince;
                                                            existCust.Phone = existCust.BillPhone = strPhone;
                                                            existCust.Country = existCust.BillCountry = strCountry;
                                                            existCust.City = existCust.BillCity = strCity;

                                                            existCust.Territory = Territory;
                                                            existCust.Channel = Channel;
                                                            existCust.ClassId = strCustClass;
                                                            existCust.Chain = Chain;
                                                            existCust.ShopType = strShopType;
                                                            existCust.Classification = Classification;
                                                           // existCust.Location = strLocation;
                                                            existCust.DeliveryUnit = DeliveryUnit;
                                                            existCust.SalesProvince = SalesProvince;

                                                            existCust.LUpd_Datetime = DateTime.Now;
                                                            existCust.LUpd_Prog = _screenName;
                                                            existCust.LUpd_User = Current.UserName;
                                                            existCust.SlsperId = strSlsPerID.PassNull();
                                                            existCust.ShopType = strShopType;
                                                            existCust.SellProduct = "";

                                                            existCust.RefCustID = strRefCustID;
                                                            if (strInActive == "X")
                                                                existCust.Status = inActiveStatus;
                                                            else
                                                                existCust.Status = activeStatus;

                                                            if (lat > 0 && lng > 0)
                                                            {
                                                                updateCustomerLocation(strEBranchID, strShopID, lat, lng);
                                                            }
                                                            else if (lat > 0 || lng > 0)
                                                            {
                                                                lineInvalidGeo.Add((i - dataRowIdx + 1).ToString());
                                                            }

                                                            lineSuccess.Add((i - dataRowIdx + 1).ToString());
                                                            #endregion
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
                                                && !string.IsNullOrWhiteSpace(Territory)
                                                && !string.IsNullOrWhiteSpace(strDistrict)
                                                && !string.IsNullOrWhiteSpace(strProvince)
                                                && !string.IsNullOrWhiteSpace(Channel)
                                                && !string.IsNullOrWhiteSpace(strCustClass)
                                                && !string.IsNullOrWhiteSpace(Chain)
                                                && !string.IsNullOrWhiteSpace(strShopType)
                                                && !string.IsNullOrWhiteSpace(Classification)
                                               // && !string.IsNullOrWhiteSpace(strLocation)
                                                )
                                            {
                                                if (_checkRequireImport == true && string.IsNullOrWhiteSpace(strShopType))
                                                {
                                                    continue;
                                                }
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
                                                            if (autoCustID == true)
                                                                strShopID = _db.OM23800_CustID(strEBranchID, "", Territory, strDistrict, Channel, Chain, "", "", "", "", strCustClass, strProvince, strShopName).FirstOrDefault().ToString();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var existCust = _db.AR_Customer.FirstOrDefault(c => c.CustName == strShopName
                                                                                && c.Addr1 == StreetName
                                                                                && c.Addr2 == StreetNo
                                                                                && c.Ward == Ward
                                                                                && c.District == strDistrict
                                                                                && c.State == strProvince
                                                                                && c.BranchID == strEBranchID);
                                                        if (existCust != null)
                                                        {
                                                            lineExist.Add((i - dataRowIdx + 1).ToString());
                                                            canInsert = false;
                                                        }
                                                        else
                                                        {
                                                            if (autoCustID == true)
                                                            {
                                                                strShopID = _db.OM23800_CustID(strEBranchID, "", Territory, strDistrict, Channel, Chain, "", "", "", "", strCustClass, strProvince, strShopName).FirstOrDefault().ToString();
                                                            }
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
                                                        #region -Insert new Cust-
                                                        var newCust = new AR_Customer();
                                                        newCust.ResetET();
                                                        newCust.ExpiryDate = DateTime.Now.ToDateShort();
                                                        newCust.Birthdate = new DateTime(1900, 1, 1).ToDateShort();
                                                        newCust.EstablishDate = new DateTime(1900, 1, 1).ToDateShort();
                                                        newCust.CustId = strShopID;
                                                        newCust.BranchID = strEBranchID;
                                                        newCust.CustName = newCust.BillName = strShopName;
                                                        newCust.Attn = newCust.BillAttn = strAttn;
                                                        newCust.BusinessName = strBusName;
                                                        newCust.Addr1 = newCust.BillAddr1 = StreetName;
                                                        newCust.Addr2 = newCust.BillAddr2 = StreetNo;
                                                        newCust.District = strDistrict;
                                                        newCust.State = newCust.BillState = strProvince;
                                                        newCust.Phone = newCust.BillPhone = strPhone;
                                                        newCust.Country = newCust.BillCountry = strCountry;
                                                        newCust.City = newCust.BillCity = strCity;
                                                        newCust.Territory = Territory;
                                                        newCust.ClassId = strCustClass;
                                                        //newCust.Location = strLocation;
                                                        newCust.CrRule = "N";
                                                        newCust.CustType = "R";
                                                        newCust.DfltShipToId = "DEFAULT";
                                                        newCust.NodeLevel = 2;
                                                        newCust.ParentRecordID = 4;
                                                        if (strInActive == "X")
                                                            newCust.Status = inActiveStatus;
                                                        else
                                                            newCust.Status = activeStatus;
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
                                                        newCust.SellProduct = "";
                                                      //  newCust.Location = strLocation;
                                                        newCust.DeliveryUnit = DeliveryUnit;
                                                        newCust.SalesProvince = SalesProvince;

                                                        newCust.RefCustID = strRefCustID;

                                                        newCust.Ward = Ward;
                                                        newCust.Territory = Territory;
                                                        newCust.Channel = Channel;
                                                        newCust.ClassId = strCustClass;
                                                        newCust.Chain = Chain;
                                                        newCust.ShopType = strShopType;
                                                        newCust.Classification = Classification;
                                                       // newCust.Location = strLocation;

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
                                                        #endregion
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //lineBlank.Add((i - dataRowIdx + 1).ToString());
                                            }
                                        }
                                        #region import MCP
                                        //if (slsright == true)
                                        //{
                                        //    strECustID = strShopID;
                                        //    try
                                        //    {
                                        //        startDate = workSheet.Cells[i, ColTexts.IndexOf("StartDate")].DateTimeValue.ToDateShort();// DateTime.FromOADate(double.Parse(workSheet.Cells[i, 6].StringValue)).Short();
                                        //        endDate = workSheet.Cells[i, ColTexts.IndexOf("EndDate")].DateTimeValue.ToDateShort(); //DateTime.FromOADate(double.Parse(workSheet.Cells[i, 7].StringValue)).Short();

                                        //    }
                                        //    catch
                                        //    {
                                        //        messageDate += string.Format(Message.GetString("2016082906", null), (i - dataRowIdx + 1).ToString());
                                        //        continue;

                                        //    }

                                        //    OM_SalesRouteMasterImport objImport = new OM_SalesRouteMasterImport();
                                        //    //  bool isNew = false;
                                        //    var recordExists = lstOM_SalesRouteMasterImport.FirstOrDefault(p => p.ID == id
                                        //                                                                    && p.BranchID == BranchID
                                        //                                                                    && p.PJPID == pJPID
                                        //                                                                    && p.SalesRouteID == strERouteID
                                        //                                                                    && p.CustID == strECustID
                                        //                                                                    && p.SlsPerID == strESlsperID);
                                        //    if (recordExists == null)
                                        //    {
                                        //        lstCustomer += strECustID + ";";
                                        //        if (_db.OM_SalesRouteMasterImport.Where(p => p.ID == id
                                        //                                                && p.BranchID == BranchID
                                        //                                                && p.PJPID == pJPID
                                        //                                                && p.SalesRouteID == strERouteID
                                        //                                                && p.CustID == strShopID
                                        //                                                && p.SlsPerID == strESlsperID).ToList().Count == 0)
                                        //        {
                                        //            objImport.ID = id;
                                        //            objImport.BranchID = BranchID;
                                        //            objImport.PJPID = pJPID;
                                        //            objImport.SalesRouteID = strERouteID;
                                        //            objImport.CustID = strShopID;
                                        //            objImport.SlsPerID = strESlsperID;
                                        //            objImport.StartDate = startDate;
                                        //            objImport.EndDate = endDate; ;
                                        //            objImport.SlsFreq = strETS;//  dataArray.GetValue(i, 9).ToString().Trim().ToUpper();
                                        //            objImport.SlsFreqType = "R";
                                        //            objImport.WeekofVisit = strETBH;// dataArray.GetValue(i, 10).ToString().Trim().ToUpper();
                                        //            objImport.Mon = workSheet.Cells[i, ColTexts.IndexOf("Mon")].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 11) == null ? false : dataArray.GetValue(i, 11).ToString().Trim().ToUpper() == "X" ? true : false;
                                        //            objImport.Tue = workSheet.Cells[i, ColTexts.IndexOf("Tue")].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 12) == null ? false : dataArray.GetValue(i, 12).ToString().Trim().ToUpper() == "X" ? true : false;
                                        //            objImport.Wed = workSheet.Cells[i, ColTexts.IndexOf("Wed")].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 13) == null ? false : dataArray.GetValue(i, 13).ToString().Trim().ToUpper() == "X" ? true : false;
                                        //            objImport.Thu = workSheet.Cells[i, ColTexts.IndexOf("Thu")].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 14) == null ? false : dataArray.GetValue(i, 14).ToString().Trim().ToUpper() == "X" ? true : false;
                                        //            objImport.Fri = workSheet.Cells[i, ColTexts.IndexOf("Fri")].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 15) == null ? false : dataArray.GetValue(i, 15).ToString().Trim().ToUpper() == "X" ? true : false;
                                        //            objImport.Sat = workSheet.Cells[i, ColTexts.IndexOf("Sat")].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 16) == null ? false : dataArray.GetValue(i, 16).ToString().Trim().ToUpper() == "X" ? true : false;
                                        //            objImport.Sun = workSheet.Cells[i, ColTexts.IndexOf("Sun")].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 17) == null ? false : dataArray.GetValue(i, 17).ToString().Trim().ToUpper() == "X" ? true : false;
                                        //            try
                                        //            {
                                        //                objImport.VisitSort = workSheet.Cells[i, ColTexts.IndexOf("VisitSort")].IntValue;// dataArray.GetValue(i, 20) == null ? 0 : dataArray.GetValue(i, 20).ToString().Trim().ToUpper() == "" ? 0 : int.Parse(dataArray.GetValue(i, 20).ToString().Trim().ToUpper());
                                        //            }
                                        //            catch
                                        //            {
                                        //                objImport.VisitSort = 0;
                                        //            }
                                        //            objImport.LUpd_DateTime = objImport.LUpd_DateTime = DateTime.Now;
                                        //            objImport.LUpd_Prog = objImport.LUpd_Prog = _screenName;
                                        //            objImport.LUpd_User = objImport.LUpd_User = Current.UserName;
                                        //            objImport.Crtd_DateTime = objImport.Crtd_DateTime = DateTime.Now;
                                        //            objImport.Crtd_Prog = objImport.Crtd_Prog = _screenName;
                                        //            objImport.Crtd_User = objImport.Crtd_User = Current.UserName;
                                        //            if (isValidSelOMSalesRouteMaster(objImport, false))
                                        //            {

                                        //                if (!lstWeekOfVisit.Any(x => x.SlsFreq == objImport.SlsFreq && x.WeekofVisit == objImport.WeekofVisit))
                                        //                {
                                        //                    messgeWeekOfVistit += (i - dataRowIdx + 1).ToString() + ",";
                                        //                }
                                        //                else
                                        //                {
                                        //                    if (workSheet.Cells[i, ColTexts.IndexOf("CustCancel")].StringValue != null && workSheet.Cells[i, ColTexts.IndexOf("CustCancel")].StringValue == "X")
                                        //                    {
                                        //                        objImport.Del = true;

                                        //                    }
                                        //                    _db.OM_SalesRouteMasterImport.AddObject(objImport);
                                        //                    lstOM_SalesRouteMasterImport.Add(objImport);
                                        //                }
                                        //            }
                                        //            else
                                        //            {
                                        //                messageerror += (i - dataRowIdx + 1).ToString() + ",";
                                        //                //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu không hợp lệ" + "\r";
                                        //            }

                                        //        }
                                        //        else messageduplicate += (i - dataRowIdx + 1).ToString() + ",";  //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu bi trùng" + "\r";
                                        //    }
                                        //    else
                                        //        messageduplicatefile += (i - dataRowIdx + 1).ToString() + ",";
                                        //}
                                        #endregion
                                    }
                                }// flagCheckError = false
                            }
                            //if (isUpdated)
                            //{
                                _db.SaveChanges();
                           // }

                            //DataAccess dal = Util.Dal();
                            //try
                            //{

                            //    PJPProcess.PJP pjp = new PJPProcess.PJP(Current.UserName, "OM23800", dal);
                            //    dal.BeginTrans(IsolationLevel.ReadCommitted);
                            //    if (!pjp.OM23800CreateMCP(id))
                            //    {
                            //        dal.RollbackTrans();
                            //    }
                            //    else
                            //    {
                            //        dal.CommitTrans();
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    dal.RollbackTrans();
                            //    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                            //}

                            var numOfLine = 10;
                            #region -Error-
                            if (lineSuccess.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082907", null),
                                    lineSuccess.Count > numOfLine ? string.Join(", ", lineSuccess.Take(numOfLine)) + ", ..." : string.Join(", ", lineSuccess));
                            }
                            if (lineCustID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineCustID.Count > numOfLine ? string.Join(", ", lineCustID.Take(numOfLine)) + ", ..." : string.Join(", ", lineCustID), workSheet.Cells[3, ColTexts.IndexOf("ShopID")].StringValue);
                            }
                            if (lineSlsPerID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineSlsPerID.Count > numOfLine ? string.Join(", ", lineSlsPerID.Take(numOfLine)) + ", ..." : string.Join(", ", lineSlsPerID), workSheet.Cells[3, ColTexts.IndexOf("SlsperID")].StringValue);
                            }
                            if (lineShopID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineShopID.Count > numOfLine ? string.Join(", ", lineShopID.Take(numOfLine)) + ", ..." : string.Join(", ", lineShopID), workSheet.Cells[3, ColTexts.IndexOf("ShopID")].StringValue);
                            }
                            if (lineShopName.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineShopName.Count > numOfLine ? string.Join(", ", lineShopName.Take(numOfLine)) + ", ..." : string.Join(", ", lineShopName), workSheet.Cells[3, ColTexts.IndexOf("ShopName")].StringValue);
                            }
                            
                            //if (lineAttn.Count > 0)
                            //{
                            //    message += string.Format(Message.GetString("2016082912", null),
                            //        lineAttn.Count > numOfLine ? string.Join(", ", lineAttn.Take(numOfLine)) + ", ..." : string.Join(", ", lineAttn), workSheet.Cells[3, 5].StringValue);
                            //}
                            //if (lineStreetNo.Count > 0)
                            //{
                            //    message += string.Format(Message.GetString("2016082912", null),
                            //        lineStreetNo.Count > numOfLine ? string.Join(", ", lineStreetNo.Take(numOfLine)) + ", ..." : string.Join(", ", lineStreetNo), workSheet.Cells[3, 6].StringValue);
                            //}
                            //if (lineStreetName.Count > 0)
                            //{
                            //    message += string.Format(Message.GetString("2016082912", null),
                            //        lineStreetName.Count > numOfLine ? string.Join(", ", lineStreetName.Take(numOfLine)) + ", ..." : string.Join(", ", lineStreetName), workSheet.Cells[3, 7].StringValue);
                            //}
                            //if (lineWard.Count > 0)
                            //{
                            //    message += string.Format(Message.GetString("2016082912", null),
                            //        lineWard.Count > numOfLine ? string.Join(", ", lineWard.Take(numOfLine)) + ", ..." : string.Join(", ", lineWard), workSheet.Cells[3, 8].StringValue);
                            //}
                            if (lineTerritory.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineTerritory.Count > numOfLine ? string.Join(", ", lineTerritory.Take(numOfLine)) + ", ..." : string.Join(", ", lineTerritory), workSheet.Cells[3, ColTexts.IndexOf("Territory")].StringValue);
                            }
                            if (lineProvince.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineProvince.Count > numOfLine ? string.Join(", ", lineProvince.Take(numOfLine)) + ", ..." : string.Join(", ", lineProvince), workSheet.Cells[3, ColTexts.IndexOf("Province")].StringValue);
                            }
                            if (lineDistrict.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineDistrict.Count > numOfLine ? string.Join(", ", lineDistrict.Take(numOfLine)) + ", ..." : string.Join(", ", lineDistrict), workSheet.Cells[3, ColTexts.IndexOf("District")].StringValue);
                            }
                            if (lineChannel.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineChannel.Count > numOfLine ? string.Join(", ", lineChannel.Take(numOfLine)) + ", ..." : string.Join(", ", lineChannel), workSheet.Cells[3, ColTexts.IndexOf("Channel")].StringValue);
                            }
                            if (lineCustClass.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineCustClass.Count > numOfLine ? string.Join(", ", lineCustClass.Take(numOfLine)) + ", ..." : string.Join(", ", lineCustClass), workSheet.Cells[3, ColTexts.IndexOf("CustClass")].StringValue);
                            }
                            //if (lineChain.Count > 0)
                            //{
                            //    message += string.Format(Message.GetString("2016082912", null),
                            //        lineChain.Count > numOfLine ? string.Join(", ", lineChain.Take(numOfLine)) + ", ..." : string.Join(", ", lineChain), workSheet.Cells[3, 15].StringValue);
                            //}
                            if (lineShopType.Count > 0 && _checkRequireImport == true)
                            {
                                message += string.Format(Message.GetString("2016082912", null),
                                    lineShopType.Count > numOfLine ? string.Join(", ", lineShopType.Take(numOfLine)) + ", ..." : string.Join(", ", lineShopType), workSheet.Cells[3, ColTexts.IndexOf("ShopType")].StringValue);
                            }
                            //if (lineClassification.Count > 0)
                            //{
                            //    message += string.Format(Message.GetString("2016082912", null),
                            //        lineClassification.Count > numOfLine ? string.Join(", ", lineClassification.Take(numOfLine)) + ", ..." : string.Join(", ", lineClassification), workSheet.Cells[3, 17].StringValue);
                            //}
                            //if (lineLocation.Count > 0)
                            //{
                            //    message += string.Format(Message.GetString("2016082912", null),
                            //        lineLocation.Count > numOfLine ? string.Join(", ", lineLocation.Take(numOfLine)) + ", ..." : string.Join(", ", lineLocation), workSheet.Cells[3, 18].StringValue);
                            //}

                            if (lineExist.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082908", null),
                                    lineExist.Count > numOfLine ? string.Join(", ", lineExist.Take(numOfLine)) + ", ..." : string.Join(", ", lineExist));
                            }
                            if (lineNoExist.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082909", null),
                                    lineNoExist.Count > numOfLine ? string.Join(", ", lineNoExist.Take(numOfLine)) + ", ..." : string.Join(", ", lineNoExist));
                            }
                            if (lineSlsNoExist.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082910", null),
                                    lineSlsNoExist.Count > numOfLine ? string.Join(", ", lineSlsNoExist.Take(numOfLine)) + ", ..." : string.Join(", ", lineSlsNoExist));
                            }

                            // 2016091413 {0} Line: {1} have not exists</br>

                            if (lineExistRefCustID.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016090701", null),
                                    lineExistRefCustID.Count > numOfLine ? string.Join(", ", lineExistRefCustID.Take(numOfLine)) + ", ..." : string.Join(", ", lineExistRefCustID));
                            }
                            if (lineInvalidDistrict.Count > 0)
                            {
                                message += string.Format(Message.GetString("2016082911", null),
                                    lineInvalidDistrict.Count > numOfLine ? string.Join(", ", lineInvalidDistrict.Take(numOfLine)) + ", ..." : string.Join(", ", lineInvalidDistrict));
                            }

                            message += messagestrETS == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrETS, workSheet.Cells[3, ColTexts.IndexOf("SlsFreq")].StringValue);
                            message += messagestrETBH == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrETBH, workSheet.Cells[3, ColTexts.IndexOf("WeekofVisit")].StringValue);
                            message += messagestrEBeginDate == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrEBeginDate, workSheet.Cells[3, ColTexts.IndexOf("StartDate")].StringValue);
                            message += messagestrEEndDate == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrEEndDate, workSheet.Cells[3, ColTexts.IndexOf("EndDate")].StringValue);
                            message += messageDate == "" ? "" : string.Format(Message.GetString("2016082906", null), messageDate.ToString());
                            message += messagestrERouteID == "" ? "" : string.Format(Message.GetString("2016082912", null), messagestrERouteID, workSheet.Cells[3, ColTexts.IndexOf("SalesRouteID")].StringValue);
                            message += messgeWeekOfVistit == "" ? "" : string.Format(Message.GetString("2017040301", null), messgeWeekOfVistit);
                            message += messageerror == "" ? "" : string.Format(Message.GetString("2016082913", null), messageerror);
                            message += messageduplicate == "" ? "" : string.Format(Message.GetString("2016082903", null), messageduplicate);
                            message += messageduplicatefile == "" ? "" : string.Format(Message.GetString("2016082903", null), messageduplicatefile);
               
                            #endregion
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
        // Check set Background
        private bool CheckSetHeaderColor(int index)
        {
            if (index < 1 || index == 17 || index >= 19)
            {
                return false;
            }
            return true;
        }

        private bool checkRefCustID(string key)
        {
            key = key.ToUpper();
            if (_lstRefCustID.Any(x => x == key))
            {
                return false;
            }
            else
            {
                _lstRefCustID.Add(key);  
            }
            return true;
        }

        private Validation GetValidation(ref Worksheet SheetMCP, string formular1, string inputMess, string errMess)
        {
            var validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
            validation.IgnoreBlank = true;
            validation.Type = Aspose.Cells.ValidationType.List;
            validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
            validation.Operator = OperatorType.Between;
            validation.Formula1 = formular1;
            validation.InputTitle = "";
            validation.InputMessage = inputMess;
            validation.ErrorMessage = errMess;
            return validation;
        }
        private CellArea GetCellArea(int startRow, int endRow, int columnIndex, int endColumnIndex = -1)
        {
            var area = new CellArea();
            area.StartRow = startRow;
            area.EndRow = endRow;
            area.StartColumn = columnIndex;
            area.EndColumn = endColumnIndex == -1 ? columnIndex : endColumnIndex;
            return area;
        }
        #endregion


        #region -Update Customer-

        #region -Get data-
        public ActionResult GetAR_Customer(string branchID, string custID)
        {
            var objCust = _db.AR_Customer.FirstOrDefault(x => x.CustId == custID && x.BranchID == branchID);
            return this.Store(objCust);
        }
        #endregion
        
        [HttpPost]
        public ActionResult SaveCustomer(FormCollection data, string CustId)
        {
            try
            {
                string isNew = "false";
                //string CustId = data["cboAddCustID"].PassNull();
                string BranchID = data["cboAddBranchID"].PassNull();
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["objHeader"]);
                var curHeader = dataHandler1.ObjectData<AR_Customer>().FirstOrDefault();

                var objAR_Setup = _db.AR_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupId == "AR");
                if (objAR_Setup != null)
                {
                    if (!objAR_Setup.AutoCustID && CustId == "")
                    {
                        throw new MessageException(MessageType.Message, "15", "", parm: new string[] { Util.GetLang("CustId") });
                    }
                    var objRefCust = _db.AR_Customer.FirstOrDefault(x => x.BranchID == BranchID && x.CustId != curHeader.CustId && x.RefCustID.ToUpper() == curHeader.RefCustID);
                    if (objRefCust != null)
                    {
                        throw new MessageException(MessageType.Message, "8001", "", parm: new string[] { Util.GetLang("RefCustID") });
                    }
                    var header = _db.AR_Customer.FirstOrDefault(p => p.BranchID == BranchID && p.CustId.ToUpper() == CustId.ToUpper());
                    if (header == null)
                    {
                        header = new AR_Customer();
                        header.ResetET();
                        if (objAR_Setup.AutoCustID == true)
                        {
                            CustId = _db.OM23800_ppNewCustID(BranchID, "", "", "", "", "", "", "", "", "", curHeader.ClassId, curHeader.CustName, curHeader.State, curHeader.District).FirstOrDefault();                            
                        }
                        header.CustId = CustId;                                                    
                        header.BranchID = BranchID;
                        UpdatingHeader(ref header, curHeader, true);
                        _db.AR_Customer.AddObject(header);                       
                    }
                    else
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(ref header, curHeader, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    #region Upload files
                    var files = Request.Files;
                    string filePath = GetFilePath();
                    if (files.Count > 0) // Co chon file de upload
                    {
                        if (files[0].ContentLength > 0)
                        {
                            string midPath = string.Format("{0}\\{1}\\{2}", header.BranchID, header.SlsperId, header.Crtd_Datetime.ToString("yyyyMM"));

                            Random rand = new Random();
                            string newFolder = filePath.TrimEnd(new char[] { '\\' }) + "\\" + midPath;
                            if (!Directory.Exists(newFolder))
                            {
                                Directory.CreateDirectory(newFolder);
                            }
                            string newFileName = CustId + rand.Next(1000, 9999) + Path.GetExtension(files[0].FileName);
                            Util.UploadFile(newFolder, newFileName, files[0]);
                            try
                            {
                                string oldFile = filePath.TrimEnd(new char[] { '\\' }) + "\\" + header.ProfilePic;
                                if (System.IO.File.Exists(oldFile))
                                {
                                    System.IO.File.Delete(oldFile);
                                }
                            }
                            catch
                            {

                            }
                            header.ProfilePic = midPath + "\\" + newFileName;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(header.ProfilePic) && string.IsNullOrWhiteSpace(curHeader.ProfilePic))
                            {

                                Util.UploadFile(filePath, header.ProfilePic, null);
                                header.ProfilePic = string.Empty;
                            }
                        }


                        if (files[1] != null && files[1].ContentLength > 0)
                        {
                            string midPath = string.Format("{0}\\{1}\\{2}", header.BranchID, header.SlsperId, header.Crtd_Datetime.ToString("yyyyMM"));

                            Random rand = new Random();
                            string newFolder = filePath.TrimEnd(new char[] { '\\' }) + "\\" + midPath;
                            if (!Directory.Exists(newFolder))
                            {
                                Directory.CreateDirectory(newFolder);
                            }
                            string newFileName = CustId + rand.Next(1000, 9999) + Path.GetExtension(files[1].FileName);
                            Util.UploadFile(newFolder, newFileName, files[1]);
                            try
                            {
                                string oldFile = filePath.TrimEnd(new char[] { '\\' }) + "\\" + header.BusinessPic;
                                if (System.IO.File.Exists(oldFile))
                                {
                                    System.IO.File.Delete(oldFile);
                                }
                            }
                            catch
                            {

                            }
                            header.BusinessPic = midPath + "\\" + newFileName;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(header.BusinessPic) && string.IsNullOrWhiteSpace(curHeader.BusinessPic))
                            {

                                Util.UploadFile(filePath, header.ProfilePic, null);
                                header.BusinessPic = string.Empty;
                            }
                        }
                                           
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(header.ProfilePic) && string.IsNullOrWhiteSpace(curHeader.ProfilePic))
                        {

                            Util.UploadFile(filePath, header.ProfilePic, null);
                            header.ProfilePic = string.Empty;
                        }

                        if (!string.IsNullOrWhiteSpace(header.BusinessPic) && string.IsNullOrWhiteSpace(curHeader.BusinessPic))
                        {

                            Util.UploadFile(filePath, header.BusinessPic, null);
                            header.BusinessPic = string.Empty;
                        }
                    }
                    #endregion
                }

                _db.SaveChanges();
                return Json(new { success = true, CustId = CustId, isNew = isNew });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        
        private void UpdatingHeader(ref AR_Customer objUpdate, AR_Customer objTmp, bool isNew)
        {            
            //if (objUpdate.Status == "O")
            //{
            //    X.Msg.Show(new MessageBoxConfig()
            //    {
            //        Message = "Email sent!"
            //    });
            //    var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            //    Approve.Mail_Approve(_screenName, objUpdate.CustId, user.UserTypes, objUpdate.Status, Handle, Current.LangID.ToString()
            //                 , Current.UserName, objUpdate.BranchID, Current.CpnyID, string.Empty, string.Empty, string.Empty);
            //}            
            objUpdate.ClassId = objTmp.ClassId;
            objUpdate.CustType = objTmp.CustType;
            objUpdate.CustName = objTmp.CustName;
            objUpdate.PriceClassID = objTmp.PriceClassID;
            objUpdate.Terms = objTmp.Terms;
            objUpdate.TradeDisc = objTmp.TradeDisc;
            objUpdate.CrRule = objTmp.CrRule;
            objUpdate.CrLmt = objTmp.CrLmt;
            objUpdate.GracePer = objTmp.GracePer;
            objUpdate.Territory = objTmp.Territory;
            objUpdate.Area = objTmp.Area;
            objUpdate.Location = objTmp.Location;
            objUpdate.Channel = objTmp.Channel;
            objUpdate.ShopType = objTmp.ShopType;
            objUpdate.GiftExchange = objTmp.GiftExchange;
            objUpdate.HasPG = objTmp.HasPG;
            objUpdate.SlsperId = objTmp.SlsperId;
            objUpdate.DeliveryID = objTmp.DeliveryID;
            objUpdate.SupID = objTmp.SupID;
            objUpdate.SiteId = objTmp.SiteId;
            objUpdate.DfltShipToId = objTmp.DfltShipToId == "" ? "DEFAULT" : objTmp.DfltShipToId;
            objUpdate.CustFillPriority = objTmp.CustFillPriority;
            objUpdate.LTTContractNbr = objTmp.LTTContractNbr;
            objUpdate.DflSaleRouteID = objTmp.DflSaleRouteID;
            objUpdate.EmpNum = objTmp.EmpNum;
            objUpdate.ExpiryDate = objTmp.ExpiryDate.ToDateShort().PassMin();
            objUpdate.EstablishDate = objTmp.EstablishDate.ToDateShort().PassMin();
            objUpdate.Birthdate = objTmp.Birthdate.ToDateShort().PassMin();

            objUpdate.Attn = objTmp.Attn;
            objUpdate.Salut = objTmp.Salut;
            objUpdate.Addr1 = objTmp.Addr1;
            objUpdate.Addr2 = objTmp.Addr2;
            objUpdate.Country = objTmp.Country;
            objUpdate.State = objTmp.State;
            objUpdate.City = objTmp.City;
            objUpdate.District = objTmp.District;
            objUpdate.Zip = objTmp.Zip;
            objUpdate.Phone = objTmp.Phone;
            objUpdate.Fax = objTmp.Fax;
            objUpdate.EMailAddr = objTmp.EMailAddr;

            objUpdate.BillName = objTmp.BillName;
            objUpdate.BillAttn = objTmp.BillAttn;
            objUpdate.BillSalut = objTmp.BillSalut;
            objUpdate.BillAddr1 = objTmp.BillAddr1;
            objUpdate.BillAddr2 = objTmp.BillAddr2;
            objUpdate.BillCountry = objTmp.BillCountry;
            objUpdate.BillState = objTmp.BillState;
            objUpdate.BillCity = objTmp.BillCity;
            objUpdate.BillZip = objTmp.BillZip;
            objUpdate.BillPhone = objTmp.BillPhone;
            objUpdate.BillFax = objTmp.BillFax;
            objUpdate.TaxDflt = objTmp.TaxDflt;
            objUpdate.TaxRegNbr = objTmp.TaxRegNbr;
            
            objUpdate.InActive = objTmp.InActive;
            objUpdate.SellProduct = objTmp.SellProduct;
            objUpdate.DeliveryUnit = objTmp.DeliveryUnit;
            objUpdate.SalesProvince = objTmp.SalesProvince;
            objUpdate.Chain = objTmp.Chain;
            objUpdate.Ward = objTmp.Ward;
            objUpdate.Classification = objTmp.Classification;
            objUpdate.LUpd_Datetime = DateTime.Now;
            objUpdate.LUpd_Prog = _screenName;
            objUpdate.LUpd_User = Current.UserName;
            objUpdate.BusinessName = objTmp.BusinessName;
            //objUpdate.BusinessPic = objTmp.BusinessPic;
            objUpdate.RefCustID = objTmp.RefCustID;
            if (isNew)
            {
                objUpdate.NodeID = "DF";
                objUpdate.NodeLevel = 1;
                objUpdate.ParentRecordID = 0;               
                
                objUpdate.Crtd_Prog = _screenName;
                objUpdate.Crtd_User = Current.UserName;
                objUpdate.Crtd_Datetime = DateTime.Now;

                var obj = _db.OM23800_pdGetDefaultData4AddCust(objTmp.ClassId, objUpdate.CustId, objUpdate.BranchID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (obj != null)
                {
                    objUpdate.TaxID00 = obj.TaxID00;
                    objUpdate.TaxID01 = obj.TaxID01;
                    objUpdate.TaxID02 = obj.TaxID02;
                    objUpdate.TaxID03 = obj.TaxID03;
                    objUpdate.TaxDflt = obj.TaxDflt;
                    objUpdate.Country = obj.Country;
                    objUpdate.Terms = obj.Terms;
                }
                objUpdate.TaxLocId = objTmp.TaxLocId;


                objUpdate.BillName = objTmp.CustName;
                objUpdate.BillAttn = objTmp.Attn;
                objUpdate.BillSalut = objTmp.Salut;
                objUpdate.BillAddr1 = objTmp.Addr1;
                objUpdate.BillAddr2 = objTmp.Addr2;
                objUpdate.BillCountry = objTmp.Country;
                objUpdate.BillState = objTmp.State;
                objUpdate.BillCity = objTmp.City;
                objUpdate.BillZip = objTmp.BillZip;
                objUpdate.BillPhone = objTmp.Phone;
                objUpdate.BillFax = objTmp.Fax;
            }
            if (objUpdate.Status == string.Empty && objTmp.Status == string.Empty)
            {
                 objUpdate.Status = "A";
            }
            else
            {
                objUpdate.Status = objTmp.Status;
            }
        }

         [HttpPost]
        public ActionResult DeleteCustomer(FormCollection data)
        {
            try
            {
                string custID = data["cboAddCustID"].PassNull().ToUpper();
                string branchID = data["cboAddBranchID"].PassNull();
                var objDelete = _db.OM23800_ppCheckDeleteCust(custID, branchID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (objDelete != null)
                {
                    if (objDelete.PassNull() == "0")
                    {
                        var objAR_Customer = _db.AR_Customer.FirstOrDefault(p => p.BranchID == branchID
                                                                   && p.CustId.ToUpper() == custID);
                        if (objAR_Customer != null)
                        {
                            _db.AR_Customer.DeleteObject(objAR_Customer);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "18", "");
                    }
                }
                return Json(new { success = true, CustId = "" });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        #endregion

         #region -File-
                  
         public ActionResult ImageToBin(string fileName)
         {
             try
             {
                 var imgString64 = Util.ImageToBin(GetFilePath(), fileName);
                 var jsonResult = Json(new { success = true, imgSrc = imgString64 }, JsonRequestBehavior.AllowGet);
                 jsonResult.MaxJsonLength = int.MaxValue;
                 return jsonResult;
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

         private string GetFilePath()
         {
            var filePath = string.Empty;
            var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "UPLOADAR20400");
            if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
            {
                filePath = config.TextVal;
            }
            else
            {
                filePath = Server.MapPath("~\\Images\\AR20400");
            }
            return filePath;
         }
         #endregion


        #region -Tree-
         [DirectMethod]
         public ActionResult OM23800_GetTreeData()
         {
             #region -Declare-
             _db = Util.CreateObjectContext<OM23800Entities>(false);
             _sys = Util.CreateObjectContext<HQ.eSkySys.eSkySysEntities>(true);
             Panel panel = this.GetCmp<Panel>("pnlTreeAVC");
             //TextField txt = this.GetCmp<TextField>("txtAllCurrentSalesman");
             string _allCurrentSalesman = "";
             TreePanel tree = new TreePanel();
             tree.ID = "treeAVC";
             tree.Fields.Add(new ModelField("Data", ModelFieldType.String));
             tree.Fields.Add(new ModelField("Color", ModelFieldType.String));
             tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
             tree.Fields.Add(new ModelField("SlsperID", ModelFieldType.String));
             tree.Fields.Add(new ModelField("CpnyID", ModelFieldType.String));

             tree.Fields.Add(new ModelField("LeafNode", ModelFieldType.Boolean));


             tree.AutoScroll = true;
             tree.Scroll = ScrollMode.Both;
             tree.RootVisible = true;

             tree.Border = false;
             tree.Header = false;
             tree.Listeners.CheckChange.Fn = "tree_CheckChange";
             //tree.Listeners.Load.Fn = "setValueAllCurrentSalesFirsrLoad";
             //tree.Listeners.SelectionChange.Fn = "treeAVC_SelectionChange";
             #endregion

             
             var branchFirst = false;
             if (!branchFirst) // Show Node Branch first
             {
                 bool isChecked = true;
                 Node node = new Node();
                 node.Checked = isChecked;
                 node.NodeID = "tree-node-root-ns";
                 Random rand = new Random();
                 
                 var lstParent = _db.OM23800_ptParentNode(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                 var lstS = _db.OM23800_ptBranchSalesperson(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                 var lstSS = lstParent.Where(x => x.Level == 0).ToList();
                 var lstAM = lstParent.Where(x => x.Level == 1).ToList();

                 var lstRM = lstParent.Where(x => x.Level == 2).ToList();
                 var lstNS = lstParent.Where(x => x.Level == 3).ToList();


                 if (lstRM.Count > 0 && lstNS.Count > 0)
                 {
                     foreach (var ns in lstNS)
                     {
                         var nodeNS = SetNodeValue(ns, isChecked, "N", "NS", Ext.Net.Icon.UserHome);
                         _allCurrentSalesman += ns.Data + ",";

                         var lstRMInNS = lstRM.Where(x => x.SupID == ns.SlsperID).ToList();
                         foreach (var rm in lstRMInNS)
                         {
                             var nodeRM = SetNodeValue(rm, isChecked, "R", "RM", Ext.Net.Icon.UserOrange);
                             _allCurrentSalesman += rm.Data + ",";


                             var lstAMInRm = lstAM.Where(x => x.SupID == rm.SlsperID).ToList();
                             foreach (var am in lstAMInRm)
                             {
                                 #region -ASM-
                                 var nodeAM = SetNodeValue(am, isChecked, "M", "AM", Ext.Net.Icon.UserMature);
                                 _allCurrentSalesman += am.Data + ",";

                                 var lstSSInAM = lstSS.Where(x => x.SupID == am.SlsperID).ToList();
                                 foreach (var ss in lstSSInAM)
                                 {
                                     #region -SS-

                                     var nodeSS = SetNodeValue(ss, isChecked, "S", "SS", Ext.Net.Icon.UserGreen);
                                     _allCurrentSalesman += ss.Data + ",";

                                     var lstSales = lstS.Where(x => x.SupID == ss.SlsperID).ToList();
                                     var lstBranch = lstSales.GroupBy(test => test.BranchID).Select(grp => grp.First()).ToList();
                                     foreach (var branch in lstBranch)
                                     {
                                         #region -Branch-
                                         var nodeBranch = SetBranchNodeValue(branch, isChecked, ss.SlsperID, "B");
                                         // _allCurrentSalesman += branch.BranchID + "###" + ",";                                                                

                                         var lstLeaf = lstSales.Where(x => x.BranchID == branch.BranchID).ToList();
                                         foreach (var taa in lstLeaf)
                                         {
                                             _allCurrentSalesman += taa.Data + ",";
                                             nodeBranch.Children.Add(SetLeafNodeValue(taa, isChecked, "T", "S"));
                                         }
                                         nodeBranch.Leaf = lstLeaf.Count == 0 ? true : false;
                                         nodeSS.Children.Add(nodeBranch);
                                         #endregion
                                     }
                                     nodeSS.Leaf = lstBranch.Count == 0 ? true : false;
                                     nodeAM.Children.Add(nodeSS);
                                     #endregion
                                 }
                                 #endregion

                                 nodeAM.Leaf = lstSSInAM.Count == 0 ? true : false;
                                 nodeRM.Children.Add(nodeAM);
                             }
                             nodeRM.Leaf = lstRMInNS.Count == 0 ? true : false;
                             nodeNS.Children.Add(nodeRM);
                         }
                         nodeNS.Leaf = lstRMInNS.Count == 0 ? true : false;
                         node.Children.Add(nodeNS);
                     }
                 }
                 else if (lstRM.Count > 0)
                 {
                     foreach (var rm in lstRM)
                     {
                         var nodeRM = SetNodeValue(rm, isChecked, "N", "RM", Ext.Net.Icon.UserOrange);
                         _allCurrentSalesman += rm.Data + ",";

                         var lstAMInRm = lstAM.Where(x => x.SupID == rm.SlsperID).ToList();
                         foreach (var am in lstAMInRm)
                         {
                             #region -ASM-
                             var nodeAM = SetNodeValue(am, isChecked, "M", "AM", Ext.Net.Icon.UserMature);
                             _allCurrentSalesman += am.Data + ",";

                             var lstSSInAM = lstSS.Where(x => x.SupID == am.SlsperID).ToList();
                             foreach (var ss in lstSSInAM)
                             {
                                 #region -SS-

                                 var nodeSS = SetNodeValue(ss, isChecked, "S", "SS", Ext.Net.Icon.UserGreen);
                                 _allCurrentSalesman += ss.Data + ",";

                                 var lstSales = lstS.Where(x => x.SupID == ss.SlsperID).ToList();
                                 var lstBranch = lstSales.GroupBy(test => test.BranchID).Select(grp => grp.First()).ToList();
                                 foreach (var branch in lstBranch)
                                 {
                                     #region -Branch-
                                     var nodeBranch = SetBranchNodeValue(branch, isChecked, ss.SlsperID, "B");
                                     // _allCurrentSalesman += branch.BranchID + "###" + ",";                                                                

                                     var lstLeaf = lstSales.Where(x => x.BranchID == branch.BranchID).ToList();
                                     foreach (var taa in lstLeaf)
                                     {
                                         _allCurrentSalesman += taa.Data + ",";
                                         nodeBranch.Children.Add(SetLeafNodeValue(taa, isChecked, "T", "S"));
                                     }
                                     nodeBranch.Leaf = lstLeaf.Count == 0 ? true : false;
                                     nodeSS.Children.Add(nodeBranch);
                                     #endregion
                                 }
                                 nodeSS.Leaf = lstBranch.Count == 0 ? true : false;
                                 nodeAM.Children.Add(nodeSS);
                                 #endregion
                             }
                             #endregion

                             nodeAM.Leaf = lstSSInAM.Count == 0 ? true : false;
                             nodeRM.Children.Add(nodeAM);
                         }

                         nodeRM.Leaf = lstAMInRm.Count == 0 ? true : false;
                         node.Children.Add(nodeRM);
                     }
                 }
                 else
                 {
                     foreach (var am in lstAM)
                     {
                         #region -ASM-
                         var nodeAM = SetNodeValue(am, isChecked, "M", "AM", Ext.Net.Icon.UserMature);
                         _allCurrentSalesman += am.Data + ",";

                         var lstSSInAM = lstSS.Where(x => x.SupID == am.SlsperID).ToList();
                         foreach (var ss in lstSSInAM)
                         {
                             #region -SS-

                             var nodeSS = SetNodeValue(ss, isChecked, "S", "SS", Ext.Net.Icon.UserGreen);
                             _allCurrentSalesman += ss.Data + ",";

                             var lstSales = lstS.Where(x => x.SupID == ss.SlsperID).ToList();
                             var lstBranch = lstSales.GroupBy(test => test.BranchID).Select(grp => grp.First()).ToList();
                             foreach (var branch in lstBranch)
                             {
                                 #region -Branch-
                                 var nodeBranch = SetBranchNodeValue(branch, isChecked, ss.SlsperID, "B");
                                 // _allCurrentSalesman += branch.BranchID + "###" + ",";                                                                

                                 var lstLeaf = lstSales.Where(x => x.BranchID == branch.BranchID).ToList();
                                 foreach (var taa in lstLeaf)
                                 {
                                     _allCurrentSalesman += taa.Data + ",";
                                     nodeBranch.Children.Add(SetLeafNodeValue(taa, isChecked, "T", "S"));
                                 }
                                 nodeBranch.Leaf = lstLeaf.Count == 0 ? true : false;
                                 nodeSS.Children.Add(nodeBranch);
                                 #endregion
                             }
                             nodeSS.Leaf = lstSales.Count == 0 ? true : false;
                             nodeAM.Children.Add(nodeSS);
                             #endregion
                         }
                         #endregion

                         nodeAM.Leaf = lstSSInAM.Count == 0 ? true : false;
                         node.Children.Add(nodeAM);
                     }
                 }
                 node.Icon = Ext.Net.Icon.FolderHome;

                 tree.Root.Add(node);

                 tree.AddTo(panel);
                 //txt.Value = _allCurrentSalesman;
             }
             tree.ExpandAll();
             return this.Direct();

         }

         private Node SetNodeValue(OM23800_ptParentNode_Result objNode, bool isChecked, string nodeType, string position, Ext.Net.Icon icon)
         {
             Node node = new Node();
             Random rand = new Random();
             string color = GetRandomColour(rand);
             node.Checked = isChecked;

             node.NodeID = objNode.Data;// "node-branch-sup-sales-position-" + '-' + objNode.SupID + '-' + objNode.SlsperID + '-' + position;// + item.CpnyID ;background-color:" + color
             node.Text = @"<span style=""color: #" + objNode.Color + "\">" + objNode.SlsperID + " - " + objNode.Name + "</span>";
             node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = nodeType, Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = objNode.Data, Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = color.Replace("#", ""), Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = objNode.SlsperID, Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = "", Mode = Ext.Net.ParameterMode.Value });
             node.Icon = icon;
             node.IconCls = "tree-node-noicon";
             return node;
         }

         private Node SetLeafNodeValue(OM23800_ptBranchSalesperson_Result objNode, bool isChecked, string nodeType, string position)
         {
             Node node = new Node();
             Random rand = new Random();
             string color = GetRandomColour(rand);
             node.Checked = isChecked;
             node.NodeID = objNode.Data;//"node-branch-sup-sales-position-" + objNode.BranchID + '-' + objNode.SupID + '-' + objNode.SlsperID + '-' + position;// + item.CpnyID ;background-color:" + color
             node.Text = @"<span style=""color: #" + objNode.Color + "\">" + objNode.SlsperID + " - " + objNode.Name + "</span>";
             node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = nodeType, Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = objNode.Data, Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = color.Replace("#", ""), Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = objNode.SlsperID, Mode = Ext.Net.ParameterMode.Value });
             node.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = objNode.BranchID, Mode = Ext.Net.ParameterMode.Value });
             node.Icon = Ext.Net.Icon.UserBrown;
             node.Leaf = true;
             node.IconCls = "tree-node-noicon";
             return node;
         }

         private Node SetBranchNodeValue(OM23800_ptBranchSalesperson_Result objNode, bool isChecked, string supID, string position)
         {
             Node nodeBranch = new Node();
             nodeBranch.Checked = isChecked;
             nodeBranch.NodeID = "node-branch-sup-sales-positon" + '-' + objNode.BranchID + '-' + supID + '-' + position;// Thêm SupID để khỏi trùng
             nodeBranch.Text = objNode.BranchID + " - " + objNode.BranchName;
             nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "B", Mode = Ext.Net.ParameterMode.Value });
             nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = objNode.BranchID + "###", Mode = Ext.Net.ParameterMode.Value });
             nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "SlsperID", Value = objNode.BranchID, Mode = Ext.Net.ParameterMode.Value });
             nodeBranch.CustomAttributes.Add(new ConfigItem() { Name = "CpnyID", Value = objNode.BranchID, Mode = Ext.Net.ParameterMode.Value });
             return nodeBranch;
         }

         private string GetRandomColour(Random rand)
         {
             var c = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
             return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
         }
        #endregion

        #region -Update data-

         [ValidateInput(false)]
         public ActionResult ResetGeo(FormCollection data)
         {
             try
             {
                 var listCustDone = new List<AR_CustomerLocation>();
                 var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                 var lstSelCust = selCustHandler.ObjectData<OM23800_pgMCL_Result>()
                             .Where(p => !string.IsNullOrWhiteSpace(p.CustId) && p.Selected == true).ToList();

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
                                    custLoc.LUpd_Datetime = DateTime.Now;
                                    custLoc.LUpd_Prog = _screenName;
                                    custLoc.LUpd_User = Current.UserName;
                                }
                                listCustDone.Add(custLoc);
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
                     return Json(new { success = true, msgCode = 201405071, listCustDone = Ext.Net.JSON.Serialize(listCustDone) });
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

         [ValidateInput(false)]
         public ActionResult UpdateAddress(FormCollection data)
         {
             try
             {
                 var listCustDone = new List<AR_Customer>();
                 var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                 var lstSelCust = selCustHandler.ObjectData<OM23800_pgMCL_Result>()
                             .Where(p => !string.IsNullOrWhiteSpace(p.CustId) && p.Selected == true).ToList();

                 if (lstSelCust.Count > 0)
                 {
                     foreach (var selCust in lstSelCust)
                     {
                         var cust = _db.AR_Customer.FirstOrDefault(p => p.CustId == selCust.CustId
                             && p.BranchID == selCust.BranchID);
                         if (cust != null)
                         {
                             if (cust.tstamp.ToHex() == selCust.CustTstamp.ToHex())
                             {
                                 if (cust.Addr1 != selCust.SuggestAddr && !string.IsNullOrWhiteSpace(selCust.SuggestAddr))
                                 {
                                     cust.Addr1 = selCust.SuggestAddr;

                                     cust.LUpd_Datetime = DateTime.Now;
                                     cust.LUpd_Prog = _screenName;
                                     cust.LUpd_User = Current.UserName;

                                     //var updateAR_Customer = _db.AR_CustomerLocation.FirstOrDefault(p => p.BranchID == selCust.BranchID && p.CustID == selCust.CustId);
                                     //if (updateAR_Customer != null)
                                     //{
                                     //    updateAR_Customer.LUpd_Datetime = cust.LUpd_Datetime;
                                     //    updateAR_Customer.LUpd_Prog = _screenName;
                                     //    updateAR_Customer.LUpd_User = Current.UserName;
                                     //}                                     
                                 }
                                 listCustDone.Add(cust);
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
                     return Json(new { success = true, msgCode = 201405071, listCustDone = Ext.Net.JSON.Serialize(listCustDone) });
                 }
                 else
                 {
                     throw new MessageException(MessageType.Message, "20412", "",
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

         [ValidateInput(false)]
         public ActionResult UpdateNewPosition(FormCollection data, string newLat, string newLng)
         {
             try
             {
                 // This is invariant
                 NumberFormatInfo format = new NumberFormatInfo();
                 // Set the 'splitter' for thousands
                 format.NumberGroupSeparator = ",";
                 // Set the decimal seperator
                 format.NumberDecimalSeparator = ".";

                 var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                 var selCust = selCustHandler.ObjectData<OM23800_pgMCL_Result>()
                             .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.CustId));
                 if (selCust != null)
                 {
                     var custLoc = _db.AR_CustomerLocation.FirstOrDefault(p => p.CustID == selCust.CustId
                             && p.BranchID == selCust.BranchID);
                     if (custLoc != null)
                     {
                         if (custLoc.tstamp.ToHex() == selCust.tstamp.ToHex())
                         {
                             custLoc.Lat = double.Parse(newLat, format);
                             custLoc.Lng = double.Parse(newLng, format);
                             custLoc.LUpd_Datetime = DateTime.Now;
                             custLoc.LUpd_Prog = _screenName;
                             custLoc.LUpd_User = Current.UserName;

                             _db.SaveChanges();
                             return Json(new { success = true, msgCode = 201405071, tstamp = Convert.ToBase64String(custLoc.tstamp) });
                         }
                         else
                         {
                             throw new MessageException(MessageType.Message, "19");
                         }
                     }
                     else
                     {
                         custLoc = new AR_CustomerLocation();
                         custLoc.CustID = selCust.CustId;
                         custLoc.BranchID = selCust.BranchID;
                         custLoc.Lat = double.Parse(newLat, format);
                         custLoc.Lng = double.Parse(newLng, format);

                         custLoc.Crtd_Datetime = custLoc.LUpd_Datetime = DateTime.Now;
                         custLoc.Crtd_Prog = custLoc.LUpd_Prog = _screenName;
                         custLoc.Crtd_User = custLoc.LUpd_User = Current.UserName;
                         _db.AR_CustomerLocation.AddObject(custLoc);

                         _db.SaveChanges();
                         return Json(new { success = true, msgCode = 201405071, tstamp = Convert.ToBase64String(custLoc.tstamp) });
                     }
                 }
                 else
                 {
                     throw new MessageException(MessageType.Message, "6");
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
        #endregion
    }
}

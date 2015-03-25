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

namespace OM30400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM30400Controller : Controller
    {
        private string _screenName = "OM30400";
        OM30400Entities _db = Util.CreateObjectContext<OM30400Entities>(false);

        //
        // GET: /OM30400/
        public ActionResult Index()
        {
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "name")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult LoadVisitCustomerPlan(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var planVisit = _db.OM30400_pgVisitCustomerPlan(Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(planVisit);
        }

        public ActionResult LoadMCL(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var mcl = _db.OM30400_pgLoadMCL(Current.CpnyID, Current.UserName, territory,
                province, distributor, channel, shopType,
                slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(mcl);
        }

        public ActionResult LoadGridActualVisit(string distributor, string slsperId, DateTime visitDate)
        {
            var actualVisit = _db.OM30400_pgGridActualVisit(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate).ToList();
            return this.Store(actualVisit);
        }

        public ActionResult LoadVisitCustomerActual(string distributor, string slsperId, DateTime visitDate)
        {
            var actualVisit = _db.OM30400_pgVisitCustomerActual(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate).ToList();
            return this.Store(actualVisit);
        }

        public ActionResult LoadAllCurrentSalesman(string distributor, DateTime visitDate)
        {
            var allSlspers = _db.OM30400_pgAllCurrentSalesman(Current.CpnyID, Current.UserName, distributor, visitDate).ToList();
            return this.Store(allSlspers);
        }

        public ActionResult LoadCustHistory(string distributor, string slsperId, string customer, DateTime startDate, DateTime endDate)
        {
            var cusHistory = _db.OM30400_pgHistory(Current.CpnyID, Current.UserName, distributor, slsperId, customer, startDate, endDate).ToList();
            return this.Store(cusHistory);
        }

        public ActionResult LoadSalesRouteMaster(string brachID, string custID, string slsPerID)
        {
            var slsRouteMster = _db.OM_SalesRouteMaster.FirstOrDefault(
                                    x => x.BranchID == brachID
                                    && x.CustID == custID
                                    && x.SlsPerID == slsPerID
                                    && x.SalesRouteID == brachID
                                    && x.PJPID == brachID);
            return this.Store(slsRouteMster);
        }

        public ActionResult SaveMcp(FormCollection data, bool custActive)
        {
            try
            {
                var custID = data["txtCustIDMcpInfo"];
                var slsperID = data["hdnSlsperIDMcpInfo"];
                var branchID = data["hdnBranchIDMcpInfo"];

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
                            #region MCP
                            var dataHandler = new StoreDataHandler(data["lstMcpInfo"]);
                            var lstMcpInfo = dataHandler.BatchObjectData<OM_SalesRouteMaster>();

                            foreach (var deleted in lstMcpInfo.Deleted)
                            {
                                var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == branchID
                                    && x.SalesRouteID == branchID
                                    && x.BranchID == branchID
                                    && x.SlsPerID == slsperID
                                    && x.CustID == custID);

                                if (obj != null)
                                {
                                    // xoa cu
                                    _db.OM_SalesRouteMaster.DeleteObject(obj);
                                    _db.SaveChanges();

                                    return Json(new
                                    {
                                        success = true,
                                        CustID = custID,
                                        SlsPerID = slsperID,
                                        BranchID = branchID,
                                        Color = "CCFF33",
                                        SlsFreq = "",
                                        WeekofVisit = "",
                                        VisitSort = "false",
                                        Sun = 0,
                                        Mon = 0,
                                        Tue = 0,
                                        Wed = 0,
                                        Thu = 0,
                                        Fri = 0,
                                        Sat = 0,
                                        Status = cust.Status
                                    });
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "8");
                                }
                            }

                            foreach (var updated in lstMcpInfo.Updated)
                            {
                                var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == branchID
                                    && x.SalesRouteID == branchID
                                    && x.BranchID == branchID
                                    && x.SlsPerID == slsperID
                                    && x.CustID == custID);

                                if (obj != null)
                                {
                                    if (obj.tstamp.ToHex() == updated.tstamp.ToHex())
                                    {
                                        // xoa cu, insert moi
                                        var newObj = new OM_SalesRouteMaster()
                                        {
                                            PJPID = obj.PJPID,
                                            SalesRouteID = obj.SalesRouteID,
                                            CustID = obj.CustID,
                                            SlsPerID = obj.SlsPerID,
                                            BranchID = obj.BranchID
                                        };

                                        _db.OM_SalesRouteMaster.DeleteObject(obj);
                                        _db.SaveChanges();

                                        updateSaleRoutesMaster(ref newObj, updated);
                                        _db.OM_SalesRouteMaster.AddObject(newObj);
                                        _db.SaveChanges();

                                        return Json(new
                                        {
                                            success = true,
                                            CustID = custID,
                                            SlsPerID = slsperID,
                                            BranchID = branchID,
                                            Color = "FF0000",
                                            SlsFreq = newObj.SlsFreq,
                                            WeekofVisit = newObj.WeekofVisit,
                                            VisitSort = newObj.VisitSort,
                                            Sun = newObj.Sun ? 1 : 0,
                                            Mon = newObj.Mon ? 1 : 0,
                                            Tue = newObj.Tue ? 1 : 0,
                                            Wed = newObj.Wed ? 1 : 0,
                                            Thu = newObj.Thu ? 1 : 0,
                                            Fri = newObj.Fri ? 1 : 0,
                                            Sat = newObj.Sat ? 1 : 0,
                                            Status = cust.Status
                                        });
                                    }
                                    else
                                    {
                                        throw new MessageException(MessageType.Message, "19");
                                    }
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "8");
                                }
                            }

                            foreach (var created in lstMcpInfo.Created)
                            {
                                var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == branchID
                                    && x.SalesRouteID == branchID
                                    && x.BranchID == branchID
                                    && x.SlsPerID == slsperID
                                    && x.CustID == custID);

                                if (obj == null)
                                {
                                    // insert moi
                                    var newObj = new OM_SalesRouteMaster()
                                    {
                                        PJPID = branchID,
                                        SalesRouteID = branchID,
                                        CustID = custID,
                                        SlsPerID = slsperID,
                                        BranchID = branchID
                                    };

                                    updateSaleRoutesMaster(ref newObj, created);
                                    _db.OM_SalesRouteMaster.AddObject(newObj);
                                    _db.SaveChanges();

                                    return Json(new
                                    {
                                        success = true,
                                        CustID = custID,
                                        SlsPerID = slsperID,
                                        BranchID = branchID,
                                        Color = "FF0000",
                                        SlsFreq = newObj.SlsFreq,
                                        WeekofVisit = newObj.WeekofVisit,
                                        VisitSort = newObj.VisitSort,
                                        Sun = newObj.Sun ? 1 : 0,
                                        Mon = newObj.Mon ? 1 : 0,
                                        Tue = newObj.Tue ? 1 : 0,
                                        Wed = newObj.Wed ? 1 : 0,
                                        Thu = newObj.Thu ? 1 : 0,
                                        Fri = newObj.Fri ? 1 : 0,
                                        Sat = newObj.Sat ? 1 : 0,
                                        Status = cust.Status
                                    });
                                }
                                else
                                {
                                    //return Json(new { success = false, msgCode = 2000, msgParam = Util.GetLang("MCP") });
                                    throw new MessageException(MessageType.Message, "2000", "", new string[] { Util.GetLang("MCP") });
                                }
                            }
                            #endregion
                        }

                        return Json(new
                        {
                            success = true,
                            CustID = custID,
                            SlsPerID = slsperID,
                            BranchID = branchID,
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
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
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
    }
}

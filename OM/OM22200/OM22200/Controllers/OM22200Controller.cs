using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
namespace OM22200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22200Controller : Controller
    {
        private string _screenNbr = "OM22200";
        private string _beginStatus = "H";
        private string _noneStatus = "N";
        private string _completeStatus = "C";
        OM22200Entities _db = Util.CreateObjectContext<OM22200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM22200");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\OM22200");
                }
                return _filePath;
            }
        }

        //
        // GET: /OM22200/
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetSaleRouteMaster(string branchId, string slsperid, string routeID, string pjpID)
        {
            var slsRouteMstr = _db.OM22200_pgSaleRouteMaster(Current.UserName, branchId, pjpID, routeID, slsperid).ToList();
            return this.Store(slsRouteMstr);
        }

        public ActionResult GetPJP(string branchId, string pjpID)
        {
            var pjp = _db.OM_PJP.FirstOrDefault(x => x.BranchID == branchId && x.PJPID == pjpID);
            return this.Store(pjp);
        }

        public ActionResult GetCustomer(string branchID, string pjpID, 
            string slsperID, string routeID,string lstCust)
        {
            var custs = _db.OM22200_pcCustomer(branchID, pjpID, Current.UserName, slsperID, routeID, lstCust).ToList();
            return this.Store(custs);
        }

        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var handle = data["cboHandle"];
                var pjpID = data["cboPJPID"];
                var branchID = data["cboBranchID"];

                if (!string.IsNullOrWhiteSpace(pjpID) && !string.IsNullOrWhiteSpace(branchID))
                {
                    var lstPJPHandler = new StoreDataHandler(data["lstPJP"]);
                    var inputPJP = lstPJPHandler.ObjectData<OM_PJP>().FirstOrDefault();

                    if (inputPJP != null)
                    {
                        inputPJP.PJPID = pjpID;
                        inputPJP.BranchID = branchID;

                        #region PJP
                        var pjp = _db.OM_PJP.FirstOrDefault(x => x.BranchID == inputPJP.BranchID && x.PJPID == inputPJP.PJPID);
                        if (pjp != null)
                        {
                            if (!string.IsNullOrWhiteSpace(handle) && handle != _noneStatus)
                            {
                                pjp.StatusHandle = handle;
                                pjp.Status = handle == _completeStatus ? true : false;
                            }
                            updatePJP(ref pjp, inputPJP, false);
                        }
                        else 
                        {
                            updatePJP(ref pjp, inputPJP, true);
                            _db.OM_PJP.AddObject(pjp);
                        }
                        #endregion

                        #region SlsRouteMaster
                        var lstSaleRouteMasterHandler = new StoreDataHandler(data["lstSaleRouteMaster"]);
                        var lstSaleRouteMaster = lstSaleRouteMasterHandler.BatchObjectData<OM22200_pgSaleRouteMaster_Result>();

                        lstSaleRouteMaster.Updated.AddRange(lstSaleRouteMaster.Created);
                        foreach (var slsRoute in lstSaleRouteMaster.Updated)
                        {
                            var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x=>x.BranchID == pjp.BranchID 
                                && x.PJPID == pjp.PJPID && x.CustID == slsRoute.CustID
                                && x.SalesRouteID == slsRoute.SalesRouteID && x.SlsPerID == slsRoute.SlsPerID);
                            if (obj != null)
                            {
                                if (obj.tstamp.ToHex() == slsRoute.tstamp.ToHex())
                                {
                                    updateSlsRoute(ref obj, slsRoute, false);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                            }
                            else
                            {
                                updateSlsRoute(ref obj, slsRoute, true);
                                _db.OM_SalesRouteMaster.AddObject(obj);
                            }
                        }

                        foreach (var deleted in lstSaleRouteMaster.Deleted)
                        {
                            var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.BranchID == pjp.BranchID
                                    && x.PJPID == pjp.PJPID && x.CustID == deleted.CustID
                                    && x.SalesRouteID == deleted.SalesRouteID && x.SlsPerID == deleted.SlsPerID);
                            if (obj != null)
                            {
                                if (obj.tstamp.ToHex() == deleted.tstamp.ToHex())
                                {
                                    _db.OM_SalesRouteMaster.DeleteObject(obj);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                            }
                        }

                        #endregion

                        _db.SaveChanges();
                        return Json(new { success = true, msgCode = 201405071 });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "53");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "53");
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

        public ActionResult DeletePJP(string branchId, string pjpID)
        {
            try
            {
                var pjp = _db.OM_PJP.FirstOrDefault(x => x.BranchID == branchId && x.PJPID == pjpID);
                if (pjp != null)
                {
                    if (pjp.StatusHandle == _beginStatus)
                    {
                        _db.OM_PJP.DeleteObject(pjp);

                        var lstSlsRoute = _db.OM_SalesRouteMaster.Where(x => x.BranchID == branchId && x.PJPID == pjpID).ToList();
                        foreach (var slsRoute in lstSlsRoute)
                        {
                            _db.OM_SalesRouteMaster.DeleteObject(slsRoute);
                        }
                        _db.SaveChanges();
                        return Json(new { success = true, msgcode = 2015032101 });
                    }
                    else {
                        throw new MessageException(MessageType.Message, "20140306");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("PJPID") });
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

        private void updateSlsRoute(ref OM_SalesRouteMaster obj, OM22200_pgSaleRouteMaster_Result slsRoute, bool isNew)
        {
            if (isNew)
            {
                obj = new OM_SalesRouteMaster();

                obj.CustID = slsRoute.CustID;
                obj.BranchID = slsRoute.BranchID;
                obj.PJPID = slsRoute.PJPID;
                obj.SalesRouteID = slsRoute.SalesRouteID;
                obj.SlsPerID = slsRoute.SlsPerID;

                obj.Crtd_DateTime = DateTime.Now;
                obj.Crtd_Prog = _screenNbr;
                obj.Crtd_User = Current.UserName;
            }
            obj.SlsFreq = slsRoute.SlsFreq;
            obj.SlsFreqType = "R";
            obj.WeekofVisit = slsRoute.WeekofVisit;
            obj.Sun = slsRoute.Sun;
            obj.Mon = slsRoute.Mon;
            obj.Tue = slsRoute.Tue;
            obj.Wed = slsRoute.Wed;
            obj.Thu = slsRoute.Thu;
            obj.Fri = slsRoute.Fri;
            obj.Sat = slsRoute.Sat;
            obj.VisitSort = slsRoute.VisitSort;

            obj.LUpd_DateTime = DateTime.Now;
            obj.LUpd_Prog = _screenNbr;
            obj.LUpd_User = Current.UserName;
        }

        private void updatePJP(ref OM_PJP updated, OM_PJP input, bool isNew)
        {
            if (isNew)
            {
                updated = new OM_PJP();
                updated.PJPID = input.PJPID;
                updated.BranchID = input.BranchID;
                updated.Status = false;
                updated.StatusHandle = _beginStatus;

                updated.Crtd_DateTime = DateTime.Now;
                updated.Crtd_Prog = _screenNbr;
                updated.Crtd_User = Current.UserName;
            }
            updated.Descr = input.Descr;

            updated.LUpd_DateTime = DateTime.Now;
            updated.LUpd_Prog = _screenNbr;
            updated.LUpd_User = Current.UserName;
        }
    }
}

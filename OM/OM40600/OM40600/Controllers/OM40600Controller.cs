using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using System.Text;
using HQ.eSkySys;
using HQFramework.DAL;
namespace OM40600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM40600Controller : Controller
    {
        private string _screenNbr = "OM40600";
        private string _userName = Current.UserName;
        OM40600Entities _db = Util.CreateObjectContext<OM40600Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        public ActionResult Index()
        {
            var minDate = false;
            int LimitedYear = 0;
            var objConfig = _db.OM40600_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                minDate=objConfig.MinDate.HasValue && objConfig.MinDate.Value;
                LimitedYear = objConfig.LimitedYear.ToInt() + 1;
            }
            ViewBag.MinDate = minDate;
            ViewBag.limitedYear = LimitedYear;
            Util.InitRight(_screenNbr);
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetData(string branchID, string pJPID, string custID, string slsperID, string routeID)
        {
            _db.CommandTimeout = int.MaxValue;
            var dets = _db.OM40600_pgSaleRouteMaster(branchID, pJPID, custID, slsperID, routeID).ToList();
            return this.Store(dets);
        }
        public ActionResult GetDataBranchID(string territory)
        {
            _db.CommandTimeout = int.MaxValue;
            var detBranchID = _db.OM40600_pgLoadBranchID(Current.UserName, Current.CpnyID, Current.LangID, territory).ToList();
            return this.Store(detBranchID);
        }
        #region DataProcess
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            DataAccess dal = Util.Dal();
            try
            {

                var detHeader = new StoreDataHandler(data["lstDet"]);
                var lstDet = detHeader.ObjectData<OM40600_pgSaleRouteMaster_Result>().Where(p => p.Selected == true).ToList();


                DateTime fromDate = data["fromDate"].ToDateShort();
                DateTime toDate = data["toDate"].ToDateShort();


                string lstslsPerID = "";
                string lstRouteID = "";
                string lstCust = "";
                string lstPJP = "";
                string lstBranch = "";
                foreach (var objHeader in lstDet)
                {
                    lstslsPerID += objHeader.SlsPerID + ",";
                    lstRouteID += objHeader.SalesRouteID + ",";
                    lstCust += objHeader.CustID + ",";
                    lstPJP += objHeader.PJPID + ",";
                    lstBranch += objHeader.BranchID + ",";
                }
                try
                {
                    PJPProcess.PJP pjp = new PJPProcess.PJP(Current.UserName, _screenNbr, dal);
                    dal.BeginTrans(IsolationLevel.ReadCommitted);
                    if (!pjp.OM40600CreateMCP(lstslsPerID, lstRouteID, lstCust, lstPJP, lstBranch, fromDate, toDate))
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

                return Util.CreateMessage(MessageProcess.Process);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion
         #region DataBranchID
        [HttpPost]
        public ActionResult SaveBranchID(FormCollection data,string BranchID,string PJPID,string SalesRouteID)
        {
           
            try
            {
                     
                //var detHeader = new StoreDataHandler(data["lstDetBranchID"]);
                //var lstDetBranchID = detHeader.ObjectData<OM40600_pgLoadBranchID_Result>().ToList();



                DateTime fromDateBranchID = data["fromDateBranchID"].ToDateShort();
                DateTime toDateBranchID = data["toDateBranchID"].ToDateShort();


                StringBuilder lstslsPerID = new StringBuilder();
                StringBuilder lstRouteID = new StringBuilder();
                StringBuilder lstCust = new StringBuilder();
                StringBuilder lstPJP = new StringBuilder();
                StringBuilder lstBranch = new StringBuilder();
                //int numDetBranchID = lstDetBranchID.Count;
                //for (int j = 0; j < numDetBranchID; j++)
                //{
                    DataAccess dal = Util.Dal();
                    dal.CmdTimeout = int.MaxValue;          
                    var lstDet = _db.OM40600_pgSaleRouteMaster(BranchID, PJPID, "%", "%", SalesRouteID).ToList();
                    int numOfDet = lstDet.Count;
                    for (int i = 0; i < numOfDet; i++)
                    {
                        lstslsPerID.Append(lstDet[i].SlsPerID).Append(",");
                        lstRouteID.Append(lstDet[i].SalesRouteID).Append(",");
                        lstCust.Append(lstDet[i].CustID).Append(",");
                        lstPJP.Append(lstDet[i].PJPID).Append(",");
                        lstBranch.Append(lstDet[i].BranchID).Append(",");
                    }
                    try
                    {
                        PJPProcess.PJP pjp = new PJPProcess.PJP(Current.UserName, _screenNbr, dal);
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!pjp.OM40600CreateMCP(lstslsPerID.PassNull(), lstRouteID.PassNull(), lstCust.PassNull(), lstPJP.PassNull(), lstBranch.PassNull(), fromDateBranchID, toDateBranchID))
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
                    finally
                    {
                        
                    }
                //}
                return Util.CreateMessage(MessageProcess.Process);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
         #endregion
    }
}

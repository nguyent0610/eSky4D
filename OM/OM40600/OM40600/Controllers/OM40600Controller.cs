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
            var dets = _db.OM40600_pgSaleRouteMaster(branchID, pJPID, custID, slsperID, routeID).ToList();
            return this.Store(dets);
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
    }
}

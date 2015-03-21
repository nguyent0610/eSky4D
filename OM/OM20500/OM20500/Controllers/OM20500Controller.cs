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
namespace OM20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20500Controller : Controller
    {
        private string _screenNbr = "OM20500";
        private string _holdStatus = "H";
        private string _openStatus = "O";
        private string _closePOStatus = "E";

        OM20500Entities _db = Util.CreateObjectContext<OM20500Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        //
        // GET: /OM20500/
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

        public ActionResult GetOrder(string branchID, 
            string slsperID, string custID, string status,
            DateTime startDate, DateTime endDate)
        {
            var orders = _db.OM20500_pgOrder(branchID, slsperID,custID, status, startDate,endDate).ToList();
            return this.Store(orders);
        }

        public ActionResult GetDet(string branchID,
            string slsperID, string custID, string status,
            DateTime startDate, DateTime endDate)
        {
            var dets = _db.OM20500_pgDet(branchID, slsperID, custID, status, startDate, endDate).ToList();
            return this.Store(dets);
        }

        public ActionResult GetHisOrd(string branchID, string orderNbr)
        {
            var hisOrders = _db.OM20500_pgHistoryOrd(branchID, orderNbr).ToList();
            return this.Store(hisOrders);
        }

        public ActionResult GetHisDet(string branchID, string orderNbr)
        {
            var hisDets = _db.OM20500_pgHisDet(branchID, orderNbr).ToList();
            return this.Store(hisDets);
        }

        [ValidateInput(false)]
        public ActionResult ClosePO(FormCollection data) 
        {
            var lstOrderChangeHandler = new StoreDataHandler(data["lstOrderChange"]);
            var lstOrderChange = lstOrderChangeHandler.BatchObjectData<OM20500_pgOrder_Result>();
            var lstOrderNbrError = new List<string>();

            foreach (var orderChange in lstOrderChange.Updated)
            {
                if (orderChange.Selected == true)
                {
                    var order = _db.OM_PDASalesOrd.FirstOrDefault(x => x.OrderNbr == orderChange.OrderNbr
                        && x.BranchID == orderChange.BranchID
                        && (x.Status == _holdStatus || x.Status == _openStatus));
                    if (order != null)
                    {
                        order.Status = _closePOStatus;
                    }
                    else
                    {
                        lstOrderNbrError.Add(orderChange.OrderNbr);
                    }
                }
                
            }
            _db.SaveChanges();

            if (lstOrderNbrError.Count() > 0)
            {
                return Json(new
                {
                    success = true,
                    msgCode = 20150320,
                    msgParam = new string[]{string.Join(",", lstOrderNbrError)}
                });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    msgCode = 201405071
                });
            }
        }
    }
}

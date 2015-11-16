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
using OMProcess;
using HQFramework.DAL;
using System.Data;

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
        private FormCollection _form;   
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
        public ActionResult GetCloseOrder(string branchID,
          string slsperID, string custID, string status,
          DateTime startDate, DateTime endDate)
        {
            var orders = _db.OM20500_pgCloseOrder(branchID, slsperID, custID, status, startDate, endDate).ToList();
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
        public ActionResult GetItemSite(string invtID, string siteID)
        {
            var objSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
            return this.Store(objSite);
        }
        public ActionResult GetLotTrans(string branchID,
            string slsperID, string custID, string status,
            DateTime startDate, DateTime endDate)
        {
            var lot = _db.OM20500_pgLotTrans(branchID, slsperID, custID, status, startDate, endDate).ToList();
            return this.Store(lot);
        }
        #region DataProcess
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            DataAccess dal = Util.Dal();
            try
            {
                _form = data;
                var detHeader = new StoreDataHandler(data["lstOrder"]);
                var lstOrd = detHeader.ObjectData<OM20500_pgOrder_Result>().Where(p => p.Selected == true).ToList();

                var detHandler = new StoreDataHandler(data["lstDet"]);
                var lstDet = detHandler.ObjectData<OM20500_pgDet_Result>().Where(p => p.Selected == true).ToList();

                var detLot = new StoreDataHandler(data["lstLot"]);
                var lstLot = detLot.ObjectData<OM20500_pgLotTrans_Result>().ToList();

                string Delivery = data["delivery"];
                DateTime dteShipDate = data["shipDate"].ToDateShort();
                DateTime dteARDocDate = data["aRDocDate"].ToDateShort();
                //bool isAddStock = data["isAddStock"].ToBool();
                string message = "";
                foreach (var objHeader in lstOrd)
                {
                    var lstDetOrNbr = lstDet.Where(p => p.OrderNbr == objHeader.OrderNbr).ToList();
                    var lstLotOrNbr = lstLot.Where(p => p.OrderNbr == objHeader.OrderNbr).ToList();

                    Dictionary<string, double> dicRef = new Dictionary<string, double>();
                    for (int i = 0; i < lstDetOrNbr.Count; i++)
                    {
                        dicRef.Add(lstDetOrNbr[i].LineRef, lstDetOrNbr[i].QtyShip);
                    }

                    Dictionary<string, double> dicRefLot = new Dictionary<string, double>();
                    for (int i = 0; i < lstLotOrNbr.Count; i++)
                    {
                        dicRefLot.Add(lstLotOrNbr[i].OMLineRef + "@" + lstLotOrNbr[i].InvtID + "@" + lstLotOrNbr[i].LotSerNbr, lstLotOrNbr[i].Qty);
                    }

                    try
                    {
                        OM om = new OM(Current.UserName, _screenNbr, dal);
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!om.OM20500_Release(objHeader.BranchID, objHeader.OrderNbr, dicRef, Delivery, dteShipDate, dteARDocDate, objHeader.IsAddStock, dicRefLot))
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
                        if (ex is MessageException)
                        {
                            var msg = ex as MessageException;
                            message += "Đơn hàng " + objHeader.OrderNbr + ":" + Message.GetString(msg.Code, msg.Parm) + "</br>";
                        }
                        else
                        {
                            message += "Đơn hàng " + objHeader.OrderNbr + " bị lỗi: " + ex.ToString() + "</br>";
                        }
                    }
                }
                if (message != string.Empty)
                {
                    throw new MessageException("20410", parm: new[] { message });
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
        [ValidateInput(false)]
        public ActionResult ClosePO(FormCollection data)
        {
            var lstOrderChangeHandler = new StoreDataHandler(data["lstOrderChange"]);
            var lstOrderChange = lstOrderChangeHandler.BatchObjectData<OM20500_pgCloseOrder_Result>();
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
                    msgParam = new string[] { string.Join(",", lstOrderNbrError) }
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
        #endregion
    }
}

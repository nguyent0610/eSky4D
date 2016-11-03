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
            _db.CommandTimeout = int.MaxValue;
            var orders = _db.OM20500_pgOrder(branchID, slsperID,custID, status, startDate,endDate, Current.UserName, Current.CpnyID,Current.LangID).ToList();
            return this.Store(orders);
        }
        public ActionResult GetCloseOrder(string branchID,
          string slsperID, string custID, string status,
          DateTime startDate, DateTime endDate)
        {
            _db.CommandTimeout = int.MaxValue;
            var orders = _db.OM20500_pgCloseOrder(branchID, slsperID, custID, status, startDate, endDate).ToList();
            return this.Store(orders);
        }
        public ActionResult GetDet(string branchID,
            string slsperID, string custID, string status,
            DateTime startDate, DateTime endDate)
        {
            _db.CommandTimeout = int.MaxValue;
            var dets = _db.OM20500_pgDet(branchID, slsperID, custID, status, startDate, endDate, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(dets);
        }
        public ActionResult GetHisOrd(string branchID, string orderNbr)
        {
            _db.CommandTimeout = int.MaxValue;
            var hisOrders = _db.OM20500_pgHistoryOrd(branchID, orderNbr, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(hisOrders);
        }
        public ActionResult GetHisDet(string branchID, string orderNbr)
        {
            _db.CommandTimeout = int.MaxValue;
            var hisDets = _db.OM20500_pgHisDet(branchID, orderNbr, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(hisDets);
        }
        public ActionResult GetItemSite(string invtID, string siteID)
        {
            _db.CommandTimeout = int.MaxValue;
            var objSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
            return this.Store(objSite);
        }
        public ActionResult GetLotTrans(string branchID,
            string slsperID, string custID, string status,
            DateTime startDate, DateTime endDate)
        {
            _db.CommandTimeout = int.MaxValue;
            var lot = _db.OM20500_pgLotTrans(branchID, slsperID, custID, status, startDate, endDate, Current.UserName, Current.CpnyID, Current.LangID).ToList();
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
                string errorDate = string.Empty;
                string errorAmt = string.Empty;
                var isCheckShipDate = false;
                var isCheckARDocDate = false;
                var orderSuccess = string.Empty;
                var objConfig = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "OM20500CHECKAPPROVE");
                if (objConfig != null)
                {
                    isCheckShipDate =  objConfig.IntVal == 1 ? true : false;
                    isCheckARDocDate = objConfig.FloatVal == 1.0 ? true : false;
                }
                bool isCheckTotAmt = false;
                var objConfigTotAmt = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "OM20500CHECKTOTAMT");
                if (objConfigTotAmt != null && objConfigTotAmt.IntVal == 1)
                {
                    isCheckTotAmt = true;
                }
                
                foreach (var objHeader in lstOrd)
                {
                    // Check ShipDate & DocDate vs OrderDate
                    if ((isCheckShipDate && objHeader.OrderDate.ToDateShort() > dteShipDate) || (isCheckARDocDate && objHeader.OrderDate.ToDateShort() > dteARDocDate))
                    {
                        errorDate += objHeader.OrderNbr + ", ";                        
                        continue;
                    }
                    var lstDetOrNbr = lstDet.Where(p => p.OrderNbr == objHeader.OrderNbr).ToList();
                    if (isCheckTotAmt && lstDetOrNbr.Sum(x => x.LineAmt) == 0)
                    {
                        errorAmt += objHeader.OrderNbr + ", ";
                        continue;
                    }
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
                            var obj = _db.OM_PDASalesOrd.Where(x => x.BranchID == objHeader.BranchID && x.OrderNbr == objHeader.OrderNbr).FirstOrDefault();
                            if (obj != null)
                            {
                                obj.LUpd_DateTime = DateTime.Now;
                                obj.LUpd_Prog = _screenNbr;
                                obj.LUpd_User = Current.UserName;
                                var lstPDADet = _db.OM_PDASalesOrdDet.Where(x => x.BranchID == objHeader.BranchID && x.OrderNbr == objHeader.OrderNbr).ToList();
                                for (int k = 0; k < lstPDADet.Count; k++)
                                {
                                    lstPDADet[k].LUpd_Datetime = DateTime.Now;
                                    lstPDADet[k].LUpd_Prog = _screenNbr;
                                    lstPDADet[k].LUpd_User = Current.UserName;
                                }
                                _db.SaveChanges();
                            }
                            orderSuccess += objHeader.OrderNbr + ",";
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
                errorAmt = errorAmt.Trim().TrimEnd(',');
                errorDate = errorDate.Trim().TrimEnd(',');
                if (message != string.Empty || errorDate != string.Empty || errorAmt != string.Empty)
                {
                    if (errorDate != string.Empty)
                    {
                        if (message != string.Empty)
                        {
                            message += "<br>";
                        }
                        message += GetMess(2016100301, new string[] { errorDate });
                    }
                    if (errorAmt != string.Empty)
                    {
                        if (message != string.Empty)
                        {
                            message += "<br>";
                        }
                        message += GetMess(2016100302, new string[] { errorAmt });
                    }
                    orderSuccess = orderSuccess.TrimEnd(',');
                    throw new MessageException("20410", parm: new[] { message, orderSuccess });
                    //return Json(new { success = false, type = "error", msgCode = "20410", param = message, OrderSuccess = orderSuccess }); //
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
        // Get mess desc
        private string GetMess(int code, string[] parm = null)
        {
            var msg = _sys.psys_LoadMessage(Current.LangID, code).FirstOrDefault();
            if (msg != null)
            {
                if (parm != null)
                {
                    for (int i = 0; i < parm.Length; i++)
                    {
                        msg.Message = msg.Message.Replace("@p" + (i + 1).ToString(), parm[i]);
                    }
                }
            }
            return msg.Message;
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
                        order.LUpd_DateTime = DateTime.Now;
                        order.LUpd_Prog = _screenNbr;
                        order.LUpd_User = Current.UserName;
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

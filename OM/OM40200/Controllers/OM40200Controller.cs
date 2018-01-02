using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Xml;
using System.Xml.Linq;
using System.Data.Objects;
using Aspose.Cells;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.Data;
using System.Drawing;
using HQFramework.DAL;
using System.Dynamic;
using HQFramework.Common;
namespace OM40200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM40200Controller : Controller
    {
        private string _screenNbr = "OM40200";
        private string _userName = Current.UserName;
        private OM40200Entities _app = Util.CreateObjectContext<OM40200Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);     
        private FormCollection _form;
        private JsonResult _logMessage;
        private List<OM40200_pgOrder_Result> _lstOrder;

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


        public ActionResult GetOrder(string branchID, string custID, string slsperID,string type, string deliveryID, DateTime fromDate, DateTime toDate)
        {
            var lstOrder = _app.OM40200_pgOrder(branchID, custID, slsperID, deliveryID, fromDate.PassMin(), toDate.PassMin(), type, "").ToList();
            return this.Store(lstOrder);
        }

        [HttpPost]
        public ActionResult Process(FormCollection data)
        {
            try
            {
                _form = data;

                var orderHandler = new StoreDataHandler(data["lstOrder"]);
                if (_lstOrder == null)
                {
                    _lstOrder = orderHandler.ObjectData<OM40200_pgOrder_Result>().Where(p => Util.PassNull(p.OrderNbr) != string.Empty).ToList();
                }

                var type = data["ProcessType"].PassNull();

                CheckData();

                if (type == "B" || type=="C")
                {
                    DataAccess dal = Util.Dal();

                    string message = "";
                    string errorOrderNbr = "";
                    foreach (var item in _lstOrder)
                    {

                        if (_app.OM40200_ppCheckCloseDate(item.BranchID, item.OrderDate.ToDateShort(), item.OrderNbr, type, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault() == "0")
                        {
                            errorOrderNbr += item.OrderNbr + ",";
                            // new MessageException(MessageType.Message, "301");
                        }
                        else
                        {
                            OMProcess.OM order = new OMProcess.OM(_userName, _screenNbr, dal);
                            try
                            {

                                dal.BeginTrans(IsolationLevel.ReadCommitted);

                                order.OM10100_PrintInvoice(item.BranchID, item.OrderNbr);

                                dal.CommitTrans();

                            }
                            catch (Exception ex)
                            {
                                dal.RollbackTrans();
                                if (ex is MessageException)
                                {
                                    var msg = ex as MessageException;
                                    message += "Đơn hàng " + item.OrderNbr + ":" + Message.GetString(msg.Code, msg.Parm) + "</br>";

                                }
                                else
                                {
                                    message += "Đơn hàng " + item.OrderNbr + " bị lỗi: " + ex.ToString() + "</br>";
                                }
                            }
                        }
                    }
                    if (errorOrderNbr != "")
                    {
                        message += "Đơn hàng " + errorOrderNbr.TrimEnd(',') +" "+ Util.GetLang("OM40200CloseDate") + "</br>";
                    }
                    if (message != string.Empty)
                    {
                        Util.AppendLog(ref _logMessage, "20410", parm: new[] { message });
                    }
                    else
                        Util.AppendLog(ref _logMessage, "9999");
                }
                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Util.CreateError(ex.ToString());
            }
        }

        private void CheckData()
        {
            var access = Session[_screenNbr] as AccessRight;

            if (!access.Release)
            {
                throw new MessageException(MessageType.Message, "728");
            }
        }
    }
}

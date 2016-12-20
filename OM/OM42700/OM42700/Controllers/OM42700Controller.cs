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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;

namespace OM42700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM42700Controller : Controller
    {
        private string _screenNbr = "OM42700";
        private string _userName = Current.UserName;
        OM42700Entities _db = Util.CreateObjectContext<OM42700Entities>(false);
        private JsonResult _logMessage;
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
        public ActionResult GetPDA(string BranchID,string SlsperID,DateTime FromDate,DateTime ToDate)
        {
            return this.Store(_db.OM42700_pgPDA(Current.CpnyID, Current.UserName, Current.LangID,BranchID,SlsperID,FromDate,ToDate).ToList());
        }
        public ActionResult GetOrder(string BranchID, string SlsperID, DateTime FromDate, DateTime ToDate)
        {
            return this.Store(_db.OM42700_pgOrder(Current.CpnyID, Current.UserName, Current.LangID, BranchID, SlsperID, FromDate, ToDate).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            try
            {
                if (data["Type"] == "1")//cho PDA
                {
                    StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);

                    ChangeRecords<OM42700_pgPDA_Result> lstPDA = dataHandler.BatchObjectData<OM42700_pgPDA_Result>();

                    lstPDA.Created.AddRange(lstPDA.Updated);
                    foreach (OM42700_pgPDA_Result curObj in lstPDA.Created)
                    {
                        if (!curObj.Selected.Value) continue;

                        var obj = _db.OM_PDASalesOrd.Where(p => p.BranchID == curObj.BranchID && p.OrderNbr == curObj.OrderNbr).FirstOrDefault();

                        if (obj != null)
                        {
                            if (obj.tstamp.ToHex() == curObj.tstamp.ToHex())
                            {
                                Update_PDA(obj, curObj);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                }else if (data["Type"] == "2")//cho Order
                    {
                        StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);

                        ChangeRecords<OM42700_pgOrder_Result> lstOrder = dataHandler.BatchObjectData<OM42700_pgOrder_Result>();

                        lstOrder.Created.AddRange(lstOrder.Updated);
                        foreach (OM42700_pgOrder_Result curObj in lstOrder.Created)
                        {
                            if (!curObj.Selected.Value) continue;

                            var obj = _db.OM_SalesOrd.Where(p => p.BranchID == curObj.BranchID && p.OrderNbr == curObj.OrderNbr).FirstOrDefault();

                            if (obj != null)
                            {
                                if (obj.tstamp.ToHex() == curObj.tstamp.ToHex())
                                {
                                    Update_Order(obj, curObj);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                    }
                _db.SaveChanges();
               

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void Update_PDA(OM_PDASalesOrd t, OM42700_pgPDA_Result s)
        {

            t.OrderDate = s.UpdateDate.ToDateShort();

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        private void Update_Order(OM_SalesOrd t, OM42700_pgOrder_Result s)
        {

            t.IsAddStock = s.IsAddStock;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

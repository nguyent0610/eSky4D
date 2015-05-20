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
namespace IN22003.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN22003Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN22003";
        private string _userName = Current.UserName;
        IN22003Entities _db = Util.CreateObjectContext<IN22003Entities>(false);
        private JsonResult mLogMessage;
        private FormCollection mForm;
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

        public ActionResult GetData(DateTime Date, string Territory, string State,string BranchID)
        {
            return this.Store(_db.IN22003_pgLoadGrid(Date, Territory, State, BranchID).ToList());
        }

        [HttpPost]
        public ActionResult Process(FormCollection data)
        {
            try
            {
                mForm = data;
                StoreDataHandler custHandler = new StoreDataHandler(data["lstIN_StockRecoveryDet"]);

                var lstIN_StockRecoveryDet = custHandler.ObjectData<IN22003_pgLoadGrid_Result>();

                var access = Session["IN22003"] as AccessRight;

                if (!access.Update && !access.Insert)
                    throw new MessageException(MessageType.Message, "728");
                string handle = data["cboHandle"];
                string date_temp = data["NewDateExp"];
                DateTime date = DateTime.Parse(date_temp);
                if (handle != "N" && handle != string.Empty)
                {
                    foreach (var item in lstIN_StockRecoveryDet)
                    {
                        if (item.ColCheck == true)
                        {
                            var obj = _db.IN_StockRecoveryDet.FirstOrDefault(p => p.BranchID == item.BranchID
                                                                                && p.StkRecNbr == item.StkRecNbr
                                                                                && p.InvtID == item.InvtID
                                                                                && p.ExpDate == item.ExpDate);
                            if (obj != null)
                            {
                                if (handle == "A")
                                {
                                    obj.ApproveStkQty = item.ApproveStkQty;
                                    obj.NewExpDate = date;

                                    var obj1 = _db.IN_StockRecoveryCust.FirstOrDefault(p => p.BranchID == item.BranchID
                                                                                        && p.StkRecNbr == item.StkRecNbr
                                                                                        && p.InvtID == item.InvtID
                                                                                        && p.SlsPerID == item.SlsperId
                                                                                        && p.NewExpDate == obj.NewExpDate
                                                                                        );
                                    if (obj1 == null)
                                    {
                                        var record = new IN_StockRecoveryCust();
                                        record.BranchID = item.BranchID;
                                        record.SlsPerID = item.SlsperId;
                                        record.StkRecNbr = item.StkRecNbr;
                                        record.InvtID = item.InvtID;
                                        record.NewExpDate = date;
                                        record.Status = "H";
                                        record.QtyGiveBack = 0;

                                        record.Crtd_DateTime = DateTime.Now;
                                        record.Crtd_Prog = _screenNbr;
                                        record.Crtd_User = _userName;
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_Prog = _screenNbr;
                                        record.LUpd_User = _userName;
                                        _db.IN_StockRecoveryCust.AddObject(record);
                                    }
                                }
                                obj.Status = handle;
                               
                                obj.LUpd_DateTime = DateTime.Now;
                                obj.LUpd_Prog = _screenNbr;
                                obj.LUpd_User = _userName; 
                            }

                        }
                    }
                    _db.SaveChanges();

                }
                if (mLogMessage != null)
                {
                    return mLogMessage;
                }
                else
                    return Json(new { success = true, type = "message", code = "8009" });
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

    }
}

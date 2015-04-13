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
namespace IN22004.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN22004Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN22004";
        private string _userName = Current.UserName;
        IN22004Entities _db = Util.CreateObjectContext<IN22004Entities>(false);
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

        public ActionResult GetData(string BranchID, DateTime Date)
        {
            return this.Store(_db.IN22004_pgLoadGrid(BranchID,Date).ToList());
        }

        [HttpPost]
        public ActionResult Process(FormCollection data)
        {
            try
            {
                mForm = data;
                StoreDataHandler custHandler = new StoreDataHandler(data["lstIN_StockRecoveryCust"]);

                var lstIN_StockRecoveryCust = custHandler.ObjectData<IN22004_pgLoadGrid_Result>();

                var access = Session["IN22004"] as AccessRight;

                if (!access.Update && !access.Insert)
                    throw new MessageException(MessageType.Message, "728");
                string handle = data["cboHandle"];
                if (handle != "N" && handle != string.Empty)
                {
                    foreach (var item in lstIN_StockRecoveryCust)
                    {
                        if (item.ColCheck == true)
                        {
                            var obj = _db.IN_StockRecoveryCust.FirstOrDefault(p => p.BranchID == item.BranchID
                                                                                && p.StkRecNbr == item.StkRecNbr
                                                                                && p.InvtID == item.InvtID
                                                                                && p.NewExpDate == item.NewExpDate
                                                                                && p.SlsPerID==item.SlsPerID);
                            if (obj != null)
                            {
                                obj.Status = handle;

                                obj.LUpd_DateTime = DateTime.Now;
                                obj.LUpd_Prog = _screenNbr;
                                obj.LUpd_User = _userName;

                                if (handle == "A")
                                {
                                    obj.QtyGiveBack = item.QtyGiveBack;
                                }
                            }
                            else
                            {
                                var record = new IN_StockRecoveryCust();
                                record.BranchID = item.BranchID;
                                record.SlsPerID = item.SlsPerID;
                                record.StkRecNbr = item.StkRecNbr;
                                record.InvtID = item.InvtID;
                                record.NewExpDate = (DateTime)item.NewExpDate;
                                record.Status = handle;
                                if (handle == "A")
                                {
                                    record.QtyGiveBack = item.QtyGiveBack;
                                }

                                record.Crtd_DateTime = DateTime.Now;
                                record.Crtd_Prog = _screenNbr;
                                record.Crtd_User = _userName;
                                record.LUpd_DateTime = DateTime.Now;
                                record.LUpd_Prog = _screenNbr;
                                record.LUpd_User = _userName;
                                _db.IN_StockRecoveryCust.AddObject(record);
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

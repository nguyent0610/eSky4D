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
namespace IN40300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN40300Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN40300";
        private string _userName = Current.UserName;
        IN40300Entities _db = Util.CreateObjectContext<IN40300Entities>(false);
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

        //public ActionResult GetData(DateTime DateFrom, DateTime DateTo, string BranchID)
        //{
        //    return this.Store(_db.IN40300_pgLoadGrid(DateFrom, DateTo, BranchID).ToList());
        //}

        //[HttpPost]
        //public ActionResult Process(FormCollection data)
        //{
        //    try
        //    {
        //        mForm = data;
        //        StoreDataHandler custHandler = new StoreDataHandler(data["lstPPC_StockRecovery"]);

        //        var lstPPC_StockRecovery = custHandler.ObjectData<IN40300_pgLoadGrid_Result>();

        //        var access = Session["IN40300"] as AccessRight;

        //        if (!access.Update && !access.Insert)
        //            throw new MessageException(MessageType.Message, "728");
        //        string handle = data["cboHandle"];
        //        if (handle != "N" && handle != string.Empty)
        //        {
        //            foreach (var item in lstPPC_StockRecovery)
        //            {
        //                if (item.ColCheck == true)
        //                {
        //                    var obj = _db.PPC_StockRecoveryDet.FirstOrDefault(p => p.BranchID == item.BranchID
        //                                                                        && p.SlsPerID == item.SlsPerID
        //                                                                        && p.StkRecNbr == item.StkRecNbr
        //                                                                        && p.InvtID == item.InvtID
        //                                                                        && p.ExpDate == item.ExpDate);
        //                    if (obj != null)
        //                    {
        //                        if (handle=="A")
        //                        {
        //                            obj.ApproveQty = item.ApproveQty;
        //                            var obj1 = _db.IN_StockRecoveryDet.FirstOrDefault(p => p.BranchID == item.BranchID
        //                                                                            && p.StkRecNbr == item.StkRecNbr
        //                                                                            && p.ExpDate == item.ExpDate
        //                                                                            && p.InvtID == item.InvtID);
        //                            if (obj1 == null)
        //                            {
        //                                var record = new IN_StockRecoveryDet();
        //                                record.BranchID = item.BranchID;
        //                                record.StkRecNbr = item.StkRecNbr;
        //                                record.ExpDate = item.ExpDate;
        //                                record.InvtID = item.InvtID;
        //                                record.Status = "H";
        //                                record.StkQty = item.ApproveQty;
        //                                record.Price = item.Price;
        //                                record.Crtd_DateTime = DateTime.Now;
        //                                record.Crtd_Prog = _screenNbr;
        //                                record.Crtd_User = _userName;
        //                                record.LUpd_DateTime = DateTime.Now;
        //                                record.LUpd_Prog = _screenNbr;
        //                                record.LUpd_User = _userName;
        //                                _db.IN_StockRecoveryDet.AddObject(record);
        //                            }
        //                            else
        //                            {
        //                                obj1.StkQty = obj1.StkQty + item.ApproveQty;
        //                                obj1.LUpd_DateTime = DateTime.Now;
        //                                obj1.LUpd_Prog = _screenNbr;
        //                                obj1.LUpd_User = _userName;
        //                            }
        //                        }

        //                        obj.Status = handle;
        //                        obj.LUpd_DateTime = DateTime.Now;
        //                        obj.LUpd_Prog = _screenNbr;
        //                        obj.LUpd_User = _userName;
        //                    }
        //                }
        //            }
        //            _db.SaveChanges();

        //        }
        //        if (mLogMessage != null)
        //        {
        //            return mLogMessage;
        //        }
        //        else
        //            return Json(new { success = true, type = "message", code = "8009" });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException)
        //        {
        //            return (ex as MessageException).ToMessage();
        //        }
        //        else
        //        {
        //            return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //        }
        //    }
        //}

    }
}

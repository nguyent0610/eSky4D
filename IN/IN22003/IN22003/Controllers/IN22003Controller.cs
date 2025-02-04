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
        private string _branchID = "";
        IN22003Entities _db = Util.CreateObjectContext<IN22003Entities>(false);
        private List<IN22003_pgLoadGridPopUp_Result> _lstPopUp = new List<IN22003_pgLoadGridPopUp_Result>();
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

        public ActionResult GetData(DateTime Date, string Territory,string BranchID)
        {
            return this.Store(_db.IN22003_pgLoadGrid(Date, Territory, BranchID).ToList());
        }

        public ActionResult GetPopUp(DateTime Date, string Territory, string BranchID)
        {
            return this.Store(_db.IN22003_pgLoadGridPopUp(Date, Territory, BranchID).ToList());
        }

        [HttpPost]
        public ActionResult Process(FormCollection data)
        {
            try
            {
                mForm = data;
                _branchID = data["cboBranchID"].PassNull();

                var detHandlerPopUp = new StoreDataHandler(data["lstPopUp"]);
                _lstPopUp = detHandlerPopUp.ObjectData<IN22003_pgLoadGridPopUp_Result>().ToList();

                StoreDataHandler custHandler = new StoreDataHandler(data["lstIN_StockRecoveryDet"]);
                var lstIN_StockRecoveryDet = custHandler.ObjectData<IN22003_pgLoadGrid_Result>();

                var access = Session["IN22003"] as AccessRight;

                if (!access.Update && !access.Insert)
                    throw new MessageException(MessageType.Message, "728");

                string handle = data["cboHandle"];
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
                                    obj.Status = handle;
                                    Save_PopUp(handle, obj);
                                }
                               
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

        private void Save_PopUp(string handle,IN_StockRecoveryDet det)
        {
            var lst=_lstPopUp.Where(p=>p.InvtID==det.InvtID &&p.BranchID==det.BranchID && p.StkRecNbr==det.StkRecNbr && p.ExpDate==det.ExpDate).ToList();
            foreach (var row in lst)
            {
                var obj=_db.IN_StockRecoveryPopUp.FirstOrDefault(p=>p.BranchID == _branchID 
                                                        && p.StkRecNbr == row.StkRecNbr
                                                        && p.ExpDate == row.ExpDate
                                                        && p.InvtID == row.InvtID
                                                        && p.NewExpDate == row.NewExpDate);

                if(obj==null)
                {
                    obj=new IN_StockRecoveryPopUp();
                    obj.ResetET();

                    obj.BranchID = _branchID;
                    obj.StkRecNbr = row.StkRecNbr;
                    obj.ExpDate = row.ExpDate;
                    obj.InvtID = row.InvtID;
                    obj.Status = handle;
                    obj.StkQty = row.StkQty;
                    obj.Price = row.Price;

                    Update_PopUp(row,obj);
                    obj.Crtd_Prog = _screenNbr;
                    obj.Crtd_User = _userName;
                    obj.Crtd_DateTime = DateTime.Now;
                    _db.IN_StockRecoveryPopUp.AddObject(obj);

                }
                else
                {

                }
            }
                                        
        }

        private void Update_PopUp(IN22003_pgLoadGridPopUp_Result row, IN_StockRecoveryPopUp obj)
        {
            obj.ApproveStkQty = row.ApproveStkQty;
            obj.NewExpDate = row.NewExpDate;
            

            obj.LUpd_Prog = _screenNbr;
            obj.LUpd_User = _userName;
            obj.LUpd_DateTime = DateTime.Now;
        }

    }
}

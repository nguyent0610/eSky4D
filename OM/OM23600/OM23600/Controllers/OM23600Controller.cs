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
namespace OM23600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23600Controller : Controller
    {
        private string _screenNbr = "OM23600";
        private string _userName = Current.UserName;
        OM23600Entities _db = Util.CreateObjectContext<OM23600Entities>(false);

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
        public ActionResult GetData()
        {
            return this.Store(_db.OM23600_pgPosmID(Current.UserName, Current.CpnyID, Current.LangID));
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<OM23600_pgPosmID_Result> lstData = dataHandler.BatchObjectData<OM23600_pgPosmID_Result>();

                lstData.Created.AddRange(lstData.Updated);
                foreach (OM23600_pgPosmID_Result del in lstData.Deleted)
                {

                    if (lstData.Created.Where(p => p.PosmID.ToLower().Trim() == del.PosmID.ToLower().Trim() && p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim() && p.CustId.ToLower().Trim() == del.CustId.ToLower().Trim() && p.InvtID.ToLower().Trim() == del.InvtID.ToLower().Trim()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.PosmID.ToLower().Trim() == del.PosmID.ToLower().Trim() && p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim() && p.CustId.ToLower().Trim() == del.CustId.ToLower().Trim() && p.InvtID.ToLower().Trim() == del.InvtID.ToLower().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_POSMBranch.ToList().Where(p => p.PosmID.ToLower().Trim() == del.PosmID.ToLower().Trim() && p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim() && p.CustId.ToLower().Trim() == del.CustId.ToLower().Trim() && p.InvtID.ToLower().Trim() == del.InvtID.ToLower().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_POSMBranch.DeleteObject(objDel);
                        }
                    }
                }


                foreach (OM23600_pgPosmID_Result curItem in lstData.Created)
                {
                    if (curItem.PosmID.PassNull() == ""
                        && curItem.BranchID.PassNull() == ""
                        && curItem.CustId.PassNull() == ""
                        && curItem.InvtID.PassNull() == "")
                    {
                        continue;
                    }


                    var BranchID = _db.OM_POSMBranch.Where(p => p.PosmID.ToLower() == curItem.PosmID.ToLower() 
                        && p.BranchID.ToLower() == curItem.BranchID.ToLower() 
                        && p.CustId.ToLower() == curItem.CustId.ToLower()
                        && p.InvtID.ToLower() == curItem.InvtID.ToLower()).FirstOrDefault();

                    if (BranchID != null)
                    {
                        if (BranchID.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_OM_POSMBranch(BranchID, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        BranchID = new OM_POSMBranch();
                        Update_OM_POSMBranch(BranchID, curItem, true);
                        _db.OM_POSMBranch.AddObject(BranchID);
                    }
                }

                _db.SaveChanges();

                return Util.CreateMessage(MessageProcess.Save);
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
            }
        }

        private void Update_OM_POSMBranch(OM_POSMBranch t, OM23600_pgPosmID_Result s, bool isNew)
        {
            if (isNew)
            {
                t.PosmID = s.PosmID;
                t.BranchID = s.BranchID;
                t.CustId = s.CustId;
                t.InvtID = s.InvtID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CustName = s.CustName;
            t.ClassID = s.ClassID;
            t.SiteID = s.SiteID;
            t.Descr = s.Descr;           
            t.Date = s.Date.ToDateTime();
            t.Qty = s.Qty;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

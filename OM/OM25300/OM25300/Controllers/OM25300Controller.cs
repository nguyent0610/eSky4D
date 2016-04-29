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
namespace OM25300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM25300Controller : Controller
    {
        private string _screenNbr = "OM25300";
        private string _userName = Current.UserName;
        OM25300Entities _db = Util.CreateObjectContext<OM25300Entities>(false);

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
        public ActionResult GetData()
        {
            return this.Store(_db.OM25300_pgPosmID(Current.UserName, Current.CpnyID, Current.LangID));
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<OM25300_pgPosmID_Result> lstData = dataHandler.BatchObjectData<OM25300_pgPosmID_Result>();

               
                foreach (OM25300_pgPosmID_Result del in lstData.Deleted)
                {

                    if (lstData.Created.Where(p => p.PosmID.ToLower().Trim() == del.PosmID.ToLower().Trim() && p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim()
                        && p.InvtID.ToLower().Trim() == del.InvtID.ToLower().Trim() && p.SiteID.ToLower().Trim() == del.SiteID.ToLower().Trim() && p.Date==del.Date).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.PosmID.ToLower().Trim() == del.PosmID.ToLower().Trim() && p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim()
                        && p.InvtID.ToLower().Trim() == del.InvtID.ToLower().Trim() && p.SiteID.ToLower().Trim() == del.SiteID.ToLower().Trim() && p.Date == del.Date).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_FCS_POSM.ToList().Where(p => p.PosmID.ToLower().Trim() == del.PosmID.ToLower().Trim() && p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim() && p.InvtID.ToLower().Trim() == del.InvtID.ToLower().Trim() && p.SiteID.ToLower().Trim() == del.SiteID.ToLower().Trim() && p.Date == del.Date).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_FCS_POSM.DeleteObject(objDel);
                        }
                    }
                }

                lstData.Created.AddRange(lstData.Updated);
                foreach (OM25300_pgPosmID_Result curItem in lstData.Created)
                {
                    if (curItem.PosmID.PassNull() == "")
                    {
                        continue;
                    }


                    var BranchID = _db.OM_FCS_POSM.Where(p => p.PosmID.ToLower() == curItem.PosmID.ToLower() 
                        && p.BranchID.ToLower() == curItem.BranchID.ToLower()
                        
                        && p.InvtID.ToLower() == curItem.InvtID.ToLower()
                        && p.SiteID.ToLower() == curItem.SiteID.ToLower() && p.Date == curItem.Date
                       ).FirstOrDefault();

                    if (BranchID != null)
                    {
                        if (BranchID.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_OM_POSMBranchID(BranchID, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        BranchID = new OM_FCS_POSM();
                        Update_OM_POSMBranchID(BranchID, curItem, true);
                        _db.OM_FCS_POSM.AddObject(BranchID);
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

        private void Update_OM_POSMBranchID(OM_FCS_POSM t, OM25300_pgPosmID_Result s, bool isNew)
        {
            if (isNew)
            {
                t.PosmID = s.PosmID;
                t.BranchID = s.BranchID;
                t.InvtID = s.InvtID;
                t.SiteID = s.SiteID;
                t.Date = s.Date.ToDateTime();
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
           
            t.ClassID = s.ClassID;
            t.CpnyName = s.CpnyName;
            t.Descr = s.Descr;
            t.FCS = s.FCS;
            
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

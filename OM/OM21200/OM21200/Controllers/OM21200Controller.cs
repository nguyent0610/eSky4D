﻿using HQ.eSkyFramework;
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
namespace OM21200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21200Controller : Controller
    {
        private string _screenNbr = "OM21200";
        private string _userName = Current.UserName;
        OM21200Entities _db = Util.CreateObjectContext<OM21200Entities>(false);
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

        public ActionResult GetUserDefault()
        {
            var UserDefaults = _db.OM21200_pgLoadUserDefault(Current.UserName,Current.CpnyID,Current.LangID).ToList();
            return this.Store(UserDefaults);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstUserDefault"]);
                ChangeRecords<OM21200_pgLoadUserDefault_Result> lstUserDefault = dataHandler.BatchObjectData<OM21200_pgLoadUserDefault_Result>();
                
                //foreach (OM21200_pgLoadUserDefault_Result deleted in lstUserDefault.Deleted)
                //{
                //    var del = _db.OM_UserDefault.Where(p => p.UserID == deleted.UserID && p.DfltBranchID == deleted.DfltBranchID).FirstOrDefault();
                //    if (del != null)
                //    {
                //        _db.OM_UserDefault.DeleteObject(del);
                //    }
                //}

                //lstUserDefault.Created.AddRange(lstUserDefault.Updated);

                lstUserDefault.Created.AddRange(lstUserDefault.Updated);

                foreach (OM21200_pgLoadUserDefault_Result deleted in lstUserDefault.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstUserDefault.Created.Where(p => p.UserID == deleted.UserID && p.DfltBranchID == deleted.DfltBranchID).Count() > 0)
                    {
                        lstUserDefault.Created.Where(p => p.UserID == deleted.UserID && p.DfltBranchID == deleted.DfltBranchID).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_UserDefault.FirstOrDefault(p => p.UserID == deleted.UserID && p.DfltBranchID == deleted.DfltBranchID);
                        if (objDel != null)
                        {
                            _db.OM_UserDefault.DeleteObject(objDel);
                        }
                    }
                }


                foreach (OM21200_pgLoadUserDefault_Result curUserDefault in lstUserDefault.Created)
                {
                    if (curUserDefault.UserID.PassNull() == "" && curUserDefault.DfltBranchID.PassNull() == "") continue;

                    var UserDefault = _db.OM_UserDefault.Where(p => p.UserID.ToLower() == curUserDefault.UserID.ToLower() && p.DfltBranchID.ToLower() == curUserDefault.DfltBranchID.ToLower()).FirstOrDefault();

                    if (UserDefault != null)
                    {
                        if (UserDefault.tstamp.ToHex() == curUserDefault.tstamp.ToHex())
                        {
                            Update_OM_UserDefault(UserDefault, curUserDefault, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        UserDefault = new OM_UserDefault();
                        UserDefault.ResetET();
                        Update_OM_UserDefault(UserDefault, curUserDefault, true);
                        _db.OM_UserDefault.AddObject(UserDefault);
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

        private void Update_OM_UserDefault(OM_UserDefault t, OM21200_pgLoadUserDefault_Result s, bool isNew)
        {
            if (isNew)
            {
                t.UserID = s.UserID;
                t.DfltBranchID = s.DfltBranchID;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.OMSite = s.OMSite;
            t.INSite = s.INSite;
            t.POSite = s.POSite;
            t.DfltOrderType = s.DfltOrderType;
            t.DfltSlsPerID = s.DfltSlsPerID;
            t.DfltSupID = s.DfltSupID;
            t.BranchSiteID = s.BranchSiteID;
            t.LastInvcNbr = s.LastInvcNbr;
            t.InvcNote = s.InvcNote;
            t.WorkingDate = s.WorkingDate;
            t.DiscSite = s.DiscSite;
            t.POSMSite = s.POSMSite;
            t.OMWhseLoc = s.OMWhseLoc;
            t.INWhseLoc = s.INWhseLoc;
            t.POWhseLoc = s.POWhseLoc;
            t.DiscWhseLoc = s.DiscWhseLoc;
            t.BranchWhseLoc = s.BranchWhseLoc;
            t.POSMWhseLoc = s.POSMWhseLoc;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

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
namespace SA02900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA02900Controller : Controller
    {
        private string _screenNbr = "SA02900";
        private string _userName = Current.UserName;
        SA02900Entities _db = Util.CreateObjectContext<SA02900Entities>(false);
        private bool isShowLoadData = false, isFlagBranchID= false;
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            var objConfig = _db.SA02900_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                isShowLoadData = objConfig.isShowLoadData.ToBool();
                isFlagBranchID = objConfig.isFlagBranchID.ToBool();
            }
            ViewBag.isShowLoadData = isShowLoadData;
            ViewBag.isFlagBranchID = isFlagBranchID;
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetTopGrid(string appFolID, string roleID)
        {
            return this.Store(_db.SA02900_pgSI_ApprovalFlowStatus(Current.LangID, appFolID, roleID).ToList());
        }

        public ActionResult GetBotGrid()
        {
            return this.Store(_db.SA02900_pgSI_ApprovalFlowHandle().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstTopGrid"]);
                var lstTopGrid = dataHandler.ObjectData<SA02900_pgSI_ApprovalFlowStatus_Result>() == null ? new List<SA02900_pgSI_ApprovalFlowStatus_Result>() : dataHandler.ObjectData<SA02900_pgSI_ApprovalFlowStatus_Result>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstBotGrid"]);
                var lstBotGrid = dataHandler1.ObjectData<SA02900_pgSI_ApprovalFlowHandle_Result>() == null ? new List<SA02900_pgSI_ApprovalFlowHandle_Result>() : dataHandler1.ObjectData<SA02900_pgSI_ApprovalFlowHandle_Result>();

                #region Save SI_ApprovalFlowStatus
                var lstOld_SI_ApprovalFlowStatus = _db.SI_ApprovalFlowStatus.ToList();

                foreach (var objold in lstOld_SI_ApprovalFlowStatus)
                {
                    if (lstTopGrid.Where(p => p.BranchID .ToLower() == objold.BranchID.ToLower() && p.AppFolID.ToLower() == objold.AppFolID.ToLower()
                                        && p.RoleID.ToLower() == objold.RoleID.ToLower()
                                        && p.Status.ToLower() == objold.Status.ToLower()).FirstOrDefault() == null)
                    {
                        _db.SI_ApprovalFlowStatus.DeleteObject(objold);
                    }
                }

                foreach (var item in lstTopGrid)
                {
                    if (item.AppFolID.PassNull() == "" 
                        || item.RoleID.PassNull() == "" 
                        || item.Status.PassNull() == "") continue;
                    var obj = _db.SI_ApprovalFlowStatus.FirstOrDefault(p => p.BranchID.ToLower() == p.BranchID.ToLower() && p.AppFolID.ToLower() == item.AppFolID.ToLower()
                                                                        && p.RoleID.ToLower() == item.RoleID.ToLower()
                                                                        && p.Status.ToLower() == item.Status.ToLower());
                    if (obj != null)
                    {
                        Update_TopGrid(obj, item, false);
                    }
                    else
                    {
                        obj = new SI_ApprovalFlowStatus();
                        obj.ResetET();
                        Update_TopGrid(obj, item, true);
                        _db.SI_ApprovalFlowStatus.AddObject(obj);
                    }
                }
                #endregion

                #region Save SI_ApprovalFlowHandle

                var lstOld_SI_ApprovalFlowHandle = _db.SI_ApprovalFlowHandle.ToList();

                foreach (var objold in lstOld_SI_ApprovalFlowHandle)
                {
                    if (lstBotGrid.Where(p => p.Handle.ToLower() == objold.Handle.ToLower() && p.BranchID.ToLower() == objold.BranchID.ToLower()
                                            && p.Status.ToLower() == objold.Status.ToLower()
                                            && p.RoleID.ToLower() == objold.RoleID.ToLower()
                                            && p.AppFolID.ToLower() == objold.AppFolID.ToLower()).FirstOrDefault() == null)
                    {
                        _db.SI_ApprovalFlowHandle.DeleteObject(objold);
                    }
                }

                foreach (var item in lstBotGrid)
                {
                    if (item.Handle.PassNull() == ""
                        || item.Status.PassNull() == ""
                        || item.RoleID.PassNull() == ""
                        || item.AppFolID.PassNull() == "") continue;

                    var obj = _db.SI_ApprovalFlowHandle.FirstOrDefault(p => p.Handle.ToLower() == item.Handle.ToLower() && p.BranchID.ToLower() == item.BranchID.ToLower()
                                                                    && p.Status.ToLower() == item.Status.ToLower()
                                                                    && p.RoleID.ToLower() == item.RoleID.ToLower()
                                                                    && p.AppFolID.ToLower() == item.AppFolID.ToLower());
                    if (obj != null)
                    {
                        Update_BotGrid(obj, item, false);
                    }
                    else
                    {
                        obj = new SI_ApprovalFlowHandle();
                        obj.ResetET();
                        Update_BotGrid(obj, item, true);
                        _db.SI_ApprovalFlowHandle.AddObject(obj);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_TopGrid(SI_ApprovalFlowStatus t, SA02900_pgSI_ApprovalFlowStatus_Result s, bool isNew)
        {
            if (isNew)
            {
                t.AppFolID = s.AppFolID;
                t.RoleID = s.RoleID;
                t.Status = s.Status;
                t.BranchID = s.BranchID;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.LangStatus = s.LangStatus;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_BotGrid(SI_ApprovalFlowHandle t, SA02900_pgSI_ApprovalFlowHandle_Result s, bool isNew)
        {
            if (isNew)
            {
                t.AppFolID = s.AppFolID;
                t.RoleID = s.RoleID;
                t.Status = s.Status;
                t.Handle = s.Handle;
                t.BranchID = s.BranchID;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.LangHandle = s.LangHandle;
            t.ToStatus = s.ToStatus;
            t.ContentApprove = s.ContentApprove;
            t.MailSubject = s.MailSubject;
            t.MailApprove = s.MailApprove;
            t.ProcContent = s.ProcContent;
            t.MailTo = s.MailTo;
            t.MailCC = s.MailCC;
            t.ProcName = s.ProcName;
            t.Param00 = s.Param00;
            t.Param01 = s.Param01;
            t.Param02 = s.Param02;
            t.Param03 = s.Param03;
            t.Param04 = s.Param04;
            t.Param05 = s.Param05;


            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        public ActionResult SaveBot(FormCollection data, string AppFolID, string RoleID, string Status)
        {
            try
            {
                //StoreDataHandler dataHandler = new StoreDataHandler(data["lstTopGrid"]);
                //ChangeRecords<SA02900_pgSI_ApprovalFlowStatus_Result> lstTopGrid = dataHandler.BatchObjectData<SA02900_pgSI_ApprovalFlowStatus_Result>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstBotGrid"]);
                ChangeRecords<SA02900_pgSI_ApprovalFlowHandle_Result> lstBotGrid = dataHandler1.BatchObjectData<SA02900_pgSI_ApprovalFlowHandle_Result>();

                #region Save Bot Grid SI_ApprovalFlowHandle
                foreach (SA02900_pgSI_ApprovalFlowHandle_Result deleted in lstBotGrid.Deleted)
                {
                    var del = _db.SI_ApprovalFlowHandle.FirstOrDefault(p => p.Handle == deleted.Handle && p.AppFolID == AppFolID && p.RoleID == RoleID && p.Status == Status);
                    if (del != null)
                    {
                        _db.SI_ApprovalFlowHandle.DeleteObject(del);
                    }
                }

                lstBotGrid.Created.AddRange(lstBotGrid.Updated);

                foreach (SA02900_pgSI_ApprovalFlowHandle_Result curLang1 in lstBotGrid.Created)
                {
                    if (curLang1.Handle.PassNull() == "") continue;

                    var lang1 = _db.SI_ApprovalFlowHandle.FirstOrDefault(p => p.Handle == curLang1.Handle && p.AppFolID == AppFolID && p.RoleID == RoleID && p.Status == Status);

                    if (lang1 != null)
                    {
                        if (lang1.tstamp.ToHex() == curLang1.tstamp.ToHex())
                        {
                            Update_BotGrid(lang1, curLang1, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang1 = new SI_ApprovalFlowHandle();
                        lang1.AppFolID = AppFolID;
                        lang1.RoleID = RoleID;
                        lang1.Status = Status;
                        Update_BotGrid(lang1, curLang1, true);
                        _db.SI_ApprovalFlowHandle.AddObject(lang1);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        ////[HttpPost]
        ////public ActionResult DeleteAll(FormCollection data, string AppFolID, string RoleID, string Status)
        ////{
        ////    try
        ////    {
               
        ////        var lstTop = _db.SI_ApprovalFlowStatus.Where(p => p.AppFolID == AppFolID && p.RoleID == RoleID && p.Status == Status).ToList();
        ////        foreach (var item in lstTop)
        ////        {
        ////            _db.SI_ApprovalFlowStatus.DeleteObject(item);
        ////        }

        ////        var lstBot = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == AppFolID && p.RoleID == RoleID && p.Status == Status).ToList();
        ////        foreach (var item in lstBot)
        ////        {
        ////            _db.SI_ApprovalFlowHandle.DeleteObject(item);
        ////        }

        ////        _db.SaveChanges();
        ////        return Json(new { success = true });
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        if (ex is MessageException) return (ex as MessageException).ToMessage();
        ////        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        ////    }
        ////}
    }
}

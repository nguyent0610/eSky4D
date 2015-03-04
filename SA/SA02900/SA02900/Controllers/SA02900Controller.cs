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

        public ActionResult GetTopGrid()
        {           
            return this.Store(_db.SA02900_pgSI_ApprovalFlowStatus().ToList());
        }

        public ActionResult GetBotGrid(string AppFolID, string RoleID, string Status)
        {
            return this.Store(_db.SA02900_pgSI_ApprovalFlowHandle(AppFolID, RoleID, Status).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data,string AppFolID, string RoleID, string Status)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstTopGrid"]);
                ChangeRecords<SA02900_pgSI_ApprovalFlowStatus_Result> lstTopGrid = dataHandler.BatchObjectData<SA02900_pgSI_ApprovalFlowStatus_Result>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstBotGrid"]);
                ChangeRecords<SA02900_pgSI_ApprovalFlowHandle_Result> lstBotGrid = dataHandler1.BatchObjectData<SA02900_pgSI_ApprovalFlowHandle_Result>();
                
                #region Save Top Grid SI_ApprovalFlowStatus
                foreach (SA02900_pgSI_ApprovalFlowStatus_Result deleted in lstTopGrid.Deleted)
                {
                    var del = _db.SI_ApprovalFlowStatus.Where(p => p.AppFolID == deleted.AppFolID && p.RoleID == deleted.RoleID && p.Status == deleted.Status).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_ApprovalFlowStatus.DeleteObject(del);
                    }
                }

                lstTopGrid.Created.AddRange(lstTopGrid.Updated);

                foreach (SA02900_pgSI_ApprovalFlowStatus_Result curLang in lstTopGrid.Created)
                {
                    if (curLang.AppFolID.PassNull() == "" && curLang.RoleID.PassNull() == "" && curLang.Status.PassNull() == "") continue;

                    var lang = _db.SI_ApprovalFlowStatus.Where(p => p.AppFolID == curLang.AppFolID && p.RoleID == curLang.RoleID && p.Status == curLang.Status).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_TopGrid(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SI_ApprovalFlowStatus();
                        Update_TopGrid(lang, curLang, true);
                        _db.SI_ApprovalFlowStatus.AddObject(lang);
                    }
                }
                #endregion

                #region Save Bot Grid SI_ApprovalFlowHandle
                foreach (SA02900_pgSI_ApprovalFlowHandle_Result deleted in lstBotGrid.Deleted)
                {
                    var del = _db.SI_ApprovalFlowHandle.FirstOrDefault(p => p.Handle == deleted.Handle && p.AppFolID == AppFolID && p.RoleID == RoleID && p.Status ==Status);
                    if (del != null)
                    {
                        _db.SI_ApprovalFlowHandle.DeleteObject(del);
                    }
                }

                lstBotGrid.Created.AddRange(lstBotGrid.Updated);

                foreach (SA02900_pgSI_ApprovalFlowHandle_Result curLang1 in lstBotGrid.Created)
                {
                    if (curLang1.Handle.PassNull() == "") continue;

                    var lang1 = _db.SI_ApprovalFlowHandle.FirstOrDefault(p => p.Handle == curLang1.Handle&&p.AppFolID == AppFolID && p.RoleID == RoleID && p.Status == Status);

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

        private void Update_TopGrid(SI_ApprovalFlowStatus t, SA02900_pgSI_ApprovalFlowStatus_Result s, bool isNew)
        {
            if (isNew)
            {
                t.AppFolID = s.AppFolID;
                t.RoleID = s.RoleID;
                t.Status = s.Status;
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
                t.Handle = s.Handle;
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
    }
}

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
namespace SA00800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00800Controller : Controller
    {
        private string _screenNbr = "SA00800";
        private string _userName = Current.UserName;
        SA00800Entities _db = Util.CreateObjectContext<SA00800Entities>(true);

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

        
        //#region Get information Company
        public ActionResult GetSYS_ReportControl(String ReportNbr)
        {
            var rpt = _db.SYS_ReportControl.FirstOrDefault(p => p.ReportNbr == ReportNbr);
            return this.Store(rpt);
           
        }

        public ActionResult GetSYS_ReportParm(String ReportNbr)
        {
            return this.Store(_db.SA00800_pgSYS_ReportParm(ReportNbr).ToList());
        }

        //#endregion

        #region Save & Update
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string ReportNbr = data["cboReportNbr"];

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_ReportControl"]);
                ChangeRecords<SYS_ReportControl> lstSYS_ReportControl = dataHandler.BatchObjectData<SYS_ReportControl>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSYS_ReportParm"]);
                ChangeRecords<SA00800_pgSYS_ReportParm_Result> lstSYS_ReportParm = dataHandler1.BatchObjectData<SA00800_pgSYS_ReportParm_Result>();


                #region Save Header SYS_ReportControl
                lstSYS_ReportControl.Created.AddRange(lstSYS_ReportControl.Updated);
                foreach (SYS_ReportControl curHeader in lstSYS_ReportControl.Created)
                {
                    if (ReportNbr.PassNull() == "") continue;

                    var header = _db.SYS_ReportControl.FirstOrDefault(p => p.ReportNbr == ReportNbr);
                    if (header != null)
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(ref header, curHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        header = new SYS_ReportControl();
                        header.ReportNbr = ReportNbr;
                        header.Crtd_Datetime = DateTime.Now;
                        header.Crtd_Prog = _screenNbr;
                        header.Crtd_User = Current.UserName;
             
                        UpdatingHeader(ref header, curHeader);
                        _db.SYS_ReportControl.AddObject(header);
                    }
                }
                #endregion

                #region Save SYS_ReportParm
                foreach (SA00800_pgSYS_ReportParm_Result deleted in lstSYS_ReportParm.Deleted)
                {
                    var objDelete = _db.SYS_ReportParm.FirstOrDefault(p => p.ReportNbr == ReportNbr && p.ReportFormat == deleted.ReportFormat);
                    if (objDelete != null)
                    {
                        _db.SYS_ReportParm.DeleteObject(objDelete);
                    }
                }

                lstSYS_ReportParm.Created.AddRange(lstSYS_ReportParm.Updated);

                foreach (SA00800_pgSYS_ReportParm_Result curLang in lstSYS_ReportParm.Created)
                {
                    if (curLang.ReportFormat.PassNull() == "" || ReportNbr.PassNull()=="") continue;

                    var lang = _db.SYS_ReportParm.FirstOrDefault(p => p.ReportNbr.ToLower() == ReportNbr.ToLower() && p.ReportFormat.ToLower() == curLang.ReportFormat.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingSYS_ReportParm(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_ReportParm();
                        lang.ReportNbr = ReportNbr;

                        UpdatingSYS_ReportParm(lang, curLang, true);
                        _db.SYS_ReportParm.AddObject(lang);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, ReportNbr = ReportNbr });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        //Update SYS_ReportControl
        #region Update SYS_ReportControl
        private void UpdatingHeader(ref SYS_ReportControl t, SYS_ReportControl s)
        {
            t.Descr = s.Descr;
            t.ReportCap00 = s.ReportCap00;
            t.ReportName00 = s.ReportName00;
            t.ReportCap01 = s.ReportCap01;
            t.ReportName01 = s.ReportName01;
            t.ReportCap02 = s.ReportCap02;
            t.ReportName02 = s.ReportName02;
            t.ReportCap03 = s.ReportCap03;
            t.ReportName03 = s.ReportName03;
            t.ReportCap04 = s.ReportCap04;
            t.ReportName04 = s.ReportName04;
            t.ReportCap05 = s.ReportCap05;
            t.ReportName05 = s.ReportName05;
            t.ReportCap06 = s.ReportCap06;
            t.ReportName06 = s.ReportName06;
            t.ReportCap07 = s.ReportCap07;
            t.ReportName07 = s.ReportName07;
            t.RunBefore = s.RunBefore;
            t.RunAfter = s.RunAfter;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        #endregion
        //Update SYS_ReportParm
        #region Update SYS_ReportParm
        private void UpdatingSYS_ReportParm(SYS_ReportParm t, SA00800_pgSYS_ReportParm_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ReportFormat = s.ReportFormat;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.StringCap00 = s.StringCap00;
            t.StringCap01 = s.StringCap01;
            t.StringCap02 = s.StringCap02;
            t.StringCap03 = s.StringCap03;
            t.DateCap00 = s.DateCap00;
            t.DateCap01 = s.DateCap01;
            t.DateCap02 = s.DateCap02;
            t.DateCap03 = s.DateCap03;
            t.BooleanCap00 = s.BooleanCap00;
            t.BooleanCap01 = s.BooleanCap01;
            t.BooleanCap02 = s.BooleanCap02;
            t.BooleanCap03 = s.BooleanCap03;
            t.PPV_Proc00 = s.PPV_Proc00;
            t.PPV_Proc01 = s.PPV_Proc01;
            t.PPV_Proc02 = s.PPV_Proc02;
            t.PPV_Proc03 = s.PPV_Proc03;
            t.ListCap00 = s.ListCap00;
            t.ListCap01 = s.ListCap01;
            t.ListCap02 = s.ListCap02;
            t.ListCap03 = s.ListCap03;
            t.ListProc00 = s.ListProc00;
            t.ListProc01 = s.ListProc01;
            t.ListProc02 = s.ListProc02;
            t.ListProc03 = s.ListProc03;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
        #endregion

        #region Delete All
        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string ReportNbr = data["cboReportNbr"];
                var cpny = _db.SYS_ReportControl.FirstOrDefault(p => p.ReportNbr == ReportNbr);
                if (cpny != null)
                {
                    _db.SYS_ReportControl.DeleteObject(cpny);
                }

                var lstAddr = _db.SYS_ReportParm.Where(p => p.ReportNbr == ReportNbr).ToList();
                foreach (var item in lstAddr)
                {
                    _db.SYS_ReportParm.DeleteObject(item);
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
        #endregion
    }
}

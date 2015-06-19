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
namespace OM24000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM24000Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "OM24000";
        private string _userName = Current.UserName;
        OM24000Entities _db = Util.CreateObjectContext<OM24000Entities>(false);
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

        public ActionResult GetPPC_DiscConsumers(string BranchID, string SlsperID, string CustID,DateTime FromDate,DateTime ToDate)
        {
            return this.Store(_db.OM24000_pgLoadGrid(BranchID, SlsperID, CustID, FromDate, ToDate).ToList());
        }

        //[HttpPost]
        //public ActionResult Save(FormCollection data)
        //{
        //    try
        //    {

        //        StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Language"]);
        //        ChangeRecords<OM24000_pgLoadGrid_Result> lstLang = dataHandler.BatchObjectData<OM24000_pgLoadGrid_Result>();
        //        foreach (OM24000_pgLoadGrid_Result deleted in lstLang.Deleted)
        //        {
        //            var del = _db.SYS_Language.Where(p => p.Code == deleted.Code).FirstOrDefault();
        //            if (del != null)
        //            {
        //                _db.SYS_Language.DeleteObject(del);
        //            }
        //        }

        //        lstLang.Created.AddRange(lstLang.Updated);

        //        foreach (OM24000_pgLoadGrid_Result curLang in lstLang.Created)
        //        {
        //            if (curLang.Code.PassNull() == "") continue;

        //            var lang = _db.SYS_Language.Where(p => p.Code.ToLower() == curLang.Code.ToLower()).FirstOrDefault();

        //            if (lang != null)
        //            {
        //                if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
        //                {
        //                    Update_Language(lang, curLang, false);
        //                }
        //                else
        //                {
        //                    throw new MessageException(MessageType.Message, "19");
        //                }
        //            }
        //            else
        //            {
        //                lang = new SYS_Language();
        //                Update_Language(lang, curLang, true);
        //                _db.SYS_Language.AddObject(lang);
        //            }
        //        }

        //        _db.SaveChanges();
         

        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}

        //private void Update_Language(SYS_Language t, OM24000_pgLoadGrid_Result s, bool isNew)
        //{
        //    if (isNew)
        //    {
        //        t.Code = s.Code;
        //        t.Crtd_Datetime = DateTime.Now;
        //        t.Crtd_Prog = _screenNbr;
        //        t.Crtd_User = _userName;
        //    }
        //    t.Lang00 = s.Lang00;
        //    t.Lang01 = s.Lang01;
        //    t.Lang02 = s.Lang02;
        //    t.Lang03 = s.Lang03;
        //    t.Lang04 = s.Lang04;

        //    t.LUpd_Datetime = DateTime.Now;
        //    t.LUpd_Prog = _screenNbr;
        //    t.LUpd_User = _userName;
        //}
      

    }
}

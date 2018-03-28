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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;

namespace SA03600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA03600Controller : Controller
    {
        private string _screenNbr = "SA03600";
        private string _userName = Current.UserName;
        SA03600Entities _db = Util.CreateObjectContext<SA03600Entities>(false);
        private JsonResult _logMessage;
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

        public ActionResult GetApp_ScreenSummary()
        {
            return this.Store(_db.SA03600_pgScreenSummary(_userName,Current.CpnyID,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstApp_ScreenSummary"]);
                ChangeRecords<SA03600_pgScreenSummary_Result> lstSA_ScreenSummary = dataHandler.BatchObjectData<SA03600_pgScreenSummary_Result>();

                lstSA_ScreenSummary.Created.AddRange(lstSA_ScreenSummary.Updated);

                foreach (SA03600_pgScreenSummary_Result del in lstSA_ScreenSummary.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSA_ScreenSummary.Created.Where(p => p.ScreenNumber == del.ScreenNumber).Count() > 0)
                    {
                        lstSA_ScreenSummary.Created.Where(p => p.ScreenNumber == del.ScreenNumber).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SA_ScreenSummary.ToList().Where(p => p.ScreenNumber == del.ScreenNumber).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SA_ScreenSummary.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SA03600_pgScreenSummary_Result curLang in lstSA_ScreenSummary.Created)
                {
                    if (curLang.ScreenNumber.PassNull() == "") continue;

                    var lang = _db.SA_ScreenSummary.Where(p => p.ScreenNumber.ToLower() == curLang.ScreenNumber.ToLower()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Language(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SA_ScreenSummary();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SA_ScreenSummary.AddObject(lang);
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true});
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_Language(SA_ScreenSummary t, SA03600_pgScreenSummary_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ScreenNumber = s.ScreenNumber;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_USer = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

            

        }

    }
}

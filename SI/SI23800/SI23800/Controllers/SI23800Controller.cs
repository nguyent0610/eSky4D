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

namespace SI23800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI23800Controller : Controller
    {
        private string _screenNbr = "SI23800";
        private string _userName = Current.UserName;
        SI23800Entities _db = Util.CreateObjectContext<SI23800Entities>(false);
        private JsonResult _logMessage;
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

        public ActionResult GetSI_Stand()
        {
            return this.Store(_db.SI23800_pgSI_Stand(Current.CpnyID,Current.UserName,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Stand"]);
                ChangeRecords<SI23800_pgSI_Stand_Result> lstSI_Stand = dataHandler.BatchObjectData<SI23800_pgSI_Stand_Result>();

                lstSI_Stand.Created.AddRange(lstSI_Stand.Updated);

                foreach (SI23800_pgSI_Stand_Result del in lstSI_Stand.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSI_Stand.Created.Where(p => p.StandID == del.StandID).Count() > 0)
                    {
                        lstSI_Stand.Created.Where(p => p.StandID == del.StandID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Stand.ToList().Where(p => p.StandID == del.StandID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Stand.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SI23800_pgSI_Stand_Result curLang in lstSI_Stand.Created)
                {
                    if (curLang.StandID.PassNull() == "") continue;

                    var lang = _db.SI_Stand.Where(p => p.StandID.ToLower() == curLang.StandID.ToLower()).FirstOrDefault();

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
                        lang = new SI_Stand();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SI_Stand.AddObject(lang);
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

        private void Update_Language(SI_Stand t, SI23800_pgSI_Stand_Result s, bool isNew)
        {
            if (isNew)
            {
                t.StandID = s.StandID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Lupd_DateTime = DateTime.Now;
            t.Lupd_Prog = _screenNbr;
            t.Lupd_User = _userName;
        }

    }
}

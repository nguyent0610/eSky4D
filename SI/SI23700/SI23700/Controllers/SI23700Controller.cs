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

namespace SI23700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI23700Controller : Controller
    {
        private string _screenNbr = "SI23700";
        private string _userName = Current.UserName;
        SI23700Entities _db = Util.CreateObjectContext<SI23700Entities>(false);
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
            return this.Store(_db.SI23700_pgSI_Display(Current.CpnyID,Current.UserName,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Display"]);
                ChangeRecords<SI23700_pgSI_Display_Result> lstSI_Display = dataHandler.BatchObjectData<SI23700_pgSI_Display_Result>();

                lstSI_Display.Created.AddRange(lstSI_Display.Updated);

                foreach (SI23700_pgSI_Display_Result del in lstSI_Display.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSI_Display.Created.Where(p => p.DisplayID == del.DisplayID).Count() > 0)
                    {
                        lstSI_Display.Created.Where(p => p.DisplayID == del.DisplayID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Display.ToList().Where(p => p.DisplayID == del.DisplayID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Display.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SI23700_pgSI_Display_Result curLang in lstSI_Display.Created)
                {
                    if (curLang.DisplayID.PassNull() == "") continue;

                    var lang = _db.SI_Display.Where(p => p.DisplayID.ToLower() == curLang.DisplayID.ToLower()).FirstOrDefault();

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
                        lang = new SI_Display();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SI_Display.AddObject(lang);
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

        private void Update_Language(SI_Display t, SI23700_pgSI_Display_Result s, bool isNew)
        {
            if (isNew)
            {
                t.DisplayID = s.DisplayID;
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

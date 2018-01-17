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

namespace SI23600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI23600Controller : Controller
    {
        private string _screenNbr = "SI23600";
        private string _userName = Current.UserName;
        SI23600Entities _db = Util.CreateObjectContext<SI23600Entities>(false);
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

        public ActionResult GetSI_Size()
        {
            return this.Store(_db.SI23600_pgSI_Size(Current.CpnyID,Current.UserName,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Size"]);
                ChangeRecords<SI23600_pgSI_Size_Result> lstSI_Size = dataHandler.BatchObjectData<SI23600_pgSI_Size_Result>();

                lstSI_Size.Created.AddRange(lstSI_Size.Updated);

                foreach (SI23600_pgSI_Size_Result del in lstSI_Size.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSI_Size.Created.Where(p => p.SizeID == del.SizeID).Count() > 0)
                    {
                        lstSI_Size.Created.Where(p => p.SizeID == del.SizeID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Size.ToList().Where(p => p.SizeID == del.SizeID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Size.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SI23600_pgSI_Size_Result curLang in lstSI_Size.Created)
                {
                    if (curLang.SizeID.PassNull() == "") continue;

                    var lang = _db.SI_Size.Where(p => p.SizeID.ToLower() == curLang.SizeID.ToLower()).FirstOrDefault();

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
                        lang = new SI_Size();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SI_Size.AddObject(lang);
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

        private void Update_Language(SI_Size t, SI23600_pgSI_Size_Result s, bool isNew)
        {
            if (isNew)
            {
                t.SizeID = s.SizeID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.SizeType = s.SizeType;
            t.Descr = s.Descr;
            t.Lupd_DateTime = DateTime.Now;
            t.Lupd_Prog = _screenNbr;
            t.Lupd_User = _userName;
        }

    }
}

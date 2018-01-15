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

namespace SI23500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI23500Controller : Controller
    {
        private string _screenNbr = "SI23500";
        private string _userName = Current.UserName;
        SI23500Entities _db = Util.CreateObjectContext<SI23500Entities>(false);
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

        public ActionResult GetSI_Brand()
        {
            return this.Store(_db.SI23500_pgSI_Brand(Current.CpnyID,Current.UserName,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Brand"]);
                ChangeRecords<SI23500_pgSI_Brand_Result> lstSI_Brand = dataHandler.BatchObjectData<SI23500_pgSI_Brand_Result>();

                lstSI_Brand.Created.AddRange(lstSI_Brand.Updated);

                foreach (SI23500_pgSI_Brand_Result del in lstSI_Brand.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSI_Brand.Created.Where(p => p.BrandID == del.BrandID).Count() > 0)
                    {
                        lstSI_Brand.Created.Where(p => p.BrandID == del.BrandID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Brand.ToList().Where(p => p.BrandID == del.BrandID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Brand.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SI23500_pgSI_Brand_Result curLang in lstSI_Brand.Created)
                {
                    if (curLang.BrandID.PassNull() == "") continue;

                    var lang = _db.SI_Brand.Where(p => p.BrandID.ToLower() == curLang.BrandID.ToLower()).FirstOrDefault();

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
                        lang = new SI_Brand();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SI_Brand.AddObject(lang);
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

        private void Update_Language(SI_Brand t, SI23500_pgSI_Brand_Result s, bool isNew)
        {
            if (isNew)
            {
                t.BrandID = s.BrandID;
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

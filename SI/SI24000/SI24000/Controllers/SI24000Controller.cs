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
using System.Reflection;
using System.Collections;
using System.Runtime.Caching;
namespace SI24000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI24000Controller : Controller
    {
        private string _screenNbr = "SI24000";
        private string _userName = Current.UserName;
        SI24000Entities _db = Util.CreateObjectContext<SI24000Entities>(false);
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
        public ActionResult GetSI_SalesSelling()
        {           
            return this.Store(_db.SI24000_pgLoadCategory(Current.CpnyID, Current.UserName, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            try
            {


                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Category"]);
                ChangeRecords<SI24000_pgLoadCategory_Result> lstLang = dataHandler.BatchObjectData<SI24000_pgLoadCategory_Result>();
                lstLang.Created.AddRange(lstLang.Updated);
                foreach (SI24000_pgLoadCategory_Result del in lstLang.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstLang.Created.Where(p => p.Code == del.Code.ToUpper().Trim()).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.Code == del.Code.ToUpper().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Category.ToList().Where(p => p.Code == del.Code.ToUpper().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Category.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SI24000_pgLoadCategory_Result curLang in lstLang.Created)
                {
                    if (curLang.Code.PassNull() == "") continue;

                    var lang = _db.SI_Category.Where(p => p.Code == curLang.Code.ToUpper().Trim()).FirstOrDefault();

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
                        lang = new SI_Category();
                        Update_Language(lang, curLang, true);
                        _db.SI_Category.AddObject(lang);
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
        private void Update_Language(SI_Category t, SI24000_pgLoadCategory_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code.ToUpper().Trim();
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }            
    }
}

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
namespace SA00400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00400Controller : Controller
    {
        private string _screenNbr = "SA00400";
        private string _userName = Current.UserName;

        SA00400Entities _db = Util.CreateObjectContext<SA00400Entities>(true);

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

        public ActionResult GetData()
        {
            return this.Store(_db.SA00400_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SYS_ScreenCat> lstLang = dataHandler.BatchObjectData<SYS_ScreenCat>();
                lstLang.Created.AddRange(lstLang.Updated);
                foreach (SYS_ScreenCat del in lstLang.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstLang.Created.Where(p => p.CatID == del.CatID).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.CatID == del.CatID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SYS_ScreenCat.ToList().Where(p => p.CatID == del.CatID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SYS_ScreenCat.DeleteObject(objDel);
                        }
                    }
                }
                //foreach (SYS_ScreenCat deleted in lstLang.Deleted)
                //{
                //    var del = _db.SYS_ScreenCat.Where(p => p.CatID == deleted.CatID).FirstOrDefault();
                //    if (del != null)
                //    {
                //        _db.SYS_ScreenCat.DeleteObject(del);
                //    }
                //}

                //lstLang.Created.AddRange(lstLang.Updated);

                foreach (SYS_ScreenCat curLang in lstLang.Created)
                {
                    if (curLang.CatID.PassNull() == "") continue;

                    var lang = _db.SYS_ScreenCat.Where(p => p.CatID.ToLower() == curLang.CatID.ToLower()).FirstOrDefault();

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
                        lang = new SYS_ScreenCat();
                        Update_Language(lang, curLang, true);
                        _db.SYS_ScreenCat.AddObject(lang);
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

        private void Update_Language(SYS_ScreenCat t, SYS_ScreenCat s, bool isNew)
        {
            if (isNew)
            {
                t.CatID = s.CatID;
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

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
namespace SA00100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00100Controller : Controller
    {
        private string _screenNbr = "SA00100";
        private string _userName = Current.UserName;

        SA00100Entities _db = Util.CreateObjectContext<SA00100Entities>(true);

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
            return this.Store(_db.SA00100_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SYS_Screen> lstLang = dataHandler.BatchObjectData<SYS_Screen>();
                //foreach (SYS_Screen deleted in lstLang.Deleted)
                //{
                //    var del = _db.SYS_Screen.Where(p => p.ScreenNumber == deleted.ScreenNumber).FirstOrDefault();
                //    if (del != null)
                //    {
                //        _db.SYS_Screen.DeleteObject(del);
                //    }
                //}

                //lstLang.Created.AddRange(lstLang.Updated);
                lstLang.Created.AddRange(lstLang.Updated);
                foreach (SYS_Screen del in lstLang.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstLang.Created.Where(p => p.ScreenNumber == del.ScreenNumber).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.ScreenNumber == del.ScreenNumber).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SYS_Screen.ToList().Where(p => p.ScreenNumber == del.ScreenNumber).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SYS_Screen.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SYS_Screen curLang in lstLang.Created)
                {
                    if (curLang.ScreenNumber.PassNull() == "") continue;

                    var lang = _db.SYS_Screen.Where(p => p.ScreenNumber.ToLower() == curLang.ScreenNumber.ToLower()).FirstOrDefault();

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
                        lang = new SYS_Screen();
                        Update_Language(lang, curLang, true);
                        _db.SYS_Screen.AddObject(lang);
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

        private void Update_Language(SYS_Screen t, SYS_Screen s, bool isNew)
        {
            if (isNew)
            {
                t.ScreenNumber = s.ScreenNumber;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_USer = _userName;
            }

            t.Descr = s.Descr;
            t.ModuleID = s.ModuleID;
            t.ScreenType = s.ScreenType;
            t.SortNbr = s.SortNbr;
            t.CatID = s.CatID;
            t.ExecPath = s.ExecPath;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

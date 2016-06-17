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
namespace SA00200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00200Controller : Controller
    {
        private string _screenNbr = "SA00200";
        private string _userName = Current.UserName;

        SA00200Entities _db = Util.CreateObjectContext<SA00200Entities>(true);

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
            return this.Store(_db.SA00200_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SYS_Module> lstLang = dataHandler.BatchObjectData<SYS_Module>();
                lstLang.Created.AddRange(lstLang.Updated);
                foreach (SYS_Module del in lstLang.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstLang.Created.Where(p => p.ModuleCode == del.ModuleCode).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.ModuleCode == del.ModuleCode).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SYS_Module.ToList().Where(p => p.ModuleCode == del.ModuleCode).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SYS_Module.DeleteObject(objDel);
                        }
                    }
                }
                //foreach (SYS_Module deleted in lstLang.Deleted)
                //{
                //    var del = _db.SYS_Module.Where(p => p.ModuleCode == deleted.ModuleCode && p.ModuleID==deleted.ModuleID).FirstOrDefault();
                //    if (del != null)
                //    {
                //        _db.SYS_Module.DeleteObject(del);
                //    }
                //}

                //lstLang.Created.AddRange(lstLang.Updated);

                foreach (SYS_Module curLang in lstLang.Created)
                {
                    if (curLang.ModuleCode.PassNull() == "" || curLang.ModuleID.PassNull()=="") continue;

                    var lang = _db.SYS_Module.Where(p => p.ModuleCode.ToLower() == curLang.ModuleCode.ToLower() && p.ModuleID.ToLower() == curLang.ModuleID.ToLower()).FirstOrDefault();

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
                        lang = new SYS_Module();
                        Update_Language(lang, curLang, true);
                        _db.SYS_Module.AddObject(lang);
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

        private void Update_Language(SYS_Module t, SYS_Module s, bool isNew)
        {
            if (isNew)
            {
                t.ModuleCode = s.ModuleCode;
                t.ModuleID = s.ModuleID;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Active = s.Active;
            t.CatID = s.CatID;
            t.ModuleName = s.ModuleName;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

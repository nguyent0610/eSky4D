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

namespace SA02800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA02800Controller : Controller
    {
        private string _screenNbr = "SA02800";
        private string _userName = Current.UserName;
        SA02800Entities _db = Util.CreateObjectContext<SA02800Entities>(true);
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

        public ActionResult GetSYS_Role()
        {
            return this.Store(_db.SA02800_pgSYS_Role().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Role"]);
                ChangeRecords<SA02800_pgSYS_Role_Result> lstSYS_Role = dataHandler.BatchObjectData<SA02800_pgSYS_Role_Result>();

                lstSYS_Role.Created.AddRange(lstSYS_Role.Updated);

                foreach (SA02800_pgSYS_Role_Result del in lstSYS_Role.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSYS_Role.Created.Where(p => p.RoleID == del.RoleID).Count() > 0)
                    {
                        lstSYS_Role.Created.Where(p => p.RoleID == del.RoleID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SYS_Role.ToList().Where(p => p.RoleID == del.RoleID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SYS_Role.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SA02800_pgSYS_Role_Result curLang in lstSYS_Role.Created)
                {
                    if (curLang.RoleID.PassNull() == "") continue;

                    var lang = _db.SYS_Role.Where(p => p.RoleID.ToLower() == curLang.RoleID.ToLower()).FirstOrDefault();

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
                        lang = new SYS_Role();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SYS_Role.AddObject(lang);
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

        private void Update_Language(SYS_Role t, SA02800_pgSYS_Role_Result s, bool isNew)
        {
            if (isNew)
            {
                t.RoleID = s.RoleID;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Desc = s.Desc;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

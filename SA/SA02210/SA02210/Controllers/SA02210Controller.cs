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

namespace SA02210.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA02210Controller : Controller
    {
        private string _screenNbr = "SA02210";
        private string _userName = Current.UserName;
        SA02210Entities _db = Util.CreateObjectContext<SA02210Entities>(true);

       
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


        public ActionResult GetSYS_FavouriteGroupUser(string UserGroupID)
        {
            return this.Store(_db.SA02210_pgSYS_FavouriteGroupUser(UserGroupID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string UserGroupID = data["cboUserGroupID"];
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_FavouriteGroupUser"]);
                ChangeRecords<SA02210_pgSYS_FavouriteGroupUser_Result> lstSYS_FavouriteGroupUser = dataHandler.BatchObjectData<SA02210_pgSYS_FavouriteGroupUser_Result>();

                foreach (SA02210_pgSYS_FavouriteGroupUser_Result deleted in lstSYS_FavouriteGroupUser.Deleted)
                {
                    var del = _db.SYS_FavouriteGroupUser.Where(p => p.UserGroupID == UserGroupID && p.ScreenNumber == deleted.ScreenNumber).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_FavouriteGroupUser.DeleteObject(del);
                    }
                }

                lstSYS_FavouriteGroupUser.Created.AddRange(lstSYS_FavouriteGroupUser.Updated);

                foreach (SA02210_pgSYS_FavouriteGroupUser_Result curLang in lstSYS_FavouriteGroupUser.Created)
                {
                    if (curLang.ScreenNumber.PassNull() == "") continue;

                    var lang = _db.SYS_FavouriteGroupUser.FirstOrDefault(p => p.UserGroupID.ToLower() == UserGroupID.ToLower() && p.ScreenNumber.ToLower() == curLang.ScreenNumber.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingSYS_FavouriteGroupUser(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_FavouriteGroupUser();
                        lang.UserGroupID = UserGroupID;
                        UpdatingSYS_FavouriteGroupUser(lang, curLang, true);
                        _db.SYS_FavouriteGroupUser.AddObject(lang);
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


        private void UpdatingSYS_FavouriteGroupUser(SYS_FavouriteGroupUser t, SA02210_pgSYS_FavouriteGroupUser_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ScreenNumber = s.ScreenNumber;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CodeGroup = s.CodeGroup;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

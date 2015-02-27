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
namespace SA02200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA02200Controller : Controller
    {
        private string _screenNbr = "SA02200";
        private string _userName = Current.UserName;
        SA02200Entities _db = Util.CreateObjectContext<SA02200Entities>(true);
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
        public ActionResult GetSYS_Favourite()
        {
            return this.Store(_db.SA02200_pgSYS_Favourite().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Favourite"]);
                ChangeRecords<SA02200_pgSYS_Favourite_Result> lstSYS_Favourite = dataHandler.BatchObjectData<SA02200_pgSYS_Favourite_Result>();
                foreach (SA02200_pgSYS_Favourite_Result deleted in lstSYS_Favourite.Deleted)
                {
                    var del = _db.SYS_Favourite.Where(p => p.ScreenNumber == deleted.ScreenNumber).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_Favourite.DeleteObject(del);
                    }
                }

                lstSYS_Favourite.Created.AddRange(lstSYS_Favourite.Updated);

                foreach (SA02200_pgSYS_Favourite_Result curLang in lstSYS_Favourite.Created)
                {
                    if (curLang.ScreenNumber.PassNull() == "") continue;

                    var lang = _db.SYS_Favourite.Where(p => p.ScreenNumber.ToLower() == curLang.ScreenNumber.ToLower()).FirstOrDefault();

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
                        lang = new SYS_Favourite();
                        Update_Language(lang, curLang, true);
                        _db.SYS_Favourite.AddObject(lang);
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
        private void Update_Language(SYS_Favourite t, SA02200_pgSYS_Favourite_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ScreenNumber = s.ScreenNumber;
                t.UserName = _userName;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
      


        
    }
}

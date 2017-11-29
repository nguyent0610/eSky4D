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
namespace IN23300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN23300Controller : Controller
    {
        private string _screenNbr = "IN23300";
        private string _userName = Current.UserName;
        IN23300Entities _db = Util.CreateObjectContext<IN23300Entities>(false);
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

        public ActionResult GetSYS_AccessDetRights(string userID, string type)
        {
            return this.Store(_db.IN23300_pgAccessRightsScreen(userID, type, Current.UserName, Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data, String recType, String userID, String cpnyID)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_AccessDetRights"]);
                ChangeRecords<IN23300_pgAccessRightsScreen_Result> lstSYS_AccessDetRights = dataHandler.BatchObjectData<IN23300_pgAccessRightsScreen_Result>();

                //foreach (IN23300_pgAccessRightsScreen_Result deleted in lstSYS_AccessDetRights.Deleted)
                //{
                //    var del = _db.IN_ReasonCodeRight.FirstOrDefault(p => p.);
                //    if (del != null)
                //    {
                //        _db.SYS_AccessDetRights.DeleteObject(del);
                //    }
                //}

                lstSYS_AccessDetRights.Created.AddRange(lstSYS_AccessDetRights.Updated);

                foreach (IN23300_pgAccessRightsScreen_Result curLang in lstSYS_AccessDetRights.Created)
                {
                    var lang = _db.IN_ReasonCodeRight.FirstOrDefault(p => p.ReasonCD.ToLower() == curLang.ReasonCD.ToLower() && p.RecType.ToLower() == curLang.RecType.ToLower() && p.UserID.ToLower() == curLang.UserID.ToLower());
                    if (curLang.CheckApplyFor == false)
                    {
                        if (curLang != null)
                        {
                            _db.IN_ReasonCodeRight.DeleteObject(lang);
                        }
                    }
                    else if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_AccessDetRights(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new IN_ReasonCodeRight();
                        lang.ResetET();
                        lang.ReasonCD = curLang.ReasonCD;
                        lang.UserID = userID;
                        lang.RecType = recType;
                        Update_AccessDetRights(lang, curLang, true);
                        _db.IN_ReasonCodeRight.AddObject(lang);
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
        private void Update_AccessDetRights(IN_ReasonCodeRight t, IN23300_pgAccessRightsScreen_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Allow = s.CheckApplyFor != null ? (bool)s.CheckApplyFor : false;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
    }
}

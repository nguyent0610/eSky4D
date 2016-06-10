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
namespace SA00700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00700Controller : Controller
    {
        private string _screenNbr = "SA00700";
        private string _userName = Current.UserName;
        SA00700Entities _db = Util.CreateObjectContext<SA00700Entities>(true);
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
        public ActionResult GetSYS_AccessDetRights(string cpnyID, string userID, string type, string module)
        {
            return this.Store(_db.SA00700_pgAccessRightsScreen(cpnyID, userID, type, module, Current.UserName, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data, String recType, String userID, String cpnyID)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_AccessDetRights"]);
                ChangeRecords<SA00700_pgAccessRightsScreen_Result> lstSYS_AccessDetRights = dataHandler.BatchObjectData<SA00700_pgAccessRightsScreen_Result>();
                foreach (SA00700_pgAccessRightsScreen_Result deleted in lstSYS_AccessDetRights.Deleted)
                {
                    var del = _db.SYS_AccessDetRights.FirstOrDefault(p => p.ScreenNumber == deleted.ScreenNumber 
                                                                        && p.UserID == userID
                                                                        && p.CpnyID == cpnyID
                                                                        && p.RecType == recType);
                    if (del != null)
                    {
                        _db.SYS_AccessDetRights.DeleteObject(del);
                    }
                }

                lstSYS_AccessDetRights.Created.AddRange(lstSYS_AccessDetRights.Updated);

                foreach (SA00700_pgAccessRightsScreen_Result curLang in lstSYS_AccessDetRights.Created)
                {
                    if (curLang.ScreenNumber.PassNull() == "" || userID.PassNull() == "" || recType.PassNull()=="" ) continue;

                    var lang = _db.SYS_AccessDetRights.FirstOrDefault(p => p.ScreenNumber.ToLower() == curLang.ScreenNumber.ToLower() 
                                                                        && p.UserID.ToLower() == userID.ToLower()
                                                                        && p.CpnyID.ToLower() == cpnyID.ToLower()
                                                                        && p.RecType.ToLower() == recType.ToLower());

                    if (lang != null)
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
                        lang = new SYS_AccessDetRights();
                        lang.ScreenNumber = curLang.ScreenNumber;
                        lang.CpnyID = cpnyID;
                        lang.UserID = userID;
                        lang.RecType = recType;
                        Update_AccessDetRights(lang, curLang, true);
                        _db.SYS_AccessDetRights.AddObject(lang);
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
        private void Update_AccessDetRights(SYS_AccessDetRights t, SA00700_pgAccessRightsScreen_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.InitRights = s.InitRights != null ? (bool)s.InitRights : false;
            t.InsertRights = s.InsertRights != null ? (bool)s.InsertRights : false;
            t.UpdateRights = s.UpdateRights != null ? (bool)s.UpdateRights : false;
            t.DeleteRights = s.DeleteRights != null ? (bool)s.DeleteRights : false;
            t.ViewRights = s.ViewRights != null ? (bool)s.ViewRights : false;
            t.ReleaseRights = s.ReleaseRights != null ? (bool)s.ReleaseRights : false;
            t.DatabaseName = Current.DBSys;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
    }
}

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
namespace SA01300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA01300Controller : Controller
    {
        private string _screenNbr = "SA01300";
        private string _userName = Current.UserName;

        SA01300Entities _db = Util.CreateObjectContext<SA01300Entities>(true);

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

        public ActionResult GetData()
        {
            return this.Store(_db.SA01300_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Configurations"]);
                ChangeRecords<SYS_Configurations> lstSYS_Configurations = dataHandler.BatchObjectData<SYS_Configurations>();
                foreach (SYS_Configurations deleted in lstSYS_Configurations.Deleted)
                {
                    var del = _db.SYS_Configurations.Where(p => p.Code == deleted.Code).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_Configurations.DeleteObject(del);
                    }
                }

                lstSYS_Configurations.Created.AddRange(lstSYS_Configurations.Updated);

                foreach (SYS_Configurations curLang in lstSYS_Configurations.Created)
                {
                    if (curLang.Code.PassNull() == "") continue;

                    var lang = _db.SYS_Configurations.Where(p => p.Code.ToLower() == curLang.Code.ToLower()).FirstOrDefault();

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
                        lang = new SYS_Configurations();
                        Update_Language(lang, curLang, true);
                        _db.SYS_Configurations.AddObject(lang);
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

        private void Update_Language(SYS_Configurations t, SYS_Configurations s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.DateVal = s.DateVal == null ? DateTime.Now : (s.DateVal.Year == 1 ? DateTime.Now : s.DateVal);
            t.FloatVal = s.FloatVal;
            t.IntVal = s.IntVal;
            t.TextVal = s.TextVal == "" ? "none" : s.TextVal;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

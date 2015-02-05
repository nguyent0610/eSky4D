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
namespace SI20700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20700Controller : Controller
    {
        private string _screenNbr = "SI20700";
        private string _userName = Current.UserName;

        SI20700Entities _db = Util.CreateObjectContext<SI20700Entities>(false);

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
            return this.Store(_db.SI20700_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SI_State> lstLang = dataHandler.BatchObjectData<SI_State>();
                foreach (SI_State deleted in lstLang.Deleted)
                {
                    var del = _db.SI_State.Where(p => p.Country == deleted.Country&&p.State==deleted.State).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_State.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (SI_State curLang in lstLang.Created)
                {
                    if (curLang.Country.PassNull() == "") continue;

                    var lang = _db.SI_State.Where(p => p.Country.ToLower() == curLang.Country.ToLower() && p.State.ToLower() == curLang.State.ToLower()).FirstOrDefault();

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
                        lang = new SI_State();
                        Update_Language(lang, curLang, true);
                        _db.SI_State.AddObject(lang);
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

        private void Update_Language(SI_State t, SI_State s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.State = s.State;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;
            t.Territory = s.Territory;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

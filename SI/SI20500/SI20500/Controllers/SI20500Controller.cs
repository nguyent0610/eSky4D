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
namespace SI20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20500Controller : Controller
    {
        private string _screenNbr = "SI20500";
        private string _userName = Current.UserName;

        SI20500Entities _db = Util.CreateObjectContext<SI20500Entities>(false);

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
            return this.Store(_db.SI20500_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_City"]);
                ChangeRecords<SI20500_pgLoadGrid_Result> lstSI_City = dataHandler.BatchObjectData<SI20500_pgLoadGrid_Result>();
                foreach (SI20500_pgLoadGrid_Result deleted in lstSI_City.Deleted)
                {
                    var del = _db.SI_City.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.City == deleted.City).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_City.DeleteObject(del);
                    }
                }

                lstSI_City.Created.AddRange(lstSI_City.Updated);

                foreach (SI20500_pgLoadGrid_Result curLang in lstSI_City.Created)
                {
                    if (curLang.Country.PassNull() == "") continue;

                    var lang = _db.SI_City.Where(p => p.Country.ToLower() == curLang.Country.ToLower() && p.State.ToLower() == curLang.State.ToLower() && p.City.ToLower() == curLang.City.ToLower()).FirstOrDefault();

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
                        lang = new SI_City();
                        Update_Language(lang, curLang, true);
                        _db.SI_City.AddObject(lang);
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

        private void Update_Language(SI_City t, SI20500_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.State = s.State;
                t.City = s.City;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Name = s.Name;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

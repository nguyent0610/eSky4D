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
namespace SI20900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20900Controller : Controller
    {
        private string _screenNbr = "SI20900";
        private string _userName = Current.UserName;

        SI20900Entities _db = Util.CreateObjectContext<SI20900Entities>(false);

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
            return this.Store(_db.SI20900_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SI_TaxCat> lstLang = dataHandler.BatchObjectData<SI_TaxCat>();
                foreach (SI_TaxCat deleted in lstLang.Deleted)
                {
                    var del = _db.SI_TaxCat.Where(p => p.CatID == deleted.CatID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_TaxCat.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (SI_TaxCat curLang in lstLang.Created)
                {
                    if (curLang.CatID.PassNull() == "") continue;

                    var lang = _db.SI_TaxCat.Where(p => p.CatID.ToLower() == curLang.CatID.ToLower()).FirstOrDefault();

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
                        lang = new SI_TaxCat();
                        Update_Language(lang, curLang, true);
                        _db.SI_TaxCat.AddObject(lang);
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

        private void Update_Language(SI_TaxCat t, SI_TaxCat s, bool isNew)
        {
            if (isNew)
            {
                t.CatID = s.CatID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

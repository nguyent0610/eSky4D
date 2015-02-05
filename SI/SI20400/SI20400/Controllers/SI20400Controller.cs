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
namespace SI20400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20400Controller : Controller
    {
        private string _screenNbr = "SI20400";
        private string _userName = Current.UserName;

        SI20400Entities _db = Util.CreateObjectContext<SI20400Entities>(false);

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
            return this.Store(_db.SI20400_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SI_MaterialType> lstLang = dataHandler.BatchObjectData<SI_MaterialType>();
                foreach (SI_MaterialType deleted in lstLang.Deleted)
                {
                    var del = _db.SI_MaterialType.Where(p => p.MaterialType == deleted.MaterialType).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_MaterialType.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (SI_MaterialType curLang in lstLang.Created)
                {
                    if (curLang.MaterialType.PassNull() == "") continue;

                    var lang = _db.SI_MaterialType.Where(p => p.MaterialType.ToLower() == curLang.MaterialType.ToLower()).FirstOrDefault();

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
                        lang = new SI_MaterialType();
                        Update_Language(lang, curLang, true);
                        _db.SI_MaterialType.AddObject(lang);
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

        private void Update_Language(SI_MaterialType t, SI_MaterialType s, bool isNew)
        {
            if (isNew)
            {
                t.MaterialType = s.MaterialType;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;
            t.Buyer = s.Buyer;


            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

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
namespace OM21500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21500Controller : Controller
    {
        private string _screenNbr = "OM21500";
        private string _userName = Current.UserName;
        OM21500Entities _db = Util.CreateObjectContext<OM21500Entities>(false);
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

        public ActionResult GetOM_DiscDescr()
        {
            return this.Store(_db.OM21500_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_DiscDescr"]);
                ChangeRecords<OM21500_pgLoadGrid_Result> lstLang = dataHandler.BatchObjectData<OM21500_pgLoadGrid_Result>();
                foreach (OM21500_pgLoadGrid_Result deleted in lstLang.Deleted)
                {
                    var del = _db.OM_DiscDescr.Where(p => p.DiscCode == deleted.DiscCode).FirstOrDefault();
                    if (del != null)
                    {
                        _db.OM_DiscDescr.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (OM21500_pgLoadGrid_Result curLang in lstLang.Created)
                {
                    if (curLang.DiscCode.PassNull() == "") continue;

                    var lang = _db.OM_DiscDescr.Where(p => p.DiscCode.ToLower() == curLang.DiscCode.ToLower()).FirstOrDefault();

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
                        lang = new OM_DiscDescr();
                        Update_Language(lang, curLang, true);
                        _db.OM_DiscDescr.AddObject(lang);
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
        private void Update_Language(OM_DiscDescr t, OM21500_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.DiscCode = s.DiscCode;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Active = s.Active;
            t.FromDate = s.FromDate;
            t.ToDate = s.ToDate;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    
    }
}

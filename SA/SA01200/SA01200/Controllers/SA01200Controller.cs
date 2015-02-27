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
namespace SA01200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA01200Controller : Controller
    {
        private string _screenNbr = "SA01200";
        private string _userName = Current.UserName;

        SA01200Entities _db = Util.CreateObjectContext<SA01200Entities>(true);

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
            return this.Store(_db.SA01200_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SYS_ModuleCat> lstLang = dataHandler.BatchObjectData<SYS_ModuleCat>();
                foreach (SYS_ModuleCat deleted in lstLang.Deleted)
                {
                    var del = _db.SYS_ModuleCat.Where(p => p.CatID == deleted.CatID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_ModuleCat.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (SYS_ModuleCat curLang in lstLang.Created)
                {
                    if (curLang.CatID.PassNull() == "") continue;

                    var lang = _db.SYS_ModuleCat.Where(p => p.CatID.ToLower() == curLang.CatID.ToLower()).FirstOrDefault();

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
                        lang = new SYS_ModuleCat();
                        Update_Language(lang, curLang, true);
                        _db.SYS_ModuleCat.AddObject(lang);
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

        private void Update_Language(SYS_ModuleCat t, SYS_ModuleCat s, bool isNew)
        {
            if (isNew)
            {
                t.CatID = s.CatID;    
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;
            t.Sort = s.Sort;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

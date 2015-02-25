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
namespace SA03000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA03000Controller : Controller
    {
        private string _screenNbr = "SA03000";
        private string _userName = Current.UserName;
        SA03000Entities _db = Util.CreateObjectContext<SA03000Entities>(true);
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
        public ActionResult GetSYS_FavouriteGroup()
        {
            return this.Store(_db.SA03000_pgSYS_FavouriteGroup().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_FavouriteGroup"]);
                ChangeRecords<SA03000_pgSYS_FavouriteGroup_Result> lstSYS_FavouriteGroup = dataHandler.BatchObjectData<SA03000_pgSYS_FavouriteGroup_Result>();
                foreach (SA03000_pgSYS_FavouriteGroup_Result deleted in lstSYS_FavouriteGroup.Deleted)
                {
                    var del = _db.SYS_FavouriteGroup.Where(p => p.Code == deleted.Code).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_FavouriteGroup.DeleteObject(del);
                    }
                }

                lstSYS_FavouriteGroup.Created.AddRange(lstSYS_FavouriteGroup.Updated);

                foreach (SA03000_pgSYS_FavouriteGroup_Result curLang in lstSYS_FavouriteGroup.Created)
                {
                    if (curLang.Code.PassNull() == "") continue;

                    var lang = _db.SYS_FavouriteGroup.Where(p => p.Code.ToLower() == curLang.Code.ToLower()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_FavouriteGroup();
                        Update(lang, curLang, true);
                        _db.SYS_FavouriteGroup.AddObject(lang);
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
        private void Update(SYS_FavouriteGroup t, SA03000_pgSYS_FavouriteGroup_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
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

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
namespace SA00500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00500Controller : Controller
    {
        private string _screenNbr = "SA00500";
        private string _userName = Current.UserName;

        SA00500Entities _db = Util.CreateObjectContext<SA00500Entities>(true);

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
            return this.Store(_db.SA00500_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SYS_Group> lstLang = dataHandler.BatchObjectData<SYS_Group>();
                foreach (SYS_Group deleted in lstLang.Deleted)
                {
                    var del = _db.SYS_Group.Where(p => p.GroupID == deleted.GroupID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_Group.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (SYS_Group curLang in lstLang.Created)
                {
                    if (curLang.GroupID.PassNull() == "") continue;

                    var lang = _db.SYS_Group.Where(p => p.GroupID.ToLower() == curLang.GroupID.ToLower()).FirstOrDefault();

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
                        lang = new SYS_Group();
                        Update_Language(lang, curLang, true);
                        _db.SYS_Group.AddObject(lang);
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

        private void Update_Language(SYS_Group t, SYS_Group s, bool isNew)
        {
            if (isNew)
            {
                t.GroupID = s.GroupID;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;
           
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

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
namespace SA03100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA03100Controller : Controller
    {
        private string _screenNbr = "SA03100";
        private string _userName = Current.UserName;
        SA03100Entities _db = Util.CreateObjectContext<SA03100Entities>(true);
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
        public ActionResult GetSYS_CompanyGroup()
        {
            return this.Store(_db.SA03100_pgSYS_CompanyGroup().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_CompanyGroup"]);
                ChangeRecords<SA03100_pgSYS_CompanyGroup_Result> lstSYS_CompanyGroup = dataHandler.BatchObjectData<SA03100_pgSYS_CompanyGroup_Result>();
                foreach (SA03100_pgSYS_CompanyGroup_Result deleted in lstSYS_CompanyGroup.Deleted)
                {
                    var del = _db.SYS_CompanyGroup.Where(p => p.GroupID == deleted.GroupID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_CompanyGroup.DeleteObject(del);
                    }
                }

                lstSYS_CompanyGroup.Created.AddRange(lstSYS_CompanyGroup.Updated);

                foreach (SA03100_pgSYS_CompanyGroup_Result curLang in lstSYS_CompanyGroup.Created)
                {
                    if (curLang.GroupID.PassNull() == "") continue;

                    var lang = _db.SYS_CompanyGroup.Where(p => p.GroupID.ToLower() == curLang.GroupID.ToLower()).FirstOrDefault();

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
                        lang = new SYS_CompanyGroup();
                        Update(lang, curLang, true);
                        _db.SYS_CompanyGroup.AddObject(lang);
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
        private void Update(SYS_CompanyGroup t, SA03100_pgSYS_CompanyGroup_Result s, bool isNew)
        {
            if (isNew)
            {
                t.GroupID = s.GroupID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.ListCpny = s.ListCpny;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
      


        
    }
}

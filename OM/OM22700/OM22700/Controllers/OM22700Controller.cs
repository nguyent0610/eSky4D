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
namespace OM22700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22700Controller : Controller
    {
        private string _screenNbr = "OM22700";
        private string _userName = Current.UserName;
        OM22700Entities _db = Util.CreateObjectContext<OM22700Entities>(false);
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

        public ActionResult GetOM_WeekOfVisit()
        {
            return this.Store(_db.OM22700_pgOM_WeekOfVisit().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_WeekOfVisit"]);
                ChangeRecords<OM22700_pgOM_WeekOfVisit_Result> lstLang = dataHandler.BatchObjectData<OM22700_pgOM_WeekOfVisit_Result>();

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (OM22700_pgOM_WeekOfVisit_Result deleted in lstLang.Deleted)
                {
                    if (lstLang.Created.Where(p => p.SlsFreqID == deleted.SlsFreqID
                                                && p.WeekofVisit == deleted.WeekofVisit).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.SlsFreqID == deleted.SlsFreqID
                                                && p.WeekofVisit == deleted.WeekofVisit).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_WeekOfVisit.FirstOrDefault(p => p.SlsFreqID == deleted.SlsFreqID
                                                                        && p.WeekofVisit == deleted.WeekofVisit);
                        if (objDel != null)
                        {
                            _db.OM_WeekOfVisit.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM22700_pgOM_WeekOfVisit_Result curLang in lstLang.Created)
                {
                    if (curLang.SlsFreqID.PassNull() == "" || curLang.WeekofVisit.PassNull() == "") continue;

                    var lang = _db.OM_WeekOfVisit.Where(p => p.SlsFreqID.ToLower() == curLang.SlsFreqID.ToLower() && p.WeekofVisit.ToLower() == curLang.WeekofVisit.ToLower()).FirstOrDefault();

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
                        lang = new OM_WeekOfVisit();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.OM_WeekOfVisit.AddObject(lang);
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
        private void Update_Language(OM_WeekOfVisit t, OM22700_pgOM_WeekOfVisit_Result s, bool isNew)
        {
            if (isNew)
            {
                t.SlsFreqID = s.SlsFreqID;
                t.WeekofVisit = s.WeekofVisit;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Mon = s.Mon;
            t.Tue = s.Tue;
            t.Wed = s.Wed;
            t.Thu = s.Thu;
            t.Fri = s.Fri;
            t.Sat = s.Sat;
            t.Sun = s.Sun;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
       
    }
}

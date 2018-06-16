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
using System.Reflection;
using System.Collections;
using System.Runtime.Caching;
namespace IN20400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20400Controller : Controller
    {
        private string _screenNbr = "IN20400";
        IN20400Entities _db = Util.CreateObjectContext<IN20400Entities>(false);
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
        public ActionResult GetIN_SiteLocation(string siteID)
        {
            return this.Store(_db.IN20400_pgLoadSiteLocation(Current.CpnyID, Current.UserName, Current.LangID, siteID).ToList());
        }

        public ActionResult GetSiteID(string siteID)
        {
            return this.Store(_db.IN20400_pdCheckSiteID(Current.CpnyID, Current.UserName, Current.LangID, siteID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string siteID = data["cboSiteID"].PassNull();
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstIN_SiteLocation"]);
                ChangeRecords<IN20400_pgLoadSiteLocation_Result> lstLang = dataHandler.BatchObjectData<IN20400_pgLoadSiteLocation_Result>();
                lstLang.Created.AddRange(lstLang.Updated);
                foreach (IN20400_pgLoadSiteLocation_Result del in lstLang.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstLang.Created.Where(p => p.SiteID == siteID && p.WhseLoc == del.WhseLoc.ToUpper().Trim()).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.SiteID == siteID && p.WhseLoc == del.WhseLoc.ToUpper().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.IN_SiteLocation.ToList().Where(p => p.SiteID == siteID && p.WhseLoc == del.WhseLoc.ToUpper().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.IN_SiteLocation.DeleteObject(objDel);
                        }
                    }
                }

                foreach (IN20400_pgLoadSiteLocation_Result curLang in lstLang.Created)
                {
                    if (curLang.WhseLoc.PassNull() == "") continue;
                    curLang.SiteID = siteID; 
                    var lang = _db.IN_SiteLocation.Where(p => p.SiteID == siteID && p.WhseLoc == curLang.WhseLoc.ToUpper().Trim()).FirstOrDefault();

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
                        lang = new IN_SiteLocation();
                        Update_Language(lang, curLang, true);
                        _db.IN_SiteLocation.AddObject(lang);
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
        private void Update_Language(IN_SiteLocation t, IN20400_pgLoadSiteLocation_Result s, bool isNew)
        {
            if (isNew)
            {
                t.SiteID = s.SiteID;
                t.WhseLoc = s.WhseLoc.ToUpper().Trim();
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
            }
            t.Descr = s.Descr;
            t.SalesAllowed = true;
            t.IssueAllowed = true;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName ?? "";
        }

        [HttpPost]

        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string SiteID = data["cboSiteID"].ToUpper().PassNull();
                var lstSite = _db.IN_SiteLocation.Where(p => p.SiteID == SiteID).ToList();

                foreach (var item in lstSite)
                {
                    _db.IN_SiteLocation.DeleteObject(item);
                }
                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Delete, SiteID);
            }

            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }

        }
    }
}

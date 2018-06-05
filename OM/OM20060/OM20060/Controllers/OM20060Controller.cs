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
namespace OM20060.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20060Controller : Controller
    {
        private string _screenNbr = "OM20060";
        private string _userName = Current.UserName;
        OM20060Entities _db = Util.CreateObjectContext<OM20060Entities>(false);
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
        public ActionResult GetOM_CompetitorCriteria()
        {           
            return this.Store(_db.OM20060_pgLoadCompetitorCriteria(Current.CpnyID, Current.UserName, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            try
            {


                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_CompetitorCriteria"]);
                ChangeRecords<OM20060_pgLoadCompetitorCriteria_Result> lstLang = dataHandler.BatchObjectData<OM20060_pgLoadCompetitorCriteria_Result>();
                lstLang.Created.AddRange(lstLang.Updated);
                foreach (OM20060_pgLoadCompetitorCriteria_Result del in lstLang.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstLang.Created.Where(p => p.CriteriaID == del.CriteriaID.ToUpper().Trim()).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.CriteriaID == del.CriteriaID.ToUpper().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_CompetitorCriteria.ToList().Where(p => p.CriteriaID == del.CriteriaID.ToUpper().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_CompetitorCriteria.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM20060_pgLoadCompetitorCriteria_Result curLang in lstLang.Created)
                {
                    if (curLang.CriteriaID.PassNull() == "") continue;

                    var lang = _db.OM_CompetitorCriteria.Where(p => p.CriteriaID == curLang.CriteriaID.ToUpper().Trim()).FirstOrDefault();

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
                        lang = new OM_CompetitorCriteria();
                        Update_Language(lang, curLang, true);
                        _db.OM_CompetitorCriteria.AddObject(lang);
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
        private void Update_Language(OM_CompetitorCriteria t, OM20060_pgLoadCompetitorCriteria_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CriteriaID = s.CriteriaID.ToUpper().Trim();
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CriteriaName = s.CriteriaName;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }            
    }
}

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
namespace OM20080.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20080Controller : Controller
    {
        private string _screenNbr = "OM20080";
        private string _userName = Current.UserName;
        OM20080Entities _db = Util.CreateObjectContext<OM20080Entities>(false);
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
        public ActionResult GetOM_CompetitorInvt()
        {           
            return this.Store(_db.OM20080_pgLoadCompetitorInvt(Current.CpnyID, Current.UserName, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            try
            {


                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_CompetitorInvt"]);
                ChangeRecords<OM20080_pgLoadCompetitorInvt_Result> lstLang = dataHandler.BatchObjectData<OM20080_pgLoadCompetitorInvt_Result>();
                lstLang.Created.AddRange(lstLang.Updated);
                foreach (OM20080_pgLoadCompetitorInvt_Result del in lstLang.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstLang.Created.Where(p => p.CompInvtID == del.CompInvtID.ToUpper().Trim()).Count() > 0)
                    {
                        lstLang.Created.Where(p => p.CompInvtID == del.CompInvtID.ToUpper().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_CompetitorInvt.ToList().Where(p => p.CompInvtID == del.CompInvtID.ToUpper().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_CompetitorInvt.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM20080_pgLoadCompetitorInvt_Result curLang in lstLang.Created)
                {
                    if (curLang.CompInvtID.PassNull() == "") continue;

                    var lang = _db.OM_CompetitorInvt.Where(p => p.CompInvtID == curLang.CompInvtID.ToUpper().Trim()).FirstOrDefault();

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
                        lang = new OM_CompetitorInvt();
                        Update_Language(lang, curLang, true);
                        _db.OM_CompetitorInvt.AddObject(lang);
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
        private void Update_Language(OM_CompetitorInvt t, OM20080_pgLoadCompetitorInvt_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CompInvtID = s.CompInvtID.ToUpper().Trim();
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CompInvtName = s.CompInvtName;
            t.CompID = s.CompID;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }            
    }
}

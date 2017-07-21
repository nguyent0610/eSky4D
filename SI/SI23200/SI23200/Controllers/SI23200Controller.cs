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

namespace SI23200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI23200Controller : Controller
    {
        private string _screenNbr = "SI23200";
        private string _userName = Current.UserName;
        SI23200Entities _db = Util.CreateObjectContext<SI23200Entities>(false);

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
            return this.Store(_db.SI23200_pgLoadArea(Current.CpnyID,Current.UserName,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                    StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_SubTerritory"]);
                    ChangeRecords<SI23200_pgLoadArea_Result> lstSI_SubTerritory = dataHandler.BatchObjectData<SI23200_pgLoadArea_Result>();
                    lstSI_SubTerritory.Created.AddRange(lstSI_SubTerritory.Updated);
                    foreach (SI23200_pgLoadArea_Result del in lstSI_SubTerritory.Deleted)
                    {
                        // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                        if (lstSI_SubTerritory.Created.Where(p => p.Code == del.Code).Count() > 0)
                        {
                            lstSI_SubTerritory.Created.Where(p => p.Code == del.Code).FirstOrDefault().tstamp = del.tstamp;
                        }
                        else
                        {
                            var objDel = _db.SI_SubTerritory.ToList().Where(p => p.Code == del.Code).FirstOrDefault();
                            if (objDel != null)
                            {
                                _db.SI_SubTerritory.DeleteObject(objDel);
                            }
                        }
                    }

                    foreach (SI23200_pgLoadArea_Result curLang in lstSI_SubTerritory.Created)
                    {
                        if (curLang.Territory.PassNull() == "") continue;

                        var lang = _db.SI_SubTerritory.Where(p => p.Code.ToLower() == curLang.Code.ToLower()).FirstOrDefault();

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
                            lang = new SI_SubTerritory();
                            Update(lang, curLang, true);
                            _db.SI_SubTerritory.AddObject(lang);
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


        private void Update(SI_SubTerritory t, SI23200_pgLoadArea_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Territory = s.Territory;            
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

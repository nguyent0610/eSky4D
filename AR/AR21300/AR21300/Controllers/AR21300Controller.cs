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

namespace AR21300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21300Controller : Controller
    {
        private string _screenNbr = "AR21300";
        private string _userName = Current.UserName;
        AR21300Entities _db = Util.CreateObjectContext<AR21300Entities>(false);

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

        public ActionResult GetData()
        {
            return this.Store(_db.AR21300_pgLoadArea().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAR_Area"]);
                ChangeRecords<AR21300_pgLoadArea_Result> lstAR_Area = dataHandler.BatchObjectData<AR21300_pgLoadArea_Result>();
                    lstAR_Area.Created.AddRange(lstAR_Area.Updated);
                    foreach (AR21300_pgLoadArea_Result del in lstAR_Area.Deleted)
                    {
                        // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                        if (lstAR_Area.Created.Where(p => p.Area == del.Area).Count() > 0)
                        {
                            lstAR_Area.Created.Where(p => p.Area == del.Area).FirstOrDefault().tstamp = del.tstamp;
                        }
                        else
                        {
                            var objDel = _db.AR_Area.ToList().Where(p => p.Area == del.Area).FirstOrDefault();
                            if (objDel != null)
                            {
                                _db.AR_Area.DeleteObject(objDel);
                            }
                        }
                    }

                    foreach (AR21300_pgLoadArea_Result curLang in lstAR_Area.Created)
                    {
                        if (curLang.Area.PassNull() == "") continue;

                        var lang = _db.AR_Area.Where(p => p.Area.ToLower() == curLang.Area.ToLower()).FirstOrDefault();

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
                            lang = new AR_Area();
                            Update(lang, curLang, true);
                            _db.AR_Area.AddObject(lang);
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


        private void Update(AR_Area t, AR21300_pgLoadArea_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Area = s.Area;
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

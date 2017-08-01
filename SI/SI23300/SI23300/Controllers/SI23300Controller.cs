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

namespace SI23300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI23300Controller : Controller
    {
        private string _screenNbr = "SI23300";
        private string _userName = Current.UserName;
        SI23300Entities _db = Util.CreateObjectContext<SI23300Entities>(false);

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
            return this.Store(_db.SI23300_pgLoadGrid(Current.CpnyID,Current.UserName,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Ward"]);
                ChangeRecords<SI23300_pgLoadGrid_Result> lstSI_Ward = dataHandler.BatchObjectData<SI23300_pgLoadGrid_Result>();
                lstSI_Ward.Created.AddRange(lstSI_Ward.Updated);
                foreach (SI23300_pgLoadGrid_Result del in lstSI_Ward.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSI_Ward.Created.Where(p => p.Country == del.Country && p.District == del.District && p.State == del.State && p.Ward == del.Ward).Count() > 0)
                    {
                        lstSI_Ward.Created.Where(p => p.Country == del.Country && p.District == del.District && p.State == del.State && p.Ward == del.Ward).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Ward.ToList().Where(p => p.Country == del.Country && p.District == del.District && p.State == del.State && p.Ward == del.Ward).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Ward.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SI23300_pgLoadGrid_Result curLang in lstSI_Ward.Created)
                {
                    if (curLang.Country.PassNull() == "") continue;

                    var lang = _db.SI_Ward.Where(p => p.Country == curLang.Country && p.District == curLang.District && p.State == curLang.State && p.Ward == curLang.Ward).FirstOrDefault();

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
                        lang = new SI_Ward();
                        Update(lang, curLang, true);
                        _db.SI_Ward.AddObject(lang);
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


        private void Update(SI_Ward t, SI23300_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.Crtd_Datetime = DateTime.Now;
                t.State = s.State;
                t.District = s.District;
                t.Ward = s.Ward;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Name = s.Name;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

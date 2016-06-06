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
namespace SI20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20500Controller : Controller
    {
        private string _screenNbr = "SI20500";
        private string _userName = Current.UserName;
        SI20500Entities _db = Util.CreateObjectContext<SI20500Entities>(false);
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
        public ActionResult GetSI_City()
        {
            return this.Store(_db.SI20500_pgLoadGrid().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SI20500_pgLoadGrid_Result> lstData = dataHandler.BatchObjectData<SI20500_pgLoadGrid_Result>();
                lstData.Created.AddRange(lstData.Updated);// Dua danh sach update chung vao danh sach tao moi
                foreach (SI20500_pgLoadGrid_Result del in lstData.Deleted)
                {
                    if (lstData.Created.Where(p => p.Country.ToLower().Trim() == del.Country.ToLower().Trim() && p.State.ToLower().Trim() == del.State.ToLower().Trim() && p.City.ToLower().Trim() == del.City.ToLower().Trim()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.Country.ToLower().Trim() == del.Country.ToLower().Trim() && p.State.ToLower().Trim() == del.State.ToLower().Trim() && p.City.ToLower().Trim() == del.City.ToLower().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_City.Where(p => p.Country == del.Country && p.State == del.State && p.City == del.City).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_City.DeleteObject(objDel);
                        }
                    }
                }
                foreach (SI20500_pgLoadGrid_Result curItem in lstData.Created)
                {
                    if (curItem.Country.PassNull() == "") continue;

                    var lang = _db.SI_City.Where(p => p.Country.ToLower() == curItem.Country.ToLower() && p.State.ToLower() == curItem.State.ToLower() && p.City.ToLower() == curItem.City.ToLower()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_SI_City(lang, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SI_City();
                        Update_SI_City(lang, curItem, true);
                        _db.SI_City.AddObject(lang);
                    }
                }
                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Save);
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
            }
        }
        private void Update_SI_City(SI_City t, SI20500_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.State = s.State;
                t.City = s.City;
                t.Crtd_Datetime = DateTime.Now;
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

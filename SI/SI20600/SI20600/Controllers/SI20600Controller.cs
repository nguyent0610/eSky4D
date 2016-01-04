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
namespace SI20600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20600Controller : Controller
    {
        private string _screenNbr = "SI20600";
        private string _userName = Current.UserName;
        SI20600Entities _db = Util.CreateObjectContext<SI20600Entities>(false);
        public ActionResult Index()
        {

            Util.InitRight(_screenNbr);// lấy quyền cho user khi truy cập màn hình này
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "lang")]//cache lại giao diện chi sinh lần đầu
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetData()
        {           
            return this.Store(_db.SI20600_pgLoadCountry().ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SI20600_pgLoadCountry_Result> lstData = dataHandler.BatchObjectData<SI20600_pgLoadCountry_Result>();
                lstData.Created.AddRange(lstData.Updated);// Dua danh sach update chung vao danh sach tao moi
                foreach (SI20600_pgLoadCountry_Result del in lstData.Deleted)
                {
                    if (lstData.Created.Where(p => p.CountryID.ToLower() == del.CountryID.ToLower()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.CountryID.ToLower() == del.CountryID.ToLower()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Country.ToList().Where(p => p.CountryID.ToLower() == del.CountryID.ToLower()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.SI_Country.DeleteObject(objDel);
                        }
                    }
                }

             

                foreach (SI20600_pgLoadCountry_Result curItem in lstData.Created)
                {
                    if (curItem.CountryID.PassNull() == "") continue;

                    var objCountry = _db.SI_Country.Where(p => p.CountryID.ToLower() == curItem.CountryID.ToLower()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_SI_Country(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new SI_Country();
                        Update_SI_Country(objCountry, curItem, true);
                        _db.SI_Country.AddObject(objCountry);
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
        private void Update_SI_Country(SI_Country t, SI20600_pgLoadCountry_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CountryID = s.CountryID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

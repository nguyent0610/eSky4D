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
namespace OM25000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM25000Controller : Controller
    {
        private string _screenNbr = "OM25000";
        private string _userName = Current.UserName;
        OM25000Entities _db = Util.CreateObjectContext<OM25000Entities>(false);

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
            return this.Store(_db.OM25000_pgLoadKPI(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<OM25000_pgLoadKPI_Result> lstData = dataHandler.BatchObjectData<OM25000_pgLoadKPI_Result>();

                lstData.Created.AddRange(lstData.Updated);
                foreach (OM25000_pgLoadKPI_Result del in lstData.Deleted)
                {

                    if (lstData.Created.Where(p => p.KPI.ToLower().Trim() == del.KPI.ToLower().Trim()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.KPI.ToLower().Trim() == del.KPI.ToLower().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPI.ToList().Where(p => p.KPI.ToLower().Trim() == del.KPI.ToLower().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPI.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM25000_pgLoadKPI_Result curItem in lstData.Created)
                {
                    if (curItem.KPI.PassNull() == "")
                    {
                        continue;
                    }
                    var ScreenNumber = _db.OM_KPI.Where(p => p.KPI.ToLower() == curItem.KPI.ToLower()).FirstOrDefault();

                    if (ScreenNumber != null)
                    {
                        if (ScreenNumber.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_OM_KPI(ScreenNumber, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        ScreenNumber = new OM_KPI();
                        Update_OM_KPI(ScreenNumber, curItem, true);
                        _db.OM_KPI.AddObject(ScreenNumber);
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

        private void Update_OM_KPI(OM_KPI t, OM25000_pgLoadKPI_Result s, bool isNew)
        {
            if (isNew)
            {
                t.KPI = s.KPI; 
                t.Crtd_User = Current.UserName;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Name = s.Name;
            t.ApplyFor = s.ApplyFor;
            t.ApplyTo = s.ApplyTo;
            t.Type = s.Type;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

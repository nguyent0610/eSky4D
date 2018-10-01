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
namespace IN20700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20700Controller : Controller
    {
        private string _screenNbr = "IN20700";
        private string _userName = Current.UserName;
        IN20700Entities _db = Util.CreateObjectContext<IN20700Entities>(false);
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
            return this.Store(_db.IN20700_pgLoadReasonCD(Current.UserName, Current.CpnyID, Current.LangID));
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<IN20700_pgLoadReasonCD_Result> lstData = dataHandler.BatchObjectData<IN20700_pgLoadReasonCD_Result>();
                lstData.Created.AddRange(lstData.Updated);// Dua danh sach update chung vao danh sach tao moi
                foreach (IN20700_pgLoadReasonCD_Result del in lstData.Deleted)
                {
                    if (lstData.Created.Where(p => p.ReasonCD.ToLower() == del.ReasonCD.ToLower()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.ReasonCD.ToLower() == del.ReasonCD.ToLower()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                       
                        var objDel = _db.IN_ReasonCode.ToList().Where(p => p.ReasonCD.ToLower() == del.ReasonCD.ToLower()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.IN_ReasonCode.DeleteObject(objDel);
                           
                        }
                       
                    }
                }



                foreach (IN20700_pgLoadReasonCD_Result curItem in lstData.Created)
                {
                    if (curItem.ReasonCD.PassNull() == "") continue;

                    var objReasonCD = _db.IN_ReasonCode.Where(p => p.ReasonCD.ToLower() == curItem.ReasonCD.ToLower()).FirstOrDefault();

                    if (objReasonCD != null)
                    {
                        if (objReasonCD.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_IN_ReasonCode(objReasonCD, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objReasonCD = new IN_ReasonCode();
                        Update_IN_ReasonCode(objReasonCD, curItem, true);
                        _db.IN_ReasonCode.AddObject(objReasonCD);
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
        public ActionResult Delete(string reasonCD)
        {
            
            string[] records = reasonCD.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in records)
            {
                if (_db.IN20700_ppCheckForDeleteReasonCD(s).FirstOrDefault() == "1")
                {
                    return Json(new { success = false }, "text/html");

                }

            }
            return Json(new { success = true }, "text/html");

        }
       
        private void Update_IN_ReasonCode(IN_ReasonCode t, IN20700_pgLoadReasonCD_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ReasonCD = s.ReasonCD;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.SiteID = s.SiteID;
            t.SlsperID = s.SlsperID;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

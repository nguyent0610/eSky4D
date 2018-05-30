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
using HQ.eSkySys;
using Ionic.Zip;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using HQFramework.Common;
namespace OM29900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM29900Controller : Controller
    {
        private string _screenNbr = "OM29900";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        OM29900Entities _db = Util.CreateObjectContext<OM29900Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;
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
        public ActionResult GetTypeOfVehicle(string BranchID)
        {
            var data = _db.OM29900_pcTypeOfVehicle(Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(data);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstTypeOfVehicle"]);
                ChangeRecords<OM29900_pcTypeOfVehicle_Result> lstOM_TypeOfVehicle = dataHandler.BatchObjectData<OM29900_pcTypeOfVehicle_Result>();


                lstOM_TypeOfVehicle.Created.AddRange(lstOM_TypeOfVehicle.Updated);

                foreach (OM29900_pcTypeOfVehicle_Result del in lstOM_TypeOfVehicle.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstOM_TypeOfVehicle.Created.Where(p => p.Code == del.Code).Count() > 0)
                    {
                        lstOM_TypeOfVehicle.Created.Where(p => p.Code == del.Code).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_TypeOfVehicle.ToList().Where(p => p.Code == del.Code).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_TypeOfVehicle.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM29900_pcTypeOfVehicle_Result curLang in lstOM_TypeOfVehicle.Created)
                {
                    //if (curLang.CustId.PassNull() == "") continue;

                    var lang = _db.OM_TypeOfVehicle.Where(p => p.Code == curLang.Code).FirstOrDefault();

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
                        if(curLang.Code.PassNull()!="")
                        {
                            lang = new OM_TypeOfVehicle();
                            lang.ResetET();
                            Update_Language(lang, curLang, true);
                            _db.OM_TypeOfVehicle.AddObject(lang);
                        }  
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
        private void Update_Language(OM_TypeOfVehicle t, OM29900_pcTypeOfVehicle_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
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


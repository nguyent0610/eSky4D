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
namespace AR24000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR24000Controller : Controller
    {
        private string _screenNbr = "AR24000";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        AR24000Entities _db = Util.CreateObjectContext<AR24000Entities>(false);
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
        public ActionResult GetMapCustomer(string BranchID)
        {
            var data = _db.AR24000_pgMapCustomer( Current.CpnyID,Current.UserName, Current.LangID, BranchID).ToList();
            return this.Store(data);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstMapCustomer"]);
                ChangeRecords<AR24000_pgMapCustomer_Result> lstAR_MapCustomer = dataHandler.BatchObjectData<AR24000_pgMapCustomer_Result>();

                string BranchID = data["cboCpnyID"].PassNull();

                lstAR_MapCustomer.Created.AddRange(lstAR_MapCustomer.Updated);

                foreach (AR24000_pgMapCustomer_Result del in lstAR_MapCustomer.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstAR_MapCustomer.Created.Where(p => p.CustID == del.CustID && p.CustID_Vendor==del.CustID_Vendor && p.BranchID==del.BranchID).Count() > 0)
                    {
                        lstAR_MapCustomer.Created.Where(p => p.CustID == del.CustID && p.CustID_Vendor==del.CustID_Vendor && p.BranchID==del.BranchID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.AR_CustomerMap.ToList().Where(p => p.CustID == del.CustID && p.CustID_Vendor==del.CustID_Vendor && p.BranchID==del.BranchID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.AR_CustomerMap.DeleteObject(objDel);
                        }
                    }
                }

                foreach (AR24000_pgMapCustomer_Result curLang in lstAR_MapCustomer.Created)
                {
                    //if (curLang.CustId.PassNull() == "") continue;

                    var lang = _db.AR_CustomerMap.Where(p => p.CustID == curLang.CustID && p.BranchID==curLang.BranchID && p.CustID_Vendor==curLang.CustID_Vendor).FirstOrDefault();

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
                        if(curLang.CustID.PassNull()!="" && curLang.CustID_Vendor.PassNull()!="")
                        {
                            lang = new AR_CustomerMap();
                            lang.ResetET();
                            if (BranchID == "" || BranchID == null)
                            {
                                throw new MessageException(MessageType.Message, "1000", Util.GetLang("Branch"));
                            }
                            lang.BranchID = BranchID;
                            Update_Language(lang, curLang, true);
                            _db.AR_CustomerMap.AddObject(lang);
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
        private void Update_Language(AR_CustomerMap t, AR24000_pgMapCustomer_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CustID = s.CustID;
                t.CustID_Vendor = s.CustID_Vendor;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Name_Vendor = s.Name_Vendor;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;



        }
        
        
        
    }
}


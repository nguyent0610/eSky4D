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
namespace IN21500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN21500Controller : Controller
    {
        private string _screenNbr = "IN21500";
        private string _userName = Current.UserName;
        IN21500Entities _db = Util.CreateObjectContext<IN21500Entities>(false);
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
            var lstInvtID=_db.IN21500_pcInvtID(Current.UserName,Current.CpnyID).ToList();
            var lst = (from p in _db.IN21500_pgData(Current.UserName, Current.CpnyID).ToList() select new IN21500_pgData_Result() { InvtID = p.InvtID, Descr = lstInvtID.Where(c => c.InvtID == p.InvtID).FirstOrDefault() == null ? p.Descr : lstInvtID.Where(c => c.InvtID == p.InvtID).FirstOrDefault().Descr,Date=p.Date,tstamp=p.tstamp }).ToList();            
            return this.Store(lst);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<IN21500_pgData_Result> lstLang = dataHandler.BatchObjectData<IN21500_pgData_Result>();
                foreach (IN21500_pgData_Result deleted in lstLang.Deleted)
                {
                    var del = _db.IN_InventoryDateMaster.Where(p => p.InvtID == deleted.InvtID 
                        && p.Date.Month == deleted.Date.Month 
                        && p.Date.Year == deleted.Date.Year
                        && p.Date.Day == deleted.Date.Day).FirstOrDefault();
                    if (del != null)
                    {
                        _db.IN_InventoryDateMaster.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (IN21500_pgData_Result curLang in lstLang.Created)
                {
                    if (curLang.InvtID.PassNull() == "") continue;

                    var lang = _db.IN_InventoryDateMaster.Where(p => p.InvtID == curLang.InvtID 
                        && p.Date.Month == curLang.Date.Month 
                        && p.Date.Year == curLang.Date.Year
                        && p.Date.Day == curLang.Date.Day).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Invt(lang, curLang);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new IN_InventoryDateMaster();
                        lang.ResetET();
                        Update_Invt(lang, curLang);
                        lang.InvtID = curLang.InvtID;
                        lang.Date = curLang.Date;
                        lang.Crtd_DateTime = DateTime.Now;
                        lang.Crtd_Prog = _screenNbr;
                        lang.Crtd_User = _userName;
                        _db.IN_InventoryDateMaster.AddObject(lang);
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
        private void Update_Invt(IN_InventoryDateMaster t, IN21500_pgData_Result s)
        {
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

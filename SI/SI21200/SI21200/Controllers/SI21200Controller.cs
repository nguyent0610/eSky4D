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
namespace SI21200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21200Controller : Controller
    {
        private string _screenNbr = "SI21200";
        private string _userName = Current.UserName;
        SI21200Entities _db = Util.CreateObjectContext<SI21200Entities>(false);
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
        public ActionResult GetSI_ShipVia()
        {
            return this.Store(_db.SI21200_pgSI_ShipVia().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_ShipVia"]);
                ChangeRecords<SI21200_pgSI_ShipVia_Result> lstSI_ShipVia = dataHandler.BatchObjectData<SI21200_pgSI_ShipVia_Result>();
                foreach (SI21200_pgSI_ShipVia_Result deleted in lstSI_ShipVia.Deleted)
                {
                    var del = _db.SI_ShipVia.Where(p => p.ShipViaID == deleted.ShipViaID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_ShipVia.DeleteObject(del);
                    }
                }

                lstSI_ShipVia.Created.AddRange(lstSI_ShipVia.Updated);

                foreach (SI21200_pgSI_ShipVia_Result curLang in lstSI_ShipVia.Created)
                {
                    if (curLang.ShipViaID.PassNull() == "") continue;

                    var lang = _db.SI_ShipVia.Where(p => p.ShipViaID.ToLower() == curLang.ShipViaID.ToLower()).FirstOrDefault();

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
                        lang = new SI_ShipVia();
                        Update_Language(lang, curLang, true);
                        _db.SI_ShipVia.AddObject(lang);
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
        private void Update_Language(SI_ShipVia t, SI21200_pgSI_ShipVia_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ShipViaID = s.ShipViaID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.CarrierID = s.CarrierID;
            t.TransitTime = s.TransitTime;
            t.S4Future09 = s.S4Future09;
            t.S4Future10 = s.S4Future10;
            t.S4Future11 = s.S4Future11;
            t.S4Future12 = s.S4Future12;
            t.MoveOnDeliveryDays = s.MoveOnDeliveryDays;
            t.WeekendDelivery = s.WeekendDelivery;
            t.DfltFrtMthd = s.DfltFrtMthd;
            t.DfltFrtAmt = s.DfltFrtAmt;
            t.FrtAcct = s.FrtAcct;
            t.FrtSub = s.FrtSub;
            t.TaxCat = s.TaxCat;
            t.SCAC = s.SCAC;
           
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        
    }
}

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
namespace SI21300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21300Controller : Controller
    {
        private string _screenNbr = "SI21300";
        private string _userName = Current.UserName;

        SI21300Entities _db = Util.CreateObjectContext<SI21300Entities>(false);

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

        public ActionResult GetSI_Carrier()
        {
            var Carriers = _db.SI21300_pgLoadCarrier().ToList();
            return this.Store(Carriers);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Carrier"]);
                ChangeRecords<SI21300_pgLoadCarrier_Result> lstSI_Carrier = dataHandler.BatchObjectData<SI21300_pgLoadCarrier_Result>();
                foreach (SI21300_pgLoadCarrier_Result deleted in lstSI_Carrier.Deleted)
                {
                    var del = _db.SI_Carrier.Where(p => p.CarrierID == deleted.CarrierID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_Carrier.DeleteObject(del);
                    }
                }

                lstSI_Carrier.Created.AddRange(lstSI_Carrier.Updated);

                foreach (SI21300_pgLoadCarrier_Result curCarrier in lstSI_Carrier.Created)
                {
                    if (curCarrier.CarrierID.PassNull() == "") continue;

                    var Carrier = _db.SI_Carrier.Where(p => p.CarrierID.ToLower() == curCarrier.CarrierID.ToLower()).FirstOrDefault();

                    if (Carrier != null)
                    {
                        if (Carrier.tstamp.ToHex() == curCarrier.tstamp.ToHex())
                        {
                            Update_SI_Carrier(Carrier, curCarrier, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Carrier = new SI_Carrier();
                        Update_SI_Carrier(Carrier, curCarrier, true);
                        _db.SI_Carrier.AddObject(Carrier);
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

        private void Update_SI_Carrier(SI_Carrier t, SI21300_pgLoadCarrier_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CarrierID = s.CarrierID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.CarrierType = s.CarrierType;
            t.TerritoryID = s.TerritoryID;
            t.CheckZones = s.CheckZones;
            t.ShipAccount = s.ShipAccount;
            t.UOM = s.UOM;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

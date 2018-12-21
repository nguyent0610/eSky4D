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
namespace SI21900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21900Controller : Controller
    {
        private string _screenNbr = "SI21900";
        private string _userName = Current.UserName;
        SI21900Entities _db = Util.CreateObjectContext<SI21900Entities>(false);
        public ActionResult Index()
        {
            var Distance = false;
            Util.InitRight(_screenNbr);
            var config = _db.SI21900_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (config != null)
            {
                Distance = config.Distance.HasValue && config.Distance.Value;
            }
            ViewBag.Distance = Distance;


            return View();
        }
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetTerritory()
        {
            return this.Store(_db.SI21900_pgLoadTerritory(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstTerritory"]);
                ChangeRecords<SI21900_pgLoadTerritory_Result> lstTerritory = dataHandler.BatchObjectData<SI21900_pgLoadTerritory_Result>();
                foreach (SI21900_pgLoadTerritory_Result deleted in lstTerritory.Deleted)
                {

                    if (lstTerritory.Created.Where(p => p.Territory == deleted.Territory).Count() > 0)
                    {
                        lstTerritory.Created.Where(p => p.Territory == deleted.Territory).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.SI_Territory.Where(p => p.Territory == deleted.Territory).FirstOrDefault();
                        if (del != null)
                        {
                            _db.SI_Territory.DeleteObject(del);
                        }
                    }

                }

                lstTerritory.Created.AddRange(lstTerritory.Updated);

                foreach (SI21900_pgLoadTerritory_Result curTerritory in lstTerritory.Created)
                {
                    if (curTerritory.Territory.PassNull() == "") continue;

                    var territory = _db.SI_Territory.Where(p => p.Territory.ToLower() == curTerritory.Territory.ToLower()).FirstOrDefault();

                    if (territory != null)
                    {
                        if (territory.tstamp.ToHex() == curTerritory.tstamp.ToHex())
                        {
                            Update_SI_Territory(territory, curTerritory, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        territory = new SI_Territory();
                        Update_SI_Territory(territory, curTerritory, true);
                        _db.SI_Territory.AddObject(territory);
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

        private void Update_SI_Territory(SI_Territory t, SI21900_pgLoadTerritory_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Territory = s.Territory;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
			t.Zone = s.Zone;
            t.Descr = s.Descr;
            t.Distance = s.Distance;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

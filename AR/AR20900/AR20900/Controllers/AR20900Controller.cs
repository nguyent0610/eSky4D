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
namespace AR20900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20900Controller : Controller
    {
        private string _screenNbr = "AR20900";
        private string _userName = Current.UserName;

        AR20900Entities _db = Util.CreateObjectContext<AR20900Entities>(false);

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

        public ActionResult GetTerritory()
        {
            return this.Store(_db.AR20900_pgLoadTerritory().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstTerritory"]);
                ChangeRecords<AR_Territory> lstTerritory = dataHandler.BatchObjectData<AR_Territory>();
                foreach (AR_Territory deleted in lstTerritory.Deleted)
                {
                    var del = _db.AR_Territory.Where(p => p.Territory == deleted.Territory).FirstOrDefault();
                    if (del != null)
                    {
                        _db.AR_Territory.DeleteObject(del);
                    }
                }

                lstTerritory.Created.AddRange(lstTerritory.Updated);

                foreach (AR_Territory curTerritory in lstTerritory.Created)
                {
                    if (curTerritory.Territory.PassNull() == "") continue;

                    var territory = _db.AR_Territory.Where(p => p.Territory.ToLower() == curTerritory.Territory.ToLower()).FirstOrDefault();

                    if (territory != null)
                    {
                        if (territory.tstamp.ToHex() == curTerritory.tstamp.ToHex())
                        {
                            Update_AR_Territory(territory, curTerritory, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        territory = new AR_Territory();
                        Update_AR_Territory(territory, curTerritory, true);
                        _db.AR_Territory.AddObject(territory);
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

        private void Update_AR_Territory(AR_Territory t, AR_Territory s, bool isNew)
        {
            if (isNew)
            {
                t.Territory = s.Territory;
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

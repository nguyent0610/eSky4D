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
namespace AR21200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21200Controller : Controller
    {
        private string _screenNbr = "AR21200";
        private string _userName = Current.UserName;

        AR21200Entities _db = Util.CreateObjectContext<AR21200Entities>(false);

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

        public ActionResult GetLocation()
        {
            return this.Store(_db.AR21200_pgLoadLocation().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstLocation"]);
                ChangeRecords<AR_Location> lstLocation = dataHandler.BatchObjectData<AR_Location>();
                foreach (AR_Location deleted in lstLocation.Deleted)
                {
                    var del = _db.AR_Location.Where(p => p.Location == deleted.Location).FirstOrDefault();
                    if (del != null)
                    {
                        _db.AR_Location.DeleteObject(del);
                    }
                }

                lstLocation.Created.AddRange(lstLocation.Updated);

                foreach (AR_Location curLocation in lstLocation.Created)
                {
                    if (curLocation.Location.PassNull() == "") continue;

                    var Location = _db.AR_Location.Where(p => p.Location.ToLower() == curLocation.Location.ToLower()).FirstOrDefault();

                    if (Location != null)
                    {
                        if (Location.tstamp.ToHex() == curLocation.tstamp.ToHex())
                        {
                            Update_AR_Location(Location, curLocation, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Location = new AR_Location();
                        Update_AR_Location(Location, curLocation, true);
                        _db.AR_Location.AddObject(Location);
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

        private void Update_AR_Location(AR_Location t, AR_Location s, bool isNew)
        {
            if (isNew)
            {
                t.Location = s.Location;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

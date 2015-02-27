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
namespace OM21600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21600Controller : Controller
    {
        private string _screenNbr = "OM21600";
        private string _userName = Current.UserName;
        OM21600Entities _db = Util.CreateObjectContext<OM21600Entities>(false);
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
        public ActionResult GetSalesRoute(string CpnyID)
        {

            return this.Store(_db.OM21600_pgLoadSalesRoute(CpnyID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                var branch = data["cboCpnyID"];
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSalesRoute"]);
                ChangeRecords<OM_SalesRoute> lstSalesRoute = dataHandler.BatchObjectData<OM_SalesRoute>();
                foreach (OM_SalesRoute deleted in lstSalesRoute.Deleted)
                {
                    var del = _db.OM_SalesRoute.Where(p => p.SalesRouteID == deleted.SalesRouteID && p.BranchID == deleted.BranchID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.OM_SalesRoute.DeleteObject(del);
                    }
                }

                lstSalesRoute.Created.AddRange(lstSalesRoute.Updated);

                foreach (OM_SalesRoute curSalesRoute in lstSalesRoute.Created)
                {
                    if (curSalesRoute.SalesRouteID.PassNull() == "" && curSalesRoute.BranchID.PassNull() == "") continue;

                    var SalesRoute = _db.OM_SalesRoute.Where(p => p.SalesRouteID.ToLower() == curSalesRoute.SalesRouteID.ToLower() && p.BranchID.ToLower() == curSalesRoute.BranchID.ToLower()).FirstOrDefault();

                    if (SalesRoute != null)
                    {
                        if (SalesRoute.tstamp.ToHex() == curSalesRoute.tstamp.ToHex())
                        {
                            SalesRoute.BranchID = branch;
                            Update_OM_SalesRoute(SalesRoute, curSalesRoute, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        SalesRoute = new OM_SalesRoute();
                        SalesRoute.BranchID = branch;
                        Update_OM_SalesRoute(SalesRoute, curSalesRoute, true);
                        _db.OM_SalesRoute.AddObject(SalesRoute);
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

        private void Update_OM_SalesRoute(OM_SalesRoute t, OM_SalesRoute s, bool isNew)
        {
            if (isNew)
            {
                t.SalesRouteID = s.SalesRouteID;
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

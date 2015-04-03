using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
//using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
namespace OM22002.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22002Controller : Controller
    {
        private string _screenNbr = "OM22002";
        OM22002Entities _db = Util.CreateObjectContext<OM22002Entities>(false);
        //
        // GET: /OM22002/
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

        public ActionResult GetDet(string zone, string territory, string cpnyID, string displayID, string levelID)
        {
            var dets = _db.OM22002_pgCust(zone, territory, cpnyID, displayID, levelID).ToList();
            return this.Store(dets);
        }

        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var custHandler = new StoreDataHandler(data["lstCustChange"]);
                var lstCustChange = custHandler.BatchObjectData<OM22002_pgCust_Result>();

                foreach (var updated in lstCustChange.Updated)
                {
                    var regisObj = _db.OM_TDisplayCustomer.FirstOrDefault(p => p.BranchID == updated.BranchID 
                        && p.SlsperID == updated.SlsperID && p.CustID == updated.CustID 
                        && p.DisplayID == updated.DisplayID && p.LevelID != updated.LevelID);
                    if (regisObj != null) continue;

                    regisObj = _db.OM_TDisplayCustomer.FirstOrDefault(p => p.BranchID == updated.BranchID
                         && p.SlsperID == updated.SlsperID && p.CustID == updated.CustID
                        && p.DisplayID == updated.DisplayID && p.LevelID == updated.LevelID);
                    if (regisObj == null)
                    {
                        regisObj = new OM_TDisplayCustomer()
                        {
                            BranchID = updated.BranchID,
                            CustID = updated.CustID,
                            DisplayID = updated.DisplayID,
                            LevelID = updated.LevelID,
                            Rate = (double)updated.Rate,
                            SlsperID = updated.SlsperID,
                            Crtd_DateTime = DateTime.Now,
                            Crtd_Prog = _screenNbr,
                            Crtd_User = Current.UserName,

                        };
                        _db.OM_TDisplayCustomer.AddObject(regisObj);

                    }
                    regisObj.LUpd_DateTime = DateTime.Now;
                    regisObj.LUpd_Prog = _screenNbr;
                    regisObj.LUpd_User = Current.UserName;
                    regisObj.Rate = (double)updated.Rate;
                    regisObj.Territory = updated.Territory.PassNull();
                    regisObj.Zone = updated.Zone;
                    regisObj.Status = "H";
                    regisObj.PercentImage = 0;
                    regisObj.PercentSales = 0;

                    _db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }
    }
}

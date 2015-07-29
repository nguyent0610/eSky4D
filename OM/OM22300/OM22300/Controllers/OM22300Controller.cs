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
using HQ.eSkySys;
namespace OM22300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22300Controller : Controller
    {
        private string _cpnyID = Current.CpnyID;
        private string _screenNbr = "OM23200";
        private string _userName = Current.UserName;
        private short _langID = Current.LangID;
        OM22300Entities _db = Util.CreateObjectContext<OM22300Entities>(false);
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

        public ActionResult GetOM_SalesRouteDet(string SalesRouteID,
                                                string CustID,
                                                string SlsPerID,
                                                DateTime FromDate,
                                                DateTime ToDate,
                                                string SlsFreqType)
        {
            return this.Store(_db.OM22300_pgLoadPJPDet(_userName, _langID, _cpnyID, SalesRouteID, CustID, SlsPerID, FromDate, ToDate, SlsFreqType).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_SalesRouteDet"]);
                ChangeRecords<OM22300_pgLoadPJPDet_Result> lstLang = dataHandler.BatchObjectData<OM22300_pgLoadPJPDet_Result>();
                foreach (OM22300_pgLoadPJPDet_Result deleted in lstLang.Deleted)
                {
                    var del = _db.OM_SalesRouteDet.Where(p => p.SalesRouteID.ToLower() == deleted.SalesRouteID.ToLower()
                                                            && p.CustID.ToLower() == deleted.CustID.ToLower()
                                                            && p.SlsPerID.ToLower() == deleted.SlsPerID.ToLower()
                                                            && p.VisitDate == deleted.VisitDate)
                                                            .FirstOrDefault();
                    if (del != null)
                    {
                        _db.OM_SalesRouteDet.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (OM22300_pgLoadPJPDet_Result curLang in lstLang.Created)
                {
                    if (curLang.SalesRouteID.PassNull() == ""
                           || curLang.CustID.PassNull() == ""
                           || curLang.SlsPerID.PassNull() == ""
                           || curLang.VisitDate.PassNull() == ""
                       ) continue;

                    var lang = _db.OM_SalesRouteDet.Where(p => p.SalesRouteID.ToLower() == curLang.SalesRouteID.ToLower()
                                                            && p.CustID.ToLower() == curLang.CustID.ToLower()
                                                            && p.SlsPerID.ToLower() == curLang.SlsPerID.ToLower()
                                                            && p.VisitDate==curLang.VisitDate
                                                        ).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            lang.VisitSort = curLang.VisitSort;
                            lang.LUpd_Datetime = DateTime.Now;
                            lang.LUpd_Prog = _screenNbr;
                            lang.LUpd_User = _userName;
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
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

       
    }
}

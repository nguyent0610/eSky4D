using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using HQ.eSkyFramework;
using System.IO;
using Aspose.Cells;
using HQFramework.DAL;
using System.Drawing;
using System.Data;
using HQFramework.Common;

namespace AR22300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR22300Controller : Controller
    {
        private string _screenName = "AR22300";
        AR22300Entities _db = Util.CreateObjectContext<AR22300Entities>(false);

        //
        // GET: /AR22300/
        public ActionResult Index()
        {
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult LoadMCP(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var planVisit = _db.AR22300_pgMCP(Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(planVisit);
        }

        [ValidateInput(false)]
        public ActionResult ResetGeo(FormCollection data) 
        {
            try
            {
                var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                var lstSelCust = selCustHandler.ObjectData<AR22300_pgMCP_Result>()
                            .Where(p => !string.IsNullOrWhiteSpace(p.CustId)).ToList();

                if (lstSelCust.Count > 0)
                {
                    foreach (var selCust in lstSelCust)
                    {
                        var custLoc = _db.AR_CustomerLocation.FirstOrDefault(p => p.CustID == selCust.CustId
                            && p.BranchID == selCust.BranchID);
                        if (custLoc != null)
                        {
                            if (custLoc.tstamp.ToHex() == selCust.tstamp.ToHex())
                            {
                                if (custLoc.Lat != 0 && custLoc.Lng != 0)
                                {
                                    custLoc.Lat = 0;
                                    custLoc.Lng = 0;
                                }
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "2015052202", "",
                                    new string[] { selCust.CustId });
                        }
                    }

                    _db.SaveChanges();
                    return Json(new { success = true, msgCode = 201405071 });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2015052201", "",
                        new string[] { Util.GetLang("Customer") });
                }
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

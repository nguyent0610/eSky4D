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

namespace SI21100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21100Controller : Controller
    {
        private string _screenNbr = "SI21100";
        private string _userName = Current.UserName;
        SI21100Entities _db = Util.CreateObjectContext<SI21100Entities>(false);

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

        public ActionResult GetSI_Terms(string TermsID)
        {
            return this.Store(_db.SI_Terms.Where(p => p.TermsID == TermsID));
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string TermsID = data["cboTermsID"];

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Terms"]);
                ChangeRecords<SI_Terms> lstSI_Terms = dataHandler.BatchObjectData<SI_Terms>();

                #region Save Header SI_Terms
                lstSI_Terms.Created.AddRange(lstSI_Terms.Updated);
                foreach (SI_Terms curHeader in lstSI_Terms.Created)
                {
                   if (TermsID.PassNull() == "") continue;
                   var header = _db.SI_Terms.FirstOrDefault(p => p.TermsID == TermsID);
                    if (header != null)
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(header, curHeader, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        header = new SI_Terms();
                        header.TermsID = TermsID;
                        UpdatingHeader(header, curHeader, true);
                        _db.SI_Terms.AddObject(header);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void UpdatingHeader(SI_Terms t, SI_Terms s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;
            t.ApplyTo = s.ApplyTo;
            t.DiscType = s.DiscType;
            t.DiscIntrv = s.DiscIntrv;
            t.DiscPct = s.DiscPct;
            t.DueType = s.DueType;
            t.DueIntrv = s.DueIntrv;
          
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try{
                string TermsID = data["cboTermsID"];
                var cpny = _db.SI_Terms.FirstOrDefault(p => p.TermsID == TermsID);
                if (cpny != null)
                {
                    _db.SI_Terms.DeleteObject(cpny);
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

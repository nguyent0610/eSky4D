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
        public ActionResult Save(FormCollection data, bool isNew)
        {

            try
            {
                string TermsID = data["cboTermsID"];

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Terms"]);
                ChangeRecords<SI_Terms> lstSI_Terms = dataHandler.BatchObjectData<SI_Terms>();

                foreach (SI_Terms setup in lstSI_Terms.Updated)
                {
                    var objHeader = _db.SI_Terms.FirstOrDefault(p => p.TermsID == TermsID);
                    if (isNew)//new record
                    {
                        if (objHeader != null)
                            return Json(new { success = false, msgCode = 2000, msgParam = TermsID });//quang message ma nha cung cap da ton tai ko the them
                        else
                        {
                            objHeader = new SI_Terms();
                            objHeader.TermsID = TermsID;
                            objHeader.Crtd_DateTime = DateTime.Now;
                            objHeader.Crtd_Prog = _screenNbr;
                            objHeader.Crtd_User = Current.UserName;
                            UpdatingHeader(setup, ref objHeader);
                            // Add data to SI_Terms
                            _db.SI_Terms.AddObject(objHeader);
                            _db.SaveChanges();
                        }
                    }
                    else if (objHeader != null)//update record
                    {
                        if (objHeader.tstamp.ToHex() == setup.tstamp.ToHex())
                        {
                            UpdatingHeader(setup, ref objHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        _db.SaveChanges();

                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                return Json(new { success = true, TermsID=TermsID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void UpdatingHeader(SI_Terms t,ref SI_Terms s)
        {
            s.Descr = t.Descr;
            s.ApplyTo = t.ApplyTo;
            s.DiscType = t.DiscType;
            s.DiscIntrv = t.DiscIntrv;
            s.DiscPct = t.DiscPct;
            s.DueType = t.DueType;
            s.DueIntrv = t.DueIntrv;
          
            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = _screenNbr;
            s.LUpd_User = _userName;
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

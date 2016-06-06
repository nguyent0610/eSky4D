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

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
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
                string TermsID = data["cboTermsID"].PassNull();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Terms"]);
                
                var curHeader = dataHandler.ObjectData<SI_Terms>().FirstOrDefault();
                #region Save Terms
                var header = _db.SI_Terms.FirstOrDefault(p => p.TermsID == TermsID);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader( curHeader,ref header);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new SI_Terms();
                    header.ResetET();
                    header.TermsID = TermsID;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader( curHeader,ref header);
                    _db.SI_Terms.AddObject(header);
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, TermsID = TermsID }, "text/html");
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
                string TermsID = data["cboTermsID"].PassNull();
                var cpny = _db.SI_Terms.FirstOrDefault(p => p.TermsID == TermsID);
                if (cpny != null)
                {
                    _db.SI_Terms.DeleteObject(cpny);
                }
                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
    }
}

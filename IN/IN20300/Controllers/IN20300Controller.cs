using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkySys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace IN20300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20300Controller : Controller
    {             

        private string _branchID = Current.CpnyID.ToString();
        private string _screenNbr = "IN20300";
        IN20300Entities _db = Util.CreateObjectContext<IN20300Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        
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

        public ActionResult GetIN_Site(string siteId)
        {
            var site = _db.IN20300_ppIN_Site(_branchID, siteId).FirstOrDefault();
            return this.Store(site);
        }

        public ActionResult GetCpny(string siteID)
        {
            var cpnys = _db.IN20300_pcCpnybySite(siteID).ToList();
            return this.Store(cpnys);
        }

        [DirectMethod]
        [HttpPost]       
        public ActionResult Save(FormCollection data, string siteId)
        {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstheader"]);
            ChangeRecords<IN20300_ppIN_Site_Result> lstheader = dataHandler.BatchObjectData<IN20300_ppIN_Site_Result>();
            bool isHo = GetIsHo();
            string SITEID = siteId.ToUpper();
            // Update Site
            foreach (IN20300_ppIN_Site_Result updated in lstheader.Updated)
            {                
                var objHeader = _db.IN_Site.Where(p => p.SiteId == SITEID).FirstOrDefault();
                if (objHeader != null)
                {
                    if (updated.tstamp.ToHex() != objHeader.tstamp.ToHex())
                    {
                        //throw new MessageException(MessageType.Message, "19");
                        return Json(new { success = false, msgCode = 19 });
                    }
                    UpdatingHeader(updated, ref objHeader, false);
                }
                else
                {
                    objHeader = new IN_Site();
                    objHeader.HOCreate = isHo;
                    objHeader.SiteId = siteId;
                    UpdatingHeader(updated, ref objHeader, true);
                    _db.IN_Site.AddObject(objHeader);
                }
            }
            // Save new Site
            foreach (IN20300_ppIN_Site_Result created in lstheader.Created)
            {
                string newSiteId = created.SiteId.PassNull().ToUpper();
                var objHeader = _db.IN_Site.Where(p => p.SiteId.ToUpper() == newSiteId).FirstOrDefault();
                if (objHeader != null)
                {
                    // duplicate data
                    return Json(new { success = false, msgCode = 219 });
                }
                else
                {
                    objHeader = new IN_Site();
                    objHeader.HOCreate = isHo;
                    objHeader.SiteId = newSiteId;
                    UpdatingHeader(created, ref objHeader, true);                    
                    _db.IN_Site.AddObject(objHeader);
                    _db.SaveChanges();
                }
            }

            // Insert new Site_Company
            var objCpny = _db.IN_SiteCpny.Where(p => p.SiteId.ToUpper() == SITEID && p.CpnyID.ToUpper() == Current.CpnyID.ToUpper()).FirstOrDefault();
            if (objCpny == null)
            {
                objCpny = new IN_SiteCpny();
                objCpny.SiteId = siteId;
                objCpny.CpnyID = Current.CpnyID;
                _db.IN_SiteCpny.AddObject(objCpny);
                 _db.SaveChanges();
            }

            #region -Save List Site_Company-
                        
            //StoreDataHandler dataCpnyHandler = new StoreDataHandler(data["lstCpny"]);
            //ChangeRecords<IN_SiteCpny> lstCpny = dataCpnyHandler.BatchObjectData<IN_SiteCpny>();
            //foreach (IN_SiteCpny deleted in lstCpny.Deleted)
            //{
            //    var obj = _db.IN_SiteCpny.Where(p => p.CpnyID == deleted.CpnyID && p.SiteId.ToUpper() == SITEID).FirstOrDefault();
            //    if (obj != null)
            //    {
            //        _db.IN_SiteCpny.DeleteObject(obj);
            //    }
            //}
            //foreach (IN_SiteCpny updated in lstCpny.Updated)
            //{
            //    // Get the image path
            //    var objHeader = _db.IN_SiteCpny.Where(p => p.SiteId == SITEID && p.CpnyID == updated.CpnyID).FirstOrDefault();
            //    if (objHeader == null)
            //    {                                        
            //        objHeader = new IN_SiteCpny();
            //        objHeader.SiteId = siteId;
            //        objHeader.CpnyID = updated.CpnyID;
            //        _db.IN_SiteCpny.AddObject(objHeader);
            //    }
            //}

            //foreach (IN_SiteCpny created in lstCpny.Created)
            //{
            //    // Get the image path
            //    var objHeader = _db.IN_SiteCpny.Where(p => p.SiteId.ToUpper() == siteId && p.CpnyID == created.CpnyID).FirstOrDefault();
            //    if (objHeader == null)
            //    {
            //        if (!string.IsNullOrWhiteSpace(created.CpnyID))
            //        {
            //            objHeader = new IN_SiteCpny();
            //            objHeader.SiteId = siteId;
            //            objHeader.CpnyID = created.CpnyID;
            //            _db.IN_SiteCpny.AddObject(objHeader);
            //            _db.SaveChanges();
            //        }
            //    }
            //}
            #endregion

            // Save all changed
            _db.SaveChanges();
            return Json(new { success = true, SiteId = siteId }, JsonRequestBehavior.AllowGet);
        }

        [DirectMethod]
        public ActionResult IN20300Delete(string siteId)
        {            
            // Delete Site by Id
            siteId = siteId.PassNull().ToUpper();
            var site = _db.IN_Site.FirstOrDefault(p => p.SiteId.ToUpper() == siteId);
            _db.IN_Site.DeleteObject(site);
            // Delete Site_Company
            var lstSiteCpny = _db.IN_SiteCpny.Where(x => x.SiteId.ToUpper() == siteId).ToList();
            foreach (var item in lstSiteCpny)
            {
                _db.IN_SiteCpny.DeleteObject(item);
            }
            _db.SaveChanges();
            return this.Direct();
        }

        // Update record from View
        private void UpdatingHeader(IN20300_ppIN_Site_Result s, ref IN_Site d, bool isNew)
        {
            if (isNew)
            {
                d.Crtd_DateTime = DateTime.Now;
                d.Crtd_Prog = _screenNbr;
                d.Crtd_User = Current.UserName;                
            }
            d.Name = s.Name;
            d.WhKeeper = s.WhKeeper;
            d.Addr1 = s.Addr1;
            d.Addr2 = s.Addr2;            
            d.City = s.City;
            d.Country = s.Country;
            d.District = s.District;
            d.State = s.State;
            d.EmailAddress = s.EmailAddress;
            d.Phone = s.Phone;
            d.Zip = s.Zip;
            d.Public = s.Public;
            d.Fax = s.Fax;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
            d.tstamp = new byte[1];
        }

        // Check user role is "HO"
        private bool GetIsHo()
        {
            var user = _sys.Users.Where(x => x.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            bool isHo = (user != null && user.UserTypes != null && user.UserTypes.Split(',').Any(p => p.ToUpper() == "HO"));
            return isHo;
        }

    }
}

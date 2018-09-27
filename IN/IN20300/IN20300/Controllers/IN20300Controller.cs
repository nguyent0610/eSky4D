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
using HQ.eSkySys;
namespace IN20300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20300Controller : Controller
    {
        private string _BranchID = Current.CpnyID.ToString();
        private string _screenNbr = "IN20300";
        private string _userName = Current.UserName;
        IN20300Entities _db = Util.CreateObjectContext<IN20300Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        public ActionResult Index()
        {
            var allowedSales = false;
            var objConfig = _db.IN20300_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                allowedSales = objConfig.AllowedSales.Value && objConfig.AllowedSales.HasValue;
            }
            ViewBag.allowedSales = allowedSales;
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetIN_Site(string SiteId,string BranchID)
        {
            var site = _db.IN20300_ppIN_Site(BranchID, SiteId).FirstOrDefault();
            return this.Store(site);
        }

        private bool GetIsHo()
        {
            var user = _sys.Users.Where(x => x.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            bool isHo = (user != null && user.UserTypes != null && user.UserTypes.Split(',').Any(p => p.ToUpper() == "HO"));
            return isHo;
        }

        public ActionResult GetCpny(string SiteID)
        {
            var cpnys = _db.IN20300_pcCpnybySite(SiteID).ToList();
            return this.Store(cpnys);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstheader"]);
                ChangeRecords<IN20300_ppIN_Site_Result> lstheader = dataHandler1.BatchObjectData<IN20300_ppIN_Site_Result>();
                bool isHo = GetIsHo();
                string SiteId = data["cboSiteId"].PassNull();
                string BranchID = data["cboBranchID"].PassNull();
                #region Header
                lstheader.Created.AddRange(lstheader.Updated);
                foreach (IN20300_ppIN_Site_Result curHeader in lstheader.Created)
                {
                    if (SiteId.PassNull() == "") continue;

                    var header = _db.IN_Site.FirstOrDefault(p => p.SiteId == SiteId);
                    if (header != null)
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(ref header, curHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        //string images = getPathThenUploadImage(curHeader, UserID);
                        header = new IN_Site();
                        header.SiteId = SiteId;
                        header.HOCreate = isHo;
                        header.Crtd_DateTime = DateTime.Now;
                        header.Crtd_Prog = _screenNbr;
                        header.Crtd_User = Current.UserName;
                        UpdatingHeader(ref header, curHeader);
                        _db.IN_Site.AddObject(header);
                    }
                }
                #endregion

                var objCpny = _db.IN_SiteCpny.FirstOrDefault(p => p.SiteId.ToLower() == SiteId.ToLower() && p.CpnyID.ToLower() == BranchID.ToLower());
                if (objCpny == null)
                {
                    objCpny = new IN_SiteCpny();
                    objCpny.SiteId = SiteId;
                    objCpny.CpnyID = BranchID;
                    _db.IN_SiteCpny.AddObject(objCpny);
                }
                //foreach (IN20300_pcCpnybySite_Result deleted in lstdetail.Deleted)
                //{
                //    var del = _db.IN_SiteCpny.Where(p => p.SiteId == deleted.SiteId && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                //    if (del != null)
                //    {
                //        _db.IN_SiteCpny.DeleteObject(del);
                //    }
                //}

                //lstdetail.Created.AddRange(lstdetail.Updated);

                //foreach (IN20300_pcCpnybySite_Result curLang in lstdetail.Created)
                //{
                //    if (curLang.SiteId.PassNull() == "" || curLang.CpnyID.PassNull()=="") continue;

                //    var lang = _db.IN_SiteCpny.Where(p => p.SiteId.ToLower() == curLang.SiteId.ToLower() && p.CpnyID.ToLower()==curLang.CpnyID.ToLower()).FirstOrDefault();

                //    if (lang != null)
                //    {
                       
                //    }
                //    else
                //    {
                //        lang = new IN_SiteCpny();
                //        lang.SiteId = SiteId;
                //        lang.CpnyID = curLang.CpnyID;
                //        _db.IN_SiteCpny.AddObject(lang);
                //    }
                //}

                _db.SaveChanges();
                return Json(new { success = true ,SiteId=SiteId});
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(ref IN_Site t, IN20300_ppIN_Site_Result s)
        {
            t.Name = s.Name;
            t.WhKeeper = s.WhKeeper;
            t.Addr1 = s.Addr1;
            t.Addr2 = s.Addr2;
            t.City = s.City;
            t.Country = s.Country;
            t.District = s.District;
            t.State = s.State;
            t.EmailAddress = s.EmailAddress;
            t.Phone = s.Phone;
            t.Zip = s.Zip;
            t.Public = s.Public;
            t.Fax = s.Fax;
            t.SiteType = s.SiteType;
            t.AllowedSales = s.AllowedSales;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string SiteId = data["cboSiteId"];
                string BranchID = data["cboBranchID"];

                var cpny = _db.IN_Site.FirstOrDefault(p => p.SiteId == SiteId);
                if (cpny != null)
                {
                    var result = _db.IN20300_ppCheckForDeleteSiteID(Current.CpnyID, Current.UserName, Current.LangID, BranchID, SiteId).FirstOrDefault();
                    if (result != "")
                        throw new MessageException(MessageType.Message, "20410", "", new string[] { result });

                    _db.IN_Site.DeleteObject(cpny);
                }

                var lstAddr = _db.IN_SiteCpny.Where(p => p.SiteId == SiteId).ToList();
                foreach (var item in lstAddr)
                {
                    _db.IN_SiteCpny.DeleteObject(item);
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

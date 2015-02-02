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
namespace AR20600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20600Controller : Controller
    {
        private string _branchID = Current.CpnyID.ToString();
        private string _screenNbr = "AR20600";
        AR20600Entities _db = Util.CreateObjectContext<AR20600Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult GetSOAddressClass(string branchID,string custId,string shipToId)
        {
            var rptSOAddress = _db.AR20600_ppAR_SOAddress().FirstOrDefault(p => p.BranchID == branchID && p.CustId == custId && p.ShipToId == shipToId);
            return this.Store(rptSOAddress);
        }


        [DirectMethod]
        [HttpPost]
       
        public ActionResult Save(FormCollection data,string custId)
        {
            var shipToId = "";
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstheader"]);
            ChangeRecords<AR20600_ppAR_SOAddress_Result> lstheader = dataHandler.BatchObjectData<AR20600_ppAR_SOAddress_Result>();

            foreach (AR20600_ppAR_SOAddress_Result updated in lstheader.Updated)
            {
                // Get the image path
                shipToId = updated.ShipToId;
                var objHeader = _db.AR_SOAddress.Where(p => p.BranchID == _branchID && p.CustId == custId && p.ShipToId == updated.ShipToId).FirstOrDefault();
                if (objHeader != null)
                {
                    UpdatingHeader(updated, ref objHeader);
                }
                else
                {
                    objHeader = new AR_SOAddress();
                    objHeader.BranchID = Current.CpnyID.ToString();
                    objHeader.CustId = custId;
                    objHeader.ShipToId = updated.ShipToId;
                    UpdatingHeader(updated, ref objHeader);
                    
                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = _screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    _db.AR_SOAddress.AddObject(objHeader);
                }
            }

            foreach (AR20600_ppAR_SOAddress_Result created in lstheader.Created)
            {
                // Get the image path
                shipToId = created.ShipToId;
                var objHeader = _db.AR_SOAddress.Where(p => p.BranchID == _branchID && p.CustId == custId && p.ShipToId == created.ShipToId).FirstOrDefault();
                if (objHeader != null)
                {
                    return Json(new { success = false });
                }
                else
                {
                    objHeader = new AR_SOAddress();
                    objHeader.BranchID = _branchID;
                    objHeader.CustId = custId;
                    objHeader.ShipToId = created.ShipToId;
                    UpdatingHeader(created, ref objHeader);

                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = _screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    _db.AR_SOAddress.AddObject(objHeader);
                    _db.SaveChanges();
                }
            }
            _db.SaveChanges();
            //this.Direct();
            return Json(new { success = true, ShipToId = shipToId }, JsonRequestBehavior.AllowGet);
        }

        [DirectMethod]
        public ActionResult AR20600Delete(string custId ,string shipToId)
        {
            
            var soAddress = _db.AR_SOAddress.FirstOrDefault(p => p.BranchID == _branchID && p.CustId == custId && p.ShipToId == shipToId);
            _db.AR_SOAddress.DeleteObject(soAddress);
            _db.SaveChanges();
            return this.Direct();
        }

        private void UpdatingHeader(AR20600_ppAR_SOAddress_Result s, ref AR_SOAddress d)
        {
            d.Descr = s.Descr;
            d.SOName = s.SOName;
            d.Attn = s.Attn;
            d.Addr1 = s.Addr1;
            d.Addr2 = s.Addr2;
            d.Country = s.Country;
            d.State = s.State;
            d.City = s.City;
            d.District = s.District;
            d.Zip = s.Zip;
            d.Phone = s.Phone;
            d.Fax = s.Fax;
            d.SiteId = s.SiteId;
            d.ShipViaID = s.ShipViaID;
            d.TaxRegNbr = s.TaxRegNbr;
            d.TaxLocId = s.TaxLocId;
            d.TaxId00 = s.TaxId00;
            d.TaxId01 = s.TaxId01;
            d.TaxId02 = s.TaxId02;
            d.TaxId03 = s.TaxId03;
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
        }

    }
}

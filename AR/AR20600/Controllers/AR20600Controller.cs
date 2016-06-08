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
        private string _BranchID = Current.CpnyID;
        private string _screenNbr = "AR20600";
        AR20600Entities _db = Util.CreateObjectContext<AR20600Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        
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

        public ActionResult GetAR_SOAddress(string branchID, string custId, string shipToId)
        {
            var rptSOAddress = _db.AR_SOAddress.FirstOrDefault(p => p.BranchID == branchID && p.CustId == custId && p.ShipToId == shipToId);
            return this.Store(rptSOAddress);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string CustId = data["cboCustId"].PassNull();
                string ShipToId = data["cboShipToId"].PassNull();
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAR_SOAddress"]);
                ChangeRecords<AR_SOAddress> lstAR_SOAddress = dataHandler.BatchObjectData<AR_SOAddress>();
                lstAR_SOAddress.Created.AddRange(lstAR_SOAddress.Updated);
                foreach (AR_SOAddress curHeader in lstAR_SOAddress.Created)
                {
                    if (CustId.PassNull() == "" || ShipToId.PassNull()=="") continue;

                    var objHeader = _db.AR_SOAddress.FirstOrDefault(p => p.BranchID == _BranchID && p.CustId == CustId && p.ShipToId == ShipToId);
                    if (objHeader != null)
                    {
                        if (objHeader.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(curHeader, ref objHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        //string images = getPathThenUploadImage(curHeader, UserID);
                        objHeader = new AR_SOAddress();
                        objHeader.BranchID = _BranchID;
                        objHeader.CustId = CustId;
                        objHeader.ShipToId = ShipToId;
                        objHeader.Crtd_DateTime = DateTime.Now;
                        objHeader.Crtd_Prog = _screenNbr;
                        objHeader.Crtd_User = Current.UserName;
                        UpdatingHeader(curHeader, ref objHeader);
                        _db.AR_SOAddress.AddObject(objHeader);
                    }
                }
                _db.SaveChanges();
                return Json(new { success = true, ShipToId = ShipToId });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(AR_SOAddress s, ref AR_SOAddress d)
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

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string CustId = data["cboCustId"].PassNull();
                string ShipToId = data["cboShipToId"].PassNull();
                double convertShipToIdToDouble = 0;
                bool j = double.TryParse(ShipToId,out convertShipToIdToDouble);

                var obj = _db.AR_SOAddress.FirstOrDefault(p =>p.BranchID==_BranchID && p.CustId == CustId && p.ShipToId == ShipToId);
                var checkPO_Trans = (PO_Trans)null;
                var checkPO_Receipt = _db.PO_Receipt.FirstOrDefault(p => p.BranchID == _BranchID && p.ShiptoID == ShipToId);
                if (j)
                {
                    checkPO_Trans = _db.PO_Trans.FirstOrDefault(p => p.BranchID == _BranchID && p.ShiptoID == convertShipToIdToDouble);
                }
                var checkAR_Customer = _db.AR_Customer.FirstOrDefault(p => p.BranchID == _BranchID && p.DfltShipToId == ShipToId);
                var checkPO_Header = _db.PO_Header.FirstOrDefault(p => p.BranchID == _BranchID && p.ShiptoID == ShipToId);
                var checkOM_OrdAddr = _db.OM_OrdAddr.FirstOrDefault(p => p.BranchID == _BranchID && p.ShiptoID == ShipToId);

                if (checkPO_Receipt == null && checkPO_Trans == null && checkAR_Customer == null && checkPO_Header == null && checkOM_OrdAddr == null)
                {
                    if (obj != null)
                    {
                        _db.AR_SOAddress.DeleteObject(obj);
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2016060201");
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

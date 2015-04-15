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
namespace SA00800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00800Controller : Controller
    {
        private string _screenNbr = "SA00800";
        private string _userName = Current.UserName;
        SA00800Entities _db = Util.CreateObjectContext<SA00800Entities>(true);

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

        
        //#region Get information Company
        public ActionResult GetReportControl(String ReportNbr)
        {
            var rpt = _db.SYS_ReportControl.FirstOrDefault(p => p.ReportNbr == ReportNbr);
            return this.Store(rpt);
           
        }

        //public ActionResult GetOM_DocNumbering(string OrderType)
        //{
        //    return this.Store(_db.SA00800_pgOM_DocNumbering(OrderType).ToList());
        //}

        //#endregion

        //#region Save & Update 
        ////Save information Company
        //[HttpPost]
        //public ActionResult Save(FormCollection data)
        //{
        //    try
        //    {
        //        string OrderType = data["cboOrderType_Main"];

        //        StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_OrderType"]);
        //        ChangeRecords<OM_OrderType> lstOM_OrderType = dataHandler.BatchObjectData<OM_OrderType>();

        //        StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstOM_DocNumbering"]);
        //        ChangeRecords<SA00800_pgOM_DocNumbering_Result> lstOM_DocNumbering = dataHandler1.BatchObjectData<SA00800_pgOM_DocNumbering_Result>();

    
        //        #region Save Header OM_OrderType
        //        lstOM_OrderType.Created.AddRange(lstOM_OrderType.Updated);
        //        foreach (OM_OrderType curHeader in lstOM_OrderType.Created)
        //        {
        //            if (OrderType.PassNull() == "") continue;

        //            var header = _db.OM_OrderType.FirstOrDefault(p => p.OrderType == OrderType);
        //            if (header != null)
        //            {
        //                if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
        //                {
        //                    UpdatingHeader(ref header, curHeader);
        //                }
        //                else
        //                {
        //                    throw new MessageException(MessageType.Message, "19");
        //                }
        //            }
        //            else
        //            {
        //                //string images = getPathThenUploadImage(curHeader, UserID);
        //                header = new OM_OrderType();
        //                header.OrderType = OrderType;
        //                header.Crtd_DateTime = DateTime.Now;
        //                header.Crtd_Prog = _screenNbr;
        //                header.Crtd_User = Current.UserName;
        //                header.tstamp = new byte[0];
        //                UpdatingHeader(ref header, curHeader);
        //                _db.OM_OrderType.AddObject(header);
        //            }
        //        }
        //        #endregion

        //        #region Save OM_DocNumbering
        //        foreach (SA00800_pgOM_DocNumbering_Result deleted in lstOM_DocNumbering.Deleted)
        //        {
        //            var objDelete = _db.OM_DocNumbering.FirstOrDefault(p => p.OrderType == OrderType && p.BranchID == deleted.BranchID);
        //            if (objDelete != null)
        //            {
        //                _db.OM_DocNumbering.DeleteObject(objDelete);
        //            }
        //        }

        //        lstOM_DocNumbering.Created.AddRange(lstOM_DocNumbering.Updated);

        //        foreach (SA00800_pgOM_DocNumbering_Result curLang in lstOM_DocNumbering.Created)
        //        {
        //            if (curLang.BranchID.PassNull() == "") continue;

        //            var lang = _db.OM_DocNumbering.FirstOrDefault(p => p.OrderType.ToLower() == OrderType.ToLower() && p.BranchID.ToLower() == curLang.BranchID.ToLower());

        //            if (lang != null)
        //            {
        //                if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
        //                {
        //                    UpdatingOM_DocNumbering(lang, curLang, false);
        //                }
        //                else
        //                {
        //                    throw new MessageException(MessageType.Message, "19");
        //                }
        //            }
        //            else
        //            {
        //                lang = new OM_DocNumbering();
        //                lang.OrderType = OrderType;
        //                UpdatingOM_DocNumbering(lang, curLang, true);
        //                _db.OM_DocNumbering.AddObject(lang);
        //            }
        //        }
        //        #endregion

        //        _db.SaveChanges();
        //        return Json(new { success = true, OrderType = OrderType });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}
        //#endregion

        ////Update OM_OrderType
        //#region Update OM_OrderType
        //private void UpdatingHeader(ref OM_OrderType t,OM_OrderType s)
        //{
        //    t.Active = s.Active;
        //    t.ApplShift = s.ApplShift;
        //    t.ARDocType = s.ARDocType;
        //    t.AutoPromotion = s.AutoPromotion;
        //    t.BO = s.BO;
        //    t.DaysToKeep = s.DaysToKeep;
        //    t.Descr = s.Descr;
        //    t.DfltCustID = s.DfltCustID;
        //    t.DiscType = s.DiscType;
        //    t.INDocType = s.INDocType;
        //    t.SalesType = s.SalesType;
        //    t.RequiredVATInvcNbr = s.RequiredVATInvcNbr;
        //    t.ShippingReport = s.ShippingReport;
        //    t.TaxFee = s.TaxFee;
        //    t.ManualDisc = s.ManualDisc;
          
        //    t.LUpd_DateTime = DateTime.Now;
        //    t.LUpd_Prog = _screenNbr;
        //    t.LUpd_User = _userName;
        //}
        //#endregion
        ////Update OM_DocNumbering
        //#region Update OM_DocNumbering
        //private void UpdatingOM_DocNumbering(OM_DocNumbering t, SA00800_pgOM_DocNumbering_Result s, bool isNew)
        //{
        //    if (isNew)
        //    {
        //        t.BranchID = s.BranchID;
        //        t.Crtd_DateTime = DateTime.Now;
        //        t.Crtd_Prog = _screenNbr;
        //        t.Crtd_User = _userName;
        //    }
        //    t.LastOrderNbr = s.LastOrderNbr;
        //    t.LastShipperNbr = s.LastShipperNbr;
        //    t.LastARRefNbr = s.LastARRefNbr;
        //    t.LastInvcNbr = s.LastInvcNbr;
        //    t.LastInvcNote = s.LastInvcNote;
        //    t.PreFixIN = s.PreFixIN;
        //    t.PreFixShip = s.PreFixShip;
        //    t.PreFixSO = s.PreFixSO;

        //    t.LUpd_DateTime = DateTime.Now;
        //    t.LUpd_Prog = _screenNbr;
        //    t.LUpd_User = _userName;

        //}
        //#endregion

        //#region Delete information Company
        ////Delete information Company
        //[HttpPost]
        //public ActionResult DeleteAll(FormCollection data)
        //{
        //    try
        //    {
        //        string OrderType = data["cboOrderType_Main"];
        //        var cpny = _db.OM_OrderType.FirstOrDefault(p => p.OrderType == OrderType);
        //        if (cpny != null)
        //        {
        //            _db.OM_OrderType.DeleteObject(cpny);
        //        }

        //        var lstAddr = _db.OM_DocNumbering.Where(p => p.OrderType == OrderType).ToList();
        //        foreach (var item in lstAddr)
        //        {
        //            _db.OM_DocNumbering.DeleteObject(item);
        //        }

        //        _db.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}
        //#endregion
    }
}

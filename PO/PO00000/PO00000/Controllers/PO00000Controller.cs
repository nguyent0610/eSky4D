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
namespace PO00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class PO00000Controller : Controller
    {
        private string _screenNbr = "PO00000";
        private string _userName = Current.UserName;

        PO00000Entities _db = Util.CreateObjectContext<PO00000Entities>(false);

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

        public ActionResult GetPO00000(string BranchID, string SetupID)
        {
            var setupData = _db.PO_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupID == SetupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data,bool isNew)
        {
            
            try
            {
                // Get params from data that's sent from client (Ajax)
                string BranchID = Current.CpnyID;
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstPO00000"]);
                ChangeRecords<PO_Setup> lstPO00000 = dataHandler.BatchObjectData<PO_Setup>();
                foreach (PO_Setup setup in lstPO00000.Updated)
                {
                    var objHeader = _db.PO_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupID == "PO");
                    if (isNew)//new record
                    {
                        if (objHeader != null)
                            return Json(new { success = false, msgCode = 2000, msgParam = BranchID });//quang message ma nha cung cap da ton tai ko the them
                        else
                        {
                            objHeader = new PO_Setup();
                            objHeader.BranchID = BranchID;
                            objHeader.SetupID = "PO";
                            objHeader.Crtd_DateTime = DateTime.Now;
                            objHeader.Crtd_Prog = _screenNbr;
                            objHeader.Crtd_User = Current.UserName;
                            UpdatingHeader(setup, ref objHeader);
                            // Add data to PO_Setup
                            _db.PO_Setup.AddObject(objHeader);
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
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(PO_Setup s,ref PO_Setup d)
        {
            d.AutoRef = s.AutoRef;
            d.AutoReleaseAP = s.AutoReleaseAP;
            d.BillAddr1 = s.BillAddr1;
            d.BillAddr2 = s.BillAddr2;
            d.BillAttn = s.BillAttn;

            d.BillCity = s.BillCity;
            d.BillCountry = s.BillCountry;
            d.BillEmail = s.BillEmail;
            d.BillFax = s.BillFax;
            d.BillName = s.BillName;

            d.BillPhone = s.BillPhone;
            d.BillState = s.BillState;
            d.BillZip = s.BillZip;
            d.DfltLstUnitCost = s.DfltLstUnitCost;
            d.DfltRcptFrom = s.DfltRcptFrom;

            d.DfltRcptUnitFromIN = s.DfltRcptUnitFromIN;
            d.EditablePOPrice = s.EditablePOPrice;
            d.LastBatNbr = s.LastBatNbr;
            d.LastPONbr = s.LastPONbr;
            d.LastRcptNbr = s.LastRcptNbr;

            d.PreFixBat = s.PreFixBat;
            d.ShipAddr1 = s.ShipAddr1;
            d.ShipAddr2 = s.ShipAddr2;
            d.ShipAttn = s.ShipAttn;
            d.ShipCity = s.ShipCity;

            d.ShipCountry = s.ShipCountry;
            d.ShipEmail = s.ShipEmail;
            d.ShipFax = s.ShipFax;
            d.ShipName = s.ShipName;
            d.ShipPhone = s.ShipPhone;

            d.ShipState = s.ShipState;
            d.ShipZip = s.ShipZip;
            d.UseAP = s.UseAP;
            d.UseBarCode = s.UseBarCode;
            d.UseIN = s.UseIN;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = _userName;
        }

    }
}

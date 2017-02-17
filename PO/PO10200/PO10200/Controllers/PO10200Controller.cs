using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.Reflection;
using System.Drawing;
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Data;
using HQ.eSkySys;
namespace PO10200.Controllers
{
    [CustomAuthorize]
    [CheckSessionOut]
    [DirectController]
    public class PO10200Controller : Controller
    {

        PO10200Entities _db = Util.CreateObjectContext<PO10200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private const string ScreenNbr = "PO10200";
        private FormCollection _form;
        private List<PO10200_pgDetail_Result> _lstPOTrans = new List<PO10200_pgDetail_Result>();
        private List<PO10200_pgLotTrans_Result> _lstLot = new List<PO10200_pgLotTrans_Result>();
        private List<PO10200_pgLoadTaxTrans_Result> _lstTax = new List<PO10200_pgLoadTaxTrans_Result>();
        private PO10200_pdPO_Setup_Result _objPO_Setup;
        private PO10200_pdHeader_Result _poHead;
        private Batch _objBatch;
        string _batNbr = "";
        string _rcptNbr = "";
        string _branchID = "";
        string _handle = "";
        string _status = "";
        private JsonResult _logMessage;
        private List<IN_ItemSite> lstInItemsiteNew = new List<IN_ItemSite>();
        private List<PO10200_pcSiteAll_Result> lstSiteAll = new List<PO10200_pcSiteAll_Result>();
        bool b235 = false;//message235
        public ActionResult Index()
        {            
            Util.InitRight(ScreenNbr);
            ViewBag.BussinessDate = DateTime.Now.ToDateShort();
            ViewBag.BussinessTime = DateTime.Now;
            bool isChangeSiteID = false;
            var obj = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToLower() == "po10200siteidconfig");
            if (obj != null)
            {
                isChangeSiteID = obj.IntVal == 1;             
            }
            ViewBag.IsChangeSiteID = isChangeSiteID;
            return View();
        }
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        #region Get Data
        public ActionResult GetHeader(string batNbr, string branchID)
        {
            var obj = _db.PO10200_pdHeader(branchID, batNbr).FirstOrDefault();
            return this.Store(obj);

        }
        public ActionResult GetAP_VendorTax(string vendID, string ordFromId)
        {

            return this.Store(_db.PO10200_pdAP_VenDorTaxes(vendID, ordFromId));

        }
        public ActionResult GetPO10200_pgDetail(string rcptNbr, string batNbr, string branchID)
        {
            var lst = _db.PO10200_pgDetail(branchID, batNbr, rcptNbr, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lst);

        }
        public ActionResult GetPO10200_pgLoadTaxTrans(string rcptNbr, string batNbr, string branchID)
        {
            return this.Store(_db.PO10200_pgLoadTaxTrans(branchID, batNbr, rcptNbr).ToList());
        }
        public ActionResult GetPO10200_ppCheckingPONbr(string branchID, string poNbr)
        {
            var obj = _db.PO10200_ppCheckingPONbr(branchID, poNbr).FirstOrDefault();
            return this.Store(obj);

        }
        public ActionResult GetPO10200_pdPODetailReceipt(string branchID, string poNbr)
        {
            var obj = _db.PO10200_pdPODetailReceipt(branchID, poNbr, 0, 0, 0).ToList();
            return this.Store(obj);

        }
        public ActionResult GetPO10200_pdPODetailReturn(string branchID, string poNbr)
        {
            var obj = _db.PO10200_pdPODetailReturn(branchID, poNbr).ToList();
            return this.Store(obj);

        }
        public ActionResult GetLotTrans(string rcptNbr, string batNbr, string branchID,string type,string poNbr)
        {
            var lst = _db.PO10200_pgLotTrans(branchID, batNbr, rcptNbr,type,poNbr).ToList();
            return this.Store(lst);
        }
        
        #endregion
        //#region DataProcess 
        public ActionResult Save(FormCollection data)
        {
            try
            {

                var acc = Session["PO10200"] as AccessRight;
                _form = data;
                _batNbr = data["cboBatNbr"];
                _rcptNbr = data["RcptNbr"];
                _branchID = data["cboBranchID"];
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();
                b235 = _form["b235"].ToBool();
                _objPO_Setup = _db.PO10200_pdPO_Setup(_branchID, "PO").FirstOrDefault();

                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO10200_pdHeader_Result>().FirstOrDefault();

                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPOTrans = detHandler.ObjectData<PO10200_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

                var detHandlerLot = new StoreDataHandler(data["lstLot"]);
                _lstLot = detHandlerLot.ObjectData<PO10200_pgLotTrans_Result>()
                            .Where(p => Util.PassNull(p.LotSerNbr) != string.Empty)
                            .ToList();

                lstSiteAll = _db.PO10200_pcSiteAll(_branchID).ToList();
                if (Data_Checking(b235))
                {
                    if ((_status == "U" || _status == "C"  ) && (_handle == "C" || _handle == "V"))
                    {

                       if ((_handle == "V" || _handle == "C") && !acc.Release)
                            {
                                throw new MessageException(MessageType.Message, "725");
                            }
                            else
                            {
                                if (_handle == "V" || _handle == "C")
                                {
                                    Data_Release();
                                }
                              
                            }                      
                    }
                    else if (_status == "H")
                    {
                        if (_handle == "R" && !acc.Release)
                        {
                            throw new MessageException(MessageType.Message, "737");
                        }
                        else Save_Batch();
                    }
                }
                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save, new { batNbr = _batNbr });

            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        [HttpPost]
        public ActionResult DeleteHeader(FormCollection data)
        {
            try
            {
                var acc = Session["PO10200"] as AccessRight;
                _form = data;
                _batNbr = data["cboBatNbr"];
                _rcptNbr = data["RcptNbr"];
                _branchID = data["cboBranchID"];
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();

                _objPO_Setup = _db.PO10200_pdPO_Setup(_branchID, "PO").FirstOrDefault();


                var detHeader = new StoreDataHandler(data["lstHeader"]);

                if (_poHead == null)
                    _poHead = detHeader.ObjectData<PO10200_pdHeader_Result>().FirstOrDefault();
                var objHeader = _db.Batches.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr).FirstOrDefault();
                if (objHeader != null)
                {
                    if (_poHead.tstamp.ToHex() != objHeader.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    _db.Batches.DeleteObject(objHeader);
                    var objRe = _db.PO_Receipt.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr).FirstOrDefault();
                    if (objRe != null)
                    {
                        _db.PO_Receipt.DeleteObject(objRe);
                    }

                    var objInvoice = _db.PO_Invoice.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr).FirstOrDefault();
                    if (objInvoice != null)
                    {
                        _db.PO_Invoice.DeleteObject(objInvoice);
                    }

                    var lstdel = _db.PO_Trans.Where(p => p.BatNbr == _batNbr && p.BranchID == _branchID && p.RcptNbr == _rcptNbr).ToList();
                    while (lstdel.FirstOrDefault() != null)
                    {
                        var obj = lstdel.FirstOrDefault();
                        if (obj != null && _poHead.RcptType == "X")
                        {
                            var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();
                            double dblQty = (obj.RcptMultDiv == "D" ? (obj.RcptQty / obj.RcptConvFact) : obj.RcptQty * obj.RcptConvFact);
                            //Clear old alloc
                            objItemSite.QtyAllocPORet = Math.Round(objItemSite.QtyAllocPORet - dblQty, 0);
                            objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + dblQty, 0);
                            objItemSite.LUpd_DateTime = DateTime.Now;
                            objItemSite.LUpd_Prog = ScreenNbr;
                            objItemSite.LUpd_User = Current.UserName;

                        }
                        if (obj != null)
                        {
                            // delete lot
                            var lstold = _db.PO_LotTrans.Where(p => p.BranchID == obj.BranchID && p.BatNbr == obj.BatNbr && p.RefNbr == obj.RcptNbr && p.POTranLineRef == obj.LineRef).ToList();
                            foreach (var objlot in lstold)
                            {
                                _db.PO_LotTrans.DeleteObject(objlot);
                                if (_poHead.RcptType == "X")
                                {
                                    double NewQty = (objlot.UnitMultDiv == "D" ? (objlot.Qty / objlot.CnvFact) : (objlot.Qty * obj.CnvFact));
                                    var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == objlot.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == objlot.LotSerNbr).FirstOrDefault();
                                    objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = ScreenNbr;
                                    objItemLot.LUpd_User = Current.UserName;
                                }
                            }
                        }
                        _db.PO_Trans.DeleteObject(obj);
                        lstdel.Remove(obj);
                    }
                    _db.SaveChanges();
                }
                return Util.CreateMessage(MessageProcess.Delete, new { batNbr = "" });

            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }

        }
        [HttpPost]
        public ActionResult DeleteGrd(FormCollection data)
        {
            try
            {
                var acc = Session["PO10200"] as AccessRight;
                _form = data;
                _batNbr = data["cboBatNbr"];
                _rcptNbr = data["RcptNbr"];
                _branchID = data["cboBranchID"];
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO10200_pdHeader_Result>().FirstOrDefault();


                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPOTrans = detHandler.ObjectData<PO10200_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

                var detHandlerLot = new StoreDataHandler(data["lstLot"]);
                _lstLot = detHandlerLot.ObjectData<PO10200_pgLotTrans_Result>()
                            .Where(p => Util.PassNull(p.LotSerNbr) != string.Empty)
                            .ToList();
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstDel"]);
                ChangeRecords<PO10200_pgDetail_Result> lst = dataHandler.BatchObjectData<PO10200_pgDetail_Result>();

                if (_poHead == null)
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                else
                {
                    foreach (PO10200_pgDetail_Result deleted in lst.Deleted.Where(p => p.tstamp != ""))
                    {
                        var obj = _db.PO_Trans.Where(p => p.BranchID == deleted.BranchID && p.BatNbr == deleted.BatNbr && p.RcptNbr == deleted.RcptNbr && p.LineRef == deleted.LineRef).FirstOrDefault();
                        if (obj != null && _poHead.RcptType == "X")
                        {
                            var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();
                            double dblQty = (obj.RcptMultDiv == "D" ? (obj.RcptQty / obj.RcptConvFact) : obj.RcptQty * obj.RcptConvFact);
                            //Clear old alloc
                            objItemSite.QtyAllocPORet = Math.Round(objItemSite.QtyAllocPORet - dblQty, 0);
                            objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + dblQty, 0);
                            objItemSite.LUpd_DateTime = DateTime.Now;
                            objItemSite.LUpd_Prog = ScreenNbr;
                            objItemSite.LUpd_User = Current.UserName;

                        }
                        if (obj != null)
                        {
                            // delete lot
                            var lstold = _db.PO_LotTrans.Where(p => p.BranchID == obj.BranchID && p.BatNbr == obj.BatNbr && p.RefNbr == obj.RcptNbr && p.POTranLineRef == obj.LineRef).ToList();
                            foreach (var objlot in lstold)
                            {
                                _db.PO_LotTrans.DeleteObject(objlot);
                                if (_poHead.RcptType == "X")
                                {
                                    double NewQty = (objlot.UnitMultDiv == "D" ? (objlot.Qty / objlot.CnvFact) : (objlot.Qty * obj.CnvFact));
                                    var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == objlot.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == objlot.LotSerNbr).FirstOrDefault();
                                    objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = ScreenNbr;
                                    objItemLot.LUpd_User = Current.UserName;
                                }
                            }
                            _db.PO_Trans.DeleteObject(obj);
                        }
                    }
                    Save_Batch(true);
                }
                return Util.CreateMessage(MessageProcess.Delete, new { batNbr = _batNbr });


            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        //get price
        [DirectMethod]
        public ActionResult PO10200POPrice(string branchID = "", string invtID = "", string Unit = "", DateTime? podate = null)
        {
            var result = _db.PO10200_ppGetPrice(branchID, invtID, Unit, podate).FirstOrDefault().Value;
            return this.Direct(result);

        }
        [DirectMethod]
        public ActionResult PO10200ItemSitePrice(string branchID = "", string invtID = "", string siteID = "")
        {
            var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == invtID && p.SiteID == siteID).FirstOrDefault();
            if (objIN_ItemSite == null)
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.ResetET();
            }
            return this.Direct(objIN_ItemSite);

        }
        [DirectMethod]
        public ActionResult PO10200ItemSiteQty(string branchID = "", string invtID = "", string siteID = "", string batNbr = "", string rcptNbr = "", string lineRef = "")
        {
            var objold = _db.PO_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RcptNbr == rcptNbr && p.InvtID == invtID && p.SiteID == siteID && p.LineRef == lineRef).FirstOrDefault();
            var qtyold = objold == null ? 0 : objold.UnitMultDiv == "M" ? objold.Qty * objold.CnvFact : objold.Qty / objold.CnvFact;

            var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == invtID && p.SiteID == siteID).FirstOrDefault();
            if (objIN_ItemSite == null)
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.ResetET();
            }
            objIN_ItemSite.QtyAvail=objIN_ItemSite.QtyAvail + qtyold;
            return this.Direct(objIN_ItemSite);

        }       
        [DirectMethod]
        public ActionResult INNumberingLot(string invtID = "", DateTime? tranDate = null, string getType = "LotNbr")
        {
            var LotNbr = _db.INNumberingLot(invtID,tranDate,getType);

            return this.Direct(LotNbr);

        }
        private void Save_Batch(bool isDeleteGrd = false)
        {

            _objBatch = _db.Batches.FirstOrDefault(p => p.Module == "IN" && p.BatNbr == _batNbr && p.BranchID == _branchID);
            if (_objBatch != null)
            {
                if (_objBatch.tstamp.ToHex() != _poHead.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                Updating_Batch(ref _objBatch);

            }
            else
            {
                _objBatch = new Batch();
                _objBatch.ResetET();
                Updating_Batch(ref _objBatch);

                var objBatNbr = _db.INNumbering(_branchID, "BatNbr").FirstOrDefault();
                _objBatch.BranchID = _branchID;
                _objBatch.BatNbr = objBatNbr;

                var objRcptNbr = _db.INNumbering(_branchID, _poHead.RcptType == "R" ? "RcptNbr" : "IssueNbr").FirstOrDefault();
                _objBatch.RefNbr = objRcptNbr;
                _objBatch.OrigBranchID = "";
                _objBatch.DateEnt = DateTime.Now.ToDateShort();
                _objBatch.Crtd_DateTime = DateTime.Now;
                _objBatch.Crtd_Prog = ScreenNbr;
                _objBatch.Crtd_User = Current.UserName;
                _objBatch.tstamp = new byte[0];
                _db.Batches.AddObject(_objBatch);

            }
            _batNbr = _objBatch.BatNbr;
            SavePO_Receipt(_objBatch);
        }
        private void SavePO_Receipt(Batch objBatch)
        {
            var objPO_Receipt = _db.PO_Receipt.FirstOrDefault(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr);
            if (objPO_Receipt != null)
            {
                Updating_PO_Receipt(ref objPO_Receipt);
            }
            else
            {
                objPO_Receipt = new PO_Receipt();
                objPO_Receipt.ResetET();
                Updating_PO_Receipt(ref objPO_Receipt);

                var objRcptNbr = _db.PONumbering(_branchID, "RcptNbr").FirstOrDefault();
                objPO_Receipt.RcptNbr = objRcptNbr;
                objPO_Receipt.BatNbr = objBatch.BatNbr;
                objPO_Receipt.BranchID = objBatch.BranchID;

                objPO_Receipt.Crtd_DateTime = DateTime.Now;
                objPO_Receipt.Crtd_Prog = ScreenNbr;
                objPO_Receipt.Crtd_User = Current.UserName;
                objPO_Receipt.tstamp = new byte[0];

                _db.PO_Receipt.AddObject(objPO_Receipt);

            }
            _rcptNbr = objPO_Receipt.RcptNbr;
            SavePO_INVoice(objPO_Receipt);
        }
        private void SavePO_INVoice(PO_Receipt objPO_Receipt)
        {


            var objPO_Invoice = _db.PO_Invoice.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == objPO_Receipt.RcptNbr).FirstOrDefault();
            if (objPO_Invoice != null)
            {

                Updating_PO_Invoice(ref objPO_Invoice);
                SavePO_Trans(objPO_Receipt);

            }
            else
            {
                objPO_Invoice = new PO_Invoice();
                objPO_Invoice.ResetET();
                Updating_PO_Invoice(ref objPO_Invoice);
                objPO_Invoice.BatNbr = objPO_Receipt.BatNbr;
                objPO_Invoice.RcptNbr = objPO_Receipt.RcptNbr;
                objPO_Invoice.BranchID = objPO_Receipt.BranchID;
                objPO_Invoice.Crtd_Datetime = DateTime.Now;
                objPO_Invoice.Crtd_Prog = ScreenNbr;
                objPO_Invoice.Crtd_User = Current.UserName;
                objPO_Invoice.tstamp = new byte[0];
                _db.PO_Invoice.AddObject(objPO_Invoice);
                SavePO_Trans(objPO_Receipt);


            }

        }
        private void SavePO_Trans(PO_Receipt objPO_Receipt)
        {


            for (int i = 0; i < _lstPOTrans.Count; i++)
            {               
                var objPOT = _lstPOTrans[i];
                var objInvtID=_db.PO10200_pdIN_Inventory(Current.UserName, Current.CpnyID, Current.LangID).Where(p=>p.InvtID==objPOT.InvtID).FirstOrDefault();

                // kiem tra xem co muc lot ko, neu san pham co quan li lot ma khong co muc lot, thong bao khong cho save
                if (objInvtID != null)
                {
                    var qtylot=_lstLot.Where(p => p.InvtID == objPOT.InvtID && p.SiteID == objPOT.SiteID && p.POTranLineRef == objPOT.LineRef).Sum(p => p.Qty);

                    if (objInvtID.LotSerTrack.PassNull() != "N" && objInvtID.LotSerTrack.PassNull() != "" && qtylot != objPOT.RcptQty)
                    {
                        throw new MessageException(MessageType.Message, "201508111", parm: new[] { objPOT.InvtID, qtylot.ToString(), objPOT.RcptQty.ToString() });
                    }
                }
                else
                {                  
                    throw new MessageException(MessageType.Message, "201508112", parm: new[] { objPOT.InvtID});
                }
                var objSite = lstSiteAll.FirstOrDefault(x => x.SiteID == objPOT.SiteID);
                if (objSite == null)
                {
                    throw new MessageException(MessageType.Message, "2016081801", parm: new[] { objPOT.SiteID, _branchID });                        
                }
                var obj = _db.PO_Trans.Where(p => p.BranchID == objPO_Receipt.BranchID && p.BatNbr == objPO_Receipt.BatNbr && p.RcptNbr == objPO_Receipt.RcptNbr && p.LineRef == objPOT.LineRef).FirstOrDefault();
                if (obj != null)
                {
                    if (obj.tstamp.ToHex() != objPOT.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    Updating_PO_Trans(_lstPOTrans[i], ref obj);

                }
                else
                {
                    obj = new PO_Trans();
                    obj.ResetET();
                    Updating_PO_Trans(_lstPOTrans[i], ref obj);
                    obj.BranchID = objPO_Receipt.BranchID;
                    obj.BatNbr = objPO_Receipt.BatNbr;
                    obj.RcptNbr = objPO_Receipt.RcptNbr;
                    obj.LineRef = objPOT.LineRef;
                    obj.Crtd_DateTime = DateTime.Now;
                    obj.Crtd_Prog = ScreenNbr;
                    obj.Crtd_User = Current.UserName;
                    obj.tstamp = new byte[0];
                    _db.PO_Trans.AddObject(obj);
                }
             
            }
            Save_PO_LotTrans();
        
        }
        private void Save_PO_LotTrans()
        {
            try
            {
                //// delete lot cu khong co tren luoi lot
                var lstold = _db.PO_LotTrans.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _rcptNbr).ToList();
                foreach (var obj in lstold)
                {
                    if (_lstLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr && p.POTranLineRef == obj.POTranLineRef).FirstOrDefault() == null)
                    {
                        _db.PO_LotTrans.DeleteObject(obj);
                        if (_poHead.RcptType == "X")
                        {
                            double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                            var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr).FirstOrDefault();

                            objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = ScreenNbr;
                            objItemLot.LUpd_User = Current.UserName;
                        }
                    }
                }
              

                //Save Lot/Serial from datatable to in_lottrans

                foreach (var row in _lstLot)
                {
                    double oldQty = 0;
                    var obj = lstold.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _rcptNbr && p.InvtID == row.InvtID && p.LotSerNbr == row.LotSerNbr && p.SiteID == row.SiteID).FirstOrDefault();
                    if (obj == null)
                    {
                        obj = new PO_LotTrans();
                        obj.ResetET();
                        Update_PO_LotTrans(row, obj);
                        obj.Crtd_Prog = ScreenNbr;
                        obj.Crtd_User = Current.UserName;
                        obj.Crtd_DateTime = DateTime.Now;
                        _db.PO_LotTrans.AddObject(obj);
                    }
                    else
                    {
                        oldQty = obj == null ? 0 : obj.UnitMultDiv == "M" ? obj.Qty * obj.CnvFact : obj.Qty / obj.CnvFact;
                        Update_PO_LotTrans(row, obj);
                    }
                     //Update Location and Site Qty
                    if (_poHead.RcptType == "X")
                    {
                        
                        var qty = obj.UnitMultDiv == "M" ? obj.Qty * obj.CnvFact : obj.Qty / obj.CnvFact;                      
                        var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr).FirstOrDefault();

                        objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet + qty - oldQty, 0);
                        objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail - qty + oldQty, 0);

                        objItemLot.LUpd_DateTime = DateTime.Now;
                        objItemLot.LUpd_Prog = ScreenNbr;
                        objItemLot.LUpd_User = Current.UserName;
                        if (objItemLot.QtyAvail < 0)
                            throw new MessageException(MessageType.Message, "35");
                    }
                }
                _db.SaveChanges();
                if (_handle == "R")
                {
                    Data_Release();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }         
        private void Updating_Batch(ref Batch objBatch)
        {

            try
            {

                objBatch.TotAmt = _poHead.TotAmt;
                objBatch.DateEnt = DateTime.Now.ToDateShort();
                objBatch.EditScrnNbr = ScreenNbr;
                objBatch.Descr = "PO Receipt";
                objBatch.Module = "IN";
                objBatch.JrnlType = "PO";
                objBatch.Rlsed = 0;
                objBatch.Status = _poHead.Status;

                objBatch.LUpd_DateTime = DateTime.Now;
                objBatch.LUpd_Prog = ScreenNbr;
                objBatch.LUpd_User = Current.UserName;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void Updating_PO_Receipt(ref PO_Receipt objR)
        {
            try
            {

                objR.RcptFeeTot = _poHead.RcptFeeTot;
                objR.RcptTot = _poHead.RcptTot;
                objR.DiscAmt = _poHead.DiscAmt;
                objR.DiscAmtPct = _poHead.DiscAmtPct;
                objR.RcptTotAmt = _poHead.RcptTotAmt;


                objR.TaxAmtTot00 = _poHead.TaxAmtTot00;
                objR.TxblAmtTot00 = _poHead.TxblAmtTot00;
                objR.TaxID00 = _poHead.TaxID00;

                objR.TaxAmtTot01 = _poHead.TaxAmtTot01;
                objR.TxblAmtTot01 = _poHead.TxblAmtTot01;
                objR.TaxID01 = _poHead.TaxID01;

                objR.TaxAmtTot02 = _poHead.TaxAmtTot02;
                objR.TxblAmtTot02 = _poHead.TxblAmtTot02;
                objR.TaxID02 = _poHead.TaxID02;

                objR.TaxAmtTot03 = _poHead.TaxAmtTot03;
                objR.TxblAmtTot03 = _poHead.TxblAmtTot03;
                objR.TaxID03 = _poHead.TaxID03;

                objR.Descr = _poHead.Descr;
                //objR.NoteID = this.ReceiptNoteID;
                objR.PONbr = _poHead.PONbr;
                objR.RcptDate = _poHead.RcptDate.ToDateShort();
                objR.RcptType = _poHead.RcptType;
                objR.RcptFrom = _poHead.RcptFrom;
                objR.RcptQtyTot = _poHead.RcptQtyTot;
                objR.VendID = _poHead.VendID;
                objR.Status = _poHead.Status;


                objR.LUpd_DateTime = DateTime.Now;
                objR.LUpd_Prog = ScreenNbr;
                objR.LUpd_User = Current.UserName;
                objR.tstamp = new byte[0];

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void Updating_PO_Invoice(ref PO_Invoice objI)
        {

            try
            {
                objI.InvcNbr = _poHead.InvcNbr;
                objI.InvcNote = _poHead.InvcNote;
                objI.InvcDate = _poHead.InvcDate.ToDateShort();
                objI.DocType = _poHead.DocType;
                objI.DocDate = _poHead.DocDate.ToDateShort();
                objI.APBatNbr = _poHead.APBatNbr;
                objI.APRefNbr = _poHead.APRefNbr;
                objI.Terms = _poHead.Terms;
                objI.VendID = _poHead.VendID;

                objI.LUpd_Datetime = DateTime.Now;
                objI.LUpd_Prog = ScreenNbr;
                objI.LUpd_User = Current.UserName;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void Updating_PO_Trans(PO10200_pgDetail_Result objr, ref PO_Trans objPO_Tr)
        {
            try
            {
                objr.PurchaseType = objr.PurchaseType;
                if (objr.PurchaseType == "GI" || objr.PurchaseType == "PR" || objr.PurchaseType == "GP" || objr.PurchaseType == "GS")
                {
                    var objIN_Inventory = _db.PO10200_pdIN_Inventory(Current.UserName, Current.CpnyID, Current.LangID).Where(p => p.InvtID == objr.InvtID).FirstOrDefault();
                    var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == objr.InvtID && p.SiteID == objr.SiteID).FirstOrDefault();
                    //Kiem tra itemsite neu chua co thi add vao
                    if (objIN_ItemSite == null && lstInItemsiteNew.Where(p => p.InvtID == objr.InvtID && p.SiteID == objr.SiteID).Count()==0)
                    {
                        Insert_IN_ItemSite(ref objIN_ItemSite, ref objIN_Inventory, objr.SiteID);
                    }
                    //Update Location and Site Qty
                    if (_poHead.RcptType == "X")
                    {
                        double OldQty = 0;
                        double NewQty = 0;
                        NewQty = (objr.RcptMultDiv == "D" ? (objr.RcptQty / objr.RcptConvFact) : (objr.RcptQty * objr.RcptConvFact));
                        OldQty = (objr.RcptMultDiv == "D" ? (objPO_Tr.RcptQty / objPO_Tr.RcptConvFact) : objPO_Tr.RcptQty * objPO_Tr.RcptConvFact);

                        //objIN_ItemSite =_db.IN_ItemSite.Where(p => p.InvtID == objr.InvtID && p.SiteID == objr.SiteID).FirstOrDefault();

                        if (objIN_ItemSite != null)
                        {
                            ////Clear old alloc
                            //objIN_ItemSite.QtyAllocPORet = Math.Round(objIN_ItemSite.QtyAllocPORet - OldQty, 0);
                            //objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail + OldQty, 0);


                            ////Update new alloc
                            //objIN_ItemSite.QtyAllocPORet = Math.Round(objIN_ItemSite.QtyAllocPORet + NewQty, 0);
                            //objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail - NewQty, 0);


                            objIN_ItemSite.QtyAllocPORet = Math.Round(objIN_ItemSite.QtyAllocPORet - OldQty + NewQty, 0);
                            objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail + OldQty - NewQty, 0);

                            objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                            objIN_ItemSite.LUpd_Prog = ScreenNbr;
                            objIN_ItemSite.LUpd_User = Current.UserName;

                            if (objIN_ItemSite.QtyAvail < 0)
                                throw new MessageException(MessageType.Message, "35");
                        }
                    }
                }



                objPO_Tr.CnvFact = objr.CnvFact;
                if (objr.CostID.Length == 0)
                {
                    objPO_Tr.CostID = _poHead.InvcNbr + "_" + _poHead.RcptDate.ToString("yyyyMMdd");
                }
                else
                {
                    objPO_Tr.CostID = objr.CostID;
                }

                objPO_Tr.CostVouched = objr.CostVouched;
                objPO_Tr.UnitCost = objr.UnitCost;
                objPO_Tr.RcptFee = objr.RcptFee;

                objPO_Tr.DocDiscAmt = objr.DocDiscAmt;
                objPO_Tr.DiscPct = objr.DiscPct;
                objPO_Tr.ExtVolume = objr.ExtVolume;
                objPO_Tr.ExtWeight = objr.ExtWeight;
                objPO_Tr.InvtID = objr.InvtID;
                objPO_Tr.JrnlType = string.IsNullOrEmpty(objr.PONbr) ? "PO" : objr.JrnlType;
                objPO_Tr.OrigRcptDate = _poHead.RcptDate.ToDateShort();
                objPO_Tr.OrigRcptNbr = objr.OrigRcptNbr;
                objPO_Tr.OrigRetRcptNbr = objr.OrigRetRcptNbr;
                objPO_Tr.POLineRef = objr.POLineRef;
                objPO_Tr.PONbr = objr.PONbr;
                objPO_Tr.POOriginal = objr.POOriginal;
                objPO_Tr.PurchaseType = objr.PurchaseType;
                objPO_Tr.RcptConvFact = objr.RcptConvFact == 0 ? 1 : objr.RcptConvFact;
                objPO_Tr.UnitMultDiv = objr.UnitMultDiv;
                objPO_Tr.CnvFact = objr.CnvFact;
                objPO_Tr.Qty = objr.Qty;
                objPO_Tr.PosmID = objr.PosmID;
                if (string.IsNullOrEmpty(objr.PONbr))
                {
                    if (objr.UnitMultDiv == "M")
                    {
                        objPO_Tr.Qty = objr.RcptMultDiv == "M" ? objr.RcptConvFact * objr.RcptQty / objPO_Tr.CnvFact : (objr.RcptQty / objr.RcptConvFact) / objPO_Tr.CnvFact;
                    }
                    else
                    {
                        objPO_Tr.Qty = objr.RcptMultDiv == "M" ? objr.RcptConvFact * objr.RcptQty * objPO_Tr.CnvFact : objr.RcptQty / objr.RcptConvFact * objPO_Tr.CnvFact;
                    }
                }
                objPO_Tr.QtyVouched = objr.QtyVouched;

                objPO_Tr.RcptDate = _poHead.RcptDate.ToDateShort();
                objPO_Tr.RcptMultDiv = objr.RcptMultDiv;

                objPO_Tr.RcptQty = objr.RcptQty;
                objPO_Tr.RcptUnitDescr = objr.RcptUnitDescr;
                objPO_Tr.ReasonCD = objr.ReasonCD;
                objPO_Tr.SiteID = objr.SiteID;
                objPO_Tr.TaxCat = objr.TaxCat;
                objPO_Tr.TaxID00 = objr.TaxID00;
                objPO_Tr.TaxID01 = objr.TaxID01;
                objPO_Tr.TaxID02 = objr.TaxID02;
                objPO_Tr.TaxID03 = objr.TaxID03;

                objPO_Tr.TaxAmt00 = objr.TaxAmt00;
                objPO_Tr.TaxAmt01 = objr.TaxAmt01;
                objPO_Tr.TaxAmt02 = objr.TaxAmt02;
                objPO_Tr.TaxAmt03 = objr.TaxAmt03;

                objPO_Tr.TxblAmt00 = objr.TxblAmt00;
                objPO_Tr.TxblAmt01 = objr.TxblAmt01;
                objPO_Tr.TxblAmt02 = objr.TxblAmt02;
                objPO_Tr.TxblAmt03 = objr.TxblAmt03;


                objPO_Tr.TranDate = _poHead.RcptDate.ToDateShort();
                objPO_Tr.TranDesc = objr.TranDesc;
                objPO_Tr.TranType = _poHead.RcptType;
                objPO_Tr.UnitDescr = objr.UnitDescr;

                objPO_Tr.UnitVolume = objr.UnitVolume;
                objPO_Tr.UnitWeight = objr.UnitWeight;
                objPO_Tr.VendID = _poHead.VendID;
                objPO_Tr.VouchStage = objr.VouchStage;
                objPO_Tr.TranAmt = objr.TranAmt;

                objPO_Tr.LUpd_DateTime = DateTime.Now;
                objPO_Tr.LUpd_Prog = ScreenNbr;
                objPO_Tr.LUpd_User = Current.UserName;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void Update_PO_LotTrans(PO10200_pgLotTrans_Result row, PO_LotTrans objPO_LotTrans)
        {
           
            objPO_LotTrans.MfcDate = row.MfcDate;
            objPO_LotTrans.BranchID = _branchID;
            objPO_LotTrans.BatNbr = _batNbr;
            objPO_LotTrans.RefNbr = _rcptNbr;
            //row.RefNbr")
            objPO_LotTrans.LotSerNbr = row.LotSerNbr;
            objPO_LotTrans.ExpDate = row.ExpDate;
            objPO_LotTrans.POTranLineRef = row.POTranLineRef;
            objPO_LotTrans.InvtID = row.InvtID;
            objPO_LotTrans.InvtMult = (_poHead.RcptType=="X"?short.Parse("-1"):short.Parse("1"));
            objPO_LotTrans.KitID = row.KitID;
            objPO_LotTrans.MfgrLotSerNbr = row.MfgrLotSerNbr;
            objPO_LotTrans.Qty = row.Qty;
            objPO_LotTrans.SiteID = row.SiteID;
            objPO_LotTrans.WhseLoc = row.WhseLoc;
            objPO_LotTrans.CnvFact = row.CnvFact;
            objPO_LotTrans.ToSiteID = row.ToSiteID;
            objPO_LotTrans.TranDate = _poHead.RcptDate;
            objPO_LotTrans.TranType = _poHead.RcptType;
            objPO_LotTrans.UnitCost = row.UnitCost;
            objPO_LotTrans.UnitPrice = row.UnitPrice;
            objPO_LotTrans.WarrantyDate = row.WarrantyDate;
            objPO_LotTrans.UnitMultDiv = row.UnitMultDiv;
            objPO_LotTrans.UnitDesc = row.UnitDesc;
          
            objPO_LotTrans.LUpd_Prog = ScreenNbr;
            objPO_LotTrans.LUpd_User = Current.UserName;
            objPO_LotTrans.LUpd_DateTime = DateTime.Now;
        }        
        public void Insert_IN_ItemSite(ref IN_ItemSite objIN_ItemSite, ref PO10200_pdIN_Inventory_Result objIN_Inventory, string SiteID)
        {
            try
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.InvtID = objIN_Inventory.InvtID;
                objIN_ItemSite.SiteID = SiteID;
                objIN_ItemSite.AvgCost = 0;
                objIN_ItemSite.QtyAlloc = 0;
                objIN_ItemSite.QtyAllocIN = 0;
                objIN_ItemSite.QtyAllocPORet = 0;
                objIN_ItemSite.QtyAllocSO = 0;
                objIN_ItemSite.QtyAvail = 0;
                objIN_ItemSite.QtyInTransit = 0;
                objIN_ItemSite.QtyOnBO = 0;
                objIN_ItemSite.QtyOnHand = 0;
                objIN_ItemSite.QtyOnPO = 0;
                objIN_ItemSite.QtyOnTransferOrders = 0;
                objIN_ItemSite.QtyOnSO = 0;
                objIN_ItemSite.QtyShipNotInv = 0;
                objIN_ItemSite.StkItem = objIN_Inventory.StkItem;
                objIN_ItemSite.TotCost = 0;
                objIN_ItemSite.LastPurchaseDate = DateTime.Now;

                objIN_ItemSite.Crtd_DateTime = DateTime.Now;
                objIN_ItemSite.Crtd_Prog = ScreenNbr;
                objIN_ItemSite.Crtd_User = Current.UserName;

                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                objIN_ItemSite.LUpd_Prog = ScreenNbr;
                objIN_ItemSite.LUpd_User = Current.UserName;
                objIN_ItemSite.tstamp = new byte[0];

                _db.IN_ItemSite.AddObject(objIN_ItemSite);
                lstInItemsiteNew.Add(objIN_ItemSite);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        //private void SendMail(PO_Header objHeader)
        //{
        //    try
        //    {
        //        HQSendMailApprove.Approve.SendMailApprove(objHeader.BranchID, objHeader.PONbr, ScreenNbr, Current.CpnyID, _status, _handle, Current.UserName, Current.LangID);
        //    }
        //    catch
        //    {
        //    }
        //}
        private bool Data_Checking(bool isCheckInvoicePass = false)
        {
            if (_poHead.Status == "H")
            {

                if (_objPO_Setup == null)
                {
                    throw new MessageException(MessageType.Message, "20404",
                      parm: new[] { "PO_Setup" });

                }

                //if (_poHead.VendID.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("VendID") });
                //}

                //if (_poHead.RcptFrom.PassNull() == "PO" && _poHead.PONbr.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("PONbr") });
                //}
                //if (_poHead.RcptFrom.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("RcptFrom") });
                //}
                //if (_poHead.RcptType.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("RcptType") });
                //}
                //if (_poHead.DocType.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("DocType") });
                //}
                //if (_poHead.VendID.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("VendID") });
                //}

                //if (_poHead.InvcNbr.PassNull() == "" || _poHead.InvcNote.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("InvcNbr") });
                //}
                //if (_poHead.Terms.PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("Terms") });
                //}

                //if (_poHead.DocDate.ToString().PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("DocDate") });
                //}

                //if (_poHead.InvcDate.ToString().PassNull() == "")
                //{
                //    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("InvcDate") });
                //}

                //Check PO has no detail data
                if (_lstPOTrans.Count == 0)
                {
                    throw new MessageException(MessageType.Message, "704");


                }
                if (_lstPOTrans.Where(p => string.IsNullOrEmpty(p.RcptUnitDescr.PassNull())).Count() > 0)
                {
                    throw new MessageException(MessageType.Message, "25");

                }

                for (Int32 i = 0; i < _lstPOTrans.Count; i++)
                {
                    PO10200_pgDetail_Result objPO_Trans = new PO10200_pgDetail_Result();
                    objPO_Trans = _lstPOTrans[i];
                    if ((objPO_Trans.PurchaseType == "GI" || objPO_Trans.PurchaseType == "GP") && (objPO_Trans.SiteID.Length == 0))
                    {
                        throw new MessageException(MessageType.Message, "222");

                    }
                    if ((objPO_Trans.RcptQty == 0 || objPO_Trans.TranAmt == 0) && objPO_Trans.PurchaseType != "PR")
                    {
                        throw new MessageException(MessageType.Message, "44");

                    }

                }


                if (_poHead.RcptType == "X")
                {
                    string invtID = "";

                    //kiểm tra trong itemSite
                    foreach (var objTran in _lstPOTrans)
                    {
                        var objold = _db.PO_Trans.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr && p.InvtID == objTran.InvtID && p.SiteID == objTran.SiteID && p.LineRef == objTran.LineRef).FirstOrDefault();

                        var qtyold = objold == null ? 0 : objold.UnitMultDiv == "M" ? objold.Qty * objold.CnvFact : objold.Qty / objold.CnvFact;
                        var qty = objTran.UnitMultDiv == "M" ? objTran.Qty * objTran.CnvFact : objTran.Qty / objTran.CnvFact;
                        var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == objTran.InvtID && p.SiteID == objTran.SiteID).FirstOrDefault();

                        if (objItemSite == null || (qty - qtyold) > objItemSite.QtyAvail)
                        {
                            invtID += objTran.InvtID + ",";
                        }

                    }
                    if (invtID != "") throw new MessageException(MessageType.Message, "1043", parm: new[] { invtID, "" });

                    //kiểm tra trong itemlot
                    foreach (var objlot in _lstLot)
                    {
                        var objold = _db.PO_LotTrans.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _rcptNbr && p.POTranLineRef == objlot.POTranLineRef && p.LotSerNbr == objlot.LotSerNbr && p.InvtID == objlot.InvtID && p.SiteID == objlot.SiteID).FirstOrDefault();

                        var qtyold = objold == null ? 0 : objold.UnitMultDiv == "M" ? objold.Qty * objold.CnvFact : objold.Qty / objold.CnvFact;
                        var qty = objlot.UnitMultDiv == "M" ? objlot.Qty * objlot.CnvFact : objlot.Qty / objlot.CnvFact;
                        var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == objlot.InvtID && p.SiteID == objlot.SiteID && p.LotSerNbr == objlot.LotSerNbr).FirstOrDefault();

                        if (objItemLot == null || (qty - qtyold) > objItemLot.QtyAvail)
                        {
                            invtID += objlot.InvtID + ",";
                        }

                    }
                    if (invtID != "") throw new MessageException(MessageType.Message, "1043", parm: new[] { invtID, "" });

                }



                if (_db.PO10200_ppCheckCloseDate(_branchID, _poHead.RcptDate.ToDateShort()).FirstOrDefault() == "0")
                    throw new MessageException(MessageType.Message, "301");


                var obj = _db.PO10200_ppCheckingRelease(_branchID, _batNbr).FirstOrDefault();
                if (obj != null)
                {
                    throw new MessageException(MessageType.Message, "60");
                }

                var obj1 = _db.PO10200_ppCheckExistingInvcNbr(_branchID, _batNbr, _poHead.VendID, _poHead.InvcNote, _poHead.InvcNbr).FirstOrDefault();
                if (obj1 != null && !isCheckInvoicePass)
                {
                    throw new MessageException(MessageType.Message, "235", fn: "process235");
                }


            }

            return true;
        }
        private void Data_Release()
        {
            if (_handle != "N")
            {
                DataAccess dal = Util.Dal();
                try
                {
                    POProcess.PO po = new POProcess.PO(Current.UserName, ScreenNbr, dal);
                    if (_handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!po.PO10200_Release(_branchID, _batNbr, _rcptNbr))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _batNbr });
                    }
                    else if (_handle == "C" || _handle == "V")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!po.PO10200_Cancel(_branchID, _batNbr, _rcptNbr, _form["b714"].ToBool()))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }
                        Util.AppendLog(ref _logMessage, "9999", data: new { success = true, batNbr = _batNbr });
                    }
                    po = null;
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
            }
        }
        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                _batNbr = data["cboBatNbr"];
                _rcptNbr = data["RcptNbr"];
                _branchID = data["cboBranchID"];
                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO10200_pdHeader_Result>().FirstOrDefault();

                string reportName = _poHead.RcptType == "R" ? "PO_Receipt" : "PO_Return";
                var rpt = new RPTRunning();
                rpt.ResetET();
                rpt.ReportNbr = "PO603";
                rpt.MachineName = "Web";
                rpt.ReportCap = "ReportName";
                rpt.ReportName = reportName;
                rpt.ReportDate = DateTime.Now;
                rpt.DateParm00 = DateTime.Now;
                rpt.DateParm01 = DateTime.Now;
                rpt.DateParm02 = DateTime.Now;
                rpt.DateParm03 = DateTime.Now;
                rpt.StringParm00 = _branchID;
                rpt.StringParm01 = _batNbr;
                rpt.StringParm02 = _rcptNbr;
                rpt.UserID = Current.UserName;
                rpt.AppPath = "Reports\\";
                rpt.ClientName = Current.UserName;
                rpt.LoggedCpnyID = Current.CpnyID;
                rpt.CpnyID = Current.CpnyID;
                rpt.LangID = Current.LangID;
                _db.RPTRunnings.AddObject(rpt);

                _db.SaveChanges();

                return Json(new { success = true, reportID = rpt.ReportID, reportName });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }  
    }
    public class CountInvtID
    {
        public int count { get; set; }
        public string invtID { get; set; }
    }
}

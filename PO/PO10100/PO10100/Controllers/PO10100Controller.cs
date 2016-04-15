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
namespace PO10100.Controllers
{
   
    [CustomAuthorize]
    [CheckSessionOut]
    [DirectController]
    public class PO10100Controller : Controller
    {
      
        PO10100Entities _db = Util.CreateObjectContext<PO10100Entities>(false);
        private const string ScreenNbr = "PO10100";
        private FormCollection _form;   
        private List<PO10100_pgDetail_Result> _lstPODetailLoad=new List<PO10100_pgDetail_Result>();     
        private PO_Setup _objPO_Setup;
        private PO_Header _poHead;
        bool _statusClose = false;
        string _ponbr = "";
        string _branchID = "";
        string _toStatus = "";
        string _status = "";
        private JsonResult _logMessage;

        List<PO10100_pdOM_DiscAllByBranchPO_Result> _lstPO10100_pdOM_DiscAllByBranchPO;      
        List<PO10100_pdIN_UnitConversion_Result> _PO10100_pdIN_UnitConversion_Result;
       // List<IN_ItemSite> _lstIN_ItemSite;
        private List <PO10100_pgDetail_Result> _lstTmpPO10100_pgDetail;
        private bool _freeLineRunning = false;
        private string _lineRef = string.Empty;
        private int _countAddItem = 0;
        private OM_UserDefault objOM_UserDefault;      
        public ActionResult Index()
        {
            ViewBag.BussinessDate = DateTime.Now.ToDateShort();
            ViewBag.BussinessTime = DateTime.Now;
            return View();
        }
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }       
        #region Get Data
        public ActionResult GetPO_Header(string pONbr, string branchID)
        {
            var obj = _db.PO_Header.Where(p => p.PONbr == pONbr && p.BranchID == branchID);
            return this.Store(obj);

        }
        public ActionResult GetAP_VendorTax(string vendID, string ordFromId)
        {
          
            return this.Store(_db.PO10100_pdAP_VenDorTaxes(vendID,ordFromId));

        }    
        public ActionResult GetPO10100_pgDetail(string pONbr, string branchID)
        {
            _lstPODetailLoad = _db.PO10100_pgDetail(pONbr, branchID, "%").ToList();
            return this.Store(_lstPODetailLoad);

        }
        public ActionResult GetPO10100_pgLoadTaxTrans(string pONbr, string branchID)
        {
            return this.Store(_db.PO10100_pgLoadTaxTrans(branchID, pONbr).ToList());
        }

        #endregion
        #region DataProcess 
        //[HttpPost]
        //public ActionResult ExportPOSuggest(string type, string branchID, DateTime pODate, string vendID)
        //{
        //    try
        //    {
        //        string filePath = GetExcelPOSuggest(type, branchID, pODate, vendID);
        //        return Json(new { success = true, filePath });
        //    }
        //    catch (Exception ex)
        //    {
        //        return (ex as MessageException).ToMessage();
        //    }
        //}
        public ActionResult Download(string filePath)
        {
            var dlFileName = string.Format("{0}_{1}.xls", Util.GetLang("PO10100Sugguest"), DateTime.Now.ToString("ddMMyyHHmmss"));
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            System.IO.File.Delete(filePath);
            return File(fileBytes, "application/xls", dlFileName);
        }
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {               
                _form = data;
                _ponbr = data["cboPONbr"];
                _branchID = data["cboBranchID"];
                _status = data["Status"].PassNull();
                _toStatus = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();
                DateTime dpoDate = data["PODate"].ToDateShort();


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO_Header>().FirstOrDefault();


                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPODetailLoad = detHandler.ObjectData<PO10100_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

                if (_status == "H")
                    Save_PO_Header();
                else
                {
                    var obj = _db.PO_Header.FirstOrDefault(p => p.PONbr == _ponbr && p.BranchID == _branchID);
                    if (obj != null)
                    {

                        if (obj.tstamp.ToHex() != _poHead.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        obj.Status = _toStatus;
                        _db.SaveChanges();
                        if (_toStatus != _status) SendMail(obj);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                return Util.CreateMessage(MessageProcess.Save, new { pONbr = _ponbr });
              
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
        public ActionResult ClosePO(FormCollection data)
        {
            try
            {
                _form = data;
                _ponbr = data["cboPONbr"];
                _branchID = data["cboBranchID"];
                _status = data["Status"].PassNull();
                _toStatus = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();
                DateTime dpoDate = data["PODate"].ToDateShort();


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO_Header>().FirstOrDefault();


                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPODetailLoad = detHandler.ObjectData<PO10100_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();
                var obj = _db.PO10100_ppCheckingPONbr(_branchID, _ponbr).FirstOrDefault();
                if (obj != null)
                {
                     throw new MessageException(MessageType.Message, "60");
                   
                }
                  var objPO_Header = _db.PO_Header.FirstOrDefault(p => p.PONbr == _ponbr && p.BranchID == _branchID);
                     objPO_Header.Status = "C";
                     objPO_Header.LUpd_DateTime = DateTime.Now;
                     objPO_Header.LUpd_Prog = ScreenNbr;
                     objPO_Header.LUpd_User = Current.UserName;
              
                for (int i = 0; i < _lstPODetailLoad.Count; i++)
                {
                    var objrPO_Detail = _lstPODetailLoad[i];
                    var objDetail = _db.PO_Detail.Where(p => p.BranchID == _branchID && p.PONbr == _ponbr && p.LineRef == objrPO_Detail.LineRef).FirstOrDefault();
                    if (objrPO_Detail.PurchaseType == "GI" || objrPO_Detail.PurchaseType == "PR" || objrPO_Detail.PurchaseType == "GP" || objrPO_Detail.PurchaseType == "GS")
                    {
                        double OldQty = Math.Round((objDetail.UnitMultDiv == "D" ? ((objDetail.QtyOrd - objDetail.QtyRcvd) / objDetail.CnvFact) : (objDetail.QtyOrd - objDetail.QtyRcvd) * objDetail.CnvFact));
                        var objIN_ItemSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == objDetail.InvtID && p.SiteID == objDetail.SiteID);
                        objIN_ItemSite.QtyOnPO = Math.Round(objIN_ItemSite.QtyOnPO - OldQty, 2);
                     
                        //UpdateOnPOQty(objDetail.InvtID, objDetail.SiteID, OldQty, 0, 2);                     
                    }
                }
                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Save, new { pONbr = _ponbr });

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
                _form = data;
                _ponbr = data["cboPONbr"];
                _branchID = data["cboBranchID"];
                var detHeader = new StoreDataHandler(data["lstHeader"]);
                if (_poHead == null)
                    _poHead = detHeader.ObjectData<PO_Header>().FirstOrDefault();
                var objHeader = _db.PO_Header.Where(p => p.BranchID == _branchID && p.PONbr == _ponbr).FirstOrDefault();
                if (objHeader == null)
                {
                }
                else
                {

                    if (_poHead.tstamp.ToHex() != objHeader.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    
                    _db.PO_Header.DeleteObject(objHeader);
                    var lstdel=_db.PO_Detail.Where(p => p.PONbr == _ponbr && p.BranchID == _branchID).ToList();
                    while (lstdel.FirstOrDefault() != null)
                    {
                        var objDetail = lstdel.FirstOrDefault();
                        if (objDetail != null)
                        {
                            if (objDetail.PurchaseType == "GI" || objDetail.PurchaseType == "PR" || objDetail.PurchaseType == "GS")
                            {

                                double OldQty = Math.Round((objDetail.UnitMultDiv == "D" ? ((objDetail.QtyOrd - objDetail.QtyRcvd) / objDetail.CnvFact) : (objDetail.QtyOrd - objDetail.QtyRcvd) * objDetail.CnvFact));
                                //UpdateOnPOQty(objDetail.InvtID, objDetail.SiteID, OldQty, 0, 2);
                                var objIN_ItemSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == objDetail.InvtID && p.SiteID == objDetail.SiteID);
                                objIN_ItemSite.QtyOnPO = Math.Round(objIN_ItemSite.QtyOnPO  - OldQty, 2);
                            }
                        }
                      
                        _db.PO_Detail.DeleteObject(lstdel.FirstOrDefault());
                        lstdel.Remove(lstdel.FirstOrDefault());
                    }
                }
                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Delete, new { pONbr = _ponbr });
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
                _form = data;
                _ponbr = data["cboPONbr"];
                _branchID = data["cboBranchID"];
                _status = data["Status"].PassNull();
                _toStatus = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();
                DateTime dpoDate = data["PODate"].ToDateShort();


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO_Header>().FirstOrDefault();


                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPODetailLoad = detHandler.ObjectData<PO10100_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstDel"]);
                ChangeRecords<PO10100_pgDetail_Result> lst = dataHandler.BatchObjectData<PO10100_pgDetail_Result>();

                if (_poHead == null)
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                else
                {
                    foreach (PO10100_pgDetail_Result deleted in lst.Deleted)
                    {
                        var obj1 = _db.PO_Detail.Where(p => p.BranchID == deleted.BranchID && p.PONbr == deleted.PONbr && p.LineRef == deleted.LineRef).FirstOrDefault();
                        if (obj1 != null)
                        {
                            if (deleted.PurchaseType == "GI" || deleted.PurchaseType == "PR" || deleted.PurchaseType == "GP" || deleted.PurchaseType == "GS")
                            {
                                double OldQty = Math.Round((obj1.UnitMultDiv == "D" ? ((obj1.QtyOrd - obj1.QtyRcvd) / obj1.CnvFact) : (obj1.QtyOrd - obj1.QtyRcvd) * obj1.CnvFact));
                                var objIN_ItemSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == obj1.InvtID && p.SiteID == obj1.SiteID);
                                objIN_ItemSite.QtyOnPO = Math.Round(objIN_ItemSite.QtyOnPO  - OldQty, 2);
                               // UpdateOnPOQty(obj1.InvtID, obj1.SiteID, OldQty, 0, 2);
                            }
                            _db.PO_Detail.DeleteObject(obj1);
                        }
                    }
                    Save_PO_Header(true);
                }
                return Util.CreateMessage(MessageProcess.Delete, new { pONbr = _ponbr });
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
        public ActionResult PO10100POPrice(string branchID="",string invtID="",string Unit="",DateTime? podate=null)
        {
            var result=_db.PO10100_ppGetPrice(branchID, invtID, Unit, podate).FirstOrDefault().Value;
            return this.Direct(result);
           
        }
        [DirectMethod]
        public ActionResult PO10100ItemSitePrice(string branchID = "", string invtID = "", string siteID = "")
        {
            var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == invtID && p.SiteID == siteID).FirstOrDefault();

            return this.Direct(objIN_ItemSite);

        }       
           
        private void Save_PO_Header(bool isDeleteGrd=false)
        {
           // _lstIN_ItemSite = new List<IN_ItemSite>();
            _lstPO10100_pdOM_DiscAllByBranchPO = _db.PO10100_pdOM_DiscAllByBranchPO(_branchID).ToList();
            _PO10100_pdIN_UnitConversion_Result = _db.PO10100_pdIN_UnitConversion().ToList();
            objOM_UserDefault = _db.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID.Trim().ToUpper() == _branchID.Trim().ToUpper() && p.UserID.Trim().ToUpper() == Current.UserName.Trim().ToUpper());
            _objPO_Setup = _db.PO_Setup.FirstOrDefault(p => p.BranchID == _branchID && p.SetupID == "PO");
            var obj = _db.PO_Header.FirstOrDefault(p => p.PONbr == _ponbr && p.BranchID == _branchID);
            if (Data_Checking(isDeleteGrd))
            {
                
                if (obj != null)
                {

                    if (obj.tstamp.ToHex() != _poHead.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    UpdatingPO_Header(ref obj, _poHead, _lstPODetailLoad);
                    Save_PO_Detail(obj, _lstPODetailLoad);
                }
                else
                {
                    if (_objPO_Setup.AutoRef == 1)
                    {
                        obj = new PO_Header();
                        UpdatingPO_Header(ref obj, _poHead, _lstPODetailLoad);

                        var obj1 = _db.PONumbering(_branchID, "PONbr").FirstOrDefault();
                        _branchID = obj.BranchID = _branchID;
                        _ponbr = obj.PONbr = obj1;
                        obj.IsExport = false;
                        obj.ImpExp = "";

                        obj.Crtd_DateTime = DateTime.Now;
                        obj.Crtd_Prog = ScreenNbr;
                        obj.Crtd_User = Current.UserName;

                        _db.PO_Header.AddObject(obj);
                        Save_PO_Detail(obj, _lstPODetailLoad);
                    }
                }
            }
        }        
        private void Save_PO_Detail(PO_Header header, List<PO10100_pgDetail_Result> lst)
        {
            int i = 0;
            try
            {
                for (i = 0; i < lst.Count; i++)
                {
                    PO10100_pgDetail_Result objDetail = lst[i];
                    var objPO_Detail = _db.PO_Detail.Where(p => p.BranchID == header.BranchID && p.PONbr == header.PONbr && p.LineRef == objDetail.LineRef).FirstOrDefault();
                    if (objPO_Detail == null)
                    {
                        objPO_Detail = new PO_Detail();
                        objPO_Detail.ResetET();

                        UpdatingPO_Detail(objDetail, ref objPO_Detail,true);
                        objPO_Detail.Crtd_DateTime = DateTime.Now;
                        objPO_Detail.Crtd_Prog = ScreenNbr;
                        objPO_Detail.Crtd_User = Current.UserName;
                        objPO_Detail.ReqdDate = objPO_Detail.ReqdDate.ToDateShort();

                        _db.PO_Detail.AddObject(objPO_Detail);
                    }
                    else
                    {
                        if (objPO_Detail.tstamp.ToHex() != objDetail.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        UpdatingPO_Detail(objDetail, ref objPO_Detail,false);
                    }
                }
                _db.SaveChanges();
                if (_toStatus != _status) SendMail(header);
                

            }
            catch (Exception ex)
            {
                throw (ex);              
            }
        }        
        private void UpdatingPO_Header(ref PO_Header objHeader, PO_Header _poHead, List<PO10100_pgDetail_Result> lst)
        {

            try
            {
                objHeader.VouchStage =_poHead.VouchStage.PassNull();
                objHeader.POAmt = _poHead.POAmt.ToDouble();
                //objHeader.RcptTotAmt = _poHead.RcptTotAmt.ToDouble();
                objHeader.POFeeTot = lst.Sum(p => p.POFee);

                //tap main
                objHeader.ReqNbr = _poHead.ReqNbr.PassNull();
                objHeader.VendID = _poHead.VendID.PassNull();
                objHeader.NoteID = 0;
                objHeader.Status = _poHead.Status.PassNull();
                objHeader.POType = _poHead.POType.PassNull();
                objHeader.BlktPONbr = _poHead.BlktPONbr.PassNull();
                //tap shipping Information
                objHeader.ShipDistAddrID = _poHead.ShipDistAddrID.PassNull();
                objHeader.ShiptoType = _poHead.ShiptoType.PassNull();
                objHeader.ShipSiteID = _poHead.ShipSiteID.PassNull();
                objHeader.ShipCustID = _poHead.ShipCustID.PassNull();
                objHeader.ShiptoID = _poHead.ShiptoID.PassNull();
                objHeader.ShipVendID = _poHead.ShipVendID.PassNull();
                objHeader.ShipVendAddrID = _poHead.ShipVendAddrID.PassNull();
                objHeader.ShipAddrID = _poHead.ShipAddrID.PassNull();
                objHeader.ShipName = _poHead.ShipName.PassNull();
                objHeader.ShipAttn = _poHead.ShipAttn.PassNull();
                objHeader.ShipAddr1 = _poHead.ShipAddr1.PassNull();
                objHeader.ShipAddr2 = _poHead.ShipAddr2.PassNull();
                objHeader.ShipCity = _poHead.ShipCity.PassNull();
                objHeader.ShipState = _poHead.ShipState.PassNull();
                objHeader.ShipZip = _poHead.ShipZip.PassNull();
                objHeader.ShipCountry = _poHead.ShipCountry.PassNull();
                objHeader.ShipPhone = _poHead.ShipPhone.PassNull();
                objHeader.ShipFax = _poHead.ShipFax.PassNull();
                objHeader.ShipEmail = _poHead.ShipEmail.PassNull();
                objHeader.ShipVia = _poHead.ShipVia.PassNull();
                //tap vendor Information
                objHeader.VendAddrID = _poHead.VendAddrID.PassNull();
                objHeader.VendName = _poHead.VendName.PassNull();
                objHeader.VendAttn = _poHead.VendAttn.PassNull();
                objHeader.VendAddr1 = _poHead.VendAddr1.PassNull();
                objHeader.VendAddr2 = _poHead.VendAddr2.PassNull();
                objHeader.VendCity = _poHead.VendCity.PassNull();
                objHeader.VendState = _poHead.VendState.PassNull();
                objHeader.VendZip = _poHead.VendZip.PassNull();
                objHeader.VendCountry = _poHead.VendCountry.PassNull();
                objHeader.VendPhone = _poHead.VendPhone.PassNull();
                objHeader.VendFax = _poHead.VendFax.PassNull();
                objHeader.VendEmail = _poHead.VendEmail.PassNull();
                //tap Other Information            
                objHeader.PODate = _poHead.PODate;
                objHeader.BlktExprDate = _poHead.BlktExprDate;
                objHeader.RcptStage = _poHead.RcptStage.PassNull();
                objHeader.Terms = _poHead.Terms.PassNull();
                objHeader.Buyer = _poHead.Buyer.PassNull();
                objHeader.Status = _toStatus;
                objHeader.SlsperID = _poHead.SlsperID.PassNull();

                objHeader.LUpd_DateTime = DateTime.Now;
                objHeader.LUpd_Prog = ScreenNbr;
                objHeader.LUpd_User = Current.UserName;
                objHeader.tstamp = new byte[0];
             
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void UpdatingPO_Detail(PO10100_pgDetail_Result objDetail, ref PO_Detail objrPO_Detail,bool isnew)
        {
            double OldQty = 0;
            double NewQty = 0;

            IN_ItemSite objIN_ItemSite = new IN_ItemSite();
            try
            {
                if (objDetail.PurchaseType == "GI" || objDetail.PurchaseType == "PR" || objDetail.PurchaseType == "GP" || objDetail.PurchaseType == "GS")
                {
                    var objIN_Inventory = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == objDetail.InvtID);
                    if (objIN_Inventory == null)
                    {
                        objIN_Inventory = new IN_Inventory();
                        objIN_Inventory.ResetET();
                        objIN_Inventory.InvtID = objDetail.InvtID;
                    }
                 
                    try
                    {
                        objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == objDetail.InvtID && p.SiteID == objDetail.SiteID).FirstOrDefault();
                        if (objIN_ItemSite == null)// && _lstIN_ItemSite.Where(p => p.InvtID == objDetail.InvtID && p.SiteID == objDetail.SiteID).FirstOrDefault()==null)
                        {
                            objIN_ItemSite = new IN_ItemSite();
                            objIN_ItemSite.ResetET();
                            NewQty = Math.Round((objDetail.UnitMultDiv == "D" ? (objDetail.QtyOrd / objDetail.CnvFact) : (objDetail.QtyOrd * objDetail.CnvFact)));

                            objIN_ItemSite.QtyOnPO = Math.Round(NewQty, 2);

                            Insert_IN_ItemSite(ref objIN_ItemSite, ref objIN_Inventory, objDetail.SiteID);
                        }
                        else
                        {
                            NewQty = Math.Round((objDetail.UnitMultDiv == "D" ? (objDetail.QtyOrd / objDetail.CnvFact) : (objDetail.QtyOrd * objDetail.CnvFact)));
                            if (isnew) OldQty = 0;
                            else
                                OldQty = Math.Round((objrPO_Detail.UnitMultDiv == "D" ? (objrPO_Detail.QtyOrd / objrPO_Detail.CnvFact) : objrPO_Detail.QtyOrd * objrPO_Detail.CnvFact));

                            objIN_ItemSite.QtyOnPO = Math.Round(objIN_ItemSite.QtyOnPO + NewQty - OldQty, 2);
                        }
                   
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }


                }
               
                objrPO_Detail.BranchID = _branchID;
                objrPO_Detail.PONbr = _ponbr;
                objrPO_Detail.LineRef = objDetail.LineRef;

                objrPO_Detail.BlktLineID = objDetail.BlktLineID;
                objrPO_Detail.BlktLineRef = objDetail.BlktLineRef == null ? "" : objDetail.BlktLineRef;
                objrPO_Detail.CnvFact = (objDetail.CnvFact == 0 ? 1 : objDetail.CnvFact);

                objrPO_Detail.CostReceived =objDetail.CostReceived;
                objrPO_Detail.CostReturned =objDetail.CostReturned;
                objrPO_Detail.CostVouched = objDetail.CostVouched;
                objrPO_Detail.ExtCost = objDetail.ExtCost;
                objrPO_Detail.POFee = objDetail.POFee;
                objrPO_Detail.UnitCost = objDetail.UnitCost;

                objrPO_Detail.ExtWeight = objDetail.ExtWeight;
                objrPO_Detail.ExtVolume = objDetail.ExtVolume;
                objrPO_Detail.InvtID = objDetail.InvtID;
                objrPO_Detail.PromDate = objDetail.PromDate.ToDateShort();
                objrPO_Detail.PurchaseType = objDetail.PurchaseType;
                objrPO_Detail.PurchUnit = objDetail.PurchUnit;

                objrPO_Detail.QtyOrd = objDetail.QtyOrd;
                objrPO_Detail.QtyRcvd = objDetail.QtyRcvd;
                objrPO_Detail.QtyReturned = objDetail.QtyReturned;
                objrPO_Detail.QtyVouched = objDetail.QtyVouched;

                objrPO_Detail.RcptStage = objDetail.RcptStage;
                objrPO_Detail.ReasonCd = objDetail.ReasonCd;
                objrPO_Detail.ReqdDate = objDetail.ReqdDate.ToDateShort();
                objrPO_Detail.SiteID = objDetail.SiteID;
                objrPO_Detail.TaxCat = objDetail.TaxCat;
                objrPO_Detail.TranDesc = objDetail.TranDesc;
                
                objrPO_Detail.TaxID00 = objDetail.TaxID00;
                objrPO_Detail.TaxID01 = objDetail.TaxID01;
                objrPO_Detail.TaxID02 = objDetail.TaxID02;
                objrPO_Detail.TaxID03 = objDetail.TaxID03;
                
                objrPO_Detail.TxblAmt00 = objDetail.TxblAmt00;
                objrPO_Detail.TxblAmt01 = objDetail.TxblAmt01;
                objrPO_Detail.TxblAmt02 = objDetail.TxblAmt02;
                objrPO_Detail.TxblAmt03 =objDetail.TxblAmt03;

                objrPO_Detail.TaxAmt00 = objDetail.TaxAmt00;
                objrPO_Detail.TaxAmt01 = objDetail.TaxAmt01;
                objrPO_Detail.TaxAmt02 = objDetail.TaxAmt02;
                objrPO_Detail.TaxAmt03 = objDetail.TaxAmt03;

                objrPO_Detail.DiscAmt = objDetail.DiscAmt;
                objrPO_Detail.DiscID = objDetail.DiscID;
                objrPO_Detail.DiscPct = objDetail.DiscPct;
                objrPO_Detail.DiscSeq = objDetail.DiscSeq;
                objrPO_Detail.DiscLineRef = objDetail.DiscLineRef;

                objrPO_Detail.UnitMultDiv = objDetail.UnitMultDiv;
                objrPO_Detail.UnitWeight = objDetail.UnitWeight;
                objrPO_Detail.UnitVolume = objDetail.UnitVolume;
                objrPO_Detail.VouchStage = objDetail.VouchStage;
                objrPO_Detail.LUpd_DateTime = DateTime.Now;
                objrPO_Detail.LUpd_Prog =ScreenNbr;
                objrPO_Detail.LUpd_User = Current.UserName;
              
            }
            catch (Exception ex)
            {

                throw (ex);

            }
        }                
        private void SendMail(PO_Header objHeader)
        {
            try
            {
                HQSendMailApprove.Approve.SendMailApprove(objHeader.BranchID, objHeader.PONbr, ScreenNbr, Current.CpnyID, _status, _toStatus, Current.UserName, Current.LangID);
            }
            catch
            {
            }
        }
        private bool Data_Checking(bool isDeleteGrd=false)
        {


            if (_objPO_Setup == null)
            {
                throw new MessageException(MessageType.Message, "20404",
                  parm: new[] { "PO_Setup" });

            }

            if (_poHead.PONbr.PassNull() == "" && _objPO_Setup.AutoRef == 0)
            {
                throw new MessageException(MessageType.Message, "15",
                 parm: new[] { Util.GetLang("PONbr") });

            }
            if (_poHead.VendID.PassNull() == "")
            {
                throw new MessageException(MessageType.Message, "15",
                parm: new[] { Util.GetLang("VendID") });

            }
            if (_poHead.ShipDistAddrID.PassNull() == "")
            {
                throw new MessageException(MessageType.Message, "15",
              parm: new[] { Util.GetLang("DistAddr") });

            }
            //Invalid data
            if (_poHead.POType.PassNull() == "" || _poHead.Status.PassNull() == "")
            {
                throw new MessageException(MessageType.Message, "744");

            }
            //Check PO has no detail data
            if (_lstPODetailLoad.Count == 0)
            {
                throw new MessageException(MessageType.Message, "704");


            }
            if (_lstPODetailLoad.Where(p => string.IsNullOrEmpty(p.PurchUnit.PassNull()) && !string.IsNullOrEmpty(p.InvtID.PassNull())).Count() > 0)
            {

                throw new MessageException(MessageType.Message, "25");

            }
            if (_db.PO10100_ppCheckPODate(_branchID, _poHead.PODate.ToDateShort(), ScreenNbr, _ponbr).FirstOrDefault() == "0")
                throw new MessageException(MessageType.Message, "201302041",
                        parm: new[] { _poHead.PODate.ToShortDateString() });

            //Check MOQ
            AP_Vendor objVendor = new AP_Vendor();
            objVendor = _db.AP_Vendor.ToList().FirstOrDefault(p => p.VendID == _poHead.VendID.PassNull());
         
            if (objVendor.MOQVal > 0)
            {
                switch (objVendor.MOQType)
                {
                    case "Q":
                        if (_lstPODetailLoad.Sum(p => p.QtyOrd * p.CnvFact) < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                parm: new[] { Util.GetLang("Quantity"), objVendor.MOQVal.ToString() });



                        }
                        break;
                    case "V":
                        if (_lstPODetailLoad.Sum(p => p.ExtVolume) < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                  parm: new[] { Util.GetLang("TotVol"), objVendor.MOQVal.ToString() + "L" });

                        }
                        break;
                    case "A":
                        if (_poHead.POAmt < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                  parm: new[] { Util.GetLang("POAmt"), objVendor.MOQVal.ToString() });


                        }
                        break;
                    case "W":
                        if (_lstPODetailLoad.Sum(p => p.ExtWeight) < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                  parm: new[] { Util.GetLang("TotWeight"), objVendor.MOQVal.ToString() + "KG" });




                        }
                        break;
                }
            }

            if (_db.PO10100_ppCheckCloseDate(_branchID, _poHead.PODate.ToDateShort(), ScreenNbr).FirstOrDefault() == "0")
                throw new MessageException(MessageType.Message, "301");


            for (Int16 i = 0; i <= _lstPODetailLoad.Count - 1; i++)
            {
                var objDetail = _lstPODetailLoad[i];

                if (objDetail.PurchaseType != "PR" && objDetail.ExtCost == 0)
                {
                    throw new MessageException(MessageType.Message, "44");
                }
                if (string.IsNullOrEmpty(objDetail.PurchUnit))
                {
                    throw new MessageException(MessageType.Message, "15",
                 parm: new[] { Util.GetLang("PurchUnit") });

                }
                if (string.IsNullOrEmpty(objDetail.SiteID))
                {
                    throw new MessageException(MessageType.Message, "15",
                    parm: new[] { Util.GetLang("SiteID") });
                }
                if (string.IsNullOrEmpty(objDetail.PurchaseType))
                {
                    throw new MessageException(MessageType.Message, "15",
                    parm: new[] { Util.GetLang("PurchaseType") });
                }
            }
            string strInvt = "";
            var query = (from f in _lstPODetailLoad
                         where f.PurchaseType != "PR"
                         //join l in lstPO10100_pgDetail on f.InvtID equals l.InvtID
                         group f by f.InvtID into g
                         let count = g.Count()
                         where count > 1
                         orderby count descending
                         select new CountInvtID
                         {
                             invtID = g.First().InvtID,
                             count = count
                         }).ToList();
            foreach (var obj in query)
            {
                strInvt += obj.invtID + ",";
            }
            if (strInvt != "")
            {
                throw new MessageException(MessageType.Message, "20417",
                    parm: new[] { strInvt.TrimEnd(',') });

            }

            var lstremove = _lstPODetailLoad.Where(p => p.PurchaseType == "PR").ToList();
            while (_lstPODetailLoad.Where(p => p.PurchaseType == "PR" && (!string.IsNullOrEmpty(p.DiscLineRef)) && (p.DiscLineRef != p.LineRef)).Count() > 0)
            {
                var obj1 = _lstPODetailLoad.FirstOrDefault(p => p.PurchaseType == "PR" && (!string.IsNullOrEmpty(p.DiscLineRef)) && (p.DiscLineRef != p.LineRef));
                var obj = obj1;
                var objD = _db.PO_Detail.FirstOrDefault(p => p.PONbr == obj.PONbr && p.LineRef == obj.LineRef && p.BranchID == obj.BranchID && p.PurchaseType == "PR" && p.DiscLineRef == obj.DiscLineRef);
                if (objD != null) _db.PO_Detail.DeleteObject(objD);
                _lstPODetailLoad.Remove(obj1);

            }
            for (Int16 i = 0; i <= _lstPODetailLoad.Count - 1; i++)
            {
                var objDetail = _lstPODetailLoad[i];

                if (objDetail.PurchaseType != "PR" && objDetail.ExtCost == 0)
                {
                    throw new MessageException(MessageType.Message, "44");
                }
                if (objDetail.PurchaseType != "PR" && objDetail.DiscAmt == 0)
                {

                    _lineRef = LastLineRef(_lstPODetailLoad);
                    double DiscAmt = 0;
                    double DiscPct = 0;
                    string discSeq = string.Empty;
                    string discID = string.Empty;
                    string budgetID = string.Empty;
                    string breaklineRef = string.Empty;
                    GetDiscLineSetup(objDetail, objDetail.QtyOrd, objDetail.ExtCost, ref DiscAmt, ref DiscPct, ref discSeq, ref discID, ref budgetID, ref breaklineRef);
                    objDetail.DiscAmt = DiscAmt;
                    objDetail.DiscPct = DiscPct;
                    objDetail.DiscSeq = discSeq;
                    objDetail.DiscID = discID;
                    objDetail.ExtCost = objDetail.QtyOrd * objDetail.UnitCost - objDetail.DiscAmt;

                }
            }
            FreeItemForLine(ref _lineRef, "");
            return true;
        }                     
        #endregion
        #region import,export, report     
        [HttpPost]
        public ActionResult Export(FormCollection data, string type)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetPOSuggest = workbook.Worksheets[0];
                SheetPOSuggest.Name = Util.GetLang("POSuggest");


                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(data["cboBranchID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PODate", DbType.DateTime, clsCommon.GetValueDBNull(data["PODate"].ToDateShort()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@User", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@VendID", DbType.String, clsCommon.GetValueDBNull(data["VendID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(type), ParameterDirection.Input, 30));
                DataTable dtInvtID = dal.ExecDataTable("PO10100_peInventory", CommandType.StoredProcedure, ref pc);
                SheetPOSuggest.Cells.ImportDataTable(dtInvtID, true, "AA1");// du lieu Inventory

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(data["cboBranchID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PODate", DbType.DateTime, clsCommon.GetValueDBNull(data["PODate"].ToDateShort()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@VendID", DbType.String, clsCommon.GetValueDBNull(data["VendID"].PassNull()), ParameterDirection.Input, 30));
                DataTable dtHeader = dal.ExecDataTable("PO10100_peHeader", CommandType.StoredProcedure, ref pc);

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(data["cboBranchID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PODate", DbType.DateTime, clsCommon.GetValueDBNull(data["PODate"].ToDateShort()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@User", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@VendID", DbType.String, clsCommon.GetValueDBNull(data["VendID"].PassNull()), ParameterDirection.Input, 30));
                DataTable dtData = dal.ExecDataTable("PO10100_peSuggest", CommandType.StoredProcedure, ref pc);


             

                

                Style style = workbook.GetStyleInPool(0);                        
                StyleFlag flag = new StyleFlag();
                Range range;
                Cell cell;
                //LOCK TRUE

                style = workbook.GetStyleInPool(0);
                style.IsLocked = true;
                range = SheetPOSuggest.Cells.CreateRange("A1", "ZZ" + dtInvtID.Rows.Count + 7);
                range.SetStyle(style);
               
                #region template
            
                SetCellValueHeader(SheetPOSuggest.Cells["B1"], Util.GetLang("PO10100EHeader"), TextAlignmentType.Center, TextAlignmentType.Center);
                SheetPOSuggest.Cells.Merge(0, 1, 1, 6);

                SetCellValueHeader(SheetPOSuggest.Cells["B2"],  Util.GetLang("PO10100VendName"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["B3"],  Util.GetLang("PO10100BranchID"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["B4"],  Util.GetLang("PO10100BranchName"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["B5"],  Util.GetLang("PO10100SNDD"), TextAlignmentType.Center, TextAlignmentType.Right);

                SetCellValueHeader(SheetPOSuggest.Cells["C2"], dtHeader.Rows[0]["VendName"].ToString(), TextAlignmentType.Center, TextAlignmentType.Left);
                SheetPOSuggest.Cells.Merge(1, 2, 1, 2);

                SetCellValueHeader(SheetPOSuggest.Cells["C3"], dtHeader.Rows[0]["CpnyID"].ToString(), TextAlignmentType.Center, TextAlignmentType.Left);
                SheetPOSuggest.Cells.Merge(2, 2, 1, 2);

                SetCellValueHeader(SheetPOSuggest.Cells["C4"], dtHeader.Rows[0]["CpnyName"].ToString(), TextAlignmentType.Center, TextAlignmentType.Left);
                SheetPOSuggest.Cells.Merge(3, 2, 1, 2);

                SetCellValueHeader(SheetPOSuggest.Cells["C5"], dtHeader.Rows[0]["SNDD"].ToString(), TextAlignmentType.Center, TextAlignmentType.Right);
               


                SetCellValueHeader(SheetPOSuggest.Cells["E2"], Util.GetLang("PO10100PODate"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["E3"], Util.GetLang("PO10100TotAmtExcel"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["E4"], Util.GetLang("PO10100MinValue"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["E5"], Util.GetLang("PO10100NXK"), TextAlignmentType.Center, TextAlignmentType.Right);

                SetCellValueHeader(SheetPOSuggest.Cells["F2"], data["PODate"].ToDateShort().ToString("dd-MM-yyyy"), TextAlignmentType.Center, TextAlignmentType.Left);

                SetCellValueHeader(SheetPOSuggest.Cells["F3"], "0", TextAlignmentType.Center, TextAlignmentType.Right);
              
                SetCellValueHeader(SheetPOSuggest.Cells["F4"], dtHeader.Rows[0]["MinValue"].ToString(), TextAlignmentType.Center, TextAlignmentType.Right);
              
                SetCellValueHeader(SheetPOSuggest.Cells["F5"], Convert.ToDateTime(dtHeader.Rows[0]["NXK"]).ToString("dd-MM-yyyy"), TextAlignmentType.Center, TextAlignmentType.Left);
               

                SetCellValueHeader(SheetPOSuggest.Cells["G2"], Util.GetLang("PO10100CreditLimit"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["G3"], Util.GetLang("PO10100Deposit"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["G4"], Util.GetLang("PO10100MaxValue"), TextAlignmentType.Center, TextAlignmentType.Right);
                SetCellValueHeader(SheetPOSuggest.Cells["G5"], Util.GetLang("PO10100CountShip"), TextAlignmentType.Center, TextAlignmentType.Right);

                SetCellValueHeader(SheetPOSuggest.Cells["H2"], dtHeader.Rows[0]["CreditLimit"].ToString(), TextAlignmentType.Center, TextAlignmentType.Right);
              
                SetCellValueHeader(SheetPOSuggest.Cells["H3"], dtHeader.Rows[0]["Deposit"].ToString(), TextAlignmentType.Center, TextAlignmentType.Right);
              
                SetCellValueHeader(SheetPOSuggest.Cells["H4"], dtHeader.Rows[0]["MaxValue"].ToString(), TextAlignmentType.Center, TextAlignmentType.Right);
               
                SetCellValueHeader(SheetPOSuggest.Cells["H5"], dtHeader.Rows[0]["CountShipment"].ToString(), TextAlignmentType.Center, TextAlignmentType.Right);
              


                
                //SheetPOSuggest.Range["A6:N6"].Interior.Color = Color.Yellow;

                SetCellValueHeader(SheetPOSuggest.Cells["A6"], Util.GetLang("PO10100ESTT"), TextAlignmentType.Center, TextAlignmentType.Center);                
                SetCellValueHeader(SheetPOSuggest.Cells["B6"], Util.GetLang("PO10100InvtID"), TextAlignmentType.Center, TextAlignmentType.Center);             
                SetCellValueHeader(SheetPOSuggest.Cells["C6"], Util.GetLang("PO10100Descr"), TextAlignmentType.Center, TextAlignmentType.Center);               
                SetCellValueHeader(SheetPOSuggest.Cells["D6"], Util.GetLang("PO10100PurchUnit"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["E6"], Util.GetLang("PO10100UnitPrice"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["N6"], Util.GetLang("SLConlai"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["G6"], Util.GetLang("SLDathang"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["H6"], Util.GetLang("ThanhTien"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["I6"], Util.GetLang("TKchuan"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["J6"], Util.GetLang("TKHientai"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["K6"], Util.GetLang("TKCuoithang"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["L6"], Util.GetLang("TBSellout"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["M6"], Util.GetLang("SLChitieu"), TextAlignmentType.Center, TextAlignmentType.Center);
                SetCellValueHeader(SheetPOSuggest.Cells["F6"], Util.GetLang("SLDenghi"), TextAlignmentType.Center, TextAlignmentType.Center);

               

            #endregion
               
                #region formular
               
    

                Validation validation = SheetPOSuggest.Validations[SheetPOSuggest.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=$AA$2:$AA$" + (dtInvtID.Rows.Count + 2);
                validation.ShowError = true;
                validation.AlertStyle = ValidationAlertType.Stop;
                validation.ErrorTitle = "Error";
                validation.InputMessage = "Chọn mã mặt hàng";
                validation.ErrorMessage = "Mã mặt hàng này không tồn tại";

                CellArea area;
                area.StartRow = 6;
                area.EndRow = dtInvtID.Rows.Count + 7;
                area.StartColumn = 1;
                area.EndColumn = 1;

                validation.AddArea(area);



                String formulaSTT = "=IF(ISERROR(IF(C7<>\"\",A6+1 & \"\",\"\")),1,IF(C7<>\"\",A6+1 & \"\",\"\"))";
                SheetPOSuggest.Cells["A7"].SetSharedFormula(formulaSTT, (dtInvtID.Rows.Count + 7), 1);
               

                String formulaDesctInvtID = string.Format("=IF(ISERROR(VLOOKUP(B7,$AA:$AD,2,0)),\"\",VLOOKUP(B7,$AA:$AD,2,0))");
                SheetPOSuggest.Cells["C7"].SetSharedFormula(formulaDesctInvtID, (dtInvtID.Rows.Count + 7), 1);

                String formulaUnit = string.Format("=IF(ISERROR(VLOOKUP(B7,$AA:$AD,3,0)),\"\",VLOOKUP(B7,$AA:$AD,3,0))");
                SheetPOSuggest.Cells["D7"].SetSharedFormula(formulaUnit, (dtInvtID.Rows.Count + 7), 1);

              
                String formulaPrice = string.Format("=IF(ISERROR(VLOOKUP(B7,$AA:$AD,4,0)),0,VLOOKUP(B7,$AA:$AD,4,0))");
                SheetPOSuggest.Cells["E7"].SetSharedFormula(formulaPrice, (dtInvtID.Rows.Count + 7), 1);
              
                String formulaTypeInvtID = string.Format("=IF(ISERROR(VLOOKUP(B7,$AA:$AE,5,0)),\"\",VLOOKUP(B7,$AA:$AE,5,0))");
                SheetPOSuggest.Cells["O7"].SetSharedFormula(formulaTypeInvtID, (dtInvtID.Rows.Count + 7), 1);
              
                String formulaZERO = string.Format("=IF(ISERROR(VLOOKUP(B7,$AA:$AD,5,0)),0,VLOOKUP(B7,$AA:$AD,5,0))");
                SheetPOSuggest.Cells["G7"].SetSharedFormula(formulaZERO, (dtInvtID.Rows.Count + 7), 1);
              
               
                String formulachuan = string.Format("=IF(ISERROR(VLOOKUP(B7,AF:$AQ,5,0)),0,VLOOKUP(B7,$AF:$AQ,5,0))");
                SheetPOSuggest.Cells["I7"].SetSharedFormula(formulachuan, (dtInvtID.Rows.Count + 7), 1);
              

                String formulahientai = string.Format("=IF(ISERROR(VLOOKUP(B7,AF:$AQ,6,0)),0,VLOOKUP(B7,$AF:$AQ,6,0))");
                SheetPOSuggest.Cells["J7"].SetSharedFormula(formulahientai, (dtInvtID.Rows.Count + 7), 1);
           

                String formulacth = string.Format("=IF(ISERROR(VLOOKUP(B7,AF:$AQ,7,0)),0,VLOOKUP(B7,$AF:$AQ,7,0))");
                SheetPOSuggest.Cells["K7"].SetSharedFormula(formulacth, (dtInvtID.Rows.Count + 7), 1);
            

                String formulaSellout = string.Format("=IF(ISERROR(VLOOKUP(B7,AF:$AQ,12,0)),0,VLOOKUP(B7,$AF:$AQ,12,0))");
                SheetPOSuggest.Cells["L7"].SetSharedFormula(formulaSellout, (dtInvtID.Rows.Count + 7), 1);
              

                String formulact = string.Format("=IF(ISERROR(VLOOKUP(B7,AF:$AQ,8,0)),0,VLOOKUP(B7,$AF:$AQ,8,0))");
                SheetPOSuggest.Cells["M7"].SetSharedFormula(formulact, (dtInvtID.Rows.Count + 7), 1);
               

                String formuladn = string.Format("=IF(ISERROR(VLOOKUP(B7,AF:$AQ,9,0)),0,VLOOKUP(B7,$AF:$AQ,9,0))");
                SheetPOSuggest.Cells["F7"].SetSharedFormula(formuladn, (dtInvtID.Rows.Count + 7), 1);
                

                String formulacl = string.Format("=IF(ISERROR(VLOOKUP(B7,AF:$AQ,10,0)),0,VLOOKUP(B7,$AF:$AQ,10,0))");
                SheetPOSuggest.Cells["N7"].SetSharedFormula(formulacl, (dtInvtID.Rows.Count + 7), 1);

                String formulaTB = string.Format("=IF(OR({0}={1},{2}={3}),{4},ROUND({5}*{6},0))", "E7", "\"\"", "G7", "\"\"", "\"\"", "E7", "G7");
                SheetPOSuggest.Cells["H7"].SetSharedFormula(formulaTB, (dtInvtID.Rows.Count + 7), 1);
               

                String formulaTot = string.Format("=SUM({0}:{1})", "H7", "H" + (dtInvtID.Rows.Count + 7));
                SheetPOSuggest.Cells["F3"].SetSharedFormula(formulaTot, 2, 1);
             
                #endregion
                
                #region truyen du lieu
                int counter = 7;
                int countPO = 2;
                ////Instantiating a "Products" DataTable object
                //DataTable dtSuggest = new DataTable("Suggest");

                ////Adding columns to the DataTable object
                cell = SheetPOSuggest.Cells["AF1"];
                cell.PutValue("InvtID");

                cell = SheetPOSuggest.Cells["AG1"];
                cell.PutValue("Descr");

                cell = SheetPOSuggest.Cells["AH1"];
                cell.PutValue("PurchUnit");

                cell = SheetPOSuggest.Cells["AI1"];
                cell.PutValue("UnitPrice");

                cell = SheetPOSuggest.Cells["AJ1"];
                cell.PutValue("TKchuan");

                cell = SheetPOSuggest.Cells["AK1"];
                cell.PutValue("TKHientai");

                cell = SheetPOSuggest.Cells["AL1"];
                cell.PutValue("TKCuoithang");

                cell = SheetPOSuggest.Cells["AM1"];
                cell.PutValue("SLChitieu");

                cell = SheetPOSuggest.Cells["AN1"];
                cell.PutValue("SLDenghi");

                cell = SheetPOSuggest.Cells["AO1"];
                cell.PutValue("SLConlai");

                cell = SheetPOSuggest.Cells["AP1"];
                cell.PutValue("SLDathang");

                cell = SheetPOSuggest.Cells["AQ1"];
                cell.PutValue("TBSellout");

              
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    cell = SheetPOSuggest.Cells["B"+ counter.ToString()];
                    cell.PutValue(dtData.Rows[i]["InvtID"].ToString());
                    ////Creating an empty row in the DataTable object
                    //DataRow dr = dtSuggest.NewRow();

                    cell = SheetPOSuggest.Cells["AF" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["InvtID"].ToString());

                    cell = SheetPOSuggest.Cells["AG" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["Descr"].ToString());

                    cell = SheetPOSuggest.Cells["AH" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["PurchUnit"].ToString());

                    cell = SheetPOSuggest.Cells["AI" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["UnitPrice"].ToString());

                    cell = SheetPOSuggest.Cells["AJ" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["TKchuan"].ToString());

                    cell = SheetPOSuggest.Cells["AK" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["TKHientai"].ToString());

                    cell = SheetPOSuggest.Cells["AL" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["TKCuoithang"].ToString());

                    cell = SheetPOSuggest.Cells["AM" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["SLChitieu"].ToString());

                    cell = SheetPOSuggest.Cells["AN" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["SLDenghi"].ToString());

                    cell = SheetPOSuggest.Cells["AO" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["SLConlai"].ToString());

                    cell = SheetPOSuggest.Cells["AP" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["SLDathang"].ToString());

                    cell = SheetPOSuggest.Cells["AQ" + countPO.ToString()];
                    cell.PutValue(dtData.Rows[i]["TBSellout"].ToString());
                
                    countPO++;
                    counter++;
                }
                //if (dtData.Rows.Count > 0)
                //{
                //    SheetPOSuggest.Cells.ImportDataTable(dtSuggest, true, "AF1");// du lieu Inventory
                 
                //}

                #endregion

                #region Fomat cell
               

                cell = SheetPOSuggest.Cells["C5"];
                style = cell.GetStyle();
                style.Custom = "#,##0";
                cell.SetStyle(style);
                SheetPOSuggest.Cells.Merge(4, 2, 1, 2);

                cell = SheetPOSuggest.Cells["F3"];
                cell.SetStyle(style);

                cell = SheetPOSuggest.Cells["F4"];
                cell.SetStyle(style);

          

                cell = SheetPOSuggest.Cells["H2"];
                cell.SetStyle(style);

                cell = SheetPOSuggest.Cells["H3"];
                cell.SetStyle(style);

                cell = SheetPOSuggest.Cells["H4"];
                cell.SetStyle(style);

                cell = SheetPOSuggest.Cells["H5"];
                cell.SetStyle(style);

                cell = SheetPOSuggest.Cells["A6"];
                style = SheetPOSuggest.Cells["A6"].GetStyle();
                style.ForegroundColor = Color.Yellow;
                style.Pattern = BackgroundType.Solid;

                range = SheetPOSuggest.Cells.CreateRange("A6", "N6");
                range.SetStyle(style);


                style = SheetPOSuggest.Cells["A7"].GetStyle();
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Right;

                range = SheetPOSuggest.Cells.CreateRange("A7", "A" + dtInvtID.Rows.Count + 7);
                range.SetStyle(style);





                style = SheetPOSuggest.Cells["E7"].GetStyle();
                style.Custom = "#,##0";
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Right;
                range = SheetPOSuggest.Cells.CreateRange("E7", "N" + dtInvtID.Rows.Count + 7);
                range.SetStyle(style);

                style = SheetPOSuggest.Cells["AI2"].GetStyle();
                style.Custom = "#,##0";
                style.Font.Color = Color.Transparent;
                style.HorizontalAlignment = TextAlignmentType.Right;
                range = SheetPOSuggest.Cells.CreateRange("AI2", "AQ" + dtInvtID.Rows.Count + 7);
                range.SetStyle(style);

                style = SheetPOSuggest.Cells["C7"].GetStyle();
                style.Number = 49; //Text
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Left;
                range = SheetPOSuggest.Cells.CreateRange("C7", "D" + dtInvtID.Rows.Count + 7);
                range.SetStyle(style);




                style = SheetPOSuggest.Cells["B7"].GetStyle();
                style.Number = 49; //Text
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Left;
                style.IsLocked = false;
                range = SheetPOSuggest.Cells.CreateRange("B7", "B" + dtInvtID.Rows.Count + 7);
                range.SetStyle(style);


                style = SheetPOSuggest.Cells["G7"].GetStyle();
                style.Custom = "#,##0";
                style.HorizontalAlignment = TextAlignmentType.Right;
                style.Font.Color = Color.Black;
                style.IsLocked = false;
                range = SheetPOSuggest.Cells.CreateRange("G7", "G" + dtInvtID.Rows.Count + 7);
                range.SetStyle(style);

                style = SheetPOSuggest.Cells["O1"].GetStyle();
                style.Font.Color = Color.Transparent;
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;




                range = SheetPOSuggest.Cells.CreateRange("O1", "ZZ" + (dtInvtID.Rows.Count + 7));
                range.ApplyStyle(style, flag);




                //Adds an empty conditional formatting
                int index = SheetPOSuggest.ConditionalFormattings.Add();
                FormatConditionCollection fcs = SheetPOSuggest.ConditionalFormattings[index];

                //Sets the conditional format range.
                CellArea ca = new CellArea();

                ca.StartRow = 6;
                ca.EndRow = dtInvtID.Rows.Count + 7;
                ca.StartColumn = 0;
                ca.EndColumn = 13;
                fcs.AddArea(ca);



                //Adds condition.
                int conditionIndex = fcs.AddCondition(FormatConditionType.Expression, OperatorType.Equal, "=$O7=\"G\"", "G");

                //Adds condition.
                int conditionIndex2 = fcs.AddCondition(FormatConditionType.Expression, OperatorType.Equal, "=$O7=\"R\"", "R");

                //Sets the background color.
                FormatCondition fc = fcs[conditionIndex];
                fc.Style.Font.Color = Color.Green;

                //Sets the background color.
                fc = fcs[conditionIndex2];
                fc.Style.Font.Color = Color.Red;

                SheetPOSuggest.AutoFilter.Range = "A6:N6";
                style = SheetPOSuggest.Cells["A6"].GetStyle();               
                //style.HorizontalAlignment = TextAlignmentType.Right;             
                style.IsLocked = false;
                range = SheetPOSuggest.Cells.CreateRange("A6:N6");
                range.SetStyle(style);
              
                #endregion
               
                SheetPOSuggest.AutoFitColumns();

                SheetPOSuggest.Cells.Columns[1].Width = 30;
                SheetPOSuggest.Cells.Columns[2].Width = 15;
                SheetPOSuggest.Cells.Columns[4].Width = 15;
                SheetPOSuggest.Cells.Columns[5].Width = 15;
                SheetPOSuggest.Cells.Columns[14].Width = 0;
                //SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Excel97To2003);
                stream.Flush();
                stream.Position = 0;
              
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "PO10100.xls" };
       
            }
            catch (Exception ex)
            {
               
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }
        public ActionResult Import(FormCollection data)
        {
            try
            {
                _ponbr = data["cboPONbr"];
                _branchID = data["cboBranchID"];
                _status = data["Status"].PassNull();
                _toStatus = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();
                DateTime dpoDate = data["PODate"].ToDateShort();

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile; // or: HttpPostedFileBase file = this.HttpContext.Request.Files[0];
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
                List<PO10100_pgDetail_Result> lstRecord = new List<PO10100_pgDetail_Result>();
                if (data["lstDet"] != null)
                {
                    var detHandler = new StoreDataHandler(data["lstDet"]);
                    _lstPODetailLoad = detHandler.ObjectData<PO10100_pgDetail_Result>()
                                .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                                .ToList();
                }
                else _lstPODetailLoad = new List<PO10100_pgDetail_Result>();
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];
                            string invtID = string.Empty;
                            int qty=0;
                            double price = 0; 
                            string unit = string.Empty;
                            string stt = string.Empty;
                            
                            int lineRef = 1;
                            for (int i = 6; i < workSheet.Cells.MaxDataRow; i++)
                            {
                                stt = workSheet.Cells[i, 0].StringValue;
                                qty = workSheet.Cells[i, 6].IntValue;
                                if (stt == string.Empty) break;
                                 if (qty == 0) continue;
                                invtID = workSheet.Cells[i, 1].StringValue;
                                unit = workSheet.Cells[i, 3].StringValue;
                                price = workSheet.Cells[i, 4].DoubleValue;
                       
                               

                                var objInvt = _db.PO10100_pcInventoryActive().FirstOrDefault(p => p.InvtID == invtID);

                                if (objInvt == null)
                                {
                                   message += string.Format("Dòng {0} mặt hàng {1} không có trong hệ thống<br/>", (i + 1).ToString(), invtID);
                                   continue;
                                }
                                var objIN_UnitConversion = _db.IN_UnitConversion.Where(p => p.InvtID.ToUpper().Trim() == invtID.ToUpper().Trim() && p.FromUnit.ToUpper().Trim() == unit.ToUpper().Trim() && p.ToUnit.ToUpper().Trim() == objInvt.StkUnit.ToUpper().Trim()).FirstOrDefault();//truong hop tu From toi to
                                if (objIN_UnitConversion == null)
                                {
                                    objIN_UnitConversion = _db.IN_UnitConversion.Where(p => p.InvtID.ToUpper().Trim() == invtID.ToUpper().Trim() && p.FromUnit.ToUpper().Trim() == objInvt.StkUnit.ToUpper().Trim() && p.ToUnit.ToUpper().Trim() == unit.ToUpper().Trim()).FirstOrDefault();

                                    if (objIN_UnitConversion != null) objIN_UnitConversion.MultDiv = objIN_UnitConversion.MultDiv == "D" ? "M" : "D";
                                    else objIN_UnitConversion = _db.IN_UnitConversion.Where(p => p.FromUnit.ToUpper().Trim() == unit.ToUpper().Trim() && p.ToUnit.ToUpper().Trim() == objInvt.StkUnit.ToUpper().Trim() && p.ClassID == "*" && p.InvtID == "*").FirstOrDefault();

                                    if (objIN_UnitConversion == null)
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} sai đơn vị quy đổi <br/>", (i + 1).ToString(), invtID, unit);
                                        continue;
                                    }
                                }
                                

                                if (_lstPODetailLoad.Where(p => p.InvtID == invtID).FirstOrDefault() != null || lstRecord.Where(p => p.InvtID == invtID).FirstOrDefault() != null)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} đã tồn tại <br/>", (i + 1).ToString(), invtID, unit);
                                    continue;
                                }
                            
                                var objInvtAll = _db.PO10100_pdIN_Inventory(Current.UserName).FirstOrDefault(p => p.InvtID == invtID);                              
                                var newrecord = new PO10100_pgDetail_Result();
                                newrecord.InvtID = invtID;
                                newrecord.PurchUnit = unit;
                                newrecord.QtyOrd = qty;
                                newrecord.TranDesc = objInvt.Descr;
                                newrecord.UnitMultDiv = objIN_UnitConversion.MultDiv;
                                newrecord.CnvFact = objIN_UnitConversion.CnvFact;
                                newrecord.UnitWeight = objInvtAll.StkWt;
                                newrecord.UnitVolume = objInvtAll.StkVol;

                                newrecord.ExtVolume = objInvtAll.StkWt*newrecord.QtyOrd;
                                newrecord.ExtWeight = objInvtAll.StkVol * newrecord.QtyOrd;
                                newrecord.TaxCat = objInvtAll.TaxCat;

                                var objUserDflt=_db.PO10100_pdOM_UserDefault(_branchID,Current.UserName).FirstOrDefault();
                                var objSite = _db.PO10100_pcSiteAll(_branchID).FirstOrDefault();
                                if (objUserDflt != null) {
                                    newrecord.SiteID= objUserDflt.POSite;
                                }
                                else if (objSite != null)
                                {
                                    newrecord.SiteID = objSite.SiteID;
                                }
                                else
                                {
                                    newrecord.SiteID = objInvtAll.DfltSite;
                                }
                                var objPO_Setup = _db.PO_Setup.FirstOrDefault(p => p.SetupID == "PO" && p.BranchID == _branchID);
                                //lay gia
                                if (objPO_Setup.DfltLstUnitCost == "A")
                                {
                                    var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == newrecord.InvtID && p.SiteID == newrecord.SiteID).FirstOrDefault();

                                    newrecord.UnitCost = objIN_ItemSite == null ? 0 : objIN_ItemSite.AvgCost;
                                    newrecord.UnitCost = Math.Round((newrecord.UnitMultDiv == "D" ? (newrecord.UnitCost / newrecord.CnvFact) : (newrecord.UnitCost * newrecord.CnvFact)));
                                    newrecord.ExtCost = newrecord.UnitCost * newrecord.QtyOrd - newrecord.DiscAmt;
                                }
                                else if (objPO_Setup.DfltLstUnitCost == "P")
                                {
                                    var result = _db.PO10100_ppGetPrice(_branchID, invtID, unit, dpoDate).FirstOrDefault().Value;
                                    newrecord.UnitCost = result;
                                    newrecord.ExtCost = result * newrecord.QtyOrd - newrecord.DiscAmt;
                                }
                                else if (objPO_Setup.DfltLstUnitCost == "I")
                                {
                                    var UnitCost = objInvtAll.POPrice;
                                    UnitCost = Math.Round((newrecord.UnitMultDiv == "D" ? (UnitCost / newrecord.CnvFact) : (UnitCost * newrecord.CnvFact)));
                                    newrecord.UnitCost = UnitCost;
                                    newrecord.ExtCost = UnitCost * newrecord.QtyOrd - newrecord.DiscAmt;
                                }

                                if (newrecord.UnitCost == 0 && objPO_Setup.EditablePOPrice==false)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có giá <br/>", (i + 1).ToString(), invtID, unit);
                                    continue;
                                }

                                lstRecord.Add(newrecord);                              
                            }
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message, lstTrans = lstRecord });
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
                return _logMessage;
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }
        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                _ponbr = data["cboPONbr"];
                _branchID = data["cboBranchID"];
                string reportName = "";
                var rpt = new RPTRunning();
                rpt.ResetET();

                reportName = "PO_PurchaseOrder";
                rpt.ReportNbr = "PO602";
                rpt.MachineName = "Web";
                rpt.ReportCap = "ReportName";
                rpt.ReportName = reportName;
                rpt.ReportDate = DateTime.Now;
                rpt.DateParm00 = DateTime.Now;
                rpt.DateParm01 = DateTime.Now;
                rpt.DateParm02 = DateTime.Now;
                rpt.DateParm03 = DateTime.Now;
                rpt.StringParm00 = _branchID;
                rpt.StringParm01 = _ponbr;
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
  
        #endregion
        #region Other
        public void Insert_IN_ItemSite(ref IN_ItemSite objIN_ItemSite, ref IN_Inventory objIN_Inventory, string SiteID)
        {
            try
            {
                PO10100Entities _dbitem = Util.CreateObjectContext<PO10100Entities>(false);
             
                
                objIN_ItemSite.InvtID = objIN_Inventory.InvtID;
                objIN_ItemSite.SiteID = SiteID;
                //objIN_ItemSite.AvgCost = 0;
                //objIN_ItemSite.QtyAlloc = 0;
                //objIN_ItemSite.QtyAllocIN = 0;
                //objIN_ItemSite.QtyAllocPORet = 0;
                //objIN_ItemSite.QtyAllocSO = 0;
                //objIN_ItemSite.QtyAvail = 0;
                //objIN_ItemSite.QtyInTransit = 0;
                //objIN_ItemSite.QtyOnBO = 0;
                //objIN_ItemSite.QtyOnHand = 0;
                ////objIN_ItemSite.QtyOnPO = 0;
                //objIN_ItemSite.QtyOnTransferOrders = 0;
                //objIN_ItemSite.QtyOnSO = 0;
                //objIN_ItemSite.QtyShipNotInv = 0;
                objIN_ItemSite.StkItem = objIN_Inventory.StkItem;
                //objIN_ItemSite.TotCost = 0;
                objIN_ItemSite.Crtd_DateTime = DateTime.Now;
                objIN_ItemSite.Crtd_Prog = ScreenNbr;
                objIN_ItemSite.Crtd_User = Current.UserName;
                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                objIN_ItemSite.LUpd_Prog = ScreenNbr;
                objIN_ItemSite.LUpd_User = Current.UserName;
                objIN_ItemSite.tstamp = new byte[0];
                
                _dbitem.IN_ItemSite.AddObject(objIN_ItemSite);
                _dbitem.SaveChanges();
                //_lstIN_ItemSite.Add(objIN_ItemSite);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
      
        private string LastLineRef(List<PO10100_pgDetail_Result> lst)
        {
            string strlineRef = "";
            int ilineRef = 0;
            strlineRef = lst.Count == 0 ? "00000" : lst.OrderByDescending(p => p.LineRef).FirstOrDefault().LineRef;
            ilineRef = int.Parse(strlineRef) + 1;
            strlineRef = ("00000" + ilineRef);
            strlineRef = strlineRef.Substring(strlineRef.Length - 5, 5);
            return strlineRef;
        }
        private double GetDiscLineSetup(PO10100_pgDetail_Result det, double qty, double amt, ref double discAmt, ref double discPct, ref string discSeq, ref string discID, ref string budgetID, ref string breaklineRef)
        {
            double discItemUnitQty = 0;
            
            var lstsetup =_db.PO10100_pdOM_DiscAllByBranchPO(_branchID).Where(p => p.DiscType == "L").ToList();
            if (lstsetup.Count > 0)
            {
                foreach (var setup in lstsetup)
                {
                    var objDisc = setup;// (from p in _PO10100Context.OM_Discounts where p.DiscType == "L" && p.Status == "C" && p.POUse == true select p).FirstOrDefault();
                    var objCpnyID = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.CpnyID == _branchID && p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim()).FirstOrDefault();
                    if (objCpnyID != null)
                    {
                        if (objDisc != null)
                        {
                            if (objDisc.DiscType == "L")
                            {
                                var lstSeq = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.Status.ToUpper().Trim() == "C" && p.Active == 1 && p.POUse == true && ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0 && p.Promo == 0) || ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0) && (DateTime.Compare(_poHead.PODate.ToDateShort(), p.POEndDate) <= 0) && p.Promo == 1))).ToList();
                                if (lstSeq.Count > 0)
                                {
                                    foreach (var seq in lstSeq)
                                    {
                                        var objItem = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p =>
                                                           p.DiscID == objDisc.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() &&
                                                           p.InvtID == det.InvtID
                                                           ).FirstOrDefault();
                                        if (objItem == null) objItem = new PO10100_pdOM_DiscAllByBranchPO_Result();
                                        var objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                        if (objInvt == null) objInvt = new IN_Inventory();
                                        double cnvFact = 0;
                                        string unitMultDiv = "";
                                        if (seq.BreakBy == "W")
                                        {

                                            discItemUnitQty = qty * objInvt.StkWt / OM_GetCnvFactFromUnit(det.InvtID, det.PurchUnit, ref cnvFact, ref unitMultDiv);
                                        }
                                        else
                                            discItemUnitQty = qty * OM_GetCnvFactFromUnit(det.InvtID, det.PurchUnit, ref cnvFact, ref unitMultDiv);

                                        if (seq.Active != 0 && objDisc.DiscClass == "II")
                                        {
                                            var objtmpItem = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == seq.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() && p.InvtID == det.InvtID).FirstOrDefault();
                                            if (objtmpItem != null)
                                            {
                                                discID = objDisc.DiscID;
                                                discSeq = seq.DiscSeq;
                                                budgetID = seq.BudgetID;
                                                CalculateLineDisc(seq.DiscID, seq.DiscSeq, discItemUnitQty, amt, seq.BreakBy,
                                                                  seq.DiscFor, ref discAmt, ref discPct, ref breaklineRef);
                                            }

                                        }
                                    }//foreach (var seq in lstSeq)
                                }// if(lstSeq.Count>0)
                            }//objDisc.DiscType=="L"                    
                        }//objDisc!=null
                    }
                }//foreach (var setup in lstsetup)
            }//lstsetup.Count>0
            return discAmt;
        }
        private void CalculateLineDisc(string discID, string discSeq, double qty, double amt, string breakBy, string discFor, ref double discAmt1, ref double discPct1, ref string breakLineRef)
        {
            double qtyBreak = 0;
            double qtyAmt = 0;
            double discAmt = 0;
            if (breakBy == "A")
                qtyAmt = amt;
            else
                qtyAmt = qty;
        begin:
            var objSeq = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == discSeq.ToUpper().Trim()).FirstOrDefault();
            if (objSeq == null) objSeq = new PO10100_pdOM_DiscAllByBranchPO_Result();
            if (objSeq.BudgetID.PassNull() != "")
            {
                throw new MessageException(MessageType.Message, "403",
                         parm: new[] { objSeq.DiscSeq, objSeq.BudgetID });
             
                return;
            }
            discAmt = GetDiscBreak(discID, discSeq, breakBy, qtyAmt, ref qtyBreak, ref breakLineRef);
            if (discAmt != 0)
            {
                if (discFor == "A")
                {
                    discAmt1 = discAmt1 + discAmt * (qtyAmt / qtyBreak).ToInt();
                    if (qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0)
                    {
                        qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                        goto begin; ;
                    }
                    discPct1 = Math.Round((discAmt1 / amt) * 100, 0);
                }
                else if (discFor == "P")
                {
                    discPct1 = discPct1 + discAmt;
                    discAmt1 = Math.Round((discPct1 * amt) / 100, 0);
                }
                else if (discFor == "X")
                    discAmt1 = discAmt1 + discAmt + qty;
            }


        }
        private double GetDiscBreak(string discID, string discSeq, string discBreak, double qtyAmt, ref double tmpqtyBreak, ref string lineRef)
        {
            double result = 0;
            double qtyBreak = tmpqtyBreak;
            PO10100_pdOM_DiscAllByBranchPO_Result objBreak = new PO10100_pdOM_DiscAllByBranchPO_Result();
            if (discBreak == "A")
                objBreak = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == discSeq.ToUpper().Trim() && p.BreakAmt <= qtyAmt && p.BreakAmt > 0).OrderByDescending(p => p.BreakAmt).FirstOrDefault();
            else
                objBreak = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == discSeq.ToUpper().Trim() && p.BreakQty <= qtyAmt && p.BreakQty > 0).OrderByDescending(p => p.BreakQty).FirstOrDefault();

            if (objBreak != null)
            {
                if (discBreak == "A")
                    qtyBreak = objBreak.BreakAmt.Value;
                else
                    qtyBreak = objBreak.BreakQty.Value;
                lineRef = objBreak.LineRef;
                result = objBreak.DiscAmt.Value;
            }
            tmpqtyBreak = qtyBreak;
            return result;
        }
        private double OM_GetCnvFactFromUnit(string invtID, string unitDesc, ref double cnvFact, ref string unitMultDiv)
        {
            //double cnvFact = 1;
            var objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt != null)
            {
                var cnv = _PO10100_pdIN_UnitConversion_Result.Where(p => p.InvtID == invtID && p.FromUnit == unitDesc && p.ToUnit == objInvt.StkUnit).FirstOrDefault();
                if (cnv == null)
                    cnv = _PO10100_pdIN_UnitConversion_Result.Where(p => p.InvtID == invtID && p.FromUnit == objInvt.StkUnit && p.ToUnit == unitDesc).FirstOrDefault();
                else if (cnv == null)
                    cnv = _PO10100_pdIN_UnitConversion_Result.Where(p => p.UnitType == "3" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == invtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == unitDesc.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == objInvt.StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                else if (cnv == null)
                    cnv = _PO10100_pdIN_UnitConversion_Result.Where(p => p.UnitType == "2" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == invtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == unitDesc.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == objInvt.StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                else if (cnv == null)
                    cnv = _PO10100_pdIN_UnitConversion_Result.Where(p => p.UnitType == "1" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == "*" && p.FromUnit.PassNull().Trim().ToUpper() == unitDesc.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == objInvt.StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();

                if (cnv != null)
                {
                    if (cnv.MultDiv == "D")
                    {
                        cnvFact = 1 / cnv.CnvFact;
                    }
                    else
                    {
                        cnvFact = cnv.CnvFact;
                    }
                    unitMultDiv = cnv.MultDiv;
                }
                else
                {
                    unitMultDiv = "M";
                    cnvFact = 1;
                }
            }
            return cnvFact;
        }
        private void FreeItemForLine(ref string lineRef, string discLineRef)
        {
            string discSeq = string.Empty;
            string discID = string.Empty;
            string boType = string.Empty;
            _lstTmpPO10100_pgDetail = _lstPODetailLoad;
            _countAddItem = 0;
            _freeLineRunning = true;
            foreach (var det in _lstTmpPO10100_pgDetail)
            {
                if (det.PurchaseType != "PR")
                {
                    discSeq = det.DiscSeq;
                    //discSeq2 = det.DiscSeq2;
                    discID = det.DiscID;
                    //discID2 = det.DiscID2;
                    boType = det.PurchaseType;
                    GetFreeItemLine(det.PurchUnit, det.InvtID, det.ExtCost, det.QtyOrd, ref discSeq, ref discID,
                                    det.LineRef, ref lineRef, det.LineRef, boType);
                }
            }
            _freeLineRunning = false;

        }
        private void GetFreeItemLine(string unit, string invtID, double amt, double qty, ref string discSeq, ref string discID, string currentLineRef, ref string lineRef, string discLineRef, string boType)
        {
            double freeItemQty1 = 0;
            string freeItemID1 = string.Empty;
            string siteID1 = string.Empty;
            string siteID2 = string.Empty;
            string uom1 = string.Empty;
            string uom2 = string.Empty;
            string budgetID1 = string.Empty;
            string budgetID2 = string.Empty;
            double qtyBreak = 0;
            double qtyAmt = 0;
            bool calcDisc = false;
            double discItemUntiQty = 0;
            List<PO10100_pdOM_DiscAllByBranchPO_Result> lstDiscSeq;
            List<PO10100_pdOM_DiscAllByBranchPO_Result> lstDiscFreeItem1;
            var lstSetup = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscType == "L").ToList();
            foreach (var setup in lstSetup)
            {

                var objDisc = setup;// (from p in _PO10100Context.OM_Discounts where p.DiscType == "L" && p.Status == "C" && p.POUse == true select p).FirstOrDefault();
                //var objCpnyID = lstPO10100_pdOM_DiscAllByBranchPO_ResultALL.Where(p=>p.CpnyID == strbranchID && p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() select p).FirstOrDefault();
                //if (objCpnyID != null)
                //{
                if (objDisc != null)
                {
                    if (objDisc.DiscType == "L")
                    {
                        _poHead.PODate = _poHead.PODate == null ? DateTime.Now.ToDateShort() : _poHead.PODate;
                        lstDiscSeq = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.Status == "C" && p.Active == 1 && p.POUse == true && ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0 && p.Promo == 0) || ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0) && (DateTime.Compare(_poHead.PODate.ToDateShort(), p.POEndDate) <= 0) && p.Promo == 1))).ToList();
                        foreach (var seq in lstDiscSeq)
                        {
                            calcDisc = false;
                            var objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                            PO10100_pdOM_DiscAllByBranchPO_Result objItem = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim()).FirstOrDefault();

                            if (objItem == null) objItem = new PO10100_pdOM_DiscAllByBranchPO_Result();
                            double cnvFact = 0;
                            string unitMultDiv = "";
                            discItemUntiQty = (qty * OM_GetCnvFactFromUnit(invtID, unit, ref cnvFact, ref unitMultDiv).ToInt());// objItem.UnitDesc)).ToInt();

                            lstDiscFreeItem1 = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim()).ToList();

                            if (seq.BreakBy == "A")
                                qtyAmt = amt;
                            else if (seq.BreakBy == "W")
                            {
                                objInvt = _db.IN_Inventory.Where(p => p.InvtID == invtID).FirstOrDefault();
                                qtyAmt = discItemUntiQty * objInvt.StkWt;
                            }
                            else
                            {
                                if (seq.DiscClass == "II" || seq.DiscClass == "CI" || seq.DiscClass == "TI")
                                    qtyAmt = discItemUntiQty;
                                else
                                    qtyAmt = amt;
                            }

                            if (lstDiscFreeItem1.Count > 0 && seq.Active != 0 && objDisc.DiscClass == "II")
                            {
                                var objTmpItem = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == seq.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() && p.InvtID == invtID).FirstOrDefault();
                                if (objTmpItem != null)
                                {
                                    calcDisc = true;
                                    goto Calc;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && seq.Active != 0 && objDisc.DiscClass == "PP")
                            {
                                objInvt = _db.IN_Inventory.Where(p => p.InvtID == invtID).FirstOrDefault();
                                var objDiscItemClass = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == seq.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() && p.ClassID.ToUpper().Trim() == objInvt.PriceClassID.ToUpper().Trim()).FirstOrDefault();
                                if (objDiscItemClass != null)
                                {
                                    calcDisc = true;
                                    goto Calc;
                                }
                            }
                            else
                            {
                                calcDisc = false;
                            }
                        Calc:
                            bool goto_Calc = false;
                            double freeItemQty = 0;
                            string breakLineRef1 = string.Empty;
                            if (calcDisc)
                            {
                                GetDiscBreak(seq.DiscID, seq.DiscSeq, seq.BreakBy, qtyAmt, ref qtyBreak, ref breakLineRef1);
                                discID = objDisc.DiscID;
                                discSeq = seq.DiscSeq;
                                var lstDiscFreeItem1tmp = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p =>
                                                            p.DiscID.ToUpper().Trim() == seq.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() && p.LineRef == breakLineRef1)
                                                          .ToList();

                                if (lstDiscFreeItem1tmp.Count > 0)
                                {
                                    if (lstDiscFreeItem1tmp.Count > 1 && !seq.AutoFreeItem && !(seq.DiscClass == "II" && seq.ProAplForItem == "M"))
                                    {

                                        goto NextDiscSeq;
                                    }//if(lstDiscFreeItem1tmp.Count>1 && ! seq.AutoFreeItem && !(seq.DiscClass=="II" &&seq.ProAplForItem=="M"))

                                    string invtID1 = string.Empty;
                                    if (seq.DiscClass == "II" && seq.ProAplForItem == "M")
                                        invtID1 = invtID;
                                    else
                                        invtID1 = "";

                                    int countRow = 0;
                                    if (lstDiscFreeItem1tmp.Where(p => p.FreeItemID.Contains(invtID1)).Count() > 0)
                                    {
                                        lstDiscFreeItem1tmp = lstDiscFreeItem1tmp.Where(p => p.FreeItemID.Contains(invtID1)).ToList();
                                    }
                                    else
                                    {
                                        lstDiscFreeItem1tmp.Clear();
                                    }
                                    foreach (var free in lstDiscFreeItem1tmp)
                                    {
                                        countRow = countRow + 1;
                                        uom1 = free.UnitDescr;
                                        budgetID1 = free.FreeItemBudgetID;
                                        freeItemQty = free.FreeItemQty.Value;
                                        siteID1 = free.FreeITemSiteID;
                                        if (!goto_Calc)
                                            freeItemQty1 = 0;

                                        if (freeItemQty > 0 || freeItemQty1 > 0)
                                        {
                                            freeItemQty1 = freeItemQty * (int)(qtyAmt / qtyBreak);
                                            if (freeItemQty > 0 && countRow == lstDiscFreeItem1tmp.Count)
                                            {
                                                if (qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0)
                                                {
                                                    qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                                                    goto_Calc = true;
                                                }
                                                else
                                                    goto_Calc = false;
                                            }
                                            freeItemID1 = free.FreeItemID;
                                            //double cnvFact = 0;
                                            //string unitMultDiv = string.Empty;
                                            objInvt = _db.IN_Inventory.Where(p => p.InvtID == freeItemID1).FirstOrDefault();
                                            if (objInvt == null) objInvt = new IN_Inventory();
                                            uom1 = string.IsNullOrEmpty(uom1) ? (objInvt.StkUnit == null ? "" : objInvt.StkUnit) : uom1;
                                            if (SetUOM(ref cnvFact, ref unitMultDiv, freeItemID1, objInvt.ClassID, objInvt.StkUnit, uom1))
                                            {

                                            }

                                            if (freeItemQty1 > 0)
                                            {
                                                //this.InsertUpdateOrdDisc(discID, discSeq, budgetID1, objDisc.DiscType,
                                                //                         seq.DiscFor, 0, 0, 0, seq.BudgetID, freeItemID1,
                                                //                         freeItemQty1, discLineRef, lineRef,
                                                //                         breakLineRef1);
                                                //discLineRef = (discLineRef.ToInt() + 1).ToString();
                                                //for (int t = discLineRef.Length; discLineRef.Length < 5; )
                                                //    discLineRef = "0" + discLineRef;

                                                if (freeItemID1 != string.Empty && freeItemQty1 > 0)
                                                {
                                                    this.AddFreeItem(discID, discSeq, freeItemID1, freeItemQty1, siteID1, uom1, lineRef, discLineRef, budgetID1, boType);
                                                    //lineRef = (lineRef.ToInt() + 1).ToString();
                                                }

                                            }//if(freeItemQty1>0)
                                        }//if(freeItemQty>0 || freeItemQty1>0)
                                        ///NextFreeItem1:
                                        //int a;
                                    }// foreach (var free in lstDiscFreeItem1tmp)
                                    if (goto_Calc)
                                        goto Calc;
                                    //objDisc = (from p in _PO10100Context.OM_Discounts where p.DiscID == setup.DiscID02 select p).FirstOrDefault();
                                    //if (setup.DiscID02.PassNull() != string.Empty && objDisc != null)
                                    //    break;
                                    //else
                                    //    return;
                                }//if(lstDiscFreeItem1tmp.Count>0)
                            }//calcDisc        
                        NextDiscSeq:
                            int b;
                        }//foreach (var seq in lstDiscSeq)
                    }//if(objDisc.DiscType=="L")   
                }// if(objDisc!=null)  
                //}
            }//foreach (var setup in lstSetup)
        }
        private void AddFreeItem(string discID, string discSeq, string freeItemID, double qty, string siteID, string uom, string lineRef, string disclineref, string budgetID, string boType)
        {
            var objDisc = _lstPO10100_pdOM_DiscAllByBranchPO.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim()).FirstOrDefault();
            if (objDisc.DiscType == "L")
                _countAddItem++;
            double cnvFact = 0;
            string unitMultDiv = string.Empty;
            double price = 0;
            string chk = string.Empty;
            double soFee = 0;
            double stkQty = 0;
            if (freeItemID != string.Empty && qty > 0)
            {
                var objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID);
                if (objInvt == null) objInvt = new IN_Inventory();

                uom = string.IsNullOrEmpty(uom) ? (objInvt.StkUnit == null ? "" : objInvt.StkUnit) : uom;
                if (SetUOM(ref cnvFact, ref unitMultDiv, freeItemID, objInvt.ClassID, objInvt.StkUnit, uom))
                {

                }

                if (unitMultDiv == "M")
                    stkQty = qty * cnvFact;
                else
                {
                    if (cnvFact != 0)
                        stkQty = qty / cnvFact;
                    else
                        stkQty = 0;
                }



                PO10100_pgDetail_Result newdet = new PO10100_pgDetail_Result();
                newdet.ResetET();//_clsApp.ResetPO10100_pgDetail_Result();
                newdet.PONbr = _ponbr;
                newdet.BranchID = _branchID;
                newdet.LineRef = lineRef;
                newdet.DiscID = discID;
                newdet.SiteID = string.IsNullOrEmpty(siteID) ? (objOM_UserDefault.DiscSite == null ? "" : objOM_UserDefault.DiscSite) : siteID;
                newdet.DiscSeq = discSeq;
                newdet.InvtID = freeItemID;
                newdet.PurchUnit = string.IsNullOrEmpty(uom) ? (objInvt.StkUnit == null ? "" : objInvt.StkUnit) : uom;
                newdet.DiscLineRef = disclineref;
                newdet.TranDesc = objInvt.Descr;
                //newdet.TranDesc=
                newdet.QtyOrd = qty;
                newdet.PurchaseType = "PR";
                newdet.VouchStage = "N";
                newdet.RcptStage = "N";
                newdet.TaxID = "*";
                newdet.UnitCost = 0;
                newdet.UnitMultDiv = unitMultDiv;
                newdet.CnvFact = cnvFact;
                _lstPODetailLoad.Add(newdet);
            }
        }
        public bool SetUOM(ref double Cnvfact, ref string UnitMultDiv, string InvtID, string ClassID, string StkUnit, string FromUnit)
        {
            Cnvfact = 0;
            PO10100_pdIN_UnitConversion_Result objIN_UnitConversion = new PO10100_pdIN_UnitConversion_Result();
            try
            {
                if (!string.IsNullOrEmpty(FromUnit))
                {
                    objIN_UnitConversion = _PO10100_pdIN_UnitConversion_Result.Where(p => p.UnitType == "3" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == InvtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == FromUnit.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                    if (objIN_UnitConversion == null)
                        objIN_UnitConversion = _PO10100_pdIN_UnitConversion_Result.Where(p => p.UnitType == "2" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == InvtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == FromUnit.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                    if (objIN_UnitConversion == null)
                        objIN_UnitConversion = _PO10100_pdIN_UnitConversion_Result.Where(p => p.UnitType == "1" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == "*" && p.FromUnit.PassNull().Trim().ToUpper() == FromUnit.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                    if (objIN_UnitConversion == null)
                    {
                        throw new MessageException(MessageType.Message, "25",
                            parm: new[] { InvtID });
                      
                      
                        Cnvfact = 0;
                        UnitMultDiv = "";
                        return false;
                    }
                    Cnvfact = objIN_UnitConversion.CnvFact;
                    UnitMultDiv = objIN_UnitConversion.MultDiv;

                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {

                throw (ex);
                return false;
            }
        }        
        private void SetCellValueHeader(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" "+lang);
            var style = c.GetStyle();
            style.Font.IsBold = true;
            style.Font.Size = 10;
            style.Font.Color = Color.Blue;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }

      
        #endregion
    }
    public class CountInvtID
    {
        public int count { get; set; }
        public string invtID { get; set; }
    }
}

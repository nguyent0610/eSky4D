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
namespace PO10100.Controllers
{
   
    [CustomAuthorize]
    [CheckSessionOut]
    [DirectController]
    public class PO10100Controller : Controller
    {
      
        PO10100Entities db = Util.CreateObjectContext<PO10100Entities>(false);
        private string screenNbr="PO10100";
        private FormCollection _form;
        private JsonResult _logMessage;
        private string _currentPO;
        private List<PO_DetailLoad_Result> lstPODetailLoad;     
        private PO_Setup objPO_Setup;
        private PO_Header _poHead;
        bool bStatusClose = false;
        string strponbr = "";
        string strbranchID = "";      
        HQ4DApp clsApp = new HQ4DApp();
        List<OM_DiscAllByBranchPO_Result> lstOM_DiscAllByBranchPO_ResultALL;
        List<IN_Inventory> lstIN_Inventory;
        List<IN_UnitConversion> lstUnitConversion;
        private List <PO_DetailLoad_Result> _lstTmpPO_DetailLoad;
        private bool _freeLineRunning = false;
        private string _lineRef = string.Empty;
        private int _countAddItem = 0;
        private OM_UserDefault objOM_UserDefault;

        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public ActionResult Index()
        {
          
            ViewBag.ListUnitConversion = db.IN_UnitConversion.ToList();
            ViewBag.Title = DateTime.Now.ToLongTimeString();
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

       
        #region Get Data
        public ActionResult GetPO_Header(string pONbr, string branchID)
        {
            var obj = db.PO_Header.Where(p => p.PONbr == pONbr && p.BranchID == branchID);
            return this.Store(obj);

        }
        public ActionResult GetAP_VendorTax(string vendID, string ordFromId)
        {
          
            return this.Store(db.AP_VendorTaxes(vendID,ordFromId));

        }    
        public ActionResult GetPO_DetailLoad(string pONbr, string branchID)
        {
            lstPODetailLoad = db.PO_DetailLoad(pONbr, branchID, "%").ToList();
            return this.Store(lstPODetailLoad);

        }
        public ActionResult GetPO10100_LoadTaxTrans(string pONbr, string branchID)
        {
            return this.Store(db.PO10100_LoadTaxTrans(branchID, pONbr).ToList());
        }
        //public ActionResult GetPO10100_LoadTaxTrans()
        //{
        //    return this.Store(new System.Data.Objects.ObjectResult<PO10100_LoadTaxTrans_Result>());
        //}  
        #endregion
        #region DataProcess    
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {
                _form = data;
               
                SaveData(data);

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Json(new { success = true, PONbr =strponbr });
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
        [DirectMethod]
        public ActionResult PO10100Delete(string UserID)
        {
            //var cpny = db.Users.FirstOrDefault(p => p.UserName == UserID);
            //if (cpny != null)
            //{
            //    db.Users.DeleteObject(cpny);
            //}

            //var lstAddr = db.SYS_UserGroup.Where(p => p.UserID == UserID).ToList();
            //foreach (var item in lstAddr)
            //{
            //    db.SYS_UserGroup.DeleteObject(item);
            //}

            //var lstSub = db.SYS_UserCompany.Where(p => p.UserName == UserID).ToList();
            //foreach (var item in lstSub)
            //{
            //    db.SYS_UserCompany.DeleteObject(item);
            //}

            //db.SaveChanges();
            return this.Direct();
        }
        //Lấy giá
        [DirectMethod]
        public ActionResult POPrice(string branchID="",string invtID="",string Unit="",DateTime? podate=null)
        {
            var result=db.PO_GetPrice(branchID, invtID, Unit, podate).FirstOrDefault().Value;
            return this.Direct(result);
           
        }
        [DirectMethod]
        public ActionResult ItemSitePrice(string branchID = "", string invtID = "", string siteID = "")
        {
            var objIN_ItemSite = db.IN_ItemSite.Where(p => p.InvtID == invtID && p.SiteID == siteID).FirstOrDefault();

            return this.Direct(objIN_ItemSite);

        }
        #region Save Data
        private void SaveData(FormCollection data)
        {
            _form = data;
            strponbr = data["cboPONbr"];
            strbranchID = data["cboBranchID"];
            DateTime dpoDate = data["PODate"].ToDateShort();

            
            var detHeader = new StoreDataHandler(data["lstHeader"]);
            if (_poHead == null)
                _poHead = detHeader.ObjectData<PO_Header>().FirstOrDefault();


            var detHandler = new StoreDataHandler(data["lstDet"]);
            lstPODetailLoad = detHandler.ObjectData<PO_DetailLoad_Result>()
                        .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                        .ToList();

            lstOM_DiscAllByBranchPO_ResultALL = db.OM_DiscAllByBranchPO(strbranchID).ToList();
            lstIN_Inventory = db.IN_Inventory.ToList();
            lstUnitConversion = db.IN_UnitConversion.ToList();
            objOM_UserDefault = db.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID.Trim().ToUpper() == strbranchID.Trim().ToUpper() && p.UserID.Trim().ToUpper() == Current.UserName.Trim().ToUpper());
            objPO_Setup = db.PO_Setup.FirstOrDefault(p => p.BranchID == strbranchID && p.SetupID == "PO");


            var obj = db.PO_Header.FirstOrDefault(p => p.PONbr == strponbr && p.BranchID == strbranchID);
            if (Data_Checking())
            {
                if (obj != null)
                {

                    if (obj.tstamp.ToHex() != _poHead.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    UpdatingPO_Header(ref obj, _poHead, lstPODetailLoad);
                    Save_PO_Detail(obj, lstPODetailLoad);
                }
                else
                {
                    if (objPO_Setup.AutoRef == 1)
                    {
                        obj = new PO_Header();
                        UpdatingPO_Header(ref obj, _poHead, lstPODetailLoad);

                        var obj1 = db.PONumbering(strbranchID, "PONbr").FirstOrDefault();
                        strbranchID = obj.BranchID = strbranchID;
                        strponbr = obj.PONbr = obj1;
                        obj.IsExport = false;
                        obj.ImpExp = "";

                        obj.Crtd_DateTime = DateTime.Now;
                        obj.Crtd_Prog = screenNbr;
                        obj.Crtd_User = Current.UserName;

                        db.PO_Header.AddObject(obj);
                        Save_PO_Detail(obj, lstPODetailLoad);
                    }
                }
            }
        }
        //private bool DeleteData(object sender, HQSLFramework.Message.HQMessageBox.ExitCode e)
        //{
        //    try
        //    {
        //        _complete = true;
        //        if (e == HQMessageBox.ExitCode.Yes)
        //        {
        //            busyIndicator.IsBusy = true;
        //            //delete Header
        //            var query = (from p in db.GetPO_Header where p.BranchID == data["cboBranchID"] && p.PONbr == data["cboPONbr"] select p);
        //            db.Load(query, LoadBehavior.RefreshCurrent, true).Completed += (sender1, args) =>
        //            {
        //                var obj = (from p in db.PO_Headers select p).Where(p => p.PONbr == data["cboPONbr"] && p.BranchID == data["cboBranchID"]).FirstOrDefault();
        //                if (obj != null)
        //                {
        //                    if (obj.tstamp.ToHex() != objPO_Header.tstamp.ToHex())
        //                    {
        //                        if (_complete)
        //                            HQMessageBox.Show(19, hqSys.LangID, null, HQmesscomplete);
        //                        _complete = false;

        //                        db.RejectChanges();
        //                        return;
        //                    }
        //                    db.PO_Headers.Remove(obj);
        //                    //delete Detail
        //                    var query1 = (from p in db.GetPO_Detail where p.BranchID == data["cboBranchID"] && p.PONbr == data["cboPONbr"] select p);
        //                    var query2 = (from p in db.GetIN_ItemSite select p);
        //                    // objItemSite = iN_ItemSiteDomainDataSource.Data.OfType<IN_ItemSite>().Where(p => p.InvtID == InvtID && p.SiteID == SiteID).FirstOrDefault();
        //                    db.Load(query2, LoadBehavior.RefreshCurrent, true).Completed += (sender3, args3) =>
        //                    {
        //                        db.Load(query1, LoadBehavior.RefreshCurrent, true).Completed += (sender2, args2) =>
        //                        {
        //                            try
        //                            {
        //                                while (lstDetail.Count() > 0)
        //                                {
        //                                    if (lstDetail.FirstOrDefault() != null)
        //                                    {
        //                                        var r = lstDetail.FirstOrDefault().Data as PO_DetailLoad_Result;
        //                                        var obj1 = db.PO_Detail.Where(p => p.BranchID == data["cboBranchID"] && p.PONbr == data["cboPONbr"] && p.LineRef == r.LineRef).FirstOrDefault();
        //                                        if (obj1 != null)
        //                                        {
        //                                            if (obj1.PurchaseType == "GI" || obj1.PurchaseType == "PR" || obj1.PurchaseType == "GS")
        //                                            {
        //                                                double OldQty = Math.Round((obj1.UnitMultDiv == "D" ? ((obj1.QtyOrd - r.QtyRcvd) / obj1.CnvFact) : (obj1.QtyOrd - obj1.QtyRcvd) * obj1.CnvFact));
        //                                                UpdateOnPOQty(obj1.InvtID, obj1.SiteID, OldQty, 0, 2);
        //                                            }
        //                                        }
        //                                        if (obj1 != null) db.PO_Details.Remove(obj1);
        //                                    }
        //                                    lstDetail.Remove(lstDetail.FirstOrDefault());
        //                                }
        //                                try
        //                                {
        //                                    if (db.HasChanges)
        //                                    {
        //                                        this.db.SubmitChanges(OnSubmitCompleted1 =>
        //                                        {
        //                                            if (OnSubmitCompleted1.HasError)
        //                                            {
        //                                                string message = "";

        //                                                if (OnSubmitCompleted1.EntitiesInError.Any())
        //                                                {
        //                                                    message = "";

        //                                                    Entity entityInError = OnSubmitCompleted1.EntitiesInError.First();
        //                                                    if (entityInError.ValidationErrors.Any())
        //                                                    {
        //                                                        message = entityInError.ValidationErrors.First().ErrorMessage;
        //                                                    }
        //                                                }
        //                                                busyIndicator.IsBusy = false;

        //                                                // ErrorWindow.CreateNew(OnSubmitCompleted1.Error, message);
        //                                                OnSubmitCompleted1.MarkErrorAsHandled();
        //                                                throw new Exception(message);
        //                                                // OnSubmitCompleted1.MarkErrorAsHandled();
        //                                            }
        //                                            else
        //                                            {
        //                                                (this.Resources["ppv_PO10100PONbr_Branch_All_ResultDomainDataSource"] as DomainDataSource).Clear();
        //                                                (this.Resources["ppv_PO10100PONbr_Branch_All_ResultDomainDataSource"] as DomainDataSource).Load();
        //                                                this.pO_DetailLoadDomainDataSource.Clear();
        //                                                this.pO_DetailLoadDomainDataSource.Load();
        //                                                //data["cboPONbr"] = strPONbr;
        //                                                busyIndicator.IsBusy = false;
        //                                                Change(false);
        //                                            }
        //                                        }, null);
        //                                    }
        //                                    else
        //                                    {
        //                                        CalCTNPCS();
        //                                        busyIndicator.IsBusy = false;
        //                                    }
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    busyIndicator.IsBusy = false;
        //                                    throw ex;
        //                                    //throw (ex);

        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                busyIndicator.IsBusy = false;
        //                                throw (ex);
        //                            }
        //                        };
        //                    };
        //                }
        //                //else HQMessageBox.Show()//Thong bao khong ton tai PONbr nay co le da xoa
        //            };



        //            return true;
        //        }
        //        return true;
        //    }

        //    catch (Exception ex)
        //    {
        //        busyIndicator.IsBusy = false;
        //        throw (ex);
        //        return false;
        //    }
        //}
        
        //private bool DeleteGrid(object sender, HQSLFramework.Message.HQMessageBox.ExitCode e)
        //{

        //    try
        //    {
        //        _complete = true;
        //        if (e == HQMessageBox.ExitCode.Yes)
        //        {
        //            busyIndicator.IsBusy = true;
        //            int i = lstDetail.Where(p => p.IsSelected == true && (p.Data as PO_DetailLoad_Result).DiscLineRef == (p.Data as PO_DetailLoad_Result).LineRef && (p.Data as PO_DetailLoad_Result).PurchaseType != "PR").Count();
        //            var query = (from p in db.GetPO_Detail where p.BranchID == data["cboBranchID"] && p.PONbr == data["cboPONbr"] select p);
        //            db.Load(query, LoadBehavior.RefreshCurrent, true).Completed += (sender1, args) =>
        //            {
        //                var query2 = (from p in db.GetIN_ItemSite select p);
        //                db.Load(query2, LoadBehavior.RefreshCurrent, true).Completed += (sender3, args3) =>
        //                {
        //                    try
        //                    {
        //                        while (lstDetail.Where(p => p.IsSelected == true && (p.Data as PO_DetailLoad_Result).DiscLineRef == (p.Data as PO_DetailLoad_Result).LineRef && (p.Data as PO_DetailLoad_Result).PurchaseType != "PR").Count() > 0)
        //                        {
        //                            if (lstDetail.Where(p => p.IsSelected == true && (p.Data as PO_DetailLoad_Result).DiscLineRef == (p.Data as PO_DetailLoad_Result).LineRef && (p.Data as PO_DetailLoad_Result).PurchaseType != "PR").FirstOrDefault() != null)
        //                            {

        //                                var r = lstDetail.Where(p => p.IsSelected == true && (p.Data as PO_DetailLoad_Result).DiscLineRef == (p.Data as PO_DetailLoad_Result).LineRef && (p.Data as PO_DetailLoad_Result).PurchaseType != "PR").FirstOrDefault().Data as PO_DetailLoad_Result;
        //                                while (lstDetail.Where(p => (p.Data as PO_DetailLoad_Result).DiscLineRef == r.LineRef).Count() > 0)
        //                                {
        //                                    var r1 = lstDetail.Where(p => (p.Data as PO_DetailLoad_Result).DiscLineRef == r.LineRef).FirstOrDefault().Data as PO_DetailLoad_Result;
        //                                    var obj1 = db.PO_Detail.Where(p => p.BranchID == data["cboBranchID"] && p.PONbr == data["cboPONbr"] && p.LineRef == r1.LineRef).FirstOrDefault();
        //                                    if (obj1 != null)
        //                                    {
        //                                        if (r.PurchaseType == "GI" || r.PurchaseType == "PR" || r.PurchaseType == "GP" || r.PurchaseType == "GS")
        //                                        {
        //                                            double OldQty = Math.Round((obj1.UnitMultDiv == "D" ? ((obj1.QtyOrd - obj1.QtyRcvd) / obj1.CnvFact) : (obj1.QtyOrd - obj1.QtyRcvd) * obj1.CnvFact));
        //                                            UpdateOnPOQty(obj1.InvtID, obj1.SiteID, OldQty, 0, 2);
        //                                        }

        //                                    }
        //                                    lstDetail.Remove(lstDetail.Where(p => (p.Data as PO_DetailLoad_Result).DiscLineRef == r.LineRef).FirstOrDefault());
        //                                    if (obj1 != null) db.PO_Details.Remove(obj1);
        //                                }

        //                            }
        //                        }
        //                        listCalcElement.ItemsSource = GridPO_Detail.ItemsSource;
        //                        if (cboPONbr.SelectedItem != null)
        //                        {
        //                            db.PO_Header, LoadBehavior.RefreshCurrent, true).Completed += (sender2, args2) =>
        //                            {
        //                                var obj = (from p in db.PO_Headers select p).Where(p => p.PONbr == data["cboPONbr"] && p.BranchID == data["cboBranchID"]).FirstOrDefault();
        //                                if (obj != null)
        //                                {
        //                                    if (obj.tstamp.ToHex() != objPO_Header.tstamp.ToHex())
        //                                    {
        //                                        if (_complete)
        //                                            HQMessageBox.Show(19, hqSys.LangID, null, HQmesscomplete);
        //                                        _complete = false;

        //                                        db.RejectChanges();
        //                                        busyIndicator.IsBusy = false;
        //                                        return;

        //                                    }
        //                                    UpdatingPO_Header(ref obj);
        //                                    strPONbr = obj.PONbr;
        //                                    objPO_Header = obj;
        //                                    //Save_PO_Detail();
        //                                    SubmitChange(false, null);
        //                                }
        //                                else busyIndicator.IsBusy = false;
        //                            };
        //                        }
        //                        else busyIndicator.IsBusy = false;

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        busyIndicator.IsBusy = false;
        //                        throw (ex);
        //                    }
        //                };
        //            };
        //            return true;

        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        busyIndicator.IsBusy = false;
        //        throw (ex);
        //        return false;
        //    }
        //}
       
        private void Save_Task(PO_Header header)
        {

        }
        private void Save_PO_Detail(PO_Header header, List<PO_DetailLoad_Result> lst)
        {
            int i = 0;
            try
            {
                for (i = 0; i < lst.Count; i++)
                {
                    PO_DetailLoad_Result objDetail = lst[i];
                    var objPO_Detail = db.PO_Detail.Where(p => p.BranchID == header.BranchID && p.PONbr == header.PONbr && p.LineRef == objDetail.LineRef).FirstOrDefault();
                    if (objPO_Detail == null)
                    {
                        objPO_Detail = clsApp.ResetPO_Detail();
                        UpdatingPO_Detail(objDetail, ref objPO_Detail);
                        objPO_Detail.Crtd_DateTime = DateTime.Now;
                        objPO_Detail.Crtd_Prog = screenNbr;
                        objPO_Detail.Crtd_User = Current.UserName;
                        objPO_Detail.ReqdDate = objPO_Detail.ReqdDate.ToDateShort();

                        db.PO_Detail.AddObject(objPO_Detail);
                    }
                    else
                    {
                        if (objPO_Detail.tstamp.ToHex() != objDetail.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        UpdatingPO_Detail(objDetail, ref objPO_Detail);
                    }
                }
                db.SaveChanges();
                //if (cboHandle.HandleToValue().Trim() == "N" || cboHandle.HandleToValue().Trim() == "")
                //    SubmitChange(false, null);
                //else
                //{
                //    header.Status = (cboHandle.SelectedItem as ppv_PO10100Handle_Result).ToStatus;
                //    SubmitChange(true, objPO_Header);
                //}


            }
            catch (Exception ex)
            {
                throw (ex);              
            }
        }
        


        private void UpdatingPO_Header(ref PO_Header objHeader, PO_Header _poHead, List<PO_DetailLoad_Result> lst)
        {

            try
            {
                objHeader.VouchStage =_poHead.VouchStage.PassNull();
                objHeader.POAmt = _poHead.POAmt.ToDouble();//.Value.ToDouble();//lstDetail.Sum(p => (p.Data as PO_DetailLoad_Result).POFee)+ lstDetail.Sum(p => (p.Data as PO_DetailLoad_Result).TaxAmt00) + lstDetail.Sum(p => (p.Data as PO_DetailLoad_Result).TaxAmt01) + lstDetail.Sum(p => (p.Data as PO_DetailLoad_Result).TaxAmt02) + lstDetail.Sum(p => (p.Data as PO_DetailLoad_Result).TaxAmt03) + lstDetail.Sum(p => (p.Data as PO_DetailLoad_Result).ExtCost);// Math.Round(double.Parse(this.txtPOAmt.Value == null ? "0" : this.txtPOAmt.Value.ToString()));
                objHeader.POFeeTot = lst.Sum(p => p.POFee);// Math.Round(double.Parse(this.txtPOFeeTot.Value == null ? "0" : this.txtPOFeeTot.Value.ToString()));
                //objHeader.FeeTransport = txtTotFeeTransport.Value.ToDouble();
                //objHeader.Surcharge = txtTotSurcharge.Value.ToDouble();
                //tap main
                objHeader.ReqNbr = _poHead.ReqNbr.PassNull();
                objHeader.VendID = _poHead.VendID.PassNull();
                objHeader.NoteID = 0;// this.HeaderNoteID;
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

                if (bStatusClose) objHeader.Status = "C";

                objHeader.LUpd_DateTime = DateTime.Now;
                objHeader.LUpd_Prog = screenNbr;
                objHeader.LUpd_User = Current.UserName;
                objHeader.tstamp = new byte[0];
                bStatusClose = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void UpdatingPO_Detail(PO_DetailLoad_Result objDetail, ref PO_Detail objrPO_Detail)
        {
            double OldQty = 0;
            double NewQty = 0;

            PO_DetailLoad_Result objDetailFirst = lstPODetailLoad.Where(p => p.BranchID == objDetail.BranchID && p.LineRef == objDetail.LineRef && p.PONbr == objDetail.PONbr).FirstOrDefault();

            IN_ItemSite objIN_ItemSite = new IN_ItemSite();
            try
            {
                if (objDetail.PurchaseType == "GI" || objDetail.PurchaseType == "PR" || objDetail.PurchaseType == "GP" || objDetail.PurchaseType == "GS")
                {
                    var objIN_Inventory = db.IN_Inventory.FirstOrDefault(p => p.InvtID == objDetail.InvtID);
                    if (objIN_Inventory == null)
                    {
                        objIN_Inventory = clsApp.ResetIN_Inventory();
                        objIN_Inventory.InvtID = objDetail.InvtID;
                    }
                 
                    try
                    {
                        objIN_ItemSite = db.IN_ItemSite.FirstOrDefault(p => p.InvtID == objDetail.InvtID && p.SiteID == objDetail.SiteID);
                        if (objIN_ItemSite == null)
                        {
                            objIN_ItemSite = new IN_ItemSite();
                            Insert_IN_ItemSite(ref objIN_ItemSite, ref objIN_Inventory, objDetail.SiteID);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }


                }
                if (objDetail.PurchaseType == "GI" || objDetail.PurchaseType == "PR" || objDetail.PurchaseType == "GP" || objDetail.PurchaseType == "GS")
                {
                    NewQty = Math.Round((objDetail.UnitMultDiv == "D" ? (objDetail.QtyOrd / objDetail.CnvFact) : (objDetail.QtyOrd * objDetail.CnvFact)));
                    if (objDetailFirst == null) OldQty = 0;
                    else
                        OldQty = Math.Round((objDetailFirst.UnitMultDiv == "D" ? (objDetailFirst.QtyOrd / objDetailFirst.CnvFact) : objDetailFirst.QtyOrd * objDetailFirst.CnvFact));
                    UpdateOnPOQty(objDetail.InvtID, objDetail.SiteID, OldQty, NewQty, 2);
                  
                }
                objrPO_Detail.BranchID = strbranchID;
                objrPO_Detail.PONbr = strponbr;
                objrPO_Detail.LineRef = objDetail.LineRef;

                objrPO_Detail.BlktLineID = objDetail.BlktLineID;
                objrPO_Detail.BlktLineRef = objDetail.BlktLineRef == null ? "" : objDetail.BlktLineRef;
                objrPO_Detail.CnvFact = (objDetail.CnvFact == 0 ? 1 : objDetail.CnvFact);

                objrPO_Detail.CostReceived = Math.Round(objDetail.CostReceived);
                objrPO_Detail.CostReturned = Math.Round(objDetail.CostReturned);
                objrPO_Detail.CostVouched = Math.Round(objDetail.CostVouched);
                objrPO_Detail.ExtCost = Math.Round(objDetail.ExtCost);
                objrPO_Detail.POFee = Math.Round(objDetail.POFee);
                objrPO_Detail.UnitCost = Math.Round(objDetail.UnitCost);

                objrPO_Detail.ExtWeight = objDetail.ExtWeight;
                objrPO_Detail.ExtVolume = objDetail.ExtVolume;
                objrPO_Detail.InvtID = objDetail.InvtID;
                objrPO_Detail.PromDate = objDetail.PromDate.ToDateShort();
                objrPO_Detail.PurchaseType = objDetail.PurchaseType;
                objrPO_Detail.PurchUnit = objDetail.PurchUnit;

                objrPO_Detail.QtyOrd = Math.Round(objDetail.QtyOrd);
                objrPO_Detail.QtyRcvd = Math.Round(objDetail.QtyRcvd);
                objrPO_Detail.QtyReturned = Math.Round(objDetail.QtyReturned);
                objrPO_Detail.QtyVouched = Math.Round(objDetail.QtyVouched);

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
                
                objrPO_Detail.TxblAmt00 = Math.Round(objDetail.TxblAmt00);
                objrPO_Detail.TxblAmt01 = Math.Round(objDetail.TxblAmt01);
                objrPO_Detail.TxblAmt02 = Math.Round(objDetail.TxblAmt02);
                objrPO_Detail.TxblAmt03 = Math.Round(objDetail.TxblAmt03);

                objrPO_Detail.TaxAmt00 = Math.Round(objDetail.TaxAmt00);
                objrPO_Detail.TaxAmt01 = Math.Round(objDetail.TaxAmt01);
                objrPO_Detail.TaxAmt02 = Math.Round(objDetail.TaxAmt02);
                objrPO_Detail.TaxAmt03 = Math.Round(objDetail.TaxAmt03);

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
                objrPO_Detail.LUpd_Prog =screenNbr;
                objrPO_Detail.LUpd_User = Current.UserName;
              
            }
            catch (Exception ex)
            {

                throw (ex);

            }
        }
        //private void SubmitChange(bool isSendMail, PO_Header objHeader)
        //{
        //    try
        //    {
        //        if (_complete)
        //        {
        //            this.db.SubmitChanges(OnSubmitCompleted1 =>
        //            {
        //                if (OnSubmitCompleted1.HasError)
        //                {
        //                    string message = "";

        //                    if (OnSubmitCompleted1.EntitiesInError.Any())
        //                    {
        //                        message = "";

        //                        Entity entityInError = OnSubmitCompleted1.EntitiesInError.First();
        //                        if (entityInError.ValidationErrors.Any())
        //                        {
        //                            message = entityInError.ValidationErrors.First().ErrorMessage;
        //                        }
        //                    }
        //                    busyIndicator.IsBusy = false;
                        

        //                    OnSubmitCompleted1.MarkErrorAsHandled();
        //                    throw new Exception(message);
        //                }
        //                else
        //                {
        //                    if (isSendMail) SendMail(objHeader);

        //                    (this.Resources["ppv_PO10100PONbr_Branch_All_ResultDomainDataSource"] as DomainDataSource).Clear();
        //                    (this.Resources["ppv_PO10100PONbr_Branch_All_ResultDomainDataSource"] as DomainDataSource).Load();
        //                    this.pO_DetailLoadDomainDataSource.Clear();
        //                    this.pO_DetailLoadDomainDataSource.Load();                         
        //                    busyIndicator.IsBusy = false;
        //                    Change(false);
        //                }
        //                CalCTNPCS();
        //            }, null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        busyIndicator.IsBusy = false;
        //        throw ex;              
        //    }
        //}
        //private bool Data_Checking()
        //{
        //    try
        //    {
        //        if (objPO_Setup == null)
        //        {
        //            if (_complete && cboBranchID.SelectedItem != null)
        //                HQMessageBox.Show(20404, hqSys.LangID, new string[] { "PO_Setup" }, HQmesscomplete);
        //            _complete = false;

        //            return false;
        //        }               
        //        if (data["cboPONbr"] == "" && objPO_Setup.AutoRef == 0)
        //        {
        //            if (_complete)
        //                HQMessageBox.Show(15, hqSys.LangID, new string[] { lblPONbr.Content.ToString() }, HQmesscomplete);
        //            _complete = false;

        //            cboPONbr.Focus();
        //            return false;
        //        }
        //        if (cboVendID.SelectedItem == null)
        //        {
        //            if (_complete)
        //                HQMessageBox.Show(15, hqSys.LangID, new string[] { lblVendID.Content.ToString() }, HQmesscomplete);
        //            _complete = false;

        //            cboVendID.Focus();
        //            return false;
        //        }
        //        if (cboDistAddr.SelectedItem == null)
        //        {
        //            if (_complete)
        //                HQMessageBox.Show(15, hqSys.LangID, new string[] { lblDistAddr.Content.ToString() }, HQmesscomplete);
        //            _complete = false;

        //            cboVendID.Focus();
        //            return false;
        //        }
        //        //Invalid data
        //        if (t["cboBranchID"].Length == 0 || this.cboVendID.SelectedItem == null || this.cboPOType.SelectedItem == null || this.cboStatus.SelectedItem == null)
        //        {
        //            if (_complete)
        //                HQMessageBox.Show(744, hqSys.LangID, null, HQmesscomplete);
        //            _complete = false;

        //            return false;
        //        }
        //        //Check PO has no detail data
        //        if (lstDetail.Count == 0)
        //        {
        //            if (_complete)
        //                HQMessageBox.Show(704, hqSys.LangID, null, HQmesscomplete);
        //            _complete = false;


        //            return false;
        //        }
        //        if (lstDetail.Where(p => string.IsNullOrEmpty((p.Data as PO_DetailLoad_Result).PurchUnit.PassNull())).Count() > 0)
        //        {
        //            if (_complete)
        //                HQMessageBox.Show(25, hqSys.LangID, null, HQmesscomplete);
        //            _complete = false;


        //            return false;
        //        }


        //        //Check MOQ
        //        AP_Vendor objVendor = new AP_Vendor();
        //        objVendor = aP_VendorDomainDataSource.Data.OfType<AP_Vendor>().Where(p => p.VendID == (cboVendID.SelectedItem as ppv_vendor_Result).VendID).FirstOrDefault();
        //        if (objVendor.MOQVal > 0)
        //        {
        //            switch (objVendor.MOQType)
        //            {
        //                case "Q":
        //                    if (double.Parse(this.txtQuantityTotal.Value == null ? "0" : this.txtQuantityTotal.Value.ToString()) < objVendor.MOQVal)
        //                    {
        //                        if (_complete)
        //                            HQMessageBox.Show(747, hqSys.LangID, new string[] { _lang.Where(p => p.Code.ToUpper() == "Quantity".ToUpper()).FirstOrDefault().Descr, objVendor.MOQVal.ToString() }, HQmesscomplete);
        //                        _complete = false;


        //                    }
        //                    break;
        //                case "V":
        //                    if (double.Parse(this.txtTotVol.Value == null ? "0" : this.txtTotVol.Value.ToString()) < objVendor.MOQVal)
        //                    {
        //                        if (_complete)
        //                            HQMessageBox.Show(747, hqSys.LangID, new string[] { _lang.Where(p => p.Code.ToUpper() == "Quantity".ToUpper()).FirstOrDefault().Descr, objVendor.MOQVal + "L" }, HQmesscomplete);
        //                        _complete = false;


        //                    }
        //                    break;
        //                case "A":
        //                    if (double.Parse(this.txtPOAmt.Value == null ? "0" : this.txtPOAmt.Value.ToString()) < objVendor.MOQVal)
        //                    {
        //                        if (_complete)
        //                            HQMessageBox.Show(747, hqSys.LangID, new string[] { _lang.Where(p => p.Code.ToUpper() == "Quantity".ToUpper()).FirstOrDefault().Descr, objVendor.MOQVal + "" }, HQmesscomplete);
        //                        _complete = false;


        //                    }
        //                    break;
        //                case "W":
        //                    if (double.Parse(this.txtTotWeight.Value == null ? "0" : this.txtTotWeight.Value.ToString()) < objVendor.MOQVal)
        //                    {
        //                        if (_complete)
        //                            HQMessageBox.Show(747, hqSys.LangID, new string[] { _lang.Where(p => p.Code.ToUpper() == "Quantity".ToUpper()).FirstOrDefault().Descr, objVendor.MOQVal + "KG" }, HQmesscomplete);
        //                        _complete = false;


        //                    }
        //                    break;
        //            }
        //        }
        //         if (objSysConfig != null)
        //        {
        //            if (objSysConfig.WrkDateChk)
        //            {
        //                int iWrkUpperDays = objSysConfig.WrkUpperDays;
        //                int iWrkLowerDays = objSysConfig.WrkLowerDays;
        //                System.DateTime dWrkAdjDate = default(System.DateTime);

        //                //Adjustment Date

        //                dWrkAdjDate = objSysConfig.WrkAdjDate;

        //                //Checking:
        //                t["PODate"] = t["PODate"] == null ? hqSys.BusinessDate : t["PODate"];
        //                if (!((((DateTime)t["PODate"].ToDateShort() >= objSysConfig.WrkOpenDate.Date.AddDays(-1 * iWrkLowerDays).ToDateShort() && (DateTime)t["PODate"].ToDateShort() <= objSysConfig.WrkOpenDate.ToDateShort()) || (((DateTime)t["PODate"].ToDateShort() <= objSysConfig.WrkOpenDate.Date.AddDays(iWrkUpperDays).ToDateShort()) && (DateTime)t["PODate"].ToDateShort() >= objSysConfig.WrkOpenDate.ToDateShort()) || (DateTime)t["PODate"].ToDateShort() == dWrkAdjDate.ToDateShort())))
        //                {
        //                    if (_complete)
        //                        HQMessageBox.Show(301, hqSys.LangID, null, HQmesscomplete);
        //                    _complete = false;

        //                    this.cboPODate.Focus();
        //                    return false;
        //                }
        //            }  
        //        for (Int16 i = 0; i <= lstDetail.Count - 1; i++)
        //        {
        //            var objDetail = lstDetail[i];

        //            if (objDetail.PurchaseType != "PR" && objDetail.ExtCost == 0)
        //            {
        //                if (_complete)
        //                    HQMessageBox.Show(44, hqSys.LangID, null, HQmesscomplete);
        //                _complete = false;

        //                return false;
        //            }
        //            if (string.IsNullOrEmpty(objDetail.PurchUnit))
        //            {
        //                if (_complete)
        //                    HQMessageBox.Show(15, hqSys.LangID, new string[] { GridPO_Detail.Columns["PurchUnit"].HeaderText.ToString() }, HQmesscomplete);
        //                _complete = false;

        //                return false;
        //            }
        //            if (string.IsNullOrEmpty(objDetail.SiteID))
        //            {
        //                if (_complete)
        //                    HQMessageBox.Show(15, hqSys.LangID, new string[] { GridPO_Detail.Columns["SiteID"].HeaderText.ToString() }, HQmesscomplete);
        //                _complete = false;

        //                return false;
        //            }
        //            if (string.IsNullOrEmpty(objDetail.PurchaseType))
        //            {
        //                if (_complete)
        //                    HQMessageBox.Show(15, hqSys.LangID, new string[] { GridPO_Detail.Columns["PurchaseType"].HeaderText.ToString() }, HQmesscomplete);
        //                _complete = false;

        //                return false;
        //            }
        //        }
        //        var lstDetail = GridPO_Detail.ItemsSource.OfType<PO_DetailLoad_Result>().ToList();
        //        string strInvt = "";
        //        var query = (from f in lstDetail
        //                     where f.PurchaseType != "PR"                            
        //                     group f by f.InvtID into g
        //                     let count = g.Count()
        //                     where count > 1
        //                     orderby count descending
        //                     select new CountInvtID
        //                     {
        //                         invtID = g.First().InvtID,
        //                         count = count
        //                     }).ToList();
        //        foreach (var obj in query)
        //        {
        //            strInvt += obj.invtID + ",";
        //        }
        //        if (strInvt != "")
        //        {
        //            if (_complete)
        //                HQMessageBox.Show(20417, hqSys.LangID, new string[] { strInvt.Substring(0, strInvt.Length - 1) }, HQmesscomplete);
        //            _complete = false;
        //        }              
        //        var lstremove = lstDetail.Where(p => (p.Data as PO_DetailLoad_Result).PurchaseType == "PR").ToList();
        //        while (lstDetail.Where(p => (p.Data as PO_DetailLoad_Result).PurchaseType == "PR" && (!string.IsNullOrEmpty((p.Data as PO_DetailLoad_Result).DiscLineRef)) && ((p.Data as PO_DetailLoad_Result).DiscLineRef != (p.Data as PO_DetailLoad_Result).LineRef)).Count() > 0)
        //        {
        //            var obj1 = lstDetail.Where(p => (p.Data as PO_DetailLoad_Result).PurchaseType == "PR" && (!string.IsNullOrEmpty((p.Data as PO_DetailLoad_Result).DiscLineRef)) && ((p.Data as PO_DetailLoad_Result).DiscLineRef != (p.Data as PO_DetailLoad_Result).LineRef)).FirstOrDefault();
        //            var obj = obj1.Data as PO_DetailLoad_Result;
        //            var objD = db.PO_Details.Where(p => p.PONbr == obj.PONbr && p.LineRef == obj.LineRef && p.BranchID == obj.BranchID && p.PurchaseType == "PR" && p.DiscLineRef == obj.DiscLineRef).FirstOrDefault();
        //            if (objD != null) db.PO_Details.Remove(objD);
        //            lstDetail.Remove(obj1);

        //        }
        //        for (Int16 i = 0; i <= lstDetail.Count - 1; i++)
        //        {
        //            var objDetail = lstDetail[i];

        //            if (objDetail.PurchaseType != "PR" && objDetail.ExtCost == 0)
        //            {
        //                if (_complete)
        //                    HQMessageBox.Show(44, hqSys.LangID, null, HQmesscomplete);
        //                _complete = false;

        //                return false;
        //            }
        //            if (objDetail.PurchaseType != "PR" && objDetail.DiscAmt == 0)
        //            {

        //                _lineRef = LastLineRef();
        //                double DiscAmt = 0;
        //                double DiscPct = 0;
        //                string discSeq = string.Empty;
        //                string discID = string.Empty;
        //                string budgetID = string.Empty;
        //                string breaklineRef = string.Empty;
        //                GetDiscLineSetup(objDetail, objDetail.QtyOrd, objDetail.ExtCost, ref DiscAmt, ref DiscPct, ref discSeq, ref discID, ref budgetID, ref breaklineRef);
        //                objDetail.DiscAmt = DiscAmt;
        //                objDetail.DiscPct = DiscPct;
        //                objDetail.DiscSeq = discSeq;
        //                objDetail.DiscID = discID;
        //                objDetail.ExtCost = objDetail.QtyOrd * objDetail.UnitCost - objDetail.DiscAmt;               
        //            }
        //        }
        //        FreeItemForLine(ref _lineRef, "");            
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw (ex);
        //        return false;
        //    }
        //}
        #endregion
        #region Data Processing
        //private void SendMail(PO_Header objHeader)
        //{
        //    _SendMail.SendMailApprove(cboBranchID.BranchIDToValue(), objHeader.PONbr, hqSys.ScreenNumber, hqSys.CpnyID, cboStatus.StatusToValue(), cboHandle.HandleToValue(), _roles, hqSys.UserName, hqSys.LangID).Completed += (sender6, arg6) =>
        //    {
        //        InvokeOperation approve = (sender6 as InvokeOperation);
        //        if (approve.HasError)
        //        {
        //            approve.MarkErrorAsHandled();
        //            //ErrorWindow.CreateNew(approve.Error);

        //            busyIndicator.IsBusy = false;
        //            throw new Exception(approve.Error.Message);
        //        }
        //    };
        //}
     
        #endregion


        private bool Data_Checking()
        {
            try
            {
              
                if (objPO_Setup == null)
                {
                    throw new MessageException(MessageType.Message, "20404",
                      parm: new[] { "PO_Setup" });

                }

                if (_poHead.PONbr.PassNull() == "" && objPO_Setup.AutoRef == 0)
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
                if (lstPODetailLoad.Count == 0)
                {
                    throw new MessageException(MessageType.Message, "704");


                }
                if (lstPODetailLoad.Where(p => string.IsNullOrEmpty(p.PurchUnit.PassNull())).Count() > 0)
                {

                    throw new MessageException(MessageType.Message, "25");

                }
                if (db.checkPODate(strbranchID, _poHead.PODate.ToDateShort(), screenNbr,strponbr).FirstOrDefault() == "0")
                    throw new MessageException(MessageType.Message, "201302041",
                            parm: new[] { _poHead.PODate.ToShortDateString() });
               
                //Check MOQ
                AP_Vendor objVendor = new AP_Vendor();
                objVendor = db.AP_Vendor.ToList().FirstOrDefault(p => p.VendID == _poHead.VendID.PassNull());
                if (objVendor.MOQVal > 0)
                {
                    switch (objVendor.MOQType)
                    {
                        case "Q":
                            if (lstPODetailLoad.Sum(p=>p.QtyOrd*p.CnvFact)< objVendor.MOQVal)
                            {
                                throw new MessageException(MessageType.Message, "747",
                                    parm: new[] { Util.GetLang("Quantity"), objVendor.MOQVal.ToString() });



                            }
                            break;
                        case "V":
                            if (lstPODetailLoad.Sum(p => p.ExtVolume) < objVendor.MOQVal)
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
                            if (lstPODetailLoad.Sum(p => p.ExtWeight) < objVendor.MOQVal)
                            {
                                throw new MessageException(MessageType.Message, "747",
                                      parm: new[] { Util.GetLang("TotWeight"), objVendor.MOQVal.ToString() + "KG" });




                            }
                            break;
                    }
                }

                if (db.checkCloseDate(strbranchID, _poHead.PODate.ToDateShort(), screenNbr).FirstOrDefault() == "0")
                    throw new MessageException(MessageType.Message, "301");


                for (Int16 i = 0; i <= lstPODetailLoad.Count - 1; i++)
                {
                    var objDetail = lstPODetailLoad[i];

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
                var query = (from f in lstPODetailLoad
                             where f.PurchaseType != "PR"
                             //join l in lstPO_DetailLoad on f.InvtID equals l.InvtID
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

                var lstremove = lstPODetailLoad.Where(p => p.PurchaseType == "PR").ToList();
                while (lstPODetailLoad.Where(p => p.PurchaseType == "PR" && (!string.IsNullOrEmpty(p.DiscLineRef)) && (p.DiscLineRef != p.LineRef)).Count() > 0)
                {
                    var obj1 = lstPODetailLoad.FirstOrDefault(p => p.PurchaseType == "PR" && (!string.IsNullOrEmpty(p.DiscLineRef)) && (p.DiscLineRef != p.LineRef));
                    var obj = obj1;
                    var objD = db.PO_Detail.FirstOrDefault(p => p.PONbr == obj.PONbr && p.LineRef == obj.LineRef && p.BranchID == obj.BranchID && p.PurchaseType == "PR" && p.DiscLineRef == obj.DiscLineRef);
                    if (objD != null) db.PO_Detail.DeleteObject(objD);
                    lstPODetailLoad.Remove(obj1);

                }
                for (Int16 i = 0; i <= lstPODetailLoad.Count - 1; i++)
                {
                    var objDetail = lstPODetailLoad[i];

                    if (objDetail.PurchaseType != "PR" && objDetail.ExtCost == 0)
                    {
                        throw new MessageException(MessageType.Message, "44");
                    }
                    if (objDetail.PurchaseType != "PR" && objDetail.DiscAmt == 0)
                    {

                        _lineRef = LastLineRef(lstPODetailLoad);
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
            catch (Exception ex)
            {
                throw (ex);
            }
        }
       
        #endregion
        #region Other
        public void Insert_IN_ItemSite(ref IN_ItemSite objIN_ItemSite, ref IN_Inventory objIN_Inventory, string SiteID)
        {
            try
            {
                objIN_ItemSite = clsApp.ResetIN_ItemSite();
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
                objIN_ItemSite.Crtd_DateTime = DateTime.Now;
                objIN_ItemSite.Crtd_Prog = screenNbr;
                objIN_ItemSite.Crtd_User = Current.UserName;
                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                objIN_ItemSite.LUpd_Prog = screenNbr;
                objIN_ItemSite.LUpd_User = Current.UserName;
                objIN_ItemSite.tstamp = new byte[0];
                db.IN_ItemSite.AddObject(objIN_ItemSite);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void UpdateOnPOQty(string InvtID, string SiteID, double OldQty, double NewQty, int DecQty)
        {
            IN_ItemSite objItemSite = new IN_ItemSite();
            try
            {             
                try
                {
                    objItemSite = db.IN_ItemSite.FirstOrDefault(p => p.InvtID == InvtID && p.SiteID == SiteID);
                    if (objItemSite != null)
                    {
                        objItemSite.QtyOnPO = Math.Round(objItemSite.QtyOnPO + NewQty - OldQty, DecQty);
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }                
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private string LastLineRef(List<PO_DetailLoad_Result> lst)
        {
            string strlineRef = "";
            int ilineRef = 0;
            strlineRef = lst.Count == 0 ? "00000" : lst.OrderByDescending(p => p.LineRef).FirstOrDefault().LineRef;
            ilineRef = int.Parse(strlineRef) + 1;
            strlineRef = ("00000" + ilineRef);
            strlineRef = strlineRef.Substring(strlineRef.Length - 5, 5);
            return strlineRef;
        }
        private double GetDiscLineSetup(PO_DetailLoad_Result det, double qty, double amt, ref double discAmt, ref double discPct, ref string discSeq, ref string discID, ref string budgetID, ref string breaklineRef)
        {
            double discItemUnitQty = 0;
            
            var lstsetup =db.OM_DiscAllByBranchPO(strbranchID).Where(p => p.DiscType == "L").ToList();
            if (lstsetup.Count > 0)
            {
                foreach (var setup in lstsetup)
                {
                    var objDisc = setup;// (from p in _PO10100Context.OM_Discounts where p.DiscType == "L" && p.Status == "C" && p.POUse == true select p).FirstOrDefault();
                    var objCpnyID = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.CpnyID == strbranchID && p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim()).FirstOrDefault();
                    if (objCpnyID != null)
                    {
                        if (objDisc != null)
                        {
                            if (objDisc.DiscType == "L")
                            {
                                var lstSeq = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.Status.ToUpper().Trim() == "C" && p.Active == 1 && p.POUse == true && ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0 && p.Promo == 0) || ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0) && (DateTime.Compare(_poHead.PODate.ToDateShort(), p.POEndDate) <= 0) && p.Promo == 1))).ToList();
                                if (lstSeq.Count > 0)
                                {
                                    foreach (var seq in lstSeq)
                                    {
                                        var objItem = lstOM_DiscAllByBranchPO_ResultALL.Where(p =>
                                                           p.DiscID == objDisc.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() &&
                                                           p.InvtID == det.InvtID
                                                           ).FirstOrDefault();
                                        if (objItem == null) objItem = new OM_DiscAllByBranchPO_Result();
                                        var objInvt = lstIN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
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
                                            var objtmpItem = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == seq.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() && p.InvtID == det.InvtID).FirstOrDefault();
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
            var objSeq = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == discSeq.ToUpper().Trim()).FirstOrDefault();
            if (objSeq == null) objSeq = new OM_DiscAllByBranchPO_Result();
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
            OM_DiscAllByBranchPO_Result objBreak = new OM_DiscAllByBranchPO_Result();
            if (discBreak == "A")
                objBreak = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == discSeq.ToUpper().Trim() && p.BreakAmt <= qtyAmt && p.BreakAmt > 0).OrderByDescending(p => p.BreakAmt).FirstOrDefault();
            else
                objBreak = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == discSeq.ToUpper().Trim() && p.BreakQty <= qtyAmt && p.BreakQty > 0).OrderByDescending(p => p.BreakQty).FirstOrDefault();

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
            var objInvt = lstIN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt != null)
            {
                var cnv = lstUnitConversion.Where(p => p.InvtID == invtID && p.FromUnit == unitDesc && p.ToUnit == objInvt.StkUnit).FirstOrDefault();
                if (cnv == null)
                    cnv = lstUnitConversion.Where(p => p.InvtID == invtID && p.FromUnit == objInvt.StkUnit && p.ToUnit == unitDesc).FirstOrDefault();
                else if (cnv == null)
                    cnv =// iN_UnitConversionDomainDataSource.Data.OfType<IN_UnitConversion>()
                        lstUnitConversion.Where(p => p.UnitType == "3" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == invtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == unitDesc.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == objInvt.StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                else if (cnv == null)
                    cnv =// iN_UnitConversionDomainDataSource.Data.OfType<IN_UnitConversion>()
                        lstUnitConversion.Where(p => p.UnitType == "2" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == invtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == unitDesc.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == objInvt.StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                else if (cnv == null)
                    cnv =// iN_UnitConversionDomainDataSource.Data.OfType<IN_UnitConversion>()
                        lstUnitConversion.Where(p => p.UnitType == "1" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == "*" && p.FromUnit.PassNull().Trim().ToUpper() == unitDesc.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == objInvt.StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();

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
            _lstTmpPO_DetailLoad = lstPODetailLoad;
            _countAddItem = 0;
            _freeLineRunning = true;
            foreach (var det in _lstTmpPO_DetailLoad)
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
            List<OM_DiscAllByBranchPO_Result> lstDiscSeq;
            List<OM_DiscAllByBranchPO_Result> lstDiscFreeItem1;
            var lstSetup = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscType == "L").ToList();
            foreach (var setup in lstSetup)
            {

                var objDisc = setup;// (from p in _PO10100Context.OM_Discounts where p.DiscType == "L" && p.Status == "C" && p.POUse == true select p).FirstOrDefault();
                //var objCpnyID = lstOM_DiscAllByBranchPO_ResultALL.Where(p=>p.CpnyID == strbranchID && p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() select p).FirstOrDefault();
                //if (objCpnyID != null)
                //{
                if (objDisc != null)
                {
                    if (objDisc.DiscType == "L")
                    {
                        _poHead.PODate = _poHead.PODate == null ? DateTime.Now.ToDateShort() : _poHead.PODate;
                        lstDiscSeq = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.Status == "C" && p.Active == 1 && p.POUse == true && ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0 && p.Promo == 0) || ((DateTime.Compare(_poHead.PODate.ToDateShort(), p.POStartDate) >= 0) && (DateTime.Compare(_poHead.PODate.ToDateShort(), p.POEndDate) <= 0) && p.Promo == 1))).ToList();
                        foreach (var seq in lstDiscSeq)
                        {
                            calcDisc = false;
                            var objInvt = lstIN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                            OM_DiscAllByBranchPO_Result objItem = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim()).FirstOrDefault();

                            if (objItem == null) objItem = new OM_DiscAllByBranchPO_Result();
                            double cnvFact = 0;
                            string unitMultDiv = "";
                            discItemUntiQty = (qty * OM_GetCnvFactFromUnit(invtID, unit, ref cnvFact, ref unitMultDiv).ToInt());// objItem.UnitDesc)).ToInt();

                            lstDiscFreeItem1 = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == objDisc.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim()).ToList();

                            if (seq.BreakBy == "A")
                                qtyAmt = amt;
                            else if (seq.BreakBy == "W")
                            {
                                objInvt = lstIN_Inventory.Where(p => p.InvtID == invtID).FirstOrDefault();
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
                                var objTmpItem = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == seq.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() && p.InvtID == invtID).FirstOrDefault();
                                if (objTmpItem != null)
                                {
                                    calcDisc = true;
                                    goto Calc;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && seq.Active != 0 && objDisc.DiscClass == "PP")
                            {
                                objInvt = lstIN_Inventory.Where(p => p.InvtID == invtID).FirstOrDefault();
                                var objDiscItemClass = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == seq.DiscID.ToUpper().Trim() && p.DiscSeq.ToUpper().Trim() == seq.DiscSeq.ToUpper().Trim() && p.ClassID.ToUpper().Trim() == objInvt.PriceClassID.ToUpper().Trim()).FirstOrDefault();
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
                                var lstDiscFreeItem1tmp = lstOM_DiscAllByBranchPO_ResultALL.Where(p =>
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
                                            objInvt = lstIN_Inventory.Where(p => p.InvtID == freeItemID1).FirstOrDefault();
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
            var objDisc = lstOM_DiscAllByBranchPO_ResultALL.Where(p => p.DiscID.ToUpper().Trim() == discID.ToUpper().Trim()).FirstOrDefault();
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
                var objInvt = lstIN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID);
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

                
                
                PO_DetailLoad_Result newdet = clsApp.ResetPO_DetailLoad_Result();
                newdet.PONbr = strponbr;
                newdet.BranchID = strbranchID;
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
                lstPODetailLoad.Add(newdet);
            }
        }
        public bool SetUOM(ref double Cnvfact, ref string UnitMultDiv, string InvtID, string ClassID, string StkUnit, string FromUnit)
        {
            Cnvfact = 0;
            IN_UnitConversion objIN_UnitConversion = new IN_UnitConversion();
            try
            {
                if (!string.IsNullOrEmpty(FromUnit))
                {
                    objIN_UnitConversion =// iN_UnitConversionDomainDataSource.Data.OfType<IN_UnitConversion>()
                        lstUnitConversion.Where(p => p.UnitType == "3" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == InvtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == FromUnit.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                    if (objIN_UnitConversion == null)
                        objIN_UnitConversion = //iN_UnitConversionDomainDataSource.Data.OfType<IN_UnitConversion>()
                            lstUnitConversion.Where(p => p.UnitType == "2" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == InvtID.PassNull().Trim().ToUpper() && p.FromUnit.PassNull().Trim().ToUpper() == FromUnit.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
                    if (objIN_UnitConversion == null)
                        objIN_UnitConversion = //iN_UnitConversionDomainDataSource.Data.OfType<IN_UnitConversion>()
                            lstUnitConversion.Where(p => p.UnitType == "1" && p.ClassID == "*" && p.InvtID.PassNull().Trim().ToUpper() == "*" && p.FromUnit.PassNull().Trim().ToUpper() == FromUnit.PassNull().Trim().ToUpper() && p.ToUnit.PassNull().Trim().ToUpper() == StkUnit.PassNull().Trim().ToUpper()).FirstOrDefault();
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
      
        #endregion
    }
    public class CountInvtID
    {
        public int count { get; set; }
        public string invtID { get; set; }
    }
}

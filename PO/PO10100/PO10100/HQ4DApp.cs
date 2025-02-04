﻿using System;
using System.Net;

namespace PO10100
{
    public class HQ4DApp
    {     
        private PO_Header _objPO_Header = new PO_Header();
        private IN_Inventory _objIN_Inventory = new IN_Inventory();
        private IN_ItemSite _objIN_ItemSite = new IN_ItemSite();
        private PO_Detail _objPO_Detail = new PO_Detail();
        private PO10100_pgDetail_Result _objPO10100_pgDetail_Result = new PO10100_pgDetail_Result();         
        #region "Public Methods"
        public IN_ItemSite ResetIN_ItemSite()
        {
            _objIN_ItemSite = new IN_ItemSite(); 
            _objIN_ItemSite.InvtID = "";
            _objIN_ItemSite.SiteID = "";
            _objIN_ItemSite.AvgCost = 0;
            _objIN_ItemSite.QtyAlloc = 0;
            _objIN_ItemSite.QtyAllocIN = 0;
            _objIN_ItemSite.QtyAllocPORet = 0;
            _objIN_ItemSite.QtyAllocSO = 0;
            _objIN_ItemSite.QtyAvail = 0;
            _objIN_ItemSite.QtyInTransit = 0;
            _objIN_ItemSite.QtyOnBO = 0;
            _objIN_ItemSite.QtyOnHand = 0;
            _objIN_ItemSite.QtyOnPO = 0;
            _objIN_ItemSite.QtyOnTransferOrders = 0;
            _objIN_ItemSite.QtyOnSO = 0;
            _objIN_ItemSite.QtyShipNotInv = 0;
            _objIN_ItemSite.StkItem = 0;
            _objIN_ItemSite.TotCost = 0;
            _objIN_ItemSite.LastPurchaseDate = DateTime.Now;
            _objIN_ItemSite.Crtd_DateTime = DateTime.Today;
            _objIN_ItemSite.Crtd_Prog = "";
            _objIN_ItemSite.Crtd_User = "";
            _objIN_ItemSite.LUpd_DateTime = DateTime.Today;
            _objIN_ItemSite.LUpd_Prog = "";
            _objIN_ItemSite.LUpd_User = "";
            _objIN_ItemSite.tstamp = new byte[0];
            _objIN_ItemSite.QtyUncosted = 0;
            return _objIN_ItemSite;
        }
        public IN_Inventory ResetIN_Inventory()
        {
            _objIN_Inventory = new IN_Inventory();
            _objIN_Inventory.InvtID = "";
            _objIN_Inventory.BarCode = "";
            _objIN_Inventory.Buyer = "";
            _objIN_Inventory.ClassID = "";
            _objIN_Inventory.Color = "";
            _objIN_Inventory.Descr = "";
            _objIN_Inventory.Descr1 = "";
            _objIN_Inventory.DfltPOUnit = "";
            _objIN_Inventory.DfltSite = "";
            _objIN_Inventory.DfltSOUnit = "";
            _objIN_Inventory.Exported = 0;
            _objIN_Inventory.InvtType = "";
            _objIN_Inventory.IRSftyStkDays = 0;
            _objIN_Inventory.IRSftyStkPct = 0;
            _objIN_Inventory.IRSftyStkQty = 0;
            _objIN_Inventory.IROverStkQty = 0;
            _objIN_Inventory.LastCost = 0;
            _objIN_Inventory.LossRate00 = 0;
            _objIN_Inventory.LossRate01 = 0;
            _objIN_Inventory.LossRate02 = 0;
            _objIN_Inventory.LossRate03 = 0;
            _objIN_Inventory.LotSerFxdLen = 0;
            _objIN_Inventory.LotSerFxdTyp = "";
            _objIN_Inventory.LotSerFxdVal = "";
            _objIN_Inventory.LotSerIssMthd = "";
            _objIN_Inventory.LotSerNumLen = 0;
            _objIN_Inventory.LotSerNumVal = "";
            _objIN_Inventory.LotSerTrack = "";
            _objIN_Inventory.MaterialType = "";
            _objIN_Inventory.NodeID = "";
            _objIN_Inventory.NodeLevel = 0;
            _objIN_Inventory.ParentRecordID = 0;
            _objIN_Inventory.Picture = "";
            _objIN_Inventory.POFee = 0;
            _objIN_Inventory.POPrice = 0;
            _objIN_Inventory.PrePayPct = 0;
            _objIN_Inventory.PriceClassID = "";
            _objIN_Inventory.SerAssign = "";
            _objIN_Inventory.ShelfLife = 0;
            _objIN_Inventory.Size = "";
            _objIN_Inventory.SOFee = 0;
            _objIN_Inventory.SOPrice = 0;
            _objIN_Inventory.Source = "";
            _objIN_Inventory.Status = "";
            _objIN_Inventory.StkItem = 0;
            _objIN_Inventory.StkUnit = "";
            _objIN_Inventory.StkVol = 0;
            _objIN_Inventory.StkWt = 0;
            _objIN_Inventory.StkWtUnit = "";
            _objIN_Inventory.Style = "";
            _objIN_Inventory.TaxCat = "";
            _objIN_Inventory.ValMthd = "";
            _objIN_Inventory.VendID1 = "";
            _objIN_Inventory.VendID2 = "";
            _objIN_Inventory.WarrantyDays = 0;
            _objIN_Inventory.Crtd_DateTime = DateTime.Today;
            _objIN_Inventory.Crtd_Prog = "";
            _objIN_Inventory.Crtd_User = "";
            _objIN_Inventory.LUpd_DateTime = DateTime.Today;
            _objIN_Inventory.LUpd_Prog = "";
            _objIN_Inventory.LUpd_User = "";
            _objIN_Inventory.tstamp = new byte[0];
            return _objIN_Inventory;
        }    
        public PO_Header ResetPO_Header()
        {
            _objPO_Header = new PO_Header(); 
            _objPO_Header.BranchID = "";
            _objPO_Header.PONbr = "";
            _objPO_Header.ShipDistAddrID = "";
            _objPO_Header.BillShipAddr = 0;
            _objPO_Header.BlktExprDate = DateTime.Today;
            _objPO_Header.BlktPONbr = "";
            _objPO_Header.Buyer = "";
            _objPO_Header.ImpExp = "";
            _objPO_Header.NoteID = 0;
            _objPO_Header.POAmt = 0;
            _objPO_Header.POFeeTot = 0;
            _objPO_Header.PODate = DateTime.Today;
            _objPO_Header.POType = "";
            _objPO_Header.RcptStage = "";
            _objPO_Header.RcptTotAmt = 0;
            _objPO_Header.ReqNbr = "";
            _objPO_Header.ShipAddr1 = "";
            _objPO_Header.ShipAddr2 = "";
            _objPO_Header.ShipAddrID = "";
            _objPO_Header.ShipAttn = "";
            _objPO_Header.ShipCity = "";
            _objPO_Header.ShipCountry = "";
            _objPO_Header.ShipCustID = "";
            _objPO_Header.ShipEmail = "";
            _objPO_Header.ShipFax = "";
            _objPO_Header.ShipName = "";
            _objPO_Header.ShipPhone = "";
            _objPO_Header.ShipSiteID = "";
            _objPO_Header.ShipState = "";
            _objPO_Header.ShiptoID = "";
            _objPO_Header.ShiptoType = "";
            _objPO_Header.ShipVendAddrID = "";
            _objPO_Header.ShipVendID = "";
            _objPO_Header.ShipVia = "";
            _objPO_Header.ShipZip = "";
            _objPO_Header.Status = "";
            _objPO_Header.Terms = "";
            _objPO_Header.VendAddr1 = "";
            _objPO_Header.VendAddr2 = "";
            _objPO_Header.VendAddrID = "";
            _objPO_Header.VendAttn = "";
            _objPO_Header.VendCity = "";
            _objPO_Header.VendCountry = "";
            _objPO_Header.VendEmail = "";
            _objPO_Header.VendFax = "";
            _objPO_Header.VendID = "";
            _objPO_Header.VendName = "";
            _objPO_Header.VendPhone = "";
            _objPO_Header.VendState = "";
            _objPO_Header.VendZip = "";
            _objPO_Header.VouchStage = "";
            _objPO_Header.Crtd_DateTime = DateTime.Today;
            _objPO_Header.Crtd_Prog = "";
            _objPO_Header.Crtd_User = "";
            _objPO_Header.LUpd_DateTime = DateTime.Today;
            _objPO_Header.LUpd_Prog = "";
            _objPO_Header.LUpd_User = "";
            _objPO_Header.IsExport = false;
            _objPO_Header.tstamp = new byte[0];
            return _objPO_Header;
        }
        public PO_Detail ResetPO_Detail()
        {
            _objPO_Detail = new PO_Detail();
            _objPO_Detail.BranchID = "";
            _objPO_Detail.PONbr = "";
            _objPO_Detail.LineRef = "";
            _objPO_Detail.BlktLineID = 0;
            _objPO_Detail.BlktLineRef = "";
            _objPO_Detail.CnvFact = 0;
            _objPO_Detail.CostReceived = 0;
            _objPO_Detail.CostReturned = 0;
            _objPO_Detail.CostVouched = 0;
            _objPO_Detail.ExtCost = 0;
            _objPO_Detail.ExtWeight = 0;
            _objPO_Detail.ExtVolume = 0;
            _objPO_Detail.InvtID = "";
            _objPO_Detail.POFee = 0;
            _objPO_Detail.PromDate = DateTime.Today;
            _objPO_Detail.PurchaseType = "";
            _objPO_Detail.PurchUnit = "";
            _objPO_Detail.QtyOrd = 0;
            _objPO_Detail.QtyRcvd = 0;
            _objPO_Detail.QtyReturned = 0;
            _objPO_Detail.QtyVouched = 0;
            _objPO_Detail.RcptStage = "";
            _objPO_Detail.ReasonCd = "";
            _objPO_Detail.ReqdDate = DateTime.Today;
            _objPO_Detail.SiteID = "";
            _objPO_Detail.TaxCat = "";
            _objPO_Detail.TranDesc = "";
            _objPO_Detail.UnitCost = 0;
            _objPO_Detail.UnitMultDiv = "";
            _objPO_Detail.UnitWeight = 0;
            _objPO_Detail.UnitVolume = 0;
            _objPO_Detail.VouchStage = "";
            _objPO_Detail.TaxAmt00 = 0;
            _objPO_Detail.TaxAmt01 = 0;
            _objPO_Detail.TaxAmt02 = 0;
            _objPO_Detail.TaxAmt03 = 0;
            _objPO_Detail.TaxID00 = "";
            _objPO_Detail.TaxID00 = "";
            _objPO_Detail.TaxID00 = "";
            _objPO_Detail.TaxID00 = "";
            _objPO_Detail.DiscAmt = 0;
            _objPO_Detail.DiscPct = 0;
            _objPO_Detail.DiscID = "";
            _objPO_Detail.DiscSeq = "";

            _objPO_Detail.Crtd_DateTime = DateTime.Today;
            _objPO_Detail.Crtd_Prog = "";
            _objPO_Detail.Crtd_User = "";
            _objPO_Detail.LUpd_DateTime = DateTime.Today;
            _objPO_Detail.LUpd_Prog = "";
            _objPO_Detail.LUpd_User = "";
            _objPO_Detail.tstamp = new byte[0];
            return _objPO_Detail;
        }
        public PO10100_pgDetail_Result ResetPO10100_pgDetail_Result()
        {
            _objPO10100_pgDetail_Result = new PO10100_pgDetail_Result();
            _objPO10100_pgDetail_Result.BranchID = "";
            _objPO10100_pgDetail_Result.PONbr = "";
            _objPO10100_pgDetail_Result.LineRef = "";
            _objPO10100_pgDetail_Result.BlktLineID = 0;
            _objPO10100_pgDetail_Result.BlktLineRef = "";
            _objPO10100_pgDetail_Result.CnvFact = 0;
            _objPO10100_pgDetail_Result.CostReceived = 0;
            _objPO10100_pgDetail_Result.CostReturned = 0;
            _objPO10100_pgDetail_Result.CostVouched = 0;
            _objPO10100_pgDetail_Result.ExtCost = 0;
            _objPO10100_pgDetail_Result.ExtWeight = 0;
            _objPO10100_pgDetail_Result.ExtVolume = 0;
            _objPO10100_pgDetail_Result.InvtID = "";
            _objPO10100_pgDetail_Result.POFee = 0;
            _objPO10100_pgDetail_Result.PromDate = DateTime.Today;
            _objPO10100_pgDetail_Result.PurchaseType = "";
            _objPO10100_pgDetail_Result.PurchUnit = "";
            _objPO10100_pgDetail_Result.QtyOrd = 0;
            _objPO10100_pgDetail_Result.QtyRcvd = 0;
            _objPO10100_pgDetail_Result.QtyReturned = 0;
            _objPO10100_pgDetail_Result.QtyVouched = 0;
            _objPO10100_pgDetail_Result.RcptStage = "";
            _objPO10100_pgDetail_Result.ReasonCd = "";
            _objPO10100_pgDetail_Result.ReqdDate = DateTime.Today;
            _objPO10100_pgDetail_Result.SiteID = "";
            _objPO10100_pgDetail_Result.TaxCat = "";
            _objPO10100_pgDetail_Result.TaxID = "";
            _objPO10100_pgDetail_Result.TranDesc = "";
            _objPO10100_pgDetail_Result.UnitCost = 0;
            _objPO10100_pgDetail_Result.UnitMultDiv = "";
            _objPO10100_pgDetail_Result.UnitWeight = 0;
            _objPO10100_pgDetail_Result.UnitVolume = 0;
            _objPO10100_pgDetail_Result.VouchStage = "";
            _objPO10100_pgDetail_Result.TaxAmt00 = 0;
            _objPO10100_pgDetail_Result.TaxAmt01 = 0;
            _objPO10100_pgDetail_Result.TaxAmt02 = 0;
            _objPO10100_pgDetail_Result.TaxAmt03 = 0;
            _objPO10100_pgDetail_Result.TaxID00 = "";
            _objPO10100_pgDetail_Result.TaxID00 = "";
            _objPO10100_pgDetail_Result.TaxID00 = "";
            _objPO10100_pgDetail_Result.TaxID00 = "";
            _objPO10100_pgDetail_Result.DiscAmt = 0;
            _objPO10100_pgDetail_Result.DiscPct = 0;
            _objPO10100_pgDetail_Result.DiscID = "";
            _objPO10100_pgDetail_Result.DiscSeq = "";         
            return _objPO10100_pgDetail_Result;
        }
        #endregion
    }
}

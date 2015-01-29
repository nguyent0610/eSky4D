using System;
using System.Net;

namespace PO10100
{
    public class HQ4DApp
    {
     
        private PO_Header objPO_Header = new PO_Header();
        private IN_Inventory objIN_Inventory = new IN_Inventory();
        private IN_ItemSite objIN_ItemSite = new IN_ItemSite();
        private PO_Detail objPO_Detail = new PO_Detail();
        private PO_DetailLoad_Result objPO_DetailLoad_Result = new PO_DetailLoad_Result(); 
        
        #region "Public Methods"
        public IN_ItemSite ResetIN_ItemSite()
        {
            objIN_ItemSite = new IN_ItemSite(); 
            objIN_ItemSite.InvtID = "";
            objIN_ItemSite.SiteID = "";
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
            objIN_ItemSite.StkItem = 0;
            objIN_ItemSite.TotCost = 0;
            objIN_ItemSite.LastPurchaseDate = DateTime.Now;
            objIN_ItemSite.Crtd_DateTime = DateTime.Today;
            objIN_ItemSite.Crtd_Prog = "";
            objIN_ItemSite.Crtd_User = "";
            objIN_ItemSite.LUpd_DateTime = DateTime.Today;
            objIN_ItemSite.LUpd_Prog = "";
            objIN_ItemSite.LUpd_User = "";
            objIN_ItemSite.tstamp = new byte[0];
            objIN_ItemSite.QtyUncosted = 0;
            return objIN_ItemSite;
        }

        public IN_Inventory ResetIN_Inventory()
        {
            objIN_Inventory = new IN_Inventory(); 
            objIN_Inventory.InvtID = "";
            objIN_Inventory.BarCode = "";
            objIN_Inventory.Buyer = "";
            objIN_Inventory.ClassID = "";
            objIN_Inventory.Color = "";
            objIN_Inventory.Descr = "";
            objIN_Inventory.Descr1 = "";
            objIN_Inventory.DfltPOUnit = "";
            objIN_Inventory.DfltSite = "";
            objIN_Inventory.DfltSOUnit = "";
            objIN_Inventory.Exported = 0;
            objIN_Inventory.InvtType = "";
            objIN_Inventory.IRSftyStkDays = 0;
            objIN_Inventory.IRSftyStkPct = 0;
            objIN_Inventory.IRSftyStkQty = 0;
            objIN_Inventory.IROverStkQty = 0;
            objIN_Inventory.LastCost = 0;
            objIN_Inventory.LossRate00 = 0;
            objIN_Inventory.LossRate01 = 0;
            objIN_Inventory.LossRate02 = 0;
            objIN_Inventory.LossRate03 = 0;
            objIN_Inventory.LotSerFxdLen = 0;
            objIN_Inventory.LotSerFxdTyp = "";
            objIN_Inventory.LotSerFxdVal = "";
            objIN_Inventory.LotSerIssMthd = "";
            objIN_Inventory.LotSerNumLen = 0;
            objIN_Inventory.LotSerNumVal = "";
            objIN_Inventory.LotSerTrack = "";
            objIN_Inventory.MaterialType = "";
            objIN_Inventory.NodeID = "";
            objIN_Inventory.NodeLevel = 0;
            objIN_Inventory.ParentRecordID = 0;
            objIN_Inventory.Picture = "";
            objIN_Inventory.POFee = 0;
            objIN_Inventory.POPrice = 0;
            objIN_Inventory.PrePayPct = 0;
            objIN_Inventory.PriceClassID = "";
            objIN_Inventory.SerAssign = "";
            objIN_Inventory.ShelfLife = 0;
            objIN_Inventory.Size = "";
            objIN_Inventory.SOFee = 0;
            objIN_Inventory.SOPrice = 0;
            objIN_Inventory.Source = "";
            objIN_Inventory.Status = "";
            objIN_Inventory.StkItem = 0;
            objIN_Inventory.StkUnit = "";
            objIN_Inventory.StkVol = 0;
            objIN_Inventory.StkWt = 0;
            objIN_Inventory.StkWtUnit = "";
            objIN_Inventory.Style = "";
            objIN_Inventory.TaxCat = "";
            objIN_Inventory.ValMthd = "";
            objIN_Inventory.VendID1 = "";
            objIN_Inventory.VendID2 = "";
            objIN_Inventory.WarrantyDays = 0;
            objIN_Inventory.Crtd_DateTime = DateTime.Today;
            objIN_Inventory.Crtd_Prog = "";
            objIN_Inventory.Crtd_User = "";
            objIN_Inventory.LUpd_DateTime = DateTime.Today;
            objIN_Inventory.LUpd_Prog = "";
            objIN_Inventory.LUpd_User = "";
            objIN_Inventory.tstamp = new byte[0];
            return objIN_Inventory;
        }

    

        public PO_Header ResetPO_Header()
        {
            objPO_Header = new PO_Header(); 
            objPO_Header.BranchID = "";
            objPO_Header.PONbr = "";
            objPO_Header.ShipDistAddrID = "";
            objPO_Header.BillShipAddr = 0;
            objPO_Header.BlktExprDate = DateTime.Today;
            objPO_Header.BlktPONbr = "";
            objPO_Header.Buyer = "";
            objPO_Header.ImpExp = "";
            objPO_Header.NoteID = 0;
            objPO_Header.POAmt = 0;
            objPO_Header.POFeeTot = 0;
            objPO_Header.PODate = DateTime.Today;
            objPO_Header.POType = "";
            objPO_Header.RcptStage = "";
            objPO_Header.RcptTotAmt = 0;
            objPO_Header.ReqNbr = "";
            objPO_Header.ShipAddr1 = "";
            objPO_Header.ShipAddr2 = "";
            objPO_Header.ShipAddrID = "";
            objPO_Header.ShipAttn = "";
            objPO_Header.ShipCity = "";
            objPO_Header.ShipCountry = "";
            objPO_Header.ShipCustID = "";
            objPO_Header.ShipEmail = "";
            objPO_Header.ShipFax = "";
            objPO_Header.ShipName = "";
            objPO_Header.ShipPhone = "";
            objPO_Header.ShipSiteID = "";
            objPO_Header.ShipState = "";
            objPO_Header.ShiptoID = "";
            objPO_Header.ShiptoType = "";
            objPO_Header.ShipVendAddrID = "";
            objPO_Header.ShipVendID = "";
            objPO_Header.ShipVia = "";
            objPO_Header.ShipZip = "";
            objPO_Header.Status = "";
            objPO_Header.Terms = "";
            objPO_Header.VendAddr1 = "";
            objPO_Header.VendAddr2 = "";
            objPO_Header.VendAddrID = "";
            objPO_Header.VendAttn = "";
            objPO_Header.VendCity = "";
            objPO_Header.VendCountry = "";
            objPO_Header.VendEmail = "";
            objPO_Header.VendFax = "";
            objPO_Header.VendID = "";
            objPO_Header.VendName = "";
            objPO_Header.VendPhone = "";
            objPO_Header.VendState = "";
            objPO_Header.VendZip = "";
            objPO_Header.VouchStage = "";
            objPO_Header.Crtd_DateTime = DateTime.Today;
            objPO_Header.Crtd_Prog = "";
            objPO_Header.Crtd_User = "";
            objPO_Header.LUpd_DateTime = DateTime.Today;
            objPO_Header.LUpd_Prog = "";
            objPO_Header.LUpd_User = "";
            objPO_Header.IsExport = false;
            objPO_Header.tstamp = new byte[0];
            return objPO_Header;
        }
        public PO_Detail ResetPO_Detail()
        {
            objPO_Detail = new PO_Detail();
            objPO_Detail.BranchID = "";
            objPO_Detail.PONbr = "";
            objPO_Detail.LineRef = "";
            objPO_Detail.BlktLineID = 0;
            objPO_Detail.BlktLineRef = "";
            objPO_Detail.CnvFact = 0;
            objPO_Detail.CostReceived = 0;
            objPO_Detail.CostReturned = 0;
            objPO_Detail.CostVouched = 0;
            objPO_Detail.ExtCost = 0;
            objPO_Detail.ExtWeight = 0;
            objPO_Detail.ExtVolume = 0;
            objPO_Detail.InvtID = "";
            objPO_Detail.POFee = 0;
            objPO_Detail.PromDate = DateTime.Today;
            objPO_Detail.PurchaseType = "";
            objPO_Detail.PurchUnit = "";
            objPO_Detail.QtyOrd = 0;
            objPO_Detail.QtyRcvd = 0;
            objPO_Detail.QtyReturned = 0;
            objPO_Detail.QtyVouched = 0;
            objPO_Detail.RcptStage = "";
            objPO_Detail.ReasonCd = "";
            objPO_Detail.ReqdDate = DateTime.Today;
            objPO_Detail.SiteID = "";
            objPO_Detail.TaxCat = "";
            objPO_Detail.TranDesc = "";
            objPO_Detail.UnitCost = 0;
            objPO_Detail.UnitMultDiv = "";
            objPO_Detail.UnitWeight = 0;
            objPO_Detail.UnitVolume = 0;
            objPO_Detail.VouchStage = "";
            objPO_Detail.TaxAmt00 = 0;
            objPO_Detail.TaxAmt01 = 0;
            objPO_Detail.TaxAmt02 = 0;
            objPO_Detail.TaxAmt03 = 0;
            objPO_Detail.TaxID00 = "";
            objPO_Detail.TaxID00 = "";
            objPO_Detail.TaxID00 = "";
            objPO_Detail.TaxID00 = "";
            objPO_Detail.DiscAmt = 0;
            objPO_Detail.DiscPct = 0;
            objPO_Detail.DiscID = "";
            objPO_Detail.DiscSeq = "";

            objPO_Detail.Crtd_DateTime = DateTime.Today;
            objPO_Detail.Crtd_Prog = "";
            objPO_Detail.Crtd_User = "";
            objPO_Detail.LUpd_DateTime = DateTime.Today;
            objPO_Detail.LUpd_Prog = "";
            objPO_Detail.LUpd_User = "";
            objPO_Detail.tstamp = new byte[0];
            return objPO_Detail;
        }
        public PO_DetailLoad_Result ResetPO_DetailLoad_Result()
        {
            objPO_DetailLoad_Result = new PO_DetailLoad_Result();
            objPO_DetailLoad_Result.BranchID = "";
            objPO_DetailLoad_Result.PONbr = "";
            objPO_DetailLoad_Result.LineRef = "";
            objPO_DetailLoad_Result.BlktLineID = 0;
            objPO_DetailLoad_Result.BlktLineRef = "";
            objPO_DetailLoad_Result.CnvFact = 0;
            objPO_DetailLoad_Result.CostReceived = 0;
            objPO_DetailLoad_Result.CostReturned = 0;
            objPO_DetailLoad_Result.CostVouched = 0;
            objPO_DetailLoad_Result.ExtCost = 0;
            objPO_DetailLoad_Result.ExtWeight = 0;
            objPO_DetailLoad_Result.ExtVolume = 0;
            objPO_DetailLoad_Result.InvtID = "";
            objPO_DetailLoad_Result.POFee = 0;
            objPO_DetailLoad_Result.PromDate = DateTime.Today;
            objPO_DetailLoad_Result.PurchaseType = "";
            objPO_DetailLoad_Result.PurchUnit = "";
            objPO_DetailLoad_Result.QtyOrd = 0;
            objPO_DetailLoad_Result.QtyRcvd = 0;
            objPO_DetailLoad_Result.QtyReturned = 0;
            objPO_DetailLoad_Result.QtyVouched = 0;
            objPO_DetailLoad_Result.RcptStage = "";
            objPO_DetailLoad_Result.ReasonCd = "";
            objPO_DetailLoad_Result.ReqdDate = DateTime.Today;
            objPO_DetailLoad_Result.SiteID = "";
            objPO_DetailLoad_Result.TaxCat = "";
            objPO_DetailLoad_Result.TaxID = "";
            objPO_DetailLoad_Result.TranDesc = "";
            objPO_DetailLoad_Result.UnitCost = 0;
            objPO_DetailLoad_Result.UnitMultDiv = "";
            objPO_DetailLoad_Result.UnitWeight = 0;
            objPO_DetailLoad_Result.UnitVolume = 0;
            objPO_DetailLoad_Result.VouchStage = "";
            objPO_DetailLoad_Result.TaxAmt00 = 0;
            objPO_DetailLoad_Result.TaxAmt01 = 0;
            objPO_DetailLoad_Result.TaxAmt02 = 0;
            objPO_DetailLoad_Result.TaxAmt03 = 0;
            objPO_DetailLoad_Result.TaxID00 = "";
            objPO_DetailLoad_Result.TaxID00 = "";
            objPO_DetailLoad_Result.TaxID00 = "";
            objPO_DetailLoad_Result.TaxID00 = "";
            objPO_DetailLoad_Result.DiscAmt = 0;
            objPO_DetailLoad_Result.DiscPct = 0;
            objPO_DetailLoad_Result.DiscID = "";
            objPO_DetailLoad_Result.DiscSeq = "";         
            return objPO_DetailLoad_Result;
        }
        #endregion
    }
}

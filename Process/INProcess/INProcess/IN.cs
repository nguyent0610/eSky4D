﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using HQFramework.DAL;
using eBiz4DApp;
using HQ.eSkyFramework;
using HQFramework.Common;
namespace INProcess
{
    public class IN 
    {
      
        public string Prog { get; set; }
        public string User { get; set; }
        public DataAccess Dal { get; set; }
        public List<MessageException> LogList { get; set; }
        public int QtyDecimalPlaces = 0;
        public int CostDecimalPlaces = 0;

        public IN(string User, string prog,DataAccess dal)
        {
            this.User = User;
            this.Prog = prog;
            this.Dal = dal;
        }
        public bool IN10100_Release(string branchID, string batNbr, bool isTransfer, string transferNbr)
        {

            clsSQL sql = new clsSQL(Dal);
            clsIN_Setup setup = new clsIN_Setup(Dal);
            clsIN_Trans tran = new clsIN_Trans(Dal);
            clsIN_Inventory inventory = new clsIN_Inventory(Dal);
            clsIN_ItemSite itemSite = new clsIN_ItemSite(Dal);
            clsIN_ItemLoc itemLoc;
            clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
            clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
            clsIN_ItemPack objItemPack = new clsIN_ItemPack(Dal);
            setup.GetByKey(branchID, "IN");
            DataTable trans = tran.GetAll(branchID, batNbr, "%", "%");
            double qty = 0;
            string User = string.Empty;
            string prog = string.Empty;
            string refNbr = string.Empty;
            DateTime? tranDate = DateTime.Now;
            if (trans.Rows.Count > 0)
            {
                User = trans.Rows[0].String("LUpd_User");
                refNbr = trans.Rows[0].String("LUpd_User");
                prog = trans.Rows[0].String("LUpd_Prog");
                tranDate = trans.Rows[0].Date("TranDate");
            }
         
            foreach (DataRow inTran in trans.Rows)
            {
                    
                inventory.GetByKey(inTran.String("InvtID"));
                #region -Save IN_itemSite-                                
                if (!itemSite.GetByKey(inTran.String("InvtID"),inTran.String("SiteID")))
                {
                    Insert_IN_ItemSite(ref itemSite, inTran.String("InvtID"), inventory.StkItem, inTran.String("SiteID"), 0);
                }
                if (inventory.StkItem == 1)
                {
                    if (inTran.String("UnitMultDiv") == "M" || inTran.String("UnitMultDiv") == string.Empty)
                        qty = inTran.Double("Qty") * inTran.Short("InvtMult") * inTran.Double("CnvFact");
                    else
                        qty = inTran.Double("Qty") * inTran.Short("InvtMult") / inTran.Double("CnvFact");

                    var decimalPlace = GetDecimalPlace(inventory.LotSerTrack);

                    if (isTransfer) itemSite.QtyInTransit -= qty;
                    itemSite.QtyOnHand = MathRound(itemSite.QtyOnHand + qty, decimalPlace);
                    itemSite.QtyAvail = MathRound(itemSite.QtyAvail + qty, decimalPlace);
                    itemSite.AvgCost = MathRound(itemSite.QtyOnHand > 0 ? (itemSite.TotCost + inTran.Double("ExtCost")) / itemSite.QtyOnHand : itemSite.AvgCost, CostDecimalPlaces);                    
                }
                itemSite.TotCost = MathRound(itemSite.TotCost + inTran.Double("ExtCost"), CostDecimalPlaces);
                itemSite.LUpd_DateTime = DateTime.Now;
                itemSite.LUpd_Prog = inTran.String("LUpd_Prog");
                itemSite.LUpd_User = inTran.String("LUpd_User");
                itemSite.Update();
                #endregion

                #region -Save IN_itemLoc-
                if (!string.IsNullOrWhiteSpace(inTran.String("WhseLoc")))
                {
                    itemLoc = new clsIN_ItemLoc(Dal);
                    if (!itemLoc.GetByKey(inTran.String("InvtID"), inTran.String("SiteID"), inTran.String("WhseLoc")))
                    {
                        Insert_IN_ItemLoc(ref itemLoc, inTran.String("InvtID"), inventory.StkItem, inTran.String("SiteID"), inTran.String("WhseLoc"), 0);
                    }
                    if (inventory.StkItem == 1)
                    {
                        if (inTran.String("UnitMultDiv") == "M" || inTran.String("UnitMultDiv") == string.Empty)
                            qty = inTran.Double("Qty") * inTran.Short("InvtMult") * inTran.Double("CnvFact");
                        else
                            qty = inTran.Double("Qty") * inTran.Short("InvtMult") / inTran.Double("CnvFact");

                        var decimalPlace = GetDecimalPlace(inventory.LotSerTrack);

                        if (isTransfer) itemLoc.QtyInTransit -= qty;
                        itemLoc.QtyOnHand = MathRound(itemLoc.QtyOnHand + qty, decimalPlace);
                        itemLoc.QtyAvail = MathRound(itemLoc.QtyAvail + qty, decimalPlace);
                        itemLoc.AvgCost = MathRound(itemLoc.QtyOnHand > 0 ? (itemLoc.TotCost + inTran.Double("ExtCost")) / itemLoc.QtyOnHand : itemLoc.AvgCost, CostDecimalPlaces);
                    }
                    itemLoc.TotCost = MathRound(itemLoc.TotCost + inTran.Double("ExtCost"), CostDecimalPlaces);
                    itemLoc.LUpd_DateTime = DateTime.Now;
                    itemLoc.LUpd_Prog = inTran.String("LUpd_Prog");
                    itemLoc.LUpd_User = inTran.String("LUpd_User");
                    itemLoc.Update();
                }
                #endregion

                #region -IN_ItemLot-
                if (inventory.StkItem == 1 && inventory.LotSerTrack != "N" && inventory.PassNull()!=string.Empty)
                {
                    DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", inTran.String("LineRef"));
                    foreach (DataRow lotRow in dtLot.Rows)
                    {
                        var decimalPlace = GetDecimalPlace(inventory.LotSerTrack);
/// Check lai cho lam tron nay
                        qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace);

                        if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                        {
                            objItemLot.Reset();
                            objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                            objItemLot.InvtID = lotRow.String("InvtID");
                            objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                            objItemLot.SiteID = lotRow.String("SiteID");
                            objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                            objItemLot.LIFODate = lotRow.Date("ExpDate");
                            objItemLot.ExpDate = lotRow.Date("ExpDate"); 
                            objItemLot.Crtd_DateTime = DateTime.Now;
                            objItemLot.Crtd_Prog = Prog;
                            objItemLot.Crtd_User = User;
                            objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Add();
                            // Thêm vào table IN_ItemPack dùng cho hàng dây thừng
                            if (inventory.LotSerTrack == "Q")
                            {
                                sql.Insert_IN_ItemPack(branchID, batNbr, Prog, User, lotRow.String("RefNbr"), objItemLot.LotSerNbr, lotRow.String("INTranLineRef"));
                                if (objItemPack.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr")))
                                {
                                    objItemLot.PackageID = objItemPack.PackageID;
                                }
                            }
                        }

                        objItemLot.ExpDate = lotRow.Date("ExpDate");
                        
                        objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);
                        objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                        objItemLot.Cost = itemSite.AvgCost * objItemLot.QtyOnHand;

                        objItemLot.LUpd_DateTime = DateTime.Now;
                        objItemLot.LUpd_Prog = Prog;
                        objItemLot.LUpd_User = User;
                        objItemLot.Update();
                    }
                }
                #endregion
                
                
            }
            if (isTransfer)
            {
                sql.IN10100_UpdateTransfer(branchID,batNbr,Prog,User,tranDate,refNbr,transferNbr,"R");
            }
            sql.IN_ReleaseBatch(branchID, batNbr, Prog, User);
            return true;
            
        }
        public bool IN10100_Cancel(string branchID, string batNbr, bool isTransfer, string transferNbr, bool isCopy)
        {
            try
            {
                if (Receipt_Cancel(branchID, batNbr, string.Empty, isTransfer, transferNbr, true) && isCopy)
                {
                    clsBatch objBatch = new clsBatch(Dal);
                    if (objBatch.GetByKey(branchID,"IN",batNbr))
                    {
                        clsBatch newBatch = new clsBatch(Dal)
                                             {
                                                 BranchID = objBatch.BranchID,
                                                 DateEnt = objBatch.DateEnt,
                                                 Descr = objBatch.Descr,
                                                 EditScrnNbr = objBatch.EditScrnNbr,
                                                 FromToSiteID = objBatch.FromToSiteID,
                                                 ImpExp = objBatch.ImpExp,
                                                 IntRefNbr = objBatch.IntRefNbr,
                                                 JrnlType = objBatch.JrnlType,
                                                 Module1 = objBatch.Module1,
                                                 NoteID = objBatch.NoteID,
                                                 TotAmt = objBatch.TotAmt,
                                                 Status = "H",
                                                 RvdBatNbr = objBatch.BatNbr,
                                                 Rlsed = 0,
                                                 ReasonCD = objBatch.ReasonCD,
                                                 OrigScrnNbr = objBatch.OrigScrnNbr,
                                                 OrigBatNbr = objBatch.OrigBatNbr,
                                                 OrigBranchID = objBatch.OrigBranchID
                                             };
                        clsSQL sql = new clsSQL(Dal);
                        newBatch.BatNbr = sql.INNumbering(branchID, "BatNbr");
                        newBatch.RefNbr = sql.INNumbering(branchID, "RefNbr");
                        newBatch.LUpd_DateTime = newBatch.Crtd_DateTime = DateTime.Now;
                        newBatch.LUpd_Prog = newBatch.Crtd_Prog = Prog;
                        newBatch.LUpd_User = newBatch.Crtd_User = User;
                        newBatch.Add();

                        clsIN_Trans objTran = new clsIN_Trans(Dal);
                        DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                        foreach (DataRow tran in lstTrans.Rows)
                        {
                            clsIN_Trans newTran = new clsIN_Trans(Dal)
                                                   {
                                                       JrnlType = tran.String("JrnlType"),
                                                       ReasonCD = tran.String("ReasonCD"),
                                                       RefNbr = newBatch.RefNbr,
                                                       Rlsed = 0,
                                                       BatNbr = newBatch.BatNbr,
                                                       BranchID = newBatch.BranchID,
                                                       CnvFact = tran.Double("CnvFact"),
                                                       CostID = tran.String("CostID"),
                                                       ExtCost = tran.Double("ExtCost"),
                                                       FreeItem = tran.Bool("FreeItem"),
                                                       InvtID = tran.String("InvtID"),
                                                       InvtMult = tran.Short("InvtMult"),
                                                       LineRef = tran.String("LineRef"),
                                                       ObjID = tran["ObjID"].ToString(),
                                                       Qty = tran.Double("Qty"),
                                                       SiteID = tran.String("SiteID"),
                                                       QtyUncosted = tran.Double("QtyUncosted"),
                                                       ToSiteID = tran.String("ToSiteID"),
                                                       SlsperID = tran.String("SlsperID"),
                                                       ShipperID = tran.String("ShipperID"),
                                                       UnitPrice = tran.Double("UnitPrice"),
                                                       UnitMultDiv = tran.String("UnitMultDiv"),
                                                       UnitDesc = tran.String("UnitDesc"),
                                                       UnitCost = tran.Double("UnitCost"),
                                                       TranType = tran.String("TranType"),
                                                       TranFee = tran.Double("TranFee"),
                                                       TranAmt = tran.Double("TranAmt"),
                                                       TranDesc = tran.String("TranDesc"),
                                                       TranDate = tran.Date("TranDate"),
                                                       ShipperLineRef = tran.String("ShipperLineRef")
                                                   };
                            newTran.LUpd_DateTime = newTran.Crtd_DateTime = DateTime.Now;
                            newTran.LUpd_Prog = newTran.Crtd_Prog = Prog;
                            newTran.LUpd_User = newTran.Crtd_User = User;
                            newTran.Add();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IN10200_Release(string branchID, string batNbr)
        {
            try
            {
                clsSQL sql = new clsSQL(Dal);
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);

                double qty = 0;
                string lineRef = string.Empty;

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");
             
                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                //clsIN_ItemLoc objItemLoc = new clsIN_ItemLoc(Dal);
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                try
                {
                    lineRef  = (Convert.ToInt32(lstTrans.Compute("Max(LineRef)", "").ToString())+1).ToString();
                    for (int l = lineRef.Length; l < 5; l++)
                        lineRef = "0" + lineRef;
                }
                catch (Exception)
                {
                    lineRef = "00001";
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objSite = new clsIN_ItemSite(Dal);
                clsIN_ItemLoc objLoc;
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID"));
                    if (tran.String("WhseLoc").PassNull() != "")
                    {
                        objLoc = new clsIN_ItemLoc(Dal);
                        objLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc"));
                    }
                    if (objInvt.ValMthd == "F" || objInvt.ValMthd == "L")
                    {
                        int negQty = 1;
                        double qtyCost = 0;
                        DataTable lstFL = new DataTable();

                        if (objInvt.ValMthd == "F")
                            lstFL = sql.GetListFIFOCost(tran.String("InvtID"), tran.String("SiteID"));
                        else if (objInvt.ValMthd == "L")
                            lstFL = sql.GetListLIFOCost(tran.String("InvtID"), tran.String("SiteID"));

                        if (tran.Double("Qty") > 0)
                            negQty = 1;
                        else
                            negQty = -1;

                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = Math.Abs(tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact"));
                        else
                            qty = Math.Abs(tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact"));                        
                        if (lstFL.Rows.Count > 0)
                        {
                            int count = 0;
                            foreach (DataRow fl in lstFL.Rows)
                            {
                                double qtyCal = 0, qtyUpdate = 0;
                                if (qtyCost < qty)
                                {
                                    if (fl.Double("Qty") <= qty - qtyCost)
                                        qtyCal = fl.Double("Qty");
                                    else
                                        qtyCal = qty - qtyCost;

                                    if (tran.String("UnitMultDiv") == "D")
                                        qtyUpdate = negQty * qtyCal * tran.Double("CnvFact");
                                    else
                                        qtyUpdate = (negQty * qtyCal) / tran.Double("CnvFact");

                                    objTran.GetByKey(branchID, batNbr, tran.String("RefNbr"), tran.String("LineRef"));                        
                                    objTran.Qty = qtyUpdate;
                                    objTran.UnitCost = fl.Double("UnitCost");
                                    objTran.ExtCost = MathRound(qtyCal * fl.Double("UnitCost"), CostDecimalPlaces);
                                    objTran.TranAmt = MathRound(qtyUpdate * tran.Double("UnitPrice"), CostDecimalPlaces);
                                    objTran.CostID= fl.String("CostID");

                                    if (count == 0)
                                    {
                                        objTran.Update();
                                    }
                                    else
                                    {
                                        objTran.LineRef = lineRef;
                                        objTran.Add();
                                        lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                                        for (int i = lineRef.Length; lineRef.Length < 5; )
                                            lineRef = "0" + lineRef;

                                    }
                                    qtyCost = qtyCost + fl.Double("Qty");
                                    cost.GetByKey(fl.Int("CostIdentity"));
                                    cost.Qty= MathRound(cost.Qty - qtyCal, QtyDecimalPlaces);
                                    cost.TotCost = MathRound(cost.TotCost - qtyCal * cost.UnitCost, CostDecimalPlaces);
                                    if (cost.Qty == 0 && cost.TotCost == 0)
                                        cost.Delete(fl.Int("CostIdentity"));
                                    else
                                        cost.Update();
                                }
                                else
                                {
                                    break;
                                }
                                count++;
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "738");
                        }

                    }

                }
                qty = 0;
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    if (!objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message,"606");
                    }
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);
                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                    }
                    if (!objSetup.NegQty && MathRound(objSite.QtyOnHand + qty, decimalPlace) < 0)
                    {
                        throw new MessageException(MessageType.Message, "608","", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    if (tran.String("JrnlType") == "IN" || (tran.String("JrnlType") == "OM" && Prog == "OM10300"))
                    {
                        objSite.QtyAllocIN = MathRound(objSite.QtyAllocIN + qty, decimalPlace);
                        objSite.QtyOnHand = MathRound(objSite.QtyOnHand + qty, decimalPlace);
                        objSite.AvgCost = MathRound(objSite.QtyOnHand != 0
                                           ? (objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objSite.QtyOnHand
                                           : objSite.AvgCost, CostDecimalPlaces);
                    }
                    else if (tran.String("JrnlType") == "OM" || Prog != "OM10300")
                    {
                        objSite.QtyShipNotInv = MathRound(objSite.QtyShipNotInv + (tran.String("TranType") == "IN" ? qty : 0), decimalPlace);
                        objSite.QtyOnHand = MathRound(objSite.QtyOnHand + qty, decimalPlace);
                        objSite.QtyAvail = MathRound(objSite.QtyAvail + (tran.String("TranType") == "CM" ? qty : 0), decimalPlace);
                    }
                    if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607","", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.TotCost = MathRound(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                    objSite.LUpd_DateTime = DateTime.Now;
                    objSite.LUpd_Prog = Prog;
                    objSite.LUpd_User = User;
                    objSite.Update();


                    ///////////////////////////////////////////////////////////Cập nhật vào table IN_ItemLoc

                    if (tran.String("WhseLoc").PassNull()!="")
                    {
                        objLoc = new clsIN_ItemLoc(Dal);
                        if (!objLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc").PassNull()))
                        {
                            throw new MessageException(MessageType.Message, "606");
                        }

                        if (objInvt.StkItem == 1)
                        {
                            if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                                qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                            else
                                qty = tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                        }
                        if (!objSetup.NegQty && MathRound(objLoc.QtyOnHand + qty, decimalPlace) < 0)
                        {
                            throw new MessageException(MessageType.Message, "2018051411", "", new[] { objLoc.InvtID, objLoc.SiteID, objLoc.WhseLoc });
                        }

                        if (tran.String("JrnlType") == "IN" || (tran.String("JrnlType") == "OM" && Prog == "OM10300"))
                        {
                            objLoc.QtyAllocIN = MathRound(objLoc.QtyAllocIN + qty, decimalPlace);
                            objLoc.QtyOnHand = MathRound(objLoc.QtyOnHand + qty, decimalPlace);
                            objLoc.AvgCost = MathRound(objLoc.QtyOnHand != 0
                                               ? (objLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objLoc.QtyOnHand
                                               : objLoc.AvgCost, CostDecimalPlaces);
                        }
                        else if (tran.String("JrnlType") == "OM" || Prog != "OM10300")
                        {
                            objLoc.QtyShipNotInv = MathRound(objLoc.QtyShipNotInv + (tran.String("TranType") == "IN" ? qty : 0), decimalPlace);
                            objLoc.QtyOnHand = MathRound(objLoc.QtyOnHand + qty, decimalPlace);
                            objLoc.QtyAvail = MathRound(objLoc.QtyAvail + (tran.String("TranType") == "CM" ? qty : 0), decimalPlace);
                        }
                        if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces) < 0)
                        {
                            throw new MessageException(MessageType.Message, "2018051411", "", new[] { objLoc.InvtID, objLoc.SiteID, objLoc.WhseLoc });
                        }

                        objLoc.TotCost = MathRound(objLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        objLoc.LUpd_DateTime = DateTime.Now;
                        objLoc.LUpd_Prog = Prog;
                        objLoc.LUpd_User = User;
                        objLoc.Update();
                    }
                    

                    ///////////////////////////////////////

                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            // check lai cho lam tron nay
                            qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.ExpDate = lotRow.Date("ExpDate");
                                objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");

                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }



                            if (!objSetup.NegQty && objItemLot.QtyOnHand + qty < 0)
                            {
                                throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID + "-" + objItemLot.LotSerNbr });
                            }

                            if (tran.String("JrnlType") == "IN" || (tran.String("JrnlType") == "OM" && Prog == "OM10300"))
                            {
                                objItemLot.QtyAllocIN = MathRound(objItemLot.QtyAllocIN + qty, decimalPlace);
                                objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                              
                            }
                            else if (tran.String("JrnlType") == "OM" || Prog != "OM10300")
                            {
                                objItemLot.QtyShipNotInv = MathRound(objItemLot.QtyShipNotInv + (lotRow.String("TranType") == "IN" ? qty : 0), decimalPlace);
                                objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                                objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + (lotRow.String("TranType") == "CM" ? qty : 0), decimalPlace);
                            }

                            objItemLot.Cost = objSite.TotCost * objItemLot.QtyOnHand;
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();
                        }
                    }

                    if (objInvt.ValMthd == "S")
                    {
                        sql.GetCostByCostID(ref cost,tran.String("InvtID"),tran.String("SiteID"),tran.String("CostID"));
                        if(cost.CostIdentity>0)
                        {
                            cost.TotCost= MathRound(cost.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                            if(cost.Qty==0 && cost.TotCost==0) 
                                sql.DelCostByCostID(tran.String("InvtID"),tran.String("SiteID"),tran.String("CostID"));
                            else
                                cost.Update();
                        }
                    }
                }
                sql.IN_ReleaseBatch(branchID, batNbr, Prog, User);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool IN10200_Cancel(string branchID, string batNbr)
        {
            try
            {
                Issue_Cancel(branchID, batNbr, string.Empty, true);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool IN10300_Release(string branchID, string batNbr)
        {
            try
            {
                double qty = 0;
                string lineRef = string.Empty;

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");

                clsIN_Transfer objTranfer = new clsIN_Transfer(Dal);
                DataTable lstTransfer = objTranfer.GetAll(branchID, batNbr, "%");
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                string User = string.Empty;
                string prog = string.Empty;
                if (lstTransfer.Rows.Count > 0)
                {
                    User = lstTransfer.Rows[0].String("LUpd_User");
                    prog = lstTransfer.Rows[0].String("LUpd_Prog");
                }

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);

                clsIN_ItemLoc objItemLoc = new clsIN_ItemLoc(Dal); 
                clsSQL sql = new clsSQL(Dal);

                foreach (DataRow transfer in lstTransfer.Rows)
                {
                    DataTable lstTrans = objTran.GetAll(branchID, batNbr, transfer.String("RefNbr"), "%");
                    lineRef = string.Empty;
                    foreach (DataRow tran in lstTrans.Rows)
                    {
                        objInvt.GetByKey(tran["InvtID"].ToString());
                        var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);
                        if (!objItem.GetByKey(tran["InvtID"].ToString(),tran["SiteID"].ToString()))
                        {
                            throw new MessageException(MessageType.Message, "606");
                        }
                        if (objInvt.StkItem == 1)
                        {
                            if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv").PassNull() == string.Empty)
                                qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                            else
                                qty = (tran.Double("Qty") * tran.Short("InvtMult")) / tran.Double("CnvFact");

                            objItem.QtyAllocIN = MathRound(objItem.QtyAllocIN + qty, decimalPlace);
                            objItem.QtyOnHand = MathRound(objItem.QtyOnHand + qty, decimalPlace);
                            objItem.AvgCost = MathRound(  objItem.QtyOnHand != 0 ? (objItem.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"))/objItem.QtyOnHand
                                        : objItem.AvgCost, CostDecimalPlaces);
                        }
                        if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItem.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces) < 0)
                        {
                            throw new MessageException(MessageType.Message, "607","", new[] { objItem.InvtID, objItem.SiteID });
                        }

                        objItem.TotCost = MathRound(objItem.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        objItem.LUpd_DateTime = DateTime.Now;
                        objItem.LUpd_Prog = prog;
                        objItem.LUpd_User = User;
                        objItem.Update();

                        clsIN_ItemSite objToItem = new clsIN_ItemSite(Dal);
                        if(!objToItem.GetByKey(tran.String("InvtID"),tran.String("ToSiteID")))
                        {
                            objToItem.Reset();
                            objToItem.LastPurchaseDate = DateTime.Now.Short();
                            objToItem.InvtID = tran.String("InvtID");
                            objToItem.StkItem = objInvt.StkItem;
                            objToItem.SiteID = tran.String("ToSiteID");
                            objToItem.LUpd_DateTime = objToItem.Crtd_DateTime = DateTime.Now;
                            objToItem.LUpd_Prog = objToItem.Crtd_Prog = prog;
                            objToItem.LUpd_User = objToItem.Crtd_User = User;
                            objToItem.Add();
                        }
                        
                        if (objInvt.StkItem == 1 && transfer.String("TransferType") == "2")
                        {
                            objToItem.QtyInTransit = MathRound(objToItem.QtyInTransit + Math.Abs(qty), decimalPlace);
                            objToItem.Update();
                        }

                        ////// IN_ItemLoc
                        clsIN_ItemLoc objToItemLoc = new clsIN_ItemLoc(Dal);
                        if (!string.IsNullOrWhiteSpace(tran.String("WhseLoc").PassNull()))
                        {
                            objItemLoc = new clsIN_ItemLoc(Dal);
                            if (!objItemLoc.GetByKey(tran["InvtID"].ToString(), tran["SiteID"].ToString(), tran["WhseLoc"].ToString()))
                            {
                                throw new MessageException(MessageType.Message, "606");
                            }
                            if (objInvt.StkItem == 1)
                            {
                                if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv").PassNull() == string.Empty)
                                    qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                                else
                                    qty = (tran.Double("Qty") * tran.Short("InvtMult")) / tran.Double("CnvFact");

                                objItemLoc.QtyAllocIN = MathRound(objItemLoc.QtyAllocIN + qty, decimalPlace);
                                objItemLoc.QtyOnHand = MathRound(objItemLoc.QtyOnHand + qty, decimalPlace);
                                objItemLoc.AvgCost = MathRound(objItemLoc.QtyOnHand != 0 ? (objItemLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objItemLoc.QtyOnHand
                                            : objItemLoc.AvgCost, 0);
                            }
                            if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItemLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces) < 0)
                            {
                                throw new MessageException(MessageType.Message, "607", "", new[] { objItemLoc.InvtID, objItemLoc.SiteID });
                            }

                            objItemLoc.TotCost = MathRound(objItemLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                            objItemLoc.LUpd_DateTime = DateTime.Now;
                            objItemLoc.LUpd_Prog = prog;
                            objItemLoc.LUpd_User = User;
                            objItemLoc.Update();                          
                        
                        }

                        if (!string.IsNullOrWhiteSpace(tran.String("ToWhseLoc").PassNull()))
                        {
                            
                            if (!objToItemLoc.GetByKey(tran.String("InvtID"), tran.String("ToSiteID"), tran.String("ToWhseLoc")))
                            {
                                objToItemLoc.Reset();
                                objToItemLoc.LastPurchaseDate = DateTime.Now.Short();
                                objToItemLoc.InvtID = tran.String("InvtID");
                                objToItemLoc.StkItem = objInvt.StkItem;
                                objToItemLoc.WhseLoc = tran.String("ToWhseLoc");
                                objToItemLoc.SiteID = tran.String("ToSiteID");
                                objToItemLoc.LUpd_DateTime = objToItem.Crtd_DateTime = DateTime.Now;
                                objToItemLoc.LUpd_Prog = objToItem.Crtd_Prog = prog;
                                objToItemLoc.LUpd_User = objToItem.Crtd_User = User;
                                objToItemLoc.Add();
                            }
                            if (objInvt.StkItem == 1 && transfer.String("TransferType") == "2")
                            {
                                objToItemLoc.QtyInTransit = MathRound(objToItemLoc.QtyInTransit + Math.Abs(qty), decimalPlace);
                                objToItemLoc.Update();
                            }
                        }

                        /////////


                        if (objInvt.StkItem == 1 && transfer.String("TransferType") == "1")
                        {
                            if (lineRef == string.Empty)
                            {
                                try
                                {
                                    lineRef = lstTrans.Compute("Max(LineRef)", "").ToString();
                                }
                                catch (Exception)
                                {
                                    lineRef = "0";
                                }
                            }
                            lineRef = Utility.LastLineRef(Convert.ToInt32(lineRef));

                            clsIN_Trans newTran = new clsIN_Trans(Dal);
                            newTran.RefNbr = tran.String("RefNbr");
                            newTran.BranchID = tran.String("BranchID");
                            newTran.BatNbr = tran.String("BatNbr");
                            newTran.LineRef = lineRef;
                            newTran.CnvFact = tran.Double("CnvFact");
                            newTran.ExtCost = MathRound(tran.Double("ExtCost"), CostDecimalPlaces);
                            newTran.InvtID = tran.String("InvtID");
                            newTran.InvtMult = 1;
                            newTran.JrnlType = tran.String("JrnlType");
                            newTran.ObjID = tran.String("ObjID");
                            newTran.Qty = tran.Double("Qty");
                            newTran.ReasonCD = tran.String("ReasonCD");
                            newTran.Rlsed = 1;
                            newTran.SlsperID = tran.String("SlsperID");
                            newTran.SiteID = tran.String("ToSiteID");
                            newTran.WhseLoc = tran.String("ToWhseLoc");
                            newTran.ShipperID = tran.String("ShipperID");
                            newTran.ShipperLineRef = tran.String("ShipperLineRef");
                            newTran.ToSiteID = string.Empty;
                            newTran.ToWhseLoc = string.Empty;
                            newTran.TranAmt = MathRound(tran.Double("TranAmt"), CostDecimalPlaces);
                            newTran.TranFee = tran.Double("TranFee");
                            newTran.TranDate = tran.Date("TranDate");
                            newTran.TranDesc = tran["TranDesc"].ToString();
                            newTran.TranType = tran["TranType"].ToString();
                            newTran.UnitCost = MathRound(tran.Double("UnitCost"), CostDecimalPlaces);
                            newTran.UnitDesc = tran.String("UnitDesc");
                            newTran.UnitPrice = MathRound(tran.Double("UnitPrice"), CostDecimalPlaces);
                            newTran.UnitMultDiv = tran.String("UnitMultDiv");
                            newTran.Crtd_DateTime = newTran.LUpd_DateTime = DateTime.Now;
                            newTran.Crtd_Prog = newTran.LUpd_Prog = prog;
                            newTran.Crtd_User = newTran.LUpd_User = User;
                            newTran.Add();

                            objToItem.QtyOnHand = MathRound(objToItem.QtyOnHand + Math.Abs(qty), decimalPlace);
                            objToItem.QtyAvail = MathRound(objToItem.QtyAvail + Math.Abs(qty), decimalPlace);
                            objToItem.TotCost = MathRound(objToItem.TotCost + tran.Double("ExtCost"), CostDecimalPlaces);
                            objToItem.AvgCost = MathRound(objToItem.QtyOnHand != 0 ? objToItem.TotCost / objToItem.QtyOnHand : objToItem.AvgCost, CostDecimalPlaces);
                            objToItem.LUpd_DateTime = DateTime.Now;
                            objToItem.LUpd_Prog = prog;
                            objToItem.LUpd_User = User;
                            objToItem.Update();

                            if (newTran.WhseLoc.PassNull()!="")
                            {
                                objToItemLoc.QtyOnHand = MathRound(objToItemLoc.QtyOnHand + Math.Abs(qty), decimalPlace);
                                objToItemLoc.QtyAvail = MathRound(objToItemLoc.QtyAvail + Math.Abs(qty), decimalPlace);
                                objToItemLoc.TotCost = MathRound(objToItemLoc.TotCost + tran.Double("ExtCost"), CostDecimalPlaces);
                                objToItemLoc.AvgCost = MathRound(objToItemLoc.QtyOnHand != 0 ? objToItemLoc.TotCost / objToItemLoc.QtyOnHand : objToItemLoc.AvgCost, CostDecimalPlaces);
                                objToItemLoc.LUpd_DateTime = DateTime.Now;
                                objToItemLoc.LUpd_Prog = prog;
                                objToItemLoc.LUpd_User = User;
                                objToItemLoc.Update();
                            }
                        }

                        if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                        {
                            DataTable dtLot = objLot.GetAll(branchID, batNbr, transfer.String("RefNbr"),"%", tran.String("LineRef"));
                            foreach (DataRow lotRow in dtLot.Rows)
                            {
                                if (objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                                {
                                    double lotQty = 0;
                                    if (lotRow.String("UnitMultDiv") == "M" || lotRow.String("UnitMultDiv").PassNull() == string.Empty)
                                        lotQty = lotRow.Double("Qty") * lotRow.Short("InvtMult") * lotRow.Double("CnvFact");
                                    else
                                        lotQty = (lotRow.Double("Qty") * lotRow.Short("InvtMult")) / lotRow.Double("CnvFact");

                                    objItemLot.QtyAllocIN = MathRound(objItemLot.QtyAllocIN + lotQty, decimalPlace);
                                    objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + lotQty, decimalPlace);

                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Update();
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "606");
                                }

                                if (!objItemLot.GetByKey(lotRow.String("ToSiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("ToWhseLoc").PassNull()))
                                {
                                    objItemLot.Reset();
                                    objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                    objItemLot.InvtID = lotRow.String("InvtID");
                                    objItemLot.WhseLoc = lotRow.String("ToWhseLoc").PassNull();
                                    objItemLot.SiteID = lotRow.String("ToSiteID");
                                    objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                    objItemLot.LIFODate = lotRow.Date("ExpDate");
                                    objItemLot.ExpDate = lotRow.Date("ExpDate");

                                    objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");

                                    objItemLot.Crtd_DateTime = DateTime.Now;
                                    objItemLot.Crtd_Prog = Prog;
                                    objItemLot.Crtd_User = User;

                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Add();
                                }
                                if (transfer.String("TransferType") == "2")
                                {
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Update();
                                }
                                if (transfer.String("TransferType") == "1")
                                {
                                    clsIN_LotTrans newLot = new clsIN_LotTrans(Dal);
                                    newLot.BatNbr = tran.String("BatNbr");
                                    newLot.BranchID = tran.String("BranchID");
                                    newLot.RefNbr = tran.String("RefNbr");
                                    newLot.INTranLineRef = lineRef;
                                    newLot.LotSerNbr = lotRow.String("LotSerNbr");
                                    newLot.CnvFact = lotRow.Double("CnvFact");
                                    newLot.InvtID = lotRow.String("InvtID");
                                    newLot.InvtMult = 1;
                                    newLot.Qty = lotRow.Double("Qty");
                                    newLot.SiteID = lotRow.String("ToSiteID");
                                    newLot.WhseLoc = lotRow.String("ToWhseLoc").PassNull();
                                    newLot.TranDate = tran.Date("TranDate");                                 
                                    newLot.TranType = tran["TranType"].ToString();
                                    newLot.UnitCost = MathRound(lotRow.Double("UnitCost"), CostDecimalPlaces);
                                    newLot.UnitDesc = lotRow.String("UnitDesc");
                                    newLot.UnitPrice = MathRound(lotRow.Double("UnitPrice"), CostDecimalPlaces);
                                    newLot.UnitMultDiv = lotRow.String("UnitMultDiv");
                                    newLot.ExpDate = lotRow.Date("ExpDate");
                                    newLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                    
                                    newLot.Crtd_DateTime = newLot.LUpd_DateTime = DateTime.Now;
                                    newLot.Crtd_Prog = newLot.LUpd_Prog = Prog;
                                    newLot.Crtd_User = newLot.LUpd_User = User;
                                    newLot.Add();

                                    objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + Math.Abs(newLot.Qty), decimalPlace);
                                    objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + Math.Abs(newLot.Qty), decimalPlace);

                                    objItemLot.Cost = objToItem.AvgCost * objItemLot.QtyOnHand;
                                  
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Update();
                                }
                            }
                        }
                    }
                }
                sql.IN_ReleaseBatch(branchID, batNbr, Prog, User);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IN10400_Release(string branchID, string batNbr)
        {
            try
            {
                double qty = 0;
                string lineRef = string.Empty;
                string User = string.Empty;
                string prog = string.Empty;
                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");
                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_ItemLoc objItemLoc = new clsIN_ItemLoc(Dal);
                clsSQL sql = new clsSQL(Dal);
                if (lstTrans.Rows.Count > 0)
                {
                    User = lstTrans.Rows[0].String("LUpd_User");
                    prog = lstTrans.Rows[0].String("LUpd_Prog");
                }               

                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    //// IN_ItemLoc
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);
                    if (tran.String("WhseLoc").PassNull() != "")
                    {
                        if (!objItemLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc")))
                        {
                            throw new MessageException(MessageType.Message, "2018052414");
                        }
                        if (objInvt.StkItem == 1)
                        {
                            if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv").PassNull() == string.Empty)
                                qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                            else
                                qty = (tran.Double("Qty") * tran.Short("InvtMult")) / tran.Double("CnvFact");

                            //objItemLoc.QtyAllocIN = MathRound(objItemLoc.QtyAllocIN + qty, 0);
                            objItemLoc.QtyOnHand = MathRound(objItemLoc.QtyOnHand + qty, decimalPlace);
                            if (qty < 0) objItemLoc.QtyAllocIN = MathRound(objItemLoc.QtyAllocIN + qty, decimalPlace);
                            if (qty > 0)
                            {
                                objItemLoc.QtyAvail = MathRound(objItemLoc.QtyAvail + qty, decimalPlace);
                            }
                        }
                        if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItemLoc.TotCost + tran.Double("ExtCost"), CostDecimalPlaces) < 0)
                        {
                            throw new MessageException(MessageType.Message, "2018052413", "", new[] { objItemLoc.InvtID, objItemLoc.SiteID, objItemLoc.WhseLoc });
                        }
                        objItemLoc.TotCost = MathRound(objItemLoc.TotCost + tran.Double("ExtCost"), CostDecimalPlaces);
                        objItemLoc.AvgCost = MathRound(objItemLoc.QtyOnHand == 0 ? objItemLoc.AvgCost : objItemLoc.TotCost / objItemLoc.QtyOnHand, CostDecimalPlaces); // tinh lai AvgCost 20160624
                        objItemLoc.LUpd_DateTime = DateTime.Now;
                        objItemLoc.LUpd_Prog = prog;
                        objItemLoc.LUpd_User = User;
                        objItemLoc.Update();

                    } 
                    //////


                    //objInvt.GetByKey(tran.String("InvtID"));
                    if (!objItem.GetByKey(tran.String("InvtID"),tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message, "606");
                    }
                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv").PassNull() == string.Empty)
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = (tran.Double("Qty") * tran.Short("InvtMult")) / tran.Double("CnvFact");

                        //objItem.QtyAllocIN = MathRound(objItem.QtyAllocIN + qty, 0);
                        objItem.QtyOnHand = MathRound(objItem.QtyOnHand + qty, decimalPlace);
                        if (qty < 0) objItem.QtyAllocIN = MathRound(objItem.QtyAllocIN + qty, decimalPlace);
                        if (qty > 0)
                        {
                            objItem.QtyAvail = MathRound(objItem.QtyAvail + qty, decimalPlace);                         
                        }
                    }
                    if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItem.TotCost + tran.Double("ExtCost"), CostDecimalPlaces) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607","", new[] { objItem.InvtID, objItem.SiteID });
                    }
                    objItem.TotCost = MathRound(objItem.TotCost + tran.Double("ExtCost"), CostDecimalPlaces);
                    objItem.AvgCost = MathRound(objItem.QtyOnHand == 0 ? objItem.AvgCost : objItem.TotCost / objItem.QtyOnHand, CostDecimalPlaces); // tinh lai AvgCost 20160624
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = prog;
                    objItem.LUpd_User = User;
                    objItem.Update();

                    

                    ///them lot 20160527
                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            /// cần check lại chỗ làm tròn này
                            qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.ExpDate = lotRow.Date("ExpDate");
                                objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");
                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;
                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }
                            if (!objSetup.NegQty && objItemLot.QtyOnHand + qty < 0)
                            {
                                throw new MessageException(MessageType.Message, "2018052413", "", new[] { objItemLot.InvtID, objItemLot.SiteID + "-" + objItemLot.LotSerNbr,objItemLot.WhseLoc });
                            }

                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                            if (qty < 0) objItemLot.QtyAllocIN = MathRound(objItemLot.QtyAllocIN + qty, decimalPlace);
                            if (qty > 0)
                            {
                                objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);
                            }
                            objItemLot.Cost = MathRound(objItem.TotCost * objItemLot.QtyOnHand,CostDecimalPlaces);
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();
                        }
                    }
                }
                sql.IN_ReleaseBatch(branchID, batNbr, Prog, User);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IN10500_Release(string tagID, string branchID, string siteID,string whseLoc)
        {
            try
            {
                string lineRef = string.Empty;

                clsSQL objSql = new clsSQL(Dal);
                var batNbr = objSql.INNumbering(branchID, "BatNbr");
                var refNbr = objSql.INNumbering(branchID, "RefNbr");
                
                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");

                clsIN_TagDetail objTagDetail = new clsIN_TagDetail(Dal);
                DataTable lstTagDetail = objTagDetail.GetAll(tagID, "%", branchID, "%"); //objTagDetail.GetAll(tagID, siteID, "%"); 

                clsIN_TagHeader objTagHeader = new clsIN_TagHeader(Dal);
                objTagHeader.GetByKey(tagID, branchID, siteID);
                //objTagHeader.GetByKey(tagID);
                objTagHeader.INBatNbr = batNbr;
                objTagHeader.Status = "C";
                objTagHeader.Update();
                clsBatch newBatch = new clsBatch(Dal)
                {
                    BatNbr = batNbr,
                    Descr = objTagHeader.Descr,
                    Module1 = "IN",
                    RefNbr = refNbr,
                    ReasonCD = objTagHeader.ReasonCD,
                    Status = "C",
                    Rlsed = 1,
                    DateEnt = objTagHeader.TranDate,
                    PerPost=objTagHeader.PerPost.PassNull(),
                    //DateTime.Now.Short(),
                    JrnlType = "IN",
                    EditScrnNbr = "IN10500",
                    BranchID = branchID,
                    LUpd_DateTime = objTagHeader.LUpd_DateTime,
                    LUpd_Prog = objTagHeader.LUpd_Prog,
                    LUpd_User = objTagHeader.LUpd_User,
                    Crtd_DateTime = objTagHeader.Crtd_DateTime,
                    Crtd_Prog = objTagHeader.Crtd_Prog,
                    Crtd_User = objTagHeader.Crtd_User,
                };
                

                lineRef = "0";
                lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                for (int i = lineRef.Length; lineRef.Length < 5; )
                    lineRef = "0" + lineRef;

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_ItemLoc objItemLoc = new clsIN_ItemLoc(Dal);
                clsIN_UnitConversion objCnv = new clsIN_UnitConversion(Dal);
                var totAmt = 0.0;
                foreach (DataRow tagDetail in lstTagDetail.Rows)
                {
                    clsIN_TagLot objTagLot = new clsIN_TagLot(Dal);
                    DataTable lstTagLot = objTagLot.GetAll(branchID, tagID, "%", tagDetail.String("InvtID"), tagDetail.String("LineRef"));
                    clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                    objInvt.GetByKey(tagDetail.String("InvtID").ToString());
                    objCnv.GetByKey("3", "*", tagDetail.String("InvtID"), "THUNG", objInvt.StkUnit);
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);
                    clsIN_Trans newTran = new clsIN_Trans(Dal)
                    {
                        BatNbr = batNbr,
                        BranchID = branchID,
                        TranDate =objTagHeader.TranDate,// DateTime.Now.Short(),
                        Crtd_DateTime = DateTime.Now,
                        Crtd_Prog = tagDetail.String("Crtd_Prog"),
                        Crtd_User = tagDetail.String("Crtd_User"),
                        InvtID = tagDetail.String("InvtID"),
                        TranType = "AJ",
                        JrnlType = "IN",
                        LUpd_DateTime = DateTime.Now,
                        LUpd_Prog = tagDetail.String("LUpd_Prog"),
                        LUpd_User = tagDetail.String("LUpd_User"),
                        Qty = tagDetail.Double("OffsetEAQty"),
                        ReasonCD = tagDetail.String("ReasonCD"),
                        RefNbr = refNbr,
                        Rlsed = 1,
                        SiteID = tagDetail.String("SiteID"),
                        WhseLoc=tagDetail.String("WhseLoc"),
                        LineRef = lineRef                        
                    };                   

                    lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                    for (int i = lineRef.Length; lineRef.Length < 5; )
                    {
                        lineRef = "0" + lineRef;
                    }
                    //Cập nhật vào IN_ItemSite
                    if (tagDetail.String("WhseLoc").PassNull() != "")
                    {
                        if (objItemLoc.GetByKey(tagDetail.String("InvtID"), tagDetail.String("SiteID"), tagDetail.String("WhseLoc")))
                        {
                            objItemLoc.QtyOnHand = MathRound(objItemLoc.QtyOnHand + tagDetail.Double("OffsetEAQty"),decimalPlace);
                            objItemLoc.QtyAvail = MathRound(objItemLoc.QtyAvail + tagDetail.Double("OffsetEAQty"),decimalPlace);
                            //objItem.QtyOnHand += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;
                            //objItem.QtyAvail += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;
                            objItemLoc.LUpd_DateTime = System.DateTime.Now;
                            objItemLoc.LUpd_Prog = tagDetail.String("LUpd_Prog");
                            objItemLoc.LUpd_User = tagDetail.String("LUpd_User");
                            objItemLoc.Update();
                        }
                    }

                    if (objItem.GetByKey(tagDetail.String("InvtID"), tagDetail.String("SiteID")))
                    {
                        objItem.QtyOnHand = MathRound(objItem.QtyOnHand + tagDetail.Double("OffsetEAQty"),decimalPlace);
                        objItem.QtyAvail = MathRound(objItem.QtyAvail + tagDetail.Double("OffsetEAQty"),decimalPlace);
                        //objItem.QtyOnHand += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;
                        //objItem.QtyAvail += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;
                        objItem.LUpd_DateTime = System.DateTime.Now;
                        objItem.LUpd_Prog = tagDetail.String("LUpd_Prog");
                        objItem.LUpd_User = tagDetail.String("LUpd_User");
                        objItem.Update();                        
                    }  
                    newTran.UnitDesc = objInvt.StkUnit;
                    // newTran.Qty += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;                    
                    newTran.CnvFact = 1;
                    newTran.ExtCost = objItem.AvgCost * Math.Abs(tagDetail.Double("OffsetEAQty"));// tagDetail.Double("OffsetEAQty"); // UnitCost * Qty
                    newTran.InvtMult = 1;
                    newTran.TranAmt = objItem.AvgCost * Math.Abs(tagDetail.Double("OffsetEAQty"));//tagDetail.Double("OffsetEAQty"); // UnitCost * Qty
                    newTran.TranDesc = tagDetail.String("InvtName");
                    newTran.UnitCost = objItem.AvgCost;
                    newTran.UnitMultDiv = "M";
                    newTran.UnitPrice = objItem.AvgCost;
                    newTran.Add();
                    totAmt += newTran.TranAmt;

                    foreach (DataRow itemTagLot in lstTagLot.Rows)
                    {
                        if (itemTagLot.String("InvtID") == tagDetail.String("InvtID"))
                        {
                            if (objItemLot.GetByKey(itemTagLot.String("SiteID"), itemTagLot.String("InvtID"), itemTagLot.String("LotSerNbr"), itemTagLot.String("WhseLoc").PassNull()))
                            {
                                if (itemTagLot.Double("OffsetEAQty") > 0)
                                {
                                    objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + itemTagLot.Double("OffsetEAQty"), decimalPlace);
                                    if (itemTagLot.Double("OffsetEAQty") < 0) objItemLot.QtyAllocIN = MathRound(objItemLot.QtyAllocIN + itemTagLot.Double("OffsetEAQty"), decimalPlace);
                                    if (itemTagLot.Double("OffsetEAQty") > 0)
                                    {
                                        objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + itemTagLot.Double("OffsetEAQty"), decimalPlace);
                                    }
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Update();
                                    clsIN_LotTrans newLotTrans = new clsIN_LotTrans(Dal)
                                    {
                                        BatNbr = batNbr,
                                        BranchID = branchID,
                                        RefNbr = newTran.RefNbr,
                                        LotSerNbr = objItemLot.LotSerNbr,
                                        INTranLineRef = newTran.LineRef,
                                        ExpDate = DateTime.Now,
                                        InvtID = newTran.InvtID,
                                        InvtMult = newTran.InvtMult,
                                        KitID = "", // ko bit
                                        MfgrLotSerNbr = "",
                                        Qty = newTran.Qty,
                                        SiteID = newTran.SiteID,
                                        WhseLoc = newTran.WhseLoc.PassNull(),
                                        ToSiteID = newTran.ToSiteID,
                                        TranDate = newTran.TranDate,
                                        TranType = newTran.TranType,
                                        UnitCost = newTran.UnitCost,
                                        UnitPrice = newTran.UnitPrice,
                                        WarrantyDate = DateTime.Now,
                                        Crtd_DateTime = DateTime.Now,
                                        Crtd_Prog = tagDetail.String("Crtd_Prog"),
                                        Crtd_User = tagDetail.String("Crtd_User"),
                                        LUpd_DateTime = DateTime.Now,
                                        LUpd_Prog = tagDetail.String("LUpd_Prog"),
                                        LUpd_User = tagDetail.String("LUpd_User"),
                                        UnitDesc = newTran.UnitDesc,
                                        CnvFact = newTran.CnvFact,
                                        UnitMultDiv = newTran.UnitMultDiv
                                    };
                                    newLotTrans.Add();
                                }
                                else
                                {
                                    var qty = Math.Abs(itemTagLot.Double("OffsetEAQty"));
                                    var valTran = 0.0;
                                    var qtyAvail = objItemLot.QtyAvail;
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    if (Math.Abs(qty) > qtyAvail)
                                    {
                                        valTran = qtyAvail;
                                        objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand - valTran, decimalPlace);
                                        objItemLot.QtyAvail = 0;
                                        objItemLot.Cost = 0;
                                        objItemLot.Update();
                                        qty = qty - qtyAvail;
                                    }
                                    else
                                    {
                                        valTran = qty;
                                        objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand - valTran, decimalPlace);
                                        objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail - valTran, decimalPlace);
                                        objItemLot.Cost = objItem.TotCost * Math.Abs(objItemLot.QtyOnHand);
                                        objItemLot.Update();
                                        qty = 0;
                                    }

                                    clsIN_LotTrans newLotTrans = new clsIN_LotTrans(Dal)
                                    {
                                        BatNbr = batNbr,
                                        BranchID = branchID,
                                        RefNbr = newTran.RefNbr,
                                        LotSerNbr = objItemLot.LotSerNbr,
                                        INTranLineRef = newTran.LineRef,
                                        ExpDate = DateTime.Now,
                                        InvtID = newTran.InvtID,
                                        InvtMult = newTran.InvtMult,
                                        KitID = "", // ko bit
                                        MfgrLotSerNbr = "",
                                        Qty = -valTran,
                                        SiteID = newTran.SiteID,
                                        WhseLoc = newTran.WhseLoc.PassNull(),
                                        ToWhseLoc = newTran.ToWhseLoc.PassNull(),
                                        ToSiteID = newTran.ToSiteID,
                                        TranDate = newTran.TranDate,
                                        TranType = newTran.TranType,
                                        UnitCost = newTran.UnitCost,
                                        UnitPrice = newTran.UnitPrice,
                                        WarrantyDate = DateTime.Now,
                                        Crtd_DateTime = DateTime.Now,
                                        Crtd_Prog = tagDetail.String("Crtd_Prog"),
                                        Crtd_User = tagDetail.String("Crtd_User"),
                                        LUpd_DateTime = DateTime.Now,
                                        LUpd_Prog = tagDetail.String("LUpd_Prog"),
                                        LUpd_User = tagDetail.String("LUpd_User"),
                                        UnitDesc = newTran.UnitDesc,
                                        CnvFact = newTran.CnvFact,
                                        UnitMultDiv = newTran.UnitMultDiv
                                    };
                                    newLotTrans.Add();
                                }
                            }
                        }                        
                    }
                }
                newBatch.TotAmt = totAmt;
                newBatch.Add();
                
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IN40100_Release(string perPost, string[] lstSite, string[] lstInvtID, string type)
        {
            try
            {
                clsSQL objSql = new clsSQL(Dal);
                for(int i=0;i<lstInvtID.Length;i++)
                {
                    clsIN_WrkCosting newWrk = new clsIN_WrkCosting();
                    newWrk.PerNbr = perPost;
                    newWrk.SiteID = lstSite[i];
                    newWrk.InvtID = lstInvtID[i];
                    newWrk.MachineName = Environment.MachineName.ToString().Trim();
                    newWrk.Crtd_Datetime = newWrk.LUpd_Datetime = DateTime.Now;
                    newWrk.Crtd_Prog = newWrk.LUpd_Prog = Prog;
                    newWrk.Crtd_User = newWrk.LUpd_User = User;
                    newWrk.Add();
                }
                if (type == "A")
                    objSql.IN_UpdateEndingAvgCost("IN40100", User, DateTime.Now);
                else
                    objSql.IN_RecalculateAvgCost("IN40100", User, DateTime.Now);
              
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string IN40300_Release(List<string> lstLang, string parm01, string parm02, string parm03, string parm04, string parm05, string parm06, string WhseLoc)
        {
            clsSQL objSql = new clsSQL(Dal);
            string mMessage = string.Empty;
            string type = parm01;
            if(type=="1")
            {
                if (parm02 == "1")
                {
                    DataTable lstValid = objSql.IN_Integrity_Validate(Prog, User);
                    foreach (DataRow valid in lstValid.Rows)
                    {
                        mMessage += lstLang[0] + " " + valid["Code"].ToString() + " " + lstLang[1] + " " + valid["TableName"].ToString() + " " + lstLang[2] + " "+
                                    valid["Type"].ToString() + "\n";
                    }
                }
                if(parm03=="1")
                {
                    mMessage +=
                        objSql.IN_Integrity_RebuildQtyonSO(Prog, User, parm05, parm06, 0, 0, 0, WhseLoc) +
                        "\n";
                }
                if (parm04 == "1")
                {
                    mMessage +=
                        objSql.IN_Integrity_RebuildQtyCost(Prog, User, parm05, parm06, 0, 0, 0, WhseLoc) +
                        "\n";
                }
            }
            else
            {
                objSql.IN_Integrity_ReBuildBatch(Prog, User, parm02, parm03, parm04);
            }         
            return mMessage;
        }
        public bool Issue_Cancel(string branchID, string batNbr, string rcptNbr, bool release)
        {
            try
            {
                double qty = 0;
                DataTable lstTrans = new DataTable();

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetAll(branchID, "IN");

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                if (string.IsNullOrEmpty(rcptNbr))
                    lstTrans = objTran.GetAll(branchID, batNbr,"%","%");
                else
                    lstTrans = objTran.GetAll(branchID, batNbr, rcptNbr, "%");

                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_ItemLoc objItemLoc = new clsIN_ItemLoc(Dal);
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);
                clsSQL sql = new clsSQL(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    if (!release && (tran.Short("Rlsed") == -1 || tran.Short("InvtMult") == 1)) continue;

                    objInvt.GetByKey(tran.String("InvtID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (!objItem.GetByKey(tran.String("InvtID"),tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message,"606");
                      
                    }
                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                        objItem.QtyAvail = MathRound(objItem.QtyAvail + qty, decimalPlace);
                        objItem.QtyOnHand = MathRound(objItem.QtyOnHand + qty, decimalPlace);
                        objItem.AvgCost = MathRound(objItem.QtyOnHand != 0 ? (objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItem.QtyOnHand :
                            objItem.AvgCost, CostDecimalPlaces);

                    }
                    objItem.TotCost = MathRound(objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = Prog;
                    objItem.LUpd_User = User;
                    objItem.Update();

                    if (tran.String("WhseLoc").PassNull()!="")
                    {
                        if (!objItemLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc").PassNull()))
                        {
                            throw new MessageException(MessageType.Message, "606");

                        }
                        if (objInvt.StkItem == 1)
                        {
                            if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                                qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                            else
                                qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                            objItemLoc.QtyAvail = MathRound(objItemLoc.QtyAvail + qty, decimalPlace);
                            objItemLoc.QtyOnHand = MathRound(objItemLoc.QtyOnHand + qty, decimalPlace);
                            objItemLoc.AvgCost = MathRound(objItemLoc.QtyOnHand != 0 ? (objItemLoc.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItemLoc.QtyOnHand :
                                objItemLoc.AvgCost, CostDecimalPlaces);

                        }
                        objItemLoc.TotCost = MathRound(objItemLoc.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        objItemLoc.LUpd_DateTime = DateTime.Now;
                        objItemLoc.LUpd_Prog = Prog;
                        objItemLoc.LUpd_User = User;
                        objItemLoc.Update();
                    }   


                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            /// cần check lai chỗ làm tròn này
                            qty = -1 * MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");
                                objItemLot.ExpDate = lotRow.Date("ExpDate"); 
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }
                            objItemLot.Cost = objItem.TotCost;
                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                            objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);


                            if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItemLot.Cost, CostDecimalPlaces) < 0)
                            {
                                throw new MessageException("607", new[] { objInvt.InvtID, objItemLot.SiteID + " - " + objItemLot.LotSerNbr });
                            }

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();
                        }
                    }


                    sql.GetCostByCostID(ref cost,tran.String("InvtID"),tran.String("SiteID"),tran.String("CostID"));
                    if(cost.CostIdentity>0)
                    {
                        cost.Qty = MathRound(cost.Qty + qty, decimalPlace);
                        cost.TotCost = MathRound(cost.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        cost.Update();
                    }
                    else
                    {
                        clsIN_ItemCost newItemCost = new clsIN_ItemCost(Dal)
                        {
                            CostID = tran.String("CostID"),
                            InvtID = tran.String("InvtID"),
                            Qty = tran.Double("Qty"),
                            RcptDate = tran.Date("TranDate"),
                            RcptNbr = tran.String("RefNbr"),
                            TotCost = tran.Double("ExtCost"),
                            UnitCost = tran.Double("UnitCost"),
                            SiteID = tran.String("SiteID"),
                            Crtd_DateTime = DateTime.Now,
                            Crtd_Prog = Prog,
                            Crtd_User = User,
                            LUpd_DateTime = DateTime.Now,
                            LUpd_Prog = Prog,
                            LUpd_User = User
                        };
                        newItemCost.Add();
                    }

                }
                if(release)
                    sql.IN_CancelBatch(branchID, batNbr, Prog, User);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool Receipt_Cancel(string branchID, string batNbr, string rcptNbr, bool isTransfer,string transferNbr, bool release)
        {
            try
            {
                clsSQL objSql = new clsSQL(Dal);
                clsIN_Setup setup = new clsIN_Setup(Dal);
                setup.GetByKey(branchID, "IN");
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = new DataTable();
                if (string.IsNullOrEmpty(rcptNbr))
                    lstTrans =objTran.GetAll(branchID,batNbr,"%","%");
                else
                    lstTrans = objTran.GetAll(branchID, batNbr, rcptNbr, "%");

                double qty = 0;

                DateTime? tranDate = DateTime.Now;

                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objSite = new clsIN_ItemSite(Dal);
                clsIN_ItemLoc itemLoc = new clsIN_ItemLoc(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    if (!release && tran.Short("Rlsed") == -1 || tran.Short("InvtMult") == -1) continue;
                   
                    objInvt.GetByKey(tran.String("InvtID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    #region -IN_ItemSite-
                    if (!objSite.GetByKey(tran.String("InvtID"),tran.String("SiteID")))
                    {
                        Insert_IN_ItemSite(ref objSite, tran.String("InvtID"), objInvt.StkItem, tran.String("SiteID"), 0);
                    }

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv") == string.Empty)
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                        if (isTransfer && tran.String("TranType") == "TR") objSite.QtyInTransit -= qty;

                        objSite.QtyOnHand = MathRound(objSite.QtyOnHand + qty, decimalPlace);
                        objSite.QtyAvail = MathRound(objSite.QtyAvail + qty, decimalPlace);
                        objSite.AvgCost = MathRound(objSite.QtyOnHand != 0 ? (objSite.TotCost - tran.Double("ExtCost")) / objSite.QtyOnHand : objSite.AvgCost, CostDecimalPlaces);
                        
                        if (!setup.NegQty && objSite.QtyAvail < 0)
                        {
                            throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID });
                        }
                    }

                    if (!setup.NegQty && setup.CheckINVal && MathRound(objSite.TotCost - tran.Double("ExtCost"), CostDecimalPlaces) < 0)
                    {
                        throw new MessageException(MessageType.Message,"607","", new[] { objInvt.InvtID, objSite.SiteID });
                    
                    }

                    objSite.TotCost = MathRound(objSite.TotCost - tran.Double("ExtCost"), CostDecimalPlaces);
                    objSite.LUpd_DateTime = DateTime.Now;
                    objSite.LUpd_Prog = tran.String("LUpd_Prog");
                    objSite.LUpd_User = tran.String("LUpd_User");
                    objSite.Update();
                    #endregion
                    
                    #region -IN_ItemLoc-
                    if (!string.IsNullOrWhiteSpace(tran.String("WhseLoc")))
                    {
                        if (!itemLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc")))
                        {
                            Insert_IN_ItemLoc(ref itemLoc, tran.String("InvtID"), objInvt.StkItem, tran.String("SiteID"), tran.String("WhseLoc"), 0);
                        }

                        if (objInvt.StkItem == 1)
                        {
                            if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv") == string.Empty)
                            {
                                qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                            }
                            else
                            {
                                qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                            }
                            if (isTransfer && tran.String("TranType") == "TR") itemLoc.QtyInTransit -= qty;

                            itemLoc.QtyOnHand = MathRound(itemLoc.QtyOnHand + qty, decimalPlace);
                            itemLoc.QtyAvail = MathRound(itemLoc.QtyAvail + qty, decimalPlace);
                            itemLoc.AvgCost = MathRound(itemLoc.QtyOnHand != 0 ? (itemLoc.TotCost - tran.Double("ExtCost")) / itemLoc.QtyOnHand : itemLoc.AvgCost, CostDecimalPlaces);

                            if (!setup.NegQty && itemLoc.QtyAvail < 0)
                            {
                                throw new MessageException(MessageType.Message, "608", "", new[] { itemLoc.InvtID, itemLoc.SiteID });
                            }
                        }

                        if (!setup.NegQty && setup.CheckINVal && MathRound(itemLoc.TotCost - tran.Double("ExtCost"), CostDecimalPlaces) < 0)
                        {
                            throw new MessageException(MessageType.Message, "607", "", new[] { objInvt.InvtID, itemLoc.SiteID });

                        }

                        itemLoc.TotCost = MathRound(itemLoc.TotCost - tran.Double("ExtCost"), CostDecimalPlaces);
                        itemLoc.LUpd_DateTime = DateTime.Now;
                        itemLoc.LUpd_Prog = tran.String("LUpd_Prog");
                        itemLoc.LUpd_User = tran.String("LUpd_User");
                        itemLoc.Update();
                    }
                    #endregion

                    #region -IN_ItemLot-
                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            if (objInvt.LotSerTrack == "Q")
                            {
                                qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace);
                            }
                            else
                            {
                                qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace);
                            }

                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.WarrantyDate =lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");
                                objItemLot.ExpDate = lotRow.Date("ExpDate"); 
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }

                            objItemLot.ExpDate = lotRow.Date("ExpDate");
                            objItemLot.Cost = objSite.TotCost;
                          
                            if (!setup.NegQty && objItemLot.QtyAvail - qty < 0)
                            {
                                throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID + "-" + objItemLot.LotSerNbr });
                            }
                            objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail - qty, decimalPlace);
                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand - qty, decimalPlace);
                           

                            if (setup.CheckINVal && objItemLot.Cost < 0)
                            {
                                throw new MessageException(MessageType.Message,"607","", new[] { objInvt.InvtID, objItemLot.SiteID  + " - " + objItemLot.LotSerNbr});
                            }

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();
                        }
                    }
                    #endregion

                    objSql.DelCostByCostID(tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                }
                if (isTransfer)
                {
                    objSql.IN10100_UpdateTransfer(branchID, batNbr, Prog, User, tranDate, "", transferNbr, "I");
                }
                if (release)
                    objSql.IN_CancelBatch(branchID, batNbr, Prog, User);
                   
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IN10201_Release(string branchID, string batNbr, string transType, string toSiteID)
        {
            try
            {
                clsSQL sql = new clsSQL(Dal);
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);

                double qty = 0;
                string lineRef = string.Empty;

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                try
                {
                    lineRef = (Convert.ToInt32(lstTrans.Compute("Max(LineRef)", "").ToString()) + 1).ToString();
                    for (int l = lineRef.Length; l < 5; l++)
                    {
                        lineRef = "0" + lineRef;
                    }
                }
                catch (Exception)
                {
                    lineRef = "00001";
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objSite = new clsIN_ItemSite(Dal);
                clsIN_ItemSite objBranchSite = null;
                clsIN_ItemLot objBranchLot = null;
                if (transType == "CB")
                {
                    objBranchSite = new clsIN_ItemSite(Dal);
                    objBranchLot = new clsIN_ItemLot(Dal);
                }
                #region -Update IN_ItemCost & IN_Trans-
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (objInvt.ValMthd == "F" || objInvt.ValMthd == "L")
                    {
                        int negQty = 1;
                        double qtyCost = 0;
                        DataTable lstFL = new DataTable();

                        if (objInvt.ValMthd == "F")
                            lstFL = sql.GetListFIFOCost(tran.String("InvtID"), tran.String("SiteID"));
                        else if (objInvt.ValMthd == "L")
                            lstFL = sql.GetListLIFOCost(tran.String("InvtID"), tran.String("SiteID"));

                        if (tran.Double("Qty") > 0)
                            negQty = 1;
                        else
                            negQty = -1;

                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = Math.Abs(tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact"));
                        else
                            qty = Math.Abs(tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact"));

                        if (lstFL.Rows.Count > 0)
                        {
                            int count = 0;
                            foreach (DataRow fl in lstFL.Rows)
                            {
                                double qtyCal = 0, qtyUpdate = 0;
                                if (qtyCost < qty)
                                {
                                    if (fl.Double("Qty") <= qty - qtyCost)
                                        qtyCal = fl.Double("Qty");
                                    else
                                        qtyCal = qty - qtyCost;

                                    if (tran.String("UnitMultDiv") == "D")
                                        qtyUpdate = negQty * qtyCal * tran.Double("CnvFact");
                                    else
                                        qtyUpdate = (negQty * qtyCal) / tran.Double("CnvFact");

                                    objTran.GetByKey(branchID, batNbr, tran.String("RefNbr"), tran.String("LineRef"));
                                    objTran.Qty = qtyUpdate;
                                    objTran.UnitCost = fl.Double("UnitCost");
                                    objTran.ExtCost = MathRound(qtyCal * fl.Double("UnitCost"), CostDecimalPlaces);
                                    objTran.TranAmt = MathRound(qtyUpdate * tran.Double("UnitPrice"), CostDecimalPlaces);
                                    objTran.CostID = fl.String("CostID");

                                    if (count == 0)
                                    {
                                        objTran.Update();
                                    }
                                    else
                                    {
                                        objTran.LineRef = lineRef;
                                        objTran.Add();
                                        lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                                        for (int i = lineRef.Length; lineRef.Length < 5; )
                                            lineRef = "0" + lineRef;

                                    }
                                    qtyCost = qtyCost + fl.Double("Qty");
                                    cost.GetByKey(fl.Int("CostIdentity"));
                                    cost.Qty = MathRound(cost.Qty - qtyCal, decimalPlace);
                                    cost.TotCost = MathRound(cost.TotCost - qtyCal * cost.UnitCost, CostDecimalPlaces);
                                    if (cost.Qty == 0 && cost.TotCost == 0)
                                        cost.Delete(fl.Int("CostIdentity"));
                                    else
                                        cost.Update();
                                }
                                else
                                {
                                    break;
                                }
                                count++;
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "738");
                        }

                    }

                }                                
                #endregion

                qty = 0;
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    if (!objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message, "606");
                    }
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                    }
                    if (!objSetup.NegQty && MathRound(objSite.QtyOnHand + qty, decimalPlace) < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.QtyAllocIN = MathRound(objSite.QtyAllocIN + qty, decimalPlace);
                    objSite.QtyOnHand = MathRound(objSite.QtyOnHand + qty, decimalPlace);
                    objSite.AvgCost = MathRound(objSite.QtyOnHand != 0
                                        ? (objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objSite.QtyOnHand
                                        : objSite.AvgCost, CostDecimalPlaces);
                    
                    if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607", "", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.TotCost = MathRound(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                    objSite.LUpd_DateTime = DateTime.Now;
                    objSite.LUpd_Prog = Prog;
                    objSite.LUpd_User = User;
                    objSite.Update();
                    if (objBranchSite  != null)
                    {
                        double tranQty = Math.Abs(qty);
                        if (!objBranchSite.GetByKey(tran.String("InvtID"), toSiteID))
                        {
                            Insert_IN_ItemSite(ref objBranchSite, tran.String("InvtID"), objInvt.StkItem, toSiteID, tranQty);
                        }
                        else
                        {
                            objBranchSite.QtyOnHand = MathRound(objBranchSite.QtyOnHand + tranQty, decimalPlace);
                            objBranchSite.QtyAvail = MathRound(objBranchSite.QtyAvail + tranQty, decimalPlace);
                            objBranchSite.LUpd_DateTime = DateTime.Now;
                            objBranchSite.LUpd_Prog = Prog;
                            objBranchSite.LUpd_User = User;
                            objBranchSite.Update();
                        }
                    }
                    

                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.ExpDate = lotRow.Date("ExpDate");
                                objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");

                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }

                            if (!objSetup.NegQty && objItemLot.QtyOnHand + qty < 0)
                            {
                                throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID + "-" + objItemLot.LotSerNbr });
                            }

                            objItemLot.QtyAllocIN = MathRound(objItemLot.QtyAllocIN + qty, decimalPlace);
                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);                           

                            objItemLot.Cost = objSite.TotCost * objItemLot.QtyOnHand;
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();

                            if (objBranchLot != null)
                            {
                                double transLotQty = Math.Abs(qty);
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                                {
                                    Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.WhseLoc, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = MathRound(objBranchLot.QtyOnHand + transLotQty, decimalPlace);
                                    objBranchLot.QtyAvail = MathRound(objBranchLot.QtyAvail + transLotQty, decimalPlace);
                                    objBranchLot.LUpd_DateTime = DateTime.Now;
                                    objBranchLot.LUpd_Prog = Prog;
                                    objBranchLot.LUpd_User = User;
                                    objBranchLot.Update();
                                }

                            }
                            
                        }
                    }

                    if (objInvt.ValMthd == "S")
                    {
                        sql.GetCostByCostID(ref cost, tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                        if (cost.CostIdentity > 0)
                        {
                            cost.TotCost = MathRound(cost.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"),CostDecimalPlaces);
                            if (cost.Qty == 0 && cost.TotCost == 0)
                                sql.DelCostByCostID(tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                            else
                                cost.Update();
                        }
                    }
                }
                sql.IN10201_ReleaseBatch(branchID, batNbr, Prog, User, transType);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool IN10201_Cancel(string branchID, string batNbr, string transType, string toSiteID)
        {
            try
            {
                string rcptNbr = string.Empty;
                bool release = true;
                double qty = 0;
                DataTable lstTrans = new DataTable();

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetAll(branchID, "IN");

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                if (string.IsNullOrEmpty(rcptNbr))
                    lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                else
                    lstTrans = objTran.GetAll(branchID, batNbr, rcptNbr, "%");

                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);

                clsIN_ItemSite objBranchSite = null;
                clsIN_ItemLot objBranchLot = null;
                if (transType == "CB")
                {
                    objBranchSite = new clsIN_ItemSite(Dal);
                    objBranchLot = new clsIN_ItemLot(Dal);
                }
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);
                clsSQL sql = new clsSQL(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    if (!release && (tran.Short("Rlsed") == -1 || tran.Short("InvtMult") == 1)) continue;

                    objInvt.GetByKey(tran.String("InvtID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (!objItem.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message, "606");
                    }
                    
                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                        objItem.QtyAvail = MathRound(objItem.QtyAvail + qty, decimalPlace);
                        objItem.QtyOnHand = MathRound(objItem.QtyOnHand + qty, decimalPlace);
                        objItem.AvgCost = MathRound(objItem.QtyOnHand != 0 ? (objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItem.QtyOnHand :
                            objItem.AvgCost, CostDecimalPlaces);

                    }
                    objItem.TotCost = MathRound(objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = Prog;
                    objItem.LUpd_User = User;
                    objItem.Update();

                    if (objBranchSite != null)
                    {
                        double tranQty = Math.Abs(qty);
                        if (!objBranchSite.GetByKey(tran.String("InvtID"), toSiteID))
                        {
                            throw new MessageException(MessageType.Message, "606"); //Insert_IN_ItemSite(ref objBranchSite, tran.String("InvtID"), objInvt.StkItem, toSiteID, tranQty);
                        }
                        else if ( objBranchSite.QtyAvail == 0 || objBranchSite.QtyOnHand == 0)
                        {
                            throw new MessageException(MessageType.Message, "608", "", new[] { tran.String("InvtID"), toSiteID });
                        }
                        else
                        {
                            objBranchSite.QtyOnHand = MathRound(objBranchSite.QtyOnHand - tranQty, decimalPlace);
                            objBranchSite.QtyAvail = MathRound(objBranchSite.QtyAvail - tranQty, decimalPlace);
                            objBranchSite.LUpd_DateTime = DateTime.Now;
                            objBranchSite.LUpd_Prog = Prog;
                            objBranchSite.LUpd_User = User;
                            objBranchSite.Update();
                        }
                    }

                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            qty = -1 * MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");
                                objItemLot.ExpDate = lotRow.Date("ExpDate");
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }
                            objItemLot.Cost = objItem.TotCost;
                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                            objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);


                            if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItemLot.Cost, CostDecimalPlaces) < 0)
                            {
                                throw new MessageException("607", new[] { objInvt.InvtID, objItemLot.SiteID + " - " + objItemLot.LotSerNbr });
                            }

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();

                            if (objBranchLot != null)
                            {
                                double transLotQty = Math.Abs(qty);
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()) || objBranchLot.QtyAvail == 0 || objBranchLot.QtyOnHand == 0)
                                {
                                    throw new MessageException(MessageType.Message, "201508181", "", new[] { "", tran.String("InvtID") + " " + Util.GetLang("Site") + " " + toSiteID, lotRow.String("LotSerNbr"), transLotQty.ToString() }); 
                                    //throw new MessageException(MessageType.Message, "606"); //Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = MathRound(objBranchLot.QtyOnHand - transLotQty, decimalPlace);
                                    objBranchLot.QtyAvail = MathRound(objBranchLot.QtyAvail - transLotQty, decimalPlace);
                                    objBranchLot.LUpd_DateTime = DateTime.Now;
                                    objBranchLot.LUpd_Prog = Prog;
                                    objBranchLot.LUpd_User = User;
                                    objBranchLot.Update();
                                }

                            }
                        }
                    }

                    sql.GetCostByCostID(ref cost, tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                    if (cost.CostIdentity > 0)
                    {
                        cost.Qty = MathRound(cost.Qty + qty, decimalPlace);
                        cost.TotCost = MathRound(cost.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        cost.Update();
                    }
                    else
                    {
                        clsIN_ItemCost newItemCost = new clsIN_ItemCost(Dal)
                        {
                            CostID = tran.String("CostID"),
                            InvtID = tran.String("InvtID"),
                            Qty = tran.Double("Qty"),
                            RcptDate = tran.Date("TranDate"),
                            RcptNbr = tran.String("RefNbr"),
                            TotCost = tran.Double("ExtCost"),
                            UnitCost = tran.Double("UnitCost"),
                            SiteID = tran.String("SiteID"),
                            Crtd_DateTime = DateTime.Now,
                            Crtd_Prog = Prog,
                            Crtd_User = User,
                            LUpd_DateTime = DateTime.Now,
                            LUpd_Prog = Prog,
                            LUpd_User = User
                        };
                        newItemCost.Add();
                    }
                }
                if (release)
                    sql.IN10201_CancelBatch(branchID, batNbr, Prog, User, transType);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void Insert_IN_ItemSite(ref clsIN_ItemSite objIN_ItemSite, string invtID, short StkItem, string SiteID, double qty)
        {
            try
            {
                objIN_ItemSite.Reset();
                objIN_ItemSite.InvtID = invtID;
                objIN_ItemSite.SiteID = SiteID;
                objIN_ItemSite.AvgCost = 0;
                objIN_ItemSite.QtyAlloc = 0;
                objIN_ItemSite.QtyAllocIN = 0;
                objIN_ItemSite.QtyAllocPORet = 0;
                objIN_ItemSite.QtyAllocSO = 0;
                objIN_ItemSite.QtyAvail = qty;
                objIN_ItemSite.QtyInTransit = 0;
                objIN_ItemSite.QtyOnBO = 0;
                objIN_ItemSite.QtyOnHand = qty;
                objIN_ItemSite.QtyOnPO = 0;
                objIN_ItemSite.QtyOnTransferOrders = 0;
                objIN_ItemSite.QtyOnSO = 0;
                objIN_ItemSite.QtyShipNotInv = 0;
                objIN_ItemSite.StkItem = StkItem;
                objIN_ItemSite.TotCost = 0;
                objIN_ItemSite.Crtd_DateTime = DateTime.Now;
                objIN_ItemSite.Crtd_Prog = Prog;
                objIN_ItemSite.Crtd_User = User;
                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                objIN_ItemSite.LUpd_Prog = Prog;
                objIN_ItemSite.LUpd_User = User;
                objIN_ItemSite.LastPurchaseDate = DateTime.Now.Short();
                objIN_ItemSite.Add();

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        private void Insert_IN_ItemLot(ref clsIN_ItemLot objIN_ItemLot, string InvtID, string SiteID, string whseLoc, string LotSerNbr, DateTime ExpDate, string MfgrLotSerNbr, double Cost, double Qty)
        {
            try
            {
                objIN_ItemLot.SiteID = SiteID;
                objIN_ItemLot.InvtID = InvtID;
                objIN_ItemLot.WhseLoc = whseLoc;
                objIN_ItemLot.LotSerNbr = LotSerNbr;
                objIN_ItemLot.Cost = Cost;
                objIN_ItemLot.ExpDate = ExpDate;
                objIN_ItemLot.LIFODate = new DateTime(1900, 1, 1);
                objIN_ItemLot.MfgrLotSerNbr = MfgrLotSerNbr;
                objIN_ItemLot.QtyAlloc = 0;
                objIN_ItemLot.QtyAllocIN = 0;
                objIN_ItemLot.QtyAllocOther = 0;
                objIN_ItemLot.QtyAllocPORet = 0;
                objIN_ItemLot.QtyAllocSO = 0;
                objIN_ItemLot.QtyAvail = Qty;
                objIN_ItemLot.QtyOnHand = Qty;
                objIN_ItemLot.QtyShipNotInv = 0;
                objIN_ItemLot.WarrantyDate = new DateTime(1900, 1, 1);
                objIN_ItemLot.Crtd_DateTime = DateTime.Now;
                objIN_ItemLot.Crtd_Prog = Prog;
                objIN_ItemLot.Crtd_User = User;
                objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                objIN_ItemLot.LUpd_Prog = Prog;
                objIN_ItemLot.LUpd_User = User;
                objIN_ItemLot.Add();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Insert_IN_ItemLoc(ref clsIN_ItemLoc objIN_ItemLoc, string invtID, short StkItem, string SiteID, string whseLoc, double qty)
        {
            try
            {
                objIN_ItemLoc.Reset();
                objIN_ItemLoc.InvtID = invtID;
                objIN_ItemLoc.SiteID = SiteID;
                objIN_ItemLoc.WhseLoc = whseLoc;
                objIN_ItemLoc.AvgCost = 0;
                objIN_ItemLoc.QtyAlloc = 0;
                objIN_ItemLoc.QtyAllocIN = 0;
                objIN_ItemLoc.QtyAllocPORet = 0;
                objIN_ItemLoc.QtyAllocSO = 0;
                objIN_ItemLoc.QtyAvail = qty;
                objIN_ItemLoc.QtyInTransit = 0;
                objIN_ItemLoc.QtyOnBO = 0;
                objIN_ItemLoc.QtyOnHand = qty;
                objIN_ItemLoc.QtyOnPO = 0;
                objIN_ItemLoc.QtyOnTransferOrders = 0;
                objIN_ItemLoc.QtyOnSO = 0;
                objIN_ItemLoc.QtyShipNotInv = 0;
                objIN_ItemLoc.StkItem = StkItem;
                objIN_ItemLoc.TotCost = 0;
                objIN_ItemLoc.Crtd_DateTime = DateTime.Now;
                objIN_ItemLoc.Crtd_Prog = Prog;
                objIN_ItemLoc.Crtd_User = User;
                objIN_ItemLoc.LUpd_DateTime = DateTime.Now;
                objIN_ItemLoc.LUpd_Prog = Prog;
                objIN_ItemLoc.LUpd_User = User;
                objIN_ItemLoc.LastPurchaseDate = DateTime.Now.Short();
                objIN_ItemLoc.Add();

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        public bool IN11200_Release(string branchID, string batNbr, string transType, string toSiteID)
        {
            try
            {
                clsSQL sql = new clsSQL(Dal);
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);

                double qty = 0;
                string lineRef = string.Empty;

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);

                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                try
                {
                    lineRef = (Convert.ToInt32(lstTrans.Compute("Max(LineRef)", "").ToString()) + 1).ToString();
                    for (int l = lineRef.Length; l < 5; l++)
                    {
                        lineRef = "0" + lineRef;
                    }
                }
                catch (Exception)
                {
                    lineRef = "00001";
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objSite = new clsIN_ItemSite(Dal);
                clsIN_ItemSite objBranchSite = null;
                clsIN_ItemLot objBranchLot = null;
                if (transType == "BC")
                {
                    objBranchSite = new clsIN_ItemSite(Dal);
                    objBranchLot = new clsIN_ItemLot(Dal);
                }
                #region -Update IN_ItemCost & IN_Trans-
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (objInvt.ValMthd == "F" || objInvt.ValMthd == "L")
                    {
                        int negQty = 1;
                        double qtyCost = 0;
                        DataTable lstFL = new DataTable();

                        if (objInvt.ValMthd == "F")
                            lstFL = sql.GetListFIFOCost(tran.String("InvtID"), tran.String("SiteID"));
                        else if (objInvt.ValMthd == "L")
                            lstFL = sql.GetListLIFOCost(tran.String("InvtID"), tran.String("SiteID"));

                        if (tran.Double("Qty") > 0)
                            negQty = 1;
                        else
                            negQty = -1;

                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = Math.Abs(tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact"));
                        else
                            qty = Math.Abs(tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact"));

                        if (lstFL.Rows.Count > 0)
                        {
                            int count = 0;
                            foreach (DataRow fl in lstFL.Rows)
                            {
                                double qtyCal = 0, qtyUpdate = 0;
                                if (qtyCost < qty)
                                {
                                    if (fl.Double("Qty") <= qty - qtyCost)
                                        qtyCal = fl.Double("Qty");
                                    else
                                        qtyCal = qty - qtyCost;

                                    if (tran.String("UnitMultDiv") == "D")
                                        qtyUpdate = negQty * qtyCal * tran.Double("CnvFact");
                                    else
                                        qtyUpdate = (negQty * qtyCal) / tran.Double("CnvFact");

                                    objTran.GetByKey(branchID, batNbr, tran.String("RefNbr"), tran.String("LineRef"));
                                    objTran.Qty = qtyUpdate;
                                    objTran.UnitCost = fl.Double("UnitCost");
                                    objTran.ExtCost = MathRound(qtyCal * fl.Double("UnitCost"), CostDecimalPlaces);
                                    objTran.TranAmt = MathRound(qtyUpdate * tran.Double("UnitPrice"), CostDecimalPlaces);
                                    objTran.CostID = fl.String("CostID");

                                    if (count == 0)
                                    {
                                        objTran.Update();
                                    }
                                    else
                                    {
                                        objTran.LineRef = lineRef;
                                        objTran.Add();
                                        lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                                        for (int i = lineRef.Length; lineRef.Length < 5; )
                                            lineRef = "0" + lineRef;

                                    }
                                    qtyCost = qtyCost + fl.Double("Qty");
                                    cost.GetByKey(fl.Int("CostIdentity"));
                                    cost.Qty = MathRound(cost.Qty - qtyCal, decimalPlace);
                                    cost.TotCost = MathRound(cost.TotCost - qtyCal * cost.UnitCost, CostDecimalPlaces);
                                    if (cost.Qty == 0 && cost.TotCost == 0)
                                        cost.Delete(fl.Int("CostIdentity"));
                                    else
                                        cost.Update();
                                }
                                else
                                {
                                    break;
                                }
                                count++;
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "738");
                        }

                    }

                }
                #endregion

                qty = 0;
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    if (!objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message, "606");
                    }
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                    }
                    if (!objSetup.NegQty && MathRound(objSite.QtyOnHand + qty, decimalPlace) < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.QtyAllocIN = MathRound(objSite.QtyAllocIN + qty, decimalPlace);
                    objSite.QtyOnHand = MathRound(objSite.QtyOnHand + qty, decimalPlace);
                    objSite.AvgCost = MathRound(objSite.QtyOnHand != 0
                                        ? (objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objSite.QtyOnHand
                                        : objSite.AvgCost, CostDecimalPlaces);

                    if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607", "", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.TotCost = MathRound(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                    objSite.LUpd_DateTime = DateTime.Now;
                    objSite.LUpd_Prog = Prog;
                    objSite.LUpd_User = User;
                    objSite.Update();
                    if (objBranchSite != null)
                    {
                        double tranQty = Math.Abs(qty);
                        if (!objBranchSite.GetByKey(tran.String("InvtID"), toSiteID))
                        {
                            Insert_IN_ItemSite(ref objBranchSite, tran.String("InvtID"), objInvt.StkItem, toSiteID, tranQty);
                        }
                        else
                        {
                            objBranchSite.QtyOnHand = MathRound(objBranchSite.QtyOnHand + tranQty, decimalPlace);
                            objBranchSite.QtyAvail = MathRound(objBranchSite.QtyAvail + tranQty, decimalPlace);
                            objBranchSite.LUpd_DateTime = DateTime.Now;
                            objBranchSite.LUpd_Prog = Prog;
                            objBranchSite.LUpd_User = User;
                            objBranchSite.Update();
                        }
                    }


                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.ExpDate = lotRow.Date("ExpDate");
                                objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");

                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }

                            if (!objSetup.NegQty && objItemLot.QtyOnHand + qty < 0)
                            {
                                throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID + "-" + objItemLot.LotSerNbr });
                            }

                            objItemLot.QtyAllocIN = MathRound(objItemLot.QtyAllocIN + qty, decimalPlace);
                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);

                            objItemLot.Cost = objSite.TotCost * objItemLot.QtyOnHand;
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();

                            if (objBranchLot != null)
                            {
                                double transLotQty = Math.Abs(qty);
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                                {
                                    Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.WhseLoc, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = MathRound(objBranchLot.QtyOnHand + transLotQty, decimalPlace);
                                    objBranchLot.QtyAvail = MathRound(objBranchLot.QtyAvail + transLotQty, decimalPlace);
                                    objBranchLot.LUpd_DateTime = DateTime.Now;
                                    objBranchLot.LUpd_Prog = Prog;
                                    objBranchLot.LUpd_User = User;
                                    objBranchLot.Update();
                                }

                            }

                        }
                    }

                    if (objInvt.ValMthd == "S")
                    {
                        sql.GetCostByCostID(ref cost, tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                        if (cost.CostIdentity > 0)
                        {
                            cost.TotCost = MathRound(cost.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"),CostDecimalPlaces);
                            if (cost.Qty == 0 && cost.TotCost == 0)
                                sql.DelCostByCostID(tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                            else
                                cost.Update();
                        }
                    }
                }
                sql.IN10201_ReleaseBatch(branchID, batNbr, Prog, User, transType);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool IN11200_Cancel(string branchID, string batNbr, string transType, string toSiteID)
        {
            try
            {
                string rcptNbr = string.Empty;
                bool release = true;
                double qty = 0;
                DataTable lstTrans = new DataTable();

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetAll(branchID, "IN");

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                if (string.IsNullOrEmpty(rcptNbr))
                    lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                else
                    lstTrans = objTran.GetAll(branchID, batNbr, rcptNbr, "%");

                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);

                clsIN_ItemSite objBranchSite = null;
                clsIN_ItemLot objBranchLot = null;
                if (transType == "BC")
                {
                    objBranchSite = new clsIN_ItemSite(Dal);
                    objBranchLot = new clsIN_ItemLot(Dal);
                }
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);
                clsSQL sql = new clsSQL(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    if (!release && (tran.Short("Rlsed") == -1 || tran.Short("InvtMult") == 1)) continue;

                    objInvt.GetByKey(tran.String("InvtID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (!objItem.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message, "606");
                    }

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                        objItem.QtyAvail = MathRound(objItem.QtyAvail + qty, decimalPlace);
                        objItem.QtyOnHand = MathRound(objItem.QtyOnHand + qty, decimalPlace);
                        objItem.AvgCost = MathRound(objItem.QtyOnHand != 0 ? (objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItem.QtyOnHand :
                            objItem.AvgCost, CostDecimalPlaces);

                    }
                    objItem.TotCost = MathRound(objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = Prog;
                    objItem.LUpd_User = User;
                    objItem.Update();

                    if (objBranchSite != null)
                    {
                        double tranQty = Math.Abs(qty);
                        if (!objBranchSite.GetByKey(tran.String("InvtID"), toSiteID))
                        {
                            throw new MessageException(MessageType.Message, "606"); //Insert_IN_ItemSite(ref objBranchSite, tran.String("InvtID"), objInvt.StkItem, toSiteID, tranQty);
                        }
                        else if (objBranchSite.QtyAvail == 0 || objBranchSite.QtyOnHand == 0)
                        {
                            throw new MessageException(MessageType.Message, "608", "", new[] { tran.String("InvtID"), toSiteID });
                        }
                        else
                        {
                            objBranchSite.QtyOnHand = MathRound(objBranchSite.QtyOnHand - tranQty, decimalPlace);
                            objBranchSite.QtyAvail = MathRound(objBranchSite.QtyAvail - tranQty, decimalPlace);
                            objBranchSite.LUpd_DateTime = DateTime.Now;
                            objBranchSite.LUpd_Prog = Prog;
                            objBranchSite.LUpd_User = User;
                            objBranchSite.Update();
                        }
                    }

                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            qty = -1 * MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                objItemLot.LIFODate = lotRow.Date("ExpDate");
                                objItemLot.ExpDate = lotRow.Date("ExpDate");
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();
                            }
                            objItemLot.Cost = objItem.TotCost;
                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                            objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);


                            if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItemLot.Cost, CostDecimalPlaces) < 0)
                            {
                                throw new MessageException("607", new[] { objInvt.InvtID, objItemLot.SiteID + " - " + objItemLot.LotSerNbr });
                            }

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();

                            if (objBranchLot != null)
                            {
                                double transLotQty = Math.Abs(qty);
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()) || objBranchLot.QtyAvail == 0 || objBranchLot.QtyOnHand == 0)
                                {
                                    throw new MessageException(MessageType.Message, "201508181", "", new[] { "", tran.String("InvtID") + " " + Util.GetLang("Site") + " " + toSiteID, lotRow.String("LotSerNbr"), transLotQty.ToString() });
                                    //throw new MessageException(MessageType.Message, "606"); //Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = MathRound(objBranchLot.QtyOnHand - transLotQty, decimalPlace);
                                    objBranchLot.QtyAvail = MathRound(objBranchLot.QtyAvail - transLotQty, decimalPlace);
                                    objBranchLot.LUpd_DateTime = DateTime.Now;
                                    objBranchLot.LUpd_Prog = Prog;
                                    objBranchLot.LUpd_User = User;
                                    objBranchLot.Update();
                                }
                            }
                        }
                    }

                    sql.GetCostByCostID(ref cost, tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                    if (cost.CostIdentity > 0)
                    {
                        cost.Qty = MathRound(cost.Qty + qty, decimalPlace);
                        cost.TotCost = MathRound(cost.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        cost.Update();
                    }
                    else
                    {
                        clsIN_ItemCost newItemCost = new clsIN_ItemCost(Dal)
                        {
                            CostID = tran.String("CostID"),
                            InvtID = tran.String("InvtID"),
                            Qty = tran.Double("Qty"),
                            RcptDate = tran.Date("TranDate"),
                            RcptNbr = tran.String("RefNbr"),
                            TotCost = tran.Double("ExtCost"),
                            UnitCost = tran.Double("UnitCost"),
                            SiteID = tran.String("SiteID"),
                            Crtd_DateTime = DateTime.Now,
                            Crtd_Prog = Prog,
                            Crtd_User = User,
                            LUpd_DateTime = DateTime.Now,
                            LUpd_Prog = Prog,
                            LUpd_User = User
                        };
                        newItemCost.Add();
                    }
                }
                if (release)
                    sql.IN10201_CancelBatch(branchID, batNbr, Prog, User, transType);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public bool IN11200_Release(string branchID, string batNbr, bool isTransfer, string transferNbr, string transType, string toSiteID)
        {

            clsSQL sql = new clsSQL(Dal);
            clsIN_Setup setup = new clsIN_Setup(Dal);
            clsIN_Trans tran = new clsIN_Trans(Dal);
            clsIN_Inventory inventory = new clsIN_Inventory(Dal);
            clsIN_ItemSite itemSite = new clsIN_ItemSite(Dal);
            clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
            clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
            setup.GetByKey(branchID, "IN");
            DataTable trans = tran.GetAll(branchID, batNbr, "%", "%");
            double qty = 0;
            string User = string.Empty;
            string prog = string.Empty;
            string refNbr = string.Empty;
            DateTime? tranDate = DateTime.Now;
            if (trans.Rows.Count > 0)
            {
                User = trans.Rows[0].String("LUpd_User");
                refNbr = trans.Rows[0].String("LUpd_User");
                prog = trans.Rows[0].String("LUpd_Prog");
                tranDate = trans.Rows[0].Date("TranDate");
            }

            foreach (DataRow inTran in trans.Rows)
            {

                inventory.GetByKey(inTran.String("InvtID"));
                var decimalPlace = GetDecimalPlace(inventory.LotSerTrack);

                if (!itemSite.GetByKey(inTran.String("InvtID"), inTran.String("SiteID")))
                {
                    itemSite.Reset();
                    itemSite.SiteID = inTran.String("SiteID");
                    itemSite.InvtID = inTran.String("InvtID");
                    itemSite.AvgCost = 0;
                    itemSite.Crtd_DateTime = DateTime.Now;
                    itemSite.Crtd_Prog = inTran.String("Crtd_Prog");
                    itemSite.Crtd_User = inTran.String("Crtd_User");
                    itemSite.LUpd_DateTime = DateTime.Now;
                    itemSite.LUpd_Prog = inTran.String("LUpd_Prog");
                    itemSite.LUpd_User = inTran.String("Crtd_User");
                    itemSite.LastPurchaseDate = DateTime.Now.Short();
                    itemSite.QtyAlloc = 0;
                    itemSite.QtyAllocIN = 0;
                    itemSite.QtyAllocPORet = 0;
                    itemSite.QtyAllocSO = 0;
                    itemSite.QtyAvail = 0;
                    itemSite.QtyInTransit = 0;
                    itemSite.QtyOnBO = 0;
                    itemSite.QtyOnHand = 0;
                    itemSite.QtyOnPO = 0;
                    itemSite.QtyOnSO = 0;
                    itemSite.QtyOnTransferOrders = 0;
                    itemSite.QtyShipNotInv = 0;
                    itemSite.QtyUncosted = 0;
                    itemSite.StkItem = inventory.StkItem;
                    itemSite.TotCost = 0;
                    itemSite.Add();
                }
                if (inventory.StkItem == 1)
                {
                    if (inTran.String("UnitMultDiv") == "M" || inTran.String("UnitMultDiv") == string.Empty)
                        qty = inTran.Double("Qty") * inTran.Short("InvtMult") * inTran.Double("CnvFact");
                    else
                        qty = inTran.Double("Qty") * inTran.Short("InvtMult") / inTran.Double("CnvFact");

                    if (isTransfer) itemSite.QtyInTransit -= qty;

                    itemSite.QtyOnHand = MathRound(itemSite.QtyOnHand + qty, decimalPlace);
                    itemSite.QtyAvail = MathRound(itemSite.QtyAvail + qty, decimalPlace);
                    itemSite.AvgCost = MathRound(itemSite.QtyOnHand > 0 ? (itemSite.TotCost + inTran.Double("ExtCost")) / itemSite.QtyOnHand : itemSite.AvgCost, CostDecimalPlaces);
                }
                itemSite.TotCost = MathRound(itemSite.TotCost + inTran.Double("ExtCost"), CostDecimalPlaces);
                itemSite.LUpd_DateTime = DateTime.Now;
                itemSite.LUpd_Prog = inTran.String("LUpd_Prog");
                itemSite.LUpd_User = inTran.String("LUpd_User");

                if (inventory.StkItem == 1 && inventory.LotSerTrack != "N" && inventory.PassNull() != string.Empty)
                {
                    DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", inTran.String("LineRef"));
                    foreach (DataRow lotRow in dtLot.Rows)
                    {
                        // chú ý check lại phần làm tròn này
                        qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace);
                        if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                        {
                            objItemLot.Reset();
                            objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                            objItemLot.InvtID = lotRow.String("InvtID");
                            objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                            objItemLot.SiteID = lotRow.String("SiteID");
                            objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                            objItemLot.LIFODate = lotRow.Date("ExpDate");
                            objItemLot.ExpDate = lotRow.Date("ExpDate");
                            objItemLot.Crtd_DateTime = DateTime.Now;
                            objItemLot.Crtd_Prog = Prog;
                            objItemLot.Crtd_User = User;
                            objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Add();
                        }

                        objItemLot.ExpDate = lotRow.Date("ExpDate");
                        objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);
                        objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                        objItemLot.Cost = itemSite.AvgCost * objItemLot.QtyOnHand;

                        objItemLot.LUpd_DateTime = DateTime.Now;
                        objItemLot.LUpd_Prog = Prog;
                        objItemLot.LUpd_User = User;
                        objItemLot.Update();
                    }
                }

                itemSite.Update();
            }
            if (isTransfer)
            {
                sql.IN10100_UpdateTransfer(branchID, batNbr, Prog, User, tranDate, refNbr, transferNbr, "R");
            }
            sql.IN_ReleaseBatch(branchID, batNbr, Prog, User);
            return true;

        }        
        public bool IN11200_Cancel(string branchID, string batNbr, bool isTransfer, string transferNbr, bool isCopy, string transType, string toSiteID)
        {
            try
            {
                if (Receipt_Cancel(branchID, batNbr, string.Empty, isTransfer, transferNbr, true) && isCopy)
                {
                    clsBatch objBatch = new clsBatch(Dal);
                    if (objBatch.GetByKey(branchID, "IN", batNbr))
                    {
                        clsBatch newBatch = new clsBatch(Dal)
                        {
                            BranchID = objBatch.BranchID,
                            DateEnt = objBatch.DateEnt,
                            Descr = objBatch.Descr,
                            EditScrnNbr = objBatch.EditScrnNbr,
                            FromToSiteID = objBatch.FromToSiteID,
                            ImpExp = objBatch.ImpExp,
                            IntRefNbr = objBatch.IntRefNbr,
                            JrnlType = objBatch.JrnlType,
                            Module1 = objBatch.Module1,
                            NoteID = objBatch.NoteID,
                            TotAmt = objBatch.TotAmt,
                            Status = "H",
                            RvdBatNbr = objBatch.BatNbr,
                            Rlsed = 0,
                            ReasonCD = objBatch.ReasonCD,
                            OrigScrnNbr = objBatch.OrigScrnNbr,
                            OrigBatNbr = objBatch.OrigBatNbr,
                            OrigBranchID = objBatch.OrigBranchID,
                        };
                        clsSQL sql = new clsSQL(Dal);
                        newBatch.BatNbr = sql.INNumbering(branchID, "BatNbr");
                        newBatch.RefNbr = sql.INNumbering(branchID, "RefNbr");
                        newBatch.LUpd_DateTime = newBatch.Crtd_DateTime = DateTime.Now;
                        newBatch.LUpd_Prog = newBatch.Crtd_Prog = Prog;
                        newBatch.LUpd_User = newBatch.Crtd_User = User;
                        newBatch.Add();

                        clsIN_Trans objTran = new clsIN_Trans(Dal);
                        DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                        foreach (DataRow tran in lstTrans.Rows)
                        {
                            clsIN_Trans newTran = new clsIN_Trans(Dal)
                            {
                                JrnlType = tran.String("JrnlType"),
                                ReasonCD = tran.String("ReasonCD"),
                                RefNbr = newBatch.RefNbr,
                                Rlsed = 0,
                                BatNbr = newBatch.BatNbr,
                                BranchID = newBatch.BranchID,
                                CnvFact = tran.Double("CnvFact"),
                                CostID = tran.String("CostID"),
                                ExtCost = tran.Double("ExtCost"),
                                FreeItem = tran.Bool("FreeItem"),
                                InvtID = tran.String("InvtID"),
                                InvtMult = tran.Short("InvtMult"),
                                LineRef = tran.String("LineRef"),
                                ObjID = tran["ObjID"].ToString(),
                                Qty = tran.Double("Qty"),
                                SiteID = tran.String("SiteID"),
                                QtyUncosted = tran.Double("QtyUncosted"),
                                ToSiteID = tran.String("ToSiteID"),
                                SlsperID = tran.String("SlsperID"),
                                ShipperID = tran.String("ShipperID"),
                                UnitPrice = tran.Double("UnitPrice"),
                                UnitMultDiv = tran.String("UnitMultDiv"),
                                UnitDesc = tran.String("UnitDesc"),
                                UnitCost = tran.Double("UnitCost"),
                                TranType = tran.String("TranType"),
                                TranFee = tran.Double("TranFee"),
                                TranAmt = tran.Double("TranAmt"),
                                TranDesc = tran.String("TranDesc"),
                                TranDate = tran.Date("TranDate"),
                                ShipperLineRef = tran.String("ShipperLineRef"),
                                WhseLoc = tran.String("WhseLoc")
                            };
                            newTran.LUpd_DateTime = newTran.Crtd_DateTime = DateTime.Now;
                            newTran.LUpd_Prog = newTran.Crtd_Prog = Prog;
                            newTran.LUpd_User = newTran.Crtd_User = User;
                            newTran.Add();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IN11700_Release(string branchID, string batNbr)
        {
            try
            {
                double qty = 0;
                string lineRef = string.Empty;
                string User = string.Empty;
                string prog = string.Empty;
                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");
                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_ItemLoc objItemLoc = new clsIN_ItemLoc(Dal);
                clsSQL sql = new clsSQL(Dal);
                if (lstTrans.Rows.Count > 0)
                {
                    User = lstTrans.Rows[0].String("LUpd_User");
                    prog = lstTrans.Rows[0].String("LUpd_Prog");
                }

                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    //// IN_ItemLoc
                    if (tran.String("WhseLoc").PassNull() != "" && tran.String("ToWhseLoc").PassNull() != "")
                    {
                        if (tran.Short("InvtMult") == -1)
                        {
                            if (!objItemLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc")))
                            {
                                throw new MessageException(MessageType.Message, "2018052414");
                            }
                        }
                        else
                        {
                            if (!objItemLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("ToWhseLoc")))
                            {
                                throw new MessageException(MessageType.Message, "2018052414");
                            }
                        }
                           
                        if (objInvt.StkItem == 1)
                        {
                            if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv").PassNull() == string.Empty)
                                qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                            else
                                qty = (tran.Double("Qty") * tran.Short("InvtMult")) / tran.Double("CnvFact");

                            //objItemLoc.QtyAllocIN = MathRound(objItemLoc.QtyAllocIN + qty, 0);
                            objItemLoc.QtyOnHand = MathRound(objItemLoc.QtyOnHand + qty, decimalPlace);
                            if (qty < 0) objItemLoc.QtyAllocIN = MathRound(objItemLoc.QtyAllocIN + qty, decimalPlace);
                            if (qty > 0)
                            {
                                objItemLoc.QtyAvail = MathRound(objItemLoc.QtyAvail + qty, decimalPlace);
                            }
                        }
                        if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItemLoc.TotCost + tran.Double("ExtCost"), CostDecimalPlaces) < 0)
                        {
                            throw new MessageException(MessageType.Message, "2018052413", "", new[] { objItemLoc.InvtID, objItemLoc.SiteID, objItemLoc.WhseLoc });
                        }
                        objItemLoc.TotCost = MathRound(objItemLoc.TotCost + tran.Double("ExtCost"), CostDecimalPlaces);
                        objItemLoc.AvgCost = MathRound(objItemLoc.QtyOnHand == 0 ? objItemLoc.AvgCost : objItemLoc.TotCost / objItemLoc.QtyOnHand, CostDecimalPlaces); // tinh lai AvgCost 20160624
                        objItemLoc.LUpd_DateTime = DateTime.Now;
                        objItemLoc.LUpd_Prog = prog;
                        objItemLoc.LUpd_User = User;
                        objItemLoc.Update();

                    }
                    //////


                    //objInvt.GetByKey(tran.String("InvtID"));
                    if (!objItem.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message, "606");
                    }
                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv").PassNull() == string.Empty)
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = (tran.Double("Qty") * tran.Short("InvtMult")) / tran.Double("CnvFact");

                        //objItem.QtyAllocIN = MathRound(objItem.QtyAllocIN + qty, 0);
                        objItem.QtyOnHand = MathRound(objItem.QtyOnHand + qty, decimalPlace);
                        if (qty < 0) objItem.QtyAllocIN = MathRound(objItem.QtyAllocIN + qty, decimalPlace);
                        if (qty > 0)
                        {
                            objItem.QtyAvail = MathRound(objItem.QtyAvail + qty, decimalPlace);
                        }
                    }
                    if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItem.TotCost + tran.Double("ExtCost"), CostDecimalPlaces) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607", "", new[] { objItem.InvtID, objItem.SiteID });
                    }
                    objItem.TotCost = MathRound(objItem.TotCost + tran.Double("ExtCost"), CostDecimalPlaces);
                    objItem.AvgCost = MathRound(objItem.QtyOnHand == 0 ? objItem.AvgCost : objItem.TotCost / objItem.QtyOnHand, CostDecimalPlaces); // tinh lai AvgCost 20160624
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = prog;
                    objItem.LUpd_User = User;
                    objItem.Update();



                    ///them lot 20160527
                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            qty = MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (tran.Short("InvtMult") == -1)
                            {
                                if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                                {
                                    objItemLot.Reset();
                                    objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                    objItemLot.InvtID = lotRow.String("InvtID");
                                    objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                    objItemLot.SiteID = lotRow.String("SiteID");
                                    objItemLot.ExpDate = lotRow.Date("ExpDate");
                                    objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                    objItemLot.LIFODate = lotRow.Date("ExpDate");
                                    objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                                    objItemLot.Crtd_DateTime = DateTime.Now;
                                    objItemLot.Crtd_Prog = Prog;
                                    objItemLot.Crtd_User = User;
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Add();
                                }
                            }
                            else
                            {
                                if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), tran.String("ToWhseLoc"), lotRow.String("LotSerNbr")))
                                {
                                    objItemLot.Reset();
                                    objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                    objItemLot.InvtID = lotRow.String("InvtID");
                                    objItemLot.WhseLoc = tran.String("ToWhseLoc");
                                    objItemLot.SiteID = lotRow.String("SiteID");
                                    objItemLot.ExpDate = lotRow.Date("ExpDate");
                                    objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                    objItemLot.LIFODate = lotRow.Date("ExpDate");
                                    objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                                    objItemLot.Crtd_DateTime = DateTime.Now;
                                    objItemLot.Crtd_Prog = Prog;
                                    objItemLot.Crtd_User = User;
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Add();
                                }
                            }
                            if (!objSetup.NegQty && objItemLot.QtyOnHand + qty < 0)
                            {
                                throw new MessageException(MessageType.Message, "2018052413", "", new[] { objItemLot.InvtID, objItemLot.SiteID + "-" + objItemLot.LotSerNbr, objItemLot.WhseLoc });
                            }

                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                            if (qty < 0) objItemLot.QtyAllocIN = MathRound(objItemLot.QtyAllocIN + qty, decimalPlace);
                            if (qty > 0)
                            {
                                objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);
                            }
                            objItemLot.Cost = objItem.TotCost * objItemLot.QtyOnHand;
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();
                        }
                    }
                }
                sql.IN_ReleaseBatch(branchID, batNbr, Prog, User);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IN11700_Cancel(string branchID, string batNbr)
        {
            try
            {
                IN11700Issue_Cancel(branchID, batNbr, string.Empty, true);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool IN11700Issue_Cancel(string branchID, string batNbr, string rcptNbr, bool release)
        {
            try
            {
                double qty = 0;
                DataTable lstTrans = new DataTable();

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetAll(branchID, "IN");

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                if (string.IsNullOrEmpty(rcptNbr))
                    lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                else
                    lstTrans = objTran.GetAll(branchID, batNbr, rcptNbr, "%");

                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_ItemLoc objItemLoc = new clsIN_ItemLoc(Dal);
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);
                clsSQL sql = new clsSQL(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    //if (!release && (tran.Short("Rlsed") == -1 || tran.Short("InvtMult") == 1)) continue;

                    objInvt.GetByKey(tran.String("InvtID"));
                    var decimalPlace = GetDecimalPlace(objInvt.LotSerTrack);

                    if (!objItem.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message, "606");

                    }
                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                        objItem.QtyAvail = MathRound(objItem.QtyAvail + qty, decimalPlace);
                        objItem.QtyOnHand = MathRound(objItem.QtyOnHand + qty, decimalPlace);
                        objItem.AvgCost = MathRound(objItem.QtyOnHand != 0 ? (objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItem.QtyOnHand :
                            objItem.AvgCost, CostDecimalPlaces);

                    }
                    objItem.TotCost = MathRound(objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = Prog;
                    objItem.LUpd_User = User;
                    objItem.Update();

                    if (tran.String("WhseLoc").PassNull() != "" && tran.String("ToWhseLoc").PassNull() != "")
                    {
                        if (tran.Short("InvtMult") == -1)
                        {
                            if (!objItemLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc").PassNull()))
                            {
                                throw new MessageException(MessageType.Message, "606");

                            }
                        }
                        else
                        {
                            if (!objItemLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("ToWhseLoc").PassNull()))
                            {
                                throw new MessageException(MessageType.Message, "606");

                            }
                        }
                       
                        if (objInvt.StkItem == 1)
                        {
                            if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                                qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                            else
                                qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                            objItemLoc.QtyAvail = MathRound(objItemLoc.QtyAvail + qty, decimalPlace);
                            objItemLoc.QtyOnHand = MathRound(objItemLoc.QtyOnHand + qty, decimalPlace);
                            objItemLoc.AvgCost = MathRound(objItemLoc.QtyOnHand != 0 ? (objItemLoc.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItemLoc.QtyOnHand :
                                objItemLoc.AvgCost, CostDecimalPlaces);

                        }
                        objItemLoc.TotCost = MathRound(objItemLoc.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        objItemLoc.LUpd_DateTime = DateTime.Now;
                        objItemLoc.LUpd_Prog = Prog;
                        objItemLoc.LUpd_User = User;
                        objItemLoc.Update();
                    }


                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                    {
                        DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", tran.String("LineRef"));
                        foreach (DataRow lotRow in dtLot.Rows)
                        {
                            qty = -1 * MathRound(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), decimalPlace) * lotRow.Short("InvtMult");
                            if (tran.Short("InvtMult") == -1)
                            {
                                if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr"), lotRow.String("WhseLoc").PassNull()))
                                {
                                    objItemLot.Reset();
                                    objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                    objItemLot.InvtID = lotRow.String("InvtID");
                                    objItemLot.SiteID = lotRow.String("SiteID");
                                    objItemLot.WhseLoc = lotRow.String("WhseLoc").PassNull();
                                    objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                    objItemLot.LIFODate = lotRow.Date("ExpDate");
                                    objItemLot.ExpDate = lotRow.Date("ExpDate");
                                    objItemLot.Crtd_DateTime = DateTime.Now;
                                    objItemLot.Crtd_Prog = Prog;
                                    objItemLot.Crtd_User = User;

                                    objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");

                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Add();
                                }
                            }
                            else
                            {
                                if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), tran.String("ToWhseLoc").PassNull(), lotRow.String("LotSerNbr")))
                                {
                                    objItemLot.Reset();
                                    objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                    objItemLot.InvtID = lotRow.String("InvtID");
                                    objItemLot.SiteID = lotRow.String("SiteID");
                                    objItemLot.WhseLoc = tran.String("ToWhseLoc");
                                    objItemLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                    objItemLot.LIFODate = lotRow.Date("ExpDate");
                                    objItemLot.ExpDate = lotRow.Date("ExpDate");
                                    objItemLot.Crtd_DateTime = DateTime.Now;
                                    objItemLot.Crtd_Prog = Prog;
                                    objItemLot.Crtd_User = User;

                                    objItemLot.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");

                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Add();
                                }
                            }
                            objItemLot.Cost = objItem.TotCost;
                            objItemLot.QtyOnHand = MathRound(objItemLot.QtyOnHand + qty, decimalPlace);
                            objItemLot.QtyAvail = MathRound(objItemLot.QtyAvail + qty, decimalPlace);


                            if (!objSetup.NegQty && objSetup.CheckINVal && MathRound(objItemLot.Cost, CostDecimalPlaces) < 0)
                            {
                                throw new MessageException("607", new[] { objInvt.InvtID, objItemLot.SiteID + " - " + objItemLot.LotSerNbr });
                            }

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();
                        }
                    }


                    sql.GetCostByCostID(ref cost, tran.String("InvtID"), tran.String("SiteID"), tran.String("CostID"));
                    if (cost.CostIdentity > 0)
                    {
                        cost.Qty = MathRound(cost.Qty + qty, decimalPlace);
                        cost.TotCost = MathRound(cost.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), CostDecimalPlaces);
                        cost.Update();
                    }
                    else
                    {
                        clsIN_ItemCost newItemCost = new clsIN_ItemCost(Dal)
                        {
                            CostID = tran.String("CostID"),
                            InvtID = tran.String("InvtID"),
                            Qty = tran.Double("Qty"),
                            RcptDate = tran.Date("TranDate"),
                            RcptNbr = tran.String("RefNbr"),
                            TotCost = tran.Double("ExtCost"),
                            UnitCost = tran.Double("UnitCost"),
                            SiteID = tran.String("SiteID"),
                            Crtd_DateTime = DateTime.Now,
                            Crtd_Prog = Prog,
                            Crtd_User = User,
                            LUpd_DateTime = DateTime.Now,
                            LUpd_Prog = Prog,
                            LUpd_User = User
                        };
                        newItemCost.Add();
                    }

                }
                if (release)
                    sql.IN_CancelBatch(branchID, batNbr, Prog, User);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private int GetDecimalPlace(string lotSerTrack)
        {
            return (lotSerTrack != "Q" ? 0 : 2);
        }

        private double MathRound(double value, int digits)
        {
            value = Math.Round(value, digits);
            return value;
        }
    }
}
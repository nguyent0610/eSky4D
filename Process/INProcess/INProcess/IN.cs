using System;
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
            clsIN_ItemLoc itemLoc = new clsIN_ItemLoc(Dal);
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
                    itemSite.QtyOnHand = Math.Round(itemSite.QtyOnHand + qty, decimalPlace);
                    itemSite.QtyAvail = Math.Round(itemSite.QtyAvail + qty, decimalPlace);
                    itemSite.AvgCost = Math.Round(itemSite.QtyOnHand > 0 ? (itemSite.TotCost + inTran.Double("ExtCost")) / itemSite.QtyOnHand : itemSite.AvgCost, decimalPlace);                    
                }
                itemSite.TotCost = Math.Round(itemSite.TotCost + inTran.Double("ExtCost"), 0);
                itemSite.LUpd_DateTime = DateTime.Now;
                itemSite.LUpd_Prog = inTran.String("LUpd_Prog");
                itemSite.LUpd_User = inTran.String("LUpd_User");
                itemSite.Update();
                #endregion

                #region -Save IN_itemLoc-
                if (!string.IsNullOrWhiteSpace(inTran.String("WhseLoc")))
                {
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
                        itemLoc.QtyOnHand = Math.Round(itemLoc.QtyOnHand + qty, decimalPlace);
                        itemLoc.QtyAvail = Math.Round(itemLoc.QtyAvail + qty, decimalPlace);
                        itemLoc.AvgCost = Math.Round(itemLoc.QtyOnHand > 0 ? (itemLoc.TotCost + inTran.Double("ExtCost")) / itemLoc.QtyOnHand : itemLoc.AvgCost, decimalPlace);
                    }
                    itemLoc.TotCost = Math.Round(itemLoc.TotCost + inTran.Double("ExtCost"), 0);
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
                        var decimalPlace = inventory.LotSerTrack == "Q" ? 2 : 0;
                        qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 10);                       
                        
                        if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                        {
                            objItemLot.Reset();
                            objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                            objItemLot.InvtID = lotRow.String("InvtID");
                            objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                            }
                        }

                        objItemLot.ExpDate = lotRow.Date("ExpDate");
                        
                        objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + qty, decimalPlace);
                        objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, decimalPlace);
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
        public bool IN10200_Release(string branchID, string batNbr, bool showWhseLoc)
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
                clsIN_ItemLoc objLoc = new clsIN_ItemLoc(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID"));
                    objLoc.GetByKey(tran.String("InvtID"), tran.String("SiteID"), tran.String("WhseLoc"));

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
                                    objTran.ExtCost = Math.Round(qtyCal * fl.Double("UnitCost"), 0);
                                    objTran.TranAmt = Math.Round(qtyUpdate * tran.Double("UnitPrice"), 0);
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
                                    cost.Qty= Math.Round(cost.Qty - qtyCal);
                                    cost.TotCost = Math.Round(cost.TotCost - qtyCal * cost.UnitCost, 0);
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

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                    }
                    if (!objSetup.NegQty && Math.Round(objSite.QtyOnHand + qty, 0) < 0)
                    {
                        throw new MessageException(MessageType.Message, "608","", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    if (tran.String("JrnlType") == "IN" || (tran.String("JrnlType") == "OM" && Prog == "OM10300"))
                    {
                        objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + qty, 0);
                        objSite.QtyOnHand = Math.Round(objSite.QtyOnHand + qty, 0);
                        objSite.AvgCost = Math.Round(objSite.QtyOnHand != 0
                                           ? (objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objSite.QtyOnHand
                                           : objSite.AvgCost, 0);
                    }
                    else if (tran.String("JrnlType") == "OM" || Prog != "OM10300")
                    {
                        objSite.QtyShipNotInv = Math.Round(objSite.QtyShipNotInv + (tran.String("TranType") == "IN" ? qty : 0), 0);
                        objSite.QtyOnHand = Math.Round(objSite.QtyOnHand + qty, 0);
                        objSite.QtyAvail = Math.Round(objSite.QtyAvail + (tran.String("TranType") == "CM" ? qty : 0), 0);
                    }
                    if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607","", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.TotCost = Math.Round(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
                    objSite.LUpd_DateTime = DateTime.Now;
                    objSite.LUpd_Prog = Prog;
                    objSite.LUpd_User = User;
                    objSite.Update();


                    ///////////////////////////////////////////////////////////Cập nhật vào table IN_ItemLoc

                    if (showWhseLoc)
                    {
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
                        if (!objSetup.NegQty && Math.Round(objLoc.QtyOnHand + qty, 0) < 0)
                        {
                            throw new MessageException(MessageType.Message, "2018051411", "", new[] { objLoc.InvtID, objLoc.SiteID, objLoc.WhseLoc });
                        }

                        if (tran.String("JrnlType") == "IN" || (tran.String("JrnlType") == "OM" && Prog == "OM10300"))
                        {
                            objLoc.QtyAllocIN = Math.Round(objLoc.QtyAllocIN + qty, 0);
                            objLoc.QtyOnHand = Math.Round(objLoc.QtyOnHand + qty, 0);
                            objLoc.AvgCost = Math.Round(objLoc.QtyOnHand != 0
                                               ? (objLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objLoc.QtyOnHand
                                               : objLoc.AvgCost, 0);
                        }
                        else if (tran.String("JrnlType") == "OM" || Prog != "OM10300")
                        {
                            objLoc.QtyShipNotInv = Math.Round(objLoc.QtyShipNotInv + (tran.String("TranType") == "IN" ? qty : 0), 0);
                            objLoc.QtyOnHand = Math.Round(objLoc.QtyOnHand + qty, 0);
                            objLoc.QtyAvail = Math.Round(objLoc.QtyAvail + (tran.String("TranType") == "CM" ? qty : 0), 0);
                        }
                        if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0) < 0)
                        {
                            throw new MessageException(MessageType.Message, "2018051411", "", new[] { objLoc.InvtID, objLoc.SiteID, objLoc.WhseLoc });
                        }

                        objLoc.TotCost = Math.Round(objLoc.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                            qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc").PassNull(), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                                objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + qty, 0);
                                objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, 0);
                              
                            }
                            else if (tran.String("JrnlType") == "OM" || Prog != "OM10300")
                            {
                                objItemLot.QtyShipNotInv = Math.Round(objItemLot.QtyShipNotInv + (lotRow.String("TranType") == "IN" ? qty : 0), GetDecimalPlace(objInvt.LotSerTrack));
                                objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, GetDecimalPlace(objInvt.LotSerTrack));
                                objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + (lotRow.String("TranType") == "CM" ? qty : 0), GetDecimalPlace(objInvt.LotSerTrack));
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
                            cost.TotCost= Math.Round(cost.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"));
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
        public bool IN10200_Cancel(string branchID, string batNbr, bool showWhseLoc)
        {
            try
            {
                Issue_Cancel(branchID, batNbr, string.Empty, true, showWhseLoc);
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
                clsSQL sql = new clsSQL(Dal);

                foreach (DataRow transfer in lstTransfer.Rows)
                {
                    DataTable lstTrans = objTran.GetAll(branchID, batNbr, transfer.String("RefNbr"), "%");
                    lineRef = string.Empty;
                    foreach (DataRow tran in lstTrans.Rows)
                    {
                        objInvt.GetByKey(tran["InvtID"].ToString());

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

                            objItem.QtyAllocIN = Math.Round(objItem.QtyAllocIN + qty, 0);
                            objItem.QtyOnHand = Math.Round(objItem.QtyOnHand + qty, 0);
                            objItem.AvgCost = Math.Round(  objItem.QtyOnHand != 0 ? (objItem.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"))/objItem.QtyOnHand
                                        : objItem.AvgCost, 0);
                        }
                        if(!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objItem.TotCost+ tran.Double("ExtCost") * tran.Short("InvtMult"),0)<0)
                        {
                            throw new MessageException(MessageType.Message, "607","", new[] { objItem.InvtID, objItem.SiteID });
                        }

                        objItem.TotCost = Math.Round(objItem.TotCost + tran.Double("ExtCost")*tran.Short("InvtMult"), 0);
                        objItem.LUpd_DateTime = DateTime.Now;
                        objItem.LUpd_Prog = prog;
                        objItem.LUpd_User = User;
                        objItem.Update();

                        clsIN_ItemSite objToItem=new clsIN_ItemSite(Dal);
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
                            objToItem.QtyInTransit = Math.Round(objToItem.QtyInTransit + Math.Abs(qty), 0);
                            objToItem.Update();
                        }

                        if(objInvt.StkItem ==1 && transfer.String("TransferType")=="1")
                        {
                            if(lineRef==string.Empty)
                            {
                                try 
	                            {	        
		                            lineRef=lstTrans.Compute("Max(LineRef)","").ToString();
	                            }
	                            catch (Exception)
	                            {
		                            lineRef="0";
	                            }
                            }
                            lineRef = Utility.LastLineRef(Convert.ToInt32(lineRef)); 

                            clsIN_Trans newTran=new clsIN_Trans(Dal);
                            newTran.RefNbr = tran.String("RefNbr");
                            newTran.BranchID = tran.String("BranchID");
                            newTran.BatNbr = tran.String("BatNbr");
                            newTran.LineRef = lineRef;
                            newTran.CnvFact = tran.Double("CnvFact");
                            newTran.ExtCost = Math.Round(tran.Double("ExtCost"), 0);
                            newTran.InvtID = tran.String("InvtID");
                            newTran.InvtMult = 1;
                            newTran.JrnlType = tran.String("JrnlType");
                            newTran.ObjID = tran.String("ObjID");
                            newTran.Qty = tran.Double("Qty");
                            newTran.ReasonCD = tran.String("ReasonCD");
                            newTran.Rlsed = 1;
                            newTran.SlsperID = tran.String("SlsperID");
                            newTran.SiteID = tran.String("ToSiteID");
                            newTran.ShipperID = tran.String("ShipperID");
                            newTran.ShipperLineRef = tran.String("ShipperLineRef");
                            newTran.ToSiteID = string.Empty;
                            newTran.TranAmt = Math.Round(tran.Double("TranAmt"), 0);
                            newTran.TranFee = tran.Double("TranFee");
                            newTran.TranDate =tran.Date("TranDate");
                            newTran.TranDesc = tran["TranDesc"].ToString();
                            newTran.TranType = tran["TranType"].ToString();
                            newTran.UnitCost = Math.Round(tran.Double("UnitCost"), 0);
                            newTran.UnitDesc = tran.String("UnitDesc");
                            newTran.UnitPrice = Math.Round(tran.Double("UnitPrice"), 0);
                            newTran.UnitMultDiv = tran.String("UnitMultDiv");
                            newTran.Crtd_DateTime = newTran.LUpd_DateTime = DateTime.Now;
                            newTran.Crtd_Prog = newTran.LUpd_Prog = prog;
                            newTran.Crtd_User = newTran.LUpd_User = User;
                            newTran.Add();         

                            objToItem.QtyOnHand = Math.Round(objToItem.QtyOnHand + Math.Abs(qty), 0);
                            objToItem.QtyAvail = Math.Round(objToItem.QtyAvail + Math.Abs(qty), 0);
                     
                            objToItem.TotCost = Math.Round(objToItem.TotCost + tran.Double("ExtCost"), 0);
                            objToItem.AvgCost = Math.Round(objToItem.QtyOnHand != 0 ? objToItem.TotCost/objToItem.QtyOnHand: objToItem.AvgCost, 0);
                            objToItem.LUpd_DateTime = DateTime.Now;
                            objToItem.LUpd_Prog = prog;
                            objToItem.LUpd_User = User;
                            objToItem.Update();
                        }

                        if (objInvt.StkItem == 1 && objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N")
                        {
                            DataTable dtLot = objLot.GetAll(branchID, batNbr, transfer.String("RefNbr"),"%", tran.String("LineRef"));
                            foreach (DataRow lotRow in dtLot.Rows)
                            {
                                if (objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                                {
                                    double lotQty = 0;
                                    if (lotRow.String("UnitMultDiv") == "M" || lotRow.String("UnitMultDiv").PassNull() == string.Empty)
                                        lotQty = lotRow.Double("Qty") * lotRow.Short("InvtMult") * lotRow.Double("CnvFact");
                                    else
                                        lotQty = (lotRow.Double("Qty") * lotRow.Short("InvtMult")) / lotRow.Double("CnvFact");

                                    objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + qty, 0);
                                    objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, 0);

                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Update();
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "606");
                                }

                                if (!objItemLot.GetByKey(lotRow.String("ToSiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                                {
                                    objItemLot.Reset();
                                    objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                    objItemLot.InvtID = lotRow.String("InvtID");
                                    objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                                    newLot.TranDate = tran.Date("TranDate");                                 
                                    newLot.TranType = tran["TranType"].ToString();
                                    newLot.UnitCost = Math.Round(lotRow.Double("UnitCost"), 0);
                                    newLot.UnitDesc = lotRow.String("UnitDesc");
                                    newLot.UnitPrice = Math.Round(lotRow.Double("UnitPrice"), 0);
                                    newLot.UnitMultDiv = lotRow.String("UnitMultDiv");
                                    newLot.ExpDate = lotRow.Date("ExpDate");
                                    newLot.WarrantyDate = lotRow.Date("WarrantyDate");
                                    newLot.WhseLoc = lotRow.String("WhseLoc");
                                    newLot.Crtd_DateTime = newLot.LUpd_DateTime = DateTime.Now;
                                    newLot.Crtd_Prog = newLot.LUpd_Prog = Prog;
                                    newLot.Crtd_User = newLot.LUpd_User = User;
                                    newLot.Add();

                                    objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + Math.Abs(newLot.Qty), 0);
                                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + Math.Abs(newLot.Qty), 0);

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

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = objTran.GetAll(branchID, batNbr, "%", "%");
                clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
              
                string User = string.Empty;
                string prog = string.Empty;
                if (lstTrans.Rows.Count > 0)
                {
                    User = lstTrans.Rows[0].String("LUpd_User");
                    prog = lstTrans.Rows[0].String("LUpd_Prog");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsSQL sql = new clsSQL(Dal);

                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));

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

                        //objItem.QtyAllocIN = Math.Round(objItem.QtyAllocIN + qty, 0);
                        objItem.QtyOnHand = Math.Round(objItem.QtyOnHand + qty, 0);
                        if (qty < 0) objItem.QtyAllocIN = Math.Round(objItem.QtyAllocIN + qty, 0);
                        if (qty > 0)
                        {
                            objItem.QtyAvail = Math.Round(objItem.QtyAvail + qty, 0);
                          
                        }
                    }
                    if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objItem.TotCost + tran.Double("ExtCost"), 0) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607","", new[] { objItem.InvtID, objItem.SiteID });
                    }
                    objItem.TotCost = Math.Round(objItem.TotCost + tran.Double("ExtCost"), 0);
                    objItem.AvgCost = Math.Round(objItem.TotCost / objItem.QtyOnHand, 0); // tinh lai AvgCost 20160624
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
                            qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                                throw new MessageException(MessageType.Message, "608", "", new[] { objItemLot.InvtID, objItemLot.SiteID + "-" + objItemLot.LotSerNbr });
                            }

                           
                            
                            objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, 0);
                            if (qty<0) objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + qty, 0);
                            if (qty > 0)
                            {                           
                                objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + qty, 0);
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
        public bool IN10500_Release(string tagID, string branchID, string siteID)
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
                DataTable lstTagDetail = objTagDetail.GetAll(tagID, "%", branchID, siteID); //objTagDetail.GetAll(tagID, siteID, "%"); 


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
                clsIN_UnitConversion objCnv = new clsIN_UnitConversion(Dal);
                var totAmt = 0.0;
                foreach (DataRow tagDetail in lstTagDetail.Rows)
                {
                    objInvt.GetByKey(tagDetail.String("InvtID").ToString());
                    objCnv.GetByKey("3", "*", tagDetail.String("InvtID"), "THUNG", objInvt.StkUnit);
                    
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
                        LineRef = lineRef                        
                    };                   

                    lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                    for (int i = lineRef.Length; lineRef.Length < 5; )
                    {
                        lineRef = "0" + lineRef;
                    }
                    if (objItem.GetByKey(tagDetail.String("InvtID"), tagDetail.String("SiteID")))
                    {
                        objItem.QtyOnHand = objItem.QtyOnHand + tagDetail.Double("OffsetEAQty");
                        objItem.QtyAvail = objItem.QtyAvail + tagDetail.Double("OffsetEAQty");
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
                    // Quản lý LOT
                    if (objInvt.LotSerTrack == "L")
                    {
                        clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                        clsIN_LotTrans objLot = new clsIN_LotTrans(Dal);
                        ParamCollection pc = new ParamCollection();
                        try
                        {
                            var isBreak = false;
                            if (tagDetail.Double("OffsetEAQty") > 0) // Thêm vào IN_ItemLot
                            {
                                pc.Add(new ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(objInvt.InvtID), ParameterDirection.Input, 30));
                                pc.Add(new ParamStruct("@TranDate", DbType.Date, clsCommon.GetValueDBNull(newTran.TranDate.ToDateShort()), ParameterDirection.Input, 30));
                                pc.Add(new ParamStruct("@GetType", DbType.String, clsCommon.GetValueDBNull("LotNbr"), ParameterDirection.Input, 30));
                                var lotNbr = (Dal.ExecDataTable("INNumberingLot", CommandType.StoredProcedure, ref pc, "").Rows[0]["Nbr"].ToString());
                                clsIN_LotTrans newLotTrans = new clsIN_LotTrans(Dal)
                                {
                                    BatNbr = batNbr,
                                    BranchID = branchID,
                                    RefNbr = newTran.RefNbr,
                                    LotSerNbr = lotNbr,
                                    INTranLineRef = newTran.LineRef,
                                    ExpDate = DateTime.Now,
                                    InvtID = newTran.InvtID,
                                    InvtMult = newTran.InvtMult,
                                    KitID = "", // ko bit
                                    MfgrLotSerNbr = "",
                                    Qty = newTran.Qty,
                                    SiteID = newTran.SiteID,
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
                                objItemLot.Reset();                               
                                objItemLot.InvtID = objItem.InvtID;
                                objItemLot.SiteID = objItem.SiteID;
                                objItemLot.WhseLoc = newTran.WhseLoc;
                                objItemLot.LotSerNbr = lotNbr;
                               // objItemLot.Cost = newTran.UnitCost;
                                
                                objItemLot.WarrantyDate = DateTime.Now;
                                objItemLot.LIFODate = DateTime.Now;
                                objItemLot.MfgrLotSerNbr = newLotTrans.MfgrLotSerNbr;
                                objItemLot.ExpDate = DateTime.Now;
                                objItemLot.Crtd_DateTime = DateTime.Now;
                                objItemLot.Crtd_Prog = Prog;
                                objItemLot.Crtd_User = User;

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Add();

                                objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + tagDetail.Double("OffsetEAQty"), 0);
                                if (tagDetail.Double("OffsetEAQty") < 0) objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + tagDetail.Double("OffsetEAQty"), 0);
                                if (tagDetail.Double("OffsetEAQty") > 0)
                                {
                                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + tagDetail.Double("OffsetEAQty"), 0);
                                }

                                objItemLot.Cost = objItem.TotCost * objItemLot.QtyOnHand;
                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = Prog;
                                objItemLot.LUpd_User = User;
                                objItemLot.Update();
                                newLotTrans.Add();
                            }
                            else // Kiểm tra trừ vào IN_ItemLot
                            {
                                var qty = Math.Abs(tagDetail.Double("OffsetEAQty"));
                                DataView dv = objItemLot.GetAll(siteID, objItem.InvtID, "%", "%").DefaultView;
                                dv.Sort = "ExpDate ASC";
                                DataTable lstLotTran = dv.ToTable();
                                var valTran = 0.0;
                                foreach (DataRow itmLot in lstLotTran.Rows)
                                {

                                }
                                foreach (DataRow itmLot in lstLotTran.Rows)
                                {
                                    var qtyAvail = itmLot.Double("QtyAvail");
                                    if (qtyAvail <= 0)
                                    {
                                        continue;
                                    }
                                    objItemLot.GetByKey(itmLot.String("SiteID"), itmLot.String("InvtID"), itmLot.String("WhseLoc"), itmLot.String("LotSerNbr"));
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    if (Math.Abs(qty) > qtyAvail)
                                    {
                                        valTran = qtyAvail;
                                        objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand - valTran, 0);
                                        objItemLot.QtyAvail = 0;
                                        objItemLot.Cost = 0;
                                        objItemLot.Update();

                                        qty = qty - qtyAvail;
                                    }
                                    else
                                    {
                                        valTran = qty;
                                        objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand - valTran, 0);
                                        objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail - valTran, 0);
                                        
                                        objItemLot.Cost = objItem.TotCost * Math.Abs(objItemLot.QtyOnHand);
                                        objItemLot.Update();
        
                                        qty = 0;
                                        isBreak = true;
                                    }
                                    clsIN_LotTrans newLotTrans = new clsIN_LotTrans(Dal)
                                    {
                                        BatNbr = batNbr,
                                        BranchID = branchID,
                                        RefNbr = newTran.RefNbr,
                                        LotSerNbr = itmLot.String("LotSerNbr"),
                                        INTranLineRef = newTran.LineRef,
                                        ExpDate = DateTime.Now,
                                        InvtID = newTran.InvtID,
                                        InvtMult = newTran.InvtMult,
                                        KitID = "", // ko bit
                                        MfgrLotSerNbr = "",
                                        Qty = -valTran,
                                        SiteID = newTran.SiteID,
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
                                    if (isBreak)
                                    {
                                        break; 
                                    }
                                }
                                if (qty > 0)
                                {
                                    throw new MessageException(MessageType.Message, "608", "", new[] { objItemLot.InvtID, objItemLot.SiteID});
                                }                                
                            }                            
                        }
                        catch (Exception ex)
                        {
                            throw ex;
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
        public bool Issue_Cancel(string branchID, string batNbr, string rcptNbr, bool release, bool showWhseLoc)
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

                        objItem.QtyAvail = Math.Round(objItem.QtyAvail + qty, 0);
                        objItem.QtyOnHand = Math.Round(objItem.QtyOnHand + qty, 0);
                        objItem.AvgCost = Math.Round(objItem.QtyOnHand != 0 ? (objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItem.QtyOnHand :
                            objItem.AvgCost, 0);

                    }
                    objItem.TotCost = Math.Round(objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = Prog;
                    objItem.LUpd_User = User;
                    objItem.Update();

                    if (showWhseLoc)
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

                            objItemLoc.QtyAvail = Math.Round(objItemLoc.QtyAvail + qty, 0);
                            objItemLoc.QtyOnHand = Math.Round(objItemLoc.QtyOnHand + qty, 0);
                            objItemLoc.AvgCost = Math.Round(objItemLoc.QtyOnHand != 0 ? (objItemLoc.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItemLoc.QtyOnHand :
                                objItemLoc.AvgCost, 0);

                        }
                        objItemLoc.TotCost = Math.Round(objItemLoc.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                            qty = -1 * Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc").PassNull(), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                            objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, GetDecimalPlace(objInvt.LotSerTrack));
                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + qty, GetDecimalPlace(objInvt.LotSerTrack));


                            if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objItemLot.Cost, GetDecimalPlace(objInvt.LotSerTrack)) < 0)
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
                        cost.Qty = Math.Round(cost.Qty + qty, 0);
                        cost.TotCost = Math.Round(cost.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                        
                        objSite.QtyOnHand = Math.Round(objSite.QtyOnHand + qty, GetDecimalPlace(objInvt.LotSerTrack));
                        objSite.QtyAvail = Math.Round(objSite.QtyAvail + qty, GetDecimalPlace(objInvt.LotSerTrack));
                        objSite.AvgCost = Math.Round(objSite.QtyOnHand != 0 ? (objSite.TotCost - tran.Double("ExtCost")) / objSite.QtyOnHand : objSite.AvgCost, GetDecimalPlace(objInvt.LotSerTrack));
                        
                        if (!setup.NegQty && objSite.QtyAvail < 0)
                        {
                            throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID });
                        }
                    }

                    if (!setup.NegQty && setup.CheckINVal && Math.Round(objSite.TotCost - tran.Double("ExtCost"), 0) < 0)
                    {
                        throw new MessageException(MessageType.Message,"607","", new[] { objInvt.InvtID, objSite.SiteID });
                    
                    }

                    objSite.TotCost = Math.Round(objSite.TotCost - tran.Double("ExtCost"), 0);
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

                            itemLoc.QtyOnHand = Math.Round(itemLoc.QtyOnHand + qty, GetDecimalPlace(objInvt.LotSerTrack));
                            itemLoc.QtyAvail = Math.Round(itemLoc.QtyAvail + qty, GetDecimalPlace(objInvt.LotSerTrack));
                            itemLoc.AvgCost = Math.Round(itemLoc.QtyOnHand != 0 ? (itemLoc.TotCost - tran.Double("ExtCost")) / itemLoc.QtyOnHand : itemLoc.AvgCost, GetDecimalPlace(objInvt.LotSerTrack));

                            if (!setup.NegQty && itemLoc.QtyAvail < 0)
                            {
                                throw new MessageException(MessageType.Message, "608", "", new[] { itemLoc.InvtID, itemLoc.SiteID });
                            }
                        }

                        if (!setup.NegQty && setup.CheckINVal && Math.Round(itemLoc.TotCost - tran.Double("ExtCost"), 0) < 0)
                        {
                            throw new MessageException(MessageType.Message, "607", "", new[] { objInvt.InvtID, itemLoc.SiteID });

                        }

                        itemLoc.TotCost = Math.Round(itemLoc.TotCost - tran.Double("ExtCost"), 0);
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
                                qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 2);
                            }
                            else
                            {
                                qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0);
                            }

                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail - qty, objInvt.LotSerTrack == "Q" ? 2: 0);
                            objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand - qty, objInvt.LotSerTrack == "Q" ? 2 : 0);
                           

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
                                    objTran.ExtCost = Math.Round(qtyCal * fl.Double("UnitCost"), 0);
                                    objTran.TranAmt = Math.Round(qtyUpdate * tran.Double("UnitPrice"), 0);
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
                                    cost.Qty = Math.Round(cost.Qty - qtyCal);
                                    cost.TotCost = Math.Round(cost.TotCost - qtyCal * cost.UnitCost, 0);
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

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                    }
                    if (!objSetup.NegQty && Math.Round(objSite.QtyOnHand + qty, 0) < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID });
                    }
                    
                    objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + qty, 0);
                    objSite.QtyOnHand = Math.Round(objSite.QtyOnHand + qty, 0);
                    objSite.AvgCost = Math.Round(objSite.QtyOnHand != 0
                                        ? (objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objSite.QtyOnHand
                                        : objSite.AvgCost, 0);
                    
                    if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607", "", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.TotCost = Math.Round(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                            objBranchSite.QtyOnHand = Math.Round(objBranchSite.QtyOnHand + tranQty, 0);
                            objBranchSite.QtyAvail = Math.Round(objBranchSite.QtyAvail + tranQty, 0);
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
                            qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.SiteID = lotRow.String("SiteID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                           
                            objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + qty, 0);
                            objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, 0);                           

                            objItemLot.Cost = objSite.TotCost * objItemLot.QtyOnHand;
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();

                            if (objBranchLot != null)
                            {
                                double transLotQty = Math.Abs(qty);
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                                {
                                    Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.WhseLoc, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = Math.Round(objBranchLot.QtyOnHand + transLotQty, 0);
                                    objBranchLot.QtyAvail = Math.Round(objBranchLot.QtyAvail + transLotQty, 0);
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
                            cost.TotCost = Math.Round(cost.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"));
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

                        objItem.QtyAvail = Math.Round(objItem.QtyAvail + qty, 0);
                        objItem.QtyOnHand = Math.Round(objItem.QtyOnHand + qty, 0);
                        objItem.AvgCost = Math.Round(objItem.QtyOnHand != 0 ? (objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItem.QtyOnHand :
                            objItem.AvgCost, 0);

                    }
                    objItem.TotCost = Math.Round(objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                            objBranchSite.QtyOnHand = Math.Round(objBranchSite.QtyOnHand - tranQty, 0);
                            objBranchSite.QtyAvail = Math.Round(objBranchSite.QtyAvail  - tranQty, 0);
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
                            qty = -1 * Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                            objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, 0);
                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + qty, 0);


                            if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objItemLot.Cost, 0) < 0)
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
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")) || objBranchLot.QtyAvail == 0 || objBranchLot.QtyOnHand == 0)
                                {
                                    throw new MessageException(MessageType.Message, "201508181", "", new[] { "", tran.String("InvtID") + " " + Util.GetLang("Site") + " " + toSiteID, lotRow.String("LotSerNbr"), transLotQty.ToString() }); 
                                    //throw new MessageException(MessageType.Message, "606"); //Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = Math.Round(objBranchLot.QtyOnHand - transLotQty, 0);
                                    objBranchLot.QtyAvail = Math.Round(objBranchLot.QtyAvail - transLotQty, 0);
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
                        cost.Qty = Math.Round(cost.Qty + qty, 0);
                        cost.TotCost = Math.Round(cost.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                                    objTran.ExtCost = Math.Round(qtyCal * fl.Double("UnitCost"), 0);
                                    objTran.TranAmt = Math.Round(qtyUpdate * tran.Double("UnitPrice"), 0);
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
                                    cost.Qty = Math.Round(cost.Qty - qtyCal);
                                    cost.TotCost = Math.Round(cost.TotCost - qtyCal * cost.UnitCost, 0);
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

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || string.IsNullOrEmpty(tran.String("UnitMultDiv")))
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");
                    }
                    if (!objSetup.NegQty && Math.Round(objSite.QtyOnHand + qty, 0) < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + qty, 0);
                    objSite.QtyOnHand = Math.Round(objSite.QtyOnHand + qty, 0);
                    objSite.AvgCost = Math.Round(objSite.QtyOnHand != 0
                                        ? (objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult")) / objSite.QtyOnHand
                                        : objSite.AvgCost, 0);

                    if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0) < 0)
                    {
                        throw new MessageException(MessageType.Message, "607", "", new[] { objSite.InvtID, objSite.SiteID });
                    }

                    objSite.TotCost = Math.Round(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                            objBranchSite.QtyOnHand = Math.Round(objBranchSite.QtyOnHand + tranQty, 0);
                            objBranchSite.QtyAvail = Math.Round(objBranchSite.QtyAvail + tranQty, 0);
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
                            qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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

                            objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + qty, 0);
                            objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, 0);

                            objItemLot.Cost = objSite.TotCost * objItemLot.QtyOnHand;
                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = Prog;
                            objItemLot.LUpd_User = User;
                            objItemLot.Update();

                            if (objBranchLot != null)
                            {
                                double transLotQty = Math.Abs(qty);
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                                {
                                    Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.WhseLoc, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = Math.Round(objBranchLot.QtyOnHand + transLotQty, 0);
                                    objBranchLot.QtyAvail = Math.Round(objBranchLot.QtyAvail + transLotQty, 0);
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
                            cost.TotCost = Math.Round(cost.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"));
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

                        objItem.QtyAvail = Math.Round(objItem.QtyAvail + qty, 0);
                        objItem.QtyOnHand = Math.Round(objItem.QtyOnHand + qty, 0);
                        objItem.AvgCost = Math.Round(objItem.QtyOnHand != 0 ? (objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult")) / objItem.QtyOnHand :
                            objItem.AvgCost, 0);

                    }
                    objItem.TotCost = Math.Round(objItem.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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
                            objBranchSite.QtyOnHand = Math.Round(objBranchSite.QtyOnHand - tranQty, 0);
                            objBranchSite.QtyAvail = Math.Round(objBranchSite.QtyAvail - tranQty, 0);
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
                            qty = -1 * Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 0) * lotRow.Short("InvtMult");
                            if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                            {
                                objItemLot.Reset();
                                objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                                objItemLot.InvtID = lotRow.String("InvtID");
                                objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                            objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, 0);
                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + qty, 0);


                            if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objItemLot.Cost, 0) < 0)
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
                                if (!objBranchLot.GetByKey(toSiteID, tran.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")) || objBranchLot.QtyAvail == 0 || objBranchLot.QtyOnHand == 0)
                                {
                                    throw new MessageException(MessageType.Message, "201508181", "", new[] { "", tran.String("InvtID") + " " + Util.GetLang("Site") + " " + toSiteID, lotRow.String("LotSerNbr"), transLotQty.ToString() });
                                    //throw new MessageException(MessageType.Message, "606"); //Insert_IN_ItemLot(ref objBranchLot, tran.String("InvtID"), toSiteID, objItemLot.LotSerNbr, objItemLot.ExpDate, objItemLot.MfgrLotSerNbr, 0.0, transLotQty);
                                }
                                else
                                {
                                    objBranchLot.QtyOnHand = Math.Round(objBranchLot.QtyOnHand - transLotQty, 0);
                                    objBranchLot.QtyAvail = Math.Round(objBranchLot.QtyAvail - transLotQty, 0);
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
                        cost.Qty = Math.Round(cost.Qty + qty, 0);
                        cost.TotCost = Math.Round(cost.TotCost - tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
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

                    itemSite.QtyOnHand = Math.Round(itemSite.QtyOnHand + qty, GetDecimalPlace(inventory.LotSerTrack));
                    itemSite.QtyAvail = Math.Round(itemSite.QtyAvail + qty, GetDecimalPlace(inventory.LotSerTrack));
                    itemSite.AvgCost = Math.Round(itemSite.QtyOnHand > 0 ? (itemSite.TotCost + inTran.Double("ExtCost")) / itemSite.QtyOnHand : itemSite.AvgCost, GetDecimalPlace(inventory.LotSerTrack));
                }
                itemSite.TotCost = Math.Round(itemSite.TotCost + inTran.Double("ExtCost"), 0);
                itemSite.LUpd_DateTime = DateTime.Now;
                itemSite.LUpd_Prog = inTran.String("LUpd_Prog");
                itemSite.LUpd_User = inTran.String("LUpd_User");

                if (inventory.StkItem == 1 && inventory.LotSerTrack != "N" && inventory.PassNull() != string.Empty)
                {
                    DataTable dtLot = objLot.GetAll(branchID, batNbr, "%", "%", inTran.String("LineRef"));
                    foreach (DataRow lotRow in dtLot.Rows)
                    {
                        qty = Math.Round(lotRow.Double("Qty") * (lotRow.String("UnitMultDiv") == "D" ? 1.0 / lotRow.Double("CnvFact") : lotRow.Double("CnvFact")), 10);
                        if (!objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("WhseLoc"), lotRow.String("LotSerNbr")))
                        {
                            objItemLot.Reset();
                            objItemLot.LotSerNbr = lotRow.String("LotSerNbr");
                            objItemLot.InvtID = lotRow.String("InvtID");
                            objItemLot.WhseLoc = lotRow.String("WhseLoc");
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
                        objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + qty, GetDecimalPlace(inventory.LotSerTrack));
                        objItemLot.QtyOnHand = Math.Round(objItemLot.QtyOnHand + qty, GetDecimalPlace(inventory.LotSerTrack));
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

        private int GetDecimalPlace(string lotSerTrack)
        {
            return (lotSerTrack != "Q" ? 0 : 2);
        }
    }
}


#region IN10500 Old
//public bool IN10500_Release(string tagID, string branchID, string siteID)
//        {
//            try
//            {
//                string lineRef = string.Empty;

//                clsSQL objSql = new clsSQL(Dal);
//                var batNbr = objSql.INNumbering(branchID, "BatNbr");
//                var refNbr = objSql.INNumbering(branchID, "RefNbr");

//                clsIN_Setup objSetup = new clsIN_Setup(Dal);
//                objSetup.GetByKey(branchID, "IN");

//                clsIN_TagDetail objTagDetail = new clsIN_TagDetail(Dal);
//                DataTable lstTagDetail = objTagDetail.GetAll(tagID, branchID, siteID, "%"); //objTagDetail.GetAll(tagID, siteID, "%"); 


//                clsIN_TagHeader objTagHeader = new clsIN_TagHeader(Dal);
//                objTagHeader.GetByKey(tagID, branchID, siteID);
//                //objTagHeader.GetByKey(tagID);
//                objTagHeader.INBatNbr = batNbr;
//                objTagHeader.Status = "C";
//                objTagHeader.Update();
//                clsBatch newBatch = new clsBatch(Dal)
//                {

//                    BatNbr = batNbr,
//                    Descr = objTagHeader.Descr,
//                    Module1 = "IN",
//                    RefNbr = refNbr,
//                    ReasonCD = objTagHeader.ReasonCD,
//                    Status = "C",
//                    Rlsed = 1,
//                    DateEnt = DateTime.Now.Short(),
//                    JrnlType = "IN",
//                    EditScrnNbr = "IN10500",
//                    BranchID = branchID,
//                    LUpd_DateTime = objTagHeader.LUpd_DateTime,
//                    LUpd_Prog = objTagHeader.LUpd_Prog,
//                    LUpd_User = objTagHeader.LUpd_User,
//                    Crtd_DateTime = objTagHeader.Crtd_DateTime,
//                    Crtd_Prog = objTagHeader.Crtd_Prog,
//                    Crtd_User = objTagHeader.Crtd_User,
//                };
//                newBatch.Add();

//                lineRef = "0";
//                lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
//                for (int i = lineRef.Length; lineRef.Length < 5; )
//                    lineRef = "0" + lineRef;

//                clsIN_Trans objTran = new clsIN_Trans(Dal);
//                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
//                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
//                clsIN_UnitConversion objCnv = new clsIN_UnitConversion(Dal);

//                foreach (DataRow tagDetail in lstTagDetail.Rows)
//                {
//                    objInvt.GetByKey(tagDetail.String("InvtID").ToString());
//                    objCnv.GetByKey("3", "*", tagDetail.String("InvtID"), "THUNG", objInvt.StkUnit);

//                    clsIN_Trans newTran = new clsIN_Trans(Dal)
//                    {
//                        BatNbr = batNbr,
//                        BranchID = branchID,
//                        TranDate = DateTime.Now.Short(),
//                        Crtd_DateTime = DateTime.Now,
//                        Crtd_Prog = tagDetail.String("Crtd_Prog"),
//                        Crtd_User = tagDetail.String("Crtd_User"),
//                        InvtID = tagDetail.String("InvtID"),
//                        TranType = "AJ",
//                        JrnlType = "IN",
//                        LUpd_DateTime = DateTime.Now,
//                        LUpd_Prog = tagDetail.String("LUpd_Prog"),
//                        LUpd_User = tagDetail.String("LUpd_User"),
//                        Qty = tagDetail.Double("OffsetEAQty"),
//                        ReasonCD = tagDetail.String("ReasonCD"),
//                        RefNbr = refNbr,
//                        Rlsed = 1,
//                        SiteID = tagDetail.String("SiteID"),
//                        LineRef = lineRef
//                    };
//                    newTran.UnitDesc = objInvt.StkUnit;
//                    newTran.Qty += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;

//                    lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
//                    for (int i = lineRef.Length; lineRef.Length < 5; )
//                        lineRef = "0" + lineRef;

//                    newTran.Add();

//                    if (objItem.GetByKey(tagDetail.String("InvtID"), tagDetail.String("SiteID")))
//                    {
//                        objItem.QtyOnHand = objItem.QtyOnHand + tagDetail.Double("OffsetEAQty");
//                        objItem.QtyAvail = objItem.QtyAvail + tagDetail.Double("OffsetEAQty");
//                        objItem.QtyOnHand += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;
//                        objItem.QtyAvail += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;

//                        objItem.LUpd_DateTime = System.DateTime.Now;
//                        objItem.LUpd_Prog = tagDetail.String("LUpd_Prog");
//                        objItem.LUpd_User = tagDetail.String("LUpd_User");
//                        objItem.Update();
//                    }
//                }
//                return true;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }

//        }
#endregion
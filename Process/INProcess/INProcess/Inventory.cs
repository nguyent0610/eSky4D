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
using log4net;
using HQ.eSkyFramework;
namespace INProcess
{
    public class Inventory 
    {
        private static readonly ILog mLogger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Prog { get; set; }
        public string User { get; set; }
        public DataAccess Dal { get; set; }
        public List<ProcessException> LogList { get; set; }

        public Inventory(string User, string prog,DataAccess dal)
        {
            this.User = User;
            this.Prog = prog;
            this.Dal = dal;
        }
        public bool IN10100_Release(string branchID, string batNbr, bool isTransfer, string transferNbr)
        {
            
            clsSQL sql=new clsSQL(Dal);
            clsIN_Setup setup=new clsIN_Setup(Dal);
            clsIN_Trans tran=new clsIN_Trans(Dal);
            clsIN_Inventory inventory=new clsIN_Inventory(Dal);
            clsIN_ItemSite itemSite=new clsIN_ItemSite(Dal);
            setup.GetByKey(branchID,"IN");
            DataTable trans=tran.GetAll(branchID,batNbr,"%","%");
            double qty = 0;
            string User = string.Empty;
            string prog = string.Empty;
            string refNbr=string.Empty;
            DateTime? tranDate=DateTime.Now;
            if(trans.Rows.Count>0)
            {
                User=trans.Rows[0].String("LUpd_User");
                refNbr=trans.Rows[0].String("LUpd_User");
                prog=trans.Rows[0].String("LUpd_Prog");
                tranDate=trans.Rows[0].Date("TranDate");
            }
         
            foreach (DataRow inTran in trans.Rows)
            {
                    
                inventory.GetByKey(inTran.String("InvtID"));
                if (!itemSite.GetByKey(inTran.String("InvtID"),inTran.String("SiteID")))
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
                    itemSite.LastPurchaseDate=DateTime.Now.Short();
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

                    itemSite.QtyOnHand = Math.Round(itemSite.QtyOnHand + qty, 0);
                    itemSite.QtyAvail = Math.Round(itemSite.QtyAvail + qty, 0);
                    itemSite.AvgCost = Math.Round(itemSite.QtyOnHand > 0 ? (itemSite.TotCost + inTran.Double("ExtCost")) / itemSite.QtyOnHand :  itemSite.AvgCost, 0);
                }
                itemSite.TotCost = Math.Round(itemSite.TotCost + inTran.Double("ExtCost"), 0);
                itemSite.LUpd_DateTime = DateTime.Now;
                itemSite.LUpd_Prog = inTran.String("LUpd_Prog");
                itemSite.LUpd_User = inTran.String("LUpd_User");

                if (inventory.StkItem == 1 && inventory.LotSerTrack != "N"){}

                itemSite.Update();
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

                string User = string.Empty;
                string prog = string.Empty;
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    User = lstTrans.Rows[0].String("LUpd_User");
                    prog = lstTrans.Rows[0].String("LUpd_Prog");
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                try
                {
                    lineRef  = (Convert.ToInt32(lstTrans.Compute("Max(LineRef)", "").ToString())+1).ToString();
                    for(int l=lineRef.Length;l<5;l++)
                            lineRef+="0"+lineRef;
                }
                catch (Exception)
                {
                    lineRef = "00001";
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objSite = new clsIN_ItemSite(Dal);
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
                            throw new ProcessException("738");
                        }

                    }

                }
                qty = 0;
                foreach (DataRow tran in lstTrans.Rows)
                {
                    objInvt.GetByKey(tran.String("InvtID"));
                    if (!objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new ProcessException("606");
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
                        throw new ProcessException("608", new[] { objSite.InvtID, objSite.SiteID });
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
                        throw new ProcessException("607", new[] { objSite.InvtID, objSite.SiteID  });
                    }

                    objSite.TotCost = Math.Round(objSite.TotCost + tran.Double("ExtCost") * tran.Short("InvtMult"), 0);
                    objSite.LUpd_DateTime = DateTime.Now;
                    objSite.LUpd_Prog = Prog;
                    objSite.LUpd_User = User;
                    objSite.Update();

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
                            throw new ProcessException("606");
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
                            throw new ProcessException("607", new[] { objItem.InvtID, objItem.SiteID });
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

                        if(objInvt.StkItem==1 && transfer.String("TransferType")=="2")
                            objToItem.QtyInTransit = Math.Round(objToItem.QtyInTransit + Math.Abs(qty), 0);

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
                        throw new ProcessException("606");
                    }
                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv").PassNull() == string.Empty)
                            qty = tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = (tran.Double("Qty") * tran.Short("InvtMult")) / tran.Double("CnvFact");

                        objItem.QtyAllocIN = Math.Round(objItem.QtyAllocIN + qty, 0);
                        objItem.QtyOnHand = Math.Round(objItem.QtyOnHand + qty, 0);

                        if(qty>0) objItem.QtyAvail = Math.Round(objItem.QtyAvail + qty, 0);
                    }
                    if (!objSetup.NegQty && objSetup.CheckINVal && Math.Round(objItem.TotCost + tran.Double("ExtCost"), 0) < 0)
                    {
                        throw new ProcessException("607", new[] { objItem.InvtID, objItem.SiteID });
                    }
                    objItem.TotCost = Math.Round(objItem.TotCost + tran.Double("ExtCost"), 0);
                    objItem.LUpd_DateTime = DateTime.Now;
                    objItem.LUpd_Prog = prog;
                    objItem.LUpd_User = User;
                    objItem.Update();
                }

                sql.IN_ReleaseBatch(branchID, batNbr, Prog, User);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IN10500_Release(string tagID, string batNbr, string refNbr, string branchID)
        {
            try
            {
                string lineRef = string.Empty;

                clsSQL objSql = new clsSQL(Dal);
                batNbr = objSql.INNumbering(branchID, "BatNbr");
                refNbr = objSql.INNumbering(branchID, "RefNbr");

                clsIN_Setup objSetup = new clsIN_Setup(Dal);
                objSetup.GetByKey(branchID, "IN");

                clsIN_TagDetail objTagDetail = new clsIN_TagDetail(Dal);
                DataTable lstTagDetail = objTagDetail.GetAll(tagID, "%", "%");

                clsIN_TagHeader objTagHeader = new clsIN_TagHeader(Dal);
                objTagHeader.GetByKey(tagID);
                objTagHeader.INBatNbr = batNbr;
                objTagHeader.Status = "C";
          
                clsBatch newBatch = new clsBatch(Dal)
                {

                    BatNbr = batNbr,
                    Descr = objTagHeader.Descr,
                    Module1 = "IN",
                    RefNbr = refNbr,
                    ReasonCD = objTagHeader.ReasonCD,
                    Status = "C",
                    Rlsed = 1,
                    DateEnt = DateTime.Now.Short(),
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
                newBatch.Add();
   
                lineRef = "0";
                lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                for (int i = lineRef.Length; lineRef.Length < 5; )
                    lineRef = "0" + lineRef;

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_UnitConversion objCnv = new clsIN_UnitConversion(Dal);
            
                foreach (DataRow tagDetail in lstTagDetail.Rows)
                {
                    objInvt.GetByKey(tagDetail.String("InvtID").ToString());
                    objCnv.GetByKey("3", "*", tagDetail.String("InvtID"), "THUNG", objInvt.StkUnit);

                    clsIN_Trans newTran = new clsIN_Trans()
                    {
                        BatNbr = batNbr,
                        BranchID = branchID,
                        TranDate = DateTime.Now.Short(),
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
                    newTran.UnitDesc = objInvt.StkUnit;
                    newTran.Qty += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;

                    lineRef = (Convert.ToInt32(lineRef) + 1).ToString();
                    for (int i = lineRef.Length; lineRef.Length < 5; )
                        lineRef = "0" + lineRef;

                    newTran.Add();
               
                    if (objItem.GetByKey(tagDetail.String("InvtID"), tagDetail.String("SiteID")))
                    {
                        objItem.QtyOnHand = objItem.QtyOnHand + tagDetail.Double("OffsetEAQty");
                        objItem.QtyAvail = objItem.QtyAvail + tagDetail.Double("OffsetEAQty");
                        objItem.QtyOnHand += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;
                        objItem.QtyAvail += tagDetail.Double("OffetCaseQty") * objCnv.CnvFact;

                        objItem.LUpd_DateTime = System.DateTime.Now;
                        objItem.LUpd_Prog = tagDetail.String("LUpd_Prog");
                        objItem.LUpd_User = tagDetail.String("LUpd_User");
                        objItem.Update();
                    }
                }
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
        public bool IN40300_Release(List<string> lstLang, string parm01, string parm02, string parm03, string parm04, string parm05, string parm06)
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
                        objSql.IN_Integrity_RebuildQtyonSO(Prog, User, parm05, parm06, 0, 0, 0) +
                        "\n";
                }
                if (parm04 == "1")
                {
                    mMessage +=
                        objSql.IN_Integrity_RebuildQtyCost(Prog, User, parm05, parm06, 0, 0, 0) +
                        "\n";
                }
            }
            else
            {
                objSql.IN_Integrity_ReBuildBatch(Prog, User, parm02, parm03, parm04);
            }

            Utility.AppendLog(LogList,new ProcessException("8009",new string[]{mMessage}));
            return true;
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

                string User = string.Empty;
                string prog = string.Empty;
                DateTime? tranDate = DateTime.Now;
                if (lstTrans.Rows.Count > 0)
                {
                    User = lstTrans.Rows[0].String("LUpd_User");
                    prog = lstTrans.Rows[0].String("LUpd_Prog");
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItem = new clsIN_ItemSite(Dal);
                clsIN_ItemCost cost = new clsIN_ItemCost(Dal);
                clsSQL sql = new clsSQL(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    if (!release && (tran.Short("Rlsed") == -1 || tran.Short("InvtMult") == 1)) continue;

                    objInvt.GetByKey(tran.String("InvtID"));

                    if (!objItem.GetByKey(tran.String("InvtID"),tran.String("SiteID")))
                    {
                        Utility.AppendLog(LogList, new ProcessException("606"));
                        return false;
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
                    objItem.LUpd_Prog = prog;
                    objItem.LUpd_User = User;
                    objItem.Update();

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

                clsIN_Trans objTran = new clsIN_Trans(Dal);
                DataTable lstTrans = new DataTable();
                if (string.IsNullOrEmpty(rcptNbr))
                    lstTrans =objTran.GetAll(branchID,batNbr,"%","%");
                else
                    lstTrans = objTran.GetAll(branchID, batNbr, rcptNbr, "%");

                double qty = 0;
                string User = string.Empty;
                string prog = string.Empty;
                DateTime? tranDate = DateTime.Now;

                if (lstTrans.Rows.Count > 0)
                {
                    User = lstTrans.Rows[0].String("LUpd_User");
                    prog = lstTrans.Rows[0].String("LUpd_Prog");
                    tranDate = lstTrans.Rows[0].Date("TranDate");
                }

                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsIN_ItemSite objSite = new clsIN_ItemSite(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    if (!release && tran.Short("Rlsed") == -1 || tran.Short("InvtMult") == -1) continue;
                   
                    objInvt.GetByKey(tran.String("InvtID"));

                    if (!objSite.GetByKey(tran.String("InvtID"),tran.String("SiteID")))
                    {
                        objSite.Reset();

                        objSite.SiteID = tran.String("SiteID");
                        objSite.InvtID = tran.String("InvtID");
                        objSite.AvgCost = 0;
                        objSite.Crtd_DateTime = DateTime.Now;
                        objSite.Crtd_Prog = tran.String("Crtd_Prog");
                        objSite.Crtd_User = tran.String("Crtd_User");
                        objSite.LUpd_DateTime = DateTime.Now;
                        objSite.LUpd_Prog = tran.String("LUpd_Prog");
                        objSite.LUpd_User = tran.String("LUpd_User");
                        objSite.LastPurchaseDate=DateTime.Now.Short();
                        objSite.QtyAlloc = 0;
                        objSite.QtyAllocIN = 0;
                        objSite.QtyAllocPORet = 0;
                        objSite.QtyAllocSO = 0;
                        objSite.QtyAvail = 0;
                        objSite.QtyInTransit = 0;
                        objSite.QtyOnBO = 0;
                        objSite.QtyOnHand = 0;
                        objSite.QtyOnPO = 0;
                        objSite.QtyOnSO = 0;
                        objSite.QtyOnTransferOrders = 0;
                        objSite.QtyShipNotInv = 0;
                        objSite.QtyUncosted = 0;
                        objSite.StkItem = objInvt.StkItem;
                        objSite.TotCost = 0;
                    
                        objSite.Add();
                    }

                    if (objInvt.StkItem == 1)
                    {
                        if (tran.String("UnitMultDiv") == "M" || tran.String("UnitMultDiv") == string.Empty)
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") * tran.Double("CnvFact");
                        else
                            qty = -1 * tran.Double("Qty") * tran.Short("InvtMult") / tran.Double("CnvFact");

                        if (isTransfer && tran.String("TranType") == "TR") objSite.QtyInTransit -= qty;

                        objSite.QtyOnHand = Math.Round(objSite.QtyOnHand + qty, 0);
                        objSite.QtyAvail = Math.Round(objSite.QtyAvail + qty, 0);
                        objSite.AvgCost = Math.Round(objSite.QtyOnHand != 0 ? (objSite.TotCost - tran.Double("ExtCost")) / objSite.QtyOnHand : objSite.AvgCost, 0);
                    }

                    if (!setup.NegQty && setup.CheckINVal && Math.Round(objSite.TotCost - tran.Double("ExtCost"), 0) < 0)
                    {
                        Utility.AppendLog(LogList, new ProcessException("607", new[] { objInvt.InvtID, objSite.SiteID }));
                        return false;
                    }

                    objSite.TotCost = Math.Round(objSite.TotCost - tran.Double("ExtCost"), 0);
                    objSite.LUpd_DateTime = DateTime.Now;
                    objSite.LUpd_Prog = tran.String("LUpd_Prog");
                    objSite.LUpd_User = tran.String("LUpd_User");
                    objSite.Update();

                    if (objInvt.StkItem == 1 && objInvt.LotSerTrack != "N"){}

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
    }
}
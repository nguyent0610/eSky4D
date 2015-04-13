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
namespace POProcess
{
    public class PO
    {
        private static readonly ILog mLogger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Prog { get; set; }
        public string User { get; set; }
        public DataAccess Dal { get; set; }
        public List<MessageException> LogList { get; set; }
        clsBatch m_objBatchIN;
        clsPO_Invoice m_objPO_Invoice;
        clsSI_Terms m_objSI_Terms;
        clsAP_Vendor m_objAP_Vendor;
        public PO(string User, string Prog, DataAccess dal)
        {
            this.User = User;
            this.Prog = Prog;
            this.Dal = dal;
        }


       
        #region PO10200 Release
        public bool PO10200_Release(string BranchID, string BatNbr,string RcptNbr)
        {
         
            clsPO_Receipt m_objPO_Receipt = new clsPO_Receipt(Dal);          
            clsPO_Trans m_objPO_Trans = new clsPO_Trans(Dal);
            clsPO_Detail m_objPO_Detail = new clsPO_Detail(Dal);        
            clsIN_Inventory m_objIN_Inventory=new clsIN_Inventory(Dal);
            clsIN_ItemSite m_objIN_ItemSite=new clsIN_ItemSite(Dal);
            clsSQL objSql = new clsSQL(Dal);
            DataTable dtPO_Trans=new DataTable() ;           
            string strAPBatNbr = "";
            string strAPRefNbr = "";
            string strPOBatNbr = "";
            string strRefNbr = "";
            bool blnPromorion = false;
            int intLineRefAP_Trans = 0;
            int intLineRefGL = 0;

            try
            {
                clsAP_Setup m_objAP_Setup = new clsAP_Setup(Dal);
                m_objAP_Setup.GetByKey(BranchID, "AP");
                clsPO_Setup m_objPO_Setup = new clsPO_Setup(Dal);
                m_objPO_Setup.GetByKey(BranchID, "PO");

                if(m_objPO_Receipt.GetByKey(BranchID, BatNbr, RcptNbr))
                {                                
                    m_objBatchIN = new clsBatch(Dal);
                    m_objPO_Invoice = new clsPO_Invoice(Dal);
                    //Insert Batch AP
                    m_objBatchIN.GetByKey(BranchID,"IN",BatNbr);                                        
                    m_objPO_Invoice.GetByKey(BranchID,BatNbr,RcptNbr);

                    if (m_objPO_Setup.AutoReleaseAP == 1)//kiem tra co day qua AP hay ko
                    {
                        strAPBatNbr = objSql.APNumbering(BranchID, "BatNbr");// = app.APNumbering(BranchID, "BatNbr").FirstOrDefault().Nbr;

                        InsertAPBatch(strAPBatNbr, m_objPO_Receipt);
                    }
                    dtPO_Trans=new DataTable();
                    dtPO_Trans = m_objPO_Trans.GetAll(BranchID, BatNbr,RcptNbr, "%");
                    IList<clsPO_Trans> ListclsPO_Trans = DataTableHelper.ConvertTo<clsPO_Trans>(dtPO_Trans);
                    if (ListclsPO_Trans.Where(p => p.PurchaseType == "PR").Count() > 0)
                    //foreach (PO_Trans.GetAll(BranchID,BatNbr,row.RcptNbr"),"%"))//app.PO_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RcptNbr == row.RcptNbr && p.PurchaseType == "PR").FirstOrDefault() != null)
                    {
                        blnPromorion = true;
                       // break;
                    }
                    else
                    {
                        blnPromorion = false;
                    }
                    if (m_objPO_Setup.AutoReleaseAP == 1)//kiem tra co day qua AP hay ko
                    {
                        //Get APLastRefNbr
                        strAPRefNbr = objSql.APNumbering(BranchID, "RefNbr");
                        //Get APAcct, APSub
                        m_objSI_Terms = new clsSI_Terms(Dal);
                        m_objAP_Vendor = new clsAP_Vendor(Dal);
                        m_objSI_Terms.GetByKey(m_objPO_Invoice.Terms);
                        m_objAP_Vendor.GetByKey(m_objPO_Receipt.VendID);
                        //Insert AP_Doc
                        InsertAP_Doc(strAPBatNbr, strAPRefNbr, blnPromorion, m_objPO_Receipt);
                    }
                    //Insert transaction                  
                    foreach (clsPO_Trans row1 in ListclsPO_Trans)
                    {

                        if (m_objPO_Receipt.RcptFrom == "PO")
                        {
                            m_objPO_Detail.GetByKey(BranchID, row1.PONbr, row1.POLineRef);
                        }
                        if (m_objPO_Setup.AutoReleaseAP == 1)//kiem tra co day qua AP hay ko
                        {
                            //Insert AP_Trans
                            intLineRefAP_Trans = intLineRefAP_Trans + 1;

                            InsertAP_Trans(strAPBatNbr, strAPRefNbr, intLineRefAP_Trans, m_objPO_Receipt, row1);
                            //
                            //Insert Fee AP_Trans
                            //
                            if (row1.RcptFee > 0)
                            {
                                intLineRefAP_Trans = intLineRefAP_Trans + 1;
                                InsertFeeAP_Trans(strAPBatNbr, strAPRefNbr, intLineRefAP_Trans, m_objPO_Receipt, row1);
                            }
                        }
                        //
                        //Insert IN_Trans
                        //
                        if (row1.PurchaseType == "GN" || row1.PurchaseType == "GI" || row1.PurchaseType == "GS" || row1.PurchaseType == "PR")
                        {
                           
                            m_objIN_Inventory.GetByKey(row1.InvtID);
                            m_objIN_ItemSite.GetByKey(row1.InvtID,row1.SiteID);

                            InsertIN_Trans(BatNbr, m_objPO_Receipt, row1);

                        }
                    }
                    if (m_objPO_Setup.AutoReleaseAP == 1)//kiem tra co day qua AP hay ko
                    {
                       
                        double TranAmt = 0;
                        if (ListclsPO_Trans.Sum(p => p.TranAmt) > 0)
                        {
                            TranAmt = ListclsPO_Trans.Sum(p => p.TranAmt);// dtPO_Trans.Sum(p => p.TranAmt);

                        }
                        intLineRefAP_Trans = intLineRefAP_Trans + 1;

                        InsertAP_TransAPAcct(strAPBatNbr, strAPRefNbr, intLineRefAP_Trans, TranAmt, m_objPO_Receipt);
                        //Update PO_Invoice
                        m_objPO_Invoice = new clsPO_Invoice(Dal);
                        if (m_objPO_Invoice.GetByKey(BranchID, BatNbr, m_objPO_Receipt.RcptNbr))
                        {

                            m_objPO_Invoice.APBatNbr = strAPBatNbr;
                            m_objPO_Invoice.APRefNbr = strAPRefNbr;
                            m_objPO_Invoice.Update();
                        }
                    }
                    //Release IN
                    if (m_objPO_Receipt.RcptType == "R")
                    {

                        if (!ReceiptIN(m_objPO_Receipt)) return false;
                    }
                    else if (m_objPO_Receipt.RcptType == "X")
                    {

                        if (!IssueIN(m_objPO_Receipt)) return false;
                    }

                    //Update PO_Receipt
                    clsPO_Receipt objPO_Receipt = new clsPO_Receipt(Dal);
                    if (m_objPO_Receipt.GetByKey(BranchID, BatNbr, m_objPO_Receipt.RcptNbr))
                    {
                        //objPO_Receipt = app.PO_Receipt.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RcptNbr == row.RcptNbr).FirstOrDefault();
                        m_objPO_Receipt.Rlsed = 1;
                        m_objPO_Receipt.Status = "C";
                        m_objPO_Receipt.Update();
                    }

                    //Update status for batch
                    clsBatch objBatch = new clsBatch(Dal);
                    if (objBatch.GetByKey(BranchID, "IN", BatNbr))
                    {
                        //objBatch = app.Batches.Where(p => p.BranchID == BranchID && p.Module == "IN" && p.BatNbr == BatNbr).FirstOrDefault();
                        objBatch.Status = "C";
                        objBatch.Rlsed = 1;
                        objBatch.Update();
                    }
                    if (m_objPO_Setup.AutoReleaseAP == 1)//kiem tra co day qua AP hay ko
                    {
                        clsBatch objBatch1 = new clsBatch(Dal);
                        clsAP_Trans objAP_Trans = new clsAP_Trans(Dal);
                        if (objBatch1.GetByKey(BranchID, "AP", strAPBatNbr))
                        {

                            DataTable dtAP_Trans = objAP_Trans.GetAll(BranchID, strAPBatNbr, "%", "%");
                            objBatch1.TotAmt = DataTableHelper.ConvertTo<clsAP_Trans>(dtAP_Trans).Where(p=>p.TranClass!="N").Sum(p => p.TranAmt);//                 
                            objBatch1.Update();
                        }


                        if (!AP10100_Release(BranchID, strAPBatNbr))
                        {
                            return false;
                        }
                    }

                    //dt1 = app.PO_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr&& p.PONbr!="" ).ToList();

                    clsIN_Setup m_objIN_Setup = new clsIN_Setup(Dal);
                    m_objIN_Setup.GetByKey(BranchID, "IN");// = app.IN_Setup.Where(p => p.SetupID == "IN" && p.BranchID==BranchID).FirstOrDefault();


                    foreach (clsPO_Trans row in ListclsPO_Trans)
                    {
                        if (row.PONbr != "") UpdatePODetail(row);
                    }

                    if(m_objPO_Receipt.PONbr.PassNull()!="")
                        UpdatePOHeader(m_objPO_Receipt.BranchID, m_objPO_Receipt.PONbr);
                    
                    //clsSQL objSql = new clsSQL(Dal);
                    objSql.IN_ReleaseBatch(BranchID, BatNbr, Prog, User);
                    if (m_objPO_Setup.AutoReleaseAP == 1)//kiem tra co day qua AP hay ko
                    {
                        objSql.AP_ReleaseBatch(BranchID, strAPBatNbr, Prog, User);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw ex;        
            }
        }
        private void InsertAP_Trans( string APBatNbr, string APRefNbr, int lineRef, clsPO_Receipt rowReceipt, clsPO_Trans rowPO_Trans)
        {
            clsAP_Trans m_objAP_Trans = new clsAP_Trans(Dal);
            m_objAP_Trans.Reset();
            m_objAP_Trans.BranchID = rowReceipt.BranchID;
            m_objAP_Trans.BatNbr = APBatNbr;
            m_objAP_Trans.RefNbr = APRefNbr;
            m_objAP_Trans.LineRef = lineRef.ToString("00000");
            m_objAP_Trans.InvtID = rowPO_Trans.InvtID;
            m_objAP_Trans.JrnlType = rowPO_Trans.JrnlType;
            //LineType
            switch (rowPO_Trans.PurchaseType)
            {
                case "FA":
                    m_objAP_Trans.LineType = "F";
                    break;
                case "GI":
                case "PR":
                    m_objAP_Trans.LineType = "R";
                    break;
                case "MI":
                    m_objAP_Trans.LineType = "C";
                    break;
                //case "FA":
                //    m_objAP_Trans.LineType = "X";
                //    break;
            }

            m_objAP_Trans.POLineRef = rowPO_Trans.POLineRef;
            m_objAP_Trans.PONbr = (rowReceipt.RcptFrom == "PO" ? rowPO_Trans.PONbr : "");
            m_objAP_Trans.UnitPrice = rowPO_Trans.UnitCost;
            m_objAP_Trans.Qty = rowPO_Trans.RcptQty;
            //SubAcct
            m_objAP_Trans.TaxAmt00 = rowPO_Trans.TaxAmt00;
            m_objAP_Trans.TaxAmt01 = rowPO_Trans.TaxAmt01;
            m_objAP_Trans.TaxAmt02 = rowPO_Trans.TaxAmt02;
            m_objAP_Trans.TaxAmt03 = rowPO_Trans.TaxAmt03;
            m_objAP_Trans.TaxCat = rowPO_Trans.TaxCat;
            m_objAP_Trans.TaxId00 = rowPO_Trans.TaxID00;
            m_objAP_Trans.TaxId01 = rowPO_Trans.TaxID01;
            m_objAP_Trans.TaxId02 = rowPO_Trans.TaxID02;
            m_objAP_Trans.TaxId03 = rowPO_Trans.TaxID03;
          
            m_objAP_Trans.TranDate = m_objPO_Invoice.DocDate.Short();
            m_objAP_Trans.TranDesc = rowPO_Trans.TranDesc;
            m_objAP_Trans.TranType = (rowPO_Trans.TranType == "R" ? "VO" : "AD");
            m_objAP_Trans.TxblAmt00 = rowPO_Trans.TxblAmt00;
            m_objAP_Trans.TxblAmt01 = rowPO_Trans.TxblAmt01;
            m_objAP_Trans.TxblAmt02 = rowPO_Trans.TxblAmt02;
            m_objAP_Trans.TxblAmt03 = rowPO_Trans.TxblAmt03;
            m_objAP_Trans.UnitPrice = rowPO_Trans.UnitCost;
            m_objAP_Trans.VendID = rowPO_Trans.VendID;

            m_objAP_Trans.TranAmt = rowPO_Trans.TxblAmt00 + rowPO_Trans.TxblAmt01 + rowPO_Trans.TxblAmt02 + rowPO_Trans.TxblAmt03 + rowPO_Trans.TaxAmt00 + rowPO_Trans.TaxAmt01 + rowPO_Trans.TaxAmt02 + rowPO_Trans.TaxAmt03;

            m_objAP_Trans.Crtd_DateTime = DateTime.Now;
            m_objAP_Trans.Crtd_Prog = rowReceipt.Crtd_Prog;
            m_objAP_Trans.Crtd_User = rowReceipt.Crtd_User;
            m_objAP_Trans.LUpd_DateTime = DateTime.Now;
            m_objAP_Trans.LUpd_Prog = rowReceipt.LUpd_Prog;
            m_objAP_Trans.LUpd_User = rowReceipt.LUpd_User;
            m_objAP_Trans.Add();
        
        }
        private void InsertFeeAP_Trans( string APBatNbr, string APRefNbr, int lineRef, clsPO_Receipt rowReceipt, clsPO_Trans rowPO_Trans)
        {
            clsAP_Trans m_objAP_Trans=new clsAP_Trans(Dal);
            m_objAP_Trans.Reset();
            m_objAP_Trans.BranchID = rowReceipt.BranchID;
            m_objAP_Trans.BatNbr = APBatNbr;
            m_objAP_Trans.RefNbr = APRefNbr;
            m_objAP_Trans.LineRef = lineRef.ToString("00000");
            m_objAP_Trans.InvtID = rowPO_Trans.InvtID;
            m_objAP_Trans.JrnlType = rowPO_Trans.JrnlType;
            m_objAP_Trans.LineType = "C";
            //m_objAP_Trans.POFee = rowPO_Trans("RcptFee")
            m_objAP_Trans.POLineRef = rowPO_Trans.POLineRef;
            m_objAP_Trans.PONbr = rowPO_Trans.PONbr;
            //m_objAP_Trans.TaxAmt00 = m_dblTaxTots(0)
            //m_objAP_Trans.TaxAmt01 = m_dblTaxTots(1)
            //m_objAP_Trans.TaxAmt02 = m_dblTaxTots(2)
            //m_objAP_Trans.TaxAmt03 = m_dblTaxTots(3)
            m_objAP_Trans.TranAmt = rowPO_Trans.RcptFee;
            m_objAP_Trans.TranDate = m_objPO_Invoice.DocDate.Short();
            m_objAP_Trans.TranDesc = "Phí Xăng Dầu";
            m_objAP_Trans.TranType = (rowPO_Trans.TranType == "R" ? "VO" : "AD");
            m_objAP_Trans.VendID = rowPO_Trans.VendID;

            m_objAP_Trans.Crtd_DateTime = DateTime.Now;
            m_objAP_Trans.Crtd_Prog = rowReceipt.Crtd_Prog;
            m_objAP_Trans.Crtd_User = rowReceipt.Crtd_User;
            m_objAP_Trans.LUpd_DateTime = DateTime.Now;
            m_objAP_Trans.LUpd_Prog = rowReceipt.LUpd_Prog;
            m_objAP_Trans.LUpd_User = rowReceipt.LUpd_User;
            m_objAP_Trans.Add();
            //app.AP_Trans.AddObject(m_objAP_Trans);
            //app.SaveChanges();
        }
        private void InsertIN_Trans( string batNbr, clsPO_Receipt rowReceipt, clsPO_Trans rowPO_Trans)
        {
            clsIN_Trans m_objIN_Trans = new clsIN_Trans(Dal);

            //var mobjIN_Trans = app.IN_Trans.Where(p => p.BranchID == rowReceipt.BranchID && p.BatNbr == batNbr && p.RefNbr == rowReceipt.RcptNbr && p.LineRef == rowPO_Trans.LineRef).FirstOrDefault();
            //clsIN_Trans m_objIN_Trans=new clsIN_Trans(Dal);
            if (m_objIN_Trans.GetByKey(rowReceipt.BranchID, batNbr, rowReceipt.RcptNbr, rowPO_Trans.LineRef))
            {

                m_objIN_Trans.BranchID = rowReceipt.BranchID;
                m_objIN_Trans.BatNbr = batNbr;
                m_objIN_Trans.RefNbr = rowReceipt.RcptNbr;
                m_objIN_Trans.LineRef = rowPO_Trans.LineRef;
                m_objIN_Trans.CostID = rowPO_Trans.CostID;
                m_objIN_Trans.CnvFact = rowPO_Trans.RcptConvFact;
                m_objIN_Trans.FreeItem = (rowPO_Trans.PurchaseType == "PR" ? true : false);
                //ExtCost
                m_objIN_Trans.ExtCost = Math.Round(rowPO_Trans.TranAmt + rowPO_Trans.RcptFee, 0);
                m_objIN_Trans.InvtID = rowPO_Trans.InvtID;
                m_objIN_Trans.InvtMult = rowReceipt.RcptType == "R" ? (short)1 : (short)-1;
                m_objIN_Trans.JrnlType = "PO";
                m_objIN_Trans.ObjID = rowReceipt.VendID;
                m_objIN_Trans.Qty = rowPO_Trans.RcptQty;
                m_objIN_Trans.SiteID = rowPO_Trans.SiteID;
                m_objIN_Trans.TranAmt = rowPO_Trans.TxblAmt00;
                m_objIN_Trans.TranFee = rowPO_Trans.RcptFee;
                m_objIN_Trans.TranDate = rowPO_Trans.TranDate.Short();
                m_objIN_Trans.TranDesc = rowPO_Trans.TranDesc;
                m_objIN_Trans.TranType = (rowReceipt.RcptType == "R" ? "RC" : "II");
                //UnitCost
                m_objIN_Trans.UnitCost = rowPO_Trans.UnitCost;


                m_objIN_Trans.UnitDesc = rowPO_Trans.RcptUnitDescr;
                m_objIN_Trans.UnitMultDiv = rowPO_Trans.RcptMultDiv;
                m_objIN_Trans.UnitPrice = rowPO_Trans.UnitCost;
                m_objIN_Trans.Rlsed = 1;

                m_objIN_Trans.Crtd_DateTime = DateTime.Now;
                m_objIN_Trans.Crtd_Prog = rowReceipt.Crtd_Prog;
                m_objIN_Trans.Crtd_User = rowReceipt.Crtd_User;
                m_objIN_Trans.LUpd_DateTime = DateTime.Now;
                m_objIN_Trans.LUpd_Prog = rowReceipt.LUpd_Prog;
                m_objIN_Trans.LUpd_User = rowReceipt.LUpd_User;
                m_objIN_Trans.Update();
            }
            else
            {
                m_objIN_Trans.Reset();// = new IN_Trans();
                m_objIN_Trans.BranchID = rowReceipt.BranchID;
                m_objIN_Trans.BatNbr = batNbr;
                m_objIN_Trans.RefNbr = rowReceipt.RcptNbr;
                m_objIN_Trans.LineRef = rowPO_Trans.LineRef;
                m_objIN_Trans.CostID = rowPO_Trans.CostID;
                m_objIN_Trans.CnvFact = rowPO_Trans.RcptConvFact;
                m_objIN_Trans.FreeItem = (rowPO_Trans.PurchaseType == "PR" ? true : false);
                //ExtCost
                m_objIN_Trans.ExtCost = Math.Round(rowPO_Trans.TranAmt + rowPO_Trans.RcptFee, 0);
                m_objIN_Trans.InvtID = rowPO_Trans.InvtID;
                m_objIN_Trans.InvtMult = rowReceipt.RcptType == "R" ? (short)1 : (short)-1;
                m_objIN_Trans.JrnlType = "PO";
                m_objIN_Trans.ObjID = rowReceipt.VendID;
                m_objIN_Trans.Qty = rowPO_Trans.RcptQty;
                m_objIN_Trans.SiteID = rowPO_Trans.SiteID;
                m_objIN_Trans.TranAmt = rowPO_Trans.TxblAmt00;
                m_objIN_Trans.TranFee = rowPO_Trans.RcptFee;
                m_objIN_Trans.TranDate = rowPO_Trans.TranDate.Short();
                m_objIN_Trans.TranDesc = rowPO_Trans.TranDesc;
                m_objIN_Trans.TranType = rowReceipt.RcptType == "R" ? "RC" : "II";
                //UnitCost
                m_objIN_Trans.UnitCost = rowPO_Trans.UnitCost;


                m_objIN_Trans.UnitDesc = rowPO_Trans.RcptUnitDescr;
                m_objIN_Trans.UnitMultDiv = rowPO_Trans.RcptMultDiv;
                m_objIN_Trans.UnitPrice = rowPO_Trans.UnitCost;
                m_objIN_Trans.Rlsed = 1;

                m_objIN_Trans.Crtd_DateTime = DateTime.Now;
                m_objIN_Trans.Crtd_Prog = rowReceipt.Crtd_Prog;
                m_objIN_Trans.Crtd_User = rowReceipt.Crtd_User;
                m_objIN_Trans.LUpd_DateTime = DateTime.Now;
                m_objIN_Trans.LUpd_Prog = rowReceipt.LUpd_Prog;
                m_objIN_Trans.LUpd_User = rowReceipt.LUpd_User;
                m_objIN_Trans.Add();
              
            }
            #region Insert IN_LotTran
            //Insert IN_LotTran
            DataTable dtLotSer = new DataTable();
            clsPO_LotTrans objPO_LotTrans = new clsPO_LotTrans(Dal);
            clsIN_LotTrans objIN_LotTrans = new clsIN_LotTrans(Dal);
            dtLotSer = objPO_LotTrans.GetAll(rowReceipt.BranchID, batNbr, rowReceipt.RcptNbr, "%", rowPO_Trans.LineRef);
            IList<clsPO_LotTrans> _listclsobjPO_LotTrans = DataTableHelper.ConvertTo<clsPO_LotTrans>(dtLotSer);
            if ((dtLotSer != null) && dtLotSer.Rows.Count > 0)
            {
                foreach (var row in _listclsobjPO_LotTrans)
                {
                    if (objIN_LotTrans.GetByKey(row.BranchID, batNbr, row.RefNbr, row.LotSerNbr, rowPO_Trans.LineRef))
                    {

                        objIN_LotTrans.BranchID = rowReceipt.BranchID;
                        objIN_LotTrans.BatNbr = batNbr;
                        objIN_LotTrans.RefNbr = rowReceipt.RcptNbr;
                        objIN_LotTrans.LotSerNbr = row.LotSerNbr;
                        objIN_LotTrans.ExpDate = row.ExpDate;
                        objIN_LotTrans.INTranLineRef = row.POTranLineRef;
                        objIN_LotTrans.InvtID = row.InvtID;
                        objIN_LotTrans.InvtMult = rowReceipt.RcptType == "R" ? (short)1 : (short)-1;
                        objIN_LotTrans.KitID = row.KitID;
                        objIN_LotTrans.MfgrLotSerNbr = row.MfgrLotSerNbr;
                        objIN_LotTrans.Qty = row.Qty;
                        objIN_LotTrans.SiteID = row.SiteID;
                        objIN_LotTrans.ToSiteID = row.ToSiteID;
                        //objIN_LotTrans.ToWhseLoc = row.ToWhseLoc;
                        objIN_LotTrans.TranDate = row.TranDate;
                        objIN_LotTrans.TranType = rowReceipt.RcptType == "R" ? "RC" : "II";
                        //objIN_LotTrans.TranScr = "PO";
                        objIN_LotTrans.UnitCost = row.UnitCost;
                        objIN_LotTrans.UnitPrice = row.UnitPrice;
                        objIN_LotTrans.UnitDesc = row.UnitDesc;
                        objIN_LotTrans.UnitMultDiv = row.UnitMultDiv;
                        objIN_LotTrans.CnvFact = row.CnvFact;
                        objIN_LotTrans.WarrantyDate = row.WarrantyDate;
                        //objIN_LotTrans.ToWhseLoc = row.WhseLoc;
                        objIN_LotTrans.Crtd_Prog = Prog;
                        objIN_LotTrans.Crtd_User = User;
                        objIN_LotTrans.Crtd_DateTime = DateTime.Now;
                        objIN_LotTrans.LUpd_Prog = Prog;
                        objIN_LotTrans.LUpd_User = User;
                        objIN_LotTrans.LUpd_DateTime = DateTime.Now;
                        m_objIN_Trans.Update();
                    }
                    else
                    {
                        objIN_LotTrans.BranchID = rowReceipt.BranchID;
                        objIN_LotTrans.BatNbr = batNbr;
                        objIN_LotTrans.RefNbr = rowReceipt.RcptNbr;
                        objIN_LotTrans.LotSerNbr = row.LotSerNbr;
                        objIN_LotTrans.ExpDate = row.ExpDate;
                        objIN_LotTrans.INTranLineRef = row.POTranLineRef;
                        objIN_LotTrans.InvtID = row.InvtID;
                        objIN_LotTrans.InvtMult = rowReceipt.RcptType == "R" ? (short)1 : (short)-1;
                        objIN_LotTrans.KitID = row.KitID;
                        objIN_LotTrans.MfgrLotSerNbr = row.MfgrLotSerNbr;
                        objIN_LotTrans.Qty = row.Qty;
                        objIN_LotTrans.SiteID = row.SiteID;
                        objIN_LotTrans.ToSiteID = row.ToSiteID;
                        //objIN_LotTrans.ToWhseLoc = row.ToWhseLoc;
                        objIN_LotTrans.TranDate = row.TranDate;
                        objIN_LotTrans.TranType = rowReceipt.RcptType == "R" ? "RC" : "II";
                        //objIN_LotTrans.TranScr = "PO";
                        objIN_LotTrans.UnitCost = row.UnitCost;
                        objIN_LotTrans.UnitPrice = row.UnitPrice;
                        objIN_LotTrans.UnitDesc = row.UnitDesc;
                        objIN_LotTrans.UnitMultDiv = row.UnitMultDiv;
                        objIN_LotTrans.CnvFact = row.CnvFact;
                        objIN_LotTrans.WarrantyDate = row.WarrantyDate;
                        //objIN_LotTrans.ToWhseLoc = row.WhseLoc;
                        objIN_LotTrans.Crtd_Prog = Prog;
                        objIN_LotTrans.Crtd_User = User;
                        objIN_LotTrans.Crtd_DateTime = DateTime.Now;
                        objIN_LotTrans.LUpd_Prog = Prog;
                        objIN_LotTrans.LUpd_User = User;
                        objIN_LotTrans.LUpd_DateTime = DateTime.Now;
                        objIN_LotTrans.Add();
                    }

                }

            }
        #endregion
        }
       
        private void InsertAP_TransAPAcct( string APBatNbr, string APRefNbr, int lineRef, double tranAmt, clsPO_Receipt rowReceipt)
        {
            clsAP_Trans m_objAP_Trans=new clsAP_Trans(Dal);
            m_objAP_Trans.Reset();// = clsApp.ResetAP_Trans();
            m_objAP_Trans.BranchID = rowReceipt.BranchID;
            m_objAP_Trans.BatNbr = APBatNbr;
            m_objAP_Trans.RefNbr = APRefNbr;
            m_objAP_Trans.LineRef = lineRef.ToString("00000");
            m_objAP_Trans.JrnlType = "PO";
            m_objAP_Trans.LineType = "R";
            m_objAP_Trans.TranAmt = tranAmt;
            m_objAP_Trans.TranClass = "N";
            m_objAP_Trans.TranDate = m_objPO_Invoice.DocDate.Short();
            m_objAP_Trans.TranType = (rowReceipt.RcptType == "R" ? "VO" : "AD");
            m_objAP_Trans.VendID = rowReceipt.VendID;

            m_objAP_Trans.Crtd_DateTime = DateTime.Now;
            m_objAP_Trans.Crtd_Prog = rowReceipt.Crtd_Prog;
            m_objAP_Trans.Crtd_User = rowReceipt.Crtd_User;
            m_objAP_Trans.LUpd_DateTime = DateTime.Now;
            m_objAP_Trans.LUpd_Prog = rowReceipt.LUpd_Prog;
            m_objAP_Trans.LUpd_User = rowReceipt.LUpd_User;
           m_objAP_Trans.Add();
            //app.AP_Trans.AddObject(m_objAP_Trans);
            //app.SaveChanges();
        }
        private void InsertAP_Doc( string APBatNbr, string APRefNbr, bool promotion, clsPO_Receipt rowReceipt)
        {
            clsAP_Doc m_objAP_Doc = new clsAP_Doc(Dal);
            m_objAP_Doc.Reset();//=clsApp.ResetAP_Doc();
            m_objAP_Doc.BatNbr = APBatNbr;
            m_objAP_Doc.RefNbr = APRefNbr;

            m_objAP_Doc.BranchID = rowReceipt.BranchID;
            m_objAP_Doc.DocBal = rowReceipt.RcptTotAmt;

            m_objAP_Doc.DocDate = m_objPO_Invoice.DocDate.Short();
            m_objAP_Doc.DocDesc = rowReceipt.Descr;
            m_objAP_Doc.DocType = m_objPO_Invoice.DocType;
            m_objAP_Doc.DueDate = m_objPO_Invoice.InvcDate.Date.AddDays(m_objSI_Terms == null ? 0 : m_objSI_Terms.DueIntrv).Short();
            m_objAP_Doc.InvcDate = m_objPO_Invoice.InvcDate.Short();
            m_objAP_Doc.InvcNbr = m_objPO_Invoice.InvcNbr;
            m_objAP_Doc.InvcNote = m_objPO_Invoice.InvcNote;
            m_objAP_Doc.OrigDocAmt = m_objAP_Doc.DocBal;
            m_objAP_Doc.PONbr = rowReceipt.PONbr;
            m_objAP_Doc.RcptNbr = rowReceipt.RcptNbr;
            m_objAP_Doc.TaxId00 = rowReceipt.TaxID00;
            m_objAP_Doc.TaxId01 = rowReceipt.TaxID01;
            m_objAP_Doc.TaxId02 = rowReceipt.TaxID02;
            m_objAP_Doc.TaxId03 = rowReceipt.TaxID03;
            m_objAP_Doc.TaxTot00 = rowReceipt.TaxAmtTot00;
            m_objAP_Doc.TaxTot01 = rowReceipt.TaxAmtTot01;
            m_objAP_Doc.TaxTot02 = rowReceipt.TaxAmtTot02;
            m_objAP_Doc.TaxTot03 = rowReceipt.TaxAmtTot03;
            m_objAP_Doc.Terms = m_objPO_Invoice.Terms;
            m_objAP_Doc.TxblTot00 = rowReceipt.TxblAmtTot00;
            m_objAP_Doc.TxblTot01 = rowReceipt.TxblAmtTot01;
            m_objAP_Doc.TxblTot02 = rowReceipt.TxblAmtTot02;
            m_objAP_Doc.TxblTot03 = rowReceipt.TxblAmtTot03;
            m_objAP_Doc.VendID = rowReceipt.VendID;

            m_objAP_Doc.Crtd_DateTime = DateTime.Now;
            m_objAP_Doc.Crtd_Prog = rowReceipt.Crtd_Prog;
            m_objAP_Doc.Crtd_User = rowReceipt.Crtd_User;
            m_objAP_Doc.LUpd_DateTime = DateTime.Now;
            m_objAP_Doc.LUpd_Prog = rowReceipt.LUpd_Prog;
            m_objAP_Doc.LUpd_User = rowReceipt.LUpd_User;
            m_objAP_Doc.Add();
            //app.AP_Doc.AddObject(m_objAP_Doc);
            //app.SaveChanges();
        }
        private void InsertAPBatch( string APBatNbr, clsPO_Receipt rowReceipt)
        {
            clsBatch m_objBatch = new clsBatch(Dal);
            m_objBatch.Reset();
            m_objBatch.BranchID = rowReceipt.BranchID;
            m_objBatch.BatNbr = APBatNbr;
            m_objBatch.Module1 = "AP";
            m_objBatch.JrnlType = "PO";
            m_objBatch.DateEnt = m_objBatchIN.DateEnt.Short();
            m_objBatch.EditScrnNbr = "AP10100";
            m_objBatch.Status = "B";
            m_objBatch.Crtd_DateTime = DateTime.Now;
            m_objBatch.Crtd_Prog = Prog;
            m_objBatch.Crtd_User = rowReceipt.Crtd_User;
            m_objBatch.LUpd_DateTime = DateTime.Now;
            m_objBatch.LUpd_Prog = Prog;
            m_objBatch.LUpd_User = rowReceipt.Crtd_User;
            m_objBatch.Add();
          
        }
        private void UpdatePODetail( clsPO_Trans row)
        {
            clsPO_Detail objPO_Detail = new clsPO_Detail(Dal);
            //PO_Receipt objPO_Receipt = new PO_Receipt();
            try
            {
                double qtyTrans = (row.RcptMultDiv == "D" ? row.RcptQty / row.RcptConvFact : row.RcptQty * row.RcptConvFact);//quy ve don vi luu kho 20150326
                //objPO_Receipt = app.PO_Receipt.Where(p => p.BranchID == row.BranchID && p.BatNbr == row.BatNbr && p.RcptNbr == row.RcptNbr).FirstOrDefault();
                if (objPO_Detail.GetByKey(row.BranchID, row.PONbr, row.POLineRef))
                {
                    var qtyOrd = objPO_Detail.UnitMultDiv == "M" ? objPO_Detail.QtyOrd * objPO_Detail.CnvFact : objPO_Detail.QtyOrd / objPO_Detail.CnvFact;
                    var qtyRcvd = objPO_Detail.UnitMultDiv == "M" ? objPO_Detail.QtyRcvd * objPO_Detail.CnvFact : objPO_Detail.QtyRcvd / objPO_Detail.CnvFact;

                    // = app.PO_Detail.Where(p => p.BranchID == row.BranchID") && p.PONbr == row.PONbr") && p.LineRef == row.POLineRef")).FirstOrDefault();
                    if (row.TranType == "R")
                    {                       
                       
                        if (qtyTrans >= (qtyOrd - qtyRcvd))
                        {
                            objPO_Detail.RcptStage = "F";
                            double OldQty = 0;
                            if (row.PurchaseType == "GN" || row.PurchaseType == "GI" || row.PurchaseType == "PR")
                            {
                                OldQty = (objPO_Detail.UnitMultDiv == "D" ? (objPO_Detail.QtyOrd - objPO_Detail.QtyRcvd) / objPO_Detail.CnvFact : (objPO_Detail.QtyOrd - objPO_Detail.QtyRcvd) * objPO_Detail.CnvFact);
                                //var objItemSite = app.IN_ItemSite.Where(p => p.InvtID == row.InvtID") && p.SiteID == row.SiteID")).FirstOrDefault();
                                clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
                                if (objItemSite.GetByKey(row.InvtID, row.SiteID))
                                {
                                    objItemSite.QtyOnPO = Math.Round(objItemSite.QtyOnPO + 0 - qtyTrans, 0);
                                    objItemSite.Update();
                                }
                            }
                        }
                        else 
                        {
                            objPO_Detail.RcptStage = "P";
                            double OldQty = 0;
                            if ((row.PurchaseType == "GN" || row.PurchaseType == "GI" | row.PurchaseType == "PR"))
                            {
                                OldQty = (row.RcptMultDiv == "D" ? row.RcptQty / row.RcptConvFact : row.RcptQty * row.RcptConvFact);
                                clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
                                if (objItemSite.GetByKey(row.InvtID, row.SiteID))
                                {
                                    // var objItemSite = app.IN_ItemSite.Where(p => p.InvtID == row.InvtID") && p.SiteID == row.SiteID")).FirstOrDefault();                                                   
                                    objItemSite.QtyOnPO = Math.Round(objItemSite.QtyOnPO + 0 - OldQty, 0);
                                    objItemSite.Update();

                                }
                                //POCommon.UpdateOnPOQty(row.InvtID, row.SiteID, OldQty, 0, 0);
                            }
                        }
                        //QUY doi ve don vi theo PODetail  20150326
                        double qty = (objPO_Detail.UnitMultDiv == "M" ? qtyTrans / objPO_Detail.CnvFact : qtyTrans * objPO_Detail.CnvFact);
                        objPO_Detail.QtyRcvd = objPO_Detail.QtyRcvd + qty; 
                        objPO_Detail.CostReceived = objPO_Detail.CostReceived + row.TranAmt;
                    }
                    else if (row.TranType == "X")
                    { 
                        //QUY doi ve don vi theo PODetail  20150326
                        double qty = (objPO_Detail.UnitMultDiv == "M" ? qtyTrans / objPO_Detail.CnvFact : qtyTrans * objPO_Detail.CnvFact);
                        objPO_Detail.QtyRcvd = objPO_Detail.QtyRcvd - qty;
                        objPO_Detail.CostReceived = objPO_Detail.CostReceived - row.TranAmt;

                        //double qty = (objPO_Detail.UnitMultDiv == "M" ? qtyTrans / objPO_Detail.CnvFact : qtyTrans * objPO_Detail.CnvFact);
                        objPO_Detail.QtyReturned = objPO_Detail.QtyReturned + qty;
                        objPO_Detail.CostReturned = objPO_Detail.CostReturned + row.TranAmt;
                        double OldQty = 0;
                        if ((row.PurchaseType == "GN" || row.PurchaseType == "GI" | row.PurchaseType == "PR") & objPO_Detail.RcptStage != "N")
                        {
                            OldQty = (row.RcptMultDiv == "D" ? row.RcptQty / row.RcptConvFact : row.RcptQty * row.RcptConvFact);
                            clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
                            if (objItemSite.GetByKey(row.InvtID, row.SiteID))
                            {
                                objItemSite.QtyOnPO = Math.Round(objItemSite.QtyOnPO + OldQty, 0);
                                objItemSite.Update();

                            }
                        }
                        if (qty == objPO_Detail.QtyOrd)
                        {
                            objPO_Detail.RcptStage = "N";
                        }
                        else if (qty < objPO_Detail.QtyOrd)
                        {
                            objPO_Detail.RcptStage = "P";
                        }
                    }
                    objPO_Detail.LUpd_DateTime = DateTime.Now;
                    objPO_Detail.LUpd_Prog = Prog;
                    objPO_Detail.LUpd_User = row.LUpd_User;

                    objPO_Detail.Update();
                }
            }
            catch (Exception ex)
            {
             
                throw ex;
            }

        }
        private void UpdatePOHeader( string BranchID, string PONbr)
        {
            int i = 0;
            DataTable dtdetail;// = new   List<PO_Detail>();
            bool chkF = false;
            bool chkN = false;
            bool chkP = false;
            clsPO_Header objPO_Header = new clsPO_Header(Dal);
            clsPO_Detail objPO_Detail = new clsPO_Detail(Dal);
            double TotRcptAmt = 0;
            try
            {
                objPO_Header.GetByKey(BranchID,PONbr);// = app.PO_Header.Where(p => p.BranchID == BranchID && p.PONbr == PONbr).FirstOrDefault();
                dtdetail = objPO_Detail.GetAll(BranchID, PONbr,"%");// app.PO_Detail.Where(p => p.BranchID == BranchID && p.PONbr == PONbr).ToList();
                try
                {
                    TotRcptAmt =DataTableHelper.ConvertTo<clsPO_Detail>(dtdetail).Sum(p=>p.CostReceived);
                }
                catch (Exception ex)
                {
                    TotRcptAmt = 0;
                }
                objPO_Header.RcptTotAmt = TotRcptAmt;
                if ((dtdetail.Rows.Count > 0))
                {
                    IList<clsPO_Detail> Listclsdtdetail = DataTableHelper.ConvertTo<clsPO_Detail>(dtdetail);
                    foreach (clsPO_Detail obj in Listclsdtdetail)
                    {
                        if (obj.RcptStage == "N")
                        {
                            chkN = true;
                        }
                        else if (obj.RcptStage == "P")
                        {
                            chkP = true;
                        }
                        else if (obj.RcptStage == "F")
                        {
                            chkF = true;
                        }
                    }

                }
                if (chkF & !chkP & !chkN)
                {
                    objPO_Header.RcptStage = "F";
                    objPO_Header.Status = "C";
                }
                else if (chkN & !chkP & !chkF)
                {
                    objPO_Header.RcptStage = "N";
                    objPO_Header.Status = "O";
                }
                else
                {
                    objPO_Header.RcptStage = "P";
                    objPO_Header.Status = "O";
                }
                objPO_Header.Update();
                //app.SaveChanges();
            }
            catch (Exception ex)
            {
              
                throw ex;
            }
        }
        private bool ReceiptIN( clsPO_Receipt row)
        {
            DataTable dtBatch ;
            DataTable dtLotSer = new DataTable();
            clsIN_Trans objIN_Trans = new clsIN_Trans(Dal);
            clsIN_LotTrans objIN_LotTrans = new clsIN_LotTrans(Dal);
            //IN_Setup objIN_Setup = new IN_Setup();
            clsIN_ItemSite objIN_ItemSite = new clsIN_ItemSite(Dal);
            clsIN_ItemCost objIN_ItemCost = new clsIN_ItemCost(Dal);
         
            clsIN_Inventory objIN_Inventory = new clsIN_Inventory(Dal);
            clsPO_Trans objPO_Trans = new clsPO_Trans(Dal);
      

            double Qty = 0;
            double UnitCost = 0;

            try
            {
                //Get Batch for processing
                //objIN_Setup = app.IN_Setup.Where(p => p.SetupID == "IN" &&p.BranchID=).FirstOrDefault();
                dtBatch = objIN_Trans.GetAll(row.BranchID, row.BatNbr, row.RcptNbr, "%");// app.IN_Trans.Where(p => p.BranchID == row.BranchID && p.BatNbr == row.BatNbr && p.RefNbr == row.RcptNbr).ToList();
                #region Release IN_Trans
                if (dtBatch.Rows.Count > 0)
                {
                   
                    IList<clsIN_Trans> ListclsIN_Trans = DataTableHelper.ConvertTo<clsIN_Trans>(dtBatch);
                    foreach (clsIN_Trans objrowIN_Tran in ListclsIN_Trans)
                    {
                        objIN_Inventory = new clsIN_Inventory(Dal);
                        objIN_ItemSite = new clsIN_ItemSite(Dal);
                        objIN_Inventory.GetByKey(objrowIN_Tran.InvtID);// = app.IN_Inventory.Where(p => p.InvtID == dtBatch1.InvtID).FirstOrDefault();
                        if (!objIN_ItemSite.GetByKey(objrowIN_Tran.InvtID, objrowIN_Tran.SiteID))
                        {
                        
                            Insert_IN_ItemSite( ref objIN_ItemSite, objIN_Inventory, objrowIN_Tran.SiteID);

                        }
                        //Calculate Qty by stkunit
                        if (objrowIN_Tran.UnitMultDiv == "M" | string.IsNullOrEmpty(objrowIN_Tran.UnitMultDiv))
                        {
                            Qty = Math.Round(objrowIN_Tran.Qty * objrowIN_Tran.InvtMult * objrowIN_Tran.CnvFact, 0);
                            UnitCost = Math.Round(objrowIN_Tran.UnitCost / objrowIN_Tran.CnvFact, 0);
                        }
                        else
                        {
                            Qty = Math.Round((objrowIN_Tran.Qty * objrowIN_Tran.InvtMult) / objrowIN_Tran.CnvFact, 0);
                            UnitCost = Math.Round(objrowIN_Tran.UnitCost * objrowIN_Tran.CnvFact, 0);
                        }
                        objPO_Trans = new clsPO_Trans(Dal);
                        objIN_ItemSite.GetByKey(objrowIN_Tran.InvtID, objrowIN_Tran.SiteID);
                        if (objPO_Trans.GetByKey(row.BranchID, row.BatNbr, row.RcptNbr, objrowIN_Tran.LineRef))
                        {
                            if ((objPO_Trans.PurchaseType == "GN" || objPO_Trans.PurchaseType == "GI" || objPO_Trans.PurchaseType == "PR" || objPO_Trans.PurchaseType == "GP" || objPO_Trans.PurchaseType == "GS"))
                            {
                                //Update Qty and Cost for Site
                                objIN_ItemSite.QtyOnHand = Math.Round(objIN_ItemSite.QtyOnHand + Qty, 10);
                                objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail + Qty, 10);
                                objIN_ItemSite.TotCost = Math.Round(objIN_ItemSite.TotCost + objrowIN_Tran.ExtCost, 0);
                                //QuangNM change to fix Issues QtyUncosted
                                //objIN_ItemSite.AvgCost = Math.Round(IIf((objIN_ItemSite.QtyOnHand) <> 0, objIN_ItemSite.TotCost / (objIN_ItemSite.QtyOnHand), objIN_ItemSite.AvgCost), m_HQSys.DecplUnitPrice, MidpointRounding.AwayFromZero)
                                objIN_ItemSite.AvgCost = Math.Round(((objIN_ItemSite.QtyOnHand - objIN_ItemSite.QtyUncosted) != 0 ? objIN_ItemSite.TotCost / (objIN_ItemSite.QtyOnHand - objIN_ItemSite.QtyUncosted) : objIN_ItemSite.AvgCost), 0);
                                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                                objIN_ItemSite.LUpd_Prog = Prog;
                                objIN_ItemSite.LUpd_User = row.LUpd_User;
                                objIN_ItemSite.Update();

                                objIN_ItemCost.Reset();
                                //Add to Item_Cost for LIFO/FIFO/Specified Cost
                                objIN_ItemCost.CostID = objrowIN_Tran.CostID;
                                objIN_ItemCost.InvtID = objrowIN_Tran.InvtID;
                                objIN_ItemCost.Qty = Qty;
                                //dtBatch(i).Item("Qty")
                                objIN_ItemCost.RcptDate = objrowIN_Tran.TranDate;
                                objIN_ItemCost.RcptNbr = row.RcptNbr;
                                objIN_ItemCost.SiteID = objrowIN_Tran.SiteID;
                                objIN_ItemCost.TotCost = objrowIN_Tran.ExtCost;
                                objIN_ItemCost.UnitCost = UnitCost;

                                objIN_ItemCost.Crtd_DateTime = DateTime.Now;
                                objIN_ItemCost.Crtd_Prog = row.Crtd_Prog;
                                objIN_ItemCost.Crtd_User = row.Crtd_User;
                                objIN_ItemCost.LUpd_DateTime = DateTime.Now;
                                objIN_ItemCost.LUpd_Prog = row.LUpd_Prog;
                                objIN_ItemCost.LUpd_User = row.LUpd_User;
                                objIN_ItemCost.Add();
                            }
                        }

                        #region//Lot/Serial Processing
                        if (objIN_Inventory.StkItem == 1 && objIN_Inventory.LotSerTrack != "N" && objIN_Inventory.LotSerTrack.PassNull() != string.Empty)
                        {
                            DataTable dtInLotTrans = objIN_LotTrans.GetAll(row.BranchID, row.BatNbr, objrowIN_Tran.RefNbr, "%", objrowIN_Tran.LineRef);
                            IList<clsIN_LotTrans> ListdtInLotTrans = DataTableHelper.ConvertTo<clsIN_LotTrans>(dtInLotTrans);

                            foreach (var r1 in ListdtInLotTrans)
                            {
                                clsIN_ItemLot objIN_ItemLot = new clsIN_ItemLot(Dal);
                                Qty = Math.Round(r1.Qty * r1.InvtMult * (r1.UnitMultDiv == "D" ? 1.0 / r1.CnvFact : r1.CnvFact), 0, MidpointRounding.AwayFromZero);
                                if (!objIN_ItemLot.GetByKey(objrowIN_Tran.SiteID, objrowIN_Tran.InvtID, r1.LotSerNbr))
                                {
                                    Insert_IN_ItemLot(ref objIN_ItemLot, objrowIN_Tran.InvtID, objrowIN_Tran.SiteID, r1.LotSerNbr, r1.ExpDate, "", 0, 0);
                                    objIN_ItemLot.Crtd_Prog = r1.Crtd_Prog;
                                    objIN_ItemLot.Crtd_User = r1.Crtd_User;
                                    objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                                    objIN_ItemLot.LUpd_Prog = r1.Crtd_Prog;
                                    objIN_ItemLot.LUpd_User = r1.LUpd_User;
                                }
                                if (objIN_Inventory.LotSerTrack == "L")
                                    objIN_ItemLot.WarrantyDate = row.RcptDate.AddDays(objIN_Inventory.WarrantyDays);
                                else objIN_ItemLot.WarrantyDate = row.RcptDate;
                                objIN_ItemLot.ExpDate = r1.ExpDate;
                                objIN_ItemLot.MfgrLotSerNbr = r1.MfgrLotSerNbr;
                                objIN_ItemLot.Cost = objIN_ItemLot.Cost + objrowIN_Tran.ExtCost / objrowIN_Tran.Qty * r1.Qty;
                                objIN_ItemLot.QtyOnHand = Math.Round(objIN_ItemLot.QtyOnHand + Qty, 10);
                                //objIN_ItemLot.AvgCost = Math.Round(objIN_ItemLot.QtyOnHand != 0 ? (objIN_ItemLot.Cost / objIN_ItemLot.QtyOnHand) : objIN_ItemLot.AvgCost, 0);
                                objIN_ItemLot.QtyAvail = Math.Round(objIN_ItemLot.QtyAvail + Qty, 10);
                                objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                                objIN_ItemLot.LUpd_Prog = Prog;
                                objIN_ItemLot.LUpd_User = User;
                                objIN_ItemLot.Update();
                            }
                        }
                        #endregion
                    }

                }
                #endregion
                
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool IssueIN( clsPO_Receipt row)
        {
            DataTable dtBatch ;
            DataTable dtLotSer = new DataTable();
            clsIN_Trans objIN_Trans = new clsIN_Trans(Dal);
            clsIN_LotTrans objIN_LotTrans = new clsIN_LotTrans(Dal);   
            clsIN_ItemSite objIN_ItemSite = new clsIN_ItemSite(Dal);
            clsIN_ItemCost objIN_ItemCost = new clsIN_ItemCost(Dal);         
            clsIN_Inventory objIN_Inventory = new clsIN_Inventory(Dal);
            clsPO_Trans objPO_Trans = new clsPO_Trans(Dal);
             clsIN_Setup objIN_Setup = new clsIN_Setup(Dal);
            double Qty = 0;
            try
            {
                //Get Batch for processing
                objIN_Setup.GetByKey(row.BranchID,"IN");
                dtBatch = objIN_Trans.GetAll(row.BranchID, row.BatNbr, row.RcptNbr, "%");// app.IN_Trans.Where(p => p.BranchID == row.BranchID && p.BatNbr == row.BatNbr && p.RefNbr == row.RcptNbr).ToList();
                IList<clsIN_Trans> ListclsIN_Trans = DataTableHelper.ConvertTo<clsIN_Trans>(dtBatch);
                foreach (clsIN_Trans dtInTrans in ListclsIN_Trans)
                {
                    //objIN_Inventory = app.IN_Inventory.Where(p => p.InvtID == dtBatch1.InvtID")).FirstOrDefault();
                    //objIN_ItemSite = app.IN_ItemSite.Where(p => p.InvtID == dtBatch1.InvtID && p.SiteID == dtBatch1.SiteID).FirstOrDefault();
                    objIN_Inventory = new clsIN_Inventory(Dal);
                    objIN_ItemSite = new clsIN_ItemSite(Dal);
                    objIN_Inventory.GetByKey(dtInTrans.InvtID);// = app.IN_Inventory.Where(p => p.InvtID == dtBatch1.InvtID).FirstOrDefault();
                    if (!objIN_ItemSite.GetByKey(dtInTrans.InvtID, dtInTrans.SiteID))
                    {
                        throw new MessageException(MessageType.Message, "606");                       
                 
                    }


                    //Calculate Qty by stkunit
                    if (dtInTrans.UnitMultDiv == "M" | string.IsNullOrEmpty(dtInTrans.UnitMultDiv))
                    {
                        Qty = dtInTrans.Qty * dtInTrans.InvtMult * dtInTrans.CnvFact;
                    }
                    else
                    {
                        Qty = (dtInTrans.Qty * dtInTrans.InvtMult) / dtInTrans.CnvFact;
                    }

                    //objPO_Trans = app.PO_Trans.Where(p => p.BranchID == row.BranchID && p.BatNbr == row.BatNbr && p.RcptNbr == row.RcptNbr && p.LineRef == dtBatch1.LineRef).FirstOrDefault();// &&p. row.BatNbr, row.RcptNbr, dtBatch1.LineRef);
                    objPO_Trans = new clsPO_Trans(Dal);
                    if (objPO_Trans.GetByKey(row.BranchID, row.BatNbr, row.RcptNbr, dtInTrans.LineRef))
                    {
                        if ((objPO_Trans.PurchaseType == "GN" || objPO_Trans.PurchaseType == "GI" || objPO_Trans.PurchaseType == "PR" || objPO_Trans.PurchaseType == "GP" || objPO_Trans.PurchaseType == "GS"))
                        {
                            //Update Qty and Cost for Site
                            objIN_ItemSite.QtyAllocPORet = Math.Round(objIN_ItemSite.QtyAllocPORet + Qty, 0);
                            //Reduce Allocated Qty for inventory issue
                            //objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail + Qty, 10);
                            objIN_ItemSite.QtyOnHand = Math.Round(objIN_ItemSite.QtyOnHand + Qty, 10);
                            objIN_ItemSite.TotCost = Math.Round(objIN_ItemSite.TotCost + dtInTrans.ExtCost * dtInTrans.InvtMult, 0);
                            //QuangNM change to fix issues QtyUncosted
                            //objIN_ItemSite.AvgCost = Math.Round(IIf((objIN_ItemSite.QtyOnHand) <> 0, objIN_ItemSite.TotCost / (objIN_ItemSite.QtyOnHand), objIN_ItemSite.AvgCost), m_HQSys.DecplUnitPrice)
                            objIN_ItemSite.AvgCost = Math.Round(((objIN_ItemSite.QtyOnHand - objIN_ItemSite.QtyUncosted) != 0 ? objIN_ItemSite.TotCost / (objIN_ItemSite.QtyOnHand - objIN_ItemSite.QtyUncosted) : objIN_ItemSite.AvgCost), 0);
                            objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                            objIN_ItemSite.LUpd_Prog = objPO_Trans.LUpd_Prog;
                            objIN_ItemSite.LUpd_User = objPO_Trans.LUpd_User;
                            objIN_ItemSite.Update();
                        }
                        //Checking Qty,TotCost      
                        if (objIN_Inventory.StkItem == 1 && !objIN_Setup.NegQty && Math.Round(objIN_ItemSite.QtyOnHand, 0, MidpointRounding.AwayFromZero) < 0)
                        {
                            throw new MessageException(MessageType.Message, "608","", new[] { objIN_ItemSite.InvtID, objIN_ItemSite.SiteID });
                        }

                        #region //Lot/Serial Processing
                        if (objIN_Inventory.StkItem == 1 && objIN_Inventory.LotSerTrack != "N" && objIN_Inventory.LotSerTrack.PassNull() != string.Empty)
                        {
                            objIN_LotTrans = new clsIN_LotTrans(Dal);
                            var dtInLotTrans = DataTableHelper.ConvertTo<clsIN_LotTrans>(objIN_LotTrans.GetAll(row.BranchID, row.BatNbr, "%", "%", dtInTrans.LineRef));
                            foreach (var r1 in dtInLotTrans)
                            {
                                var objIN_ItemLot = new clsIN_ItemLot(Dal);
                                Qty = Math.Round(r1.Qty * r1.InvtMult * (r1.UnitMultDiv == "D" ? 1.0 / r1.CnvFact : r1.CnvFact), 0, MidpointRounding.AwayFromZero);
                                if (!objIN_ItemLot.GetByKey(dtInTrans.SiteID, dtInTrans.InvtID, r1.LotSerNbr))
                                {
                                    //chua xu li
                                    Insert_IN_ItemLot(ref objIN_ItemLot, dtInTrans.InvtID, dtInTrans.SiteID, r1.LotSerNbr, r1.ExpDate, "", 0, 0);
                                    objIN_ItemLot.Crtd_Prog = r1.Crtd_Prog;
                                    objIN_ItemLot.Crtd_User = r1.Crtd_User;
                                    objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                                    objIN_ItemLot.LUpd_Prog = r1.Crtd_Prog;
                                    objIN_ItemLot.LUpd_User = r1.LUpd_User;
                                }


                                if (objIN_Inventory.LotSerTrack == "L")
                                    objIN_ItemLot.WarrantyDate = row.RcptDate.AddDays(objIN_Inventory.WarrantyDays);
                                else objIN_ItemLot.WarrantyDate = row.RcptDate;

                                objIN_ItemLot.ExpDate = r1.ExpDate;
                                objIN_ItemLot.MfgrLotSerNbr = r1.MfgrLotSerNbr;
                                objIN_ItemLot.Cost = objIN_ItemLot.Cost + (dtInTrans.ExtCost / dtInTrans.Qty) * r1.Qty;
                                objIN_ItemLot.QtyAvail = Math.Round(objIN_ItemLot.QtyAvail + Qty, 0);
                                objIN_ItemLot.QtyOnHand = Math.Round(objIN_ItemLot.QtyOnHand + Qty, 10);
                                objIN_ItemLot.QtyAllocPORet = Math.Round(objIN_ItemLot.QtyAllocPORet + Qty, 0, MidpointRounding.AwayFromZero);
                                //objIN_ItemLot.AvgCost = Math.Round((objIN_ItemLot.QtyOnHand) != 0 ? (objIN_ItemLot.Cost) / (objIN_ItemLot.QtyOnHand) : objIN_ItemLot.AvgCost, 0);
                                objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                                objIN_ItemLot.LUpd_Prog = row.LUpd_Prog;
                                objIN_ItemLot.LUpd_User = row.LUpd_User;
                                objIN_ItemLot.Update();
                                //Checking Qty
                                if (objIN_Inventory.StkItem == 1 && !objIN_Setup.NegQty && Math.Round(objIN_ItemLot.QtyOnHand, 0, MidpointRounding.AwayFromZero) < 0)
                                {
                                    throw new MessageException("608", new[] { objIN_ItemSite.InvtID, objIN_ItemSite.SiteID });
                                }
                            }
                        }
                        #endregion

                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Insert_IN_ItemSite(ref clsIN_ItemSite objIN_ItemSite, clsIN_Inventory objIN_Inventory, string SiteID)
        {
            try
            {
                objIN_ItemSite.Reset();// = clsApp.ResetIN_ItemSite();
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
                objIN_ItemSite.Crtd_Prog = Prog;
                objIN_ItemSite.Crtd_User = objIN_Inventory.Crtd_Prog;
                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                objIN_ItemSite.LUpd_Prog = Prog;
                objIN_ItemSite.LUpd_User = objIN_Inventory.Crtd_Prog;
                objIN_ItemSite.LastPurchaseDate = DateTime.Now.Short();
                objIN_ItemSite.Add();
              
            }
            catch (Exception ex)
            {
               
                throw ex;
               
            }
        }
        private void Insert_IN_ItemLot(ref clsIN_ItemLot objIN_ItemLot, string InvtID, string SiteID, string LotSerNbr, System.DateTime ExpDate, string MfgrLotSerNbr, double Cost, double Qty)
        {
            try
            {
                objIN_ItemLot.SiteID = SiteID;             
                objIN_ItemLot.InvtID = InvtID;
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
        private bool AP10100_Release(string BranchID, string BatNbr)
        {
            DataTable dtAPDoc ;//= new List<AP_Doc>();
            try
            {
                clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
                clsAP_Balances objAP_Balances = new clsAP_Balances(Dal);
                //Get Batch for processing
                dtAPDoc = objAP_Doc.GetAll(BranchID, BatNbr,"%");// app.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                IList<clsAP_Doc> ListclsAP_Doc = DataTableHelper.ConvertTo<clsAP_Doc>(dtAPDoc);
                foreach (clsAP_Doc dr in ListclsAP_Doc)
                {
                    ProcessAPBalance(  dr.VendID, dr.DocDate, dr.DocType, dr.OrigDocAmt);
                }
                return true;
            }
            catch (Exception ex)
            {
              
                throw ex;
            }
        }
        private void ProcessAPBalance( string VendID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                clsAP_Balances objAP_Bal = new clsAP_Balances(Dal);

                //var objAP_Bal = (from p in app.AP_Balances where p.VendID == VendID select p).FirstOrDefault();
                if (objAP_Bal.GetByKey(VendID))
                {
                    UpdateAPBalance(ref objAP_Bal,VendID, DocDate, DocType, OrigDocAmt);
                    objAP_Bal.Update();
                }
                else
                {
                    objAP_Bal.Reset();
                    UpdateAPBalance(ref objAP_Bal, VendID, DocDate, DocType, OrigDocAmt);
                    objAP_Bal.VendID = VendID;
                    objAP_Bal.Crtd_DateTime = DateTime.Now;
                    objAP_Bal.Crtd_Prog = Prog;
                    objAP_Bal.Crtd_User = User;
                    objAP_Bal.LastChkDate = DateTime.Now.Short();
                    objAP_Bal.LastVODate = DateTime.Now.Short();
                    objAP_Bal.Add();
                }
             
            }
            catch (Exception ex)
            {
              
                throw ex;
            }
        }
        private void UpdateAPBalance(ref clsAP_Balances objAP_Balances, string VendID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                objAP_Balances.VendID = VendID;
                objAP_Balances.CurrBal = objAP_Balances.CurrBal + ((DocType == "VO" | DocType == "AC" | DocType == "CR" ? OrigDocAmt : 0));
                objAP_Balances.CurrBal = objAP_Balances.CurrBal + ((DocType == "AD" | DocType == "PP" ? -1 * OrigDocAmt : 0));
                objAP_Balances.LUpd_DateTime = DateTime.Now;
                objAP_Balances.LUpd_Prog =Prog;
                objAP_Balances.LUpd_User = User;
            }
            catch (Exception ex)
            {
               
                throw ex;
            }
        }
        
        #endregion
        #region PO10200 Cancel
        public bool PO10200_Cancel(string BranchID, string BatNbr, string RcptNbr, bool bCheck714)
        {
            DataTable dtReceipt;// = new List<PO_Receipt>();
            DataTable dtPOInvoice;// = new List<PO_Invoice>();
            DataTable dtAP_Doc;//= new List<AP_Doc>();
            DataTable dtIN_Trans;// = new List<IN_Trans>();
            clsPO_Receipt objPO_Receipt = new clsPO_Receipt(Dal);
            clsPO_Invoice objPO_Invoice = new clsPO_Invoice(Dal);
            clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
            clsIN_Trans objIN_Trans = new clsIN_Trans(Dal);
            clsSQL objSql = new clsSQL(Dal);
            //Checking                      
            dtReceipt = objPO_Receipt.GetAll(BranchID, BatNbr, "%");// app.PO_Receipt.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
            if (dtReceipt.Rows.Count > 0)
            {
                IList<clsPO_Receipt> ListdtReceipt = DataTableHelper.ConvertTo<clsPO_Receipt>(dtReceipt);
                foreach (clsPO_Receipt obj in ListdtReceipt)
                {
                    if (obj.Rlsed != -1)
                    {
                        if (objSql.PO_CheckForCancel(BranchID, BatNbr, obj.RcptNbr) == "1")
                        {
                            throw new MessageException(MessageType.Message, "144", "", new[] { obj.RcptNbr });

                        }

                    }
                }
            }

            //Check AP

            dtPOInvoice = objPO_Invoice.GetAll(BranchID, BatNbr, "%");// app.PO_Invoice.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
            if (dtPOInvoice.Rows.Count > 0)
                objPO_Invoice = DataTableHelper.ConvertTo<clsPO_Invoice>(dtPOInvoice).FirstOrDefault();// dtPOInvoice.Rows.OfType<clsPO_Invoice>().FirstOrDefault();
            //var objInvoice = objPO_Invoice.FirstOrDefault();
            dtAP_Doc = objAP_Doc.GetAll(BranchID, objPO_Invoice.APBatNbr, "%");// app.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objInvoice.APBatNbr).ToList();
            if (dtAP_Doc.Rows.Count > 0)
            {
                IList<clsAP_Doc> ListdtAP_Doc = DataTableHelper.ConvertTo<clsAP_Doc>(dtAP_Doc);
                foreach (clsAP_Doc obj in ListdtAP_Doc)
                {
                    if (obj.Rlsed != -1)
                    {
                        if (objSql.AP_CheckForCancel(BranchID, objPO_Invoice.APBatNbr, obj.RefNbr) == "1")
                        {
                            throw new MessageException(MessageType.Message, "715", "", new[] { obj.RefNbr });
                        }

                    }
                }
            }

            //Check IN
            if (!bCheck714)
            {
                dtIN_Trans = objIN_Trans.GetAll(BranchID, BatNbr, RcptNbr, "%");// app.IN_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == refNbr).ToList();
                IList<clsIN_Trans> ListdtIN_Trans = DataTableHelper.ConvertTo<clsIN_Trans>(dtIN_Trans);
                foreach (clsIN_Trans obj in ListdtIN_Trans)
                {
                    clsIN_ItemSite objIN_ItemSite = new clsIN_ItemSite(Dal);
                    //var objIN_ItemSite = app.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();

                    if (objIN_ItemSite.GetByKey(obj.InvtID, obj.SiteID))
                    {
                        if (objIN_ItemSite.AvgCost != obj.UnitCost)
                        {
                            throw new MessageException(MessageType.Message, "714", "process714", new[] { obj.InvtID, obj.SiteID });

                        }
                    }
                }
            }
            try
            {
                //cancel IN

                if (Receipt_Cancel(BranchID, BatNbr, RcptNbr, false))
                {
                    //cancel PO          
                    if (POReceipt_Cancel(BranchID, BatNbr, RcptNbr))
                    {
                        clsPO_Invoice objPO_Invoice1 = new clsPO_Invoice(Dal);
                        objPO_Invoice1.GetByKey(BranchID, BatNbr, RcptNbr);
                        APProcess.AP ap = new APProcess.AP(User, "PO10200", Dal);
                        if (ap.AP10100_Cancel(BranchID, objPO_Invoice1.APBatNbr, objPO_Invoice1.APRefNbr))
                        {

                            return true;
                        }
                        else return false;
                        ap = null;
                    }
                    else return false;
                }
                else return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private bool Receipt_Cancel(string branchID, string batNbr, string rcptNbr, bool isTransfer)
        {
            try
            {
              
                clsIN_Setup objIN_Setup = new clsIN_Setup(Dal);
                clsIN_Trans objIN_Trans = new clsIN_Trans(Dal);
                clsIN_LotTrans objIN_LotTrans = new clsIN_LotTrans(Dal);
                clsIN_Inventory objInventory = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
                clsSQL objSql = new clsSQL(Dal);
                objIN_Setup.GetByKey(branchID, "IN");

                DataTable trans;// = new List<IN_Trans>();
                trans = objIN_Trans.GetAll(branchID, batNbr, rcptNbr, "%");
                double qty = 0;
                IList<clsIN_Trans> Listtrans = DataTableHelper.ConvertTo<clsIN_Trans>(trans);
                string User = Listtrans.Count == 0 ? "" : Listtrans.FirstOrDefault().LUpd_User;
                string Prog = Listtrans.Count == 0 ? "" : Listtrans.FirstOrDefault().LUpd_Prog;             
               
                foreach (clsIN_Trans inTran in Listtrans)
                {
                    if (!objItemSite.GetByKey(inTran.InvtID, inTran.SiteID))
                    {
                        throw new MessageException(MessageType.Message,"606");
                     
                    }                    
                    objInventory.GetByKey(inTran.InvtID);
                    if (objInventory.StkItem == 1)
                    {
                        if (inTran.UnitMultDiv == "M" || inTran.UnitMultDiv == string.Empty)
                            qty = -1 * inTran.Qty * inTran.InvtMult * inTran.CnvFact;
                        else
                            qty = -1 * (inTran.Qty * inTran.InvtMult) / inTran.CnvFact;
                        if (isTransfer && inTran.TranType == "TR")
                            objItemSite.QtyInTransit -= qty;
                        objItemSite.QtyOnHand = Math.Round(objItemSite.QtyOnHand + qty, 10);
                        objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + qty, 10);
                        objItemSite.AvgCost =  Math.Round(objItemSite.QtyOnHand != 0 ? (objItemSite.TotCost - inTran.ExtCost) / objItemSite.QtyOnHand :
                                        objItemSite.AvgCost, 0);
                    }
                    //Checking Qty
                    if (objInventory.StkItem == 1 && !objIN_Setup.NegQty && Math.Round(objItemSite.QtyOnHand, 0, MidpointRounding.AwayFromZero) < 0)
                    {
                        throw new MessageException(MessageType.Message,"608","", new[] { objItemSite.InvtID, objItemSite.SiteID });   
                      
                    }
                    //if (!objIN_Setup.NegQty && Math.Round(objItemSite.TotCost - inTran.ExtCost, 0) < 0)
                    //{
                    //    log.HasLog = true;
                    //    log.Status = "E";
                    //    log.Clear = true;
                    //    log.Log = "##ProcessMessage:607#@#" + objInventory.InvtID + "#@#" + objItemSite.SiteID + "##";
                    //    return false;
                    //}
                   
                    objItemSite.TotCost = Math.Round(objItemSite.TotCost - inTran.ExtCost, 0);
                    objItemSite.LUpd_DateTime = DateTime.Now;
                    objItemSite.LUpd_Prog = inTran.LUpd_Prog;
                    objItemSite.LUpd_User = inTran.LUpd_User;
                    objItemSite.Update();
                    if (objInventory.StkItem == 1 && objInventory.LotSerTrack != "N")
                    {


                    }
                    //clsIN_ItemCost objIN_ItemCost = new clsIN_ItemCost(Dal);
                    DataTable cost = objSql.IN_ItemCostProcessing_GetCost(inTran.InvtID, inTran.SiteID, inTran.CostID);// app.IN_ItemCostProcessing_GetCost(inTran.InvtID, inTran.SiteID, inTran.CostID).FirstOrDefault();
                    clsIN_ItemCost objItemCost = new clsIN_ItemCost(Dal);                          
                    if (cost.Rows.Count > 0)
                    {
                        clsIN_ItemCost objitem = DataTableHelper.CreateItem<clsIN_ItemCost>(cost.Rows[0]);
                        int CostIdentity = objitem.CostIdentity;
                        if (objItemCost.GetByKey(CostIdentity))
                        {
                            objItemCost.Qty = Math.Round(objItemCost.Qty + qty, 0);
                            objItemCost.TotCost = Math.Round(objItemCost.TotCost - inTran.ExtCost * inTran.InvtMult, 0);
                            objItemCost.Update();
                        }
                    }
                    else
                    {
                        objItemCost.Reset();

                        objItemCost.CostID = inTran.CostID == null ? string.Empty : inTran.CostID;
                        objItemCost.InvtID = inTran.InvtID;
                        objItemCost.Qty = inTran.Qty;
                        objItemCost.RcptDate = inTran.TranDate;
                        objItemCost.RcptNbr = inTran.RefNbr;
                        objItemCost.TotCost = inTran.ExtCost;
                        objItemCost.UnitCost = inTran.UnitCost;
                        objItemCost.SiteID = inTran.SiteID;
                        objItemCost.Crtd_DateTime = DateTime.Now;
                        objItemCost.Crtd_Prog = Prog;
                        objItemCost.Crtd_User = User;
                        objItemCost.LUpd_DateTime = DateTime.Now;
                        objItemCost.LUpd_Prog = Prog;
                        objItemCost.LUpd_User = User;

                        objItemCost.Add();
                    }
                    #region lot
                    if (objInventory.StkItem == 1 && objInventory.LotSerTrack != "N" && objInventory.LotSerTrack.PassNull() != string.Empty)
                    {
                        #region // Them release IN_LotTran 20140908
                        DataTable dtInLotTrans = objIN_LotTrans.GetAll(inTran.BranchID, inTran.BatNbr, inTran.RefNbr, "%", inTran.LineRef);
                        IList<clsIN_LotTrans> ListdtInLotTrans = DataTableHelper.ConvertTo<clsIN_LotTrans>(dtInLotTrans);

                        foreach (var r1 in ListdtInLotTrans)
                        {
                            clsIN_ItemLot objIN_ItemLot = new clsIN_ItemLot(Dal);
                            if (!objIN_ItemLot.GetByKey(inTran.SiteID, inTran.InvtID, r1.LotSerNbr))
                            {
                                objIN_ItemLot.Reset();

                                Insert_IN_ItemLot(ref objIN_ItemLot, inTran.InvtID, inTran.SiteID, r1.LotSerNbr, r1.ExpDate, "", 0, 0);
                                objIN_ItemLot.Crtd_Prog = r1.Crtd_Prog;
                                objIN_ItemLot.Crtd_User = r1.Crtd_User;
                                objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                                objIN_ItemLot.LUpd_Prog = r1.Crtd_Prog;
                                objIN_ItemLot.LUpd_User = r1.LUpd_User;

                            }
                            if (objInventory.StkItem == 1)
                            {
                                if (inTran.UnitMultDiv == "M" || inTran.UnitMultDiv == string.Empty)
                                    qty = -1 * r1.Qty * r1.InvtMult * r1.CnvFact;
                                else
                                    qty = -1 * (r1.Qty * r1.InvtMult) / r1.CnvFact;
                                //if (isTransfer && r1.TranType == "TR")
                                //    objIN_ItemLot.QtyInTransit -= qty;
                                objIN_ItemLot.QtyOnHand = Math.Round(objIN_ItemLot.QtyOnHand + qty, 10);
                                objIN_ItemLot.QtyAvail = Math.Round(objIN_ItemLot.QtyAvail + qty, 10);
                                //objIN_ItemLot.AvgCost = Math.Round((objIN_ItemLot.QtyOnHand) != 0 ? (objIN_ItemLot.Cost) / (objIN_ItemLot.QtyOnHand) : objIN_ItemLot.AvgCost, 0);
                                objIN_ItemLot.Update();
                            }
                        }
                        #endregion

                    }
                    #endregion
                }
                if (Listtrans.Count > 0)
                {
                    objSql.PO_CancelBatchIN(branchID, batNbr, rcptNbr, Prog, User);
                    objSql.IN_CancelBatch(branchID, batNbr, Prog, User);
                }
                //app.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool Issue_Cancel(string branchID, string batNbr, string rcptNbr)
        {
            try
            {
                double qty = 0;

                clsIN_LotTrans objIN_LotTrans = new clsIN_LotTrans(Dal);
                clsSQL objSQL = new clsSQL(Dal);
                clsIN_Setup objIN_Setup = new clsIN_Setup(Dal);
                clsIN_Trans objIN_Trans = new clsIN_Trans(Dal);
                clsIN_Inventory objInventory = new clsIN_Inventory(Dal);
                clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
            
                objIN_Setup.GetByKey(branchID, "IN");

                DataTable trans;
                trans = objIN_Trans.GetAll(branchID, batNbr, rcptNbr, "%");
                IList<clsIN_Trans> Listtrans = DataTableHelper.ConvertTo<clsIN_Trans>(trans);
                string User =Listtrans.Count==0?"": Listtrans.FirstOrDefault().LUpd_User;
                string Prog = Listtrans.Count == 0 ? "" : Listtrans.FirstOrDefault().LUpd_Prog;
             
                foreach (clsIN_Trans inTran in trans.Rows.OfType<clsIN_Trans>())
                {
                    objInventory.GetByKey(inTran.InvtID);
                    if (!objItemSite.GetByKey(inTran.InvtID, inTran.SiteID))
                    {
                        throw new MessageException(MessageType.Message,"606");
                     
                    }
                    if (objInventory.StkItem == 1)
                    {
                        if (inTran.UnitMultDiv == "M" || string.IsNullOrEmpty(inTran.UnitMultDiv))
                            qty = -1 * inTran.Qty * inTran.InvtMult * inTran.CnvFact;
                        else
                            qty = -1 * (inTran.Qty * inTran.InvtMult) / inTran.CnvFact;

                        objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + qty, 10);
                        objItemSite.QtyOnHand = Math.Round(objItemSite.QtyOnHand + qty, 10);
                        objItemSite.AvgCost = Math.Round(objItemSite.QtyOnHand != 0 ?
                            (objItemSite.TotCost - inTran.ExtCost * inTran.InvtMult) / objItemSite.QtyOnHand :
                            objItemSite.AvgCost, 0);

                    }
                    objItemSite.TotCost = Math.Round(objItemSite.TotCost - inTran.ExtCost * inTran.InvtMult, 0);
                    objItemSite.LUpd_DateTime = DateTime.Now;
                    objItemSite.LUpd_Prog = Prog;
                    objItemSite.LUpd_User = User;
                    objItemSite.Update();

                    DataTable cost =objSQL.IN_ItemCostProcessing_GetCost(inTran.InvtID, inTran.SiteID, inTran.CostID);// app.IN_ItemCostProcessing_GetCost(inTran.InvtID, inTran.SiteID, inTran.CostID).FirstOrDefault();
                    clsIN_ItemCost objItemCost = new clsIN_ItemCost(Dal);
                    if (cost.Rows.Count>0)
                    {
                        clsIN_ItemCost objitem = DataTableHelper.CreateItem<clsIN_ItemCost>(cost.Rows[0]);
                        int CostIdentity = objitem.CostIdentity;
                      
                        if (objItemCost.GetByKey(CostIdentity))
                        {
                            objItemCost.Qty = Math.Round(objItemCost.Qty + qty, 0);
                            objItemCost.TotCost = Math.Round(objItemCost.TotCost - inTran.ExtCost * inTran.InvtMult, 0);
                            objItemCost.Update();
                        }

                       
                    }
                    else
                    {
                       objItemCost.Reset();
                       
                            objItemCost.CostID = inTran.CostID == null ? string.Empty : inTran.CostID;
                            objItemCost.InvtID = inTran.InvtID;
                            objItemCost.Qty = inTran.Qty;
                            objItemCost.RcptDate = inTran.TranDate;
                            objItemCost.RcptNbr = inTran.RefNbr;
                            objItemCost.TotCost = inTran.ExtCost;
                            objItemCost.UnitCost = inTran.UnitCost;
                            objItemCost.SiteID = inTran.SiteID;
                            objItemCost.Crtd_DateTime = DateTime.Now;
                            objItemCost.Crtd_Prog = Prog;
                            objItemCost.Crtd_User = User;
                            objItemCost.LUpd_DateTime = DateTime.Now;
                            objItemCost.LUpd_Prog = Prog;
                            objItemCost.LUpd_User = User;

                            objItemCost.Add();
                        //app.IN_ItemCost.AddObject(newItemCost);
                    }
                    #region Lot
                    if (objInventory.StkItem == 1 && objInventory.LotSerTrack != "N" && objInventory.LotSerTrack.PassNull() != string.Empty)
                    {
                        #region // Them release IN_LotTran 20140908
                        DataTable dtInLotTrans = objIN_LotTrans.GetAll(inTran.BranchID, inTran.BatNbr, inTran.RefNbr, "%", inTran.LineRef);
                        IList<clsIN_LotTrans> ListdtInLotTrans = DataTableHelper.ConvertTo<clsIN_LotTrans>(dtInLotTrans);

                        foreach (var r1 in ListdtInLotTrans)
                        {
                            clsIN_ItemLot objIN_ItemLot = new clsIN_ItemLot(Dal);
                            if (!objIN_ItemLot.GetByKey(inTran.SiteID, inTran.InvtID, r1.LotSerNbr))
                            {
                                objIN_ItemLot.Reset();

                                Insert_IN_ItemLot(ref objIN_ItemLot, inTran.InvtID, inTran.SiteID, r1.LotSerNbr, r1.ExpDate, "", 0, 0);
                                objIN_ItemLot.Crtd_Prog = r1.Crtd_Prog;
                                objIN_ItemLot.Crtd_User = r1.Crtd_User;
                                objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                                objIN_ItemLot.LUpd_Prog = r1.Crtd_Prog;
                                objIN_ItemLot.LUpd_User = r1.LUpd_User;
                            }
                            if (objInventory.StkItem == 1)
                            {
                                if (inTran.UnitMultDiv == "M" || inTran.UnitMultDiv == string.Empty)
                                    qty = -1 * r1.Qty * r1.InvtMult * r1.CnvFact;
                                else
                                    qty = -1 * (r1.Qty * r1.InvtMult) / r1.CnvFact;
                                //if (isTransfer && r1.TranType == "TR")
                                //    objIN_ItemLot.QtyInTransit -= qty;
                                objIN_ItemLot.QtyOnHand = Math.Round(objIN_ItemLot.QtyOnHand + qty, 10);
                                objIN_ItemLot.QtyAvail = Math.Round(objIN_ItemLot.QtyAvail + qty, 10);
                                //objIN_ItemLot.AvgCost = Math.Round((objIN_ItemLot.QtyOnHand) != 0 ? (objIN_ItemLot.Cost) / (objIN_ItemLot.QtyOnHand) : objIN_ItemLot.AvgCost, 0);
                                objIN_ItemLot.Update();
                            }
                        }
                        #endregion

                    }
                    #endregion
                }

                if(Listtrans.Count>0)
                objSQL.IN_CancelBatch(branchID, batNbr, Prog, User);
                //app.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        private bool POReceipt_Cancel(string BranchID, string BatNbr, string refNbr)
        {           
            DataTable dt1 ;//= new List<PO_Trans>();
            DataTable dtReceipt;// = new List<PO_Receipt>();           
            try
            {
                clsPO_Trans objPO_Trans = new clsPO_Trans(Dal);
                clsPO_Receipt objPO_Receipt = new clsPO_Receipt(Dal);
                clsIN_Setup objIN_Setup = new clsIN_Setup(Dal);
                clsSQL objSql = new clsSQL(Dal);
                objIN_Setup.GetByKey(BranchID, "IN");
              
                dtReceipt = objPO_Receipt.GetAll(BranchID, BatNbr,refNbr);
                IList<clsPO_Receipt> ListdtReceipt = DataTableHelper.ConvertTo<clsPO_Receipt>(dtReceipt);
                foreach (clsPO_Receipt row in ListdtReceipt)
                {
                    if (row.Rlsed != -1)
                    {
                        dt1 = objPO_Trans.GetAll(BranchID, BatNbr, row.RcptNbr,"%");// app.PO_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RcptNbr == row.RcptNbr).ToList();
                        IList<clsPO_Trans> Listdt1 = DataTableHelper.ConvertTo<clsPO_Trans>(dt1);
                        foreach (clsPO_Trans row1 in Listdt1)
                        {
                            if (row1.PONbr != "")
                                UpdateCancelPODetail( row1);
                        }
                        //IList<clsPO_Receipt> ListdtReceipt = DataTableHelper.ConvertTo<clsPO_Receipt>(dtReceipt);
                        foreach (clsPO_Receipt row1 in ListdtReceipt)
                        {
                            if(row1.PONbr!="")
                            UpdatePOHeader(row1.BranchID, row1.PONbr);
                        }
                    }
                }
                objSql.PO_CancelBatch(BranchID, BatNbr, refNbr, Prog, User);
               
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void UpdateCancelPODetail( clsPO_Trans row)
        {
          
            clsPO_Detail objPO_Detail = new clsPO_Detail(Dal);
            clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
          
            try
            {
              
                if (objPO_Detail.GetByKey(row.BranchID, row.PONbr, row.POLineRef))
                {
                    double qtyTrans = (row.RcptMultDiv == "D" ? row.RcptQty / row.RcptConvFact : row.RcptQty * row.RcptConvFact);//quy ve don vi luu kho 20150326
                    var qtyOrd = objPO_Detail.UnitMultDiv == "M" ? objPO_Detail.QtyOrd * objPO_Detail.CnvFact : objPO_Detail.QtyOrd / objPO_Detail.CnvFact;
                    var qtyRcvd = objPO_Detail.UnitMultDiv == "M" ? objPO_Detail.QtyRcvd * objPO_Detail.CnvFact : objPO_Detail.QtyRcvd / objPO_Detail.CnvFact;
                     //QUY doi ve don vi theo PODetail  20150326
                     double qty = (objPO_Detail.UnitMultDiv == "M" ? qtyTrans / objPO_Detail.CnvFact : qtyTrans * objPO_Detail.CnvFact);

                    if (row.TranType == "R")
                    {
                       
                        objPO_Detail.QtyRcvd = objPO_Detail.QtyRcvd - qty;
                        objPO_Detail.CostReceived = objPO_Detail.CostReceived - row.TranAmt;
                        if ((row.PurchaseType == "GN" | row.PurchaseType == "GI" | row.PurchaseType == "PR"))
                        {
                            double OldQty = 0;
                            //OldQty = (row.RcptMultDiv == "D" ? row.RcptQty / row.RcptConvFact : row.RcptQty * row.RcptConvFact);
                          
                            //var objItemSite = app.IN_ItemSite.Where(p => p.InvtID == row.InvtID && p.SiteID == row.SiteID).FirstOrDefault();

                            if (objItemSite.GetByKey(row.InvtID,row.SiteID))
                            {
                                objItemSite.QtyOnPO = Math.Round(objItemSite.QtyOnPO + qtyTrans, 0);
                                objItemSite.Update();
                            }
                            //POCommon.UpdateOnPOQty(row.InvtID, row.SiteID, 0, OldQty, m_HQSys.DecplQty, m_Dal);
                        }
                        if (objPO_Detail.QtyOrd < objPO_Detail.QtyRcvd)
                        {
                            objPO_Detail.RcptStage = "F";
                        }
                        else if (objPO_Detail.QtyOrd > objPO_Detail.QtyRcvd && objPO_Detail.QtyRcvd > 0)
                        {
                            objPO_Detail.RcptStage = "P";
                        }
                        else if (objPO_Detail.QtyOrd > objPO_Detail.QtyRcvd && objPO_Detail.QtyRcvd == 0)
                        {
                            objPO_Detail.RcptStage = "N";
                        }
                    }
                    else if (row.TranType == "X")
                    {
                        objPO_Detail.QtyRcvd = objPO_Detail.QtyRcvd + qty;
                        objPO_Detail.CostReceived = objPO_Detail.CostReceived + row.TranAmt;

                        objPO_Detail.QtyReturned = objPO_Detail.QtyReturned - qty;
                        objPO_Detail.CostReturned = objPO_Detail.QtyReturned - row.TranAmt;
                        double OldQty = 0;
                        if ((row.PurchaseType == "GN" | row.PurchaseType == "GI" | row.PurchaseType == "PR") & objPO_Detail.RcptStage != "N")
                        {
                            //OldQty = (row.RcptMultDiv == "D" ? row.RcptQty / row.RcptConvFact : row.RcptQty * row.RcptConvFact);
                            
                            if (objItemSite.GetByKey(row.InvtID,row.SiteID))                            
                            {
                                objItemSite.QtyOnPO = Math.Round(objItemSite.QtyOnPO + 0 - qtyTrans, 0);
                                objItemSite.Update();
                            }
                            //POCommon.UpdateOnPOQty(row.InvtID, row.SiteID, OldQty, 0, m_HQSys.DecplQty, m_Dal);
                        }
                        if (objPO_Detail.QtyOrd < objPO_Detail.QtyRcvd)
                        {
                            objPO_Detail.RcptStage = "F";
                        }
                        else if (objPO_Detail.QtyOrd > objPO_Detail.QtyRcvd && objPO_Detail.QtyRcvd > 0)
                        {
                            objPO_Detail.RcptStage = "P";
                        }
                        else if (objPO_Detail.QtyOrd > objPO_Detail.QtyRcvd && objPO_Detail.QtyRcvd == 0)
                        {
                            objPO_Detail.RcptStage = "N";
                        }
                    }
                    objPO_Detail.LUpd_DateTime = DateTime.Now;
                    objPO_Detail.LUpd_Prog = "PO10200";
                    objPO_Detail.LUpd_User = Prog;
                    objPO_Detail.Update();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //private bool AP10100_Cancel(string BranchID, string BatNbr, string RefNbr)
        //{
         
        //    DataTable dtAPDoc;
        //    try
        //    {
        //        clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
        //        clsAP_Balances objAP_Balances = new clsAP_Balances(Dal);
             
        //        if (RefNbr!="%")
        //        {
        //            dtAPDoc = objAP_Doc.GetAll(BranchID, BatNbr, RefNbr);
        //        }
        //        else
        //        {
        //            dtAPDoc = objAP_Doc.GetAll(BranchID, BatNbr, "%");
        //        }
        //        IList<clsAP_Doc> ListdtAPDoc = DataTableHelper.ConvertTo<clsAP_Doc>(dtAPDoc);
        //        foreach (clsAP_Doc dr in ListdtAPDoc)
        //        {
        //            if (dr.Rlsed != -1)
        //            {
        //                ProcessAPBalance(dr.VendID, dr.DocDate, dr.DocType, -dr.OrigDocAmt);
        //            }
        //        }
        //        clsSQL sql = new clsSQL(Dal);
        //        sql.AP_CancelBatch(BranchID, BatNbr, RefNbr, Prog, User);
              
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
       
       

        #endregion

    }
}
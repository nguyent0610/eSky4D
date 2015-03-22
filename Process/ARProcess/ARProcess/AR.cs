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
namespace ARProcess
{
    public class AR
    {
        private static readonly ILog mLogger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Prog { get; set; }
        public string User { get; set; }
        public DataAccess Dal { get; set; }
        public List<MessageException> LogList { get; set; } 
        public AR(string User, string Prog, DataAccess dal)
        {
            this.User = User;
            this.Prog = Prog;
            this.Dal = dal;
        }



        #region AR10100
        public bool AR10100_Cancel(  string BranchID, string BatNbr, string RefNbr, string Handle)
        {
            try
            {
                clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
                clsSQL objSql = new clsSQL(Dal);
     
                IList<clsAR_Doc> dtDocCheck =DataTableHelper.ConvertTo<clsAR_Doc>(objAR_Doc.GetAll(BranchID,BatNbr,RefNbr));              
                if (dtDocCheck.Count > 0)
                {
                    foreach (clsAR_Doc obj in dtDocCheck)
                    {
                        if (obj.Rlsed != -1)
                        {
                            throw new MessageException(MessageType.Message, "715", "", new[] { obj.RefNbr });   
                                        
                        }
                    }
                }
              
                if (AR10100CancelFD(  BranchID, BatNbr, RefNbr))
                {
                    clsBatch newobjBatch = new clsBatch(Dal);
                    clsAR_Doc newobjAR_Doc = new clsAR_Doc(Dal);
                    clsAR_Trans newobjAR_Trans = new clsAR_Trans(Dal);
                    clsBatch oldobjBatch = new clsBatch(Dal);
                    clsAR_Trans objAR_Trans = new clsAR_Trans(Dal);
                    //Copy to new batch
                    if (Handle == "C")
                    {                    
                        string strNewBatNbr = objSql.ARNumbering(BranchID,"BatNbr");
                        string strNewRefNbr = "";
                        oldobjBatch.GetByKey(BranchID,"AR",BatNbr);// = app.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AR").ToList().FirstOrDefault();
                        
                        newobjBatch.Reset();// = new Batch();
                        newobjBatch.BatNbr = strNewBatNbr;
                        newobjBatch.BranchID = BranchID;
                        newobjBatch.Crtd_DateTime = DateTime.Now;
                        newobjBatch.Crtd_Prog = Prog;
                        newobjBatch.Crtd_User = User;
                        newobjBatch.DateEnt = oldobjBatch.DateEnt.Short();
                        newobjBatch.Descr = oldobjBatch.Descr == null ? " " : oldobjBatch.Descr;
                        newobjBatch.EditScrnNbr = oldobjBatch.EditScrnNbr == null ? " " : oldobjBatch.EditScrnNbr;
                        newobjBatch.FromToSiteID = oldobjBatch.FromToSiteID == null ? " " : oldobjBatch.FromToSiteID;
                        newobjBatch.ImpExp = oldobjBatch.ImpExp == null ? " " : oldobjBatch.ImpExp;
                        newobjBatch.IntRefNbr = oldobjBatch.IntRefNbr == null ? " " : oldobjBatch.IntRefNbr;
                        newobjBatch.JrnlType = oldobjBatch.JrnlType == null ? " " : oldobjBatch.JrnlType;
                        newobjBatch.LUpd_DateTime = DateTime.Now;
                        newobjBatch.LUpd_Prog = Prog;
                        newobjBatch.LUpd_User = User;
                        newobjBatch.Module1 = oldobjBatch.Module1 == null ? " " : oldobjBatch.Module1;
                        newobjBatch.NoteID = oldobjBatch.NoteID == null ? 0 : oldobjBatch.NoteID;
                        newobjBatch.OrigBatNbr = oldobjBatch.OrigBatNbr == null ? " " : oldobjBatch.OrigBatNbr;
                        newobjBatch.OrigBranchID = oldobjBatch.OrigBranchID == null ? " " : oldobjBatch.OrigBranchID;
                        newobjBatch.OrigScrnNbr = oldobjBatch.OrigScrnNbr == null ? " " : oldobjBatch.OrigScrnNbr;
                        newobjBatch.ReasonCD = oldobjBatch.ReasonCD == null ? " " : oldobjBatch.ReasonCD;
                        newobjBatch.RefNbr = oldobjBatch.RefNbr == null ? " " : oldobjBatch.RefNbr;
                        newobjBatch.Rlsed = 0;
                        newobjBatch.RvdBatNbr = BatNbr;
                        newobjBatch.Status = "H";
                        newobjBatch.TotAmt = oldobjBatch.TotAmt;
                        newobjBatch.Add();
                        
                   
                        IList<clsAR_Doc> dtDoc =DataTableHelper.ConvertTo<clsAR_Doc>(objAR_Doc.GetAll(BranchID,BatNbr,RefNbr));
                       
                        foreach (clsAR_Doc objAR_Doc1 in dtDoc)
                        {
                            
                            newobjAR_Doc = new clsAR_Doc(Dal);
                            newobjAR_Doc.Reset();                           
                            newobjAR_Doc.BatNbr = strNewBatNbr;
                            newobjAR_Doc.BranchID = BranchID;
                            newobjAR_Doc.Crtd_DateTime = DateTime.Now;
                            newobjAR_Doc.Crtd_Prog = Prog;
                            newobjAR_Doc.Crtd_User = User;
                            newobjAR_Doc.CustId = objAR_Doc1.CustId == null ? "" : objAR_Doc1.CustId;
                            newobjAR_Doc.DeliveryID = objAR_Doc1.DeliveryID == null ? "" : objAR_Doc1.DeliveryID;
                            newobjAR_Doc.DiscBal = objAR_Doc1.DiscBal;
                            newobjAR_Doc.DiscDate = objAR_Doc1.DiscDate;
                            newobjAR_Doc.DocBal = objAR_Doc1.DocBal;
                            newobjAR_Doc.DocDate = objAR_Doc1.DocDate;
                            newobjAR_Doc.DocDesc = objAR_Doc1.DocDesc == null ? "" : objAR_Doc1.DocDesc;
                            newobjAR_Doc.DocType = objAR_Doc1.DocType == null ? "" : objAR_Doc1.DocType;
                            newobjAR_Doc.DueDate = objAR_Doc1.DueDate;
                            newobjAR_Doc.InvcNbr = objAR_Doc1.InvcNbr == null ? "" : objAR_Doc1.InvcNbr;
                            newobjAR_Doc.InvcNote = objAR_Doc1.InvcNote == null ? "" : objAR_Doc1.InvcNote;
                            newobjAR_Doc.LUpd_DateTime = DateTime.Now;
                            newobjAR_Doc.LUpd_Prog = Prog;
                            newobjAR_Doc.LUpd_User = User;
                            newobjAR_Doc.NoteId = objAR_Doc1.NoteId;
                            newobjAR_Doc.OrdNbr = objAR_Doc1.OrdNbr;
                            newobjAR_Doc.OrigDocAmt = objAR_Doc1.OrigDocAmt;
                            newobjAR_Doc.RefNbr = objSql.ARNumbering(BranchID,"RefNbr");
                            newobjAR_Doc.Rlsed = 0;
                            newobjAR_Doc.SlsperId = objAR_Doc1.SlsperId;
                            newobjAR_Doc.TaxId00 = objAR_Doc1.TaxId00;
                            newobjAR_Doc.TaxId01 = objAR_Doc1.TaxId01;
                            newobjAR_Doc.TaxId02 = objAR_Doc1.TaxId02;
                            newobjAR_Doc.TaxId03 = objAR_Doc1.TaxId03;
                            newobjAR_Doc.TaxTot00 = objAR_Doc1.TaxTot00;
                            newobjAR_Doc.TaxTot01 = objAR_Doc1.TaxTot01;
                            newobjAR_Doc.TaxTot02 = objAR_Doc1.TaxTot02;
                            newobjAR_Doc.TaxTot03 = objAR_Doc1.TaxTot03;
                            newobjAR_Doc.Terms = objAR_Doc1.Terms;
                            newobjAR_Doc.TxblTot00 = objAR_Doc1.TxblTot00;
                            newobjAR_Doc.TxblTot01 = objAR_Doc1.TxblTot01;
                            newobjAR_Doc.TxblTot02 = objAR_Doc1.TxblTot02;
                            newobjAR_Doc.TxblTot03 = objAR_Doc1.TxblTot03;

                            newobjAR_Doc.Add();
                           
                            strNewRefNbr = newobjAR_Doc.RefNbr;
                          
                            IList<clsAR_Trans> dtTran = DataTableHelper.ConvertTo<clsAR_Trans>(objAR_Trans.GetAll(BranchID, BatNbr, objAR_Doc1.RefNbr, "%"));// app.AR_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == objAR_Doc1.RefNbr).ToList();
                            foreach (var oldobjAR_Trans in dtTran)
                            {
                               
                                newobjAR_Trans = new clsAR_Trans(Dal);                               
                                newobjAR_Trans.Reset();
                                newobjAR_Trans.BatNbr = strNewBatNbr;
                                newobjAR_Trans.BranchID = BranchID;
                                newobjAR_Trans.Crtd_DateTime = DateTime.Now;
                                newobjAR_Trans.Crtd_Prog = Prog;
                                newobjAR_Trans.Crtd_User = User;
                                newobjAR_Trans.InvcNbr = oldobjAR_Trans.InvcNbr;
                                newobjAR_Trans.InvcNote = oldobjAR_Trans.InvcNote;
                                newobjAR_Trans.InvtId = oldobjAR_Trans.InvtId;
                                newobjAR_Trans.JrnlType = oldobjAR_Trans.JrnlType;
                                newobjAR_Trans.LineRef = oldobjAR_Trans.LineRef;
                                newobjAR_Trans.LineType = oldobjAR_Trans.LineType;
                                newobjAR_Trans.LUpd_DateTime = DateTime.Now;
                                newobjAR_Trans.LUpd_Prog = Prog;
                                newobjAR_Trans.LUpd_User = User;
                                newobjAR_Trans.Qty = oldobjAR_Trans.Qty;
                                newobjAR_Trans.RefNbr = newobjAR_Doc.RefNbr;
                                newobjAR_Trans.TaxAmt00 = oldobjAR_Trans.TaxAmt00;
                                newobjAR_Trans.TaxAmt01 = oldobjAR_Trans.TaxAmt01;
                                newobjAR_Trans.TaxAmt02 = oldobjAR_Trans.TaxAmt02;
                                newobjAR_Trans.TaxAmt03 = oldobjAR_Trans.TaxAmt03;
                                newobjAR_Trans.TaxCat = oldobjAR_Trans.TaxCat;
                                newobjAR_Trans.TaxId00 = oldobjAR_Trans.TaxId00;
                                newobjAR_Trans.TaxId01 = oldobjAR_Trans.TaxId01;
                                newobjAR_Trans.TaxId02 = oldobjAR_Trans.TaxId02;
                                newobjAR_Trans.TaxId03 = oldobjAR_Trans.TaxId03;
                                newobjAR_Trans.TranAmt = oldobjAR_Trans.TranAmt;
                                newobjAR_Trans.TranClass = oldobjAR_Trans.TranClass;
                                newobjAR_Trans.TranDate = oldobjAR_Trans.TranDate;
                                newobjAR_Trans.TranDesc = oldobjAR_Trans.TranDesc;
                                newobjAR_Trans.TranType = oldobjAR_Trans.TranType;
                                newobjAR_Trans.TxblAmt00 = oldobjAR_Trans.TxblAmt00;
                                newobjAR_Trans.TxblAmt01 = oldobjAR_Trans.TxblAmt01;
                                newobjAR_Trans.TxblAmt02 = oldobjAR_Trans.TxblAmt02;
                                newobjAR_Trans.TxblAmt03 = oldobjAR_Trans.TxblAmt03;
                                newobjAR_Trans.UnitPrice = newobjAR_Trans.UnitPrice;
                                newobjAR_Trans.Add();
                              

                            }
                        }
                    }
                                     
                    objSql.AR_CancelBatch(BranchID, BatNbr, RefNbr,Prog,User);
                
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch (Exception ex)
            {
                throw ex;               
            }
            
        }
        public bool AR10100_Release(  string BranchID, string BatNbr)
        {

            try
            {
              
                clsBatch objBatch = new clsBatch(Dal);
                clsSQL objSql = new clsSQL(Dal);
              
                if (objBatch.GetByKey(BranchID, "AR", BatNbr))// them module =AR code 4D tren form ko co
                {
                    if (objBatch.JrnlType.PassNull() != "OM")
                    {

                        objSql.AR_UpdateTranAmtToBeforeTaxAmt(BranchID, BatNbr);
                    }
                }
              
                if (AR10100Release(  BranchID, BatNbr))
                {                   
                    objSql.AR_ReleaseBatch(BranchID, BatNbr, Prog, User);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }

        }
        #region "Public Method AR10100"

        private bool AR10100Release(  string BranchID, string BatNbr)
        {          
            DataTable dtARTrans = new DataTable();
            clsSQL objSql = new clsSQL(Dal);
            clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
            try
            {
             
                DataTable dtARDoc;

                dtARDoc = objAR_Doc.GetAll(BranchID, BatNbr, "%"); 
                IList<clsAR_Doc> ListdtAR_Doc = DataTableHelper.ConvertTo<clsAR_Doc>(dtARDoc);
                foreach (var dr in ListdtAR_Doc)
                {
                    if (dr.DocType == "IN" || dr.DocType == "CM" || dr.DocType == "DM")
                    {
                        ProcessARBalance10100( dr.CustId,BranchID, dr.DocDate, dr.DocType, dr.OrigDocAmt);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;   
            }
        }

        private bool AR10100CancelFD(string BranchID, string BatNbr, string RefNbr)
        {
            IList<clsAR_Doc> dtARDoc;
            clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
            try
            {
                
                dtARDoc = DataTableHelper.ConvertTo<clsAR_Doc>(objAR_Doc.GetAll(BranchID, BatNbr, "%"));
                foreach (clsAR_Doc dr in dtARDoc)
                {
                    if (dr.Rlsed != -1)
                    {
                        if (dr.DocType == "IN" || dr.DocType == "CM" || dr.DocType == "DM")
                        {
                            ProcessARBalance10100( dr.CustId,BranchID, dr.DocDate, dr.DocType, -dr.OrigDocAmt);
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

        private void ProcessARBalance10100(string CustID, string BranchID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
               
                clsAR_Balances objAR_Balances = new clsAR_Balances(Dal);               
                if (objAR_Balances.GetByKey(CustID,BranchID))
                {
                    UpdateARBalance10100(ref objAR_Balances,  CustID, BranchID, DocDate, DocType, OrigDocAmt);
                    objAR_Balances.Update();
                }
                else
                {
                    objAR_Balances.Reset();// = clsApp.ResetAR_Balances(); 
                    UpdateARBalance10100(ref objAR_Balances, CustID,BranchID, DocDate, DocType, OrigDocAmt);
                    objAR_Balances.Crtd_DateTime = DateTime.Now;
                    objAR_Balances.Crtd_Prog = Prog;
                    objAR_Balances.Crtd_User = User;
                    objAR_Balances.Add();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateARBalance10100(ref clsAR_Balances objAR_Balances,  string CustID,string BranchID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                objAR_Balances.CustID = CustID;
                objAR_Balances.BranchID = BranchID;
                objAR_Balances.CurrBal = objAR_Balances.CurrBal + ((DocType == "IN" || DocType == "DM") ? OrigDocAmt : 0);
                objAR_Balances.CurrBal = objAR_Balances.CurrBal + ((DocType == "CM" || DocType == "PP" || DocType == "PA") ? -1 * OrigDocAmt : 0);
                objAR_Balances.LastInvcDate = DocDate.Short();
                objAR_Balances.LUpd_DateTime = DateTime.Now;
                objAR_Balances.LUpd_Prog = Prog;
                objAR_Balances.LUpd_User = User;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        #endregion
        #region AR10200
        #region "Public Method AR10200"
        public bool AR10200_Release(  string BatNbr, string BranchID)
        {
            IList<clsAR_Adjust> dtAR_A;// = new List<AR_Adjust>();
            IList<clsAR_Doc> dtAR_Doc;
            clsBatch objBatch = new clsBatch(Dal);
            clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
            clsAR_Trans objAR_Trans = new clsAR_Trans(Dal);
            clsAR_Adjust objAR_Adjust = new clsAR_Adjust(Dal);
            try
            {
               

                //Update Batch                
                if (objBatch.GetByKey(BranchID, "AR", BatNbr))
                {
                    objBatch.Status = "C";
                    objBatch.Rlsed = 1;
                    objBatch.Update();
                }

                //Update AR_Doc                
                dtAR_Doc = DataTableHelper.ConvertTo<clsAR_Doc>(objAR_Doc.GetAll(BranchID, BatNbr, "%"));
                foreach (clsAR_Doc oldobjAR_Doc in dtAR_Doc)
                {
                    objAR_Doc = new clsAR_Doc(Dal);
                    if (objAR_Doc.GetByKey(oldobjAR_Doc.BranchID, oldobjAR_Doc.BatNbr, oldobjAR_Doc.RefNbr))
                    {
                        objAR_Doc.Rlsed = 1;
                        objAR_Doc.Update();
                    }
                }      
               
                //Update AR_doc chung tu hoa don
                dtAR_A = DataTableHelper.ConvertTo<clsAR_Adjust>(objAR_Adjust.GetAll(BranchID, BatNbr,"%","%"));
                foreach (clsAR_Adjust objAR_A in dtAR_A)
                {                  
                    objAR_Doc = new clsAR_Doc(Dal);
                    objAR_Doc.GetByKey(BranchID,objAR_A.AdjdBatNbr, objAR_A.AdjdRefNbr);
                    objAR_Doc.DocBal = objAR_Doc.DocBal - objAR_A.AdjAmt;
                    objAR_Doc.Update();
                    ProcessARBalance(  objAR_Doc.CustId, objAR_Doc.BranchID, objAR_Doc.DocDate, objAR_Doc.DocType, objAR_A.AdjAmt);
                }
                
                return true;
            }
            catch (Exception ex)
            {              
                throw ex;
            }
        }
        public bool AR10200_Cancel(  string BatNbr, string BranchID)
        {
            IList<clsAR_Adjust> lstdtAR_A;
            IList<clsAR_Doc> lstdtAR_D;
            clsBatch objBatch = new clsBatch(Dal);
            clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
            clsAR_Trans objAR_Trans = new clsAR_Trans(Dal);
            clsAR_Adjust objAR_Adjust = new clsAR_Adjust(Dal);
            try
            {
                //update Batch
                if (objBatch.GetByKey(BranchID, "AR", BatNbr))
                {
                    objBatch.Status = "V";
                    objBatch.Rlsed = -1;
                    objBatch.Update();
                }
                //Update AR_Doc
                lstdtAR_D = DataTableHelper.ConvertTo<clsAR_Doc>(objAR_Doc.GetAll(BranchID, BatNbr, "%"));
                foreach (clsAR_Doc oldobjAR_Doc in lstdtAR_D)
                {
                    objAR_Doc = new clsAR_Doc(Dal);
                    if (objAR_Doc.GetByKey(oldobjAR_Doc.BranchID, oldobjAR_Doc.BatNbr, oldobjAR_Doc.RefNbr))
                    {
                        objAR_Doc.Rlsed = -1;
                        objAR_Doc.Update();
                    }
                }      
               
                //Update AR_doc chung tu hoa don
                lstdtAR_A = DataTableHelper.ConvertTo<clsAR_Adjust>(objAR_Adjust.GetAll(BranchID, BatNbr, "%", "%"));// app.AR_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (clsAR_Adjust objAR_A in lstdtAR_A)
                {
                    if (objAR_A.Reversal != "NS")
                    {
                        //Update AR_Adjust
                        objAR_Adjust = new clsAR_Adjust(Dal);
                        if (objAR_Adjust.GetByKey(objAR_A.BranchID, objAR_A.BatNbr, objAR_A.AdjdRefNbr, objAR_A.AdjgRefNbr))
                        {
                            objAR_Adjust.Reversal = "NS";
                            objAR_Adjust.Update();
                        }
                       //update AR_Doc
                        objAR_Doc = new clsAR_Doc(Dal);
                        objAR_Doc.GetByKey(BranchID, objAR_A.AdjdBatNbr, objAR_A.AdjdRefNbr);
                        objAR_Doc.DocBal = objAR_Doc.DocBal + objAR_A.AdjAmt;
                        objAR_Doc.Update();

                        ProcessARBalance( objAR_Doc.CustId,BranchID,objAR_Doc.DocDate,objAR_Doc.DocType, -1 * objAR_A.AdjAmt);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
              
                throw ex;
            }
        }
        public bool AR10200_Cancel(  string BatNbr, string BranchID, string RefNbr)
        {
            IList<clsAR_Adjust> lstdtAR_A ;
            IList<clsAR_Doc> lstdtAR_D;
            try
            {
                clsAR_Adjust objAR_Adjust = new clsAR_Adjust(Dal);
                clsBatch objBatch = new clsBatch(Dal);
                clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
                //if (RefNbr != "%")
                //{
                //    //lstdtAR_D = app.AR_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                   
                //    //foreach (var objRefNbr in lstdtAR_D)
                //    //{
                //    //    if (RefNbr.Split(',').Contains(objRefNbr.RefNbr))
                //    //    {
                //    //        lstdtAR_D.Remove(objRefNbr);
                //    //    }
                      
                //    //}
                //    //foreach (var objRefNbr in lstdtAR_A)
                //    //{
                //    //    if (RefNbr.Split(',').Contains(objRefNbr.AdjdRefNbr))
                //    //    {
                //    //       lstdtAR_A.Remove(objRefNbr);
                //    //    }

                //    //}
                //}  
                lstdtAR_A = DataTableHelper.ConvertTo<clsAR_Adjust>(objAR_Adjust.GetAll(BranchID, BatNbr, "%", "%"));
        
                foreach (clsAR_Adjust objAR_A  in lstdtAR_A)
                {
                    if (RefNbr.Split(',').Contains(objAR_A.AdjdRefNbr))
                    {
                        //Update AP_Adjust
                        objAR_Adjust=new clsAR_Adjust(Dal);
                        if(objAR_Adjust.GetByKey(objAR_A.BranchID,objAR_A.BatNbr ,objAR_A.AdjdRefNbr, objAR_A.AdjgRefNbr))
                        {
                            
                            objAR_Adjust.Reversal = "NS";
                            objAR_Adjust.Update();
                        }
                        clsAR_Doc objAR_Doc1=new clsAR_Doc(Dal);
                        if(objAR_Doc1.GetByKey(BranchID, objAR_A.AdjdBatNbr ,objAR_A.AdjdRefNbr))
                        {
                         
                            objAR_Doc1.DocBal = objAR_Doc.DocBal + objAR_A.AdjAmt;
                            objAR_Doc1.Update();
                        }


                        //update adj doc
                         clsAR_Doc objAR_Doc2=new clsAR_Doc(Dal);
                         if(objAR_Doc2.GetByKey(BranchID, objAR_A.AdjdBatNbr ,objAR_A.AdjgRefNbr))
                        {
                       
                            objAR_Doc2.OrigDocAmt = objAR_Doc.OrigDocAmt - objAR_A.AdjAmt;
                              objAR_Doc2.Update();
                         }
                        //update batch
                        objBatch=new clsBatch(Dal);
                        if(objBatch.GetByKey(BranchID,"AR",objAR_A.BatNbr))
                        {
                        
                        objBatch.TotAmt = objBatch.TotAmt - objAR_A.AdjAmt;
                            objBatch.Update();
                        }
                        ProcessARBalance(  objAR_A.CustID, BranchID, objAR_Doc1.DocDate, objAR_Doc1.DocType, -1 * objAR_A.AdjAmt);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        #endregion
        #endregion
        #region AR10300
        public bool AR10300Release(  string BranchID, string BatNbr)
        {
            clsSQL objSql = new clsSQL(Dal);
            try
            {
                //this.objProgress.AddMess("Update ARHist, Balance and CASumD " + Constants.vbCrLf);
                if (AR10300_Release(  BranchID, BatNbr))
                {
                    //Insert into GL_trans
                    //Step progress bar
                    objSql.AR_ReleaseBatch(BranchID, BatNbr, Prog, User);
                    return true;
                    //short? msgnbr = 0;
                    ////Step progress bar

                    //if (dt.Count > 0)
                    //{
                    //    msgnbr = dt[0].Column1;
                    //}

                    //if (msgnbr != 9999)
                    //{
                    //    log.HasLog = true;
                    //    log.Status = "E";
                    //    log.Clear = true;
                    //    log.Log = "##ProcessMessage:" + msgnbr + "#@#" + "app.AR_ReleaseBatch" + "##";
                    //    return false;
                    //}
                    //else
                    //{

                    //    return true;
                    //}
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
               
                throw ex;
            }

        }
        #region "Public Method AR10300"

        private bool AR10300_Release(string BranchID, string BatNbr)
        {
            try
            {
                IList<clsAR_Doc> dtARDoc;// = new List<AR_Doc>();
                clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
                dtARDoc = DataTableHelper.ConvertTo<clsAR_Doc>(objAR_Doc.GetAll(BranchID, BatNbr, "%"));
                foreach (clsAR_Doc dr in dtARDoc)
                {
                    //cap nhat so du cong no cua KH
                    ProcessARBalance10300(  dr.CustId,BranchID, dr.DocDate, dr.DocType, dr.OrigDocAmt);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ProcessARBalance10300(  string CustID, string BranchID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                clsAR_Balances objAR_Balances = new clsAR_Balances(Dal);
                
                if (objAR_Balances.GetByKey(CustID, BranchID))
                {
                    UpdateARBalance10300(ref objAR_Balances,  CustID, BranchID, DocDate, DocType, OrigDocAmt);
                    objAR_Balances.Update();
                }
                else
                {
                    objAR_Balances.Reset();// = clsApp.ResetAR_Balances(); 
                    UpdateARBalance10300(ref objAR_Balances,  CustID, BranchID, DocDate, DocType, OrigDocAmt);
                    objAR_Balances.Crtd_DateTime = DateTime.Now;
                    objAR_Balances.Crtd_Prog = Prog;
                    objAR_Balances.Crtd_User = User;
                    objAR_Balances.Add();
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
                
        }

        private void UpdateARBalance10300(ref clsAR_Balances objAR_Balances,  string CustID, string BranchID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                objAR_Balances.CustID = CustID;
                objAR_Balances.BranchID = BranchID;
                objAR_Balances.CurrBal = objAR_Balances.CurrBal + ((DocType == "CM" || DocType == "PP" || DocType == "PA") ? -1 * OrigDocAmt : 0);
                objAR_Balances.LastInvcDate = DocDate.Short();
                objAR_Balances.LUpd_DateTime = DateTime.Now;
                objAR_Balances.LUpd_Prog = Prog;
                objAR_Balances.LUpd_User = User;
            }
            catch (Exception ex)
            {
               
                throw ex;
            }
        }

        #endregion
        #endregion
        #region AR10400      
        #region "Public Method AR10400"
        public bool AR10400_Release(  string BranchID, string BatNbr)
        {
            IList<clsAR_Adjust> dt ;//= new List<AR_Adjust>();
            clsBatch objBatch = new clsBatch(Dal);
            clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
            clsAR_Adjust objAR_Adjust = new clsAR_Adjust(Dal);
            try
            {
                if (objBatch.GetByKey(BranchID, "AR", BatNbr))
                {
                    //var objBatch = app.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr&&p.Module=="AR").ToList().FirstOrDefault();               
                    objBatch.Status = "C";
                    objBatch.Rlsed = 1;
                    objBatch.Update();
                }
                //Update AR_doc chung tu hoa don
                dt = DataTableHelper.ConvertTo<clsAR_Adjust>(objAR_Adjust.GetAll(BranchID, BatNbr, "%", "%"));//; app.AR_Adjust.Where(p=>p.BranchID==BranchID&&p.BatNbr==BatNbr).ToList();
                foreach (clsAR_Adjust objAR_A in dt)
                {
                    //Update Adjusted Document
                    objAR_Doc=new clsAR_Doc(Dal);
                    if(objAR_Doc.GetByKey(BranchID,objAR_A.AdjdBatNbr,objAR_A.AdjdRefNbr))
                    {
                    //var objAR_Doc = app.AR_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objAR_A.AdjdBatNbr && p.RefNbr == objAR_A.AdjdRefNbr).ToList().FirstOrDefault();
                    objAR_Doc.DocBal = objAR_Doc.DocBal - objAR_A.AdjAmt;
                        objAR_Doc.Update();
                    }
                    clsAR_Doc objAR_Doc1 =new clsAR_Doc(Dal);
                    if(objAR_Doc1.GetByKey(BranchID,objAR_A.AdjgBatNbr,objAR_A.AdjgRefNbr))
                    {
                    //app.AR_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objAR_A.AdjgBatNbr && p.RefNbr == objAR_A.AdjgRefNbr).ToList().FirstOrDefault();
                    //Update Adjusting Document                   
                        objAR_Doc1.DocBal = objAR_Doc1.DocBal - objAR_A.AdjAmt; 
                        objAR_Doc1.Update();
                    }
                }               
                return true;
            }
            catch (Exception ex)
            {
               
                return false;
            }
        }
        public bool AR10400_Cancel(  string BranchID, string BatNbr)
        {
            IList<clsAR_Adjust> dt;// = new List<AR_Adjust>();
            try
            {
                clsAR_Adjust objAR_Adjust = new clsAR_Adjust(Dal);
                clsBatch objBatch = new clsBatch(Dal);               

                //Update Batch
               // if(
                //var objBatch = app.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AR").ToList().FirstOrDefault();                             
                objBatch.GetByKey(BranchID, "AR", BatNbr);
                objBatch.Status = "V";
                objBatch.Rlsed = -1;
                objBatch.Update();
                //Update AR_doc chung tu hoa don
                dt = DataTableHelper.ConvertTo<clsAR_Adjust>(objAR_Adjust.GetAll(BranchID ,BatNbr,"%","%"));// app.AR_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (clsAR_Adjust objAR_A in dt)
                {
                    if (objAR_A.Reversal != "NS")
                    {
                        //Update AR_Adjust
                        //var objAR_AD = app.AR_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == objAR_A.BatNbr && p.AdjdRefNbr == objAR_A.AdjdRefNbr && p.AdjgRefNbr == objAR_A.AdjgRefNbr).ToList().FirstOrDefault();
                        clsAR_Adjust objAR_AD = new clsAR_Adjust(Dal);
                        objAR_AD.GetByKey(BranchID ,objAR_A.BatNbr,objAR_A.AdjdRefNbr ,objAR_A.AdjgRefNbr);
                        objAR_AD.Reversal = "NS";
                        objAR_AD.Update();
                        //Update Adjusted Document
                       
                        clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
                        objAR_Doc.GetByKey(BranchID , objAR_A.AdjdBatNbr , objAR_A.AdjdRefNbr);
                        objAR_Doc.DocBal = objAR_Doc.DocBal + objAR_A.AdjAmt;
                        objAR_Doc.Update();
                        //Update Adjusting Document
                        clsAR_Doc objAR_Doc1 = new clsAR_Doc(Dal);
                        objAR_Doc1.GetByKey(BranchID, objAR_A.AdjgBatNbr, objAR_A.AdjgRefNbr);
                        objAR_Doc1.DocBal = objAR_Doc.DocBal + objAR_A.AdjAmt;
                        objAR_Doc1.Update();
                    }
                }               
                return true;
            }
            catch (Exception ex)
            {
               
                return false;
            }
        }
        public bool AR10400_Cancel(  string BranchID, string BatNbr, string RefNbr)
        {
            IList<clsAR_Adjust> dt ;//= new List<AR_Adjust>();
            try
            {
                clsAR_Adjust objAR_Adjust = new clsAR_Adjust(Dal);
              
                //Update AR_doc chung tu hoa don
                dt = DataTableHelper.ConvertTo<clsAR_Adjust>(objAR_Adjust.GetAll(BranchID,BatNbr,"%","%"));// app.AR_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();                  
                foreach (string refNbr in RefNbr.Split(','))
                {
                    var lstobjAR_A = dt.Where(p => p.AdjdRefNbr == refNbr).ToList();
                    foreach(var objAR_A in lstobjAR_A)
                    {
                        //Update AR_Adjust
                        //var objAR_AD = app.AR_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == objAR_A.BatNbr && p.AdjdRefNbr == objAR_A.AdjdRefNbr && p.AdjgRefNbr == objAR_A.AdjgRefNbr).ToList().FirstOrDefault();
                        clsAR_Adjust objAR_AD = new clsAR_Adjust(Dal);
                        objAR_AD.GetByKey(BranchID, objAR_A.BatNbr, objAR_A.AdjdRefNbr, objAR_A.AdjgRefNbr);
                        objAR_AD.Reversal = "NS";
                        objAR_AD.Update();

                        //Update Adjusted Document
                        //var objAR_Doc = app.AR_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objAR_A.AdjdBatNbr && p.RefNbr == objAR_A.AdjdRefNbr).ToList().FirstOrDefault();                                          
                        clsAR_Doc objAR_Doc = new clsAR_Doc(Dal);
                        objAR_Doc.GetByKey(BranchID, objAR_A.AdjdBatNbr, objAR_A.AdjdRefNbr);
                        objAR_Doc.DocBal = objAR_Doc.DocBal + objAR_A.AdjAmt;
                        objAR_Doc.Update();

                        //Update Adjusting Document
                        //var objAR_Doc1 = app.AR_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objAR_A.AdjgBatNbr && p.RefNbr == objAR_A.AdjgRefNbr).ToList().FirstOrDefault();                                                              
                        clsAR_Doc objAR_Doc1 = new clsAR_Doc(Dal);
                        objAR_Doc1.GetByKey(BranchID, objAR_A.AdjgBatNbr, objAR_A.AdjgRefNbr);
                        objAR_Doc1.DocBal = objAR_Doc.DocBal + objAR_A.AdjAmt;
                        objAR_Doc1.Update();
                                      

                        //update batch
                      
                        //var objBatch = app.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == objAR_A.BatNbr && p.Module == "AR").ToList().FirstOrDefault();
                        clsBatch objBatch = new clsBatch(Dal);
                        objBatch.GetByKey(BranchID, "AR", objAR_A.BatNbr);
                        objBatch.TotAmt = objBatch.TotAmt - objAR_A.AdjAmt;
                        objBatch.Update();
                    }
                }
                //dt = app.AR_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Reversal == "").ToList();
                if (dt.Where(p=>p.Reversal.PassNull().Trim()=="").Count() == RefNbr.Split(',').Count())
                {
                    //Update Batch
                    //var objBatch = app.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AR").ToList().FirstOrDefault();
                    clsBatch objBatch = new clsBatch(Dal);
                    objBatch.GetByKey(BranchID, "AR", BatNbr);
                    objBatch.Status = "V";
                    objBatch.Rlsed = -1;
                    objBatch.Update();
                }
               
                return true;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
        #endregion
        #endregion

        private void ProcessARBalance(  string CustID, string BranchID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                clsAR_Balances objAR_Balances = new clsAR_Balances(Dal);
                //var objAR_Balances = app.AR_Balances.Where(p => p.CustID.Trim().ToUpper() == CustID.Trim().ToUpper() && p.BranchID.Trim().ToUpper() == BranchID.Trim().ToUpper()).FirstOrDefault();
                if (objAR_Balances.GetByKey(CustID, BranchID))
                {
                    UpdateARBalance(ref objAR_Balances,  CustID, BranchID, DocDate, DocType, OrigDocAmt);
                    objAR_Balances.Update();
                }
                else
                {
                    objAR_Balances.Reset();// = clsApp.ResetAR_Balances();
                    UpdateARBalance(ref objAR_Balances,  CustID, BranchID, DocDate, DocType, OrigDocAmt);
                    objAR_Balances.Crtd_DateTime = DateTime.Now;
                    objAR_Balances.Crtd_Prog = Prog;
                    objAR_Balances.Crtd_User = User;
                    objAR_Balances.Add();
                }
            }
            catch (Exception ex)
            {
              
                throw ex;
            }

        }

        private void UpdateARBalance(ref clsAR_Balances objAR_Balances, string CustID, string BranchID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                objAR_Balances.CustID = CustID;
                objAR_Balances.BranchID = BranchID;
                objAR_Balances.CurrBal = objAR_Balances.CurrBal + ((DocType == "CM" || DocType == "PP" || DocType == "PA") ? -1 * OrigDocAmt : 0);
                objAR_Balances.LastInvcDate = DocDate.Short();
                objAR_Balances.LUpd_DateTime = DateTime.Now;
                objAR_Balances.LUpd_Prog = Prog;
                objAR_Balances.LUpd_User = User;
            }
            catch (Exception ex)
            {
               
                throw ex;
            }
        }


    }
}
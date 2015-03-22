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
namespace APProcess
{
    public class AP
    {
        private static readonly ILog mLogger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Prog { get; set; }
        public string User { get; set; }
        public DataAccess Dal { get; set; }
        public List<MessageException> LogList { get; set; }
        public AP(string User, string Prog, DataAccess dal)
        {
            this.User = User;
            this.Prog = Prog;
            this.Dal = dal;
        }

        #region AP10100
        public bool AP10100_Cancel( string BranchID, string BatNbr, string RefNbr, string Handle)
        {
            try
            {
                IList<clsAP_Doc> dtDocCheck ;//= new List<AP_Doc>();
                clsSQL objSql = new clsSQL(Dal);
                clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
              
                dtDocCheck =DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID,BatNbr,RefNbr));// dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == RefNbr).ToList();
           
                if (dtDocCheck.Count > 0)
                {
                    foreach (var obj in dtDocCheck)
                    {
                        if (obj.Rlsed != -1)
                        {
                            string dt = objSql.AP_CheckForCancel(BranchID, BatNbr, obj.RefNbr);
                            if (dt != null)
                            {
                                if (dt == "1")
                                {
                                    throw new MessageException(MessageType.Message, "715", "", new[] { obj.RefNbr });   
                                  
                                }
                            }
                        }
                    }
                }
               
                if (AP10100Cancel( BranchID, BatNbr, RefNbr))
                {
                 
                    //Copy to new batch
                    if (Handle == "C")
                    {
                       
                        string strNewBatNbr = objSql.APNumbering(BranchID, "BatNbr");
                        string strNewRefNbr = "";

                        clsBatch objBatch = new clsBatch(Dal);// dal.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AP").ToList().FirstOrDefault();
                        objBatch.GetByKey(BranchID,"AP",BatNbr);

                        clsBatch newobjBatch = new clsBatch(Dal);
                        newobjBatch.Reset();
                        newobjBatch.BatNbr = strNewBatNbr;
                        newobjBatch.BranchID = BranchID;
                        newobjBatch.Crtd_DateTime = DateTime.Now;
                        newobjBatch.Crtd_Prog = Prog;
                        newobjBatch.Crtd_User = User;
                        newobjBatch.DateEnt = objBatch.DateEnt;
                        newobjBatch.Descr = objBatch.Descr == null ? " " : objBatch.Descr;
                        newobjBatch.EditScrnNbr = objBatch.EditScrnNbr == null ? " " : objBatch.EditScrnNbr;
                        newobjBatch.FromToSiteID = objBatch.FromToSiteID == null ? " " : objBatch.FromToSiteID;
                        newobjBatch.ImpExp = objBatch.ImpExp == null ? " " : objBatch.ImpExp;
                        newobjBatch.IntRefNbr = objBatch.IntRefNbr == null ? " " : objBatch.IntRefNbr;
                        newobjBatch.JrnlType = objBatch.JrnlType == null ? " " : objBatch.JrnlType;
                        newobjBatch.LUpd_DateTime = DateTime.Now;
                        newobjBatch.LUpd_Prog = Prog;
                        newobjBatch.LUpd_User = User;
                        newobjBatch.Module1 = objBatch.Module1 == null ? " " : objBatch.Module1;
                        newobjBatch.NoteID = objBatch.NoteID;
                        newobjBatch.OrigBatNbr = objBatch.OrigBatNbr == null ? " " : objBatch.OrigBatNbr;
                        newobjBatch.OrigBranchID = objBatch.OrigBranchID == null ? " " : objBatch.OrigBranchID;
                        newobjBatch.OrigScrnNbr = objBatch.OrigScrnNbr == null ? " " : objBatch.OrigScrnNbr;
                        newobjBatch.ReasonCD = objBatch.ReasonCD == null ? " " : objBatch.ReasonCD;
                        newobjBatch.RefNbr = objBatch.RefNbr == null ? " " : objBatch.RefNbr;
                        newobjBatch.Rlsed = 0;
                        newobjBatch.RvdBatNbr = BatNbr;
                        newobjBatch.Status = "H";
                        newobjBatch.TotAmt = objBatch.TotAmt;
                        newobjBatch.Add();
                        
                        //Me.SetRvdBatNbrText(Me.txtBatNbr.Text)
                        //Me.SetBatNbrText(strNewBatNbr)
                        IList<clsAP_Doc> dtDoc = new List<clsAP_Doc>();
                       
                        //if (RefNbr == "%")
                        //    dtDoc = dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                        //else
                        //    dtDoc = dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == RefNbr).ToList();
                        dtDoc = DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID,BatNbr,RefNbr));
                        foreach (var objAP_Doc1 in dtDoc)
                        {
                            //var objAP_Doc1 = dal.AR_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == dtDoc[i].RefNbr).ToList().FirstOrDefault();
                            clsAP_Doc newobjAP_Doc = new clsAP_Doc(Dal);
                            //objAR_Doc = objAP_Doc1;
                            newobjAP_Doc.BatNbr = strNewBatNbr;
                            newobjAP_Doc.BranchID = BranchID;
                            newobjAP_Doc.Crtd_DateTime = DateTime.Now;
                            newobjAP_Doc.Crtd_Prog = Prog;
                            newobjAP_Doc.Crtd_User = User;                          
                            //newobjAP_Doc.VendID = objAP_Doc1.VendID == null ? "" : objAP_Doc1.VendID;
                            //newobjAP_Doc. = objAP_Doc1.DeliveryID == null ? "" : objAP_Doc1.DeliveryID;
                            newobjAP_Doc.DiscBal = objAP_Doc1.DiscBal;
                            newobjAP_Doc.DiscDate = objAP_Doc1.DiscDate;
                            newobjAP_Doc.DocBal = objAP_Doc1.DocBal;
                            newobjAP_Doc.DocDate = objAP_Doc1.DocDate;
                            newobjAP_Doc.DiscTkn=objAP_Doc1.DiscTkn;
                            newobjAP_Doc.DocDesc = objAP_Doc1.DocDesc == null ? "" : objAP_Doc1.DocDesc;
                            newobjAP_Doc.DocType = objAP_Doc1.DocType == null ? "" : objAP_Doc1.DocType;
                            newobjAP_Doc.DueDate = objAP_Doc1.DueDate;
                            newobjAP_Doc.InvcDate = objAP_Doc1.InvcDate ;
                            newobjAP_Doc.InvcNbr = objAP_Doc1.InvcNbr == null ? "" : objAP_Doc1.InvcNbr;
                            newobjAP_Doc.InvcNote = objAP_Doc1.InvcNote == null ? "" : objAP_Doc1.InvcNote;
                            newobjAP_Doc.LUpd_DateTime = DateTime.Now;
                            newobjAP_Doc.LUpd_Prog = Prog;
                            newobjAP_Doc.LUpd_User = User;                         
                            newobjAP_Doc.NoteID = objAP_Doc1.NoteID;
                            newobjAP_Doc.PONbr = objAP_Doc1.PONbr;
                            newobjAP_Doc.OrigDocAmt = objAP_Doc1.OrigDocAmt;
                            newobjAP_Doc.RcptNbr = objAP_Doc1.RcptNbr;
                            newobjAP_Doc.RefNbr = objSql.APNumbering(BranchID,"RefNbr");
                            newobjAP_Doc.Rlsed = 0;                          
                            //newobjAP_Doc.SlsperId = objAP_Doc1.SlsperId;
                            newobjAP_Doc.TaxId00 = objAP_Doc1.TaxId00;
                            newobjAP_Doc.TaxId01 = objAP_Doc1.TaxId01;
                            newobjAP_Doc.TaxId02 = objAP_Doc1.TaxId02;
                            newobjAP_Doc.TaxId03 = objAP_Doc1.TaxId03;
                            newobjAP_Doc.TaxTot00 = objAP_Doc1.TaxTot00;
                            newobjAP_Doc.TaxTot01 = objAP_Doc1.TaxTot01;
                            newobjAP_Doc.TaxTot02 = objAP_Doc1.TaxTot02;
                            newobjAP_Doc.TaxTot03 = objAP_Doc1.TaxTot03;
                            
                            newobjAP_Doc.Terms = objAP_Doc1.Terms;
                            newobjAP_Doc.TxblTot00 = objAP_Doc1.TxblTot00;
                            newobjAP_Doc.TxblTot01 = objAP_Doc1.TxblTot01;
                            newobjAP_Doc.TxblTot02 = objAP_Doc1.TxblTot02;
                            newobjAP_Doc.TxblTot03 = objAP_Doc1.TxblTot03;
                           
                            newobjAP_Doc.VendID = objAP_Doc1.VendID;
                            newobjAP_Doc.Add();
                            //if (i == 0)
                            //{
                            strNewRefNbr = newobjAP_Doc.RefNbr;
                            //}
                            clsAP_Trans m_objAP_Trans=new clsAP_Trans(Dal);
                            IList<clsAP_Trans> dtTran = DataTableHelper.ConvertTo<clsAP_Trans>(m_objAP_Trans.GetAll(BranchID, BatNbr, objAP_Doc1.RefNbr, "%"));// dal.AP_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == objAP_Doc1.RefNbr).ToList();
                            foreach (var objAP_Trans in dtTran)
                            {
                                //var objAP_Trans = dal.AR_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == objAP_Doc1.RefNbr && p.LineRef == dtTran[j].LineRef).ToList().FirstOrDefault();
                                clsAP_Trans newobjAP_Trans = new clsAP_Trans(Dal);
                                //objAP_Trans = objAP_Trans1;                               
                                newobjAP_Trans.Addr = objAP_Trans.Addr;
                                newobjAP_Trans.BatNbr = strNewBatNbr;
                                newobjAP_Trans.BranchID = BranchID;
                                newobjAP_Trans.Crtd_DateTime = DateTime.Now;
                                newobjAP_Trans.Crtd_Prog = Prog;
                                newobjAP_Trans.Crtd_User = User;
                                newobjAP_Trans.InvcDate = objAP_Trans.InvcDate;
                                newobjAP_Trans.InvcNbr = objAP_Trans.InvcNbr;
                                newobjAP_Trans.InvcNote = objAP_Trans.InvcNote;
                                newobjAP_Trans.InvtID = objAP_Trans.InvtID;
                                newobjAP_Trans.JrnlType = objAP_Trans.JrnlType;
                                newobjAP_Trans.LineRef = objAP_Trans.LineRef;
                                newobjAP_Trans.LineType = objAP_Trans.LineType;
                                newobjAP_Trans.LUpd_DateTime = DateTime.Now;
                                newobjAP_Trans.LUpd_Prog = Prog;                                
                                newobjAP_Trans.LUpd_User = User;
                                newobjAP_Trans.POLineRef = objAP_Trans.POLineRef;
                                newobjAP_Trans.PONbr = objAP_Trans.PONbr;
                                newobjAP_Trans.Qty = objAP_Trans.Qty;
                                newobjAP_Trans.RefNbr = newobjAP_Doc.RefNbr;
                                newobjAP_Trans.TaxAmt00 = objAP_Trans.TaxAmt00;
                                newobjAP_Trans.TaxAmt01 = objAP_Trans.TaxAmt01;
                                newobjAP_Trans.TaxAmt02 = objAP_Trans.TaxAmt02;
                                newobjAP_Trans.TaxAmt03 = objAP_Trans.TaxAmt03;
                                newobjAP_Trans.TaxCat = objAP_Trans.TaxCat;
                                newobjAP_Trans.TaxId00 = objAP_Trans.TaxId00;
                                newobjAP_Trans.TaxId01 = objAP_Trans.TaxId01;                                                                
                                newobjAP_Trans.TaxId02 = objAP_Trans.TaxId02;
                                newobjAP_Trans.TaxId03 = objAP_Trans.TaxId03;
                                newobjAP_Trans.TaxRegNbr = objAP_Trans.TaxRegNbr;
                                newobjAP_Trans.TranAmt = objAP_Trans.TranAmt;
                                newobjAP_Trans.TranClass = objAP_Trans.TranClass;
                                newobjAP_Trans.TranDate = objAP_Trans.TranDate;
                                newobjAP_Trans.TranDesc = objAP_Trans.TranDesc;
                                newobjAP_Trans.TranType = objAP_Trans.TranType;
                                newobjAP_Trans.TxblAmt00 = objAP_Trans.TxblAmt00;
                                newobjAP_Trans.TxblAmt01 = objAP_Trans.TxblAmt01;
                                newobjAP_Trans.TxblAmt02 = objAP_Trans.TxblAmt02;
                                newobjAP_Trans.TxblAmt03 = objAP_Trans.TxblAmt03;
                                newobjAP_Trans.UnitPrice = newobjAP_Trans.UnitPrice;
                                newobjAP_Trans.VendID = objAP_Trans.VendID;
                                newobjAP_Trans.VendName = objAP_Trans.VendName;
                                //newobjAP_Trans.tstamp = new byte[0];
                                newobjAP_Trans.Add();
                                //dal.AP_Trans.AddObject(newobjAP_Trans);


                            }
                        }
                    }
                    //update AP_Doc set rlsed=-1               
                    objSql.AP_CancelBatch(BranchID, BatNbr, RefNbr, Prog, User);
                    //dal.SaveChanges(SaveOptions.DetectChangesBeforeSave);
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
        public bool AP10100_Release( string BranchID, string BatNbr)
        {
            clsSQL objSql = new clsSQL(Dal);
            clsBatch objBatch = new clsBatch(Dal);
            //Update CuryTranAmt = CuryTxblAmt00 to correct Amount in case price has included tax
            //var objBatch=dal.Batches.Where(p=>p.BranchID==BranchID&&p.BatNbr==BatNbr).ToList().FirstOrDefault();
           
            if (objBatch.GetByKey(BranchID,"AP",BatNbr))
            {
                if (objBatch.JrnlType != "PO")
                {
                    objSql.AP_UpdateTranAmtToBeforeTaxAmt(BranchID, BatNbr);               
                }
            }
            //Update APHist, balance, CASumD
            //this.objProgress.AddMess("Update APHist, Balance and CASumD " + Constants.vbCrLf);
       
            if (AP10100Release( BranchID,BatNbr))
            {
                //Insert into GL_trans
                //Step progress bar
                objSql.AP_ReleaseBatch(BranchID, BatNbr, Prog, User);
                //short? msgnbr = 0;
                ////Step progress bar
               
                //if (dt.Count > 0)
                //{
                //   msgnbr = dt[0].Column1;
                //}
           
                //if (msgnbr != 9999)
                //{
                //    log.HasLog = true;
                //    log.Status = "E";
                //    log.Clear = true;
                //    log.Log = "##ProcessMessage:"+msgnbr+"#@#" + "dal.AR_ReleaseBatch" + "##";              
                //    return false;
                //}
                //else
                //{
                   
                //    return true;
                //}
                return true;
            }
            else
            {               
                return false;
            }

        }
        #region "Public Method AP10100"    
        private bool AP10100Release(string BranchID, string BatNbr)
        {
           
            try
            {
                //objAP_Doc = new clsAP_Doc(m_Dal);
                //objAP_Balances = new clsAP_Balances(m_Dal);
                //Get Batch for processing
                IList<clsAP_Doc> dtAPDoc = new List<clsAP_Doc>();
                clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
                dtAPDoc = DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID,BatNbr,"%"));// dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (var dr in dtAPDoc)
                {
                    ProcessAPBalance10100( dr.VendID, dr.DocDate, dr.DocType, dr.OrigDocAmt);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool AP10100Cancel(string BranchID, string BatNbr, string RefNbr)
        {
            IList<clsAP_Doc> dtAPDoc = new List<clsAP_Doc>();
            clsAP_Doc objAP_Doc=new clsAP_Doc(Dal);
            try
            {
                            
                dtAPDoc =DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID, BatNbr, RefNbr));
                foreach (var dr in dtAPDoc)
                {
                    if (dr.Rlsed != -1)
                    {
                        ProcessAPBalance10100( dr.VendID, dr.DocDate, dr.DocType, -dr.OrigDocAmt);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void ProcessAPBalance10100( string VendID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
             try
            {
                clsAP_Balances objAP_Balances = new clsAP_Balances(Dal);
                //var objAP_Balances = dal.AP_Balances.Where(p => p.VendID == VendID).FirstOrDefault();
                if (objAP_Balances.GetByKey(VendID))
                {
                    UpdateAPBalance10100(ref objAP_Balances,  VendID, DocDate, DocType, OrigDocAmt);
                    objAP_Balances.Update();
                }
                else
                {
                    objAP_Balances.Reset();// = clsdal.ResetAP_Balances();
                    UpdateAPBalance10100(ref objAP_Balances, VendID, DocDate, DocType, OrigDocAmt);
                    objAP_Balances.Crtd_DateTime = DateTime.Now;
                    objAP_Balances.Crtd_Prog = Prog;
                    objAP_Balances.Crtd_User = User;
                    objAP_Balances.Add();                   
                }

            }
             catch (Exception ex)
             {
              
                 throw ex;
             }
        }
        private void UpdateAPBalance10100(ref clsAP_Balances objAP_Balances,  string VendID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                objAP_Balances.VendID = VendID;
                objAP_Balances.CurrBal = objAP_Balances.CurrBal + ((DocType == "VO" || DocType == "AC" || DocType == "CR")? OrigDocAmt: 0);
                objAP_Balances.CurrBal = objAP_Balances.CurrBal + ((DocType == "AD" || DocType == "PP")? -1 * OrigDocAmt: 0);
                objAP_Balances.LUpd_DateTime = DateTime.Now;
                objAP_Balances.LUpd_Prog = Prog;
                objAP_Balances.LUpd_User = User;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
        #endregion
        #endregion
        #region AP10200      
        public bool AP10200_Release( string BranchID, string BatNbr)
        {
            clsSQL objSql = new clsSQL(Dal);
            
            if (AP10200Release( BranchID, BatNbr))
            {
               
                objSql.AP_ReleaseBatch(BranchID, BatNbr, Prog, User);
                
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool AP10200_Cancel(string BranchID, string BatNbr, string RefNbr)
        {
            IList<clsAP_Doc> dtAPDoc = new List<clsAP_Doc>();
            clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
            try
            {

                dtAPDoc = DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID, BatNbr, RefNbr));
                foreach (var dr in dtAPDoc)
                {
                    if (dr.Rlsed != -1)
                    {
                        ProcessAPBalance10200(dr.VendID, dr.DocDate, dr.DocType, -dr.OrigDocAmt);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region "Public Method"

        private bool AP10200Release(string BranchID, string BatNbr)
        {
            IList<clsAP_Doc> dtAPDoc = new List<clsAP_Doc>();
            clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
            int i = 0;
            try
            {
              
                //Get Batch for processing
            
                dtAPDoc = DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID,BatNbr,"%"));// dal.AP_Doc.Where(p => p.BranchID.Trim().ToUpper() == BranchID.Trim().ToUpper() && p.BatNbr.Trim().ToUpper() == BatNbr.Trim().ToUpper()).ToList();
                foreach (var dr in dtAPDoc)
                {
                    ProcessAPBalance10200( dr.VendID, dr.DocDate, dr.DocType, dr.OrigDocAmt);
                }
                return true;
            }
            catch (Exception ex)
            {
              
                return false;
            }
        }
       
        private void ProcessAPBalance10200( string VendID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
               
                clsAP_Balances objAP_Balances = new clsAP_Balances(Dal);
                
                if (objAP_Balances.GetByKey(VendID))
                {
                    UpdateAPBalance10200(ref objAP_Balances, VendID, DocDate, DocType, OrigDocAmt);
                    objAP_Balances.Update();
                    
                }
                else
                {
                    objAP_Balances.Reset();// = clsdal.ResetAP_Balances();
                    UpdateAPBalance10200(ref objAP_Balances, VendID, DocDate, DocType, OrigDocAmt);
                    objAP_Balances.Crtd_DateTime = DateTime.Now;
                    objAP_Balances.Crtd_Prog = Prog;
                    objAP_Balances.Crtd_User = User;
                    objAP_Balances.Add();
                   
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
        private void UpdateAPBalance10200(ref clsAP_Balances objAP_Balances,  string VendID, DateTime DocDate, string DocType, double OrigDocAmt)
        {
            try
            {
                objAP_Balances.VendID = VendID;
                objAP_Balances.CurrBal = objAP_Balances.CurrBal + ((DocType == "VO" || DocType == "AC" || DocType == "CR")? OrigDocAmt: 0);
                objAP_Balances.CurrBal = objAP_Balances.CurrBal + ((DocType == "AD" || DocType == "PP")? -1 * OrigDocAmt: 0);

                objAP_Balances.LUpd_DateTime = DateTime.Now;
                objAP_Balances.LUpd_Prog = Prog;
                objAP_Balances.LUpd_User = User;
            }
            catch (Exception ex)
            {
               
                throw ex;
            }

        }

        #endregion



        #endregion
        #region AP10300       
        #region "Public Method AP10300"       
        public bool AP10300_Release( string BranchID, string BatNbr)
        {
            IList<clsAP_Adjust> dt = new List<clsAP_Adjust>();
            try
            {           
                //Update Batch
                clsBatch objBatch = new clsBatch(Dal);
                
                objBatch.GetByKey(BranchID, "AP", BatNbr);
                objBatch.Status = "C";
                objBatch.Rlsed = 1;
                objBatch.Update();
                //Update AP_doc chung tu hoa don
             
                clsAP_Adjust objAP_Adjust = new clsAP_Adjust(Dal);
                dt = DataTableHelper.ConvertTo<clsAP_Adjust>(objAP_Adjust.GetAll(BranchID,BatNbr,"%","%"));
                foreach (var objAP_A in dt)
                {
                    //Update Adjusted Document
                    clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
                   
                    objAP_Doc.GetByKey(BranchID, objAP_A.AdjdBatNbr , objAP_A.AdjdRefNbr);
                    objAP_Doc.DocBal = objAP_Doc.DocBal - objAP_A.AdjAmt;
                    objAP_Doc.Update();

                    clsAP_Doc objAP_Doc1 = new clsAP_Doc(Dal);
                    
                    //Update Adjusting Document   
                    objAP_Doc1.GetByKey(BranchID, objAP_A.AdjgBatNbr, objAP_A.AdjgRefNbr);
                    objAP_Doc1.DocBal = objAP_Doc1.DocBal - objAP_A.AdjAmt;
                    objAP_Doc1.Update();
                }
               
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool AP10300_Cancel( string BranchID, string BatNbr)
        {
            IList<clsAP_Adjust> dt = new List<clsAP_Adjust>();
            clsAP_Adjust objAP_Adjust = new clsAP_Adjust(Dal);
            try
            {              

                //Update Batch
                clsBatch objBatch = new clsBatch(Dal);
               
                objBatch.GetByKey(BranchID,"AP",BatNbr);
                objBatch.Status = "V";
                objBatch.Rlsed = -1;
                objBatch.Update();

                //Update AP_doc chung tu hoa don
                dt = DataTableHelper.ConvertTo<clsAP_Adjust>(objAP_Adjust.GetAll(BranchID,BatNbr,"%","%"));// dal.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (var objAP_A in dt)
                {
                    //Update AP_Adjust  
                    clsAP_Adjust objAP_AD = new clsAP_Adjust(Dal);
                    objAP_AD.GetByKey(BranchID , objAP_A.BatNbr ,objAP_A.AdjdRefNbr , objAP_A.AdjgRefNbr);
                    //var objAP_AD = dal.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == objAP_A.BatNbr && p.AdjdRefNbr == objAP_A.AdjdRefNbr && p.AdjgRefNbr == objAP_A.AdjgRefNbr).ToList().FirstOrDefault();
                    objAP_AD.Reversal = "NS";
                    objAP_AD.Update();

                    //Update Adjusted Document
                    clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
                    objAP_Doc.GetByKey(BranchID, objAP_A.AdjdBatNbr, objAP_A.AdjdRefNbr);
                    //var objAP_Doc = dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objAP_A.AdjdBatNbr && p.RefNbr == objAP_A.AdjdRefNbr).ToList().FirstOrDefault();
                    objAP_Doc.DocBal = objAP_Doc.DocBal + objAP_A.AdjAmt;
                    objAP_Doc.Update();
                    //Update Adjusting Document
                    clsAP_Doc objAP_Doc1 = new clsAP_Doc(Dal);
                    objAP_Doc1.GetByKey(BranchID , objAP_A.AdjgBatNbr, objAP_A.AdjgRefNbr);
                    //var objAP_Doc1 = dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objAP_A.AdjgBatNbr && p.RefNbr == objAP_A.AdjgRefNbr).ToList().FirstOrDefault();
                    objAP_Doc1.DocBal = objAP_Doc1.DocBal + objAP_A.AdjAmt;
                    objAP_Doc1.Update();
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
        #region AP10400 
     
        #region "Public Method AP10400"

        public bool AP10400_Release( string BatNbr, string BranchID)
        {
            IList<clsAP_Adjust> dtAP_A = new List<clsAP_Adjust>();
            clsAP_Adjust objAP_Adjust = new clsAP_Adjust(Dal);
            try
            {
               
                //Update Batch
                clsBatch objBatch = new clsBatch(Dal);
                objBatch.GetByKey(BranchID, "AP", BatNbr);
                //var objBatch = dal.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AP").ToList().FirstOrDefault();
                objBatch.Status = "C";
                objBatch.Rlsed = 1;
                objBatch.Update();

                //Update AP_Doc     
                clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
                IList<clsAP_Doc> dtAP_D = new List<clsAP_Doc>();
              
                dtAP_D =DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID, BatNbr, "%"));//
                foreach (var obj in dtAP_D)
                {
                    objAP_Doc = new clsAP_Doc(Dal);
                    objAP_Doc.GetByKey(BranchID, BatNbr, obj.RefNbr);
                    objAP_Doc.Rlsed = 1;
                    objAP_Doc.Update();
                }         
                //Update AP_doc chung tu hoa don
                dtAP_A = DataTableHelper.ConvertTo<clsAP_Adjust>(objAP_Adjust.GetAll(BranchID,BatNbr,"%","%"));
            
                foreach (var objAP_A in dtAP_A)
                {
                    clsAP_Doc objAP_Doc1 = new clsAP_Doc(Dal);
                   
                    objAP_Doc1.GetByKey(BranchID, objAP_A.AdjdBatNbr, objAP_A.AdjdRefNbr);
                    objAP_Doc1.DocBal = objAP_Doc1.DocBal - objAP_A.AdjAmt;
                    objAP_Doc1.Update();
                    ProcessAPBalance10400( objAP_A.VendID, objAP_A.AdjAmt);
                }
                //dal.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
              
                throw ex;
            }
        }

        public bool AP10400_Cancel( string BatNbr, string BranchID)
        {
            IList<clsAP_Adjust> dtAP_A = new List<clsAP_Adjust>();
            clsAP_Adjust objAP_Adjust = new clsAP_Adjust(Dal);
            try
            {
               
                //Update Batch
                clsBatch objBatch = new clsBatch(Dal);
                //var objBatch = dal.Batches.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AP").ToList().FirstOrDefault();
                objBatch.GetByKey(BranchID,"AP",BatNbr);
                objBatch.Status = "V";
                objBatch.Rlsed = -1;
                objBatch.Update();
                //Update AP_Doc
                
                //var objAP_Doc = dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList().FirstOrDefault();
                clsAP_Doc objAP_Doc = new clsAP_Doc(Dal);
                IList<clsAP_Doc> dtAP_D = new List<clsAP_Doc>();
                dtAP_D = DataTableHelper.ConvertTo<clsAP_Doc>(objAP_Doc.GetAll(BranchID, BatNbr, "%"));//
                foreach (var obj in dtAP_D)
                {
                    objAP_Doc = new clsAP_Doc(Dal);
                    objAP_Doc.GetByKey(BranchID, BatNbr, obj.RefNbr);
                    objAP_Doc.Rlsed = -1;
                    objAP_Doc.Update();
                    
                }         
              
                //Update AP_doc chung tu hoa don

               // dtAP_A = dal.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                 dtAP_A = DataTableHelper.ConvertTo<clsAP_Adjust>(objAP_Adjust.GetAll(BranchID,BatNbr,"%","%"));// dal.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();                
                foreach (var objAP_A in dtAP_A)                
                {
                    //Update AP_Adjust
                    clsAP_Adjust objAP_AD = new clsAP_Adjust(Dal);
                    //var objAP_AD = dal.AP_Adjust.Where(p => p.BranchID == objAP_A.BranchID && p.BatNbr == objAP_A.BatNbr && p.AdjdRefNbr == objAP_A.AdjdRefNbr && p.AdjgRefNbr == objAP_A.AdjgRefNbr).ToList().FirstOrDefault();
                    objAP_AD.GetByKey(objAP_A.BranchID, objAP_A.BatNbr , objAP_A.AdjdRefNbr , objAP_A.AdjgRefNbr);
                    objAP_AD.Reversal = "NS";
                    objAP_AD.Update();

                    clsAP_Doc objAP_Doc1 = new clsAP_Doc(Dal);
                    //var objAP_Doc1 = dal.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == objAP_A.AdjdBatNbr && p.RefNbr == objAP_A.AdjdRefNbr).ToList().FirstOrDefault();
                    objAP_Doc1.GetByKey(BranchID, objAP_A.AdjdBatNbr, objAP_A.AdjdRefNbr);
                    objAP_Doc1.DocBal = objAP_Doc1.DocBal + objAP_A.AdjAmt;
                    objAP_Doc1.Update();

                    ProcessAPBalance10400( objAP_AD.VendID, -1 * objAP_A.AdjAmt);
                }

                return true;
            }
            catch (Exception ex)
            {
             
                throw ex;
            }
        }

        private void ProcessAPBalance10400(string VendID, double OrigDocAmt)
        {
            try
            {
                clsAP_Balances objAP_Balances = new clsAP_Balances(Dal);
                //var objAP_Balances = dal.AP_Balances.Where(p => p.VendID.Trim().ToUpper() == VendID.Trim().ToUpper()).FirstOrDefault();
                if (objAP_Balances.GetByKey(VendID))
                {
                    UpdateAPBalance10400(ref objAP_Balances, VendID, OrigDocAmt);
                    objAP_Balances.Update();
                }
                else
                {
                    objAP_Balances.Reset();// = clsdal.ResetAP_Balances();
                    UpdateAPBalance10400(ref objAP_Balances, VendID, OrigDocAmt);
                    objAP_Balances.LastChkDate = DateTime.Now.Short();
                    objAP_Balances.LastVODate = DateTime.Now.Short();
                    objAP_Balances.Crtd_DateTime = DateTime.Now;
                    objAP_Balances.Crtd_Prog = Prog;                   
                    objAP_Balances.Crtd_User = User;
                    objAP_Balances.Add();
                }
            }
            catch (Exception ex)
            {
               
                throw ex;
            }

        }

        private void UpdateAPBalance10400(ref clsAP_Balances objAP_Balances, string VendID, double OrigDocAmt)
        {
            try
            {
                objAP_Balances.VendID = VendID;
                objAP_Balances.CurrBal = objAP_Balances.CurrBal - OrigDocAmt;

                objAP_Balances.LUpd_DateTime = DateTime.Now;
                objAP_Balances.LUpd_Prog = Prog;
                objAP_Balances.LUpd_User = User;
            }
            catch (Exception ex)
            {
              
                throw ex;
            }

        }

        #endregion
        #endregion

    }
}
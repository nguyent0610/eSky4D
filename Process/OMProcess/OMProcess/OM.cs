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
using INProcess;
using ARProcess;
namespace OMProcess
{
    public class OM 
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Prog { get; set; }
        public string User { get; set; }
        public DataAccess Dal { get; set; }
        public List<MessageException> LogList { get; set; }
        public OM(string User, string prog, DataAccess dal)
        {
            this.User = User;
            this.Prog = prog;
            this.Dal = dal;
        }
        public bool OM10100_PrintInvoice(string branchID, string orderNbr)
        {
            clsOM_Wrk objWrk = new clsOM_Wrk(Dal);
            try
            {
                if(string.IsNullOrEmpty(orderNbr))
                {
                    throw new MessageException(MessageType.Message,"8012","", new[] { orderNbr });
                  
                }

                clsOM_SalesOrd objSalesOrd = new clsOM_SalesOrd(Dal);
                if (!objSalesOrd.GetByKey(branchID, orderNbr))
                {
                    throw new MessageException(MessageType.Message,"8012","", new[] { orderNbr });
                   
                }

                clsOM_OrderType objType = new clsOM_OrderType(Dal);
                if (!objType.GetByKey(objSalesOrd.OrderType))
                {
                    throw new MessageException(MessageType.Message,"8013","", new[] { objSalesOrd.OrderType });
                 
                }

                clsSQL objSql = new clsSQL(Dal);
                string shipperID = objSql.OMNumbering(branchID, "ShipperID", objSalesOrd.OrderType);
             
                if(!objWrk.GetByKey(Environment.MachineName.Trim(),User,branchID,orderNbr))
                {
                    string ARRefNbr = objSql.OMNumbering(branchID, "ARRefNbr", objSalesOrd.OrderType);

                    objWrk.Reset();
                    objWrk.BranchID = branchID;
                    objWrk.InternetAddress = Environment.MachineName.Trim();
                    objWrk.ARRefNbr = ARRefNbr;
                    objWrk.ARDocType = objType.ARDocType;
                    objWrk.INDocType = objType.INDocType;
                    objWrk.ShipperID = shipperID;
                    objWrk.QtyDecPl = 0;
                    objWrk.TranAmtDecPl = 0;
                    objWrk.CuryTranAmtDecPl = 0;
                    objWrk.ProcID = Prog;
                    objWrk.UserID = User;
                    objWrk.OrderNbr = orderNbr;
                    objWrk.Add();
                }
                else
                {
                    throw new MessageException(MessageType.Message,"48","", new[] { orderNbr });
                }

                objSql.OM_NoneShipPrintInvc(Environment.MachineName.Trim(), branchID, objSalesOrd.ARDocDate.Short(), Prog, User);
                objWrk.Delete(objWrk.InternetAddress, objWrk.UserID, objWrk.BranchID, objWrk.OrderNbr);
               
                return true;
            }
            catch (Exception ex)
            {
                objWrk.Delete(objWrk.InternetAddress, objWrk.UserID, objWrk.BranchID, objWrk.OrderNbr);
                throw ex;
            }
        }
        public bool OM10100_InvoiceRelease(string branchID, string orderNbr, string action, DateTime tranDate)
        {

            clsOM_Wrk objWrk = new clsOM_Wrk(Dal);
            try
            {
                clsSQL objSql = new clsSQL(Dal);
                if (string.IsNullOrEmpty(orderNbr))
                {
                    throw new MessageException(MessageType.Message,"8012","", new[] { orderNbr });
                    
                }

                clsOM_SalesOrd objSalesOrd = new clsOM_SalesOrd(Dal);
                if (!objSalesOrd.GetByKey(branchID, orderNbr))
                {
                    throw new MessageException(MessageType.Message,"8012","", new[] { orderNbr });
               
                }

                clsOM_Invoice objInvoice = new clsOM_Invoice(Dal);
                clsOM_OrderType objType = new clsOM_OrderType(Dal);
                if (!objInvoice.GetByKey(branchID, objSalesOrd.ARRefNbr))
                {
                    throw new MessageException(MessageType.Message,"8015","", new[] { objSalesOrd.ARRefNbr });
                  
                }


                if (!objWrk.GetByKey(Environment.MachineName.Trim(), User, branchID, orderNbr))
                {
                    DataTable lstDocDesc = objSql.OM_GetDocDesc(branchID, orderNbr);
                    if (lstDocDesc.Rows.Count == 0)
                    {
                        throw new MessageException(MessageType.Message,"9991","", new[] { "not found doc desc" });
                       
                    }

                    DataRow docDesc = lstDocDesc.Rows[0];
                    if (!objType.GetByKey(objSalesOrd.OrderType))
                    {
                        throw new MessageException(MessageType.Message,"8013","", new[] { objSalesOrd.OrderType });
                      
                    }

                    objWrk.Reset();
                    objWrk.BranchID = branchID;
                    objWrk.InternetAddress = Environment.MachineName.Trim();
                    objWrk.OrderNbr = orderNbr;
                    objWrk.DocDesc = docDesc.String("DocDesc");
                    objWrk.ARRefNbr = objSalesOrd.ARRefNbr;
                    objWrk.SalesType = objType.SalesType;
                    objWrk.ARDocType = objInvoice.DocType;
                    objWrk.INDocType = objInvoice.INDocType;
                    objWrk.CuryTranAmtDecPl = 0;
                    objWrk.QtyDecPl = 0;
                    objWrk.TranAmtDecPl = 0;
                    objWrk.ProcID = Prog;
                    objWrk.UserID = User;
                    objWrk.Add();
                }
                else
                {
                    throw new MessageException(MessageType.Message, "48");
                  
                }

                clsIN_Setup objINSetup = new clsIN_Setup(Dal);
                clsAR_Setup objARSetup = new clsAR_Setup(Dal);
                clsOM_Setup objOMSetup = new clsOM_Setup(Dal);

                if (action == "R")
                {
                    if (!objARSetup.GetByKey(branchID, "AR"))
                    {
                        throw new MessageException(MessageType.Message,"9991","", new[] { "chưa có cấu hình AR" });
                       
                    }

                    if (!objINSetup.GetByKey(branchID, "IN"))
                    {
                       throw new MessageException(MessageType.Message,"9991","", new[] { "chưa có cấu hình IN" });
                     
                    }

                    if (!objOMSetup.GetByKey("OM"))
                    {
                        throw new MessageException(MessageType.Message,"9991","", new[] { "chưa có cấu hình OM" });
                       
                    }


                    DataTable lstReleaseAR = objSql.OM_GetInvoicesReleaseAR(Environment.MachineName.Trim(), branchID, Prog);
                    if (lstReleaseAR.Rows.Count == 0)
                    {
                        goto releaseIN;
                    }
                    foreach (DataRow ar in lstReleaseAR.Rows)
                    {
                        string ARNbr = string.Empty;
                        if (ar.String("ImpExp") == "I")
                            ARNbr = ar.String("OrigBatNbr");
                        else
                            ARNbr = objSql.ARNumbering(branchID, "BatNbr");

                        clsBatch newBatAR = new clsBatch(Dal);
                        newBatAR.BranchID = branchID;
                        newBatAR.BatNbr = ARNbr;
                        newBatAR.OrigBatNbr = ar.String("OrigBatNbr");
                        newBatAR.Module1 = "AR";
                        newBatAR.JrnlType = "OM";
                        newBatAR.DateEnt = tranDate;
                        newBatAR.EditScrnNbr = "AR10100";
                        newBatAR.ImpExp = ar.String("ImpExp");
                        newBatAR.Status = "B";
                        newBatAR.Crtd_DateTime = newBatAR.LUpd_DateTime = DateTime.Now;
                        newBatAR.Crtd_User = newBatAR.LUpd_User = User;
                        newBatAR.Crtd_Prog = newBatAR.LUpd_Prog = Prog;
                        newBatAR.Add();

                        objSql.OM_ReleaseToARDoc(Environment.MachineName.Trim(), branchID, ARNbr, Prog, User);
                        objSql.OM_ReleaseToARTrans(Environment.MachineName.Trim(), branchID, ARNbr, Prog, User);
                        objSql.OM_ReleaseToARFinalUpdate(Environment.MachineName.Trim(), branchID, ARNbr, Prog, User);

                        if (objOMSetup.AutoReleaseAR != 0)
                        {
                            AR arr = new AR(User, Prog, Dal);
                            if (!arr.AR10100_Release(branchID, ARNbr))
                            {
                                return false;
                            }
                            objSql.AR_ReleaseBatch(branchID, ARNbr, Prog, User);
                        }
                    }
                releaseIN:

                    DataTable lstReleaseIN = objSql.OM_GetInvoicesReleaseIN(Environment.MachineName.Trim(), branchID, Prog);
                    if (lstReleaseIN.Rows.Count == 0)
                    {
                        return true;
                    }
                    foreach (DataRow inv in lstReleaseIN.Rows)
                    {
                        string INNbr = string.Empty;
                        if (inv.String("ImpExp") == "I")
                            INNbr = inv.String("OrigINBatNbr");
                        else
                            INNbr = objSql.INNumbering(branchID, "BatNbr");

                        clsBatch newBatIN = new clsBatch(Dal);
                        newBatIN.BranchID = branchID;
                        newBatIN.BatNbr = INNbr;
                        newBatIN.OrigBatNbr = inv.String("OrigINBatNbr");
                        newBatIN.Module1 = "IN";
                        newBatIN.JrnlType = "OM";
                        newBatIN.ImpExp = inv.String("ImpExp");
                        newBatIN.EditScrnNbr = "IN10200";
                        newBatIN.Status = "B";
                        newBatIN.DateEnt = tranDate;
                        newBatIN.Crtd_DateTime = newBatIN.LUpd_DateTime = DateTime.Now;
                        newBatIN.Crtd_Prog = newBatIN.LUpd_Prog = Prog;
                        newBatIN.Crtd_User = newBatIN.LUpd_User = User;
                        newBatIN.Add();

                        objSql.OM_ReleaseToINTrans(Environment.MachineName.Trim(), branchID, INNbr, Prog, User);
                        objSql.OM_ReleaseToINFinalUpdate(Environment.MachineName.Trim(), branchID, INNbr, Prog, User);
                        if (objOMSetup.AutoReleaseIN != 0)
                        {
                            IN inventory = new IN(User, Prog, Dal);
                            inventory.LogList = LogList;
                            if (!inventory.IN10200_Release(branchID, INNbr))
                            {
                                return false;
                            }
                            objSql.IN_ReleaseBatch(branchID, INNbr, Prog, User);
                        }
                    }
                    if (objSalesOrd.OrigOrderNbr != string.Empty)
                    {
                        clsOM_PDASalesOrd pdaOrd = new clsOM_PDASalesOrd(Dal);
                        clsOM_PDASalesOrdDet pdaDet = new clsOM_PDASalesOrdDet(Dal);
                        clsOM_SalesOrdDet det = new clsOM_SalesOrdDet(Dal);
                        if (pdaOrd.GetByKey(branchID, objSalesOrd.OrigOrderNbr))
                        {
                            DataTable lstPDADetail = pdaDet.GetAll(branchID, objSalesOrd.OrigOrderNbr, "%");
                            DataTable lstDetail = objSql.OM_GetSalesDetFromPDA(branchID, objSalesOrd.OrigOrderNbr);

                            int result = 0;
                            foreach (DataRow pdaDetail in lstPDADetail.Rows)
                            {
                                double pdaQty = 0;
                                double qty = 0;
                                foreach (DataRow item in lstPDADetail.Rows)
                                {
                                    if (item.String("InvtID") == pdaDetail.String("InvtID"))
                                    {
                                        if (item.String("UnitMultDiv") == "M")
                                            pdaQty += item.Double("LineQty") * item.Double("UnitRate");
                                        else
                                            pdaQty += item.Double("LineQty") / item.Double("UnitRate");
                                    }
                                }
                                foreach (DataRow item in lstDetail.Rows)
                                {
                                    if (item.String("InvtID") == pdaDetail.String("InvtID"))
                                    {
                                        if (item.String("UnitMultDiv").ToString() == "M")
                                            qty += item.Double("LineQty") * item.Double("UnitRate");
                                        else
                                            qty += item.Double("LineQty") / item.Double("UnitRate");
                                    }
                                }
                                if (qty >= pdaQty)
                                    result++;
                                if (det.GetByKey(branchID, objSalesOrd.OrderNbr, pdaDetail.String("LineRef")))
                                {
                                    pdaDet.GetByKey(branchID, pdaDetail.String("OrderNbr"), pdaDetail.String("LineRef"));
                                    pdaDet.QtyShip += det.LineQty;
                                    pdaDet.Update();
                                }
                            }
                            if (result == lstPDADetail.Rows.Count)
                            {
                                pdaOrd.Status = "C";
                                pdaOrd.Update();
                            }
                        }
                    }
                }
                else if (action == "L")
                {
                    objSql.OM_CancelPrintedInvoices(Environment.MachineName.Trim(), branchID, Prog, User);
                }
                if (!string.IsNullOrEmpty(objWrk.OrderNbr))
                {
                    objWrk.Delete(objWrk.InternetAddress, objWrk.UserID, objWrk.BranchID, objWrk.OrderNbr);
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(objWrk.OrderNbr))
                {
                    objWrk.Delete(objWrk.InternetAddress, objWrk.UserID, objWrk.BranchID, objWrk.OrderNbr);
                }
                throw ex;
            }
            finally
            {
                if (!string.IsNullOrEmpty(objWrk.OrderNbr))
                {
                    objWrk.Delete(objWrk.InternetAddress, objWrk.UserID, objWrk.BranchID, objWrk.OrderNbr);
                }
            }
        }
        public bool OM10100_Cancel(string branchID, string orderNbr, string voidRefNbr)
        {
            try
            {

                clsOM_SalesOrd objSalesOrd = new clsOM_SalesOrd(Dal);
                if (voidRefNbr == null) voidRefNbr = string.Empty;

                clsSQL objSql = new clsSQL(Dal);
                var omCheck = objSql.OM_CheckForCancel(branchID, orderNbr);
                if (omCheck == 1)
                {
                    throw new MessageException(MessageType.Message,"144","", new[] { orderNbr });
             
                }
                else if (omCheck == 2 && !string.IsNullOrEmpty(voidRefNbr))
                {
                    throw new MessageException(MessageType.Message,"145","", new[] { orderNbr });
            
                }

                if (!objSalesOrd.GetByKey(branchID, orderNbr))
                {
                    throw new MessageException(MessageType.Message,"8012","", new[] { orderNbr });
             
                }

                DataTable lstDoc = new DataTable();
                clsAR_Doc objARDoc = new clsAR_Doc(Dal);
                if (string.IsNullOrEmpty(voidRefNbr))
                {
                    lstDoc = objARDoc.GetAll(branchID, objSalesOrd.ARBatNbr, "%");
                }
                else
                {
                    lstDoc = objARDoc.GetAll(branchID, objSalesOrd.ARBatNbr, voidRefNbr);
                }

                foreach (DataRow doc in lstDoc.Rows)
                {
                    if (doc.Short("Rlsed") != -1)
                    {
                        var arCheck = objSql.AR_CheckForCancel(branchID, objSalesOrd.ARBatNbr, doc.String("RefNbr"));
                        if (arCheck == "1")
                        {
                            throw new MessageException(MessageType.Message,"715","", new[] { doc.String("RefNbr") });
                            
                        }
                    }
                }

                DataTable lstTrans = new DataTable();
                clsIN_Trans objTran = new clsIN_Trans(Dal);
                if (string.IsNullOrEmpty(voidRefNbr))
                {
                    lstTrans = objTran.GetAll(branchID, objSalesOrd.INBatNbr, "%", "%");
                }
                else
                {
                    lstTrans = objTran.GetAll(branchID, objSalesOrd.INBatNbr, voidRefNbr, "%");
                }

                clsIN_ItemSite objSite = new clsIN_ItemSite(Dal);
                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                foreach (DataRow tran in lstTrans.Rows)
                {
                    if (!objSite.GetByKey(tran.String("InvtID"), tran.String("SiteID")))
                    {
                        throw new MessageException(MessageType.Message,"9991","", new[] { string.Format("Sản phẩm {0} trong kho {1} không tồn tại", tran.String("InvtID"), tran.String("SiteID")) });
                     
                    }
                    else
                    {
                        if (objSite.AvgCost != tran.Double("UnitCost"))
                        {
                            //if (_process.Parm06.PassNull() == "0")
                            //{
                            //    log.HasLog = true;
                            //    log.Status = "C";
                            //    log.Clear = true;
                            //    log.Log = "##ProcessMessage:9991#@#" +
                            //              string.Format("Sản phẩm {0} trong kho {1} có AvgCost là {2}", tran.String("InvtID"),
                            //                            tran.String("SiteID"), objSite.AvgCost.ToString()) + "##YesNoAvgCost";
                            //    return false;
                            //}
                        }
                    }
                }

                if (!string.IsNullOrEmpty(objSalesOrd.ARBatNbr))
                {
                    AR ar = new AR(User, Prog, Dal);
                    if (ar.AR10100_Cancel(branchID, objSalesOrd.ARBatNbr, voidRefNbr,""))
                    {
                        objSql.AR_CancelBatch(branchID, objSalesOrd.ARBatNbr, string.IsNullOrEmpty(voidRefNbr) ? "%" : voidRefNbr, Prog, User);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message,"9991");
                     
                    }
                }

                IN inv = new IN(User, Prog, Dal);
                inv.LogList = LogList;
                if (!string.IsNullOrEmpty(objSalesOrd.INBatNbr))
                {
                    if (inv.Receipt_Cancel(branchID, objSalesOrd.INBatNbr, voidRefNbr, false, "", false) && inv.Issue_Cancel(branchID, objSalesOrd.INBatNbr, voidRefNbr, false))
                    {
                        objSql.OM_CancelBatchIN(branchID, objSalesOrd.INBatNbr, string.IsNullOrEmpty(voidRefNbr) ? "%" : voidRefNbr, Prog, User);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "9991");
                    }
                }

                objSql.OM_CancelBatch(branchID, objSalesOrd.ARBatNbr, objSalesOrd.INBatNbr, voidRefNbr, Prog, User);
                if (objSalesOrd.OrigOrderNbr != string.Empty)
                {
                    clsOM_PDASalesOrd objPDAOrd = new clsOM_PDASalesOrd(Dal);
                    clsOM_PDASalesOrdDet objPDADet = new clsOM_PDASalesOrdDet(Dal);
                    clsOM_SalesOrdDet objSalesDet = new clsOM_SalesOrdDet(Dal);
                    if (objPDAOrd.GetByKey(branchID, objSalesOrd.OrigOrderNbr))
                    {
                        DataTable lstPDADetail = objPDADet.GetAll(branchID, objSalesOrd.OrigOrderNbr, "%");
                        DataTable lstDetail = objSql.OM_GetSalesDetFromPDA(branchID, objSalesOrd.OrigOrderNbr);
                        int result = 0;
                        foreach (DataRow pdaDetail in lstPDADetail.Rows)
                        {
                            double pdaQty = 0, qty = 0;
                            foreach (DataRow item in lstPDADetail.Rows)
                            {
                                if (item.String("InvtID") == pdaDetail.String("InvtID"))
                                {
                                    if (item.String("UnitMultDiv") == "M")
                                        pdaQty += item.Double("LineQty") * item.Double("UnitRate");
                                    else
                                        pdaQty += item.Double("LineQty") / item.Double("UnitRate");
                                }
                            }
                            foreach (DataRow item in lstDetail.Rows)
                            {
                                if (item.String("InvtID") == pdaDetail.String("InvtID"))
                                {
                                    if (item.String("UnitMultDiv") == "M")
                                        qty += item.Double("LineQty") * item.Double("UnitRate");
                                    else
                                        qty += item.Double("LineQty") / item.Double("UnitRate");
                                }
                            }

                            if (qty >= pdaQty)
                                result++;

                            if (objSalesDet.GetByKey(objSalesOrd.BranchID, objSalesOrd.OrderNbr, pdaDetail.String("LineRef")))
                            {
                                objInvt.GetByKey(pdaDetail.String("InvtID"));
                                clsIN_UnitConversion uomFrom = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, objSalesDet.SlsUnit);
                                clsIN_UnitConversion uomTo = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, pdaDetail.String("SlsUnit"));
                                double rate = 1;
                                double rate2 = 1;

                                if (uomFrom.MultDiv == "M")
                                    rate = uomFrom.CnvFact;
                                else
                                    rate = 1 / uomFrom.CnvFact;

                                if (uomTo.MultDiv == "M")
                                    rate2 = uomTo.CnvFact;
                                else
                                    rate2 = 1 / uomTo.CnvFact;

                                rate = Math.Round(rate / rate2, 2);
                                objPDADet.GetByKey(branchID, objSalesOrd.OrigOrderNbr, pdaDetail.String("LineRef"));
                                objPDADet.QtyShip = objPDADet.QtyShip - objSalesDet.LineQty * rate;
                                objPDADet.Update();
                            }
                        }
                        if (result != lstPDADetail.Rows.Count)
                        {
                            objPDAOrd.Status = "O";
                            objPDAOrd.Update();
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

        public bool OM20500_Release(string branchID, string orderNbr, Dictionary<string, double> dicRef, string deliveryID, DateTime shipDate, DateTime docDate,bool isAddStock, Dictionary<string, double> dicRefLot)
        {

            try
            {
                if (string.IsNullOrEmpty(orderNbr))
                {
                    throw new MessageException(MessageType.Message,"8012","", new[] { orderNbr });
                  
                }

                clsOM_PDASalesOrd objPDAOrd = new clsOM_PDASalesOrd(Dal);
                if (!objPDAOrd.GetByKey(branchID, orderNbr))
                {
                    throw new MessageException(MessageType.Message, "8012", "", new[] { orderNbr });
                 
                }

                clsOM_OrderType objType = new clsOM_OrderType(Dal);
                if (!objType.GetByKey(objPDAOrd.OrderType))
                {
                    throw new MessageException(MessageType.Message, "8013", "", new[] { objPDAOrd.OrderType });
                   
                }

                clsIN_Setup objINSetup = new clsIN_Setup(Dal);
                objINSetup.GetByKey(branchID, "IN");

                clsSQL objSql = new clsSQL(Dal);

                string nbr = objSql.OMNumbering(branchID, "OrderNbr", objType.OrderType);
                clsOM_Setup objOM = new clsOM_Setup(Dal);
                objOM.GetByKey("OM");
                clsOM_SalesOrd objSalesOrd = new clsOM_SalesOrd(Dal);
                if (objPDAOrd.PriceClassID == string.Empty)
                {
                    clsAR_Customer objCust = new clsAR_Customer(Dal);
                    objCust.GetByKey(objPDAOrd.CustID, branchID);
                    objSalesOrd.PriceClassID = objCust.PriceClassID;
                    objSalesOrd.ClassID = objCust.ClassId;
                }
                else
                {
                    objSalesOrd.PriceClassID = objPDAOrd.PriceClassID;
                    objSalesOrd.ClassID = objPDAOrd.ClassID;
                }
                objSalesOrd.IsAddStock = isAddStock;
                objSalesOrd.BranchID = branchID;
                objSalesOrd.OrderNbr = nbr;
                objSalesOrd.DeliveryID = deliveryID;
                objSalesOrd.ShipDate = shipDate;
                objSalesOrd.ARDocDate = docDate;
                objSalesOrd.ARBatNbr = string.Empty;
                objSalesOrd.ARRefNbr = string.Empty;
                objSalesOrd.BudgetID1 = objPDAOrd.BudgetID1;
                objSalesOrd.BudgetID2 = objPDAOrd.BudgetID2;
                objSalesOrd.CmmnPct = objPDAOrd.CmmnPct;
                objSalesOrd.CreditHold = objPDAOrd.CreditHold;
                objSalesOrd.CreditMgrID = objPDAOrd.CreditMgrID;
                objSalesOrd.Crtd_DateTime = DateTime.Now;
                objSalesOrd.Crtd_Prog = Prog;
                objSalesOrd.Crtd_User = User;
                objSalesOrd.CustID = objPDAOrd.CustID;
                objSalesOrd.CustOrderNbr = objPDAOrd.CustOrderNbr;
                objSalesOrd.DoNotCalDisc = objPDAOrd.DoNotCalDisc;
                objSalesOrd.ExpiryDate = objPDAOrd.ExpiryDate;
                objSalesOrd.FreightAllocAmt = objPDAOrd.FreightAllocAmt;
                objSalesOrd.FreightAmt = objPDAOrd.FreightAmt;
                objSalesOrd.FreightCost = objPDAOrd.FreightCost;
                objSalesOrd.FreightTermsID = objPDAOrd.FreightTermsID;
                objSalesOrd.ImpExp = objPDAOrd.ImpExp;
                objSalesOrd.INBatNbr = string.Empty;
                objSalesOrd.INRefNbr = string.Empty;
                objSalesOrd.InvcNbr = string.Empty;
                objSalesOrd.InvcNote = string.Empty;
                objSalesOrd.IssueMethod = objPDAOrd.IssueMethod;
                objSalesOrd.IssueNumber = objPDAOrd.IssueNumber;
                objSalesOrd.LUpd_DateTime = DateTime.Now;
                objSalesOrd.LUpd_Prog = Prog;
                objSalesOrd.LUpd_User = User;
                objSalesOrd.MiscAmt = objPDAOrd.MiscAmt;
                objSalesOrd.NoteId = objPDAOrd.NoteId;
                objSalesOrd.OrderDate = objPDAOrd.OrderDate;

                short ordNo = objSql.OM_GetOrderNo(objPDAOrd.BranchID, objPDAOrd.SlsPerID, objPDAOrd.OrderDate);
                objSalesOrd.OrderNo = (short)(ordNo != 0 ? ordNo + 1 : 0);
                objSalesOrd.OrderType = objPDAOrd.OrderType;
                objSalesOrd.OrderWeight = objPDAOrd.OrderWeight;
                objSalesOrd.OrigOrderNbr = objPDAOrd.OrderNbr;
                objSalesOrd.PaymentBatNbr = objPDAOrd.PaymentBatNbr;
                objSalesOrd.PaymentID = objPDAOrd.PaymentID;
                objSalesOrd.PaymentNbr = objPDAOrd.PaymentNbr;
                objSalesOrd.PmtAmt = objPDAOrd.PmtAmt;
                objSalesOrd.PmtDate = objPDAOrd.PmtDate;
                objSalesOrd.PremFreightAmt = objPDAOrd.PremFreightAmt;
                objSalesOrd.PromiseDate = objPDAOrd.PromiseDate;
                objSalesOrd.ReasonCode = objPDAOrd.ReasonCode;
                objSalesOrd.ReceiptAmt = objPDAOrd.ReceiptAmt;
                objSalesOrd.ReFundAmt = objPDAOrd.ReFundAmt;
                objSalesOrd.ShiftID = objPDAOrd.ShiftID;
                objSalesOrd.ShipPriority = objPDAOrd.ShipPriority;
                objSalesOrd.ShipViaId = objPDAOrd.ShipViaId;
                objSalesOrd.SlsPerID = objPDAOrd.SlsPerID;
                objSalesOrd.StationID = objPDAOrd.StationID;
                objSalesOrd.Status = "N";
                objSalesOrd.Terms = objPDAOrd.Terms;
                objSalesOrd.ToSiteID = objPDAOrd.ToSiteID;
                objSalesOrd.UnitsShipped = objPDAOrd.UnitsShipped;
                objSalesOrd.VolDiscAmt = objPDAOrd.VolDiscAmt;
                objSalesOrd.VolDiscPct = objPDAOrd.VolDiscPct;

                double taxAmt00 = 0;
                double taxAmt01 = 0;
                double taxAmt02 = 0;
                double taxAmt03 = 0;
                double txblAmt00 = 0;
                double txblAmt01 = 0;
                double txblAmt02 = 0;
                double txblAmt03 = 0;
                double soFee = 0;
                double curyLineDiscAmt = 0;
                double ordQty = 0;
                double curyLineAmt = 0;

                string listLineRef = string.Empty;
                foreach (var item in dicRef)
                    listLineRef += item.Key + ",";

                DataTable lstDet = objSql.OM_GetPDASalesOrdDetByLineRef(objPDAOrd.BranchID, objPDAOrd.OrderNbr, listLineRef);
                clsOM_PDASalesOrdDet objPDADet = new clsOM_PDASalesOrdDet(Dal);
                clsOM_SalesOrdDet objSalesDet = new clsOM_SalesOrdDet(Dal);
                clsOM_LotTrans objLot = new clsOM_LotTrans(Dal);
                clsOM_PDAOrdDisc objPDAOrdDisc = new clsOM_PDAOrdDisc(Dal);
                clsOM_OrdAddr objOrdDisc = new clsOM_OrdAddr(Dal);
                foreach (var item in dicRef)
                {
                    if (objPDADet.GetByKey(branchID, orderNbr, item.Key))
                    {
                       
                        double rate = item.Value / objPDADet.LineQty;

                        objSalesDet.Reset();
                        objSalesDet.BarCode = objPDADet.BarCode;
                        objSalesDet.BOCustID = objPDADet.BOCustID;
                        objSalesDet.BOType = objPDADet.BOType;
                        objSalesDet.BranchID = objPDADet.BranchID;
                        objSalesDet.BudgetID1 = objPDADet.BudgetID1;
                        objSalesDet.BudgetID2 = objPDADet.BudgetID2;
                        objSalesDet.CostID = objPDADet.CostID;
                        objSalesDet.Crtd_Datetime = DateTime.Now;
                        objSalesDet.Crtd_Prog = Prog;
                        objSalesDet.Crtd_User = User;
                        objSalesDet.Descr = objPDADet.Descr;
                        objSalesDet.DiscAmt = Math.Round(objPDADet.DiscAmt * rate, 2);
                        objSalesDet.DiscAmt1 = Math.Round(objPDADet.DiscAmt1 * rate, 2);
                        objSalesDet.DiscAmt2 = Math.Round(objPDADet.DiscAmt2 / rate, 2);
                        objSalesDet.DiscCode = objPDADet.DiscCode;
                        objSalesDet.DiscID1 = objPDADet.DiscID1;
                        objSalesDet.DiscID2 = objPDADet.DiscID2;
                        objSalesDet.DiscPct = Math.Round(objPDADet.DiscPct * rate, 2);
                        objSalesDet.DiscPct1 = Math.Round(objPDADet.DiscPct1 * rate, 2);
                        objSalesDet.DiscPct2 = Math.Round(objPDADet.DiscPct2 * rate, 2);
                        objSalesDet.DiscSeq1 = objPDADet.DiscSeq1;
                        objSalesDet.DiscSeq2 = objPDADet.DiscSeq2;
                        objSalesDet.DocDiscAmt = Math.Round(objPDADet.DocDiscAmt / rate, 2);
                        objSalesDet.FreeItem = objPDADet.FreeItem;
                        objSalesDet.FreeItemQty1 = Math.Round(objPDADet.FreeItemQty1 * rate, 2);
                        objSalesDet.FreeItemQty2 = Math.Round(objPDADet.FreeItemQty2 * rate, 2);
                        objSalesDet.GroupDiscAmt1 = Math.Round(objPDADet.GroupDiscAmt1 * rate, 2);
                        objSalesDet.GroupDiscAmt2 = Math.Round(objPDADet.GroupDiscAmt2 * rate, 2);
                        objSalesDet.GroupDiscID1 = objPDADet.GroupDiscID1;
                        objSalesDet.GroupDiscID2 = objPDADet.GroupDiscID2;
                        objSalesDet.GroupDiscPct1 = Math.Round(objPDADet.GroupDiscPct1 * rate, 2);
                        objSalesDet.GroupDiscPct2 = Math.Round(objPDADet.GroupDiscPct2 * rate, 2);
                        objSalesDet.GroupDiscSeq1 = objPDADet.GroupDiscSeq1;
                        objSalesDet.GroupDiscSeq2 = objPDADet.GroupDiscSeq2;
                        objSalesDet.InvtID = objPDADet.InvtID;
                        objSalesDet.ItemPriceClass = objPDADet.ItemPriceClass;
                        objSalesDet.LineAmt = Math.Round(objPDADet.LineAmt * rate, 2);
                        objSalesDet.LineQty = item.Value;
                        objSalesDet.LineRef = item.Key;
                        objSalesDet.LUpd_Datetime = DateTime.Now;
                        objSalesDet.LUpd_Prog = Prog;
                        objSalesDet.LUpd_User = User;
                        objSalesDet.ManuDiscAmt = Math.Round(objPDADet.ManuDiscAmt * rate, 2);
                        objSalesDet.OrderType = objPDAOrd.OrderType;
                        objSalesDet.OrigOrderNbr = objPDAOrd.OrderNbr;
                        objSalesDet.QtyBO = Math.Round(objPDADet.QtyBO * rate, 2);
                        objSalesDet.QtyInvc = Math.Round(objPDADet.QtyInvc * rate, 2);
                        objSalesDet.QtyOpenShip = Math.Round(objPDADet.QtyOpenShip * rate, 2);
                        objSalesDet.QtyShip = Math.Round(objPDADet.QtyShip * rate, 2);
                        objSalesDet.ShipStatus = objPDADet.ShipStatus;
                        objSalesDet.SiteID = objPDADet.SiteID;
                        objSalesDet.SlsPrice = objPDADet.SlsPrice;
                        objSalesDet.SlsUnit = objPDADet.SlsUnit;
                        objSalesDet.SOFee = Math.Round(objPDADet.SOFee * rate, 2);
                        objSalesDet.TaxAmt00 = Math.Round(objPDADet.TaxAmt00 * rate, 2);
                        objSalesDet.TaxAmt01 = Math.Round(objPDADet.TaxAmt01 * rate, 2);
                        objSalesDet.TaxAmt02 = Math.Round(objPDADet.TaxAmt02 * rate, 2);
                        objSalesDet.TaxAmt03 = Math.Round(objPDADet.TaxAmt03 * rate, 2);
                        objSalesDet.TaxCat = objPDADet.TaxCat;
                        objSalesDet.TaxID00 = objPDADet.TaxID00;
                        objSalesDet.TaxID01 = objPDADet.TaxID01;
                        objSalesDet.TaxID02 = objPDADet.TaxID02;
                        objSalesDet.TaxID03 = objPDADet.TaxID03;

                        objSalesDet.TxblAmt00 = Math.Round(objPDADet.TxblAmt00 * rate, 2);
                        objSalesDet.TxblAmt01 = Math.Round(objPDADet.TxblAmt01 * rate, 2);
                        objSalesDet.TxblAmt02 = Math.Round(objPDADet.TxblAmt02 * rate, 2);
                        objSalesDet.TxblAmt03 = Math.Round(objPDADet.TxblAmt03 * rate, 2);
                        objSalesDet.UnitMultDiv = objPDADet.UnitMultDiv;
                        objSalesDet.UnitRate = objPDADet.UnitRate;
                        objSalesDet.UnitWeight = objPDADet.UnitWeight;
                        objSalesDet.OrderNbr = objSalesOrd.OrderNbr;
                        objSalesDet.POSM = objPDADet.POSM;
                        objSalesDet.POSMImg = objPDADet.POSMImg;
                      

                        taxAmt00 += objSalesDet.TaxAmt00;
                        taxAmt01 += objSalesDet.TaxAmt01;
                        taxAmt02 += objSalesDet.TaxAmt02;
                        taxAmt03 += objSalesDet.TaxAmt03;
                        txblAmt00 += objSalesDet.TxblAmt00;
                        txblAmt01 += objSalesDet.TxblAmt01;
                        txblAmt02 += objSalesDet.TxblAmt02;
                        txblAmt03 += objSalesDet.TxblAmt03;
                        soFee += objSalesDet.SOFee;
                        curyLineAmt += objSalesDet.LineAmt;
                        curyLineDiscAmt += objSalesDet.DiscAmt + objSalesDet.ManuDiscAmt;
                        ordQty += objSalesDet.LineQty;

                        objSalesDet.Add();

                        clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                        clsOM_LotTrans objLotTrans = new clsOM_LotTrans(Dal);
                        clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
                        clsIN_ItemLot objItemLot = new clsIN_ItemLot(Dal);
                        objInvt.GetByKey(objPDADet.InvtID);
                        double editQty = 0;
                        double qtyTot = 0;

                        if (objInvt.StkItem == 1 || objPDADet.BOType != "B")
                        {
                            if (objPDADet.UnitMultDiv == "M")
                                editQty = objSalesDet.LineQty * (objSalesDet.UnitRate == 0 ? 1 : objSalesDet.UnitRate);
                            else
                                editQty = objSalesDet.LineQty / (objSalesDet.UnitRate == 0 ? 1 : objSalesDet.UnitRate);

                            qtyTot = editQty + CalculateInvtTotals(objSalesOrd, lstDet, objSalesDet.InvtID, objSalesDet.SiteID, objSalesDet.LineRef);

                            objItemSite.GetByKey(objSalesDet.InvtID, objSalesDet.SiteID);
                            if (!objINSetup.NegQty)
                            {

                                if (objType != null)
                                {
                                    if (objType.INDocType != "CM" && objType.INDocType != "DM" && objType.INDocType != "NA" && objType.INDocType != "RC")
                                    {
                                        if (qtyTot > objItemSite.QtyAvail)
                                        {
                                            throw new MessageException(MessageType.Message,"10431","", new[] { orderNbr, objInvt.InvtID, objPDADet.SiteID });

                                        }
                                    }
                                    else if (objSalesOrd.OrderType != "SR" && objSalesOrd.OrderType != "BL" && objSalesOrd.OrderType != "OC" && objType.INDocType != "CM" && objType.INDocType != "DM" && objType.INDocType != "NA" && objType.INDocType != "RC")
                                    {
                                        if (editQty > objItemSite.QtyAvail)
                                        {
                                            throw new MessageException(MessageType.Message,"10431","", new[] { orderNbr, objInvt.InvtID, objPDADet.SiteID });
                                           
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (objType.INDocType != "CM" && objType.INDocType != "DM" && objType.INDocType != "NA" && objType.INDocType != "RC")
                                {
                                    if (editQty > objItemSite.QtyAvail)
                                    {
                                        throw new MessageException(MessageType.Message,"10431","", new[] { orderNbr, objInvt.InvtID, objPDADet.SiteID });
                                       
                                    }
                                }
                            }
                        }

                        if (objPDADet.DiscAmt > 0 && objPDADet.DiscCode != string.Empty && objSalesDet.BudgetID1 != string.Empty)
                        {
                            double discAmt = Math.Round(objSalesDet.DiscAmt, 0);
                            string discID = objSalesDet.DiscCode;
                            string budGetID = objSalesDet.BudgetID1;
                            string tmp = string.Empty;
                            if (!CheckAvailableDiscBudget(objSalesOrd, branchID, budGetID, discID, tmp, discAmt, false, true, objSalesDet.LineRef, true, true, objSalesDet.InvtID, objSalesDet.SlsUnit))
                            {
                                return false;
                            }
                        }

                        DataTable checkShipQty = objSql.OM20500_ppCheckShipQty(branchID, objPDAOrd.OrderNbr);
                        foreach (DataRow check in checkShipQty.Rows)
                        {
                            if (check.String("InvtID") == objPDADet.InvtID && check.Bool("FreeItem") == objPDADet.FreeItem && objPDADet.LineQty - check.Double("Qty") < objSalesDet.LineQty)
                            {
                                throw new MessageException(MessageType.Message,"10211","", new[] { orderNbr, check.String("InvtID"), (objPDADet.LineQty - check.Double("Qty")).ToString() });
                            }
                        }

                        if (objInvt.StkItem != 0 && objPDADet.BOType != "B" && objType.INDocType != "CM" && objType.INDocType != "DM" && objType.INDocType != "NA" && objType.INDocType != "RC")
                        {
                            if (!UpdateAllocSO(objINSetup, objSalesDet.InvtID, objSalesDet.SiteID, 0, editQty, 0))
                            {
                                return false;
                            }
                        }

                        int rtrn = (objType.ARDocType == "CM" || objType.ARDocType == "CC") ? -1 : 1;
                        clsOM_PPBudget objBudget = new clsOM_PPBudget(Dal);
                        clsOM_Discount objDisc = new clsOM_Discount(Dal);
                        objSql.OM_GetBudgetIDCpny(ref objBudget, branchID, objSalesDet.BudgetID1);
                        objDisc.GetByKey(objSalesDet.DiscID1);

                        if (objType.ARDocType != "NA" && objSalesDet.BOType != "R" && objBudget.Active && objDisc.DiscType == "L")
                        {
                            if (objBudget.ApplyTo == "A")
                            {
                                clsOM_PPAlloc objAlloc = new clsOM_PPAlloc(Dal);
                                if (objAlloc.GetByKey(objBudget.BudgetID, branchID, (objBudget.AllocType == "1" ? objSalesOrd.SlsPerID : objSalesOrd.CustID), "."))
                                {
                                    double spent = objSalesDet.DiscAmt1 * rtrn;
                                    if (objAlloc.QtyAmtAlloc - (objAlloc.QtyAmtSpent + spent) < 0)
                                    {
                                        throw new MessageException(MessageType.Message,"7531","", new[] { orderNbr, objSalesDet.DiscID1, objSalesDet.DiscSeq1 });
                                    }
                                    else
                                    {
                                        objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                        objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                        objAlloc.Update();
                                    }
                                    clsOM_PPCpny objCpny = new clsOM_PPCpny(Dal);
                                    if (objCpny.GetByKey(objBudget.BudgetID, branchID, "."))
                                    {
                                        objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                        objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                        objCpny.Update();
                                    }
                                }
                            }//  if(objBudget.ApplyTo=="A")
                            else
                            {
                                if (objSalesDet.FreeItem)
                                {
                                    clsOM_PPFreeItem objPPInvt = new clsOM_PPFreeItem(Dal);
                                    if (objPPInvt.GetByKey(objBudget.BudgetID, objSalesDet.InvtID))
                                    {

                                        clsIN_UnitConversion uomFrom = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, objSalesDet.SlsUnit);
                                        if (uomFrom != null)
                                        {
                                            clsIN_UnitConversion uomTo = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                            double rate1 = 1;
                                            double rate2 = 1;

                                            if (uomFrom.MultDiv == "M")
                                                rate1 = uomFrom.CnvFact;
                                            else
                                                rate1 = 1 / uomFrom.CnvFact;

                                            if (uomTo.MultDiv == "M")
                                                rate2 = uomTo.CnvFact;
                                            else
                                                rate2 = 1 / uomTo.CnvFact;

                                            rate1 = Math.Round(rate1 / rate2, 2);

                                            clsOM_PPAlloc objAlloc = new clsOM_PPAlloc(Dal);
                                            if (objAlloc.GetByKey(objBudget.BudgetID, branchID, (objBudget.AllocType == "1" ? objSalesOrd.SlsPerID : objSalesOrd.CustID), objSalesDet.InvtID))
                                            {
                                                double spent = objSalesDet.FreeItemQty1 * rate1 * rtrn;
                                                objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent + spent;
                                                objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;
                                                objPPInvt.Update();

                                                clsOM_PPCpny objCpny = new clsOM_PPCpny(Dal);
                                                if (objCpny.GetByKey(objBudget.BudgetID, branchID, objSalesDet.InvtID))
                                                {
                                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                                    objCpny.Update();
                                                }

                                                objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                                objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                                objAlloc.Update();
                                            }

                                        }
                                    }
                                }

                            }
                        }
                        objSql.OM_GetBudgetIDCpny(ref objBudget, branchID, objSalesDet.BudgetID2);
                        objDisc.GetByKey(objSalesDet.DiscID2);
                        if (objType.ARDocType != "NA" && objSalesDet.BOType != "R" && objBudget.Active && objDisc.DiscType == "L")
                        {
                            if (objBudget.ApplyTo == "A")
                            {
                                clsOM_PPAlloc objAlloc = new clsOM_PPAlloc(Dal);
                                if (objAlloc.GetByKey(objBudget.BudgetID, branchID, (objBudget.AllocType == "1" ? objSalesOrd.SlsPerID : objSalesOrd.CustID), "."))
                                {
                                    double spent = objSalesDet.DiscAmt2 * rtrn;
                                    if (objAlloc.QtyAmtAlloc - (objAlloc.QtyAmtSpent + spent) < 0)
                                    {
                                        throw new MessageException(MessageType.Message,"7531","", new[] { orderNbr, objSalesDet.DiscID1, objSalesDet.DiscSeq1 });
                                    }
                                    else
                                    {
                                        objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                        objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                        objAlloc.Update();

                                        clsOM_PPCpny objCpny = new clsOM_PPCpny(Dal);
                                        if (objCpny.GetByKey(objBudget.BudgetID, branchID, "."))
                                        {
                                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                            objCpny.Update();
                                        }
                                    }
                                }
                            }//  if(objBudget.ApplyTo=="A")
                            else
                            {
                                if (objSalesDet.FreeItem)
                                {
                                    clsOM_PPFreeItem objPPInvt = new clsOM_PPFreeItem(Dal);
                                    if (objPPInvt.GetByKey(objBudget.BudgetID, objSalesDet.InvtID))
                                    {
                                        clsIN_UnitConversion uomFrom = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, objSalesDet.SlsUnit);
                                        if (uomFrom != null)
                                        {
                                            clsIN_UnitConversion uomTo = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                            double rate1 = 1;
                                            double rate2 = 1;

                                            if (uomFrom.MultDiv == "M")
                                                rate1 = uomFrom.CnvFact;
                                            else
                                                rate1 = 1 / uomFrom.CnvFact;

                                            if (uomTo.MultDiv == "M")
                                                rate2 = uomTo.CnvFact;
                                            else
                                                rate2 = 1 / uomTo.CnvFact;

                                            rate1 = Math.Round(rate1 / rate2, 2);

                                            clsOM_PPAlloc objAlloc = new clsOM_PPAlloc(Dal);
                                            if (objAlloc.GetByKey(objBudget.BudgetID, branchID, (objBudget.AllocType == "1" ? objSalesOrd.SlsPerID : objSalesOrd.CustID), objSalesDet.InvtID))
                                            {
                                                double spent = (objSalesDet.FreeItemQty2 * rate1) * rtrn;
                                                objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent + spent;
                                                objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;
                                                objPPInvt.Update();

                                                clsOM_PPCpny objCpny = new clsOM_PPCpny(Dal);
                                                if (objCpny.GetByKey(objBudget.BudgetID, branchID, objSalesDet.InvtID))
                                                {
                                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                                    objCpny.Update();
                                                }

                                                objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                                objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                                objAlloc.Update();
                                            }

                                        }
                                    }
                                }

                            }
                        }
                        objSql.OM_GetBudgetIDCpny(ref objBudget, branchID, objSalesDet.BudgetID1);
                        if (objSalesDet.DiscCode != string.Empty && (objSalesDet.FreeItem || objSalesDet.DiscAmt > 0) && objType.ARDocType != "NA" && objSalesDet.BOType != "R" && objBudget != null && objBudget.Active)
                        {
                            if (objBudget.ApplyTo == "A")
                            {
                                clsOM_PPAlloc objAlloc = new clsOM_PPAlloc(Dal);
                                if (objAlloc.GetByKey(objBudget.BudgetID, branchID, (objBudget.AllocType == "1" ? objSalesOrd.SlsPerID : objSalesOrd.CustID), "."))
                                {
                                    double spent = objSalesDet.DiscAmt * rtrn;
                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                    objAlloc.Update();

                                    clsOM_PPCpny objCpny = new clsOM_PPCpny(Dal);
                                    objCpny.GetByKey(objBudget.BudgetID, branchID, ".");
                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                    objCpny.Update();
                                }

                            }// if(objBudget.ApplyTo=="A")
                            else
                            {
                                clsOM_PPFreeItem objPPInvt = new clsOM_PPFreeItem(Dal);
                                if (objPPInvt.GetByKey(objBudget.BudgetID, objSalesDet.InvtID))
                                {
                                    clsIN_UnitConversion uomFrom = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, objSalesDet.SlsUnit);
                                    if (uomFrom != null)
                                    {
                                        clsIN_UnitConversion uomTo = SetUOM(objSalesDet.InvtID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                        double rate1 = 1;
                                        double rate2 = 1;

                                        if (uomFrom.MultDiv == "M")
                                            rate1 = uomFrom.CnvFact;
                                        else
                                            rate1 = 1 / uomFrom.CnvFact;

                                        if (uomTo.MultDiv == "M")
                                            rate2 = uomTo.CnvFact;
                                        else
                                            rate2 = 1 / uomTo.CnvFact;

                                        rate1 = Math.Round(rate1 / rate2, 2);

                                        clsOM_PPAlloc objAlloc = new clsOM_PPAlloc(Dal);
                                        if (objAlloc.GetByKey(objBudget.BudgetID, branchID, (objBudget.AllocType == "1" ? objSalesOrd.SlsPerID : objSalesOrd.CustID), objSalesDet.InvtID))
                                        {
                                            double spent = objSalesDet.LineQty * rate1 * rtrn;
                                            objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent + spent;
                                            objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;
                                            objPPInvt.Update();

                                            objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                            objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                            objAlloc.Update();

                                            clsOM_PPCpny objCpny = new clsOM_PPCpny(Dal);
                                            objCpny.GetByKey(objBudget.BudgetID, branchID, objSalesDet.InvtID);
                                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                            objCpny.Update();
                                        }

                                    }
                                }
                            }
                        }
                        #region truong hop lay tu dong lot
                        if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack.PassNull() != "N" && dicRefLot.Count==0)
                        {
                            DataTable dtItemLot = objSql.OM_GetItemLot(objSalesDet.SiteID, objSalesDet.InvtID);

                            double needQty = objPDADet.UnitMultDiv == "M" ? objSalesDet.LineQty * objSalesDet.UnitRate : objSalesDet.LineQty / objSalesDet.UnitRate;

                            foreach (DataRow lotRow in dtItemLot.Rows)
                            {
                                double newQty = 0;

                                if (objItemLot.GetByKey(lotRow.String("SiteID"), lotRow.String("InvtID"), lotRow.String("LotSerNbr")))
                                {
                                    if (objItemLot.QtyAvail >= needQty)
                                    {
                                        newQty = needQty;
                                        objItemLot.QtyAvail = objItemLot.QtyAvail - needQty;
                                        objItemLot.QtyAllocSO = objItemLot.QtyAllocSO + needQty;

                                        needQty = 0;
                                    }
                                    else
                                    {
                                        newQty = objItemLot.QtyAvail;
                                        needQty -= objItemLot.QtyAvail;
                                        objItemLot.QtyAvail = 0;
                                        objItemLot.QtyAllocSO = objItemLot.QtyAllocSO + newQty;
                                    }
                                    objItemLot.LUpd_DateTime = DateTime.Now;
                                    objItemLot.LUpd_Prog = Prog;
                                    objItemLot.LUpd_User = User;
                                    objItemLot.Update();

                                    if (newQty != 0)
                                    {
                                        objLotTrans.Reset();
                                        objLotTrans.BranchID = objSalesDet.BranchID;
                                        objLotTrans.OrderNbr = objSalesDet.OrderNbr;
                                        objLotTrans.LotSerNbr = lotRow.String("LotSerNbr");
                                        objLotTrans.ExpDate = lotRow.Date("ExpDate");
                                        objLotTrans.MfgrLotSerNbr = lotRow.String("MfgrLotSerNbr");
                                        objLotTrans.WarrantyDate = lotRow.Date("WarrantyDate");
                                        objLotTrans.TranDate = objSalesOrd.OrderDate;
                                        objLotTrans.INDocType = "IN";
                                        objLotTrans.OMLineRef = objSalesDet.LineRef;
                                        objLotTrans.SiteID = objSalesDet.SiteID;
                                        objLotTrans.InvtID = objSalesDet.InvtID;
                                        objLotTrans.InvtMult = -1;

                                        if ((objSalesDet.UnitMultDiv == "M" ? newQty / objSalesDet.UnitRate : newQty * objSalesDet.UnitRate) % 1 > 0)
                                        {
                                            objLotTrans.CnvFact = 1;
                                            objLotTrans.UnitMultDiv = "M";
                                            objLotTrans.Qty = newQty;
                                            objLotTrans.UnitDesc = objInvt.StkUnit;
                                            if (objOM.DfltSalesPrice == "I")
                                            {
                                                double price = Math.Round(objLotTrans.UnitMultDiv == "M" ? objInvt.SOPrice * objLotTrans.CnvFact : objInvt.SOPrice / objLotTrans.CnvFact, 0);
                                                objLotTrans.UnitPrice = price;
                                                objLotTrans.UnitCost = price;
                                            }
                                            else
                                            {
                                                objLotTrans.UnitPrice = Math.Round(objSalesDet.UnitMultDiv == "M" ? objSalesDet.SlsPrice / objSalesDet.UnitRate : objSalesDet.SlsPrice * objSalesDet.UnitRate, 0);
                                                objLotTrans.UnitCost = objLotTrans.UnitPrice;
                                            }

                                        }
                                        else
                                        {
                                            objLotTrans.Qty = Math.Round(objSalesDet.UnitMultDiv == "M" ? newQty / objSalesDet.UnitRate : newQty * objSalesDet.UnitRate, 0);
                                            objLotTrans.CnvFact = objSalesDet.UnitRate;
                                            objLotTrans.UnitMultDiv = objSalesDet.UnitMultDiv;
                                            objLotTrans.UnitPrice = objSalesDet.SlsPrice;
                                            objLotTrans.UnitCost = objSalesDet.SlsPrice;
                                            objLotTrans.UnitDesc = objSalesDet.SlsUnit;
                                        }
                                        objLotTrans.LUpd_DateTime = objLotTrans.Crtd_DateTime = DateTime.Now;
                                        objLotTrans.LUpd_Prog = objLotTrans.Crtd_Prog = Prog;
                                        objLotTrans.LUpd_User = objLotTrans.Crtd_User = User;

                                        objLotTrans.Add();
                                    }
                                }

                                if (needQty == 0) break;
                            }
                        }
                        #endregion
                        else
                        {
                            foreach (var lotRow in dicRefLot.Where(p => p.Key.Contains(objSalesDet.LineRef + "@" + objSalesDet.InvtID)))
                            {
                                double needQty = lotRow.Value;
                                if (objItemLot.GetByKey(objSalesDet.SiteID, lotRow.Key.Split('@')[1], lotRow.Key.Split('@')[2]))
                                {
                                    if (objItemLot.QtyAvail >= needQty)
                                    {
                                        objItemLot.QtyAvail = objItemLot.QtyAvail - needQty;
                                        objItemLot.QtyAllocSO = objItemLot.QtyAllocSO + needQty;
                                        objItemLot.LUpd_DateTime = DateTime.Now;
                                        objItemLot.LUpd_Prog = Prog;
                                        objItemLot.LUpd_User = User;
                                        objItemLot.Update();

                                        objLotTrans.Reset();
                                        objLotTrans.BranchID = objSalesDet.BranchID;
                                        objLotTrans.OrderNbr = objSalesDet.OrderNbr;
                                        objLotTrans.LotSerNbr = objItemLot.LotSerNbr;
                                        objLotTrans.ExpDate = objItemLot.ExpDate;
                                        objLotTrans.MfgrLotSerNbr = objItemLot.MfgrLotSerNbr;
                                        objLotTrans.WarrantyDate = objItemLot.WarrantyDate;
                                        objLotTrans.TranDate = objSalesOrd.OrderDate;
                                        objLotTrans.INDocType = "IN";
                                        objLotTrans.OMLineRef = objSalesDet.LineRef;
                                        objLotTrans.SiteID = objSalesDet.SiteID;
                                        objLotTrans.InvtID = objSalesDet.InvtID;
                                        objLotTrans.InvtMult = -1;
                                        objLotTrans.CnvFact = objSalesDet.UnitRate;
                                        objLotTrans.Qty = lotRow.Value;
                                        objLotTrans.UnitCost = objSalesDet.SlsPrice;
                                        objLotTrans.UnitDesc = objSalesDet.SlsUnit;
                                        objLotTrans.UnitMultDiv = objSalesDet.UnitMultDiv;
                                        objLotTrans.UnitPrice = objSalesDet.SlsPrice;
                                        objLotTrans.Crtd_DateTime = DateTime.Now;
                                        objLotTrans.Crtd_Prog = Prog;
                                        objLotTrans.Crtd_User = User;
                                        objLotTrans.LUpd_DateTime = objLotTrans.Crtd_DateTime = DateTime.Now;
                                        objLotTrans.LUpd_Prog = objLotTrans.Crtd_Prog = Prog;
                                        objLotTrans.LUpd_User = objLotTrans.Crtd_User = User;

                                        objLotTrans.Add();
                                    }
                                    else
                                    {
                                        throw new MessageException(MessageType.Message, "201508181", "", new[] { objSalesDet.OrderNbr, objSalesDet.InvtID, objItemLot.LotSerNbr, needQty.ToString() });
                                    }
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "201508181", "", new[] { objSalesDet.OrderNbr, objSalesDet.InvtID, lotRow.Key.Split('@')[2], needQty.ToString() });
                                }
                            }
                        }
                    }
                }
                objSalesOrd.TaxAmtTot00 = taxAmt00;
                objSalesOrd.TaxAmtTot01 = taxAmt01;
                objSalesOrd.TaxAmtTot02 = taxAmt02;
                objSalesOrd.TaxAmtTot03 = taxAmt03;
                double taxAmt = taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03;
                objSalesOrd.TxblAmtTot00 = txblAmt00;
                objSalesOrd.TxblAmtTot01 = txblAmt01;
                objSalesOrd.TxblAmtTot02 = txblAmt02;
                objSalesOrd.TxblAmtTot03 = txblAmt03;

                objSalesOrd.TaxID00 = objPDAOrd.TaxID00;
                objSalesOrd.TaxID01 = objPDAOrd.TaxID01;
                objSalesOrd.TaxID02 = objPDAOrd.TaxID02;
                objSalesOrd.TaxID03 = objPDAOrd.TaxID03;

                objSalesOrd.SOFeeTot = soFee;
                objSalesOrd.OrdQty = ordQty;
                objSalesOrd.LineAmt = curyLineAmt;
                objSalesOrd.LineDiscAmt = curyLineDiscAmt;
                double txblAmt = 0;
                if (objType.DiscType == "B")
                    txblAmt = curyLineAmt;
                else
                {
                    if (objType.TaxFee)
                        txblAmt = curyLineAmt - taxAmt + soFee * 0.1;
                    else
                        txblAmt = curyLineAmt - taxAmt;
                }
                double curyOrdDiscAmt = 0;

                clsOM_Setup omSetup = new clsOM_Setup(Dal);
                omSetup.GetByKey("OM");
                if (omSetup.InlcSOFeeDisc)
                {

                    if (objType.TaxFee)
                        curyOrdDiscAmt =
                            Math.Round(
                                (objSalesOrd.VolDiscPct *
                                 (curyLineAmt + soFee * 1.1 -
                                  objSalesOrd.VolDiscAmt)) / 100, 0);
                    else
                        curyOrdDiscAmt =
                            Math.Round(
                                (objSalesOrd.VolDiscPct *
                                 (curyLineAmt + soFee -
                                  objSalesOrd.VolDiscAmt)) / 100, 0);

                }
                else
                {
                    curyOrdDiscAmt =
                            Math.Round(
                                (objSalesOrd.VolDiscPct *
                                 (curyLineAmt -
                                  objSalesOrd.VolDiscAmt)) / 100, 0);
                }
                objSalesOrd.OrdAmt = Math.Round(
                        txblAmt + objPDAOrd.FreightAllocAmt +
                        objPDAOrd.MiscAmt + taxAmt +
                        soFee - objSalesOrd.VolDiscAmt -
                        curyOrdDiscAmt, 0);

                objPDAOrd.Status = "O";
                objPDAOrd.Update();

                objSalesOrd.Add();

                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #region Tinh Allocate
        public bool OM41200_Release(string branchID, string allocateID)
        {
            try
            {


                clsSQL objSql = new clsSQL(Dal);
                clsOM_AllocateHeader objHeader = new clsOM_AllocateHeader(Dal);
                clsOM_AllocateSales objSales = new clsOM_AllocateSales(Dal);
                clsOM_AllocateTrans objTrans = new clsOM_AllocateTrans(Dal);
                if (objHeader.GetByKey(branchID, allocateID))
                {
                    var dtAlloc = objSales.GetAll(branchID, allocateID, "%", "%");
                    foreach (DataRow item in dtAlloc.Rows)
                    {
                        objTrans.Reset();
                        objTrans.BranchID = branchID;
                        objTrans.AllocID = allocateID;
                        objTrans.SlsperID = item["SlsperID"].ToString();
                        objTrans.InvtID = item["InvtID"].ToString();
                        objTrans.LUpd_DateTime = DateTime.Now;
                        objTrans.LUpd_Prog = Prog;
                        objTrans.LUpd_User = User;
                        objTrans.Qty = (double)item["Qty"];
                        objTrans.Crtd_DateTime = DateTime.Now;
                        objTrans.Crtd_Prog = Prog;
                        objTrans.Crtd_User = User;
                        objTrans.InvtMul = 1;
                        objTrans.TranDate = DateTime.Now;
                        objTrans.LineRef = "00001";
                        objTrans.Add();

                        clsOM_Allocate alloc = new clsOM_Allocate(Dal);
                        if (alloc.GetByKey(branchID, objTrans.InvtID, objTrans.SlsperID))
                        {
                            alloc.QtyAvail += objTrans.Qty;
                            alloc.Qty += objTrans.Qty;
                            alloc.Update();
                        }
                        else
                        {
                            alloc.BranchID = branchID;
                            alloc.InvtID = objTrans.InvtID;
                            alloc.SlsperID = objTrans.SlsperID;
                            alloc.QtyAvail = objTrans.Qty;
                            alloc.Qty = objTrans.Qty;
                            alloc.LUpd_DateTime = DateTime.Now;
                            alloc.LUpd_Prog = Prog;
                            alloc.LUpd_User = User;
                            alloc.Crtd_DateTime = DateTime.Now;
                            alloc.Crtd_Prog = Prog;
                            alloc.Crtd_User = User;
                            alloc.Add();
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
        public bool OM41200_Cancel(string branchID, string allocateID, string user, string prog)
        {
            try
            {


                clsSQL objSql = new clsSQL(Dal);
                clsOM_AllocateHeader objHeader = new clsOM_AllocateHeader(Dal);
                clsOM_AllocateSales objSales = new clsOM_AllocateSales(Dal);
                clsOM_AllocateTrans objTrans = new clsOM_AllocateTrans(Dal);
                if (objHeader.GetByKey(branchID, allocateID))
                {
                    var dtAlloc = objSales.GetAll(branchID, allocateID, "%", "%");
                    foreach (DataRow item in dtAlloc.Rows)
                    {
                        objTrans.Reset();
                        objTrans.BranchID = branchID;
                        objTrans.AllocID = allocateID;
                        objTrans.SlsperID = item["SlsperID"].ToString();
                        objTrans.InvtID = item["InvtID"].ToString();
                        objTrans.LUpd_DateTime = DateTime.Now;
                        objTrans.LUpd_Prog = prog;
                        objTrans.LUpd_User = user;
                        objTrans.Qty = (double)item["Qty"];
                        objTrans.Crtd_DateTime = DateTime.Now;
                        objTrans.Crtd_Prog = prog;
                        objTrans.Crtd_User = user;
                        objTrans.InvtMul = -1;
                        objTrans.TranDate = DateTime.Now;
                        objTrans.LineRef = "00002";
                        objTrans.Add();

                        clsOM_Allocate alloc = new clsOM_Allocate(Dal);
                        if (alloc.GetByKey(branchID, objTrans.InvtID, objTrans.SlsperID))
                        {
                            alloc.QtyAvail -= objTrans.Qty;
                            alloc.Qty -= objTrans.Qty;
                            alloc.Update();
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
        #endregion


        //private bool AR10100Cancel(string branchID, string batNbr, string refNbr)
        //{
        //    try
        //    {
        //        clsAR_Doc objARDoc = new clsAR_Doc(Dal);
        //        DataTable lstDoc = new DataTable();
        //        if (string.IsNullOrEmpty(refNbr))
        //            lstDoc = objARDoc.GetAll(branchID, batNbr, "%");
        //        else
        //            lstDoc = objARDoc.GetAll(branchID, batNbr, refNbr);
        //        foreach (DataRow doc in lstDoc.Rows)
        //        {
        //            if (doc.Short("Rlsed") != -1)
        //            {
        //                if (doc.String("DocType") == "IN" || doc.String("DocType") == "CM" || doc.String("DocType") == "DM")
        //                    ProcessARBalance(doc.String("CustId"), branchID, doc.Date("DocDate"), doc.String("DocType"), doc.Double("OrigDocAmt") * -1);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //private bool AR10100Release(string branchID, string batNbr)
        //{
        //    try
        //    {
        //        clsAR_Doc doc = new clsAR_Doc(Dal);
        //        DataTable lstDoc = doc.GetAll(branchID, batNbr, "%");
        //        foreach (DataRow arDoc in lstDoc.Rows)
        //        {
        //            if (arDoc["DocType"].ToString() == "IN" || arDoc["DocType"].ToString() == "CM" || arDoc["DocType"].ToString() == "DM")
        //            {
        //                ProcessARBalance(arDoc["CustId"].ToString(), branchID, (DateTime)arDoc["DocDate"], arDoc["DocType"].ToString(), (double)arDoc["OrigDocAmt"]);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}

        private void ProcessARBalance(string custID, string branchID, DateTime docDate, string docType, double origDocAmt)
        {
            try
            {
                clsAR_Balances objBalance = new clsAR_Balances(Dal);
                if (objBalance.GetByKey(custID, branchID))
                {
                    UpdateARBalance(objBalance, custID, docDate, docType, origDocAmt);
                    objBalance.Update();
                }
                else
                {
                    objBalance.Reset();
                    UpdateARBalance(objBalance, custID, docDate, docType, origDocAmt);
                    objBalance.LastActDate = DateTime.Now.Short();
                    objBalance.LastAgeDate = DateTime.Now.Short();
                    objBalance.BranchID = branchID;
                    objBalance.CustID = custID;
                    objBalance.Crtd_DateTime = DateTime.Now;
                    objBalance.Crtd_Prog = Prog;
                    objBalance.Crtd_User = User;
                    objBalance.Add();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void UpdateARBalance(clsAR_Balances bal, string custID, DateTime docDate, string docType, double origDocAmt)
        {
            bal.CurrBal = bal.CurrBal + (docType == "IN" || docType == "DM" ? origDocAmt : 0);
            bal.CurrBal = bal.CurrBal + (docType == "CM" || docType == "PP" || docType == "PA" ? -1 * origDocAmt : 0);
            bal.LastInvcDate = DateTime.Now.Short();
            bal.LUpd_User = User;
            bal.LUpd_Prog = Prog;
            bal.LUpd_DateTime = DateTime.Now.Short();

        }
        private bool UpdateAllocSO(clsIN_Setup setup, string invtID, string siteID, double oldQty, double newQty, int decQty)
        {
            clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
            clsIN_ItemSite objItemSite = new clsIN_ItemSite(Dal);
            objInvt.GetByKey(invtID);
            if (objInvt.StkItem == 1)
            {
                if (objItemSite.GetByKey(invtID, siteID))
                {
                    if (!setup.NegQty && newQty > 0 && objItemSite.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message,"608","", new[] { objItemSite.InvtID, objItemSite.SiteID });
                    }
                    objItemSite.QtyAllocSO = Math.Round(objItemSite.QtyAllocSO + newQty - oldQty, decQty);
                    objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + oldQty - newQty, decQty);
                    objItemSite.Update();
                }
                return true;
            }
            return false;
        }
        private bool CheckAvailableDiscBudget(clsOM_SalesOrd ord, string branchID, string refbudgetID, string discID, string discSeq, double discAmtQty, bool freeItem, bool manualDisc, string lineRef, bool firstCal, bool lineDisc, string invtID, string unit)
        {
            try
            {

                clsOM_OrderType objOrderType = new clsOM_OrderType(Dal);
                clsOM_PPBudget objBudget = new clsOM_PPBudget(Dal);
                clsOM_PPCpny objCpny = new clsOM_PPCpny(Dal);
                clsOM_PPAlloc objbudgetAlloc = new clsOM_PPAlloc(Dal);
                clsOM_PPFreeItem objPPInvt = new clsOM_PPFreeItem(Dal);
                clsIN_Inventory objInvt = new clsIN_Inventory(Dal);
                clsSQL sql = new clsSQL(Dal);
                string budgetID = refbudgetID;
                objOrderType.GetByKey(ord.OrderType);
                sql.OM_GetBudgetIDCpny(ref objBudget, branchID, budgetID);
                if (objOrderType.ARDocType != "NA" && objOrderType.ARDocType != "CM" && objOrderType.ARDocType != "CC" && budgetID != string.Empty && objBudget != null && objBudget.Active)
                {
                    if ((objBudget.ApplyTo == "A" && !freeItem) || (objBudget.ApplyTo != "A" && freeItem))
                    {
                        if (objbudgetAlloc.GetByKey(budgetID, branchID, (objBudget.AllocType == "1" ? ord.SlsPerID : ord.CustID), objBudget.ApplyTo == "A" ? "." : invtID))
                        {

                            if (objBudget.ApplyTo == "A" && lineDisc)
                            {
                                objCpny.GetByKey(budgetID, branchID, ".");
                                if (objCpny.QtyAmtAvail < discAmtQty)
                                {
                                    if (!lineDisc)
                                        discAmtQty = 0;
                                    budgetID = string.Empty;
                                    discID = string.Empty;
                                    discSeq = string.Empty;
                                    throw new MessageException(MessageType.Message,"402");
                                 
                                }
                            }
                            else if (objBudget.ApplyTo != "A" && lineDisc)
                            {
                                if (objPPInvt.GetByKey(budgetID, invtID))
                                {
                                    objCpny.GetByKey(budgetID, branchID, invtID);
                                    objInvt.GetByKey(invtID);
                                    clsIN_UnitConversion uomFrom = SetUOM(invtID, objInvt.ClassID, objInvt.StkUnit, unit);
                                    if (uomFrom != null)
                                    {
                                        clsIN_UnitConversion uomTo = SetUOM(invtID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                        double rate = 1;
                                        double rate2 = 1;

                                        if (uomFrom.MultDiv == "M")
                                            rate = uomFrom.CnvFact;
                                        else
                                            rate = 1 / uomFrom.CnvFact;

                                        if (uomTo.MultDiv == "M")
                                            rate2 = uomTo.CnvFact;
                                        else
                                            rate2 = 1 / uomTo.CnvFact;

                                        rate = Math.Round(rate / rate2, 2);

                                        double tmp = discAmtQty * rate;
                                        if (objCpny.QtyAmtAvail < tmp)
                                        {
                                            if (!lineDisc)
                                                discAmtQty = 0;
                                            budgetID = string.Empty;
                                            discID = string.Empty;
                                            discSeq = string.Empty;
                                             throw new MessageException(MessageType.Message,"402");
                                        

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!lineDisc)
                                discAmtQty = 0;
                          
                            budgetID = string.Empty;
                            discID = string.Empty;
                            discSeq = string.Empty;
                            throw new MessageException(MessageType.Message, "403", "", new string[] { discSeq, budgetID });
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
        private clsIN_UnitConversion SetUOM(string invtID, string classID, string stkUnit, string fromUnit)
        {
            clsIN_UnitConversion unit = new clsIN_UnitConversion(Dal);
            if (!string.IsNullOrEmpty(fromUnit))
            {


                if (unit.GetByKey("3", "*", invtID, fromUnit, stkUnit))
                    return unit;
                if (unit.GetByKey("2", classID, "*", fromUnit, stkUnit))
                    return unit;
                if (unit.GetByKey("1", "*", "*", fromUnit, stkUnit))
                    return unit;
                return unit;

            }
            return unit;
        }
        private double CalculateInvtTotals(clsOM_SalesOrd ord, DataTable lstOrdDet, string invtID, string siteID, string lineRef)
        {
            double qty = 0;
            if (ord.OrderType == "SR" || ord.OrderType == "BL" || ord.OrderType == "OC")
            {
                foreach (DataRow item in lstOrdDet.Rows)
                {
                    if (item["InvtID"].ToString() == invtID && item["LineRef"].ToString() != lineRef && item["BOType"].ToString() != "B")
                    {
                        if (item["UnitMultDiv"].ToString() == "M")
                            qty += (double)item["LineQty"] * (double)item["UnitRate"];
                        else
                            qty += (double)item["LineQty"] / (double)item["UnitRate"];
                    }
                }


            }
            else
            {
                foreach (DataRow item in lstOrdDet.Rows)
                {
                    if (item["InvtID"].ToString() == invtID && item["LineRef"].ToString() != lineRef && item["BOType"].ToString() != "B" && item["SiteID"].ToString() == siteID)
                    {
                        if (item["UnitMultDiv"].ToString() == "M")
                            qty += (double)item["LineQty"] * (double)item["UnitRate"];
                        else
                            qty += (double)item["LineQty"] / (double)item["UnitRate"];
                    }
                }
            }
            return qty;

        }
    }
}
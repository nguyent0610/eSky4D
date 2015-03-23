using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HQFramework.DAL;
using HQFramework.Common;
using System.Data;
using eBiz4DApp;
namespace OMProcess
{
    class clsSQL
    {
        #region SQLCommand
        string strOM_GetPDASalesOrdDetByLineRef = "select * from OM_PDASalesOrdDet as a where a.BranchID=@BranchID and a.OrderNbr=@OrderNbr and exists (select * from dbo.fr_SplitString(@ListLineRef,',') where part=a.LineRef)";
        string strBudgetIDCpny = "select * from OM_PPBudget as b join OM_PPCpny as c on b.BudgetID=c.BudgetID where c.CpnyID=@BranchID";

        #endregion
        private DataAccess mDal;
        public clsSQL(DataAccess da)
        {
            mDal = da;
        }
        #region Inventory
        public void IN10100_UpdateTransfer(string branchID, string batNbr, string prog, string user, DateTime? tranDate, string refNbr, string transferNbr)
        {

            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@TranDate", DbType.DateTime, clsCommon.GetValueDBNull(tranDate), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@TrnsfrDocNbr", DbType.String, clsCommon.GetValueDBNull(transferNbr), ParameterDirection.Input, 30));
                mDal.ExecNonQuery("IN10100_UpdateTransfer", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void IN_ReleaseBatch(string branchID, string batNbr, string prog, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("IN_ReleaseBatch", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void IN_CancelBatch(string branchID, string batNbr, string prog, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("IN_CancelBatch", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool DelCostByCostID(string invtID, string siteID, string costID)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@Action", DbType.String, "DelCostByCostID", ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(invtID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(siteID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CostID", DbType.String, clsCommon.GetValueDBNull(costID), ParameterDirection.Input, 30));
                return (mDal.ExecNonQuery("IN_ItemCostProcessing", CommandType.StoredProcedure, ref pc, "") > 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string INNumbering(string branchID, string type)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@GetType", DbType.String, clsCommon.GetValueDBNull(type), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("INNumbering", CommandType.StoredProcedure, ref pc, "").Rows[0]["Nbr"].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable GetListFIFOCost(string invtID, string siteID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@Action", DbType.String, "GetListFIFOCost", ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(invtID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(siteID), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("IN_ItemCostProcessing", CommandType.StoredProcedure, ref pc, ""));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable GetListLIFOCost(string invtID, string siteID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@Action", DbType.String, "GetListLIFOCost", ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(invtID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(siteID), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("IN_ItemCostProcessing", CommandType.StoredProcedure, ref pc, ""));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool GetCostByCostID(ref clsIN_ItemCost cost, string invtID, string siteID, string costID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                DataSet ds = new DataSet();
                pc.Add(new ParamStruct("@Action", DbType.String, "GetCostByCostID", ParameterDirection.Input, 50)); ;
                pc.Add(new ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(invtID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(siteID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CostID", DbType.String, clsCommon.GetValueDBNull(costID), ParameterDirection.Input, 30));
                ds = mDal.ExecDataSet("IN_ItemCostProcessing", CommandType.StoredProcedure, ref pc, "");
                cost.Reset();
                if (ds.Tables[0].Rows.Count == 0) return false;
                else
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    cost.CostIdentity = (int)clsCommon.GetValue(row["CostIdentity"], cost.CostIdentity.GetType().FullName);
                    cost.InvtID = (string)clsCommon.GetValue(row["InvtID"], cost.InvtID.GetType().FullName);
                    cost.Qty = (double)clsCommon.GetValue(row["Qty"], cost.Qty.GetType().FullName);
                    cost.RcptDate = (DateTime)clsCommon.GetValue(row["RcptDate"], cost.RcptDate.GetType().FullName);
                    cost.RcptNbr = (string)clsCommon.GetValue(row["RcptNbr"], cost.RcptNbr.GetType().FullName);
                    cost.SiteID = (string)clsCommon.GetValue(row["SiteID"], cost.SiteID.GetType().FullName);
                    cost.CostID = (string)clsCommon.GetValue(row["CostID"], cost.CostID.GetType().FullName);
                    cost.TotCost = (double)clsCommon.GetValue(row["TotCost"], cost.TotCost.GetType().FullName);
                    cost.UnitCost = (double)clsCommon.GetValue(row["UnitCost"], cost.UnitCost.GetType().FullName);
                    cost.Crtd_DateTime = (DateTime)clsCommon.GetValue(row["Crtd_DateTime"], cost.Crtd_DateTime.GetType().FullName);
                    cost.Crtd_Prog = (string)clsCommon.GetValue(row["Crtd_Prog"], cost.Crtd_Prog.GetType().FullName);
                    cost.Crtd_User = (string)clsCommon.GetValue(row["Crtd_User"], cost.Crtd_User.GetType().FullName);
                    cost.LUpd_DateTime = (DateTime)clsCommon.GetValue(row["LUpd_DateTime"], cost.LUpd_DateTime.GetType().FullName);
                    cost.LUpd_Prog = (string)clsCommon.GetValue(row["LUpd_Prog"], cost.LUpd_Prog.GetType().FullName);
                    cost.LUpd_User = (string)clsCommon.GetValue(row["LUpd_User"], cost.LUpd_User.GetType().FullName);
                    cost.tstamp = (string)clsCommon.GetValue(row["tstamp"], cost.tstamp.GetType().FullName);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void IN_UpdateEndingAvgCost(string prog, string user, DateTime costDate)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@CostDate", DbType.DateTime, clsCommon.GetValueDBNull(costDate), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("IN_UpdateEndingAvgCost", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void IN_RecalculateAvgCost(string prog, string user, DateTime costDate)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@CostDate", DbType.DateTime, clsCommon.GetValueDBNull(costDate), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("IN_RecalculateAvgCost", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable IN_Integrity_Validate(string prog, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                return mDal.ExecDataTable("IN_Integrity_Validate", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string IN_Integrity_RebuildQtyonSO(string prog, string user, string invtID, string siteID, int qtyDecPl, int prcDecPl, int tranAmtDecPl)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@PInvtID", DbType.String, clsCommon.GetValueDBNull(invtID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PSiteID", DbType.String, clsCommon.GetValueDBNull(siteID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@QtyDecPl", DbType.Int32, clsCommon.GetValueDBNull(qtyDecPl), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@PrcDecPl", DbType.Int32, clsCommon.GetValueDBNull(prcDecPl), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@TranAmtDecPl", DbType.Int32, clsCommon.GetValueDBNull(tranAmtDecPl), ParameterDirection.Input, 10));
                DataTable dt = mDal.ExecDataTable("IN_Integrity_RebuildQtyonSO", CommandType.StoredProcedure, ref pc);
                if (dt.Rows.Count == 0) return string.Empty;
                else return dt.Rows[0]["Mess"].ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string IN_Integrity_RebuildQtyCost(string prog, string user, string invtID, string siteID, int qtyDecPl, int PrcDecPl, int tranAmtDecPl)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@PInvtID", DbType.String, clsCommon.GetValueDBNull(invtID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PSiteID", DbType.String, clsCommon.GetValueDBNull(siteID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@QtyDecPl", DbType.Int32, clsCommon.GetValueDBNull(qtyDecPl), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@PrcDecPl", DbType.Int32, clsCommon.GetValueDBNull(PrcDecPl), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@TranAmtDecPl", DbType.Int32, clsCommon.GetValueDBNull(tranAmtDecPl), ParameterDirection.Input, 10));
                DataTable dt = mDal.ExecDataTable("IN_Integrity_RebuildQtyCost", CommandType.StoredProcedure, ref pc);
                if (dt.Rows.Count == 0) return string.Empty;
                else return dt.Rows[0]["Mess"].ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void IN_Integrity_ReBuildBatch(string prog, string user, string posted, string branchID, string batNbr)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@Posted", DbType.String, clsCommon.GetValueDBNull(posted), ParameterDirection.Input, 1));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.Int32, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                mDal.ExecNonQuery("IN_Integrity_ReBuildBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable IN_ItemCostProcessing_GetCost(string invtid, string Siteid, string costid)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(invtid), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(Siteid), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@CostID", DbType.String, clsCommon.GetValueDBNull(costid), ParameterDirection.Input, 30));
                return mDal.ExecDataTable("IN_ItemCostProcessing_GetCost", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion
        #region Order
        public string OMNumbering(string branchID, string type, string orderType)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@OrderType", DbType.String, clsCommon.GetValueDBNull(orderType), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@GetType", DbType.String, clsCommon.GetValueDBNull(type), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("OMNumbering", CommandType.StoredProcedure, ref pc, "").Rows[0]["Nbr"].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void OM_NoneShipPrintInvc(string internetAddress, string branchID, DateTime docDate, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@DocDate", DbType.DateTime, clsCommon.GetValueDBNull(docDate), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProcID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_NoneShipPrintInvc", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable OM_GetDocDesc(string branchID, string orderNbr)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@OrderNbr", DbType.String, clsCommon.GetValueDBNull(orderNbr), ParameterDirection.Input, 30));
                return mDal.ExecDataTable("OM_GetDocDesc", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable OM_GetInvoicesReleaseAR(string internetAddress, string branchID, string proc)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                return mDal.ExecDataTable("OM_GetInvoicesReleaseAR", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void OM_ReleaseToARDoc(string internetAddress, string branchID, string batNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_ReleaseToARDoc", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_ReleaseToARTrans(string internetAddress, string branchID, string batNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_ReleaseToARTrans", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_ReleaseToARFinalUpdate(string internetAddress, string branchID, string batNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_ReleaseToARFinalUpdate", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable OM_GetInvoicesReleaseIN(string internetAddress, string branchID, string proc)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                return mDal.ExecDataTable("OM_GetInvoicesReleaseIN", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_ReleaseToINTrans(string internetAddress, string branchID, string batNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_ReleaseToINTrans", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_ReleaseToINFinalUpdate(string internetAddress, string branchID, string batNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_ReleaseToINFinalUpdate", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable OM_GetSalesDetFromPDA(string branchID, string origOrderNbr)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@OrigOrderNbr", DbType.String, clsCommon.GetValueDBNull(origOrderNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                return mDal.ExecDataTable("OM_GetSalesDetFromPDA", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_CancelPrintedInvoices(string internetAddress, string branchID, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(internetAddress), ParameterDirection.Input, 21));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_CancelPrintedInvoices", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public int OM_CheckForCancel(string branchID, string orderNbr)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@OrderNbr", DbType.String, clsCommon.GetValueDBNull(orderNbr), ParameterDirection.Input, 30));
                return (int)mDal.ExecDataTable("OM_CheckForCancel", CommandType.StoredProcedure, ref pc).Rows[0][0];

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_CancelBatchIN(string branchID, string batnbr, string refNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_CancelBatchIN", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_CancelBatch(string branchID, string arBatNbr, string inBatNbr, string refNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@ARBatNbr", DbType.String, clsCommon.GetValueDBNull(arBatNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@INBatNbr", DbType.String, clsCommon.GetValueDBNull(inBatNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("OM_CancelBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public short OM_GetOrderNo(string branchID, string slsperID, DateTime orderDate)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@SlsPerID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@OrderDate", DbType.DateTime, clsCommon.GetValueDBNull(orderDate), ParameterDirection.Input, 30));
                return (short)mDal.ExecDataTable("OM_GetOrderNo", CommandType.StoredProcedure, ref pc).Rows[0][0];

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable OM_GetPDASalesOrdDetByLineRef(string branchID, string orderNbr, string listLineRef)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@OrderNbr", DbType.String, clsCommon.GetValueDBNull(orderNbr), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@ListLineRef", DbType.String, clsCommon.GetValueDBNull(listLineRef), ParameterDirection.Input, 500));
                return mDal.ExecDataTable(strOM_GetPDASalesOrdDetByLineRef, CommandType.Text, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void OM_GetBudgetIDCpny(ref clsOM_PPBudget budget, string branchID, string budgetID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                if (mDal.ExecDataTable(strBudgetIDCpny, CommandType.Text, ref pc).Rows.Count > 0)
                    budget.GetByKey(budgetID);
                else
                    budget.Reset();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable OM20500_ppCheckShipQty(string branchID, string orderNbr)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@OrderNbr", DbType.String, clsCommon.GetValueDBNull(orderNbr), ParameterDirection.Input, 50));
                return mDal.ExecDataTable("OM20500_ppCheckShipQty", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable OM41200_AllocateSales(string branchID, string invtID, string slsperID, DateTime orderDate)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));

                pc.Add(new ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(invtID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@OrderDate", DbType.DateTime, clsCommon.GetValueDBNull(orderDate), ParameterDirection.Input, 50));
                return mDal.ExecDataTable("OM41200_AllocSales", CommandType.StoredProcedure, ref pc);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion
        #region AR
        public string ARNumbering(string branchID, string type)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@GetType", DbType.String, clsCommon.GetValueDBNull(type), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("ARNumbering", CommandType.StoredProcedure, ref pc, "").Rows[0]["Nbr"].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AR_ReleaseBatch(string branchID, string batnbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("AR_ReleaseBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string AR_CheckForCancel(string branchID, string batNbr, string refNbr)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("AR_CheckForCancel", CommandType.StoredProcedure, ref pc, "").Rows[0][0].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AR_CancelBatch(string branchID, string batnbr, string refNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("AR_CancelBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void AR_UpdateTranAmtToBeforeTaxAmt(string BranchID, string BatNbr)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.Input, 30));
                mDal.ExecNonQuery("AR_UpdateTranAmtToBeforeTaxAmt", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion
        #region AP
        public string APNumbering(string branchID, string type)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@GetType", DbType.String, clsCommon.GetValueDBNull(type), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("APNumbering", CommandType.StoredProcedure, ref pc, "").Rows[0]["Nbr"].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AP_ReleaseBatch(string branchID, string batnbr, string prog, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("AP_ReleaseBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void AP_CancelBatch(string branchID, string batnbr, string refNbr, string prog, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("AP_CancelBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string AP_CheckForCancel(string branchID, string batNbr, string refNbr)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("AP_CheckForCancel", CommandType.StoredProcedure, ref pc, "").Rows[0][0].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AP_UpdateTranAmtToBeforeTaxAmt(string BranchID, string BatNbr)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.Input, 30));
                mDal.ExecNonQuery("AP_UpdateTranAmtToBeforeTaxAmt", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion
        #region PO

        public void PO_CancelBatch(string branchID, string BatNbr, string RcptNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RcptNbr", DbType.String, clsCommon.GetValueDBNull(RcptNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("PO_CancelBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string PO_CheckForCancel(string branchID, string batNbr, string rcptNbr)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RcptNbr", DbType.String, clsCommon.GetValueDBNull(rcptNbr), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("PO_CheckForCancel", CommandType.StoredProcedure, ref pc, "").Rows[0][0].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void PO_CancelBatchIN(string branchID, string batnbr, string rcptNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RcptNbr", DbType.String, clsCommon.GetValueDBNull(rcptNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("PO_CancelBatchIN", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region CA
        public string CANumbering(string BranchID, string GetType)
        {
            ParamCollection pc = new ParamCollection();
            try
            {
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@GetType", DbType.String, clsCommon.GetValueDBNull(GetType), ParameterDirection.Input, 30));
                return (mDal.ExecDataTable("CANumbering", CommandType.StoredProcedure, ref pc, "").Rows[0]["Nbr"].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CA_ReleaseBatch(string branchID, string batnbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("CA_ReleaseBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void CA_CancelBatch(string branchID, string batnbr, string refNbr, string proc, string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 50));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batnbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(proc), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("CA_CancelBatch", CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion
    }
}

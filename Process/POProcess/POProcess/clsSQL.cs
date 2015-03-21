using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HQFramework.DAL;
using HQFramework.Common;
using System.Data;
using eBiz4DApp;
namespace POProcess
{
    class clsSQL
    {
        private DataAccess mDal;
        public clsSQL(DataAccess da)
        {
            mDal = da;
        }
        public void IN10100_UpdateTransfer(string branchID,string batNbr,string prog,string user,DateTime? tranDate,string refNbr,string transferNbr,string status)
        {
          
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                pc.Add(new ParamStruct("@TranDate", DbType.DateTime, clsCommon.GetValueDBNull(tranDate), ParameterDirection.Input,30));
                pc.Add(new ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(refNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@TrnsfrDocNbr", DbType.String, clsCommon.GetValueDBNull(transferNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@Status", DbType.String, clsCommon.GetValueDBNull(status), ParameterDirection.Input, 1));
                mDal.ExecNonQuery("IN_UpdateTransfer", CommandType.StoredProcedure,ref pc);
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
        public void IN_ReleaseBatch(string branchID,string batNbr,string prog,string user)
        {        
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(batNbr), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ProgID", DbType.String, clsCommon.GetValueDBNull(prog), ParameterDirection.Input, 8));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 10));
                mDal.ExecNonQuery("IN_ReleaseBatch", CommandType.StoredProcedure,ref pc);
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
            ParamCollection pc=new ParamCollection();
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
        public string INNumbering(string branchID,string type)
        {
            ParamCollection pc=new ParamCollection();
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
        
        public void IN_UpdateEndingAvgCost(string prog, string user,DateTime costDate)
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
        public string IN_Integrity_RebuildQtyonSO(string prog, string user,string invtID,string siteID,int qtyDecPl,int prcDecPl,int tranAmtDecPl)
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
                DataTable dt=mDal.ExecDataTable("IN_Integrity_RebuildQtyonSO", CommandType.StoredProcedure, ref pc);
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
        public DataTable IN_ItemCostProcessing_GetCost(string invtid, string Siteid,string costid)
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

    }
}

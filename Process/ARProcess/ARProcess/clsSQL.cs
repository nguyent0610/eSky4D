using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HQFramework.DAL;
using HQFramework.Common;
using System.Data;
using eBiz4DApp;
namespace ARProcess
{
    class clsSQL
    {
        private DataAccess mDal;
        public clsSQL(DataAccess da)
        {
            mDal = da;
        }
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

    }
}

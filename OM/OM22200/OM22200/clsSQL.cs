﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HQFramework.DAL;
using HQFramework.Common;
using System.Data;
using eBiz4DApp_PJP;

namespace OM22200
{
    class clsSQL
    {
        #region SQLCommand
        string InsertRptID = "Insert Into RPTRunning(ReportNbr,MachineName,ReportName,ReportCap,ReportDate,StringParm00,StringParm01,StringParm02,StringParm03," +
                      "DateParm00,DateParm01,DateParm02,DateParm03,BooleanParm00,BooleanParm01,BooleanParm02,BooleanParm03,SelectionFormular,UserID,AppPath,ClientName,LoggedCpnyID,CpnyID,LangID) " +
                      "values(@ReportNbr,@MachineName,@ReportName,@ReportCap,@ReportDate,@StringParm00,@StringParm01,@StringParm02,@StringParm03," +
                      "@DateParm00,@DateParm01,@DateParm02,@DateParm03,@BooleanParm00,@BooleanParm01,@BooleanParm02,@BooleanParm03,@SelectionFormular,@UserID,@AppPath,@ClientName,@LoggedCpnyID,@CpnyID,@LangID) SELECT @@IDENTITY";
       
        #endregion
        private DataAccess _da;
        public clsSQL(DataAccess da)
        {
            _da = da;
        }
     
        public string Content(string user)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(user), ParameterDirection.Input, 1000));
                return (_da.ExecScalar("ServerApp_AutoExpireContentMail", CommandType.StoredProcedure, ref pc, "").ToString());
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void OM_DeleteSalesRouteDetByDate(DateTime FromDate, DateTime Todate, string RouteID, string CustID, string BranchID, string PJPID, string SlsPerID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@FromDate", DbType.DateTime, clsCommon.GetValueDBNull(FromDate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@ToDate", DbType.DateTime, clsCommon.GetValueDBNull(Todate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(RouteID), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(PJPID), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@SlsPerID", DbType.String, clsCommon.GetValueDBNull(SlsPerID), ParameterDirection.Input, 1000));
                _da.ExecScalar("OM_DeleteSalesRouteDetByDate", CommandType.StoredProcedure, ref pc, "");
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void OM_DeleteSalesRouteMaster(string RouteID, string CustID, string BranchID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(RouteID), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 1000));
                _da.ExecScalar("OM_DeleteSalesRouteMaster", CommandType.StoredProcedure, ref pc, "");
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void UpdatePDA(string branchID, string custID, string newCustID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(custID), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@NewCustID", DbType.String, clsCommon.GetValueDBNull(newCustID), ParameterDirection.Input, 1000));
                _da.ExecScalar("ppv_OM20500UpdatePda", CommandType.StoredProcedure, ref pc, "");
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public DataTable GetListPJP(string lstBranch, string lstPJP, string lstCust, string lstSlsperID, string lstSalesRouteID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@lstBranchID", DbType.String, clsCommon.GetValueDBNull(lstBranch), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@lstPJP", DbType.String, clsCommon.GetValueDBNull(lstPJP), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@lstCust", DbType.String, clsCommon.GetValueDBNull(lstCust), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@lstSlsperID", DbType.String, clsCommon.GetValueDBNull(lstSlsperID), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@lstSalesRouteID", DbType.String, clsCommon.GetValueDBNull(lstSalesRouteID), ParameterDirection.Input, 2147483647));
                return (_da.ExecDataTable("GETListPJPMaster", CommandType.StoredProcedure, ref pc, ""));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable GetListNewCust(string lstBranch, string lstCust)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(lstBranch), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@ListCust", DbType.String, clsCommon.GetValueDBNull(lstCust), ParameterDirection.Input, 2147483647));
                return (_da.ExecDataTable("PPV_AR20500GetListNewCust", CommandType.StoredProcedure, ref pc, ""));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void OM22200_CoppyMCP(string ID,DateTime StartDate,DateTime EndDate,string Type)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@StartDate", DbType.DateTime, clsCommon.GetValueDBNull(StartDate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@EndDate", DbType.DateTime, clsCommon.GetValueDBNull(EndDate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Type), ParameterDirection.Input, 1000));
                _da.ExecNonQuery("OM22200_CoppyMCP", CommandType.StoredProcedure, ref pc, "");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable GetListNewOM_SalesRouteMaster(string ID)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 2147483647));           
                return (_da.ExecDataTable("OM22200_GetNewOM_Import", CommandType.StoredProcedure, ref pc, ""));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string GetNewCustID(string branchID, string channel)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@KeyTree", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@PreFix1", DbType.String, clsCommon.GetValueDBNull(channel), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@PreFix2", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@PreFix3", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@PreFix4", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@SufFix1", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@SufFix2", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@SufFix3", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@SufFix4", DbType.String, clsCommon.GetValueDBNull(""), ParameterDirection.Input, 2147483647));
                return _da.ExecScalar("AR20400CustID", CommandType.StoredProcedure, ref pc, "").ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable ExcuteSQL(string proc)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                return _da.ExecDataTable(proc, CommandType.StoredProcedure, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable ExcuteSQLProcText(string sql)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                return _da.ExecDataTable(sql, CommandType.Text, ref pc);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
       
    }
    public class clsPJP
    {
        public string PJPID { get; set; }
        public string BranchID { get; set; }
        public string SalesRouteID { get; set; }
        public string CustID { get; set; }
        public string SlsPerID { get; set; }
        public string SlsFreq { get; set; }
        public string SlsFreqType { get; set; }
        public int VisitSort { get; set; }
        public string WeekofVisit { get; set; }
        public bool Mon { get; set; }
        public bool Tue { get; set; }
        public bool Wed { get; set; }
        public bool Thu { get; set; }
        public bool Fri { get; set; }
        public bool Sat { get; set; }
        public bool Sun { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HQFramework.DAL;
using HQFramework.Common;
using System.Data;

namespace PJPProcess
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
        //public void OM_DeleteSalesRouteMaster(string RouteID, string CustID, string BranchID)
        //{
        //    try
        //    {
        //        ParamCollection pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(RouteID), ParameterDirection.Input, 1000));
        //        pc.Add(new ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.Input, 1000));
        //        pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 1000));
        //        _da.ExecScalar("OM_DeleteSalesRouteMaster", CommandType.StoredProcedure, ref pc, "");
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }

        //}
        
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
                return (_da.ExecDataTable("OM40600_ppRouteMaster", CommandType.StoredProcedure, ref pc, ""));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public void OM40600_CoppyMCP(string ID, DateTime StartDate, DateTime EndDate, string Type)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@StartDate", DbType.DateTime, clsCommon.GetValueDBNull(StartDate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@EndDate", DbType.DateTime, clsCommon.GetValueDBNull(EndDate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Type), ParameterDirection.Input, 1000));
                _da.ExecNonQuery("OM40600_ppCoppyMCP", CommandType.StoredProcedure, ref pc, "");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //public DataTable GetListNewOM_SalesRouteMaster(string ID)
        //{
        //    try
        //    {
        //        ParamCollection pc = new ParamCollection();
        //        pc.Add(new ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 2147483647));
        //        return (_da.ExecDataTable("OM22200_GetNewOM_Import", CommandType.StoredProcedure, ref pc, ""));
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}
        public void OM23800_CoppyMCP(string ID, DateTime StartDate, DateTime EndDate, string Type)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@StartDate", DbType.DateTime, clsCommon.GetValueDBNull(StartDate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@EndDate", DbType.DateTime, clsCommon.GetValueDBNull(EndDate), ParameterDirection.Input, 1000));
                pc.Add(new ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Type), ParameterDirection.Input, 1000));
                _da.ExecNonQuery("OM23800_CoppyMCP", CommandType.StoredProcedure, ref pc, "");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable GetListNewOM_SalesRouteMaster(string ID, bool delOldSlsperID = false)
        {
            try
            {
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 2147483647));
                pc.Add(new ParamStruct("@DelOldSlsperID", DbType.Boolean, clsCommon.GetValueDBNull(delOldSlsperID), ParameterDirection.Input, 2147483647));
                return (_da.ExecDataTable("OM23800_GetNewOM_ImportMCP", CommandType.StoredProcedure, ref pc, ""));
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
        public bool ExtendRoute { get; set; }
    }

}
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
using eBiz4DAppPJP;

using HQ.eSkyFramework;
using System.Globalization;


namespace PJPProcess
{
    public class PJP 
    {
       
        public string Prog { get; set; }
        public string User { get; set; }
        public DataAccess Dal { get; set; }
        public List<MessageException> LogList { get; set; }
        public PJP(string User, string prog, DataAccess dal)
        {
            this.User = User;
            this.Prog = prog;
            this.Dal = dal;
        }


        public bool OM40600CreateMCP(string lstslsPerID, string lstRouteID, string lstCust, string lstPJP, string lstBranch, DateTime Fromdate, DateTime Todate)
        {
            try
            {                                                                              
                string ID = Guid.NewGuid().ToString();
                clsOM_SalesRouteDet objOM_SalesRouteDet = new clsOM_SalesRouteDet(Dal);
                clsOM_SalesRouteMaster objOM_SalesRouteMaster = new clsOM_SalesRouteMaster(Dal);
                clsSQL objSql = new clsSQL(Dal);
                #region insert Bulk BANG TAM
                string strOM_Dettmp = "select * from OM_SalesRouteDetTmp where 'A'='B'";
                DataTable dtOm_SalesRouteDet = new DataTable() { TableName = "OM_SalesRouteDetTmp" };
                dtOm_SalesRouteDet = objSql.ExcuteSQLProcText(strOM_Dettmp);
                dtOm_SalesRouteDet.TableName = "OM_SalesRouteDetTmp";              
                #endregion
               
                DataTable dt = objSql.GetListPJP(lstBranch, lstPJP, lstCust, lstslsPerID.PassNull(), lstRouteID.PassNull());
                IList<clsPJP> lstclsOM_SalesRouteMaster = DataTableHelper.ConvertTo<clsPJP>(dt);
                foreach (var obj in lstclsOM_SalesRouteMaster)
                {
                    clsOM_SalesRouteMaster objmaster = new clsOM_SalesRouteMaster();
                    objmaster.Reset();
                    objmaster.BranchID = obj.BranchID;
                    objmaster.CustID = obj.CustID;
                    objmaster.Fri = obj.Fri;
                    objmaster.Mon = obj.Mon;
                    objmaster.PJPID = obj.PJPID;
                    objmaster.SalesRouteID = obj.SalesRouteID;
                    objmaster.Sat = obj.Sat;
                    objmaster.SlsFreq = obj.SlsFreq;
                    objmaster.SlsFreqType = obj.SlsFreqType;
                    objmaster.SlsPerID = obj.SlsPerID;
                    objmaster.Sun = obj.Sun;
                    objmaster.Thu = obj.Thu;
                    objmaster.Tue = obj.Tue;
                    objmaster.VisitSort = 0;
                    objmaster.Wed = obj.Wed;
                    objmaster.WeekofVisit = obj.WeekofVisit;
                    objmaster.Crtd_User = User;
                    objmaster.Crtd_Prog = Prog;
                    var lstobjdetail = CreateItemNotCommit(objmaster, Fromdate, Todate);
                    //_daapp.CommitTrans();
                    if (lstobjdetail.Count > 0)
                    {
                        foreach (var objdetail in lstobjdetail)
                        {
                            DataRow dtRow = dtOm_SalesRouteDet.NewRow();
                            dtRow["ID"] = ID;
                            dtRow["BranchID"] = objdetail.BranchID;
                            dtRow["SalesRouteID"] = objdetail.SalesRouteID;
                            dtRow["CustID"] = objdetail.CustID;
                            dtRow["SlsPerID"] = objdetail.SlsPerID;
                            dtRow["PJPID"] = objdetail.PJPID;
                            dtRow["SlsFreq"] = objdetail.SlsFreq;
                            dtRow["SlsFreqType"] = objdetail.SlsFreqType;
                            dtRow["WeekofVisit"] = objdetail.WeekofVisit;
                            dtRow["VisitSort"] = objdetail.VisitSort;
                            dtRow["Crtd_Datetime"] = objdetail.Crtd_Datetime;
                            dtRow["Crtd_Prog"] = objdetail.Crtd_Prog;
                            dtRow["Crtd_User"] = objdetail.Crtd_User;
                            dtRow["LUpd_Datetime"] = objdetail.LUpd_Datetime;
                            dtRow["LUpd_Prog"] = objdetail.Crtd_Prog;
                            dtRow["LUpd_User"] = objdetail.Crtd_User;
                            dtRow["VisitDate"] = objdetail.VisitDate; ;
                            dtRow["DayofWeek"] = objdetail.DayofWeek;
                            dtRow["WeekNbr"] = objdetail.WeekNbr;
                            dtOm_SalesRouteDet.Rows.Add(dtRow);

                        }
                    }
                    //lognet.Info("lstobjdetail:" + lstobjdetail.Count);
                }
                //CreateItem(p);
                if (dtOm_SalesRouteDet.Rows.Count > 0)
                {
                    using (SqlConnection dbConnection = new SqlConnection(Dal.ConnectionString))
                    {
                        dbConnection.Open();
                        using (SqlBulkCopy s1 = new SqlBulkCopy(dbConnection))
                        {

                            s1.DestinationTableName = dtOm_SalesRouteDet.TableName;
                            foreach (var column in dtOm_SalesRouteDet.Columns)
                                s1.ColumnMappings.Add(column.ToString(), column.ToString());
                            s1.WriteToServer(dtOm_SalesRouteDet, DataRowState.Added);
                            objSql.OM40600_CoppyMCP(ID, Fromdate, Todate, "Generate");
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
        public bool OM23800CreateMCP(string id)
        {
            try
            {
                DateTime Todate = DateTime.Now;
                DateTime Fromdate = DateTime.Now;
                string ID = Guid.NewGuid().ToString();

                clsSQL objSql = new clsSQL(Dal);
                #region insert Bulk BANG TAM
                string strOM_Dettmp = "select * from OM_SalesRouteDetTmp where 'A'='B'";
                System.Data.DataTable dtOm_SalesRouteDet = new System.Data.DataTable() { TableName = "OM_SalesRouteDetTmp" };
                dtOm_SalesRouteDet = objSql.ExcuteSQLProcText(strOM_Dettmp);
                dtOm_SalesRouteDet.TableName = "OM_SalesRouteDetTmp";

                #endregion

                try
                {
                 
                    System.Data.DataTable dt = objSql.GetListNewOM_SalesRouteMaster(id);//, "%", "%", "%", "%", "%");
                    //TRAN TRUNG HO: PJP sửa lại comit cho AR_Customer với OM_SalesRouteMaster trước,
                    //trước khi sinh OM_SalesRouteDet nếu sinh OM_SalesRouteDet có lỗi thi ko có rollback lại được OM_SalesRouteMaster với AR_Customer đã thay đổi trước đó
                    Dal.CommitTrans();
                    foreach (DataRow r in dt.Rows)
                    {
                        Fromdate = Convert.ToDateTime(r["StartDate"], CultureInfo.InvariantCulture);
                        Todate = Convert.ToDateTime(r["EndDate"], CultureInfo.InvariantCulture);
                    
                        clsOM_SalesRouteMaster objmaster = new clsOM_SalesRouteMaster();
                        objmaster.Reset();
                        objmaster.BranchID = r["BranchID"].ToString();
                        objmaster.CustID = r["CustID"].ToString();
                        objmaster.Fri = r["Fri"].ToString() == "1" ? true : false;
                        objmaster.Mon = r["Mon"].ToString() == "1" ? true : false;
                        objmaster.PJPID = r["PJPID"].ToString();
                        objmaster.SalesRouteID = r["SalesRouteID"].ToString();
                        objmaster.Sat = r["Sat"].ToString() == "1" ? true : false;
                        objmaster.SlsFreq = r["SlsFreq"].ToString();
                        objmaster.SlsFreqType = r["SlsFreqType"].ToString();
                        objmaster.SlsPerID = r["SlsPerID"].ToString();
                        objmaster.Sun = r["Sun"].ToString() == "1" ? true : false;
                        objmaster.Thu = r["Thu"].ToString() == "1" ? true : false;
                        objmaster.Tue = r["Tue"].ToString() == "1" ? true : false;
                        objmaster.VisitSort = r["VisitSort"].ToString() != "" ? int.Parse(r["VisitSort"].ToString()) : 0;
                        objmaster.Wed = r["Wed"].ToString() == "1" ? true : false;
                        objmaster.WeekofVisit = r["WeekofVisit"].ToString();
                        objmaster.Crtd_User = Current.UserName;
                        objmaster.Crtd_Prog = "OM23800";
                        if (Convert.ToBoolean(r["Del"]))
                        {
                           
                        }
                        else
                        {
                            var lstobjdetail = CreateItemNotCommit(objmaster, Fromdate, Todate);
                            if (lstobjdetail.Count > 0)
                            {
                                foreach (var objdetail in lstobjdetail)
                                {
                                    DataRow dtRow = dtOm_SalesRouteDet.NewRow();
                                    dtRow["ID"] = ID;
                                    dtRow["BranchID"] = objdetail.BranchID;
                                    dtRow["SalesRouteID"] = objdetail.SalesRouteID;
                                    dtRow["CustID"] = objdetail.CustID;
                                    dtRow["SlsPerID"] = objdetail.SlsPerID;
                                    dtRow["PJPID"] = objdetail.PJPID;
                                    dtRow["SlsFreq"] = objdetail.SlsFreq;
                                    dtRow["SlsFreqType"] = objdetail.SlsFreqType;
                                    dtRow["WeekofVisit"] = objdetail.WeekofVisit;
                                    dtRow["VisitSort"] = objdetail.VisitSort;
                                    dtRow["Crtd_Datetime"] = objdetail.Crtd_Datetime;
                                    dtRow["Crtd_Prog"] = objdetail.Crtd_Prog;
                                    dtRow["Crtd_User"] = objdetail.Crtd_User;
                                    dtRow["LUpd_Datetime"] = objdetail.LUpd_Datetime;
                                    dtRow["LUpd_Prog"] = objdetail.Crtd_Prog;
                                    dtRow["LUpd_User"] = objdetail.Crtd_User;
                                    dtRow["VisitDate"] = objdetail.VisitDate; ;
                                    dtRow["DayofWeek"] = objdetail.DayofWeek;
                                    dtRow["WeekNbr"] = objdetail.WeekNbr;
                                    dtOm_SalesRouteDet.Rows.Add(dtRow);
                                }
                            }
                        
                        }
                    }
                    //CreateItem(p);

                    if (dtOm_SalesRouteDet.Rows.Count > 0)
                    {
                        using (SqlConnection dbConnection = new SqlConnection(Dal.ConnectionString))
                        {
                            dbConnection.Open();
                            using (SqlBulkCopy s1 = new SqlBulkCopy(dbConnection))
                            {
                                s1.DestinationTableName = dtOm_SalesRouteDet.TableName;
                                foreach (var column in dtOm_SalesRouteDet.Columns)
                                    s1.ColumnMappings.Add(column.ToString(), column.ToString());
                                s1.WriteToServer(dtOm_SalesRouteDet, DataRowState.Added);
                                objSql.OM23800_CoppyMCP(ID, DateTime.Now, DateTime.Now, "IMPOM23800");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<clsOM_SalesRouteDet> CreateItemNotCommit(clsOM_SalesRouteMaster objSaleMaster, DateTime Fromdate, DateTime Todate)
        {
            string prog = objSaleMaster.Crtd_Prog;
            string user = objSaleMaster.Crtd_User;


            System.DateTime dMon = default(System.DateTime);
            System.DateTime dTue = default(System.DateTime);
            System.DateTime dWed = default(System.DateTime);
            System.DateTime dThu = default(System.DateTime);
            System.DateTime dFri = default(System.DateTime);
            System.DateTime dSat = default(System.DateTime);
            System.DateTime dSun = default(System.DateTime);
            List<clsOM_SalesRouteDet> lstOM_SalesRouteDet = new List<clsOM_SalesRouteDet>();
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = new DateTime(2011, 1, 1);
            Calendar cal = dfi.Calendar;
            int subYear = Todate.Year-Fromdate.Year  ;
            DateTime TodateTmp=Todate;

            var nextDate = Fromdate;
            var weekNbr = 0;
            var nextWeek = 0;

            for (int y = 0; y <= subYear; y++)
            {

                int nextyear = Fromdate.Year + 1;
                if (y != 0)
                {
                    Fromdate = new DateTime(nextyear, 1, 1);
                    Todate = new DateTime(nextyear, 12, 31);
                }
                else
                {
                    Todate = new DateTime(Fromdate.Year, 12, 31);
                }
                if (y == subYear)
                {
                    Todate = TodateTmp;
                }
                int iWeekStart = cal.GetWeekOfYear(Fromdate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

                int iWeekStartStart = cal.GetWeekOfYear(Fromdate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

                int iWeekEnd = cal.GetWeekOfYear(Todate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

                if (objSaleMaster.SlsFreqType == "R")
                {
                    if (objSaleMaster.SlsFreq == "F1A" || objSaleMaster.SlsFreq == "F1B" || objSaleMaster.SlsFreq == "F1C" || objSaleMaster.SlsFreq == "F1D")
                    {
                        if (weekNbr == 0) // Lấy ngày bắt đầu viếng thăm
                        {
                            weekNbr = cal.GetWeekOfYear(nextDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) - 1;
                            if (objSaleMaster.Mon)
                            {
                                nextDate = GetDateFromDayofWeek(Fromdate.Year, weekNbr, "Monday");
                            }
                            else if (objSaleMaster.Tue)
                            {
                                nextDate = GetDateFromDayofWeek(Fromdate.Year, weekNbr, "Tuesday");
                            }
                            else if (objSaleMaster.Wed)
                            {
                                nextDate = GetDateFromDayofWeek(Fromdate.Year, weekNbr, "Wednesday");
                            }
                            else if (objSaleMaster.Thu)
                            {
                                nextDate = GetDateFromDayofWeek(Fromdate.Year, weekNbr, "Thursday");
                            }
                            else if (objSaleMaster.Fri)
                            {
                                nextDate = GetDateFromDayofWeek(Fromdate.Year, weekNbr, "Friday");
                            }
                            else if (objSaleMaster.Sat)
                            {
                                nextDate = GetDateFromDayofWeek(Fromdate.Year, weekNbr, "Saturday");
                            }
                            else if (objSaleMaster.Sun)
                            {
                                nextDate = GetDateFromDayofWeek(Fromdate.Year, weekNbr, "Sunday");
                            }
                            while (nextDate < Fromdate)
                            {
                                nextDate = nextDate.AddDays(7);
                            }
                        }
                        if (nextWeek == 0) // Số tuần tiếp theo sẽ viếng thăm
                        {
                            if (objSaleMaster.SlsFreq == "F1A")
                            {
                                nextWeek = 5;
                            }
                            else if (objSaleMaster.SlsFreq == "F1B")
                            {
                                nextWeek = 6;
                            }
                            else if (objSaleMaster.SlsFreq == "F1C")
                            {
                                nextWeek = 7;
                            }
                            else if (objSaleMaster.SlsFreq == "F1D")
                            {
                                nextWeek = 8;
                            }
                        }
                        #region -F1A-
                        while (nextDate <= Todate && nextDate >= Fromdate)
                        {
                            clsOM_SalesRouteDet det1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet();
                            //det1.ResetET();
                            //det1.BranchID = objSaleMaster.BranchID;
                            //det1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //det1.CustID = objSaleMaster.CustID;
                            //det1.SlsPerID = objSaleMaster.SlsPerID;
                            //det1.PJPID = objSaleMaster.PJPID;
                            //det1.SlsFreq = objSaleMaster.SlsFreq;
                            //det1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //det1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //det1.VisitSort = objSaleMaster.VisitSort;
                            //det1.Crtd_Datetime = DateTime.Now;
                            //det1.Crtd_Prog = Prog;
                            //det1.Crtd_User = User;
                            //det1.LUpd_Datetime = DateTime.Now;
                            //det1.LUpd_Prog = Prog;
                            //det1.LUpd_User = User;

                            det1.VisitDate = nextDate;
                            det1.DayofWeek = nextDate.DayOfWeek.ToString().Substring(0, 3);
                            det1.WeekNbr = cal.GetWeekOfYear(nextDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                            lstOM_SalesRouteDet.Add(det1);
                            //_db.OM_SalesRouteDet.AddObject(det1);
                            nextDate = nextDate.AddDays(nextWeek * 7); // Ngày viếng thăm tiếp theo
                        }
                        #endregion
                    }
                    else
                    {
                        if ((objSaleMaster.SlsFreq == "F1/3") || (objSaleMaster.SlsFreq == "F1/2"))
                        {
                            iWeekStart = Util.ToInt(objSaleMaster.WeekofVisit.Replace("W", ""));
                        }

                        for (int i = iWeekStart; i <= iWeekEnd; i++)
                        {
                            dMon = GetDateFromDayofWeek(Fromdate.Year, i, "Monday");
                            dTue = GetDateFromDayofWeek(Fromdate.Year, i, "Tuesday");
                            dWed = GetDateFromDayofWeek(Fromdate.Year, i, "Wednesday");
                            dThu = GetDateFromDayofWeek(Fromdate.Year, i, "Thursday");
                            dFri = GetDateFromDayofWeek(Fromdate.Year, i, "Friday");
                            dSat = GetDateFromDayofWeek(Fromdate.Year, i, "Saturday");
                            dSun = GetDateFromDayofWeek(Fromdate.Year, i, "Sunday");

                            if ((objSaleMaster.SlsFreq == "F1/3") || (objSaleMaster.SlsFreq == "F1/2"))
                            {
                                if (i >= iWeekStartStart)
                                {
                                    clsOM_SalesRouteDet objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(this.Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = objSaleMaster.Mon ? dMon : (objSaleMaster.Tue ? dTue : (objSaleMaster.Wed ? dWed : (objSaleMaster.Thu ? dThu : (objSaleMaster.Fri ? dFri : (objSaleMaster.Sat ? dSat : (objSaleMaster.Sun ? dSun : DateTime.Now.ToDateShort()))))));
                                    objOM_SalesRouteDet1.DayofWeek = objSaleMaster.Mon ? "Mon" : (objSaleMaster.Tue ? "Tue" : (objSaleMaster.Wed ? "Wed" : (objSaleMaster.Thu ? "Thu" : (objSaleMaster.Fri ? "Fri" : (objSaleMaster.Sat ? "Sat" : (objSaleMaster.Sun ? "Sun" : ""))))));
                                    objOM_SalesRouteDet1.WeekNbr = i;

                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                                if (objSaleMaster.SlsFreq == "F1/3")
                                {
                                    i += 11;
                                }
                                else if (objSaleMaster.SlsFreq == "F1/2")
                                {
                                    i += 7;
                                }
                            }
                            else if (objSaleMaster.SlsFreq == "F1")
                            {
                                if ((objSaleMaster.WeekofVisit == "W159" && (i % 4) == 1) || (objSaleMaster.WeekofVisit == "W2610" && (i % 4) == 2) || (objSaleMaster.WeekofVisit == "W3711" && (i % 4) == 3) || (objSaleMaster.WeekofVisit == "W4812" && (i % 4) == 0))
                                {
                                    if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                                    {

                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dMon;
                                        objOM_SalesRouteDet1.DayofWeek = "Mon";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                                    {

                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dTue;
                                        objOM_SalesRouteDet1.DayofWeek = "Tue";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                                    {

                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dWed;
                                        objOM_SalesRouteDet1.DayofWeek = "Wed";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                                    {

                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dThu;
                                        objOM_SalesRouteDet1.DayofWeek = "Thu";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                                    {

                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dFri;
                                        objOM_SalesRouteDet1.DayofWeek = "Fri";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                                    {

                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dSat;
                                        objOM_SalesRouteDet1.DayofWeek = "Sat";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                                    {

                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dSun;
                                        objOM_SalesRouteDet1.DayofWeek = "Sun";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                }
                            }
                            else if (objSaleMaster.SlsFreq == "F2")
                            {
                                if ((objSaleMaster.WeekofVisit == "OW" && (i % 2) != 0) || (objSaleMaster.WeekofVisit == "EW" && (i % 2) == 0))
                                {
                                    if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                                    {
                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dMon;
                                        objOM_SalesRouteDet1.DayofWeek = "Mon";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                                    {
                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dTue;
                                        objOM_SalesRouteDet1.DayofWeek = "Tue";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                                    {
                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dWed;
                                        objOM_SalesRouteDet1.DayofWeek = "Wed";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                                    {
                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dThu;
                                        objOM_SalesRouteDet1.DayofWeek = "Thu";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                                    {
                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dFri;
                                        objOM_SalesRouteDet1.DayofWeek = "Fri";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                                    {
                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dSat;
                                        objOM_SalesRouteDet1.DayofWeek = "Sat";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                    if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                                    {
                                        var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                        //objOM_SalesRouteDet1.Reset();
                                        //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                        //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                        //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                        //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                        //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                        //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                        //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                        //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                        //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                        //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                        //objOM_SalesRouteDet1.Crtd_User = user;
                                        //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                        //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                        //objOM_SalesRouteDet1.LUpd_User = user;

                                        objOM_SalesRouteDet1.VisitDate = dSun;
                                        objOM_SalesRouteDet1.DayofWeek = "Sun";
                                        objOM_SalesRouteDet1.WeekNbr = i;
                                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                    }
                                }
                            }
                            else if (objSaleMaster.SlsFreq == "F4" || objSaleMaster.SlsFreq == "F4A" || objSaleMaster.SlsFreq == "F8" || objSaleMaster.SlsFreq == "F8A" || objSaleMaster.SlsFreq == "F12" || objSaleMaster.SlsFreq == "F16" || objSaleMaster.SlsFreq == "F20" || objSaleMaster.SlsFreq == "F24" || objSaleMaster.SlsFreq == "A")
                            //else if (objSaleMaster.SlsFreq == "F4" || objSaleMaster.SlsFreq == "F8" || objSaleMaster.SlsFreq == "F12" || objSaleMaster.SlsFreq == "A" || objSaleMaster.SlsFreq == "F24")
                            {

                                if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                                {
                                    var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = dMon;
                                    objOM_SalesRouteDet1.DayofWeek = "Mon";
                                    objOM_SalesRouteDet1.WeekNbr = i;
                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                                if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                                {
                                    var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = dTue;
                                    objOM_SalesRouteDet1.DayofWeek = "Tue";
                                    objOM_SalesRouteDet1.WeekNbr = i;
                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                                if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                                {
                                    var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = dWed;
                                    objOM_SalesRouteDet1.DayofWeek = "Wed";
                                    objOM_SalesRouteDet1.WeekNbr = i;
                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                                if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                                {
                                    var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = dThu;
                                    objOM_SalesRouteDet1.DayofWeek = "Thu";
                                    objOM_SalesRouteDet1.WeekNbr = i;
                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                                if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                                {
                                    var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = dFri;
                                    objOM_SalesRouteDet1.DayofWeek = "Fri";
                                    objOM_SalesRouteDet1.WeekNbr = i;
                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                                if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                                {
                                    var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = dSat;
                                    objOM_SalesRouteDet1.DayofWeek = "Sat";
                                    objOM_SalesRouteDet1.WeekNbr = i;
                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                                if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                                {
                                    var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user);// new clsOM_SalesRouteDet(Dal);
                                    //objOM_SalesRouteDet1.Reset();
                                    //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                    //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                    //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                    //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                    //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                    //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                    //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                    //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                    //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                    //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.Crtd_Prog = prog;
                                    //objOM_SalesRouteDet1.Crtd_User = user;
                                    //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                    //objOM_SalesRouteDet1.LUpd_Prog = prog;
                                    //objOM_SalesRouteDet1.LUpd_User = user;

                                    objOM_SalesRouteDet1.VisitDate = dSun;
                                    objOM_SalesRouteDet1.DayofWeek = "Sun";
                                    objOM_SalesRouteDet1.WeekNbr = i;
                                    lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                }
                            }
                            //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                        }
                    }
                }
                else
                {

                    for (Int32 i = iWeekStart; i <= iWeekEnd; i++)
                    {
                        clsOM_SalesRouteDet objOM_SalesRouteDet = new clsOM_SalesRouteDet(Dal);
                        objOM_SalesRouteDet.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet.Crtd_Prog = prog;
                        objOM_SalesRouteDet.Crtd_User = user;
                        objOM_SalesRouteDet.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet.LUpd_Prog = user;
                        objOM_SalesRouteDet.LUpd_User = user;
                        dMon = GetDateFromDayofWeek(Fromdate.Year, i, "Monday");
                        dTue = GetDateFromDayofWeek(Fromdate.Year, i, "Tuesday");
                        dWed = GetDateFromDayofWeek(Fromdate.Year, i, "Wednesday");
                        dThu = GetDateFromDayofWeek(Fromdate.Year, i, "Thursday");
                        dFri = GetDateFromDayofWeek(Fromdate.Year, i, "Friday");
                        dSat = GetDateFromDayofWeek(Fromdate.Year, i, "Saturday");
                        dSun = GetDateFromDayofWeek(Fromdate.Year, i, "Sunday");
                        if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user,dMon, "Mon", i);// new clsOM_SalesRouteDet(Dal);
                            //objOM_SalesRouteDet1.Reset();
                            //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.Crtd_Prog = prog;
                            //objOM_SalesRouteDet1.Crtd_User = user;
                            //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.LUpd_Prog = prog;
                            //objOM_SalesRouteDet1.LUpd_User = user;

                            objOM_SalesRouteDet1.VisitDate = dMon;
                            objOM_SalesRouteDet1.DayofWeek = "Mon";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user, dTue, "Tue", i);//new clsOM_SalesRouteDet(Dal);
                            //objOM_SalesRouteDet1.Reset();
                            //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.Crtd_Prog = prog;
                            //objOM_SalesRouteDet1.Crtd_User = user;
                            //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.LUpd_Prog = prog;
                            //objOM_SalesRouteDet1.LUpd_User = user;

                            objOM_SalesRouteDet1.VisitDate = dTue;
                            objOM_SalesRouteDet1.DayofWeek = "Tue";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user, dWed, "Wed", i);//new clsOM_SalesRouteDet(Dal);
                            //objOM_SalesRouteDet1.Reset();
                            //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.Crtd_Prog = prog;
                            //objOM_SalesRouteDet1.Crtd_User = user;
                            //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.LUpd_Prog = prog;
                            //objOM_SalesRouteDet1.LUpd_User = user;

                            objOM_SalesRouteDet1.VisitDate = dWed;
                            objOM_SalesRouteDet1.DayofWeek = "Wed";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user, dThu, "Thu", i);//new clsOM_SalesRouteDet(Dal);
                            //objOM_SalesRouteDet1.Reset();
                            //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.Crtd_Prog = prog;
                            //objOM_SalesRouteDet1.Crtd_User = user;
                            //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.LUpd_Prog = prog;
                            //objOM_SalesRouteDet1.LUpd_User = user;

                            objOM_SalesRouteDet1.VisitDate = dThu;
                            objOM_SalesRouteDet1.DayofWeek = "Thu";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user, dFri, "Fri", i);// new clsOM_SalesRouteDet(Dal);
                            //objOM_SalesRouteDet1.Reset();
                            //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.Crtd_Prog = prog;
                            //objOM_SalesRouteDet1.Crtd_User = user;
                            //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.LUpd_Prog = prog;
                            //objOM_SalesRouteDet1.LUpd_User = user;

                            objOM_SalesRouteDet1.VisitDate = dFri;
                            objOM_SalesRouteDet1.DayofWeek = "Fri";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user, dSat, "Sat", i);// new clsOM_SalesRouteDet(Dal);
                            //objOM_SalesRouteDet1.Reset();
                            //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.Crtd_Prog = prog;
                            //objOM_SalesRouteDet1.Crtd_User = user;
                            //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.LUpd_Prog = prog;
                            //objOM_SalesRouteDet1.LUpd_User = user;

                            objOM_SalesRouteDet1.VisitDate = dSat;
                            objOM_SalesRouteDet1.DayofWeek = "Sat";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = CreateNewOM_SalesRouteDet(objSaleMaster, prog, user, dSun, "Sun", i);//   new clsOM_SalesRouteDet(Dal);
                            //objOM_SalesRouteDet1.Reset();
                            //objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            //objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            //objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            //objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            //objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            //objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            //objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            //objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            //objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            //objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.Crtd_Prog = prog;
                            //objOM_SalesRouteDet1.Crtd_User = user;
                            //objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            //objOM_SalesRouteDet1.LUpd_Prog = prog;
                            //objOM_SalesRouteDet1.LUpd_User = user;

                            objOM_SalesRouteDet1.VisitDate = dSun;
                            objOM_SalesRouteDet1.DayofWeek = "Sun";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                    }
                }
            }
            //lognet.Info("lstOM_SalesRouteDet COUNT:" + lstOM_SalesRouteDet.Count);
            return lstOM_SalesRouteDet;
        }

        private clsOM_SalesRouteDet CreateNewOM_SalesRouteDet(clsOM_SalesRouteMaster objSaleMaster, string prog, string user, DateTime? visitDate = null, string dayOfWeek = "", int weekNbr = 0)
        {
            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(Dal);
            objOM_SalesRouteDet1.Reset();
            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
            objOM_SalesRouteDet1.Crtd_Prog = prog;
            objOM_SalesRouteDet1.Crtd_User = user;
            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
            objOM_SalesRouteDet1.LUpd_Prog = prog;
            objOM_SalesRouteDet1.LUpd_User = user;
            return objOM_SalesRouteDet1;
        }

        public DateTime GetDateFromDayofWeek(int year, int week, string strDayofWeek)
        {
            System.DateTime firstOfYear = new System.DateTime(year, 1, 1);
            DayOfWeek dayOfweek = default(DayOfWeek);
            try
            {
                switch (strDayofWeek)
                {
                    case "Monday":
                        dayOfweek = DayOfWeek.Monday;
                        break;
                    case "Tuesday":
                        dayOfweek = DayOfWeek.Tuesday;
                        break;
                    case "Wednesday":
                        dayOfweek = DayOfWeek.Wednesday;
                        break;
                    case "Thursday":
                        dayOfweek = DayOfWeek.Thursday;
                        break;
                    case "Friday":
                        dayOfweek = DayOfWeek.Friday;
                        break;
                    case "Saturday":
                        dayOfweek = DayOfWeek.Saturday;
                        break;
                    case "Sunday":
                        dayOfweek = DayOfWeek.Sunday;
                        break;
                }
                return firstOfYear.AddDays((week - 1) * 7 + dayOfweek - firstOfYear.DayOfWeek);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
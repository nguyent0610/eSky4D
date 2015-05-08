using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using System.Text;
namespace AR20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20500Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "AR20500";
        private string _userName = Current.UserName;
        AR20500Entities _db = Util.CreateObjectContext<AR20500Entities>(false);
        private JsonResult mLogMessage;
        private FormCollection mForm;
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }
        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetCustList(string BranchID,DateTime fromDate, DateTime toDate, List<string> slsperID, string status)
        {
            string sls = string.Empty;
            if (slsperID != null)
            {
                foreach (var item in slsperID)
                {
                    sls += item + ",";
                }
            }
            return this.Store(_db.AR20500_pgDetail(BranchID, fromDate.PassMin(), toDate.PassMin(), sls, status).ToList());
        }

        [HttpPost]
        public ActionResult Process(FormCollection data)
        {
            try
            {
                mForm = data;
                StoreDataHandler custHandler = new StoreDataHandler(data["lstCust"]);

                var lstCust = custHandler.ObjectData<AR20500_pgDetail_Result>();

                var access = Session["AR20500"] as AccessRight;

                if (!access.Update && !access.Insert)
                    throw new MessageException(MessageType.Message, "728");
                string handle = data["cboHandle"];
                if (handle != "N" && handle != string.Empty)
                {
                    foreach (var item in lstCust)
                    {
                        if (item.ColCheck == true)
                        {
                            if (item.NewCustID.PassNull() != string.Empty) continue;
                            AR_NewCustomerInfor objNew = _db.AR_NewCustomerInfor.FirstOrDefault(p => p.ID == item.ID && p.BranchID == item.BranchID);
                            if (objNew == null || objNew.Status == "A" || objNew.Status == "D") continue;
                            if (handle == "A")
                            {
                                var objCust = new AR_Customer();
                                objCust.ResetET();

                                objCust.Addr1 = item.Addr2.PassNull() + (item.Addr1.PassNull() != "" ? "," + item.Addr1.PassNull() : "");
                                //objCust.Addr2 = 
                                objCust.BranchID = item.BranchID.PassNull();
                                objCust.City = item.City.PassNull();
                                objCust.State = item.State.PassNull();
                                objCust.ClassId = item.ClassId.PassNull();
                                objCust.CrRule = "N";
                                objCust.Crtd_Datetime = DateTime.Now;
                                objCust.Crtd_Prog = "AR20500";
                                objCust.Crtd_User = Current.UserName;

                                //var state = _db.SI_State.FirstOrDefault(p => p.State.ToLower() == objCust.State.ToLower());
                                //if (state != null) 
                                //{
                                objCust.Territory = item.Territory.PassNull();
                                //}
                                objCust.Attn = objCust.BillAttn = item.ContactName.PassNull();
                                objCust.CustName = objCust.BillName = item.OutletName.PassNull();
                                objCust.CustType = item.CustType.PassNull(); ;
                                objCust.PriceClassID = item.PriceClass.PassNull(); ;
                                objCust.Location = item.Location.PassNull(); ;
                                objCust.DeliveryID = item.DeliveryID.PassNull(); ;
                                objCust.Country = objCust.BillCountry = "VN";
                                objCust.District = item.District.PassNull(); ;
                                objCust.CustId = _db.AR20500_CustID(Current.CpnyID, "", objCust.Territory, objCust.District, "", "", "", "", "", "", objCust.ClassId).FirstOrDefault();


                                objCust.LUpd_Datetime = DateTime.Now;
                                objCust.LUpd_Prog = "AR20500";
                                objCust.LUpd_User = Current.UserName;
                                objCust.Phone = objCust.BillPhone = item.Phone.PassNull(); ;
                                objCust.Channel = objCust.Channel.PassNull();
                                objCust.ShopType = item.ShopType.PassNull(); ;
                                objCust.State = item.State.PassNull(); ;
                                objCust.Status = "A";// item.IsActive == 1 ? "A" : "I";
                                objCust.TaxDflt = "C";
                                objCust.TaxID00 = "OVAT10-00";
                                objCust.TaxID01 = "OVAT05-00";
                                objCust.TaxID02 = "VAT00";
                                objCust.TaxID03 = "NONEVAT";
                                objCust.Terms = "07";


                                //objCust.Territory = "Z3";

                                objCust.ParentRecordID = 4;
                                objCust.NodeLevel = 2;
                                objCust.SlsperId = item.SlsperID;
                                objCust.ExpiryDate = DateTime.Now.ToDateShort();
                                _db.AR_Customer.AddObject(objCust);

                                AR_CustomerLocation loc = new AR_CustomerLocation();
                                loc.BranchID = objCust.BranchID;
                                loc.CustID = objCust.CustId;
                                loc.Crtd_Datetime = loc.LUpd_Datetime = DateTime.Now;
                                loc.Crtd_Prog = loc.LUpd_Prog = "AR20500";
                                loc.Crtd_User = loc.LUpd_User = Current.UserName;
                                loc.Lng = item.Lng.Value;
                                loc.Lat = item.Lat.Value;

                                _db.AR_CustomerLocation.AddObject(loc);

                                OM_SalesRouteMaster master = new OM_SalesRouteMaster();
                                master.ResetET();
                                master.PJPID = item.BranchID;
                                master.SalesRouteID = item.BranchID;
                                master.SlsFreq = item.SlsFreq;
                                master.SlsPerID = item.SlsperID;
                                master.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                                master.EndDate = new DateTime(DateTime.Now.Year, 12, 31);
                                master.VisitSort = item.VisitSort.Value;
                                master.SlsFreqType = "R";
                                master.Crtd_DateTime = DateTime.Now;
                                master.Crtd_Prog = "AR20500";
                                master.Crtd_User = Current.UserName;
                                master.LUpd_DateTime = DateTime.Now;
                                master.LUpd_Prog = "AR20500";
                                master.LUpd_User = Current.UserName;
                                master.CustID = objCust.CustId;
                                master.BranchID = Current.CpnyID;
                                master.WeekofVisit = item.WeekofVisit;
                                master.Mon = item.Mon.ToBool();
                                master.Tue = item.Tue.ToBool();
                                master.Wed = item.Wed.ToBool();
                                master.Thu = item.Thu.ToBool();
                                master.Fri = item.Fri.ToBool();
                                master.Sat = item.Sat.ToBool();
                                master.Sun = item.Sun.ToBool();


                                _db.OM_SalesRouteMaster.AddObject(master);
                                CreateRoute(master);


                                var custID = item.CustID.PassNull().ToLower();
                                if (objNew != null)
                                {

                                    objNew.LUpd_Datetime = DateTime.Now;


                                    objNew.NewCustID = objCust.CustId;
                                    objNew.Status = "A";
                                }
                            }
                            else if (handle == "D")
                            {
                                var custID = item.CustID.PassNull().ToLower();

                                if (objNew != null)
                                {

                                    objNew.LUpd_Datetime = DateTime.Now;


                                    objNew.Status = "D";


                                }
                            }
                            _db.SaveChanges();
                        }
                    }
                }

                if (mLogMessage != null)
                {
                    return mLogMessage;
                }
                else
                    return Json(new { success = true, type = "message", code = "8009" });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }


        public void CreateRoute(OM_SalesRouteMaster master)//(clsOM_SalesRouteMaster objSaleMaster)
        {

            Int32 weekStart = default(Int32);
            Int32 weekEnd = default(Int32);
            var fromdate = master.StartDate.Value;
            var todate = master.EndDate.Value;
            var dMon = default(System.DateTime);
            var dTue = default(System.DateTime);
            var dWed = default(System.DateTime);
            var dThu = default(System.DateTime);
            var dFri = default(System.DateTime);
            var dSat = default(System.DateTime);
            var dSun = default(System.DateTime);
            _db.AR20500_DeleteSalesRouteDetByDate(fromdate, todate, master.SalesRouteID, master.CustID, master.BranchID);
            if (master.SlsFreqType == "R")
            {
                weekStart = Utility.WeeksInYear(fromdate);
                weekEnd = Utility.WeeksInYear(todate);
                for (Int32 i = weekStart; i <= weekEnd; i++)
                {
                    OM_SalesRouteDet det = new OM_SalesRouteDet();
                    det.BranchID = master.BranchID;
                    det.SalesRouteID = master.SalesRouteID;
                    det.CustID = master.CustID;
                    det.SlsPerID = master.SlsPerID;
                    det.PJPID = master.PJPID;
                    det.SlsFreq = master.SlsFreq;
                    det.SlsFreqType = master.SlsFreqType;
                    det.WeekofVisit = master.WeekofVisit;
                    det.VisitSort = master.VisitSort;
                    det.Crtd_Datetime = DateTime.Now;
                    det.Crtd_Prog = _screenNbr;
                    det.Crtd_User = _userName;
                    det.LUpd_Datetime = DateTime.Now;
                    det.LUpd_Prog = _screenNbr;
                    det.LUpd_User = _userName;
                    dMon = GetDateFromDayofWeek(fromdate.Year, i, "Monday");
                    dTue = GetDateFromDayofWeek(fromdate.Year, i, "Tuesday");
                    dWed = GetDateFromDayofWeek(fromdate.Year, i, "Wednesday");
                    dThu = GetDateFromDayofWeek(fromdate.Year, i, "Thursday");
                    dFri = GetDateFromDayofWeek(fromdate.Year, i, "Friday");
                    dSat = GetDateFromDayofWeek(fromdate.Year, i, "Saturday");
                    dSun = GetDateFromDayofWeek(fromdate.Year, i, "Sunday");
                    if (master.SlsFreq == "F1")
                    {
                        if ((master.WeekofVisit == "W159" && (i % 4) == 1) || (master.WeekofVisit == "W2610" && (i % 4) == 2) || (master.WeekofVisit == "W3711" && (i % 4) == 3) || (master.WeekofVisit == "W4812" && (i % 4) == 0))
                        {
                            if (master.Mon && dMon <= todate && dMon >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dMon;
                                det1.DayofWeek = "Mon";
                                det1.WeekNbr = i;
                                _db.OM_SalesRouteDet.AddObject(det1);

                            }
                            if (master.Tue && dTue <= todate && dTue >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dTue;
                                det1.DayofWeek = "Tue";
                                det1.WeekNbr = i;
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Wed && dWed <= todate && dWed >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dWed;
                                det1.DayofWeek = "Wed";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Thu && dThu <= todate && dThu >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dThu;
                                det1.DayofWeek = "Thu";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Fri && dFri <= todate && dFri >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dFri;
                                det1.DayofWeek = "Fri";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Sat && dSat <= todate && dSat >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dSat;
                                det1.DayofWeek = "Sat";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Sun && dSun <= todate && dSun >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dSun;
                                det1.DayofWeek = "Sun";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                        }
                    }
                    else if (master.SlsFreq == "F2")
                    {
                        if ((master.WeekofVisit == "OW" && (i % 2) != 0) || (master.WeekofVisit == "EW" && (i % 2) == 0))
                        {
                            if (master.Mon && dMon <= todate && dMon >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dMon;
                                det1.DayofWeek = "Mon";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Tue && dTue <= todate && dTue >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dTue;
                                det1.DayofWeek = "Tue";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Wed && dWed <= todate && dWed >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dWed;
                                det1.DayofWeek = "Wed";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Thu && dThu <= todate && dThu >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dThu;
                                det1.DayofWeek = "Thu";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Fri && dFri <= todate && dFri >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dFri;
                                det1.DayofWeek = "Fri";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Sat && dSat <= todate && dSat >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dSat;
                                det1.DayofWeek = "Sat";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                            if (master.Sun && dSun <= todate && dSun >= fromdate)
                            {
                                OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                                det1.ResetET();
                                det1.BranchID = master.BranchID;
                                det1.SalesRouteID = master.SalesRouteID;
                                det1.CustID = master.CustID;
                                det1.SlsPerID = master.SlsPerID;
                                det1.PJPID = master.PJPID;
                                det1.SlsFreq = master.SlsFreq;
                                det1.SlsFreqType = master.SlsFreqType;
                                det1.WeekofVisit = master.WeekofVisit;
                                det1.VisitSort = master.VisitSort;
                                det1.Crtd_Datetime = DateTime.Now;
                                det1.Crtd_Prog = _screenNbr;
                                det1.Crtd_User = _userName;
                                det1.LUpd_Datetime = DateTime.Now;
                                det1.LUpd_Prog = _screenNbr;
                                det1.LUpd_User = _userName;

                                det1.VisitDate = dSun;
                                det1.DayofWeek = "Sun";
                                det1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                _db.OM_SalesRouteDet.AddObject(det1);
                            }
                        }
                    }
                    else if (master.SlsFreq == "F4" || master.SlsFreq == "F8" || master.SlsFreq == "F12" || master.SlsFreq == "A")
                    {
                        if (master.Mon && dMon <= todate && dMon >= fromdate)
                        {
                            OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                            det1.ResetET();
                            det1.BranchID = master.BranchID;
                            det1.SalesRouteID = master.SalesRouteID;
                            det1.CustID = master.CustID;
                            det1.SlsPerID = master.SlsPerID;
                            det1.PJPID = master.PJPID;
                            det1.SlsFreq = master.SlsFreq;
                            det1.SlsFreqType = master.SlsFreqType;
                            det1.WeekofVisit = master.WeekofVisit;
                            det1.VisitSort = master.VisitSort;
                            det1.Crtd_Datetime = DateTime.Now;
                            det1.Crtd_Prog = _screenNbr;
                            det1.Crtd_User = _userName;
                            det1.LUpd_Datetime = DateTime.Now;
                            det1.LUpd_Prog = _screenNbr;
                            det1.LUpd_User = _userName;

                            det1.VisitDate = dMon;
                            det1.DayofWeek = "Mon";
                            det1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                            _db.OM_SalesRouteDet.AddObject(det1);
                        }
                        if (master.Tue && dTue <= todate && dTue >= fromdate)
                        {
                            OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                            det1.ResetET();
                            det1.BranchID = master.BranchID;
                            det1.SalesRouteID = master.SalesRouteID;
                            det1.CustID = master.CustID;
                            det1.SlsPerID = master.SlsPerID;
                            det1.PJPID = master.PJPID;
                            det1.SlsFreq = master.SlsFreq;
                            det1.SlsFreqType = master.SlsFreqType;
                            det1.WeekofVisit = master.WeekofVisit;
                            det1.VisitSort = master.VisitSort;
                            det1.Crtd_Datetime = DateTime.Now;
                            det1.Crtd_Prog = _screenNbr;
                            det1.Crtd_User = _userName;
                            det1.LUpd_Datetime = DateTime.Now;
                            det1.LUpd_Prog = _screenNbr;
                            det1.LUpd_User = _userName;

                            det1.VisitDate = dTue;
                            det1.DayofWeek = "Tue";
                            det1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                            _db.OM_SalesRouteDet.AddObject(det1);
                        }
                        if (master.Wed && dWed <= todate && dWed >= fromdate)
                        {
                            OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                            det1.ResetET();
                            det1.BranchID = master.BranchID;
                            det1.SalesRouteID = master.SalesRouteID;
                            det1.CustID = master.CustID;
                            det1.SlsPerID = master.SlsPerID;
                            det1.PJPID = master.PJPID;
                            det1.SlsFreq = master.SlsFreq;
                            det1.SlsFreqType = master.SlsFreqType;
                            det1.WeekofVisit = master.WeekofVisit;
                            det1.VisitSort = master.VisitSort;
                            det1.Crtd_Datetime = DateTime.Now;
                            det1.Crtd_Prog = _screenNbr;
                            det1.Crtd_User = _userName;
                            det1.LUpd_Datetime = DateTime.Now;
                            det1.LUpd_Prog = _screenNbr;
                            det1.LUpd_User = _userName;

                            det1.VisitDate = dWed;
                            det1.DayofWeek = "Wed";
                            det1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            _db.OM_SalesRouteDet.AddObject(det1);
                        }
                        if (master.Thu && dThu <= todate && dThu >= fromdate)
                        {
                            OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                            det1.ResetET();
                            det1.BranchID = master.BranchID;
                            det1.SalesRouteID = master.SalesRouteID;
                            det1.CustID = master.CustID;
                            det1.SlsPerID = master.SlsPerID;
                            det1.PJPID = master.PJPID;
                            det1.SlsFreq = master.SlsFreq;
                            det1.SlsFreqType = master.SlsFreqType;
                            det1.WeekofVisit = master.WeekofVisit;
                            det1.VisitSort = master.VisitSort;
                            det1.Crtd_Datetime = DateTime.Now;
                            det1.Crtd_Prog = _screenNbr;
                            det1.Crtd_User = _userName;
                            det1.LUpd_Datetime = DateTime.Now;
                            det1.LUpd_Prog = _screenNbr;
                            det1.LUpd_User = _userName;

                            det1.VisitDate = dThu;
                            det1.DayofWeek = "Thu";
                            det1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            _db.OM_SalesRouteDet.AddObject(det1);
                        }
                        if (master.Fri && dFri <= todate && dFri >= fromdate)
                        {
                            OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                            det1.ResetET();
                            det1.BranchID = master.BranchID;
                            det1.SalesRouteID = master.SalesRouteID;
                            det1.CustID = master.CustID;
                            det1.SlsPerID = master.SlsPerID;
                            det1.PJPID = master.PJPID;
                            det1.SlsFreq = master.SlsFreq;
                            det1.SlsFreqType = master.SlsFreqType;
                            det1.WeekofVisit = master.WeekofVisit;
                            det1.VisitSort = master.VisitSort;
                            det1.Crtd_Datetime = DateTime.Now;
                            det1.Crtd_Prog = _screenNbr;
                            det1.Crtd_User = _userName;
                            det1.LUpd_Datetime = DateTime.Now;
                            det1.LUpd_Prog = _screenNbr;
                            det1.LUpd_User = _userName;

                            det1.VisitDate = dFri;
                            det1.DayofWeek = "Fri";
                            det1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            _db.OM_SalesRouteDet.AddObject(det1);
                        }
                        if (master.Sat && dSat <= todate && dSat >= fromdate)
                        {
                            OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                            det1.ResetET();
                            det1.BranchID = master.BranchID;
                            det1.SalesRouteID = master.SalesRouteID;
                            det1.CustID = master.CustID;
                            det1.SlsPerID = master.SlsPerID;
                            det1.PJPID = master.PJPID;
                            det1.SlsFreq = master.SlsFreq;
                            det1.SlsFreqType = master.SlsFreqType;
                            det1.WeekofVisit = master.WeekofVisit;
                            det1.VisitSort = master.VisitSort;
                            det1.Crtd_Datetime = DateTime.Now;
                            det1.Crtd_Prog = _screenNbr;
                            det1.Crtd_User = _userName;
                            det1.LUpd_Datetime = DateTime.Now;
                            det1.LUpd_Prog = _screenNbr;
                            det1.LUpd_User = _userName;

                            det1.VisitDate = dSat;
                            det1.DayofWeek = "Sat";
                            det1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            _db.OM_SalesRouteDet.AddObject(det1);
                        }
                        if (master.Sun && dSun <= todate && dSun >= fromdate)
                        {
                            OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                            det1.ResetET();
                            det1.BranchID = master.BranchID;
                            det1.SalesRouteID = master.SalesRouteID;
                            det1.CustID = master.CustID;
                            det1.SlsPerID = master.SlsPerID;
                            det1.PJPID = master.PJPID;
                            det1.SlsFreq = master.SlsFreq;
                            det1.SlsFreqType = master.SlsFreqType;
                            det1.WeekofVisit = master.WeekofVisit;
                            det1.VisitSort = master.VisitSort;
                            det1.Crtd_Datetime = DateTime.Now;
                            det1.Crtd_Prog = _screenNbr;
                            det1.Crtd_User = _userName;
                            det1.LUpd_Datetime = DateTime.Now;
                            det1.LUpd_Prog = _screenNbr;
                            det1.LUpd_User = _userName;

                            det1.VisitDate = dSun;
                            det1.DayofWeek = "Sun";
                            det1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            _db.OM_SalesRouteDet.AddObject(det1);
                        }
                    }
                    //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                }
            }
            else
            {
                weekStart = Utility.WeeksInYear(fromdate);
                weekEnd = Utility.WeeksInYear(todate);
                for (Int32 i = weekStart; i <= weekEnd; i++)
                {
                    OM_SalesRouteDet det = new OM_SalesRouteDet();
                    det.BranchID = master.BranchID;
                    det.SalesRouteID = master.SalesRouteID;
                    det.CustID = master.CustID;
                    det.SlsPerID = master.SlsPerID;
                    det.PJPID = master.PJPID;
                    det.SlsFreq = master.SlsFreq;
                    det.SlsFreqType = master.SlsFreqType;
                    det.WeekofVisit = master.WeekofVisit;
                    det.VisitSort = master.VisitSort;
                    det.Crtd_Datetime = DateTime.Now;
                    det.Crtd_Prog = _screenNbr;
                    det.Crtd_User = _userName;
                    det.LUpd_Datetime = DateTime.Now;
                    det.LUpd_Prog = _userName;
                    det.LUpd_User = _userName;
                    dMon = GetDateFromDayofWeek(fromdate.Year, i, "Monday");
                    dTue = GetDateFromDayofWeek(fromdate.Year, i, "Tuesday");
                    dWed = GetDateFromDayofWeek(fromdate.Year, i, "Wednesday");
                    dThu = GetDateFromDayofWeek(fromdate.Year, i, "Thursday");
                    dFri = GetDateFromDayofWeek(fromdate.Year, i, "Friday");
                    dSat = GetDateFromDayofWeek(fromdate.Year, i, "Saturday");
                    dSun = GetDateFromDayofWeek(fromdate.Year, i, "Sunday");
                    if (master.Mon && dMon <= todate && dMon >= fromdate)
                    {
                        OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                        det1.ResetET();
                        det1.BranchID = master.BranchID;
                        det1.SalesRouteID = master.SalesRouteID;
                        det1.CustID = master.CustID;
                        det1.SlsPerID = master.SlsPerID;
                        det1.PJPID = master.PJPID;
                        det1.SlsFreq = master.SlsFreq;
                        det1.SlsFreqType = master.SlsFreqType;
                        det1.WeekofVisit = master.WeekofVisit;
                        det1.VisitSort = master.VisitSort;
                        det1.Crtd_Datetime = DateTime.Now;
                        det1.Crtd_Prog = _screenNbr;
                        det1.Crtd_User = _userName;
                        det1.LUpd_Datetime = DateTime.Now;
                        det1.LUpd_Prog = _screenNbr;
                        det1.LUpd_User = _userName;

                        det1.VisitDate = dMon;
                        det1.DayofWeek = "Mon";
                        det1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                        _db.OM_SalesRouteDet.AddObject(det1);
                    }
                    if (master.Tue && dTue <= todate && dTue >= fromdate)
                    {
                        OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                        det1.ResetET();
                        det1.BranchID = master.BranchID;
                        det1.SalesRouteID = master.SalesRouteID;
                        det1.CustID = master.CustID;
                        det1.SlsPerID = master.SlsPerID;
                        det1.PJPID = master.PJPID;
                        det1.SlsFreq = master.SlsFreq;
                        det1.SlsFreqType = master.SlsFreqType;
                        det1.WeekofVisit = master.WeekofVisit;
                        det1.VisitSort = master.VisitSort;
                        det1.Crtd_Datetime = DateTime.Now;
                        det1.Crtd_Prog = _screenNbr;
                        det1.Crtd_User = _userName;
                        det1.LUpd_Datetime = DateTime.Now;
                        det1.LUpd_Prog = _screenNbr;
                        det1.LUpd_User = _userName;

                        det1.VisitDate = dTue;
                        det1.DayofWeek = "Tue";
                        det1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                        _db.OM_SalesRouteDet.AddObject(det1);
                    }
                    if (master.Wed && dWed <= todate && dWed >= fromdate)
                    {
                        OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                        det1.ResetET();
                        det1.BranchID = master.BranchID;
                        det1.SalesRouteID = master.SalesRouteID;
                        det1.CustID = master.CustID;
                        det1.SlsPerID = master.SlsPerID;
                        det1.PJPID = master.PJPID;
                        det1.SlsFreq = master.SlsFreq;
                        det1.SlsFreqType = master.SlsFreqType;
                        det1.WeekofVisit = master.WeekofVisit;
                        det1.VisitSort = master.VisitSort;
                        det1.Crtd_Datetime = DateTime.Now;
                        det1.Crtd_Prog = _screenNbr;
                        det1.Crtd_User = _userName;
                        det1.LUpd_Datetime = DateTime.Now;
                        det1.LUpd_Prog = _screenNbr;
                        det1.LUpd_User = _userName;

                        det1.VisitDate = dWed;
                        det1.DayofWeek = "Wed";
                        det1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        _db.OM_SalesRouteDet.AddObject(det1);
                    }
                    if (master.Thu && dThu <= todate && dThu >= fromdate)
                    {
                        OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                        det1.ResetET();
                        det1.BranchID = master.BranchID;
                        det1.SalesRouteID = master.SalesRouteID;
                        det1.CustID = master.CustID;
                        det1.SlsPerID = master.SlsPerID;
                        det1.PJPID = master.PJPID;
                        det1.SlsFreq = master.SlsFreq;
                        det1.SlsFreqType = master.SlsFreqType;
                        det1.WeekofVisit = master.WeekofVisit;
                        det1.VisitSort = master.VisitSort;
                        det1.Crtd_Datetime = DateTime.Now;
                        det1.Crtd_Prog = _screenNbr;
                        det1.Crtd_User = _userName;
                        det1.LUpd_Datetime = DateTime.Now;
                        det1.LUpd_Prog = _screenNbr;
                        det1.LUpd_User = _userName;

                        det1.VisitDate = dThu;
                        det1.DayofWeek = "Thu";
                        det1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        _db.OM_SalesRouteDet.AddObject(det1);
                    }
                    if (master.Fri && dFri <= todate && dFri >= fromdate)
                    {
                        OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                        det1.ResetET();
                        det1.BranchID = master.BranchID;
                        det1.SalesRouteID = master.SalesRouteID;
                        det1.CustID = master.CustID;
                        det1.SlsPerID = master.SlsPerID;
                        det1.PJPID = master.PJPID;
                        det1.SlsFreq = master.SlsFreq;
                        det1.SlsFreqType = master.SlsFreqType;
                        det1.WeekofVisit = master.WeekofVisit;
                        det1.VisitSort = master.VisitSort;
                        det1.Crtd_Datetime = DateTime.Now;
                        det1.Crtd_Prog = _screenNbr;
                        det1.Crtd_User = _userName;
                        det1.LUpd_Datetime = DateTime.Now;
                        det1.LUpd_Prog = _screenNbr;
                        det1.LUpd_User = _userName;

                        det1.VisitDate = dFri;
                        det1.DayofWeek = "Fri";
                        det1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        _db.OM_SalesRouteDet.AddObject(det1);
                    }
                    if (master.Sat && dSat <= todate && dSat >= fromdate)
                    {
                        OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                        det1.ResetET();
                        det1.BranchID = master.BranchID;
                        det1.SalesRouteID = master.SalesRouteID;
                        det1.CustID = master.CustID;
                        det1.SlsPerID = master.SlsPerID;
                        det1.PJPID = master.PJPID;
                        det1.SlsFreq = master.SlsFreq;
                        det1.SlsFreqType = master.SlsFreqType;
                        det1.WeekofVisit = master.WeekofVisit;
                        det1.VisitSort = master.VisitSort;
                        det1.Crtd_Datetime = DateTime.Now;
                        det1.Crtd_Prog = _screenNbr;
                        det1.Crtd_User = _userName;
                        det1.LUpd_Datetime = DateTime.Now;
                        det1.LUpd_Prog = _screenNbr;
                        det1.LUpd_User = _userName;

                        det1.VisitDate = dSat;
                        det1.DayofWeek = "Sat";
                        det1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        _db.OM_SalesRouteDet.AddObject(det1);
                    }
                    if (master.Sun && dSun <= todate && dSun >= fromdate)
                    {
                        OM_SalesRouteDet det1 = new OM_SalesRouteDet();
                        det1.ResetET();
                        det1.BranchID = master.BranchID;
                        det1.SalesRouteID = master.SalesRouteID;
                        det1.CustID = master.CustID;
                        det1.SlsPerID = master.SlsPerID;
                        det1.PJPID = master.PJPID;
                        det1.SlsFreq = master.SlsFreq;
                        det1.SlsFreqType = master.SlsFreqType;
                        det1.WeekofVisit = master.WeekofVisit;
                        det1.VisitSort = master.VisitSort;
                        det1.Crtd_Datetime = DateTime.Now;
                        det1.Crtd_Prog = _screenNbr;
                        det1.Crtd_User = _userName;
                        det1.LUpd_Datetime = DateTime.Now;
                        det1.LUpd_Prog = _screenNbr;
                        det1.LUpd_User = _userName;

                        det1.VisitDate = dSun;
                        det1.DayofWeek = "Sun";
                        det1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        _db.OM_SalesRouteDet.AddObject(det1);
                    }
                }

            }
            //_daapp.CommitTrans();
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

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
using HQ.eSkySys;
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
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult mLogMessage;
        private FormCollection mForm;

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadAR20500");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                    _filePath = config.TextVal;
                else
                    _filePath = Server.MapPath("~\\Images\\AR20500");
                return _filePath;
            }
        }
        
        private bool _AR20500Anova;
        internal bool AR20500Anova
        {
            get
            {
                // IntVal = 0 khong check dieu kien - IntVal = 1 co check dieu kien (Name/Addr/Phone)
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "AR20500Anova");
                if (config != null)
                {
                    if (config.IntVal == 1)
                        _AR20500Anova = true;
                    else
                        _AR20500Anova = false;
                }
                else
                    _AR20500Anova = false;
                return _AR20500Anova;
            }
        }

        private int _checkApprove;
        internal int checkApprove
        {
            get
            {
                // IntVal = 0 khong check dieu kien - IntVal = 1 co check dieu kien (Name/Addr/Phone)
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "AR20500CheckApprove");
                if (config != null && (config.IntVal == 0 || config.IntVal == 1))
                    _checkApprove = config.IntVal;
                else
                    _checkApprove = 0;
                return _checkApprove;
            }
        }



        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            ViewBag.ImagePath = FilePath;
            ViewBag.IsShowCustHT = _db.AR20500_pdIsShowCustHT().FirstOrDefault();
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
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
        public ActionResult Process(FormCollection data, DateTime fromDate, DateTime toDate, int askApprove)
        {
            try
            {
                string errorCustID = string.Empty;
                mForm = data;
                StoreDataHandler custHandler = new StoreDataHandler(data["lstCust"]);

                var lstCust = custHandler.ObjectData<AR20500_pgDetail_Result>();
               
              
                var access = Session["AR20500"] as AccessRight;

                if (!access.Update && !access.Insert)
                    throw new MessageException(MessageType.Message, "728");
                string handle = data["cboHandle"];
                string custlist = "";

                if (handle != "N" && handle != string.Empty)
                {
                    if (checkApprove == 1 && askApprove == 0 && handle == "A")
                    {
                        foreach (var item in lstCust)
                        {
                            if (item.ColCheck == true)
                            {
                                //Check dieu kien Name/Addr/Phone
                                var objCheck = _db.AR20500_ppCheckApprove(item.OutletName, item.Phone, item.Addr1, Current.LangID).FirstOrDefault();
                                if (objCheck != null)
                                {
                                    if (objCheck.Result == true)
                                        errorCustID += item.CustID + ",";
                                }
                            }
                        }
                    }
                    if (errorCustID != string.Empty)
                        throw new MessageException(MessageType.Message, "2016062801", "askApprove", parm: new string[] { errorCustID });

                    foreach (var item in lstCust)
                    {
                        if (item.ColCheck == true)
                        {
                            _db = Util.CreateObjectContext<AR20500Entities>(false);
                            if (item.NewCustID.PassNull() != string.Empty) continue;
                           
                            AR_NewCustomerInfor objNew = _db.AR_NewCustomerInfor.FirstOrDefault(p => p.ID == item.ID && p.BranchID == item.BranchID);
                            if (objNew == null || objNew.Status == "A" || objNew.Status == "D") continue;
                            if (handle == "A")
                            {
                                string cust = _db.AR20500_ppCheckCustomerApprove(data["cboCpnyID"].ToString(), item.CustID,item.ID,item.OutletName,item.Phone,item.Addr1).FirstOrDefault();
                                if (cust.PassNull() != "")
                                {
                                    custlist += cust.TrimEnd(',') + ",";
                                    _db.Dispose();
                                    continue;
                                    //throw new MessageException(MessageType.Message, "201405281", "", new string[] { cust.TrimEnd(',') });
                                }
                                objNew.WeekofVisit = item.WeekofVisit;
                                objNew.Mon = item.Mon.Value ? int.Parse("1") : int.Parse("0");
                                objNew.Tue = item.Tue.Value ? int.Parse("1") : int.Parse("0");
                                objNew.Wed = item.Wed.Value ? int.Parse("1") : int.Parse("0");
                                objNew.Thu = item.Thu.Value ? int.Parse("1") : int.Parse("0");
                                objNew.Fri = item.Fri.Value ? int.Parse("1") : int.Parse("0");
                                objNew.Sat = item.Sat.Value ? int.Parse("1") : int.Parse("0");
                                objNew.Sun = item.Sun.Value ? int.Parse("1") : int.Parse("0");
                                objNew.SalesRouteID = item.SalesRouteID;
                                objNew.PJPID = item.PJPID;
                                objNew.SlsFreq = item.SlsFreq;
                                objNew.Startday = fromDate;
                                objNew.Endday = toDate;
                                objNew.VisitSort = item.VisitSort.Value;

                                objNew.OutletName = item.OutletName;
                                objNew.Phone = item.Phone;
                                objNew.Addr1 = item.Addr1;

                                var objCust = new AR_Customer();
                                objCust.ResetET();

                                objCust.Addr1 =  objCust.BillAddr1=item.Addr1.PassNull();// item.Addr2.PassNull() + (item.Addr1.PassNull() != "" ? "," + item.Addr1.PassNull() : "");
                                objCust.Addr2 =  objCust.BillAddr2=item.Addr2.PassNull();//
                                objCust.BranchID = item.BranchID.PassNull();
                                objCust.City = objCust.BillCity = item.City.PassNull();
                                objCust.State = objCust.BillState = item.State.PassNull();
                                objCust.ClassId = item.ClassId.PassNull();

                                objCust.Phone = item.Phone.PassNull();

                                objCust.Channel = item.Channel.PassNull();
                                objCust.Area = item.Area.PassNull();
                                objCust.EMailAddr = item.Email.PassNull();
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
                                objCust.CustId = _db.AR20500_CustID(item.BranchID, "", objCust.Territory, objCust.District, "", "", "", "", "", "", objCust.ClassId, item.State.PassNull(), objCust.CustName).FirstOrDefault();
                                objCust.DfltShipToId = "DEFAULT";
                                objCust.LTTContractNbr = item.CustHT;
                                objCust.LUpd_Datetime = DateTime.Now;
                                objCust.LUpd_Prog = "AR20500";
                                objCust.LUpd_User = Current.UserName;
                                objCust.Phone = objCust.BillPhone = item.Phone.PassNull(); ;
                                objCust.Channel = objCust.Channel.PassNull();
                                objCust.Area = objCust.Area.PassNull();
                                if (AR20500Anova == true)
                                    objCust.ShopType = "RS";
                                else
                                    objCust.ShopType = item.ShopType.PassNull();
                                objCust.State = item.State.PassNull();
                                objCust.Status = "A";// item.IsActive == 1 ? "A" : "I";
                                objCust.TaxDflt = "C";
                                objCust.TaxID00 = "OVAT10-00";
                                objCust.TaxID01 = "OVAT05-00";
                                objCust.TaxID02 = "VAT00";
                                objCust.TaxID03 = "NONEVAT";
                                objCust.Terms = "07";
                                objCust.ParentRecordID = 4;
                                objCust.NodeLevel = 2;
                                objCust.SlsperId = item.SlsperID;
                                objCust.ExpiryDate = DateTime.Now.ToDateShort();
                                objCust.CustType = "R";
                                objCust.EstablishDate = new DateTime(1900, 1, 1);
                                objCust.Birthdate = new DateTime(1900, 1, 1);
                                objCust.RefCustID = "";
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

                                AR_SOAddress objAR_SOAddress = new AR_SOAddress();
                                objAR_SOAddress.ResetET();
                                objAR_SOAddress.Addr1 = objCust.Addr1.PassNull();// item.Addr2.PassNull() + (item.Addr1.PassNull() != "" ? "," + item.Addr1.PassNull() : "");
                                objAR_SOAddress.Addr2 = objCust.Addr2.PassNull();//
                                objAR_SOAddress.Attn = objCust.Attn.PassNull();
                                objAR_SOAddress.BranchID = objCust.BranchID;
                                objAR_SOAddress.City = objCust.City;
                                objAR_SOAddress.Country = objCust.Country;
                                objAR_SOAddress.Crtd_DateTime = objCust.Crtd_Datetime;
                                objAR_SOAddress.Crtd_Prog = objCust.Crtd_Prog;
                                objAR_SOAddress.Crtd_User = objCust.Crtd_User;

                                objAR_SOAddress.CustId = objCust.CustId;
                                objAR_SOAddress.Descr = objCust.CustName;
                                objAR_SOAddress.District = objCust.District;

                                objAR_SOAddress.Fax = objCust.Fax;
                                objAR_SOAddress.LUpd_DateTime = objCust.LUpd_Datetime;
                                objAR_SOAddress.LUpd_Prog = objCust.LUpd_Prog;
                                objAR_SOAddress.LUpd_User = objCust.LUpd_User;

                                objAR_SOAddress.Phone = objCust.Phone;
                                objAR_SOAddress.ShipToId = "DEFAULT";// objCust.CustId.Length > 10 ? objCust.CustId.Substring(objCust.CustId.Length - 10, 10) : objCust.CustId;
                                objAR_SOAddress.ShipViaID = "";
                                objAR_SOAddress.SiteId = objCust.SiteId;
                                objAR_SOAddress.SOName = objCust.CustName;
                                objAR_SOAddress.State = objCust.State;
                                objAR_SOAddress.TaxId00 = objCust.TaxID00;
                                objAR_SOAddress.TaxId01 = objCust.TaxID01;
                                objAR_SOAddress.TaxId02 = objCust.TaxID02;
                                objAR_SOAddress.TaxId01 = objCust.TaxID01;
                                objAR_SOAddress.TaxLocId = objCust.TaxLocId;
                                objAR_SOAddress.TaxRegNbr = objCust.TaxRegNbr;
                                objAR_SOAddress.Zip = objCust.Zip;
                                _db.AR_SOAddress.AddObject(objAR_SOAddress);  
                             
                                OM_SalesRouteMaster master = new OM_SalesRouteMaster();
                                master.ResetET();
                                master.PJPID = item.BranchID;
                                master.SalesRouteID = item.BranchID;
                                master.SlsFreq = item.SlsFreq;
                                master.SlsPerID = item.SlsperID;
                                master.StartDate = fromDate;
                                master.EndDate = toDate;
                                master.VisitSort = item.VisitSort.Value;
                                master.SlsFreqType = "R";
                                master.Crtd_DateTime = DateTime.Now;
                                master.Crtd_Prog = "AR20500";
                                master.Crtd_User = Current.UserName;
                                master.LUpd_DateTime = DateTime.Now;
                                master.LUpd_Prog = "AR20500";
                                master.LUpd_User = Current.UserName;
                                master.CustID = objCust.CustId;
                                master.BranchID = item.BranchID;
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
                            _db.AR20500_AfterApprove(objNew.NewCustID, item.CustID, item.BranchID, handle);
                            _db.Dispose();
                        }
                    }
                }
                if (custlist != "") throw new MessageException(MessageType.Message, "201405281", "", new string[] { custlist.TrimEnd(',') });
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
                    else if (master.SlsFreq == "F4" || master.SlsFreq == "F4A" || master.SlsFreq == "F8" || master.SlsFreq == "F8A" || master.SlsFreq == "F12" || master.SlsFreq == "F16" || master.SlsFreq == "F20" || master.SlsFreq == "F24" || master.SlsFreq == "A")
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
                        // xet cho ngay thu 7, neu la F8A thi tuan di tuan nghi
                        if (master.SlsFreq == "F8A" || master.SlsFreq == "F4A")
                        {
                            if ((master.WeekofVisit == "OW" && (i % 2) != 0) || (master.WeekofVisit == "EW" && (i % 2) == 0) || (master.WeekofVisit == "NA" && (i % 2) == (weekStart % 2)))
                            {
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
                            }
                        }
                        else if (master.Sat && dSat <= todate && dSat >= fromdate)
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
        public ActionResult ImageToBin(string fileName)
        {
            try
            {
                string filename = FilePath + "\\" + fileName;
                if (System.IO.File.Exists(filename))
                {
                    FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    BinaryReader reader = new BinaryReader(fileStream);
                    byte[] imageBytes = reader.ReadBytes((int)fileStream.Length);
                    reader.Close();

                    var imgString64 = Convert.ToBase64String(imageBytes, 0, imageBytes.Length);

                    var jsonResult = Json(new { success = true, imgSrc = @"data:image/jpg;base64," + imgString64 }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                else
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
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

    }
}

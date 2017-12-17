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
using System.Globalization;
using HQ.eSky.RPT.Controllers;
using HQ.eSky.RPT;
using System.Data.EntityClient;
using Stimulsoft.Report;
using System.Configuration;
using Stimulsoft.Report.Dictionary;
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
        RPTEntities _dbRPT = Util.CreateObjectContext<RPTEntities>(false);
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
            var requireRefCustID = false;
            // IntVal = 0 khong check dieu kien - IntVal = 1 co check dieu kien trùng RefCustID
            var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "AR20500CheckRefCust");
            if (config != null && config.FloatVal == 1)
            {
                requireRefCustID = true;
            }
            ViewBag.IsRequireRefCustID = requireRefCustID;
            var isShowCustHT = false;
		    var isShowReason = false;
            var isShowERPCust = false;
            bool isShowExport = false;
            var isShowEditCust = false;
            string allowSave = string.Empty;
            string allowApproveEditCust = string.Empty;
            var allowEditReason = string.Empty;
            var objConfig = _db.AR20500_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                isShowCustHT = objConfig.IsShowCustHT.HasValue ? objConfig.IsShowCustHT.Value : false;
                isShowReason = objConfig.IsShowReason.HasValue ? objConfig.IsShowReason.Value : false;                
                isShowERPCust = objConfig.IsShowERPCust.HasValue ? objConfig.IsShowERPCust.Value : false;
                isShowExport = objConfig.ShowExport.HasValue ? objConfig.ShowExport.Value : false; //an hien nut export
                isShowEditCust = objConfig.ShowEditCust.HasValue ? objConfig.ShowEditCust.Value : false;
                allowSave = objConfig.AllowSave.PassNull();
                allowApproveEditCust = objConfig.AllowApproveEditCust.PassNull();
                allowEditReason = objConfig.AllowEditReason.PassNull();
            }
            ViewBag.IsShowERPCust = isShowERPCust;
            ViewBag.IsShowCustHT = isShowCustHT;
            ViewBag.IsShowReason = isShowReason;
            ViewBag.ShowExport = isShowExport;
            ViewBag.ShowEditCust = isShowEditCust;
            ViewBag.AllowSave = allowSave;
            ViewBag.AllowApproveEditCust = allowApproveEditCust;
            ViewBag.AllowEditReason = allowEditReason;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetCustList(string BranchID, DateTime fromDate, DateTime toDate, List<string> slsperID, string status, List<string> territory, int updateType)
        {
            string sls = string.Empty;
            if (slsperID != null)
            {
                sls = string.Join(",", slsperID);
            }
            string terr = string.Empty;
            if (territory != null)
            {
                terr = string.Join(",", territory);
            }
            var data = _db.AR20500_pgDetail(BranchID, fromDate.PassMin(), toDate.PassMin(), sls, status, terr, updateType, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(data);
        }

        [HttpPost]
        public ActionResult Process(FormCollection data, DateTime fromDate, DateTime toDate, int askApprove)
        {
            try
            {
                _db.CommandTimeout = int.MaxValue;
                string errorCustID = string.Empty;
                mForm = data;
                StoreDataHandler custHandler = new StoreDataHandler(data["lstCust"]);

                var lstCust = custHandler.ObjectData<AR20500_pgDetail_Result>();
               
                var access = Session[_screenNbr] as AccessRight;

                if (!access.Update && !access.Insert)
                    throw new MessageException(MessageType.Message, "728");
                string handle = data["cboHandle"];
                string custlist = "";
                List<string> lstCustHT = new List<string>();

                if (handle != "N" && handle != string.Empty)
                {
                    #region -Check Approve-                                        
                    if (checkApprove == 1 && askApprove == 0 && handle == "A")
                    {
                        var isCheckRefCustID = IsCheckRefCustID();
                        foreach (var item in lstCust)
                        {
                            if (item.ColCheck == true && item.UpdateType == 0)
                            {
                                if (isCheckRefCustID && !string.IsNullOrWhiteSpace(item.ERPCustID))
                                {
                                    string key = (item.BranchID + item.ERPCustID).ToUpper();
                                    if (lstCustHT.Any(x => x == key))
                                    {
                                        throw new MessageException(MessageType.Message, "2017071401", "", parm: new string[] { item.BranchID, item.CustID, item.ERPCustID });
                                    }
                                    else
                                    {
                                        var obj = _db.AR_Customer.FirstOrDefault(x => x.BranchID == item.BranchID && x.RefCustID.ToUpper() == item.ERPCustID.ToUpper());
                                        if (obj != null)
                                        {
                                            throw new MessageException(MessageType.Message, "2017071401", "", parm: new string[] { item.BranchID, item.CustID, item.ERPCustID });
                                        }
                                        if (!lstCustHT.Any(x => x == key))
                                        {
                                            lstCustHT.Add(key);
                                        }
                                    }
                                }
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
                    {
                        throw new MessageException(MessageType.Message, "2016062801", "askApprove", parm: new string[] { errorCustID });
                    }
                    #endregion

                    List<string> lstCustID = new List<string>();
                    foreach (var item in lstCust)
                    {
                        if (item.ColCheck == true)
                        {
                            _db = Util.CreateObjectContext<AR20500Entities>(false);
                            AR_NewCustomerInfor objNew = _db.AR_NewCustomerInfor.FirstOrDefault(p => p.ID == item.ID && p.BranchID == item.BranchID);
                            if (objNew == null || objNew.Status == "A" || objNew.Status == "D") 
                            {
                                continue; 
                            }

                            if (item.NewCustID.PassNull() != string.Empty && objNew.UpdateType != 1)
                            {
                                continue;
                            }
                            var checkApproveEditCust = CheckAllowApproveEditCust(objNew.Status, objNew.UpdateType, handle);
                            var hisLineRef = 0;
                            var objCust = _db.AR_Customer.Where(x => x.CustId == item.NewCustID && x.BranchID == item.BranchID).FirstOrDefault();
                            if (objNew.Status == "H")
                            {
                                if (objCust != null)
                                {
                                    Insert_NewCustHis(objNew, ref hisLineRef, objCust.AllowEdit, objCust.ProfilePic.PassNull() == string.Empty, objCust.BusinessPic.PassNull() == string.Empty);
                                }else
	                            {
                                    Insert_NewCustHis(objNew, ref hisLineRef, 0, objNew.ProfilePic.PassNull() == string.Empty, objNew.BusinessPic.PassNull() == string.Empty);
	                            }
                                
                            }
                            
                            var callAfterApprove = objNew.UpdateType == 0;
                            
                            if (handle == "A")
                            {
                                #region -Duyệt update KH qua AR_Customer...-                                                                
                                string cust = _db.AR20500_ppCheckCustomerApprove(data["cboCpnyID"].ToString(), item.CustID,item.ID,item.OutletName,item.Phone,item.Addr1).FirstOrDefault();
                                if (cust.PassNull() != "")
                                {
                                    custlist += cust.TrimEnd(',') + ",";
                                    _db.Dispose();
                                    continue;
                                    //throw new MessageException(MessageType.Message, "201405281", "", new string[] { cust.TrimEnd(',') });
                                }                                         
                                //if (item.ERPCustID.PassNull() != "" )
                                //{
                                //    string custERP = item.ERPCustID.PassNull().ToUpper().Trim();
                                //    string branchID = item.BranchID.PassNull();
                                //    if (lstCustID.Contains(custERP))
                                //    {
                                //        throw new MessageException(MessageType.Message, "1112", "", parm: new string[] { Util.GetLang("CustID") + " " + item.ERPCustID.PassNull() });
                                //    }

                                //    var objCust1 = _db.AR_Customer.Where(p => p.BranchID == branchID && p.CustId.ToUpper() == custERP).FirstOrDefault();
                                //    if(objCust1!=null)
                                //    {
                                //        throw new MessageException(MessageType.Message, "8001", "", parm: new string[] { Util.GetLang("CustID") + " " + item.ERPCustID.PassNull() });
                                //    }
                                //    lstCustID.Add(item.ERPCustID.PassNull().ToUpper());
                                //}

                                // Update AR_NewCustomerInfor
                                Update_NewCust(ref objNew, item);
                                objNew.Startday = fromDate;
                                objNew.Endday = toDate;                  
                                // Update Customer
                               
                                if (objCust == null)
                                {
                                    objCust = new AR_Customer();
                                    objCust.ResetET();
                                    objCust.CustId = _db.AR20500_CustID(item.BranchID, "", objCust.Territory, objCust.District, "", "", "", "", "", "", objCust.ClassId, item.State.PassNull(), objCust.CustName).FirstOrDefault();//item.ERPCustID.PassNull() != "" ? item.ERPCustID.PassNull() : _db.AR20500_CustID(item.BranchID, "", objCust.Territory, objCust.District, "", "", "", "", "", "", objCust.ClassId, item.State.PassNull(), objCust.CustName).FirstOrDefault();                                    
                                    objCust.BranchID = item.BranchID.PassNull();
                                    objCust.Crtd_Datetime = DateTime.Now;
                                    objCust.Crtd_Prog = _screenNbr;
                                    objCust.Crtd_User = Current.UserName;
                                    _db.AR_Customer.AddObject(objCust);
                                }
                                Update_AR_Customer(ref objCust, item, objNew);                                
                                                           
                                // Customer Location
                                AR_CustomerLocation loc = _db.AR_CustomerLocation.FirstOrDefault(x => x.BranchID == objNew.BranchID && x.CustID == objCust.CustId);                                 
                                if (loc == null)
                                {
                                    loc = new AR_CustomerLocation();
                                    loc.BranchID = objCust.BranchID;
                                    loc.CustID = objCust.CustId;
                                    _db.AR_CustomerLocation.AddObject(loc);
                                }                               
                                loc.Crtd_Datetime = loc.LUpd_Datetime = DateTime.Now;
                                loc.Crtd_Prog = loc.LUpd_Prog = _screenNbr;
                                loc.Crtd_User = loc.LUpd_User = Current.UserName;
                                loc.Lng = item.Lng.Value;
                                loc.Lat = item.Lat.Value;
                                // Địa chỉ
                                AR_SOAddress objAR_SOAddress = _db.AR_SOAddress.FirstOrDefault(x => x.CustId == objCust.CustId && x.BranchID == objCust.BranchID);
                                if (objAR_SOAddress == null)
                                {
                                    objAR_SOAddress = new AR_SOAddress();
                                    objAR_SOAddress.ResetET();
                                    _db.AR_SOAddress.AddObject(objAR_SOAddress);
                                    objAR_SOAddress.BranchID = objCust.BranchID;
                                    objAR_SOAddress.ShipToId = "DEFAULT";
                                    objAR_SOAddress.CustId = objCust.CustId;
                                }
                                Update_SOAddress(ref objAR_SOAddress, objCust);

                                // KH mới thì cập nhật MCP
                                if (item.UpdateType == 0)
                                {
                                    // MCP
                                    OM_SalesRouteMaster master = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == item.BranchID && x.SalesRouteID == item.BranchID && x.SlsPerID == item.SlsperID && x.CustID == objCust.CustId && x.BranchID == item.BranchID);
                                    if (master == null)
                                    {
                                        master = new OM_SalesRouteMaster();
                                        master.ResetET();
                                        _db.OM_SalesRouteMaster.AddObject(master);
                                        master.PJPID = item.BranchID;
                                        master.SalesRouteID = item.BranchID;
                                        master.SlsPerID = item.SlsperID;
                                        master.CustID = objCust.CustId;
                                        master.BranchID = item.BranchID;
                                    }
                                    Update_SalesRouteMaster(ref master, item, fromDate, toDate);
                                    // Tạo tuyến bán hàng                                                  
                                    CreateRoute(master);
                                }
                                // Update NewCustID
                                objNew.NewCustID = objCust.CustId;
                                objNew.Status = "A";                              
                                #endregion
                                if (checkApproveEditCust)
                                {
                                    UpdateEditType(ref objNew, ref objCust, item);
                                }
                               
                            }
                            else if (handle != "N" && handle != "")
                            {
                                // Trạng thái trung gian                                 
                                Update_NewCust(ref objNew, item);
                                objNew.Startday = fromDate;
                                objNew.Endday = toDate;                                
                                objNew.Status = handle;
                                if (checkApproveEditCust)
                                {
                                    objCust = _db.AR_Customer.Where(x => x.CustId == item.NewCustID && x.BranchID == item.BranchID).FirstOrDefault();
                                    if (objCust != null)
                                    {
                                        UpdateEditType(ref objNew, ref objCust, item);
                                    }
                                }
                            }
                            
                            objNew.LUpd_Datetime = DateTime.Now;
                            objNew.LUpd_Prog = _screenNbr;
                            objNew.LUpd_User = Current.UserName;

                            if (objCust != null)
                            {
                                Insert_NewCustHis(objNew, ref hisLineRef, objCust.AllowEdit, objCust.ProfilePic.PassNull() == string.Empty, objCust.BusinessPic.PassNull() == string.Empty);
                            }
                            else
                            {
                                Insert_NewCustHis(objNew, ref hisLineRef, 0, objNew.ProfilePic.PassNull() == string.Empty, objNew.BusinessPic.PassNull() == string.Empty);
                            }
                           // Insert_NewCustHis(objNew, ref hisLineRef, objCust.AllowEdit, item.EditProfilePic.Value, item.EditBusinessPic.Value);

                            _db.SaveChanges();
                            if (callAfterApprove)
                            {
                                _db.AR20500_AfterApprove(objNew.NewCustID, item.CustID, item.BranchID, handle);    
                            }
                            
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
                 if (ex is System.Data.SqlClient.SqlException)
                {
                    return Json(new { success = false, type = "message", code = "2017110301" }); //return Json(new { success = true, message = GetMess(2017110301, null) });
                }
                else if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        private void Update_NewCust(ref AR_NewCustomerInfor objNew, AR20500_pgDetail_Result item)
        {
            if (item.UpdateType == 0)
            {
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
                //objNew.Startday = fromDate;
                //objNew.Endday = toDate;
                objNew.VisitSort = item.VisitSort.Value;
            }
            objNew.SubTerritory = item.SubTerritory;
            objNew.Channel = item.Channel;
            objNew.ShopType = item.ShopType;
            objNew.Location = item.Location;
            objNew.Salut = item.Salut;
            objNew.OutletName = item.OutletName;
            objNew.Phone = item.Phone;
            objNew.Addr1 = item.Addr1;
            objNew.Addr2 = item.Addr2;

            objNew.Territory = item.Territory;
            objNew.State = item.State;
            objNew.District = item.District;
            objNew.Ward = item.Ward;

            objNew.Reason = item.Reason;
            objNew.ClassId = item.ClassId;
            objNew.PriceClass = item.PriceClass;
            objNew.CodeHT = item.ERPCustID;
        }

        private void Insert_NewCustHis(AR_NewCustomerInfor objNew, ref int hisLineRef, int allowEdit, bool isDelProfile, bool isDellBussPic)
        {            
            var lineRefIdx = 1;
            if (hisLineRef > 0)
	        {
                lineRefIdx = lineRefIdx + 1;
            }
            else
            {
                var obj = _db.AR_NewCustomerInforHis.Where(x => x.ID == objNew.ID && x.BranchID == objNew.BranchID).OrderByDescending(x => x.LineRef).FirstOrDefault();
                if (obj != null)
                {
                    lineRefIdx = obj.LineRef + 1;
                }            
            }
            hisLineRef = lineRefIdx;
            var objHis = new AR_NewCustomerInforHis()
            {
                ID = objNew.ID,
                BranchID = objNew.BranchID,
                LineRef = lineRefIdx,
                CustID = objNew.CustID,
                OutletName = objNew.OutletName,
                ContactName = objNew.ContactName,
                Phone = objNew.Phone,
                Mobile = objNew.Mobile,
                Fax = objNew.Fax,
                Email = objNew.Email,
                Addr1 = objNew.Addr1,
                Addr2 = objNew.Addr2,
                Addr3 = objNew.Addr3,
                State = objNew.State,
                City = objNew.City,
                District = objNew.District,
                Channel = objNew.Channel,
                ClassId = objNew.ClassId,
                Area = objNew.Area,
                ShopType = objNew.ShopType,
                TradeType = objNew.TradeType,
                Lat = objNew.Lat,
                Lng = objNew.Lng,
                ImageFileName = objNew.ImageFileName,
                Status = objNew.Status,
                IsActive = objNew.IsActive,
                NewCustCrtd_Datetime = objNew.Crtd_Datetime,
                NewCustLUpd_Datetime = objNew.LUpd_Datetime,
                SlsperID = objNew.SlsperID,
                ApproveStatus = objNew.ApproveStatus,
                NewCustID = objNew.NewCustID,
                Checked = objNew.Checked,
                CustType = objNew.CustType,
                Territory = objNew.Territory,
                Location = objNew.Location,
                PriceClass = objNew.PriceClass,
                Startday = objNew.Startday,
                Endday = objNew.Endday,
                PJPID = objNew.PJPID,
                SlsFreq = objNew.SlsFreq,
                WeekofVisit = objNew.WeekofVisit,
                SalesRouteID = objNew.SalesRouteID,
                Mon = objNew.Mon,
                Tue = objNew.Tue,
                Wed = objNew.Wed,
                Thu = objNew.Thu,
                Fri = objNew.Fri,
                Sat = objNew.Sat,
                Sun = objNew.Sun,
                Ward = objNew.Ward,
                DeliveryID = objNew.DeliveryID,
                DateCust = objNew.DateCust,
                TaxCode = objNew.TaxCode,
                VisitSort = objNew.VisitSort,
                Salut = objNew.Salut,
                ProfilePic = objNew.ProfilePic,
                SubTerritory = objNew.SubTerritory,
                CodeHT = objNew.CodeHT,
                BusinessPic = objNew.BusinessPic,

                NewCustCrtd_User = objNew.Crtd_User,
                NewCustLUpd_User = objNew.LUpd_User,
                NewCustCrtd_Prog = objNew.Crtd_Prog,
                NewCustLUpd_Prog = "AR20500",
                UpdateType = objNew.UpdateType,
                Reason = objNew.Reason,
                AllowEdit = allowEdit,
                DelProfile = isDelProfile,
                DelBusiness = isDellBussPic,
                Crtd_User = Current.UserName,
                Crtd_Prog = "AR20500",
                Crtd_Datetime = DateTime.Now
            };
            _db.AR_NewCustomerInforHis.AddObject(objHis);
        }
        private void Update_SalesRouteMaster(ref OM_SalesRouteMaster master, AR20500_pgDetail_Result item, DateTime fromDate, DateTime toDate)
        {
            master.SlsFreq = item.SlsFreq;
            master.StartDate = fromDate;
            master.EndDate = toDate;
            master.VisitSort = item.VisitSort.Value;
            master.SlsFreqType = "R";
            master.Crtd_DateTime = DateTime.Now;
            master.Crtd_Prog = _screenNbr;
            master.Crtd_User = Current.UserName;
            master.LUpd_DateTime = DateTime.Now;
            master.LUpd_Prog = _screenNbr;
            master.LUpd_User = Current.UserName;

            master.WeekofVisit = item.WeekofVisit;
            master.Mon = item.Mon.ToBool();
            master.Tue = item.Tue.ToBool();
            master.Wed = item.Wed.ToBool();
            master.Thu = item.Thu.ToBool();
            master.Fri = item.Fri.ToBool();
            master.Sat = item.Sat.ToBool();
            master.Sun = item.Sun.ToBool();
        }
        private void Update_AR_Customer(ref AR_Customer objCust, AR20500_pgDetail_Result  item, AR_NewCustomerInfor objNew)
        {
            objCust.Addr1 = objCust.BillAddr1 = item.Addr1.PassNull();// item.Addr2.PassNull() + (item.Addr1.PassNull() != "" ? "," + item.Addr1.PassNull() : "");
            objCust.Addr2 = objCust.BillAddr2 = item.Addr2.PassNull();//
            objCust.District = item.District.PassNull();
            objCust.City = objCust.BillCity = item.City.PassNull();
            objCust.State = objCust.BillState = item.State.PassNull();
//            objCust.State = item.State.PassNull();
            objCust.ClassId = item.ClassId.PassNull();
            objCust.Salut = item.Salut.PassNull();
            objCust.Phone = item.Phone.PassNull();

            objCust.Channel = item.Channel.PassNull();
            objCust.Area = item.Area.PassNull();
            objCust.EMailAddr = item.Email.PassNull();
            objCust.CrRule = "N";            

            ///thôi dị e chỉnh dùm chị AR_Customer.ProfilePic = AR_NewCustomerInfor.imageFileName
            //objCust.ProfilePic = objNew.ProfilePic;
            objCust.ProfilePic = objNew.ImageFileName.PassNull();
            objCust.BusinessPic = objNew.BusinessPic.PassNull();

            objCust.SubTerritory = item.SubTerritory;
            objCust.Territory = item.Territory.PassNull();
            objCust.Attn = objCust.BillAttn = item.ContactName.PassNull();
            objCust.CustName = objCust.BillName = item.OutletName.PassNull();
            objCust.CustType = item.CustType.PassNull(); ;
            objCust.PriceClassID = item.PriceClass.PassNull(); ;
            objCust.Location = item.Location.PassNull(); ;
            objCust.DeliveryID = item.DeliveryID.PassNull(); ;
            objCust.Country = objCust.BillCountry = "VN";
            objCust.BillWard = objCust.Ward = item.Ward;
            
            objCust.DfltShipToId = "DEFAULT";
            if (item.UpdateType == 0)
            {
                objCust.LTTContractNbr = item.CustHT;
                objCust.RefCustID = item.ERPCustID.PassNull();
            }            
            objCust.LUpd_Datetime = DateTime.Now;
            objCust.LUpd_Prog = _screenNbr;
            objCust.LUpd_User = Current.UserName;
            objCust.Phone = objCust.BillPhone = item.Phone.PassNull(); 
            objCust.ShopType = AR20500Anova ? "RS" : item.ShopType.PassNull();
            
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
            objCust.AllowEdit = 0;
        }
        private void Update_SOAddress(ref AR_SOAddress objAR_SOAddress, AR_Customer objCust)
        {
            objAR_SOAddress.Addr1 = objCust.Addr1.PassNull();// item.Addr2.PassNull() + (item.Addr1.PassNull() != "" ? "," + item.Addr1.PassNull() : "");
            objAR_SOAddress.Addr2 = objCust.Addr2.PassNull();//
            objAR_SOAddress.Attn = objCust.Attn.PassNull();
            
            objAR_SOAddress.City = objCust.City;
            objAR_SOAddress.Country = objCust.Country;
            objAR_SOAddress.Crtd_DateTime = objCust.Crtd_Datetime;
            objAR_SOAddress.Crtd_Prog = objCust.Crtd_Prog;
            objAR_SOAddress.Crtd_User = objCust.Crtd_User;
            
            objAR_SOAddress.Descr = objCust.CustName;
            objAR_SOAddress.District = objCust.District;

            objAR_SOAddress.Fax = objCust.Fax;
            objAR_SOAddress.LUpd_DateTime = objCust.LUpd_Datetime;
            objAR_SOAddress.LUpd_Prog = objCust.LUpd_Prog;
            objAR_SOAddress.LUpd_User = objCust.LUpd_User;

            objAR_SOAddress.Phone = objCust.Phone;
           
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
        }
        public void CreateRoute(OM_SalesRouteMaster master)//(clsOM_SalesRouteMaster objSaleMaster)
        {
            Int32 weekStart = default(Int32);
            Int32 weekEnd = default(Int32);
            var FromDate = master.StartDate.Value;
            var ToDate = master.EndDate.Value;

            var dMon = default(System.DateTime);
            var dTue = default(System.DateTime);
            var dWed = default(System.DateTime);
            var dThu = default(System.DateTime);
            var dFri = default(System.DateTime);
            var dSat = default(System.DateTime);
            var dSun = default(System.DateTime);
            _db.AR20500_DeleteSalesRouteDetByDate(FromDate, ToDate, master.SalesRouteID, master.CustID, master.BranchID);
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = new DateTime(2011, 1, 1);
            Calendar cal = dfi.Calendar;
            int subYear = ToDate.Year - FromDate.Year;
            DateTime TodateTmp = ToDate;
            for (int y = 0; y <= subYear; y++)
            {

                int nextyear = FromDate.Year + 1;
                if (y != 0)
                {
                    FromDate = new DateTime(nextyear, 1, 1);
                    ToDate = new DateTime(nextyear, 12, 31);
                }
                else ToDate = new DateTime(FromDate.Year, 12, 31);
                if (y == subYear)
                {
                    ToDate = TodateTmp;
                }
                int iWeekStart = cal.GetWeekOfYear(FromDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                int iWeekStartStart = cal.GetWeekOfYear(FromDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                int iWeekEnd = cal.GetWeekOfYear(ToDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

                if (master.SlsFreqType == "R")
                {
                    for (Int32 i = iWeekStart; i <= iWeekEnd; i++)
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
                        dMon = GetDateFromDayofWeek(FromDate.Year, i, "Monday");
                        dTue = GetDateFromDayofWeek(FromDate.Year, i, "Tuesday");
                        dWed = GetDateFromDayofWeek(FromDate.Year, i, "Wednesday");
                        dThu = GetDateFromDayofWeek(FromDate.Year, i, "Thursday");
                        dFri = GetDateFromDayofWeek(FromDate.Year, i, "Friday");
                        dSat = GetDateFromDayofWeek(FromDate.Year, i, "Saturday");
                        dSun = GetDateFromDayofWeek(FromDate.Year, i, "Sunday");
                        if (master.SlsFreq == "F1")
                        {
                            if ((master.WeekofVisit == "W159" && (i % 4) == 1) || (master.WeekofVisit == "W2610" && (i % 4) == 2) || (master.WeekofVisit == "W3711" && (i % 4) == 3) || (master.WeekofVisit == "W4812" && (i % 4) == 0))
                            {
                                if (master.Mon && dMon <= ToDate && dMon >= FromDate)
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
                                if (master.Tue && dTue <= ToDate && dTue >= FromDate)
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
                                if (master.Wed && dWed <= ToDate && dWed >= FromDate)
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
                                if (master.Thu && dThu <= ToDate && dThu >= FromDate)
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
                                if (master.Fri && dFri <= ToDate && dFri >= FromDate)
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
                                if (master.Sat && dSat <= ToDate && dSat >= FromDate)
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
                                if (master.Sun && dSun <= ToDate && dSun >= FromDate)
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
                                if (master.Mon && dMon <= ToDate && dMon >= FromDate)
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
                                if (master.Tue && dTue <= ToDate && dTue >= FromDate)
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
                                if (master.Wed && dWed <= ToDate && dWed >= FromDate)
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
                                if (master.Thu && dThu <= ToDate && dThu >= FromDate)
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
                                if (master.Fri && dFri <= ToDate && dFri >= FromDate)
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
                                if (master.Sat && dSat <= ToDate && dSat >= FromDate)
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
                                if (master.Sun && dSun <= ToDate && dSun >= FromDate)
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
                            if (master.Mon && dMon <= ToDate && dMon >= FromDate)
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
                            if (master.Tue && dTue <= ToDate && dTue >= FromDate)
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
                            if (master.Wed && dWed <= ToDate && dWed >= FromDate)
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
                            if (master.Thu && dThu <= ToDate && dThu >= FromDate)
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
                            if (master.Fri && dFri <= ToDate && dFri >= FromDate)
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
                                    if (master.Sat && dSat <= ToDate && dSat >= FromDate)
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
                            else if (master.Sat && dSat <= ToDate && dSat >= FromDate)
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
                            if (master.Sun && dSun <= ToDate && dSun >= FromDate)
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
                        dMon = GetDateFromDayofWeek(FromDate.Year, i, "Monday");
                        dTue = GetDateFromDayofWeek(FromDate.Year, i, "Tuesday");
                        dWed = GetDateFromDayofWeek(FromDate.Year, i, "Wednesday");
                        dThu = GetDateFromDayofWeek(FromDate.Year, i, "Thursday");
                        dFri = GetDateFromDayofWeek(FromDate.Year, i, "Friday");
                        dSat = GetDateFromDayofWeek(FromDate.Year, i, "Saturday");
                        dSun = GetDateFromDayofWeek(FromDate.Year, i, "Sunday");
                        if (master.Mon && dMon <= ToDate && dMon >= FromDate)
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
                        if (master.Tue && dTue <= ToDate && dTue >= FromDate)
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
                        if (master.Wed && dWed <= ToDate && dWed >= FromDate)
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
                        if (master.Thu && dThu <= ToDate && dThu >= FromDate)
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
                        if (master.Fri && dFri <= ToDate && dFri >= FromDate)
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
                        if (master.Sat && dSat <= ToDate && dSat >= FromDate)
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
                        if (master.Sun && dSun <= ToDate && dSun >= FromDate)
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
                var imgString64 = Util.ImageToBin(FilePath, fileName);
                var jsonResult = Json(new { success = true, imgSrc = imgString64 }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
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

        private bool IsCheckRefCustID()
        {
            var _checkRefCustID = false;
            // IntVal = 0 khong check dieu kien - IntVal = 1 co check dieu kien trùng RefCustID
            var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "AR20500CheckRefCust");
            if (config != null && config.IntVal == 1)
            {
                _checkRefCustID = true;
            }
            return _checkRefCustID;
        }

        [HttpPost]
        public ActionResult ExportExcel(FormCollection data, string reportNbr, string ReportName)
        {
            ////RPTController rpt = new RPTController();
            ////return rpt.ExportExcelDirect(185, "IN_ItemList");
            //string ReportName = "IN_ItemList";
            int ReportID = UpdateRPT(data, reportNbr, ReportName);


            var objRPT_peGetName = _dbRPT.RPT_peGetName(ReportID).FirstOrDefault();
            string nameReport = objRPT_peGetName.NameExport == "" ? ReportName + ReportID : objRPT_peGetName.NameExport;

            string strConnection = EntityConnectionStringHelper.Build(
                                          Current.Server,
                                          Current.DBApp,
                                          "HQ.eSkySysModel");
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder(strConnection);

            var report = new StiReport();

            report.Load(Server.MapPath("~/Reports/" + ReportName + ".mrt"));

            string provider = ConfigurationManager.AppSettings["SQLNCLI"] ?? "SQLNCLI11.0";
            report.Dictionary.Databases.Clear();
            //report.Dictionary.Databases.Add(new Stimulsoft.Report.Dictionary.StiSqlDatabase("Data", entityBuilder.ProviderConnectionString + ";Connect Timeout=60000" + (objRPT_peGetName.isReadOnly != "1" ? "" : ";ApplicationIntent=ReadOnly")));//
            report.Dictionary.Databases.Add(new Stimulsoft.Report.Dictionary.StiOleDbDatabase("Data", @"Provider=" + provider + ";" + entityBuilder.ProviderConnectionString + ";Connect Timeout=60000;General Timeout=6000" + (objRPT_peGetName.isReadOnly != "1" ? "" : ";Application Intent=ReadOnly")));//Data Source=MARSSVR\SQL2012;Initial Catalog=eBiz4DWebApp;Persist Security Info=True;User ID=sa;Password=P@ssw0rd;MultipleActiveResultSets=True;Application Name=EntityFramework"));           

            for (int j = 0; j < report.Dictionary.DataSources.Count; j++)
            {
                ((StiOleDbSource)report.Dictionary.DataSources[j]).CommandTimeout = 600000;
                ((StiOleDbSource)report.Dictionary.DataSources[j]).Parameters[0].Expression = ReportID.ToString();
                ((StiOleDbSource)report.Dictionary.DataSources[j]).Parameters[0].Type = 2;
                ((StiOleDbSource)report.Dictionary.DataSources[j]).SqlCommand = ((StiOleDbSource)report.Dictionary.DataSources[j]).ToString();
            }
            report.Compile();

            report.Render(false);
            //report.Dictionary.Variables.Clear();
            Stream stream = new MemoryStream();
            //report.ExportDocument(StiExportFormat.Excel2007, stream);
            //stream.Flush();
            //stream.Position = 0;
            //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = nameReport + ".xlsx" };
            
            var fileName = nameReport + DateTime.Now.ToString("yyyyMMddHHmm")+ReportID + ".xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
            report.ExportDocument(StiExportFormat.Excel2007, fullPath);
            return Json(new { success = true, fileName = fileName, errorMessage = "" });
        }
        [HttpGet]
        [DeleteFileAttribute] //Action Filter, it will auto delete the file after download,I will explain it later
        public ActionResult DownloadAndDelete(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.ms-excel", file);
        }

        private int UpdateRPT(FormCollection data, string reportNbr, string reportName)
        {

            DateTime DateParm00 = Convert.ToDateTime(data["FromDate"]);
            DateTime DateParm01 = Convert.ToDateTime(data["ToDate"]);
            string branchID = data["cboCpnyID"];
            string TypeCust = data["cboUpdateType"];
            string Status = data["cboStatus"];
            string list0 = data["cboSlsperId"];
            var created = new RPTRunning();
            created.ResetET();
            short bit1 = 1;
            short bit0 = 0;
            created.AppPath = "Reports\\";
            created.BooleanParm00 = data["chk00"].PassNull() != "" ? bit1 : bit0;
            created.BooleanParm01 = data["chk01"].PassNull() != "" ? bit1 : bit0;
            created.BooleanParm02 = data["chk02"].PassNull() != "" ? bit1 : bit0;
            created.BooleanParm03 = data["chk03"].PassNull() != "" ? bit1 : bit0;
            created.ClientName = Current.UserName;
            created.CpnyID = _sys.Users.Where(p => p.UserName == Current.UserName).FirstOrDefault() == null ? "@@" : _sys.Users.Where(p => p.UserName == Current.UserName).FirstOrDefault().CpnyID;
            created.DateParm00 = new DateTime(DateParm00.Year, DateParm00.Month, DateParm00.Day, 0, 0, 0);
            created.DateParm01 = new DateTime(DateParm01.Year, DateParm01.Month, DateParm01.Day, 0, 0, 0);
            created.LangID = Current.LangID;
            created.LoggedCpnyID = Current.CpnyID;
            created.MachineName = "Web";
            created.ReportCap = reportNbr;
            created.ReportDate = DateTime.Now; ;
            created.ReportID = 0;
            created.ReportName = reportName;
            created.ReportNbr = reportNbr;
            created.SelectionFormular = "";
            created.StringParm00 = branchID;// data["StringParm00"];
            created.StringParm01 = TypeCust;// data["StringParm01"];
            created.StringParm02 = Status;// data["StringParm02"];
            //created.StringParm03 = data["StringParm03"];
            created.UserID = Current.UserName;

            _dbRPT.RPTRunnings.AddObject(created);
            _dbRPT.SaveChanges();



            #region//Insert RptParm0 RPTRunningParm0 parm0;luoi nhan vien
            int i = 0;

            foreach (var ID in list0.PassNull().TrimEnd(',').Split(','))
            {
                if (ID.PassNull() != "")
                {
                    i++;
                    var parm0 = new RPTRunningParm0();
                    parm0.ReportNbr = created.ReportNbr;
                    parm0.ReportID = created.ReportID;
                    parm0.MachineName = "Web";
                    parm0.LineRef = i.ToString();
                    parm0.StringParm = ID;
                    parm0.DateParm = DateTime.Now;
                    parm0.NumericParm = 0;
                    parm0.Crtd_DateTime = DateTime.Now;
                    parm0.Crtd_Prog = created.ReportNbr;
                    parm0.Crtd_User = Current.UserName;
                    parm0.LUpd_DateTime = DateTime.Now;
                    parm0.LUpd_Prog = created.ReportNbr;
                    parm0.LUpd_User = Current.UserName;
                    parm0.tstamp = new byte[1];
                    _dbRPT.RPTRunningParm0.AddObject(parm0);
                }

            }
            #endregion

            _dbRPT.SaveChanges();
            return created.ReportID;
        }

       /////////////////////////////////////////////////////////////////////////////////////
        [HttpPost]
        public ActionResult SaveEdit(FormCollection data)
        {
            try
            {
               // _db.CommandTimeout = int.MaxValue;
                string errorCustID = string.Empty;
                StoreDataHandler custHandler = new StoreDataHandler(data["lstCust"]);

                var lstCust = custHandler.ObjectData<AR20500_pgDetail_Result>();

                var access = Session[_screenNbr] as AccessRight;
                if (!access.Update && !access.Insert)
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                foreach (var item in lstCust)
                {
                    if (item.ColCheck == true)
                    {
                        var objNew = _db.AR_NewCustomerInfor.FirstOrDefault(p => p.ID == item.ID && p.BranchID == item.BranchID);
                        var objCust = _db.AR_Customer.Where(x => x.CustId == item.NewCustID && x.BranchID == item.BranchID).FirstOrDefault();
                        if (objNew == null || objCust == null) 
                        {
                            continue; 
                        }
                        var checkApproveEditCust = CheckAllowApproveEditCust(objNew.Status, objNew.UpdateType, string.Empty);
                        if (checkApproveEditCust)
                        {                            
                                // Trạng thái trung gian 
                            Update_NewCust(ref objNew, item);                            
                            int allowEdit = UpdateEditType(ref objNew, ref objCust, item);
                            
                            objNew.LUpd_Datetime = DateTime.Now;
                            objNew.LUpd_Prog = _screenNbr;
                            objNew.LUpd_User = Current.UserName;

                            int hisLineRef = 0;
                            Insert_NewCustHis(objNew, ref hisLineRef, allowEdit, item.EditProfilePic.Value, item.EditBusinessPic.Value); // History  

                            _db.SaveChanges();
                        }
                    }
                }
                
                if (mLogMessage != null)
                {
                    return mLogMessage;
                }
                else
                {
                    return Json(new { success = true, type = "message", code = "8009" });
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Data.SqlClient.SqlException)
                {
                    return Json(new { success = false, type = "message", code = "2017110301" }); //return Json(new { success = true, message = GetMess(2017110301, null) });
                }
                else if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        private int UpdateEditType(ref AR_NewCustomerInfor objNew, ref AR_Customer objCust, AR20500_pgDetail_Result item)
        {
            int allowEdit = 0;
            int editType = 0;
            if (item.AllowChangeEditInfo == true && item.EditInfo == true)
            {
                editType += 1; // Sửa thông tin
            }
            if (item.AllowChangeEditInfo == true && item.EditBusinessPic == true || item.AllowChangeEditInfo == true && item.EditProfilePic == true)
            {
                editType += 2; // Sửa hình ảnh
                if (item.AllowChangeEditInfo == true && item.EditBusinessPic == true)
                {
                    objCust.BusinessPic = string.Empty;
                    objNew.BusinessPic = string.Empty;
                }
                if (item.AllowChangeEditInfo == true && item.EditProfilePic == true)
                {
                    objCust.ProfilePic = string.Empty;
                    objNew.ImageFileName = string.Empty;
                }
            }

            switch (editType)
            {
                case 1:
                    objCust.AllowEdit = 2; // Sửa hình
                    allowEdit = 2;
                    break;
                case 2:
                    objCust.AllowEdit = 3; // Sửa thông tin
                    allowEdit = 3;
                    break;
                case 3:
                    objCust.AllowEdit = 1; // Sửa hình và thông tin
                    allowEdit = 1;
                    break;
                default:
                    break;
            }
            return allowEdit;
        }

        private bool CheckAllowApproveEditCust(string status, int updateType, string toStatus)
        {
            string value = status + updateType + toStatus;
            var objConfig = _db.AR20500_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                string[] items = objConfig.AllowApproveEditCust.PassNull().Split(',');
                for (var i = 0; i < items.Length; i++)
                {
                    if (items[i] == value)
                    {
                        return true;
                    }
                }
                value = status + updateType;
                items = objConfig.AllowSave.PassNull().Split(',');
                for (var i = 0; i < items.Length; i++)
                {
                    if (items[i] == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    public class DeleteFileAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Flush();

            //convert the current filter context to file and get the file path
            string filePath = (filterContext.Result as FilePathResult).FileName;

            //delete the file after download
            System.IO.File.Delete(filePath);
        }
    }
}

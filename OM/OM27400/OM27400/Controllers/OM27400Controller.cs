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
using HQFramework.DAL;
using HQFramework;
using HQ.eSkyFramework.HQControl;
using System.Drawing;
using Aspose.Cells;
using HQ.eSkySys;

namespace OM27400.Controllers
{
    [DirectController]
    public class OM27400Controller : Controller
    {
        private string _screenNbr = "OM27400";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        private JsonResult _logMessage;

        OM27400Entities _db = Util.CreateObjectContext<OM27400Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            var objConfig = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "OM27400TabCondition");
            if (objConfig != null)
                ViewBag.ShowTabCondition = objConfig.IntVal;
            else
                ViewBag.ShowTabCondition = 0;
            return View();
        }
        
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        #region -Get data-                

        public ActionResult GetMCCodeHeader(string QuarterNbr, string KPI)
        {
            var MCCodeHeader = _db.OM_KPIQuarterHeader.FirstOrDefault(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI);
            return this.Store(MCCodeHeader);
        }

        public ActionResult GetKPICpny_All(string QuarterNbr, string KPI, string Zone, string Territory)
        {
            return this.Store(_db.OM27400_pgKPICpny_All(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPICpny_Class(string QuarterNbr, string KPI, string Zone, string Territory)
        {
            return this.Store(_db.OM27400_pgKPICpny_Class(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPICpny_Invt(string QuarterNbr, string KPI, string Zone, string Territory)
        {
            return this.Store(_db.OM27400_pgKPICpny_Invt(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPISales_All(string QuarterNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_All = _db.OM27400_pgKPISales_All(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_All);
            //return this.Store(_db.OM27400_pgKPISales_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPICustomer_All(string QuarterNbr, string KPI, string Zone, string Territory)
		{
            var lstKPISales_All = _db.OM27400_pgKPICustomer_All(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList();
			return this.Store(lstKPISales_All);
            //return this.Store(_db.OM27400_pgKPISales_All(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList());
		}
        public ActionResult GetKPICustomer_Class(string QuarterNbr, string KPI, string Zone, string Territory)
		{
            var lstKPISales_Class = _db.OM27400_pgKPICustomer_Class(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList();
			return this.Store(lstKPISales_Class);
            //return this.Store(_db.OM27400_pgKPISales_Class(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList());
		}
        public ActionResult GetKPICustomer_Invt(string QuarterNbr, string KPI, string Zone, string Territory)
		{
            var lstKPISales_Invt = _db.OM27400_pgKPICustomer_Invt(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList();
			return this.Store(lstKPISales_Invt);
		}
        public ActionResult GetKPISales_Class(string QuarterNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_Class = _db.OM27400_pgKPISales_Class(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_Class);
            //return this.Store(_db.OM27400_pgKPISales_Class(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPISales_Invt(string QuarterNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_Invt = _db.OM27400_pgKPISales_Invt(Current.UserName, Current.CpnyID, Current.LangID, QuarterNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_Invt);
        }
        public ActionResult GetKPICondition(string QuarterNbr, string KPI)
        {
            var lstKPICondition = _db.OM27400_pgKPICondition(QuarterNbr, KPI, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstKPICondition);
        }
        #endregion

        #region -Save delete-
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string QuarterNbr = data["cboQuarterNbr"].PassNull();
                string KPI = data["cboKPI"].PassNull();
				string handle = data["cboHandle"].PassNull();


                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstMCCode"]);
                var curHeader = dataHandler1.ObjectData<OM_KPIQuarterHeader>().FirstOrDefault();

                StoreDataHandler dataHandler_grid = new StoreDataHandler(data["stoOM_KPIQuarterCpny_All"]);
                ChangeRecords<OM27400_pgKPICpny_All_Result> stoOM_KPIQuarterCpny_All = dataHandler_grid.BatchObjectData<OM27400_pgKPICpny_All_Result>();
                stoOM_KPIQuarterCpny_All.Created.AddRange(stoOM_KPIQuarterCpny_All.Updated);// Dua danh sach update chung vao danh sach tao moi

                StoreDataHandler dataHander_grid1 = new StoreDataHandler(data["stoOM_KPIQuarterCpny_Class"]);
                ChangeRecords<OM27400_pgKPICpny_Class_Result> stoOM_KPIQuarterCpny_Class = dataHander_grid1.BatchObjectData<OM27400_pgKPICpny_Class_Result>();
                stoOM_KPIQuarterCpny_Class.Created.AddRange(stoOM_KPIQuarterCpny_Class.Updated);// Dua danh sach update chung vao danh sach tao moi

                StoreDataHandler dataHander_grid2 = new StoreDataHandler(data["stoOM_KPIQuarterCpny_Invt"]);
                ChangeRecords<OM27400_pgKPICpny_Invt_Result> stoOM_KPIQuarterCpny_Invt = dataHander_grid2.BatchObjectData<OM27400_pgKPICpny_Invt_Result>();
                stoOM_KPIQuarterCpny_Invt.Created.AddRange(stoOM_KPIQuarterCpny_Invt.Updated);

                StoreDataHandler dataHander_grid3 = new StoreDataHandler(data["stoOM_KPIQuarterSales_All"]);
                ChangeRecords<OM27400_pgKPISales_All_Result> stoOM_KPIQuarterSales_All = dataHander_grid3.BatchObjectData<OM27400_pgKPISales_All_Result>();
                stoOM_KPIQuarterSales_All.Created.AddRange(stoOM_KPIQuarterSales_All.Updated);

                StoreDataHandler dataHander_grid4 = new StoreDataHandler(data["stoOM_KPIQuarterSales_Class"]);
                ChangeRecords<OM27400_pgKPISales_Class_Result> stoOM_KPIQuarterSales_Class = dataHander_grid4.BatchObjectData<OM27400_pgKPISales_Class_Result>();
                stoOM_KPIQuarterSales_Class.Created.AddRange(stoOM_KPIQuarterSales_Class.Updated);

                StoreDataHandler dataHander_grid5 = new StoreDataHandler(data["stoOM_KPIQuarterSales_Invt"]);
                ChangeRecords<OM27400_pgKPISales_Invt_Result> stoOM_KPIQuarterSales_Invt = dataHander_grid5.BatchObjectData<OM27400_pgKPISales_Invt_Result>();
                stoOM_KPIQuarterSales_Invt.Created.AddRange(stoOM_KPIQuarterSales_Invt.Updated);

				StoreDataHandler dataHander_grid6 = new StoreDataHandler(data["stoOM_KPIQuarterCustomer_All"]);
				ChangeRecords<OM27400_pgKPICustomer_All_Result> stoOM_KPIQuarterCustomer_All = dataHander_grid6.BatchObjectData<OM27400_pgKPICustomer_All_Result>();
				stoOM_KPIQuarterCustomer_All.Created.AddRange(stoOM_KPIQuarterCustomer_All.Updated);

				StoreDataHandler dataHander_grid7 = new StoreDataHandler(data["stoOM_KPIQuarterCustomer_Class"]);
				ChangeRecords<OM27400_pgKPICustomer_Class_Result> stoOM_KPIQuarterCustomer_Class = dataHander_grid7.BatchObjectData<OM27400_pgKPICustomer_Class_Result>();
				stoOM_KPIQuarterCustomer_Class.Created.AddRange(stoOM_KPIQuarterCustomer_Class.Updated);

				StoreDataHandler dataHander_grid8 = new StoreDataHandler(data["stoOM_KPIQuarterCustomer_Invt"]);
				ChangeRecords<OM27400_pgKPICustomer_Invt_Result> stoOM_KPIQuarterCustomer_Invt = dataHander_grid8.BatchObjectData<OM27400_pgKPICustomer_Invt_Result>();
				stoOM_KPIQuarterCustomer_Invt.Created.AddRange(stoOM_KPIQuarterCustomer_Invt.Updated);

                StoreDataHandler dataHander_gridCondition = new StoreDataHandler(data["stoOM_KPIQuarterCondition"]);
                ChangeRecords<OM27400_pgKPICondition_Result> stoOM_KPIQuarterCondition = dataHander_gridCondition.BatchObjectData<OM27400_pgKPICondition_Result>();
                stoOM_KPIQuarterCondition.Created.AddRange(stoOM_KPIQuarterCondition.Updated);

                #region OM_KPIQuarterHeader
                var header = _db.OM_KPIQuarterHeader.FirstOrDefault(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI);

                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {

                        UpdatingHeader(ref header, curHeader);
                        if (handle != "N" && handle != "")
                        {
                            if(handle=="C")
                            {
                                header.Status = handle;
                            }
							if (handle == "A")
							{
								header.Status = "C";
							}
                            if (handle == "H")
                            {
                                header.Status = handle;
                            }
                            //header.Status = handle;
                            if (handle == "O")
                            {
                                header.Status = "W";
                            }
                            if (handle == "R")
                            {
                                header.Status = "D";
                            }
                        }

                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }

                }
                else
                {
                    header = new OM_KPIQuarterHeader();
                    header.ResetET();
                    //header.Status = handle;
                    header.QuarterNbr = QuarterNbr;
                    header.KPI = KPI;                   
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader);
                    _db.OM_KPIQuarterHeader.AddObject(header);
                }
                #endregion

                #region OM_KPIQuarterCpny_All
                foreach (OM27400_pgKPICpny_All_Result del in stoOM_KPIQuarterCpny_All.Deleted)
                {
                    if (stoOM_KPIQuarterCpny_All.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == del.KPI.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPIQuarterCpny_All.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPIQuarterCpny_All.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPIQuarterCpny_All.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM27400_pgKPICpny_All_Result curItem in stoOM_KPIQuarterCpny_All.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterCpny_All.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPIQuarterCpny_All(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPIQuarterCpny_All();
                        Update_OM_KPIQuarterCpny_All(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
                        objCountry.KPI = KPI;
                        
                        _db.OM_KPIQuarterCpny_All.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM27400_pgKPICpny_Class
                foreach (OM27400_pgKPICpny_Class_Result del in stoOM_KPIQuarterCpny_Class.Deleted)
                {
                    if (stoOM_KPIQuarterCpny_Class.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPIQuarterCpny_Class.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPIQuarterCpny_Class.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPIQuarterCpny_Class.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM27400_pgKPICpny_Class_Result curItem in stoOM_KPIQuarterCpny_Class.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterCpny_Class.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.ClassID.ToUpper() == curItem.ClassID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPIQuarterCpny_Class(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPIQuarterCpny_Class();
                        Update_OM_KPIQuarterCpny_Class(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPIQuarterCpny_Class.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM27400_pgKPICpny_Invt
                foreach (OM27400_pgKPICpny_Invt_Result del in stoOM_KPIQuarterCpny_Invt.Deleted)
                {
                    if (stoOM_KPIQuarterCpny_Invt.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPIQuarterCpny_Invt.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPIQuarterCpny_Invt.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPIQuarterCpny_Invt.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM27400_pgKPICpny_Invt_Result curItem in stoOM_KPIQuarterCpny_Invt.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterCpny_Invt.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.InvtID.ToUpper() == curItem.InvtID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPIQuarterCpny_Invt(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPIQuarterCpny_Invt();
                        Update_OM_KPIQuarterCpny_Invt(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPIQuarterCpny_Invt.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPIQuarterSales_All
                foreach (OM27400_pgKPISales_All_Result del in stoOM_KPIQuarterSales_All.Deleted)
                {
                    if (stoOM_KPIQuarterSales_All.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPIQuarterSales_All.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPIQuarterSales_All.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPIQuarterSales_All.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM27400_pgKPISales_All_Result curItem in stoOM_KPIQuarterSales_All.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterSales_All.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPIQuarterSales_All(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPIQuarterSales_All();
                        Update_OM_KPIQuarterSales_All(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPIQuarterSales_All.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPIQuarterSales_Class
                foreach (OM27400_pgKPISales_Class_Result del in stoOM_KPIQuarterSales_Class.Deleted)
                {
                    if (stoOM_KPIQuarterSales_Class.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPIQuarterSales_Class.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPIQuarterSales_Class.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPIQuarterSales_Class.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM27400_pgKPISales_Class_Result curItem in stoOM_KPIQuarterSales_Class.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterSales_Class.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.ClassID.ToUpper() == curItem.ClassID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPIQuarterSales_Class(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPIQuarterSales_Class();
                        Update_OM_KPIQuarterSales_Class(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPIQuarterSales_Class.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPIQuarterSales_Invt
                foreach (OM27400_pgKPISales_Invt_Result del in stoOM_KPIQuarterSales_Invt.Deleted)
                {
                    if (stoOM_KPIQuarterSales_Invt.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPIQuarterSales_Invt.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPIQuarterSales_Invt.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper() 
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPIQuarterSales_Invt.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM27400_pgKPISales_Invt_Result curItem in stoOM_KPIQuarterSales_Invt.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterSales_Invt.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper() 
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.InvtID.ToUpper() == curItem.InvtID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPIQuarterSales_Invt(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPIQuarterSales_Invt();
                        Update_OM_KPIQuarterSales_Invt(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
                        objCountry.KPI = KPI;
                        
                        _db.OM_KPIQuarterSales_Invt.AddObject(objCountry);
                    }
                }
                #endregion

				#region OM_KPIQuarterCustomer_All
				foreach (OM27400_pgKPICustomer_All_Result del in stoOM_KPIQuarterCustomer_All.Deleted)
				{
					if (stoOM_KPIQuarterCustomer_All.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
						&& p.KPI.ToUpper() == del.KPI.ToUpper()&&p.CustID.ToUpper()==del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
					{
						stoOM_KPIQuarterCustomer_All.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
					}
					else
					{
                        var objDel = _db.OM_KPIQuarterCustomer_All.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault();
						if (objDel != null)
						{
							_db.OM_KPIQuarterCustomer_All.DeleteObject(objDel);
						}
					}
				}



				foreach (OM27400_pgKPICustomer_All_Result curItem in stoOM_KPIQuarterCustomer_All.Created)
				{
					if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterCustomer_All.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper()
						&& p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper()).FirstOrDefault();

					if (objCountry != null)
					{
						if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
						{
							Update_OM_KPIQuarterCustomer_All(objCountry, curItem, false);
						}
						else
						{
							throw new MessageException(MessageType.Message, "19");
						}
					}
					else
					{
						objCountry = new OM_KPIQuarterCustomer_All();
						Update_OM_KPIQuarterCustomer_All(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
						objCountry.KPI = KPI;

						_db.OM_KPIQuarterCustomer_All.AddObject(objCountry);
					}
				}
				#endregion

				#region OM_KPIQuarterCustomer_Class
				foreach (OM27400_pgKPICustomer_Class_Result del in stoOM_KPIQuarterCustomer_Class.Deleted)
				{
					if (stoOM_KPIQuarterCustomer_Class.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
						&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
					{
						stoOM_KPIQuarterCustomer_Class.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
					}
					else
					{
                        var objDel = _db.OM_KPIQuarterCustomer_Class.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault();
						if (objDel != null)
						{
							_db.OM_KPIQuarterCustomer_Class.DeleteObject(objDel);
						}
					}
				}



				foreach (OM27400_pgKPICustomer_Class_Result curItem in stoOM_KPIQuarterCustomer_Class.Created)
				{
					if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterCustomer_Class.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper()
						&& p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.ClassID.ToUpper() == curItem.ClassID.ToUpper()).FirstOrDefault();

					if (objCountry != null)
					{
						if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
						{
							Update_OM_KPIQuarterCustomer_Class(objCountry, curItem, false);
						}
						else
						{
							throw new MessageException(MessageType.Message, "19");
						}
					}
					else
					{
						objCountry = new OM_KPIQuarterCustomer_Class();
						Update_OM_KPIQuarterCustomer_Class(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
						objCountry.KPI = KPI;

						_db.OM_KPIQuarterCustomer_Class.AddObject(objCountry);
					}
				}
				#endregion

				#region OM_KPIQuarterCustomer_Invt
				foreach (OM27400_pgKPICustomer_Invt_Result del in stoOM_KPIQuarterCustomer_Invt.Deleted)
				{
					if (stoOM_KPIQuarterCustomer_Invt.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
						&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
					{
						stoOM_KPIQuarterCustomer_Invt.Created.Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
					}
					else
					{
                        var objDel = _db.OM_KPIQuarterCustomer_Invt.ToList().Where(p => p.BranchID == del.BranchID && p.QuarterNbr.ToUpper() == del.QuarterNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault();
						if (objDel != null)
						{
							_db.OM_KPIQuarterCustomer_Invt.DeleteObject(objDel);
						}
					}
				}

				foreach (OM27400_pgKPICustomer_Invt_Result curItem in stoOM_KPIQuarterCustomer_Invt.Created)
				{
					if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPIQuarterCustomer_Invt.Where(p => p.BranchID == curItem.BranchID && p.QuarterNbr.ToUpper() == curItem.QuarterNbr.ToUpper()
						&& p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.InvtID.ToUpper() == curItem.InvtID.ToUpper()).FirstOrDefault();

					if (objCountry != null)
					{
						if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
						{
							Update_OM_KPIQuarterCustomer_Invt(objCountry, curItem, false);
						}
						else
						{
							throw new MessageException(MessageType.Message, "19");
						}
					}
					else
					{
						objCountry = new OM_KPIQuarterCustomer_Invt();
						Update_OM_KPIQuarterCustomer_Invt(objCountry, curItem, true);
                        objCountry.QuarterNbr = QuarterNbr;
						objCountry.KPI = KPI;

						_db.OM_KPIQuarterCustomer_Invt.AddObject(objCountry);
					}
				}
				#endregion


                #region OM_KPIQuarterCondition
                foreach (var del in stoOM_KPIQuarterCondition.Deleted)
                {
                    if (stoOM_KPIQuarterCondition.Created.Where(p => p.LineRef == del.LineRef).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPIQuarterCondition.Created.Where(p => p.LineRef == del.LineRef).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPIQuarterCondition.ToList().Where(p => p.QuarterNbr == header.QuarterNbr
                            && p.KPI.ToUpper() == header.KPI.ToUpper() && p.LineRef == del.LineRef).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPIQuarterCondition.DeleteObject(objDel);
                        }
                    }
                }

                foreach (var curItem in stoOM_KPIQuarterCondition.Created)
                {
                    if (curItem.Type.PassNull() == "") continue;

                    var objCondition = _db.OM_KPIQuarterCondition.Where(p => p.QuarterNbr == header.QuarterNbr
                        && p.KPI.ToUpper() == header.KPI.ToUpper() && p.LineRef.ToUpper() == curItem.LineRef.ToUpper()).FirstOrDefault();

                    if (objCondition != null)
                    {
                        if (objCondition.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                             Update_OM_KPIQuarterCondition(ref objCondition, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCondition = new OM_KPIQuarterCondition();
                        Update_OM_KPIQuarterCondition(ref objCondition, curItem, true);
                        objCondition.QuarterNbr = QuarterNbr;
                        objCondition.KPI = KPI;
                        _db.OM_KPIQuarterCondition.AddObject(objCondition);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, QuarterNbr = QuarterNbr }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string QuarterNbr = data["cboQuarterNbr"].PassNull();
                string KPI = data["cboKPI"].PassNull();

                var obj = _db.OM_KPIQuarterHeader.FirstOrDefault(p => p.QuarterNbr == QuarterNbr && p.KPI==KPI);
                if (obj != null)
                {
                    _db.OM_KPIQuarterHeader.DeleteObject(obj);
                }

                var lstTargetGroupDet = _db.OM_KPIQuarterCpny_All.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstTargetGroupDet)
                {
                    _db.OM_KPIQuarterCpny_All.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPIQuarterCpny_Class = _db.OM_KPIQuarterCpny_Class.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPIQuarterCpny_Class)
                {
                    _db.OM_KPIQuarterCpny_Class.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPIQuarterCpny_Invt = _db.OM_KPIQuarterCpny_Invt.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPIQuarterCpny_Invt)
                {
                    _db.OM_KPIQuarterCpny_Invt.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPIQuarterSales_All = _db.OM_KPIQuarterSales_All.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPIQuarterSales_All)
                {
                    _db.OM_KPIQuarterSales_All.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPIQuarterSales_Class = _db.OM_KPIQuarterSales_Class.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPIQuarterSales_Class)
                {
                    _db.OM_KPIQuarterSales_Class.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPIQuarterSales_Invt = _db.OM_KPIQuarterSales_Invt.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPIQuarterSales_Invt)
                {
                    _db.OM_KPIQuarterSales_Invt.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPIQuarterCondition = _db.OM_KPIQuarterCondition.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).ToList();
                foreach (var objCondition in lstOM_KPIQuarterCondition)
                {
                    _db.OM_KPIQuarterCondition.DeleteObject(objCondition);
                }
                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
                        
        private void UpdatingHeader(ref OM_KPIQuarterHeader t, OM_KPIQuarterHeader s)
        {
            t.Status = s.Status;           

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPIQuarterCpny_All(OM_KPIQuarterCpny_All t, OM27400_pgKPICpny_All_Result s, bool isNew)
        {
            if (isNew)
            {
                t.QuarterNbr = s.QuarterNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CpnyName = s.CpnyName;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPIQuarterCpny_Class(OM_KPIQuarterCpny_Class t, OM27400_pgKPICpny_Class_Result s, bool isNew)
        {
            if (isNew)
            {
                t.QuarterNbr = s.QuarterNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.ClassID = s.ClassID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CpnyName = s.CpnyName;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPIQuarterCpny_Invt(OM_KPIQuarterCpny_Invt t, OM27400_pgKPICpny_Invt_Result s, bool isNew)
        {
            if (isNew)
            {
                t.QuarterNbr = s.QuarterNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.InvtID = s.InvtID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CpnyName = s.CpnyName;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPIQuarterSales_All(OM_KPIQuarterSales_All t, OM27400_pgKPISales_All_Result s, bool isNew)
        {
            if (isNew)
            {
                t.QuarterNbr = s.QuarterNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.SlsperId = s.SlsperId;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CpnyName = s.CpnyName;
            t.SlsperName = s.SlsperName;
            t.Position = s.Position;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.tstamp = new byte[1];
        }

		private void Update_OM_KPIQuarterCustomer_All(OM_KPIQuarterCustomer_All t, OM27400_pgKPICustomer_All_Result s, bool isNew)
		{
			if (isNew)
			{
                t.QuarterNbr = s.QuarterNbr;
				t.KPI = s.KPI;
				t.BranchID = s.BranchID;
				t.SlsperId = s.SlsperId;
				t.CustID = s.CustID;
				t.Crtd_DateTime = DateTime.Now;
				t.Crtd_Prog = _screenNbr;
				t.Crtd_User = _userName;
			}
			t.CpnyName = s.CpnyName;
			t.SlsperName = s.SlsperName;
			t.CustName = s.CustName;
			t.Target = s.Target;
			t.LUpd_DateTime = DateTime.Now;
			t.LUpd_Prog = _screenNbr;
			t.LUpd_User = _userName;
		}

        private void Update_OM_KPIQuarterSales_Class(OM_KPIQuarterSales_Class t, OM27400_pgKPISales_Class_Result s, bool isNew)
        {
            if (isNew)
            {
                t.QuarterNbr = s.QuarterNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.SlsperId = s.SlsperId;
                t.ClassID = s.ClassID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CpnyName = s.CpnyName;
            t.SlsperName = s.SlsperName;
            t.Position = s.Position;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPIQuarterSales_Invt(OM_KPIQuarterSales_Invt t, OM27400_pgKPISales_Invt_Result s, bool isNew)
        {
            if (isNew)
            {
                t.QuarterNbr = s.QuarterNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.SlsperId = s.SlsperId;
                t.InvtID = s.InvtID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.CpnyName = s.CpnyName;
            t.SlsperName = s.SlsperName;
            t.Position = s.Position;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

		private void Update_OM_KPIQuarterCustomer_Class(OM_KPIQuarterCustomer_Class t, OM27400_pgKPICustomer_Class_Result s, bool isNew)
		{
			if (isNew)
			{
                t.QuarterNbr = s.QuarterNbr;
				t.KPI = s.KPI;
				t.BranchID = s.BranchID;
				t.CustID = s.CustID;
				t.SlsperId = s.SlsperId;
				t.ClassID = s.ClassID;
				t.Crtd_DateTime = DateTime.Now;
				t.Crtd_Prog = _screenNbr;
				t.Crtd_User = _userName;
			}
			t.CpnyName = s.CpnyName;
			t.SlsperName = s.SlsperName;
			t.CustName = s.CustName;
			t.Target = s.Target;
			t.LUpd_DateTime = DateTime.Now;
			t.LUpd_Prog = _screenNbr;
			t.LUpd_User = _userName;
		}

		private void Update_OM_KPIQuarterCustomer_Invt(OM_KPIQuarterCustomer_Invt t, OM27400_pgKPICustomer_Invt_Result s, bool isNew)
		{
			if (isNew)
			{
                t.QuarterNbr = s.QuarterNbr;
				t.KPI = s.KPI;
				t.BranchID = s.BranchID;
				t.CustID = s.CustID;
				t.SlsperId = s.SlsperId;
				t.InvtID = s.InvtID;
				t.Crtd_DateTime = DateTime.Now;
				t.Crtd_Prog = _screenNbr;
				t.Crtd_User = _userName;
			}
			t.CpnyName = s.CpnyName;
			t.SlsperName = s.SlsperName;
			t.CustName = s.CustName;
			t.Target = s.Target;
			t.LUpd_DateTime = DateTime.Now;
			t.LUpd_Prog = _screenNbr;
			t.LUpd_User = _userName;
        }

        private void Update_OM_KPIQuarterCondition(ref OM_KPIQuarterCondition t, OM27400_pgKPICondition_Result s, bool isNew)
        {
            if (isNew)
            {
                t.LineRef = s.LineRef;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
            }
            
            t.Type = s.Type;
            t.Descr = s.Descr;
            t.Value1 = s.Value1;
            t.Value2 = s.Value2;
            t.Value3 = s.Value3;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        #endregion

        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                string KPIheader = data["cboKPI"].PassNull();
                string appFor = data["cboApplyFor"].PassNull();
                string appTo = data["cboApplyTo"].PassNull();

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        #region Check Template
                        if (appFor == "C")
                        {
                            if (appTo == "A")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "I")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "InvtID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "G")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "ClassID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                        }
                        else if (appFor == "S")
                        {
                            if (appTo == "A")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "I")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "InvtID"
                                  || workSheet.Cells[0, 5].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "G")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "ClassID"
                                  || workSheet.Cells[0, 5].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                        }
                        else if (appFor == "CUS")
                        {
                            if (appTo == "A")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "CustID"
                                  || workSheet.Cells[0, 5].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "I")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                     || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                     || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                     || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID"
                                     || workSheet.Cells[0, 4].StringValue.Trim() != "CustID"
                                     || workSheet.Cells[0, 5].StringValue.Trim() != "InvtID"
                                     || workSheet.Cells[0, 6].StringValue.Trim() != "Target"
                                     )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "G")
                            {
                                if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("QuarterNbr")
                                     || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                     || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                     || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID"
                                     || workSheet.Cells[0, 4].StringValue.Trim() != "CustID"
                                     || workSheet.Cells[0, 5].StringValue.Trim() != "ClassID"
                                     || workSheet.Cells[0, 6].StringValue.Trim() != "Target"
                                     )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "2017011810", "", null, "", null);
                        }
                        #endregion

                        #region Define
                        string message = string.Empty;
                        string errorCheckExist = string.Empty;
                        string errorCycleNbr = string.Empty;
                        string errorCycleNbrnotExists = string.Empty;
                        string errorKPI = string.Empty;
                        string errorKPInotExists = string.Empty;
                        string errorKPIotherType = string.Empty;
                        string errorBranchID = string.Empty;
                        string errorBranchIDnotExists = string.Empty;
                        string errorTargetFormat = string.Empty;
                        string errorClassID = string.Empty;
                        string errorClassIDnotExists = string.Empty;
                        string errorInvtID = string.Empty;
                        string errorInvtIDnotExists = string.Empty;
                        string errorSlsperID = string.Empty;
                        string errorSlsperIDnotExists = string.Empty;
                        string errorCustID = string.Empty;
                        string errorCustIDnotExists = string.Empty;
                        string errorStatus = string.Empty;

                        var lstCycle = _db.OM27400_piQuarter(_userName, _cpnyID,Current.LangID).ToList();
                        var lstKPI = _db.OM27400_piKPI(_userName, _cpnyID, Current.LangID).ToList();
                        var lstBranch = _db.OM27400_piBranchID(_userName, _cpnyID, Current.LangID).ToList();
                        var lstClass = _db.OM27400_piClassID(_userName, _cpnyID, Current.LangID).ToList();
                        var lstInvt = _db.OM27400_piInvtID(_userName, _cpnyID, Current.LangID).ToList();
                        var lstSlsper = _db.OM27400_piSlsperId(_userName, _cpnyID, Current.LangID).ToList();
                        var lstCust = _db.OM27400_piCustId(_userName, _cpnyID, Current.LangID).ToList();

                        List<OM_KPIQuarterHeader> lstOM_KPIQuarterHeader = new List<OM_KPIQuarterHeader>();
                        List<OM_KPIQuarterCpny_All> lstOM_KPIQuarterCpny_All = new List<OM_KPIQuarterCpny_All>();
                        List<OM_KPIQuarterCpny_Class> lstOM_KPIQuarterCpny_Class = new List<OM_KPIQuarterCpny_Class>();
                        List<OM_KPIQuarterCpny_Invt> lstOM_KPIQuarterCpny_Invt = new List<OM_KPIQuarterCpny_Invt>();
                        List<OM_KPIQuarterSales_All> lstOM_KPIQuarterSales_All = new List<OM_KPIQuarterSales_All>();
                        List<OM_KPIQuarterSales_Class> lstOM_KPIQuarterSales_Class = new List<OM_KPIQuarterSales_Class>();
                        List<OM_KPIQuarterSales_Invt> lstOM_KPIQuarterSales_Invt = new List<OM_KPIQuarterSales_Invt>();
                        List<OM_KPIQuarterCustomer_All> lstOM_KPIQuarterCustomer_All = new List<OM_KPIQuarterCustomer_All>();
                        List<OM_KPIQuarterCustomer_Class> lstOM_KPIQuarterCustomer_Class = new List<OM_KPIQuarterCustomer_Class>();
                        List<OM_KPIQuarterCustomer_Invt> lstOM_KPIQuarterCustomer_Invt = new List<OM_KPIQuarterCustomer_Invt>();
                        #endregion

                        for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                        {
                            string QuarterNbr = string.Empty;
                            string KPI = string.Empty;
                            string BranchID = string.Empty;
                            string BranchName = string.Empty;
                            string Target = string.Empty;
                            string ClassID = string.Empty;
                            string InvtID = string.Empty;
                            string SlsperID = string.Empty;
                            string SlsperName = string.Empty;
                            string CustID = string.Empty;
                            string CustName = string.Empty;

                            var objCycle = new OM27400_piQuarter_Result();
                            var objKPI = new OM27400_piKPI_Result();
                            var objBranch = new OM27400_piBranchID_Result();
                            var objClass = new OM27400_piClassID_Result();
                            var objInvt = new OM27400_piInvtID_Result();
                            var objSlsper = new OM27400_piSlsperId_Result();
                            var objCust = new OM27400_piCustId_Result();

                            bool flagCheck = false;

                            #region GetValue & CheckError
                            QuarterNbr = workSheet.Cells[i, 0].StringValue.PassNull();
                            KPI = workSheet.Cells[i, 1].StringValue.PassNull();
                            BranchID = workSheet.Cells[i, 2].StringValue.PassNull();

                            if (appFor == "C")
                            {
                                if (appTo == "A")
                                {
                                    Target = workSheet.Cells[i, 3].StringValue.PassNull();
                                }
                                else if (appTo == "I")
                                {
                                    InvtID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 4].StringValue.PassNull();
                                }
                                else if (appTo == "G")
                                {
                                    ClassID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 4].StringValue.PassNull();
                                }
                            }
                            else if (appFor == "S")
                            {
                                if (appTo == "A")
                                {
                                    SlsperID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 4].StringValue.PassNull();
                                }
                                else if (appTo == "I")
                                {
                                    SlsperID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    InvtID = workSheet.Cells[i, 4].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 5].StringValue.PassNull();
                                }
                                else if (appTo == "G")
                                {
                                    SlsperID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    ClassID = workSheet.Cells[i, 4].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 5].StringValue.PassNull();
                                }
                            }
                            else if (appFor == "CUS")
                            {
                                if (appTo == "A")
                                {
                                    SlsperID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    CustID = workSheet.Cells[i, 4].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 5].StringValue.PassNull();
                                }
                                else if (appTo == "I")
                                {
                                    SlsperID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    CustID = workSheet.Cells[i, 4].StringValue.PassNull();
                                    InvtID = workSheet.Cells[i, 5].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 6].StringValue.PassNull();
                                }
                                else if (appTo == "G")
                                {
                                    SlsperID = workSheet.Cells[i, 3].StringValue.PassNull();
                                    CustID = workSheet.Cells[i, 4].StringValue.PassNull();
                                    ClassID = workSheet.Cells[i, 5].StringValue.PassNull();
                                    Target = workSheet.Cells[i, 6].StringValue.PassNull();
                                }
                            }

                            if (QuarterNbr == "" && KPI == "")
                            {
                                continue;
                            }

                            if (QuarterNbr == "")
                            {
                                errorCycleNbr += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }
                            else
                            {
                                if (lstCycle.FirstOrDefault(p => p.QuarterNbr == QuarterNbr) == null)
                                {
                                    errorCycleNbrnotExists += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                            }

                            if (KPI == "")
                            {
                                errorKPI += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }
                            else
                            {
                                objKPI = lstKPI.FirstOrDefault(p => p.KPI == KPI);
                                if (objKPI == null)
                                {
                                    errorKPInotExists += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {//errorKPIotherType
                                    if (objKPI.ApplyFor != appFor || objKPI.ApplyTo != appTo)
                                    {
                                        errorKPIotherType += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                }
                            }

                            var flagBranchID = false;
                            if (BranchID == "")
                            {
                                errorBranchID += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }
                            else
                            {
                                objBranch = lstBranch.FirstOrDefault(p => p.CpnyID == BranchID);
                                if (objBranch == null)
                                {
                                    errorBranchIDnotExists += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                    flagBranchID = true;
                                }
                                else
                                    BranchName = objBranch.CpnyName;
                            }

                            if (Target != "")
                            {
                                float n;
                                bool isNumeric = float.TryParse(Target, out n);
                                if (!isNumeric)
                                {
                                    errorTargetFormat += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                            }
                            else
                            {
                                Target = "0";
                            }

                            if (appFor == "C")
                            {
                                if (appTo == "A")
                                {

                                }
                                else if (appTo == "I")
                                {
                                    if (InvtID == "")
                                    {
                                        errorInvtID += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (flagBranchID == false)
                                        {
                                            if (lstInvt.FirstOrDefault(p => p.InvtID == InvtID) == null)
                                            {
                                                errorInvtIDnotExists += (i + 1).ToString() + ",";
                                                flagCheck = true;
                                            }
                                        }
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    if (ClassID == "")
                                    {
                                        errorClassID += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstClass.FirstOrDefault(p => p.ClassID == ClassID) == null)
                                        {
                                            errorClassIDnotExists += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                }
                            }
                            else if (appFor == "S")
                            {
                                if (SlsperID == "")
                                {
                                    errorSlsperID += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (flagBranchID == false)
                                    {
                                        objSlsper = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID);
                                        if (objSlsper == null)
                                        {
                                            errorSlsperIDnotExists += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                        else
                                            SlsperName = objSlsper.Name;
                                    }
                                }

                                if (appTo == "A")
                                {

                                }
                                else if (appTo == "I")
                                {
                                    if (InvtID == "")
                                    {
                                        errorInvtID += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (flagBranchID == false)
                                        {
                                            if (lstInvt.FirstOrDefault(p => p.InvtID == InvtID) == null)
                                            {
                                                errorInvtIDnotExists += (i + 1).ToString() + ",";
                                                flagCheck = true;
                                            }
                                        }
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    if (ClassID == "")
                                    {
                                        errorClassID += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstClass.FirstOrDefault(p => p.ClassID == ClassID) == null)
                                        {
                                            errorClassIDnotExists += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                }
                            }
                            else if (appFor == "CUS")
                            {
                                if (SlsperID == "")
                                {
                                    errorSlsperID += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (flagBranchID == false)
                                    {
                                        objSlsper = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID);
                                        if (objSlsper == null)
                                        {
                                            errorSlsperIDnotExists += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                        else
                                            SlsperName = objSlsper.Name;
                                    }
                                }

                                if (CustID == "")
                                {
                                    errorCustID += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    if (flagBranchID == false)
                                    {
                                        objCust = lstCust.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperID == SlsperID && p.CustId == CustID);
                                        if (objCust == null)
                                        {
                                            errorCustIDnotExists += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                        else
                                            CustName = objCust.CustName;
                                    }
                                }

                                if (appTo == "A")
                                {

                                }
                                else if (appTo == "I")
                                {
                                    if (InvtID == "")
                                    {
                                        errorInvtID += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (flagBranchID == false)
                                        {
                                            if (lstInvt.FirstOrDefault(p => p.InvtID == InvtID) == null)
                                            {
                                                errorInvtIDnotExists += (i + 1).ToString() + ",";
                                                flagCheck = true;
                                            }
                                        }
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    if (ClassID == "")
                                    {
                                        errorClassID += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }
                                    else
                                    {
                                        if (lstClass.FirstOrDefault(p => p.ClassID == ClassID) == null)
                                        {
                                            errorClassIDnotExists += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                }
                            }

                            if (flagCheck == true)
                            {
                                continue;
                            }

                            #endregion

                            #region Import data to DB
                            // Import to header
                            var record = _db.OM_KPIQuarterHeader.FirstOrDefault(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI);
                            var CheckRecordEmpity = lstOM_KPIQuarterHeader.Where(p => p.QuarterNbr == QuarterNbr && p.KPI == KPI).FirstOrDefault();
                            if (CheckRecordEmpity == null)
                            {
                                if (record != null)
                                {
                                    if (record.Status != "H")
                                    {
                                        errorStatus += (i + 1).ToString() + ",";
                                        continue;
                                    }
                                    else
                                    {
                                        record.LUpd_DateTime = DateTime.Now;
                                        record.LUpd_User = Current.UserName;
                                        record.LUpd_Prog = _screenNbr;
                                    }
                                }
                                else
                                {
                                    record = new OM_KPIQuarterHeader();
                                    record.KPI = KPI;
                                    record.QuarterNbr = QuarterNbr;
                                    record.Status = "H";
                                    record.Crtd_DateTime = DateTime.Now;
                                    record.Crtd_User = Current.UserName;
                                    record.Crtd_Prog = _screenNbr;
                                    record.LUpd_DateTime = DateTime.Now;
                                    record.LUpd_User = Current.UserName;
                                    record.LUpd_Prog = _screenNbr;
                                    _db.OM_KPIQuarterHeader.AddObject(record);
                                }
                                lstOM_KPIQuarterHeader.Add(record);
                            }
                            else
                            {
                                errorCheckExist += (i + 1).ToString() + ",";
                            }
                            if (appFor == "C")
                            {
                                if (appTo == "A")
                                {
                                    //OM_KPIQuarterCpny_All
                                    var recordExists_OM_KPIQuarterCpny_All = lstOM_KPIQuarterCpny_All.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                        && p.KPI == KPI
                                                                                                        && p.BranchID == BranchID);
                                    if (recordExists_OM_KPIQuarterCpny_All == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterCpny_All.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                            && p.KPI == KPI
                                                                                            && p.BranchID == BranchID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterCpny_All();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.Target = Convert.ToDouble(Target);

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterCpny_All.AddObject(recordItem);
                                            lstOM_KPIQuarterCpny_All.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                                else if (appTo == "I")
                                {
                                    //OM_KPIQuarterCpny_Invt
                                    var recordExists_OM_KPIQuarterCpny_Invt = lstOM_KPIQuarterCpny_Invt.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.InvtID == InvtID);
                                    if (recordExists_OM_KPIQuarterCpny_Invt == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterCpny_Invt.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.InvtID == InvtID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterCpny_Invt();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.InvtID = InvtID;
                                            recordItem.Target = Convert.ToDouble(Target);

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterCpny_Invt.AddObject(recordItem);
                                            lstOM_KPIQuarterCpny_Invt.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    //OM_KPIQuarterCpny_Class
                                    var recordExists_OM_KPIQuarterCpny_Class = lstOM_KPIQuarterCpny_Class.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.ClassID == ClassID);
                                    if (recordExists_OM_KPIQuarterCpny_Class == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterCpny_Class.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.ClassID == ClassID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterCpny_Class();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.ClassID = ClassID;
                                            recordItem.Target = Convert.ToDouble(Target);

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterCpny_Class.AddObject(recordItem);
                                            lstOM_KPIQuarterCpny_Class.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                            }
                            else if (appFor == "S")
                            {
                                if (appTo == "A")
                                {
                                    //OM_KPIQuarterSales_All
                                    var recordExists_OM_KPIQuarterSales_All = lstOM_KPIQuarterSales_All.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.SlsperId == SlsperID);
                                    if (recordExists_OM_KPIQuarterSales_All == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterSales_All.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                            && p.KPI == KPI
                                                                                            && p.BranchID == BranchID
                                                                                            && p.SlsperId == SlsperID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterSales_All();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.SlsperName = SlsperName;
                                            recordItem.Position = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID).Position;
                                            recordItem.Target = Convert.ToDouble(Target);

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterSales_All.AddObject(recordItem);
                                            lstOM_KPIQuarterSales_All.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                                else if (appTo == "I")
                                {
                                    //OM_KPIQuarterSales_Invt
                                    var recordExists_OM_KPIQuarterSales_Invt = lstOM_KPIQuarterSales_Invt.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.SlsperId == SlsperID
                                                                                                            && p.InvtID == InvtID);
                                    if (recordExists_OM_KPIQuarterSales_Invt == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterSales_Invt.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.InvtID == InvtID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterSales_Invt();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.SlsperName = SlsperName;
                                            recordItem.InvtID = InvtID;
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.Position = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID).Position;

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterSales_Invt.AddObject(recordItem);
                                            lstOM_KPIQuarterSales_Invt.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    //OM_KPIQuarterSales_Class
                                    var recordExists_OM_KPIQuarterSales_Class = lstOM_KPIQuarterSales_Class.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.ClassID == ClassID);
                                    if (recordExists_OM_KPIQuarterSales_Class == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterSales_Class.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.ClassID == ClassID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterSales_Class();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.SlsperName = SlsperName;
                                            recordItem.ClassID = ClassID;
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.Position = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID).Position;

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterSales_Class.AddObject(recordItem);
                                            lstOM_KPIQuarterSales_Class.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                            }
                            else if (appFor == "CUS")
                            {
                                if (appTo == "A")
                                {
                                    //OM_KPIQuarterCustomer_All
                                    var recordExists_OM_KPIQuarterCustomer_All = lstOM_KPIQuarterCustomer_All.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.CustID == CustID);
                                    if (recordExists_OM_KPIQuarterCustomer_All == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterCustomer_All.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.CustID == CustID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterCustomer_All();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.SlsperName = SlsperName;
                                            recordItem.CustID = CustID;
                                            recordItem.CustName = CustName;
                                            recordItem.Target = Convert.ToDouble(Target);

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterCustomer_All.AddObject(recordItem);
                                            lstOM_KPIQuarterCustomer_All.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                                else if (appTo == "I")
                                {
                                    //OM_KPIQuarterCustomer_Invt
                                    var recordExists_OM_KPIQuarterCustomer_Invt = lstOM_KPIQuarterCustomer_Invt.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.CustID == CustID
                                                                                                                && p.InvtID == InvtID);
                                    if (recordExists_OM_KPIQuarterCustomer_Invt == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterCustomer_Invt.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.CustID == CustID
                                                                                                && p.InvtID == InvtID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterCustomer_Invt();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.SlsperName = SlsperName;
                                            recordItem.CustID = CustID;
                                            recordItem.CustName = CustName;
                                            recordItem.InvtID = InvtID;
                                            recordItem.Target = Convert.ToDouble(Target);

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterCustomer_Invt.AddObject(recordItem);
                                            lstOM_KPIQuarterCustomer_Invt.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    //OM_KPIQuarterCustomer_Class
                                    var recordExists_OM_KPIQuarterCustomer_Class = lstOM_KPIQuarterCustomer_Class.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.CustID == CustID
                                                                                                                && p.ClassID == ClassID);
                                    if (recordExists_OM_KPIQuarterCustomer_Class == null)
                                    {
                                        var recordItem = _db.OM_KPIQuarterCustomer_Class.FirstOrDefault(p => p.QuarterNbr == QuarterNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.CustID == CustID
                                                                                                && p.ClassID == ClassID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPIQuarterCustomer_Class();
                                            recordItem.ResetET();
                                            recordItem.QuarterNbr = QuarterNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.CpnyName = BranchName;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.SlsperName = SlsperName;
                                            recordItem.CustID = CustID;
                                            recordItem.CustName = CustName;
                                            recordItem.ClassID = ClassID;
                                            recordItem.Target = Convert.ToDouble(Target);

                                            recordItem.Crtd_DateTime = recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.Crtd_User = recordItem.LUpd_User = _userName;
                                            _db.OM_KPIQuarterCustomer_Class.AddObject(recordItem);
                                            lstOM_KPIQuarterCustomer_Class.Add(recordItem);
                                        }
                                        else
                                        {
                                            recordItem.Target = Convert.ToDouble(Target);
                                            recordItem.LUpd_DateTime = DateTime.Now;
                                            recordItem.LUpd_Prog = _screenNbr;
                                            recordItem.LUpd_User = _userName;
                                        }
                                    }
                                }
                            }
                            #endregion
                        }// Vòng for

                        message = errorCycleNbr == "" ? "" : string.Format(Message.GetString("2016091412", null), "Cycle", errorCycleNbr);
                        message += errorCycleNbrnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "Cycle", errorCycleNbrnotExists);
                        message += errorKPI == "" ? "" : string.Format(Message.GetString("2016091412", null), "KPI", errorKPI);
                        message += errorKPInotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "KPI", errorKPInotExists);
                        message += errorKPIotherType == "" ? "" : string.Format(Message.GetString("2017011811", null), errorKPIotherType);
                        message += errorBranchID == "" ? "" : string.Format(Message.GetString("2016091412", null), "BranchID", errorBranchID);
                        message += errorBranchIDnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "BranchID", errorBranchIDnotExists);
                        message += errorTargetFormat == "" ? "" : string.Format(Message.GetString("2016091415", null), "Target", errorTargetFormat);
                        message += errorClassID == "" ? "" : string.Format(Message.GetString("2016091412", null), "ClassID", errorClassID);
                        message += errorClassIDnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "ClassID", errorClassIDnotExists);
                        message += errorInvtID == "" ? "" : string.Format(Message.GetString("2016091412", null), "InvtID", errorInvtID);
                        message += errorInvtIDnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "InvtID", errorInvtIDnotExists);
                        message += errorSlsperID == "" ? "" : string.Format(Message.GetString("2016091412", null), "SlsperID", errorSlsperID);
                        message += errorSlsperIDnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "SlsperID", errorSlsperIDnotExists);
                        message += errorCustID == "" ? "" : string.Format(Message.GetString("2016091412", null), "CustID", errorCustID);
                        message += errorCustIDnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "CustID", errorCustIDnotExists);
                        message += errorStatus == "" ? "" : string.Format(Message.GetString("2016091420", null), errorStatus);

                        // Check Record Exsit
                        message += errorCheckExist.RemoveLast() == "" ? "" : string.Format(Message.GetString("20170726", null), errorCheckExist.RemoveLast());
                        if (message == "" || message == string.Empty)
                        {
                            _db.SaveChanges();
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                    }
                    return _logMessage;
                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
            return _logMessage;
        }
    }
}

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

namespace OM25100.Controllers
{
    [DirectController]
    public class OM25100Controller : Controller
    {
        private string _screenNbr = "OM25100";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        private JsonResult _logMessage;

        OM25100Entities _db = Util.CreateObjectContext<OM25100Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            var showTabCondition = 0;
            var showTabSalesClassDetail = false;
            var objConfig = _db.OM25100_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                showTabCondition = objConfig.ShowTabCondition.HasValue ? objConfig.ShowTabCondition.Value : 0;
                showTabSalesClassDetail = objConfig.ShowTabSalesClassDetail.HasValue && objConfig.ShowTabSalesClassDetail.Value;
            }
            ViewBag.showTabCondition = showTabCondition;
            ViewBag.showTabSalesClassDetail = showTabSalesClassDetail;
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        #region -Get data-

        public ActionResult GetMCCodeHeader(string CycleNbr, string KPI)
        {
            var MCCodeHeader = _db.OM_KPIHeader.FirstOrDefault(p => p.CycleNbr == CycleNbr && p.KPI == KPI);
            return this.Store(MCCodeHeader);
        }

        public ActionResult GetKPICpny_All(string CycleNbr, string KPI, string Zone, string Territory)
        {
            return this.Store(_db.OM25100_pgKPICpny_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPICpny_Class(string CycleNbr, string KPI, string Zone, string Territory)
        {
            return this.Store(_db.OM25100_pgKPICpny_Class(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPICpny_Invt(string CycleNbr, string KPI, string Zone, string Territory)
        {
            return this.Store(_db.OM25100_pgKPICpny_Invt(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPISales_All(string CycleNbr, string KPI, string Zone, string Territory, string TypeStaff)
        {
            var lstKPISales_All = _db.OM25100_pgKPISales_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory, TypeStaff).ToList();
            return this.Store(lstKPISales_All);
        }
        public ActionResult GetKPICustomer_All(string CycleNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_All = _db.OM25100_pgKPICustomer_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_All);
        }
        public ActionResult GetKPICustomer_Class(string CycleNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_Class = _db.OM25100_pgKPICustomer_Class(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_Class);
        }
        public ActionResult GetKPICustomer_Invt(string CycleNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_Invt = _db.OM25100_pgKPICustomer_Invt(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_Invt);
        }
        public ActionResult GetKPISales_Class(string CycleNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_Class = _db.OM25100_pgKPISales_Class(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_Class);
        }
        public ActionResult GetKPISales_Invt(string CycleNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_Invt = _db.OM25100_pgKPISales_Invt(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_Invt);
        }
        public ActionResult GetKPICondition(string CycleNbr, string KPI)
        {
            var lstKPICondition = _db.OM25100_pgKPICondition(CycleNbr, KPI, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstKPICondition);
        }
        public ActionResult GetKPISalesClassDetail(string CycleNbr, string KPI)
        {
            var lstKPICondition = _db.OM25100_pgSalesClassDetail(CycleNbr, KPI, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstKPICondition);
        }
        #endregion

        #region -Save delete-
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string CycleNbr = data["cboCycleNbr"].PassNull();
                string KPI = data["cboKPI"].PassNull();
                string handle = data["cboHandle"].PassNull();
                string appFor = data["cboApplyFor"].PassNull();
                string appTo = data["cboApplyTo"].PassNull();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstMCCode"]);
                var curHeader = dataHandler1.ObjectData<OM_KPIHeader>().FirstOrDefault();

                StoreDataHandler dataHandler_grid = new StoreDataHandler(data["stoOM_KPICpny_All"]);
                ChangeRecords<OM25100_pgKPICpny_All_Result> stoOM_KPICpny_All = dataHandler_grid.BatchObjectData<OM25100_pgKPICpny_All_Result>();
                stoOM_KPICpny_All.Created.AddRange(stoOM_KPICpny_All.Updated);// Dua danh sach update chung vao danh sach tao moi

                StoreDataHandler dataHander_grid1 = new StoreDataHandler(data["stoOM_KPICpny_Class"]);
                ChangeRecords<OM25100_pgKPICpny_Class_Result> stoOM_KPICpny_Class = dataHander_grid1.BatchObjectData<OM25100_pgKPICpny_Class_Result>();
                stoOM_KPICpny_Class.Created.AddRange(stoOM_KPICpny_Class.Updated);// Dua danh sach update chung vao danh sach tao moi

                StoreDataHandler dataHander_grid2 = new StoreDataHandler(data["stoOM_KPICpny_Invt"]);
                ChangeRecords<OM25100_pgKPICpny_Invt_Result> stoOM_KPICpny_Invt = dataHander_grid2.BatchObjectData<OM25100_pgKPICpny_Invt_Result>();
                stoOM_KPICpny_Invt.Created.AddRange(stoOM_KPICpny_Invt.Updated);

                StoreDataHandler dataHander_grid3 = new StoreDataHandler(data["stoOM_KPISales_All"]);
                ChangeRecords<OM25100_pgKPISales_All_Result> stoOM_KPISales_All = dataHander_grid3.BatchObjectData<OM25100_pgKPISales_All_Result>();
                stoOM_KPISales_All.Created.AddRange(stoOM_KPISales_All.Updated);

                StoreDataHandler dataHander_grid4 = new StoreDataHandler(data["stoOM_KPISales_Class"]);
                ChangeRecords<OM25100_pgKPISales_Class_Result> stoOM_KPISales_Class = dataHander_grid4.BatchObjectData<OM25100_pgKPISales_Class_Result>();
                stoOM_KPISales_Class.Created.AddRange(stoOM_KPISales_Class.Updated);

                StoreDataHandler dataHander_grid5 = new StoreDataHandler(data["stoOM_KPISales_Invt"]);
                ChangeRecords<OM25100_pgKPISales_Invt_Result> stoOM_KPISales_Invt = dataHander_grid5.BatchObjectData<OM25100_pgKPISales_Invt_Result>();
                stoOM_KPISales_Invt.Created.AddRange(stoOM_KPISales_Invt.Updated);

                StoreDataHandler dataHander_grid6 = new StoreDataHandler(data["stoOM_KPICustomer_All"]);
                ChangeRecords<OM25100_pgKPICustomer_All_Result> stoOM_KPICustomer_All = dataHander_grid6.BatchObjectData<OM25100_pgKPICustomer_All_Result>();
                stoOM_KPICustomer_All.Created.AddRange(stoOM_KPICustomer_All.Updated);

                StoreDataHandler dataHander_grid7 = new StoreDataHandler(data["stoOM_KPICustomer_Class"]);
                ChangeRecords<OM25100_pgKPICustomer_Class_Result> stoOM_KPICustomer_Class = dataHander_grid7.BatchObjectData<OM25100_pgKPICustomer_Class_Result>();
                stoOM_KPICustomer_Class.Created.AddRange(stoOM_KPICustomer_Class.Updated);

                StoreDataHandler dataHander_grid8 = new StoreDataHandler(data["stoOM_KPICustomer_Invt"]);
                ChangeRecords<OM25100_pgKPICustomer_Invt_Result> stoOM_KPICustomer_Invt = dataHander_grid8.BatchObjectData<OM25100_pgKPICustomer_Invt_Result>();
                stoOM_KPICustomer_Invt.Created.AddRange(stoOM_KPICustomer_Invt.Updated);

                StoreDataHandler dataHander_gridCondition = new StoreDataHandler(data["stoOM_KPICondition"]);
                ChangeRecords<OM25100_pgKPICondition_Result> stoOM_KPICondition = dataHander_gridCondition.BatchObjectData<OM25100_pgKPICondition_Result>();
                stoOM_KPICondition.Created.AddRange(stoOM_KPICondition.Updated);


                StoreDataHandler dataHander_gridSalesClassDetail = new StoreDataHandler(data["stoSalesClassDetail"]);
                ChangeRecords<OM25100_pgSalesClassDetail_Result> stoSalesClassDetail = dataHander_gridSalesClassDetail.BatchObjectData<OM25100_pgSalesClassDetail_Result>();
                stoSalesClassDetail.Created.AddRange(stoSalesClassDetail.Updated);

                #region OM_KPIHeader
                var header = _db.OM_KPIHeader.FirstOrDefault(p => p.CycleNbr == CycleNbr && p.KPI == KPI);

                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {

                        UpdatingHeader(ref header, curHeader);
                        if (handle != "N" && handle != "")
                        {
                            if (handle == "C")
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
                    header = new OM_KPIHeader();
                    header.ResetET();
                    //header.Status = handle;
                    header.CycleNbr = CycleNbr;
                    header.KPI = KPI;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader);
                    _db.OM_KPIHeader.AddObject(header);
                }
                #endregion

                #region OM_KPICpny_All
                foreach (OM25100_pgKPICpny_All_Result del in stoOM_KPICpny_All.Deleted)
                {
                    if (stoOM_KPICpny_All.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPICpny_All.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPICpny_All.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPICpny_All.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM25100_pgKPICpny_All_Result curItem in stoOM_KPICpny_All.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPICpny_All.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPICpny_All(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPICpny_All();
                        Update_OM_KPICpny_All(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPICpny_All.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM25100_pgKPICpny_Class
                foreach (OM25100_pgKPICpny_Class_Result del in stoOM_KPICpny_Class.Deleted)
                {
                    if (stoOM_KPICpny_Class.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPICpny_Class.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPICpny_Class.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPICpny_Class.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM25100_pgKPICpny_Class_Result curItem in stoOM_KPICpny_Class.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPICpny_Class.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.ClassID.ToUpper() == curItem.ClassID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPICpny_Class(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPICpny_Class();
                        Update_OM_KPICpny_Class(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPICpny_Class.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM25100_pgKPICpny_Invt
                foreach (OM25100_pgKPICpny_Invt_Result del in stoOM_KPICpny_Invt.Deleted)
                {
                    if (stoOM_KPICpny_Invt.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPICpny_Invt.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPICpny_Invt.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPICpny_Invt.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM25100_pgKPICpny_Invt_Result curItem in stoOM_KPICpny_Invt.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPICpny_Invt.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.InvtID.ToUpper() == curItem.InvtID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPICpny_Invt(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPICpny_Invt();
                        Update_OM_KPICpny_Invt(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPICpny_Invt.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPISales_All
                foreach (OM25100_pgKPISales_All_Result del in stoOM_KPISales_All.Deleted)
                {
                    if (stoOM_KPISales_All.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPISales_All.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPISales_All.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPISales_All.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM25100_pgKPISales_All_Result curItem in stoOM_KPISales_All.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPISales_All.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPISales_All(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPISales_All();
                        Update_OM_KPISales_All(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPISales_All.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPISales_Class
                foreach (OM25100_pgKPISales_Class_Result del in stoOM_KPISales_Class.Deleted)
                {
                    if (stoOM_KPISales_Class.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPISales_Class.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPISales_Class.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPISales_Class.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM25100_pgKPISales_Class_Result curItem in stoOM_KPISales_Class.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPISales_Class.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.ClassID.ToUpper() == curItem.ClassID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPISales_Class(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPISales_Class();
                        Update_OM_KPISales_Class(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPISales_Class.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPISales_Invt
                foreach (OM25100_pgKPISales_Invt_Result del in stoOM_KPISales_Invt.Deleted)
                {
                    if (stoOM_KPISales_Invt.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPISales_Invt.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPISales_Invt.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPISales_Invt.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM25100_pgKPISales_Invt_Result curItem in stoOM_KPISales_Invt.Created)
                {
                    if (curItem.BranchID.PassNull() == "") continue;

                    var objCountry = _db.OM_KPISales_Invt.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.InvtID.ToUpper() == curItem.InvtID.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPISales_Invt(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPISales_Invt();
                        Update_OM_KPISales_Invt(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPISales_Invt.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPICustomer_All
                foreach (OM25100_pgKPICustomer_All_Result del in stoOM_KPICustomer_All.Deleted)
                {
                    if (stoOM_KPICustomer_All.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPICustomer_All.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPICustomer_All.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPICustomer_All.DeleteObject(objDel);
                        }
                    }
                }



                foreach (OM25100_pgKPICustomer_All_Result curItem in stoOM_KPICustomer_All.Created)
                {
                    if (curItem.BranchID.PassNull() == "" || curItem.CustID == "") continue;

                    var objCountry = _db.OM_KPICustomer_All.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPICustomer_All(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPICustomer_All();
                        Update_OM_KPICustomer_All(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPICustomer_All.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPICustomer_Class
                foreach (OM25100_pgKPICustomer_Class_Result del in stoOM_KPICustomer_Class.Deleted)
                {
                    if (stoOM_KPICustomer_Class.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPICustomer_Class.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPICustomer_Class.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPICustomer_Class.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM25100_pgKPICustomer_Class_Result curItem in stoOM_KPICustomer_Class.Created)
                {
                    if (curItem.BranchID.PassNull() == "" || curItem.CustID == "" || curItem.ClassID == "") continue;

                    var objCountry = _db.OM_KPICustomer_Class.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.ClassID.ToUpper() == curItem.ClassID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPICustomer_Class(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPICustomer_Class();
                        Update_OM_KPICustomer_Class(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPICustomer_Class.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPICustomer_Invt
                foreach (OM25100_pgKPICustomer_Invt_Result del in stoOM_KPICustomer_Invt.Deleted)
                {
                    if (stoOM_KPICustomer_Invt.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper() && p.SlsperId.ToUpper() == p.SlsperId.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPICustomer_Invt.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPICustomer_Invt.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
                            && p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPICustomer_Invt.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM25100_pgKPICustomer_Invt_Result curItem in stoOM_KPICustomer_Invt.Created)
                {
                    if (curItem.BranchID.PassNull() == "" || curItem.CustID == "" || curItem.InvtID == "") continue;

                    var objCountry = _db.OM_KPICustomer_Invt.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
                        && p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.InvtID.ToUpper() == curItem.InvtID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper()).FirstOrDefault();

                    if (objCountry != null)
                    {
                        if (objCountry.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPICustomer_Invt(objCountry, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCountry = new OM_KPICustomer_Invt();
                        Update_OM_KPICustomer_Invt(objCountry, curItem, true);
                        objCountry.CycleNbr = CycleNbr;
                        objCountry.KPI = KPI;

                        _db.OM_KPICustomer_Invt.AddObject(objCountry);
                    }
                }
                #endregion

                #region OM_KPICondition
                foreach (var del in stoOM_KPICondition.Deleted)
                {
                    if (stoOM_KPICondition.Created.Where(p => p.LineRef == del.LineRef).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        stoOM_KPICondition.Created.Where(p => p.LineRef == del.LineRef).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_KPICondition.ToList().Where(p => p.CycleNbr == header.CycleNbr
                            && p.KPI.ToUpper() == header.KPI.ToUpper() && p.LineRef == del.LineRef).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_KPICondition.DeleteObject(objDel);
                        }
                    }
                }

                foreach (var curItem in stoOM_KPICondition.Created)
                {
                    if (curItem.Type.PassNull() == "") continue;

                    var objCondition = _db.OM_KPICondition.Where(p => p.CycleNbr == header.CycleNbr
                        && p.KPI.ToUpper() == header.KPI.ToUpper() && p.LineRef.ToUpper() == curItem.LineRef.ToUpper()).FirstOrDefault();

                    if (objCondition != null)
                    {
                        if (objCondition.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {
                            Update_OM_KPICondition(ref objCondition, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objCondition = new OM_KPICondition();
                        Update_OM_KPICondition(ref objCondition, curItem, true);
                        objCondition.CycleNbr = CycleNbr;
                        objCondition.KPI = KPI;
                        _db.OM_KPICondition.AddObject(objCondition);
                    }
                }
                #endregion

                var showTabSalesClassDetail = false;
                var objConfig = _db.OM25100_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (objConfig != null)
                {
                    showTabSalesClassDetail = objConfig.ShowTabSalesClassDetail.HasValue && objConfig.ShowTabSalesClassDetail.Value;
                }

                if (appFor == "S" && appTo == "G" && showTabSalesClassDetail)
                {
                    #region OM_KPISalesClassDetail
                    foreach (var del in stoSalesClassDetail.Deleted)
                    {
                        if (stoSalesClassDetail.Created.Where(p => p.ClassID == del.ClassID).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                        {
                            stoSalesClassDetail.Created.Where(p => p.ClassID == del.ClassID).FirstOrDefault().tstamp = del.tstamp;
                        }
                        else
                        {
                            var objDel = _db.OM_KPISalesClassDetail.FirstOrDefault(p => p.CycleNbr == header.CycleNbr
                                && p.KPI.ToUpper() == header.KPI.ToUpper() && p.ClassID == del.ClassID);
                            if (objDel != null)
                            {
                                _db.OM_KPISalesClassDetail.DeleteObject(objDel);
                            }
                        }
                    }

                    foreach (var curItem in stoSalesClassDetail.Created)
                    {
                        if (curItem.ClassID.PassNull() == "") continue;

                        var objCondition = _db.OM_KPISalesClassDetail.Where(p => p.CycleNbr == header.CycleNbr
                            && p.KPI == header.KPI && p.ClassID == curItem.ClassID).FirstOrDefault();

                        if (objCondition != null)
                        {
                            if (objCondition.tstamp.ToHex() == curItem.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                            {
                                Update_OM_KPISalesClassDetail(ref objCondition, curItem, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            objCondition = new OM_KPISalesClassDetail();
                            Update_OM_KPISalesClassDetail(ref objCondition, curItem, true);
                            objCondition.CycleNbr = CycleNbr;
                            objCondition.KPI = KPI;
                            _db.OM_KPISalesClassDetail.AddObject(objCondition);
                        }
                    }
                    #endregion
                }
                else
                {
                    var lstOM_KPISalesClassDetail = _db.OM_KPISalesClassDetail.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                    foreach (var objCondition in lstOM_KPISalesClassDetail)
                    {
                        _db.OM_KPISalesClassDetail.DeleteObject(objCondition);
                    }
                }
                _db.SaveChanges();
                return Json(new { success = true, CycleNbr = CycleNbr }, "text/html");
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
                string CycleNbr = data["cboCycleNbr"].PassNull();
                string KPI = data["cboKPI"].PassNull();

                var obj = _db.OM_KPIHeader.FirstOrDefault(p => p.CycleNbr == CycleNbr && p.KPI == KPI);
                if (obj != null)
                {
                    _db.OM_KPIHeader.DeleteObject(obj);
                }

                var lstTargetGroupDet = _db.OM_KPICpny_All.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstTargetGroupDet)
                {
                    _db.OM_KPICpny_All.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPICpny_Class = _db.OM_KPICpny_Class.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPICpny_Class)
                {
                    _db.OM_KPICpny_Class.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPICpny_Invt = _db.OM_KPICpny_Invt.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPICpny_Invt)
                {
                    _db.OM_KPICpny_Invt.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPISales_All = _db.OM_KPISales_All.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPISales_All)
                {
                    _db.OM_KPISales_All.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPISales_Class = _db.OM_KPISales_Class.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPISales_Class)
                {
                    _db.OM_KPISales_Class.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPISales_Invt = _db.OM_KPISales_Invt.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var OjbGroupDet in lstOM_KPISales_Invt)
                {
                    _db.OM_KPISales_Invt.DeleteObject(OjbGroupDet);
                }

                var lstOM_KPICondition = _db.OM_KPICondition.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var objCondition in lstOM_KPICondition)
                {
                    _db.OM_KPICondition.DeleteObject(objCondition);
                }

                var lstOM_KPISalesClassDetail = _db.OM_KPISalesClassDetail.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).ToList();
                foreach (var objCondition in lstOM_KPISalesClassDetail)
                {
                    _db.OM_KPISalesClassDetail.DeleteObject(objCondition);
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

        private void UpdatingHeader(ref OM_KPIHeader t, OM_KPIHeader s)
        {
            t.Status = s.Status;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPICpny_All(OM_KPICpny_All t, OM25100_pgKPICpny_All_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPICpny_Class(OM_KPICpny_Class t, OM25100_pgKPICpny_Class_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.ClassID = s.ClassID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPICpny_Invt(OM_KPICpny_Invt t, OM25100_pgKPICpny_Invt_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.InvtID = s.InvtID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPISales_All(OM_KPISales_All t, OM25100_pgKPISales_All_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.SlsperId = s.SlsperId;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Position = s.Position;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPICustomer_All(OM_KPICustomer_All t, OM25100_pgKPICustomer_All_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.CustID = s.CustID;
                t.SlsperId = s.SlsperId;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPISales_Class(OM_KPISales_Class t, OM25100_pgKPISales_Class_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.SlsperId = s.SlsperId;
                t.ClassID = s.ClassID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Position = s.Position;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPISales_Invt(OM_KPISales_Invt t, OM25100_pgKPISales_Invt_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.SlsperId = s.SlsperId;
                t.InvtID = s.InvtID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Position = s.Position;
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPICustomer_Class(OM_KPICustomer_Class t, OM25100_pgKPICustomer_Class_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.CustID = s.CustID;
                t.ClassID = s.ClassID;
                t.SlsperId = s.SlsperId;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPICustomer_Invt(OM_KPICustomer_Invt t, OM25100_pgKPICustomer_Invt_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CycleNbr = s.CycleNbr;
                t.KPI = s.KPI;
                t.BranchID = s.BranchID;
                t.CustID = s.CustID;
                t.InvtID = s.InvtID;
                t.SlsperId = s.SlsperId;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_OM_KPICondition(ref OM_KPICondition t, OM25100_pgKPICondition_Result s, bool isNew)
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

        private void Update_OM_KPISalesClassDetail(ref OM_KPISalesClassDetail t, OM25100_pgSalesClassDetail_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ClassID = s.ClassID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
            }

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
                string CycleHeader = data["cboCycleNbr"].PassNull();
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
                            if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("Cycle")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID")
                            {
                                throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                            }

                            if (appTo == "A")
                            {
                                if (workSheet.Cells[0, 3].StringValue.Trim() != "Target")
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "I")
                            {
                                if (workSheet.Cells[0, 3].StringValue.Trim() != "InvtID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "Target")
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "G")
                            {
                                if (workSheet.Cells[0, 3].StringValue.Trim() != "ClassID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "Target")
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                        }
                        else if (appFor == "S")
                        {
                            if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("Cycle")
                                 || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                 || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                 || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID")
                            {
                                throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                            }

                            if (appTo == "A")
                            {
                                if (workSheet.Cells[0, 4].StringValue.Trim() != "Target")
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "I")
                            {
                                if (workSheet.Cells[0, 4].StringValue.Trim() != "InvtID"
                                  || workSheet.Cells[0, 5].StringValue.Trim() != "Target")
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "G")
                            {
                                if (workSheet.Cells[0, 4].StringValue.Trim() != "ClassID"
                                  || workSheet.Cells[0, 5].StringValue.Trim() != "Target")
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                        }
                        else if (appFor == "CUS")
                        {
                            if (!workSheet.Cells[0, 0].StringValue.Trim().StartsWith("Cycle")
                                  || workSheet.Cells[0, 1].StringValue.Trim() != "KPI"
                                  || workSheet.Cells[0, 2].StringValue.Trim() != "BranchID"
                                  || workSheet.Cells[0, 3].StringValue.Trim() != "SlsperID"
                                  || workSheet.Cells[0, 4].StringValue.Trim() != "CustID")
                            {
                                throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                            }
                            if (appTo == "A")
                            {
                                if (workSheet.Cells[0, 5].StringValue.Trim() != "Target"
                                  )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "I")
                            {
                                if (workSheet.Cells[0, 5].StringValue.Trim() != "InvtID"
                                     || workSheet.Cells[0, 6].StringValue.Trim() != "Target"
                                     )
                                {
                                    throw new MessageException(MessageType.Message, "2017011710", "", parm: new string[] { KPIheader });
                                }
                            }
                            else if (appTo == "G")
                            {
                                if (workSheet.Cells[0, 5].StringValue.Trim() != "ClassID"
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
                        string errorCycleNbr = string.Empty;
                        string errorCycleNbrnotExists = string.Empty;
                        string errorKPI = string.Empty;
                        string errorKPInotExists = string.Empty;
                        string errorKPIotherType = string.Empty;
                        string errorCycleCheckSeleted = string.Empty;
                        string errorBranchID = string.Empty;
                        string errorBranchIDnotExists = string.Empty;
                        string errBranchByUser = string.Empty;
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

                        var lstCycle = _db.OM25100_piCycle(_userName, _cpnyID, Current.LangID).ToList();
                        var lstKPI = _db.OM25100_piKPI(_userName, _cpnyID, Current.LangID).ToList();
                        var lstBranch = _db.OM25100_piBranchID(_userName, _cpnyID, Current.LangID).ToList();
                        var lstClass = _db.OM25100_piClassID(_userName, _cpnyID, Current.LangID).ToList();
                        var lstInvt = _db.OM25100_piInvtID(_userName, _cpnyID, Current.LangID).ToList();
                        var lstSlsper = _db.OM25100_piSlsperId(_userName, _cpnyID, Current.LangID).ToList();
                        var lstCust = _db.OM25100_piCustId(_userName, _cpnyID, Current.LangID).ToList();

                        List<OM_KPIHeader> lstOM_KPIHeader = new List<OM_KPIHeader>();
                        List<OM_KPICpny_All> lstOM_KPICpny_All = new List<OM_KPICpny_All>();
                        List<OM_KPICpny_Class> lstOM_KPICpny_Class = new List<OM_KPICpny_Class>();
                        List<OM_KPICpny_Invt> lstOM_KPICpny_Invt = new List<OM_KPICpny_Invt>();
                        List<OM_KPISales_All> lstOM_KPISales_All = new List<OM_KPISales_All>();
                        List<OM_KPISales_Class> lstOM_KPISales_Class = new List<OM_KPISales_Class>();
                        List<OM_KPISales_Invt> lstOM_KPISales_Invt = new List<OM_KPISales_Invt>();
                        List<OM_KPICustomer_All> lstOM_KPICustomer_All = new List<OM_KPICustomer_All>();
                        List<OM_KPICustomer_Class> lstOM_KPICustomer_Class = new List<OM_KPICustomer_Class>();
                        List<OM_KPICustomer_Invt> lstOM_KPICustomer_Invt = new List<OM_KPICustomer_Invt>();


                        #endregion

                        var showTabSalesClassDetail = false;
                        var objConfig = _db.OM25100_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                        if (objConfig != null)
                        {
                            showTabSalesClassDetail = objConfig.ShowTabSalesClassDetail.HasValue && objConfig.ShowTabSalesClassDetail.Value;
                        }

                        for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                        {
                            string CycleNbr = string.Empty;
                            string KPI = string.Empty;
                            string BranchID = string.Empty;
                            string Target = string.Empty;
                            string ClassID = string.Empty;
                            string InvtID = string.Empty;
                            string SlsperID = string.Empty;
                            string CustID = string.Empty;

                            var objCycle = new OM25100_piCycle_Result();
                            var objKPI = new OM25100_piKPI_Result();
                            var objBranch = new OM25100_piBranchID_Result();
                            var objClass = new OM25100_piClassID_Result();
                            var objInvt = new OM25100_piInvtID_Result();
                            var objSlsper = new OM25100_piSlsperId_Result();
                            var objCust = new OM25100_piCustId_Result();

                            bool flagCheck = false;

                            #region GetValue & CheckError
                            CycleNbr = workSheet.Cells[i, 0].StringValue.PassNull();
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

                            if (CycleNbr == "" && KPI == "")
                            {
                                continue;
                            }

                            if (CycleNbr == "")
                            {
                                errorCycleNbr += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }
                            else
                            {
                                if (CycleNbr != CycleHeader)
                                {
                                    errorCycleCheckSeleted += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else if (lstCycle.FirstOrDefault(p => p.CycleNbr == CycleNbr) == null)
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
                                {
                                    if (objBranch.AllowImport == false)
                                    {
                                        errBranchByUser += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                        flagBranchID = true;
                                    }
                                }
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
                                    if (showTabSalesClassDetail)
                                    {
                                        ClassID = "*";
                                    }
                                    else
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
                            }
                            else if (appFor == "CUS")
                            {
                                if (SlsperID == "")
                                {
                                    //errorSlsperID += (i + 1).ToString() + ",";
                                    //flagCheck = true;
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
                                        objCust = lstCust.FirstOrDefault(p => p.BranchID == BranchID && p.CustId == CustID && p.SlsperID == SlsperID); // && p.SlsperID == SlsperID
                                        if (objCust == null)
                                        {
                                            errorCustIDnotExists += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
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
                            var record = _db.OM_KPIHeader.FirstOrDefault(p => p.CycleNbr == CycleNbr && p.KPI == KPI);
                            //if (record != null)
                            //{
                            //    if (record.Status != "H")
                            //    {
                            //        errorStatus += (i + 1).ToString() + ",";
                            //        continue;
                            //    }
                            //}
                            var CheckRecordEmpity = lstOM_KPIHeader.Where(p => p.CycleNbr == CycleNbr && p.KPI == KPI).FirstOrDefault();
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
                                    record = new OM_KPIHeader();
                                    record.KPI = KPI;
                                    record.CycleNbr = CycleNbr;
                                    record.Status = "H";
                                    record.Crtd_DateTime = DateTime.Now;
                                    record.Crtd_User = Current.UserName;
                                    record.Crtd_Prog = _screenNbr;
                                    record.LUpd_DateTime = DateTime.Now;
                                    record.LUpd_User = Current.UserName;
                                    record.LUpd_Prog = _screenNbr;
                                    _db.OM_KPIHeader.AddObject(record);
                                }
                                lstOM_KPIHeader.Add(record);
                            }

                            if (appFor == "C")
                            {
                                #region -Company-
                                if (appTo == "A")
                                {
                                    //OM_KPICpny_All
                                    var recordExists_OM_KPICpny_All = lstOM_KPICpny_All.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                        && p.KPI == KPI
                                                                                                        && p.BranchID == BranchID);
                                    if (recordExists_OM_KPICpny_All == null)
                                    {
                                        var recordItem = _db.OM_KPICpny_All.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                            && p.KPI == KPI
                                                                                            && p.BranchID == BranchID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPICpny_All();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPICpny_All.AddObject(recordItem);
                                            lstOM_KPICpny_All.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;
                                    }
                                }
                                else if (appTo == "I")
                                {
                                    //OM_KPICpny_Invt
                                    var recordExists_OM_KPICpny_Invt = lstOM_KPICpny_Invt.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.InvtID == InvtID);
                                    if (recordExists_OM_KPICpny_Invt == null)
                                    {
                                        var recordItem = _db.OM_KPICpny_Invt.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.InvtID == InvtID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPICpny_Invt();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.InvtID = InvtID;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPICpny_Invt.AddObject(recordItem);
                                            lstOM_KPICpny_Invt.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    //OM_KPICpny_Class
                                    var recordExists_OM_KPICpny_Class = lstOM_KPICpny_Class.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.ClassID == ClassID);
                                    if (recordExists_OM_KPICpny_Class == null)
                                    {
                                        var recordItem = _db.OM_KPICpny_Class.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.ClassID == ClassID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPICpny_Class();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.ClassID = ClassID;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPICpny_Class.AddObject(recordItem);
                                            lstOM_KPICpny_Class.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;

                                    }
                                }
                                #endregion
                            }
                            else if (appFor == "S")
                            {
                                #region -Sales-
                                if (appTo == "A")
                                {
                                    //OM_KPISales_All
                                    var recordExists_OM_KPISales_All = lstOM_KPISales_All.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.SlsperId == SlsperID);
                                    if (recordExists_OM_KPISales_All == null)
                                    {
                                        var recordItem = _db.OM_KPISales_All.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                            && p.KPI == KPI
                                                                                            && p.BranchID == BranchID
                                                                                            && p.SlsperId == SlsperID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPISales_All();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.Position = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID).Position;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPISales_All.AddObject(recordItem);
                                            lstOM_KPISales_All.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;

                                    }
                                }
                                else if (appTo == "I")
                                {
                                    //OM_KPISales_Invt
                                    var recordExists_OM_KPISales_Invt = lstOM_KPISales_Invt.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                            && p.KPI == KPI
                                                                                                            && p.BranchID == BranchID
                                                                                                            && p.SlsperId == SlsperID
                                                                                                            && p.InvtID == InvtID);
                                    if (recordExists_OM_KPISales_Invt == null)
                                    {
                                        var recordItem = _db.OM_KPISales_Invt.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.InvtID == InvtID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPISales_Invt();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.InvtID = InvtID;
                                            recordItem.Position = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID).Position;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPISales_Invt.AddObject(recordItem);
                                            lstOM_KPISales_Invt.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;

                                    }
                                }
                                else if (appTo == "G")
                                {
                                    //OM_KPISales_Class
                                    var recordExists_OM_KPISales_Class = lstOM_KPISales_Class.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.ClassID == ClassID);
                                    if (recordExists_OM_KPISales_Class == null)
                                    {
                                        var recordItem = _db.OM_KPISales_Class.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.ClassID == ClassID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPISales_Class();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.ClassID = ClassID;
                                            recordItem.Position = lstSlsper.FirstOrDefault(p => p.BranchID == BranchID && p.SlsperId == SlsperID).Position;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPISales_Class.AddObject(recordItem);
                                            lstOM_KPISales_Class.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;

                                    }
                                }
                                #endregion
                            }
                            else if (appFor == "CUS")
                            {
                                #region -Customer-
                                if (appTo == "A")
                                {
                                    //OM_KPICustomer_All
                                    var recordExists_OM_KPICustomer_All = lstOM_KPICustomer_All.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.CustID == CustID);
                                    if (recordExists_OM_KPICustomer_All == null)
                                    {
                                        var recordItem = _db.OM_KPICustomer_All.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.CustID == CustID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPICustomer_All();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.CustID = CustID;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPICustomer_All.AddObject(recordItem);
                                            lstOM_KPICustomer_All.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;
                                    }
                                }
                                else if (appTo == "I")
                                {
                                    //OM_KPICustomer_Invt
                                    var recordExists_OM_KPICustomer_Invt = lstOM_KPICustomer_Invt.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.CustID == CustID
                                                                                                                && p.InvtID == InvtID);
                                    if (recordExists_OM_KPICustomer_Invt == null)
                                    {
                                        var recordItem = _db.OM_KPICustomer_Invt.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.CustID == CustID
                                                                                                && p.InvtID == InvtID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPICustomer_Invt();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.CustID = CustID;
                                            recordItem.InvtID = InvtID;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPICustomer_Invt.AddObject(recordItem);
                                            lstOM_KPICustomer_Invt.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;
                                    }
                                }
                                else if (appTo == "G")
                                {
                                    //OM_KPICustomer_Class
                                    var recordExists_OM_KPICustomer_Class = lstOM_KPICustomer_Class.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                                && p.KPI == KPI
                                                                                                                && p.BranchID == BranchID
                                                                                                                && p.SlsperId == SlsperID
                                                                                                                && p.CustID == CustID
                                                                                                                && p.ClassID == ClassID);
                                    if (recordExists_OM_KPICustomer_Class == null)
                                    {
                                        var recordItem = _db.OM_KPICustomer_Class.FirstOrDefault(p => p.CycleNbr == CycleNbr
                                                                                                && p.KPI == KPI
                                                                                                && p.BranchID == BranchID
                                                                                                && p.SlsperId == SlsperID
                                                                                                && p.CustID == CustID
                                                                                                && p.ClassID == ClassID);

                                        if (recordItem == null)
                                        {
                                            recordItem = new OM_KPICustomer_Class();
                                            recordItem.ResetET();
                                            recordItem.CycleNbr = CycleNbr;
                                            recordItem.KPI = KPI;
                                            recordItem.BranchID = BranchID;
                                            recordItem.SlsperId = SlsperID;
                                            recordItem.CustID = CustID;
                                            recordItem.ClassID = ClassID;

                                            recordItem.Crtd_DateTime = DateTime.Now;
                                            recordItem.Crtd_Prog = _screenNbr;
                                            recordItem.Crtd_User = _userName;
                                            _db.OM_KPICustomer_Class.AddObject(recordItem);
                                            lstOM_KPICustomer_Class.Add(recordItem);
                                        }
                                        recordItem.Target = Convert.ToDouble(Target);
                                        recordItem.LUpd_DateTime = DateTime.Now;
                                        recordItem.LUpd_Prog = _screenNbr;
                                        recordItem.LUpd_User = _userName;
                                    }
                                }
                                #endregion
                            }
                            #endregion
                        }// Vòng for

                        message = errorCycleNbr == "" ? "" : string.Format(Message.GetString("2016091412", null), "Cycle", errorCycleNbr);
                        message += errorCycleNbrnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "Cycle", errorCycleNbrnotExists);
                        message += errorKPI == "" ? "" : string.Format(Message.GetString("2016091412", null), "KPI", errorKPI);
                        message += errorKPInotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "KPI", errorKPInotExists);
                        message += errorKPIotherType == "" ? "" : string.Format(Message.GetString("2017011811", null), errorKPIotherType);
                        message += errorCycleCheckSeleted == "" ? "" : string.Format(Message.GetString("20180704", null), "Cycle", errorCycleCheckSeleted);
                        message += errorBranchID == "" ? "" : string.Format(Message.GetString("2016091412", null), "BranchID", errorBranchID);
                        message += errorBranchIDnotExists == "" ? "" : string.Format(Message.GetString("2016091413", null), "BranchID", errorBranchIDnotExists);
                        message += errBranchByUser == "" ? "" : string.Format(Message.GetString("2018122751", null), "BranchID", errBranchByUser, Current.UserName);
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

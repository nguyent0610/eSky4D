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

namespace OM25100.Controllers
{
    [DirectController]
    public class OM25100Controller : Controller
    {
        private string _screenNbr = "OM25100";
        private string _userName = Current.UserName;

        OM25100Entities _db = Util.CreateObjectContext<OM25100Entities>(false);
        //eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }
        
      //  [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }


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
        public ActionResult GetKPISales_All(string CycleNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_All = _db.OM25100_pgKPISales_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_All);
            //return this.Store(_db.OM25100_pgKPISales_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
        }
		public ActionResult GetKPICustomer_All(string CycleNbr, string KPI, string Zone, string Territory)
		{
			var lstKPISales_All = _db.OM25100_pgKPICustomer_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
			return this.Store(lstKPISales_All);
			//return this.Store(_db.OM25100_pgKPISales_All(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
		}
		public ActionResult GetKPICustomer_Class(string CycleNbr, string KPI, string Zone, string Territory)
		{
			var lstKPISales_Class = _db.OM25100_pgKPICustomer_Class(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
			return this.Store(lstKPISales_Class);
			//return this.Store(_db.OM25100_pgKPISales_Class(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
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
            //return this.Store(_db.OM25100_pgKPISales_Class(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList());
        }
        public ActionResult GetKPISales_Invt(string CycleNbr, string KPI, string Zone, string Territory)
        {
            var lstKPISales_Invt = _db.OM25100_pgKPISales_Invt(Current.UserName, Current.CpnyID, Current.LangID, CycleNbr, KPI, Zone, Territory).ToList();
            return this.Store(lstKPISales_Invt);
        }
       
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string CycleNbr = data["cboCycleNbr"].PassNull();
                string KPI = data["cboKPI"].PassNull();
				string handle = data["cboHandle"].PassNull();


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

                #region OM_KPIHeader
                var header = _db.OM_KPIHeader.FirstOrDefault(p => p.CycleNbr == CycleNbr && p.KPI == KPI);

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
						&& p.KPI.ToUpper() == del.KPI.ToUpper()&&p.CustID.ToUpper()==del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
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
					if (curItem.BranchID.PassNull() == "") continue;

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
						&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
					{
						stoOM_KPICustomer_Class.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
					}
					else
					{
						var objDel = _db.OM_KPICustomer_Class.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.ClassID.ToUpper() == del.ClassID.ToUpper()).FirstOrDefault();
						if (objDel != null)
						{
							_db.OM_KPICustomer_Class.DeleteObject(objDel);
						}
					}
				}



				foreach (OM25100_pgKPICustomer_Class_Result curItem in stoOM_KPICustomer_Class.Created)
				{
					if (curItem.BranchID.PassNull() == "") continue;

					var objCountry = _db.OM_KPICustomer_Class.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
						&& p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.ClassID.ToUpper() == curItem.ClassID.ToUpper()).FirstOrDefault();

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
						&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
					{
						stoOM_KPICustomer_Invt.Created.Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
					}
					else
					{
						var objDel = _db.OM_KPICustomer_Invt.ToList().Where(p => p.BranchID == del.BranchID && p.CycleNbr.ToUpper() == del.CycleNbr.ToUpper()
							&& p.KPI.ToUpper() == del.KPI.ToUpper() && p.CustID.ToUpper() == del.CustID.ToUpper() && p.SlsperId.ToUpper() == del.SlsperId.ToUpper() && p.InvtID.ToUpper() == del.InvtID.ToUpper()).FirstOrDefault();
						if (objDel != null)
						{
							_db.OM_KPICustomer_Invt.DeleteObject(objDel);
						}
					}
				}



				foreach (OM25100_pgKPICustomer_Invt_Result curItem in stoOM_KPICustomer_Invt.Created)
				{
					if (curItem.BranchID.PassNull() == "") continue;

					var objCountry = _db.OM_KPICustomer_Invt.Where(p => p.BranchID == curItem.BranchID && p.CycleNbr.ToUpper() == curItem.CycleNbr.ToUpper()
						&& p.KPI.ToUpper() == curItem.KPI.ToUpper() && p.CustID.ToUpper() == curItem.CustID.ToUpper() && p.SlsperId.ToUpper() == curItem.SlsperId.ToUpper() && p.InvtID.ToUpper() == curItem.InvtID.ToUpper()).FirstOrDefault();

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

                var obj = _db.OM_KPIHeader.FirstOrDefault(p => p.CycleNbr == CycleNbr && p.KPI==KPI);
                if (obj != null)
                {
                    _db.OM_KPIHeader.DeleteObject(obj);
                }

                var lstTargetGroupDet = _db.OM_KPICpny_All.Where(p => p.CycleNbr == CycleNbr && p.KPI==KPI).ToList();
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
            t.CpnyName = s.CpnyName;
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
            t.CpnyName = s.CpnyName;
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
            t.CpnyName = s.CpnyName;
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
            t.CpnyName = s.CpnyName;
            t.SlsperName = s.SlsperName;
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
            t.CpnyName = s.CpnyName;
            t.SlsperName = s.SlsperName;
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
            t.CpnyName = s.CpnyName;
            t.SlsperName = s.SlsperName;
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

		private void Update_OM_KPICustomer_Invt(OM_KPICustomer_Invt t, OM25100_pgKPICustomer_Invt_Result s, bool isNew)
		{
			if (isNew)
			{
				t.CycleNbr = s.CycleNbr;
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
    }
}

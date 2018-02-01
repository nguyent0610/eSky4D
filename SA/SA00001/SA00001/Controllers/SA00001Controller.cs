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
using System.Drawing;

namespace SA00001.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00001Controller : Controller
    {
        private string _screenNbr = "SA00001";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        private Int16 _langID = Current.LangID;
        string cpnyNPP = string.Empty;
        SA00001Entities _db = Util.CreateObjectContext<SA00001Entities>(true);
        public ActionResult Index()
        {
            bool allowSalesState = false
                , allowSalesDistrict = false;
            bool allowAddress2 = false;
            bool allowOwer = false;


            var objConfig = _db.SA00001_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                allowSalesState = objConfig.AllowSalesState.HasValue ? objConfig.AllowSalesState.Value : false;
                allowSalesDistrict = objConfig.AllowSalesDistrict.HasValue ? objConfig.AllowSalesDistrict.Value : false;
                allowAddress2= objConfig.allowAddress2.HasValue ? objConfig.allowAddress2.Value : false;
                allowOwer = objConfig.allowOwer.HasValue ? objConfig.allowOwer.Value : false;
            }

            ViewBag.allowSalesState = allowSalesState;
            ViewBag.allowSalesDistrict = allowSalesDistrict;
            ViewBag.allowAddress2 = allowAddress2;
            ViewBag.allowOwer = allowOwer;
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetSYS_Role()
        {
            return this.Store(_db.SA00001_pgLoadGridCompany(_userName, _cpnyID, _langID).ToList());
        }

        public ActionResult GetSys_CompanyAddr(string CpnyID)
        {
            return this.Store(_db.SA000001_pgCompanyAddr(CpnyID).ToList());
        }

        public ActionResult GetState(string Country, string listState)
        {
            var dataState = this.Store(_db.SA00001_pgState(Current.CpnyID, Current.UserName, Current.LangID, Country, listState).ToList());
            return dataState;
        }

        public ActionResult GetDistrict(string Country, string State, string listDistrict)
        {
            var dataDistrict = this.Store(_db.SA00001_pgDistrict(Current.CpnyID, Current.UserName, Current.LangID, Country, State, listDistrict).ToList());
            return dataDistrict;
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Cpny"]);
                ChangeRecords<SA00001_pgLoadGridCompany_Result> lstSYS_Cpny = dataHandler.BatchObjectData<SA00001_pgLoadGridCompany_Result>();
                lstSYS_Cpny.Created.AddRange(lstSYS_Cpny.Updated);
                cpnyNPP = data["txtCpnyID"];
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSys_CompanyAddr"]);
                ChangeRecords<SA000001_pgCompanyAddr_Result> lstSys_CompanyAddr = dataHandler1.BatchObjectData<SA000001_pgCompanyAddr_Result>();

                foreach (SA00001_pgLoadGridCompany_Result deleted in lstSYS_Cpny.Deleted)
                {
                    if (lstSYS_Cpny.Created.Where(p => p.CpnyID.ToLower() == deleted.CpnyID.ToLower()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstSYS_Cpny.Created.Where(p => p.CpnyID.ToLower() == deleted.CpnyID.ToLower()).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDelete = _db.SYS_Company.FirstOrDefault(p => p.CpnyID == deleted.CpnyID);
                        if (objDelete != null)
                        {
                            _db.SYS_Company.DeleteObject(objDelete);
                        }
                    }
                }
           
                foreach (SA00001_pgLoadGridCompany_Result curRow in lstSYS_Cpny.Created)
                {
                    if (curRow.CpnyID.PassNull() == "") continue;
                    var RowDB = _db.SYS_Company.FirstOrDefault(p => p.CpnyID.ToLower() == curRow.CpnyID.ToLower());
                    if (RowDB != null)
                    {
                        if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                        {
                            Update_Language(RowDB, curRow, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        RowDB = new SYS_Company();
                        RowDB.ResetET();                      
                        Update_Language(RowDB, curRow, true);
                        _db.SYS_Company.AddObject(RowDB);
                    }
                }
                
                foreach (SA000001_pgCompanyAddr_Result deleted in lstSys_CompanyAddr.Deleted)
                {
                    if (lstSys_CompanyAddr.Created.Where(p => p.CpnyID.ToLower() == deleted.CpnyID.ToLower()
                                                            && p.AddrID.ToLower() == deleted.AddrID.ToLower()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstSys_CompanyAddr.Created.Where(p => p.CpnyID.ToLower() == deleted.CpnyID.ToLower()).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDelete = _db.Sys_CompanyAddr.FirstOrDefault(p => p.CpnyID == deleted.CpnyID && p.AddrID == deleted.AddrID);
                        if (objDelete != null)
                        {
                            _db.Sys_CompanyAddr.DeleteObject(objDelete);
                        }
                    }
                }

                lstSys_CompanyAddr.Created.AddRange(lstSys_CompanyAddr.Updated);
                foreach (SA000001_pgCompanyAddr_Result curRow in lstSys_CompanyAddr.Created)
                {
                    if (curRow.AddrID.PassNull() == "") continue;

                    var RowDB = _db.Sys_CompanyAddr.FirstOrDefault(p => p.CpnyID.ToLower() == curRow.CpnyID.ToLower() && p.AddrID.ToLower() == curRow.AddrID.ToLower());

                    if (RowDB != null)
                    {
                        if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                        {
                            UpdatingSys_CompanyAddr(RowDB, curRow, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        RowDB = new Sys_CompanyAddr();
                        RowDB.ResetET();
                        UpdatingSys_CompanyAddr(RowDB, curRow, true);
                        _db.Sys_CompanyAddr.AddObject(RowDB);
                    }
                }
                _db.SaveChanges();
                return Json(new { success = true});
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void Update_Language(SYS_Company t, SA00001_pgLoadGridCompany_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CpnyID = s.CpnyID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.CpnyName = s.CpnyName;
            t.Address = s.Address;
            t.Address1 = s.Address1;
            t.Address2 = s.Address2;
            t.Status = s.Status;
            t.Tel = s.Tel;
            t.Fax = s.Fax;
            t.SalesState = s.SalesState;
            t.SalesDistrict = s.SalesDistrict;
            t.TaxRegNbr = s.TaxRegNbr;
            t.Channel = s.Channel;
            t.Territory = s.Territory;
            t.Country = s.Country;
            t.City = s.City;
            t.District = s.District;
            t.CpnyType = s.CpnyType;
            t.Email = s.Email;
            t.Owner = s.Owner;
            t.Plant = s.Plant;
            t.Deposit = s.Deposit;
            t.CreditLimit = s.CreditLimit;
            t.MaxValue = s.MaxValue;
            t.State = s.State;
            t.Zone = s.Zone;
            t.Lat = s.Lat;
            t.Lng = s.Lng;

        }

        private void UpdatingSys_CompanyAddr(Sys_CompanyAddr t, SA000001_pgCompanyAddr_Result s, bool isNew)
        {
            if (isNew)
            {
                t.AddrID = s.AddrID;
                t.CpnyID = cpnyNPP;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Addr1 = s.Addr1;
            t.Addr2 = s.Addr2;
            t.Attn = s.Attn;
            t.City = s.City;
            t.Country = s.Country;
            t.Fax = s.Fax;
            t.Name = s.Name;
            t.Phone = s.Phone;
            t.Salut = s.Salut;
            t.State = s.State;
            t.TaxId00 = s.TaxId00;
            t.TaxId01 = s.TaxId01;
            t.TaxId02 = s.TaxId02;
            t.TaxId03 = s.TaxId03;
            t.TaxLocId = s.TaxLocId;
            t.TaxRegNbr = s.TaxRegNbr;
            t.Zip = s.Zip;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

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
namespace SA00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00000Controller : Controller
    {
        private string _screenNbr = "SA00000";
        private string _userName = Current.UserName;
        SA00000Entities _db = Util.CreateObjectContext<SA00000Entities>(true);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        public ActionResult Index()
        {
            var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "SA00000PP");
            if (config != null )
            {
                 ViewBag.SA00000PP = config.IntVal;
            }
            else
            {
                ViewBag.SA00000PP = 0;
            }

            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public PartialViewResult BodyofND(string lang)
        {
            return PartialView();
        }

        #region Get information Company
        public ActionResult GetSYS_Company(string CpnyID)
        {
            return this.Store(_db.SA00000_pdHeader(Current.UserName,Current.CpnyID,Current.LangID,CpnyID).FirstOrDefault());
        }

        public ActionResult GetSys_CompanyAddr(string CpnyID)
        {
            return this.Store(_db.SA00000_pgCompanyAddr(CpnyID).ToList());
        }

        public ActionResult GetSYS_SubCompany(string CpnyID)
        {
            return this.Store(_db.SA00000_pgSubCompany(CpnyID).ToList());
        }
        #endregion

        #region Save & Update information Company
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string CpnyID = data["cboCpnyID"].PassNull();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Company"]);
                var curHeader = dataHandler.ObjectData<SA00000_pdHeader_Result>().FirstOrDefault();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSys_CompanyAddr"]);
                ChangeRecords<SA00000_pgCompanyAddr_Result> lstSys_CompanyAddr = dataHandler1.BatchObjectData<SA00000_pgCompanyAddr_Result>();

                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstSYS_SubCompany"]);
                ChangeRecords<SA00000_pgSubCompany_Result> lstSYS_SubCompany = dataHandler2.BatchObjectData<SA00000_pgSubCompany_Result>();

                #region Save Header Company
                var header = _db.SYS_Company.FirstOrDefault(p => p.CpnyID == CpnyID);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new SYS_Company();
                    header.ResetET();
                    header.CpnyID = CpnyID;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader);
                    _db.SYS_Company.AddObject(header);
                }
                #endregion

                #region Save Sys_CompanyAddr
                foreach (SA00000_pgCompanyAddr_Result deleted in lstSys_CompanyAddr.Deleted)
                {
                    if (lstSys_CompanyAddr.Created.Where(p => p.CpnyID.ToLower() == deleted.CpnyID.ToLower()
                                                            && p.AddrID.ToLower() == deleted.AddrID.ToLower()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstSys_CompanyAddr.Created.Where(p => p.CpnyID.ToLower() == deleted.CpnyID.ToLower()).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDelete = _db.Sys_CompanyAddr.FirstOrDefault(p => p.CpnyID == CpnyID && p.AddrID == deleted.AddrID);
                        if (objDelete != null)
                        {
                            _db.Sys_CompanyAddr.DeleteObject(objDelete);
                        }
                    }
                }

                lstSys_CompanyAddr.Created.AddRange(lstSys_CompanyAddr.Updated);

                foreach (SA00000_pgCompanyAddr_Result curRow in lstSys_CompanyAddr.Created)
                {
                    if (curRow.AddrID.PassNull() == "") continue;

                    var RowDB = _db.Sys_CompanyAddr.FirstOrDefault(p => p.CpnyID.ToLower() == CpnyID.ToLower() && p.AddrID.ToLower() == curRow.AddrID.ToLower());

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
                        RowDB.CpnyID = CpnyID;
                        UpdatingSys_CompanyAddr(RowDB, curRow, true);
                        _db.Sys_CompanyAddr.AddObject(RowDB);
                    }
                }
                #endregion

                #region Save SYS_SubCompany
                foreach (SA00000_pgSubCompany_Result deleted in lstSYS_SubCompany.Deleted)
                {
                    if (lstSYS_SubCompany.Created.Where(p => p.SubCpnyID.ToLower() == deleted.SubCpnyID.ToLower()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstSYS_SubCompany.Created.Where(p => p.SubCpnyID.ToLower() == deleted.SubCpnyID.ToLower()).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.SYS_SubCompany.FirstOrDefault(p => p.CpnyID == CpnyID && p.SubCpnyID == deleted.SubCpnyID);
                        if (del != null)
                        {
                            _db.SYS_SubCompany.DeleteObject(del);
                        }
                    }
                }

                lstSYS_SubCompany.Created.AddRange(lstSYS_SubCompany.Updated);

                foreach (SA00000_pgSubCompany_Result curRow in lstSYS_SubCompany.Created)
                {
                    if (curRow.SubCpnyID.PassNull() == "") continue;

                    var RowDB = _db.SYS_SubCompany.FirstOrDefault(p => p.CpnyID.ToLower() == CpnyID.ToLower() && p.SubCpnyID.ToLower() == curRow.SubCpnyID.ToLower());

                    if (RowDB != null)
                    {
                        if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                        {
                            UpdatingSYS_SubCompany(RowDB, curRow, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        RowDB = new SYS_SubCompany();
                        RowDB.ResetET();
                        RowDB.CpnyID = CpnyID;
                        RowDB.SubCpnyID = curRow.SubCpnyID;
                        UpdatingSYS_SubCompany(RowDB, curRow, true);
                        _db.SYS_SubCompany.AddObject(RowDB);
                    }
                }
                #endregion

                _db.SaveChanges();
                // sau khi save xong gọi tới hàm tạo user hoặc chuyển save, truyền xuống danh sách
                Dictionary<string, string> dicData = new Dictionary<string, string>();
                dicData.Add("@BranchID",header.CpnyID);
                dicData.Add("@UserManger",data["cboManager"]);
                dicData.Add("@BranchOld",data["cboBranchOld"]);
                dicData.Add("@SlsperID",data["cboSlsperID"]);

                Util.getDataTableFromProc("SA00000_ppUserSales", dicData, true);
                return Json(new { success = true, CpnyID = CpnyID }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        //Update Header Company
        private void UpdatingHeader(ref SYS_Company t,SA00000_pdHeader_Result s)
        {
            t.CpnyName = s.CpnyName;
            t.Address = s.Address;
            t.Address1 = s.Address1;
            t.Address2 = s.Address2;
            t.Tel = s.Tel;
            t.Fax = s.Fax;
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
            t.DatabaseName = s.DatabaseName;
            t.Deposit = s.Deposit;
            t.CreditLimit = s.CreditLimit;
            t.MaxValue = s.MaxValue;
            t.Type = s.Type;
            t.State = s.State;
            t.ReturnLimit = s.ReturnLimit;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        //Update SYS_SubCompany
        #region Update SYS_SubCompany
        private void UpdatingSYS_SubCompany(SYS_SubCompany t, SA00000_pgSubCompany_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
          
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
        #endregion

        //Update Sys_CompanyAddr
        #region Update SYS_CompanyAddr
        private void UpdatingSys_CompanyAddr(Sys_CompanyAddr t, SA00000_pgCompanyAddr_Result s, bool isNew)
        {
            if (isNew)
            {
                t.AddrID = s.AddrID;
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
        #endregion

        #region Delete information Company
        //Delete information Company
        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string CpnyID = data["cboCpnyID"].PassNull();
                var cpny = _db.SYS_Company.FirstOrDefault(p => p.CpnyID == CpnyID);
                if (cpny != null)
                {
                    _db.SYS_Company.DeleteObject(cpny);
                }

                var lstAddr = _db.Sys_CompanyAddr.Where(p => p.CpnyID == CpnyID).ToList();
                foreach (var item in lstAddr)
                {
                    _db.Sys_CompanyAddr.DeleteObject(item);
                }

                var lstSub = _db.SYS_SubCompany.Where(p => p.CpnyID == CpnyID).ToList();
                foreach (var item in lstSub)
                {
                    _db.SYS_SubCompany.DeleteObject(item);
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
        #endregion
    }
}

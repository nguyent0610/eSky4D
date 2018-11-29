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
using Aspose.Cells;
using System.Drawing;
using System.Text.RegularExpressions;
using HQFramework.Common;
using HQFramework.DAL;

namespace SA00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00000Controller : Controller
    {
        private string _screenNbr = "SA00000";
        private string _userName = Current.UserName;
        private JsonResult _logMessage;
        SA00000Entities _db = Util.CreateObjectContext<SA00000Entities>(true);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "SA00000PP");
            ViewBag.SA00000PP = config != null ? config.IntVal : 0;
            bool showSalesState = false;
            bool allowAddress2 = false;
            bool allowOwer = false;
            bool showCountSiteID = false;
            bool showExcel = false;
            var objConfig = _db.SA00000_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                showSalesState = objConfig.Show.HasValue && objConfig.Show.Value;
                allowAddress2 = objConfig.allowAddress2.HasValue && objConfig.allowAddress2.Value;
                allowOwer = objConfig.allowOwer.HasValue && objConfig.allowOwer.Value;
                showCountSiteID = objConfig.ShowCountSiteID.HasValue && objConfig.ShowCountSiteID.Value;
                showExcel = objConfig.AllowExcel ?? false;
            }

            ViewBag.showSalesState = showSalesState;
            ViewBag.allowAddress2 = allowAddress2;
            ViewBag.allowOwer = allowOwer;
            ViewBag.showExcel = showExcel;
            ViewBag.showCountSiteID = showCountSiteID;
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
            return this.Store(_db.SA00000_pdHeader(Current.UserName, Current.CpnyID, Current.LangID, CpnyID).FirstOrDefault());
        }

        public ActionResult GetSys_CompanyAddr(string CpnyID)
        {
            return this.Store(_db.SA00000_pgCompanyAddr(CpnyID).ToList());
        }

        public ActionResult GetState(string Country, string listState)
        {
            var dataState = this.Store(_db.SA00000_pgState(Current.CpnyID, Current.UserName, Current.LangID, Country, listState).ToList());
            return dataState;
        }

        public ActionResult GetDistrict(string Country, string State, string listDistrict)
        {
            var dataDistrict = this.Store(_db.SA00000_pgDistrict(Current.CpnyID, Current.UserName, Current.LangID, Country, State, listDistrict).ToList());
            return dataDistrict;
        }

        public ActionResult GetSYS_SubCompany(string CpnyID)
        {
            return this.Store(_db.SA00000_pgSubCompany(CpnyID).ToList());
        }

        public ActionResult GetUsers()
        {
            return this.Store(_db.SA00000_pgLoadUsers(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        #endregion

        #region Save & Update information Company
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data,string status)
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
                        UpdatingHeader(ref header, curHeader, status);
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
                    UpdatingHeader(ref header, curHeader, status);
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
                dicData.Add("@BranchID", header.CpnyID ?? "");
                dicData.Add("@UserManger", data["txtManager"] ?? "");
                dicData.Add("@BranchOld", data["cboBranchOld"] ?? "");
                dicData.Add("@SlsperID", data["cboSlsperID"] ?? "");
                dicData.Add("@DisplayID", data["cboTDisplayID"] ?? "");
                dicData.Add("@AccumulateID", data["cboAccumulatedID"] ?? "");
                dicData.Add("@UserID",_userName);

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
        private void UpdatingHeader(ref SYS_Company t, SA00000_pdHeader_Result s,string status)
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
            t.Lat = s.Lat;
            t.Status = status;
            t.Lng = s.Lng;
            t.CountSiteID = s.CountSiteID;
            t.SalesDistrict = s.SalesDistrict;
            t.SalesState = s.SalesState;
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


        #region Excel


        [HttpPost]
        public ActionResult Import()
        {
            var colTexts = HeaderExcel();
            var dataRowIdx = 1;
            FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
            HttpPostedFile file = fileUploadField.PostedFile;
            FileInfo fileInfo = new FileInfo(file.FileName);
            List<string> lstCpnyID = new List<string>();
            try
            {

                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    Worksheet workSheet = workbook.Worksheets[0];
                    var message = "";
               
                    var regex = new Regex(@"^-*[0-9,\. ]+$");

                    if (workSheet.Cells.MaxDataRow < 1)
                    {
                        throw new MessageException(MessageType.Message, "2018062955");
                    }

                    string errCpnyID = String.Empty;
                    string cpnyNameNull = String.Empty;
                    string addressNull = String.Empty;
                    string phoneNull = String.Empty;
                    string faxNull = String.Empty;
                    string taxRegNbrNull = String.Empty;
                    string channelNull = String.Empty;
                    string territoryNull = String.Empty;
                    string countryNull = String.Empty;
                    string stateNull = String.Empty;
                    string districtNull = String.Empty;
                    string cpnyTypeNull = String.Empty;
                    string emailNull = String.Empty;
                    string ownerNull = String.Empty;

                    for (int i = dataRowIdx; i <= workSheet.Cells.MaxDataRow; i++) //index luôn đi từ 0
                    {
                        #region -Get data from excel-

                        bool flagCheck = false;
                        string cpnyID = workSheet.Cells[i, colTexts.IndexOf("CpnyID")].StringValue.Trim();
                        string cpnyName = workSheet.Cells[i, colTexts.IndexOf("CpnyName")].StringValue.Trim();
                        string address = workSheet.Cells[i, colTexts.IndexOf("Address")].StringValue.Trim();
                        string phone = workSheet.Cells[i, colTexts.IndexOf("Phone")].StringValue.Trim();
                        string fax = workSheet.Cells[i, colTexts.IndexOf("Fax")].StringValue.Trim();
                        string taxRegNbr = workSheet.Cells[i, colTexts.IndexOf("TaxRegNbr")].StringValue.Trim();
                        string channel = workSheet.Cells[i, colTexts.IndexOf("Channel")].StringValue.Trim();
                        string territory = workSheet.Cells[i, colTexts.IndexOf("Territory")].StringValue.Trim();
                        string country = workSheet.Cells[i, colTexts.IndexOf("Country")].StringValue.Trim();
                        string state = workSheet.Cells[i, colTexts.IndexOf("State")].StringValue.Trim();
                        string district = workSheet.Cells[i, colTexts.IndexOf("District")].StringValue.Trim();
                        string cpnyType = workSheet.Cells[i, colTexts.IndexOf("CpnyType")].StringValue.Trim();
                        string email = workSheet.Cells[i, colTexts.IndexOf("EMAIL")].StringValue.Trim();
                        string owner = workSheet.Cells[i, colTexts.IndexOf("Owner")].StringValue.Trim();
                        double creditLimit = 0;
                        try
                        {
                            creditLimit = workSheet.Cells[i, colTexts.IndexOf("CreditLimit")].DoubleValue;
                        }
                        catch
                        {
                            creditLimit = 0;
                        }

                        #region Checkdata

                        if (string.IsNullOrEmpty(cpnyName))
                        {
                            cpnyNameNull += (i + 1) + ", ";
                            flagCheck = true;
                        }
                        if (string.IsNullOrEmpty(address))
                        {
                            addressNull += (i + 1) + ", ";
                            flagCheck = true;
                        }
                        if (string.IsNullOrEmpty(phone))
                        {
                            phoneNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(fax))
                        {
                            faxNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(taxRegNbr))
                        {
                            taxRegNbrNull += (i + 1) + ", ";
                            flagCheck = true;
                        }
                        if (string.IsNullOrEmpty(channel))
                        {
                            channelNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(territory))
                        {
                            territoryNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(country))
                        {
                            countryNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(state))
                        {
                            stateNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(district))
                        {
                            districtNull += (i + 1) + ", ";
                            flagCheck = true;
                        }
                        if (string.IsNullOrEmpty(cpnyType))
                        {
                            cpnyTypeNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(email))
                        {
                            emailNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (string.IsNullOrEmpty(owner))
                        {
                            ownerNull += (i + 1) + ", ";
                            flagCheck = true;
                        }

                        if (lstCpnyID.Any(x => x == cpnyID))
                        {
                            errCpnyID += (i + 1) + ", ";
                            flagCheck = true;
                        }
                        #endregion
                  

                        if (cpnyID.Length > 0 && !flagCheck)
                        {
                            lstCpnyID.Add(cpnyID);
                            var objCpny = _db.SYS_Company.FirstOrDefault(x => x.CpnyID.ToLower() == cpnyID.ToLower());
                            if (objCpny == null)
                            {
                                objCpny = new SYS_Company();
                                objCpny.CpnyID = cpnyID;

                                objCpny.Crtd_DateTime = DateTime.Now;
                                objCpny.Crtd_User = Current.UserName;
                                objCpny.Crtd_Prog = _screenNbr;
                                _db.SYS_Company.AddObject(objCpny);
                            }

                            objCpny.CpnyName = cpnyName;
                            objCpny.Address = address;
                            objCpny.Tel = phone;
                            objCpny.Fax = fax;
                            objCpny.TaxRegNbr = taxRegNbr;
                            objCpny.Channel = GetCodeFromExcel(channel);
                            objCpny.Territory = GetCodeFromExcel(territory);
                            objCpny.Country =GetCodeFromExcel(country);
                            objCpny.State = GetCodeFromExcel(state);
                            objCpny.District = GetCodeFromExcel(district);
                            objCpny.CpnyType = GetCodeFromExcel(cpnyType);
                            objCpny.Email = email;
                            objCpny.Owner = owner;
                            objCpny.DatabaseName = "";
                            objCpny.City = "";
                            objCpny.Address1 = "";
                            objCpny.Address2 = "";
                            objCpny.Plant = "";
                            objCpny.Type = "";
                            objCpny.Lng = 0;
                            objCpny.Lat = 0;
                            objCpny.ReturnLimit = 0;
                            objCpny.SalesState = "";
                            objCpny.CountSiteID = 0;
                            objCpny.CreditLimit = creditLimit;  
                            objCpny.Status = "AC";
                            objCpny.LUpd_DateTime = DateTime.Now;
                            objCpny.LUpd_User = Current.UserName;
                            objCpny.LUpd_Prog = _screenNbr;
                        
                        }
                        #endregion
                    }

                    message = cpnyNameNull == "" ? "" : string.Format(Message.GetString("2018082851", null), cpnyNameNull.TrimEnd(','), Util.GetLang("CpnyName"));
                    message += addressNull == "" ? "" : string.Format(Message.GetString("2018082851", null), addressNull.TrimEnd(','), Util.GetLang("Address"));
                    message += phoneNull == "" ? "" : string.Format(Message.GetString("2018082851", null), phoneNull.TrimEnd(','), Util.GetLang("Phone"));
                    message += faxNull == "" ? "" : string.Format(Message.GetString("2018082851", null), faxNull.TrimEnd(','), Util.GetLang("Fax"));
                    message += taxRegNbrNull == "" ? "" : string.Format(Message.GetString("2018082851", null), taxRegNbrNull.TrimEnd(','), Util.GetLang("TaxRegNbr"));
                    message += channelNull == "" ? "" : string.Format(Message.GetString("2018082851", null), channelNull.TrimEnd(','), Util.GetLang("Channel"));
                    message += territoryNull == "" ? "" : string.Format(Message.GetString("2018082851", null), territoryNull.TrimEnd(','), Util.GetLang("Territory"));
                    message += countryNull == "" ? "" : string.Format(Message.GetString("2018082851", null), countryNull.TrimEnd(','), Util.GetLang("Country"));
                    message += stateNull == "" ? "" : string.Format(Message.GetString("2018082851", null), stateNull.TrimEnd(','), Util.GetLang("State"));
                    message += districtNull == "" ? "" : string.Format(Message.GetString("2018082851", null), districtNull.TrimEnd(','), Util.GetLang("District"));
                    message += cpnyTypeNull == "" ? "" : string.Format(Message.GetString("2018082851", null), cpnyTypeNull.TrimEnd(','), Util.GetLang("CpnyType"));
                    message += emailNull == "" ? "" : string.Format(Message.GetString("2018082851", null), emailNull.TrimEnd(','), Util.GetLang("EMAIL"));
                    message += ownerNull == "" ? "" : string.Format(Message.GetString("2018082851", null), ownerNull.TrimEnd(','), Util.GetLang("Owner"));

                    message += errCpnyID == "" ? "" : string.Format(Message.GetString("2018112951", null), errCpnyID.TrimEnd(','));

                    if (string.IsNullOrEmpty(message))
                    {
                        _db.SaveChanges();
                    }
                    Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "148");
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

            return _logMessage;

        }

        [HttpPost]
        public ActionResult Export(FormCollection data)
        {

            var ColTexts = HeaderExcel();
           
            Stream stream = new MemoryStream();
            Workbook workbook = new Workbook();
            workbook.Worksheets.Add();
            Cell cell;
            Worksheet SheetMCP = workbook.Worksheets[0];
            Worksheet MasterData = workbook.Worksheets[1];

            DataAccess dal = Util.Dal();
            SheetMCP.Name = Util.GetLang("SA00000NameSheet");
            MasterData.Name = "MasterData";
            #region header info
            // Header text columns
            SheetMCP.AutoFitColumns();

            for (int i = 0; i < ColTexts.Count; i++)
            {
                SetCellValue(SheetMCP.Cells[0, i], Util.GetLang(ColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, false, false);
                SheetMCP.Cells.SetColumnWidth(i, 25);
            }

            #endregion
            var allColumns = new List<string>();
            allColumns.AddRange(ColTexts);

 
            SheetMCP.Cells.SetRowHeight(0, 30);

            var style = workbook.GetStyleInPool(0);


            StyleFlag flag = new StyleFlag();
            Range range;
            //#region GetDataToComboExcel



            ////lấy data cho combo Channel
            ParamCollection pc = new ParamCollection();
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtChannel = dal.ExecDataTable("SA00000_peAR_Channel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtChannel, true, 0, 0, false);



            ////lấy data cho combo CpnyType
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtCpnyType = dal.ExecDataTable("SA00000_peCpnyType", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtCpnyType, true, 0, 1, false);


            ////lấy data cho combo Territory
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtTerritory = dal.ExecDataTable("SA00000_peTerritory", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtTerritory, true, 0, 2, false);

            ////lấy data cho combo CountryID
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtCountry = dal.ExecDataTable("SA00000_peCountryID", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtCountry, true, 0, 3, false);

            ////lấy data cho combo State
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtState = dal.ExecDataTable("SA00000_peState", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtState, true, 0, 5, false);




            //lấy data cho combo District
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.String, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtDistrict = dal.ExecDataTable("SA00000_peDistrict", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtDistrict, true, 0, 15 , false);




            //#region formular
            ////Channel
            string formulaGender = "=MasterData!$A$2:$A$" + (dtChannel.Rows.Count + 2);
            Validation validation = GetValidation(ref SheetMCP, formulaGender, "Chọn Kênh", "Mã kênh này không tồn tại");
            validation.AddArea(GetCellArea(1, dtChannel.Rows.Count + 100, ColTexts.IndexOf("Channel")));

            ////CpnyType
            string formulaMarialStatus = "=MasterData!$B$2:$B$" + (dtCpnyType.Rows.Count + 2);
            validation = GetValidation(ref SheetMCP, formulaMarialStatus, "Chọn Loại Công Ty", "Mã Công ty này không tồn tại");
            validation.AddArea(GetCellArea(1, dtCpnyType.Rows.Count + 100, ColTexts.IndexOf("CpnyType")));

            //dtTerritory
            string formulaFiPerson = "=MasterData!$C$2:$C$" + (dtTerritory.Rows.Count + 2);
            validation = GetValidation(ref SheetMCP, formulaFiPerson, "Chọn Vùng", "Mã Vùng này không tồn tại");
            validation.AddArea(GetCellArea(1, dtTerritory.Rows.Count + 100, ColTexts.IndexOf("Territory")));

            ////Country
            string formulaAddressType = "=MasterData!$D$2:$D$" + (dtCountry.Rows.Count + 2);
            validation = GetValidation(ref SheetMCP, formulaAddressType, "Chọn Đất Nước", "Mã Đất Nước này không tồn tại");
            validation.AddArea(GetCellArea(1, dtCountry.Rows.Count + 100, ColTexts.IndexOf("Country")));


            ////State
            string formulaState = string.Format("=OFFSET(MasterData!$G$1,IFERROR(MATCH(H{0},MasterData!$F$2:$F${1},0),{2}),0, IF(COUNTIF(MasterData!$F$2:$F${1},H{0})=0,1,COUNTIF(MasterData!$F$2:$F${1},H{0})),1)",
                   new string[] { "2", (dtState.Rows.Count + 100).ToString(), (dtState.Rows.Count + 64).ToString() });

            validation = GetValidation(ref SheetMCP, formulaState, "Chọn Tỉnh", "Mã Tỉnh này không tồn tại");
            validation.AddArea(GetCellArea(1, dtState.Rows.Count + 100, ColTexts.IndexOf("State")));


       


            ////District
            string formulaDistrict = string.Format("=OFFSET(MasterData!$Q$1,IFERROR(MATCH(J{0},MasterData!$P$2:$P${1},0),{2}),0, IF(COUNTIF(MasterData!$P$2:$P${1},J{0})=0,1,COUNTIF(MasterData!$P$2:$P${1},J{0})),1)",
                   new string[] { "2", (dtDistrict.Rows.Count + 100).ToString(), (dtDistrict.Rows.Count + 64).ToString() });

            validation = GetValidation(ref SheetMCP, formulaDistrict, "Chọn Quận Huyện", "Mã quận huyện này không tồn tại");
            validation.AddArea(GetCellArea(1, dtDistrict.Rows.Count + 100, ColTexts.IndexOf("District")));

            //#endregion

   

            MasterData.Protect(ProtectionType.All);
            MasterData.VisibilityType = VisibilityType.Hidden;



            workbook.Save(stream, SaveFormat.Xlsx);
            stream.Flush();
            stream.Position = 0;

            return new FileStreamResult(stream, "application/vnd.ms-excel")
            {
                FileDownloadName = string.Format("{0}.xlsx", Util.GetLang("SA00000_excel"))
            };


        }

        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle, bool isBackground, bool isWrapsTex)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
            {
                style.Font.Color = Color.Red;
            }
            if (isBackground)
            {
                style.Font.Color = Color.Red;
                style.Pattern = BackgroundType.Solid;
                style.ForegroundColor = Color.Yellow;
            }
            if (isWrapsTex)
            {
                style.IsTextWrapped = true;
            }
            c.SetStyle(style);
        }


        private Validation GetValidation(ref Worksheet SheetMCP, string formular1, string inputMess, string errMess)
        {
            var validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
            validation.IgnoreBlank = true;
            validation.Type = Aspose.Cells.ValidationType.List;
            validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
            validation.Operator = OperatorType.Between;
            validation.Formula1 = formular1;
            validation.InputTitle = "";
            validation.InputMessage = inputMess;
            validation.ErrorMessage = errMess;
            return validation;
        }
        private CellArea GetCellArea(int startRow, int endRow, int columnIndex, int endColumnIndex = -1)
        {
            var area = new CellArea();
            area.StartRow = startRow;
            area.EndRow = endRow;
            area.StartColumn = columnIndex;
            area.EndColumn = endColumnIndex == -1 ? columnIndex : endColumnIndex;
            return area;
        }
        private List<string> HeaderExcel()
        {
            return new List<string>() { "CpnyID", "CpnyName", "Address", "Phone", "Fax", "TaxRegNbr", "Channel", "Territory", "Country", "State",  "District", "CpnyType", "EMAIL", "Owner", "CreditLimit" };
        }


        private string Getcell(int column) // Hàm bị sai khi lấy vị trí column AA
        {
            if (column == 0)
            {
                return "A";
            }
            bool flag = false;
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 >= 1)
            {
                cell += ABC.Substring((column / 26) - 1, 1);
                column = column - 26;
                flag = true;

            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            else
            {
                if (column % 26 == 0 && flag)
                {
                    cell += ABC.Substring(0, 1);
                }
            }

            return cell;
        }
        // Get Code from execl 
        private string GetCodeFromExcel(string codeDescr)
        {
            int index = codeDescr.IndexOf("-", StringComparison.Ordinal);
            return index > 0 ? codeDescr.Substring(0, index).Trim() : codeDescr.Trim();
        }





        #endregion




    }
}

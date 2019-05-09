using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
//using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Aspose.Cells;
using HQFramework.Common;
using HQFramework.DAL;
using HQSendMailApprove;
using IN10700.Models;
namespace IN10700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10700Controller : Controller
    {
        private string _screenNbr = "IN10700";
        IN10700Entities _db = Util.CreateObjectContext<IN10700Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetStockOutlet(string branchID, string slsperID, string custID, string stockType, DateTime stkOutDate, string invtType)
        {
            var outlet = _db.IN10700_phStockOutlet(Current.UserName, Current.CpnyID, branchID, slsperID, custID, stockType, stkOutDate, invtType).ToList();
            return this.Store(outlet);
        }

        public ActionResult GetStockOutletDet(string branchID, string slsperID, string stkOutNbr, string invtType)
        {
            return this.Store(_db.IN10700_pgStockOutletDet(Current.UserName, Current.CpnyID, branchID, slsperID, stkOutNbr, invtType).ToList());
        }
        public ActionResult GetStockOutletPOSM(string branchID, string slsperID, string stkOutNbr)
        {
            return this.Store(_db.IN10700_pgStockOutletDetPOSM(Current.UserName, Current.CpnyID, branchID, slsperID, stkOutNbr).ToList());
        }

        public ActionResult SaveData(FormCollection data, bool isNew)
        {
            try
            {
                var lstStockOutletHandler = new StoreDataHandler(data["lstStockOutlet"]);
                var inputStockOutlet = lstStockOutletHandler.ObjectData<IN10700_phStockOutlet_Result>().FirstOrDefault();



                var lstStockOutletDetChangeHandler = new StoreDataHandler(data["lstStockOutletDetChange"]);
                var lstStockOutletDetChange = lstStockOutletDetChangeHandler.BatchObjectData<IN10700_pgStockOutletDet_Result>();

                var lstStockOutletPOSMHandle = new StoreDataHandler(data["lstStockOutletPOSM"]);
                var lstStockOutletPOSM = lstStockOutletPOSMHandle.BatchObjectData<IN10700_pgStockOutletDetPOSM_Result>();
             

                var outlet = _db.PPC_StockOutlet.FirstOrDefault(o => o.BranchID == inputStockOutlet.BranchID 
                    && o.SlsPerID == inputStockOutlet.SlsPerID 
                    && o.StkOutNbr == inputStockOutlet.StkOutNbr);
                #region Header
                if (outlet != null)
                {
                    //update
                    if(!isNew)
                    {
                        if (outlet.tstamp.ToHex() == inputStockOutlet.tstamp.ToHex())
                        {
                            // update info
                            updateStockOutlet(ref outlet, inputStockOutlet, false);
                        }
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "2000", "", new string[]{
                                    Util.GetLang("StkOutNbr")
                                });
                    }
                }
                else { 
                    //new
                    if (isNew)
                    {
                        var setup = _db.IN_Setup.FirstOrDefault(s => s.SetupID == "IN" && s.BranchID == inputStockOutlet.BranchID);
                        if (setup != null)
                        {
                            var newStkOut = _db.IN10700_ppStkOutNbr(Current.CpnyID, Current.UserName, inputStockOutlet.BranchID).FirstOrDefault();
                            if (newStkOut != null)
                            {
                                inputStockOutlet.StkOutNbr = newStkOut.PrefixBat + newStkOut.LastStkOutNbr;

                                var outletAuto = _db.PPC_StockOutlet.FirstOrDefault(c => c.StkOutNbr == inputStockOutlet.StkOutNbr && c.BranchID == inputStockOutlet.BranchID);
                                if (outletAuto != null)
                                {
                                    throw new MessageException(MessageType.Message, "8001", "", new string[] { string.Format("{0}: {1}", Util.GetLang("StkOutNbr"), inputStockOutlet.StkOutNbr) });
                                }
                                //add new outlet
                                updateStockOutlet(ref outlet, inputStockOutlet, true);
                                _db.PPC_StockOutlet.AddObject(outlet);

                                setup.LastStkOutNbr = newStkOut.LastStkOutNbr;
                                setup.LUpd_DateTime = DateTime.Now;
                                setup.LUpd_Prog = _screenNbr;
                                setup.LUpd_User = Current.UserName;
                            }
                            else {
                                throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("Setup") });
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "20404", "", new string[] { Util.GetLang("BranchID") });
                        }
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                #endregion

                #region Detail
                lstStockOutletDetChange.Updated.AddRange(lstStockOutletDetChange.Created);
                foreach (var deleted in lstStockOutletDetChange.Deleted)
                {
                    if (!string.IsNullOrWhiteSpace(deleted.InvtID) )
                    {
                        if (lstStockOutletDetChange.Updated.Where(p => p.InvtID == deleted.InvtID && p.ExpDate==deleted.ExpDate).Count() == 0)
                        {
                            deleted.StkOutNbr = outlet.StkOutNbr;
                            deleted.BranchID = outlet.BranchID;
                            deleted.SlsperID = outlet.SlsPerID;

                            var deletedDetail = _db.PPC_StockOutletDet.FirstOrDefault(
                               x => x.BranchID == deleted.BranchID
                                    && x.StkOutNbr == deleted.StkOutNbr
                                    && x.SlsPerID == deleted.SlsperID
                                    && x.InvtID == deleted.InvtID
                                    && x.ExpDate == deleted.ExpDate);
                            if (deletedDetail != null)
                            {
                                _db.PPC_StockOutletDet.DeleteObject(deletedDetail);
                            }

                            var lstdeletedPOSM = _db.PPC_StockOutletPOSM.Where(
                              x => x.BranchID == outlet.BranchID
                                   && x.StkOutNbr == outlet.StkOutNbr
                                   && x.SlsPerID == outlet.SlsPerID
                                   && x.InvtID == deleted.InvtID).ToList();
                            foreach (var obj in lstdeletedPOSM)
                            {
                                _db.PPC_StockOutletPOSM.DeleteObject(obj);
                            }
                        }
                        else
                        {
                            lstStockOutletDetChange.Updated.Where(p => p.InvtID == deleted.InvtID).FirstOrDefault().tstamp = deleted.tstamp;
                        }


                    }
                }

                foreach (var updated in lstStockOutletDetChange.Updated)
                {
                    if (!string.IsNullOrWhiteSpace(updated.InvtID))
                    {
                        updated.StkOutNbr = outlet.StkOutNbr;
                        updated.BranchID = outlet.BranchID;
                        updated.SlsperID = outlet.SlsPerID;

                        var updatedDetail = _db.PPC_StockOutletDet.FirstOrDefault(
                            x => x.BranchID == updated.BranchID
                                && x.StkOutNbr == updated.StkOutNbr
                                && x.SlsPerID == updated.SlsperID
                                && x.InvtID == updated.InvtID
                                && x.ExpDate == updated.ExpDate);
                        if (updatedDetail != null)
                        {
                            if (updatedDetail.tstamp.ToHex() == updated.tstamp.ToHex())
                            {
                                updateStockOutletDet(ref updatedDetail, updated, false);
                            } 
                            else throw new MessageException(MessageType.Message, "19");
                        }
                        else
                        {
                            updateStockOutletDet(ref updatedDetail, updated, true);
                            _db.PPC_StockOutletDet.AddObject(updatedDetail);
                        }
                    }
                }

              
                #endregion


                #region Detail POSM
                lstStockOutletPOSM.Updated.AddRange(lstStockOutletPOSM.Created);
                foreach (var deleted in lstStockOutletPOSM.Deleted)
                {
                    if (!string.IsNullOrWhiteSpace(deleted.PosmID) && lstStockOutletDetChange.Updated.Where(p => p.InvtID == deleted.InvtID).Count() > 0)
                    {
                        if (lstStockOutletPOSM.Updated.Where(p => p.InvtID == deleted.InvtID && p.PosmID==deleted.PosmID && p.ExpDate==deleted.ExpDate).Count() == 0)
                        {
                            var deletedDetail = _db.PPC_StockOutletPOSM.FirstOrDefault(
                               x => x.BranchID == outlet.BranchID
                                    && x.StkOutNbr == outlet.StkOutNbr
                                    && x.SlsPerID == outlet.SlsPerID
                                    && x.InvtID == deleted.InvtID
                                    && x.PosmID == deleted.PosmID
                                    && x.ExpDate == deleted.ExpDate);
                            if (deletedDetail != null)
                            {
                                _db.PPC_StockOutletPOSM.DeleteObject(deletedDetail);
                            }
                        }
                        else
                        {
                            lstStockOutletPOSM.Updated.Where(p => p.InvtID == deleted.InvtID).FirstOrDefault().tstamp = deleted.tstamp;
                        }
                    }
                }
                foreach (var updated in lstStockOutletPOSM.Updated)
                {
                    if (!string.IsNullOrWhiteSpace(updated.PosmID))
                    {                    
                        var updatedDetail = _db.PPC_StockOutletPOSM.FirstOrDefault(
                            x => x.BranchID == outlet.BranchID
                                && x.StkOutNbr == outlet.StkOutNbr
                                && x.SlsPerID == outlet.SlsPerID
                                && x.InvtID == updated.InvtID
                                && x.PosmID == updated.PosmID
                                && x.ExpDate == updated.ExpDate);
                        if (updatedDetail != null)
                        {
                            if (updatedDetail.tstamp.ToHex() == updated.tstamp.ToHex())
                            {
                                updateStockOutletPOSM(ref updatedDetail, updated, outlet, false);
                            }
                            else throw new MessageException(MessageType.Message, "19"); 
                        }
                        else
                        {
                            updateStockOutletPOSM(ref updatedDetail, updated,outlet, true);
                            _db.PPC_StockOutletPOSM.AddObject(updatedDetail);
                        }
                    }
                }               
                #endregion
                _db.SaveChanges();

                return Json(new { success = true, msgCode = 201405071 });
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

        private void updateStockOutletDet(ref PPC_StockOutletDet updatedDetail, IN10700_pgStockOutletDet_Result updated, bool isNew)
        {
            if (isNew) 
            {
                updatedDetail = new PPC_StockOutletDet();
                updatedDetail.ResetET();
                updatedDetail.BranchID = updated.BranchID;
                updatedDetail.SlsPerID = updated.SlsperID;
                updatedDetail.StkOutNbr = updated.StkOutNbr;

                updatedDetail.InvtID = updated.InvtID;
                updatedDetail.ExpDate = updated.ExpDate.PassMin().Date;

                updatedDetail.Crtd_DateTime = DateTime.Now;
                updatedDetail.Crtd_Prog = _screenNbr;
                updatedDetail.Crtd_User = Current.UserName;

                updatedDetail.CS = 0;
                updatedDetail.PC = 0;
                updatedDetail.ProdDate = new DateTime(1900, 1, 1);
            }
            updatedDetail.StkQty = updated.StkQty;
            updatedDetail.ReasonID = updated.ReasonID;
            updatedDetail.PosmID = updated.PosmID;

            updatedDetail.LUpd_DateTime = DateTime.Now;
            updatedDetail.LUpd_Prog = _screenNbr;
            updatedDetail.LUpd_User = Current.UserName;
        }
        private void updateStockOutletPOSM(ref PPC_StockOutletPOSM updatedDetail, IN10700_pgStockOutletDetPOSM_Result updated,PPC_StockOutlet objHeader, bool isNew)
        {
            if (isNew)
            {
                updatedDetail = new PPC_StockOutletPOSM();
                updatedDetail.ResetET();
                updatedDetail.BranchID = objHeader.BranchID;
                updatedDetail.SlsPerID = objHeader.SlsPerID;
                updatedDetail.StkOutNbr = objHeader.StkOutNbr;

                updatedDetail.InvtID = updated.InvtID;
                updatedDetail.PosmID = updated.PosmID;
                updatedDetail.ExpDate = updated.ExpDate.PassMin().Date;

                updatedDetail.Crtd_DateTime = DateTime.Now;
                updatedDetail.Crtd_Prog = _screenNbr;
                updatedDetail.Crtd_User = Current.UserName;

                updatedDetail.CS = 0;
                updatedDetail.PC = 0;
                updatedDetail.ProdDate = new DateTime(1900, 1, 1);
            }
            updatedDetail.StkQty = updated.StkQty;
         
            updatedDetail.LUpd_DateTime = DateTime.Now;
            updatedDetail.LUpd_Prog = _screenNbr;
            updatedDetail.LUpd_User = Current.UserName;
        }
        private void updateStockOutlet(ref PPC_StockOutlet outlet, IN10700_phStockOutlet_Result inputStockOutlet, bool isNew)
        {
            if (isNew) {
                outlet = new PPC_StockOutlet();
                outlet.ResetET();
                outlet.BranchID = inputStockOutlet.BranchID;
                outlet.CustID = inputStockOutlet.CustID;
                outlet.SlsPerID = inputStockOutlet.SlsPerID;
                outlet.StkOutNbr = inputStockOutlet.StkOutNbr;
                outlet.StkOutDate = inputStockOutlet.StkOutDate;
                outlet.StockType = inputStockOutlet.StockType;
                outlet.Crtd_DateTime = DateTime.Now;
                outlet.Crtd_Prog = _screenNbr;
                outlet.Crtd_User = Current.UserName;

                outlet.LUpd_DateTime = DateTime.Now;
                outlet.LUpd_Prog = _screenNbr;
                outlet.LUpd_User = Current.UserName;
            }
            
        }

        [HttpPost]
        public ActionResult DeleteHeader(FormCollection data, bool isPOSM)
        {
            try
            {
                var lstInvtID = _db.IN10700_pdInvtAll(Current.UserName, Current.CpnyID, Current.LangID).Where(x => x.IsPOSM == isPOSM).ToList();
                var lstStockOutletHandler = new StoreDataHandler(data["lstStockOutlet"]);
                var inputStockOutlet = lstStockOutletHandler.ObjectData<IN10700_phStockOutlet_Result>().FirstOrDefault();
                var objHeader = _db.PPC_StockOutlet.Where(p => p.BranchID == inputStockOutlet.BranchID && p.SlsPerID == inputStockOutlet.SlsPerID && p.StkOutNbr == inputStockOutlet.StkOutNbr).FirstOrDefault();
                if (objHeader != null)
                {
                    var lstdel = _db.PPC_StockOutletDet.Where(p => p.BranchID == inputStockOutlet.BranchID && p.SlsPerID == inputStockOutlet.SlsPerID && p.StkOutNbr == inputStockOutlet.StkOutNbr).ToList();
                    var numDelItem = 0;
                    for (int i = 0; i < lstdel.Count; i++)
                    {
                        string delInvtID = lstdel[i].InvtID;
                        var objDel = lstInvtID.FirstOrDefault(x => x.InvtID == delInvtID);
                        if (objDel != null)
                        {
                            _db.PPC_StockOutletDet.DeleteObject(lstdel[i]);
                            numDelItem++;
                        }
                    }
                    if (numDelItem == lstdel.Count)
                    {
                        _db.PPC_StockOutlet.DeleteObject(objHeader);
                    }
                    //while (lstdel.FirstOrDefault() != null)
                    //{

                    //    _db.PPC_StockOutletDet.DeleteObject(lstdel.FirstOrDefault());
                    //    lstdel.Remove(lstdel.FirstOrDefault());
                    //}

                    var lstdelPOSM = _db.PPC_StockOutletPOSM.Where(p => p.BranchID == inputStockOutlet.BranchID && p.SlsPerID == inputStockOutlet.SlsPerID && p.StkOutNbr == inputStockOutlet.StkOutNbr).ToList();
                    while (lstdelPOSM.FirstOrDefault() != null)
                    {

                        _db.PPC_StockOutletPOSM.DeleteObject(lstdelPOSM.FirstOrDefault());
                        lstdelPOSM.Remove(lstdelPOSM.FirstOrDefault());
                    }
                }
                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Delete);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }

        }


        [HttpPost]
        public ActionResult Import()
        {
            var colTexts = HeaderExcel();
            var dataRowIdx = 1;
            FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
            HttpPostedFile file = fileUploadField.PostedFile;
            FileInfo fileInfo = new FileInfo(file.FileName);
            try
            {
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    Worksheet workSheet = workbook.Worksheets[0];
                 
                    string message = string.Empty;

                    string branchNull = string.Empty;
                    string slsperNull = string.Empty;
                    string slsperErr = string.Empty;
                    string custNull = string.Empty;
                    string custErr = string.Empty;
                    string invtTypeNull = string.Empty;
                    string stockTypeNull = string.Empty;
                    string stkDateNull = string.Empty;
                    string stkExpDateNull = string.Empty;
                    string stkExpDateError = string.Empty;
                    string invtNull = string.Empty;
                    string invtErr = string.Empty;
                    string qtyNull = string.Empty;
                    string posmNull = string.Empty;
                    string qtyErr = string.Empty;

                    var regex = new Regex(@"^[0-9]*$");
                    DataRow dtTemp;
                    Dictionary<string, int> dicStkOutNbr = new Dictionary<string, int>();


                    #region -Table temp-
                    System.Data.DataTable dtIN_StockOutletTmp = new System.Data.DataTable() { TableName = "IN_StockOutletTmp" };
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "BranchID" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "SlsperID" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "CustID" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "InvtID" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "ExpDate" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "ProdDate" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "StkQty" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "ReasonID" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "PosmID" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "Unit" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "StockType" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "InvtType" });
                    dtIN_StockOutletTmp.Columns.Add(new DataColumn() { ColumnName = "StkOutNbr" });
                    var lstInvtDate = _db.IN10700_pcInvtDateExcel(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                    bool flagCheck = false;
                    #endregion
                    int index = 0;
                    int dataBlank = 1;
                    for (int i = dataRowIdx; i <= workSheet.Cells.MaxDataRow; i++) //index luôn đi từ 0
                    {
                        #region -Get data from excel-

                        var branchID = workSheet.Cells[i, colTexts.IndexOf("BranchID")].StringValue.Trim();
                        var slsperID = workSheet.Cells[i, colTexts.IndexOf("SlsperID")].StringValue.Trim();
                        var custID = workSheet.Cells[i, colTexts.IndexOf("IN10700CustID")].StringValue.Trim();
                        var invtType = workSheet.Cells[i, colTexts.IndexOf("IN10700InvtType")].StringValue.Trim();
                        var stockType = workSheet.Cells[i, colTexts.IndexOf("IN10700StockType")].StringValue.Trim();
                        var invtID = workSheet.Cells[i, colTexts.IndexOf("InvtID")].StringValue.Trim();
                        var unit = workSheet.Cells[i, colTexts.IndexOf("StkUnit")].StringValue.Trim();
                        if (string.IsNullOrEmpty(branchID) && string.IsNullOrEmpty(slsperID) && string.IsNullOrEmpty(custID) && string.IsNullOrEmpty(invtType) && string.IsNullOrEmpty(stockType) && string.IsNullOrEmpty(invtID))
                        {
                             if (dataBlank == 10)
                            {
                                if (i == dataBlank)
                                {
                                    throw new MessageException(MessageType.Message, "2019040451");
                                }
                                break;
                            }
                             dataBlank ++;
                             continue;
                        }
                 

                        DateTime stkDate;
                      
                        DateTime expDate;
                        Double qty = 0;

                        try
                        {
                            if (workSheet.Cells[i, colTexts.IndexOf("IN10700StkDate")].StringValue.Length > 0)
                            {
                                stkDate = DateTime.ParseExact(workSheet.Cells[i, colTexts.IndexOf("IN10700StkDate")].StringValue, "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture);
                                    //workSheet.Cells[i, colTexts.IndexOf("IN10700OutDate")].DateTimeValue;
                            }
                            else
                            {
                                stkDate = "1990/1/1".ToDateTime();
                                stkDateNull += (i + 1) + ", ";
                                flagCheck = true;
                            }
                           
                        }
                        catch (Exception)
                        {
                            stkDate = "1990/1/1".ToDateTime();
                        }

                        if (workSheet.Cells[i, colTexts.IndexOf("IN10700OutDate")].StringValue.Trim().Length == 0 && GetCodeFromExcel(stockType) != "SP")
                        {
                            stkExpDateNull += (i + 1) + ", ";
                            flagCheck = true;
                        }


                        if (workSheet.Cells[i, colTexts.IndexOf("IN10700OutDate")].StringValue.Trim().Length > 0 && !flagCheck)
                        {
                            if (!lstInvtDate.Any(p => p.InvtID.ToUpper() == invtID.ToUpper() && p.Date == workSheet.Cells[i, colTexts.IndexOf("IN10700OutDate")].StringValue))
                            {
                                stkExpDateError += (i + 1) + ", ";
                                flagCheck = true;
                            }
                            expDate = GetCodeFromExcel(stockType) == "SP" ? "1990/1/1".ToDateTime() : DateTime.ParseExact(workSheet.Cells[i, colTexts.IndexOf("IN10700OutDate")].StringValue, "dd/MM/yyyy",
                                   System.Globalization.CultureInfo.InvariantCulture);

                        }
                        else
                        {
                            expDate = "1990/1/1".ToDateTime();
                        }
                       // stkDate = GetCodeFromExcel(stockType) == "SP" ? "1990/1/1".ToDateTime() : workSheet.Cells[i, colTexts.IndexOf("IN10700StkDate")].DateTimeValue;

                        try
                        {
                            if (!regex.IsMatch(workSheet.Cells[i, colTexts.IndexOf("Qty")].StringValue))
                            {
                                qtyErr += (i + 1) + ", ";
                                flagCheck = true;
                            }
                            else
                            {
                                qty = workSheet.Cells[i, colTexts.IndexOf("Qty")].DoubleValue;
                            }
                        }
                        catch
                        {
                            qty = 0;
                        }
                        var reason = workSheet.Cells[i, colTexts.IndexOf("Reason")].StringValue.Trim();
                        var posm = workSheet.Cells[i, colTexts.IndexOf("IN10700POSM")].StringValue.Trim();
                        #endregion
                        #region validate
                            if (string.IsNullOrEmpty(branchID))
                            {
                                branchNull += (i + 1) + ", ";
                                flagCheck = true;
                            }

                            if (string.IsNullOrEmpty(slsperID))
                            {
                                slsperNull += (i + 1) + ", ";
                                flagCheck = true;
                            }
                            else
                            {
                                var objSlser = _db.IN10700_piSlsperID(Current.UserName,Current.CpnyID,Current.LangID,branchID,slsperID).FirstOrDefault();
                                if (objSlser == null)
                                {
                                    slsperErr += (i + 1) + ", ";
                                    flagCheck = true;
                                }
                            }

                            if (string.IsNullOrEmpty(custID))
                            {
                                custNull += (i + 1) + ", ";
                                flagCheck = true;
                            }
                            else
                            {
                                var objCust = _db.IN10700_piCustID(Current.UserName, Current.CpnyID, Current.LangID, branchID, custID).FirstOrDefault();
                                if (objCust == null)
                                {
                                    custErr += (i + 1) + ", ";
                                    flagCheck = true;
                                }
                            }

                            if (string.IsNullOrEmpty(invtType))
                            {
                                invtTypeNull += (i + 1) + ", ";
                                flagCheck = true;
                            }

                            if (GetCodeFromExcel(invtType).ToUpper() == "POSM" && string.IsNullOrEmpty(posm))
                            {
                                posmNull += (i + 1) + ", ";
                                flagCheck = true;
                            }

                            if (string.IsNullOrEmpty(stockType))
                            {
                                stockTypeNull += (i + 1) + ", ";
                                flagCheck = true;
                            }

                            if (string.IsNullOrEmpty(invtID))
                            {
                                invtNull += (i + 1) + ", ";
                                flagCheck = true;
                            }
                            else
                            {
                                var objInvt = _db.IN10700_piInvtID(Current.UserName, Current.CpnyID, Current.LangID, GetCodeFromExcel(invtType), invtID).FirstOrDefault();
                                if (objInvt == null)
                                {
                                    invtErr += (i + 1) + ", ";
                                    flagCheck = true;
                                }
                            }

                            if (qty == 0)
                            {
                                qtyNull += (i + 1) + ", ";
                                flagCheck = true;
                            }
                        #endregion

                        if (!flagCheck)
                        {

                            var key = branchID + slsperID + custID + invtType + stockType + stkDate + reason;

                            if (!dicStkOutNbr.ContainsKey(key))
                            {
                                dicStkOutNbr.Add(key, index);
                                index++;
                            }
                            dtTemp = dtIN_StockOutletTmp.NewRow();
                            dtTemp["BranchID"] = branchID;
                            dtTemp["CustID"] = custID;
                            dtTemp["SlsperID"] = slsperID;
                            dtTemp["InvtID"] = invtID;
                            dtTemp["ExpDate"] = expDate;
                            dtTemp["ProdDate"] = stkDate;
                            dtTemp["StkQty"] = qty;
                            dtTemp["ReasonID"] = GetCodeFromExcel(reason);
                            dtTemp["PosmID"] = posm; // GetCodeFromExcel(posm);
                            dtTemp["Unit"] = unit;
                            dtTemp["StockType"] = GetCodeFromExcel(stockType);
                            dtTemp["InvtType"] = GetCodeFromExcel(invtType);
                            dtTemp["StkOutNbr"] = custID + DateTime.Now.ToString("yyyyMMddHHmmss") + CountLineRef(dicStkOutNbr[key].ToString());
                            dtIN_StockOutletTmp.Rows.Add(dtTemp);
                        }
                    }

                    message = branchNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("BranchID"), branchNull.TrimEnd(','));
                    message += slsperNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("SlsperID"), slsperNull.TrimEnd(','));
                    message += stkDateNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("IN10700StkDate"), stkDateNull.TrimEnd(','));
                    message += custNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("IN10700CustID"), custNull.TrimEnd(','));
                    message += invtTypeNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("IN10700InvtType"), invtTypeNull.TrimEnd(','));
                    message += stockTypeNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("IN10700StockType"), stockTypeNull.TrimEnd(','));
                    message += stkExpDateError == "" ? "" : string.Format(Message.GetString("2019040851", null), stkExpDateError.TrimEnd(','), Util.GetLang("IN10700OutDate"));
                    message += invtNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("InvtID"), invtNull.TrimEnd(','));
                    message += qtyNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("Qty"), qtyNull.TrimEnd(','));
                    message += posmNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("IN10700POSM"), posmNull.TrimEnd(','));
                    message += qtyErr == "" ? "" : string.Format(Message.GetString("2019040551", null), qtyErr.TrimEnd(','));
                    message += slsperErr == "" ? "" : string.Format(Message.GetString("2019040851", null), slsperErr.TrimEnd(',') , Util.GetLang("SlsperID"));
                    message += custErr == "" ? "" : string.Format(Message.GetString("2019040851", null), custErr.TrimEnd(','), Util.GetLang("IN10700CustID"));
                    message += invtErr == "" ? "" : string.Format(Message.GetString("2019040851", null), invtErr.TrimEnd(','), Util.GetLang("InvtID"));
                    message += stkExpDateNull == "" ? "" : string.Format(Message.GetString("2019022560", null), Util.GetLang("IN10700OutDate"), stkExpDateNull.TrimEnd(','));


                    if (string.IsNullOrEmpty(message))
                    {
                        try
                        {
                            // Save data
                            var dal = Util.Dal();
                            using (SqlConnection dbConnection = new SqlConnection(dal.ConnectionString))
                            {
                                dbConnection.Open();
                                using (SqlTransaction sqlTran = dbConnection.BeginTransaction())
                                {
                                    using (SqlBulkCopy s = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity, sqlTran))
                                    {
                                        try
                                        {
                                            // Insert vào bảng tạm OM_FCSHeader
                                            s.DestinationTableName = dtIN_StockOutletTmp.TableName;
                                            foreach (var col in dtIN_StockOutletTmp.Columns)
                                            {
                                                s.ColumnMappings.Add(col.ToString(), col.ToString());
                                            }
                                            s.WriteToServer(dtIN_StockOutletTmp);
                                            ////Gọi store insert, update từ bảng tạm vào bảng chính
                                            SqlCommand cmd1 = new SqlCommand("IN10700_ppServerImport", dbConnection, sqlTran);
                                            cmd1.CommandType = CommandType.StoredProcedure;

                                            cmd1.Parameters.AddWithValue("@UserName", Current.UserName);
                                            cmd1.Parameters.AddWithValue("@CpnyID", Current.CpnyID);
                                            cmd1.Parameters.AddWithValue("@LangID", Current.LangID);

                                            cmd1.ExecuteScalar();
                                            sqlTran.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            sqlTran.Rollback();
                                            throw ex;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }

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
            Worksheet sheet = workbook.Worksheets[0];
            Worksheet MasterData = workbook.Worksheets[1];

            DataAccess dal = Util.Dal();
            sheet.Name = Util.GetLang("IN10700NameSheet");
            MasterData.Name = "MasterData";
            #region header info
            // Header text columns
            sheet.AutoFitColumns();

            for (int i = 0; i < ColTexts.Count; i++)
            {
                SetCellValue(sheet.Cells[0, i], Util.GetLang(ColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, false, false);
                sheet.Cells.SetColumnWidth(i, 25);
            }

            #endregion
            var allColumns = new List<string>();
            allColumns.AddRange(ColTexts);


            sheet.Cells.SetRowHeight(0, 30);

            var style = workbook.GetStyleInPool(0);

           
            StyleFlag flag = new StyleFlag();
            Range range;
            #region GetDataToComboExcel



            //Get data cho combo 
            ParamCollection pc = new ParamCollection();
            pc = new ParamCollection();
            pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
            pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

            DataTable dtBranch = dal.ExecDataTable("IN10700_pcBranchExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtBranch, true, 0, 0, false);


            DataTable dtSlsper = dal.ExecDataTable("IN10700_pcSlsperExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtSlsper, true, 0, 5, false);

            DataTable dtCust = dal.ExecDataTable("IN10700_pcCustExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtCust, true, 0, 10, false);

            DataTable dtInvtType = dal.ExecDataTable("IN10700_pcInvtTypeExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtInvtType, true, 0, 15, false);

            DataTable dtStockType = dal.ExecDataTable("IN10700_pcStockTypeExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtStockType, true, 0, 20, false);


            DataTable dtInvt = dal.ExecDataTable("IN10700_pcInvtExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtInvt, true, 0, 25, false);

            DataTable dtReason = dal.ExecDataTable("IN10700_pcReasonExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtReason, true, 0, 30, false);

            DataTable dtPosm = dal.ExecDataTable("IN10700_pcPosmExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtPosm, true, 0, 35, false);

            DataTable dtDate = dal.ExecDataTable("IN10700_pcInvtDateExcel", CommandType.StoredProcedure, ref pc);
            MasterData.Cells.ImportDataTable(dtDate,true, 0, 40, false);

            #endregion

            #region formular

            string formulaBranch = "=MasterData!$A$2:$A$" + (dtBranch.Rows.Count + 2);
            Validation validation = GetValidation(ref sheet, formulaBranch, "Chọn NPP", "Mã NPP này không tồn tại");
            validation.AddArea(GetCellArea(1, dtBranch.Rows.Count + 100, ColTexts.IndexOf("BranchID")));

            string formulaReason = "=MasterData!$AE$2:$AE$" + (dtReason.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaReason, "Chọn NPP", "Mã NPP này không tồn tại");
            validation.AddArea(GetCellArea(1, dtReason.Rows.Count + 100, ColTexts.IndexOf("Reason")));


            string formulaBranchName = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$A:$B,2,0)),\"\",VLOOKUP({0},MasterData!$A:$B,2,0))", "A2");
            sheet.Cells["B2"].SetSharedFormula(formulaBranchName, 1000, 1);

            string formulaSlsperID = string.Format("=OFFSET(MasterData!$F$1,IFERROR(MATCH(A{0},MasterData!$H$2:$H${1},0),{2}),0, IF(COUNTIF(MasterData!$H$2:$H${1},A{0})=0,1,COUNTIF(MasterData!$H$2:$H${1},A{0})),1)",
                   new string[] { "2", (dtSlsper.Rows.Count + 100).ToString(), (dtSlsper.Rows.Count + 64).ToString() });
            //string formulaSlsperID = "=MasterData!$F$2:$F$" + (dtSlsper.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaSlsperID, "Chọn Nhân Viên", "Mã Nhân Viên này không tồn tại");
            validation.AddArea(GetCellArea(1, dtSlsper.Rows.Count + 100, ColTexts.IndexOf("SlsperID")));

            string formulaSlsperName = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$F:$G,2,0)),\"\",VLOOKUP({0},MasterData!$F:$G,2,0))", "C2");
            sheet.Cells["D2"].SetSharedFormula(formulaSlsperName, 1000, 1);

            string formulaCustID = string.Format("=OFFSET(MasterData!$K$1,IFERROR(MATCH(A{0},MasterData!$M$2:$M${1},0),{2}),0, IF(COUNTIF(MasterData!$M$2:$M${1},A{0})=0,1,COUNTIF(MasterData!$M$2:$M${1},A{0})),1)",
                   new string[] { "2", (dtCust.Rows.Count + 100).ToString(), (dtCust.Rows.Count + 64).ToString() });
            //string formulaCustID = "=MasterData!$K$2:$K$" + (dtCust.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaCustID, "Chọn Shop", "Mã Shop này không tồn tại");
            validation.AddArea(GetCellArea(1, dtCust.Rows.Count + 100, ColTexts.IndexOf("IN10700CustID")));

            string formulaCustName = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$K:$L,2,0)),\"\",VLOOKUP({0},MasterData!$K:$L,2,0))", "E2");
            sheet.Cells["F2"].SetSharedFormula(formulaCustName, 1000, 1);

            string formulaInvtType = "=MasterData!$P$2:$P$" + (dtInvtType.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaInvtType, "Chọn Loại Sản Phẩm", "Mã Loại Sản Phẩm này không tồn tại");
            validation.AddArea(GetCellArea(1, dtInvtType.Rows.Count + 100, ColTexts.IndexOf("IN10700InvtType")));

            string formulaStockType = "=MasterData!$U$2:$U$" + (dtStockType.Rows.Count + 2);
            validation = GetValidation(ref sheet, formulaStockType, "Chọn Loại Tồn Kho", "Mã Loại Tồn Kho này không tồn tại");
            validation.AddArea(GetCellArea(1, dtStockType.Rows.Count + 100, ColTexts.IndexOf("IN10700StockType")));

            string formulaInvt = string.Format("=OFFSET(MasterData!$AA$1,IFERROR(MATCH(G{0},MasterData!$Z$2:$Z${1},0),{2}),0, IF(COUNTIF(MasterData!$Z$2:$Z${1},G{0})=0,1,COUNTIF(MasterData!$Z$2:$Z${1},G{0})),1)",
                   new string[] { "2", (dtInvt.Rows.Count + 100).ToString(), (dtInvt.Rows.Count + 64).ToString() });
            validation = GetValidation(ref sheet, formulaInvt, "Chọn Sản Phẩm", "Mã Sản này không tồn tại");
            validation.AddArea(GetCellArea(1, dtInvt.Rows.Count + 100, ColTexts.IndexOf("InvtID")));

            string formulaInvtDate = string.Format("=OFFSET(MasterData!$AP$1,IFERROR(MATCH(J{0},MasterData!$AO$2:$AO${1},0),{2}),0, IF(COUNTIF(MasterData!$AO$2:$AO${1},J{0})=0,1,COUNTIF(MasterData!$AO$2:$AO${1},J{0})),1)",
               new string[] { "2", (dtDate.Rows.Count + 100).ToString(), (dtDate.Rows.Count + 64).ToString() });
            validation = GetValidation(ref sheet, formulaInvtDate, "Chọn HSD", "Ngày HSD này không tồn tại");
            validation.AddArea(GetCellArea(1, dtDate.Rows.Count + 100, ColTexts.IndexOf("IN10700OutDate")));


            string formulaInvtName = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AC,2,0)),\"\",VLOOKUP({0},MasterData!$AA:$AC,2,0))", "J2");
            sheet.Cells["K2"].SetSharedFormula(formulaInvtName, 1000, 1);
            
            string formulaUnit = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AC,3,0)),\"\",VLOOKUP({0},MasterData!$AA:$AC,3,0))", "J2");
            sheet.Cells["L2"].SetSharedFormula(formulaUnit, 1000, 1);


            string formulaPosm = string.Format("=OFFSET(MasterData!$AK$1,IFERROR(MATCH(G{0},MasterData!$AJ$2:$AJ${1},0),{2}),0, IF(COUNTIF(MasterData!$AJ$2:$AJ${1},G{0})=0,1,COUNTIF(MasterData!$AJ$2:$AJ${1},G{0})),1)",
                   new string[] { "2", (dtPosm.Rows.Count + 100).ToString(), (dtPosm.Rows.Count + 64).ToString() });
            validation = GetValidation(ref sheet, formulaPosm, "Chọn POSM", "Mã POSM này không tồn tại");
            validation.AddArea(GetCellArea(1, dtPosm.Rows.Count + 100, ColTexts.IndexOf("IN10700POSM")));
            #endregion

            #region format cell

            var strFirstRow = 2.ToString();
            style = sheet.Cells["A" + strFirstRow].GetStyle();
            //style.IsLocked = false;


            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("BranchID")) + 2, Getcell(allColumns.IndexOf("BranchID")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("SlsperID")) + 2, Getcell(allColumns.IndexOf("SlsperID")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("IN10700CustID")) + 2, Getcell(allColumns.IndexOf("IN10700CustID")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("IN10700InvtType")) + 2, Getcell(allColumns.IndexOf("IN10700InvtType")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("InvtID")) + 2, Getcell(allColumns.IndexOf("InvtID")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("IN10700StockType")) + 2, Getcell(allColumns.IndexOf("IN10700StockType")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("Qty")) + 2, Getcell(allColumns.IndexOf("Qty")) + 1000);

            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("IN10700POSM")) + 2, Getcell(allColumns.IndexOf("IN10700POSM")) + 1000);
            range.SetStyle(style);


            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("Reason")) + 2, Getcell(allColumns.IndexOf("Reason")) + 1000);
            range.SetStyle(style);

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("IN10700OutDate")) + 2, Getcell(allColumns.IndexOf("IN10700OutDate")) + 1000);
            range.SetStyle(style);
            //style.Number = 49;

            style.Number = 14;

            range = sheet.Cells.CreateRange(Getcell(allColumns.IndexOf("IN10700StkDate")) + 2, Getcell(allColumns.IndexOf("IN10700StkDate")) + 1000);
            range.SetStyle(style);

           

            #endregion



            //sheet.Protect(ProtectionType.All);
            MasterData.Protect(ProtectionType.All);
            MasterData.VisibilityType = VisibilityType.Hidden;


         
            workbook.Save(stream, SaveFormat.Xlsx);
            stream.Flush();
            stream.Position = 0;

            return new FileStreamResult(stream, "application/vnd.ms-excel")
            {
                FileDownloadName = string.Format("{0}.xlsx", Util.GetLang("IN10700_excel"))
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
            return new List<string>()
            {
                "BranchID",
                "BranchName",
                "SlsperID",
                "Name",
                "IN10700CustID",
                "IN10700CustName",
                "IN10700InvtType",
                "IN10700StockType",
                "IN10700StkDate",
                "InvtID",
                "FreeItemDescr",
                "StkUnit",
                "IN10700OutDate",
                "Qty",
                "Reason",
                "IN10700POSM"
            };
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
            int index = codeDescr.IndexOf("-");
            if (index > 0)
            {
                return codeDescr.Substring(0, index).Trim();
            }
            return codeDescr.Trim();
        }


        private string GetCodeAddressTypeFromExcel(string codeDescr)
        {
            switch (codeDescr)
            {
                case "CURRES":
                    return "A";
                case "PERMNENT":
                    return "B";
                case "HEADOFF":
                    return "C";
                case "BCHOFF":
                    return "D";
                case "OTHER":
                    return "F";
                default:
                    return "";
            }

        }



        private string CountLineRef(string liRef)
        {
            if (string.IsNullOrEmpty(liRef)) return "00000";
            int number = liRef.ToInt();
            number++;
            var lineRef = number.ToString();
            for (int i = 0; i < 6 - lineRef.Length; i++)
            {
                lineRef = "0" + lineRef;
            }
            return lineRef;
        }
    }
}

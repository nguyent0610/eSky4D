using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using HQ.eSkyFramework;
using System.IO;
using Aspose.Cells;
using HQFramework.DAL;
using System.Drawing;
using System.Data;
using HQFramework.Common;
using HQ.eSkySys;

namespace OM30400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM30400Controller : Controller
    {
        private string _screenName = "OM30400";
        OM30400Entities _db = Util.CreateObjectContext<OM30400Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        //
        // GET: /OM30400/
        public ActionResult Index()
        {
            var hideButtonCurrentPosition = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "OM30400btnPosition");
            if (hideButtonCurrentPosition == null)
                ViewBag.hideButtonPosition = "false";
            else
            {
                if (hideButtonCurrentPosition.IntVal == 1)
                    ViewBag.hideButtonPosition = "true";
                else
                    ViewBag.hideButtonPosition = "false";
            }
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult LoadVisitCustomerPlan(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var planVisit = _db.OM30400_pgVisitCustomerPlan(Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(planVisit);
        }

        public ActionResult LoadMCL(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var mcl = _db.OM30400_pgLoadMCL(Current.CpnyID, Current.UserName, territory,
                province, distributor, channel, shopType,
                slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(mcl);
        }

        public ActionResult LoadGridActualVisit(string distributor, string slsperId, DateTime visitDate, bool realTime)
        {
            var actualVisit = _db.OM30400_pgGridActualVisit(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate, realTime).ToList();

            var planVisit = _db.OM30400_pdVisitPlan(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate).ToList();
            for (int i = 0; i < planVisit.Count; i++)
            {
                actualVisit.Add(new OM30400_pgGridActualVisit_Result()
                {
                    CustId = planVisit[i].CustId,
                    CustName = planVisit[i].CustName,
                    Addr = planVisit[i].Addr,
                    VisitDate = DateTime.Now,
                    Checkin = new TimeSpan(0, 0, 0),
                    Checkout = new TimeSpan(0, 0, 0),
                    TGCICO = "",
                    Amt = 0,
                    GPSCheckin = "",
                    GPSCheckout = "",
                    CiLat = planVisit[i].Lat,
                    CiLng = planVisit[i].Lng,
                    CoLat = 0,
                    CoLng = 0,
                    CustLat = planVisit[i].Lat,
                    CustLng = planVisit[i].Lng,
                    PicPath = "",
                    Color = "3B5998",
                    IsNotVisited = 0,
                    TurnOver = 0,
                    Distance = 0,
                    TypeMapPlan = "1"
                });
            }

            double lat1 = 0;
            double lng1 = 0;
            double lat2 = 0;
            double lng2 = 0;
            string colorTmp = "";
            for (int i = 0; i < actualVisit.Count; i++)
            { 
                if (i > 0)
                {
                    lat1 = (double)actualVisit[i - 1].CiLat;
                    lng1 = (double)actualVisit[i - 1].CiLng;
                    lat2 = (double) actualVisit[i].CiLat;
                    lng2 = (double) actualVisit[i].CiLng;
                }
                if (actualVisit[i].Color != colorTmp)
                {
                    lat1 = 0;
                    lng1 = 0;
                    lat2 = 0;
                    lng2 = 0;
                }
                actualVisit[i].Distance = DistanceBetweenPlaces(lng1, lat1, lng2, lat2);
                colorTmp = actualVisit[i].Color;
            }

            
            return this.Store(actualVisit);
        }

        public ActionResult LoadGridPlanVisit(string distributor, string slsperId, DateTime visitDate)
        {
            var planVisit = _db.OM30400_pdVisitPlan(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate).ToList();
            return this.Store(planVisit);
        }

        public ActionResult LoadMapActualVisit(string distributor, string slsperId, DateTime visitDate, bool realTime)
        {
            var actualVisit = _db.OM30400_pgGridActualVisit(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate, true).ToList();

            return this.Store(actualVisit);
        }

        public ActionResult LoadVisitCustomerActual(string distributor, string slsperId, DateTime visitDate, bool realTime)
        {
            var actualVisit = _db.OM30400_pgVisitCustomerActual(Current.CpnyID, Current.UserName, distributor, slsperId, visitDate, realTime).ToList();
            return this.Store(actualVisit);
        }

        public ActionResult LoadAllCurrentSalesman(string distributor, DateTime visitDate)
        {
            var allSlspers = _db.OM30400_pgAllCurrentSalesman(Current.CpnyID, Current.UserName, distributor, visitDate).ToList();
            return this.Store(allSlspers);
        }

        public ActionResult LoadCustHistory(string distributor, string slsperId, string customer, DateTime startDate, DateTime endDate)
        {
            var cusHistory = _db.OM30400_pgHistory(Current.CpnyID, Current.UserName, distributor, slsperId, customer, startDate, endDate).ToList();
            return this.Store(cusHistory);
        }

        public ActionResult LoadSalesRouteMaster(string branchID, string custID, string slsPerID)
        {
            var slsRouteMster = _db.OM_SalesRouteMaster.FirstOrDefault(
                                    x => x.BranchID == branchID
                                    && x.CustID == custID
                                    && x.SlsPerID == slsPerID
                                    && x.SalesRouteID == branchID
                                    && x.PJPID == branchID);
            return this.Store(slsRouteMster);
        }

        public ActionResult SaveMcp(FormCollection data, bool custActive)
        {
            try
            {
                var custID = data["txtCustIDMcpInfo"];
                var slsperID = data["hdnSlsperIDMcpInfo"];
                var branchID = data["hdnBranchIDMcpInfo"];

                if (!string.IsNullOrWhiteSpace(custID)
                    && !string.IsNullOrWhiteSpace(slsperID)
                    && !string.IsNullOrWhiteSpace(branchID))
                {
                    var cust = _db.AR_Customer.FirstOrDefault(c => c.CustId == custID && c.BranchID == branchID);
                    if (cust != null)
                    {
                        var custActived = cust.Status.ToUpper() == "A" ? true : false;
                        if (custActived != custActive)
                        {
                            cust.Status = custActive ? "A" : "I";
                            cust.LUpd_Datetime = DateTime.Now;
                            cust.LUpd_Prog = _screenName;
                            cust.LUpd_User = Current.UserName;

                            _db.SaveChanges();
                        }

                        if (custActive)
                        {
                            #region MCP
                            var dataHandler = new StoreDataHandler(data["lstMcpInfo"]);
                            var lstMcpInfo = dataHandler.BatchObjectData<OM_SalesRouteMaster>();

                            foreach (var deleted in lstMcpInfo.Deleted)
                            {
                                var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == branchID
                                    && x.SalesRouteID == branchID
                                    && x.BranchID == branchID
                                    && x.SlsPerID == slsperID
                                    && x.CustID == custID);

                                if (obj != null)
                                {
                                    // xoa cu
                                    _db.OM_SalesRouteMaster.DeleteObject(obj);
                                    _db.SaveChanges();

                                    return Json(new
                                    {
                                        success = true,
                                        CustID = custID,
                                        SlsPerID = slsperID,
                                        BranchID = branchID,
                                        Color = "CCFF33",
                                        SlsFreq = "",
                                        WeekofVisit = "",
                                        VisitSort = "false",
                                        Sun = 0,
                                        Mon = 0,
                                        Tue = 0,
                                        Wed = 0,
                                        Thu = 0,
                                        Fri = 0,
                                        Sat = 0,
                                        Status = cust.Status
                                    });
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "8");
                                }
                            }

                            foreach (var updated in lstMcpInfo.Updated)
                            {
                                var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == branchID
                                    && x.SalesRouteID == branchID
                                    && x.BranchID == branchID
                                    && x.SlsPerID == slsperID
                                    && x.CustID == custID);

                                if (obj != null)
                                {
                                    if (obj.tstamp.ToHex() == updated.tstamp.ToHex())
                                    {
                                        // xoa cu, insert moi
                                        var newObj = new OM_SalesRouteMaster()
                                        {
                                            PJPID = obj.PJPID,
                                            SalesRouteID = obj.SalesRouteID,
                                            CustID = obj.CustID,
                                            SlsPerID = obj.SlsPerID,
                                            BranchID = obj.BranchID
                                        };

                                        _db.OM_SalesRouteMaster.DeleteObject(obj);
                                        _db.SaveChanges();

                                        updateSaleRoutesMaster(ref newObj, updated);
                                        _db.OM_SalesRouteMaster.AddObject(newObj);
                                        _db.SaveChanges();

                                        return Json(new
                                        {
                                            success = true,
                                            CustID = custID,
                                            SlsPerID = slsperID,
                                            BranchID = branchID,
                                            Color = "FF0000",
                                            SlsFreq = newObj.SlsFreq,
                                            WeekofVisit = newObj.WeekofVisit,
                                            VisitSort = newObj.VisitSort,
                                            Sun = newObj.Sun ? 1 : 0,
                                            Mon = newObj.Mon ? 1 : 0,
                                            Tue = newObj.Tue ? 1 : 0,
                                            Wed = newObj.Wed ? 1 : 0,
                                            Thu = newObj.Thu ? 1 : 0,
                                            Fri = newObj.Fri ? 1 : 0,
                                            Sat = newObj.Sat ? 1 : 0,
                                            Status = cust.Status
                                        });
                                    }
                                    else
                                    {
                                        throw new MessageException(MessageType.Message, "19");
                                    }
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "8");
                                }
                            }

                            foreach (var created in lstMcpInfo.Created)
                            {
                                var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.PJPID == branchID
                                    && x.SalesRouteID == branchID
                                    && x.BranchID == branchID
                                    && x.SlsPerID == slsperID
                                    && x.CustID == custID);

                                if (obj == null)
                                {
                                    // insert moi
                                    var newObj = new OM_SalesRouteMaster()
                                    {
                                        PJPID = branchID,
                                        SalesRouteID = branchID,
                                        CustID = custID,
                                        SlsPerID = slsperID,
                                        BranchID = branchID
                                    };

                                    updateSaleRoutesMaster(ref newObj, created);
                                    _db.OM_SalesRouteMaster.AddObject(newObj);
                                    _db.SaveChanges();

                                    return Json(new
                                    {
                                        success = true,
                                        CustID = custID,
                                        SlsPerID = slsperID,
                                        BranchID = branchID,
                                        Color = "FF0000",
                                        SlsFreq = newObj.SlsFreq,
                                        WeekofVisit = newObj.WeekofVisit,
                                        VisitSort = newObj.VisitSort,
                                        Sun = newObj.Sun ? 1 : 0,
                                        Mon = newObj.Mon ? 1 : 0,
                                        Tue = newObj.Tue ? 1 : 0,
                                        Wed = newObj.Wed ? 1 : 0,
                                        Thu = newObj.Thu ? 1 : 0,
                                        Fri = newObj.Fri ? 1 : 0,
                                        Sat = newObj.Sat ? 1 : 0,
                                        Status = cust.Status
                                    });
                                }
                                else
                                {
                                    //return Json(new { success = false, msgCode = 2000, msgParam = Util.GetLang("MCP") });
                                    throw new MessageException(MessageType.Message, "2000", "", new string[] { Util.GetLang("MCP") });
                                }
                            }
                            #endregion
                        }

                        return Json(new
                        {
                            success = true,
                            CustID = custID,
                            SlsPerID = slsperID,
                            BranchID = branchID,
                            Status = cust.Status,
                            Color = (cust.Status == "I" ? "000000" : "undefined")
                        });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "8", "", new string[] { Util.GetLang("MCP") });
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "22701");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
        }

        [DirectMethod]
        public ActionResult ExportCustomer(string custIDs)
        {
            try
            {
                string[] paraStringsD = JSON.Deserialize<string[]>(custIDs);

                if (custIDs.Length > 0)
                {
                    var headerRowIdx = 3;

                    Stream stream = new MemoryStream();
                    Workbook workbook = new Workbook();
                    Worksheet SheetMCP = workbook.Worksheets[0];
                    SheetMCP.Name = Util.GetLang("MCL");
                    DataAccess dal = Util.Dal();
                    Style style = workbook.GetStyleInPool(0);
                    StyleFlag flag = new StyleFlag();
                    var beforeColTexts = new string[] { "N0", "BranchID", "CpnyName", "SlsperID", "SlsName", "CustID", "CustName", "Address", "WeekofVisit", "DayOfWeek" };

                    #region header info
                    // Title header
                    SetCellValue(SheetMCP.Cells["A2"],
                        string.Format("{0} ({1})", Util.GetLang("OM30400EHeader"), Util.GetLang("DrawingArea")),
                        TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                    SheetMCP.Cells.Merge(1, 0, 1, beforeColTexts.Length);

                    // Header text columns
                    
                    for (int i = 0; i < beforeColTexts.Length; i++)
                    {
                        var cell = SheetMCP.Cells[3, i];
                        SetCellValue(cell, Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                        //SheetMCP.Cells.Merge(headerRowIdx, colIdx, 1, 1);

                        //Apply the border styles to the cell
                        setCellStyle(cell);
                    }
                    var allColumns = new List<string>();
                    allColumns.AddRange(beforeColTexts);

                    #endregion

                    #region export data
                    ParamCollection pc = new ParamCollection();
                    pc.Add(new ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(string.Join(",", paraStringsD)), ParameterDirection.Input, int.MaxValue));
                    pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));

                    DataTable dtDataExport = dal.ExecDataTable("OM30400_peExportData", CommandType.StoredProcedure, ref pc);
                    //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

                    for (int i = 0; i < dtDataExport.Rows.Count; i++)
                    {
                        for (int j = 0; j < allColumns.Count; j++)
                        {
                            var cell = SheetMCP.Cells[4 + i, j];
                            if (allColumns[j] == "N0")
                            {
                                cell.PutValue(i + 1);
                            }
                            else if (dtDataExport.Columns.Contains(allColumns[j]))
                            {
                                cell.PutValue(dtDataExport.Rows[i][allColumns[j]]);
                            }

                            //Apply the border styles to the cell
                            setCellStyle(cell);
                        }
                    }
                    #endregion
                    SheetMCP.AutoFitColumns();

                    //SheetPOSuggest.Protect(ProtectionType.Objects);
                    workbook.Save(stream, SaveFormat.Xlsx);
                    stream.Flush();
                    stream.Position = 0;

                    //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM30400") + ".xlsx" };
                    return File(stream, "application/vnd.ms-excel", Util.GetLang("OM30400") + ".xlsx");
                }
                else {
                    throw new MessageException(MessageType.Message, "14091901");
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

        [DirectMethod]
        public ActionResult ExportExcelActual(string distributor, string slsperId, string visitDate, bool realTime)
        {
            try
            {
                var headerRowIdx = 3;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetMCP = workbook.Worksheets[0];
                SheetMCP.Name = Util.GetLang("MCL");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                var beforeColTexts = new string[] { "N0", "CustId", "CustName", "Checkin", "Checkout", "TGCICO", "Amt", "GPSCheckin", "GPSCheckout", "Distance" };

                #region header info
                // Title header
                SetCellValue(SheetMCP.Cells["A2"],
                    string.Format("{0} ({1})", Util.GetLang("OM30400EHeader"), Util.GetLang("AVC")),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetMCP.Cells.Merge(1, 0, 1, beforeColTexts.Length);

                // Header text columns
                for (int i = 0; i < beforeColTexts.Length; i++)
                {
                    var cell = SheetMCP.Cells[3, i];
                    SetCellValue(cell, Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    //SheetMCP.Cells.Merge(headerRowIdx, colIdx, 1, 1);

                    //Apply the border styles to the cell
                    setCellStyle(cell);
                }
                var allColumns = new List<string>();
                allColumns.AddRange(beforeColTexts);

                #endregion

                #region export data
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@Distributor", DbType.String, clsCommon.GetValueDBNull(distributor), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@SlsperId", DbType.String, clsCommon.GetValueDBNull(slsperId), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@VisitDate", DbType.DateTime, clsCommon.GetValueDBNull(DateTime.Parse(visitDate)), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@RealTime", DbType.Boolean, clsCommon.GetValueDBNull(realTime), ParameterDirection.Input, int.MaxValue));
                

                DataTable dtDataExport = dal.ExecDataTable("OM30400_pgGridActualVisit", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@Distributor", DbType.String, clsCommon.GetValueDBNull(distributor), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@SlsperId", DbType.String, clsCommon.GetValueDBNull(slsperId), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@VisitDate", DbType.DateTime, clsCommon.GetValueDBNull(DateTime.Parse(visitDate)), ParameterDirection.Input, int.MaxValue));
              
                DataTable dtDataExportMapPlan = dal.ExecDataTable("OM30400_pdVisitPlan", CommandType.StoredProcedure, ref pc);

                for (int i = 0; i < dtDataExportMapPlan.Rows.Count; i++)
                {
                    DataRow row = dtDataExport.NewRow();
                    row["CustId"] = dtDataExportMapPlan.Rows[i]["CustId"].PassNull();
                    row["CustName"] = dtDataExportMapPlan.Rows[i]["CustName"].PassNull();
                    row["Addr"] = dtDataExportMapPlan.Rows[i]["Addr"].PassNull();
                    row["VisitDate"] = DateTime.Now.ToString();
                    row["Checkin"] = new TimeSpan(0, 0, 0).ToString();
                    row["Checkout"] = new TimeSpan(0, 0, 0).ToString();
                    row["TGCICO"] = "0";
                    row["Amt"] = "0";
                    row["GPSCheckin"] = "";
                    row["GPSCheckout"] = "";
                    row["CiLat"] = dtDataExportMapPlan.Rows[i]["Lat"].PassNull();
                    row["CiLng"] = dtDataExportMapPlan.Rows[i]["Lng"].PassNull();
                    row["CoLat"] = "0";
                    row["CoLng"] = "0";
                    row["CustLat"] = dtDataExportMapPlan.Rows[i]["Lat"].PassNull();
                    row["CustLng"] = dtDataExportMapPlan.Rows[i]["Lng"].PassNull();
                    row["PicPath"] = "";
                    row["Color"] = "3B5998";
                    row["IsNotVisited"] = "0";
                    row["TurnOver"] = "0";
                    row["Distance"] = "0";
                    row["TypeMapPlan"] = "1";

                    dtDataExport.Rows.Add(row);
                }


                double lat1 = 0;
                double lng1 = 0;
                double lat2 = 0;
                double lng2 = 0;
                string colorTmp = "";
                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    if (i > 0)
                    {
                        lat1 = double.Parse(dtDataExport.Rows[i - 1]["CiLat"].ToString());
                        lng1 = double.Parse(dtDataExport.Rows[i - 1]["CiLng"].ToString());
                        lat2 = double.Parse(dtDataExport.Rows[i]["CiLat"].ToString());
                        lng2 = double.Parse(dtDataExport.Rows[i]["CiLng"].ToString());
                    }
                    if (colorTmp != dtDataExport.Rows[i]["Color"].ToString())
                    {
                        lat1 = 0;
                        lng1 = 0;
                        lat2 = 0;
                        lng2 = 0;
                    }
                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        var cell = SheetMCP.Cells[4 + i, j];
                        if (allColumns[j] == "N0")
                        {
                            cell.PutValue(i + 1);
                        }
                        else if (allColumns[j] == "Distance")
                        {
                            cell.PutValue(DistanceBetweenPlaces(lng1,lat1,lng2,lat2));
                        }
                        else if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            cell.PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }

                        //Apply the border styles to the cell
                        setCellStyle(cell);
                    }
                    colorTmp = dtDataExport.Rows[i]["Color"].ToString();

                }
                #endregion
                SheetMCP.AutoFitColumns();

                //SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM30400") + ".xlsx" };
                return File(stream, "application/vnd.ms-excel", string.Format("{0}_{1}.xlsx", Util.GetLang("OM30400"), Util.GetLang("AVC")));
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

        public ActionResult Download(string filePath, string fileName)
        {
            var dlFileName = string.Format("{0}.xlsx", fileName);

            return File(filePath, "application/xls", dlFileName);
        }

        private void setCellStyle(Cell cell)
        {
            //Create a style object
            Style style = cell.GetStyle();

            //Setting the line style of the top border
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the top border
            style.Borders[BorderType.TopBorder].Color = Color.Black;

            //Setting the line style of the bottom border
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the bottom border
            style.Borders[BorderType.BottomBorder].Color = Color.Black;

            //Setting the line style of the left border
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the left border
            style.Borders[BorderType.LeftBorder].Color = Color.Black;

            //Setting the line style of the right border
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;

            //Setting the color of the right border
            style.Borders[BorderType.RightBorder].Color = Color.Black;

            cell.SetStyle(style);
        }

        private void updateSaleRoutesMaster(ref OM_SalesRouteMaster updated, OM_SalesRouteMaster inputted)
        {
            updated.SlsFreq = inputted.SlsFreq;
            updated.SlsFreqType = "R";
            updated.VisitSort = inputted.VisitSort;
            updated.WeekofVisit = inputted.WeekofVisit;
            updated.Mon = inputted.Mon;
            updated.Tue = inputted.Tue;
            updated.Wed = inputted.Wed;
            updated.Thu = inputted.Thu;
            updated.Fri = inputted.Fri;
            updated.Sat = inputted.Sat;
            updated.Sun = inputted.Sun;

            updated.Crtd_DateTime = DateTime.Now;
            updated.Crtd_Prog = _screenName;
            updated.Crtd_User = Current.UserName;

            updated.LUpd_DateTime = DateTime.Now;
            updated.LUpd_Prog = _screenName;
            updated.LUpd_User = Current.UserName;

            updated.StartDate = DateTime.Now.Date;
            updated.EndDate = new DateTime(DateTime.Now.Year, 12, 31);
        }

        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle = false)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
                style.Font.Color = Color.Red;
            c.SetStyle(style);
        }

        const double PIx = 3.141592653589793;
        const double RADIUS = 6378.16;

        public double Radians(double x)
        {
            return x * PIx / 180;
        }

        public double DistanceBetweenPlaces(double lon1, double lat1, double lon2, double lat2)
        {
            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Math.Round((angle * RADIUS) * 1000, 0);
        }
    }
}

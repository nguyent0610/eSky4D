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
using System.Configuration;
using HQFramework.Common;
using System.Globalization;


namespace OM24100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM24100Controller : Controller
    {
        private string _screenName = "OM24100";
        OM24100Entities _db = Util.CreateObjectContext<OM24100Entities>(false);

        //
        // GET: /OM24100/
        public ActionResult Index(string data)
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            if (data != null)//dung cho PVN lay du lieu tu silverlight goi len
            {
                // user;company;langid => ?data=admin;100024;1
                try
                {
                    data = data.Replace(" ", "+");
                    data = Encryption.Encryption.Decrypt(data, DateTime.Now.ToString("yyyyMMdd"));
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();

                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["UserName"] = Current.UserName = data.Split(';')[0];
                    Session["CpnyID"] = Current.CpnyID = data.Split(';')[1];
                    Session["Language"] = Current.Language = short.Parse(data.Split(';')[2]) == 0 ? "en" : "vi";
                    Session["LangID"] = short.Parse(data.Split(';')[2]);
                    Util.InitRight(_screenName);
                }
                catch
                {
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["Language"] = Current.Language = ConfigurationManager.AppSettings["LangID"].ToString();
                    Session["LangID"] = Current.Language == "vi" ? 1 : 0;
                    ViewBag.Title = Util.GetLang("OM24100");
                    ViewBag.Error = Message.GetString("225", null);
                    return View("Error");
                }
            }
            if (Current.UserName.PassNull() == "")
            {
                Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                Session["Language"] = Current.Language = ConfigurationManager.AppSettings["LangID"].ToString();
                Session["LangID"] = Current.Language == "vi" ? 1 : 0;
                ViewBag.Title = Util.GetLang("OM24100");
                ViewBag.Error = Message.GetString("225", null);
                return View("Error");
            }
            Util.InitRight(_screenName);
            ViewBag.Title = HQ.eSkyFramework.Util.GetLang("OM24100");
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Suggest(string lang)
        {
            return PartialView();
        }

        public ActionResult LoadMCP(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit)
        {
            var planVisit = _db.OM24100_pgMCP(Current.CpnyID, Current.UserName,
                channel, territory, province, distributor,
                shopType, slsperId, daysOfWeek, weekOfVisit).ToList();
            return this.Store(planVisit);
        }

        [ValidateInput(false)]
        public ActionResult ResetGeo(FormCollection data) 
        {
            try
            {
                var listCustDone = new List<AR_CustomerLocation>();
                var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                var lstSelCust = selCustHandler.ObjectData<OM24100_pgMCP_Result>()
                            .Where(p => !string.IsNullOrWhiteSpace(p.CustId)).ToList();

                if (lstSelCust.Count > 0)
                {
                    foreach (var selCust in lstSelCust)
                    {
                        var custLoc = _db.AR_CustomerLocation.FirstOrDefault(p => p.CustID == selCust.CustId
                            && p.BranchID == selCust.BranchID);
                        if (custLoc != null)
                        {
                            if (custLoc.tstamp.ToHex() == selCust.tstamp.ToHex())
                            {
                                if (custLoc.Lat != 0 && custLoc.Lng != 0)
                                {
                                    // Lưu lịch sử
                                    InsertCustLocationHis(custLoc, 0, 0);

                                    custLoc.Lat = 0;
                                    custLoc.Lng = 0;

                                    custLoc.LUpd_Datetime = DateTime.Now;
                                    custLoc.LUpd_Prog = _screenName;
                                    custLoc.LUpd_User = Current.UserName;
                                }
                                listCustDone.Add(custLoc);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "2015052202", "",
                                    new string[] { selCust.CustId });
                        }
                    }

                    _db.SaveChanges();
                    return Json(new { success = true, msgCode = 201405071, listCustDone = Ext.Net.JSON.Serialize(listCustDone) });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2015052201", "",
                        new string[] { Util.GetLang("Customer") });
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

        [ValidateInput(false)]
        public ActionResult UpdateAddress(FormCollection data)
        {
            try
            {
                var listCustDone = new List<AR_Customer>();
                var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                var lstSelCust = selCustHandler.ObjectData<OM24100_pgMCP_Result>()
                            .Where(p => !string.IsNullOrWhiteSpace(p.CustId)).ToList();

                if (lstSelCust.Count > 0)
                {
                    foreach (var selCust in lstSelCust)
                    {
                        var cust = _db.AR_Customer.FirstOrDefault(p => p.CustId == selCust.CustId
                            && p.BranchID == selCust.BranchID);
                        if (cust != null)
                        {
                            if (cust.tstamp.ToHex() == selCust.ctstamp.ToHex())
                            {
                                if (cust.Addr1 != selCust.SuggestAddr && !string.IsNullOrWhiteSpace(selCust.SuggestAddr))
                                {
                                    cust.Addr1 = selCust.SuggestAddr;

                                    cust.LUpd_Datetime = DateTime.Now;
                                    cust.LUpd_Prog = _screenName;
                                    cust.LUpd_User = Current.UserName;

                                    var updateAR_Customer = _db.AR_CustomerLocation.FirstOrDefault(p => p.BranchID == selCust.BranchID && p.CustID == selCust.CustId);
                                    if (updateAR_Customer != null)
                                    {
                                        updateAR_Customer.LUpd_Datetime = cust.LUpd_Datetime;
                                        updateAR_Customer.LUpd_Prog = _screenName;
                                        updateAR_Customer.LUpd_User = Current.UserName;
                                    }
                                }
                                listCustDone.Add(cust);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "2015052202", "",
                                    new string[] { selCust.CustId });
                        }
                    }

                    _db.SaveChanges();
                    return Json(new { success = true, msgCode = 201405071, listCustDone = Ext.Net.JSON.Serialize(listCustDone) });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2015052201", "",
                        new string[] { Util.GetLang("Customer") });
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

        [ValidateInput(false)]
        public ActionResult UpdateNewPosition(FormCollection data, string newLat, string newLng)
        {
            try
            {
                // This is invariant
                NumberFormatInfo format = new NumberFormatInfo();
                // Set the 'splitter' for thousands
                format.NumberGroupSeparator = ",";
                // Set the decimal seperator
                format.NumberDecimalSeparator = ".";

                var selCustHandler = new StoreDataHandler(data["lstSelCust"]);
                var selCust = selCustHandler.ObjectData<OM24100_pgMCP_Result>()
                            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.CustId));
                if (selCust != null) 
                {
                    var custLoc = _db.AR_CustomerLocation.FirstOrDefault(p => p.CustID == selCust.CustId
                            && p.BranchID == selCust.BranchID);
                    if (custLoc != null)
                    {
                        if (custLoc.tstamp.ToHex() == selCust.tstamp.ToHex())
                        {
                            // Lưu lịch sử
                            InsertCustLocationHis(custLoc, double.Parse(newLat, format), double.Parse(newLng, format));
                            custLoc.Lat = double.Parse(newLat, format);
                            custLoc.Lng = double.Parse(newLng, format);
                            custLoc.LUpd_Datetime = DateTime.Now;
                            custLoc.LUpd_Prog = _screenName;
                            custLoc.LUpd_User = Current.UserName;

                            _db.SaveChanges();
                            return Json(new { success = true, msgCode = 201405071, tstamp = Convert.ToBase64String(custLoc.tstamp) });
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        custLoc = new AR_CustomerLocation();
                        custLoc.CustID = selCust.CustId;
                        custLoc.BranchID = selCust.BranchID;
                        custLoc.Lat = double.Parse(newLat, format);
                        custLoc.Lng = double.Parse(newLng, format);

                        custLoc.Crtd_Datetime = custLoc.LUpd_Datetime = DateTime.Now;
                        custLoc.Crtd_Prog = custLoc.LUpd_Prog = _screenName;
                        custLoc.Crtd_User = custLoc.LUpd_User = Current.UserName;
                        _db.AR_CustomerLocation.AddObject(custLoc);

                        _db.SaveChanges();
                        return Json(new { success = true, msgCode = 201405071, tstamp = Convert.ToBase64String(custLoc.tstamp) });
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "6");
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

        private int InsertCustLocationHis(AR_CustomerLocation custLoc, double newLat, double newLng)
        {
            var obj = _db.OM24100_ppInsertCustLocationHis(custLoc.BranchID, custLoc.CustID, custLoc.Lat, custLoc.Lng, newLat, newLng
                                                        , custLoc.LUpd_Datetime, custLoc.LUpd_Prog, custLoc.LUpd_User
                                                        , custLoc.Crtd_Datetime, custLoc.Crtd_Prog, custLoc.Crtd_User
                                                        , Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            var lineRef = obj.HasValue ? obj.Value : 0;
            return lineRef;
        }

        [DirectMethod]
        public ActionResult ExportMcpExcel(string channel, string territory,
            string province, string distributor, string shopType,
            string slsperId, string daysOfWeek, string weekOfVisit,
            string territoryHeader, string distributorHeader, string slsperHeader,
            string daysOfWeekHeader, string weekOfVisitHeader)
        {
            try
            {
                var headerRowIdx = 8;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetMCP = workbook.Worksheets[0];
                SheetMCP.Name = Util.GetLang("MCL");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                var beforeColTexts = new string[] { "N0", "CustId", "CustName", "Addr", Util.GetLang("Lat") + "/" + Util.GetLang("Lng") };
                var dataIndexes = new string[] { "N0", "CustId", "CustName", "Addr", "LatLng" };

                #region header info
                // Title header
                SetCellValue(SheetMCP.Cells["A2"],
                    string.Format("{0}", Util.GetLang("OM24100EHeader")),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetMCP.Cells.Merge(1, 0, 1, beforeColTexts.Length);

                SetCellValue(SheetMCP.Cells["B3"],
                    string.Format("{0}", Util.GetLang("Area")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C3"].PutValue(territoryHeader);

                SetCellValue(SheetMCP.Cells["B4"],
                    string.Format("{0}", Util.GetLang("Distributor")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C4"].PutValue(distributorHeader);

                SetCellValue(SheetMCP.Cells["B5"],
                    string.Format("{0}", Util.GetLang("SalesMan")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C5"].PutValue(slsperHeader);

                SetCellValue(SheetMCP.Cells["B6"],
                    string.Format("{0}", Util.GetLang("DayOfWeek")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C6"].PutValue(daysOfWeekHeader);

                SetCellValue(SheetMCP.Cells["B7"],
                    string.Format("{0}", Util.GetLang("WeekOfVisit")),
                    TextAlignmentType.Center, TextAlignmentType.Left, true, 10, false);
                SheetMCP.Cells["C7"].PutValue(weekOfVisitHeader);

                // Header text columns
                for (int i = 0; i < beforeColTexts.Length; i++)
                {
                    var cell = SheetMCP.Cells[headerRowIdx, i];
                    SetCellValue(cell, Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    //SheetMCP.Cells.Merge(headerRowIdx, colIdx, 1, 1);

                    //Apply the border styles to the cell
                    setCellStyle(cell);
                }
                var allColumns = new List<string>();
                allColumns.AddRange(dataIndexes);

                #endregion

                #region export data
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@Channel", DbType.String, clsCommon.GetValueDBNull(channel), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@Territory", DbType.String, clsCommon.GetValueDBNull(territory), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@Province", DbType.String, clsCommon.GetValueDBNull(province), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@Distributor", DbType.String, clsCommon.GetValueDBNull(distributor), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@ShopType", DbType.String, clsCommon.GetValueDBNull(shopType), ParameterDirection.Input, int.MaxValue));
                pc.Add(new ParamStruct("@SlsperId", DbType.String, clsCommon.GetValueDBNull(slsperId), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@DaysOfWeek", DbType.String, clsCommon.GetValueDBNull(daysOfWeek), ParameterDirection.Input, 3));
                pc.Add(new ParamStruct("@WeekEO", DbType.String, clsCommon.GetValueDBNull(weekOfVisit), ParameterDirection.Input, 2));

                DataTable dtDataExport = dal.ExecDataTable("OM24100_pgMCP", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export

                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    var isRed = false;
                    var lat = dtDataExport.Rows[i]["Lat"];
                    var lng = dtDataExport.Rows[i]["Lng"];
                    if ((lat != null && lat.ToDouble() == 0) || (lng != null &&lng.ToDouble() == 0))
                    {
                        isRed = true;
                    }

                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        var cell = SheetMCP.Cells[headerRowIdx + 1 + i, j];
                        if (allColumns[j] == "N0")
                        {
                            cell.PutValue(i + 1);
                        }
                        else if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            cell.PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }

                        if (isRed)
                        {
                            var redStyle = cell.GetStyle();
                            style.Font.Color = Color.Red;
                            cell.SetStyle(style);
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
                return File(stream, "application/vnd.ms-excel", string.Format("{0}.xlsx", Util.GetLang("OM24100")));
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
    }
}

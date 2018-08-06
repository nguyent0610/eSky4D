using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Xml;
//using System.Xml.Linq;
using System.Data.Objects;
using Aspose.Cells;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.Data;
using System.Drawing;
using HQFramework.DAL;
using System.Dynamic;
using System.Globalization;
using HQFramework.Common;
namespace IN10200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10200Controller : Controller
    {
        private string _screenNbr = "IN10200";
        private string _userName = Current.UserName;
        private string _handle = "";
        private IN10200Entities _app = Util.CreateObjectContext<IN10200Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);     
        private FormCollection _form;
        private IN10200_pcBatch_Result _objBatch;
        private JsonResult _logMessage;
        private List<IN10200_pgIssueLoad_Result> _lstTrans;
        private List<IN10200_pgIN_LotTrans_Result> _lstLot;
        private IN_Setup _objIN;
        private string _whseLoc = string.Empty;
        public class IN_LotTransExt : IN_LotTrans
        {
            public double QtyOnHand { get; set; }
        }
        public class InvtCompare : IEqualityComparer<IN_LotTrans>
        {

            public bool Equals(IN_LotTrans x, IN_LotTrans y)
            {
                return x.InvtID.Equals(y.InvtID);
            }

            public int GetHashCode(IN_LotTrans obj)
            {
                return obj.InvtID.GetHashCode();
            }
        }
        #region Action
        public ActionResult Index(string branchID)
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);

            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);
            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }
            var showFromSite = false;
            var showQtyOnhand = false;
            int showWhseLoc=0;
            string perPost = string.Empty;
            bool checkPerPost = false;
            bool showSiteColumn = false;
            bool showWhseLocColumn = false;
            bool isChangeSite = false;
            bool allowSlsper = false;
            var objConfig = _app.IN10200_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                showFromSite = objConfig.ShowFromSite.HasValue && objConfig.ShowFromSite.Value;
                showQtyOnhand = objConfig.ShowQtyOnhand.HasValue && objConfig.ShowQtyOnhand.Value;
                if (objConfig.ShowWhseLoc != null) showWhseLoc = objConfig.ShowWhseLoc.Value;
                perPost = objConfig.PerPost;
                checkPerPost = objConfig.CheckPerPost ?? false;
                showSiteColumn = objConfig.ShowSiteColumn ?? false;
                showWhseLocColumn = objConfig.ShowWhseLocColumn ?? false;
                isChangeSite = objConfig.IsChangeSite ?? false;
                allowSlsper = objConfig.AllowSlsper ?? false;
            }
            //var showFromSite = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "IN10200ShowFromSite");

            //if (showFromSite != null && showFromSite.IntVal == 1)
            //{
            //    ViewBag.showFromSite = "true";
            //}
            //else
            //{
            //    
            //}
            

            if (branchID == null) branchID = Current.CpnyID;

            var userDft = _app.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID == branchID && p.UserID == Current.UserName);

            ViewBag.INSite = userDft == null ? "" : userDft.INSite;
            ViewBag.showFromSite = showFromSite;
            ViewBag.WhseLoc = userDft == null ? "" : userDft.INWhseLoc;
            ViewBag.showQtyOnhand = showQtyOnhand;
            ViewBag.BranchID = branchID;
            ViewBag.showWhseLoc = showWhseLoc;

            ViewBag.showSiteColumn = showSiteColumn;
            ViewBag.showWhseLocColumn = showWhseLocColumn;
            ViewBag.isChangeSite = isChangeSite;
            ViewBag.allowSlsper = allowSlsper;

            ViewBag.perpost = perPost;
            ViewBag.checkPerPost = checkPerPost;

            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                _form = data;
                _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();
                User user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
                var rpt = new RPTRunning();
                rpt.ResetET();

                rpt.ReportNbr = "IN602";
                rpt.MachineName = "Web";
                rpt.ReportCap = "IN_Issue";
                rpt.ReportName = "IN_Issue";
                rpt.ReportDate = DateTime.Now;
                rpt.DateParm00 = DateTime.Now;
                rpt.DateParm01 = DateTime.Now;
                rpt.DateParm02 = DateTime.Now;
                rpt.DateParm03 = DateTime.Now;
                rpt.StringParm00 = _objBatch.BranchID;
                rpt.StringParm01 = _objBatch.BatNbr;
                rpt.UserID = Current.UserName;
                rpt.AppPath = "Reports\\";
                rpt.ClientName = Current.UserName;
                rpt.LoggedCpnyID = Current.CpnyID;
                rpt.CpnyID = user.CpnyID;
                rpt.LangID = Current.LangID;

                _app.RPTRunnings.AddObject(rpt);
                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Json(new { success = true, reportID = rpt.ReportID, reportName = rpt.ReportName });
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
        public ActionResult Save(FormCollection data)
        {
            try
            {
                _form = data;
                SaveData(data);
                bool checkPerPost = data["checkPerPost"].ToBool();
                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save, new { batNbr = _objBatch.BatNbr, checkPerPost });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Util.CreateError(ex.ToString());
            }
        }
        [HttpPost]
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                var access = Session["IN10200"] as AccessRight;
                _whseLoc = data["WhseLoc"].PassNull();
                List<IN_Trans> lstTrans = new List<IN_Trans>();
                List<IN_LotTrans> lstLot = new List<IN_LotTrans>();
                if (!access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();

                if (_objBatch.Status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }

                _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
                if (_objIN == null) _objIN = new IN_Setup();

                var batch = _app.Batches.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr);
                if (batch != null) _app.Batches.DeleteObject(batch);

                if (_whseLoc != "")
                {
                    lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr &&p.WhseLoc==_whseLoc).ToList();
                }
                else
                {
                    lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                }                
               
                foreach (var trans in lstTrans)
                {
                    double oldQty = 0;
                    if (trans.TranType == "II")
                    {
                        oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                        UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                        Update_IN_ItemLoc(_whseLoc, trans.InvtID, trans.SiteID, oldQty, 0);
                    }
                    _app.IN_Trans.DeleteObject(trans);
                }
                if (_whseLoc != "")
                {
                    lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.WhseLoc==_whseLoc).ToList();
                }
                else
                {
                    lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                }
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;
                    if (lot.TranType == "II")
                    {
                        oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                        if (lot.WhseLoc.PassNull() != "")
                        {
                            UpdateAllocLot_WhseLoc(_whseLoc, lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                        }
                        else
                        {
                            UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                        }
                        //UpdateAllocLot(lot.InvtID, lot.SiteID,lot.LotSerNbr, oldQty, 0,0);
                    }
                    _app.IN_LotTrans.DeleteObject(lot);
                }

                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Delete);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Util.CreateError(ex.ToString());
            }
        }
        [HttpPost]
        public ActionResult DeleteTrans(FormCollection data)
        {
            try
            {
                var access = Session["IN10200"] as AccessRight;
                _whseLoc = data["WhseLoc"].PassNull();
                _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();

                if (_objBatch.Status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }
                if ((_objBatch.BatNbr.PassNull() == string.Empty && !access.Insert) || (_objBatch.BatNbr.PassNull() != string.Empty && !access.Update))
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
                if (_objIN == null) _objIN = new IN_Setup();
                string lineRef = Util.PassNull(data["LineRef"]);
                List<IN_Trans> lstTrans = new List<IN_Trans>();

                if (_whseLoc.PassNull() != "") {
                    lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.WhseLoc==_whseLoc).ToList();
                    var trans = lstTrans.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.LineRef == lineRef && p.WhseLoc == _whseLoc);

                    if (trans != null)
                    {
                        double oldQty = 0;
                        if (trans.TranType == "II")
                        {
                            oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                            UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                            Update_IN_ItemLoc(_whseLoc, trans.InvtID, trans.SiteID, oldQty, 0);
                        }

                        lstTrans.Remove(trans);
                        _app.IN_Trans.DeleteObject(trans);
                    }

                    var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef &&p.WhseLoc==_whseLoc).ToList();
                    foreach (var lot in lstLot)
                    {
                        double oldQty = 0;
                        if (lot.TranType == "II")
                        {
                            oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                            if (lot.WhseLoc.PassNull() != "")
                            {
                                UpdateAllocLot_WhseLoc(_whseLoc, lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                            }
                            else
                            {
                                UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                            }
                            //UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                        }
                        _app.IN_LotTrans.DeleteObject(lot);
                    }    
                }
                else
                {
                    lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                    var trans = lstTrans.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.LineRef == lineRef);

                    if (trans != null)
                    {
                        double oldQty = 0;
                        if (trans.TranType == "II")
                        {
                            oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                            UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                        }

                        lstTrans.Remove(trans);
                        _app.IN_Trans.DeleteObject(trans);
                    }

                    var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef).ToList();
                    foreach (var lot in lstLot)
                    {
                        double oldQty = 0;
                        if (lot.TranType == "II")
                        {
                            oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                            UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                        }
                        _app.IN_LotTrans.DeleteObject(lot);
                    }

                    
                }

                var batch = _app.Batches.FirstOrDefault(p => p.BatNbr == _objBatch.BatNbr && p.BranchID == _objBatch.BranchID);
                if (batch != null)
                {
                    double totAmt = 0;
                    foreach (var item in lstTrans)
                    {
                        totAmt += item.TranAmt;
                    }
                    batch.TotAmt = totAmt;
                    batch.LUpd_DateTime = DateTime.Now;
                    batch.LUpd_Prog = _screenNbr;
                    batch.LUpd_User = _userName;
                }

                _app.SaveChanges();

                string tstamp = "";
                if (batch != null)
                {
                    tstamp = batch.tstamp.ToHex();
                }

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Delete, new { tstamp });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Util.CreateError(ex.ToString());
            }
        }
        [HttpPost]
        public ActionResult Export(FormCollection data, string branchID)
        {
            try
            {
                
           
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                workbook.Worksheets.Add();
                Worksheet sheetTrans = workbook.Worksheets[0];
                Worksheet masterData = workbook.Worksheets[1];

                sheetTrans.Name = "Details";
                masterData.Name = "MasterData";
                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(data["BranchID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@EffDate", DbType.DateTime, clsCommon.GetValueDBNull(data["DateEnd"].ToDateShort()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(data["SiteID"].PassNull()), ParameterDirection.Input, 30));
                DataTable dt = dal.ExecDataTable("IN10200_pdExport", CommandType.StoredProcedure, ref pc);

                List<IN10200_pgIssueLoad_Result> lstDetail = _app.IN10200_pgIssueLoad(data["BatNbr"].PassNull(), data["BranchID"].PassNull(), "%", "%", Current.UserName, Current.CpnyID, Current.LangID).ToList();

                masterData.Cells.ImportDataTable(dt, false, "AA2");


                //lấy data cho combo SiteID
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtSite = dal.ExecDataTable("IN10200_peSiteID", CommandType.StoredProcedure, ref pc);
                masterData.Cells.ImportDataTable(dtSite, true, 0, 19, false);

                ////lấy data cho combo SiteLocation
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtSiteLocation = dal.ExecDataTable("IN10200_peSiteLocation", CommandType.StoredProcedure, ref pc);
                masterData.Cells.ImportDataTable(dtSiteLocation, true, 0, 35, false);



                Style style = workbook.GetStyleInPool(0);
                style.Font.Color = Color.Transparent;
                style.IsLocked = true;
                StyleFlag flag = new StyleFlag();
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;

                sheetTrans.Cells.Columns[19].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[20].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[21].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[26].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[27].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[28].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[29].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[35].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[36].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[37].ApplyStyle(style, flag);

                var cell = sheetTrans.Cells["B1"];
                cell.PutValue("CHI TIẾT XUẤT KHO");
                style = cell.GetStyle();
                style.Font.IsBold = true;
                style.Font.Size = 16;
                style.Font.Color = Color.Blue;
                style.HorizontalAlignment = TextAlignmentType.Center;
                cell.SetStyle(style);
                sheetTrans.Cells.Merge(0, 1, 1, 6);


                cell = sheetTrans.Cells["B2"];
                cell.PutValue("Tổng Tiền");
                style = cell.GetStyle();
                style.Font.IsBold = true;
                style.VerticalAlignment = TextAlignmentType.Center;
                style.HorizontalAlignment = TextAlignmentType.Right;
                cell.SetStyle(style);

                cell = sheetTrans.Cells["C2"];
                cell.Formula = "=SUM(H5:H" + (dt.Rows.Count + 5).ToString() + ")";
                style = cell.GetStyle();
                style.IsLocked = true;
                style.Custom = "#,##0";
                cell.SetStyle(style);

                style = sheetTrans.Cells["A4"].GetStyle();
                style.Font.IsBold = true;

                sheetTrans.Cells["A4"].PutValue("Mã Mặt Hàng");
                sheetTrans.Cells["B4"].PutValue("Diễn Giải");
                sheetTrans.Cells["C4"].PutValue("Kho");
                sheetTrans.Cells["D4"].PutValue("Vị Trí Kho");
                sheetTrans.Cells["E4"].PutValue("Đơn Vị Tính");
                sheetTrans.Cells["F4"].PutValue("Số Lượng");
                sheetTrans.Cells["G4"].PutValue("Giá Bán");
                sheetTrans.Cells["H4"].PutValue("Tổng Tiền");
                sheetTrans.Cells["I4"].PutValue("Số LOT");
                sheetTrans.Cells["J4"].PutValue("Ngày Hết Hạn(yyyy/mm/dd)");

                sheetTrans.Cells["A4"].SetStyle(style);
                sheetTrans.Cells["B4"].SetStyle(style);
                sheetTrans.Cells["C4"].SetStyle(style);
                sheetTrans.Cells["D4"].SetStyle(style);
                sheetTrans.Cells["E4"].SetStyle(style);
                sheetTrans.Cells["F4"].SetStyle(style);
                sheetTrans.Cells["G4"].SetStyle(style);
                sheetTrans.Cells["H4"].SetStyle(style);
                sheetTrans.Cells["I4"].SetStyle(style);
                sheetTrans.Cells["J4"].SetStyle(style);

                style = workbook.GetStyleInPool(0);
                style.Number = 49; //Text
                style.Font.Color = Color.Black;

                sheetTrans.Cells.Columns[0].ApplyStyle(style, flag);

                Validation validation = sheetTrans.Validations[sheetTrans.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=MasterData!$AA$2:$AA$" + dt.Rows.Count + 2;
                validation.ShowError = true;
                validation.AlertStyle = ValidationAlertType.Stop;
                validation.ErrorTitle = "Error";
                validation.InputMessage = "Chọn mã mặt hàng";
                validation.ErrorMessage = "Mã mặt hàng này không tồn tại";

                CellArea area;
                area.StartRow = 4;
                area.EndRow = dt.Rows.Count * 2 + 4;
                area.StartColumn = 0;
                area.EndColumn = 0;

                validation.AddArea(area);
                try
                {
                    string formulaInventory = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AD,2,0)),\"\",VLOOKUP({0},MasterData!$AA:$AD,2,0))", "A5");
                    sheetTrans.Cells["B5"].SetSharedFormula(formulaInventory, dt.Rows.Count * 2, 1);

                    string formulaUnitInventory = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AD,3,0)),\"\",VLOOKUP({0},MasterData!$AA:$AD,3,0))", "A5");
                    sheetTrans.Cells["E5"].SetSharedFormula(formulaUnitInventory, dt.Rows.Count * 2, 1);


                    string formulaPriceInventory = string.Format("=IF(E5<>\"\",IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AD,4,0)),\"\",VLOOKUP({0},MasterData!$AA:$AD,4,0)),\"\")", "A5");
                    sheetTrans.Cells["G5"].SetSharedFormula(formulaPriceInventory, dt.Rows.Count * 2, 1);

                    sheetTrans.Cells["H5"].SetSharedFormula("=IF(ISERROR(F5*G5),\"\",F5*G5)", dt.Rows.Count * 2, 1);



                    //Site
                    string formulaSiteID = "=MasterData!$T$2:$T$" + (dtSite.Rows.Count + 1000);
                    validation = GetValidation(ref sheetTrans, formulaSiteID, "Chọn kho", "Mã kho không tồn tại");
                    validation.AddArea(GetCellArea(1, dtSite.Rows.Count + 100, 2));

                    //SiteLocation
                    //string formulaSiteLocationID = "=Details!$AJ$2:$AJ$" + (dtSiteLocation.Rows.Count + 2);

                    string formulaSiteLocationID = string.Format("=OFFSET(MasterData!$AJ$1,IFERROR(MATCH(C{0},MasterData!$AK$2:$AK${1},0),{2}),0, IF(COUNTIF(MasterData!$AK$2:$AK${1},C{0})=0,1,COUNTIF(MasterData!$AK$2:$AK${1},C{0})),1)",
                        new string[] { "2", (dtSiteLocation.Rows.Count + 100).ToString(), (dtSiteLocation.Rows.Count + 64).ToString() });

                    validation = GetValidation(ref sheetTrans, formulaSiteLocationID, "Chọn vị trí kho", "Mã vị trí kho Không tồn tại");
                    validation.AddArea(GetCellArea(1, dtSiteLocation.Rows.Count + 100, 3));
                }
                catch (Exception)
                {

                }


                style = sheetTrans.Cells["H5"].GetStyle();
                style.Custom = "#,##0";
                Range range = sheetTrans.Cells.CreateRange("H5", "H" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);
                range = sheetTrans.Cells.CreateRange("G5", "G" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style = sheetTrans.Cells["C5"].GetStyle();
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("C5", "C" + (dtSite.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style = sheetTrans.Cells["A5"].GetStyle();
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("A5", "A" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style = sheetTrans.Cells["D5"].GetStyle();
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("D5", "D" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style.Number = 49;
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("E5", "E" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style.Number = 1;
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("F5", "F" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style.Number = 49;
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("I5", "I" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style.Number = 49;
                style.IsLocked = false;
              
                range = sheetTrans.Cells.CreateRange("J5", "J" + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                sheetTrans.AutoFitColumns();

                sheetTrans.Cells.Columns[1].Width = 30;
                sheetTrans.Cells.Columns[2].Width = 15;
                sheetTrans.Cells.Columns[4].Width = 15;
                sheetTrans.Cells.Columns[5].Width = 15;
                sheetTrans.Cells.Columns[6].Width = 15;
                sheetTrans.Cells.Columns[7].Width = 15;
                sheetTrans.Cells.Columns[8].Width = 15;

                sheetTrans.Protect(ProtectionType.All);
                masterData.Protect(ProtectionType.All);
                masterData.VisibilityType = VisibilityType.Hidden;
                int row = 5;
                foreach (var item in lstDetail)
                {
                    var invt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                    if (invt != null && invt.LotSerTrack == "L")
                    {
                        var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == item.BranchID && p.BatNbr == item.BatNbr && p.INTranLineRef == item.LineRef).ToList();
                        foreach (var item2 in lstLot)
                        {
                            sheetTrans.Cells["A" + row].PutValue(item.InvtID);
                            sheetTrans.Cells["F" + row].PutValue(item2.Qty);
                            sheetTrans.Cells["E" + row].PutValue(item2.UnitDesc);
                            sheetTrans.Cells["I" + row].PutValue(item2.LotSerNbr);
                            sheetTrans.Cells["J" + row].PutValue(item2.ExpDate.ToString("yyyy/MM/dd"));
                            row++;
                        }
                    }
                    else
                    {
                        sheetTrans.Cells["A" + row].PutValue(item.InvtID);
                        sheetTrans.Cells["F" + row].PutValue(item.Qty);
                        sheetTrans.Cells["E" + row].PutValue(item.UnitDesc);
                        sheetTrans.Cells["I" + row].PutValue("");
                        sheetTrans.Cells["J" + row].PutValue("");
                        row++;
                    }
                }

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = (data["cboBatNbr"].PassNull() == "" ? "IN10200" : data["BatNbr"].PassNull()) + ".xlsx" };
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


        [HttpPost]
        public ActionResult Import1(FormCollection data, string branchID) //hàm import củ
        {
            try
            {

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile; // or: HttpPostedFileBase file = this.HttpContext.Request.Files[0];
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
                List<IN_Trans> lstTrans = new List<IN_Trans>();
                List<IN_LotTransExt> lstLot = new List<IN_LotTransExt>();

                string siteID = data["SiteID"].PassNull();
                //string branchID = data["BranchID"].PassNull();
                int line = data["lineRef"].ToInt();
                List<string> lstImport = new List<string>();

                bool isChangeSite = false;

                var config = _app.IN10200_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (config != null) isChangeSite = config.IsChangeSite ?? false;
                var lstSite = _app.IN10200_peSiteID(branchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                var lstSiteLocation = _app.IN10200_peSiteLocation(branchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                if (fileInfo.Extension.ToLower() == ".csv" || fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    try
                    {
                        
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];                          
                            for (int j = 1; j <= workSheet.Cells.MaxDataRow; j++)
                            {
                                string InvtID = workSheet.Cells[j, 0].Value.PassNull().Trim();
                                string Unit = workSheet.Cells[j, 1].Value.PassNull().Trim();
                                string Qty = workSheet.Cells[j, 5].Value.PassNull().Trim();
                                if (InvtID.PassNull() != "")
                                {
                                    lstImport.Add(InvtID + "\t" + Unit + "\t" + Qty);
                                }
                                else
                                {
                                    lstImport.Add("@@"+ "\t" + Unit + "\t" + Qty);
                                }
                            }
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
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                    return _logMessage;
                }
               
                int i = 0;
                foreach (var item in lstImport)
                {
                    i++;
                    var obj = item.Split('\t');
                    if(obj[0].PassNull() == "" && obj[1].PassNull() == "" && obj[2].PassNull() == "")
                    {
                        continue;
                    }

                    if (obj[0].PassNull() == "@@")
                    {
                        message += string.Format("Dòng {0} thiếu mã sản phẩm<br/>", i + 1);
                        continue;
                    }
                    if (obj[1].PassNull() == "")
                    {
                        message += string.Format("Dòng {0} mặt hàng {1} không có đơn vị<br/>", i + 1, obj[0].PassNull());
                        continue;
                    }
                    if (obj[2].PassNull() == "")
                    {
                        message += string.Format("Dòng {0} mặt hàng {1} không có số lượng<br/>", i + 1, obj[0].PassNull());
                        continue;
                    }
                    double qty = 0;
                    if (double.TryParse(obj[2], out qty))
                    {
                        string invtID = obj[0];
                        var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                        if (objInvt != null)
                        {
                            var uom = SetUOM(objInvt.InvtID, objInvt.ClassID, objInvt.StkUnit, obj[1]);
                            if (uom != null)
                            {
                                var newTran = new IN_Trans();
                                newTran.CnvFact = uom.CnvFact;
                                newTran.InvtID = objInvt.InvtID;
                                newTran.Qty = qty;
                                newTran.UnitDesc = obj[1];
                                newTran.UnitMultDiv = uom.MultDiv;
                                newTran.LineRef = LastLineRef(line);
                                newTran.SiteID = siteID;
                                newTran.TranDesc = objInvt.Descr;
                                if (objInvt.ValMthd == "A" || objInvt.ValMthd == "E")
                                {
                                    var itemSite = _app.IN_ItemSite.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == objInvt.InvtID);
                                    if (itemSite != null)
                                    {
                                        newTran.UnitPrice = Math.Round(newTran.UnitMultDiv == "M" ? itemSite.AvgCost * newTran.CnvFact : itemSite.AvgCost / newTran.CnvFact, 0);
                                    }
                                }
                                newTran.TranAmt = newTran.ExtCost = newTran.UnitPrice * newTran.Qty;
                                if (objInvt.LotSerTrack == "L")
                                {
                                    double needQty = Math.Round(newTran.UnitMultDiv == "M" ? newTran.Qty * newTran.CnvFact : newTran.Qty / newTran.CnvFact, 0);

                                    List<IN_ItemLot> lstLotDB = _app.IN_ItemLot.Where(p => p.SiteID == siteID && p.InvtID == objInvt.InvtID && p.QtyAvail > 0).OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();

                                    foreach (var itemLot in lstLotDB)
                                    {
                                        double newQty = 0;
                                        if (itemLot.QtyAvail >= needQty)
                                        {
                                            newQty = needQty;
                                            needQty = 0;
                                        }
                                        else
                                        {
                                            newQty = itemLot.QtyAvail;
                                            needQty -= itemLot.QtyAvail;
                                        }

                                        if (newQty != 0)
                                        {
                                            var newLot = new IN_LotTransExt();
                                            newLot.LotSerNbr = itemLot.LotSerNbr;
                                            newLot.ExpDate = itemLot.ExpDate;
                                            newLot.WarrantyDate = DateTime.Now;
                                            newLot.INTranLineRef = LastLineRef(line);
                                            newLot.SiteID = newTran.SiteID;
                                            newLot.InvtID = newTran.InvtID;
                                            newLot.InvtMult = -1;
                                              
                                            if ((newTran.UnitMultDiv == "M" ? newQty / newTran.CnvFact : newQty * newTran.CnvFact) % 1 > 0)
                                            {
                                                newLot.CnvFact = 1;
                                                newLot.UnitMultDiv = "M";
                                                newLot.Qty = newQty;
                                                newLot.UnitDesc = objInvt.StkUnit;

                                                if (objInvt.ValMthd == "A" || objInvt.ValMthd == "E")
                                                {
                                                    newLot.UnitPrice = newLot.UnitCost = Math.Round(newTran.UnitMultDiv == "M" ? newTran.UnitPrice / newTran.CnvFact : newTran.UnitPrice * newTran.CnvFact,0);
                                                }
                                                else
                                                {
                                                    newLot.UnitPrice = newLot.UnitCost = 0;
                                                }
                                            }
                                            else
                                            {
                                                newLot.Qty = Math.Round(newTran.UnitMultDiv == "M" ? newQty / newTran.CnvFact : newQty * newTran.CnvFact,0);
                                                newLot.CnvFact = newTran.CnvFact;
                                                newLot.UnitMultDiv = newTran.UnitMultDiv;
                                                newLot.UnitPrice = newTran.UnitPrice;
                                                newLot.UnitCost = newTran.UnitPrice;
                                                newLot.UnitDesc = newTran.UnitDesc;                                                
                                            }
                                            var objOnHand = _app.IN_ItemLot.Where(x => x.InvtID == newLot.InvtID && x.SiteID == newLot.SiteID && x.LotSerNbr == newLot.LotSerNbr).FirstOrDefault();
                                            if (objOnHand != null)
                                            {
                                                newLot.QtyOnHand = objOnHand.QtyOnHand;
                                            }                                           
                                            lstLot.Add(newLot);
                                        }

                                        if (needQty == 0) break;
                                    }

                                    if (needQty != 0)
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} không đủ số lượng LOT để xuất<br/>", i + 1, obj[0]);
                                    }
                                    else
                                    {
                                        lstTrans.Add(newTran);
                                        line++;
                                    }
                                }
                                else
                                {

                                    lstTrans.Add(newTran);
                                    line++;

                                }
                            }
                            else
                            {
                                message += string.Format("Dòng {0} mặt hàng {1} đơn vị quy đổi {2} không đúng<br/>", i + 1, obj[0], obj[1]);
                            }
                        }
                        else
                        {
                            message += string.Format("Dòng {0} mặt hàng {1} không tồn tại<br/>", i + 1, obj[0]);
                        }
                    }
                    else
                    {
                        if (obj[0].PassNull() == "")
                            message += string.Format("Dòng {0} thiếu Mã Sản Phẩm<br/>", i + 1);
                        if (obj[1].PassNull() == "")
                            message += string.Format("Dòng {0} thiếu Đơn Vị<br/>", i + 1);
                        if (obj[2].PassNull() == "")
                            message += string.Format("Dòng {0} thiếu Số Lượng<br/>", i + 1);
                        else
                            message += string.Format("Dòng {0}  Số Lượng không đúng định dạng<br/>", i + 1);

                    }
                    
                }
                Util.AppendLog(ref _logMessage, "20121418", "", data: new { message, lstTrans, lstLot });
                return _logMessage;
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

        public ActionResult Import(FormCollection data, string branchID)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile; // or: HttpPostedFileBase file = this.HttpContext.Request.Files[0];
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
                List<object> lstTrans = new List<object>();
                List<IN_LotTransExt> lstLot = new List<IN_LotTransExt>();

                bool isChangeSite = false;

                var config = _app.IN10200_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (config != null) isChangeSite = config.IsChangeSite ?? false;
                var lstSite = _app.IN10200_peSiteID(branchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                var lstSiteLocation = _app.IN10200_peSiteLocation(branchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    try
                    {
                        bool check = false;
                        var lstInvtID = new List<IN_Inventory>();

                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        int lineRef = data["lineRef"].ToInt();
                        string whseLoc = data["cboWhseLoc"].PassNull();
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];
                            string invtID = string.Empty;
                           
                            for (int i = 4; i < workSheet.Cells.MaxDataRow; i++)
                            {
                                invtID = workSheet.Cells[i, 0].StringValue;
                                if (invtID == string.Empty) break;
                                var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                                if (objInvt == null)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có trong hệ thống<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                if (lstInvtID.All(x => x.InvtID != invtID))
                                {
                                    lstInvtID.Add(objInvt);
                                }
                                var siteID = workSheet.Cells[i, 2].StringValue.PassNull();
                                if (string.IsNullOrEmpty(siteID))
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có kho <br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }

                                if (isChangeSite)
                                {
                                    if (lstSite.FirstOrDefault(x => x.SiteID.ToLower() == siteID.ToLower()) == null)
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} kho không tồn tại <br/>", (i + 1).ToString(), invtID);
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (siteID.ToLower() != data["SiteID"].PassNull().ToLower())
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} kho sai dữ liệu <br/>", (i + 1).ToString(), invtID);
                                        continue;
                                    }
                                }

                                var siteLocation = workSheet.Cells[i, 3].StringValue.PassNull();
                                if (string.IsNullOrEmpty(siteLocation))
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có vị trí kho <br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                if (isChangeSite)
                                {
                                    if (lstSiteLocation.FirstOrDefault(x => x.SiteID.ToLower() == siteID.ToLower() && x.WhseLoc == siteLocation) == null)
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} vị trí kho không tồn tại <br/>", (i + 1).ToString(), invtID);
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (siteLocation.ToLower() != whseLoc.ToLower())
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} vị trí kho sai dữ liệu <br/>", (i + 1).ToString(), invtID);
                                        continue;
                                    }
                                }

                                if (workSheet.Cells[i, 4].StringValue.PassNull() == string.Empty)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có đơn vị<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                if (workSheet.Cells[i, 5].StringValue.PassNull() == "")
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có số lượng<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                else
                                {
                                    float n;
                                    bool isNumeric = float.TryParse(workSheet.Cells[i, 5].StringValue, out n);
                                    if (isNumeric == true)
                                    {
                                        if (workSheet.Cells[i, 5].FloatValue == 0)
                                        {
                                            message += string.Format("Dòng {0} mặt hàng {1} chưa nhập số lượng<br/>", (i + 1).ToString(), invtID);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        message += string.Format("Dòng {0} sai định dạng Số Lượng<br/>", (i + 1).ToString());
                                        continue;
                                    }
                                }

                                if (objInvt.LotSerTrack == "L" && workSheet.Cells[i, 8].StringValue.PassNull() == string.Empty)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} chưa nhập số LOT<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }

                                if (objInvt.LotSerTrack == "L" && workSheet.Cells[i, 9].Value.PassNull() == string.Empty)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} chưa nhập ngày hết hạn<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                if (objInvt.LotSerTrack == "L" && workSheet.Cells[i, 9].Value.PassNull() != string.Empty)
                                {

                                    DateTime parsed;
                                    bool valid = DateTime.TryParseExact(workSheet.Cells[i, 9].StringValue, "yyyy/MM/dd",
                                                                        CultureInfo.InvariantCulture,
                                                                        DateTimeStyles.None,
                                                                        out parsed);

                                    if (valid == false)
                                    {
                                        message += string.Format("Dòng {0} sai định dạng Ngày Hết Hạn. Ngày Hết Hạn phải có dạng ({1})<br/>", (i + 1).ToString(), "yyyy/MM/dd");
                                        continue;
                                    }
                                }


                                if (objInvt.LotSerTrack == "L" && lstLot.Any(p => p.InvtID == invtID && p.LotSerNbr == workSheet.Cells[i, 8].StringValue))
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} trùng số lot {2}<br/>", (i + 1).ToString(), invtID, workSheet.Cells[i, 8].StringValue);
                                    continue;
                                }

                                if (objInvt.LotSerTrack != "L" && workSheet.Cells[i, 8].Value.PassNull() != string.Empty)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} Không được nhập số lot<br/>", (i + 1), invtID);
                                    continue;
                                }


                                if (objInvt.LotSerTrack == "L" && workSheet.Cells[i, 8].Value.PassNull() != string.Empty)
                                {
                                    var lstLotCheck = _app.IN10200_pdGetLot(1, siteID, invtID, siteLocation, "", Current.UserName, Current.CpnyID, Current.LangID).ToList();
                                    if (lstLotCheck.All(x => x.LotSerNbr.Trim() != workSheet.Cells[i, 8].StringValue.Trim()) || !lstLotCheck.Any())
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} Nhập sai lot<br/>", (i + 1), invtID);
                                        continue;
                                    }
                                
                                }
                                var newLot = new IN_LotTransExt();

                                workSheet.Cells[i, 4].Calculate(true, null);
                                workSheet.Cells[i, 6].Calculate(true, null);

                                newLot.CnvFact = 1;
                                if (objInvt.LotSerTrack == "L")
                                {
                                    string[] strExpDate = workSheet.Cells[i, 9].StringValue.PassNull().Split('/');
                                    DateTime dExpDate = new DateTime(int.Parse(strExpDate[0]), int.Parse(strExpDate[1]), int.Parse(strExpDate[2]));
                                    newLot.LotSerNbr = workSheet.Cells[i, 8].StringValue.PassNull();
                                    var item = _app.IN_ItemLot.FirstOrDefault(p => p.InvtID == invtID && p.LotSerNbr == newLot.LotSerNbr);
                                    if (item != null)
                                    {
                                        newLot.ExpDate = item.ExpDate;
                                        check = true;
                                    }
                                    else
                                    {
                                        newLot.ExpDate = dExpDate;
                                    }

                                }

                                newLot.InvtID = invtID.ToUpper();
                                newLot.InvtMult = 1;
                                newLot.Qty = workSheet.Cells[i, 5].StringValue.PassNull() == "" ? 0 : workSheet.Cells[i, 5].FloatValue;

                                newLot.SiteID = siteID; //data["SiteID"].PassNull();
                                newLot.WhseLoc = siteLocation;//whseLoc;
                                newLot.TranDate = data["DateEnt"].ToDateShort();
                                newLot.TranType = "RC";
                                newLot.UnitDesc = workSheet.Cells[i, 4].StringValue;
                                newLot.UnitMultDiv = "M";
                                newLot.WarrantyDate = ("1-1-1900").ToDateShort();
                                //if (objInvt.ValMthd == "A" || objInvt.ValMthd == "E")
                                //{
                                //    var item = _app.IN_ItemSite.FirstOrDefault(p => p.SiteID == newLot.SiteID && p.InvtID== newLot.InvtID);
                                //    if (item != null)
                                //    {
                                //        newLot.UnitCost = newLot.UnitPrice = item.AvgCost;
                                //    }
                                //}
                                //else
                                //{
                                newLot.UnitCost = newLot.UnitPrice = _app.IN10200_pdPrice("", invtID, newLot.UnitDesc, newLot.TranDate, objInvt.ValMthd, newLot.SiteID).FirstOrDefault().Price.Value;
                                //}

                                var qtyLotOnHand = _app.IN_ItemLot.FirstOrDefault(x => x.InvtID == newLot.InvtID && x.SiteID == newLot.SiteID && x.LotSerNbr == newLot.LotSerNbr);
                                if (qtyLotOnHand != null)
                                {
                                    newLot.QtyOnHand = qtyLotOnHand.QtyOnHand;
                                }

                                lstLot.Add(newLot);
                            }
                        }

                        var lstInvt = lstLot.Distinct(new InvtCompare()).ToList();
                        foreach (var item in lstInvt)
                        {
                            var objInvt =  lstInvtID.FirstOrDefault(p => p.InvtID.ToUpper() == item.InvtID.ToUpper());// _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);

                            var newTrans = new IN10200_pgIssueLoad_Result();
                            newTrans.InvtID = item.InvtID.ToUpper();
                            newTrans.LineRef = LastLineRef(lineRef);
                            newTrans.ReasonCD = data["ReasonCD"].PassNull();
                            newTrans.TranDate = item.TranDate;
                            newTrans.UnitDesc = item.UnitDesc;
                            newTrans.CnvFact = 1;
                            newTrans.UnitMultDiv = "M";
                            newTrans.InvtMult = 1;
                            newTrans.TranType = "RC";
                            newTrans.JrnlType = "IN";
                            newTrans.TranDesc = objInvt.Descr;
                            newTrans.WhseLoc = item.WhseLoc;// whseLoc;

                            var tmp = lstLot.Where(p => p.InvtID == item.InvtID).ToList();
                            foreach (var lot in tmp)
                            {
                                newTrans.Qty += lot.Qty;
                                lot.INTranLineRef = newTrans.LineRef;
                                if (objInvt.LotSerTrack != "L")
                                {
                                    lstLot.Remove(lot);
                                }
                            }


                            newTrans.UnitPrice = newTrans.UnitCost = item.UnitPrice;
                            newTrans.TranAmt = Math.Round(newTrans.UnitPrice * newTrans.Qty, 0);
                            newTrans.SiteID = item.SiteID;
                            var qtyOnHand = _app.IN_ItemSite.FirstOrDefault(x => x.InvtID == newTrans.InvtID && x.SiteID == newTrans.SiteID);
                            if (qtyOnHand != null)
                            {
                                newTrans.QtyOnHand = qtyOnHand.QtyOnHand;
                            }

                            lstTrans.Add(newTrans);

                            lineRef++;

                        }
                        //if (check)
                        //{
                        //    Util.AppendLog(ref _logMessage, "123", "", data: new { message, lstTrans, lstLot });
                        //}
                        //else
                        //{
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message, lstTrans, lstLot });
                        //}

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
                return _logMessage;
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
        #endregion

        #region Source
        public ActionResult GetBatch(string branchID, string query, int start, int limit, int page)
        {
            query = query ?? string.Empty;
            if (page != 1) query = string.Empty;
            var lstBatch = _app.IN10200_pcBatch(Current.UserName, branchID, query, start + 1, start + 20).ToList();
            var paging = new Paging<IN10200_pcBatch_Result>(lstBatch, lstBatch.Count > 0 ? lstBatch[0].TotalRecords.Value : 0);
            return this.Store(paging.Data, paging.TotalRecords);
        }
        public ActionResult GetUserDefault()
        {
            string userName = Current.UserName;
            string cpnyID = Current.CpnyID;
            var objUser = _app.OM_UserDefault.FirstOrDefault(p => p.UserID == userName && p.DfltBranchID == cpnyID);
            return this.Store(objUser);
        }
        public ActionResult GetSetup()
        {
            string cpnyID = Current.CpnyID;
            var objSetup = _app.IN_Setup.FirstOrDefault(p => p.SetupID == "IN" && p.BranchID == cpnyID);
            return this.Store(objSetup);
        }
        public ActionResult GetTrans(string batNbr, string branchID)
        {
            var lstTrans = _app.IN10200_pgIssueLoad(batNbr, branchID, "%", "%", Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstTrans);
        }
        public ActionResult GetPrice(string invtID, string uom, DateTime effDate, string siteID, string valMthd)
        {
            var lstPrice = _app.IN10200_pdPrice("", invtID, uom, DateTime.Now, valMthd, siteID).ToList();
            return this.Store(lstPrice);
        }
        public ActionResult GetItemSite(string invtID, string siteID, string whseLoc,int showWhseLoc)
        {
            
            if(showWhseLoc==2 || (showWhseLoc==1 && (whseLoc.PassNull()!=""))){
               var  objSite = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc==whseLoc);
               return this.Store(objSite);
            }
            else{
               var  objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
               return this.Store(objSite);
            }  
            
            
        }
        public ActionResult GetUnitConversion()
        {
            var lstUnit = _app.IN10200_pcUnitConversion(Current.CpnyID).ToList();
            return this.Store(lstUnit);
        }
        public ActionResult GetUnit(string invtID)
        {
            IN_Inventory invt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (invt == null) invt = new IN_Inventory();
            List<IN10200_pcUnit_Result> lstUnit = _app.IN10200_pcUnit(invt.ClassID, invt.InvtID).ToList();
            return this.Store(lstUnit, lstUnit.Count);
        }

        //public ActionResult GetLot(string invtID, string siteID, string batNbr, string branchID, string whseLoc, bool showWhseLoc)
        //{
        //    List<IN10200_pdIN_ItemLot_Result> lstLot = new List<IN10200_pdIN_ItemLot_Result>();
        //    List<IN10200_pdIN_ItemLot_Result> lstLotDB = new List<IN10200_pdIN_ItemLot_Result>();
        //    List<IN_LotTrans> lstLotTrans = new List<IN_LotTrans>();
        //    if (showWhseLoc)
        //    {
        //        lstLotDB = _app.IN10200_pdIN_ItemLot(showWhseLoc, siteID, invtID, whseLoc, Current.UserName, Current.CpnyID, Current.LangID).ToList();   //.Where(p => p.SiteID == siteID && p.InvtID == invtID && p.WhseLoc == whseLoc && p.QtyAvail > 0).ToList();
        //        lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc).ToList();
        //    }
        //    else
        //    {
        //        lstLotDB = _app.IN10200_pdIN_ItemLot(showWhseLoc, siteID, invtID, whseLoc, Current.UserName, Current.CpnyID, Current.LangID).ToList();
        //        lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID).ToList();
        //    }            

        //    foreach (var item in lstLotDB)
        //    {
        //        lstLot.Add(item);
        //    }           
           
        //    foreach (var item in lstLotTrans)
        //    {
        //        var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
        //        if (lot == null)
        //        {
                    
        //            if (showWhseLoc)
        //            {
        //                var lotDB = _app.IN10200_pdIN_ItemLot(showWhseLoc, siteID, invtID, whseLoc, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
        //                lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
        //                lstLot.Add(lotDB);
        //            }
        //            else
        //            {
        //                var lotDB = _app.IN10200_pdIN_ItemLot(showWhseLoc, siteID, invtID, whseLoc, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
        //                lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
        //                lstLot.Add(lotDB);
        //            }
        //        }
        //        else
        //        {
        //            lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
        //        }


        //    }
        //    lstLot = lstLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();

        //    return this.Store(lstLot, lstLot.Count);
        //}

        public ActionResult GetLot(string invtID, string siteID, string batNbr, string branchID, string whseLoc, int showWhseLoc, int cnvFact)
        {
            List<IN10200_pdGetLot_Result> lstLot = new List<IN10200_pdGetLot_Result>();
            List<IN10200_pdGetLot_Result> lstLotDB = new List<IN10200_pdGetLot_Result>();
            List<IN_LotTrans> lstLotTrans;
            bool key = showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != "");

            if (key)
            {
                lstLotDB = _app.IN10200_pdGetLot(1, siteID, invtID, whseLoc,"", Current.UserName, Current.CpnyID, Current.LangID).ToList();
                lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc).ToList();
            }
            else
            {
                lstLotDB = _app.IN10200_pdGetLot(0, siteID, invtID, whseLoc,"", Current.UserName, Current.CpnyID, Current.LangID).ToList();
                lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID).ToList();
            }
            foreach (var item in lstLotDB)
            {
                item.QtyAvail = Math.Floor(item.QtyAvail / cnvFact);
                lstLot.Add(item);
            }
            foreach (var item in lstLotTrans)
            {
                var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                if (lot == null)
                {
                    if (key)
                    {
                        var lotDB = _app.IN10200_pdGetLot(2, siteID, invtID, whseLoc, item.LotSerNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();//.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == item.LotSerNbr && p.WhseLoc == whseLoc);
                        lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        lstLot.Add(lotDB);
                    }
                    else
                    {
                        var lotDB = _app.IN10200_pdGetLot(3, siteID, invtID, whseLoc, item.LotSerNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                        lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        lstLot.Add(lotDB);
                    }
                }
                else
                {
                    lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                }
            }
            lstLot = lstLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();
            return this.Store(lstLot, lstLot.Count);
        }
        public ActionResult GetLotTrans(string branchID, string batNbr, string whseLoc)
        {
            List<IN10200_pgIN_LotTrans_Result> lstLotTrans = _app.IN10200_pgIN_LotTrans(batNbr, branchID,whseLoc, Current.UserName, Current.CpnyID, Current.LangID).ToList();  //.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetItemLot(string invtID, string siteID, string lotSerNbr, string branchID, string batNbr, string whseLoc, int showWhseLoc)
        {
            bool key = false;
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                key = true;
            }
            var lot = _app.IN10200_pdGetItemLot(key, siteID, invtID, whseLoc, lotSerNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();//.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr);
            List<IN_LotTrans> lotTrans = new List<IN_LotTrans>();
            if (lot == null) lot = new IN10200_pdGetItemLot_Result()
            {
                InvtID = invtID,
                SiteID = siteID,
                LotSerNbr = lotSerNbr
            };
            if (key)
            {
                lotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr && p.WhseLoc==whseLoc).ToList();
            }
            else
            {
                lotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr).ToList();
            }
            

            foreach (var item in lotTrans)
            {
                lot.QtyAvail += (item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact);
            }

            List<IN10200_pdGetItemLot_Result> lstLot = new List<IN10200_pdGetItemLot_Result>() { lot };          
            return this.Store(lstLot, lstLot.Count);
        }
       
        #endregion

        private void CheckData()
        {
            var access = Session[_screenNbr] as AccessRight;

            if ((_objBatch.BatNbr.PassNull() != string.Empty && !access.Update) || (_objBatch.BatNbr.PassNull() == string.Empty && !access.Insert))
            {
                throw new MessageException(MessageType.Message, "728");
            }

            if (_objBatch.Status.PassNull() != "H" && (_handle==string.Empty || _handle=="N"))
            {
                throw new MessageException(MessageType.Message, "2015020803");
            }
           
            if (_lstTrans.Count == 0)
            {
                throw new MessageException(MessageType.Message, "2015020804", "");
            }

            for (int i = 0; i < _lstTrans.Count; i++)
            {
                string invtID = _lstTrans[i].InvtID;
                string siteID = _lstTrans[i].SiteID;
                double editQty = 0;
                double qtyTot = 0;
                if (_lstTrans[i].InvtID.PassNull() == "") continue;
                if (_lstTrans[i].Qty == 0)
                {
                    throw new MessageException("1000", new[] { Util.GetLang("Qty") });
                }

                if (_lstTrans[i].SiteID.PassNull() == string.Empty)
                {
                    throw new MessageException("1000", new[] { Util.GetLang("SiteID") });
                }

                if (_lstTrans[i].UnitMultDiv.PassNull() == string.Empty || _lstTrans[i].UnitDesc.PassNull() == string.Empty)
                {
                    throw new MessageException("2525", new[] { _lstTrans[i].InvtID });
                }

                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack != "N")
                {
                    var lstLot = _lstLot.Where(p => p.INTranLineRef == _lstTrans[i].LineRef).ToList();
                    double lotQty = 0;
                    foreach (var item in lstLot)
                    {
                        if (item.InvtID != _lstTrans[i].InvtID || item.SiteID != _lstTrans[i].SiteID)
                        {
                            throw new MessageException("2015040501", new[] { _lstTrans[i].InvtID });
                        }

                        if (item.UnitMultDiv.PassNull() == string.Empty || item.UnitDesc.PassNull() == string.Empty)
                        {
                            throw new MessageException("2015040503", new[] { _lstTrans[i].InvtID });
                        }

                        lotQty += Math.Round(item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact, 0);
                    }
                    double detQty = Math.Round(_lstTrans[i].UnitMultDiv == "M" ? _lstTrans[i].Qty * _lstTrans[i].CnvFact : _lstTrans[i].Qty / _lstTrans[i].CnvFact, 0);
                    if (detQty != lotQty)
                    {
                        throw new MessageException("2015040502", new[] { _lstTrans[i].InvtID });
                    }
                }
            }
        }

        private void SaveData(FormCollection data)
        {
            _whseLoc = data["WhseLoc"].PassNull();
            if (_lstTrans == null)
            {
                var transHandler = new StoreDataHandler(data["lstTrans"]);
                _lstTrans = transHandler.ObjectData<IN10200_pgIssueLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }
            if (_lstLot == null)
            {
                var lotHandler = new StoreDataHandler(data["lstLot"]);
                _lstLot = lotHandler.ObjectData<IN10200_pgIN_LotTrans_Result>().Where(p => Util.PassNull(p.INTranLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            }

            _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();

            _handle = data["Handle"].PassNull();
            _objBatch.Status = _objBatch.Status.PassNull() == string.Empty ? "H" : _objBatch.Status;

            if (_app.IN10200_ppCheckCloseDate(_objBatch.BranchID, _objBatch.DateEnt.ToDateShort(), "IN10200",Current.UserName,Current.CpnyID, Current.LangID).FirstOrDefault() == "0")
                throw new MessageException(MessageType.Message, "301");

            Batch batch = _app.Batches.FirstOrDefault(p => p.BatNbr == _objBatch.BatNbr && p.BranchID == _objBatch.BranchID);
            if ((_objBatch.Status == "U" || _objBatch.Status == "C") && (_handle == "C" || _handle == "V"))
            {
                if (batch != null)
                {
                    if (batch.tstamp.ToHex() != data["tstamp"].ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2015020802", "", new[] { batch.BatNbr });
                }

            }
            else
            {

                CheckData();

                Save_Batch(batch);

            }
            _app.SaveChanges();

            if (_handle != "N")
            {
                DataAccess dal = Util.Dal();
                INProcess.IN inventory = new INProcess.IN(_userName, _screenNbr, dal);
                try
                {
                    if (_handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);

                        inventory.IN10200_Release(_objBatch.BranchID, _objBatch.BatNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                    else if (_handle == "C" || _handle == "V")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);

                        inventory.IN10200_Cancel(_objBatch.BranchID, _objBatch.BatNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", "");
                    }

                }
                catch (Exception ex)
                {
                    dal.RollbackTrans();
                    throw ex;
                }
                finally
                {
                    inventory = null;
                    dal = null;
                }
            }
        }
        private void Save_Batch(Batch batch)
        {
            if (batch != null)
            {
                if (batch.tstamp.ToHex() != _form["tstamp"].ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }

                Update_Batch(batch, false);

            }
            else
            {
                _objBatch.BatNbr = _app.INNumbering(_objBatch.BranchID, "BatNbr").FirstOrDefault();
                _objBatch.RefNbr = _app.INNumbering(_objBatch.BranchID, "RefNbr").FirstOrDefault();
                batch = new Batch();
                Update_Batch(batch, true);
                _app.Batches.AddObject(batch);
            }

            Save_Trans(batch);
        }
        private void Save_Trans(Batch batch)
        {
            _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            if (_objIN == null) _objIN = new IN_Setup();
            foreach (var trans in _lstTrans)
            {

                var transDB = _app.IN_Trans.FirstOrDefault(p =>
                                p.BatNbr == _objBatch.BatNbr && p.RefNbr == trans.RefNbr &&
                                p.BranchID == _objBatch.BranchID && p.LineRef == trans.LineRef);

                if (transDB != null)
                {
                    if (transDB.tstamp.ToHex() != trans.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    Update_Trans(batch, transDB, trans, false);
                }
                else
                {
                    transDB = new IN_Trans();
                    Update_Trans(batch, transDB, trans, true);
                    _app.IN_Trans.AddObject(transDB);
                }
                Save_Lot(batch, transDB);
            }

            _app.SaveChanges();
        }
        private bool Save_Lot(Batch batch, IN_Trans tran)
        {
            var lots = _app.IN_LotTrans.Where(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr).ToList();
            foreach (var item in lots)
            {
                if (item.EntityState == EntityState.Deleted || item.EntityState == EntityState.Detached) continue;
                if (!_lstLot.Any(p => p.INTranLineRef == item.INTranLineRef && p.LotSerNbr == item.LotSerNbr))
                {
                    var oldQty = item.UnitMultDiv == "D" ? item.Qty / item.CnvFact : item.Qty * item.CnvFact;
                    if (item.WhseLoc.PassNull()!="")
                    {
                        UpdateAllocLot_WhseLoc(_whseLoc,item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
                    }
                    else
                    {
                        UpdateAllocLot(item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
                    }
                    _app.IN_LotTrans.DeleteObject(item);
                }
            }

            var lstLotTmp = _lstLot.Where(p => p.INTranLineRef == tran.LineRef).ToList();
            foreach (var lotCur in lstLotTmp)
            {
                var lot = _app.IN_LotTrans.FirstOrDefault(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr && p.INTranLineRef == lotCur.INTranLineRef && p.LotSerNbr == lotCur.LotSerNbr);
                if (lot == null || lot.EntityState == EntityState.Deleted || lot.EntityState == EntityState.Detached)
                {
                    lot = new IN_LotTrans();
                    Update_Lot(lot, lotCur, batch, tran, true);
                    _app.IN_LotTrans.AddObject(lot);
                }
                else
                {
                    Update_Lot(lot, lotCur, batch, tran, false);
                }
            }
            return true;
        }

        private void Update_Batch(Batch t, bool isNew)
        {
            if (isNew)
            {
                t.ResetET();
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                t.RefNbr = _objBatch.RefNbr;
                t.Module = "IN";

                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
                t.Crtd_DateTime = DateTime.Now;
            }
            else
                t.RefNbr = _objBatch.RefNbr;

            t.JrnlType = _objBatch.JrnlType.PassNull() == string.Empty ? "IN" : _objBatch.JrnlType;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.DateEnt = _objBatch.DateEnt.ToDateShort();
            t.Descr = _objBatch.Descr;
            t.EditScrnNbr = t.EditScrnNbr.PassNull() == string.Empty ? _screenNbr : t.EditScrnNbr;
            t.FromToSiteID = _objBatch.FromToSiteID;
            t.IntRefNbr = _objBatch.IntRefNbr;
            t.NoteID = 0;
            t.RvdBatNbr = _objBatch.RvdBatNbr;
            t.ReasonCD = _objBatch.ReasonCD;
            t.TotAmt = _objBatch.TotAmt;
            t.Rlsed = 0;
            t.Status = _objBatch.Status;

            t.PerPost = _objBatch.PerPost;
            t.WhseLoc = _objBatch.WhseLoc;
            t.SiteID = _objBatch.SiteID;
        }
        private void Update_Trans(Batch batch, IN_Trans t, IN10200_pgIssueLoad_Result s, bool isNew)
        {
            double oldQty, newQty;

            if (s.TranType == "II")
            {
                if (!isNew)
                    oldQty = t.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                else
                    oldQty = 0;

                newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

                UpdateINAlloc(t.InvtID, t.SiteID, oldQty, 0);
                UpdateINAlloc(s.InvtID, s.SiteID, 0, newQty);
                if (!string.IsNullOrEmpty(s.WhseLoc))
                {
                    Update_IN_ItemLoc(s.WhseLoc, t.InvtID, t.SiteID, oldQty, 0);
                    Update_IN_ItemLoc(s.WhseLoc, s.InvtID, s.SiteID, 0, newQty);
                }
                
                
            }

            if (isNew)
            {
                t.ResetET();

                t.LineRef = s.LineRef;
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                t.RefNbr = _objBatch.RefNbr;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
           // t.ReasonCD = batch.ReasonCD;
            t.CnvFact = s.CnvFact;
            t.ExtCost = Math.Round(s.TranAmt, 0);
            t.InvtID = s.InvtID;
            t.InvtMult = s.InvtMult;
            t.JrnlType = s.JrnlType;
            t.ObjID = s.ObjID;
            t.Qty = s.Qty;
            t.SiteID = s.SiteID;
            t.ToSiteID = s.ToSiteID;
            t.ShipperID = s.ShipperID;
            t.ShipperLineRef = s.ShipperLineRef;
            t.TranAmt = s.TranAmt;
            t.TranFee = s.TranFee;
            t.TranDesc = s.TranDesc;
            t.TranType = s.TranType;
            t.TranDate = batch.DateEnt;
            t.UnitCost = s.UnitCost;
            t.UnitDesc = s.UnitDesc;
            t.UnitMultDiv = s.UnitMultDiv;
            t.UnitPrice = s.UnitPrice;
            t.WhseLoc = s.WhseLoc; //_form["cboWhseLoc"].PassNull();
            t.ReasonCD = s.ReasonCD;//batch.ReasonCD;
            t.SlsperID = s.SlsperID; //_form["SlsperID"].PassNull();
            t.PosmID = s.PosmID;
        }
        private bool Update_Lot(IN_LotTrans t, IN10200_pgIN_LotTrans_Result s, Batch batch, IN_Trans tran, bool isNew)
        {

            if (isNew)
            {
                t.ResetET();
                t.BatNbr = batch.BatNbr;
                t.BranchID = batch.BranchID;
                t.INTranLineRef = s.INTranLineRef;
                t.LotSerNbr = s.LotSerNbr;
                t.RefNbr = tran.RefNbr;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;

                t.WarrantyDate = s.WarrantyDate;//DateTime.Now.ToDateShort();
            }

            double oldQty = 0;
            double newQty = 0;

            if (tran.TranType == "II")
            {
                if (!isNew)
                    oldQty = s.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                else
                    oldQty = 0;

                newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

                if (!string.IsNullOrEmpty(s.WhseLoc))
                {
                    UpdateAllocLot_WhseLoc(s.WhseLoc, t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot_WhseLoc(s.WhseLoc, s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
                    {
                        throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                    }
                }
                else
                {
                    UpdateAllocLot(t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot(s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
                    {
                        throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                    }
                }               
            }           
            
            t.UnitDesc = s.UnitDesc;
            t.ExpDate = s.ExpDate;
            t.InvtID = s.InvtID;
            t.InvtMult = tran.InvtMult;
            t.Qty = s.Qty;
            t.WhseLoc = _whseLoc.PassNull();
            t.SiteID = s.SiteID;

            t.MfgrLotSerNbr = s.MfgrLotSerNbr.PassNull();
            t.TranType = tran.TranType;

            t.TranDate = batch.DateEnt;
            t.CnvFact = s.CnvFact;
            t.UnitCost = s.UnitCost;
            t.UnitPrice = s.UnitPrice;

            t.UnitMultDiv = s.UnitMultDiv;

            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.LUpd_DateTime = DateTime.Now;

            return true;
        }

        private bool UpdateINAlloc(string invtID, string siteID, double oldQty, double newQty)
        {
            try
            {
                var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objSite = _app.IN_ItemSite.FirstOrDefault(p=> p.InvtID == invtID && p.SiteID == siteID);

                    if (objSite == null) objSite = new IN_ItemSite() { SiteID = siteID, InvtID = invtID };
                    
                    if (!_objIN.NegQty && newQty > 0 && objSite.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "608","",new string[] {invtID,siteID});
                    }
                    objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + newQty - oldQty, 0);
                    objSite.QtyAvail = Math.Round(objSite.QtyAvail - newQty + oldQty, 0);
                    
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private bool Update_IN_ItemLoc(string whseLoc,string invtID, string siteID, double oldQty, double newQty)
        {
            try
            {                
                var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objSite = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc==whseLoc) ??
                                  new IN_ItemLoc() { SiteID = siteID, InvtID = invtID, WhseLoc=whseLoc };

                    if (!_objIN.NegQty && newQty > 0 && objSite.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "2018051411", "", new string[] { invtID, siteID,_whseLoc });
                    }
                    objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + newQty - oldQty, 0);
                    objSite.QtyAvail = Math.Round(objSite.QtyAvail - newQty + oldQty, 0);

                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool UpdateAllocLot(string invtID, string siteID, string lotSerNbr, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                var objItemLot = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr);
                if (objItemLot != null)
                {
                    if (!_objIN.NegQty && newQty > 0 && objItemLot.QtyAvail + oldQty - newQty < 0)
                    {
                        
                        //Util.AppendLog(ref _logMessage, "608", parm: new[] { objItemLot.InvtID + " " objItemLot.LotSerNbr , objItemSite.SiteID });
                        return false;
                    }
                    objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + newQty - oldQty, decQty);
                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail - newQty + oldQty, decQty);
                }
                return true;
            }
            return true;
        }

        private bool UpdateAllocLot_WhseLoc(string whseLoc, string invtID, string siteID, string lotSerNbr, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                var objItemLot = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc);
                if (objItemLot != null)
                {
                    if (!_objIN.NegQty && newQty > 0 && objItemLot.QtyAvail + oldQty - newQty < 0)
                    {

                        //Util.AppendLog(ref _logMessage, "608", parm: new[] { objItemLot.InvtID + " " objItemLot.LotSerNbr , objItemSite.SiteID });
                        return false;
                    }
                    objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + newQty - oldQty, decQty);
                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail - newQty + oldQty, decQty);
                }
                return true;
            }
            return true;
        }

        private string LastLineRef(int num)
        {
            string lineRef = num.ToString();
            int len = lineRef.Length;
            for (int i = 0; i < 5 - len; i++)
            {
                lineRef = "0" + lineRef;
            }
            return lineRef;
        }
        private IN_UnitConversion SetUOM(string invtID, string classID, string stkUnit, string fromUnit)
        {
            if (!string.IsNullOrEmpty(fromUnit))
            {
                IN_UnitConversion data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "3" && p.ClassID == "*" && p.InvtID == invtID && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "2" && p.ClassID == classID && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "1" && p.ClassID == "*" && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                throw new MessageException("2525", new[] { invtID });
                return null;
            }
            return null;
        }

      
    }
}

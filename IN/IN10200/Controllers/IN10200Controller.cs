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
            bool showImport = false;
            bool showExport = false;


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
                showImport = objConfig.ShowImport ?? false;
                showExport = objConfig.ShowExport ?? false;
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
            ViewBag.showImport = showImport;
            ViewBag.showExport = showExport;

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
                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save, new { batNbr = _objBatch.BatNbr });
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
               
                lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();                               
               
                foreach (var trans in lstTrans)
                {
                    double oldQty = 0;
                    if (trans.TranType == "II")
                    {
                        oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                        UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                        if (trans.WhseLoc.PassNull() != "")
                        {
                            Update_IN_ItemLoc(trans.WhseLoc, trans.InvtID, trans.SiteID, oldQty, 0);
                        }
                    }
                    _app.IN_Trans.DeleteObject(trans);
                }
                
                lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();

                foreach (var lot in lstLot)
                {
                    double oldQty = 0;
                    if (lot.TranType == "II")
                    {
                        oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                        if (lot.WhseLoc.PassNull() != "")
                        {
                            UpdateAllocLot_WhseLoc(lot.WhseLoc, lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
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
                    lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                    var trans = lstTrans.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.LineRef == lineRef);

                    if (trans != null)
                    {
                        double oldQty = 0;
                        if (trans.TranType == "II")
                        {
                            oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                            UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                            if (trans.WhseLoc.PassNull() != "")
                            {
                                Update_IN_ItemLoc(trans.PassNull(), trans.InvtID, trans.SiteID, oldQty, 0);
                            }                            
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
                            if (lot.WhseLoc.PassNull() != "")
                            {
                                UpdateAllocLot_WhseLoc(lot.WhseLoc, lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
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
                int showWhseLoc = 0;
                bool showPackageID = false;
                var config = _app.IN10200_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (config != null)
                {
                    showWhseLoc = config.ShowWhseLoc.ToInt();
                }
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
                pc.Add(new ParamStruct("@WhseLoc", DbType.String, clsCommon.GetValueDBNull(data["cboWhseLoc"].PassNull()), ParameterDirection.Input, 30));
                DataTable dt = dal.ExecDataTable("IN10200_pdImportInventory", CommandType.StoredProcedure, ref pc);

                List<IN10200_pgIssueLoad_Result> lstDetail = _app.IN10200_pgIssueLoad(data["BatNbr"].PassNull(), data["BranchID"].PassNull(), "%", "%", Current.UserName, Current.CpnyID, Current.LangID).ToList();

                masterData.Cells.ImportDataTable(dt, true, 1, 26, false);

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

                ////lấy data cho combo LotSerNbr
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtLotSerNbr = dal.ExecDataTable("IN10200_peLotSerNbr", CommandType.StoredProcedure, ref pc);
                masterData.Cells.ImportDataTable(dtLotSerNbr, true, 0, 40, false);


                var colTextsHeader = GetHeader(showPackageID, showWhseLoc);

                for (int i = 0; i < colTextsHeader.Count; i++)
                {
                    SetCellValue(sheetTrans.Cells[3, i], Util.GetLang(colTextsHeader[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, false);
                }

                Style style = workbook.GetStyleInPool(0);
                style.Font.Color = Color.Transparent;
                style.IsLocked = true;
                StyleFlag flag = new StyleFlag();
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;

                //sheetTrans.Cells.Columns[19].ApplyStyle(style, flag);
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
                cell.PutValue(Util.GetLang("IN10200Header"));
                style = cell.GetStyle();
                style.Font.IsBold = true;
                style.Font.Size = 16;
                style.Font.Color = Color.Blue;
                style.HorizontalAlignment = TextAlignmentType.Center;
                cell.SetStyle(style);

                sheetTrans.Cells.Merge(0, 1, 1, 6);


                cell = sheetTrans.Cells["B2"];
                cell.PutValue(Util.GetLang("IN10200TotalAmount"));
                style = cell.GetStyle();
                style.Font.IsBold = true;
                style.VerticalAlignment = TextAlignmentType.Center;
                style.HorizontalAlignment = TextAlignmentType.Right;
                cell.SetStyle(style);

                cell = sheetTrans.Cells["C2"];
                string sum = string.Format("=SUM({0}:{1}{2})", Getcell(colTextsHeader.IndexOf("IN10200TranAmt")) + "5", Getcell(colTextsHeader.IndexOf("IN10200TranAmt")), (dt.Rows.Count + 5).ToString());
                cell.Formula = sum;
                style = cell.GetStyle();
                style.IsLocked = true;
                style.Custom = "#,##0";
                cell.SetStyle(style);


                style = sheetTrans.Cells["A4"].GetStyle();
                style.Font.IsBold = true;
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

                sheetTrans.Cells.Columns[colTextsHeader.IndexOf("IN10200InvtID")].ApplyStyle(style, flag);

                Validation validation = sheetTrans.Validations[sheetTrans.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=MasterData!$AA$2:$AA$" + dt.Rows.Count;
                validation.ShowError = true;
                validation.AlertStyle = ValidationAlertType.Stop;
                validation.ErrorTitle = "Error";
                validation.InputMessage = Util.GetLang("IN10200ChooseInvt");
                validation.ErrorMessage = Util.GetLang("IN10200NotExistsInvt");

                CellArea area;
                area.StartRow = 4;
                area.EndRow = dt.Rows.Count * 2 + 4;
                area.StartColumn = 0;
                area.EndColumn = 0;

                validation.AddArea(area);
                try
                {
                    string formulaInventory = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AD,2,0)),\"\",VLOOKUP({0},MasterData!$AA$1:$AD${1},2,0))", Getcell(colTextsHeader.IndexOf("IN10200InvtID")) + "5", dt.Rows.Count.ToString());
                    sheetTrans.Cells[4, colTextsHeader.IndexOf("Descr")].SetSharedFormula(formulaInventory, dt.Rows.Count * 2, 1);

                    string formulaUnitInventory = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AD,3,0)),\"\",VLOOKUP({0},MasterData!$AA:$AD,3,0))", Getcell(colTextsHeader.IndexOf("IN10200InvtID")) + "5");
                    sheetTrans.Cells[Getcell(colTextsHeader.IndexOf("IN10200Unit")) + "5"].SetSharedFormula(formulaUnitInventory, dt.Rows.Count * 2, 1);


                    string formulaPriceInventory = string.Format("=IF({1}<>\"\",IF(ISERROR(VLOOKUP({0},MasterData!$AA:$AD,4,0)),\"\",VLOOKUP({0},MasterData!$AA:$AD,4,0)),\"\")", Getcell(colTextsHeader.IndexOf("IN10200InvtID")) + "5", Getcell(colTextsHeader.IndexOf("IN10200Unit")) + "5");
                    sheetTrans.Cells[Getcell(colTextsHeader.IndexOf("Price")) + "5"].SetSharedFormula(formulaPriceInventory, dt.Rows.Count * 2, 1);

                    string formulaTranAmt = string.Format("=IF(ISERROR({0}*{1}),\"\",{0}*{1})", Getcell(colTextsHeader.IndexOf("Qty")) + "5", Getcell(colTextsHeader.IndexOf("Price")) + "5");
                    sheetTrans.Cells[Getcell(colTextsHeader.IndexOf("IN10200TranAmt")) + "5"].SetSharedFormula(formulaTranAmt, dt.Rows.Count * 2, 1);

                    //Site
                    string formulaSiteID = string.Format("=MasterData!$T${0}:$T$" + (dtSite.Rows.Count), colTextsHeader.IndexOf("IN10200SiteID"));
                    validation = GetValidation(ref sheetTrans, formulaSiteID, Util.GetLang("IN10200ChooseSite"), Util.GetLang("IN10200NotExistsSite"));
                    validation.AddArea(GetCellArea(4, dtSite.Rows.Count + 100, 2));

                    //SiteLocation
                    //string formulaSiteLocationID = "=Details!$AJ$2:$AJ$" + (dtSiteLocation.Rows.Count + 2);
                    string formulaCustomer3 = "";
                    if (showWhseLoc != 0)
                    {
                        string formulaSiteLocationID = string.Format("=OFFSET(MasterData!$AJ$1,IFERROR(MATCH(C{0},MasterData!$AK${3}:$AK${1},0),{2}),0, IF(COUNTIF(MasterData!$AK${3}:$AK${1},C{0})=0,1,COUNTIF(MasterData!$AK${3}:$AK${1},C{0})),1)",
                        new string[] { "2", (dtSiteLocation.Rows.Count + 100).ToString(), (dtSiteLocation.Rows.Count + 64).ToString(), colTextsHeader.IndexOf("IN10200SiteID").ToString() });

                        validation = GetValidation(ref sheetTrans, formulaSiteLocationID, Util.GetLang("IN10200ChooseWhseLoc"), Util.GetLang("IN10200NotExtWhseLoc"));
                        validation.AddArea(GetCellArea(1, dtSiteLocation.Rows.Count + 100, colTextsHeader.IndexOf("WhseLoc")));

                        formulaCustomer3 = string.Format("=OFFSET(MasterData!$AO$1,IFERROR(MATCH(A{0}&C{0}&D{0},MasterData!$AR$2:$AR${1},0),{2}),0, IF(COUNTIF(MasterData!$AR$2:$AR${1},A{0}&C{0}&D{0})=0,1,COUNTIF(MasterData!$AR$2:$AR${1},A{0}&C{0}&D{0})),1)",
                        new string[] { "2", (dtLotSerNbr.Rows.Count + 1).ToString(), (dtLotSerNbr.Rows.Count + 1).ToString(), Getcell(colTextsHeader.IndexOf("WhseLoc")) });
                    }
                    else
                    {
                        formulaCustomer3 = string.Format("=OFFSET(MasterData!$AO$1,IFERROR(MATCH(A{0}&C{0},MasterData!$AR$2:$AR${1},0),{2}),0, IF(COUNTIF(MasterData!$AR$2:$AR${1},A{0}&C{0})=0,1,COUNTIF(MasterData!$AR$2:$AR${1},A{0}&C{0})),1)",
                       new string[] { "2", (dtLotSerNbr.Rows.Count + 100).ToString(), (dtLotSerNbr.Rows.Count + 195).ToString() });
                    }

                    validation = GetValidation(ref sheetTrans, formulaCustomer3, Util.GetLang("IN10200ChooseLot"), Util.GetLang("IN10200NotExtLot"));
                    validation.AddArea(GetCellArea(1, dtLotSerNbr.Rows.Count + 100, colTextsHeader.IndexOf("LotSerNbr")));

                    if (showPackageID)
                    {
                        string formulaInventory1 = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AO:$AR,2,0)),\"\",VLOOKUP({0},MasterData!$AO$1:$AR${1},2,0))", Getcell(colTextsHeader.IndexOf("LotSerNbr")) + "5", dt.Rows.Count.ToString());
                        sheetTrans.Cells[4, colTextsHeader.IndexOf("IN10200PackageID")].SetSharedFormula(formulaInventory1, dt.Rows.Count * 2, 1);
                    }

                    formulaInventory = string.Format("=IF(ISERROR(VLOOKUP({0},MasterData!$AO:$AR,3,0)),\"\",VLOOKUP({0},MasterData!$AO$1:$AR${1},3,0))", Getcell(colTextsHeader.IndexOf("LotSerNbr")) + "5", dt.Rows.Count.ToString());
                    sheetTrans.Cells[4, colTextsHeader.IndexOf("IN10200DateEnt")].SetSharedFormula(formulaInventory, dt.Rows.Count * 2, 1);
                }
                catch (Exception)
                {

                }

                style = sheetTrans.Cells[Getcell(colTextsHeader.IndexOf("IN10200TranAmt")) + "5"].GetStyle();
                style.Custom = "#,##0";
                Range range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("IN10200TranAmt")) + "5", Getcell(colTextsHeader.IndexOf("IN10200TranAmt")) + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);
                range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("Price")) + "5", Getcell(colTextsHeader.IndexOf("Price")) + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style = sheetTrans.Cells[Getcell(colTextsHeader.IndexOf("IN10200InvtID")) + "5"].GetStyle();
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("IN10200InvtID")) + "5", Getcell(colTextsHeader.IndexOf("IN10200InvtID")) + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);


                style = sheetTrans.Cells[Getcell(colTextsHeader.IndexOf("IN10200SiteID")) + "5"].GetStyle();
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("IN10200SiteID")) + "5", Getcell(colTextsHeader.IndexOf("IN10200SiteID")) + (dtSite.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                if (showWhseLoc != 0)
                {
                    style = sheetTrans.Cells[Getcell(colTextsHeader.IndexOf("WhseLoc")) + "5"].GetStyle();
                    style.IsLocked = false;

                    range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("WhseLoc")) + "5", Getcell(colTextsHeader.IndexOf("WhseLoc")) + (dt.Rows.Count * 2 + 5));
                    range.ApplyStyle(style, flag);
                }

                style.Number = 49;
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("IN10200Unit")) + "5", Getcell(colTextsHeader.IndexOf("IN10200Unit")) + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                if (showPackageID)
                {
                    style.Number = 2;
                }
                else
                {
                    style.Number = 1;
                }
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("Qty")) + "5", Getcell(colTextsHeader.IndexOf("Qty")) + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);

                style.Number = 49;
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange(Getcell(colTextsHeader.IndexOf("LotSerNbr")) + "5", Getcell(colTextsHeader.IndexOf("LotSerNbr")) + (dt.Rows.Count * 2 + 5));
                range.ApplyStyle(style, flag);



                range = sheetTrans.Cells.CreateRange("I5", "I" + (dt.Rows.Count * 2 + 5));
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
                if (showWhseLoc == 0)
                {
                    foreach (var item in lstDetail)
                    {
                        var invt = _app.IN10200_pdInventory(item.InvtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                        if (invt != null && invt.LotSerTrack == "L")
                        {
                            var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == item.BranchID && p.BatNbr == item.BatNbr && p.INTranLineRef == item.LineRef).ToList();
                            foreach (var item2 in lstLot)
                            {
                                sheetTrans.Cells["A" + row].PutValue(item.InvtID);
                                sheetTrans.Cells["E" + row].PutValue(item2.Qty);
                                sheetTrans.Cells["D" + row].PutValue(item2.UnitDesc);
                                sheetTrans.Cells["H" + row].PutValue(item2.LotSerNbr);
                                sheetTrans.Cells["I" + row].PutValue(item2.ExpDate.ToString("yyyy/MM/dd"));
                                row++;
                            }
                        }
                        else
                        {
                            sheetTrans.Cells["A" + row].PutValue(item.InvtID);
                            sheetTrans.Cells["E" + row].PutValue(item.Qty);
                            sheetTrans.Cells["D" + row].PutValue(item.UnitDesc);
                            sheetTrans.Cells["H" + row].PutValue("");
                            sheetTrans.Cells["I" + row].PutValue("");
                            row++;
                        }
                    }
                }
                else
                {
                    foreach (var item in lstDetail)
                    {
                        var invt = _app.IN10200_pdInventory(item.InvtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
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
                }

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = (data["BatNbr"].PassNull() == "" ? "IN10200" : data["BatNbr"].PassNull()) + ".xlsx" };
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
                bool showPackageID = false;
                int showWhseLoc = 0;
                var config = _app.IN10200_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (config != null)
                {
                    isChangeSite = config.IsChangeSite ?? false;
                    showWhseLoc = config.ShowWhseLoc.ToInt();
                }
                var lstUnit = _app.IN10200_piUnit(Current.CpnyID, Current.UserName, Current.LangID).ToList();
                var lstInventory = _app.IN10200_piInventory(branchID,Current.UserName, Current.CpnyID, Current.LangID).ToList();
                var lstSite = _app.IN10200_peSiteID(branchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                var lstSiteLocation = _app.IN10200_peSiteLocation(branchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                var lstLotSerNbr = _app.IN10200_piLotSerNbr(branchID, Current.UserName, Current.CpnyID, Current.LangID).ToList();

                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    try
                    {
                        bool check = false;
                        var lstInvtID = new List<IN10200_piInventory_Result>();

                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        int lineRef = data["lineRef"].ToInt();
                        string whseLoc = data["cboWhseLoc"].PassNull();
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];
                            var colTextsHeader = GetHeader(false, showWhseLoc);

                            bool checkHeader = false;
                            for (int i = 0; i < colTextsHeader.Count; i++)
                            {
                                if (workSheet.Cells[3, i].StringValue.ToUpper().Trim() != Util.GetLang(colTextsHeader[i]).ToUpper().Trim())
                                {
                                    checkHeader = true;
                                    break;
                                }
                            }
                            if (checkHeader)
                            {
                                throw new MessageException(MessageType.Message, "148");
                            }

                            if (showWhseLoc == 0 && workSheet.Cells[3, 9].StringValue.Trim().PassNull()!="")
                            {
                                throw new MessageException(MessageType.Message, "148");
                            }

                            for (int i = 4; i <= workSheet.Cells.MaxDataRow; i++)
                            {
                                string invtIDImport = workSheet.Cells[i, colTextsHeader.IndexOf("IN10200InvtID")].StringValue.PassNull();
                                if (invtIDImport == string.Empty)
                                {
                                    continue;
                                }
                                string descrImport = workSheet.Cells[i, colTextsHeader.IndexOf("Descr")].StringValue.PassNull();
                                string siteIDImport = workSheet.Cells[i, colTextsHeader.IndexOf("IN10200SiteID")].StringValue.PassNull();
                                string whseLocImport = colTextsHeader.IndexOf("WhseLoc") != -1 ? workSheet.Cells[i, colTextsHeader.IndexOf("WhseLoc")].StringValue.PassNull() : "";
                                string unitDescImport = workSheet.Cells[i, colTextsHeader.IndexOf("IN10200Unit")].StringValue.PassNull();
                                string qtyImport = workSheet.Cells[i, colTextsHeader.IndexOf("Qty")].StringValue.PassNull();
                                string priceImport = workSheet.Cells[i, colTextsHeader.IndexOf("Price")].StringValue.PassNull();
                                string tranAmtImport = workSheet.Cells[i, colTextsHeader.IndexOf("IN10200TranAmt")].StringValue.PassNull();
                                string lotSerNbrImport = workSheet.Cells[i, colTextsHeader.IndexOf("LotSerNbr")].StringValue.PassNull();
                                string dateEntImport = workSheet.Cells[i, colTextsHeader.IndexOf("IN10200DateEnt")].StringValue.PassNull();

                                var objInvt = lstInventory.FirstOrDefault(p => p.InvtID.ToUpper() == invtIDImport.ToUpper());
                                if (objInvt == null)
                                {
                                    message += string.Format(Message.GetString("2018091913", null), (i + 1).ToString(), invtIDImport);                                    
                                    continue;
                                }
                                else
                                {
                                    invtIDImport = objInvt.InvtID;
                                }

                                if (lstInvtID.All(x => x.InvtID != invtIDImport))
                                {
                                    lstInvtID.Add(objInvt);
                                }

                                if (string.IsNullOrEmpty(siteIDImport))
                                {
                                    message += string.Format(Message.GetString("2018091914", null), (i + 1).ToString(), invtIDImport);
                                    continue;
                                }
                                else if (isChangeSite)
                                {
                                    var objSiteID = lstSite.FirstOrDefault(x => x.SiteID.ToLower() == siteIDImport.ToLower());
                                    if (objSiteID == null)
                                    {
                                        message += string.Format(Message.GetString("2018091915", null), (i + 1).ToString(), invtIDImport);
                                        continue;
                                    }
                                    else
                                    {
                                        siteIDImport = objSiteID.SiteID;
                                    }
                                }
                                else
                                {
                                    if (siteIDImport.ToLower() != data["SiteID"].PassNull().ToLower())
                                    {
                                        message += string.Format(Message.GetString("2018091916", null), (i + 1).ToString(), invtIDImport);
                                        continue;
                                    }
                                    else
                                    {
                                        siteIDImport = data["SiteID"].PassNull();
                                    }
                                }

                                if (showWhseLoc != 0)
                                {
                                    if (string.IsNullOrEmpty(whseLocImport))
                                    {
                                        message += string.Format(Message.GetString("2018091917", null), (i + 1).ToString(), invtIDImport);
                                        continue;
                                    }
                                    else if (isChangeSite)
                                    {
                                        var objWhseLoc = lstSiteLocation.FirstOrDefault(x => x.SiteID.ToLower() == siteIDImport.ToLower() && x.WhseLoc.ToLower() == whseLocImport.ToLower());
                                        if (objWhseLoc == null)
                                        {
                                            message += string.Format(Message.GetString("2018091918", null), (i + 1).ToString(), invtIDImport);
                                            continue;
                                        }
                                        else
                                        {
                                            whseLocImport = objWhseLoc.WhseLoc;
                                        }
                                    }
                                    else
                                    {
                                        if (whseLocImport.ToLower() != whseLoc.ToLower())
                                        {
                                            message += string.Format(Message.GetString("2018091919", null), (i + 1).ToString(), invtIDImport);
                                            continue;
                                        }
                                        else
                                        {
                                            whseLocImport = whseLoc;
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(unitDescImport))
                                {
                                    message += string.Format(Message.GetString("2018091920", null), (i + 1).ToString(), invtIDImport);
                                    continue;
                                }

                                else
                                {
                                    var objUnit = lstUnit.FirstOrDefault(p => p.InvtID == invtIDImport && p.Unit.ToUpper() == unitDescImport.ToUpper());
                                    if (objUnit == null)
                                    {
                                        message += string.Format(Message.GetString("2018091921", null), (i + 1).ToString(), invtIDImport);
                                        continue;
                                    }
                                    else
                                    {
                                        unitDescImport = objUnit.Unit;
                                    }
                                }

                                if (string.IsNullOrEmpty(qtyImport))
                                {
                                    message += string.Format(Message.GetString("2018091922", null), (i + 1).ToString(), invtIDImport);
                                    continue;
                                }
                                else
                                {
                                    float n;
                                    bool isNumeric = float.TryParse(qtyImport, out n);
                                    if (isNumeric == true)
                                    {
                                        if (qtyImport.ToDouble() == 0)
                                        {
                                            message += string.Format(Message.GetString("2018092011", null), (i + 1).ToString(), invtIDImport);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        message += string.Format(Message.GetString("2018092012", null), (i + 1).ToString());
                                        continue;
                                    }
                                }

                                if ((objInvt.LotSerTrack == "L" || objInvt.LotSerTrack == "Q") && lotSerNbrImport == "")
                                {
                                    message += string.Format(Message.GetString("2018092013", null), (i + 1).ToString(), invtIDImport);                                    
                                    continue;
                                }

                                if ((objInvt.LotSerTrack == "L" || objInvt.LotSerTrack == "Q") && lotSerNbrImport.Length > 50)
                                {
                                    message += string.Format(Message.GetString("2018092014", null), (i + 1).ToString(), invtIDImport);
                                    continue;
                                }

                                var objLot = lstLotSerNbr.FirstOrDefault(p => p.InvtID == invtIDImport && p.LotSerNbr.ToUpper() == lotSerNbrImport.ToUpper() && p.SiteID == siteIDImport && p.WhseLoc==whseLocImport);

                                if ((objInvt.LotSerTrack != "N") && objLot == null)
                                {
                                    message += string.Format(Message.GetString("2018092015", null), (i + 1).ToString(), invtIDImport);
                                    continue;
                                }

                                if (objLot != null)
                                {
                                    dateEntImport = objLot.WarrantyDate;
                                    lotSerNbrImport = objLot.LotSerNbr;
                                }

                                if (objInvt.LotSerTrack != "N")
                                {
                                    if (lstLot.Any(p => p.InvtID == invtIDImport && p.LotSerNbr == lotSerNbrImport && p.SiteID == siteIDImport && p.WhseLoc == whseLocImport) && showWhseLoc!=0)
                                    {
                                        message += string.Format(Message.GetString("2018092016", null), (i + 1).ToString(), invtIDImport, siteIDImport, whseLocImport, lotSerNbrImport);
                                        continue;
                                    }
                                    else
                                    {
                                        if (lstLot.Any(p => p.InvtID == invtIDImport && p.LotSerNbr == lotSerNbrImport && p.SiteID == siteIDImport ) && showWhseLoc == 0)
                                        {
                                            message += string.Format(Message.GetString("2018092017", null), (i + 1).ToString(), invtIDImport, siteIDImport, lotSerNbrImport);
                                            continue;
                                        }
                                    }
                                }

                                double qty = qtyImport == "" ? 0 : Math.Round(qtyImport.ToDouble(), 0);


                                var newLot = new IN_LotTransExt();

                                workSheet.Cells[i, 4].Calculate(true, null);
                                workSheet.Cells[i, 6].Calculate(true, null);

                                newLot.CnvFact = 1;
                                if (objInvt.LotSerTrack == "L" || objInvt.LotSerTrack == "Q")
                                {
                                    string[] strExpDate = dateEntImport.Split('/');
                                    DateTime dExpDate = new DateTime(int.Parse(strExpDate[0]), int.Parse(strExpDate[1]), int.Parse(strExpDate[2]));

                                    newLot.LotSerNbr = lotSerNbrImport;
                                    var item = _app.IN_ItemLot.FirstOrDefault(p => p.InvtID == invtIDImport && p.LotSerNbr == newLot.LotSerNbr && p.SiteID==siteIDImport && p.WhseLoc== whseLocImport);
                                    if (item != null)
                                    {
                                        newLot.ExpDate = item.ExpDate;
                                        newLot.WarrantyDate = item.WarrantyDate;
                                        check = true;
                                        if (item.QtyAvail<qty)
                                        {                                           
                                            if (showWhseLoc != 0)
                                            {
                                                message += string.Format(Message.GetString("2018092018", null), (i + 1).ToString(), invtIDImport, siteIDImport, whseLocImport, lotSerNbrImport);
                                                continue;
                                            }
                                            else
                                            {
                                                message += string.Format(Message.GetString("2018092019", null), (i + 1).ToString(), invtIDImport, siteIDImport, lotSerNbrImport);
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        newLot.ExpDate = dExpDate;
                                        newLot.WarrantyDate = ("1-1-1900").ToDateShort();
                                    }                                    
                                }
                                else
                                {                                    
                                    if (showWhseLoc != 0)
                                    {
                                        var objLoc = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtIDImport && p.SiteID == siteIDImport && p.WhseLoc == whseLocImport);
                                        if (objLoc == null)
                                        {
                                            message += string.Format(Message.GetString("2018092024", null), (i + 1).ToString(), invtIDImport, siteIDImport, whseLocImport);
                                            continue;
                                        }
                                        else
                                        {
                                            if (objLoc.QtyAvail < qty)
                                            {
                                                message += string.Format(Message.GetString("2018092020", null), (i + 1).ToString(), invtIDImport, siteIDImport, whseLocImport);
                                                continue;
                                            } 
                                        }                                                                               
                                    }
                                    else
                                    {
                                        var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtIDImport && p.SiteID == siteIDImport );
                                        if(objSite==null)
                                        {
                                            message += string.Format(Message.GetString("2018092023", null), (i + 1).ToString(), invtIDImport, siteIDImport);
                                            continue;
                                        }                                        
                                        else{
                                            if (objSite.QtyAvail < qty)
                                            {
                                                message += string.Format(Message.GetString("2018092021", null), (i + 1).ToString(), invtIDImport, siteIDImport);
                                                continue;
                                            }
                                        }                                        
                                    }                                    
                                }
                                
                                newLot.InvtID = invtIDImport;
                                newLot.InvtMult = 1;
                                newLot.Qty = qtyImport == "" ? 0 : Math.Round(qtyImport.ToDouble(), 0);

                                newLot.SiteID = siteIDImport; //data["SiteID"].PassNull();
                                newLot.WhseLoc = whseLocImport;//whseLoc;
                                newLot.TranDate = data["DateEnt"].ToDateShort();
                                newLot.TranType = "RC";
                                newLot.UnitDesc = unitDescImport;
                                newLot.UnitMultDiv = "M";
                                
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
                                newLot.UnitCost = newLot.UnitPrice = _app.IN10200_pdPrice("", invtIDImport, newLot.UnitDesc, newLot.TranDate, objInvt.ValMthd, newLot.SiteID).FirstOrDefault().Price.Value;
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
                        //var lstInvt = lstLot.GroupBy(x => new { x.InvtID, x.SiteID, x.WhseLoc }).Select(x => x.FirstOrDefault()).ToList();
                        //List<IN_LotTransExt> lstLot = new List<IN_LotTransExt>();
                        var lst = new List<IN_LotTransExt>();
                        foreach (var item in lstLot)
                        {
                            if (!lst.Any(x => x.InvtID == item.InvtID && x.SiteID == item.SiteID && x.WhseLoc == item.WhseLoc))
                            {
                                lst.Add(item);
                            }
                        }
                        foreach (var item in lst)
                        {
                            var objInvt1 = _app.IN10200_piInventory(branchID,Current.UserName, Current.CpnyID, Current.LangID).Where(p => p.InvtID == item.InvtID).FirstOrDefault();
                            var objInvt = lstInvtID.FirstOrDefault(p => p.InvtID.ToUpper() == item.InvtID.ToUpper());// _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                            var newTrans = new IN10200_pgIssueLoad_Result();
                            newTrans.InvtID = item.InvtID;
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

                            var tmp = lstLot.Where(p => p.InvtID == item.InvtID && p.SiteID == item.SiteID && p.WhseLoc == item.WhseLoc).ToList();
                            foreach (var lot in tmp)
                            {
                                newTrans.Qty += Math.Round(lot.Qty, 0);
                                lot.INTranLineRef = newTrans.LineRef;
                                if (objInvt.LotSerTrack != "L" && objInvt.LotSerTrack != "Q")
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
                        
                        if (lstTrans.Count == 0 && message.PassNull()=="")
                        {
                            message += string.Format(Message.GetString("2018092022", null));
                        }
                        //if (check)
                        //{
                        //    Util.AppendLog(ref _logMessage, "123", "", data: new { message, lstTrans, lstLot });
                        //}
                        //else
                        //{
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message, lstTrans, lstLot });
                        //}

                        //if (message == "" || message == string.Empty)
                        //{
                        //    _db.SaveChanges();
                        //}
                        //Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });

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
            var objSite = _app.IN10200_pdGetItemSite(showWhseLoc, whseLoc, Current.CpnyID, Current.UserName, Current.LangID);
           return this.Store(objSite);    
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
                        if (lotDB != null)
                        {
                            lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                            lstLot.Add(lotDB);
                        }                        
                    }
                    else
                    {
                        var lotDB = _app.IN10200_pdGetLot(3, siteID, invtID, whseLoc, item.LotSerNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                        if (lotDB != null)
                        {
                            lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                            lstLot.Add(lotDB);
                        }                        
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

        public ActionResult GetGetWhseLocMax()
        {
            var objSetup = _app.IN10200_pdGetWhseLocMax(Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(objSetup);
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
                        UpdateAllocLot_WhseLoc(item.WhseLoc,item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
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
                    Update_IN_ItemLoc(t.WhseLoc, t.InvtID, t.SiteID, oldQty, 0);
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
            t.SlsperID = _form["SlsperID"].PassNull(); // s.SlsperID;
            
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
                    UpdateAllocLot_WhseLoc(t.WhseLoc, t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot_WhseLoc(s.WhseLoc, s.InvtID, s.SiteID, s.LotSerNbr, 0, newQty, 0))
                    {
                        throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                    }
                }
                else
                {
                    UpdateAllocLot(t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot(s.InvtID, s.SiteID, s.LotSerNbr, 0, newQty, 0))
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
            t.WhseLoc = s.WhseLoc.PassNull();
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
                        throw new MessageException(MessageType.Message, "2018051411", "", new string[] { invtID, siteID,whseLoc });
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

        private List<string> GetHeader(bool showPackageID, int showWhseLoc)
        {
            var colTextsHeader = new List<string>() { "IN10200InvtID", "Descr", "IN10200SiteID", "WhseLoc", "IN10200Unit", "Qty", "Price", "IN10200TranAmt", "LotSerNbr", "IN10200DateEnt", "IN10200PackageID" };

            if (showWhseLoc == 0)
            {
                colTextsHeader.Remove("WhseLoc");
            }

            if (!showPackageID)
            {
                colTextsHeader.Remove("IN10200PackageID");
            }
            return colTextsHeader;

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

        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle, bool isBackground = false)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            style.IsTextWrapped = true;
            //style.ForegroundColor = Color.Red;
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
            c.SetStyle(style);
        }
      
    }
}

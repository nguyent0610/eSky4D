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
using System.Xml.Linq;
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
using HQFramework.Common;
namespace IN10100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10100Controller : Controller
    {
        private string _screenNbr = "IN10100";
        private string _userName = Current.UserName;
        private string _handle = "";
        private IN10100Entities _app = Util.CreateObjectContext<IN10100Entities>();
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);     
        private FormCollection _form;
        private IN10100_pcBatch_Result _objBatch;
        private JsonResult _logMessage;
        private List<IN10100_pgReceiptLoad_Result> _lstTrans;
        private IN_Setup _objIN;
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
        
        public ActionResult GetBatch(string branchID, string query, int start, int limit, int page)
        {
            query = query ?? string.Empty;
            if (page != 1) query = string.Empty;
            var lstBatch = _app.IN10100_pcBatch(branchID, query, start, start +20).ToList();
            var paging = new Paging<IN10100_pcBatch_Result>(lstBatch, lstBatch.Count>0 ? lstBatch[0].TotalRecords.Value:0);
            return this.Store(paging.Data, paging.TotalRecords);
        }
        public ActionResult GetTrans(string batNbr, string branchID)
        {
            var lstTrans = _app.IN10100_pgReceiptLoad(batNbr, branchID, "%", "%").ToList();
            return this.Store(lstTrans);
        }
        public ActionResult GetTransfer(string branchID, DateTime tranDate, string trnsfrDocNbr)
        {
            var lstTransfer = _app.IN10100_pdTrnsfer( branchID,trnsfrDocNbr,tranDate).ToList();
            return this.Store(lstTransfer);
        }
        public ActionResult GetItemSite(string invtID, string siteID)
        {
            var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
            return this.Store(objSite);
        }
        public ActionResult GetUnitConversion()
        {
            var lstUnit = _app.IN10100_pcUnitConversion(Current.CpnyID).ToList();
            return this.Store(lstUnit);
        }
        public ActionResult GetPrice(string invtID, string uom, DateTime effDate)
        {
            var lstPrice = _app.IN10100_pdPrice("",invtID,uom,DateTime.Now).ToList();
            return this.Store(lstPrice);
        }
        public ActionResult GetUnit(string invtID)
        {
            IN_Inventory invt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (invt == null) invt = new IN_Inventory();
            List<IN10100_pcUnit_Result> lstUnit = _app.IN10100_pcUnit(invt.ClassID, invt.InvtID).ToList();
            return this.Store(lstUnit, lstUnit.Count);
        }
        public ActionResult GetSetup()
        {
            string cpnyID = Current.CpnyID;
            var objSetup = _app.IN_Setup.FirstOrDefault(p => p.SetupID == "IN" && p.BranchID == cpnyID);
            return this.Store(objSetup);
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
                return Json(new { success = true, data= new {batNbr= _objBatch.BatNbr} });
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
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                var access = Session["IN10100"] as AccessRight;
                if (!access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objBatch = data.ConvertToObject<IN10100_pcBatch_Result>();

                if (_objBatch.Status!="H")
                {
                    throw new MessageException(MessageType.Message, "2015020805","",new string[]{_objBatch.BatNbr});
                }

                var batch = _app.Batches.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr);
                if (batch != null) _app.Batches.DeleteObject(batch);

                var lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                foreach (var trans in lstTrans)
                {
                    _app.IN_Trans.DeleteObject(trans);
                }

                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Json(new { success = true });
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
        public ActionResult DeleteTrans(FormCollection data)
        {
            try
            {
                var access = Session["IN10100"] as AccessRight;
                
                _objBatch = data.ConvertToObject<IN10100_pcBatch_Result>();

                if (_objBatch.Status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }
                if ((_objBatch.BatNbr.PassNull() == string.Empty && !access.Insert) || (_objBatch.BatNbr.PassNull() != string.Empty && !access.Update))
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                string lineRef = Util.PassNull(data["LineRef"]);

                var lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                var trans = lstTrans.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.LineRef == lineRef);
              
                if (trans != null)
                {
                    lstTrans.Remove(trans);
                    _app.IN_Trans.DeleteObject(trans);
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
                if (_objBatch != null)
                {
                    tstamp = _objBatch.tstamp.ToHex();
                }

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Json(new { success = true, tstamp });
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
        private void SaveData(FormCollection data)
        {
            
            var transHandler = new StoreDataHandler(data["lstTrans"]);
            if (_lstTrans == null)
            {
                _lstTrans = transHandler.ObjectData<IN10100_pgReceiptLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }

            _objBatch = data.ConvertToObject<IN10100_pcBatch_Result>();

            _handle = data["Handle"].PassNull();
            _objBatch.Status = _objBatch.Status.PassNull() == string.Empty ? "H" : _objBatch.Status;

            var cfgWrkDateChk = _sys.SYS_CloseDateSetUp.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            if (cfgWrkDateChk != null && cfgWrkDateChk.WrkDateChk)
            {
                DateTime tranDate = _objBatch.DateEnt;
                if (!((DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(-1 * cfgWrkDateChk.WrkLowerDays)) >=
                       0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) <= 0)
                      ||
                      (DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(cfgWrkDateChk.WrkUpperDays)) <=
                       0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) >= 0)
                      || DateTime.Compare(tranDate, cfgWrkDateChk.WrkAdjDate) == 0))
                {
                    throw new MessageException(MessageType.Message, "301");
                }
            }
            Batch batch = _app.Batches.FirstOrDefault(p => p.BatNbr == _objBatch.BatNbr && p.BranchID == _objBatch.BranchID);
            if (( _objBatch.Status == "U" ||  _objBatch.Status == "C") && (_handle == "C" || _handle == "V"))
            {
                if (batch!=null)
                {
                    if(batch.tstamp.ToHex() != data["tstamp"].ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2015020802","",new []{batch.BatNbr});
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
                try
                {
                    INProcess.Inventory inventory = new INProcess.Inventory(_userName, _screenNbr, dal);
                    if (_handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!inventory.IN10100_Release(_objBatch.BranchID, _objBatch.BatNbr, data["isTransfer"].ToBool(), data["isTransfer"].ToBool() ? _lstTrans[0].RefNbr : ""))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                    else if (_handle == "C" || _handle == "V")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!inventory.IN10100_Cancel(_objBatch.BranchID, _objBatch.BatNbr, data["isTransfer"].ToBool(), data["isTransfer"].ToBool() ? _lstTrans[0].RefNbr : "", _handle == "C"))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }
                        Util.AppendLog(ref _logMessage, "9999", "");
                    }
                    inventory = null;
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
            }
        }
        private void CheckData()
        {
            var access = Session[_screenNbr] as AccessRight;

            if((_objBatch.BatNbr.PassNull()!=string.Empty && !access.Update) || (_objBatch.BatNbr.PassNull()==string.Empty && !access.Insert))
            {
                 throw new MessageException(MessageType.Message, "728");
            }

            if(_objBatch.Status.PassNull()!= "H"  && (!access.Update) || (_objBatch.BatNbr.PassNull()==string.Empty && !access.Insert))
            {
                 throw new MessageException(MessageType.Message, "2015020803");
            }

            if(_objBatch.JrnlType == "PO")
            {
                 throw new MessageException(MessageType.Message, "2015020801","",new string[]{_objBatch.BatNbr});
            }
            if (_form["isTransfer"].ToBool())
            {
                string refNbr=_lstTrans[0].RefNbr;
                var transfer = _app.IN_Transfer.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.TrnsfrDocNbr == refNbr);
                if (transfer == null || transfer.Status != "I")
                {
                    throw new MessageException(MessageType.Message, "2015020901","",new []{_lstTrans[0].RefNbr});
                }
            }
            if (_lstTrans.Count == 0)
            {
                throw new MessageException(MessageType.Message, "2015020804", "");
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

            }

            _app.SaveChanges();
        }

        private void Update_Batch(Batch t,bool isNew)
        {
            if (isNew)
            {
                t.ResetET();
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                t.RefNbr = _objBatch.RefNbr;
                t.Module = "IN";

                t.Crtd_Prog  = _screenNbr;
                t.Crtd_User  = _userName;
                t.Crtd_DateTime  = DateTime.Now;
            }
            else
                t.RefNbr = _objBatch.RefNbr;

            t.JrnlType = _objBatch.JrnlType.PassNull() == string.Empty ? "IN" : _objBatch.JrnlType;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.DateEnt = _objBatch.DateEnt.ToDateShort();
            t.Descr = _objBatch.Descr;
            t.EditScrnNbr = _screenNbr;
            t.FromToSiteID = _objBatch.FromToSiteID;
            t.IntRefNbr = _objBatch.IntRefNbr;
            t.NoteID = 0;
            t.RvdBatNbr = _objBatch.RvdBatNbr;
            t.ReasonCD = _objBatch.ReasonCD;
            t.TotAmt = _objBatch.TotAmt;
            t.Rlsed = 0;
            t.Status = _objBatch.Status;
        }
        private void Update_Trans(Batch batch, IN_Trans t, IN10100_pgReceiptLoad_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ResetET();
             
                t.LineRef = s.LineRef;
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                if (_form["TrnsferNbr"].PassNull()!=string.Empty)
                {
                    t.RefNbr = _form["TrnsferNbr"].PassNull();
                }
                else
                {
                    if (_objIN.AutoRefNbr)
                        t.RefNbr = _objBatch.RefNbr;
                    else
                        t.RefNbr = s.RefNbr;
                }

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;


            t.ReasonCD = batch.ReasonCD;
            t.CnvFact = s.CnvFact;
            t.ExtCost = Math.Round(s.ExtCost, 0);
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
            t.SlsperID = _form["SlsperID"].PassNull();
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

        public ActionResult Export(FormCollection data)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet sheetTrans = workbook.Worksheets[0];

                sheetTrans.Name = "Details";

                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(data["BranchID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@EffDate", DbType.DateTime, clsCommon.GetValueDBNull(data["DateEnd"].ToDateShort()), ParameterDirection.Input, 30));
                DataTable dt = dal.ExecDataTable("IN10100_pdImportInventory", CommandType.StoredProcedure, ref pc);

                List<IN10100_pgReceiptLoad_Result> lstDetail = _app.IN10100_pgReceiptLoad(data["BatNbr"].PassNull(), data["BranchID"].PassNull(),"%","%").ToList();

                sheetTrans.Cells.ImportDataTable(dt, false, "AA2");
                
            

                Style style = workbook.GetStyleInPool(0);
                style.Font.Color = Color.Transparent;
                style.IsLocked = true;
                StyleFlag flag = new StyleFlag();
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;

                sheetTrans.Cells.Columns[26].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[27].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[28].ApplyStyle(style, flag);
                sheetTrans.Cells.Columns[29].ApplyStyle(style, flag);


                var cell = sheetTrans.Cells["B1"];
                cell.PutValue("CHI TIẾT NHẬP KHO");
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
                cell.Formula = "=SUM(F5:F" + (dt.Rows.Count + 5).ToString() + ")";
                style = cell.GetStyle();
                style.IsLocked = true;
                style.Custom = "#,##0";
                cell.SetStyle(style);

                style = sheetTrans.Cells["A4"].GetStyle();
                style.Font.IsBold = true;

                sheetTrans.Cells["A4"].PutValue("Mã Mặt Hàng");
                sheetTrans.Cells["B4"].PutValue("Diễn Giải");
                sheetTrans.Cells["C4"].PutValue("Đơn Vị Tính");
                sheetTrans.Cells["D4"].PutValue("Số Lượng");
                sheetTrans.Cells["E4"].PutValue("Giá Bán");
                sheetTrans.Cells["F4"].PutValue("Tổng Tiền");

                sheetTrans.Cells["A4"].SetStyle(style);
                sheetTrans.Cells["B4"].SetStyle(style);
                sheetTrans.Cells["C4"].SetStyle(style);
                sheetTrans.Cells["D4"].SetStyle(style);
                sheetTrans.Cells["E4"].SetStyle(style);
                sheetTrans.Cells["F4"].SetStyle(style);

                style = workbook.GetStyleInPool(0);
                style.Number = 49; //Text
                style.Font.Color = Color.Black;
                sheetTrans.Cells.Columns[0].ApplyStyle(style, flag);

                Validation validation = sheetTrans.Validations[sheetTrans.Validations.Add()];
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.Operator = OperatorType.Between;
                validation.InCellDropDown = true;
                validation.Formula1 = "=$AA2:$AA" + dt.Rows.Count + 2;
                validation.ShowError = true;
                validation.AlertStyle = ValidationAlertType.Stop;
                validation.ErrorTitle = "Error";
                validation.InputMessage = "Chọn mã mặt hàng";
                validation.ErrorMessage = "Mã mặt hàng này không tồn tại";

                CellArea area;
                area.StartRow = 4;
                area.EndRow = dt.Rows.Count + 4;
                area.StartColumn = 0;
                area.EndColumn = 0;

                validation.AddArea(area);

                string formulaInventory = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AD,2,0)),\"\",VLOOKUP({0},AA:AD,2,0))", "A5");
                sheetTrans.Cells["B5"].SetSharedFormula(formulaInventory, dt.Rows.Count, 1);

                string formulaUnitInventory = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AD,3,0)),\"\",VLOOKUP({0},AA:AD,3,0))", "A5");
                sheetTrans.Cells["C5"].SetSharedFormula(formulaUnitInventory, dt.Rows.Count, 1);


                string formulaPriceInventory = string.Format("=IF(C5<>\"\",IF(ISERROR(VLOOKUP({0},AA:AD,4,0)),\"\",VLOOKUP({0},AA:AD,4,0)),\"\")", "A5");
                sheetTrans.Cells["E5"].SetSharedFormula(formulaPriceInventory, dt.Rows.Count, 1);

                sheetTrans.Cells["F5"].SetSharedFormula("=IF(ISERROR(D5*E5),\"\",D5*E5)", dt.Rows.Count, 1);

                style = sheetTrans.Cells["F5"].GetStyle();
                style.Custom = "#,##0";
                Range range = sheetTrans.Cells.CreateRange("F5", "F" + (dt.Rows.Count + 5));
                range.ApplyStyle(style, flag);
                range = sheetTrans.Cells.CreateRange("E5", "E" + (dt.Rows.Count + 5));
                range.ApplyStyle(style, flag);

                style = sheetTrans.Cells["A5"].GetStyle();
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("A5", "A" + (dt.Rows.Count + 5));
                range.ApplyStyle(style, flag);

                style = sheetTrans.Cells["D5"].GetStyle();
                style.Custom = "#,##0";
                style.IsLocked = false;

                range = sheetTrans.Cells.CreateRange("D5", "D" + (dt.Rows.Count + 5));
                range.ApplyStyle(style, flag);



                sheetTrans.AutoFitColumns();

                sheetTrans.Cells.Columns[1].Width = 30;
                sheetTrans.Cells.Columns[2].Width = 15;
                sheetTrans.Cells.Columns[4].Width = 15;
                sheetTrans.Cells.Columns[5].Width = 15;
                sheetTrans.Protect(ProtectionType.All);

                int row = 5;
                foreach (var item in lstDetail)
                {
                    sheetTrans.Cells["A" + row].PutValue(item.InvtID);
                    sheetTrans.Cells["D" + row].PutValue(item.Qty);
                    row++;
                }

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = data["BatNbr"].PassNull() + ".xlsx" };
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

        public ActionResult Import(FormCollection data)
        {
            try
            {
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile; // or: HttpPostedFileBase file = this.HttpContext.Request.Files[0];
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
                List<object> lstTrans = new List<object>();
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        if (workbook.Worksheets.Count > 0)
                        {
                            Worksheet workSheet = workbook.Worksheets[0];
                            string invtID = string.Empty;
                            int lineRef = 1;
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
                                if (workSheet.Cells[i, 3].FloatValue == 0)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} chưa nhập số lượng<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }

                                var newTrans = new IN10100_pgReceiptLoad_Result();
                                newTrans.InvtID = invtID;
                                newTrans.LineRef = LastLineRef(lineRef);
                                newTrans.ReasonCD = data["ReasonCD"].PassNull();

                                workSheet.Cells[i, 2].Calculate(true, null);

                                newTrans.UnitDesc = workSheet.Cells[i, 2].StringValue;
                                newTrans.CnvFact = 1;
                                newTrans.UnitMultDiv = "M";
                                newTrans.InvtMult = 1;
                                newTrans.TranType = "RC";
                                newTrans.JrnlType = "IN";
                                newTrans.TranDesc = objInvt.Descr;
                                newTrans.Qty = workSheet.Cells[i, 3].FloatValue;

                                workSheet.Cells[i, 4].Calculate(true, null);

                                newTrans.UnitPrice = Math.Round(workSheet.Cells[i, 4].FloatValue, 0);
                                newTrans.TranAmt = Math.Round(newTrans.UnitPrice * newTrans.Qty, 0);
                                newTrans.SiteID = data["SiteID"].PassNull();
                                lstTrans.Add(newTrans);
                                lineRef++;

                            }
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message, lstTrans });
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

        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                _form = data;
                _objBatch = data.ConvertToObject<IN10100_pcBatch_Result>();
                User user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
                var rpt = new RPTRunning();
                rpt.ResetET();

                rpt.ReportNbr = "IN602";
                rpt.MachineName = "Web";
                rpt.ReportCap = "IN_Receipt";
                rpt.ReportName = "IN_Receipt";
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
    }
}

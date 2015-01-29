using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using eBiz4DWebFrame;
using eBiz4DWebSys;
using System.Xml;
using System.Xml.Linq;
using System.Data.Objects;

using System.IO;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Runtime.InteropServices;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
//using EasyXLS;
using System.Data;
using System.IO;
using System.Drawing;
namespace IN10100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10100Controller : Controller
    {
        string screenNbr = "IN10100";
        IN10100Entities _db = Util.CreateObjectContext<IN10100Entities>(false);
        eBiz4DWebSysEntities _sys = Util.CreateObjectContext<eBiz4DWebSysEntities>(true);

    

        public ActionResult Index()
        {
            return View();//.OrderBy(x => x.tstamp)
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {

            return PartialView();
        }

        public ActionResult GetDataForm(String branchID, String batNbr)
        {
            var rptCtrl = _db.ppv_INReceiptBatch(branchID).ToList();
            var headerRecord = rptCtrl.Where(p => p.BatNbr == batNbr).FirstOrDefault();
            return this.Store(headerRecord);
        }

        public ActionResult GetDataGrid(String batNbr, String branchID)
        {
            
            var recordGrid = _db.ppv_IN10100_ReceiptLoad(batNbr, branchID, "%", "%").ToList();

            return this.Store(recordGrid);
        }
        //public ActionResult GetData(String userID, String branchID, String areaCode, String state,String dsm,
        //    String fromDate,String toDate,String questionType,String questionCode)
        //{
        //    //string[] words = month.Split('-');
        //    //var month1 = Convert.ToInt32(words[1]);
        //    ////string[] words2 = Regex.Split(words[0], "\"");
        //    //var year1 = Convert.ToInt32(words[0]);
        //    //var lst = _db.ppv_IN10100_LoadGrid(month1, year1, dsm).ToList();
        //    var lst = _db.ppv_IN10100_LoadGrid(userID, branchID, areaCode, state, dsm, Convert.ToDateTime(fromDate).Short(), Convert.ToDateTime(toDate).Short(), questionType, questionCode).ToList();


        //    return this.Store(lst);
        //}
        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string branchID,string handle,string batNbr)
        {
            string mailidNew = DateTime.Now.ToString("yyyyMMddhhmmssff");
            StoreDataHandler dataHeader = new StoreDataHandler(data["lstheader"]);
            ChangeRecords<ppv_INReceiptBatch_Result> lstheader = dataHeader.BatchObjectData<ppv_INReceiptBatch_Result>();
            StoreDataHandler dataGrid = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<ppv_IN10100_ReceiptLoad_Result> lstgrd = dataGrid.BatchObjectData<ppv_IN10100_ReceiptLoad_Result>();

            var storeGrid = this.GetCmp<Store>("storeGrid");




            //var tmpGridChangeOrNot = 0;
            var batNbrIfNull = "";
            var tmpBatNbr = "";
            var tmpRefNbr = "";
            var tmpcatchHandle = "";

            if (handle == "R")
            {
                var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "IN" && p.EditScrnNbr == "IN10100" && p.BatNbr == batNbr).FirstOrDefault();
                //var recordRefNbrUpdate = _db.IN_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).FirstOrDefault();
                recordBatNbrUpdate.Rlsed = 1;
                recordBatNbrUpdate.Status = "C";
                //recordRefNbrUpdate.Rlsed = 1;
                tmpcatchHandle = "1";
            }


         
            foreach (ppv_INReceiptBatch_Result created in lstheader.Created)
            {

                var recordHeader = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "IN" && p.BatNbr == created.BatNbr).FirstOrDefault();

                if (recordHeader == null)
                {
                    recordHeader = new Batch();
                    //bat dau xet cac dieu kien batNbr va refNbr
                    recordHeader.BranchID = branchID;
                    //dieu kien set batNbr
                    recordHeader.Module = "IN";
                    recordHeader.BatNbr = functionBatNbrIfNull(branchID);
                    tmpBatNbr = recordHeader.BatNbr;
                    //dieu kien set refNbr
                    //neu batNbr khac Null thi ta dang khoi tao cai RefNbr thu 2 cho cai batNbr nay
                   
                   recordHeader.RefNbr = functionRefNbrIfNull(branchID);

                    //set batNbr vao bien tam de reload ngoai man hinh
                   batNbrIfNull = recordHeader.BatNbr;

                   tmpRefNbr = recordHeader.RefNbr;
                    if (handle == "R")
                    {
                       recordHeader.Rlsed = 1;
                       recordHeader.Status = "C"; // sua lai sau
                    }
                    else if (handle == "N")
                    {
                       recordHeader.Rlsed = 0;
                       recordHeader.Status = "H";
                    }
                    recordHeader.OrigBranchID = "";
                    UpdatingFormBatch(created, ref recordHeader);

                    recordHeader.Crtd_DateTime = DateTime.Now;
                    recordHeader.Crtd_Prog = screenNbr;
                    recordHeader.Crtd_User = Current.UserName;
                    recordHeader.tstamp = new byte[0];
                    if (lstgrd.Updated.Count != 0)
                    {
                        _db.Batches.AddObject(recordHeader);
                    }
                    else
                    {
                        return Json(new { success = false, value = "NoRecordGrid" }, JsonRequestBehavior.AllowGet);
                    }
                    _db.SaveChanges();
                }
         
            }



            foreach (ppv_INReceiptBatch_Result updated in lstheader.Updated)
            {

                var recordHeader = _db.Batches.Where(p => p.BranchID == branchID && p.Module == "IN" && p.BatNbr == updated.BatNbr).FirstOrDefault();
                if (recordHeader != null)
                {
                    if (recordHeader.tstamp.ToHex() != updated.tstamp.ToHex())
                    {
                        return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    }

                    if (handle == "R")
                    {
                        recordHeader.Rlsed = 1;
                        recordHeader.Status = "C"; // sua lai sau
                    }
                    else if (handle == "N")
                    {
                        recordHeader.Rlsed = 0;
                        recordHeader.Status = "H";
                    }
                    tmpRefNbr = recordHeader.RefNbr;
                    UpdatingFormBatch(updated, ref recordHeader);
                }


            }



            foreach (ppv_IN10100_ReceiptLoad_Result created in lstgrd.Created)
            {

                var recordgrid = _db.IN_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == created.RefNbr &&
                                             p.LineRef == created.LineRef).FirstOrDefault();

                if (recordgrid == null)
                {
                    recordgrid = new IN_Trans();
                    //bat dau xet cac dieu kien batNbr va refNbr
                    recordgrid.BranchID = branchID;
                    if (batNbr != "")
                    {
                        recordgrid.BatNbr = batNbr;
                    }
                    else
                    {
                        recordgrid.BatNbr = tmpBatNbr;
                    }
                    recordgrid.RefNbr = tmpRefNbr;

                    if (handle == "R")
                    {
                        recordgrid.Rlsed = 1;
                  
                    }
                    else if (handle == "N")
                    {
                        recordgrid.Rlsed = 0;
                 
                    }

                    UpdatingGridIN_Trans(created, ref recordgrid);
                    recordgrid.TranDate = DateTime.Now.ToDateShort();
                    recordgrid.Crtd_DateTime = DateTime.Now;
                    recordgrid.Crtd_Prog = screenNbr;
                    recordgrid.Crtd_User = Current.UserName;
                    recordgrid.tstamp = new byte[0];
                    if (recordgrid.BatNbr != "" && recordgrid.RefNbr != "" && recordgrid.LineRef != "" && recordgrid.InvtID != "")
                    {
                        _db.IN_Trans.AddObject(recordgrid);
                    }
                    _db.SaveChanges();
                }

            }

            foreach (ppv_IN10100_ReceiptLoad_Result updated in lstgrd.Updated)
            {

                var recordgrid = _db.IN_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == updated.RefNbr &&
                                             p.LineRef == updated.LineRef).FirstOrDefault();

                if (recordgrid != null)
                {

                    if (recordgrid.tstamp.ToHex() != updated.tstamp.ToHex())
                    {
                        return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    }

                    if (handle == "R")
                    {
                        recordgrid.Rlsed = 1;
                    
                    }
                    else if (handle == "N")
                    {
                        recordgrid.Rlsed = 0;
                     
                    }
                    UpdatingGridIN_Trans(updated, ref recordgrid);
                }
                else
                {
                    recordgrid = new IN_Trans();
                    //bat dau xet cac dieu kien batNbr va refNbr
                    recordgrid.BranchID = branchID;
                    if (batNbr != "")
                    {
                        recordgrid.BatNbr = batNbr;
                    }
                    else
                    {
                        recordgrid.BatNbr = tmpBatNbr;
                    }


                    recordgrid.RefNbr = tmpRefNbr;

                    if (handle == "R")
                    {
                        recordgrid.Rlsed = 1;

                    }
                    else if (handle == "N")
                    {
                        recordgrid.Rlsed = 0;

                    }

                    UpdatingGridIN_Trans(updated, ref recordgrid);
                    recordgrid.TranDate = DateTime.Now.ToDateShort();
                    recordgrid.Crtd_DateTime = DateTime.Now;
                    recordgrid.Crtd_Prog = screenNbr;
                    recordgrid.Crtd_User = Current.UserName;
                    recordgrid.tstamp = new byte[0];
                    if (recordgrid.BatNbr != "" && recordgrid.RefNbr != "" && recordgrid.LineRef != "" && recordgrid.InvtID != "")
                    {
                        _db.IN_Trans.AddObject(recordgrid);
                    }
                    _db.SaveChanges();

                }
            }

            foreach (ppv_IN10100_ReceiptLoad_Result deleted in lstgrd.Deleted)
            {
                var del = _db.IN_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == deleted.RefNbr &&
                                             p.LineRef == deleted.LineRef).FirstOrDefault();
                if (del != null)
                {
                    _db.IN_Trans.DeleteObject(del);

                }

            }





            _db.SaveChanges();
            //this.Direct();


            return Json(new { success = true, value = batNbrIfNull }, JsonRequestBehavior.AllowGet);

        }


        private void UpdatingFormBatch(ppv_INReceiptBatch_Result s, ref Batch d)
        {
            d.TotAmt = s.TotAmt;
            d.DateEnt = Convert.ToDateTime(s.DateEnt);
            d.Descr = s.Descr;
            d.EditScrnNbr = "IN10100";
            d.FromToSiteID = s.FromToSiteID;
            d.IntRefNbr = s.IntRefNbr;
            d.JrnlType = "IN";
            d.ReasonCD = s.ReasonCD;
            d.RvdBatNbr = s.RvdBatNbr;
          
            

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void UpdatingGridIN_Trans(ppv_IN10100_ReceiptLoad_Result s, ref IN_Trans d)
        {
            d.LineRef = s.LineRef;
            d.CnvFact = s.CnvFact;
            d.ReasonCD = s.ReasonCD;
            d.FreeItem = false;
            d.InvtID = s.InvtID;
            d.InvtMult = s.InvtMult;
            d.JrnlType = s.JrnlType;
            d.ObjID = s.ObjID;
            d.Qty = s.Qty;
            d.ShipperID = s.ShipperID;
            d.ShipperLineRef = s.ShipperLineRef;
            d.SiteID = s.SiteID;
            d.SlsperID = s.SlsperID;
            d.ToSiteID = s.ToSiteID;
            d.TranAmt = s.TranAmt;
            d.TranFee = s.TranFee;
            
            d.TranDesc = s.TranDesc;
            d.TranType = s.TranType;
            d.UnitCost = s.UnitCost;
            d.UnitDesc = s.UnitDesc;
            d.UnitMultDiv = s.UnitMultDiv;
            d.UnitPrice = s.UnitPrice;
            d.CostID = s.CostID;
            d.QtyUncosted = 0;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.INNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.INNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        [DirectMethod]
        public ActionResult DeleteFormTopBatchIN10100(string batNbr, string branchID)
        {
            var recordTopBatch = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.Module == "IN");
            if (recordTopBatch != null)
            {
                var recordBotIN_Trans = _db.IN_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                if (recordBotIN_Trans != null)
                {
                    for (int k = 0; k < recordBotIN_Trans.Count; k++)
                    {
                        _db.IN_Trans.DeleteObject(recordBotIN_Trans[k]);
                       
                    }
                }
                _db.Batches.DeleteObject(recordTopBatch);
            }

            _db.SaveChanges();
            return this.Direct();
        }

        [DirectMethod]
        public ActionResult Export1(String batNbr,String branchID,String tranDate)
        {
            Application excelApplication = new Application();
            Workbook excelWorkBook;
            Worksheet SheetMCP;
            //Worksheet SheetTmp;
            object oMissing = Missing.Value;
            excelWorkBook = excelApplication.Workbooks.Add();
            excelWorkBook.Worksheets.Add();


            var lstInventory = _db.ppv_IN10100_ReceiptLoad(batNbr, branchID, "%", "%").ToList();

            var lstInvtIDAndDescr = _db.ppv_InventoryActiveByBranch(branchID).ToList();

            //var lstUnitDescr = _db.ppv_IN10100_UnitDescCol(batNbr, branchID, "%", "%").ToList();
          
            var NumexportValue = _sys.SYS_Configurations.Where(p => p.Code == "Numexport").FirstOrDefault();
            var Numexport = NumexportValue.IntVal;

            var fileName = this.HttpContext.Server.MapPath("~\\TempExport") + "\\" + Guid.NewGuid().ToString();
            if (!Directory.Exists(this.HttpContext.Server.MapPath("~\\TempExport")))
            {
                Directory.CreateDirectory(this.HttpContext.Server.MapPath("~\\TempExport"));
            }
            try
            {
                //tao Sheet
                SheetMCP = excelWorkBook.Worksheets[1];
                SheetMCP.Name = "MasterData";
                //SheetTmp = excelWorkBook.Worksheets[2];
                //SheetTmp.Name = "DataTmp";
                //SheetMCP.Range["AA1:CZ" + (lstGrid.Count+1)].NumberFormat = "@";

                //tao cot dau tien header
                this.SetCellValueHeader(SheetMCP, "B1", Util.GetLang("InventoryInputDetails"));
                SheetMCP.Range["B1:F1"].Merge();
                SheetMCP.Range["B1:F1"].Font.Bold = true;
                SheetMCP.Range["B1:F1"].Font.Size = 16;
                SheetMCP.Range["B1:F1"].Font.Color = Color.Blue;

                this.SetCellValueHeader(SheetMCP, "B2", "Tổng Tiền");
                SheetMCP.Range["B2:B2"].VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                SheetMCP.Range["B2:B2"].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                SheetMCP.Range["C2:C2"].Formula = "=SUM(F5:F" + Numexport + ")";
                SheetMCP.Range["C2:C2"].Locked = true;
                SheetMCP.Range["C2:C2"].NumberFormat = "#,##0";


                this.SetCellValueHeader(SheetMCP, "B2", Util.GetLang("TotAmt"));
                this.SetCellValueHeader(SheetMCP, "A4", Util.GetLang("InvtID"));
                this.SetCellValueHeader(SheetMCP, "B4", Util.GetLang("Descr"));
                this.SetCellValueHeader(SheetMCP, "C4", Util.GetLang("UnitDesc"));
                this.SetCellValueHeader(SheetMCP, "D4", Util.GetLang("Qty"));
                this.SetCellValueHeader(SheetMCP, "E4", Util.GetLang("UnitPrice"));
                this.SetCellValueHeader(SheetMCP, "F4", Util.GetLang("TotAmt"));
      
                //cac cot an
                this.SetCellValueHeader(SheetMCP, "AA1", Util.GetLang("InvtID"));
                this.SetCellValueHeader(SheetMCP, "AB1", Util.GetLang("Descr"));
                this.SetCellValueHeader(SheetMCP, "AC1", Util.GetLang("UnitDesc"));
                this.SetCellValueHeader(SheetMCP, "AD1", Util.GetLang("UnitPrice"));
   
                //an header 
                SheetMCP.get_Range("AA1", "AA1").Font.Color = Color.Transparent;
                SheetMCP.get_Range("AB1", "AB1").Font.Color = Color.Transparent;
                SheetMCP.get_Range("AC1", "AC1").Font.Color = Color.Transparent;
                SheetMCP.get_Range("AD1", "AD1").Font.Color = Color.Transparent;
         


                string cellName;
                int counter = 2;

                //do du lieu tu grid vao SheetMCP may cot phu an~ truoc
                counter = 2;
                for (int i = 0; i < lstInvtIDAndDescr.Count; i++)
                {
                    cellName = "AA" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).NumberFormat = "@";
                    SheetMCP.get_Range(cellName, cellName).Value2 = lstInvtIDAndDescr[i].InvtID;
                    SheetMCP.get_Range(cellName, cellName).Font.Color = Color.Transparent;

                    cellName = "AB" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).NumberFormat = "@";
                    SheetMCP.get_Range(cellName, cellName).Value2 = lstInvtIDAndDescr[i].Descr;
                    SheetMCP.get_Range(cellName, cellName).Font.Color = Color.Transparent;

                    cellName = "AC" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).NumberFormat = "@";
                    SheetMCP.get_Range(cellName, cellName).Value2 = lstInvtIDAndDescr[i].StkUnit;
                    SheetMCP.get_Range(cellName, cellName).Font.Color = Color.Transparent;


                    var tranDateConverted = Convert.ToDateTime(tranDate).ToDateShort();
                    var price = _db.PO_GetPrice(branchID, lstInvtIDAndDescr[i].InvtID, lstInvtIDAndDescr[i].StkUnit, tranDateConverted).FirstOrDefault();
                    cellName = "AD" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).NumberFormat = "@";
                    SheetMCP.get_Range(cellName, cellName).Value2 = price;
                    SheetMCP.get_Range(cellName, cellName).Font.Color = Color.Transparent;




                    counter++;
                }

                String formulaArea = "=$AA$2:$AA$" + Convert.ToString(lstInvtIDAndDescr.Count + 1);

                SheetMCP.get_Range("A5", "A" + Numexport).Validation.Add(XlDVType.xlValidateList, XlDVAlertStyle.xlValidAlertStop, XlFormatConditionOperator.xlBetween, formulaArea, oMissing);
                SheetMCP.get_Range("A5", "A" + Numexport).Validation.IgnoreBlank = true;
                SheetMCP.get_Range("A5", "A" + Numexport).Validation.InputTitle = "";
                SheetMCP.get_Range("A5", "A" + Numexport).Validation.ErrorTitle = "";
                SheetMCP.get_Range("A5", "A" + Numexport).Validation.InputMessage = "Chọn Mã Sản Phẩm";
                SheetMCP.get_Range("A5", "A" + Numexport).Validation.ErrorMessage = "Mã Sản Phẩm này không tồn tại";
                SheetMCP.get_Range("A5", "A" + Numexport).Validation.ShowInput = true;
                SheetMCP.get_Range("A5", "A" + Numexport).Validation.ShowError = true;
                SheetMCP.get_Range("A5", "A" + Numexport).NumberFormat = "@";

                String formulaDescr = string.Format("=IF(ISERROR(VLOOKUP({0},$AA$2:$AB${1},2,0)),\"\",VLOOKUP({0},$AA$2:$AB${1},2,0))", "A5", Convert.ToSingle(lstInvtIDAndDescr.Count + 1));

                SheetMCP.get_Range("B5", "B" + Numexport).Formula = formulaDescr;


                String formulaUnitDescr = string.Format("=IF(ISERROR(VLOOKUP({0},$AA$2:$AC${1},3,0)),\"\",VLOOKUP({0},$AA$2:$AC${1},3,0))", "A5", Convert.ToSingle(lstInvtIDAndDescr.Count + 1));
                SheetMCP.get_Range("C5", "C" + Numexport).Formula = formulaUnitDescr;

                String formulaPriceInventory = string.Format("=IF(ISERROR(VLOOKUP({0},$AA$2:$AD${1},4,0)),\"\",VLOOKUP({0},$AA$2:$AD${1},4,0))", "A5", Convert.ToSingle(lstInvtIDAndDescr.Count + 1));
                SheetMCP.get_Range("E5", "E" + Numexport).Formula = formulaPriceInventory;

                SheetMCP.get_Range("F5", "F" + Numexport).Formula = "=IF(ISERROR(D5*E5),\"\",D5*E5)";

                SheetMCP.get_Range("D5", "D" + Numexport).NumberFormat = "#,##0";
                SheetMCP.get_Range("E5", "E" + Numexport).NumberFormat = "#,##0";
                SheetMCP.get_Range("F5", "F" + Numexport).NumberFormat = "#,##0";






                //counter = 2;
            

                //bo du lieu san co tu grid trong form sang excel
                counter = 5;
                //string [] objCustomer=lstcustomer.TrimEnd(';').Split(';');
                for (int i = 0; i < lstInventory.Count; i++)
                {

                    cellName = "A" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).NumberFormat = "@";
                    SheetMCP.get_Range(cellName, cellName).Value2 = lstInventory[i].InvtID;



                    cellName = "C" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).Value2 = lstInventory[i].UnitDesc;

                    cellName = "D" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).Value2 = lstInventory[i].Qty;

                    cellName = "E" + counter.ToString();
                    SheetMCP.get_Range(cellName, cellName).Value2 = lstInventory[i].UnitPrice;

                    counter++;
                }

                SheetMCP.Columns.AutoFit();

                SheetMCP.Columns["B"].ColumnWidth = 50;
                SheetMCP.Columns["E"].ColumnWidth = 20;
                SheetMCP.Columns["F"].ColumnWidth = 25;
                SheetMCP.get_Range("C2", "C2").ColumnWidth = 20;
                SheetMCP.get_Range("A5", "A" + Numexport).Locked = false;
                SheetMCP.get_Range("D5", "D" + Numexport).Locked = false;


                SheetMCP.Protect("pvm2013");

                var Format = ".xlsx";
                excelApplication.DisplayAlerts = false;
                if (Format == ".xlsx")
                {
                    excelWorkBook.SaveAs(fileName, XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                else
                {
                    excelWorkBook.SaveAs(fileName, XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                //excelWorkBook.SaveAs(fileName, XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange);
                excelWorkBook.Close(true, Type.Missing, Type.Missing);
                excelApplication.Quit();
                excelApplication = null;

                //byte[] buffer = new byte[1];
                //using (FileStream fs = new FileStream(fileName, FileMode.Open,
                //                   FileAccess.Read, FileShare.Read))
                //{
                //    buffer = new byte[fs.Length];
                //    fs.Read(buffer, 0, (int)fs.Length);

                //}  

                //return File(string.Format("{0}.xls", fileName), "application/xls","");
                return Json(new { success = true, filePath = string.Format("{0}.xlsx", fileName) });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
            finally
            {
                if (System.IO.File.Exists(fileName))
                    System.IO.File.Delete(fileName);
                SheetMCP = null;
                if (excelWorkBook != null)
                    excelWorkBook = null;
                if (excelApplication != null)
                {
                    excelApplication.Quit();
                    excelApplication = null;
                }
            }

            //return Json(new { success = true });
        }

        public ActionResult Download(string filePath)
        {
            var dlFileName = string.Format("{0}_{1}.xlsx", screenNbr, DateTime.Now.ToString("ddMMyyHHmmss"));

            return File(filePath, "application/xls", dlFileName);
        }


        private void SetCellValueHeader(Worksheet sheet, string cell, object value)
        {
            sheet.Range[cell].Value = value;
            sheet.Range[cell].VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
            sheet.Range[cell].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            sheet.Range[cell].Font.Bold = true;
            sheet.Range[cell].Font.Size = 12;
            //sheet.Range[cell].Font.Color = Color.Blue;
            // cell.CellFormat.ShrinkToFit = ExcelDefaultableBoolean.True;
            //sheet.Range[cell].VerticalAlignment=
            //sheet.Range[cell].Alignment = HorizontalCellAlignment.Center;
        }

        [DirectMethod]
        public ActionResult Import(String batNbr, String branchID, String refNbr, String handle, String reasonCD, String siteID)
        {

            //var tmpCheckRowAdded = "";
            //string lstUser = "";
            //string lstName = "";
            //string lstPass = "";
            var k = 0;
            var message = "";
            try
            {

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("ImportTemplate");

                HttpPostedFile file = fileUploadField.PostedFile; // or: HttpPostedFileBase file = this.HttpContext.Request.Files[0];
                string fileName = file.FileName;
                FileInfo info = new FileInfo(fileName);
                if (fileName.ToLower().Contains(".xls") || fileName.ToLower().Contains(".xlsx"))
                {
                    System.Globalization.CultureInfo oldCI = System.Threading.Thread.CurrentThread.CurrentCulture;
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");



                    fileName = Server.MapPath("~\\TempImport") + "\\" + Guid.NewGuid().ToString() + info.Extension;


                    file.SaveAs(fileName);

                    Application excelApplication = null;
                    Workbook excelWorkBook = null;
                    Worksheet sheetCust = null;
                    var errorRow = "hang";
                    try
                    {
                        excelApplication = new Application();
                        excelApplication.Visible = false;
                        //ExcelFile ef = new ExcelFile();
                        //excelWorkBook = ExcelFile.Load(fileName);
                        //sheetCust = excelWorkBook.Worksheets
                        excelWorkBook = excelApplication.Workbooks.Open(fileName);
                        //sheetCust = excelWorkBook.Worksheets[2];
                        sheetCust = excelWorkBook.Sheets[1];
                        //sheetCust.Columns["A"].NumberFormat = "@";
                        //sheetCust.Columns["B"].NumberFormat = "@";
                        //sheetCust.Columns["C"].NumberFormat = "@";
                        //sheetCust.Columns["A"].Style = "currency";
                        //sheetCust.Columns["B"].Style = "currency";
                        //sheetCust.Columns["C"].Style = "currency";
                        //sheetCust.number
                        //sheetCust.Columns["A"].NumberFormat = excelWorkBook.Styles("currency").NumberFormat;
                        //sheetCust.Columns["A"].NumberFormat = "_($* #,##0.00_);_($* (#,##0.00);_($* " - "??_);_(@_)";
                        //sheetCust.Columns["B"].NumberFormat = "_($* #,##0.00_);_($* (#,##0.00);_($* " - "??_);_(@_)";
                        //sheetCust.Columns["C"].NumberFormat = "_($* #,##0.00_);_($* (#,##0.00);_($* " - "??_);_(@_)";
                        sheetCust.Unprotect("pvm2013");
                        Microsoft.Office.Interop.Excel.Range last = sheetCust.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell, Type.Missing);
                        Microsoft.Office.Interop.Excel.Range range = sheetCust.get_Range("A1", last);

                        int lastUsedRow = last.Row;

                        range = sheetCust.get_Range("A5", "AM" + lastUsedRow.ToString()); // get all values
                        System.Array dataArray = (System.Array)(range.Cells.Value2);
                        int length = dataArray.GetUpperBound(0);
                        string userName = Current.UserName;

                        

                        //IN_ReceiptLoad


                        for (int i = 1; i <= length; i++)
                        {
                            var invtID = dataArray.GetValue(i, 1).PassNull();
                            var invtDescr = dataArray.GetValue(i, 2).PassNull();
                            var unitDescr = dataArray.GetValue(i, 3).PassNull();
                            var qty = dataArray.GetValue(i, 4).PassNull();
                            var unitPrice = dataArray.GetValue(i, 5).PassNull();


                            var lstInvt = _db.ppv_InventoryActiveByBranch(branchID).ToList();

                            if (invtID != "")
                            {
                                var objInvt = lstInvt.FirstOrDefault(p => p.InvtID == invtID);
                                if (objInvt == null)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không tồn tại hoặc hết tồn kho\n", (i + 1).ToString(), invtID);
                                    //continue;
                                }
                            }
                        
                            //if (unitDescr == string.Empty || unitDescr.ToDouble() == 0)
                            //{
                            //    continue;
                            //}

                            var lstOnGrid = _db.ppv_IN10100_ReceiptLoad(batNbr, branchID, "%", "%").ToList();
                            var recordOnGrid = lstOnGrid.FirstOrDefault(p => p.InvtID == invtID && p.UnitDesc == unitDescr);

                            var lineRef = "";

                            if (lstOnGrid.Count + 1 < 10)
                            {
                                lineRef = "0000" + (lstOnGrid.Count + 1);
                            }
                            else if ((lstOnGrid.Count + 1) >= 10 && (lstOnGrid.Count + 1) < 100)
                            {
                                lineRef = "000" + (lstOnGrid.Count + 1);
                            }
                            else if ((lstOnGrid.Count + 1) >= 100 && (lstOnGrid.Count + 1) < 1000)
                            {
                                lineRef = "00" + (lstOnGrid.Count + 1);
                            }
                            else if ((lstOnGrid.Count + 1) >= 1000 && (lstOnGrid.Count + 1) < 10000)
                            {
                                lineRef = "0" + (lstOnGrid.Count + 1);
                            }
                            else if ((lstOnGrid.Count + 1) >= 10000 && (lstOnGrid.Count + 1) < 100000)
                            {
                                lineRef = Convert.ToString(lstOnGrid.Count + 1);
                            }

                            if (recordOnGrid == null && invtID != "")
                            {
                                var record = new IN_Trans();
                                record.BatNbr = batNbr;
                                record.BranchID = branchID;
                                record.RefNbr = refNbr;
                                record.LineRef = lineRef;
                                record.CnvFact = 1;
                                record.UnitMultDiv = "M";
                                record.InvtID = invtID;
                                record.ReasonCD = reasonCD;
                                if (qty != "")
                                {
                                    record.Qty = Convert.ToDouble(qty);
                                }
                                
                                record.FreeItem = false;
                                record.UnitDesc = unitDescr;
                                record.TranDesc = invtDescr;
                                record.TranDate = DateTime.Now.ToDateShort();
                                record.SiteID = siteID;
                                if (unitPrice != "")
                                {
                                    record.UnitPrice = Convert.ToDouble(unitPrice);
                                }
                                if (handle == "R")
                                {
                                    record.Rlsed = 1;

                                }
                                else if (handle == "N")
                                {
                                    record.Rlsed = 0;

                                }

                                record.LUpd_DateTime = DateTime.Now;
                                record.LUpd_Prog = screenNbr;
                                record.LUpd_User = Current.UserName;
                                record.Crtd_DateTime = DateTime.Now;
                                record.Crtd_Prog = screenNbr;
                                record.Crtd_User = Current.UserName;
                                record.tstamp = new byte[0];
                                if (record.BatNbr != "" && record.RefNbr != "" && record.LineRef != "" && record.InvtID != "")
                                {
                                    _db.IN_Trans.AddObject(record);
                                }

                                _db.SaveChanges();

                            }
                            else
                            {
                                //message += string.Format("Dòng {0} mặt hàng {1} đơn vị {2} trùng\n", i, invtID, unitDescr);
                            }
                        //ngoac ket thuc vong for
                        }

                            
                    }
                    catch (Exception ex)
                    {
                        excelWorkBook.Close(true, Type.Missing, Type.Missing);
                        excelApplication.Quit();
                        return (ex as MessageException).ToMessage();
                    }
                    finally
                    {
                        if (sheetCust != null)
                        {
                            Marshal.ReleaseComObject(sheetCust);
                            sheetCust = null;
                        }
                        if (excelWorkBook != null)
                        {
                            excelWorkBook.Close(true, Type.Missing, Type.Missing);
                            Marshal.ReleaseComObject(excelWorkBook);
                            excelWorkBook = null;
                        }
                        if (excelApplication != null)
                        {
                            excelApplication.Quit();
                            excelApplication = null;
                        }

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        if (System.IO.File.Exists(fileName))
                        {
                            System.IO.File.Delete(fileName);
                        }
                    }
                    //Util.AppendLog(ref mLogMessage, "20121418");
                }
                else
                {
                    //Util.AppendLog(ref mLogMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
                //X.Msg.Alert("Uploaded", string.Format("The file '{0}' has been successfully uploaded.", fileName)).Show();

                //if (mLogMessage != null)
                //{
                //    return mLogMessage;
                //}
                //else
                return Json(new { success = true, value = message });

            }
            catch (Exception ex)
            {
                throw ex;
                //if (ex is MessageException)
                //{
                //    return (ex as MessageException).ToMessage();
                //}
                //else
                //{
                //    return Json(new { success = false, value = k, value1 = tmpCheckRowAdded });
                //}
            }

        }











    }
}

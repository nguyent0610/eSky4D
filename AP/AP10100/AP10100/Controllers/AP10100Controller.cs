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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using HQ.eSkyFramework.HQControl;
using System.Drawing;
using HQ.eSkySys;
namespace AP10100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AP10100Controller : Controller
    {
        private string _screenNbr = "AP10100";
        private string _userName = Current.UserName;
    
        AP10100Entities _app = Util.CreateObjectContext<AP10100Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);


        private List<AP10100_pgLoadInvoiceMemo_Result> _lstAPTrans = new List<AP10100_pgLoadInvoiceMemo_Result>();
        private List<AP10100_pgLoadTaxTrans_Result> _lstTax = new List<AP10100_pgLoadTaxTrans_Result>();
        private Batch _objBatch;
        private FormCollection _form;
        string _batNbr = "";
        string _refNbr = "";
        string _branchID = "";
        string _handle = "";
        string _status = "";
        private AP10100_pcGetVendor_Result _objVendor = new AP10100_pcGetVendor_Result();
        private JsonResult _logMessage;
        private AP10100_pdHeader_Result _pdHead;


        public ActionResult Index(string branchID)
        {
            Util.InitRight(_screenNbr);

            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);

            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }

            //var userDft = _app.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID == branchID && p.UserID == Current.UserName);
            //if (branchID == null) branchID = Current.CpnyID;
            //ViewBag.INSite = userDft == null ? "" : userDft.INSite;
            ViewBag.BranchID = branchID;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetHeader(string branchID, string batNbr)
        {
            var obj = _app.AP10100_pdHeader(branchID, batNbr).FirstOrDefault();
            return this.Store(obj);
        }

        public ActionResult GetAPTrans(String branchID, String batNbr, String refNbr)
        {
            var lst = _app.AP10100_pgLoadInvoiceMemo(branchID, batNbr, refNbr).ToList();
            return this.Store(lst);
        }

        public ActionResult GetTaxTrans(string branchID, string batNbr, string refNbr)
        {
            return this.Store(_app.AP10100_pgLoadTaxTrans(branchID, batNbr, refNbr).ToList());
        }


        [HttpPost]
        public ActionResult Save(FormCollection data, string branchID, string handle, string batNbr, string refNbr, string docType, string vendID)
        {
            try
            {
                var acc = Session["AP10100"] as AccessRight;

                StoreDataHandler dataGrid = new StoreDataHandler(data["lstgrd"]);
                _lstAPTrans = dataGrid.ObjectData<AP10100_pgLoadInvoiceMemo_Result>().Where(p => p.TranAmt != 0).ToList();
                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _pdHead = detHeader.ObjectData<AP10100_pdHeader_Result>().FirstOrDefault();

                _batNbr = data["cboBatNbr"];
                _refNbr = data["RefNbr"];
                _branchID = data["txtBranchID"];
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();
                _objVendor = _app.AP10100_pcGetVendor(Current.UserName, _branchID).FirstOrDefault(p => p.VendID == _pdHead.VendID);
                if (_objVendor == null)
                {
                    _objVendor = new AP10100_pcGetVendor_Result();
                    _objVendor.ResetET();

                }
                if (Data_Checking())
                {

                    if ((_status == "U" || _status == "C") && (_handle == "C" || _handle == "V"))
                    {

                        if (_handle == "V" || _handle == "C")
                        {
                            if ((_handle == "V" || _handle == "C") && !acc.Release)
                            {
                                throw new MessageException(MessageType.Message, "725");
                            }
                            else
                            {
                                if (_handle == "V" || _handle == "C")
                                {
                                    Data_Release();
                                }
                            }
                        }
                    }
                    else if (_status == "H")
                    {
                        if (_handle == "R" && !acc.Release)
                        {
                            throw new MessageException(MessageType.Message, "737");
                        }
                        else
                        {
                            Save_Batch();
                            _app.SaveChanges();
                            Data_Release();
                        }
                    }
                }

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
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });

            }
        }
        #region ProcessData
        private void Save_Batch(bool isDeleteGrd = false)
        {

            _objBatch = _app.Batches.Where(p => p.Module == "AP" && p.BatNbr == _batNbr && p.BranchID == _branchID).FirstOrDefault();
            if (_objBatch != null)
            {
                if (_objBatch.tstamp.ToHex() != _pdHead.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                Updating_Batch(ref _objBatch);

            }
            else
            {
                _objBatch = new Batch();
                _objBatch.ResetET();
                _objBatch.Rlsed = 0;
                Updating_Batch(ref _objBatch);

                var objBatNbr = _app.APNumbering(_branchID, "BatNbr").FirstOrDefault();
                _objBatch.BranchID = _branchID;
                _objBatch.BatNbr = objBatNbr;


                _objBatch.Crtd_DateTime = DateTime.Now;
                _objBatch.Crtd_Prog = _screenNbr;
                _objBatch.Crtd_User = Current.UserName;
                //_objBatch.tstamp = new byte[0];
                _app.Batches.AddObject(_objBatch);

            }
            _batNbr = _objBatch.BatNbr;
            SaveAP_Doc(_objBatch);

        }

        private void Updating_Batch(ref Batch objBatch)
        {

            objBatch.Module = "AP";
            objBatch.JrnlType = "AP";
            objBatch.TotAmt = _pdHead.TotAmt;
            objBatch.Status = _pdHead.Status;
            objBatch.EditScrnNbr = _screenNbr;
            objBatch.DateEnt = _pdHead.DocDate.ToDateShort();
            objBatch.NoteID = 0;
            objBatch.RvdBatNbr = _pdHead.RvdBatNbr;
            objBatch.Descr = _pdHead.DocDesc;

            objBatch.LUpd_DateTime = DateTime.Now;
            objBatch.LUpd_Prog = _screenNbr;
            objBatch.LUpd_User = Current.UserName;

        }

        private void SaveAP_Doc(Batch objBatch)
        {

            var obj = _app.AP_Doc.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _refNbr).FirstOrDefault();
            if (obj != null)
            {

                Updating_AP_Doc(ref obj);
                SaveAP_Trans(obj);
            }
            else
            {
                obj = new AP_Doc();
                obj.ResetET();
                obj.RefNbr = _app.APNumbering(_branchID, "RefNbr").FirstOrDefault();
                obj.BranchID = objBatch.BranchID;
                obj.BatNbr = objBatch.BatNbr;
                obj.Crtd_DateTime = DateTime.Now;
                obj.Crtd_Prog = _screenNbr;
                obj.Crtd_User = Current.UserName;
                Updating_AP_Doc(ref obj);
                _app.AP_Doc.AddObject(obj);
                SaveAP_Trans(obj);
            }

        }
        private void Updating_AP_Doc(ref AP_Doc objD)
        {


            objD.NoteID = 0;
            objD.PONbr = _pdHead.PONbr;
            objD.InvcNbr = _pdHead.InvcNbr;
            objD.InvcNote = _pdHead.InvcNote;
            objD.RcptNbr = _pdHead.RcptNbr;
            objD.DocBal = _pdHead.DocBal;
            objD.OrigDocAmt = _pdHead.OrigDocAmt;

            objD.TaxTot00 = _pdHead.TaxTot00;
            objD.TxblTot00 = _pdHead.TxblTot00;
            objD.TaxId00 = _pdHead.TaxId00;
            objD.TaxTot01 = _pdHead.TaxTot01;
            objD.TxblTot01 = _pdHead.TxblTot01;
            objD.TaxId01 = _pdHead.TaxId01;
            objD.TaxTot02 = _pdHead.TaxTot02;
            objD.TxblTot02 = _pdHead.TxblTot02;
            objD.TaxId02 = _pdHead.TaxId02;
            objD.TaxTot03 = _pdHead.TaxTot03;
            objD.TxblTot03 = _pdHead.TxblTot03;
            objD.TaxId03 = _pdHead.TaxId03;

            //objD.CustId = this.cboCustId.SelectedItem == null ? "" : (this.cboCustId.SelectedItem as ppv_CustomerActive_Result).CustID;
            objD.DocDesc = _pdHead.DocDesc;
            objD.InvcDate = _pdHead.InvcDate;
            objD.DiscDate = _pdHead.DiscDate;
            objD.DocDate = _pdHead.DocDate;
            objD.Terms = _pdHead.Terms;
            objD.DueDate = _pdHead.DueDate;
            //objD.NoteId = 0;// DocNoteID;
            objD.DocType = _pdHead.DocType;
            objD.VendID = _pdHead.VendID;

            objD.LUpd_DateTime = DateTime.Now;
            objD.LUpd_Prog = _screenNbr;
            objD.LUpd_User = Current.UserName;


        }
        private void SaveAP_Trans(AP_Doc objD)
        {
            for (int i = 0; i < _lstAPTrans.Count; i++)
            {
                var objRecord = _lstAPTrans[i];
                var obj = _app.AP_Trans.Where(p => p.BranchID == objD.BranchID && p.BatNbr == objD.BatNbr && p.RefNbr == objD.RefNbr && p.LineRef == objRecord.LineRef).FirstOrDefault();
                if (obj != null)
                {
                    if (obj.tstamp.ToHex() != objRecord.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    Updating_AP_Trans(_lstAPTrans[i], ref obj);

                }
                else
                {
                    obj = new AP_Trans();
                    obj.ResetET();
                    Updating_AP_Trans(_lstAPTrans[i], ref obj);
                    obj.BranchID = objD.BranchID;
                    obj.BatNbr = objD.BatNbr;
                    obj.RefNbr = objD.RefNbr;

                    obj.Crtd_DateTime = DateTime.Now;
                    obj.Crtd_Prog = _screenNbr;
                    obj.Crtd_User = Current.UserName;
                   // obj.tstamp = new byte[0];

                    _app.AP_Trans.AddObject(obj);
                }

            }

        }
        private void Updating_AP_Trans(AP10100_pgLoadInvoiceMemo_Result objr, ref AP_Trans objAP_Trans)
        {
            objAP_Trans.LineRef = objr.LineRef;
            objAP_Trans.VendID = _pdHead.VendID;
            objAP_Trans.VendName = _objVendor.Name;
            objAP_Trans.Addr = _objVendor.Address;

            objAP_Trans.InvcNbr = _pdHead.InvcNbr;
            objAP_Trans.InvcNote = _pdHead.InvcNote;
            objAP_Trans.InvtID = objr.InvtID;
            objAP_Trans.InvcDate = _pdHead.InvcDate;
            objAP_Trans.JrnlType = objr.JrnlType;
            objAP_Trans.LineType = objr.LineType;
            objAP_Trans.POLineRef = objr.POLineRef;
            objAP_Trans.PONbr = objr.PONbr;
            objAP_Trans.Qty = objr.Qty;
            objAP_Trans.TranAmt = objr.TranAmt;
            objAP_Trans.TranClass = string.IsNullOrEmpty(objr.TranClass) ? "" : objr.TranClass;
            objAP_Trans.TranDate = _pdHead.DocDate.ToDateShort();
            objAP_Trans.TranDesc = objr.TranDesc;
            objAP_Trans.TranType = Util.GetLang(_pdHead.DocType);
            objAP_Trans.TaxRegNbr = objr.TaxRegNbr;
            objAP_Trans.UnitPrice = objr.UnitPrice;


            objAP_Trans.TaxCat = objr.TaxCat;
            objAP_Trans.TaxId00 = objr.TaxId00;
            objAP_Trans.TaxId01 = objr.TaxId01;
            objAP_Trans.TaxId02 = objr.TaxId02;
            objAP_Trans.TaxId03 = objr.TaxId03;


            objAP_Trans.TaxAmt00 = objr.TaxAmt00;
            objAP_Trans.TaxAmt01 = objr.TaxAmt01;
            objAP_Trans.TaxAmt02 = objr.TaxAmt02;
            objAP_Trans.TaxAmt03 = objr.TaxAmt03;

            objAP_Trans.TxblAmt00 = objr.TxblAmt00;
            objAP_Trans.TxblAmt01 = objr.TxblAmt01;
            objAP_Trans.TxblAmt02 = objr.TxblAmt02;
            objAP_Trans.TxblAmt03 = objr.TxblAmt03;


            objAP_Trans.LUpd_DateTime = DateTime.Now;
            objAP_Trans.LUpd_Prog = _screenNbr;
            objAP_Trans.LUpd_User = Current.UserName;
        }
        #endregion
        #region Release
        private void Data_Release()
        {
            if (_handle != "N")
            {
                DataAccess dal = Util.Dal();
                try
                {
                    APProcess.AP ap = new APProcess.AP(Current.UserName, _screenNbr, dal);
                    if (_handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!ap.AP10100_Release(_branchID, _batNbr))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _batNbr });
                    }
                    else if (_handle == "C" || _handle == "V")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!ap.AP10100_Cancel(_branchID, _batNbr, _refNbr, _handle))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }
                        Util.AppendLog(ref _logMessage, "9999", data: new { success = true, batNbr = _batNbr });
                    }
                    ap = null;
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
            }
        }
        #endregion
        #region Orther
        private bool Data_Checking()
        {


            //Check PO has no detail data
            if (_lstAPTrans.Count == 0)
            {
                throw new MessageException(MessageType.Message, "20405");
            }


            for (Int32 i = 0; i < _lstAPTrans.Count; i++)
            {
                AP10100_pgLoadInvoiceMemo_Result objAP_Trans = new AP10100_pgLoadInvoiceMemo_Result();
                objAP_Trans = _lstAPTrans[i];
                if (objAP_Trans.TranAmt == 0)
                {
                    throw new MessageException(MessageType.Message, "220");
                }
				if (objAP_Trans.InvtID.PassNull() != "")
				{
					if (objAP_Trans.Qty.ToDouble() == 0 )
					{
						throw new MessageException(MessageType.Message, "15", "", new string[] { Util.GetLang("Qty") });
					}
					else if (objAP_Trans.UnitPrice.ToDouble() == 0)
					{
						throw new MessageException(MessageType.Message, "15", "", new string[] { Util.GetLang("UnitPrice") });
				
					}

				}
            }

			if (_app.AP10100_ppCheckCloseDate(_branchID, _pdHead.DocDate.ToDateShort()).FirstOrDefault() == "0")
				throw new MessageException(MessageType.Message, "301");

            return true;
        }
        #endregion


        [HttpPost]
        public ActionResult DeleteHeader(FormCollection data)
        {
            try
            {
                var acc = Session["PO10200"] as AccessRight;
                _form = data;
                _batNbr = data["cboBatNbr"];
                _refNbr = data["RefNbr"];
                _branchID = Current.CpnyID;
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();

                var detHeader = new StoreDataHandler(data["lstHeader"]);

                if (_pdHead == null)
                    _pdHead = detHeader.ObjectData<AP10100_pdHeader_Result>().FirstOrDefault();
                var objHeader = _app.Batches.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr).FirstOrDefault();
                if (objHeader != null)
                {
                    if (_pdHead.tstamp.ToHex() != objHeader.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    _app.Batches.DeleteObject(objHeader);
                    var objRe = _app.AP_Doc.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _refNbr).FirstOrDefault();
                    if (objRe != null)
                    {
                        _app.AP_Doc.DeleteObject(objRe);
                    }
                    var lstdel = _app.AP_Trans.Where(p => p.BatNbr == _batNbr && p.BranchID == _branchID && p.RefNbr == _refNbr).ToList();
                    while (lstdel.FirstOrDefault() != null)
                    {
                        var obj = lstdel.FirstOrDefault();
                        _app.AP_Trans.DeleteObject(obj);
                        lstdel.Remove(obj);
                    }
                    _app.SaveChanges();
                }
                return Util.CreateMessage(MessageProcess.Delete, new { batNbr = "" });

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
        public ActionResult DeleteGrd(FormCollection data)
        {
            try
            {
                var acc = Session["AP10100"] as AccessRight;
                _form = data;
                _batNbr = data["cboBatNbr"];
                _refNbr = data["RefNbr"];
                _branchID = Current.CpnyID;


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _pdHead = detHeader.ObjectData<AP10100_pdHeader_Result>().FirstOrDefault();

                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstAPTrans = detHandler.ObjectData<AP10100_pgLoadInvoiceMemo_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstDel"]);
                ChangeRecords<AP10100_pgLoadInvoiceMemo_Result> lst = dataHandler.BatchObjectData<AP10100_pgLoadInvoiceMemo_Result>();

                if (_pdHead == null)
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                else
                {
                    foreach (AP10100_pgLoadInvoiceMemo_Result deleted in lst.Deleted.Where(p => p.tstamp.ToHex() != ""))
                    {
                        var obj = _app.AP_Trans.Where(p => p.BranchID == deleted.BranchID && p.BatNbr == deleted.BatNbr && p.RefNbr == deleted.RefNbr && p.LineRef == deleted.LineRef).FirstOrDefault();
                        _app.AP_Trans.DeleteObject(obj);
                    }
                    Save_Batch(true);
                    _app.SaveChanges();
                }
                return Util.CreateMessage(MessageProcess.Delete, new { batNbr = _batNbr });


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

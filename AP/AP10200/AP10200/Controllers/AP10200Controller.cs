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

namespace AP10200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AP10200Controller : Controller
    {
        private string _screenNbr = "AP10200";
        private string _userName = Current.UserName;
        private string _branchID = "";
        AP10200Entities _db = Util.CreateObjectContext<AP10200Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        private FormCollection _form;
        private Batch _objBatch;
        private AP_Doc _objAP_Doc;
        private List<AP10200_pgLoadGridTrans_Result> _lstTrans;
        private JsonResult _logMessage;
        private string _handle;
        private AP_Setup _objAP;
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

        #region GetData 
        public ActionResult GetBatch(String branchID, String batNbr)
        {
            var rptCtrl = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.Module == "AP" && p.BatNbr == batNbr);
            return this.Store(rptCtrl);
        }

        public ActionResult GetAP_Doc(String branchID, String batNbr, String refNbr)
        {
            //var lst = _db.AP_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
           
            var lst = _db.AP_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
            return this.Store(lst);
        }

        public ActionResult GetAPTrans(String branchID, String batNbr, String refNbr)
        {
            var lst = _db.AP10200_pgLoadGridTrans(branchID, batNbr, refNbr).ToList();

            return this.Store(lst);
        }
        #endregion

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                _form = data;
				if (_db.AP10100_ppCheckCloseDate(_form["txtBranchID"].PassNull(), _form["dteDocDate"].ToDateShort()).FirstOrDefault() == "0")
					throw new MessageException(MessageType.Message, "301");
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

        #region Save
        private void SaveData(FormCollection data)
        {
            string batNbr = data["cboBatNbr"];
            string branchID = data["txtBranchID"];
		
            var batchHander = new StoreDataHandler(data["batch"]);
            _objBatch = batchHander.ObjectData<Batch>().FirstOrDefault();

            _handle = data["cboHandle"];
            _objBatch.Status = _objBatch.Status == string.Empty ? "H" : _objBatch.Status;
   
            var docHander = new StoreDataHandler(data["doc"]);
            _objAP_Doc = docHander.ObjectData<AP_Doc>().FirstOrDefault();
            if (_lstTrans == null)
            {
                 var transHandler = new StoreDataHandler(data["trans"]);
                _lstTrans = transHandler.ObjectData<AP10200_pgLoadGridTrans_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }

            SaveBatch(batNbr, branchID);
            SaveDoc(batNbr, branchID);
			StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrdtrans"]);
			ChangeRecords<AP10200_pgLoadGridTrans_Result> lstgrd = dataHandlerGrid.BatchObjectData<AP10200_pgLoadGridTrans_Result>();
			foreach (AP10200_pgLoadGridTrans_Result deleted in lstgrd.Deleted)
			{
				var record = _db.AP_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.LineRef == deleted.LineRef ).FirstOrDefault();
				if (record != null)
				{
					_db.AP_Trans.DeleteObject(record);
				}

			}
            SaveTrans(batNbr, branchID,data);
            _db.SaveChanges();

            if (_handle != "N")
            {
                DataAccess dal = Util.Dal();
                APProcess.AP ap = new APProcess.AP(_userName, _screenNbr, dal);
                try
                {
                    if (_handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);

                        ap.AP10200_Release(_objBatch.BranchID, _objBatch.BatNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                    else if (_handle == "C" || _handle == "V")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!ap.AP10200_Cancel(_objBatch.BranchID,_objBatch.BatNbr, _objBatch.RefNbr))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }
                        Util.AppendLog(ref _logMessage, "9999", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
                finally
                {
                    ap = null;
                    dal = null;
                }
            }
        }

        private void SaveBatch(string batNbr, string branchID)
        {
			 
            var batch = _db.Batches.FirstOrDefault(p => p.BatNbr == batNbr && p.BranchID == branchID);
            if (batch != null)
            {
                if (batch.tstamp.ToHex() != _objBatch.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }
				UpdateBatch(batch, false);
            }
            else {
                _objBatch.BatNbr = functionBatNbrIfNull(branchID);
                _objBatch.RefNbr = functionRefNbrIfNull(branchID);
                _objBatch.BranchID = branchID;
                batch = new Batch();
				UpdateBatch(batch, true);
                _db.Batches.AddObject(batch);
            }
        }

        private void SaveDoc(string batNbr, string branchID)
        {
            var doc = _db.AP_Doc.FirstOrDefault(p => p.BatNbr == batNbr && p.BranchID == branchID && p.RefNbr == _objAP_Doc.RefNbr);
            if (doc != null)
            {
                if (doc.tstamp.ToHex() != _objAP_Doc.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                UpdateDoc(doc, false);
            }
            else{
                _objAP_Doc.BatNbr = _objBatch.BatNbr;
                _objAP_Doc.BranchID = branchID;
                _objAP_Doc.RefNbr = _objBatch.RefNbr;
                doc = new AP_Doc();
                UpdateDoc(doc, true);
                _db.AP_Doc.AddObject(doc);
            }
        }

		private void SaveTrans(string batNbr, string branchID, FormCollection data)
        {
            _objAP = _db.AP_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            if (_objAP == null) _objAP = new AP_Setup();
        
            foreach (var trans in _lstTrans)
            {

                var transDB = _db.AP_Trans.FirstOrDefault(p =>
                                p.BatNbr == _objBatch.BatNbr && p.RefNbr == _objBatch.RefNbr &&
                                p.BranchID == _objBatch.BranchID && p.LineRef == trans.LineRef);

                if (transDB != null)
                {
                    if (transDB.tstamp.ToHex() != trans.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    UpdateTrans(transDB, trans,data, false);
                }
                else
                {
                    transDB = new AP_Trans();
					UpdateTrans(transDB, trans, data, true);
                    _db.AP_Trans.AddObject(transDB);
                }
              
            }
        }
        #endregion

        #region Delete
        [HttpPost]
        public ActionResult DeleteHeader(FormCollection data, string batNbr, string branchID)
        {
            try
            {
                var access = Session["AP10200"] as AccessRight;
                if (data["cboStatus"] != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { batNbr });
                }
                if ((batNbr.PassNull() == string.Empty && !access.Insert) || (batNbr.PassNull() != string.Empty && !access.Update ))
                {
                    throw new MessageException(MessageType.Message, "728");
                }

                var recordTopBatch = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.Module == "AP");
                if (recordTopBatch != null)
                {
                    var recordBotAP_Doc = _db.AP_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                    if (recordBotAP_Doc != null)
                    {
                        for (int k = 0; k < recordBotAP_Doc.Count; k++)
                        {
                            _db.AP_Doc.DeleteObject(recordBotAP_Doc[k]);
                            var recordGridTrans = _db.AP_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                            if (recordGridTrans != null)
                            {
                                for (int i = 0; i < recordGridTrans.Count; i++)
                                {
                                    _db.AP_Trans.DeleteObject(recordGridTrans[i]);
                                }
                            }
                        }
                    }
                    _db.Batches.DeleteObject(recordTopBatch);
                }

                _db.SaveChanges();
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
        public ActionResult DeleteTrans(FormCollection data)
        {
            try
            {
                var access = Session["AP10200"] as AccessRight;

              
                string branchID = data["txtBranchID"];
                string batNbr = data["cboBatNbr"];

                if (data["cboStatus"] != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { batNbr });
                }
                if ((batNbr.PassNull() == string.Empty && !access.Insert) || (batNbr.PassNull() != string.Empty && !access.Update))
                {
                    throw new MessageException(MessageType.Message, "728");
                }
				string lineRef = Util.PassNull(data["lineRefSel"]);

                var lstTrans = _db.AP_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                var trans = lstTrans.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.LineRef == lineRef);

                if (trans != null)
                {
                    

                    lstTrans.Remove(trans);
                    _db.AP_Trans.DeleteObject(trans);
                }
                var batch = _db.Batches.FirstOrDefault(p => p.BatNbr == batNbr && p.BranchID == branchID);
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

                var doc = _db.AP_Doc.FirstOrDefault(p => p.BatNbr == batNbr && p.BranchID == branchID);
                if (doc != null)
                {
                    double totAmt = 0;
                    foreach (var item in lstTrans)
                    {
                        totAmt += item.TranAmt;
                    }
                    doc.DocBal = totAmt;
                    doc.OrigDocAmt = totAmt;
                    doc.LUpd_DateTime = DateTime.Now;
                    doc.LUpd_Prog = _screenNbr;
                    doc.LUpd_User = _userName;
                }

                _db.SaveChanges();

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
        #endregion

        #region Orther Function
		private void UpdateBatch(Batch batch, bool isNew)
        {
            if (isNew)
            {
                batch.BatNbr = _objBatch.BatNbr;
                batch.BranchID = _objBatch.BranchID;
                batch.RefNbr = _objBatch.RefNbr;
                batch.Module = "AP";
                batch.EditScrnNbr = "AP10200";
                batch.JrnlType = "AP";
                batch.Crtd_DateTime = DateTime.Now;
                batch.Crtd_Prog = _screenNbr;
                batch.Crtd_User = Current.UserName;
                batch.OrigBranchID = "";
            }

            if (!string.IsNullOrEmpty(_handle))
            {
                if (_handle == "R")
                {
                    batch.Rlsed = 1;
                    batch.Status = "C"; 
                }
                else if (_handle == "N")
                {
                    batch.Rlsed = 0;
                    batch.Status = "H";
                }
            }

			batch.TotAmt = Convert.ToDouble(_form["dteCuryCrTot"]); // IntRefNbr
            batch.DateEnt = Convert.ToDateTime(_form["dteDocDate"]);
			batch.IntRefNbr = (_form["IntRefNbr"]).PassNull();
            batch.ReasonCD = _form["ReasonCD"];
            batch.LUpd_DateTime = DateTime.Now;
            batch.LUpd_Prog = _screenNbr;
            batch.LUpd_User = Current.UserName;
             

        }

        private void UpdateDoc(AP_Doc d, bool isNew)
        {
            if (isNew)
            {
                d.BranchID = _objAP_Doc.BranchID;
                d.BatNbr = _objAP_Doc.BatNbr;
                d.NoteID = 0;
                d.RefNbr = _objAP_Doc.RefNbr;
                d.DocType = _objAP_Doc.DocType;
                d.VendID = _objAP_Doc.VendID;

                d.Crtd_DateTime = DateTime.Now;
                d.Crtd_Prog = _screenNbr;
                d.Crtd_User = Current.UserName;
            }
            if (!string.IsNullOrEmpty(_handle))
            {
                if (_handle == "R")
                {
                    d.Rlsed = 1;
                  
                }
                else if (_handle == "N")
                {
                    d.Rlsed = 0;
                }
            }
            d.DocBal = _objAP_Doc.DocBal;
            d.DocDate = _objAP_Doc.DocDate;
            d.DocDesc = _objAP_Doc.DocDesc;
            
            d.InvcNbr = _objAP_Doc.InvcNbr;
            d.DiscDate = _objAP_Doc.DocDate;
            d.DueDate = _objAP_Doc.DocDate;
            d.InvcDate = _objAP_Doc.DocDate;
            d.InvcNote = "";
            d.Terms = "";
            d.TaxId00 = "";
            d.TaxId01 = "";
            d.TaxId02 = "";
            d.TaxId03 = "";
            d.RcptNbr = "";
            d.OrigDocAmt = _objAP_Doc.OrigDocAmt;
            d.PONbr = _objAP_Doc.PONbr;

            
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
        }

		private void UpdateTrans(AP_Trans t, AP10200_pgLoadGridTrans_Result s, FormCollection data, bool isNew)
        {
			//string doctype = 
            if (isNew)
            {
                t.BatNbr = _objBatch.BatNbr;
                t.BranchID = _objBatch.BranchID;
                t.RefNbr = _objBatch.RefNbr;
                t.LineRef = s.LineRef;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
            }
			t.JrnlType = "AP";
			t.LineType = "N";
			t.TranType = data["DocType"];
			t.VendID = data["VendID"];
			t.VendName = data["VendName"];
			t.Addr = data["txtAddr"];
			t.TranDate = Convert.ToDateTime(_form["dteDocDate"]).ToDateShort() ;
            t.TranAmt = s.TranAmt;
            t.TranDesc = s.TranDesc;
            t.InvcDate = DateTime.Now;
          //  t.TranDate = DateTime.Now;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
        }

        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AP10200_ppAPNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AP10200_ppAPNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }
        #endregion
    }
}

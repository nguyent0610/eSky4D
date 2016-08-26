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

namespace AP10300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AP10300Controller : Controller
    {
        private string _screenNbr = "AP10300";
        private string _userName = Current.UserName;
        private string _branchID = "";
        AP10300Entities _db = Util.CreateObjectContext<AP10300Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        private FormCollection _form;
        private Batch _objBatch;
        private AP_Adjust _objAPAdjust;
        private List<AP10300_pgLoadGridAdjg_Result> _lstAdjusting;
        private List<AP10300_pgLoadGridAdjd_Result> _lstAdjusted;
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

        public ActionResult GetAP_Adjust(String branchID, String batNbr, String refNbr)
        {
            //var lst = _db.AP_Doc.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RefNbr == refNbr).ToList();
            var lst = _db.AP_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
            return this.Store(lst);
        }

        public ActionResult GetAdjusting(String batNbr, String branchID, String vendID, String docType)
        {
            var lst = _db.AP10300_pgLoadGridAdjg(batNbr, branchID, vendID, docType);

            return this.Store(lst);
        }

        public ActionResult GetAdjusted(String batNbr, String branchID, String vendID)
        {
            var lst = _db.AP10300_pgLoadGridAdjd(batNbr, branchID, vendID);

            return this.Store(lst);
        }
        #endregion

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

        #region Save
        private void SaveData(FormCollection data)
        {

			
            string batNbr = data["cboBatNbr"];
            string branchID = data["txtBranchID"];

            var access = Session["AP10300"] as AccessRight;
          
            var batchHander = new StoreDataHandler(data["batch"]);
            _objBatch = batchHander.ObjectData<Batch>().FirstOrDefault();

            _handle = data["cboHandle"];
            _objBatch.Status = _objBatch.Status == string.Empty ? "H" : _objBatch.Status;

            var docHander = new StoreDataHandler(data["adjust"]);
            _objAPAdjust = docHander.ObjectData<AP_Adjust>().FirstOrDefault();

			if (_db.AP10100_ppCheckCloseDate(branchID, Convert.ToDateTime(_form["txtDocDate"]).ToDateShort()).FirstOrDefault() == "0")
				throw new MessageException(MessageType.Message, "301");
            if (_lstAdjusting == null)
            {
                var transHandler2 = new StoreDataHandler(data["lstAdjusting"]);
                _lstAdjusting = transHandler2.ObjectData<AP10300_pgLoadGridAdjg_Result>().ToList();

            }

            if (_lstAdjusted == null)
            {
                var transHandler = new StoreDataHandler(data["lstAdjusted"]);
                _lstAdjusted = transHandler.ObjectData<AP10300_pgLoadGridAdjd_Result>().ToList();
            }

             SaveBatch(batNbr, branchID);
             SaveAdjust(batNbr, branchID);

            _db.SaveChanges();

            if (_handle != "N")
            {
                DataAccess dal = Util.Dal();
                APProcess.AP ap = new APProcess.AP(_userName, _screenNbr, dal);
                try
                {
                    if (_handle == "R" )
                    {
                        if (!access.Release) // not realse
                             throw new MessageException(MessageType.Message, "725");
                        dal.BeginTrans(IsolationLevel.ReadCommitted);

                        ap.AP10300_Release(_objBatch.BranchID, _objBatch.BatNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                    else if (_handle == "C" || _handle == "V")
                    {
                        if (!access.Release)
                            throw new MessageException(MessageType.Message,"725");
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!ap.AP10300_Cancel(_objBatch.BranchID, _objBatch.BatNbr))
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
            else
            {
                _objBatch.BatNbr = functionBatNbrIfNull(branchID);
               _objBatch.RefNbr = functionRefNbrIfNull(branchID);
                _objBatch.BranchID = branchID;
                batch = new Batch();
                UpdateBatch(batch, true);
                _db.Batches.AddObject(batch);
            }
        }

        private void SaveAdjust(string batNbr, string branchID)
        {
			_objAP = _db.AP_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
			if (_objAP == null)
				_objAP = new AP_Setup();
			List<BuildDataTableAdjust> dt = new List<BuildDataTableAdjust>();
			double dblPaid = 0;
			double dblPayment = 0;
			bool blnNewPaymentRow = false;
			string strAdjgBatNbr = "";
			string strAdjgRefNbr = "";
			Int32 i = default(Int32);
			var lst = _db.AP_Adjust.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
			foreach (var obj in lst)
			{
				_db.AP_Adjust.DeleteObject(obj);
			}
			//Check payment document
			for (i = 0; i <= this._lstAdjusting.Count - 1; i++)
			{
				var objAP_Adjusting = _lstAdjusting[i];
				if (objAP_Adjusting.Payment.Value > 0)
				{

					var lstAP10300_IsExistAP_Adjustg_Results = _db.AP10300_ppIsExistAP_Adjustg(_objBatch.BatNbr, _objBatch.BranchID, objAP_Adjusting.BatNbr, objAP_Adjusting.RefNbr).ToList();
						if (lstAP10300_IsExistAP_Adjustg_Results.Count() > 0)
						{
							throw new MessageException(MessageType.Message, "19");
						}
				}
			}
			if (this._lstAdjusted.Where(p => p.Payment.Value > 0).Count() == 0)
			{
				throw new MessageException(MessageType.Message, "1000", "", new string[] { Util.GetLang("ASSIGNEDDOC") });
			}

			//Check paid document
			for (i = 0; i <= this._lstAdjusted.Count - 1; i++)
			{
				var objAP_Adjusted = _lstAdjusted[i];
				if (objAP_Adjusted.Payment.Value > 0)
				{
					
						var lstAP10300_IsExistAP_Adjustd_Results = _db.AP10300_ppIsExistAP_Adjustd(_objBatch.BatNbr, _objBatch.BranchID, objAP_Adjusted.BatNbr, objAP_Adjusted.RefNbr).ToList();
							
						if (lstAP10300_IsExistAP_Adjustd_Results.Count() > 0)
						{
							throw new MessageException(MessageType.Message, "19");
							
						}
				}
			}

			//Fill Payment to datatable
			for (i = 0; i <= this._lstAdjusting.Count - 1; i++)
			{
				var objAP_Adjusting = _lstAdjusting[i];
				if (objAP_Adjusting.Payment.Value > 0)
				{
					var row = new BuildDataTableAdjust();
					row.AdjgBatNbr = objAP_Adjusting.BatNbr;
					row.AdjgRefNbr = objAP_Adjusting.RefNbr;
					row.AdjgAmt = objAP_Adjusting.Payment.Value;

					dt.Add(row);
				}
			}

			//Fill paid to datatable
			for (i = 0; i <= this._lstAdjusted.Count - 1; i++)
			{
				var objAP_Adjusted = _lstAdjusted[i];
				if (objAP_Adjusted.Payment.Value > 0)
				{
					dblPaid = objAP_Adjusted.Payment.Value;

					blnNewPaymentRow = false;
					//dv = dt.DefaultView;
					//dv.Sort = "AdjgBatNbr, AdjgRefNbr asc";
					foreach (BuildDataTableAdjust rowv_loopVariable in dt)
					{
						var rowv = rowv_loopVariable;
						if (rowv.AdjgAmt > 0)
						{
							if (rowv.AdjgAmt > dblPaid)
							{
								rowv.AdjgAmt = rowv.AdjgAmt - dblPaid;
								rowv.AdjAmt = dblPaid;
								rowv.AdjdBatNbr = objAP_Adjusted.BatNbr;
								rowv.AdjdRefNbr = objAP_Adjusted.RefNbr;
								rowv.AdjdDocType = objAP_Adjusted.DocType;
								dblPaid = 0;
								blnNewPaymentRow = true;
								dblPayment = rowv.AdjgAmt;
								rowv.AdjgAmt = rowv.AdjgAmt - dblPaid;//0;
								strAdjgBatNbr = rowv.AdjgBatNbr;
								strAdjgRefNbr = rowv.AdjgRefNbr;
							}
							else
							{
								rowv.AdjAmt = rowv.AdjgAmt;
								dblPaid = dblPaid - rowv.AdjgAmt;
								rowv.AdjgAmt = 0;
								rowv.AdjdBatNbr = objAP_Adjusted.BatNbr;
								rowv.AdjdRefNbr = objAP_Adjusted.RefNbr;
								rowv.AdjdDocType = objAP_Adjusted.DocType;
							}
						}

						if (dblPaid == 0)
						{
							break; // TODO: might not be correct. Was : Exit For
						}
					}

					if (blnNewPaymentRow)
					{
						var row = new BuildDataTableAdjust();
						row.AdjgBatNbr = strAdjgBatNbr;
						row.AdjgRefNbr = strAdjgRefNbr;
						row.AdjgAmt = dblPayment;

						dt.Add(row);
					}
				}
			}

			//Insert Adjust
			foreach (var row_loopVariable in dt)
			{
				var row = row_loopVariable;
				//
				if (!string.IsNullOrEmpty(row.AdjgBatNbr) && !string.IsNullOrEmpty(row.AdjdBatNbr) && (row.AdjAmt > 0))
				{
					Updating_AP_Adjust(row);
				}
			}
			//  lst = (from p in _db.AP_Adjust where p.BranchID == _objBatch.BranchID && p.BatNbr == txtBatNbr.Text select p).ToList();
			//var lst = _db.AP_Adjust.(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.AdjdRefNbr == dRefNbr && p.AdjgRefNbr == gRefNbr);
					
			//int n = 0;
			//int m = 0;
			//int count = _lstAdjusting.Where(p => p.Selected == true).Count(); ;
			//int lenAdjusted = _lstAdjusted.Count();
			//int lenAdjusting = _lstAdjusting.Count();
			

			//while (true)
			//{
			//	string dRefNbr = "";
			//	string gRefNbr = "";
			//	if (n < lenAdjusted)
			//	{
				
			//			dRefNbr = _lstAdjusted[n].RefNbr;
					
			//	}

			//	if (n < lenAdjusting )
			//	{
			//		//if (_lstAdjusting[n].Selected==true)
			//			gRefNbr = _lstAdjusting[n].RefNbr;
					
			//	}
				

			//	if (string.IsNullOrEmpty(dRefNbr) )
			//		break;
			//	if (_lstAdjusted[n].Selected == true)
			//	{
			//		var objAdjust = new AP_Adjust();
			//		if (!string.IsNullOrEmpty(gRefNbr))
			//			objAdjust = _db.AP_Adjust.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.AdjdRefNbr == dRefNbr && p.AdjgRefNbr == gRefNbr);
			//		else
			//			objAdjust = _db.AP_Adjust.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.AdjdRefNbr == dRefNbr && p.AdjgRefNbr == _objBatch.RefNbr);


			//		if (objAdjust == null) // null  // isNew 
			//		{
			//			objAdjust = new AP_Adjust();
			//			objAdjust.ResetET();
			//			if (string.IsNullOrEmpty(gRefNbr))
			//			{
			//				UpdateAdjust(objAdjust, _lstAdjusted[n], null, true);
			//			}
			//			else
			//			{
			//				UpdateAdjust(objAdjust, _lstAdjusted[n], _lstAdjusting[n], true);
			//			}
			//			if (objAdjust.AdjAmt > 0 && !string.IsNullOrEmpty(objAdjust.AdjgBatNbr) && !string.IsNullOrEmpty(objAdjust.AdjdBatNbr))
			//				_db.AP_Adjust.AddObject(objAdjust);
			//		}
			//		else
			//		{
			//			if (string.IsNullOrEmpty(gRefNbr))
			//			{
			//				UpdateAdjust(objAdjust, _lstAdjusted[n], null, true);
			//			}
			//			else
			//			{
			//				UpdateAdjust(objAdjust, _lstAdjusted[n], _lstAdjusting[n], false);
			//			}
			//		}
			//	}
			//	n++;
			//}
			
          
        }
		//private void Save_AP_Adjust(BuildDataTableAdjust row)
		//{
		//	Updating_AP_Adjust(row);
		//}

		private void Updating_AP_Adjust(BuildDataTableAdjust row)
		{
			try
			{
				//_objBatch.BatNbr, _objBatch.BranchID, objAP_Adjusting.BatNbr, objAP_Adjusting.RefNbr
				var objAP_Ad = new AP_Adjust();//clsApp.ResetAP_Adjust();
				objAP_Ad.BranchID = _objBatch.BranchID;
				objAP_Ad.BatNbr = _objBatch.BatNbr;//strBatNbr;
				objAP_Ad.AdjgBatNbr = row.AdjgBatNbr;
				objAP_Ad.AdjgRefNbr = row.AdjgRefNbr;
				objAP_Ad.AdjdRefNbr = row.AdjdRefNbr;
				objAP_Ad.AdjdBatNbr = row.AdjdBatNbr;
				objAP_Ad.AdjAmt = row.AdjAmt;
				objAP_Ad.AdjdDocType = row.AdjdDocType;
				objAP_Ad.AdjDiscAmt = 0;
				objAP_Ad.AdjgDocDate = _objAPAdjust.AdjgDocDate;//((DateTime)this.cboDocDate.Value).Short();
				objAP_Ad.AdjgDocType = _objAPAdjust.AdjgDocType;//(cboDocType.SelectedItem as psys_LoadLang_Result).Code;
				objAP_Ad.Reversal = "";
				objAP_Ad.VendID = _objAPAdjust.VendID;//(cboVendID.SelectedItem as ppv_vendor_active_Result).VendID;
				objAP_Ad.LUpd_DateTime = DateTime.Now;
				objAP_Ad.LUpd_Prog = _screenNbr;
				objAP_Ad.LUpd_User = Current.UserName;
				objAP_Ad.Crtd_DateTime = DateTime.Now;
				objAP_Ad.Crtd_Prog = _screenNbr;
				objAP_Ad.Crtd_User = Current.UserName;
				_db.AP_Adjust.AddObject(objAP_Ad);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}
        #endregion

        #region Delete
        [HttpPost]
        public ActionResult Delete(FormCollection data, string batNbr, string branchID)
        {
            try
            {
                var access = Session["AP10300"] as AccessRight;
                if (data["cboStatus"] != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { batNbr });
                }
                if (batNbr.PassNull() != string.Empty && !access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }

                var recordTopBatch = _db.Batches.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr && p.Module == "AP");
                if (recordTopBatch != null)
                {
                    var recordAP_Adjust = _db.AP_Adjust.Where(p => p.BranchID == branchID && p.BatNbr == batNbr).ToList();
                    foreach (var item in recordAP_Adjust)
                    {
                        _db.AP_Adjust.DeleteObject(item);
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
                batch.EditScrnNbr = "AP10300";
                batch.JrnlType = "AP";
               
                batch.Crtd_DateTime = DateTime.Now;
                batch.Crtd_Prog = _screenNbr;
                batch.Crtd_User = Current.UserName;
                batch.OrigBranchID = "";
            }

            batch.Status = _objBatch.Status;
            batch.TotAmt = Convert.ToDouble(_form["dteCuryCrTot"]); // 
            batch.DateEnt = Convert.ToDateTime(_form["txtDocDate"]);
           
            batch.EditScrnNbr = _screenNbr;
            batch.NoteID = 0;
            batch.Descr = _form["txtDescr"];
            batch.LUpd_DateTime = DateTime.Now;
            batch.LUpd_Prog = _screenNbr;
            batch.LUpd_User = Current.UserName;


        }

        private void UpdateAdjust(AP_Adjust d, AP10300_pgLoadGridAdjd_Result adjusted, AP10300_pgLoadGridAdjg_Result adjusting, bool isNew)
        {
            if (isNew)
            {
                d.BranchID = _objBatch.BranchID;
                d.BatNbr = _objBatch.BatNbr;

                d.AdjdRefNbr = adjusted.RefNbr;
                d.AdjdBatNbr = adjusted.BatNbr;
                d.AdjdDocType = adjusted.DocType;

                 d.AdjgDocType = _objAPAdjust.AdjgDocType;
                if (adjusting != null)
                {
                    d.AdjgRefNbr = adjusting.RefNbr;
                    d.AdjgBatNbr = adjusting.BatNbr;
                }
				else
				{
					d.AdjgRefNbr = _objBatch.RefNbr;
				}

                d.VendID = _objAPAdjust.VendID;

                d.Crtd_DateTime = DateTime.Now;
                d.Crtd_Prog = _screenNbr;
                d.Crtd_User = Current.UserName;
            }

            d.AdjAmt = adjusted.Payment.ToDouble();
            d.AdjDiscAmt = 0;
            d.AdjgDocDate = _objAPAdjust.AdjgDocDate;
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
        }

       

        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.APNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.APNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }
        #endregion
    }
	public class BuildDataTableAdjust
	{
		public string AdjgBatNbr { get; set; }
		public string AdjgRefNbr { get; set; }
		public string AdjdBatNbr { get; set; }
		public string AdjdRefNbr { get; set; }
		public string AdjdDocType { get; set; }
		public double AdjAmt { get; set; }
		public double AdjgAmt { get; set; }
	}
}

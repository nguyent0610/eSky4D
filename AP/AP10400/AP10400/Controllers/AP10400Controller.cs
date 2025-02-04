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
//using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using HQFramework.Common;
using HQ.eSkySys;

namespace AP10400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AP10400Controller : Controller
    {
        private string _screenNbr = "AP10400";
        private string _userName = Current.UserName;
        AP10400Entities _db = Util.CreateObjectContext<AP10400Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        //AP10400SysEntities _AP10400sys = Util.CreateObjectContext<AP10400SysEntities>(true,"Sys");
        private JsonResult _logMessage;
		string _handle = "";
		string _status = "";
		string _batNbr = "";
		string _refNbr = "";
		string _branchID = "";

        public ActionResult Index(string branchID)
        {
            Util.InitRight(_screenNbr);
            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);

            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }

            if (branchID == null) branchID = Current.CpnyID;

            //var userDft = _db.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID == branchID);

            //ViewBag.INSite = userDft == null ? "" : userDft.INSite;
            ViewBag.BranchID = branchID;

            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetHeader(string BranchID, string BatNbr)
        {
            return this.Store(_db.AP10400_pdHeader(BranchID, BatNbr).FirstOrDefault());
        }

		//public ActionResult GetDetail(string BranchID, string BatNbr)
		//{
		//	return this.Store(_db.AP10400_pgLoadGridTrans(BranchID, BatNbr).ToList());
		//}

		public ActionResult GetDetail(String batNbr, String branchID, String vendID, String refNbr, DateTime fromDate, DateTime toDate, String dateType, String isGridF3)
		{
			return this.Store(_db.AP10400_pgLoadGridTrans(batNbr, branchID, vendID, refNbr, fromDate, toDate, dateType, isGridF3).ToList());

			
		}

        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AP10400_ppAPNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AP10400_ppAPNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }


        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
				var acc = Session["AP10400"] as AccessRight;
				
                var BranchID = data["txtBranchID"].PassNull();
				var Handle = data["cboHandle"].PassNull();
                var BatNbr = data["cboBatNbr"].PassNull();
                var RefNbr = data["cboRefNbr"].PassNull();
				var ReasonCD = data["cboBankAcct"].PassNull();
				var toAmt = data["txtCuryCrTot"].PassNull();
				_status = data["cboStatus"].PassNull();
				_handle = data["cboHandle"].PassNull() == "" ? _status : data["cboHandle"].PassNull();
				_batNbr = BatNbr;
				_refNbr = RefNbr;
				_branchID = BranchID;

				if ((_status == "U" || _status == "C") && (_handle == "C" || _handle == "V"))
				{

					if (_handle == "V" || _handle == "C")
					{
						if ((_handle == "V" || _handle == "C") && !acc.Release)
						{
							throw new MessageException(MessageType.Message, "725");
						}
					}
				}
				else if (_status == "H")
				{
					if (_handle == "R" && !acc.Release)
					{
						throw new MessageException(MessageType.Message, "737");
					}
				}
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstHeader"]);
                var curHeader = dataHandler1.ObjectData<AP10400_pdHeader_Result>().FirstOrDefault();
                StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
                ChangeRecords<AP10400_pgLoadGridTrans_Result> lstgrd = dataHandlerGrid.BatchObjectData<AP10400_pgLoadGridTrans_Result>();

				StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstAp_Adjust"]);
				var lstAp_Adjust = dataHandler2.ObjectData<AP10400_pgLoadGridTrans_Result>().ToList();
				if (_db.AP10100_ppCheckCloseDate(BranchID.PassNull(), curHeader.DocDate.ToDateShort()).FirstOrDefault() == "0"
					&&  _status=="H")
					throw new MessageException(MessageType.Message, "301");
				#region Save Header
                var headerBatch = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AP" && p.EditScrnNbr == "AP10400");
                var headerDoc = _db.AP_Doc.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr);
                if (headerBatch != null)
                {
                    if (headerBatch.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        headerBatch.IntRefNbr = curHeader.IntRefNbr;
						//headerBatch.ReasonCD = curHeader.ReasonCD;
						headerBatch.Descr = data["txtDocDescr"].PassNull();//curHeader.Descr;
                       // headerBatch.TotAmt = Convert.ToDouble(curHeader.TotAmt);
						headerBatch.TotAmt = Convert.ToDouble(toAmt);
						headerBatch.LUpd_DateTime = DateTime.Now;
						headerBatch.LUpd_User = Current.UserName;
						headerBatch.LUpd_Prog = _screenNbr;
						headerBatch.ReasonCD = ReasonCD;
						if (Handle == "R")
						{
							headerBatch.Rlsed = 1;
							headerBatch.Status = "C";
						}
                        UpdatingFormBotAP_Doc(ref headerDoc, curHeader, Handle);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    headerBatch = new Batch();
                    headerBatch.ResetET();
                    headerBatch.BranchID = BranchID;
                    headerBatch.BatNbr = functionBatNbrIfNull(BranchID);
                    headerBatch.RefNbr = functionRefNbrIfNull(BranchID);
                    headerBatch.Module = "AP";
                    headerBatch.JrnlType = "AP"; 
                    headerBatch.EditScrnNbr = "AP10400";
                    headerBatch.TotAmt = curHeader.TotAmt;
                    headerBatch.DateEnt = curHeader.DocDate;
                    headerBatch.OrigBranchID = "";
                    //if (Handle == "R")
                    //{
                    //    headerBatch.Rlsed = 1;
                    //    headerBatch.Status = "C";
                    //}
                    //else 
                    //if (Handle == "N" || Handle == "")
                    //{
						headerBatch.Rlsed = 0;
						headerBatch.Status = "H";
                    //}
                    headerBatch.IntRefNbr = curHeader.IntRefNbr;
                    headerBatch.ReasonCD = curHeader.ReasonCD;
                    headerBatch.TotAmt = curHeader.TotAmt;
                    headerBatch.TotAmt = curHeader.TotAmt;
                    headerBatch.TotAmt = curHeader.TotAmt;
					headerBatch.Descr = data["txtDocDescr"].PassNull();

                    headerBatch.Crtd_DateTime = DateTime.Now;
                    headerBatch.Crtd_Prog = _screenNbr;
                    headerBatch.Crtd_User = Current.UserName;
                    headerBatch.LUpd_DateTime = DateTime.Now;
                    headerBatch.LUpd_Prog = _screenNbr;
                    headerBatch.LUpd_User = Current.UserName;
					

                    _db.Batches.AddObject(headerBatch);

                    headerDoc = new AP_Doc();
                    headerDoc.ResetET();
                    headerDoc.BranchID = BranchID;
                    headerDoc.BatNbr = headerBatch.BatNbr;
                    headerDoc.RefNbr = headerBatch.RefNbr;
					headerDoc.DocType = "HC";// curHeader.DocType;
                    headerDoc.Crtd_DateTime = DateTime.Now;
                    headerDoc.Crtd_Prog = _screenNbr;
                    headerDoc.Crtd_User = Current.UserName;
                    UpdatingFormBotAP_Doc(ref headerDoc, curHeader, Handle);
					headerDoc.InvcDate = DateTime.Now.ToDateShort();
                    _db.AP_Doc.AddObject(headerDoc);

                    BatNbr = headerBatch.BatNbr;
                    RefNbr = headerBatch.RefNbr;

                }
                #endregion

				#region Save AP_Adjust
				//foreach (AP10400_pgLoadGridTrans_Result deleted in lstgrd.Deleted)
				//{
				//	var objDelete = _db.AP_Adjust.Where(p => p.BranchID == BranchID
				//										&& p.BatNbr == BatNbr
				//										&& p.RefNbr == RefNbr
				//									  //  && p.LineRef == deleted.LineRef
				//										).FirstOrDefault();
				//	if (objDelete != null)
				//	{
				//		_db.AP_Trans.DeleteObject(objDelete);
				//	}
				//}

				//lstgrd.Created.AddRange(lstgrd.Updated);

				//foreach (AP10400_pgLoadGridTrans_Result curLang in lstgrd.Created)
				//{
				//	if (BranchID.PassNull() == "" || BatNbr.PassNull() == "" || RefNbr.PassNull() == "" //|| curLang.LineRef.PassNull() == ""
				//		) continue;

				//	var lang = _db.AP_Adjust.FirstOrDefault(p => p.BranchID.ToLower() == BranchID.ToLower()
				//										&& p.BatNbr.ToLower() == BatNbr.ToLower()
				//										&& p.RefNbr.ToLower() == RefNbr.ToLower()
				//									   // && p.LineRef.ToLower() == curLang.LineRef.ToLower()
				//										);

				//	if (lang != null)
				//	{
				//		if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
				//		{
				//			UpdatingAP_Trans(lang, curLang, false);
				//		}
				//		else
				//		{
				//			throw new MessageException(MessageType.Message, "19");
				//		}
				//	}
				//	else
				//	{
				//		lang = new AP_Trans();
				//		lang.ResetET();
				//		lang.BranchID = BranchID;
				//		lang.BatNbr = BatNbr;
				//		lang.RefNbr = RefNbr;
				//		lang.JrnlType = "AR";
				//		UpdatingAP_Trans(lang, curLang, true);
				//		_db.AP_Trans.AddObject(lang);
				//	}
				//}
				foreach (AP10400_pgLoadGridTrans_Result deleted in lstgrd.Deleted)
				{
					var record = _db.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.AdjdRefNbr == deleted.RefNbr && p.AdjgRefNbr == RefNbr).FirstOrDefault();
					if (record != null)
					{
						_db.AP_Adjust.DeleteObject(record);
					}

				}
				lstgrd.Created.AddRange(lstgrd.Updated);
				if (lstAp_Adjust.Count() == 0 &&  headerBatch.Status != "C")
				{
					throw new MessageException(MessageType.Message, "1000", "", new string[] { Util.GetLang("Payment") });
				}
				int coutpayment = 0;
				foreach (AP10400_pgLoadGridTrans_Result created in lstAp_Adjust)//lstgrd.Created)
				{
                    if (created.InvcNbr.PassNull() != "" && created.Payment != 0 && _status != "C")
					{
						coutpayment++;
						var checkrecord = _db.AP_Adjust.Where(p => p.BranchID == BranchID && p.AdjdRefNbr == created.RefNbr && p.BatNbr != BatNbr).FirstOrDefault();//&& p.BatNbr == BatNbr&& p.BatNbr == BatNbr && p.AdjgRefNbr == RefNbr

						if (checkrecord != null)
						{
							var checkheaderBatch = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == checkrecord.BatNbr/*BatNbr*/ && p.Module == "AP" && p.EditScrnNbr == "AP10400" && p.Status == "H");
						  if (checkheaderBatch!=null)
							throw new MessageException(MessageType.Message, "20160808", "", new string[] { checkrecord.BatNbr });

						}
						var record = _db.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.AdjdRefNbr == created.RefNbr && p.AdjgRefNbr == RefNbr).FirstOrDefault();
						if (record == null)
						{
							record = new AP_Adjust();
							record.BranchID = BranchID;
							if (BatNbr == "")
							{
								record.AdjgBatNbr = headerBatch.BatNbr;
								record.BatNbr = headerBatch.BatNbr;
							}
							else
							{
								record.BatNbr = BatNbr;
								record.AdjgBatNbr = BatNbr;
							}
							if (RefNbr == "")
							{
								record.AdjgRefNbr = headerBatch.RefNbr;
							}
							else
							{
								record.AdjgRefNbr = RefNbr;
							}

							UpdatingGridAd_Adjust(created, ref record);
							record.Crtd_DateTime = DateTime.Now;
							record.Crtd_Prog = _screenNbr;
							record.Crtd_User = Current.UserName;
							_db.AP_Adjust.AddObject(record);
						}
						else
						{
							UpdatingGridAd_Adjust(created, ref record);
						}
					}
				}
				if (coutpayment == 0 &&  headerBatch.Status != "C")
				{
					throw new MessageException(MessageType.Message, "1000", "", new string[] { Util.GetLang("Payment") });
				}
				//foreach (AP10400_pgLoadGridTrans_Result updated in lstgrd.Updated)
				//{
				//	var record = _db.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.AdjdRefNbr == updated.RefNbr && p.AdjgRefNbr == RefNbr).FirstOrDefault();
				//	if (record != null)
				//	{
				//		if (BatNbr == "")
				//		{
				//			record.AdjgBatNbr = headerBatch.BatNbr;
				//			record.BatNbr = headerBatch.BatNbr;
				//		}

				//		if (RefNbr == "")
				//		{
				//			record.AdjgRefNbr = headerBatch.RefNbr;
				//		}

				//		UpdatingGridAd_Adjust(updated, ref record);
				//	}
				//	else
				//	{
				//		record = new AP_Adjust();
				//		record.BranchID = BranchID;
				//		if (BatNbr == "")
				//		{
				//			record.AdjgBatNbr = headerBatch.BatNbr;
				//			record.BatNbr = headerBatch.BatNbr;
				//		}
				//		else
				//		{
				//			record.BatNbr = BatNbr;
				//			record.AdjgBatNbr = BatNbr;
				//		}
				//		if (RefNbr == "")
				//		{
				//			record.AdjgRefNbr = headerBatch.RefNbr;
				//		}
				//		else
				//		{
				//			record.AdjgRefNbr = RefNbr;
				//		}

				//		UpdatingGridAd_Adjust(updated, ref record);
				//		record.Crtd_DateTime = DateTime.Now;
				//		record.Crtd_Prog = screenNbr;
				//		record.Crtd_User = Current.UserName;
				//		_db.AP_Adjust.AddObject(record);
				//	}
				//}

				#endregion

				//_db.SaveChanges();

				//var justUpdateBatNbr = _db.Batches.Where(p => p.BranchID == BranchID && p.Module == "AP" && p.EditScrnNbr == "AP10400" && p.BatNbr == BatNbr).FirstOrDefault();
				//if (justUpdateBatNbr != null)
				//{
				//	justUpdateBatNbr.TotAmt = Convert.ToDouble(toAmt);
				//	justUpdateBatNbr.ReasonCD = ReasonCD;
				//	//_db.SaveChanges();
				//}


				//#endregion
				////_db.SaveChanges();
				//if (Handle == "R")
				//{
				//	var recordBatNbrUpdate = _db.Batches.Where(p => p.BranchID == BranchID && p.Module == "AP" && p.EditScrnNbr == "AP10400" && p.BatNbr == BatNbr).FirstOrDefault();
				//	var recordRefNbrUpdate = _db.AP_Doc.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.RefNbr == RefNbr).FirstOrDefault();
				//	recordBatNbrUpdate.Rlsed = 1;
				//	recordBatNbrUpdate.Status = "C";
				//	recordRefNbrUpdate.Rlsed = 1;
				//	//tmpcatchHandle = "1";
				//}
                _db.SaveChanges();
				_batNbr = BatNbr;
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
						//Save_Batch();
						//	_app.SaveChanges();
						Data_Release();
					}
				}
				_db.SaveChanges();
                return Json(new { success = true, BatNbr = BatNbr });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

		private void UpdatingGridAd_Adjust(AP10400_pgLoadGridTrans_Result s, ref AP_Adjust d)
		{
			d.AdjdBatNbr = s.BatNbr;
			d.AdjdRefNbr = s.RefNbr;
			d.AdjAmt = Convert.ToDouble(s.Payment);
			d.AdjgDocDate = s.DocDate;
			d.AdjgDocType = "HC";
			d.AdjdDocType = s.DocType;// "VO";
			d.VendID = s.VendID;
			d.LUpd_DateTime = DateTime.Now;
			d.LUpd_Prog = _screenNbr;
			d.LUpd_User = Current.UserName;
		}


        private void UpdatingFormBotAP_Doc(ref AP_Doc t, AP10400_pdHeader_Result s, string Handle)
        {
            if (Handle == "R")
                t.Rlsed = 1;
            else if (Handle == "N")
                t.Rlsed = 0;
            t.DocBal = s.DocBal;
            t.DocDate = s.DocDate;
            t.DocDesc = s.DocDesc;
			//t.SlsperId = s.SlsperId;
			//t.CustId = s.CustId;
            t.InvcNbr = s.InvcNbr;
            t.DiscDate = s.DocDate;
            t.DueDate = s.DocDate;
            t.InvcNote = "";
            t.Terms = "";
            t.TaxId00 = "";
            t.TaxId01 = "";
            t.TaxId02 = "";
            t.TaxId03 = "";
            t.OrigDocAmt = s.OrigDocAmt;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
        }

		//private void UpdatingAP_Trans(AP_Trans t, AP10400_pgLoadGridTrans_Result s, bool isNew)
		//{
		//	if (isNew)
		//	{
		//	   // t.LineRef = s.LineRef;
		//		t.Crtd_DateTime = DateTime.Now;
		//		t.Crtd_Prog = _screenNbr;
		//		t.Crtd_User = _userName;
		//	}
		//	//t.TranAmt = s.TranAmt;
		//	//t.TranDesc = s.TranDesc;
		//	t.TranDate = DateTime.Now;
		//	t.LUpd_DateTime = DateTime.Now;
		//	t.LUpd_Prog = _screenNbr;
		//	t.LUpd_User = _userName;

		//}

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string BranchID = data["txtBranchID"].PassNull();
                string BatNbr = data["cboBatNbr"].PassNull();

                var objBatch = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AP");
                if (objBatch != null)
                {
                    _db.Batches.DeleteObject(objBatch);
                }

                var objDoc = _db.AP_Doc.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr);
                if (objDoc != null)
                {
                    _db.AP_Doc.DeleteObject(objDoc);
                }

                var lstAP_Trans = _db.AP_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (var item in lstAP_Trans)
                {
                    _db.AP_Trans.DeleteObject(item);
                }

				var lstAP_Adjust = _db.AP_Adjust.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
				foreach (var item in lstAP_Adjust)
				{
					_db.AP_Adjust.DeleteObject(item);
				}

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

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
						if (!ap.AP10400_Release(_batNbr, _branchID))
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
						if (!ap.AP10400_Cancel(_batNbr, _branchID))
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
    }
}

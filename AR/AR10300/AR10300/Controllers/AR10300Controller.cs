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

namespace AR10300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10300Controller : Controller
    {
        private string _screenNbr = "AR10300";
        private string _userName = Current.UserName;
        AR10300Entities _db = Util.CreateObjectContext<AR10300Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        //AR10300SysEntities _AR10300sys = Util.CreateObjectContext<AR10300SysEntities>(true,"Sys");
        private JsonResult _logMessage;

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

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetHeader(string BranchID, string BatNbr)
        {
            return this.Store(_db.AR10300_pdHeader(BranchID, BatNbr).FirstOrDefault());
        }

        public ActionResult GetDetail(string BranchID, string BatNbr)
        {
            return this.Store(_db.AR10300_pgLoadGridTrans(BranchID, BatNbr).ToList());
        }

        private string functionBatNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AR10300_ppARNumbering(branchID, "BatNbr").FirstOrDefault();
            return recordLastBatNbr;
        }

        private string functionRefNbrIfNull(string branchID)
        {
            var recordLastBatNbr = _db.AR10300_ppARNumbering(branchID, "RefNbr").FirstOrDefault();
            return recordLastBatNbr;
        }


        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                var BranchID = data["txtBranchID"].PassNull();
                var Status = data["cboStatus"].PassNull();
                var Handle = data["cboHandle"].PassNull();
                var BatNbr = data["cboBatNbr"].PassNull();
                var RefNbr = data["cboRefNbr"].PassNull();
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstHeader"]);
                var curHeader = dataHandler1.ObjectData<AR10300_pdHeader_Result>().FirstOrDefault();

                //StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
                //ChangeRecords<AR10300_pgLoadGridTrans_Result> lstgrd = dataHandlerGrid.BatchObjectData<AR10300_pgLoadGridTrans_Result>();

                StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
                var lstgrd = dataHandlerGrid.ObjectData<AR10300_pgLoadGridTrans_Result>() == null ? new List<AR10300_pgLoadGridTrans_Result>() : dataHandlerGrid.ObjectData<AR10300_pgLoadGridTrans_Result>().Where(p => p.TranAmt > 0);

                if (Status == "H")
                {
                    if (_db.AR10300_ppCheckCloseDate(BranchID, curHeader.DocDate.ToDateShort(), "AR10300").FirstOrDefault() == "0")
                        throw new MessageException(MessageType.Message, "301");
                }

                #region Save Header
                var headerBatch = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AR" && p.EditScrnNbr == "AR10300");
                var headerDoc = _db.AR_Doc.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr);
                if (headerBatch != null)
                {
                    if (headerBatch.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        if (Handle == "V")
                        {
                            var objCheckCancel = _db.AR10300_ppCheckForCancel(BranchID, headerBatch.BatNbr, headerBatch.RefNbr).FirstOrDefault();
                            if (objCheckCancel != null)
                            {
                                if (objCheckCancel.Result == "1")
                                    throw new MessageException(MessageType.Message, "201307313", parm: new string[] { BranchID, headerBatch.BatNbr, headerBatch.RefNbr });
                                else
                                {
                                    headerBatch.Status = "V";
                                    headerBatch.Rlsed = -1;
                                    headerBatch.LUpd_DateTime = DateTime.Now;
                                    headerBatch.LUpd_Prog = _screenNbr;
                                    headerBatch.LUpd_User = _userName;

                                    headerDoc.Rlsed = -1;
                                    headerDoc.LUpd_DateTime = DateTime.Now;
                                    headerDoc.LUpd_Prog = _screenNbr;
                                    headerDoc.LUpd_User = _screenNbr;

                                    var objAR_Balances = _db.AR_Balances.FirstOrDefault(p => p.CustID == curHeader.CustId && p.BranchID == BranchID);
                                    if (objAR_Balances != null)
                                    {
                                        objAR_Balances.CurrBal = objAR_Balances.CurrBal + curHeader.TotAmt;
                                        objAR_Balances.LUpd_DateTime = DateTime.Now;
                                        objAR_Balances.LUpd_Prog = _screenNbr;
                                        objAR_Balances.LUpd_User = _screenNbr;
                                    }
                                }
                            }
                        }
                        else
                        {
                            headerBatch.Descr = curHeader.DocDesc;
                            headerBatch.IntRefNbr = curHeader.IntRefNbr;
                            headerBatch.ReasonCD = curHeader.ReasonCD;
                            headerBatch.TotAmt = Convert.ToDouble(curHeader.TotAmt);
                            if (Handle == "R")
                            {
                                headerBatch.Rlsed = 1;
                                headerBatch.Status = "C";

                                var objAR_Balances = _db.AR_Balances.FirstOrDefault(p => p.CustID == curHeader.CustId && p.BranchID == BranchID);
                                if (objAR_Balances != null)
                                {
                                    objAR_Balances.CurrBal = objAR_Balances.CurrBal - curHeader.TotAmt;
                                    objAR_Balances.LUpd_DateTime = DateTime.Now;
                                    objAR_Balances.LUpd_Prog = _screenNbr;
                                    objAR_Balances.LUpd_User = _screenNbr;
                                }
                            }
                            UpdatingFormBotAR_Doc(ref headerDoc, curHeader, Handle);
                        }
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
                    headerBatch.Module = "AR";
                    headerBatch.JrnlType = "AR"; 
                    headerBatch.EditScrnNbr = "AR10300";
                    headerBatch.TotAmt = curHeader.TotAmt;
                    headerBatch.DateEnt = curHeader.DocDate;
                    headerBatch.OrigBranchID = "";
                    headerBatch.Descr = curHeader.DocDesc;
                    if (Handle == "R")
                    {
                        headerBatch.Rlsed = 1;
                        headerBatch.Status = "C";

                        var objAR_Balances = _db.AR_Balances.FirstOrDefault(p => p.CustID == curHeader.CustId && p.BranchID == BranchID);
                        if (objAR_Balances != null)
                        {
                            objAR_Balances.CurrBal = objAR_Balances.CurrBal - curHeader.TotAmt;
                            objAR_Balances.LUpd_DateTime = DateTime.Now;
                            objAR_Balances.LUpd_Prog = _screenNbr;
                            objAR_Balances.LUpd_User = _screenNbr;
                        }
                    }
                    else if (Handle == "N" || Handle == "")
                    {
                        headerBatch.Rlsed = 0;
                        headerBatch.Status = "H";
                    }
                    headerBatch.IntRefNbr = curHeader.IntRefNbr;
                    headerBatch.ReasonCD = curHeader.ReasonCD;
                    headerBatch.TotAmt = curHeader.TotAmt;

                    headerBatch.Crtd_DateTime = DateTime.Now;
                    headerBatch.Crtd_Prog = _screenNbr;
                    headerBatch.Crtd_User = _userName;
                    headerBatch.LUpd_DateTime = DateTime.Now;
                    headerBatch.LUpd_Prog = _screenNbr;
                    headerBatch.LUpd_User = _userName;

                    _db.Batches.AddObject(headerBatch);

                    headerDoc = new AR_Doc();
                    headerDoc.ResetET();
                    headerDoc.BranchID = BranchID;
                    headerDoc.BatNbr = headerBatch.BatNbr;
                    headerDoc.RefNbr = headerBatch.RefNbr;
                    
                    headerDoc.Crtd_DateTime = DateTime.Now;
                    headerDoc.Crtd_Prog = _screenNbr;
                    headerDoc.Crtd_User = _userName;
                    UpdatingFormBotAR_Doc(ref headerDoc, curHeader, Handle);

                    _db.AR_Doc.AddObject(headerDoc);

                    BatNbr = headerBatch.BatNbr;
                    RefNbr = headerBatch.RefNbr;
                }
                #endregion

                #region Save AR_Trans
                var lstOldAR_Trans = _db.AR_Trans.Where(p => p.BranchID == BranchID
                                                        && p.BatNbr == BatNbr
                                                        && p.RefNbr == RefNbr).ToList();

                foreach (var objold in lstOldAR_Trans)
                {
                    if (lstgrd.Where(p => p.LineRef == objold.LineRef).FirstOrDefault() == null)
                    {
                        _db.AR_Trans.DeleteObject(objold);
                    }
                }

                foreach (var item in lstgrd)
                {
                    if (BranchID.PassNull() == "" || BatNbr.PassNull() == "" || RefNbr.PassNull() == "" || item.LineRef.PassNull() == "") continue;

                    var lang = _db.AR_Trans.FirstOrDefault(p => p.BranchID.ToLower() == BranchID.ToLower()
                                                       && p.BatNbr.ToLower() == BatNbr.ToLower()
                                                       && p.RefNbr.ToLower() == RefNbr.ToLower()
                                                       && p.LineRef.ToLower() == item.LineRef.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == item.tstamp.ToHex())
                        {
                            lang.TranType = headerDoc.DocType;
                            UpdatingAR_Trans(lang, item, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new AR_Trans();
                        lang.ResetET();
                        lang.BranchID = BranchID;
                        lang.BatNbr = BatNbr;
                        lang.RefNbr = RefNbr;
                        lang.JrnlType = "AR";
                        lang.TranType = headerDoc.DocType;
                        UpdatingAR_Trans(lang, item, true);
                        _db.AR_Trans.AddObject(lang);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, BatNbr = BatNbr });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingFormBotAR_Doc(ref AR_Doc t, AR10300_pdHeader_Result s, string Handle)
        {
            if (Handle == "R")
                t.Rlsed = 1;
            else if (Handle == "N")
                t.Rlsed = 0;
            t.DocBal = s.DocBal;
            t.DocDate = s.DocDate;
            t.DocDesc = s.DocDesc;
            t.DocType = s.DocType;
            t.SlsperId = s.SlsperId;
            t.CustId = s.CustId;
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
            t.LUpd_User = _userName;
        }

        private void UpdatingAR_Trans(AR_Trans t, AR10300_pgLoadGridTrans_Result s, bool isNew)
        {
            if (isNew)
            {
                t.LineRef = s.LineRef;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.TranAmt = s.TranAmt;
            t.TranDesc = s.TranDesc;
            t.TranDate = DateTime.Now;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string BranchID = data["txtBranchID"].PassNull();
                string BatNbr = data["cboBatNbr"].PassNull();

                var objBatch = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AR");
                if (objBatch != null)
                {
                    _db.Batches.DeleteObject(objBatch);
                }

                var objDoc = _db.AR_Doc.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr);
                if (objDoc != null)
                {
                    _db.AR_Doc.DeleteObject(objDoc);
                }

                var lstAR_Trans = _db.AR_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (var item in lstAR_Trans)
                {
                    _db.AR_Trans.DeleteObject(item);
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
    }
}

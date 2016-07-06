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
                var Handle = data["cboHandle"].PassNull();
                var BatNbr = data["cboBatNbr"].PassNull();
                var RefNbr = data["cboRefNbr"].PassNull();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstHeader"]);
                var curHeader = dataHandler1.ObjectData<AR10300_pdHeader_Result>().FirstOrDefault();
                StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstgrd"]);
                ChangeRecords<AR10300_pgLoadGridTrans_Result> lstgrd = dataHandlerGrid.BatchObjectData<AR10300_pgLoadGridTrans_Result>();
                
                #region Save Header
                var headerBatch = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "AR" && p.EditScrnNbr == "AR10300");
                var headerDoc = _db.AR_Doc.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr);
                if (headerBatch != null)
                {
                    if (headerBatch.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        headerBatch.IntRefNbr = curHeader.IntRefNbr;
                        headerBatch.ReasonCD = curHeader.ReasonCD;
                        headerBatch.TotAmt = Convert.ToDouble(curHeader.TotAmt);
                        if (Handle == "R")
                        {
                            headerBatch.Rlsed = 1;
                            headerBatch.Status = "C";
                        }
                        UpdatingFormBotAR_Doc(ref headerDoc, curHeader, Handle);
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
                    if (Handle == "R")
                    {
                        headerBatch.Rlsed = 1;
                        headerBatch.Status = "C";
                    }
                    else if (Handle == "N" || Handle == "")
                    {
                        headerBatch.Rlsed = 0;
                        headerBatch.Status = "H";
                    }
                    headerBatch.IntRefNbr = curHeader.IntRefNbr;
                    headerBatch.ReasonCD = curHeader.ReasonCD;
                    headerBatch.TotAmt = curHeader.TotAmt;
                    headerBatch.TotAmt = curHeader.TotAmt;
                    headerBatch.TotAmt = curHeader.TotAmt;

                    headerBatch.Crtd_DateTime = DateTime.Now;
                    headerBatch.Crtd_Prog = _screenNbr;
                    headerBatch.Crtd_User = Current.UserName;
                    headerBatch.LUpd_DateTime = DateTime.Now;
                    headerBatch.LUpd_Prog = _screenNbr;
                    headerBatch.LUpd_User = Current.UserName;

                    _db.Batches.AddObject(headerBatch);

                    headerDoc = new AR_Doc();
                    headerDoc.ResetET();
                    headerDoc.BranchID = BranchID;
                    headerDoc.BatNbr = headerBatch.BatNbr;
                    headerDoc.RefNbr = headerBatch.RefNbr;
                    headerDoc.DocType = curHeader.DocType;
                    headerDoc.Crtd_DateTime = DateTime.Now;
                    headerDoc.Crtd_Prog = _screenNbr;
                    headerDoc.Crtd_User = Current.UserName;
                    UpdatingFormBotAR_Doc(ref headerDoc, curHeader, Handle);

                    _db.AR_Doc.AddObject(headerDoc);

                    BatNbr = headerBatch.BatNbr;
                    RefNbr = headerBatch.RefNbr;
                }
                #endregion

                #region Save AR_Trans
                foreach (AR10300_pgLoadGridTrans_Result deleted in lstgrd.Deleted)
                {
                    var objDelete = _db.AR_Trans.Where(p => p.BranchID == BranchID
                                                        && p.BatNbr == BatNbr
                                                        && p.RefNbr == RefNbr
                                                        && p.LineRef == deleted.LineRef).FirstOrDefault();
                    if (objDelete != null)
                    {
                        _db.AR_Trans.DeleteObject(objDelete);
                    }
                }

                lstgrd.Created.AddRange(lstgrd.Updated);

                foreach (AR10300_pgLoadGridTrans_Result curLang in lstgrd.Created)
                {
                    if (BranchID.PassNull() == "" || BatNbr.PassNull() == "" || RefNbr.PassNull() == "" || curLang.LineRef.PassNull() == "") continue;

                    var lang = _db.AR_Trans.FirstOrDefault(p => p.BranchID.ToLower() == BranchID.ToLower()
                                                        && p.BatNbr.ToLower() == BatNbr.ToLower()
                                                        && p.RefNbr.ToLower() == RefNbr.ToLower()
                                                        && p.LineRef.ToLower() == curLang.LineRef.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingAR_Trans(lang, curLang, false);
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
                        UpdatingAR_Trans(lang, curLang, true);
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
            t.LUpd_User = Current.UserName;
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

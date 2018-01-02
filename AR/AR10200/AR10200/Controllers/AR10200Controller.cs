using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQFramework.DAL;
using System.Data;
using HQ.eSkySys;
namespace AR10200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR10200Controller : Controller
    {
        private string _screenNbr = "AR10200";
        private string _moduleAR = "AR";
        private string _holdStatus = "H";
        private bool isNewBatch = false;
        AR10200Entities _db = Util.CreateObjectContext<AR10200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;
        //
        // GET: /AR10200/
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

        public ActionResult GetBatNbr(string branchID,
            string module, string batNbr)
        {
            var batch = _db.Batches.FirstOrDefault(x => x.BatNbr == batNbr
                && x.BranchID == branchID
                && x.Module == module);
            return this.Store(batch);
        }

        public ActionResult GetRefNbr(string branchID,
            string refNbr, string batNbr)
        {
            var arRef = _db.AR_Doc.FirstOrDefault(x => x.BatNbr == batNbr
                && x.BranchID == branchID
                && x.RefNbr == refNbr);
            return this.Store(arRef);
        }

        //GetRefList
        public ActionResult GetAdjust(string batNbr, string branchID,
            string custID, string slsperId, string deliveryId,
            string refNbr, DateTime fromDate, DateTime toDate,
            string dateType, string isGridF3)
        {
            var lst = _db.AR10200_pgBindingGrid(batNbr, branchID,
                custID, slsperId, deliveryId,
                refNbr, fromDate, toDate,
                dateType, isGridF3).ToList();

            return this.Store(lst);
        }

        public ActionResult GetRef(string branchID, string batNbr)
        {
            var refs = _db.AR10200_pgBindingGridCancel(batNbr, branchID).ToList();
            return this.Store(refs);
        }

        public ActionResult ReleaseAdjdRef(FormCollection data, string strAdjdRefNbr)
        {
            var branchID = data["txtBranchID"];
            var cboBatNbr = data["cboBatNbr"];
            var batHandler = new StoreDataHandler(data["lstBatNbr"]);
            var inputBatNbr = batHandler.ObjectData<Batch>().FirstOrDefault();
            var batchObj = _db.Batches.FirstOrDefault(
                   x => x.BranchID == inputBatNbr.BranchID
                       && x.BatNbr == inputBatNbr.BatNbr
                       && x.Module == inputBatNbr.Module
                       && x.Status == inputBatNbr.Status);

            if (batchObj != null)
            {
                if (batchObj.tstamp.ToHex() == inputBatNbr.tstamp.ToHex())
                {
                    Data_Release("V", branchID, cboBatNbr, strAdjdRefNbr);
                }
                else
                {
                    throw new MessageException(MessageType.Message, "19");
                }
            }
           
            return Util.CreateMessage(MessageProcess.Save, new { batNbr = cboBatNbr });
        }

        public ActionResult SaveBatch(FormCollection data)
        {
            try
            {
                isNewBatch = false;
                var branchID = data["txtBranchID"];
                var cboBatNbr = data["cboBatNbr"];
                var txtDescr = data["txtDescr"];
                var txtTotAmt = data["txtTotAmt"];
                var cboBankAcct = data["cboBankAcct"];

                var batHandler = new StoreDataHandler(data["lstBatNbr"]);
                var inputBatNbr = batHandler.ObjectData<Batch>().FirstOrDefault();
                var refHandler = new StoreDataHandler(data["lstRefNbr"]);
                var inputRefNbr = refHandler.ObjectData<AR_Doc>().FirstOrDefault();

                if (_db.AR10200_ppCheckCloseDate(Current.CpnyID,Current.UserName,Current.LangID,branchID, inputRefNbr.DocDate.ToDateShort(), "AR10200").FirstOrDefault() == "0")
                    throw new MessageException(MessageType.Message, "301");
                
                inputBatNbr.Descr = txtDescr;
                inputBatNbr.ReasonCD = cboBankAcct;

                // Save Batch
                var batchObj = _db.Batches.FirstOrDefault(
                    x => x.BranchID == branchID
                        && x.BatNbr == cboBatNbr
                        && x.Module == _moduleAR
                        && x.Status == _holdStatus);

                if (batchObj != null)
                {
                    if (batchObj.tstamp.ToHex() == inputBatNbr.tstamp.ToHex())
                    {
                        
                        Updating_Batch(ref batchObj, inputBatNbr, false, branchID);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    Updating_Batch(ref batchObj, inputBatNbr, true, branchID);

                    _db.Batches.AddObject(batchObj);
                    isNewBatch = true;
                }


                inputRefNbr.RefNbr = batchObj.RefNbr;
                inputRefNbr.BatNbr = batchObj.BatNbr;
                inputRefNbr.DocDesc = txtDescr;
                inputRefNbr.OrigDocAmt = batchObj.TotAmt;
                // Save RefNbr/AR_Doc
                var refObj = _db.AR_Doc.FirstOrDefault(x => x.BranchID == branchID
                    && x.BatNbr == batchObj.BatNbr
                    && x.RefNbr == batchObj.RefNbr);

                if (refObj != null)
                {
                    if (refObj.tstamp.ToHex() == inputRefNbr.tstamp.ToHex())
                    {

                        Updating_AR_Doc(ref refObj, inputRefNbr, false, branchID);
                        SaveAR_Adjust(data, refObj, branchID);
                        return Json(new
                        {
                            success = true,
                            msgCode = 201405071,
                            batNbr = batchObj.BatNbr,
                            refNbr = batchObj.RefNbr
                        });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    Updating_AR_Doc(ref refObj, inputRefNbr, true, branchID);
                    _db.AR_Doc.AddObject(refObj);
                    SaveAR_Adjust(data, refObj, branchID);
                    return Json(new
                    {
                        success = true,
                        msgCode = 201405071,
                        batNbr = batchObj.BatNbr,
                        refNbr = batchObj.RefNbr
                    });
                }

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save, new { batNbr = data["cboBatNbr"] });

            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                    return (ex as MessageException).ToMessage();
                return
                    Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void SaveAR_Adjust(FormCollection data, AR_Doc refObj, string branchID)
        {
            var handle = data["cboHandle"].PassNull();
            var adjHandler = new StoreDataHandler(data["lstAdjust"]);
            var lstAdjust = adjHandler.ObjectData<AR10200_pgBindingGrid_Result>()
                .Where(x => !string.IsNullOrWhiteSpace(x.InvcNbr)).ToList();

            var adjusts = _db.AR_Adjust.Where(x => x.BranchID == branchID
                && x.AdjgBatNbr == refObj.BatNbr
                && x.AdjgRefNbr == refObj.RefNbr).ToList();
            foreach (var adjust in adjusts)
            {
                _db.AR_Adjust.DeleteObject(adjust);
            }
            foreach (var adjust in lstAdjust)
            {
                var result = _db.AR_Adjust.FirstOrDefault(x => x.BranchID == branchID
                    && x.BatNbr == adjust.BatNbr
                    && x.AdjgBatNbr == refObj.BatNbr
                    && x.AdjgRefNbr == refObj.RefNbr);
                if (result != null)
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                else
                {
                    var obj = new AR_Adjust();
                    obj.ResetET();
                    obj.Crtd_DateTime = DateTime.Now;
                    obj.Crtd_Prog = _screenNbr;
                    obj.Crtd_User = Current.UserName;
                    Updating_AR_Adjust(ref obj, adjust,refObj, branchID, handle);
                    obj.BatNbr = refObj.BatNbr;
                    obj.AdjgBatNbr = refObj.BatNbr;
                    obj.AdjgRefNbr = refObj.RefNbr;
                    if (obj.AdjAmt > 0)
                    {
                        _db.AR_Adjust.AddObject(obj);
                    };
                }
                //kiểm tra batch đã có tạo nó chưa. Nếu đã có tạo mà chưa release thì ko cho nhập
                if (isNewBatch)
                {
                    var objDocData = _db.AR_Doc.FirstOrDefault(x => x.BranchID == branchID
                   && x.BatNbr == adjust.BatNbr
                   && x.RefNbr == adjust.RefNbr);
                    if (objDocData != null)
                    {
                        if (objDocData.tstamp.ToHex() != adjust.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        else
                        {
                            objDocData.LUpd_DateTime = DateTime.Now;
                            objDocData.LUpd_User =Current.UserName;
                            objDocData.LUpd_Prog = _screenNbr;
                        }
                    }
                    else throw new MessageException(MessageType.Message, "19");
                }
            }
            _db.SaveChanges();
            if (handle == "R")
            {
                Data_Release("R", refObj.BranchID, refObj.BatNbr, "");
            }

        }

        private void Updating_AR_Adjust(ref AR_Adjust objAd, AR10200_pgBindingGrid_Result adjust,AR_Doc objDoc, string branchID, string handle)
        {
            objAd.BranchID = branchID;
            objAd.AdjAmt = adjust.Payment != null ? adjust.Payment.ToDouble() : 0;
            objAd.AdjdDocType = adjust.DocType;
            objAd.AdjdBatNbr = adjust.BatNbr;
            objAd.AdjdRefNbr = adjust.RefNbr;
            objAd.AdjDiscAmt = 0;
            objAd.AdjgDocDate = objDoc.DocDate.Short();
            objAd.AdjgDocType = "PA";
            objAd.Reversal = "";
            objAd.CustID = adjust.CustId;
            objAd.LUpd_DateTime = DateTime.Now;
            objAd.LUpd_Prog = _screenNbr;
            objAd.LUpd_User = Current.UserName;

            if (handle == "R")
            {
                var objAR_Balances = _db.AR_Balances.FirstOrDefault(p => p.CustID == adjust.CustId && p.BranchID == branchID);
                if (objAR_Balances != null)
                {
                    objAR_Balances.CurrBal = objAR_Balances.CurrBal - (double) adjust.Payment;
                    objAR_Balances.LUpd_DateTime = DateTime.Now;
                    objAR_Balances.LUpd_Prog = _screenNbr;
                    objAR_Balances.LUpd_User = _screenNbr;
                }
            }
            else if (handle == "V")
            {
                var objAR_Balances = _db.AR_Balances.FirstOrDefault(p => p.CustID == adjust.CustId && p.BranchID == branchID);
                if (objAR_Balances != null)
                {
                    objAR_Balances.CurrBal = objAR_Balances.CurrBal + (double) adjust.Payment;
                    objAR_Balances.LUpd_DateTime = DateTime.Now;
                    objAR_Balances.LUpd_Prog = _screenNbr;
                    objAR_Balances.LUpd_User = _screenNbr;
                }
            }
        }

        [ValidateInput(false)]
        public ActionResult DeleteBatch(FormCollection data)
        {
            try
            {
                var branchID = data["txtBranchID"];
                var cboBatNbr = data["cboBatNbr"];
                var batHandler = new StoreDataHandler(data["lstBatNbr"]);
                var inputBatNbr = batHandler.ObjectData<Batch>()
                            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.BatNbr));

                var batchObj = _db.Batches.FirstOrDefault(
                    x => x.BranchID == branchID
                        && x.BatNbr == cboBatNbr
                        && x.Module == _moduleAR
                        && x.Status == _holdStatus);
                if (batchObj != null)
                {
                    if (batchObj.tstamp.ToHex() == inputBatNbr.tstamp.ToHex())
                    {
                        var docList = _db.AR_Doc.Where(
                            x => x.BranchID == branchID
                                && x.BatNbr == batchObj.BatNbr).ToList();
                        foreach (var docObj in docList)
                        {
                            _db.AR_Doc.DeleteObject(docObj);
                        }

                        var adjustList = _db.AR_Adjust.Where(
                            x => x.BranchID == branchID
                                && x.BatNbr == batchObj.BatNbr).ToList();
                        foreach (var adjustObj in adjustList)
                        {
                            _db.AR_Adjust.DeleteObject(adjustObj);
                        }

                        _db.Batches.DeleteObject(batchObj);

                        _db.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                return Json(new { success = true });
                //else
                //{
                //    throw new MessageException(MessageType.Message, "19");
                //    //throw new MessageException(MessageType.Message, "22701");
                //}
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                    return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [ValidateInput(false)]
        public ActionResult DeleteAdjust(FormCollection data)
        {
            try
            {
                var cboBatNbr = data["cboBatNbr"];
                var cboRefNbr = data["cboRefNbr"];
                var branchID = data["txtBranchID"];
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAdjust"]);
                ChangeRecords<AR10200_pgBindingGrid_Result> lstAdjust = dataHandler.BatchObjectData<AR10200_pgBindingGrid_Result>();

                foreach (AR10200_pgBindingGrid_Result deleted in lstAdjust.Deleted)
                {
                    var deletedRecord = _db.AR_Adjust.FirstOrDefault(
                        x => x.BranchID == branchID
                            && x.BatNbr == cboBatNbr
                            && x.AdjdRefNbr == deleted.RefNbr
                            && x.AdjgRefNbr == cboRefNbr);
                    if (deletedRecord != null)
                    {
                        _db.AR_Adjust.DeleteObject(deletedRecord);
                    }
                }
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
        }

        private void Updating_Batch(ref Batch objB, Batch input, bool isNew, string branchID)
        {
            if (isNew)
            {
                objB = new Batch();
                objB.ResetET();
                objB.BranchID = branchID;
                objB.BatNbr = _db.ARNumbering(branchID, "BatNbr").FirstOrDefault();
                objB.RefNbr = _db.ARNumbering(branchID, "RefNbr").FirstOrDefault();
                objB.Module = _moduleAR;
                objB.Crtd_DateTime = DateTime.Now;
                objB.Crtd_Prog = _screenNbr;
                objB.Crtd_User = Current.UserName;
            }
            objB.TotAmt = input.TotAmt;
            objB.NoteID = 0;
            objB.EditScrnNbr = _screenNbr;
            objB.JrnlType = _moduleAR;

            objB.Rlsed = 0;
            objB.Status = input.Status;
            objB.Descr = input.Descr;//data["txtDescr"]
            objB.ReasonCD = input.ReasonCD;

            objB.DateEnt = DateTime.Now.Short();
            objB.OrigBranchID = string.Empty;

            objB.LUpd_DateTime = DateTime.Now;
            objB.LUpd_Prog = _screenNbr;
            objB.LUpd_User = Current.UserName;

        }
        private void Updating_AR_Doc(ref AR_Doc objD, AR_Doc inputRef, bool isNew, string branchID)
        {
            if (isNew)
            {
                objD = new AR_Doc();
                objD.ResetET();
                objD.BatNbr = inputRef.BatNbr;
                objD.BranchID = branchID;
               // objD.RefNbr = _db.ARNumbering(Current.CpnyID, "RefNbr").FirstOrDefault();
                objD.RefNbr = inputRef.RefNbr;
                objD.Crtd_DateTime = DateTime.Now;
                objD.Crtd_Prog = _screenNbr;
                objD.Crtd_User = Current.UserName;
            }
            
            if (string.IsNullOrWhiteSpace(inputRef.InvcNbr))
            {
                objD.InvcNbr = _db.ARNumbering(branchID, "ReceiptNbr").FirstOrDefault();
            }
            else
            {
                objD.InvcNbr = inputRef.InvcNbr;
            }
            objD.NoteId = 0;
            objD.DocType = "PA";
            objD.DocDate = inputRef.DocDate.Short();
            objD.DiscDate = inputRef.DocDate.Short();
            objD.DueDate = inputRef.DocDate.Short();
            objD.SlsperId = inputRef.SlsperId;
            objD.OrigDocAmt = inputRef.OrigDocAmt;//data["txtTotAmt"]
            objD.DocDesc = inputRef.DocDesc;

            objD.LUpd_DateTime = DateTime.Now;
            objD.LUpd_Prog = _screenNbr;
            objD.LUpd_User = Current.UserName;
        }

        private void Data_Release(string handle, string BranchID, string BatNbr, string refNbr)
        {
            if (handle != "N")
            {
                DataAccess dal = Util.Dal();
                try
                {
                    ARProcess.AR ar = new ARProcess.AR(Current.UserName, _screenNbr, dal);
                    if (handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!ar.AR10200_Release(BatNbr, BranchID))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }
                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = BatNbr });
                    }
                    else if (handle == "V")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (refNbr == "%")
                            ar.AR10200_Cancel(BatNbr, BranchID);
                        else
                            ar.AR10200_Cancel(BatNbr, BranchID, refNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", data: new { success = true, batNbr = BatNbr });
                    }                   
                    ar = null;
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
            }
        }
    }
}

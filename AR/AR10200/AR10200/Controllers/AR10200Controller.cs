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
using HQSendMailApprove;
using HQFramework.DAL;
using System.Data;
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
        AR10200Entities _db = Util.CreateObjectContext<AR10200Entities>(false);
  
        //
        // GET: /AR10200/
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

        public ActionResult ReleaseAdjdRef(string strAdjdRefNbr)
        {
            //Data_Release("V");
            return Json(new { success = true }); 
        }

        public ActionResult SaveBatch(FormCollection data)
        {
            try
            {
                var cboBatNbr = data["cboBatNbr"];
                var txtDescr = data["txtDescr"];
                var txtTotAmt = data["txtTotAmt"];
                var batHandler = new StoreDataHandler(data["lstBatNbr"]);
                var inputBatNbr = batHandler.ObjectData<Batch>().FirstOrDefault();

                // Check input BatNbr
                if (inputBatNbr != null)
                {
                    inputBatNbr.Descr = txtDescr;

                    // Save Batch
                    var batchObj = _db.Batches.FirstOrDefault(
                        x => x.BranchID == Current.CpnyID
                            && x.BatNbr == cboBatNbr
                            && x.Module == _moduleAR
                            && x.Status == _holdStatus);

                    if (batchObj != null)
                    {
                        if (batchObj.tstamp.ToHex() == inputBatNbr.tstamp.ToHex())
                        {
                            Updating_Batch(ref batchObj, inputBatNbr, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Updating_Batch(ref batchObj, inputBatNbr, true);

                        _db.Batches.AddObject(batchObj);
                    }

                    // Input RefNbr
                    var refHandler = new StoreDataHandler(data["lstRefNbr"]);
                    var inputRefNbr = refHandler.ObjectData<AR_Doc>().FirstOrDefault();

                    if (inputRefNbr != null)
                    {
                        inputRefNbr.BatNbr = batchObj.BatNbr;
                        inputRefNbr.OrigDocAmt = data["txtTotAmt"] != null ? double.Parse(data["txtTotAmt"]) : 0;
                        
                        // Save RefNbr/AR_Doc
                        var refObj = _db.AR_Doc.FirstOrDefault(x => x.BranchID == Current.CpnyID
                            && x.BatNbr == batchObj.BatNbr
                            && x.RefNbr == inputRefNbr.RefNbr);

                        if (refObj != null)
                        {
                            if (refObj.tstamp.ToHex() == inputRefNbr.tstamp.ToHex())
                            {
                                
                                Updating_AR_Doc(ref refObj, inputRefNbr, false);
                                SaveAR_Adjust(data, refObj);
                                return Json(new { 
                                    success = true, msgCode = 201405071, 
                                    batNbr = batchObj.BatNbr, 
                                    refNbr = refObj.RefNbr
                                });
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            Updating_AR_Doc(ref refObj, inputRefNbr, true);
                            _db.AR_Doc.AddObject(refObj);
                            SaveAR_Adjust(data, refObj);
                            return Json(new {
                                success = true,
                                msgCode = 201405071,
                                batNbr = batchObj.BatNbr,
                                refNbr = refObj.RefNbr
                            });
                        }
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "22701");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "22701");
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                    return (ex as MessageException).ToMessage();
                return 
                    Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void SaveAR_Adjust(FormCollection data, AR_Doc refObj)
        {
            var adjHandler = new StoreDataHandler(data["lstAdjust"]);
            var lstAdjust = adjHandler.ObjectData<AR10200_pgBindingGrid_Result>()
                .Where(x=>!string.IsNullOrWhiteSpace(x.InvcNbr)).ToList();

            var adjusts = _db.AR_Adjust.Where(x => x.BranchID == Current.CpnyID 
                && x.AdjgBatNbr == refObj.BatNbr 
                && x.AdjgRefNbr == refObj.RefNbr).ToList();
            foreach (var adjust in adjusts)
            {
                _db.AR_Adjust.DeleteObject(adjust);
            }

            foreach (var adjust in lstAdjust)
            {
                var result = _db.AR_Adjust.FirstOrDefault(x => x.BranchID == Current.CpnyID 
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
                    obj.Crtd_DateTime = DateTime.Now;
                    obj.Crtd_Prog = _screenNbr;
                    obj.Crtd_User = Current.UserName;
                    Updating_AR_Adjust(ref obj, adjust);
                    obj.BatNbr = refObj.BatNbr;
                    obj.AdjgBatNbr = refObj.BatNbr;
                    obj.AdjgRefNbr = refObj.RefNbr;
                    if (obj.AdjAmt > 0) {
                        _db.AR_Adjust.AddObject(obj);
                    };
                }
            }
            _db.SaveChanges();
            //Data_Release("R");
        }

        private void Updating_AR_Adjust(ref AR_Adjust objAd, AR10200_pgBindingGrid_Result adjust)
        {
            objAd.BranchID = Current.CpnyID;
            objAd.AdjAmt = adjust.Payment != null ? adjust.Payment.ToDouble() : 0;
            objAd.AdjdDocType = adjust.DocType;
            objAd.AdjdBatNbr = adjust.BatNbr;
            objAd.AdjdRefNbr = adjust.RefNbr;
            objAd.AdjDiscAmt = 0;
            objAd.AdjgDocDate = adjust.DocDate.Short();
            objAd.AdjgDocType = "PA";
            objAd.Reversal = "";
            objAd.CustID = adjust.CustId;
            objAd.LUpd_DateTime = DateTime.Now;
            objAd.LUpd_Prog = _screenNbr;
            objAd.LUpd_User = Current.UserName;
        }

        [ValidateInput(false)]
        public ActionResult DeleteBatch(FormCollection data)
        {
            try
            {
                var cboBatNbr = data["cboBatNbr"];
                var batHandler = new StoreDataHandler(data["lstBatNbr"]);
                var inputBatNbr = batHandler.ObjectData<Batch>()
                            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.BatNbr));

                var batchObj = _db.Batches.FirstOrDefault(
                    x => x.BranchID == Current.CpnyID
                        && x.BatNbr == cboBatNbr
                        && x.Module == _moduleAR
                        && x.Status == _holdStatus);
                if (null != batchObj)
                {
                    if (batchObj.tstamp.ToHex() == inputBatNbr.tstamp.ToHex())
                    {
                        var docList = _db.AR_Doc.Where(
                            x => x.BranchID == Current.CpnyID
                                && x.BatNbr == batchObj.BatNbr).ToList();
                        foreach (var docObj in docList)
                        {
                            _db.AR_Doc.DeleteObject(docObj);
                        }

                        var adjustList = _db.AR_Adjust.Where(
                            x => x.BranchID == Current.CpnyID
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
                else
                {
                    throw new MessageException(MessageType.Message, "22701");
                }
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

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAdjust"]);
                ChangeRecords<AR10200_pgBindingGrid_Result> lstAdjust = dataHandler.BatchObjectData<AR10200_pgBindingGrid_Result>();

                foreach (AR10200_pgBindingGrid_Result deleted in lstAdjust.Deleted)
                {
                    var deletedRecord = _db.AR_Adjust.FirstOrDefault(
                        x => x.BranchID == Current.CpnyID
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

        private void Updating_Batch(ref Batch objB, Batch input, bool isNew)
        {
            if (isNew) {
                objB = new Batch();


                objB.BranchID = Current.CpnyID;
                objB.BatNbr = _db.ARNumbering(Current.CpnyID, "BatNbr").FirstOrDefault();
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
        private void Updating_AR_Doc(ref AR_Doc objD, AR_Doc inputRef, bool isNew)
        {
            if (isNew) {
                objD = new AR_Doc();

                objD.BatNbr = inputRef.BatNbr;
                objD.BranchID = Current.CpnyID;
                objD.RefNbr = _db.ARNumbering(Current.CpnyID, "RefNbr").FirstOrDefault();

                objD.Crtd_DateTime = DateTime.Now;
                objD.Crtd_Prog = _screenNbr;
                objD.Crtd_User = Current.UserName;
            }
            if (string.IsNullOrWhiteSpace(inputRef.InvcNbr))
            {
                objD.InvcNbr = _db.ARNumbering(Current.CpnyID, "ReceiptNbr").FirstOrDefault();
            }
            else {
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
    }
}

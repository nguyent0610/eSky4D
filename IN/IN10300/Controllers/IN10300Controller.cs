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
namespace IN10300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10300Controller : Controller
    {
        private string _screenNbr = "IN10300";
        private string _userName = Current.UserName;
        private string _handle = "";
        private IN10300Entities _app = Util.CreateObjectContext<IN10300Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);     
        private FormCollection _form;
        private IN10300_pcBatch_Result _objBatch;
        private JsonResult _logMessage;
        private List<IN10300_pgTransferLoad_Result> _lstTrans;
        private List<IN_LotTrans> _lstLot;
        private IN_Setup _objIN;
      
        public ActionResult Index(string branchID)
        {
            Util.InitRight(_screenNbr);

            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);

            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }

            if (branchID == null) branchID = Current.CpnyID;

            bool isSetDefaultShipViaID = false;
            var objConfig = _app.IN10300_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                isSetDefaultShipViaID = objConfig.IsSetDefaultShipViaID != null ? objConfig.IsSetDefaultShipViaID.Value : false;
            }
            ViewBag.isSetDefaultShipViaID = isSetDefaultShipViaID;
            ViewBag.BranchID = branchID;

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
            var lstBatch = _app.IN10300_pcBatch(branchID, query, start, start + 20).ToList();
            var paging = new Paging<IN10300_pcBatch_Result>(lstBatch, lstBatch.Count > 0 ? lstBatch[0].TotalRecords.Value : 0);
            return this.Store(paging.Data, paging.TotalRecords);
        }
        public ActionResult GetUserDefault()
        {
            string userName = Current.UserName;
            string cpnyID = Current.CpnyID;
            var objUser = _app.OM_UserDefault.FirstOrDefault(p => p.UserID == userName && p.DfltBranchID == cpnyID);
            return this.Store(objUser);
        }
        public ActionResult GetSetup()
        {
            string cpnyID = Current.CpnyID;
            var objSetup = _app.IN_Setup.FirstOrDefault(p => p.SetupID == "IN" && p.BranchID == cpnyID);
            return this.Store(objSetup);
        }
        public ActionResult GetTrans(string batNbr, string branchID, string refNbr)
        {
            var lstTrans = _app.IN10300_pgTransferLoad(batNbr, branchID, "%", refNbr).ToList();
            return this.Store(lstTrans);
        }
        public ActionResult GetLot(string invtID, string siteID, string batNbr, string branchID)
        {
            List<IN_ItemLot> lstLot = new List<IN_ItemLot>();

            List<IN_ItemLot> lstLotDB = _app.IN_ItemLot.Where(p => p.SiteID == siteID && p.InvtID == invtID && p.QtyAvail > 0).ToList();

            foreach (var item in lstLotDB)
            {
                lstLot.Add(item);
            }

            List<IN_LotTrans> lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID).ToList();
            foreach (var item in lstLotTrans)
            {
                var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                if (lot == null)
                {
                    var lotDB = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == item.LotSerNbr);
                    lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                    lstLot.Add(lotDB);
                }
                else
                {
                    lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                }


            }
            lstLot = lstLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();
            return this.Store(lstLot, lstLot.Count);
            //return this.Store(lstLot.OrderBy(p => p.LotSerNbr).ToList(), lstLot.Count);
        }
        public ActionResult GetLotTrans(string branchID, string batNbr)
        {
            List<IN_LotTrans> lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.ToSiteID!="").ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetItemLot(string invtID, string siteID, string lotSerNbr, string branchID, string batNbr)
        {
            var lot = _app.IN_ItemLot.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr);

            if (lot == null) lot = new IN_ItemLot()
            {
                InvtID = invtID,
                SiteID = siteID,
                LotSerNbr = lotSerNbr
            };

            var lotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr).ToList();

            foreach (var item in lotTrans)
            {
                lot.QtyAvail += (item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact);
            }

            List<IN_ItemLot> lstLot = new List<IN_ItemLot>() { lot };
            return this.Store(lstLot, lstLot.Count);
        }
        
        public ActionResult GetItemSite(string invtID, string siteID)
        {
            var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
            return this.Store(objSite);
        }
        public ActionResult GetUnitConversion()
        {
            var lstUnit = _app.IN10300_pcUnitConversion(Current.CpnyID).ToList();
            return this.Store(lstUnit);
        }
        public ActionResult GetPrice(string invtID, string uom, DateTime effDate, string branchID, string valMthd, string siteID)
        {
            var lstPrice = _app.IN10300_pdPrice("", invtID, uom, DateTime.Now, branchID, valMthd, siteID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstPrice);
        }
        public ActionResult GetUnit(string invtID)
        {
            IN_Inventory invt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (invt == null) invt = new IN_Inventory();
            List<IN10300_pcUnit_Result> lstUnit = _app.IN10300_pcUnit(invt.ClassID, invt.InvtID).ToList();
            return this.Store(lstUnit, lstUnit.Count);
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
                return Util.CreateMessage(MessageProcess.Save,  new { batNbr = _objBatch.BatNbr });
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
        [HttpPost]
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                var access = Session["IN10300"] as AccessRight;
                if (!access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objBatch = data.ConvertToObject<IN10300_pcBatch_Result>();

                if (_objBatch.Status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }

                _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
                if (_objIN == null) _objIN = new IN_Setup();

                var batch = _app.Batches.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr);
                if (batch != null) _app.Batches.DeleteObject(batch);

                var transfer = _app.IN_Transfer.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.RefNbr == _objBatch.RefNbr);
                if (transfer != null) _app.IN_Transfer.DeleteObject(transfer);

                var lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.RefNbr == _objBatch.RefNbr).ToList();
                foreach (var trans in lstTrans)
                {
                    double oldQty = 0;

                    oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                    UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);

                    _app.IN_Trans.DeleteObject(trans);
                }

                var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;

                    oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;

                    UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);

                    _app.IN_LotTrans.DeleteObject(lot);
                }

                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Delete);
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
        [HttpPost]
        public ActionResult DeleteTrans(FormCollection data)
        {
            try
            {
                var access = Session["IN10300"] as AccessRight;

                _objBatch = data.ConvertToObject<IN10300_pcBatch_Result>();

                if (_objBatch.Status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }
                if ((_objBatch.BatNbr.PassNull() == string.Empty && !access.Insert) || (_objBatch.BatNbr.PassNull() != string.Empty && !access.Update))
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
                if (_objIN == null) _objIN = new IN_Setup();
                string lineRef = Util.PassNull(data["LineRef"]);

                var lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.RefNbr == _objBatch.RefNbr).ToList();

                var trans = lstTrans.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.LineRef == lineRef && p.RefNbr == _objBatch.RefNbr);

                if (trans != null)
                {
                    double oldQty = 0;
                   
                    oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                    UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                    
                    lstTrans.Remove(trans);
                    _app.IN_Trans.DeleteObject(trans);
                }

                var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef).ToList();
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;

                    oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                    UpdateAllocLot(lot.InvtID, lot.SiteID,lot.LotSerNbr, oldQty, 0,0);

                    _app.IN_LotTrans.DeleteObject(lot);
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

        private void SaveData(FormCollection data)
        {

           
            if (_lstTrans == null)
            {
                var transHandler = new StoreDataHandler(data["lstTrans"]);
                _lstTrans = transHandler.ObjectData<IN10300_pgTransferLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }
            if (_lstLot == null)
            {
                var lotHandler = new StoreDataHandler(data["lstLot"]);
                _lstLot = lotHandler.ObjectData<IN_LotTrans>().Where(p => Util.PassNull(p.INTranLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            }

            _objBatch = data.ConvertToObject<IN10300_pcBatch_Result>();

            _handle = data["Handle"].PassNull();
            _objBatch.Status = _objBatch.Status.PassNull() == string.Empty ? "H" : _objBatch.Status;

            if (_app.IN10300_ppCheckCloseDate(_objBatch.BranchID, _objBatch.TranDate.ToDateShort(), "IN10300").FirstOrDefault() == "0")
                throw new MessageException(MessageType.Message, "301");

            //var cfgWrkDateChk = _sys.SYS_CloseDateSetUp.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            //if (cfgWrkDateChk != null && cfgWrkDateChk.WrkDateChk)
            //{
            //    DateTime tranDate = _objBatch.DateEnt;
            //    if (!((DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(-1 * cfgWrkDateChk.WrkLowerDays)) >=
            //           0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) <= 0)
            //          ||
            //          (DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(cfgWrkDateChk.WrkUpperDays)) <=
            //           0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) >= 0)
            //          || DateTime.Compare(tranDate, cfgWrkDateChk.WrkAdjDate) == 0))
            //    {
            //        throw new MessageException(MessageType.Message, "301");
            //    }
            //}
         
            Batch batch = _app.Batches.FirstOrDefault(p => p.BatNbr == _objBatch.BatNbr && p.BranchID == _objBatch.BranchID);
            if ((_objBatch.Status == "U" || _objBatch.Status == "C") && (_handle == "C" || _handle == "V"))
            {
                if (batch != null)
                {
                    if (batch.tstamp.ToHex() != data["tstamp"].ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2015020802", "", new[] { batch.BatNbr });
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
                INProcess.IN inventory = new INProcess.IN(_userName, _screenNbr, dal);
                try
                {
                   
                    if (_handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);

                        inventory.IN10300_Release(_objBatch.BranchID, _objBatch.BatNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                    inventory = null;
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
                finally
                {
                    dal = null;
                    inventory = null;
                }
            }
        }
        private void CheckData()
        {
            var access = Session[_screenNbr] as AccessRight;

            if ((_objBatch.BatNbr.PassNull() != string.Empty && !access.Update) || (_objBatch.BatNbr.PassNull() == string.Empty && !access.Insert))
            {
                throw new MessageException(MessageType.Message, "728");
            }

            if (_objBatch.Status.PassNull() != "H" && (_handle == string.Empty || _handle == "N"))
            {
                throw new MessageException(MessageType.Message, "2015020803");
            }
            if (_objBatch.BranchID != _objBatch.ToCpnyID && _objBatch.TransferType == "1")
            {
                throw new MessageException(MessageType.Message, "1092");
            }
            if (_lstTrans.Count == 0)
            {
                throw new MessageException(MessageType.Message, "2015020804", "");
            }

            for (int i = 0; i < _lstTrans.Count; i++)
            {
                string invtID = _lstTrans[i].InvtID;
                string siteID = _lstTrans[i].SiteID;
                double editQty = 0;
                double qtyTot = 0;
                if (_lstTrans[i].Qty == 0)
                {
                    throw new MessageException("1000", new[] { Util.GetLang("Qty") });
                }

                if (_lstTrans[i].SiteID.PassNull() == string.Empty)
                {
                    throw new MessageException("1000", new[] { Util.GetLang("SiteID") });
                }

                if (_lstTrans[i].UnitMultDiv.PassNull() == string.Empty || _lstTrans[i].UnitDesc.PassNull() == string.Empty)
                {
                    throw new MessageException("2525", new[] { _lstTrans[i].InvtID });
                }

                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack != "N")
                {
                    var lstLot = _lstLot.Where(p => p.INTranLineRef == _lstTrans[i].LineRef).ToList();
                    double lotQty = 0;
                    foreach (var item in lstLot)
                    {
                        if (item.InvtID != _lstTrans[i].InvtID || item.SiteID != _lstTrans[i].SiteID)
                        {
                            throw new MessageException("2015040501", new[] { _lstTrans[i].InvtID });
                        }

                        if (item.UnitMultDiv.PassNull() == string.Empty || item.UnitDesc.PassNull() == string.Empty)
                        {
                            throw new MessageException("2015040503", new[] { _lstTrans[i].InvtID });
                        }

                        lotQty += Math.Round(item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact, 0);
                    }
                    double detQty = Math.Round(_lstTrans[i].UnitMultDiv == "M" ? _lstTrans[i].Qty * _lstTrans[i].CnvFact : _lstTrans[i].Qty / _lstTrans[i].CnvFact, 0);
                    if (detQty != lotQty)
                    {
                        throw new MessageException("2015040502", new[] { _lstTrans[i].InvtID });
                    }
                }
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
                batch = new Batch();
                Update_Batch(batch, true);
                _app.Batches.AddObject(batch);
            }

            Save_Transfer(batch);
        }

        private void Save_Transfer(Batch batch)
        {
            var transfer = _app.IN_Transfer.FirstOrDefault(p => p.BatNbr == batch.BatNbr && p.TrnsfrDocNbr == _objBatch.TrnsfrDocNbr && p.BranchID == batch.BranchID);
            if (transfer != null)
            {
                Update_Transfer(transfer, false);
            }
            else
            {
                transfer = new IN_Transfer();
                transfer.ResetET();
                transfer.TrnsfrDocNbr = _app.INNumbering(_objBatch.BranchID, "TrnsNbr").FirstOrDefault();
                transfer.RefNbr = _app.INNumbering(_objBatch.BranchID, "RefNbr").FirstOrDefault();
              
                Update_Transfer(transfer, true);
                _app.IN_Transfer.AddObject(transfer);
            }

            Save_Trans(transfer);
        }

        private void Save_Trans(IN_Transfer transfer)
        {
            _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            if (_objIN == null) _objIN = new IN_Setup();
            foreach (var trans in _lstTrans)
            {

                var transDB = _app.IN_Trans.FirstOrDefault(p =>
                                p.BatNbr == transfer.BatNbr && p.RefNbr == transfer.RefNbr &&
                                p.BranchID == transfer.BranchID && p.LineRef == trans.LineRef);

                if (transDB != null)
                {
                    if (transDB.tstamp.ToHex() != trans.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    Update_Trans(transfer, transDB, trans, false);
                }
                else
                {
                    transDB = new IN_Trans();
                    Update_Trans(transfer, transDB, trans, true);
                    _app.IN_Trans.AddObject(transDB);
                }
                Save_Lot(transfer, transDB);
            }
        }
        private bool Save_Lot(IN_Transfer transfer, IN_Trans tran)
        {
            var lots = _app.IN_LotTrans.Where(p => p.BranchID == transfer.BranchID && p.BatNbr == transfer.BatNbr).ToList();
            foreach (var item in lots)
            {
                if (item.EntityState == EntityState.Deleted || item.EntityState == EntityState.Detached) continue;
                if (!_lstLot.Any(p => p.INTranLineRef == item.INTranLineRef && p.LotSerNbr == item.LotSerNbr))
                {
                    var oldQty = item.UnitMultDiv == "D" ? item.Qty / item.CnvFact : item.Qty * item.CnvFact;

                    UpdateAllocLot(item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);

                    _app.IN_LotTrans.DeleteObject(item);
                }
            }

            var lstLotTmp = _lstLot.Where(p => p.INTranLineRef == tran.LineRef).ToList();
            foreach (var lotCur in lstLotTmp)
            {
                var lot = _app.IN_LotTrans.FirstOrDefault(p => p.BranchID == transfer.BranchID && p.BatNbr == transfer.BatNbr && p.INTranLineRef == lotCur.INTranLineRef && p.LotSerNbr == lotCur.LotSerNbr);
                if (lot == null || lot.EntityState == EntityState.Deleted || lot.EntityState == EntityState.Detached)
                {
                    lot = new IN_LotTrans();
                    Update_Lot(lot, lotCur, transfer, tran, true);
                    _app.IN_LotTrans.AddObject(lot);
                }
                else
                {
                    Update_Lot(lot, lotCur, transfer, tran, false);
                }
            }
            return true;
        }

        private void Update_Batch(Batch t, bool isNew)
        {
            if (isNew)
            {
                t.ResetET();
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                t.Module = "IN";

                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
                t.Crtd_DateTime = DateTime.Now;
            }

            t.JrnlType = _objBatch.JrnlType.PassNull() == string.Empty ? "IN" : _objBatch.JrnlType;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.DateEnt = _objBatch.TranDate.ToDateShort();
            t.Descr = _objBatch.Descr;
            t.EditScrnNbr = t.EditScrnNbr.PassNull() == string.Empty ? _screenNbr : t.EditScrnNbr;
            t.NoteID = 0;
            t.ReasonCD = _objBatch.ReasonCD;
            t.TotAmt = _objBatch.TotAmt;
            t.Rlsed = 0;
            t.Status = _objBatch.Status;
        }

        private void Update_Transfer(IN_Transfer t, bool isNew)
        {
            if (isNew)
            {
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                t.TrnsfrDocNbr = t.TrnsfrDocNbr;
                t.RefNbr = t.RefNbr;

                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
                t.Crtd_DateTime = DateTime.Now;
            }

            t.NoteID = 0;
            t.Comment = _objBatch.Comment.PassNull();
            t.ReasonCD = _objBatch.ReasonCD.PassNull();
            t.RcptDate = _objBatch.RcptDate.ToDateShort();
            t.SiteID = _objBatch.SiteID.PassNull();
            t.Status = _objBatch.TransferStatus.PassNull();
            t.ToSiteID = _objBatch.ToSiteID.PassNull();
            t.TranDate = _objBatch.TranDate.ToDateShort();
            t.TransferType = _objBatch.TransferType.PassNull();
            t.ShipViaID = _objBatch.ShipViaID.PassNull();
            t.Source = "IN";
            t.ToCpnyID = _objBatch.ToCpnyID.PassNull();
            t.ExpectedDate = _objBatch.ExpectedDate.ToDateShort();

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_Trans(IN_Transfer transfer, IN_Trans t, IN10300_pgTransferLoad_Result s, bool isNew)
        {
            double oldQty, newQty;

           
            if (!isNew)
                oldQty = t.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
            else
                oldQty = 0;

            newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

            UpdateINAlloc(t.InvtID, t.SiteID, oldQty, 0);

            UpdateINAlloc(s.InvtID, s.SiteID, 0, newQty);

            

            if (isNew)
            {
                t.ResetET();

                t.LineRef = s.LineRef;
                t.BranchID = transfer.BranchID;
                t.BatNbr = transfer.BatNbr;
                t.RefNbr = transfer.RefNbr;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

            t.CnvFact = s.CnvFact;
            t.ExtCost = s.ExtCost;
            t.InvtID = s.InvtID;
            t.InvtMult = s.InvtMult;
            t.JrnlType = s.JrnlType;
            t.ObjID = s.ObjID;
            t.ReasonCD = transfer.ReasonCD;
            t.Qty = Math.Round(s.Qty, 0);
            t.Rlsed = s.Rlsed;
            t.ShipperID = s.ShipperID;
            t.ShipperLineRef = s.ShipperLineRef;
            t.SiteID = transfer.SiteID;
            t.ToSiteID = transfer.ToSiteID;
            t.TranAmt = Math.Round(s.TranAmt, 0);
            t.TranFee = s.TranFee;
            t.TranDate = transfer.TranDate;
            t.TranDesc = s.TranDesc;
            t.TranType = s.TranType;
            t.UnitCost = s.UnitCost;
            t.UnitDesc = s.UnitDesc;
            t.UnitMultDiv = s.UnitMultDiv;
            t.UnitPrice = Math.Round(s.UnitPrice, 0);
        }
        private bool Update_Lot(IN_LotTrans t, IN_LotTrans s, IN_Transfer transfer, IN_Trans tran, bool isNew)
        {

            if (isNew)
            {
                t.ResetET();
                t.BatNbr = transfer.BatNbr;
                t.BranchID = transfer.BranchID;
                t.INTranLineRef = s.INTranLineRef;
                t.LotSerNbr = s.LotSerNbr;
                t.RefNbr = tran.RefNbr;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;

                t.WarrantyDate = DateTime.Now.ToDateShort();
            }

            double oldQty = 0;
            double newQty = 0;

           
            if (!isNew)
                oldQty = s.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
            else
                oldQty = 0;

            newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

            UpdateAllocLot(t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

            if (!UpdateAllocLot(s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
            {
                throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
            }

            t.UnitDesc = s.UnitDesc;
            t.ExpDate = s.ExpDate;
            t.InvtID = s.InvtID;
            t.InvtMult = tran.InvtMult;
            t.Qty = s.Qty;
            t.ToSiteID = tran.ToSiteID;
            t.SiteID = s.SiteID;

            t.MfgrLotSerNbr = s.MfgrLotSerNbr.PassNull();
            t.TranType = tran.TranType;

            t.TranDate = tran.TranDate;
            t.CnvFact = s.CnvFact;
            t.UnitCost = s.UnitCost;
            t.UnitPrice = s.UnitPrice;

            t.UnitMultDiv = s.UnitMultDiv;

            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.LUpd_DateTime = DateTime.Now;

            return true;
        }

        private bool UpdateINAlloc(string invtID, string siteID, double oldQty, double newQty)
        {
            try
            {
                var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
                    if (objSite == null) objSite = new IN_ItemSite() { InvtID = invtID, SiteID = siteID };

                    if (!_objIN.NegQty && newQty > 0 && objSite.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new string[] { invtID, siteID });
                    }
                    objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + newQty - oldQty, 0);
                    objSite.QtyAvail = Math.Round(objSite.QtyAvail - newQty + oldQty, 0);
                    
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool UpdateAllocLot(string invtID, string siteID, string lotSerNbr, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                var objItemLot = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr);
                if (objItemLot != null)
                {
                    if (!_objIN.NegQty && newQty > 0 && objItemLot.QtyAvail + oldQty - newQty < 0)
                    {

                        //Util.AppendLog(ref _logMessage, "608", parm: new[] { objItemLot.InvtID + " " objItemLot.LotSerNbr , objItemSite.SiteID });
                        return false;
                    }
                    objItemLot.QtyAllocIN = Math.Round(objItemLot.QtyAllocIN + newQty - oldQty, decQty);
                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail - newQty + oldQty, decQty);
                }
                return true;
            }
            return true;
        }

        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                _form = data;
                _objBatch = data.ConvertToObject<IN10300_pcBatch_Result>();
                User user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
                var rpt = new RPTRunning();
                rpt.ResetET();

                rpt.ReportNbr = "IN602";
                rpt.MachineName = "Web";
                rpt.ReportCap = "IN_Transfer";
                rpt.ReportName = "IN_Transfer";
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

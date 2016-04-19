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
namespace IN10400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10400Controller : Controller
    {
        private string _screenNbr = "IN10400";
        private string _userName = Current.UserName;
        private string _branchID = "";
        private string _handle = "";
        IN10400Entities _app = Util.CreateObjectContext<IN10400Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private IN10400_pcINAdjustmentBatch_Result _objBatch;
        private FormCollection _form;
        private IN_Setup _objIN;
        private List<IN10400_pgAdjustmentLoad_Result> _lstTrans;
        private int _decQty;
        private int _decAmt;
        private int _decPrice;

        private JsonResult _logMessage;

        public ActionResult Index(string branchID)
        {
            Util.InitRight(_screenNbr);

            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);

            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }

            var userDft = _app.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID == branchID);
            if (branchID == null) branchID = Current.CpnyID;
            ViewBag.INSite = userDft == null ? "" : userDft.INSite;
            ViewBag.BranchID = branchID;
            return View();

        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        #region LoadData
        //public ActionResult GetBatch(string branchID, string query, int start, int limit, int page)
        //{
        //    query = query ?? string.Empty;
        //    if (page != 1) query = string.Empty;
        //    var lstBatch = _app.IN10200_pcBatch(branchID, query, start, start + 20).ToList();
        //    var paging = new Paging<IN10200_pcBatch_Result>(lstBatch, lstBatch.Count > 0 ? lstBatch[0].TotalRecords.Value : 0);
        //    return this.Store(paging.Data, paging.TotalRecords);
        //}

        public ActionResult GetBatch(string branchID)
        {
            var lstBatch = _app.IN10400_pcINAdjustmentBatch(branchID ).ToList();
            return this.Store(lstBatch);
        }

        public ActionResult GetUserDefault()
        {
            string userName = Current.UserName;
            string cpnyID = Current.CpnyID;
            var objUser = _app.OM_UserDefault.FirstOrDefault(p => p.UserID == userName && p.DfltBranchID == cpnyID);
            return this.Store(objUser);
        }

        public ActionResult GetTrans(string batNbr, string branchID)
        {
            var lstTrans = _app.IN10400_pgAdjustmentLoad(batNbr, branchID, "%", "%").ToList();
            return this.Store(lstTrans);
        }

        public ActionResult GetSetup()
        {
            string cpnyID = Current.CpnyID;
            var objSetup = _app.IN_Setup.FirstOrDefault(p => p.SetupID == "IN" && p.BranchID == cpnyID);
            return this.Store(objSetup);
        }

        public ActionResult GetUnit(string invtID)
        {
            IN_Inventory invt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (invt == null) invt = new IN_Inventory();
            List<IN10400_pcUnit_Result> lstUnit = _app.IN10400_pcUnit(invt.ClassID, invt.InvtID).ToList();
            return this.Store(lstUnit, lstUnit.Count);
        }
        public ActionResult GetUnitConversion()
        {
            var lstUnit = _app.IN10400_pcUnitConversion(Current.CpnyID).ToList();
            return this.Store(lstUnit);
        }

        public ActionResult GetItemSite(string invtID, string siteID)
        {
            var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
            return this.Store(objSite);
        }
        #endregion

        #region Action
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
        [HttpPost]
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                var access = Session["IN10400"] as AccessRight;
                if (!access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                string branchID = data["txtBranchID"];
                string batNbr = data["cboBatNbr"];
                _objBatch = _app.IN10400_pcINAdjustmentBatch(branchID).FirstOrDefault(p => p.BatNbr == batNbr);

                if (_objBatch.Status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }

                _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
                if (_objIN == null) _objIN = new IN_Setup();

                var batch = _app.Batches.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr);
                if (batch != null) _app.Batches.DeleteObject(batch);

                var lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                foreach (var trans in lstTrans)
                {
                    double oldQty = 0;
                    //if (trans.TranType == "II")
                    //{
                    //    oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                    //    UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                    //}
                    _app.IN_Trans.DeleteObject(trans);
                }
                
                //var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                //foreach (var lot in lstLot)
                //{
                //    double oldQty = 0;
                //    if (lot.TranType == "II")
                //    {
                //        oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                //        UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                //    }
                //    _app.IN_LotTrans.DeleteObject(lot);
                //}

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
                var access = Session["IN10400"] as AccessRight;

                //_objBatch = data.ConvertToObject<IN10400_pcINAdjustmentBatch_Result>();
                string branchID = data["txtBranchID"];
                string batNbr = data["cboBatNbr"];
                _objBatch = _objBatch = _app.IN10400_pcINAdjustmentBatch(branchID).FirstOrDefault(p => p.BatNbr == batNbr);
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

                var lstTrans = _app.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                var trans = lstTrans.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.LineRef == lineRef);

                if (trans != null)
                {
                    double oldQty = 0;
                    if (trans.TranType == "II")
                    {
                        oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                        UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                    }

                    lstTrans.Remove(trans);
                    _app.IN_Trans.DeleteObject(trans);
                }

                //var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef).ToList();
                //foreach (var lot in lstLot)
                //{
                //    double oldQty = 0;
                //    if (lot.TranType == "II")
                //    {
                //        oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                //        UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                //    }
                //    _app.IN_LotTrans.DeleteObject(lot);
                //}

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


        #endregion

        #region Process
        private bool UpdateINAlloc(string invtID, string siteID, double oldQty, double newQty)
        {
            try
            {
                var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);

                    if (objSite == null) objSite = new IN_ItemSite() { SiteID = siteID, InvtID = invtID };

                    if (!_objIN.NegQty && newQty > 0 && objSite.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new string[] { invtID, siteID });
                    }
                   objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + newQty - oldQty, _decQty);
                   objSite.QtyAvail = Math.Round(objSite.QtyAvail - newQty + oldQty, _decQty);

                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private void SaveData(FormCollection data)
        {

            string branchID = data["txtBranchID"];
            string batNbr = data["cboBatNbr"];

            if (_lstTrans == null)
            {
                var transHandler = new StoreDataHandler(data["lstTrans"]);
                _lstTrans = transHandler.ObjectData<IN10400_pgAdjustmentLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }
            //if (_lstLot == null)
            //{
            //    var lotHandler = new StoreDataHandler(data["lstLot"]);
            //    _lstLot = lotHandler.ObjectData<IN_LotTrans>().Where(p => Util.PassNull(p.INTranLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            //}
            if (string.IsNullOrEmpty(batNbr))
            {

                _objBatch = _app.IN10400_pcINAdjustmentBatch(branchID).FirstOrDefault(p => p.BatNbr == batNbr);
                if (_objBatch == null)
                {
                    _objBatch = new IN10400_pcINAdjustmentBatch_Result();
                    _objBatch.EditScrnNbr = "IN10400";
                    _objBatch.Descr = data["txtDescr"];
                    _objBatch.ReasonCD = data["cboReasonCD"];
                    _objBatch.BranchID = branchID;
                    _objBatch.TotAmt = Convert.ToDouble(data["txtTotAmt"]);
                    _objBatch.Module = "IN";
                    _objBatch.JrnlType = "IN";
                    _objBatch.DateEnt = Convert.ToDateTime(data["txtDateEnt"]);

                }
            }
            else {
                _objBatch = _app.IN10400_pcINAdjustmentBatch(branchID).FirstOrDefault(p => p.BatNbr == batNbr);
                if (_objBatch != null)
                {
                    var bacth = new StoreDataHandler(data["lstbatch"]);
                    _objBatch = new IN10400_pcINAdjustmentBatch_Result();
                    _objBatch = bacth.ObjectData<IN10400_pcINAdjustmentBatch_Result>().FirstOrDefault(p=>p.BatNbr == batNbr);
                }
            }
            _decAmt = _app.vs_Configurations.FirstOrDefault(p => p.Code.ToLower() == "DecPlTranAmt".ToLower()).IntVal;
            _decPrice = _app.vs_Configurations.FirstOrDefault(p => p.Code.ToLower() == "DecPlUnitPrice".ToLower()).IntVal;
            _decQty = _app.vs_Configurations.FirstOrDefault(p => p.Code.ToLower() == "DecPlQty".ToLower()).IntVal;

            _handle = data["cboHandle"].PassNull();
            _objBatch.Status = _objBatch.Status.PassNull() == string.Empty ? "H" : _objBatch.Status;

            var cfgWrkDateChk = _sys.SYS_CloseDateSetUp.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            if (cfgWrkDateChk != null && cfgWrkDateChk.WrkDateChk)
            {
                DateTime tranDate = _objBatch.DateEnt;
                if (!((DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(-1 * cfgWrkDateChk.WrkLowerDays)) >=
                       0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) <= 0)
                      ||
                      (DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(cfgWrkDateChk.WrkUpperDays)) <=
                       0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) >= 0)
                      || DateTime.Compare(tranDate, cfgWrkDateChk.WrkAdjDate) == 0))
                {
                    throw new MessageException(MessageType.Message, "301");
                }
            }
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

                Save_Batch(batch, data,_objBatch);

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

                        inventory.IN10400_Release(_objBatch.BranchID, _objBatch.BatNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                    //else if (_handle == "C" || _handle == "V")
                    //{
                    //    dal.BeginTrans(IsolationLevel.ReadCommitted);

                    //    inventory.IN10400_Release(_objBatch.BranchID, _objBatch.BatNbr, );

                    //    dal.CommitTrans();

                    //    Util.AppendLog(ref _logMessage, "9999", "");
                    //}

                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
                finally
                {
                    inventory = null;
                    dal = null;
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

                //IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                //if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack != "N")
                //{
                //    var lstLot = _lstLot.Where(p => p.INTranLineRef == _lstTrans[i].LineRef).ToList();
                //    double lotQty = 0;
                //    foreach (var item in lstLot)
                //    {
                //        if (item.InvtID != _lstTrans[i].InvtID || item.SiteID != _lstTrans[i].SiteID)
                //        {
                //            throw new MessageException("2015040501", new[] { _lstTrans[i].InvtID });
                //        }

                //        if (item.UnitMultDiv.PassNull() == string.Empty || item.UnitDesc.PassNull() == string.Empty)
                //        {
                //            throw new MessageException("2015040503", new[] { _lstTrans[i].InvtID });
                //        }

                //        lotQty += Math.Round(item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact, 0);
                //    }
                //    double detQty = Math.Round(_lstLot[i].UnitMultDiv == "M" ? _lstTrans[i].Qty * _lstTrans[i].CnvFact : _lstTrans[i].Qty / _lstTrans[i].CnvFact, 0);
                //    if (detQty != lotQty)
                //    {
                //        throw new MessageException("2015040502", new[] { _lstTrans[i].InvtID });
                //    }
                //}
            }
        }
        private void Save_Batch(Batch batch,FormCollection data,IN10400_pcINAdjustmentBatch_Result s)
        {
           

            if (batch != null)
            {
                if (batch.tstamp.ToHex() != s.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }

                Update_Batch(batch, false);

            }
            else
            {
                _objBatch.BatNbr = _app.INNumbering(_objBatch.BranchID, "BatNbr").FirstOrDefault();
                _objBatch.RefNbr = _app.INNumbering(_objBatch.BranchID, "RefNbr").FirstOrDefault();
                batch = new Batch();
                Update_Batch(batch, true);
                _app.Batches.AddObject(batch);
            }

            Save_Trans(batch);
        }
        private void Save_Trans(Batch batch)
        {
            _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            if (_objIN == null) _objIN = new IN_Setup();
            foreach (var trans in _lstTrans)
            {

                var transDB = _app.IN_Trans.FirstOrDefault(p =>
                                p.BatNbr == _objBatch.BatNbr && p.RefNbr == trans.RefNbr &&
                                p.BranchID == _objBatch.BranchID && p.LineRef == trans.LineRef);

                if (transDB != null)
                {
                    if (transDB.tstamp.ToHex() != trans.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    Update_Trans(batch, transDB, trans, false);
                }
                else
                {
                    transDB = new IN_Trans();
                    Update_Trans(batch, transDB, trans, true);
                    _app.IN_Trans.AddObject(transDB);
                }
                //Save_Lot(batch, transDB);
            }

            _app.SaveChanges();
        }
        private void Update_Batch(Batch t, bool isNew)
        {
            if (isNew)
            {
                t.ResetET();
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                t.RefNbr = _objBatch.RefNbr;
                t.Module = "IN";

                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
                t.Crtd_DateTime = DateTime.Now;
            }
            else
                t.RefNbr = _objBatch.RefNbr;

            t.JrnlType = _objBatch.JrnlType.PassNull() == string.Empty ? "IN" : _objBatch.JrnlType;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.DateEnt = _objBatch.DateEnt.ToDateShort();
            t.Descr = _objBatch.Descr;
            t.EditScrnNbr = t.EditScrnNbr.PassNull() == string.Empty ? _screenNbr : t.EditScrnNbr;
           // t.FromToSiteID = _objBatch.FromToSiteID;
           // t.IntRefNbr = _objBatch.IntRefNbr;
            t.NoteID = 0;
           // t.RvdBatNbr = _objBatch.RvdBatNbr;
            t.ReasonCD = _objBatch.ReasonCD;
            t.TotAmt = Math.Round(_objBatch.TotAmt, _decAmt);
            t.Rlsed = 0;
            t.Status = _objBatch.Status;
        }
        private void Update_Trans(Batch batch, IN_Trans t, IN10400_pgAdjustmentLoad_Result s, bool isNew)
        {
            double oldQty, newQty;

            if (s.TranType == "AJ")
            {
                if (!isNew)
                    oldQty = t.UnitMultDiv == "M" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                else
                    oldQty = 0;

                newQty = s.UnitMultDiv == "M" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

                UpdateINAlloc(t.InvtID, t.SiteID, oldQty, 0);

                UpdateINAlloc(s.InvtID, s.SiteID, 0, newQty);

            }

            if (isNew)
            {
                t.ResetET();

                t.LineRef = s.LineRef;
                t.BranchID = _objBatch.BranchID;
                t.BatNbr = _objBatch.BatNbr;
                t.RefNbr = _objBatch.RefNbr;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;


            t.ReasonCD = batch.ReasonCD;
            t.CnvFact = s.CnvFact;
            t.ExtCost = Math.Round(s.TranAmt, _decAmt);
            t.InvtID = s.InvtID;
            t.InvtMult = s.InvtMult;
            t.JrnlType = s.JrnlType;
            t.ObjID = s.ObjID;
            t.Qty = Math.Round(s.Qty, _decQty);
            t.SiteID = s.SiteID;
            t.ToSiteID = s.ToSiteID;
            t.ShipperID = s.ShipperID;
            t.ShipperLineRef = s.ShipperLineRef;
            t.TranAmt = Math.Round(s.TranAmt, _decAmt);
            t.TranFee = s.TranFee;
            t.TranDesc = s.TranDesc;
            t.TranType = s.TranType;
            t.TranDate = batch.DateEnt;
            t.UnitCost = Math.Round(s.UnitPrice, _decPrice);
            t.UnitDesc = s.UnitDesc;
            t.UnitMultDiv = s.UnitMultDiv;
            t.UnitPrice = Math.Round(s.UnitPrice, _decPrice);
           // t.SlsperID = _form["SlsperID"].PassNull();
        }
        #endregion
        

    }
}

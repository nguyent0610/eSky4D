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
namespace IN10200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10200Controller : Controller
    {
        private string _screenNbr = "IN10200";
        private string _userName = Current.UserName;
        private string _tstamp = "";
        private string _handle = "";
        private IN10200Entities _app = Util.CreateObjectContext<IN10200Entities>();
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);     
        private FormCollection _form;
        private IN10200_pcBatch_Result _objBatch;
        private JsonResult _logMessage;
        private List<IN10200_pgIssueLoad_Result> _lstTrans;
        private IN_Setup _objIN;
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetBatch(string branchID, string query, int start, int limit, int page)
        {
            query = query ?? string.Empty;
            if (page != 1) query = string.Empty;
            var lstBatch = _app.IN10200_pcBatch(branchID, query, start, start + 20).ToList();
            var paging = new Paging<IN10200_pcBatch_Result>(lstBatch, lstBatch.Count > 0 ? lstBatch[0].TotalRecords.Value : 0);
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
        public ActionResult GetTrans(string batNbr, string branchID)
        {
            var lstTrans = _app.IN10200_pgIssueLoad(batNbr, branchID, "%", "%").ToList();
            return this.Store(lstTrans);
        }

        public ActionResult GetItemSite(string invtID, string siteID)
        {
            var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
            return this.Store(objSite);
        }
        public ActionResult GetUnitConversion()
        {
            var lstUnit = _app.IN10200_pcUnitConversion(Current.CpnyID).ToList();
            return this.Store(lstUnit);
        }
        public ActionResult GetUnit(string invtID)
        {
            IN_Inventory invt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (invt == null) invt = new IN_Inventory();
            List<IN10200_pcUnit_Result> lstUnit = _app.IN10200_pcUnit(invt.ClassID, invt.InvtID).ToList();
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
                return Json(new { success = true, data = new { batNbr = _objBatch.BatNbr } });
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
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                var access = Session["IN10200"] as AccessRight;
                if (!access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();

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
                    if (trans.TranType == "II")
                    {
                        oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                        UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                    }
                    _app.IN_Trans.DeleteObject(trans);
                }

                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Json(new { success = true });
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
                var access = Session["IN10200"] as AccessRight;

                _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();

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
                return Json(new { success = true, tstamp  });
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
        private void SaveData(FormCollection data)
        {

            var transHandler = new StoreDataHandler(data["lstTrans"]);
            if (_lstTrans == null)
            {
                _lstTrans = transHandler.ObjectData<IN10200_pgIssueLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }

            _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();

            _handle = data["Handle"].PassNull();
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
            _tstamp = data["tstamp"].StartsWith("0x") ? data["tstamp"].ToHex() : data["tstamp"].ToString();
            Batch batch = _app.Batches.FirstOrDefault(p => p.BatNbr == _objBatch.BatNbr && p.BranchID == _objBatch.BranchID);
            if ((_objBatch.Status == "U" || _objBatch.Status == "C") && (_handle == "C" || _handle == "V"))
            {
                if (batch != null)
                {
                    if (batch.tstamp.ToHex() != _tstamp)
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
                try
                {
                    INProcess.Inventory inventory = new INProcess.Inventory(_userName, _screenNbr, dal);
                    if (_handle == "R")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!inventory.IN10200_Release(_objBatch.BranchID, _objBatch.BatNbr))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
                    else if (_handle == "C" || _handle == "V")
                    {
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!inventory.IN10200_Cancel(_objBatch.BranchID, _objBatch.BatNbr))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }
                        Util.AppendLog(ref _logMessage, "9999", "");
                    }
                    inventory = null;
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
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

            if (_objBatch.Status.PassNull() != "H" && (_handle==string.Empty || _handle=="N"))
            {
                throw new MessageException(MessageType.Message, "2015020803");
            }

           
            if (_lstTrans.Count == 0)
            {
                throw new MessageException(MessageType.Message, "2015020804", "");
            }
        }
        private void Save_Batch(Batch batch)
        {
            if (batch != null)
            {
                if (batch.tstamp.ToHex() != _tstamp)
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
            t.FromToSiteID = _objBatch.FromToSiteID;
            t.IntRefNbr = _objBatch.IntRefNbr;
            t.NoteID = 0;
            t.RvdBatNbr = _objBatch.RvdBatNbr;
            t.ReasonCD = _objBatch.ReasonCD;
            t.TotAmt = _objBatch.TotAmt;
            t.Rlsed = 0;
            t.Status = _objBatch.Status;
        }
        private void Update_Trans(Batch batch, IN_Trans t, IN10200_pgIssueLoad_Result s, bool isNew)
        {
            double oldQty, newQty;

            if (s.TranType == "II")
            {
                if (!isNew)
                    oldQty = t.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                else
                    oldQty = 0;

                newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

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
            t.ExtCost = Math.Round(s.ExtCost, 0);
            t.InvtID = s.InvtID;
            t.InvtMult = s.InvtMult;
            t.JrnlType = s.JrnlType;
            t.ObjID = s.ObjID;
            t.Qty = s.Qty;
            t.SiteID = s.SiteID;
            t.ToSiteID = s.ToSiteID;
            t.ShipperID = s.ShipperID;
            t.ShipperLineRef = s.ShipperLineRef;
            t.TranAmt = s.TranAmt;
            t.TranFee = s.TranFee;
            t.TranDesc = s.TranDesc;
            t.TranType = s.TranType;
            t.TranDate = batch.DateEnt;
            t.UnitCost = s.UnitCost;
            t.UnitDesc = s.UnitDesc;
            t.UnitMultDiv = s.UnitMultDiv;
            t.UnitPrice = s.UnitPrice;
            t.SlsperID = _form["SlsperID"].PassNull();
        }


        private bool UpdateINAlloc(string invtID, string siteID, double oldQty, double newQty)
        {
            try
            {
                var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objSite = _app.IN_ItemSite.FirstOrDefault(p=> p.InvtID == invtID && p.SiteID == siteID);
                    if (objSite != null)
                    {
                        if (!_objIN.NegQty && newQty > 0 && objSite.QtyAvail + oldQty - newQty < 0)
                        {
                            throw new MessageException(MessageType.Message, "608","",new string[] {invtID,siteID});
                        }
                        objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + newQty - oldQty, 0);
                        objSite.QtyAvail = Math.Round(objSite.QtyAvail - newQty + oldQty, 0);
                    }
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                _form = data;
                _objBatch = data.ConvertToObject<IN10200_pcBatch_Result>();
                User user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
                var rpt = new RPTRunning();
                rpt.ResetET();

                rpt.ReportNbr = "IN602";
                rpt.MachineName = "Web";
                rpt.ReportCap = "IN_Issue";
                rpt.ReportName = "IN_Issue";
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

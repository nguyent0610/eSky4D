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
        private List<IN10300_pdGetDataItemTrans_Result> _lstItemTrans;
        List<Batch> _lstBatch = new List<Batch>();
        List<IN_MaxTranfers> _lstIN_MaxTranfers = new List<IN_MaxTranfers>();
        List<IN_Transfer> _lstIN_Transfer = new List<IN_Transfer>();
        List<IN_Trans> _lstIN_Trans = new List<IN_Trans>();


        private List<IN10300_pgIN_LotTrans_Result> _lstLot;
        private IN_Setup _objIN;
        private string _branch = "";
        private string _advanceType = "";
        private string _batNbr = "";
        private int _lineRefnumber = 0;
        private string _LineRef = string.Empty;
        public ActionResult Index(string branchID)
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);

            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);

            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }

            if (branchID == null) branchID = Current.CpnyID;

            bool hideRptExpDate = false
                , hideWarehouss = false, readOnlyReasonCD = false, hideExpectedDateRcptDate = false
                , allowDescrBlank = false, allowNoteBlank=false, isSetDefaultShipViaID=false, isSetDefaultSiteID=false, dflReasonCD=false;

            bool showImprtExprt = false;
            bool checkperPost = false;
            string perPost = "";
            var objConfig = _app.IN10300_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            var showWhseLoc = 0;
            if (objConfig != null)
            {
                hideRptExpDate = objConfig.HideRptExpDate.HasValue ? objConfig.HideRptExpDate.Value : false;
                hideWarehouss = objConfig.HideWarehous.HasValue ? objConfig.HideWarehous.Value : false;
                readOnlyReasonCD = objConfig.ReadOnlyReasonCD.HasValue ? objConfig.ReadOnlyReasonCD.Value : false;
                hideExpectedDateRcptDate = objConfig.HideExpectedDateRcptDate.HasValue ? objConfig.HideExpectedDateRcptDate.Value : false;
                allowDescrBlank = objConfig.AllowDescrBlank.HasValue ? objConfig.AllowDescrBlank.Value : false;
                allowNoteBlank = objConfig.AllowNoteBlank.HasValue ? objConfig.AllowNoteBlank.Value : false;
                isSetDefaultShipViaID = objConfig.IsSetDefaultShipViaID.HasValue ? objConfig.IsSetDefaultShipViaID.Value : false;
                isSetDefaultSiteID = objConfig.IsSetDefaultSiteID.HasValue ? objConfig.IsSetDefaultSiteID.Value : false;
                dflReasonCD = objConfig.DflReasonCD.HasValue ? objConfig.DflReasonCD.Value : false;
                showImprtExprt = objConfig.ShowImprtExprt.HasValue ? objConfig.ShowImprtExprt.Value : false;
                showWhseLoc = objConfig.showWhseLoc;
                perPost = objConfig.PerPost;
                checkperPost = objConfig.CheckPerPost.HasValue && objConfig.CheckPerPost.Value;
            }           
            ViewBag.hideRptExpDate = hideRptExpDate;
            ViewBag.hideWarehouss = hideWarehouss;
            ViewBag.readOnlyReasonCD = readOnlyReasonCD;
            ViewBag.hideExpectedDateRcptDate = hideExpectedDateRcptDate;
            ViewBag.allowDescrBlank = allowDescrBlank;
            ViewBag.allowNoteBlank = allowNoteBlank;
            ViewBag.isSetDefaultShipViaID = isSetDefaultShipViaID;
            ViewBag.isSetDefaultSiteID = isSetDefaultSiteID;
            ViewBag.dflReasonCD = dflReasonCD;
            ViewBag.showImprtExprt = showImprtExprt;
            ViewBag.BranchID = branchID;
            ViewBag.showWhseLoc = showWhseLoc;
            ViewBag.PerPost = perPost;
            ViewBag.CheckperPost = checkperPost;
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
            var lstBatch = _app.IN10300_pcBatch(Current.UserName, Current.CpnyID, Current.LangID, branchID, query, start, start + 20).ToList();
            var paging = new Paging<IN10300_pcBatch_Result>(lstBatch, lstBatch.Count > 0 ? lstBatch[0].TotalRecords.Value : 0);
            return this.Store(paging.Data, paging.TotalRecords);
        }

        


        public ActionResult GetUserDefault(string BranchID)
        {
            //string tam = BranchID;
            //int bd = tam.LastIndexOf("=");
            //_branch = tam.Substring(bd+1);
            var objUser = _app.IN10300_pdOM_UserDefault(Current.CpnyID,Current.UserName,Current.LangID, BranchID).FirstOrDefault();
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
            
            var lstTrans = _app.IN10300_pgTransferLoad(Current.UserName, Current.CpnyID, Current.LangID, batNbr, branchID, "%", refNbr).ToList();
            return this.Store(lstTrans);
        }

        public ActionResult GetLot(string invtID, string siteID, string batNbr, string branchID, string whseLoc, int showWhseLoc, int cnvFact)
        {
            List<IN10300_pdGetLot_Result> lstLot = new List<IN10300_pdGetLot_Result>();
            List<IN10300_pdGetLot_Result> lstLotDB = new List<IN10300_pdGetLot_Result>();
            if (showWhseLoc == 2 || (whseLoc.PassNull() != "" && showWhseLoc == 1))
            {
                lstLotDB = _app.IN10300_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).Where(p => p.QtyAvail > 0 && p.WhseLoc==whseLoc).ToList();
                foreach (var item in lstLotDB)
                {
                    item.QtyAvail = Math.Floor(item.QtyAvail / cnvFact);
                    lstLot.Add(item);
                }

                List<IN_LotTrans> lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc==whseLoc).ToList();
                foreach (var item in lstLotTrans)
                {
                    var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                    if (lot == null)
                    {
                        var lotDB = _app.IN10300_pdGetLot(siteID,invtID,Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr && p.WhseLoc==whseLoc);
                        lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        lstLot.Add(lotDB);
                    }
                    else
                    {
                        lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                    }
                }
            }
            else
            {
                lstLotDB = _app.IN10300_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).Where(p => p.QtyAvail > 0).ToList();
                foreach (var item in lstLotDB)
                {
                    item.QtyAvail = Math.Floor(item.QtyAvail / cnvFact);
                    lstLot.Add(item);
                }

                List<IN_LotTrans> lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID).ToList();
                foreach (var item in lstLotTrans)
                {
                    var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                    if (lot == null)
                    {
                        var lotDB = _app.IN10300_pdGetLot(siteID,invtID,Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                        lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        lstLot.Add(lotDB);
                    }
                    else
                    {
                        lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                    }
                }
            }            
            lstLot = lstLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();
            return this.Store(lstLot, lstLot.Count);
            //return this.Store(lstLot.OrderBy(p => p.LotSerNbr).ToList(), lstLot.Count);
        }
        public ActionResult GetLotTrans(string branchID, string batNbr)
        {
            List<IN10300_pgIN_LotTrans_Result> lstLotTrans = _app.IN10300_pgIN_LotTrans(batNbr,branchID,Current.UserName,Current.CpnyID,Current.LangID).Where(p => p.ToSiteID!="").ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetItemLot(string invtID, string siteID, string lotSerNbr, string branchID, string batNbr, string whseLoc, int showWhseLoc)
        {
            var lot = new IN10300_pdGetLot_Result();
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                lot = _app.IN10300_pdGetLot(siteID,invtID,Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr && p.WhseLoc==whseLoc);

                if (lot == null) lot = new IN10300_pdGetLot_Result()
                {
                    InvtID = invtID,
                    SiteID = siteID,
                    LotSerNbr = lotSerNbr
                };

                var lotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr && p.WhseLoc==whseLoc).ToList();

                foreach (var item in lotTrans)
                {
                    lot.QtyAvail += (item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact);
                }
            }
            else
            {
                lot = _app.IN10300_pdGetLot(siteID,invtID,Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr);

                if (lot == null) lot = new IN10300_pdGetLot_Result()
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
            }
            List<IN10300_pdGetLot_Result> lstLot = new List<IN10300_pdGetLot_Result>() { lot };
            return this.Store(lstLot, lstLot.Count);
        }
        
        public ActionResult GetItemSite(string invtID, string siteID,string whseLoc,int showWhseLoc)
        {
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                var objSite = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc==whseLoc);
                return this.Store(objSite);
            }
            else
            {
                var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
                return this.Store(objSite);
            }                       
        }
        public ActionResult GetUnitConversion()
        {
            var lstUnit = _app.IN10300_pcUnitConversion(Current.CpnyID).ToList();
            return this.Store(lstUnit);
        }
        public ActionResult GetPrice(string invtID, string uom, DateTime effDate,string branchID,string siteID,string valMthd)
        {
            var lstPrice = _app.IN10300_pdPrice("", invtID, uom, DateTime.Now,branchID,siteID,valMthd,Current.UserName,Current.CpnyID,Current.LangID).ToList();
            return this.Store(lstPrice);
        }
        public ActionResult GetUnit(string invtID)
        {
            List<IN10300_pcUnit_Result> lstUnit = _app.IN10300_pcUnit(Current.CpnyID, Current.UserName, Current.LangID, invtID).ToList();
            return this.Store(lstUnit, lstUnit.Count);
        }

        public ActionResult GetAllInvtINSite(string branchID, string SiteID)
        {
            var lstTrans = _app.IN10300_pgIN_ItemSite(Current.UserName, Current.CpnyID, Current.LangID, branchID, SiteID).ToList();
            return this.Store(lstTrans);
        }
        public ActionResult GetLotTransVanSale(string BranchID, string SiteID,string ToSiteID)
        {
            var lstTransVanSale = _app.IN10300_pgLotTransVanSale(Current.UserName, Current.CpnyID, Current.LangID, BranchID, SiteID,ToSiteID).ToList();
            return this.Store(lstTransVanSale);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                _advanceType = data["cboAdvanceType"].PassNull();
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

                    if (!string.IsNullOrWhiteSpace(trans.WhseLoc.PassNull()))
                    {
                        UpdateINAllocWhseLoc(trans.InvtID, trans.SiteID,trans.WhseLoc, oldQty, 0);
                    }
                    _app.IN_Trans.DeleteObject(trans);
                }

                var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;

                    oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;

                    UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr,lot.WhseLoc, oldQty, 0, 0);

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
                    if (!string.IsNullOrWhiteSpace(trans.WhseLoc.PassNull()))
                    {
                        UpdateINAllocWhseLoc(trans.InvtID, trans.SiteID, trans.WhseLoc, oldQty, 0);
                    }    
                    lstTrans.Remove(trans);
                    _app.IN_Trans.DeleteObject(trans);
                }

                var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef).ToList();
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;

                    oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                    UpdateAllocLot(lot.InvtID, lot.SiteID,lot.LotSerNbr,lot.WhseLoc, oldQty, 0,0);

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
                _lstTrans = transHandler.ObjectData<IN10300_pgTransferLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty && Util.PassNull(p.InvtID)!=string.Empty).ToList();
            }
            if (_lstLot == null)
            {
                var lotHandler = new StoreDataHandler(data["lstLot"]);
                _lstLot = lotHandler.ObjectData<IN10300_pgIN_LotTrans_Result>().Where(p => Util.PassNull(p.INTranLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            }

            _objBatch = data.ConvertToObject<IN10300_pcBatch_Result>();

            _handle = data["Handle"].PassNull();
            _objBatch.Status = _objBatch.Status.PassNull() == string.Empty ? "H" : _objBatch.Status;

            if (_app.IN10300_ppCheckCloseDate(Current.UserName,Current.CpnyID,Current.LangID,_objBatch.BranchID, _objBatch.TranDate.ToDateShort(), "IN10300").FirstOrDefault() == "0")
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
                if (_handle == "R")
                {
                    string periodID = "";

                    if (_objBatch.TranDate.Month.ToString().Length == 1)
                    {
                        periodID = _objBatch.TranDate.Year.ToString() + "0" + _objBatch.TranDate.Month.ToString();
                    }
                    else
                    {
                        periodID = _objBatch.TranDate.Year.ToString() + _objBatch.TranDate.Month.ToString();
                    }
                    string lstInvtID = "";
                    foreach (var item in _lstTrans)
                    {
                        lstInvtID = lstInvtID + item.InvtID + "@#@#" + item.Qty + "@#@#" + item.UnitDesc +",";
                    }
                    string messageerorr="";
                    var checkQty = _app.IN10300_pdCheckQtyInvtInGrd(lstInvtID, _objBatch.SiteID, _objBatch.ToSiteID, _objBatch.BranchID, periodID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                    if (checkQty != null)
                    {
                        messageerorr = string.Format(Message.GetString("2018033016", null), checkQty.InvtID, _objBatch.ToSiteID);
                        throw new MessageException(MessageType.Message, "20410", "", new string[] { messageerorr });                       
                    }
                }

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

                var objInvt = GetInvt(invtID);
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

                    UpdateAllocLot(item.InvtID, item.SiteID, item.LotSerNbr,item.WhseLoc, oldQty, 0, 0);

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
            t.PerPost = _objBatch.PerPost;
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
            t.AdvanceType = _advanceType;
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
            t.WhseLoc = _objBatch.WhseLoc;
            t.ToWhseLoc = _objBatch.ToWhseLoc;
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

            // update IN_ItemSite
            UpdateINAlloc(t.InvtID, t.SiteID, oldQty, 0);
            UpdateINAlloc(s.InvtID, s.SiteID, 0, newQty);
            // Update IN_ItemLoc
            UpdateINAllocWhseLoc(t.InvtID, t.SiteID,t.WhseLoc, oldQty, 0);
            UpdateINAllocWhseLoc(s.InvtID, s.SiteID,s.WhseLoc, 0, newQty);
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
            t.WhseLoc = s.WhseLoc;
            t.ToWhseLoc = s.ToWhseLoc;
            t.RptExpDate = s.RptExpDate;
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
        private bool Update_Lot(IN_LotTrans t, IN10300_pgIN_LotTrans_Result s, IN_Transfer transfer, IN_Trans tran, bool isNew)
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
                t.WarrantyDate = s.WarrantyDate;//DateTime.Now.ToDateShort();
            }

            double oldQty = 0;
            double newQty = 0;
           
            if (!isNew)
                oldQty = s.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
            else
                oldQty = 0;

            newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

            UpdateAllocLot(t.InvtID, t.SiteID, t.LotSerNbr,t.WhseLoc, oldQty, 0, 0);

            if (!UpdateAllocLot(s.InvtID, s.SiteID, t.LotSerNbr,s.WhseLoc, 0, newQty, 0))
            {
                throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
            }
            t.WhseLoc = s.WhseLoc;
            t.ToWhseLoc = s.ToWhseLoc;
            t.Reason = s.Reason;
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
                var objInvt = GetInvt(invtID);
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

        private bool UpdateINAllocWhseLoc(string invtID, string siteID,string whseLoc, double oldQty, double newQty)
        {
            try
            {
                var objInvt = GetInvt(invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objWhseLoc = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc==whseLoc);
                    if (objWhseLoc == null) objWhseLoc = new IN_ItemLoc() { InvtID = invtID, SiteID = siteID };
                    if (!_objIN.NegQty && newQty > 0 && objWhseLoc.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new string[] { invtID, siteID });
                    }
                    objWhseLoc.QtyAllocIN = Math.Round(objWhseLoc.QtyAllocIN + newQty - oldQty, 0);
                    objWhseLoc.QtyAvail = Math.Round(objWhseLoc.QtyAvail - newQty + oldQty, 0);
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool UpdateAllocLot(string invtID, string siteID, string lotSerNbr, string whseLoc, double oldQty, double newQty, int decQty)
        {
            var objInvt = GetInvt(invtID);
            if (objInvt.StkItem == 1)
            {
                var objItemLot = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc);
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

        private IN10300_pdIN_Inventory_Result GetInvt(string invtID)
        {
            var objInvt = _app.IN10300_pdIN_Inventory(Current.CpnyID, Current.UserName, Current.LangID, invtID).FirstOrDefault();
            if (objInvt == null)
            {
                objInvt = new IN10300_pdIN_Inventory_Result();
            } 
            return objInvt;
        }
        #region Export
        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
                Cell cell;
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = Util.GetLang("IN10300Export");
                SetCellValueGrid(SheetData.Cells["A1"], Util.GetLang("IN10300ListDiscSeq"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["B1"], Util.GetLang("IN10300BranchID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["C1"], Util.GetLang("IN10300RefBatNbr"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["D1"], Util.GetLang("IN10300BatchDr"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["E1"], Util.GetLang("IN10300Comment"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["F1"], Util.GetLang("IN10300RcptDate") + " \r\n" + " (MM/dd/yyyy)", TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["G1"], Util.GetLang("IN10300FromDate") + " \r\n" + " (MM/dd/yyyy)", TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["H1"], Util.GetLang("IN10300ToDate") + " \r\n" + " (MM/dd/yyyy)", TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["I1"], Util.GetLang("IN10300InvtID"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["J1"], Util.GetLang("IN10300QtyMaximum"), TextAlignmentType.Center, TextAlignmentType.Left);
                SetCellValueGrid(SheetData.Cells["K1"], Util.GetLang("IN10300QtyTU"), TextAlignmentType.Center, TextAlignmentType.Left);


                Style colStyle = SheetData.Cells.Columns[2].Style;
                Style colStyle1 = SheetData.Cells.Columns[3].Style;
                StyleFlag flag = new StyleFlag();
                flag.NumberFormat = false;
                colStyle.Number = 49;
                colStyle1.Number = 49;
                SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[4].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[5].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[6].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[7].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[8].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[9].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[10].ApplyStyle(colStyle, flag);
                SheetData.Cells.Columns[11].ApplyStyle(colStyle, flag);

                //SheetData.Cells.Columns[8].ApplyStyle(colStyle, flag);


                SheetData.Cells.SetColumnWidth(0, 15);
                SheetData.Cells.SetColumnWidth(1, 15);
                SheetData.Cells.SetColumnWidth(2, 15);
                SheetData.Cells.SetColumnWidth(3, 15);
                SheetData.Cells.SetColumnWidth(4, 15);
                SheetData.Cells.SetColumnWidth(5, 15);
                SheetData.Cells.SetColumnWidth(6, 15);
                SheetData.Cells.SetColumnWidth(7, 15);
                SheetData.Cells.SetColumnWidth(8, 15);
                SheetData.Cells.SetColumnWidth(9, 15);
                SheetData.Cells.SetColumnWidth(10, 15);
                //SheetData.Cells.SetColumnWidth(7, 15);
                //var style = SheetData.Cells["F2"].GetStyle();
                //style.Custom = "MM/dd/yyyy";
                //style.Font.Color = Color.Black;
                //style.HorizontalAlignment = TextAlignmentType.Left;
                //var range = SheetData.Cells.CreateRange("F2", "E2000");
                //range.SetStyle(style);
                //range = SheetData.Cells.CreateRange("G2", "G2000");
                //range.SetStyle(style);
                //range = SheetData.Cells.CreateRange("H2", "H1000");
                //range.SetStyle(style);
                var style = SheetData.Cells["B2"].GetStyle();
                style.Number = 49;
                var range = SheetData.Cells.CreateRange("B2", "B" + 1000);
                range.SetStyle(style);

                style = SheetData.Cells["G1"].GetStyle();
                style.Number = 14;
                range = SheetData.Cells.CreateRange("G2", "G" + 1000);
                range.SetStyle(style);

                style = SheetData.Cells["F1"].GetStyle();
                style.Number = 14;
                range = SheetData.Cells.CreateRange("F2", "F" + 1000);
                range.SetStyle(style);

                style = SheetData.Cells["H1"].GetStyle();
                style.Number = 14;
                range = SheetData.Cells.CreateRange("H2", "H" + 1000);
                range.SetStyle(style);

                style = SheetData.Cells["J1"].GetStyle();
                style.Number = 1;
                range = SheetData.Cells.CreateRange("J2", "J" + 1000);
                range.SetStyle(style);

                style = SheetData.Cells["K1"].GetStyle();
                style.Number = 1;
                range = SheetData.Cells.CreateRange("K2", "K" + 1000);
                range.SetStyle(style);


                //CommentPGType
                int commentIndex = SheetData.Comments.Add("N3");



                Validation validation = SheetData.Validations[SheetData.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                string filename = Util.GetLang("IN10300Export") + ".xlsx";
                var fileName = filename;
                string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);

                workbook.Settings.AutoRecover = true;

                workbook.Save(fullPath, SaveFormat.Xlsx);
                return Json(new { success = true, fileName = fileName, errorMessage = "" });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        //public ActionResult Export(FormCollection data, string templateExport)
        //{
        //    try
        //    {
        //        Stream stream = new MemoryStream();
        //        Workbook workbook = new Workbook();
        //        string filename = string.Empty;
        //        Worksheet SheetData = workbook.Worksheets[0];
        //        SheetData.Name = Util.GetLang("ExportIN10300");
        //        workbook.Worksheets.Add();
        //        DataAccess dal = Util.Dal();
        //        //Chương trình Khuyến mãi
        //        SetCellValueGrid(SheetData.Cells["A1"], Util.GetLang("IN10300ListDiscSeq"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["B1"], Util.GetLang("IN10300BranchID"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["C1"], Util.GetLang("IN10300RefBatNbr"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["D1"], Util.GetLang("IN10300BatchDr"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["E1"], Util.GetLang("IN10300Comment"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["F1"], Util.GetLang("Warehousetransfer") + " \r\n" + " (MM/dd/yyyy)", TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["G1"], Util.GetLang("IN10300FromDate") + " \r\n" + " (MM/dd/yyyy)", TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["H1"], Util.GetLang("IN10300ToDate") + " \r\n" + " (MM/dd/yyyy)", TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["I1"], Util.GetLang("IN10300InvtID"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["J1"], Util.GetLang("IN10300UOM"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["K1"], Util.GetLang("IN10300QtyMaximum"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SetCellValueGrid(SheetData.Cells["L1"], Util.GetLang("IN10300QtyTU"), TextAlignmentType.Center, TextAlignmentType.Center);
        //        SheetData.Cells.SetRowHeight(0, 45);

        //        Style colStyle = SheetData.Cells.Columns[2].Style;
        //        Style colStyle1 = SheetData.Cells.Columns[3].Style;
        //        StyleFlag flag = new StyleFlag();
        //        StyleFlag flag1 = new StyleFlag();
        //        flag.NumberFormat = true;
        //        flag1.NumberFormat = false;
        //        //Set the formating on the as text formating 
        //        colStyle.Number = 49;
        //        colStyle1.Number = 49;
        //        SheetData.Cells.Columns[0].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.Columns[1].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.Columns[2].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.Columns[3].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.Columns[4].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.Columns[5].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.Columns[6].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.Columns[7].ApplyStyle(colStyle, flag);
        //        SheetData.Cells.SetColumnWidth(0, 15);
        //        SheetData.Cells.SetColumnWidth(1, 25);
        //        SheetData.Cells.SetColumnWidth(2, 15);
        //        SheetData.Cells.SetColumnWidth(3, 25);
        //        SheetData.Cells.SetColumnWidth(4, 15);
        //        SheetData.Cells.SetColumnWidth(5, 15);
        //        SheetData.Cells.SetColumnWidth(6, 15);
        //        SheetData.Cells.SetColumnWidth(7, 10);


        //        //var style1 = SheetData.Cells["G2"].GetStyle();
        //        //style1.IsLocked = true;
        //        //style1.Number = 3;
        //        //var range1 = SheetData.Cells.CreateRange("G2", "G200");
        //        //range1.SetStyle(style1);
        //        //var range2 = SheetData.Cells.CreateRange("H2", "H200");
        //        //range2.SetStyle(style1);



        //        var style = SheetData.Cells["F2"].GetStyle();
        //        style.Custom = "MM/dd/yyyy";
        //        style.Font.Color = Color.Black;
        //        style.HorizontalAlignment = TextAlignmentType.Left;
        //        var range = SheetData.Cells.CreateRange("F2", "E2000");
        //        range.SetStyle(style);
        //        range = SheetData.Cells.CreateRange("G2", "G2000");
        //        range.SetStyle(style);
        //        range = SheetData.Cells.CreateRange("H2", "H1000");
        //        range.SetStyle(style);
        //        int commentIndex = SheetData.Comments.Add("N3");
        //        Comment comment = SheetData.Comments[commentIndex];
        //        CellArea position = new CellArea();
        //        position.StartRow = 3;
        //        position.EndRow = 1000;
        //        position.StartColumn = 5;
        //        position.EndColumn = 5;              

        //        Validation validation = SheetData.Validations[SheetData.Validations.Add()];
        //        validation.IgnoreBlank = true;
        //        validation.Type = Aspose.Cells.ValidationType.List;
        //        validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
        //        validation.Operator = OperatorType.Between;
        //        var fileName = filename;
        //        string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);

        //        workbook.Settings.AutoRecover = true;

        //        workbook.Save(fullPath, SaveFormat.Xlsx);
        //        return Json(new { success = true, fileName = fileName, errorMessage = "" });

        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException)
        //        {
        //            return (ex as MessageException).ToMessage();
        //        }
        //        else
        //        {
        //            return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //        }
        //    }
        //}
        


        private void SetCellValueGridHeader(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 12;
            style.Font.Color = Color.CornflowerBlue;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }

        private void SetCellValueGrid(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 11;
            style.Font.Color = Color.Black;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            style.IsTextWrapped = true;
            c.SetStyle(style);
        }


        [HttpGet]
         //Action Filter, it will auto delete the file after download,I will explain it later
        public ActionResult DownloadAndDelete(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);
            return File(fullPath, "application/vnd.ms-excel", file);
        }

        #endregion

        #region Import
        [HttpPost]
        public ActionResult Import(FormCollection data)
        {
            try
            {
                var access = Session["IN10300"] as AccessRight;
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
                string errorlistDiscSeq = string.Empty;
                string errorbranchID = string.Empty;
                string errorrefBatNbr = string.Empty;
                string errorbatchDr = string.Empty;
                string errorcomment = string.Empty;
                string errorRcptDate = string.Empty;
                string errorfromDate = string.Empty;
                string errortoDate = string.Empty;
                string errorinvtID = string.Empty;
                string errorUOM = string.Empty;
                string errorline = string.Empty;
                string error = string.Empty;
                string errorBranch = string.Empty;
                string errorqtyMaximum = string.Empty;
                string errorqtyTU = string.Empty;
                List < IN10300_pdDeatail_Result > lstDeatail= new List<IN10300_pdDeatail_Result>();
                List<IN10300_pdDeatail_Result> lstDeatail2 = new List<IN10300_pdDeatail_Result>();
                List<IN10300_pdHeader_Result> lstHeader = new List<IN10300_pdHeader_Result>();
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        //if (workSheet.Cells[0, 0].StringValue.Trim() != Util.GetLang("IN10300ListDiscSeq")
                        //  || workSheet.Cells[0, 1].StringValue.Trim() != Util.GetLang("IN10300BranchID")
                        //  || workSheet.Cells[0, 2].StringValue.Trim() != Util.GetLang("IN10300RefBatNbr")
                        //  || workSheet.Cells[0, 3].StringValue.Trim() != Util.GetLang("IN10300BatchDr")
                        //  || workSheet.Cells[0, 4].StringValue.Trim() != Util.GetLang("IN10300Comment")
                        //  || workSheet.Cells[0, 8].StringValue.Trim() != Util.GetLang("IN10300InvtID")
                        //  || workSheet.Cells[0, 9].StringValue.Trim() != Util.GetLang("IN10300QtyMaximum")
                        //  || workSheet.Cells[0, 10].StringValue.Trim() != Util.GetLang("IN10300QtyTU")
                        //  )
                        //{
                        //    throw new MessageException(MessageType.Message, "148");
                        //}

                        string listDiscSeq = string.Empty;
                        string branchID = string.Empty;
                        string refBatNbr = string.Empty;
                        string batchDr = string.Empty;
                        string comment = string.Empty;
                        DateTime RcptDate = DateTime.Now;
                        DateTime fromDate = DateTime.Now;
                        DateTime toDate = DateTime.Now;
                        string invtID = string.Empty;
                        string UOM = string.Empty;
                        double qtyMaximum = 0;
                        double qtyTU = 0;
                        var lstBranchID = _app.IN10300_pdBanchID(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                        var lstUOM = _app.IN10300_pdUOM(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                        for (int i = 1; i <= workSheet.Cells.MaxDataRow; i++)
                        {
                            bool flagCheck = false;
                            bool checkTU = false;
                            listDiscSeq = workSheet.Cells[i, 0].StringValue.PassNull();
                            branchID = workSheet.Cells[i, 1].StringValue.PassNull();
                            refBatNbr = workSheet.Cells[i, 2].StringValue.PassNull();
                            batchDr = workSheet.Cells[i, 3].StringValue.PassNull();
                            comment = workSheet.Cells[i, 4].StringValue.PassNull();
                            invtID = workSheet.Cells[i, 8].StringValue.PassNull();
                            if (branchID == "" && batchDr == "" && invtID == "" && comment == "" && workSheet.Cells[i, 5].StringValue.PassNull() == "" && workSheet.Cells[i, 6].StringValue.PassNull() == ""
                                && workSheet.Cells[i, 7].StringValue.PassNull() == "" && workSheet.Cells[i, 10].StringValue.PassNull() == "" && workSheet.Cells[i, 11].StringValue.PassNull() == "")
                                continue;
                            //check ngày chuyển kho

                            try
                            {
                                RcptDate = workSheet.Cells[i, 5].DateTimeValue;
                            }
                            catch
                            {
                                errorRcptDate += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }

                            try
                            {
                                qtyTU = workSheet.Cells[i, 10].DoubleValue;
                                if (qtyTU < 0)
                                {
                                    errorqtyTU += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                else
                                {
                                    checkTU = true;
                                }
                            }
                            catch
                            {
                                errorqtyTU += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }
                            //check từ ngày
                            try
                            {
                                fromDate = workSheet.Cells[i, 6].DateTimeValue;
                            }
                            catch
                            {
                                errorfromDate += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }

                            //check đến ngày

                            if (checkTU)
                            {
                                if (qtyTU > 0)
                                {
                                    try
                                    {
                                        toDate = workSheet.Cells[i, 7].DateTimeValue;
                                        if ((toDate.Month < DateTime.Now.Month) && toDate.Year <= DateTime.Now.Year)
                                        {
                                            errortoDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                        else
                                        {
                                            if (fromDate.ToDateShort() > toDate.ToDateShort())
                                            {
                                                errortoDate += (i + 1).ToString() + ",";
                                                flagCheck = true;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        errortoDate += (i + 1).ToString() + ",";
                                        flagCheck = true;
                                    }

                                    if (flagCheck == false)
                                    {
                                        if (RcptDate.ToDateShort() > toDate.ToDateShort() || RcptDate.ToDateShort() < fromDate.ToDateShort())
                                        {
                                            errorRcptDate += (i + 1).ToString() + ",";
                                            flagCheck = true;
                                        }
                                    }
                                }                                
                            }                            

                            try
                            {
                                qtyMaximum = workSheet.Cells[i, 9].DoubleValue;
                                if (qtyMaximum <= 0)
                                {
                                    errorqtyMaximum += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                            }
                            catch
                            {
                                errorqtyMaximum += (i + 1).ToString() + ",";
                                flagCheck = true;
                            }

                            

                            //Kiểm tra tính đúng đắng của dữ liệu
                            if (lstBranchID != null)
                            {
                                var checkBranch = lstBranchID.Where(p => p.CpnyID == branchID.Trim()).FirstOrDefault();
                                if (checkBranch == null)
                                {
                                    errorBranch = errorBranch + branchID + ",";
                                    errorbranchID += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                            }

                            //if (batchDr == "")
                            //{
                            //    errorbatchDr += (i + 1).ToString() + ",";
                            //    flagCheck = true;
                            //}

                            //if (comment == "")
                            //{
                            //    errorcomment += (i + 1).ToString() + ",";
                            //    flagCheck = true;
                            //}
                            var keycheckUOM = true;
                            var objInvtID = lstUOM.Where(p => p.InvtID == invtID.Trim()).FirstOrDefault();
                            if (objInvtID == null)
                            {
                                errorinvtID += (i + 1).ToString() + ",";
                                keycheckUOM = false;
                                flagCheck = true;
                            }
                            
                            if (flagCheck == false)
                            {
                                string periodID = "";
                                if (RcptDate.Month.ToString().Length == 1)
                                {
                                    periodID = RcptDate.Year.ToString() + "0" + RcptDate.Month.ToString();
                                }
                                else
                                {
                                    periodID = RcptDate.Year.ToString() + RcptDate.Month.ToString();
                                }
                                var objmax = _lstIN_MaxTranfers.FirstOrDefault(p=>p.PeriodID==periodID && p.InvtID==invtID &&p.BranchID==branchID);
                                if (objmax == null)
                                {
                                    objmax = new IN_MaxTranfers();
                                    objmax.ResetET();
                                    objmax.BranchID = branchID;
                                    objmax.PeriodID = periodID;
                                    objmax.InvtID = invtID;
                                    objmax.QtyMax = qtyMaximum;
                                    _lstIN_MaxTranfers.Add(objmax);
                                }
                                else
                                {
                                    for (var j = 0; i < _lstIN_MaxTranfers.Count; i++)
                                    {
                                        if (_lstIN_MaxTranfers[j].BranchID == branchID && _lstIN_MaxTranfers[j].PeriodID == periodID && _lstIN_MaxTranfers[j].InvtID == invtID)
                                        {
                                            _lstIN_MaxTranfers[j].QtyMax = qtyMaximum;
                                        }
                                    }
                                }
                                                     
                                var tam = lstHeader.Where(p => p.BranchID == branchID && p.RcptDate.ToDateShort() == RcptDate.ToDateShort() && p.ToDate.ToDateShort() == toDate.ToDateShort() && p.FromDate.ToDateShort() == fromDate.ToDateShort()).FirstOrDefault();
                                if (tam == null)
                                {
                                    tam = new IN10300_pdHeader_Result();
                                    tam.ResetET();
                                    tam.BranchID = branchID;
                                    tam.RcptDate = RcptDate.ToDateShort();
                                    tam.ToDate = toDate.ToDateShort();
                                    tam.FromDate = fromDate.ToDateShort();
                                    tam.RefBatNbr = refBatNbr;
                                    tam.LineRef = i;
                                    tam.LstDiscSep = listDiscSeq;
                                    tam.Comment = comment;
                                    tam.Descr = batchDr;
                                    lstHeader.Add(tam);
                                }
                                else
                                {
                                    if (tam.Descr == "" && batchDr != "")
                                    {
                                        for (int j = 0; j < lstHeader.Count; j++)
                                        {
                                            if (lstHeader[j].RcptDate == RcptDate.ToDateShort() && lstHeader[j].BranchID == branchID && lstHeader[j].ToDate.ToDateShort() == toDate.ToDateShort() && lstHeader[j].FromDate.ToDateShort() == fromDate.ToDateShort())
                                            {
                                                lstHeader[j].Descr = batchDr;
                                            }
                                        }
                                    }
                                    if (tam.Comment == "" && comment != "")
                                    {
                                        for (int j = 0; j < lstHeader.Count; j++)
                                        {
                                            if (lstHeader[j].RcptDate == RcptDate.ToDateShort() && lstHeader[j].BranchID == branchID && lstHeader[j].ToDate.ToDateShort() == toDate.ToDateShort() && lstHeader[j].FromDate.ToDateShort() == fromDate.ToDateShort())
                                            {
                                                lstHeader[j].Comment = comment;
                                            }
                                        }
                                    }
                                    if (tam.LstDiscSep == "" && listDiscSeq != "")
                                    {
                                        for (int j = 0; j < lstHeader.Count; j++)
                                        {
                                            if (lstHeader[j].RcptDate == RcptDate.ToDateShort() && lstHeader[j].BranchID == branchID && lstHeader[j].ToDate.ToDateShort() == toDate.ToDateShort() && lstHeader[j].FromDate.ToDateShort() == fromDate.ToDateShort())
                                            {
                                                lstHeader[j].LstDiscSep = listDiscSeq;
                                            }
                                        }
                                    }
                                    if (tam.RefBatNbr == "" && refBatNbr != "")
                                    {
                                        for (int j = 0; j < lstHeader.Count; j++)
                                        {
                                            if (lstHeader[j].RcptDate == RcptDate.ToDateShort() && lstHeader[j].BranchID == branchID && lstHeader[j].ToDate.ToDateShort() == toDate.ToDateShort() && lstHeader[j].FromDate.ToDateShort() == fromDate.ToDateShort())
                                            {
                                                lstHeader[j].RefBatNbr = refBatNbr;
                                            }
                                        }
                                    } 
                                }
                                var objtem = lstDeatail2.FirstOrDefault(p => p.InvtID == invtID && p.BranchID == branchID);
                                if (objtem != null)
                                {
                                    error += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }
                                
                                var tam2 = lstDeatail.Where(p => p.BranchID == branchID && p.LineRef == tam.LineRef && p.InvtID == invtID).FirstOrDefault();
                                if (tam2 == null)
                                {
                                    tam2 = new IN10300_pdDeatail_Result();
                                    tam2.ResetET();
                                    tam2.BranchID = branchID;
                                    tam2.LineRef = tam.LineRef;
                                    tam2.Qty = qtyTU;
                                    tam2.QtyMax = qtyMaximum;
                                    tam2.InvtID = invtID;
                                    lstDeatail.Add(tam2);
                                    lstDeatail2.Add(tam2);
                                }
                                else
                                {
                                    errorline += (i + 1).ToString() + ",";
                                    flagCheck = true;
                                }



                                //var objtem = lstDeatail.Where(p=>p.BranchID== branchID && p.InvtID==invtID).ToList();
                                //if (objtem != null)
                                //{
                                //    error += (i + 1).ToString() + ",";
                                //    flagCheck = true;
                                //}
                            }

                        }

                        if (lstHeader != null)
                        {
                            foreach (var itemobj in lstHeader)
                            {
                                if (itemobj.Descr == "")
                                {
                                    errorbatchDr = errorbatchDr + (itemobj.LineRef + 1) + ",";
                                }
                                if (itemobj.Comment == "")
                                {
                                    errorcomment = errorcomment + (itemobj.LineRef + 1) + ",";
                                }
                                if (itemobj.LstDiscSep.Length > 1000)
                                {
                                    errorlistDiscSeq = errorlistDiscSeq + (itemobj.LineRef + 1) + ",";
                                }
                            }                            
                        }                        
                        message += errorlistDiscSeq == "" ? "" : string.Format(Message.GetString("2018003311", null), Util.GetLang("IN10300ListDiscSeq"), errorlistDiscSeq);
                        message = errorBranch == "" ? "" : string.Format(Message.GetString("2018003314", null), Util.GetLang("IN10300BranchID"), errorBranch,errorbranchID,Current.UserName, null);
                        message += errorbatchDr == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("IN10300BatchDr"), errorbatchDr);
                        message += errorcomment == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("IN10300Comment"), errorcomment);
                        message += errorRcptDate == "" ? "" : string.Format(Message.GetString("2018032912", null), Util.GetLang("IN10300RcptDate"), errorRcptDate);
                        message += errorfromDate == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("IN10300FromDate"), errorfromDate);
                        message += errortoDate == "" ? "" : string.Format(Message.GetString("2018003313", null), Util.GetLang("IN10300ToDate"), errortoDate);
                        message += errorinvtID == "" ? "" : string.Format(Message.GetString("2018032911", null), Util.GetLang("IN10300InvtID"), errorinvtID);                       
                        message += errorqtyMaximum == "" ? "" : string.Format(Message.GetString("2018032914", null), Util.GetLang("IN10300InvtID"), errorqtyMaximum);
                        message += errorqtyTU == "" ? "" : string.Format(Message.GetString("2018032914", null), Util.GetLang("IN10300QtyTU"), errorqtyTU);
                        message += errorqtyTU == "" ? "" : string.Format(Message.GetString("2018032914", null), Util.GetLang("IN10300QtyTU"), errorqtyTU);
                        message += error == "" ? "" : string.Format(Message.GetString("2018003312", null), error, null);
                        if (message == "" || message == string.Empty)
                        {

                            try
                            {
                                string cpny = string.Empty;
                                foreach (var item in lstHeader)
                                {
                                    cpny = cpny + item.BranchID + ",";
                                }
                                var lstGetcheckSite = _app.IN10300_pdGetDataSite(cpny, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                                if (lstGetcheckSite != null)
                                {
                                    var lsterror = "";
                                    foreach (var cur in lstGetcheckSite)
                                    {
                                        if (cur.SiteID == null || cur.SiteID == "" || cur.ToSiteID == null || cur.ToSiteID == "")
                                        {
                                            lsterror = lsterror + cur.BranchID + ",";
                                        }
                                    }
                                    if (lsterror != "")
                                    {
                                        throw new MessageException(MessageType.Message, "2018032816", "", new string[] { lsterror });
                                    }
                                }
                                foreach (var item in lstHeader)
                                {
                                   
                                    var itemsite = lstGetcheckSite.Where(p => p.BranchID == item.BranchID).FirstOrDefault();
                                    var lstItem = lstDeatail.Where(p => p.LineRef == item.LineRef && p.BranchID == item.BranchID).ToList();
                                    if (lstItem.Count > 0)
                                    {
                                        var lstinvtid = "";
                                        var lstinvtidqty = "";
                                        foreach (var cur in lstItem)
                                        {
                                            lstinvtid = lstinvtid + cur.InvtID + "@#@#" + cur.QtyMax + "@#@#" + cur.Qty + "@#@#" + cur.UOM + ",";
                                            lstinvtidqty = lstinvtidqty + cur.InvtID + "@#@#" + cur.Qty+",";
                                        }
                                        string messageerorr = "";
                                        var checkqty = _app.IN10300_pdCheckQty(lstinvtidqty, itemsite.SiteID, itemsite.ToSiteID, itemsite.BranchID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                                        if (checkqty != null)
                                        {
                                            
                                            messageerorr += string.Format(Message.GetString("2018033017", null), itemsite.SiteID, itemsite.BranchID, checkqty.InvtID);
                                            throw new MessageException(MessageType.Message, "20410", "", new string[] { messageerorr });
                                        }

                                        string periodID = "";
                                        if (item.RcptDate.Month.ToString().Length == 1)
                                        {
                                            periodID = item.RcptDate.Year.ToString() + "0" + item.RcptDate.Month.ToString();
                                        }
                                        else
                                        {
                                            periodID = item.RcptDate.Year.ToString() + item.RcptDate.Month.ToString();
                                        }
                                        var check = _app.IN10300_pdGetcheckQtyInvt(lstinvtid, itemsite.SiteID, itemsite.ToSiteID, itemsite.BranchID, periodID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                                        if (check != null)
                                        {
                                            messageerorr += string.Format(Message.GetString("2018033018", null), check.InvtID,itemsite.BranchID , itemsite.ToSiteID);
                                            throw new MessageException(MessageType.Message, "20410", "", new string[] { messageerorr });
                                        }
                                        var tam = lstItem.Where(p => p.BranchID == item.BranchID && p.LineRef == item.LineRef && p.Qty > 0).ToList();

                                        if (tam.Count > 0)
                                        {
                                            string lstinvtid1 = string.Empty;
                                            foreach (var cur in tam)
                                            {
                                                lstinvtid1 = lstinvtid1 + cur.InvtID + "@#@#" + cur.QtyMax + "@#@#" + cur.Qty + "@#@#" + cur.UOM + ",";
                                            }
                                            _app = Util.CreateObjectContext<IN10300Entities>(false);
                                            Save_BatchImport(itemsite, item, lstinvtid1);
                                            _app.SaveChanges();

                                            DataAccess dal = Util.Dal();
                                            INProcess.IN inventory = new INProcess.IN(Current.UserName, _screenNbr, dal);
                                            try
                                            {
                                                dal.BeginTrans(IsolationLevel.ReadCommitted);
                                                inventory.IN10300_Release(item.BranchID, _batNbr);
                                                inventory = null;
                                                dal.CommitTrans();
                                            }
                                            catch (Exception)
                                            {
                                                var lstIN_Transfer = _app.IN_Transfer.Where(p => p.BranchID == item.BranchID && p.BatNbr == _batNbr).ToList();
                                                var lstIN_Trans = _app.IN_Trans.Where(p => p.BranchID == item.BranchID && p.BatNbr == _batNbr).ToList();
                                                var lstIN_LotTrans = _app.IN_LotTrans.Where(p => p.BranchID == item.BranchID && p.BatNbr == _batNbr).ToList();
                                                UpdateDataErroReleas(item.BranchID, _batNbr, lstIN_Transfer, lstIN_Trans, lstIN_LotTrans, true);
                                                _app.SaveChanges();
                                                throw;
                                            }
                                            finally
                                            {
                                                dal = null;
                                                inventory = null;
                                            } 
                                        }
                                        
                                    }
                                                                       
                                }                             

                                foreach (var obj in _lstIN_MaxTranfers)
                                {
                                    var objIN_MaxTranfers = _app.IN_MaxTranfers.FirstOrDefault(p => p.BranchID == obj.BranchID && p.PeriodID == obj.PeriodID && p.InvtID == obj.InvtID);
                                    if (objIN_MaxTranfers != null)
                                    {
                                        Update_MaxTranfers(objIN_MaxTranfers, obj, false);
                                    }
                                    else
                                    {
                                        objIN_MaxTranfers = new IN_MaxTranfers();
                                        objIN_MaxTranfers.ResetET();
                                        Update_MaxTranfers(objIN_MaxTranfers, obj, true);
                                        _app.IN_MaxTranfers.AddObject(objIN_MaxTranfers);
                                    }
                                }                                
                                _app.SaveChanges();
                            }
                            catch
                            {
                                throw;
                            }
                        }
                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                    }

                }

            }
            catch (Exception ex)
            {
                DataAccess dal = Util.Dal();
                try
                {                      
                    foreach (Batch objBatch in _lstBatch)
                    {
                        INProcess.IN inventory = new INProcess.IN(Current.UserName, _screenNbr, dal);
                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        var objBatchdb = _app.Batches.Where(p => p.BranchID == objBatch.BranchID && p.BatNbr == objBatch.BatNbr && p.Status == "C").FirstOrDefault();
                        if (objBatchdb != null)
                        {
                            inventory.Issue_Cancel(objBatchdb.BranchID, objBatchdb.BatNbr, string.Empty, true);
                            inventory = null;
                            dal.CommitTrans();
                        }
                        var objBatchStatusH = _app.Batches.Where(p => p.BranchID == objBatch.BranchID && p.BatNbr == objBatch.BatNbr && p.Status == "H").FirstOrDefault();
                        if (objBatchStatusH != null)
                        {
                            var lstIN_Transfer = _app.IN_Transfer.Where(p => p.BranchID == objBatchStatusH.BranchID && p.BatNbr == objBatchStatusH.BatNbr).ToList();
                            var lstIN_Trans = _app.IN_Trans.Where(p => p.BranchID == objBatchStatusH.BranchID && p.BatNbr == objBatchStatusH.BatNbr).ToList();
                            var lstIN_LotTrans = _app.IN_LotTrans.Where(p => p.BranchID == objBatchStatusH.BranchID && p.BatNbr == objBatchStatusH.BatNbr).ToList();
                            UpdateDataErroReleas(objBatchStatusH.BranchID, objBatchStatusH.BatNbr, lstIN_Transfer, lstIN_Trans, lstIN_LotTrans, true);
                        }
                        _app.SaveChanges();
                    }
                    
                    
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                }
                

                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
            return _logMessage;
        }
        
        private void Save_BatchImport(IN10300_pdGetDataSite_Result itemsite, IN10300_pdHeader_Result item, string lstinvtid)
        {
            Batch batch;            
            _lstItemTrans = _app.IN10300_pdGetDataItemTrans(itemsite.SiteID, item.BranchID, lstinvtid, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            _batNbr = _app.INNumbering(item.BranchID, "BatNbr").FirstOrDefault();
            batch = new Batch();
            Update_BatchImport(batch, itemsite, item, true);
            _lstBatch.Add(batch);
            _app.Batches.AddObject(batch);           
            Save_TransferImport(batch, item, itemsite);
        }


        private void Update_MaxTranfers(IN_MaxTranfers t, IN_MaxTranfers s, bool isNew)
        {
            if (isNew)
            {
                t.InvtID = s.InvtID;
                t.PeriodID = s.PeriodID;
                t.BranchID = s.BranchID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.QtyMax = s.QtyMax;
            t.Lupd_DateTime = DateTime.Now;
            t.Lupd_Prog = _screenNbr;
            t.Lupd_User = _userName;
        }

        private void Save_TransferImport(Batch batch, IN10300_pdHeader_Result itemHeader, IN10300_pdGetDataSite_Result itemsite)
        {
            var transfer = new IN_Transfer();
            transfer.ResetET();
            transfer.TrnsfrDocNbr = _app.INNumbering(itemHeader.BranchID, "TrnsNbr").FirstOrDefault();
            transfer.RefNbr = _app.INNumbering(itemHeader.BranchID, "RefNbr").FirstOrDefault();
            Update_TransferImport(transfer, itemHeader, itemsite, true);
            _app.IN_Transfer.AddObject(transfer);
            _lstIN_Transfer.Add(transfer);
            Save_TransImport(transfer, itemsite, itemHeader);
        }

        private void Save_TransImport(IN_Transfer transfer, IN10300_pdGetDataSite_Result objTrans, IN10300_pdHeader_Result itemHeader)
        {
            _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == objTrans.BranchID);
            if (_objIN == null)
            {
                _objIN = new IN_Setup();
            }
            var obj = _app.IN10300_pdGetLineRef(transfer.BatNbr, transfer.BranchID, transfer.RefNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (obj != null && obj != "")
            {
                _lineRefnumber = Convert.ToInt32(obj);
            }
            else
            {
                _lineRefnumber = 0;
            }

            foreach (var trans in _lstItemTrans)
            {
                _lineRefnumber = _lineRefnumber + 1;
                var transDB = _app.IN_Trans.FirstOrDefault(p =>
                                p.BatNbr == transfer.BatNbr && p.RefNbr == transfer.RefNbr &&
                                p.BranchID == transfer.BranchID);

                _LineRef = string.Empty;
                for (var i = 0; i < (5 - (_lineRefnumber.ToString().Length)); i++)
                {
                    _LineRef = _LineRef + '0';
                }
                _LineRef = _LineRef + _lineRefnumber;
                transDB = new IN_Trans();
                transDB.SiteID = objTrans.SiteID;
                transDB.InvtID = trans.InvtID;
                Update_TransImport(transfer, transDB, trans, itemHeader, true);
                _app.IN_Trans.AddObject(transDB);
                _lstIN_Trans.Add(transDB);
                Save_LotImport(transfer, transDB);
            }

        }

        private bool Save_LotImport(IN_Transfer transfer, IN_Trans tran)
        {
            //var lots = _db.IN_LotTrans.Where(p => p.BranchID == transfer.BranchID && p.BatNbr == transfer.BatNbr).ToList();
            //foreach (var item in lots)
            //{
            //    if (item.EntityState == EntityState.Deleted || item.EntityState == EntityState.Detached) continue;
            //    if (!_lstLot.Any(p => p.INTranLineRef == item.INTranLineRef && p.LotSerNbr == item.LotSerNbr))
            //    {
            //        var oldQty = item.UnitMultDiv == "D" ? item.Qty / item.CnvFact : item.Qty * item.CnvFact;

            //        UpdateAllocLot(item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);

            //        _app.IN_LotTrans.DeleteObject(item);
            //    }
            //}

            //var lstLotTmp = _lstLot.Where(p => p.INTranLineRef == tran.LineRef).ToList();
            //foreach (var lotCur in lstLotTmp)
            //{
            //    var lot = _app.IN_LotTrans.FirstOrDefault(p => p.BranchID == transfer.BranchID && p.BatNbr == transfer.BatNbr && p.INTranLineRef == lotCur.INTranLineRef && p.LotSerNbr == lotCur.LotSerNbr);
            //    if (lot == null || lot.EntityState == EntityState.Deleted || lot.EntityState == EntityState.Detached)
            //    {
            //        lot = new IN_LotTrans();
            //        Update_Lot(lot, lotCur, transfer, tran, true);
            //        _app.IN_LotTrans.AddObject(lot);
            //    }
            //    else
            //    {
            //        Update_Lot(lot, lotCur, transfer, tran, false);
            //    }
            //}
            return true;
        }

        private void Update_BatchImport(Batch t, IN10300_pdGetDataSite_Result objTrans, IN10300_pdHeader_Result inputHeader, bool isNew)
        {
            if (isNew)
            {
                t.ResetET();
                t.BranchID = inputHeader.BranchID;
                t.BatNbr = _batNbr;
                t.Module = "IN";
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
                t.Crtd_DateTime = DateTime.Now;
            }
            Double tong = _lstItemTrans.Sum(x => x.Qty * x.Price);
            t.JrnlType = "IN";
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
            t.DateEnt = inputHeader.RcptDate.ToDateShort();
            t.Descr = inputHeader.Descr;
            t.EditScrnNbr = t.EditScrnNbr.PassNull() == string.Empty ? _screenNbr : t.EditScrnNbr;
            t.NoteID = 0;
            t.ReasonCD = "KM";
            t.TotAmt = tong;
            t.Rlsed = 0;
            t.Status = "H";
            t.DiscID = "";
            t.DiscSeq = "";
        }


        private void Update_TransferImport(IN_Transfer t, IN10300_pdHeader_Result itemHeader, IN10300_pdGetDataSite_Result itemsite, bool isNew)
        {
            if (isNew)
            {
                t.BranchID = itemHeader.BranchID;
                t.BatNbr = _batNbr;
                t.TrnsfrDocNbr = t.TrnsfrDocNbr;
                t.RefNbr = t.RefNbr;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
                t.Crtd_DateTime = DateTime.Now;
            }
            t.RcptBatNbr = "";
            t.RefBatNbr = itemHeader.RefBatNbr;
            t.ListDiscSeq = itemHeader.LstDiscSep;
            t.NoteID = 0;
            t.AdvanceType = string.Empty;
            t.Comment = itemHeader.Comment.PassNull();
            t.ReasonCD = "KM";
            t.RcptDate = itemHeader.RcptDate.ToDateShort();
            t.SiteID = itemsite.SiteID.PassNull();
            t.Status = "H";
            t.ToSiteID = itemsite.ToSiteID.PassNull();
            t.TranDate = itemHeader.RcptDate.ToDateShort();
            t.TransferType = "1";
            t.ShipViaID = itemsite.ShipViaID.PassNull();
            t.Source = "IN";
            t.ToCpnyID = itemHeader.BranchID;
            t.ExpectedDate = itemHeader.RcptDate.ToDateShort();
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
            t.FromDate = itemHeader.FromDate.ToDateShort();
            t.ToDate = itemHeader.ToDate.ToDateShort();
        }

        private void Update_TransImport(IN_Transfer transfer, IN_Trans t, IN10300_pdGetDataItemTrans_Result s, IN10300_pdHeader_Result itemHeader,  bool isNew)
        {
            double oldQty, newQty;


            if (!isNew)
                oldQty = t.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
            else
                oldQty = 0;

            newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

            UpdateINAlloc(t.InvtID, t.SiteID, oldQty, 0);

            UpdateINAlloc(s.InvtID, t.SiteID, 0, newQty);


            if (isNew)
            {
                t.ResetET();
                t.LineRef = _LineRef;
                t.BranchID = transfer.BranchID;
                t.BatNbr = transfer.BatNbr;
                t.RefNbr = transfer.RefNbr;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = Current.UserName;
            }

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
            t.RptExpDate = itemHeader.RcptDate.ToDateShort();
            t.CnvFact = s.CnvFact;
            t.ExtCost = s.ExtCost;
            t.InvtID = s.InvtID;
            t.InvtMult = -1;
            t.JrnlType = s.JrnlType;
            t.ObjID = s.ObjID;
            t.ReasonCD = transfer.ReasonCD;
            t.Qty = Math.Round(s.Qty, 0);
            t.Rlsed = 0;
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

        private void UpdateDataErroReleas(string branchID, string batNbr, List<IN_Transfer> lstIN_Transfer, List<IN_Trans> lstIN_Trans, List<IN_LotTrans> lstIN_LotTrans, bool isNew)
        {
            try
            {
                DataAccess dal = Util.Dal();
                dal.RollbackTrans();

                var batch = _app.Batches.FirstOrDefault(p => p.BranchID == branchID && p.BatNbr == batNbr);
                if (batch != null)
                {
                    batch.Status = "V";
                    batch.Rlsed = -1;
                }
                
                foreach (IN_Transfer objIN_Transfer in lstIN_Transfer)
                {
                    var objTransfer = _app.IN_Transfer.FirstOrDefault(p => p.BranchID == objIN_Transfer.BranchID && p.BatNbr == objIN_Transfer.BatNbr && p.TrnsfrDocNbr == objIN_Transfer.TrnsfrDocNbr);
                    if (objTransfer != null)
                    {
                        objTransfer.Status = "R";
                    }
                }

                foreach (IN_Trans objIN_Trans in lstIN_Trans)
                {
                    double oldQty = 0;
                    oldQty = objIN_Trans.UnitMultDiv == "D" ? objIN_Trans.Qty / objIN_Trans.CnvFact : objIN_Trans.Qty * objIN_Trans.CnvFact;
                    objIN_Trans.Rlsed = -1;
                    UpdateINAlloc(objIN_Trans.InvtID, objIN_Trans.SiteID, oldQty, 0);
                }

                foreach (var lot in lstIN_LotTrans)
                {
                    double oldQty = 0;

                    oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;

                    UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr,lot.WhseLoc, oldQty, 0, 0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
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

using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using System.Text;
using HQ.eSkySys;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using System.Text.RegularExpressions;
namespace IN11700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN11700Controller : Controller
    {
        private string _screenNbr = "IN11700";
        private string _userName = Current.UserName;
        private string _whseLoc = string.Empty;
        private string _whseLocTo = string.Empty;
        private string _site = string.Empty;
        private string _siteTo = string.Empty;
        private int _decQty;
        private FormCollection _form;
        private JsonResult _logMessage;
        private IN11700_pcBatch_Result _objBatch;
        private List<IN11700_pgReceiptLoad_Result> _lstTrans;
        private List<IN11700_pgIN_LotTrans_Result> _lstLot;
        private List<IN11700_pgIN_LotTransDPBB_Result> _lstLotDPBB;
        private List<IN11700_pgComponent_Result> _lstComponent;
        IN11700Entities _db = Util.CreateObjectContext<IN11700Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        
        private string _handle = "";
        private IN_Setup _objIN;
        public ActionResult Index()
        {
            int showWhseLoc = 0;
            var objconfig = _db.IN11700_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objconfig != null)
            {
                showWhseLoc = objconfig.ShowWhseLoc.Value;
            }
            ViewBag.showWhseLoc = showWhseLoc;
            Util.InitRight(_screenNbr);
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
            var lstBatch = _db.IN11700_pcBatch(Current.UserName, branchID, query, start + 1, start + 20).ToList();
            var paging = new Paging<IN11700_pcBatch_Result>(lstBatch, lstBatch.Count > 0 ? lstBatch[0].TotalRecords.Value : 0);
            return this.Store(paging.Data, paging.TotalRecords);
        }
        public ActionResult GetKit(string branchID, string BatNbr)
        {
            return this.Store(_db.IN11700_pgReceiptLoad(BatNbr, branchID, "%", "%",Current.CpnyID, Current.UserName, Current.LangID).ToList());
        }

        public ActionResult GetComponent(string KitID, string branchID, string lineRef, string refNbr, string batNbr)
        {
            return this.Store(_db.IN11700_pgComponent(Current.CpnyID, Current.UserName, Current.LangID, KitID, branchID, lineRef, refNbr,batNbr).ToList());
        }
        public ActionResult GetLot(string component, string siteID, string batNbr, string branchID, string whseLoc, int showWhseLoc)
        {
            List<IN11700_pdGetLot_Result> lstLot = new List<IN11700_pdGetLot_Result>();
            //List<IN_ItemLot> lstLotDB = _db.IN_ItemLot.Where(p => p.SiteID == siteID && p.InvtID == invtID && p.QtyAvail > 0).ToList();
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() == ""))
            {
                List<IN11700_pdGetLot_Result> lstLotDB = _db.IN11700_pdGetLot(siteID, component, Current.UserName, Current.CpnyID, Current.LangID).Where(p => p.WhseLoc == whseLoc).ToList();
                foreach (var item in lstLotDB)
                {
                    lstLot.Add(item);
                }
                List<IN_LotTrans> lstLotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == component && p.SiteID == siteID && p.WhseLoc == whseLoc).ToList();
                foreach (var item in lstLotTrans)
                {
                    var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                    if (lot == null)
                    {
                        var lotDB = _db.IN11700_pdGetLot(siteID, component, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr && p.WhseLoc == whseLoc);
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
                List<IN11700_pdGetLot_Result> lstLotDB = _db.IN11700_pdGetLot(siteID, component, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                foreach (var item in lstLotDB)
                {
                    lstLot.Add(item);
                }
                List<IN_LotTrans> lstLotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == component && p.SiteID == siteID).ToList();
                foreach (var item in lstLotTrans)
                {
                    var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                    if (lot == null)
                    {
                        var lotDB = _db.IN11700_pdGetLot(siteID, component, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
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
        }
        public ActionResult GetLotDPBB(string invtID, string siteID, string batNbr, string branchID, string whseLoc, int showWhseLoc, int cnvFact)
        {
            List<IN11700_pdGetLotDPBB_Result> lstLot = new List<IN11700_pdGetLotDPBB_Result>();
            List<IN11700_pdGetLotDPBB_Result> lstLotDB = new List<IN11700_pdGetLotDPBB_Result>();
            List<IN_LotTrans> lstLotTrans = new List<IN_LotTrans>();
            bool key = false;
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                key = true;
            }

            if (key)
            {
                lstLotDB = _db.IN11700_pdGetLotDPBB(1, siteID, invtID, whseLoc, "", Current.UserName, Current.CpnyID, Current.LangID).ToList();
                lstLotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc).ToList();
            }
            else
            {
                lstLotDB = _db.IN11700_pdGetLotDPBB(0, siteID, invtID, whseLoc, "", Current.UserName, Current.CpnyID, Current.LangID).ToList();
                lstLotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID).ToList();
            }
            foreach (var item in lstLotDB)
            {
                item.QtyAvail = Math.Floor(item.QtyAvail / cnvFact);
                lstLot.Add(item);
            }
            foreach (var item in lstLotTrans)
            {
                var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                if (lot == null)
                {
                    if (key)
                    {
                        var lotDB = _db.IN11700_pdGetLotDPBB(2, siteID, invtID, whseLoc, item.LotSerNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();//.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == item.LotSerNbr && p.WhseLoc == whseLoc);
                        lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        lstLot.Add(lotDB);
                    }
                    else
                    {
                        var lotDB = _db.IN11700_pdGetLotDPBB(3, siteID, invtID, whseLoc, item.LotSerNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                        lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        lstLot.Add(lotDB);
                    }
                }
                else
                {
                    lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                }
            }
            lstLot = lstLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();
            return this.Store(lstLot, lstLot.Count);
        }
        public ActionResult GetUnit(string invtID)
        {
            IN_Inventory invt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (invt == null) invt = new IN_Inventory();
            List<IN11700_pcUnit_Result> lstUnit = _db.IN11700_pcUnit(invt.ClassID, invt.InvtID).ToList();
            return this.Store(lstUnit, lstUnit.Count);
        }
        public ActionResult GetLotTrans(string branchID, string batNbr, string whseLoc)
        {
            List<IN11700_pgIN_LotTrans_Result> lstLotTrans = _db.IN11700_pgIN_LotTrans(batNbr, branchID, whseLoc, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetLotTransDPBB(string branchID, string batNbr, string whseLoc)
        {
            List<IN11700_pgIN_LotTransDPBB_Result> lstLotTrans = _db.IN11700_pgIN_LotTransDPBB(batNbr, branchID, whseLoc, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetItemLotDPBB(string invtID, string siteID, string lotSerNbr, string branchID, string batNbr, string whseLoc, int showWhseLoc)
        {
            bool key = false;
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                key = true;
            }
            var lot = _db.IN11700_pdGetItemLotDPBB(key, siteID, invtID, whseLoc, lotSerNbr, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            List<IN_LotTrans> lotTrans = new List<IN_LotTrans>();
            if (lot == null) lot = new IN11700_pdGetItemLotDPBB_Result()
            {
                InvtID = invtID,
                SiteID = siteID,
                LotSerNbr = lotSerNbr
            };
            if (key)
            {
                lotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc).ToList();
            }
            else
            {
                lotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr).ToList();
            }


            foreach (var item in lotTrans)
            {
                lot.QtyAvail += (item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact);
            }

            List<IN11700_pdGetItemLotDPBB_Result> lstLot = new List<IN11700_pdGetItemLotDPBB_Result>() { lot };
            return this.Store(lstLot, lstLot.Count);
        }
        public ActionResult GetItemLot(string invtID, string siteID, string lotSerNbr, string branchID, string batNbr, string whseLoc, int showWhseLoc)
        {
            var lot = new IN11700_pdGetLot_Result();
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                lot = _db.IN11700_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc);

                if (lot == null) lot = new IN11700_pdGetLot_Result()
                {
                    InvtID = invtID,
                    SiteID = siteID,
                    LotSerNbr = lotSerNbr
                };
                var lotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc).ToList();
                foreach (var item in lotTrans)
                {
                    lot.QtyAvail += (item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact);
                }
            }
            else
            {
                lot = _db.IN11700_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr);

                if (lot == null) lot = new IN11700_pdGetLot_Result()
                {
                    InvtID = invtID,
                    SiteID = siteID,
                    LotSerNbr = lotSerNbr
                };
                var lotTrans = _db.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr).ToList();
                foreach (var item in lotTrans)
                {
                    lot.QtyAvail += (item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact);
                }
            }

            List<IN11700_pdGetLot_Result> lstLot = new List<IN11700_pdGetLot_Result>() { lot };
            return this.Store(lstLot, lstLot.Count);
        }
        public ActionResult GetItemSite(string invtID, string siteID, string whseLoc, int showWhseLoc)
        {
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                var objSite = _db.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc);
                return this.Store(objSite);
            }
            else
            {
                var objSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
                return this.Store(objSite);
            }
        }
        public ActionResult GetUnitConversion(string cpnyID)
        {
            var lstUnit = _db.IN11700_pcUnitConversion(cpnyID).ToList();
            return this.Store(lstUnit);
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
                return Util.CreateMessage(MessageProcess.Save, new { batNbr = _objBatch.BatNbr });
            }
            catch(Exception ex)
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
            _whseLoc = data["WhseLoc"];
            _whseLocTo = data["WhseLocTo"];
            _site = data["SiteID"];
            _siteTo = data["ToSiteID"];
            
            if (_lstTrans == null)
            {
                var transHandler = new StoreDataHandler(data["lstTrans"]);
                _lstTrans = transHandler.ObjectData<IN11700_pgReceiptLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }
            if (_lstLot == null)
            {
                var lotHandler = new StoreDataHandler(data["lstLot"]);
                _lstLot = lotHandler.ObjectData<IN11700_pgIN_LotTrans_Result>().Where(p => Util.PassNull(p.INTranLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            }
            if (_lstLotDPBB == null)
            {
                var lotHandler = new StoreDataHandler(data["lstLotDPBB"]);
                _lstLotDPBB = lotHandler.ObjectData<IN11700_pgIN_LotTransDPBB_Result>().Where(p => Util.PassNull(p.INTranLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            }
            if(_lstComponent==null)
            {
                var comHandler = new StoreDataHandler(data["lstComponent"]);
                _lstComponent = comHandler.ObjectData<IN11700_pgComponent_Result>().ToList();
            }
            _objBatch = data.ConvertToObject<IN11700_pcBatch_Result>();
            _decQty = _sys.SYS_Configurations.FirstOrDefault(p => p.Code.ToLower() == "DecPlQty".ToLower()).IntVal;
            _handle = data["cboHandle"].PassNull();
            _objBatch.Status = _objBatch.Status.PassNull() == string.Empty ? "H" : _objBatch.Status;

            if (_db.IN11700_ppCheckCloseDate(_objBatch.BranchID, _objBatch.DateEnt.ToDateShort(), "IN11700", Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault() == "0")
                throw new MessageException(MessageType.Message, "301");

            Batch batch = _db.Batches.FirstOrDefault(p => p.BatNbr == _objBatch.BatNbr && p.BranchID == _objBatch.BranchID);
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

                //CheckData();

                Save_Batch(batch);

            }
            _db.SaveChanges();
            var access = Session[_screenNbr] as AccessRight;
            if (_handle != "N")
            {
                DataAccess dal = Util.Dal();
                INProcess.IN inventory = new INProcess.IN(_userName, _screenNbr, dal);
                try
                {
                    if (_handle == "R")
                    {
                        if (!access.Release)
                            throw new MessageException(MessageType.Message, "728");
                        dal.BeginTrans(IsolationLevel.ReadCommitted);

                        inventory.IN10400_Release(_objBatch.BranchID, _objBatch.BatNbr);

                        dal.CommitTrans();

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _objBatch.BatNbr });
                    }
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
                string InvtID = _lstTrans[i].InvtID;
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

                IN_Inventory objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == InvtID);
                if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack != "N")
                {
                    //var lstLot = _lstLot.Where(p => p.INTranLineRef == _lstTrans[i].LineRef).ToList();
                    //double lotQty = 0;
                    //foreach (var item in lstLot)
                    //{
                    //    if (item.InvtID != _lstTrans[i].InvtID || item.SiteID != _lstTrans[i].SiteID)
                    //    {
                    //        throw new MessageException("2015040501", new[] { _lstTrans[i].InvtID });
                    //    }

                    //    if (item.UnitMultDiv.PassNull() == string.Empty || item.UnitDesc.PassNull() == string.Empty)
                    //    {
                    //        throw new MessageException("2015040503", new[] { _lstTrans[i].InvtID });
                    //    }

                    //    lotQty += Math.Round(item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact, 0);
                    //}
                    //double detQty = Math.Round(_lstTrans[i].UnitMultDiv == "M" ? _lstTrans[i].Qty * _lstTrans[i].CnvFact : _lstTrans[i].Qty / _lstTrans[i].CnvFact, 0);
                    //if (detQty != lotQty)
                    //{
                    //    throw new MessageException("2015040502", new[] { _lstTrans[i].InvtID });
                    //}
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
                _objBatch.RefNbr = batch.RefNbr;
                Update_Batch(batch, false);

            }
            else
            {
                _objBatch.BatNbr = _db.INNumbering(_objBatch.BranchID, "BatNbr").FirstOrDefault();
                _objBatch.RefNbr = _db.INNumbering(_objBatch.BranchID, "RefNbr").FirstOrDefault();
                batch = new Batch();
                Update_Batch(batch, true);
                _db.Batches.AddObject(batch);
            }
            Save_Trans(batch);
        }
        private void Save_Trans(Batch batch)
        {
            _objIN = _db.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
            if (_objIN == null) _objIN = new IN_Setup();
            foreach (var trans in _lstTrans)
            {

                var transDB = _db.IN_Trans.FirstOrDefault(p =>
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
                    if(trans.InvtID.PassNull()!="")
                    {
                        transDB = new IN_Trans();
                        Update_Trans(batch, transDB, trans, true);
                        _db.IN_Trans.AddObject(transDB);
                    }
                }
                Save_LotDPBB(batch, transDB);
            }
            foreach (var trans in _lstComponent)
            {

                var transDB = _db.IN_Trans.FirstOrDefault(p =>
                                p.BatNbr == _objBatch.BatNbr && p.RefNbr == trans.RefNbr &&
                                p.BranchID == _objBatch.BranchID && p.LineRef == trans.LineRef);
                if(_lstTrans.Where(p=>p.InvtID == trans.InvtID).Count()!=0)
                {
                    if (transDB != null)
                    {
                        if (transDB.tstamp.ToHex() != trans.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        Update_TransCom(batch, transDB, trans, false);
                    }
                    else
                    {
                        if (trans.ComponentID.PassNull() != "")
                        {
                            transDB = new IN_Trans();
                            Update_TransCom(batch, transDB, trans, true);
                            _db.IN_Trans.AddObject(transDB);
                        }
                    }
                    Save_Lot(batch, transDB);
                }
            }
            _db.SaveChanges();
        }
        private bool Save_LotDPBB(Batch batch, IN_Trans tran)
        {
            var lots = _db.IN_LotTrans.Where(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr).ToList();
            foreach (var item in lots)
            {
                if (item.EntityState == EntityState.Deleted || item.EntityState == EntityState.Detached) continue;
                if (!_lstLotDPBB.Any(p => p.INTranLineRef == item.INTranLineRef && p.LotSerNbr == item.LotSerNbr))
                {
                    var oldQty = item.UnitMultDiv == "D" ? item.Qty / item.CnvFact : item.Qty * item.CnvFact;
                    if (item.WhseLoc.PassNull() != "")
                    {
                        UpdateAllocLot_WhseLoc(_whseLoc, item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
                    }
                    else
                    {
                        UpdateAllocLot(item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
                    }
                    _db.IN_LotTrans.DeleteObject(item);
                }
            }

            var lstLotTmp = _lstLotDPBB.Where(p => p.INTranLineRef == tran.LineRef).ToList();
            foreach (var lotCur in lstLotTmp)
            {
                var lot = _db.IN_LotTrans.FirstOrDefault(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr && p.INTranLineRef == lotCur.INTranLineRef && p.LotSerNbr == lotCur.LotSerNbr);
                if (lot == null || lot.EntityState == EntityState.Deleted || lot.EntityState == EntityState.Detached)
                {
                    lot = new IN_LotTrans();
                    Update_LotDPBB(lot, lotCur, batch, tran, true);
                    _db.IN_LotTrans.AddObject(lot);
                }
                else
                {
                    Update_LotDPBB(lot, lotCur, batch, tran, false);
                }
            }
            return true;
        }
        private bool Save_Lot(Batch batch, IN_Trans tran)
        {
            var lots = _db.IN_LotTrans.Where(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr).ToList();
            foreach (var item in lots)
            {
                if (item.EntityState == EntityState.Deleted || item.EntityState == EntityState.Detached) continue;
                if (!_lstLot.Any(p => p.INTranLineRef == item.INTranLineRef && p.LotSerNbr == item.LotSerNbr))
                {
                    var oldQty = item.UnitMultDiv == "D" ? item.Qty / item.CnvFact : item.Qty * item.CnvFact;
                    if (item.WhseLoc.PassNull() != "")
                    {
                        UpdateAllocLot_WhseLoc(_whseLoc, item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
                    }
                    else
                    {
                        UpdateAllocLot(item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
                    }
                    _db.IN_LotTrans.DeleteObject(item);
                }
            }

            var lstLotTmp = _lstLot.Where(p => p.INTranLineRef == tran.LineRef).ToList();
            foreach (var lotCur in lstLotTmp)
            {
                var lot = _db.IN_LotTrans.FirstOrDefault(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr && p.INTranLineRef == lotCur.INTranLineRef && p.LotSerNbr == lotCur.LotSerNbr);
                if (lot == null || lot.EntityState == EntityState.Deleted || lot.EntityState == EntityState.Detached)
                {
                    lot = new IN_LotTrans();
                    Update_Lot(lot, lotCur, batch, tran, true);
                    _db.IN_LotTrans.AddObject(lot);
                }
                else
                {
                    Update_Lot(lot, lotCur, batch, tran, false);
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
        private void Update_Trans(Batch batch, IN_Trans t, IN11700_pgReceiptLoad_Result s, bool isNew)
        {
            double oldQty = 0, newQty = 0;
            //if (!isNew)
            //{
            //    if (t.Qty < 0)
            //        oldQty = Math.Abs(t.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact);
            //    else
            //        oldQty = 0;
            //}
            //else
            //    oldQty = 0;


            //if (s.Qty < 0)
            //    newQty = Math.Abs(s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact);
            if (s.TranType == "AJ")
            {
                if (!isNew)
                {
                    if (t.Qty < 0)
                    {
                        oldQty = Math.Abs(t.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact);
                    }
                    else
                    {
                        oldQty = 0;
                    }
                }
                else
                {
                    oldQty = 0;
                }

                if (s.Qty < 0)
                {
                    newQty = Math.Abs(s.UnitMultDiv == "D" ? s.Qty / s.CnvFact.ToDouble() : s.Qty * s.CnvFact.ToDouble());
                }
                UpdateINAlloc(t.InvtID, t.SiteID, oldQty, 0);
                UpdateINAlloc(s.InvtID, s.SiteID, 0, newQty);
                if (_whseLoc.PassNull() != "")
                {
                    UpdateIN_ItemLoc(t.InvtID, t.SiteID, _whseLoc, oldQty, 0);
                    UpdateIN_ItemLoc(s.InvtID, s.SiteID, _whseLoc, 0, newQty);
                }
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
            t.CnvFact = s.CnvFact.ToDouble();
            t.ExtCost = Math.Round(s.TranAmt, 0);
            t.InvtID = s.InvtID;
            t.InvtMult = s.InvtMult;
            t.JrnlType = s.JrnlType;
            t.ObjID = s.ObjID;
            t.Qty = s.Qty;
            t.SiteID = _site;
            t.ToSiteID = _siteTo;
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
            t.WhseLoc = _whseLoc.PassNull();
            t.ToWhseLoc = _whseLocTo.PassNull();
            t.PosmID = s.PosmID;
        }
        private void Update_TransCom(Batch batch, IN_Trans t, IN11700_pgComponent_Result s, bool isNew)
        {
            double oldQty = 0, newQty = 0;
            //if (!isNew)
            //{
            //    if (t.Qty < 0)
            //        oldQty = Math.Abs(t.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact);
            //    else
            //        oldQty = 0;
            //}
            //else
            //    oldQty = 0;


            //if (s.Qty < 0)
            //    newQty = Math.Abs(s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact);
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
            t.CnvFact = s.CnvFact.ToDouble();
            //t.ExtCost = Math.Round(s.TranAmt, 0);
            t.InvtID = s.ComponentID;
            t.InvtMult = s.InvtMult.ToShort();
            t.JrnlType = s.JrnlType;
           // t.ObjID = s.ObjID;
            t.Qty = s.ComponentQty.ToDouble();
            t.SiteID = _site;
            t.ToSiteID = _siteTo;
           // t.ShipperID = s.ShipperID;
            //t.ShipperLineRef = s.ShipperLineRef;
            //t.TranAmt = s.TranAmt;
            //t.TranFee = s.TranFee;
            //t.TranDesc = s.TranDesc;
            t.TranType = s.TranType;
            //t.TranDate = batch.DateEnt;
            //t.UnitCost = s.UnitCost;
            t.UnitDesc = s.Unit;
            //t.UnitMultDiv = s.UnitMultDiv;
            //t.UnitPrice = s.UnitPrice;
            t.SlsperID = _form["SlsperID"].PassNull();
            t.WhseLoc = _whseLoc.PassNull();
            t.ToWhseLoc = _whseLocTo.PassNull();
            //t.PosmID = s.PosmID;
        }
        private bool Update_LotDPBB(IN_LotTrans t, IN11700_pgIN_LotTransDPBB_Result s, Batch batch, IN_Trans tran, bool isNew)
        {

            if (isNew)
            {
                t.ResetET();
                t.BatNbr = batch.BatNbr;
                t.BranchID = batch.BranchID;
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

            if (tran.TranType == "II")
            {
                if (!isNew)
                    oldQty = s.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                else
                    oldQty = 0;

                newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

                if (_whseLoc.PassNull() != "")
                {
                    UpdateAllocLot_WhseLoc(_whseLoc, t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot_WhseLoc(_whseLoc, s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
                    {
                        throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                    }
                }
                else
                {
                    UpdateAllocLot(t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot(s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
                    {
                        throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                    }
                }
            }

            t.UnitDesc = s.UnitDesc;
            t.ExpDate = s.ExpDate;
            t.InvtID = s.InvtID;
            t.InvtMult = tran.InvtMult;
            t.Qty = s.Qty;
            t.WhseLoc = _whseLoc.PassNull();
            t.SiteID = s.SiteID;

            t.MfgrLotSerNbr = s.MfgrLotSerNbr.PassNull();
            t.TranType = tran.TranType;

            t.TranDate = batch.DateEnt;
            t.CnvFact = s.CnvFact;
            t.UnitCost = s.UnitCost;
            t.UnitPrice = s.UnitPrice;

            t.UnitMultDiv = s.UnitMultDiv;

            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.LUpd_DateTime = DateTime.Now;

            return true;
        }
        private bool Update_Lot(IN_LotTrans t, IN11700_pgIN_LotTrans_Result s, Batch batch, IN_Trans tran, bool isNew)
        {

            if (isNew)
            {
                t.ResetET();
                t.BatNbr = batch.BatNbr;
                t.BranchID = batch.BranchID;
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

            if (tran.TranType == "II")
            {
                if (!isNew)
                    oldQty = s.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                else
                    oldQty = 0;

                newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

                if (_whseLoc.PassNull() != "")
                {
                    UpdateAllocLot_WhseLoc(_whseLoc, t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot_WhseLoc(_whseLoc, s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
                    {
                        throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                    }
                }
                else
                {
                    UpdateAllocLot(t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                    if (!UpdateAllocLot(s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
                    {
                        throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                    }
                }
            }

            t.UnitDesc = s.UnitDesc;
            t.ExpDate = s.ExpDate;
            t.InvtID = s.ComponentID;
            t.InvtMult = tran.InvtMult;
            t.Qty = s.Qty;
            t.WhseLoc = _whseLoc.PassNull();
            t.SiteID = _site.PassNull();
            t.ToWhseLoc = _whseLocTo.PassNull();
            t.ToSiteID = _siteTo.PassNull();

            t.MfgrLotSerNbr = s.MfgrLotSerNbr.PassNull();
            t.TranType = tran.TranType;

            t.TranDate = batch.DateEnt;
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
                var objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objSite =_db.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
                    if (objSite == null) objSite = new IN_ItemSite() { SiteID = siteID, InvtID = invtID };
                    if (!_objIN.NegQty && newQty > 0 && objSite.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "608", "", new string[] { invtID, siteID });
                    }
                    objSite.QtyAllocIN = Math.Round(objSite.QtyAllocIN + newQty - oldQty, _decQty);
                    objSite.QtyAvail = Math.Round(objSite.QtyAvail - newQty + oldQty, _decQty);
                    // itemsite.QtyAllocIN = Math.Round(itemsite.QtyAllocIN + newQty - oldQty, 0);
                    // itemsite.QtyAvail = Math.Round(itemsite.QtyAvail - newQty + oldQty, 0);
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool UpdateIN_ItemLoc(string invtID, string siteID, string whseLoc, double oldQty, double newQty)
        {
            try
            {
                var objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objLoc = _db.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc);
                    if (objLoc == null) objLoc = new IN_ItemLoc() { SiteID = siteID, InvtID = invtID };
                    if (!_objIN.NegQty && newQty > 0 && objLoc.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "2018052413", "", new string[] { invtID, siteID, whseLoc });
                    }
                    objLoc.QtyAllocIN = Math.Round(objLoc.QtyAllocIN + newQty - oldQty, _decQty);
                    objLoc.QtyAvail = Math.Round(objLoc.QtyAvail - newQty + oldQty, _decQty);
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool UpdateAllocLot_WhseLoc(string whseLoc, string invtID, string siteID, string lotSerNbr, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                var objItemLot = _db.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc);
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
        private bool UpdateAllocLot(string invtID, string siteID, string lotSerNbr, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                var objItemLot = _db.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr);
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
        private IN_UnitConversion SetUOM(string invtID, string classID, string stkUnit, string fromUnit)
        {
            if (!string.IsNullOrEmpty(fromUnit))
            {
                IN_UnitConversion data = _db.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "3" && p.ClassID == "*" && p.InvtID == invtID && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _db.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "2" && p.ClassID == classID && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _db.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "1" && p.ClassID == "*" && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                throw new MessageException("2525", new[] { invtID });
                return null;
            }
            return null;
        }
        private bool Update_IN_ItemLoc(string whseLoc, string invtID, string siteID, double oldQty, double newQty)
        {
            try
            {
                var objInvt = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objSite = _db.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc);

                    if (objSite == null) objSite = new IN_ItemLoc() { SiteID = siteID, InvtID = invtID, WhseLoc = whseLoc };

                    if (!_objIN.NegQty && newQty > 0 && objSite.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "2018051411", "", new string[] { invtID, siteID, _whseLoc });
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
        [HttpPost]
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                var access = Session["IN11700"] as AccessRight;
                _whseLoc = data["WhseLoc"].PassNull();
                var status = data["cboStatus"].PassNull();
                List<IN_Trans> lstTrans = new List<IN_Trans>();
                List<IN_LotTrans> lstLot = new List<IN_LotTrans>();
                if (!access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objBatch = data.ConvertToObject<IN11700_pcBatch_Result>();

                if (status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }

                _objIN = _db.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
                if (_objIN == null) _objIN = new IN_Setup();

                var batch = _db.Batches.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr);
                if (batch != null) _db.Batches.DeleteObject(batch);

                if (_whseLoc != "")
                {
                    lstTrans = _db.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.WhseLoc == _whseLoc).ToList();
                }
                else
                {
                    lstTrans = _db.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                }

                foreach (var trans in lstTrans)
                {
                    double oldQty = 0;
                    if (trans.TranType == "II")
                    {
                        oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                        UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                        Update_IN_ItemLoc(_whseLoc, trans.InvtID, trans.SiteID, oldQty, 0);
                    }
                    _db.IN_Trans.DeleteObject(trans);
                }
                if (_whseLoc != "")
                {
                    lstLot = _db.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.WhseLoc == _whseLoc).ToList();
                }
                else
                {
                    lstLot = _db.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                }
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;
                    if (lot.TranType == "II")
                    {
                        oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                        if (lot.WhseLoc.PassNull() != "")
                        {
                            UpdateAllocLot_WhseLoc(_whseLoc, lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                        }
                        else
                        {
                            UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                        }
                    }
                    _db.IN_LotTrans.DeleteObject(lot);
                }

                _db.SaveChanges();

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
        public ActionResult DeleteKitID(FormCollection data)
        {
            try
            {
                var access = Session["IN11700"] as AccessRight;
                _whseLoc = data["WhseLoc"].PassNull();
                _objBatch = data.ConvertToObject<IN11700_pcBatch_Result>();

                if (_objBatch.Status != "H")
                {
                    throw new MessageException(MessageType.Message, "2015020805", "", new string[] { _objBatch.BatNbr });
                }
                if ((_objBatch.BatNbr.PassNull() == string.Empty && !access.Insert) || (_objBatch.BatNbr.PassNull() != string.Empty && !access.Update))
                {
                    throw new MessageException(MessageType.Message, "728");
                }
                _objIN = _db.IN_Setup.FirstOrDefault(p => p.BranchID == _objBatch.BranchID);
                if (_objIN == null) _objIN = new IN_Setup();
                string lineRef = Util.PassNull(data["LineRef"]);
                List<IN_Trans> lstTrans = new List<IN_Trans>();

                if (_whseLoc.PassNull() != "")
                {
                    lstTrans = _db.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.WhseLoc == _whseLoc).ToList();
                    var trans = lstTrans.FirstOrDefault(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.LineRef == lineRef && p.WhseLoc == _whseLoc);

                    if (trans != null)
                    {
                        double oldQty = 0;
                        if (trans.TranType == "II")
                        {
                            oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                            UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                            Update_IN_ItemLoc(_whseLoc, trans.InvtID, trans.SiteID, oldQty, 0);
                        }

                        lstTrans.Remove(trans);
                        _db.IN_Trans.DeleteObject(trans);
                    }

                    var lstLot = _db.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef && p.WhseLoc == _whseLoc).ToList();
                    foreach (var lot in lstLot)
                    {
                        double oldQty = 0;
                        if (lot.TranType == "II")
                        {
                            oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                            if (lot.WhseLoc.PassNull() != "")
                            {
                                UpdateAllocLot_WhseLoc(_whseLoc, lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                            }
                            else
                            {
                                UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                            }
                        }
                        _db.IN_LotTrans.DeleteObject(lot);
                    }
                }
                else
                {
                    lstTrans = _db.IN_Trans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
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
                        _db.IN_Trans.DeleteObject(trans);
                    }

                    var lstLot = _db.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef).ToList();
                    foreach (var lot in lstLot)
                    {
                        double oldQty = 0;
                        if (lot.TranType == "II")
                        {
                            oldQty = lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact;
                            UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                        }
                        _db.IN_LotTrans.DeleteObject(lot);
                    }


                }

                var batch = _db.Batches.FirstOrDefault(p => p.BatNbr == _objBatch.BatNbr && p.BranchID == _objBatch.BranchID);
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

                _db.SaveChanges();

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
    }
}

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
        private string _whseLoc = "";
        IN10400Entities _app = Util.CreateObjectContext<IN10400Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private IN10400_pcBatch_Result _objBatch;
        private FormCollection _form;
        private IN_Setup _objIN;
        private List<IN10400_pgAdjustmentLoad_Result> _lstTrans;
        private List<IN10400_pgIN_LotTrans_Result> _lstLot;
        private int _decQty;
        private int _decAmt;
        private int _decPrice;

        private JsonResult _logMessage;

        public ActionResult Index(string branchID)
        {
            Util.InitRight(_screenNbr);
            int showWhseLoc = 0;
            var objconfig = _app.IN10400_pdConfig(Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault();
            if (objconfig != null)
            {
                showWhseLoc = objconfig.ShowWhseLoc.Value;
            }
            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);
            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }
            var userDft = _app.OM_UserDefault.FirstOrDefault(p => p.DfltBranchID == branchID && p.UserID == Current.UserName);
            if (branchID == null) branchID = Current.CpnyID;
            ViewBag.INSite = userDft == null ? "" : userDft.INSite;
            ViewBag.WhseLoc = userDft == null ? "" : userDft.INWhseLoc;
            ViewBag.BranchID = branchID;
            ViewBag.showWhseLoc = showWhseLoc;
            return View();

        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        #region LoadData
        public ActionResult GetBatch(string branchID, string query, int start, int limit, int page)
        {
            query = query ?? string.Empty;
            if (page != 1) query = string.Empty;
            var lstBatch = _app.IN10400_pcBatch(branchID, query, start, start + 20).ToList();
            var paging = new Paging<IN10400_pcBatch_Result>(lstBatch, lstBatch.Count > 0 ? lstBatch[0].TotalRecords.Value : 0);
            return this.Store(paging.Data, paging.TotalRecords);
        }

        //public ActionResult GetBatch(string branchID)
        //{
        //    var lstBatch = _app.IN10400_pcINAdjustmentBatch(branchID ).ToList();
        //    return this.Store(lstBatch);
        //}

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

        public ActionResult GetItemSite(string invtID, string siteID, string whseLoc, int showWhseLoc)
        {
            if (showWhseLoc == 2||(showWhseLoc==1 && whseLoc.PassNull()!=""))
            {
                var objSite = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID &&p.WhseLoc==whseLoc);
                return this.Store(objSite);
            }
            else
            {
                var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
                return this.Store(objSite);
            }            
        }

        public ActionResult GetLot(string invtID, string siteID, string batNbr, string branchID,string whseLoc,int showWhseLoc)
        {
            List<IN10400_pdGetLot_Result> lstLot = new List<IN10400_pdGetLot_Result>();
            //List<IN_ItemLot> lstLotDB = _app.IN_ItemLot.Where(p => p.SiteID == siteID && p.InvtID == invtID && p.QtyAvail > 0).ToList();
            if(showWhseLoc==2 ||(showWhseLoc==1 && whseLoc.PassNull() == ""))
            {
                List<IN10400_pdGetLot_Result> lstLotDB = _app.IN10400_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).Where(p=>p.WhseLoc==whseLoc).ToList();
                foreach (var item in lstLotDB)
                {
                    lstLot.Add(item);
                }
                List<IN_LotTrans> lstLotTrans = _app.IN_LotTrans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc==whseLoc).ToList();
                foreach (var item in lstLotTrans)
                {
                    var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                    if (lot == null)
                    {
                        var lotDB = _app.IN10400_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr && p.WhseLoc==whseLoc);
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
                List<IN10400_pdGetLot_Result> lstLotDB = _app.IN10400_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
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
                        var lotDB = _app.IN10400_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
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
        public ActionResult GetLotTrans(string branchID, string batNbr,string whseLoc)
        {
            List<IN10400_pgIN_LotTrans_Result> lstLotTrans = _app.IN10400_pgIN_LotTrans(batNbr,branchID,whseLoc,Current.UserName,Current.CpnyID,Current.LangID).ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetItemLot(string invtID, string siteID, string lotSerNbr, string branchID, string batNbr, string whseLoc, int showWhseLoc)
        {
            var lot = new IN10400_pdGetLot_Result();
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                lot = _app.IN10400_pdGetLot(siteID,invtID,Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr && p.WhseLoc==whseLoc);

                if (lot == null) lot = new IN10400_pdGetLot_Result()
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
                lot = _app.IN10400_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr);

                if (lot == null) lot = new IN10400_pdGetLot_Result()
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
            
            List<IN10400_pdGetLot_Result> lstLot = new List<IN10400_pdGetLot_Result>() { lot };
            return this.Store(lstLot, lstLot.Count);
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
                _whseLoc = data["WhseLoc"].PassNull();
                _objBatch = data.ConvertToObject<IN10400_pcBatch_Result>();

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
                    if (trans != null)
                    {
                        if (trans.Qty < 0)
                        {
                            oldQty = Math.Abs(trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact);
                            //oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                            UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                            if (_whseLoc.PassNull() != "")
                            {
                                UpdateIN_ItemLoc(trans.InvtID, trans.SiteID, _whseLoc, oldQty, 0);
                            }                            
                        }
                    }
                    _app.IN_Trans.DeleteObject(trans);
                }

                var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr).ToList();
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;
                    if (lot != null)
                    {
                        if (lot.Qty  < 0)
                        {
                            oldQty = Math.Abs(lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact);
                            UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr, _whseLoc, oldQty, 0, 0);
                        }
                    }
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
                var access = Session["IN10400"] as AccessRight;

                //_objBatch = data.ConvertToObject<IN10400_pcINAdjustmentBatch_Result>();
                string branchID = data["txtBranchID"];
                string batNbr = data["cboBatNbr"];
                _whseLoc = data["WhseLoc"].PassNull();
                _objBatch = data.ConvertToObject<IN10400_pcBatch_Result>();
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
                    if (trans.Qty < 0)
                    {
                        oldQty = Math.Abs(trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact);
                        //oldQty = trans.UnitMultDiv == "D" ? trans.Qty / trans.CnvFact : trans.Qty * trans.CnvFact;
                        UpdateINAlloc(trans.InvtID, trans.SiteID, oldQty, 0);
                        if (_whseLoc.PassNull() != "")
                        {
                            UpdateIN_ItemLoc(trans.InvtID, trans.SiteID, _whseLoc, oldQty, 0);
                        }                        
                    }         
                    lstTrans.Remove(trans);
                    _app.IN_Trans.DeleteObject(trans);
                }
                var lstLot = _app.IN_LotTrans.Where(p => p.BranchID == _objBatch.BranchID && p.BatNbr == _objBatch.BatNbr && p.INTranLineRef == lineRef).ToList();
                foreach (var lot in lstLot)
                {
                    double oldQty = 0;
                    if (lot.Qty < 0)
                    {
                        oldQty = Math.Abs(lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact);
                        UpdateAllocLot(lot.InvtID, lot.SiteID, lot.LotSerNbr,_whseLoc, oldQty, 0, 0);
                    }
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
                var objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null && objInvt.StkItem == 1)
                {
                    var objLoc = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc);
                    if (objLoc == null) objLoc = new IN_ItemLoc() { SiteID = siteID, InvtID = invtID };
                    if (!_objIN.NegQty && newQty > 0 && objLoc.QtyAvail + oldQty - newQty < 0)
                    {
                        throw new MessageException(MessageType.Message, "2018052413", "", new string[] { invtID, siteID,whseLoc});
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
        private bool UpdateAllocLot(string invtID, string siteID, string lotSerNbr,string whseLoc, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                if (whseLoc.PassNull() != "")
                {
                    var objItemLot = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr && p.WhseLoc==whseLoc);
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
                }
                else
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
                }                
                return true;
            }
            return true;
        }
        private void SaveData(FormCollection data)
        {
            string branchID = data["txtBranchID"];
            string batNbr = data["cboBatNbr"];
            _whseLoc = data["WhseLoc"];
            if (_lstTrans == null)
            {
                var transHandler = new StoreDataHandler(data["lstTrans"]);
                _lstTrans = transHandler.ObjectData<IN10400_pgAdjustmentLoad_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty).ToList();
            }
            if (_lstLot == null)
            {
                var lotHandler = new StoreDataHandler(data["lstLot"]);
                _lstLot = lotHandler.ObjectData<IN10400_pgIN_LotTrans_Result>().Where(p => Util.PassNull(p.INTranLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            }
            _objBatch = data.ConvertToObject<IN10400_pcBatch_Result>();

            //if (string.IsNullOrEmpty(batNbr))
            //{

            //    _objBatch = _app.IN10400_pcINAdjustmentBatch(branchID).FirstOrDefault(p => p.BatNbr == batNbr);
            //    if (_objBatch == null)
            //    {
            //        _objBatch = new IN10400_pcINAdjustmentBatch_Result();
            //        _objBatch.EditScrnNbr = "IN10400";
            //        _objBatch.Descr = data["txtDescr"];
            //        _objBatch.ReasonCD = data["cboReasonCD"];
            //        _objBatch.BranchID = branchID;
            //        _objBatch.TotAmt = Convert.ToDouble(data["txtTotAmt"]);
            //        _objBatch.Module = "IN";
            //        _objBatch.JrnlType = "IN";
            //        _objBatch.DateEnt = Convert.ToDateTime(data["txtDateEnt"]);

            //    }
            //}
            //else {
            //    _objBatch = _app.IN10400_pcINAdjustmentBatch(branchID).FirstOrDefault(p => p.BatNbr == batNbr);
            //    if (_objBatch != null)
            //    {
            //        _objBatch = new IN10400_pcINAdjustmentBatch_Result();
            //        _objBatch.ResetET();
            //        var bacth = new StoreDataHandler(data["lstbatch"]);
                
            //        _objBatch = bacth.ObjectData<IN10400_pcINAdjustmentBatch_Result>().FirstOrDefault(p => p.BatNbr == batNbr);
            //    }
            //    else {
            //        _objBatch = new IN10400_pcINAdjustmentBatch_Result();
            //        _objBatch.EditScrnNbr = "IN10400";
            //        _objBatch.Descr = data["txtDescr"];
            //        _objBatch.ReasonCD = data["cboReasonCD"];
            //        _objBatch.BranchID = branchID;
            //        _objBatch.TotAmt = Convert.ToDouble(data["txtTotAmt"]);
            //        _objBatch.Module = "IN";
            //        _objBatch.JrnlType = "IN";
            //        _objBatch.DateEnt = Convert.ToDateTime(data["txtDateEnt"]);
            //    }
            //}
            _decAmt = _sys.SYS_Configurations.FirstOrDefault(p => p.Code.ToLower() == "DecPlTranAmt".ToLower()).IntVal;
            _decPrice = _sys.SYS_Configurations.FirstOrDefault(p => p.Code.ToLower() == "DecPlUnitPrice".ToLower()).IntVal;
            _decQty = _sys.SYS_Configurations.FirstOrDefault(p => p.Code.ToLower() == "DecPlQty".ToLower()).IntVal;

            _handle = data["cboHandle"].PassNull();
            _objBatch.Status = _objBatch.Status.PassNull() == string.Empty ? "H" : _objBatch.Status;

            var tam = _app.IN10400_pdCheckCloseDateSetUp(Current.CpnyID, Current.UserName, Current.LangID, _objBatch.BranchID, _objBatch.DateEnt.ToDateShort(), _objBatch.BatNbr).FirstOrDefault();
            if (tam != null)
            {
                if (tam.CheckCloseDateSetUp == false)
                {
                    throw new MessageException(MessageType.Message, "301");
                }
            }            
            // sau khi save xong gọi tới hàm tạo user hoặc chuyển save, truyền xuống danh sách
            Dictionary<string, string> dicData = new Dictionary<string, string>();
            dicData.Add("@UserName", Current.UserName);
            dicData.Add("@CpnyID", Current.CpnyID);
            dicData.Add("@LangID", Current.LangID.ToString());
            dicData.Add("@BranchID", _objBatch.BranchID);
            dicData.Add("@TranDate", _objBatch.DateEnt.ToDateShort().ToString());
            dicData.Add("@BatNbr", _objBatch.BatNbr);

            Util.getDataTableFromProc("IN10400_pdCheckCloseDateSetUp", dicData);


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
                Save_Batch(batch, data,_objBatch);
            }
            _app.SaveChanges();
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
                if (!string.IsNullOrEmpty(invtID))
                {
                    if (_whseLoc.PassNull() != "")
                    {
                        var obj = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc==_whseLoc);
                        if (obj == null)
                        {
                            throw new MessageException("2018052411", new[] { invtID, siteID,_whseLoc });
                        }
                    }
                    else
                    {
                        var obj = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
                        if (obj == null)
                        {
                            throw new MessageException("2016042101", new[] { invtID, siteID });
                        }
                    }
                    
                    IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                    if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack != "N")
                    {
                        //if (_lstTrans[i].Qty == 0 && _lstLot.Where(p => p.INTranLineRef == _lstTrans[i].LineRef && p.Qty != 0).Count() == 0)
                        //{
                        //    throw new MessageException("201605301", new[] { Util.GetLang("Qty") });
                        //}
                        if (_lstTrans[i].Qty == 0 && _lstTrans[i].TranAmt == 0)
                        {
                            throw new MessageException("201605301", new[] { Util.GetLang("Qty") });
                        }
                    }
                    else
                    {
                        if (_lstTrans[i].Qty == 0 && _lstTrans[i].TranAmt == 0)
                        {
                            throw new MessageException("201605301", new[] { Util.GetLang("Qty") });
                        }
                    }
                    if (_lstTrans[i].SiteID.PassNull() == string.Empty)
                    {
                        throw new MessageException("1000", new[] { Util.GetLang("SiteID") });
                    }
                    if (_lstTrans[i].UnitMultDiv.PassNull() == string.Empty || _lstTrans[i].UnitDesc.PassNull() == string.Empty)
                    {
                        throw new MessageException("2525", new[] { _lstTrans[i].InvtID });
                    }
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
        private void Save_Batch(Batch batch,FormCollection data,IN10400_pcBatch_Result s)
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
                if (string.IsNullOrWhiteSpace(trans.InvtID))
                {
                    continue;
                }
                var transDB = _app.IN_Trans.FirstOrDefault(p =>
                                p.BatNbr == _objBatch.BatNbr && p.RefNbr == trans.RefNbr &&
                                p.BranchID == _objBatch.BranchID && p.LineRef == trans.LineRef);

                if (transDB != null)
                {
                    if (transDB.tstamp.ToHex() != trans.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    Update_Trans(batch, transDB, trans,_whseLoc, false);
                }
                else
                {
                    transDB = new IN_Trans();
                    Update_Trans(batch, transDB, trans,_whseLoc, true);
                    _app.IN_Trans.AddObject(transDB);
                }
                Save_Lot(batch, transDB);
            }
            _app.SaveChanges();
        }
        private bool Save_Lot(Batch batch, IN_Trans tran)
        {
            var lots = _app.IN_LotTrans.Where(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr && p.RefNbr == tran.RefNbr).ToList();
            foreach (var item in lots)
            {
                if (item.EntityState == EntityState.Deleted || item.EntityState == EntityState.Detached) continue;
                if (!_lstLot.Any(p => p.INTranLineRef == item.INTranLineRef && p.LotSerNbr == item.LotSerNbr))
                {
                    var oldQty = item.UnitMultDiv == "D" ? item.Qty / item.CnvFact : item.Qty * item.CnvFact;
                    UpdateAllocLot(item.InvtID, item.SiteID, item.LotSerNbr,_whseLoc, -oldQty, 0, 0);
                    _app.IN_LotTrans.DeleteObject(item);
                }
            }

            var lstLotTmp = _lstLot.Where(p => p.INTranLineRef == tran.LineRef).ToList();
            foreach (var lotCur in lstLotTmp)
            {
                var lot = _app.IN_LotTrans.FirstOrDefault(p => p.BranchID == batch.BranchID && p.BatNbr == batch.BatNbr && p.INTranLineRef == lotCur.INTranLineRef && p.LotSerNbr == lotCur.LotSerNbr);
                if (lot == null || lot.EntityState == EntityState.Deleted || lot.EntityState == EntityState.Detached)
                {
                    lot = new IN_LotTrans();
                    Update_Lot(lot, lotCur, batch, tran,_whseLoc, true);
                    _app.IN_LotTrans.AddObject(lot);
                }
                else
                {
                    Update_Lot(lot, lotCur, batch, tran, _whseLoc, false);
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
            {
                t.RefNbr = _objBatch.RefNbr;
            }               
            t.JrnlType = _objBatch.JrnlType.PassNull() == string.Empty ? "IN" : _objBatch.JrnlType;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.DateEnt = _objBatch.DateEnt.ToDateShort();
            t.Descr = _objBatch.Descr;
            t.EditScrnNbr = t.EditScrnNbr.PassNull() == string.Empty ? _screenNbr : t.EditScrnNbr;
           // t.FromToSiteID = _objBatch.FromToSiteID;
           // t.IntRefNbr = _objBatch.IntRefNbr;
            t.NoteID = 1;
           // t.RvdBatNbr = _objBatch.RvdBatNbr;
            t.ReasonCD = _objBatch.ReasonCD;
            t.TotAmt = Math.Round(_objBatch.TotAmt, _decAmt);
            t.Rlsed = 0;
            t.Status = _objBatch.Status;
        }
        private void Update_Trans(Batch batch, IN_Trans t, IN10400_pgAdjustmentLoad_Result s, string whseLoc, bool isNew)
        {
            double oldQty = 0, newQty =0;
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
                else {
                    oldQty = 0;
                }

                if (s.Qty < 0)
                {
                    newQty =  Math.Abs(s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact);
                }
                if (_whseLoc.PassNull() != "")
                {
                    UpdateIN_ItemLoc(t.InvtID, t.SiteID, _whseLoc, oldQty, 0);
                    UpdateIN_ItemLoc(s.InvtID, s.SiteID, _whseLoc, 0, newQty);
                }
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
            t.WhseLoc = whseLoc.PassNull();
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
        private bool Update_Lot(IN_LotTrans t, IN10400_pgIN_LotTrans_Result s, Batch batch, IN_Trans tran, string whseLoc, bool isNew)
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

            if (tran.TranType == "AJ")
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
                    newQty = Math.Abs(s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact);
                }
                //if (!isNew)
                //    oldQty = s.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                //else
                //    oldQty = 0;

                //newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

                UpdateAllocLot(t.InvtID, t.SiteID, t.LotSerNbr, whseLoc, oldQty, 0, 0);

                if (whseLoc.PassNull() != "")
                {
                    if (!UpdateAllocLot(s.InvtID, s.SiteID, t.LotSerNbr, whseLoc, 0, newQty, 0))
                    {
                        throw new MessageException("2018052412", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID,whseLoc });
                    }
                }
                else
                {
                    if (!UpdateAllocLot(s.InvtID, s.SiteID, t.LotSerNbr, whseLoc, 0, newQty, 0))
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
            t.WarrantyDate = s.WarrantyDate;
            t.SiteID = s.SiteID;
            t.WhseLoc = whseLoc.PassNull();
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
        #endregion       

    }
}

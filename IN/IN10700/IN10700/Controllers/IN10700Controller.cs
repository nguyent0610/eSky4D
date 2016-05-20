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
//using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
using IN10700.Models;
namespace IN10700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10700Controller : Controller
    {
        private string _screenNbr = "IN10700";
        IN10700Entities _db = Util.CreateObjectContext<IN10700Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

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

        public ActionResult GetStockOutlet(string branchID, string slsperID, string custID, string stockType, DateTime stkOutDate)
        {
            return this.Store(_db.IN10700_phStockOutlet(Current.UserName, Current.CpnyID, branchID, slsperID, custID, stockType, stkOutDate).ToList());
        }

        public ActionResult GetStockOutletDet(string branchID, string slsperID, string stkOutNbr)
        {
            return this.Store(_db.IN10700_pgStockOutletDet(Current.UserName, Current.CpnyID, branchID, slsperID, stkOutNbr).ToList());
        }
        public ActionResult GetStockOutletPOSM(string branchID, string slsperID, string stkOutNbr)
        {
            return this.Store(_db.IN10700_pgStockOutletDetPOSM(Current.UserName, Current.CpnyID, branchID, slsperID, stkOutNbr).ToList());
        }

        public ActionResult SaveData(FormCollection data, bool isNew)
        {
            try
            {
                var lstStockOutletHandler = new StoreDataHandler(data["lstStockOutlet"]);
                var inputStockOutlet = lstStockOutletHandler.ObjectData<IN10700_phStockOutlet_Result>().FirstOrDefault();



                var lstStockOutletDetChangeHandler = new StoreDataHandler(data["lstStockOutletDetChange"]);
                var lstStockOutletDetChange = lstStockOutletDetChangeHandler.BatchObjectData<IN10700_pgStockOutletDet_Result>();

                var lstStockOutletPOSMHandle = new StoreDataHandler(data["lstStockOutletPOSM"]);
                var lstStockOutletPOSM = lstStockOutletPOSMHandle.BatchObjectData<IN10700_pgStockOutletDetPOSM_Result>();
             

                var outlet = _db.PPC_StockOutlet.FirstOrDefault(o => o.BranchID == inputStockOutlet.BranchID 
                    && o.SlsPerID == inputStockOutlet.SlsPerID 
                    && o.StkOutNbr == inputStockOutlet.StkOutNbr);
                #region Header
                if (outlet != null)
                {
                    //update
                    if(!isNew)
                    {
                        if (outlet.tstamp.ToHex() == inputStockOutlet.tstamp.ToHex())
                        {
                            // update info
                            updateStockOutlet(ref outlet, inputStockOutlet, false);
                        }
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "2000", "", new string[]{
                                    Util.GetLang("StkOutNbr")
                                });
                    }
                }
                else { 
                    //new
                    if (isNew)
                    {
                        var setup = _db.IN_Setup.FirstOrDefault(s => s.SetupID == "IN" && s.BranchID == inputStockOutlet.BranchID);
                        if (setup != null)
                        {
                            var newStkOut = _db.IN10700_ppStkOutNbr(Current.CpnyID, Current.UserName, inputStockOutlet.BranchID).FirstOrDefault();
                            if (newStkOut != null)
                            {
                                inputStockOutlet.StkOutNbr = newStkOut.PrefixBat + newStkOut.LastStkOutNbr;

                                var outletAuto = _db.PPC_StockOutlet.FirstOrDefault(c => c.StkOutNbr == inputStockOutlet.StkOutNbr && c.BranchID == inputStockOutlet.BranchID);
                                if (outletAuto != null)
                                {
                                    throw new MessageException(MessageType.Message, "8001", "", new string[] { string.Format("{0}: {1}", Util.GetLang("StkOutNbr"), inputStockOutlet.StkOutNbr) });
                                }
                                //add new outlet
                                updateStockOutlet(ref outlet, inputStockOutlet, true);
                                _db.PPC_StockOutlet.AddObject(outlet);

                                setup.LastStkOutNbr = newStkOut.LastStkOutNbr;
                                setup.LUpd_DateTime = DateTime.Now;
                                setup.LUpd_Prog = _screenNbr;
                                setup.LUpd_User = Current.UserName;
                            }
                            else {
                                throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("Setup") });
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "20404", "", new string[] { Util.GetLang("BranchID") });
                        }
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                #endregion

                #region Detail
                lstStockOutletDetChange.Updated.AddRange(lstStockOutletDetChange.Created);
                foreach (var deleted in lstStockOutletDetChange.Deleted)
                {
                    if (!string.IsNullOrWhiteSpace(deleted.InvtID) )
                    {
                        if (lstStockOutletDetChange.Updated.Where(p => p.InvtID == deleted.InvtID && p.ExpDate==deleted.ExpDate).Count() == 0)
                        {
                            deleted.StkOutNbr = outlet.StkOutNbr;
                            deleted.BranchID = outlet.BranchID;
                            deleted.SlsperID = outlet.SlsPerID;

                            var deletedDetail = _db.PPC_StockOutletDet.FirstOrDefault(
                               x => x.BranchID == deleted.BranchID
                                    && x.StkOutNbr == deleted.StkOutNbr
                                    && x.SlsPerID == deleted.SlsperID
                                    && x.InvtID == deleted.InvtID
                                    && x.ExpDate == deleted.ExpDate);
                            if (deletedDetail != null)
                            {
                                _db.PPC_StockOutletDet.DeleteObject(deletedDetail);
                            }

                            var lstdeletedPOSM = _db.PPC_StockOutletPOSM.Where(
                              x => x.BranchID == outlet.BranchID
                                   && x.StkOutNbr == outlet.StkOutNbr
                                   && x.SlsPerID == outlet.SlsPerID
                                   && x.InvtID == deleted.InvtID).ToList();
                            foreach (var obj in lstdeletedPOSM)
                            {
                                _db.PPC_StockOutletPOSM.DeleteObject(obj);
                            }
                        }
                        else
                        {
                            lstStockOutletDetChange.Updated.Where(p => p.InvtID == deleted.InvtID).FirstOrDefault().tstamp = deleted.tstamp;
                        }


                    }
                }

                foreach (var updated in lstStockOutletDetChange.Updated)
                {
                    if (!string.IsNullOrWhiteSpace(updated.InvtID))
                    {
                        updated.StkOutNbr = outlet.StkOutNbr;
                        updated.BranchID = outlet.BranchID;
                        updated.SlsperID = outlet.SlsPerID;

                        var updatedDetail = _db.PPC_StockOutletDet.FirstOrDefault(
                            x => x.BranchID == updated.BranchID
                                && x.StkOutNbr == updated.StkOutNbr
                                && x.SlsPerID == updated.SlsperID
                                && x.InvtID == updated.InvtID
                                && x.ExpDate == updated.ExpDate);
                        if (updatedDetail != null)
                        {
                            if (updatedDetail.tstamp.ToHex() == updated.tstamp.ToHex())
                            {
                                updateStockOutletDet(ref updatedDetail, updated, false);
                            } 
                            else throw new MessageException(MessageType.Message, "19");
                        }
                        else
                        {
                            updateStockOutletDet(ref updatedDetail, updated, true);
                            _db.PPC_StockOutletDet.AddObject(updatedDetail);
                        }
                    }
                }

              
                #endregion


                #region Detail POSM
                lstStockOutletPOSM.Updated.AddRange(lstStockOutletPOSM.Created);
                foreach (var deleted in lstStockOutletPOSM.Deleted)
                {
                    if (!string.IsNullOrWhiteSpace(deleted.PosmID) && lstStockOutletDetChange.Updated.Where(p => p.InvtID == deleted.InvtID).Count() > 0)
                    {
                        if (lstStockOutletPOSM.Updated.Where(p => p.InvtID == deleted.InvtID && p.PosmID==deleted.PosmID && p.ExpDate==deleted.ExpDate).Count() == 0)
                        {
                            var deletedDetail = _db.PPC_StockOutletPOSM.FirstOrDefault(
                               x => x.BranchID == outlet.BranchID
                                    && x.StkOutNbr == outlet.StkOutNbr
                                    && x.SlsPerID == outlet.SlsPerID
                                    && x.InvtID == deleted.InvtID
                                    && x.PosmID == deleted.PosmID
                                    && x.ExpDate == deleted.ExpDate);
                            if (deletedDetail != null)
                            {
                                _db.PPC_StockOutletPOSM.DeleteObject(deletedDetail);
                            }
                        }
                        else
                        {
                            lstStockOutletPOSM.Updated.Where(p => p.InvtID == deleted.InvtID).FirstOrDefault().tstamp = deleted.tstamp;
                        }
                    }
                }
                foreach (var updated in lstStockOutletPOSM.Updated)
                {
                    if (!string.IsNullOrWhiteSpace(updated.PosmID))
                    {                    
                        var updatedDetail = _db.PPC_StockOutletPOSM.FirstOrDefault(
                            x => x.BranchID == outlet.BranchID
                                && x.StkOutNbr == outlet.StkOutNbr
                                && x.SlsPerID == outlet.SlsPerID
                                && x.InvtID == updated.InvtID
                                && x.PosmID == updated.PosmID
                                && x.ExpDate == updated.ExpDate);
                        if (updatedDetail != null)
                        {
                            if (updatedDetail.tstamp.ToHex() == updated.tstamp.ToHex())
                            {
                                updateStockOutletPOSM(ref updatedDetail, updated, outlet, false);
                            }
                            else throw new MessageException(MessageType.Message, "19"); 
                        }
                        else
                        {
                            updateStockOutletPOSM(ref updatedDetail, updated,outlet, true);
                            _db.PPC_StockOutletPOSM.AddObject(updatedDetail);
                        }
                    }
                }               
                #endregion
                _db.SaveChanges();

                return Json(new { success = true, msgCode = 201405071 });
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

        private void updateStockOutletDet(ref PPC_StockOutletDet updatedDetail, IN10700_pgStockOutletDet_Result updated, bool isNew)
        {
            if (isNew) 
            {
                updatedDetail = new PPC_StockOutletDet();
                updatedDetail.ResetET();
                updatedDetail.BranchID = updated.BranchID;
                updatedDetail.SlsPerID = updated.SlsperID;
                updatedDetail.StkOutNbr = updated.StkOutNbr;

                updatedDetail.InvtID = updated.InvtID;
                updatedDetail.ExpDate = updated.ExpDate.PassMin().Date;

                updatedDetail.Crtd_DateTime = DateTime.Now;
                updatedDetail.Crtd_Prog = _screenNbr;
                updatedDetail.Crtd_User = Current.UserName;

                updatedDetail.CS = 0;
                updatedDetail.PC = 0;
                updatedDetail.ProdDate = new DateTime(1900, 1, 1);
            }
            updatedDetail.StkQty = updated.StkQty;
            updatedDetail.ReasonID = updated.ReasonID;
            updatedDetail.PosmID = updated.PosmID;

            updatedDetail.LUpd_DateTime = DateTime.Now;
            updatedDetail.LUpd_Prog = _screenNbr;
            updatedDetail.LUpd_User = Current.UserName;
        }
        private void updateStockOutletPOSM(ref PPC_StockOutletPOSM updatedDetail, IN10700_pgStockOutletDetPOSM_Result updated,PPC_StockOutlet objHeader, bool isNew)
        {
            if (isNew)
            {
                updatedDetail = new PPC_StockOutletPOSM();
                updatedDetail.ResetET();
                updatedDetail.BranchID = objHeader.BranchID;
                updatedDetail.SlsPerID = objHeader.SlsPerID;
                updatedDetail.StkOutNbr = objHeader.StkOutNbr;

                updatedDetail.InvtID = updated.InvtID;
                updatedDetail.PosmID = updated.PosmID;
                updatedDetail.ExpDate = updated.ExpDate.PassMin().Date;

                updatedDetail.Crtd_DateTime = DateTime.Now;
                updatedDetail.Crtd_Prog = _screenNbr;
                updatedDetail.Crtd_User = Current.UserName;

                updatedDetail.CS = 0;
                updatedDetail.PC = 0;
                updatedDetail.ProdDate = new DateTime(1900, 1, 1);
            }
            updatedDetail.StkQty = updated.StkQty;
         
            updatedDetail.LUpd_DateTime = DateTime.Now;
            updatedDetail.LUpd_Prog = _screenNbr;
            updatedDetail.LUpd_User = Current.UserName;
        }
        private void updateStockOutlet(ref PPC_StockOutlet outlet, IN10700_phStockOutlet_Result inputStockOutlet, bool isNew)
        {
            if (isNew) {
                outlet = new PPC_StockOutlet();
                outlet.ResetET();
                outlet.BranchID = inputStockOutlet.BranchID;
                outlet.CustID = inputStockOutlet.CustID;
                outlet.SlsPerID = inputStockOutlet.SlsPerID;
                outlet.StkOutNbr = inputStockOutlet.StkOutNbr;
                outlet.StkOutDate = inputStockOutlet.StkOutDate;
                outlet.StockType = inputStockOutlet.StockType;

                outlet.Crtd_DateTime = DateTime.Now;
                outlet.Crtd_Prog = _screenNbr;
                outlet.Crtd_User = Current.UserName;

                outlet.LUpd_DateTime = DateTime.Now;
                outlet.LUpd_Prog = _screenNbr;
                outlet.LUpd_User = Current.UserName;
            }
            
        }

        [HttpPost]
        public ActionResult DeleteHeader(FormCollection data)
        {
            try
            {
                var lstStockOutletHandler = new StoreDataHandler(data["lstStockOutlet"]);
                var inputStockOutlet = lstStockOutletHandler.ObjectData<IN10700_phStockOutlet_Result>().FirstOrDefault();
                var objHeader = _db.PPC_StockOutlet.Where(p => p.BranchID == inputStockOutlet.BranchID && p.SlsPerID == inputStockOutlet.SlsPerID && p.StkOutNbr == inputStockOutlet.StkOutNbr).FirstOrDefault();
                if (objHeader == null)
                {
                }
                else
                {


                    _db.PPC_StockOutlet.DeleteObject(objHeader);
                    var lstdel = _db.PPC_StockOutletDet.Where(p => p.BranchID == inputStockOutlet.BranchID && p.SlsPerID == inputStockOutlet.SlsPerID && p.StkOutNbr == inputStockOutlet.StkOutNbr).ToList();
                    while (lstdel.FirstOrDefault() != null)
                    {

                        _db.PPC_StockOutletDet.DeleteObject(lstdel.FirstOrDefault());
                        lstdel.Remove(lstdel.FirstOrDefault());
                    }

                    var lstdelPOSM = _db.PPC_StockOutletPOSM.Where(p => p.BranchID == inputStockOutlet.BranchID && p.SlsPerID == inputStockOutlet.SlsPerID && p.StkOutNbr == inputStockOutlet.StkOutNbr).ToList();
                    while (lstdelPOSM.FirstOrDefault() != null)
                    {

                        _db.PPC_StockOutletPOSM.DeleteObject(lstdelPOSM.FirstOrDefault());
                        lstdelPOSM.Remove(lstdelPOSM.FirstOrDefault());
                    }
                }
                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Delete);
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

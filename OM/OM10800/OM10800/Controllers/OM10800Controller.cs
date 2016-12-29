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
using System.Drawing;
using System.Globalization;
using HQ.eSkySys;
namespace OM10800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM10800Controller : Controller
    {
        private string _screenNbr = "OM10800";
        private string _userName = Current.UserName;
        OM10800Entities _db = Util.CreateObjectContext<OM10800Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
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

            ViewBag.BranchID = branchID;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetHeader(string branchID, string batNbr)
        {
            var obj = _db.OM10800_pdHeader(Current.CpnyID, Current.UserName, Current.LangID, branchID,batNbr).FirstOrDefault();
            return this.Store(obj);
        }
        public ActionResult GetOrder(string branchID, string batNbr, DateTime startDate, DateTime endDate)
        {
            _db.CommandTimeout = int.MaxValue;
            var orders = _db.OM10800_pgOrder(Current.UserName, Current.CpnyID, Current.LangID,branchID,batNbr, startDate, endDate).ToList();
            return this.Store(orders);
        }
        public ActionResult GetDet(string branchID, string batNbr,
            DateTime startDate, DateTime endDate)
        {
            _db.CommandTimeout = int.MaxValue;
            var dets = _db.OM10800_pgDet(Current.UserName, Current.CpnyID, Current.LangID, branchID, batNbr,startDate, endDate).ToList();
            return this.Store(dets);
        }
        public ActionResult GetDelivery(string branchID, string batNbr)
        {
            _db.CommandTimeout = int.MaxValue;
            var dets = _db.OM10800_pgDelivery(Current.UserName, Current.CpnyID, Current.LangID, branchID, batNbr).ToList();
            return this.Store(dets);
        }
        [HttpPost]
        public ActionResult DeleteHeader(FormCollection data)
        {
            try
            {
                string BranchID = data["txtBranchID"].PassNull();
                string BatNbr = data["cboBatNbr"].PassNull();

                var batch = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "OM");
                if (batch != null)
                {
                    _db.Batches.DeleteObject(batch);
                }

                var lstOM_ShipLine = _db.OM_ShipLine.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (var item in lstOM_ShipLine)
                {
                    _db.OM_ShipLine.DeleteObject(item);
                }

                var lstOM_ShipDelivery = _db.OM_ShipDelivery.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (var item in lstOM_ShipDelivery)
                {
                    _db.OM_ShipDelivery.DeleteObject(item);
                }

                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Delete, BatNbr);
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstHeader"]);
                var curHeader = dataHandler.ObjectData<OM10800_pdHeader_Result>().FirstOrDefault();
                var CpnyID = data["txtBranchID"].PassNull().ToUpper().Trim();
                var Handle = data["cboHandle"].PassNull().ToUpper().Trim();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstOrder"]);
                ChangeRecords<OM10800_pgOrder_Result> lstOrder = dataHandler1.BatchObjectData<OM10800_pgOrder_Result>();

                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstDelivery"]);
                ChangeRecords<OM10800_pgDelivery_Result> lstDelivery = dataHandler2.BatchObjectData<OM10800_pgDelivery_Result>();

                #region Save Header Company
                var header = _db.Batches.FirstOrDefault(p => p.BranchID == CpnyID && p.BatNbr==curHeader.BatNbr && p.Module=="OM");
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        Updating_Batch(ref header, curHeader);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new Batch();
                    header.ResetET();
                    header.BranchID = CpnyID;
                    header.BatNbr = _db.OMNumbering(CpnyID, "Shipper", "SH").FirstOrDefault();
                    header.Module = "OM";

                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = _userName;
                    Updating_Batch(ref header, curHeader);
                    _db.Batches.AddObject(header);
                }
                #endregion

                #region Save Order

                lstOrder.Created.AddRange(lstOrder.Updated);
                var lstRowDB = _db.OM_ShipLine.Where(p => p.BranchID == header.BranchID && p.BatNbr == header.BatNbr).ToList();
                foreach (OM10800_pgOrder_Result curRow in lstOrder.Created)
                {

                    var RowDB = _db.OM_ShipLine.FirstOrDefault(p => p.BranchID == header.BranchID && p.BatNbr == header.BatNbr && p.OrdNbr == curRow.OrderNbr);
                    if (curRow.Selected.Value)
                    {
                        if (RowDB != null)
                        {
                            if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                            {
                                Updating_OM_ApproveOrder(RowDB, curRow);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            RowDB = new OM_ShipLine();
                            RowDB.ResetET();
                            RowDB.BatNbr = header.BatNbr;
                            RowDB.BranchID = header.BranchID;
                            RowDB.OrdNbr = curRow.OrderNbr;
                            RowDB.Crtd_DateTime = DateTime.Now;
                            RowDB.Crtd_Prog = _screenNbr;
                            RowDB.Crtd_User = _userName;

                            Updating_OM_ApproveOrder(RowDB, curRow);
                            _db.OM_ShipLine.AddObject(RowDB);
                            lstRowDB.Add(RowDB);
                        }
                    }
                    else
                    {
                        if (RowDB != null)
                        {
                            _db.OM_ShipLine.DeleteObject(RowDB);
                            lstRowDB.Remove(RowDB);
                        }
                    }
                }
                #endregion

                #region Save Delivery


                lstDelivery.Created.AddRange(lstDelivery.Updated);

                foreach (OM10800_pgDelivery_Result curRow in lstDelivery.Created)
                {

                    var RowDB = _db.OM_ShipDelivery.FirstOrDefault(p => p.BranchID == header.BranchID && p.BatNbr == header.BatNbr && p.SlsperID == curRow.SlsPerID);
                    if (curRow.Selected.Value)
                    {
                        if (RowDB != null)
                        {
                            if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                            {
                                Updating_OM_ShipDelivery(RowDB, curRow);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            RowDB = new OM_ShipDelivery();
                            RowDB.ResetET();
                            RowDB.BatNbr = header.BatNbr;
                            RowDB.BranchID = header.BranchID;
                            RowDB.SlsperID = curRow.SlsPerID;
                            RowDB.Crtd_DateTime = DateTime.Now;
                            RowDB.Crtd_Prog = _screenNbr;
                            RowDB.Crtd_User = _userName;

                            Updating_OM_ShipDelivery(RowDB, curRow);
                            _db.OM_ShipDelivery.AddObject(RowDB);
                        }
                    }
                    else
                    {
                        if (RowDB != null)
                            _db.OM_ShipDelivery.DeleteObject(RowDB);
                    }
                }
                #endregion
                if (Handle.PassNull() != "" && Handle.PassNull() != "N")
                {
                    string lOrder = "";
                    foreach (var objr in lstRowDB)
                    {
                        lOrder += objr.OrdNbr + ",";
                    }

                    var obj = _db.OM10800_ppCheckChangeStatus(Current.UserName, Current.CpnyID, Current.LangID, header.BranchID, header.BatNbr,lOrder, Handle).FirstOrDefault();
                    if (obj.PassNull() == "")
                        header.Status = Handle;
                    else
                    {
                        throw new MessageException(MessageType.Message, "2013103001", "", new string[] { obj });
                       
                    }
                }
                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Save, header);

            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }


        private void Updating_Batch(ref Batch t, OM10800_pdHeader_Result s)
        {          
            t.Descr = s.Descr;         
            t.DateEnt = s.DateEnt;
            t.EditScrnNbr = _screenNbr;
            t.JrnlType = "OM";
            t.Rlsed = 0;
            t.Status =s.Status;
            t.RefNbr = s.RefNbr;//so xe

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
        private void Updating_OM_ApproveOrder(OM_ShipLine t, OM10800_pgOrder_Result s)
        {                
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Updating_OM_ShipDelivery(OM_ShipDelivery t, OM10800_pgDelivery_Result s)
        {

          

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }

    }
}

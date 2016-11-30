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
namespace CA10200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class CA10200Controller : Controller
    {
        private string _screenNbr = "CA10200";
        private string _userName = Current.UserName;

        CA10200Entities _db = Util.CreateObjectContext<CA10200Entities>(false);

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
        #region DataSource lấy source cho combo, lưới...
        public ActionResult GetHeader(string BranchID, string BatNbr)
        {
            var obj = _db.CA10200_pdHeader(Current.CpnyID, Current.UserName, Current.LangID, BranchID, BatNbr).FirstOrDefault();
            return this.Store(obj);
        }
        public ActionResult GetDetail(string BranchID, string BatNbr)
        {
            var lstobj = _db.CA10200_pgDetail(Current.CpnyID, Current.UserName, Current.LangID, BranchID, BatNbr).ToList();
            return this.Store(lstobj);
        }

        #endregion

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstHeader"]);
                var curHeader = dataHandler.ObjectData<CA10200_pdHeader_Result>().FirstOrDefault();
                var BranchID = data["cboBranchID"].PassNull().ToUpper().Trim();
                var BatNbr = data["cboBatNbr"].PassNull().ToUpper().Trim();
                var Handle = data["cboHandle"].PassNull().ToUpper().Trim();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstDetail"]);
                ChangeRecords<CA10200_pgDetail_Result> lstDetail = dataHandler1.BatchObjectData<CA10200_pgDetail_Result>();
                #region Save Header
                var header = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.Module == "CA");
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {

                    BatNbr = _db.CANumbering(BranchID, "BatNbr").FirstOrDefault();
                    if (BatNbr == null)
                    {
                        throw new MessageException(MessageType.Message, "20404",
                      parm: new[] { "CA_Setup" });
                    }
                    header = new Batch();
                    header.ResetET();
                    header.BranchID = BranchID;
                    header.BatNbr = BatNbr;                    
                    header.JrnlType = "CA";

                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;

                    UpdatingHeader(ref header, curHeader);
                    _db.Batches.AddObject(header);
                }
                #endregion

                #region Save Detail
                foreach (CA10200_pgDetail_Result deleted in lstDetail.Deleted)
                {
                    if (lstDetail.Created.Where(p => p.LineRef == deleted.LineRef.ToUpper()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstDetail.Created.Where(p => p.LineRef == deleted.LineRef.ToUpper()).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDelete = _db.CA_Trans.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.LineRef == deleted.LineRef.ToUpper());
                        if (objDelete != null)
                        {
                            _db.CA_Trans.DeleteObject(objDelete);
                        }
                    }
                }

                lstDetail.Created.AddRange(lstDetail.Updated);

                foreach (CA10200_pgDetail_Result curRow in lstDetail.Created)
                {
                    if (curRow.BankAcct.PassNull() == "") continue;
                  
                    var RowDB = _db.CA_Trans.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr && p.LineRef == curRow.LineRef.ToUpper());

                    if (RowDB != null)
                    {
                        if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                        {
                            UpdatingDetail(RowDB, curRow, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        RowDB = new CA_Trans();
                        RowDB.ResetET();
                        RowDB.BranchID = BranchID;
                        RowDB.BatNbr = header.BatNbr;
                        RowDB.RefNbr = header.RefNbr;
                        RowDB.LineRef = curRow.LineRef;
                        UpdatingDetail(RowDB, curRow, true);
                        RowDB.TranDate = curHeader.DateEnt;
                        RowDB.EntryID = "TR";

                        _db.CA_Trans.AddObject(RowDB);
                    }
                }
                #endregion

                _db.SaveChanges();
                if (Handle == "R")
                {
                    _db.CA_ReleaseBatch(header.BranchID, header.BatNbr, _screenNbr, Current.UserName);
                }
                else if (Handle == "V" || Handle == "C")
                {
                    _db.CA_CancelBatch(header.BranchID, header.BatNbr, _screenNbr, Current.UserName);
                    if (Handle == "C") //copy thành lô mới
                    {

                        BatNbr = _db.CANumbering(BranchID, "BatNbr").FirstOrDefault();
                        if (BatNbr == null)
                        {
                            throw new MessageException(MessageType.Message, "20404",
                          parm: new[] { "CA_Setup" });
                        }
                        var newheader = new Batch();
                        newheader.ResetET();
                        newheader.BranchID = BranchID;
                        newheader.BatNbr = BatNbr;
                        newheader.RefNbr = header.RefNbr;
                        newheader.Rlsed =0;
                        newheader.JrnlType = header.JrnlType;
                        newheader.RvdBatNbr = header.BatNbr;
                        newheader.ReasonCD = header.ReasonCD;
                        newheader.Descr = header.Descr;
                        newheader.TotAmt = header.TotAmt;
                        newheader.DateEnt = header.DateEnt;
                        newheader.EditScrnNbr = header.EditScrnNbr;
                        newheader.Module = header.Module;
                        newheader.Status = "H";
                        newheader.FromToSiteID = header.FromToSiteID;

                        newheader.Crtd_DateTime = DateTime.Now;
                        newheader.Crtd_Prog = _screenNbr;
                        newheader.Crtd_User = Current.UserName;
                        newheader.LUpd_DateTime = DateTime.Now;
                        newheader.LUpd_Prog = _screenNbr;
                        newheader.LUpd_User = _userName;
                        _db.Batches.AddObject(newheader);

                        var lstRowDB = _db.CA_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == header.BatNbr).ToList();
                        foreach (var obj in lstRowDB)
                        {
                            var objDetail = new CA_Trans();
                            objDetail.ResetET();

                            objDetail.BranchID = obj.BranchID;
                            objDetail.BatNbr = newheader.BatNbr;
                            objDetail.RefNbr = newheader.RefNbr;
                            objDetail.LineRef = obj.LineRef;
                            objDetail.TranAmt = obj.TranAmt;
                            objDetail.TranType = obj.TranType;
                            objDetail.BankAcct = obj.BankAcct;
                            objDetail.CustID = obj.CustID;
                            objDetail.EmployeeID = obj.EmployeeID;
                            objDetail.EntryID = obj.EntryID;
                            objDetail.Rlsed = 0;
                            objDetail.TranDate = obj.TranDate;
                            objDetail.TranDesc = obj.TranDesc;
                            objDetail.VendID = obj.VendID;
                            objDetail.VendName = obj.VendName;
                            objDetail.Addr = obj.Addr;
                            objDetail.InvcDate = obj.InvcDate;
                            objDetail.InvcNbr = obj.InvcNbr;
                            objDetail.InvcNote = obj.InvcNote;
                            objDetail.TaxRegNbr = obj.TaxRegNbr;
                            objDetail.TaxID = obj.TaxID;
                            objDetail.PayerReceiverAddr = obj.PayerReceiverAddr;
                            objDetail.PayerReceiver = obj.PayerReceiver;
                            objDetail.Transportation = obj.Transportation;

                            objDetail.TrsfToBankAcct = obj.TrsfToBankAcct;
                            objDetail.TrsfToBranchID = obj.TrsfToBranchID;


                            objDetail.Crtd_DateTime = DateTime.Now;
                            objDetail.Crtd_Prog = _screenNbr;
                            objDetail.Crtd_User = _userName;
                            objDetail.LUpd_DateTime = DateTime.Now;
                            objDetail.LUpd_Prog = _screenNbr;
                            objDetail.LUpd_User = _userName;

                            _db.CA_Trans.AddObject(objDetail);
                        }
                        _db.SaveChanges();

                    }

                }
                return Util.CreateMessage((Handle == "R" || Handle == "V" || Handle == "C") ? MessageProcess.Process : MessageProcess.Save, BatNbr);
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        //Update Header Company
        private void UpdatingHeader(ref Batch t, CA10200_pdHeader_Result s)
        {


            t.RvdBatNbr = s.RvdBatNbr;       
            t.TotAmt = s.TotAmt;
            t.DateEnt = s.DateEnt;
            t.Descr = s.Descr;
            t.EditScrnNbr = "CA10200";
            t.Module = "CA";
            t.JrnlType = t.JrnlType.Length == 0 ? "CA" : t.JrnlType;
            t.Status = s.Status;
           

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        #region Update Detail
        private void UpdatingDetail(CA_Trans t, CA10200_pgDetail_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.LineRef = s.LineRef;
            t.TranAmt = s.TranAmt;       
            t.BankAcct = s.BankAcct;                     
            t.TranDesc = s.TranDesc;
            t.TrsfToBankAcct = s.TrsfToBankAcct;
            t.TrsfToBranchID = s.TrsfToBranchID;
          
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
        #endregion

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                var BranchID = data["cboBranchID"].PassNull().ToUpper().Trim();
                var BatNbr = data["cboBatNbr"].PassNull().ToUpper().Trim();

                var cpny = _db.Batches.FirstOrDefault(p => p.BranchID == BranchID && p.BatNbr == BatNbr);
                if (cpny != null)
                {
                    _db.Batches.DeleteObject(cpny);
                }

                var lstAddr = _db.CA_Trans.Where(p => p.BranchID == BranchID && p.BatNbr == BatNbr).ToList();
                foreach (var item in lstAddr)
                {
                    _db.CA_Trans.DeleteObject(item);
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
    }
}

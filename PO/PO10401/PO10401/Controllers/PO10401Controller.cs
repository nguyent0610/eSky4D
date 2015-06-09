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
using PO10401.Models;
using System.Data.SqlClient;
namespace PO10401.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class PO10401Controller : Controller
    {
        private string _screenNbr = "PO10401";
        PO10401Entities _db = Util.CreateObjectContext<PO10401Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
      
      
        private List<PO10401_pgDetail_Result> _lstPODetailLoad = new List<PO10401_pgDetail_Result>();
    
        private PO10401_pdOM_UserDefault_Result objOM_UserDefault;

        private List<IN_ItemSite> lstInItemsiteNew = new List<IN_ItemSite>();

        List<PO10401_pdOM_DiscAllByBranchPO_Result> _lstPO10401_pdOM_DiscAllByBranchPO;
        List<PO10401_pdIN_UnitConversion_Result> _PO10401_pdIN_UnitConversion_Result;
        private List<PO10401_pgDetail_Result> _lstTmpPO10401_pgDetail;
        private bool _freeLineRunning = false;


        private const string ScreenNbr = "PO10401";
        private FormCollection _form;
        private List<PO10401_pgDetail_Result> _lstPOTrans = new List<PO10401_pgDetail_Result>();
        private List<PO10401_pgLotTrans_Result> _lstLot = new List<PO10401_pgLotTrans_Result>();
        private List<PO10401_pgLoadTaxTrans_Result> _lstTax = new List<PO10401_pgLoadTaxTrans_Result>();
        private PO10401_pdPO_Setup_Result _objPO_Setup;
        private PO10401_pdHeader_Result _poHead;
        private Batch _objBatch;
        string _batNbr = "";
        string _rcptNbr = "";
        string _branchID = "";
        string _handle = "";
        string _status = "";
        private JsonResult _logMessage;


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
        #region Get Data
        public ActionResult GetDet(string cpnyID, DateTime? fromDate, DateTime? todate, string status)
        {
            var dets = _db.PO10401_pgLoadPOHeader(Current.UserName, cpnyID, fromDate, todate, status, DateTime.Now.ToDateShort()).ToList();
            return this.Store(dets);
        }
       
        #endregion
        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data,bool isCancel)
        {
            try
            {
                var handle = data["cboHandleApprove"];
                var status = data["cboStatusApprove"];
                var imgHandler = new StoreDataHandler(data["lstDetChange"]);
                var lstDet = imgHandler.ObjectData<PO10401_pgLoadPOHeader_Result>().ToList();
                string message = "";
                var user = _sys.Users.FirstOrDefault(p => p.UserName.ToUpper() == Current.UserName.ToUpper());
                int count = lstDet.Where(p => p.Selected == true).ToList().Count();

                var objHandle = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == _screenNbr && p.Status == status && p.Handle == handle).FirstOrDefault();
                string lstBranchID = "";
                string lstBatNbr = "";
                string lstRcptNbr = "";
                var objConfig = _db.PO10401_ppApproveConfig(Current.UserName, Current.LangID, status).FirstOrDefault();

                if (objHandle == null) throw new MessageException(MessageType.Message, "20404", parm: new[] { Util.GetLang("SI_Handle") });
                foreach (var objDet in lstDet)
                {
                    lstBranchID += objDet.BranchID + ",";
                    lstBatNbr += objDet.BatNbr + ",";
                    lstRcptNbr += objDet.RcptNbr + ",";
                }
                #region co kiem tra ton kho
                if (objConfig.isCheckStock == true)//co kiem tra ton kho
                {
                    if (objConfig.isCheckAllOrder == true)
                    {
                        var objCheckApprove = _db.PO10401_ppCheckApprove(lstBranchID, lstBatNbr,lstRcptNbr, DateTime.Now.ToDateShort(), objHandle.ToStatus).ToList();
                        if (objCheckApprove.Count() > 0)//show thong bao khi co san pham khong du ton kho 
                        {
                            foreach (var obj in objCheckApprove)
                            {

                                message += string.Format("{0} {1} {2}<br/>", Util.GetLang("InvtID") + " " + obj.InvtID, Util.GetLang("QtyNotEnough") + " " + obj.Qty, Util.GetLang("QtyStock") + " " + obj.QtyAvail);
                            }
                            throw new MessageException("20410", parm: new[] { message });

                        }
                        else
                        {
                            foreach (var objDet in lstDet)
                            {
                                var header = _db.Batches.Where(p => p.Module == "IN" && p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr).FirstOrDefault();
                                if (header != null)
                                {
                                    if (objDet.tstamp.ToHex() != header.tstamp.ToHex())
                                    {
                                        throw new MessageException(MessageType.Message, "19");
                                    }
                                    header.Status = objHandle.ToStatus;

                                    header.LUpd_DateTime = DateTime.Now;
                                    header.LUpd_Prog = _screenNbr;
                                    header.LUpd_User = Current.UserName;

                                    if (isCancel)// cap nhat lai itemlot, itemsite
                                    {

                                        var objPO_Receipt = _db.PO_Receipt.FirstOrDefault(p => p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr && p.RcptNbr == objDet.RcptNbr);
                                        if (objPO_Receipt != null)
                                        {
                                            objPO_Receipt.Status = (handle != "N" && handle != "") ? objHandle.ToStatus : status;
                                        }

                                        ////cap nhat itemsite
                                        var lstpotran = _db.PO_Trans.Where(p => p.BranchID ==  objDet.BranchID && p.BatNbr == objDet.BatNbr && p.RcptNbr == objDet.RcptNbr).ToList();
                                        foreach (var obj in lstpotran)
                                        {

                                            double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                                            var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();

                                            objItemSite.QtyAllocPORet = Math.Round(objItemSite.QtyAllocPORet - NewQty, 0);
                                            objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + NewQty, 0);

                                            objItemSite.LUpd_DateTime = DateTime.Now;
                                            objItemSite.LUpd_Prog = ScreenNbr;
                                            objItemSite.LUpd_User = Current.UserName;

                                        }
                                        //// delete lot cu khong co tren luoi lot
                                        var lstold = _db.PO_LotTrans.Where(p => p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr && p.RefNbr == objDet.RcptNbr).ToList();
                                        foreach (var obj in lstold)
                                        {

                                            double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                                            var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr).FirstOrDefault();

                                            objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);

                                            objItemLot.LUpd_DateTime = DateTime.Now;
                                            objItemLot.LUpd_Prog = ScreenNbr;
                                            objItemLot.LUpd_User = Current.UserName;

                                        }

                                    }
                                }
                                else throw new MessageException(MessageType.Message, "19");
                            }
                            //cap nhat lai sapstock
                            _db.PO10401_ppUpdateStock(lstBranchID, lstBatNbr,lstRcptNbr, DateTime.Now.ToDateShort(), objHandle.ToStatus);
                            _db.SaveChanges();
                        }
                    }
                    else
                    {

                        foreach (var objDet in lstDet)
                        {
                            var objCheckApprove = _db.PO10401_ppCheckApprove(objDet.BranchID, objDet.BatNbr,objDet.RcptNbr, DateTime.Now.ToDateShort(), objHandle.ToStatus).ToList();
                            if (objCheckApprove.Count() > 0)
                            {
                                message += objDet.BatNbr + ",";
                            }
                            else
                            {
                                try
                                {
                                    var header = _db.Batches.Where(p => p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr).FirstOrDefault();
                                    if (header != null)
                                    {
                                        if (objDet.tstamp.ToHex() != header.tstamp.ToHex())
                                        {
                                            throw new MessageException(MessageType.Message, "19");
                                        }
                                        header.Status = objHandle.ToStatus;

                                        header.LUpd_DateTime = DateTime.Now;
                                        header.LUpd_Prog = _screenNbr;
                                        header.LUpd_User = Current.UserName;
                                        if (isCancel)// cap nhat lai itemlot, itemsite
                                        {

                                            var objPO_Receipt = _db.PO_Receipt.FirstOrDefault(p => p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr && p.RcptNbr == objDet.RcptNbr);
                                            if (objPO_Receipt != null)
                                            {
                                                objPO_Receipt.Status = (handle != "N" && handle != "") ? objHandle.ToStatus : status;
                                            }

                                            ////cap nhat itemsite
                                            var lstpotran = _db.PO_Trans.Where(p => p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr && p.RcptNbr == objDet.RcptNbr).ToList();
                                            foreach (var obj in lstpotran)
                                            {

                                                double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                                                var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();

                                                objItemSite.QtyAllocPORet = Math.Round(objItemSite.QtyAllocPORet - NewQty, 0);
                                                objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + NewQty, 0);

                                                objItemSite.LUpd_DateTime = DateTime.Now;
                                                objItemSite.LUpd_Prog = ScreenNbr;
                                                objItemSite.LUpd_User = Current.UserName;

                                            }
                                            //// delete lot cu khong co tren luoi lot
                                            var lstold = _db.PO_LotTrans.Where(p => p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr && p.RefNbr == objDet.RcptNbr).ToList();
                                            foreach (var obj in lstold)
                                            {

                                                double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                                                var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr).FirstOrDefault();

                                                objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                                                objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);

                                                objItemLot.LUpd_DateTime = DateTime.Now;
                                                objItemLot.LUpd_Prog = ScreenNbr;
                                                objItemLot.LUpd_User = Current.UserName;

                                            }

                                        }
                                    }
                                    else throw new MessageException(MessageType.Message, "19");
                                    //cap nhat lai sapstock
                                    _db.PO10401_ppUpdateStock(objDet.BranchID, objDet.BatNbr,objDet.RcptNbr, DateTime.Now.ToDateShort(), objHandle.ToStatus);
                                    _db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                        if (message != "")
                        {
                            message = string.Format("{0} {1}<br/>", Util.GetLang("BatNbr") + " " + message.TrimEnd(','), Util.GetLang("notenoughqty"));
                            Util.AppendLog(ref _logMessage, "20410", "", parm: new[] { message });
                            return _logMessage;
                        }

                    }
                }
                #endregion 
                else//khong kiem tra ton kho
                {
                    foreach (var objDet in lstDet)
                    {
                        var header = _db.Batches.Where(p => p.BranchID == objDet.BranchID && p.BatNbr == objDet.BatNbr).FirstOrDefault();
                        if (header != null)
                        {
                            if (objDet.tstamp.ToHex() != header.tstamp.ToHex())
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                            header.Status = objHandle.ToStatus;

                            header.LUpd_DateTime = DateTime.Now;
                            header.LUpd_Prog = _screenNbr;
                            header.LUpd_User = Current.UserName;
                        }
                        else throw new MessageException(MessageType.Message, "19");
                    }
                    //cap nhat lai sapstock
                    _db.PO10401_ppUpdateStock(lstBranchID, lstBatNbr,lstRcptNbr, DateTime.Now.ToDateShort(), objHandle.ToStatus);
                    _db.SaveChanges();
                }


                return Util.CreateMessage(MessageProcess.Process);
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

        private string GetMail(string listBranch, string listMail)
        {
            listMail = listMail == null ? string.Empty : listMail;
            listBranch = listBranch == null ? string.Empty : listBranch;
            string to = string.Empty;
            string[] branchs = listBranch.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] roles = listMail.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in roles)
            {
                if (role.ToUpper() == "DIST" || role.ToUpper() == "SUBDIST")
                {
                    foreach (var branch in branchs)
                    {
                        var company = (from p in _sys.SYS_Company where p.CpnyID == branch select p).FirstOrDefault();
                        if (company != null)
                            to += ';' + company.Email;
                    }
                }
                else if (role.ToUpper() == "SUP" || role.ToUpper() == "ADMIN")
                {
                    foreach (var branch in branchs)
                    {
                        var user = (from p in _sys.Users
                                    where p.UserTypes != null && p.UserTypes.Split(',').Any(c => c.ToUpper() == role.ToUpper())
                                        && p.CpnyID != null && p.CpnyID.Split(',').Any(c => c.ToUpper() == branch.ToUpper())
                                    select p).FirstOrDefault();
                        if (user != null)
                            to += ';' + user.Email;
                    }
                }
                else
                {
                    var users = (from p in _sys.Users where p.UserTypes != null && p.UserTypes.Split(',').Any(c => c.ToUpper() == role.ToUpper()) select p).ToList();
                    foreach (var user in users)
                    {
                        to += ';' + user.Email;
                    }
                }
            }
            return to;
        }

        //public void SubmitApprove(string proc, Dictionary<string, string> parameter, string to, string cc, string subject, string note)
        //{
        //    var lstParams = new List<SqlParameter>();

        //    string[] except = new string[] { "@Content", "@LangID", "@Roles", "@UserName", "@FromStatus" };
        //    foreach (var parm in parameter)
        //    {
        //        if (!except.Any(p => p.ToUpper() == parm.Key.ToUpper()))
        //        {
        //            lstParams.Add(new SqlParameter(parm.Key, parm.Value));
        //        }
        //    }

        //    // stored procedure PO10401_Approve
        //    var toStatus = _db.ExecuteStoreQuery<string>(proc, lstParams).FirstOrDefault();

        //    var contentProc = _db.PO10401_ApproveContent(parameter["@ScreenNbr"],
        //        parameter["@CustID"], parameter["@SlsPerID"],
        //        parameter["@BranchID"], parameter["@DisplayID"],
        //        parameter["@LevelID"], parameter["@Status"],
        //        toStatus, parameter["@Action"].ToShort(),
        //        parameter["@Handle"], parameter["@Roles"],
        //        parameter["@LangID"].ToShort(), parameter["@User"]);
        //    string content = string.Format("<html><body><p>{0}</p><p>{1}</p></body></html>", contentProc, note);
        //    SendMailApprove(to, cc, subject, content);
        //}

        //public void SendMailApprove(string mailTo, string mailCC, string subject, string content)
        //{
        //    try
        //    {
        //        if (mailTo == string.Empty && mailCC == string.Empty) return;

        //        var email = _db.HO_EmailConfig.Where(p => p.EmailID.ToUpper() == "Approve".ToUpper()).FirstOrDefault();
        //        if (email != null)
        //            Approve.SendMail(mailTo, mailCC, subject, content);
        //        else
        //            throw new Exception("No email config");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #region Popup
        #region Get Data
        public ActionResult GetHeader(string batNbr, string branchID)
        {
            var obj = _db.PO10401_pdHeader(branchID, batNbr).FirstOrDefault();
            return this.Store(obj);

        }
        public ActionResult GetAP_VendorTax(string vendID, string ordFromId)
        {

            return this.Store(_db.PO10401_pdAP_VenDorTaxes(vendID, ordFromId));

        }
        public ActionResult GetPO10401_pgDetail(string rcptNbr, string batNbr, string branchID)
        {
            var lst = _db.PO10401_pgDetail(branchID, batNbr, rcptNbr).ToList();
            return this.Store(lst);

        }
        public ActionResult GetPO10401_pgLoadTaxTrans(string rcptNbr, string batNbr, string branchID)
        {
            return this.Store(_db.PO10401_pgLoadTaxTrans(branchID, batNbr, rcptNbr).ToList());
        }
        public ActionResult GetPO10401_ppCheckingPONbr(string branchID, string poNbr)
        {
            var obj = _db.PO10401_ppCheckingPONbr(branchID, poNbr).FirstOrDefault();
            return this.Store(obj);

        }
       
        public ActionResult GetLotTrans(string rcptNbr, string batNbr, string branchID, string type, string poNbr)
        {
            var lst = _db.PO10401_pgLotTrans(branchID, batNbr, rcptNbr, type, poNbr).ToList();
            return this.Store(lst);
        }

        #endregion
        //#region DataProcess 
        public ActionResult Save(FormCollection data,bool isCancel)
        {
            try
            {

                var acc = Session["PO10401"] as AccessRight;
                _form = data;
                _batNbr = data["txtBatNbr"];
                _rcptNbr = data["RcptNbr"];
                _branchID = data["txtBranchID"];
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();             
                _objPO_Setup = _db.PO10401_pdPO_Setup(_branchID, "PO").FirstOrDefault();

                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO10401_pdHeader_Result>().FirstOrDefault();

                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPOTrans = detHandler.ObjectData<PO10401_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

                var detHandlerLot = new StoreDataHandler(data["lstLot"]);
                _lstLot = detHandlerLot.ObjectData<PO10401_pgLotTrans_Result>()
                            .Where(p => Util.PassNull(p.LotSerNbr) != string.Empty)
                            .ToList();

                if (isCancel)// cap nhat lai itemlot, itemsite
                {
                    _objBatch = _db.Batches.FirstOrDefault(p => p.Module == "IN" && p.BatNbr == _batNbr && p.BranchID == _branchID);
                    if (_objBatch != null)
                    {
                        if (_objBatch.tstamp.ToHex() != _poHead.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        _objBatch.Status = (_handle != "N" && _handle != "") ? _handle : _poHead.Status;

                    }

                    _batNbr = _objBatch.BatNbr;
                  
                    var objPO_Receipt = _db.PO_Receipt.FirstOrDefault(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr);
                    if (objPO_Receipt != null)
                    {
                        objPO_Receipt.Status = (_handle != "N" && _handle != "") ? _handle : _poHead.Status;
                    }

                    _rcptNbr = objPO_Receipt.RcptNbr;
                    ////cap nhat itemsite
                    var lstpotran = _db.PO_Trans.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr).ToList();
                    foreach (var obj in lstpotran)
                    {
                        if (_poHead.RcptType == "X")
                        {
                            double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                            var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();

                            objItemSite.QtyAllocPORet = Math.Round(objItemSite.QtyAllocPORet - NewQty, 0);
                            objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + NewQty, 0);

                            objItemSite.LUpd_DateTime = DateTime.Now;
                            objItemSite.LUpd_Prog = ScreenNbr;
                            objItemSite.LUpd_User = Current.UserName;
                        }
                    }
                    //// delete lot cu khong co tren luoi lot
                    var lstold = _db.PO_LotTrans.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _rcptNbr).ToList();
                    foreach (var obj in lstold)
                    {                      
                        if (_poHead.RcptType == "X")
                        {
                            double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                            var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr).FirstOrDefault();

                            objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = ScreenNbr;
                            objItemLot.LUpd_User = Current.UserName;
                        }                        
                    }
                    _db.SaveChanges();
                }
                else if (Data_Checking())
                {
                    if ((_status == "U" || _status == "C" || _status == "H") && (_handle == "C" || _handle == "V" || _handle == "R"))
                    {

                        if (_handle == "R" || _handle == "V" || _handle == "C")
                        {
                            if (_handle == "R" && !acc.Release)
                            {
                                throw new MessageException(MessageType.Message, "737");
                            }
                            else if ((_handle == "V" || _handle == "C") && !acc.Release)
                            {
                                throw new MessageException(MessageType.Message, "725");
                            }
                            else
                            {
                                if (_handle == "V" || _handle == "C")
                                {
                                    Data_Release();
                                }
                                else if (_handle == "R") Save_Batch();
                            }
                        }
                    }
                    else Save_Batch();
                }
                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save, new { batNbr = _batNbr });

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
        public ActionResult DeleteHeader(FormCollection data)
        {
            try
            {
                var acc = Session["PO10401"] as AccessRight;
                _form = data;
                _batNbr = data["txtBatNbr"];
                _rcptNbr = data["RcptNbr"];
                _branchID = data["txtBranchID"];
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();

                _objPO_Setup = _db.PO10401_pdPO_Setup(_branchID, "PO").FirstOrDefault();


                var detHeader = new StoreDataHandler(data["lstHeader"]);

                if (_poHead == null)
                    _poHead = detHeader.ObjectData<PO10401_pdHeader_Result>().FirstOrDefault();
                var objHeader = _db.Batches.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr).FirstOrDefault();
                if (objHeader != null)
                {
                    if (_poHead.tstamp.ToHex() != objHeader.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    _db.Batches.DeleteObject(objHeader);
                    var objRe = _db.PO_Receipt.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr).FirstOrDefault();
                    if (objRe != null)
                    {
                        _db.PO_Receipt.DeleteObject(objRe);
                    }

                    var objInvoice = _db.PO_Invoice.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr).FirstOrDefault();
                    if (objInvoice != null)
                    {
                        _db.PO_Invoice.DeleteObject(objInvoice);
                    }

                    var lstdel = _db.PO_Trans.Where(p => p.BatNbr == _batNbr && p.BranchID == _branchID && p.RcptNbr == _rcptNbr).ToList();
                    while (lstdel.FirstOrDefault() != null)
                    {
                        var obj = lstdel.FirstOrDefault();
                        if (obj != null && _poHead.RcptType == "X")
                        {
                            var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();
                            double dblQty = (obj.RcptMultDiv == "D" ? (obj.RcptQty / obj.RcptConvFact) : obj.RcptQty * obj.RcptConvFact);
                            //Clear old alloc
                            objItemSite.QtyAllocPORet = Math.Round(objItemSite.QtyAllocPORet - dblQty, 0);
                            objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + dblQty, 0);
                            objItemSite.LUpd_DateTime = DateTime.Now;
                            objItemSite.LUpd_Prog = ScreenNbr;
                            objItemSite.LUpd_User = Current.UserName;

                            // delete lot
                            var lstold = _db.PO_LotTrans.Where(p => p.BranchID == obj.BranchID && p.BatNbr == obj.BatNbr && p.RefNbr == obj.RcptNbr && p.POTranLineRef == obj.LineRef).ToList();
                            foreach (var objlot in lstold)
                            {
                                _db.PO_LotTrans.DeleteObject(objlot);
                                double NewQty = (objlot.UnitMultDiv == "D" ? (objlot.Qty / objlot.CnvFact) : (objlot.Qty * obj.CnvFact));
                                var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == objlot.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == objlot.LotSerNbr).FirstOrDefault();

                                objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                                objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);

                                objItemLot.LUpd_DateTime = DateTime.Now;
                                objItemLot.LUpd_Prog = ScreenNbr;
                                objItemLot.LUpd_User = Current.UserName;


                            }
                        }
                        _db.PO_Trans.DeleteObject(obj);
                        lstdel.Remove(obj);
                    }
                    _db.SaveChanges();
                }
                return Util.CreateMessage(MessageProcess.Delete, new { batNbr = "" });

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
        public ActionResult DeleteGrd(FormCollection data)
        {
            try
            {
                var acc = Session["PO10401"] as AccessRight;
                _form = data;
                _batNbr = data["txtBatNbr"];
                _rcptNbr = data["RcptNbr"];
                _branchID = data["txtBranchID"];
                _status = data["Status"].PassNull();
                _handle = data["Handle"].PassNull() == "" ? _status : data["Handle"].PassNull();


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO10401_pdHeader_Result>().FirstOrDefault();


                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPOTrans = detHandler.ObjectData<PO10401_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstDel"]);
                ChangeRecords<PO10401_pgDetail_Result> lst = dataHandler.BatchObjectData<PO10401_pgDetail_Result>();

                if (_poHead == null)
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                else
                {
                    foreach (PO10401_pgDetail_Result deleted in lst.Deleted.Where(p => p.tstamp != ""))
                    {
                        var obj = _db.PO_Trans.Where(p => p.BranchID == deleted.BranchID && p.BatNbr == deleted.BatNbr && p.RcptNbr == deleted.RcptNbr && p.LineRef == deleted.LineRef).FirstOrDefault();
                        if (obj != null && _poHead.RcptType == "X")
                        {
                            var objItemSite = _db.IN_ItemSite.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID).FirstOrDefault();
                            double dblQty = (obj.RcptMultDiv == "D" ? (obj.RcptQty / obj.RcptConvFact) : obj.RcptQty * obj.RcptConvFact);
                            //Clear old alloc
                            objItemSite.QtyAllocPORet = Math.Round(objItemSite.QtyAllocPORet - dblQty, 0);
                            objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + dblQty, 0);
                            objItemSite.LUpd_DateTime = DateTime.Now;
                            objItemSite.LUpd_Prog = ScreenNbr;
                            objItemSite.LUpd_User = Current.UserName;
                        }
                        _db.PO_Trans.DeleteObject(obj);
                    }
                    Save_Batch(true);
                }
                return Util.CreateMessage(MessageProcess.Delete, new { batNbr = _batNbr });


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
        //get price
        [DirectMethod]
        public ActionResult PO10401POPrice(string branchID = "", string invtID = "", string Unit = "", DateTime? podate = null)
        {
            var result = _db.PO10401_ppGetPrice(branchID, invtID, Unit, podate).FirstOrDefault().Value;
            return this.Direct(result);

        }
        [DirectMethod]
        public ActionResult PO10401ItemSitePrice(string branchID = "", string invtID = "", string siteID = "")
        {
            var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == invtID && p.SiteID == siteID).FirstOrDefault();
            if (objIN_ItemSite == null)
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.ResetET();
            }
            return this.Direct(objIN_ItemSite);

        }
        [DirectMethod]
        public ActionResult PO10401ItemSiteQty(string branchID = "", string invtID = "", string siteID = "", string batNbr = "", string rcptNbr = "", string lineRef = "")
        {
            var objold = _db.PO_Trans.Where(p => p.BranchID == branchID && p.BatNbr == batNbr && p.RcptNbr == rcptNbr && p.InvtID == invtID && p.SiteID == siteID && p.LineRef == lineRef).FirstOrDefault();
            var qtyold = objold == null ? 0 : objold.UnitMultDiv == "M" ? objold.Qty * objold.CnvFact : objold.Qty / objold.CnvFact;

            var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == invtID && p.SiteID == siteID).FirstOrDefault();
            if (objIN_ItemSite == null)
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.ResetET();
            }
            objIN_ItemSite.QtyAvail = objIN_ItemSite.QtyAvail + qtyold;
            return this.Direct(objIN_ItemSite);

        }


      
        private void Save_Batch(bool isDeleteGrd = false)
        {

            _objBatch = _db.Batches.FirstOrDefault(p => p.Module == "IN" && p.BatNbr == _batNbr && p.BranchID == _branchID);
            if (_objBatch != null)
            {
                if (_objBatch.tstamp.ToHex() != _poHead.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                Updating_Batch(ref _objBatch);

            }
           
            _batNbr = _objBatch.BatNbr;
            SavePO_Receipt(_objBatch);
        }
        private void SavePO_Receipt(Batch objBatch)
        {
            var objPO_Receipt = _db.PO_Receipt.FirstOrDefault(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == _rcptNbr);
            if (objPO_Receipt != null)
            {
                Updating_PO_Receipt(ref objPO_Receipt);
            }
           
            _rcptNbr = objPO_Receipt.RcptNbr;
            SavePO_INVoice(objPO_Receipt);
        }
        private void SavePO_INVoice(PO_Receipt objPO_Receipt)
        {


            var objPO_Invoice = _db.PO_Invoice.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RcptNbr == objPO_Receipt.RcptNbr).FirstOrDefault();
            if (objPO_Invoice != null)
            {

                Updating_PO_Invoice(ref objPO_Invoice);
                SavePO_Trans(objPO_Receipt);

            }
            else
            {
                objPO_Invoice = new PO_Invoice();
                objPO_Invoice.ResetET();
                Updating_PO_Invoice(ref objPO_Invoice);
                objPO_Invoice.BatNbr = objPO_Receipt.BatNbr;
                objPO_Invoice.RcptNbr = objPO_Receipt.RcptNbr;
                objPO_Invoice.BranchID = objPO_Receipt.BranchID;
                objPO_Invoice.Crtd_Datetime = DateTime.Now;
                objPO_Invoice.Crtd_Prog = ScreenNbr;
                objPO_Invoice.Crtd_User = Current.UserName;
                objPO_Invoice.tstamp = new byte[0];
                _db.PO_Invoice.AddObject(objPO_Invoice);
                SavePO_Trans(objPO_Receipt);


            }

        }
        private void SavePO_Trans(PO_Receipt objPO_Receipt)
        {


            for (int i = 0; i < _lstPOTrans.Count; i++)
            {
                var objPOT = _lstPOTrans[i];
                var obj = _db.PO_Trans.Where(p => p.BranchID == objPO_Receipt.BranchID && p.BatNbr == objPO_Receipt.BatNbr && p.RcptNbr == objPO_Receipt.RcptNbr && p.LineRef == objPOT.LineRef).FirstOrDefault();
                if (obj != null)
                {
                    if (obj.tstamp.ToHex() != objPOT.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    Updating_PO_Trans(_lstPOTrans[i], ref obj);

                }
                else
                {
                    obj = new PO_Trans();
                    obj.ResetET();
                    Updating_PO_Trans(_lstPOTrans[i], ref obj);
                    obj.BranchID = objPO_Receipt.BranchID;
                    obj.BatNbr = objPO_Receipt.BatNbr;
                    obj.RcptNbr = objPO_Receipt.RcptNbr;
                    obj.LineRef = objPOT.LineRef;
                    obj.Crtd_DateTime = DateTime.Now;
                    obj.Crtd_Prog = ScreenNbr;
                    obj.Crtd_User = Current.UserName;
                    obj.tstamp = new byte[0];
                    _db.PO_Trans.AddObject(obj);
                }

            }
            Save_PO_LotTrans();

        }
        private void Save_PO_LotTrans()
        {
            try
            {
                //// delete lot cu khong co tren luoi lot
                var lstold = _db.PO_LotTrans.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _rcptNbr).ToList();
                foreach (var obj in lstold)
                {
                    if (_lstLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr && p.POTranLineRef == obj.POTranLineRef).FirstOrDefault() == null)
                    {
                        _db.PO_LotTrans.DeleteObject(obj);
                        if (_poHead.RcptType == "X")
                        {
                            double NewQty = (obj.UnitMultDiv == "D" ? (obj.Qty / obj.CnvFact) : (obj.Qty * obj.CnvFact));
                            var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr).FirstOrDefault();

                            objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet - NewQty, 0);
                            objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + NewQty, 0);

                            objItemLot.LUpd_DateTime = DateTime.Now;
                            objItemLot.LUpd_Prog = ScreenNbr;
                            objItemLot.LUpd_User = Current.UserName;
                        }
                    }
                }


                //Save Lot/Serial from datatable to in_lottrans

                foreach (var row in _lstLot)
                {
                    double oldQty = 0;
                    var obj = lstold.Where(p => p.BranchID == _branchID && p.BatNbr == _batNbr && p.RefNbr == _rcptNbr && p.InvtID == row.InvtID && p.LotSerNbr == row.LotSerNbr && p.SiteID == row.SiteID).FirstOrDefault();
                    if (obj == null)
                    {
                        obj = new PO_LotTrans();
                        obj.ResetET();
                        Update_PO_LotTrans(row, obj);
                        obj.Crtd_Prog = ScreenNbr;
                        obj.Crtd_User = Current.UserName;
                        obj.Crtd_DateTime = DateTime.Now;
                        _db.PO_LotTrans.AddObject(obj);
                    }
                    else
                    {
                        oldQty = obj == null ? 0 : obj.UnitMultDiv == "M" ? obj.Qty * obj.CnvFact : obj.Qty / obj.CnvFact;
                        Update_PO_LotTrans(row, obj);
                    }
                    //Update Location and Site Qty
                    if (_poHead.RcptType == "X")
                    {

                        var qty = obj.UnitMultDiv == "M" ? obj.Qty * obj.CnvFact : obj.Qty / obj.CnvFact;
                        var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == obj.InvtID && p.SiteID == obj.SiteID && p.LotSerNbr == obj.LotSerNbr).FirstOrDefault();

                        objItemLot.QtyAllocPORet = Math.Round(objItemLot.QtyAllocPORet + qty - oldQty, 0);
                        objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail - qty + oldQty, 0);

                        objItemLot.LUpd_DateTime = DateTime.Now;
                        objItemLot.LUpd_Prog = ScreenNbr;
                        objItemLot.LUpd_User = Current.UserName;
                        if (objItemLot.QtyAvail < 0)
                            throw new MessageException(MessageType.Message, "35");
                    }
                }
                _db.SaveChanges();
                if (_handle == "R")
                {
                    Data_Release();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private void Updating_Batch(ref Batch objBatch)
        {

            try
            {

                objBatch.TotAmt = _poHead.TotAmt;
                objBatch.DateEnt = DateTime.Now.ToDateShort();              
                objBatch.Descr = "PO Receipt";
                objBatch.Module = "IN";
                objBatch.JrnlType = "PO";
                objBatch.Rlsed = 0;
                objBatch.Status = (_handle != "N" && _handle != "") ? _handle : _poHead.Status;

                objBatch.LUpd_DateTime = DateTime.Now;
                objBatch.LUpd_Prog = ScreenNbr;
                objBatch.LUpd_User = Current.UserName;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void Updating_PO_Receipt(ref PO_Receipt objR)
        {
            try
            {

                objR.RcptFeeTot = _poHead.RcptFeeTot;
                objR.RcptTot = _poHead.RcptTot;
                objR.DiscAmt = _poHead.DiscAmt;
                objR.DiscAmtPct = _poHead.DiscAmtPct;
                objR.RcptTotAmt = _poHead.RcptTotAmt;


                objR.TaxAmtTot00 = _poHead.TaxAmtTot00;
                objR.TxblAmtTot00 = _poHead.TxblAmtTot00;
                objR.TaxID00 = _poHead.TaxID00;

                objR.TaxAmtTot01 = _poHead.TaxAmtTot01;
                objR.TxblAmtTot01 = _poHead.TxblAmtTot01;
                objR.TaxID01 = _poHead.TaxID01;

                objR.TaxAmtTot02 = _poHead.TaxAmtTot02;
                objR.TxblAmtTot02 = _poHead.TxblAmtTot02;
                objR.TaxID02 = _poHead.TaxID02;

                objR.TaxAmtTot03 = _poHead.TaxAmtTot03;
                objR.TxblAmtTot03 = _poHead.TxblAmtTot03;
                objR.TaxID03 = _poHead.TaxID03;

                objR.Descr = _poHead.Descr;
                //objR.NoteID = this.ReceiptNoteID;
                objR.PONbr = _poHead.PONbr;
                objR.RcptDate = _poHead.RcptDate.ToDateShort();
                objR.RcptType = _poHead.RcptType;
                objR.RcptFrom = _poHead.RcptFrom;
                objR.RcptQtyTot = _poHead.RcptQtyTot;
                objR.VendID = _poHead.VendID;
                objR.Status = (_handle != "N" && _handle != "") ? _handle : _poHead.Status;



                objR.LUpd_DateTime = DateTime.Now;
                objR.LUpd_Prog = ScreenNbr;
                objR.LUpd_User = Current.UserName;
                objR.tstamp = new byte[0];

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void Updating_PO_Invoice(ref PO_Invoice objI)
        {

            try
            {
                objI.InvcNbr = _poHead.InvcNbr;
                objI.InvcNote = _poHead.InvcNote;
                objI.InvcDate = _poHead.InvcDate.ToDateShort();
                objI.DocType = _poHead.DocType;
                objI.DocDate = _poHead.DocDate.ToDateShort();
                objI.APBatNbr = _poHead.APBatNbr;
                objI.APRefNbr = _poHead.APRefNbr;
                objI.Terms = _poHead.Terms;
                objI.VendID = _poHead.VendID;

                objI.LUpd_Datetime = DateTime.Now;
                objI.LUpd_Prog = ScreenNbr;
                objI.LUpd_User = Current.UserName;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void Updating_PO_Trans(PO10401_pgDetail_Result objr, ref PO_Trans objPO_Tr)
        {
            try
            {
                objr.PurchaseType = objr.PurchaseType;
                if (objr.PurchaseType == "GI" || objr.PurchaseType == "PR" || objr.PurchaseType == "GP" || objr.PurchaseType == "GS")
                {
                    var objIN_Inventory = _db.PO10401_pdIN_Inventory(Current.UserName).Where(p => p.InvtID == objr.InvtID).FirstOrDefault();
                    var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == objr.InvtID && p.SiteID == objr.SiteID).FirstOrDefault();
                    //Kiem tra itemsite neu chua co thi add vao
                    if (objIN_ItemSite == null && lstInItemsiteNew.Where(p => p.InvtID == objr.InvtID && p.SiteID == objr.SiteID).Count() == 0)
                    {
                        Insert_IN_ItemSite(ref objIN_ItemSite, ref objIN_Inventory, objr.SiteID);
                    }
                    //Update Location and Site Qty
                    if (_poHead.RcptType == "X")
                    {
                        double OldQty = 0;
                        double NewQty = 0;
                        NewQty = (objr.RcptMultDiv == "D" ? (objr.RcptQty / objr.RcptConvFact) : (objr.RcptQty * objr.RcptConvFact));
                        OldQty = (objr.RcptMultDiv == "D" ? (objPO_Tr.RcptQty / objPO_Tr.RcptConvFact) : objPO_Tr.RcptQty * objPO_Tr.RcptConvFact);

                        //objIN_ItemSite =_db.IN_ItemSite.Where(p => p.InvtID == objr.InvtID && p.SiteID == objr.SiteID).FirstOrDefault();

                        if (objIN_ItemSite != null)
                        {
                            ////Clear old alloc
                            //objIN_ItemSite.QtyAllocPORet = Math.Round(objIN_ItemSite.QtyAllocPORet - OldQty, 0);
                            //objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail + OldQty, 0);


                            ////Update new alloc
                            //objIN_ItemSite.QtyAllocPORet = Math.Round(objIN_ItemSite.QtyAllocPORet + NewQty, 0);
                            //objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail - NewQty, 0);


                            objIN_ItemSite.QtyAllocPORet = Math.Round(objIN_ItemSite.QtyAllocPORet - OldQty + NewQty, 0);
                            objIN_ItemSite.QtyAvail = Math.Round(objIN_ItemSite.QtyAvail + OldQty - NewQty, 0);

                            objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                            objIN_ItemSite.LUpd_Prog = ScreenNbr;
                            objIN_ItemSite.LUpd_User = Current.UserName;

                            if (objIN_ItemSite.QtyAvail < 0)
                                throw new MessageException(MessageType.Message, "35");
                        }
                    }
                }



                objPO_Tr.CnvFact = objr.CnvFact;
                if (objr.CostID.Length == 0)
                {
                    objPO_Tr.CostID = _poHead.InvcNbr + "_" + _poHead.RcptDate.ToString("yyyyMMdd");
                }
                else
                {
                    objPO_Tr.CostID = objr.CostID;
                }

                objPO_Tr.CostVouched = objr.CostVouched;
                objPO_Tr.UnitCost = objr.UnitCost;
                objPO_Tr.RcptFee = objr.RcptFee;

                objPO_Tr.DocDiscAmt = objr.DocDiscAmt;
                objPO_Tr.DiscPct = objr.DiscPct;
                objPO_Tr.ExtVolume = objr.ExtVolume;
                objPO_Tr.ExtWeight = objr.ExtWeight;
                objPO_Tr.InvtID = objr.InvtID;
                objPO_Tr.JrnlType = string.IsNullOrEmpty(objr.PONbr) ? "PO" : objr.JrnlType;
                objPO_Tr.OrigRcptDate = _poHead.RcptDate.ToDateShort();
                objPO_Tr.OrigRcptNbr = objr.OrigRcptNbr;
                objPO_Tr.OrigRetRcptNbr = objr.OrigRetRcptNbr;
                objPO_Tr.POLineRef = objr.POLineRef;
                objPO_Tr.PONbr = objr.PONbr;
                objPO_Tr.POOriginal = objr.POOriginal;
                objPO_Tr.PurchaseType = objr.PurchaseType;
                objPO_Tr.RcptConvFact = objr.RcptConvFact == 0 ? 1 : objr.RcptConvFact;
                objPO_Tr.UnitMultDiv = objr.UnitMultDiv;
                objPO_Tr.CnvFact = objr.CnvFact;
                objPO_Tr.Qty = objr.Qty;
                if (string.IsNullOrEmpty(objr.PONbr))
                {
                    if (objr.UnitMultDiv == "M")
                    {
                        objPO_Tr.Qty = objr.RcptMultDiv == "M" ? objr.RcptConvFact * objr.RcptQty / objPO_Tr.CnvFact : (objr.RcptQty / objr.RcptConvFact) / objPO_Tr.CnvFact;
                    }
                    else
                    {
                        objPO_Tr.Qty = objr.RcptMultDiv == "M" ? objr.RcptConvFact * objr.RcptQty * objPO_Tr.CnvFact : objr.RcptQty / objr.RcptConvFact * objPO_Tr.CnvFact;
                    }
                }
                objPO_Tr.QtyVouched = objr.QtyVouched;

                objPO_Tr.RcptDate = _poHead.RcptDate.ToDateShort();
                objPO_Tr.RcptMultDiv = objr.RcptMultDiv;

                objPO_Tr.RcptQty = objr.RcptQty;
                objPO_Tr.RcptUnitDescr = objr.RcptUnitDescr;
                objPO_Tr.ReasonCD = objr.ReasonCD;
                objPO_Tr.SiteID = objr.SiteID;
                objPO_Tr.TaxCat = objr.TaxCat;
                objPO_Tr.TaxID00 = objr.TaxID00;
                objPO_Tr.TaxID01 = objr.TaxID01;
                objPO_Tr.TaxID02 = objr.TaxID02;
                objPO_Tr.TaxID03 = objr.TaxID03;

                objPO_Tr.TaxAmt00 = objr.TaxAmt00;
                objPO_Tr.TaxAmt01 = objr.TaxAmt01;
                objPO_Tr.TaxAmt02 = objr.TaxAmt02;
                objPO_Tr.TaxAmt03 = objr.TaxAmt03;

                objPO_Tr.TxblAmt00 = objr.TxblAmt00;
                objPO_Tr.TxblAmt01 = objr.TxblAmt01;
                objPO_Tr.TxblAmt02 = objr.TxblAmt02;
                objPO_Tr.TxblAmt03 = objr.TxblAmt03;


                objPO_Tr.TranDate = _poHead.RcptDate.ToDateShort();
                objPO_Tr.TranDesc = objr.TranDesc;
                objPO_Tr.TranType = _poHead.RcptType;
                objPO_Tr.UnitDescr = objr.UnitDescr;

                objPO_Tr.UnitVolume = objr.UnitVolume;
                objPO_Tr.UnitWeight = objr.UnitWeight;
                objPO_Tr.VendID = _poHead.VendID;
                objPO_Tr.VouchStage = objr.VouchStage;
                objPO_Tr.TranAmt = objr.TranAmt;

                objPO_Tr.LUpd_DateTime = DateTime.Now;
                objPO_Tr.LUpd_Prog = ScreenNbr;
                objPO_Tr.LUpd_User = Current.UserName;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void Update_PO_LotTrans(PO10401_pgLotTrans_Result row, PO_LotTrans objPO_LotTrans)
        {
            objPO_LotTrans.MfcDate = row.MfcDate;
            objPO_LotTrans.BranchID = _branchID;
            objPO_LotTrans.BatNbr = _batNbr;
            objPO_LotTrans.RefNbr = _rcptNbr;
            //row.RefNbr")
            objPO_LotTrans.LotSerNbr = row.LotSerNbr;
            objPO_LotTrans.ExpDate = row.ExpDate;
            objPO_LotTrans.POTranLineRef = row.POTranLineRef;
            objPO_LotTrans.InvtID = row.InvtID;
            objPO_LotTrans.InvtMult = row.InvtMult;
            objPO_LotTrans.KitID = row.KitID;
            objPO_LotTrans.MfgrLotSerNbr = row.MfgrLotSerNbr;
            objPO_LotTrans.Qty = row.Qty;
            objPO_LotTrans.SiteID = row.SiteID;
            objPO_LotTrans.WhseLoc = row.WhseLoc;
            objPO_LotTrans.CnvFact = row.CnvFact;
            objPO_LotTrans.ToSiteID = row.ToSiteID;
            objPO_LotTrans.TranDate = _poHead.RcptDate;
            objPO_LotTrans.TranType = _poHead.RcptType;
            objPO_LotTrans.UnitCost = row.UnitCost;
            objPO_LotTrans.UnitPrice = row.UnitPrice;
            objPO_LotTrans.WarrantyDate = row.WarrantyDate;
            objPO_LotTrans.UnitMultDiv = row.UnitMultDiv;
            objPO_LotTrans.UnitDesc = row.UnitDesc;


            objPO_LotTrans.LUpd_Prog = ScreenNbr;
            objPO_LotTrans.LUpd_User = Current.UserName;
            objPO_LotTrans.LUpd_DateTime = DateTime.Now;
        }




        public void Insert_IN_ItemSite(ref IN_ItemSite objIN_ItemSite, ref PO10401_pdIN_Inventory_Result objIN_Inventory, string SiteID)
        {
            try
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.InvtID = objIN_Inventory.InvtID;
                objIN_ItemSite.SiteID = SiteID;
                objIN_ItemSite.AvgCost = 0;
                objIN_ItemSite.QtyAlloc = 0;
                objIN_ItemSite.QtyAllocIN = 0;
                objIN_ItemSite.QtyAllocPORet = 0;
                objIN_ItemSite.QtyAllocSO = 0;
                objIN_ItemSite.QtyAvail = 0;
                objIN_ItemSite.QtyInTransit = 0;
                objIN_ItemSite.QtyOnBO = 0;
                objIN_ItemSite.QtyOnHand = 0;
                objIN_ItemSite.QtyOnPO = 0;
                objIN_ItemSite.QtyOnTransferOrders = 0;
                objIN_ItemSite.QtyOnSO = 0;
                objIN_ItemSite.QtyShipNotInv = 0;
                objIN_ItemSite.StkItem = objIN_Inventory.StkItem;
                objIN_ItemSite.TotCost = 0;
                objIN_ItemSite.LastPurchaseDate = DateTime.Now;

                objIN_ItemSite.Crtd_DateTime = DateTime.Now;
                objIN_ItemSite.Crtd_Prog = ScreenNbr;
                objIN_ItemSite.Crtd_User = Current.UserName;

                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                objIN_ItemSite.LUpd_Prog = ScreenNbr;
                objIN_ItemSite.LUpd_User = Current.UserName;
                objIN_ItemSite.tstamp = new byte[0];

                _db.IN_ItemSite.AddObject(objIN_ItemSite);
                lstInItemsiteNew.Add(objIN_ItemSite);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //private void SendMail(PO_Header objHeader)
        //{
        //    try
        //    {
        //        HQSendMailApprove.Approve.SendMailApprove(objHeader.BranchID, objHeader.PONbr, ScreenNbr, Current.CpnyID, _status, _handle, Current.UserName, Current.LangID);
        //    }
        //    catch
        //    {
        //    }
        //}
        private bool Data_Checking(bool isDeleteGrd = false)
        {
           
                if (_objPO_Setup == null)
                {
                    throw new MessageException(MessageType.Message, "20404",
                      parm: new[] { "PO_Setup" });

                }

                if (_poHead.VendID.PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("VendID") });
                }

                if (_poHead.RcptFrom.PassNull() == "PO" && _poHead.PONbr.PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("PONbr") });
                }
                if (_poHead.RcptFrom.PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("RcptFrom") });
                }
                if (_poHead.RcptType.PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("RcptType") });
                }
                if (_poHead.DocType.PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("DocType") });
                }
                if (_poHead.VendID.PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("VendID") });
                }

              
                if (_poHead.Terms.PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("Terms") });
                }

                if (_poHead.DocDate.ToString().PassNull() == "")
                {
                    throw new MessageException(MessageType.Message, "15", parm: new[] { Util.GetLang("DocDate") });
                }


                //Check PO has no detail data
                if (_lstPOTrans.Count == 0)
                {
                    throw new MessageException(MessageType.Message, "704");


                }
                if (_lstPOTrans.Where(p => string.IsNullOrEmpty(p.RcptUnitDescr.PassNull())).Count() > 0)
                {
                    throw new MessageException(MessageType.Message, "25");

                }

                for (Int32 i = 0; i < _lstPOTrans.Count; i++)
                {
                    PO10401_pgDetail_Result objPO_Trans = new PO10401_pgDetail_Result();
                    objPO_Trans = _lstPOTrans[i];
                    if ((objPO_Trans.PurchaseType == "GI" || objPO_Trans.PurchaseType == "GP") && (objPO_Trans.SiteID.Length == 0))
                    {
                        throw new MessageException(MessageType.Message, "222");

                    }
                    if (objPO_Trans.RcptQty == 0 || objPO_Trans.TranAmt == 0)
                    {
                        throw new MessageException(MessageType.Message, "44");

                    }
                }

                //kiem tra ton kho co du tra hang ko
                if (_poHead.RcptType == "X")
                {
                    string invtID = "";
                    //kiểm tra trong itemlot
                    foreach (var objlot in _lstLot)
                    {
                        var qty = objlot.UnitMultDiv == "M" ? objlot.Qty * objlot.CnvFact : objlot.Qty / objlot.CnvFact;
                        var objItemLot = _db.IN_ItemLot.Where(p => p.InvtID == objlot.InvtID && p.SiteID == objlot.SiteID && p.LotSerNbr == objlot.LotSerNbr).FirstOrDefault();
                        if (objItemLot == null || qty > objItemLot.QtyAvail)
                        {
                            invtID += objlot.InvtID + ",";
                        }

                    }
                    if (invtID != "") throw new MessageException(MessageType.Message, "1043", parm: new[] { invtID, "" });
                }
            
            return true;
        }

        private void Data_Release()
        {
            //if (_handle != "N" && _handle!="")
            //{
            //    DataAccess dal = Util.Dal();
            //    try
            //    {
            //        POProcess.PO po = new POProcess.PO(Current.UserName, ScreenNbr, dal);
            //        if (_handle == "R")
            //        {
            //            dal.BeginTrans(IsolationLevel.ReadCommitted);
            //            if (!po.PO10401_Release(_branchID, _batNbr, _rcptNbr))
            //            {
            //                dal.RollbackTrans();
            //            }
            //            else
            //            {
            //                dal.CommitTrans();
            //            }

            //            Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, batNbr = _batNbr });
            //        }
            //        else if (_handle == "C" || _handle == "V")
            //        {
            //            dal.BeginTrans(IsolationLevel.ReadCommitted);
            //            if (!po.PO10401_Cancel(_branchID, _batNbr, _rcptNbr, _form["b714"].ToBool()))
            //            {
            //                dal.RollbackTrans();
            //            }
            //            else
            //            {
            //                dal.CommitTrans();
            //            }
            //            Util.AppendLog(ref _logMessage, "9999", data: new { success = true, batNbr = _batNbr });
            //        }
            //        po = null;
            //    }
            //    catch (Exception)
            //    {
            //        dal.RollbackTrans();
            //        throw;
            //    }
            //}
        }

      
        #endregion
    }
    public class CountInvtID
    {
        public int count { get; set; }
        public string invtID { get; set; }
    }
}

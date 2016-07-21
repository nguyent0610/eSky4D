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
using PO10400.Models;
using System.Data.SqlClient;
namespace PO10400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class PO10400Controller : Controller
    {
        private string _screenNbr = "PO10400";
        PO10400Entities _db = Util.CreateObjectContext<PO10400Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;

        private FormCollection _form;
        private List<PO10400_pgDetail_Result> _lstPODetailLoad = new List<PO10400_pgDetail_Result>();
        private PO_Setup _objPO_Setup;
        private PO_Header _poHead;
        private PO10400_pdOM_UserDefault_Result objOM_UserDefault;      
        bool _statusClose = false;
        string _ponbr = "";
        string _branchID = "";    
     
        List<PO10400_pdOM_DiscAllByBranchPO_Result> _lstPO10400_pdOM_DiscAllByBranchPO;
        List<PO10400_pdIN_UnitConversion_Result> _PO10400_pdIN_UnitConversion_Result;
        private List<PO10400_pgDetail_Result> _lstTmpPO10400_pgDetail;
        private bool _freeLineRunning = false;

     
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
        public ActionResult GetDet(string cpnyID,DateTime? fromDate, DateTime? todate, string status)
        {
            var dets = _db.PO10400_pgLoadPOHeader(Current.UserName, cpnyID,fromDate, todate, status,DateTime.Now.ToDateShort()).ToList();
            return this.Store(dets);
        }
        public ActionResult GetPO_Header(string pONbr, string branchID)
        {
            var obj = _db.PO_Header.Where(p => p.PONbr == pONbr && p.BranchID == branchID);
            return this.Store(obj);

        }
        public ActionResult GetAP_VendorTax(string vendID, string ordFromId)
        {

            return this.Store(_db.PO10400_pdAP_VenDorTaxes(vendID, ordFromId));

        }
        public ActionResult GetPO10400_pgDetail(string pONbr, string branchID)
        {
         
            return this.Store(_db.PO10400_pgDetail(pONbr, branchID,DateTime.Now.ToDateShort(), "%").ToList());

        }
        public ActionResult GetPO10400_pgLoadTaxTrans(string pONbr, string branchID)
        {
            return this.Store(_db.PO10400_pgLoadTaxTrans(branchID, pONbr).ToList());
        }

        //get price
        [DirectMethod]
        public ActionResult PO10400POPrice(string branchID = "", string invtID = "", string Unit = "", DateTime? podate = null)
        {
            var result = _db.PO10400_ppGetPrice(branchID, invtID, Unit, podate).FirstOrDefault().Value;
            return this.Direct(result);

        }
        [DirectMethod]
        public ActionResult PO10400ItemSitePrice(string branchID = "", string invtID = "", string siteID = "")
        {
            var objIN_ItemSite = _db.IN_ItemSite.Where(p => p.InvtID == invtID && p.SiteID == siteID).FirstOrDefault();
            if (objIN_ItemSite == null)
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.ResetET();
            }
            return this.Direct(objIN_ItemSite);

        }       
           
        #endregion
        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var handle = data["cboHandleApprove"];
                var status = data["cboStatusApprove"];
                var imgHandler = new StoreDataHandler(data["lstDetChange"]);
                var lstDet = imgHandler.ObjectData<PO10400_pgLoadPOHeader_Result>().ToList();
                string message = "";
                var user = _sys.Users.FirstOrDefault(p => p.UserName.ToUpper() == Current.UserName.ToUpper());
                int count = lstDet.Where(p => p.Selected == true).ToList().Count();

                var objHandle = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == _screenNbr && p.Status == status && p.Handle == handle).FirstOrDefault();
                string lstBranchID="";
                string lstPONbr="";
                var objConfig = _db.PO10400_ppApproveConfig(Current.UserName, Current.LangID, status).FirstOrDefault();

                if (objHandle == null) throw new MessageException(MessageType.Message, "20404", parm: new[] { Util.GetLang("SI_Handle") });
                foreach (var objDet in lstDet)
                {
                    lstBranchID += objDet.BranchID + ",";
                    lstPONbr += objDet.PONbr + ",";
                }
                #region co kiem tra ton kho
                if (objConfig.isCheckStock == true)//co kiem tra ton kho
                {
                    if (objConfig.isCheckAllOrder == true)
                    {
                        var objCheckApprove = _db.PO10400_ppCheckApprove(lstBranchID, lstPONbr, DateTime.Now.ToDateShort(), objHandle.ToStatus).ToList();
                        if (objCheckApprove.Count() > 0)//show thong bao khi co san pham khong du ton kho sap
                        {
                            foreach (var obj in objCheckApprove)
                            {

                                message += string.Format("{0} {1} {2}<br/>", Util.GetLang("InvtID") + " " + obj.InvtID, Util.GetLang("QtyNotEnough") + " " + obj.Qty, Util.GetLang("QtyStock") + " " + obj.AvailableQty);
                            }
                            throw new MessageException("20410", parm: new[] { message });
                           
                        }
                        else
                        {
                            foreach (var objDet in lstDet)
                            {
                                var header = _db.PO_Header.Where(p => p.BranchID == objDet.BranchID && p.PONbr == objDet.PONbr).FirstOrDefault();
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
                            _db.PO10400_ppUpdateSapStock(lstBranchID, lstPONbr, DateTime.Now.ToDateShort(),objHandle.ToStatus);
                            _db.SaveChanges();
                        }
                    }
                    else
                    {

                        foreach (var objDet in lstDet)
                        {
                            var objCheckApprove = _db.PO10400_ppCheckApprove(objDet.BranchID, objDet.PONbr, DateTime.Now.ToDateShort(), objHandle.ToStatus).ToList();
                            if (objCheckApprove.Count() > 0)
                            {
                                message += objDet.PONbr + ",";                                                                               
                            }
                            else
                            {
                                try
                                {
                                    var header = _db.PO_Header.Where(p => p.BranchID == objDet.BranchID && p.PONbr == objDet.PONbr).FirstOrDefault();
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
                                    //cap nhat lai sapstock
                                    _db.PO10400_ppUpdateSapStock(objDet.BranchID, objDet.PONbr, DateTime.Now.ToDateShort(), objHandle.ToStatus);
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
                            message = string.Format("{0} {1}<br/>", Util.GetLang("PONbr") + " " + message.TrimEnd(','), Util.GetLang("notenoughqty"));
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
                        var header = _db.PO_Header.Where(p => p.BranchID == objDet.BranchID && p.PONbr == objDet.PONbr).FirstOrDefault();
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
                    _db.PO10400_ppUpdateSapStock(lstBranchID, lstPONbr, DateTime.Now.ToDateShort(), objHandle.ToStatus);
                    _db.SaveChanges();
                }
                if (!string.IsNullOrWhiteSpace(handle) && handle != "N")
                {
                    SendMail(lstBranchID.TrimEnd(','), lstPONbr.TrimEnd(','), status, handle);
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

        private void SendMail(string branchID, string lstObj, string status, string handle)
        {
            try
            {
                string[] roles = new string[] {};
                var objUser = _sys.Users.FirstOrDefault(x => x.UserName.ToUpper() == Current.UserName.ToUpper());
                if (objUser != null && !string.IsNullOrWhiteSpace(objUser.UserTypes))
                {
                    roles = objUser.UserTypes.Split(',');
                }
                HQSendMailApprove.Approve.SendMailApprove(branchID, lstObj, _screenNbr, Current.CpnyID, status, handle, roles, Current.UserName, Current.LangID);

                //_SendMail.SendMailApprove             (branch, lstObj, hqSys.ScreenNumber, hqSys.CpnyID, cboStatus.StatusToValue(), cboHandle.HandleToValue(), _roles, hqSys.UserName, hqSys.LangID).Completed += (sender6, arg6) =>
                //{
                //    InvokeOperation approve = (sender6 as InvokeOperation);
                //    if (approve.HasError)
                //    {
                //        //approve.MarkErrorAsHandled();
                //        //ErrorWindow.CreateNew(approve.Error);
                //        busyIndicator.IsBusy = false;
                //        //throw new Exception(approve.Error.ToString());
                //    }
                //    busyIndicator.IsBusy = false;
                //};
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private string GetMail(string listBranch, string listMail)
        //{
        //    listMail = listMail == null ? string.Empty : listMail;
        //    listBranch = listBranch == null ? string.Empty : listBranch;
        //    string to = string.Empty;
        //    string[] branchs = listBranch.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        //    string[] roles = listMail.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        //    foreach (var role in roles)
        //    {
        //        if (role.ToUpper() == "DIST" || role.ToUpper() == "SUBDIST")
        //        {
        //            foreach (var branch in branchs)
        //            {
        //                var company = (from p in _sys.SYS_Company where p.CpnyID == branch select p).FirstOrDefault();
        //                if (company != null)
        //                    to += ';' + company.Email;
        //            }
        //        }
        //        else if (role.ToUpper() == "SUP" || role.ToUpper() == "ADMIN")
        //        {
        //            foreach (var branch in branchs)
        //            {
        //                var user = (from p in _sys.Users
        //                            where p.UserTypes != null && p.UserTypes.Split(',').Any(c => c.ToUpper() == role.ToUpper())
        //                                && p.CpnyID != null && p.CpnyID.Split(',').Any(c => c.ToUpper() == branch.ToUpper())
        //                            select p).FirstOrDefault();
        //                if (user != null)
        //                    to += ';' + user.Email;
        //            }
        //        }
        //        else
        //        {
        //            var users = (from p in _sys.Users where p.UserTypes != null && p.UserTypes.Split(',').Any(c => c.ToUpper() == role.ToUpper()) select p).ToList();
        //            foreach (var user in users)
        //            {
        //                to += ';' + user.Email;
        //            }
        //        }
        //    }
        //    return to;
        //}

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

        //    // stored procedure PO10400_Approve
        //    var toStatus = _db.ExecuteStoreQuery<string>(proc, lstParams).FirstOrDefault();

        //    var contentProc = _db.PO10400_ApproveContent(parameter["@ScreenNbr"],
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
        [HttpPost]
        public ActionResult DeleteGrd(FormCollection data)
        {
            try
            {
                _form = data;
                _ponbr = data["txtPONbr"];
                _branchID = data["txtBranchID"];
         
               
                DateTime dpoDate = data["PODate"].ToDateShort();


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO_Header>().FirstOrDefault();


                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPODetailLoad = detHandler.ObjectData<PO10400_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstDel"]);
                ChangeRecords<PO10400_pgDetail_Result> lst = dataHandler.BatchObjectData<PO10400_pgDetail_Result>();

                if (_poHead == null)
                {
                    throw new MessageException(MessageType.Message, "19");
                }
                else
                {
                    foreach (PO10400_pgDetail_Result deleted in lst.Deleted)
                    {
                        var obj1 = _db.PO_Detail.Where(p => p.BranchID == deleted.BranchID && p.PONbr == deleted.PONbr && p.LineRef == deleted.LineRef).FirstOrDefault();
                        if (obj1 != null)
                        {
                            if (deleted.PurchaseType == "GI" || deleted.PurchaseType == "PR" || deleted.PurchaseType == "GP" || deleted.PurchaseType == "GS")
                            {
                                double OldQty = Math.Round((obj1.UnitMultDiv == "D" ? ((obj1.QtyOrd - obj1.QtyRcvd) / obj1.CnvFact) : (obj1.QtyOrd - obj1.QtyRcvd) * obj1.CnvFact));
                                UpdateOnPOQty(obj1.InvtID, obj1.SiteID, OldQty, 0, 2);
                            }
                            _db.PO_Detail.DeleteObject(obj1);
                        }
                    }
                    Save_PO_Header(true);
                    _db.SaveChanges();
                }
                return Util.CreateMessage(MessageProcess.Delete, new { pONbr = _ponbr });
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
        public ActionResult SaveEdit(FormCollection data)
        {
            try
            {
                _form = data;
                _ponbr = data["txtPONbr"];
                _branchID = data["txtBranchID"];               
                var handle = data["Handle"];
                string message="";
                DateTime dpoDate = data["PODate"].ToDateShort();


                var detHeader = new StoreDataHandler(data["lstHeader"]);
                _poHead = detHeader.ObjectData<PO_Header>().FirstOrDefault();


                var detHandler = new StoreDataHandler(data["lstDet"]);
                _lstPODetailLoad = detHandler.ObjectData<PO10400_pgDetail_Result>()
                            .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                            .ToList();

               
                Save_PO_Header();
                _db.SaveChanges();
                if (handle != "" && handle != "N")
                {
                    var objHandle = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == _screenNbr && p.Status == _poHead.Status && p.Handle == handle).FirstOrDefault();
                    var objConfig = _db.PO10400_ppApproveConfig(Current.UserName, Current.LangID, _poHead.Status).FirstOrDefault();
                    if (objHandle == null) throw new MessageException(MessageType.Message, "20404", parm: new[] { Util.GetLang("SI_Handle") });                   
                    #region co kiem tra ton kho
                    if (objConfig.isCheckStock == true)//co kiem tra ton kho
                    {
                        var objCheckApprove = _db.PO10400_ppCheckApprove(_poHead.BranchID, _poHead.PONbr, DateTime.Now.ToDateShort(), objHandle.ToStatus).ToList();
                        if (objCheckApprove.Count() > 0)
                        {

                            message = string.Format("{0} {1}<br/>", Util.GetLang("PONbr") + " " + _poHead.PONbr, Util.GetLang("notenoughqty"));
                            Util.AppendLog(ref _logMessage, "20410", "", parm: new[] { message });
                            return _logMessage;
                        }
                        else
                        {
                            try
                            {
                                var header = _db.PO_Header.Where(p => p.BranchID == _poHead.BranchID && p.PONbr == _poHead.PONbr).FirstOrDefault();
                                header.Status = objHandle.ToStatus;
                                header.LUpd_DateTime = DateTime.Now;
                                header.LUpd_Prog = _screenNbr;
                                header.LUpd_User = Current.UserName;
                                _db.PO10400_ppUpdateSapStock(_poHead.BranchID, _poHead.PONbr, DateTime.Now.ToDateShort(), objHandle.ToStatus);
                                _db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }                       
                    }
                    #endregion
                    else//khong kiem tra ton kho
                    {
                        var header = _db.PO_Header.Where(p => p.BranchID == _poHead.BranchID && p.PONbr == _poHead.PONbr).FirstOrDefault();
                        header.Status = objHandle.ToStatus;
                        header.LUpd_DateTime = DateTime.Now;
                        header.LUpd_Prog = _screenNbr;
                        header.LUpd_User = Current.UserName;
                        //cap nhat lai sapstock
                        _db.PO10400_ppUpdateSapStock(_poHead.BranchID, _poHead.PONbr, DateTime.Now.ToDateShort(), objHandle.ToStatus);
                        _db.SaveChanges();
                    }
                    return Util.CreateMessage(MessageProcess.Process, new { pONbr = _ponbr });
                }
                return Util.CreateMessage(MessageProcess.Save, new { pONbr = _ponbr });

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
        private void Save_PO_Header(bool isDeleteGrd = false)
        {
            _lstPO10400_pdOM_DiscAllByBranchPO = _db.PO10400_pdOM_DiscAllByBranchPO(_branchID).ToList();
            _PO10400_pdIN_UnitConversion_Result = _db.PO10400_pdIN_UnitConversion().ToList();
            objOM_UserDefault = _db.PO10400_pdOM_UserDefault(_branchID,Current.UserName).FirstOrDefault();
            _objPO_Setup = _db.PO_Setup.FirstOrDefault(p => p.BranchID == _branchID && p.SetupID == "PO");
            var obj = _db.PO_Header.FirstOrDefault(p => p.PONbr == _ponbr && p.BranchID == _branchID);
            if (Data_Checking(isDeleteGrd))
            {
                if (obj != null)
                {

                    if (obj.tstamp.ToHex() != _poHead.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                    UpdatingPO_Header(ref obj, _poHead, _lstPODetailLoad);
                    Save_PO_Detail(obj, _lstPODetailLoad);
                }
                else
                {
                    throw new MessageException(MessageType.Message, "19");
                }
            }
        }
        private void Save_PO_Detail(PO_Header header, List<PO10400_pgDetail_Result> lst)
        {
            int i = 0;
            try
            {
                for (i = 0; i < lst.Count; i++)
                {
                    PO10400_pgDetail_Result objDetail = lst[i];
                    var objPO_Detail = _db.PO_Detail.Where(p => p.BranchID == header.BranchID && p.PONbr == header.PONbr && p.LineRef == objDetail.LineRef).FirstOrDefault();
                    if (objPO_Detail == null)
                    {
                        objPO_Detail = new PO_Detail();
                        objPO_Detail.ResetET();

                        UpdatingPO_Detail(objDetail, ref objPO_Detail);
                        objPO_Detail.Crtd_DateTime = DateTime.Now;
                        objPO_Detail.Crtd_Prog = _screenNbr;
                        objPO_Detail.Crtd_User = Current.UserName;
                        objPO_Detail.ReqdDate = objPO_Detail.ReqdDate.ToDateShort();

                        _db.PO_Detail.AddObject(objPO_Detail);
                    }
                    else
                    {
                        if (objPO_Detail.tstamp.ToHex() != objDetail.tstamp.ToHex())
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        UpdatingPO_Detail(objDetail, ref objPO_Detail);
                    }
                }
            
             


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void UpdatingPO_Header(ref PO_Header objHeader, PO_Header _poHead, List<PO10400_pgDetail_Result> lst)
        {

            try
            {
                objHeader.VouchStage = _poHead.VouchStage.PassNull();
                objHeader.POAmt = _poHead.POAmt.ToDouble();
                //objHeader.RcptTotAmt = _poHead.RcptTotAmt.ToDouble();
                objHeader.POFeeTot = lst.Sum(p => p.POFee);

                //tap main
                objHeader.ReqNbr = _poHead.ReqNbr.PassNull();
                objHeader.VendID = _poHead.VendID.PassNull();
                objHeader.NoteID = 0;
                objHeader.Status = _poHead.Status.PassNull();
                objHeader.POType = _poHead.POType.PassNull();
                objHeader.BlktPONbr = _poHead.BlktPONbr.PassNull();
                //tap shipping Information
                objHeader.ShipDistAddrID = _poHead.ShipDistAddrID.PassNull();
                objHeader.ShiptoType = _poHead.ShiptoType.PassNull();
                objHeader.ShipSiteID = _poHead.ShipSiteID.PassNull();
                objHeader.ShipCustID = _poHead.ShipCustID.PassNull();
                objHeader.ShiptoID = _poHead.ShiptoID.PassNull();
                objHeader.ShipVendID = _poHead.ShipVendID.PassNull();
                objHeader.ShipVendAddrID = _poHead.ShipVendAddrID.PassNull();
                objHeader.ShipAddrID = _poHead.ShipAddrID.PassNull();
                objHeader.ShipName = _poHead.ShipName.PassNull();
                objHeader.ShipAttn = _poHead.ShipAttn.PassNull();
                objHeader.ShipAddr1 = _poHead.ShipAddr1.PassNull();
                objHeader.ShipAddr2 = _poHead.ShipAddr2.PassNull();
                objHeader.ShipCity = _poHead.ShipCity.PassNull();
                objHeader.ShipState = _poHead.ShipState.PassNull();
                objHeader.ShipZip = _poHead.ShipZip.PassNull();
                objHeader.ShipCountry = _poHead.ShipCountry.PassNull();
                objHeader.ShipPhone = _poHead.ShipPhone.PassNull();
                objHeader.ShipFax = _poHead.ShipFax.PassNull();
                objHeader.ShipEmail = _poHead.ShipEmail.PassNull();
                objHeader.ShipVia = _poHead.ShipVia.PassNull();
                //tap vendor Information
                objHeader.VendAddrID = _poHead.VendAddrID.PassNull();
                objHeader.VendName = _poHead.VendName.PassNull();
                objHeader.VendAttn = _poHead.VendAttn.PassNull();
                objHeader.VendAddr1 = _poHead.VendAddr1.PassNull();
                objHeader.VendAddr2 = _poHead.VendAddr2.PassNull();
                objHeader.VendCity = _poHead.VendCity.PassNull();
                objHeader.VendState = _poHead.VendState.PassNull();
                objHeader.VendZip = _poHead.VendZip.PassNull();
                objHeader.VendCountry = _poHead.VendCountry.PassNull();
                objHeader.VendPhone = _poHead.VendPhone.PassNull();
                objHeader.VendFax = _poHead.VendFax.PassNull();
                objHeader.VendEmail = _poHead.VendEmail.PassNull();
                //tap Other Information            
                objHeader.PODate = _poHead.PODate;
                objHeader.BlktExprDate = _poHead.BlktExprDate;
                objHeader.RcptStage = _poHead.RcptStage.PassNull();
                objHeader.Terms = _poHead.Terms.PassNull();
                objHeader.Buyer = _poHead.Buyer.PassNull();
             


                objHeader.LUpd_DateTime = DateTime.Now;
                objHeader.LUpd_Prog = _screenNbr;
                objHeader.LUpd_User = Current.UserName;
                objHeader.tstamp = new byte[0];

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void UpdatingPO_Detail(PO10400_pgDetail_Result objDetail, ref PO_Detail objrPO_Detail)
        {
            double OldQty = 0;
            double NewQty = 0;

            IN_ItemSite objIN_ItemSite = new IN_ItemSite();
            try
            {
                if (objDetail.PurchaseType == "GI" || objDetail.PurchaseType == "PR" || objDetail.PurchaseType == "GP" || objDetail.PurchaseType == "GS")
                {
                    var objIN_Inventory = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == objDetail.InvtID);
                    if (objIN_Inventory == null)
                    {
                        objIN_Inventory = new IN_Inventory();
                        objIN_Inventory.ResetET();
                        objIN_Inventory.InvtID = objDetail.InvtID;
                    }

                    try
                    {
                        objIN_ItemSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == objDetail.InvtID && p.SiteID == objDetail.SiteID);
                        if (objIN_ItemSite == null)
                        {
                            objIN_ItemSite = new IN_ItemSite();
                            Insert_IN_ItemSite(ref objIN_ItemSite, ref objIN_Inventory, objDetail.SiteID);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }


                }
                if (objDetail.PurchaseType == "GI" || objDetail.PurchaseType == "PR" || objDetail.PurchaseType == "GP" || objDetail.PurchaseType == "GS")
                {
                    NewQty = Math.Round((objDetail.UnitMultDiv == "D" ? (objDetail.QtyOrd / objDetail.CnvFact) : (objDetail.QtyOrd * objDetail.CnvFact)));
                    if (objrPO_Detail == null) OldQty = 0;
                    else
                        OldQty = Math.Round((objrPO_Detail.UnitMultDiv == "D" ? (objrPO_Detail.QtyOrd / objrPO_Detail.CnvFact) : objrPO_Detail.QtyOrd * objrPO_Detail.CnvFact));
                    UpdateOnPOQty(objDetail.InvtID, objDetail.SiteID, OldQty, NewQty, 2);

                }
                objrPO_Detail.BranchID = _branchID;
                objrPO_Detail.PONbr = _ponbr;
                objrPO_Detail.LineRef = objDetail.LineRef;

                objrPO_Detail.BlktLineID = objDetail.BlktLineID;
                objrPO_Detail.BlktLineRef = objDetail.BlktLineRef == null ? "" : objDetail.BlktLineRef;
                objrPO_Detail.CnvFact = (objDetail.CnvFact == 0 ? 1 : objDetail.CnvFact);

                objrPO_Detail.CostReceived = objDetail.CostReceived;
                objrPO_Detail.CostReturned = objDetail.CostReturned;
                objrPO_Detail.CostVouched = objDetail.CostVouched;
                objrPO_Detail.ExtCost = objDetail.ExtCost;
                objrPO_Detail.POFee = objDetail.POFee;
                objrPO_Detail.UnitCost = objDetail.UnitCost;

                objrPO_Detail.ExtWeight = objDetail.ExtWeight;
                objrPO_Detail.ExtVolume = objDetail.ExtVolume;
                objrPO_Detail.InvtID = objDetail.InvtID;
                objrPO_Detail.PromDate = objDetail.PromDate.ToDateShort();
                objrPO_Detail.PurchaseType = objDetail.PurchaseType;
                objrPO_Detail.PurchUnit = objDetail.PurchUnit;

                objrPO_Detail.QtyOrd = objDetail.QtyOrd;
                objrPO_Detail.QtyRcvd = objDetail.QtyRcvd;
                objrPO_Detail.QtyReturned = objDetail.QtyReturned;
                objrPO_Detail.QtyVouched = objDetail.QtyVouched;

                objrPO_Detail.RcptStage = objDetail.RcptStage;
                objrPO_Detail.ReasonCd = objDetail.ReasonCd;
                objrPO_Detail.ReqdDate = objDetail.ReqdDate.ToDateShort();
                objrPO_Detail.SiteID = objDetail.SiteID;
                objrPO_Detail.TaxCat = objDetail.TaxCat;
                objrPO_Detail.TranDesc = objDetail.TranDesc;

                objrPO_Detail.TaxID00 = objDetail.TaxID00;
                objrPO_Detail.TaxID01 = objDetail.TaxID01;
                objrPO_Detail.TaxID02 = objDetail.TaxID02;
                objrPO_Detail.TaxID03 = objDetail.TaxID03;

                objrPO_Detail.TxblAmt00 = objDetail.TxblAmt00;
                objrPO_Detail.TxblAmt01 = objDetail.TxblAmt01;
                objrPO_Detail.TxblAmt02 = objDetail.TxblAmt02;
                objrPO_Detail.TxblAmt03 = objDetail.TxblAmt03;

                objrPO_Detail.TaxAmt00 = objDetail.TaxAmt00;
                objrPO_Detail.TaxAmt01 = objDetail.TaxAmt01;
                objrPO_Detail.TaxAmt02 = objDetail.TaxAmt02;
                objrPO_Detail.TaxAmt03 = objDetail.TaxAmt03;

                objrPO_Detail.DiscAmt = objDetail.DiscAmt;
                objrPO_Detail.DiscID = objDetail.DiscID;
                objrPO_Detail.DiscPct = objDetail.DiscPct;
                objrPO_Detail.DiscSeq = objDetail.DiscSeq;
                objrPO_Detail.DiscLineRef = objDetail.DiscLineRef;

                objrPO_Detail.UnitMultDiv = objDetail.UnitMultDiv;
                objrPO_Detail.UnitWeight = objDetail.UnitWeight;
                objrPO_Detail.UnitVolume = objDetail.UnitVolume;
                objrPO_Detail.VouchStage = objDetail.VouchStage;
                objrPO_Detail.LUpd_DateTime = DateTime.Now;
                objrPO_Detail.LUpd_Prog = _screenNbr;
                objrPO_Detail.LUpd_User = Current.UserName;

            }
            catch (Exception ex)
            {

                throw (ex);

            }
        }
        public void Insert_IN_ItemSite(ref IN_ItemSite objIN_ItemSite, ref IN_Inventory objIN_Inventory, string SiteID)
        {
            try
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.ResetET();
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
                objIN_ItemSite.Crtd_DateTime = DateTime.Now;
                objIN_ItemSite.Crtd_Prog = _screenNbr;
                objIN_ItemSite.Crtd_User = Current.UserName;
                objIN_ItemSite.LUpd_DateTime = DateTime.Now;
                objIN_ItemSite.LUpd_Prog = _screenNbr;
                objIN_ItemSite.LUpd_User = Current.UserName;
                objIN_ItemSite.tstamp = new byte[0];
                _db.IN_ItemSite.AddObject(objIN_ItemSite);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
      
        public void UpdateOnPOQty(string InvtID, string SiteID, double OldQty, double NewQty, int DecQty)
        {
            IN_ItemSite objItemSite = new IN_ItemSite();
            try
            {
                try
                {
                    objItemSite = _db.IN_ItemSite.FirstOrDefault(p => p.InvtID == InvtID && p.SiteID == SiteID);
                    if (objItemSite != null)
                    {
                        objItemSite.QtyOnPO = Math.Round(objItemSite.QtyOnPO + NewQty - OldQty, DecQty);
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private bool Data_Checking(bool isDeleteGrd = false)
        {



            //Check PO has no detail data
            if (_lstPODetailLoad.Count == 0)
            {
                throw new MessageException(MessageType.Message, "704");


            }
            if (_lstPODetailLoad.Where(p => string.IsNullOrEmpty(p.PurchUnit.PassNull())).Count() > 0)
            {

                throw new MessageException(MessageType.Message, "25");

            }


            //Check MOQ
            AP_Vendor objVendor = new AP_Vendor();
            objVendor = _db.AP_Vendor.ToList().FirstOrDefault(p => p.VendID == _poHead.VendID.PassNull());
            if (objVendor.MOQVal > 0)
            {
                switch (objVendor.MOQType)
                {
                    case "Q":
                        if (_lstPODetailLoad.Sum(p => p.QtyOrd * p.CnvFact) < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                parm: new[] { Util.GetLang("Quantity"), objVendor.MOQVal.ToString() });



                        }
                        break;
                    case "V":
                        if (_lstPODetailLoad.Sum(p => p.ExtVolume) < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                  parm: new[] { Util.GetLang("TotVol"), objVendor.MOQVal.ToString() + "L" });

                        }
                        break;
                    case "A":
                        if (_poHead.POAmt < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                  parm: new[] { Util.GetLang("POAmt"), objVendor.MOQVal.ToString() });


                        }
                        break;
                    case "W":
                        if (_lstPODetailLoad.Sum(p => p.ExtWeight) < objVendor.MOQVal)
                        {
                            throw new MessageException(MessageType.Message, "747",
                                  parm: new[] { Util.GetLang("TotWeight"), objVendor.MOQVal.ToString() + "KG" });




                        }
                        break;
                }
            }



            for (Int16 i = 0; i <= _lstPODetailLoad.Count - 1; i++)
            {
                var objDetail = _lstPODetailLoad[i];

                if (objDetail.PurchaseType != "PR" && objDetail.ExtCost == 0)
                {
                    throw new MessageException(MessageType.Message, "44");
                }
                if (string.IsNullOrEmpty(objDetail.PurchUnit))
                {
                    throw new MessageException(MessageType.Message, "15",
                 parm: new[] { Util.GetLang("PurchUnit") });

                }
                if (string.IsNullOrEmpty(objDetail.SiteID))
                {
                    throw new MessageException(MessageType.Message, "15",
                    parm: new[] { Util.GetLang("SiteID") });
                }
                if (string.IsNullOrEmpty(objDetail.PurchaseType))
                {
                    throw new MessageException(MessageType.Message, "15",
                    parm: new[] { Util.GetLang("PurchaseType") });
                }
            }
            //    string strInvt = "";
            //    var query = (from f in _lstPODetailLoad
            //                 where f.PurchaseType != "PR"
            //                 //join l in lstPO10100_pgDetail on f.InvtID equals l.InvtID
            //                 group f by f.InvtID into g
            //                 let count = g.Count()
            //                 where count > 1
            //                 orderby count descending
            //                 select new CountInvtID
            //                 {
            //                     invtID = g.First().InvtID,
            //                     count = count
            //                 }).ToList();
            //    foreach (var obj in query)
            //    {
            //        strInvt += obj.invtID + ",";
            //    }
            //    if (strInvt != "")
            //    {
            //        throw new MessageException(MessageType.Message, "20417",
            //            parm: new[] { strInvt.TrimEnd(',') });

            //    }

            //    var lstremove = _lstPODetailLoad.Where(p => p.PurchaseType == "PR").ToList();
            //    while (_lstPODetailLoad.Where(p => p.PurchaseType == "PR" && (!string.IsNullOrEmpty(p.DiscLineRef)) && (p.DiscLineRef != p.LineRef)).Count() > 0)
            //    {
            //        var obj1 = _lstPODetailLoad.FirstOrDefault(p => p.PurchaseType == "PR" && (!string.IsNullOrEmpty(p.DiscLineRef)) && (p.DiscLineRef != p.LineRef));
            //        var obj = obj1;
            //        var objD = _db.PO_Detail.FirstOrDefault(p => p.PONbr == obj.PONbr && p.LineRef == obj.LineRef && p.BranchID == obj.BranchID && p.PurchaseType == "PR" && p.DiscLineRef == obj.DiscLineRef);
            //        if (objD != null) _db.PO_Detail.DeleteObject(objD);
            //        _lstPODetailLoad.Remove(obj1);

            //    }
            //    for (Int16 i = 0; i <= _lstPODetailLoad.Count - 1; i++)
            //    {
            //        var objDetail = _lstPODetailLoad[i];

            //        if (objDetail.PurchaseType != "PR" && objDetail.ExtCost == 0)
            //        {
            //            throw new MessageException(MessageType.Message, "44");
            //        }
            //        if (objDetail.PurchaseType != "PR" && objDetail.DiscAmt == 0)
            //        {

            //            _lineRef = LastLineRef(_lstPODetailLoad);
            //            double DiscAmt = 0;
            //            double DiscPct = 0;
            //            string discSeq = string.Empty;
            //            string discID = string.Empty;
            //            string budgetID = string.Empty;
            //            string breaklineRef = string.Empty;
            //            GetDiscLineSetup(objDetail, objDetail.QtyOrd, objDetail.ExtCost, ref DiscAmt, ref DiscPct, ref discSeq, ref discID, ref budgetID, ref breaklineRef);
            //            objDetail.DiscAmt = DiscAmt;
            //            objDetail.DiscPct = DiscPct;
            //            objDetail.DiscSeq = discSeq;
            //            objDetail.DiscID = discID;
            //            objDetail.ExtCost = objDetail.QtyOrd * objDetail.UnitCost - objDetail.DiscAmt;

            //        }
            //    }
            //    FreeItemForLine(ref _lineRef, "");
            return true;
        }                             
        #endregion
    }
}

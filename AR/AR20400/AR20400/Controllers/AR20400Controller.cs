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
using System.Drawing;
using HQ.eSkySys;
using HQSendMailApprove;

namespace AR20400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20400Controller : Controller
    {
        private string _screenNbr = "AR20400";
        private string _userName = Current.UserName;
        AR20400Entities _db = Util.CreateObjectContext<AR20400Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;
        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadAR20400");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\AR20400");
                }
                return _filePath;
            }
        }
        public ActionResult Index()
        {
            var tabContract = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabContract");
            var tabAdvTool = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabAdvTool");
            var tabSellingProduct = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabSellingProduct");
            var tabDisplayMethod = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabDisplayMethod");
			var readonlyShopType = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "blockShopType");

            if (tabContract == null)
                ViewBag.Contract = "false";
            else
            {
                if (tabContract.IntVal == 1)
                    ViewBag.Contract = "true";
                else
                    ViewBag.Contract = "false";
            }

            if (tabAdvTool == null)
                ViewBag.AdvTool = "false";
            else
            {
                if (tabAdvTool.IntVal == 1)
                    ViewBag.AdvTool = "true";
                else
                    ViewBag.AdvTool = "false";
            }

            if (tabSellingProduct == null)
                ViewBag.SellingProduct = "false";
            else
            {
                if (tabSellingProduct.IntVal == 1)
                    ViewBag.SellingProduct = "true";
                else
                    ViewBag.SellingProduct = "false";
            }

            if (tabDisplayMethod == null)
                ViewBag.DisplayMethod = "false";
            else
            {
                if (tabDisplayMethod.IntVal == 1)
                    ViewBag.DisplayMethod = "true";
                else
                    ViewBag.DisplayMethod = "false";
            }

			if (readonlyShopType == null)
				ViewBag.ReadonlyShopType = "false";
			else
			{
				if (readonlyShopType.IntVal == 1)
					ViewBag.ReadonlyShopType = "true";
				else
					ViewBag.ReadonlyShopType = "false";
			}
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetAR_Customer(string CpnyID, string CustId)
        {
            return this.Store(_db.AR20400_pdHeader(CpnyID, CustId).FirstOrDefault());
        }

        public ActionResult GetAR_CustAdvTool(string CustId)
        {
            return this.Store(_db.AR20400_pgAR_CustAdvTool(CustId).ToList());
        }

        public ActionResult GetAR_CustSellingProducts(string CustId)
        {
            return this.Store(_db.AR20400_pgAR_CustSellingProducts(CustId).ToList());
        }

        public ActionResult GetAR_CustDisplayMethod(string CustId)
        {
            return this.Store(_db.AR20400_pgAR_CustDisplayMethod(CustId).ToList());
        }

        public ActionResult GetAR_LTTContract(string CustId)
        {
            return this.Store(_db.AR20400_pgAR_LTTContract(CustId).ToList());
        }

        public ActionResult GetAR_LTTContractDetail(string CustId)
        {
            return this.Store(_db.AR20400_pgAR_LTTContractDetail(CustId).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data, string NodeID, string NodeLevel, string ParentRecordID, string HiddenTree)
        {
            try
            {
                string isNew = "false";
                string CustId = data["cboCustId"].PassNull();
                string BranchID = data["cboCpnyID"].PassNull();
                string ClassId = data["cboClassId"].PassNull();
                string Status = data["cboStatus"].PassNull();
                string Handle = data["cboHandle"].PassNull();
                bool checkFlag = false;
                string LineRef_tmp = string.Empty;

                //strRefix = _db.AR20400_ppRefixCustomer(_userName, BranchID).FirstOrDefault();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstAR_Customer"]);
                var curHeader = dataHandler1.ObjectData<AR20400_pdHeader_Result>().FirstOrDefault();

                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstAR_CustAdvTool"]);
                ChangeRecords<AR20400_pgAR_CustAdvTool_Result> lstAR_CustAdvTool = dataHandler2.BatchObjectData<AR20400_pgAR_CustAdvTool_Result>();

                StoreDataHandler dataHandler3 = new StoreDataHandler(data["lstAR_CustDisplayMethod"]);
                ChangeRecords<AR20400_pgAR_CustDisplayMethod_Result> lstAR_CustDisplayMethod = dataHandler3.BatchObjectData<AR20400_pgAR_CustDisplayMethod_Result>();

                StoreDataHandler dataHandler6 = new StoreDataHandler(data["lstAR_CustSellingProducts"]);
                ChangeRecords<AR20400_pgAR_CustSellingProducts_Result> lstAR_CustSellingProducts = dataHandler6.BatchObjectData<AR20400_pgAR_CustSellingProducts_Result>();

                StoreDataHandler dataHandler4 = new StoreDataHandler(data["lstAR_LTTContract"]);
                var lstAR_LTTContract = dataHandler4.ObjectData<AR20400_pgAR_LTTContract_Result>() == null ? new List<AR20400_pgAR_LTTContract_Result>() : dataHandler4.ObjectData<AR20400_pgAR_LTTContract_Result>().Where(p => p.LTTContractNbr != "");

                StoreDataHandler dataHandler5 = new StoreDataHandler(data["lstAR_LTTContractDetail"]);
                var lstAR_LTTContractDetail = dataHandler5.ObjectData<AR20400_pgAR_LTTContractDetail_Result>() == null ? new List<AR20400_pgAR_LTTContractDetail_Result>() : dataHandler5.ObjectData<AR20400_pgAR_LTTContractDetail_Result>().Where(p => p.Type != "");

                var objAR_Setup = _db.AR_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupId == "AR");
                if (objAR_Setup != null)
                {
                    if (!objAR_Setup.AutoCustID && CustId == "")
                    {
                        throw new MessageException(MessageType.Message, "15", "", parm: new string[] { "CustId" });
                    }

                    #region Save AR_Customer
                    var header = _db.AR_Customer.FirstOrDefault(p => p.BranchID == BranchID && p.CustId == CustId);
                    if (header == null)
                    {
                        if (objAR_Setup.AutoCustID == true)
                        {
                            isNew = "true";
                            var objCustID = _db.AR20400_ppCustID(BranchID, "", "", "", "", "", "", "", "", "", ClassId, curHeader.CustName,curHeader.State, curHeader.District).FirstOrDefault();
                            header = new AR_Customer();
                            header.ResetET();
                            header.CustId = objCustID.PassNull();
                            header.BranchID = BranchID;
                            header.Crtd_Datetime = DateTime.Now;
                            header.Crtd_Prog = _screenNbr;
                            header.Crtd_User = Current.UserName;
                            UpdatingHeader(ref header, curHeader, NodeID, NodeLevel, ParentRecordID, HiddenTree, Status, Handle);
                            _db.AR_Customer.AddObject(header);

                            AR_CustHist objAR_CustHist = new AR_CustHist();
                            objAR_CustHist.ResetET();
                            Update_AR_CustHist(ref objAR_CustHist, header);
                            _db.AR_CustHist.AddObject(objAR_CustHist);
                        }
                        else
                        {
                            isNew = "true";
                            header = new AR_Customer();
                            header.ResetET();
                            header.CustId = CustId;
                            header.BranchID = BranchID;
                            header.Crtd_Datetime = DateTime.Now;
                            header.Crtd_Prog = _screenNbr;
                            header.Crtd_User = Current.UserName;
                            UpdatingHeader(ref header, curHeader, NodeID, NodeLevel, ParentRecordID, HiddenTree, Status, Handle);
                            _db.AR_Customer.AddObject(header);

                            AR_CustHist objAR_CustHist = new AR_CustHist();
                            objAR_CustHist.ResetET();
                            Update_AR_CustHist(ref objAR_CustHist, header);
                            _db.AR_CustHist.AddObject(objAR_CustHist);
                        }
                    }
                    else
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(ref header, curHeader, NodeID, NodeLevel, ParentRecordID, HiddenTree, Status, Handle);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }

                    CustId = header.CustId;

                    #endregion

                    #region Save AR_SOAddress
                    var objAR_SOAddress = _db.AR_SOAddress.FirstOrDefault(p => p.BranchID == BranchID
                                                                            && p.CustId == CustId
                                                                            && p.ShipToId == (curHeader.DfltShipToId == "" ? "DEFAULT" : curHeader.DfltShipToId));
                    if (objAR_SOAddress == null)
                    {
                        objAR_SOAddress = new AR_SOAddress();
                        objAR_SOAddress.ResetET();
                        objAR_SOAddress.BranchID = BranchID;
                        objAR_SOAddress.CustId = CustId;
                        objAR_SOAddress.ShipToId = curHeader.DfltShipToId == "" ? "DEFAULT" : curHeader.DfltShipToId;
                        objAR_SOAddress.Crtd_DateTime = DateTime.Now;
                        objAR_SOAddress.Crtd_Prog = _screenNbr;
                        objAR_SOAddress.Crtd_User = Current.UserName;
                        UpdatingAR_SOAddress(ref objAR_SOAddress, curHeader);
                        _db.AR_SOAddress.AddObject(objAR_SOAddress);
                    }
                    else
                    {
                        UpdatingAR_SOAddress(ref objAR_SOAddress, curHeader);
                    }
                    #endregion

                    #region Save AR_CustAdvTool

                    checkFlag = false;
                    LineRef_tmp = string.Empty;

                    foreach (AR20400_pgAR_CustAdvTool_Result deleted in lstAR_CustAdvTool.Deleted)
                    {
                        var objDelete = _db.AR_CustAdvTool.Where(p => p.CustID == CustId
                                                                      && p.LineRef == deleted.LineRef).FirstOrDefault();
                        if (objDelete != null)
                        {
                            _db.AR_CustAdvTool.DeleteObject(objDelete);
                        }
                    }

                    lstAR_CustAdvTool.Created.AddRange(lstAR_CustAdvTool.Updated);

                    foreach (AR20400_pgAR_CustAdvTool_Result curLang in lstAR_CustAdvTool.Created)
                    {
                        if (CustId.PassNull() == "" || curLang.Type.PassNull() == "") continue;

                        var lang = _db.AR_CustAdvTool.FirstOrDefault(p => p.CustID.ToLower() == CustId.ToLower()
                                                                      && p.LineRef.ToLower() == curLang.LineRef.ToLower());

                        if (lang != null)
                        {
                            if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                            {
                                UpdatingAR_CustAdvTool(lang, curLang, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            lang = new AR_CustAdvTool();
                            lang.ResetET();
                            lang.CustID = CustId;

                            if (checkFlag == false)
                            {
                                lang.LineRef = _db.AR20400_ppGetLineRefAR_CustAdvTool(CustId).FirstOrDefault().PassNull();
                            }
                            else
                            {
                                int sub = int.Parse(LineRef_tmp.Substring(LineRef_tmp.Length - 3, 3)) + 1;
                                string strtemp = "000" + sub.ToString();
                                lang.LineRef = LineRef_tmp.Substring(0, LineRef_tmp.Length - 3) + strtemp.Substring(strtemp.Length - 3, 3);
                            }
                            LineRef_tmp = lang.LineRef;


                            UpdatingAR_CustAdvTool(lang, curLang, true);
                            _db.AR_CustAdvTool.AddObject(lang);
                            checkFlag = true;
                        }
                    }
                    #endregion

                    #region Save AR_CustSellingProducts

                    foreach (AR20400_pgAR_CustSellingProducts_Result deleted in lstAR_CustSellingProducts.Deleted)
                    {
                        var objDelete = _db.AR_CustSellingProducts.Where(p => p.CustID == CustId
                                                                      && p.Code == deleted.Code).FirstOrDefault();
                        if (objDelete != null)
                        {
                            _db.AR_CustSellingProducts.DeleteObject(objDelete);
                        }
                    }

                    lstAR_CustSellingProducts.Created.AddRange(lstAR_CustSellingProducts.Updated);

                    foreach (AR20400_pgAR_CustSellingProducts_Result curLang in lstAR_CustSellingProducts.Created)
                    {
                        if (CustId.PassNull() == "" || curLang.Code.PassNull() == "") continue;

                        var lang = _db.AR_CustSellingProducts.FirstOrDefault(p => p.CustID.ToLower() == CustId.ToLower()
                                                                      && p.Code.ToLower() == curLang.Code.ToLower());

                        if (lang != null)
                        {
                            if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                            {
                                UpdatingAR_CustSellingProducts(lang, curLang, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            lang = new AR_CustSellingProducts();
                            lang.ResetET();
                            lang.CustID = CustId;
                            lang.Code = curLang.Code;
                            UpdatingAR_CustSellingProducts(lang, curLang, true);
                            _db.AR_CustSellingProducts.AddObject(lang);
                        }
                    }
                    #endregion

                    #region Save AR_CustDisplayMethod

                    foreach (AR20400_pgAR_CustDisplayMethod_Result deleted in lstAR_CustDisplayMethod.Deleted)
                    {
                        var objDelete = _db.AR_CustDisplayMethod.Where(p => p.CustID == CustId
                                                                      && p.DispMethod == deleted.DispMethod).FirstOrDefault();
                        if (objDelete != null)
                        {
                            _db.AR_CustDisplayMethod.DeleteObject(objDelete);
                        }
                    }

                    lstAR_CustDisplayMethod.Created.AddRange(lstAR_CustDisplayMethod.Updated);

                    foreach (AR20400_pgAR_CustDisplayMethod_Result curLang in lstAR_CustDisplayMethod.Created)
                    {
                        if (CustId.PassNull() == "" || curLang.DispMethod.PassNull() == "") continue;

                        var lang = _db.AR_CustDisplayMethod.FirstOrDefault(p => p.CustID.ToLower() == CustId.ToLower()
                                                                      && p.DispMethod.ToLower() == curLang.DispMethod.ToLower());

                        if (lang != null)
                        {
                            if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                            {
                                UpdatingAR_CustDisplayMethod(lang, curLang, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            lang = new AR_CustDisplayMethod();
                            lang.ResetET();
                            lang.CustID = CustId;
                            lang.DispMethod = curLang.DispMethod;
                            UpdatingAR_CustDisplayMethod(lang, curLang, true);
                            _db.AR_CustDisplayMethod.AddObject(lang);
                        }
                    }
                    #endregion

                    #region Save AR_LTTContract
                    var lstOld_AR_LTTContract = _db.AR_LTTContract.Where(p => p.CustID.ToLower() == CustId.ToLower()).ToList();

                    foreach (var objold in lstOld_AR_LTTContract)
                    {
                        if (lstAR_LTTContract.Where(p => p.LTTContractNbr == objold.LTTContractNbr).FirstOrDefault() == null)
                        {
                            _db.AR_LTTContract.DeleteObject(objold);
                        }
                    }

                    foreach (var item in lstAR_LTTContract)
                    {
                        if (item.LTTContractNbr.PassNull() == "") continue;
                        var obj = _db.AR_LTTContract.FirstOrDefault(p => p.LTTContractNbr.ToLower() == item.LTTContractNbr.ToLower());
                        if (obj != null)
                        {
                            UpdatingAR_LTTContract(obj, item, false);
                        }
                        else
                        {
                            obj = new AR_LTTContract();
                            obj.ResetET();
                            obj.CustID = CustId;
                            UpdatingAR_LTTContract(obj, item, true);
                            _db.AR_LTTContract.AddObject(obj);
                        }
                    }
                    #endregion

                    #region Save AR_LTTContractDetail
                    checkFlag = false;
                    LineRef_tmp = string.Empty;

                    var lstOld_AR_LTTContractDetail = _db.AR_LTTContractDetail.ToList();

                    foreach (var objold in lstOld_AR_LTTContractDetail)
                    {
                        if (lstAR_LTTContractDetail.Where(p => p.LTTContractNbr == objold.LTTContractNbr
                                                        && p.LineRef == objold.LineRef).FirstOrDefault() == null)
                        {
                            _db.AR_LTTContractDetail.DeleteObject(objold);
                        }
                    }

                    foreach (var item in lstAR_LTTContractDetail)
                    {
                        if (item.LTTContractNbr.PassNull() == "" || item.Type.PassNull() == "") continue;
                        var obj = _db.AR_LTTContractDetail.FirstOrDefault(p => p.LTTContractNbr.ToLower() == item.LTTContractNbr.ToLower()
                                                                        && p.Type.ToLower() == item.Type.ToLower());
                        if (obj != null)
                        {
                            UpdatingAR_LTTContractDetail(obj, item, false);
                        }
                        else
                        {
                            obj = new AR_LTTContractDetail();
                            obj.ResetET();

                            if (checkFlag == false)
                            {
                                obj.LineRef = _db.AR20400_ppGetLineRefAR_LTTContractDetail(item.LTTContractNbr).FirstOrDefault().PassNull();
                            }
                            else
                            {
                                int sub = int.Parse(LineRef_tmp.Substring(LineRef_tmp.Length - 3, 3)) + 1;
                                string strtemp = "000" + sub.ToString();
                                obj.LineRef = LineRef_tmp.Substring(0, LineRef_tmp.Length - 3) + strtemp.Substring(strtemp.Length - 3, 3);
                            }
                            LineRef_tmp = obj.LineRef;

                            UpdatingAR_LTTContractDetail(obj, item, true);
                            _db.AR_LTTContractDetail.AddObject(obj);

                            checkFlag = true;
                        }
                    }
                    #endregion

                    #region Upload files
                    var files = Request.Files;
                    if (files.Count > 0 && files[0].ContentLength > 0) // Co chon file de upload
                    {

                        string newFileName = string.Format("{0}_{1}{2}", header.BranchID, header.CustId, Path.GetExtension(files[0].FileName));
                        Util.UploadFile(FilePath, newFileName, files[0]);
                        header.PhotoCode = newFileName;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(header.PhotoCode) && string.IsNullOrWhiteSpace(curHeader.PhotoCode))
                        {

                            Util.UploadFile(FilePath, header.PhotoCode, null);
                            header.PhotoCode = string.Empty;
                        }
                    }
                    #endregion
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2016030901");
                }
                
                _db.SaveChanges();
                return Json(new { success = true, CustId = CustId, isNew = isNew });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(ref AR_Customer t, AR20400_pdHeader_Result s, string NodeID, string NodeLevel, string ParentRecordID, string HiddenTree, string Status, string Handle)
        {
            if (HiddenTree != "true")
            {
                t.NodeID = NodeID;
                t.NodeLevel = short.Parse(NodeLevel.PassNull() == "" ? "0" : NodeLevel);
                t.ParentRecordID = short.Parse(ParentRecordID.PassNull() == "" ? "0" : ParentRecordID);
            }
            else
            {
                t.NodeID = "DF";
                t.NodeLevel = 1;
                t.ParentRecordID = 0;
            }

            if (Handle == string.Empty || Handle == "N")
                t.Status = Status;
            else
                t.Status = Handle;

            if (t.Status == "O")
            {
                X.Msg.Show(new MessageBoxConfig()
                {
                    Message = "Email sent!"
                });
                var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
                Approve.Mail_Approve(_screenNbr, t.CustId, user.UserTypes, t.Status, Handle, Current.LangID.ToString()
                             , _userName, t.BranchID, Current.CpnyID, string.Empty, string.Empty, string.Empty);
            }

            t.ClassId = s.ClassId;
            t.CustType = s.CustType;
            t.CustName = s.CustName;
            t.PriceClassID = s.PriceClassID;
            t.Terms = s.Terms;
            t.TradeDisc = s.TradeDisc;
            t.CrRule = s.CrRule;
            t.CrLmt = s.CrLmt;
            t.GracePer = s.GracePer;
            t.Territory = s.Territory;
            t.Area = s.Area;
            t.Location = s.Location;
            t.Channel = s.Channel;
            t.ShopType = s.ShopType;
            t.GiftExchange = s.GiftExchange;
            t.HasPG = s.HasPG;
            t.SlsperId = s.SlsperId;
            t.DeliveryID = s.DeliveryID;
            t.SupID = s.SupID;
            t.SiteId = s.SiteId;
            t.DfltShipToId = s.DfltShipToId == "" ? "DEFAULT" : s.DfltShipToId;
            t.CustFillPriority = s.CustFillPriority;
            t.LTTContractNbr = s.LTTContractNbr;
            t.DflSaleRouteID = s.DflSaleRouteID;
            t.EmpNum = s.EmpNum;
            t.ExpiryDate = s.ExpiryDate.ToDateShort();
            t.EstablishDate = s.EstablishDate.ToDateShort();
            t.Birthdate = s.Birthdate.ToDateShort();
            t.CustName = s.CustName;
            t.Attn = s.Attn;
            t.Salut = s.Salut;
            t.Addr1 = s.Addr1;
            t.Addr2 = s.Addr2;
            t.Country = s.Country;
            t.State = s.State;
            t.City = s.City;
            t.District = s.District;
            t.Zip = s.Zip;
            t.Phone = s.Phone;
            t.Fax = s.Fax;
            t.EMailAddr = s.EMailAddr;
            t.BillName = s.BillName;
            t.BillAttn = s.BillAttn;
            t.BillSalut = s.BillSalut;
            t.BillAddr1 = s.BillAddr1;
            t.BillAddr2 = s.BillAddr2;
            t.BillCountry = s.BillCountry;
            t.BillState = s.BillState;
            t.BillCity = s.BillCity;
            t.BillZip = s.BillZip;
            t.BillPhone = s.BillPhone;
            t.BillFax = s.BillFax;
            t.TaxDflt = s.TaxDflt;
            t.TaxRegNbr = s.TaxRegNbr;
            t.TaxLocId = s.TaxLocId;
            t.TaxID00 = s.TaxID00;
            t.TaxID01 = s.TaxID01;
            t.TaxID02 = s.TaxID02;
            t.TaxID03 = s.TaxID03;
            t.InActive = s.InActive;
            t.SellProduct = s.SellProduct;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void Update_AR_CustHist(ref AR_CustHist t, AR_Customer s)
        {
            var objCustHist1 = _db.AR_CustHist.Where(p => p.BranchID == s.BranchID && p.CustID == s.CustId).OrderByDescending(p => p.Seq).FirstOrDefault();
            string strSeq = objCustHist1 == null ? "0000000001" : ("0000000000" + (double.Parse(objCustHist1.Seq) + 1));
            strSeq = strSeq.Substring(strSeq.Length - 10, 10);
            t.Seq = strSeq;
            t.BranchID = s.BranchID;
            t.CustID = s.CustId;
            t.Note = "Tạo mới khách hàng";
            t.FromDate = DateTime.Now.Short();
            t.ToDate = DateTime.Now.Short().AddYears(100);

            t.Crtd_DateTime = DateTime.Now;
            t.Crtd_Prog = _screenNbr;
            t.Crtd_User = _userName;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void UpdatingAR_SOAddress(ref AR_SOAddress t, AR20400_pdHeader_Result s)
        {
            t.SOName = s.CustName;
            t.Attn = s.Attn;
            t.Addr1 = s.Addr1;
            t.Addr2 = s.Addr2;
            t.City = s.City;
            t.State = s.State;
            t.District = s.District;
            t.Zip = s.Zip;
            t.Country = s.Country;
            t.Phone = s.Phone;
            t.Fax = s.Fax;
            t.TaxRegNbr = s.TaxRegNbr;
            t.TaxLocId = s.TaxLocId;
            t.TaxId00 = s.TaxID00;
            t.TaxId01 = s.TaxID01;
            t.TaxId02 = s.TaxID02;
            t.TaxId03 = s.TaxID03;
            t.Country = s.Country;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void UpdatingAR_CustAdvTool(AR_CustAdvTool t, AR20400_pgAR_CustAdvTool_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Active = s.Active;
            t.Amt = s.Amt;
            t.Descr = s.Descr;
            t.FitupDate = s.FitupDate;
            t.Status = s.Status;
            t.Qty = s.Qty;
            t.Type = s.Type;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }

        private void UpdatingAR_CustSellingProducts(AR_CustSellingProducts t, AR20400_pgAR_CustSellingProducts_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void UpdatingAR_CustDisplayMethod(AR_CustDisplayMethod t, AR20400_pgAR_CustDisplayMethod_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void UpdatingAR_LTTContract(AR_LTTContract t, AR20400_pgAR_LTTContract_Result s, bool isNew)
        {
            if (isNew)
            {
                t.LTTContractNbr = s.LTTContractNbr;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Active = s.Active;
            t.MonthNum = s.MonthNum;
            t.ExtDate = s.ExtDate;
            t.FromDate = s.FromDate;
            t.AmtCommit = s.AmtCommit;
            t.QtyCommit = s.QtyCommit;
            t.Status = s.Status;
            t.ToDate = s.ToDate;
            t.TotAmt = s.TotAmt;
            t.TotQty = s.TotQty;
            t.UnitCommit = s.UnitCommit;
            t.ActualQty = s.ActualQty;
            t.ActualAmt = s.ActualAmt;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void UpdatingAR_LTTContractDetail(AR_LTTContractDetail t, AR20400_pgAR_LTTContractDetail_Result s, bool isNew)
        {
            if (isNew)
            {
                t.LTTContractNbr = s.LTTContractNbr;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Amt = (double)s.Amt;
            t.Descr = s.Descr;
            t.Status = s.Status;
            t.Type = s.Type;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string CustId = data["cboCustId"].PassNull();
                string BranchID = data["cboCpnyID"].PassNull();

                var objDelete = _db.AR20400_ppCheckDeleteCust(CustId,BranchID).FirstOrDefault();
                if (objDelete != null)
                {
                    if (objDelete.PassNull() == "0")
                    {
                        var objAR_Customer = _db.AR_Customer.FirstOrDefault(p => p.BranchID == BranchID
                                                                   && p.CustId == CustId);
                        if (objAR_Customer != null)
                        {
                            _db.AR_Customer.DeleteObject(objAR_Customer);
                            if (!string.IsNullOrWhiteSpace(objAR_Customer.PhotoCode))
                            {

                                Util.UploadFile(FilePath, objAR_Customer.PhotoCode, null);

                            }
                        }

                        var objAR_SOAddress = _db.AR_SOAddress.FirstOrDefault(p => p.BranchID == BranchID
                                                                   && p.CustId == CustId);
                        if (objAR_SOAddress != null)
                        {
                            _db.AR_SOAddress.DeleteObject(objAR_SOAddress);
                        }

                        var lstAR_CustAdvTool = _db.AR_CustAdvTool.Where(p => p.CustID == CustId).ToList();
                        foreach (var item in lstAR_CustAdvTool)
                        {
                            _db.AR_CustAdvTool.DeleteObject(item);
                        }

                        var lstAR_CustSellingProducts = _db.AR_CustSellingProducts.Where(p => p.CustID == CustId).ToList();
                        foreach (var item in lstAR_CustSellingProducts)
                        {
                            _db.AR_CustSellingProducts.DeleteObject(item);
                        }

                        var lstAR_CustDisplayMethod = _db.AR_CustDisplayMethod.Where(p => p.CustID == CustId).ToList();
                        foreach (var item in lstAR_CustDisplayMethod)
                        {
                            _db.AR_CustDisplayMethod.DeleteObject(item);
                        }

                        var lstAR_LTTContract = _db.AR_LTTContract.Where(p => p.CustID == CustId).ToList();
                        foreach (var item in lstAR_LTTContract)
                        {
                            var lstAR_LTTContractDetail = _db.AR_LTTContractDetail.Where(p => p.LTTContractNbr == item.LTTContractNbr).ToList();
                            foreach (var item1 in lstAR_LTTContractDetail)
                            {
                                _db.AR_LTTContractDetail.DeleteObject(item1);
                            }

                            _db.AR_LTTContract.DeleteObject(item);
                        }
                        
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "18","");
                    }
                }
                return Json(new { success = true, CustId = "" });
            }
            catch (Exception ex)		
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, int z, string CpnyID)
        {
            var node = new Node();
            var k = -1;
            if (inactiveHierachy.Descr == "root")
            {
                node.Text = inactiveHierachy.Descr;
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeID", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeLevel", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "ParentRecordID", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "RecordID", Value = "", Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "CustID", Value = "", Mode = ParameterMode.Value });
            }
            else
            {
                node.Text = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
                node.NodeID = inactiveHierachy.NodeID + "-" + inactiveHierachy.NodeLevel + "-" + inactiveHierachy.ParentRecordID.ToString() + "-" + inactiveHierachy.RecordID;
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeID", Value = inactiveHierachy.NodeID, Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "NodeLevel", Value = inactiveHierachy.NodeLevel.ToString(), Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "ParentRecordID", Value = inactiveHierachy.ParentRecordID.ToString(), Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "RecordID", Value = inactiveHierachy.RecordID.ToString(), Mode = ParameterMode.Value });
                node.CustomAttributes.Add(new ConfigItem() { Name = "CustID", Value = "", Mode = ParameterMode.Value });
            }

            var tmps = _db.AR20400_ptCustomer(CpnyID)
                .Where(p => p.NodeID == inactiveHierachy.NodeID
                    && p.ParentRecordID == inactiveHierachy.ParentRecordID
                    && p.NodeLevel == level - 1).ToList();

            var childrenInactiveHierachies = _db.SI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.Type == inactiveHierachy.Type
                    && p.NodeLevel == level).ToList();

            if (tmps != null && tmps.Count > 0)
            {
                foreach (AR20400_ptCustomer_Result tmp in tmps)
                {
                    k++;
                    Node nodetmp = new Node();
                    nodetmp.Text = tmp.CustId + "-" + tmp.CustName;
                    nodetmp.NodeID = tmp.CustId + "-" + "|";
                    nodetmp.Leaf = true;
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "NodeID", Value = inactiveHierachy.NodeID, Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "NodeLevel", Value = inactiveHierachy.NodeLevel.ToString(), Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "ParentRecordID", Value = inactiveHierachy.ParentRecordID.ToString(), Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "RecordID", Value = inactiveHierachy.RecordID.ToString(), Mode = ParameterMode.Value });
                    nodetmp.CustomAttributes.Add(new ConfigItem() { Name = "CustID", Value = tmp.CustId.ToString(), Mode = ParameterMode.Value });
           
                    node.Children.Add(nodetmp);
                }
            }

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (SI_Hierarchy childrenInactiveNode in childrenInactiveHierachies)
                {
                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, z++, CpnyID));
                }
            }
            else
            {
                node.Leaf = false;
                if (tmps.Count == 0 && childrenInactiveHierachies.Count == 0)
                {
                    node.Leaf = true;
                }
                else
                {
                    node.Leaf = false;
                }
                
            }
            System.Diagnostics.Debug.WriteLine(node.Text);
            return node;
        }

        [DirectMethod]
        public ActionResult ReloadTreeAR20400(string CpnyID)
        {
            var root = new Node() { };
            var nodeType = "C";

            var hierarchy = new SI_Hierarchy()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "root",
                Type = nodeType
            };
            var z = 0;
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, z, CpnyID);

            //quan trong dung de refresh slmTree
            this.GetCmp<TreePanel>("treeCust").SetRootNode(node);
            return this.Direct();
        }

        //[HttpPost]
        //public ActionResult Report(FormCollection data)
        //{
        //    try
        //    {

        //        User user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
        //        string reportName = "";
        //        string reportNbr = "";
        //        var rpt = new RPTRunning();
        //        rpt.ResetET();

        //        reportName = data["reportName"];
        //        reportNbr = data["reportNbr"];

        //        rpt.ReportNbr = reportNbr;
        //        rpt.MachineName = "Web";
        //        rpt.ReportCap = "ReportName";
        //        rpt.ReportName = reportName;

        //        rpt.ReportDate = DateTime.Now;
        //        rpt.DateParm00 = DateTime.Now;
        //        rpt.DateParm01 = DateTime.Now;
        //        rpt.DateParm02 = DateTime.Now;
        //        rpt.DateParm03 = DateTime.Now;
        //        rpt.StringParm00 = data["cboBranchID_Header"];
        //        rpt.StringParm01 = data["cboContract"];
        //        rpt.UserID = Current.UserName;
        //        rpt.AppPath = "Reports\\";
        //        rpt.ClientName = Current.UserName;
        //        rpt.LoggedCpnyID = Current.CpnyID;
        //        rpt.CpnyID = user.CpnyID;
        //        rpt.LangID = Current.LangID;

        //        _db.RPTRunnings.AddObject(rpt);
        //        _db.SaveChanges();

        //        if (_logMessage != null)
        //        {
        //            return _logMessage;
        //        }
        //        return Json(new { success = true, reportID = rpt.ReportID, reportName = rpt.ReportName });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException)
        //        {
        //            return (ex as MessageException).ToMessage();
        //        }
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}

        public ActionResult ImageToBin(string fileName)
        {
            try
            {
                var imgString64 = Util.ImageToBin(FilePath, fileName);
                var jsonResult = Json(new { success = true, imgSrc = imgString64 }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
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

    }
}

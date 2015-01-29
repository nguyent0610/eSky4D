using eBiz4DWebFrame;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Excel;
using System.Drawing;
using System.Reflection;
using System.IO;
using eBiz4DWebSys;
using HQSendMailApprove;
namespace AR21600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21600Controller : Controller
    {
        string screenNbr = "AR21600";
        AR21600Entities _db = Util.CreateObjectContext<AR21600Entities>(false);
        eBiz4DWebSysEntities _sys = Util.CreateObjectContext<eBiz4DWebSysEntities>(true);
        
 

        public ActionResult Index()
        {
            return View();//.OrderBy(x => x.tstamp)
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {

            return PartialView();
        }

        public ActionResult GetDataGridDetailFromCust(String object1, String task, String fromBranch, String toBranch, String fromStatus,
            String toStatus, String langID)
        {

            var lst = _db.AR21600_ListChooseDetailFromCust(object1, task, fromBranch, toBranch, fromStatus, toStatus, Convert.ToInt16(langID)).ToList();


            return this.Store(lst);
        }


        public ActionResult GetDataGridDetailFromSls(String object1, String task, String fromBranch, String toBranch, String fromStatus,
           String toStatus, String langID)
        {

            var lst = _db.AR21600_ListChooseDetailFromSls(object1, task, fromBranch, toBranch, fromStatus, toStatus, Convert.ToInt16(langID)).ToList();


            return this.Store(lst);
        }

        public ActionResult GetDataGridDetailToCust(String object1, String task, String fromBranch, String toBranch, String fromStatus,
            String toStatus, String langID)
        {

            var lst = _db.AR21600_ListChooseDetailToCust(object1, task, fromBranch, toBranch, fromStatus, toStatus, Convert.ToInt16(langID)).ToList();


            return this.Store(lst);
        }

        public ActionResult GetDataGridDetailToSls(String object1, String task, String fromBranch, String toBranch, String fromStatus,
          String toStatus, String langID)
        {

            var lst = _db.AR21600_ListChooseDetailToSls(object1, task, fromBranch, toBranch, fromStatus, toStatus, Convert.ToInt16(langID)).ToList();


            return this.Store(lst);
        }

        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string object1,string task,string fromBranch,string toBranch,string fromStatus,string toStatus,string reason)
        {
            StoreDataHandler dataHandlerFromCust = new StoreDataHandler(data["lstgrdFromCust"]);
            StoreDataHandler dataHandlerFromSls = new StoreDataHandler(data["lstgrdFromSls"]);
            StoreDataHandler dataHandlerToCust = new StoreDataHandler(data["lstgrdToCust"]);
            StoreDataHandler dataHandlerToSls = new StoreDataHandler(data["lstgrdToSls"]);
            ChangeRecords<AR21600_ListChooseDetailFromCust_Result> lstFromCust = dataHandlerFromCust.BatchObjectData<AR21600_ListChooseDetailFromCust_Result>();
            ChangeRecords<AR21600_ListChooseDetailFromSls_Result> lstFromSls = dataHandlerFromSls.BatchObjectData<AR21600_ListChooseDetailFromSls_Result>();
            ChangeRecords<AR21600_ListChooseDetailToCust_Result> lstToCust = dataHandlerToCust.BatchObjectData<AR21600_ListChooseDetailToCust_Result>();
            ChangeRecords<AR21600_ListChooseDetailToSls_Result> lstToSls = dataHandlerToSls.BatchObjectData<AR21600_ListChooseDetailToSls_Result>();

            //var docDate = data["txtDocDate"];
            //khai bao bien tam AR_Customer
            var custId = "";
            var custName = "";
            var addr1 = "";
            var addr2 = "";
            var attn = "";
            var billAddr1 = "";
            var billAddr2 = "";
            var billAttn = "";
            var billCity = "";
            var billCountry = "";
            var billFax = "";
            var billName = "";
            var billPhone = "";
            var billSalut = "";
            var billState = "";
            var billZip = "";
            var channel = "";
            var city = "";
            var classId = "";
            var country = "";
            double crLmt = 0;
            var crRule = "";
            Int16 custFillPriority = 0;
            var custType = "";
            var deliveryID = "";
            var dflSaleRouteID = "";
            var dfltShipToId = "";
            var district = "";
            var eMailAddr = "";
            Int32 empNum = 0;
            DateTime expiryDate = DateTime.Now;
            Int16 exported = 0;
            var fax = "";
            Int16 gracePer = 0;
            var lTTContractNbr = "";
            var nodeID = "";
            Int16 nodeLevel = 0;
            Int32 parentRecordID = 0;
            var phone = "";
            var salut = "";
            var shopType = "";
            var siteId = "";
            var slsperId = "";
            var state = "";
            var status = "";
            var supID = "";
            var taxDflt = "";
            var taxID00 = "";
            var taxID01 = "";
            var taxID02 = "";
            var taxID03 = "";
            var taxLocId = "";
            var taxRegNbr = "";
            var terms = "";
            var territory = "";
            double tradeDisc = 0;
            var zip = "";
            var location = "";
            var area = "";
            Boolean giftExchange = false;
            Boolean hasPG = false;
            DateTime lUpd_Datetime = DateTime.Now;
            var lUpd_Prog = "";
            var lUpd_User = "";
            DateTime crtd_Datetime = DateTime.Now;
            var crtd_Prog = "";
            var crtd_User = "";
            var branchID = "";
            DateTime establishDate = DateTime.Now;
            DateTime birthdate = DateTime.Now;
            
            //khai bao bien tam AR_SalesPerson
            var slsperIdSls = "";
            var addr1Sls = "";
            var addr2Sls = "";
            double cmmnPctSls = 0;
            var countrySls = "";
            var eMailAddrSls = "";
            var faxSls = "";
            var nameSls = "";
            var phoneSls = "";
            var productGroupSls = "";
            var stateSls = "";
            var branchIDSls = "";
            var deliveryManSls = "";
            var positionSls = "";
            DateTime lUpd_DateTimeSls = DateTime.Now;
            var lUpd_ProgSls = "";
            var lUpd_UserSls = "";
            DateTime crtd_DateTimeSls = DateTime.Now;
            var crtd_ProgSls = "";
            var crtd_UserSls = "";
            var districtSls = "";
            var supIDSls = "";
            Boolean activeSls = false ;
            var pPCPasswordSls = "";
            Boolean pPCStorePicReqSls = false;
            var vendIDSls = "";
            var statusSls = "";
            Boolean pPCAdminSls = false;
            var imagesSls = "";


            //bien tam list obj de bat may cai thay doi
            var listObj = "";
            foreach (AR21600_ListChooseDetailFromCust_Result deleted in lstFromCust.Deleted)
            {
                var recordFromCust = _db.AR_Customer.Where(p => p.CustId == deleted.CustId && p.BranchID == fromBranch && p.Status == fromStatus).FirstOrDefault();
                if (recordFromCust != null)
                {
                    if (toBranch != "")
                    {
                        //recordFromCust.BranchID = toBranch;
                        custId = recordFromCust.CustId;
                        custName = recordFromCust.CustName;
                        addr1 = recordFromCust.Addr1;
                        addr2 = recordFromCust.Addr2;
                        attn = recordFromCust.Attn;
                        billAddr1 = recordFromCust.BillAddr1;
                        billAddr2 = recordFromCust.BillAddr2;
                        billAttn = recordFromCust.BillAttn;
                        billCity = recordFromCust.BillCity;
                        billCountry = recordFromCust.BillCountry;
                        billFax = recordFromCust.BillFax;
                        billName = recordFromCust.BillName;
                        billPhone = recordFromCust.BillPhone;
                        billSalut = recordFromCust.BillSalut;
                        billState = recordFromCust.BillState;
                        billZip = recordFromCust.BillZip;
                        channel = recordFromCust.Channel;
                        city = recordFromCust.City;
                        classId = recordFromCust.ClassId;
                        country = recordFromCust.Country;
                        crLmt = recordFromCust.CrLmt;
                        crRule = recordFromCust.CrRule;
                        custFillPriority = recordFromCust.CustFillPriority;
                        custType = recordFromCust.CustType;
                        deliveryID = recordFromCust.DeliveryID;
                        dflSaleRouteID = recordFromCust.DflSaleRouteID;
                        dfltShipToId = recordFromCust.DfltShipToId;
                        district = recordFromCust.District;
                        eMailAddr = recordFromCust.EMailAddr;
                        empNum = recordFromCust.EmpNum;
                        expiryDate = recordFromCust.ExpiryDate;
                        exported = recordFromCust.Exported;
                        fax = recordFromCust.Fax;
                        gracePer = recordFromCust.GracePer;
                        lTTContractNbr = recordFromCust.LTTContractNbr;
                        nodeID = recordFromCust.NodeID;
                        nodeLevel = recordFromCust.NodeLevel;
                        parentRecordID = recordFromCust.ParentRecordID;
                        phone = recordFromCust.Phone;
                        salut = recordFromCust.Salut;
                        shopType = recordFromCust.ShopType;
                        siteId = recordFromCust.SiteId;
                        slsperId = recordFromCust.SlsperId;
                        state = recordFromCust.State;
                        status = recordFromCust.Status;
                        supID = recordFromCust.SupID;
                        taxDflt = recordFromCust.TaxDflt;
                        taxID00 = recordFromCust.TaxID00;
                        taxID01 = recordFromCust.TaxID01;
                        taxID02 = recordFromCust.TaxID02;
                        taxID03 = recordFromCust.TaxID03;
                        taxLocId = recordFromCust.TaxLocId;
                        taxRegNbr = recordFromCust.TaxRegNbr;
                        terms = recordFromCust.Terms;
                        territory = recordFromCust.Territory;
                        tradeDisc = recordFromCust.TradeDisc;
                        zip = recordFromCust.Zip;
                        location = recordFromCust.Location;
                        area = recordFromCust.Area;
                        giftExchange = recordFromCust.GiftExchange;
                        hasPG = recordFromCust.HasPG;
                        lUpd_Datetime = recordFromCust.LUpd_Datetime;
                        lUpd_Prog = recordFromCust.LUpd_Prog;
                        lUpd_User = recordFromCust.LUpd_User;
                        crtd_Datetime = recordFromCust.Crtd_Datetime;
                        crtd_Prog = recordFromCust.Crtd_Prog;
                        crtd_User = recordFromCust.Crtd_User;
                        branchID = recordFromCust.BranchID;
                        establishDate = Convert.ToDateTime(recordFromCust.EstablishDate);
                        birthdate = Convert.ToDateTime(recordFromCust.Birthdate);
                        _db.AR_Customer.DeleteObject(recordFromCust);
                        _db.SaveChanges();
                    }
                    else if (toStatus != "")
                    {
                        recordFromCust.Status = toStatus;
                        _db.SaveChanges();
                    }
                    

                }


                //bat cac record duoc tao moi chuyen wa Customer khac
                foreach (AR21600_ListChooseDetailToCust_Result created in lstToCust.Created)
                {

                    var recordToCust = _db.AR_Customer.Where(p => p.CustId == created.ToCustId && p.BranchID == toBranch && p.Status == fromStatus).FirstOrDefault();
                    if (recordToCust == null)
                    {
                        if (toBranch != "")
                        {
                            recordToCust = new AR_Customer();
                            recordToCust.CustId = created.ToCustId;//luu y
                            listObj += created.ToCustId + ",";
                            recordToCust.CustName = custName;
                            recordToCust.Addr1 = addr1;
                            recordToCust.Addr2 = addr2;
                            recordToCust.Attn = attn;
                            recordToCust.BillAddr1 = billAddr1;
                            recordToCust.BillAddr2 = billAddr2;
                            recordToCust.BillAttn = billAttn;
                            recordToCust.BillCity = billCity;
                            recordToCust.BillCountry = billCountry;
                            recordToCust.BillFax = billFax;
                            recordToCust.BillName = billName;
                            recordToCust.BillPhone = billPhone;
                            recordToCust.BillSalut = billSalut;
                            recordToCust.BillState = billState;
                            recordToCust.BillZip = billZip;
                            recordToCust.Channel = channel;
                            recordToCust.City = city;
                            recordToCust.ClassId = classId;
                            recordToCust.Country = country;
                            recordToCust.CrLmt = crLmt;
                            recordToCust.CrRule = crRule;
                            recordToCust.CustFillPriority = custFillPriority;
                            recordToCust.CustType = custType;
                            recordToCust.DeliveryID = deliveryID;
                            recordToCust.DflSaleRouteID = dflSaleRouteID;
                            recordToCust.DfltShipToId = dfltShipToId;
                            recordToCust.District = district;
                            recordToCust.EMailAddr = eMailAddr;
                            recordToCust.EmpNum = empNum;
                            recordToCust.ExpiryDate = expiryDate.ToDateShort();
                            recordToCust.Exported = exported;
                            recordToCust.Fax = fax;
                            recordToCust.GracePer = gracePer;
                            recordToCust.LTTContractNbr = lTTContractNbr;
                            recordToCust.NodeID = nodeID;
                            recordToCust.NodeLevel = nodeLevel;
                            recordToCust.ParentRecordID = parentRecordID;
                            recordToCust.Phone = phone;
                            recordToCust.Salut = salut;
                            recordToCust.ShopType = shopType;
                            recordToCust.SiteId = siteId;
                            recordToCust.SlsperId = slsperId;
                            recordToCust.State = state;
                            recordToCust.Status = status;
                            recordToCust.SupID = supID;
                            recordToCust.TaxDflt = taxDflt;
                            recordToCust.TaxID00 = taxID00;
                            recordToCust.TaxID01 = taxID01;
                            recordToCust.TaxID02 = taxID02;
                            recordToCust.TaxID03 = taxID03;
                            recordToCust.TaxLocId = taxLocId;
                            recordToCust.TaxRegNbr = taxRegNbr;
                            recordToCust.Terms = terms;
                            recordToCust.Territory = territory;
                            recordToCust.TradeDisc = tradeDisc;
                            recordToCust.Zip = zip;
                            recordToCust.Location = location;
                            recordToCust.Area = area;
                            recordToCust.GiftExchange = giftExchange;
                            recordToCust.HasPG = hasPG;
                            recordToCust.LUpd_Datetime = lUpd_Datetime;
                            recordToCust.LUpd_Prog = lUpd_Prog;
                            recordToCust.LUpd_User = lUpd_User;
                            recordToCust.Crtd_Datetime = crtd_Datetime;
                            recordToCust.Crtd_Prog = crtd_Prog;
                            recordToCust.Crtd_User = crtd_User;
                            recordToCust.BranchID = toBranch; // luu y
                            recordToCust.EstablishDate = establishDate.ToDateShort();
                            recordToCust.Birthdate = birthdate.ToDateShort();
                            _db.AR_Customer.AddObject(recordToCust);
                            _db.SaveChanges();
                        }
                    }
                }
                //ham nay chi tac dung neu khi chuyen wa grid To roi chuyen nguoc lai Grid From
                foreach (AR21600_ListChooseDetailFromCust_Result created in lstFromCust.Created)
                {
                    var recordFromCustAgain = _db.AR_Customer.Where(p => p.CustId == created.CustId && p.BranchID == fromBranch && p.Status == fromStatus).FirstOrDefault();
                    if (recordFromCustAgain == null)
                    {
                        if (toBranch != "")
                        {
                            recordFromCustAgain = new AR_Customer();
                            recordFromCustAgain.CustId = created.CustId;//luu y
                            recordFromCustAgain.CustName = custName;
                            recordFromCustAgain.Addr1 = addr1;
                            recordFromCustAgain.Addr2 = addr2;
                            recordFromCustAgain.Attn = attn;
                            recordFromCustAgain.BillAddr1 = billAddr1;
                            recordFromCustAgain.BillAddr2 = billAddr2;
                            recordFromCustAgain.BillAttn = billAttn;
                            recordFromCustAgain.BillCity = billCity;
                            recordFromCustAgain.BillCountry = billCountry;
                            recordFromCustAgain.BillFax = billFax;
                            recordFromCustAgain.BillName = billName;
                            recordFromCustAgain.BillPhone = billPhone;
                            recordFromCustAgain.BillSalut = billSalut;
                            recordFromCustAgain.BillState = billState;
                            recordFromCustAgain.BillZip = billZip;
                            recordFromCustAgain.Channel = channel;
                            recordFromCustAgain.City = city;
                            recordFromCustAgain.ClassId = classId;
                            recordFromCustAgain.Country = country;
                            recordFromCustAgain.CrLmt = crLmt;
                            recordFromCustAgain.CrRule = crRule;
                            recordFromCustAgain.CustFillPriority = custFillPriority;
                            recordFromCustAgain.CustType = custType;
                            recordFromCustAgain.DeliveryID = deliveryID;
                            recordFromCustAgain.DflSaleRouteID = dflSaleRouteID;
                            recordFromCustAgain.DfltShipToId = dfltShipToId;
                            recordFromCustAgain.District = district;
                            recordFromCustAgain.EMailAddr = eMailAddr;
                            recordFromCustAgain.EmpNum = empNum;
                            recordFromCustAgain.ExpiryDate = expiryDate.ToDateShort();
                            recordFromCustAgain.Exported = exported;
                            recordFromCustAgain.Fax = fax;
                            recordFromCustAgain.GracePer = gracePer;
                            recordFromCustAgain.LTTContractNbr = lTTContractNbr;
                            recordFromCustAgain.NodeID = nodeID;
                            recordFromCustAgain.NodeLevel = nodeLevel;
                            recordFromCustAgain.ParentRecordID = parentRecordID;
                            recordFromCustAgain.Phone = phone;
                            recordFromCustAgain.Salut = salut;
                            recordFromCustAgain.ShopType = shopType;
                            recordFromCustAgain.SiteId = siteId;
                            recordFromCustAgain.SlsperId = slsperId;
                            recordFromCustAgain.State = state;
                            recordFromCustAgain.Status = status;
                            recordFromCustAgain.SupID = supID;
                            recordFromCustAgain.TaxDflt = taxDflt;
                            recordFromCustAgain.TaxID00 = taxID00;
                            recordFromCustAgain.TaxID01 = taxID01;
                            recordFromCustAgain.TaxID02 = taxID02;
                            recordFromCustAgain.TaxID03 = taxID03;
                            recordFromCustAgain.TaxLocId = taxLocId;
                            recordFromCustAgain.TaxRegNbr = taxRegNbr;
                            recordFromCustAgain.Terms = terms;
                            recordFromCustAgain.Territory = territory;
                            recordFromCustAgain.TradeDisc = tradeDisc;
                            recordFromCustAgain.Zip = zip;
                            recordFromCustAgain.Location = location;
                            recordFromCustAgain.Area = area;
                            recordFromCustAgain.GiftExchange = giftExchange;
                            recordFromCustAgain.HasPG = hasPG;
                            recordFromCustAgain.LUpd_Datetime = lUpd_Datetime;
                            recordFromCustAgain.LUpd_Prog = lUpd_Prog;
                            recordFromCustAgain.LUpd_User = lUpd_User;
                            recordFromCustAgain.Crtd_Datetime = crtd_Datetime;
                            recordFromCustAgain.Crtd_Prog = crtd_Prog;
                            recordFromCustAgain.Crtd_User = crtd_User;
                            recordFromCustAgain.BranchID = fromBranch; // luu y
                            recordFromCustAgain.EstablishDate = establishDate.ToDateShort();
                            recordFromCustAgain.Birthdate = birthdate.ToDateShort();
                            _db.AR_Customer.AddObject(recordFromCustAgain);
                            _db.SaveChanges();
                        }
                    }
                }


            //ngoac ket thuc foreach deleted FromCust
            }

            foreach (AR21600_ListChooseDetailFromSls_Result deleted in lstFromSls.Deleted)
            {
                var recordFromSls = _db.AR_Salesperson.Where(p => p.SlsperId == deleted.SlsperId && p.BranchID == fromBranch && p.Status == fromStatus).FirstOrDefault();
                if (recordFromSls != null)
                {
                    if (toBranch != "")
                    {

                        slsperIdSls = "";
                        addr1Sls = "";
                        addr2Sls = "";
                        cmmnPctSls = 0;
                        countrySls = "";
                        eMailAddrSls = "";
                        faxSls = "";
                        nameSls = "";
                        phoneSls = "";
                        productGroupSls = "";
                        stateSls = "";
                        branchIDSls = "";
                        deliveryManSls = "";
                        positionSls = "";
                        lUpd_DateTimeSls = DateTime.Now;
                        lUpd_ProgSls = "";
                        lUpd_UserSls = "";
                        crtd_DateTimeSls = DateTime.Now;
                        crtd_ProgSls = "";
                        crtd_UserSls = "";
                        districtSls = "";
                        supIDSls = "";
                        activeSls = false;
                        pPCPasswordSls = "";
                        pPCStorePicReqSls = false;
                        vendIDSls = "";
                        statusSls = "";
                        pPCAdminSls = false;
                        imagesSls = "";
                        _db.AR_Salesperson.DeleteObject(recordFromSls);
                        _db.SaveChanges();
                    }
                    else if (toStatus != "")
                    {
                        recordFromSls.Status = toStatus;
                        _db.SaveChanges();
                    }
                }

                foreach (AR21600_ListChooseDetailToSls_Result created in lstToSls.Created)
                {
                    var recordToSls = _db.AR_Salesperson.Where(p => p.SlsperId == created.ToSlsId && p.BranchID == toBranch && p.Status == fromStatus).FirstOrDefault();
                    if (recordToSls != null)
                    {
                        if (toBranch != "")
                        {
                            recordToSls = new AR_Salesperson();
                            recordToSls.SlsperId = created.ToSlsId;//luu y
                            listObj += created.ToSlsId + ",";
                            recordToSls.Addr1 = addr1Sls;
                            recordToSls.Addr2 = addr2Sls;
                            recordToSls.CmmnPct = cmmnPctSls;
                            recordToSls.Country = countrySls;
                            recordToSls.EMailAddr = eMailAddrSls;
                            recordToSls.Fax = faxSls;
                            recordToSls.Name = nameSls;
                            recordToSls.Phone = phoneSls;
                            recordToSls.ProductGroup = productGroupSls;
                            recordToSls.State = stateSls;
                            recordToSls.BranchID = toBranch;// luu y
                            recordToSls.DeliveryMan = deliveryManSls;
                            recordToSls.Position = positionSls;
                            recordToSls.LUpd_DateTime = lUpd_DateTimeSls;
                            recordToSls.LUpd_Prog = lUpd_ProgSls;
                            recordToSls.LUpd_User = lUpd_UserSls;
                            recordToSls.Crtd_DateTime = crtd_DateTimeSls;
                            recordToSls.Crtd_Prog = crtd_ProgSls;
                            recordToSls.Crtd_User = crtd_UserSls;
                            recordToSls.District = districtSls;
                            recordToSls.SupID = supIDSls;
                            recordToSls.Active = activeSls;
                            recordToSls.PPCPassword = pPCPasswordSls;
                            recordToSls.PPCStorePicReq = pPCStorePicReqSls;
                            recordToSls.VendID = vendIDSls;
                            recordToSls.Status = statusSls;
                            recordToSls.PPCAdmin = pPCAdminSls;
                            recordToSls.Images = imagesSls;
          
                            _db.AR_Salesperson.AddObject(recordFromSls);
                            _db.SaveChanges();
                        }
                        //else if (toStatus != "")
                        //{
                        //    recordFromSls.Status = toStatus;
                        //    _db.SaveChanges();
                        //}

                    }

                //ngoac ket thuc created ToSls
                }


                //ham nay chi tac dung neu khi chuyen wa grid To roi chuyen nguoc lai Grid From
                foreach (AR21600_ListChooseDetailFromSls_Result created in lstFromSls.Created)
                {
                    var recordFromSlsAgain = _db.AR_Salesperson.Where(p => p.SlsperId == created.SlsperId && p.BranchID == fromBranch && p.Status == fromStatus).FirstOrDefault();
                    if (recordFromSlsAgain != null)
                    {
                        if (toBranch != "")
                        {
                            recordFromSlsAgain = new AR_Salesperson();
                            recordFromSlsAgain.SlsperId = created.SlsperId;//luu y
                            recordFromSlsAgain.Addr1 = addr1Sls;
                            recordFromSlsAgain.Addr2 = addr2Sls;
                            recordFromSlsAgain.CmmnPct = cmmnPctSls;
                            recordFromSlsAgain.Country = countrySls;
                            recordFromSlsAgain.EMailAddr = eMailAddrSls;
                            recordFromSlsAgain.Fax = faxSls;
                            recordFromSlsAgain.Name = nameSls;
                            recordFromSlsAgain.Phone = phoneSls;
                            recordFromSlsAgain.ProductGroup = productGroupSls;
                            recordFromSlsAgain.State = stateSls;
                            recordFromSlsAgain.BranchID = fromBranch;//luu y
                            recordFromSlsAgain.DeliveryMan = deliveryManSls;
                            recordFromSlsAgain.Position = positionSls;
                            recordFromSlsAgain.LUpd_DateTime = lUpd_DateTimeSls;
                            recordFromSlsAgain.LUpd_Prog = lUpd_ProgSls;
                            recordFromSlsAgain.LUpd_User = lUpd_UserSls;
                            recordFromSlsAgain.Crtd_DateTime = crtd_DateTimeSls;
                            recordFromSlsAgain.Crtd_Prog = crtd_ProgSls;
                            recordFromSlsAgain.Crtd_User = crtd_UserSls;
                            recordFromSlsAgain.District = districtSls;
                            recordFromSlsAgain.SupID = supIDSls;
                            recordFromSlsAgain.Active = activeSls;
                            recordFromSlsAgain.PPCPassword = pPCPasswordSls;
                            recordFromSlsAgain.PPCStorePicReq = pPCStorePicReqSls;
                            recordFromSlsAgain.VendID = vendIDSls;
                            recordFromSlsAgain.Status = statusSls;
                            recordFromSlsAgain.PPCAdmin = pPCAdminSls;
                            recordFromSlsAgain.Images = imagesSls;

                            _db.AR_Salesperson.AddObject(recordFromSls);
                            _db.SaveChanges();
                        }
                        //else if (toStatus != "")
                        //{
                        //    recordFromSls.Status = toStatus;
                        //    _db.SaveChanges();
                        //}

                    }

                    //ngoac ket thuc created ToSls
                }





            //ngoac ket thuc deleted FromSls
            }


            _db.SaveChanges();

            string brandID = Current.CpnyID;
            string lang = Current.LangID.ToString();
            string userNameLogin = Current.UserName;
            var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            var Role = user.UserTypes;
            //send mail thong bao thay doi 
            if (lstToCust.Created.Count != 0 || lstToSls.Created.Count != 0)
            {
                X.Msg.Show(new MessageBoxConfig()
                {
                    Message = "Email sent!"
                });
                Approve.Mail_Approve(screenNbr, userNameLogin, object1, task, fromBranch,
                             toBranch, fromStatus, toStatus,
                             listObj, reason, Role, lang);
            }


            return Json(new { success = true, value =  0}, JsonRequestBehavior.AllowGet);


        }

    }
}

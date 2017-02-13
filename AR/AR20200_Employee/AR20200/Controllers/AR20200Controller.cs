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
namespace AR20200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20200Controller : Controller
    {
        private string _screenNbr = "AR20200";
        private string _beginStatus = "H";
        //private string _noneStatus = "N";
        private string _mt = "MT";
        AR20200Entities _db = Util.CreateObjectContext<AR20200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadAR20200");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\AR20200");
                }
                return _filePath;
            }
        }

        //
        // GET: /AR20200/
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

        public ActionResult GetSalesPersonById(string branchId, string slsperid)
        {
            var slsper = _db.AR_Salesperson.FirstOrDefault(x => x.BranchID == branchId && x.SlsperId == slsperid);
            return this.Store(slsper);
        }

        public ActionResult GetSlsperCpnyAddr(string branchId, string slsperid)
        {
            var slsperCpnyAddrs = _db.AR20200_pgSlsperCpnyAddr(Current.UserName, branchId, slsperid).ToList();
            return this.Store(slsperCpnyAddrs);
        }

        [DirectMethod]
        public ActionResult GetTreeCpnyAddr(string panelID)
        {
            TreePanel tree = new TreePanel();
            tree.ID = "treePanelCpnyAddr";
            tree.ItemID = "treePanelCpnyAddr";

            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
            tree.Root.Add(node);

            var lstCpnyMT = _db.AR20200_ptCpnyByChannel(Current.UserName, _mt).ToList(); //danh sach tat ca cpny co MT

            foreach (var item in lstCpnyMT)
            {
                var nodeCpny = new Node();
                nodeCpny.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = item.CpnyID, Mode = ParameterMode.Value });
                nodeCpny.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Cpny", Mode = ParameterMode.Value });
                //nodeTerritory.Cls = "tree-node-parent";
                nodeCpny.Text = item.CpnyName;
                nodeCpny.Checked = false;
                nodeCpny.NodeID = "cpny-" + item.CpnyID;
                nodeCpny.Qtip = item.CpnyID;
                //nodeCpny.IconCls = "tree-parent-icon";

                var lstAddrsInCpny = _db.AR20200_ptCpnyAddr(Current.UserName, item.CpnyID).ToList();
                foreach (var addr in lstAddrsInCpny)
                {
                    var nodeCpnyAddr = new Node();
                    nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = addr.AddrID, Mode = ParameterMode.Value });
                    nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Addr", Mode = ParameterMode.Value });
                    nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "AddrName", Value = addr.Name, Mode = ParameterMode.Value });
                    nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "Addr1", Value = addr.Addr1, Mode = ParameterMode.Value });
                    //nodeCompany.Cls = "tree-node-parent";
                    nodeCpnyAddr.Text = addr.Name;
                    nodeCpnyAddr.Checked = false;
                    nodeCpnyAddr.Leaf = true;
                    nodeCpnyAddr.NodeID = "cpny-addr-" + item.CpnyID + "-" + addr.AddrID;
                    nodeCpnyAddr.Qtip = addr.Addr1;
                    //nodeCompany.IconCls = "tree-parent-icon";

                    nodeCpny.Children.Add(nodeCpnyAddr);

                }
                if (lstAddrsInCpny.Count() == 0)
                {
                    nodeCpny.Leaf = true;
                    nodeCpny.Icon = Icon.Folder;
                }
                node.Children.Add(nodeCpny);
            }

            var treeCpnyAddr = X.GetCmp<Panel>(panelID);

            //tree.Listeners.ItemClick.Fn = "DiscDefintion.nodeClick";
            tree.Listeners.CheckChange.Fn = "treePanelCpnyAddr_checkChange";

            tree.AddTo(treeCpnyAddr);

            return this.Direct();
        }

        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data, bool isNew, string channel, string HandleCombo)
        {
            try
            {
                string slsperID = data["cboSlsperid"].PassNull();
                string branchID = data["cboBranchID"].PassNull();
                string Status = data["cboStatus"].PassNull();
                string Handle = data["cboHandle"].PassNull();
                if (!string.IsNullOrWhiteSpace(branchID))
                {
                    //var slsperHandler = new StoreDataHandler(data["lstSalesPerson"]);
                    //var inputSlsper = slsperHandler.ObjectData<AR_Salesperson>()
                    //            .FirstOrDefault(p => p.SlsperId == slsperID && p.BranchID == branchID);

                    StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSalesPerson"]);
                    var inputSlsper = dataHandler1.ObjectData<AR_Salesperson>().FirstOrDefault();
                    //if (inputSlsper != null)
                    //{
                    #region Slsperson info
                    var slsper = _db.AR_Salesperson.FirstOrDefault(x => x.BranchID == branchID && x.SlsperId == slsperID);
                    if (slsper != null)
                    {
                        if (!isNew)
                        {
                            // update
                            if (slsper.tstamp.ToHex() == inputSlsper.tstamp.ToHex())
                            {
                                updateSlsper(ref slsper, inputSlsper, Status, Handle, slsperID, branchID, HandleCombo);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {

                        slsperID = slsperID.PassNull() == "" ? _db.AR20200_pdAutoSlsperID(branchID, Current.UserName, inputSlsper.State, inputSlsper.District).FirstOrDefault().PassNull() : slsperID;
                        // Create slsper
                        slsper = new AR_Salesperson();
                        slsper.ResetET();
                        slsper.BranchID = branchID;
                        slsper.SlsperId = slsperID;
                        slsper.Crtd_DateTime = DateTime.Now;
                        slsper.Crtd_Prog = _screenNbr;
                        slsper.Crtd_User = Current.UserName;
                        updateSlsper(ref slsper, inputSlsper, Status, Handle, slsperID, branchID, HandleCombo);
                        _db.AR_Salesperson.AddObject(slsper);
                        AddSalesPerHist(slsper);
                    }
                    #endregion

                    #region Slsperson Cpny Addr
                    if (channel == _mt)
                    {
                        var cpnyAddrHandler = new StoreDataHandler(data["lstSlsperCpnyAddr"]);
                        var lstSlsperCpnyAddr = cpnyAddrHandler.BatchObjectData<AR20200_pgSlsperCpnyAddr_Result>();

                        foreach (var created in lstSlsperCpnyAddr.Created)
                        {
                            if (!string.IsNullOrWhiteSpace(created.CpnyAddrID))
                            {
                                created.BranchID = branchID;
                                created.SlsperID = slsperID;

                                var createdCpnyAddr = _db.AR_SalespersonCpnyAddr.FirstOrDefault(
                                    x => x.CpnyAddrID == created.CpnyAddrID
                                        && x.BranchID == created.BranchID
                                        && x.SlsperID == created.SlsperID);
                                if (createdCpnyAddr == null)
                                {
                                    updateSlsperCpnyAddr(ref createdCpnyAddr, created, true);
                                    _db.AR_SalespersonCpnyAddr.AddObject(createdCpnyAddr);
                                }
                            }
                        }

                        foreach (var updated in lstSlsperCpnyAddr.Updated)
                        {
                            if (!string.IsNullOrWhiteSpace(updated.CpnyAddrID))
                            {
                                updated.BranchID = branchID;
                                updated.SlsperID = slsperID;

                                var updatedCpnyAddr = _db.AR_SalespersonCpnyAddr.FirstOrDefault(
                                    x => x.CpnyAddrID == updated.CpnyAddrID
                                        && x.BranchID == updated.BranchID
                                        && x.SlsperID == updated.SlsperID);
                                if (updatedCpnyAddr != null)
                                {
                                    updateSlsperCpnyAddr(ref updatedCpnyAddr, updated, false);
                                }
                            }
                        }

                        foreach (var deleted in lstSlsperCpnyAddr.Deleted)
                        {
                            if (!string.IsNullOrWhiteSpace(deleted.CpnyAddrID))
                            {
                                deleted.BranchID = branchID;
                                deleted.SlsperID = slsperID;

                                var deletedCpnyAddr = _db.AR_SalespersonCpnyAddr.FirstOrDefault(
                                    x => x.CpnyAddrID == deleted.CpnyAddrID
                                        && x.BranchID == deleted.BranchID
                                        && x.SlsperID == deleted.SlsperID);
                                if (deletedCpnyAddr != null)
                                {
                                    _db.AR_SalespersonCpnyAddr.DeleteObject(deletedCpnyAddr);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Upload files
                    var files = Request.Files;
                    if (files.Count > 0 && files[0].ContentLength > 0) // Co chon file de upload
                    {
                     
                        //// Xoa file cu di
                        //var oldPath = string.Format("{0}\\{1}", FilePath, slsper.Images);
                        //if (System.IO.File.Exists(oldPath))
                        //{
                        //    System.IO.File.Delete(oldPath);
                        //}
                       
                        //// Upload file moi
                        //string newFileName = string.Format("{0}_{1}{2}", branchID, slsperID, Path.GetExtension(files[0].FileName));
                        //files[0].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                        string newFileName = string.Format("{0}_{1}{2}", branchID, slsperID, Path.GetExtension(files[0].FileName));
                        Util.UploadFile(FilePath, newFileName, files[0]);
                        slsper.Images = newFileName;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(slsper.Images) && string.IsNullOrWhiteSpace(inputSlsper.Images))
                        {
                            //// Xoa file cu di
                            //var oldPath = string.Format("{0}\\{1}", FilePath, slsper.Images);
                            //if (System.IO.File.Exists(oldPath))
                            //{
                            //    System.IO.File.Delete(oldPath);
                            //}
                            Util.UploadFile(FilePath, slsper.Images, null);
                            slsper.Images = string.Empty;
                        }
                    }
                    #endregion

                    _db.SaveChanges();

                    return Json(new { success = true, msgCode = 201405071, slsperID = slsperID });
                    //}
                    //else
                    //{
                    //    throw new MessageException(MessageType.Message, "1555");
                    //}
                }
                else
                {
                    throw new MessageException(MessageType.Message, "1000", "",
                        new string[]{
                            string.Format("{0}", Util.GetLang("BranchID"))
                        });
                }
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

        private void updateSlsperCpnyAddr(ref AR_SalespersonCpnyAddr createdCpnyAddr, AR20200_pgSlsperCpnyAddr_Result created, bool isNew)
        {
            if (isNew)
            {
                createdCpnyAddr = new AR_SalespersonCpnyAddr();
                createdCpnyAddr.CpnyAddrID = created.CpnyAddrID;
                createdCpnyAddr.BranchID = created.BranchID;
                createdCpnyAddr.SlsperID = created.SlsperID;

                createdCpnyAddr.Crtd_DateTime = DateTime.Now;
                createdCpnyAddr.Crtd_Prog = _screenNbr;
                createdCpnyAddr.Crtd_User = Current.UserName;
            }
            createdCpnyAddr.LUpd_DateTime = DateTime.Now;
            createdCpnyAddr.LUpd_Prog = _screenNbr;
            createdCpnyAddr.LUpd_User = Current.UserName;
        }

        public ActionResult Delete(string slsperID, string branchID, bool isNew)
        {
            try
            {
                var check_AR_Doc = _db.AR_Doc.Where(p => p.SlsperId == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_AR_SalesOrderHeader = _db.AR_SalesOrderHeader.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_UserDefault = _db.OM_UserDefault.Where(p => p.DfltSlsPerID == slsperID && p.DfltBranchID == branchID).FirstOrDefault();
                var check_AR_RedInvoiceDoc = _db.AR_RedInvoiceDoc.Where(p => p.SlsPerID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_IN_StockRecoveryCust = _db.IN_StockRecoveryCust.Where(p => p.SlsPerID == slsperID && p.BranchID == branchID).FirstOrDefault();

                var check_AR_SalesPerson_Map_IN_ProductClass = _db.AR_SalesPerson_Map_IN_ProductClass.Where(p => p.SlsperId == slsperID).FirstOrDefault();

                var check_OM_TDisplayImage = _db.OM_TDisplayImage.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_AR_SalespersonCpnyAddr = _db.AR_SalespersonCpnyAddr.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();

                var check_AR_Setup = _db.AR_Setup.Where(p => p.BranchID == branchID).FirstOrDefault();

                var check_OM_Invoice = _db.OM_Invoice.Where(p => p.SlsPerID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_TDisplayCustomerHist = _db.OM_TDisplayCustomerHist.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_IN_POSMCust = _db.IN_POSMCust.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_AR_SalespersonLocationTrace = _db.AR_SalespersonLocationTrace.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_TBonusCustomer = _db.OM_TBonusCustomer.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_AR_Customer = _db.AR_Customer.Where(p => p.SlsperId == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_OrdDisc = _db.OM_OrdDisc.Where(p => p.SlsPerID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_SalesRouteDet = _db.OM_SalesRouteDet.Where(p => p.SlsPerID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_PDAOrdDisc = _db.OM_PDAOrdDisc.Where(p => p.SlsPerID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_FCS = _db.OM_FCS.Where(p => p.SlsperId == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_SalesOrd = _db.OM_SalesOrd.Where(p => p.SlsPerID == slsperID && p.BranchID == branchID).FirstOrDefault();

                var check_OM_CustDisD = _db.OM_CustDisD.Where(p => p.SlsperID == slsperID).FirstOrDefault();

                var check_PO_Header = _db.PO_Header.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();
                var check_OM_DiscClaimOrd = _db.OM_DiscClaimOrd.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();

                var check_OM_SalPerObj = _db.OM_SalPerObj.Where(p => p.SlsPerID == slsperID).FirstOrDefault();

                var check_AR_NewCustomerInfor = _db.AR_NewCustomerInfor.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();

                var check_IN_POSM = _db.IN_POSM.Where(p => p.SlsPerId == slsperID).FirstOrDefault();

                //var check_OM_KPISell_Detail = _db.OM_KPISell_Detail.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).FirstOrDefault();

                if (check_AR_Doc == null && check_AR_SalesOrderHeader == null && check_OM_UserDefault == null &&
                   check_AR_RedInvoiceDoc == null && check_IN_StockRecoveryCust == null && check_AR_SalesPerson_Map_IN_ProductClass == null &&
                    check_OM_TDisplayImage == null && check_AR_SalespersonCpnyAddr == null && check_OM_Invoice == null &&
                    check_OM_TDisplayCustomerHist == null && check_IN_POSMCust == null && check_AR_SalespersonLocationTrace == null &&
                    check_OM_TBonusCustomer == null && check_AR_Customer == null && check_OM_OrdDisc == null &&
                    check_OM_SalesRouteDet == null && check_OM_PDAOrdDisc == null && check_OM_SalesRouteDet == null &&
                    check_OM_PDAOrdDisc == null && check_OM_FCS == null && check_OM_SalesOrd == null &&
                    check_OM_CustDisD == null && check_PO_Header == null && check_OM_DiscClaimOrd == null &&
                    check_OM_SalPerObj == null && check_AR_NewCustomerInfor == null && check_IN_POSM == null)
                {
                    var slsper = _db.AR_Salesperson.FirstOrDefault(p => p.SlsperId == slsperID && p.BranchID == branchID);
                    if (slsper != null)
                    {
                        if (slsper.Status == _beginStatus)
                        {
                            var fileName = slsper.Images;
                            _db.AR_Salesperson.DeleteObject(slsper);
                            var lstAddr = _db.AR_SalespersonCpnyAddr.Where(p => p.SlsperID == slsperID && p.BranchID == branchID).ToList();
                            foreach (var addr in lstAddr)
                            {
                                _db.AR_SalespersonCpnyAddr.DeleteObject(addr);
                            }
                            _db.SaveChanges();

                            if (!string.IsNullOrWhiteSpace(fileName))
                            {
                                // Xoa file cu di
                                var oldPath = string.Format("{0}\\{1}", FilePath, fileName);
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                            }

                            return Json(new { success = true });
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "20140306");
                        }
                    }
                    else
                    {
                        return Json(new { success = true });
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2016060201");
                }
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

        private void updateSlsper(ref AR_Salesperson slsper, AR_Salesperson inputSlsper, string Status, string Handle, string SlsperId, string BranchID, string HandleCombo)
        {
            if (Handle == string.Empty || Handle == "N")
                slsper.Status = Status;
            else
            {
                var roles = _sys.Users.FirstOrDefault(x => x.UserName.ToLower() == Current.UserName.ToLower()).UserTypes;
                var arrRoles = (!string.IsNullOrEmpty(roles) && roles.Contains(","))
                    ? roles.Split(',') : (!string.IsNullOrEmpty(roles) ? new string[] { roles } : new string[] { "" });

                var approveHandle = _db.SI_ApprovalFlowHandle
                    .FirstOrDefault(p => p.AppFolID == _screenNbr
                                        && p.Status == inputSlsper.Status
                                        && p.Handle == HandleCombo
                                        && arrRoles.Contains(p.RoleID));

                // If has right of approval, then send email
                if (approveHandle != null)
                {
                    slsper.Status = approveHandle.ToStatus;
                    try
                    {
                        // send email
                        Approve.Mail_Approve(_screenNbr, SlsperId,
                            approveHandle.RoleID, approveHandle.Status, approveHandle.Handle,
                            Current.LangID.ToString(), Current.UserName, BranchID, Current.CpnyID,
                            string.Empty, string.Empty, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        //throw new MessageException(MessageType.Message, "11032015");
                    }
                }
            }
            slsper.Addr1 = inputSlsper.Addr1;
            slsper.Addr2 = inputSlsper.Addr2;
            slsper.CmmnPct = inputSlsper.CmmnPct;
            slsper.Country = inputSlsper.Country;
            slsper.EMailAddr = inputSlsper.EMailAddr;
            slsper.Fax = inputSlsper.Fax;
            slsper.Name = inputSlsper.Name;
            slsper.Phone = inputSlsper.Phone;
            slsper.ProductGroup = inputSlsper.ProductGroup;
            slsper.State = inputSlsper.State;
            slsper.DeliveryMan = inputSlsper.DeliveryMan;
            slsper.Position = inputSlsper.Position;
            slsper.District = inputSlsper.District;
            slsper.SupID = inputSlsper.SupID;
            slsper.CrLmt = inputSlsper.CrLmt;
            slsper.Active = inputSlsper.Active;
            slsper.Channel = inputSlsper.Channel;
            // The Silverlight project does NOT encrypt the PPCPassword.
            slsper.PPCPassword = inputSlsper.PPCPassword;

            slsper.PPCStorePicReq = inputSlsper.PPCStorePicReq;
            slsper.VendID = inputSlsper.VendID;
            slsper.PPCAdmin = inputSlsper.PPCAdmin;

            slsper.LUpd_DateTime = DateTime.Now;
            slsper.LUpd_Prog = _screenNbr;
            slsper.LUpd_User = Current.UserName;
        }

        private void AddSalesPerHist(AR_Salesperson obj)
        {
            DateTime now = DateTime.Now;
            AR_SalesPerHist objSalesHist = new AR_SalesPerHist();
            var objSalesHist1 = _db.AR_SalesPerHist
                .Where(p => p.BranchID == obj.BranchID && p.SlsPerID == obj.SlsperId)
                .OrderByDescending(p => p.Seq)
                .FirstOrDefault();

            string strSeq = objSalesHist1 == null ? "0000000001" : ("0000000000" + (double.Parse(objSalesHist1.Seq) + 1));
            strSeq = strSeq.Substring(strSeq.Length - 10, 10);

            objSalesHist.Seq = strSeq;
            objSalesHist.BranchID = obj.BranchID;
            objSalesHist.Crtd_DateTime = now;
            objSalesHist.Crtd_Prog = _screenNbr;
            objSalesHist.Crtd_User = Current.UserName;
            objSalesHist.SlsPerID = obj.SlsperId;
            objSalesHist.LUpd_DateTime = now;
            objSalesHist.LUpd_Prog = _screenNbr;
            objSalesHist.LUpd_User = Current.UserName;
            objSalesHist.Note = "Tạo mới Nhân viên";
            objSalesHist.FromDate = new DateTime(now.Year, now.Month, now.Day);
            objSalesHist.ToDate = (new DateTime(now.Year, now.Month, now.Day)).AddYears(100);

            _db.AR_SalesPerHist.AddObject(objSalesHist);
        }


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

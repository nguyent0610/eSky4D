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
        private string _noneStatus = "N";
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
                    _filePath = Server.MapPath("\\Images\\AR20200");
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
            tree.Listeners.CheckChange.Fn = "Event.Tree.treePanelCpnyAddr_checkChange";

            tree.AddTo(treeCpnyAddr);

            return this.Direct();
        }

        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data, bool isNew, string channel)
        {
            try
            {
                string slsperID = data["cboSlsperid"];
                string branchID = data["cboBranchID"];

                if (!string.IsNullOrWhiteSpace(branchID) && !string.IsNullOrWhiteSpace(slsperID))
                {
                    string handle = data["cboHandle"];
                    var slsperHandler = new StoreDataHandler(data["lstSalesPerson"]);
                    var inputSlsper = slsperHandler.ObjectData<AR_Salesperson>()
                                .FirstOrDefault(p => p.SlsperId == slsperID && p.BranchID == branchID);

                    if (inputSlsper != null)
                    {
                        #region Slsperson info
                        var slsper = _db.AR_Salesperson.FirstOrDefault(x => x.BranchID == branchID && x.SlsperId == slsperID);
                        if (slsper != null)
                        {
                            if (!isNew)
                            {
                                // update
                                if (slsper.tstamp.ToHex() == inputSlsper.tstamp.ToHex())
                                {
                                    updateSlsper(ref slsper, inputSlsper, false);

                                    if (!string.IsNullOrWhiteSpace(handle) && handle != _noneStatus)
                                    {
                                        // Check pending task
                                        var task = _db.HO_PendingTasks.FirstOrDefault(p => p.ObjectID == slsperID
                                                    && p.EditScreenNbr == _screenNbr && p.BranchID == branchID);

                                        // Checking right of approval
                                        var roles = _sys.Users.FirstOrDefault(x => x.UserName.ToLower() == Current.UserName.ToLower()).UserTypes;
                                        var arrRoles = (!string.IsNullOrEmpty(roles) && roles.Contains(","))
                                            ? roles.Split(',') : (!string.IsNullOrEmpty(roles) ? new string[] { roles } : new string[] { "" });
                                        
                                        var approveHandle = _db.SI_ApprovalFlowHandle
                                            .FirstOrDefault(p => p.AppFolID == _screenNbr
                                                                && p.Status == inputSlsper.Status
                                                                && p.Handle == handle
                                                                && arrRoles.Contains(p.RoleID));

                                        // If not in pending task but has right of approval, then add pending task
                                        if (task == null && approveHandle != null)
                                        {
                                            updatePendingTask(slsper, approveHandle);
                                            slsper.Status = approveHandle.ToStatus;
                                        }

                                        // If has right of approval, then send email
                                        if (approveHandle != null)
                                        {
                                            try
                                            {
                                                // send email
                                                Approve.Mail_Approve(_screenNbr, slsperID,
                                                    approveHandle.RoleID, approveHandle.Status, approveHandle.Handle,
                                                    Current.LangID.ToString(), Current.UserName, branchID, Current.CpnyID,
                                                    string.Empty, string.Empty, string.Empty);
                                            }
                                            catch
                                            {
                                                throw new MessageException(MessageType.Message, "11032015");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "2000","",new string[]{
                                    Util.GetLang("Slsperid")
                                });
                            }
                        }
                        else
                        {
                            if (isNew)
                            {
                                // Create slsper
                                slsper = new AR_Salesperson();
                                slsper.BranchID = branchID;
                                slsper.SlsperId = slsperID;

                                updateSlsper(ref slsper, inputSlsper, true);
                                _db.AR_Salesperson.AddObject(slsper);

                                // Add data to Sales Per Hist
                                AddSalesPerHist(slsper);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
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
                            // Xoa file cu di
                            var oldPath = string.Format("{0}\\{1}", FilePath, slsper.Images);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }

                            // Upload file moi
                            string newFileName = string.Format("{0}_{1}{2}", branchID, slsperID, Path.GetExtension(files[0].FileName));
                            files[0].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                            slsper.Images = newFileName;
                        }
                        else 
                        {
                            if (!string.IsNullOrWhiteSpace(slsper.Images) && string.IsNullOrWhiteSpace(inputSlsper.Images))
                            {
                                // Xoa file cu di
                                var oldPath = string.Format("{0}\\{1}", FilePath, slsper.Images);
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                                slsper.Images = string.Empty;
                            }
                        }
                        #endregion

                        _db.SaveChanges();

                        return Json(new { success = true, msgCode = 201405071 });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "1555");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "1000", "",
                        new string[]{
                            string.Format("{0}, {1}", Util.GetLang("BranchID"), Util.GetLang("SlsperID"))
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

        private void updateSlsper(ref AR_Salesperson slsper, AR_Salesperson inputSlsper, bool isNew)
        {
            if (isNew)
            {
                slsper.Status = _beginStatus;
                slsper.Crtd_DateTime = DateTime.Now;
                slsper.Crtd_Prog = _screenNbr;
                slsper.Crtd_User = Current.UserName;
            }

            slsper.Addr1 = inputSlsper.Addr1;
            slsper.Addr2 = inputSlsper.Addr2;
            slsper.CmmnPct = inputSlsper.CmmnPct;
            slsper.Country = inputSlsper.Country;
            slsper.EMailAddr = inputSlsper.EMailAddr;
            slsper.Fax = inputSlsper.Fax;
            slsper.Name = inputSlsper.Name;
            slsper.Phone = inputSlsper.Phone;
            //slsper.ProductGroup = inputSlsper.ProductGroup;
            slsper.State = inputSlsper.State;
            slsper.DeliveryMan = inputSlsper.DeliveryMan;
            slsper.Position = inputSlsper.Position;
            slsper.District = inputSlsper.District;
            slsper.SupID = inputSlsper.SupID;
            slsper.CrLmt = inputSlsper.CrLmt;
            slsper.Active = inputSlsper.Active;

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
            objSalesHist.tstamp = new byte[0];
            objSalesHist.Note = "Tạo mới Nhân viên";
            objSalesHist.FromDate = new DateTime(now.Year, now.Month, now.Day);
            objSalesHist.ToDate = (new DateTime(now.Year, now.Month, now.Day)).AddYears(100);

            _db.AR_SalesPerHist.AddObject(objSalesHist);
        }

        public void updatePendingTask(AR_Salesperson slsper, SI_ApprovalFlowHandle approveHandle)
        {
            if (!approveHandle.Param00.PassNull().Split(',').Any(p => p.ToLower() == "notapprove"))
            {
                HO_PendingTasks newTask = new HO_PendingTasks();
                newTask.BranchID = slsper.BranchID;
                newTask.ObjectID = slsper.SlsperId;
                newTask.EditScreenNbr = _screenNbr;
                newTask.Content = string.Format(approveHandle.ContentApprove, slsper.SlsperId, slsper.Name, slsper.BranchID);
                newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                newTask.Crtd_Prog = newTask.LUpd_Prog = _screenNbr;
                newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                newTask.Status = approveHandle.ToStatus;
                newTask.Parm00 = string.Empty;
                newTask.Parm01 = string.Empty;
                newTask.Parm02 = string.Empty;
                _db.HO_PendingTasks.AddObject(newTask);
            }
        }

        public ActionResult ImageToBin(string fileName)
        {
            try
            {
                string filename = FilePath + "\\" + fileName;
                if (System.IO.File.Exists(filename))
                {
                    FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    BinaryReader reader = new BinaryReader(fileStream);
                    byte[] imageBytes = reader.ReadBytes((int)fileStream.Length);
                    reader.Close();

                    var imgString64 = Convert.ToBase64String(imageBytes, 0, imageBytes.Length);

                    var jsonResult = Json(new { success = true, imgSrc = @"data:image/jpg;base64," + imgString64 }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                else
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
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

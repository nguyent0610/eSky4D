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
using HQFramework.DAL;
using HQ.eSkySys;
using HQSendMailApprove;

namespace IN10500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10500Controller : Controller
    {
        private string _screenNbr = "IN10500";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        IN10500Entities _db = Util.CreateObjectContext<IN10500Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);    
        private JsonResult _logMessage;

        public ActionResult Index(string BranchID,string TagID,string SiteID)
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);
            if (BranchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }
            if (BranchID == null) BranchID = Current.CpnyID;
            var dftSiteID = _db.IN10500_pdDefaultSite(Current.UserName, BranchID, Current.LangID).FirstOrDefault().PassNull();
            bool allowAddNewInvtID = false;
            var right = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "ALLOWADDNEWINVTID");
            if (right != null)
            {
                allowAddNewInvtID = right.IntVal == 1 ? true : false;
            }
           // var right = _db.IN10500_pdAddNewInvtRight(Current.UserName, BranchID, Current.LangID).FirstOrDefault();
            
            ViewBag.BranchID = BranchID;
            ViewBag.DftSiteID = dftSiteID;
            ViewBag.TagID = TagID.PassNull();
            ViewBag.SiteID = SiteID.PassNull();
            ViewBag.allowAddNewInvtID = allowAddNewInvtID;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetIN_TagHeader(string tagID, string branchID, string siteID)
        {
            var data = _db.IN_TagHeader.FirstOrDefault(x => x.TAGID == tagID && x.BranchID == branchID && x.SiteID == siteID);
            return this.Store(data);
        }

        public ActionResult GetIN_TagDetail(string TagID, string BranchID, string SiteID, string ReasonCD)
        {
            var data = _db.IN10500_pgLoadGrid(TagID, BranchID, SiteID, ReasonCD, Current.UserName, Current.CpnyID, Current.LangID);
            return this.Store(data);
        }
        [DirectMethod]
        public ActionResult IN10500_pdCheckCreateIN_Tag(string BranchID, string SiteID)
        {
            var chkINTag = _db.IN10500_pdCheckCreateIN_Tag(BranchID, SiteID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault().PassNull();            
            return this.Direct(chkINTag);
        }

        #region Save & Update information Company
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data, string siteName)
        {
            try
            {
                _db.CommandTimeout = int.MaxValue;                
                string ReasonCD = data["cboReasonCD"].PassNull();
                string Status = data["cboStatus"].PassNull();
                string Handle = data["cboHandle"].PassNull();
                string BranchID = data["cboBranchID"].PassNull();
                string descr = data["txtDescr"].PassNull();
                StoreDataHandler detHeader = new StoreDataHandler(data["lstIN_TagHeader"]);
                IN_TagHeader curHeader = detHeader.ObjectData<IN_TagHeader>().FirstOrDefault();
                curHeader.BranchID = BranchID;
                curHeader.Descr = descr;
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstIN_TagDetail"]);
                List<IN10500_pgLoadGrid_Result> lstIN_TagDetail = dataHandler1.ObjectData<IN10500_pgLoadGrid_Result>();
                if (lstIN_TagDetail == null)
                {
                    lstIN_TagDetail = new List<IN10500_pgLoadGrid_Result>();
                }
                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstDel"]);
                List<IN10500_pgLoadGrid_Result> lstDel = dataHandler2.ObjectData<IN10500_pgLoadGrid_Result>();
                if (lstDel == null)
                {
                    lstDel = new List<IN10500_pgLoadGrid_Result>();
                }

                if (lstIN_TagDetail.Count == 0)
                {
                    throw new MessageException(MessageType.Message, "20405");
                }
                
                #region Save Header

                var header = _db.IN_TagHeader.FirstOrDefault(p => p.TAGID == curHeader.TAGID && p.BranchID == curHeader.BranchID && p.SiteID == curHeader.SiteID);
                
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader, Status, Handle);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    var chkINTag = _db.IN10500_pdCheckCreateIN_Tag(curHeader.BranchID, curHeader.SiteID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(chkINTag)) // KHi không đc tạo thẻ kho
                    {
                        throw new MessageException(MessageType.Message, "2015070801", "", parm: new string[] { chkINTag });
                    }
                    var tagID = _db.IN10500_ppGetLastTagHeader(curHeader.BranchID, curHeader.SiteID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                    header = new IN_TagHeader();
                    header.TAGID = tagID;
                    header.BranchID = curHeader.BranchID;
                    header.SiteID = curHeader.SiteID;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    header.tstamp = new byte[1];
                    UpdatingHeader(ref header, curHeader, Status, Handle);

                    _db.IN_TagHeader.AddObject(header);
                }

            #endregion

                #region Save IN_TagDetail
                // Delete item
                for (var i = 0; i < lstDel.Count; i++)
                {
                    var item = lstDel[i];
                    if (lstIN_TagDetail.Any(x => x.InvtID == item.InvtID))
                    {
                        lstIN_TagDetail.FirstOrDefault(x => x.InvtID == item.InvtID).tstamp = item.tstamp;
                    }
                    else
                    {
                        var objDel = _db.IN_TagDetail.FirstOrDefault(x => x.TAGID == item.TAGID && x.BranchID == item.BranchID && x.SiteID == item.SiteID && x.InvtID == item.InvtID);
                        if (objDel != null)
                        {
                            _db.IN_TagDetail.DeleteObject(objDel);
                        }
                    }
                }

                foreach (IN10500_pgLoadGrid_Result currDet in lstIN_TagDetail)
                {
                    if (currDet.InvtID.PassNull() == "") continue;

                    var tagDet = _db.IN_TagDetail.FirstOrDefault(p => p.TAGID == header.TAGID
                                                                    && p.BranchID == header.BranchID
                                                                    && p.SiteID == header.SiteID
                                                                    && p.InvtID == currDet.InvtID);

                    if (tagDet != null)
                    {
                        if (tagDet.tstamp.ToHex() == currDet.tstamp.ToHex())
                        {
                            UpdatingIN_TagDetail(tagDet, currDet);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        tagDet = new IN_TagDetail();
                        tagDet.ResetET();
                        tagDet.TAGID = header.TAGID;
                        tagDet.BranchID = header.BranchID;
                        tagDet.SiteID = header.SiteID;
                        tagDet.InvtID = currDet.InvtID;
                        UpdatingIN_TagDetail(tagDet, currDet);
                        tagDet.Crtd_DateTime = DateTime.Now;
                        tagDet.Crtd_Prog = _screenNbr;
                        tagDet.Crtd_User = _userName;
                        _db.IN_TagDetail.AddObject(tagDet);
                    }
                    if (!_db.IN_ItemSite.Any(x => x.SiteID == header.SiteID && x.InvtID == tagDet.InvtID))
                    {
                        var objItemSite = new IN_ItemSite();
                        Insert_IN_ItemSite(ref objItemSite, header.SiteID, tagDet.InvtID, currDet.StkItem);
                    }
                }
                curHeader.TAGID = header.TAGID;
                #endregion

                _db.SaveChanges();
             
                var lstIN_Tag = _db.IN_TagDetail.Where(p => p.TAGID == curHeader.TAGID && p.BranchID == curHeader.BranchID && p.SiteID == curHeader.SiteID).ToList();
                if (Handle == "C" && lstIN_Tag.Where(p => p.OffsetEAQty != 0).Count() > 0)
                {
                    DataAccess dal = Util.Dal();
                    try
                    {
                        INProcess.IN inpr = new INProcess.IN(Current.UserName, _screenNbr, dal);

                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!inpr.IN10500_Release(curHeader.TAGID, curHeader.BranchID, header.SiteID))
                        {
                            dal.RollbackTrans();
                        }
                        else
                        {
                            dal.CommitTrans();
                        }

                        Util.AppendLog(ref _logMessage, "9999", "", data: new { success = true, TagID = curHeader.TAGID });

                        inpr = null;
                    }
                    catch (Exception)
                    {
                        dal.RollbackTrans();
                        throw;
                    }
                }
                else if (Handle == "C")
                {
                    header = _db.IN_TagHeader.FirstOrDefault(p => p.TAGID == curHeader.TAGID && p.BranchID == curHeader.BranchID && p.SiteID == curHeader.SiteID);
                    header.Status = "C";
                    _db.SaveChanges();
                }
                #region -Send mail approve-
                string userID = Current.UserName.ToUpper();
                var user = _sys.Users.FirstOrDefault(x => x.UserName.ToUpper() == userID);
                string userTypes = string.Empty;
                if (user != null)
                {
                    userTypes = user.UserTypes.PassNull();
                    string branchName = string.Empty;
                    var objBranch = _sys.SYS_Company.FirstOrDefault(x => x.CpnyID == header.BranchID);
                    if (objBranch != null)
                    {
                        branchName = objBranch.CpnyName.PassNull();
                    }
                    string[] lstRole = userTypes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var role in lstRole)
                    {
                        Mail_Approve(header.TAGID, role, Status, Handle, Current.LangID.PassNull(), Current.UserName, header.BranchID, Current.CpnyID, header.TAGID, header.BranchID, header.SiteID,
                            header.BranchID, branchName, header.SiteID, siteName);
                    }
                } 
                #endregion
                return Json(new { success = true, TagID = header.TAGID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        //Update Header Company
        private void UpdatingHeader(ref IN_TagHeader t, IN_TagHeader s, string Status, string Handle)
        {
            if (Handle == string.Empty || Handle == "N")
                t.Status = Status;
            else
                t.Status = Handle == "C" ? Status : Handle;
            t.Descr = s.Descr;
            t.ReasonCD = s.ReasonCD;
            t.INBatNbr = s.INBatNbr;            
            t.TranDate = s.TranDate;
            t.Note = s.Note;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        //Update IN_TagDetail
        #region Update IN_TagDetail
        private void UpdatingIN_TagDetail(IN_TagDetail t, IN10500_pgLoadGrid_Result s)
        {
            t.InvtName = s.InvtName;            
            t.EAUnit = s.EAUnit;            
            t.ActualEAQty = s.ActualEAQty;
            t.BookEAQty = s.BookEAQty;
            t.OffsetEAQty = s.OffsetEAQty;
            t.StkQtyUnder1Month = s.StkQtyUnder1Month;
            t.ReasonCD = s.ReasonCD;            
            t.Notes = s.Notes;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        #endregion

        #region Delete
        //Delete information Company
        [HttpPost]
        public ActionResult DeleteAll(FormCollection data, string SiteID)
        {
            try
            {
                string BranchID = data["cboBranchID"].PassNull();
                string TagID = data["cboTagID"].PassNull();
                var header = _db.IN_TagHeader.FirstOrDefault(p => p.TAGID == TagID);
                if (header != null)
                {
                    _db.IN_TagHeader.DeleteObject(header);
                }
                // Delete detail
                var lstIN_TagDetail = _db.IN_TagDetail.Where(p => p.TAGID == TagID).ToList();
                foreach (var item in lstIN_TagDetail)
                {
                    _db.IN_TagDetail.DeleteObject(item);
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion     

        public void Mail_Approve(string objID, string role, string status, string handle, string langID, string userName, string lstBranch, string currentBranch, string parm00, string parm01, string parm02,
          string branchID,  string branchName, string siteID, string siteName)
        {
            var approvehandle = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == _screenNbr && p.RoleID == role && p.Status == status && p.Handle == handle).FirstOrDefault();
            if (approvehandle != null && approvehandle.MailSubject.PassNull() != string.Empty)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("@CurrentBranchID", lstBranch);
                dic.Add("@BranchID", lstBranch);
                dic.Add("@ObjID", objID);
                dic.Add("@ScreenNbr", approvehandle.AppFolID);
                dic.Add("@FromStatus", approvehandle.Status);
                dic.Add("@ToStatus", approvehandle.ToStatus);
                dic.Add("@Action", "2");
                dic.Add("@RoleID", approvehandle.RoleID);
                dic.Add("@Handle", approvehandle.Handle);
                dic.Add("@LangID", langID);
                dic.Add("@User", userName);
                dic.Add("@Parm00", parm00);
                dic.Add("@Parm01", parm01);
                dic.Add("@Parm02", parm02);
                string mailSubject = approvehandle.MailSubject.PassNull()
                        .Replace("@p1", branchID)
                        .Replace("@p2", branchName)
                        .Replace("@p3", siteID)
                        .Replace("@p4", siteName);
                var mail = Approve.GetMail(approvehandle.MailApprove.PassNull() == string.Empty ? "MailSend" : approvehandle.MailApprove, dic);
               foreach (var item in mail)
                {
                    string content = string.Format("<html><body><p>{0}</p></body></html>", item.Content.PassNull());
                    Approve.SendMail(item.To.PassNull(), item.CC.PassNull(), mailSubject, content);
                }
            }
        }

        public void Insert_IN_ItemSite(ref IN_ItemSite objIN_ItemSite, string SiteID, string invtID, short stkItem)
        {
            try
            {
                objIN_ItemSite = new IN_ItemSite();
                objIN_ItemSite.InvtID = invtID;
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
                objIN_ItemSite.StkItem = stkItem;
                objIN_ItemSite.TotCost = 0;
                objIN_ItemSite.LastPurchaseDate = DateTime.Now;

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

        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                string BranchID = data["cboBranchID"].PassNull();             
                StoreDataHandler detHeader = new StoreDataHandler(data["lstIN_TagHeader"]);
                IN_TagHeader curHeader = detHeader.ObjectData<IN_TagHeader>().FirstOrDefault();
                curHeader.BranchID = BranchID;
               

               
                User user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
                string reportName = "";
                string reportNbr = "";

                var rpt = new RPTRunning();
                rpt.ResetET();

                reportName = data["reportName"];
                reportNbr = data["reportNbr"];

                rpt.ReportNbr = reportNbr;
                rpt.MachineName = "Web";
                rpt.ReportCap = "ReportName";
                rpt.ReportName = reportName;

                rpt.ReportDate = DateTime.Now;
                rpt.DateParm00 = DateTime.Now;
                rpt.DateParm01 = DateTime.Now;
                rpt.DateParm02 = DateTime.Now;
                rpt.DateParm03 = DateTime.Now;
                rpt.StringParm00 = curHeader.BranchID;
                rpt.StringParm01 = curHeader.INBatNbr;
                rpt.StringParm02 = curHeader.TAGID;
                rpt.StringParm03 = curHeader.SiteID;
                rpt.UserID = Current.UserName;
                rpt.AppPath = "Reports\\";
                rpt.ClientName = Current.UserName;
                rpt.LoggedCpnyID = curHeader.BranchID;
                rpt.CpnyID = user.CpnyID;
                rpt.LangID = Current.LangID;

                _db.RPTRunnings.AddObject(rpt);
                _db.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Json(new { success = true, reportID = rpt.ReportID, reportName = rpt.ReportName });
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

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
using Aspose.Cells;
using HQFramework.Common;
using System.Drawing;
using System.Threading;

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
        IN10500Entities _app = Util.CreateObjectContext<IN10500Entities>(false);
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;

        public ActionResult Index(string BranchID, string TagID, string SiteID)
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            string dftSiteID = "";
            string dftWhseLoc = "";
            string project = "";
            bool showType = false;
            bool allowAddNewInvtID = false;
            bool showColSiteID = false;
            bool editSiteWhseLoc = false;
            int showColWhseLoc = 0;
            int showWhseLoc = 0;
            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);
            if (BranchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }
            if (BranchID == null) BranchID = Current.CpnyID;
            var objDft = _app.IN10500_pdDefaultSite(Current.UserName, BranchID, Current.LangID).FirstOrDefault();

            if (objDft != null)
            {
                dftSiteID = objDft.INSite.PassNull();
                dftWhseLoc = objDft.INWhseLoc.PassNull();
            }
            
            var right = _sys.SYS_Configurations.FirstOrDefault(x => x.Code.ToUpper() == "ALLOWADDNEWINVTID");
            if (right != null)
            {
                allowAddNewInvtID = right.IntVal == 1 ? true : false;
            }
            // var right = _db.IN10500_pdAddNewInvtRight(Current.UserName, BranchID, Current.LangID).FirstOrDefault();            
            var objConfig = _app.IN10500_pdConfig(Current.UserName,Current.CpnyID,Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                showWhseLoc = objConfig.ShowWhseLoc.Value;
                showType = objConfig.ShowType.HasValue && objConfig.ShowType.Value;
                project = objConfig.Project.PassNull();
                showColSiteID = objConfig.ShowColSiteID.HasValue && objConfig.ShowColSiteID.Value;
                showColWhseLoc =objConfig.ShowColWhseLoc.Value;
                editSiteWhseLoc = objConfig.EditSiteWhseLoc.HasValue && objConfig.EditSiteWhseLoc.Value;
            }

            ViewBag.BranchID = BranchID;
            ViewBag.DftSiteID = dftSiteID;
            ViewBag.dftWhseLoc = dftWhseLoc;
            ViewBag.showWhseLoc = showWhseLoc;
            ViewBag.TagID = TagID.PassNull();
            ViewBag.SiteID = SiteID.PassNull();
            ViewBag.allowAddNewInvtID = allowAddNewInvtID;
            ViewBag.showType = showType;
            ViewBag.project = project;
            ViewBag.showColSiteID = showColSiteID;
            ViewBag.showColWhseLoc = showColWhseLoc;
            ViewBag.editSiteWhseLoc = editSiteWhseLoc;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetIN_TagHeader(string tagID, string branchID, string siteID)
        {
            var data = _app.IN_TagHeader.FirstOrDefault(x => x.TAGID == tagID && x.BranchID == branchID && x.SiteID == siteID);
            return this.Store(data);
        }

        public ActionResult GetItemSite(string invtID, string siteID, string whseLoc, int showWhseLoc)
        {
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                var objSite = _app.IN_ItemLoc.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc);
                return this.Store(objSite);
            }
            else
            {
                var objSite = _app.IN_ItemSite.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID);
                return this.Store(objSite);
            }
        }

        public ActionResult GetIN_TagLot(string tagID, string branchID, string siteID)
        {
            List<IN10500_pgGetIN_TagLot_Result> lstLotTrans = _app.IN10500_pgGetIN_TagLot(tagID, branchID, siteID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetIN_TagDetail(string TagID, string BranchID, string SiteID, string ReasonCD, string ClassID, string WhseLoc, string Project)
        {
            _app.CommandTimeout = int.MaxValue;
            var data = _app.IN10500_pgLoadGrid(TagID, BranchID, SiteID,WhseLoc,Project, ReasonCD, ClassID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
            string lineRef="0000001";           
            foreach (var item in data)
            {
                if (item.LineRef.PassNull() == "")
                {
                    var objLineRef = data.OrderByDescending(x => x.LineRef).FirstOrDefault();
                    if (objLineRef != null)
                    {
                        if (objLineRef.LineRef.PassNull() != "")
                        {
                            lineRef = "00000" + (objLineRef.LineRef.ToDouble() + 1).ToString();
                        }
                    }
                    lineRef = lineRef.Substring(lineRef.Length - 5, 5);
                    item.LineRef = lineRef;
                }                
            }            
            return this.Store(data);
        }
        [DirectMethod]
        public ActionResult IN10500_pdCheckCreateIN_Tag(string BranchID, string SiteID, string ClassID)
        {
            var chkINTag = _app.IN10500_pdCheckCreateIN_Tag(BranchID, SiteID, ClassID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault().PassNull();
            return this.Direct(chkINTag);
        }

        public ActionResult GetLot(string invtID, string siteID, string tagID, string branchID, string whseLoc, int? showWhseLoc)
        {
            List<IN10500_pdGetLot_Result> lstLot = new List<IN10500_pdGetLot_Result>();
            //List<IN_ItemLot> lstLotDB = _app.IN_ItemLot.Where(p => p.SiteID == siteID && p.InvtID == invtID && p.QtyAvail > 0).ToList();
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                List<IN10500_pdGetLot_Result> lstLotDB = _app.IN10500_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).Where(p => p.WhseLoc == whseLoc).ToList();
                foreach (var item in lstLotDB)
                {
                    lstLot.Add(item);
                }                
            }
            else
            {
                List<IN10500_pdGetLot_Result> lstLotDB = _app.IN10500_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).ToList();
                foreach (var item in lstLotDB)
                {
                    lstLot.Add(item);
                }                
            }
            lstLot = lstLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();
            return this.Store(lstLot, lstLot.Count);
        }


        public ActionResult GetItemLot(string invtID, string siteID, string lotSerNbr, string branchID, string tagID, string whseLoc, int? showWhseLoc)
        {
            var lot = new IN10500_pdGetLot_Result();
            if (showWhseLoc == 2 || (showWhseLoc == 1 && whseLoc.PassNull() != ""))
            {
                lot = _app.IN10500_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc);

                if (lot == null) lot = new IN10500_pdGetLot_Result()
                {
                    InvtID = invtID,
                    SiteID = siteID,
                    LotSerNbr = lotSerNbr
                };
                var lotTrans = _app.IN_TagLot.Where(p => p.BranchID == branchID && p.TAGID == tagID && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr && p.WhseLoc == whseLoc).ToList();
                
            }
            else
            {
                lot = _app.IN10500_pdGetLot(siteID, invtID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault(p => p.LotSerNbr == lotSerNbr);

                if (lot == null) lot = new IN10500_pdGetLot_Result()
                {
                    InvtID = invtID,
                    SiteID = siteID,
                    LotSerNbr = lotSerNbr
                };
                var lotTrans = _app.IN_TagLot.Where(p => p.BranchID == branchID && p.TAGID == tagID && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr).ToList();
                
            }

            List<IN10500_pdGetLot_Result> lstLot = new List<IN10500_pdGetLot_Result>() { lot };
            return this.Store(lstLot, lstLot.Count);
        }

        [DirectMethod]
        public ActionResult INNumberingLot(string invtID = "", DateTime? tranDate = null, string getType = "LotNbr")
        {
            var LotNbr = _app.INNumberingLot(invtID, tranDate, getType);
            return this.Direct(LotNbr);
        }

        #region Save & Update information Company
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data, string siteName)
        {
            try
            {
                _app.CommandTimeout = int.MaxValue;
                string ReasonCD = data["cboReasonCD"].PassNull();
                string Status = data["cboStatus"].PassNull();
                string ClassID = data["cboClassID"].PassNull();
                string Handle = data["cboHandle"].PassNull();
                string BranchID = data["cboBranchID"].PassNull();
                string descr = data["txtDescr"].PassNull();
                StoreDataHandler detHeader = new StoreDataHandler(data["lstIN_TagHeader"]);
                IN_TagHeader curHeader = detHeader.ObjectData<IN_TagHeader>().FirstOrDefault();
                curHeader.BranchID = BranchID;
                curHeader.Descr = descr;
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstIN_TagDetail"]);
                List<IN10500_pgLoadGrid_Result> lstIN_TagDetail = dataHandler1.ObjectData<IN10500_pgLoadGrid_Result>();

                StoreDataHandler dataHandler3 = new StoreDataHandler(data["lstIN_TagLot"]);
                List<IN10500_pgGetIN_TagLot_Result> lstIN_TagLot = dataHandler3.ObjectData<IN10500_pgGetIN_TagLot_Result>();
                lstIN_TagLot = dataHandler3.ObjectData<IN10500_pgGetIN_TagLot_Result>().Where(p => Util.PassNull(p.LotSerNbr)!="").ToList();

                if (lstIN_TagLot == null)
                {
                    lstIN_TagLot = new List<IN10500_pgGetIN_TagLot_Result>();
                }
                if (lstIN_TagDetail == null)
                {
                    lstIN_TagDetail = new List<IN10500_pgLoadGrid_Result>();
                }
                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstDel"]);
                List<IN10500_pgLoadGrid_Result> lstDel = dataHandler2.ObjectData<IN10500_pgLoadGrid_Result>();

                StoreDataHandler dataHandler4 = new StoreDataHandler(data["lstTagLotDel"]);
                List<IN10500_pgGetIN_TagLot_Result> lstTagLotDel = dataHandler4.ObjectData<IN10500_pgGetIN_TagLot_Result>();

                if (lstDel == null)
                {
                    lstDel = new List<IN10500_pgLoadGrid_Result>();
                }

                if (lstTagLotDel == null)
                {
                    lstTagLotDel = new List<IN10500_pgGetIN_TagLot_Result>();
                }

                if (lstIN_TagDetail.Count == 0)
                {
                    throw new MessageException(MessageType.Message, "20405");
                }

                #region Save Header

                var header = _app.IN_TagHeader.FirstOrDefault(p => p.TAGID == curHeader.TAGID && p.BranchID == curHeader.BranchID && p.SiteID == curHeader.SiteID);

                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        header.WhseLoc = curHeader.WhseLoc;
                        UpdatingHeader(ref header, curHeader, Status, Handle);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    var chkINTag = _app.IN10500_pdCheckCreateIN_Tag(curHeader.BranchID, curHeader.SiteID, ClassID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(chkINTag)) // KHi không đc tạo thẻ kho
                    {
                        throw new MessageException(MessageType.Message, "2015070801", "", parm: new string[] { chkINTag });
                    }
                    var tagID = _app.IN10500_ppGetLastTagHeader(curHeader.BranchID, curHeader.SiteID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                    header = new IN_TagHeader();
                    header.TAGID = tagID;
                    header.BranchID = curHeader.BranchID;
                    header.SiteID = curHeader.SiteID;
                    header.WhseLoc = curHeader.WhseLoc;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    header.tstamp = new byte[1];
                    UpdatingHeader(ref header, curHeader, Status, Handle);

                    _app.IN_TagHeader.AddObject(header);
                }

                #endregion

                #region Save IN_TagDetail
                // Delete item
                for (var i = 0; i < lstDel.Count; i++)
                {
                    var item = lstDel[i];
                    if (lstIN_TagDetail.Any(x => x.InvtID == item.InvtID && x.LineRef==item.LineRef))
                    {
                        lstIN_TagDetail.FirstOrDefault(x => x.InvtID == item.InvtID && x.LineRef == item.LineRef).tstamp = item.tstamp;
                    }
                    else
                    {
                        var objDel = _app.IN_TagDetail.FirstOrDefault(x => x.TAGID == item.TAGID && x.BranchID == item.BranchID && x.LineRef == item.LineRef && x.InvtID == item.InvtID);
                        if (objDel != null)
                        {
                            _app.IN_TagDetail.DeleteObject(objDel);
                        }
                    }
                }


                // Delete item
                for (var i = 0; i < lstTagLotDel.Count; i++)
                {
                    var item = lstTagLotDel[i];
                    if (item.WhseLoc.PassNull() == "")
                    {
                        if (lstIN_TagLot.Any(x => x.InvtID == item.InvtID && x.LineRef == item.LineRef && x.LotSerNbr == item.LotSerNbr))
                        {
                            lstIN_TagLot.FirstOrDefault(x => x.InvtID == item.InvtID).tstamp = item.tstamp;
                        }
                        else
                        {
                            var objDel = _app.IN_TagLot.FirstOrDefault(x => x.TAGID == item.TAGID && x.BranchID == item.BranchID && x.LineRef == item.LineRef && x.InvtID == item.InvtID && x.SiteID==item.SiteID && x.WhseLoc==item.WhseLoc && x.LotSerNbr ==item.LotSerNbr);
                            if (objDel != null)
                            {
                                _app.IN_TagLot.DeleteObject(objDel);
                            }
                        }
                    }
                    else
                    {
                        if (lstIN_TagLot.Any(x => x.InvtID == item.InvtID && x.SiteID == item.SiteID && x.LotSerNbr == item.LotSerNbr && x.WhseLoc == item.WhseLoc && x.LineRef==item.LineRef))
                        {
                            lstIN_TagLot.FirstOrDefault(x => x.InvtID == item.InvtID).tstamp = item.tstamp;
                        }
                        else
                        {
                            var objDel = _app.IN_TagLot.FirstOrDefault(x => x.TAGID == item.TAGID && x.BranchID == item.BranchID && x.SiteID == item.SiteID && x.InvtID == item.InvtID && x.LotSerNbr==item.LotSerNbr && x.WhseLoc == item.WhseLoc && x.LineRef==item.LineRef);
                            if (objDel != null)
                            {
                                _app.IN_TagLot.DeleteObject(objDel);
                            }
                        }
                    }                    
                }

                foreach (IN10500_pgLoadGrid_Result currDet in lstIN_TagDetail)
                {
                    if (currDet.InvtID.PassNull() == "") continue;

                    var tagDet = _app.IN_TagDetail.FirstOrDefault(p => p.TAGID == header.TAGID
                                                                    && p.BranchID == header.BranchID
                                                                    && p.LineRef == currDet.LineRef
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
                        tagDet.LineRef = currDet.LineRef;
                        tagDet.InvtID = currDet.InvtID;
                        UpdatingIN_TagDetail(tagDet, currDet);
                        tagDet.Crtd_DateTime = DateTime.Now;
                        tagDet.Crtd_Prog = _screenNbr;
                        tagDet.Crtd_User = _userName;
                        _app.IN_TagDetail.AddObject(tagDet);
                    }
                    if (!_app.IN_ItemSite.Any(x => x.SiteID == tagDet.SiteID && x.InvtID == tagDet.InvtID))
                    {
                        var objItemSite = new IN_ItemSite();
                        Insert_IN_ItemSite(ref objItemSite, tagDet.SiteID, tagDet.InvtID, currDet.StkItem);
                    }
                    if (tagDet.WhseLoc.PassNull() != "")
                    {
                        if (!_app.IN_ItemLoc.Any(x => x.SiteID == tagDet.SiteID && x.InvtID == tagDet.InvtID && x.WhseLoc == tagDet.WhseLoc))
                        {
                            var objItemLoc = new IN_ItemLoc();
                            Insert_IN_ItemLoc(ref objItemLoc, tagDet.SiteID, tagDet.InvtID, tagDet.WhseLoc, currDet.StkItem);
                        }
                    }
                }
                curHeader.TAGID = header.TAGID;
                #endregion

                #region Save IN_TagLot
                foreach (var item in lstIN_TagLot)
                {
                    if (item.LotSerNbr.PassNull() == "") continue;
                    var objTaglot = _app.IN_TagLot.FirstOrDefault(p => p.TAGID == header.TAGID && p.BranchID == header.BranchID && p.LineRef==item.LineRef && p.InvtID==item.InvtID && p.LotSerNbr==item.LotSerNbr);
                    if (objTaglot != null)
                    {
                        if (objTaglot.tstamp.ToHex() == item.tstamp.ToHex())
                        {
                            UpdatingIN_TagLot(objTaglot, item);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objTaglot = new IN_TagLot();
                        objTaglot.ResetET();
                        objTaglot.BranchID = header.BranchID;
                        objTaglot.TAGID = header.TAGID;
                        objTaglot.SiteID = header.SiteID;
                        objTaglot.LotSerNbr = item.LotSerNbr;
                        objTaglot.Crtd_DateTime = DateTime.Now;
                        objTaglot.Crtd_Prog = _screenNbr;
                        objTaglot.Crtd_User = _userName;
                        objTaglot.ExpDate = DateTime.Now.ToDateShort();
                        UpdatingIN_TagLot(objTaglot, item);
                        _app.IN_TagLot.AddObject(objTaglot);
                    }

                    if (!_app.IN_ItemLot.Any(x => x.SiteID == header.SiteID && x.InvtID == item.InvtID && x.WhseLoc == header.WhseLoc && x.LotSerNbr == item.LotSerNbr))
                    {
                        var objItemLot = new IN_ItemLot();
                        Insert_IN_ItemLot(ref objItemLot, header.SiteID, item.InvtID, header.WhseLoc,item.LotSerNbr);
                    }
                }

                #endregion

                _app.SaveChanges();

                var lstIN_Tag = _app.IN_TagDetail.Where(p => p.TAGID == curHeader.TAGID && p.BranchID == curHeader.BranchID && p.SiteID == curHeader.SiteID).ToList();
                if (Handle == "C" && lstIN_Tag.Where(p => p.OffsetEAQty != 0).Count() > 0)
                {
                    DataAccess dal = Util.Dal();
                    try
                    {
                        INProcess.IN inpr = new INProcess.IN(Current.UserName, _screenNbr, dal);

                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!inpr.IN10500_Release(curHeader.TAGID, curHeader.BranchID, header.SiteID,header.WhseLoc))
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
                    header = _app.IN_TagHeader.FirstOrDefault(p => p.TAGID == curHeader.TAGID && p.BranchID == curHeader.BranchID && p.SiteID == curHeader.SiteID);
                    header.Status = "C";
                    _app.SaveChanges();
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
            t.Type = s.Type;
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
            t.SiteID = s.SiteID;
            t.WhseLoc = s.WhseLoc;
            t.Notes = s.Notes;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        #endregion

        //Update IN_TagLot
        #region Update IN_TagLot
        private void UpdatingIN_TagLot(IN_TagLot t, IN10500_pgGetIN_TagLot_Result s)
        {
            t.InvtID = s.InvtID;
            t.InvtMult = s.InvtMult;
            t.ActualEAQty = s.ActualEAQty;
            t.BookEAQty = s.BookEAQty;
            t.OffsetEAQty = s.OffsetEAQty;            
            t.TranDate = DateTime.Now.ToDateShort();            
            t.TranType = s.TranType;
            t.UnitCost = s.UnitCost;
            t.UnitDesc = s.UnitDesc;
            t.LineRef = s.LineRef;
            t.WhseLoc = s.WhseLoc;
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
                var header = _app.IN_TagHeader.FirstOrDefault(p => p.TAGID == TagID);
                if (header != null)
                {
                    _app.IN_TagHeader.DeleteObject(header);
                }
                // Delete detail
                var lstIN_TagDetail = _app.IN_TagDetail.Where(p => p.TAGID == TagID).ToList();
                foreach (var item in lstIN_TagDetail)
                {
                    _app.IN_TagDetail.DeleteObject(item);
                }

                // Delete detail
                var lstIN_TagLot = _app.IN_TagLot.Where(p => p.TAGID == TagID).ToList();
                foreach (var item in lstIN_TagLot)
                {
                    _app.IN_TagLot.DeleteObject(item);
                }

                _app.SaveChanges();
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
          string branchID, string branchName, string siteID, string siteName)
        {
            var approvehandle = _app.SI_ApprovalFlowHandle.Where(p => p.AppFolID == _screenNbr && p.RoleID == role && p.Status == status && p.Handle == handle).FirstOrDefault();
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

                _app.IN_ItemSite.AddObject(objIN_ItemSite);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void Insert_IN_ItemLoc(ref IN_ItemLoc objIN_ItemLoc, string SiteID, string invtID,string whseLoc, short stkItem)
        {
            try
            {
                objIN_ItemLoc = new IN_ItemLoc();
                objIN_ItemLoc.InvtID = invtID;
                objIN_ItemLoc.SiteID = SiteID;
                objIN_ItemLoc.WhseLoc = whseLoc;
                objIN_ItemLoc.AvgCost = 0;
                objIN_ItemLoc.QtyAlloc = 0;
                objIN_ItemLoc.QtyAllocIN = 0;
                objIN_ItemLoc.QtyAllocPORet = 0;
                objIN_ItemLoc.QtyAllocSO = 0;
                objIN_ItemLoc.QtyAvail = 0;
                objIN_ItemLoc.QtyInTransit = 0;
                objIN_ItemLoc.QtyOnBO = 0;
                objIN_ItemLoc.QtyOnHand = 0;
                objIN_ItemLoc.QtyOnPO = 0;
                objIN_ItemLoc.QtyOnTransferOrders = 0;
                objIN_ItemLoc.QtyOnSO = 0;
                objIN_ItemLoc.QtyShipNotInv = 0;
                objIN_ItemLoc.StkItem = stkItem;
                objIN_ItemLoc.TotCost = 0;
                objIN_ItemLoc.LastPurchaseDate = DateTime.Now;
                objIN_ItemLoc.Crtd_DateTime = DateTime.Now;
                objIN_ItemLoc.Crtd_Prog = _screenNbr;
                objIN_ItemLoc.Crtd_User = Current.UserName;
                objIN_ItemLoc.LUpd_DateTime = DateTime.Now;
                objIN_ItemLoc.LUpd_Prog = _screenNbr;
                objIN_ItemLoc.LUpd_User = Current.UserName;
                objIN_ItemLoc.tstamp = new byte[0];

                _app.IN_ItemLoc.AddObject(objIN_ItemLoc);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void Insert_IN_ItemLot(ref IN_ItemLot objIN_ItemLot, string SiteID, string invtID, string whseLoc, string lotSerNbr)
        {
            try
            {
                objIN_ItemLot = new IN_ItemLot();
                objIN_ItemLot.InvtID = invtID;
                objIN_ItemLot.SiteID = SiteID;
                objIN_ItemLot.LotSerNbr = lotSerNbr;
                objIN_ItemLot.WhseLoc = whseLoc;
                objIN_ItemLot.Cost = 0;
                objIN_ItemLot.ExpDate = DateTime.Now.ToDateShort();
                objIN_ItemLot.LIFODate = DateTime.Now.ToDateShort();
                objIN_ItemLot.MfgrLotSerNbr = "";
                objIN_ItemLot.QtyAlloc = 0;
                objIN_ItemLot.QtyAllocIN = 0;
                objIN_ItemLot.QtyAllocOther = 0;
                objIN_ItemLot.QtyAllocPORet = 0;
                objIN_ItemLot.QtyAllocSO = 0;
                objIN_ItemLot.QtyAvail = 0;
                objIN_ItemLot.QtyOnHand = 0;          
                objIN_ItemLot.QtyShipNotInv = 0;
                objIN_ItemLot.WarrantyDate = DateTime.Now.ToDateShort();
                objIN_ItemLot.PackageID = "";
                objIN_ItemLot.QtyAllocPDASO = 0;
                objIN_ItemLot.Crtd_DateTime = DateTime.Now;
                objIN_ItemLot.Crtd_Prog = _screenNbr;
                objIN_ItemLot.Crtd_User = Current.UserName;
                objIN_ItemLot.LUpd_DateTime = DateTime.Now;
                objIN_ItemLot.LUpd_Prog = _screenNbr;
                objIN_ItemLot.LUpd_User = Current.UserName;
                objIN_ItemLot.tstamp = new byte[0];
                _app.IN_ItemLot.AddObject(objIN_ItemLot);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        [HttpPost]
        public ActionResult ExportFileName()
        {

            string fileName = "A" + Guid.NewGuid().ToString("N") + ".xlsx";
            string path = Server.MapPath("~/temp") + @"\" + fileName;
            return Json(new { success = true, id = fileName, name = Util.GetLang("IN10500") + ".xlsx" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CheckFile(string name, string id)
        {
            string path = Server.MapPath("~/temp");
            string fullName = path + @"\" + id;
            while (!System.IO.File.Exists(fullName))
            {
                Thread.Sleep(5 * 1000);
            }
            //kiem tra file co dang su dung hay ko? Vi khi export dang save file, khi do da co file nhung save chua xong, file download ve loi
            FileInfo fi = new FileInfo(fullName);
            while (IsFileLocked(fi))
            {
                Thread.Sleep(5 * 1000);
            }

            return Json(new { success = true, id = id, name = name }, JsonRequestBehavior.AllowGet);

        }
        protected bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public ActionResult Export(FormCollection data, string id, string name)
        {
            try
            {
                int showWhseLoc = 0;
                var objConfig = _app.IN10500_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                if (objConfig != null)
                {
                    showWhseLoc = objConfig.ShowWhseLoc ?? 0;
                }

                LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet sheetTrans = workbook.Worksheets[0];

                sheetTrans.Name = "Details";

                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@TagID", DbType.String, clsCommon.GetValueDBNull(data["cboTagID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(data["cboBranchID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(data["cboSiteID"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@WhseLoc", DbType.String, clsCommon.GetValueDBNull(data["cboWhseLoc"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ReasonCD", DbType.String, clsCommon.GetValueDBNull(data["cboReasonCD"].PassNull()), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ClassID", DbType.String, clsCommon.GetValueDBNull(data["cboClassID"].PassNull()), ParameterDirection.Input, 30));
                DataTable dt = dal.ExecDataTable("IN10500_peExport", CommandType.StoredProcedure, ref pc);
                sheetTrans.Cells.ImportDataTable(dt, false, "A2");



                Style style = workbook.GetStyleInPool(0);
                style.Font.Color = Color.Transparent;
                style.IsLocked = true;
                StyleFlag flag = new StyleFlag();
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;

                style = sheetTrans.Cells["A1"].GetStyle();
                style.Font.IsBold = true;
                
                    sheetTrans.Cells["A1"].PutValue(Util.GetLang("InvtID"));
                    sheetTrans.Cells["B1"].PutValue(Util.GetLang("InvtName"));
                    sheetTrans.Cells["C1"].PutValue(Util.GetLang("UOM"));
                    sheetTrans.Cells["D1"].PutValue(Util.GetLang("SiteID"));
                    sheetTrans.Cells["E1"].PutValue(Util.GetLang("WhseLoc"));
                    sheetTrans.Cells["F1"].PutValue(Util.GetLang("LotSerNbr"));
                    sheetTrans.Cells["G1"].PutValue(Util.GetLang("IN10500ActInventory"));
                    sheetTrans.Cells["H1"].PutValue(Util.GetLang("IN10500Inventory"));
                    sheetTrans.Cells["A1"].SetStyle(style);
                    sheetTrans.Cells["B1"].SetStyle(style);
                    sheetTrans.Cells["C1"].SetStyle(style);
                    sheetTrans.Cells["D1"].SetStyle(style);
                    sheetTrans.Cells["E1"].SetStyle(style);
                    sheetTrans.Cells["F1"].SetStyle(style);
                    sheetTrans.Cells["G1"].SetStyle(style);
                    sheetTrans.Cells["H1"].SetStyle(style);
                    style = sheetTrans.Cells["D2"].GetStyle();
                    style.Custom = "#,##0";
                    Range range = sheetTrans.Cells.CreateRange("D2", "E" + (dt.Rows.Count + 2));
                    range.ApplyStyle(style, flag);
                    sheetTrans.AutoFitColumns();
                                

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Position = 0;

                var fileName = id;
                string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
                workbook.Save(fullPath, SaveFormat.Xlsx);
                return Json(new { success = true, id = id, name = name, errorMessage = "" });
                //return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "Data.xlsx" };
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

        public ActionResult Import(FormCollection data)
        {
            try
            {

                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile; // or: HttpPostedFileBase file = this.HttpContext.Request.Files[0];
                FileInfo fileInfo = new FileInfo(file.FileName);
                string message = string.Empty;
               
                //List<object> lstTrans = new List<object>();
                List<IN10500_pgLoadGrid_Result> lstData = new List<IN10500_pgLoadGrid_Result>();
                List<IN10500_peExport_Result> lstDataImport = new List<IN10500_peExport_Result>();
                List<IN10500_pgGetIN_TagLot_Result> lstDataLot = new List<IN10500_pgGetIN_TagLot_Result>();
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    try
                    {
                        Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                        int lineRef = data["lineRef"].ToInt();
                        string branchID = data["cboBranchID"].ToString();
                        string tagID = data["cboTagID"].ToString();
                        if (workbook.Worksheets.Count > 0)
                        {
                            var lstWhseLoc = _app.IN10500_piWhseLoc(Current.CpnyID,Current.UserName,Current.LangID,branchID).ToList();
                            var lsSiteID = _app.IN10500_piSiteID(Current.CpnyID, Current.UserName, Current.LangID, branchID).ToList();

                            Worksheet workSheet = workbook.Worksheets[0];
                            string invtID = string.Empty;
                            string unit = string.Empty;
                            string siteID=string.Empty;
                            string whseLoc = string.Empty;
                            string lotSerNbr=string.Empty;
                            var listInventory = _app.IN10500_piInvtory(Current.CpnyID, Current.UserName, Current.LangID, branchID).ToList();
                            for (int i = 1; i < workSheet.Cells.MaxDataRow + 1; i++)
                            {
                                invtID = workSheet.Cells[i, 0].StringValue;
                                unit = workSheet.Cells[i, 2].StringValue.PassNull();
                                double cnfv = 1;
                                if (invtID == string.Empty) break;
                                var objInvt = listInventory.FirstOrDefault(p => p.InvtID == invtID);
                                if (objInvt == null)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có trong hệ thống hoặc trong quá trình chờ sử lý<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                if (workSheet.Cells[i, 2].StringValue.PassNull() == string.Empty)
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có đơn vị<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                else if (unit != objInvt.StkUnit)
                                {
                                    IN_UnitConversion uomTo = SetUOM(invtID, objInvt.ClassID, objInvt.StkUnit, unit);
                                    if (uomTo != null)
                                    {
                                        cnfv = uomTo.MultDiv == "M" ? uomTo.CnvFact : (1 / uomTo.CnvFact);
                                    }
                                    else
                                    {
                                        IN_UnitConversion uomfrom = SetUOM(invtID, objInvt.ClassID, unit, objInvt.StkUnit);
                                        if (uomfrom != null)
                                        {
                                            cnfv = uomfrom.MultDiv == "M" ? (1 / uomfrom.CnvFact) : uomfrom.CnvFact;
                                        }
                                        else
                                        {
                                            message += string.Format("Dòng {0} mặt hàng {1} sai đơn vị !<br/>", (i + 1).ToString(), invtID);
                                            continue;
                                        }
                                    }
                                    unit = objInvt.StkUnit;
                                }

                                if (workSheet.Cells[i, 3].StringValue.PassNull() == "")
                                {
                                    message += string.Format("Dòng {0} kho {1} không được để trống!<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                else
                                {
                                    siteID=workSheet.Cells[i, 3].StringValue.PassNull();
                                    if (lsSiteID.FirstOrDefault(p=>p.SiteID == siteID) == null)
                                    {
                                        message += string.Format("Dòng {0} có kho không đúng!<br/>", (i + 1).ToString());
                                        continue;
                                    }
                                }

                                if (workSheet.Cells[i, 4].StringValue.PassNull() == "")
                                {
                                    message += string.Format("Dòng {0} vị trí {1} không được để trống!<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                else
                                {
                                    whseLoc = workSheet.Cells[i, 4].StringValue.PassNull();
                                    if (lstWhseLoc.FirstOrDefault(p => p.SiteID == siteID && p.WhseLoc == whseLoc) == null)
                                    {
                                        message += string.Format("Dòng {0} có vị trí kho không đúng!<br/>", (i + 1).ToString());
                                        continue;
                                    }
                                }

                                if (workSheet.Cells[i, 6].StringValue.PassNull() == "")
                                {
                                    message += string.Format("Dòng {0} mặt hàng {1} không có số lượng<br/>", (i + 1).ToString(), invtID);
                                    continue;
                                }
                                else
                                {
                                    float n;
                                    bool isNumeric = float.TryParse(workSheet.Cells[i, 6].StringValue, out n);
                                    if (isNumeric == true)
                                    {
                                        if (workSheet.Cells[i, 6].FloatValue < 0)
                                        {
                                            message += string.Format("Dòng {0} mặt hàng {1} số lượng không được phép nhỏ hơn 0<br/>", (i + 1).ToString(), invtID);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        message += string.Format("Dòng {0} sai định dạng Số Lượng<br/>", (i + 1).ToString());
                                        continue;
                                    }
                                }

                                if (objInvt.LotSerTrack != "N")
                                {
                                    if (workSheet.Cells[i, 5].StringValue.PassNull() == "")
                                    {
                                        message += string.Format("Dòng {0} mặt hàng {1} có quản lý Lot nên Mã Lot được để trống !<br/>", (i + 1).ToString(), invtID);
                                        continue;
                                    }
                                }
                                lotSerNbr = workSheet.Cells[i, 5].StringValue.PassNull();

                                if (objInvt.LotSerTrack == "N")
                                {
                                    if (lstDataImport.FirstOrDefault(p=>p.InvtID==invtID && p.SiteID==siteID && p.WhseLoc==whseLoc) != null)
                                    {
                                        message += string.Format("Dòng {0} dữ liệu bị trùng !<br/>", (i + 1).ToString());
                                        continue;
                                    }
                                }
                                else{
                                    if (lstDataImport.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.WhseLoc == whseLoc && p.LotSerNbr==lotSerNbr) != null)
                                    {
                                        message += string.Format("Dòng {0} dữ liệu bị trùng !<br/>", (i + 1).ToString());
                                        continue;
                                    }
                                }                                

                                var obj=new IN10500_peExport_Result();
                                obj.ResetET();
                                obj.InvtID=objInvt.InvtID;
                                obj.InvtName=objInvt.Descr;
                                obj.LotSerNbr=lotSerNbr;
                                obj.SiteID=siteID;
                                obj.WhseLoc=whseLoc;
                                obj.ActualEAQty=workSheet.Cells[i, 6].FloatValue * cnfv;
                                obj.EAUnit="";
                                obj.BookEAQty=0;
                                lstDataImport.Add(obj);



                            }

                            foreach (var item in lstDataImport)
                            {
                                if (lstData.FirstOrDefault(p => p.InvtID == item.InvtID && p.SiteID == item.SiteID && p.WhseLoc == item.WhseLoc) == null)
                                {
                                    double total = lstDataImport.Where(p=>p.InvtID==item.InvtID && p.SiteID==item.SiteID && p.WhseLoc==item.WhseLoc).Sum(x => x.ActualEAQty);
                                    lstData.Add(new IN10500_pgLoadGrid_Result()
                                    {
                                        ActualEAQty = total,
                                        BookEAQty = 0,
                                        BranchID = "",
                                        EAUnit = "",
                                        LineRef = "",
                                        InvtID = item.InvtID,
                                        InvtName = item.InvtName,
                                        Notes = "",
                                        OffsetEAQty = 0,
                                        ReasonCD = "",
                                        SiteID = item.SiteID,
                                        StkItem = 0,
                                        StkQtyUnder1Month = 0,
                                        TAGID = "",                                        
                                        WhseLoc = item.WhseLoc
                                    });
                                }

                                if (lstDataLot.FirstOrDefault(p => p.InvtID == item.InvtID && p.SiteID == item.SiteID && p.WhseLoc == item.WhseLoc && p.LotSerNbr==item.LotSerNbr) == null)
                                {
                                    lstDataLot.Add(new IN10500_pgGetIN_TagLot_Result()
                                    {
                                        ActualEAQty = item.ActualEAQty,
                                        BookEAQty = 0,
                                        BranchID = "",
                                        LineRef = "",
                                        InvtID = item.InvtID,
                                        UnitDesc="",
                                        OffsetEAQty = 0,
                                        SiteID = item.SiteID,
                                        TAGID = "",
                                        WhseLoc = item.WhseLoc,
                                        LotSerNbr=item.LotSerNbr
                                    });
                                }

                            }
                            if (message == "")
                                Util.AppendLog(ref _logMessage, "20121418", "", data: new { message, lstData, lstDataLot });
                            else
                            {
                                lstData = new List<IN10500_pgLoadGrid_Result>();
                                Util.AppendLog(ref _logMessage, "20121418", "", data: new { message, lstData, lstDataLot });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
                return _logMessage;
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
        [HttpGet]
        //[DeleteFileAttribute] //Action Filter, it will auto delete the file after download,I will explain it later
        public ActionResult DownloadAndDelete(string name, string id)
        {
            string fullPath = Path.Combine(Server.MapPath("~/temp"), id);
            return File(fullPath, "application/vnd.ms-excel", name);
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

                _app.RPTRunnings.AddObject(rpt);
                _app.SaveChanges();

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

        private IN_UnitConversion SetUOM(string invtID, string classID, string stkUnit, string fromUnit)
        {
            if (!string.IsNullOrEmpty(fromUnit))
            {
                IN_UnitConversion data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "3" && p.ClassID == "*" && p.InvtID == invtID && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "2" && p.ClassID == classID && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "1" && p.ClassID == "*" && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                return null;
            }
            return null;
        }
        
    }
}

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

        public ActionResult Index(string branchID)
        {
            Util.InitRight(_screenNbr);
            var user = _sys.Users.FirstOrDefault(p => p.UserName == Current.UserName);
            if (branchID == null && user != null && user.CpnyID.PassNull().Split(',').Length > 1)
            {
                return View("Popup");
            }
            //var obj = _sys.SYS_Configurations.Where(p => p.Code.ToUpper() == "IN10500MINUTE").FirstOrDefault();
            //if (obj != null)
            //{
            //    ViewBag.Miniute = obj.IntVal;
            //}
            //else
            //{
            //    ViewBag.Miniute = 0;
            //}
            //var objRight = _sys.SYS_Configurations.Where(p => p.Code.ToUpper() == "IN10500RIGHT").FirstOrDefault();
            //if (objRight != null)
            //{
            //    ViewBag.Right = objRight.IntVal;
            //}
            //else
            //{
            //    ViewBag.Right = 0;
            //}
            if (branchID == null) branchID = Current.CpnyID;
            ViewBag.BranchID = branchID;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetIN_TagHeader(string TagID)
        {
            var data = _db.IN_TagHeader.FirstOrDefault(x => x.TAGID == TagID);
            return this.Store(data);
        }

        public ActionResult GetIN_TagDetail(string TagID, string BranchID, string SiteID, string ReasonCD)
        {
            var data = _db.IN10500_pgLoadGrid(TagID, BranchID, SiteID, ReasonCD, Current.UserName, Current.CpnyID, Current.LangID)
                                //.Select(p => new IN10500_pgLoadGrid_Result()
                                //{
                                //    TAGID = p.TAGID,
                                //    SiteID = p.SiteID,
                                //    InvtID = p.InvtID,
                                //    CaseUnit = p.CaseUnit,
                                //    InvtName = p.InvtName,
                                //    BookCaseQty = p.BookCaseQty,
                                //    ActualCaseQty = p.ActualCaseQty, 
                                //    StkQtyUnder1Month = p.StkQtyUnder1Month,
                                //    OffetCaseQty = p.OffetCaseQty != 0 ? p.OffetCaseQty : p.ActualCaseQty - p.BookCaseQty,
                                //    ReasonCD = p.ReasonCD,
                                //    tstamp = p.tstamp
                                //})
                                .ToList();
            return this.Store(data);
        }


        #region Save & Update information Company
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {               
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

                #region Save Header

                var header = _db.IN_TagHeader.FirstOrDefault(p => p.TAGID == curHeader.TAGID);
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
                    var chkINTag = _db.IN10500_pdCheckCreateIN_Tag(curHeader.BranchID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                    if (chkINTag.HasValue && chkINTag != 0) // KHi không đc tạo thẻ kho
                    {
                        throw new MessageException(MessageType.Message, "1001", "", parm: new string[] { "" });
                    }
                    var tagID = _db.IN10500_ppGetLastTagHeader(curHeader.BranchID, curHeader.SiteID, Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                    header = new IN_TagHeader();
                    header.TAGID = tagID;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    header.tstamp = new byte[1];
                    UpdatingHeader(ref header, curHeader, Status, Handle);
                    
                    _db.IN_TagHeader.AddObject(header);
                }

                #endregion

                #region Save IN_TagDetail


                foreach (IN10500_pgLoadGrid_Result currDet in lstIN_TagDetail)
                {
                    if (currDet.InvtID.PassNull() == "") continue;

                    var tagDet = _db.IN_TagDetail.FirstOrDefault(p => p.TAGID == header.TAGID
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
                        tagDet.InvtID = currDet.InvtID;
                        UpdatingIN_TagDetail(tagDet, currDet);
                        tagDet.Crtd_DateTime = DateTime.Now;
                        tagDet.Crtd_Prog = _screenNbr;
                        tagDet.Crtd_User = _userName;
                        _db.IN_TagDetail.AddObject(tagDet);                        
                    }
                }
                #endregion

                _db.SaveChanges();
                var lstIN_Tag = _db.IN_TagDetail.Where(p => p.TAGID == curHeader.TAGID).ToList();
                if (Handle == "C" && lstIN_Tag.Where(p => p.OffetCaseQty != 0).Count() > 0)
                {
                    DataAccess dal = Util.Dal();
                    try
                    {
                        INProcess.IN inpr = new INProcess.IN(Current.UserName, _screenNbr, dal);

                        dal.BeginTrans(IsolationLevel.ReadCommitted);
                        if (!inpr.IN10500_Release(curHeader.TAGID, curHeader.BranchID))
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
                    header = _db.IN_TagHeader.FirstOrDefault(p => p.TAGID == curHeader.TAGID);
                    header.Status = "C";
                    _db.SaveChanges();
                }

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
            t.BranchID = s.BranchID;
            t.SiteID = s.SiteID;
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
            t.CaseUnit = s.CaseUnit;            
            t.ActualCaseQty = s.ActualCaseQty;
            t.BookCaseQty = s.BookCaseQty;
            t.OffetCaseQty = s.OffetCaseQty;
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
    }
}

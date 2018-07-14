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
using OM20090.Models;
using System.Xml;
namespace OM20090.Controllers
{
   
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20090Controller : Controller
    {
        private string _screenNbr = "OM20090";
        OM20090Entities _db = Util.CreateObjectContext<OM20090Entities>(false);

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

        public ActionResult GetCompetitorInvt(string branchID, string surveyID)
        {
            var dets = _db.OM20090_pgCompetitorInvt(Current.CpnyID, Current.UserName, Current.LangID, branchID, surveyID).ToList();
            return this.Store(dets);
        }

        public ActionResult GetSurveyInvt(string branchID, string surveyID, string invtID)
        {
            var dets = _db.OM20090_pgCompetitorSurveyInvt(Current.CpnyID, Current.UserName, Current.LangID, branchID, surveyID).ToList();
            return this.Store(dets);
        }

        public ActionResult GetSurveyCriteria(string branchID, string surveyID)
        {
            var dets = _db.OM20090_pgCompetitorSurveyCriteria(Current.CpnyID, Current.UserName, Current.LangID, branchID, surveyID).ToList();
            return this.Store(dets);
        }

        public ActionResult GetHeaderSurvey(string surveyID, string branchID)
        {
            var dets = _db.OM20090_pdHeaderSurvey(Current.CpnyID, Current.UserName, Current.LangID, branchID, surveyID).ToList();
            return this.Store(dets);
        }

        [HttpPost]

        public ActionResult Save(FormCollection data)
        {

            try
            {
                string Handle = data["cboHandle"];
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstHeader"]);

                var curHeader = dataHandler.ObjectData<OM_CompetitorSurveyHeader>().FirstOrDefault();


                StoreDataHandler dataHandlerDel = new StoreDataHandler(data["lstDel"]);
                List<OM20090_pgCompetitorSurveyInvt_Result> lstDel = dataHandlerDel.ObjectData<OM20090_pgCompetitorSurveyInvt_Result>();
                curHeader.SurveyID = data["cboSurveyID"].PassNull().ToUpper().Trim();
                curHeader.BranchID = data["cboDistributor"];
                if (Handle != "N")
                {
                    curHeader.Status = Handle;
                }

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSurveyCriteria"]);

                ChangeRecords<OM20090_pgCompetitorSurveyCriteria_Result> lstSurveyCriteria = dataHandler1.BatchObjectData<OM20090_pgCompetitorSurveyCriteria_Result>();



                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstSurveyInvt"]);

                ChangeRecords<OM20090_pgCompetitorSurveyInvt_Result> lstSurveyInvt = dataHandler2.BatchObjectData<OM20090_pgCompetitorSurveyInvt_Result>();



                #region Save Header Company

                var header = _db.OM_CompetitorSurveyHeader.FirstOrDefault(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim());

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

                    header = new OM_CompetitorSurveyHeader();

                    header.ResetET();

                    header.BranchID = curHeader.BranchID;
                    header.SurveyID = curHeader.SurveyID.ToUpper().Trim();

                    header.Crtd_Datetime = DateTime.Now;

                    header.Crtd_Prog = _screenNbr;

                    header.Crtd_User = Current.UserName;

                    UpdatingHeader(ref header, curHeader);

                    _db.OM_CompetitorSurveyHeader.AddObject(header);

                }

                #endregion



                #region Save OM_CompetitorSurveyInvt

                foreach (OM20090_pgCompetitorSurveyInvt_Result deleted in lstDel)
                {
                    if (lstSurveyInvt.Created.Where(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.CompInvtID == deleted.CompID && p.CompID == deleted.CompID).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {

                        lstSurveyInvt.Created.Where(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.CompInvtID == deleted.CompID && p.CompID == deleted.CompID).FirstOrDefault().tstamp = deleted.tstamp;

                    }

                    else
                    {

                        var objDelete = _db.OM_CompetitorSurveyInvt.FirstOrDefault(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.CompInvtID == deleted.CompInvtID && p.CompID == deleted.CompID);

                        if (objDelete != null)
                        {

                            _db.OM_CompetitorSurveyInvt.DeleteObject(objDelete);

                        }

                    }
                }


                foreach (OM20090_pgCompetitorSurveyInvt_Result deleted in lstSurveyInvt.Deleted)
                {

                    if (lstSurveyInvt.Created.Where(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.CompInvtID == deleted.CompID && p.CompID == deleted.CompID).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {

                        lstSurveyInvt.Created.Where(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.CompInvtID == deleted.CompID && p.CompID == deleted.CompID).FirstOrDefault().tstamp = deleted.tstamp;

                    }

                    else
                    {

                        var objDelete = _db.OM_CompetitorSurveyInvt.FirstOrDefault(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.InvtID == deleted.InvtID && p.CompInvtID == deleted.CompInvtID && p.CompID == deleted.CompID);

                        if (objDelete != null)
                        {

                            _db.OM_CompetitorSurveyInvt.DeleteObject(objDelete);

                        }

                    }

                }



                lstSurveyInvt.Created.AddRange(lstSurveyInvt.Updated);



                foreach (OM20090_pgCompetitorSurveyInvt_Result curRow in lstSurveyInvt.Created)
                {

                    if (curRow.CompInvtID.PassNull() == "" || curRow.CompID.PassNull() == "") continue;



                    var RowDB = _db.OM_CompetitorSurveyInvt.FirstOrDefault(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.InvtID == curRow.InvtID && p.CompInvtID == curRow.CompID && p.CompID == curRow.CompID);



                    if (RowDB != null)
                    {

                        if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                        {
                            UpdatingCompetitorSurveyInvt(RowDB, curRow, false);

                        }

                        else
                        {

                            throw new MessageException(MessageType.Message, "19");

                        }

                    }

                    else
                    {

                        RowDB = new OM_CompetitorSurveyInvt();

                        RowDB.ResetET();
                        RowDB.BranchID = curHeader.BranchID;
                        RowDB.SurveyID = curHeader.SurveyID.ToUpper().Trim();

                        UpdatingCompetitorSurveyInvt(RowDB, curRow, true);

                        _db.OM_CompetitorSurveyInvt.AddObject(RowDB);

                    }

                }

                #endregion



                #region Save SYS_SubCompany

                foreach (OM20090_pgCompetitorSurveyCriteria_Result deleted in lstSurveyCriteria.Deleted)
                {

                    if (lstSurveyCriteria.Created.Where(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.CriteriaID == deleted.CriteriaID).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {

                        lstSurveyCriteria.Created.Where(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.CriteriaID == deleted.CriteriaID).FirstOrDefault().tstamp = deleted.tstamp;

                    }

                    else
                    {

                        var del = _db.OM_CompetitorSurveyCriteria.FirstOrDefault(p => p.BranchID == curHeader.BranchID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.CriteriaID == deleted.CriteriaID);

                        if (del != null)
                        {

                            _db.OM_CompetitorSurveyCriteria.DeleteObject(del);

                        }

                    }

                }



                lstSurveyCriteria.Created.AddRange(lstSurveyCriteria.Updated);



                foreach (OM20090_pgCompetitorSurveyCriteria_Result curRow in lstSurveyCriteria.Created)
                {

                    if (curRow.CriteriaID.PassNull() == "") continue;



                    var RowDB = _db.OM_CompetitorSurveyCriteria.FirstOrDefault(p => p.CriteriaID.ToUpper() == curRow.CriteriaID && p.SurveyID == curHeader.SurveyID.ToUpper().Trim() && p.BranchID == curHeader.BranchID);



                    if (RowDB != null)
                    {

                        if (RowDB.tstamp.ToHex() == curRow.tstamp.ToHex())
                        {

                            UpdatingOM_CompetitorSurveyCriteria(RowDB, curRow, false);

                        }

                        else
                        {

                            throw new MessageException(MessageType.Message, "19");

                        }

                    }

                    else
                    {

                        RowDB = new OM_CompetitorSurveyCriteria();

                        RowDB.ResetET();

                        RowDB.BranchID = curHeader.BranchID;
                        RowDB.SurveyID = curHeader.SurveyID.ToUpper().Trim();

                        UpdatingOM_CompetitorSurveyCriteria(RowDB, curRow, true);

                        _db.OM_CompetitorSurveyCriteria.AddObject(RowDB);

                    }

                }

                #endregion



                _db.SaveChanges();

                return Util.CreateMessage(MessageProcess.Save, header);

            }

            catch (Exception ex)
            {

                if (ex is MessageException) return (ex as MessageException).ToMessage();

                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });

            }

        }

        [HttpPost]

        public ActionResult DeleteAll(FormCollection data)
        {

            try
            {

                string CpnyID = data["cboDistributor"];
                string SurveyID = data["cboSurveyID"];

                var cpny = _db.OM_CompetitorSurveyHeader.FirstOrDefault(p => p.BranchID == CpnyID && p.SurveyID == SurveyID.ToUpper().Trim());

                if (cpny != null)
                {

                    _db.OM_CompetitorSurveyHeader.DeleteObject(cpny);

                }



                var lstAddr = _db.OM_CompetitorSurveyCriteria.Where(p => p.BranchID == CpnyID && p.SurveyID == SurveyID.ToUpper().Trim()).ToList();

                foreach (var item in lstAddr)
                {

                    _db.OM_CompetitorSurveyCriteria.DeleteObject(item);

                }



                var lstSub = _db.OM_CompetitorSurveyInvt.Where(p => p.BranchID == CpnyID && p.SurveyID == SurveyID.ToUpper().Trim()).ToList();

                foreach (var item in lstSub)
                {

                    _db.OM_CompetitorSurveyInvt.DeleteObject(item);

                }



                _db.SaveChanges();

                return Util.CreateMessage(MessageProcess.Delete, CpnyID);

            }

            catch (Exception ex)
            {

                if (ex is MessageException) return (ex as MessageException).ToMessage();

                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });

            }

        }

        private void UpdatingHeader(ref OM_CompetitorSurveyHeader t, OM_CompetitorSurveyHeader s)
        {

            t.FromDate = s.FromDate;

            t.ToDate = s.ToDate;

            t.Status = s.Status;

            t.SurveyName = s.SurveyName;

            t.LUpd_Datetime = DateTime.Now;

            t.LUpd_Prog = _screenNbr;

            t.LUpd_User = Current.UserName;
        }

        private void UpdatingCompetitorSurveyInvt(OM_CompetitorSurveyInvt t, OM20090_pgCompetitorSurveyInvt_Result s, bool isNew)
        {

            if (isNew)
            {
                t.InvtID = s.InvtID;

                t.CompID = s.CompID;

                t.CompInvtID = s.CompInvtID;

                t.Crtd_Datetime = DateTime.Now;

                t.Crtd_Prog = _screenNbr;

                t.Crtd_User = Current.UserName;

            }
            
            t.LUpd_Datetime = DateTime.Now; 
            t.LUpd_User = Current.UserName;
            t.LUpd_Prog = _screenNbr;
        }

        private void UpdatingOM_CompetitorSurveyCriteria(OM_CompetitorSurveyCriteria t, OM20090_pgCompetitorSurveyCriteria_Result s, bool isNew)
        {

            if (isNew)
            {

                t.CriteriaID = s.CriteriaID;

                t.Crtd_Datetime = DateTime.Now;

                t.Crtd_Prog = _screenNbr;

                t.Crtd_User = Current.UserName;

            }
            t.Required = s.Required;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_User = Current.UserName;
            t.LUpd_Prog = _screenNbr;
        }
    }
}

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
using HQ.eSkySys;
namespace OM22300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22300Controller : Controller
    {
        string screenNbr = "OM22300";
        OM22300Entities _db = Util.CreateObjectContext<OM22300Entities>(false);


        public ActionResult Index()
        {
            ViewBag.BussinessDate = DateTime.Now.ToDateShort();
            ViewBag.BussinessTime = DateTime.Now;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
          
            return PartialView();
        }

        //public ActionResult GetMailHeader(string mailID)
        //{
        //    ViewBag.BussinessDate = DateTime.Now.ToDateShort();
        //    ViewBag.BussinessTime = DateTime.Now;
        //    var obj=_db.Server_MailAutoHeader.FirstOrDefault(p => p.MailID == mailID);
        //    if(obj!=null)
        //    obj.PassUnZip=obj.PassUnZip.ToString().PassNull()==""?"":Encryption.Decrypt(obj.PassUnZip.ToString(), "1210Hq10s081f359t");
        //    return this.Store(obj);
        //}
        public ActionResult GetPJPDetail(string salesRouteID, string custID, string slsPerID, DateTime fromDate, DateTime toDate, string slsFreqType)
        {
            return this.Store(_db.OM22300_pgLoadPJPDet(Current.UserName, Current.LangID, Current.CpnyID, salesRouteID, custID, slsPerID, fromDate, toDate, slsFreqType).ToList()); //.Where(p => p.MailID == mailID));
        }
       
        [HttpPost]
        public ActionResult Save(FormCollection data, bool isNew)
        {
            try
            {
            //    string mailId = isNew ? DateTime.Now.ToString("yyyyMMddhhmmssff") : data["cboCustID"];
            //    StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
            //    ChangeRecords<OM_SalesRouteDet> lstgrd = dataHandler1.BatchObjectData<OM_SalesRouteDet>();
            //    StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
            //    ChangeRecords<Server_MailAutoHeader> lstheader = dataHandler2.BatchObjectData<Server_MailAutoHeader>();
            //    //xoa cac record tren luoi
            //    foreach (OM_SalesRouteDet deleted in lstgrd.Deleted)
            //    {
            //        var del = _db.OM_SalesRouteDet.Where(p => p.MailID == mailId && p.ReportID == deleted.ReportID && p.ReportViewID == deleted.ReportViewID).FirstOrDefault();
            //        if (del != null)
            //        {
            //            _db.OM_SalesRouteDet.DeleteObject(del);
            //        }
            //    }
            //    //them hoac update cac record tren luoi
            //    lstgrd.Created.AddRange(lstgrd.Updated);
            //    foreach (OM_SalesRouteDet created in lstgrd.Created)
            //    {
            //        if (created.ReportID.PassNull() == "") continue;
            //        var record = _db.OM_SalesRouteDet.Where(p => p.MailID == mailId && p.ReportID == created.ReportID && p.ReportViewID == created.ReportViewID).FirstOrDefault();
            //        if (created.tstamp.ToHex() == "")//dong nay la dong them moi
            //        {
            //            if (record == null)
            //            {
            //                record = new OM_SalesRouteDet();
            //                record.MailID = mailId;
            //                record.ReportID = created.ReportID;
            //                record.ReportViewID = created.ReportViewID;
            //                record.Crtd_Datetime = DateTime.Now;
            //                record.Crtd_Prog = screenNbr;
            //                record.Crtd_User = Current.UserName;
            //                record.tstamp = new byte[0];
            //                UpdatingOM_SalesRouteDet(created, ref record);
            //                _db.OM_SalesRouteDet.AddObject(record);


            //            }
            //            else
            //            {
            //                throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
            //            }
            //        }
            //        else
            //        {
            //            if (created.tstamp.ToHex() == record.tstamp.ToHex())
            //            {
            //                UpdatingOM_SalesRouteDet(created, ref record);
            //            }
            //            else
            //            {
            //                throw new MessageException(MessageType.Message, "19");
            //            }
            //        }
            //    }
            //    //xu li header
            //    foreach (Server_MailAutoHeader created in lstheader.Updated)
            //    {
            //        var objHeader = _db.Server_MailAutoHeader.Where(p => p.MailID == created.MailID).FirstOrDefault();
            //        if (isNew)//new record
            //        {
            //            if (objHeader != null)
            //                return Json(new { success = false, msgCode = 2000, msgParam = mailId });//quang message ma nha cung cap da ton tai ko the them
            //            else
            //            {
            //                objHeader = new Server_MailAutoHeader();
            //                objHeader.MailID = mailId;
            //                objHeader.Crtd_Datetime = DateTime.Now;
            //                objHeader.Crtd_Prog = screenNbr;
            //                objHeader.Crtd_User = Current.UserName;
            //                objHeader.tstamp = new byte[0];
            //                UpdatingHeader(created, ref objHeader);
            //                _db.Server_MailAutoHeader.AddObject(objHeader);
                          
            //            }
            //        }
            //        else if (objHeader != null)//update record
            //        {
            //            if (objHeader.tstamp.ToHex() == created.tstamp.ToHex())
            //            {
            //                UpdatingHeader(created, ref objHeader);
            //            }
            //            else
            //            {
            //                throw new MessageException(MessageType.Message, "19");
            //            }                  
            //        }
            //        else
            //        {
            //            throw new MessageException(MessageType.Message, "19");
            //        }

            //    }
            //    _db.SaveChanges();
              //  return Json(new { success = true, mailId = mailId });
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }

        }
        [HttpPost]
        public ActionResult Delete(string mailId)
        {
            try
            {
                ////xoa grid
                //var lstDetail = _db.OM_SalesRouteDet.Where(p => p.MailID == mailId).ToList();
                //for (int i = 0; i < lstDetail.Count(); i++)
                //{
                //    _db.OM_SalesRouteDet.DeleteObject(lstDetail[i]);                    
                //}
                ////xoa header
                //var cpny = _db.Server_MailAutoHeader.FirstOrDefault(p => p.MailID == mailId);
                //if (cpny != null)
                //{
                //    _db.Server_MailAutoHeader.DeleteObject(cpny);
                //}
                //_db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
        }

        private void UpdatingOM_SalesRouteDet(OM22300_pgLoadPJPDet_Result s, ref OM_SalesRouteDet d)
        {
            d.BranchID = Current.CpnyID.ToString();
            d.SalesRouteID = s.SalesRouteID;
            d.CustID = s.CustID;
            d.SlsPerID = s.SlsPerID;
            d.VisitDate = s.VisitDate;
            d.DayofWeek = s.DayofWeek;
            d.PJPID = s.PJPID;
            d.SlsFreq = s.SlsFreq;
            d.SlsFreqType = s.SlsFreqType;
            d.WeekofVisit = s.WeekofVisit;
            d.WeekNbr = s.WeekNbr;
            d.VisitSort = s.VisitSort;
           
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        //private void UpdatingHeader(Server_MailAutoHeader s, ref Server_MailAutoHeader d)
        //{
        //    d.MailTo = s.MailTo.Replace(",",";");  
        //    d.MailCC = s.MailCC.Replace(",", ";");
        //    d.Subject = s.Subject;
        //    d.TemplateFile = s.TemplateFile;
        //    d.ExportFolder = s.ExportFolder;
        //    d.Active = s.Active;
        //    d.TypeAuto = s.TypeAuto;
        //    d.DateTime = s.DateTime;
        //    d.Time =new DateTime(s.DateTime.Year,s.DateTime.Month,s.DateTime.Day,s.Time.Value.Hour,s.Time.Value.Minute,0);


        //    d.IsNotDeleteFile = s.IsNotDeleteFile;
        //    d.IsNotAttachFile = s.IsNotAttachFile;

        //    d.Header = s.Header;
        //    d.Body = s.Body;
        //    d.PassUnZip = s.PassUnZip.PassNull().Trim()==string.Empty?"": Encryption.Encrypt(s.PassUnZip, "1210Hq10s081f359t"); ;
        //    d.FileName = s.FileName;
                 
        //    d.LUpd_Datetime = DateTime.Now;
        //    d.LUpd_Prog = screenNbr;
        //    d.LUpd_User = Current.UserName;
        //}
    }
}

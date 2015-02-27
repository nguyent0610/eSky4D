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
namespace SA40300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA40300Controller : Controller
    {
        string screenNbr = "SA40300";
        SA40300Entities _db = Util.CreateObjectContext<SA40300Entities>(true);


        public ActionResult Index()
        {
            
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetMailHeader(String mailID)
        {
            var obj=_db.Server_MailAutoHeader.FirstOrDefault(p => p.MailID == mailID);
            if(obj!=null)
            obj.PassUnZip=obj.PassUnZip.ToString().PassNull()==""?"":Encryption.Decrypt(obj.PassUnZip.ToString(), "1210Hq10s081f359t");
            return this.Store(obj);
        }
        public ActionResult GetMailDetail(String mailID)
        {
           // var lst = .ToList();
            return this.Store(_db.Server_MailAutoDetail.Where(p => p.MailID == mailID));
        }
       
        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string mailId)
        {
            string mailidNew = DateTime.Now.ToString("yyyyMMddhhmmssff");
            StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<Server_MailAutoDetail> lstgrd = dataHandler1.BatchObjectData<Server_MailAutoDetail>();
            StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
            ChangeRecords<Server_MailAutoHeader> lstheader = dataHandler2.BatchObjectData<Server_MailAutoHeader>();
            foreach (Server_MailAutoDetail deleted in lstgrd.Deleted)
            {
                var del = _db.Server_MailAutoDetail.Where(p => p.MailID == mailId && p.ReportID == deleted.ReportID && p.ReportViewID == deleted.ReportViewID).FirstOrDefault();
                if (del != null)
                {
                    _db.Server_MailAutoDetail.DeleteObject(del);

                }

            }
            foreach (Server_MailAutoDetail created in lstgrd.Created)
            {
                
                var record = _db.Server_MailAutoDetail.Where(p => p.MailID == mailId && p.ReportID == created.ReportID && p.ReportViewID == created.ReportViewID).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new Server_MailAutoDetail();
                        if (mailId == "")
                        {
                            mailId = mailidNew;
                        }
                        
                        record.MailID = mailId;
                        record.ReportID = created.ReportID;
                        record.ReportViewID = created.ReportViewID;
                        record.LoggedCpnyID = created.LoggedCpnyID;
                        record.CpnyID = created.CpnyID;
                        record.LangID = created.LangID;
                        record.StringParm00 = created.StringParm00;
                        record.StringParm01 = created.StringParm01;
                        record.StringParm02 = created.StringParm02;
                        record.StringParm03 = created.StringParm03;
                        record.BeforeDateParm00 = created.BeforeDateParm00;
                        record.BeforeDateParm01 = created.BeforeDateParm01;
                        record.BeforeDateParm02 = created.BeforeDateParm02;
                        record.BeforeDateParm03 = created.BeforeDateParm03;
                        record.BooleanParm00 = created.BooleanParm00;
                        record.BooleanParm01 = created.BooleanParm01;
                        record.BooleanParm02 = created.BooleanParm02;
                        record.BooleanParm03 = created.BooleanParm03;
                        record.ListParm00 = created.ListParm00;
                        record.ListParm01 = created.ListParm01;
                        record.ListParm02 = created.ListParm02;
                        record.ListParm03 = created.ListParm03;
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        if (record.ReportID != "" && record.ReportViewID != "")
                        {
                            UpdatingServer_MailAutoDetail(created, ref record);
                            _db.Server_MailAutoDetail.AddObject(record);
                        }

                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            code = "151",
                            colName = Util.GetLang("ReportViewID"),
                            value = created.ReportViewID
                        }, JsonRequestBehavior.AllowGet);
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
            }



            foreach (Server_MailAutoDetail updated in lstgrd.Updated)
            {

                var record = _db.Server_MailAutoDetail.Where(p => p.MailID == mailId && p.ReportID == updated.ReportID && p.ReportViewID == updated.ReportViewID).FirstOrDefault();


                if (record != null)
                {

                    if (record.tstamp.ToHex() != updated.tstamp.ToHex())
                    {
                        return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    }
                    UpdatingServer_MailAutoDetail(updated, ref record);
                }
                else
                {
                    if (updated.tstamp.ToHex() == "")
                    {
                        record = new Server_MailAutoDetail();
                        record.MailID = mailId;
                        record.ReportID = updated.ReportID;
                        record.ReportViewID = updated.ReportViewID;
                        record.LoggedCpnyID = updated.LoggedCpnyID;
                        record.CpnyID = updated.CpnyID;
                        record.LangID = updated.LangID;
                        record.StringParm00 = updated.StringParm00;
                        record.StringParm01 = updated.StringParm01;
                        record.StringParm02 = updated.StringParm02;
                        record.StringParm03 = updated.StringParm03;
                        record.BeforeDateParm00 = updated.BeforeDateParm00;
                        record.BeforeDateParm01 = updated.BeforeDateParm01;
                        record.BeforeDateParm02 = updated.BeforeDateParm02;
                        record.BeforeDateParm03 = updated.BeforeDateParm03;
                        record.BooleanParm00 = updated.BooleanParm00;
                        record.BooleanParm01 = updated.BooleanParm01;
                        record.BooleanParm02 = updated.BooleanParm02;
                        record.BooleanParm03 = updated.BooleanParm03;
                        record.ListParm00 = updated.ListParm00;
                        record.ListParm01 = updated.ListParm01;
                        record.ListParm02 = updated.ListParm02;
                        record.ListParm03 = updated.ListParm03;
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingServer_MailAutoDetail(updated, ref record);
                        _db.Server_MailAutoDetail.AddObject(record);

                    }
                    else
                    {

                        return Json(new { success = false, code = "19" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }

            foreach (Server_MailAutoHeader updated in lstheader.Updated)
            {
                // Get the image path


                var objHeader = _db.Server_MailAutoHeader.Where(p => p.MailID == updated.MailID).FirstOrDefault();
                if (objHeader != null)
                {                                     
                        UpdatingHeader(updated, ref objHeader);                    
                }
            }
            foreach (Server_MailAutoHeader created in lstheader.Created)
            {
                var objHeader = _db.Server_MailAutoHeader.Where(p => p.MailID == mailidNew).FirstOrDefault();
                if (objHeader == null)
                {
                    objHeader = new Server_MailAutoHeader();
                    objHeader.MailID = mailidNew;
                    objHeader.Crtd_Datetime = DateTime.Now;
                    objHeader.Crtd_Prog = screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    objHeader.tstamp = new byte[0];
                    UpdatingHeader(created, ref objHeader);
                    _db.Server_MailAutoHeader.AddObject(objHeader);
                    _db.SaveChanges();

                }
            }


            _db.SaveChanges();
            //this.Direct();

            if (mailId == "")
            {
                mailId = mailidNew;
            }
            return Json(new { success = true, value = mailId }, JsonRequestBehavior.AllowGet);
        }
        [DirectMethod]
        public ActionResult SA40300Delete(string mailid)
        {
            var cpny = _db.Server_MailAutoHeader.FirstOrDefault(p => p.MailID == mailid);
            
            
                _db.Server_MailAutoHeader.DeleteObject(cpny);
               
            

            _db.SaveChanges();
            return this.Direct();
        }

        private void UpdatingServer_MailAutoDetail(Server_MailAutoDetail s, ref Server_MailAutoDetail d)
        {
            d.LoggedCpnyID = s.LoggedCpnyID;
            d.CpnyID = s.CpnyID;
            d.LangID = s.LangID;
            d.StringParm00 = s.StringParm00;
            d.StringParm01 = s.StringParm01;
            d.StringParm02 = s.StringParm02;
            d.StringParm03 = s.StringParm03;
            d.BeforeDateParm00 = s.BeforeDateParm00;
            d.BeforeDateParm01 = s.BeforeDateParm01;
            d.BeforeDateParm02 = s.BeforeDateParm02;
            d.BeforeDateParm03 = s.BeforeDateParm03;
            d.BooleanParm00 = s.BooleanParm00;
            d.BooleanParm01 = s.BooleanParm01;
            d.BooleanParm02 = s.BooleanParm02;
            d.BooleanParm03 = s.BooleanParm03;
            d.ListParm00 = s.ListParm00;
            d.ListParm01 = s.ListParm01;
            d.ListParm02 = s.ListParm02;
            d.ListParm03 = s.ListParm03;


            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void UpdatingHeader(Server_MailAutoHeader s, ref Server_MailAutoHeader d)
        {

            d.MailTo = s.MailTo.Replace(",",";");  // ???
            d.MailCC = s.MailCC.Replace(",", ";");
            d.Subject = s.Subject;
            d.TemplateFile = s.TemplateFile;
            d.ExportFolder = s.ExportFolder;
            d.Active = s.Active;
            d.TypeAuto = s.TypeAuto;
            d.DateTime = s.DateTime;
            d.Header = s.Header;
            d.Body = s.Body;
            d.PassUnZip = s.PassUnZip.PassNull().Trim()==string.Empty?"": Encryption.Encrypt(s.PassUnZip, "1210Hq10s081f359t"); ;
            d.FileName = s.FileName;
                 
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

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
            ViewBag.BussinessDate = DateTime.Now.ToDateShort();
            ViewBag.BussinessTime = DateTime.Now;
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
          
            return PartialView();
        }

        public ActionResult GetMailHeader(string mailID)
        {
            ViewBag.BussinessDate = DateTime.Now.ToDateShort();
            ViewBag.BussinessTime = DateTime.Now;
            var obj=_db.Server_MailAutoHeader.FirstOrDefault(p => p.MailID == mailID);
            if (obj != null)
            {
                obj.PassUnZip = obj.PassUnZip.ToString().PassNull() == "" ? "" : Encryption.Decrypt(obj.PassUnZip.ToString(), "1210Hq10s081f359t");
            }
            return this.Store(obj);
        }
        public ActionResult GetMailDetail(string mailID)
        {
            return this.Store(_db.Server_MailAutoDetail.Where(p => p.MailID == mailID));
        }
        public ActionResult GetMailAutoUser(string emailID, string sendType, string listUser)
        {
            var lstData = _db.SA40300_pgMailtAutoUser(emailID, sendType, listUser, Current.UserName, Current.CpnyID, Current.LangID);
            return this.Store(lstData);
        }
        [HttpPost]
        public ActionResult Save(FormCollection data, bool isNew)
        {
            try
            {
                string mailType = data["cboMailType"].PassNull();
                string mailId = isNew ? DateTime.Now.ToString("yyyyMMddhhmmssff") : data["cboMailID"];
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
                ChangeRecords<Server_MailAutoDetail> lstgrd = dataHandler1.BatchObjectData<Server_MailAutoDetail>();
                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
                ChangeRecords<Server_MailAutoHeader> lstheader = dataHandler2.BatchObjectData<Server_MailAutoHeader>();
                //xoa cac record tren luoi
                foreach (Server_MailAutoDetail deleted in lstgrd.Deleted)
                {
                    var del = _db.Server_MailAutoDetail.Where(p => p.MailID == mailId && p.ReportID == deleted.ReportID && p.ReportViewID == deleted.ReportViewID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.Server_MailAutoDetail.DeleteObject(del);
                    }
                }
                //them hoac update cac record tren luoi
                lstgrd.Created.AddRange(lstgrd.Updated);
                foreach (Server_MailAutoDetail created in lstgrd.Created)
                {
                    if (created.ReportID.PassNull() == "") continue;
                    var record = _db.Server_MailAutoDetail.Where(p => p.MailID == mailId && p.ReportID == created.ReportID && p.ReportViewID == created.ReportViewID).FirstOrDefault();
                    if (created.tstamp.ToHex() == "")//dong nay la dong them moi
                    {
                        if (record == null)
                        {
                            record = new Server_MailAutoDetail();
                            record.MailID = mailId;
                            record.ReportID = created.ReportID;
                            record.ReportViewID = created.ReportViewID;
                            record.Crtd_Datetime = DateTime.Now;
                            record.Crtd_Prog = screenNbr;
                            record.Crtd_User = Current.UserName;
                            record.tstamp = new byte[0];
                            UpdatingServer_MailAutoDetail(created, ref record);
                            _db.Server_MailAutoDetail.AddObject(record);

                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
                        }
                    }
                    else
                    {
                        if (created.tstamp.ToHex() == record.tstamp.ToHex())
                        {
                            UpdatingServer_MailAutoDetail(created, ref record);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                }
                //xu li header
                foreach (Server_MailAutoHeader created in lstheader.Updated)
                {
                    var objHeader = _db.Server_MailAutoHeader.Where(p => p.MailID == created.MailID).FirstOrDefault();
                    if (isNew)//new record
                    {
                        if (objHeader != null)
                            return Json(new { success = false, msgCode = 2000, msgParam = mailId });//quang message ma nha cung cap da ton tai ko the them
                        else
                        {
                            objHeader = new Server_MailAutoHeader();
                            objHeader.MailType = mailType;
                            objHeader.MailID = mailId;
                            objHeader.Crtd_Datetime = DateTime.Now;
                            objHeader.Crtd_Prog = screenNbr;
                            objHeader.Crtd_User = Current.UserName;
                            objHeader.tstamp = new byte[0];
                            UpdatingHeader(created, ref objHeader);
                            _db.Server_MailAutoHeader.AddObject(objHeader);
                          
                        }
                    }
                    else if (objHeader != null)//update record
                    {
                        if (objHeader.tstamp.ToHex() == created.tstamp.ToHex())
                        {
                            UpdatingHeader(created, ref objHeader);
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
                _db.SaveChanges();
                return Json(new { success = true, mailId = mailId });
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
                //xoa grid
                var lstDetail = _db.Server_MailAutoDetail.Where(p => p.MailID == mailId).ToList();
                for (int i = 0; i < lstDetail.Count(); i++)
                {
                    _db.Server_MailAutoDetail.DeleteObject(lstDetail[i]);                    
                }
                //xoa header
                var cpny = _db.Server_MailAutoHeader.FirstOrDefault(p => p.MailID == mailId);
                if (cpny != null)
                {
                    _db.Server_MailAutoHeader.DeleteObject(cpny);
                }
                // Xoa Server_MailAutoUser
                var lstMail = _db.Server_MailAutoUser.Where(p => p.MailID == mailId).ToList();
                for (int i = 0; i < lstMail.Count(); i++)
                {
                    _db.Server_MailAutoUser.DeleteObject(lstMail[i]);
                }
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
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

            d.StoreName = s.StoreName;

            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private void UpdatingHeader(Server_MailAutoHeader s, ref Server_MailAutoHeader d)
        {
            if (d.MailType == "HTML")
            {
                string mailID = d.MailID.ToUpper();
                var lstMail = _db.Server_MailAutoUser.Where(p => p.MailID == s.MailID && p.SendType == "TO").ToList();
                if (s.MailTo.Length > 0)
                {
                    string[] mailTos = s.MailTo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in lstMail)
                    {
                        if (mailTos.FirstOrDefault(x => x == item.UserID) == null)
                        {
                            _db.Server_MailAutoUser.DeleteObject(item);
                        }
                    }
                    // Delete 
                    foreach (var item in mailTos)
                    {
                        var obj = _db.Server_MailAutoUser.FirstOrDefault(x => x.MailID.ToUpper() == mailID && x.UserID == item && x.SendType == "TO");
                        if (obj == null)
                        {
                            obj = new Server_MailAutoUser();
                            obj.UserID = item;
                            obj.SendType = "TO";
                            obj.MailID = d.MailID;
                            obj.Crtd_DateTime = DateTime.Now;
                            obj.Crtd_Prog = screenNbr;
                            obj.Crtd_User = Current.UserName;
                            obj.LUpd_DateTime = DateTime.Now;
                            obj.LUpd_Prog = screenNbr;
                            obj.LUpd_User = Current.UserName;
                            _db.Server_MailAutoUser.AddObject(obj);
                        }
                    }
                }
                else
                {
                    // Xoa Server_MailAutoUser                    
                    for (int i = 0; i < lstMail.Count(); i++)
                    {
                        _db.Server_MailAutoUser.DeleteObject(lstMail[i]);
                    }
                }

                // Mail CC
                lstMail = _db.Server_MailAutoUser.Where(p => p.MailID == s.MailID && p.SendType == "CC").ToList();
                if (s.MailCC.Length > 0)
                {
                    string[] mailTos = s.MailCC.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in lstMail)
                    {
                        if (mailTos.FirstOrDefault(x => x == item.UserID) == null)
                        {
                            _db.Server_MailAutoUser.DeleteObject(item);
                        }
                    }
                    // Delete 
                    foreach (var item in mailTos)
                    {
                        var obj = _db.Server_MailAutoUser.FirstOrDefault(x => x.MailID.ToUpper() == mailID && x.UserID == item && x.SendType == "CC");
                        if (obj == null)
                        {
                            obj = new Server_MailAutoUser();
                            obj.UserID = item;
                            obj.SendType = "CC";
                            obj.MailID = d.MailID;
                            obj.Crtd_DateTime = DateTime.Now;
                            obj.Crtd_Prog = screenNbr;
                            obj.Crtd_User = Current.UserName;
                            obj.LUpd_DateTime = DateTime.Now;
                            obj.LUpd_Prog = screenNbr;
                            obj.LUpd_User = Current.UserName;
                            _db.Server_MailAutoUser.AddObject(obj);
                        }                        
                    }
                }
                else
                {
                    // Xoa Server_MailAutoUser                    
                    for (int i = 0; i < lstMail.Count(); i++)
                    {
                        _db.Server_MailAutoUser.DeleteObject(lstMail[i]);
                    }
                }
            }
            d.MailTo = s.MailTo;//.Replace(",",";");  
            d.MailCC = s.MailCC;//.Replace(",", ";");
            d.Subject = s.Subject;
            d.TemplateFile = s.TemplateFile;
            d.ExportFolder = s.ExportFolder;
            d.Active = s.Active;
            d.TypeAuto = s.TypeAuto;
            d.DateTime = s.DateTime;
            d.Time =new DateTime(s.DateTime.Year,s.DateTime.Month,s.DateTime.Day,s.Time.Value.Hour,s.Time.Value.Minute,0);
            
            d.IsNotDeleteFile = s.IsNotDeleteFile;
            d.IsNotAttachFile = s.IsNotAttachFile;

            d.Header = s.Header;
            d.Body = s.Body;
            d.PassUnZip = s.PassUnZip.PassNull().Trim()==string.Empty?"": Encryption.Encrypt(s.PassUnZip, "1210Hq10s081f359t"); ;
            d.FileName = s.FileName;
            d.IsZipFile = s.IsZipFile;
            d.UseStore = s.UseStore;

            d.StoreName = s.StoreName.PassNull();
            d.SplitMailTo = s.SplitMailTo;

            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

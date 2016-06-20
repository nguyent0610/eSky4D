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
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mime;
namespace SA02400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA02400Controller : Controller
    {
        private string _screenNbr = "SA02400";
        private string _userName = Current.UserName;

        SA02400Entities _db = Util.CreateObjectContext<SA02400Entities>(false);

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

        public ActionResult GetSA02400()
        {
            var setupData = _db.HO_EmailConfig.FirstOrDefault(p => p.EmailID.ToUpper() == "APPROVE");
            setupData.Pass = Encryption.Decrypt(setupData.Pass, "1210Hq10s081f359t");
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data,bool isNew)
        {
            
            try
            {
                // Get params from data that's sent from client (Ajax)
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSA02400"]);
                ChangeRecords<HO_EmailConfig> lstSA02400 = dataHandler.BatchObjectData<HO_EmailConfig>();
                foreach (HO_EmailConfig setup in lstSA02400.Updated)
                {
                    var objHeader = _db.HO_EmailConfig.FirstOrDefault(p => p.EmailID.ToUpper() == "APPROVE");
                    if (isNew)//new record
                    {
                        if (objHeader != null)
                        { }//quang message ma nha cung cap da ton tai ko the them
                        else
                        {
                            objHeader = new HO_EmailConfig();
                            objHeader.EmailID = "APPROVE";
                            objHeader.Crtd_Datetime = DateTime.Now;
                            objHeader.Crtd_Prog = _screenNbr;
                            objHeader.Crtd_User = Current.UserName;
                            UpdatingHeader(setup, ref objHeader);
                            // Add data to HO_EmailConfig
                            _db.HO_EmailConfig.AddObject(objHeader);
                            _db.SaveChanges();
                        }
                    }
                    else if (objHeader != null)//update record
                    {
                        if (objHeader.tstamp.ToHex() == setup.tstamp.ToHex())
                        {
                            UpdatingHeader(setup, ref objHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(HO_EmailConfig s,ref HO_EmailConfig d)
        {

            d.SMTPServer = s.SMTPServer;
            d.UserName = s.UserName;
            d.Name = s.Name;
            d.MailBox = s.MailBox;
            d.Port = s.Port;
            d.SSL = s.SSL;
            try
            {
                d.Pass = Encryption.Encrypt(s.Pass, "1210Hq10s081f359t");
            }
            catch
            {
                d.Pass = string.Empty;
            }

            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult Send(string toEmail, string subject, string content)
        {
            try 
	        {	        
		        var setupData = _db.HO_EmailConfig.FirstOrDefault(p => p.EmailID.ToUpper() == "APPROVE");
                SendMail(setupData.SMTPServer, setupData.Port, setupData.SSL, setupData.UserName, setupData.Pass, setupData.MailBox, setupData.Name, toEmail, "", subject, content, "");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void SendMail(string smtp, int port, bool ssl, string userName, string password, string fromMail, string fromName, string toMail, string ccMail, string subject, string content, string attachmentFile)
		{
			try
			{
                if (toMail == string.Empty && ccMail == string.Empty)
                {
                    return;
                }

				SmtpClient smtpClient = new SmtpClient(smtp, port);
                string regexEmail = @"\b[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}\b";
				Regex regex = new Regex(regexEmail);
				if (!regex.IsMatch(fromMail) || !regex.IsMatch(toMail)) throw new Exception("Wrong email address");

				NetworkCredential auth = new NetworkCredential(userName, Encryption.Decrypt(password, "1210Hq10s081f359t"));
				smtpClient.Credentials = auth;
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

				if (ssl) smtpClient.EnableSsl = true;

				MailAddress from = new MailAddress(fromMail, fromName);
				MailMessage mail = new MailMessage();

				string[] cc = ccMail.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var item in cc)
					mail.CC.Add(item);

				string[] to = toMail.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var item in to)
					mail.To.Add(item);

				mail.From = from;
				mail.Subject = subject;
				mail.SubjectEncoding = Encoding.UTF8;

				mail.Body = content;
				mail.BodyEncoding = Encoding.UTF8;
				mail.IsBodyHtml = true;

				if (!string.IsNullOrWhiteSpace(attachmentFile))
				{
					Attachment attachment = new Attachment(attachmentFile, MediaTypeNames.Application.Octet);
					mail.Attachments.Add(attachment);
				}
				try
				{
					smtpClient.SendCompleted += (ss, aa) => {
						smtpClient.Dispose();
						mail.Dispose();
					};
					smtpClient.SendAsync(mail, null);
					//smtpClient.Send(mail);					
				}
				catch (Exception ex)
				{
                    throw ex;// ex.InnerException != null ? ex.InnerException.Message : ex.Message;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		
    }    
}

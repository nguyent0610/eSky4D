using System;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Data.EntityClient;
using System.IO;

namespace HQSendMailApprove
{

    public class GetMailResult
    {
  
        public int ID { get; set; }
  
        public string To { get; set; }
    
        public string CC { get; set; }
    
        public string BranchID { get; set; }
  
        public string Content { get; set; }
        public string Subject { get; set; }
       
    }
    public static class Approve
    {
        #region Trunght

        public static void SendMail(string mailTo, string mailCC, string subject, string content, string[] fullPathAttach = null, string emailID = "")
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            try
            {
                if (mailTo == string.Empty && mailCC == string.Empty) return;
                var email = app.HO_EmailConfig.Where(p => p.EmailID.ToUpper() == (emailID == "" ? "Approve".ToUpper() : emailID.ToUpper())).FirstOrDefault();
                if (email != null)
                {
                    SendMailEnd(email.SMTPServer, email.Port, email.SSL, email.UserName, email.Pass, email.MailBox, email.Name, mailTo, mailCC, subject, content, fullPathAttach);
                }
                else
                {
                    throw new Exception("No email config");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SendMailByte(string mailTo, string mailCC, string subject, string content, List<byte[]> byteMemory = null, string[] fullPathAttach = null, string emailID = "")
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            try
            {
                if (mailTo == string.Empty && mailCC == string.Empty) return;
                var email = app.HO_EmailConfig.Where(p => p.EmailID.ToUpper() == (emailID == "" ? "Approve".ToUpper() : emailID.ToUpper())).FirstOrDefault();
                if (email != null)
                {
                    SendMailEnd(email.SMTPServer, email.Port, email.SSL, email.UserName, email.Pass, email.MailBox, email.Name, mailTo, mailCC, subject, content, byteMemory, fullPathAttach);
                }
                else
                {
                    throw new Exception("No email config");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void SendMailEnd(string smtp, int port, bool ssl, string userName, string password, string fromMail, string fromName, string toMail, string ccMail, string subject, string content,string[] FileAttach =null)
        {
            try
            {


                if (toMail == string.Empty && ccMail == string.Empty) return;

                string regexEmail = @"\b[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}\b";

                using (SmtpClient smtpClient = new SmtpClient(smtp, port))
                {
                    Regex regex = new Regex(regexEmail);
                    if (!regex.IsMatch(fromMail) || !regex.IsMatch(toMail)) throw new Exception("Wrong email address");
                   // smtpClient.UseDefaultCredentials = true;
                    NetworkCredential auth = new NetworkCredential(userName, Encryption.Decrypt(password, "1210Hq10s081f359t"));
                    smtpClient.Credentials = auth;
                    
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    if (ssl) smtpClient.EnableSsl = true;

                    MailAddress from = new MailAddress(fromMail, fromName);
                    using (MailMessage mail = new MailMessage())
                    {
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
                        if (FileAttach != null)
                        {
                            for (var i = 0; i < FileAttach.Length; i++)
                            {
                                mail.Attachments.Add(new Attachment(FileAttach[i]));
                            }
                        }
                        smtpClient.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void SendMailEnd(string smtp, int port, bool ssl, string userName, string password, string fromMail, string fromName, string toMail, string ccMail, string subject, string content, List<byte[]> byteMemory = null,string[] FileAttach =null)
        {
            try
            {


                if (toMail == string.Empty && ccMail == string.Empty) return;

                string regexEmail = @"\b[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}\b";

                using (SmtpClient smtpClient = new SmtpClient(smtp, port))
                {
                    Regex regex = new Regex(regexEmail);
                    if (!regex.IsMatch(fromMail) || !regex.IsMatch(toMail)) throw new Exception("Wrong email address");
                    // smtpClient.UseDefaultCredentials = true;
                    NetworkCredential auth = new NetworkCredential(userName, Encryption.Decrypt(password, "1210Hq10s081f359t"));
                    smtpClient.Credentials = auth;

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    if (ssl) smtpClient.EnableSsl = true;

                    MailAddress from = new MailAddress(fromMail, fromName);
                    using (MailMessage mail = new MailMessage())
                    {
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
                        if (FileAttach != null)
                        {
                            for (var i = 0; i < byteMemory.Count; i++)
                            {
                                mail.Attachments.Add(new Attachment(new MemoryStream(byteMemory[i]), FileAttach[i]));
                            }
                        }

                        smtpClient.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
        public static List<GetMailResult> GetMail(string procName, Dictionary<string, string> parameter)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(app.Connection.ConnectionString);

            SqlCommand cmd = new SqlCommand();
            SqlConnection cn = new SqlConnection(entityBuilder.ProviderConnectionString);
            SqlDataAdapter adap = new SqlDataAdapter();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            try
            {
                // Lấy danh sách email đổ vào DataTable
                cn.Open();
                cmd.Connection = cn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;
                cmd.CommandTimeout = 10000;
                foreach (var parm in parameter)
                {
                    cmd.Parameters.AddWithValue(parm.Key, parm.Value);
                }
                adap.SelectCommand = cmd;
                adap.Fill(dt);

                // Convert DataTable to List
                List<GetMailResult> lstMail = new List<GetMailResult>();
                foreach (DataRow item in dt.Rows)
                {
                    string to = item.Table.Columns.Contains("To") ? (string)item["To"] : string.Empty;
                    if (to == string.Empty) { 
                        continue; 
                    } 
                    lstMail.Add(new GetMailResult() { 
                        ID = item["ID"].ToInt()
                        , To = to
                        , CC = item.Table.Columns.Contains("CC") ? (string)item["CC"] : string.Empty
                        , BranchID = item.Table.Columns.Contains("BranchID")  ? (string)item["BranchID"]: string.Empty
                        , Content =  item.Table.Columns.Contains("Content")  ? (string)item["Content"] : string.Empty                      
                        , Subject = item.Table.Columns.Contains("Subject") ? (string)item["Subject"] : string.Empty
                    });
                }
                return lstMail;

            }
            finally
            {
                cn.Close();
            }

        }
     
        public static string Approve_Content(string procName, Dictionary<string, string> parameter)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(app.Connection.ConnectionString);

            SqlCommand cmd = new SqlCommand();
            SqlConnection cn = new SqlConnection(entityBuilder.ProviderConnectionString);

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            try
            {
                cn.Open();
                cmd.Connection = cn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;
                foreach (var parm in parameter)
                {
                    cmd.Parameters.Add(new SqlParameter(parm.Key, parm.Value));
                }

                object data = cmd.ExecuteScalar();
                if (data != null) return (string)data;
                return null;
            }
            finally
            {
                cn.Close();
            }

        }
      
        public static void InsertHOPendingTask(string procName, Dictionary<string, string> parameter)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(app.Connection.ConnectionString);

            SqlCommand cmd = new SqlCommand();
            SqlConnection cn = new SqlConnection(entityBuilder.ProviderConnectionString);

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            try
            {
                cn.Open();
                cmd.Connection = cn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;
                foreach (var parm in parameter)
                {
                    cmd.Parameters.Add(new SqlParameter(parm.Key, parm.Value));
                }

                object data = cmd.ExecuteScalar();

            }
            finally
            {
                cn.Close();
            }
        }
  
        public static  void SendMailProc(string lstBranchID, Dictionary<string, string> ParameterGetMailSend, Dictionary<string, string> ParameterProcGetMailContentSend, SI_ApprovalFlowHandle handle)
        {

            foreach (string strBranchID in lstBranchID.Split(','))
            {
                try
                {
                     var branchID = strBranchID.ToUpper().Trim();
                    var lstMail = GetMail(handle.ProcName.PassNull().Trim() == "" ? "GetMailSend" : handle.ProcName, ParameterGetMailSend);
                    string Content = Approve_Content(handle.ProcContent.PassNull().Trim() == "" ? "GetMailContentSend" : handle.ProcContent, ParameterProcGetMailContentSend);
                    string to = "";
                    string cc = "";
                    string subject = handle.MailSubject;
                    var objMail = lstMail.Where(p => p.BranchID.ToUpper().Trim().Split(',').Contains(branchID)).FirstOrDefault();
                    if (objMail != null)
                    {
                        to = objMail.To.PassNull();
                        cc = objMail.CC.PassNull();
                        subject = !string.IsNullOrWhiteSpace(objMail.Subject) ? objMail.Subject.PassNull() : subject;
                    }
                    SendMail(to, cc, subject, Content);
                }
                catch
                {
                    // return "NO OK";
                }
            }
            // return "OK";
        }

        public static  void SendMailApprove(string lstBranchID, string lstObj, string ScreenNbr, string CurrentBranch, string Status, string Handle, string[] _roles, string User, short LangID)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            var objhandle = new SI_ApprovalFlowHandle();
            var lstobjhandle = app.SI_ApprovalFlowHandle.Where(p => p.AppFolID.ToUpper().Trim() == ScreenNbr.ToUpper().Trim() && p.Status.ToUpper().Trim() == Status.ToUpper().Trim() && p.Handle.ToUpper().Trim() == Handle.ToUpper().Trim()).ToList();
            objhandle = lstobjhandle.Where(p => _roles.Any(d => d.ToUpper().Trim() == p.RoleID.ToUpper().ToUpper().Trim())).FirstOrDefault();
            objhandle = objhandle == null ? new SI_ApprovalFlowHandle() : objhandle;
            try
            {

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("@CurrentBranchID", CurrentBranch);
                dic.Add("@ScreenNbr", objhandle.AppFolID.PassNull());
                dic.Add("@ObjID", lstObj);
                dic.Add("@BranchID", lstBranchID);
                dic.Add("@FromStatus", objhandle.Status.PassNull());
                dic.Add("@ToStatus", objhandle.ToStatus.PassNull());
                dic.Add("@Action", "2");
                dic.Add("@Handle", objhandle.Handle.PassNull());
                dic.Add("@RoleID", objhandle.RoleID.PassNull());
                dic.Add("@LangID", LangID.ToString());
                dic.Add("@User", User);

                if (objhandle.Param00.PassNull().ToUpper().Split(',').Contains("PUSHTASK"))
                    InsertHOPendingTask("InsertHOPendingTask", dic);
                var lstMail = GetMail(objhandle.MailApprove.PassNull().Trim() == "" ? "MailSend" : objhandle.MailApprove, dic);
                string subject = objhandle.MailSubject;
                foreach (var item in lstMail)
                {
                    subject = item.Subject.PassNull() != string.Empty ? item.Subject.PassNull() : objhandle.MailSubject;
                    SendMail(item.To.PassNull(), item.CC.PassNull(), subject, item.Content);
                }
            }
            catch
            {
                throw new Exception();

            }

        }

        public static void SendMailApprove(string lstBranchID, string lstObj, string ScreenNbr, string CurrentBranch, string Status, string ToStatus, string User, short LangID)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
            var objhandle = new SI_ApprovalFlowHandle();
            var _roles = _sys.Users.Where(p => p.UserName.ToUpper() == User.ToUpper()).FirstOrDefault().UserTypes.PassNull().Split(',');
            var lstobjhandle = app.SI_ApprovalFlowHandle.Where(p => p.AppFolID.ToUpper().Trim() == ScreenNbr.ToUpper().Trim() && p.Status.ToUpper().Trim() == Status.ToUpper().Trim() && p.ToStatus.ToUpper().Trim() == ToStatus.ToUpper().Trim()).ToList();          
            objhandle = lstobjhandle.Where(p => _roles.Any(d => d.ToUpper().Trim() == p.RoleID.ToUpper().ToUpper().Trim())).FirstOrDefault();
            objhandle = objhandle == null ? new SI_ApprovalFlowHandle() : objhandle;
            try
            {

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("@CurrentBranchID", CurrentBranch);
                dic.Add("@ScreenNbr", objhandle.AppFolID.PassNull());
                dic.Add("@ObjID", lstObj);
                dic.Add("@BranchID", lstBranchID);
                dic.Add("@FromStatus", objhandle.Status.PassNull());
                dic.Add("@ToStatus", objhandle.ToStatus.PassNull());
                dic.Add("@Action", "2");
                dic.Add("@Handle", objhandle.Handle.PassNull());
                dic.Add("@RoleID", objhandle.RoleID.PassNull());
                dic.Add("@LangID", LangID.ToString());
                dic.Add("@User", User);

                if (objhandle.Param00.PassNull().ToUpper().Split(',').Contains("PUSHTASK"))
                    InsertHOPendingTask("InsertHOPendingTask", dic);
                var lstMail = GetMail(objhandle.MailApprove.PassNull().Trim() == "" ? "MailSend" : objhandle.MailApprove, dic);
                string subject = objhandle.MailSubject;
                foreach (var item in lstMail)
                {
                    subject = item.Subject.PassNull() != string.Empty ? item.Subject.PassNull() : objhandle.MailSubject;
                    SendMail(item.To.PassNull(), item.CC.PassNull(), subject, item.Content);
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        public static void SendMailFile(string mailTo, string mailCC, string subject, string content, string EmailID, string[] FileAttach,string emailID="")
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            try
            {
                if (mailTo == string.Empty && mailCC == string.Empty) return;
                var email = app.HO_EmailConfig.Where(p => p.EmailID.ToUpper() == (emailID == "" ? "Approve".ToUpper() : emailID.PassNull().ToUpper())).FirstOrDefault();
                if (email != null)
                    SendMailEndFile(email.SMTPServer, email.Port, email.SSL, email.UserName, email.Pass, email.MailBox, email.Name, mailTo, mailCC, subject, content, FileAttach);
                else
                    throw new Exception("No email config");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SendMailFileProc(string procName, Dictionary<string, string> parameter, string subject, string EmailID, string[] FileAttach,string emailID="")
        {

            try
            {
                List<GetMailResult> lst = GetMail(procName, parameter);
                HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
                var email = app.HO_EmailConfig.Where(p => p.EmailID.ToUpper() == (emailID == "" ? "Approve".ToUpper() : emailID.PassNull().ToUpper())).FirstOrDefault();
                if (email != null)
                    foreach (var obj in lst)
                    {
                        SendMailEndFile(email.SMTPServer, email.Port, email.SSL, email.UserName, email.Pass, email.MailBox, email.Name, obj.To, obj.CC, subject, obj.Content, FileAttach);
                    }
                else
                    throw new Exception("No email config");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SendMailEndFile(string SMTPServer, int Port, bool SSL, string UserName, string Pass, string MailBox, string fromName, string toMail, string ccMail, string subject, string content, string[] FileAttach)
        {
            try
            {


                if (toMail == string.Empty && ccMail == string.Empty) return;

                string regexEmail = @"\b[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}\b";
                using (SmtpClient smtpClient = new SmtpClient(SMTPServer, Port))
                {


                    Regex regex = new Regex(regexEmail);
                    if (!regex.IsMatch(MailBox) || !regex.IsMatch(toMail)) throw new Exception("Wrong email address");

                    NetworkCredential auth = new NetworkCredential(UserName, Encryption.Decrypt(Pass, "1210Hq10s081f359t"));
                    smtpClient.Credentials = auth;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    if (SSL) smtpClient.EnableSsl = true;

                    MailAddress from = new MailAddress(MailBox, fromName);
                    using (MailMessage mail = new MailMessage())
                    {

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
                        if (FileAttach != null)
                        {
                            for (var i = 0; i < FileAttach.Length; i++)
                            {
                                mail.Attachments.Add(new Attachment(FileAttach[i]));
                            }
                        }
                        smtpClient.Send(mail);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
        public static void SendMailApproveAR21600(string ScreenNbr, string User, string ObjectValue, string Task, string FromBranch, string ToBranch, string FromStatus, string ToStatus, string lstObj, string Reason, string Handle, string[] _roles, short LangID)
        {
            try
            {
                HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
                var objhandle = new SI_ApprovalFlowHandle();
                if (Task.ToUpper().Trim() == "CB")
                {
                    var lst = app.SI_ApprovalFlowHandle.Where(p => p.AppFolID.ToUpper().Trim() == ObjectValue.Trim().ToUpper() && _roles.Any(d => d.ToUpper().ToUpper().Trim() == p.RoleID.ToUpper().ToUpper().Trim())).ToList();
                    foreach (var obj in lst)
                    {
                        if (obj.Param01.PassNull().ToUpper().Split(',').Contains("TRANSFER"))
                        {
                            objhandle = obj;
                            break;
                        }
                    }
                }
                else objhandle = app.SI_ApprovalFlowHandle.Where(p => p.AppFolID.ToUpper().Trim() == ObjectValue.Trim().ToUpper() && p.Status.ToUpper().Trim() == FromStatus.ToUpper().Trim() && p.ToStatus.ToUpper().Trim() == ToStatus.ToUpper().Trim() && _roles.Any(d => d.ToUpper().ToUpper().Trim() == p.RoleID.ToUpper().ToUpper().Trim())).FirstOrDefault();

                //if (objhandle != null)
                //{
                objhandle = objhandle == null ? new SI_ApprovalFlowHandle() : objhandle;
                try
                {

                    Dictionary<string, string> dic = new Dictionary<string, string>();

                    dic.Add("@Prog", ScreenNbr);
                    dic.Add("@User", User);
                    dic.Add("@Object", ObjectValue);
                    dic.Add("@Task", Task);
                    dic.Add("@FromBranch", FromBranch);
                    dic.Add("@ToBranch", ToBranch);
                    dic.Add("@FromStatus", FromStatus);
                    dic.Add("@ToStatus", ToStatus);
                    dic.Add("@List", lstObj);
                    dic.Add("@Reason", Reason);

                    if (objhandle.Param00.PassNull().ToUpper().Split(',').Contains("PUSHTASK"))
                        InsertHOPendingTask("InsertHOPendingTask", dic);
                    var lstMail = GetMail(objhandle.MailApprove.PassNull().Trim() == "" ? "MailSend" : objhandle.MailApprove, dic);
                    string subject = objhandle.MailSubject;
                    foreach (var item in lstMail)
                    {
                        subject = item.Subject.PassNull() != string.Empty ? item.Subject.PassNull() : objhandle.MailSubject;
                        SendMail(item.To.PassNull(), item.CC.PassNull(), subject, item.Content);
                    }
                }
                catch
                {
                    throw new Exception();


                }
            }
            catch
            {
                throw new Exception();

            }


        }
        #endregion
        #region TrucVT

        
        public static void Mail_Approve(string screenNbr, string objID, string role, string status, string handle, string langID, string userName, string lstBranch, string currentBranch, string parm00, string parm01, string parm02)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>(false);
            var approvehandle = app.SI_ApprovalFlowHandle.Where(p => p.AppFolID == screenNbr && p.RoleID == role && p.Status == status && p.Handle == handle).FirstOrDefault();
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

                var mail = GetMail(approvehandle.MailApprove.PassNull() == string.Empty ? "MailSend" : approvehandle.MailApprove, dic);
                string subject = approvehandle.MailSubject.PassNull();
                foreach (var item in mail)
                {
                    string content = string.Format("<html><body><p>{0}</p></body></html>", item.Content.PassNull());
                    subject = item.Subject.PassNull() != string.Empty ? item.Subject.PassNull() : approvehandle.MailSubject;
                    SendMail(item.To.PassNull(), item.CC.PassNull(), subject, content);
                }
            }
        }

        #endregion
    }
}



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

namespace HQSendMailApprove
{

    public class GetMailResult
    {
  
        public int ID { get; set; }
  
        public string To { get; set; }
    
        public string CC { get; set; }
    
        public string BranchID { get; set; }
  
        public string Content { get; set; }
       
    }
    public static class Approve
    {

        #region Trunght
      
        public static void SendMail(string mailTo, string mailCC, string subject, string content,string[] fullPathAttach =null)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
            try
            {
                if (mailTo == string.Empty && mailCC == string.Empty) return;
                var email = app.HO_EmailConfig.Where(p => p.EmailID.ToUpper() == "Approve".ToUpper()).FirstOrDefault();
                if (email != null)
                    SendMailEnd(email.SMTPServer, email.Port, email.SSL, email.UserName, email.Pass, email.MailBox, email.Name, mailTo, mailCC, subject, content, fullPathAttach);
                else
                    throw new Exception("No email config");
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
        public static List<GetMailResult> GetMail(string procName, Dictionary<string, string> parameter)
        {

            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(app.Connection.ConnectionString);


            SqlCommand cmd = new SqlCommand();
            SqlConnection cn = new SqlConnection(entityBuilder.ProviderConnectionString);
            SqlDataAdapter adap = new SqlDataAdapter();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            try
            {
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
                List<GetMailResult> result = new List<GetMailResult>();

                foreach (DataRow item in dt.Rows)
                {
                    string to = item["To"] is DBNull ? string.Empty : (string)item["To"];
                    string cc = item["CC"] is DBNull ? string.Empty : (string)item["CC"];
                    string Content = item["Content"] is DBNull ? string.Empty : (string)item["Content"];
                    if (to == string.Empty) continue;
                    result.Add(new GetMailResult() { ID = (int)item["ID"], To = to, CC = cc, BranchID = (string)item["BranchID"], Content = Content });
                }
                return result;

            }
            finally
            {
                cn.Close();
            }

        }
     
        public static string Approve_Content(string procName, Dictionary<string, string> parameter)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
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
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
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

                    var lstMail = GetMail(handle.ProcName.PassNull().Trim() == "" ? "GetMailSend" : handle.ProcName, ParameterGetMailSend);
                    string Content = Approve_Content(handle.ProcContent.PassNull().Trim() == "" ? "GetMailContentSend" : handle.ProcContent, ParameterProcGetMailContentSend);
                    string to = "";
                    string cc = "";
                    if (lstMail.Where(p => p.BranchID.ToUpper().Trim().Split(',').Contains(strBranchID.ToUpper().Trim())).FirstOrDefault() != null)
                        to = lstMail.Where(p => p.BranchID.ToUpper().Trim().Split(',').Contains(strBranchID.ToUpper().Trim())).FirstOrDefault().To.PassNull();
                    if (lstMail.Where(p => p.BranchID.ToUpper().Trim().Split(',').Contains(strBranchID.ToUpper().Trim())).FirstOrDefault() != null)
                        cc = lstMail.Where(p => p.BranchID.ToUpper().Trim().Split(',').Contains(strBranchID.ToUpper().Trim())).FirstOrDefault().CC.PassNull();
                    SendMail(to, cc, handle.MailSubject, Content);
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
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
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
                foreach (var item in lstMail)
                {
                    SendMail(item.To.PassNull(), item.CC.PassNull(), objhandle.MailSubject, item.Content);
                }
            }
            catch
            {
                throw new Exception();

            }

        }

        public static void SendMailApprove(string lstBranchID, string lstObj, string ScreenNbr, string CurrentBranch, string Status, string ToStatus, string User, short LangID)
        {
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
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
                foreach (var item in lstMail)
                {
                    SendMail(item.To.PassNull(), item.CC.PassNull(), objhandle.MailSubject, item.Content);
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
                HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
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
                    foreach (var item in lstMail)
                    {
                        SendMail(item.To.PassNull(), item.CC.PassNull(), objhandle.MailSubject, item.Content);
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
            HQSendMailApproveEntities app = Util.CreateObjectContext<HQSendMailApproveEntities>();
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
                foreach (var item in mail)
                {
                    string content = string.Format("<html><body><p>{0}</p></body></html>", item.Content.PassNull());
                    SendMail(item.To.PassNull(), item.CC.PassNull(), approvehandle.MailSubject.PassNull(), content);
                }
            }
        }

        #endregion
    }
}



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
using HQSendMailApprove;
using OM41200.Models;
using System.Data.SqlClient;
namespace OM41200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM41200Controller : Controller
    {
        private string _screenNbr = "OM41200";
        OM41200Entities _db = Util.CreateObjectContext<OM41200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM41200");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\OM41200");
                }
                return _filePath;
            }
        }
        //
        // GET: /OM41200/
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

        public ActionResult GetDet(string zone, string territory, string cpnyID,
            string displayID, DateTime? fromDate, DateTime? todate, string status)
        {
            var dets = _db.OM41200_pgLoadGrid(Current.UserName, Current.CpnyID, zone, 
                territory, cpnyID, status, fromDate, todate, displayID).ToList();
            return this.Store(dets);
        }

        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var handle = data["cboHandle"];
                var imgHandler = new StoreDataHandler(data["lstDet"]);
                var lstDet = imgHandler.ObjectData<OM41200_pgLoadGrid_Result>().ToList();

                var user = _sys.Users.FirstOrDefault(p=>p.UserName.ToUpper() == Current.UserName.ToUpper());
                int count = lstDet.Where(p => p.Selected == true).ToList().Count();
                foreach (var pending in lstDet.Where(p => p.Selected == true))
                {
                    var step = _db.SI_ApprovalFlowHandle.FirstOrDefault(p => p.Status == pending.Status
                        && p.AppFolID == _screenNbr
                        && p.Handle == handle);
                    if (step != null)
                    {
                        count--;
                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        dic.Add("@BranchID", pending.BranchID);
                        dic.Add("@SlsPerID", pending.SlsperID);
                        dic.Add("@CustID", pending.CustID);
                        dic.Add("@DisplayID", pending.DisplayID);
                        dic.Add("@LevelID", pending.LevelID);
                        dic.Add("@ScreenNbr", _screenNbr);
                        //dic.Add("@Content", "");
                        dic.Add("@Status", pending.Status);
                        dic.Add("@Action", (handle == "A" || handle == "I") ? "0" : "1");
                        dic.Add("@User", Current.UserName);
                        dic.Add("@Roles", user.UserTypes);
                        dic.Add("@LangID", Current.LangID.ToString());
                        dic.Add("@Handle", handle);

                        string to = GetMail(pending.BranchID, step.MailTo);
                        string cc = GetMail(pending.BranchID, step.MailCC);
                        try
                        {
                            //stored procedure OM41200_Approve
                            SubmitApprove(step.ProcName, dic, to, cc, step.MailSubject, "");
                        }
                        catch{
                        
                        }
                    }
                }

                return Json(new { success = true });
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

        private string GetMail(string listBranch, string listMail)
        {
            listMail = listMail == null ? string.Empty : listMail;
            listBranch = listBranch == null ? string.Empty : listBranch;
            string to = string.Empty;
            string[] branchs = listBranch.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] roles = listMail.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in roles)
            {
                if (role.ToUpper() == "DIST" || role.ToUpper() == "SUBDIST")
                {
                    foreach (var branch in branchs)
                    {
                        var company = (from p in _sys.SYS_Company where p.CpnyID == branch select p).FirstOrDefault();
                        if (company != null)
                            to += ';' + company.Email;
                    }
                }
                else if (role.ToUpper() == "SUP" || role.ToUpper() == "ADMIN")
                {
                    foreach (var branch in branchs)
                    {
                        var user = (from p in _sys.Users
                                    where p.UserTypes != null && p.UserTypes.Split(',').Any(c => c.ToUpper() == role.ToUpper())
                                        && p.CpnyID != null && p.CpnyID.Split(',').Any(c => c.ToUpper() == branch.ToUpper())
                                    select p).FirstOrDefault();
                        if (user != null)
                            to += ';' + user.Email;
                    }
                }
                else
                {
                    var users = (from p in _sys.Users where p.UserTypes != null && p.UserTypes.Split(',').Any(c => c.ToUpper() == role.ToUpper()) select p).ToList();
                    foreach (var user in users)
                    {
                        to += ';' + user.Email;
                    }
                }
            }
            return to;
        }

        public void SubmitApprove(string proc, Dictionary<string, string> parameter, string to, string cc, string subject, string note)
        {
            var lstParams = new List<SqlParameter>();

            string[] except = new string[] { "@Content", "@LangID", "@Roles", "@UserName", "@FromStatus" };
            foreach (var parm in parameter)
            {
                if (!except.Any(p => p.ToUpper() == parm.Key.ToUpper()))
                {
                    lstParams.Add(new SqlParameter(parm.Key, parm.Value));
                }
            }

            // stored procedure OM41200_Approve
            var toStatus = _db.ExecuteStoreQuery<string>(proc, lstParams).FirstOrDefault();

            var contentProc = _db.OM41200_ApproveContent(parameter["@ScreenNbr"],
                parameter["@CustID"], parameter["@SlsPerID"],
                parameter["@BranchID"], parameter["@DisplayID"],
                parameter["@LevelID"], parameter["@Status"],
                toStatus, parameter["@Action"].ToShort(),
                parameter["@Handle"], parameter["@Roles"],
                parameter["@LangID"].ToShort(), parameter["@User"]);
            string content = string.Format("<html><body><p>{0}</p><p>{1}</p></body></html>", contentProc, note);
            SendMailApprove(to, cc, subject, content);
        }

        public void SendMailApprove(string mailTo, string mailCC, string subject, string content)
        {
            try
            {
                if (mailTo == string.Empty && mailCC == string.Empty) return;

                var email = _db.HO_EmailConfig.Where(p => p.EmailID.ToUpper() == "Approve".ToUpper()).FirstOrDefault();
                if (email != null)
                    Approve.SendMail(mailTo, mailCC, subject, content);
                else
                    throw new Exception("No email config");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

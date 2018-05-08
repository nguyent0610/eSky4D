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
using HQSendMailApprove;

namespace SA02600.Controllers
{
    [DirectController]
    public class SA02600Controller : Controller
    {
        private string _screenNbr = "SA02600";
        private string _userName = Current.UserName;
        SA02600Entities _db = Util.CreateObjectContext<SA02600Entities>(true);

        public ActionResult Index()
        {                       
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }


        [HttpPost]
        public ActionResult Restore(string username)
        {
            var value = _db.Users.Where(p => p.UserName.ToUpper() == username.ToUpper()).FirstOrDefault();
            if (value != null)
            {
                var cpny = _db.SYS_Company.FirstOrDefault();
                Current.DBApp = cpny.DatabaseName;
                try
                {

                    Approve.SendMail(value.Email.PassNull(), "", "Khôi phục mật khẩu ", "Mật khẩu: " + Encryption.Decrypt(value.Password, "1210Hq10s081f359t"));

                    return Json(new
                    {
                        success = true,
                    });
                }
                catch (Exception ex)
                {
                    // Encode the string input
                    StringBuilder sb = new StringBuilder(
                        HttpUtility.HtmlEncode(ex.Message));
                    // Selectively allow <b> and <i>
                    sb.Replace("&lt;b&gt;", "<b>");
                    sb.Replace("&lt;/b&gt;", "</b>");
                    sb.Replace("&lt;i&gt;", "<i>");
                    sb.Replace("&lt;/i&gt;", "</i>");
                    return Json(new { success = false, code = "999912311", error = sb.ToString() });

                }
            }
            else
            {
                return Json(new { success = false, code = "1507", error = "" });
            }

        }
        [HttpPost]
        public ActionResult ResetPassword(string username)
        {
            var value = _db.Users.Where(p => p.UserName.ToUpper() == username.ToUpper()).FirstOrDefault();
            if (value != null)
            {
                var cpny = _db.SYS_Company.FirstOrDefault();
                Current.DBApp = cpny.DatabaseName;
                try
                {
                    var rand = new Random();
                    var objConfig = _db.SA02600_pdEmailConfig(username, Current.CpnyID, Current.LangID).FirstOrDefault();
                    if (objConfig != null)
                    {
                        string title = objConfig.Title;
                        string emailTo = value.Email.PassNull();
                        string activeCode = rand.Next(100000, 999999).ToString();
                        string param = "/SA03700?ActiveCode=" + Encryption.Encrypt(activeCode + "&" + DateTime.Now.ToString("yyyyMMdd HHmmss") + "&" + username, "1210Hq10s081f359t");
                        string link = !string.IsNullOrWhiteSpace(objConfig.Link) ? objConfig.Link + param :
                                       "http://" + System.Web.HttpContext.Current.Request.Url.Host +":54549" + param;
                        var content = objConfig.Conent.Replace("@UserName", username).Replace("@ActiveCode", activeCode) + link + "$";
                        Approve.SendMail(emailTo, "", title, content);
                    }
                    return Json(new
                    {
                        success = true,
                    });
                }
                catch (Exception ex)
                {
                    // Encode the string input
                    StringBuilder sb = new StringBuilder(
                        HttpUtility.HtmlEncode(ex.Message));
                    // Selectively allow <b> and <i>
                    sb.Replace("&lt;b&gt;", "<b>");
                    sb.Replace("&lt;/b&gt;", "</b>");
                    sb.Replace("&lt;i&gt;", "<i>");
                    sb.Replace("&lt;/i&gt;", "</i>");
                    return Json(new { success = false, code = "999912311", error = sb.ToString() });

                }
            }
            else
            {
                return Json(new { success = false, code = "1507", error = "" });
            }

        }

    }
}

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
namespace SA02500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA02500Controller : Controller
    {
        private string _screenNbr = "SA02500";
        private string _userName = Current.UserName;
        SA02500Entities _db = Util.CreateObjectContext<SA02500Entities>(true);

        public ActionResult Index()
        {
            ViewBag.TextVal = _db.SYS_Configurations.FirstOrDefault(p => p.Code == "SA02500Check").TextVal;
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetPass()
        {
            var username = Current.UserName.ToString();
            var objHeader = _db.SYS_PassHistory.FirstOrDefault(p => p.UserName == username);
            return this.Store(objHeader);
        }

        [HttpPost]
        public ActionResult SA02500Save(FormCollection data)
        {
            string oldPassword = data["txtOldPassword"];
            string newPassword = data["txtNewPassword"];
            string reNewPassword = data["txtReNewPassword"];
            string username = Current.UserName.ToString();

            // Kiem tra pass trung 5 lan gan nhat
            var flag = _db.SA02500_ppCheckPass(username, Encryption.Encrypt(reNewPassword, "1210Hq10s081f359t")).FirstOrDefault();

            if(flag.ToString()=="0")
            {
                if (newPassword == reNewPassword)
                {
                    var objUser = _db.Users.Where(p => p.UserName == Current.UserName).FirstOrDefault();
                    var objHeader = _db.SYS_PassHistory.FirstOrDefault();
                    objUser.Password = Encryption.Decrypt(objUser.Password, "1210Hq10s081f359t");
                    if (oldPassword != objUser.Password)
                    {
                        return Json(new { success = false, code = "20140408" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (reNewPassword == objUser.Password)
                        {
                            return Json(new { success = false, code = "201303073" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            try
                            {
                                objUser.Password = Encryption.Encrypt(reNewPassword, "1210Hq10s081f359t");
                                objHeader = new SYS_PassHistory();
                                objHeader.ResetET();
                                objHeader.UserName = Current.UserName.ToString();
                                objHeader.Password = Encryption.Encrypt(reNewPassword, "1210Hq10s081f359t");
                                objHeader.Crtd_Datetime = DateTime.Now;
                                objHeader.Crtd_Prog = _screenNbr;
                                objHeader.Crtd_User = Current.UserName;
                                objHeader.LUpd_Datetime = DateTime.Now;
                                objHeader.LUpd_Prog = _screenNbr;
                                objHeader.LUpd_User = Current.UserName;

                            }
                            catch
                            {
                                objUser.Password = string.Empty;
                            }
                        }
                        _db.SYS_PassHistory.AddObject(objHeader);
                    }
                    objUser.LoggedIn = true;
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, code = "1503" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, code = "201303072" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SA02500Render(FormCollection data)
        {
            string totalpassRule = Util.GetLang("PassRule1") + "\n" + Util.GetLang("PassRule2") + "\n" + Util.GetLang("PassRule3") + "\n" + Util.GetLang("PassRule4");

            return Json(new { success = true, totalpassRule }, JsonRequestBehavior.AllowGet);
        }
        

    }
}

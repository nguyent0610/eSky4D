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
    //[CustomAuthorize]
    //[CheckSessionOut]
    public class SA02500Controller : Controller
    {
        private string _screenNbr = "SA02500";
        private string _userName = Current.UserName;
        SA02500Entities _db = Util.CreateObjectContext<SA02500Entities>(true);

        public ActionResult Index()
        {
            var objSA02500Check = _db.SYS_Configurations.FirstOrDefault(p => p.Code == "SA02500Check");
            var objSA02500CheckAdmin = _db.SYS_Configurations.FirstOrDefault(p => p.Code == "SA02500CheckAdmin");
            var objUserGroup = _db.SYS_UserGroup.FirstOrDefault(p => p.UserID == Current.UserName && p.GroupID=="Admin");
            ViewBag.TextVal = objSA02500Check == null ? "0" : objSA02500Check.TextVal;
            ViewBag.TextValAdmin = objUserGroup == null ? "0" : (objSA02500CheckAdmin == null ? "0" : objSA02500CheckAdmin.TextVal);
            ViewBag.GroupAdmin = objUserGroup == null ? "0" : "1";
           
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
        public ActionResult SA02500Save(FormCollection data, bool? checkRule)
        {
            try
            {
                string oldPassword = data["txtOldPassword"];
                string newPassword = data["txtNewPassword"];
                string reNewPassword = data["txtReNewPassword"];
                string username = Current.UserName.ToString();

                // Kiem tra pass trung 5 lan gan nhat
                string flag = _db.SA02500_ppCheckPass(username, Encryption.Encrypt(reNewPassword, "1210Hq10s081f359t"), checkRule == null? true: checkRule.Value).FirstOrDefault();

                if (flag.ToString() == "0")
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
                                    objUser.BeginDay = DateTime.Now;
                                    objUser.LUpd_Datetime = DateTime.Now;
                                    objUser.LUpd_Prog = _screenNbr;
                                    objUser.LUpd_User = Current.UserName;

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
                                    objHeader.tstamp = new byte[1];
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
            catch (Exception ex)
            {
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult SA02500Render(FormCollection data)
        {
            string totalpassRule = Util.GetLang("PassRule1") + "\n" + Util.GetLang("PassRule2") + "\n" + Util.GetLang("PassRule3") + "\n" + Util.GetLang("PassRule4");
            var result = _db.SA02500_pdConfig().FirstOrDefault();
            bool isShowLogin = result.isShowLogin == null ? false: result.isShowLogin.Value;
            bool isShowRule = result.isShowRule == null ? false : result.isShowRule.Value;
            return Json(new { success = true, totalpassRule, isShowLogin, isShowRule }, JsonRequestBehavior.AllowGet);
        }
        

    }
}

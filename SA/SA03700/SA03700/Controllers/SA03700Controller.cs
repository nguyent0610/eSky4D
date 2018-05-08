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
namespace SA03700.Controllers
{
    [DirectController]
    //[CustomAuthorize]
    //[CheckSessionOut]
    public class SA03700Controller : Controller
    {
        private string _screenNbr = "SA03700";
        private string _userName = Current.UserName;
        SA03700Entities _db = Util.CreateObjectContext<SA03700Entities>(true);

        public ActionResult Index(string ActiveCode)
        {
            if (ActiveCode != null)
            {
                try
                {
                    string data = Encryption.Decrypt(ActiveCode.TrimEnd('$').Replace(" ", "+"), "1210Hq10s081f359t");
                    var lstParam = data.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lstParam.Length == 3)
                    {
                        ViewBag.activeCode = lstParam[0];
                        ViewBag.userName = lstParam[2];
                        Current.UserName = lstParam[2];
                        DateTime myDate = DateTime.ParseExact(lstParam[1], "yyyyMMdd HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                        TimeSpan span = (DateTime.Now - myDate);
                        var expiredTime = span.Minutes + span.Days * 24 * 60 + span.Hours * 60;
                        ViewBag.isExpired =  expiredTime > 30;
                    }
                }
                catch
                {
                    // báo lỗi link không hợp lệ
                }
            }

            var objSA03700Check = _db.SYS_Configurations.FirstOrDefault(p => p.Code == "SA02500Check");
            var objSA03700CheckAdmin = _db.SYS_Configurations.FirstOrDefault(p => p.Code == "SA02500CheckAdmin");
            var objUserGroup = _db.SYS_UserGroup.FirstOrDefault(p => p.UserID == Current.UserName && p.GroupID=="Admin");
            ViewBag.TextVal = objSA03700Check == null ? "0" : objSA03700Check.TextVal;
            ViewBag.TextValAdmin = objUserGroup == null ? "0" : (objSA03700CheckAdmin == null ? "0" : objSA03700CheckAdmin.TextVal);
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
        public ActionResult SA03700Save(FormCollection data, string activeCode)
        {
            try
            {
                string oldPassword = data["txtActiveCode"];
                string newPassword = data["txtNewPassword"];
                string reNewPassword = data["txtReNewPassword"];
                string username = Current.UserName.ToString();
                               
                // Kiem tra pass trung 5 lan gan nhat
                var flag = _db.SA03700_ppCheckPass(username, Encryption.Encrypt(reNewPassword, "1210Hq10s081f359t")).FirstOrDefault();

                if (flag.ToString() == "0")
                {
                    if (newPassword == reNewPassword)
                    {
                        var objUser = _db.Users.Where(p => p.UserName == Current.UserName).FirstOrDefault();
                        var objHeader = _db.SYS_PassHistory.FirstOrDefault();
                        objUser.Password = Encryption.Decrypt(objUser.Password, "1210Hq10s081f359t");
                        if (activeCode != oldPassword)
                        {
                            return Json(new { success = false, code = "2018050801" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SA03700Render(FormCollection data)
        {
            string totalpassRule = Util.GetLang("PassRule1") + "\n" + Util.GetLang("PassRule2") + "\n" + Util.GetLang("PassRule3") + "\n" + Util.GetLang("PassRule4");

            return Json(new { success = true, totalpassRule }, JsonRequestBehavior.AllowGet);
        }
        

    }
}

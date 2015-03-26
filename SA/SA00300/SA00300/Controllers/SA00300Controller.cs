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



namespace SA00300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00300Controller : Controller
    {
        private string _screenNbr = "SA00300";
        private string _userName = Current.UserName;
        SA00300Entities _db = Util.CreateObjectContext<SA00300Entities>(true);

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _db.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadSA00300");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("\\Images\\SA00300");
                }
                return _filePath;
            }
        }

        public ActionResult Index()
        {  
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetUser(string UserID)
        {
            var objHeader = _db.Users.FirstOrDefault(p => p.UserName == UserID);
            if (objHeader != null)
            {
                try
                {
                    objHeader.Password = objHeader.Password.PassNull() == "" ? "" : Encryption.Decrypt(objHeader.Password, "1210Hq10s081f359t");
                }
                catch
                {
                }
                try
                {
                    objHeader.PasswordAnswer = objHeader.PasswordAnswer.PassNull() == "" ? "" : Encryption.Decrypt(objHeader.PasswordAnswer, "1210Hq10s081f359t");
                }
                catch
                {
                }
                return this.Store(objHeader);
            }
            return this.Store(_db.Users.Where(p => p.UserName == UserID));
        }

        public ActionResult GetSYS_UserCompany(string UserID)
        {
            return this.Store(_db.SA00300_pgLoadSYS_UserCompany(UserID).ToList());
        }

        public ActionResult GetSYS_UserGroup(string UserID)
        {
            return this.Store(_db.SA00300_pgLoadSYS_UserGroup(UserID).ToList());
        }
        
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string UserID = data["cboUserID"];
                if (data["isAuto"].PassNull().ToUpper() == "true".ToUpper())
                {
                    bool b = true;
                    string strID = "";
                    while (b)
                    {
                        strID = (DateTime.Now.ToString("yyyyMMddhhmmssff") + data["FirstName"]).GetHashCode().ToString().ToHex() + "000000000";
                        strID = strID.Substring(0, 6);
                        UserID = strID;
                        var obj = (from p in _db.Users select p).Where(p => p.UserName.ToUpper().Trim() == UserID.ToUpper().Trim()).FirstOrDefault();
                        if (obj == null) b = false;
                    }
                }
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstUser"]);
                ChangeRecords<User> lstUser = dataHandler.BatchObjectData<User>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSYS_UserCompany"]);
                ChangeRecords<SA00300_pgLoadSYS_UserCompany_Result> lstSYS_UserCompany = dataHandler1.BatchObjectData<SA00300_pgLoadSYS_UserCompany_Result>();

                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstSYS_UserGroup"]);
                ChangeRecords<SA00300_pgLoadSYS_UserGroup_Result> lstSYS_UserGroup = dataHandler2.BatchObjectData<SA00300_pgLoadSYS_UserGroup_Result>();
               
                #region Save Header Users
                lstUser.Created.AddRange(lstUser.Updated);
                foreach (User curHeader in lstUser.Created)
                {
                   if (UserID.PassNull() == "") continue;
                   var header = _db.Users.FirstOrDefault(p => p.UserName == UserID);
                   
                   var files = Request.Files;
                   if (files.Count > 0 && files[0].ContentLength > 0) // Co chon file de upload
                   {
                       // Xoa file cu di
                       var oldPath = string.Format("{0}\\{1}", FilePath, curHeader.Images);
                       if (System.IO.File.Exists(oldPath))
                       {
                           System.IO.File.Delete(oldPath);
                       }

                       // Upload file moi
                       string newFileName = string.Format("{0}_{1}{2}", UserID,curHeader.CpnyID, Path.GetExtension(files[0].FileName));
                       files[0].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                       header.Images = newFileName;
                   }
                   else
                   {
                       if (!string.IsNullOrWhiteSpace(curHeader.Images) && string.IsNullOrWhiteSpace(header.Images))
                       {
                           // Xoa file cu di
                           var oldPath = string.Format("{0}\\{1}", FilePath, curHeader.Images);
                           if (System.IO.File.Exists(oldPath))
                           {
                               System.IO.File.Delete(oldPath);
                           }
                           header.Images = string.Empty;
                       }
                   }

                    if (header != null)
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            UpdatingHeader(header, curHeader, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        header = new User();
                        header.UserName = UserID;
                        UpdatingHeader(header, curHeader, true);
                        _db.Users.AddObject(header);
                    }
                }
                #endregion

               

                #region Save SYS_UserGroup
                foreach (SA00300_pgLoadSYS_UserGroup_Result deleted in lstSYS_UserGroup.Deleted)
                {
                    var del = _db.SYS_UserGroup.Where(p => p.UserID == UserID && p.GroupID == deleted.GroupID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_UserGroup.DeleteObject(del);
                    }
                }

                lstSYS_UserGroup.Created.AddRange(lstSYS_UserGroup.Updated);

                foreach (SA00300_pgLoadSYS_UserGroup_Result curLang in lstSYS_UserGroup.Created)
                {
                    if (curLang.GroupID.PassNull() == "") continue;

                    var lang = _db.SYS_UserGroup.FirstOrDefault(p => p.UserID.ToLower() == UserID.ToLower() && p.GroupID.ToLower() == curLang.GroupID.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingSYS_UserGroup(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_UserGroup();
                        lang.UserID = UserID;
                        UpdatingSYS_UserGroup(lang, curLang, true);
                        _db.SYS_UserGroup.AddObject(lang);
                    }
                }
                #endregion

                #region Save SYS_UserCompany
                foreach (SA00300_pgLoadSYS_UserCompany_Result deleted in lstSYS_UserCompany.Deleted)
                {
                    var objDelete = _db.SYS_UserCompany.Where(p => p.UserName == UserID && p.GroupID == deleted.GroupID).FirstOrDefault();
                    if (objDelete != null)
                    {
                        _db.SYS_UserCompany.DeleteObject(objDelete);
                    }
                }

                lstSYS_UserCompany.Created.AddRange(lstSYS_UserCompany.Updated);

                foreach (SA00300_pgLoadSYS_UserCompany_Result curLang in lstSYS_UserCompany.Created)
                {
                    if (curLang.GroupID.PassNull() == "") continue;

                    var lang = _db.SYS_UserCompany.FirstOrDefault(p => p.UserName.ToLower() == UserID.ToLower() && p.GroupID.ToLower() == curLang.GroupID.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingSYS_UserCompany(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_UserCompany();
                        lang.UserName = UserID;
                        UpdatingSYS_UserCompany(lang, curLang, true);
                        _db.SYS_UserCompany.AddObject(lang);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void UpdatingHeader(User t, User s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.CpnyID = s.CpnyID;
            t.Address = s.Address;
            t.FirstName = s.FirstName;
            t.Email = s.Email;
            t.HomeScreenNbr = s.HomeScreenNbr;
            t.LastName = s.LastName;

            try
            {
                t.Password = Encryption.Encrypt(s.Password, "1210Hq10s081f359t");
            }
            catch
            {
                t.Password = string.Empty;
            }

            t.PasswordQuestion = s.PasswordQuestion;

            try
            {
                t.PasswordAnswer = Encryption.Encrypt(s.PasswordAnswer, "1210Hq10s081f359t");
            }
            catch
            {
                t.PasswordAnswer = string.Empty;
            }
          
            t.UserTypes = s.UserTypes;
            t.Tel = s.Tel;
            t.ComputerID = s.ComputerID;
            t.Blocked = s.Blocked;
            t.BlockedTime = s.BlockedTime == null ? new DateTime(1900, 1, 1) : s.BlockedTime;
            t.LoggedIn = s.LoggedIn;
            t.LastLoggedIn = s.LastLoggedIn == null ? new DateTime(1900, 1, 1) : s.LastLoggedIn;
            t.JobTitle = s.JobTitle;
            t.Manager = s.Manager;
            t.Department = s.Department;
            t.Channel = s.Channel;
            t.AutoID = s.AutoID;
            t.ExpireDay = s.ExpireDay;
            t.FailedLoginCount = s.FailedLoginCount;
            t.BeginDay = s.BeginDay == null ? DateTime.Now : (s.BeginDay.Year == 1 ? DateTime.Now : s.BeginDay);
            t.CheckFirstLogin = s.CheckFirstLogin;
            t.CpnyIDHand = s.CpnyIDHand;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void UpdatingSYS_UserGroup(SYS_UserGroup t, SA00300_pgLoadSYS_UserGroup_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.GroupID = s.GroupID;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }

        private void UpdatingSYS_UserCompany(SYS_UserCompany t, SA00300_pgLoadSYS_UserCompany_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.GroupID = s.GroupID;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try{
            string UserID = data["cboUserID"];
            var cpny = _db.Users.FirstOrDefault(p => p.UserName == UserID);
            if (cpny != null)
            {
                var fileName = cpny.Images;
                _db.Users.DeleteObject(cpny);
                _db.SaveChanges();

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    // Xoa file cu di
                    var oldPath = string.Format("{0}\\{1}", FilePath, fileName);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
            }

            var lstAddr = _db.SYS_UserGroup.Where(p => p.UserID == UserID).ToList();
            foreach (var item in lstAddr)
            {
                _db.SYS_UserGroup.DeleteObject(item);
            }

            var lstSub = _db.SYS_UserCompany.Where(p => p.UserName == UserID).ToList();
            foreach (var item in lstSub)
            {
                _db.SYS_UserCompany.DeleteObject(item);

            }

            _db.SaveChanges();
            return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        public ActionResult ImageToBin(string fileName)
        {
            try
            {
                string filename = FilePath + "\\" + fileName;
                if (System.IO.File.Exists(filename))
                {
                    FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    BinaryReader reader = new BinaryReader(fileStream);
                    byte[] imageBytes = reader.ReadBytes((int)fileStream.Length);
                    reader.Close();

                    var imgString64 = Convert.ToBase64String(imageBytes, 0, imageBytes.Length);

                    var jsonResult = Json(new { success = true, imgSrc = @"data:image/jpg;base64," + imgString64 }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                else
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
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
    }
}

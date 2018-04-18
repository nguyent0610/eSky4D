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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;

namespace SA03001.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA03001Controller : Controller
    {
        private string _screenNbr = "SA03001";
        private string _userName = Current.UserName;
        SA03001Entities _db = Util.CreateObjectContext<SA03001Entities>(true);
        private JsonResult _logMessage;

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

        public ActionResult LoadGrid()
        {
            return this.Store(_db.SA03001_pgLoadGrid(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }

        public ActionResult LoadBranch(string listBranchID) 
        {
            return this.Store(_db.SA03001_pgBranchAllByUser(Current.CpnyID, Current.UserName, Current.LangID,listBranchID).ToList());
        }

        public ActionResult LoadForm(string userName)
        {
            var item = _db.Users.FirstOrDefault(p => p.UserName == userName);
            item.Password = item.Password == "" ? "" : Encryption.Decrypt(item.Password.ToString(), "1210Hq10s081f359t");
            return this.Store(item);
        }

        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstUser"]);
                User objTemp = new User();
                objTemp = dataHandler.ObjectData<User>().FirstOrDefault();
                string userName = data["txtUserName"].PassNull();
                string firstName = data["txtFirstName"].PassNull();
                string passWord = data["txtPassword"].PassNull();
                string email = data["txtEmail"].PassNull();
                string cpnyID = data["lstCpnyID"].PassNull();
                string userType = data["cboUserTypes"].PassNull();
                string userGroup = data["cboUserGroup"].PassNull();
                string blocked = data["valueBloked"].PassNull();
                //string tmpStartDate = data["valueStartDate"].PassNull();
                //string tmpEndDate = data["valueEndDate"].PassNull();
                string valueTstamp = data["valueTstamp"].PassNull();
                //DateTime StartDate = DateTime.Parse(tmpStartDate);
                //DateTime EndDate = DateTime.Parse(tmpEndDate);
                var a = _db.SA03001_pdCheckSaveUser(userName, cpnyID, userType, Current.CpnyID, Current.UserName, Current.LangID).ToList();
                if (a.Count>0)
                {
                    //if (a.CheckUser == true)
                    //{
                    //    string messageerorr = string.Format(Message.GetString("2018032711", null), a.UserID, a.BranchID, a.TypeUser);
                    //    throw new MessageException(MessageType.Message, "20410", "", new string[] { messageerorr });
                    //}
                    
                }
                bool isNewUser = false;

                if (data["isNewUser"].PassNull() == "true")
                {
                    isNewUser = true;
                }
                bool isBlock = false;
                if (blocked == "true")
                {
                    isBlock = true;
                }
                int failedLoginCount = Convert.ToInt32(data["txtFailedLoginCount"].PassNull() == "" ? "0" : data["txtFailedLoginCount"].PassNull());
                string manager = data["cboManager"].PassNull();

                var objUser = _db.Users.FirstOrDefault(p => p.UserName == userName);
                if (isNewUser == true)
                {
                    if (objUser != null)
                    {
                        throw new MessageException(MessageType.Message, "201308241" ,"", new[] {Util.GetLang("UserName"), userName });
                    }
                }
                bool isNew = false;
                if (objUser == null)
                {
                    isNew = true;
                    objUser = new User();
                    objUser.ResetET();
                    objUser.UserName = userName;
                    objUser.Crtd_Datetime = DateTime.Now;
                    objUser.Crtd_Prog = _screenNbr;
                    objUser.Crtd_User = _userName;
                    objUser.Address = ".";
                    objUser.LastName = ".";
                    objUser.Channel = "DMS";
                    objUser.CheckFirstLogin = false;
                    objUser.ExpireDay = 90;
                    objUser.BeginDay = DateTime.Now;
                    objUser.LoggedIn = true;
                }
                else
                {
                    if (objUser.tstamp.ToHex() != valueTstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
               
                var listUserGroup = _db.SYS_UserGroup.Where(p => p.UserID == userName).ToList();
                foreach(var item in listUserGroup)
                {
                    if(!userGroup.PassNull().Split(',').Contains(item.GroupID))
                        _db.DeleteObject(item);
                }
                foreach (var userGroupItem in userGroup.PassNull().Split(','))
                {
                    if (!string.IsNullOrEmpty(userGroupItem))
                    {
   
                        var objUserGroup = _db.SYS_UserGroup.FirstOrDefault(p => p.UserID == userName && p.GroupID == userGroupItem);
                        if (objUserGroup == null)
                        {
                            objUserGroup = new SYS_UserGroup();
                            objUserGroup.ResetET();
                            objUserGroup.UserID = userName;
                            objUserGroup.GroupID = userGroupItem;
                            objUserGroup.Crtd_Datetime = DateTime.Now;
                            objUserGroup.Crtd_Prog = _screenNbr;
                            objUserGroup.Crtd_User = _userName;
                            objUserGroup.LUpd_Datetime = DateTime.Now;
                            objUserGroup.LUpd_Prog = _screenNbr;
                            objUserGroup.LUpd_User = _userName;
                            _db.SYS_UserGroup.AddObject(objUserGroup);
                        }
                        else
                        {
                            objUserGroup.LUpd_Datetime = DateTime.Now;
                            objUserGroup.LUpd_Prog = _screenNbr;
                            objUserGroup.LUpd_User = _userName;
                        }
                    }
                }

                objUser.Blocked = isBlock;
                if (!string.IsNullOrEmpty(data["dteBlockedTime"]))
                    objUser.BlockedTime = Convert.ToDateTime(data["dteBlockedTime"]);
                objUser.CpnyID = cpnyID;
                objUser.Email = email;

                objUser.FailedLoginCount = failedLoginCount;
                objUser.FirstName = firstName;
                objUser.UserTypes = userType;
                objUser.Manager = manager;
                //objUser.StartDate = StartDate;
                //objUser.EndDate = EndDate;
                if (string.IsNullOrEmpty(passWord))
                {
                    objUser.Password = Encryption.Encrypt("123456", "1210Hq10s081f359t");
                }
                else
                {
                    if (objUser.Password != passWord)
                    {
                        objUser.Password = Encryption.Encrypt(passWord, "1210Hq10s081f359t");
                    }
                }
                if (isNew == true)
                {
                    _db.Users.AddObject(objUser);
                }
                _db.SaveChanges();
                return Json(new { success = true});
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
    }
}

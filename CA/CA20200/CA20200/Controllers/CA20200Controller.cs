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
namespace CA20200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class CA20200Controller : Controller
    {
        private string _screenNbr = "CA20200";
        private string _userName = Current.UserName;

        CA20200Entities _db = Util.CreateObjectContext<CA20200Entities>(false);

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

        public ActionResult GetAccount()
        {
            return this.Store(_db.CA20200_pgLoadAccount().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAccount"]);
                ChangeRecords<CA_Account> lstAccount = dataHandler.BatchObjectData<CA_Account>();
                foreach (CA_Account deleted in lstAccount.Deleted)
                {
                    var del = _db.CA_Account.Where(p => p.BranchID == deleted.BranchID&& p.BankAcct==deleted.BankAcct).FirstOrDefault();
                    if (del != null)
                    {
                        _db.CA_Account.DeleteObject(del);
                    }
                }

                lstAccount.Created.AddRange(lstAccount.Updated);

                foreach (CA_Account curAccount in lstAccount.Created)
                {
                    if (curAccount.BranchID.PassNull() == "" && curAccount.BankAcct.PassNull() == "") continue;

                    var Account = _db.CA_Account.Where(p => p.BranchID.ToLower() == curAccount.BranchID.ToLower() && p.BankAcct.ToLower() == curAccount.BankAcct.ToLower()).FirstOrDefault();

                    if (Account != null)
                    {
                        if (Account.tstamp.ToHex() == curAccount.tstamp.ToHex())
                        {
                            Update_CA_Account(Account, curAccount, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Account = new CA_Account();
                        Update_CA_Account(Account, curAccount, true);
                        _db.CA_Account.AddObject(Account);
                    }
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

        private void Update_CA_Account(CA_Account t, CA_Account s, bool isNew)
        {
            if (isNew)
            {
                t.BranchID = s.BranchID;
                t.BankAcct = s.BankAcct;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.AcctName = s.AcctName;
            t.AcctNbr = s.AcctNbr;
            t.AddrID = s.AddrID;
            t.Active = s.Active;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

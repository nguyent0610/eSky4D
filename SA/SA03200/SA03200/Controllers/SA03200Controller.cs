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
namespace SA03200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA03200Controller : Controller
    {
        private string _screenNbr = "SA03200";
        private string _userName = Current.UserName;
        SA03200Entities _db = Util.CreateObjectContext<SA03200Entities>(false);
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
        public ActionResult GetPPC_License()
        {
            return this.Store(_db.SA03200_pgPPC_License().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstPPC_License"]);
                ChangeRecords<SA03200_pgPPC_License_Result> lstPPC_License = dataHandler.BatchObjectData<SA03200_pgPPC_License_Result>();
                foreach (SA03200_pgPPC_License_Result deleted in lstPPC_License.Deleted)
                {
                    var del = _db.PPC_License.Where(p => p.PDAID == deleted.PDAID && p.BranchID == deleted.BranchID && p.SlsperId == deleted.SlsperId).FirstOrDefault();
                    if (del != null)
                    {
                        _db.PPC_License.DeleteObject(del);
                    }
                }

                lstPPC_License.Created.AddRange(lstPPC_License.Updated);

                foreach (SA03200_pgPPC_License_Result curLang in lstPPC_License.Created)
                {
                    if (curLang.PDAID.PassNull() == "" && curLang.BranchID.PassNull() == "" && curLang.SlsperId.PassNull() == "") continue;

                    var lang = _db.PPC_License.Where(p => p.PDAID.ToLower() == curLang.PDAID.ToLower() && p.BranchID.ToLower() == curLang.BranchID.ToLower() && p.SlsperId.ToLower() == curLang.SlsperId.ToLower()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new PPC_License();
                        Update(lang, curLang, true);
                        _db.PPC_License.AddObject(lang);
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
        private void Update(PPC_License t, SA03200_pgPPC_License_Result s, bool isNew)
        {
            if (isNew)
            {
                t.PDAID = s.PDAID;
                t.SlsperId = s.SlsperId;
                t.BranchID = s.BranchID;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Password = s.Password;
            t.LicenseKey = s.LicenseKey;
            t.ActivitionDay = s.ActivitionDay;
            t.CheckAct = false;
            t.LastSyncDate = s.LastSyncDate == null ? DateTime.Now : (s.LastSyncDate.Year == 1 ? DateTime.Now : s.LastSyncDate);
            t.WorkingDate = s.WorkingDate;
            t.SIMID = s.SIMID;
            t.Status = s.Status;
 
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
      


        
    }
}

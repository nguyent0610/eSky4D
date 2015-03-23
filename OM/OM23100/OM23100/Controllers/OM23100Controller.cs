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
namespace OM23100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23100Controller : Controller
    {
        private string _screenNbr = "OM23100";
        private string _userName = Current.UserName;
        OM23100Entities _db = Util.CreateObjectContext<OM23100Entities>(false);
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
        public ActionResult GetOM_FCS(string BranchID, DateTime FCSDate)
        {
            return this.Store(_db.OM23100_pgOM_FCS(BranchID, FCSDate).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            string BranchID = data["cboDist"];
            string FCSDate_temp = data["dateFcs"];
            DateTime FCSDate = DateTime.Parse(FCSDate_temp);
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_FCS"]);
                ChangeRecords<OM23100_pgOM_FCS_Result> lstOM_FCS = dataHandler.BatchObjectData<OM23100_pgOM_FCS_Result>();
                foreach (OM23100_pgOM_FCS_Result deleted in lstOM_FCS.Deleted)
                {
                    var del = _db.OM_FCS.FirstOrDefault(p => p.SlsperId == deleted.SlsperId && p.BranchID == BranchID && p.FCSDate.Year==FCSDate.Year && p.FCSDate.Month==FCSDate.Month);
                    if (del != null)
                    {
                        _db.OM_FCS.DeleteObject(del);
                    }
                }

                lstOM_FCS.Created.AddRange(lstOM_FCS.Updated);

                foreach (OM23100_pgOM_FCS_Result curLang in lstOM_FCS.Created)
                {
                    if (curLang.SlsperId.PassNull() == "") continue;

                    var lang = _db.OM_FCS.FirstOrDefault(p => p.SlsperId.ToLower() == curLang.SlsperId.ToLower() && p.BranchID.ToLower() == BranchID.ToLower() && p.FCSDate.Year == FCSDate.Year && p.FCSDate.Month == FCSDate.Month);

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_AccessDetRights(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_FCS();
                        lang.BranchID = BranchID;
                        lang.FCSDate=FCSDate;
                        Update_AccessDetRights(lang, curLang, true);
                        _db.OM_FCS.AddObject(lang);
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
        private void Update_AccessDetRights(OM_FCS t, OM23100_pgOM_FCS_Result s, bool isNew)
        {
            if (isNew)
            {
                t.SlsperId = s.SlsperId;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Coverage = s.Coverage;
            t.DNA = s.DNA;
            t.Visit = s.Visit;
            t.SellIn = s.SellIn;
            t.LPPC = s.LPPC;
            t.ForcusedSKU = s.ForcusedSKU;
            t.VisitTime = s.VisitTime;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
    }
}

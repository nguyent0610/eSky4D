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
namespace OM24000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM24000Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "OM24000";
        private string _userName = Current.UserName;
        OM24000Entities _db = Util.CreateObjectContext<OM24000Entities>(false);
        private JsonResult mLogMessage;
        private FormCollection mForm;
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

        public ActionResult GetPPC_DiscConsumers(string BranchID, string SlsperID, string CustID,DateTime FromDate,DateTime ToDate)
        {
            return this.Store(_db.OM24000_pgLoadGrid(BranchID, SlsperID, CustID, FromDate, ToDate).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                var errorRecoverd = "";
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstPPC_DiscConsumers"]);
                ChangeRecords<OM24000_pgLoadGrid_Result> lstLang = dataHandler.BatchObjectData<OM24000_pgLoadGrid_Result>();
                var objItem=lstLang.Updated.Count>0?lstLang.Updated[0]:null;
                if(objItem!=null)
                {
                List<PPC_DiscConsumers> lstdata =_db.PPC_DiscConsumers.Where(p=>p.BranchID==objItem.BranchID&& p.SlsperID==objItem.SlsperID).ToList();
                List<OM24000_pgLoadGrid_Result> lstGrid = lstLang.Updated;
                ////cap nhat lai cho so luong lstdata
                //foreach (var obj in lstGrid)
                //{
                //    var obj=lstdata.dat
                //}


                foreach (OM24000_pgLoadGrid_Result curLang in lstLang.Updated)
                {
                    if (curLang.BranchID.PassNull() == "" || curLang.SlsperID.PassNull() == "" || curLang.CustID.PassNull() == "" || curLang.InvtID.PassNull() == "" || curLang.VisitDate.PassNull() == "") continue;

                    var lang = _db.PPC_DiscConsumers.Where(p => p.BranchID.ToLower() == curLang.BranchID.ToLower()
                                                             && p.SlsperID.ToLower() == curLang.SlsperID.ToLower()
                                                             && p.CustID.ToLower() == curLang.CustID.ToLower()
                                                             && p.InvtID.ToLower() == curLang.InvtID.ToLower()
                                                             && p.VisitDate.Year == curLang.VisitDate.Year
                                                             && p.VisitDate.Month == curLang.VisitDate.Month
                                                             && p.VisitDate.Day == curLang.VisitDate.Day
                                                            ).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Language(lang, curLang);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                }

   
                var lst = _db.PPC_DiscConsumers.ToList();
                string error="";

                foreach (var tot in lstLang.Updated.Select(p=>p.InvtID).Distinct())
                {
                    var objTot = _db.OM24000_ppGetTotAlloc(lstLang.Updated[0].BranchID).Where(p => p.InvtID == tot).FirstOrDefault();
                    var obj = lst.Where(p => p.BranchID == lstLang.Updated[0].BranchID && p.InvtID == tot).Sum(p => p.AdvanceNew);
                    if (obj > (objTot == null ? 0 : objTot.TotAlloc)) error += tot + ",";
                }

                if (error != "")
                {
                    throw new MessageException(MessageType.Message, "2015062001", parm: new[] { error.TrimEnd(',') });
                }

                _db.SaveChanges();

                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_Language(PPC_DiscConsumers t, OM24000_pgLoadGrid_Result s)
        {
            t.AdvanceNew = s.AdvanceNew;
            t.Recovered = s.Recovered;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

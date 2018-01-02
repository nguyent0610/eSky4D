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
using HQ.eSkySys;
using HQFramework.DAL;
namespace IN40300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN40300Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN40300";
        private string _userName = Current.UserName;
        IN40300Entities _db = Util.CreateObjectContext<IN40300Entities>(false);
        //private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult mLogMessage;
        private FormCollection mForm;
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

        //public ActionResult GetData(DateTime DateFrom, DateTime DateTo, string BranchID)
        //{
        //    return this.Store(_db.IN40300_pgLoadGrid(DateFrom, DateTo, BranchID).ToList());
        //}

        [HttpPost]
        public ActionResult Process(FormCollection data)
        {
            try
            {
                
                bool inValidate = false;
                bool inRebuildQtyCost = false;
                bool inRebuildOnSOQty = false;

                string a = data["chkINValidate"];

                if (data["chkINValidates"] == "true")
                {
                    inValidate = true;
                }
                if (data["chkCalQtys"] == "true")
                {
                    inRebuildQtyCost = true;
                }
                if (data["chkCalSOQtys"] == "true")
                {
                    inRebuildOnSOQty = true;
                }
                string param01 = data["type"];
                string param02 = inValidate.ToShort().ToString();
                string param03 = inRebuildOnSOQty.ToShort().ToString();
                string param04 = inRebuildQtyCost.ToShort().ToString();

                if (param01 == "2")
                {
                    param02 = data["cboStatus"];
                    param03 = data["txtBranchID"];
                    param04 = data["txtBatNbr"];
                }

                List<string> lstLang = new List<string>();


                string langID = Current.LangID.ToString();
                    string langObj =
                       _db.vs_Language.Where(p => p.Code.ToLower() == "obj").Select(
                            p =>
                            langID == "0" ? p.Lang00 :
                            langID == "1" ? p.Lang01 :
                            langID == "2" ? p.Lang02 :
                            langID == "3" ? p.Lang03 :
                            langID == "4" ? p.Lang04 : "")
                            .FirstOrDefault();

                    string langMsg0 =
                        _db.vs_Language.Where(p => p.Code.ToLower() == "inintegmsg00").Select(
                            p =>
                            langID == "0" ? p.Lang00 :
                            langID == "1" ? p.Lang01 :
                            langID == "2" ? p.Lang02 :
                            langID == "3" ? p.Lang03 :
                            langID == "4" ? p.Lang04 : "")
                            .FirstOrDefault();
                    string langMsg1 =
                       _db.vs_Language.Where(p => p.Code.ToLower() == "inintegmsg01").Select(
                           p =>
                           langID == "0" ? p.Lang00 :
                           langID == "1" ? p.Lang01 :
                           langID == "2" ? p.Lang02 :
                           langID == "3" ? p.Lang03 :
                           langID == "4" ? p.Lang04 : "")
                           .FirstOrDefault();
                    lstLang.Add(langObj);
                    lstLang.Add(langMsg0);
                    lstLang.Add(langMsg1);
                

                string param05 = data["cboInvtID"];
                string param06 = data["cboSiteID"];
                string message = string.Empty;
                DataAccess dal = Util.Dal();
                INProcess.IN inventory = new INProcess.IN(_userName, _screenNbr, dal);
                try
                {
                    dal.BeginTrans(IsolationLevel.ReadCommitted);

                    message = inventory.IN40300_Release(lstLang, param01, param02, param03, param04, param05, param06);

                    dal.CommitTrans();
                }
                catch (Exception)

                {
                    dal.RollbackTrans();
                    throw;
                }
                finally
                {
                    inventory = null;
                    dal = null;
                }

                //_db.SaveChanges();

                return Json(new { success = true, Msg = message }, JsonRequestBehavior.AllowGet);
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

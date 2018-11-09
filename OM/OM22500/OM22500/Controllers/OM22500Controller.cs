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
namespace OM22500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22500Controller : Controller
    {
        private string _screenNbr = "OM22500";
        private string _userName = Current.UserName;
        OM22500Entities _db = Util.CreateObjectContext<OM22500Entities>(false);
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            var Reasonable = false; 
            var ReasonIsShow = false;
            var objConfig = _db.OM22500_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                Reasonable = objConfig.Reasonable.Value && objConfig.Reasonable.HasValue;
                ReasonIsShow = objConfig.ReasonIsShow.Value && objConfig.ReasonIsShow.HasValue;
            }
            ViewBag.Reasonable = Reasonable;
            ViewBag.ReasonIsShow = ReasonIsShow;


            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetOM_ReasonCode()
        {           
            return this.Store(_db.OM22500_pgOM_ReasonCode().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_ReasonCode"]);
                ChangeRecords<OM22500_pgOM_ReasonCode_Result> lstOM_ReasonCode = dataHandler.BatchObjectData<OM22500_pgOM_ReasonCode_Result>();

                lstOM_ReasonCode.Created.AddRange(lstOM_ReasonCode.Updated);

                foreach (OM22500_pgOM_ReasonCode_Result deleted in lstOM_ReasonCode.Deleted)
                {
                    if (lstOM_ReasonCode.Created.Where(p => p.Code == deleted.Code).Count() > 0)
                    {
                        lstOM_ReasonCode.Created.Where(p => p.Code == deleted.Code).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_ReasonCode.FirstOrDefault(p => p.Code == deleted.Code);
                        if (objDel != null)
                        {
                            _db.OM_ReasonCode.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM22500_pgOM_ReasonCode_Result curLang in lstOM_ReasonCode.Created)
                {
                    if (curLang.Code.PassNull() == "") continue;

                    var lang = _db.OM_ReasonCode.Where(p => p.Code.ToLower() == curLang.Code.ToLower()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Language(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_ReasonCode();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.OM_ReasonCode.AddObject(lang);
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
        private void Update_Language(OM_ReasonCode t, OM22500_pgOM_ReasonCode_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Reasonable = s.Reasonable;
            t.ReasonIsShow = s.ReasonIsShow;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
       
    }
}

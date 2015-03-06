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
namespace IN20700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20700Controller : Controller
    {
        private string _screenNbr = "IN20700";
        private string _userName = Current.UserName;
        IN20700Entities _db = Util.CreateObjectContext<IN20700Entities>(false);
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
        public ActionResult GetReasonCode()
        {           
            return this.Store(_db.IN20700_pgLoadReasonCode().ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstReasonCode"]);
                ChangeRecords<IN_ReasonCode> lstReasonCode = dataHandler.BatchObjectData<IN_ReasonCode>();
                foreach (IN_ReasonCode deleted in lstReasonCode.Deleted)
                {
                    var del = _db.IN_ReasonCode.Where(p => p.ReasonCD == deleted.ReasonCD).FirstOrDefault();
                    if (del != null)
                    {
                        _db.IN_ReasonCode.DeleteObject(del);
                    }
                }

                lstReasonCode.Created.AddRange(lstReasonCode.Updated);

                foreach (IN_ReasonCode curReasonCode in lstReasonCode.Created)
                {
                    if (curReasonCode.ReasonCD.PassNull() == "") continue;

                    var ReasonCode = _db.IN_ReasonCode.Where(p => p.ReasonCD.ToLower() == curReasonCode.ReasonCD.ToLower()).FirstOrDefault();

                    if (ReasonCode != null)
                    {
                        if (ReasonCode.tstamp.ToHex() == curReasonCode.tstamp.ToHex())
                        {
                            Update_IN_ReasonCode(ReasonCode, curReasonCode, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        ReasonCode = new IN_ReasonCode();
                        Update_IN_ReasonCode(ReasonCode, curReasonCode, true);
                        _db.IN_ReasonCode.AddObject(ReasonCode);
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

        private void Update_IN_ReasonCode(IN_ReasonCode t, IN_ReasonCode s, bool isNew)
        {
            if (isNew)
            {
                t.ReasonCD = s.ReasonCD;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.SiteID = s.SiteID;
            t.SlsperID = s.SlsperID;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

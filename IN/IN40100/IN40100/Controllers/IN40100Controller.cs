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


namespace IN40100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN40100Controller : Controller
    {

        private string mCpnyID = Current.CpnyID;
        private string _screenNbr = "IN40100";
        private string _userName = Current.UserName;
        IN40100Entities _db = Util.CreateObjectContext<IN40100Entities>(false);
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

        public ActionResult GetData(string PerNbr, string SiteID, short Type)
        {
            return this.Store(_db.IN40100_pgGetListItemForCosting(PerNbr, SiteID, Type).ToList());
        }

        [HttpPost]
        public ActionResult Process(FormCollection data,bool mess76,bool mess81)
        {
            try
            {
                
                string PerNbr=data["txtPerPost"].PassNull();
                var lstwrk = _db.IN_WrkCosting.ToList();
                if (mess76) goto MESS76;
                if (mess81) goto MESS81;
                if (lstwrk.Count > 0)
                {
                    throw new MessageException(MessageType.Message, "74");
                }

                throw new MessageException(MessageType.Message, "76",fn:"process76");
            MESS76:
                var endCost = _db.IN_EndingCost.FirstOrDefault(p => p.PerNbr == PerNbr);
                if (endCost != null)
                {
                    throw new MessageException(MessageType.Message, "81", fn: "process81");
                MESS81:
                    Data_Release();
                }
                else
                {
                    Data_Release();
                }
                return Json(new { success = true });
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

        private void Data_Release()
        {

        }


    }
}

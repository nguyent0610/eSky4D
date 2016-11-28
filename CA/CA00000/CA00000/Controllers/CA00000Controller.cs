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
namespace CA00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class CA00000Controller : Controller
    {
        private string _screenNbr = "CA00000";
        private string _userName = Current.UserName;

        CA00000Entities _db = Util.CreateObjectContext<CA00000Entities>(false);

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

        public ActionResult GetCA_Setup(string branchId, string setupID)
        {
            var CASetups = _db.CA00000_pdLoadSetup().Where(p => p.BranchID == branchId && p.SetUpID == setupID).ToList();
            return this.Store(CASetups);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstCA_Setup"]);
                ChangeRecords<CA_Setup> lstLang = dataHandler.BatchObjectData<CA_Setup>();


                foreach (CA_Setup currecord in lstLang.Updated)
                {
                    var record = _db.CA_Setup.Where(p => p.BranchID == currecord.BranchID).FirstOrDefault();

                    if (record != null)
                    {
                        if (record.tstamp.ToHex() == record.tstamp.ToHex())
                        {
                            UpdatingPOSetup(record, currecord, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else {
                        record = new CA_Setup();
                        UpdatingPOSetup(record, currecord, true);
                        _db.CA_Setup.AddObject(record);
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
        private void UpdatingPOSetup(CA_Setup t,  CA_Setup s, bool isNew)
        {
            if (isNew) {
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
                t.BranchID = Current.CpnyID;
                t.SetUpID = "CA";
            }
            t.LastBatNbr = s.LastBatNbr;
            t.LastPaymentNbr = s.LastPaymentNbr;
            t.LastReceiptNbr = s.LastReceiptNbr;
            t.PreFixBat = s.PreFixBat;
            
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

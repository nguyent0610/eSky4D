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
namespace IN00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN00000Controller : Controller
    {
        private string _screenNbr = "IN00000";
        private string _userName = Current.UserName;
        IN00000Entities _db = Util.CreateObjectContext<IN00000Entities>(false);

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

        public ActionResult GetIN00000(string BranchID, string SetupID)
        {
            var setupData = _db.IN_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupID == SetupID);
            return this.Store(setupData);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data,bool isNew)
        {
            
            try
            {
                // Get params from data that's sent from client (Ajax)
                string BranchID = Current.CpnyID;
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstIN00000"]);
                ChangeRecords<IN_Setup> lstIN00000 = dataHandler.BatchObjectData<IN_Setup>();
                foreach (IN_Setup setup in lstIN00000.Updated)
                {
                    var objHeader = _db.IN_Setup.FirstOrDefault(p => p.BranchID == BranchID && p.SetupID == "IN");
                    if (isNew)//new record
                    {
                        if (objHeader != null)
                            return Json(new { success = false, msgCode = 2000, msgParam = BranchID });//quang message ma nha cung cap da ton tai ko the them
                        else
                        {
                            objHeader = new IN_Setup();
                            objHeader.BranchID = BranchID;
                            objHeader.SetupID = "IN";
                            objHeader.Crtd_DateTime = DateTime.Now;
                            objHeader.Crtd_Prog = _screenNbr;
                            objHeader.Crtd_User = Current.UserName;
                            UpdatingHeader(setup, ref objHeader);
                            // Add data to IN_Setup
                            _db.IN_Setup.AddObject(objHeader);
                            _db.SaveChanges();
                        }
                    }
                    else if (objHeader != null)//update record
                    {
                        if (objHeader.tstamp.ToHex() == setup.tstamp.ToHex())
                        {
                            UpdatingHeader(setup, ref objHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                        _db.SaveChanges();

                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(IN_Setup s,ref IN_Setup d)
        {
          
            d.AutoRefNbr = s.AutoRefNbr;
            d.CnvFactEditable = s.CnvFactEditable;
            d.NegQty = s.NegQty;
            d.LastBatNbr = s.LastBatNbr;
            d.LastRefNbr = s.LastRefNbr;
            d.LastTransferNbr = s.LastTransferNbr;
            d.LastIssueNbr = s.LastIssueNbr;
            d.LastRcptNbr = s.LastRcptNbr;
            d.DfltValMthd = s.DfltValMthd;
            d.DfltSite = s.DfltSite;
            d.PreFixBat = s.PreFixBat;
            d.UseBarCode = s.UseBarCode;
            d.CheckINVal = s.CheckINVal;

            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = _userName;
        }

    }
}

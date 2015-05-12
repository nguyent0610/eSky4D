using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
//using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
namespace IN23000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN23000Controller : Controller
    {
        private string _screenNbr = "IN23000";
        private string _noneStatus = "N";
        private string _beginStatus = "H";
        IN23000Entities _db = Util.CreateObjectContext<IN23000Entities>(false);
        //
        // GET: /IN23000/
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

        public ActionResult GetPosmInfo(string posmID)
        {
            var posm = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID == posmID);
            return this.Store(posm);
        }

        public ActionResult GetBranch(string posmID)
        {
            var dets = _db.IN23000_pgBranch(Current.UserName, posmID).ToList();
            return this.Store(dets);
        }

        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var posmID = data["cboPosmID"];
                var status = data["cboStatus"];
                var handle = data["cboHandle"];

                if (!string.IsNullOrWhiteSpace(posmID))
                {
                    var posm = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID == posmID);

                    if (posm != null)
                    {
                        var custHandler = new StoreDataHandler(data["lstDetChange"]);
                        var lstDetChange = custHandler.BatchObjectData<IN23000_pgBranch_Result>();

                        foreach (var updated in lstDetChange.Updated)
                        {
                            if (!string.IsNullOrWhiteSpace(updated.BranchID))
                            {
                                updated.PosmID = posmID;

                                var updatedCpny = _db.IN_POSMCust.FirstOrDefault(
                                    x => x.BranchID == updated.BranchID
                                        && x.PosmID == updated.PosmID
                                        && x.CustID == updated.CustID
                                        && x.PosmCode == updated.PosmCode);
                                if (updatedCpny != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(handle) 
                                        && updatedCpny.Status == status
                                        && updatedCpny.Status != handle)
                                    {
                                        if (status == _beginStatus && handle == _noneStatus)
                                        { 
                                            // update AppQty
                                            updatedCpny.AppQty = updated.AppQty;
                                        }
                                        else if (status == _beginStatus && handle != _noneStatus)
                                        {
                                            // update AppQty va Status
                                            updatedCpny.AppQty = updated.AppQty;
                                            updatedCpny.Status = handle;
                                        }
                                        else if(handle != _noneStatus)
                                        { 
                                            // update Status
                                            updatedCpny.Status = handle;
                                        }
                                        updatedCpny.LUpd_DateTime = DateTime.Now;
                                        updatedCpny.LUpd_Prog = _screenNbr;
                                        updatedCpny.LUpd_User = Current.UserName;
                                    }
                                }
                            }
                        }

                        _db.SaveChanges();
                        return Json(new { success = true, msgCode = 201405071 });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "8");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "744");
                }
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

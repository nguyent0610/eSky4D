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
namespace IN22000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN22000Controller : Controller
    {
        private string _screenNbr = "IN22000";
        private string _beginStatus = "H";
        IN22000Entities _db = Util.CreateObjectContext<IN22000Entities>(false);
        //
        // GET: /IN22000/
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
            var dets = _db.IN22000_pgBranch(Current.UserName, posmID).ToList();
            return this.Store(dets);
        }

        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var posmID = data["cboPosmID"];
                if (!string.IsNullOrWhiteSpace(posmID))
                {
                    var posm = _db.IN_POSMHeader.FirstOrDefault(x => x.PosmID == posmID);

                    if (posm != null)
                    {
                        var custHandler = new StoreDataHandler(data["lstDetChange"]);
                        var lstDetChange = custHandler.BatchObjectData<IN22000_pgBranch_Result>();

                        foreach (var created in lstDetChange.Created)
                        {
                            if (!string.IsNullOrWhiteSpace(created.BranchID))
                            {
                                created.PosmID = posmID;

                                var createdCpny = _db.IN_POSMCust.FirstOrDefault(
                                    x => x.BranchID == created.BranchID
                                        && x.PosmID == created.PosmID
                                        && x.CustID == created.CustID
                                        && x.PosmCode == created.PosmCode);
                                if (createdCpny == null)
                                {
                                    createdCpny = new IN_POSMCust();
                                    updateBranch(ref createdCpny, created, true);
                                    _db.IN_POSMCust.AddObject(createdCpny);
                                }
                            }
                        }

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
                                    updateBranch(ref updatedCpny, updated, true);
                                }
                            }
                        }

                        foreach (var deleted in lstDetChange.Deleted)
                        {
                            if (!string.IsNullOrWhiteSpace(deleted.BranchID))
                            {
                                deleted.PosmID = posmID;

                                var deletedCpny = _db.IN_POSMCust.FirstOrDefault(
                                    x => x.BranchID == deleted.BranchID
                                        && x.PosmID == deleted.PosmID
                                        && x.CustID == deleted.CustID
                                        && x.PosmCode == deleted.PosmCode);
                                if (deletedCpny != null)
                                {
                                    _db.IN_POSMCust.DeleteObject(deletedCpny);
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

        public ActionResult DeletePosm(string posmID)
        {
            try
            {
                var posm = _db.IN_POSMHeader.FirstOrDefault(p => p.PosmID == posmID);
                if (posm != null)
                {
                    var cpnies = _db.IN_POSMCust.Where(c => c.PosmID == posmID).ToList();
                    foreach (var cpny in cpnies)
                    {
                        if (cpny.Status == _beginStatus)
                        {
                            _db.IN_POSMCust.DeleteObject(cpny);
                        }
                        else 
                        {
                            throw new MessageException(MessageType.Message, "20140306");
                        }
                    }
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("PosmID") });
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

        private void updateBranch(ref IN_POSMCust createdCpny, IN22000_pgBranch_Result created, bool isNew)
        {
            if (isNew)
            {
                createdCpny.ResetET();
                createdCpny.PosmID = created.PosmID;
                createdCpny.BranchID = created.BranchID;
                createdCpny.CustID = created.CustID;
                createdCpny.SlsperID = created.SlsperID;
                createdCpny.PosmCode = created.PosmCode;
                createdCpny.Status = _beginStatus;
                createdCpny.IsAgree = false;

                createdCpny.Crtd_DateTime = DateTime.Now;
                createdCpny.Crtd_Prog = _screenNbr;
                createdCpny.Crtd_User = Current.UserName;
            }
            createdCpny.Qty = created.Qty;
            createdCpny.AppQty = created.Qty;
            createdCpny.LUpd_DateTime = DateTime.Now;
            createdCpny.LUpd_Prog = _screenNbr;
            createdCpny.LUpd_User = Current.UserName;
        }
    }
}

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
using HQ.eSkySys;
using System.Collections.ObjectModel;

namespace SI22000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI22000Controller : Controller
    {
        string screenNbr = "SI22000";
        SI22000Entities _db = Util.CreateObjectContext<SI22000Entities>(false);


        public ActionResult Index()
        {
            Util.InitRight(screenNbr);
            ViewBag.BussinessDate = DateTime.Now.ToDateShort();
            ViewBag.BussinessTime = DateTime.Now;
            var obj= _db.SI22000_ppCon().FirstOrDefault();
            ViewBag.SI22000SetUp = obj == null ? "" : obj.TextVal;
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
          
            return PartialView();
        }

        public ActionResult GetCon()
        {
            return this.Store(_db.SI22000_ppCon());
        }
       
        //public string CheckIsHO()
        //{
        //    var lstUser = _db.SI22000_ppCheckIsHO(Current.UserName).ToList();
        //    if (lstUser.Count > 0)
        //    {
        //        return lstUser.FirstOrDefault().UserTypes;
        //    }
        //    return string.Empty;
        //}

        public ActionResult GetCycle(string yearNbr)
        {
            return this.Store(_db.SI22000_pgLoadCycle(yearNbr).ToList());
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string year_temp = data["dateKPI"];
                DateTime year = DateTime.Parse(year_temp);
                string yearNbr = year.Year.PassNull().ToString();

                string status = data["cboStatus"];
                string handle = data["cboHandle"];
                 
               
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
                ChangeRecords<SI22000_pgLoadCycle_Result> lstgrd = dataHandler1.BatchObjectData<SI22000_pgLoadCycle_Result>();


                if (lstgrd == null) {
                    return Json(new { success = false, type = "error" });
                }
                if (status != "C" && status != "H" && handle != "N") {
                    return Json(new { success = false, type = "error" });
                }

                for (int i = 0; i < lstgrd.Updated.Count(); i++)
                {
                    var b = lstgrd.Updated[i];
                    if (b != null)
                    {
                        if (b.StartDate > b.EndDate)
                        {
                            return Json(new { success = false, type = "error" });
                        }
                    }
                    if ((i + 1) < lstgrd.Updated.Count())
                    {
                        var c = lstgrd.Updated[i + 1];
                       
                        if (c != null)
                        {
                            if (b.EndDate > c.StartDate)
                            {
                                return Json(new { success = false, type = "error" });
                            }
                        }
                    }
                    
                }
                foreach (SI22000_pgLoadCycle_Result updated in lstgrd.Updated)
                {

                    var record = _db.SI_Cycle.Where(p => p.YearNbr == yearNbr && p.CycleNbr == updated.CycleNbr).FirstOrDefault();


                    if (record != null)
                    {
                        record.StartDate = updated.StartDate;
                        record.EndDate = updated.EndDate;
                        record.Status = status;
                        record.WorkingDay = updated.WorkingDay;

                        record.LUpd_DateTime = DateTime.Now;
                        record.LUpd_Prog = screenNbr;
                        record.LUpd_User = Current.UserName;
                    }
                    else
                    {
                        record = new SI_Cycle();
                        record.YearNbr = yearNbr;
                        record.CycleNbr = updated.CycleNbr;
                        record.StartDate = updated.StartDate;
                        record.EndDate = updated.EndDate;
                        record.WorkingDay = updated.WorkingDay;
                        if (handle == "N" || handle == null) {
                            record.Status = handle.PassNull();
                        }
                        record.Status = status;
                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        record.LUpd_DateTime = DateTime.Now;
                        record.LUpd_Prog = screenNbr;
                        record.LUpd_User = Current.UserName;
                        if (record.YearNbr != "" && record.CycleNbr != "")
                        {
                            _db.SI_Cycle.AddObject(record);
                        }
                    }
                
                }
                _db.SaveChanges();

                if (handle != "N" && handle != null)
                {
                    foreach (var obj in _db.SI_Cycle.Where(p => p.YearNbr == yearNbr))
                    {
                        string branch = Current.CpnyID;
                        var data1 = (from p in _db.HO_PendingTasks
                                     where p.ObjectID == yearNbr
                                       && p.EditScreenNbr == screenNbr
                                       && p.BranchID == branch
                                     select p).FirstOrDefault();

                        var handle1 = (from p in _db.SI_ApprovalFlowHandle
                                       where p.AppFolID == screenNbr && p.Status == status && p.Handle == handle
                                       select p).FirstOrDefault();
                        obj.Status = handle1.ToStatus;
                        if (data1 == null && handle1 != null)
                        {
                            if (!handle1.Param00.PassNull().Split(',').Any(p => p.ToLower() == "notapprove"))
                            {
                                HO_PendingTasks newTask = new HO_PendingTasks();
                                newTask.BranchID = branch;
                                newTask.ObjectID = yearNbr;
                                newTask.EditScreenNbr = screenNbr;
                                newTask.Content = string.Format(handle1.ContentApprove.PassNull(), yearNbr, "", branch);
                                if (newTask.Content.PassNull().Trim() == "")
                                    newTask.Content = "Chưa định nghĩa ContentApprove";
                                newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                                newTask.Crtd_Prog = newTask.LUpd_Prog = screenNbr;
                                newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                                newTask.Status = handle1.ToStatus;
                                _db.HO_PendingTasks.AddObject(newTask);

                            }
                        }
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


    }
}

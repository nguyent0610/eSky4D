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

namespace OM23900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23900Controller : Controller
    {
        private string _screenNbr = "OM23900";
        private string _userName = Current.UserName;
        OM23900Entities _db = Util.CreateObjectContext<OM23900Entities>(false);

       
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

        public ActionResult GetOM_DiscConsumer(string BranchID)
        {
            return this.Store(_db.OM23900_pgLoadGrid(BranchID).ToList());
        }

        public ActionResult GetPPC_DisConsumers(string BranchID)
        {
            return this.Store(_db.OM23900_pdPPC_DisConsumers(BranchID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string BranchID = data["cboBranchID"].PassNull();
                
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_DiscConsumers"]);
                ChangeRecords<OM23900_pgLoadGrid_Result> lstOM_DiscConsumers = dataHandler.BatchObjectData<OM23900_pgLoadGrid_Result>();

                //var lstPPC_DiscConsumers = _db.OM23900_pdPPC_DisConsumers(BranchID).ToList();

                //foreach (OM23900_pgLoadGrid_Result deleted in lstOM_DiscConsumers.Deleted)
                //{
                //    var flag = lstPPC_DiscConsumers.FirstOrDefault(p => p.InvtID == deleted.InvtID);
                //    if (flag!=null)
                //    {
                //        throw new MessageException(MessageType.Message, "2015061901",parm:new string []{deleted.InvtID});
                //    }
                //    else
                //    {
                //        var del = _db.OM_DiscConsumers.Where(p => p.BranchID == BranchID && p.InvtID == deleted.InvtID).FirstOrDefault();
                //        if (del != null)
                //        {
                //            _db.OM_DiscConsumers.DeleteObject(del);
                //        }
                //    }
                //}

                lstOM_DiscConsumers.Created.AddRange(lstOM_DiscConsumers.Updated);

                foreach (OM23900_pgLoadGrid_Result curLang in lstOM_DiscConsumers.Created)
                {
                    if (curLang.InvtID.PassNull() == "" || BranchID =="") continue;

                    var lang = _db.OM_DiscConsumers.FirstOrDefault(p => p.BranchID.ToLower() == BranchID.ToLower() && p.InvtID.ToLower() == curLang.InvtID.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingOM_DiscConsumers(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_DiscConsumers();
                        lang.ResetET();
                        lang.BranchID = BranchID;
                        UpdatingOM_DiscConsumers(lang, curLang, true);
                        _db.OM_DiscConsumers.AddObject(lang);
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


        private void UpdatingOM_DiscConsumers(OM_DiscConsumers t, OM23900_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.InvtID = s.InvtID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.TotAlloc = s.TotAlloc;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data,string InvtID)
        {
            try
            {
                string BranchID = data["cboBranchID"].PassNull();

                var lstPPC_DiscConsumers = _db.OM23900_pdPPC_DisConsumers(BranchID).ToList();
                var flag = lstPPC_DiscConsumers.FirstOrDefault(p => p.InvtID == InvtID);

                if (flag != null)
                {
                    throw new MessageException(MessageType.Message, "2015061901", parm: new[] { InvtID });
                }
                else
                {
                    var del = _db.OM_DiscConsumers.FirstOrDefault(p => p.BranchID == BranchID && p.InvtID == InvtID);
                    if (del != null)
                    {
                        _db.OM_DiscConsumers.DeleteObject(del);
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

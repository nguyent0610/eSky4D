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
namespace AR23800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR23800Controller : Controller
    {
        private string _screenNbr = "AR23800";
        private string _userName = Current.UserName;
        AR23800Entities _db = Util.CreateObjectContext<AR23800Entities>(false);
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
        public ActionResult GetCodeID()
        {
            return this.Store(_db.AR23800_pgAR_Position(Current.CpnyID, Current.UserName, Current.LangID).ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstCodeID"]);
                ChangeRecords<AR23800_pgAR_Position_Result> lstCodeID = dataHandler.BatchObjectData<AR23800_pgAR_Position_Result>();
                foreach (AR23800_pgAR_Position_Result deleted in lstCodeID.Deleted)
                {

                    if (lstCodeID.Created.Where(p => p.CodeID == deleted.CodeID).Count() > 0)
                    {
                        lstCodeID.Created.Where(p => p.CodeID == deleted.CodeID).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.AR_Position.Where(p => p.CodeID == deleted.CodeID).FirstOrDefault();
                        if (del != null)
                        {
                            if(_db.AR23800_ppCheckDeletePosition(deleted.CodeID).FirstOrDefault() == "1")
                                throw new MessageException(MessageType.Message, "18", "");
                            _db.AR_Position.DeleteObject(del);
                        }
                    }

                }

                lstCodeID.Created.AddRange(lstCodeID.Updated);

                foreach (AR23800_pgAR_Position_Result curCodeID in lstCodeID.Created)
                {
                    if (curCodeID.CodeID.PassNull() == "") continue;

                    var CodeID = _db.AR_Position.Where(p => p.CodeID.ToLower() == curCodeID.CodeID.ToLower()).FirstOrDefault();

                    if (CodeID != null)
                    {
                        if (CodeID.tstamp.ToHex() == curCodeID.tstamp.ToHex())
                        {
                            Update_AR_Position(CodeID, curCodeID, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        CodeID = new AR_Position();
                        Update_AR_Position(CodeID, curCodeID, true);
                        _db.AR_Position.AddObject(CodeID);
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

        private void Update_AR_Position(AR_Position t, AR23800_pgAR_Position_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CodeID = s.CodeID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

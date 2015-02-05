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
namespace CA20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class CA20100Controller : Controller
    {
        private string _screenNbr = "CA20100";
        private string _userName = Current.UserName;

        CA20100Entities _db = Util.CreateObjectContext<CA20100Entities>(false);

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

        public ActionResult GetEntryType()
        {
            var entryTypes = _db.CA20100_pgLoadEntryType().ToList();
            return this.Store(entryTypes);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstEntryType"]);
                ChangeRecords<CA_EntryType> lstEntryType = dataHandler.BatchObjectData<CA_EntryType>();
                foreach (CA_EntryType deleted in lstEntryType.Deleted)
                {
                    var del = _db.CA_EntryType.Where(p => p.EntryID == deleted.EntryID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.CA_EntryType.DeleteObject(del);
                    }
                }

                lstEntryType.Created.AddRange(lstEntryType.Updated);

                foreach (CA_EntryType curEntryType in lstEntryType.Created)
                {
                    if (curEntryType.EntryID.PassNull() == "") continue;

                    var EntryType = _db.CA_EntryType.Where(p => p.EntryID.ToLower() == curEntryType.EntryID.ToLower()).FirstOrDefault();

                    if (EntryType != null)
                    {
                        if (EntryType.tstamp.ToHex() == curEntryType.tstamp.ToHex())
                        {
                            Update_CA_EntryType(EntryType, curEntryType, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        EntryType = new CA_EntryType();
                        Update_CA_EntryType(EntryType, curEntryType, true);
                        _db.CA_EntryType.AddObject(EntryType);
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

        private void Update_CA_EntryType(CA_EntryType t, CA_EntryType s, bool isNew)
        {
            if (isNew)
            {
                t.EntryID = s.EntryID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.descr = s.descr;
            t.active = s.active;
            t.RcptDisbFlg = s.RcptDisbFlg;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

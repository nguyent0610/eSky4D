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
namespace SA01900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA01900Controller : Controller
    {
        private string _screenNbr = "SA01900";
        private string _userName = Current.UserName;
        SA01900Entities _db = Util.CreateObjectContext<SA01900Entities>(true);
        public ActionResult Index()
        {
            
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string selectRow)
        {
            return PartialView();
        }

        public ActionResult GetSYS_RibbonTab()
        {
            return this.Store(_db.SA01900_pgSYS_RibbonTab().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_RibbonTab"]);
                ChangeRecords<SA01900_pgSYS_RibbonTab_Result> lstSYS_RibbonTab = dataHandler.BatchObjectData<SA01900_pgSYS_RibbonTab_Result>();
                foreach (SA01900_pgSYS_RibbonTab_Result deleted in lstSYS_RibbonTab.Deleted)
                {
                    var del = _db.SYS_RibbonTab.Where(p => p.TabID == deleted.TabID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_RibbonTab.DeleteObject(del);
                    }
                }

                lstSYS_RibbonTab.Created.AddRange(lstSYS_RibbonTab.Updated);

                foreach (SA01900_pgSYS_RibbonTab_Result curRow in lstSYS_RibbonTab.Created)
                {
                    if (curRow.TabID.PassNull() == "") continue;

                    var selectRow = _db.SYS_RibbonTab.Where(p => p.TabID.ToLower() == curRow.TabID.ToLower()).FirstOrDefault();

                    if (selectRow != null)
                    {
                        if (selectRow.tstamp.ToHex() == curRow.tstamp.ToHex())
                        {
                            Update_SYS_RibbonTab(selectRow, curRow, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        selectRow = new SYS_RibbonTab();
                        Update_SYS_RibbonTab(selectRow, curRow, true);
                        _db.SYS_RibbonTab.AddObject(selectRow);
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
        private void Update_SYS_RibbonTab(SYS_RibbonTab t, SA01900_pgSYS_RibbonTab_Result s, bool isNew)
        {
            if (isNew)
            {
                t.TabID = s.TabID;
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

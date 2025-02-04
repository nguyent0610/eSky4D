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
namespace SA02100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA02100Controller : Controller
    {
        private string _screenNbr = "SA02100";
        private string _userName = Current.UserName;
        SA02100Entities _db = Util.CreateObjectContext<SA02100Entities>(true);
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

        public ActionResult GetSYS_RibbonScreen()
        {
            return this.Store(_db.SA02100_pgSYS_RibbonScreen().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_RibbonScreen"]);
                ChangeRecords<SA02100_pgSYS_RibbonScreen_Result> lstSYS_RibbonScreen = dataHandler.BatchObjectData<SA02100_pgSYS_RibbonScreen_Result>();

                lstSYS_RibbonScreen.Created.AddRange(lstSYS_RibbonScreen.Updated);

                foreach (SA02100_pgSYS_RibbonScreen_Result deleted in lstSYS_RibbonScreen.Deleted)
                {
                    if (lstSYS_RibbonScreen.Created.Where(p => p.ScreenNumber == deleted.ScreenNumber
                                                            && p.TabID == deleted.TabID
                                                            && p.GroupID == deleted.GroupID).Count() > 0)
                    {
                        lstSYS_RibbonScreen.Created.Where(p => p.ScreenNumber == deleted.ScreenNumber
                                                            && p.TabID == deleted.TabID
                                                            && p.GroupID == deleted.GroupID).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SYS_RibbonScreen.FirstOrDefault(p => p.ScreenNumber == deleted.ScreenNumber
                                                            && p.TabID == deleted.TabID
                                                            && p.GroupID == deleted.GroupID);
                        if (objDel != null)
                        {
                            _db.SYS_RibbonScreen.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SA02100_pgSYS_RibbonScreen_Result curLang in lstSYS_RibbonScreen.Created)
                {
                    if (curLang.ScreenNumber.PassNull() == "" || curLang.GroupID.PassNull() == "" || curLang.TabID.PassNull() == "") continue;

                    var lang = _db.SYS_RibbonScreen.Where(p => p.ScreenNumber == curLang.ScreenNumber
                                                            && p.TabID== curLang.TabID 
                                                            && p.GroupID==curLang.GroupID).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Language(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_RibbonScreen();
                        lang.ResetET();
                        Update_Language(lang, curLang, true);
                        _db.SYS_RibbonScreen.AddObject(lang);
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


        private void Update_Language(SYS_RibbonScreen t, SA02100_pgSYS_RibbonScreen_Result s, bool isNew)
        {
            if (isNew)
            {
                t.TabID = s.TabID;
                t.GroupID = s.GroupID;
                t.ScreenNumber = s.ScreenNumber;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
      


        
    }
}

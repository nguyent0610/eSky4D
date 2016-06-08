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
namespace SI20400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20400Controller : Controller
    {
        private string _screenNbr = "SI20400";
        private string _userName = Current.UserName;

        SI20400Entities _db = Util.CreateObjectContext<SI20400Entities>(false);

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

        public ActionResult GetSI_MaterialType()
        {
            return this.Store(_db.SI20400_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_MaterialType"]);
                ChangeRecords<SI20400_pgLoadGrid_Result> lstSI_MaterialType = dataHandler.BatchObjectData<SI20400_pgLoadGrid_Result>();
                foreach (SI20400_pgLoadGrid_Result deleted in lstSI_MaterialType.Deleted)

                {
                    var check_IN_Inventory = _db.IN_Inventory.Where(p => p.MaterialType == deleted.MaterialType).FirstOrDefault();
                    var check_IN_ProductClass = _db.IN_ProductClass.Where(p => p.MaterialType == deleted.MaterialType).FirstOrDefault();
                    if (check_IN_Inventory == null && check_IN_ProductClass == null)
                    {
                        if (lstSI_MaterialType.Created.Where(p => p.MaterialType == deleted.MaterialType).Count() > 0)
                        {
                            lstSI_MaterialType.Created.Where(p => p.MaterialType == deleted.MaterialType).FirstOrDefault().tstamp = deleted.tstamp;
                        }
                        else
                        {
                            var del = _db.SI_MaterialType.Where(p => p.MaterialType == deleted.MaterialType).FirstOrDefault();
                            if (del != null)
                            {
                                _db.SI_MaterialType.DeleteObject(del);
                            }
                        }
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "2016060201");
                    }
                }

                lstSI_MaterialType.Created.AddRange(lstSI_MaterialType.Updated);

                foreach (SI20400_pgLoadGrid_Result curLang in lstSI_MaterialType.Created)
                {
                    if (curLang.MaterialType.PassNull() == "") continue;

                    var lang = _db.SI_MaterialType.Where(p => p.MaterialType.ToLower() == curLang.MaterialType.ToLower()).FirstOrDefault();

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
                        lang = new SI_MaterialType();
                        Update_Language(lang, curLang, true);
                        _db.SI_MaterialType.AddObject(lang);
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

        private void Update_Language(SI_MaterialType t, SI20400_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.MaterialType = s.MaterialType;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.Descr;
            t.Buyer = s.Buyer;


            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

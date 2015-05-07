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
namespace SI20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20100Controller : Controller
    {
        private string _screenNbr = "SI20100";
        private string _userName = Current.UserName;
        SI20100Entities _db = Util.CreateObjectContext<SI20100Entities>(false);

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

        public ActionResult GetIN_Buyer()
        {
            return this.Store(_db.SI20100_pgLoadGrid().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstIN_Buyer"]);
                ChangeRecords<SI20100_pgLoadGrid_Result> lstIN_Buyer = dataHandler.BatchObjectData<SI20100_pgLoadGrid_Result>();
                foreach (SI20100_pgLoadGrid_Result deleted in lstIN_Buyer.Deleted)
                {
                    var del = _db.SI_Buyer.Where(p => p.Buyer == deleted.Buyer).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_Buyer.DeleteObject(del);
                    }
                }

                lstIN_Buyer.Created.AddRange(lstIN_Buyer.Updated);

                foreach (SI20100_pgLoadGrid_Result curLang in lstIN_Buyer.Created)
                {
                    if (curLang.Buyer.PassNull() == "") continue;

                    var lang = _db.SI_Buyer.Where(p => p.Buyer.ToLower() == curLang.Buyer.ToLower()).FirstOrDefault();

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
                        lang = new SI_Buyer();
                        Update_Language(lang, curLang, true);
                        _db.SI_Buyer.AddObject(lang);
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

        private void Update_Language(SI_Buyer t, SI20100_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Buyer = s.Buyer;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.BuyerName = s.BuyerName;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

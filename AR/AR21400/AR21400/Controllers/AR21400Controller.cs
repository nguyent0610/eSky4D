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

namespace AR21400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21400Controller : Controller
    {
        private string _screenNbr = "AR21400";
        private string _userName = Current.UserName;
        AR21400Entities _db = Util.CreateObjectContext<AR21400Entities>(false);

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
        public ActionResult GetData()
        {
            return this.Store(_db.AR21400_pgLoadSellingProducts().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAR_SellingProducts"]);
                ChangeRecords<AR21400_pgLoadSellingProducts_Result> lstAR_SellingProducts = dataHandler.BatchObjectData<AR21400_pgLoadSellingProducts_Result>();
                foreach (AR21400_pgLoadSellingProducts_Result deleted in lstAR_SellingProducts.Deleted)
                {
                    var del = _db.AR_SellingProducts.Where(p => p.Code == deleted.Code).FirstOrDefault();
                    if (del != null)
                    {
                        _db.AR_SellingProducts.DeleteObject(del);
                    }
                }

                lstAR_SellingProducts.Created.AddRange(lstAR_SellingProducts.Updated);

                foreach (AR21400_pgLoadSellingProducts_Result curLang in lstAR_SellingProducts.Created)
                {
                    if (curLang.Code.PassNull() == "") continue;

                    var lang = _db.AR_SellingProducts.Where(p => p.Code.ToLower() == curLang.Code.ToLower()).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new AR_SellingProducts();
                        Update(lang, curLang, true);
                        _db.AR_SellingProducts.AddObject(lang);
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


        private void Update(AR_SellingProducts t, AR21400_pgLoadSellingProducts_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

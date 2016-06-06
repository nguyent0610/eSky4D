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
namespace SI20900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20900Controller : Controller
    {
        private string _screenNbr = "SI20900";
        private string _userName = Current.UserName;
        SI20900Entities _db = Util.CreateObjectContext<SI20900Entities>(false);
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
        public ActionResult GetTaxCat()
        {
            return this.Store(_db.SI20900_pgLoadTaxCat().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstTaxCat"]);
                ChangeRecords<SI20900_pgLoadTaxCat_Result> lstTaxCat = dataHandler.BatchObjectData<SI20900_pgLoadTaxCat_Result>();
                foreach (SI20900_pgLoadTaxCat_Result deleted in lstTaxCat.Deleted)
                {

                    if (lstTaxCat.Created.Where(p => p.CatID == deleted.CatID).Count() > 0)
                    {
                        lstTaxCat.Created.Where(p => p.CatID == deleted.CatID).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.SI_TaxCat.Where(p => p.CatID == deleted.CatID).FirstOrDefault();
                        if (del != null)
                        {
                            _db.SI_TaxCat.DeleteObject(del);
                        }
                    }
                }

                lstTaxCat.Created.AddRange(lstTaxCat.Updated);

                foreach (SI20900_pgLoadTaxCat_Result curTaxCat in lstTaxCat.Created)
                {
                    if (curTaxCat.CatID.PassNull() == "") continue;

                    var TaxCat = _db.SI_TaxCat.Where(p => p.CatID.ToLower() == curTaxCat.CatID.ToLower()).FirstOrDefault();

                    if (TaxCat != null)
                    {
                        if (TaxCat.tstamp.ToHex() == curTaxCat.tstamp.ToHex())
                        {
                            Update_SI_TaxCat(TaxCat, curTaxCat, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        TaxCat = new SI_TaxCat();
                        Update_SI_TaxCat(TaxCat, curTaxCat, true);
                        _db.SI_TaxCat.AddObject(TaxCat);
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

        private void Update_SI_TaxCat(SI_TaxCat t, SI20900_pgLoadTaxCat_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CatID = s.CatID;
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

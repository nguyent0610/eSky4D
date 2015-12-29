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
namespace SI20600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20600Controller : Controller
    {
        private string _screenNbr = "SI20600";
        private string _userName = Current.UserName;
        SI20600Entities _db = Util.CreateObjectContext<SI20600Entities>(false);
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
        public ActionResult GetData()
        {           
            return this.Store(_db.SI20600_pgLoadCountry().ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<SI20600_pgLoadCountry_Result> lstData = dataHandler.BatchObjectData<SI20600_pgLoadCountry_Result>();
                lstData.Created.AddRange(lstData.Updated);
                foreach (SI20600_pgLoadCountry_Result deleted in lstData.Deleted)
                {
                    if (lstData.Created.Where(p => p.CountryID.ToLower() == deleted.CountryID.ToLower()).Count() > 0)
                    {
                        lstData.Created.Where(p => p.CountryID.ToLower() == deleted.CountryID.ToLower()).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.SI_Country.ToList().Where(p => p.CountryID.ToLower() == deleted.CountryID.ToLower()).FirstOrDefault();
                        if (del != null)
                        {
                            _db.SI_Country.DeleteObject(del);
                        }
                    }
                }

             

                foreach (SI20600_pgLoadCountry_Result curCountry in lstData.Created)
                {
                    if (curCountry.CountryID.PassNull() == "") continue;

                    var Country = _db.SI_Country.Where(p => p.CountryID.ToLower() == curCountry.CountryID.ToLower()).FirstOrDefault();

                    if (Country != null)
                    {
                        if (Country.tstamp.ToHex() == curCountry.tstamp.ToHex())
                        {
                            Update_SI_Country(Country, curCountry, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Country = new SI_Country();
                        Update_SI_Country(Country, curCountry, true);
                        _db.SI_Country.AddObject(Country);
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

        private void Update_SI_Country(SI_Country t, SI20600_pgLoadCountry_Result s, bool isNew)
        {
            if (isNew)
            {
                t.CountryID = s.CountryID;
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

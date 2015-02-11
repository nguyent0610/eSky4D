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
namespace SI21700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21700Controller : Controller
    {
        private string _screenNbr = "SI21700";
        private string _userName = Current.UserName;

        SI21700Entities _db = Util.CreateObjectContext<SI21700Entities>(false);
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
        public ActionResult GetDistrict()
        {
            var Districts = _db.SI21700_pgLoadDistrict().ToList();
            return this.Store(Districts);
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_District"]);
                ChangeRecords<SI21700_pgLoadDistrict_Result> lstSI_District = dataHandler.BatchObjectData<SI21700_pgLoadDistrict_Result>();
                foreach (SI21700_pgLoadDistrict_Result deleted in lstSI_District.Deleted)
                {
                    var del = _db.SI_District.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.District == deleted.District).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SI_District.DeleteObject(del);
                    }
                }

                lstSI_District.Created.AddRange(lstSI_District.Updated);

                foreach (SI21700_pgLoadDistrict_Result curDistrict in lstSI_District.Created)
                {
                    if (curDistrict.Country.PassNull() == "" && curDistrict.State.PassNull() == "" && curDistrict.District.PassNull() == "") continue;

                    var District = _db.SI_District.Where(p => p.Country.ToLower() == curDistrict.Country.ToLower() && p.State.ToLower() == curDistrict.State.ToLower() && p.District.ToLower() == curDistrict.District.ToLower()).FirstOrDefault();

                    if (District != null)
                    {
                        if (District.tstamp.ToHex() == curDistrict.tstamp.ToHex())
                        {
                            Update_SI_District(District, curDistrict, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        District = new SI_District();
                        Update_SI_District(District, curDistrict, true);
                        _db.SI_District.AddObject(District);
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
        private void Update_SI_District(SI_District t, SI21700_pgLoadDistrict_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Country = s.Country;
                t.State = s.State;
                t.District = s.District;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Name = s.Name;
         
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

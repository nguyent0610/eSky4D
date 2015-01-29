using eBiz4DWebFrame;
using Ext.Net;
using Ext.Net.MVC;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace SI20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20500Controller : Controller
    {
        string screenNbr = "SI20500";
        SI20500Entities _db = Util.CreateObjectContext<SI20500Entities>(false);

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView(_db.SI_City);
        }

        public ActionResult GetData()
        {
            
            return this.Store(_db.SI_City);
        }
        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<SI_City> lstMsg = dataHandler.BatchObjectData<SI_City>();
            foreach (SI_City deleted in lstMsg.Deleted)
            {
                var del = _db.SI_City.Where(p => p.Country == deleted.Country && p.State == deleted.State && p.City == deleted.City).FirstOrDefault();
                if (del != null)
                {
                    _db.SI_City.DeleteObject(del);

                }

            }
            foreach (SI_City created in lstMsg.Created)
            {
                if (created.Country == "") continue;
                var record = _db.SI_City.Where(p => p.Country == created.Country && p.State == created.State && p.City == created.City).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new SI_City();
                        record.Country = created.Country;
                        record.State = created.State;
                        record.City = created.City;
                        record.Name = created.Name;
                        
                        record.tstamp = new byte[0];
                        
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(created, ref record);
                        _db.SI_City.AddObject(record);

                    }
                    else
                    {
                        var record2 = new SI_City();
                        record2.Country = created.Country;
                        record2.State = created.State;
                        record2.City = created.City;
                        record2.Name = created.Name;

                        record2.Crtd_Datetime = DateTime.Now;
                        record2.Crtd_Prog = screenNbr;
                        record2.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(created, ref record2);
                        _db.SI_City.AddObject(record2);
                        //Message.Show(2000, new string[] { Util.GetLang("Country"), record.Country.ToString() }, null);
                        //return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("Country"), record.Country.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }



            foreach (SI_City updated in lstMsg.Updated)
            {

                var record = _db.SI_City.Where(p => p.Country == updated.Country && p.State == updated.State && p.City == updated.City).FirstOrDefault();


                if (record != null)
                {
                    
                    if (record.tstamp.ToHex() != updated.tstamp.ToHex())
                    {
                        return Json(new { success = false });
                    }
                    UpdatingSYS_ModuleCat(updated, ref record);
                }
                else
                {
                    if (updated.tstamp.ToHex() == "")
                    {
                        record = new SI_City();
                        record.Country = updated.Country;
                        record.State = updated.State;
                        record.City = updated.City;
                        record.Name = updated.Name;
                       
                    
                        record.tstamp = new byte[0];

                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(updated, ref record);
                        _db.SI_City.AddObject(record);
                        
                    }
                    else
                    {

                        return Json(new { success = false });
                    }
                }

            }




            _db.SaveChanges();
            return Json(new { success = true });


        }


        private void UpdatingSYS_ModuleCat(SI_City s, ref SI_City d)
        {

            d.Name = s.Name;
            
            
          
           
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

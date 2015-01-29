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
namespace SI20700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20700Controller : Controller
    {
        string screenNbr = "SI20700";
        SI20700Entities _db = Util.CreateObjectContext<SI20700Entities>(false);

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView(_db.SI_State);
        }

        public ActionResult GetData()
        {
            
            return this.Store(_db.SI_State);
        }
        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<SI_State> lstMsg = dataHandler.BatchObjectData<SI_State>();
            foreach (SI_State deleted in lstMsg.Deleted)
            {
                var del = _db.SI_State.Where(p => p.Country == deleted.Country && p.State == deleted.State).FirstOrDefault();
                if (del != null)
                {
                    _db.SI_State.DeleteObject(del);

                }

            }
            foreach (SI_State created in lstMsg.Created)
            {
                if (created.Country == "") continue;
                var record = _db.SI_State.Where(p => p.Country == created.Country && p.State == created.State).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new SI_State();
                        record.Country = created.Country;
                        record.State = created.State;
                        record.Descr = created.Descr;
                        record.Territory = created.Territory;
                        
                        record.tstamp = new byte[0];
                        
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(created, ref record);
                        _db.SI_State.AddObject(record);

                    }
                    else
                    {
                        var record2 = new SI_State();
                        record2.Country = created.Country;
                        record2.State = created.State;
                        record2.Descr = created.Descr;
                        record2.Territory = created.Territory;

                        record2.Crtd_Datetime = DateTime.Now;
                        record2.Crtd_Prog = screenNbr;
                        record2.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(created, ref record2);
                        _db.SI_State.AddObject(record2);
                        
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



            foreach (SI_State updated in lstMsg.Updated)
            {

                var record = _db.SI_State.Where(p => p.Country == updated.Country && p.State == updated.State).FirstOrDefault();


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
                        record = new SI_State();
                        record.Country = updated.Country;

                        record.State = updated.State;
                        record.Descr = updated.Descr;
                        record.Territory = updated.Territory;
                    
                        record.tstamp = new byte[0];

                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(updated, ref record);
                        _db.SI_State.AddObject(record);
                        
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


        private void UpdatingSYS_ModuleCat(SI_State s, ref SI_State d)
        {

            d.Descr = s.Descr;
            
            
            d.Territory = s.Territory;
           
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

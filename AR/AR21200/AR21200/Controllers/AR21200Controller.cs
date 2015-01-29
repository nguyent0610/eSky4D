using eBiz4DWebFrame;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AR21200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21200Controller : Controller
    {
        string screenNbr = "AR21200";
        AR21200Entities _db = Util.CreateObjectContext<AR21200Entities>(false);

        public ActionResult Index()
        {
            return View(_db.AR_Location);//.OrderBy(x => x.tstamp)
        }

        public ActionResult GetData()
        {
            return this.Store(_db.AR_Location);
        }
        [DirectMethod]
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_Location> lstMsg = dataHandler.BatchObjectData<AR_Location>();
            foreach (AR_Location deleted in lstMsg.Deleted)
            {
                var del = _db.AR_Location.Where(p => p.Location == deleted.Location).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_Location.DeleteObject(del);
                }
              
            }
            foreach (AR_Location created in lstMsg.Created)
            {
                if (created.Location == "") continue;
                var record = _db.AR_Location.Where(p => p.Location == created.Location).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new AR_Location();
                        record.Location = created.Location;
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingAR_Location(created, ref record);
                        _db.AR_Location.AddObject(record);
                    }
                    else
                    {
                        Message.Show(2000, new string[] { Util.GetLang("Location"), record.Location.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("Location"), record.Location.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }

            

            foreach (AR_Location updated in lstMsg.Updated)
            {
                var record = _db.AR_Location.Where(p => p.Location == updated.Location).FirstOrDefault();
                                
                if (record != null)
                {
                    UpdatingAR_Location(updated, ref record);
                }
                else
                {
                    record = new AR_Location();
                    record.Location = updated.Location;
                    record.Crtd_Datetime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;                
                    UpdatingAR_Location(updated, ref record);
                    _db.AR_Location.AddObject(record);
                }
                

            }
            _db.SaveChanges();

            return Json(new { success = true });
              
            }
            catch (Exception ex)
            {

                return Json(new { success = false, errorMsg = ex.ToString()});
                
              
            }
        }

        private void UpdatingAR_Location(AR_Location s, ref AR_Location d)
        {
            d.Descr = s.Descr;                   
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

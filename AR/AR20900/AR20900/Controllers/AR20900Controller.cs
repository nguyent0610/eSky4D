using eBiz4DWebFrame;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AR20900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20900Controller : Controller
    {
        string screenNbr = "AR20900";
        AR20900Entities _db = Util.CreateObjectContext<AR20900Entities>(false);

        public ActionResult Index()
        {
            return View(_db.AR_Territory);
        }

        public ActionResult GetData()
        {
            return this.Store(_db.AR_Territory);
        }
        [DirectMethod]
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_Territory> lstMsg = dataHandler.BatchObjectData<AR_Territory>();
            foreach (AR_Territory deleted in lstMsg.Deleted)
            {
                var del = _db.AR_Territory.Where(p => p.Territory == deleted.Territory).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_Territory.DeleteObject(del);
                }
              
            }
            foreach (AR_Territory created in lstMsg.Created)
            {
                if (created.Territory == "") continue;
                var record = _db.AR_Territory.Where(p => p.Territory == created.Territory).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new AR_Territory();
                        record.Territory = created.Territory;
                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingAR_Territory(created, ref record);
                        _db.AR_Territory.AddObject(record);
                    }
                    else
                    {
                        Message.Show(2000, new string[] { Util.GetLang("Territory"), record.Territory.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("Territory"), record.Territory.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }

            

            foreach (AR_Territory updated in lstMsg.Updated)
            {
                var record = _db.AR_Territory.Where(p => p.Territory == updated.Territory).FirstOrDefault();
                                
                if (record != null)
                {
                    UpdatingAR_Territory(updated, ref record);
                }
                else
                {
                    record = new AR_Territory();
                    record.Territory = updated.Territory;
                    record.Crtd_DateTime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;                
                    UpdatingAR_Territory(updated, ref record);
                    _db.AR_Territory.AddObject(record);
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

        private void UpdatingAR_Territory(AR_Territory s, ref AR_Territory d)
        {
            d.Descr = s.Descr;
                   
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

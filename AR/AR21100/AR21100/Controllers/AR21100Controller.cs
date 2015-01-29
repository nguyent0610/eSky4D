using eBiz4DWebFrame;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AR21100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21100Controller : Controller
    {
        string screenNbr = "AR21100";
        AR21100Entities _db = Util.CreateObjectContext<AR21100Entities>(false);

        public ActionResult Index()
        {
            return View(_db.AR_Channel);//.OrderBy(x => x.tstamp)
        }

        public ActionResult GetData()
        {
            return this.Store(_db.AR_Channel);
        }
        [DirectMethod]
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_Channel> lstMsg = dataHandler.BatchObjectData<AR_Channel>();
            foreach (AR_Channel deleted in lstMsg.Deleted)
            {
                var del = _db.AR_Channel.Where(p => p.Code == deleted.Code).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_Channel.DeleteObject(del);
                }
              
            }
            foreach (AR_Channel created in lstMsg.Created)
            {
                if (created.Code == "") continue;
                var record = _db.AR_Channel.Where(p => p.Code == created.Code).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new AR_Channel();
                        record.Code = created.Code;
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingAR_Channel(created, ref record);
                        _db.AR_Channel.AddObject(record);
                    }
                    else
                    {
                        Message.Show(2000, new string[] { Util.GetLang("Code"), record.Code.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("Code"), record.Code.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }

            

            foreach (AR_Channel updated in lstMsg.Updated)
            {
                var record = _db.AR_Channel.Where(p => p.Code == updated.Code).FirstOrDefault();
                                
                if (record != null)
                {
                    UpdatingAR_Channel(updated, ref record);
                }
                else
                {
                    record = new AR_Channel();
                    record.Code = updated.Code;
                    record.Crtd_Datetime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;                
                    UpdatingAR_Channel(updated, ref record);
                    _db.AR_Channel.AddObject(record);
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

        private void UpdatingAR_Channel(AR_Channel s, ref AR_Channel d)
        {
            d.Descr = s.Descr;                   
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

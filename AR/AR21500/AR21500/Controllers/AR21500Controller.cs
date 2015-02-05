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

namespace AR21500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21500Controller : Controller
    {
        string screenNbr = "AR21500";
        AR21500Entities _db = Util.CreateObjectContext<AR21500Entities>(false);

        public ActionResult Index()
        {

            Util.InitRight(screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetData()
        {
            return this.Store(_db.AR21500_pgLoadDisplayMethod().ToList());
        }
        [DirectMethod]
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_DisplayMethod> lstMsg = dataHandler.BatchObjectData<AR_DisplayMethod>();
            foreach (AR_DisplayMethod deleted in lstMsg.Deleted)
            {
                var del = _db.AR_DisplayMethod.Where(p => p.DispMethod == deleted.DispMethod).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_DisplayMethod.DeleteObject(del);
                }
              
            }
            foreach (AR_DisplayMethod created in lstMsg.Created)
            {
                if (created.DispMethod == "") continue;
                var record = _db.AR_DisplayMethod.Where(p => p.DispMethod == created.DispMethod).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new AR_DisplayMethod();
                        record.DispMethod = created.DispMethod;
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingAR_DisplayMethod(created, ref record);
                        _db.AR_DisplayMethod.AddObject(record);
                    }
                    else
                    {
                        Message.Show(2000, new string[] { Util.GetLang("DispMethod"), record.DispMethod.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("DispMethod"), record.DispMethod.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }

            

            foreach (AR_DisplayMethod updated in lstMsg.Updated)
            {
                var record = _db.AR_DisplayMethod.Where(p => p.DispMethod == updated.DispMethod).FirstOrDefault();
                                
                if (record != null)
                {
                    UpdatingAR_DisplayMethod(updated, ref record);
                }
                else
                {
                    record = new AR_DisplayMethod();
                    record.DispMethod = updated.DispMethod;
                    record.Crtd_Datetime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;                
                    UpdatingAR_DisplayMethod(updated, ref record);
                    _db.AR_DisplayMethod.AddObject(record);
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

        private void UpdatingAR_DisplayMethod(AR_DisplayMethod s, ref AR_DisplayMethod d)
        {
            d.Descr = s.Descr;
            d.Active = s.Active;
            d.Type = s.Type;
            d.Level = s.Level;
            d.Shelf = s.Shelf;
            d.Style = s.Style;
            d.Seq = s.Seq;
            d.Target = s.Target;
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

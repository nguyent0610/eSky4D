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
        string screenNbr = "AR21400";
        AR21400Entities _db = Util.CreateObjectContext<AR21400Entities>(false);

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
            return this.Store(_db.AR21400_pgLoadSellingProducts().ToList());
        }
        [DirectMethod]
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_SellingProducts> lstMsg = dataHandler.BatchObjectData<AR_SellingProducts>();
            foreach (AR_SellingProducts deleted in lstMsg.Deleted)
            {
                var del = _db.AR_SellingProducts.Where(p => p.Code == deleted.Code).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_SellingProducts.DeleteObject(del);
                }
              
            }
            foreach (AR_SellingProducts created in lstMsg.Created)
            {
                if (created.Code == "") continue;
                var record = _db.AR_SellingProducts.Where(p => p.Code == created.Code).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new AR_SellingProducts();
                        record.Code = created.Code;
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingAR_SellingProducts(created, ref record);
                        _db.AR_SellingProducts.AddObject(record);
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

            

            foreach (AR_SellingProducts updated in lstMsg.Updated)
            {
                var record = _db.AR_SellingProducts.Where(p => p.Code == updated.Code).FirstOrDefault();
                                
                if (record != null)
                {
                    UpdatingAR_SellingProducts(updated, ref record);
                }
                else
                {
                    record = new AR_SellingProducts();
                    record.Code = updated.Code;
                    record.Crtd_Datetime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;                
                    UpdatingAR_SellingProducts(updated, ref record);
                    _db.AR_SellingProducts.AddObject(record);
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

        private void UpdatingAR_SellingProducts(AR_SellingProducts s, ref AR_SellingProducts d)
        {
            d.Descr = s.Descr;                   
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

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
namespace SI20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20100Controller : Controller
    {
        string screenNbr = "SI20100";
        SI20100Entities _db = Util.CreateObjectContext<SI20100Entities>(false);

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView(_db.SI_Buyer);
        }

        public ActionResult GetData()
        {
            
            return this.Store(_db.SI_Buyer);
        }
        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<SI_Buyer> lstMsg = dataHandler.BatchObjectData<SI_Buyer>();
            foreach (SI_Buyer deleted in lstMsg.Deleted)
            {
                var del = _db.SI_Buyer.Where(p => p.Buyer == deleted.Buyer).FirstOrDefault();
                if (del != null)
                {
                    _db.SI_Buyer.DeleteObject(del);

                }

            }
            foreach (SI_Buyer created in lstMsg.Created)
            {
                if (created.Buyer == "") continue;
                var record = _db.SI_Buyer.Where(p => p.Buyer == created.Buyer).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new SI_Buyer();
                        record.Buyer = created.Buyer;
                        record.BuyerName = created.BuyerName;
                        
                        record.tstamp = new byte[0];
                        
                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(created, ref record);
                        _db.SI_Buyer.AddObject(record);

                    }
                    else
                    {
                        //var record2 = new SI_Buyer();
                        //record2.Active = created.Active;
                        //record2.ModuleCode = created.ModuleCode;
                        //record2.ModuleID = created.ModuleID;
                        //record2.CatID = created.CatID;
                        //record2.ModuleName = created.ModuleName;

                        //record2.Crtd_DateTime = DateTime.Now;
                        //record2.Crtd_Prog = screenNbr;
                        //record2.Crtd_User = Current.UserName;
                        //UpdatingSYS_ModuleCat(created, ref record2);
                        //_db.SI_Buyer.AddObject(record2);
                        Message.Show(2000, new string[] { Util.GetLang("Buyer"), record.Buyer.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("Buyer"), record.Buyer.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }



            foreach (SI_Buyer updated in lstMsg.Updated)
            {

                var record = _db.SI_Buyer.Where(p => p.Buyer == updated.Buyer).FirstOrDefault();


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
                        record = new SI_Buyer();
                        record.Buyer = updated.Buyer;
                        record.BuyerName = updated.BuyerName;
                    
                        record.tstamp = new byte[0];

                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(updated, ref record);
                        _db.SI_Buyer.AddObject(record);
                        
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


        private void UpdatingSYS_ModuleCat(SI_Buyer s, ref SI_Buyer d)
        {
           
            d.BuyerName = s.BuyerName;
           
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

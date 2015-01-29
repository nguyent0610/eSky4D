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
namespace SI20400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI20400Controller : Controller
    {
        string screenNbr = "SI20400";
        SI20400Entities _db = Util.CreateObjectContext<SI20400Entities>(false);

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView(_db.SI_MaterialType);
        }

        public ActionResult GetData()
        {
            
            return this.Store(_db.SI_MaterialType);
        }
        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<SI_MaterialType> lstMsg = dataHandler.BatchObjectData<SI_MaterialType>();
            foreach (SI_MaterialType deleted in lstMsg.Deleted)
            {
                var del = _db.SI_MaterialType.Where(p => p.MaterialType == deleted.MaterialType).FirstOrDefault();
                if (del != null)
                {
                    _db.SI_MaterialType.DeleteObject(del);

                }

            }
            foreach (SI_MaterialType created in lstMsg.Created)
            {
                if (created.MaterialType == "") continue;
                var record = _db.SI_MaterialType.Where(p => p.MaterialType == created.MaterialType).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new SI_MaterialType();
                        record.MaterialType = created.MaterialType;
                        record.Descr = created.Descr;
                        record.Buyer = created.Buyer;
                        
                        record.tstamp = new byte[0];
                        
                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(created, ref record);
                        _db.SI_MaterialType.AddObject(record);

                    }
                    else
                    {
                        //var record2 = new SI_MaterialType();
                        //record2.Active = created.Active;
                        //record2.ModuleCode = created.ModuleCode;
                        //record2.ModuleID = created.ModuleID;
                        //record2.CatID = created.CatID;
                        //record2.ModuleName = created.ModuleName;

                        //record2.Crtd_DateTime = DateTime.Now;
                        //record2.Crtd_Prog = screenNbr;
                        //record2.Crtd_User = Current.UserName;
                        //UpdatingSYS_ModuleCat(created, ref record2);
                        //_db.SI_MaterialType.AddObject(record2);
                        Message.Show(2000, new string[] { Util.GetLang("MaterialType"), record.MaterialType.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("MaterialType"), record.MaterialType.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }



            foreach (SI_MaterialType updated in lstMsg.Updated)
            {

                var record = _db.SI_MaterialType.Where(p => p.MaterialType == updated.MaterialType).FirstOrDefault();


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
                        record = new SI_MaterialType();
                        record.MaterialType = updated.MaterialType;
                        record.Descr = updated.Descr;
                        record.Buyer = updated.Buyer;
                    
                        record.tstamp = new byte[0];

                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(updated, ref record);
                        _db.SI_MaterialType.AddObject(record);
                        
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


        private void UpdatingSYS_ModuleCat(SI_MaterialType s, ref SI_MaterialType d)
        {

            d.Descr = s.Descr;
            d.Buyer = s.Buyer;
           
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

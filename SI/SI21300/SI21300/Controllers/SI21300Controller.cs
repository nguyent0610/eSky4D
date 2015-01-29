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
namespace SI21300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21300Controller : Controller
    {
        string screenNbr = "SI21300";
        SI21300Entities _db = Util.CreateObjectContext<SI21300Entities>(false);

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView(_db.SI_Carrier);
        }

        public ActionResult GetData()
        {
            
            return this.Store(_db.SI_Carrier);
        }
        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<SI_Carrier> lstMsg = dataHandler.BatchObjectData<SI_Carrier>();
            foreach (SI_Carrier deleted in lstMsg.Deleted)
            {
                var del = _db.SI_Carrier.Where(p => p.CarrierID == deleted.CarrierID).FirstOrDefault();
                if (del != null)
                {
                    _db.SI_Carrier.DeleteObject(del);

                }

            }
            foreach (SI_Carrier created in lstMsg.Created)
            {
                if (created.CarrierID == "") continue;
                var record = _db.SI_Carrier.Where(p => p.CarrierID == created.CarrierID).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new SI_Carrier();
                        record.CarrierID = created.CarrierID;
                        record.Descr = created.Descr;
                        record.CarrierType = created.CarrierType;
                        record.TerritoryID = created.TerritoryID;
                        record.CheckZones = created.CheckZones;
                        record.ShipAccount = created.ShipAccount;
                        record.UOM = created.UOM;
                        record.tstamp = new byte[0];
                        
                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(created, ref record);
                        _db.SI_Carrier.AddObject(record);

                    }
                    else
                    {
                        //var record2 = new SI_Carrier();
                        //record2.Active = created.Active;
                        //record2.ModuleCode = created.ModuleCode;
                        //record2.ModuleID = created.ModuleID;
                        //record2.CatID = created.CatID;
                        //record2.ModuleName = created.ModuleName;

                        //record2.Crtd_DateTime = DateTime.Now;
                        //record2.Crtd_Prog = screenNbr;
                        //record2.Crtd_User = Current.UserName;
                        //UpdatingSYS_ModuleCat(created, ref record2);
                        //_db.SI_Carrier.AddObject(record2);
                        Message.Show(2000, new string[] { Util.GetLang("CarrierID"), record.CarrierID.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("CarrierID"), record.CarrierID.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }



            foreach (SI_Carrier updated in lstMsg.Updated)
            {

                var record = _db.SI_Carrier.Where(p => p.CarrierID == updated.CarrierID).FirstOrDefault();


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
                        record = new SI_Carrier();
                        record.CarrierID = updated.CarrierID;
                        record.Descr = updated.Descr;
                        record.CarrierType = updated.CarrierType;
                        record.TerritoryID = updated.TerritoryID;
                        record.CheckZones = updated.CheckZones;
                        record.ShipAccount = updated.ShipAccount;
                        record.UOM = updated.UOM;
                        record.tstamp = new byte[0];

                        record.Crtd_DateTime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingSYS_ModuleCat(updated, ref record);
                        _db.SI_Carrier.AddObject(record);
                        
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


        private void UpdatingSYS_ModuleCat(SI_Carrier s, ref SI_Carrier d)
        {

            d.Descr = s.Descr;
            d.CarrierType = s.CarrierType;
            d.TerritoryID = s.TerritoryID;
            d.CheckZones = s.CheckZones;
            d.ShipAccount = s.ShipAccount;
            d.UOM = s.UOM;
           
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

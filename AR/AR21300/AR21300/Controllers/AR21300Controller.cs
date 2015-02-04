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
using System.IO;
using System.Text;

namespace AR21300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21300Controller : Controller
    {
        string screenNbr = "AR21300";
        AR21300Entities _db = Util.CreateObjectContext<AR21300Entities>(false);
        public ActionResult Index()
        {
            Util.InitRight(screenNbr);
            return View();
        }
        public PartialViewResult Body()
        {
            return PartialView(_db.AR21300_pgLoadArea().ToList());
        }
        public ActionResult GetData()
        {
            return this.Store(_db.AR21300_pgLoadArea().ToList());
        }
        [DirectMethod]
        [HttpPost]      
        public ActionResult Save(FormCollection data)
        {
            try
            {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<AR_Area> lstMsg = dataHandler.BatchObjectData<AR_Area>();
            foreach (AR_Area deleted in lstMsg.Deleted)
            {
                var del = _db.AR_Area.Where(p => p.Area == deleted.Area).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_Area.DeleteObject(del);
                }
              
            }
            foreach (AR_Area created in lstMsg.Created)
            {
                if (created.Area == "") continue;
                var record = _db.AR_Area.Where(p => p.Area == created.Area).FirstOrDefault();

                if (created.tstamp.ToHex() == "")
                {
                    if (record == null)
                    {
                        record = new AR_Area();
                        record.Area = created.Area;
                        record.Crtd_Datetime = DateTime.Now;
                        record.Crtd_Prog = screenNbr;
                        record.Crtd_User = Current.UserName;
                        UpdatingAR_Area(created, ref record);
                        _db.AR_Area.AddObject(record);
                    }
                    else
                    {
                        Message.Show(2000, new string[] { Util.GetLang("Area"), record.Area.ToString() }, null);
                        return this.Direct();
                        //tra ve loi da ton tai ma ngon ngu nay ko the them
                    }
                }
                else
                {
                    Message.Show(2000, new string[] { Util.GetLang("Area"), record.Area.ToString() }, null);
                    return this.Direct();
                    //tra ve loi da ton tai ma ngon ngu nay ko the them
                }


            }

            

            foreach (AR_Area updated in lstMsg.Updated)
            {
                var record = _db.AR_Area.Where(p => p.Area == updated.Area).FirstOrDefault();
                                
                if (record != null)
                {
                    UpdatingAR_Area(updated, ref record);
                }
                else
                {
                    record = new AR_Area();
                    record.Area = updated.Area;
                    record.Crtd_Datetime = DateTime.Now;
                    record.Crtd_Prog = screenNbr;
                    record.Crtd_User = Current.UserName;                
                    UpdatingAR_Area(updated, ref record);
                    _db.AR_Area.AddObject(record);
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

        private void UpdatingAR_Area(AR_Area s, ref AR_Area d)
        {
            d.Descr = s.Descr;                   
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        [DirectMethod]
        public ActionResult GetDataGrid()
        {
            var lst = _db.AR21300_pgLoadArea().ToList();

            this.GetCmp<TextField>("stoArea").Data = lst;
              

            return this.Direct(lst);
        }
    }
}

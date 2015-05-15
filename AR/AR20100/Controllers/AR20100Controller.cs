using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkySys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace AR20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20100Controller : Controller
    {
        private string _branchID = Current.CpnyID.ToString();
        private string _screenNbr = "AR20100";
        AR20100Entities _db = Util.CreateObjectContext<AR20100Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetARCustClass(string classId)
        {
            var rptSOAddress = _db.AR20100_ppAR_CustClass().Where(x => x.ClassId.PassNull().ToUpper() == classId.PassNull().ToUpper()).FirstOrDefault();
            return this.Store(rptSOAddress);
        }


        [DirectMethod]
        [HttpPost]
       
        public ActionResult Save(FormCollection data, string classId)
        {
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstheader"]);
            ChangeRecords<AR20100_ppAR_CustClass_Result> lstheader = dataHandler.BatchObjectData<AR20100_ppAR_CustClass_Result>();

            foreach (AR20100_ppAR_CustClass_Result updated in lstheader.Updated)
            {                 
                // Get the image path
                string CLASSID = updated.ClassId;
                var objHeader = _db.AR_CustClass.Where(p => p.ClassId == CLASSID).FirstOrDefault();
                if (objHeader != null)
                {                    
                    if (updated.tstamp.ToHex() != objHeader.tstamp.ToHex())
                    {
                        //throw new MessageException(MessageType.Message, "19");
                        return Json(new { success = false, msgCode = 19 });
                    }
                    UpdatingHeader(updated, ref objHeader);
                }
                else
                {
                    objHeader = new AR_CustClass();
                    UpdatingHeader(updated, ref objHeader);
                    
                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = _screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    _db.AR_CustClass.AddObject(objHeader);
                }
            }

            foreach (AR20100_ppAR_CustClass_Result created in lstheader.Created)
            {
                // Get the image path
                classId = created.ClassId.PassNull().ToUpper();
                var objHeader = _db.AR_CustClass.Where(p => p.ClassId.ToUpper() == classId).FirstOrDefault();
                if (objHeader != null)
                {
                    return Json(new { success = false });
                }
                else
                {
                    objHeader = new AR_CustClass();                    
                    UpdatingHeader(created, ref objHeader);
                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = _screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    _db.AR_CustClass.AddObject(objHeader);
                    _db.SaveChanges();
                }
            }
            _db.SaveChanges();
            //this.Direct();
            return Json(new { success = true, ClassId = classId }, JsonRequestBehavior.AllowGet);
        }

        [DirectMethod]
        public ActionResult AR20100Delete(string classId)
        {
            classId = classId.PassNull().ToUpper();
            var soAddress = _db.AR_CustClass.FirstOrDefault(p => p.ClassId.ToUpper() == classId);
            _db.AR_CustClass.DeleteObject(soAddress);
            _db.SaveChanges();
            return this.Direct();
        }

        private void UpdatingHeader(AR20100_ppAR_CustClass_Result s, ref AR_CustClass d)
        {
            d.ClassId = s.ClassId;
            d.City = s.City;
            d.Country = s.Country;
            d.Descr = s.Descr;
            d.District = s.District;
            d.PriceClass = s.PriceClass;
            d.State = s.State;
            d.Terms = s.Terms;
            d.Territory = s.Territory;
            d.TradeDisc = s.TradeDisc;
            d.TaxDflt = s.TaxDflt;
            d.TaxID00 = s.TaxID00;
            d.TaxID01 = s.TaxID01;
            d.TaxID02 = s.TaxID02;
            d.TaxID03 = s.TaxID03;            
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = _screenNbr;
            d.LUpd_User = Current.UserName;
            d.tstamp = new byte[1];
        }

    }
}

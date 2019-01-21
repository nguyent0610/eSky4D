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
        private string _userName = Current.UserName;
        AR20100Entities _db = Util.CreateObjectContext<AR20100Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetAR_CustClass(string classId)
        {
            var rptSOAddress = _db.AR_CustClass.FirstOrDefault(x => x.ClassId == classId);
            return this.Store(rptSOAddress);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string ClassId = data["cboClassId"].PassNull();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstAR_CustClass"]);
                var curHeader = dataHandler.ObjectData<AR_CustClass>().FirstOrDefault();

                var header = _db.AR_CustClass.FirstOrDefault(p => p.ClassId == ClassId);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    //string images = getPathThenUploadImage(curHeader, UserID);
                    header = new AR_CustClass();
                    header.ResetET();
                    header.ClassId = ClassId;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader);
                    _db.AR_CustClass.AddObject(header);
                }



                _db.SaveChanges();
                return Json(new { success = true, ClassId = ClassId });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(ref AR_CustClass t, AR_CustClass s)
        {
            t.City = s.City;
            t.Country = s.Country;
            t.Descr = s.Descr;
            t.District = s.District;
            t.PriceClass = s.PriceClass;
            t.State = s.State;
            t.Terms = s.Terms;
            t.Territory = s.Territory;
            t.TradeDisc = s.TradeDisc;
            t.TaxDflt = s.TaxDflt;
            t.TaxID00 = s.TaxID00;
            t.TaxID01 = s.TaxID01;
            t.TaxID02 = s.TaxID02;
            t.TaxID03 = s.TaxID03;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string ClassId = data["cboClassId"].PassNull();
                var cpny = _db.AR_CustClass.FirstOrDefault(p => p.ClassId == ClassId);
                if (cpny != null)
                {
                    _db.AR_CustClass.DeleteObject(cpny);
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
    }
}

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
namespace AR21000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21000Controller : Controller
    {
        private string _screenNbr = "AR21000";
        private string _userName = Current.UserName;

        AR21000Entities _db = Util.CreateObjectContext<AR21000Entities>(false);

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

        public ActionResult GetShopType()
        {
            return this.Store(_db.AR21000_pgLoadShopType().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstShopType"]);
                ChangeRecords<AR_ShopType> lstShopType = dataHandler.BatchObjectData<AR_ShopType>();

                lstShopType.Created.AddRange(lstShopType.Updated);
                foreach (AR_ShopType del in lstShopType.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstShopType.Created.Where(p => p.Code == del.Code).Count() > 0)
                    {
                        lstShopType.Created.Where(p => p.Code == del.Code).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.AR_ShopType.ToList().Where(p => p.Code == del.Code).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.AR_ShopType.DeleteObject(objDel);
                        }
                    }
                }
                foreach (AR_ShopType curShopType in lstShopType.Created)
                {
                    if (curShopType.Code.PassNull() == "") continue;

                    var ShopType = _db.AR_ShopType.Where(p => p.Code.ToLower() == curShopType.Code.ToLower()).FirstOrDefault();

                    if (ShopType != null)
                    {
                        if (ShopType.tstamp.ToHex() == curShopType.tstamp.ToHex())
                        {
                            Update_AR_ShopType(ShopType, curShopType, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        ShopType = new AR_ShopType();
                        Update_AR_ShopType(ShopType, curShopType, true);
                        _db.AR_ShopType.AddObject(ShopType);
                    }
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

        private void Update_AR_ShopType(AR_ShopType t, AR_ShopType s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

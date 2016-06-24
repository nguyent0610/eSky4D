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
namespace CA20300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class CA20300Controller : Controller
    {
        private string _screenNbr = "CA20300";
        private string _userName = Current.UserName;

        CA20300Entities _db = Util.CreateObjectContext<CA20300Entities>(false);

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

       public ActionResult GetCostType()
        {
            return this.Store(_db.CA20300_pgLoadCostType().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstCostType"]);
                ChangeRecords<CA_CostType> lstCostType = dataHandler.BatchObjectData<CA_CostType>();
                //foreach (CA_CostType deleted in lstCostType.Deleted)
                //{
                //    var del = _db.CA_CostType.Where(p => p.TypeID == deleted.TypeID).FirstOrDefault();
                //    if (del != null)
                //    {
                //        _db.CA_CostType.DeleteObject(del);
                //    }
                //}

                //lstCostType.Created.AddRange(lstCostType.Updated);
                lstCostType.Created.AddRange(lstCostType.Updated);
                foreach (CA_CostType del in lstCostType.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstCostType.Created.Where(p => p.TypeID == del.TypeID).Count() > 0)
                    {
                        lstCostType.Created.Where(p => p.TypeID == del.TypeID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.CA_CostType.ToList().Where(p => p.TypeID == del.TypeID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.CA_CostType.DeleteObject(objDel);
                        }
                    }
                }
                foreach (CA_CostType curCostType in lstCostType.Created)
                {
                    if (curCostType.TypeID.PassNull() == "") continue;

                    var CostType = _db.CA_CostType.Where(p => p.TypeID.ToLower() == curCostType.TypeID.ToLower()).FirstOrDefault();

                    if (CostType != null)
                    {
                        if (CostType.tstamp.ToHex() == curCostType.tstamp.ToHex())
                        {
                            Update_CA_CostType(CostType, curCostType, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        CostType = new CA_CostType();
                        Update_CA_CostType(CostType, curCostType, true);
                        _db.CA_CostType.AddObject(CostType);
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

        private void Update_CA_CostType(CA_CostType t, CA_CostType s, bool isNew)
        {
            if (isNew)
            {
                t.TypeID = s.TypeID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

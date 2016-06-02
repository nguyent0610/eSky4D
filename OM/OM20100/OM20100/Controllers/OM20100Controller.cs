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
namespace OM20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20100Controller : Controller
    {
        private string _screenNbr = "OM20100";
        private string _userName = Current.UserName;
        OM20100Entities _db = Util.CreateObjectContext<OM20100Entities>(false);
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
        public ActionResult GetOM_PriceClass()
        {           
            return this.Store(_db.OM20100_pgPriceClass().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_PriceClass"]);
                ChangeRecords<OM20100_pgPriceClass_Result> lstOM_PriceClass = dataHandler.BatchObjectData<OM20100_pgPriceClass_Result>();
          
                lstOM_PriceClass.Created.AddRange(lstOM_PriceClass.Updated);
                foreach (OM20100_pgPriceClass_Result del in lstOM_PriceClass.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstOM_PriceClass.Created.Where(p => p.PriceClassID == del.PriceClassID).Count() > 0)
                    {
                        lstOM_PriceClass.Created.Where(p => p.PriceClassID == del.PriceClassID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_PriceClass.ToList().Where(p => p.PriceClassID == del.PriceClassID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_PriceClass.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM20100_pgPriceClass_Result curLang in lstOM_PriceClass.Created)
                {
                    if (curLang.PriceClassID.PassNull() == "") continue;

                    var lang = _db.OM_PriceClass.FirstOrDefault(p => p.PriceClassID.ToLower() == curLang.PriceClassID.ToLower() && p.PriceClassType == "I");

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Language(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_PriceClass();
                        Update_Language(lang, curLang, true);
                        _db.OM_PriceClass.AddObject(lang);
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
        private void Update_Language(OM_PriceClass t, OM20100_pgPriceClass_Result s, bool isNew)
        {
            if (isNew)
            {
                t.PriceClassID = s.PriceClassID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.PriceClassType = "I";
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

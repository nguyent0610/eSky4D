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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;

namespace OM23300.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23300Controller : Controller
    {
        private string _screenNbr = "OM23300";
        private string _userName = Current.UserName;
        OM23300Entities _db = Util.CreateObjectContext<OM23300Entities>(false);
        private JsonResult _logMessage;
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

        public ActionResult GetOM_POSMStructure(string PosmID)
        {
            return this.Store(_db.OM23300_pgOM_POSMStructure(PosmID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string PosmID = data["cboPosmID"].PassNull();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_POSMStructure"]);
                ChangeRecords<OM23300_pgOM_POSMStructure_Result> lstOM_POSMStructure = dataHandler.BatchObjectData<OM23300_pgOM_POSMStructure_Result>();

                lstOM_POSMStructure.Created.AddRange(lstOM_POSMStructure.Updated);

                foreach (OM23300_pgOM_POSMStructure_Result del in lstOM_POSMStructure.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstOM_POSMStructure.Created.Where(p => p.PosmID == PosmID && p.InvtID == del.InvtID).Count() > 0)
                    {
                        lstOM_POSMStructure.Created.Where(p => p.PosmID == PosmID && p.InvtID == del.InvtID).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_POSMStructure.ToList().Where(p => p.PosmID == PosmID && p.InvtID == del.InvtID).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_POSMStructure.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM23300_pgOM_POSMStructure_Result curLang in lstOM_POSMStructure.Created)
                {
                    if (PosmID.PassNull() == "" || curLang.InvtID.PassNull() == "") continue;

                    var lang = _db.OM_POSMStructure.Where(p => p.PosmID.ToLower() == PosmID.ToLower()
                                                            && p.InvtID.ToLower() == curLang.InvtID.ToLower()).FirstOrDefault();

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
                        lang = new OM_POSMStructure();
                        lang.ResetET();
                        lang.PosmID = PosmID;
                        Update_Language(lang, curLang, true);
                        _db.OM_POSMStructure.AddObject(lang);
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true, PosmID = PosmID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_Language(OM_POSMStructure t, OM23300_pgOM_POSMStructure_Result s, bool isNew)
        {
            if (isNew)
            {
                t.InvtID = s.InvtID;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Structure = s.Structure;
            t.Descr = s.Descr;
            t.SlsPrice = s.SlsPrice;
            t.UnitCost = s.UnitCost;
            t.CnvFact = s.CnvFact;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string PosmID = data["cboPosmID"].PassNull();

                var lst = _db.OM_POSMStructure.Where(p => p.PosmID == PosmID).ToList();
                foreach (var item in lst)
                {
                    _db.OM_POSMStructure.DeleteObject(item);
                }

                _db.SaveChanges();
                return Json(new { success = true, PosmID = PosmID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

    }
}

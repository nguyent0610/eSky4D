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
namespace SA01100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA01100Controller : Controller
    {
        private string _screenNbr = "SA01100";
        private string _userName = Current.UserName;
        SA01100Entities _db = Util.CreateObjectContext<SA01100Entities>(true);
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

        public ActionResult GetSYS_Message()
        {
            _db.CommandTimeout = int.MaxValue;
            return this.Store(_db.SA01100_pgSYS_Message().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Message"]);
                ChangeRecords<SA01100_pgSYS_Message_Result> lstSYS_Message = dataHandler.BatchObjectData<SA01100_pgSYS_Message_Result>();
      
                lstSYS_Message.Created.AddRange(lstSYS_Message.Updated);

                foreach (SA01100_pgSYS_Message_Result deleted in lstSYS_Message.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (lstSYS_Message.Created.Where(p => p.Code == deleted.Code).Count() > 0)
                    {
                        lstSYS_Message.Created.Where(p => p.Code == deleted.Code).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SYS_Message.FirstOrDefault(p => p.Code == deleted.Code);
                        if (objDel != null)
                        {
                            _db.SYS_Message.DeleteObject(objDel);
                        }
                    }
                }


                foreach (SA01100_pgSYS_Message_Result curLang in lstSYS_Message.Created)
                {
                    if (curLang.Code.PassNull() == "" || curLang.Code == 0) continue;

                    var lang = _db.SYS_Message.FirstOrDefault(p => p.Code == curLang.Code);

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdateSYS_Message(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_Message();
                        lang.ResetET();
                        UpdateSYS_Message(lang, curLang, true);
                        _db.SYS_Message.AddObject(lang);
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
        private void UpdateSYS_Message(SYS_Message t, SA01100_pgSYS_Message_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Type = s.Type.ToShort();
            t.Title00 = s.Title00;
            t.Title01 = s.Title01;
            t.Title02 = s.Title02;
            t.Title03 = s.Title03;
            t.Title04 = s.Title04;

            t.Msg00 = s.Msg00;
            t.Msg01 = s.Msg01;
            t.Msg02 = s.Msg02;
            t.Msg03 = s.Msg03;
            t.Msg04 = s.Msg04;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }   
    }
}

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
namespace SA00900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA00900Controller : Controller
    {
        private string _screenNbr = "SA00900";
        private string _userName = Current.UserName;
        SA00900Entities _db = Util.CreateObjectContext<SA00900Entities>(true);
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
        public ActionResult GetSYS_Language()
        {           
            return this.Store(_db.SA00900_pgLoadSYS_Language().ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_Language"]);
                ChangeRecords<SA00900_pgLoadSYS_Language_Result> lstLang = dataHandler.BatchObjectData<SA00900_pgLoadSYS_Language_Result>();
                foreach (SA00900_pgLoadSYS_Language_Result deleted in lstLang.Deleted)
                {
                    var del = _db.SYS_Language.Where(p => p.Code == deleted.Code).FirstOrDefault();
                    if (del != null)
                    {
                        _db.SYS_Language.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (SA00900_pgLoadSYS_Language_Result curLang in lstLang.Created)
                {
                    if (curLang.Code.PassNull() == "") continue;

                    var lang = _db.SYS_Language.Where(p => p.Code.ToLower() == curLang.Code.ToLower()).FirstOrDefault();

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
                        lang = new SYS_Language();
                        Update_Language(lang, curLang, true);
                        _db.SYS_Language.AddObject(lang);
                    }
                }

                _db.SaveChanges();
                ExportLangJS(0);
                ExportLangJS(1);
                ExportLangJS(2);
                ExportLangJS(3);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void Update_Language(SYS_Language t, SA00900_pgLoadSYS_Language_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Lang00 = s.Lang00;
            t.Lang01 = s.Lang01;
            t.Lang02 = s.Lang02;
            t.Lang03 = s.Lang03;
            t.Lang04 = s.Lang04;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        private void ExportLangJS(short LangID)
        {
            System.IO.TextWriter writeFile = new StreamWriter(Server.MapPath("~\\Scripts\\hq.language" + LangID + ".js"));
            try
            {

                StringBuilder sb = new StringBuilder();
                var lst = _db.SA00900_GetLangJs(LangID).ToList();
                sb.Append("var HQLang = {");

                for (int i = 0; i < lst.Count() - 1; i++)
                {

                    sb.Append("\""+lst[i].Code + "\":\"" + lst[i].Lang + "\",");

                }
                sb.Append("\"" + lst[lst.Count() - 1].Code + "\":\"" + lst[lst.Count() - 1].Lang + "\"};");

                writeFile.Write(sb.ToString());

                //writeFile.Close();
                //writeFile = null;

            }
            catch (Exception ex)
            {
                writeFile.Close();
                writeFile = null;

            }
            finally
            {
                writeFile.Close();
                writeFile = null;
            }



        }
    }
}

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
namespace AP20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AP20100Controller : Controller
    {
        private string _screenNbr = "AP20100";
        private string _userName = Current.UserName;

        AP20100Entities _db = Util.CreateObjectContext<AP20100Entities>(false);

        public ActionResult Index()
        {
            
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult GetLanguage()
        {
          //  var lang = _db.AP20100_pgGetVendClass.ToList();
            return this.Store(_db.AP20100_pgGetVendClass().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstLanguage"]);
                ChangeRecords<AP20100_pgGetVendClass_Result> lstLang = dataHandler.BatchObjectData<AP20100_pgGetVendClass_Result>();
                foreach (AP20100_pgGetVendClass_Result deleted in lstLang.Deleted)
                {
                    var del = _db.AP_VendClass.Where(p => p.ClassID == deleted.ClassID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.AP_VendClass.DeleteObject(del);
                    }
                }

                lstLang.Created.AddRange(lstLang.Updated);

                foreach (AP20100_pgGetVendClass_Result curLang in lstLang.Created)
                {
                    if (curLang.ClassID.PassNull() == "") continue;

                    var lang = _db.AP_VendClass.Where(p => p.ClassID.ToLower() == curLang.ClassID.ToLower()).FirstOrDefault();

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
                        lang = new AP_VendClass();
                        Update_Language(lang, curLang, true);
                        _db.AP_VendClass.AddObject(lang);
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

        private void Update_Language(AP_VendClass t, AP20100_pgGetVendClass_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ClassID = s.ClassID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Terms = s.Terms;


            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        private void ExportLangJS(short LangID)
        {
            //System.IO.TextWriter writeFile = new StreamWriter(Server.MapPath("~\\Scripts\\hq.language" + LangID + ".js"));
            //try
            //{

            //    StringBuilder sb = new StringBuilder();
            //    var lst = _db.AP20100_GetLangJs(LangID).ToList();
            //    sb.Append("var HQLang = {");

            //    for (int i = 0; i < lst.Count() - 1; i++)
            //    {

            //        sb.Append("\""+lst[i].Code + "\":\"" + lst[i].Lang + "\",");

            //    }
            //    sb.Append("\"" + lst[lst.Count() - 1].Code + "\":\"" + lst[lst.Count() - 1].Lang + "\"};");

            //    writeFile.Write(sb.ToString());

            //    //writeFile.Close();
            //    //writeFile = null;

            //}
            //catch (Exception ex)
            //{
            //    writeFile.Close();
            //    writeFile = null;

            //}
            //finally
            //{
            //    writeFile.Close();
            //    writeFile = null;
            //}



        }
    }
}

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
namespace OM21600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21600Controller : Controller
    {
        private string _screenNbr = "OM21600";
        private string _userName = Current.UserName;
        OM21600Entities _db = Util.CreateObjectContext<OM21600Entities>(false);

        public ActionResult Index()
        {
            bool allowRouteType = false
                , requiredRouteType = false;


            var objConfig = _db.OM21600_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                allowRouteType = objConfig.AllowRouteType.HasValue ? objConfig.AllowRouteType.Value : false;
                requiredRouteType = objConfig.RequiredRouteType.HasValue ? objConfig.RequiredRouteType.Value : false;
            }

            ViewBag.allowRouteType = allowRouteType;
            ViewBag.requiredRouteType = requiredRouteType;
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }



        public ActionResult GetSalesRoute(string CpnyID, string Territory)
        {
            return this.Store(_db.OM21600_pgLoadSalesRoute(_userName,Current.CpnyID,Current.LangID,Territory,CpnyID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string BranchID = data["cboCnpyID"].PassNull();
                 
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_SalesRoute"]);
                ChangeRecords<OM21600_pgLoadSalesRoute_Result> lstOM_SalesRoute = dataHandler.BatchObjectData<OM21600_pgLoadSalesRoute_Result>();
                foreach (OM21600_pgLoadSalesRoute_Result deleted in lstOM_SalesRoute.Deleted)
                {
                    var del = _db.OM_SalesRoute.Where(p => p.SalesRouteID == deleted.SalesRouteID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.OM_SalesRoute.DeleteObject(del);
                    }
                }

                lstOM_SalesRoute.Created.AddRange(lstOM_SalesRoute.Updated);
                string erorrSave = string.Empty;
                foreach (OM21600_pgLoadSalesRoute_Result curLang in lstOM_SalesRoute.Created)
                {
                    if (curLang.SalesRouteID.PassNull() == "" || BranchID.PassNull()=="") continue;

                    var lang = _db.OM_SalesRoute.Where(p => p.SalesRouteID.ToLower() == curLang.SalesRouteID.ToLower()).FirstOrDefault();


                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            Update_Language(lang, curLang, false);
                        }
                        else
                        {

                            //throw new MessageException(MessageType.Message, "19");
                            erorrSave = erorrSave + lang.SalesRouteID + ",";
                        }
                    }
                    else
                    {
                        lang = new OM_SalesRoute();
                       // lang.BranchID = BranchID;
                        Update_Language(lang, curLang, true);
                        _db.OM_SalesRoute.AddObject(lang);
                    }
                }
                if (erorrSave != string.Empty)
                {
                    string message = string.Format(Message.GetString("2017122111", null), erorrSave);
                    throw new MessageException(MessageType.Message, "20410","", new string[] { message });
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

        private void Update_Language(OM_SalesRoute t, OM21600_pgLoadSalesRoute_Result s, bool isNew)
        {
            if (isNew)
            {
                t.SalesRouteID = s.SalesRouteID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }

            t.Descr = s.DescrSales;
            t.BranchID = s.BranchRouteID;
            t.RouteType = s.RouteType;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [HttpPost]
        public ActionResult CheckDelete(FormCollection data)
        {
            try
            {
                string lstIndexRow = data["lstIndexColum"];
                string lstDataCheck = data["lstCheck"];
                string errorDelete = "";
                string rowError = "";
                int key = 0;
                var lstCheckDelete = _db.OM21600_ppCheckDelete(Current.UserName, Current.CpnyID, Current.LangID, lstDataCheck).ToList();


                string check = lstDataCheck;

                //string dt = "1,subid01;3,subid03;";
                string[] lstDelete = check.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] lstRow = lstIndexRow.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lstDelete.Count(); i++)
                {
                    var tam = lstCheckDelete.Where(p => p.SalesRouteID.ToUpper().Trim() == lstDelete[i].Trim().ToUpper()).FirstOrDefault();
                    if (tam != null)
                    {
                        errorDelete = errorDelete + lstDelete[i] + ",";
                        rowError = rowError + lstRow[i] + ",";
                        key = 1;
                    }
                }
                if (key == 0)
                {
                    return Json(new { success = true });
                }
                else
                {
                    string message = string.Format(Message.GetString("2017122015", null), errorDelete, rowError);
                    throw new MessageException(MessageType.Message, "20410", "", new string[] { message });
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
    }
}

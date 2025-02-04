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
namespace IN40500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN40500Controller : Controller
    {
        private string _screenNbr = "IN40500";
        private string _userName = Current.UserName;
        IN40500Entities _db = Util.CreateObjectContext<IN40500Entities>(false);

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

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            try
            {
                string BranchID = data["cboBranchID"].PassNull();
                string Date_tmp = data["dtpTranDate"].PassNull();
                string Descr = data["txtDescr"].PassNull();
                string SiteID = data["cboSiteID"].PassNull();
                string ClassID=data["cboProductClass"].PassNull();
                var CheckCreateIN_Tag = _db.IN40500_ppCheckCreateIN_Tag(BranchID, SiteID).FirstOrDefault();
                if (CheckCreateIN_Tag != null)
                {
                    if (CheckCreateIN_Tag.Result == "")
                    {
                        var result = _db.IN40500_ppGetInsertIN_TagDetail(_userName, BranchID, Descr, DateTime.Parse(Date_tmp), SiteID, ClassID).FirstOrDefault();
                        if (result != null)
                        {
                            return Json(new { success = true, TAGID = result.Result });
                            //throw new MessageException(MessageType.Message, "20403", "afterCreate" ,new string [] {result.Result},"", TAGID = );
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "201405301");
                        }
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "2016092910", "", parm: new string[] { CheckCreateIN_Tag.Result.TrimEnd(',') });
                        //throw new MessageException(MessageType.Message, "1001");
                    }
                    //throw new MessageException(MessageType.Message, "19");
                }
                return Json(new { success = true , TAGID = "" });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }



    }
}

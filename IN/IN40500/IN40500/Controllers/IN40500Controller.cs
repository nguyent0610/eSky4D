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

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {

            try
            {
                string BranchID = data["txtBranchID"].PassNull();
                string Date_tmp = data["dtpTranDate"].PassNull();
                string Descr = data["txtDescr"].PassNull();
                string SiteID = data["cboSiteID"].PassNull();
                string ClassID=data["cboProductClass"].PassNull();
                var CheckCreateIN_Tag = _db.IN40500_ppCheckCreateIN_Tag(BranchID, SiteID).FirstOrDefault().Value;

                if (CheckCreateIN_Tag == 0)
                {
                    var result=_db.IN40500_ppGetInsertIN_TagDetail(_userName,BranchID,Descr,DateTime.Parse(Date_tmp),SiteID,ClassID);
                    if (result.PassNull() != "")
                    {
                        throw new MessageException(MessageType.Message, "20403");
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "201405301");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "1001");
                }
                    //throw new MessageException(MessageType.Message, "19");
                
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

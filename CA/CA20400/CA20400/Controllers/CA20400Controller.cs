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
namespace CA20400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class CA20400Controller : Controller
    {
        private string _screenNbr = "CA20400";
        private string _userName = Current.UserName;

        CA20400Entities _db = Util.CreateObjectContext<CA20400Entities>(false);

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

       public ActionResult GetCostCode()
        {
            return this.Store(_db.CA20400_pgLoadCostCode().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstCostCode"]);
                ChangeRecords<CA_CostCode> lstCostCode = dataHandler.BatchObjectData<CA_CostCode>();
                foreach (CA_CostCode deleted in lstCostCode.Deleted)
                {
                    var del = _db.CA_CostCode.Where(p => p.CostID == deleted.CostID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.CA_CostCode.DeleteObject(del);
                    }
                }

                lstCostCode.Created.AddRange(lstCostCode.Updated);

                foreach (CA_CostCode curCostCode in lstCostCode.Created)
                {
                    if (curCostCode.CostID.PassNull() == "") continue;

                    var CostCode = _db.CA_CostCode.Where(p => p.CostID.ToLower() == curCostCode.CostID.ToLower()).FirstOrDefault();

                    if (CostCode != null)
                    {
                        if (CostCode.tstamp.ToHex() == curCostCode.tstamp.ToHex())
                        {
                            Update_CA_CostCode(CostCode, curCostCode, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        CostCode = new CA_CostCode();
                        Update_CA_CostCode(CostCode, curCostCode, true);
                        _db.CA_CostCode.AddObject(CostCode);
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

        private void Update_CA_CostCode(CA_CostCode t, CA_CostCode s, bool isNew)
        {
            if (isNew)
            {
                t.CostID = s.CostID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Type = s.Type;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

    }
}

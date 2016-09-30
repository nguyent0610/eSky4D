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

namespace OM22400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22400Controller : Controller
    {
        private string _screenNbr = "OM22400";
        private string _userName = Current.UserName;
        OM22400Entities _db = Util.CreateObjectContext<OM22400Entities>(false);
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

        public ActionResult GetDataGrid(string BranchID, string PJPID, string SlsPerID, string SalesRouteID)
        {
            return this.Store(_db.OM22400_pgLoadGrid(BranchID, PJPID, SlsPerID, SalesRouteID).ToList());
        }

        [HttpPost]
        public ActionResult Process(FormCollection data)
        {
            try
            {
                string BranchID = data["cboBranchID"].PassNull();
                string PJPID = data["cboPJPID"].PassNull();
                string SlsPerID = data["cboSalesMan"].PassNull();
                string SalesRouteID = data["cboRouteID"].PassNull();
                string BranchID1 = data["cboBranchID1"].PassNull();
                string PJPID1 = data["cboPJPID1"].PassNull();
                string SlsPerID1 = data["cboSalesMan1"].PassNull();
                string SalesRouteID1 = data["cboRouteID1"].PassNull();

                DateTime dtNow = DateTime.Now;
                var dataHandler = new StoreDataHandler(data["lstData"]);
                var lstData = dataHandler.ObjectData<OM22400_pgLoadGrid_Result>().Where(p => p.Selected == true).ToList();
                var strCust = string.Empty;

                bool Flag = false;
                foreach (OM22400_pgLoadGrid_Result record in lstData)
                {
                    if (record.Selected == true)
                    {
                        if (Flag == false)
                        {
                            if (record.BranchID != BranchID || record.PJPID != PJPID || record.SlsPerID != SlsPerID || record.SalesRouteID != SalesRouteID)
                            {
                                throw new MessageException(MessageType.Message, "2016091610");
                            }
                            Flag = true;
                        }
                        strCust += record.CustID.PassNull() + ",";
                    }
                }
                if (strCust != "" && strCust != string.Empty)
                {
                    _db.OM22400_ppData(_userName, BranchID, PJPID, SlsPerID, SalesRouteID, BranchID1, PJPID1, SlsPerID1, SalesRouteID1, strCust.TrimEnd(','));
                }
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

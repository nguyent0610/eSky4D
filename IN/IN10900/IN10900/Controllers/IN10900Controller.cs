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
using System.Text.RegularExpressions;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using HQFramework.Common;
using System.Data.SqlClient;
namespace IN10900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN10900Controller : Controller
    {
        private string _screenNbr = "IN10900";
        private string _userName = Current.UserName;
        private static readonly Regex boxNumberRegex = new Regex(@"^\d{2}/\d{4}$");
      //  private JsonResult _logMessage;
        IN10900Entities _db = Util.CreateObjectContext<IN10900Entities>(false);

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
        public ActionResult GetData(string CpnyID, string SlsperId,DateTime FromDate,DateTime ToDate)
        {
            return this.Store(_db.IN10900_pgLoadGrid(CpnyID, SlsperId, FromDate, ToDate));
        }
        public ActionResult Save(FormCollection data)
        {
			try
			{
				StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstData"]);
				ChangeRecords<IN10900_pgLoadGrid_Result> lstData = dataHandlerGrid.BatchObjectData<IN10900_pgLoadGrid_Result>();
				var docDate = data["dteCheckDate"];
				var branchID = data["cboCpnyID"];
				if (_db.IN10900_ppCheckCloseDate(branchID.PassNull(), docDate.ToDateShort(), "IN10900").FirstOrDefault() == "0")
				{
					throw new MessageException(MessageType.Message, "301");
					return new MessageException(MessageType.Message, "301").ToMessage();
				}
				lstData.Created.AddRange(lstData.Updated);
				foreach (IN10900_pgLoadGrid_Result curItem in lstData.Created.Where(p => p.Selected == true))
				{
					if (curItem.Selected == false)
					{
						continue;
					}

					var objStockOutlet = _db.PPC_StockOutlet.Where(p => p.BranchID.ToLower() == branchID.ToLower()
						&& p.SlsPerID.ToLower() == curItem.SlsPerID.ToLower()
						&& p.StkOutNbr.ToLower() == curItem.StkOutNbr.ToLower()
					   ).FirstOrDefault();
					if (objStockOutlet != null)
					{
						objStockOutlet.StkOutDate = docDate.ToDateShort();
						objStockOutlet.LUpd_DateTime = DateTime.Now;
						objStockOutlet.LUpd_Prog = _screenNbr;
						objStockOutlet.LUpd_User = Current.UserName;
					}
					var objin_trans = _db.IN_Trans.Where(p => p.BranchID.ToLower() == branchID.ToLower() && p.BatNbr.ToLower() == curItem.StkOutNbr.ToLower()).ToList();

					if (objin_trans.Count() > 0)
					{
						foreach (var a in objin_trans)
						{
							a.TranDate = docDate.ToDateShort();
							a.LUpd_DateTime = DateTime.Now;
							a.LUpd_Prog = _screenNbr;
							a.LUpd_User = Current.UserName;
						}
					}

				}

				_db.SaveChanges();

				return Util.CreateMessage(MessageProcess.Save);

			}
			catch (Exception ex)
			{
				if (ex is MessageException) return (ex as MessageException).ToMessage();
				return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
			}
        }

		//private void Update_OM_POSMBranchID(OM_FCS_POSM t, IN10900_pgPosmID_Result s, bool isNew)
		//{
		//	if (isNew)
		//	{
		//		t.BranchID = s.BranchID;
		//		t.SiteID = s.SiteID;
		//		t.Crtd_DateTime = DateTime.Now;
		//		t.Crtd_Prog = _screenNbr;
		//		t.Crtd_User = _userName;
		//	}
           
		//	t.ClassID = s.ClassID;
		//	t.CpnyName = s.CpnyName;
		//	t.Descr = s.Descr;
		//	t.FCS = s.FCS;
            
		//	t.LUpd_DateTime = DateTime.Now;
		//	t.LUpd_Prog = _screenNbr;
		//	t.LUpd_User = _userName;
		//}       

      
        // Verify format
		//public static bool VerifyBoxNumber(string boxNumber)
		//{
		//	return boxNumberRegex.IsMatch(boxNumber);
		//}
      //  #endregion
    }
}

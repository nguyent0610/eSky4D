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
        public ActionResult GetData(string CpnyID, string SlsperId, DateTime? FromDate, DateTime? ToDate, string handleType, DateTime? checkDate)
        {
            return this.Store(_db.IN10900_pgLoadGrid(CpnyID, SlsperId, FromDate ?? DateTime.Now, ToDate, checkDate ?? DateTime.Now, handleType));
        }
        public ActionResult Save(FormCollection data)
        {
			try
			{
				StoreDataHandler dataHandlerGrid = new StoreDataHandler(data["lstData"]);
				ChangeRecords<IN10900_pgLoadGrid_Result> lstData = dataHandlerGrid.BatchObjectData<IN10900_pgLoadGrid_Result>();
				var docDate = data["dteCheckDate"];
				var branchID = data["cboCpnyID"];
                var handle = data["cboHandle"];
		
				lstData.Created.AddRange(lstData.Updated);

			    string arrSlsperID = "";
			    string arrBranchID = "";
			    string temp = "";

			    var checkDate = _db.IN10900_ppCheckCloseDate(Current.CpnyID, docDate.ToDateTime(), _screenNbr).FirstOrDefault();
                if (checkDate != null)
			    {
			        if (checkDate == "0")
			        {
                        throw new MessageException(MessageType.Message, "301");
			        }
			    }

				foreach (IN10900_pgLoadGrid_Result curItem in lstData.Created.Where(p => p.Selected == true))
				{
					if (curItem.Selected == false)
					{
						continue;
					}

                    if (handle == "R")
                    {
                        arrBranchID += branchID + ",";
                        arrSlsperID += curItem.SlsPerID + ",";
                        temp = branchID + "#" + curItem.SlsPerID + "#" + curItem.CustID + "#" + curItem.StkOutNbr + ",";
                        continue;
                    }

					var objStockOutlet = _db.PPC_StockOutlet.FirstOrDefault(p => p.BranchID.ToLower() == branchID.ToLower()
						&& p.SlsPerID.ToLower() == curItem.SlsPerID.ToLower()
						&& p.StkOutNbr.ToLower() == curItem.StkOutNbr.ToLower()
					   );
					if (objStockOutlet != null)
					{
						objStockOutlet.StkOutDate = docDate.ToDateShort();
						objStockOutlet.LUpd_DateTime = DateTime.Now;
						objStockOutlet.LUpd_Prog = _screenNbr;
						objStockOutlet.LUpd_User = Current.UserName;
					}
					if (curItem.StockType.PassNull() == "")
					{
						var objBatch = _db.Batches.Where(p => p.BranchID.ToLower() == branchID.ToLower() && p.BatNbr.ToLower() == curItem.StkOutNbr.ToLower() && p.Module.ToLower() == "in").FirstOrDefault();
						objBatch.DateEnt = docDate.ToDateShort();
						objBatch.LUpd_DateTime = DateTime.Now;
						objBatch.LUpd_Prog = _screenNbr;
						objBatch.LUpd_User = Current.UserName;
					}
					else
					{
						var _objStockOutlet = _db.PPC_StockOutlet.Where(p => p.BranchID.ToLower() == branchID.ToLower() && p.StkOutNbr.ToLower() == curItem.StkOutNbr.ToLower() && p.SlsPerID == curItem.SlsPerID).FirstOrDefault();
						_objStockOutlet.StkOutDate = docDate.ToDateShort();
						_objStockOutlet.LUpd_DateTime = DateTime.Now;
						_objStockOutlet.LUpd_Prog = _screenNbr;
						_objStockOutlet.LUpd_User = Current.UserName;
					}

				}

				_db.SaveChanges();

			    if (arrBranchID != "" || arrSlsperID != "")
			    {
			        Dictionary<string, string> dicData = new Dictionary<string, string>();
			        dicData.Add("@UserName", Current.UserName ?? "");
			        dicData.Add("@CpnyID", Current.CpnyID ?? "");
			        dicData.Add("@LangID", Current.LangID.ToString());
			        dicData.Add("@BranchID", arrBranchID);
			        dicData.Add("@SlsperID", arrSlsperID);
			        dicData.Add("@Temp", temp);
			        dicData.Add("@FromDate",
			            data["dteFromDate"].ToDateTime().ToString("yyyy-MM-dd hh:mm:ss") ??
			            DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
			        dicData.Add("@ToDate",
			            data["dteToDate"].ToDateTime().ToString("yyyy-MM-dd hh:mm:ss") ??
			            DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
			        dicData.Add("@CheckDate",
			            docDate.ToDateTime().ToString("yyyy-MM-dd hh:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
			        dicData.Add("@HandleType", data["cboHandleType"] ?? "");

			        Util.getDataTableFromProc("IN10900_ppRelease", dicData);
			    }

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

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
using HQ.eSkyFramework.HQControl;
using System.Drawing;
namespace IF30100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IF30100Controller : Controller
    {
        private string _screenNbr = "IF30100";
        private string _userName = Current.UserName;
        private string _branchID = "";
        IF30100Entities _db = Util.CreateObjectContext<IF30100Entities>(false);
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

        public ActionResult GetIF30100_pgData(string view)
        {
            return this.Store(_db.IF30100_pgData(view).ToList());
        }

        [HttpPost]
        public ActionResult Export(FormCollection data, string view, string name)
        {
            try
            {
                string select = "";
                string param = "";
                string proc="";
                var detHandler = new StoreDataHandler(data["lstDet"]);
                var lstDet = detHandler.ObjectData<IF30100_pgData_Result>().ToList();
                foreach(var obj in lstDet)
                {
                    select += obj.Checked == true ? obj.Column_Name + "," : "";
                    if (obj.Operator.PassNull() != "")
                    {
                        if (obj.Operator.ToUpper().Trim() == "Between".ToUpper())
                        {
                            param += obj.Column_Name + " Between " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' AND " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value2 + "' AND ";
                        }
                        else if (obj.Operator.ToUpper().Trim() == "AND".ToUpper())
                        {
                            param += obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' AND " + obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value2 + "' AND ";
                        }
                        else if (obj.Operator.ToUpper().Trim() == "OR".ToUpper())
                        {
                            param += obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' OR " + obj.Column_Name + " = " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value2 + "' AND ";
                        }
                        else if (obj.Operator.ToUpper().Trim() == "IN".ToUpper())
                        {

                            param += obj.Column_Name + " IN('"+ obj.Value1.Replace(",","','")+ "') AND ";
                        }
                        else param += obj.Column_Name + " " + obj.Operator + " " + (obj.Data_Type.ToUpper() == "NVARCHAR" ? "N'" : "'") + obj.Value1 + "' AND ";
                        
                    }

                }
                param = param.Length > 3 ?  " Where " + param.Substring(0, param.Length - 4) : param;
                proc="select "+select.TrimEnd(',')+" from " + view +param;
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetData = workbook.Worksheets[0];
                SheetData.Name = name;


                DataAccess dal = Util.Dal();
                ParamCollection pc = new ParamCollection();
                DataTable dtInvtID = dal.ExecDataTable(proc, CommandType.Text, ref pc);
                SheetData.Cells.ImportDataTable(dtInvtID, true, "A1");// du lieu Inventory

                           

                SheetData.AutoFitColumns();

                

                //SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Excel97To2003);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = name + ".xls" };

            }
            catch (Exception ex)
            {

                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }
       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Xml;
using System.Xml.Linq;
using System.Data.Objects;
using Aspose.Cells;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.Data;
using System.Drawing;
using HQFramework.DAL;
using System.Dynamic;
using HQFramework.Common;
using System.Globalization;
using System.Text;
using System.Configuration;
using System.Web.Security;
namespace OM21300.Controllers
{
  
    public class DiscountChoice
    {
        public string DiscID { get; set; }
        public string DiscSeq { get; set; }
        public string DiscLineRef { get; set; }
        public string Descr { get; set; }
        public string DiscDescr { get; set; }
        public double MaxQty { get; set; }
        public string InvtID { get; set; }
        public string UnitDesc { get; set; }
        public double Qty { get; set; }
        public string ParentCode { get; set; }
        public string UnitMultDiv {get;set;}
        public double CnvFact {get;set;}
        public string SiteID { get; set; }
        public string ItemLine { get; set; }
        public string LineRef { get; set; }
        public double Price { get; set; }
        public string ProAplForItem { get; set; }
        public string FreeType { get; set; }
    }

    [DirectController]
    //[CustomAuthorize]
    //[CheckSessionOut]
    public class OM21300Controller : Controller
    {
        OM21300Entities _app = null;
        #region Action
        public OM21300Controller()
        {
            if (Current.Server != null && Current.DBApp!=null)
            {
                _app = Util.CreateObjectContext<OM21300Entities>(false);
            }
           
        }
        public ActionResult Index(string data)
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            if (data != null)
            {
            
                try
                {
                  
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["UserName"] = Current.UserName = data.Split(';')[0];
                    Session["CpnyID"] = Current.CpnyID = data.Split(';')[1];
                    Session["Language"] = Current.Language = short.Parse(data.Split(';')[2]) == 0 ? "en" : "vi";
                    Session["LangID"] = short.Parse(data.Split(';')[2]);

                    FormsAuthentication.SetAuthCookie(Current.UserName, false);
                }
                catch
                {
                    Session["Server"] = Current.Server = ConfigurationManager.AppSettings["Server"].ToString();
                    Session["DBApp"] = Current.DBApp = ConfigurationManager.AppSettings["DBApp"].ToString();
                    Session["DBSys"] = Current.DBSys = ConfigurationManager.AppSettings["DBSys"].ToString();
                    Session["Language"] = Current.Language = ConfigurationManager.AppSettings["LangID"].ToString();
                    Session["LangID"] = Current.Language == "vi" ? 1 : 0;
                    ViewBag.Title = Util.GetLang("OM23800");
                    ViewBag.Error = Message.GetString("225", null);
                    return View("Error");
                }

            }
            _app = Util.CreateObjectContext<OM21300Entities>(false);
            Util.InitRight("OM21300");
            var objRole = _app.OM21300_pdRole(Current.CpnyID, Current.UserName).FirstOrDefault();
            ViewBag.Role =objRole.Role;
            ViewBag.IsShowDisctrictBorder = objRole.IsShowDisctrictBorder;
            if (ViewBag.Role == "DSM" || ViewBag.Role == "DSR" || ViewBag.Role == "ASM")
            {
                return View("IndexDSM");
            }
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        private string GetRandomColour(Random rand)
        {
            var c = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        [DirectMethod]
        public ActionResult OM21300_GetTreeData(string cpnyID)
        {
            var lstCompany = _app.OM21300_pdCompany(Current.UserName).ToList();
            Panel panel = this.GetCmp<Panel>("pnlTree");
            TreePanel tree = new TreePanel();
            tree.ID = "treeDSR";
            tree.Fields.Add(new ModelField("Data", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Color", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));
            tree.AutoScroll = true;
            tree.RootVisible = true;            
            tree.Border = false;
            tree.Header = false;
            tree.Listeners.CheckChange.Fn = "tree_CheckChange";
    
            Node node = new Node();
            node.NodeID = "tree-node-root-company";
            node.Checked = true;
            node.Expanded = true;
            Random rand = new Random();

            var role = _app.OM21300_pdRole(Current.CpnyID, Current.UserName).FirstOrDefault().Role; 
            foreach (var item in lstCompany)
	        {
                if (cpnyID!=""&&!cpnyID.Replace(" ", "").Split(',').Contains(item.CpnyID)) continue;
		        var nodeCompany = new Node();
                nodeCompany.Checked = false;
                nodeCompany.NodeID = "node-company-" + item.CpnyID;
                nodeCompany.Text = item.CpnyID + " - " + item.CpnyName;
                nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "C", Mode = ParameterMode.Value });
                nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = item.CpnyID, Mode = ParameterMode.Value });
                nodeCompany.IconCls = "tree-parent-icon";
                nodeCompany.IconFile = Url.Content(@"~\Content\Images\OM21300\company.png");
                nodeCompany.Expanded = false;// (role == "DSM" || role == "ASM");
                var lstDSM = _app.OM21300_pdDSM(Current.UserName, item.CpnyID).ToList();
                nodeCompany.Leaf = lstDSM.Count == 0 ? true : false;
                foreach (var dsm in lstDSM)
                {
                    string colorDSM = GetRandomColour(rand);
                    var nodeDSM = new Node();
                    nodeDSM.Checked = false;
                    nodeDSM.NodeID = "node-dsm-" + item.CpnyID +  '-' + dsm.SlsperId;
                    nodeDSM.Text = @"<span style="" width:15px;height:15px;background-color:" + colorDSM + @";display:inline-block;vertical-align:middle;margin-right:5px;""></span><span style=""vertical-align: middle;"">" + dsm.SlsperId + " - " + dsm.Name + "</span>";
                    nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "M", Mode = ParameterMode.Value });
                    nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = item.CpnyID + '#' + dsm.SlsperId, Mode = ParameterMode.Value });
                    nodeDSM.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = colorDSM.Replace("#", ""), Mode = ParameterMode.Value });
                    nodeDSM.Icon = Ext.Net.Icon.None;
                    nodeDSM.IconCls = "tree-node-noicon";
                    var lstDSR = _app.OM21300_pdDSR(Current.UserName, item.CpnyID, dsm.SlsperId).ToList();

                    nodeDSM.Expanded = (role == "DSM" || role == "ASM");
                    foreach (var dsr in lstDSR)
                    {
                        string color = GetRandomColour(rand);
                        var nodeDSR = new Node();
                        nodeDSR.Checked = false;
                        nodeDSR.NodeID = "node-dsr-" + item.CpnyID + '-' + dsm.SlsperId + '-' + dsr.SlsperID;
                        nodeDSR.Text = @"<span style="" width:15px;height:15px;background-color:" + color + @";display:inline-block;vertical-align:middle;margin-right:5px;""></span><span style=""vertical-align: middle;"">" + dsr.SlsperID + " - " + dsr.Name + "</span>";
                        nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "S", Mode = ParameterMode.Value });
                        nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "Data", Value = item.CpnyID + '#'+ dsr.SlsperID + '#' + dsr.City + '#' + dsr.District, Mode = ParameterMode.Value });
                        nodeDSR.CustomAttributes.Add(new ConfigItem() { Name = "Color", Value = color.Replace("#", ""), Mode = ParameterMode.Value });
                        nodeDSR.Icon = Ext.Net.Icon.None;
                        nodeDSR.IconCls = "tree-node-noicon";
                        nodeDSR.Leaf = true;

                        nodeDSM.Children.Add(nodeDSR);
                    }

                    nodeDSM.Leaf = lstDSR.Count == 0 ? true : false;
                    nodeCompany.Children.Add(nodeDSM);
                }
                node.Children.Add(nodeCompany);
	        }
            if (node.Children.Count == 0)
            {
                node.Leaf = true;
            }
            tree.Root.Add(node);
            tree.AddTo(panel);
            return this.Direct();
                           
        }

        public ActionResult Export(string fromDate,  string dsr,  string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt)
        {
            try
            {
                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet sheetTrans = workbook.Worksheets[0];

                sheetTrans.Name = "Details";

                DataAccess dal = Util.Dal();
                CultureInfo ui = new CultureInfo("en-US");
                DateTime date = Convert.ToDateTime(fromDate, ui);
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@Period", DbType.String, clsCommon.GetValueDBNull(date.ToString("yyyyMM")), ParameterDirection.Input, 30));              
                pc.Add(new ParamStruct("@DSRID", DbType.String, clsCommon.GetValueDBNull(dsr), ParameterDirection.Input, Int32.MaxValue));
                pc.Add(new ParamStruct("@DaysOfWeek", DbType.String, clsCommon.GetValueDBNull(dayOfWeek), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@WeekEO", DbType.String, clsCommon.GetValueDBNull(weekEO), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@ClassID", DbType.String, clsCommon.GetValueDBNull(classID), ParameterDirection.Input, 2000));
                pc.Add(new ParamStruct("@BrandID", DbType.String, clsCommon.GetValueDBNull(brand), ParameterDirection.Input, 2000));
                pc.Add(new ParamStruct("@FromAmt", DbType.Double, clsCommon.GetValueDBNull(fromAmt), ParameterDirection.Input, 2000));
                pc.Add(new ParamStruct("@ToAmt", DbType.Double, clsCommon.GetValueDBNull(toAmt), ParameterDirection.Input, 2000));

                DataTable dt = dal.ExecDataTable("OM21300_pdCoverageExport", CommandType.StoredProcedure, ref pc);

                //List<OM21300_pdCoverage_Result> lstDetail = _app.OM21300_pdCoverage_Result(data["BatNbr"].PassNull(), data["BranchID"].PassNull(), "%", "%").ToList();
                
                sheetTrans.Cells.ImportDataTable(dt, true, "A1");
                sheetTrans.AutoFitColumns();
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "Coverage_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx" };
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
        public ActionResult GetDay()
        {
            var data = _app.OM21300_pcDay(Current.LangID).ToList();
            return this.Store(data);
        }
        //public ActionResult GetLocationType()
        //{
        //    var data = _app.OM21300_pcLocation(Current.LangID).ToList();
        //    return this.Store(data);
        //}       
        public ActionResult GetVisitType()
        {
            var data = _app.OM21300_pcVisitType(Current.LangID).ToList();
            return this.Store(data);
        }
        public ActionResult GetCityShape()
        {
            var data = _app.OM21300_pdCityShape().ToList();
            return this.Store(data);
        }
        public ActionResult GetCustClass()
        {
            Random rand = new Random();
            var data = _app.OM21300_pcCustClass().ToList();
            foreach (var item in data)
            {
                if (item.Color == "") item.Color = GetRandomColour(rand);
            }
            return this.Store(data);
        }
        public ActionResult GetLocationType()
        {
            var data = _app.OM21300_pcLocation(Current.LangID).ToList();
            return this.Store(data);
        }
        public ActionResult GetBrand(string materialType)
        {
            Random rand = new Random();
            var data = _app.OM21300_pcBrand().ToList();
            foreach (var item in data)
            {
                if (item.Color == "") item.Color = GetRandomColour(rand);
            }
            return this.Store(data);
        }
        public ActionResult GetFillType()
        {
            var data = _app.OM21300_pcFillType(Current.LangID).ToList();
            return this.Store(data);
        }
        public ActionResult GetCityBorder(string shapeID)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdBorder(Current.UserName, shapeID).ToList();
            return this.Store(data);
        }
        public ActionResult GetDistrictBorder(string cityID, string districtID, string shapeID)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdDistrictBorder(Current.UserName, cityID, districtID, shapeID).ToList();
            return this.Store(data);
        }
        public ActionResult GetDistrictDSM(string dsr)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdDistrictDSM(Current.UserName, Current.CpnyID, dsr).ToList();
            return this.Store(data);
        }
        public ActionResult GetDistrict()
        {
            var data = _app.OM21300_pdDistrict(Current.UserName, Current.CpnyID).ToList();
            return this.Store(data);
        }
        public ActionResult GetCoverage(DateTime fromDate,  string dsr,  string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdCoverage(fromDate.ToString("yyyyMM"), dsr,  dayOfWeek, weekEO, classID, brand, fromAmt, toAmt).ToList();
            return this.Store(data);
        }
        public ActionResult GetCust1000(DateTime fromDate,  string dsr,  string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdCust1000(fromDate.ToString("yyyyMM"), dsr, dayOfWeek, weekEO, classID, brand, fromAmt, toAmt).ToList();
            return this.Store(data);
        }
        public ActionResult GetLocation(DateTime fromDate,  string dsr,  string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdLocationBranch(fromDate.ToString("yyyyMM"), dsr,  dayOfWeek, weekEO, classID, brand, fromAmt, toAmt).ToList();
            return this.Store(data);
        }

        public ActionResult GetCity(DateTime fromDate,  string dsr, string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdCity(fromDate.ToString("yyyyMM"), dsr, classID, dayOfWeek, weekEO, brand, fromAmt, toAmt).ToList();
            return this.Store(data);
        }
        public ActionResult GetChartPie(DateTime fromDate, string dsr, string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdChartPie(fromDate.ToString("yyyyMM"),  dsr, dayOfWeek, weekEO, classID, brand, fromAmt, toAmt).ToList();
            return this.Store(data);
        }
        public ActionResult GetCustChartPie(DateTime fromDate,  string dsr,  string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt, string custID)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdCustChartPie(fromDate.ToString("yyyyMM"),  dsr,  dayOfWeek, weekEO, classID, brand, fromAmt, toAmt, custID).ToList();
            return this.Store(data);
        }
        public ActionResult GetChartColumn(DateTime fromDate,  string dsr,  string dayOfWeek, string weekEO, string classID, string brand, double fromAmt, double toAmt)
        {
            _app.CommandTimeout = 3600;
            var data = _app.OM21300_pdChartColumn( fromDate.ToString("yyyyMM") , dsr,  dayOfWeek, weekEO, classID, brand, fromAmt, toAmt).ToList();
            return this.Store(data);
        }
        #endregion
        private void ExportCountry()
        {
            if (!Directory.Exists(Server.MapPath("")))
            {
                Directory.CreateDirectory(Server.MapPath(""));
            }
            System.IO.TextWriter writeFile = new StreamWriter(Server.MapPath("map.country.js"));
            try
            {
                DataAccess dal = Util.Dal();
                DataTable dt = dal.ExecDataTable("select distinct ShapeID from MAP_Country", CommandType.Text, null);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("var countryMap = [");
             
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    sb.AppendLine("{shapeID:\"" + dt.Rows[j]["ShapeID"].ToString()+"\",points:[");
                    DataTable dt2 = dal.ExecDataTable("select * from MAP_Country where ShapeID = '" + dt.Rows[j]["ShapeID"].ToString()+"'", CommandType.Text, null);
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        if (i == dt2.Rows.Count - 1)
                        {
                            sb.AppendLine("new google.maps.LatLng(" + dt2.Rows[i]["Y"].ToString() + "," + dt2.Rows[i]["X"].ToString() + ")]");
                        }
                        else
                        {
                            sb.AppendLine("new google.maps.LatLng(" + dt2.Rows[i]["Y"].ToString() + "," + dt2.Rows[i]["X"].ToString() + "),");
                        }
                    }

                    if(j == dt.Rows.Count - 1)
                    {
                        sb.AppendLine("}");
                    }
                    else
                    {
                        sb.AppendLine("},");
                    }
                }

                sb.AppendLine("];");

                writeFile.Write(sb.ToString());

                writeFile.Close();
                writeFile = null;

            }
            catch (Exception)
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
        private void ExportCity()
        {
            if (!Directory.Exists(Server.MapPath("")))
            {
                Directory.CreateDirectory(Server.MapPath(""));
            }
            System.IO.TextWriter writeFile = new StreamWriter(Server.MapPath("map.city2.js"));
            try
            {
                DataAccess dal = Util.Dal();
                DataTable dt = dal.ExecDataTable("select * from MAP_City", CommandType.Text, null);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("var cityMap = [");

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    sb.AppendLine("{shapeID:\"" + dt.Rows[j]["ShapeID"].ToString() + "\",city:\"" + dt.Rows[j]["City"].ToString() + "\",name:\"" + dt.Rows[j]["Name"].ToString() + "\",points:[");
                    DataTable dt2 = dal.ExecDataTable("select * from MAP_CityNode where ShapeID = '" + dt.Rows[j]["ShapeID"].ToString() + "'", CommandType.Text, null);
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        if (i == dt2.Rows.Count - 1)
                        {
                            sb.AppendLine("new google.maps.LatLng(" + dt2.Rows[i]["Y"].ToString() + "," + dt2.Rows[i]["X"].ToString() + ")]");
                        }
                        else
                        {
                            sb.AppendLine("new google.maps.LatLng(" + dt2.Rows[i]["Y"].ToString() + "," + dt2.Rows[i]["X"].ToString() + "),");
                        }
                    }

                    if (j == dt.Rows.Count - 1)
                    {
                        sb.AppendLine("}");
                    }
                    else
                    {
                        sb.AppendLine("},");
                    }
                }

                sb.AppendLine("];");

                writeFile.Write(sb.ToString());

                writeFile.Close();
                writeFile = null;

            }
            catch (Exception)
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
        private void ExportDistrict()
        {
            if (!Directory.Exists(Server.MapPath("")))
            {
                Directory.CreateDirectory(Server.MapPath(""));
            }
            System.IO.TextWriter writeFile = new StreamWriter(Server.MapPath("map.district.js"));
            try
            {
                DataAccess dal = Util.Dal();
                DataTable dt = dal.ExecDataTable(@"Select distinct d.ShapeID, d.Name, District, c.City
	                From MAP_District as d 
		            inner join MAP_City as c on d.CityID = c.CityID", CommandType.Text, null);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("var districtMap = [");

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    sb.AppendLine("{shapeID:\"" + dt.Rows[j]["ShapeID"].ToString() + "\",district:\"" + dt.Rows[j]["District"].ToString() + "\",city:\"" + dt.Rows[j]["City"].ToString() + "\",name:\"" + dt.Rows[j]["Name"].ToString() + "\",points:[");
                    DataTable dt2 = dal.ExecDataTable("select * from MAP_DistrictNode where ShapeID = '" + dt.Rows[j]["ShapeID"].ToString() + "'", CommandType.Text, null);
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        if (i == dt2.Rows.Count - 1)
                        {
                            sb.AppendLine("new google.maps.LatLng(" + dt2.Rows[i]["Y"].ToString() + "," + dt2.Rows[i]["X"].ToString() + ")]");
                        }
                        else
                        {
                            sb.AppendLine("new google.maps.LatLng(" + dt2.Rows[i]["Y"].ToString() + "," + dt2.Rows[i]["X"].ToString() + "),");
                        }
                    }

                    if (j == dt.Rows.Count - 1)
                    {
                        sb.AppendLine("}");
                    }
                    else
                    {
                        sb.AppendLine("},");
                    }
                }

                sb.AppendLine("];");

                writeFile.Write(sb.ToString());

                writeFile.Close();
                writeFile = null;

            }
            catch (Exception)
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

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
using HQ.eSkySys;
namespace OM20700.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM20700Controller : Controller
    {
        private string _screenNbr = "OM20700";
        private string _userName = Current.UserName;
        OM20700Entities _db = Util.CreateObjectContext<OM20700Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            return View();
        }
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetHeader(string branchID, string priceID )
        {
            return this.Store(_db.OM20700_pdHeader(Current.UserName, branchID, priceID).ToList());
        }
        public ActionResult GetListPrice(string priceID)
        {
            return this.Store(_db.OM20700_pgDetail(Current.UserName, Current.CpnyID, priceID).ToList());
        }
        public ActionResult GetListPriceCustomer(string priceID)
        {
            var lstPriceCust = _db.OM20700_pgPriceCust(priceID, Current.UserName).ToList();
            return this.Store(lstPriceCust);
        }
        public ActionResult GetListPriceCompany(string priceID)
        {
            var lstCompany = _db.OM20700_pgCompany(Current.UserName, priceID).ToList();
            return this.Store(lstCompany);
        }


        private ActionResult CheckData(FormCollection data)
        {
            var lstCpny = JSON.Deserialize<List<OM_SlsPriceCpny>>(data["lstAllCompany"]);

            var lstPrice = JSON.Deserialize<List<OM20700_pgDetail_Result>>(data["lstAllPrice"]);
            var lstAllCust = JSON.Deserialize<List<OM20700_pgPriceCust_Result>>(data["lstAllCust"]); 
            string status = data["Status"].PassNull();
            string handle = data["Handle"].PassNull();
          

            var lstApprove = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == "OM20700" && p.Status == status).ToList();
            var begin = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == "OM20700").ToList().FirstOrDefault(p => p.Param00.PassNull().Split(',').Any(c => c.ToLower() == "begin"));
            var end = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == "OM20700").ToList().FirstOrDefault(p => p.Param00.PassNull().Split(',').Any(c => c.ToLower() == "end"));
            var objhandle = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == _screenNbr && p.Status == status && p.Handle == handle).FirstOrDefault();

            var beginStatus = begin == null ? "H" : begin.Status;
            var endStatus = end == null ? "C" : end.Status;

            if ((lstCpny.Where(p => p.CpnyID.PassNull() !="" ).Count() == 0 && (data["chkPublic"] == null ? false : true) == false))
            {
                throw new MessageException(MessageType.Message, "1888");
                
            }

            if ((lstPrice.Where(p => p.InvtID.PassNull() != "" && p.QtyBreak==0).Count()>0))
            {
                throw new MessageException(MessageType.Message, "97");

            }
            if ((lstPrice.Where(p => p.InvtID.PassNull() != "" && p.QtyBreak != 0).Count() == 0))
            {
                throw new MessageException(MessageType.Message, "2015020804");

            }
            if ((lstAllCust.Where(p => p.CustID.PassNull() != "").Count() == 0 && data["cboPriceCat"] == "IC"))
            {
                throw new MessageException(MessageType.Message, "201511192");
                
            }

            
            return null;

        }
      
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                CheckData(data);
 
                string priceID = data["cboPriceID"].PassNull();

                StoreDataHandler dataHandlerHeader = new StoreDataHandler(data["lstHeader"]);
                var objHeader = dataHandlerHeader.ObjectData<OM_SalesPriceHeader>().FirstOrDefault();

                StoreDataHandler dataHandlerPrice = new StoreDataHandler(data["lstPrice"]);
                var lstPrice = dataHandlerPrice.BatchObjectData<OM20700_pgDetail_Result>();
                StoreDataHandler dataHandlerPriceCust = new StoreDataHandler(data["lstCust"]);
                var lstPriceCust = dataHandlerPriceCust.BatchObjectData<OM_SalesPriceCust>();
                StoreDataHandler dataHandlerCpny = new StoreDataHandler(data["lstCompany"]);
                var lstCpny = dataHandlerCpny.BatchObjectData<OM_SlsPriceCpny>();
                var objPriceHeader = _db.OM_SalesPriceHeader.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower());

                if (objPriceHeader == null)
                {
                    objPriceHeader = new OM_SalesPriceHeader();
                    objPriceHeader.ResetET();
                    objPriceHeader.PriceID = priceID;
                    objPriceHeader.Crtd_DateTime = DateTime.Now;
                    objPriceHeader.Crtd_Prog = "OM20700";
                    objPriceHeader.Crtd_User = Current.UserName;
                    Update_PriceHeader(objPriceHeader, objHeader);
                    _db.OM_SalesPriceHeader.AddObject(objPriceHeader);

                }
                else
                {
                    Update_PriceHeader(objPriceHeader, objHeader);
                }

                foreach (var item in lstPrice.Deleted)
                {
                    var del = _db.OM_SalesPrice.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower() && p.InvtID == item.InvtID);
                    if (del != null) _db.OM_SalesPrice.DeleteObject(del);
                }

                lstPrice.Created.AddRange(lstPrice.Updated);
                foreach (var item in lstPrice.Created)
                {
                    if (string.IsNullOrEmpty(item.InvtID)) continue;
                    var priceInvt = _db.OM_SalesPrice.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower() && p.InvtID == item.InvtID);
                    if (priceInvt == null)
                    {
                        var newPrice = new OM_SalesPrice();

                        newPrice.ResetET();
                        newPrice.PriceID = priceID;
                        newPrice.InvtID = item.InvtID;
                        newPrice.SlsUnit = item.SlsUnit;
                        newPrice.Crtd_DateTime = DateTime.Now;
                        newPrice.Crtd_Prog = "OM20700";
                        newPrice.Crtd_User = Current.UserName;
                      
                        Update_Price(newPrice, item);
                        _db.OM_SalesPrice.AddObject(newPrice);
                    }
                    else
                    {
                        Update_Price(priceInvt, item);
                    }
                }

                foreach (var item in lstPriceCust.Deleted)
                {
                    var del = _db.OM_SalesPriceCust.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower() && p.CustID == item.CustID && p.BranchID == item.BranchID);
                    if (del != null) _db.OM_SalesPriceCust.DeleteObject(del);
                }

                lstPriceCust.Created.AddRange(lstPriceCust.Updated);
                foreach (var item in lstPriceCust.Created)
                {
                    if (string.IsNullOrEmpty(item.CustID)) continue;
                    var cust = _db.OM_SalesPriceCust.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower() && p.CustID == item.CustID && p.BranchID == item.BranchID);
                    if (cust == null)
                    {
                        var newCust = new OM_SalesPriceCust();                      
                        newCust.ResetET();
                        newCust.PriceID = priceID;
                        newCust.CustID = item.CustID;
                        newCust.BranchID = item.BranchID;
                       
                        Update_PriceCust(newCust, item);
                        _db.OM_SalesPriceCust.AddObject(newCust);
                    }
                    else
                    {
                        Update_PriceCust(cust, item);
                    }
                }

                foreach (var item in lstCpny.Deleted)
                {
                    var del = _db.OM_SlsPriceCpny.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower() && p.CpnyID == item.CpnyID);
                    if (del != null) _db.OM_SlsPriceCpny.DeleteObject(del);
                }

                lstCpny.Created.AddRange(lstCpny.Updated);
                foreach (var item in lstCpny.Created)
                {
                    if (string.IsNullOrEmpty(item.CpnyID)) continue;
                    var cpny = _db.OM_SlsPriceCpny.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower() && p.CpnyID == item.CpnyID);
                    if (cpny == null)
                    {
                        var newCpny = new OM_SlsPriceCpny();
                        newCpny.ResetET();
                        newCpny.PriceID = priceID;
                        newCpny.CpnyID = item.CpnyID;
                        
                        Update_Cpny(newCpny, item);
                        _db.OM_SlsPriceCpny.AddObject(newCpny);
                    }
                    else
                    {
                        Update_Cpny(cpny, item);
                    }
                }

                var handle = data["Handle"].PassNull();
                if (handle != string.Empty && handle != "N")
                {
                    var objhandle = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == _screenNbr && p.Status == objPriceHeader.Status && p.Handle == handle).FirstOrDefault();
                    objPriceHeader.Status = objhandle == null ? objPriceHeader.Status : objhandle.ToStatus;
                }

                _db.SaveChanges();


                return Util.CreateMessage(MessageProcess.Save, new { priceID = priceID });
              
            }catch(Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_PriceHeader(OM_SalesPriceHeader s, OM_SalesPriceHeader t)
        {
           
            s.CustClassID =t.CustClassID ;
            s.Descr = t.Descr;
            s.FromDate = t.FromDate;
            s.HOCreate = t.Public;
            s.PriceCat = t.PriceCat;
            s.Prom = t.Prom;
            s.Public = t.Public;
            s.SiteID = t.SiteID;
            s.Status = t.Status;
            s.ToDate = t.ToDate;

            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = "OM20700";
            s.LUpd_User = Current.UserName;
        }
        private void Update_Price(OM_SalesPrice s, OM20700_pgDetail_Result t)
        {
            
            s.Price = t.Price;

            s.QtyBreak = t.QtyBreak;
           
            s.LUpd_DateTime = DateTime.Now;
            s.LUpd_Prog = "OM20700";
            s.LUpd_User = Current.UserName;
        }
        private void Update_PriceCust(OM_SalesPriceCust s, OM_SalesPriceCust t)
        {
            
        }
        private void Update_Cpny(OM_SlsPriceCpny s, OM_SlsPriceCpny t)
        {
           
        }

        [HttpPost]
        public ActionResult DeleteHeader(FormCollection data)
        {
            try
            {
                string priceID=data["cboPriceID"];

                var price = _db.OM_SalesPriceHeader.FirstOrDefault(p => p.PriceID.ToLower() == priceID.ToLower());
                if (price.HOCreate)
                {
                    var user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
                    var lstApprove = _db.SI_ApprovalFlowHandle.Where(p => p.AppFolID == "OM20700" && p.Status == price.Status).ToList();
                    if (!lstApprove.Any(p => p.Param03.ToLower() == "public" && user.UserTypes.Split(',').Any(c => c.ToLower() == p.RoleID.ToLower())))
                    {
                        return Json(new { success = false, code = "20140306", type = "message", fn = "", parm = "" });
                    }
                }
                if (price != null)
                {
                    _db.OM_SalesPriceHeader.DeleteObject(price);
                }

                var lstPrice = _db.OM_SalesPrice.Where(p => p.PriceID.ToLower() == priceID.ToLower()).ToList();
                foreach (var item in lstPrice)
                {
                    _db.OM_SalesPrice.DeleteObject(item);
                }

                var lstCust = _db.OM_SalesPriceCust.Where(p => p.PriceID.ToLower() == priceID.ToLower()).ToList();
                foreach (var item in lstCust)
                {
                    _db.OM_SalesPriceCust.DeleteObject(item);
                }

                var lstCpny = _db.OM_SlsPriceCpny.Where(p => p.PriceID.ToLower() == priceID.ToLower()).ToList();
                foreach (var item in lstCpny)
                {
                    _db.OM_SlsPriceCpny.DeleteObject(item);
                }

                _db.SaveChanges();
                return Util.CreateMessage(MessageProcess.Delete, new { priceID = priceID });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }

        }
        [DirectMethod]
        public ActionResult OM20700NodeLoad(string nodeId, string type, string value)
        {
            NodeCollection nodes = new Ext.Net.NodeCollection();

            if (type == "root")
            {
                var ters = _db.OM20700_pdTerritory(Current.UserName).ToList();
                foreach (var item in ters)
                {
                    Node node = new Node();
                    node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Territory", Mode = ParameterMode.Value });
                    node.CustomAttributes.Add(new ConfigItem() { Name = "Value", Value = item.Territory, Mode = ParameterMode.Value });
                    node.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = item.Descr, Mode = ParameterMode.Value });
                    node.NodeID = "territory-" + item.Territory;
                    node.Text = item.Territory + " - " + item.Descr;
                    nodes.Add(node);
                }
            }
            else if (type.PassNull().ToLower() == "territory")
            {
                var cpnys = _db.OM20700_pcBranchAllByUser(Current.UserName).Where(p => p.Territory.ToLower() == value.ToLower()).ToList(); ;
                foreach (var item in cpnys)
                {
                    Node node = new Node();
                    node.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Company", Mode = ParameterMode.Value });
                    node.CustomAttributes.Add(new ConfigItem() { Name = "Value", Value = item.BranchID, Mode = ParameterMode.Value });
                    node.CustomAttributes.Add(new ConfigItem() { Name = "Descr", Value = item.BranchName, Mode = ParameterMode.Value });
                    node.NodeID = "company-" + item.BranchID;
                    node.Leaf = true;
                    node.Text = item.BranchID + " - " + item.BranchName;
                    nodes.Add(node);
                }

            }
            return new JsonResult() { Data = nodes.ToJson(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [DirectMethod]
        public ActionResult OM20700GetTreeBranch(string panelID)
        {
            var a = new ItemsCollection<Plugin>();
            a.Add(Html.X().TreeViewDragDrop().DDGroup("CpnyID").EnableDrop(false));

            TreeView v = new TreeView();
            v.Plugins.Add(a);
            v.Copy = true;
            TreePanel tree = new TreePanel()
            {
                ViewConfig = v
            };
            tree.ID = "treePanelBranch";
            tree.ItemID = "treePanelBranch";
            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";

            tree.Root.Add(node);


            var lstTerritories = _db.OM20700_pdTerritory(Current.UserName).ToList();//tam thoi
            var companies = _db.OM20700_pcBranchAllByUser(Current.UserName).ToList();

            foreach (var item in lstTerritories)
            {
                var nodeTerritory = new Node();
                nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = item.Territory, Mode = ParameterMode.Value });
                nodeTerritory.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Territory", Mode = ParameterMode.Value });
                //nodeTerritory.Cls = "tree-node-parent";
                nodeTerritory.Text = item.Descr;
                nodeTerritory.Checked = false;
                nodeTerritory.NodeID = "territory-" + item.Territory;
                //nodeTerritory.IconCls = "tree-parent-icon";

                var lstCompaniesInTerr = companies.Where(x => x.Territory == item.Territory);
                foreach (var company in lstCompaniesInTerr)
                {
                    var nodeCompany = new Node();
                    nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = company.BranchID, Mode = ParameterMode.Value });
                    nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Company", Mode = ParameterMode.Value });
                    //nodeCompany.Cls = "tree-node-parent";
                    nodeCompany.Text = company.BranchName;
                    nodeCompany.Checked = false;
                    nodeCompany.Leaf = true;
                    nodeCompany.NodeID = "territory-company-" + item.Territory + "-" + company.BranchID;
                    //nodeCompany.IconCls = "tree-parent-icon";

                    nodeTerritory.Children.Add(nodeCompany);

                }
                if (lstCompaniesInTerr.Count() == 0)
                {
                    nodeTerritory.Leaf = true;
                }
                node.Children.Add(nodeTerritory);
            }

            var treeBranch = X.GetCmp<Panel>(panelID);

            //tree.Listeners.ItemClick.Fn = "DiscDefintion.nodeClick";
            tree.Listeners.CheckChange.Fn = "treePanelBranch_checkChange";

            tree.AddTo(treeBranch);

            return this.Direct();
        }

    }
}

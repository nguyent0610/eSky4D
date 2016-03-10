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
namespace SA40000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA40000Controller : Controller
    {
        private string _screenNbr = "SA40000";
        private string _userName = Current.UserName;
        SA40000Entities _sys = Util.CreateObjectContext<SA40000Entities>(true);

        public ActionResult Index()
        {  
            Util.InitRight(_screenNbr);
            return View();
        }

      //  [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetSYS_CloseDateSetUp()
        {
            return this.Store(_sys.SA40000_pgSYS_CloseDateSetUp().ToList());
        }


        [DirectMethod]
        public ActionResult SA40000GetTreeBranch(string panelID)
        {
            var a=new ItemsCollection<Plugin>();
            a.Add(Html.X().TreeViewDragDrop().DDGroup("BranchID").EnableDrop(false));

            TreeView v = new TreeView();
            v.Plugins.Add(a);
            v.Copy= true;
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


			var lstTerritories = _sys.SA40000_pdTerritory(Current.UserName).ToList();//tam thoi
            var companies = _sys.SA40000_pdCompany(Current.UserName).ToList();

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
                    nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = company.CpnyID, Mode = ParameterMode.Value });
                    nodeCompany.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Company", Mode = ParameterMode.Value });
                    //nodeCompany.Cls = "tree-node-parent";
                    nodeCompany.Text = company.CpnyName;
                    nodeCompany.Checked = false;
                    nodeCompany.Leaf = true;
                    nodeCompany.NodeID = "territory-company-" + item.Territory + "-" + company.CpnyID;
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

        #region Save & Update 
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSYS_CloseDateSetUp"]);
                ChangeRecords<SA40000_pgSYS_CloseDateSetUp_Result> lstSYS_CloseDateSetUp = dataHandler1.BatchObjectData<SA40000_pgSYS_CloseDateSetUp_Result>();

                #region Save SYS_CloseDateSetUp
                foreach (SA40000_pgSYS_CloseDateSetUp_Result deleted in lstSYS_CloseDateSetUp.Deleted)
                {
                    var objDelete = _sys.SYS_CloseDateSetUp.FirstOrDefault(p => p.BranchID == deleted.BranchID);
                    if (objDelete != null)
                    {
                        _sys.SYS_CloseDateSetUp.DeleteObject(objDelete);
                       
                    }
                }

                lstSYS_CloseDateSetUp.Created.AddRange(lstSYS_CloseDateSetUp.Updated);

                foreach (SA40000_pgSYS_CloseDateSetUp_Result curLang in lstSYS_CloseDateSetUp.Created)
                {
                    if (curLang.BranchID.PassNull() == "") continue;

                    var lang = _sys.SYS_CloseDateSetUp.FirstOrDefault(p => p.BranchID.ToLower() == curLang.BranchID.ToLower());

                    if (lang != null && lstSYS_CloseDateSetUp.Deleted.Where(p => p.BranchID.ToLower() == curLang.BranchID.ToLower()).Count()==0)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingSYS_CloseDateSetUp(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new SYS_CloseDateSetUp();
                        UpdatingSYS_CloseDateSetUp(lang, curLang, true);
                        _sys.SYS_CloseDateSetUp.AddObject(lang);
                    }
                }
                #endregion

                _sys.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        //Update SYS_CloseDateSetUp
        #region Update SYS_CloseDateSetUp
        private void UpdatingSYS_CloseDateSetUp(SYS_CloseDateSetUp t, SA40000_pgSYS_CloseDateSetUp_Result s, bool isNew)
        {
            if (isNew)
            {
                t.BranchID = s.BranchID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.WrkAdjDate = s.WrkAdjDate;
            t.WrkDateChk = s.WrkDateChk;
            t.WrkLowerDays = s.WrkLowerDays;
            t.WrkOpenDate = s.WrkOpenDate;
            t.WrkUpperDays = s.WrkUpperDays;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
        #endregion
    }
}

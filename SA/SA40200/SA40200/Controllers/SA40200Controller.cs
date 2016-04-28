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
namespace SA40200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA40200Controller : Controller
    {
        private string _screenNbr = "SA40200";
        private string _userName = Current.UserName;
        SA40200Entities _sys = Util.CreateObjectContext<SA40200Entities>(true);

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

        public ActionResult GetSYS_CloseDateBranchAuto(string ID)
        {
            int value = ID.PassNull()==""?0:int.Parse(ID);
            return this.Store(_sys.SA40200_pgSYS_CloseDateBranchAuto(value).ToList());
        }

        public ActionResult GetSYS_CloseDateAuto(string ID)
        {
            int value = ID.PassNull() == "" ? 0 : int.Parse(ID);
            return this.Store(_sys.SA40200_pdHeader().FirstOrDefault(p => p.ID == value));
        }

        [DirectMethod]
        public ActionResult SA40200GetTreeBranch(string panelID)
        {
            var a = new ItemsCollection<Plugin>();
            a.Add(Html.X().TreeViewDragDrop().ID("treeBranchDrop").DDGroup("BranchID").EnableDrop(false));

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

            var lstTerritories = _sys.SA40200_ptTerritory().ToList();//tam thoi
            var companies = _sys.SA40200_ptCompany(Current.UserName).ToList();

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
            tree.Listeners.BeforeItemExpand.Handler = "App.treePanelBranch.el.mask('Loading...', 'x-mask-loading');Ext.suspendLayouts();";
            tree.Listeners.AfterItemExpand.Handler = "App.treePanelBranch.el.unmask();Ext.resumeLayouts(true);";
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
                string ID_temp = data["cboID"].PassNull();
                int ID = ID_temp == "" ? 0 : int.Parse(ID_temp);
                string Time = "";
                string time_temp = data["txtTime"].PassNull();
                string time_cut = time_temp.Substring(time_temp.Length-2,2);
                if(time_cut=="pm")
                {
                    int index= time_temp.IndexOf(":");
                    int plus = int.Parse(time_temp.Substring(0, index))+12;
                    if (plus == 24)
                    {
                        plus = 0;

                    }
                    Time += plus + time_temp.Substring(index,3);
                  
                }
                else
                {
                    int index = time_temp.IndexOf(":");
                    int plus = int.Parse(time_temp.Substring(0, index));
                    if (index == 1)
                    {
                        Time = "0";
                    }
                    if (plus == 12)
                    {
                        Time += "0";
                        plus = 0;
                    }
                    Time +=plus +time_temp.Substring(index, 3);
                }
                
                
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_CloseDateAuto"]);
                ChangeRecords<SA40200_pdHeader_Result> lstSYS_CloseDateAuto = dataHandler.BatchObjectData<SA40200_pdHeader_Result>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSYS_CloseDateBranchAuto"]);
                var lstSYS_CloseDateBranchAuto = dataHandler1.ObjectData<SA40200_pgSYS_CloseDateBranchAuto_Result>();

                #region Save Header SYS_CloseDateAuto
                lstSYS_CloseDateAuto.Created.AddRange(lstSYS_CloseDateAuto.Updated);
                foreach (SA40200_pdHeader_Result curHeader in lstSYS_CloseDateAuto.Created)
                {
                    if (ID.PassNull() == "") continue;

                    var header = _sys.SYS_CloseDateAuto.FirstOrDefault(p => p.ID == ID);
                    if (header != null)
                    {
                        if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                        {
                            header.Time = Time;
                            UpdatingHeader(ref header, curHeader);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        var iID = _sys.SA40200_GetAutoNumber().FirstOrDefault() ;

                        header = new SYS_CloseDateAuto();
                        header.ID = ID = iID.Value;
                        header.Time = Time;
                        header.Crtd_DateTime = DateTime.Now;
                        header.Crtd_Prog = _screenNbr;
                        header.Crtd_User = Current.UserName;
                        UpdatingHeader(ref header, curHeader);
                        _sys.SYS_CloseDateAuto.AddObject(header);
                    }
                }
                #endregion
                
                #region Detail
                var lstSYS_CloseDateBranchAuto_DB = _sys.SYS_CloseDateBranchAuto.Where(p => p.ID == ID).ToList();

                foreach (var del in lstSYS_CloseDateBranchAuto_DB)
                {
                    if (lstSYS_CloseDateBranchAuto.Where(p => p.ID == ID && p.BranchID == del.BranchID).Count() == 0)
                        _sys.SYS_CloseDateBranchAuto.DeleteObject(del);
                }

                foreach (var obj in lstSYS_CloseDateBranchAuto)
                {
                    if (obj.BranchID.PassNull() == "") continue;
                    var objBranch = _sys.SYS_CloseDateBranchAuto.FirstOrDefault(p => p.ID == ID && p.BranchID == obj.BranchID);
                    if (objBranch == null)
                    {
                        objBranch = new SYS_CloseDateBranchAuto();
                        objBranch.ResetET();
                        objBranch.ID = ID;
                        objBranch.BranchID = obj.BranchID;

                        _sys.SYS_CloseDateBranchAuto.AddObject(objBranch);
                    }
                }
                #endregion

                _sys.SaveChanges();
                return Json(new { success = true, ID = ID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        //Update SYS_CloseDateAuto
        #region Update SYS_CloseDateAuto
        private void UpdatingHeader(ref SYS_CloseDateAuto t, SA40200_pdHeader_Result s)
        {
            t.Active = s.Active;
            t.Descr = s.Descr;
            t.UpDates = s.UpDates;
      
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        #endregion

        #region Delete information Company
        //Delete information Company
        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string ID_temp = data["cboID"].PassNull();
                int ID = ID_temp == "" ? 0 : int.Parse(ID_temp);
                var cpny = _sys.SYS_CloseDateAuto.FirstOrDefault(p => p.ID == ID);
                if (cpny != null)
                {
                    _sys.SYS_CloseDateAuto.DeleteObject(cpny);
                }

                var lstAddr = _sys.SYS_CloseDateBranchAuto.Where(p => p.ID == ID).ToList();
                foreach (var item in lstAddr)
                {
                    _sys.SYS_CloseDateBranchAuto.DeleteObject(item);
                }

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
    }
}

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
namespace SA40100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA40100Controller : Controller
    {
        private string _screenNbr = "SA40100";
        private string _userName = Current.UserName;
        SA40100Entities _sys = Util.CreateObjectContext<SA40100Entities>(true);
        SA40100Entities _app = Util.CreateObjectContext<SA40100Entities>(false);
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

        public ActionResult GetSYS_CloseDateHistDetail(string HistID)
        {
            return this.Store(_sys.SA40100_pgSYS_CloseDateHistDetail(HistID).ToList());
        }
        public ActionResult GetSYS_CloseDateHistHeader()
        {
            return this.Store(_sys.SA40100_pfSYS_CloseDateHistHeader().ToList());
        }
        
        public ActionResult GetDayCloseDateSetUp()
        {
            return this.Store(_sys.SA40100_ppGetDayCloseDateSetUp().ToList());
        }
        
        [DirectMethod]
        public ActionResult SA40100GetTreeBranch(string panelID)
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

            var lstTerritories = _sys.SA40100_ptTerritory().ToList();//tam thoi
            var companies = _sys.SA40100_ptCompany(Current.UserName).ToList();

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
                string HistID = data["cboHistID"];
                string Task = data["cboTask"];
                string WrkDate_temp = data["lblDate"];
                DateTime dtOpen;
                DateTime WrkDate = DateTime.Parse(WrkDate_temp).ToDateShort();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_CloseDateHistHeader"]);
                ChangeRecords<SYS_CloseDateHistHeader> lstSYS_CloseDateHistHeader = dataHandler.BatchObjectData<SYS_CloseDateHistHeader>();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSYS_CloseDateHistDetail"]);
                ChangeRecords<SA40100_pgSYS_CloseDateHistDetail_Result> lstSYS_CloseDateHistDetail = dataHandler1.BatchObjectData<SA40100_pgSYS_CloseDateHistDetail_Result>();

                #region Save Header
                //lstSYS_CloseDateHistHeader.Created.AddRange(lstSYS_CloseDateHistHeader.Updated);
                //foreach (SYS_CloseDateHistHeader curHeader in lstSYS_CloseDateHistHeader.Created)
                //{
                var objHeader = _sys.SYS_CloseDateHistHeader.FirstOrDefault(p => p.HistID == HistID);
                if (objHeader == null)
                {
                    string _dateServer = DateTime.Now.ToString("yyyyMMdd");
                    var ID = _sys.SA40100_pcCreateHistID(_dateServer).FirstOrDefault();

                    var header = new SYS_CloseDateHistHeader();
                    header.HistID = ID;
                    header.Task = Task;
                    header.WrkDate = WrkDate;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = _userName;
                    header.LUpd_DateTime = DateTime.Now;
                    header.LUpd_Prog = _screenNbr;
                    header.LUpd_User = _userName;

                    _sys.SYS_CloseDateHistHeader.AddObject(header);

                    #region Save CD
                    if (Task.Trim() == "CD")
                    {
                        lstSYS_CloseDateHistDetail.Created.AddRange(lstSYS_CloseDateHistDetail.Updated);

                        dtOpen = lstSYS_CloseDateHistDetail.Updated.OrderBy(p => p.WrkOpenDateBefore).FirstOrDefault().WrkOpenDateBefore;
                        var strBranch = "";
                        foreach (var obj in lstSYS_CloseDateHistDetail.Updated)
                        {
                            strBranch += obj.BranchID + ",";
                        }
                        var lstCloseDateChecking = _app.SA40100_CloseDateChecking(dtOpen, WrkDate, strBranch, "").ToList();
                        foreach (var grdSetup in lstSYS_CloseDateHistDetail.Updated)
                        {

                            var obj = _sys.SYS_CloseDateHistDetail.Where(p => p.BranchID == grdSetup.BranchID && p.HistID == ID).FirstOrDefault();
                            if (obj == null)
                            {
                                obj = new SYS_CloseDateHistDetail();
                                obj.HistID = ID;
                                obj.BranchID = grdSetup.BranchID;
                                obj.Crtd_DateTime = DateTime.Now;
                                obj.Crtd_Prog = _screenNbr;
                                obj.Crtd_User = _userName;
                                obj.LUpd_DateTime = DateTime.Now;
                                obj.LUpd_Prog = _screenNbr;
                                obj.LUpd_User = _userName;

                                obj.WrkAdjDateBefore = grdSetup.WrkAdjDateBefore.Short();
                                obj.WrkDateChk = grdSetup.WrkDateChk;
                                obj.WrkLowerDays = grdSetup.WrkLowerDays;
                                obj.WrkOpenDateBefore = grdSetup.WrkOpenDateBefore.Short();
                                obj.WrkUpperDays = grdSetup.WrkUpperDays;
                                obj.WrkOpenDateAfter = grdSetup.WrkAdjDateBefore.Short();
                                obj.WrkAdjDateAfter = grdSetup.WrkOpenDateBefore.Short();
                                obj.Status = "H";
                                obj.ContentHist = "";
                                string content = "";
                                foreach (var objitem in lstCloseDateChecking)
                                {
                                    if (objitem.BranchID == grdSetup.BranchID)
                                    {
                                        content += objitem.BranchID + "  " + objitem.Bat + "  " + objitem.Module + " " + objitem.Screen + "\\r";
                                    }
                                }
                                if (content == "")
                                {
                                    var objSetup = _sys.SYS_CloseDateSetUp.Where(p => p.BranchID == grdSetup.BranchID).FirstOrDefault();
                                    objSetup.WrkOpenDate = WrkDate;
                                    objSetup.WrkAdjDate = WrkDate;
                                    objSetup.LUpd_DateTime = DateTime.Now;
                                    objSetup.LUpd_Prog = _screenNbr;
                                    objSetup.LUpd_User = _userName;
                                    obj.WrkOpenDateAfter = WrkDate;
                                    obj.WrkAdjDateAfter = WrkDate;
                                    obj.Status = "C";
                                }
                                else obj.ContentHist = content;
                                _sys.SYS_CloseDateHistDetail.AddObject(obj);

                            }
                        }
                    }
                    #endregion

                    #region Save OD
                    else
                    {

                        lstSYS_CloseDateHistDetail.Created.AddRange(lstSYS_CloseDateHistDetail.Updated);

                        dtOpen = lstSYS_CloseDateHistDetail.Updated.OrderBy(p => p.WrkOpenDateBefore).FirstOrDefault().WrkOpenDateBefore;
                        var strBranch = "";
                        foreach (var obj in lstSYS_CloseDateHistDetail.Updated)
                        {
                            strBranch += obj.BranchID + ",";
                        }
                        var lstCloseDateChecking = _app.SA40100_CloseDateChecking(dtOpen, WrkDate, strBranch, "").ToList();
                        foreach (SA40100_pgSYS_CloseDateHistDetail_Result curGrd in lstSYS_CloseDateHistDetail.Created)
                        {
                            if (curGrd.BranchID.PassNull() == "") continue;

                            var obj = _sys.SYS_CloseDateHistDetail.FirstOrDefault(p => p.BranchID.ToLower() == curGrd.BranchID.ToLower() && p.HistID == ID);

                            if (obj == null)
                            {
                                obj = new SYS_CloseDateHistDetail();

                                obj.HistID = ID;
                                obj.BranchID = curGrd.BranchID;
                                obj.WrkAdjDateBefore = curGrd.WrkAdjDateBefore;
                                obj.WrkAdjDateAfter = curGrd.WrkAdjDateAfter;
                                obj.WrkOpenDateBefore = curGrd.WrkOpenDateBefore;
                                obj.WrkOpenDateAfter = curGrd.WrkOpenDateAfter;
                                obj.WrkDateChk = curGrd.WrkDateChk;
                                obj.WrkLowerDays = curGrd.WrkLowerDays;
                                obj.WrkUpperDays = curGrd.WrkUpperDays;
                                obj.Status = "C";
                                obj.ContentHist = "";
                                obj.Crtd_DateTime = DateTime.Now;
                                obj.Crtd_Prog = _screenNbr;
                                obj.Crtd_User = _userName;
                                obj.LUpd_DateTime = DateTime.Now;
                                obj.LUpd_Prog = _screenNbr;
                                obj.LUpd_User = _userName;
                                string content = "";
                                var objSetup = _sys.SYS_CloseDateSetUp.FirstOrDefault(p => p.BranchID == curGrd.BranchID);
                                objSetup.WrkAdjDate = WrkDate;
                                obj.WrkAdjDateAfter = WrkDate;
                                _sys.SYS_CloseDateHistDetail.AddObject(obj);
                            }
                        }
                    }
                    #endregion

                #endregion
                }
                else
                {
                    foreach (SA40100_pgSYS_CloseDateHistDetail_Result deleted in lstSYS_CloseDateHistDetail.Deleted)
                    {
                        var del = _sys.SYS_CloseDateHistDetail.FirstOrDefault(p => p.BranchID == deleted.BranchID && p.HistID == HistID);
                        if (del != null)
                        {
                            _sys.SYS_CloseDateHistDetail.DeleteObject(del);
                        }
                    }
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

        //Update SYS_CloseDateSetUp
        #region Update SYS_CloseDateSetUp
        //private void UpdatingSYS_CloseDateSetUp(SYS_CloseDateHistDetail t, SA40100_pgSYS_CloseDateHistDetail_Result s, bool isNew)
        //{
        //    if (isNew)
        //    {
        //        t.BranchID = s.BranchID;
        //        t.Crtd_DateTime = DateTime.Now;
        //        t.Crtd_Prog = _screenNbr;
        //        t.Crtd_User = _userName;
        //    }
        //    t.WrkAdjDate = s.WrkAdjDate;
        //    t.WrkDateChk = s.WrkDateChk;
        //    t.WrkLowerDays = s.WrkLowerDays;
        //    t.WrkOpenDate = s.WrkOpenDate;
        //    t.WrkUpperDays = s.WrkUpperDays;

        //    t.LUpd_DateTime = DateTime.Now;
        //    t.LUpd_Prog = _screenNbr;
        //    t.LUpd_User = _userName;

        //}
        #endregion
    }
}

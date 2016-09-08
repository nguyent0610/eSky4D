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
namespace OM21500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21500Controller : Controller
    {
        private string _screenNbr = "OM21500";
        private string _userName = Current.UserName;
        OM21500Entities _db = Util.CreateObjectContext<OM21500Entities>(false);
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
        // Get Grid DiscDescr
        public ActionResult GetOM_DiscDescr()
        {
            return this.Store(_db.OM21500_pgLoadGrid(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        // Get Grid DiscDescr Company
        public ActionResult GetOM_DiscDescrCpny(string discCode)
        {
            return this.Store(_db.OM21500_pgDiscCompany(discCode, Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                // List header
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_DiscDescr"]);
                ChangeRecords<OM21500_pgLoadGrid_Result> lstHeader = dataHandler.BatchObjectData<OM21500_pgLoadGrid_Result>();
                // List details
                StoreDataHandler dataDetHandler = new StoreDataHandler(data["lstCpny"]);
                List<OM21500_pgDiscCompany_Result> lstDetails = dataDetHandler.ObjectData<OM21500_pgDiscCompany_Result>().ToList();
                // List details det
                StoreDataHandler dataDetsHandler = new StoreDataHandler(data["lstCpnyDel"]);
                var lstT = dataDetsHandler.ObjectData<OM21500_pgDiscCompany_Result>();
                List<OM21500_pgDiscCompany_Result> lstDetDel = new List<OM21500_pgDiscCompany_Result>();
                if (lstT != null)
                {
                    lstDetDel = new List<OM21500_pgDiscCompany_Result>(lstT);
                }
                lstHeader.Created.AddRange(lstHeader.Updated);
                // Delete or update tstamp header
                foreach (OM21500_pgLoadGrid_Result deleted in lstHeader.Deleted)
                {
                    if (lstHeader.Created.Where(p => p.DiscCode == deleted.DiscCode).Count() > 0)
                    {
                        lstHeader.Created.Where(p => p.DiscCode == deleted.DiscCode).FirstOrDefault().tstamp = deleted.tstamp;
                    }
                    else
                    {
                        var del = _db.OM_DiscDescr.Where(p => p.DiscCode == deleted.DiscCode).FirstOrDefault();
                        if (del != null)
                        {
                            var lstDetailsDel1 = _db.OM_DiscDescCpny.Where(x => x.DiscCode == del.DiscCode).ToList();
                            for (int i = 0; i < lstDetailsDel1.Count; i++)
                            {
                                _db.OM_DiscDescCpny.DeleteObject(lstDetailsDel1[i]);
                            }
                            var lstDetailsDel2 = lstDetails.Where(x => x.DiscCode == del.DiscCode).ToList();
                            while (lstDetailsDel2.Count > 0)
                            {
                                lstDetails.Remove(lstDetailsDel2.FirstOrDefault());
                            }
                            _db.OM_DiscDescr.DeleteObject(del);
                        }
                    }                    
                }
                // Update or add new header
                foreach (OM21500_pgLoadGrid_Result crrHeader in lstHeader.Created)
                {
                    if (crrHeader.DiscCode.PassNull() == "") continue;                    
                    var objHeader = _db.OM_DiscDescr.Where(p => p.DiscCode.ToLower() == crrHeader.DiscCode.ToLower()).FirstOrDefault();

                    if (objHeader != null)
                    {
                        if (objHeader.tstamp.ToHex() == crrHeader.tstamp.ToHex())
                        {
                            Update_DiscDescr(objHeader, crrHeader, false);                            
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        objHeader = new OM_DiscDescr();
                        Update_DiscDescr(objHeader, crrHeader, true);
                        _db.OM_DiscDescr.AddObject(objHeader);
                    }                    
                }
                // Del det
                for (int i = 0; i < lstDetDel.Count; i++)
                {
                    if (lstDetails.Where(p => p.DiscCode == lstDetDel[i].DiscCode && p.CpnyID == lstDetDel[i].CpnyID).Count() == 0)
                    {
                        var objD = lstDetDel[i];
                        var del = _db.OM_DiscDescCpny.Where(p => p.DiscCode == objD.DiscCode && p.CpnyID == objD.CpnyID).FirstOrDefault();
                        if (del != null)
                        {
                            _db.OM_DiscDescCpny.DeleteObject(del);
                        }
                    }
                }
                // Add det
                for (int i = 0; i < lstDetails.Count; i++)
                {
                    var obj = lstDetails[i];
                    var objDet = _db.OM_DiscDescCpny.FirstOrDefault(x =>
                        x.DiscCode.ToLower() == obj.DiscCode.ToLower() &&
                        x.CpnyID == obj.CpnyID);
                    if (objDet == null)
                    {
                        objDet = new OM_DiscDescCpny();
                        objDet.CpnyID = obj.CpnyID;
                        objDet.DiscCode = obj.DiscCode;
                        _db.OM_DiscDescCpny.AddObject(objDet);
                    }
                }
                // Save all change
                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
            }
        }
        private void Update_DiscDescr(OM_DiscDescr t, OM21500_pgLoadGrid_Result s, bool isNew)
        {
            if (isNew)
            {
                t.DiscCode = s.DiscCode;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
                t.BudgetID = "";
            }
            t.PromoType = s.PromoType;
            t.ObjApply = s.ObjApply;
            t.Descr = s.Descr;
            t.Active = s.Active;
            t.FromDate = s.FromDate;
            t.ToDate = s.ToDate;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        [DirectMethod]
        public ActionResult OM21500GetTreeBranch(string panelID)
        {
            TreePanel tree = new TreePanel();
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

            var lstTerritories = _db.OM21500_ptTerritory(Current.UserName, Current.CpnyID, Current.LangID).ToList();//tam thoi
            var companies = _db.OM21500_ptCompany(Current.UserName, Current.CpnyID, Current.LangID).ToList();

            if (lstTerritories.Count == 0)
            {
                node.Leaf = true;
            }

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
    }
}

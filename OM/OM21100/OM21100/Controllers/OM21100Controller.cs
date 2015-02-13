using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
//using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace OM21100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21100Controller : Controller
    {
        private string _screenNbr = "OM21100";
        OM21100Entities _db = Util.CreateObjectContext<OM21100Entities>(false);
        //eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        //
        // GET: /OM21100/
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

        public ActionResult GetDiscSeqInfo(string discSeq)
        {
            var discSeqInfo = _db.OM_DiscSeq.FirstOrDefault(x => x.DiscSeq == discSeq);
            return this.Store(discSeqInfo);
        }

        public ActionResult GetDiscBreak(string discID, string discSeq)
        {
            var discBreaks = _db.OM21100_pgDiscBreak(discID, discSeq).ToList();
            return this.Store(discBreaks);
        }

        public ActionResult GetFreeItem(string discID, string discSeq)
        {
            var freeItems = _db.OM21100_pgFreeItem(discID, discSeq, "").ToList();
            return this.Store(freeItems);
        }

        [DirectMethod]
        public ActionResult OM21100GetTreeBranch(string panelID)
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

            var lstTerritories = _db.OM21100_ptTerritory(Current.UserName).ToList();//tam thoi
            var companies = _db.OM21100_ptCompany(Current.UserName).ToList();

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
            tree.Listeners.CheckChange.Fn = "DiscDefintion.Event.treePanelBranch_checkChange";

            tree.AddTo(treeBranch);
            
            return this.Direct();
        }

        public ActionResult GetCompany(string discID, string discSeq)
        {
            var companies = _db.OM21100_pgCompany(discID, discSeq, Current.CpnyID).ToList();
            return this.Store(companies);
        }

        public ActionResult GetDiscItem(string discID, string discSeq)
        {
            var discDiscItems = _db.OM21100_pgDiscItem(discID, discSeq).ToList();
            return this.Store(discDiscItems);
        }

        public ActionResult GetDiscBundle(string discID, string discSeq)
        {
            var discBundles = _db.OM21100_pgDiscBundle(discID, discSeq).ToList();
            return this.Store(discBundles);
        }

        public ActionResult GetDiscCustClass(string discID, string discSeq)
        {
            var discCustClasses = _db.OM21100_pgDiscCustClass(discID, discSeq).ToList();
            return this.Store(discCustClasses);
        }

        public ActionResult GetDiscCust(string discID, string discSeq)
        {
            var discCusts = _db.OM21100_pgDiscCust(discID, discSeq, Current.CpnyID).ToList();
            return this.Store(discCusts);
        }

        public ActionResult GetDiscItemClass(string discID, string discSeq)
        {
            var discItems = _db.OM21100_pgDiscItemClass(discID, discSeq).ToList();
            return this.Store(discItems);
        }

        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var discID = data["cboDiscID"];
                var discSeq = data["cboDiscSeq"];

                if (!string.IsNullOrWhiteSpace(discID) && !string.IsNullOrWhiteSpace(discSeq))
                {
                    #region Create/Update discount
                    var disc = _db.OM_Discount.FirstOrDefault(dc => dc.DiscID == discID);
                    var tstamp = data["tstamp"];
                    var inputDisc = new OM_Discount() { 
                        Descr = data["txtDescr"],
                        DiscType = data["cboDiscType"],
                        DiscClass = data["cboDiscClass"]
                    };

                    if (disc != null)
                    {
                        // Update discount
                        if (disc.tstamp.ToHex() == tstamp)
                        {
                            updateDiscount(ref disc, inputDisc, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    { 
                        // Create new discount
                        disc = new OM_Discount();
                        disc.DiscID = discID;
                        updateDiscount(ref disc, inputDisc, true);

                        _db.OM_Discount.AddObject(disc);
                    }
                    #endregion

                    #region Create/Update/Delete discSeq
                    var discSeqHandler = new StoreDataHandler(data["lstDiscSeqInfo"]);
                    var lstDiscSeqInfo = discSeqHandler.BatchObjectData<OM_DiscSeq>();
                    

                    foreach (var deleted in lstDiscSeqInfo.Deleted)
                    { 
                        // Chua xoa DiscSeq
                    }

                    lstDiscSeqInfo.Created.AddRange(lstDiscSeqInfo.Updated);

                    foreach (var created in lstDiscSeqInfo.Created)
                    {
                        var discSeqObj = _db.OM_DiscSeq.FirstOrDefault(ds => ds.DiscID == discID && ds.DiscSeq == discSeq);
                        if (discSeqObj != null)
                        {
                            // Update discSeq
                            if (discSeqObj.tstamp.ToHex() == created.tstamp.ToHex())
                            {
                                updateDiscSeq(ref discSeqObj, created);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            discSeqObj = new OM_DiscSeq();
                            discSeqObj.DiscID = discID;
                            discSeqObj.DiscSeq = discSeq;
                            discSeqObj.Crtd_DateTime = DateTime.Now;
                            discSeqObj.Crtd_Prog = _screenNbr;
                            discSeqObj.Crtd_User = Current.UserName;

                            updateDiscSeq(ref discSeqObj, created);
                            _db.OM_DiscSeq.AddObject(discSeqObj);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete DiscBreak
                    var discBreakHandler = new StoreDataHandler(data["lstDiscBreak"]);
                    var lstDiscBreak = discBreakHandler.BatchObjectData<OM21100_pgDiscBreak_Result>();

                    foreach (var deleted in lstDiscBreak.Deleted)
                    {
                        var del = _db.OM_DiscBreak.FirstOrDefault(x => x.DiscID == discID 
                            && x.DiscSeq == discSeq 
                            && x.LineRef == deleted.LineRef);

                        if (del != null)
                        {
                            _db.OM_DiscBreak.DeleteObject(del);
                        }
                    }

                    lstDiscBreak.Created.AddRange(lstDiscBreak.Updated);

                    foreach (var created in lstDiscBreak.Created)
                    {
                        if (created.LineRef.PassNull() == "") continue;

                        var discBreak = _db.OM_DiscBreak.FirstOrDefault(x => x.DiscID == created.DiscID 
                            && x.DiscSeq == created.DiscSeq 
                            && x.LineRef == created.LineRef);

                        if (discBreak != null)
                        {
                            if (discBreak.tstamp.ToHex() == created.tstamp.ToHex())
                            {
                                updateDiscBreak(ref discBreak, created, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            discBreak = new OM_DiscBreak();
                            updateDiscBreak(ref discBreak, created, true);
                            _db.OM_DiscBreak.AddObject(discBreak);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete FreeItem
                    var freeItemHandler = new StoreDataHandler(data["lstFreeItem"]);
                    var lstFreeItem = freeItemHandler.BatchObjectData<OM21100_pgFreeItem_Result>();

                    foreach (var deleted in lstFreeItem.Deleted)
                    {
                        var del = _db.OM_DiscFreeItem.Where(x => x.DiscID == discID 
                            && x.DiscSeq == discSeq 
                            && x.LineRef == deleted.LineRef
                            && x.FreeItemID == deleted.FreeItemID).FirstOrDefault();

                        if (del != null)
                        {
                            _db.OM_DiscFreeItem.DeleteObject(del);
                        }
                    }

                    lstFreeItem.Created.AddRange(lstFreeItem.Updated);

                    foreach (var created in lstFreeItem.Created)
                    {
                        if (string.IsNullOrWhiteSpace(created.LineRef) || string.IsNullOrWhiteSpace(created.FreeItemID)) 
                            continue;

                        var freeItem = _db.OM_DiscFreeItem.FirstOrDefault(x => x.DiscID == created.DiscID 
                            && x.DiscSeq == created.DiscSeq 
                            && x.LineRef == created.LineRef
                            && x.FreeItemID == created.FreeItemID);

                        if (freeItem != null)
                        {
                            if (freeItem.tstamp.ToHex() == created.tstamp.ToHex())
                            {
                                updateFreeItem(ref freeItem, created, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            freeItem = new OM_DiscFreeItem();
                            updateFreeItem(ref freeItem, created, true);
                            _db.OM_DiscFreeItem.AddObject(freeItem);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete Company
                    var companyHandler = new StoreDataHandler(data["lstCompany"]);
                    var lstCompany = companyHandler.BatchObjectData<OM21100_pgCompany_Result>();

                    foreach (var deleted in lstCompany.Deleted)
                    {
                        var del = _db.OM_DiscCpny.Where(x => x.DiscID == discID
                            && x.DiscSeq == discSeq
                            && x.CpnyID == deleted.CpnyID).FirstOrDefault();

                        if (del != null)
                        {
                            _db.OM_DiscCpny.DeleteObject(del);
                        }
                    }

                    lstCompany.Created.AddRange(lstCompany.Updated);

                    foreach (var created in lstCompany.Created)
                    {
                        if (string.IsNullOrWhiteSpace(created.CpnyID)) 
                            continue;

                        var cpny = _db.OM_DiscCpny.FirstOrDefault(x => x.DiscID == created.DiscID
                            && x.DiscSeq == created.DiscSeq
                            && x.CpnyID == created.CpnyID);

                        if (cpny != null)
                        {
                            updateCpny(ref cpny, created, false);
                        }
                        else
                        {
                            cpny = new OM_DiscCpny();
                            updateCpny(ref cpny, created, true);
                            _db.OM_DiscCpny.AddObject(cpny);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete DiscItem
                    var discItemHandler = new StoreDataHandler(data["lstDiscItem"]);
                    var lstDiscItem = discItemHandler.BatchObjectData<OM21100_pgDiscItem_Result>();

                    foreach (var deleted in lstDiscItem.Deleted)
                    {
                        var del = _db.OM_DiscItem.Where(x => x.DiscID == discID
                            && x.DiscSeq == discSeq
                            && x.InvtID == deleted.InvtID).FirstOrDefault();

                        if (del != null)
                        {
                            _db.OM_DiscItem.DeleteObject(del);
                        }
                    }

                    lstDiscItem.Created.AddRange(lstDiscItem.Updated);

                    foreach (var created in lstDiscItem.Created)
                    {
                        if (string.IsNullOrWhiteSpace(created.InvtID))
                            continue;

                        var discItem = _db.OM_DiscItem.FirstOrDefault(x => x.DiscID == created.DiscID
                            && x.DiscSeq == created.DiscSeq
                            && x.InvtID == created.InvtID);

                        if (discItem != null)
                        {
                            updateDiscItem(ref discItem, created, false);
                        }
                        else
                        {
                            discItem = new OM_DiscItem();
                            updateDiscItem(ref discItem, created, true);
                            _db.OM_DiscItem.AddObject(discItem);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete Bundle
                    var bundleHandler = new StoreDataHandler(data["lstBundle"]);
                    var lstBundle = discItemHandler.BatchObjectData<OM21100_pgDiscBundle_Result>();

                    foreach (var deleted in lstBundle.Deleted)
                    {
                        var del = _db.OM_DiscItem.Where(x => x.DiscID == discID
                            && x.DiscSeq == discSeq
                            && x.InvtID == deleted.InvtID).FirstOrDefault();

                        if (del != null)
                        {
                            _db.OM_DiscItem.DeleteObject(del);
                        }
                    }

                    lstBundle.Created.AddRange(lstBundle.Updated);

                    foreach (var created in lstBundle.Created)
                    {
                        if (string.IsNullOrWhiteSpace(created.InvtID))
                            continue;

                        var bundle = _db.OM_DiscItem.FirstOrDefault(x => x.DiscID == created.DiscID
                            && x.DiscSeq == created.DiscSeq
                            && x.InvtID == created.InvtID);

                        if (bundle != null)
                        {
                            updateBundle(ref bundle, created, false);
                        }
                        else
                        {
                            bundle = new OM_DiscItem();
                            updateBundle(ref bundle, created, true);
                            _db.OM_DiscItem.AddObject(bundle);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete DiscCustClass
                    var discCustClassHandler = new StoreDataHandler(data["lstDiscCustClass"]);
                    var lstDiscCustClass = discCustClassHandler.BatchObjectData<OM21100_pgDiscCustClass_Result>();

                    foreach (var deleted in lstDiscCustClass.Deleted)
                    {
                        var del = _db.OM_DiscCustClass.Where(x => x.DiscID == discID
                            && x.DiscSeq == discSeq
                            && x.ClassID == deleted.ClassID).FirstOrDefault();

                        if (del != null)
                        {
                            _db.OM_DiscCustClass.DeleteObject(del);
                        }
                    }

                    lstDiscCustClass.Created.AddRange(lstDiscCustClass.Updated);

                    foreach (var created in lstDiscCustClass.Created)
                    {
                        if (string.IsNullOrWhiteSpace(created.ClassID))
                            continue;

                        var discCustClass = _db.OM_DiscCustClass.FirstOrDefault(x => x.DiscID == created.DiscID
                            && x.DiscSeq == created.DiscSeq
                            && x.ClassID == created.ClassID);

                        if (discCustClass != null)
                        {
                            updateDiscCustClass(ref discCustClass, created, false);
                        }
                        else
                        {
                            discCustClass = new OM_DiscCustClass();
                            updateDiscCustClass(ref discCustClass, created, false);
                            _db.OM_DiscCustClass.AddObject(discCustClass);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete DiscCust
                    var discCustHandler = new StoreDataHandler(data["lstDiscCust"]);
                    var lstDiscCust = discCustHandler.BatchObjectData<OM21100_pgDiscCust_Result>();

                    foreach (var deleted in lstDiscCust.Deleted)
                    {
                        var del = _db.OM_DiscCust.Where(x => x.DiscID == discID
                            && x.DiscSeq == discSeq
                            && x.CustID == deleted.CustID).FirstOrDefault();

                        if (del != null)
                        {
                            _db.OM_DiscCust.DeleteObject(del);
                        }
                    }

                    lstDiscCust.Created.AddRange(lstDiscCust.Updated);

                    foreach (var created in lstDiscCust.Created)
                    {
                        if (string.IsNullOrWhiteSpace(created.CustID))
                            continue;

                        var discCust = _db.OM_DiscCust.FirstOrDefault(x => x.DiscID == created.DiscID
                            && x.DiscSeq == created.DiscSeq
                            && x.CustID == created.CustID);

                        if (discCust != null)
                        {
                            updateDiscCust(ref discCust, created, false);
                        }
                        else
                        {
                            discCust = new OM_DiscCust();
                            updateDiscCust(ref discCust, created, false);
                            _db.OM_DiscCust.AddObject(discCust);
                        }
                    }
                    #endregion

                    #region Create/Update/Delete DiscItemClass
                    var discItemClassHandler = new StoreDataHandler(data["lstDiscItemClass"]);
                    var lstDiscItemClass = discItemClassHandler.BatchObjectData<OM21100_pgDiscItemClass_Result>();

                    foreach (var deleted in lstDiscItemClass.Deleted)
                    {
                        var del = _db.OM_DiscItemClass.Where(x => x.DiscID == discID
                            && x.DiscSeq == discSeq
                            && x.ClassID == deleted.ClassID).FirstOrDefault();

                        if (del != null)
                        {
                            _db.OM_DiscItemClass.DeleteObject(del);
                        }
                    }

                    lstDiscItemClass.Created.AddRange(lstDiscItemClass.Updated);

                    foreach (var created in lstDiscItemClass.Created)
                    {
                        if (string.IsNullOrWhiteSpace(created.ClassID))
                            continue;

                        var discItemClass = _db.OM_DiscItemClass.FirstOrDefault(x => x.DiscID == created.DiscID
                            && x.DiscSeq == created.DiscSeq
                            && x.ClassID == created.ClassID);

                        if (discItemClass != null)
                        {
                            updateDiscItemClass(ref discItemClass, created, false);
                        }
                        else
                        {
                            discItemClass = new OM_DiscItemClass();
                            updateDiscItemClass(ref discItemClass, created, false);
                            _db.OM_DiscItemClass.AddObject(discItemClass);
                        }
                    }
                    #endregion

                    _db.SaveChanges();
                    return Json(new { success = true, msgCode = 201405071 });
                }
                else
                {
                    return Json(new { success = false, 
                        msgCode = 15, 
                        msgParam = string.Format("{0} & {1}",Util.GetLang("DiscID"), Util.GetLang("DiscSeq")) 
                    });
                }
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

        private void updateDiscItemClass(ref OM_DiscItemClass discItemClass, OM21100_pgDiscItemClass_Result created, bool isNew)
        {
            if (isNew)
            {
                discItemClass.ClassID = created.ClassID;
                discItemClass.DiscID = created.DiscID;
                discItemClass.DiscSeq = created.DiscSeq;

                discItemClass.Crtd_DateTime = DateTime.Now;
                discItemClass.Crtd_Prog = _screenNbr;
                discItemClass.Crtd_User = Current.UserName;
            }

            discItemClass.UnitDesc = created.UnitDesc;
            discItemClass.LUpd_DateTime = DateTime.Now;
            discItemClass.LUpd_Prog = _screenNbr;
            discItemClass.LUpd_User = Current.UserName;
        }

        private void updateDiscCust(ref OM_DiscCust discCust, OM21100_pgDiscCust_Result created, bool isNew)
        {
            if (isNew)
            {
                discCust.CustID = created.CustID;
                discCust.DiscID = created.DiscID;
                discCust.DiscSeq = created.DiscSeq;

                discCust.Crtd_DateTime = DateTime.Now;
                discCust.Crtd_Prog = _screenNbr;
                discCust.Crtd_User = Current.UserName;
            }

            discCust.LUpd_DateTime = DateTime.Now;
            discCust.LUpd_Prog = _screenNbr;
            discCust.LUpd_User = Current.UserName;
        }

        private void updateDiscCustClass(ref OM_DiscCustClass discCustClass, OM21100_pgDiscCustClass_Result created, bool isNew)
        {
            if (isNew)
            {
                discCustClass.ClassID = created.ClassID;
                discCustClass.DiscID = created.DiscID;
                discCustClass.DiscSeq = created.DiscSeq;

                discCustClass.Crtd_DateTime = DateTime.Now;
                discCustClass.Crtd_Prog = _screenNbr;
                discCustClass.Crtd_User = Current.UserName;
            }
            discCustClass.LUpd_DateTime = DateTime.Now;
            discCustClass.LUpd_Prog = _screenNbr;
            discCustClass.LUpd_User = Current.UserName;
        }

        private void updateBundle(ref OM_DiscItem bundle, OM21100_pgDiscBundle_Result created, bool isNew)
        {
            if (isNew)
            {
                bundle.InvtID = created.InvtID;
                bundle.DiscID = created.DiscID;
                bundle.DiscSeq = created.DiscSeq;
                bundle.Crtd_DateTime = DateTime.Now;
                bundle.Crtd_Prog = _screenNbr;
                bundle.Crtd_User = Current.UserName;
            }
            bundle.BundleQty = created.BundleQty;
            bundle.BundleAmt = created.BundleAmt;
            bundle.BundleNbr = created.BundleNbr;
        }

        private void updateDiscItem(ref OM_DiscItem discItem, OM21100_pgDiscItem_Result created, bool p)
        {
            //discItem.
        }

        private void updateCpny(ref OM_DiscCpny cpny, OM21100_pgCompany_Result created, bool p)
        {
            throw new NotImplementedException();
        }

        private void updateFreeItem(ref OM_DiscFreeItem discBreak, OM21100_pgFreeItem_Result created, bool p)
        {
            throw new NotImplementedException();
        }

        private void updateDiscBreak(ref OM_DiscBreak discBreak, OM21100_pgDiscBreak_Result created, bool p)
        {
            throw new NotImplementedException();
        }

        private void updateDiscSeq(ref OM_DiscSeq updatedDiscSeq, OM_DiscSeq inputDiscSeq)
        {
            updatedDiscSeq.Crtd_Role = Current.UserName;
        }

        private void updateDiscount(ref OM_Discount updatedDiscount, OM_Discount inputedDiscount, bool isNew)
        {
            if (isNew)
            {
                updatedDiscount.Crtd_DateTime = DateTime.Now;
                updatedDiscount.Crtd_Prog = _screenNbr;
                updatedDiscount.Crtd_Role = Current.UserName;
                updatedDiscount.Crtd_User = Current.UserName;
            }

            updatedDiscount.Descr = inputedDiscount.Descr;
            updatedDiscount.DiscType = inputedDiscount.DiscType;
            updatedDiscount.DiscClass = inputedDiscount.DiscClass;

            updatedDiscount.LUpd_Prog = _screenNbr;
            updatedDiscount.LUpd_DateTime = DateTime.Now;
            updatedDiscount.Crtd_Role = Current.UserName;
            updatedDiscount.LUpd_User = Current.UserName;
        }
    }
}

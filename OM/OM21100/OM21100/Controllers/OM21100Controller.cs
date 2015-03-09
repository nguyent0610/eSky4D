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
using System.Web.Script.Serialization;
namespace OM21100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21100Controller : Controller
    {
        private string _screenNbr = "OM21100";
        OM21100Entities _db = Util.CreateObjectContext<OM21100Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

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

        public ActionResult GetDiscSeqInfo(string discID, string discSeq)
        {
            var discSeqInfo = _db.OM_DiscSeq.FirstOrDefault(x => x.DiscID == discID && x.DiscSeq == discSeq);
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

        public ActionResult SaveData(FormCollection data, bool isNewDiscID, bool isNewDiscSeq)
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
                    var inputDisc = new OM_Discount()
                    {
                        DiscID = discID,
                        Descr = data["txtDescr"],
                        DiscType = data["cboDiscType"],
                        DiscClass = data["cboDiscClass"]
                    };

                    var discSeqInfoHandler = new StoreDataHandler(data["lstDiscSeqInfo"]);
                    var inputDiscSeq = discSeqInfoHandler.ObjectData<OM_DiscSeq>()
                                .FirstOrDefault(p => p.DiscID == discID && p.DiscSeq==discSeq);
                    if (inputDiscSeq != null)
                    {
                        inputDiscSeq.DiscClass = inputDisc.DiscClass;
                    }

                    var roles = _sys.Users.FirstOrDefault(x=>x.UserName==Current.UserName).UserTypes.Split(',');

                    if (disc != null)
                    {
                        if (isNewDiscID)
                        {
                            throw new MessageException(MessageType.Message, "8001", "", new string[] { Util.GetLang("DiscID") });
                        }
                        else
                        {
                            // Update discount
                            if (disc.tstamp.ToHex() == tstamp)
                            {
                                updateDiscount(ref disc, inputDisc, false, roles);
                                saveDiscSeq(data, disc, inputDiscSeq, isNewDiscSeq);

                                return Json(new { success = true, msgCode = 201405071, tstamp=disc.tstamp.ToHex() });
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                    }
                    else
                    {
                        // Create new discount
                        disc = new OM_Discount();
                        updateDiscount(ref disc, inputDisc, true, roles);
                        _db.OM_Discount.AddObject(disc);
                        saveDiscSeq(data, disc, inputDiscSeq, isNewDiscSeq);

                        return Json(new { success = true, msgCode = 201405071, tstamp = disc.tstamp.ToHex() });
                    }
                    #endregion
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msgCode = 15,
                        msgParam = string.Format("{0} & {1}", Util.GetLang("DiscID"), Util.GetLang("DiscSeq"))
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

        private void saveDiscSeq(FormCollection data, OM_Discount inputDisc, OM_DiscSeq inputDiscSeq, bool isNewDiscSeq)
        {
            var handle = data["cboHandle"];

            var discBreakHandler = new StoreDataHandler(data["lstDiscBreak"]);
            var lstDiscBreak = discBreakHandler.ObjectData<OM21100_pgDiscBreak_Result>()
                        .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                        .ToList();

            var freeItemHandler = new StoreDataHandler(data["lstFreeItem"]);
            var lstFreeItem = freeItemHandler.ObjectData<OM21100_pgFreeItem_Result>()
                        .Where(p => Util.PassNull(p.FreeItemID) != string.Empty)
                        .ToList();

            var discItemHandler = new StoreDataHandler(data["lstDiscItem"]);
            var lstDiscItem = discItemHandler.ObjectData<OM21100_pgDiscItem_Result>()
                        .Where(p => Util.PassNull(p.InvtID) != string.Empty)
                        .ToList();

            var companyHandler = new StoreDataHandler(data["lstCompany"]);
            var lstCompany = companyHandler.ObjectData<OM21100_pgCompany_Result>()
                        .Where(p => Util.PassNull(p.CpnyID) != string.Empty)
                        .ToList();

            var discCustClassHandler = new StoreDataHandler(data["lstDiscCustClass"]);
            var lstDiscCustClass = discCustClassHandler.ObjectData<OM21100_pgDiscCustClass_Result>()
                        .Where(p => Util.PassNull(p.ClassID) != string.Empty)
                        .ToList();

            var discCustHandler = new StoreDataHandler(data["lstDiscCust"]);
            var lstDiscCust = discCustHandler.ObjectData<OM21100_pgDiscCust_Result>()
                        .Where(p => Util.PassNull(p.CustID) != string.Empty)
                        .ToList();

            var freeItemChangeHandler = new StoreDataHandler(data["lstFreeItemChange"]);
            var lstFreeItemChange = freeItemChangeHandler.BatchObjectData<OM21100_pgFreeItem_Result>();

            var roles = _sys.Users.FirstOrDefault(x => x.UserName.ToLower() == Current.UserName.ToLower()).UserTypes.Split(',');
            
            foreach (var item in lstDiscBreak)
            {
                if (item.DiscAmt == 0 
                    && !((_db.OM_DiscFreeItem.Any(p => p.LineRef == item.LineRef && p.DiscID == inputDisc.DiscID && p.DiscSeq == inputDiscSeq.DiscSeq)
                    && !lstFreeItemChange.Deleted.Any(p => p.LineRef == item.LineRef))
                    || lstFreeItem.Any(p => p.LineRef == item.LineRef)
                    || lstFreeItemChange.Created.Any(p => p.LineRef == item.LineRef)
                    || lstFreeItemChange.Updated.Any(p => p.LineRef == item.LineRef)))
                {
                    throw new MessageException(MessageType.Message, "1798");
                }
            }
            if (!roles.Contains("HO") && !roles.Contains("DIST") && isNewDiscSeq)
            {
                lstCompany.Add(new OM21100_pgCompany_Result() { CpnyID = Current.CpnyID });
            }
            var seq = (from p in _db.OM_DiscSeq where p.DiscID == inputDisc.DiscID && p.DiscSeq == inputDiscSeq.DiscSeq select p).FirstOrDefault();
            if (seq != null)
            {
                if (isNewDiscSeq)
                {
                    throw new MessageException(MessageType.Message, "8001", "", new string[] { Util.GetLang("DiscSeq") });
                }

                updateDiscSeq(ref seq, inputDiscSeq, false, roles, handle);
            }
            else
            {
                seq = new OM_DiscSeq();
                updateDiscSeq(ref seq, inputDiscSeq, true, roles, handle);
                _db.OM_DiscSeq.AddObject(seq);
            }

            if (seq.Active == 1)
            {
                var lstSeq = (from p in _db.OM_DiscSeq where p.DiscClass == inputDisc.DiscClass 
                                  && (p.DiscID.ToUpper() != inputDisc.DiscID.ToUpper() 
                                  || (p.DiscID.ToUpper() == inputDisc.DiscID && p.DiscSeq.ToUpper() != inputDiscSeq.DiscSeq.ToUpper())) select p).ToList();
                if (seq.Promo == 1)
                {
                    foreach (var othSeq in lstSeq)
                    {
                        if (othSeq.Active == 1 && othSeq.Promo == 1 
                            && ((DateTime.Compare(seq.StartDate, othSeq.EndDate) <= 0 
                            && DateTime.Compare(seq.StartDate, othSeq.StartDate) >= 0) 
                            || (DateTime.Compare(seq.EndDate, othSeq.EndDate) <= 0 
                            && DateTime.Compare(seq.EndDate, othSeq.StartDate) >= 0) 
                            || (DateTime.Compare(seq.EndDate, othSeq.EndDate) >= 0 
                            && DateTime.Compare(seq.StartDate, othSeq.StartDate) <= 0)))
                        {
                            var lstDisItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper()
                                                  && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            var lstCpny = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();

                            if (inputDisc.DiscClass == "CI" || inputDisc.DiscClass == "II" || inputDisc.DiscClass == "TI")
                            {
                                bool flat = false;
                                if (lstDisItem.Any(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)) 
                                    && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                {
                                    if (inputDisc.DiscClass == "CI")
                                    {
                                        var lstDiscCustCi = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCi.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TI")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] 
                                            { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                lstDisItem.Where(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)).FirstOrDefault().InvtID, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID
                                            });
                                    }
                                }
                            }
                            else if (inputDisc.DiscClass == "BB" || inputDisc.DiscClass == "CB" || inputDisc.DiscClass == "TB")
                            {
                                int match = 0;
                                string bundleItem = string.Empty;
                                foreach (var s in lstDiscItem)
                                {
                                    bundleItem += s.InvtID + ", ";
                                    if (lstDisItem.Any(c => c.InvtID == s.InvtID && c.UnitDesc == c.UnitDesc) 
                                        && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                        match++;
                                }
                                if (match == lstDiscItem.Count)
                                {
                                    bool flat = false;
                                    if (inputDisc.DiscClass == "CB")
                                    {
                                        var lstDiscCustCb = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCb.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TB")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        bundleItem = bundleItem.Substring(0, bundleItem.Length - 2);
                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] 
                                            { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                bundleItem, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID
                                            });
                                    }


                                }
                            }
                        }
                        else if (othSeq.Active == 1 && othSeq.Promo == 0 && DateTime.Compare(seq.EndDate, othSeq.StartDate) >= 0)
                        {
                            var lstDisItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                  && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            var lstCpny = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();

                            if (inputDisc.DiscClass == "CI" || inputDisc.DiscClass == "II" || inputDisc.DiscClass == "TI")
                            {
                                if (lstDisItem.Any(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)) 
                                    && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                {
                                    throw new MessageException(MessageType.Message,"1089","",
                                        new string[] { 
                                            seq.DiscID, 
                                            seq.DiscSeq, 
                                            othSeq.DiscID, 
                                            othSeq.DiscSeq, 
                                            lstDisItem.Where(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)).FirstOrDefault().InvtID, 
                                            lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                        });
                                }
                            }
                            else if (inputDisc.DiscClass == "BB" || inputDisc.DiscClass == "CB" || inputDisc.DiscClass == "TB")
                            {
                                int match = 0;
                                string bundleItem = string.Empty;
                                foreach (var s in lstDiscItem)
                                {
                                    bundleItem += s.InvtID + ", ";
                                    if (lstDisItem.Any(c => c.InvtID == s.InvtID && c.UnitDesc == c.UnitDesc) 
                                        && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                        match++;
                                }
                                bool flat = false;
                                if (match == lstDiscItem.Count)
                                {
                                    if (inputDisc.DiscClass == "CB")
                                    {
                                        var lstDiscCustCb = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCb.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TB")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                                && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        bundleItem = bundleItem.Substring(0, bundleItem.Length - 2);

                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                bundleItem, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var othSeq in lstSeq)
                    {
                        if (othSeq.Active == 1 && ((othSeq.Promo == 1 && DateTime.Compare(othSeq.EndDate, seq.StartDate) >= 0) || othSeq.Promo == 0))
                        {
                            var lstDisItem = (from p in _db.OM_DiscItem where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                  && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            var lstCpny = (from p in _db.OM_DiscCpny where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                            if (inputDisc.DiscClass == "CI" || inputDisc.DiscClass == "II" || inputDisc.DiscClass == "TI")
                            {
                                bool flat = false;
                                if (lstDisItem.Any(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)) 
                                    && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                {
                                    if (inputDisc.DiscClass == "CI")
                                    {
                                        var lstDiscCustCi = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCi.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TI")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                                && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstDiscCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;

                                    if (flat)
                                    {
                                        throw new MessageException(MessageType.Message, "1089", "",
                                            new string[] { 
                                                seq.DiscID, 
                                                seq.DiscSeq, 
                                                othSeq.DiscID, 
                                                othSeq.DiscSeq, 
                                                lstDisItem.Where(c => lstDiscItem.Any(p => p.InvtID == c.InvtID)).FirstOrDefault().InvtID, 
                                                lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                            });
                                    }
                                }
                            }
                            else if (inputDisc.DiscClass == "BB" || inputDisc.DiscClass == "CB" || inputDisc.DiscClass == "TB")
                            {
                                int match = 0;
                                string bundleItem = string.Empty;
                                foreach (var s in lstDiscItem)
                                {
                                    bundleItem += s.InvtID + ", ";
                                    if (lstDisItem.Any(c => c.InvtID == s.InvtID && c.UnitDesc == c.UnitDesc) 
                                        && lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)))
                                        match++;
                                }
                                bool flat = false;
                                if (match == lstDiscItem.Count)
                                {
                                    if (inputDisc.DiscClass == "CB")
                                    {
                                        var lstDiscCustCb = (from p in _db.OM_DiscCust where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                               && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstDiscCustCb.Any(c => lstDiscCust.Any(p => p.CustID == c.CustID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else if (inputDisc.DiscClass == "TB")
                                    {
                                        var lstCustClass = (from p in _db.OM_DiscCustClass where p.DiscID.ToUpper() == othSeq.DiscID.ToUpper() 
                                                                && p.DiscSeq.ToUpper() == othSeq.DiscSeq.ToUpper() select p).ToList();
                                        if (lstCustClass.Any(c => lstCustClass.Any(p => p.ClassID == c.ClassID)) 
                                            && (lstCpny.Any(c => lstCompany.Any(p => p.CpnyID == c.CpnyID))))
                                        {
                                            flat = true;
                                        }
                                    }
                                    else
                                        flat = true;
                                    if (flat)
                                    {
                                        bundleItem = bundleItem.Substring(0, bundleItem.Length - 2);

                                        throw new MessageException(MessageType.Message, "1089", "",
                                        new string[] { 
                                            seq.DiscID, 
                                            seq.DiscSeq, 
                                            othSeq.DiscID, 
                                            othSeq.DiscSeq, 
                                            bundleItem, 
                                            lstCpny.Where(c => lstCompany.Any(p => p.CpnyID == c.CpnyID)).FirstOrDefault().CpnyID 
                                        });
                                    }

                                }
                            }
                        }
                    }
                }

            }

            saveCompany(data,inputDisc, inputDiscSeq, seq, roles, lstCompany);
        }

        private void saveCompany(FormCollection data, 
            OM_Discount inputDisc, 
            OM_DiscSeq inputSeq, 
            OM_DiscSeq seq, 
            string[] roles, 
            List<OM21100_pgCompany_Result> lstCompany)
        {
            var handle = data["cboHandle"];
            if (roles.Contains("HO") || roles.Contains("DIST"))
            {
                foreach (var cnpy in lstCompany)
                {
                    var sysCom = (from p in _db.OM_DiscCpny
                                  where p.DiscID.ToUpper() == inputDisc.DiscID.ToUpper() 
                                      && p.DiscSeq.ToUpper() == inputSeq.DiscSeq.ToUpper() 
                                      && p.CpnyID == cnpy.CpnyID select p).FirstOrDefault();
                    if (sysCom == null)
                    {
                        OM_DiscCpny newComp = new OM_DiscCpny();
                        newComp.DiscID = inputDisc.DiscID;
                        newComp.DiscSeq = inputSeq.DiscSeq;
                        newComp.CpnyID = cnpy.CpnyID;

                        _db.OM_DiscCpny.AddObject(newComp);
                    }
                }
            }
            else
            {
                var sysCom = (from p in _db.OM_DiscCpny
                              where p.DiscID.ToUpper() == inputDisc.DiscID.ToUpper() 
                                  && p.DiscSeq.ToUpper() == inputSeq.DiscSeq.ToUpper() 
                                  && p.CpnyID == Current.CpnyID select p).FirstOrDefault();
                if (sysCom == null)
                {
                    OM_DiscCpny newComp = new OM_DiscCpny();
                    newComp.DiscID = inputDisc.DiscID;
                    newComp.DiscSeq = inputSeq.DiscSeq;
                    newComp.CpnyID = Current.CpnyID;

                    _db.OM_DiscCpny.AddObject(newComp);
                    if (lstCompany.Count == 0)
                    {
                        lstCompany.Add(new OM21100_pgCompany_Result() { 
                            DiscID = inputDisc.DiscID,
                            DiscSeq = inputSeq.DiscSeq,
                            CpnyID = Current.CpnyID 
                        });
                    }
                }

            }
            if (handle != "N" && handle != null && (roles.Any(c => c.ToUpper() == inputSeq.Crtd_Role)
                || roles.Any(c => c.ToUpper() == inputDisc.Crtd_Role.ToUpper()) // khong hieu cai role
                || (inputSeq.Crtd_Role.PassNull() == "SUBDIST"
                && roles.Any(c => c.ToUpper() == "DIST"))))
                Save_Task(data, lstCompany, seq, inputSeq, handle);
            else
                Save_Break(data, null, null, inputSeq);
        }

        private void Save_Task(FormCollection data, List<OM21100_pgCompany_Result> lstCompany, OM_DiscSeq seq, OM_DiscSeq inputSeq, string cboHandle)
        {
            string branches = string.Empty;
            foreach (var cpny in lstCompany)
            {
                branches += cpny.CpnyID + ',';
            }
            if (branches.Length > 0) branches = branches.Substring(0, branches.Length - 1);
            var handle = (from p in _db.SI_ApprovalFlowHandle where p.AppFolID == _screenNbr 
                              && p.Status == inputSeq.Status
                              && p.Handle == cboHandle
                          select p).FirstOrDefault();
            if (handle != null && handle.Param03.PassNull().Split(',').Any(c => c.ToLower() == "many"))
            {
                foreach (var branch in branches.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var dataTask = (from p in _db.HO_PendingTasks
                                where p.ObjectID == inputSeq.DiscID + "-" + inputSeq.DiscSeq 
                                    && p.EditScreenNbr == _screenNbr
                                    && p.BranchID == branch
                                select p).FirstOrDefault();
                    if (dataTask == null && handle != null)
                    {
                        if (!handle.Param00.PassNull().Split(',').Any(c => c.ToLower() == "notapprove"))
                        {
                            HO_PendingTasks newTask = new HO_PendingTasks();
                            newTask.BranchID = branch;
                            newTask.ObjectID = inputSeq.DiscID + "-" + inputSeq.DiscSeq;
                            newTask.EditScreenNbr = _screenNbr;
                            newTask.Content = string.Format(handle.ContentApprove, inputSeq.DiscID + "-" + inputSeq.DiscSeq, inputSeq.Descr, branch);
                            newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                            newTask.Crtd_Prog = newTask.LUpd_Prog = _screenNbr;
                            newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                            newTask.Status = handle.ToStatus;
                            newTask.tstamp = new byte[1];
                            _db.HO_PendingTasks.AddObject(newTask);
                        }
                        seq.Status = handle.ToStatus;
                    }
                }

            }
            else
            {
                var dataTask = (from p in _db.HO_PendingTasks
                            where p.ObjectID == inputSeq.DiscID + "-" + inputSeq.DiscSeq && p.EditScreenNbr == _screenNbr
                                && p.BranchID == branches
                            select p).FirstOrDefault();
                if (dataTask == null && handle != null)
                {
                    if (!handle.Param00.PassNull().Split(',').Any(c => c.ToLower() == "notapprove"))
                    {
                        HO_PendingTasks newTask = new HO_PendingTasks();
                        newTask.BranchID = branches;
                        newTask.ObjectID = inputSeq.DiscID + "-" + inputSeq.DiscSeq;
                        newTask.EditScreenNbr = _screenNbr;
                        newTask.Content = string.Format(handle.ContentApprove, inputSeq.DiscID + "-" + inputSeq.DiscSeq, inputSeq.Descr, branches);
                        newTask.Crtd_Datetime = newTask.LUpd_Datetime = DateTime.Now;
                        newTask.Crtd_Prog = newTask.LUpd_Prog = _screenNbr;
                        newTask.Crtd_User = newTask.LUpd_User = Current.UserName;
                        newTask.Status = handle.ToStatus;
                        newTask.tstamp = new byte[1];
                        _db.HO_PendingTasks.AddObject(newTask);
                    }
                    seq.Status = handle.ToStatus;
                }
            }
            Save_Break(data, handle, branches, inputSeq);
        }

        private void Save_Break(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discBreakHandler = new StoreDataHandler(data["lstDiscBreak"]);
            var lstDiscBreak = discBreakHandler.ObjectData<OM21100_pgDiscBreak_Result>()
                        .Where(p => Util.PassNull(p.LineRef) != string.Empty)
                        .ToList();

            foreach (var currentBreak in lstDiscBreak)
            {
                var discBreak = (from p in _db.OM_DiscBreak
                                 where
                                     p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                     p.LineRef == currentBreak.LineRef
                                 select p).FirstOrDefault();
                if (discBreak != null)
                {
                    Update_Break(discBreak, currentBreak, false);
                }
                else
                {
                    OM_DiscBreak newBreak = new OM_DiscBreak();
                    Update_Break(newBreak, currentBreak, true);
                    _db.OM_DiscBreak.AddObject(newBreak);
                }

            }
            Save_FreeItem(data, handle, branches, inputSeq);
        }

        private void Update_Break(OM_DiscBreak t, OM21100_pgDiscBreak_Result s, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.LineRef = s.LineRef;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }

                t.BreakAmt = s.BreakAmt;
                t.BreakQty = s.BreakQty;
                t.DiscAmt = s.DiscAmt;
                t.Descr = s.Descr;

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Save_FreeItem(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var freeItemChangeHandler = new StoreDataHandler(data["lstFreeItemChange"]);
            var lstFreeItemChange = freeItemChangeHandler.BatchObjectData<OM21100_pgFreeItem_Result>();

            foreach (var currentFree in lstFreeItemChange.Created)
            {
                var free = (from p in _db.OM_DiscFreeItem
                            where
                                p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                p.LineRef == currentFree.LineRef && p.FreeItemID == currentFree.FreeItemID
                            select p).FirstOrDefault();
                if (free != null)
                {
                    Update_FreeItem(free, currentFree, false);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(currentFree.FreeItemID))
                    {
                        OM_DiscFreeItem newFree = new OM_DiscFreeItem();
                        Update_FreeItem(newFree, currentFree, true);
                        _db.OM_DiscFreeItem.AddObject(newFree);
                    }
                }

            }
            foreach (var currentFree in lstFreeItemChange.Updated)
            {
                var free = (from p in _db.OM_DiscFreeItem
                            where
                                p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                p.LineRef == currentFree.LineRef && p.FreeItemID == currentFree.FreeItemID
                            select p).FirstOrDefault();
                if (free != null)
                {
                    Update_FreeItem(free, currentFree, false);
                }
                else
                {
                    OM_DiscFreeItem newFree = new OM_DiscFreeItem();
                    Update_FreeItem(newFree, currentFree, true);
                    _db.OM_DiscFreeItem.AddObject(newFree);
                }

            }

            foreach (var currentFree in lstFreeItemChange.Deleted)
            {
                var free = (from p in _db.OM_DiscFreeItem
                            where
                                p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                p.LineRef == currentFree.LineRef && p.FreeItemID == currentFree.FreeItemID
                            select p).FirstOrDefault();
                if (free != null)
                    _db.OM_DiscFreeItem.DeleteObject(free);
            }
            if (inputSeq.DiscClass == "II")
                Save_DiscItem(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "BB")
                Save_Bundle(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TT")
                Save_DiscCustClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CC")
                Save_DiscCust(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "PP")
                Save_DiscItemClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CI")
                Save_DiscItem(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TI")
                Save_DiscItem(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TP")
                Save_DiscItemClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TB")
                Save_Bundle(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CB")
                Save_Bundle(data, handle, branches, inputSeq);
        }

        private void Save_DiscCustClass(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discCustClassHandler = new StoreDataHandler(data["lstDiscCustClass"]);
            var lstDiscCustClass = discCustClassHandler.ObjectData<OM21100_pgDiscCustClass_Result>()
                        .Where(p => Util.PassNull(p.ClassID) != string.Empty)
                        .ToList();

            foreach (var currentCustClass in lstDiscCustClass)
            {
                var custClass = (from p in _db.OM_DiscCustClass
                                 where
                                     p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                     p.ClassID == currentCustClass.ClassID
                                 select p).FirstOrDefault();
                if (custClass != null)
                {
                    Update_DiscCustClass(custClass, currentCustClass, false);
                }
                else
                {
                    OM_DiscCustClass newCustClass = new OM_DiscCustClass();
                    Update_DiscCustClass(newCustClass, currentCustClass, true);
                    _db.OM_DiscCustClass.AddObject(newCustClass);
                }

            }
            if (inputSeq.DiscClass == "TT")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TI")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TP")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TB")
                Submit_Data(handle, branches, inputSeq);
        }

        private void Submit_Data(SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            _db.SaveChanges();
            //if (handle != null)
            //{
            //    HQSendMailApprove.Approve.Mail_Approve(handle.AppFolID,
            //        inputSeq.DiscID + "-" + inputSeq.DiscSeq,
            //        handle.RoleID,
            //        handle.Status,
            //        handle.Handle,
            //        Current.LangID.ToString(),
            //        Current.UserName,
            //        branches,
            //        Current.CpnyID,
            //        string.Empty, string.Empty, string.Empty);
            //} 
            //ppv_DiscountAll_ResultDomainDataSource.Load();
        }

        private void Save_DiscItem(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discType = data["cboDiscType"];
            var discItemHandler = new StoreDataHandler(data["lstDiscItem"]);
            var lstDiscItem = discItemHandler.ObjectData<OM21100_pgDiscItem_Result>()
                        .Where(p => Util.PassNull(p.InvtID) != string.Empty)
                        .ToList();

            foreach (var currentItem in lstDiscItem)
            {
                var discItem = (from p in _db.OM_DiscItem
                                where
                                    p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                    p.InvtID == currentItem.InvtID
                                select p).FirstOrDefault();
                if (discItem != null)
                {
                    currentItem.DiscType = discType;
                    Update_DiscItem(discItem, currentItem, false);
                }
                else
                {
                    OM_DiscItem newItem = new OM_DiscItem();
                    currentItem.DiscType = discType;
                    Update_DiscItem(newItem, currentItem, true);
                    _db.OM_DiscItem.AddObject(newItem);
                }

            }
            if (inputSeq.DiscClass == "II")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CI")
                Save_DiscCust(data,handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TI")
                Save_DiscCustClass(data, handle, branches, inputSeq);

        }

        private void Save_DiscCust(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discCustHandler = new StoreDataHandler(data["lstDiscCust"]);
            var lstDiscCust = discCustHandler.ObjectData<OM21100_pgDiscCust_Result>()
                        .Where(p => Util.PassNull(p.CustID) != string.Empty)
                        .ToList();

            foreach (var currentCust in lstDiscCust)
            {
                var cust = (from p in _db.OM_DiscCust
                            where
                                p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                p.CustID == currentCust.CustID
                            select p).FirstOrDefault();
                if (cust != null)
                {
                    Update_DiscCust(cust, currentCust, false);
                }
                else
                {
                    OM_DiscCust newCust = new OM_DiscCust();
                    Update_DiscCust(newCust, currentCust, true);
                    _db.OM_DiscCust.AddObject(newCust);
                }

            }
            if (inputSeq.DiscClass == "CC")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CI")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CB")
                Submit_Data(handle, branches, inputSeq);
        }

        private void Update_DiscCust(OM_DiscCust t, OM21100_pgDiscCust_Result s, bool isNew)
        {
            try
            {
                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.CustID = s.CustID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Save_DiscItemClass(FormCollection data,SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discItemClassHandler = new StoreDataHandler(data["lstDiscItemClass"]);
            var lstDiscItemClass = discItemClassHandler.ObjectData<OM21100_pgDiscItemClass_Result>()
                        .Where(p => Util.PassNull(p.ClassID) != string.Empty)
                        .ToList();

            foreach (var currentItemClass in lstDiscItemClass)
            {
                var itemClass = (from p in _db.OM_DiscItemClass
                                 where
                                     p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                     p.ClassID == currentItemClass.ClassID
                                 select p).FirstOrDefault();
                if (itemClass != null)
                {
                    Update_DiscItemClass(itemClass, currentItemClass, false);
                }
                else
                {
                    OM_DiscItemClass newItemClass = new OM_DiscItemClass();
                    Update_DiscItemClass(newItemClass, currentItemClass, true);
                    _db.OM_DiscItemClass.AddObject(newItemClass);
                }

            }
            if (inputSeq.DiscClass == "PP")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TP")
                Save_DiscCustClass(data, handle, branches, inputSeq);

        }

        private void Save_Bundle(FormCollection data, SI_ApprovalFlowHandle handle, string branches, OM_DiscSeq inputSeq)
        {
            var discType = data["cboDiscType"];
            var discBundleHandler = new StoreDataHandler(data["lstBundle"]);
            var lstDiscBundle = discBundleHandler.ObjectData<OM21100_pgDiscBundle_Result>()
                        .Where(p => Util.PassNull(p.InvtID) != string.Empty)
                        .ToList();

            foreach (var currentBundle in lstDiscBundle)
            {
                var discBundle = (from p in _db.OM_DiscItem
                                  where
                                      p.DiscID == inputSeq.DiscID && p.DiscSeq == inputSeq.DiscSeq &&
                                      p.InvtID == currentBundle.InvtID
                                  select p).FirstOrDefault();
                if (discBundle != null)
                {
                    currentBundle.DiscType = discType;
                    Update_Bundle(discBundle, currentBundle, false);
                }
                else
                {
                    OM_DiscItem newBundle = new OM_DiscItem();
                    currentBundle.DiscType = discType;
                    Update_Bundle(newBundle, currentBundle, true);
                    _db.OM_DiscItem.AddObject(newBundle);
                }

            }
            if (inputSeq.DiscClass == "BB")
                Submit_Data(handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "TB")
                Save_DiscCustClass(data, handle, branches, inputSeq);
            else if (inputSeq.DiscClass == "CB")
                Save_DiscCust(data, handle, branches, inputSeq);

        }

        private void Update_DiscItemClass(OM_DiscItemClass t, OM21100_pgDiscItemClass_Result s, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.ClassID = s.ClassID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }
                t.UnitDesc = s.UnitDesc;
                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Update_DiscItem(OM_DiscItem t, OM21100_pgDiscItem_Result s, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.InvtID = s.InvtID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }
                t.UnitDesc = s.UnitDesc;
                t.BundleAmt = 0;
                t.BundleNbr = 0;
                t.BundleOrItem = "I";
                t.BundleQty = 0;
                t.DiscType = s.DiscType;


                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Update_Bundle(OM_DiscItem t, OM21100_pgDiscBundle_Result s, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.InvtID = s.InvtID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }
                t.BundleAmt = s.BundleAmt;
                t.BundleNbr = s.BundleNbr;
                t.BundleOrItem = "B";
                t.BundleQty = s.BundleQty;
                t.DiscType = s.DiscType;
                t.UnitDesc = string.Empty;

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Update_FreeItem(OM_DiscFreeItem t, OM21100_pgFreeItem_Result s, bool isNew)
        {
            try
            {
                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.LineRef = s.LineRef;
                    t.FreeItemID = s.FreeItemID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }

                t.FreeITemSiteID = s.FreeITemSiteID;
                t.FreeItemBudgetID = s.FreeItemBudgetID;
                t.FreeItemQty = s.FreeItemQty;
                t.UnitDescr = s.UnitDescr;

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
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

        private void Update_DiscCustClass(OM_DiscCustClass t, OM21100_pgDiscCustClass_Result s, bool isNew)
        {
            try
            {
                if (isNew)
                {
                    t.ResetET();
                    t.DiscID = s.DiscID;
                    t.DiscSeq = s.DiscSeq;
                    t.ClassID = s.ClassID;
                    t.Crtd_DateTime = DateTime.Now;
                    t.Crtd_Prog = _screenNbr;
                    t.Crtd_User = Current.UserName;
                    t.tstamp = new byte[1];
                }

                t.LUpd_DateTime = DateTime.Now;
                t.LUpd_Prog = _screenNbr;
                t.LUpd_User = Current.UserName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        private void updateFreeItem(ref OM_DiscFreeItem discBreak, OM21100_pgFreeItem_Result created, bool p)
        {
            throw new NotImplementedException();
        }

        private void updateDiscBreak(ref OM_DiscBreak discBreak, OM21100_pgDiscBreak_Result created, bool p)
        {
            throw new NotImplementedException();
        }

        private void updateDiscSeq(ref OM_DiscSeq updatedDiscSeq, OM_DiscSeq inputDiscSeq, bool isNew, string[] roles, string handle)
        {
            if (isNew)
            {
                updatedDiscSeq.ResetET();
                updatedDiscSeq.DiscID = inputDiscSeq.DiscID;
                updatedDiscSeq.DiscSeq = inputDiscSeq.DiscSeq;
                updatedDiscSeq.Crtd_DateTime = DateTime.Now;
                updatedDiscSeq.Crtd_Prog = _screenNbr;
                updatedDiscSeq.Crtd_User = Current.UserName;
                updatedDiscSeq.tstamp = new byte[1];
                updatedDiscSeq.Status = "H";
            }
            if (updatedDiscSeq.Crtd_Role.PassNull() == string.Empty)
            {
                updatedDiscSeq.Crtd_Role = roles.Contains("HO") ? "HO" : roles.Contains("DIST") ? "DIST" : roles.Contains("SUBDIST") ? "SUBDIST" : string.Empty;
            }
            updatedDiscSeq.DiscFor = inputDiscSeq.DiscFor;
            updatedDiscSeq.DiscClass = inputDiscSeq.DiscClass;
            updatedDiscSeq.EndDate = inputDiscSeq.EndDate.ToDateShort();
            updatedDiscSeq.StartDate = inputDiscSeq.StartDate.ToDateShort();
            updatedDiscSeq.Active = inputDiscSeq.Active;
            updatedDiscSeq.AllowEditDisc = inputDiscSeq.AllowEditDisc;
            updatedDiscSeq.AutoFreeItem = inputDiscSeq.AutoFreeItem;
            updatedDiscSeq.BreakBy = inputDiscSeq.BreakBy;
            updatedDiscSeq.BudgetID = inputDiscSeq.BudgetID;
            updatedDiscSeq.Descr = inputDiscSeq.Descr;
            updatedDiscSeq.ProAplForItem = inputDiscSeq.ProAplForItem;
            updatedDiscSeq.Promo = inputDiscSeq.Promo;
            updatedDiscSeq.POUse = inputDiscSeq.POUse;
            updatedDiscSeq.POEndDate = inputDiscSeq.POEndDate.ToDateShort();
            updatedDiscSeq.POStartDate = inputDiscSeq.POStartDate.ToDateShort();


            if (!string.IsNullOrEmpty(handle) && handle != "N" && updatedDiscSeq.Status != handle)
            {
                updatedDiscSeq.Status = handle;
            }

            updatedDiscSeq.LUpd_DateTime = DateTime.Now;
            updatedDiscSeq.LUpd_Prog = _screenNbr;
            updatedDiscSeq.LUpd_User = Current.UserName;
        }

        private void updateDiscount(ref OM_Discount updatedDiscount, OM_Discount inputedDiscount, bool isNew, string[] roles)
        {
            if (isNew)
            {
                updatedDiscount.DiscID = inputedDiscount.DiscID;
                updatedDiscount.Crtd_DateTime = DateTime.Now;
                updatedDiscount.Crtd_Prog = _screenNbr;
                //updatedDiscount.Crtd_Role = Current.UserName;
                updatedDiscount.Crtd_User = Current.UserName;
            }

            updatedDiscount.Descr = inputedDiscount.Descr;
            updatedDiscount.DiscType = inputedDiscount.DiscType;
            updatedDiscount.DiscClass = inputedDiscount.DiscClass;

            if (updatedDiscount.Crtd_Role.PassNull() == string.Empty)
                updatedDiscount.Crtd_Role = roles.Contains("HO") ? "HO" : roles.Contains("DIST") ? "DIST" : roles.Contains("SUBDIST") ? "SUBDIST" : string.Empty;

            updatedDiscount.LUpd_Prog = _screenNbr;
            updatedDiscount.LUpd_DateTime = DateTime.Now;
            //updatedDiscount.Crtd_Role = Current.UserName;
            updatedDiscount.LUpd_User = Current.UserName;
        }
    }
}

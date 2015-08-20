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
using System.IO;
using HQSendMailApprove;
namespace AR20201.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20201Controller : Controller
    {
        private string _screenNbr = "AR20201";
        AR20201Entities _db = Util.CreateObjectContext<AR20201Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        //
        // GET: /AR20201/
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

        public ActionResult GetPGById(string branchId, string pgID)
        {
            var pg = _db.AR_PG.FirstOrDefault(x => x.BranchID == branchId && x.PGID == pgID);
            return this.Store(pg);
        }

        public ActionResult GetPGCpnyAddr(string branchId, string pgID, string channel) 
        {
            var slsperCpnyAddrs = _db.AR20201_pgPGCpnyAddr(Current.UserName, Current.CpnyID, branchId, pgID, channel).ToList();
            return this.Store(slsperCpnyAddrs);
        }

        [DirectMethod]
        public ActionResult GetTreeCpnyAddr(string panelID, string branchID, string channel)
        {
            TreePanel tree = new TreePanel();
            tree.ID = "treePanelCpnyAddr";
            tree.ItemID = "treePanelCpnyAddr";

            tree.Fields.Add(new ModelField("RecID", ModelFieldType.String));
            tree.Fields.Add(new ModelField("Type", ModelFieldType.String));

            tree.Border = false;
            tree.RootVisible = true;
            tree.Animate = true;

            Node node = new Node();
            node.NodeID = "Root";
            tree.Root.Add(node);

            var lstCpnyMT = _db.AR20201_ptCpnyByChannel(Current.UserName, Current.CpnyID, branchID, channel).ToList(); //danh sach tat ca cpny co MT
            if (lstCpnyMT.Count() == 0)
            {
                node.Leaf = true;
            }
            else
            {
                foreach (var item in lstCpnyMT)
                {
                    var nodeCpny = new Node();
                    nodeCpny.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = item.CpnyID, Mode = ParameterMode.Value });
                    nodeCpny.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Cpny", Mode = ParameterMode.Value });
                    //nodeTerritory.Cls = "tree-node-parent";
                    nodeCpny.Text = item.CpnyName;
                    nodeCpny.Checked = false;
                    nodeCpny.NodeID = "cpny-" + item.CpnyID;
                    nodeCpny.Qtip = item.CpnyID;
                    //nodeCpny.IconCls = "tree-parent-icon";

                    var lstAddrsInCpny = _db.AR20201_ptCpnyAddr(Current.UserName, item.CpnyID, channel).ToList();
                    foreach (var addr in lstAddrsInCpny)
                    {
                        var nodeCpnyAddr = new Node();
                        nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "RecID", Value = addr.AddrID, Mode = ParameterMode.Value });
                        nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "Type", Value = "Addr", Mode = ParameterMode.Value });
                        nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "AddrName", Value = addr.Name, Mode = ParameterMode.Value });
                        nodeCpnyAddr.CustomAttributes.Add(new ConfigItem() { Name = "Addr1", Value = addr.Addr1, Mode = ParameterMode.Value });
                        //nodeCompany.Cls = "tree-node-parent";
                        nodeCpnyAddr.Text = addr.AddrID +" - "+ addr.Name;
                        nodeCpnyAddr.Checked = false;
                        nodeCpnyAddr.Leaf = true;
                        nodeCpnyAddr.NodeID = "cpny-addr-" + item.CpnyID + "-" + addr.AddrID;
                        nodeCpnyAddr.Qtip = addr.Addr1;
                        //nodeCompany.IconCls = "tree-parent-icon";

                        nodeCpny.Children.Add(nodeCpnyAddr);

                    }
                    if (lstAddrsInCpny.Count() == 0)
                    {
                        nodeCpny.Leaf = true;
                        nodeCpny.Icon = Icon.Folder;
                    }
                    node.Children.Add(nodeCpny);
                }
            }
            var treeCpnyAddr = X.GetCmp<Panel>(panelID);

            //tree.Listeners.ItemClick.Fn = "DiscDefintion.nodeClick";
            tree.Listeners.CheckChange.Fn = "Event.Tree.treePanelCpnyAddr_checkChange";

            tree.AddTo(treeCpnyAddr);

            return this.Direct();
        }

        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data, bool isNew, string channel)
        {
            try
            {
                string pgID = data["cboPGID"];
                string branchID = data["cboBranchID"];

                if (!string.IsNullOrWhiteSpace(branchID) && !string.IsNullOrWhiteSpace(pgID))
                {
                    var pgHandler = new StoreDataHandler(data["lstPG"]);
                    var inputPG = pgHandler.ObjectData<AR_PG>()
                                .FirstOrDefault(p => p.PGID == pgID && p.BranchID == branchID);

                    if (inputPG != null)
                    {
                        #region PG info
                        var pg = _db.AR_PG.FirstOrDefault(x => x.BranchID == branchID && x.PGID == pgID);
                        if (pg != null)
                        {
                            if (!isNew)
                            {
                                // update
                                if (pg.tstamp.ToHex() == inputPG.tstamp.ToHex())
                                {
                                    updateSlsper(ref pg, inputPG, false);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "2000", "", new string[]{
                                    Util.GetLang("PGID")
                                });
                            }
                        }
                        else
                        {
                            if (isNew)
                            {
                                // Create slsper
                                pg = new AR_PG();
                                pg.BranchID = branchID;
                                pg.PGID = pgID;

                                updateSlsper(ref pg, inputPG, true);
                                _db.AR_PG.AddObject(pg);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        #endregion

                        #region PG Cpny Addr
                        var cpnyAddrHandler = new StoreDataHandler(data["lstPGCpnyAddr"]);
                        var lstPGCpnyAddr = cpnyAddrHandler.BatchObjectData<AR20201_pgPGCpnyAddr_Result>();

                        foreach (var created in lstPGCpnyAddr.Created)
                        {
                            if (!string.IsNullOrWhiteSpace(created.AddrID))
                            {
                                created.BranchID = branchID;
                                created.PGID = pgID;

                                var createdCpnyAddr = _db.AR_PGCpnyAddr.FirstOrDefault(
                                    x => x.AddrID == created.AddrID
                                        && x.BranchID == created.BranchID
                                        && x.PGID == created.PGID);
                                if (createdCpnyAddr == null)
                                {
                                    updateSlsperCpnyAddr(ref createdCpnyAddr, created, true);
                                    _db.AR_PGCpnyAddr.AddObject(createdCpnyAddr);
                                }
                            }
                        }

                        foreach (var updated in lstPGCpnyAddr.Updated)
                        {
                            if (!string.IsNullOrWhiteSpace(updated.AddrID))
                            {
                                updated.BranchID = branchID;
                                updated.PGID = pgID;

                                var updatedCpnyAddr = _db.AR_PGCpnyAddr.FirstOrDefault(
                                    x => x.AddrID == updated.AddrID
                                        && x.BranchID == updated.BranchID
                                        && x.PGID == updated.PGID);
                                if (updatedCpnyAddr != null)
                                {
                                    updateSlsperCpnyAddr(ref updatedCpnyAddr, updated, false);
                                }
                            }
                        }

                        foreach (var deleted in lstPGCpnyAddr.Deleted)
                        {
                            if (!string.IsNullOrWhiteSpace(deleted.AddrID))
                            {
                                deleted.BranchID = branchID;
                                deleted.PGID = pgID;

                                var deletedCpnyAddr = _db.AR_PGCpnyAddr.FirstOrDefault(
                                    x => x.AddrID == deleted.AddrID
                                        && x.BranchID == deleted.BranchID
                                        && x.PGID == deleted.PGID);
                                if (deletedCpnyAddr != null)
                                {
                                    _db.AR_PGCpnyAddr.DeleteObject(deletedCpnyAddr);
                                }
                            }
                        }
                        #endregion

                        _db.SaveChanges();

                        return Json(new { success = true, msgCode = 201405071 });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "1555");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "1000", "",
                        new string[]{
                            string.Format("{0}, {1}", Util.GetLang("BranchID"), Util.GetLang("PGID"))
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

        private void updateSlsperCpnyAddr(ref AR_PGCpnyAddr createdCpnyAddr, AR20201_pgPGCpnyAddr_Result created, bool isNew)
        {
            if (isNew)
            {
                createdCpnyAddr = new AR_PGCpnyAddr();
                createdCpnyAddr.AddrID = created.AddrID;
                createdCpnyAddr.BranchID = created.BranchID;
                createdCpnyAddr.PGID = created.PGID;

                createdCpnyAddr.Crtd_DateTime = DateTime.Now;
                createdCpnyAddr.Crtd_Prog = _screenNbr;
                createdCpnyAddr.Crtd_User = Current.UserName;
            }
            createdCpnyAddr.WorkingTime = created.WorkingTime;
            createdCpnyAddr.LUpd_DateTime = DateTime.Now;
            createdCpnyAddr.LUpd_Prog = _screenNbr;
            createdCpnyAddr.LUpd_User = Current.UserName;
        }

        public ActionResult Delete(string pgID, string branchID, bool isNew)
        {
            try
            {
                var pg = _db.AR_PG.FirstOrDefault(p => p.PGID == pgID && p.BranchID == branchID);
                if (pg != null)
                {
                    _db.AR_PG.DeleteObject(pg);
                    var lstAddrs = _db.AR_PGCpnyAddr.Where(p => p.PGID == pgID && p.BranchID == branchID).ToList();
                    foreach (var addr in lstAddrs)
                    {
                        _db.AR_PGCpnyAddr.DeleteObject(addr);
                    }
                    _db.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = true });
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

        private void updateSlsper(ref AR_PG pg, AR_PG inputPG, bool isNew)
        {
            if (isNew)
            {
                pg.Crtd_DateTime = DateTime.Now;
                pg.Crtd_Prog = _screenNbr;
                pg.Crtd_User = Current.UserName;
            }

            pg.Addr = inputPG.Addr;
            pg.PGName = inputPG.PGName;
            pg.Position = inputPG.Position;
            pg.PGLeader = inputPG.PGLeader;

            pg.LUpd_DateTime = DateTime.Now;
            pg.LUpd_Prog = _screenNbr;
            pg.LUpd_User = Current.UserName;
        }
    }
}

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
using System.Data.SqlClient;
using Aspose.Cells;
namespace IN20800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20800Controller : Controller
    {
        private string _screenNbr = "IN20800";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        IN20800Entities _db = Util.CreateObjectContext<IN20800Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        public ActionResult Index()
        {
            bool Price = false;
            bool Pack = false;
            bool showPricetype = false;
            bool showCheckDuration = false;
            bool showDiscountPct = false;
            var objConfig = _db.IN20800_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                Price = objConfig.Price.HasValue ? objConfig.Price.Value : false;
                Pack = objConfig.Pack.HasValue ? objConfig.Pack.Value : false;
                showPricetype = objConfig.ShowPriceType.HasValue ? objConfig.ShowPriceType.Value:false;
                showCheckDuration = objConfig.ShowCheckDuration.HasValue ? objConfig.ShowCheckDuration.Value : false;
                showDiscountPct = objConfig.ShowDiscountPct.HasValue ? objConfig.ShowDiscountPct.Value : false;
            }
            ViewBag.showPricetype = showPricetype;
            ViewBag.Price = Price;
            ViewBag.Pack = Pack;
            ViewBag.showCheckDuration = showCheckDuration;
            ViewBag.showDiscountPct = showDiscountPct;
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetIN_KitHeader(string KitID)
        {
            var pGID = _db.IN_Kit.FirstOrDefault(p => p.KitID == KitID);         
            return this.Store(pGID);
        }
        public ActionResult GetIN_Component(string KitID)
        {
            return this.Store(_db.IN20800_pgLoadIN_Kit(Current.CpnyID, Current.UserName, Current.LangID, KitID).ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstIN_Kit"]);
                var curHeader = dataHandler.ObjectData<IN_Kit>().FirstOrDefault();
                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstIN_Component"]);
                ChangeRecords<IN20800_pgLoadIN_Kit_Result> lstIN_Component = dataHandler1.BatchObjectData<IN20800_pgLoadIN_Kit_Result>();

                #region Save IN_Kit
                string KitID = data["cboKitID"].PassNull();
                string Pack = data["chkPack"].PassNull();
                var header = _db.IN_Kit.FirstOrDefault(p => p.KitID == KitID);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        if(header.Pack!= curHeader.Pack || header.PriceType!=curHeader.PriceType)
                        {
                            var objcheck = _db.IN20800_pdCheckKit(Current.CpnyID, Current.UserName, Current.LangID, KitID).FirstOrDefault();
                            bool check = true;
                            if (objcheck != null)
                            {
                                check = objcheck.CheckKit ?? true;
                            }
                            if (!check)
                            {
                                throw new MessageException(MessageType.Message, "2018052311");
                            }
                        }
                        UpdatingHeader(ref header, curHeader);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    header = new IN_Kit();
                    header.ResetET();                   
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader);
                    header.KitID = KitID;
                    _db.IN_Kit.AddObject(header);
                }
                #endregion
                lstIN_Component.Created.AddRange(lstIN_Component.Updated);
                #region Delete IN_Component
                foreach (IN20800_pgLoadIN_Kit_Result del in lstIN_Component.Deleted)
                {
                    var objcheck = _db.IN20800_pdCheckKit(Current.CpnyID, Current.UserName, Current.LangID, KitID).FirstOrDefault();
                    bool check = true;
                    if (objcheck != null)
                    {
                        check = objcheck.CheckKit ?? true;
                    }
                    if (!check)
                    {
                        throw new MessageException(MessageType.Message, "2018052311");
                    }

                    if (lstIN_Component.Created.Where(p => p.KitID.ToLower().Trim() == del.KitID.ToLower().Trim() && p.ComponentID.ToLower().Trim() == del.ComponentID.ToLower().Trim()).Count() > 0)
                    {
                        lstIN_Component.Created.Where(p => p.KitID.ToLower().Trim() == del.KitID.ToLower().Trim() && p.ComponentID.ToLower().Trim() == del.ComponentID.ToLower().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.IN_Component.ToList().Where(p => p.KitID.ToLower().Trim() == del.KitID.ToLower().Trim() && p.ComponentID.ToLower().Trim() == del.ComponentID.ToLower().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.IN_Component.DeleteObject(objDel);
                        }
                    }
                }
                #endregion
                #region Save IN_Component
                var dataHandler2 = new StoreDataHandler(data["lstIN_ComponentCreate"]);
                var lstIN_ComponentCreate = dataHandler2.ObjectData<IN20800_pgLoadIN_Kit_Result>().ToList();
                foreach (IN20800_pgLoadIN_Kit_Result createKitDetail in lstIN_ComponentCreate)
                {
                    if (KitID == "" || createKitDetail.ComponentID == "") continue;
                    var objcheck = _db.IN20800_pdCheckKit(Current.CpnyID, Current.UserName, Current.LangID, KitID).FirstOrDefault();
                    bool check = true;
                    if (objcheck != null)
                    {
                        check = objcheck.CheckKit ?? true;
                    }
                    if (!check)
                    {
                        throw new MessageException(MessageType.Message, "2018052311");
                    }
                    var createKit = _db.IN_Component.FirstOrDefault(p => p.KitID == KitID && p.ComponentID == createKitDetail.ComponentID);
                    if (createKit != null)
                    {
                        if (createKit.tstamp.ToHex() == createKitDetail.tstamp.ToHex())//check tstamp xem co ai cap nhat chua
                        {                        
                            UpdatingIN_Component(createKit, createKitDetail, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        createKit = new IN_Component();
                        createKit.ResetET();
                        UpdatingIN_Component(createKit, createKitDetail, true);
                        createKit.KitID = KitID;
                        _db.IN_Component.AddObject(createKit);
                    }
                }
                #endregion
                
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                string KitID = data["cboKitID"].PassNull();
                var  objcheck = _db.IN20800_pdCheckKit(Current.CpnyID,Current.UserName,Current.LangID,KitID).FirstOrDefault();
                bool check = true;
                if (objcheck != null)
                {
                    check = objcheck.CheckKit ?? true;
                }
                if (!check) 
                {
                    throw new MessageException(MessageType.Message, "2018052311");
                }
                //get header
                var obj = _db.IN_Kit.FirstOrDefault(p => p.KitID == KitID);
                if (obj != null)
                {
                    //delete header
                    _db.IN_Kit.DeleteObject(obj);
                }

                //get grid IN_Component
                var lstIN_Component = _db.IN_Component.Where(p => p.KitID == KitID).ToList();
                //delete IN_Component
                foreach (var OjblstIN_Component in lstIN_Component)
                {
                    _db.IN_Component.DeleteObject(OjblstIN_Component);
                }

                _db.SaveChanges();
                return Json(new { success = true }, "text/html");
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void UpdatingHeader(ref IN_Kit pg, IN_Kit inputPG)
        {
            pg.KitID = inputPG.KitID;
            pg.Pack = inputPG.Pack;
            pg.Active =true;
            pg.Duration = inputPG.Duration;
            pg.FromDate = inputPG.FromDate.ToDateShort();
            pg.ToDate = inputPG.ToDate.ToDateShort();
            pg.PriceType = inputPG.PriceType;
            pg.LUpd_DateTime = DateTime.Now;
            pg.LUpd_Prog = _screenNbr;
            pg.LUpd_User = Current.UserName;
        }

        #region Update IN_Component
        private void UpdatingIN_Component(IN_Component t, IN20800_pgLoadIN_Kit_Result s, bool isNew)
        {
            if (isNew)
            {
                t.KitID = s.KitID;
                t.ComponentID = s.ComponentID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.DiscCode = s.DiscCode.PassNull();
            t.ComponentQty = s.ComponentQty;
            t.Unit = s.Unit;
            t.Price = s.Price;
            t.DiscountPct = s.DiscountPct;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
        #endregion


        private Node createNode(Node root, List<IN20800_ptKitID_Result> lstUsers)
        {
            var node = new Node();

            foreach (var childrenNodeUsers in lstUsers)
            {
                var nodeUserID = new Node();
                nodeUserID.CustomAttributes.Add(new ConfigItem() { Name = "KitID", Value = childrenNodeUsers.KitID, Mode = ParameterMode.Value });//20170405
                nodeUserID.Text = childrenNodeUsers.KitID + "-" + childrenNodeUsers.Descr;
                nodeUserID.NodeID = childrenNodeUsers.KitID;
                nodeUserID.Leaf = true;
                node.Children.Add(nodeUserID);
            }
            System.Diagnostics.Debug.WriteLine(node.Text);
            return node;
        }
        [DirectMethod]
        public ActionResult ReloadTreeIN20800()
        {
            var root = new Node() { };
            List<IN20800_ptKitID_Result> lstKitID = _db.IN20800_ptKitID(Current.CpnyID, Current.UserName, Current.LangID).ToList();
            Node node = createNode(root, lstKitID);
            this.GetCmp<TreePanel>("treeKitID").SetRootNode(node);
            return this.Direct();
        }
    }
}


using HQ.eSkyFramework;
using HQ.eSkySys;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace IN20500.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20500Controller : Controller
    {
        string screenNbr = "IN20500";
        IN20500Entities _db = Util.CreateObjectContext<IN20500Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        string b = "";
        string tmpChangeTreeDic = "0";
        private string _pathImage;
        internal string PathImage
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadIN20500");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _pathImage = config.TextVal;
                }
                else
                {
                    _pathImage = string.Empty;
                }
                return _pathImage;
            }
        }

        private bool _isConfig;
        internal bool IsConfig
        {
            get
            {
                _isConfig = string.IsNullOrWhiteSpace(PathImage) ? false : true;
                return _isConfig;
            }
        }
        public ActionResult Index()
        {
            //var user =_sys.Users.Where(p=> p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            //ViewBag.Roles = user.UserTypes;
            //var root = new Node() { };
            //var nodeType = "I";

            //var hierarchy = new SI_Hierarchy()
            //{
            //    RecordID = 0,
            //    NodeID = "",
            //    ParentRecordID = 0,
            //    NodeLevel = 1,
            //    Descr = "root",
            //    Type = nodeType
            //};
            //var z = 0 ;
            //ViewData["resultRoot"] = createNode(root, hierarchy, hierarchy.NodeLevel, z);
            return View();
        }

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, int z)
        {
            var node = new Node();
            var k = -1;
            //GetNodeItem(root, inactiveHierachy, level);
            if (inactiveHierachy.Descr == "root")
            {
                node.Text = inactiveHierachy.Descr;
            }
            else
            {
                node.Text = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
                node.NodeID = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
            }

            var tmps = _db.IN_Inventory
                .Where(p => p.NodeID == inactiveHierachy.NodeID
                    && p.ParentRecordID == inactiveHierachy.ParentRecordID
                    && p.NodeLevel == level - 1).ToList();
            var childrenInactiveHierachies = _db.SI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.Type == inactiveHierachy.Type
                    && p.NodeLevel == level).ToList();

            if (tmps != null && tmps.Count > 0)
            {
                foreach (IN_Inventory tmp in tmps)
                {

                    k++;

                    Node nodetmp = new Node();
                    nodetmp.Text = tmp.InvtID + "-" + tmp.Descr;
                    nodetmp.NodeID = tmp.InvtID + "-" + tmp.Descr;
                    nodetmp.Leaf = true;

                    node.Children.Add(nodetmp);
                    //System.Diagnostics.Debug.WriteLine(nodetmp.Text);

                }
            }

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (SI_Hierarchy childrenInactiveNode in childrenInactiveHierachies)
                {

                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, z++));

                }
            }
            else
            {
                if (tmps.Count == 0 && childrenInactiveHierachies.Count == 0)
                {

                    node.Leaf = true;
                    //node.NodeID = Convert.ToString(level) + Convert.ToString(z);
                }
                else
                {

                    node.Leaf = false;
                }
            }
            System.Diagnostics.Debug.WriteLine(node.Text);

            return node;
        }


        //[OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            ViewBag.Roles = user.UserTypes;
            //var root = new Node() { };
            //var nodeType = "I";

            //var hierarchy = new SI_Hierarchy()
            //{
            //    RecordID = 0,
            //    NodeID = "",
            //    ParentRecordID = 0,
            //    NodeLevel = 1,
            //    Descr = "root",
            //    Type = nodeType
            //};
            //var z = 0;
            //ViewData["resultRoot"] = createNode(root, hierarchy, hierarchy.NodeLevel, z);
            return PartialView();
        }

        [DirectMethod]
        public ActionResult ReloadTreeIN20500()
        {

            var root = new Node() { };
            var nodeType = "I";

            var hierarchy = new SI_Hierarchy()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "root",
                Type = nodeType
            };
            var z = 0;
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, z);

            //var m = ViewData["resultRoot2"];


            //quan trong dung de refresh Tree
            this.GetCmp<TreePanel>("IDTree").SetRootNode(node);

            return this.Direct();

        }


        public ActionResult GetProductClass(String classID)
        {
            var rptCtrl = from p in _db.IN_ProductClass
                          where p.ClassID == classID
                          select new
                          {
                              p.ClassID,
                              p.Public,
                              StkItem = p.DfltStkItem,
                              InvtType = p.DfltInvtType,
                              Source = p.DfltSource,
                              ValMthd = p.DfltValMthd,
                              LotSerTrack = p.DfltLotSerTrack,
                              p.Buyer,
                              StkUnit = p.DfltStkUnit,
                              p.DfltPOUnit,
                              p.DfltSOUnit,
                              p.MaterialType,
                              TaxCat = p.DfltSlsTaxCat,
                              SerAssign = p.DfltLotSerAssign,
                              LotSerIssMthd = p.DfltLotSerMthd,
                              ShelfLife = p.DfltLotSerShelfLife,
                              WarrantyDays = p.DfltWarrantyDays,
                              LotSerFxdTyp = p.DfltLotSerFxdTyp,
                              LotSerFxdLen = p.DfltLotSerFxdLen,
                              LotSerFxdVal = p.DfltLotSerFxdVal,
                              LotSerNumLen = p.DfltLotSerNumLen,
                              LotSerNumVal = p.DfltLotSerNumVal

                          };
            return this.Store(rptCtrl);
        }

        public ActionResult GetInventoryClass(String invtID)
        {
            var rptInven = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            return this.Store(rptInven);
        }

        public ActionResult GetCompanyClass(String classID, String invtID, String chooseGrid)
        {
            // var lst = new List<>();
            //var lst = _db.IN20500_pgGetCompanyInvt(invtID).ToList();
            //var lst = from p in _db.IN20500_pgGetCompanyInvt(invtID).ToList()

            //          select new
            //          {
            //              ClassID = p.InvtID,
            //              p.CpnyID,
            //              p.CpnyName,


            //          };
            //var lst1 = _db.IN20500_pgGetCompany(classID).ToList();
            if (chooseGrid == "1")
            {
                var lst = from p in _db.IN20500_pgGetCompanyInvt(invtID).ToList()

                          select new
                          {
                              ClassID = p.InvtID,
                              p.CpnyID,
                              p.CpnyName,


                          };
                //var lst = _db.IN20500_pgGetCompanyInvt(invtID).ToList();

                return this.Store(lst);
            }
            else
            {
                var lst = _db.IN20500_pgGetCompany(classID).ToList();
                return this.Store(lst);
            }
            //return this.Store(lst);
        }








        [DirectMethod]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(FormCollection data, string invtID, string handle, string nodeID, int nodeLevel, string parentRecordID, int hadChild, string approveStatus, bool Public, bool StkItem, string imageChange, int tmpImageDelete, string tmpImageForUpload, int tmpMediaDelete, string tmpSelectedNode, string tmpCopyFormSave, string tmpCopyForm, string tmpCopyFormImageUrl, string tmpCopyFormMedia, string tmpOldFileName, string mediaExist)
        {
            StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
            ChangeRecords<IN_Inventory> lstheader = dataHandler2.BatchObjectData<IN_Inventory>();
            StoreDataHandler dataHandler3 = new StoreDataHandler(data["lstheader2"]);
            ChangeRecords<IN_ProductClass> lstheader2 = dataHandler2.BatchObjectData<IN_ProductClass>();
            StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<IN20500_pgGetCompanyInvt_Result> lstgrd = dataHandler1.BatchObjectData<IN20500_pgGetCompanyInvt_Result>();

            ChangeRecords<IN20500_pgGetCompany_Result> lstgrd2 = dataHandler1.BatchObjectData<IN20500_pgGetCompany_Result>();

            tmpChangeTreeDic = "0";

            //StoreDataHandler dataImage = new StoreDataHandler(imageChange);
            //ChangeRecords<IN_Inventory> lstImage = dataImage.BatchObjectData<IN_Inventory>();

            //StoreDataHandler sdh = new Ext.Net.StoreDataHandler("{" + imageChange + "}");
            //Ext.Net.ChangeRecords<Business.Entities.Cand> messagesCand = sdh.ObjectData<Business.Entities.Cand>(); 

            string invtIDCopyForm = data["cboInvtID"];
            var BarCode = data[""];
            //var approveStatus = data["cboApproveStatus"];
            var Descr = data["txtDescr"];
            var Descr1 = data["txtDescr1"];
            var Status = data["cboStatus"];
            //var Public = data["chkPublic"];
            var ClassID = data["cboClassID"];
            //var StkItem = data["chkStkItem"];
            var PriceClassID = data["cboPriceClassID"];
            var InvtType = data["cboInvtType"];
            var Source = data["cboSource"];
            var ValMthd = data["cboValMthd"];
            var LotSerTrack = data["cboLotSerTrack"];
            var Buyer = data["cboBuyer"];
            var StkUnit = data["cboStkUnit"];
            var DfltPOUnit = data["cboDfltPOUnit"];
            var DfltSOUnit = data["cboDfltSOUnit"];
            var MaterialType = data["cboMaterialType"];
            var DfltSlsTaxCat = data["cboDfltSlsTaxCat"];
            var Color = data["txtColor"];
            var PrePayPct = data["txtPrePayPct"];
            var Size = data["txtSize"];
            var POFee = data["txtPOFee"];
            var Style = data["cboStyle"];
            var SOFee = data["txtSOFee"];
            var StkVol = data["txtStkVol"];
            var Vendor1 = data["cboVendor1"];
            var StkWt = data["txtStkWt"];
            var Vendor2 = data["cboVendor2"];
            var StkWtUnit = data["cboStkWtUnit"];
            var LossRate00 = data["txtLossRate00"];
            var SOPrice = data["txtSOPrice"];
            var LossRate01 = data["txtLossRate01"];
            var POPrice = data["txtPOPrice"];
            var LossRate02 = data["txtLossRate02"];
            var IRSftyStkDays = data["txtIRSftyStkDays"];
            var LossRate03 = data["txtLossRate03"];
            var IRSftyStkPct = data["txtIRSftyStkPct"];
            var IRSftyStkQty = data["txtIRSftyStkQty"];
            var IROverStkQty = data["txtIROverStkQty"];
            var SerAssign = data["cboSerAssign"];
            var LotSerIssMthd = data["cboLotSerIssMthd"];
            var ShelfLife = data["txtShelfLife"];
            var WarrantyDays = data["txtWarrantyDays"];
            var LotSerFxdTyp = data["cboLotSerFxdTyp"];
            var LotSerFxdLen = data["txtLotSerFxdLen"];
            var LotSerFxdVal = data["txtLotSerFxdVal"];
            var LotSerNumLen = data["txtLotSerNumLen"];
            var LotSerNumVal = data["txtLotSerNumVal"];












            foreach (IN20500_pgGetCompanyInvt_Result deleted in lstgrd.Deleted)
            {
                if (approveStatus != "H" && approveStatus != "D" && approveStatus != "C")
                {
                    var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                    if (del != null)
                    {
                        _db.IN_InvtCpny.DeleteObject(del);

                    }
                }
            }
            foreach (IN20500_pgGetCompanyInvt_Result created in lstgrd.Created)
            {
                var record = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == created.CpnyID).FirstOrDefault();
                if (record == null)
                {
                    record = new IN_InvtCpny();
                    record.InvtID = invtID;
                    record.CpnyID = created.CpnyID;


                    if (record.InvtID != "" && record.CpnyID != "")
                    {

                        _db.IN_InvtCpny.AddObject(record);
                    }
                }
            }



            foreach (IN20500_pgGetCompanyInvt_Result updated in lstgrd.Updated)
            {
                var record = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == updated.CpnyID).FirstOrDefault();
                if (record != null)
                {
                }
                else
                {
                    record = new IN_InvtCpny();
                    record.InvtID = invtID;
                    record.CpnyID = updated.CpnyID;
                    _db.IN_InvtCpny.AddObject(record);
                }
            }


            //foreach (IN20500_pgGetCompany_Result deleted in lstgrd2.Deleted)
            //{
            //    var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
            //    if (del != null)
            //    {
            //        _db.IN_InvtCpny.DeleteObject(del);

            //    }

            //}
            //foreach (IN20500_pgGetCompany_Result created in lstgrd2.Created)
            //{
            //    var record = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == created.CpnyID).FirstOrDefault();
            //    if (record == null)
            //    {
            //        record = new IN_InvtCpny();
            //        record.InvtID = invtID;
            //        record.CpnyID = created.CpnyID;


            //        if (record.InvtID != "" && record.CpnyID != "")
            //        {

            //            _db.IN_InvtCpny.AddObject(record);
            //        }
            //    }
            //}



            //foreach (IN20500_pgGetCompany_Result updated in lstgrd2.Updated)
            //{
            //    var record = _db.IN_InvtCpny.Where(p => p.InvtID == invtID && p.CpnyID == updated.CpnyID).FirstOrDefault();
            //    if (record != null)
            //    {
            //    }
            //    else
            //    {
            //        record = new IN_InvtCpny();
            //        record.InvtID = invtID;
            //        record.CpnyID = updated.CpnyID;
            //        _db.IN_InvtCpny.AddObject(record);
            //    }
            //}





            foreach (IN_Inventory updated in lstheader.Updated)
            {
                // Get the image path

                string images = getPathThenUploadImage(updated, invtID);
                string media = getPathMedia(updated, invtID, mediaExist);
                var objHeader = _db.IN_Inventory.Where(p => p.InvtID == updated.InvtID).FirstOrDefault();
                if (objHeader != null)
                {
                    //updating
                    if (hadChild == 0)
                    {

                        objHeader.BarCode = BarCode;
                        if (handle == "N" || handle == "")
                        {
                            objHeader.ApproveStatus = approveStatus;
                        }
                        else if (handle == "A")
                        {
                            objHeader.ApproveStatus = "C";
                        }
                        else if (handle == "I")
                        {
                            objHeader.ApproveStatus = "H";
                        }


                        if (approveStatus == "")
                        {
                            objHeader.ApproveStatus = "H";
                        }
                        objHeader.Descr = Descr;
                        objHeader.Descr1 = Descr1;
                        objHeader.Status = Status;



                        objHeader.Public = Convert.ToBoolean(Public);
                        if (Convert.ToBoolean(Public) == true)
                        {
                            var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
                            for (int i = 0; i < del.Count; i++)
                            {

                                _db.IN_InvtCpny.DeleteObject(del[i]);

                            }
                            //foreach (IN_ProdClassCpny proclass in del)
                            //{
                            //    _db.IN_ProdClassCpny.DeleteObject(proclass);
                            //}
                        }
                        //tab1

                        objHeader.ClassID = ClassID;
                        if (StkItem == true)
                        {
                            objHeader.StkItem = 1;
                        }
                        else
                        {
                            objHeader.StkItem = 0;
                        }
                        // ???
                        objHeader.PriceClassID = PriceClassID;
                        objHeader.InvtType = InvtType;

                        objHeader.Source = Source;
                        objHeader.ValMthd = ValMthd;
                        objHeader.LotSerTrack = LotSerTrack;
                        objHeader.Buyer = Buyer;
                        objHeader.StkUnit = StkUnit;
                        objHeader.DfltPOUnit = DfltPOUnit;
                        objHeader.DfltSOUnit = DfltSOUnit;
                        objHeader.MaterialType = MaterialType;
                        //objHeader.DfltSite = created.DfltSite;
                        objHeader.TaxCat = DfltSlsTaxCat;

                        //tab 2

                        objHeader.Color = Color;

                        objHeader.PrePayPct = Convert.ToDouble(PrePayPct);
                        objHeader.Size = Size;
                        objHeader.POFee = Convert.ToDouble(POFee);
                        objHeader.Style = Style;
                        objHeader.SOFee = Convert.ToDouble(SOFee);
                        objHeader.StkVol = Convert.ToDouble(StkVol);
                        objHeader.VendID1 = Vendor1;
                        objHeader.StkWt = Convert.ToDouble(StkWt);
                        objHeader.VendID2 = Vendor2;
                        objHeader.StkWtUnit = StkWtUnit;
                        objHeader.LossRate00 = Convert.ToDouble(LossRate00);
                        objHeader.SOPrice = Convert.ToDouble(SOPrice);
                        objHeader.LossRate01 = Convert.ToDouble(LossRate01);
                        objHeader.POPrice = Convert.ToDouble(POPrice);
                        objHeader.LossRate02 = Convert.ToDouble(LossRate02);
                        objHeader.IRSftyStkDays = Convert.ToDouble(IRSftyStkDays);
                        objHeader.LossRate03 = Convert.ToDouble(LossRate03);
                        objHeader.IRSftyStkPct = Convert.ToDouble(IRSftyStkPct);
                        objHeader.IRSftyStkQty = Convert.ToDouble(IRSftyStkQty);
                        objHeader.IROverStkQty = Convert.ToDouble(IROverStkQty);

                        //tab 3

                        objHeader.SerAssign = SerAssign;
                        objHeader.LotSerIssMthd = LotSerIssMthd;
                        objHeader.ShelfLife = Convert.ToInt16(ShelfLife);
                        objHeader.WarrantyDays = Convert.ToInt16(WarrantyDays);
                        objHeader.LotSerFxdTyp = LotSerFxdTyp;
                        objHeader.LotSerFxdLen = Convert.ToInt16(LotSerFxdLen);
                        objHeader.LotSerFxdVal = LotSerFxdVal;
                        objHeader.LotSerNumLen = Convert.ToInt16(LotSerNumLen);
                        objHeader.LotSerNumVal = LotSerNumVal;
                        //Node




                        //Image and Media

                        objHeader.Picture = images;
                        objHeader.Media = media;

                        //String[] nodeid = nodeID.Split('-');
                        //objHeader.NodeID = nodeid[0];
                        //objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                        objHeader.LUpd_DateTime = DateTime.Now;
                        objHeader.LUpd_Prog = screenNbr;
                        objHeader.LUpd_User = Current.UserName;
                        //UpdatingHeader(created, ref objHeader);

                        _db.SaveChanges();
                    }
                    else
                    {
                        objHeader.BarCode = BarCode;
                        if (handle == "N" || handle == "")
                        {
                            objHeader.ApproveStatus = approveStatus;
                        }
                        else if (handle == "A")
                        {
                            objHeader.ApproveStatus = "C";
                        }
                        else if (handle == "I")
                        {
                            objHeader.ApproveStatus = "H";
                        }


                        if (approveStatus == "")
                        {
                            objHeader.ApproveStatus = "H";
                        }
                        objHeader.Descr = Descr;
                        objHeader.Descr1 = Descr1;
                        objHeader.Status = Status;



                        objHeader.Public = Convert.ToBoolean(Public);
                        if (Convert.ToBoolean(Public) == true)
                        {
                            var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
                            for (int i = 0; i < del.Count; i++)
                            {

                                _db.IN_InvtCpny.DeleteObject(del[i]);

                            }
                            //foreach (IN_ProdClassCpny proclass in del)
                            //{
                            //    _db.IN_ProdClassCpny.DeleteObject(proclass);
                            //}
                        }
                        //tab1

                        objHeader.ClassID = ClassID;
                        if (StkItem == true)
                        {
                            objHeader.StkItem = 1;
                        }
                        else
                        {
                            objHeader.StkItem = 0;
                        }
                        // ???
                        objHeader.PriceClassID = PriceClassID;
                        objHeader.InvtType = InvtType;

                        objHeader.Source = Source;
                        objHeader.ValMthd = ValMthd;
                        objHeader.LotSerTrack = LotSerTrack;
                        objHeader.Buyer = Buyer;
                        objHeader.StkUnit = StkUnit;
                        objHeader.DfltPOUnit = DfltPOUnit;
                        objHeader.DfltSOUnit = DfltSOUnit;
                        objHeader.MaterialType = MaterialType;
                        //objHeader.DfltSite = created.DfltSite;
                        objHeader.TaxCat = DfltSlsTaxCat;

                        //tab 2

                        objHeader.Color = Color;

                        objHeader.PrePayPct = Convert.ToDouble(PrePayPct);
                        objHeader.Size = Size;
                        objHeader.POFee = Convert.ToDouble(POFee);
                        objHeader.Style = Style;
                        objHeader.SOFee = Convert.ToDouble(SOFee);
                        objHeader.StkVol = Convert.ToDouble(StkVol);
                        objHeader.VendID1 = Vendor1;
                        objHeader.StkWt = Convert.ToDouble(StkWt);
                        objHeader.VendID2 = Vendor2;
                        objHeader.StkWtUnit = StkWtUnit;
                        objHeader.LossRate00 = Convert.ToDouble(LossRate00);
                        objHeader.SOPrice = Convert.ToDouble(SOPrice);
                        objHeader.LossRate01 = Convert.ToDouble(LossRate01);
                        objHeader.POPrice = Convert.ToDouble(POPrice);
                        objHeader.LossRate02 = Convert.ToDouble(LossRate02);
                        objHeader.IRSftyStkDays = Convert.ToDouble(IRSftyStkDays);
                        objHeader.LossRate03 = Convert.ToDouble(LossRate03);
                        objHeader.IRSftyStkPct = Convert.ToDouble(IRSftyStkPct);
                        objHeader.IRSftyStkQty = Convert.ToDouble(IRSftyStkQty);
                        objHeader.IROverStkQty = Convert.ToDouble(IROverStkQty);

                        //tab 3

                        objHeader.SerAssign = SerAssign;
                        objHeader.LotSerIssMthd = LotSerIssMthd;
                        objHeader.ShelfLife = Convert.ToInt16(ShelfLife);
                        objHeader.WarrantyDays = Convert.ToInt16(WarrantyDays);
                        objHeader.LotSerFxdTyp = LotSerFxdTyp;
                        objHeader.LotSerFxdLen = Convert.ToInt16(LotSerFxdLen);
                        objHeader.LotSerFxdVal = LotSerFxdVal;
                        objHeader.LotSerNumLen = Convert.ToInt16(LotSerNumLen);
                        objHeader.LotSerNumVal = LotSerNumVal;
                        //Node

                        String[] nodeid = nodeID.Split('-');
                        if (objHeader.NodeID != nodeid[0])
                        {
                            tmpChangeTreeDic = "1";
                        }
                        else
                        {
                            tmpChangeTreeDic = "0";
                        }
                        objHeader.NodeID = nodeid[0];
                        objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                        var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "I").FirstOrDefault();
                        objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;


                        //Image and Media

                        objHeader.Picture = images;
                        objHeader.Media = media;

                        //String[] nodeid = nodeID.Split('-');
                        //objHeader.NodeID = nodeid[0];
                        //objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                        objHeader.LUpd_DateTime = DateTime.Now;
                        objHeader.LUpd_Prog = screenNbr;
                        objHeader.LUpd_User = Current.UserName;
                        //UpdatingHeader(created, ref objHeader);

                        _db.SaveChanges();
                    }

                }
                else
                {
                    //bo sung code add new copyForm neu update
                    images = getPathThenUploadImageCopyForm(tmpCopyFormImageUrl, invtID);
                    media = getPathMediaCopyForm(tmpCopyFormMedia, invtID, tmpOldFileName);
                    objHeader.InvtID = invtID;
                    objHeader.BarCode = BarCode;
                    if (handle == "N" || handle == "")
                    {
                        objHeader.ApproveStatus = approveStatus;
                    }
                    else if (handle == "A")
                    {
                        objHeader.ApproveStatus = "C";
                    }
                    else if (handle == "I")
                    {
                        objHeader.ApproveStatus = "H";
                    }


                    if (approveStatus == "")
                    {
                        objHeader.ApproveStatus = "H";
                    }

                    objHeader.Descr = Descr;
                    objHeader.Descr1 = Descr1;
                    objHeader.Status = Status;



                    objHeader.Public = Convert.ToBoolean(Public);
                    if (Convert.ToBoolean(Public) == true)
                    {
                        var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
                        for (int i = 0; i < del.Count; i++)
                        {

                            _db.IN_InvtCpny.DeleteObject(del[i]);

                        }
                        //foreach (IN_ProdClassCpny proclass in del)
                        //{
                        //    _db.IN_ProdClassCpny.DeleteObject(proclass);
                        //}
                    }
                    //tab1

                    objHeader.ClassID = ClassID;
                    if (StkItem == true)
                    {
                        objHeader.StkItem = 1;
                    }
                    else
                    {
                        objHeader.StkItem = 0;
                    }
                    // ???
                    objHeader.PriceClassID = PriceClassID;
                    objHeader.InvtType = InvtType;

                    objHeader.Source = Source;
                    objHeader.ValMthd = ValMthd;
                    objHeader.LotSerTrack = LotSerTrack;
                    objHeader.Buyer = Buyer;
                    objHeader.StkUnit = StkUnit;
                    objHeader.DfltPOUnit = DfltPOUnit;
                    objHeader.DfltSOUnit = DfltSOUnit;
                    objHeader.MaterialType = MaterialType;
                    //objHeader.DfltSite = created.DfltSite;
                    objHeader.TaxCat = DfltSlsTaxCat;

                    //tab 2

                    objHeader.Color = Color;
                    //if(created.PrePayPct == "")
                    //{
                    //    objHeader.PrepayPct = 0;
                    //}else{
                    //    objHeader.PrePayPct = created.PrePayPct;
                    //}
                    objHeader.PrePayPct = Convert.ToDouble(PrePayPct);
                    objHeader.Size = Size;
                    objHeader.POFee = Convert.ToDouble(POFee);
                    objHeader.Style = Style;
                    objHeader.SOFee = Convert.ToDouble(SOFee);
                    objHeader.StkVol = Convert.ToDouble(StkVol);
                    objHeader.VendID1 = Vendor1;
                    objHeader.StkWt = Convert.ToDouble(StkWt);
                    objHeader.VendID2 = Vendor2;
                    objHeader.StkWtUnit = StkWtUnit;
                    objHeader.LossRate00 = Convert.ToDouble(LossRate00);
                    objHeader.SOPrice = Convert.ToDouble(SOPrice);
                    objHeader.LossRate01 = Convert.ToDouble(LossRate01);
                    objHeader.POPrice = Convert.ToDouble(POPrice);
                    objHeader.LossRate02 = Convert.ToDouble(LossRate02);
                    objHeader.IRSftyStkDays = Convert.ToDouble(IRSftyStkDays);
                    objHeader.LossRate03 = Convert.ToDouble(LossRate03);
                    objHeader.IRSftyStkPct = Convert.ToDouble(IRSftyStkPct);
                    objHeader.IRSftyStkQty = Convert.ToDouble(IRSftyStkQty);
                    objHeader.IROverStkQty = Convert.ToDouble(IROverStkQty);

                    //tab 3

                    objHeader.SerAssign = SerAssign;
                    objHeader.LotSerIssMthd = LotSerIssMthd;
                    objHeader.ShelfLife = Convert.ToInt16(ShelfLife);
                    objHeader.WarrantyDays = Convert.ToInt16(WarrantyDays);
                    objHeader.LotSerFxdTyp = LotSerFxdTyp;
                    objHeader.LotSerFxdLen = Convert.ToInt16(LotSerFxdLen);
                    objHeader.LotSerFxdVal = LotSerFxdVal;
                    objHeader.LotSerNumLen = Convert.ToInt16(LotSerNumLen);
                    objHeader.LotSerNumVal = LotSerNumVal;


                    String[] nodeid = nodeID.Split('-');
                    objHeader.NodeID = nodeid[0];
                    objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                    var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "I").FirstOrDefault();
                    objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;
                    objHeader.Exported = 0;

                    //Image and Media

                    objHeader.Picture = images;
                    objHeader.Media = media;

                    //
                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    objHeader.tstamp = new byte[0];
                    objHeader.LUpd_DateTime = DateTime.Now;
                    objHeader.LUpd_Prog = screenNbr;
                    objHeader.LUpd_User = Current.UserName;




                    _db.IN_Inventory.AddObject(objHeader);
                    _db.SaveChanges();




                }


                // If there is a change in handling status (keepStatus is False),
                // add a new pending task with an approved handle.



                // ===============================================================

                // Get out of the loop (only update the first data)

            }
            foreach (IN_Inventory created in lstheader.Created)
            {
                string images = getPathThenUploadImage(created, invtID);
                string media = getPathMedia(created, invtID, mediaExist);
                var objHeader = _db.IN_Inventory.Where(p => p.InvtID == invtID).FirstOrDefault();
                if (objHeader == null)
                {
                    if (hadChild != 0)
                    {
                        objHeader = new IN_Inventory();

                        objHeader.InvtID = invtID;
                        objHeader.BarCode = BarCode;
                        if (handle == "N" || handle == "")
                        {
                            objHeader.ApproveStatus = approveStatus;
                        }
                        else if (handle == "A")
                        {
                            objHeader.ApproveStatus = "C";
                        }
                        else if (handle == "I")
                        {
                            objHeader.ApproveStatus = "H";
                        }


                        if (approveStatus == "")
                        {
                            objHeader.ApproveStatus = "H";
                        }

                        objHeader.Descr = Descr;
                        objHeader.Descr1 = Descr1;
                        objHeader.Status = Status;



                        objHeader.Public = Convert.ToBoolean(Public);
                        if (Convert.ToBoolean(Public) == true)
                        {
                            var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
                            for (int i = 0; i < del.Count; i++)
                            {

                                _db.IN_InvtCpny.DeleteObject(del[i]);

                            }
                            //foreach (IN_ProdClassCpny proclass in del)
                            //{
                            //    _db.IN_ProdClassCpny.DeleteObject(proclass);
                            //}
                        }
                        //tab1

                        objHeader.ClassID = ClassID;
                        if (StkItem == true)
                        {
                            objHeader.StkItem = 1;
                        }
                        else
                        {
                            objHeader.StkItem = 0;
                        }
                        // ???
                        objHeader.PriceClassID = PriceClassID;
                        objHeader.InvtType = InvtType;

                        objHeader.Source = Source;
                        objHeader.ValMthd = ValMthd;
                        objHeader.LotSerTrack = LotSerTrack;
                        objHeader.Buyer = Buyer;
                        objHeader.StkUnit = StkUnit;
                        objHeader.DfltPOUnit = DfltPOUnit;
                        objHeader.DfltSOUnit = DfltSOUnit;
                        objHeader.MaterialType = MaterialType;
                        //objHeader.DfltSite = created.DfltSite;
                        objHeader.TaxCat = DfltSlsTaxCat;

                        //tab 2

                        objHeader.Color = Color;
                        //if(created.PrePayPct == "")
                        //{
                        //    objHeader.PrepayPct = 0;
                        //}else{
                        //    objHeader.PrePayPct = created.PrePayPct;
                        //}
                        objHeader.PrePayPct = Convert.ToDouble(PrePayPct);
                        objHeader.Size = Size;
                        objHeader.POFee = Convert.ToDouble(POFee);
                        objHeader.Style = Style;
                        objHeader.SOFee = Convert.ToDouble(SOFee);
                        objHeader.StkVol = Convert.ToDouble(StkVol);
                        objHeader.VendID1 = Vendor1;
                        objHeader.StkWt = Convert.ToDouble(StkWt);
                        objHeader.VendID2 = Vendor2;
                        objHeader.StkWtUnit = StkWtUnit;
                        objHeader.LossRate00 = Convert.ToDouble(LossRate00);
                        objHeader.SOPrice = Convert.ToDouble(SOPrice);
                        objHeader.LossRate01 = Convert.ToDouble(LossRate01);
                        objHeader.POPrice = Convert.ToDouble(POPrice);
                        objHeader.LossRate02 = Convert.ToDouble(LossRate02);
                        objHeader.IRSftyStkDays = Convert.ToDouble(IRSftyStkDays);
                        objHeader.LossRate03 = Convert.ToDouble(LossRate03);
                        objHeader.IRSftyStkPct = Convert.ToDouble(IRSftyStkPct);
                        objHeader.IRSftyStkQty = Convert.ToDouble(IRSftyStkQty);
                        objHeader.IROverStkQty = Convert.ToDouble(IROverStkQty);

                        //tab 3

                        objHeader.SerAssign = SerAssign;
                        objHeader.LotSerIssMthd = LotSerIssMthd;
                        objHeader.ShelfLife = Convert.ToInt16(ShelfLife);
                        objHeader.WarrantyDays = Convert.ToInt16(WarrantyDays);
                        objHeader.LotSerFxdTyp = LotSerFxdTyp;
                        objHeader.LotSerFxdLen = Convert.ToInt16(LotSerFxdLen);
                        objHeader.LotSerFxdVal = LotSerFxdVal;
                        objHeader.LotSerNumLen = Convert.ToInt16(LotSerNumLen);
                        objHeader.LotSerNumVal = LotSerNumVal;


                        String[] nodeid = nodeID.Split('-');
                        objHeader.NodeID = nodeid[0];
                        objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                        var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "I").FirstOrDefault();
                        objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;
                        objHeader.Exported = 0;
                        //tmpChangeTreeDic = "1";
                        //Image and Media

                        objHeader.Picture = images;
                        objHeader.Media = media;

                        //
                        objHeader.Crtd_DateTime = DateTime.Now;
                        objHeader.Crtd_Prog = screenNbr;
                        objHeader.Crtd_User = Current.UserName;
                        objHeader.tstamp = new byte[0];
                        UpdatingHeader(created, ref objHeader);




                        _db.IN_Inventory.AddObject(objHeader);
                        _db.SaveChanges();




                    }
                    else
                    {
                        return Json(new { success = false, code = "8001" }, JsonRequestBehavior.AllowGet);
                    }

                }
            }




            var objtmp = _db.IN_Inventory.Where(p => p.InvtID == invtIDCopyForm).FirstOrDefault();
            if (objtmp != null)
            {
                objtmp.Picture = tmpImageForUpload;
                if (tmpMediaDelete == 1)
                {
                    objtmp.Media = "";
                }
                string images = getPathThenUploadImage(objtmp, invtID);
                string media = getPathMedia(objtmp, invtID, mediaExist);
                if (tmpImageDelete == 0)
                {
                    objtmp.Picture = images;

                }
                else if (tmpImageDelete == 1)
                {
                    objtmp.Picture = "";
                }
                if (tmpMediaDelete == 0)
                {
                    objtmp.Media = media;
                }
                else if (tmpMediaDelete == 1)
                {
                    objtmp.Media = "";
                }
                if (handle == "N" || handle == "")
                {
                    objtmp.ApproveStatus = approveStatus;
                }
                else if (handle == "A")
                {
                    objtmp.ApproveStatus = "C";
                }
                else if (handle == "I")
                {
                    objtmp.ApproveStatus = "H";
                }
                if (lstheader.Updated.Count == 0 && hadChild != 0)
                {
                    String[] nodeid = nodeID.Split('-');
                    if (objtmp.NodeID != nodeid[0])
                    {
                        tmpChangeTreeDic = "1";
                    }
                    else
                    {
                        tmpChangeTreeDic = "0";
                    }
                    objtmp.NodeID = nodeid[0];

                    objtmp.NodeLevel = Convert.ToInt16(nodeLevel);
                    var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "I").FirstOrDefault();
                    objtmp.ParentRecordID = searchparentRecordID.ParentRecordID;
                    //tmpChangeTreeDic = "1";
                }
            }
            else if (objtmp == null)
            {
                if (tmpCopyForm == "1" && lstheader.Updated.Count == 0 && hadChild != 0)
                {
                    string images = getPathThenUploadImageCopyForm(tmpCopyFormImageUrl, invtID);
                    string media = getPathMediaCopyForm(tmpCopyFormMedia, invtIDCopyForm, tmpOldFileName);
                    objtmp = new IN_Inventory();
                    objtmp.InvtID = invtIDCopyForm;
                    objtmp.BarCode = BarCode;
                    if (handle == "N" || handle == "")
                    {
                        objtmp.ApproveStatus = approveStatus;
                    }
                    else if (handle == "A")
                    {
                        objtmp.ApproveStatus = "C";
                    }
                    else if (handle == "I")
                    {
                        objtmp.ApproveStatus = "H";
                    }


                    if (approveStatus == "")
                    {
                        objtmp.ApproveStatus = "H";
                    }

                    objtmp.Descr = Descr;
                    objtmp.Descr1 = Descr1;
                    objtmp.Status = Status;



                    objtmp.Public = Convert.ToBoolean(Public);
                    if (Convert.ToBoolean(Public) == true)
                    {
                        var del = _db.IN_InvtCpny.Where(p => p.InvtID == invtID).ToList();
                        for (int i = 0; i < del.Count; i++)
                        {

                            _db.IN_InvtCpny.DeleteObject(del[i]);

                        }
                        //foreach (IN_ProdClassCpny proclass in del)
                        //{
                        //    _db.IN_ProdClassCpny.DeleteObject(proclass);
                        //}
                    }
                    //tab1

                    objtmp.ClassID = ClassID;
                    if (StkItem == true)
                    {
                        objtmp.StkItem = 1;
                    }
                    else
                    {
                        objtmp.StkItem = 0;
                    }
                    // ???
                    objtmp.PriceClassID = PriceClassID;
                    objtmp.InvtType = InvtType;

                    objtmp.Source = Source;
                    objtmp.ValMthd = ValMthd;
                    objtmp.LotSerTrack = LotSerTrack;
                    objtmp.Buyer = Buyer;
                    objtmp.StkUnit = StkUnit;
                    objtmp.DfltPOUnit = DfltPOUnit;
                    objtmp.DfltSOUnit = DfltSOUnit;
                    objtmp.MaterialType = MaterialType;
                    //objHeader.DfltSite = created.DfltSite;
                    objtmp.TaxCat = DfltSlsTaxCat;

                    //tab 2

                    objtmp.Color = Color;
                    //if(created.PrePayPct == "")
                    //{
                    //    objHeader.PrepayPct = 0;
                    //}else{
                    //    objHeader.PrePayPct = created.PrePayPct;
                    //}
                    objtmp.PrePayPct = Convert.ToDouble(PrePayPct);
                    objtmp.Size = Size;
                    objtmp.POFee = Convert.ToDouble(POFee);
                    objtmp.Style = Style;
                    objtmp.SOFee = Convert.ToDouble(SOFee);
                    objtmp.StkVol = Convert.ToDouble(StkVol);
                    objtmp.VendID1 = Vendor1;
                    objtmp.StkWt = Convert.ToDouble(StkWt);
                    objtmp.VendID2 = Vendor2;
                    objtmp.StkWtUnit = StkWtUnit;
                    objtmp.LossRate00 = Convert.ToDouble(LossRate00);
                    objtmp.SOPrice = Convert.ToDouble(SOPrice);
                    objtmp.LossRate01 = Convert.ToDouble(LossRate01);
                    objtmp.POPrice = Convert.ToDouble(POPrice);
                    objtmp.LossRate02 = Convert.ToDouble(LossRate02);
                    objtmp.IRSftyStkDays = Convert.ToDouble(IRSftyStkDays);
                    objtmp.LossRate03 = Convert.ToDouble(LossRate03);
                    objtmp.IRSftyStkPct = Convert.ToDouble(IRSftyStkPct);
                    objtmp.IRSftyStkQty = Convert.ToDouble(IRSftyStkQty);
                    objtmp.IROverStkQty = Convert.ToDouble(IROverStkQty);

                    //tab 3

                    objtmp.SerAssign = SerAssign;
                    objtmp.LotSerIssMthd = LotSerIssMthd;
                    objtmp.ShelfLife = Convert.ToInt16(ShelfLife);
                    objtmp.WarrantyDays = Convert.ToInt16(WarrantyDays);
                    objtmp.LotSerFxdTyp = LotSerFxdTyp;
                    objtmp.LotSerFxdLen = Convert.ToInt16(LotSerFxdLen);
                    objtmp.LotSerFxdVal = LotSerFxdVal;
                    objtmp.LotSerNumLen = Convert.ToInt16(LotSerNumLen);
                    objtmp.LotSerNumVal = LotSerNumVal;


                    String[] nodeid = nodeID.Split('-');
                    objtmp.NodeID = nodeid[0];
                    objtmp.NodeLevel = Convert.ToInt16(nodeLevel);
                    var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "I").FirstOrDefault();
                    objtmp.ParentRecordID = searchparentRecordID.ParentRecordID;
                    objtmp.Exported = 0;

                    //Image and Media

                    objtmp.Picture = images;
                    objtmp.Media = media;

                    //
                    objtmp.Crtd_DateTime = DateTime.Now;
                    objtmp.Crtd_Prog = screenNbr;
                    objtmp.Crtd_User = Current.UserName;
                    objtmp.tstamp = new byte[0];
                    objtmp.LUpd_DateTime = DateTime.Now;
                    objtmp.LUpd_Prog = screenNbr;
                    objtmp.LUpd_User = Current.UserName;




                    _db.IN_Inventory.AddObject(objtmp);
                    _db.SaveChanges();
                }
                else
                {
                    return Json(new { success = false, code = "8001" }, JsonRequestBehavior.AllowGet);
                }

            }





            _db.SaveChanges();
            //this.Direct();


            if (hadChild != 0)
            {
                return Json(new { success = true, value = invtID, value2 = Descr, value3 = "addNew", value4 = tmpChangeTreeDic, value5 = tmpSelectedNode }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, value = invtID, value2 = Descr, value3 = "update" }, JsonRequestBehavior.AllowGet);
            }

        }






        [DirectMethod]
        public ActionResult IN20500Delete(string invtID)
        {
            var inv = _db.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);


            _db.IN_Inventory.DeleteObject(inv);



            _db.SaveChanges();
            return this.Direct();
        }


        private void UpdatingHeader(IN_Inventory s, ref IN_Inventory d)
        {




            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }


        [DirectMethod]
        public ActionResult GetImages(string Name)
        {
            string typeFile = "";
            if (Name.EndsWith(".jpg"))
            {
                typeFile = "jpg";
            }
            else if (Name.EndsWith(".png"))
            {
                typeFile = "png";
            }
            else if (Name.EndsWith(".gif"))
            {
                typeFile = "gif";
            }
            var Images = this.GetCmp<Image>("imgPPCStorePicReq");
            string a = getStringImage(Name);

            Images.ImageUrl = @"data:image/" + typeFile + ";base64," + a;

            return this.Direct();
        }
        private string getStringImage(string name)
        {
            var a = IN20500ImgHelper.IN20500GetImage(name, PathImage, string.IsNullOrWhiteSpace(PathImage) ? false : true);
            if (a == null)
            {
                return string.Empty;
            }
            else
            {
                return Convert.ToBase64String(a);
            }
        }

        private byte[] getByteImage(string dataIamge)
        {
            return Encoding.ASCII.GetBytes(dataIamge);
        }

        [DirectMethod]
        public ActionResult Upload()
        {
            if (this.GetCmp<FileUploadField>("NamePPCStorePicReq").HasFile)
            {
                var FileUpload1 = this.GetCmp<FileUploadField>("NamePPCStorePicReq");//.PostedFile.InputStream;
                string typeFile = "";
                if (FileUpload1.PostedFile.FileName.EndsWith(".jpg"))
                {
                    typeFile = "jpg";
                }
                else if (FileUpload1.PostedFile.FileName.EndsWith(".png"))
                {
                    typeFile = "png";
                }
                else if (FileUpload1.PostedFile.FileName.EndsWith(".gif"))
                {
                    typeFile = "gif";
                }
                if (typeFile != "")
                {
                    var Images = this.GetCmp<Image>("imgPPCStorePicReq");
                    var txtImages = this.GetCmp<TextField>("PPCStorePicReq");

                    int intLength = Convert.ToInt32(FileUpload1.PostedFile.InputStream.Length);
                    byte[] arrContent = new byte[intLength];
                    string imgType = FileUpload1.PostedFile.ContentType;

                    FileUpload1.PostedFile.InputStream.Read(arrContent, 0, intLength);
                    Images.ImageUrl = @"data:image/" + typeFile + ";base64," + Convert.ToBase64String(arrContent); ;
                    txtImages.Text = FileUpload1.PostedFile.FileName;
                }
                else
                {
                    X.Msg.Show(new MessageBoxConfig
                    {
                        Buttons = MessageBox.Button.OK,
                        Icon = MessageBox.Icon.ERROR,
                        Title = "Fail",
                        Message = "File format .jpg,.png,.gif"
                    });
                }
            }
            else
            {
                X.Msg.Show(new MessageBoxConfig
                {
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.ERROR,
                    Title = "Fail",
                    Message = "No file uploaded"
                });
            }
            DirectResult result = new DirectResult();
            result.IsUpload = true;

            return result;
        }


        private string getPathThenUploadImage(IN_Inventory inventory, string invtID)
        {
            string images = string.Format("{0}.jpg", invtID);

            if (!string.IsNullOrWhiteSpace(inventory.Picture) && !inventory.Picture.Contains(".jpg"))
            {
                string strImage = inventory.Picture
                    .Replace("data:image/jpg;base64,", "")
                    .Replace("data:image/png;base64,", "")
                    .Replace("data:image/gif;base64,", "");

                // Upload a new file.
                IN20500ImgHelper.IN20500UploadImage(images,
                    Convert.FromBase64CharArray(strImage.ToCharArray(), 0, strImage.Length),
                    PathImage, IsConfig);
            }
            else if (!string.IsNullOrWhiteSpace(inventory.Picture) && inventory.Picture.Contains(".jpg"))
            {
                images = inventory.Picture;
            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteFile(images, PathImage, IsConfig);
            }

            return images;
        }

        private string getPathThenUploadImageCopyForm(string tmpCopyFormImageUrl, string invtID)
        {
            string images = string.Format("{0}.jpg", invtID);

            if (!string.IsNullOrWhiteSpace(tmpCopyFormImageUrl) && !tmpCopyFormImageUrl.Contains(".jpg"))
            {
                string strImage = tmpCopyFormImageUrl
                    .Replace("data:image/jpg;base64,", "")
                    .Replace("data:image/png;base64,", "")
                    .Replace("data:image/gif;base64,", "");

                // Upload a new file.
                IN20500ImgHelper.IN20500UploadImage(images,
                    Convert.FromBase64CharArray(strImage.ToCharArray(), 0, strImage.Length),
                    PathImage, IsConfig);
            }
            else if (!string.IsNullOrWhiteSpace(tmpCopyFormImageUrl) && tmpCopyFormImageUrl.Contains(".jpg"))
            {
                images = tmpCopyFormImageUrl;
            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteFile(images, PathImage, IsConfig);
            }

            return images;
        }
        [DirectMethod]
        public ActionResult PlayMedia(string fileVideo)
        {
            var pathMedia = "/eBizWeb/Media/" + fileVideo;
            //var pathMedia = PathImage.Substring(0, PathImage.Length - 14) + "Media\\" + fileVideo;
            //var pathMedia = "/DevProjects/FrameworkWeb/App/Media/a.mp4";
            //var pathMedia = "file://192.168.130.4/DevProjects/FrameworkWeb/App/Media/a.mp4";
            //var pathMedia = "http://techslides.com/demos/sample-videos/small.mp4";

            Window win = new Window
            {
                ID = "Window1",
                Title = "Media",
                Height = 520,
                Width = 640,
                Closable = true,
                CloseAction = CloseAction.Destroy,
                Modal = true,

                Html = "<video width='640' height='480' controls autoplay><source src='" + pathMedia + "' type='video/mp4'></video>"

            };

            win.Render(RenderMode.RenderTo);
            //return Json(new { success = true, value = win }, JsonRequestBehavior.AllowGet);
            return this.Direct();

        }



        [DirectMethod]
        public ActionResult UploadMedia(string invtID)
        {
            string fullFileName = "";
            if (this.GetCmp<FileUploadField>("NamePPCStoreMediaReq").HasFile)
            {
                var FileUpload1 = this.GetCmp<FileUploadField>("NamePPCStoreMediaReq");//.PostedFile.InputStream;
                string typeFile = "";
                if (FileUpload1.PostedFile.FileName.EndsWith(".mp4"))
                {
                    typeFile = "mp4";
                }
                else if (FileUpload1.PostedFile.FileName.EndsWith(".wmv"))
                {
                    typeFile = "wmv";
                }

                if (typeFile != "")
                {
                    //string filePath = "C:\\nhac\\a.mp4";

                    //string fullFileName = "\\\\192.168.130.4\\DevProjects\\FrameworkWeb\\App\\Media\\a.mp4";
                    fullFileName = invtID + "." + typeFile;
                    //string fullFileName = PathImage + invtID + "_" + Descr + "." + typeFile;
                    // doi icon anh
                    var Images = this.GetCmp<Image>("imgPPCStoreMediaReq");
                    string a = getStringImage("anh1.jpg");
                    Images.ImageUrl = @"data:image/" + "jpg" + ";base64," + a;
                    b = Images.ImageUrl;

                    // upload media len
                    int intLength = Convert.ToInt32(FileUpload1.PostedFile.InputStream.Length);
                    byte[] arrContent = new byte[intLength];
                    string imgType = FileUpload1.PostedFile.ContentType;

                    FileUpload1.PostedFile.InputStream.Read(arrContent, 0, intLength);

                    //txtImages.Text = FileUpload1.PostedFile.FileName;
                    IN20500ImgHelper.IN20500UploadMedia(fullFileName, arrContent, PathImage, IsConfig);
                    //File.WriteAllBytes(fullFileName, arrContent);
                }
                else
                {
                    X.Msg.Show(new MessageBoxConfig
                    {
                        Buttons = MessageBox.Button.OK,
                        Icon = MessageBox.Icon.ERROR,
                        Title = "Fail",
                        Message = "File format .mp4,.wmv"
                    });
                }
            }
            else
            {
                X.Msg.Show(new MessageBoxConfig
                {
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.ERROR,
                    Title = "Fail",
                    Message = "No file uploaded"
                });
            }

            return Json(new { success = true, value = b, value2 = fullFileName }, JsonRequestBehavior.AllowGet);



        }


        [DirectMethod]
        public ActionResult SetMediaImage()
        {

            // doi icon anh
            var Images = this.GetCmp<Image>("imgPPCStoreMediaReq");
            string a = getStringImage("anh1.jpg");
            Images.ImageUrl = @"data:image/" + "jpg" + ";base64," + a;
            b = Images.ImageUrl;
            return Json(new { success = true, value = b }, JsonRequestBehavior.AllowGet);
        }




        private string getPathMedia(IN_Inventory inventory, string invtID, string mediaExist)
        {
            string media = "";
            if (mediaExist != "")
            {
                media = string.Format("{0}.mp4", invtID);
            }


            if (!string.IsNullOrWhiteSpace(inventory.Media) && inventory.Media.Contains(".mp4"))
            {
                media = inventory.Media;
            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteMedia(media, PathImage, IsConfig);
            }

            return media;
        }

        private string getPathMediaCopyForm(string tmpCopyFormMedia, string invtID, string tmpOldFileName)
        {
            string media = string.Format("{0}.mp4", invtID);


            if (!string.IsNullOrWhiteSpace(media) && media.Contains(".mp4"))
            {



                IN20500ImgHelper.IN20500CopyMedia(media, tmpOldFileName, PathImage, IsConfig);

            }
            else // Images is empty
            {
                // If there is an existing file, delete it.
                IN20500ImgHelper.DeleteMedia(media, PathImage, IsConfig);
            }

            return media;
        }

    }
}

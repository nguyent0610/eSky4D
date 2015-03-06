using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using SendMailService.Web;
using HQSendMailApprove;
namespace AR20400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR20400Controller : Controller
    {
        string screenNbr = "AR20400";
        AR20400Entities _db = Util.CreateObjectContext<AR20400Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        string b = "";
        string tmpChangeTreeDic = "0";
        string brandID = Current.CpnyID;
        string lang = Current.LangID.ToString();
        string userNameLogin = Current.UserName;

        public ActionResult Index()
        {
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            ViewBag.Roles = user.UserTypes;
            //var root = new Node() { };
            //var nodeType = "C";

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

            var tmps = _db.AR20400_pcCustomer(brandID)
                .Where(p => p.NodeID == inactiveHierachy.NodeID
                    && p.ParentRecordID == inactiveHierachy.ParentRecordID
                    && p.NodeLevel == level - 1).ToList();
            var childrenInactiveHierachies = _db.SI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.Type == inactiveHierachy.Type
                    && p.NodeLevel == level).ToList();

            if (tmps != null && tmps.Count > 0)
            {
                foreach (AR20400_pcCustomer_Result tmp in tmps)
                {

                    k++;

                    Node nodetmp = new Node();
                    nodetmp.Text = tmp.CustId + "-" + tmp.CustName;
                    nodetmp.NodeID = tmp.CustId + "-" + tmp.CustName;
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

        //    dang submit ko load duoc lai cai not Root slmTree
        //[DirectMethod]
        //public ActionResult ReloadTree(string cpnyID)
        //{
        //    brandID = cpnyID;

        //    var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
        //    ViewBag.Roles = user.UserTypes;
        //    var root = new Node() { };
        //    var nodeType = "C";

        //    var hierarchy = new SI_Hierarchy()
        //    {
        //        RecordID = 0,
        //        NodeID = "",
        //        ParentRecordID = 0,
        //        NodeLevel = 1,
        //        Descr = "root",
        //        Type = nodeType
        //    };
        //    var z = 0;
        //    Node node = createNode(root, hierarchy, hierarchy.NodeLevel, z);
        //    //var m = ViewData["resultRoot2"];


        //    //return Json(new { success = true  , value = m}, JsonRequestBehavior.AllowGet);
        //    //return PartialView();
        //    //this.GetCmp<TreePanel>("IDTree").SetRootNode(node);
        //    return Json(new { success = true});

        //}

        //dang method co the load duoc node root slmTree
        [DirectMethod]
        public ActionResult ReloadTree(string cpnyID)
        {
            brandID = cpnyID;

            var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            ViewBag.Roles = user.UserTypes;
            var root = new Node() { };
            var nodeType = "C";

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


            //quan trong dung de refresh slmTree
            this.GetCmp<TreePanel>("IDTree").SetRootNode(node);

            return this.Direct();

        }



        public StoreResult GetNodes(string cpnyID)
        {
            NodeCollection nodes = new NodeCollection(false);
            if (cpnyID != "")
            {
                brandID = cpnyID;
            }

            var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
            ViewBag.Roles = user.UserTypes;
            var root = new Node() { };
            var nodeType = "C";

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
            Node nodeRoot = new Node();
            nodeRoot = createNode(root, hierarchy, hierarchy.NodeLevel, z);
            //nodeRoot.NodeID = "Root";
            //ViewData["resultRoot2"] = nodeRoot;
            return this.Store(nodeRoot);
        }

        public ActionResult GetCustomer(String custId, String branchID)
        {
            ViewBag.BusinessDate = DateTime.Now.ToDateShort();
            var rptCustomer = _db.AR_Customer.FirstOrDefault(p => p.CustId == custId && p.BranchID == branchID);
            return this.Store(rptCustomer);
        }


        [DirectMethod]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(FormCollection data, string custID, string handle, string nodeID, int nodeLevel, string parentRecordID, int hadChild, string status, string tmpSelectedNode, string branchID, string custName, bool isNew)
        {
            try
            {
                StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
                ChangeRecords<AR_Customer> lstheader = dataHandler2.BatchObjectData<AR_Customer>();
                var approveHandle = _db.SI_ApprovalFlowHandle
                           .FirstOrDefault(p => p.AppFolID == screenNbr
                                               && p.Status == status
                                               && p.Handle == handle);
                var user = _sys.Users.Where(p => p.UserName.ToUpper() == Current.UserName.ToUpper()).FirstOrDefault();
                var Role = user.UserTypes;
                //var custName = data["txtCustName"];

                //var statusAfterAll = "";
                foreach (AR_Customer updated in lstheader.Updated)
                {
                    // Get the image path


                    var objHeader = _db.AR_Customer.Where(p => p.CustId == custID && p.BranchID == branchID).FirstOrDefault();
                    if (isNew)//new record
                    {
                        if (objHeader != null)
                        {
                            return Json(new { success = false, msgCode = 2000, msgParam = custID });//quang message ma nha cung cap da ton tai ko the them

                        }
                        else
                        {
                            if (hadChild != 0)
                            {
                                objHeader = new AR_Customer();
                                objHeader.CustId = custID;
                                objHeader.BranchID = branchID;

                                if (handle == "N" || handle == "")
                                {
                                    objHeader.Status = status;
                                }
                                else
                                {
                                    objHeader.Status = approveHandle.ToStatus;

                                }
                                if (objHeader.Status == "O")
                                {
                                    X.Msg.Show(new MessageBoxConfig()
                                    {
                                        Message = "Email sent!"
                                    });
                                    Approve.Mail_Approve(screenNbr, custID, Role, objHeader.Status, handle,
                                                 lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                                }
                                //statusAfterAll = objHeader.Status;

                                String[] nodeid = nodeID.Split('-');
                                objHeader.NodeID = nodeid[0];
                                objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                                var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                                objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;

                                objHeader.Crtd_Datetime = DateTime.Now;
                                objHeader.Crtd_Prog = screenNbr;
                                objHeader.Crtd_User = Current.UserName;
                                objHeader.tstamp = new byte[0];

                                UpdatingHeader(updated, ref objHeader);
                                _db.AR_Customer.AddObject(objHeader);
                                _db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        if (objHeader != null)
                        {
                            if (hadChild == 0) // nếu selection tree ko có con
                            {
                                if (objHeader.tstamp.ToHex() == updated.tstamp.ToHex())
                                {
                                    if (handle == "N" || handle == "")
                                    {
                                        objHeader.Status = status;
                                    }
                                    else
                                    {
                                        objHeader.Status = approveHandle.ToStatus;

                                    }
                                    if (objHeader.Status == "O")
                                    {
                                        X.Msg.Show(new MessageBoxConfig()
                                        {
                                            Message = "Email sent!"
                                        });
                                        Approve.Mail_Approve(screenNbr, custID, Role, objHeader.Status, handle,
                                                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                                    }
                                    //Node


                                    //String[] nodeid = nodeID.Split('-');
                                    //objHeader.NodeID = nodeid[0];
                                    //objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                                    UpdatingHeader(updated, ref objHeader);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                                _db.SaveChanges();


                            }
                            else // nếu selection tree có con
                            {
                                if (objHeader.tstamp.ToHex() == updated.tstamp.ToHex())
                                {
                                    if (handle == "N" || handle == "")
                                    {
                                        objHeader.Status = status;
                                    }
                                    else
                                    {
                                        objHeader.Status = approveHandle.ToStatus;

                                    }
                                    if (objHeader.Status == "O")
                                    {
                                        X.Msg.Show(new MessageBoxConfig()
                                        {
                                            Message = "Email sent!"
                                        });
                                        Approve.Mail_Approve(screenNbr, custID, Role, objHeader.Status, handle,
                                                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                                    }
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
                                    var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                                    objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;
                                    UpdatingHeader(updated, ref objHeader);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                                _db.SaveChanges();
                            }

                        }
                    }
                    //if (objHeader != null)
                   // {

                        ////updating
                        //if (hadChild == 0)
                        //{


                        //    if (handle == "N" || handle == "")
                        //    {
                        //        objHeader.Status = status;
                        //    }
                        //    else
                        //    {
                        //        objHeader.Status = approveHandle.ToStatus;

                        //    }
                        //    if (objHeader.Status == "O")
                        //    {
                        //        X.Msg.Show(new MessageBoxConfig()
                        //        {
                        //            Message = "Email sent!"
                        //        });
                        //        Approve.Mail_Approve(screenNbr, custID, Role, objHeader.Status, handle,
                        //                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                        //    }
                        //    //Node


                        //    //String[] nodeid = nodeID.Split('-');
                        //    //objHeader.NodeID = nodeid[0];
                        //    //objHeader.NodeLevel = Convert.ToInt16(nodeLevel);

                        //    UpdatingHeader(updated, ref objHeader);

                        //    _db.SaveChanges();
                        //}
                    //    else
                    //    {
                    //        if (handle == "N" || handle == "")
                    //        {
                    //            objHeader.Status = status;
                    //        }
                    //        else
                    //        {
                    //            objHeader.Status = approveHandle.ToStatus;

                    //        }
                    //        if (objHeader.Status == "O")
                    //        {
                    //            X.Msg.Show(new MessageBoxConfig()
                    //            {
                    //                Message = "Email sent!"
                    //            });
                    //            Approve.Mail_Approve(screenNbr, custID, Role, objHeader.Status, handle,
                    //                         lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                    //        }

                    //        String[] nodeid = nodeID.Split('-');
                    //        if (objHeader.NodeID != nodeid[0])
                    //        {
                    //            tmpChangeTreeDic = "1";
                    //        }
                    //        else
                    //        {
                    //            tmpChangeTreeDic = "0";
                    //        }
                    //        objHeader.NodeID = nodeid[0];
                    //        objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                    //        var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                    //        objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;

                    //        //Image and Media

                    //        //String[] nodeid = nodeID.Split('-');
                    //        //objHeader.NodeID = nodeid[0];
                    //        //objHeader.NodeLevel = Convert.ToInt16(nodeLevel);

                    //        UpdatingHeader(updated, ref objHeader);

                    //        _db.SaveChanges();
                    //    }

                    //}
                    //else
                    //{
                    //    //bo sung code add new copyForm neu update
                    //    objHeader = new AR_Customer();
                    //    objHeader.CustId = custID;
                    //    objHeader.BranchID = branchID;

                    //    if (handle == "N" || handle == "")
                    //    {
                    //        objHeader.Status = status;
                    //    }
                    //    else
                    //    {
                    //        objHeader.Status = approveHandle.ToStatus;

                    //    }
                    //    if (objHeader.Status == "O")
                    //    {
                    //        X.Msg.Show(new MessageBoxConfig()
                    //        {
                    //            Message = "Email sent!"
                    //        });
                    //        Approve.Mail_Approve(screenNbr, custID, Role, objHeader.Status, handle,
                    //                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                    //    }
                    //    //statusAfterAll = objHeader.Status;

                    //    String[] nodeid = nodeID.Split('-');
                    //    objHeader.NodeID = nodeid[0];
                    //    objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                    //    var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                    //    objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;

                    //    objHeader.Crtd_Datetime = DateTime.Now;
                    //    objHeader.Crtd_Prog = screenNbr;
                    //    objHeader.Crtd_User = Current.UserName;
                    //    objHeader.tstamp = new byte[0];

                    //    UpdatingHeader(updated, ref objHeader);
                    //    _db.AR_Customer.AddObject(objHeader);
                    //    _db.SaveChanges();




                    //}



                    // If there is a change in handling status (keepStatus is False),
                    // add a new pending task with an approved handle.



                    // ===============================================================

                    // Get out of the loop (only update the first data)

                }
                //foreach (AR_Customer created in lstheader.Created)
                //{

                //    var objHeader = _db.AR_Customer.Where(p => p.CustId == custID && p.BranchID == branchID).FirstOrDefault();
                //    if (objHeader == null)
                //    {
                //        if (hadChild != 0)
                //        {

                //            objHeader = new AR_Customer();
                //            objHeader.CustId = custID;
                //            objHeader.BranchID = branchID;

                //            if (handle == "N" || handle == "")
                //            {
                //                objHeader.Status = status;
                //            }
                //            else
                //            {
                //                objHeader.Status = approveHandle.ToStatus;

                //            }
                //            if (objHeader.Status == "O")
                //            {
                //                X.Msg.Show(new MessageBoxConfig()
                //                {
                //                    Message = "Email sent!"
                //                });
                //                Approve.Mail_Approve(screenNbr, custID, Role, objHeader.Status, handle,
                //                             lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                //            }

                //            String[] nodeid = nodeID.Split('-');
                //            objHeader.NodeID = nodeid[0];
                //            objHeader.NodeLevel = Convert.ToInt16(nodeLevel);
                //            var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                //            objHeader.ParentRecordID = searchparentRecordID.ParentRecordID;

                //            objHeader.Crtd_Datetime = DateTime.Now;
                //            objHeader.Crtd_Prog = screenNbr;
                //            objHeader.Crtd_User = Current.UserName;
                //            objHeader.tstamp = new byte[0];

                //            UpdatingHeader(created, ref objHeader);
                //            _db.AR_Customer.AddObject(objHeader);
                //            _db.SaveChanges();

                //        }
                //        else
                //        {
                //            return Json(new { success = false, code = "213" }, JsonRequestBehavior.AllowGet);
                //        }

                //    }
                //}

                var objtmp = _db.AR_Customer.Where(p => p.CustId == custID && p.BranchID == branchID).FirstOrDefault();
                if (objtmp != null)
                {

                    if (handle == "N" || handle == "")
                    {
                        objtmp.Status = status;
                    }
                    else
                    {
                        objtmp.Status = approveHandle.ToStatus;

                    }
                    if (objtmp.Status == "O" && lstheader.Updated.Count == 0 && lstheader.Created.Count == 0)
                    {
                        X.Msg.Show(new MessageBoxConfig()
                        {
                            Message = "Email sent!"
                        });
                        Approve.Mail_Approve(screenNbr, custID, Role, objtmp.Status, handle,
                                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                    }

                    //statusAfterAll = objtmp.Status;
                    //loc ra la insert du lieu moi hay chuyen cay cho du lieu
                    if (lstheader.Updated.Count == 0 && hadChild != 0)
                    {
                        String[] nodeid = nodeID.Split('-');
                        if (objtmp.NodeID != nodeid[0])
                        {
                            //chuyen cay du lieu
                            tmpChangeTreeDic = "1";
                        }
                        else
                        {
                            tmpChangeTreeDic = "0";
                        }
                        objtmp.NodeID = nodeid[0];

                        objtmp.NodeLevel = Convert.ToInt16(nodeLevel);
                        var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                        objtmp.ParentRecordID = searchparentRecordID.ParentRecordID;
                        //tmpChangeTreeDic = "1";
                    }


                }


                _db.SaveChanges();
                //this.Direct();


                if (hadChild != 0)
                {
                    return Json(new { success = true, custID = custID, custName = custName, addNewOrUpdate = "addNew", changeTreeBranch = tmpChangeTreeDic, selectedNode = tmpSelectedNode }, JsonRequestBehavior.AllowGet);
                    //return Json(new { success = true });
                }
                else
                {
                    //return Json(new { success = true });
                    return Json(new { success = true, custID = custID, custName = custName, addNewOrUpdate = "update" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }








        [DirectMethod]
        public ActionResult AR20400Delete(string custID, string branchID, string status)
        {

            var inv = _db.AR_Customer.FirstOrDefault(p => p.CustId == custID && p.BranchID == branchID);
            _db.AR_Customer.DeleteObject(inv);
            _db.SaveChanges();
            return this.Direct();

            //Ext.Net.ResourceManager.AjaxSuccess = false;
            return this.Direct(false);


        }


        private void UpdatingHeader(AR_Customer s, ref AR_Customer d)
        {
            d.ClassId = s.ClassId;
            d.CustType = s.CustType;
            d.CustName = s.CustName;
            d.PriceClassID = s.PriceClassID;
            d.Terms = s.Terms;
            d.TradeDisc = s.TradeDisc;
            d.CrRule = s.CrRule;
            d.CrLmt = s.CrLmt;
            d.GracePer = s.GracePer;
            d.Territory = s.Territory;
            d.Area = s.Area;
            d.Location = s.Location;
            d.Channel = s.Channel;
            d.ShopType = s.ShopType;
            d.GiftExchange = s.GiftExchange;
            d.HasPG = s.HasPG;
            d.SlsperId = s.SlsperId;
            d.DeliveryID = s.DeliveryID;
            d.SupID = s.SupID;
            d.SiteId = s.SiteId;
            d.DfltShipToId = s.DfltShipToId;
            d.CustFillPriority = s.CustFillPriority;
            d.LTTContractNbr = s.LTTContractNbr;
            d.DflSaleRouteID = s.DflSaleRouteID;
            d.EmpNum = s.EmpNum;
            d.ExpiryDate = s.ExpiryDate;
            d.EstablishDate = s.EstablishDate;
            d.Birthdate = s.Birthdate;
            d.CustName = s.CustName;
            d.Attn = s.Attn;
            d.Salut = s.Salut;
            d.Addr1 = s.Addr1;
            d.Addr2 = s.Addr2;
            d.Country = s.Country;
            d.State = s.State;
            d.City = s.City;
            d.District = s.District;
            d.Zip = s.Zip;
            d.Phone = s.Phone;
            d.Fax = s.Fax;
            d.EMailAddr = s.EMailAddr;
            d.BillName = s.BillName;
            d.BillAttn = s.BillAttn;
            d.BillSalut = s.BillSalut;
            d.BillAddr1 = s.BillAddr1;
            d.BillAddr2 = s.BillAddr2;
            d.BillCountry = s.BillCountry;
            d.BillState = s.BillState;
            d.BillCity = s.BillCity;
            d.BillZip = s.BillZip;
            d.BillPhone = s.BillPhone;
            d.BillFax = s.BillFax;
            d.TaxDflt = s.TaxDflt;
            d.TaxRegNbr = s.TaxRegNbr;
            d.TaxLocId = s.TaxLocId;
            d.TaxID00 = s.TaxID00;
            d.TaxID01 = s.TaxID01;
            d.TaxID02 = s.TaxID02;
            d.TaxID03 = s.TaxID03;


            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }




    }
}

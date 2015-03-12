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

        public ActionResult GetDataGridLTTContract(String custId)
        {

            var lstLTTContract = _db.AR_LTTContract.Where(p => p.CustID == custId).ToList();
            return this.Store(lstLTTContract);
        }

        public ActionResult GetDataGridLTTContractDetail(String lTTContractNbr)
        {

            var lstLTTContractDetail = _db.AR_LTTContractDetail.Where(p => p.LTTContractNbr == lTTContractNbr).ToList();
            return this.Store(lstLTTContractDetail);
        }

        public ActionResult GetDataGridAdv(String custId)
        {

            var lstAR_CustAdvTool = _db.AR_CustAdvTool.Where(p => p.CustID == custId).ToList();
            return this.Store(lstAR_CustAdvTool);
        }

        public ActionResult GetDataGridSellingProd(String custId)
        {

            var lstAR_CustSellingProducts = _db.AR_CustSellingProducts.Where(p => p.CustID == custId).ToList();
            return this.Store(lstAR_CustSellingProducts);
        }

        public ActionResult GetDataGridDispMethod(String custId)
        {

            var lstAR_CustDisplayMethod = _db.AR_CustDisplayMethod.Where(p => p.CustID == custId).ToList();
            return this.Store(lstAR_CustDisplayMethod);
        }

        [DirectMethod]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveTree(FormCollection data, string custID, string handle, string nodeID, int nodeLevel, string parentRecordID,
            int hadChild, string status, string tmpSelectedNode, string branchID, string custName, bool isNew, bool tmpHiddenTree, string lTTContractNbr)
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


                    var objAR_Customer = _db.AR_Customer.Where(p => p.CustId == custID && p.BranchID == branchID).FirstOrDefault();
                    //if (tmpHiddenTree == false)
                    //{
                        if (isNew)//new record
                        {
                            if (objAR_Customer != null)
                            {
                                return Json(new { success = false, msgCode = 2000, msgParam = custID });//quang message ma nha cung cap da ton tai ko the them

                            }
                            else
                            {
                                if (hadChild != 0)
                                {
                                    objAR_Customer = new AR_Customer();
                                    objAR_Customer.CustId = custID;
                                    objAR_Customer.BranchID = branchID;

                                    if (handle == "N" || handle == "")
                                    {
                                        objAR_Customer.Status = status;
                                    }
                                    else
                                    {
                                        objAR_Customer.Status = approveHandle.ToStatus;

                                    }
                                    if (objAR_Customer.Status == "O")
                                    {
                                        X.Msg.Show(new MessageBoxConfig()
                                        {
                                            Message = "Email sent!"
                                        });
                                        Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
                                                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                                    }
                                    //statusAfterAll = objAR_Customer.Status;

                                    String[] nodeid = nodeID.Split('-');
                                    objAR_Customer.NodeID = nodeid[0];
                                    objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);
                                    var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                                    objAR_Customer.ParentRecordID = searchparentRecordID.ParentRecordID;

                                    objAR_Customer.Crtd_Datetime = DateTime.Now;
                                    objAR_Customer.Crtd_Prog = screenNbr;
                                    objAR_Customer.Crtd_User = Current.UserName;
                                    objAR_Customer.tstamp = new byte[0];

                                    UpdatingHeader(updated, ref objAR_Customer);
                                    if (data["cboEstablishDate"] == "")
                                    {
                                        objAR_Customer.EstablishDate = null;
                                    }
                                    _db.AR_Customer.AddObject(objAR_Customer);
                                    _db.SaveChanges();


                                    //save may bang khac
                                    var objAR_Setup = _db.AR_Setup.Where(p => p.BranchID == brandID && p.SetupId == "AR").FirstOrDefault();
                                    if (objAR_Setup.AutoCustID)
                                    {
                                        var objCustHist = new AR_CustHist();
                                        var objCustHist1 = _db.AR_CustHist.Where(p => p.BranchID == objAR_Customer.BranchID &&
                                             p.CustID == objAR_Customer.CustId).OrderByDescending(p => p.Seq).FirstOrDefault();
                                        string strSeq = objCustHist1 == null ? "0000000001" : ("0000000000" + (double.Parse(objCustHist1.Seq) + 1));
                                        strSeq = strSeq.Substring(strSeq.Length - 10, 10);
                                        objCustHist.Seq = strSeq;
                                        objCustHist.BranchID = objAR_Customer.BranchID;
                                        objCustHist.Crtd_DateTime = DateTime.Now;
                                        objCustHist.Crtd_Prog = screenNbr;
                                        objCustHist.Crtd_User = Current.UserName;
                                        objCustHist.CustID = objAR_Customer.CustId;
                                        objCustHist.LUpd_DateTime = DateTime.Now;
                                        objCustHist.LUpd_Prog = screenNbr;
                                        objCustHist.LUpd_User = Current.UserName;
                                        objCustHist.tstamp = new byte[0];
                                        objCustHist.Note = "Tạo mới khách hàng";
                                        objCustHist.FromDate = DateTime.Now.ToDateShort();
                                        objCustHist.ToDate = DateTime.Now.ToDateShort().AddYears(100);
                                        _db.AR_CustHist.AddObject(objCustHist);
                                        // SubmitchangeAR_Customer();
                                        _db.SaveChanges();
                                        SaveAR_SOAddress(data, isNew);
                                        _db.SaveChanges();
                                    }
                                    else
                                    {
                                        var objCustHist = new AR_CustHist();
                                        var objCustHist1 = _db.AR_CustHist.Where(p => p.BranchID == objAR_Customer.BranchID &&
                                             p.CustID == objAR_Customer.CustId).OrderByDescending(p => p.Seq).FirstOrDefault();
                                        string strSeq = objCustHist1 == null ? "0000000001" : ("0000000000" + (double.Parse(objCustHist1.Seq) + 1));
                                        strSeq = strSeq.Substring(strSeq.Length - 10, 10);
                                        objCustHist.Seq = strSeq;
                                        objCustHist.BranchID = objAR_Customer.BranchID;
                                        objCustHist.Crtd_DateTime = DateTime.Now;
                                        objCustHist.Crtd_Prog = screenNbr;
                                        objCustHist.Crtd_User = Current.UserName;
                                        objCustHist.CustID = objAR_Customer.CustId;
                                        objCustHist.LUpd_DateTime = DateTime.Now;
                                        objCustHist.LUpd_Prog = screenNbr;
                                        objCustHist.LUpd_User = Current.UserName;
                                        objCustHist.tstamp = new byte[0];
                                        objCustHist.Note = "Tạo mới khách hàng";
                                        objCustHist.FromDate = DateTime.Now.ToDateShort();
                                        objCustHist.ToDate = DateTime.Now.ToDateShort().AddYears(100);
                                        _db.AR_CustHist.AddObject(objCustHist);
                                        // SubmitchangeAR_Customer();
                                        _db.SaveChanges();
                                        SaveAR_SOAddress(data, isNew);
                                        _db.SaveChanges();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (objAR_Customer != null)
                            {
                                if (hadChild == 0) // nếu selection tree ko có con
                                {
                                    if (objAR_Customer.tstamp.ToHex() == updated.tstamp.ToHex())
                                    {
                                        if (handle == "N" || handle == "")
                                        {
                                            objAR_Customer.Status = status;
                                        }
                                        else
                                        {
                                            objAR_Customer.Status = approveHandle.ToStatus;

                                        }
                                        if (objAR_Customer.Status == "O")
                                        {
                                            X.Msg.Show(new MessageBoxConfig()
                                            {
                                                Message = "Email sent!"
                                            });
                                            Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
                                                         lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                                        }
                                        //Node


                                        //String[] nodeid = nodeID.Split('-');
                                        //objAR_Customer.NodeID = nodeid[0];
                                        //objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);
                                        UpdatingHeader(updated, ref objAR_Customer);
                                    }
                                    else
                                    {
                                        throw new MessageException(MessageType.Message, "19");
                                    }
                                    _db.SaveChanges();


                                }
                                else // nếu selection tree có con
                                {
                                    if (objAR_Customer.tstamp.ToHex() == updated.tstamp.ToHex())
                                    {
                                        if (handle == "N" || handle == "")
                                        {
                                            objAR_Customer.Status = status;
                                        }
                                        else
                                        {
                                            objAR_Customer.Status = approveHandle.ToStatus;

                                        }
                                        if (objAR_Customer.Status == "O")
                                        {
                                            X.Msg.Show(new MessageBoxConfig()
                                            {
                                                Message = "Email sent!"
                                            });
                                            Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
                                                         lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                                        }
                                        //Node


                                        String[] nodeid = nodeID.Split('-');
                                        if (objAR_Customer.NodeID != nodeid[0])
                                        {
                                            tmpChangeTreeDic = "1";
                                        }
                                        else
                                        {
                                            tmpChangeTreeDic = "0";
                                        }
                                        objAR_Customer.NodeID = nodeid[0];
                                        objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);
                                        var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                                        objAR_Customer.ParentRecordID = searchparentRecordID.ParentRecordID;
                                        UpdatingHeader(updated, ref objAR_Customer);
                                    }
                                    else
                                    {
                                        throw new MessageException(MessageType.Message, "19");
                                    }
                                    _db.SaveChanges();
                                }

                            }
                        }




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

                            _db.SaveChanges();
                        }



                        _db.SaveChanges();
                        //this.Direct();


                        if (hadChild != 0)
                        {
                             return Json(new { success = true, custID = custID, custName = custName, addNewOrUpdate = "addNew", changeTreeBranch = tmpChangeTreeDic, selectedNode = tmpSelectedNode, tmpHiddenTree = tmpHiddenTree }, JsonRequestBehavior.AllowGet);
                            //return Json(new { success = true });
                        }
                        else
                        {
                            //return Json(new { success = true });
                             Json(new { success = true, custID = custID, custName = custName, addNewOrUpdate = "update", tmpHiddenTree = tmpHiddenTree}, JsonRequestBehavior.AllowGet);
                        }
                    //}
                    //else //truong hop ko co tree
                    //{

                    //    if (isNew)//new record
                    //    {
                    //        if (objAR_Customer != null)
                    //        {
                    //            return Json(new { success = false, msgCode = 2000, msgParam = custID });//quang message ma nha cung cap da ton tai ko the them

                    //        }
                    //        else
                    //        {
                             
                    //                objAR_Customer = new AR_Customer();
                    //                objAR_Customer.CustId = custID;
                    //                objAR_Customer.BranchID = branchID;

                    //                if (handle == "N" || handle == "")
                    //                {
                    //                    objAR_Customer.Status = status;
                    //                }
                    //                else
                    //                {
                    //                    objAR_Customer.Status = approveHandle.ToStatus;

                    //                }
                    //                if (objAR_Customer.Status == "O")
                    //                {
                    //                    X.Msg.Show(new MessageBoxConfig()
                    //                    {
                    //                        Message = "Email sent!"
                    //                    });
                    //                    Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
                    //                                 lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                    //                }
                    //                //statusAfterAll = objAR_Customer.Status;

                    //                String[] nodeid = nodeID.Split('-');
                    //                objAR_Customer.NodeID = nodeid[0];
                    //                objAR_Customer.NodeLevel = Convert.ToInt16("0");
                    //                //var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                    //                objAR_Customer.ParentRecordID = 0;

                    //                objAR_Customer.Crtd_Datetime = DateTime.Now;
                    //                objAR_Customer.Crtd_Prog = screenNbr;
                    //                objAR_Customer.Crtd_User = Current.UserName;
                    //                objAR_Customer.tstamp = new byte[0];

                    //                UpdatingHeader(updated, ref objAR_Customer);
                    //                _db.AR_Customer.AddObject(objAR_Customer);
                    //                _db.SaveChanges();
                                
                    //        }
                    //     //ngoac dong isNew
                    //     }
                    //     else
                    //     {
                    //        if (objAR_Customer != null)
                    //        {
                               
                    //                if (objAR_Customer.tstamp.ToHex() == updated.tstamp.ToHex())
                    //                {
                    //                    if (handle == "N" || handle == "")
                    //                    {
                    //                        objAR_Customer.Status = status;
                    //                    }
                    //                    else
                    //                    {
                    //                        objAR_Customer.Status = approveHandle.ToStatus;

                    //                    }
                    //                    if (objAR_Customer.Status == "O")
                    //                    {
                    //                        X.Msg.Show(new MessageBoxConfig()
                    //                        {
                    //                            Message = "Email sent!"
                    //                        });
                    //                        Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
                    //                                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                    //                    }
                    //                    //Node


                    //                    //String[] nodeid = nodeID.Split('-');
                    //                    //objAR_Customer.NodeID = nodeid[0];
                    //                    //objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);
                    //                    UpdatingHeader(updated, ref objAR_Customer);
                    //                }
                    //                else
                    //                {
                    //                    throw new MessageException(MessageType.Message, "19");
                    //                }
                    //                _db.SaveChanges();

                    //            }

                    //        }


                    //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    //}

                //dong ngoac foreach Customer        
                }

                //xet cac tab co the an xem co thay doi gi ko
                var TabContract = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabContract");
                if (TabContract.IntVal == 1)
                {
                    SaveAR_LTTContract(data, custID);
                    SaveAR_LTTContractDetail(data, lTTContractNbr);
                }

                var TabAdvTool = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabAdvTool");

                if (TabAdvTool.IntVal == 1)
                {
                    SaveAR_CustAdvTool(data, custID);

                }

                var TabSellingProduct = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabSellingProduct");
                if (TabSellingProduct.IntVal == 1)
                {
                    SaveAR_CustSellingProducts(data, custID);

                }


                var TabDisplayMethod = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabDisplayMethod");
                if (TabDisplayMethod.IntVal == 1)
                {
                    SaveAR_CustDisplayMethod(data, custID);

                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            //dong ngoac try
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        //Save no Tree
        [DirectMethod]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveNoTree(FormCollection data, string custID, string handle, string status, string tmpSelectedNode,
            string branchID, string custName, bool isNew, string lTTContractNbr)
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

                

                foreach (AR_Customer updated in lstheader.Updated)
                {
                    //Save AR_Customer
                    var objAR_Customer = _db.AR_Customer.Where(p => p.CustId == custID && p.BranchID == branchID).FirstOrDefault();
                    if (isNew)//new record
                    {
                        if (objAR_Customer != null)
                        {
                            return Json(new { success = false, msgCode = 2000, msgParam = custID });//quang message ma nha cung cap da ton tai ko the them

                        }
                        else
                        {

                            objAR_Customer = new AR_Customer();
                            objAR_Customer.CustId = custID;
                            objAR_Customer.BranchID = branchID;

                            if (handle == "N" || handle == "")
                            {
                                objAR_Customer.Status = status;
                            }
                            else
                            {
                                objAR_Customer.Status = approveHandle.ToStatus;

                            }
                            if (objAR_Customer.Status == "O")
                            {
                                X.Msg.Show(new MessageBoxConfig()
                                {
                                    Message = "Email sent!"
                                });
                                Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
                                                lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                            }
                            //statusAfterAll = objAR_Customer.Status;

                            //String[] nodeid = nodeID.Split('-');
                            objAR_Customer.NodeID = "DF";
                            objAR_Customer.NodeLevel = 1;
                            //var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
                            objAR_Customer.ParentRecordID = 0;

                            objAR_Customer.Crtd_Datetime = DateTime.Now;
                            objAR_Customer.Crtd_Prog = screenNbr;
                            objAR_Customer.Crtd_User = Current.UserName;
                            objAR_Customer.tstamp = new byte[0];

                            UpdatingHeader(updated, ref objAR_Customer);
                            if (data["cboEstablishDate"] == "")
                            {
                                objAR_Customer.EstablishDate = null;
                            }
                            _db.AR_Customer.AddObject(objAR_Customer);
                            _db.SaveChanges();

                            //save may bang khac
                            var objAR_Setup = _db.AR_Setup.Where(p => p.BranchID == brandID && p.SetupId == "AR").FirstOrDefault();
                            if (objAR_Setup.AutoCustID)
                            {
                                var objCustHist = new AR_CustHist();
                                var objCustHist1 = _db.AR_CustHist.Where(p => p.BranchID == objAR_Customer.BranchID &&
                                     p.CustID == objAR_Customer.CustId).OrderByDescending(p => p.Seq).FirstOrDefault();                                
                                string strSeq = objCustHist1 == null ? "0000000001" : ("0000000000" + (double.Parse(objCustHist1.Seq) + 1));
                                strSeq = strSeq.Substring(strSeq.Length - 10, 10);
                                objCustHist.Seq = strSeq;
                                objCustHist.BranchID = objAR_Customer.BranchID;
                                objCustHist.Crtd_DateTime = DateTime.Now;
                                objCustHist.Crtd_Prog = screenNbr;
                                objCustHist.Crtd_User = Current.UserName;
                                objCustHist.CustID = objAR_Customer.CustId;
                                objCustHist.LUpd_DateTime = DateTime.Now;
                                objCustHist.LUpd_Prog = screenNbr;
                                objCustHist.LUpd_User = Current.UserName;
                                objCustHist.tstamp = new byte[0];
                                objCustHist.Note = "Tạo mới khách hàng";
                                objCustHist.FromDate = DateTime.Now.ToDateShort();
                                objCustHist.ToDate = DateTime.Now.ToDateShort().AddYears(100);
                                _db.AR_CustHist.AddObject(objCustHist);
                                // SubmitchangeAR_Customer();
                                _db.SaveChanges();
                                SaveAR_SOAddress(data, isNew);
                                _db.SaveChanges();
                            }
                            else
                            {
                                var objCustHist = new AR_CustHist();
                                var objCustHist1 = _db.AR_CustHist.Where(p => p.BranchID == objAR_Customer.BranchID &&
                                     p.CustID == objAR_Customer.CustId).OrderByDescending(p => p.Seq).FirstOrDefault();
                                string strSeq = objCustHist1 == null ? "0000000001" : ("0000000000" + (double.Parse(objCustHist1.Seq) + 1));
                                strSeq = strSeq.Substring(strSeq.Length - 10, 10);
                                objCustHist.Seq = strSeq;
                                objCustHist.BranchID = objAR_Customer.BranchID;
                                objCustHist.Crtd_DateTime = DateTime.Now;
                                objCustHist.Crtd_Prog = screenNbr;
                                objCustHist.Crtd_User = Current.UserName;
                                objCustHist.CustID = objAR_Customer.CustId;
                                objCustHist.LUpd_DateTime = DateTime.Now;
                                objCustHist.LUpd_Prog = screenNbr;
                                objCustHist.LUpd_User = Current.UserName;
                                objCustHist.tstamp = new byte[0];
                                objCustHist.Note = "Tạo mới khách hàng";
                                objCustHist.FromDate = DateTime.Now.ToDateShort();
                                objCustHist.ToDate = DateTime.Now.ToDateShort().AddYears(100);
                                _db.AR_CustHist.AddObject(objCustHist);
                                // SubmitchangeAR_Customer();
                                _db.SaveChanges();
                                SaveAR_SOAddress(data, isNew);
                                _db.SaveChanges();
                            }






                        }
                    //ngoac dong isNew
                    }
                    else // else nay la update
                    {
                        if (objAR_Customer != null)
                        {

                            if (objAR_Customer.tstamp.ToHex() == updated.tstamp.ToHex())
                            {
                                if (handle == "N" || handle == "")
                                {
                                    objAR_Customer.Status = status;
                                }
                                else
                                {
                                    objAR_Customer.Status = approveHandle.ToStatus;

                                }
                                if (objAR_Customer.Status == "O")
                                {
                                    X.Msg.Show(new MessageBoxConfig()
                                    {
                                        Message = "Email sent!"
                                    });
                                    Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
                                                    lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
                                }

                                UpdatingHeader(updated, ref objAR_Customer);
                                if (data["cboEstablishDate"] == "")
                                {
                                    objAR_Customer.EstablishDate = null;
                                }
                                _db.SaveChanges();
                                //save may bang khac 
                                SaveAR_SOAddress(data, isNew);
                                _db.SaveChanges();
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                            _db.SaveChanges();
                        }

                    }

  
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);


                } //dong ngoac foreach Customer  


                //xet cac tab co the an xem co thay doi gi ko
                var TabContract = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabContract");
                if (TabContract.IntVal == 1)
                {
                    SaveAR_LTTContract(data,custID);
                    SaveAR_LTTContractDetail(data, lTTContractNbr);
                }

                var TabAdvTool = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabAdvTool");

                if (TabAdvTool.IntVal == 1)
                {
                    SaveAR_CustAdvTool(data, custID);
                   
                }

                var TabSellingProduct = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabSellingProduct");
                if (TabSellingProduct.IntVal == 1)
                {
                    SaveAR_CustSellingProducts(data, custID);

                }

                
                var TabDisplayMethod = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabDisplayMethod");
                if (TabDisplayMethod.IntVal == 1)
                {
                    SaveAR_CustDisplayMethod(data, custID);

                }


                return Json(new { success = true }, JsonRequestBehavior.AllowGet);




            }//dong ngoac try
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }





        [HttpPost]
        public ActionResult AR20400DeleteHeader(string custID, string branchID, string status)
        {

            
            try
            {
                var delHeaderAR_Customer = _db.AR_Customer.FirstOrDefault(p => p.CustId == custID && p.BranchID == branchID);
                if (delHeaderAR_Customer != null)
                {
                    //delete SOAddress
                    var delAR_SOAddress = _db.AR_SOAddress.Where(p => p.CustId == custID && p.BranchID == branchID).ToList();
                    if(delAR_SOAddress != null){
                        foreach (var del in delAR_SOAddress)
                        {
                            _db.AR_SOAddress.DeleteObject(del);
                            _db.SaveChanges();
                        }

                    }

                    var delLTTContract = _db.AR_LTTContract.Where(p => p.LTTContractNbr == delHeaderAR_Customer.LTTContractNbr).ToList();
                    if (delLTTContract != null)
                    {
                        foreach (var delContract in delLTTContract)
                        {
                            var delLTTContractDetail = _db.AR_LTTContractDetail.Where(p => p.LTTContractNbr == delContract.LTTContractNbr).ToList();
                            if (delLTTContractDetail != null)
                            {
                                foreach (var delDetail in delLTTContractDetail)
                                {
                                    _db.AR_LTTContractDetail.DeleteObject(delDetail);
                                    _db.SaveChanges();
                                }
                            }

                            _db.AR_LTTContract.DeleteObject(delContract);
                            _db.SaveChanges();
                        }
                      
                    }

                    var delAR_CustAdvTool = _db.AR_CustAdvTool.Where(p => p.CustID == custID).ToList();
                    if (delAR_CustAdvTool != null)
                    {
                        foreach (var delAdv in delAR_CustAdvTool)
                        {
                            _db.AR_CustAdvTool.DeleteObject(delAdv);
                            _db.SaveChanges();
                        }
                    }

                    var delCustSellingProducts = _db.AR_CustSellingProducts.Where(p => p.CustID == custID).ToList();
                    if (delCustSellingProducts != null)
                    {
                        foreach (var delSellingProd in delCustSellingProducts)
                        {
                            _db.AR_CustSellingProducts.DeleteObject(delSellingProd);
                            _db.SaveChanges();
                        }
                    }

                    var delCustDisplayMethod = _db.AR_CustDisplayMethod.Where(p => p.CustID == custID).ToList();
                    if (delCustDisplayMethod != null)
                    {
                        foreach (var delDispMethod in delCustDisplayMethod)
                        {
                            _db.AR_CustDisplayMethod.DeleteObject(delDispMethod);
                            _db.SaveChanges();
                        }
                    }




                    _db.AR_Customer.DeleteObject(delHeaderAR_Customer);
                    _db.SaveChanges();
                }
               
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }

        }

        [HttpPost]
        public ActionResult checkTreeAndHiddenTab(string cpnyID)// kiem tra xem voi branchID nay co Tree hay ko
        {
            try
            {
                var tmpHiddenTabContract = false;
                var tmpHiddenTabAdvTool = false;
                var tmpHiddenTabSellingProduct = false;
                var tmpHiddenTabDisplayMethod = false;
                var cpny = _db.AR_Setup.FirstOrDefault(p => p.BranchID == cpnyID && p.SetupId == "AR" && p.HiddenHierarchy == true);
                var TabContract = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabContract");
                var TabAdvTool = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabAdvTool");
                var TabSellingProduct = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabSellingProduct");
                var TabDisplayMethod = _sys.SYS_Configurations.FirstOrDefault(p => p.Code == "TabDisplayMethod");
                //Contract
                if (TabContract.IntVal == 1)
                {
                    tmpHiddenTabContract = false;
                }
                else
                {
                    tmpHiddenTabContract = true;
                }
                //AdvTool
                if (TabAdvTool.IntVal == 1)
                {
                    tmpHiddenTabAdvTool = false;
                }
                else
                {
                    tmpHiddenTabAdvTool = true;
                }
                //SellingProduct
                if (TabSellingProduct.IntVal == 1)
                {
                    tmpHiddenTabSellingProduct = false;
                }
                else
                {
                    tmpHiddenTabSellingProduct = true;
                }
                //DisplayMethod
                if (TabDisplayMethod.IntVal == 1)
                {
                    tmpHiddenTabDisplayMethod = false;
                }
                else
                {
                    tmpHiddenTabDisplayMethod = true;
                }

                if (cpny != null)
                {
                    //nếu có thì xóa Tree
                    return Json(new 
                    { 
                        success = false, tmpHiddenTabContract = tmpHiddenTabContract, 
                        tmpHiddenTabAdvTool = tmpHiddenTabAdvTool,
                        tmpHiddenTabSellingProduct = tmpHiddenTabSellingProduct,
                        tmpHiddenTabDisplayMethod = tmpHiddenTabDisplayMethod
                     });
                }
                else
                {
                    //nếu ko có thì để tree
                    return Json(new
                    {
                        success = true,
                        tmpHiddenTabContract = tmpHiddenTabContract,
                        tmpHiddenTabAdvTool = tmpHiddenTabAdvTool,
                        tmpHiddenTabSellingProduct = tmpHiddenTabSellingProduct,
                        tmpHiddenTabDisplayMethod = tmpHiddenTabDisplayMethod
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }

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
            d.EstablishDate = Convert.ToDateTime(s.EstablishDate).ToDateShort();
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

        private bool SaveAR_SOAddress(FormCollection data, bool isNew)
        {
            //var objAr_SOAddr = new AR_SOAddress();

            var custId = data["cboCustId"];
            var branchID = data["cboCpnyID"];
            var dfltShipToId = data["cboDfltShipToId"];
            var objAr_SOAddr = _db.AR_SOAddress.FirstOrDefault(p => p.BranchID == branchID && p.CustId == custId && p.ShipToId == dfltShipToId);
            var objAR_Customer = _db.AR_Customer.FirstOrDefault(p => p.BranchID == branchID && p.CustId == custId);
            //UpdatingAR_SOAddress();
            try
            {

                if (objAr_SOAddr != null)
                {
                    isNew = false;

                }
                else
                {
                    isNew = true;
                    objAr_SOAddr = new AR_SOAddress();
                    objAr_SOAddr.CustId = data["cboCustId"];
                    objAr_SOAddr.ShipToId = (data["cboDfltShipToId"] == "" ? "DEFAULT" : data["cboDfltShipToId"]);
                }

                //objAr_SOAddr.BranchID = this.txtBranchID.Text;
                objAr_SOAddr.BranchID = data["cboCpnyID"];
                objAr_SOAddr.SOName = data["txtCustName"];
                objAr_SOAddr.Attn = data["txtAttn"];
                objAr_SOAddr.Addr1 = data["txtAddr1"];
                objAr_SOAddr.Addr2 = data["txtAddr2"];
                objAr_SOAddr.City = objAR_Customer.City;
                objAr_SOAddr.State = objAR_Customer.State;
                objAr_SOAddr.District = objAR_Customer.District;
                objAr_SOAddr.Zip = data["txtZip"];
                objAr_SOAddr.Country = objAR_Customer.Country;
                objAr_SOAddr.Phone = data["txtPhone"];
                objAr_SOAddr.Fax = data["txtFax"];
                objAr_SOAddr.TaxRegNbr = data["txtTaxRegNbr"];
                objAr_SOAddr.TaxLocId = data["txtTaxLocId"];
                objAr_SOAddr.TaxId00 = objAR_Customer.TaxID00;
                objAr_SOAddr.TaxId01 = objAR_Customer.TaxID01;
                objAr_SOAddr.TaxId02 = objAR_Customer.TaxID02;
                objAr_SOAddr.TaxId03 = objAR_Customer.TaxID03;
                objAr_SOAddr.Crtd_DateTime = objAR_Customer.Crtd_Datetime;
                objAr_SOAddr.Crtd_Prog = objAR_Customer.Crtd_Prog;
                objAr_SOAddr.Crtd_User = objAR_Customer.Crtd_User;
                objAr_SOAddr.LUpd_DateTime = objAR_Customer.LUpd_Datetime;
                objAr_SOAddr.LUpd_Prog = objAR_Customer.LUpd_Prog;
                objAr_SOAddr.LUpd_User = objAR_Customer.LUpd_User;
                objAr_SOAddr.tstamp = objAR_Customer.tstamp;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            if (!isNew)
            {

            }
            else
            {
                _db.AR_SOAddress.AddObject(objAr_SOAddr);
                _db.SaveChanges();

            }

            return true;
        }

        private bool SaveAR_LTTContract(FormCollection data, string custID)
        {
            StoreDataHandler dataLTTContract = new StoreDataHandler(data["lstGridLTTContract"]);
            ChangeRecords<AR_LTTContract> lstGridLTTContract = dataLTTContract.BatchObjectData<AR_LTTContract>();

            foreach (AR_LTTContract deleted in lstGridLTTContract.Deleted)
            {
                var delLTTContract = _db.AR_LTTContract.Where(p => p.LTTContractNbr == deleted.LTTContractNbr).FirstOrDefault();
                if (delLTTContract != null)
                {
                    var delLTTContractDetail = _db.AR_LTTContractDetail.Where(p => p.LTTContractNbr == delLTTContract.LTTContractNbr).ToList();
                    if (delLTTContractDetail != null)
                    {
                        foreach (var del in delLTTContractDetail)
                        {
                            _db.AR_LTTContractDetail.DeleteObject(del);
                            _db.SaveChanges();
                        }
                    }

                    _db.AR_LTTContract.DeleteObject(delLTTContract);
                    _db.SaveChanges();
                }
            }

            lstGridLTTContract.Created.AddRange(lstGridLTTContract.Updated);// bo cai Update vao Created luon cho gon
            foreach (AR_LTTContract created in lstGridLTTContract.Created)
            {

                var recordLTTContract = _db.AR_LTTContract.Where(p => p.LTTContractNbr == created.LTTContractNbr).FirstOrDefault();
                if (created.tstamp.ToHex() == "")//dong nay la dong them moi
                {
                    if (recordLTTContract == null)
                    {
                        recordLTTContract = new AR_LTTContract();
                        recordLTTContract.LTTContractNbr = created.LTTContractNbr;
                        recordLTTContract.CustID = custID;
                        recordLTTContract.Crtd_Datetime = DateTime.Now;
                        recordLTTContract.Crtd_Prog = screenNbr;
                        recordLTTContract.Crtd_User = Current.UserName;
                        recordLTTContract.tstamp = new byte[0];

                        UpdatingAR_LTTContract(created, ref recordLTTContract);
                        if (recordLTTContract.LTTContractNbr != "" && recordLTTContract.CustID != "")
                        {
                            _db.AR_LTTContract.AddObject(recordLTTContract);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
                    }
                }
                else //update
                {
                    if (created.tstamp.ToHex() == recordLTTContract.tstamp.ToHex())
                    {
                        UpdatingAR_LTTContract(created, ref recordLTTContract);
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
            }
          
            //
            _db.SaveChanges();
            return true;
        }

        private void UpdatingAR_LTTContract(AR_LTTContract s, ref AR_LTTContract d)
        {
            d.Active = s.Active;
            d.ActualAmt = s.ActualAmt;
            d.ActualQty = s.ActualQty;
            d.AmtCommit = s.AmtCommit;
           
            d.ExtDate = s.ExtDate;
            d.FromDate = s.FromDate;
            d.MonthNum = s.MonthNum;
            d.QtyCommit = s.QtyCommit;
            d.Status = s.Status;
            d.ToDate = s.ToDate;
            d.TotAmt = s.TotAmt;
            d.TotQty = s.TotQty;
            d.tstamp = new byte[0];
            d.UnitCommit = s.UnitCommit;

            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private bool SaveAR_LTTContractDetail(FormCollection data, string lTTContractNbr)
        {
            StoreDataHandler dataLTTContractDetail = new StoreDataHandler(data["lstGridLTTContractDetail"]);
            ChangeRecords<AR_LTTContractDetail> lstGridLTTContractDetail = dataLTTContractDetail.BatchObjectData<AR_LTTContractDetail>();

            foreach (AR_LTTContractDetail deleted in lstGridLTTContractDetail.Deleted)
            {
                var del = _db.AR_LTTContractDetail.Where(p => p.LTTContractNbr == deleted.LTTContractNbr && p.LineRef == deleted.LineRef && p.Type == deleted.Type).FirstOrDefault();
                if (del != null)
                {
                    _db.AR_LTTContractDetail.DeleteObject(del);
                    _db.SaveChanges();
                }
            }


            lstGridLTTContractDetail.Created.AddRange(lstGridLTTContractDetail.Updated);// bo cai Update vao Created luon cho gon
            foreach (AR_LTTContractDetail created in lstGridLTTContractDetail.Created)
            {

                var recordLTTContractDetail = _db.AR_LTTContractDetail.Where(p => p.LTTContractNbr == created.LTTContractNbr && p.LineRef == created.LineRef && p.Type == created.Type).FirstOrDefault();
                if (created.tstamp.ToHex() == "")//dong nay la dong them moi
                {
                    if (recordLTTContractDetail == null)
                    {
                        recordLTTContractDetail = new AR_LTTContractDetail();
                        recordLTTContractDetail.LTTContractNbr = lTTContractNbr;
                        //lấy LineRef tu dong
                        var lastRecordLTTContractDetail = _db.AR_LTTContractDetail.Where(p => p.LTTContractNbr == created.LTTContractNbr).LastOrDefault();
                        if (lastRecordLTTContractDetail != null)
                        {
                            var numberLineRefLTTBot = Convert.ToInt16(lastRecordLTTContractDetail.LineRef) + 1;
                            if (numberLineRefLTTBot < 10)
                            {
                                recordLTTContractDetail.LineRef = "0000" + Convert.ToString(numberLineRefLTTBot);
                            }
                            else if (numberLineRefLTTBot >= 10 && (numberLineRefLTTBot) < 100)
                            {
                                recordLTTContractDetail.LineRef = "000" + Convert.ToString(numberLineRefLTTBot);
                            }
                            else if (numberLineRefLTTBot >= 100 && (numberLineRefLTTBot) < 1000)
                            {
                                recordLTTContractDetail.LineRef = "00" + Convert.ToString(numberLineRefLTTBot);
                            }
                            else if (numberLineRefLTTBot >= 1000 && (numberLineRefLTTBot) < 10000)
                            {
                                recordLTTContractDetail.LineRef = "0" + Convert.ToString(numberLineRefLTTBot);
                            }
                            else if (numberLineRefLTTBot >= 10000 && (numberLineRefLTTBot) < 100000)
                            {
                                recordLTTContractDetail.LineRef = Convert.ToString(numberLineRefLTTBot);
                            }
                              
                        }
                        else
                        {
                            recordLTTContractDetail.LineRef = "00001";
                        }

                        recordLTTContractDetail.Crtd_Datetime = DateTime.Now;
                        recordLTTContractDetail.Crtd_Prog = screenNbr;
                        recordLTTContractDetail.Crtd_User = Current.UserName;
                        recordLTTContractDetail.tstamp = new byte[0];

                        UpdatingAR_LTTContractDetail(created, ref recordLTTContractDetail);
                        if (recordLTTContractDetail.LTTContractNbr != "" && recordLTTContractDetail.LineRef != "" && recordLTTContractDetail.Type != "")
                        {
                            _db.AR_LTTContractDetail.AddObject(recordLTTContractDetail);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
                    }
                }
                else //update
                {
                    if (created.tstamp.ToHex() == recordLTTContractDetail.tstamp.ToHex())
                    {
                        UpdatingAR_LTTContractDetail(created, ref recordLTTContractDetail);
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
            }






            //
            _db.SaveChanges();
            return true;
        }

        private void UpdatingAR_LTTContractDetail(AR_LTTContractDetail s, ref AR_LTTContractDetail d)
        {
            d.Amt = s.Amt;
            d.Descr = s.Descr;
            d.Status = s.Status;
            d.Type = s.Type;
            //d.LineRef = s.LineRef;

            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private bool SaveAR_CustAdvTool(FormCollection data,string custID)
        {
            StoreDataHandler dataCustAdvTool = new StoreDataHandler(data["lstGridAdv"]);
            ChangeRecords<AR_CustAdvTool> lstGridAdv = dataCustAdvTool.BatchObjectData<AR_CustAdvTool>();

            foreach (AR_CustAdvTool deleted in lstGridAdv.Deleted)
            {
                var delAR_CustAdvTool = _db.AR_CustAdvTool.Where(p => p.CustID == custID && p.LineRef == deleted.LineRef && p.Type == deleted.Type).FirstOrDefault();
                if (delAR_CustAdvTool != null)
                {
                    _db.AR_CustAdvTool.DeleteObject(delAR_CustAdvTool);
                    _db.SaveChanges();
                }
            }

            lstGridAdv.Created.AddRange(lstGridAdv.Updated);// bo cai Update vao Created luon cho gon
            foreach (AR_CustAdvTool created in lstGridAdv.Created)
            {

                var recordAdv = _db.AR_CustAdvTool.Where(p => p.CustID == custID && p.LineRef == created.LineRef && p.Type == created.Type).FirstOrDefault();
                if (created.tstamp.ToHex() == "")//dong nay la dong them moi
                {
                    if (recordAdv == null)
                    {
                        recordAdv = new AR_CustAdvTool();
                        recordAdv.CustID = custID;

                        //lấy LineRef tu dong
                        var lastRecordAdv = _db.AR_CustAdvTool.Where(p => p.CustID == custID).LastOrDefault();
                        if (lastRecordAdv != null)
                        {
                            var numberLineRefAdv = Convert.ToInt16(lastRecordAdv.LineRef) + 1;
                            if (numberLineRefAdv < 10)
                            {
                                recordAdv.LineRef = "0000" + Convert.ToString(numberLineRefAdv);
                            }
                            else if (numberLineRefAdv >= 10 && (numberLineRefAdv) < 100)
                            {
                                recordAdv.LineRef = "000" + Convert.ToString(numberLineRefAdv);
                            }
                            else if (numberLineRefAdv >= 100 && (numberLineRefAdv) < 1000)
                            {
                                recordAdv.LineRef = "00" + Convert.ToString(numberLineRefAdv);
                            }
                            else if (numberLineRefAdv >= 1000 && (numberLineRefAdv) < 10000)
                            {
                                recordAdv.LineRef = "0" + Convert.ToString(numberLineRefAdv);
                            }
                            else if (numberLineRefAdv >= 10000 && (numberLineRefAdv) < 100000)
                            {
                                recordAdv.LineRef = Convert.ToString(numberLineRefAdv);
                            }

                        }
                        else
                        {
                            recordAdv.LineRef = "00001";
                        }



                        recordAdv.Crtd_Datetime = DateTime.Now;
                        recordAdv.Crtd_Prog = screenNbr;
                        recordAdv.Crtd_User = Current.UserName;
                        recordAdv.tstamp = new byte[0];

                        UpdatingAR_CustAdvTool(created, ref recordAdv);
                        if (recordAdv.CustID != "" && recordAdv.LineRef != "" && recordAdv.Type != "")
                        {
                            _db.AR_CustAdvTool.AddObject(recordAdv);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
                    }
                }
                else //update
                {
                    if (created.tstamp.ToHex() == recordAdv.tstamp.ToHex())
                    {
                        UpdatingAR_CustAdvTool(created, ref recordAdv);
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
            }

            //
            _db.SaveChanges();
            return true;
        }

        private void UpdatingAR_CustAdvTool(AR_CustAdvTool s, ref AR_CustAdvTool d)
        {
            d.Active = s.Active;
            d.Type = s.Type;
            d.Descr = s.Descr;
            d.Qty = s.Qty;
            d.Amt = s.Amt;
            d.FitupDate = s.FitupDate;
            
            d.Status = s.Status;
            //d.LineRef = s.LineRef;
            
            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }


        private bool SaveAR_CustSellingProducts(FormCollection data, string custID)
        {
            StoreDataHandler dataSellingProd = new StoreDataHandler(data["lstGridSellingProd"]);
            ChangeRecords<AR_CustSellingProducts> lstGridSellingProd = dataSellingProd.BatchObjectData<AR_CustSellingProducts>();

            foreach (AR_CustSellingProducts deleted in lstGridSellingProd.Deleted)
            {
                var delCustSellingProducts = _db.AR_CustSellingProducts.Where(p => p.CustID == custID && p.Code == deleted.Code).FirstOrDefault();
                if (delCustSellingProducts != null)
                {
                    _db.AR_CustSellingProducts.DeleteObject(delCustSellingProducts);
                    _db.SaveChanges();
                }
            }

            lstGridSellingProd.Created.AddRange(lstGridSellingProd.Updated);// bo cai Update vao Created luon cho gon
            foreach (AR_CustSellingProducts created in lstGridSellingProd.Created)
            {

                var recordSellingProd = _db.AR_CustSellingProducts.Where(p => p.CustID == custID && p.Code == created.Code).FirstOrDefault();
                if (created.tstamp.ToHex() == "")//dong nay la dong them moi
                {
                    if (recordSellingProd == null)
                    {
                        recordSellingProd = new AR_CustSellingProducts();
                        recordSellingProd.CustID = custID;
                        recordSellingProd.Code = created.Code;
                        recordSellingProd.Crtd_Datetime = DateTime.Now;
                        recordSellingProd.Crtd_Prog = screenNbr;
                        recordSellingProd.Crtd_User = Current.UserName;
                        recordSellingProd.tstamp = new byte[0];

                        UpdatingAR_CustSellingProducts(created, ref recordSellingProd);
                        if (recordSellingProd.CustID != "" && recordSellingProd.Code != "" )
                        {
                            _db.AR_CustSellingProducts.AddObject(recordSellingProd);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
                    }
                }
                else //update
                {
                    if (created.tstamp.ToHex() == recordSellingProd.tstamp.ToHex())
                    {
                        UpdatingAR_CustSellingProducts(created, ref recordSellingProd);
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
            }

            //
            _db.SaveChanges();
            return true;
        }

        private void UpdatingAR_CustSellingProducts(AR_CustSellingProducts s, ref AR_CustSellingProducts d)
        {
           
            d.Descr = s.Descr;

            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }

        private bool SaveAR_CustDisplayMethod(FormCollection data, string custID)
        {
            StoreDataHandler dataDispMethod = new StoreDataHandler(data["lstGridDispMethod"]);
            ChangeRecords<AR_CustDisplayMethod> lstGridDispMethod = dataDispMethod.BatchObjectData<AR_CustDisplayMethod>();

            foreach (AR_CustDisplayMethod deleted in lstGridDispMethod.Deleted)
            {
                var delCustDisplayMethod = _db.AR_CustDisplayMethod.Where(p => p.CustID == custID && p.DispMethod == deleted.DispMethod).FirstOrDefault();
                if (delCustDisplayMethod != null)
                {
                    _db.AR_CustDisplayMethod.DeleteObject(delCustDisplayMethod);
                    _db.SaveChanges();
                }
            }


            lstGridDispMethod.Created.AddRange(lstGridDispMethod.Updated);// bo cai Update vao Created luon cho gon
            foreach (AR_CustDisplayMethod created in lstGridDispMethod.Created)
            {

                var recordDispMethod = _db.AR_CustDisplayMethod.Where(p => p.CustID == custID && p.DispMethod == created.DispMethod).FirstOrDefault();
                if (created.tstamp.ToHex() == "")//dong nay la dong them moi
                {
                    if (recordDispMethod == null)
                    {
                        recordDispMethod = new AR_CustDisplayMethod();
                        recordDispMethod.CustID = custID;
                        recordDispMethod.DispMethod = created.DispMethod;
                        recordDispMethod.Crtd_Datetime = DateTime.Now;
                        recordDispMethod.Crtd_Prog = screenNbr;
                        recordDispMethod.Crtd_User = Current.UserName;
                        recordDispMethod.tstamp = new byte[0];

                        UpdatingAR_CustDisplayMethod(created, ref recordDispMethod);
                        if (recordDispMethod.CustID != "" && recordDispMethod.DispMethod != "")
                        {
                            _db.AR_CustDisplayMethod.AddObject(recordDispMethod);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");//da co ung dung them record nay
                    }
                }
                else //update
                {
                    if (created.tstamp.ToHex() == recordDispMethod.tstamp.ToHex())
                    {
                        UpdatingAR_CustDisplayMethod(created, ref recordDispMethod);
                        _db.SaveChanges();
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
            }

            //
            _db.SaveChanges();
            return true;
        }

        private void UpdatingAR_CustDisplayMethod(AR_CustDisplayMethod s, ref AR_CustDisplayMethod d)
        {

            d.Descr = s.Descr;

            d.LUpd_Datetime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
































        //private void UpdatingAR_SOAddress()
        //{
        //    try
        //    {
              
        //        if (objAr_SOAddr != null)
        //        {
        //            _isNew = false;

        //        }
        //        else
        //        {
        //            _isNew = true;
        //            objAr_SOAddr = new AR_SOAddress();
        //            objAr_SOAddr.CustId = this.objAR_Customer.CustId;
        //            objAr_SOAddr.ShipToId = (cboDfltShipToId.SelectedItem == null ? "DEFAULT" : (cboDfltShipToId.SelectedItem as ppv_ShipToId_Result).ShipToId);
        //        }
        //        if (this.cboDfltShipToId.SelectedItem == null)
        //        {
        //            this.cboDfltShipToId.SelectedItem = lstppv_ShipToId_Result.Where(p => p.ShipToId.ToUpper() == "DEFAULT").FirstOrDefault();
        //        }
        //        //objAr_SOAddr.BranchID = this.txtBranchID.Text;
        //        objAr_SOAddr.BranchID = cboCpnyID.SelectedItem == null ? "" : (cboCpnyID.SelectedItem as SYS_Company).CpnyID;
        //        objAr_SOAddr.SOName = this.txtCustName.Text;
        //        objAr_SOAddr.Attn = this.txtAttn.Text;
        //        objAr_SOAddr.Addr1 = this.txtAddr1.Text;
        //        objAr_SOAddr.Addr2 = this.txtAddr2.Text;
        //        objAr_SOAddr.City = this.objAR_Customer.City;
        //        objAr_SOAddr.State = this.objAR_Customer.State;
        //        objAr_SOAddr.District = this.objAR_Customer.District;
        //        objAr_SOAddr.Zip = this.txtZip.Text;
        //        objAr_SOAddr.Country = this.objAR_Customer.Country;
        //        objAr_SOAddr.Phone = this.txtPhone.Text;
        //        objAr_SOAddr.Fax = this.txtFax.Text;
        //        objAr_SOAddr.TaxRegNbr = this.txtTaxRegNbr.Text;
        //        objAr_SOAddr.TaxLocId = this.txtTaxLocId.Text;
        //        objAr_SOAddr.TaxId00 = this.objAR_Customer.TaxID00;
        //        objAr_SOAddr.TaxId01 = this.objAR_Customer.TaxID01;
        //        objAr_SOAddr.TaxId02 = this.objAR_Customer.TaxID02;
        //        objAr_SOAddr.TaxId03 = this.objAR_Customer.TaxID03;
        //        objAr_SOAddr.Crtd_DateTime = objAR_Customer.Crtd_Datetime;
        //        objAr_SOAddr.Crtd_Prog = objAR_Customer.Crtd_Prog;
        //        objAr_SOAddr.Crtd_User = objAR_Customer.Crtd_User;
        //        objAr_SOAddr.LUpd_DateTime = objAR_Customer.LUpd_Datetime;
        //        objAr_SOAddr.LUpd_Prog = objAR_Customer.LUpd_Prog;
        //        objAr_SOAddr.LUpd_User = objAR_Customer.LUpd_User;
        //        objAr_SOAddr.tstamp = objAR_Customer.tstamp;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }


        //}  


        //if (objAR_Customer != null)
        // {

        ////updating
        //if (hadChild == 0)
        //{


        //    if (handle == "N" || handle == "")
        //    {
        //        objAR_Customer.Status = status;
        //    }
        //    else
        //    {
        //        objAR_Customer.Status = approveHandle.ToStatus;

        //    }
        //    if (objAR_Customer.Status == "O")
        //    {
        //        X.Msg.Show(new MessageBoxConfig()
        //        {
        //            Message = "Email sent!"
        //        });
        //        Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
        //                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
        //    }
        //    //Node


        //    //String[] nodeid = nodeID.Split('-');
        //    //objAR_Customer.NodeID = nodeid[0];
        //    //objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);

        //    UpdatingHeader(updated, ref objAR_Customer);

        //    _db.SaveChanges();
        //}
        //    else
        //    {
        //        if (handle == "N" || handle == "")
        //        {
        //            objAR_Customer.Status = status;
        //        }
        //        else
        //        {
        //            objAR_Customer.Status = approveHandle.ToStatus;

        //        }
        //        if (objAR_Customer.Status == "O")
        //        {
        //            X.Msg.Show(new MessageBoxConfig()
        //            {
        //                Message = "Email sent!"
        //            });
        //            Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
        //                         lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
        //        }

        //        String[] nodeid = nodeID.Split('-');
        //        if (objAR_Customer.NodeID != nodeid[0])
        //        {
        //            tmpChangeTreeDic = "1";
        //        }
        //        else
        //        {
        //            tmpChangeTreeDic = "0";
        //        }
        //        objAR_Customer.NodeID = nodeid[0];
        //        objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);
        //        var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
        //        objAR_Customer.ParentRecordID = searchparentRecordID.ParentRecordID;

        //        //Image and Media

        //        //String[] nodeid = nodeID.Split('-');
        //        //objAR_Customer.NodeID = nodeid[0];
        //        //objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);

        //        UpdatingHeader(updated, ref objAR_Customer);

        //        _db.SaveChanges();
        //    }

        //}
        //else
        //{
        //    //bo sung code add new copyForm neu update
        //    objAR_Customer = new AR_Customer();
        //    objAR_Customer.CustId = custID;
        //    objAR_Customer.BranchID = branchID;

        //    if (handle == "N" || handle == "")
        //    {
        //        objAR_Customer.Status = status;
        //    }
        //    else
        //    {
        //        objAR_Customer.Status = approveHandle.ToStatus;

        //    }
        //    if (objAR_Customer.Status == "O")
        //    {
        //        X.Msg.Show(new MessageBoxConfig()
        //        {
        //            Message = "Email sent!"
        //        });
        //        Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
        //                     lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
        //    }
        //    //statusAfterAll = objAR_Customer.Status;

        //    String[] nodeid = nodeID.Split('-');
        //    objAR_Customer.NodeID = nodeid[0];
        //    objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);
        //    var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
        //    objAR_Customer.ParentRecordID = searchparentRecordID.ParentRecordID;

        //    objAR_Customer.Crtd_Datetime = DateTime.Now;
        //    objAR_Customer.Crtd_Prog = screenNbr;
        //    objAR_Customer.Crtd_User = Current.UserName;
        //    objAR_Customer.tstamp = new byte[0];

        //    UpdatingHeader(updated, ref objAR_Customer);
        //    _db.AR_Customer.AddObject(objAR_Customer);
        //    _db.SaveChanges();




        //}

        //foreach (AR_Customer created in lstheader.Created)
        //{

        //    var objAR_Customer = _db.AR_Customer.Where(p => p.CustId == custID && p.BranchID == branchID).FirstOrDefault();
        //    if (objAR_Customer == null)
        //    {
        //        if (hadChild != 0)
        //        {

        //            objAR_Customer = new AR_Customer();
        //            objAR_Customer.CustId = custID;
        //            objAR_Customer.BranchID = branchID;

        //            if (handle == "N" || handle == "")
        //            {
        //                objAR_Customer.Status = status;
        //            }
        //            else
        //            {
        //                objAR_Customer.Status = approveHandle.ToStatus;

        //            }
        //            if (objAR_Customer.Status == "O")
        //            {
        //                X.Msg.Show(new MessageBoxConfig()
        //                {
        //                    Message = "Email sent!"
        //                });
        //                Approve.Mail_Approve(screenNbr, custID, Role, objAR_Customer.Status, handle,
        //                             lang, userNameLogin, branchID, brandID, string.Empty, string.Empty, string.Empty);
        //            }

        //            String[] nodeid = nodeID.Split('-');
        //            objAR_Customer.NodeID = nodeid[0];
        //            objAR_Customer.NodeLevel = Convert.ToInt16(nodeLevel);
        //            var searchparentRecordID = _db.SI_Hierarchy.Where(p => p.NodeID == parentRecordID && p.Type == "C").FirstOrDefault();
        //            objAR_Customer.ParentRecordID = searchparentRecordID.ParentRecordID;

        //            objAR_Customer.Crtd_Datetime = DateTime.Now;
        //            objAR_Customer.Crtd_Prog = screenNbr;
        //            objAR_Customer.Crtd_User = Current.UserName;
        //            objAR_Customer.tstamp = new byte[0];

        //            UpdatingHeader(created, ref objAR_Customer);
        //            _db.AR_Customer.AddObject(objAR_Customer);
        //            _db.SaveChanges();

        //        }
        //        else
        //        {
        //            return Json(new { success = false, code = "213" }, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //}

        // If there is a change in handling status (keepStatus is False),
        // add a new pending task with an approved handle.



        // ===============================================================

        // Get out of the loop (only update the first data)

    }
}

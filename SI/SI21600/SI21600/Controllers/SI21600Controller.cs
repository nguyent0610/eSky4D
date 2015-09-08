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
namespace SI21600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI21600Controller : Controller
    {
        private string _screenNbr = "SI21600";
        private string _userName = Current.UserName;
        SI21600Entities _sys = Util.CreateObjectContext<SI21600Entities>(false);

        public ActionResult Index()
        {  
            Util.InitRight(_screenNbr);
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetSI_Hierarchy(string Type, string RecordID)
        {
            int ID = RecordID.PassNull() == "" ? 0 : int.Parse(RecordID);
            var obj = _sys.SI_Hierarchy.FirstOrDefault(p => p.RecordID == ID && p.Type == Type);
            return this.Store(obj);
        }

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, string nodeType)
        {
            var node = new Node();
            var k = -1;
            if (inactiveHierachy.Descr == "Root")
            {
                node.Text = inactiveHierachy.Descr;
            }
            else
            {
                node.Text = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
                node.NodeID = inactiveHierachy.NodeID + "-" + inactiveHierachy.NodeLevel + "-" + inactiveHierachy.ParentRecordID.ToString()+"-"+inactiveHierachy.RecordID;   
                
            }

            var childrenInactiveHierachies = _sys.SI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.Type == nodeType
                    && p.NodeLevel == level).ToList();

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (SI_Hierarchy childrenInactiveNode in childrenInactiveHierachies)
                {
                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, nodeType));
                }
            }
            else
            {
                if (childrenInactiveHierachies.Count == 0)
                {
                    node.Leaf = true;
                }
                else
                {
                    node.Leaf = false;
                }
            }
            System.Diagnostics.Debug.WriteLine(node.Text);
            return node;
        }

        [DirectMethod]
        public ActionResult ReloadTreeSI21600(string cboType)
        {
            var root = new Node() { };
            var nodeType = cboType;

            var hierarchy = new SI_Hierarchy()
            {
                RecordID = 0,
                NodeID = "",
                ParentRecordID = 0,
                NodeLevel = 1,
                Descr = "Root",
                Type = nodeType
            };
            var z = 0;
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, nodeType);

            //quan trong dung de refresh slmTree
            this.GetCmp<TreePanel>("IDTree").SetRootNode(node);

            return this.Direct();
        }

    
      
        [HttpPost]
        public ActionResult Save(FormCollection data, string NodeID,short NodeLevel,int ParentRecordID,bool isNew)
        {
            try
            {
                string Type = data["cboType"];
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstSI_Hierarchy"]);
                ChangeRecords<SI_Hierarchy> lstSI_Hierarchy = dataHandler.BatchObjectData<SI_Hierarchy>();
                var header = new SI_Hierarchy();
              
                lstSI_Hierarchy.Created.AddRange(lstSI_Hierarchy.Updated);
                foreach (SI_Hierarchy curHeader in lstSI_Hierarchy.Created)
                {
                    if (NodeID.PassNull() == "") continue;

                    header = _sys.SI_Hierarchy.FirstOrDefault(p => p.NodeID == NodeID && p.NodeLevel==NodeLevel && p.ParentRecordID==ParentRecordID && p.Type==Type );
                    if (header != null && isNew == true) throw new MessageException(MessageType.Message, "19");

                    if (header != null)
                    {
                        
                            if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                            {
                                UpdatingHeader(ref header, curHeader);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        
                    }
                    else
                    {
                        var obj = _sys.SI_Hierarchy.FirstOrDefault(p => p.NodeID == NodeID && p.Type == Type);

                        if (obj==null)
                        {
                            header = new SI_Hierarchy();
                            header.NodeID = NodeID;
                            header.NodeLevel = NodeLevel;
                            header.ParentRecordID = ParentRecordID;
                            header.Type = Type;
                            header.RecordID = _sys.SI21600_ppMaxRC(Type).FirstOrDefault().Value;
                            header.Crtd_Datetime = DateTime.Now;
                            header.Crtd_Prog = _screenNbr;
                            header.Crtd_User = _userName;
                    
                            UpdatingHeader(ref header, curHeader);
                            _sys.SI_Hierarchy.AddObject(header);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "1112", parm: new[] { NodeID });
                        }
                    }
                }
             

                _sys.SaveChanges();
                return Json(new { success = true, NodeID = NodeID, RecordID = header.RecordID});
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }



        private void UpdatingHeader(ref SI_Hierarchy t, SI_Hierarchy s)
        {
            t.Descr = s.Descr;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }



        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string Type = data["cboType"];
                string NodeID = data["cboNodeID"];

                var obj = _sys.SI_Hierarchy.FirstOrDefault(p => p.NodeID == NodeID && p.Type==Type);
                if (obj != null)
                {
                    _sys.SI_Hierarchy.DeleteObject(obj);
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
    }
}

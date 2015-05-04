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

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        //public ActionResult GetSYS_CloseDateBranchAuto(string ID)
        //{
        //    int value = ID.PassNull()==""?0:int.Parse(ID);
        //    return this.Store(_sys.SI21600_pgSYS_CloseDateBranchAuto(value).ToList());
        //}

        //public ActionResult GetSYS_CloseDateAuto(string ID)
        //{
        //    int value = ID.PassNull() == "" ? 0 : int.Parse(ID);
        //    return this.Store(_sys.SI21600_pdHeader().FirstOrDefault(p => p.ID == value));
        //}

        private Node createNode(Node root, SI_Hierarchy inactiveHierachy, int level, int z)
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
                node.NodeID = inactiveHierachy.NodeID.ToString() + "-" + inactiveHierachy.Descr.ToString();
            }

            var childrenInactiveHierachies = _sys.SI_Hierarchy
                .Where(p => p.ParentRecordID == inactiveHierachy.RecordID
                    && p.Type == inactiveHierachy.Type
                    && p.NodeLevel == level).ToList();

            if (childrenInactiveHierachies != null && childrenInactiveHierachies.Count > 0)
            {
                foreach (SI_Hierarchy childrenInactiveNode in childrenInactiveHierachies)
                {
                    node.Children.Add(createNode(node, childrenInactiveNode, level + 1, z++));
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
            Node node = createNode(root, hierarchy, hierarchy.NodeLevel, z);

            //quan trong dung de refresh slmTree
            this.GetCmp<TreePanel>("IDTree").SetRootNode(node);

            return this.Direct();
        }

        //#region Save & Update
        ////Save information Company
        //[HttpPost]
        //public ActionResult Save(FormCollection data)
        //{
        //    try
        //    {
        //        string ID_temp = data["cboID"].PassNull();
        //        int ID = ID_temp == "" ? 0 : int.Parse(ID_temp);
        //        string Time = "";
        //        string time_temp = data["txtTime"].PassNull();
        //        string time_cut = time_temp.Substring(time_temp.Length-2,2);
        //        if(time_cut=="pm")
        //        {
        //            int index= time_temp.IndexOf(":");
        //            int plus = int.Parse(time_temp.Substring(0, index))+12;
        //            if (plus == 24)
        //            {
        //                plus = 0;

        //            }
        //            Time += plus + time_temp.Substring(index,3);
                  
        //        }
        //        else
        //        {
        //            int index = time_temp.IndexOf(":");
        //            int plus = int.Parse(time_temp.Substring(0, index));
        //            if (index == 1)
        //            {
        //                Time = "0";
        //            }
        //            if (plus == 12)
        //            {
        //                Time += "0";
        //                plus = 0;
        //            }
        //            Time +=plus +time_temp.Substring(index, 3);
        //        }
                
                
        //        StoreDataHandler dataHandler = new StoreDataHandler(data["lstSYS_CloseDateAuto"]);
        //        ChangeRecords<SI21600_pdHeader_Result> lstSYS_CloseDateAuto = dataHandler.BatchObjectData<SI21600_pdHeader_Result>();

        //        StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstSYS_CloseDateBranchAuto"]);
        //        ChangeRecords<SI21600_pgSYS_CloseDateBranchAuto_Result> lstSYS_CloseDateBranchAuto = dataHandler1.BatchObjectData<SI21600_pgSYS_CloseDateBranchAuto_Result>();


        //        #region Save Header SYS_CloseDateAuto
        //        lstSYS_CloseDateAuto.Created.AddRange(lstSYS_CloseDateAuto.Updated);
        //        foreach (SI21600_pdHeader_Result curHeader in lstSYS_CloseDateAuto.Created)
        //        {
        //            if (ID.PassNull() == "") continue;

        //            var header = _sys.SYS_CloseDateAuto.FirstOrDefault(p => p.ID == ID);
        //            if (header != null)
        //            {
        //                if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
        //                {
        //                    header.Time = Time;
        //                    UpdatingHeader(ref header, curHeader);
        //                }
        //                else
        //                {
        //                    throw new MessageException(MessageType.Message, "19");
        //                }
        //            }
        //            else
        //            {
        //                var iID = _sys.SI21600_GetAutoNumber().FirstOrDefault() ;

        //                header = new SYS_CloseDateAuto();
        //                header.ID = iID.Value;
        //                header.Time = Time;
        //                header.Crtd_DateTime = DateTime.Now;
        //                header.Crtd_Prog = _screenNbr;
        //                header.Crtd_User = Current.UserName;
        //                UpdatingHeader(ref header, curHeader);
        //                _sys.SYS_CloseDateAuto.AddObject(header);
        //            }
        //        }
        //        #endregion

        //        #region Save SYS_CloseDateBranchAuto
        //        foreach (SI21600_pgSYS_CloseDateBranchAuto_Result deleted in lstSYS_CloseDateBranchAuto.Deleted)
        //        {
        //            var objDelete = _sys.SYS_CloseDateBranchAuto.FirstOrDefault(p => p.ID == ID && p.BranchID == deleted.BranchID);
        //            if (objDelete != null)
        //            {
        //                _sys.SYS_CloseDateBranchAuto.DeleteObject(objDelete);
        //            }
        //        }

        //        lstSYS_CloseDateBranchAuto.Created.AddRange(lstSYS_CloseDateBranchAuto.Updated);

        //        foreach (SI21600_pgSYS_CloseDateBranchAuto_Result curLang in lstSYS_CloseDateBranchAuto.Created)
        //        {
        //            if (curLang.BranchID.PassNull() == "") continue;

        //            var lang = _sys.SYS_CloseDateBranchAuto.FirstOrDefault(p => p.ID== ID && p.BranchID.ToLower() == curLang.BranchID.ToLower());

        //            if (lang != null)
        //            {

        //                    throw new MessageException(MessageType.Message, "19");
        //            }
        //            else
        //            {
        //                lang = new SYS_CloseDateBranchAuto();
        //                lang.ID = ID;
        //                lang.BranchID = curLang.BranchID;
        //                _sys.SYS_CloseDateBranchAuto.AddObject(lang);
        //            }
        //        }
        //        #endregion

        //        _sys.SaveChanges();
        //        return Json(new { success = true, ID = ID });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}
        //#endregion

        ////Update SYS_CloseDateAuto
        //#region Update SYS_CloseDateAuto
        //private void UpdatingHeader(ref SYS_CloseDateAuto t, SI21600_pdHeader_Result s)
        //{
        //    t.Active = s.Active;
        //    t.Descr = s.Descr;
        //    t.UpDates = s.UpDates;
      
        //    t.LUpd_DateTime = DateTime.Now;
        //    t.LUpd_Prog = _screenNbr;
        //    t.LUpd_User = _userName;
        //}
        //#endregion

        //#region Delete information Company
        ////Delete information Company
        //[HttpPost]
        //public ActionResult DeleteAll(FormCollection data)
        //{
        //    try
        //    {
        //        string ID_temp = data["cboID"].PassNull();
        //        int ID = ID_temp == "" ? 0 : int.Parse(ID_temp);
        //        var cpny = _sys.SYS_CloseDateAuto.FirstOrDefault(p => p.ID == ID);
        //        if (cpny != null)
        //        {
        //            _sys.SYS_CloseDateAuto.DeleteObject(cpny);
        //        }

        //        var lstAddr = _sys.SYS_CloseDateBranchAuto.Where(p => p.ID == ID).ToList();
        //        foreach (var item in lstAddr)
        //        {
        //            _sys.SYS_CloseDateBranchAuto.DeleteObject(item);
        //        }

        //        _sys.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}
        //#endregion
    }
}

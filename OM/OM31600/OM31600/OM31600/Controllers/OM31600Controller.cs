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
using OM31600.Models;
namespace OM31600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM31600Controller : Controller
    {
        private string _userName = Current.UserName;
        private string _screenNbr = "OM31600";
        OM31600Entities _db = Util.CreateObjectContext<OM31600Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

       
        //
        // GET: /OM31600/
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            ViewBag.BussinessTime = DateTime.Now;
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

      
        public ActionResult GetDet(string territory, string state, string branch, DateTime? fromDate)
        {
            DateTime toDate = DateTime.Now;
            var dets = _db.OM31600_pgReportChecking(Current.CpnyID,Current.UserName,Current.LangID, branch, territory,state, fromDate, toDate).ToList();
            return this.Store(dets);
        }

        public ActionResult GetReportTop2(string branchID, string dayVisit, string slsperID)
        {
            DateTime dayvisit = Convert.ToDateTime(dayVisit.ToString());
            return this.Store(_db.OM31600_pgReportTop2(Current.CpnyID, Current.UserName, Current.LangID, branchID,slsperID, dayvisit).ToList());
        }

        public ActionResult GetReportTop3(string branchID, string custID,string dayVisit,string slsperID)
        {
            DateTime dayvisit = Convert.ToDateTime(dayVisit.ToString());
            var a= _db.OM31600_pgReportTop3(Current.CpnyID, Current.UserName, Current.LangID, branchID, slsperID, dayvisit, custID).ToList();

            return this.Store(a);
        }


        //public ActionResult GetGridLevel3(string id)
        //{
        //    var lstReportTop3 = _db.OM31600_pgReportTop3(Current.CpnyID, Current.UserName, Current.LangID).ToList();
        //    GridPanel grid = new GridPanel
        //    {
        //        Height = 200,
        //        EnableColumnHide = false,
        //        Store =
        //        {
        //            new Store
        //            {
        //                Model =
        //                {
        //                    new Model {
        //                        IDProperty = "CustId",
        //                        Fields = {
        //                            new ModelField("OM31600_Time"),
        //                            new ModelField("Step")
        //                        }
        //                    }
        //                },
        //                DataSource = lstReportTop3
        //            }
        //        },
        //        ColumnModel =
        //        {
        //            Columns =
        //            {
        //                new Column { Text = "OM31600_Time", DataIndex = "OM31600_Time" },
        //                new Column { Text = "Step", DataIndex = "Step" }
        //            }
        //        }
        //    };

        //    return this.ComponentConfig(grid);
        //}


    }
}

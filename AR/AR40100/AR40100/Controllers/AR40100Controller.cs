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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;
using ARProcess;

namespace AR40100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR40100Controller : Controller
    {
        private string _screenNbr = "AR40100";
        private string _userName = Current.UserName;
        AR40100Entities _db = Util.CreateObjectContext<AR40100Entities>(false);
        private JsonResult _logMessage;
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

        public ActionResult GetData()
        {
            return this.Store(_db.AR40100_pgBatch(Current.UserName,Current.CpnyID,Current.LangID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                var detHeader = new StoreDataHandler(data["lstData"]);
                var lstData = detHeader.ObjectData<AR40100_pgBatch_Result>().Where(p => p.Selected == true).ToList();
                string ErrChange = "";//những đơn đã thay đổi
                string message = "";//những đơn đã thay đổi
                if (lstData.Count > 0)
                {
                    var lstOldData = _db.AR40100_pgBatch(Current.UserName, Current.CpnyID, Current.LangID).ToList();
                   
                    DataAccess dal = Util.Dal();
                    foreach (var obj in lstData)
                    {
                        //kiểm tra tstamp thông báo 1 lần
                        var objData = lstOldData.Where(p => p.BranchID == obj.BranchID && p.BatNbr == obj.BatNbr).FirstOrDefault();
                        if (objData.tstamp.ToHex() == obj.tstamp.ToHex())//nếu giống nhau thì gọi hàm release
                        {
                            try
                            {
                                AR om = new AR(Current.UserName, _screenNbr, dal);

                                dal.BeginTrans(IsolationLevel.ReadCommitted);
                                if (!om.AR10100_Release(objData.BranchID, objData.BatNbr))
                                {
                                    dal.RollbackTrans();
                                }
                                else
                                {
                                    dal.CommitTrans();                                  
                                }
                            }
                            catch (Exception ex)
                            {
                                dal.RollbackTrans();
                                if (ex is MessageException)
                                {
                                    var msg = ex as MessageException;
                                    message += "Lô" + objData.BranchID +" - "+objData.BatNbr + ":" + Message.GetString(msg.Code, msg.Parm) + "</br>";
                                }
                                else
                                {
                                    message += "Lô" + objData.BranchID + " - " + objData.BatNbr + " bị lỗi: " + ex.ToString() + "</br>";
                                }
                            }
                        }
                        else //cộng chuỗi những đơn lỗi
                        {
                            ErrChange += objData.BranchID + " - " + objData.BatNbr + ",";
                        }

                    }
                }
                if (message == "" && ErrChange == "")
                    return Util.CreateMessage(MessageProcess.Process);
                else
                {
                    if (ErrChange!="")
                        message += "Lô" + ErrChange + ": " + Message.GetString("19",new string[]{}) + "</br>";
                    throw new MessageException("20410", parm: new[] { message });
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

     

    }
}

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
using Ionic.Zip;
using Aspose.Cells;
using System.Drawing;
using HQFramework.DAL;
using HQFramework.Common;
namespace SA20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SA20100Controller : Controller
    {
        private string _screenNbr = "SA20100";
        private string _userName = Current.UserName;
        private string _cpnyID = Current.CpnyID;
        SA20100Entities _db = Util.CreateObjectContext<SA20100Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        public ActionResult Index()
        {
            Util.InitRight(_screenNbr);
            bool showContentEng = false;
            var objConfig = _db.SA20100_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                showContentEng = objConfig.Value;
            }
            ViewBag.showContentEng = showContentEng;
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetStatus()
        {
            var data = _db.SA20100_pgLoadStatus(Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(data);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstPotential"]);
                ChangeRecords<SA20100_pgLoadStatus_Result> lstSI_Status = dataHandler.BatchObjectData<SA20100_pgLoadStatus_Result>();


                lstSI_Status.Created.AddRange(lstSI_Status.Updated);

                foreach (SA20100_pgLoadStatus_Result del in lstSI_Status.Deleted)
                {
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp upStatusID
                    if (lstSI_Status.Created.Where(p => p.StatusID.ToUpper() == del.StatusID.ToUpper() && p.StatusType.ToUpper() == del.StatusType.ToUpper()).Count() > 0)
                    {
                        lstSI_Status.Created.Where(p => p.StatusID.ToUpper() == del.StatusID.ToUpper() && p.StatusType.ToUpper() == del.StatusType.ToUpper()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.SI_Status.ToList().Where(p => p.StatusID.ToUpper() == del.StatusID.ToUpper() && p.StatusType.ToUpper() == del.StatusType.ToUpper()).FirstOrDefault();
                        if (objDel != null)
                        {
                            if (_db.SA20100_pdCheckbeforedelete(Current.CpnyID, Current.UserName, Current.LangID, objDel.StatusID, objDel.StatusType).FirstOrDefault().Value == 1){
                                var objStatusType = _db.SA20100_pcStatusType(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
                                var objStatusTypeDescr = objStatusType != null ? objStatusType.Descr : "";

                                throw new MessageException(MessageType.Message, "2018081760", "", new string[] { Util.GetLang("StatusType"), objStatusTypeDescr, Util.GetLang("LangStatus"), objDel.StatusID });
                            }
                            else
                                _db.SI_Status.DeleteObject(objDel);
                        }
                    }
                }

                foreach (SA20100_pgLoadStatus_Result curItem in lstSI_Status.Created)
                {
                    if (curItem.StatusID.PassNull() == "" || curItem.StatusType.PassNull() == "") continue;

                    var item = _db.SI_Status.Where(p => p.StatusID.ToUpper() == curItem.StatusID.ToUpper() && p.StatusType.ToUpper() == curItem.StatusType.ToUpper()).FirstOrDefault();

                    if (item != null)
                    {
                        if (item.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_SIStatus(item, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        if (curItem.StatusID.PassNull() != "")
                        {
                            item = new SI_Status();
                            item.ResetET();
                            Update_SIStatus(item, curItem, true);
                            _db.SI_Status.AddObject(item);
                        }  
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        private void Update_SIStatus(SI_Status t, SA20100_pgLoadStatus_Result s, bool isNew)
        {
            if (isNew)
            {
                t.StatusID = s.StatusID.ToUpper();
                t.StatusType = s.StatusType.ToUpper();
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.StatusName = s.StatusName;
            t.LangID = s.LangID;
            t.IsDefault = s.IsDefault;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;



        }
               
    }
}


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
namespace OM21900.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21900Controller : Controller
    {
        private string _screenNbr = "OM21900";
        private string _userName = Current.UserName;
        OM21900Entities _db = Util.CreateObjectContext<OM21900Entities>(false);

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

        public ActionResult GetOM_HOKPI(string kpiId)
        {
            return this.Store(_db.OM_HOKPI.FirstOrDefault(p => p.ID == kpiId));
        }

        public ActionResult GetOM_HOKPIDetail(string kpiId)
        {
            return this.Store(_db.OM21900_pgHOKPIDetails(kpiId).ToList());
        }


        #region Save & Update information Company
        //Save information Company
        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string ID = data["cboID"].PassNull();
                string Status = data["cboStatus"].PassNull();
                string Handle = data["cboHandle"].PassNull();

                StoreDataHandler detHeader = new StoreDataHandler(data["lstOM_HOKPI"]);
                OM_HOKPI curHeader = detHeader.ObjectData<OM_HOKPI>().FirstOrDefault();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstOM_HOKPIDetail"]);
                ChangeRecords<OM21900_pgHOKPIDetails_Result> lstOM_HOKPIDetail = dataHandler1.BatchObjectData<OM21900_pgHOKPIDetails_Result>();

                #region Save Header 

                var header = _db.OM_HOKPI.FirstOrDefault(p => p.ID == ID);
                if (header != null)
                {
                    if (header.tstamp.ToHex() == curHeader.tstamp.ToHex())
                    {
                        UpdatingHeader(ref header, curHeader, Status, Handle);
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }
                else
                {
                    //string images = getPathThenUploadImage(curHeader, UserID);
                    header = new OM_HOKPI();
                    header.ResetET();
                    header.ID = ID;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader, Status, Handle);
                    _db.OM_HOKPI.AddObject(header);
                }

                #endregion

                #region Save OM_HOKPIDetail
                foreach (OM21900_pgHOKPIDetails_Result deleted in lstOM_HOKPIDetail.Deleted)
                {
                    var objDelete = _db.OM_HOKPIDetail.FirstOrDefault(p => p.ID == ID && p.Branch == deleted.Branch && p.Area == deleted.Area && p.SKU == deleted.SKU);
                    if (objDelete != null)
                    {
                        _db.OM_HOKPIDetail.DeleteObject(objDelete);
                    }
                }

                lstOM_HOKPIDetail.Created.AddRange(lstOM_HOKPIDetail.Updated);

                foreach (OM21900_pgHOKPIDetails_Result curLang in lstOM_HOKPIDetail.Created)
                {
                    if (ID.PassNull() == "" || curLang.Branch.PassNull() == "" || curLang.Area.PassNull() == "" || curLang.SKU.PassNull() == "") continue;

                    var lang = _db.OM_HOKPIDetail.FirstOrDefault(p =>  p.ID.ToLower() == ID.ToLower()
                                                                    && p.Branch.ToLower() == curLang.Branch.ToLower() 
                                                                    && p.Area.ToLower() == curLang.Area.ToLower() 
                                                                    && p.SKU.ToLower() == curLang.SKU.ToLower());

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
                        {
                            UpdatingOM_HOKPIDetail(lang, curLang, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_HOKPIDetail();
                        lang.ResetET();
                        lang.ID = ID;
                        UpdatingOM_HOKPIDetail(lang, curLang, true);
                        _db.OM_HOKPIDetail.AddObject(lang);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, ID = ID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        //Update Header Company
        private void UpdatingHeader(ref OM_HOKPI t, OM_HOKPI s,string Status,string Handle)
        {
            if (Handle == string.Empty || Handle == "N")
                t.Status = Status;
            else
                t.Status = Handle;
            t.Descr = s.Descr;
            t.FromDate = s.FromDate;
            t.EndDate = s.EndDate;
  
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        //Update OM_HOKPIDetail
        #region Update OM_HOKPIDetail
        private void UpdatingOM_HOKPIDetail(OM_HOKPIDetail t, OM21900_pgHOKPIDetails_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Branch = s.Branch;
                t.Area = s.Area;
                t.SKU = s.SKU;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Target = s.Target;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;

        }
        #endregion

        #region Delete
        //Delete information Company
        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string ID = data["cboID"];
                var header = _db.OM_HOKPI.FirstOrDefault(p => p.ID == ID);
                if (header != null)
                {
                    _db.OM_HOKPI.DeleteObject(header);
                }

                var lstOM_HOKPIDetail = _db.OM_HOKPIDetail.Where(p => p.ID == ID).ToList();
                foreach (var item in lstOM_HOKPIDetail)
                {
                    _db.OM_HOKPIDetail.DeleteObject(item);
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
        #endregion
    }
}

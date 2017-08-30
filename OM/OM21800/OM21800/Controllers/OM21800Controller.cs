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
using HQ.eSkySys;

namespace OM21800.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM21800Controller : Controller
    {
        private string _screenNbr = "OM21800";
        private string _userName = Current.UserName;
        OM21800Entities _db = Util.CreateObjectContext<OM21800Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;
        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM21800111");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\OM21800");
                }
                return _filePath;
            }
        }
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

        public ActionResult GetOM_DiscountInfor()
        {
            return this.Store(_db.OM21800_pgOM_DiscountInfor().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string POSMID = data["cboPOSMID"].PassNull();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_DiscountInfor"]);
                ChangeRecords<OM21800_pgOM_DiscountInfor_Result> lstOM_DiscountInfor = dataHandler.BatchObjectData<OM21800_pgOM_DiscountInfor_Result>();

                lstOM_DiscountInfor.Created.AddRange(lstOM_DiscountInfor.Updated);// Dua danh sach update chung vao danh sach tao moi
                foreach (OM21800_pgOM_DiscountInfor_Result del in lstOM_DiscountInfor.Deleted)
                {
                    if (lstOM_DiscountInfor.Created.Where(p => p.Territory.ToLower().Trim() == del.Territory.ToLower().Trim()
                                                            && p.DiscID.ToLower().Trim() == del.DiscID.ToLower().Trim()
                                                            && p.DiscSeq.ToLower().Trim() == del.DiscSeq.ToLower().Trim()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstOM_DiscountInfor.Created.Where(p => p.Territory.ToLower().Trim() == del.Territory.ToLower().Trim()
                                                            && p.DiscID.ToLower().Trim() == del.DiscID.ToLower().Trim()
                                                            && p.DiscSeq.ToLower().Trim() == del.DiscSeq.ToLower().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_DiscountInfor.Where(p => p.Territory == del.Territory
                                                                && p.DiscID == del.DiscID
                                                                && p.DiscSeq == del.DiscSeq).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_DiscountInfor.DeleteObject(objDel);
                        }
                    }
                }
                foreach (OM21800_pgOM_DiscountInfor_Result curItem in lstOM_DiscountInfor.Created)
                {
                    if (curItem.Territory.PassNull() == "" || curItem.DiscID.PassNull() =="" || curItem.DiscSeq.PassNull()=="") continue;

                    var lang = _db.OM_DiscountInfor.Where(p => p.Territory.ToLower() == curItem.Territory.ToLower()
                                                            && p.DiscID.ToLower() == curItem.DiscID.ToLower()
                                                            && p.DiscSeq.ToLower() == curItem.DiscSeq.ToLower()).FirstOrDefault();
                    #region Upload files
                    var files = Request.Files;
                    if (files.Count > 0 && files[0].ContentLength > 0) // Co chon file de upload
                    {
                        // Xoa file cu di
                        var oldPath = string.Format("{0}\\{1}", FilePath, curItem.Poster);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                        // Upload file moi
                        string newFileName = string.Format("{0}", files[0].FileName);
                        files[0].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                        curItem.Poster = newFileName;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(curItem.Poster) && string.IsNullOrWhiteSpace(lang.Poster))
                        {
                            // Xoa file cu di
                            var oldPath = string.Format("{0}\\{1}", FilePath, curItem.Poster);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                            curItem.Poster = string.Empty;
                        }
                    }
                    #endregion
                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_Language(lang, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_DiscountInfor();
                        lang.ResetET();
                        Update_Language(lang, curItem, true);
                        _db.OM_DiscountInfor.AddObject(lang);
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

        private void Update_Language(OM_DiscountInfor t, OM21800_pgOM_DiscountInfor_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Territory = s.Territory;
                t.DiscID = s.DiscID;
                t.DiscSeq = s.DiscSeq;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Channel = s.Channel;
            t.DescrTerr = s.DescrTerr;
            t.Descr = s.Descr;
            t.ClassID = s.ClassID;
            t.DescrClass = s.DescrClass;
            t.StartDate = s.StartDate;
            t.EndDate = s.EndDate;
            t.Poster = s.Poster;
            t.Status = s.Status;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        //[HttpPost]
        //public ActionResult DeleteAll(FormCollection data)
        //{
        //    try
        //    {
        //        string POSMID = data["cboPOSMID"].PassNull();

        //        var lst = _db.OM_DiscountInfor.Where(p => p.POSMID == POSMID).ToList();
        //        foreach (var item in lst)
        //        {
        //            _db.OM_DiscountInfor.DeleteObject(item);
        //        }

        //        _db.SaveChanges();
        //        return Json(new { success = true, POSMID = POSMID });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}

    }
}

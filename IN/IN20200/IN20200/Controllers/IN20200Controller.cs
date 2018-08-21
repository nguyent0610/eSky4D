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

namespace IN20200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20200Controller : Controller
    {
        string _screenNbr = "IN20200";
        IN20200Entities _db = Util.CreateObjectContext<IN20200Entities>(false);
        private JsonResult _logMessage;

        public ActionResult Index()
        {
            bool DfltValMthd = false;
            var objConfig = _db.IN20200_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (objConfig != null)
            {
                DfltValMthd = objConfig.Value;
            }
            ViewBag.DfltValMthd = DfltValMthd;
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetIN_ProductClass(string ClassID)
        {
            return this.Store(_db.IN_ProductClass.FirstOrDefault(p => p.ClassID == ClassID));
        }

        public ActionResult GetCpny(string ClassID)
        {
            return this.Store(_db.IN20200_pgLoadgetCompany(ClassID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string ClassID = data["cboClassID"].PassNull();

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstIN_ProductClass"]);
                IN_ProductClass curHeader = dataHandler.ObjectData<IN_ProductClass>().FirstOrDefault();

                StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstCpny"]);
                ChangeRecords<IN20200_pgLoadgetCompany_Result> lstCpny = dataHandler1.BatchObjectData<IN20200_pgLoadgetCompany_Result>();

                #region Save IN_ProductClass
                var header = _db.IN_ProductClass.FirstOrDefault(p => p.ClassID == ClassID);
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
                    header = new IN_ProductClass();
                    header.ResetET();
                    header.ClassID = ClassID;
                    header.Crtd_DateTime = DateTime.Now;
                    header.Crtd_Prog = _screenNbr;
                    header.Crtd_User = Current.UserName;
                    UpdatingHeader(ref header, curHeader);
                    _db.IN_ProductClass.AddObject(header);
                }
                #endregion

                #region Save Cpny
                foreach (IN20200_pgLoadgetCompany_Result deleted in lstCpny.Deleted)
                {
                    var objDelete = _db.IN_ProdClassCpny.Where(p => p.ClassID == ClassID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                    if (objDelete != null)
                    {
                        _db.IN_ProdClassCpny.DeleteObject(objDelete);
                    }
                }

                lstCpny.Created.AddRange(lstCpny.Updated);

                foreach (IN20200_pgLoadgetCompany_Result curLang in lstCpny.Created)
                {
                    if (ClassID.PassNull() == "" || curLang.CpnyID.PassNull() == "") continue;

                    var lang = _db.IN_ProdClassCpny.FirstOrDefault(p => p.ClassID.ToLower() == ClassID.ToLower() && p.CpnyID.ToLower() == curLang.CpnyID.ToLower());

                    if (lang == null)
                    {
                        lang = new IN_ProdClassCpny();
                        lang.ResetET();
                        lang.ClassID = ClassID;
                        lang.CpnyID = curLang.CpnyID;

                        lang.Crtd_DateTime = DateTime.Now;
                        lang.Crtd_Prog = _screenNbr;
                        lang.Crtd_User = Current.UserName;
                        lang.LUpd_DateTime = DateTime.Now;
                        lang.LUpd_Prog = _screenNbr;
                        lang.LUpd_User = Current.UserName;

                        _db.IN_ProdClassCpny.AddObject(lang);
                    }
                }
                #endregion

                _db.SaveChanges();
                return Json(new { success = true, ClassID = ClassID });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void UpdatingHeader(ref IN_ProductClass t,IN_ProductClass s)
        {
            t.Public = s.Public;
            t.Descr = s.Descr;
            t.DfltInvtType = s.DfltInvtType;
            t.DfltStkItem = s.DfltStkItem;
            t.DfltSource = s.DfltSource;
            t.DfltValMthd = s.DfltValMthd;
            t.DfltLotSerTrack = s.DfltLotSerTrack;
            t.Buyer = s.Buyer;
            t.DfltStkUnit = s.DfltStkUnit;
            t.DfltPOUnit = s.DfltPOUnit;
            t.DfltSOUnit = s.DfltSOUnit;
            t.MaterialType = s.MaterialType;
            t.DfltSite = s.DfltSite;
            t.DfltSlsTaxCat = s.DfltSlsTaxCat;

            if (t.DfltLotSerTrack == "N")
            {
                t.DfltLotSerAssign = null;
                t.DfltLotSerMthd = null;
                t.DfltLotSerShelfLife = 0;
                t.DfltWarrantyDays = 0;
                t.DfltLotSerFxdTyp = null;
                t.DfltLotSerFxdLen = 0;
                t.DfltLotSerFxdVal = "";
                t.DfltLotSerNumLen = 0;
                t.DfltLotSerNumVal = "";
            }
            else
            {
                t.DfltLotSerAssign = s.DfltLotSerAssign;
                t.DfltLotSerMthd = s.DfltLotSerMthd;
                t.DfltLotSerShelfLife = s.DfltLotSerShelfLife;
                t.DfltWarrantyDays = s.DfltWarrantyDays;
                t.DfltLotSerFxdTyp = s.DfltLotSerFxdTyp;
                t.DfltLotSerFxdLen = s.DfltLotSerFxdLen;
                t.DfltLotSerFxdVal = s.DfltLotSerFxdVal;
                t.DfltLotSerNumLen = s.DfltLotSerNumLen;
                t.DfltLotSerNumVal = s.DfltLotSerNumVal;
            }

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = Current.UserName;
        }

        [HttpPost]
        public ActionResult DeleteAll(FormCollection data)
        {
            try
            {
                string ClassID = data["cboClassID"];
                var Flag = _db.IN20200_ppCheckClassID(ClassID).FirstOrDefault();
                if (Flag == true)
                {
                    throw new MessageException(MessageType.Message, "2015101901", "");
                }
                else
                {
                    var obj = _db.IN_ProductClass.FirstOrDefault(p => p.ClassID == ClassID);
                    if (obj != null)
                    {
                        _db.IN_ProductClass.DeleteObject(obj);
                    }

                    var lstCpny = _db.IN_ProdClassCpny.Where(p => p.ClassID == ClassID).ToList();
                    foreach (var item in lstCpny)
                    {
                        _db.IN_ProdClassCpny.DeleteObject(item);
                    }

                    _db.SaveChanges();
                }
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

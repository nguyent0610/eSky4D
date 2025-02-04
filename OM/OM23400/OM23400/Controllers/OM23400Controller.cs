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
namespace OM23400.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23400Controller : Controller
    {
        private string _screenNbr = "OM23400";
        private string _ka = "KA";
        private string _rs = "RS";

        private string _monthType = "M";
        private string _quarterType = "Q";
        private string _yeatType = "Y";

        OM23400Entities _db = Util.CreateObjectContext<OM23400Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        //
        // GET: /OM23400/
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

        public ActionResult GetBonusInfo(string bonusID)
        {
            var bonus = _db.OM_TBonus.FirstOrDefault(x => x.BonusID == bonusID);
            return this.Store(bonus);
        }

        public ActionResult GetBonusRS(string bonusID)
        {
            var BonusRSs = _db.OM23400_pgBonusRS(Current.UserName, bonusID).ToList();
            return this.Store(BonusRSs);
        }

        public ActionResult GetProduct(string bonusID, string productType)
        {
            var Products = _db.OM23400_pgProduct(Current.UserName, bonusID, productType).ToList();
            return this.Store(Products);
        }

        public ActionResult GetBonusKA(string bonusID, string kaType)
        {
            var kas = _db.OM23400_pgBonusKA(Current.UserName, bonusID, kaType).ToList();
            return this.Store(kas);
        }

        public ActionResult GetBonusKADetail(string bonusID, string kaType)
        {
            var kaDetails = _db.OM23400_pgBonusKADetail(Current.UserName, bonusID, kaType).ToList();
            return this.Store(kaDetails);
        }

        public ActionResult SaveData(FormCollection data, bool isNew)
        {
            try
            {
                var bonusID = data["cboBonusID"];

                var bonusHandler = new StoreDataHandler(data["lstBonus"]);
                var bonusInput = bonusHandler.ObjectData<OM_TBonus>().FirstOrDefault();

                if (bonusInput != null && !string.IsNullOrWhiteSpace(bonusID))
                {
                    bonusInput.BonusID = bonusID;
                    var bonus = _db.OM_TBonus.FirstOrDefault(x => x.BonusID == bonusInput.BonusID);

                    if (bonus != null)
                    {
                        if (isNew)
                        {
                            throw new MessageException(MessageType.Message, "8001", "",
                                new string[]{
                                    Util.GetLang("BonusID")
                                });
                        }
                        else
                        {
                            if (bonus.tstamp.ToHex() == bonusInput.tstamp.ToHex())
                            {
                                updateBonus(ref bonus, bonusInput, false);
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                    }
                    else
                    {
                        bonus = new OM_TBonus();
                        updateBonus(ref bonus, bonusInput, true);
                        _db.OM_TBonus.AddObject(bonus);
                    }

                    if (bonusInput.Channel == _rs)
                    {
                        #region BonusRS
                        var BonusRSHandler = new StoreDataHandler(data["lstBonusRSChange"]);
                        var lstBonusRSChange = BonusRSHandler.BatchObjectData<OM23400_pgBonusRS_Result>();

                        foreach (var created in lstBonusRSChange.Created)
                        {
                            if (created.AmtBegin > 0 || created.AmtEnd > 0 || created.AmtBonus > 0)
                            {
                                created.BonusID = bonusInput.BonusID;

                                var createdBonusRS = _db.OM_TBonusRS.FirstOrDefault(
                                    x => x.LevelNbr == created.LevelNbr
                                        && x.BonusID == created.BonusID);
                                if (createdBonusRS == null)
                                {
                                    createdBonusRS = new OM_TBonusRS();
                                    updateBonusRS(ref createdBonusRS, created, true);
                                    _db.OM_TBonusRS.AddObject(createdBonusRS);
                                }
                            }
                        }

                        foreach (var updated in lstBonusRSChange.Updated)
                        {
                            if (updated.AmtBegin > 0 || updated.AmtEnd > 0 || updated.AmtEnd > 0)
                            {
                                updated.BonusID = bonusInput.BonusID;

                                var updatedBonusRS = _db.OM_TBonusRS.FirstOrDefault(
                                    x => x.LevelNbr == updated.LevelNbr
                                        && x.BonusID == updated.BonusID);
                                if (updatedBonusRS != null)
                                {
                                    updateBonusRS(ref updatedBonusRS, updated, true);
                                }
                            }
                        }

                        //foreach (var deleted in lstBonusRSChange.Deleted)
                        //{
                        //    if (deleted.AmtBegin > 0 || deleted.AmtEnd > 0 || deleted.AmtEnd > 0)
                        //    {
                        //        deleted.BonusID = bonusInput.BonusID;

                        //        var deletedBonusRS = _db.OM_TBonusRS.FirstOrDefault(
                        //            x => x.LevelNbr == deleted.LevelNbr
                        //                && x.BonusID == deleted.BonusID);
                        //        if (deletedBonusRS != null)
                        //        {
                        //            _db.OM_TBonusRS.DeleteObject(deletedBonusRS);
                        //        }
                        //    }
                        //}
                        #endregion

                        #region Product
                        // If defined product in grid (not all product)
                        var ProductHandler = new StoreDataHandler(data["lstProductChange"]);
                        var lstProductChange = ProductHandler.BatchObjectData<OM23400_pgProduct_Result>();

                        foreach (var created in lstProductChange.Created)
                        {
                            if (!string.IsNullOrWhiteSpace(created.ProductID))
                            {
                                created.BonusID = bonusInput.BonusID;
                                created.ProductType = bonusInput.RSApplyType;

                                var createdProduct = _db.OM_TBonusProduct.FirstOrDefault(
                                    x => x.ProductID == created.ProductID
                                        && x.BonusID == created.BonusID
                                        && x.ProductType == created.ProductType);
                                if (createdProduct == null)
                                {
                                    createdProduct = new OM_TBonusProduct();
                                    updateProduct(ref createdProduct, created, true);
                                    _db.OM_TBonusProduct.AddObject(createdProduct);
                                }
                            }
                        }

                        foreach (var updated in lstProductChange.Updated)
                        {
                            if (!string.IsNullOrWhiteSpace(updated.BonusID))
                            {
                                updated.BonusID = bonusInput.BonusID;
                                updated.ProductType = bonusInput.RSApplyType;

                                var updatedProduct = _db.OM_TBonusProduct.FirstOrDefault(
                                    x => x.ProductID == updated.ProductID
                                        && x.BonusID == updated.BonusID
                                        && x.ProductType == updated.ProductType);
                                if (updatedProduct != null)
                                {
                                    updateProduct(ref updatedProduct, updated, true);
                                }
                            }
                        }

                        foreach (var deleted in lstProductChange.Deleted)
                        {
                            if (!string.IsNullOrWhiteSpace(deleted.BonusID))
                            {
                                deleted.BonusID = bonusInput.BonusID;
                                deleted.ProductType = bonusInput.RSApplyType;

                                var deletedProduct = _db.OM_TBonusProduct.FirstOrDefault(
                                    x => x.ProductID == deleted.ProductID
                                        && x.BonusID == deleted.BonusID
                                        && x.ProductType == deleted.ProductType);
                                if (deletedProduct != null)
                                {
                                    _db.OM_TBonusProduct.DeleteObject(deletedProduct);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (bonusInput.Channel == _ka)
                    {
                        #region Month/Quarter/Year
                        var dicMQY = new Dictionary<string, string[]>();
                        dicMQY.Add(_monthType, new string[] { "lstMonthChange", "lstMonthDetailChange" });
                        dicMQY.Add(_quarterType, new string[] { "lstQuarterChange", "lstQuarterDetailChange" });
                        dicMQY.Add(_yeatType, new string[] { "lstYearChange", "lstYearDetailChange" });

                        foreach (var mqy in dicMQY)
                        {
                            var mqyHandler = new StoreDataHandler(data[mqy.Value[0]]);
                            var lstMqyChange = mqyHandler.BatchObjectData<OM23400_pgBonusKA_Result>();

                            var mqyDetailHandler = new StoreDataHandler(data[mqy.Value[1]]);
                            var lstMqyDetailChange = mqyDetailHandler.BatchObjectData<OM23400_pgBonusKADetail_Result>();

                            // Bonus KA
                            lstMqyChange.Updated.AddRange(lstMqyChange.Created);
                            foreach (var updated in lstMqyChange.Updated)
                            {
                                if (updated.SlsAmt > 0)
                                {
                                    updated.BonusID = bonusInput.BonusID;
                                    updated.KaType = mqy.Key;

                                    var updatedBonusKA = _db.OM_TBonusKA.FirstOrDefault(
                                        x => x.LevelNbr == updated.LevelNbr
                                            && x.BonusID == updated.BonusID
                                            && x.KaType == updated.KaType);
                                    if (updatedBonusKA != null)
                                    {
                                        updateBonusKA(ref updatedBonusKA, updated, false);
                                    }
                                    else
                                    {
                                        updatedBonusKA = new OM_TBonusKA();
                                        updateBonusKA(ref updatedBonusKA, updated, true);
                                        _db.OM_TBonusKA.AddObject(updatedBonusKA);
                                    }
                                }
                            }

                            // Bonus KA detail
                            lstMqyDetailChange.Updated.AddRange(lstMqyDetailChange.Created);
                            foreach (var updated in lstMqyDetailChange.Updated)
                            {
                                if (updated.AmtBegin > 0 || updated.AmtEnd > 0 || updated.AmtBonus > 0)
                                {
                                    updated.BonusID = bonusInput.BonusID;
                                    updated.KaType = mqy.Key;

                                    var updatedBonusKaDetail = _db.OM_TBonusKADetail.FirstOrDefault(
                                        x => x.LevelNbr == updated.LevelNbr
                                            && x.BonusID == updated.BonusID
                                            && x.KaType == updated.KaType
                                            && x.RecID == updated.RecID);
                                    if (updatedBonusKaDetail != null)
                                    {
                                        updateBonusKADetail(ref updatedBonusKaDetail, updated, false);
                                    }
                                    else {
                                        updateBonusKADetail(ref updatedBonusKaDetail, updated, true);
                                        _db.OM_TBonusKADetail.AddObject(updatedBonusKaDetail);
                                    }
                                }
                            }

                            foreach (var deleted in lstMqyDetailChange.Deleted)
                            {
                                if (deleted.AmtBegin > 0 || deleted.AmtEnd > 0 || deleted.AmtBonus > 0)
                                {
                                    deleted.BonusID = bonusInput.BonusID;
                                    deleted.KaType = mqy.Key;

                                    var deletedBonusKaDetail = _db.OM_TBonusKADetail.FirstOrDefault(
                                        x => x.LevelNbr == deleted.LevelNbr
                                            && x.BonusID == deleted.BonusID
                                            && x.KaType == deleted.KaType
                                            && x.RecID == deleted.RecID);
                                    if (deletedBonusKaDetail != null)
                                    {
                                        _db.OM_TBonusKADetail.DeleteObject(deletedBonusKaDetail);
                                    }
                                }
                            }

                            //foreach (var deleted in lstMqyChange.Deleted)
                            //{
                            //    if (deleted.SlsAmt > 0 || deleted.AmtBegin > 0 || deleted.AmtEnd > 0 || deleted.AmtBonus > 0)
                            //    {
                            //        deleted.BonusID = bonusInput.BonusID;
                            //        deleted.KaType = mqy.Key;

                            //        var deletedBonusKA = _db.OM_TBonusKA.FirstOrDefault(
                            //            x => x.LevelNbr == deleted.LevelNbr
                            //                && x.BonusID == deleted.BonusID
                            //                && x.KaType == deleted.KaType);
                            //        if (deletedBonusKA != null)
                            //        {
                            //            if (!lstMqyChange.Created.Exists(c => c.LevelNbr == deletedBonusKA.LevelNbr))
                            //            {
                            //                _db.OM_TBonusKA.DeleteObject(deletedBonusKA);
                            //            }
                            //        }
                            //    }
                            //}
                        }
                        #endregion
                    }

                    _db.SaveChanges();
                    return Json(new { success = true, msgCode = 201405071 });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "744");
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        public ActionResult DeleteBonus(string bonusID)
        {
            try
            {
                var bonus = _db.OM_TBonus.FirstOrDefault(p => p.BonusID == bonusID);
                if (bonus != null)
                {
                    _db.OM_TBonus.DeleteObject(bonus);

                    if (bonus.Channel == _rs)
                    {
                        var BonusRSs = _db.OM_TBonusRS.Where(c => c.BonusID == bonusID).ToList();
                        foreach (var BonusRS in BonusRSs)
                        {
                            _db.OM_TBonusRS.DeleteObject(BonusRS);
                        }

                        var Products = _db.OM_TBonusProduct.Where(c => c.BonusID == bonusID).ToList();
                        foreach (var Product in Products)
                        {
                            _db.OM_TBonusProduct.DeleteObject(Product);
                        }
                    }
                    else if (bonus.Channel == _ka)
                    {
                        var bonusKAs = _db.OM_TBonusKA.Where(c => c.BonusID == bonusID).ToList();
                        foreach (var bonusKA in bonusKAs)
                        {
                            _db.OM_TBonusKA.DeleteObject(bonusKA);
                        }

                        var bonusKaDetails = _db.OM_TBonusKADetail.Where(c => c.BonusID == bonusID).ToList();
                        foreach (var bonusKaDetail in bonusKaDetails)
                        {
                            _db.OM_TBonusKADetail.DeleteObject(bonusKaDetail);
                        }
                    }

                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("PosmID") });
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        public ActionResult DeleteBonusKA(FormCollection data)
        {
            try
            {
                var mqyHandler = new StoreDataHandler(data["lstChange"]);
                var lstMqyChange = mqyHandler.BatchObjectData<OM23400_pgBonusKA_Result>();

                foreach (var deleted in lstMqyChange.Deleted)
                {
                    var deletedBonusKAs = _db.OM_TBonusKA.Where(
                        x => x.BonusID == deleted.BonusID
                            && x.KaType == deleted.KaType).ToList();
                    foreach (var deletedBonusKA in deletedBonusKAs)
                    {
                        if (deletedBonusKA.LevelNbr >= deleted.LevelNbr)
                        {
                            _db.OM_TBonusKA.DeleteObject(deletedBonusKA);
                            var details = _db.OM_TBonusKADetail.Where(d => d.BonusID == deleted.BonusID
                                && d.KaType == deleted.KaType && d.LevelNbr == deletedBonusKA.LevelNbr).ToList();
                            foreach (var detail in details)
                            {
                                _db.OM_TBonusKADetail.DeleteObject(detail);
                            }
                        }
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        public ActionResult DeleteBonusRS(FormCollection data)
        {
            try
            {
                var bonusRSHandler = new StoreDataHandler(data["lstBonusRSChange"]);
                var lstBonusRSChange = bonusRSHandler.BatchObjectData<OM23400_pgBonusRS_Result>();

                foreach (var deleted in lstBonusRSChange.Deleted)
                {
                    var deletedBonusRSs = _db.OM_TBonusRS.Where(
                        x => x.BonusID == deleted.BonusID).ToList();
                    foreach (var deletedBonusRS in deletedBonusRSs)
                    {
                        if (deletedBonusRS.LevelNbr >= deleted.LevelNbr)
                        {
                            _db.OM_TBonusRS.DeleteObject(deletedBonusRS);
                        }
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        private void updateBonusKADetail(ref OM_TBonusKADetail updatedBonusKaDetail, OM23400_pgBonusKADetail_Result updated, bool isNew)
        {
            if (isNew)
            {
                updatedBonusKaDetail = new OM_TBonusKADetail();
                updatedBonusKaDetail.BonusID = updated.BonusID;
                updatedBonusKaDetail.LevelNbr = updated.LevelNbr;
                updatedBonusKaDetail.KaType = updated.KaType;

                updatedBonusKaDetail.Crtd_DateTime = DateTime.Now;
                updatedBonusKaDetail.Crtd_Prog = _screenNbr;
                updatedBonusKaDetail.Crtd_User = Current.UserName;
            }
            updatedBonusKaDetail.AmtBegin = updated.AmtBegin;
            updatedBonusKaDetail.AmtEnd = updated.AmtEnd;
            updatedBonusKaDetail.AmtBonus = updated.AmtBonus;

            updatedBonusKaDetail.LUpd_DateTime = DateTime.Now;
            updatedBonusKaDetail.LUpd_Prog = _screenNbr;
            updatedBonusKaDetail.LUpd_User = Current.UserName;
        }

        private void updateBonusKA(ref OM_TBonusKA updatedBonusKA, OM23400_pgBonusKA_Result updated, bool isNew)
        {
            if (isNew)
            {
                updatedBonusKA.BonusID = updated.BonusID;
                updatedBonusKA.LevelNbr = updated.LevelNbr;
                updatedBonusKA.KaType = updated.KaType;

                updatedBonusKA.Crtd_DateTime = DateTime.Now;
                updatedBonusKA.Crtd_Prog = _screenNbr;
                updatedBonusKA.Crtd_User = Current.UserName;
            }
            updatedBonusKA.SlsAmt = updated.SlsAmt;

            updatedBonusKA.LUpd_DateTime = DateTime.Now;
            updatedBonusKA.LUpd_Prog = _screenNbr;
            updatedBonusKA.LUpd_User = Current.UserName;
        }

        private void updateBonusRS(ref OM_TBonusRS createdCpny, OM23400_pgBonusRS_Result created, bool isNew)
        {
            if (isNew)
            {
                createdCpny.BonusID = created.BonusID;
                createdCpny.LevelNbr = created.LevelNbr;

                createdCpny.Crtd_DateTime = DateTime.Now;
                createdCpny.Crtd_Prog = _screenNbr;
                createdCpny.Crtd_User = Current.UserName;
            }

            createdCpny.AmtBegin = created.AmtBegin;
            createdCpny.AmtBonus = created.AmtBonus;
            createdCpny.AmtEnd = created.AmtEnd;

            createdCpny.LUpd_DateTime = DateTime.Now;
            createdCpny.LUpd_Prog = _screenNbr;
            createdCpny.LUpd_User = Current.UserName;
        }

        private void updateProduct(ref OM_TBonusProduct createdProduct, OM23400_pgProduct_Result created, bool isNew)
        {
            if (isNew)
            {
                createdProduct.BonusID = created.BonusID;
                createdProduct.ProductID = created.ProductID;
                createdProduct.ProductType = created.ProductType;

                createdProduct.Crtd_DateTime = DateTime.Now;
                createdProduct.Crtd_Prog = _screenNbr;
                createdProduct.Crtd_User = Current.UserName;
            }
            createdProduct.LUpd_DateTime = DateTime.Now;
            createdProduct.LUpd_Prog = _screenNbr;
            createdProduct.LUpd_User = Current.UserName;
        }

        private void updateBonus(ref OM_TBonus bonus, OM_TBonus bonusInput, bool isNew)
        {
            if (isNew)
            {
                bonus.BonusID = bonusInput.BonusID;
                bonus.Crtd_DateTime = DateTime.Now;
                bonus.Crtd_Prog = _screenNbr;
                bonus.Crtd_User = Current.UserName;

                bonus.RSApplyType = bonusInput.RSApplyType;
            }
            bonus.FromDate = bonusInput.FromDate;
            bonus.ToDate = bonusInput.ToDate;
            bonus.ApplyFor = bonusInput.ApplyFor;
            bonus.BonusFor = bonusInput.BonusFor;
            bonus.BonusName = bonusInput.BonusName;
            bonus.Channel = bonusInput.Channel;
            bonus.Zone = bonusInput.Zone;

            //bonus.AllProduct = bonusInput.AllProduct;

            bonus.LUpd_DateTime = DateTime.Now;
            bonus.LUpd_Prog = _screenNbr;
            bonus.LUpd_User = Current.UserName;
        }
    }
}

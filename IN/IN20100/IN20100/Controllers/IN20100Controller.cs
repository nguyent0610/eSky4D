﻿using HQ.eSkyFramework;
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

namespace IN20100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]

    public class IN20100Controller : Controller
    {
        private string _screenNbr = "IN20100";
        private string _userName = Current.UserName;
        IN20100Entities _db = Util.CreateObjectContext<IN20100Entities>(false);

        public ActionResult Index()
        {
            
            Util.InitRight(_screenNbr);
            var config = _db.IN20100_pdConfig(Current.UserName, Current.CpnyID, Current.LangID).FirstOrDefault();
            var isNvarchar = false;
            if (config != null)
            {
                isNvarchar = config ?? false;
            }
            ViewBag.isNvarchar = isNvarchar;

            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetUnitConversion()
        {           
            return this.Store(_db.IN20100_pgLoadUnitConversion().ToList());
        }

        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstUnitConversion"]);
                ChangeRecords<IN20100_pgLoadUnitConversion_Result> lstUnitConversion = dataHandler.BatchObjectData<IN20100_pgLoadUnitConversion_Result>();
                lstUnitConversion.Created.AddRange(lstUnitConversion.Updated);
                foreach (IN20100_pgLoadUnitConversion_Result del in lstUnitConversion.Deleted)
                {
                    var obj = lstUnitConversion.Created.FirstOrDefault(p => p.UnitType == del.UnitType && p.ClassID.ToLower() == del.ClassID.ToLower() && p.InvtID.ToLower() == del.InvtID.ToLower() && p.FromUnit.ToLower() == del.FromUnit.ToLower() && p.ToUnit.ToLower() == del.ToUnit.ToLower());
                    // neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    if (obj != null)
                    {
                        obj.tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.IN_UnitConversion.FirstOrDefault(p => p.UnitType == del.UnitType && p.ClassID.ToLower() == del.ClassID.ToLower() && p.InvtID.ToLower() == del.InvtID.ToLower() && p.FromUnit.ToLower() == del.FromUnit.ToLower() && p.ToUnit.ToLower() == del.ToUnit.ToLower());
                        if (objDel != null)
                        {
                            _db.IN_UnitConversion.DeleteObject(objDel);
                        }
                    }
                }
                //foreach (IN20100_pgLoadUnitConversion_Result deleted in lstUnitConversion.Deleted)
                //{
                //    var del = _db.IN_UnitConversion.Where(p => p.UnitType == deleted.UnitType && p.ClassID == deleted.ClassID && p.InvtID == deleted.InvtID && p.FromUnit == deleted.FromUnit && p.ToUnit == deleted.ToUnit).FirstOrDefault();
                //    if (del != null)
                //    {
                //        _db.IN_UnitConversion.DeleteObject(del);
                //    }
                //}

                //lstUnitConversion.Created.AddRange(lstUnitConversion.Updated);

                foreach (IN20100_pgLoadUnitConversion_Result curUnitConversion in lstUnitConversion.Created)
                {
                    if (curUnitConversion.UnitType.PassNull() == "" && curUnitConversion.ClassID.PassNull() == "" && curUnitConversion.InvtID.PassNull() == "" && curUnitConversion.FromUnit.PassNull()== "" && curUnitConversion.ToUnit.PassNull() == "") continue;

                    var unitConversion = _db.IN_UnitConversion.FirstOrDefault(p => p.UnitType.ToLower() == curUnitConversion.UnitType.ToLower() && p.ClassID.ToLower() == curUnitConversion.ClassID.ToLower() && p.InvtID.ToLower() == curUnitConversion.InvtID.ToLower() && p.FromUnit.ToLower() == curUnitConversion.FromUnit.ToLower() && p.ToUnit.ToLower() == curUnitConversion.ToUnit.ToLower());

                    if (unitConversion != null)
                    {
                        if (unitConversion.tstamp.ToHex() == curUnitConversion.tstamp.ToHex())
                        {
                            Update_IN_UnitConversion(unitConversion, curUnitConversion, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        unitConversion = new IN_UnitConversion();
                        Update_IN_UnitConversion(unitConversion, curUnitConversion, true);
                        _db.IN_UnitConversion.AddObject(unitConversion);
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

        private void Update_IN_UnitConversion(IN_UnitConversion t, IN20100_pgLoadUnitConversion_Result s, bool isNew)
        {
            if (isNew)
            {
                t.UnitType = s.UnitType;
                t.ClassID = s.ClassID;
                t.InvtID = s.InvtID;
                t.FromUnit = s.FromUnit;
                t.ToUnit = s.ToUnit;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.MultDiv = s.MultDiv;
            t.CnvFact = s.CnvFact;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

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
namespace OM26600.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM26600Controller : Controller
    {
        private string _screenNbr = "OM26600";
        private string _userName = Current.UserName;
        OM26600Entities _db = Util.CreateObjectContext<OM26600Entities>(false);

        public ActionResult Index()
        {
            bool Descr = false;
            bool TypeOfVehicle = false;
            bool WeightMax = false;
            bool ValueMax = false;
            bool SlsperID = false;

            var objConfig = _db.OM26600_pdConfig(Current.CpnyID,Current.UserName,Current.LangID).FirstOrDefault();
            if(objConfig!=null)
            {
                Descr = objConfig.Descr.HasValue ? objConfig.Descr.Value : false;
                TypeOfVehicle = objConfig.TypeOfVehicle.HasValue ? objConfig.TypeOfVehicle.Value : false;
                WeightMax = objConfig.WeightMax.HasValue ? objConfig.WeightMax.Value : false;
                ValueMax = objConfig.ValueMax.HasValue ? objConfig.ValueMax.Value : false;
                SlsperID = objConfig.SlsperID.HasValue ? objConfig.SlsperID.Value : false;
            }
            ViewBag.Descr = Descr;
            ViewBag.TypeOfVehicle = TypeOfVehicle;
            ViewBag.WeightMax = WeightMax;
            ViewBag.ValueMax = ValueMax;
            ViewBag.SlsperID = SlsperID;
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
            return this.Store(_db.OM26600_pgOM_Truck(Current.UserName, Current.CpnyID, Current.LangID).ToList());
        }
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstData"]);
                ChangeRecords<OM26600_pgOM_Truck_Result> lstData = dataHandler.BatchObjectData<OM26600_pgOM_Truck_Result>();

                lstData.Created.AddRange(lstData.Updated);
                foreach (OM26600_pgOM_Truck_Result del in lstData.Deleted)
                {

                    if (lstData.Created.Where(p => p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim() && p.Code.ToLower().Trim() == del.Code.ToLower().Trim()).Count() > 0)// neu danh sach them co chua danh sach xoa thi khong xoa thằng đó cập nhật lại tstamp của thằng đã xóa xem nhu trường hợp xóa thêm mới là trường hợp update
                    {
                        lstData.Created.Where(p => p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim() && p.Code.ToLower().Trim() == del.Code.ToLower().Trim()).FirstOrDefault().tstamp = del.tstamp;
                    }
                    else
                    {
                        var objDel = _db.OM_Truck.ToList().Where(p => p.BranchID.ToLower().Trim() == del.BranchID.ToLower().Trim() && p.Code.ToLower().Trim() == del.Code.ToLower().Trim()).FirstOrDefault();
                        if (objDel != null)
                        {
                            _db.OM_Truck.DeleteObject(objDel);
                        }
                    }
                }

                foreach (OM26600_pgOM_Truck_Result curItem in lstData.Created)
                {
                    if (curItem.BranchID.PassNull() == "" || curItem.Code.PassNull() == "")
                    {
                        continue;
                    }
                    var itemTruck = _db.OM_Truck.Where(p => p.BranchID.ToLower() == curItem.BranchID.ToLower() && p.Code.ToLower() ==  curItem.Code.ToLower()).FirstOrDefault();

                    if (itemTruck != null)
                    {
                        if (itemTruck.tstamp.ToHex() == curItem.tstamp.ToHex())
                        {
                            Update_OM_Truck(itemTruck, curItem, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        itemTruck = new OM_Truck();
                        Update_OM_Truck(itemTruck, curItem, true);
                        _db.OM_Truck.AddObject(itemTruck);
                    }
                }

                _db.SaveChanges();

                return Util.CreateMessage(MessageProcess.Save);
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, "text/html");
            }
        }

        private void Update_OM_Truck(OM_Truck t, OM26600_pgOM_Truck_Result s, bool isNew)
        {
            if (isNew)
            {
                t.BranchID = s.BranchID;
                t.Code = s.Code;
                t.TypeOfVehicle = s.TypeOfVehicle;
                t.Crtd_User = Current.UserName;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.ValueMax = s.ValueMax;
            t.WeightMax = s.WeightMax;
            t.SlsperID = s.SlsperID;
            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

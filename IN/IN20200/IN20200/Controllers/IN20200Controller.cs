using eBiz4DWebFrame;
using Ext.Net;
using Ext.Net.MVC;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
namespace IN20200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class IN20200Controller : Controller
    {
        string screenNbr = "IN20200";
        IN20200Entities _db = Util.CreateObjectContext<IN20200Entities>(false);
        

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "none")]
        public PartialViewResult Body()
        {
            return PartialView();
        }

        public ActionResult GetProductClass(String classID)
        {
            var rptCtrl = _db.IN_ProductClass.FirstOrDefault(p => p.ClassID == classID);
            return this.Store(rptCtrl);
        }
        public ActionResult GetCompany(String classID)
        {
            var lst = _db.ppv_IN20200_getCompany(classID).ToList();
            return this.Store(lst);
        }

        [DirectMethod]
        [HttpPost]
        public ActionResult Save(FormCollection data, string classID)
        {
            StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstheader"]);
            ChangeRecords<IN_ProductClass> lstheader = dataHandler2.BatchObjectData<IN_ProductClass>();
            StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstgrd"]);
            ChangeRecords<ppv_IN20200_getCompany_Result> lstgrd = dataHandler1.BatchObjectData<ppv_IN20200_getCompany_Result>();

            foreach (ppv_IN20200_getCompany_Result deleted in lstgrd.Deleted)
            {
                var del = _db.IN_ProdClassCpny.Where(p => p.ClassID == classID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                if (del != null)
                {
                    _db.IN_ProdClassCpny.DeleteObject(del);

                }

            }
            foreach (ppv_IN20200_getCompany_Result created in lstgrd.Created)
            {

                var record = _db.IN_ProdClassCpny.Where(p => p.ClassID == classID && p.CpnyID == created.CpnyID).FirstOrDefault();

               
                    if (record == null)
                    {
                        record = new IN_ProdClassCpny();


                        record.ClassID = classID;
                        record.CpnyID = created.CpnyID;


                        if (record.ClassID != "" && record.CpnyID != "")
                        {
                           
                           _db.IN_ProdClassCpny.AddObject(record);
                        }

                    
                    
                }
            }



            foreach (ppv_IN20200_getCompany_Result updated in lstgrd.Updated)
            {

                var record = _db.IN_ProdClassCpny.Where(p => p.ClassID == classID && p.CpnyID == updated.CpnyID).FirstOrDefault();


                if (record != null)
                {

                    
                }
                else
                {
                    
                        record = new IN_ProdClassCpny();


                        record.ClassID = classID;
                        record.CpnyID = updated.CpnyID;


                        _db.IN_ProdClassCpny.AddObject(record);

                    

                       
                    
                }

            }

            foreach (IN_ProductClass updated in lstheader.Updated)
            {
                // Get the image path


                var objHeader = _db.IN_ProductClass.Where(p => p.ClassID == updated.ClassID).FirstOrDefault();
                if (objHeader != null)
                {
                    //updating
                    if (updated.Public == true)
                    {
                        var del = _db.IN_ProdClassCpny.Where(p => p.ClassID == classID).ToList();
                        for (int i = 0; i < del.Count; i++)
                        {

                            _db.IN_ProdClassCpny.DeleteObject(del[i]);

                        }
                        //foreach (IN_ProdClassCpny proclass in del)
                        //{
                        //    _db.IN_ProdClassCpny.DeleteObject(proclass);
                        //}
                    }
                    
                    
                    

                        objHeader.Public = updated.Public;
                        objHeader.Descr = updated.Descr; // ???

                        objHeader.DfltInvtType = updated.DfltInvtType;
                        objHeader.DfltStkItem = updated.DfltStkItem;
                        objHeader.DfltSource = updated.DfltSource;
                        objHeader.DfltValMthd = updated.DfltValMthd;
                        objHeader.DfltLotSerTrack = updated.DfltLotSerTrack;
                        objHeader.Buyer = updated.Buyer;
                        objHeader.DfltStkUnit = updated.DfltStkUnit;
                        objHeader.DfltPOUnit = updated.DfltPOUnit;
                        objHeader.DfltSOUnit = updated.DfltSOUnit;
                        objHeader.MaterialType = updated.MaterialType;
                        objHeader.DfltSite = updated.DfltSite;
                        objHeader.DfltSlsTaxCat = updated.DfltSlsTaxCat;
                        if (updated.DfltLotSerTrack == "N")
                        {
                            objHeader.DfltLotSerAssign = null;
                            objHeader.DfltLotSerMthd = null;
                            objHeader.DfltLotSerShelfLife = 0;
                            objHeader.DfltWarrantyDays = 0;
                            objHeader.DfltLotSerFxdTyp = null;
                            objHeader.DfltLotSerFxdLen = 0;
                            objHeader.DfltLotSerFxdVal = "";
                            objHeader.DfltLotSerNumLen = 0;
                            objHeader.DfltLotSerNumVal = "";
                        }
                        else
                        {

                            objHeader.DfltLotSerAssign = updated.DfltLotSerAssign;
                            objHeader.DfltLotSerMthd = updated.DfltLotSerMthd;
                            objHeader.DfltLotSerShelfLife = updated.DfltLotSerShelfLife;
                            objHeader.DfltWarrantyDays = updated.DfltWarrantyDays;
                            objHeader.DfltLotSerFxdTyp = updated.DfltLotSerFxdTyp;
                            objHeader.DfltLotSerFxdLen = updated.DfltLotSerFxdLen;
                            objHeader.DfltLotSerFxdVal = updated.DfltLotSerFxdVal;
                            objHeader.DfltLotSerNumLen = updated.DfltLotSerNumLen;
                            objHeader.DfltLotSerNumVal = updated.DfltLotSerNumVal;
                        }
                        
                        
                        UpdatingHeader(updated, ref objHeader);
                        
                        _db.SaveChanges();
                    
                }
                else
                {
                    objHeader = new IN_ProductClass();

                    objHeader.ClassID = classID;
                    objHeader.Public = updated.Public;
                    objHeader.Descr = updated.Descr; // ???

                    objHeader.DfltInvtType = updated.DfltInvtType;
                    objHeader.DfltStkItem = updated.DfltStkItem;
                    objHeader.DfltSource = updated.DfltSource;
                    objHeader.DfltValMthd = updated.DfltValMthd;
                    objHeader.DfltLotSerTrack = updated.DfltLotSerTrack;
                    objHeader.Buyer = updated.Buyer;
                    objHeader.DfltStkUnit = updated.DfltStkUnit;
                    objHeader.DfltPOUnit = updated.DfltPOUnit;
                    objHeader.DfltSOUnit = updated.DfltSOUnit;
                    objHeader.MaterialType = updated.MaterialType;
                    objHeader.DfltSite = updated.DfltSite;
                    objHeader.DfltSlsTaxCat = updated.DfltSlsTaxCat;


                    objHeader.DfltLotSerAssign = updated.DfltLotSerAssign;
                    objHeader.DfltLotSerMthd = updated.DfltLotSerMthd;
                    objHeader.DfltLotSerShelfLife = updated.DfltLotSerShelfLife;
                    objHeader.DfltWarrantyDays = updated.DfltWarrantyDays;
                    objHeader.DfltLotSerFxdLen = updated.DfltLotSerFxdLen;
                    objHeader.DfltLotSerFxdVal = updated.DfltLotSerFxdVal;
                    objHeader.DfltLotSerNumLen = updated.DfltLotSerNumLen;
                    objHeader.DfltLotSerNumVal = updated.DfltLotSerNumVal;

                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    objHeader.tstamp = new byte[0];
                    UpdatingHeader(updated, ref objHeader);
                    _db.IN_ProductClass.AddObject(objHeader);
                    _db.SaveChanges();



                }






                // If there is a change in handling status (keepStatus is False),
                // add a new pending task with an approved handle.



                // ===============================================================

                // Get out of the loop (only update the first data)

            }
            foreach (IN_ProductClass created in lstheader.Created)
            {
                var objHeader = _db.IN_ProductClass.Where(p => p.ClassID == classID).FirstOrDefault();
                if (objHeader == null)
                {
                    objHeader = new IN_ProductClass();

                    objHeader.ClassID = classID;
                    objHeader.Public = created.Public;
                    objHeader.Descr = created.Descr; // ???

                    objHeader.DfltInvtType = created.DfltInvtType;
                    objHeader.DfltStkItem = created.DfltStkItem;
                    objHeader.DfltSource = created.DfltSource;
                    objHeader.DfltValMthd = created.DfltValMthd;
                    objHeader.DfltLotSerTrack = created.DfltLotSerTrack;
                    objHeader.Buyer = created.Buyer;
                    objHeader.DfltStkUnit = created.DfltStkUnit;
                    objHeader.DfltPOUnit = created.DfltPOUnit;
                    objHeader.DfltSOUnit = created.DfltSOUnit;
                    objHeader.MaterialType = created.MaterialType;
                    objHeader.DfltSite = created.DfltSite;
                    objHeader.DfltSlsTaxCat = created.DfltSlsTaxCat;


                    objHeader.DfltLotSerAssign = created.DfltLotSerAssign;
                    objHeader.DfltLotSerMthd = created.DfltLotSerMthd;
                    objHeader.DfltLotSerShelfLife = created.DfltLotSerShelfLife;
                    objHeader.DfltWarrantyDays = created.DfltWarrantyDays;
                    objHeader.DfltLotSerFxdLen = created.DfltLotSerFxdLen;
                    objHeader.DfltLotSerFxdVal = created.DfltLotSerFxdVal;
                    objHeader.DfltLotSerNumLen = created.DfltLotSerNumLen;
                    objHeader.DfltLotSerNumVal = created.DfltLotSerNumVal;
                    
                  

                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = screenNbr;
                    objHeader.Crtd_User = Current.UserName;
                    objHeader.tstamp = new byte[0];
                    UpdatingHeader(created, ref objHeader);
                    _db.IN_ProductClass.AddObject(objHeader);
                    _db.SaveChanges();

                }
            }


            _db.SaveChanges();
            //this.Direct();


            return Json(new { success = true, value = classID }, JsonRequestBehavior.AllowGet);


        }
        [DirectMethod]
        public ActionResult IN20200Delete(string classID)
        {
            var cpny = _db.IN_ProductClass.FirstOrDefault(p => p.ClassID == classID);


            _db.IN_ProductClass.DeleteObject(cpny);



            _db.SaveChanges();
            return this.Direct();
        }

        //private void UpdatingDetail(SYS_ReportParm s, ref SYS_ReportParm d)
        //{
        //    d.ReportFormat = s.ReportFormat;
        //    d.StringCap00 = s.StringCap00;
        //    d.StringCap01 = s.StringCap01;
        //    d.StringCap02 = s.StringCap02;
        //    d.StringCap03 = s.StringCap03;
        //    d.DateCap00 = s.DateCap00;
        //    d.DateCap01 = s.DateCap01;
        //    d.DateCap02 = s.DateCap02;
        //    d.DateCap03 = s.DateCap03;
        //    d.BooleanCap00 = s.BooleanCap00;
        //    d.BooleanCap01 = s.BooleanCap01;
        //    d.BooleanCap02 = s.BooleanCap02;
        //    d.BooleanCap03 = s.BooleanCap03;
        //    d.PPV_Proc00 = s.PPV_Proc00;
        //    d.PPV_Proc01 = s.PPV_Proc01;
        //    d.PPV_Proc02 = s.PPV_Proc02;
        //    d.PPV_Proc03 = s.PPV_Proc03;
        //    d.ListCap00 = s.ListCap00;
        //    d.ListCap01 = s.ListCap01;
        //    d.ListCap02 = s.ListCap02;
        //    d.ListCap03 = s.ListCap03;
        //    d.ListProc00 = s.ListProc00;
        //    d.ListProc01 = s.ListProc01;
        //    d.ListProc02 = s.ListProc02;
        //    d.ListProc03 = s.ListProc03;


        //    d.LUpd_DateTime = DateTime.Now;
        //    d.LUpd_Prog = screenNbr;
        //    d.LUpd_User = Current.UserName;
        //}

        private void UpdatingHeader(IN_ProductClass s, ref IN_ProductClass d)
        {

            d.Public = s.Public;
            d.Descr = s.Descr; // ???

            d.DfltInvtType = s.DfltInvtType;
            d.DfltStkItem = s.DfltStkItem;
            d.DfltSource = s.DfltSource;
            d.DfltValMthd = s.DfltValMthd;
            d.DfltLotSerTrack = s.DfltLotSerTrack;
            d.Buyer = s.Buyer;
            d.DfltStkUnit = s.DfltStkUnit;
            d.DfltPOUnit = s.DfltPOUnit;
            d.DfltSOUnit = s.DfltSOUnit;
            d.MaterialType = s.MaterialType;
            d.DfltSite = s.DfltSite;
            d.DfltSlsTaxCat = s.DfltSlsTaxCat;


            d.DfltLotSerAssign = s.DfltLotSerAssign;
            d.DfltLotSerMthd = s.DfltLotSerMthd;
            d.DfltLotSerShelfLife = s.DfltLotSerShelfLife;
            d.DfltWarrantyDays = s.DfltWarrantyDays;
            d.DfltLotSerFxdTyp = s.DfltLotSerFxdTyp;
            d.DfltLotSerFxdLen = s.DfltLotSerFxdLen;
            d.DfltLotSerFxdVal = s.DfltLotSerFxdVal;
            d.DfltLotSerNumLen = s.DfltLotSerNumLen;
            d.DfltLotSerNumVal = s.DfltLotSerNumVal;


            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = screenNbr;
            d.LUpd_User = Current.UserName;
        }
    }
}

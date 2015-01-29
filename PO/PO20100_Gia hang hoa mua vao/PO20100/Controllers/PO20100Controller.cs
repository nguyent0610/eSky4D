using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using eBiz4DWebFrame;
using eBiz4DWebSys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
namespace PO20100.Controllers
{
    [DirectController]
    public class PO20100Controller : Controller
    {



        PO20100Entities db = Util.CreateObjectContext<PO20100Entities>(false);
        
        public ActionResult Index()
        {

          
            return View(db.PO_PriceHeader);
           
        }

        public ActionResult GetPOPriceHeader(string PriceID)
        {

            return this.Store(db.PO_PriceHeader.Where(p=>p.PriceID==PriceID));
           
        }
        public ActionResult GetPOPrice(string PriceID)
        {

            return this.Store(db.PO20100GetPOPrice(PriceID));

        }
        public ActionResult GetPOPriceCpny(string PriceID)
        {

            return this.Store(db.PO20100GetPOPriceCpny(PriceID));

        }
        [HttpPost]
        [DirectMethod]
        public ActionResult Save(FormCollection data )     
        {
            string PriceID=data["cboPriceID"];
            StoreDataHandler dataHandler = new StoreDataHandler(data["lstPOPriceHeader"]);
            ChangeRecords<PO_PriceHeader> lstPOPriceHeader = dataHandler.BatchObjectData<PO_PriceHeader>();
          
            StoreDataHandler dataHandler1 = new StoreDataHandler(data["lstPOPrice"]);
            ChangeRecords<PO20100GetPOPrice_Result> lstPOPrice = dataHandler1.BatchObjectData<PO20100GetPOPrice_Result>();

            StoreDataHandler dataHandler2 = new StoreDataHandler(data["lstPOPriceCpny"]);
            ChangeRecords<PO20100GetPOPriceCpny_Result> lstPOPriceCpny = dataHandler2.BatchObjectData<PO20100GetPOPriceCpny_Result>();

            foreach (PO_PriceHeader created in lstPOPriceHeader.Created)
            {
                var objHeader = db.PO_PriceHeader.FirstOrDefault(p => p.PriceID == PriceID);            
                if (objHeader == null)
                {
                    objHeader = new PO_PriceHeader();
                    objHeader.PriceID = PriceID;
                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = "PO20100";
                    objHeader.Crtd_User = "ADMIN";
                    UpdatingHeader(created, ref objHeader);
                    db.PO_PriceHeader.AddObject(objHeader);


                }
                else
                {
                    UpdatingHeader(created, ref objHeader);
                }
                break;
            }
            foreach (PO_PriceHeader created in lstPOPriceHeader.Updated)
            {
                var objHeader = db.PO_PriceHeader.FirstOrDefault(p => p.PriceID == PriceID);
                if (objHeader == null)
                {
                    objHeader = new PO_PriceHeader();
                    objHeader.PriceID = PriceID;
                    objHeader.Crtd_DateTime = DateTime.Now;
                    objHeader.Crtd_Prog = "PO20100";
                    objHeader.Crtd_User = "ADMIN";
                    UpdatingHeader(created, ref objHeader);
                    db.PO_PriceHeader.AddObject(objHeader);
                

                }
                else
                {
                    UpdatingHeader(created, ref objHeader);
                }
                break;
            }

            #region SavePO_Price
            foreach (PO20100GetPOPrice_Result deleted in lstPOPrice.Deleted)
            {
                var objDelete = db.PO_Price.Where(p => p.PriceID == deleted.PriceID && p.InvtID == deleted.InvtID).FirstOrDefault();
                if (objDelete != null)
                {
                    db.PO_Price.DeleteObject(objDelete);
                }
            }
            foreach (PO20100GetPOPrice_Result created in lstPOPrice.Created)
            {
                if (created.InvtID.PassNull().Trim() == "") continue;
                var objPrice = db.PO_Price.FirstOrDefault(p => p.PriceID == created.PriceID && p.InvtID==created.InvtID);
                if (objPrice == null)
                {
                    objPrice = new PO_Price();
                    objPrice.PriceID = PriceID;
                    objPrice.InvtID = created.InvtID;                 
                    objPrice.Crtd_DateTime = DateTime.Now;
                    objPrice.Crtd_Prog = "PO20100";
                    objPrice.Crtd_User = "ADMIN";
                    UpdatingPOPrice(created, ref objPrice);
                    db.PO_Price.AddObject(objPrice);


                }
                else
                {
                    UpdatingPOPrice(created, ref objPrice);
                }             
            }

           

            foreach (PO20100GetPOPrice_Result Updated in lstPOPrice.Updated)
            {
                var objPrice = db.PO_Price.FirstOrDefault(p => p.PriceID == Updated.PriceID && p.InvtID == Updated.InvtID);
                if (objPrice == null)
                {
                    objPrice = new PO_Price();
                    objPrice.PriceID = PriceID;
                    objPrice.InvtID = Updated.InvtID;     
                    objPrice.Crtd_DateTime = DateTime.Now;
                    objPrice.Crtd_Prog = "PO20100";
                    objPrice.Crtd_User = "ADMIN";
                    UpdatingPOPrice(Updated, ref objPrice);
                    db.PO_Price.AddObject(objPrice);


                }
                else
                {
                    UpdatingPOPrice(Updated, ref objPrice);
                }
            }

            #endregion
            #region PO_PriceCpny

            foreach (PO20100GetPOPriceCpny_Result deleted in lstPOPriceCpny.Deleted)
            {
                var objDelete = db.PO_PriceCpny.Where(p => p.PriceID == deleted.PriceID && p.CpnyID == deleted.CpnyID).FirstOrDefault();
                if (objDelete != null)
                {
                    db.PO_PriceCpny.DeleteObject(objDelete);
                }
            }
            foreach (PO20100GetPOPriceCpny_Result created in lstPOPriceCpny.Created)
            {
                if (created.CpnyID.PassNull().Trim() == "") continue;
                var objPrice = db.PO_PriceCpny.FirstOrDefault(p => p.PriceID == created.PriceID && p.CpnyID == created.CpnyID);
                if (objPrice == null)
                {
                    objPrice = new PO_PriceCpny();
                    objPrice.PriceID = PriceID;
                    objPrice.CpnyID = created.CpnyID;                                   
                    db.PO_PriceCpny.AddObject(objPrice);


                }
               
            }


            foreach (PO20100GetPOPriceCpny_Result Updated in lstPOPriceCpny.Updated)
            {
                var objPrice = db.PO_PriceCpny.FirstOrDefault(p => p.PriceID == Updated.PriceID && p.CpnyID == Updated.CpnyID);
                if (objPrice == null)
                {
                    objPrice = new PO_PriceCpny();
                    objPrice.PriceID = PriceID;
                    objPrice.CpnyID = Updated.CpnyID;
                    db.PO_PriceCpny.AddObject(objPrice);


                }

            }
            #endregion
            db.SaveChanges();
            return this.Direct();
            
        }
        [DirectMethod]
        public ActionResult Delete(string PriceID)
        {
            var cpny = db.PO_PriceHeader.FirstOrDefault(p => p.PriceID == PriceID);
            if (cpny != null)
            {
                db.PO_PriceHeader.DeleteObject(cpny);
            }

            var lstAddr = db.PO_Price.Where(p => p.PriceID == PriceID).ToList();
            foreach (var item in lstAddr)
            {
                db.PO_Price.DeleteObject(item);
            }

            var lstSub = db.PO_PriceCpny.Where(p => p.PriceID == PriceID).ToList();
            foreach (var item in lstSub)
            {
                db.PO_PriceCpny.DeleteObject(item);
            }
           
            db.SaveChanges();
            return this.Direct();
        }
        private void UpdatingHeader(PO_PriceHeader s,ref PO_PriceHeader d)
        {
            d.Descr = s.Descr;
            d.Disc = s.Disc;
            d.EffDate = s.EffDate;
            d.HOCreate = s.HOCreate;
            d.Public = s.Public;
            d.Status = s.Status;
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = "PO20100";
            d.LUpd_User = Current.UserName;
          
        }

        private void UpdatingPOPrice(PO20100GetPOPrice_Result s, ref PO_Price d)
        {
            d.VendID ="*";
            d.InvtID = s.InvtID;
            d.UOM = s.UOM;
            d.Price = s.Price;               
            d.Descr = s.Descr;
            d.QtyBreak = s.QtyBreak;
            d.Disc = s.Disc;
            d.LUpd_DateTime = DateTime.Now;
            d.LUpd_Prog = "PO20100";
            d.LUpd_User = Current.UserName;
        }
       
        public ActionResult Refresh()
        {
            X.GetCmp<Store>("storePO_Setup").Reload();  
            return this.Direct();
            
        }
        [DirectMethod]
        public ActionResult AskClose()
        {
            Message.Show(5, null, new JFunction() { Fn = "askClose" });
            return this.Direct();
        }

       
             
    }
    public static class Util1
    {
        public static Model GenerateModel(this Type type, string modelName, string key, string boolField = "@@")
        {
            Model model = new Model();

            var pros = type.GetProperties();
            foreach (var pro in pros)
            {
               
                if (boolField.Split(',').Contains(pro.Name))
                    model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Boolean));
                else if (pro.PropertyType.Name == "Nullable`1")
                {
                   
                    if (Nullable.GetUnderlyingType(pro.PropertyType).Name == "Double")
                        model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Float));
                    else if (Nullable.GetUnderlyingType(pro.PropertyType).Name == "Boolean")
                        model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Boolean));
                    else if (Nullable.GetUnderlyingType(pro.PropertyType).Name == "String")
                        model.Fields.Add(new ModelField(pro.Name, ModelFieldType.String));
                    else if (Nullable.GetUnderlyingType(pro.PropertyType).Name == "DateTime")
                        model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Date));
                    else
                        model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Auto));
                }
                else if (pro.PropertyType.Name == "Double")
                    model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Float));
                else if (pro.PropertyType.Name == "Boolean")
                    model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Boolean));
                else if (pro.PropertyType.Name == "String")
                    model.Fields.Add(new ModelField(pro.Name, ModelFieldType.String));
                else if (pro.PropertyType.Name == "DateTime")
                    model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Date));
                else
                    model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Auto));

            }
            model.IDProperty = key;
            model.ID = modelName;
            return model;
        }
    }
}
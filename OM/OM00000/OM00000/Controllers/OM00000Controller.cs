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
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
namespace OM00000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM00000Controller : Controller
    {
        private string _screenNbr = "OM00000";
        private string _om = "OM";
        OM00000Entities _db;
        //eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);

        public OM00000Controller()
        { 
            _db = Util.CreateObjectContext<OM00000Entities>(false);
        }

        //
        // GET: /OM00000/
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

        public ActionResult GetOMSetup()
        {
            var setup = _db.OM_Setup.FirstOrDefault(p => p.SetupID == _om);
            return this.Store(setup);
        }

        public ActionResult SaveSetup(FormCollection data, bool isNew)
        {
            try
            {
                var setupHandler = new StoreDataHandler(data["lstSetup"]);
                var inputSetup = setupHandler.ObjectData<OM_Setup>().FirstOrDefault();
                if (inputSetup != null)
                {
                    var setup = _db.OM_Setup.FirstOrDefault(s => s.SetupID == inputSetup.SetupID);
                    if (setup != null)
                    {
                        if (!isNew)
                        {
                            // update
                            if (setup.tstamp.ToHex() == inputSetup.tstamp.ToHex())
                            {
                                updateSetUp(ref setup, inputSetup, false);
                                _db.SaveChanges();

                                return Json(new { success = true, msgCode = 201405071 });
                            }
                            else
                            {
                                throw new MessageException(MessageType.Message, "19");
                            }
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "2000", "", new string[]{
                                    Util.GetLang("SetupID")
                                });
                        }
                    }
                    else 
                    {
                        if (isNew)
                        {
                            updateSetUp(ref setup, inputSetup, true);
                            _db.OM_Setup.AddObject(setup);
                            _db.SaveChanges();

                            return Json(new { success = true, msgCode = 201405071 });
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "1555");
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

        private void updateSetUp(ref OM_Setup setup, OM_Setup inputSetup, bool isNew)
        {
            if (isNew)
            {
                setup = new OM_Setup();
                setup.Crtd_DateTime = DateTime.Now;
                setup.Crtd_Prog = _screenNbr;
                setup.Crtd_User = Current.UserName;
            }
            setup.ShowKPI = inputSetup.ShowKPI;
            setup.ShowCustClass = inputSetup.ShowCustClass;
            setup.AutoReleaseAR = inputSetup.AutoReleaseAR;
            setup.AutoReleaseIN = inputSetup.AutoReleaseIN;
            setup.UseBarCode = inputSetup.UseBarCode;
            setup.EditableSlsPrice = inputSetup.EditableSlsPrice;
            setup.AutoInvcNbr = inputSetup.AutoInvcNbr;
            setup.POSPrinter = inputSetup.POSPrinter;
            setup.DfltOrderType = inputSetup.DfltOrderType;
            setup.PrefixBat = inputSetup.PrefixBat;
            setup.DfltSalesPrice = inputSetup.DfltSalesPrice;
            setup.CreditChkRule = inputSetup.CreditChkRule;

            setup.ProrateDisc = inputSetup.ProrateDisc;
            setup.SimpleDiscounts = inputSetup.SimpleDiscounts;

            setup.DetDiscG1App = inputSetup.DetDiscG1App;
            setup.DetDiscG2App = inputSetup.DetDiscG2App;
            setup.DetDiscG3App = inputSetup.DetDiscG3App;
            setup.DetDiscG4App = inputSetup.DetDiscG4App;
            setup.DetDiscG5App = inputSetup.DetDiscG5App;
            setup.DetDiscG6App = inputSetup.DetDiscG6App;


            setup.GroupDiscG1App = inputSetup.GroupDiscG1App;
            setup.GroupDiscG2App = inputSetup.GroupDiscG2App;
            setup.GroupDiscG3App = inputSetup.GroupDiscG3App;
            setup.GroupDiscG4App = inputSetup.GroupDiscG4App;
            setup.GroupDiscG5App = inputSetup.GroupDiscG5App;
            setup.GroupDiscG6App = inputSetup.GroupDiscG6App;
            setup.GroupDiscG7App = inputSetup.GroupDiscG7App;
            setup.GroupDiscG8App = inputSetup.GroupDiscG8App;
            setup.GroupDiscG9App = inputSetup.GroupDiscG9App;
            setup.GroupDiscG10App = inputSetup.GroupDiscG10App;
            setup.GroupDiscG11App = inputSetup.GroupDiscG11App;
            setup.GroupDiscG12App = inputSetup.GroupDiscG12App;

            setup.DocDiscG1App = inputSetup.DocDiscG1App;
            setup.DocDiscG2App = inputSetup.DocDiscG2App;
            setup.DocDiscG3App = inputSetup.DocDiscG3App;
            setup.DocDiscG4App = inputSetup.DocDiscG4App;
            setup.DocDiscG5App = inputSetup.DocDiscG5App;
            setup.DocDiscG6App = inputSetup.DocDiscG6App;


            setup.DetDiscG1C1 = inputSetup.DetDiscG1C1;
            setup.DetDiscG2C1 = inputSetup.DetDiscG2C1;
            setup.DetDiscG3C1 = inputSetup.DetDiscG3C1;
            setup.DetDiscG4C1 = inputSetup.DetDiscG4C1;
            setup.DetDiscG5C1 = inputSetup.DetDiscG5C1;
            setup.DetDiscG6C1 = inputSetup.DetDiscG6C1;

            setup.DetDiscG1C2 = inputSetup.DetDiscG1C2;
            setup.DetDiscG2C2 = inputSetup.DetDiscG2C2;
            setup.DetDiscG3C2 = inputSetup.DetDiscG3C2;
            setup.DetDiscG4C2 = inputSetup.DetDiscG4C2;
            setup.DetDiscG5C2 = inputSetup.DetDiscG5C2;
            setup.DetDiscG6C2 = inputSetup.DetDiscG6C2;

            setup.GroupDiscG1C1 = inputSetup.GroupDiscG1C1;
            setup.GroupDiscG2C1 = inputSetup.GroupDiscG2C1;
            setup.GroupDiscG3C1 = inputSetup.GroupDiscG3C1;
            setup.GroupDiscG4C1 = inputSetup.GroupDiscG4C1;
            setup.GroupDiscG5C1 = inputSetup.GroupDiscG5C1;
            setup.GroupDiscG6C1 = inputSetup.GroupDiscG6C1;
            setup.GroupDiscG7C1 = inputSetup.GroupDiscG7C1;
            setup.GroupDiscG8C1 = inputSetup.GroupDiscG8C1;
            setup.GroupDiscG9C1 = inputSetup.GroupDiscG9C1;
            setup.GroupDiscG10C1 = inputSetup.GroupDiscG10C1;
            setup.GroupDiscG11C1 = inputSetup.GroupDiscG11C1;
            setup.GroupDiscG12C1 = inputSetup.GroupDiscG12C1;

            setup.GroupDiscG1C2 = inputSetup.GroupDiscG1C2;
            setup.GroupDiscG2C2 = inputSetup.GroupDiscG2C2;
            setup.GroupDiscG3C2 = inputSetup.GroupDiscG3C2;
            setup.GroupDiscG4C2 = inputSetup.GroupDiscG4C2;
            setup.GroupDiscG5C2 = inputSetup.GroupDiscG5C2;
            setup.GroupDiscG6C2 = inputSetup.GroupDiscG6C2;
            setup.GroupDiscG7C2 = inputSetup.GroupDiscG7C2;
            setup.GroupDiscG8C2 = inputSetup.GroupDiscG8C2;
            setup.GroupDiscG9C2 = inputSetup.GroupDiscG9C2;
            setup.GroupDiscG10C2 = inputSetup.GroupDiscG10C2;
            setup.GroupDiscG11C2 = inputSetup.GroupDiscG11C2;
            setup.GroupDiscG12C2 = inputSetup.GroupDiscG12C2;

            setup.DocDiscG1C1 = inputSetup.DocDiscG1C1;
            setup.DocDiscG2C1 = inputSetup.DocDiscG2C1;
            setup.DocDiscG3C1 = inputSetup.DocDiscG3C1;
            setup.DocDiscG4C1 = inputSetup.DocDiscG4C1;
            setup.DocDiscG5C1 = inputSetup.DocDiscG5C1;
            setup.DocDiscG6C1 = inputSetup.DocDiscG6C1;

            setup.DocDiscG1C2 = inputSetup.DocDiscG1C2;
            setup.DocDiscG2C2 = inputSetup.DocDiscG2C2;
            setup.DocDiscG3C2 = inputSetup.DocDiscG3C2;
            setup.DocDiscG4C2 = inputSetup.DocDiscG4C2;
            setup.DocDiscG5C2 = inputSetup.DocDiscG5C2;
            setup.DocDiscG6C2 = inputSetup.DocDiscG6C2;

            setup.PriceSeq00 = inputSetup.PriceSeq00;
            setup.PriceSeq01 = inputSetup.PriceSeq00;
            setup.PriceSeq02 = inputSetup.PriceSeq00;
            setup.PriceSeq03 = inputSetup.PriceSeq00;
            setup.PriceSeq04 = inputSetup.PriceSeq00;
            setup.PriceSeq05 = inputSetup.PriceSeq00;

            setup.ReqDiscID = inputSetup.ReqDiscID;
            setup.BGWarningMsg = inputSetup.BGWarningMsg;
            setup.VN32Promotion = 1;

            setup.UseDiscTerm = inputSetup.UseDiscTerm;
            setup.InlcSOFeeDisc = inputSetup.InlcSOFeeDisc;
            setup.InlcSOFeeProm = inputSetup.InlcSOFeeProm;
            setup.ReqDiscID = inputSetup.ReqDiscID;

            setup.LUpd_DateTime = DateTime.Now;
            setup.LUpd_Prog = _screenNbr;
            setup.LUpd_User = Current.UserName;
        }
    }
}

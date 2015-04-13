using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Xml;
using System.Xml.Linq;
using System.Data.Objects;
using Aspose.Cells;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.Data;
using System.Drawing;
using HQFramework.DAL;
using System.Dynamic;
using HQFramework.Common;
namespace OM10100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM10100Controller : Controller
    {
        private string _screenNbr = "OM10100";
        private string _userName = Current.UserName;
        private OM10100Entities _app = Util.CreateObjectContext<OM10100Entities>();
        private eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);     
        private FormCollection _form;
        private JsonResult _logMessage;
        private List<OM10100_pgOrderDet_Result> _lstOrdDet;
        private List<OM10100_pgOrderDet_Result> _lstOldOrdDet;
        private List<OM_LotTrans> _lstLot;
        private List<OM10100_pgTaxTrans_Result> _lstTax;
        private List<OM10100_pgTaxTrans_Result> _lstTaxDoc;
        private List<OM_OrdDisc> _lstDisc;
        private List<OM10100_pdSOPrice_Result> _lstPrice;
        private OM10100_pcOrder_Result _objOrder;
        private OM_Setup _objOM;
        private IN_Setup _objIN;
        private OM_UserDefault _objUser;
        private OM_OrderType _objType;
        private AR_Customer _objCust;

        private string _handle;
        private bool _isDelete;
        private string _discLineRef;
        private string _lineRef;
        private double _docDiscAmt;

        #region Action
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

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                _form = data;
                SaveData(data);

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Save, new { orderNbr = _objOrder.OrderNbr });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Util.CreateError(ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult Delete(FormCollection data)
        {
            try
            {
                var access = Session["OM10100"] as AccessRight;
                if (!access.Delete)
                {
                    throw new MessageException(MessageType.Message, "728");
                }

                _objOrder = data.ConvertToObject<OM10100_pcOrder_Result>(false, new string[] { "DoNotCalDisc","CreditHold" });
                _objOrder.DoNotCalDisc = (data["DoNotCalDisc"].PassNull() != string.Empty ? 1 : 0).ToShort();
                _objOrder.CreditHold = (data["CreditHold"].PassNull() != string.Empty ? 1 : 0).ToBool();

                _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objOrder.BranchID && p.SetupID == "IN");
                if (_objIN == null)
                {
                    throw new MessageException(MessageType.Message, "8006");
                }

                _objOM = _app.OM_Setup.FirstOrDefault();
                if (_objOM == null)
                {
                    throw new MessageException("8006");
                }


                _objType = _app.OM_OrderType.FirstOrDefault(p => p.OrderType == _objOrder.OrderType);
                if (_objType == null)
                {
                    throw new MessageException("8013");
                }

                _objCust = _app.AR_Customer.FirstOrDefault(p => p.CustId == _objOrder.CustID && p.BranchID == _objOrder.BranchID);
                if (_objCust == null)
                {
                    throw new MessageException("2015032701", new string[] { _objOrder.CustID });
                }

                _objUser = _app.OM_UserDefault.First(p => p.UserID == Current.UserName && p.DfltBranchID == _objOrder.BranchID);
                if (_objType == null)
                {
                    throw new MessageException("8013");
                }

                OM_SalesOrd order = _app.OM_SalesOrd.FirstOrDefault(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr);
                if (order != null && _objOrder.Status != "N")
                {
                    throw new MessageException(MessageType.Message, "20140709");
                }
                if (order == null)
                {
                    throw new MessageException(MessageType.Message, "8012", parm: new[] { _objOrder.OrderNbr });
                }

                double oldQty = 0;

                OM_OrdAddr ordAddr = _app.OM_OrdAddr.FirstOrDefault(p => p.OrderNbr == _objOrder.OrderNbr && p.BranchID == _objOrder.BranchID);
                if (ordAddr != null) _app.OM_OrdAddr.DeleteObject(ordAddr);

                ClearGroupDocBudget();
                _lstOrdDet = _app.OM10100_pgOrderDet(_objOrder.BranchID, _objOrder.OrderNbr, "%").ToList();
                foreach (var detLoadResult in _lstOrdDet)
                {
                    OM_SalesOrdDet det = _app.OM_SalesOrdDet.FirstOrDefault(
                            p =>
                                p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr &&
                                p.LineRef == detLoadResult.LineRef);
                    if (det != null)
                    {
                        if (det.BOType != "B" && _objType.INDocType != "CM" && _objType.INDocType != "DM" &&
                            _objType.INDocType != "NA" && _objType.INDocType != "RC")
                        {
                            oldQty = det.UnitMultDiv == "D"
                                ? det.LineQty / det.UnitRate
                                : det.LineQty * det.UnitRate;
                            UpdateAllocSO(det.InvtID, det.SiteID, oldQty, 0, 0);
                        }
                        //if(det.BOType!="B" && _objType.INDocType!="CM" && _objType.INDocType!="DM" && _objType.INDocType!="NA" && _objType.INDocType!="RC")

                        OM_PPBudget objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == det.BudgetID1);
                        OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == det.DiscID1);
                        if (_objType.ARDocType != "NA" && det.BOType != "R" && objBudget != null && objDisc != null &&
                            objDisc.DiscType == "L")
                        {
                            if (objBudget.ApplyTo == "A")
                            {
                                OM_PPAlloc objAlloc =
                                    _app.OM_PPAlloc.FirstOrDefault(
                                        p =>
                                            p.BudgetID == objBudget.BudgetID &&
                                            p.ObjID ==
                                            (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID) &&
                                            p.FreeItemID == "." && p.CpnyID == _objOrder.BranchID);
                                if (objAlloc != null)
                                {
                                    OM_PPCpny objCpny =
                                        _app.OM_PPCpny.FirstOrDefault(
                                            p =>
                                                p.CpnyID == _objOrder.BranchID && p.BudgetID == det.BudgetID1 &&
                                                p.FreeItemID == ".");

                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent -
                                                          det.DiscAmt1 *
                                                          (_objType.ARDocType == "CM" ||
                                                           _objType.ARDocType == "CC"
                                                              ? -1
                                                              : 1);
                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent -
                                                           det.DiscAmt1 *
                                                           (_objType.ARDocType == "CM" ||
                                                            _objType.ARDocType == "CC"
                                                               ? -1
                                                               : 1);
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                            else
                            {
                                OM_PPFreeItem objPPInvt =
                                    _app.OM_PPFreeItem.FirstOrDefault(
                                        p => p.BudgetID == det.BudgetID1 && p.FreeItemID == det.InvtID);
                                if (objPPInvt != null)
                                {
                                    IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                    if (objInvt == null) objInvt = new IN_Inventory();
                                    IN_UnitConversion uomFrom = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                        det.SlsUnit);
                                    if (uomFrom != null)
                                    {
                                        IN_UnitConversion uomTo = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                            objPPInvt.UnitDesc);
                                        double rate = 1;
                                        double rate2 = 1;

                                        if (uomFrom.MultDiv == "M")
                                            rate = uomFrom.CnvFact;
                                        else
                                            rate = 1 / uomFrom.CnvFact;

                                        if (uomTo.MultDiv == "M")
                                            rate2 = uomTo.CnvFact;
                                        else
                                            rate2 = 1 / uomTo.CnvFact;
                                        rate = Math.Round(rate / rate2, 2);

                                        OM_PPAlloc objAlloc =
                                            _app.OM_PPAlloc.FirstOrDefault(
                                                p =>
                                                    p.BudgetID == objBudget.BudgetID &&
                                                    p.ObjID ==
                                                    (objBudget.AllocType == "1"
                                                        ? _objOrder.SlsPerID
                                                        : _objOrder.CustID) && p.FreeItemID == det.InvtID &&
                                                    p.CpnyID == _objOrder.BranchID);
                                        if (objAlloc != null)
                                        {
                                            objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.FreeItemQty1 * rate *
                                                                   (_objType.ARDocType == "CM" ||
                                                                    _objType.ARDocType == "CC"
                                                                       ? -1
                                                                       : 1);
                                            objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;

                                            OM_PPCpny objCpny =
                                                _app.OM_PPCpny.FirstOrDefault(
                                                    p =>
                                                        p.CpnyID == _objOrder.BranchID &&
                                                        p.BudgetID == det.BudgetID1 && p.FreeItemID == det.InvtID);
                                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent -
                                                                  det.FreeItemQty1 * rate *
                                                                  (_objType.ARDocType == "CM" ||
                                                                   _objType.ARDocType == "CC"
                                                                      ? -1
                                                                      : 1);
                                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                                            objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent -
                                                                    det.FreeItemQty1 * rate *
                                                                    (_objType.ARDocType == "CM" ||
                                                                     _objType.ARDocType == "CC"
                                                                        ? -1
                                                                        : 1);
                                            objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;
                                        }
                                    }
                                }
                            }
                        }
                        //if(_objType.ARDocType!="NA" && det.BOType!="R" && objBudget!=null &&  objDisc!=null && objDisc.DiscType=="L" )

                        objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == det.BudgetID2);
                        objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == det.DiscID2);
                        if (_objType.ARDocType != "NA" && det.BOType != "R" && objBudget != null && objDisc != null &&
                            objDisc.DiscType == "L")
                        {
                            if (objBudget.ApplyTo == "A")
                            {
                                OM_PPAlloc objAlloc =
                                    _app.OM_PPAlloc.FirstOrDefault(
                                        p =>
                                            p.BudgetID == objBudget.BudgetID &&
                                            p.ObjID ==
                                            (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID) &&
                                            p.FreeItemID == "." && p.CpnyID == _objOrder.BranchID);
                                if (objAlloc != null)
                                {
                                    OM_PPCpny objCpny =
                                        _app.OM_PPCpny.FirstOrDefault(
                                            p =>
                                                p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID &&
                                                p.FreeItemID == ".");
                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent -
                                                          det.DiscAmt2 *
                                                          (_objType.ARDocType == "CM" ||
                                                           _objType.ARDocType == "CC"
                                                              ? -1
                                                              : 1);
                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.DiscAmt2 *
                                                           (_objType.ARDocType == "CM" ||
                                                            _objType.ARDocType == "CC"
                                                               ? -1
                                                               : 1);
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                            else
                            {
                                OM_PPFreeItem objPPInvt =
                                    _app.OM_PPFreeItem.FirstOrDefault(
                                        p => p.BudgetID == det.BudgetID1 && p.FreeItemID == det.InvtID);
                                if (objPPInvt != null)
                                {
                                    IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                    if (objInvt == null) objInvt = new IN_Inventory();
                                    IN_UnitConversion uomFrom = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                        det.SlsUnit);
                                    if (uomFrom != null)
                                    {
                                        IN_UnitConversion uomTo = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                            objPPInvt.UnitDesc);
                                        double rate = 1;
                                        double rate2 = 1;

                                        if (uomFrom.MultDiv == "M")
                                            rate = uomFrom.CnvFact;
                                        else
                                            rate = 1 / uomFrom.CnvFact;

                                        if (uomTo.MultDiv == "M")
                                            rate2 = uomTo.CnvFact;
                                        else
                                            rate2 = 1 / uomTo.CnvFact;
                                        rate = Math.Round(rate / rate2, 2);


                                        OM_PPAlloc objAlloc =
                                            _app.OM_PPAlloc.FirstOrDefault(
                                                p =>
                                                    p.BudgetID == objBudget.BudgetID &&
                                                    p.ObjID ==
                                                    (objBudget.AllocType == "1"
                                                        ? _objOrder.SlsPerID
                                                        : _objOrder.CustID) && p.FreeItemID == det.InvtID &&
                                                    p.CpnyID == _objOrder.BranchID);
                                        if (objAlloc != null)
                                        {
                                            objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.FreeItemQty2 * rate *
                                                                   (_objType.ARDocType == "CM" ||
                                                                    _objType.ARDocType == "CC"
                                                                       ? -1
                                                                       : 1);
                                            objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;

                                            OM_PPCpny objCpny =
                                                _app.OM_PPCpny.FirstOrDefault(
                                                    p =>
                                                        p.CpnyID == _objOrder.BranchID &&
                                                        p.BudgetID == det.BudgetID1 && p.FreeItemID == det.InvtID);
                                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent -
                                                                  det.FreeItemQty2 * rate *
                                                                  (_objType.ARDocType == "CM" ||
                                                                   _objType.ARDocType == "CC"
                                                                      ? -1
                                                                      : 1);
                                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                                            objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent -
                                                                    det.FreeItemQty2 * rate *
                                                                    (_objType.ARDocType == "CM" ||
                                                                     _objType.ARDocType == "CC"
                                                                        ? -1
                                                                        : 1);
                                            objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;
                                        }
                                    }
                                }
                            }
                        }
                        //if(_objType.ARDocType!="NA" && det.BOType!="R" && objBudget!=null &&  objDisc!=null && objDisc.DiscType=="L" )

                        objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == det.BudgetID1);
                        if (Util.PassNull(det.DiscCode) != string.Empty && _objType.ARDocType != "NA" &&
                            det.BOType != "R" && (det.FreeItem || det.DiscAmt > 0) && objBudget != null)
                        {
                            if (objBudget.ApplyTo == "A")
                            {
                                OM_PPAlloc objAlloc =
                                    _app.OM_PPAlloc.FirstOrDefault(
                                        p =>
                                            p.BudgetID == objBudget.BudgetID &&
                                            p.ObjID ==
                                            (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID) &&
                                            p.FreeItemID == "." && p.CpnyID == _objOrder.BranchID);
                                if (objAlloc != null)
                                {
                                    OM_PPCpny objCpny =
                                        _app.OM_PPCpny.FirstOrDefault(
                                            p =>
                                                p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID &&
                                                p.FreeItemID == ".");
                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - det.DiscAmt *
                                                          (_objType.ARDocType == "CM" ||
                                                           _objType.ARDocType == "CC"
                                                              ? -1
                                                              : 1);
                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.DiscAmt *
                                                           (_objType.ARDocType == "CM" ||
                                                            _objType.ARDocType == "CC"
                                                               ? -1
                                                               : 1);
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            } //  if(objBudget.ApplyTo=="A")
                            else
                            {
                                if (det.FreeItem)
                                {
                                    OM_PPFreeItem objPPInvt =
                                        _app.OM_PPFreeItem.FirstOrDefault(
                                            p => p.BudgetID == det.BudgetID1 && p.FreeItemID == det.InvtID);
                                    if (objPPInvt != null)
                                    {
                                        IN_Inventory objInvt =
                                            _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                        if (objInvt == null) objInvt = new IN_Inventory();
                                        IN_UnitConversion uomFrom = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                            det.SlsUnit);
                                        if (uomFrom != null)
                                        {
                                            IN_UnitConversion uomTo = SetUOM(det.InvtID, objInvt.ClassID,
                                                objInvt.StkUnit, objPPInvt.UnitDesc);
                                            double rate = 1;
                                            double rate2 = 1;

                                            if (uomFrom.MultDiv == "M")
                                                rate = uomFrom.CnvFact;
                                            else
                                                rate = 1 / uomFrom.CnvFact;

                                            if (uomTo.MultDiv == "M")
                                                rate2 = uomTo.CnvFact;
                                            else
                                                rate2 = 1 / uomTo.CnvFact;
                                            rate = Math.Round(rate / rate2, 2);


                                            OM_PPAlloc objAlloc =
                                                _app.OM_PPAlloc.FirstOrDefault(
                                                    p =>
                                                        p.BudgetID == objBudget.BudgetID &&
                                                        p.ObjID ==
                                                        (objBudget.AllocType == "1"
                                                            ? _objOrder.SlsPerID
                                                            : _objOrder.CustID) && p.FreeItemID == det.InvtID &&
                                                        p.CpnyID == _objOrder.BranchID);
                                            if (objAlloc != null)
                                            {
                                                objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.LineQty * rate *
                                                                       (_objType.ARDocType == "CM" ||
                                                                        _objType.ARDocType == "CC"
                                                                           ? -1
                                                                           : 1);
                                                objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;

                                                OM_PPCpny objCpny =
                                                    _app.OM_PPCpny.FirstOrDefault(
                                                        p =>
                                                            p.CpnyID == _objOrder.BranchID &&
                                                            p.BudgetID == det.BudgetID1 && p.FreeItemID == det.InvtID);
                                                objCpny.QtyAmtSpent = objCpny.QtyAmtSpent -
                                                                      det.LineQty * rate *
                                                                      (_objType.ARDocType == "CM" ||
                                                                       _objType.ARDocType == "CC"
                                                                          ? -1
                                                                          : 1);
                                                objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                                                objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent -
                                                                        det.LineQty * rate *
                                                                        (_objType.ARDocType == "CM" ||
                                                                         _objType.ARDocType == "CC"
                                                                            ? -1
                                                                            : 1);
                                                objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;
                                            }
                                        }
                                    }
                                }
                            }
                        } //if(det.DiscCode.PassNull()!=string.Empty && _objType.ARDocType!="NA" && det.BOType!="R" && (det.FreeItem || det.DiscAmt>0) && objBudget!=null)
                        var lstLot = _app.OM_LotTrans.Where(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr && p.OMLineRef == det.LineRef).ToList();
                        foreach (var lot in lstLot)
                        {
                            if (det.BOType != "B" && _objType.INDocType != "CM" && _objType.INDocType != "DM" && _objType.INDocType != "NA" && _objType.INDocType != "RC")
                            {
                                oldQty = Math.Round(lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact, 0);

                                UpdateAllocLotSO(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                            }
                            _app.OM_LotTrans.DeleteObject(lot);
                        }
                        _app.OM_SalesOrdDet.DeleteObject(det);
                    } //if(det!=null)
                  
                } //foreach (var detLoadResult in _lstOrdDet)

               

                _lstDisc =
                    _app.OM_OrdDisc.Where(
                        p => p.OrderNbr == _objOrder.OrderNbr && p.BranchID == _objOrder.BranchID).ToList();
                foreach (OM_OrdDisc currentDisc in _lstDisc)
                {
                    _app.OM_OrdDisc.DeleteObject(currentDisc);
                }

                _app.OM_SalesOrd.DeleteObject(order);

                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Delete);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult DeleteDet(FormCollection data)
        {
            try
            {
                _isDelete = true;
                var access = Session["OM10100"] as AccessRight;

                var detHandler = new StoreDataHandler(data["lstOrdDet"]);
                _lstOrdDet = detHandler.ObjectData<OM10100_pgOrderDet_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty && p.InvtID.PassNull()!=string.Empty).ToList();

                var lotHandler = new StoreDataHandler(data["lstLot"]);
                _lstLot = lotHandler.ObjectData<OM_LotTrans>().Where(p => Util.PassNull(p.OMLineRef) != string.Empty && p.InvtID.PassNull() != string.Empty && p.LotSerNbr.PassNull()!=string.Empty).ToList();

                _objOrder = data.ConvertToObject<OM10100_pcOrder_Result>(false, new string[] { "DoNotCalDisc", "CreditHold" });

                string lineRef = Util.PassNull(data["LineRef"]);
                OM_SalesOrdDet det = _app.OM_SalesOrdDet.FirstOrDefault(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr && p.LineRef == lineRef);

                var taxHandler = new StoreDataHandler(data["lstTax"]);
                _lstTax = taxHandler.ObjectData<OM10100_pgTaxTrans_Result>().ToList();

                var discHandler = new StoreDataHandler(data["lstDisc"]);
                _lstDisc = discHandler.ObjectData<OM_OrdDisc>().ToList();

             
                _objOrder.DoNotCalDisc = (data["DoNotCalDisc"].PassNull() != string.Empty ? 1 : 0).ToShort();
                _objOrder.CreditHold = (data["CreditHold"].PassNull() != string.Empty ? 1 : 0).ToBool();

                _lstOldOrdDet = _app.OM10100_pgOrderDet(_objOrder.BranchID, _objOrder.OrderNbr, "%").ToList();

                _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objOrder.BranchID && p.SetupID == "IN");
                if (_objIN == null)
                {
                    throw new MessageException(MessageType.Message, "8006");
                }

                _objOM = _app.OM_Setup.FirstOrDefault();
                if (_objOM == null)
                {
                    throw new MessageException("8006");
                }


                _objType = _app.OM_OrderType.FirstOrDefault(p => p.OrderType == _objOrder.OrderType);
                if (_objType == null)
                {
                    throw new MessageException("8013");
                }

                _objCust = _app.AR_Customer.FirstOrDefault(p => p.CustId == _objOrder.CustID && p.BranchID == _objOrder.BranchID);
                if (_objCust == null)
                {
                    throw new MessageException("2015032701", new string[] { _objOrder.CustID });
                }

                _objUser = _app.OM_UserDefault.First(p => p.UserID == Current.UserName && p.DfltBranchID == _objOrder.BranchID);
                if (_objType == null)
                {
                    throw new MessageException("8013");
                }

             
                int indexDet = -1;
                Delete_Det(data["LineRef"]);
                for (int i = 0; i < _lstOrdDet.Count; i++)
                {
                    if (_lstOrdDet[i].LineRef == lineRef)
                    {
                        indexDet = i;
                        break;
                    }
                }
                if (indexDet == -1) return Json(new { success = true });
                DelTax(indexDet);
                foreach (OM_OrdDisc disc in _lstDisc)
                {
                    if (_lstOrdDet[indexDet].FreeItem && Util.PassNull(_lstOrdDet[indexDet].DiscID1) != string.Empty &&
                        disc.FreeItemID == _lstOrdDet[indexDet].InvtID && disc.SOLineRef == _lstOrdDet[indexDet].LineRef)
                    {
                        disc.FreeItemID = disc.FreeItemID + "_D";
                        disc.FreeItemQty = 0;
                        disc.UserOperationLog = "User Deleted Free Item";
                    }
                }
                _lstOrdDet.RemoveAt(indexDet);
                CalcDet();
                CalcTaxTotal();
                SaveData(data);

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Util.CreateMessage(MessageProcess.Delete);
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult Report(FormCollection data)
        {
            try
            {
                _form = data;
                _objOrder = data.ConvertToObject<OM10100_pcOrder_Result>(false, new string[] { "DoNotCalDisc" ,"CreditHold"});
                _objOrder.DoNotCalDisc = (data["DoNotCalDisc"].PassNull() != string.Empty ? 1 : 0).ToShort();
                _objOrder.CreditHold = (data["CreditHold"].PassNull() != string.Empty ? 1 : 0).ToBool();

                User user = _sys.Users.FirstOrDefault(p => p.UserName.ToLower() == Current.UserName.ToLower());
                string reportName = "";
                var rpt = new RPTRunning();
                rpt.ResetET();
                if (_form["type"] == "V")
                {
                    reportName = "OM_VatInvoice";
                    rpt.ReportNbr = "OM604";
                    rpt.MachineName = "Web";
                    rpt.ReportCap = "OM_VatInvoice";
                    rpt.ReportName = "OM_VatInvoice";
                    rpt.ReportDate = DateTime.Now;
                    rpt.DateParm00 = DateTime.Now;
                    rpt.DateParm01 = DateTime.Now;
                    rpt.DateParm02 = DateTime.Now;
                    rpt.DateParm03 = DateTime.Now;
                    rpt.StringParm00 = _objOrder.BranchID;
                    rpt.StringParm01 = _objOrder.OrderNbr;
                    rpt.UserID = Current.UserName;
                    rpt.AppPath = "Reports\\";
                    rpt.ClientName = Current.UserName;
                    rpt.LoggedCpnyID = Current.CpnyID;
                    rpt.CpnyID = user.CpnyID;
                    rpt.LangID = Current.LangID;
                    _app.RPTRunnings.AddObject(rpt);
                }
                else if (_form["type"] == "S")
                {
                    reportName = "OM_ShipInvcPayment";
                    rpt.ReportNbr = "OM602";
                    rpt.MachineName = "Web";
                    rpt.ReportCap = "OM_ShipInvcPayment";
                    rpt.ReportName = "OM_ShipInvcPayment";
                    rpt.BooleanParm00 = (_form["ARDocType"] == "CM" ? true : false).ToShort();
                    rpt.ReportDate = DateTime.Now;
                    rpt.DateParm00 = _objOrder.OrderDate;
                    rpt.DateParm01 = _objOrder.OrderDate;
                    rpt.DateParm02 = DateTime.Now;
                    rpt.DateParm03 = DateTime.Now;
                    rpt.StringParm00 = _objOrder.SlsPerID;
                    rpt.StringParm01 = _objOrder.DeliveryID;
                    rpt.StringParm02 = _objOrder.BranchID;
                    rpt.StringParm03 = _objOrder.OrderNbr;
                    rpt.UserID = Current.UserName;
                    rpt.AppPath = "Reports\\";
                    rpt.ClientName = Current.UserName;
                    rpt.LoggedCpnyID = Current.CpnyID;
                    rpt.CpnyID = user.CpnyID;
                    rpt.LangID = Current.LangID;
                    _app.RPTRunnings.AddObject(rpt);
                }
                _app.SaveChanges();

                if (_logMessage != null)
                {
                    return _logMessage;
                }
                return Json(new { success = true, reportID = rpt.ReportID, reportName });
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }
        #endregion

        #region Function
        private bool CheckData_Order()
        {
            if (_objOrder.OrderType.PassNull() == string.Empty)
            {
                throw new MessageException("1000", new[] { Util.GetLang("ordertype") });
            }

            if (_objOrder.CustID.PassNull() == string.Empty)
            {
                throw new MessageException("1000", new[] { Util.GetLang("custid") });
            }
            if (_objOrder.SlsPerID.PassNull() == string.Empty)
            {
                throw new MessageException("1000", new[] { Util.GetLang("slsperid") });
            }
            //if (_objOrder.DeliveryID.PassNull() == string.Empty)
            //{
            //    throw new MessageException("1000", new[] { Util.GetLang("DeliveryID") });
            //}
            if (_objOrder.Terms.PassNull() == string.Empty)
            {
                throw new MessageException("1000", new[] { Util.GetLang("terms") });
            }
            if (_lstOrdDet.Count == 0 && !_isDelete)
            {
                throw new MessageException("704");
            }

            for (int i = 0; i < _lstOrdDet.Count; i++)
            {
                string invtID = _lstOrdDet[i].InvtID;
                string siteID = _lstOrdDet[i].SiteID;
                double editQty = 0;
                double qtyTot = 0;
                if (_lstOrdDet[i].LineQty == 0) 
                {
                    throw new MessageException("1000", new[] { Util.GetLang("Qty") });
                }

                if (_lstOrdDet[i].SiteID.PassNull()==string.Empty) {
                    throw new MessageException("1000", new[] { Util.GetLang("SiteID") });
                }

                if (_lstOrdDet[i].UnitMultDiv.PassNull() == string.Empty || _lstOrdDet[i].SlsUnit.PassNull() == string.Empty)
                {
                    throw new MessageException("2525", new[] { _lstOrdDet[i].InvtID });
                }
                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                if (objInvt != null)
                {
                    if (objInvt.StkItem == 1 || _lstOrdDet[i].BOType != "B")
                    {
                        if (_lstOrdDet[i].UnitMultDiv == "M")
                            editQty = _lstOrdDet[i].LineQty * (_lstOrdDet[i].UnitRate == 0 ? 1 : _lstOrdDet[i].UnitRate);
                        else
                            editQty = _lstOrdDet[i].LineQty / (_lstOrdDet[i].UnitRate == 0 ? 1 : _lstOrdDet[i].UnitRate);
                        qtyTot = editQty + CalculateInvtTotals(_lstOrdDet[i].InvtID, _lstOrdDet[i].SiteID, _lstOrdDet[i].LineRef);
                        IN_ItemSite objItemSite = _app.IN_ItemSite.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID);
                        if (objItemSite == null) objItemSite = new IN_ItemSite();
                        if (!_objIN.NegQty)
                        {
                            if (_objType != null)
                            {
                                if (_objType.INDocType != "CM" && _objType.INDocType != "DM" &&
                                    _objType.INDocType != "NA" && _objType.INDocType != "RC")
                                {
                                    if (qtyTot > objItemSite.QtyAvail)
                                    {
                                        throw new MessageException("1043", new[] { objInvt.InvtID, siteID });
                                    }
                                    if (_objType.BO && _lstOrdDet[i].BOType == "R")
                                    {
                                        double? qtyB = _app.OM10100_pdSalespersonStock(_objOrder.SlsPerID, invtID, _objOrder.OrderDate.ToDateShort()).FirstOrDefault();
                                        if (qtyB == null) qtyB = 0;
                                        if (editQty > qtyB)
                                        {
                                            throw new MessageException("1043", new[] { objInvt.InvtID, siteID });
                                        }
                                    }
                                }
                                else if (_objOrder.OrderType != "SR" && _objOrder.OrderType != "BL" &&
                                         _objOrder.OrderType != "OC" && _objType.INDocType != "CM" &&
                                         _objType.INDocType != "DM" && _objType.INDocType != "NA" &&
                                         _objType.INDocType != "RC")
                                {
                                    if (editQty > objItemSite.QtyAvail)
                                    {
                                        throw new MessageException("1043", new[] { objInvt.InvtID, objItemSite.SiteID });
                                    }
                                }
                                else if (_objOrder.OrderType == "SR" || _objOrder.OrderType == "BL" ||
                                         _objOrder.OrderType == "OC")
                                {
                                    double? stock = _app.OM10100_pdSalespersonStock(_objOrder.SlsPerID, invtID, _objOrder.OrderDate.ToDateShort()).FirstOrDefault();
                                    if (stock == null) stock = 0;
                                    if (qtyTot > stock)
                                    {
                                        throw new MessageException("1044", new[] { objInvt.InvtID, _form["SlsperID"] });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_objType.INDocType != "CM" && _objType.INDocType != "DM" &&
                                _objType.INDocType != "NA" && _objType.INDocType != "RC")
                            {
                                if (editQty > objItemSite.QtyAvail)
                                {
                                    throw new MessageException("1043", new[] { objInvt.InvtID + "," + objItemSite.SiteID });
                                }
                            }
                        }
                    }

                    if (objInvt.StkItem == 1 && _lstOrdDet[i].BOType != "O" && _lstOrdDet[i].LineQty == 0 && _lstOrdDet[i].LineAmt != 0)
                    {
                        throw new MessageException("702");
                    }
                    if (objInvt.ValMthd == "S" && Util.PassNull(_lstOrdDet[i].CostID) == string.Empty)
                    {
                        throw new MessageException("734");
                    }
                }
                if (_lstOrdDet[i].FreeItem && _lstOrdDet[i].LineAmt != 0)
                {
                    throw new MessageException("703");
                }
                if (!_lstOrdDet[i].FreeItem && _lstOrdDet[i].BOType != "R" && _lstOrdDet[i].LineAmt == 0 &&
                    _lstOrdDet[i].QtyBO == 0)
                {
                    throw new MessageException("703");
                }
                if (_objType.BO)
                {
                    if (_lstOrdDet[i].BOType != "O" && _lstOrdDet[i].LineQty == 0 && _lstOrdDet[i].QtyBO == 0)
                    {
                        throw new MessageException("233");
                    }
                }
                else
                {
                    if (_lstOrdDet[i].LineQty == 0 && _lstOrdDet[i].QtyBO == 0)
                    {
                        throw new MessageException("233");
                    }
                }
                if (_lstOrdDet[i].SlsPrice == 0 && !_lstOrdDet[i].FreeItem)
                {
                    throw new MessageException("726");
                }
                if (_objOM.ReqDiscID && Util.PassNull(_lstOrdDet[i].DiscCode) == string.Empty &&
                    Util.PassNull(_lstOrdDet[i].DiscID1) == string.Empty && _lstOrdDet[i].FreeItem)
                {
                    throw new MessageException("746");
                }

                if (_lstOrdDet[i].BOType == "B" && Util.PassNull(_lstOrdDet[i].BOCustID) == string.Empty)
                {
                    throw new MessageException("734");
                }
                if (_lstOrdDet[i].DiscAmt > 0 && _lstOrdDet[i].DiscCode.PassNull() != string.Empty &&
                    _lstOrdDet[i].BudgetID1.PassNull() != string.Empty)
                {
                    double discAmt = _lstOrdDet[i].DiscAmt;
                    string discID = _lstOrdDet[i].DiscCode;
                    string budGetID = _lstOrdDet[i].BudgetID1;
                    string tmp = string.Empty;
                    if (!CheckAvailableDiscBudget(ref budGetID, ref discID, ref tmp, ref discAmt, false, true,
                            _lstOrdDet[i].LineRef, true, true, _lstOrdDet[i].InvtID, _lstOrdDet[i].SlsUnit, true))
                    {
                        //throw new MessageException("2015032702", new []{_lstOrdDet[i].DiscCode, _lstOrdDet[i].BudgetID1});
                    }
                }

                if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack != "N" )
                {
                    var lstLot = _lstLot.Where(p => p.OMLineRef == _lstOrdDet[i].LineRef).ToList();
                    double lotQty = 0;
                    foreach (var item in lstLot)
                    {
                        if (item.InvtID != _lstOrdDet[i].InvtID || item.SiteID != _lstOrdDet[i].SiteID)
                        {
                             throw new MessageException("2015040501", new[] { _lstOrdDet[i].InvtID  });
                        }

                        if (item.UnitMultDiv.PassNull() == string.Empty || item.UnitDesc.PassNull() == string.Empty)
                        {
                            throw new MessageException("2015040503", new[] { _lstOrdDet[i].InvtID }); 
                        }


                        lotQty += Math.Round(item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact,0);
                    }
                    double detQty = Math.Round(_lstLot[i].UnitMultDiv == "M" ? _lstOrdDet[i].LineQty * _lstOrdDet[i].UnitRate : _lstOrdDet[i].LineQty  / _lstOrdDet[i].UnitRate,0);
                    if (detQty != lotQty)
                    {
                         throw new MessageException("2015040502", new[] { _lstOrdDet[i].InvtID });
                    }
                }
            }

            //if (!_checkInvcNbr && txtInvcNbr.Text.Trim() != string.Empty && _objType.ARDocType != "CM" && _objType.ARDocType != "CC")
            if (_objOrder.InvcNbr.PassNull() != string.Empty && _objType.ARDocType != "CM" && _objType.ARDocType != "CC")
            {
                string orderNbr = _objOrder.OrderNbr.PassNull();
                string invcNbr = _objOrder.InvcNbr.PassNull();
                OM_SalesOrd data = _app.OM_SalesOrd.FirstOrDefault(p => p.OrderNbr.ToUpper() != orderNbr && p.InvcNbr.ToUpper() == invcNbr.ToUpper());
                if (data != null)
                {
                    throw new MessageException("746");
                }
            }

            return true;
        }

        private double CalculateInvtTotals(string invtID, string siteID, string lineRef)
        {
            double qty = 0;
            double oldQTy = 0;
            if (_form["OrderType"] == "SR" || _form["OrderType"] == "BL" || _form["OrderType"] == "OC")
            {
                oldQTy = _app.OM10100_pgOrderDet(_objOrder.BranchID, _objOrder.OrderNbr, "%").Where(p => p.InvtID == invtID && p.BOType != "B").Sum(p => p.StkQty);
                qty = _lstOrdDet.Where(p => p.InvtID == invtID && p.LineRef != lineRef && p.BOType != "B").Sum(p => p.StkQty);
            }
            else
            {
                oldQTy = _app.OM10100_pgOrderDet(_objOrder.BranchID, _objOrder.OrderNbr, "%").Where(p => p.InvtID == invtID && p.SiteID == siteID && p.BOType != "B").Sum(p => p.StkQty);
                qty = _lstOrdDet.Where(p => p.InvtID == invtID && p.LineRef != lineRef && p.BOType != "B" && p.SiteID == siteID).Sum(p => p.StkQty);
            }
            return qty - oldQTy;
        }

        private bool CheckAvailableDiscBudget(ref string refbudgetID, ref string discID, ref string discSeq,
            ref double discAmtQty, bool freeItem, bool manualDisc, string lineRef, bool firstCal, bool lineDisc,
            string invtID, string unit, bool throwEx = false)
        {
            string budgetID = refbudgetID;
            OM_PPBudget objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == budgetID);
            if (objBudget == null) objBudget = new OM_PPBudget();

            if (_objType.ARDocType != "NA" && _objType.ARDocType != "CM" && _objType.ARDocType != "CC" && budgetID != string.Empty && objBudget != null && objBudget.Active)
            {
                if ((objBudget.ApplyTo == "A" && !freeItem) || (objBudget.ApplyTo != "A" && freeItem))
                {
                    OM_PPAlloc objbudgetAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.BudgetID == budgetID && p.CpnyID == _objOrder.BranchID &&
                        p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                    if (objbudgetAlloc != null)
                    {
                        var objCpny = new OM_PPCpny();
                        if (objBudget.ApplyTo == "A" && lineDisc)
                        {
                            objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == budgetID && p.FreeItemID == ".");
                            if (objCpny == null) objCpny = new OM_PPCpny();
                            if (objCpny.QtyAmtAvail < discAmtQty)
                            {
                                if (throwEx) throw new MessageException("402");
                                if (!lineDisc) discAmtQty = 0;
                                budgetID = string.Empty;
                                discID = string.Empty;
                                discSeq = string.Empty;
                                return false;
                            }
                        }
                        else if (objBudget.ApplyTo != "A" && lineDisc)
                        {
                            OM_PPFreeItem objPPInvt = _app.OM_PPFreeItem.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == invtID);
                            if (objPPInvt != null)
                            {
                                objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == budgetID &&
                                            p.FreeItemID == invtID);

                                if (objCpny == null) objCpny = new OM_PPCpny();
                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);

                                IN_UnitConversion uomFrom = SetUOM(invtID, objInvt.ClassID, objInvt.StkUnit, unit);
                                if (uomFrom != null)
                                {
                                    IN_UnitConversion uomTo = SetUOM(invtID, objInvt.ClassID, objInvt.StkUnit,
                                        objPPInvt.UnitDesc);
                                    double rate = 1;
                                    double rate2 = 1;

                                    if (uomFrom.MultDiv == "M")
                                        rate = uomFrom.CnvFact;
                                    else
                                        rate = 1 / uomFrom.CnvFact;

                                    if (uomTo.MultDiv == "M")
                                        rate2 = uomTo.CnvFact;
                                    else
                                        rate2 = 1 / uomTo.CnvFact;

                                    rate = Math.Round(rate / rate2, 2);

                                    double tmp = discAmtQty * rate;
                                    if (objCpny.QtyAmtAvail < tmp)
                                    {
                                        if (throwEx)
                                        {
                                            throw new MessageException(MessageType.Message, "402");
                                        }
                                        if (!lineDisc) discAmtQty = 0;
                                        budgetID = string.Empty;
                                        discID = string.Empty;
                                        discSeq = string.Empty;
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (throwEx)
                        {
                            throw new MessageException(MessageType.Message, "403");
                        }
                        if (!lineDisc) discAmtQty = 0;
                        budgetID = string.Empty;
                        discID = string.Empty;
                        discSeq = string.Empty;
                        return false;
                    }
                }
            }
            return true;
        }

        private void CheckSubInLineQty(OM10100_pgOrderDet_Result det, double stkQty, string key)
        {
            if (!_objOrder.DoNotCalDisc.ToBool() && _objType.SalesType != "PRO" && !det.FreeItem && det.BOType != "R" && det.DiscPct == 0)
            {
                string discID1 = string.Empty;
                string discID2 = string.Empty;
                double discAmt = 0;
                double discAmt1 = 0;
                double discAmt2 = 0;
                double discPct1 = 0;
                double discPct2 = 0;
                string discSeq1 = "0";
                string discSeq2 = "0";
                string budgetID1 = string.Empty;
                string budgetID2 = string.Empty;
                double soFee = 0;
                string breakLineRef1 = string.Empty;
                string breakLineRef2 = string.Empty;
                if (_objOM.InlcSOFeeDisc)
                    soFee = det.SOFee;
                else
                    soFee = 0;

                OM_SalesOrdDet ordDet = _app.OM_SalesOrdDet.FirstOrDefault(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr && p.LineRef == det.LineRef);
                if (ordDet == null) ordDet = new OM_SalesOrdDet();
                if (det.BOType == "O")
                {
                    discAmt = GetDiscLineSetup(det, stkQty, det.LineQty * det.SlsPrice + soFee, ref discAmt1, ref discAmt2,
                        ref discPct1, ref discPct2, ref discSeq1, ref discSeq2, ref discID1,
                        ref discID2, ref budgetID1, ref budgetID2, ref breakLineRef1,
                        ref breakLineRef2);
                }
                else
                {
                    discAmt = GetDiscLineSetup(det, stkQty, (det.LineQty) * det.SlsPrice + soFee, ref discAmt1,
                        ref discAmt2,
                        ref discPct1, ref discPct2, ref discSeq1, ref discSeq2, ref discID1,
                        ref discID2, ref budgetID1, ref budgetID2, ref breakLineRef1,
                        ref breakLineRef2);

                }
                discAmt = Math.Round(discAmt, 0);
                discAmt1 = Math.Round(discAmt1, 0);
                discAmt2 = Math.Round(discAmt2, 0);

                if (discAmt1 == 0)
                {
                    discID1 = string.Empty;
                    discSeq1 = string.Empty;
                }
                if (discAmt2 == 0)
                {
                    discID2 = string.Empty;
                    discSeq2 = string.Empty;
                }
                if (!CheckAvailableDiscBudget(ref budgetID1, ref discID1, ref discSeq1, ref discAmt1, false, false,
                        det.LineRef, true, true, det.InvtID, det.SlsUnit))
                {
                    discAmt = discAmt - discAmt1;
                    discAmt1 = 0;
                    discPct1 = 0;
                    budgetID1 = string.Empty;
                    discID1 = string.Empty;
                    discSeq1 = string.Empty;
                    return;
                }
                if (!CheckAvailableDiscBudget(ref budgetID2, ref discID2, ref discSeq2, ref discAmt2, false, false,
                        det.LineRef, true, true, det.InvtID, det.SlsUnit))
                {
                    discAmt = discAmt - discAmt1;
                    discAmt2 = 0;
                    discPct2 = 0;
                    budgetID2 = string.Empty;
                    discID2 = string.Empty;
                    discSeq2 = string.Empty;
                    return;
                }
                det.DiscAmt = discAmt;
                if (det.BOType == "O")
                    det.DiscPct = Math.Round(discAmt != 0 ? ((det.DiscAmt * 100) / ((det.LineQty + det.QtyBO) * det.SlsPrice + soFee)) : 0, 0);
                else
                    det.DiscPct = Math.Round(discAmt != 0 ? ((det.DiscAmt * 100) / (det.LineQty * det.SlsPrice + soFee)) : 0, 0);

                det.DiscID1 = discID1;
                det.DiscID2 = discID2;
                det.DiscSeq1 = discSeq1;
                det.DiscSeq2 = discSeq2;
                det.BudgetID1 = budgetID1;
                det.BudgetID2 = budgetID2;
                det.DiscAmt1 = discAmt1;
                det.DiscAmt2 = discAmt2;
                det.DiscPct1 = Math.Round(discPct1, 2);

                if (det.BOType == "O")
                    det.LineAmt = Math.Round((det.LineQty + det.QtyBO) * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                else
                    det.LineAmt = Math.Round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);

                det.DiscCode = string.Empty;
                det.OrigOrderNbr = breakLineRef1 + breakLineRef2;
            }
            if (det.FreeItem)
                det.LineAmt = 0;
            else if (det.BOType == "R")
            {
                det.LineAmt = 0;
                det.SOFee = 0;
            }
        }

        private double GetDiscLineSetup(OM10100_pgOrderDet_Result det, double qty, double amt, ref double discAmt1,
            ref double discAmt2, ref double discPct1, ref double discPct2, ref string discSeq1, ref string discSeq2,
            ref string discID1, ref string discID2, ref string budgetID1, ref string budgetID2, ref string breaklineRef1,
            ref string breaklineRef2)
        {
            double discItemUnitQty = 0;
            List<OM10100_pdDiscLineSetUp_Result> lstSetup = _app.OM10100_pdDiscLineSetUp(_objOrder.BranchID).ToList();
            if (lstSetup.Count > 0)
            {
                foreach (OM10100_pdDiscLineSetUp_Result setup in lstSetup)
                {
                    OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID01);
                    if (objDisc != null)
                    {
                        if (objDisc.DiscType == "L")
                        {
                            List<OM_DiscSeq> lstSeq = _app.OM_DiscSeq.Where(p => p.DiscID == objDisc.DiscID && p.Status == "C" && p.Active == 1).ToList();
                            if (lstSeq.Count > 0)
                            {
                                foreach (OM_DiscSeq seq in lstSeq)
                                {
                                    OM_DiscItem objItem = _app.OM_DiscItem.FirstOrDefault(p =>
                                        p.DiscID == objDisc.DiscID && p.DiscSeq == seq.DiscSeq &&
                                        p.InvtID == det.InvtID);

                                    if (objItem == null) objItem = new OM_DiscItem();
                                    if (seq.BreakBy == "W")
                                    {
                                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                        if (objInvt == null) objInvt = new IN_Inventory();
                                        discItemUnitQty = qty * objInvt.StkWt / OM_GetCnvFactFromUnit(det.InvtID, objItem.UnitDesc);
                                    }
                                    else
                                        discItemUnitQty = qty / OM_GetCnvFactFromUnit(det.InvtID, objItem.UnitDesc);

                                    if (objDisc.DiscClass == "CC" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo != 0) ||
                                            ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);
                                        if (objDiscCust != null)
                                        {
                                            discID1 = objDisc.DiscID;
                                            discSeq1 = seq.DiscSeq;
                                            budgetID1 = seq.BudgetID;
                                            CalculateLineDisc(seq.DiscID, seq.DiscSeq, qty, amt, seq.BreakBy, seq.DiscFor, ref discAmt1, ref discPct1, ref breaklineRef1);
                                            if (discAmt1 != 0)
                                            {
                                                OM_Discount objtmpDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                                if (setup.DiscID02.PassNull() != string.Empty && objtmpDisc != null) break;
                                                return discAmt1;
                                            }
                                        }
                                    }
                                    else if (objDisc.DiscClass == "II" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        OM_DiscItem objtmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.InvtID == det.InvtID);
                                        if (objtmpItem != null)
                                        {
                                            discID1 = objDisc.DiscID;
                                            discSeq1 = seq.DiscSeq;
                                            budgetID1 = seq.BudgetID;
                                            CalculateLineDisc(seq.DiscID, seq.DiscSeq, discItemUnitQty, amt, seq.BreakBy, seq.DiscFor, ref discAmt1, ref discPct1, ref breaklineRef1);
                                            if (discAmt1 != 0)
                                            {
                                                OM_Discount objtmpDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                                if (setup.DiscID02.PassNull() != string.Empty && objtmpDisc != null) break;
                                                return discAmt1;
                                            }
                                        }
                                    }
                                    else if (objDisc.DiscClass == "TT" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        OM_DiscCustClass objCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                        if (objCustClass != null)
                                        {
                                            discID1 = objDisc.DiscID;
                                            discSeq1 = seq.DiscSeq;
                                            budgetID1 = seq.BudgetID;
                                            CalculateLineDisc(seq.DiscID, seq.DiscSeq, qty, amt, seq.BreakBy, seq.DiscFor, ref discAmt1, ref discPct1, ref breaklineRef1);
                                            if (discAmt1 != 0)
                                            {
                                                OM_Discount objtmpDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                                if (setup.DiscID02.PassNull() != string.Empty && objtmpDisc != null) break;
                                                return discAmt1;
                                            }
                                        }
                                    }
                                    else if (objDisc.DiscClass == "PP" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                        OM_DiscItemClass objItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == objInvt.PriceClassID);
                                        if (objItemClass != null)
                                        {
                                            discID1 = objDisc.DiscID;
                                            discSeq1 = seq.DiscSeq;
                                            budgetID1 = seq.BudgetID;
                                            CalculateLineDisc(seq.DiscID, seq.DiscSeq, qty, amt, seq.BreakBy, seq.DiscFor, ref discAmt1, ref discPct1, ref breaklineRef1);
                                            if (discAmt1 != 0)
                                            {
                                                OM_Discount objtmpDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                                if (setup.DiscID02.PassNull() != string.Empty && objtmpDisc != null) break;
                                                return discAmt1;
                                            }
                                        }
                                    }
                                    else if (objDisc.DiscClass == "CI" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);
                                        OM_DiscItem objtmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.InvtID == det.InvtID);
                                        if (objtmpItem != null && objDiscCust != null)
                                        {
                                            discID1 = objDisc.DiscID;
                                            discSeq1 = seq.DiscSeq;
                                            budgetID1 = seq.BudgetID;
                                            CalculateLineDisc(seq.DiscID, seq.DiscSeq, discItemUnitQty, amt, seq.BreakBy, seq.DiscFor, ref discAmt1, ref discPct1, ref breaklineRef1);
                                            if (discAmt1 != 0)
                                            {
                                                OM_Discount objtmpDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                                if (setup.DiscID02.PassNull() != string.Empty && objtmpDisc != null) break;
                                                return discAmt1;
                                            }
                                        }
                                    }
                                    else if (objDisc.DiscClass == "TI" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        OM_DiscCustClass objCustClass = _app.OM_DiscCustClass.FirstOrDefault(p =>
                                                    p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);

                                        OM_DiscItem objtmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID &&
                                                    p.DiscSeq == seq.DiscSeq && p.InvtID == det.InvtID);

                                        if (objtmpItem != null && objCustClass != null)
                                        {
                                            discID1 = objDisc.DiscID;
                                            discSeq1 = seq.DiscSeq;
                                            budgetID1 = seq.BudgetID;
                                            CalculateLineDisc(seq.DiscID, seq.DiscSeq, discItemUnitQty, amt, seq.BreakBy, seq.DiscFor, ref discAmt1, ref discPct1, ref breaklineRef1);
                                            if (discAmt1 != 0)
                                            {
                                                OM_Discount objtmpDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                                if (setup.DiscID02.PassNull() != string.Empty && objtmpDisc != null) break;
                                                return discAmt1;
                                            }
                                        }
                                    }
                                    else if (objDisc.DiscClass == "TP" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        OM_DiscCustClass objCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);

                                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);

                                        OM_DiscItemClass objItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == objInvt.PriceClassID);
                                        if (objItemClass != null && objCustClass != null)
                                        {
                                            discID1 = objDisc.DiscID;
                                            discSeq1 = seq.DiscSeq;
                                            budgetID1 = seq.BudgetID;
                                            CalculateLineDisc(seq.DiscID, seq.DiscSeq, qty, amt, seq.BreakBy, seq.DiscFor, ref discAmt1, ref discPct1, ref breaklineRef1);
                                            if (discAmt1 != 0)
                                            {
                                                OM_Discount objtmpDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                                if (setup.DiscID02.PassNull() != string.Empty && objtmpDisc != null) break;
                                                return discAmt1;
                                            }
                                        }
                                    }
                                } //foreach (var seq in lstSeq)
                            } // if(lstSeq.Count>0)
                        } //objDisc.DiscType=="L"
                        objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                        if (objDisc != null && discID1 != string.Empty)
                        {
                            if (objDisc.DiscType == "L")
                            {
                                List<OM_DiscSeq> lstSeq2 = _app.OM_DiscSeq.Where(p => p.DiscID == objDisc.DiscID && p.Status == "C" && p.Active == 1).ToList();
                                if (lstSeq2.Count > 0)
                                {
                                    foreach (OM_DiscSeq seq2 in lstSeq2)
                                    {
                                        OM_DiscItem objItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.InvtID == det.InvtID);
                                        if (objItem == null) objItem = new OM_DiscItem();
                                        if (seq2.BreakBy == "W")
                                        {
                                            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                            if (objInvt == null) objInvt = new IN_Inventory();
                                            discItemUnitQty = (qty * objInvt.StkWt / OM_GetCnvFactFromUnit(det.InvtID, objItem.UnitDesc).ToInt()).ToInt();
                                        }
                                        else
                                            discItemUnitQty = (qty / OM_GetCnvFactFromUnit(det.InvtID, objItem.UnitDesc).ToInt()).ToInt();

                                        if (seq2.BudgetID.PassNull() != string.Empty && _objOrder.SlsPerID.PassNull() == string.Empty)
                                        {
                                            Util.AppendLog(ref _logMessage, "403", parm: new[] { seq2.DiscSeq, seq2.BudgetID });
                                            break;
                                        }
                                        if (objDisc.DiscClass == "CC" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq2.DiscID, seq2.DiscSeq))))
                                        {
                                            OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.CustID == _objOrder.CustID);
                                            if (objDiscCust != null)
                                            {
                                                discID1 = objDisc.DiscID;
                                                discSeq1 = seq2.DiscSeq;
                                                budgetID1 = seq2.BudgetID;
                                                CalculateLineDisc(seq2.DiscID, seq2.DiscSeq, qty, amt, seq2.BreakBy, seq2.DiscFor, ref discAmt2, ref discPct2, ref breaklineRef2);
                                                if (discAmt2 != 0)
                                                {
                                                    if (setup.Comp == 0)
                                                        return discAmt1 + discAmt2;
                                                    discAmt2 = (amt - discAmt1) * (discAmt2 / amt);
                                                    amt = (amt - discAmt1);
                                                    return Math.Round((discAmt1 + discAmt2), 0);
                                                }
                                                return discAmt1;
                                            }
                                            return discAmt1;
                                        }
                                        if (objDisc.DiscClass == "II" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq2.DiscID, seq2.DiscSeq))))
                                        {
                                            OM_DiscItem objtempItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.InvtID == det.InvtID);
                                            if (objtempItem != null)
                                            {
                                                discID1 = objDisc.DiscID;
                                                discSeq1 = seq2.DiscSeq;
                                                budgetID1 = seq2.BudgetID;
                                                CalculateLineDisc(seq2.DiscID, seq2.DiscSeq, discItemUnitQty, amt, seq2.BreakBy, seq2.DiscFor, ref discAmt2, ref discPct2, ref breaklineRef2);
                                                if (discAmt2 != 0)
                                                {
                                                    if (setup.Comp == 0) return discAmt1 + discAmt2;
                                                    discAmt2 = (amt - discAmt1) * (discAmt2 / amt);
                                                    amt = (amt - discAmt1);
                                                    return Math.Round((discAmt1 + discAmt2), 0);
                                                }
                                                return discAmt1;
                                            }
                                            return discAmt1;
                                        }
                                        if (objDisc.DiscClass == "TT" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq2.DiscID, seq2.DiscSeq))))
                                        {
                                            OM_DiscCustClass objCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                            if (objCustClass != null)
                                            {
                                                discID1 = objDisc.DiscID;
                                                discSeq1 = seq2.DiscSeq;
                                                budgetID1 = seq2.BudgetID;
                                                CalculateLineDisc(seq2.DiscID, seq2.DiscSeq, qty, amt, seq2.BreakBy, seq2.DiscFor, ref discAmt2, ref discPct2, ref breaklineRef2);
                                                if (discAmt2 != 0)
                                                {
                                                    if (setup.Comp == 0) return discAmt1 + discAmt2;
                                                    discAmt2 = (amt - discAmt1) * (discAmt2 / amt);
                                                    amt = (amt - discAmt1);
                                                    return Math.Round((discAmt1 + discAmt2), 0);
                                                }
                                                return discAmt1;
                                            }
                                            return discAmt1;
                                        }
                                        if (objDisc.DiscClass == "PP" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq2.DiscID, seq2.DiscSeq))))
                                        {
                                            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                            OM_DiscItemClass objItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.ClassID == objInvt.PriceClassID);
                                            if (objItemClass != null)
                                            {
                                                discID1 = objDisc.DiscID;
                                                discSeq1 = seq2.DiscSeq;
                                                budgetID1 = seq2.BudgetID;
                                                CalculateLineDisc(seq2.DiscID, seq2.DiscSeq, qty, amt, seq2.BreakBy, seq2.DiscFor, ref discAmt2, ref discPct2, ref breaklineRef2);
                                                if (discAmt2 != 0)
                                                {
                                                    if (setup.Comp == 0) return discAmt1 + discAmt2;
                                                    discAmt2 = (amt - discAmt1) * (discAmt2 / amt);
                                                    amt = (amt - discAmt1);
                                                    return Math.Round((discAmt1 + discAmt2), 0);
                                                }
                                                return discAmt1;
                                            }
                                            return discAmt1;
                                        }
                                        if (objDisc.DiscClass == "CI" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq2.DiscID, seq2.DiscSeq))))
                                        {
                                            OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.CustID == _objOrder.CustID);
                                            OM_DiscItem objtempItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.InvtID == det.InvtID);

                                            if (objtempItem != null && objDiscCust != null)
                                            {
                                                discID1 = objDisc.DiscID;
                                                discSeq1 = seq2.DiscSeq;
                                                budgetID1 = seq2.BudgetID;
                                                CalculateLineDisc(seq2.DiscID, seq2.DiscSeq, discItemUnitQty, amt, seq2.BreakBy, seq2.DiscFor, ref discAmt2, ref discPct2, ref breaklineRef2);
                                                if (discAmt2 != 0)
                                                {
                                                    if (setup.Comp == 0) return discAmt1 + discAmt2;
                                                    discAmt2 = (amt - discAmt1) * (discAmt2 / amt);
                                                    amt = (amt - discAmt1);
                                                    return Math.Round((discAmt1 + discAmt2), 0);
                                                }
                                                return discAmt1;
                                            }
                                            return discAmt1;
                                        }
                                        if (objDisc.DiscClass == "TI" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq2.DiscID, seq2.DiscSeq))))
                                        {
                                            OM_DiscCustClass objCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                            OM_DiscItem objtempItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.InvtID == det.InvtID);
                                            if (objtempItem != null && objCustClass != null)
                                            {
                                                discID1 = objDisc.DiscID;
                                                discSeq1 = seq2.DiscSeq;
                                                budgetID1 = seq2.BudgetID;
                                                CalculateLineDisc(seq2.DiscID, seq2.DiscSeq, discItemUnitQty, amt, seq2.BreakBy, seq2.DiscFor, ref discAmt2, ref discPct2, ref breaklineRef2);
                                                if (discAmt2 != 0)
                                                {
                                                    if (setup.Comp == 0) return discAmt1 + discAmt2;
                                                    discAmt2 = (amt - discAmt1) * (discAmt2 / amt);
                                                    amt = (amt - discAmt1);
                                                    return Math.Round((discAmt1 + discAmt2), 0);
                                                }
                                                return discAmt1;
                                            }
                                            return discAmt1;
                                        }
                                        if (objDisc.DiscClass == "TP" && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.EndDate) <= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq2.StartDate) >= 0 && seq2.Promo != 0) ||
                                                ((_objType.OrderType == "CM" || _objType.OrderType == "CC") && CheckReturnDisc(seq2.DiscID, seq2.DiscSeq))))
                                        {
                                            OM_DiscCustClass objCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                                            OM_DiscItemClass objItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq2.DiscID && p.DiscSeq == seq2.DiscSeq && p.ClassID == objInvt.PriceClassID);

                                            if (objItemClass != null && objCustClass != null)
                                            {
                                                discID1 = objDisc.DiscID;
                                                discSeq1 = seq2.DiscSeq;
                                                budgetID1 = seq2.BudgetID;
                                                CalculateLineDisc(seq2.DiscID, seq2.DiscSeq, qty, amt, seq2.BreakBy, seq2.DiscFor, ref discAmt2, ref discPct2, ref breaklineRef2);
                                                if (discAmt2 != 0)
                                                {
                                                    if (setup.Comp == 0) return discAmt1 + discAmt2;
                                                    discAmt2 = (amt - discAmt1) * (discAmt2 / amt);
                                                    amt = (amt - discAmt1);
                                                    return Math.Round((discAmt1 + discAmt2), 0);
                                                }
                                                return discAmt1;
                                            }
                                            return discAmt1;
                                        }
                                    }
                                }
                            }
                        }
                    } //objDisc!=null
                } //foreach (var setup in lstsetup)
            } //lstsetup.Count>0
            return discAmt1 + discAmt2;
        }

        private double OM_GetCnvFactFromUnit(string invtID, string unitDesc)
        {
            double cnvFact = 1;
            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt != null)
            {
                IN_UnitConversion cnv = _app.IN_UnitConversion.FirstOrDefault(p => p.InvtID == invtID && p.FromUnit == unitDesc && p.ToUnit == objInvt.StkUnit);
                if (cnv != null)
                {
                    if (cnv.MultDiv == "D")
                        cnvFact = 1 / cnv.CnvFact;
                    else
                        cnvFact = cnv.CnvFact;
                }
            }
            return cnvFact;
        }
        private void CalculateLineDisc(string discID, string discSeq, double qty, double amt, string breakBy, string discFor, ref double discAmt1, ref double discPct1, ref string breakLineRef)
        {
            double qtyBreak = 0;
            double qtyAmt = 0;
            double discAmt = 0;
            if (breakBy == "A")
                qtyAmt = amt;
            else
                qtyAmt = qty;
        begin:
            OM_DiscSeq objSeq = _app.OM_DiscSeq.FirstOrDefault(p => p.DiscID == discID && p.DiscSeq == discSeq && p.Status == "C" && p.Active == 1);
            if (objSeq == null) objSeq = new OM_DiscSeq();
            if (Util.PassNull(objSeq.BudgetID) != "" && Util.PassNull(_objOrder.SlsPerID) == string.Empty)
            {
                Util.AppendLog(ref _logMessage, "403", parm: new[] { objSeq.DiscSeq, objSeq.BudgetID });
                return;
            }
            discAmt = GetDiscBreak(discID, discSeq, breakBy, qtyAmt, ref qtyBreak, ref breakLineRef);
            if (discAmt != 0)
            {
                if (discFor == "A")
                {
                    discAmt1 = discAmt1 + discAmt * (qtyAmt / qtyBreak).ToInt();
                    if (qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 && _objOM.ProrateDisc != 0)
                    {
                        qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                        goto begin;
                    }
                    discPct1 = Math.Round((discAmt1 / amt) * 100, 2);
                }
                else if (discFor == "P")
                {
                    discPct1 = discPct1 + discAmt;
                    discAmt1 = Math.Round((discPct1 * amt) / 100, 0);
                }
                else if (discFor == "X")
                    discAmt1 = discAmt1 + discAmt + qty;
            }
        }
        private bool CheckReturnDisc(string discID, string discSeq)
        {
            foreach (var omOrdDisc in _lstDisc)
            {
                if (omOrdDisc.DiscID == discID && omOrdDisc.DiscSeq == discSeq)
                    return true;
            }
            return false;
        }
        private double GetDiscBreak(string discID, string discSeq, string discBreak, double qtyAmt, ref double tmpqtyBreak, ref string lineRef)
        {
            double result = 0;
            double qtyBreak = tmpqtyBreak;
            OM_DiscBreak objBreak;
            if (discBreak == "A")
                objBreak = _app.OM_DiscBreak.Where(p => p.DiscID == discID && p.DiscSeq == discSeq && p.BreakAmt <= qtyAmt && p.BreakAmt > 0).OrderByDescending(p => p.BreakAmt).FirstOrDefault();
            else
                objBreak = _app.OM_DiscBreak.Where(p => p.DiscID == discID && p.DiscSeq == discSeq && p.BreakQty <= qtyAmt && p.BreakQty > 0).OrderByDescending(p => p.BreakQty).FirstOrDefault();

            if (objBreak != null)
            {
                if (discBreak == "A")
                    qtyBreak = objBreak.BreakAmt;
                else
                    qtyBreak = objBreak.BreakQty;

                lineRef = objBreak.LineRef;
                result = objBreak.DiscAmt;
            }
            tmpqtyBreak = qtyBreak;
            return result;
        }
        private void CheckManualDisccountBudget(OM10100_pgOrderDet_Result det)
        {
            if (det.DiscAmt > 0 && det.DiscCode.PassNull() != string.Empty && det.BudgetID1.PassNull() != string.Empty)
            {
                double discAmt = det.DiscAmt;
                string discID = det.DiscCode;
                string budGetID = det.BudgetID1;
                string tmp = string.Empty;
                if (!CheckAvailableDiscBudget(ref budGetID, ref discID, ref tmp, ref discAmt, false, true, det.LineRef, true, true, det.InvtID, det.SlsUnit))
                {
                    det.DiscAmt = 0;
                }
            }
        }
        private void ClearGroupDocBudget()
        {
            int rtrn = _objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1;
            List<OM_OrdDisc> lstDisc = _app.OM_OrdDisc.Where(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr).ToList();
            foreach (OM_OrdDisc disc in lstDisc)
            {
                OM_PPBudget objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == disc.BudgetID);
                if (_objType.ARDocType != "NA" && disc.DiscType != "L" && objBudget != null && objBudget.Active)
                {
                    if (objBudget.ApplyTo == "A")
                    {
                        OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID && p.FreeItemID == ".");

                        objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - disc.DiscAmt * rtrn;
                        objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                        OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID
                            && p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID)
                            && p.FreeItemID == "." && p.CpnyID == _objOrder.BranchID);

                        if (objAlloc != null)
                        {
                            objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - disc.DiscAmt * rtrn;
                            objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                        }
                    }
                    else
                    {
                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == disc.FreeItemID);
                        if (objInvt == null) objInvt = new IN_Inventory();

                        OM_PPFreeItem objPPInvt = _app.OM_PPFreeItem.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == disc.FreeItemID);
                        if (objPPInvt != null)
                        {
                            IN_UnitConversion uomFrom = SetUOM(disc.FreeItemID, objInvt.ClassID, objInvt.StkUnit, disc.DiscUOM);
                            if (uomFrom != null)
                            {
                                IN_UnitConversion uomTo = SetUOM(disc.FreeItemID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                double rate = 1;
                                double rate2 = 1;
                                if (uomFrom.MultDiv == "M")
                                    rate = uomFrom.CnvFact;
                                else
                                    rate = 1 / uomFrom.CnvFact;

                                if (uomTo.MultDiv == "M")
                                    rate2 = uomTo.CnvFact;
                                else
                                    rate2 = 1 / uomTo.CnvFact;

                                rate = Math.Round(rate / rate2, 2);

                                objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent - disc.FreeItemQty * rate * rtrn;
                                objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;

                                OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID && p.FreeItemID == disc.FreeItemID);
                                if (objCpny != null)
                                {
                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - disc.FreeItemQty * rate * rtrn;
                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                }

                                OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID && p.FreeItemID == disc.FreeItemID
                                    && p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                                if (objAlloc != null)
                                {
                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - disc.FreeItemQty * rate * rtrn;
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void ClearFreeItemInGrid()
        {
            for (int i = _lstOrdDet.Count - 1; i >= 0; i--)
            {
                if (_lstOrdDet[i].FreeItem && _lstOrdDet[i].DiscCode.PassNull() == string.Empty && (_lstOrdDet[i].DiscID1.PassNull() != string.Empty || _lstOrdDet[i].DiscID2.PassNull() != string.Empty))
                {
                    Delete_Det(_lstOrdDet[i].LineRef);
                    DelTax(i);
                    foreach (OM_OrdDisc disc in _lstDisc)
                    {
                        if (_lstOrdDet[i].FreeItem && _lstOrdDet[i].DiscID1.PassNull() != string.Empty && disc.FreeItemID == _lstOrdDet[i].InvtID && disc.SOLineRef == _lstOrdDet[i].LineRef)
                        {
                            disc.FreeItemID = disc.FreeItemID + "_D";
                            disc.FreeItemQty = 0;
                            disc.UserOperationLog = "User Deleted Free Item";
                        }
                    }
                    _lstOrdDet.RemoveAt(i);
                }
            }
            CalcTaxTotal();
            CalcDet();
        }
        private void ClearGroupDocDisc()
        {
            List<OM_OrdDisc> lstDiscOld = _app.OM_OrdDisc.Where(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr).ToList();
            foreach (OM_OrdDisc omOrdDisc in lstDiscOld)
            {
                _app.OM_OrdDisc.DeleteObject(omOrdDisc);
            }

            _lstDisc.Clear();
            foreach (OM10100_pgOrderDet_Result det in _lstOrdDet)
            {
                det.GroupDiscID1 = string.Empty;
                det.GroupDiscID2 = string.Empty;
                det.GroupDiscSeq1 = string.Empty;
                det.GroupDiscSeq2 = string.Empty;
                det.GroupDiscAmt1 = 0;
                det.GroupDiscAmt2 = 0;
                det.GroupDiscPct1 = 0;
                det.GroupDiscPct2 = 0;
                det.FreeItemQty1 = 0;
                det.FreeItemQty2 = 0;
                if (det.DiscAmt1 == 0)
                {
                    det.DiscID1 = string.Empty;
                    det.DiscSeq1 = string.Empty;
                }
                if (det.DiscAmt2 == 0)
                {
                    det.DiscID2 = string.Empty;
                    det.DiscSeq2 = string.Empty;
                }
            }
            CalcDet();
        }
        private void CalcDet()
        {
            if (_objType == null || _objType.OrderType == null) return;
            double taxAmt00 = 0;
            double taxAmt01 = 0;
            double taxAmt02 = 0;
            double taxAmt03 = 0;

            double soFee = 0;
            double curyLineDiscAmt = 0;
            double ordQty = 0;


            double curyLineAmt = 0;

            foreach (OM10100_pgOrderDet_Result det in _lstOrdDet)
            {
                taxAmt00 += det.TaxAmt00;
                taxAmt01 += det.TaxAmt01;
                taxAmt02 += det.TaxAmt02;
                taxAmt03 += det.TaxAmt03;
                soFee += det.SOFee;
                curyLineAmt += det.LineAmt;
                curyLineDiscAmt += det.DiscAmt + det.ManuDiscAmt;
                ordQty += det.LineQty;
            }


            _objOrder.SOFeeTot = Math.Round(soFee, 0);
            double curyTaxAmt = Math.Round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0);
            _objOrder.LineDiscAmt = Math.Round(curyLineDiscAmt, 0);
            _objOrder.LineAmt = Math.Round(curyLineAmt, 0);
            double txblAmt = 0;
            if (_objType.DiscType == "B")
                txblAmt = curyLineAmt;
            else
            {
                if (_objType.TaxFee)
                    txblAmt = curyLineAmt - curyTaxAmt.ToDouble() + _objOrder.SOFeeTot * 0.1;
                else
                    txblAmt = curyLineAmt - curyTaxAmt.ToDouble();
            }


            _objOrder.OrdAmt = Math.Round(txblAmt + _objOrder.FreightAmt + _objOrder.MiscAmt + curyTaxAmt + _objOrder.SOFeeTot - _objOrder.VolDiscAmt - _objOrder.OrdDiscAmt, 0);
            _objOrder.OrdQty = Math.Round(ordQty, 0);
        }
        private bool DelTax(int i)
        {
            if (i < 0) return false;
            if (_objOrder.Status == "C" || _objOrder.Status == "L" || _objOrder.Status == "I")
                return false;
            string lineRef = _lstOrdDet[i].LineRef;
            for (int j = _lstTax.Count - 1; j >= 0; j--)
            {
                if (_lstTax[j].LineRef == lineRef)
                    _lstTax.RemoveAt(j);
            }
            ClearTax(i);
            CalcTaxTotal();
            CalcDet();
            return true;
        }
        private void ClearTax(int index)
        {
            _lstOrdDet[index].TaxId00 = string.Empty;
            _lstOrdDet[index].TaxAmt00 = 0;
            _lstOrdDet[index].TxblAmt00 = 0;

            _lstOrdDet[index].TaxId01 = string.Empty;
            _lstOrdDet[index].TaxAmt01 = 0;
            _lstOrdDet[index].TxblAmt01 = 0;

            _lstOrdDet[index].TaxId02 = string.Empty;
            _lstOrdDet[index].TaxAmt02 = 0;
            _lstOrdDet[index].TxblAmt02 = 0;

            _lstOrdDet[index].TaxId03 = string.Empty;
            _lstOrdDet[index].TaxAmt03 = 0;
            _lstOrdDet[index].TxblAmt03 = 0;
        }
        private string LastLineRef()
        {
            int num = 0;

            foreach (OM10100_pgOrderDet_Result det in _lstOrdDet)
            {
                if (det.LineRef.PassNull() != string.Empty && det.LineRef.ToInt() > num)
                    num = det.LineRef.ToInt();
            }
            num++;
            string lineRef = num.ToString();
            int len = lineRef.Length;
            for (int i = 0; i < 5 - len; i++)
            {
                lineRef = "0" + lineRef;
            }
            return lineRef;
        }
        private bool UpdateAllocSO(string invtID, string siteID, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                IN_ItemSite objItemSite = _app.IN_ItemSite.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID);
                if (objItemSite != null)
                {
                    if (!_objIN.NegQty && newQty > 0 && objItemSite.QtyAvail + oldQty - newQty < 0)
                    {
                        //Util.AppendLog(ref _logMessage, "608", parm: new[] { objItemSite.InvtID, objItemSite.SiteID });
                        return false;
                    }
                    objItemSite.QtyAllocSO = Math.Round(objItemSite.QtyAllocSO + newQty - oldQty, decQty);
                    objItemSite.QtyAvail = Math.Round(objItemSite.QtyAvail + oldQty - newQty, decQty);
                }
                return true;
            }
            return false;
        }
        private bool UpdateAllocLotSO(string invtID, string siteID, string lotSerNbr, double oldQty, double newQty, int decQty)
        {
            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
            if (objInvt == null) objInvt = new IN_Inventory();
            if (objInvt.StkItem == 1)
            {
                var objItemLot = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == lotSerNbr);
                if (objItemLot != null)
                {
                    if (!_objIN.NegQty && newQty > 0 && objItemLot.QtyAvail +oldQty - newQty < 0)
                    {
                        //Util.AppendLog(ref _logMessage, "608", parm: new[] { objItemLot.InvtID + " " objItemLot.LotSerNbr , objItemSite.SiteID });
                        return false;
                    }
                    objItemLot.QtyAllocSO = Math.Round(objItemLot.QtyAllocSO + newQty - oldQty, decQty);
                    objItemLot.QtyAvail = Math.Round(objItemLot.QtyAvail + oldQty - newQty, decQty);
                }
                return true;
            }
            return true;
        }
        private IN_UnitConversion SetUOM(string invtID, string classID, string stkUnit, string fromUnit)
        {
            if (!string.IsNullOrEmpty(fromUnit))
            {
                IN_UnitConversion data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "3" && p.ClassID == "*" && p.InvtID == invtID && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "2" && p.ClassID == classID && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                data = _app.IN_UnitConversion.FirstOrDefault(p =>
                        p.UnitType == "1" && p.ClassID == "*" && p.InvtID == "*" && p.FromUnit == fromUnit &&
                        p.ToUnit == stkUnit);
                if (data != null)
                {
                    return data;
                }
                throw new MessageException("2525", new[] { invtID });
                return null;
            }
            return null;
        }
        private int CheckQtyAvail(string invtID, string siteID, string unitMultDiv, double lineQty, double unitRate, bool freeItem, string lineRef)
        {
            double qty1 = 0;
            double qty = 0;
            if (unitMultDiv == "M")
                qty1 = lineQty * unitRate;
            else
                qty1 = lineQty / unitRate;

            qty = CalculateInvtTotals(invtID, siteID, lineRef);

            if (freeItem) qty += qty1;
            IN_ItemSite objItemSite = _app.IN_ItemSite.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID);

            if (objItemSite == null) objItemSite = new IN_ItemSite();

            if (_objType.INDocType != "CM" && _objType.INDocType != "DM" && _objType.INDocType != "NA" && _objType.INDocType != "RC")
            {
                if (qty > objItemSite.QtyAvail)
                    return 1;
                if (qty1 > objItemSite.QtyAvail)
                    return 2;
            }
            return 0;
        }
        private void TotalInvt(string invtID, ref double qty, ref double amt, bool inlcSOFeeDisc, bool isComp = false)
        {
            double soFee = 0;
            qty = 0;
            amt = 0;
            if (!isComp)
            {
                qty =
                    _lstOldOrdDet.Where(
                        p =>
                            p.InvtID == invtID && Util.PassNull(p.GroupDiscID1) == string.Empty && !p.FreeItem &&
                            p.BOType != "R").Sum(p => p.StkQty);
                amt =
                    _lstOldOrdDet.Where(
                        p =>
                            p.InvtID == invtID && Util.PassNull(p.GroupDiscID1) == string.Empty && !p.FreeItem &&
                            p.BOType != "R").Sum(p => p.LineAmt);
                if (inlcSOFeeDisc)
                    soFee =
                        _lstOldOrdDet.Where(
                            p => p.InvtID == invtID && Util.PassNull(p.GroupDiscID1) == string.Empty && p.BOType != "R")
                            .Sum(p => p.SOFee);
                else
                    soFee = 0;
            }
            else
            {
                qty =
                    _lstOldOrdDet.Where(
                        p =>
                            p.InvtID == invtID && Util.PassNull(p.GroupDiscID2) == string.Empty && !p.FreeItem &&
                            p.BOType != "R").Sum(p => p.StkQty);
                amt =
                    _lstOldOrdDet.Where(
                        p =>
                            p.InvtID == invtID && Util.PassNull(p.GroupDiscID2) == string.Empty && !p.FreeItem &&
                            p.BOType != "R").Sum(p => p.LineAmt - p.GroupDiscAmt1);
                if (inlcSOFeeDisc)
                    soFee =
                        _lstOldOrdDet.Where(
                            p => p.InvtID == invtID && Util.PassNull(p.GroupDiscID2) == string.Empty && p.BOType != "R")
                            .Sum(p => p.SOFee);
                else
                    soFee = 0;
            }
            amt = amt + soFee;
        }
        public void TotalItemClass(string classID, ref double qty, ref double amt, bool inlcSOFreeDisc, string uom,
            bool isComp = false)
        {
            double soFee = 0;

            qty = 0;
            amt = 0;
            if (!isComp)
            {
                qty = (from p in _lstOldOrdDet
                       where
                           p.ItemPriceClass == classID && Util.PassNull(p.GroupDiscID1) == string.Empty && !p.FreeItem &&
                           p.BOType != "R" && p.SlsUnit == uom
                       select p).Sum(p => p.StkQty);
                amt = (from p in _lstOldOrdDet
                       where
                           p.ItemPriceClass == classID && Util.PassNull(p.GroupDiscID1) == string.Empty && !p.FreeItem &&
                           p.BOType != "R" && p.SlsUnit == uom
                       select p).Sum(p => p.LineAmt);

                if (inlcSOFreeDisc)
                    soFee = (from p in _lstOldOrdDet
                             where
                                 p.ItemPriceClass == classID && Util.PassNull(p.GroupDiscID1) == string.Empty &&
                                 p.BOType != "R" && p.SlsUnit == uom
                             select p).Sum(p => p.SOFee);
                else
                    soFee = 0;
            }
            else
            {
                qty = (from p in _lstOldOrdDet
                       where
                           p.ItemPriceClass == classID && Util.PassNull(p.GroupDiscID2) == string.Empty && !p.FreeItem &&
                           p.BOType != "R" && p.SlsUnit == uom
                       select p).Sum(p => p.StkQty);
                amt = (from p in _lstOldOrdDet
                       where
                           p.ItemPriceClass == classID && Util.PassNull(p.GroupDiscID2) == string.Empty && !p.FreeItem &&
                           p.BOType != "R" && p.SlsUnit == uom
                       select p).Sum(p => p.LineAmt - p.GroupDiscAmt1);

                if (inlcSOFreeDisc)
                    soFee = (from p in _lstOldOrdDet
                             where
                                 p.ItemPriceClass == classID && Util.PassNull(p.GroupDiscID2) == string.Empty &&
                                 p.BOType != "R" && p.SlsUnit == uom
                             select p).Sum(p => p.SOFee);
                else
                    soFee = 0;
            }
            amt += soFee;
        }
        private void InsertOrdDiscForLineDiscount(ref string discLineRef)
        {
            for (int i = _lstDisc.Count - 1; i >= 0; i--)
            {
                if (_lstDisc[i].DiscType == "L" && _lstDisc[i].DiscAmt != 0)
                {
                    OM_OrdDisc objDisc = _app.OM_OrdDisc.FirstOrDefault(p =>
                        p.BranchID == Current.CpnyID && p.DiscID == _lstDisc[i].DiscID &&
                        p.DiscSeq == _lstDisc[i].DiscSeq && p.OrderNbr == _objOrder.OrderNbr &&
                        p.LineRef == _lstDisc[i].LineRef);
                    if (objDisc != null)
                        _app.OM_OrdDisc.DeleteObject(objDisc);
                    _lstDisc.RemoveAt(i);
                }
            }

            foreach (var det in _lstOrdDet)
            {
                if (det.DiscAmt1 != 0 && Util.PassNull(det.DiscID1) != string.Empty &&
                    Util.PassNull(det.DiscSeq1) != string.Empty)
                {
                    OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == det.DiscID1);
                    if (objDisc == null) objDisc = new OM_Discount();
                    OM_DiscSeq objSeq =
                        _app.OM_DiscSeq.FirstOrDefault(
                            p => p.DiscID == det.DiscID1 && p.DiscSeq == det.DiscSeq1 && p.Status == "C");
                    if (objSeq == null) objSeq = new OM_DiscSeq();
                    InsertUpdateOrdDisc(objSeq.DiscID, objSeq.DiscSeq, objSeq.BudgetID, objDisc.DiscType, objSeq.DiscFor,
                        det.DiscAmt1, det.LineQty * det.SlsPrice, det.LineQty, "", "", 0, discLineRef, det.LineRef,
                        det.OrigOrderNbr.Length >= 5 ? det.OrigOrderNbr.Substring(0, 5) : "");
                    discLineRef = (discLineRef.ToInt() + 1).ToString();
                    for (int l = discLineRef.Length; discLineRef.Length < 5; )
                        discLineRef = "0" + discLineRef;
                }
                if (det.DiscAmt2 != 0 && Util.PassNull(det.DiscID2) != string.Empty &&
                    Util.PassNull(det.DiscSeq2) != string.Empty)
                {
                    OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == det.DiscID2);
                    if (objDisc == null) objDisc = new OM_Discount();
                    OM_DiscSeq objSeq =
                        _app.OM_DiscSeq.FirstOrDefault(
                            p => p.DiscID == det.DiscID2 && p.DiscSeq == det.DiscSeq2 && p.Status == "C");
                    if (objSeq == null) objSeq = new OM_DiscSeq();
                    InsertUpdateOrdDisc(objSeq.DiscID, objSeq.DiscSeq, objSeq.BudgetID, objDisc.DiscType, objSeq.DiscFor,
                        det.DiscAmt2, det.LineQty * det.SlsPrice, det.LineQty, "", "", 0, discLineRef, det.LineRef,
                        det.OrigOrderNbr.Length >= 5 ? det.OrigOrderNbr.Substring(0, 5) : "");
                    discLineRef = (discLineRef.ToInt() + 1).ToString();
                    for (int l = discLineRef.Length; discLineRef.Length < 5; )
                        discLineRef = "0" + discLineRef;
                }
                det.OrigOrderNbr = string.Empty;
            }
        }

        private void UpdateDocDiscAmt()
        {
            double docDiscAmt = _docDiscAmt + _objOrder.OrdDiscAmt;
            double docDiscPct = 0, totDocDiscAmt = 0;
            int lastRowIndex = 0;
            if (_objOrder.LineAmt != 0)
                docDiscPct = Math.Round(docDiscAmt / _objOrder.LineAmt, 4);
            else
                docDiscPct = 0;
            for (int i = 0; i < _lstOrdDet.Count; i++)
            {
                if (!_lstOrdDet[i].FreeItem)
                    lastRowIndex = i;
            }
            for (int i = 0; i < _lstOrdDet.Count; i++)
            {
                if (!_lstOrdDet[i].FreeItem)
                {
                    if (i == lastRowIndex)
                        _lstOrdDet[i].DocDiscAmt = Math.Round(docDiscAmt - totDocDiscAmt, 0);
                    else
                    {
                        _lstOrdDet[i].DocDiscAmt = Math.Round(docDiscPct * _lstOrdDet[i].LineAmt, 0);
                        totDocDiscAmt += _lstOrdDet[i].DocDiscAmt;
                    }
                    DelTax(i);
                    CalcTax(i);
                    CalcTaxTotal();

                }
            }
        }
        private double SlspersonStock(string invtID)
        {
            double? stock = _app.OM10100_pdSalespersonStock(_objOrder.SlsPerID, _objOrder.InvcNote, _objOrder.OrderDate)
                    .FirstOrDefault();
            if (stock != null) return stock.Value;
            return 0;
        }

        #endregion

        #region Data
        private void SaveData(FormCollection data)
        {
            _form = data;

            if (_lstOrdDet == null)
            {
                var detHandler = new StoreDataHandler(data["lstOrdDet"]);
                _lstOrdDet = detHandler.ObjectData<OM10100_pgOrderDet_Result>().Where(p => Util.PassNull(p.LineRef) != string.Empty && Util.PassNull(p.InvtID)!=string.Empty).ToList();
            }

            if (_lstLot == null)
            {
                var lotHandler = new StoreDataHandler(data["lstLot"]);
                _lstLot = lotHandler.ObjectData<OM_LotTrans>().Where(p => Util.PassNull(p.OMLineRef) != string.Empty && Util.PassNull(p.LotSerNbr) != string.Empty && Util.PassNull(p.InvtID) != string.Empty).ToList();
            }

            if (_lstTax == null)
            {
                var taxHandler = new StoreDataHandler(data["lstTax"]);
                _lstTax = taxHandler.ObjectData<OM10100_pgTaxTrans_Result>().ToList();
            }

            if (_lstDisc == null)
            {
                var discHandler = new StoreDataHandler(data["lstDisc"]);
                _lstDisc = discHandler.ObjectData<OM_OrdDisc>().ToList();
            }
           

            _objOrder = data.ConvertToObject<OM10100_pcOrder_Result>(false,new string[]{"DoNotCalDisc","CreditHold"});
            _objOrder.DoNotCalDisc = (data["DoNotCalDisc"].PassNull() != string.Empty ? 1 : 0).ToShort();
            _objOrder.CreditHold = (data["CreditHold"].PassNull() != string.Empty ? 1 : 0).ToBool();

            _lstOldOrdDet = _app.OM10100_pgOrderDet(_objOrder.BranchID, _objOrder.OrderNbr, "%").ToList();

            var access = Session["OM10100"] as AccessRight;
            if (_objOrder.OrderNbr == string.Empty && !access.Update)
                throw new MessageException(MessageType.Message, "728");
            if (_objOrder.OrderNbr != string.Empty && !access.Insert)
                throw new MessageException(MessageType.Message, "728");

            _handle = data["Handle"].PassNull() == string.Empty ? "N" : data["Handle"].PassNull();

            if (_handle != "L")
            {
                var cfgWrkDateChk = _sys.SYS_CloseDateSetUp.FirstOrDefault(p => p.BranchID == _objOrder.BranchID);
                if (cfgWrkDateChk != null && cfgWrkDateChk.WrkDateChk)
                {
                    DateTime tranDate = _objOrder.OrderDate;
                    if (!((DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(-1 * cfgWrkDateChk.WrkLowerDays)) >=
                           0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) <= 0)
                          ||
                          (DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate.AddDays(cfgWrkDateChk.WrkUpperDays)) <=
                           0 && DateTime.Compare(tranDate, cfgWrkDateChk.WrkOpenDate) >= 0)
                          || DateTime.Compare(tranDate, cfgWrkDateChk.WrkAdjDate) == 0))
                    {
                        throw new MessageException(MessageType.Message, "301");
                    }
                }
            }

            if (_handle == "C" || _handle == "V" || _handle == "L")
            {
                DataAccess dal = Util.Dal();
                OMProcess.OM order = new OMProcess.OM(Current.UserName, "OM10100", dal);
                try
                {
                    dal.BeginTrans(IsolationLevel.ReadCommitted);

                    if (_form["Handle"] == "V")
                    {
                        order.OM10100_Cancel(_objOrder.BranchID, _objOrder.OrderNbr, string.Empty);

                        dal.CommitTrans();
                    }
                    else if (_form["Handle"] == "C")
                    {

                        order.OM10100_InvoiceRelease(_objOrder.BranchID, _objOrder.OrderNbr, "R", _objOrder.OrderDate);

                        dal.CommitTrans();
                    }
                    else if (_form["Handle"] == "L")
                    {
                        order.OM10100_InvoiceRelease(_objOrder.BranchID, _objOrder.OrderNbr, "L", _objOrder.OrderDate);

                        dal.CommitTrans();
                    }

                    Util.AppendLog(ref _logMessage, "9999");
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
                finally
                {
                    order = null;
                }
                return;
            }

            _objIN = _app.IN_Setup.FirstOrDefault(p => p.BranchID == _objOrder.BranchID && p.SetupID == "IN");
            if (_objIN == null)
            {
                throw new MessageException(MessageType.Message, "8006");
            }

            _objOM = _app.OM_Setup.FirstOrDefault();
            if (_objOM == null)
            {
                throw new MessageException("8006");
            }


            _objType = _app.OM_OrderType.FirstOrDefault(p => p.OrderType == _objOrder.OrderType);
            if (_objType == null)
            {
                throw new MessageException("8013");
            }

            _objCust = _app.AR_Customer.FirstOrDefault(p => p.CustId == _objOrder.CustID && p.BranchID == _objOrder.BranchID);
            if (_objCust == null)
            {
                throw new MessageException("2015032701", new string[] { _objOrder.CustID });
            }

            _objUser = _app.OM_UserDefault.First(p => p.UserID == Current.UserName && p.DfltBranchID == _objOrder.BranchID);
            if (_objType == null)
            {
                throw new MessageException("8013");
            }

            CheckData_Order();

            

            _discLineRef = "00001";
            _lineRef = string.Empty;
            _lstPrice = _app.OM10100_pdSOPrice(_objOrder.BranchID,_objCust.CustId, "", _objOrder.OrderDate.ToString("yyyy-MM-dd") ).ToList();

            foreach (OM10100_pgOrderDet_Result det in _lstOrdDet)
            {
                double stkQty = 0;
                if (det.UnitMultDiv == "M")
                    stkQty = det.LineQty * (det.UnitRate == 0 ? 1 : det.UnitRate);
                else
                    stkQty = det.LineQty / (det.UnitRate == 0 ? 1 : det.UnitRate);

                CheckSubInLineQty(det, stkQty, string.Empty);
                CheckManualDisccountBudget(det);
            }

            if (!_objOrder.DoNotCalDisc.ToBool() && _objType.SalesType != "PRO" && _objOrder.Status != "C" && _objOrder.Status != "L")
            {
                ClearGroupDocBudget();
                ClearFreeItemInGrid();
                ClearGroupDocDisc();

                _lineRef = LastLineRef();

                FreeItemForLine(ref _lineRef, ref _discLineRef);

                _objOrder.VolDiscAmt = GetDiscGroupSetup(ref _lineRef, ref _discLineRef);

                CalcDet();

                _docDiscAmt = GetDiscDocSetup(ref _lineRef, ref _discLineRef);
                _objOrder.VolDiscAmt = _objOrder.VolDiscAmt + _docDiscAmt;
                _lstOrdDet = _lstOldOrdDet;

                CalcDet();

                for (int i = 0; i < _lstOrdDet.Count; i++)
                {
                    if (_lstOrdDet[i].FreeItem)
                        CalcTax(i);
                }
            }
            UpdateDocDiscAmt();
            InsertOrdDiscForLineDiscount(ref _discLineRef);

            AR_Balances objARBalance = _app.AR_Balances.FirstOrDefault(p => p.CustID == _objOrder.CustID);
            if (objARBalance == null) objARBalance = new AR_Balances();
            if (_objType.ARDocType != "CM" && (_objCust.CrRule == "A" || _objCust.CrRule == "B") &&
                (_objCust.CrLmt - objARBalance.CurrBal) < _objOrder.OrdAmt)
            {
                if (_objOM.CreditChkRule == "W")
                {
                    Util.AppendLog(ref _logMessage, "735");
                }
                else if (_objOM.CreditChkRule == "E")
                {
                    Util.AppendLog(ref _logMessage, "736");
                    _objOrder.CreditHold = true;
                }
            }

            CalcDet();

            Save_Ord();

            _app.SaveChanges();

            if (_handle == "I")
            {
                DataAccess dal = Util.Dal();
                var order = new OMProcess.OM(Current.UserName, "OM10100", dal);
                try
                {
                    dal.BeginTrans(IsolationLevel.ReadCommitted);

                    order.OM10100_PrintInvoice(_objOrder.BranchID, _objOrder.OrderNbr);

                    dal.CommitTrans();

                    Util.AppendLog(ref _logMessage, "9999",data: new {orderNbr = _objOrder.OrderNbr});
                }
                catch (Exception)
                {
                    dal.RollbackTrans();
                    throw;
                }
                finally
                {
                    order = null;
                }
            }
        }

        private void Delete_Det(string lineRef)
        {
            double oldQty = 0;
            OM_SalesOrdDet det = _app.OM_SalesOrdDet.FirstOrDefault(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr && p.LineRef == lineRef);
            if (det != null)
            {
                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == det.InvtID);
                if (objInvt == null) objInvt = new IN_Inventory();
                if (det.BOType != "B" && _objType.INDocType != "CM" && _objType.INDocType != "DM" && _objType.INDocType != "NA" && _objType.INDocType != "RC")
                {
                    oldQty = det.UnitMultDiv == "D" ? det.LineQty / det.UnitRate : det.LineQty * det.UnitRate;
                    UpdateAllocSO(det.InvtID, det.SiteID, oldQty, 0, 0);
                }

                OM_PPBudget objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == det.BudgetID1);
                OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == det.DiscID1);

                if (_objType.ARDocType != "NA" && det.BOType != "R" && objBudget != null && objDisc != null && objDisc.DiscType == "L")
                {

                    if (objBudget.ApplyTo == "A")
                    {
                        OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == "." &&
                                    p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                        if (objAlloc != null)
                        {
                            OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == ".");

                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - det.DiscAmt1 *
                                                  ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1);
                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                            objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.DiscAmt1 *
                                                   ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1);
                            objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                        }
                    }
                    else
                    {
                        OM_PPFreeItem objPPInvt = _app.OM_PPFreeItem.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == det.InvtID);
                        if (objPPInvt != null)
                        {
                            OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == det.InvtID && p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                            if (objAlloc != null)
                            {
                                IN_UnitConversion uomFrom = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit, det.SlsUnit);
                                if (uomFrom != null)
                                {
                                    IN_UnitConversion uomTo = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                    double rate = 1;
                                    double rate2 = 1;

                                    if (uomFrom.MultDiv == "M")
                                        rate = uomFrom.CnvFact;
                                    else
                                        rate = 1 / uomFrom.CnvFact;

                                    if (uomTo.MultDiv == "M")
                                        rate2 = uomTo.CnvFact;
                                    else
                                        rate2 = 1 / uomTo.CnvFact;

                                    rate = Math.Round(rate / rate2, 2);

                                    objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent - det.FreeItemQty1 * rate *
                                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1);
                                    objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;

                                    OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == det.InvtID);
                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - det.FreeItemQty1 * rate *
                                                          ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));

                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.FreeItemQty1 * rate *
                                                           ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1);
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                        }
                    }
                }

                objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == det.BudgetID2);
                objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == det.DiscID2);

                if (_objType.ARDocType != "NA" && det.BOType != "R" && objBudget != null && objDisc != null && objDisc.DiscType == "L")
                {
                    if (objBudget.ApplyTo == "A")
                    {
                        OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == "." &&
                                    p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                        if (objAlloc != null)
                        {
                            OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == ".");

                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - det.DiscAmt2 *
                                                  ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));

                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                            objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.DiscAmt2 *
                                                   ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));

                            objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                        }
                    }
                    else
                    {
                        OM_PPFreeItem objPPInvt = _app.OM_PPFreeItem.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == det.InvtID);
                        if (objPPInvt != null)
                        {
                            OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == det.InvtID &&
                                        p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                            if (objAlloc != null)
                            {
                                IN_UnitConversion uomFrom = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit, det.SlsUnit);
                                if (uomFrom != null)
                                {
                                    IN_UnitConversion uomTo = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                    double rate = 1;
                                    double rate2 = 1;

                                    if (uomFrom.MultDiv == "M")
                                        rate = uomFrom.CnvFact;
                                    else
                                        rate = 1 / uomFrom.CnvFact;

                                    if (uomTo.MultDiv == "M")
                                        rate2 = uomTo.CnvFact;
                                    else
                                        rate2 = 1 / uomTo.CnvFact;

                                    rate = Math.Round(rate / rate2, 2);

                                    objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent - det.FreeItemQty2 * rate *
                                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1);

                                    objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;

                                    OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == det.InvtID);

                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - det.FreeItemQty2 * rate *
                                                          ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));
                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;


                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.FreeItemQty2 * rate *
                                                           ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1);

                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                        }
                    }
                }
                //if(_objType.ARDocType!="NA" && det.BOType!="R" && objBudget!=null &&  objDisc!=null && objDisc.DiscType=="L" )

                objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == det.BudgetID1);
                if (det.DiscCode.PassNull() != string.Empty && _objType.ARDocType != "NA" && det.BOType != "R" && (det.FreeItem || det.DiscAmt > 0) && objBudget != null)
                {
                    if (objBudget.ApplyTo == "A")
                    {
                        OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == "." &&
                                    p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                        if (objAlloc != null)
                        {
                            OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.CpnyID == _objOrder.BranchID && p.BudgetID == objBudget.BudgetID && p.FreeItemID == ".");

                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - det.DiscAmt *
                                                  ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));

                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;

                            objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.DiscAmt *
                                                   ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));

                            objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                        }
                    } //  if(objBudget.ApplyTo=="A")
                    else
                    {
                        OM_PPFreeItem objPPInvt = _app.OM_PPFreeItem.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == det.InvtID);
                        if (objPPInvt != null)
                        {
                            OM_PPAlloc objAlloc = _app.OM_PPAlloc.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID && p.FreeItemID == det.InvtID &&
                                        p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                            if (objAlloc != null)
                            {
                                IN_UnitConversion uomFrom = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit, det.SlsUnit);
                                if (uomFrom != null)
                                {
                                    IN_UnitConversion uomTo = SetUOM(det.InvtID, objInvt.ClassID, objInvt.StkUnit, objPPInvt.UnitDesc);
                                    double rate = 1;
                                    double rate2 = 1;
                                    if (uomFrom.MultDiv == "M")
                                        rate = uomFrom.CnvFact;
                                    else
                                        rate = 1 / uomFrom.CnvFact;

                                    if (uomTo.MultDiv == "M")
                                        rate2 = uomTo.CnvFact;
                                    else
                                        rate2 = 1 / uomTo.CnvFact;

                                    rate = Math.Round(rate / rate2, 2);

                                    objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent - det.LineQty * rate *
                                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1);

                                    objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;

                                    OM_PPCpny objCpny = _app.OM_PPCpny.FirstOrDefault(p => p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID && p.FreeItemID == det.InvtID);

                                    objCpny.QtyAmtSpent = objCpny.QtyAmtSpent - det.LineQty * rate *
                                                          ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));

                                    objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;


                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent - det.LineQty * rate *
                                                           ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC" ? -1 : 1));

                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                        }
                    }
                }
                //if(det.DiscCode.PassNull()!=string.Empty && _objType.ARDocType!="NA" && det.BOType!="R" && (det.FreeItem || det.DiscAmt>0) && objBudget!=null)
                var lots = _app.OM_LotTrans.Where(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr && p.OMLineRef == det.LineRef).ToList();
                foreach (var lot in lots)
                {
                    if (det.BOType != "B" && _objType.INDocType != "CM" && _objType.INDocType != "DM" && _objType.INDocType != "NA" && _objType.INDocType != "RC")
                    {
                        oldQty = Math.Round(lot.UnitMultDiv == "D" ? lot.Qty / lot.CnvFact : lot.Qty * lot.CnvFact, 0); 

                        UpdateAllocLotSO(lot.InvtID, lot.SiteID, lot.LotSerNbr, oldQty, 0, 0);
                    }
                    _app.OM_LotTrans.DeleteObject(lot);
                }
                for (int j = _lstLot.Count - 1; j >= 0; j--)
                {
                    if (_lstLot[j].OMLineRef == det.LineRef)
                    {
                        _lstLot.RemoveAt(j);
                    }
                }
                _app.OM_SalesOrdDet.DeleteObject(det);
            }


        }


        private void Save_Ord()
        {
            var ord = _app.OM_SalesOrd.FirstOrDefault(
                    p => p.OrderNbr == _objOrder.OrderNbr && p.BranchID == _objOrder.BranchID);
            if (ord != null)
            {
                if (_form["tstamp"].ToHex() != ord.tstamp.ToHex())
                {
                    throw new MessageException(MessageType.Message, "2014071002");
                }
                Update_Ord(ord, false);
                Save_Addr(ord);
            }
            else
            {
                _objOrder.OrderNbr =  _app.OMNumbering(_objOrder.BranchID, "OrderNbr", _objOrder.OrderType).FirstOrDefault();
                ord = new OM_SalesOrd();
                Update_Ord(ord, true);
                _app.OM_SalesOrd.AddObject(ord);
                Save_Addr(ord);
            }
        }

        private void Save_Addr(OM_SalesOrd ord)
        {
            OM_OrdAddr addr =
                _app.OM_OrdAddr.FirstOrDefault(p => p.OrderNbr == ord.OrderNbr && p.BranchID == _objOrder.BranchID);
            if (addr != null)
            {
                Update_Addr(addr, ord, false);
            }
            else
            {
                var newAddr = new OM_OrdAddr();
                Update_Addr(newAddr, ord, true);
                _app.OM_OrdAddr.AddObject(newAddr);
            }
            Save_Disc(ord);
        }

        private void Save_Disc(OM_SalesOrd ord)
        {
            foreach (OM_OrdDisc currentDisc in _lstDisc)
            {
                OM_OrdDisc disc = _app.OM_OrdDisc.FirstOrDefault(p =>
                    p.DiscID == currentDisc.DiscID && p.DiscSeq == currentDisc.DiscSeq &&
                    p.BranchID == ord.BranchID && p.OrderNbr == ord.OrderNbr &&
                    p.LineRef == currentDisc.LineRef);
                if (disc != null && disc.EntityState != EntityState.Deleted && disc.EntityState != EntityState.Detached)
                {
                    Update_Disc(disc, currentDisc, ord, false);
                }
                else
                {
                    var newDisc = new OM_OrdDisc();
                    Update_Disc(newDisc, currentDisc, ord, true);
                    _app.OM_OrdDisc.AddObject(newDisc);
                }
            }
            Save_Det(ord);
        }

        private void Save_Det(OM_SalesOrd ord)
        {
            foreach (var currentDet in _lstOrdDet)
            {
                OM_SalesOrdDet det = _app.OM_SalesOrdDet.FirstOrDefault(p => p.BranchID == ord.BranchID && p.OrderNbr == ord.OrderNbr && p.LineRef == currentDet.LineRef);
                if (det != null && det.EntityState != EntityState.Deleted && det.EntityState != EntityState.Detached)
                {
                    Update_Det(det, currentDet, ord, false);
                }
                else
                {
                    det = new OM_SalesOrdDet();
                    Update_Det(det, currentDet, ord, true);
                    _app.OM_SalesOrdDet.AddObject(det);
                }

                Save_Lot(ord, det);
            }
        }

        private bool Save_Lot(OM_SalesOrd ord, OM_SalesOrdDet det)
        {
            var lots = _app.OM_LotTrans.Where(p => p.BranchID == ord.BranchID && p.OrderNbr == ord.OrderNbr).ToList();
            foreach (var item in lots)
            {
                if (item.EntityState == EntityState.Deleted || item.EntityState == EntityState.Detached) continue;
                if (!_lstLot.Any(p => p.OMLineRef == item.OMLineRef && p.LotSerNbr == item.LotSerNbr))
                {
                    var oldQty = item.UnitMultDiv == "D" ? item.Qty / item.CnvFact : item.Qty * item.CnvFact;
                    if (det.BOType != "B" && _objType.INDocType != "CM" && _objType.INDocType != "DM" && _objType.INDocType != "NA" && _objType.INDocType != "RC")
                    {
                        UpdateAllocLotSO(item.InvtID, item.SiteID, item.LotSerNbr, oldQty, 0, 0);
                    }
                    _app.OM_LotTrans.DeleteObject(item);
                }
            }

            var lstLotTmp = _lstLot.Where(p => p.OMLineRef == det.LineRef).ToList();
            foreach (var lotCur in lstLotTmp)
            {
                var lot = _app.OM_LotTrans.FirstOrDefault(p => p.BranchID == ord.BranchID && p.OrderNbr == ord.OrderNbr && p.OMLineRef == lotCur.OMLineRef && p.LotSerNbr == lotCur.LotSerNbr);
                if (lot == null || lot.EntityState == EntityState.Deleted || lot.EntityState == EntityState.Detached)
                {
                    lot = new OM_LotTrans();
                    Update_Lot(lot, lotCur, ord, det, true);
                    _app.OM_LotTrans.AddObject(lot);
                }
                else
                {
                    Update_Lot(lot, lotCur, ord, det, false);
                }
            }
            return true;
        }

        private void Update_Ord(OM_SalesOrd t, bool isNew)
        {
            if (isNew)
            {
                t.ResetET();
                t.NoteId = 0.ToShort();
                t.OrderNbr = _objOrder.OrderNbr;
                t.BranchID = _objOrder.BranchID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = "OM10100";
                t.Crtd_User = Current.UserName;
            }

            t.PriceClassID = Util.PassNull(_objOrder.PriceClassID);
            t.DoNotCalDisc = _objOrder.DoNotCalDisc;
            t.CreditHold = _objOrder.CreditHold;
            t.IssueMethod = Util.PassNull(_objOrder.IssueMethod);
            t.OrigOrderNbr = Util.PassNull(_objOrder.OrigOrderNbr);
            t.ReasonCode = Util.PassNull(_objOrder.ReasonCode);
            t.ARDocDate = _objOrder.ARDocDate;
            t.ARRefNbr = Util.PassNull(_objOrder.ARRefNbr);
            t.InvcNbr = Util.PassNull(_objOrder.InvcNbr);
            t.InvcNote = Util.PassNull(_objOrder.InvcNote);
            t.BudgetID1 = Util.PassNull(_objOrder.BudgetID1);
            t.CmmnPct = _objOrder.CmmnPct;
            t.CustOrderNbr = Util.PassNull(_objOrder.CustOrderNbr);
            t.FreightAllocAmt = _objOrder.FreightAllocAmt;
            t.FreightAmt = _objOrder.FreightAmt;
            t.FreightCost = _objOrder.FreightCost;
            t.LineAmt = _objOrder.LineAmt;
            t.LineDiscAmt = _objOrder.LineDiscAmt;
            t.MiscAmt = _objOrder.MiscAmt;
            t.OrdDiscAmt = _objOrder.OrdDiscAmt;
            t.OrdAmt = _objOrder.OrdAmt;
            t.PmtAmt = _objOrder.PmtAmt;
            t.PremFreightAmt = _objOrder.PremFreightAmt;
            t.VolDiscAmt = _objOrder.VolDiscAmt;
            t.SOFeeTot = _objOrder.SOFeeTot;
            t.PromiseDate = DateTime.Now.ToDateShort();
            t.TaxAmtTot00 = 0;
            t.TxblAmtTot00 = 0;
            t.TaxAmtTot00 = 0;
            t.TxblAmtTot00 = 0;
            t.TaxID00 = "";
            t.TaxAmtTot01 = 0;
            t.TxblAmtTot01 = 0;
            t.TaxAmtTot01 = 0;
            t.TxblAmtTot01 = 0;
            t.TaxID01 = "";
            t.TaxAmtTot02 = 0;
            t.TxblAmtTot02 = 0;
            t.TaxAmtTot02 = 0;
            t.TxblAmtTot02 = 0;
            t.TaxID02 = "";
            t.TaxAmtTot03 = 0;
            t.TxblAmtTot03 = 0;
            t.TaxAmtTot03 = 0;
            t.TxblAmtTot03 = 0;
            t.TaxID03 = "";

            for (int i = 0; i < _lstTaxDoc.Count; i++)
            {
                if (i == 0)
                {
                    t.TaxAmtTot00 = _lstTaxDoc[i].TaxAmt;
                    t.TxblAmtTot00 = _lstTaxDoc[i].TxblAmt;
                    t.TaxID00 = Util.PassNull(_lstTaxDoc[i].TaxID);
                }
                else if (i == 1)
                {
                    t.TaxAmtTot01 = _lstTaxDoc[i].TaxAmt;
                    t.TxblAmtTot01 = _lstTaxDoc[i].TxblAmt;
                    t.TaxID01 = Util.PassNull(_lstTaxDoc[i].TaxID);
                }
                else if (i == 2)
                {
                    t.TaxAmtTot02 = _lstTaxDoc[i].TaxAmt;
                    t.TxblAmtTot02 = _lstTaxDoc[i].TxblAmt;
                    t.TaxID02 = Util.PassNull(_lstTaxDoc[i].TaxID);
                }
                else if (i == 2)
                {
                    t.TaxAmtTot03 = _lstTaxDoc[i].TaxAmt;
                    t.TxblAmtTot03 = _lstTaxDoc[i].TxblAmt;
                    t.TaxID03 = Util.PassNull(_lstTaxDoc[i].TaxID);
                }
            }

            t.CustID = Util.PassNull(_objOrder.CustID);
            t.ExpiryDate = _objOrder.ExpiryDate;
            t.OrderDate = _objOrder.OrderDate;

            t.OrderType = Util.PassNull(_objOrder.OrderType);
            t.OrdQty = _objOrder.OrdQty;
            t.ShipPriority = Util.PassNull(_objOrder.ShipPriority);
            t.ShipViaId = Util.PassNull(_objOrder.ShipViaId);
            t.ShipDate = _objOrder.ShipDate.PassMin();
            t.SlsPerID = Util.PassNull(_objOrder.SlsPerID);
            t.Status = Util.PassNull(_objOrder.Status);
            t.Terms = Util.PassNull(_objOrder.Terms);
            t.ToSiteID = Util.PassNull(_objOrder.ToSiteID);
            t.UnitsShipped = _objOrder.UnitsShipped;
            t.OrderWeight = _objOrder.OrderWeight;
            t.VolDiscPct = _objOrder.VolDiscPct;

            t.PaymentID = Util.PassNull(_objOrder.PaymentID);
            t.PmtDate = _objOrder.PmtDate.PassMin();
            t.PaymentBatNbr = Util.PassNull(_objOrder.PaymentBatNbr);
            t.PaymentNbr = Util.PassNull(_objOrder.PaymentNbr);
            t.IssueNumber = _objOrder.IssueNumber;
            t.OrderNo = _objOrder.OrderNo;
            t.DeliveryID = Util.PassNull(_objOrder.DeliveryID);

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = "OM10100";
            t.LUpd_User = Current.UserName;
        }

        private void Update_Addr(OM_OrdAddr t, OM_SalesOrd ord, bool isNew)
        {
            if (isNew)
            {
                t.OrderClass = string.Empty;
                t.OrderNbr = ord.OrderNbr;
                t.BranchID = ord.BranchID;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = "OM10100";
                t.Crtd_User = Current.UserName;
                t.tstamp = new byte[1];
            }

            t.BillName = Util.PassNull(_form["BillName"]);
            t.BillAttn = Util.PassNull(_form["BillAttn"]);
            t.BillAddrLine1 = Util.PassNull(_form["BillAddrLine1"]);
            t.BillAddrLine2 = Util.PassNull(_form["BillAddrLine2"]);
            t.BillCity = Util.PassNull(_form["BillCity"]);
            t.BillStateID = Util.PassNull(_form["BillStateID"]);
            t.BillCntryID = Util.PassNull(_form["BillCntryID"]);
            t.BillZip = Util.PassNull(_form["BillZip"]);
            t.BillPhone = Util.PassNull(_form["BillPhone"]);
            t.BillFax = Util.PassNull(_form["BillFax"]);

            t.ShipCustID = _objOrder.CustID;
            t.ShiptoID = Util.PassNull(_form["ShiptoID"]);
            t.ShipName = Util.PassNull(_form["ShipName"]);
            t.ShipAttn = Util.PassNull(_form["ShipAttn"]);
            t.ShipAddrLine1 = Util.PassNull(_form["ShipAddrLine1"]);
            t.ShipAddrLine2 = Util.PassNull(_form["ShipAddrLine2"]);
            t.ShipCity = Util.PassNull(_form["ShipCity"]);
            t.ShipStateID = Util.PassNull(_form["ShipStateID"]);
            t.ShipCntryID = Util.PassNull(_form["ShipCntryID"]);
            t.ShipZip = Util.PassNull(_form["ShipZip"]);
            t.ShipPhone = Util.PassNull(_form["ShipPhone"]);
            t.ShipFax = Util.PassNull(_form["ShipFax"]);

            t.CheckNbr = Util.PassNull(_form["CheckNbr"]);
            t.CardNbr = Util.PassNull(_form["CardNbr"]);
            t.CardName = Util.PassNull(_form["CardName"]);
            t.CardExpDate = Util.PassNull(_form["CardExpDate"]) == string.Empty
                ? DateTime.Now
                : _form["CardExpDate"].ToDateTime();
            t.AuthCode = Util.PassNull(_form["AuthCode"]);
            t.TaxRegNbr = Util.PassNull(_form["TaxRegNbr"]);

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = "OM10100";
            t.LUpd_User = Current.UserName;
        }

        private void Update_Disc(OM_OrdDisc t, OM_OrdDisc s, OM_SalesOrd ord, bool isNew)
        {
            if (isNew)
            {
                t.OrderNbr = ord.OrderNbr;
                t.BranchID = ord.BranchID;
                t.LineRef = s.LineRef;
                t.DiscID = s.DiscID;
                t.DiscSeq = s.DiscSeq;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = "OM10100";
                t.Crtd_User = Current.UserName;
                t.tstamp = new byte[1];
            }

            t.BreakBy = Util.PassNull(s.BreakBy);
            t.BudgetID = Util.PassNull(s.BudgetID);
            t.DiscAmt = s.DiscAmt;
            t.DiscFor = Util.PassNull(s.DiscFor);
            t.DisctblAmt = s.DisctblAmt;
            t.DisctblQty = s.DisctblQty;
            t.DiscType = Util.PassNull(s.DiscType);
            t.DiscUOM = Util.PassNull(s.DiscUOM);
            t.FreeItemBudgetID = Util.PassNull(s.FreeItemBudgetID);
            t.FreeItemID = Util.PassNull(s.FreeItemID);
            t.FreeItemQty = s.FreeItemQty;
            t.OrigFreeItemQty = s.OrigFreeItemQty;
            t.SlsPerID = Util.PassNull(s.SlsPerID);
            t.SOLineRef = Util.PassNull(s.SOLineRef);
            t.UserOperationLog = Util.PassNull(s.UserOperationLog);
            t.DiscBreakLineRef = Util.PassNull(s.DiscBreakLineRef);

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = "OM10100";
            t.LUpd_User = Current.UserName;
        }

        private bool Update_Det(OM_SalesOrdDet t, OM10100_pgOrderDet_Result s, OM_SalesOrd ord, bool isNew)
        {
            double oldQty = 0;
            double newQty = 0;
            if (s.BOType != "B" && _objType.INDocType != "CM" && _objType.INDocType != "DM" &&
                _objType.INDocType != "NA" && _objType.INDocType != "RC")
            {
                if (isNew)
                    oldQty = 0;
                else
                    oldQty = t.UnitMultDiv == "D" ? t.LineQty / t.UnitRate : t.LineQty * t.UnitRate;

                newQty = s.UnitMultDiv == "D" ? s.LineQty / s.UnitRate : s.LineQty * s.UnitRate;

                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == t.InvtID);
                if (objInvt == null) objInvt = new IN_Inventory();
                if (objInvt.StkItem != 0)
                {
                    UpdateAllocSO(t.InvtID, t.SiteID, oldQty, 0, 0);
                }
                objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == s.InvtID);
                if (objInvt == null) objInvt = new IN_Inventory();
                if (objInvt.StkItem != 0)
                {
                    if (!UpdateAllocSO(s.InvtID, s.SiteID, 0, newQty, 0))
                    {
                        throw new MessageException("1043",new[] { s.InvtID});
                    }
                }
            }
            //  if(s.BOType!="B" && _objType.INDocType!="CM" && _objType.INDocType!="DM" && _objType.INDocType!="NA" && _objType.INDocType!="RC")
            else if (_objOrder.OrderType == "BL" || _objOrder.OrderType == "OC" ||
                     _objOrder.OrderType == "SR")
            {
                if (isNew)
                    oldQty = 0;
                else
                    oldQty = t.UnitMultDiv == "D" ? t.LineQty / t.UnitRate : t.LineQty * t.UnitRate;

                newQty = s.UnitMultDiv == "D" ? s.LineQty / s.UnitRate : s.LineQty * s.UnitRate;

                if (newQty > oldQty + SlspersonStock(s.InvtID))
                {
                    throw new MessageException(MessageType.Message, "1044",
                        parm: new[] { s.InvtID, _objOrder.SlsPerID });
                }
            }
            //else if(cboOrderType.ToValue()=="BL" || cboOrderType.ToValue()=="OC" || cboOrderType.ToValue()=="SR")

            int rtrn = (_objType.ARDocType == "CM" || _objType.ARDocType == "CC") ? -1 : 1;


            OM_PPBudget objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == s.BudgetID1);
            OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == s.DiscID1);
            if (_objType.ARDocType != "NA" && s.BOType != "R" && objBudget != null && objBudget.Active &&
                objDisc != null && objDisc.DiscType == "L")
            {
                if (objBudget.ApplyTo == "A")
                {
                    OM_PPAlloc objAlloc =
                        _app.OM_PPAlloc.FirstOrDefault(
                            p =>
                                p.BudgetID == objBudget.BudgetID &&
                                p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID) &&
                                p.CpnyID == _objOrder.BranchID && p.FreeItemID == ".");
                    if (objAlloc != null)
                    {
                        double spent = (s.DiscAmt1 - t.DiscAmt1) * rtrn;
                        if (objAlloc.QtyAmtAlloc - (objAlloc.QtyAmtSpent + spent) < 0)
                        {
                            throw new MessageException(MessageType.Message, "753",
                                parm: new[] { s.DiscID1 + "-" + s.DiscSeq1 });
                        }
                        objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                        objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                        OM_PPCpny objCpny =
                            _app.OM_PPCpny.FirstOrDefault(
                                p =>
                                    p.BudgetID == objBudget.BudgetID && p.FreeItemID == "." &&
                                    p.CpnyID == _objOrder.BranchID);
                        if (objCpny != null)
                        {
                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                        }
                    }
                } //  if(objBudget.ApplyTo=="A")
                else
                {
                    if (s.FreeItem)
                    {
                        OM_PPFreeItem objPPInvt =
                            _app.OM_PPFreeItem.FirstOrDefault(
                                p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == s.InvtID);
                        if (objPPInvt != null)
                        {
                            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == s.InvtID);
                            if (objInvt == null) objInvt = new IN_Inventory();
                            IN_UnitConversion uomFrom = SetUOM(s.InvtID, objInvt.ClassID, objInvt.StkUnit, s.SlsUnit);
                            if (uomFrom != null)
                            {
                                IN_UnitConversion uomTo = SetUOM(s.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                    objPPInvt.UnitDesc);
                                double rate = 1;
                                double rate2 = 1;
                                double rate3 = 1;
                                if (uomFrom.MultDiv == "M")
                                    rate = uomFrom.CnvFact;
                                else
                                    rate = 1 / uomFrom.CnvFact;

                                if (uomTo.MultDiv == "M")
                                    rate2 = uomTo.CnvFact;
                                else
                                    rate2 = 1 / uomTo.CnvFact;


                                IN_UnitConversion uomToOld = SetUOM(t.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                    objPPInvt.UnitDesc);

                                if (uomToOld.MultDiv == "M")
                                    rate3 = uomToOld.CnvFact;
                                else
                                    rate3 = 1 / uomToOld.CnvFact;

                                rate3 = Math.Round(rate / rate3, 2);
                                rate = Math.Round(rate / rate2, 2);


                                OM_PPAlloc objAlloc =
                                    _app.OM_PPAlloc.FirstOrDefault(
                                        p =>
                                            p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID &&
                                            p.FreeItemID == s.InvtID &&
                                            p.ObjID ==
                                            (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                                if (objAlloc != null)
                                {
                                    double spent = (s.FreeItemQty1 * rate - t.FreeItemQty1 * rate3) * rtrn;
                                    if (objAlloc.QtyAmtAlloc - (objAlloc.QtyAmtSpent + spent) < 0)
                                    {
                                        throw new MessageException(MessageType.Message, "753",
                                            parm: new[] { s.DiscID1 + "-" + s.DiscSeq1 });
                                    }
                                    objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent + spent;
                                    objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;

                                    OM_PPCpny objCpny =
                                        _app.OM_PPCpny.FirstOrDefault(
                                            p =>
                                                p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID &&
                                                p.FreeItemID == s.InvtID);
                                    if (objCpny != null)
                                    {
                                        objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                        objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                    }

                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                        }
                    }
                }
            }
            //if(_objType.ARDocType!="NA" && s.BOType!="R" && objBudget!=null && objBudget.Active && objDisc!=null && objDisc.DiscType=="L")

            objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == s.BudgetID2);
            objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == s.DiscID2);
            if (_objType.ARDocType != "NA" && s.BOType != "R" && objBudget != null && objBudget.Active &&
                objDisc != null && objDisc.DiscType == "L")
            {
                if (objBudget.ApplyTo == "A")
                {
                    OM_PPAlloc objAlloc =
                        _app.OM_PPAlloc.FirstOrDefault(
                            p =>
                                p.BudgetID == objBudget.BudgetID &&
                                p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID) &&
                                p.CpnyID == _objOrder.BranchID && p.FreeItemID == ".");
                    if (objAlloc != null)
                    {
                        double spent = (s.DiscAmt2 - t.DiscAmt2) * rtrn;
                        if (objAlloc.QtyAmtAlloc - (objAlloc.QtyAmtSpent + spent) < 0)
                        {
                            throw new MessageException(MessageType.Message, "753",
                                parm: new[] { s.DiscID2 + "-" + s.DiscSeq2 });
                        }
                        objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                        objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;

                        OM_PPCpny objCpny =
                            _app.OM_PPCpny.FirstOrDefault(
                                p =>
                                    p.BudgetID == objBudget.BudgetID && p.FreeItemID == "." &&
                                    p.CpnyID == _objOrder.BranchID);
                        if (objCpny != null)
                        {
                            objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                            objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                        }
                    }
                } //  if(objBudget.ApplyTo=="A")
                else
                {
                    if (s.FreeItem)
                    {
                        OM_PPFreeItem objPPInvt =
                            _app.OM_PPFreeItem.FirstOrDefault(
                                p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == s.InvtID);
                        if (objPPInvt != null)
                        {
                            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == s.InvtID);
                            if (objInvt == null) objInvt = new IN_Inventory();
                            IN_UnitConversion uomFrom = SetUOM(s.InvtID, objInvt.ClassID, objInvt.StkUnit, s.SlsUnit);
                            if (uomFrom != null)
                            {
                                IN_UnitConversion uomTo = SetUOM(s.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                    objPPInvt.UnitDesc);
                                double rate = 1;
                                double rate2 = 1;
                                double rate3 = 1;
                                if (uomFrom.MultDiv == "M")
                                    rate = uomFrom.CnvFact;
                                else
                                    rate = 1 / uomFrom.CnvFact;

                                if (uomTo.MultDiv == "M")
                                    rate2 = uomTo.CnvFact;
                                else
                                    rate2 = 1 / uomTo.CnvFact;


                                IN_UnitConversion uomToOld = SetUOM(t.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                    objPPInvt.UnitDesc);

                                if (uomToOld.MultDiv == "M")
                                    rate3 = uomToOld.CnvFact;
                                else
                                    rate3 = 1 / uomToOld.CnvFact;

                                rate3 = Math.Round(rate / rate3, 2);
                                rate = Math.Round(rate / rate2, 2);


                                OM_PPAlloc objAlloc =
                                    _app.OM_PPAlloc.FirstOrDefault(
                                        p =>
                                            p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID &&
                                            p.FreeItemID == s.InvtID &&
                                            p.ObjID ==
                                            (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                                if (objAlloc != null)
                                {
                                    double spent = (s.FreeItemQty2 * rate - t.FreeItemQty2 * rate3) * rtrn;
                                    if (objAlloc.QtyAmtAlloc - (objAlloc.QtyAmtSpent + spent) < 0)
                                    {
                                        throw new MessageException(MessageType.Message, "753",
                                            parm: new[] { s.DiscID2 + "-" + s.DiscSeq2 });
                                    }
                                    objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent + spent;
                                    objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;

                                    OM_PPCpny objCpny =
                                        _app.OM_PPCpny.FirstOrDefault(
                                            p =>
                                                p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID &&
                                                p.FreeItemID == s.InvtID);
                                    if (objCpny != null)
                                    {
                                        objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                        objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                                    }

                                    objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;
                                }
                            }
                        }
                    }
                }
            }
            //if(_objType.ARDocType!="NA" && s.BOType!="R" && objBudget!=null && objBudget.Active && objDisc!=null && objDisc.DiscType=="L")

            objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == s.BudgetID1);
            if (Util.PassNull(s.DiscCode) != string.Empty && (s.FreeItem || s.DiscAmt > 0) &&
                _objType.ARDocType != "NA" && s.BOType != "R" && objBudget != null && objBudget.Active)
            {
                if (objBudget.ApplyTo == "A")
                {
                    //if (_objOrder.SlsPerID != ord.SlsPerID.PassNull() && objBudget.AllocType == "1")
                    //{
                    //    //var objAlloc = (from p in _ebiz.OM_PPAllocs where p.BudgetID == objBudget.BudgetID && p.ObjID == (objBudget.AllocType == "1" ? _currentOrd.SlsPerID : cboCustID.ToValue()) select p).FirstOrDefault();
                    //    //if (objAlloc != null)
                    //    //{
                    //    //    spent = SpentBudget(objBudget.BudgetID, objBudget.AllocType == "1" ? _currentOrd.SlsPerID : "", objBudget.AllocType == "2" ? cboCustID.ToValue() : "", true);
                    //    //    objAlloc.QtyAmtSpent = spent - t.DiscAmt * rtrn;
                    //    //    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAvail - spent + t.DiscAmt * rtrn;
                    //    //}
                    //    //objAlloc = (from p in _ebiz.OM_PPAllocs where p.BudgetID == objBudget.BudgetID && p.ObjID == (objBudget.AllocType == "1" ? cboSlsPerID.ToValue() : cboCustID.ToValue()) select p).FirstOrDefault();
                    //    //if (objAlloc != null)
                    //    //{
                    //    //    spent = SpentBudget(objBudget.BudgetID, objBudget.AllocType == "1" ? cboSlsPerID.ToValue() : "", objBudget.AllocType == "2" ? cboCustID.ToValue() : "", true);
                    //    //    objAlloc.QtyAmtSpent = spent + s.DiscAmt * rtrn - t.DiscAmt * rtrn;
                    //    //    objAlloc.QtyAmtAvail = objAlloc.QtyAmtAvail - spent - s.DiscAmt * rtrn + t.DiscAmt * rtrn;
                    //    //}
                    //}
                    //else
                    //{
                    OM_PPAlloc objAlloc =
                        _app.OM_PPAlloc.FirstOrDefault(
                            p =>
                                p.BudgetID == objBudget.BudgetID &&
                                p.ObjID == (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID) &&
                                p.CpnyID == _objOrder.BranchID && p.FreeItemID == ".");
                    if (objAlloc != null)
                    {
                        double spent = s.DiscAmt * rtrn - t.DiscAmt * rtrn;
                        //spent = SpentBudget(objBudget.BudgetID, objBudget.AllocType == "1" ? cboSlsPerID.ToValue() : "", objBudget.AllocType == "2" ? cboCustID.ToValue() : "", true);
                        objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                        objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;

                        OM_PPCpny objCpny =
                            _app.OM_PPCpny.FirstOrDefault(
                                p =>
                                    p.BudgetID == objBudget.BudgetID && p.FreeItemID == "." &&
                                    p.CpnyID == _objOrder.BranchID);

                        objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                        objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                    }
                    //}
                } // if(objBudget.ApplyTo=="A")
                else
                {
                    OM_PPFreeItem objPPInvt =
                        _app.OM_PPFreeItem.FirstOrDefault(
                            p => p.BudgetID == objBudget.BudgetID && p.FreeItemID == s.InvtID);
                    if (objPPInvt != null)
                    {
                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == s.InvtID);
                        if (objInvt == null) objInvt = new IN_Inventory();
                        IN_UnitConversion uomFrom = SetUOM(s.InvtID, objInvt.ClassID, objInvt.StkUnit, s.SlsUnit);
                        if (uomFrom != null)
                        {
                            IN_UnitConversion uomTo = SetUOM(s.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                objPPInvt.UnitDesc);
                            double rate = 1;
                            double rate2 = 1;
                            double rate3 = 1;
                            if (uomFrom.MultDiv == "M")
                                rate = uomFrom.CnvFact;
                            else
                                rate = 1 / uomFrom.CnvFact;

                            if (uomTo.MultDiv == "M")
                                rate2 = uomTo.CnvFact;
                            else
                                rate2 = 1 / uomTo.CnvFact;


                            IN_UnitConversion uomToOld = SetUOM(t.InvtID, objInvt.ClassID, objInvt.StkUnit,
                                objPPInvt.UnitDesc);

                            if (uomToOld.MultDiv == "M")
                                rate3 = uomToOld.CnvFact;
                            else
                                rate3 = 1 / uomToOld.CnvFact;

                            rate3 = Math.Round(rate / rate3, 2);
                            rate = Math.Round(rate / rate2, 2);


                            OM_PPAlloc objAlloc =
                                _app.OM_PPAlloc.FirstOrDefault(
                                    p =>
                                        p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID &&
                                        p.FreeItemID == s.InvtID &&
                                        p.ObjID ==
                                        (objBudget.AllocType == "1" ? _objOrder.SlsPerID : _objOrder.CustID));
                            if (objAlloc != null)
                            {
                                double spent = (s.LineQty * rate - t.LineQty * rate3) * rtrn;
                                if (objAlloc.QtyAmtAlloc - (objAlloc.QtyAmtSpent + spent) < 0)
                                {
                                    throw new MessageException(MessageType.Message, "753",
                                        parm: new[] { s.DiscID1 + "-" + s.DiscSeq1 });
                                }
                                objPPInvt.QtyAmtSpent = objPPInvt.QtyAmtSpent + spent;
                                objPPInvt.QtyAmtAvail = objPPInvt.QtyAmtAlloc - objPPInvt.QtyAmtSpent;

                                objAlloc.QtyAmtSpent = objAlloc.QtyAmtSpent + spent;
                                objAlloc.QtyAmtAvail = objAlloc.QtyAmtAlloc - objAlloc.QtyAmtSpent;

                                OM_PPCpny objCpny =
                                    _app.OM_PPCpny.FirstOrDefault(
                                        p =>
                                            p.BudgetID == objBudget.BudgetID && p.CpnyID == _objOrder.BranchID &&
                                            p.FreeItemID == s.InvtID);
                                objCpny.QtyAmtSpent = objCpny.QtyAmtSpent + spent;
                                objCpny.QtyAmtAvail = objCpny.QtyAmtAlloc - objCpny.QtyAmtSpent;
                            }
                        }
                    }
                }
            }
            //if(s.DiscCode.PassNull()!=string.Empty && (s.FreeItem||s.DiscAmt>0) && _objType.ARDocType!="NA" && s.BOType!="R" && objBudget!=null && objBudget.Active)


            if (isNew)
            {
                t.ResetET();
                t.OrderNbr = ord.OrderNbr;
                t.BranchID = ord.BranchID;
                t.LineRef = s.LineRef;

                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = "OM10100";
                t.Crtd_User = Current.UserName;
            }

            t.BudgetID1 = Util.PassNull(s.BudgetID1);
            t.BudgetID2 = Util.PassNull(s.BudgetID2);
            t.CostID = Util.PassNull(s.CostID);

            t.ManuDiscAmt = s.ManuDiscAmt;
            t.DiscAmt = s.DiscAmt;
            t.DocDiscAmt = s.DocDiscAmt;
            t.DiscAmt1 = s.DiscAmt1;
            t.DiscAmt2 = s.DiscAmt2;
            t.GroupDiscAmt1 = s.GroupDiscAmt1;
            t.GroupDiscAmt2 = s.GroupDiscAmt2;
            t.LineAmt = s.LineAmt;
            t.SlsPrice = s.SlsPrice;
            t.SOFee = s.SOFee;

            t.Descr = Util.PassNull(s.Descr);
            t.DiscCode = Util.PassNull(s.DiscCode);

            t.DiscPct = s.DiscPct;
            t.DiscPct1 = s.DiscPct1;
            t.DiscPct2 = s.DiscPct2;
            t.DiscID1 = Util.PassNull(s.DiscID1);
            t.DiscID2 = Util.PassNull(s.DiscID2);
            t.DiscSeq1 = Util.PassNull(s.DiscSeq1);
            t.DiscSeq2 = Util.PassNull(s.DiscSeq2);

            t.GroupDiscPct1 = s.GroupDiscPct1;
            t.GroupDiscPct2 = s.GroupDiscPct2;
            t.GroupDiscID1 = Util.PassNull(s.GroupDiscID1);
            t.GroupDiscID2 = Util.PassNull(s.GroupDiscID2);
            t.GroupDiscSeq1 = Util.PassNull(s.GroupDiscSeq1);
            t.GroupDiscSeq2 = Util.PassNull(s.GroupDiscSeq2);

            t.FreeItem = s.FreeItem;
            t.FreeItemQty1 = s.FreeItemQty1;
            t.FreeItemQty2 = s.FreeItemQty2;
            t.InvtID = Util.PassNull(s.InvtID);
            t.BarCode = Util.PassNull(s.BarCode);
            t.ItemPriceClass = s.ItemPriceClass;
            t.LineQty = s.LineQty;
            t.OrderType = _objOrder.OrderType;
            t.QtyInvc = s.QtyInvc;
            t.QtyOpenShip = s.QtyOpenShip;
            t.QtyShip = s.QtyShip;

            t.BOType = Util.PassNull(s.BOType);
            t.QtyBO = s.QtyBO;
            if (t.BOType == "O" && Util.PassNull(s.BOCustID) == string.Empty)
                t.BOCustID = _objOrder.CustID;
            else
                t.BOCustID = Util.PassNull(s.BOCustID);

            t.SiteID = Util.PassNull(s.SiteID);
            t.SlsUnit = Util.PassNull(s.SlsUnit);
            t.ShipStatus = Util.PassNull(s.ShipStatus);
            t.TaxCat = Util.PassNull(s.TaxCat);
            t.TaxID00 = Util.PassNull(s.TaxId00);
            t.TaxID01 = Util.PassNull(s.TaxId01);
            t.TaxID02 = Util.PassNull(s.TaxId02);
            t.TaxID03 = Util.PassNull(s.TaxId03);

            t.TaxAmt00 = s.TaxAmt00;
            t.TaxAmt01 = s.TaxAmt01;
            t.TaxAmt02 = s.TaxAmt02;
            t.TaxAmt03 = s.TaxAmt03;

            t.TxblAmt00 = s.TxblAmt00;
            t.TxblAmt01 = s.TxblAmt01;
            t.TxblAmt02 = s.TxblAmt02;
            t.TxblAmt03 = s.TxblAmt03;

            t.UnitRate = s.UnitRate;
            t.UnitMultDiv = s.UnitMultDiv;
            t.UnitWeight = s.UnitWeight;
            t.OrigOrderNbr = s.OrigOrderNbr;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = "OM10100";
            t.LUpd_User = Current.UserName;
            return true;
        }

        private bool Update_Lot(OM_LotTrans t, OM_LotTrans s, OM_SalesOrd ord, OM_SalesOrdDet det, bool isNew)
        {
         
            if (isNew)
            {
                t.ResetET();
                t.OrderNbr = ord.OrderNbr;
                t.BranchID = ord.BranchID;
                t.OMLineRef = s.OMLineRef;
                t.LotSerNbr = s.LotSerNbr;
                t.UnitDesc = s.UnitDesc;
                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;

                t.WarrantyDate = DateTime.Now.ToDateShort();
            }

            double oldQty = 0;
            double newQty = 0;

            if (det.BOType != "B" && _objType.INDocType != "CM" && _objType.INDocType != "DM" && _objType.INDocType != "NA" && _objType.INDocType != "RC")
            {
                if (!isNew)
                    oldQty = s.UnitMultDiv == "D" ? t.Qty / t.CnvFact : t.Qty * t.CnvFact;
                else
                    oldQty = 0;
                newQty = s.UnitMultDiv == "D" ? s.Qty / s.CnvFact : s.Qty * s.CnvFact;

                UpdateAllocLotSO(t.InvtID, t.SiteID, t.LotSerNbr, oldQty, 0, 0);

                if (!UpdateAllocLotSO(s.InvtID, s.SiteID, t.LotSerNbr, 0, newQty, 0))
                {
                    throw new MessageException("1043", new string[] { s.InvtID + " " + s.LotSerNbr, s.SiteID });
                }

            }

            t.ExpDate = s.ExpDate;
            t.InvtID = s.InvtID;
            t.InvtMult = s.InvtMult;
            t.Qty = s.Qty;

            t.SiteID = s.SiteID;

            t.MfgrLotSerNbr = s.MfgrLotSerNbr.PassNull();
            t.INDocType = _objType.INDocType.PassNull();

            t.TranDate = ord.OrderDate;
            t.CnvFact = s.CnvFact;
            t.UnitCost = s.UnitCost;
            t.UnitPrice = s.UnitPrice;

            t.UnitMultDiv = s.UnitMultDiv;

            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
            t.LUpd_DateTime = DateTime.Now;




            return true;
        }
        #endregion

        #region Tax
        private bool CalcTax(int i)
        {
            if (_objOrder.Status == "C" || _objOrder.Status == "L" || _objOrder.Status == "I")
                return false;
            if (i < 0) return true;
            double groupDocDistAmt = _lstOrdDet[i].DocDiscAmt + _lstOrdDet[i].GroupDiscAmt1 +
                                     _lstOrdDet[i].GroupDiscAmt2;
            var dt = new List<OM10100_pdCustomerTax_Result>();
            List<OM10100_pdCustomerTax_Result> lstTax = _app.OM10100_pdCustomerTax(_objOrder.CustID, _objOrder.ShipViaId, _objOrder.BranchID).ToList();
            if (_lstOrdDet[i].TaxID == "*")
            {
                dt = new List<OM10100_pdCustomerTax_Result>(lstTax);
            }
            else
            {
                string[] strTax = Util.PassNull(_lstOrdDet[i].TaxID).Split(',');
                if (strTax.Length > 0)
                {
                    for (int k = 0; k < strTax.Length; k++)
                    {
                        for (int j = 0; j < lstTax.Count; j++)
                        {
                            if (strTax[k] == lstTax[j].TaxID)
                            {
                                dt.Add(new OM10100_pdCustomerTax_Result
                                {
                                    TaxID = strTax[k],
                                    PrcTaxIncl = lstTax[j].PrcTaxIncl
                                });
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (Util.PassNull(_lstOrdDet[i].TaxID) == string.Empty ||
                        Util.PassNull(_lstOrdDet[i].TaxCat) == string.Empty)
                        _lstOrdDet[i].TxblAmt00 = _lstOrdDet[i].LineAmt - groupDocDistAmt;
                    return false;
                }
            }

            string taxCat = Util.PassNull(_lstOrdDet[i].TaxCat);
            double prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
            for (int j = 0; j < dt.Count; j++)
            {
                OM10100_pdCustomerTax_Result objTax =
                    lstTax.Where(p => Util.PassNull(p.TaxID) == Util.PassNull(dt[j].TaxID)).FirstOrDefault();
                if (objTax != null && taxCat != string.Empty)
                {
                    if (taxCat == "*" ||
                        (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat &&
                         objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat &&
                         objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                        ||
                        (objTax.CatFlg == "N" &&
                         (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                          objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                          objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat)))
                    {
                        if ((_objType.DiscType == "A" && objTax.PrcTaxIncl == "0") ||
                            (_objType.DiscType == "B" && objTax.PrcTaxIncl != "0"))
                        {
                            Util.AppendLog(ref _logMessage, "730");
                            return false;
                        }
                        if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0")
                        {
                            prcTaxInclRate = prcTaxInclRate + objTax.TaxRate;
                        }
                    }
                }
            }
            if (_objType.SalesType == "PET" && !_lstOrdDet[i].FreeItem)
            {
                txblAmtL1 = Math.Round(_lstOrdDet[i].SlsPrice / (1 + prcTaxInclRate / 100), 0) * _lstOrdDet[i].LineQty -
                            _lstOrdDet[i].DiscAmt - _lstOrdDet[i].ManuDiscAmt;
            }
            else
            {
                if (prcTaxInclRate == 0)
                    txblAmtL1 = Math.Round(_lstOrdDet[i].LineAmt - groupDocDistAmt, 0);
                else
                    txblAmtL1 = Math.Round((_lstOrdDet[i].LineAmt - groupDocDistAmt) / (1 + prcTaxInclRate / 100), 0);
            }

            _lstOrdDet[i].TxblAmt00 = txblAmtL1;

            for (int j = 0; j < dt.Count; j++)
            {
                string taxID = string.Empty, lineRef = string.Empty;
                double taxRate = 0, taxAmtL1 = 0;
                OM10100_pdCustomerTax_Result objTax =
                    lstTax.Where(p => Util.PassNull(p.TaxID) == Util.PassNull(dt[j].TaxID)).FirstOrDefault();
                if (objTax != null && taxCat != string.Empty)
                {
                    if (taxCat == "*" ||
                        (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat &&
                         objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat &&
                         objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                        ||
                        (objTax.CatFlg == "N" &&
                         (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                          objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                          objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat)))
                    {
                        if (objTax.TaxCalcLvl == "1")
                        {
                            taxID = dt[j].TaxID;
                            lineRef = _lstOrdDet[i].LineRef;
                            taxRate = objTax.TaxRate;
                            taxAmtL1 = Math.Round(txblAmtL1 * objTax.TaxRate / 100, 0);
                            if (objTax.Lvl2Exmpt == 0)
                                txblAmtAddL2 += txblAmtL1;
                            if (objTax.PrcTaxIncl != "0" && _objType.SalesType != "PET")
                            {
                                bool chk = false;
                                if (j < dt.Count - 1)
                                {
                                    for (int k = j + 1; k < dt.Count; k++)
                                    {
                                        objTax = dt[k];
                                        if (objTax != null && taxCat != string.Empty)
                                        {
                                            if (taxCat == "*" ||
                                                (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat &&
                                                 objTax.CatExcept01 != taxCat && objTax.CatExcept02 != taxCat &&
                                                 objTax.CatExcept03 != taxCat && objTax.CatExcept04 != taxCat &&
                                                 objTax.CatExcept05 != taxCat)
                                                ||
                                                (objTax.CatFlg == "N" &&
                                                 (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                  objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                  objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat)))
                                            {
                                                if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0")
                                                {
                                                    chk = false;
                                                    break;
                                                }
                                            }
                                        }
                                        chk = true;
                                    }
                                }
                                else
                                {
                                    chk = true;
                                }

                                if (chk)
                                {
                                    if (_objType.TaxFee)
                                    {
                                        if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 -
                                            (_lstOrdDet[i].SOFee * taxRate / 100) != _lstOrdDet[i].LineAmt)
                                            taxAmtL1 =
                                                Math.Round(
                                                    _lstOrdDet[i].LineAmt + (_lstOrdDet[i].SOFee * taxRate / 100) -
                                                    groupDocDistAmt - (totPrcTaxInclAmt + txblAmtL1), 0);
                                    }
                                    else
                                    {
                                        if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 != _lstOrdDet[i].LineAmt)
                                            taxAmtL1 =
                                                Math.Round(
                                                    _lstOrdDet[i].LineAmt - groupDocDistAmt -
                                                    (totPrcTaxInclAmt + txblAmtL1), 0);
                                    }
                                }
                                else
                                    totPrcTaxInclAmt += totPrcTaxInclAmt + taxAmtL1;
                            }

                            if (_objType.TaxFee)
                                InsertUpdateTax(taxID, lineRef, taxRate, taxAmtL1, txblAmtL1 + _lstOrdDet[i].SOFee,
                                    1);
                            else
                                InsertUpdateTax(taxID, lineRef, taxRate, taxAmtL1, txblAmtL1, 1);
                        }
                    }
                }
            }

            for (int j = 0; j < dt.Count; j++)
            {
                string taxID = string.Empty, lineRef = string.Empty;
                double taxRate = 0, txblAmtL2 = 0, taxAmtL2 = 0;
                OM10100_pdCustomerTax_Result objTax =
                    lstTax.Where(p => Util.PassNull(p.TaxID) == Util.PassNull(dt[j].TaxID)).FirstOrDefault();
                if (objTax != null && taxCat != string.Empty)
                {
                    if (taxCat == "*" ||
                        (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat &&
                         objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat &&
                         objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                        ||
                        (objTax.CatFlg == "N" &&
                         (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                          objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                          objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat)))
                    {
                        if (objTax.TaxCalcLvl == "2")
                        {
                            taxID = dt[j].TaxID;
                            lineRef = _lstOrdDet[i].LineRef;
                            taxRate = objTax.TaxRate;
                            txblAmtL2 = Math.Round(txblAmtAddL2 + txblAmtL1, 0);
                            taxAmtL2 = Math.Round(txblAmtAddL2 * objTax.TaxRate / 100, 0);
                            InsertUpdateTax(taxID, lineRef, taxRate, taxAmtL2, txblAmtL2, 2);
                        }
                    }
                }
            }
            UpdateTax(i);
            CalcDet();
            return true;
        }

        private void UpdateTax(int i)
        {
            if (i < 0) return;
            int j = 0;
            foreach (OM10100_pgTaxTrans_Result tax in _lstTax)
            {
                if (tax.LineRef == _lstOrdDet[i].LineRef)
                {
                    if (j == 0)
                    {
                        _lstOrdDet[i].TaxId00 = tax.TaxID;
                        _lstOrdDet[i].TxblAmt00 = tax.TxblAmt;
                        _lstOrdDet[i].TaxAmt00 = tax.TaxAmt;
                    }
                    else if (j == 1)
                    {
                        _lstOrdDet[i].TaxId01 = tax.TaxID;
                        _lstOrdDet[i].TxblAmt01 = tax.TxblAmt;
                        _lstOrdDet[i].TaxAmt01 = tax.TaxAmt;
                    }
                    else if (j == 2)
                    {
                        _lstOrdDet[i].TaxId02 = tax.TaxID;
                        _lstOrdDet[i].TxblAmt02 = tax.TxblAmt;
                        _lstOrdDet[i].TaxAmt02 = tax.TaxAmt;
                    }
                    else if (j == 3)
                    {
                        _lstOrdDet[i].TaxId03 = tax.TaxID;
                        _lstOrdDet[i].TxblAmt03 = tax.TxblAmt;
                        _lstOrdDet[i].TaxAmt03 = tax.TaxAmt;
                    }
                    j++;
                }
                if (j != 0 && tax.LineRef != _lstOrdDet[i].LineRef)
                    break;
            }
        }

        private void InsertUpdateTax(string taxID, string lineRef, double taxRate, double taxAmt, double txblAmt,
            int taxLevel)
        {
            bool flat = false;
            for (int i = 0; i < _lstTax.Count; i++)
            {
                if (_lstTax[i].TaxID == taxID && _lstTax[i].LineRef == lineRef)
                {
                    OM10100_pgTaxTrans_Result tax = _lstTax[i];
                    tax.OrderNbr = _objOrder.OrderNbr;
                    tax.BranchID = _objOrder.BranchID;
                    tax.TaxID = taxID;
                    tax.LineRef = lineRef;
                    tax.TaxRate = taxRate;
                    tax.TaxLevel = taxLevel.ToString();
                    tax.TaxAmt = taxAmt;
                    tax.TxblAmt = txblAmt;
                    flat = true;
                    break;
                }
            }
            if (!flat)
            {
                _lstTax.Add(new OM10100_pgTaxTrans_Result
                {
                    BranchID = _objOrder.BranchID,
                    OrderNbr = _objOrder.OrderNbr,
                    TaxID = taxID,
                    LineRef = lineRef,
                    TaxRate = taxRate,
                    TaxLevel = taxLevel.ToString(),
                    TaxAmt = taxAmt,
                    TxblAmt = txblAmt
                });
            }

            CalcDet();
        }

        private void CalcTaxTotal()
        {
            bool flat;
            _lstTaxDoc = new List<OM10100_pgTaxTrans_Result>();
            foreach (var tran in _lstTax)
            {
                flat = true;
                foreach (var total in _lstTaxDoc)
                {
                    if (total.BranchID == tran.BranchID && total.OrderNbr == tran.OrderNbr && total.TaxID == tran.TaxID)
                    {
                        total.TxblAmt += tran.TxblAmt;
                        total.TaxAmt += tran.TaxAmt;
                        flat = false;
                        break;
                    }
                }
                if (flat)
                {
                    _lstTaxDoc.Add(new OM10100_pgTaxTrans_Result
                    {
                        BranchID = tran.BranchID,
                        OrderNbr = tran.OrderNbr,
                        TaxID = tran.TaxID,
                        TaxAmt = tran.TaxAmt,
                        TaxRate = tran.TaxRate,
                        TxblAmt = tran.TxblAmt
                    });
                }
            }
        }

        #endregion

        #region Discount
        private void UpdateGroupDiscByInvtID(string invtID, double discPct, double discAmt, string discID,
            string discSeq, bool isC2, double amtTot, ref double iterateAmt, bool isLastItem)
        {
            foreach (OM10100_pgOrderDet_Result det in _lstOldOrdDet)
            {
                if (amtTot != 0 && !det.FreeItem && det.InvtID == invtID)
                {
                    if (isC2 && Util.PassNull(det.GroupDiscID2) == string.Empty)
                    {
                        det.GroupDiscID2 = discID;
                        det.GroupDiscSeq2 = discSeq;
                        det.GroupDiscPct2 = Math.Round(discPct, 2);
                        det.GroupDiscAmt2 = isLastItem
                            ? discAmt - iterateAmt
                            : Math.Round((discAmt * det.LineAmt) / amtTot, 0);
                        iterateAmt += det.GroupDiscAmt2;
                    }
                    else if (Util.PassNull(det.GroupDiscID1) == string.Empty)
                    {
                        det.GroupDiscID1 = discID;
                        det.GroupDiscSeq1 = discSeq;
                        det.GroupDiscPct1 = Math.Round(discPct, 2);
                        det.GroupDiscAmt1 = isLastItem
                            ? discAmt - iterateAmt
                            : Math.Round((discAmt * det.LineAmt) / amtTot, 0);
                    }
                }
            }
        }

        private void UpdateGroupDiscByPriceClass(string classID, double discPct, double discAmt, string discID,
            string discSeq, bool isC2, double amtTot, ref double iteratedAmt, bool isLastItem)
        {
            int j = 0;
            int i = 0;
            foreach (OM10100_pgOrderDet_Result det in _lstOldOrdDet)
            {
                if (amtTot != 0 && !det.FreeItem && det.ItemPriceClass == classID)
                {
                    if (isC2 && Util.PassNull(det.GroupDiscID2) == string.Empty)
                    {
                        det.GroupDiscID2 = discID;
                        det.GroupDiscSeq2 = discSeq;
                        det.GroupDiscPct2 = Math.Round(discPct, 2);
                        det.GroupDiscAmt2 = Math.Round((discAmt * det.LineAmt) / amtTot, 0);
                        iteratedAmt += det.GroupDiscAmt2;
                    }
                    else if (Util.PassNull(det.GroupDiscID1) == string.Empty)
                    {
                        det.GroupDiscID1 = discID;
                        det.GroupDiscSeq1 = discSeq;
                        det.GroupDiscPct1 = Math.Round(discPct, 2);
                        det.GroupDiscAmt1 = Math.Round((discAmt * det.LineAmt) / amtTot, 0);
                        iteratedAmt += det.GroupDiscAmt1;
                    }
                    j = i;
                }
                i++;
            }
            if (j > 0 && isLastItem)
            {
                if (isC2 && Util.PassNull(_lstOldOrdDet[j].GroupDiscID2) == string.Empty)
                {
                    _lstOldOrdDet[j].GroupDiscID2 = discID;
                    _lstOldOrdDet[j].GroupDiscSeq2 = discSeq;
                    _lstOldOrdDet[j].GroupDiscPct2 = Math.Round(discPct, 2);
                    _lstOldOrdDet[j].GroupDiscAmt2 = discAmt - iteratedAmt - _lstOldOrdDet[j].GroupDiscAmt2;
                    iteratedAmt += _lstOldOrdDet[j].GroupDiscAmt2;
                }
                else if (Util.PassNull(_lstOldOrdDet[j].GroupDiscID1) == string.Empty)
                {
                    _lstOldOrdDet[j].GroupDiscID1 = discID;
                    _lstOldOrdDet[j].GroupDiscSeq1 = discSeq;
                    _lstOldOrdDet[j].GroupDiscPct1 = Math.Round(discPct, 2);
                    _lstOldOrdDet[j].GroupDiscAmt1 = discAmt - iteratedAmt - _lstOldOrdDet[j].GroupDiscAmt1;
                    iteratedAmt += _lstOldOrdDet[j].GroupDiscAmt1;
                }
            }
        }
        private void InsertUpdateOrdDisc(string discID, string discSeq, string budgetID, string discType, string discFor,
         double discAmt, double amt, double qty, string freeItemBudgetID, string freeItemID, double freeItemQty,
         string lineRef, string soLineRef, string discBreakLineRef)
        {
            OM_DiscSeq objSeq = _app.OM_DiscSeq.FirstOrDefault(p => p.DiscID == discID && p.DiscSeq == discSeq && p.Status == "C" && p.Active == 1);
            if (objSeq == null) objSeq = new OM_DiscSeq();
            var newDisc = new OM_OrdDisc();
            newDisc.BranchID = _objOrder.BranchID;
            newDisc.OrderNbr = _objOrder.OrderNbr;
            newDisc.DiscID = discID;
            newDisc.DiscSeq = discSeq;
            newDisc.LineRef = lineRef;
            newDisc.DiscBreakLineRef = discBreakLineRef;
            newDisc.BreakBy = objSeq.BreakBy;
            newDisc.BudgetID = budgetID;
            newDisc.DiscAmt = discAmt;
            newDisc.DisctblAmt = amt;
            newDisc.DiscFor = discFor;
            newDisc.DisctblQty = qty;
            newDisc.DiscType = discType;
            newDisc.DiscUOM = string.Empty;
            newDisc.FreeItemBudgetID = freeItemBudgetID;
            newDisc.FreeItemID = freeItemID;
            newDisc.FreeItemQty = freeItemQty;
            newDisc.SlsPerID = _objOrder.SlsPerID;
            newDisc.OrigFreeItemQty = freeItemQty;
            newDisc.SOLineRef = soLineRef;
            newDisc.UserOperationLog = "Promotion is automatic inserted by system";
            newDisc.Crtd_DateTime = newDisc.LUpd_DateTime = DateTime.Now;
            newDisc.Crtd_Prog = newDisc.LUpd_Prog = "OM10100";
            newDisc.Crtd_User = newDisc.LUpd_User = Current.UserName;
            newDisc.tstamp = new byte[1];
            _lstDisc.Add(newDisc);
        }
        private void UpdateGroupDiscBundleByInvtID(string invtID, double discPct, double discAmt, string discID,
            string discSeq, bool isC2, ref double iterateAmt, bool isLastItem, double bndAmtQty, double bndTotAmtQty)
        {
            foreach (OM10100_pgOrderDet_Result det in _lstOldOrdDet)
            {
                if (bndTotAmtQty != 0 && !det.FreeItem && det.InvtID == invtID)
                {
                    if (isC2 && Util.PassNull(det.GroupDiscID2) == string.Empty)
                    {
                        det.GroupDiscID2 = discID;
                        det.GroupDiscSeq2 = discSeq;
                        det.GroupDiscPct2 = Math.Round(discPct, 2);
                        det.GroupDiscAmt2 = isLastItem
                            ? discAmt - iterateAmt
                            : Math.Round(discAmt * bndAmtQty / bndTotAmtQty, 0);
                        iterateAmt += det.GroupDiscAmt2;
                    }
                    else if (Util.PassNull(det.GroupDiscID1) == string.Empty)
                    {
                        det.GroupDiscID1 = discID;
                        det.GroupDiscSeq1 = discSeq;
                        det.GroupDiscPct1 = Math.Round(discPct, 2);
                        det.GroupDiscAmt1 = isLastItem
                            ? discAmt - iterateAmt
                            : Math.Round(discAmt * bndAmtQty / bndTotAmtQty, 0);
                        iterateAmt += det.GroupDiscAmt1;
                    }
                }
            }
        }
        private void GetBundleFreeItemBreak(string discID, string discSeq, double bundleNbr, ref double bundleNbrBreak,
            ref string lineRef)
        {
            OM_DiscBreak data =
                _app.OM_DiscBreak.Where(
                    p => p.DiscID == discID && p.DiscSeq == discSeq && p.BreakAmt <= bundleNbr && p.BreakAmt > 0)
                    .OrderByDescending(p => p.BreakAmt)
                    .FirstOrDefault();
            if (data != null)
            {
                lineRef = data.LineRef;
                bundleNbrBreak = data.BreakAmt;
            }
        }
        private double GetBundleDiscBreak(string discID, string discSeq, double bundleNbr, ref double bundleNbrBreak)
        {
            OM_DiscBreak data =
                _app.OM_DiscBreak.Where(
                    p => p.DiscSeq == discSeq && p.DiscID == discID && p.BreakAmt <= bundleNbr && p.BreakAmt > 0)
                    .OrderByDescending(p => p.BreakAmt)
                    .FirstOrDefault();
            if (data != null)
            {
                bundleNbrBreak = data.BreakAmt;
                return data.DiscAmt;
            }
            return 0;
        }
        private void CalculateGroupDisc(bool comp, bool isC2, string discID, string discSeq, string discType, double qty,
            double amt, string breakBy, string discFor, List<OM_DiscItem> item, List<OM_DiscItemClass> itemClass,
            ref string lineRef, ref string discLineRef, ref double discAmt1, ref double discAmt2)
        {
            double discAmt = 0,
                discAmtCal = 0,
                discPct = 0,
                freeItemQty = 0,
                freeItemQtyCal = 0,
                qtyBreak = 0,
                qtyAmt = 0,
                cnvFact = 0,
                iteratedAmt = 0;
            string siteID = string.Empty,
                uom = string.Empty,
                unitMultDiv = string.Empty,
                freeItemID = string.Empty,
                budgetID = string.Empty,
                breakLineRef = string.Empty;

            OM_DiscSeq objSeq =
                _app.OM_DiscSeq.FirstOrDefault(p => p.DiscID == discID && p.DiscSeq == discSeq && p.Status == "C");
            if (objSeq == null) objSeq = new OM_DiscSeq();
            budgetID = Util.PassNull(objSeq.BudgetID);
            OM_PPBudget objBudget = _app.OM_PPBudget.FirstOrDefault(p => p.BudgetID == budgetID);
            if (Util.PassNull(objSeq.BudgetID) != string.Empty && objBudget != null && objBudget.AllocType == "1" &&
                Util.PassNull(_objOrder.SlsPerID) != string.Empty)
            {
                Util.AppendLog(ref _logMessage, "403", parm: new[] { objSeq.DiscSeq, objSeq.BudgetID });
                return;
            }
            if (breakBy == "A")
                qtyAmt = amt;
            else
                qtyAmt = qty;

        Begin:
            bool beginCalc = false;
            discAmtCal = GetDiscBreak(discID, discSeq, breakBy, qtyAmt, ref qtyBreak, ref breakLineRef);
            if (discAmtCal > 0 || discAmt > 0)
            {
                if (discFor == "P")
                {
                    discAmt = Math.Round(discAmtCal * amt / 100, 0);
                    discPct = discAmtCal;
                }
                else
                {
                    if (discAmtCal > 0)
                    {
                        if (qtyBreak == 1)
                            discAmt = Math.Round(discAmt + discAmtCal, 0);
                        else
                            discAmt = Math.Round(discAmt + discAmtCal * (qtyAmt / qtyBreak).ToInt(), 0);

                        if (qtyBreak != 1 && qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 &&
                            _objOM.ProrateDisc != 0)
                        {
                            qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                            goto Begin;
                        }
                    }
                }

                if (discAmt > 0)
                {
                    if (isC2)
                        discAmt2 += discAmt;
                    else
                        discAmt1 += discAmt;

                    if (
                        !CheckAvailableDiscBudget(ref budgetID, ref discID, ref discSeq, ref discAmt, false, false,
                            string.Empty, true, false, freeItemID, string.Empty))
                    {
                        discAmt = 0;
                        discAmt1 = 0;
                        discAmt2 = 0;
                    }
                    if (item == null)
                    {
                        for (int k = 0; k < itemClass.Count; k++)
                            UpdateGroupDiscByPriceClass(itemClass[k].ClassID, discPct, discAmt, discID, discSeq,
                                isC2, amt, ref iteratedAmt,
                                k == itemClass.Count - 1 ? true : false);
                    }
                    else
                    {
                        for (int k = 0; k < item.Count; k++)
                            UpdateGroupDiscByInvtID(item[k].InvtID, discPct, discAmt, discID, discSeq,
                                isC2, amt, ref iteratedAmt,
                                k == item.Count - 1 ? true : false);
                    }

                    if (discAmt > 0)
                    {
                        InsertUpdateOrdDisc(discID, discSeq, budgetID, discType, discFor, discAmt, amt, qty,
                            budgetID, freeItemID, freeItemQty, discLineRef, string.Empty,
                            breakLineRef);
                        discLineRef = (discLineRef.ToInt() + 1).ToString();
                        for (int l = discLineRef.Length; discLineRef.Length < 5; )
                            discLineRef = "0" + discLineRef;
                    }
                } // if(discAmt>0) 
            } // if(discAmtCal>0 || discAmt>0)

            qtyBreak = 0;
            breakLineRef = string.Empty;
            GetDiscBreak(discID, discSeq, breakBy, qtyAmt, ref qtyBreak, ref breakLineRef);
            var lstFreeItem = _app.OM_DiscFreeItem.Where(p =>
                p.DiscID == objSeq.DiscID && p.DiscSeq == objSeq.DiscSeq &&
                p.LineRef == breakLineRef)
                .Select(p => new
                {
                    Sel = true,
                    p.LineRef,
                    p.DiscID,
                    p.DiscSeq,
                    p.FreeITemSiteID,
                    p.FreeItemBudgetID,
                    p.FreeItemID,
                    p.FreeItemQty,
                    p.UnitDescr
                }).ToList();
            if (lstFreeItem.Count > 0)
            {
                if (lstFreeItem.Count > 1 && !objSeq.AutoFreeItem &&
                    !(objSeq.DiscClass == "II" && objSeq.ProAplForItem == "M"))
                {
                }
                int countRow = 0;
                foreach (var free in lstFreeItem)
                {
                    countRow++;
                    budgetID = free.FreeItemBudgetID;
                    freeItemQtyCal = free.FreeItemQty;
                    if (!beginCalc) freeItemQty = 0;
                    if (discAmtCal > 0 || freeItemQtyCal > 0)
                    {
                        freeItemQty = Math.Round(freeItemQty + freeItemQtyCal * (qtyAmt / qtyBreak).ToInt(), 0);
                        if (countRow == lstFreeItem.Count && qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 &&
                            _objOM.ProrateDisc != 0)
                        {
                            qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                            beginCalc = true;
                        }
                    } // if(discAmtCal>0 || freeItemQtyCal>0)

                    if (freeItemQty > 0)
                    {
                        if (
                            !CheckAvailableDiscBudget(ref budgetID, ref discID, ref discSeq, ref freeItemQty, true,
                                false, string.Empty, true, false, free.FreeItemID, free.UnitDescr))
                        {
                            discAmt = 0;
                            discAmt1 = 0;
                            discAmt = 0;
                            freeItemQty = 0;
                        }
                    } // if(freeItemQty>0)

                    if (Util.PassNull(free.FreeItemID) != string.Empty)
                    {
                        if (Util.PassNull(_objUser.DiscSite) != string.Empty)
                            siteID = _objUser.DiscSite;
                        else
                            siteID = free.FreeITemSiteID;

                        uom = free.UnitDescr;
                        freeItemID = free.FreeItemID;
                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID);
                        if (objInvt == null) objInvt = new IN_Inventory();

                        IN_UnitConversion cnv = SetUOM(freeItemID, objInvt.ClassID, objInvt.StkUnit, uom);
                        if (cnv != null)
                        {
                            cnvFact = cnv.CnvFact;
                            unitMultDiv = cnv.MultDiv;
                        }
                        if (CheckQtyAvail(freeItemID, siteID, unitMultDiv, freeItemQty, cnvFact, true, string.Empty) ==
                            1)
                        {
                            Util.AppendLog(ref _logMessage, "1045", parm: new[] { freeItemID, siteID });
                            freeItemID = string.Empty;
                            freeItemQty = 0;
                        }
                        else
                        {
                            AddFreeItem(discID, discSeq, freeItemID, freeItemQty, siteID, uom, lineRef, budgetID,
                                string.Empty);
                            lineRef = (lineRef.ToInt() + 1).ToString();
                            for (int l = lineRef.Length; lineRef.Length < 5; )
                                lineRef = "0" + lineRef;
                        }
                    } // if(free.FreeItemID.PassNull()!=string.Empty)

                    if (Util.PassNull(freeItemID) != string.Empty)
                    {
                        InsertUpdateOrdDisc(discID, discSeq, budgetID, discType, discFor, 0, amt, qty, budgetID,
                            freeItemID, freeItemQty, discLineRef, lineRef, breakLineRef);
                        discLineRef = (discLineRef.ToInt() + 1).ToString();
                        for (int l = discLineRef.Length; discLineRef.Length < 5; )
                            discLineRef = "0" + discLineRef;
                    }
                } //foreach (var free in lstFreeItem)
                if (beginCalc)
                    goto Begin;
            } //if(lstFreeItem.Count>0)
        }

        private void CalculateGroupDiscBundle(bool comp, bool isC2, string discID, string discSeq, string discType,
            double qty, double amt, string breakBy, string discFor, List<OM_DiscItem> item, ref string lineRef,
            ref string discLineRef, ref double discAmt1, ref double discAmt2, ref double[] bundleNbr)
        {
            double discAmt = 0,
                discAmtCal = 0,
                discPct = 0,
                freeItemQty = 0,
                qtyBreak = 0,
                freeItemQtyCal = 0,
                qtyAmt = 0,
                cnvFact = 0,
                iterateAmt = 0,
                bndTotAmtQty = 0;
            string siteID = string.Empty,
                uom = string.Empty,
                freeItemID = string.Empty,
                unitMultDiv = string.Empty,
                budgetID = string.Empty,
                breakLineRef = string.Empty;

            OM_DiscSeq objSeq =
                _app.OM_DiscSeq.FirstOrDefault(p => p.DiscID == discID && p.DiscSeq == discSeq && p.Status == "C");
            if (objSeq == null) objSeq = new OM_DiscSeq();
            Array.Sort(bundleNbr);
            qtyAmt = bundleNbr[0].ToInt();
        Begin:
            bool beginCalc = false;
            discAmtCal = GetBundleDiscBreak(discID, discSeq, qtyAmt, ref qtyBreak);
            if (discAmtCal > 0 || discAmt > 0)
            {
                if (
                    !CheckAvailableDiscBudget(ref budgetID, ref discID, ref discSeq, ref discAmtCal, false, false,
                        string.Empty, true, false, freeItemID, string.Empty))
                    discAmtCal = 0;
                if (discFor == "P")
                {
                    discAmt = discAmtCal * amt / 100;
                    discPct = discAmtCal;
                }
                else
                {
                    if (discAmtCal > 0 || freeItemQtyCal > 0)
                    {
                        discAmt = discAmt + discAmtCal * (qtyAmt / qtyBreak).ToInt();
                        if (qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 && _objOM.ProrateDisc != 0)
                        {
                            qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                            goto Begin;
                        }
                    }
                } //if(discFor=="P")

                if (isC2)
                    discAmt2 += discAmt;
                else
                    discAmt1 += discAmt;

                if (discAmt > 0)
                {
                    discID = objSeq.DiscID;
                    discSeq = objSeq.DiscSeq;
                    bndTotAmtQty = item.Sum(p => p.BundleQty + p.BundleAmt);
                    int k = 0;
                    foreach (OM_DiscItem omDiscItem in item)
                    {
                        UpdateGroupDiscBundleByInvtID(omDiscItem.InvtID, discPct, discAmt, discID, discSeq, isC2,
                            ref iterateAmt, k == item.Count - 1 ? true : false,
                            item[k].BundleQty + item[k].BundleAmt, bndTotAmtQty);
                        k++;
                    }
                    InsertUpdateOrdDisc(discID, discSeq, budgetID, discType, discFor, discAmt, amt, qty, budgetID,
                        freeItemID, freeItemQty, discLineRef, string.Empty, string.Empty);
                    discLineRef = (discLineRef.ToInt() + 1).ToString();
                    for (int l = discLineRef.Length; discLineRef.Length < 5; )
                        discLineRef = "0" + discLineRef;
                } //if(discAmt>0)
            } //if(discAmtCal>0 || discAmt>0)
            else
            {
                qtyBreak = 0;
                breakLineRef = string.Empty;
                GetBundleFreeItemBreak(discID, discSeq, qtyAmt, ref qtyBreak, ref breakLineRef);
                var lstFree = _app.OM_DiscFreeItem.Where(p =>
                    p.DiscID == objSeq.DiscID && p.DiscSeq == objSeq.DiscSeq &&
                    p.LineRef == breakLineRef).Select(p => new
                    {
                        Sel = true,
                        p.LineRef,
                        p.DiscID,
                        p.DiscSeq,
                        p.FreeITemSiteID,
                        p.FreeItemBudgetID,
                        p.FreeItemID,
                        p.FreeItemQty,
                        p.UnitDescr
                    }).ToList();
                if (lstFree.Count > 0)
                {
                    if (lstFree.Count > 1 && !objSeq.AutoFreeItem &&
                        !(objSeq.DiscClass == "II" && objSeq.ProAplForItem == "M"))
                    {
                    }
                    foreach (var free in lstFree)
                    {
                        budgetID = free.FreeItemBudgetID;
                        freeItemQtyCal = free.FreeItemQty;
                        if (!CheckAvailableDiscBudget(ref budgetID, ref discID, ref discSeq, ref freeItemQtyCal, true,
                                false, string.Empty, true, false, free.FreeItemID, free.UnitDescr))
                            discAmtCal = 0;
                        if (!beginCalc)
                            freeItemQty = 0;
                        if (freeItemQtyCal > 0)
                        {
                            freeItemQty = freeItemQty + freeItemQtyCal * (qtyAmt / qtyBreak).ToInt();
                            if (qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 && _objOM.ProrateDisc != 0)
                            {
                                qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                                beginCalc = true;
                            }
                        } // if(freeItemQtyCal>0)
                        if (freeItemQty > 0)
                        {
                            if (Util.PassNull(_objUser.DiscSite) != string.Empty)
                                siteID = _objUser.DiscSite;
                            else
                                siteID = free.FreeITemSiteID;

                            uom = free.UnitDescr;
                            freeItemID = free.FreeItemID;
                            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID);
                            if (objInvt == null) objInvt = new IN_Inventory();
                            IN_UnitConversion cnv = SetUOM(freeItemID, objInvt.ClassID, objInvt.StkUnit, uom);
                            if (cnv != null)
                            {
                                cnvFact = cnv.CnvFact;
                                unitMultDiv = cnv.MultDiv;
                            }
                            if (
                                CheckQtyAvail(freeItemID, siteID, unitMultDiv, freeItemQty, cnvFact, true, string.Empty) ==
                                1)
                            {
                                Util.AppendLog(ref _logMessage, "1045", parm: new[] { freeItemID, siteID });
                                freeItemID = string.Empty;
                                freeItemQty = 0;
                            }
                            else
                            {
                                AddFreeItem(objSeq.DiscID, objSeq.DiscSeq, freeItemID, freeItemQty, siteID, uom,
                                    lineRef, budgetID, string.Empty);
                                lineRef = (lineRef.ToInt() + 1).ToString();
                                for (int l = lineRef.Length; lineRef.Length < 5; )
                                    lineRef = "0" + lineRef;
                            }
                        } // if(freeItemQty>0)

                        if (freeItemQty > 0)
                        {
                            discID = objSeq.DiscID;
                            discSeq = objSeq.DiscSeq;
                            bndTotAmtQty = item.Sum(p => p.BundleQty + p.BundleAmt);
                            for (int k = 0; k < item.Count; k++)
                                UpdateGroupDiscBundleByInvtID(item[k].InvtID, discPct, discAmt, discID, discSeq,
                                    isC2, ref iterateAmt,
                                    k == item.Count - 1 ? true : false,
                                    item[k].BundleQty + item[k].BundleAmt, bndTotAmtQty);
                            discLineRef = (discLineRef.ToInt() + 1).ToString();
                            for (int l = discLineRef.Length; discLineRef.Length < 5; )
                                discLineRef = "0" + discLineRef;
                        } // if(freeItemQty>0)
                    } // foreach (var free in lstFree)
                    if (beginCalc)
                        goto Begin;
                    ;
                } // if(lstFree.Count>0)
            }
        }
        private double GetDiscGroupSetup(ref string lineRef, ref string discLineRef)
        {
            double discAmt1 = 0, discAmt2 = 0, qty = 0, amt = 0;
            string discID1 = string.Empty;
            double discItemUnitQty = 0;
            List<OM10100_pdDiscGroupSetUp_Result> lstSetup = _app.OM10100_pdDiscGroupSetUp(_objOrder.BranchID).ToList();
            foreach (var setup in lstSetup)
            {
                qty = 0;
                amt = 0;
                OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID01);
                if (objDisc != null)
                {
                    if (objDisc.DiscType == "G")
                    {
                        List<OM_DiscSeq> lstSeq = _app.OM_DiscSeq.Where(p => p.DiscID == objDisc.DiscID && p.Status == "C" && p.Active==1).ToList();
                        foreach (OM_DiscSeq seq in lstSeq)
                        {
                            discID1 = seq.DiscID;
                            OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);
                            if (_objCust == null) _objCust = new AR_Customer();
                            OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p =>
                                        p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq &&
                                        p.ClassID == _objCust.PriceClassID);

                            if (objDisc.DiscClass == "II" && (
                                    (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                    (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                    ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstItem.Count > 0)
                                {
                                    foreach (OM_DiscItem item in lstItem)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc);
                                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                                        if (objInvt == null) objInvt = new IN_Inventory();
                                        if (seq.BreakBy == "W")
                                            discItemUnitQty = qty1.ToInt() * objInvt.StkWt / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                        else
                                            discItemUnitQty = qty1.ToInt() / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                        
                                        qty += discItemUnitQty;
                                        amt += amt1;
                                    }
                                    CalculateGroupDisc(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, null, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                    qty = 0;
                                    amt = 0;
                                } // if(lstItem.Count>0)  
                            }
                            //if(seq.Active!=0 && objDisc.DiscClass=="II" &&objDisc.DiscClass=="CC" && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))                 
                            else if (objDisc.DiscClass == "CI" && objDiscCust != null && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstItem.Count > 0)
                                {
                                    foreach (OM_DiscItem item in lstItem)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc);
                                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                                        if (objInvt == null) objInvt = new IN_Inventory();
                                        if (seq.BreakBy == "W")
                                            discItemUnitQty = qty1.ToInt() * objInvt.StkWt / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                        else
                                            discItemUnitQty = qty1.ToInt() / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();

                                        qty += discItemUnitQty;
                                        amt += amt1;
                                    }

                                    CalculateGroupDisc(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, null, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                    qty = 0;
                                    amt = 0;
                                }
                            }
                            //else if(seq.Active!=0 && objDisc.DiscClass=="CI" && objDiscCust!=null && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))
                            else if (objDisc.DiscClass == "TI" && objDiscCustClass != null && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstItem.Count > 0)
                                {
                                    foreach (OM_DiscItem item in lstItem)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc);
                                        IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                                        if (objInvt == null) objInvt = new IN_Inventory();
                                        if (seq.BreakBy == "W")
                                            discItemUnitQty = qty1.ToInt() * objInvt.StkWt / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                        else
                                            discItemUnitQty = qty1.ToInt() /OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();

                                        qty += discItemUnitQty;
                                        amt += amt1;
                                    }
                                    CalculateGroupDisc(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, null, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                    
                                    qty = 0;
                                    amt = 0;
                                }
                            }
                            else if (objDisc.DiscClass == "PP" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) || 
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItemClass> lstPriceClass = _app.OM_DiscItemClass.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstPriceClass.Count > 0)
                                {
                                    foreach (OM_DiscItemClass item in lstPriceClass)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalItemClass(item.ClassID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc, item.UnitDesc);
                                        qty += qty1;
                                        amt += amt1;
                                    }
                                    CalculateGroupDisc(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, null, lstPriceClass, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                    qty = 0;
                                    amt = 0;
                                }
                            }
                            //else if(seq.Active!=0 && objDisc.DiscClass=="PP" && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))
                            else if (objDisc.DiscClass == "TP" && objDiscCustClass != null && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItemClass> lstPriceClass = _app.OM_DiscItemClass.Where( p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstPriceClass.Count > 0)
                                {
                                    foreach (OM_DiscItemClass item in lstPriceClass)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalItemClass(item.ClassID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc, item.UnitDesc);
                                        qty += qty1;
                                        amt += amt1;
                                    }
                                    CalculateGroupDisc(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, null, lstPriceClass, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                    qty = 0;
                                    amt = 0;
                                }
                            }
                            //else if(seq.Active!=0 && objDisc.DiscClass=="TP" && objDiscCustClass!=null && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))
                            else if (objDisc.DiscClass == "BB" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstItem.Count > 0)
                                {
                                    var bundleNbr = new double[lstItem.Count];
                                    int k = 0;
                                    foreach (OM_DiscItem item in lstItem)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc);
                                        if (qty1 == 0)
                                        {
                                            qty = 0;
                                            amt = 0;
                                            goto Finish;
                                        }

                                        amt += amt1;
                                        qty += qty1;
                                        if (seq.BreakBy == "A")
                                            bundleNbr[k] = amt1 / item.BundleAmt;
                                        else
                                            bundleNbr[k] = qty1 / item.BundleQty;
                                        k++;
                                    }
                                    CalculateGroupDiscBundle(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2, ref bundleNbr);

                                    qty = 0;
                                    amt = 0;
                                }
                            }
                            //else if(seq.Active!=0 && objDisc.DiscClass=="BB" && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))
                            else if (objDisc.DiscClass == "CB" && objDiscCust != null && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstItem.Count > 0)
                                {
                                    var bundleNbr = new double[lstItem.Count];
                                    int k = 0;
                                    foreach (OM_DiscItem item in lstItem)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc);
                                        if (qty1 == 0)
                                        {
                                            qty = 0;
                                            amt = 0;
                                            goto Finish;
                                        }
                                        amt += amt1;
                                        qty += qty1;
                                        if (seq.BreakBy == "A")
                                            bundleNbr[k] = amt1 / item.BundleAmt;
                                        else
                                            bundleNbr[k] = qty1 / item.BundleQty;
                                        k++;
                                    }
                                    CalculateGroupDiscBundle(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2, ref bundleNbr);

                                    qty = 0;
                                    amt = 0;
                                }
                            }
                            // else if(seq.Active!=0 && objDisc.DiscClass=="CB" && _objCust!=null && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))
                            else if (objDisc.DiscClass == "TB" && objDiscCustClass != null && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (lstItem.Count > 0)
                                {
                                    var bundleNbr = new double[lstItem.Count];
                                    int k = 0;
                                    foreach (OM_DiscItem item in lstItem)
                                    {
                                        double qty1 = 0, amt1 = 0;
                                        TotalInvt(item.InvtID, ref qty1, ref amt1,_objOM.InlcSOFeeDisc);
                                        if (qty1 == 0)
                                        {
                                            qty = 0;
                                            amt = 0;
                                            goto Finish;
                                        }
                                        amt += amt1;
                                        qty += qty1;
                                        if (seq.BreakBy == "A")
                                            bundleNbr[k] = amt1 / item.BundleAmt;
                                        else
                                            bundleNbr[k] = qty1 / item.BundleQty;
                                        k++;
                                    }
                                    CalculateGroupDiscBundle(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2, ref bundleNbr);

                                    qty = 0;
                                    amt = 0;
                                }
                            }
                        Finish:
                            string tmp = string.Empty;
                        } //foreach (var seq in lstSeq)

                        OM_Discount objDisc2 = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                        if (objDisc2 != null && Util.PassNull(discID1) != string.Empty)
                        {
                            if (objDisc2.DiscType == "G")
                            {
                                List<OM_DiscSeq> lstSeq2 = _app.OM_DiscSeq.Where(p => p.DiscID == objDisc2.DiscID && p.Status == "C" && p.Active == 1).ToList();
                                foreach (OM_DiscSeq seq in lstSeq2)
                                {
                                    discID1 = seq.DiscID;
                                    OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);

                                    OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                    if (objDisc.DiscClass == "II" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                        if (lstItem.Count > 0)
                                        {
                                            foreach (OM_DiscItem item in lstItem)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc, true);
                                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                                                if (objInvt == null) objInvt = new IN_Inventory();
                                                if (seq.BreakBy == "W")
                                                    discItemUnitQty = qty1.ToInt() * objInvt.StkWt / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                                else
                                                    discItemUnitQty = qty1.ToInt() / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();

                                                qty += discItemUnitQty;
                                                amt += amt1;
                                            }
                                            CalculateGroupDisc(setup.Comp != 0 ? true : false, true, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, null, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                            qty = 0;
                                            amt = 0;
                                        } // if(lstItem.Count>0)  
                                    }
                                    //if(seq.Active!=0 && objDisc.DiscClass=="II" &&objDisc.DiscClass=="CC" && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))   
                                    else if (objDisc.DiscClass == "CI" && objDiscCust != null && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                                ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                        if (lstItem.Count > 0)
                                        {
                                            foreach (OM_DiscItem item in lstItem)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc, true);
                                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                                                if (objInvt == null) objInvt = new IN_Inventory();
                                                if (seq.BreakBy == "W")
                                                    discItemUnitQty = qty1.ToInt() * objInvt.StkWt / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                                else
                                                    discItemUnitQty = qty1.ToInt() / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();

                                                qty += discItemUnitQty;
                                                amt += amt1;
                                            }
                                            CalculateGroupDisc(setup.Comp != 0 ? true : false, true, seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt, seq.BreakBy, seq.DiscFor, lstItem, null, ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                            qty = 0;
                                            amt = 0;
                                        }
                                    }
                                    //else if(seq.Active!=0 && objDisc.DiscClass=="CI" && objDiscCust!=null && ((_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && seq.Promo==0)|| (_objType.ARDocType!="CM" && _objType.ARDocType!="CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.StartDate)>=0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(),seq.EndDate)<=0 && seq.Promo!=0) || ((_objType.ARDocType=="CM" || _objType.ARDocType=="CC") && this.CheckReturnDisc(seq.DiscID,seq.DiscSeq))))
                                    else if (objDisc.DiscClass == "TI" && objDiscCustClass != null && (
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) || 
                                                (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) || 
                                                ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItem> lstItem = _app.OM_DiscItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                        if (lstItem.Count > 0)
                                        {
                                            foreach (OM_DiscItem item in lstItem)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalInvt(item.InvtID, ref qty1, ref amt1, _objOM.InlcSOFeeDisc, true);
                                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == item.InvtID);
                                                if (objInvt == null) objInvt = new IN_Inventory();
                                                if (seq.BreakBy == "W")
                                                    discItemUnitQty = qty1.ToInt() * objInvt.StkWt / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                                else
                                                    discItemUnitQty = qty1.ToInt() / OM_GetCnvFactFromUnit(item.InvtID, item.UnitDesc).ToInt();
                                                qty += discItemUnitQty;
                                                amt += amt1;
                                            }
                                            CalculateGroupDisc(setup.Comp != 0 ? true : false, true, seq.DiscID,
                                                seq.DiscSeq, objDisc.DiscType, qty, amt,
                                                seq.BreakBy, seq.DiscFor, lstItem, null, ref lineRef,
                                                ref discLineRef, ref discAmt1, ref discAmt2);
                                            qty = 0;
                                            amt = 0;
                                        }
                                    }
                                    else if (seq.Active != 0 && objDisc.DiscClass == "PP" &&
                                             ((_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 && seq.Promo == 0) ||
                                              (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.EndDate) <= 0 && seq.Promo != 0) ||
                                              ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") &&
                                               CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItemClass> lstPriceClass =
                                            _app.OM_DiscItemClass.Where(
                                                p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq)
                                                .ToList();
                                        if (lstPriceClass.Count > 0)
                                        {
                                            foreach (OM_DiscItemClass item in lstPriceClass)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalItemClass(item.ClassID, ref qty1, ref amt1,
                                                    _objOM.InlcSOFeeDisc, item.UnitDesc, true);
                                                qty += qty1;
                                                amt += amt1;
                                            }
                                            CalculateGroupDisc(setup.Comp != 0 ? true : false, true,
                                                seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt,
                                                seq.BreakBy, seq.DiscFor, null, lstPriceClass, ref lineRef,
                                                ref discLineRef, ref discAmt1, ref discAmt2);
                                            qty = 0;
                                            amt = 0;
                                        }
                                    }
                                    else if (seq.Active != 0 && objDisc.DiscClass == "TP" &&
                                             objDiscCustClass != null &&
                                             ((_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 && seq.Promo == 0) ||
                                              (_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.EndDate) <= 0 && seq.Promo != 0) ||
                                              ((_objType.ARDocType == "CM" ||
                                                _objType.ARDocType == "CC") &&
                                               CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItemClass> lstPriceClass =
                                            _app.OM_DiscItemClass.Where(
                                                p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq)
                                                .ToList();
                                        if (lstPriceClass.Count > 0)
                                        {
                                            foreach (OM_DiscItemClass item in lstPriceClass)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalItemClass(item.ClassID, ref qty1, ref amt1,
                                                    _objOM.InlcSOFeeDisc, item.UnitDesc, true);
                                                qty += qty1;
                                                amt += amt1;
                                            }
                                            CalculateGroupDisc(setup.Comp != 0 ? true : false, true,
                                                seq.DiscID, seq.DiscSeq, objDisc.DiscType, qty, amt,
                                                seq.BreakBy, seq.DiscFor, null, lstPriceClass,
                                                ref lineRef,
                                                ref discLineRef, ref discAmt1, ref discAmt2);
                                            qty = 0;
                                            amt = 0;
                                        }
                                    }
                                    else if (seq.Active != 0 && objDisc.DiscClass == "BB" &&
                                             ((_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 && seq.Promo == 0) ||
                                              (_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 &&
                                               DateTime.Compare(_objOrder.OrderDate.ToDateShort(),
                                                   seq.EndDate) <= 0 && seq.Promo != 0) ||
                                              ((_objType.ARDocType == "CM" ||
                                                _objType.ARDocType == "CC") &&
                                               CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItem> lstItem =
                                            _app.OM_DiscItem.Where(
                                                p =>
                                                    p.DiscID == seq.DiscID &&
                                                    p.DiscSeq == seq.DiscSeq).ToList();
                                        if (lstItem.Count > 0)
                                        {
                                            var bundleNbr = new double[lstItem.Count];
                                            int k = 0;
                                            foreach (OM_DiscItem item in lstItem)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalInvt(item.InvtID, ref qty1, ref amt1,
                                                    _objOM.InlcSOFeeDisc, true);
                                                if (qty1 == 0)
                                                {
                                                    qty = 0;
                                                    amt = 0;
                                                    goto FinishC;
                                                }

                                                amt += amt1;
                                                qty += qty1;
                                                if (seq.BreakBy == "A")
                                                    bundleNbr[k] = amt1 / item.BundleAmt;
                                                else
                                                    bundleNbr[k] = qty1 / item.BundleQty;
                                                k++;
                                            }
                                            CalculateGroupDiscBundle(
                                                setup.Comp != 0 ? true : false, true, seq.DiscID,
                                                seq.DiscSeq, objDisc.DiscType,
                                                qty, amt,
                                                seq.BreakBy, seq.DiscFor, lstItem, ref lineRef,
                                                ref discLineRef, ref discAmt1, ref discAmt2,
                                                ref bundleNbr);

                                            qty = 0;
                                            amt = 0;
                                        }
                                    }
                                    else if (seq.Active != 0 && objDisc.DiscClass == "CB" &&
                                             objDiscCust != null &&
                                             ((_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(
                                                   _objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 && seq.Promo == 0) ||
                                              (_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(
                                                   _objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 &&
                                               DateTime.Compare(
                                                   _objOrder.OrderDate.ToDateShort(), seq.EndDate) <=
                                               0 && seq.Promo != 0) ||
                                              ((_objType.ARDocType == "CM" ||
                                                _objType.ARDocType == "CC") &&
                                               CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItem> lstItem =
                                            _app.OM_DiscItem.Where(
                                                p =>
                                                    p.DiscID == seq.DiscID &&
                                                    p.DiscSeq == seq.DiscSeq).ToList();
                                        if (lstItem.Count > 0)
                                        {
                                            var bundleNbr = new double[lstItem.Count];
                                            int k = 0;
                                            foreach (OM_DiscItem item in lstItem)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalInvt(item.InvtID, ref qty1, ref amt1,
                                                    _objOM.InlcSOFeeDisc, true);
                                                if (qty1 == 0)
                                                {
                                                    qty = 0;
                                                    amt = 0;
                                                    goto FinishC;
                                                }
                                                amt += amt1;
                                                qty += qty1;
                                                if (seq.BreakBy == "A")
                                                    bundleNbr[k] = amt1 / item.BundleAmt;
                                                else
                                                    bundleNbr[k] = qty1 / item.BundleQty;
                                                k++;
                                            }
                                            CalculateGroupDiscBundle(
                                                setup.Comp != 0 ? true : false, true, seq.DiscID,
                                                seq.DiscSeq, objDisc.DiscType,
                                                qty, amt,
                                                seq.BreakBy, seq.DiscFor, lstItem, ref lineRef,
                                                ref discLineRef, ref discAmt1, ref discAmt2,
                                                ref bundleNbr);

                                            qty = 0;
                                            amt = 0;
                                        }
                                    }
                                    else if (seq.Active != 0 && objDisc.DiscClass == "TB" &&
                                             objDiscCustClass != null &&
                                             ((_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(
                                                   _objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 && seq.Promo == 0) ||
                                              (_objType.ARDocType != "CM" &&
                                               _objType.ARDocType != "CC" &&
                                               DateTime.Compare(
                                                   _objOrder.OrderDate.ToDateShort(),
                                                   seq.StartDate) >= 0 &&
                                               DateTime.Compare(
                                                   _objOrder.OrderDate.ToDateShort(),
                                                   seq.EndDate) <= 0 && seq.Promo != 0) ||
                                              ((_objType.ARDocType == "CM" ||
                                                _objType.ARDocType == "CC") &&
                                               CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                    {
                                        List<OM_DiscItem> lstItem =
                                            _app.OM_DiscItem.Where(
                                                p =>
                                                    p.DiscID == seq.DiscID &&
                                                    p.DiscSeq == seq.DiscSeq).ToList();
                                        if (lstItem.Count > 0)
                                        {
                                            var bundleNbr = new double[lstItem.Count];
                                            int k = 0;
                                            foreach (OM_DiscItem item in lstItem)
                                            {
                                                double qty1 = 0, amt1 = 0;
                                                TotalInvt(item.InvtID, ref qty1, ref amt1,
                                                    _objOM.InlcSOFeeDisc, true);
                                                if (qty1 == 0)
                                                {
                                                    qty = 0;
                                                    amt = 0;
                                                    goto FinishC;
                                                }
                                                amt += amt1;
                                                qty += qty1;
                                                if (seq.BreakBy == "A")
                                                    bundleNbr[k] = amt1 / item.BundleAmt;
                                                else
                                                    bundleNbr[k] = qty1 / item.BundleQty;
                                                k++;
                                            }
                                            CalculateGroupDiscBundle(
                                                setup.Comp != 0 ? true : false, true,
                                                seq.DiscID, seq.DiscSeq, objDisc.DiscType,
                                                qty, amt,
                                                seq.BreakBy, seq.DiscFor, lstItem,
                                                ref lineRef,
                                                ref discLineRef, ref discAmt1, ref discAmt2,
                                                ref bundleNbr);

                                            qty = 0;
                                            amt = 0;
                                        }
                                    }
                                FinishC:
                                    string tmp2 = string.Empty;
                                } //foreach (var seq in lstSeq)
                            } ////if(objDisc2.DiscType=="G")
                        }
                    } //if(objDisc.DiscType=="G")
                } //if(objDisc!=null)
            } //foreach (var setup  in lstSetup)
            return discAmt1 + discAmt2;
        }
        private void CalculateDocDisc(bool comp, bool isC2, string discID, string discSeq, string discType,
            string budgetID, string breakBy, string discFor, ref string lineRef, ref string discLineRef,
            ref double discAmt1, ref double discAmt2)
        {
            double discAmt = 0,
                discAmtCal = 0,
                discPct = 0,
                freeItemQty = 0,
                freeItemQtyCal = 0,
                qtyBreak = 0,
                amt = 0,
                qtyAmt = 0,
                cnvFact = 0,
                docAmt = 0;
            string siteID = string.Empty,
                uom = string.Empty,
                unitMultDiv = string.Empty,
                freeItemID = string.Empty,
                breakLineRef = string.Empty;
            OM_DiscSeq objSeq =
                _app.OM_DiscSeq.FirstOrDefault(p => p.DiscID == discID && p.DiscSeq == discSeq && p.Status == "C");
            if (objSeq == null) objSeq = new OM_DiscSeq();
            if (Util.PassNull(objSeq.BudgetID) != string.Empty && _objOrder.SlsPerID == string.Empty)
            {
                Util.AppendLog(ref _logMessage, "403", parm: new[] { objSeq.DiscSeq, objSeq.BudgetID });
                return;
            }
            docAmt = _objOrder.LineAmt - _objOrder.VolDiscAmt;
            if (comp)
                docAmt = (docAmt - discAmt1);

            amt = docAmt;
            if (_objOM.InlcSOFeeDisc)
            {
                qtyAmt = amt + _objOrder.SOFeeTot;
                amt = amt + _objOrder.SOFeeTot;
            }
            else
                qtyAmt = amt;

        Begin:
            bool beginCalc = false;
            discAmtCal = GetDiscBreak(discID, discSeq, breakBy, qtyAmt, ref qtyBreak, ref breakLineRef);
            if (discAmtCal > 0 || discAmt > 0)
            {
                if (discFor == "P")
                {
                    discAmt = Math.Round(discAmtCal * amt / 100, 0);
                    discPct = discAmtCal;
                } //if(discFor=="P")
                else
                {
                    if (discAmtCal > 0 || freeItemQtyCal > 0)
                    {
                        if (qtyBreak == 1)
                            discAmt = Math.Round(discAmt + discAmtCal, 0);
                        else
                            discAmt = Math.Round(discAmt + discAmtCal * (qtyAmt / qtyBreak).ToInt(), 0);

                        if (qtyBreak != 1 && qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 &&
                            _objOM.ProrateDisc != 0)
                        {
                            qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                            goto Begin;
                        }
                    } //if(discAmtCal>0 || freeItemQtyCal>0)
                }

                if (discAmt > 0)
                {
                    amt = docAmt;
                    if (isC2)
                        discAmt2 += discAmt;
                    else
                        discAmt1 += discAmt;

                    if (
                        !CheckAvailableDiscBudget(ref budgetID, ref discID, ref discSeq, ref discAmt, false, false,
                            string.Empty, true, false, freeItemID, string.Empty))
                    {
                        discAmt = 0;
                        discAmt1 = 0;
                        discAmt2 = 0;
                    }
                    if (discAmt > 0)
                    {
                        InsertUpdateOrdDisc(discID, discSeq, budgetID, discType, objSeq.DiscFor, discAmt, amt, 0,
                            budgetID, freeItemID, freeItemQty, discLineRef, string.Empty, breakLineRef);
                        discLineRef = (discLineRef.ToInt() + 1).ToString();
                        for (int l = discLineRef.Length; discLineRef.Length < 5; )
                            discLineRef = "0" + discLineRef;
                    }
                }
            } //if(discAmtCal>0 || discAmt>0)
            else
            {
                qtyBreak = 0;
                breakLineRef = string.Empty;
                GetDiscBreak(discID, discSeq, breakBy, qtyAmt, ref qtyBreak, ref breakLineRef);
                var lstFreeItem =
                    _app.OM_DiscFreeItem.Where(p => p.DiscID == objSeq.DiscID && p.DiscSeq == objSeq.DiscSeq &&
                                                   p.LineRef == breakLineRef).Select(p => new
                                                   {
                                                       Sel = true,
                                                       p.LineRef,
                                                       p.DiscID,
                                                       p.DiscSeq,
                                                       p.FreeITemSiteID,
                                                       p.FreeItemBudgetID,
                                                       p.FreeItemID,
                                                       p.FreeItemQty,
                                                       p.UnitDescr
                                                   }).ToList();


                if (lstFreeItem.Count > 0)
                {
                    if (lstFreeItem.Count > 1 && !objSeq.AutoFreeItem &&
                        !(objSeq.DiscClass == "II" && objSeq.ProAplForItem == "M"))
                    {
                    }
                    int countRow = 0;
                    foreach (var free in lstFreeItem)
                    {
                        countRow++;
                        budgetID = free.FreeItemBudgetID;
                        freeItemQtyCal = free.FreeItemQty;
                        if (!beginCalc)
                            freeItemQty = 0;

                        if (freeItemQtyCal > 0)
                        {
                            freeItemQty = Math.Round(freeItemQty + freeItemQtyCal * (qtyAmt / qtyBreak).ToInt(), 0);
                            if (countRow == lstFreeItem.Count && qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 &&
                                _objOM.ProrateDisc != 0)
                            {
                                qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                                beginCalc = true;
                            }
                            else
                                beginCalc = false;
                        } // if(freeItemQtyCal>0)

                        if (freeItemQty > 0)
                        {
                            amt = docAmt;
                            if (isC2)
                                discAmt2 += discAmt;
                            else
                                discAmt1 += discAmt;

                            if (
                                !CheckAvailableDiscBudget(ref budgetID, ref discID, ref discSeq, ref freeItemQty, true,
                                    false, string.Empty, true, false, free.FreeItemID, free.UnitDescr))
                            {
                                discAmt = 0;
                                discAmt1 = 0;
                                discAmt2 = 0;
                                freeItemQty = 0;
                            }
                            if (Util.PassNull(_objUser.DiscSite) != string.Empty)
                                siteID = _objUser.DiscSite;
                            else
                                siteID = free.FreeITemSiteID;

                            uom = free.UnitDescr;
                            freeItemID = free.FreeItemID;

                            if (freeItemQty > 0)
                            {
                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID);
                                if (objInvt == null) objInvt = new IN_Inventory();
                                IN_UnitConversion cnv = SetUOM(freeItemID, objInvt.ClassID, objInvt.StkUnit, uom);
                                if (cnv != null)
                                {
                                    cnvFact = cnv.CnvFact;
                                    unitMultDiv = cnv.MultDiv;
                                }
                                if (
                                    CheckQtyAvail(freeItemID, siteID, unitMultDiv, freeItemQty, cnvFact, true,
                                        string.Empty) == 1)
                                {
                                    Util.AppendLog(ref _logMessage, "1045", parm: new[] { freeItemID, siteID });
                                    freeItemID = string.Empty;
                                    freeItemQty = 0;
                                }
                                else
                                {
                                    AddFreeItem(discID, discSeq, freeItemID, freeItemQty, siteID, uom, lineRef,
                                        budgetID, string.Empty);
                                    lineRef = (lineRef.ToInt() + 1).ToString();
                                    for (int l = lineRef.Length; lineRef.Length < 5; )
                                        lineRef = "0" + lineRef;
                                }
                            } // if(freeItemQty>0)
                            if (freeItemQty > 0)
                            {
                                InsertUpdateOrdDisc(discID, discSeq, budgetID, discType, objSeq.DiscFor, discAmt,
                                    amt, 0, budgetID, freeItemID, freeItemQty, discLineRef, lineRef,
                                    breakLineRef);
                                discLineRef = (discLineRef.ToInt() + 1).ToString();
                                for (int l = discLineRef.Length; discLineRef.Length < 5; )
                                    discLineRef = "0" + discLineRef;
                            } // if(freeItemQty>0)
                        } // if(freeItemQty>0)
                    } // foreach (var free in lstFreeItem)
                    if (beginCalc)
                        goto Begin;
                } //if(lstFreeItem.Count>0)
            }
        }
        private double GetDiscDocSetup(ref string lineRef, ref string discLineRef)
        {
            double discAmt1 = 0, discAmt2 = 0;
            string discID1 = string.Empty;
            List<OM10100_pdDiscDocSetUp_Result> lstSetup = _app.OM10100_pdDiscDocSetUp(_objOrder.BranchID).ToList();
            foreach (var setup in lstSetup)
            {
                OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID01);
                if (objDisc == null) objDisc = new OM_Discount();
                if (objDisc.DiscType == "D")
                {
                    List<OM_DiscSeq> lstSeq =
                        _app.OM_DiscSeq.Where(p => p.DiscID == objDisc.DiscID && p.Status == "C" && p.Active==1).ToList();
                    foreach (OM_DiscSeq seq in lstSeq)
                    {
                        if (seq.Active != 0 && objDisc.DiscClass == "CC" &&
                            ((_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                              DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                              seq.Promo == 0) ||
                             (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                              DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                              DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 &&
                              seq.Promo != 0) ||
                             ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") &&
                              CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                        {
                            OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p =>
                                p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq &&
                                p.CustID == _objOrder.CustID);
                            if (objDiscCust != null)
                            {
                                CalculateDocDisc(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType,
                                    seq.BudgetID, seq.BreakBy, seq.DiscFor, ref lineRef,
                                    ref discLineRef, ref discAmt1, ref discAmt2);
                                OM_Discount objDisc2 =
                                    _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                if (Util.PassNull(setup.DiscID02) != string.Empty && objDisc2 != null)
                                {
                                    discID1 = seq.DiscID;
                                    break;
                                }
                                return discAmt1;
                            }
                        }
                        else if (seq.Active != 0 && objDisc.DiscClass == "TT" &&
                                 ((_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                   DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                                   seq.Promo == 0) ||
                                  (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                   DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                                   DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 &&
                                   seq.Promo != 0) ||
                                  ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") &&
                                   CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                        {
                            if (_objCust == null) _objCust = new AR_Customer();
                            ;
                            OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p =>
                                p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq &&
                                p.ClassID == _objCust.PriceClassID);
                            if (objDiscCustClass != null)
                            {
                                CalculateDocDisc(false, false, seq.DiscID, seq.DiscSeq, objDisc.DiscType,
                                    seq.BudgetID, seq.BreakBy, seq.DiscFor, ref lineRef,
                                    ref discLineRef, ref discAmt1, ref discAmt2);
                                OM_Discount objDisc2 =
                                    _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                if (Util.PassNull(setup.DiscID02) != string.Empty && objDisc2 != null)
                                {
                                    discID1 = seq.DiscID;
                                    break;
                                }
                                return discAmt1;
                            }
                        }
                    } //foreach (var seq in lstSeq)

                    objDisc =
                        _app.OM_Discount.FirstOrDefault(
                            p => p.DiscID == setup.DiscID02 && discID1 != string.Empty);
                    if (objDisc == null) objDisc = new OM_Discount();
                    if (objDisc.DiscType == "D")
                    {
                        List<OM_DiscSeq> lstseq =
                            _app.OM_DiscSeq.Where(p => p.DiscID == objDisc.DiscID && p.Status == "C").ToList();
                        foreach (OM_DiscSeq seq in lstseq)
                        {
                            if (seq.Active != 0 && seq.DiscClass == "CC" &&
                                ((_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                  DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                                  seq.Promo == 0) ||
                                 (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                  DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                                  DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 &&
                                  seq.Promo != 0) ||
                                 ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") &&
                                  CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p =>
                                    p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq &&
                                    p.CustID == _objOrder.CustID);
                                if (objDiscCust != null)
                                {
                                    CalculateDocDisc(setup.Comp != 0 ? true : false, true, seq.DiscID, seq.DiscSeq,
                                        objDisc.DiscType, seq.BudgetID, seq.BreakBy, seq.DiscFor, ref lineRef,
                                        ref discLineRef, ref discAmt1, ref discAmt2);
                                    if (discAmt2 != 0)
                                        return Math.Round(discAmt1 + discAmt2, 0);
                                    return discAmt1;
                                }
                                return discAmt1;
                            }
                            if (seq.Active != 0 && objDisc.DiscClass == "TT" &&
                                ((_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                  DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                                  seq.Promo == 0) ||
                                 (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" &&
                                  DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 &&
                                  DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 &&
                                  seq.Promo != 0) ||
                                 ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") &&
                                  CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                if (_objCust == null) _objCust = new AR_Customer();
                                OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p =>
                                    p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq &&
                                    p.ClassID == _objCust.PriceClassID);
                                if (objDiscCustClass != null)
                                {
                                    CalculateDocDisc(setup.Comp != 0 ? true : false, true, seq.DiscID, seq.DiscSeq,
                                        objDisc.DiscType, seq.BudgetID, seq.BreakBy, seq.DiscFor,
                                        ref lineRef, ref discLineRef, ref discAmt1, ref discAmt2);
                                    if (discAmt2 != 0)
                                        return Math.Round(discAmt1 + discAmt2, 0);
                                    return discAmt1;
                                }
                                return discAmt1;
                            }
                        } //foreach (var seq in lstseq)
                    } //  if(objDisc2.DiscType=="D")
                } // if(objDisc.DiscType=="D")
            } //foreach(var setup in lstSetup)

            return discAmt1 + discAmt2;
        }
        private void FreeItemForLine(ref string lineRef, ref string discLineRef)
        {
            string discSeq1 = string.Empty;
            string discSeq2 = string.Empty;
            string discID1 = string.Empty;
            string discID2 = string.Empty;
            string boType = string.Empty;

            _lstOldOrdDet = new List<OM10100_pgOrderDet_Result>(_lstOrdDet);
            foreach (OM10100_pgOrderDet_Result det in _lstOrdDet)
            {
                if (!det.FreeItem)
                {
                    discSeq1 = det.DiscSeq1;
                    discSeq2 = det.DiscSeq2;
                    discID1 = det.DiscID1;
                    discID2 = det.DiscID2;
                    boType = det.BOType;
                    GetFreeItemLine(det.InvtID, det.LineAmt, det.LineQty * det.UnitRate, ref discSeq1, ref discSeq2,
                        ref discID1, ref discID2, det.LineRef, ref lineRef, ref discLineRef, boType);
                }
            }
        }
        private void GetFreeItemLine(string invtID, double amt, double qty, ref string discSeq1, ref string discSeq2,
            ref string discID1, ref string discID2, string currentLineRef, ref string lineRef, ref string discLineRef, string boType)
        {
            double freeItemQty1 = 0;
            double freeItemQty2 = 0;
            string freeItemID1 = string.Empty;
            string freeItemID2 = string.Empty;
            string siteID1 = string.Empty;
            string siteID2 = string.Empty;
            string uom1 = string.Empty;
            string uom2 = string.Empty;
            string budgetID1 = string.Empty;
            string budgetID2 = string.Empty;
            double qtyBreak = 0;
            double qtyAmt = 0;
            bool calcDisc = false;
            bool calcDiscC = false;
            double discItemUntiQty = 0;
            List<OM_DiscSeq> lstDiscSeq;
            List<OM_DiscFreeItem> lstDiscFreeItem1;
            List<OM_DiscFreeItem> lstDiscFreeItem2;
            List<OM10100_pdDiscLineSetUp_Result> lstSetup = _app.OM10100_pdDiscLineSetUp(_objOrder.BranchID).ToList();
            foreach (var setup in lstSetup)
            {
                OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID01);
                if (objDisc != null)
                {
                    if (objDisc.DiscType == "L")
                    {
                        lstDiscSeq = _app.OM_DiscSeq.Where(p => p.DiscID == objDisc.DiscID && p.Status == "C" && p.Active == 1).ToList();
                        foreach (OM_DiscSeq seq in lstDiscSeq)
                        {
                            calcDisc = false;
                            calcDiscC = false;
                            OM_DiscItem objItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == objDisc.DiscID && p.DiscSeq == seq.DiscSeq);
                            
                            if (objItem == null) objItem = new OM_DiscItem();
                            discItemUntiQty = (qty / OM_GetCnvFactFromUnit(invtID, objItem.UnitDesc)).ToInt();

                            lstDiscFreeItem1 = _app.OM_DiscFreeItem.Where(p => p.DiscID == objDisc.DiscID && p.DiscSeq == seq.DiscSeq).ToList();

                            if (seq.BreakBy == "A")
                                qtyAmt = amt;
                            else if (seq.BreakBy == "W")
                            {
                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                                qtyAmt = discItemUntiQty * objInvt.StkWt;
                            }
                            else
                            {
                                if (seq.DiscClass == "II" || seq.DiscClass == "CI" || seq.DiscClass == "TI")
                                    qtyAmt = discItemUntiQty;
                                else
                                    qtyAmt = amt;
                            }

                            if (lstDiscFreeItem1.Count > 0 && objDisc.DiscClass == "CC" && (
                                    (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate, seq.StartDate) >= 0 && seq.Promo == 0) ||
                                    (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                    ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);
                                if (objDiscCust != null)
                                {
                                    calcDisc = true;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && objDisc.DiscClass == "II" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                OM_DiscItem objTmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.InvtID == invtID);
                                if (objTmpItem != null)
                                {
                                    calcDisc = true;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && objDisc.DiscClass == "PP" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                                OM_DiscItemClass objDiscItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == objInvt.PriceClassID);
                                if (objDiscItemClass != null)
                                {
                                    calcDisc = true;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && objDisc.DiscClass == "TT" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                if (objDiscCustClass != null)
                                {
                                    calcDisc = true;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && objDisc.DiscClass == "CI" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);
                               
                                OM_DiscItem objTmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.InvtID == invtID);
                                if (objTmpItem != null && objDiscCust != null)
                                {
                                    calcDisc = true;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && objDisc.DiscClass == "TI" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                OM_DiscItem objTmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.InvtID == invtID);
                                if (objTmpItem != null && objDiscCustClass != null)
                                {
                                    calcDisc = true;
                                }
                            }
                            else if (lstDiscFreeItem1.Count > 0 && objDisc.DiscClass == "TP" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" ||_objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                            {
                                OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                                OM_DiscItemClass objDiscItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == objInvt.PriceClassID);
                                if (objDiscItemClass != null && objDiscCustClass != null)
                                {
                                    calcDisc = true;
                                }
                            }
                            else
                            {
                                calcDisc = false;
                            }
                        Calc:
                            bool goto_Calc = false;
                            double freeItemQty = 0;
                            string breakLineRef1 = string.Empty;

                            if (calcDisc)
                            {
                                GetDiscBreak(seq.DiscID, seq.DiscSeq, seq.BreakBy, qtyAmt, ref qtyBreak, ref breakLineRef1);
                                discID1 = objDisc.DiscID;
                                discSeq1 = seq.DiscSeq;
                                var lstDiscFreeItem1tmp = _app.OM_DiscFreeItem.Where(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.LineRef == breakLineRef1)
                                    .Select(p =>
                                        new
                                        {
                                            p.DiscSeq,
                                            p.DiscID,
                                            p.FreeITemSiteID,
                                            p.FreeItemBudgetID,
                                            p.FreeItemID,
                                            p.FreeItemQty,
                                            p.LineRef,
                                            p.UnitDescr,
                                            Setl = true
                                        }).ToList();

                                if (lstDiscFreeItem1tmp.Count > 0)
                                {
                                    if (lstDiscFreeItem1tmp.Count > 1 && !seq.AutoFreeItem &&
                                        !(seq.DiscClass == "II" && seq.ProAplForItem == "M"))
                                    {
                                        //Dim frmPro As New frmFreeItem
                                        //            frmPro.Text = Language.GetDescr(HQSys.LangID, "FreeItemList")
                                        //            frmPro.GridFreeItemList.DataSource = dtDiscFreeItem1
                                        //            Me.FormatFreeItemList(frmPro.GridFreeItemList)
                                        //            frmPro.ShowDialog()
                                        //            If frmPro.DialogResult = Windows.Forms.DialogResult.OK Then
                                        //                frmPro.GridFreeItemList.UpdateData()
                                        //                dtDiscFreeItem1 = frmPro.GridFreeItemList.DataSource
                                        //                For irow As Int32 = dtDiscFreeItem1.Rows.Count - 1 To 0 Step -1
                                        //                    If Not dtDiscFreeItem1.Rows(irow).Item("Sel") Then
                                        //                        dtDiscFreeItem1.Rows.RemoveAt(irow)
                                        //                    End If
                                        //                Next
                                        //                dtDiscFreeItem1.AcceptChanges()
                                        //            Else
                                        //                GoTo NextDiscSeq
                                        //            End If
                                        goto NextDiscSeq;
                                    }
                                    //if(lstDiscFreeItem1tmp.Count>1 && ! seq.AutoFreeItem && !(seq.DiscClass=="II" &&seq.ProAplForItem=="M"))

                                    string invtID1 = string.Empty;
                                    if (seq.DiscClass == "II" && seq.ProAplForItem == "M")
                                        invtID1 = invtID;
                                    else
                                        invtID1 = "";

                                    int countRow = 0;
                                    if (lstDiscFreeItem1tmp.Where(p => p.FreeItemID.Contains(invtID1)).Count() > 0)
                                    {
                                        lstDiscFreeItem1tmp = lstDiscFreeItem1tmp.Where(p => p.FreeItemID.Contains(invtID1)).ToList();
                                    }
                                    else
                                    {
                                        lstDiscFreeItem1tmp.Clear();
                                    }
                                    foreach (var free in lstDiscFreeItem1tmp)
                                    {
                                        countRow = countRow + 1;
                                        if (Util.PassNull(_objUser.DiscSite) != string.Empty)
                                            siteID1 = _objUser.DiscSite;
                                        else
                                            siteID1 = free.FreeITemSiteID;

                                        uom1 = free.UnitDescr;
                                        budgetID1 = free.FreeItemBudgetID;
                                        freeItemQty = free.FreeItemQty;
                                        if (!goto_Calc)
                                            freeItemQty1 = 0;

                                        if (freeItemQty > 0 || freeItemQty1 > 0)
                                        {
                                            freeItemQty1 = freeItemQty * (int)(qtyAmt / qtyBreak);
                                            if (freeItemQty > 0 && countRow == lstDiscFreeItem1tmp.Count)
                                            {
                                                if (qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 && _objOM.ProrateDisc != 0)
                                                {
                                                    qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                                                    goto_Calc = true;
                                                }
                                                else
                                                    goto_Calc = false;
                                            }
                                            freeItemID1 = free.FreeItemID;
                                            double cnvFact = 0;
                                            string unitMultDiv = string.Empty;
                                            IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID1);
                                            if (objInvt == null) objInvt = new IN_Inventory();
                                            IN_UnitConversion cnv = SetUOM(freeItemID1, objInvt.ClassID, objInvt.StkUnit,
                                                uom1);
                                            if (cnv != null)
                                            {
                                                cnvFact = cnv.CnvFact;
                                                unitMultDiv = cnv.MultDiv;
                                            }
                                            if (invtID != freeItemID1)
                                                boType = "S";
                                            if (boType == "S" && CheckQtyAvail(freeItemID1, siteID1, unitMultDiv, freeItemQty1, cnvFact, true, "") == 1)
                                            {
                                                Util.AppendLog(ref _logMessage, "1045", parm: new[] { freeItemID1, siteID1 });
                                                freeItemQty1 = 0;
                                                goto NextFreeItem1;
                                            }
                                            if (freeItemQty1 > 0)
                                            {
                                                if (!CheckAvailableDiscBudget(ref budgetID1, ref discID1, ref discSeq1, ref freeItemQty1, true, false, currentLineRef, true, true, invtID, free.UnitDescr))
                                                {
                                                    freeItemQty1 = 0;
                                                    freeItemID1 = string.Empty;
                                                    budgetID1 = string.Empty;
                                                }
                                                else
                                                {
                                                    InsertUpdateOrdDisc(discID1, discSeq1, budgetID1, objDisc.DiscType, seq.DiscFor, 0, 0, 0, seq.BudgetID, freeItemID1, freeItemQty1, discLineRef, lineRef, breakLineRef1);
                                                    discLineRef = (discLineRef.ToInt() + 1).ToString();
                                                    for (int t = discLineRef.Length; discLineRef.Length < 5; )
                                                        discLineRef = "0" + discLineRef;

                                                    if (freeItemID1 != string.Empty && freeItemQty1 > 0)
                                                    {
                                                        AddFreeItem(discID1, discSeq1, freeItemID1, freeItemQty1, siteID1, uom1, lineRef, budgetID1, boType);
                                                        lineRef = (lineRef.ToInt() + 1).ToString();
                                                        for (int t = lineRef.Length; lineRef.Length < 5; )
                                                            lineRef = "0" + lineRef;
                                                    }
                                                }
                                            } //if(freeItemQty1>0)
                                        } //if(freeItemQty>0 || freeItemQty1>0)
                                    NextFreeItem1:
                                        int a;
                                    } // foreach (var free in lstDiscFreeItem1tmp)
                                    if (goto_Calc)
                                        goto Calc;
                                    objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                                    if (setup.DiscID02.PassNull() != string.Empty && objDisc != null)
                                        break;
                                    return;
                                } //if(lstDiscFreeItem1tmp.Count>0)
                            } //calcDisc        
                        NextDiscSeq:
                            int b;
                        } //foreach (var seq in lstDiscSeq)
                    } //if(objDisc.DiscType=="L")   
                    objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == setup.DiscID02);
                    if (freeItemQty1 > 0 && objDisc != null && discID1 != string.Empty && discID2 == string.Empty)
                    {
                        if (objDisc.DiscType == "L")
                        {
                            lstDiscSeq =  _app.OM_DiscSeq.Where(p => p.DiscID == objDisc.DiscID && p.Status == "C" && p.Active==1).ToList();
                            foreach (OM_DiscSeq seq in lstDiscSeq)
                            {
                                lstDiscFreeItem2 = _app.OM_DiscFreeItem.Where(p => p.DiscID == objDisc.DiscID && p.DiscSeq == seq.DiscSeq).ToList();
                                if (seq.BreakBy == "A")
                                    qtyAmt = amt;
                                else
                                    qtyAmt = qty;

                                if (lstDiscFreeItem2.Count > 0 && objDisc.DiscClass == "CC" && (
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                        (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                        ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                {
                                    OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);
                                    if (objDiscCust != null)
                                    {
                                        calcDiscC = true;
                                    }
                                }
                                else if (lstDiscFreeItem2.Count > 0 && objDisc.DiscClass == "II" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                {
                                    OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                    if (objDiscCustClass != null)
                                    {
                                        calcDiscC = true;
                                    }
                                }
                                else if (lstDiscFreeItem2.Count > 0 && objDisc.DiscClass == "TT" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                {
                                    OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                    if (objDiscCustClass != null)
                                    {
                                        calcDiscC = true;
                                    }
                                }
                                else if (lstDiscFreeItem2.Count > 0 &&  objDisc.DiscClass == "CI" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                {
                                    OM_DiscCust objDiscCust = _app.OM_DiscCust.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.CustID == _objOrder.CustID);
                                    OM_DiscItem objTmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.InvtID == invtID);
                                    if (objTmpItem != null && objDiscCust != null)
                                    {
                                        calcDiscC = true;
                                    }
                                }
                                else if (lstDiscFreeItem2.Count > 0 && objDisc.DiscClass == "TI" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                {
                                    OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                    OM_DiscItem objTmpItem = _app.OM_DiscItem.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.InvtID == invtID);
                                    if (objTmpItem != null && objDiscCustClass != null)
                                    {
                                        calcDiscC = true;
                                    }
                                }
                                else if (lstDiscFreeItem2.Count > 0 && objDisc.DiscClass == "PP" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                {
                                    IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                                    OM_DiscItemClass objDiscItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == objInvt.PriceClassID);
                                    if (objDiscItemClass != null)
                                    {
                                        calcDiscC = true;
                                    }
                                }
                                else if (lstDiscFreeItem2.Count > 0 && objDisc.DiscClass == "TP" && (
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && seq.Promo == 0) ||
                                            (_objType.ARDocType != "CM" && _objType.ARDocType != "CC" && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.StartDate) >= 0 && DateTime.Compare(_objOrder.OrderDate.ToDateShort(), seq.EndDate) <= 0 && seq.Promo != 0) ||
                                            ((_objType.ARDocType == "CM" || _objType.ARDocType == "CC") && CheckReturnDisc(seq.DiscID, seq.DiscSeq))))
                                {
                                    OM_DiscCustClass objDiscCustClass = _app.OM_DiscCustClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == _objCust.PriceClassID);
                                    IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == invtID);
                                    OM_DiscItemClass objDiscItemClass = _app.OM_DiscItemClass.FirstOrDefault(p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.ClassID == objInvt.PriceClassID);
                                    if (objDiscItemClass != null && objDiscCustClass != null)
                                    {
                                        calcDiscC = true;
                                    }
                                }
                                else
                                    calcDiscC = false;
                            CalcC:
                                bool goto_CalcC = false;
                                double freeItemQty = 0;
                                string breakLineRef2 = string.Empty;
                                if (calcDiscC)
                                {
                                    GetDiscBreak(seq.DiscID, seq.DiscSeq, seq.BreakBy, qtyAmt, ref qtyBreak,
                                        ref breakLineRef2);
                                    discID2 = objDisc.DiscID;
                                    discSeq2 = seq.DiscSeq;
                                    var lstDiscFreeItem2tmp =
                                        _app.OM_DiscFreeItem.Where( p => p.DiscID == seq.DiscID && p.DiscSeq == seq.DiscSeq && p.LineRef == breakLineRef2)
                                            .Select(p => new
                                            {
                                                p.DiscSeq,
                                                p.DiscID,
                                                p.FreeITemSiteID,
                                                p.FreeItemBudgetID,
                                                p.FreeItemID,
                                                p.FreeItemQty,
                                                p.LineRef,
                                                p.UnitDescr,
                                                Setl = true
                                            }).ToList();
                                    if (lstDiscFreeItem2tmp.Count > 0)
                                    {
                                        if (lstDiscFreeItem2tmp.Count > 1 && !seq.AutoFreeItem &&
                                            !(seq.DiscClass == "II" && seq.ProAplForItem == "M"))
                                        {
                                            goto NextDiscSeqC;
                                        }
                                        string invtID2 = string.Empty;
                                        if (seq.DiscClass == "II" && seq.ProAplForItem == "M")
                                            invtID2 = invtID;
                                        else
                                            invtID2 = "";

                                        int countRow = 0;
                                        if (lstDiscFreeItem2tmp.Where(p => p.FreeItemID.Contains(invtID2)).Count() > 0)
                                        {
                                            lstDiscFreeItem2tmp = lstDiscFreeItem2tmp.Where(p => p.FreeItemID.Contains(invtID2)).ToList();
                                        }
                                        else
                                        {
                                            lstDiscFreeItem2tmp.Clear();
                                        }
                                        foreach (var free in lstDiscFreeItem2tmp)
                                        {
                                            countRow = countRow + 1;
                                            if (_objUser.DiscSite.PassNull() != string.Empty)
                                                siteID2 = _objUser.DiscSite;
                                            else
                                                siteID2 = free.FreeITemSiteID;

                                            uom2 = free.UnitDescr;
                                            budgetID2 = free.FreeItemBudgetID;
                                            if (!goto_CalcC)
                                                freeItemQty2 = 0;

                                            if (freeItemQty > 0 || freeItemQty2 > 0)
                                            {
                                                freeItemQty2 = freeItemQty2 + freeItemQty * (qtyAmt / qtyBreak).ToInt();
                                                if (freeItemQty > 0 && countRow == lstDiscFreeItem2tmp.Count)
                                                {
                                                    if (qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak > 0 &&
                                                        _objOM.ProrateDisc != 0)
                                                    {
                                                        qtyAmt = qtyAmt - (qtyAmt / qtyBreak).ToInt() * qtyBreak;
                                                        goto_CalcC = true;
                                                    }
                                                    else
                                                        goto_CalcC = false;
                                                }
                                                freeItemID2 = free.FreeItemID;
                                                double cnvFact = 0;
                                                string unitMultDiv = string.Empty;
                                                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID2);
                                                if (objInvt == null) objInvt = new IN_Inventory();
                                                IN_UnitConversion cnv = SetUOM(freeItemID2, objInvt.ClassID, objInvt.StkUnit, uom2);
                                                if (cnv != null)
                                                {
                                                    cnvFact = cnv.CnvFact;
                                                    unitMultDiv = cnv.MultDiv;
                                                }
                                                if (invtID2 != freeItemID2)
                                                    boType = "S";
                                                if (boType == "S" && CheckQtyAvail(freeItemID2, siteID2, unitMultDiv, freeItemQty2, cnvFact, true, "") == 1)
                                                {
                                                    Util.AppendLog(ref _logMessage, "1045", parm: new[] { freeItemID2, siteID2 });
                                                    freeItemQty2 = 0;
                                                    goto NextFreeItem2;
                                                }
                                                if (freeItemQty2 > 0)
                                                {
                                                    if (!CheckAvailableDiscBudget(ref budgetID2, ref discID2,
                                                            ref discSeq2, ref freeItemQty2,
                                                            true, false, currentLineRef, true,
                                                            true, freeItemID2, free.UnitDescr))
                                                    {
                                                        freeItemQty2 = 0;
                                                        freeItemID2 = string.Empty;
                                                        budgetID2 = string.Empty;
                                                    }
                                                    else
                                                    {
                                                        InsertUpdateOrdDisc(discID2, discSeq2, budgetID2, objDisc.DiscType, seq.DiscFor, 0, 0, 0, seq.BudgetID, freeItemID2, freeItemQty2, discLineRef, lineRef, breakLineRef2);
                                                        discLineRef = (discLineRef.ToInt() + 1).ToString();
                                                        for (int t = discLineRef.Length; discLineRef.Length < 5; )
                                                            discLineRef = "0" + discLineRef;

                                                        if (freeItemID2 != string.Empty && freeItemQty2 > 0)
                                                        {
                                                            AddFreeItem(discID2, discSeq2, freeItemID2, freeItemQty2, siteID2, uom2, lineRef, budgetID2, boType);
                                                            lineRef = (lineRef.ToInt() + 1).ToString();
                                                            for (int t = lineRef.Length; lineRef.Length < 5; )
                                                                lineRef = "0" + lineRef;
                                                        }
                                                    }
                                                } //if(freeItemQty2>0)
                                                return;
                                            } //if(freeItemQty>0 || freeItemQty2>0)
                                        NextFreeItem2:
                                            int a;
                                        } //foreach (var free in lstDiscFreeItem2tmp)
                                        if (goto_CalcC)
                                            goto CalcC;
                                    } //f (lstDiscFreeItem2tmp.Count > 0)
                                } //if(calcDiscC)   
                            NextDiscSeqC:
                                int b;
                            } //  foreach (var seq in lstDiscSeq)
                        } //if(objDisc.DiscType=="L")
                    } //if(freeItemQty1>0 && objDisc!=null && discID1!=string.Empty && discID2==string.Empty)
                } // if(objDisc!=null)     
            } //foreach (var setup in lstSetup)
        }

        private void AddFreeItem(string discID, string discSeq, string freeItemID, double qty, string siteID, string uom, string lineRef, string budgetID, string boType)
        {
            OM_Discount objDisc = _app.OM_Discount.FirstOrDefault(p => p.DiscID == discID);
            double cnvFact = 0;
            string unitMultDiv = string.Empty;
            double price = 0;
            string chk = string.Empty;
            double soFee = 0;
            double stkQty = 0;
            if (freeItemID != string.Empty && qty > 0)
            {
                IN_Inventory objInvt = _app.IN_Inventory.FirstOrDefault(p => p.InvtID == freeItemID);
                if (objInvt == null) objInvt = new IN_Inventory();
                IN_UnitConversion cnv = SetUOM(freeItemID, objInvt.ClassID, objInvt.StkUnit, uom);
                if (cnv != null)
                {
                    cnvFact = cnv.CnvFact;
                    unitMultDiv = cnv.MultDiv;
                }
                if (unitMultDiv == "M")
                    stkQty = qty * cnvFact;
                else
                {
                    if (cnvFact != 0)
                        stkQty = qty / cnvFact;
                    else
                        stkQty = 0;
                }
                if (_objOM.InlcSOFeeProm)
                    soFee = objInvt.SOFee * stkQty;
                else
                    soFee = 0;

                var newdet = new OM10100_pgOrderDet_Result();
                newdet.OrderNbr = _objOrder.OrderNbr;
                newdet.BranchID = _objOrder.BranchID;
                newdet.LineRef = lineRef;
                if (boType == "O")
                    newdet.BOType = "O";
                else
                    newdet.BOType = "S";

                newdet.BudgetID1 = budgetID;
                newdet.Descr = objInvt.Descr;
                newdet.DiscID1 = discID;
                newdet.DiscSeq1 = discSeq;
                newdet.FreeItemQty1 = qty;
                newdet.FreeItem = true;
                newdet.InvtID = freeItemID;
                newdet.ItemPriceClass = objInvt.PriceClassID;
                newdet.BarCode = objInvt.BarCode;
                newdet.LineQty = qty;
                newdet.DumyLineQty = qty;
                newdet.StkQty = stkQty;
                newdet.OrderType = _objOrder.OrderType;
                newdet.SiteID = siteID;
                newdet.SOFee = soFee;
                newdet.SlsUnit = uom;
                newdet.TaxCat = objInvt.TaxCat;
                newdet.TaxID = "*";
                newdet.UnitRate = cnvFact;
                newdet.UnitMultDiv = unitMultDiv;


                if (_objOM.DfltSalesPrice == "I")
                {
                    price = unitMultDiv == "M" ? objInvt.SOPrice * cnvFact : objInvt.SOPrice / cnvFact;
                    newdet.SlsPrice = price;
                    _lstOldOrdDet.Add(newdet);
                }
                else
                {
                    if (_objCust == null) _objCust = new AR_Customer();

                    var priceData = _lstPrice.FirstOrDefault(p => p.InvtID == newdet.InvtID && p.Unit == newdet.SlsUnit);

                    newdet.SlsPrice = priceData.Price.ToDouble();

                    _lstOldOrdDet.Add(newdet);
                }

                if (objInvt.LotSerTrack.PassNull() != string.Empty && objInvt.LotSerTrack != "N")
                {
                    for (int i = _lstLot.Count - 1; i >= 0; i--)
                    {
                        if (_lstLot[i].OMLineRef == newdet.LineRef)
                        {
                            _lstLot.RemoveAt(i);
                        }
                    }
                    OM10100Entities app = Util.CreateObjectContext<OM10100Entities>();
                    var lstItemLot = app.IN_ItemLot.Where(p => p.SiteID == newdet.SiteID && p.InvtID == newdet.InvtID && p.QtyAvail > 0).ToList();

                    List<OM_LotTrans> lstLotTrans = app.OM_LotTrans.Where(p => p.BranchID == _objOrder.BranchID && p.OrderNbr == _objOrder.OrderNbr && p.InvtID == newdet.InvtID && p.SiteID == newdet.SiteID).ToList();
                    foreach (var item in lstLotTrans)
                    {
                        var lot = lstItemLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                        if (lot == null)
                        {
                            var lotDB = app.IN_ItemLot.FirstOrDefault(p => p.SiteID == newdet.SiteID && p.InvtID == newdet.InvtID && p.LotSerNbr == item.LotSerNbr);
                            lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                            lstItemLot.Add(lotDB);
                        }
                        else
                        {
                            lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        }
                    }
                    lstItemLot = lstItemLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();
                    double needQty = needQty = newdet.UnitMultDiv == "M" ? newdet.LineQty * newdet.UnitRate : newdet.LineQty / newdet.UnitRate;

                    foreach (var item in lstItemLot)
                    {
                        double newQty = 0;
                        double curQty = 0;
                        foreach (var item2 in _lstLot)
	                    {
		                    if(item.SiteID == item2.SiteID && item.InvtID == item2.InvtID && item.LotSerNbr == item2.LotSerNbr)
                            {
                                 curQty += item2.UnitMultDiv == "M" ? item2.Qty * item2.CnvFact : item2.Qty * item2.CnvFact;
                            }
	                    }

                        if (Math.Round(item.QtyAvail - curQty,0) == 0) continue;

                        if ((item.QtyAvail - curQty) >= needQty)
                        {
                            newQty = needQty;
                            needQty = 0;
                        }
                        else
                        {
                            newQty = (item.QtyAvail - curQty);
                            needQty -= (item.QtyAvail - curQty);
                            item.QtyAvail = 0;
                        }

                        if (newQty != 0) {
                            var newLot = new OM_LotTrans();
                            newLot.ResetET();
                            newLot.BranchID = _objOrder.BranchID;
                            newLot.OrderNbr = _objOrder.OrderNbr;
                            newLot.LotSerNbr = item.LotSerNbr;
                            newLot.ExpDate = item.ExpDate;
                           
                            newLot.OMLineRef = newdet.LineRef;
                            newLot.SiteID = newdet.SiteID;
                            newLot.InvtID = newdet.InvtID;
                            newLot.InvtMult = -1;

                            if ((newdet.UnitMultDiv == "M" ? newQty / newdet.UnitRate : newQty * newdet.UnitRate) % 1 > 0) {
                                newLot.CnvFact = 1;
                                newLot.UnitMultDiv = "M";
                                newLot.Qty = newQty;
                                newLot.UnitDesc = objInvt.StkUnit;
                                if (_objOM.DfltSalesPrice == "I") {
                                    price = Math.Round(newLot.UnitMultDiv == "M" ? objInvt.SOPrice * newLot.CnvFact : objInvt.SOPrice / newLot.CnvFact,0);
                                    newLot.UnitPrice = price;
                                    newLot.UnitCost = price;
                                } else {
                                    var priceLot = _lstPrice.FirstOrDefault(p => p.InvtID == newLot.InvtID && p.Unit == newLot.UnitDesc);

                                    newLot.UnitPrice = priceLot.Price.Value;
                                    newLot.UnitCost = priceLot.Price.Value;
                                }
                              
                            } else {
                                newLot.Qty = Math.Round(newdet.UnitMultDiv == "M" ? newQty / newdet.UnitRate : newQty * newdet.UnitRate,0);
                                newLot.CnvFact = newdet.UnitRate;
                                newLot.UnitMultDiv = newdet.UnitMultDiv;
                                newLot.UnitPrice = newdet.SlsPrice;
                                newLot.UnitCost = newdet.SlsPrice;
                                newLot.UnitDesc = newdet.SlsUnit;
                            }

                            _lstLot.Add(newLot);
                        }



                        if (needQty == 0) break;
                    }
                    


                }
            }
        }
        #endregion

        #region Source
        public ActionResult GetOrder(string branchID, string orderType, string query, int start, int limit, int page)
        {
            query = query ?? string.Empty;
            if (page != 1) query = string.Empty;
            var lstOrder = _app.OM10100_pcOrder(branchID, orderType, query, start, start + 20).ToList();
            var paging = new Paging<OM10100_pcOrder_Result>(lstOrder, lstOrder.Count > 0 ? lstOrder[0].TotalRecords.Value : 0);
            return this.Store(paging.Data, paging.TotalRecords);
        }
        public ActionResult GetUnitConversion(string branchID)
        {
            var lstUnit = _app.OM10100_pcUnitConversion(branchID).ToList();
            return this.Store(lstUnit);
        }
        public ActionResult GetPrice(string branchID, string custID, DateTime? orderDate)
        {
            List<OM10100_pdSOPrice_Result> lstPrice = _app.OM10100_pdSOPrice(branchID, custID, ""
                , orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd")).ToList();
            return this.Store(lstPrice, lstPrice.Count);
        }
        public ActionResult GetTax(string branchID)
        {
            List<OM10100_pcTax_Result> lstTax = _app.OM10100_pcTax(branchID).ToList();
            return this.Store(lstTax, lstTax.Count);
        }
        public ActionResult GetCustomer(string branchID, string custID)
        {
            List<AR_Customer> lstCust = _app.AR_Customer.Where(p => p.BranchID == branchID && p.CustId == custID).ToList();
            return this.Store(lstCust, lstCust.Count);
        }
        public ActionResult GetShipToID(string branchID, string custID)
        {
            List<OM10100_pcShipToId_Result> lstShip = _app.OM10100_pcShipToId(custID, branchID).ToList();
            return this.Store(lstShip, lstShip.Count);
        }
        public ActionResult GetInvt(string branchID)
        {
            List<OM10100_pcInvt_Result> lstInvt = _app.OM10100_pcInvt(branchID).ToList();
            return this.Store(lstInvt, lstInvt.Count);
        }
        public ActionResult GetSOAddress(string branchID, string custID, string shipToID)
        {
            List<AR_SOAddress> lstAddress = _app.AR_SOAddress.Where(p => p.BranchID == branchID && p.CustId == custID && p.ShipToId == shipToID).ToList();
            return this.Store(lstAddress, lstAddress.Count);
        }
        public ActionResult GetOrdDet(string branchID, string orderNbr)
        {
            List<OM10100_pgOrderDet_Result> lstDet = _app.OM10100_pgOrderDet(branchID, orderNbr, "%").ToList();
            return this.Store(lstDet, lstDet.Count);
        }
        public ActionResult GetTaxTrans(string branchID, string orderNbr)
        {
            List<OM10100_pgTaxTrans_Result> lstTax = _app.OM10100_pgTaxTrans(branchID, orderNbr).ToList();
            return this.Store(lstTax, lstTax.Count);
        }
        public ActionResult GetOrdDisc(string branchID, string orderNbr)
        {
            List<OM_OrdDisc> lstDisc = _app.OM_OrdDisc.Where(p => p.BranchID == branchID && p.OrderNbr == orderNbr).ToList();
            return this.Store(lstDisc, lstDisc.Count);
        }
        public ActionResult GetOrdAddr(string branchID, string orderNbr)
        {
            List<OM_OrdAddr> lstOrdAddr = _app.OM_OrdAddr.Where(p => p.BranchID == branchID && p.OrderNbr == orderNbr).ToList();
            return this.Store(lstOrdAddr, lstOrdAddr.Count);
        }
        public ActionResult GetINSetup(string branchID)
        {
            var objSetup = _app.IN_Setup.FirstOrDefault(p => p.SetupID == "IN" && p.BranchID == branchID);
            return this.Store(objSetup);
        }
        public ActionResult GetOMSetup()
        {
            var objSetup = _app.OM_Setup.FirstOrDefault(p => p.SetupID == "OM");
            return this.Store(objSetup);
        }
        public ActionResult GetUserDefault(string branchID)
        {
            string userName = Current.UserName;
            var objUser = _app.OM_UserDefault.FirstOrDefault(p => p.UserID == userName && p.DfltBranchID == branchID);
            return this.Store(objUser);
        }
        public ActionResult GetItemSite(string siteID, string invtID, string branchID)
        {
            List<OM10100_pdItemSite_Result> lstSite = _app.OM10100_pdItemSite(branchID, invtID, siteID).ToList();
            return this.Store(lstSite);
        }
        public ActionResult GetDiscCode(string branchID, string orderNbr, DateTime orderDate)
        {
            List<OM10100_pcDiscCode_Result> lstDisc = _app.OM10100_pcDiscCode(orderDate, orderNbr, branchID).ToList();
            return this.Store(lstDisc, lstDisc.Count);
        }
        public ActionResult GetLot(string invtID, string siteID, string orderNbr, string branchID, bool all)
        {
            List<IN_ItemLot> lstLot = new List<IN_ItemLot>();
            if (all)
            {
                List<IN_ItemLot> lstLotDB = _app.IN_ItemLot.Where(p => p.SiteID == siteID && p.InvtID == invtID).ToList();
                foreach (var item in lstLotDB)
                {
                    lstLot.Add(item);
                }
            }
            else
            {
                List<IN_ItemLot> lstLotDB = _app.IN_ItemLot.Where(p => p.SiteID == siteID && p.InvtID == invtID && p.QtyAvail > 0).ToList();

                foreach (var item in lstLotDB)
                {
                    lstLot.Add(item);
                }

                List<OM_LotTrans> lstLotTrans = _app.OM_LotTrans.Where(p => p.BranchID == branchID && p.OrderNbr == orderNbr && p.InvtID == invtID && p.SiteID == siteID).ToList();
                foreach (var item in lstLotTrans)
                {
                    var lot = lstLot.FirstOrDefault(p => p.LotSerNbr == item.LotSerNbr);
                    if (lot == null)
                    {
                        var lotDB = _app.IN_ItemLot.FirstOrDefault(p => p.SiteID == siteID && p.InvtID == invtID && p.LotSerNbr == item.LotSerNbr);
                        lotDB.QtyAvail = item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                        lstLot.Add(lotDB);
                    }
                    else
                    {
                        lot.QtyAvail += item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact;
                    }


                }
                lstLot = lstLot.OrderBy(p => p.ExpDate).ThenBy(p => p.LotSerNbr).ToList();
            }
           
           
            return this.Store(lstLot.OrderBy(p=>p.LotSerNbr).ToList(), lstLot.Count);
        }
        public ActionResult GetLotTrans(string branchID, string orderNbr)
        {
            List<OM_LotTrans> lstLotTrans = _app.OM_LotTrans.Where(p => p.BranchID == branchID && p.OrderNbr == orderNbr).ToList();
            return this.Store(lstLotTrans.OrderBy(p => p.LotSerNbr).ToList(), lstLotTrans.Count);
        }
        public ActionResult GetItemLot(string invtID, string siteID, string lotSerNbr, string branchID, string orderNbr)
        {
            var lot = _app.IN_ItemLot.FirstOrDefault(p => p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr);

            if (lot == null) lot = new IN_ItemLot()
            {
                InvtID = invtID,
                SiteID = siteID,
                LotSerNbr = lotSerNbr
            };

            var lotTrans = _app.OM_LotTrans.Where(p => p.BranchID == branchID && p.OrderNbr == orderNbr && p.InvtID == invtID && p.SiteID == siteID && p.LotSerNbr == lotSerNbr).ToList();

            foreach (var item in lotTrans)
            {
                lot.QtyAvail += (item.UnitMultDiv == "M" ? item.Qty * item.CnvFact : item.Qty / item.CnvFact);
            }

            List<IN_ItemLot> lstLot = new List<IN_ItemLot>() { lot };
            return this.Store(lstLot, lstLot.Count);
        }
        #endregion
     
        

    }
}

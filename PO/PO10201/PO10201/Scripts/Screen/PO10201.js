//// Declare //////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var _keys = ['InvtID'];
var _fieldsCheckRequire = ["InvtID", "PurchaseType", "SiteID", "RcptUnitDescr"];
var _fieldsLangCheckRequire = ["InvtID", "PurchaseType", "SiteID", "RcptUnitDescr"];

var _objUserDflt = null;
var _objPO_Setup = null;
var _objrecordTran = null;
var _objrecordinvt = null;

var _invtID = "";
var _classID = "";
var _stkUnit = "";
var _purUnit = "";
var _siteID = "";
var _lineRef = "";

var _objIN_ItemSite = null;


//////////////////////////////////////////////////////////////////
//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var loadDataHeader = function (sto) {
    
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));   
    HQ.common.setForceSelection(App.frmMain, false, "cboBranchID,cboBatNbr,cboHandle");
    HQ.isFirstLoad = true;
    
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "PONbr");
        var record = sto.getAt(0);
        //gan du lieu mac dinh ban dau
        //record.data.ShiptoType = "D";
        record.data.Status = "H";
        record.data.RcptType = "X";
        record.data.RcptDate = HQ.bussinessDate;
        record.data.InvcDate = HQ.bussinessDate;
        record.data.DocDate = HQ.bussinessDate;
        record.data.RcptFrom = _objPO_Setup.DfltRcptFrom;
        record.data.DocType = "VO";
        record.data.Descr =_objPO_Setup.DfltRcptFrom=="DR"? "Directly Receipt/Return":"";
        HQ.isNew = true;//record la new    
        HQ.common.setRequire(App.frmMain);  //to do cac o la require   
        if (App.cboBranchID.valueModels != undefined && App.cboBranchID.value != null && !App.cboBatNbr.hasFocus)
            App.cboBatNbr.focus();//focus ma khi tao moi
        else  if (App.cboBranchID.valueModels == undefined) App.cboBranchID.focus();//focus branch khi branch =''

        sto.commitChanges();
    }
    var record = sto.getAt(0);

    HQ.currRecord = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record); 
    HQ.isChange = false;
    HQ.common.changeData(HQ.isChange, 'PO10201');
   
    App.stoPO10201_pgDetail.reload();
    App.stoPO10201_pgLoadTaxTrans.reload();
    App.grdTaxTrans.getView().refresh();
    calcDet();
    if (App.stoPO10201_pdPO_Setup.data.length == 0) {// chua cai dat PO_Setup thong bao 
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'PO10201');
        lockControl(true);
        App.cboBranchID.setReadOnly(false);
    }
    App.cboHandle.setValue("N");
    HQ.common.showBusy(false);
    if (record.data.RcptFrom == "PO") {
        HQ.grid.show(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
    }
    if (record.data.RcptFrom == "DR") {
        App.cboPONbr.allowBlank = true;
        App.cboPONbr.validate();
    }
    else {
        App.cboPONbr.allowBlank = false;
        App.cboPONbr.validate();
    }
};
var loadDataDetail = function (sto) {
    //neu sto da co du lieu thi ko duoc sua cac combo ben duoi
    if (sto.data.length > 0) {
        App.cboRcptFrom.setReadOnly(true);
        App.cboRcptType.setReadOnly(true);
        App.cboVendID.setReadOnly(true);
        App.cboPONbr.setReadOnly(true);
    }
    else {
        App.cboRcptFrom.setReadOnly(false);
        App.cboRcptType.setReadOnly(false);
        App.cboVendID.setReadOnly(false);
      
    }
    if (HQ.isFirstLoad) {
        HQ.store.insertBlank(sto, _keys);
        HQ.isFirstLoad = false;
    }
    calcDet();
    frmChange();
    HQ.common.showBusy(false);
    App.stoLotTrans.reload();
};

var loadPO10201_pdSI_Tax = function () {
    if (App.stoPO10201_pdSI_Tax.getCount() > 0) {
    
    }
};
var loadPO10201_pdIN_Inventory = function () {
   
};
var loadPO10201_pdIN_UnitConversion = function () {
    if (App.stoPO10201_pdIN_UnitConversion.getCount() > 0) {
       
    }
};
var loadstoPO10201_pgLoadTaxTrans = function () {
    App.stoPO10201_LoadTaxDoc.clearData();
    calcTaxTotal();
};
//////////////////////////////////////////////////////////////////
//// Event ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboBatNbr, HQ.isChange);             
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.first(App.grdDetail);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.next(App.grdDetail);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.prev(App.grdDetail);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.last(App.grdDetail);
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoPO10201_pgDetail, _keys, _fieldsCheckRequire, _fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
            break;
        case "delete":
            if (HQ.isDelete && App.cboStatus.value == 'H') {
                if (HQ.isDelete) {
                    if (HQ.focus == 'header') {
                        HQ.message.show(11, '', 'deleteHeader');
                    } else if (HQ.focus == 'grdDetail') {
                        var rowindex = HQ.grid.indexSelect(App.grdDetail);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDetail), ''], 'deleteRecordGrid', true)
                    }
                }
                break;
                
            }
            break;
        case "close":
                HQ.common.close(this);            
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        HQ.isNew = true;
                        App.cboBatNbr.focus();
                        App.cboBatNbr.setValue(null);
                    }
                } else if (HQ.focus == 'grdDetail') {
                    HQ.grid.insert(App.grdDetail, _keys);
                }
            }
            break;
           
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.cboBatNbr.getStore().load(function(){
                    App.stoHeader.reload(); 
                });                            
            }
            break;
           
            break;
        case "print":
            if (App.frmMain.isValid()) {
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("LoadReporting"),
                    method: 'POST',
                    url: 'PO10201/Report',
                    timeout: 180000,
                    success: function (msg, data) {
                        if (this.result.reportID != null) {

                            window.open('Report?ReportName=' + this.result.reportName + '&_RPTID=' + this.result.reportID, '_blank')
                        }

                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
            break;
        default:
    }
};
//load lần đầu khi mở
var firstLoad = function () {    
    App.cboDocType.getStore().load(function () {
        App.cboBranchID.getStore().load(function () {
            App.cboBranchID.setValue(HQ.cpnyID);
        });
    });   
};
var frmChange = function (sender) {
    if (App.stoHeader.data.length > 0 && App.cboBranchID.getValue()!=null) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoHeader) == false ? HQ.store.isChange(App.stoPO10201_pgDetail) : true;
        HQ.common.changeData(HQ.isChange, 'PO10201');//co thay doi du lieu gan * tren tab title header

        App.cboBranchID.setReadOnly(HQ.isChange);
        App.cboBatNbr.setReadOnly(HQ.isChange);
    }
    else {
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'PO10201');
    }
    if (App.stoPO10201_pdPO_Setup.data.length == 0) {
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'PO10201');
        lockControl(true);
        App.cboBranchID.setReadOnly(false);
    }
    if (HQ.store.isChange(App.stoPO10201_pgDetail) || !Ext.isEmpty(App.txtRcptNbr.getValue()) || App.stoPO10201_pgDetail.data.length>1) {
        App.cboRcptFrom.setReadOnly(true);
        App.cboRcptType.setReadOnly(true);
        App.cboPONbr.setReadOnly(true);
        App.cboVendID.setReadOnly(true);
    }
    else {
        App.cboRcptFrom.setReadOnly(false);
        App.cboRcptType.setReadOnly(false);      
        App.cboVendID.setReadOnly(false);
        if (App.cboRcptFrom.getValue() == "PO") {
            App.cboPONbr.setReadOnly(false);
        }
    }
    
    //if (App.cboStatus.getValue() != "H") App.btnImport.disable();
    //if (_objPO_Setup == null) {
    //    App.btnImport.disable();

    //}
    //else if (HQ.util.passNull(App.cboBranchID.getValue()) == "") {
    //    App.btnImport.disable();
    //}
    //else if (HQ.util.passNull(App.cboBatNbr.getValue()) == "" && _objPO_Setup.AutoRef == 0) {
    //    App.btnImport.disable();
    //}
    //else if (HQ.util.passNull(App.cboVendID.getValue()) == "") {
    //    App.btnImport.disable();

    //}
    //else if (App.dtRcptDate.getValue() == "") {
    //    App.btnImport.disable();
    //}
    //else if (App.cboDistAddr.getValue() == "") {
    //    App.btnImport.disable();
    //}
    //else App.btnImport.enable();
};
var grdPO_Trans_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != "H" || !App.winLot.hidden) return false;
    if (_objPO_Setup == null) {
        HQ.message.show(20404, 'PO_Setup', '');
        return false;

    }
    else if (HQ.util.passNull(App.cboBatNbr.getValue()) == "" && _objPO_Setup.AutoRef == 0) {
        HQ.message.show(15, App.cboBatNbr.fieldLabel, '');
        return false;
    }
    else if (HQ.util.passNull(App.cboVendID.getValue()) == "") {
        HQ.message.show(41, '', '');
        return false;

    }
    else if (App.dtRcptDate.getValue() == "") {
        HQ.message.show(15, App.dtRcptDate.fieldLabel, '');
        return false;

    }
   
    var det = e.record.data;
    _purUnit = e.record.data.RcptUnitDescr;
  
    if (det.PurchaseType == "") {
        e.record.set("PurchaseType", "GI");
        e.record.set("SiteID", _objUserDflt == undefined ? "" : _objUserDflt.POSite);
        e.record.set("VouchStage", 'N');
        e.record.set("RcptStage", 'N');
        var valueTax = '';
        App.cboTaxID.getStore().data.each(function (det) {
            valueTax += det.data.taxid + ',';

        });
        valueTax = valueTax.length > 0 ? valueTax.substring(0, valueTax.length - 1) : '';
        e.record.set("TaxID", valueTax);

        e.record.set("ReqdDate", HQ.bussinessDate);
        e.record.set("PromDate", HQ.bussinessDate);
        return false;
    }
    if (e.field != "InvtID" && e.field != "PurchaseType" && Ext.isEmpty(det.InvtID)) {
        return false;
    }
    if (det.PurchaseType == "" && e.column.dataIndex != "PurchaseType") {
        HQ.message.show(15, e.grid.columns[1].text, '');
        return false;
    }
    if (!_objPO_Setup.EditablePOPrice && e.column.dataIndex == "UnitCost") {
        return false;
    }
    if (Ext.isEmpty(det.LineRef)) {
        e.record.set('LineRef', lastLineRef(App.stoPO10201_pgDetail));
    }
    if (e.field == 'RcptUnitDescr' || e.field == 'SiteID') {
       
        var objIN_Inventory = HQ.store.findInStore(App.stoPO10201_pdIN_Inventory, ["InvtID"], [det.InvtID]);
        _invtID = objIN_Inventory.InvtID;
        _classID = objIN_Inventory.ClassID;
        _stkUnit = objIN_Inventory.StkUnit;
        App.cboRcptUnitDescr.getStore().reload();
    }
   
    if (e.field == "RcptQty" || e.field == 'RcptUnitDescr' || e.field == "InvtID" || e.field == "SiteID") {
        if (App.cboRcptType.getValue() == "X" && (det.PurchaseType == "GI" | det.PurchaseType == "PR" | det.PurchaseType == "GS")) {
            HQ.common.showBusy(true);
            App.direct.PO10201ItemSitePrice(App.cboBranchID.getValue(), det.InvtID, det.SiteID,
               {
                   success: function (result) {
                       _objIN_ItemSite = result;
                       if (!Ext.isEmpty(_objIN_ItemSite)) {
                           App.lblQtyAvail.setText(det.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + _objIN_ItemSite.QtyAvail);
                       }
                       else {
                           App.lblQtyAvail.setText(det.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
                       }
                       HQ.common.showBusy(false);
                   },
                   failure: function (result) {
                       HQ.common.showBusy(false);
                   }

               });

        }
    }

};
var grdPO_Trans_ValidateEdit = function (item, e) {
    if (App.cboStatus.getValue() != "H" || !App.winLot.hidden) return false;
    var Qty = 0;
    var objdet = e.record;// App.slmPO_Trans.getSelection()[0];
    if (_keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdDetail, e, _keys)) {
            HQ.message.show(1112, e.value, '');
            return false;
        }
    }
    

    if (e.field == "InvtID") {
        var r = HQ.store.findInStore(App.cboInvtID.getStore(), ["InvtID"], [e.value]);


        if (r == undefined) {
            objdet.set('TranDesc', "");
            objdet.set('RcptUnitDescr', "");
            //objdet.set('TranDesc', "");
        }
        else {
            var objIN_Inventory = HQ.store.findInStore(App.stoPO10201_pdIN_Inventory, ["InvtID"], [r.InvtID]);
            _invtID = objIN_Inventory.InvtID;
            _classID = objIN_Inventory.ClassID;
            _stkUnit = objIN_Inventory.StkUnit;

            App.cboRcptUnitDescr.getStore().reload();

            if (objdet.get("SiteID") == "") {
                if (_objUserDflt != null) {
                    objdet.set('SiteID', _objUserDflt.POSite);
                }
                else if (App.cboShipSiteID.getStore().getCount() > 0) {
                    objdet.set('SiteID', App.cboShipSiteID.getStore().getAt(0).data.SiteID);
                }
                else {
                    objdet.set('SiteID', objIN_Inventory.DfltSite);
                }

            }
            objdet.set('TaxCat', objIN_Inventory.TaxCat == null ? "" : objIN_Inventory.TaxCat);
            objdet.set('RcptUnitDescr', objIN_Inventory.DfltPOUnit == null ? "" : objIN_Inventory.DfltPOUnit);
            objdet.set('UnitWeight', objIN_Inventory.StkWt);
            objdet.set('UnitVolume', objIN_Inventory.StkVol);
            objdet.set('TranDesc', r.Descr);
        }
    }
    if (e.field == "RcptQty" || e.field == "RcptUnitDescr") {
        var RcptUnitDescr = e.field == "RcptUnitDescr" ? e.value : objdet.data.RcptUnitDescr;
        var RcptQty = e.field == "RcptQty" ? e.value : objdet.data.RcptQty;

        var objIN_Inventory = HQ.store.findInStore(App.stoPO10201_pdIN_Inventory, ["InvtID"], [objdet.data.InvtID]);
        var cnv = setUOM(objIN_Inventory.InvtID, objIN_Inventory.ClassID, objIN_Inventory.StkUnit, RcptUnitDescr);

        if (!Ext.isEmpty(cnv)) {
            e.record.set('RcptConvFact', cnv.CnvFact);
            e.record.set('RcptMultDiv', cnv.MultDiv);
        } else {
            e.record.set('RcptUnitDescr', "");
            return;
        }
        if (objdet.data.RcptMultDiv == "M") {
            Qty = RcptQty * objdet.data.RcptConvFact;
        }
        else {
            Qty = RcptQty / objdet.data.RcptConvFact == 0 ? 1 : objdet.data.RcptConvFact;
        }
        if (objdet.data.PONbr != '' && App.cboRcptType.getValue() != "X") {
            if (Qty > (objdet.data.UnitMultDiv == "M" ? objdet.data.Qty * objdet.data.CnvFact : objdet.data.Qty / objdet.data.CnvFact)) {
                HQ.message.show(20150326, '', '');
                return false;
            }
        }
        if (App.cboRcptType.getValue() == "X" && (objdet.data.PurchaseType == "GI" | objdet.data.PurchaseType == "PR" | objdet.data.PurchaseType == "GS")) {
            if (objdet.data.PONbr != '') {
                if (Qty > (objdet.data.UnitMultDiv == "M" ? objdet.data.Qty * objdet.data.CnvFact : objdet.data.Qty / objdet.data.CnvFact)) {
                    HQ.message.show(201503261, '', '');
                    return false;
                }
            }

            QtyAvail = _objIN_ItemSite == null ? 0 : _objIN_ItemSite.QtyAvail;
            //var QtyTot = Qty;//+ CalculateInvtTotals(dtIntrans, objDetail.InvtID, objDetail.SiteID, objDetail.LineRef);XX vi chi co 1 san pham nen ko tinh lai
            if (Qty > QtyAvail) {
                HQ.message.show(35, '', '');
                objdet.set('RcptQty', 0);
              
                return false;
            }
        }
        if (objdet.data.PurchaseType == "FA") {
            if (e.value > 1) {
                HQ.message.show(58, '', '');
                objdet.set('RcptQty', 1);
              
            }
        }
        //else if (objdet.data.PurchaseType == "GI" | objdet.data.PurchaseType == "PR" | objdet.data.PurchaseType == "GS") {
        //    if (objdet.data.UnitMultDiv == "M") {
        //        objdet.set('Qty', Qty / objdet.data.CnvFact);
              
        //    }
        //    else {
        //        objdet.set('Qty', Qty * objdet.data.CnvFact);
            
        //    }
        //}
    }
}
var grdPO_Trans_Edit = function (item, e) {
    var objDetail = e.record.data;
    var objIN_Inventory = HQ.store.findInStore(App.stoPO10201_pdIN_Inventory, ["InvtID"], [objDetail.InvtID]);
    objIN_Inventory = objIN_Inventory == null ? "" : objIN_Inventory;
  
    if (e.field == "RcptUnitDescr" || e.field == "InvtID") {
        var cnv = setUOM(objIN_Inventory.InvtID, objIN_Inventory.ClassID, objIN_Inventory.StkUnit, objDetail.RcptUnitDescr);

        if (!Ext.isEmpty(cnv)) {
           
            objDetail.RcptConvFact = cnv.CnvFact;
            objDetail.RcptMultDiv = cnv.MultDiv;
            e.record.set('RcptConvFact', cnv.CnvFact);
            e.record.set('RcptMultDiv', cnv.MultDiv);
        } else {
            e.record.set('RcptUnitDescr', "");
            return;
        }
        if (objDetail.PONbr=="") {
            var cnv1 = setUOM(objIN_Inventory.InvtID, objIN_Inventory.ClassID, objIN_Inventory.StkUnit, objIN_Inventory.DfltPOUnit);
            if (!Ext.isEmpty(cnv1)) {
                objDetail.CnvFact = cnv1.CnvFact;
                objDetail.UnitMultDiv = cnv1.MultDiv;
                e.record.set('CnvFact', cnv1.CnvFact);
                e.record.set('UnitMultDiv', cnv1.MultDiv);
                e.record.set('UnitDescr', objIN_Inventory.DfltPOUnit);
            } else {
                e.record.set('UnitDescr', '');
                return;
            }
        }
        HQ.grid.checkInsertKey(App.grdDetail, e, _keys);
    }
    if (e.field == "RcptQty") {
        if (objDetail.PurchaseType == "FA") {
            if (objDetail.RcptQty > 1) {

                HQ.message.show(58, '', '');
                return false;

            }
        }

        StkQty = Math.round((objDetail.RcptMultDiv == "D" ? (objDetail.RcptQty / objDetail.RcptConvFact) : (objDetail.RcptQty * objDetail.RcptConvFact)));
        e.record.set("TranAmt", objDetail.RcptQty * objDetail.UnitCost - objDetail.DocDiscAmt);
        objDetail.POFee = StkQty * objIN_Inventory.POFee;
      
        e.record.set("ExtWeight", objDetail.RcptQty * objDetail.UnitWeight);
        e.record.set("ExtVolume", objDetail.RcptQty * objDetail.UnitVolume);

       
    }
    else if (e.field == "UnitWeight") {
        e.record.set("ExtWeight", objDetail.RcptQty * objDetail.UnitWeight);
        //cap nhat lai don vi gia cho lot trans
        App.stoLotTrans.clearFilter();
        App.stoLotTrans.data.each(function (item) {
            if (item.data.POTranLineRef == objDetail.LineRef) {
                item.data.SiteID = objDetail.SiteID;
                item.data.InvtID = objDetail.InvtID;
                item.data.UnitDesc = objDetail.RcptUnitDescr;
                item.data.UnitCost = objDetail.UnitCost;
                item.data.UnitPrice = objDetail.UnitCost;
                item.data.CnvFact = objDetail.RcptConvFact;
                item.data.UnitMultDiv = objDetail.RcptMultDiv;

            }

        });
    }
    else if (e.field == "UnitCost") {
        e.record.set("TranAmt", objDetail.RcptQty * objDetail.UnitCost - objDetail.DocDiscAmt);


    }
    else if (e.field == "UnitVolume") {
        e.record.set("ExtVolume", objDetail.RcptQty * objDetail.UnitVolume);

    }
    else if (e.field == "DocDiscAmt") {
        e.record.set("TranAmt", objDetail.UnitCost * objDetail.RcptQty - objDetail.DocDiscAmt);
        if (objDetail.RcptQty != 0) {
            e.record.set("DiscPct", HQ.util.mathRound((objDetail.DocDiscAmt / (objDetail.UnitCost * objDetail.RcptQty)) * 100, 2));//Math.round((objDetail.DocDiscAmt / (objDetail.UnitCost * objDetail.RcptQty)) * 100, 2));
        }

    }
    else if (e.field == "DiscPct") {
        if (objDetail.TranAmt != 0) {
            e.record.set("DocDiscAmt", HQ.util.mathRound((objDetail.UnitCost * objDetail.RcptQty * objDetail.DiscPct) / 100, 2));//Math.round((objDetail.UnitCost * objDetail.RcptQty * objDetail.DiscPct) / 100, 2));
        }
        e.record.set("TranAmt", objDetail.UnitCost * objDetail.RcptQty - objDetail.DocDiscAmt);
    }
    else if (e.field == "RcptUnitDescr" || e.field == "InvtID" || e.field == "SiteID") {        
        if (_objPO_Setup.DfltLstUnitCost == "A" || _objPO_Setup.DfltLstUnitCost == "L") {
            HQ.common.showBusy(true);
            App.direct.PO10201ItemSitePrice(
                App.cboBranchID.getValue(), objDetail.InvtID, objDetail.SiteID,
               {
                   success: function (result) {
                       _objIN_ItemSite = result;
                       UnitCost = result == null ? 0 : (_objPO_Setup.DfltLstUnitCost == "A" ? result.AvgCost : result.LastPurchasePrice);
                       UnitCost = Math.round((objDetail.RcptMultDiv == "D" ? (UnitCost / objDetail.RcptConvFact) : (UnitCost * objDetail.RcptConvFact)));
                       e.record.set("UnitCost", UnitCost);
                       e.record.set("TranAmt", UnitCost * objDetail.RcptQty - objDetail.DocDiscAmt);
                       //cap nhat lai don vi gia cho lot trans
                       App.stoLotTrans.clearFilter();
                       App.stoLotTrans.data.each(function (item) {
                           if (item.data.POTranLineRef == objDetail.LineRef) {
                               item.data.SiteID= objDetail.SiteID;
                               item.data.InvtID= objDetail.InvtID;
                               item.data.UnitDesc= objDetail.RcptUnitDescr;
                               item.data.UnitCost= objDetail.UnitCost;
                               item.data.UnitPrice= objDetail.UnitCost;
                               item.data.CnvFact= objDetail.RcptConvFact;
                               item.data.UnitMultDiv= objDetail.RcptMultDiv;
                             
                           }
                                                                            
                       });
                       delTax(e.record);
                       calcTax(e.record);
                       calcTaxTotal();
                       HQ.common.showBusy(false);
                   },
                   failure: function (result) {
                       HQ.common.showBusy(false);
                   }
                          
               });
        }
        else if (_objPO_Setup.DfltLstUnitCost == "P") {
            HQ.common.showBusy(true);
            App.direct.PO10201POPrice(
               App.cboBranchID.getValue(), objDetail.InvtID, objDetail.RcptUnitDescr, Ext.Date.format(App.dtRcptDate.getValue(), 'Y-m-d'),
                {
                    success: function (result) {
                        UnitCost = result;
                        e.record.set("UnitCost", result);
                        e.record.set("TranAmt", result * objDetail.RcptQty - objDetail.DocDiscAmt);
                        App.stoLotTrans.clearFilter();
                        App.stoLotTrans.data.each(function (item) {
                            if (item.data.POTranLineRef == objDetail.LineRef) {
                                item.data.SiteID = objDetail.SiteID;
                                item.data.InvtID = objDetail.InvtID;
                                item.data.UnitDesc = objDetail.RcptUnitDescr;
                                item.data.UnitCost = objDetail.UnitCost;
                                item.data.UnitPrice = objDetail.UnitCost;
                                item.data.CnvFact = objDetail.RcptConvFact;
                                item.data.UnitMultDiv = objDetail.RcptMultDiv;

                            }
                        });
                        delTax(e.record);
                        calcTax(e.record);
                        calcTaxTotal();
                        HQ.common.showBusy(false);
                       
                    },
                    failure: function (result) {
                        HQ.common.showBusy(false);
                    }
                });

        }
        else if (_objPO_Setup.DfltLstUnitCost == "I") {
            var UnitCost = objIN_Inventory.POPrice;
            UnitCost = Math.round((objDetail.RcptMultDiv == "D" ? (UnitCost / objDetail.RcptConvFact) : (UnitCost * objDetail.RcptConvFact)));
            e.record.set("UnitCost", UnitCost);
            e.record.set("TranAmt", UnitCost * objDetail.RcptQty - objDetail.DocDiscAmt);
            App.stoLotTrans.clearFilter();
            App.stoLotTrans.data.each(function (item) {
                if (item.data.POTranLineRef == objDetail.LineRef) {
                    item.data.SiteID = objDetail.SiteID;
                    item.data.InvtID = objDetail.InvtID;
                    item.data.UnitDesc = objDetail.RcptUnitDescr;
                    item.data.UnitCost = objDetail.UnitCost;
                    item.data.UnitPrice = objDetail.UnitCost;
                    item.data.CnvFact = objDetail.RcptConvFact;
                    item.data.UnitMultDiv = objDetail.RcptMultDiv;

                }
            });
            delTax(e.record);
            calcTax(e.record);
            calcTaxTotal();
        }
    }
    if (objDetail.PurchaseType == "PR") {
        e.record.set("UnitCost", 0);
    }
    if ( e.field == "RcptQty" || e.field == "DiscPct" || e.field == "DocDiscAmt" || e.field == "UnitCost" || e.field == "TaxCat" || e.field == "TaxID") {
      
        delTax(e.record);
        calcTax(e.record);
        calcTaxTotal();

    }
   
};
var grdPO_Trans_Deselect = function (item, e) {
    calcDet();
    delTax(e.record);
    calcTaxTotal();
};
var grdPO_Trans_Reject = function (record) {
    if (record.data.tstamp == '') {
    
        delTax(record);
        calcTaxTotal();
        App.grdDetail.getStore().remove(record, App.grdDetail);
        App.grdDetail.getView().focusRow(App.grdDetail.getStore().getCount() - 1);
        App.grdDetail.getSelectionModel().select(App.grdDetail.getStore().getCount() - 1);
        calcDet();
    } else {   
        record.reject();
        delTax(record);
        calcTax(record);
        calcTaxTotal();
        calcDet();

    }
};
var cboGInvtID_Change = function (item, newValue, oldValue) {
    var objdet = App.slmPO_Trans.getSelection()[0];
    App.direct.PO10201ItemSitePrice(App.cboBranchID.getValue(), newValue, objdet.data.SiteID,
          {
              success: function (result) {
                  _objIN_ItemSite = result;
                  if (!Ext.isEmpty(_objIN_ItemSite)) {
                      App.lblQtyAvail.setText(newValue + " - " + HQ.common.getLang('qtyavail') + ":" + _objIN_ItemSite.QtyAvail);
                  }
                  else {
                      App.lblQtyAvail.setText(newValue + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
                  }
              }
          });
};

//cac store co param la branchID thi load lai sau khi cboBranchID thay doi
var cboBranchID_Change = function (item, newValue, oldValue) {
    if (item.valueModels != null && App.cboBranchID.getValue() != null && !item.hasFocus) {//truong hop co chon branchid
        App.txtBranchName.setValue(App.cboBranchID.valueModels[0].data.BranchName);
        _cpnyID = App.cboBranchID.valueModels[0].data.BranchID;
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.stoPO10201_pdOM_UserDefault.load(function () {
            App.stoPO10201_pdPO_Setup.load(function () {
                App.cboVendID.getStore().load(function () {
                //    App.cboShipSiteID.getStore().load(function () {
                //        App.cboShipCustID.getStore().load(function () {
                            App.cboSiteID.getStore().load(function () {
                                _objUserDflt = App.stoPO10201_pdOM_UserDefault.data.length > 0 ? App.stoPO10201_pdOM_UserDefault.getAt(0).data : { POSite: '', };;
                                if (App.stoPO10201_pdPO_Setup.data.length == 0) {
                                    if (item.hasFocus) {
                                        App.cboBatNbr.setValue('');
                                        App.stoHeader.reload();
                                    }
                                    HQ.message.show(20404, 'PO_Setup', '');
                                    lockControl(true);
                                    App.cboBranchID.setReadOnly(false);
                                    HQ.common.showBusy(false);
                                }
                                else {
                                    lockControl(false);
                                    _objPO_Setup = App.stoPO10201_pdPO_Setup.getAt(0).data;
                                    if (_objPO_Setup.AutoRef == 1) App.cboBatNbr.forceSelection = true;
                                    else App.cboBatNbr.forceSelection = false;
                                    App.cboBatNbr.getStore().load(function () {
                                        App.cboBatNbr.setValue('');
                                        App.stoHeader.reload();
                                        //App.cboDistAddr.setValue(App.cboBranchID.getValue());
                                    });
                                }
                               
                            });
                //        });
                //    });
                });
            });
        });
    }
    else { //truong hop khong chon
        if (Ext.isEmpty(App.cboBranchID.getValue())) {
            App.txtBranchName.setValue('');
            _cpnyID = '';
            App.stoPO10201_pdPO_Setup.load(function () {
                App.cboBatNbr.setValue('');
                App.stoHeader.reload();
            });
        }
    }
    
};
var cboBranchID_Select = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.txtBranchName.setValue(App.cboBranchID.valueModels[0].data.BranchName);
        _cpnyID = App.cboBranchID.valueModels[0].data.BranchID;
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.stoPO10201_pdOM_UserDefault.load(function () {
            App.stoPO10201_pdPO_Setup.load(function () {
                App.cboVendID.getStore().load(function () {
                    //    App.cboShipSiteID.getStore().load(function () {
                    //        App.cboShipCustID.getStore().load(function () {
                    App.cboSiteID.getStore().load(function () {
                        _objUserDflt = App.stoPO10201_pdOM_UserDefault.data.length > 0 ? App.stoPO10201_pdOM_UserDefault.getAt(0).data : { POSite: '', };;
                        if (App.stoPO10201_pdPO_Setup.data.length == 0) {
                            if (item.hasFocus) {
                                App.cboBatNbr.setValue('');
                                App.stoHeader.reload();
                            }
                            HQ.message.show(20404, 'PO_Setup', '');
                            lockControl(true);
                            App.cboBranchID.setReadOnly(false);
                            HQ.common.showBusy(false);
                        }
                        else {
                            lockControl(false);
                            _objPO_Setup = App.stoPO10201_pdPO_Setup.getAt(0).data;
                            if (_objPO_Setup.AutoRef == 1) App.cboBatNbr.forceSelection = true;
                            else App.cboBatNbr.forceSelection = false;
                            App.cboBatNbr.getStore().load(function () {
                                App.cboBatNbr.setValue('');
                                App.stoHeader.reload();
                                //App.cboDistAddr.setValue(App.cboBranchID.getValue());
                            });
                        }

                    });
                    //        });
                    //    });
                });
            });
        });
    }
}
var cboBatNbr_Change = function (item, newValue, oldValue) {   
    if ((!HQ.isNew || item.valueModels != null) && !App.stoHeader.loading) {
        App.stoHeader.reload();
    }

};
var cboBatNbr_Select = function (item) {
    if (item.valueModels != null && !App.stoHeader.loading) {
        App.stoHeader.reload();
    }
};
var cboPONbr_Change = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.stoPO10201_pgDetail.clearData();
        App.stoPO10201_pgDetail.loadData([], false);
        App.grdDetail.view.refresh();
        HQ.store.insertBlank(App.stoPO10201_pgDetail, _keys);
        
    }
    if ((HQ.isNew && item.valueModels != null && !App.stoPO10201_ppCheckingPONbr.loading) && !Ext.isEmpty(newValue)) {
        App.stoPO10201_ppCheckingPONbr.load(function () {
            if (App.stoPO10201_ppCheckingPONbr.data.length > 0 ) {
                HQ.message.show(60, '', '');
                App.cboPONbr.setValue('');
            }
            else {
                App.cboTerms.setValue(item.valueModels[0].data.Terms);
                App.cboVendID.setValue(item.valueModels[0].data.VendID);
                App.cboDocType.setValue(App.cboRcptType.getValue() == "R" ? "VO" : "AD");
                App.txtDescr.setValue(HQ.common.getLang("Receipt from vendor:") + item.valueModels[0].data.VendID);
                if (!Ext.isEmpty(App.cboRcptType.getValue()) && App.cboRcptType.getValue() == "R") {
                    App.stoPO10201_pdPODetailReceipt.load(function () {
                     
                        App.stoPO10201_pdPODetailReceipt.data.each(function (det) {
                            insertItemGrid(App.grdDetail, det.data);
                        });
                        App.stoLotTrans.reload();
                        frmChange();
                        App.cboPONbr.setReadOnly(false);
                     
                    });
                } else {

                    App.stoPO10201_pdPODetailReturn.load(function () {
                        
                        App.stoPO10201_pdPODetailReturn.data.each(function (det) {
                           
                            insertItemGrid(App.grdDetail, det.data);
                        });
                        App.stoLotTrans.reload();
                        frmChange();
                        App.cboPONbr.setReadOnly(false);
                    });
                }
            }
        });
    }
};
var cboPONbr_Select = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.stoPO10201_pgDetail.clearData();
        App.stoPO10201_pgDetail.loadData([], false);
        App.grdDetail.view.refresh();
        HQ.store.insertBlank(App.stoPO10201_pgDetail, _keys);
    }
    if (HQ.isNew && item.valueModels != null && !App.stoPO10201_ppCheckingPONbr.loading && !Ext.isEmpty(newValue)) {
       
        App.stoPO10201_ppCheckingPONbr.load(function () {
            if (App.stoPO10201_ppCheckingPONbr.data.length > 0) {
                HQ.message.show(60, '', '');
                App.cboPONbr.setValue('');
            }
            else {
                App.cboTerms.setValue(item.valueModels[0].data.Terms);
                App.cboVendID.setValue(item.valueModels[0].data.VendID);
                App.cboDocType.setValue(App.cboRcptType.getValue() == "R" ? "VO" : "AD");
                App.txtDescr.setValue(HQ.common.getLang("Receipt from vendor:") + item.valueModels[0].data.VendID);
                if (!Ext.isEmpty(App.cboRcptType.getValue()) && App.cboRcptType.getValue() == "R") {
                    App.stoPO10201_pdPODetailReceipt.load(function () {
                      
                        App.stoPO10201_pdPODetailReceipt.data.each(function (det) {
                            insertItemGrid(App.grdDetail,det.data);
                        });
                        frmChange();
                        App.cboPONbr.setReadOnly(false);
                    });
                } else {
                    App.stoPO10201_pdPODetailReturn.load(function () {
                       
                        App.stoPO10201_pdPODetailReturn.data.each(function (det) {
                            insertItemGrid(App.grdDetail, det.data);
                        });
                        frmChange();
                        App.cboPONbr.setReadOnly(false);
                    });
                }
            }
        });
    }
};


//cac store co param la VendID thi load lai sau khi VendID thay doi
var cboVendID_Change = function (item, newValue, oldValue) {
    App.stoPO10201_pdAP_VenDorTaxes.load(function () {
        App.cboTaxID.getStore().load(function () {         
                if (App.cboVendID.hasFocus) {//co chon cboVendID thi gan cboVendAddrID= gia tri chon                                   
                    if (item.valueModels != null) {
                        if (item.valueModels[0] != undefined) {
                            var objVendor = item.valueModels[0].data;
                            App.cboTerms.setValue(objVendor.Terms);
                        }
                        else  App.cboTerms.setValue('');
                    }
                }               

        });
    });
}
var cboRcptType_Change = function (item, newValue, oldValue) {
    if (item.valueModels != null) {
        if (item.valueModels[0] != undefined) {
            if (item.valueModels[0].data.Code == "X")
            {                          
                if (App.cboRcptFrom.getValue() == "PO")
                {
                    HQ.grid.show(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
               
                    //GridPO_Trans.Columns["OrigRcptDate"].Visibility = Visibility.Visible;
                   
                }
                else
                {
                    HQ.grid.hide(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
                }
            }
            else
            {
                HQ.grid.hide(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
                if (App.cboRcptFrom.getValue() == "PO") {
                    HQ.grid.show(App.grdDetail, ["POLineRef", "Qty", "UnitDescr"]);

                    //GridPO_Trans.Columns["OrigRcptDate"].Visibility = Visibility.Visible;

                }
                else {
                    HQ.grid.hide(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
                }

                //HQ.grid.hide(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
           
             
             
                //GridPO_Trans.Columns["OrigRcptDate"].Visibility = Visibility.Collapsed;
              
            }
            App.cboPONbr.getStore().load(function(){

            });                               
        
        }      
    }

}
var cboRcptFrom_Change = function (item, newValue, oldValue) {

    HQ.grid.hide(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
    if (item.valueModels != null) {
        if (item.valueModels[0] != undefined) {
            if (item.valueModels[0].data.Code == "DR" && (App.cboStatus.getValue() == null ? "H" : App.cboStatus.getValue() == "H")) {
                App.cboPONbr.setReadOnly(true);
				App.cboPONbr.setValue('');
                App.cboVendID.setReadOnly(false);
                App.cboPONbr.allowBlank = true;
                App.cboPONbr.validate();
                App.cboDocType.setValue(App.cboRcptType.getValue() == "R" ? "VO" : "AD");
                App.txtDescr.setValue("Directly Receipt/Return");
                if (App.txtRcptNbr.value.length == 0) {
                    HQ.grid.hide(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty","UnitDescr"]);

                }
                if (item.hasFocus)
                    App.cboVendID.focus();
               
            }
            else if (Ext.isEmpty(App.txtRcptNbr.getValue())) {
                App.cboVendID.setValue('');
                App.cboVendID.setReadOnly(true);
                App.cboPONbr.setReadOnly(false);
                App.cboPONbr.allowBlank = false;
                App.cboPONbr.validate();

                if (App.txtRcptNbr.value.length == 0) {
                    if (App.cboRcptType.getValue() == "X") {
                        HQ.grid.show(App.grdDetail, ["POLineRef", "OrigRcptDate", "OrigRcptNbr", "Qty", "UnitDescr"]);
                    }
                    else {
                        HQ.grid.show(App.grdDetail, ["POLineRef",  "Qty", "UnitDescr"]);
                    }
                }

                App.cboPONbr.getStore().load(function () {
                    if (item.hasFocus) App.cboPONbr.focus();
                   
                });

            }
        } else App.cboPONbr.setReadOnly(true);
    } else App.cboPONbr.setReadOnly(true);
}
var cboStatus_Change = function (item, newValue, oldValue) {
    App.cboHandle.getStore().reload();
  
    if (newValue == 'H' && HQ.isInsert && HQ.isUpdate) {
        HQ.common.lockItem(App.frmMain, false);       
    }
    else HQ.common.lockItem(App.frmMain, true);
    {
        App.cboBranchID.setReadOnly(false);
        App.cboHandle.setReadOnly(false);
        App.cboBatNbr.setReadOnly(false);
    }
};


var txtcRcptQty_Change = function (sender) {
    var record = App.slmPO_Trans.selected.items[0];
    var objIN_Inventory = HQ.store.findInStore(App.stoPO10201_pdIN_Inventory, ["InvtID"], [record.data.InvtID]);
    if (objIN_Inventory.LotSerTrack != 'N' && !Ext.isEmpty(objIN_Inventory.LotSerTrack)) {
        showLot(record);

    }

}
var btnLot_Click = function (record) {
    showLot(this.record);   
}
var showLot = function (record) {
    App.winLot.invt = HQ.store.findInStore(App.stoPO10201_pdIN_Inventory, ['InvtID'], [record.data.InvtID]);
    if (!Ext.isEmpty(record.data.InvtID) && !Ext.isEmpty(record.data.RcptUnitDescr) && App.winLot.invt.LotSerTrack != 'N' && !Ext.isEmpty(App.winLot.invt.LotSerTrack)) {

        _classID = App.winLot.invt.ClassID;
        _stkUnit = App.winLot.invt.StkUnit;
        _invtID = App.winLot.invt.InvtID;
        _lineRef = record.data.LineRef;
        _siteID = record.data.SiteID;
      
        PopupWinLot.showLot(record);
    }

}
///////DataProcess///
function save(b714) {//mess714 khi huy
    App.frmMain.getForm().updateRecord();
    App.stoLotTrans.clearFilter();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('SavingData'),
            method: 'POST',
            url: 'PO10201/Save',
            timeout: 1800000,
            params: {
                lstLot: Ext.encode(App.stoLotTrans.getRecordsValues()),
                lstDet: Ext.encode(App.stoPO10201_pgDetail.getRecordsValues()),
                lstHeader: Ext.encode(App.stoHeader.getRecordsValues()),
                b714: b714==undefined?false:true
            },
            success: function (msg, data) {
                var batNbr = '';
                
                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    batNbr = this.result.data.batNbr;                  
                }               
                HQ.message.process(msg, data, true);
            
                App.cboBatNbr.getStore().load(function () {
                    App.cboBatNbr.setValue(batNbr);
                    App.stoHeader.reload();
                });                              
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
function process714(item) {
    if (item == 'yes') {
        save(true);
    }
}
function deleteHeader(item) {
    if (item == 'yes') {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                method: 'POST',
                url: 'PO10201/DeleteHeader',
                timeout: 180000,
                params: {
                    lstHeader: Ext.encode(App.stoHeader.getRecordsValues())

                },
                success: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    var batNbr = '';
                    if (this.result.data != undefined && this.result.data.batNbr != null) {
                        batNbr = this.result.data.batNbr;

                    }
                    App.cboBatNbr.getStore().load(function () {
                        App.cboBatNbr.setValue(batNbr);
                        App.stoHeader.reload();
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};
var deleteRecordGrid = function (item) {
    if (item == "yes") {
        if (item == 'yes') {
            if (App.slmPO_Trans.selected.items[0].data.tstamp != "") {
                App.grdDetail.deleteSelected();
                delTaxMutil();
                calcDet();
                App.frmMain.getForm().updateRecord();
                if (App.frmMain.isValid()) {
                    App.frmMain.submit({
                        waitMsg: HQ.common.getLang('DeletingData'),
                        method: 'POST',
                        url: 'PO10201/DeleteGrd',
                        timeout: 180000,
                        params: {
                            lstDel: HQ.store.getData(App.stoPO10201_pgDetail),
                            lstDet: Ext.encode(App.stoPO10201_pgDetail.getRecordsValues()),
                            lstHeader: Ext.encode(App.stoHeader.getRecordsValues())

                        },
                        success: function (msg, data) {
                            HQ.message.process(msg, data, true);
                            var batNbr = '';                          
                            if (this.result.data != undefined && this.result.data.batNbr != null) {
                                batNbr = this.result.data.batNbr;
                               
                            }
                            App.cboBatNbr.getStore().load(function () {
                                App.cboBatNbr.setValue(batNbr);
                                App.stoHeader.reload();
                            });
                        },
                        failure: function (msg, data) {
                            HQ.message.process(msg, data, true);                          
                        }
                    });
                }
            }
            else {
                App.stoLotTrans.clearFilter();
                var det = App.slmPO_Trans.selected.items[0].data;
                for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
                    if (App.stoLotTrans.data.items[i].data.POTranLineRef == det.LineRef) {
                        App.stoLotTrans.data.removeAt(i);
                    }
                }
                App.grdDetail.deleteSelected();
                delTaxMutil();
                calcDet();
               
            }
        }
    }
};
var btnImport_Click = function (c, e) {
    
    var fileName = c.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            clientValidation: false,
            method: 'POST',
            url: 'PO10201/Import',
            timeout: 1000000,
            params: {
                lstDet: Ext.encode(App.stoPO10201_pgDetail.getRecordsValues())
            },
            success: function (msg, data) {
                if (this.result.data.lstTrans != undefined) {                   
                    HQ.store.insertBlank(App.stoPO10201_pgDetail, _keys);                  
                        this.result.data.lstTrans.forEach(function (item) {
                            insertItemGrid(App.grdDetail, item);
                         
                            //grdPO_Trans_ValidateEdit(App.grdDetail, App.stoPO10201_pgDetail.data.items[0]);
                            //HQ.store.insertRecord(App.stoPO10201_pgDetail, 'InvtID', newTrans, true);
                        });
                     
                        //App.stoHeader.commitChanges();
                        //App.stoPO10201_pgDetail.commitChanges();
                        //HQ.store.insertRecord(App.stoPO10201_pgDetail, "InvtID", Ext.create('App.mdlPO10201_pgDetailModel'), true);
                        //calculate();
                        //checkTransAdd();
                   
                    if (!Ext.isEmpty(this.result.data.message)) {
                        HQ.message.show('2013103001', [this.result.data.message], '', true);
                    }
                } else {
                    HQ.message.process(msg, data, true);
                }

            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.btnImport.reset();
            }
        });
    } else {
        HQ.message.show('2014070701', [ext], '', true);
        App.btnImport.reset();
    }
};
var btnExport_Click = function () {
    if (Ext.isEmpty(App.cboBranchID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('BranchID')], '', true);
        return;
    }
    else if (Ext.isEmpty(App.cboVendID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('VendID')], '', true);
        return;
    }
     
    App.frmMain.submit({
        //waitMsg: HQ.common.getLang("Exporting"),
        url: 'PO10201/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
            type: 'O',
        },
        success: function (msg, data) {
            alert('sus');
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};
var insertItemGrid = function (grd, item) {
    var objDetail = App.stoPO10201_pgDetail.data.items[App.stoPO10201_pgDetail.getCount() - 1];
   
    var valueTax = '';
    App.cboTaxID.getStore().data.each(function (det) {
        valueTax += det.data.taxid + ',';

    });
    valueTax = valueTax.length > 0 ? valueTax.substring(0, valueTax.length - 1) : '';

    objDetail.set('CnvFact', item.CnvFact);
    objDetail.set("CostID", item.CostID);
    objDetail.set("CostVouched", item.CostVouched);
    objDetail.set("DiscPct", item.DiscPct);
    objDetail.set("DocDiscAmt", item.DocDiscAmt);
    objDetail.set("ExtVolume", item.ExtVolume);


    objDetail.set('ExtWeight', item.ExtWeight);
    objDetail.set('InvtID', item.InvtID);
    objDetail.set('JrnlType', item.JrnlType);
    objDetail.set('LineRef', item.LineRef);
    objDetail.set('OrigRcptDate', item.OrigRcptDate);
    objDetail.set('OrigRcptNbr', item.OrigRcptNbr);
    objDetail.set('OrigRetRcptNbr', item.OrigRetRcptNbr);

    objDetail.set('POLineRef', item.POLineRef);
    objDetail.set('PONbr', item.PONbr);
    objDetail.set('POOriginal', item.POOriginal);
    objDetail.set('PurchaseType', item.PurchaseType);
    objDetail.set("Qty", item.Qty);

    objDetail.set('QtyVouched', item.QtyVouched);
    objDetail.set('RcptConvFact', item.RcptConvFact);
    objDetail.set('RcptDate', item.RcptDate);

    objDetail.set('RcptFee', item.RcptFee);
    objDetail.set('RcptMultDiv', item.RcptMultDiv);
    objDetail.set('RcptNbr', item.RcptNbr);


    
    objDetail.set('RcptQty', item.RcptQty);
    objDetail.set('RcptUnitDescr', item.RcptUnitDescr);
    objDetail.set('ReasonCD', item.ReasonCD);
    objDetail.set('SiteID', item.SiteID);
    objDetail.set('TaxAmt00', item.TaxAmt00);
    objDetail.set('TaxAmt01', item.TaxAmt01);

    objDetail.set('TaxAmt02', item.TaxAmt02);
    objDetail.set('TaxAmt03', item.TaxAmt03);
    objDetail.set('TaxCat', item.TaxCat);
    objDetail.set('TaxID', item.TaxID);
    objDetail.set('TaxID00', item.TaxID00);
    objDetail.set('TaxID01', item.TaxID01);
    objDetail.set('TaxID02', item.TaxID02);
    objDetail.set('TaxID03', item.TaxID03);
    objDetail.set('TranAmt', item.TranAmt);

    objDetail.set('TranDate', item.TranDate);
    objDetail.set('TranDesc', item.TranDesc);

    objDetail.set('TranType', item.TranType);

    objDetail.set('TxblAmt00', item.TxblAmt00);
    objDetail.set('TxblAmt01', item.TxblAmt01);
    objDetail.set('TxblAmt02', item.TxblAmt02);
    objDetail.set('TxblAmt03', item.TxblAmt03);

    objDetail.set('UnitCost', item.UnitCost);
    objDetail.set('UnitDescr', item.UnitDescr);
    objDetail.set('UnitMultDiv', item.UnitMultDiv);
    objDetail.set('UnitVolume', item.UnitVolume);

    objDetail.set('UnitWeight', item.UnitWeight);
    objDetail.set('VendID', item.VendID);
    objDetail.set('VouchStage', item.VouchStage);



    calcDet();
    delTax(objDetail);
    calcTax(objDetail);
    calcTaxTotal();
    //HQ.grid.insert(App.grdDetail, _keys);
    HQ.store.insertBlank(App.stoPO10201_pgDetail, _keys);
};
//////////////////////////////////////////////////////////////////
//// Function ////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


//Cal tax
function calcDet() {
    //if (App.OrderType.getValue() == null) return;
    var taxAmt00 = 0;
    var taxAmt01 = 0;
    var taxAmt02 = 0;
    var taxAmt03 = 0;


    var txblAmtTot00 = 0;
    var txblAmtTot01 = 0;
    var txblAmtTot02 = 0;
    var txblAmtTot03 = 0;
   

    var extvol = 0;
    var extwei = 0;
    var extCost = 0;
    var discount = 0;
    var poFee = 0;
    var RcptConvFact = 0;
    var CTN = 0;
    var PCS = 0;
    var qty = 0;
    var lstdata = App.stoPO10201_pgDetail.allData ? App.stoPO10201_pgDetail.allData : App.stoPO10201_pgDetail.data;
    for (var j = 0; j < lstdata.length; j++) {
        var det = lstdata.items[j];
        taxAmt00 += det.data.TaxAmt00;
        taxAmt01 += det.data.TaxAmt01;
        taxAmt02 += det.data.TaxAmt02;
        taxAmt03 += det.data.TaxAmt03;

        

        poFee += det.data.RcptFee;     
        extCost += det.data.TranAmt;
        discount += det.data.DocDiscAmt
        qty += det.data.RcptQty;

     

    };
    var record = App.stoHeader.getAt(0).data;
    for (var i = 0; i < App.stoPO10201_LoadTaxDoc.data.items.length; i++) {
        var det = App.stoPO10201_LoadTaxDoc.data.items[j];
        if (i == 0) {
            record.TaxAmtTot00 = det.data.TaxAmt;
            record.TxblAmtTot00 = det.data.TxblAmt;
            record.TaxID00 = det.data.TaxID;
        }
        else if (i == 1) {
            record.TaxAmtTot01 = det.data.TaxAmt;
            record.TxblAmtTot01 = det.data.TxblAmt;
            record.TaxID01 = det.data.TaxID;
        }
        else if (i == 2) {
            record.TaxAmtTot02 = det.data.TaxAmt;
            record.TxblAmtTot02 = det.data.TxblAmt;
            record.TaxID02 = det.data.TaxID;
        }
        else if (i == 3) {
            record.TaxAmtTot03 = det.data.TaxAmt;
            record.TxblAmtTot03 = det.data.TxblAmt;
            record.TaxID03 = det.data.TaxID;
        }        
    };
    if(App.cboStatus.getValue()!="V")
    App.txtTotAmt.setValue(Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0) + Math.round(extCost, 0) + Math.round(poFee, 0));
    record.RcptTotAmt = (Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0) + Math.round(extCost, 0) + Math.round(poFee, 0));
    App.txtRcptAmtTot.setValue(Math.round(extCost, 0) + Math.round(poFee, 0));
    App.txtRcptQtyTot.setValue(Math.round(qty, 0));
    App.txtDiscAmt.setValue(Math.round(discount, 0));
    App.txtRcptTot.setValue(Math.round(extCost, 0));
    App.txtTaxAmt.setValue(Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0));
    App.txtRcptFeeTot.setValue(Math.round(poFee, 0));
    App.txtAfterTaxAmt.setValue(Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0) + Math.round(extCost, 0) + Math.round(poFee, 0));

  

}
function delTaxMutil() {

    for (var i = App.stoPO10201_pgLoadTaxTrans.data.length - 1; i >= 0; i--) {
        var data = HQ.store.findInStore(App.stoPO10201_pgDetail, ['LineRef'], [App.stoPO10201_pgLoadTaxTrans.data.items[i].data.LineRef])
        if (!data) App.stoPO10201_pgLoadTaxTrans.data.removeAt(i);
    }
    calcTaxTotal();
};
function delTax(record) {
    //if (App.cboStatus != "H" ) return false;oM_GetRcptConvFactToUnit
    var lineRef = record.data.LineRef;

    for (var j = App.stoPO10201_pgLoadTaxTrans.data.length - 1; j >= 0; j--) {
        if (App.stoPO10201_pgLoadTaxTrans.data.items[j].data.LineRef == lineRef)
            App.stoPO10201_pgLoadTaxTrans.data.removeAt(j);
    }
    clearTax(record);
    calcTaxTotal();
    calcDet();
    return true;

}
function clearTax(record) {
    record.set('TaxID00', '');
    record.set('TaxAmt00', 0);
    record.set('TxblAmt00', 0);

    record.set('TaxID01', '');
    record.set('TaxAmt01', 0);
    record.set('TxblAmt01', 0);

    record.set('TaxID02', '');
    record.set('TaxAmt02', 0);
    record.set('TxblAmt02', 0);

    record.set('TaxID03', '');
    record.set('TaxAmt03', 0);
    record.set('TxblAmt03', 0);
}
function calcTax(record) {

    var det = record.data;
    if (!record) return true;


    var dt = [];
    if (det.TaxID == "*") {
        for (var j = 0; j < App.stoPO10201_pdAP_VenDorTaxes.data.length; j++) {
            var item = App.stoPO10201_pdAP_VenDorTaxes.data.items[j];
            dt.push(item.data);
        };
    }
    else {
        var strTax = det.TaxID.split(',');
        if (strTax.length > 0) {
            for (var k = 0; k < strTax.length; k++) {
                for (var j = 0; j < App.stoPO10201_pdAP_VenDorTaxes.data.length; j++) {
                    if (strTax[k] == App.stoPO10201_pdAP_VenDorTaxes.data.items[j].data.taxid) {
                        dt.push(App.stoPO10201_pdAP_VenDorTaxes.data.items[j].data);
                        break;
                    }
                }
            }
        }
        else {
            if (Ext.isEmpty(det.TaxID) || Ext.isEmpty(det.TaxCat))
                App.stoPO10201_pgDetail.data.items[i].set('TxblAmt00', det.TranAmt);
            return false;
        }
    }

    var taxCat = det.TaxCat;
    var prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
    for (var j = 0; j < dt.length; j++) {
        var objTax = HQ.store.findInStore(App.stoPO10201_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {

                if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                    prcTaxInclRate = prcTaxInclRate + objTax.TaxRate;
                }
            }
        }
    }


    if (prcTaxInclRate == 0)
        txblAmtL1 = Math.round(det.TranAmt, 0);
    else
        txblAmtL1 = Math.round((det.TranAmt) / (1 + prcTaxInclRate / 100), 0);


    record.set('TxblAmt00', txblAmtL1);

    for (var j = 0; j < dt.length; j++) {

        var taxID = "", lineRef = "";
        var taxRate = 0, taxAmtL1 = 0;
        var objTax = HQ.store.findInStore(App.stoPO10201_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "1") {
                    taxID = dt[j].taxid;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    taxAmtL1 = HQ.util.mathRound(txblAmtL1 * objTax.TaxRate / 100, 2); //Math.round(txblAmtL1 * objTax.TaxRate / 100, 0);

                    if (objTax.Lvl2Exmpt == 0) txblAmtAddL2 += txblAmtL1;

                    if (objTax.PrcTaxIncl != "0") {
                        var chk = false;
                        if (j < dt.length - 1) {
                            for (var k = j + 1; k < dt.length; k++) {
                                objTax = dt[k];
                                if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
                                    if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat &&
                                                            objTax.CatExcept01 != taxCat && objTax.CatExcept02 != taxCat &&
                                                            objTax.CatExcept03 != taxCat && objTax.CatExcept04 != taxCat &&
                                                            objTax.CatExcept05 != taxCat)
                                                      || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                                                    objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                                                    objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                                        if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                                            chk = false;
                                            break;
                                        }
                                    }
                                }
                                chk = true;
                            }
                        }
                        else {
                            chk = true;
                        }

                        if (chk) {

                            if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 != det.TranAmt)
                                taxAmtL1 = HQ.util.mathRound(det.TranAmt - (totPrcTaxInclAmt + txblAmtL1), 2);//Math.round(det.TranAmt - (totPrcTaxInclAmt + txblAmtL1), 0);

                        }
                        else
                            totPrcTaxInclAmt += totPrcTaxInclAmt + taxAmtL1;
                    }

                    insertUpdateTax(taxID, lineRef, taxRate, taxAmtL1, txblAmtL1, 1);

                }
            }
        }
    }

    for (var j = 0; j < dt.Count; j++) {
        var taxID = "", lineRef = "";
        var taxRate = 0, txblAmtL2 = 0, taxAmtL2 = 0;
        var objTax = HQ.store.findInStore(App.stoPO10201_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "2") {
                    taxID = dt[j].taxid;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    txblAmtL2 = Math.round(txblAmtAddL2 + txblAmtL1, 0);
                    taxAmtL2 = HQ.util.mathRound(txblAmtAddL2 * objTax.TaxRate / 100,2);//Math.round(txblAmtAddL2 * objTax.TaxRate / 100, 0);
                    insertUpdateTax(taxID, lineRef, taxRate, taxAmtL2, txblAmtL2, 2);
                }
            }
        }
    }
    updateTax(record);
    calcDet();
    return true;
}
function insertUpdateTax(taxID, lineRef, taxRate, taxAmt, txblAmt, taxLevel) {
    var flat = false;
    for (var i = 0; i < App.stoPO10201_pgLoadTaxTrans.data.length; i++) {
        if (App.stoPO10201_pgLoadTaxTrans.data.items[i].data.taxid == taxID && App.stoPO10201_pgLoadTaxTrans.data.items[i].data.LineRef == lineRef) {
            var tax = App.stoPO10201_pdAP_VenDorTaxes.data.items[i];
            tax.set('BranchID', _cpnyID),
            tax.set('TaxID', taxID);
            tax.set('LineRef', lineRef);
            tax.set('TaxRate', taxRate);
            tax.set('TaxLevel', taxLevel.toString());
            tax.set('TaxAmt', taxAmt)
            tax.set('TxblAmt', txblAmt);
            flat = true;
            break;
        }
    }
    if (!flat) {
        var newTax = Ext.create('App.ModelPO10201_pgLoadTaxTrans_Result');
        newTax.data.BranchID = _cpnyID;
        newTax.data.TaxID = taxID;
        newTax.data.LineRef = lineRef;
        newTax.data.TaxRate = taxRate;
        newTax.data.TaxLevel = taxLevel.toString();
        newTax.data.TaxAmt = taxAmt;
        newTax.data.TxblAmt = txblAmt;

        App.stoPO10201_pgLoadTaxTrans.data.add(newTax);
    }
    App.stoPO10201_pgLoadTaxTrans.sort('LineRef', "ASC");
    calcDet();

}
function updateTax(record) {

    if (!record) return;
    var j = 0;
    var det = record.data;
    for (var i = 0; i < App.stoPO10201_pgLoadTaxTrans.data.length; i++) {
        var item = App.stoPO10201_pgLoadTaxTrans.data.items[i];
        if (item.data.LineRef == det.LineRef) {
            if (j == 0) {
                record.set('TaxID00', item.data.TaxID);
                record.set('TxblAmt00', item.data.TxblAmt);
                record.set('TaxAmt00', item.data.TaxAmt);
            }
            else if (j == 1) {
                record.set('TaxID01', item.data.TaxID);
                record.set('TxblAmt01', item.data.TxblAmt);
                record.set('TaxAmt01', item.data.TaxAmt);
            }
            else if (j == 2) {
                record.set('TaxID02', item.data.TaxID);
                record.set('TxblAmt02', item.data.TxblAmt);
                record.set('TaxAmt02', item.data.TaxAmt);
            }
            else if (j == 3) {
                record.set('TaxID03', item.data.TaxID);
                record.set('TxblAmt03', item.data.TxblAmt);
                record.set('TaxAmt03', item.data.TaxAmt);
            }
            j++;
        }
        if (j != 0 && item.data.LineRef != det.LineRef)
            return false;
    };

}
function calcTaxTotal() {
    App.stoPO10201_LoadTaxDoc.clearData();
    var flat = false;
    for (var i = 0; i < App.stoPO10201_pgLoadTaxTrans.data.length; i++) {
        var tax = App.stoPO10201_pgLoadTaxTrans.data.items[i];
        flat = true;
        for (var j = 0; j < App.stoPO10201_LoadTaxDoc.data.length; j++) {
            var taxDoc = App.stoPO10201_LoadTaxDoc.data.items[j];
            if ( tax.data.TaxID == taxDoc.data.TaxID) {
                taxDoc.data.TxblAmt += tax.data.TxblAmt;
                taxDoc.data.TaxAmt += tax.data.TaxAmt;
                flat = false;
                taxDoc.commit();
                break;
            }
        };
        if (flat) {
            var newTaxDoc = Ext.create('App.mdlPO10201_pgLoadTaxTransDoc');
            newTaxDoc.data.BranchID = tax.data.BranchID;
            newTaxDoc.data.RcptNbr = tax.data.RcptNbr;
            newTaxDoc.data.TaxID = tax.data.TaxID;
            newTaxDoc.data.TaxAmt = tax.data.TaxAmt;
            newTaxDoc.data.TaxRate = tax.data.TaxRate;
            newTaxDoc.data.TxblAmt = tax.data.TxblAmt;

            App.stoPO10201_LoadTaxDoc.data.add(newTaxDoc);
            // newTaxDoc.commit();
        }

    };
    App.grdTaxTrans.getView().refresh(false);
    App.grdTaxDoc.getView().refresh(false);
   
}

function lastLineRef(store) {
    var num = 0;
    for (var j = 0; j < store.data.length; j++) {
        var item = store.data.items[j];

        if (!Ext.isEmpty(item.data.LineRef) && parseInt(item.data.LineRef) > num) {
            num = parseInt(item.data.LineRef);
        }
    };
    num++;
    var lineRef = num.toString();
    var len = lineRef.length;
    for (var i = 0; i < 5 - len; i++) {
        lineRef = "0" + lineRef;
    }
    return lineRef;
}
function setUOM(invtID, classID, stkUnit, fromUnit) {
    if (!Ext.isEmpty(fromUnit)) {
        var data = HQ.store.findInStore(App.stoPO10201_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["3", "*", invtID, fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }

        data = HQ.store.findInStore(App.stoPO10201_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["2", classID, "*", fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }

        data = HQ.store.findInStore(App.stoPO10201_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["1", "*", "*", fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }
        HQ.message.show(25, invtID, '');
        return null;
    }
    return null;
};
function oM_GetCnvFactToUnit(invtID, unitDesc) {
    var cnvFact = 1;
    var data;
    App.stoPO10201_pdIN_UnitConversion.data.each(function (item) {
        if (item.data.InvtID == invtID && item.data.FromUnit != unitDesc && item.data.ToUnit == unitDesc) {
            data = item;
            return;
        }
    });
    if (data != null) {
        if (data.data.MultDiv == "D")
            cnvFact = 1 / data.data.CnvFact;
        else
            cnvFact = data.data.CnvFact;
    }

    return cnvFact;
};
     
//// Other Functions ////////////////////////////////////////////////////
function lockControl(isLock) {
    HQ.common.lockItem(App.frmMain, isLock);
}
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.cboBatNbr.getStore().load(function () {
            App.stoHeader.reload();
        });
    }
};
function disableComboAddress() {
    App.cboDistAddr.setValue("");
    App.cboShipSiteID.setValue("");
    App.cboShipCustID.setValue("");
    App.cboShiptoID.setValue("");
    App.cboShipVendID.setValue("");
    App.cboShipVendAddrID.setValue("");
    App.cboShipAddrID.setValue("");

    App.cboDistAddr.disable(false);
    App.cboShipSiteID.disable(true);
    App.cboShipCustID.disable(true);
    App.cboShiptoID.disable(true);
    App.cboShipVendID.disable(true);
    App.cboShipVendAddrID.disable(true);
    App.cboShipAddrID.disable(true);
}
function reSetValueAddress() {
    App.txtShipName.setValue("");
    App.txtShipAttn.setValue("");
    App.txtShipAddr1.setValue("");
    App.txtShipAddr2.setValue("");
    App.cboShipCity.setValue("");
    App.cboShipState.setValue("");
    App.txtShipZip.setValue("");
    App.cboShipCountry.setValue("");
    App.cboShipVia.setValue("");
    App.txtShipPhone.setValue("");
    App.txtShipFax.setValue("");
    App.txtShipEmail.setValue("");
}
var renderPurchaseType = function (value) {
    var obj = App.PurchaseType.getStore().findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderVouchStage = function (value) {
    var obj = App.VouchStage.getStore().findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderRcptStage = function (value) {
    var obj = App.RcptStage.getStore().findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderSiteID = function (value) {
    var obj = App.cboSiteID.getStore().findRecord("SiteID", value);
    if (obj) {
        return obj.data.Name;
    }
    return value;
};
var renderTaxID = function (value) {
    //App.stoPO10201_pgDetail.data.each(function (det) {
    //    value.split(',')

    //});

    //if (obj) {
    //    return obj.data.Descr;
    //}
    //return value;
};



var PopupWinLot = {
    showLot: function (record) {
        App.cbocolLotUnitDesc.getStore().reload();
        App.lblQtyAvail.setText('');
        var lock = !((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) || App.cboStatus.getValue() != "H";
        App.grdLot.isLock = lock;
      

        App.stoLotTrans.clearFilter();
        App.stoLotTrans.filter('POTranLineRef', record.data.LineRef);
        App.winLot.record = record.data;
  
        App.winLot.record = record;
        App.grdLot.view.refresh();
       
        App.winLot.setTitle(record.data.InvtID + ' - ' + record.data.SiteID + ' - ' + record.data.RcptUnitDescr);
        var flat = false;
        if (App.cboStatus.getValue() == "H") {
            if (App.cboRcptType.getValue() == "X") {

                App.stoLotTrans.data.each(function (item) {
                    //if (item.data.POTranLineRef == App.winLot.record.LineRef && !Ext.isEmpty(item.data.LotSerNbr)) {
                    //    flat = true;
                    //}
                    flat = true;
                });
                if (!flat) {
                    App.cboLotSerNbr.getStore().load(function () {                        
                        PopupWinLot.addNewLot(record.data, App.cboLotSerNbr.getStore().getCount() > 0 ? App.cboLotSerNbr.getStore().getAt(0).data.LotSerNbr : '');
                    })
                } else {

                    App.cboLotSerNbr.getStore().reload();
                    PopupWinLot.addNewLot(record.data);
                }
            }
            else {
                App.cboLotSerNbr.getStore().reload();
                PopupWinLot.addNewLot(record.data);
            }
        }
        App.winLot.show();
    },
    btnLotOK_Click: function () {
      
        var recordTran = App.winLot.record.data;

        var flat = null;
        App.stoLotTrans.data.each(function (item) {
            if (App.cboStatus.getValue() == "H") {

                if (!Ext.isEmpty(item.data.LotSerNbr)) {
                    if (item.data.Qty == 0) {
                        if (App.cboRcptType.getValue() == "X") {
                            App.smlLot.select(App.stoLotTrans.indexOf(item));
                            App.grdLot.deleteSelected();
                        }
                        else {
                            HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
                            flat = item;
                            return false;
                        }
                    }

                    if (Ext.isEmpty(item.data.UnitDesc)) {
                        HQ.message.show(1000, [HQ.common.getLang('unitDesc')], '', true);
                        flat = item;
                        return false;
                    }

                    if (Ext.isEmpty(item.data.UnitMultDiv)) {
                        HQ.message.show(2525, [item.data.InvtID], '', true);
                        flat = item;
                        return false;
                    }
                }
                else if (item.data.Qty == 0) {
                    if (App.cboRcptType.getValue() == "X") {
                        App.smlLot.select(App.stoLotTrans.indexOf(item));
                        App.grdLot.deleteSelected();
                    }
                } else if (item.data.Qty > 0) {
                    return false;
                }
            }
        });
        App.grdLot.view.refresh();
        if (!Ext.isEmpty(flat)) {
            App.smlLot.select(App.stoLotTrans.indexOf(flat));
            return;
        }

        var qty = 0;
        App.stoLotTrans.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                if (item.data.SiteID == recordTran.SiteID && item.data.InvtID == recordTran.InvtID && item.data.POTranLineRef == recordTran.LineRef) {
                    qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                }
            }

        });
        if (!Ext.isEmpty(recordTran.PONbr) && qty > (recordTran.UnitMultDiv == "M" ? recordTran.Qty * recordTran.CnvFact : recordTran.Qty / recordTran.CnvFact)) {
            HQ.message.show(20150326, '', '');
            return false;
        }
        var RcptQty = (recordTran.UnitMultDiv == "M" ? qty / recordTran.RcptConvFact : recordTran.RcptQty * recordTran.RcptConvFact)
        if (RcptQty % 1 > 0) {
            App.winLot.record.data.RcptQty = qty;
            App.winLot.record.data.RcptUnitDescr = App.winLot.record.invt.StkUnit;
            App.winLot.record.data.RcptConvFact = 1;
            App.winLot.record.data.UnitMultDiv = "M";


            if (_objPO_Setup.DfltLstUnitCost == "A" || _objPO_Setup.DfltLstUnitCost == "L") {
                HQ.common.showBusy(true);
                App.direct.PO10201ItemSitePrice(
                    App.cboBranchID.getValue(), recordTran.InvtID, recordTran.SiteID,
                   {
                       success: function (result) {
                           _objIN_ItemSite = result;
                           UnitCost = result == null ? 0 : (_objPO_Setup.DfltLstUnitCost == "A" ? result.AvgCost : result.LastPurchasePrice);
                           UnitCost = Math.round((recordTran.RcptMultDiv == "D" ? (UnitCost / recordTran.RcptConvFact) : (UnitCost * recordTran.RcptConvFact)));
                           App.winLot.record.set("UnitCost", UnitCost);
                           App.winLot.record.set("TranAmt", UnitCost * recordTran.RcptQty - recordTran.DocDiscAmt);
                           App.winLot.record.commit();
                           delTax(App.winLot.record);
                           calcTax(App.winLot.record);
                           calcTaxTotal();
                           HQ.common.showBusy(false);
                       },
                       failure: function (result) {
                           HQ.common.showBusy(false);
                       }

                   });
            }
            else if (_objPO_Setup.DfltLstUnitCost == "P") {
                HQ.common.showBusy(true);
                App.direct.PO10201POPrice(
                   App.cboBranchID.getValue(), recordTran.InvtID, recordTran.RcptUnitDescr, Ext.Date.format(App.dtRcptDate.getValue(), 'Y-m-d'),
                    {
                        success: function (result) {
                            UnitCost = result;
                            App.winLot.record.set("UnitCost", result);
                            App.winLot.record.set("TranAmt", result * recordTran.RcptQty - recordTran.DocDiscAmt);
                            App.winLot.record.commit();
                            delTax(App.winLot.record);
                            calcTax(App.winLot.record);
                            calcTaxTotal();
                            HQ.common.showBusy(false);

                        },
                        failure: function (result) {
                            HQ.common.showBusy(false);
                        }
                    });

            }
            else if (_objPO_Setup.DfltLstUnitCost == "I") {
                var UnitCost = objIN_Inventory.POPrice;
                UnitCost = Math.round((recordTran.RcptMultDiv == "D" ? (UnitCost / recordTran.RcptConvFact) : (UnitCost * recordTran.RcptConvFact)));
                App.winLot.record.set("UnitCost", UnitCost);
                App.winLot.record.set("TranAmt", UnitCost * recordTran.RcptQty - recordTran.DocDiscAmt);
                App.winLot.record.commit();
                delTax(App.winLot.record);
                calcTax(App.winLot.record);
                calcTaxTotal();
            }
        } else {
            App.winLot.record.data.RcptQty = Math.round(RcptQty);
            App.winLot.record.set("TranAmt", App.winLot.record.data.UnitCost * recordTran.RcptQty - recordTran.DocDiscAmt);
            App.winLot.record.commit();
            delTax(App.winLot.record);
            calcTax(App.winLot.record);
            calcTaxTotal();
        }


        //checkSubDisc(App.winLot.record);
        //checkTaxInGrid("RcptQty", App.winLot.record);

        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (!Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr)) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
        App.winLot.hide();
    },
    btnLotDel_Click: function () {

        if ((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) {
            if (App.cboStatus.getValue() != "H") {
                HQ.message.show(2015020805, [App.cboBatNbr.value], '', true);
                return;
            }
            if (App.smlLot.selected.items.length != 0) {
                if (!Ext.isEmpty(App.smlLot.selected.items[0].data.LotSerNbr)) {
                    HQ.message.show(2015020806, [App.smlLot.selected.items[0].data.InvtID + ' ' + App.smlLot.selected.items[0].data.LotSerNbr], 'PopupWinLot.deleteLot', true);
                }
            }
        }
    },
    grdLot_BeforeEdit: function (item, e) {
        var obj = e.record.data;
        if (App.cboRcptType.getValue() == "X") {
            var objLot = HQ.store.findInStore(App.cboLotSerNbr.getStore(), ['LotSerNbr'], [obj.LotSerNbr]);
            if (!Ext.isEmpty(objLot)) {

                var Qty = obj.UnitMultDiv == "M" ? objLot.Qty / obj.CnvFact : objLot.Qty * obj.CnvFact;
                App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + Qty + " " + obj.UnitDesc);
                App.smlLot.selected.items[0].set("ExpDate", objLot.ExpDate);
            }
            else {
                App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
            }

        }

        if (App.grdLot.isLock || e.field == "UnitDesc") {
            return false;
        }
        if (e.field == 'LotSerNbr' && App.winLot.invt.LotSerRcptAuto && App.cboRcptType.getValue() != 'X') return false;
        if (e.field != 'LotSerNbr' && App.cboRcptType.getValue() == 'X' && Ext.isEmpty(e.record.data.LotSerNbr)) return false;
       

       

        var det = App.winLot.record;
        if (!_objPO_Setup.EditablePOPrice && e.column.dataIndex == "UnitPrice") {
            return false;
        }
        if (Ext.isEmpty(det.LineRef)) {
            e.record.set('LineRef', lastLineRef(App.stoPO10201_pgDetail));
        }

        if (e.field == "Qty") {
          
        }

    },
    grdLot_SelectionChange: function (item, selected) {
        //HQ.focus = 'lot';
        //if (selected.length > 0) {
        //    if (!Ext.isEmpty(selected[0].data.InvtID)) {
        //        HQ.numSelectLot = 0;
        //        HQ.maxSelectLot = 1;
        //        App.grdLot.view.loadMask.show();
        //        App.stoItemLot.load({
        //            params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr },
        //            callback: checkSelectLot,
        //            row: selected[0]
        //        });
        //    } else {
        //        App.lblLotQtyAvail.setText('');
        //    }
        //}
    },
    grdLot_Edit: function (item, e) {
      
        var objDetail = e.record.data;
        
        var recordTran = App.winLot.record.data;
        var objIN_Inventory = App.winLot.invt;

        if (e.field == "UnitDesc") {
            var cnv = setUOM(objIN_Inventory.InvtID, objIN_Inventory.ClassID, objIN_Inventory.StkUnit, e.value);

            if (!Ext.isEmpty(cnv)) {

                e.record.set('CnvFact', cnv.CnvFact);
                e.record.set('UnitMultDiv', cnv.MultDiv);
            } else {
                e.record.set('UnitDesc', "");
                 HQ.common.showBusy(false, '', App.winLot);
                return;
            }
            if (_objPO_Setup.DfltLstUnitCost == "A" || _objPO_Setup.DfltLstUnitCost == "L") {
                HQ.common.showBusy(true, '', App.winLot);
                App.direct.PO10201ItemSitePrice(
                    App.cboBranchID.getValue(), recordTran.InvtID, recordTran.SiteID,
                   {
                       success: function (result) {
                           _objIN_ItemSite = result;
                           UnitCost = result == null ? 0 : (_objPO_Setup.DfltLstUnitCost == "A" ? result.AvgCost : result.LastPurchasePrice);
                           UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnvFact)));
                           e.record.set("UnitPrice", UnitCost);
                           HQ.common.showBusy(false, '', App.winLot);
                       },
                       failure: function (result) {
                           HQ.common.showBusy(false, '', App.winLot);
                       }

                   });
            }
            else if (_objPO_Setup.DfltLstUnitCost == "P") {
                HQ.common.showBusy(true, '', App.winLot);
                App.direct.PO10201POPrice(
                   App.cboBranchID.getValue(), recordTran.InvtID, objDetail.UnitDesc, Ext.Date.format(App.dtRcptDate.getValue(), 'Y-m-d'),
                    {
                        success: function (result) {
                            UnitCost = result;
                            e.record.set("UnitPrice", result);
                            HQ.common.showBusy(false, '', App.winLot);

                        },
                        failure: function (result) {
                            HQ.common.showBusy(false, '', App.winLot);
                        }
                    });

            }
            else if (_objPO_Setup.DfltLstUnitCost == "I") {
                var UnitCost = objIN_Inventory.POPrice;
                UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnnvFact)));
                e.record.set("UnitPrice", UnitCost);
            }

        }
        if (e.field == "Qty") {
            if (objDetail.PurchaseType == "FA") {
                if (objDetail.Qty > 1) {

                    HQ.message.show(58, '', '');
                    return false;

                }
            }
        }

        if (objDetail.PurchaseType == "PR") {
            e.record.set("UnitPrice", 0);
        }
        if (e.field == "Qty" && e.value > 0) {
            if (App.cboRcptType.getValue() != "X") {
                if (objIN_Inventory.LotSerRcptAuto) {
                    if (Ext.isEmpty(objDetail.LotSerNbr)) {
                        HQ.common.showBusy(true, '', App.winLot);
                        App.direct.INNumberingLot(
                            recordTran.InvtID, Ext.Date.format(App.dtRcptDate.getValue(), 'Y-m-d'), 'LotNbr',
                            {
                                success: function (result) {
                                    e.record.set("LotSerNbr", result);
                                    HQ.common.showBusy(false, '', App.winLot);
                                    if (!Ext.isEmpty(objDetail.LotSerNbr)) {
                                        PopupWinLot.addNewLot(recordTran);
                                    }

                                },
                                failure: function (result) {
                                    HQ.common.showBusy(false, '', App.winLot);
                                }
                            });

                    }
                }
                else if (!Ext.isEmpty(objDetail.LotSerNbr)) {
                    PopupWinLot.addNewLot(recordTran);
                }
            }
            else if (!Ext.isEmpty(objDetail.LotSerNbr)) {
                PopupWinLot.addNewLot(recordTran);
            }
        } else if (!Ext.isEmpty(objDetail.LotSerNbr)) {
            PopupWinLot.addNewLot(recordTran);
        }
    },
    grdLot_ValidateEdit: function (item, e) {
        if (App.cboStatus.getValue() != "H") return false;
        var Qty = 0;
        var objdet = e.record;
        var recordTran = App.winLot.record.data;
        if (["LotSerNbr"].indexOf(e.field) != -1) {
            if (HQ.grid.checkDuplicate(App.grdLot, e, ["LotSerNbr"])) {
                HQ.message.show(1112, e.value, '');
                return false;
            }
        }          
        if (e.field == "Qty") {         
            var Qty = 0;
            Qty = e.record.data.UnitMultDiv == "M" ? e.value * e.record.data.CnvFact : e.value / e.record.data.CnvFact;
            if (App.cboRcptType.getValue() == "X" && (recordTran.PurchaseType == "GI" | recordTran.PurchaseType == "PR" | recordTran.PurchaseType == "GS")) {                
                QtyAvail = HQ.store.findInStore(App.cboLotSerNbr.getStore(), ['LotSerNbr'], [objdet.data.LotSerNbr]).Qty;
                if (Qty > QtyAvail) {
                    HQ.message.show(35, '', '');
                    objdet.set('Qty', 0);
                    return false;
                }
            }                    
        }
    },
    cboLotTrans_Change: function (sender)
    {
        if (App.cboRcptType.getValue() == "X") {
            var objLot = HQ.store.findInStore(App.cboLotSerNbr.getStore(), ['LotSerNbr'], [sender.value]);
            if (!Ext.isEmpty(objLot)) {
                var obj = App.smlLot.selected.items[0].data;
                var Qty=obj.UnitMultDiv == "M" ? objLot.Qty / obj.CnvFact : objLot.Qty * obj.CnvFact;
                App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + Qty +" "+obj.UnitDesc);
                App.smlLot.selected.items[0].set("ExpDate", objLot.ExpDate);
            }
            else {
                App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
            }
        }
    },
    deleteLot: function (item) {
        if (item == 'yes') {
            App.grdLot.deleteSelected();
        }
    },
    addNewLot: function (record, lotSerNbr) {
        var newRow = Ext.create('App.mdlLotTrans');

        newRow.data.LotSerNbr = !Ext.isEmpty(lotSerNbr) ? lotSerNbr : '';

        newRow.data.POTranLineRef = record.LineRef;
        newRow.data.UnitDesc = record.RcptUnitDescr;
        newRow.data.InvtID = record.InvtID;
        newRow.data.SiteID = record.SiteID;
        newRow.data.CnvFact = record.RcptConvFact;
        newRow.data.UnitMultDiv = record.RcptMultDiv;

        newRow.data.TranScr = 'PO';
        newRow.data.TranType = App.cboRcptType.getValue();
        newRow.data.InvtMult = newRow.TranType == "X" ? -1 : 1;
        newRow.data.WhseLoc = '';
        newRow.data.UnitPrice = record.UnitCost;
        newRow.data.UnitCost = record.UnitCost;
        newRow.data.MfcDate = HQ.bussinessDate;
        newRow.data.WarrantyDate = App.dtRcptDate.getValue().addDays(App.winLot.invt.WarrantyDays)

        newRow.data.ExpDate = App.dtRcptDate.getValue().addDays(App.winLot.invt.ShelfLife);
        HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);
    }
}
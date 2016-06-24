//// Declare //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////
var keys = ['LineType'];

var fieldsCheckRequire = ['TranAmt'];
var fieldsLangCheckRequire = ['TranAmt'];
var _Source = 0;
var _maxSource = 8;
var _isLoadMaster = false;

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        App.stoAP10100_pdHeader.reload();
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboBatNbr, HQ.isChange);
            } else if (HQ.focus == 'AP_Trans') {
                HQ.grid.first(App.grdAP_Trans);
            }
            else if (HQ.focus == 'TaxTrans') {
                HQ.grid.first(App.grdTaxTrans);
            }
            else if (HQ.focus == 'TaxDoc') {
                HQ.grid.first(App.grdTaxDoc);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboBatNbr);
            } else if (HQ.focus == 'AP_Trans') {
                HQ.grid.prev(App.grdAP_Trans);
            }
            else if (HQ.focus == 'TaxTrans') {
                HQ.grid.prev(App.grdTaxTrans);
            }
            else if (HQ.focus == 'TaxDoc') {
                HQ.grid.prev(App.grdTaxDoc);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboBatNbr);
            } else if (HQ.focus == 'AP_Trans') {
                HQ.grid.next(App.grdAP_Trans);
            }
            else if (HQ.focus == 'TaxTrans') {
                HQ.grid.next(App.grdTaxTrans);
            }
            else if (HQ.focus == 'TaxDoc') {
                HQ.grid.next(App.grdTaxDoc);
            }
            break;

        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboBatNbr);
            } else if (HQ.focus == 'AP_Trans') {
                HQ.grid.last(App.grdAP_Trans);
            }
            else if (HQ.focus == 'TaxTrans') {
                HQ.grid.last(App.grdTaxTrans);
            }
            else if (HQ.focus == 'TaxDoc') {
                HQ.grid.last(App.grdTaxDoc);
            }
            break;
        case "refresh":
            if (HQ.isChange == true) {
                HQ.message.show(20150303, '', 'AP10100refresh');
            }
            else {
                HQ.isFirstLoad = true;
                HQ.common.showBusy(true, HQ.common.getLang('loading'));
                if (App.cboBatNbr.valueModels == null)
                    App.cboBatNbr.setValue('');
                App.stoAP10100_pdHeader.reload();

            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header' || HQ.focus == 'mider') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboBatNbr.forceSelection = false
                        App.cboBatNbr.events['change'].suspend();
                        App.cboBatNbr.setValue('');
                        App.cboBatNbr.events['change'].resume();
                        App.stoAP10100_pdHeader.reload();
                    }
                }
                else if (HQ.focus == 'AP_Trans') {
                    if (App.cboStatus.getValue() != 'H')
                        return;
                    HQ.grid.insert(App.grdAP_Trans, keys);
                    App.cboLineType.setValue('N');
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboBatNbr.value != "" && App.cboStatus.value != "C" && App.cboStatus.value != "V") {
                    if (HQ.focus == 'header') {
                        HQ.message.show(11, '', 'deleteHeader');
                    } else if (HQ.focus == 'AP_Trans') {
                        var rowindex = HQ.grid.indexSelect(App.grdAP_Trans);
                        if (rowindex != '') {
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdAP_Trans), ''], 'deleteRecordGrid', true)
                        }
                    }
                }
            }
            break;
        case "save":
            if (App.cboStatus.getValue() == "V" || ((Ext.isEmpty(App.cboHandle.getValue()) || App.cboHandle.getValue() == "N") && App.cboStatus.getValue() == "C")) return;
            if (App.txtCuryDocBal.getValue() == '0') {
                HQ.message.show(704, '', '');
                return;
            }
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) &&
                    HQ.store.checkRequirePass(App.stoAP_Trans, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                    ) {
                    save();
                }
            }
            break;
    }
};

var firstLoad = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loading..."), App.frmMain);
    App.txtBranchID.setValue(HQ.cpnyID);
    App.cboBatNbr.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboDocType.getStore().addListener('load', checkLoad);
    App.cboVendID.getStore().addListener('load', checkLoad);
    App.cboTerms.getStore().addListener('load', checkLoad);
    App.cboLineType.getStore().addListener('load', checkLoad);
    App.cboInvtID.getStore().addListener('load', checkLoad);
    App.cboTaxCat.getStore().addListener('load', checkLoad);
    HQ.util.checkAccessRight();
    
};
var stoAP10100_pdHeader_BeforeLoad = function (store) {
    HQ.common.showBusy(true, HQ.common.getLang('loading...'), App.frmMain);
};
var stoAP10100_pdHeader_Load = function (store) {
   // HQ.common.showBusy(true, HQ.common.getLang("loading..."));
    HQ.isFirstLoad = true;
    HQ.isNew = false;

    App.cboVendID.setReadOnly(true);
    App.cboDocType.setReadOnly(true);
    if (store.data.length == 0) {

        HQ.store.insertBlank(store, "RefNbr");
        record = store.getAt(0);
        record.data.Status = 'H';
        record.data.DocDate = HQ.businessDate;
        record.data.DueDate = HQ.businessDate;
        record.data.DiscDate = HQ.businessDate;
        record.data.InvcDate = HQ.businessDate;
        record.data.DocType = "VO";
        record.data.OrigDocAmt = 0;
        record.data.DocBal = 0;
        record.data.OrigDocAmt = 0;

        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require
        App.cboVendID.setReadOnly(false);
        App.cboDocType.setReadOnly(false);

    }
    var record = store.getAt(0);
 
    App.frmMain.getForm().loadRecord(record);
    App.stoAP10100_pdHeader.commitChanges();
    if (HQ.isInsert == true)
        HQ.isFirstLoad = true;
    App.stoAP_Trans.reload();

    frmChange();
    //App.stoAP10100_pgLoadTaxTrans.reload();
    //HQ.common.showBusy(false);
};

var stoAPTrans_Load = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(store, keys);
        }
        HQ.isFirstLoad = false;
    }
    // frmChange();
    App.stoAP10100_pgLoadTaxTrans.reload();
    App.stoAP10100_pdHeader.commitChanges();
    frmChange();
   // HQ.common.showBusy(false, '', App.frmMain);
}

var stoTaxTrans_Load = function (sto) {
    App.stoAP10100_LoadTaxDoc.clearData();
    calcTaxTotal();
    HQ.common.showBusy(false, '', App.frmMain);
    frmChange();
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AP10100');
    if (HQ.isChange)
    {
        App.cboDocType.setReadOnly(true);
        App.cboVendID.setReadOnly(true);
    }
};

var cboBatNbr_Change = function (sender, value) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoAP10100_pdHeader.loading) {
        App.stoAP10100_pdHeader.reload();
    }
};
var cboBatNbr_Select = function (sender, value) {
    if (!App.stoAP10100_pdHeader.loading) {
        App.stoAP10100_pdHeader.reload();
    }
}
var cboVendID_Change = function (sender, e)
{
    if (sender.valueModels != null) {
        if (!Ext.isEmpty(App.cboVendID.getValue())) {
            App.cboTaxID.getStore().reload();
            var obj = HQ.store.findInStore(App.cboVendID.getStore(), ["VendID"], [e]);
            if (obj != undefined && App.cboVendID.hasFocus) {
                App.cboTerms.setValue(obj.Terms);
                var obj = HQ.store.findInStore(App.cboTerms.getStore(), ["TermsID"], [obj.Terms]);
                if (obj != undefined) {
                    App.dteDueDate.setValue(App.dteDocDate.value ? App.dteDocDate.getValue().addDays(obj.DueIntrv) : App.dteDueDate.value);
                }
            }
        }
    }
}

var cboTerms_Change = function (sender, e) {
    var obj = HQ.store.findInStore(App.cboTerms.getStore(), ["TermsID"], [e]);
    if (obj != undefined && App.cboTerms.hasFocus) {
        App.dteDueDate.setValue(App.dteDocDate.value ? App.dteDocDate.getValue().addDays(obj.DueIntrv) : App.dteDueDate.value);
    }
}
var cboStatus_Change = function (item, newValue, oldValue) {
    if (newValue == 'H' && HQ.isInsert && HQ.isUpdate) {
        HQ.common.lockItem(App.frmMain, false);
    }
    else HQ.common.lockItem(App.frmMain, true);
    App.cboHandle.getStore().load(function (records, operation, success) {
        App.cboHandle.setValue("N");
    });
}
var dteDocDate_Change = function (sender, e) {
    var obj = HQ.store.findInStore(App.cboTerms.getStore(), ["TermsID"], [App.cboTerms.getValue()]);
    if (obj != undefined && App.dteDocDate.hasFocus) {
        App.dteDueDate.setValue(App.dteDocDate.value ? App.dteDocDate.getValue().addDays(obj.DueIntrv) : App.dteDueDate.value);
    }
}

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
// reject // 

var grdAP_Trans_Reject = function (record) {
    if (record.data.tstamp == '') {
        var index = App.stoAP_Trans.indexOf(record);
        delTax(index);
        calcTaxTotal();

        //App.stoAP_Trans.remove(record);
        //App.grdAP_Trans.getView().focusRow(App.stoAP_Trans.getCount() - 1);
        //App.grdAP_Trans.getSelectionModel().select(App.stoAP_Trans.getCount() - 1);
    } else {
        var index = App.stoAP_Trans.indexOf(record);
        record.reject();
        delTax(index);
        calcTax(index);
        calcTaxTotal();
        calcDet();
    }
    HQ.grid.checkReject(record, App.grdAP_Trans);
    stoChanged(App.stoAP_Trans);
    frmChange();
};

var grdAP_Trans_BeforeEdit = function (editor, e) {
    var det = e.record.data;
    if (App.cboStatus.value != "H") {
        return false;
    }
    if (Ext.isEmpty(App.cboVendID.getValue())) {
        HQ.message.show(15, App.cboVendID.fieldLabel, '');
        return false;
    }
    else if (Ext.isEmpty(App.txtDocDescr.getValue())) {
        HQ.message.show(15, App.txtDocDescr.fieldLabel, '');
        return false;
    }
   
    if (Ext.isEmpty(det.LineRef)) {
        e.record.set("LineType", "N");
        var indexVendIDToGetDescr = App.cboVendID.store.indexOf(App.cboVendID.store.findRecord("VendID", App.cboVendID.getValue()));
        var obj = HQ.store.findInStore(App.cboVendID.getStore(), ["VendID"], [App.cboVendID.getValue()]);
        var tranDesc = obj == undefined ? "" : obj.VendID + ' - ' + obj.Name;
        e.record.set("TranDesc", tranDesc != "" ? tranDesc : "");


        var valueTax = '';
        App.cboTaxID.getStore().data.each(function (det) {
            valueTax += det.data.TaxID + ',';

        });
        valueTax = valueTax.length > 0 ? valueTax.substring(0, valueTax.length - 1) : '';
        e.record.set("TaxID", valueTax);
        e.record.set('LineRef', HQ.store.lastLineRef(App.stoAP_Trans));
        //return false;
    }
    //return HQ.grid.checkBeforeEdit(e, keys);
};

var grdAP_Trans_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdAP_Trans, e, 'LineType');
    var r = e.record.data;
    if (e.field == 'Qty') {
        var quantity = e.value;
        e.record.set('TranAmt', quantity * r.UnitPrice);
    } else if (e.field == 'UnitPrice') {
        var unitprice = e.value;
        e.record.set('TranAmt', r.Qty * unitprice);
    } else if (e.field == 'TranAmt') {
        var tranAmt = e.value;
        if (r.Qty != 0) {
            e.record.set('UnitPrice', tranAmt / r.Qty);
        }
    }


    if (e.field == 'InvtID') {
        var obj = App.cboInvtID.getStore().findRecord("InvtID", e.value);
        e.record.set("TaxCat", obj == undefined ? '' : obj.data.TaxCat);
    }



   delTax(e.rowIdx);
   calcTax(e.rowIdx);
   calcTaxTotal();
};

var grdAP_Trans_ValidateEdit = function (item, e) {
   // return HQ.grid.checkValidateEdit(App.grdAP_Trans, e, keys);
};

var frmChange = function () {
    if (App.frmMain.getRecord() != undefined ) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoAP10100_pdHeader) == false ? HQ.store.isChange(App.stoAP_Trans) : true;
        HQ.common.changeData(HQ.isChange, 'AP10100');//co thay doi du lieu gan * tren tab title header      
        App.cboBatNbr.setReadOnly(HQ.isChange);

    }
    else {
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'AP10100');
    }
};


function calcTaxTotal() {
    App.stoAP10100_LoadTaxDoc.clearData();
    var flat = false;
    for (var i = 0; i < App.stoAP10100_pgLoadTaxTrans.data.length; i++) {
        var tax = App.stoAP10100_pgLoadTaxTrans.data.items[i];
        flat = true;
        for (var j = 0; j < App.stoAP10100_LoadTaxDoc.data.length; j++) {
            var taxDoc = App.stoAP10100_LoadTaxDoc.data.items[j];
            if (tax.data.PONbr == taxDoc.data.PONbr && tax.data.TaxID == taxDoc.data.TaxID) {
                taxDoc.data.TxblAmt += tax.data.TxblAmt;
                taxDoc.data.TaxAmt += tax.data.TaxAmt;
                flat = false;
                taxDoc.commit();
                break;
            }
        };
        if (flat) {
            var newTaxDoc = Ext.create('App.mdlAP10100_pgLoadTaxTransDoc');
            newTaxDoc.data.BranchID = tax.data.BranchID;
            newTaxDoc.data.BatNbr = tax.data.BatNbr;
            newTaxDoc.data.RefNbr = tax.data.RefNbr;
            newTaxDoc.data.TaxID = tax.data.TaxID;
            newTaxDoc.data.TaxAmt = tax.data.TaxAmt;
            newTaxDoc.data.TaxRate = tax.data.TaxRate;
            newTaxDoc.data.TxblAmt = tax.data.TxblAmt;

            App.stoAP10100_LoadTaxDoc.data.add(newTaxDoc);
            // newTaxDoc.commit();
        }

    };
    App.grdTaxTrans.getView().refresh(false);
    App.grdTaxDoc.getView().refresh(false);

};
function calcDet() {
    if (App.cboStatus.getValue() != 'H') return;
    //if (App.OrderType.getValue() == null) return;
    var taxAmt00 = 0;
    var taxAmt01 = 0;
    var taxAmt02 = 0;
    var taxAmt03 = 0;
    var TxblAmt00 = 0;
    var TxblAmt01 = 0;
    var TxblAmt02 = 0;
    var TxblAmt03 = 0;

    var TotAmt = 0;
    for (var j = 0; j < App.stoAP_Trans.data.length; j++) {
        var det = App.stoAP_Trans.data.items[j];
        taxAmt00 += det.data.TaxAmt00;
        taxAmt01 += det.data.TaxAmt01;
        taxAmt02 += det.data.TaxAmt02;
        taxAmt03 += det.data.TaxAmt03;

        TxblAmt00 += det.data.TxblAmt00;
        TxblAmt01 += det.data.TxblAmt01;
        TxblAmt02 += det.data.TxblAmt02;
        TxblAmt03 += det.data.TxblAmt03;

    };
    //for (var j = 0; j < App.cboRefNbr.getStore().data.length; j++) {
    //    var det = App.cboRefNbr.getStore().data.items[j].data;
    //    if(det.RefNbr != App.cboRefNbr.getValue() && det.Rlsed != -1)
    //    {
    //        TotAmt += det.OrigDocAmt;
    //    }
    //    else if (det.RefNbr == App.cboRefNbr.getValue() && det.Rlsed != -1) {
    //        TotAmt += taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03 + TxblAmt00 + TxblAmt01 + TxblAmt02 + TxblAmt03;
    //    }         
    //};
    //if (Ext.isEmpty(App.cboRefNbr.getValue())) TotAmt += taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03 + TxblAmt00 + TxblAmt01 + TxblAmt02 + TxblAmt03;
    TotAmt = taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03 + TxblAmt00 + TxblAmt01 + TxblAmt02 + TxblAmt03;
    App.txtCuryCrTot.setValue(TotAmt);
    App.txtCuryOrigDocAmt.setValue(TotAmt);
    App.txtCuryDocBal.setValue(TotAmt);
};
function delTaxMutil() {

    for (var i = App.stoAP10100_pgLoadTaxTrans.data.length - 1; i >= 0; i--) {
        var data = HQ.store.findInStore(App.stoAP_Trans, ['LineRef'], [App.stoAP10100_pgLoadTaxTrans.data.items[i].data.LineRef])
        if (!data) App.stoAP10100_pgLoadTaxTrans.data.removeAt(i);
    }
    calcTaxTotal();
};
function delTax(index) {
    //if (App.cboStatus != "H" ) return false;
    var lineRef = App.stoAP_Trans.data.items[index].data.LineRef;

    for (var j = App.stoAP10100_pgLoadTaxTrans.data.length - 1; j >= 0; j--) {
        if (App.stoAP10100_pgLoadTaxTrans.data.items[j].data.LineRef == lineRef)
            App.stoAP10100_pgLoadTaxTrans.data.removeAt(j);
    }
    clearTax(index);
    calcTaxTotal();
    calcDet();
    return true;

};
function clearTax(index) {
    App.stoAP_Trans.data.items[index].set('TaxId00', '');
    App.stoAP_Trans.data.items[index].set('TaxAmt00', 0);
    App.stoAP_Trans.data.items[index].set('TxblAmt00', 0);

    App.stoAP_Trans.data.items[index].set('TaxId01', '');
    App.stoAP_Trans.data.items[index].set('TaxAmt01', 0);
    App.stoAP_Trans.data.items[index].set('TxblAmt01', 0);

    App.stoAP_Trans.data.items[index].set('TaxId02', '');
    App.stoAP_Trans.data.items[index].set('TaxAmt02', 0);
    App.stoAP_Trans.data.items[index].set('TxblAmt02', 0);

    App.stoAP_Trans.data.items[index].set('TaxId03', '');
    App.stoAP_Trans.data.items[index].set('TaxAmt03', 0);
    App.stoAP_Trans.data.items[index].set('TxblAmt03', 0);
};
function calcTax(index) {

    var det = App.stoAP_Trans.data.items[index].data;
    var record = App.stoAP_Trans.data.items[index];
    if (index < 0) return true;


    var dt = [];
    if (det.TaxID == "*") {
        for (var j = 0; j < App.cboTaxID.getStore().data.length; j++) {
            var item = App.cboTaxID.getStore().data.items[j];
            dt.push(item.data);
        };
    }
    else {
        var strTax = det.TaxID.split(',');
        if (strTax.length > 0) {
            for (var k = 0; k < strTax.length; k++) {
                for (var j = 0; j < App.cboTaxID.getStore().data.length; j++) {
                    if (strTax[k] == App.cboTaxID.getStore().data.items[j].data.TaxID) {
                        dt.push(App.cboTaxID.getStore().data.items[j].data);
                        break;
                    }
                }
            }
        }
        else {
            if (Ext.isEmpty(det.TaxID) || Ext.isEmpty(det.TaxCat))
                App.stoAP_Trans.data.items[i].set('TxblAmt00', det.TranAmt);
            return false;
        }
    }

    var taxCat = det.TaxCat;
    var prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
    for (var j = 0; j < dt.length; j++) {
        var objTax = HQ.store.findInStore(App.stoAP10100_pdSI_Tax, ['TaxID'], [dt[j].TaxID]);
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

        var TaxID = "", lineRef = "";
        var taxRate = 0, taxAmtL1 = 0;
        var objTax = HQ.store.findInStore(App.stoAP10100_pdSI_Tax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "1") {
                    TaxID = dt[j].TaxID;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    taxAmtL1 = HQ.util.mathRound(txblAmtL1 * objTax.TaxRate / 100, 2);//Math.round(txblAmtL1 * objTax.TaxRate / 100, 2);

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
                                taxAmtL1 = HQ.util.mathRound(det.TranAmt - (totPrcTaxInclAmt + txblAmtL1), 2); //Math.round(det.TranAmt - (totPrcTaxInclAmt + txblAmtL1), 2);

                        }
                        else
                            totPrcTaxInclAmt += totPrcTaxInclAmt + taxAmtL1;
                    }

                    insertUpdateTax(TaxID, lineRef, taxRate, taxAmtL1, txblAmtL1, 1);

                }
            }
        }
    }

    for (var j = 0; j < dt.Count; j++) {
        var TaxID = "", lineRef = "";
        var taxRate = 0, txblAmtL2 = 0, taxAmtL2 = 0;
        var objTax = HQ.store.findInStore(App.stoAP10100_pdSI_Tax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "2") {
                    TaxID = dt[j].TaxID;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    txblAmtL2 = Math.round(txblAmtAddL2 + txblAmtL1, 0);
                    taxAmtL2 = HQ.util.mathRound(txblAmtAddL2 * objTax.TaxRate / 100, 2);//Math.round(txblAmtAddL2 * objTax.TaxRate / 100, 2);
                    insertUpdateTax(TaxID, lineRef, taxRate, taxAmtL2, txblAmtL2, 2);
                }
            }
        }
    }
    updateTax(index);
    calcDet();
    return true;
};
function insertUpdateTax(TaxID, lineRef, taxRate, taxAmt, txblAmt, taxLevel) {
    var flat = false;
    for (var i = 0; i < App.stoAP10100_pgLoadTaxTrans.data.length; i++) {
        if (App.stoAP10100_pgLoadTaxTrans.data.items[i].data.TaxID == TaxID && App.stoAP10100_pgLoadTaxTrans.data.items[i].data.LineRef == lineRef) {
            var tax = App.cboTaxID.getStore().data.items[i];
            tax.set('BranchID', App.txtBranchID.getValue()),
            tax.set('TaxID', TaxID);
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
        var newTax = Ext.create('App.ModelAP10100_pgLoadTaxTrans_Result');
        newTax.data.BranchID = App.txtBranchID.getValue();
        newTax.data.TaxID = TaxID;
        newTax.data.LineRef = lineRef;
        newTax.data.TaxRate = taxRate;
        newTax.data.TaxLevel = taxLevel.toString();
        newTax.data.TaxAmt = taxAmt;
        newTax.data.TxblAmt = txblAmt;

        App.stoAP10100_pgLoadTaxTrans.data.add(newTax);
    }
    App.stoAP10100_pgLoadTaxTrans.sort('LineRef', "ASC");
    calcDet();

};
function updateTax(index) {

    if (index < 0) return;
    var j = 0;
    var det = App.stoAP_Trans.data.items[index].data;
    var record = App.stoAP_Trans.data.items[index];
    for (var i = 0; i < App.stoAP10100_pgLoadTaxTrans.data.length; i++) {
        var item = App.stoAP10100_pgLoadTaxTrans.data.items[i];
        if (item.data.LineRef == det.LineRef) {
            if (j == 0) {
                record.set('TaxId00', item.data.TaxID);
                record.set('TxblAmt00', item.data.TxblAmt);
                record.set('TaxAmt00', item.data.TaxAmt);
            }
            else if (j == 1) {
                record.set('TaxId01', item.data.TaxID);
                record.set('TxblAmt01', item.data.TxblAmt);
                record.set('TaxAmt01', item.data.TaxAmt);
            }
            else if (j == 2) {
                record.set('TaxId02', item.data.TaxID);
                record.set('TxblAmt02', item.data.TxblAmt);
                record.set('TaxAmt02', item.data.TaxAmt);
            }
            else if (j == 3) {
                record.set('TaxId03', item.data.TaxID);
                record.set('TxblAmt03', item.data.TxblAmt);
                record.set('TaxAmt03', item.data.TaxAmt);
            }
            j++;
        }
        if (j != 0 && item.data.LineRef != det.LineRef)
            return false;
    };

};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
function deleteHeader(item) {
    if (item == 'yes') {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                method: 'POST',
                url: 'AP10100/DeleteHeader',
                timeout: 180000,
                params: {
                    lstHeader: Ext.encode(App.stoAP10100_pdHeader.getRecordsValues())

                },
                success: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    var batNbr = '';
                    if (this.result.data != undefined && this.result.data.batNbr != null) {
                        batNbr = this.result.data.batNbr;

                    }
                    App.cboBatNbr.getStore().load(function () {
                        App.cboBatNbr.setValue(batNbr);
                        App.stoAP10100_pdHeader.reload();
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

    if (item == 'yes') {
        if (App.slmgrdAP_Trans.selected.items[0].data.tstamp != "") {
            App.grdAP_Trans.deleteSelected();
            delTaxMutil();
            calcDet();
            App.frmMain.getForm().updateRecord();
            if (App.frmMain.isValid()) {
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang('DeletingData'),
                    method: 'POST',
                    url: 'AP10100/DeleteGrd',
                    timeout: 180000,
                    params: {
                        lstDel: HQ.store.getData(App.stoAP_Trans),
                        lstDet: Ext.encode(App.stoAP_Trans.getRecordsValues()),
                        lstHeader: Ext.encode(App.stoAP10100_pdHeader.getRecordsValues())

                    },
                    success: function (msg, data) {
                        HQ.message.process(msg, data, true);
                        var batNbr = '';
                        if (this.result.data != undefined && this.result.data.batNbr != null) {
                            batNbr = this.result.data.batNbr;

                        }
                        App.cboBatNbr.getStore().load(function () {
                            App.cboBatNbr.setValue(batNbr);
                            App.stoAP10100_pdHeader.reload();
                        });
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
        } else {
            App.grdAP_Trans.deleteSelected();
            delTaxMutil();
            calcDet();

        }
    }
};


var save = function Save() {
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'AP10100/Save',
            params: {
                lstHeader: Ext.encode(App.stoAP10100_pdHeader.getRecordsValues()),
                lstgrd: Ext.encode(App.stoAP_Trans.getRecordsValues())
            },
            success: function (msg, data) {
                var batNbr = '';
                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    batNbr = this.result.data.batNbr;
                }
                HQ.message.process(msg, data, true);
                App.cboBatNbr.getStore().load(function () {
                    App.cboBatNbr.setValue(batNbr);
                    App.stoAP10100_pdHeader.reload();
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};


var AP10100refresh = function (item) {
    if (item == 'yes') {
        HQ.isChange = false;
        menuClick("refresh");

    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ///////////////////////////////////////////////////


var rendererLineType = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboLineType.store, ['Code'], [record.data.LineType])
    if (Ext.isEmpty(r)) {
        return value
    }
    return r.data.Descr;

};

var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'AP10100?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
}

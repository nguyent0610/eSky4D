//// Declare //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////
var keys = ['TranAmt'];
var fieldsCheckRequire = ["TranAmt"];
var fieldsLangCheckRequire = ["TranAmt"];
HQ.objBatch = null;

var _Source = 0;
var _maxSource = 5;
var _isLoadMaster = false;
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true
        _Source = 0;
        App.stoBatch.reload();
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header')
                HQ.combo.first(App.cboBatNbr);
            else if (HQ.focus == 'mider')
            {
                HQ.combo.first(App.cboBatNbr);
            }
            else if (HQ.focus == 'grdAPTrans') {
                HQ.grid.first(App.grdAPTrans);
            }
            break;
           
        case "prev":
            if (HQ.focus == 'header')
                HQ.combo.prev(App.cboBatNbr);
            else if (HQ.focus == 'mider') {
                HQ.combo.prev(App.cboBatNbr);
            }
            else if (HQ.focus == 'grdAPTrans') {
                HQ.grid.prev(App.grdAPTrans);
            }
            break;
        case "next":
            if (HQ.focus == 'header')
                HQ.combo.next(App.cboBatNbr);
            else if (HQ.focus == 'mider') {
                HQ.combo.next(App.cboBatNbr);
            }
            else if (HQ.focus == 'grdAPTrans') {
                HQ.grid.next(App.grdAPTrans);
            }
            break;

        case "last":
            if (HQ.focus == 'header')
                HQ.combo.last(App.cboBatNbr);
            else if (HQ.focus == 'mider') {
                HQ.combo.last(App.cboBatNbr);
            }
            else if (HQ.focus == 'grdAPTrans') {
                HQ.grid.last(App.grdAPTrans);
            }
            break;

        case "refresh":
            if (HQ.isChange == true) {
                HQ.message.show(20150303, '', 'AP10200refresh');
            }
            else {
                HQ.isFirstLoad = true;
                HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
                if (App.cboBatNbr.valueModels == null)
                    App.cboBatNbr.setValue('');
                App.stoBatch.reload();

            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header' || HQ.focus =='mider') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboBatNbr.forceSelection = false
                        App.cboBatNbr.events['change'].suspend();
                        App.cboBatNbr.setValue('');
                        App.cboBatNbr.events['change'].resume();
                        App.stoBatch.reload();
                    }
                }
                else if (HQ.focus == 'grdAPTrans') {
                    if (App.cboStatus.getValue() != 'H')
                        return;
                    HQ.grid.insert(App.grdAPTrans, keys);
                }
            }
            break;
        case "delete":
            if (App.cboBatNbr.value != "" && App.cboStatus.value != "C" && App.cboStatus.value != "V") {
                if (HQ.isDelete) {
                    if (HQ.focus == 'header' || HQ.focus == 'mider') {
                        HQ.message.show(11, '', 'deleteHeader');
                    }
                    else if (HQ.focus == 'grdAPTrans') {
                        if (App.grdAPTrans.hasSelection() == true) {
                            if (App.slmAPTrans.selected.items[0] != undefined) {
                                HQ.message.show(2015020807, [indexSelect(App.grdAPTrans), ''], 'deleteData', true);
                            }
                        }
                    }
                }
            }
            break;
        case "save":
            if (App.cboStatus.getValue() == "V" || ((Ext.isEmpty(App.cboHandle.getValue()) || App.cboHandle.getValue() == "N") && App.cboStatus.getValue() == "C"))
                return;
            if (App.txtCuryDocBal.getValue() == '0') {
                HQ.message.show(704, '', '');
                return;
            }
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoAPTrans, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
    }
};

var firstLoad = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.txtBranchID.setValue(HQ.cpnyID);
    App.cboBatNbr.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboDocType.getStore().addListener('load', checkLoad);
    App.cboVendID.getStore().addListener('load', checkLoad);
    App.cboBankAcct.getStore().addListener('load', checkLoad);
    HQ.util.checkAccessRight();
};

var cboBatNbr_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null && !App.stoBatch.loading && !App.stoAP_Doc.loading && !App.stoAPTrans.loading)
    {
        App.cboRefNbr.getStore().reload();
        App.stoBatch.reload();
    }
};

var cboStatus_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null) {
        App.cboHandle.getStore().reload();
        if (App.cboStatus.getValue() != "H") {
            HQ.common.lockItem(App.frmMain, true);
            App.menuClickbtnDelete.disable();
        }
        else {
            HQ.common.lockItem(App.frmMain, false);
            App.menuClickbtnDelete.enable();
        }
        App.cboHandle.setValue('N');
    }
};

var cboVendID_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null) {
        App.cboPONbr.getStore().reload();
        App.txtVendName.setValue('');
        App.txtAddr.setValue('');
        if (!Ext.isEmpty(newValue))
        {
            var obj = App.cboVendID.getStore().findRecord("VendID", newValue);
            if (obj != null)
            {
                App.txtVendName.setValue(obj.data.name);
                App.txtAddr.setValue(obj.data.Address);
            }
        }
    }
};

//////////////sto load//

var stoBach_load = function (sto) {
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BatNbr");
        record = sto.getAt(0);
        HQ.isNew = true;
        record.set('Status', 'H');
        HQ.common.setRequire(App.frmMain);
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    HQ.objBatch = record;
    App.frmMain.loadRecord(record);
    App.stoAP_Doc.reload();
    sto.commitChanges();
    frmChange();
};

var stoAPDoc_load = function (sto) {
    HQ.isNew = false;
  
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "DocType");
        record = sto.getAt(0);
        if (App.cboDocType.store.data.items[0] != undefined) {
            record.set("DocType", App.cboDocType.store.data.items[0].data.Code);
        }
        record.set('DocDate', new Date());
        HQ.isNew = true;     
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.loadRecord(record);
    if (HQ.isInsert == true)
    {
        if (App.cboStatus.getValue() == "H")
        {
            HQ.isFirstLoad = true;
        }
    }
    if (HQ.isNew == false)
    {
        App.cboDocType.setReadOnly(true);
        App.cboVendID.setReadOnly(true);
    }
    else{
        App.cboDocType.setReadOnly(false);
        App.cboVendID.setReadOnly(false);
    }
    App.stoAPTrans.reload();
    sto.commitChanges();
    frmChange()
};

var stoAPTrans_Load = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AP10200');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    HQ.common.showBusy(false);
    App.stoBatch.commitChanges();
    App.stoAP_Doc.commitChanges();
    frmChange()
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AP10200');
};
//trước khi load trang busy la dang load data

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
// reject // 
var grdAPTrans_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAPTrans);
    stoChanged(App.stoSpecialDet);
    setAmt();
};

var grdAPTrans_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H')
        return;
    if (Ext.isEmpty(App.cboDocType.getValue()))
    {
        HQ.message.show(15, App.cboDocType.fieldLabel);
        return false;
    }
    if (Ext.isEmpty(App.cboVendID.getValue()))
    {
        HQ.message.show(15, App.cboVendID.fieldLabel);
        return false;
    }
    if (Ext.isEmpty(e.record.data.LineRef)) {
        e.record.data.LineRef = lastLineRef();
        e.record.data.BranchID = HQ.cpnyID;
        e.record.data.BatNbr = HQ.objBatch.data.BatNbr;
        e.record.commit();
    }
    return HQ.grid.checkBeforeEdit(e, keys); 
};

var grdAPTrans_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdAPTrans, e, keys);
    if (e.field == 'TranAmt')
    {
        e.record.set('TranDesc', App.cboVendID.getValue() + "-" + App.txtVendName.getValue());
    }
    setAmt();
    
};

var grdAPTrans_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAPTrans, e, keys);
};

var checkValidateEdit = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
    }
};

var frmChange = function () {
    if (App.frmMain.getRecord() != undefined ) {
        App.frmMain.updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoBatch);
        if (!HQ.isChange)
        {
            HQ.isChange = HQ.store.isChange(App.stoAP_Doc);
        }
        if (!HQ.isChange)
        {
            HQ.isChange = HQ.store.isChange(App.stoAPTrans);
        }
        HQ.common.changeData(HQ.isChange, 'AP10200');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        App.cboBatNbr.setReadOnly(HQ.isChange);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var deleteData = function (item) {
    if (item == "yes") {
        if (Ext.isEmpty(App.slmAPTrans.selected.items[0].data.tstamp)) {
            App.grdSKU.deleteSelected();
            setAmt();
        }
        else {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'Ap10200/DeleteTrans',
                timeout: 180000,
                params: {
                    lineRef: App.slmAPTrans.selected.items[0].data.LineRef,
                },
                success: function (msg, data) {
                    if (!Ext.isEmpty(data.result.data.tstamp)) {
                        App.stoBatch.reload();
                    }
                   // App.grdTrans.deleteSelected();
                   // calculate();
                    HQ.message.process(msg, data, true);
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

var deleteHeader = function (item) {
    if (item == 'yes') {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                method: 'POST',
                url: 'AP10200/DeleteHeader',
                timeout: 180000,
                params: {
                    batNbr: App.cboBatNbr.getValue(),
                    branchID: App.txtBranchID.getValue(),
                },
                success: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    var batNbr = '';
                    if (this.result.data != undefined && this.result.data.batNbr != null) {
                        batNbr = this.result.data.batNbr;
                    }
                    App.cboBatNbr.getStore().load(function () {
                        App.cboBatNbr.setValue(batNbr);
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

var save = function () {
    if (Ext.isEmpty(App.cboDocType.getValue()))
    {
        HQ.message.show(15, App.cboDocType.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.cboVendID.getValue())) {
        HQ.message.show(15, App.cboVendID.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.dteDocDate.getValue())) {
        HQ.message.show(15, App.dteDocDate.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.cboBankAcct.getValue())) {
        HQ.message.show(15, App.cboBankAcct.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.txtDocDescr.getValue())) {
        HQ.message.show(15, App.txtDocDescr.fieldLabel);
        return;
    }
    App.frmMain.updateRecord();
    if (App.frmMain.isValid()) {          
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'AP10200/Save',
            type: 'POST',
            params: {
                batch:Ext.encode(App.stoBatch.getRecordsValues()),
                doc: Ext.encode(App.stoAP_Doc.getRecordsValues()),
                trans: Ext.encode(App.stoAPTrans.getRecordsValues()),
                BatNbr: App.cboBatNbr.getValue(),
                RefNbr: App.cboRefNbr.getValue(),
                DocType: App.cboDocType.getValue(),
                VendID: App.cboVendID.getValue(),
                IntRefNbr: App.txtOrigRefNbr.getValue(),
                ReasonCD: App.cboBankAcct.getValue(),
            },
            success: function (msg, data) {
                var batNbr = '';
                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    var batNbr = this.result.data.batNbr
                }
                if (!Ext.isEmpty(batNbr)) {
                    App.cboBatNbr.store.reload();
                    App.cboBatNbr.forceSelection = false
                    App.cboBatNbr.events['change'].suspend();
                    App.cboBatNbr.setValue(batNbr);
                    App.cboBatNbr.events['change'].resume();
                    if (Ext.isEmpty(HQ.recentRecord)) {
                        HQ.recentRecord = batNbr;
                    }
                    App.stoBatch.reload();
                }
                HQ.message.process(msg, data, true);

            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
           
    }
   
};

var AP10200refresh = function (item) {
    if (item == 'yes') {
        HQ.isChange = false;
        menuClick("refresh");

    }
};

var btnImport_Click = function (c, e) {
    if (HQ.isInsert || HQ.isUpdate) {
        var files = c.fileInputEl.dom.files;
        var fileName = c.getValue();
        var ext = fileName.split(".").pop().toLowerCase();
        if (ext == "xls" || ext == "xlsx") {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                url: 'AP10200/Import',
                timeout: 1800000,
                params: {
                    
                },
                success: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    
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
    }
};

var Export = function () {
    App.frmMain.submit({
        timeout: 1800000,
        //waitMsg: HQ.common.getLang("SavingData"),
        clientValidation: false,
        type: 'POST',
        url: 'AP10200/Export',
        params: {
            key: HQ.store.getData(App.stoSpecialDet),
        },
        success: function (msg, data) {
            HQ.message.process(msg, data, true);
            //menuClick("refresh");
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
}
/////////////////////////////////////////////////////////////////////////
//// Other Functions ///////////////////////////////////////////////////

var indexSelect = function (grd) {
    var index = '';
    var arr = grd.getSelectionModel().getSelection();
    arr.forEach(function (itm) {
        index += ((itm.index) + 1) + ',';
        //index += (itm.index == undefined ? grd.getStore().totalCount : itm.index + 1) + ',';
    });

    return index.substring(0, index.length - 1);
};

var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'AP10200?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
}

var lastLineRef = function () {
    var num = 0;

    App.stoAPTrans.data.each(function (item) {
        if (!Ext.isEmpty(item.data.LineRef) && parseInt(item.data.LineRef) > num) {
            num = parseInt(item.data.LineRef);
        }
    });

    num++;
    var lineRef = num.toString();
    var len = lineRef.length;
    for (var i = 0; i < 5 - len; i++) {
        lineRef = "0" + lineRef;
    }
    return lineRef;
}

var calAmt = function (sto) {
    var res = 0;
    var store = sto.allData;
    store.each(function (items) {
        res += items.data.TranAmt;
    })
    return res;
};

var setAmt = function () {
    var amt = calAmt(App.stoAPTrans);
    App.dteCuryCrTot.setValue(amt);
    App.txtCuryDocBal.setValue(amt);
    App.txtCuryOrigDocAmt.setValue(amt);
};
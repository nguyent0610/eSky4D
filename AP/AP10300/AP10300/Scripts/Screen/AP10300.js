//// Declare //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////
var keys = ['TranAmt'];
var fieldsCheckRequire = ["TranAmt"];
var fieldsLangCheckRequire = ["TranAmt"];
HQ.objBatch = null;
var tmpApplicationTotal = 0;
var _Source = 0;
var _maxSource = 4;
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
            else if (HQ.focus == 'Adjusting') {
                HQ.grid.first(App.grdAdjusting);
            }
            else if (HQ.focus == 'Adjusted') {
                HQ.grid.first(App.grdAdjusted);
            }
            break;
           
        case "prev":
            if (HQ.focus == 'header')
                HQ.combo.prev(App.cboBatNbr);
            else if (HQ.focus == 'mider') {
                HQ.combo.prev(App.cboBatNbr);
            }
            else if (HQ.focus == 'Adjusting') {
                HQ.grid.prev(App.grdAdjusting);
            }
            else if (HQ.focus == 'Adjusted') {
                HQ.grid.prev(App.grdAdjusted);
            }
            break;
        case "next":
            if (HQ.focus == 'header')
                HQ.combo.next(App.cboBatNbr);
            else if (HQ.focus == 'mider') {
                HQ.combo.next(App.cboBatNbr);
            }
            else if (HQ.focus == 'Adjusting') {
                HQ.grid.next(App.grdAdjusting);
            }
            else if (HQ.focus == 'Adjusted') {
                HQ.grid.next(App.grdAdjusted);
            }
            break;

        case "last":
            if (HQ.focus == 'header')
                HQ.combo.last(App.cboBatNbr);
            else if (HQ.focus == 'mider') {
                HQ.combo.last(App.cboBatNbr);
            }
            else if (HQ.focus == 'Adjusting') {
                HQ.grid.last(App.grdAdjusting);
            }
            else if (HQ.focus == 'Adjusted') {
                HQ.grid.last(App.grdAdjusted);
            }
            break;

        case "refresh":
            if (HQ.isChange == true) {
                HQ.message.show(20150303, '', 'AP10300refresh');
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
            //if (App.cboBatNbr.value != "" && App.cboStatus.value != "C" && App.cboStatus.value != "V") {
            //    if (HQ.isDelete) {
            //        HQ.message.show(11, '', 'deleteHeader');
            //    }
            //}
            if (HQ.isDelete) {
                if (App.cboStatus.getValue() == 'H') {
                    if (HQ.focus == 'header') {
                        if (App.cboBatNbr.value) {
                            HQ.message.show(11, '', 'deleteHeader');
                        } else {
                            menuClick('new');
                        }
                    }
                    else if (HQ.focus == 'Adjusting') {
                        if (App.slmAdjusting.selected.items[0] != undefined) {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAdjusting)], 'deleteData', true);
                            // }
                        }
                    }
                    else if (HQ.focus == 'Adjusted') {
                        if (App.slmAdjusted.selected.items[0] != undefined) {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAdjusted)], 'deleteData', true);
                            // }
                        }
                    }
                }
            }
            break;
        case "save":
            if (App.cboStatus.getValue() == "V" || ((Ext.isEmpty(App.cboHandle.getValue()) || App.cboHandle.getValue() == "N") && App.cboStatus.getValue() == "C"))
                return;
            if (App.dteCuryCrTot.getValue() == '0') {
                HQ.message.show(704, '', '');
                return;
            }
            if (App.txtOdd.getValue() < 0) {
                HQ.message.show(2015110901, HQ.common.getLang('BALANCE'), '');
                return;
            }
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                 save();
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
    HQ.util.checkAccessRight();
  
};


var cboBatNbr_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null && !App.stoBatch.loading && !App.stoAP_Adjust.loading && !App.stoAdjusting.loading && !App.stoAdjusted.loading)
    {
        HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
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
            if(HQ.isDelete)
            App.menuClickbtnDelete.enable();
        }
        App.cboHandle.setValue('N');
    }
};

var cboVendID_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null) {
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.stoAdjusting.reload();
    }
};


var cboDocType_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null) {
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.stoAdjusting.reload();
    }
};

var txtDescr_Change = function (sender, newValue, oldValue) {
    if (newValue != oldValue) {
        HQ.isChange = true;
        HQ.common.changeData(HQ.isChange, 'AP10300');//co thay doi du lieu gan * tren tab title header
        App.cboBatNbr.setReadOnly(HQ.isChange);
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
    App.stoAP_Adjust.reload();
    sto.commitChanges();
    
     frmChange();
   
};

var stoAPAdjust_load = function (sto) {
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "DocType");
        record = sto.getAt(0);
        if (App.cboDocType.store.data.items[0] != undefined) {
            record.set("AdjgDocType", App.cboDocType.store.data.items[0].data.Code);
        }
        record.set('AdjgDocDate', new Date());
        HQ.isNew = true;     
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.loadRecord(record);
    if (HQ.isInsert == true)
    {
        if (App.cboStatus.getValue() == "H")
        {
            //HQ.isFirstLoad = true;
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
    App.stoAdjusting.reload();
    sto.commitChanges();
   
    frmChange()
};

var stoAdjusting_Load = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AP10300');
   
    //App.stoBatch.commitChanges();
    //App.stoAP_Doc.commitChanges();
    frmChange()
    App.stoAdjusted.reload();
};


var stoAdjusted_Load = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AP10300');
    HQ.common.showBusy(false);
    var docbal = calTotalDocBal(sto);
    App.txtOrigDocAmt.setValue(docbal);
    App.txtUnTotPayment.setValue(docbal);
    App.txtPaid.setValue(calTotalPayment(sto));
    App.dteCuryCrTot.setValue(calTotalPayment(sto));
    total();
    frmChange()
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AP10300');
};
//trước khi load trang busy la dang load data

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
// reject // 

var grdAPAdjusted_Edit = function (item, e) {
   // HQ.grid.checkInsertKey(App.grdAPTrans, e, keys);
    if (e.field == 'Selected')
    {
        if (e.value == true) {
            e.record.set('Payment', e.record.data.DocBal + e.record.data.Payment);
            e.record.set('DocBal', 0);
        }
        else {
            e.record.set('DocBal', e.record.data.DocBal + e.record.data.Payment);//e.record.data.OrigDocBal);
            e.record.set('Payment', 0);
        }
    }
    if (e.field == 'Payment')
    {
        if (e.value >= e.record.data.OrigDocBal) {
            e.record.set('Payment',  e.record.data.OrigDocBal);
            e.record.set('Selected', true);
            e.record.set('DocBal', 0);
        }
        else {
            e.record.set('Selected', false);
            e.record.set('DocBal', e.record.data.OrigDocBal - e.value);
        }
    }
    // setAmt();
    App.txtPaid.setValue(calTotalPayment(App.stoAdjusted));
    App.txtUnTotPayment.setValue(calTotalDocBal(App.stoAdjusted));
    App.dteCuryCrTot.setValue(calTotalPayment(App.stoAdjusted));
    if (App.stoAdjusting.getCount() > 0)
        App.txtOdd.setValue(calTotalPayment(App.stoAdjusting) - App.txtPaid.getValue());
};

var  grd_BeforeEdit= function (editor, e) {
    if (HQ.isUpdate) {
        if (!HQ.isInsert && App.cboBatNbr.getValue() == null)
            return false;
        if(App.cboStatus.value!="H") {
            return false;
        }
    }else
    {
        return false;
    }
};

var grdAdjusting_Edit = function (item, e) {
    if (e.field == 'Selected') {
        if (e.value == true) {
            e.record.set('Payment', e.record.data.DocBal + e.record.data.Payment);
            e.record.set('DocBal', 0);
        }
        else {
            e.record.set('DocBal', e.record.data.DocBal + e.record.data.Payment);//e.record.data.OrigDocBal);
            e.record.set('Payment', 0);
        }
    }
    if (e.field == 'Payment') {
        if (e.value >= e.record.data.OrigDocBal) {
            e.record.set('Payment', e.record.data.OrigDocBal);
            e.record.set('Selected', true);
            e.record.set('DocBal', 0);
        }
        else {
            e.record.set('Selected', false);
            e.record.set('DocBal', e.record.data.OrigDocBal - e.value);
        }
    }
    App.txtPayment.setValue(calTotalPayment(App.stoAdjusting));
    App.txtOdd.setValue(calTotalPayment(App.stoAdjusting) - App.txtPaid.getValue());
    App.dteCuryCrTot.setValue(calTotalPayment(App.stoAdjusting));
};

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

var grdAPTrans_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAPTrans, e, keys);
};

var frmChange = function () {
    if (App.frmMain.getRecord() != undefined) {
        App.frmMain.getForm().updateRecord();
        if (!HQ.isChange)
        HQ.isChange = HQ.store.isChange(App.stoBatch);
        if (!HQ.isChange)
        {
            HQ.isChange = HQ.store.isChange(App.stoAP_Adjust);
        }
        if (!HQ.isChange)
        {
            HQ.isChange = HQ.store.isChange(App.stoAdjusted);
        }
        if (!HQ.isChange)
        {
            HQ.isChange = HQ.store.isChange(App.stoAdjusting);
        }
        HQ.common.changeData(HQ.isChange, 'AP10300');//co thay doi du lieu gan * tren tab title header
        App.cboBatNbr.setReadOnly(HQ.isChange);
        if (!HQ.isUpdate)
            App.txtDescr.setReadOnly(true);
        
    }
};

var AP10300refresh = function (item) {
    if (item == 'yes') {
        HQ.isChange = false;
        menuClick("refresh");

    }
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////

var deleteData = function (item) {
    if (item == "yes") {
        //if (HQ.focus == 'header') {
        //    if (App.frmMain.isValid()) {
        //        App.frmMain.updateRecord();
        //        App.frmMain.submit({
        //            waitMsg: HQ.common.getLang("DeletingData"),
        //            url: 'AP10400/DeleteAll',
        //            timeout: 7200,
        //            success: function (msg, data) {
        //                App.cboBatNbr.getStore().load();
        //                menuClick("new");
        //            },
        //            failure: function (msg, data) {
        //                HQ.message.process(msg, data, true);
        //            }
        //        });
        //    }

        //}
        //else
        if (HQ.focus == 'Adjusting') {
           // var rec = App.stoAdjusting.findRecord("RefNbr", App.grdAdjusting.selModel.selected.items[0].data.RefNbr);//
            var recordAdjusting = HQ.store.findInStore(App.grdAdjusting.store,
                                      ['BatNbr', 'RefNbr'],
                                      [App.grdAdjusting.selModel.selected.items[0].data.BatNbr, App.grdAdjusting.selModel.selected.items[0].data.RefNbr]);
            //recordAdjusting.set('Payment', App.grdAdjusting.selModel.selected.items[0].data.DocBal);
            //recordAdjusting.set('DocBal', 0);
            //   App.grdAdjusting.deleteSelected();
            //App.grdAdjusting.selModel.selected.items[0].data.Payment = App.grdAdjusting.selModel.selected.items[0].data.DocBal;
            //App.grdAdjusting.selModel.selected.items[0].data.DocBal = 0;
            //item.set('Payment', item.data.DocBal)
            //item.set('DocBal', 0);

            var selRecs = App.grdAdjusting.selModel.selected;
            selRecs.items[0].set("Selected", false);
            selRecs.items[0].set("DocBal", App.grdAdjusting.selModel.selected.items[0].data.Payment + App.grdAdjusting.selModel.selected.items[0].data.DocBal);
            selRecs.items[0].set("Payment", 0);
          
        }
        else if (HQ.focus == 'Adjusted')
        {
            //App.grdAdjusted.deleteSelected();
            //App.grdAdjusted.selModel.selected.items[0].data.Payment = App.grdAdjusted.selModel.selected.items[0].data.DocBal;
            //App.grdAdjusted.selModel.selected.items[0].data.DocBal = 0;
            var selRecs = App.grdAdjusted.selModel.selected;
            selRecs.items[0].set("Selected", false);
            selRecs.items[0].set("DocBal", App.grdAdjusted.selModel.selected.items[0].data.Payment + App.grdAdjusted.selModel.selected.items[0].data.DocBal);
            selRecs.items[0].set("Payment", 0);
         
        }

        frmChange();
        total();
    }
};


var deleteHeader = function (item) {
    if (item == 'yes') {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                method: 'POST',
                url: 'AP10300/Delete',
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
                    App.cboBatNbr.forceSelection = false
                    App.cboBatNbr.events['change'].suspend();
                    App.cboBatNbr.setValue(batNbr);
                    App.cboBatNbr.events['change'].resume();
                    App.cboBatNbr.getStore().reload();
                    App.stoBatch.reload();
                   
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
    if (Ext.isEmpty(App.txtDocDate.getValue())) {
        HQ.message.show(15, App.txtDocDate.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.txtDescr.getValue())) {
        HQ.message.show(15, App.txtDescr.fieldLabel);
        return;
    }
    App.frmMain.updateRecord();
    if (App.frmMain.isValid()) {          
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'AP10300/Save',
            type: 'POST',
            params: {
                batch:Ext.encode(App.stoBatch.getRecordsValues()),
                adjust: Ext.encode(App.stoAP_Adjust.getRecordsValues()),
                lstAdjusting: Ext.encode(App.stoAdjusting.getRecordsValues()),
                lstAdjusted: Ext.encode(App.stoAdjusted.getRecordsValues()),
            },
            success: function (msg, data) {
                var batNbr = '';
                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    var batNbr = this.result.data.batNbr
                }
                if (!Ext.isEmpty(batNbr)) {
                    App.cboBatNbr.forceSelection = false
                    App.cboBatNbr.events['change'].suspend();
                    App.cboBatNbr.setValue(batNbr);
                    App.cboBatNbr.events['change'].resume();
                    if (Ext.isEmpty(HQ.recentRecord)) {
                        HQ.recentRecord = batNbr;
                    }
                    App.cboBatNbr.getStore().reload();
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



var btnImport_Click = function (c, e) {
    if (HQ.isInsert || HQ.isUpdate) {
        var files = c.fileInputEl.dom.files;
        var fileName = c.getValue();
        var ext = fileName.split(".").pop().toLowerCase();
        if (ext == "xls" || ext == "xlsx") {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                url: 'AP10300/Import',
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
        url: 'AP10300/Export',
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
        window.location.href = 'AP10300?branchID=' + App.cboPopupCpny.getValue();
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

var calTotalDocBal = function (sto) {
    var res = 0;
    var store = sto.allData;
    store.each(function (items) {
        res += items.data.DocBal;
    })
    return res;
};

var calTotalPayment = function (sto) {
    var res = 0;
    var store = sto.allData;
    store.each(function (item) {
        res += item.data.Payment;
    })
    return res;
};

var AdjustedCheckAll_Change = function () {
    var value = App.AdjustedCheckAll.getValue();
    App.stoAdjusted.suspendEvents();
    var store = App.stoAdjusted.allData;
    if (value == true) {
        store.each(function (item) {
            item.set('Selected', true);
            item.set('Payment', item.data.DocBal + item.data.Payment)
            item.set('DocBal', 0);
        })
    }
    else {
        store.each(function (item) {
            item.set('Selected', false);
            item.set('DocBal', item.data.DocBal + item.data.Payment);
            item.set('Payment', 0)
          
        })
    }
    App.stoAdjusted.resumeEvents();
    App.txtPaid.setValue(calTotalPayment(App.stoAdjusted));
    App.txtUnTotPayment.setValue(calTotalDocBal(App.stoAdjusted));
    App.dteCuryCrTot.setValue(calTotalPayment(App.stoAdjusted));
    if (App.stoAdjusting.getCount() > 0)
        App.txtOdd.setValue(calTotalPayment(App.stoAdjusting) - App.txtPaid.getValue());

};

var AdjustingCheckAll_Change = function () {
    var value = App.AdjustingCheckAll.getValue();
    App.stoAdjusting.suspendEvents();
    var store = App.stoAdjusting.allData;
    if (value == true) {
        store.each(function (item) {
            item.set('Selected', true);
            item.set('Payment', item.data.DocBal + item.data.Payment)
            item.set('DocBal', 0);
        })
    }
    else {
        store.each(function (item) {
            item.set('Selected', false);
            item.set('DocBal', item.data.DocBal +item.data.Payment);
            item.set('Payment', 0)
            //item.set('DocBal', item.data.OrigDocBal);
        })
    }
    App.stoAdjusting.resumeEvents();
    App.txtPayment.setValue(calTotalPayment(App.stoAdjusting));
    App.txtOdd.setValue(calTotalPayment(App.stoAdjusting) - App.txtPaid.getValue());
    App.dteCuryCrTot.setValue(calTotalPayment(App.stoAdjusting));
};

//Khi an nut Auto Apply dem gia tri cua o Balance sang o Total Amount
var AutoAssign_Click = function () {
    if (App.cboStatus.getValue() == "H") {
        var dblSum = App.txtPayment.value;
        for (var i = 0; i < App.stoAdjusted.data.length; i++) {
            App.stoAdjusted.data.items[i].set("Payment", 0);
            App.stoAdjusted.data.items[i].set("Selected", false)
            if (dblSum > App.stoAdjusted.data.items[i].data.OrigDocBal) {
                dblSum = dblSum - App.stoAdjusted.data.items[i].data.OrigDocBal;
                App.stoAdjusted.data.items[i].set("Payment", App.stoAdjusted.data.items[i].data.OrigDocBal);
                App.stoAdjusted.data.items[i].set("Selected", true)
            }
            else if (dblSum > 0) {
                App.stoAdjusted.data.items[i].set("Selected", false);
                App.stoAdjusted.data.items[i].set("Payment", dblSum);
                dblSum = 0;
            }
            App.stoAdjusted.data.items[i].set("DocBal", App.stoAdjusted.data.items[i].data.OrigDocBal - App.stoAdjusted.data.items[i].data.Payment);
        }
        total();
    }
 
};

var total = function () {
    var totalPaymentted = 0; var totalOrigDocBalted = 0;
    var totalPaymentAdjting = 0;
    for (var i = 0; i < App.stoAdjusted.data.length; i++) {
        totalPaymentted += App.stoAdjusted.data.items[i].data.Payment;
        totalOrigDocBalted += App.stoAdjusted.data.items[i].data.OrigDocBal;
    }
    for (var i = 0; i < App.stoAdjusting.data.length; i++) {
        totalPaymentAdjting += App.stoAdjusting.data.items[i].data.Payment;
    }
    App.txtPaid.setValue(totalPaymentted);
    App.dteCuryCrTot.setValue(totalPaymentted);
    App.txtOrigDocAmt.setValue(totalOrigDocBalted);
    App.txtPayment.setValue(totalPaymentAdjting);
    App.txtOdd.setValue(totalPaymentAdjting - totalPaymentted);
};

var keysTab_2 = ['TranAmt'];
var fieldsCheckRequireTab_2 = ['TranAmt'];
var fieldsLangCheckRequireTab_2 = ['TranAmt'];

var BatNbr = '';
var _Source = 0;
var _maxSource = 6;
var _isLoadMaster = false;

var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        App.txtBranchID.setValue(HQ.cpnyID);
        _isLoadMaster = true;
        _Source = 0;
        App.stoHeader.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboBatNbr.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboDocType.getStore().addListener('load', checkLoad);
    App.cboCustId.getStore().addListener('load', checkLoad);
    App.cboDebtCollector.getStore().addListener('load', checkLoad);
    App.cboBankAcct.getStore().addListener('load', checkLoad);
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus  == 'header') {
                HQ.combo.first(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.first(App.grdDetail);
            }
            break;
        case "prev":
            if (HQ.focus  == 'header') {
                HQ.combo.prev(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.prev(App.grdDetail);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.next(App.grdDetail);
            }
            break;
        case "last":
            if (HQ.focus  == 'header') {
                HQ.combo.last(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.last(App.grdDetail);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus  == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    } else {
                        BatNbr = '';
                        App.cboBatNbr.setValue('');
                        App.stoHeader.reload();
                    }
                }
                else if (HQ.focus  == 'pnlDetail') {
                    HQ.grid.insert(App.grdDetail);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboStatus.getValue() == 'H') {
                    if (HQ.focus == 'header') {
                        if (App.cboBatNbr.value) {
                            HQ.message.show(11, '', 'deleteData');
                        } else {
                            menuClick('new');
                        }
                    }
                    else if (HQ.focus == 'pnlDetail') {
                        if (App.slmDetail.selected.items[0] != undefined) {
                            if (App.slmDetail.selected.items[0].data.TranAmt != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdDetail)], 'deleteData', true);
                            }
                        }
                    }
                }
                else {
                    HQ.message.show(2015020805,[App.cboBatNbr.getValue()],'',true);
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoDetail, keysTab_2, fieldsCheckRequireTab_2, fieldsLangCheckRequireTab_2))
                {
                    if (checkGrid(App.stoDetail,'TranAmt')==true) {
                        save();
                    }
                    else {
                        HQ.message.show(704, '','');
                    }
                }
            }
            break;
        case "print":
            break;
        case "close":
            break;
    }
};

var checkGrid = function (store, field) {
    var count = 0;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data[field]) {
            count++;
            return false;
        }
    });
    if (count > 0)
        return true;
    else
        return false;
};

var cboBatNbr_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoHeader.loading && sender.hasFocus) {
        BatNbr = value;
        App.stoHeader.reload();
    }
};

var cboBatNbr_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoHeader.loading && sender.hasFocus) {
        BatNbr = value;
        App.stoHeader.reload();
    }
};

var frmChange = function () {
    if (App.stoHeader.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoHeader) == false ? (HQ.store.isChange(App.stoDetail)): true;
    HQ.common.changeData(HQ.isChange, 'AR10300');
    if (App.cboBatNbr.valueModels == null || HQ.isNew == true) {
        App.cboBatNbr.setReadOnly(false);
    }
    else {
        App.cboBatNbr.setReadOnly(HQ.isChange);
    }
    if (checkGrid(App.stoDetail, 'TranAmt') == true) {
        App.cboDocType.setReadOnly(true);
        App.cboCustId.setReadOnly(true);
    }
    else {
        App.cboDocType.setReadOnly(false);
        App.cboCustId.setReadOnly(false);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.setForceSelection(App.frmMain, false, "cboBatNbr");
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BatNbr");
        record = sto.getAt(0);
        record.set('Status', 'H');
        record.set('DocType', 'PP');
        record.set('DocDate', HQ.bussinessDate);
        HQ.isNew = true;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboBatNbr.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.stoDetail.reload();
    
    if (record.data.Status == 'H')
        HQ.common.lockItem(App.frmMain, false);
    else
        HQ.common.lockItem(App.frmMain, true);

    if (!HQ.isInsert && HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
};

//var lockControl = function (value) {
//    //setTimeout(function () {
//        App.cboVendor.setReadOnly(value);
//        App.cboCash.setReadOnly(value);
//        App.txtDescr.setReadOnly(value);
//        App.lblPODate.setReadOnly(value)
//    //}, 300);
//};

/////////////////////////////// GIRD AR_Trans /////////////////////////////////
var stoDetail_Load = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysTab_2);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    total();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdDetail_BeforeEdit = function (editor, e) {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        if (App.cboStatus.getValue() != 'H') return false;
        if (!HQ.grid.checkBeforeEdit(e, keysTab_2)) return false;
    }
    else
        return false;
};

var grdDetail_Edit = function (item, e) {
    if (e.field == 'TranAmt') {
        if (!Ext.isEmpty(e.value)) {
            e.record.set('LineRef', HQ.store.lastLineRef(App.stoDetail));
            if (Ext.isEmpty(e.record.data.TranDesc))
                e.record.set('TranDesc', App.cboCustId.valueModels[0].data.CustID + ' - ' + App.cboCustId.valueModels[0].data.Name);
            total();
        }
    }
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdDetail, e, keysTab_2);
    frmChange();
};

var grdDetail_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    //return HQ.grid.checkValidateEdit(App.grdDetail, e, keysTab_2);
};

var grdDetail_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdDetail);
    frmChange();
};

//Process
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            timeout: 18000,
            url: 'AR10300/Save',
            params: {
                lstHeader: Ext.encode(App.stoHeader.getRecordsValues()),
                lstgrd: Ext.encode(App.stoDetail.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                BatNbr = data.result.BatNbr;
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                //App.cboHandle.setValue('');
                App.cboBatNbr.getStore().reload({
                    callback: function () {
                        if (Ext.isEmpty(App.cboBatNbr.getValue())) {
                            App.cboBatNbr.setValue(data.result.BatNbr);
                            App.stoHeader.reload();
                        }
                        else {
                            App.cboBatNbr.setValue(data.result.BatNbr);
                            App.stoHeader.reload();
                        }
                    }
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        if (HQ.focus == 'header') {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'AR10300/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        App.cboBatNbr.getStore().load();
                        menuClick("new");
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (HQ.focus == 'pnlDetail') {
            App.grdDetail.deleteSelected();
            frmChange();
            total();
        }
    }
};

function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew) {
            BatNbr = ''
            App.cboBatNbr.setValue('');
        }
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoHeader.reload();
    }
};

var total = function () {
    var totalAmt = 0;
    var store = App.stoDetail;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data.TranAmt) {
            totalAmt += record.data.TranAmt;
        }
    });
    App.txtCuryCrTot.setValue(totalAmt);
    App.txtCuryOrigDocAmt.setValue(totalAmt);
    App.txtCuryDocBal.setValue(totalAmt);
};

var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'AR10300?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
};

//// Declare //////////////////////////////////////////////////////////
var keys = ['Structure','InvtID'];
var fieldsCheckRequire = ["Structure","InvtID"];
var fieldsLangCheckRequire = ["OM23300Structure","OM23300InvtID"];

var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;


///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_POSMStructure.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.first(App.cboPosmID, HQ.isChange);
            }
            else if (HQ.focus == 'grdOM_POSMStructure') {
                HQ.grid.first(App.grdOM_POSMStructure);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboPosmID, HQ.isChange);
            }
            else if (HQ.focus == 'grdOM_POSMStructure') {
                HQ.grid.prev(App.grdOM_POSMStructure);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboPosmID, HQ.isChange);
            }
            else if (HQ.focus == 'grdOM_POSMStructure') {
                HQ.grid.next(App.grdOM_POSMStructure);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboPosmID, HQ.isChange);
            }
            else if (HQ.focus == 'grdOM_POSMStructure') {
                HQ.grid.last(App.grdOM_POSMStructure);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_POSMStructure.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboPosmID.setValue('');
                        App.stoOM_POSMStructure.reload();
                    }
                }
                else {
                    HQ.grid.insert(App.grdOM_POSMStructure, keys);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (HQ.focus == 'header') {
                    if (App.cboPosmID.getValue()) {
                        HQ.message.show(11, '', 'deleteData');
                    } else {
                        menuClick('new');
                    }
                }
                else {
                    if (App.slmOM_POSMStructure.selected.items[0] != undefined) {
                        if (App.slmOM_POSMStructure.selected.items[0].data.CpnyID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_POSMStructure)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoOM_POSMStructure, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);            
            break;
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboPosmID.getStore().addListener('load', checkLoad);
    App.cboInvtID.getStore().addListener('load', checkLoad);
};

var cboPosmID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && sender.valueModels.length > 0 && !App.stoOM_POSMStructure.loading) {
        App.stoOM_POSMStructure.reload();
    }
};

var cboPosmID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && sender.valueModels.length > 0 && !App.stoOM_POSMStructure.loading) {
        App.stoOM_POSMStructure.reload();
    }
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoOM_POSMStructure);
    HQ.common.changeData(HQ.isChange, 'OM23300');//co thay doi du lieu gan * tren tab title header
    if (App.cboPosmID.valueModels == null || HQ.isNew == true)
        App.cboPosmID.setReadOnly(false);
    else App.cboPosmID.setReadOnly(HQ.isChange);
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoOM_POSMStructure_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    //Sto tiep theo
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdOM_POSMStructure_BeforeEdit = function (editor, e) {
    //if (e.field == 'Structure') {
    //    if (HQ.isUpdate == true)
    //        return true;
    //    else if (HQ.isInsert == true && Ext.isEmpty(e.record.data.tstamp))
    //        return true;
    //}
    if (!HQ.grid.checkBeforeEdit(e,keys)) return false;
};

var grdOM_POSMStructure_Edit = function (item, e) {
    if (e.field == 'InvtID') {
        if (e.value) {
            e.record.set('PosmID', App.cboPosmID.getValue());
            e.record.set('CnvFact', 1);
            var objInvt = App.cboInvtIDOM23300_pcInvtID.findRecord(['InvtID'], [e.value]);
            if (objInvt) {
                e.record.set('Descr', objInvt.data.Descr);
                e.record.set('SlsPrice', objInvt.data.SlsPrice);
            }
        }
    }
    else if (e.field == 'CnvFact') {
        if (e.value < 1) {
            e.record.set(e.field, e.originalValue);
            HQ.message.show(2015110901, [HQ.common.getLang('CnvFact')], '', true);
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_POSMStructure, e, keys);
    frmChange();
};

var grdOM_POSMStructure_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_POSMStructure, e, keys);
};

var grdOM_POSMStructure_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_POSMStructure);
    frmChange();
};

////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM23300/Save',
            params: {
                lstOM_POSMStructure: HQ.store.getData(App.stoOM_POSMStructure)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoOM_POSMStructure.reload();
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
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'OM23300/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        HQ.isFirstLoad = true;
                        App.stoOM_POSMStructure.reload();
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (HQ.focus == 'grdOM_POSMStructure') {
            App.grdOM_POSMStructure.deleteSelected();
            frmChange();
        }
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_POSMStructure.reload();
    }
};
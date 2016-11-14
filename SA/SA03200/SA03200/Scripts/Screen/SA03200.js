//// Declare //////////////////////////////////////////////////////////
var keys = ['PDAID', 'BranchID', 'SlsperId'];
var fieldsCheckRequire = ["PDAID", "BranchID", "SlsperId", "LicenseKey"];
var fieldsLangCheckRequire = ["PDAID", "BranchID", "SlsperId", "LicenseKey"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoPPC_License.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboBranchID.getStore().addListener('load', checkLoad);
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdPPC_License);
            break;
        case "prev":
            HQ.grid.prev(App.grdPPC_License);
            break;
        case "next":
            HQ.grid.next(App.grdPPC_License);
            break;
        case "last":
            HQ.grid.last(App.grdPPC_License);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdPPC_License, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmPPC_License.selected.items[0] != undefined) {
                    if (App.slmPPC_License.selected.items[0].data.PDAID != "" &&
                        App.slmPPC_License.selected.items[0].data.BranchID != "" &&
                        App.slmPPC_License.selected.items[0].data.SlsperId != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdPPC_License)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoPPC_License, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            break;
    }
};

var StatusCheckAll_Change = function (sender, value) {
    if (sender.hasFocus) {
        HQ.isChange = true;
        var store = App.stoPPC_License;
        var allRecords = store.snapshot || store.allData || store.data;
        store.suspendEvents();
        allRecords.each(function (record) {
            if (record.data.PDAID && record.data.BranchID && record.data.SlsperId)
                record.set('Status', value);
        });
        store.resumeEvents();
        App.grdPPC_License.view.refresh();
    }
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03200');
};

var stoLoad = function (sto) {
    App.StatusCheckAll.setValue(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03200');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keys);
            HQ.store.insertRecord(sto, keys, { LastSyncDate: new Date(HQ.bussinessDate) });
        }
        HQ.isFirstLoad = false;
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var grdPPC_License_BeforeEdit = function (editor, e) {
    if (e.field == 'SlsperId') {
        if (e.record.data.BranchID) {
            App.cboSlsperId.store.reload();
        }
        else {
            return false;
        }
    }
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdPPC_License_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdPPC_License, e, keys);
};

var grdPPC_License_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdPPC_License, e, keys, false);
};

var grdPPC_License_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdPPC_License);
    stoChanged(App.stoPPC_License);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA03200/Save',
            params: {
                lstPPC_License: HQ.store.getData(App.stoPPC_License)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdPPC_License.deleteSelected();
        stoChanged(App.stoPPC_License);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoPPC_License.reload();
    }
};
///////////////////////////////////

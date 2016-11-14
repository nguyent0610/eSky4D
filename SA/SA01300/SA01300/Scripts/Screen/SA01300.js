//// Declare //////////////////////////////////////////////////////////
var keys = ['Code'];
var fieldsCheckRequire = [];
var fieldsLangCheckRequire = [];

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
        App.stoSYS_Configurations.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    checkLoad();
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Configurations);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Configurations);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Configurations);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Configurations);
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
                HQ.grid.insert(App.grdSYS_Configurations, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmData.selected.items[0] != undefined) {
                    if (App.slmData.selected.items[0].data.Code != ""
                        ) {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_Configurations)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Configurations, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA01300');
};

var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA01300');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var grdSYS_Configurations_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSYS_Configurations_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Configurations, e, keys);
};

var grdSYS_Configurations_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Configurations, e, keys);
};

var grdSYS_Configurations_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Configurations);
    stoChanged(App.stoSYS_Configurations);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            timeout: 1800000,
            url: 'SA01300/Save',
            params: {
                lstSYS_Configurations: HQ.store.getData(App.stoSYS_Configurations)
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
        App.grdSYS_Configurations.deleteSelected();
        stoChanged(App.stoSYS_Configurations);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Configurations.reload();
    }
};
///////////////////////////////////

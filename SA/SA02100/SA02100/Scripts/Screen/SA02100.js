//// Declare //////////////////////////////////////////////////////////
var keys = ['ScreenNumber', 'TabID', 'GroupID'];
var fieldsCheckRequire = ["ScreenNumber", "TabID", "GroupID"];
var fieldsLangCheckRequire = ["ScreenNumber", "TabID", "GroupID"];

var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;

var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_RibbonScreen.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboScreenNumber.getStore().addListener('load', checkLoad);
    App.cboTabID.getStore().addListener('load', checkLoad);
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_RibbonScreen);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_RibbonScreen);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_RibbonScreen);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_RibbonScreen);
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
                HQ.grid.insert(App.grdSYS_RibbonScreen, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSYS_RibbonScreen.selected.items[0] != undefined) {
                    if (App.slmSYS_RibbonScreen.selected.items[0].data.ScreenNumber != ""
                        && App.slmSYS_RibbonScreen.selected.items[0].data.TabID != ""
                        && App.slmSYS_RibbonScreen.selected.items[0].data.GroupID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_RibbonScreen)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_RibbonScreen, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    HQ.common.changeData(HQ.isChange, 'SA02100');
};

var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var grdSYS_RibbonScreen_BeforeEdit = function (editor, e) {
    if (e.field == 'GroupID') {
        App.cboGroupID.store.reload();
    }
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSYS_RibbonScreen_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_RibbonScreen, e, keys);
};

var grdSYS_RibbonScreen_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_RibbonScreen, e, keys);
};

var grdSYS_RibbonScreen_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_RibbonScreen);
    stoChanged(App.stoSYS_RibbonScreen);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA02100/Save',
            params: {
                lstSYS_RibbonScreen: HQ.store.getData(App.stoSYS_RibbonScreen)
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
        App.grdSYS_RibbonScreen.deleteSelected();
        stoChanged(App.stoSYS_RibbonScreen);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_RibbonScreen.reload();
    }
};
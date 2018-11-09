//// Declare //////////////////////////////////////////////////////////
var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Descr"];
var fieldsLangCheckRequire = ["Code", "Descr"];

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
        App.stoOM_ReasonCode.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    checkLoad();
    App.chkReasonable.setVisible(HQ.Reasonable);
    App.chkReasonIsShow.setVisible(HQ.ReasonIsShow);
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_ReasonCode);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_ReasonCode);
            break;
        case "next":
            HQ.grid.next(App.grdOM_ReasonCode);
            break;
        case "last":
            HQ.grid.last(App.grdOM_ReasonCode);
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
                HQ.grid.insert(App.grdOM_ReasonCode, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmOM_ReasonCode.selected.items[0] != undefined) {
                    if (App.slmOM_ReasonCode.selected.items[0].data.Code != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_ReasonCode)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_ReasonCode, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    HQ.common.changeData(HQ.isChange, 'OM22500');
};

var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22500');
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
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var grdOM_ReasonCode_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdOM_ReasonCode_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_ReasonCode, e, keys);
};

var grdOM_ReasonCode_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_ReasonCode, e, keys, true);
};

var grdOM_ReasonCode_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_ReasonCode);
    stoChanged(App.stoOM_ReasonCode);
};


/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            timeout: 180000,
            url: 'OM22500/Save',
            params: {
                lstOM_ReasonCode: HQ.store.getData(App.stoOM_ReasonCode)
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
        App.grdOM_ReasonCode.deleteSelected();
        stoChanged(App.stoOM_ReasonCode);
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_ReasonCode.reload();
    }
};
///////////////////////////////////
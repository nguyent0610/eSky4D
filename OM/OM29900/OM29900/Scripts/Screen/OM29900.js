//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var keys = ['Code'];
var fieldsCheckRequire = ['Code','Descr'];
var fieldsLangCheckRequire = ['Code','Descr'];
var _status = '';
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
//load lần đầu khi mở
var firstLoad = function () {
    HQ.util.checkAccessRight();
    App.frmMain.isValid();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoTypeOfVehicle, keys, ['', '']);
        if (!record) {
            HQ.store.insertBlank(App.stoTypeOfVehicle, keys);
        }
    }
    App.stoTypeOfVehicle.reload();
    checkLoad();
};
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoTypeOfVehicle);
    HQ.common.changeData(HQ.isChange, 'OM29900');
};
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdTypeOfVehicle);
            break;
        case "prev":
            HQ.grid.prev(App.grdTypeOfVehicle);
            break;
        case "next":
            HQ.grid.next(App.grdTypeOfVehicle);
            break;
        case "last":
            HQ.grid.last(App.grdTypeOfVehicle);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    HQ.isFirstLoad = true;
                    App.stoTypeOfVehicle.reload();
                }
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdTypeOfVehicle, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmTypeOfVehicle.selected.items[0] != undefined) {
                    if (App.slmTypeOfVehicle.selected.items[0].data.Market != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdTypeOfVehicle)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoTypeOfVehicle, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                    ) {
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
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
var stoLoadTypeOfVehicle = function (sto) {
    HQ.isFirstLoad = false;
    HQ.common.showBusy(false);
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoTypeOfVehicle, keys, ['', '']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }
    }
    frmChange();
};
var grdTypeOfVehicle_BeforeEdit = function (item, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys))
        return false;
};
var grdTypeOfVehicle_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdTypeOfVehicle, e, keys);
    frmChange();
};
var grdTypeOfVehicle_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTypeOfVehicle, e, keys, true);
};
var grdTypeOfVehicle_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTypeOfVehicle);
    frmChange();
};
var save = function () {
    //App.frmMain.updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM29900/Save',
            params: {
                lstTypeOfVehicle: HQ.store.getData(App.stoTypeOfVehicle),

            },
            success: function (msg, data) {
                HQ.isFirstLoad = true;
                HQ.message.show(201405071);
                App.stoTypeOfVehicle.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        App.grdTypeOfVehicle.deleteSelected();
        frmChange();
    }
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoTypeOfVehicle.reload();
    }
};

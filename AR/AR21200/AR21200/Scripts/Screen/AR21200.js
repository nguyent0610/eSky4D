//// Declare //////////////////////////////////////////////////////////
var fieldsCheckRequire = ["Location", "Descr"];
var keys = ['Location'];
var fieldsLangCheckRequire = ["Code", "Descr"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;

///////////////////////////////////////////////////////////////////////
// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoLocation.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdLocation);
            break;
        case "prev":
            HQ.grid.prev(App.grdLocation);
            break;
        case "next":
            HQ.grid.next(App.grdLocation);
            break;
        case "last":
            HQ.grid.last(App.grdLocation);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoChannel.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdLocation);
            }
            break;
        case "delete":
            if (App.slmLocation.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmLocation.selected.items[0] != undefined) {
                        if (App.slmLocation.selected.items[0].data.Location != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdLocation)], 'deleteData',true);
                        }
                    }
                }
            }
            break;
          
        case "save":
           
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoLocation, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad(); // Mới
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoLocation);
    HQ.common.changeData(HQ.isChange, 'AR21200');//co thay doi du lieu gan * tren tab title header
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoLocation.reload();
    }
};

var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdLocation_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdLocation_Edit = function (item, e) {

    HQ.grid.checkInsertKey(App.grdLocation, e, keys);
    frmChange();
};

var grdLocation_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdLocation, e, keys);
};

var grd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdLocation);
    frmChange();
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR21200/Save',
            params: {
                lstLocation: HQ.store.getData(App.stoLocation)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdLocation.deleteSelected();
        frmChange();
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
/////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////









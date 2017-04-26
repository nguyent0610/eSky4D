//// Declare //////////////////////////////////////////////////////////
var keys = ['CodeID'];
var fieldsCheckRequire = ["CodeID", "Descr"];
var fieldsLangCheckRequire = ["AR23800CodeID", "AR23800Descr"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoCodeID.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdCodeID);
            break;
        case "prev":
            HQ.grid.prev(App.grdCodeID);
            break;
        case "next":
            HQ.grid.next(App.grdCodeID);
            break;
        case "last":
            HQ.grid.last(App.grdCodeID);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoCodeID.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdCodeID, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmCodeID.selected.items[0] != undefined) {
                    if (App.slmCodeID.selected.items[0].data.RoleID != "") {
                        var values = HQ.grid.indexSelect(App.grdCodeID).split(',');
                        for (var i = 0; i < values.length; i++) {
                            if (App.grdCodeID.store.data.items[values[i] - 1].data.IsDelete != "") {
                                HQ.message.show(18);
                                return false;
                            }
                        }
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdCodeID)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoCodeID, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoCodeID);
    HQ.common.changeData(HQ.isChange, 'AR23800');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoCodeID_Load = function (sto) {
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

var grdCodeID_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdCodeID_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdCodeID, e, keys);
    frmChange();
};

var grdCodeID_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdCodeID, e, keys);
};

var grdCodeID_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCodeID);
    frmChange();
};

//////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
////Process Data
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR23800/Save',
            params: {
                lstCodeID: HQ.store.getData(App.stoCodeID)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoCodeID.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdCodeID.deleteSelected();
        frmChange();
    }
};

/////////////////////////////////////////
////////////////////////////////////////
//Other Function

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoCodeID.reload();
    }
};
var keys = ['ModuleCode', 'ModuleID'];
var fieldsCheckRequire = ["ModuleCode", "ModuleID", "ModuleName", "CatID"];
var fieldsLangCheckRequire = ["ModuleCode", "ModuleID", "ModuleName", "CatID"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_Module.reload();
        HQ.common.showBusy(false);
    }
};
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Module);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Module);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Module);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Module);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_Module.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Module, keys);
            }
            break;
        case "delete":
            if (App.slmData.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmData.selected.items[0] != undefined) {
                        if (App.slmData.selected.items[0].data.ModuleCode != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_Module)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Module, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var grdSYS_Module_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_Module_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Module, e, keys);
};
var grdSYS_Module_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Module, e, keys);
};
var grdSYS_Module_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Module);
    frmChange();
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA00200/Save',
            params: {
                lstData: HQ.store.getData(App.stoSYS_Module)
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
        App.grdSYS_Module.deleteSelected();
        frmChange();
    }
};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad(); // Mới
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSYS_Module);
    HQ.common.changeData(HQ.isChange, 'SA00200');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Module.reload();
    }
};


//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
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

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Module.reload();
    }
};
///////////////////////////////////
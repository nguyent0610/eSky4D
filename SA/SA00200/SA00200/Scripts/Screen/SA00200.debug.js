var keys = ['ModuleCode', 'ModuleID'];
var fieldsCheckRequire = ["ModuleCode", "ModuleID", "ModuleName", "CatID"];
var fieldsLangCheckRequire = ["ModuleCode", "ModuleID", "ModuleName", "CatID"];

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
            HQ.isFirstLoad = true;
            App.stoSYS_Module.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Module, keys);
            }
            break;
        case "delete":
            if (App.slmData.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
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
            if (HQ.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
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
    stoChanged(App.stoSYS_Module);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA00200/Save',
            params: {
                lstSYS_Language: HQ.store.getData(App.stoSYS_Module)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                menuClick("refresh");
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
        stoChanged(App.stoSYS_Module);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, 'SA00200');//khi dong roi gan lai cho change la false
        HQ.common.close(this);
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_Module.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA00200');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA00200');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};


var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber", "Descr", "ModuleID", "ScreenType", "CatID"];
var fieldsLangCheckRequire = ["ScreenNumber", "Descr", "ModuleID", "ScreenType", "CatID"];
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Screen);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Screen);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Screen);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Screen);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_Screen.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Screen, keys);
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
                if (HQ.store.checkRequirePass(App.stoSYS_Screen, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var grdSYS_Screen_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_Screen_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Screen, e, keys);
};
var grdSYS_Screen_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Screen, e, keys);
};
var grdSYS_Screen_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Screen);
    stoChanged(App.stoSYS_Screen);
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'IN11700/Save',
            params: {
                lstData: HQ.store.getData(App.stoSYS_Screen)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
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
        App.grdSYS_Screen.deleteSelected();
        stoChanged(App.stoSYS_Screen);
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_Screen.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN11700');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN11700');
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


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Screen.reload();
    }
};
///////////////////////////////////

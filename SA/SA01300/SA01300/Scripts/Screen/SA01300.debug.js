//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
var fieldsCheckRequire = [];
var fieldsLangCheckRequire = [];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

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
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_Configurations.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Configurations, keys);
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
                if (HQ.store.checkRequirePass(App.stoSYS_Configurations, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
            url: 'SA01300/Save',
            params: {
                lstSYS_Configurations: HQ.store.getData(App.stoSYS_Configurations)
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
        App.grdSYS_Configurations.deleteSelected();
        stoChanged(App.stoSYS_Configurations);
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_Configurations.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA01300');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
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
        App.stoSYS_Configurations.reload();
    }
};
///////////////////////////////////

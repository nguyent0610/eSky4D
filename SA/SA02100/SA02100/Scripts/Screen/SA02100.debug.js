//// Declare //////////////////////////////////////////////////////////
var keys = ['ScreenNumber', 'TabID', 'GroupID'];
var fieldsCheckRequire = ["ScreenNumber", "TabID", "GroupID"];
var fieldsLangCheckRequire = ["ScreenNumber", "TabID", "GroupID"];

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
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_RibbonScreen.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_RibbonScreen, keys);
            }
            break;
        case "delete":
            if (App.slmSYS_RibbonScreen.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdSYS_RibbonScreen);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSYS_RibbonScreen), ''], 'deleteData', true)
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
            HQ.common.close(this);
            break;
    }

};

var cboTabID_Change = function () {
    //App.slmSYS_RibbonScreen.selected.items[0].set('GroupID', '');
    App.cboGroupID.store.reload();
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_RibbonScreen.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02100');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02100');
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

var grdSYS_RibbonScreen_BeforeEdit = function (editor, e) {
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
///////////////////////////////////
//// Declare //////////////////////////////////////////////////////////

var keys = ['ScreenNumber', 'TabID', 'GroupID'];
var fieldsCheckRequire = ["ScreenNumber", "TabID", "GroupID"];
var fieldsLangCheckRequire = ["ScreenNumber", "TabID", "GroupID"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

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
            App.stoSYS_RibbonScreen.reload();
            HQ.grid.first(App.grdSYS_RibbonScreen);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_RibbonScreen);
            }
            break;
        case "delete":
            if (App.slmSYS_RibbonScreen.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
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
            if (HQ.store.isChange(App.stoSYS_RibbonScreen)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var cboTabID_Change = function () {
    App.slmSYS_RibbonScreen.selected.items[0].set('GroupID', '');
};
var grdSYS_RibbonScreen_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
    //nếu cell đang click vào là State thì reload store của cboState
    if (e.field == "GroupID") {
        App.cboGroupID.getStore().reload();
    }
};
var grdSYS_RibbonScreen_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_RibbonScreen, e, keys);
};
var grdSYS_RibbonScreen_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_RibbonScreen, e, keys);
};
var grdSYS_RibbonScreen_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_RibbonScreen);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA02100/Save',
            params: {
                lstSYS_RibbonScreen: HQ.store.getData(App.stoSYS_RibbonScreen)
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
        App.grdSYS_RibbonScreen.deleteSelected();
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
/////////////////////////////////////////////////////////////////////////
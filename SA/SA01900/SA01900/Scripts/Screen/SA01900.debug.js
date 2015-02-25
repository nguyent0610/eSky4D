//// Declare //////////////////////////////////////////////////////////

var keys = ['TabID'];
var fieldsCheckRequire = ["TabID", "Descr"];
var fieldsLangCheckRequire = ["TabID", "Descr"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_RibbonTab);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_RibbonTab);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_RibbonTab);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_RibbonTab);
            break;
        case "refresh":
            App.stoSYS_RibbonTab.reload();
            HQ.grid.first(App.grdSYS_RibbonTab);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_RibbonTab);
            }
            break;
        case "delete":
            if (App.slmSYS_RibbonTab.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_RibbonTab, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_RibbonTab)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_RibbonTab_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_RibbonTab_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_RibbonTab, e, keys);
};
var grdSYS_RibbonTab_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_RibbonTab, e, keys);
};
var grdSYS_RibbonTab_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_RibbonTab);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA01900/Save',
            params: {
                lstSYS_RibbonTab: HQ.store.getData(App.stoSYS_RibbonTab)
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
        App.grdSYS_RibbonTab.deleteSelected();
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
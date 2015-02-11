//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Lang00", "Lang01"];
var fieldsLangCheckRequire = ["Code", "Lang00", "Lang01"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Language);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Language);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Language);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Language);
            break;
        case "refresh":
            App.stoSYS_Language.reload();
            HQ.grid.first(App.grdSYS_Language);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Language);
            }
            break;
        case "delete":
            if (App.slmSYS_Language.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Language, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_Language)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_Language_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_Language_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Language, e, keys);
};
var grdSYS_Language_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Language, e, keys);
};
var grdSYS_Language_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Language);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA00900/Save',
            params: {
                lstSYS_Language: HQ.store.getData(App.stoSYS_Language)
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
        App.grdSYS_Language.deleteSelected();
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









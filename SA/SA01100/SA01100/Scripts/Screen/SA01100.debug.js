//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
var fieldsCheckRequire = ["Title00", "Title01","Msg00", "Msg01"];
var fieldsLangCheckRequire = ["Title00", "Title01","Msg00", "Msg01"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Message);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Message);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Message);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Message);
            break;
        case "refresh":
            App.stoSYS_Message.reload();
            HQ.grid.first(App.grdSYS_Message);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Message);
            }
            break;
        case "delete":
            if (App.slmSYS_Message.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Message, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_Message)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_Message_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_Message_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Message, e, keys);
};
var grdSYS_Message_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Message, e, keys);
};
var grdSYS_Message_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Message);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA01100/Save',
            params: {
                lstSYS_Message: HQ.store.getData(App.stoSYS_Message)
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
        App.grdSYS_Message.deleteSelected();
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
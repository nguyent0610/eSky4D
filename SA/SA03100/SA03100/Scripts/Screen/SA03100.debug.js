//// Declare //////////////////////////////////////////////////////////

var keys = ['GroupID'];
var fieldsCheckRequire = ["GroupID", "Descr", "ListCpny"];
var fieldsLangCheckRequire = ["GroupID", "Descr", "ListCpny"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_CompanyGroup);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_CompanyGroup);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_CompanyGroup);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_CompanyGroup);
            break;
        case "refresh":
            App.stoSYS_CompanyGroup.reload();
            HQ.grid.first(App.grdSYS_CompanyGroup);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_CompanyGroup);
            }
            break;
        case "delete":
            if (App.slmSYS_CompanyGroup.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_CompanyGroup, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_CompanyGroup)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_CompanyGroup_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_CompanyGroup_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_CompanyGroup, e, keys);
};
var grdSYS_CompanyGroup_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_CompanyGroup, e, keys);
};
var grdSYS_CompanyGroup_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_CompanyGroup);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA03100/Save',
            params: {
                lstSYS_CompanyGroup: HQ.store.getData(App.stoSYS_CompanyGroup)
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
        App.grdSYS_CompanyGroup.deleteSelected();
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
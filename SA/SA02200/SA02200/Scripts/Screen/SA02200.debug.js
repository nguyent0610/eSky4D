//// Declare //////////////////////////////////////////////////////////

var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber"];
var fieldsLangCheckRequire = ["ScreenNumber"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Favourite);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Favourite);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Favourite);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Favourite);
            break;
        case "refresh":
            App.stoSYS_Favourite.reload();
            HQ.grid.first(App.grdSYS_Favourite);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Favourite);
            }
            break;
        case "delete":
            if (App.slmSYS_Favourite.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Favourite, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_Favourite)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};

var cboScreenNumber_Change = function (value) {
    var k = value.displayTplData[0].Descr;
    App.slmSYS_Favourite.selected.items[0].set('Descr',k);
};

var grdSYS_Favourite_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_Favourite_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Favourite, e, keys);
};
var grdSYS_Favourite_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Favourite, e, keys);
};
var grdSYS_Favourite_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Favourite);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA02200/Save',
            params: {
                lstSYS_Favourite: HQ.store.getData(App.stoSYS_Favourite)
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
        App.grdSYS_Favourite.deleteSelected();
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









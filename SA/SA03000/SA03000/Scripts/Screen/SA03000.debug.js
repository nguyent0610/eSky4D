//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Descr"];
var fieldsLangCheckRequire = ["Code", "Descr"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_FavouriteGroup);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_FavouriteGroup);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_FavouriteGroup);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_FavouriteGroup);
            break;
        case "refresh":
            App.stoSYS_FavouriteGroup.reload();
            HQ.grid.first(App.grdSYS_FavouriteGroup);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_FavouriteGroup);
            }
            break;
        case "delete":
            if (App.slmSYS_FavouriteGroup.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_FavouriteGroup, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_FavouriteGroup)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_FavouriteGroup_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_FavouriteGroup_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_FavouriteGroup, e, keys);
};
var grdSYS_FavouriteGroup_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_FavouriteGroup, e, keys);
};
var grdSYS_FavouriteGroup_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_FavouriteGroup);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA03000/Save',
            params: {
                lstSYS_FavouriteGroup: HQ.store.getData(App.stoSYS_FavouriteGroup)
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
        App.grdSYS_FavouriteGroup.deleteSelected();
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









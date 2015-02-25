
var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber"];
var fieldsLangCheckRequire = ["ScreenNumber"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_FavouriteGroupUser);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_FavouriteGroupUser);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_FavouriteGroupUser);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_FavouriteGroupUser);
            break;
        case "refresh":
            App.stoSYS_FavouriteGroupUser.reload();
            HQ.grid.first(App.grdSYS_FavouriteGroupUser);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_FavouriteGroupUser);
            }
            break;
        case "delete":
            if (App.slmSYS_FavouriteGroupUser.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_FavouriteGroupUser, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_FavouriteGroupUser)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};

var tabSA02210_AfterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 100);
    }
};

var cboUserGroupID_Change = function (item, newValue, oldValue) {
    App.grdSYS_FavouriteGroupUser.store.reload();
};

var cboScreenNumber_Change = function (value) {
    var k = value.displayTplData[0].Descr;
    App.slmSYS_FavouriteGroupUser.selected.items[0].set('Descr', k);
};
var grdSYS_FavouriteGroupUser_BeforeEdit = function (editor, e) {
     if (!HQ.grid.checkBeforeEdit(e, keys)) return false;  
};

var grdSYS_FavouriteGroupUser_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_FavouriteGroupUser, e, keys);



};

var grdSYS_FavouriteGroupUser_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSYS_FavouriteGroupUser, e, keys);
};

var grdSYS_FavouriteGroupUser_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSYS_FavouriteGroupUser);
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA02210/Save',
            params: {
                lstSYS_FavouriteGroupUser: HQ.store.getData(App.stoSYS_FavouriteGroupUser)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.cboUserGroupID.getStore().reload();
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
        App.grdSYS_FavouriteGroupUser.deleteSelected();
    }
};
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
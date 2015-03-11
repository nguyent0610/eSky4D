
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
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_FavouriteGroupUser.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_FavouriteGroupUser, keys);
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
            HQ.common.close(this);
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
    HQ.isFirstLoad = true;
    App.grdSYS_FavouriteGroupUser.store.reload();
};

var cboScreenNumber_Change = function (value) {
};
var grdSYS_FavouriteGroupUser_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSYS_FavouriteGroupUser_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_FavouriteGroupUser, e, keys);
    if (e.field == "ScreenNumber") {
        var selectedRecord = App.cboScreenNumber.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("Descr", "");
        }
    }
};

var grdSYS_FavouriteGroupUser_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSYS_FavouriteGroupUser, e, keys);
};

var grdSYS_FavouriteGroupUser_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSYS_FavouriteGroupUser);
    stoChanged(App.stoSYS_FavouriteGroupUser);
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
                HQ.isChange = false;
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
        stoChanged(App.stoSYS_FavouriteGroupUser);
    }
};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_FavouriteGroupUser.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02210');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02210');
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
        App.stoSYS_FavouriteGroupUser.reload();
    }
};
///////////////////////////////////
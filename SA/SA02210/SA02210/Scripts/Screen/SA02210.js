var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber", "CodeGroup"];
var fieldsLangCheckRequire = ["ScreenNumber", "CodeGroup"];

var _Source = 0;
var _maxSource = 3;
var _isLoadMaster = false;

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboUserGroupID.getStore().addListener('load', checkLoad);
    App.cboScreenNumber.getStore().addListener('load', checkLoad);
    App.cboCodeGroup.getStore().addListener('load', checkLoad);
};

var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_FavouriteGroupUser.reload();
        HQ.common.showBusy(false);
    }
};

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

var cboUserGroupID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if(sender.valueModels != null && !App.stoSYS_FavouriteGroupUser.loading)
        App.grdSYS_FavouriteGroupUser.store.reload();
};

var cboUserGroupID_Select = function (sender,value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSYS_FavouriteGroupUser.loading)
        App.grdSYS_FavouriteGroupUser.store.reload();
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02210');
    App.cboUserGroupID.setReadOnly(HQ.isChange);
};

var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02210');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    stoChanged(App.stoSYS_FavouriteGroupUser);
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};
var grdSYS_FavouriteGroupUser_BeforeEdit = function (editor, e) {
    if (HQ.form.checkRequirePass(App.frmMain))
        return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSYS_FavouriteGroupUser_Edit = function (item, e) {
    
    if (e.field == "ScreenNumber") {
        var selectedRecord = App.cboScreenNumber.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("Descr", "");
        }
    }
    HQ.grid.checkInsertKey(App.grdSYS_FavouriteGroupUser, e, keys);
};

var grdSYS_FavouriteGroupUser_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_FavouriteGroupUser, e, keys, true);
};

var grdSYS_FavouriteGroupUser_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_FavouriteGroupUser);
    stoChanged(App.stoSYS_FavouriteGroupUser);
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            timeout: 1000000,
            url: 'SA02210/Save',
            params: {
                lstSYS_FavouriteGroupUser: HQ.store.getData(App.stoSYS_FavouriteGroupUser)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.cboUserGroupID.getStore().reload();
                refresh('yes');
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
//// Declare //////////////////////////////////////////////////////////
var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber", "CodeGroup"];
var fieldsLangCheckRequire = ["ScreenNumber", "CodeGroup"];

var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_Favourite.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboScreenNumber.getStore().addListener('load', checkLoad);
    App.cboCodeGroup.getStore().addListener('load', checkLoad);
};

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
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Favourite, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSYS_Favourite.selected.items[0] != undefined) {
                    if (App.slmSYS_Favourite.selected.items[0].data.ScreenNumber != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_Favourite)], 'deleteData', true);
                    }
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
            break;
    }

};

/////////////////////////////////////////////////////////////////////////


//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02200');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02200');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var grdSYS_Favourite_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSYS_Favourite_Edit = function (item, e) {
    if (e.field == "ScreenNumber") {
        if (e.value) {
            var objScreen = App.cboScreenNumberSA02200_pcSYS_Screen.findRecord(['ScreenNumber'], [e.value]);
            if (objScreen)
                e.record.set('Descr', objScreen.data.Descr);
        }
    }
    HQ.grid.checkInsertKey(App.grdSYS_Favourite, e, keys);
};

var grdSYS_Favourite_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Favourite, e, keys, true);
};

var grdSYS_Favourite_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Favourite);
    stoChanged(App.stoSYS_Favourite);
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
        App.grdSYS_Favourite.deleteSelected();
        stoChanged(App.stoSYS_Favourite);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Favourite.reload();
    }
};
///////////////////////////////////
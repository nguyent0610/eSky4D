//// Declare //////////////////////////////////////////////////////////

var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber", "CodeGroup"];
var fieldsLangCheckRequire = ["ScreenNumber", "CodeGroup"];
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
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_Favourite.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Favourite, keys);
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
            HQ.common.close(this);
            break;
    }

};

/////////////////////////////////////////////////////////////////////////
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_Favourite.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02200');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02200');
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

var cboScreenNumber_Change = function (value) {
};

var grdSYS_Favourite_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSYS_Favourite_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Favourite, e, keys);

    if (e.field == "ScreenNumber") {
        var selectedRecord = App.cboScreenNumber.store.findRecord(e.field, e.value);
        if (selectedRecord != "") {
            if (selectedRecord) {
                e.record.set("Descr", selectedRecord.data.Descr);
            }
            else {
                e.record.set("Descr", "");
            }
        }
    }
};

var grdSYS_Favourite_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Favourite, e, keys);
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
                HQ.isChange = false;
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
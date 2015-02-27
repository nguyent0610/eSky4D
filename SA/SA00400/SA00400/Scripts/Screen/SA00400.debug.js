//// Declare //////////////////////////////////////////////////////////

var keys = ['CatID'];
var fieldsCheckRequire = ["CatID", "Descr"];
var fieldsLangCheckRequire = ["CatID", "Descr"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_ScreenCat);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_ScreenCat);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_ScreenCat);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_ScreenCat);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoSYS_ScreenCat.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_ScreenCat, keys);
            }
            break;
        case "delete":
            if (App.slmData.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_ScreenCat, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_ScreenCat_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_ScreenCat_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_ScreenCat, e, keys);
};
var grdSYS_ScreenCat_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_ScreenCat, e, keys);
};
var grdSYS_ScreenCat_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_ScreenCat);
    stoChanged(App.stoSYS_ScreenCat);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA00400/Save',
            params: {
                lstData: HQ.store.getData(App.stoSYS_ScreenCat)
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
        App.grdSYS_ScreenCat.deleteSelected();
        stoChanged(App.stoSYS_ScreenCat);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, 'SA00400');//khi dong roi gan lai cho change la false
        HQ.common.close(this);
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_ScreenCat.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA00400');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA00400');
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

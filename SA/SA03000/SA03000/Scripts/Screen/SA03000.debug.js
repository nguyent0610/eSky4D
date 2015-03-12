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
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_FavouriteGroup.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_FavouriteGroup, keys);
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
            HQ.common.close(this);
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
    stoChanged(App.stoSYS_FavouriteGroup);
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
        App.grdSYS_FavouriteGroup.deleteSelected();
        stoChanged(App.stoSYS_FavouriteGroup);
    }
};

/////////////////////////////////////////////////////////////////////////
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_FavouriteGroup.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03000');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03000');
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
        App.stoSYS_FavouriteGroup.reload();
    }
};
///////////////////////////////////
//// Declare //////////////////////////////////////////////////////////
var keys = ['UserID','DfltBranchID'];
var fieldsCheckRequire = ["UserID", "DfltBranchID"];
var fieldsLangCheckRequire = ["UserID", "DfltBranchID"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdUserDefault);
            break;
        case "prev":
            HQ.grid.prev(App.grdUserDefault);
            break;
        case "next":
            HQ.grid.next(App.grdUserDefault);
            break;
        case "last":
            HQ.grid.last(App.grdUserDefault);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoUserDefault.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdUserDefault, keys);
            }
            break;
        case "delete":
            if (App.slmUserDefault.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdUserDefault);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdUserDefault), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoUserDefault, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoUserDefault.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21200');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21200');
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

var grdUserDefault_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdUserDefault_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdUserDefault, e, keys);
    stoChanged(App.stoUserDefault);
};

var grdUserDefault_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdUserDefault, e, keys);
};

var grdUserDefault_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdUserDefault);
    stoChanged(App.stoUserDefault);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21200/Save',
            params: {
                lstUserDefault: HQ.store.getData(App.stoUserDefault)
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
        App.grdUserDefault.deleteSelected();
        stoChanged(App.stoUserDefault);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoUserDefault.reload();
    }
};
///////////////////////////////////
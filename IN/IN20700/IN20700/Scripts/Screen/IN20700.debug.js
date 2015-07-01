//// Declare //////////////////////////////////////////////////////////

var keys = ['ReasonCD'];
var fieldsCheckRequire = ["ReasonCD"];
var fieldsLangCheckRequire = ["ReasonCD"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdReasonCode);
            break;
        case "prev":
            HQ.grid.prev(App.grdReasonCode);
            break;
        case "next":
            HQ.grid.next(App.grdReasonCode);
            break;
        case "last":
            HQ.grid.last(App.grdReasonCode);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoReasonCode.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdReasonCode, keys);
            }
            break;
        case "delete":
            if (App.slmReasonCode.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdReasonCode);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdReasonCode), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoReasonCode, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoReasonCode.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20700');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20700');
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

var grdReasonCode_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdReasonCode_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdReasonCode, e, keys);
};

var grdReasonCode_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdReasonCode, e, keys);
};

var grdReasonCode_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdReasonCode);
    stoChanged(App.stoReasonCode);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IN20700/Save',
            params: {
                lstReasonCode: HQ.store.getData(App.stoReasonCode)
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
        App.grdReasonCode.deleteSelected();
        stoChanged(App.stoReasonCode);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoReasonCode.reload();
    }
};
///////////////////////////////////
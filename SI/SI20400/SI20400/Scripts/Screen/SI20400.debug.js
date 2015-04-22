//// Declare //////////////////////////////////////////////////////////
var keys = ['MaterialType'];
var fieldsCheckRequire = ["MaterialType"];
var fieldsLangCheckRequire = ["MaterialType"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_MaterialType);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_MaterialType);
            break;
        case "next":
            HQ.grid.next(App.grdSI_MaterialType);
            break;
        case "last":
            HQ.grid.last(App.grdSI_MaterialType);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSI_MaterialType.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_MaterialType, keys);
            }
            break;
        case "delete":
            if (App.slmSI_MaterialType.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdSI_MaterialType);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSI_MaterialType), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_MaterialType, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoSI_MaterialType.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20400');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20400');
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

var grdSI_MaterialType_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSI_MaterialType_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSI_MaterialType, e, keys);
};

var grdSI_MaterialType_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_MaterialType, e, keys);
};

var grdSI_MaterialType_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_MaterialType);
    stoChanged(App.stoSI_MaterialType);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI20400/Save',
            params: {
                lstSI_MaterialType: HQ.store.getData(App.stoSI_MaterialType)
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
        App.grdSI_MaterialType.deleteSelected();
        stoChanged(App.stoSI_MaterialType);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_MaterialType.reload();
    }
};
///////////////////////////////////
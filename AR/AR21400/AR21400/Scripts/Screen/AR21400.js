//// Declare //////////////////////////////////////////////////////////
var keys = ['Code'];
var fieldsCheckRequire = ["Code"];
var fieldsLangCheckRequire = ["Code"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSellingProducts);
            break;
        case "prev":
            HQ.grid.prev(App.grdSellingProducts);
            break;
        case "next":
            HQ.grid.next(App.grdSellingProducts);
            break;
        case "last":
            HQ.grid.last(App.grdSellingProducts);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSellingProducts.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSellingProducts, keys);
            }
            break;
        case "delete":
            if (App.slmSellingProducts.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdSellingProducts);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSellingProducts), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSellingProducts, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoSellingProducts.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR21400');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR21400');
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

var grdSellingProducts_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSellingProducts_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSellingProducts, e, keys);
};

var grdSellingProducts_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSellingProducts, e, keys);
};

var grdSellingProducts_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSellingProducts);
    stoChanged(App.stoSellingProducts);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'AR21400/Save',
            params: {
                lstAR_SellingProducts: HQ.store.getData(App.stoSellingProducts)
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
        App.grdSellingProducts.deleteSelected();
        stoChanged(App.stoSellingProducts);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSellingProducts.reload();
    }
};
///////////////////////////////////
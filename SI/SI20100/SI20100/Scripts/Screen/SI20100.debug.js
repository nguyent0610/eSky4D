//// Declare //////////////////////////////////////////////////////////
var keys = ['Buyer'];
var fieldsCheckRequire = ["Buyer"];
var fieldsLangCheckRequire = ["Buyer"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdIN_Buyer);
            break;
        case "prev":
            HQ.grid.prev(App.grdIN_Buyer);
            break;
        case "next":
            HQ.grid.next(App.grdIN_Buyer);
            break;
        case "last":
            HQ.grid.last(App.grdIN_Buyer);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoIN_Buyer.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdIN_Buyer, keys);
            }
            break;
        case "delete":
            if (App.slmIN_Buyer.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdIN_Buyer);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdIN_Buyer), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoIN_Buyer, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoIN_Buyer.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20100');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20100');
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

var grdIN_Buyer_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdIN_Buyer_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdIN_Buyer, e, keys);
};

var grdIN_Buyer_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdIN_Buyer, e, keys);
};

var grdIN_Buyer_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN_Buyer);
    stoChanged(App.stoIN_Buyer);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI20100/Save',
            params: {
                lstIN_Buyer: HQ.store.getData(App.stoIN_Buyer)
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
        App.grdIN_Buyer.deleteSelected();
        stoChanged(App.stoIN_Buyer);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoIN_Buyer.reload();
    }
};
///////////////////////////////////
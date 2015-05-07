//// Declare //////////////////////////////////////////////////////////
var keys = ['SalesRouteID'];
var fieldsCheckRequire = ["SalesRouteID"];
var fieldsLangCheckRequire = ["SalesRouteID"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSalesRoute);
            break;
        case "prev":
            HQ.grid.prev(App.grdSalesRoute);
            break;
        case "next":
            HQ.grid.next(App.grdSalesRoute);
            break;
        case "last":
            HQ.grid.last(App.grdSalesRoute);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSalesRoute.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSalesRoute, keys);
            }
            break;
        case "delete":
            if (App.slmSalesRoute.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdSalesRoute);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSalesRoute), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSalesRoute, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var cboCpnyID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoSalesRoute.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboCpnyID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboCpnyID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboCpnyID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboCpnyID.setValue('');
    }

};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSalesRoute.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21600');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21600');
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

var grdSalesRoute_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSalesRoute_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSalesRoute, e, keys);
};

var grdSalesRoute_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSalesRoute, e, keys);
};

var grdSalesRoute_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSalesRoute);
    stoChanged(App.stoSalesRoute);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21600/Save',
            params: {
                lstOM_SalesRoute: HQ.store.getData(App.stoSalesRoute)
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
        App.grdSalesRoute.deleteSelected();
        stoChanged(App.stoSalesRoute);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSalesRoute.reload();
    }
};
///////////////////////////////////









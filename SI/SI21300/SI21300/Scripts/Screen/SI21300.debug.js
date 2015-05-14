//// Declare //////////////////////////////////////////////////////////
var keys = ['CarrierID'];
var fieldsCheckRequire = ["CarrierID"];
var fieldsLangCheckRequire = ["CarrierID"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_Carrier);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_Carrier);
            break;
        case "next":
            HQ.grid.next(App.grdSI_Carrier);
            break;
        case "last":
            HQ.grid.last(App.grdSI_Carrier);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSI_Carrier.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_Carrier, keys);
            }
            break;
        case "delete":
            if (App.slmSI_Carrier.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdSI_Carrier);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSI_Carrier), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_Carrier, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoSI_Carrier.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI21300');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI21300');
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

var grdSI_Carrier_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSI_Carrier_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSI_Carrier, e, keys);
};

var grdSI_Carrier_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_Carrier, e, keys);
};

var grdSI_Carrier_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_Carrier);
    stoChanged(App.stoSI_Carrier);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI21300/Save',
            params: {
                lstSI_Carrier: HQ.store.getData(App.stoSI_Carrier)
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
        App.grdSI_Carrier.deleteSelected();
        stoChanged(App.stoSI_Carrier);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_Carrier.reload();
    }
};
///////////////////////////////////

var CheckZonesCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdSI_Carrier.getStore().each(function (item) {
            item.set("CheckZones", value.checked);
        });
    }
};

/////////////////////////////////////////////////////////////////////////

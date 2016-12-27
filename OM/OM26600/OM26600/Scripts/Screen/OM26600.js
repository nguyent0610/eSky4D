//// Declare //////////////////////////////////////////////////////////
var keys = ['BranchID', 'Code'];
var fieldsCheckRequire = ["BranchID", "Code", "Descr"];
var fieldsLangCheckRequire = ["BranchID", "OM26600Code", "Descr"];

var _Source = 0;
var _maxSource = 1;

var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true
        _Source = 0;
        App.stoOM_Truck.reload();
    }
};

///////////Store/////////////////
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoOM_Truck_changed = function (sto) {
    HQ.isChange = HQ.store.isChange(App.stoOM_Truck);
    HQ.common.changeData(HQ.isChange, 'OM26600');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoOM_Truck_load = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(App.stoOM_Truck);
    HQ.common.changeData(HQ.isChange, 'OM26600');
    var obj = HQ.store.findInStore(sto, ['BranchID', 'Code', 'Descr'], ['', '', '']);
    if (!obj) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
    }
    HQ.isFirstLoad = false;
};
//trước khi load trang busy la dang load data
var stoOM_Truck_beforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
/////////////////////////////////
////////////Event/////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_Truck);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_Truck);
            break;
        case "next":
            HQ.grid.next(App.grdOM_Truck);
            break;
        case "last":
            HQ.grid.last(App.grdOM_Truck);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
               
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_Truck, keys);
            }
            break;
        case "delete":
            if (App.slmData.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdOM_Truck);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_Truck), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_Truck, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    App.cboBranchID.getStore().addListener('load', checkLoad);
    HQ.isFirstLoad = true;
    HQ.util.checkAccessRight();
};
var grdOM_Truck_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_Truck_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_Truck, e, keys);
};
var grdOM_Truck_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_Truck, e, keys);
};
var grdOM_Truck_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_Truck);
    HQ.isChange = HQ.store.isChange(App.stoOM_Truck);
};
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM26600/Save',
            params: {
                lstData: HQ.store.getData(App.stoOM_Truck)
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_Truck.deleteSelected();
        stoOM_Truck_changed(App.stoOM_Truck);
      
    }
};
var refresh = function (item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_Truck.reload();
    }
}
var renderBranchName = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboBranchID.store, ['BranchID'], [record.data.BranchID])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.BranchName;
};
var stringFilter = function (record) {
    if (this.dataIndex == 'BranchID') {
        App.cboBranchID.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboBranchID.store, "BranchID", "BranchName");
    }
}
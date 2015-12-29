var keys = ['CountryID'];
var fieldsCheckRequire = ["CountryID", "Descr"];
var fieldsLangCheckRequire = ["CountryID", "Descr"];
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdDet);
            break;
        case "prev":
            HQ.grid.prev(App.grdDet);
            break;
        case "next":
            HQ.grid.next(App.grdDet);
            break;
        case "last":
            HQ.grid.last(App.grdDet);
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
                HQ.grid.insert(App.grdDet);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmData.selected.items[0] != undefined) {
                    var rowindex = HQ.grid.indexSelect(App.grdDet);
                    if (rowindex != '')
                        HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDet), ''], 'deleteData', true)
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoData, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;     
    }

};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoData.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoData_changed = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20600');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoData_load = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20600');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoData_beforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdDet_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdDet_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdDet, e, keys);
};
var grdDet_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDet, e, keys);
};
var grdDet_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDet);
    stoData_changed(App.stoData);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI20600/Save',
            params: {
                lstData: HQ.store.getData(App.stoData)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
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
        App.grdDet.deleteSelected();
        stoData_changed(App.stoData);
    }
};
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();
    }
};
///////////////////////////////////
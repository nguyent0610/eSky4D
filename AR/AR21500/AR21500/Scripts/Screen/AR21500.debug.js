//// Declare //////////////////////////////////////////////////////////
var keys = ['DispMethod'];
var fieldsCheckRequire = ["DispMethod"];
var fieldsLangCheckRequire = ["DispMethod"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdDisplayMethod);
            break;
        case "prev":
            HQ.grid.prev(App.grdDisplayMethod);
            break;
        case "next":
            HQ.grid.next(App.grdDisplayMethod);
            break;
        case "last":
            HQ.grid.last(App.grdDisplayMethod);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoDisplayMethod.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdDisplayMethod, keys);
            }
            break;
        case "delete":
            if (App.slmDisplayMethod.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdDisplayMethod);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDisplayMethod), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoDisplayMethod, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.cboType.store.load(function () {
        App.cboStyle.store.load(function () {
            App.cboShelf.store.load(function () {
                App.stoDisplayMethod.reload();
            })
        })
    });

};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR21500');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR21500');
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

var grdDisplayMethod_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdDisplayMethod_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdDisplayMethod, e, keys);
};

var grdDisplayMethod_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDisplayMethod, e, keys);
};

var grdDisplayMethod_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDisplayMethod);
    stoChanged(App.stoDisplayMethod);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'AR21500/Save',
            params: {
                lstAR_DisplayMethod: HQ.store.getData(App.stoDisplayMethod)
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
        App.grdDisplayMethod.deleteSelected();
        stoChanged(App.stoDisplayMethod);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoDisplayMethod.reload();
    }
};
///////////////////////////////////

var rdType = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboTypeAR21500_pcLoadType.findRecord("Code", rec.data.Type);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
var rdStyle = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboStyleAR21500_pcLoadStyle.findRecord("Code", rec.data.Style);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
var rdShelf = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboShelfAR21500_pcLoadShelf.findRecord("Code", rec.data.Shelf);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var ActiveCheckAll_Change = function (value) {
    if (value) {
        App.grdDisplayMethod.getStore().each(function (item) {

            item.set("Active", value.checked);
        });
    }
};

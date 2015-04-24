//// Declare //////////////////////////////////////////////////////////

var keys = ['CatID'];
var fieldsCheckRequire = ["CatID", "Descr"];
var fieldsLangCheckRequire = ["CatID", "Descr"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdTaxCat);
            break;
        case "prev":
            HQ.grid.prev(App.grdTaxCat);
            break;
        case "next":
            HQ.grid.next(App.grdTaxCat);
            break;
        case "last":
            HQ.grid.last(App.grdTaxCat);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoTaxCat.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdTaxCat, keys);
            }
            break;
        case "delete":
            if (App.slmTaxCat.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdTaxCat);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdTaxCat), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoTaxCat, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var grdTaxCat_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdTaxCat_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdTaxCat, e, keys);
};
var grdTaxCat_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTaxCat, e, keys);
};
var grdTaxCat_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTaxCat);
    stoChanged(App.stoTaxCat);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI20900/Save',
            params: {
                lstTaxCat: HQ.store.getData(App.stoTaxCat)
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
        App.grdTaxCat.deleteSelected();
        stoChanged(App.stoTaxCat);
    }
};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoTaxCat.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20900');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI20900');
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

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoTaxCat.reload();
    }
};
///////////////////////////////////







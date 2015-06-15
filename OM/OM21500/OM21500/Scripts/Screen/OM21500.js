//// Declare //////////////////////////////////////////////////////////

var keys = ['DiscCode'];
var fieldsCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate"];
var fieldsLangCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_DiscDescr);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_DiscDescr);
            break;
        case "next":
            HQ.grid.next(App.grdOM_DiscDescr);
            break;
        case "last":
            HQ.grid.last(App.grdOM_DiscDescr);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_DiscDescr.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_DiscDescr, keys);
            }
            break;
        case "delete":
            if (App.slmOM_DiscDescr.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_DiscDescr, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoOM_DiscDescr.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21500');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21500');
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
var grdOM_DiscDescr_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_DiscDescr_Edit = function (item, e) {
    if (e.field == 'ToDate') {
        if (e.value < e.record.data.FromDate) {
            HQ.message.show(2015061501, '', '');
        }
        else {
            HQ.grid.checkInsertKey(App.grdOM_DiscDescr, e, keys);
        }
    }


};
var grdOM_DiscDescr_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_DiscDescr, e, keys);
};
var grdOM_DiscDescr_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_DiscDescr);
    stoChanged(App.stoOM_DiscDescr);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21500/Save',
            params: {
                lstOM_DiscDescr: HQ.store.getData(App.stoOM_DiscDescr)
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
        App.grdOM_DiscDescr.deleteSelected();
        stoChanged(App.stoOM_DiscDescr);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_DiscDescr.reload();
    }
};
///////////////////////////////////
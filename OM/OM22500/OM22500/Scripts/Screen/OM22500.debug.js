//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Descr"];
var fieldsLangCheckRequire = ["Code", "Descr"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_ReasonCode);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_ReasonCode);
            break;
        case "next":
            HQ.grid.next(App.grdOM_ReasonCode);
            break;
        case "last":
            HQ.grid.last(App.grdOM_ReasonCode);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoOM_ReasonCode.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_ReasonCode, keys);
            }
            break;
        case "delete":
            if (App.slmOM_ReasonCode.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_ReasonCode, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdOM_ReasonCode_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_ReasonCode_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_ReasonCode, e, keys);
};
var grdOM_ReasonCode_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_ReasonCode, e, keys);
};
var grdOM_ReasonCode_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_ReasonCode);
    stoChanged(App.stoOM_ReasonCode);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM22500/Save',
            params: {
                lstOM_ReasonCode: HQ.store.getData(App.stoOM_ReasonCode)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
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
        App.grdOM_ReasonCode.deleteSelected();
        stoChanged(App.stoOM_ReasonCode);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, 'OM22500');//khi dong roi gan lai cho change la false
        HQ.common.close(this);
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoOM_ReasonCode.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22500');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22500');
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

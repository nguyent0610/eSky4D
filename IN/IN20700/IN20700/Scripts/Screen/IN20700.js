//// Declare //////////////////////////////////////////////////////////

var keys = ['ReasonCD'];
var fieldsCheckRequire = ["ReasonCD"];
var fieldsLangCheckRequire = ["ReasonCD"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":

            HQ.grid.first(App.grdReasonCode);
            break;
        case "prev":
            HQ.grid.prev(App.grdReasonCode);
            break;
        case "next":
            HQ.grid.next(App.grdReasonCode);
            break;
        case "last":
            HQ.grid.last(App.grdReasonCode);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoReasonCode.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdReasonCode, keys);
            }
            break;
        case "delete":
            if (App.slmReasonCode.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoReasonCode, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var grdReasonCode_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdReasonCode_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdReasonCode, e, keys);
};
var grdReasonCode_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdReasonCode, e, keys);
};
var grdReasonCode_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdReasonCode);
    stoChanged(App.stoReasonCode);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IN20700/Save',
            params: {
                lstReasonCode: HQ.store.getData(App.stoReasonCode)
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
        App.grdReasonCode.deleteSelected();
        stoChanged(App.stoReasonCode);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, 'IN20700');//khi dong roi gan lai cho change la false
        HQ.common.close(this);
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoReasonCode.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20700');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20700');
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









//// Declare //////////////////////////////////////////////////////////
var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Type", "Title00", "Title01", "Msg00", "Msg01"];
var fieldsLangCheckRequire = ["Code", "Type", "Title00", "Title01", "Msg00", "Msg01"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Message);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Message);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Message);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Message);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_Message.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Message, keys);
            }
            break;
        case "delete":
            if (App.slmSYS_Message.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Message, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoSYS_Message.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA01100');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA01100');
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

var grdSYS_Message_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSYS_Message_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Message, e, keys);
};

var grdSYS_Message_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Message, e, keys);
};

var grdSYS_Message_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Message);
    stoChanged(App.stoSYS_Message);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA01100/Save',
            params: {
                lstSYS_Message: HQ.store.getData(App.stoSYS_Message)
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
        App.grdSYS_Message.deleteSelected();
        stoChanged(App.stoSYS_Message);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Message.reload();
    }
};
////////////////////////////////////////////////////////////////////////
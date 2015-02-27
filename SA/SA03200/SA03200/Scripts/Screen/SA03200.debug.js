//// Declare //////////////////////////////////////////////////////////
var keys = ['PDAID', 'BranchID', 'SlsperId'];
var fieldsCheckRequire = ["PDAID", "BranchID", "SlsperId", "LicenseKey"];
var fieldsLangCheckRequire = ["PDAID", "BranchID", "SlsperId", "LicenseKey"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdPPC_License);
            break;
        case "prev":
            HQ.grid.prev(App.grdPPC_License);
            break;
        case "next":
            HQ.grid.next(App.grdPPC_License);
            break;
        case "last":
            HQ.grid.last(App.grdPPC_License);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoPPC_License.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdPPC_License);
            }
            break;
        case "delete":
            if (App.slmPPC_License.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoPPC_License, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoPPC_License)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var StatusCheckAll_Change = function (value) {
    if (value) {
        App.grdPPC_License.getStore().each(function (item) {
            item.set("Status", value.checked);
        });
    }
}

var grdPPC_License_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdPPC_License_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdPPC_License, e, keys);
};
var grdPPC_License_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdPPC_License, e, keys);
};
var grdPPC_License_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdPPC_License);
    stoChanged(App.stoPPC_License);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA03200/Save',
            params: {
                lstPPC_License: HQ.store.getData(App.stoPPC_License)
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
        App.grdPPC_License.deleteSelected();
        stoChanged(App.stoPPC_License);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoPPC_License.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03200');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03200');
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

//// Declare //////////////////////////////////////////////////////////

var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID"];
var fieldsLangCheckRequire = ["BranchID"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_CloseDateSetUp);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_CloseDateSetUp);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_CloseDateSetUp);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_CloseDateSetUp);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_CloseDateSetUp.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_CloseDateSetUp, keys);
            }
            break;
        case "delete":
            if (App.slmSYS_CloseDateSetUp.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_CloseDateSetUp, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var renderBranchName = function (value) {
    var record = App.cboBranchIDSA40000_pcCompany.findRecord("CpnyID", value);
    if (record) {
        return record.data.CpnyName;
    }
    else {
        return '';
    }
};

var renderTerritory = function (value) {
    var record = App.cboBranchIDSA40000_pcCompany.findRecord("CpnyID", value);
    if (record) {
        return record.data.Territory;
    }
    else {
        return '';
    }
};

var renderAddress = function (value) {
    var record = App.cboBranchIDSA40000_pcCompany.findRecord("CpnyID", value);
    if (record) {
        return record.data.Address;
    }
    else {
        return '';
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_CloseDateSetUp.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA40000');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA40000');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keys);
            HQ.store.insertRecord(sto, keys, { WrkAdjDate: new Date(_dateServer) });
                //, WrkOpenDate: new Date(_dateServer)
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdSYS_CloseDateSetUp_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_CloseDateSetUp_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_CloseDateSetUp, e, keys);

    //if (e.field == "BranchID") {
    //    var selectedRecord = App.cboBranchID.store.findRecord(e.field, e.value);
    //    if (selectedRecord) {
    //        e.record.set("Territory", selectedRecord.data.Territory);
    //    }
    //    else {
    //        e.record.set("Territory", "");
    //    }
    //}

    //if (e.field == "BranchID") {
    //    var selectedRecord = App.cboBranchID.store.findRecord(e.field, e.value);
    //    if (selectedRecord) {
    //        e.record.set("CpnyName", selectedRecord.data.CpnyName);
    //    }
    //    else {
    //        e.record.set("CpnyName", "");
    //    }
    //}

    //if (e.field == "BranchID") {
    //    var selectedRecord = App.cboBranchID.store.findRecord(e.field, e.value);
    //    if (selectedRecord) {
    //        e.record.set("Address", selectedRecord.data.Address);
    //    }
    //    else {
    //        e.record.set("Address", "");
    //    }
    //}
};
var grdSYS_CloseDateSetUp_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_CloseDateSetUp, e, keys);
};
var grdSYS_CloseDateSetUp_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_CloseDateSetUp);
    stoChanged(App.stoSYS_CloseDateSetUp);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA40000/Save',
            params: {
                lstSYS_CloseDateSetUp: HQ.store.getData(App.stoSYS_CloseDateSetUp)
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
        App.grdSYS_CloseDateSetUp.deleteSelected();
        stoChanged(App.stoSYS_CloseDateSetUp);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_CloseDateSetUp.reload();
    }
};
///////////////////////////////////









//// Declare //////////////////////////////////////////////////////////
var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID"];
var fieldsLangCheckRequire = ["BranchID"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboZone.getStore().load(function () {
        App.cboTerritory.getStore().load(function () {
            App.cboState.getStore().load(function () {
                HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
            })
        })
    })
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_FCSBranch);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_FCSBranch);
            break;
        case "next":
            HQ.grid.next(App.grdOM_FCSBranch);
            break;
        case "last":
            HQ.grid.last(App.grdOM_FCSBranch);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_FCSBranch.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                //App.App
                HQ.grid.insert(App.grdOM_FCSBranch, keys);
            }
            break;
        case "delete":
            if (App.slmOM_FCSBranch.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_FCSBranch, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var beforeSelectcombo = function () {
    loadSourceCombo();
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoOM_FCSBranch.reload();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23101');
    App.cboState.setReadOnly(HQ.isChange);
    App.cboTerritory.setReadOnly(HQ.isChange);
    App.cboZone.setReadOnly(HQ.isChange);
    App.dateFcs.setReadOnly(HQ.isChange);
    App.btnSearch.setDisabled(HQ.isChange);
    //App.dateFcs.setDisabled(HQ.isChange);
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23101');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    HQ.common.showBusy(false);
    stoChanged(App.stoOM_FCSBranch);
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var grdOM_FCSBranch_BeforeEdit = function (editor, e) {
    // thang nho hon thi khong cho sua
    var d = new Date(App.dateFcs.getValue());

    if (d.getYear() > _dateServer.getYear()) {
        return HQ.grid.checkBeforeEdit(e, keys);
    }
    else if (d.getYear() == _dateServer.getYear()) {
        if (d.getMonth() < _dateServer.getMonth()) {
            return false;
        }
        else if (d.getMonth() >= _dateServer.getMonth()) {
            return HQ.grid.checkBeforeEdit(e, keys);
        }
    }
    else if (d.getYear() > _dateServer.getYear()) {
        return false;
    }
};

var grdOM_FCSBranch_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_FCSBranch, e, keys);
    if (e.field == "BranchID") {
        var selectedRecord = App.cboBranchID.store.findRecord('CpnyID', e.value);
        if (selectedRecord) {
            e.record.set("CpnyName", selectedRecord.data.CpnyName);
        }
        else {
            e.record.set("CpnyName", "");
        }
    }
};

var grdOM_FCSBranch_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_FCSBranch, e, keys);
};
var grdOM_FCSBranch_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_FCSBranch);
    stoChanged(App.stoOM_FCSBranch);
};

var btnSearch_Click = function (sender, e) {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        HQ.isFirstLoad = true;
        App.grdOM_FCSBranch.show();
        App.stoOM_FCSBranch.reload();
    }
};

var dateFcs_expand = function (dte, eOpts) {
    dte.picker.setWidth(300);
    dte.picker.monthEl.setWidth(200);
};

var cboZone_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCSBranch.store.removeAll();
        App.grdOM_FCSBranch.hide();
        App.cboTerritory.store.load();
    }
};
var cboTerritory_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCSBranch.store.removeAll();
        App.grdOM_FCSBranch.hide();
        App.cboState.store.load();
    }
};
var cboState_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCSBranch.store.removeAll();
        App.grdOM_FCSBranch.hide();
        App.cboBranchID.store.reload();
    }
};

var dateFcs_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCSBranch.store.removeAll();
        App.grdOM_FCSBranch.hide();
    }
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    //HQ.isFirstLoad = true;
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM23101/Save',
            params: {
                //lstOM_FCS: Ext.HQ.store.getData(App.stoOM_FCSBranch)
                lstOM_FCSBranch: Ext.encode(App.stoOM_FCSBranch.getChangedData({ skipIdForPhantomRecords: false })),
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_FCSBranch.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_FCSBranch.deleteSelected();
        stoChanged(App.stoOM_FCSBranch);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_FCSBranch.reload();
    }
};
////////////////////////////////////////////////////////////////////////
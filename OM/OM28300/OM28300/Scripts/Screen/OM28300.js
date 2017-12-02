
var keys = ['BranchID', 'EquipmentID', 'IMEI'];
var fieldsCheckRequire = ["BranchID", "EquipmentID", "IMEI"];
var fieldsLangCheckRequire = ["BranchID", "EquipmentID", "IMEI"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_EquipmentStatus.reload();
        HQ.common.showBusy(false);
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_EquipmentStatus);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_EquipmentStatus);
            break;
        case "next":
            HQ.grid.next(App.grdOM_EquipmentStatus);
            break;
        case "last":
            HQ.grid.last(App.grdOM_EquipmentStatus);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_EquipmentStatus.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_EquipmentStatus, keys);
            }
            break;
        case "delete":
            if (App.slmData.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmData.selected.items[0] != undefined) {
                        if (App.slmData.selected.items[0].data.EquipmentID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_EquipmentStatus)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_EquipmentStatus, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var grdOM_EquipmentStatus_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};
var grdOM_EquipmentStatus_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_EquipmentStatus, e, keys);
    frmChange();
};
var grdOM_EquipmentStatus_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_EquipmentStatus, e, keys);
};
var grdOM_EquipmentStatus_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_EquipmentStatus);
    frmChange();
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM28300/Save',
            params: {
                lstData: HQ.store.getData(App.stoOM_EquipmentStatus)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_EquipmentStatus.deleteSelected();
        frmChange();
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    App.btnExport.setVisible(HQ.allowExport);
    App.btnImport.setVisible(HQ.allowImport);
    App.cboStatus.store.reload();
    checkLoad(); // Mới
}
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoOM_EquipmentStatus);
    HQ.common.changeData(HQ.isChange, 'OM28300');//co thay doi du lieu gan * tren tab title header
};

var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
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
        App.stoOM_EquipmentStatus.reload();
    }
};
///////////////////////////////////

var renderStatus = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboStatus.store, ['Code'], [record.data.Status]);
    if (Ext.isEmpty(r))
        return value;
    else
        return r.data.Descr;
};
var stringFilterStatus = function (record) {

    if (this.dataIndex == 'Status') {
        App.cboStatus.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboStatus.store, "Code", "Descr");
    }

    return HQ.grid.filterString(record, this);
}
//////////// IMPORT/ EXPORT /////////////////////////////////////////////
var btnExport_Click = function () {
    App.frmMain.submit({
        waitMsg: HQ.common.getLang("Exporting"),
        url: 'OM28300/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
            lstOM_EquipmentStatus: Ext.encode(App.stoOM_EquipmentStatus.getRecordsValues())
        },
        success: function (msg, data) {
            window.location = 'OM28300/DownloadAndDelete?file=' + data.result.fileName;
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};

var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'OM28300/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_EquipmentStatus.reload();
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701',ext, '');
        sender.reset();
    }
};



var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchID.findRecord("BranchID", rec.data.BranchID);
    if (record) {
        return record.data.BranchName;
    }
    else {
        return value;
    }
};
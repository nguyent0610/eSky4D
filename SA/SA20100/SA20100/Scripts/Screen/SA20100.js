//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var _statusID = '';
var keys = ['StatusID'];
var fieldsCheckRequire = ['StatusID', 'StatusName', 'LangID'];
var fieldsLangCheckRequire = ['LangStatus', 'StatusName', 'SA20100LangID'];
//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
//load lần đầu khi mở
var firstLoad = function () {
    HQ.util.checkAccessRight();
    App.frmMain.isValid();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoStatus, keys, ['']);
        if (!record) {
            HQ.store.insertBlank(App.stoStatus, keys);
        }
    }
    App.stoStatus.reload();
};
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoStatus);
    HQ.common.changeData(HQ.isChange, 'SA20100');
};
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdStatus);
            break;
        case "prev":
            HQ.grid.prev(App.grdStatus);
            break;
        case "next":
            HQ.grid.next(App.grdStatus);
            break;
        case "last":
            HQ.grid.last(App.grdStatus);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    HQ.isFirstLoad = true;
                    App.stoStatus.reload();
                }
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdStatus, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                
                if (App.slmStatus.selected.items[0] != undefined) {
                    if (App.stocheckDelete.data.items[0].data.Delete == 0)
                    {
                        if (App.slmStatus.selected.items[0].data.StatusID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdStatus)], 'deleteData', true);
                        }
                    }
                    else
                    {
                        var StatusID = App.grdStatus.selModel.selected.items[0].data.StatusID;
                        HQ.message.show(2018081760, [App.grdStatus.columnManager.columns[2].text, StatusID], '', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoStatus, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                    ) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            break;
    }
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
var stoLoadStatus = function (sto) {
    HQ.isFirstLoad = false;
    HQ.common.showBusy(false);
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoStatus, keys, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }
    }
    frmChange();
};
var grdStatus_BeforeEdit = function (item, e) {
    _statusID = e.record.data.StatusID;
    if (!HQ.grid.checkBeforeEdit(e, keys))
        return false;
};
var grdStatus_Edit = function (item, e) {
    if (e.field == 'LangID')
    {
        var objLangID = HQ.store.findInStore(App.cboLangID.store, ['LangID'], [e.value]);
        if(objLangID != undefined)
        {
            e.record.set('Content', objLangID.Content);
        }
        else
        {
            e.record.set('Content', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdStatus, e, keys);
    frmChange();
};
var grdStatus_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdStatus, e, keys, true);
};
var grdStatus_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdStatus);
    frmChange();
};
var grdStatus_Select = function (item, record) {
    _statusID = record.data.StatusID;
    App.stocheckDelete.reload();
}
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA20100/Save',
            params: {
                lstPotential: HQ.store.getData(App.stoStatus),

            },
            success: function (msg, data) {
                HQ.isFirstLoad = true;
                HQ.message.show(201405071);
                App.stoStatus.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        App.grdStatus.deleteSelected();
        frmChange();
    }
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoStatus.reload();
    }
};
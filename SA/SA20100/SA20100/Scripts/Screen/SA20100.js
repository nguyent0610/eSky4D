//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;
var _statusID = '';
var _statusType = '';
var _record = '';
var lstDelete = [];
var keys = ['StatusID', 'StatusType'];
var fieldsCheckRequire = ['StatusID', 'StatusType', 'StatusName', 'LangID'];
var fieldsLangCheckRequire = ['LangStatus', 'StatusType', 'StatusName', 'SA20100LangID'];
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        App.stoStatus.reload();
        _isLoadMaster = true
        _Source = 0;
    }
};
////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
//load lần đầu khi mở
var firstLoad = function () {
    HQ.util.checkAccessRight();
    App.frmMain.isValid();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboStatusType.getStore().addListener('load', checkLoad);
    App.cboLangID.getStore().addListener('load', checkLoad);
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoStatus, keys, ['', '']);
        if (!record) {
            HQ.store.insertBlank(App.stoStatus, keys);
        }
    }
    App.cboLangID.store.reload();
    App.cboStatusType.store.reload();
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
                    if (App.slmStatus.selected.items[0].data.StatusID != "" && App.slmStatus.selected.items[0].data.StatusType != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdStatus)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequirePass(App.stoStatus, keys, fieldsCheckRequire, fieldsLangCheckRequire)
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
        var record = HQ.store.findRecord(App.stoStatus, keys, ['', '']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }
    }
    frmChange();
    if(HQ.showContentEng)
        HQ.grid.show(App.grdStatus, ['ContentEng']);
    else
        HQ.grid.hide(App.grdStatus, ['ContentEng']);
};

var grdStatus_BeforeEdit = function (item, e) {
    _statusID = e.record.data.StatusID;
    _statusType = e.record.data.StatusType;
    App.cboLangID.forceSelection = false;
    if (!HQ.grid.checkBeforeEdit(e, keys))
        return false;
    if (e.field == 'LangID') {
        var objPageLang = findRecordCombo(e.value);
        if (objPageLang) {
            var positionLang = calcPage(objPageLang.index);
            App.cboLangID.loadPage(positionLang);
        }
    }
};

var grdStatus_Edit = function (item, e) {
    if (e.field == 'LangID')
    {
        var objLangID = HQ.store.findInStore(App.cboLangID.store, ['LangID'], [e.value]);
        if(objLangID != undefined)
        {
            e.record.set('Content', objLangID.Content);
            e.record.set('ContentEng', objLangID.ContentEng);
        }
        else
        {
            e.record.set('Content', '');
            e.record.set('ContentEng', '');
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
    _statusType = record.data.StatusType;
    _record = record;
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
function renderStatusType(value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboStatusType.store, ['Code'], [record.data.StatusType])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.Descr;
};
function stringFilter(record) {
    if (this.dataIndex == 'StatusType') {
        App.cboStatusType.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboStatusType.store, "Code", "Descr");
    }
};
var checkRequirePass = function (store, keys, fieldsCheck, fieldsLang) {
    var sto = App.stoStatus.allData != undefined ? App.stoStatus.allData : App.stoStatus.data;

        for (var i = 0; i < sto.length; i++) {
            for (var jkey = 0; jkey < keys.length; jkey++) {
                if (sto.items[i].data[keys[jkey]]) {
                    for (var k = 0; k < fieldsCheck.length; k++) {
                        if (HQ.util.passNull(sto.items[i].data[fieldsCheck[k]]).toString().trim() == "") {
                            HQ.message.show(2018091002, [i+1,HQ.common.getLang(fieldsLang == undefined ? fieldsCheck[k] : fieldsLang[k])], '', true);
                            return false;

                        }
                    }
                    break; // Check data one time
                }
            }
        }

    return true;
};
var findRecordCombo = function (value) {
    var data = null;
    var store = App.cboLangID.store;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data.LangID == value) {
            data = record;
            return false;
        }
    });
    return data;
};
var calcPage = function (value) {
    var tmpValue = (Number(value) + 1) / 20;
    if (Number.isInteger(tmpValue))
        return Number(tmpValue);
    else
        return Math.floor(Number(tmpValue)) + 1;
};

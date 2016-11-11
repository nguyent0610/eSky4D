//// Declare //////////////////////////////////////////////////////////
var msgError = '';
var keys = ['SlsFreqID', 'WeekofVisit'];
var fieldsCheckRequire = ["SlsFreqID", "WeekofVisit","Descr"];
var fieldsLangCheckRequire = ["SlsFreqID", "WeekofVisit", "Descr"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_WeekOfVisit.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboFreq.getStore().addListener('load', checkLoad);
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_WeekOfVisit);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_WeekOfVisit);
            break;
        case "next":
            HQ.grid.next(App.grdOM_WeekOfVisit);
            break;
        case "last":
            HQ.grid.last(App.grdOM_WeekOfVisit);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_WeekOfVisit, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmOM_WeekOfVisit.selected.items[0] != undefined) {
                    if (App.slmOM_WeekOfVisit.selected.items[0].data.SlsFreqID != "" &&
                        App.slmOM_WeekOfVisit.selected.items[0].data.WeekofVisit != ""
                        ) {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_WeekOfVisit)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_WeekOfVisit, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22700');
};

var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22700');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var grdOM_WeekOfVisit_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdOM_WeekOfVisit_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_WeekOfVisit, e, keys);
    stoChanged(App.stoOM_WeekOfVisit);
};

var grdOM_WeekOfVisit_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_WeekOfVisit, e, keys);
};

var grdOM_WeekOfVisit_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_WeekOfVisit);
    stoChanged(App.stoOM_WeekOfVisit);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (CheckData()) {
        HQ.message.show(201302071, [msgError.slice(0, -2)], '', true);
    }
    else {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                timeout: 1800000,
                waitMsg: HQ.common.getLang("SavingData"),
                url: 'OM22700/Save',
                params: {
                    lstOM_WeekOfVisit: HQ.store.getData(App.stoOM_WeekOfVisit)
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    refresh('yes');
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

var CheckData = function () {
    msgError = '';
    var stt = 0;
    var store = App.stoOM_WeekOfVisit;
    var allRecords = store.snapshot || store.allData || store.data;
    store.suspendEvents();
    allRecords.each(function (item) {
        var total = item.data.SlsFreqID == 'F8' ? 2 : 3;
        var count = 0;
        if (item.data.SlsFreqID != '' && item.data.WeekofVisit != '') {
            stt++;
            if (item.data.Mon)
                count++;
            if (item.data.Tue)
                count++;
            if (item.data.Wed)
                count++;
            if (item.data.Thu)
                count++;
            if (item.data.Fri)
                count++;
            if (item.data.Sat)
                count++;
            if (item.data.Sun)
                count++;

            if (count != total)
                msgError += stt + ', ';
        }
    });
    store.resumeEvents();

    if (msgError)
        return true;
    else
        return false;
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_WeekOfVisit.deleteSelected();
        stoChanged(App.stoOM_WeekOfVisit);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_WeekOfVisit.reload();
    }
};
///////////////////////////////////
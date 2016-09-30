//// Declare //////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 3;
var _isLoadMaster = false;

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdData);
            break;
        case "prev":
            HQ.grid.prev(App.grdData);
            break;
        case "next":
            HQ.grid.next(App.grdData);
            break;
        case "last":
            HQ.grid.last(App.grdData);
            break;
        case "refresh":     
            break;
        case "new":
            break;
        case "delete":
            break;
        case "save":
            break;
        case "print":
            break;
        case "close":         
            break;
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboBranchID.getStore().addListener('load', checkLoad);
    App.cboBranchID1.getStore().addListener('load', checkLoad);
    App.stoWeekOfVisit.addListener('load', checkLoad);
};

var cboBranchID_Change = function (sender, value) {
    if (sender.valueModels != null && sender.valueModels.length > 0) {
        App.cboPJPID.clearValue();
        App.cboPJPID.store.reload();
        App.cboSalesMan.clearValue();
        App.cboSalesMan.store.reload();
        App.cboRouteID.clearValue();
        App.cboRouteID.store.reload();
    }
};

var cboBranchID_Select = function (sender, value) {
    if (sender.valueModels != null && sender.valueModels.length > 0) {
        App.cboPJPID.clearValue();
        App.cboPJPID.store.reload();
        App.cboSalesMan.clearValue();
        App.cboSalesMan.store.reload();
        App.cboRouteID.clearValue();
        App.cboRouteID.store.reload();
    }
};

var cboBranchID_TriggerClick = function (sender,value) {
    if (value) {
        App.cboBranchID.clearValue();
        App.cboPJPID.clearValue();
        App.cboSalesMan.clearValue();
        App.cboRouteID.clearValue();
    }
};

var cboBranchID1_Change = function (sender, value) {
    if (sender.valueModels != null && sender.valueModels.length > 0) {
        App.cboPJPID1.clearValue();
        App.cboPJPID1.store.reload();
        App.cboSalesMan1.clearValue();
        App.cboSalesMan1.store.reload();
        App.cboRouteID1.clearValue();
        App.cboRouteID1.store.reload();
    }
};

var cboBranchID1_Select = function (sender, value) {
    if (sender.valueModels != null && sender.valueModels.length > 0) {
        App.cboPJPID1.clearValue();
        App.cboPJPID1.store.reload();
        App.cboSalesMan1.clearValue();
        App.cboSalesMan1.store.reload();
        App.cboRouteID1.clearValue();
        App.cboRouteID1.store.reload();
    }
};

var cboBranchID1_TriggerClick = function (sender, value) {
    if (value) {
        App.cboBranchID1.clearValue();
        App.cboPJPID1.clearValue();
        App.cboSalesMan1.clearValue();
        App.cboRouteID1.clearValue();
    }
};

var chkSelect_Change = function (chk, newValue, oldValue, eOpts) {
    var store = App.stoData;
    store.suspendEvents();
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        record.set('Selected', newValue);
    });
    store.resumeEvents();
    App.grdData.view.refresh();
};

var btnLoadData_Click = function () {
    App.cboBranchID1.allowBlank = true;
    App.cboBranchID1.isValid(false);
    App.cboPJPID1.allowBlank = true;
    App.cboPJPID1.isValid(false);
    App.cboSalesMan1.allowBlank = true;
    App.cboSalesMan1.isValid(false);
    App.cboRouteID1.allowBlank = true;
    App.cboRouteID1.isValid(false);
    if (HQ.form.checkRequirePass(App.frmMain))
        App.stoData.reload();
};

var btnProcess_Click = function () {
    App.cboBranchID1.allowBlank = false;
    App.cboBranchID1.isValid(true);
    App.cboPJPID1.allowBlank = false;
    App.cboPJPID1.isValid(true);
    App.cboSalesMan1.allowBlank = false;
    App.cboSalesMan1.isValid(true);
    App.cboRouteID1.allowBlank = false;
    App.cboRouteID1.isValid(true);
    if (HQ.form.checkRequirePass(App.frmMain)) {
        if (checkGrid()) {
            if ((App.cboBranchID.getValue() == App.cboBranchID1.getValue())
                && (App.cboPJPID.getValue() == App.cboPJPID1.getValue())
                && (App.cboSalesMan.getValue() == App.cboSalesMan1.getValue())
                && (App.cboRouteID.getValue() == App.cboRouteID1.getValue())) {
                HQ.message.show(2016091310, '', '');
            }
            else {
                OM22400_Process();
            }
        }
        else {
            HQ.message.show(2016091210, '', '');
        }
    }
};

var checkGrid = function () {
    var store = App.stoData;
    var field = "Selected";
    var count = 0;
    store.suspendEvents();
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data[field]) {
            count++;
            store.resumeEvents();
            return false;
        }
    });
    store.resumeEvents();
    if (count > 0)
        return true;
    else
        return false;
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoData_Load = function (sto) {
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

////Function menuClick
var OM22400_Process = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM22400/Process',
            params: {
                lstData: HQ.store.getAllData(App.stoData, ["Selected"], [true]),
            },
            success: function (msg, data) {
                HQ.message.show(8009,'','');
                btnLoadData_Click();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var renderWeekOfVisit = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.stoWeekOfVisit.findRecord("Code", rec.data.WeekofVisit);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var stringFilter = function (record) {
    if (this.dataIndex == 'WeekofVisit') {
        App.stoWeekOfVisit.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.stoWeekOfVisit, "Code", "Descr");
    }
    else return HQ.grid.filterString(record, this);
}
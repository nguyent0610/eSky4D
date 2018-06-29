//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;
var keys = ['CustID','CustID_Vendor'];
var fieldsCheckRequire = ['CustID','CustID_Vendor','Name_Vendor'];
var fieldsLangCheckRequire = ['CustID','AR24000CustIDVen','AR24000CustNameVen'];
var _status = '';
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
//// First Load ////////////////////////////////////////////////////////
//load lần đầu khi mở
var firstLoad = function () {
    HQ.util.checkAccessRight();
    App.frmMain.isValid();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoLoadMapCustomer, keys, ['', '']);
        if (!record) {
            HQ.store.insertBlank(App.stoLoadMapCustomer, keys);
        }
    }
    App.cboCpnyID.getStore().addListener('load', checkLoad);
    App.cboCustID.getStore().addListener('load', checkLoad);
};
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoLoadMapCustomer);
    HQ.common.changeData(HQ.isChange, 'AR24000');
};
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == "header") {
                HQ.isFirstLoad = true;
                HQ.combo.first(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdMapCustomer') {
                HQ.grid.first(App.grdMapCustomer);
            }
            break;
        case "prev":
            if (HQ.focus == "header") {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdMapCustomer') {
                HQ.grid.prev(App.grdMapCustomer);
            }
            break;
        case "next":
            if (HQ.focus == "header") {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdMapCustomer') {
                HQ.grid.next(App.grdMapCustomer);
            }
            break;
        case "last":
            if (HQ.focus == "header") {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdMapCustomer') {
                HQ.grid.last(App.grdMapCustomer);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    HQ.isFirstLoad = true;
                    App.stoLoadMapCustomer.reload();
                }
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdMapCustomer, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmMapCustomer.selected.items[0] != undefined) {
                    if (App.slmMapCustomer.selected.items[0].data.Market != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdMapCustomer)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoLoadMapCustomer, keys, fieldsCheckRequire, fieldsLangCheckRequire)
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
var stoLoadMapCustomer = function (sto) {
    HQ.isFirstLoad = false;
    HQ.common.showBusy(false);
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoLoadMapCustomer, keys, ['', '']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }
    }
    frmChange();
};
var btnAssess_Click = function () {
    App.grdMapCustomer.store.reload();
    frmChange();
};

var grdMapCustomer_BeforeEdit = function (item, e) {
    if (App.cboCpnyID.getValue() == "" || App.cboCpnyID.getValue()==null)
    {
        HQ.message.show(2018061860);
        return false;
    }
    if(e.field=="CustID")
    {
        var code = "@@@@@@";
        App.cboCustID.store.clearFilter();
        if(App.cboCpnyID.getValue()!='')
        {
            code = App.cboCpnyID.getValue();
        }
        App.cboCustID.store.filter("BranchID", code);
    }
    if (e.field == "CustID" && e.record.data.CustID != "")
    {
        return false;
    }
    if (e.record.data.CustID != "")
    {
        App.cboCpnyID.setReadOnly(true);
    }
    if (!HQ.grid.checkBeforeEdit(e, keys))
        return false;
};
var grdMapCustomer_Edit = function (item, e) {

    var s = HQ.store.findRecord(App.cboCustID.store, ['CustID'], [record.data.CustID]);
    if (s != undefined)
    {
        e.record.set("CustName", s.data.CustName);
    }
    HQ.grid.checkInsertKey(App.grdMapCustomer, e, keys);
    frmChange();
};
var grdMapCustomer_ValidateEdit = function (item, e) {
    if (e.field == "CustID_Vendor")
    {
        return HQ.grid.checkValidateEdit(App.grdMapCustomer, e, keys, true);
    }
    
};
var grdMapCustomer_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdMapCustomer);
    frmChange();
};
var cboCpnyID_Change = function () {
    App.stoLoadMapCustomer.reload();
};
var save = function () {
    //App.frmMain.updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR24000/Save',
            params: {
                lstMapCustomer: HQ.store.getData(App.stoLoadMapCustomer),

            },
            success: function (msg, data) {
                HQ.isFirstLoad = true;
                HQ.message.show(201405071);
                App.stoLoadMapCustomer.reload();
                
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    App.cboCpnyID.setReadOnly(false);
};
var deleteData = function (item) {
    if (item == "yes") {
        App.grdMapCustomer.deleteSelected();
        frmChange();
    }
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoLoadMapCustomer.reload();
        App.cboCpnyID.setReadOnly(false);
    }
};

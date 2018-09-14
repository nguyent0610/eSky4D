//// Declare //////////////////////////////////////////////////////////
var screenNbr = 'SI24300';
var keys = ['State', 'District', 'Ward'];
var fieldsCheckRequire = ["State", "District", "Ward", "WardName"];
var fieldsLangCheckRequire = ["ProvinceCode", "SI24300DistrictCode", "SI24300Ward", "WardName"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var state = '';

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSI_Ward.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_Ward);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_Ward);
            break;
        case "next":
            HQ.grid.next(App.grdSI_Ward);
            break;
        case "last":
            HQ.grid.last(App.grdSI_Ward);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSI_Ward.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_Ward, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSI_Ward.selected.items[0] != undefined) {
                    if (App.slmSI_Ward.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSI_Ward)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_Ward, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            //HQ.common.close(this);            
            break;
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboState.getStore().addListener('load', checkLoad);
    App.cboState.store.reload();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSI_Ward);
    HQ.common.changeData(HQ.isChange, 'SI24300');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoSI_Ward_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    //Sto tiep theo
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};
var cboState_Change = function () {
    App.cboDistrict.store.reload();
};
var grdSI_Ward_BeforeEdit = function (editor, e) {
    state = e.record.data.State;
    if (e.field == 'District') {
        App.cboDistrict.store.reload();
    }
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSI_Ward_Edit = function (item, e) {
    if (e.field == 'State')
    {
        var objState = App.cboState.valueModels[0];
        if (objState != undefined) {
            state = objState.data.State;
            e.record.set('StateName', objState.data.Descr);
            e.record.set('Country', objState.data.Country);
        }
        else {
            e.record.set('StateName', '');
            e.record.set('Country', '');
            e.record.set('District', '');
            e.record.set('DistrictName', '');
        }
    }
    if (e.field == 'District') {
        var objDistrict = App.cboDistrict.valueModels[0];
        if (objDistrict != undefined) {
            e.record.set('DistrictName', objDistrict.data.Name);
        }
        else {
            e.record.set('DistrictName', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdSI_Ward, e, keys);
    frmChange();
};

var grdSI_Ward_ValidateEdit = function (item, e) {
    if (e.field == 'State') {
        state = e.value;
        App.cboDistrict.store.reload();
    }
    return HQ.grid.checkValidateEdit(App.grdSI_Ward, e, keys);
};

var grdSI_Ward_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_Ward);
    frmChange();
    state = '';
};

//////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
////Process Data
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI24300/Save',
            params: {
                lstSI_Ward: HQ.store.getData(App.stoSI_Ward)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSI_Ward.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSI_Ward.deleteSelected();
        frmChange();
    }
};
//Export
var btnExport_Click = function () {
    App.frmMain.submit({
        url: 'SI24300/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {

        },
        success: function (msg, data) {
            alert('sus');
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};
//Import
var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'SI24300/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                    app.stoSI_Ward.reload();
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701', '', '');
        sender.reset();
    }
};
/////////////////////////////////////////
////////////////////////////////////////
//Other Function
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_Ward.reload();
    }
};
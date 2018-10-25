//// Declare //////////////////////////////////////////////////////////
//var screenNbr = 'SI21700';
var keys = ['Country', 'District', 'State'];
var fieldsCheckRequire = ["Country", "State", "District", "Name"];
var fieldsLangCheckRequire = ["Country", "State", "District", "Name"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSI_District.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_District);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_District);
            break;
        case "next":
            HQ.grid.next(App.grdSI_District);
            break;
        case "last":
            HQ.grid.last(App.grdSI_District);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSI_District.reload();
            }
            var record = HQ.store.findRecord(App.stoData, keys, ['Country', 'State']);
            if (!record) {
                HQ.store.insertBlank(App.stoData, keys);
            }
            break;
        case "new":
            if (HQ.isInsert) {
                debugger
                HQ.grid.insert(App.grdSI_District, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSI_District.selected.items[0] != undefined) {
                    if (App.slmSI_District.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSI_District)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_District, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    //checkLoad();
    App.cboCountry.getStore().addListener('load', checkLoad);
    
    if (HQ.countryView == false) {

        keys = ["State", "District"];
        fieldsCheckRequire = ["State", "District", "Name"];
        fieldsLangCheckRequire = ["State", "District", "Name"];
        App.Country.hide();
    }
    else {
        keys = ['Country', 'State', "District"];
        fieldsCheckRequire = ["Country", "State", "District", "Name"];
        fieldsLangCheckRequire = ["Country", "State", "District", "Name"];
        App.Country.show();
    }
    checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSI_District);
    HQ.common.changeData(HQ.isChange, 'SI21700');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoSI_District_Load = function (sto) {
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

var grdSI_District_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSI_District_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSI_District, e, keys);
    frmChange();
};

var grdSI_District_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_District, e, keys);
};

var grdSI_District_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_District);
    frmChange();
};

//var cboCountry_Change = function () {
//    App.cboState.getStore().load(function () {
//        var curRecord = App.frmMain.getRecord();
//        if (curRecord != undefined)
//            if (curRecord.data.State) {
//                App.cboState.setValue(curRecord.data.State);
//            }
//        var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
//        if (!dt) {
//            //curRecord.data.State = '';
//            App.cboState.setValue("");
//        }
//    });
//};
//////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
////Process Data
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21700/Save',
            params: {
                lstSI_District: HQ.store.getData(App.stoSI_District)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSI_District.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSI_District.deleteSelected();
        frmChange();
    }
};
/////////////////////////////////////////
////////////////////////////////////////
//Other Function
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_District.reload();
    }
};



//////////////////////// combo phụ thuộc vào combo
var grdStateClassDetail_Edit = function (item, e) {

    if (e.field == "State") {
        var a = HQ.store.findRecord(App.cboState.store, ['State'], [e.value]);
        if (e.value != "" && e.value != null) {
            e.record.set('DescrState', a.data.DescrState);
        }
        else {
            e.record.set('DescrState', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdSI_District, e, keys);
    frmChange();
};

///////////// Import
var btnImport_Click = function (sender, e) {
    debugger
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'SI21700/Import',
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
///////////////////////////////////
//Export
var btnExport_Click = function () {
    App.frmMain.submit({
        url: 'SI21700/Export',
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

//// Declare //////////////////////////////////////////////////////////
var keys = ['Territory'];
var fieldsCheckRequire = ["Territory", "Descr","Zone"];
var fieldsLangCheckRequire = ["Territory", "Descr","Zone"];

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
        App.stoTerritory.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdTerritory);
            break;
        case "prev":
            HQ.grid.prev(App.grdTerritory);
            break;
        case "next":
            HQ.grid.next(App.grdTerritory);
            break;
        case "last":
            HQ.grid.last(App.grdTerritory);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoTerritory.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdTerritory, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmTerritory.selected.items[0] != undefined) {
                    if (App.slmTerritory.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdTerritory)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoTerritory, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    checkLoad();
   // App.txtDistance.setVisible(HQ.Distance);
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoTerritory);
    HQ.common.changeData(HQ.isChange, 'SA02800');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoTerritory_Load = function (sto) {
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

var grdTerritory_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
    if (e.field == 'Zone')
        App.cboZone.store.clearFilter();
};

var grdTerritory_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdTerritory, e, keys);
    frmChange();
};

var grdTerritory_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTerritory, e, keys);
};

var grdTerritory_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTerritory);
    frmChange();
};

var cboZone_Change = function (item, newValue, oldValue) {
    if (item.valueModels != null) {
        if (item.valueModels.length > 0) {
            var r = HQ.store.findRecord(App.cboZone.getStore(), ["Code"], [item.valueModels[0].data.Code])
            if (!r) {
                App.slmTerritory.getSelection()[0].set('ZoneDescr', "");
                //App.slmTerritory.getSelection()[0].set('BranchID', "");
            }
            else {
                App.slmTerritory.getSelection()[0].set('ZoneDescr', r.data.Descr);
                // App.slmgrdCust.getSelection()[0].set('BranchID', r.data.BranchID);
            }
        }
    }
};
//////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
////Process Data
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21900/Save',
            params: {
                lstTerritory: HQ.store.getData(App.stoTerritory)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoTerritory.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdTerritory.deleteSelected();
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
        App.stoTerritory.reload();
    }
};
var renderDistance = function (value) {
    return Ext.util.Format.number(value, '0,000.00');
};
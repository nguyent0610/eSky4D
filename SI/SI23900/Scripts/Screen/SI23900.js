//// Declare //////////////////////////////////////////////////////////
var keys = ['Zone','Territory','SubTerritory','State','District','Market'];
var fieldsCheckRequire = ["Zone","Territory","SubTerritory","State","District","Market","Descr"];
var fieldsLangCheckRequire = ["Zone", "Territory", "SubTerritory", "State", "District", "Market", "Descr"];

var _Source = 0;
var _maxSource = 7;
var _isLoadMaster = false;
//var _state = "";


///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoApp_Market.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdApp_Market);
            break;
        case "prev":
            HQ.grid.prev(App.grdApp_Market);
            break;
        case "next":
            HQ.grid.next(App.grdApp_Market);
            break;
        case "last":
            HQ.grid.last(App.grdApp_Market);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoApp_Market.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdApp_Market, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmApp_Market.selected.items[0] != undefined) {
                    if (App.slmApp_Market.selected.items[0].data.Market != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdApp_Market)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoApp_Market, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboState.getStore().addListener('load', checkLoad);
    App.cboDistrict.getStore().addListener('load', checkLoad);
    App.cboZone.getStore().addListener('load', checkLoad);
    App.cboTerritory.getStore().addListener('load', checkLoad);
    App.cboSubTerritory.getStore().addListener('load', checkLoad);
    App.cboState.store.reload();
    App.cboDistrict.store.reload();
    App.cboTerritory.store.reload();
    App.cboSubTerritory.store.reload();
    App.cboZone.store.reload();
   // checkLoad();

};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoApp_Market);
    HQ.common.changeData(HQ.isChange, 'SI23900');//co thay doi du lieu gan * tren tab title header
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var State_render = function (value) {
    var record = HQ.store.findRecord(App.cboState.store, ["State"], [value]);
    return (record) ? record.data.Descr : value;
};

var District_render = function (value) {
    var record = HQ.store.findRecord(App.cboDistrict.store, ["District"], [value]);
    return (record) ? record.data.Name : value;
};
var Zone_render = function (value) {
    var record = HQ.store.findRecord(App.cboZone.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var Territory_render = function (value) {
    var record = HQ.store.findRecord(App.cboTerritory.store, ["Territory"], [value]);
    return (record) ? record.data.Descr : value;
};

var SubTerritory_render = function (value) {
    var record = HQ.store.findRecord(App.cboSubTerritory.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var stoApp_Market_Load = function (sto) {
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

var grdApp_Market_BeforeEdit = function (editor, e) {

    //if (e.field == "District")
    //{
    //    _state = e.record.data.State;
    //    App.cboDistrict.store.reload();
    //}

    if (e.field == "District") {
        var tam = "@@@@@@@@@@";
        App.cboDistrict.store.clearFilter();
        if (e.record.data.State != "" && e.record.data.State != null) {
            tam = e.record.data.State;
        }        
        App.cboDistrict.store.filter('State', tam);
    }
    if (e.field == "Territory") {
        var tam = "@@@@@@@@@@";
        App.cboTerritory.store.clearFilter();
        if (e.record.data.Zone != "" && e.record.data.Zone != null) {
            tam = e.record.data.Zone;
        }
        App.cboTerritory.store.filter('Zone', tam);
    }
    if (e.field == "SubTerritory") {
        var tam = "@@@@@@@@@@";
        App.cboSubTerritory.store.clearFilter();
        if (e.record.data.Territory != "" && e.record.data.Territory != null) {
            tam = e.record.data.Territory;
        }
        App.cboSubTerritory.store.filter('Territory', tam);
    }
    if (e.field == "State") {
        var tam = "@@@@@@@@@@";
        App.cboState.store.clearFilter();
        if (e.record.data.Territory != "" && e.record.data.Territory != null) {
            tam = e.record.data.Territory;
        }
        App.cboState.store.filter('Territory', tam);
    }
    if (e.field == "Territory" || e.field=="SubTerritory" || e.field=="State" || e.field=="Market" || e.field=="District")
    {
        if(e.record.data.Zone=="" || e.record.data.Zone==null)
        {
            return false;
        }
    }
    

    if (!HQ.grid.checkBeforeEdit(e, keys))
        return false;
};

var grdApp_Market_Edit = function (item, e) {
    if (e.field == "State") {
        var a = HQ.store.findRecord(App.cboState.store, ['State'], [e.value]);
        if (e.value != "" && e.value !=null)
        {
            e.record.set('Descr_State', a.data.Descr);
        }
        else
        {
            e.record.set('Descr_State', "");
        }
    }
    if (e.field == "District") {
        var a = HQ.store.findRecord(App.cboDistrict.store, ['District'], [e.value]);
        if (e.value != "" && e.value != null) {
            e.record.set('Name', a.data.Name);
        }
        else {
            e.record.set('Name', "");
        }
    }
    App.cboDistrict.store.clearFilter();
    App.cboTerritory.store.clearFilter();
    App.cboSubTerritory.store.clearFilter();
    App.cboState.store.clearFilter();
    HQ.grid.checkInsertKey(App.grdApp_Market, e, keys);
    frmChange();
};

var grdApp_Market_ValidateEdit = function (item, e) {
    if (e.field == "Market") {
        if (e.value == "")
        {
            HQ.message.show(15, e.field);
            return false;
        }
        else
        {
            for (var i = 0; i < App.grdApp_Market.store.snapshot.items.length - 1; i++) {
                if (App.grdApp_Market.store.snapshot.items[i].data.Market == e.value) {
                    HQ.message.show(1112, e.value);
                    return false;
                }
            }
        } 
    }
    else if (e.record.data.Market && e.record.data.Market != "") {
        return HQ.grid.checkValidateEdit(App.grdApp_Market, e, keys, false);
    }
    return HQ.grid.checkValidateEdit(App.grdApp_Market, e, keys);
};

var grdApp_Market_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdApp_Market);
    frmChange();
};
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI23900/Save',
            params: {
                lstApp_Market: HQ.store.getData(App.stoApp_Market)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoApp_Market.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdApp_Market.deleteSelected();
        frmChange();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoApp_Market.reload();
    }
};
var stringState = function (record) {

    if (this.dataIndex == 'State') {
        App.cboState.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboState.store, "State", "Descr");
    }

    return HQ.grid.filterString(record, this);
};
var stringDistrict = function (record) {

    if (this.dataIndex == 'District') {
        App.cboDistrict.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboDistrict.store, "District", "Name");
    }

    return HQ.grid.filterString(record, this);
};

var stringZone = function (record) {

    if (this.dataIndex == 'Zone') {
        App.cboZone.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboZone.store, "Code", "Descr");
    }

    return HQ.grid.filterString(record, this);
};

var stringTerritory = function (record) {

    if (this.dataIndex == 'Territory') {
        App.cboTerritory.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboTerritory.store, "Territory", "Descr");
    }

    return HQ.grid.filterString(record, this);
};

var stringSubTerritory = function (record) {

    if (this.dataIndex == 'SubTerritory') {
        App.cboSubTerritory.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboSubTerritory.store, "Code", "Descr");
    }

    return HQ.grid.filterString(record, this);
};

// Import Export
var btnExport_Click = function () {
    App.frmMain.submit({
//        waitMsg: "Exporting....",
        url: 'SI23900/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        //params: {
        //    lstApp_Market: Ext.encode(App.stoApp_Market.getRecordsValues())
        //},
        success: function (msg, data) {
            HQ.common.showBusy(false);
            alert('sus');
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });

};


var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (HQ.isInsert || HQ.isUpdate) {
        if (ext == "xls" || ext == "xlsx") {
            App.frmMain.submit({
                waitMsg: "Importing....",
                url: 'SI23900/Import',
                timeout: 18000000,
                clientValidation: false,
                method: 'POST',
                params: {
                },
                success: function (msg, data) {
                    HQ.isChange = false;
                    HQ.isFirstLoad = true;
                    App.stoApp_Market.reload();
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
    }
    else {
        HQ.message.show('2017112001', '', '');
    }

};

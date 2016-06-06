//// Declare //////////////////////////////////////////////////////////
var screenNbr = 'SI20500';
var keys = ['Country', 'State', 'City'];
var fieldsCheckRequire = ["Country", "State", "City", "Name"];
var fieldsLangCheckRequire = ["Country", "State", "City", "Name"];

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
        App.stoSI_City.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_City);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_City);
            break;
        case "next":
            HQ.grid.next(App.grdSI_City);
            break;
        case "last":
            HQ.grid.last(App.grdSI_City);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSI_City.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_City, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSI_City.selected.items[0] != undefined) {
                    if (App.slmSI_City.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSI_City)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_City, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.cboCountryID.getStore().addListener('load', checkLoad);
    //checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSI_City);
    HQ.common.changeData(HQ.isChange, 'SI20500');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoSI_City_Load = function (sto) {
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

var grdSI_City_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
    if (e.field == 'State') {
        App.cboState.store.load();
    }
};

var grdSI_City_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSI_City, e, keys);
    frmChange();
};

var grdSI_City_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_City, e, keys);
};

var grdSI_City_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_City);
    frmChange();
};

var cboCountry_Change = function () {
    App.slmSI_City.getSelection()[0].set('State', '');
};


/////////////////////////////////////
////////////////////////////////////
////Process
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI20500/Save',
            params: {
                lstData: HQ.store.getData(App.stoSI_City)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSI_City.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSI_City.deleteSelected();
        frmChange();
    }
};

///////////////////////////////////////////////
//////////////////////////////////////////////
//Other function

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_City.reload();
    }
};


//// Declare //////////////////////////////////////////////////////////
var keys = ['MaterialType'];
var fieldsCheckRequire = ["MaterialType", "Descr"];
var fieldsLangCheckRequire = ["MaterialType", "Descr"];

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
        App.stoSI_MaterialType.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_MaterialType);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_MaterialType);
            break;
        case "next":
            HQ.grid.next(App.grdSI_MaterialType);
            break;
        case "last":
            HQ.grid.last(App.grdSI_MaterialType);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSI_MaterialType.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_MaterialType, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSI_MaterialType.selected.items[0] != undefined) {
                    if (App.slmSI_MaterialType.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSI_MaterialType)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_MaterialType, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.cboBuyer.getStore().addListener('load', checkLoad);
    //checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSI_MaterialType);
    HQ.common.changeData(HQ.isChange, 'SI20400');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoSI_MaterialType_Load = function (sto) {
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

var grdSI_MaterialType_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSI_MaterialType_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSI_MaterialType, e, keys);
    frmChange();
};

var grdSI_MaterialType_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_MaterialType, e, keys);
};

var grdSI_MaterialType_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_MaterialType);
    frmChange();
};


/////////////////////////////////////
////////////////////////////////////
////Process
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI20400/Save',
            params: {
                lstSI_MaterialType: HQ.store.getData(App.stoSI_MaterialType)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSI_MaterialType.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                refresh('yes');
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSI_MaterialType.deleteSelected();
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
        App.stoSI_MaterialType.reload();
    }
};
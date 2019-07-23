//// Declare //////////////////////////////////////////////////////////
var keys = ['StandID'];
var fieldsCheckRequire = ["StandID", "Descr"];
var fieldsLangCheckRequire = ["StandID", "Descr"];

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
        App.stoSI_Stand.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_Stand);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_Stand);
            break;
        case "next":
            HQ.grid.next(App.grdSI_Stand);
            break;
        case "last":
            HQ.grid.last(App.grdSI_Stand);
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
                HQ.grid.insert(App.grdSI_Stand, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSI_Stand.selected.items[0] != undefined) {
                    if (App.slmSI_Stand.selected.items[0].data.StandID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSI_Stand)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_Stand, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    
    checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSI_Stand);
    HQ.common.changeData(HQ.isChange, 'SI23800');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoSI_Stand_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
 
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoSI_Stand, keys, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }            
     }
        HQ.isFirstLoad = false; //sto load cuoi se su dung

    //Sto tiep theo
    frmChange();
};

var grdSI_Stand_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSI_Stand_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSI_Stand, e, keys);
    frmChange();
};

var grdSI_Stand_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_Stand, e, keys);
};

var grdSI_Stand_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_Stand);
    frmChange();
};

////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI23800/Save',
            params: {
                lstSI_Stand: HQ.store.getData(App.stoSI_Stand)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSI_Stand.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSI_Stand.deleteSelected();
        frmChange();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_Stand.reload();
    }
};
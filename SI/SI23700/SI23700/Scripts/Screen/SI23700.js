//// Declare //////////////////////////////////////////////////////////
var keys = ['DisplayID'];
var fieldsCheckRequire = ["DisplayID", "Descr"];
var fieldsLangCheckRequire = ["DisplayID", "Descr"];

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
        App.stoSI_Display.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_Display);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_Display);
            break;
        case "next":
            HQ.grid.next(App.grdSI_Display);
            break;
        case "last":
            HQ.grid.last(App.grdSI_Display);
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
                HQ.grid.insert(App.grdSI_Display, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSI_Display.selected.items[0] != undefined) {
                    if (App.slmSI_Display.selected.items[0].data.DisplayID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSI_Display)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_Display, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    HQ.isChange = HQ.store.isChange(App.stoSI_Display);
    HQ.common.changeData(HQ.isChange, 'SI23700');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoSI_Display_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
 
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoSI_Display, keys, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }            
     }
        HQ.isFirstLoad = false; //sto load cuoi se su dung

    //Sto tiep theo
    frmChange();
};

var grdSI_Display_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSI_Display_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSI_Display, e, keys);
    frmChange();
};

var grdSI_Display_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_Display, e, keys);
};

var grdSI_Display_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_Display);
    frmChange();
};

////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI23700/Save',
            params: {
                lstSI_Display: HQ.store.getData(App.stoSI_Display)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSI_Display.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSI_Display.deleteSelected();
        frmChange();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_Display.reload();
    }
};
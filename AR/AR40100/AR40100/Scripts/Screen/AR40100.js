//// Declare //////////////////////////////////////////////////////////
var keys = ['RoleID'];
var fieldsCheckRequire = ["RoleID","Desc"];
var fieldsLangCheckRequire = ["RoleID","Desc"];

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
        App.stoData.reload();
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
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoData.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdData, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmData.selected.items[0] != undefined) {
                    if (App.slmData.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdData)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoData, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoData);
    HQ.common.changeData(HQ.isChange, 'AR40100');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoData_Load = function (sto) {
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

var grdData_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdData);
    frmChange();
};
var ColCheck_Header_Change = function (value) {
    if (value) {
        var allData = App.stoData.snapshot || App.stoData.allData || App.stoData.data;
        App.stoData.suspendEvents();
        allData.each(function (item) {
            item.set("Selected", value.checked);
        });
        App.stoData.resumeEvents();
        App.grdData.view.refresh();
    }
};
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR40100/Save',
            params: {
                lstData: HQ.store.getAllData(App.stoData, ["Selected"], [true])
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                HQ.isFirstLoad = true;
                App.stoData.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                if (this.result.parm && this.result.parm[1] != '') {
                    var lstBranchBatNbr = this.result.parm[1].split(',');
                    for (var i = 0; i < lstBranchBatNbr.length; i++) {
                        var BranchBatNbr = lstBranchBatNbr[i].split('#');
                        var record = HQ.store.findRecord(App.stoData, ['BranchID','BatNbr'], [BranchBatNbr[0], BranchBatNbr[1]]);
                        if (record) {
                            App.stoData.remove(record);
                        }                       
                    }
                }
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdData.deleteSelected();
        frmChange();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();
    }
};
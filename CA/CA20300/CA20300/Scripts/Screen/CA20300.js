//// Declare //////////////////////////////////////////////////////////

var keys = ['TypeID'];
///////////////////////////////////////////////////////////////////////

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
        App.stoCostType.reload();
        HQ.common.showBusy(false);
    }
};

//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdCostType);
            break;
        case "prev":
            HQ.grid.prev(App.grdCostType);
            break;
        case "next":
            HQ.grid.next(App.grdCostType);
            break;
        case "last":
            HQ.grid.last(App.grdCostType);
            break;
        case "refresh":
           if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoCostType.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdCostType);
            }
            break;
        case "delete":
            if (App.slmCostType.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmCostType.selected.items[0] != undefined) {
                        if (App.slmCostType.selected.items[0].data.TypeID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdCostType)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoCostType.getChangedData().Created) && checkRequire(App.stoCostType.getChangedData().Updated)) {
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
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
   // HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    checkLoad(); // Mới
};


var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoCostType);
    HQ.common.changeData(HQ.isChange, 'CA20300');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    stoLoad
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoCostType.reload();
    }
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto,keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};
var grdCostType_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};
var grdCostType_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdCostType, e, keys);
    frmChange();
};
var grdCostType_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdCostType, e, keys);
};

var grd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCostType);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////



//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'CA20300/Save',
            params: {
                lstCostType:HQ.store.getData(App.stoCostType)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdCostType.deleteSelected();
        frmChange();
    }
};
//kiem tra key da nhap du chua
var isAllValidKey = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var j = 0; j < keys.length; j++) {
                if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
                    return false;
            }
        }
        return true;
    } else {
        return true;
    }
};
//kiem tra nhung field yeu cau bat buoc nhap
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i]["TypeID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("TypeID"));
                return false;
            }
            if (items[i]["Descr"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Descr"));
                return false;
            }

        }
        return true;
    } else {
        return true;
    }
};
/////////////////////////////////////////////////////////////////////////



//// Other Functions ////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////









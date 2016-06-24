//// Declare //////////////////////////////////////////////////////////

var keys = ['CostID'];
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
        App.stoCostCode.reload();
        HQ.common.showBusy(false);
    }
};

//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdCostCode);
            break;
        case "prev":
            HQ.grid.prev(App.grdCostCode);
            break;
        case "next":
            HQ.grid.next(App.grdCostCode);
            break;
        case "last":
            HQ.grid.last(App.grdCostCode);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoCostCode.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdCostCode);
            }
            break;
        case "delete":
            if (App.slmCostCode.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmCostCode.selected.items[0] != undefined) {
                        if (App.slmCostCode.selected.items[0].data.CostID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdCostCode)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoCostCode.getChangedData().Created) && checkRequire(App.stoCostCode.getChangedData().Updated)) {
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
    HQ.isChange = HQ.store.isChange(App.stoCostCode);
    HQ.common.changeData(HQ.isChange, 'CA20400');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    stoLoad
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoCostCode.reload();
    }
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData' ));
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
var grdCostCode_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdCostCode_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdCostCode, e, keys);
    frmChange();
};
var grdCostCode_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdCostCode, e, keys);
};

var grd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCostCode);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////



//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'CA20400/Save',
            params: {
                lstCostCode: HQ.store.getData(App.stoCostCode)
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
        App.grdCostCode.deleteSelected();
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
            if (items[i]["CostID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("CostID"));
                return false;
            }
            if (items[i]["Descr"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Descr"));
                return false;
            }
            if (items[i]["Type"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Type"));
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









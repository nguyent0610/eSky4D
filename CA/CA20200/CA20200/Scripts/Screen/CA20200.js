//// Declare //////////////////////////////////////////////////////////

var keys = ['BranchID', 'BankAcct'];
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
        App.stoAccount.reload();
        HQ.common.showBusy(false);
    }
};

//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdAccount);
            break;
        case "prev":
            HQ.grid.prev(App.grdAccount);
            break;
        case "next":
            HQ.grid.next(App.grdAccount);
            break;
        case "last":
            HQ.grid.last(App.grdAccount);
            break;
        case "refresh":
            App.stoAccount.reload();
            HQ.grid.first(App.grdAccount);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdAccount);
            }
            break;
        case "delete":
            if (App.slmAccount.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmAccount.selected.items[0] != undefined) {
                        if (App.slmAccount.selected.items[0].data.BranchID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAccount)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoAccount.getChangedData().Created) && checkRequire(App.stoAccount.getChangedData().Updated)) {
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
    checkLoad(); // Mới
};


var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoAccount);
    HQ.common.changeData(HQ.isChange, 'CA20200');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    stoLoad
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoAccount.reload();
    }
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
var grdAccount_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdAccount_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdAccount, e, keys);
    frmChange();
};
var grdAccount_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAccount, e, keys);
};

var grdAccount_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};

var grd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAccount);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////



//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'CA20200/Save',
            params: {
                lstAccount: HQ.store.getData(App.stoAccount)
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
        App.grdAccount.deleteSelected();
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
            if (HQ.grid.checkRequirePass(items[i])) continue;
            if (items[i]["BranchID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("BranchID"));
                return false;
            }

            if (items[i]["BankAcct"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("BankAcct"));
                return false;
            }
            if (items[i]["AcctName"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("AcctName"));
                return false;
            }
            if (items[i]["AcctNbr"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("AcctNbr"));
                return false;
            }
            if (items[i]["AddrID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("AddrID"));
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

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};

var ActiveCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdAccount.getStore().each(function (item) {

            item.set("Active", value.checked);
        });
    }
};
/////////////////////////////////////////////////////////////////////////









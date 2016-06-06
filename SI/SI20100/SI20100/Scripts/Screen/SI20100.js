//// Declare //////////////////////////////////////////////////////////
var keys = ['Buyer'];
var fieldsCheckRequire = ["Buyer"];
var fieldsLangCheckRequire = ["Buyer"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


///////////////////////////////////////////////////////////////////////4
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoIN_Buyer.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdIN_Buyer);
            break;
        case "prev":
            HQ.grid.prev(App.grdIN_Buyer);
            break;
        case "next":
            HQ.grid.next(App.grdIN_Buyer);
            break;
        case "last":
            HQ.grid.last(App.grdIN_Buyer);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoIN_Buyer.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdIN_Buyer, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmIN_Buyer.selected.items[0] != undefined) {
                    if (App.slmIN_Buyer.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdIN_Buyer)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoIN_Buyer, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoIN_Buyer);
    HQ.common.changeData(HQ.isChange, 'SI20100');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
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

var grdIN_Buyer_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdIN_Buyer_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdIN_Buyer, e, keys);
    frmChange();
};

var grdIN_Buyer_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdIN_Buyer, e, keys);
};

var grdIN_Buyer_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN_Buyer);
    frmChange();
};

////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI20100/Save',
            params: {
                lstIN_Buyer: HQ.store.getData(App.stoIN_Buyer)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoIN_Buyer.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdIN_Buyer.deleteSelected();
        frmChange();
    }
};

//Other function

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoIN_Buyer.reload();
    }
};
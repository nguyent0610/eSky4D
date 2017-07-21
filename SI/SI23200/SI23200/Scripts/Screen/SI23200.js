//// Declare //////////////////////////////////////////////////////////
var keys = ['Code'];
var fieldsCheckRequire = ["Territory", "Code"]
var fieldsLangCheckRequire = ["Territory", "Code"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
/////////////////////Store////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoArea.reload();
        HQ.common.showBusy(false);
    }
};
////////////////Event///////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdArea);
            break;
        case "prev":
            HQ.grid.prev(App.grdArea);
            break;
        case "next":
            HQ.grid.next(App.grdArea);
            break;
        case "last":
            HQ.grid.last(App.grdArea);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoArea.reload();
            }
            break;
           
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdArea, keys);
            }
            break;
        case "delete":
            if (App.slmArea.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmArea.selected.items[0] != undefined) {
                        if (App.slmArea.selected.items[0].data.Area != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdArea)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoArea, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    //HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    //HQ.isFirstLoad = true;
    //App.frmMain.isValid();
    //checkLoad(); // Mới
    HQ.isFirstLoad = true;
    if (HQ.isInsert == false) {
        App.menuClickbtnNew.disable();
    }
    if (HQ.isDelete == false) {
        App.menuClickbtnDelete.disable();
    }
    if (HQ.isUpdate == false && HQ.isInsert == false && HQ.isDelete == false) {
        App.menuClickbtnSave.disable();
    }
    App.cboTerritory.getStore().addListener('load', checkLoad);
    //App.stoArea.reload();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoArea);
    HQ.common.changeData(HQ.isChange, 'SI23200');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoArea.reload();
    }
};


//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var grdArea_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdArea_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdArea, e, keys);
};

var grdArea_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdArea, e, keys);
};

var grdArea_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdArea);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
//var save = function () {
//    if (App.frmMain.isValid()) {
//        App.frmMain.submit({
//            timeout: 1800000,
//            waitMsg: HQ.common.getLang("SavingData"),
//            url: 'SI23200/Save',
//            params: {
//                lstAR_Area: HQ.store.getData(App.stoArea)
//            },
//            success: function (msg, data) {
//                HQ.message.show(201405071);
//                HQ.isChange = false;
//                refresh("yes");
//            },
//            failure: function (msg, data) {
//                HQ.message.process(msg, data, true);
//            }
//        });
//    }
//};
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI23200/Save',
            params: {
                lstSI_SubTerritory: HQ.store.getData(App.stoArea)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
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
        App.grdArea.deleteSelected();
        frmChange();
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoArea.reload();
    }
};
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i][Area].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Area"));
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

///////////////////////////////////
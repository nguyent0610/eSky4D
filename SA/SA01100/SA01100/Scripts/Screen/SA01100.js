//// Declare //////////////////////////////////////////////////////////
var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Type", "Msg00", "Msg01"];
var fieldsLangCheckRequire = ["Code", "Type", "Msg00", "Msg01"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_Message.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    checkLoad();
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Message);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Message);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Message);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Message);
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
                HQ.grid.insert(App.grdSYS_Message, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSYS_Message.selected.items[0] != undefined) {
                    if (App.slmSYS_Message.selected.items[0].data.Code != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_Message)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequirePass(App.stoSYS_Message, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var checkRequirePass = function (store, keys, fieldsCheck, fieldsLang) {
    items = store.getChangedData().Created;
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var jkey = 0; jkey < keys.length; jkey++) {
                if (items[i][keys[jkey]]) {
                    for (var k = 0; k < fieldsCheck.length; k++) {
                        if (fieldsCheck[k] == 'Type' && items[i].Type.toString().trim() == "0") {
                            HQ.message.show(15, HQ.common.getLang(fieldsLang == undefined ? fieldsCheck[k] : fieldsLang[k]));
                            return false;
                        }
                        if (HQ.util.passNull(items[i][fieldsCheck[k]]).toString().trim() == "") {
                            HQ.message.show(15, HQ.common.getLang(fieldsLang == undefined ? fieldsCheck[k] : fieldsLang[k]));
                            return false;
                        }
                    }
                    break; // Check data one time
                }
            }
        }
    }

    items = store.getChangedData().Updated;
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var jkey = 0; jkey < keys.length; jkey++) {
                if (items[i][keys[jkey]]) {
                    for (var k = 0; k < fieldsCheck.length; k++) {
                        if (fieldsCheck[k] == 'Type' && items[i].Type.toString().trim() == "0") {
                            HQ.message.show(15, HQ.common.getLang(fieldsLang == undefined ? fieldsCheck[k] : fieldsLang[k]));
                            return false;
                        }
                        if (HQ.util.passNull(items[i][fieldsCheck[k]]).toString().trim() == "") {
                            HQ.message.show(15, HQ.common.getLang(fieldsLang == undefined ? fieldsCheck[k] : fieldsLang[k]));
                            return false;
                        }
                    }
                    break; // Check data one time
                }
            }
        }
    }
    return true;
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA01100');
};

var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA01100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};


//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var grdSYS_Message_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSYS_Message_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Message, e, keys);
};

var grdSYS_Message_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Message, e, keys);
};

var grdSYS_Message_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Message);
    stoChanged(App.stoSYS_Message);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA01100/Save',
            params: {
                lstSYS_Message: HQ.store.getData(App.stoSYS_Message)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSYS_Message.deleteSelected();
        stoChanged(App.stoSYS_Message);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Message.reload();
    }
};
////////////////////////////////////////////////////////////////////////
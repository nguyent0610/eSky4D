//// Declare //////////////////////////////////////////////////////////


var fieldsCheckRequire = ["Code", "Descr"];
var keys = ['Code'];
var fieldsLangCheckRequire = ["Code", "Descr"];

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
        App.stoShopType.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdShopType);
            break;
        case "prev":
            HQ.grid.prev(App.grdShopType);
            break;
        case "next":
            HQ.grid.next(App.grdShopType);
            break;
        case "last":
            HQ.grid.last(App.grdShopType);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoShopType.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdShopType);
            }
            break;
        case "delete":
            if (App.slmShopType.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmShopType.selected.items[0] != undefined) {
                        if (App.slmShopType.selected.items[0].data.Code != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdShopType)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoShopType, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
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
    HQ.isChange = HQ.store.isChange(App.stoShopType);
    HQ.common.changeData(HQ.isChange, 'AR21000');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {stoLoad
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoShopType.reload();
    }
};
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



var grdShopType_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdShopType_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdShopType, e, keys);
};
var grdShopType_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdShopType, e, keys);
};

var grd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdShopType);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////



//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR21000/Save',
            params: {
                lstShopType: HQ.store.getData(App.stoShopType)
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
        App.grdShopType.deleteSelected();
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
}
//kiem tra nhung field yeu cau bat buoc nhap
var checkRequire = function (items) {
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

var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i]["Code"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Code"));
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

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
/////////////////////////////////////////////////////////////////////////



//// Other Functions ////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////









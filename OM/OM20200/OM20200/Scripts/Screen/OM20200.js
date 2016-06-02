//// Declare //////////////////////////////////////////////////////////

var keys = ['PriceClassID'];
var fieldsCheckRequire = ["PriceClassID", "Descr"];
var fieldsLangCheckRequire = ["PriceClassID", "Descr"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
///////////////////////////////////////////////////////////////////////

// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_PriceClass.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_PriceClass);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_PriceClass);
            break;
        case "next":
            HQ.grid.next(App.grdOM_PriceClass);
            break;
        case "last":
            HQ.grid.last(App.grdOM_PriceClass);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_PriceClass.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_PriceClass);
            }
            break;
        case "delete":
            if (App.slmOM_PriceClass.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmOM_PriceClass.selected.items[0] != undefined) {
                        if (App.slmOM_PriceClass.selected.items[0].data.PriceClassID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_PriceClass)], 'deleteData', true);
                        }
                    }
                }
            }
            break;

        case "save":

            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_PriceClass, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    HQ.isChange = HQ.store.isChange(App.stoOM_PriceClass);
    HQ.common.changeData(HQ.isChange, 'OM20200');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_PriceClass.reload();
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
var grdOM_PriceClass_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};
var grdOM_PriceClass_Edit = function (item, e) {

    HQ.grid.checkInsertKey(App.grdOM_PriceClass, e, keys);
    frmChange();
};
var grdOM_PriceClass_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_PriceClass, e, keys);
};

var grdOM_PriceClass_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_PriceClass);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////



//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM20200/Save',
            params: {
                lstOM_PriceClass: HQ.store.getData(App.stoOM_PriceClass)
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
        App.grdOM_PriceClass.deleteSelected();
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
            if (items[i]["PriceClassID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("PriceClassID"));
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

/////////////////////////////////////////////////////////////////////////









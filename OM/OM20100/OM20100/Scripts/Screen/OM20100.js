//// Declare //////////////////////////////////////////////////////////
var keys = ['PriceClassID'];
var fieldsCheckRequire = ["PriceClassID"];
var fieldsLangCheckRequire = ["PriceClassID"];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
/////////////////////Store///////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_PriceClass.reload();
        HQ.common.showBusy(false);
    }
};
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
                HQ.grid.insert(App.grdOM_PriceClass, keys);
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

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad(); // Mới
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoOM_PriceClass);
    HQ.common.changeData(HQ.isChange, 'OM20100');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_PriceClass.reload();
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

var grdOM_PriceClass_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdOM_PriceClass_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_PriceClass, e, keys);
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
            url: 'OM20100/Save',
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

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////

var checkRequire = function (items)
{
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i][OM_PriceClass].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("PriceClassID"));
                return false;
            }
            if (items[i]["PriceClassTypes"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("PriceClassType"));
                return false;
            }

        }
        return true;
    } else {
        return true;
    }
}

///////////////////////////////////
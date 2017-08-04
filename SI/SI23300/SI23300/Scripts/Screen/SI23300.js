//// Declare //////////////////////////////////////////////////////////
var keys = ["Country", "State", "District", "Ward"];
var fieldsCheckRequire = ["Country", "State", "District", "Ward", "Name"]
var fieldsLangCheckRequire = ["Country", "State", "District", "Ward", "Name"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
/////////////////////Store////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoWard.reload();
        HQ.common.showBusy(false);
    }
};
////////////////Event///////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdWard);
            break;
        case "prev":
            HQ.grid.prev(App.grdWard);
            break;
        case "next":
            HQ.grid.next(App.grdWard);
            break;
        case "last":
            HQ.grid.last(App.grdWard);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoWard.reload();
            }
            break;

        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdWard, keys);
            }
            break;
        case "delete":
            if (App.slmArea.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmArea.selected.items[0] != undefined) {
                        if (App.slmArea.selected.items[0].data.Country != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdWard)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoWard, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    HQ.isFirstLoad = true;
    App.cboCountry.getStore().addListener('load', checkLoad);
    //App.cboState.getStore().addListener('load', checkLoad);
    //App.cboDistrict.getStore().addListener('load', checkLoad);
    //App.stoArea.reload();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoWard);
    HQ.common.changeData(HQ.isChange, 'SI23300');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoWard.reload();
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
    if (e.field == 'State') {
        App.cboState.getStore().reload();
    }
    if (e.field == 'District') {
        App.cboDistrict.getStore().reload();
    }
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdArea_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdWard, e, keys);
    if (e.field == 'Ward' && e.value != '') {
        App.cboCountry.value = null;
    }
    frmChange();
};

var grdArea_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdWard, e, keys);
};

var grdArea_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdWard);
    frmChange();
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI23300/Save',
            params: {
                lstSI_Ward: HQ.store.getData(App.stoWard)
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
        App.grdWard.deleteSelected();
        frmChange();
    }
};

///////////////// Event Change Combo /
var cboCountry_change = function (sender, value) {
    App.cboState.setValue("");
    App.cboState.getStore().reload();
    //App.cboState.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord != undefined)
    //        if (curRecord.data.State) {
    //            App.cboState.setValue(curRecord.data.State);
    //        }
    //    var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
    //    if (!dt) {
    //        App.cboState.setValue("");
    //    }
    //});
}

var cboState_change = function (sender, value) {
    App.cboDistrict.setValue("");
    App.cboDistrict.getStore().reload();
    //App.cboDistrict.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord != undefined)
    //        if (curRecord.data.District) {
    //            App.cboDistrict.setValue(curRecord.data.District);
    //        }
    //    var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], [App.cboDistrict.getValue()]);
    //    if (!dt) {
    //        App.cboDistrict.setValue("");
    //    }
    //});
}
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoWard.reload();
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
//// Declare ///////////////////////////////////////////////////////////
var keys = ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'];
var fieldsCheckRequire = ["UnitType", "ClassID", "InvtID", "FromUnit", "ToUnit", "MultDiv", "CnvFact"];
var fieldsLangCheckRequire = ["UnitType", "ClassID", "InvtID", "FromUnit", "ToUnit", "MultDiv", "CnvFact"];
////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdUnitConversion);
            break;
        case "prev":
            HQ.grid.prev(App.grdUnitConversion);
            break;
        case "next":
            HQ.grid.next(App.grdUnitConversion);
            break;
        case "last":
            HQ.grid.last(App.grdUnitConversion);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoUnitConversion.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdUnitConversion, keys);
            }
            break;
        case "delete":
            if (App.slmUnitConversion.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdUnitConversion);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdUnitConversion), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoUnitConversion, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var UnitTypechange = function (value) {
    var record = App.stoUnitType.findRecord("UnitType", value);
    if (record) {
        return record.data.Code;
    }
    else {
        return value;
    }
};

var MultDivchange = function (value) {
    var record = App.stoMultDiv.findRecord("MultDiv", value);
    if (record) {
        return record.data.Code;
    }
    else {
        return value;
    }
};

var getDescr = function (value) {
    var record = App.cboInvtID.findRecord("InvtID", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoUnitConversion.reload(
        App.stoUnitType.reload(
            App.stoMultDiv.reload()
        )
    );
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20100');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var grdUnitConversion_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdUnitConversion_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdUnitConversion, e, keys);
};

var grdUnitConversion_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdUnitConversion, e, keys);
};

var grdUnitConversion_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdUnitConversion);
    stoChanged(App.stoUnitConversion);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IN20100/Save',
            params: {
                lstUnitConversion: HQ.store.getData(App.stoUnitConversion)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdUnitConversion.deleteSelected();
        stoChanged(App.stoUnitConversion);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoUnitConversion.reload();
    }
};
/////////////////////////////////////////////////////////////////////////
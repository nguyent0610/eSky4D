//// Declare ///////////////////////////////////////////////////////////



var keys = ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'];
var fieldsCheckRequire = ["UnitType", "ClassID", "InvtID", "FromUnit", "ToUnit", "MultDiv", "CnvFact"];
var fieldsLangCheckRequire = ["UnitType", "ClassID", "InvtID", "FromUnit", "ToUnit", "MultDiv", "CnvFact"];

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;

var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoUnitConversion.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboUnitType.getStore().load(function () {
        App.cboClassID.getStore().load(function () {
            App.cboInvtID.getStore().load(function () {
                App.cboMultDiv.getStore().load(function () {
                    HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
                    App.stoUnitConversion.reload();

                    var UnitType = [];
                    App.cboUnitType.getStore().data.each(function (item) {
                        if (UnitType.indexOf(item.data.Code) == -1) {
                            UnitType.push([item.data.UnitType, item.data.Code]);
                        }
                    });
                    filterFeature = App.grdUnitConversion.filters;
                    colAFilter = filterFeature.getFilter('UnitType');
                    colAFilter.menu = colAFilter.createMenu({
                        options: UnitType
                    });

                    var MultDiv = [];
                    App.cboMultDiv.getStore().data.each(function (item) {
                        if (MultDiv.indexOf(item.data.Code) == -1) {
                            MultDiv.push([item.data.MultDiv, item.data.Code]);
                        }
                    });

                    colAFilter = filterFeature.getFilter('MultDiv');
                    colAFilter.menu = colAFilter.createMenu({
                        options: MultDiv
                    });
                })
            })
        })
    })
};

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
            break;
    }
};

var UnitTypechange = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboUnitTypeIN20100_pcLoadUnitType.findRecord("UnitType", rec.data.UnitType);
    if (record) {
        return record.data.Code;
    }
    else {
        return value;
    }
};

var MultDivchange = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboMultDivIN20100_pcLoadMultDiv.findRecord("MultDiv", rec.data.MultDiv);
    if (record) {
        return record.data.Code;
    }
    else {
        return value;
    }
};

//var getDescr = function (value) {
//    var record = App.cboInvtID.findRecord("InvtID", value);
//    if (record) {
//        return record.data.Descr;
//    }
//    else {
//        return value;
//    }
//};

var getDescr = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboInvtID1IN20100_pcLoadInventory.findRecord("InvtID", rec.data.InvtID);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
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
    HQ.isChange = HQ.store.isChange(App.stoUnitConversion);
    HQ.common.changeData(HQ.isChange, 'IN20100');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoUnitConversion.reload();
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

var grdUnitConversion_BeforeEdit = function (editor, e) {
    if (e.field == 'Descr') {
        return false;
    }
    if (e.record.data.UnitType == '1') {
        if (e.field == 'InvtID' || e.field == 'ClassID')
            return false;
    }
    if (e.record.data.UnitType == '2') {
        if (e.field == 'InvtID')
            return false;
    }
    if (e.record.data.UnitType == '3') {
        if (e.field == 'ClassID')
            return false;
    }
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdUnitConversion_Edit = function (item, e) {
    if (e.field == 'UnitType') {
        if (e.value == '1') {
            e.record.set('ClassID', '*');
            e.record.set('InvtID', '*');
            e.record.set('Descr', '*');
            e.record.set('CnvFact', 1);
            App.cboFromUnit.forceSelection = false;
            App.cboToUnit.forceSelection = false;
        }
        if (e.value == '2') {
            e.record.set('InvtID', '*');
            e.record.set('Descr', '*');
            e.record.set('ClassID', '');
            e.record.set('CnvFact', 1);
            App.cboFromUnit.forceSelection = true;
            App.cboToUnit.forceSelection = true;
        }
        if (e.value == '3') {
            e.record.set('InvtID', '');
            e.record.set('Descr', '');
            e.record.set('ClassID', '*');
            e.record.set('CnvFact', 1);
            App.cboFromUnit.forceSelection = true;
            App.cboToUnit.forceSelection = true;
        }
    }
    HQ.grid.checkInsertKey(App.grdUnitConversion, e, keys);

};

var grdUnitConversion_ValidateEdit = function (item, e) {
    if (e.field === 'InvtID' || e.field === 'ClassID' || ((e.field === 'FromUnit' || e.field === 'ToUnit') && HQ.isNvarchar)) {
        return HQ.grid.checkValidateEditDG(App.grdUnitConversion, e, keys);
    } else {
        return HQ.grid.checkValidateEdit(App.grdUnitConversion, e, keys, true);
    }
    
};

var grdUnitConversion_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdUnitConversion);
    frmChange();
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
                App.cboFromUnit.store.reload();
                App.cboToUnit.store.reload();
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
        App.grdUnitConversion.deleteSelected();
        frmChange();
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
//=============== Declare ===============
var keys = ["State"];
var _District = "";
var fieldsCheckRequire = ["Country", "State", "District", "Name"];
var fieldsLangCheckRequire = ["SI21700_Country", "SI21700_State", "SI21700_District", "SI21700_Name"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var keys1 = ['Country', 'State'];
var keys2 = ["State", "District"];
var keyth = ['Country', 'State', 'District'];

//=============== checkLoad ==============
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSI_District.reload();
        HQ.common.showBusy(false);
     
    }
};

//=============== menuClick ==============
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_District);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_District);
            break;
        case "next":
            HQ.grid.next(App.grdSI_District);
            break;
        case "last":
            HQ.grid.last(App.grdSI_District);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSI_District.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_District, ['State']);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSI_District.selected.items[0] != undefined) {
                    if (App.slmSI_District.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSI_District)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_District, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

//=============== refresh ================
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_District.reload();
    }
};

//=============== firstLoad ==============
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboCountry.getStore().addListener('load', checkLoad);
    
    if (HQ.countryView == false) {

        keys = ["State", "District"];
        fieldsCheckRequire = ["State", "District", "Name"];
        fieldsLangCheckRequire = ["SI21700_State", "SI21700_District", "SI21700_Name"];
        App.Country.hide();
    }
    else {
        keys = ['Country', 'State'];
        fieldsCheckRequire = ["Country", "State", "District", "Name"];
        fieldsLangCheckRequire = ["SI21700_Country", "SI21700_State", "SI21700_District", "SI21700_Name"];
        App.Country.show();
    }
    checkLoad();
    HQ.store.insertBlank(App.stoSI_District, ['State']);
};

//=============== frmChange ==============
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSI_District);
    HQ.common.changeData(HQ.isChange, 'SI21700');
};

//=============== stoBeforeLoad ==========
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

//=============== stoSI_District =========
var stoSI_District_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));

    if (HQ.isInsert) {
        var obj = HQ.store.findInStore(App.stoSI_District, keyth, ['', '', '']);
        if(!obj)
            HQ.store.insertBlank(App.stoSI_District, keyth);
    }
    frmChange();
    //Sto tiep theo
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

//=============== Grid Event =============
// Grid Select
var grdSI_District_Select = function (slm, selRec, idx, eOpts)
{
    _District = selRec.data.District;
}
// Grid BeforeEdit
var grdSI_District_BeforeEdit = function (editor, e) {
    if (e.field == "District")
    {
        if(e.value != "" || e.record.data.Country == "" || e.record.data.State == "")
        {
            return false;
        }
    }
    if (e.field == "State")
    {
        if(e.value != "" || e.record.data.Country == "")
        {
            return false;
        }
    }
    if (!HQ.grid.checkBeforeEdit(e, keyth)) return false;
};

// Grid StateStateClassDetail Edit
var grdStateClassDetail_Edit = function (item, e) {

    if (e.field == "State") {
        var a = HQ.store.findRecord(App.cboState.store, ['State'], [e.value]);
        if (e.value != "" && e.value != null) {
            e.record.set('DescrState', a.data.DescrState);
        }
        else {
            e.record.set('DescrState', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdSI_District, e, keyth);
};
var grdSI_District_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSI_District, e, keyth);
};
// Grid Reject
var grdSI_District_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSI_District);
};

//=============== checkValidate ==========
var checkValidateEditN = function (grd, e, keys, isCheckSpecialChar) {
    if (keys.indexOf(e.field) != -1) {
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (isCheckSpecialChar == undefined) isCheckSpecialChar = true;
        if (isCheckSpecialChar) {
            if (e.value)
                if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
                    HQ.message.show(2018120350, e.column.text);
                    return false;
                }
        }
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            if (e.column.xtype == "datecolumn")
                HQ.message.show(1112, Ext.Date.format(e.value, e.column.format));
            else HQ.message.show(1112, e.value);
            return false;
        }

    }
}

//=============== firstLoad ==============
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21700/Save',
            params: {
                lstSI_District: HQ.store.getData(App.stoSI_District)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSI_District.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        var indexcolum = '';
        var check = '';
        var lstCountry = '';
        var lstState = '';
        var lstDistrict = '';
        var District = '';
        var lstSelete = App.grdSI_District.selModel.selected;
        for (var i = 0; i < lstSelete.length; i++) {
            indexcolum = indexcolum + (lstSelete.items[i].index + 1) + ",";
            lstCountry = check + lstSelete.items[i].data.Country + ",";
            lstState = check + lstSelete.items[i].data.State + ",";
            lstDistrict = check + lstSelete.items[i].data.District + ",";
            District = check + lstSelete.items[i].data.District;
        }
        checkDeleteData(indexcolum, lstCountry, lstState, lstDistrict, District);
    }
};

//=============== checkDeleteData ========
var checkDeleteData = function (lstIndexColum, lstCountry, lstState, lstDistrict, District) {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI21700/CheckDelete',
            params: {
                lstIndexColum: lstIndexColum,
                lstCountry: lstCountry,
                lstState: lstState,
                lstDistrict: lstDistrict,
                District : District
            },
            success: function (msg, data) {
                App.grdSI_District.deleteSelected();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

//=============== Button Import ============
var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'SI21700/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
                App.stoSI_District.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701', '', '');
        sender.reset();
    }
};

//=============== Button Export =============
var btnExport_Click = function () {
    App.frmMain.submit({
        url: 'SI21700/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {

        },
        success: function (msg, data) {
            alert('sus');
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};

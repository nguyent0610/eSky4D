////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 10;
var _isLoadMaster = false;

////////////////////////////////////////////////////////////////////////
//// Store ////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoAR_CustClass.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    //if (HQ.isInsert == false && HQ.isDelete == false && HQ.isUpdate == false)
    //    App.menuClickbtnSave.disable();
    HQ.util.checkAccessRight();
    App.frmMain.isValid(); //Require các field yêu cầu trên man hình
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isFirstLoad = true;
    App.cboClassId.getStore().addListener('load', checkLoad);
    App.cboTerritory.getStore().addListener('load', checkLoad);
    App.cboCountry.getStore().addListener('load', checkLoad);
    App.cboPriceClass.getStore().addListener('load', checkLoad);
    App.cboTerms.getStore().addListener('load', checkLoad);
    App.cboTaxDflt.getStore().addListener('load', checkLoad);
    App.cboTaxID00.getStore().addListener('load', checkLoad);
    App.cboTaxID01.getStore().addListener('load', checkLoad);
    App.cboTaxID02.getStore().addListener('load', checkLoad);
    App.cboTaxID03.getStore().addListener('load', checkLoad);
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboClassId, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboClassId, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboClassId, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboClassId, HQ.isChange);
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    save();
                }
            }
            break;

        case "close":
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    App.cboShipToId.setValue('');
                }
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                var curRecord = App.frmMain.getRecord();
                if (curRecord) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;

        default:
    }
};

var frmChange = function () {
    if (App.stoAR_CustClass.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoAR_CustClass);
    HQ.common.changeData(HQ.isChange, 'AR20100');
    if (App.cboClassId.valueModels == null || HQ.isNew == true)
        App.cboClassId.setReadOnly(false);
    else App.cboClassId.setReadOnly(HQ.isChange);

  
};

var stoLoad = function (sto) {

    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    App.cboCountry.forceSelection = false;
    App.cboState.forceSelection = false;
    App.cboDistrict.forceSelection = false;
    App.cboCity.forceSelection = false;
    App.cboClassId.forceSelection = false;
    App.cboCountry.store.clearFilter();
    App.cboState.store.clearFilter();

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "CustId,ShipToId");
        record = sto.getAt(0);
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require 
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    if (!HQ.isInsert && HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    if (App.cboCountry.value == record.data.Country) {
        cboCountry_Changed(App.cboCountry, record.data.Country);
    }
    else if (App.cboState.value == record.data.State) {
        cboState_Changed(App.cboState, record.data.State);
    }

    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
};


var cboClassId_Changed = function () {
    App.stoAR_CustClass.reload();
};

var cboCountry_Changed = function (sender, newValue, oldValue) {
    App.cboState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.State) {
                App.cboState.setValue(curRecord.data.State);
            }
        var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
        if (!dt) {
            curRecord.data.State = '';
            App.cboState.setValue("");
        }
        if (App.cboState.value == curRecord.data.State) {
            cboState_Changed(App.cboState, curRecord.data.State);
        }
    });
};

var cboState_Changed = function () {
    App.cboCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord && curRecord.data.City) {
            App.cboCity.setValue(curRecord.data.City);
        }
        var dt = HQ.store.findInStore(App.cboCity.getStore(), ["City"], [App.cboCity.getValue()]);
        if (!dt) {
            curRecord.data.City = '';
            App.cboCity.setValue("");
        }

        App.cboDistrict.getStore().load(function () {
            var curRecord = App.frmMain.getRecord();
            if (curRecord && curRecord.data.District) {
                App.cboDistrict.setValue(curRecord.data.District);
            }
            var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], [App.cboDistrict.getValue()]);
            if (!dt) {
                curRecord.data.District = '';
                App.cboDistrict.setValue("");
            }
        });
    });
};


//Truoc khi load store se hien Busy
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        if (HQ.util.checkSpecialChar(App.cboClassId.getValue()) == true) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("WaitMsg"),
                url: 'AR20100/Save',
                params: {
                    lstAR_CustClass: Ext.encode(App.stoAR_CustClass.getRecordsValues())
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    var ClassId = data.result.ClassId;
                    App.cboClassId.getStore().load(function () {
                        App.cboClassId.setValue(ClassId);
                        App.stoAR_CustClass.reload();
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
        else {
            HQ.message.show(2015123111, App.cboClassId.fieldLabel);
            App.cboClassId.focus();
            App.cboClassId.selectText();
        }

    }
};

function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR20100/DeleteAll',
            timeout: 7200,
            success: function (action, data) {
                //App.cboClassId.setValue("");
                App.cboClassId.getStore().load(function () { cboClassId_Changed(App.cboClassId); });
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};





/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        //if (HQ.isNew)
            //App.cboClassId.setValue('');
            HQ.isChange = false;
        App.stoAR_CustClass.reload();
    }
};
///////////////////////////////////
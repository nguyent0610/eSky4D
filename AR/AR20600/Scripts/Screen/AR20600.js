////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 8;
var _isLoadMaster = false;

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSOAddress.reload();
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
    App.cboCustId.getStore().addListener('load', checkLoad);
    App.cboCountry.getStore().addListener('load', checkLoad);
    App.cboSiteId.getStore().addListener('load', checkLoad);
    App.cboShipViaID.getStore().addListener('load', checkLoad);
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
            HQ.combo.first(App.cboShipToId, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboShipToId, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboShipToId, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboShipToId, HQ.isChange);
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
    if (App.stoSOAddress.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoSOAddress);
    HQ.common.changeData(HQ.isChange, 'AR20600');
    if (App.cboCustId.valueModels == null || HQ.isNew == true)
        App.cboCustId.setReadOnly(false);
    else App.cboCustId.setReadOnly(HQ.isChange);

    if (App.cboShipToId.valueModels == null || HQ.isNew == true)
        App.cboShipToId.setReadOnly(false);
    else App.cboShipToId.setReadOnly(HQ.isChange);
};

var stoLoad = function (sto) {

    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    App.cboShipToId.forceSelection = false;
    //App.cboCustId.forceSelection = false;
    App.cboCountry.forceSelection = false;
    App.cboState.forceSelection = false;
    App.cboDistrict.forceSelection = false;
    App.cboCity.forceSelection = false;
    App.cboSiteId.forceSelection = false;
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

var cboShipToId_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboShipToId.setValue('');
    }
};


var cboCustID_Changed = function (sender, e) {
    App.cboShipToId.setValue("");
    App.cboShipToId.getStore().load();
};

var cboShipToId_Changed = function (sender, e) {
    App.stoSOAddress.reload();
};

var cboCountry_Changed = function (sender, e) {
    App.cboState.getStore().load();
};

var cboState_Changed = function (sender, e) {
    App.cboCity.getStore().load();
    App.cboDistrict.getStore().load();
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
        if (HQ.util.checkSpecialChar(App.cboShipToId.getValue()) == true) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("WaitMsg"),
                url: 'AR20600/Save',
                params: {
                    lstAR_SOAddress: Ext.encode(App.stoSOAddress.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    var ShipToId = data.result.ShipToId;
                    App.cboShipToId.getStore().load(function () {
                        App.cboShipToId.setValue(ShipToId);
                        App.stoSOAddress.reload();
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
        else {
            HQ.message.show(20140811, App.cboShipToId.fieldLabel);
            App.cboShipToId.focus();
            App.cboShipToId.selectText();
        }
        
    }
};

function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR20600/DeleteAll',
            timeout: 7200,
            success: function (action, data) {
                //App.cboShipToId.setValue("");
                App.cboShipToId.getStore().load(function () { cboShipToId_Changed(App.cboShipToId); });
                menuClick("new");
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
        if (HQ.isNew)
        //App.cboVendID.setValue('');
        HQ.isChange = false;
        App.stoSOAddress.reload();
    }
};
///////////////////////////////////
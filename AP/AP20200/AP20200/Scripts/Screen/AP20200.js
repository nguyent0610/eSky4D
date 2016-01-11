////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 12;
var _isLoadMaster = false;
var VendID = '';

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoVendor.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    HQ.util.checkAccessRight(); // Kiem tra quyen Insert Update Delete de disable button tren top bar
    HQ.isFirstLoad = true;
    App.frmMain.isValid(); // Require cac field yeu cau tren from

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboVendID.getStore().addListener('load', checkLoad);
    App.cboCountry.getStore().addListener('load', checkLoad);
    App.cboBillCountry.getStore().addListener('load', checkLoad);
    App.cboClassID.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboTermsID.getStore().addListener('load', checkLoad);
    App.cboTaxDflt.getStore().addListener('load', checkLoad);
    App.cboTaxId00.getStore().addListener('load', checkLoad);
    App.cboTaxId01.getStore().addListener('load', checkLoad);
    App.cboTaxId02.getStore().addListener('load', checkLoad);
    App.cboTaxId03.getStore().addListener('load', checkLoad);
    App.cboMOQType.getStore().addListener('load', checkLoad);
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboVendID, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboVendID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboVendID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboVendID, HQ.isChange);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', 'refresh');
                } else {
                    App.cboVendID.setValue('');
                    App.stoVendor.reload();
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboVendID.getValue()) {
                    HQ.message.show(11, '', 'deleteData');
                } else {
                    menuClick('new');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
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

var frmChange = function () {
    if (App.stoVendor.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoVendor);
    HQ.common.changeData(HQ.isChange, 'AP20200');

    if (App.cboVendID.valueModels == null || HQ.isNew == true) 
        App.cboVendID.setReadOnly(false);
    else 
        App.cboVendID.setReadOnly(HQ.isChange);
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    App.cboVendID.forceSelection = true;
    App.cboBillCity.forceSelection = false;
    App.cboBillState.forceSelection = false;
    App.cboCity.forceSelection = false;
    App.cboState.forceSelection = false;

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "VendID");
        record = sto.getAt(0);
        record.set('Status', 'A');
        record.set('TaxDflt', 'A');
        record.set('MOQType', 'Q');
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        App.cboVendID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboVendID.focus(true); //focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    if (!HQ.isInsert && HQ.isNew) {
        App.cboVendID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
};

//Truoc khi load store se hien Busy
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var cboVendID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoVendor.loading) {
        VendID = value;
        App.cboDfltOrdFromId.store.reload();
        App.stoVendor.reload();
    }
};

var cboVendID_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoVendor.loading) {
        VendID = value;
        App.cboDfltOrdFromId.store.reload();
        App.stoVendor.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboVendID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboVendID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboVendID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboVendID.setValue('');
    }
};

var txtName_Change = function (sender, e) {
    App.txtShipName.setValue(App.txtName.getValue());

};
// Event when cboCountry is changed or selected item
var cboCountry_Change = function (sender, e) {
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

    });
};
// Event when cboBillCountry is changed or selected item
var cboBillCountry_Change = function (sender, e) {
    App.cboBillState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.RemitState) {
                App.cboBillState.setValue(curRecord.data.RemitState);
            }
        var dt = HQ.store.findInStore(App.cboBillState.getStore(), ["State"], [App.cboBillState.getValue()]);
        if (!dt) {
            curRecord.data.RemitState = '';
            App.cboBillState.setValue("");
        }
    });
};
// Event when cboState is changed or selected item
var cboState_Change = function (sender, e) {
    App.cboCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.City) {
                App.cboCity.setValue(curRecord.data.City);
            }
        var dt = HQ.store.findInStore(App.cboCity.getStore(), ["City"], [App.cboCity.getValue()]);
        if (!dt) {
            curRecord.data.City = '';
            App.cboCity.setValue("");
        }

    });
};
// Event when cboBillState is changed or selected item
var cboBillState_Change = function (sender, e) {
    App.cboBillCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.RemitCity) {
                App.cboBillCity.setValue(curRecord.data.RemitCity);
            }
        var dt = HQ.store.findInStore(App.cboBillCity.getStore(), ["City"], [App.cboBillCity.getValue()]);
        if (!dt) {
            curRecord.data.RemitCity = '';
            App.cboBillCity.setValue("");
        }

    });
};

var btnCopy_Click = function (sender, e) {
    App.txtBillAddr1.setValue(App.txtAddr1.getValue());
    App.txtBillAddr2.setValue(App.txtAddr2.getValue());
    App.txtBillAttn.setValue(App.txtAttn.getValue());
    App.txtBillFax.setValue(App.txtFax.getValue());
    App.txtBillName.setValue(App.txtShipName.getValue());
    App.txtBillPhone.setValue(App.txtPhone.getValue());
    App.txtBillSalut.setValue(App.txtSalut.getValue());
    App.txtBillZip.setValue(App.txtShipZip.getValue());
    App.cboBillCountry.setValue(App.cboCountry.getValue());
    App.cboBillState.setValue(App.cboState.getValue());
    App.cboBillCity.setValue(App.cboCity.getValue());
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AP20200/Save',
            params: {
                lstVendor: Ext.encode(App.stoVendor.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                VendID = data.result.VendID;
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.cboVendID.getStore().load({
                    callback: function () {
                        if (Ext.isEmpty(App.cboVendID.getValue())) {
                            App.cboVendID.setValue(VendID);
                            App.stoVendor.reload();
                        }
                        else {
                            App.cboVendID.setValue(VendID);
                            App.stoVendor.reload();
                        }
                    }
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

// Submit the deleted data into server side
var deleteData = function (item) {
    if (item == "yes") {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("DeletingData"),
                url: 'AP20200/DeleteAll',
                timeout: 7200,
                success: function (msg, data) {
                    App.cboVendID.getStore().load();
                    menuClick("new");
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

    
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew)
            App.cboVendID.setValue('');
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoVendor.reload();
    }
};
///////////////////////////////////
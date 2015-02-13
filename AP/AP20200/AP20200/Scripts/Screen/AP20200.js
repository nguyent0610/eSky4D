var selectedIndex = 0;

//ATTENTION: isUpdate, isInsert, isDelete  -- from index.cshtml

// Submit the changed data (created, updated) into server side
function Save() {
    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        //curRecord.data.Name = App.txtName.getValue();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AP20200/Save',
                params: {
                    lstAPVendorHeader: Ext.encode(App.stoVendor.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
                    var venID = App.cboVendID.getValue();
                    App.cboVendID.getStore().load();
                    App.cboVendID.setValue(venID);
                    //menuClick('refresh');
                },

                failure: function (action, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                }
            });
        }
    }
};

// Submit the deleted data into server side
function Delete(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AP20200/Delete',
            params: {
                vendID: App.cboVendID.getValue()
            },
            success: function (action, data) {
                App.cboVendID.getStore().load();
                App.cboVendID.setValue("");
            },

            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });
    }
};

// Load and show binding data to the form
var stoVendor_load = function () {
    var record = App.stoVendor.getAt(0);
    if (record) {
        // Edit record
        App.frmMain.getForm().loadRecord(record);


    } else {
        // If has no record then create a new
        var insertedRecord = Ext.create("App.mdlAP_Vendor", {
            VendID: ""
        })
        App.stoVendor.insert(0, insertedRecord);
        App.frmMain.getForm().loadRecord(insertedRecord);
    }

};



// Event when cboVendID is changed or selected item 
var cboVendID_Change = function (sender, e) {
    App.cboDfltOrdFromId.getStore().load();
    App.stoVendor.reload();
    var curRecord = App.frmMain.getRecord();
    if (curRecord != undefined) {
        App.txtShipName.setValue(curRecord.data.Name);
    }
};

var txtName_Change = function (sender, e) {
    App.txtShipName.setValue(App.txtName.getValue());
};
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboVendID.getStore().load(function () {
        App.cboCountry.getStore().load(function () {
            App.cboBillCountry.getStore().load(function () {
                App.cboClassID.getStore().load(function () {
                    App.cboStatus.getStore().load(function () {
                        App.cboTermsID.getStore().load(function () {
                            App.cboTaxDflt.getStore().load(function () {
                                App.cboTaxId00.getStore().load(function () {
                                    App.cboTaxId01.getStore().load(function () {
                                        App.cboTaxId02.getStore().load(function () {
                                            App.cboTaxId03.getStore().load(function () {
                                                HQ.common.showBusy(false);
                                            })
                                        })
                                    })
                                })
                            })
                        })
                    })
                })
            })
        })
    });
}
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
            App.cboBillCity.setValue("");
        }

    });
};

//Event when cboStatus is changed or selected item
//var cboTermsID_Change = function (sender, e) {
//    var curRecord = App.frmMain.getRecord();
//    curRecord.setDirty();
//};
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

// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            selectedIndex = 0;
            var vendor = App.cboVendID.store.getAt(selectedIndex);
            if (vendor) {
                App.cboVendID.setValue(vendor.data.VendID);
            }
            break;
        case "next":
            if (selectedIndex < (App.cboVendID.store.getCount() - 1))
                selectedIndex += 1;
            var vendor = App.cboVendID.store.getAt(selectedIndex);
            if (vendor) {
                App.cboVendID.setValue(vendor.data.VendID);
            }
            break;
        case "prev":
            if (selectedIndex > 0)
                selectedIndex -= 1;
            var vendor = App.cboVendID.store.getAt(selectedIndex);
            if (vendor) {
                App.cboVendID.setValue(vendor.data.VendID);
            }
            break;
        case "last":
            selectedIndex = App.cboVendID.store.getCount() - 1;
            var vendor = App.cboVendID.store.getAt(selectedIndex);
            if (vendor) {
                App.cboVendID.setValue(vendor.data.VendID);
            }
            break;
        case "save":
            Save();
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (curRecord) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'Delete');
                }
            }
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) {
                App.frmMain.updateRecord()
            };
            if (storeIsChange(App.stoVendor, false)) {
                HQ.message.show(5, '', 'closeScreen');
            } else {
                this.parentAutoLoadControl.close()
            }
            break;
        case "new":
            if (HQ.isInsert) {
                selectedIndex = 0;
                App.cboVendID.setValue("");
                cboVendID_Change(App.cboVendID);
            }
            else {
                HQ.message.show(4, '', '');
            }
            break;
        case "refresh":
            if (App.frmMain.isValid()) {
                App.stoVendor.reload();


            }
            break;
        default:
    }
};

// When anwser the confirmed closing
var closeScreen = function (item) {
    if (item == "no") {
        this.parentAutoLoadControl.close()
    }
    else if (item == "yes") {
        Save();
    }
};

// Check the store of data is change or not
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};

////Declare//////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
//load source cho cac combo ban dau
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
                                                App.stoVendor.reload();
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
        case "next":           
            HQ.combo.next(App.cboVendID, HQ.isChange);
            break;
        case "prev":           
            HQ.combo.prev(App.cboVendID, HQ.isChange);
            break;
        case "last":           
            HQ.combo.last(App.cboVendID, HQ.isChange);
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if(HQ.form.checkRequirePass(App.frmMain))//check require truoc khi save
                    save();
            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (curRecord) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "close":                       
            HQ.common.close(this);            
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    App.cboVendID.setValue('');
                    cboVendID_Change(App.cboVendID);
                }
            }           
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboVendID.valueModels == null) App.cboVendID.setValue('');
                App.cboVendID.getStore().reload();
                App.stoVendor.reload();
            }
           
            break;
        default:
    }
};
//load lần đầu khi mở
var firstLoad = function () {    
    loadSourceCombo();  
};
//load store khi co su thay doi vendid
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboVendID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "VendID");
        record = sto.getAt(0);
        //gan du lieu mac dinh ban dau
        record.data.Status = 'A';
        record.data.TaxDflt = 'A';
        record.data.MOQType = 'Q';

        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        App.cboVendID.forceSelection = false;       
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboVendID.focus(true);//focus ma khi tao moi
      
    }
    var record = sto.getAt(0);     
    App.frmMain.getForm().loadRecord(record);
    frmChange();
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

////////////Kiem tra combo chinh VendID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    if (App.stoVendor.data.length > 0) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoVendor);
        HQ.common.changeData(HQ.isChange, 'AP20200');//co thay doi du lieu gan * tren tab title header
        HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboVendID.valueModels == null || HQ.isNew == true)//App.cboVendID.valueModels == null khi ko co select item nao
            App.cboVendID.setReadOnly(false);
        else App.cboVendID.setReadOnly(HQ.isChange);
    }

};
// Event when cboVendID is changed or selected item 
var cboVendID_Change = function (sender, value) {    
    if(sender.valueModels != null) {
            App.cboDfltOrdFromId.getStore().reload();
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
        menuClick('new');
    }

};
////////////////////////////////////////
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
function save() {
    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        //curRecord.data.Name = App.txtName.getValue();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AP20200/Save',
                params: {
                    lstAPVendorHeader: Ext.encode(App.stoVendor.getChangedData({ skipIdForPhantomRecords: false })),
                    isNew: HQ.isNew
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
                    var vendID = App.cboVendID.getValue();
                    App.cboVendID.getStore().load(function () {
                        App.cboVendID.setValue(vendID);
                        App.stoVendor.reload();
                    });
                               
                  
                },
                failure: function (msg, data) {
                    if (data.result.msgCode) {
                        if(data.result.msgCode==2000)//loi trung key ko the add
                            HQ.message.show(data.result.msgCode, [App.cboVendID.fieldLabel, App.cboVendID.getValue()], '',true);
                        else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
    }
};
// Submit the deleted data into server side
function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AP20200/Delete',
            params: {
                vendID: App.cboVendID.getValue()
            },
            success: function (action, data) {               
                App.cboVendID.setValue("");
                App.cboVendID.getStore().load(function () { cboVendID_Change(App.cboVendID); });

            },

            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        if (App.cboVendID.valueModels == null) App.cboVendID.setValue('');
        App.cboVendID.getStore().reload();
        App.stoVendor.reload();
    }
};
///////////////////////////////////
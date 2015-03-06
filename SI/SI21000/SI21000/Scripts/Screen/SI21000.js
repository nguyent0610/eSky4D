////Declare//////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
//load source cho cac combo ban dau
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboTaxID.getStore().load(function () {
        App.cboTaxBasis.getStore().load(function () {
            App.cboTaxCalcType.getStore().load(function () {
                App.cboTaxCalcLevel.getStore().load(function () {
                    App.cboARtaxPTDate.getStore().load(function () {
                        App.cboAPtaxPTDate.getStore().load(function () {
                            App.cboOPTaxPtDate.getStore().load(function () {
                                App.cboPOTaxPtDate.getStore().load(function () {
                                    App.cboCatFlg.getStore().load(function () {
                                        App.cboCatExcept00.getStore().load(function () {
                                            App.cboCatExcept01.getStore().load(function () {
                                                App.cboCatExcept02.getStore().load(function () {
                                                    App.cboCatExcept03.getStore().load(function () {
                                                        App.cboCatExcept04.getStore().load(function () {
                                                            App.cboCatExcept05.getStore().load(function () {
                                                                App.storeSI_Tax.reload();
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
            HQ.combo.first(App.cboTaxID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboTaxID, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboTaxID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboTaxID, HQ.isChange);
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain))//check require truoc khi save
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
                    App.cboTaxID.setValue('');
                    cboTaxID_Change(App.cboTaxID);
                }
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboTaxID.valueModels == null) App.cboTaxID.setValue('');
                App.cboTaxID.getStore().reload();
                App.storeSI_Tax.reload();
            }

            break;
        default:
    }
};
//load lần đầu khi mở
var firstLoad = function () {
    loadSourceCombo();   
};
//load store khi co su thay doi TaxID
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboTaxID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "TaxID");
        record = sto.getAt(0);
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        App.cboTaxID.forceSelection = false;
        //HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboTaxID.focus(true);//focus ma khi tao moi

    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    frmChange();
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

////////////Kiem tra combo chinh TaxID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    if (App.storeSI_Tax.data.length > 0) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.storeSI_Tax);
        HQ.common.changeData(HQ.isChange, 'SI21000');//co thay doi du lieu gan * tren tab title header
        HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboTaxID.valueModels == null || HQ.isNew == true)//App.cboTaxID.valueModels == null khi ko co select item nao
            App.cboTaxID.setReadOnly(false);
        else App.cboTaxID.setReadOnly(HQ.isChange);
    }
};
// Event when cboTaxID is changed or selected item 
var cboTaxID_Change = function (sender, value) {
    if (sender.valueModels != null) {
        App.storeSI_Tax.reload();
    }

};
//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboTaxID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboTaxID.collapse();
    }
};
////////////////////////////////////////

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
                url: 'SI21000/Save',
                params: {
                    lstTax: Ext.encode(App.storeSI_Tax.getChangedData({ skipIdForPhantomRecords: false })),
                    isNew: HQ.isNew
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
                    var taxID = App.cboTaxID.getValue();
                    App.cboTaxID.getStore().load(function () {
                        App.cboTaxID.setValue(taxID);
                        App.storeSI_Tax.reload();
                    });


                },
                failure: function (msg, data) {
                    if (data.result.msgCode) {
                        if (data.result.msgCode == 2000)//loi trung key ko the add
                            HQ.message.show(data.result.msgCode, [App.cboTaxID.fieldLabel, App.cboTaxID.getValue()], '', true);
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
	        clientValidation: false,
            timeout:1800000,		
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'SI21000/Delete',
            params: {
                taxID: App.cboTaxID.getValue()
            },
            success: function (action, data) {
                App.cboTaxID.setValue("");
                App.cboTaxID.getStore().load(function () { cboTaxID_Change(App.cboTaxID); });

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
        if (App.cboTaxID.valueModels == null) App.cboTaxID.setValue('');
        App.cboTaxID.getStore().reload();
        App.storeSI_Tax.reload();
    }
};
///////////////////////////////////
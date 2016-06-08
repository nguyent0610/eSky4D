////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 15;
var _isLoadMaster = false;
var TaxID = '';

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.storeSI_Tax.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    HQ.util.checkAccessRight(); // Kiem tra quyen Insert Update Delete de disable button tren top bar
    if (HQ.isInsert == false && HQ.isDelete == true && HQ.isUpdate == false) {
        App.menuClickbtnSave.disable();
    }
    HQ.isFirstLoad = true;
    App.frmMain.isValid(); // Require cac field yeu cau tren from

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboTaxID.getStore().addListener('load', checkLoad);
    App.cboTaxBasis.getStore().addListener('load', checkLoad);
    App.cboTaxCalcType.getStore().addListener('load', checkLoad);
    App.cboTaxCalcLevel.getStore().addListener('load', checkLoad);
    App.cboARtaxPTDate.getStore().addListener('load', checkLoad);
    App.cboAPtaxPTDate.getStore().addListener('load', checkLoad);
    App.cboOPTaxPtDate.getStore().addListener('load', checkLoad);
    App.cboPOTaxPtDate.getStore().addListener('load', checkLoad);
    App.cboCatFlg.getStore().addListener('load', checkLoad);
    //
    App.cboCatExcept00.getStore().addListener('load', checkLoad);
    App.cboCatExcept01.getStore().addListener('load', checkLoad);
    App.cboCatExcept02.getStore().addListener('load', checkLoad);
    App.cboCatExcept03.getStore().addListener('load', checkLoad);
    App.cboCatExcept04.getStore().addListener('load', checkLoad);
    App.cboCatExcept05.getStore().addListener('load', checkLoad);
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
        case "prev":
            HQ.combo.prev(App.cboTaxID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboTaxID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboTaxID, HQ.isChange);
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
                    App.cboTaxID.setValue('');
                    App.storeSI_Tax.reload();
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboTaxID.getValue()) {
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
            //HQ.common.close(this);
            break;
    }
};

var frmChange = function () {
    if (App.storeSI_Tax.data.length > 0) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.storeSI_Tax);
        HQ.common.changeData(HQ.isChange, 'SI21000');//co thay doi du lieu gan * tren tab title header

        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboTaxID.valueModels == null || HQ.isNew == true)//App.cboTaxID.valueModels == null khi ko co select item nao
            App.cboTaxID.setReadOnly(false);
        else App.cboTaxID.setReadOnly(HQ.isChange);
    }
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    App.cboTaxID.forceSelection = true;

    if (sto.data.length == 0) {
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

        }
        var record = sto.getAt(0);
        App.frmMain.getForm().loadRecord(record);

    if (!HQ.isInsert && HQ.isNew) {
        App.cboTaxID.forceSelection = true;
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

var cboTaxID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.storeSI_Tax.loading) {
        TaxID = value;
        App.cboDfltOrdFromId.store.reload();
        App.storeSI_Tax.reload();
    }
};

var cboTaxID_Select = function (sender, value) {
    if (sender.valueModels != null && !App.storeSI_Tax.loading) {
        TaxID = value;
        App.cboDfltOrdFromId.store.reload();
        App.storeSI_Tax.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra





var cboTaxID_Change = function (sender, value) {
    //if (sender.valueModels != null) {
    //    App.storeSI_Tax.reload();
    //}
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.storeSI_Tax.loading) {
        TaxID = value;
        //App.cboDfltOrdFromId.store.reload();
        App.storeSI_Tax.reload();
    }

};
    //khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboTaxID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboTaxID.collapse();
    }
};



/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21000/Save',
            params: {
                lstTax: Ext.encode(App.storeSI_Tax.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                TaxID = data.result.TaxID;
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.cboTaxID.getStore().load({
                    callback: function () {
                        if (Ext.isEmpty(App.cboTaxID.getValue())) {
                            App.cboTaxID.setValue(TaxID);
                            App.storeSI_Tax.reload();
                        }
                        else {
                            App.cboTaxID.setValue(TaxID);
                            App.storeSI_Tax.reload();
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
                url: 'SI21000/DeleteAll',
                timeout: 7200,
                success: function (msg, data) {
                    App.cboTaxID.getStore().load();
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
            App.cboTaxID.setValue('');
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.storeSI_Tax.reload();
    }
};
///////////////////////////////////
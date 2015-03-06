var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboShipCountry.getStore().load(function () {
        App.cboBillCountry.getStore().load(function () {
            App.cboDfltLstUnitCost.getStore().load(function () {
                App.cboDfltRcptFrom.getStore().load(function () {
                    App.stoPO00000.reload();
                })
            })
        })
    });
};

// Submit the changed data (created, updated) into server side
var cboShipCountry_Change = function (sender, e) {
    App.cboShipState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.ShipState) {
                App.cboShipState.setValue(curRecord.data.ShipState);
            }
        var dt = HQ.store.findInStore(App.cboShipState.getStore(), ["State"], [App.cboShipState.getValue()]);
        if (!dt) {
            curRecord.data.ShipState = '';
            App.cboShipState.setValue("");
        }
    });

};
var cboShipState_Change = function (sender, e) {
    App.cboShipCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.ShipCity) {
                App.cboShipCity.setValue(curRecord.data.ShipCity);
            }
        var dt = HQ.store.findInStore(App.cboShipCity.getStore(), ["City"], [App.cboShipCity.getValue()]);
        if (!dt) {
            curRecord.data.ShipCity = '';
            App.cboShipCity.setValue("");
        }
    });
};
var cboBillCountry_Change = function (sender, e) {
    App.cboBillState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.BillState) {
                App.cboBillState.setValue(curRecord.data.BillState);
            }
        var dt = HQ.store.findInStore(App.cboBillState.getStore(), ["State"], [App.cboBillState.getValue()]);
        if (!dt) {
            curRecord.data.BillState = '';
            App.cboBillState.setValue("");
        }
    });

};
var cboBillState_Change = function (sender, e) {
    App.cboBillCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.BillCity) {
                App.cboBillCity.setValue(curRecord.data.BillCity);
            }
        var dt = HQ.store.findInStore(App.cboBillCity.getStore(), ["City"], [App.cboBillCity.getValue()]);
        if (!dt) {
            curRecord.data.BillCity = '';
            App.cboBillCity.setValue("");
        }
    });
};

function save() {

    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        //curRecord.data.Name = App.txtName.getValue();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'PO00000/Save',
                params: {
                    lstPO00000: Ext.encode(App.stoPO00000.getChangedData({ skipIdForPhantomRecords: false })),
                    isNew: HQ.isNew
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
                    App.stoPO00000.reload();

                },
                failure: function (msg, data) {

                    HQ.message.process(msg, data, true);
                }
            });
        }
    }

};


// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            break;
        case "next":
            break;
        case "prev":
            break;
        case "last":
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain))//check require truoc khi save
                    save();
            }
            break;
        case "close":
            HQ.common.close(this);
            break;
        case "new":
            break;
        case "delete":
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.stoPO00000.reload();
            }
            break;
        default:
    }
};


var firstLoad = function () {
    loadSourceCombo();
};

//load store khi co su thay doi
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BranchID");
        record = sto.getAt(0);
        record.data.BranchID = HQ.cpnyID;
        record.data.SetupID = 'PO';
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new

    }
    App.frmMain.getForm().loadRecord(App.stoPO00000.getAt(0));
    HQ.common.setRequire(App.frmMain);  //to do cac o la require  
    frmChange();
};

//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoPO00000);
    HQ.common.changeData(HQ.isChange, 'PO00000');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.stoPO00000.reload();
    }
};
///////////////////////////////////



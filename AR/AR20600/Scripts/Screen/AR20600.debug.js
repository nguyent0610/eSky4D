var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboCustId.getStore().load(function () {
        App.cboShipToId.getStore().load(function () {
            App.cboCountry.getStore().load(function () {
                App.stoSOAddress.reload();
            })
        })
    });
};

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
                if (HQ.form.checkRequirePass(App.frmMain))//check require truoc khi save
                    save();
            }
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
        case "delete":
            if (HQ.isDelete) {
                var curRecord = App.frmMain.getRecord();
                if (curRecord) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.stoSOAddress.reload();
            }
            break;
        case "close":
            HQ.common.close(this);
            break;
        default:
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

    //App.cboState.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord != undefined)
    //        if (curRecord.data.State) {
    //            App.cboState.setValue(curRecord.data.State);
    //        }
    //    var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
    //    if (!dt) {
    //        curRecord.data.State = '';
    //        App.cboState.setValue("");
    //    }
    //    if (App.cboState.value == curRecord.data.State) {
    //        cboState_Changed(App.cboState, curRecord.data.State);
    //    }
    //});
};

var cboState_Changed = function (sender, e) {
    App.cboCity.getStore().load();
    App.cboDistrict.getStore().load();

    //App.cboCity.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord && curRecord.data.City) {
    //        App.cboCity.setValue(curRecord.data.City);
    //    }
    //    var dt = HQ.store.findInStore(App.cboCity.getStore(), ["City"], [App.cboCity.getValue()]);
    //    if (!dt) {
    //        curRecord.data.City = '';
    //        App.cboCity.setValue("");
    //    }

    //    App.cboDistrict.getStore().load(function () {
    //        var curRecord = App.frmMain.getRecord();
    //        if (curRecord && curRecord.data.District) {
    //            App.cboDistrict.setValue(curRecord.data.District);
    //        }
    //        var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], [App.cboDistrict.getValue()]);
    //        if (!dt) {
    //            curRecord.data.District = '';
    //            App.cboDistrict.setValue("");
    //        }
    //    });
    //});
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var firstLoad = function () {
    loadSourceCombo();
};

//load store khi co su thay doi
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboShipToId.forceSelection = false;
    App.cboCustId.forceSelection = false;
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
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    if (App.cboCountry.value == record.data.Country) {
        cboCountry_Changed(App.cboCountry, record.data.Country);
    }
    else if (App.cboState.value == record.data.State) {
        cboState_Changed(App.cboState, record.data.State);
    }
    frmChange();
};

//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
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

// Submit the changed data (created, updated) into server side
function save() {
    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AR20600/Save',
                params: {
                    lstAR_SOAddress: Ext.encode(App.stoSOAddress.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');

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
    }
};

function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR20600/DeleteAll',
            success: function (action, data) {
                App.cboShipToId.setValue("");
                App.cboShipToId.getStore().load(function () { cboShipToId_Change(App.cboShipToId); });

            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
            //failure: function (action, data) {
            //    if (data.result.msgCode) {
            //        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
            //    }
            //}
        });
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSOAddress.reload();
    }
};
///////////////////////////////////

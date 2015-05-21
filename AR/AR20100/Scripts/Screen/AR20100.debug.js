var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboClassId.getStore().load(function () {
        App.cboTerritory.getStore().load(function () {
            App.cboCountry.getStore().load(function () {
                App.stoAR_CustClass.reload();
            })
        })
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.isFirstLoad = true;
            HQ.combo.first(App.cboClassId, HQ.isChange);
            break;
        case "prev":
            HQ.isFirstLoad = true;
            HQ.combo.prev(App.cboClassId, HQ.isChange);
            break;
        case "next":
            HQ.isFirstLoad = true;
            HQ.combo.next(App.cboClassId, HQ.isChange);
            break;
        case "last":
            HQ.isFirstLoad = true;
            HQ.combo.last(App.cboClassId, HQ.isChange);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboClassId.valueModels == null) App.cboClassId.setValue('');
                App.cboClassId.getStore().load(function () { App.stoAR_CustClass.reload(); });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    App.cboClassId.setValue('');
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

var firstLoad = function () {
    loadSourceCombo();
};

var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboCountry.forceSelection = false;
    App.cboState.forceSelection = false;
    App.cboDistrict.forceSelection = false;
    App.cboCity.forceSelection = false;
    App.cboClassId.forceSelection = false;
    App.cboCountry.store.clearFilter();
    App.cboState.store.clearFilter();
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BranchID");
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

var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoAR_CustClass);
    HQ.common.changeData(HQ.isChange, 'AR20100');
    if (App.cboClassId.valueModels == null || HQ.isNew == true)
        App.cboClassId.setReadOnly(false);
    else App.cboClassId.setReadOnly(HQ.isChange);
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

function save() {
    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        //curRecord.data.Name = App.txtName.getValue();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AR20100/Save',
                params: {
                    lstAR_CustClass: Ext.encode(App.stoAR_CustClass.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
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
    }
};

function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR20100/DeleteAll',
            success: function (action, data) {
                App.cboClassId.setValue("");
                App.cboClassId.getStore().load(function () { cboClassId_Changed(App.cboClassId); });

            },
            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoAR_CustClass.reload();
    }
};
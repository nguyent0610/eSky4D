var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboAddrID.getStore().load(function () {
        App.cboCountry.getStore().load(function () {
            HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
            App.stoSI_Address.reload();
        })
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboAddrID, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboAddrID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboAddrID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboAddrID, HQ.isChange);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboAddrID.valueModels == null) App.cboAddrID.setValue('');
                App.cboAddrID.getStore().load(function () { App.stoSI_Address.reload(); });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    App.cboAddrID.setValue('');
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

//load lần đầu khi mở
var firstLoad = function () {
    HQ.isFirstLoad = true;
    loadSourceCombo();
};

////////////Kiem tra combo chinh AddrID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoSI_Address);
    HQ.common.changeData(HQ.isChange, 'SI21400');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboAddrID.valueModels == null || HQ.isNew == true)
        App.cboAddrID.setReadOnly(false);
    else App.cboAddrID.setReadOnly(HQ.isChange);
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SI21400');
};

//load store khi co su thay doi AddrID
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboAddrID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "AddrID");
        record = sto.getAt(0);
        HQ.isNew = true;//record la new    
        App.cboAddrID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboAddrID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    frmChange();
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

// Event when cboVendID is changed or selected item 
var cboAddrID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoSI_Address.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboAddrID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboAddrID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboAddrID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }

};

function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'SI21400/Save',
            params: {
                lstSI_Address: Ext.encode(App.stoSI_Address.getChangedData({ skipIdForPhantomRecords: false })),
                isNew: HQ.isNew
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var AddrID = data.result.AddrID;
                App.cboAddrID.getStore().load(function () {
                    App.cboAddrID.setValue(AddrID);
                    App.stoSI_Address.reload();
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

// Submit the deleted data into server side
function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'SI21400/DeleteAll',
            success: function (action, data) {
                App.cboAddrID.setValue("");
                App.cboAddrID.getStore().load(function () { cboAddrID_Change(App.cboAddrID); });

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
        var AddrID = '';
        if (App.cboAddrID.valueModels != null) AddrID = App.cboAddrID.getValue();
        App.cboAddrID.getStore().load(function () { App.cboAddrID.setValue(AddrID); App.stoSI_Address.reload(); });
    }
};
///////////////////////////////////
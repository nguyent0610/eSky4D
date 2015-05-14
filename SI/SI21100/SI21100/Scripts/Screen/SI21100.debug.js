var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboTermsID, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboTermsID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboTermsID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboTermsID, HQ.isChange);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.stoSI_Terms.reload();
            }
            break;
        case "new":
            App.cboTermsID.setValue("");
            break;
        case "delete":
            if (App.cboTermsID.value) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            } else {
                menuClick('new');
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
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (HQ.store.isChange(App.stoSI_Terms)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }
};

var focusChecking = function (item) {
    if (App[invalidField]) {
        App[invalidField].focus();
    }
};

var tabSI21100_AfterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 100);
    }
};

var cboTermsID_Change = function (item, newValue, oldValue) {
    App.stoSI_Terms.load();
};

var firstLoad = function () {
    //loadSourceCombo();
};
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BranchID");
        record = sto.getAt(0);
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
    }
    App.frmMain.getForm().loadRecord(App.stoSI_Terms.getAt(0));
    frmChange();
};

//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoSI_Terms);
    HQ.common.changeData(HQ.isChange, 'SI21100');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboTermsID.valueModels == null || HQ.isNew == true)
        App.cboTermsID.setReadOnly(false);
    else App.cboTermsID.setReadOnly(HQ.isChange);
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21100/Save',
            params: {
                lstSI_Terms: HQ.store.getData(App.frmMain.getRecord().store),
                isNew: HQ.isNew
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.cboTermsID.getStore().reload();
                var TermsID = data.result.TermsID;
                App.cboTermsID.getStore().load(function () {
                    App.cboTermsID.setValue(TermsID);
                    App.stoSI_Terms.reload();
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("DeletingData"),
                url: 'SI21100/DeleteAll',
                timeout: 7200,
                success: function (msg, data) {
                    App.cboTermsID.getStore().load();
                    menuClick("new");
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.stoSI_Terms.reload();
    }
};
////////////////////////////////////////////////////////////////////////

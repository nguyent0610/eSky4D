var menuClick = function (command) {
    switch (command) {
        case "first":
            App.cboTermsID.setValue(App.cboTermsID.getStore().first().get('UserName'));
            break;
        case "prev":
            var combobox = App.cboTermsID;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboTermsID.setValue(combobox.store.getAt(index - 1).data.UserName);
            break;
        case "next":
            var combobox = App.cboTermsID;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboTermsID.setValue(combobox.store.getAt(index + 1).data.UserName);
            break;
        case "last":
            App.cboTermsID.setValue(App.cboTermsID.getStore().last().get('UserName'));
            break;
        case "refresh":
            App.stoSI_Terms.load();
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



var frmloadAfterRender = function (obj) {
    App.stoSI_Terms.load();
};

var loadData = function () {
    if (App.stoSI_Terms.getCount() == 0) {
        App.stoSI_Terms.insert(0, Ext.data.Record());
    }
    App.frmMain.getForm().loadRecord(App.stoSI_Terms.getAt(0));
};


var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21100/Save',
            params: {
                lstSI_Terms: HQ.store.getData(App.frmMain.getRecord().store)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.cboTermsID.getStore().reload();
                menuClick("refresh");
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
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
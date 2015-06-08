var Declare = {
    OM: "OM"
};

var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            App.stoSetup.reload();
        },

        frmMain_fieldChange: function () {
            if (App.stoSetup.getCount() > 0) {
                App.frmMain.updateRecord();

                HQ.isChange = HQ.store.isChange(App.stoSetup);

                HQ.common.changeData(HQ.isChange, 'OM00000');//co thay doi du lieu gan * tren tab title header
                App.cboPOSPrinter.setReadOnly(HQ.isChange);
            }
        },

        menuClick: function (command) {
            switch (command) {
                case "save":
                    if (HQ.isInsert || HQ.isUpdate) {
                        Process.saveData();
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;

                case "delete":
                    if (HQ.isDelete) {
                        
                    }
                    else {
                        HQ.message.show(4, '', '');
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
                            
                        }
                    }
                    break;

                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        Process.refresh("yes");
                    }
                    break;
                default:
            }
        }
    },

    Store: {
        stoSetup_load: function (sto, records, successful, eOpts) {
            HQ.isNew = false;
            if (sto.getCount() == 0) {
                var newSlsper = Ext.create("App.mdlOM_Setup", {
                    SetupID: Declare.OM
                });
                sto.insert(0, newSlsper);
                HQ.isNew = true;
            }
            var frmRecord = sto.getAt(0);
            App.frmMain.loadRecord(frmRecord);
        }
    }
};

var Process = {
    saveData: function () {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();

            App.frmMain.submit({
                url: 'OM00000/SaveSetup',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstSetup: Ext.encode(App.stoSetup.getRecordsValues()),
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.stoSetup.reload();
                },
                failure: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
    },

    showFieldInvalid: function (form) {
        var done = 1;
        form.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                HQ.message.show(15, field.fieldLabel, 'Process.focusOnInvalidField');
                done = 0;
                return false;
            }
        });
        return done;
    },

    focusOnInvalidField: function (item) {
        if (item == "ok") {
            App.frmMain.getForm().getFields().each(function (field) {
                if (!field.isValid()) {
                    field.focus();
                    return false;
                }
            });
        }
    },

    refresh: function (item) {
        if (item == "yes") {
            App.stoSetup.reload();
        }
    }
};
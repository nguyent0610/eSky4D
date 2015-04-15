var _beginStatus = "H";

var Process = {
    renderStatus: function (value) {
        var record = App.cboStatus.store.findRecord("Code",value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },

    saveData: function () {
        App.frmMain.submit({
            url: 'OM41200/SaveData',
            waitMsg: HQ.common.getLang('Submiting') + "...",
            timeout: 1800000,
            params: {
                lstDet: Ext.encode(App.grdDet.store.getRecordsValues())
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                App.grdDet.store.reload();
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
};

var Store = {

};

var Event = {
    Form: {
        frmMain_boxReady: function () {
            App.dtpFromDate.setValue(HQ.dateNow);
            App.dtpToDate.setValue(HQ.dateNow);
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(newValue);
            App.dtpToDate.validate();
        },

        cboStatus_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboHandle.store.reload()

            if (App.grdDet.store.getCount()) {
                App.grdDet.store.each(function (record) {
                    if (record.data.Selected && record.data.Status != newValue) {
                        record.set("Selected", false);
                    }
                });
            }
        },

        btnLoad_click: function (btn, e) {
            if (App.frmMain.isValid()) {
                App.grdDet.store.reload();
            }
        },

        btnImport_click: function (btn, e) {
            Ext.Msg.alert("Import", "Coming soon!");
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    HQ.grid.first(App.grdDet);
                    break;
                case "next":
                    HQ.grid.next(App.grdDet);
                    break;
                case "prev":
                    HQ.grid.prev(App.grdDet);
                    break;
                case "last":
                    HQ.grid.last(App.grdDet);
                    break;
                case "refresh":
                    App.grdDet.store.reload();
                    break;
                case "save":
                    if (HQ.isUpdate) {
                        Process.saveData();
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
            }
        }
    },

    Grid: {
        grd_reject: function (col, record) {
            //var grd = col.up('grid');
            //if (!record.data.tstamp) {
            //    grd.getStore().remove(record, grd);
            //    grd.getView().focusRow(grd.getStore().getCount() - 1);
            //    grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            //} else {
                record.reject();
            //}
        },

        grdDet_beforeEdit: function (editor, e) {
            if (e.field == "Selected") {
                if (e.record.data.Status != App.cboStatus.getValue()) {
                    return false;
                }
            }
        },

        chkSelectHeader_click: function (chk, newValue, oldValue, eOpts) {
            if (newValue)
            {
                if (App.cboHandle.valueModels && App.cboHandle.valueModels.length) {
                    App.grdDet.store.each(function (record) {
                        record.set("Selected", (record.data.Status == App.cboHandle.valueModels[0].data.Status));
                    });
                }
            }
            else{
                App.grdDet.store.each(function (record) {
                    record.set("Selected", false);
                });
            }
        }
    },
};
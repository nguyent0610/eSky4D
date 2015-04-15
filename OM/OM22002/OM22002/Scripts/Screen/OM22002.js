var _beginStatus = "H";

var Process = {
    checkAllValid: function (store) {
        var flag = true;
        store.each(function (record) {
            if (record.data.Selected && !record.data.LevelID) {
                flag = false;
                HQ.message.show(15, HQ.common.getLang("LevelID"));
                return false;
            }
        });
        return flag;
    },

    saveData: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM22002/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstCustChange: HQ.store.getData(App.grdDet.store),
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
    }
};

var Store = {

};

var Event = {
    Form: {
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

        cboCpny_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboDisplayID.store.reload();
        },

        cboDisplayID_change: function (cbo, newValue, oldValue, eOpts) {
            //App.cboLevelID.store.reload();
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
                    if (App.frmMain.isValid()) {
                        App.grdDet.store.reload();
                    }
                    break;
                case "save":
                    if (HQ.isUpdate) {
                        if (App.frmMain.isValid()) {
                            if (Process.checkAllValid(App.grdDet.store)) {
                                HQ.message.show(20150407, '', 'Process.saveData');
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
            }
        }
    },

    Grid: {
        grdDet_beforeEdit: function (editor, e) {
            if (e.field == "LevelID") {
                App.cboColLevelID.store.reload();
            }
        },
        grd_reject: function (col, record) {
            //var grd = col.up('grid');
            //if (!record.data.tstamp) {
            //    grd.getStore().remove(record, grd);
            //    grd.getView().focusRow(grd.getStore().getCount() - 1);
            //    grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            //} else {
                record.reject();
            //}
        }
    },
};
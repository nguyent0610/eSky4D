var _beginStatus = "H";

var Process = {

};

var Store = {

};

var Event = {
    Form: {
        btnLoad_click: function (btn, e) {
            App.grdDet.store.reload();
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
            App.cboLevelID.store.reload();
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
                        if (App.frmMain.isValid()) {
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
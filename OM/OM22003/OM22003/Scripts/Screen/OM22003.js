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

        btnLoad_click: function (btn, e) {
            if (App.frmMain.isValid()) {
                App.grdDet.store.reload();
            }
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        btnConfirm_click: function (btn, e) {
            if (HQ.isUpdate) {
                App.frmImage.submit({
                    url: 'OM22003/SaveAppraise',
                    waitMsg: HQ.common.getLang('Submiting') + "...",
                    timeout: 1800000,
                    params: {
                        lstImage: Ext.encode(App.grdImage.store.getRecordsValues()),
                        record: Ext.encode([App.frmImage.record.data])
                    },
                    success: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        App.grdDet.store.reload();
                        App.winImgAppraise.close();
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
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnCancel_click: function (btn, e) {
            App.winImgAppraise.close();
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

        colBtnView_click: function (command, record) {
            if (command == "View") {
                App.frmImage.record = record;

                App.grdImage.store.reload();
                App.winImgAppraise.show();
            }
        }
    },
};
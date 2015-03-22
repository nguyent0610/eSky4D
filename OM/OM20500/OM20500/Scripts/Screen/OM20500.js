var Process = {
    getCheckedOrder: function (grid) {
        var orders = [];
        grid.store.each(function (item) {
            if (item.data.Selected) {
                orders.push(item.data.OrderNbr);
            }
        });
        return orders.join(",");
    }
};

var Store = {
    stoOrder_load: function (sto, records, successful, eOpts) {
        App.stoDet.reload();
    },

    stoDet_load: function (sto, records, successful, eOpts) {
        sto.filterBy(function (record) { }); // show empty
        App.slmOrder.select(0);
    },

    //stoDet_update: function (sto, record, operation, modifiedFieldNames, eOpts ) {
    //    if (modifiedFieldNames.indexOf("Selected") == 0) {
    //        if (record.data.Selected) {
    //            record.set("QtyShip", record.data.LineQty);
    //        }
    //        else {
    //            record.set("QtyShip", 0);
    //        }
    //    }
    //    else if (modifiedFieldNames.indexOf("QtyShip") == 0) {
    //        if (record.data.QtyShip) {
    //            record.set("Selected", true);
    //        }
    //        else {
    //            record.set("Selected", false);
    //        }
    //    }
    //},

    stoHisOrd_load: function (sto, records, successful, eOpts) {
        if (sto.getCount()) {
            App.slmHisOrd.select(0);
        }
    }
};

var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            App.cboCpnyID.store.load(function (records, operation, success) {
                App.cboCpnyID.setValue(HQ.cpnyID);
            });
        },

        cboCpnyID_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboSlsperId.store.reload();
            App.cboCustID.store.reload();
        },

        btnLoad_click: function (btn, e) {
            App.grdOrder.store.reload();
        },

        btnClose_click: function (btn, e) {
            if (App.frmMain.isValid()) {
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang('Processing')+'...',
                    url: 'OM20500/ClosePO',
                    timeout: 1000000,
                    params: {
                        lstOrderChange: HQ.store.getData(App.grdOrder.store)
                    },
                    success: function (action, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode, data.result.msgParam, '', true);
                        }
                        else {
                            HQ.message.show(201405071);
                        }
                        App.grdOrder.store.reload();
                    },
                    failure: function (errorMsg, data) {
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
    },

    Grid: {
        slmOrder_select: function (slm, selRec, idx, eOpts) {
            App.grdHisOrd.store.reload();
            App.grdDet.store.filterBy(function (record) {
                if (record.data.OrderNbr == selRec.data.OrderNbr) {
                    return record;
                }
            });
        },

        chkSelectHeaderOrder_change: function (chk, newValue, oldValue, eOpts) {
            App.stoOrder.data.each(function (record) {
                record.data.Selected = chk.value;
            });
            App.grdOrder.view.refresh();
        },

        chkSelectHeaderDet_change: function (chk, newValue, oldValue, eOpts) {
            App.stoDet.each(function (record) {
                record.data.Selected = chk.value;

                if (chk.value) {
                    record.data.QtyShip = (record.data.LineQty - record.data.QtyShipped);
                }
                else {
                    record.data.QtyShip = 0;
                }
            });
        },

        grdDet_edit: function (editor, e) {
            if (e.field == "Selected") {
                if (e.record.data.Selected) {
                    e.record.data.QtyShip = e.record.data.LineQty;
                }
                else {
                    e.record.data.QtyShip = 0;
                }
            }
            else if (e.field == "QtyShip") {
                if (e.record.data.QtyShip) {
                    e.record.data.Selected = true;
                }
                else {
                    e.record.data.Selected = false;
                }
            }
            e.record.commit();
        },

        grdDet_validateEdit: function (editor, e) {
            if (e.field == "QtyShip") {
                if (e.value > e.record.data.LineQty) {
                    return false;
                }
            }
        },

        slmHisOrd_select: function (slm, selRec, idx, eOpts) {
            App.grdHisDet.store.reload();
        },

        grd_Reject: function (record, grid) {
            HQ.grid.checkReject(record, grid);

            if (record.data.tstamp == '') {
                grid.getStore().remove(record);
                grid.getView().focusRow(grid.getStore().getCount() - 1);
                grid.getSelectionModel().select(grid.getStore().getCount() - 1);
            } else {
                record.reject();
            }
        }
    }
};
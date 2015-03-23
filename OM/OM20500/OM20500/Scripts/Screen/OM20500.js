var Process = {
    getCheckedOrder: function (grid) {
        var orders = [];
        grid.store.each(function (item) {
            if (item.data.Selected) {
                orders.push(item.data.OrderNbr);
            }
        });
        return orders.join(",");
    },
    processOrderDet: function (orderNbr, remove) {
        if (!remove) {
            App.grdDet.store.snapshot.each(function (item) {
                if (item.data.OrderNbr == orderNbr) {
                    var qtyShip = item.data.LineQty - item.data.QtyShipped;
                    item.set("QtyShip", qtyShip);
                    if (qtyShip > 0) {
                        item.set("Selected", 1);

                    }
                    else
                        item.set("Selected", 0);
                }
            });
        }
        else {
            App.grdDet.store.snapshot.each(function (item) {
                if (item.data.OrderNbr == orderNbr) {
                    item.set("QtyShip", 0);
                    item.set("Selected", 0);
                }
            });
        }
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
    menuClick: function (command) {
        switch (command) {
            case "save":
                var recordOrder = HQ.store.findRecord(App.stoOrder, ["Selected"], [true]);
                if(recordOrder!=undefined)
                App.winOrder.show();
                break;

            default:
        }
    },

    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            App.cboCpnyID.store.load(function (records, operation, success) {
                App.cboCpnyID.setValue(HQ.cpnyID);
            });
        },

        cboCpnyID_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboSlsperId.store.reload();
            App.cboDelivery.store.reload();
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
    Popup:
        {
            btnOK_click: function (btn, e) {
                save();
            },
            btnCancel_click: function (btn, e) {
                App.cboDelivery.setValue('');
                App.winOrder.hide();
            },
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
        slmDet_select: function (slm, selRec, idx, eOpts) {
            var item = slm.selected.items[0].data;
            App.stoItemSite.load({
                params: { siteID: item.SiteID, invtID: item.InvtID },
                callback: function () {
                    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [item.InvtID, item.SiteID]);
                    if (!Ext.isEmpty(site)) {
                        App.lblQtyAvail.setText(item.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + site.QtyAvail);
                    }
                    else {
                        App.lblQtyAvail.setText(item.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
                    }
                },
            });
        },
        chkSelectHeaderOrder_change: function (chk, newValue, oldValue, eOpts) {
            App.stoOrder.data.each(function (record) {
                record.data.Selected = chk.value;
                if (record.data.Selected == true) {
                    Process.processOrderDet(record.data.OrderNbr);
                }
                else {
                    Process.processOrderDet(record.data.OrderNbr, true);
                   
                }
            });
            App.grdOrder.view.refresh();
        },
        grdOrder_beforeEdit: function (editor, e)
        {
            if (e.record.data.Status == 'C' || e.record.data.Status == 'E' ) return false;
        },
        grdOrder_edit: function (editor, e) {
            if (e.field == "Selected") {
                if (e.record.data.Selected) {
                    Process.processOrderDet(e.record.data.OrderNbr);
                }
                else {
                    Process.processOrderDet(e.record.data.OrderNbr, true);
                }
            }      
        },
        chkSelectHeaderDet_change: function (chk, newValue, oldValue, eOpts) {
            App.stoDet.each(function (record) {
                record.data.Selected = chk.value;
                if (chk.value) {
                    Process.processOrderDet(record.data.OrderNbr);
                    var record = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [record.data.OrderNbr]);
                    record.set("Selected", true);
                    
                }
                else {
                    Process.processOrderDet(record.data.OrderNbr, true);
                    var record = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [record.data.OrderNbr]);
                    record.set("Selected", false);
                }
            });
        },
        grdDet_beforeEdit: function (editor, e) {
            var recordOrder = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [e.record.data.OrderNbr]);
            if (recordOrder.data.Status == 'C' || recordOrder.data.Status == 'E' ) return false;
        },
        grdDet_edit: function (editor, e) {
            if (e.field == "Selected") {
                if (e.record.data.Selected) {
                    e.record.set("QtyShip",e.record.data.LineQty);
                }
                else {
                    e.record.set("QtyShip", 0);
                }
            }
            else if (e.field == "QtyShip") {
                if (e.record.data.QtyShip!=0) {
                    e.record.set("Selected",true);
                }
                else {
                    e.record.set("Selected", false);
                }
            }
           
            var record = HQ.store.findRecord(App.stoDet, ["OrderNbr", "Selected"], [e.record.data.OrderNbr, true]);
            var recordOrder = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [e.record.data.OrderNbr]);
            if (record != undefined) {              
                recordOrder.set("Selected", true);
            }
            else recordOrder.set("Selected", false);
           
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



var save = function () {
   
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('SavingData'),
            method: 'POST',
            url: 'OM20500/Save',
            timeout: 1800000,
            params: {
                lstDet: HQ.store.getAllData(App.stoDet,["Selected"],[true]),
                lstOrder: HQ.store.getAllData(App.stoOrder, ["Selected"], [true]),
                shipDate: App.dteShipDate.getValue(),
                aRDocDate: App.dteARDocDate.getValue()
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                Event.Form.btnLoad_click();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
}
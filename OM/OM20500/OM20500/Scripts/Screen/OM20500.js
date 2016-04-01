
var _invtID = "";
var _classID = "";
var _stkUnit = "";
var _purUnit = "";
var _siteID = "";
var _lineRef = "";
var _branchID = "";
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
      
        var allData = App.grdDet.store.snapshot || App.grdDet.store.allData || App.grdDet.store.data;
        if (!remove) {
            allData.each(function (item) {
                if (item.data.OrderNbr == orderNbr) {
                    var objInvtID = HQ.store.findInStore(App.stoOM20500_pdIN_Inventory, ['InvtID'], [item.data.InvtID]);
                    if (!Ext.isEmpty(item.data.InvtID) && !Ext.isEmpty(item.data.SlsUnit) && objInvtID.LotSerTrack != 'N' && !Ext.isEmpty(objInvtID.LotSerTrack)) {
                        if (item.data.QtyShip > 0) {
                            item.set("Selected", 1);
                        }
                        else
                            item.set("Selected", 0);
                    }
                    else {
                        var qtyShip = item.data.LineQty - item.data.QtyShipped;
                        item.set("QtyShip", qtyShip);
                        if (qtyShip > 0) {
                            item.set("Selected", 1);
                        }
                        else
                            item.set("Selected", 0);
                    }
                }
            });
        }
        else {
            allData.each(function (item) {
                if (item.data.OrderNbr == orderNbr) {
                    App.stoLotTrans.clearFilter();
                    var objInvtID = HQ.store.findInStore(App.stoOM20500_pdIN_Inventory, ['InvtID'], [item.data.InvtID]);
                    if (!Ext.isEmpty(item.data.InvtID) && !Ext.isEmpty(item.data.SlsUnit) && objInvtID.LotSerTrack != 'N' && !Ext.isEmpty(objInvtID.LotSerTrack)) {
                        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
                            if (!Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr) && App.stoLotTrans.data.items[i].data.OMLineRef == item.data.LineRef && App.stoLotTrans.data.items[i].data.BranchID == item.data.BranchID && App.stoLotTrans.data.items[i].data.OrderNbr == item.data.OrderNbr) {
                                App.stoLotTrans.data.removeAt(i);
                                item.set("QtyShip", 0);
                                item.set("Selected", 0);
                            }
                        }
                    }
                    else {
                        if (item.data.OrderNbr == orderNbr) {
                            item.set("QtyShip", 0);
                            item.set("Selected", 0);
                        }
                    }
                }
            });
        }
        var record = HQ.store.findRecord(App.stoDet, ["OrderNbr", "Selected"], [orderNbr, true]);
        var recordOrder = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [orderNbr]);
        if (record != undefined) {
            recordOrder.set("Selected", true);
        }
        else recordOrder.set("Selected", false);
      

    }
};
var Store = {
    stoCloseOrder_load: function (sto, records, successful, eOpts) {
        HQ.common.showBusy(false);
    },
    stoOrder_load: function (sto, records, successful, eOpts) {
        App.stoDet.reload();
    },
    stoDet_load: function (sto, records, successful, eOpts) {
        sto.filterBy(function (record) { }); // show empty
        App.slmOrder.select(0);
    },
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
                if (recordOrder != undefined) {
                    App.cboDelivery.setValue('');
                    App.dteShipDate.setValue(HQ.bussinessDate);
                    App.dteARDocDate.setValue(HQ.bussinessDate);
                    //App.chkAddStock.setValue(false);
                    App.winOrder.show();
                }
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
            App.stoLotTrans.reload();
        },

        btnClose_click: function (btn, e) {

            HQ.common.showBusy(true);
            App.stoCloseOrder.reload();
            App.winCloseOrder.show();
            App.chkSelectHeaderCloseOrder.setValue(false);
            //if (App.frmMain.isValid()) {
            //    App.frmMain.submit({
            //        waitMsg: HQ.common.getLang('Processing')+'...',
            //        url: 'OM20500/ClosePO',
            //        timeout: 1000000,
            //        params: {
            //            lstOrderChange: HQ.store.getData(App.grdOrder.store)
            //        },
            //        success: function (action, data) {
            //            if (data.result.msgCode) {
            //                HQ.message.show(data.result.msgCode, data.result.msgParam, '', true);
            //            }
            //            else {
            //                HQ.message.show(201405071);
            //            }
            //            App.grdOrder.store.reload();
            //        },
            //        failure: function (errorMsg, data) {
            //            if (data.result.msgCode) {
            //                HQ.message.show(data.result.msgCode);
            //            }
            //            else {
            //                HQ.message.process(msg, data, true);
            //            }
            //        }
            //    });
            //}
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
        grdOrder_RowClass: function (record) {
            if (record.data.isHighlight == '1')
                return 'hightlight-row'
          
        },

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
            App.stoOrder.suspendEvents();
            App.grdDet.store.suspendEvents();
            var allData = App.stoOrder.snapshot || App.stoOrder.allData || App.stoOrder.data;
            allData.each(function (record) {
                if (record.data.Status != 'C' && record.data.Status != 'E') {
                    record.data.Selected = chk.value;
                    if (record.data.Selected == true) {
                        Process.processOrderDet(record.data.OrderNbr);
                    }
                    else {
                        Process.processOrderDet(record.data.OrderNbr, true);

                    }
                }
            });
            App.stoOrder.resumeEvents();
            App.grdDet.store.resumeEvents();
            App.grdDet.view.refresh();
            App.grdOrder.view.refresh();
        },
        chkSelectHeaderIsAddStock_change: function (chk, newValue, oldValue, eOpts) {
            App.stoOrder.data.each(function (record) {
                if (record.data.Status != 'C' && record.data.Status != 'E') {
                    record.data.IsAddStock = chk.value;                   
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
                var recordOrder = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [record.data.OrderNbr]);
                if (recordOrder.data.Status != 'C' && recordOrder.data.Status != 'E' )
                {
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
                }
            });
        },
        grdDet_beforeEdit: function (editor, e) {
            if (e.field == "QtyShip" && !e.record.data.isEditQtyShip) return false;
            var recordOrder = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [e.record.data.OrderNbr]);
            if (recordOrder.data.Status == 'C' || recordOrder.data.Status == 'E') return false;
            var objInvtID = HQ.store.findInStore(App.stoOM20500_pdIN_Inventory, ['InvtID'], [e.record.data.InvtID]);
            if (!Ext.isEmpty(e.record.data.InvtID) && !Ext.isEmpty(e.record.data.SlsUnit) && objInvtID.LotSerTrack != 'N' && !Ext.isEmpty(objInvtID.LotSerTrack)) {
                if (e.field == "Selected") {
                    if (!e.record.data.Selected) {
                        return false;
                    }
                }
            }
        },
        grdDet_edit: function (editor, e) {
            App.stoLotTrans.clearFilter();
            var objInvtID = HQ.store.findInStore(App.stoOM20500_pdIN_Inventory, ['InvtID'], [e.record.data.InvtID]);
            if (!Ext.isEmpty(e.record.data.InvtID) && !Ext.isEmpty(e.record.data.SlsUnit) && objInvtID.LotSerTrack != 'N' && !Ext.isEmpty(objInvtID.LotSerTrack)) {
                if (e.field == "Selected") {
                    if (!e.record.data.Selected) {
                        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
                            if (!Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr) && App.stoLotTrans.data.items[i].data.OMLineRef == e.record.data.LineRef && App.stoLotTrans.data.items[i].data.BranchID == e.record.data.BranchID && App.stoLotTrans.data.items[i].data.OrderNbr == e.record.data.OrderNbr) {
                                App.stoLotTrans.data.removeAt(i);
                                e.record.set("QtyShip", 0);
                                e.record.set("Selected", 0);
                            }
                        }
                    }                   
                }                
            }
            else {
                if (e.field == "Selected") {
                    if (e.record.data.Selected) {
                        e.record.set("QtyShip", e.record.data.LineQty);
                    }
                    else {
                        e.record.set("QtyShip", 0);
                    }
                }
                else if (e.field == "QtyShip") {
                    if (e.record.data.QtyShip != 0) {
                        e.record.set("Selected", true);
                    }
                    else {
                        e.record.set("Selected", false);
                    }
                }
            }

            var record = HQ.store.findRecord(App.stoDet, ["OrderNbr", "Selected"], [e.record.data.OrderNbr, true]);
            var recordOrder = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [e.record.data.OrderNbr]);
            if (record != undefined) {              
                recordOrder.set("Selected", true);
            }
            else recordOrder.set("Selected", false);
            if (e.field == 'QtyShip') {                
                    Event.Grid.showLot(e.record);                
            }
           
        },

        grdDet_validateEdit: function (editor, e) {
            if (e.field == "QtyShip") {
                if (e.value > e.record.data.LineQty) {
                    return false;
                }
                else if (e.record.data.QtyShip == e.value) return false;
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
        },
       
        txtQtyShip_Change: function (sender) {
         
        },
        showLot: function (record) {
            var recordOrder = App.slmOrder.selected.items[0];
            if (recordOrder.data.Status == 'C' || recordOrder.data.Status == 'E') return false;

            App.winLot.invt = HQ.store.findInStore(App.stoOM20500_pdIN_Inventory, ['InvtID'], [record.data.InvtID]);
            if (!Ext.isEmpty(record.data.InvtID) && !Ext.isEmpty(record.data.SlsUnit) && App.winLot.invt.LotSerTrack != 'N' && !Ext.isEmpty(App.winLot.invt.LotSerTrack)) {

                _classID = App.winLot.invt.ClassID;
                _stkUnit = App.winLot.invt.StkUnit;
                _invtID = App.winLot.invt.InvtID;
                _lineRef = record.data.LineRef;
                _siteID = record.data.SiteID;
                _branchID = record.data.BranchID;
                PopupWinLot.calcLot(record);
            }

            },
        renderRowNumber : function (value, meta, record) {
            return App.stoDet.data.indexOf(record) + 1;
        }
    }
};



var save = function () {
    App.stoLotTrans.clearFilter();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('SavingData'),
            method: 'POST',
            url: 'OM20500/Save',
            timeout: 1800000,
            params: {
                lstDet: HQ.store.getAllData(App.stoDet,["Selected"],[true]),
                lstOrder: HQ.store.getAllData(App.stoOrder, ["Selected"], [true]),
                lstLot: HQ.store.getAllData(App.stoLotTrans),
                delivery: App.cboDelivery.getValue(),
                shipDate: App.dteShipDate.getValue(),
                aRDocDate: App.dteARDocDate.getValue()
                //isAddStock: App.chkAddStock.getValue()
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                Event.Form.btnLoad_click();
                App.winOrder.hide();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.winOrder.hide();
            }
        });
    }
}


var PopupWinLot = {
    calcLot: function (record) {
        record.invt = HQ.store.findInStore(App.stoOM20500_pdIN_Inventory, ['InvtID'], [record.data.InvtID]);
        if (!Ext.isEmpty(record.invt) && !Ext.isEmpty(record.invt.LotSerTrack) && record.invt.LotSerTrack != 'N' && !Ext.isEmpty(record.data.SlsUnit)) {
            var flat = false;
            var det = record.data;
            App.stoLotTrans.clearFilter();
            App.stoLotTrans.data.each(function (item) {
                if (item.data.OMLineRef == det.LineRef && !Ext.isEmpty(item.data.LotSerNbr)) {
                    flat = true;
                }
            });
            if (!flat) {
                HQ.common.showBusy(true, HQ.waitMsg);
                App.cboLotSerNbr.getStore().load({
                    det: record.data,
                    row: record,
                    callback: function (records, options, success) {
                        var det = options.det;
                        var needQty = Math.round(det.UnitMultDiv == "M" ? det.QtyShip * det.UnitRate : det.LineQty / det.UnitRate);
                        App.stoLotTrans.clearFilter();
                        App.cboLotSerNbr.getStore().data.each(function (item) {
                            var newQty = 0;
                            var curQty = 0;
                            App.stoLotTrans.data.each(function (item2) {
                                if (item2.data.LotSerNbr == item.data.LotSerNbr && item2.data.InvtID == item.data.InvtID && item2.data.SiteID == item.data.SiteID) {
                                    curQty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
                                }
                            });
                            if (Math.round(item.data.Qty - curQty) == 0) return true;
                            if ((item.data.Qty - curQty) >= needQty) {
                                newQty = needQty;
                                needQty = 0;
                            }
                            else {
                                newQty = (item.data.Qty - curQty);
                                needQty -= (item.data.Qty - curQty);
                                item.data.Qty = 0;
                            }
                            if (newQty != 0) {
                                var newLot = Ext.create('App.mdlLotTrans');
                                newLot.data.BranchID = det.BranchID;
                                newLot.data.OrderNbr = det.OrderNbr;
                                newLot.data.LotSerNbr = item.data.LotSerNbr;
                                newLot.data.ExpDate = item.data.ExpDate;
                                newLot.data.OMLineRef = det.LineRef;
                                newLot.data.SiteID = det.SiteID;
                                newLot.data.InvtID = det.InvtID;
                                newLot.data.InvtMult = -1;
                                if ((det.UnitMultDiv == "M" ? newQty / det.UnitRate : newQty * det.UnitRate) % 1 > 0) {
                                    newLot.data.CnvFact = 1;
                                    newLot.data.UnitMultDiv = 'M';
                                    newLot.data.Qty = newQty;
                                    newLot.data.UnitDesc = options.row.invt.StkUnit;
                                    if (HQ.objOM.DfltSalesPrice == "I") {
                                        price = Math.round(newLot.data.UnitMultDiv == "M" ? options.row.invt.SOPrice * newLot.data.CnvFact : options.row.invt.SOPrice / newLot.data.CnvFact);
                                        newLot.data.UnitPrice = price;
                                        newLot.data.UnitCost = price;
                                    } else {
                                        var price = HQ.store.findInStore(App.stoPrice, ['InvtID', 'Unit'], [det.InvtID, options.row.invt.StkUnit]);
                                        if (!Ext.isEmpty(price)) price = price.Price;
                                        else price = 0;
                                        newLot.data.UnitPrice = price;
                                        newLot.data.UnitCost = price;
                                    }
                                } else {
                                    newLot.data.Qty = Math.round(det.UnitMultDiv == "M" ? newQty / det.UnitRate : newQty * det.UnitRate);
                                    newLot.data.CnvFact = det.UnitRate;
                                    newLot.data.UnitMultDiv = det.UnitMultDiv;
                                    newLot.data.UnitPrice = det.SlsPrice;
                                    newLot.data.UnitCost = det.SlsPrice;
                                    newLot.data.UnitDesc = det.SlsUnit;
                                }
                                newLot.commit();
                                App.stoLotTrans.insert(App.stoLotTrans.getCount(), newLot);
                            }
                            if (needQty == 0) return false;
                        });
                        App.stoLotTrans.commitChanges();
                        HQ.common.showBusy(false);
                        PopupWinLot.showLot(options.row);
                        PopupWinLot.addNewLot(det);
                    }
                });
            } else {
                PopupWinLot.showLot(record);
                PopupWinLot.addNewLot(det);
            }
        }
    },

    showLot: function (record) {
        App.lblLotQtyAvail.setText('');// xet lai so luong co the xuat =''     
        App.lblQtyAvail.setText('');
        App.stoLotTrans.clearFilter();
        PopupWinLot.filterStore(App.stoLotTrans, ["OMLineRef", "OrderNbr"], [record.data.LineRef, record.data.OrderNbr]);
        //HQ.store.filterStore(App.stoLotTrans, ["OMLineRef", "OrderNbr"], [record.data.LineRef, record.data.OrderNbr]);
        //App.stoLotTrans.filter('OMLineRef', record.data.LineRef);
        //App.stoLotTrans.filter('OrderNbr', record.data.OrderNbr);
        App.winLot.record = record.data;
        App.winLot.record = record;
        App.grdLot.view.refresh();
        App.winLot.setTitle(record.data.InvtID + ' - ' + record.data.SiteID + ' - ' + record.data.QtyShip + ' ' + record.data.SlsUnit);
        var flat = false;
        //App.stoLotTrans.data.each(function (item) {
        //    flat = true;
        //    if (item.data.OMLineRef == App.winLot.record.LineRef && !Ext.isEmpty(item.data.LotSerNbr)) {
        //        flat = true;
        //    }
        //});
        //if (!flat) {
        //    App.cboLotSerNbr.getStore().load(function () {
        //        PopupWinLot.addNewLot(record.data, App.cboLotSerNbr.getStore().getCount() > 0 ? App.cboLotSerNbr.getStore().getAt(0).data : null);
        //    })
        //} else {
        //    App.cboLotSerNbr.getStore().reload();
        //    PopupWinLot.addNewLot(record.data);
        //}
        App.winLot.show();
    },
    btnLotOK_Click: function () {
        setTimeout(function () {
            HQ.common.showBusy(false);
            var recordTran = App.winLot.record.data;
            var flat = null;
            App.stoLotTrans.data.each(function (item) {
                if (!Ext.isEmpty(item.data.LotSerNbr)) {
                    if (item.data.Qty == 0) {
                        App.smlLot.select(App.stoLotTrans.indexOf(item));
                        App.grdLot.deleteSelected();
                    }

                    if (Ext.isEmpty(item.data.UnitDesc)) {
                        HQ.message.show(1000, [HQ.common.getLang('unitDesc')], '', true);
                        flat = item;
                        return false;
                    }

                    if (Ext.isEmpty(item.data.UnitMultDiv)) {
                        HQ.message.show(2525, [item.data.InvtID], '', true);
                        flat = item;
                        return false;
                    }
                }
                else if (item.data.Qty == 0) {

                    App.smlLot.select(App.stoLotTrans.indexOf(item));
                    App.grdLot.deleteSelected();
                }
                else if (item.data.Qty > 0) {
                    return false;
                }
            });
            if (!Ext.isEmpty(flat)) {
                App.smlLot.select(App.stoLotTrans.indexOf(flat));
                return;
            }

            var qty = 0;
            App.stoLotTrans.data.each(function (item) {
                if (!Ext.isEmpty(item.data.LotSerNbr)) {
                    if (item.data.SiteID == recordTran.SiteID && item.data.InvtID == recordTran.InvtID && item.data.OMLineRef == recordTran.LineRef) {
                        qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                    }
                }

            });
            App.winLot.record.set("QtyShip", qty);
            App.winLot.record.set("Selected", qty == 0 ? false : true);
            var record = HQ.store.findRecord(App.stoDet, ["OrderNbr", "Selected"], [App.winLot.record.data.OrderNbr, true]);
            var recordOrder = HQ.store.findRecord(App.stoOrder, ["OrderNbr"], [App.winLot.record.data.OrderNbr]);
            if (record != undefined) {
                recordOrder.set("Selected", true);
            }
            else recordOrder.set("Selected", false);
            for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
                if (!Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr)) {
                    App.stoLotTrans.data.removeAt(i);
                }
            }
            App.winLot.hide();
        }, 300);
    },
    btnLotDel_Click: function () {
        if (App.smlLot.selected.items.length != 0) {
            if (!Ext.isEmpty(App.smlLot.selected.items[0].data.LotSerNbr)) {
                HQ.message.show(2015020806, [App.smlLot.selected.items[0].data.InvtID + ' ' + App.smlLot.selected.items[0].data.LotSerNbr], 'PopupWinLot.deleteLot', true);
            }
        }
    },
    grdLot_BeforeEdit: function (item, e) {
        var obj = e.record.data;
        App.lblLotQtyAvail.setText('');
        var objLot = HQ.store.findInStore(App.cboLotSerNbr.getStore(), ['LotSerNbr'], [obj.LotSerNbr]);
        if (!Ext.isEmpty(objLot)) {
            var Qty = obj.UnitMultDiv == "M" ? Math.floor(objLot.Qty / obj.CnvFact) : objLot.Qty * obj.CnvFact;
            App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + Qty + " " + obj.UnitDesc);
            App.smlLot.selected.items[0].set("ExpDate", objLot.ExpDate);
        }
        else {
            App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
        }
        if (e.field != 'LotSerNbr' && Ext.isEmpty(e.record.data.LotSerNbr)) return false;
    },
    grdLot_SelectionChange: function (item, selected) {
    },
    grdLot_Edit: function (item, e) {
        HQ.common.showBusy(true);
        var objDetail = e.record.data;

        var recordTran = App.winLot.record.data;
        var objIN_Inventory = App.winLot.invt;

        if (e.field == "Qty") {
            //if (objDetail.Qty > 1) {
            //    HQ.message.show(58, '', '');
            //    return false;
            //}            
        }
        if (!Ext.isEmpty(objDetail.LotSerNbr)) {
            PopupWinLot.addNewLot(recordTran);
        }
    },
    grdLot_ValidateEdit: function (item, e) {
        var Qty = 0;
        var objdet = e.record;
        var recordTran = App.winLot.record.data;
        if (["LotSerNbr"].indexOf(e.field) != -1) {
            if (HQ.grid.checkDuplicate(App.grdLot, e, ["LotSerNbr", "OMLineRef", "OrderNbr"])) {
                HQ.message.show(1112, e.value, '');
                return false;
            }
        }
        if (e.field == "Qty") {
            var Qty = 0;
            Qty = e.record.data.UnitMultDiv == "M" ? e.value * e.record.data.CnvFact : e.value / e.record.data.CnvFact;
            var objLot = HQ.store.findInStore(App.cboLotSerNbr.getStore(), ['LotSerNbr'], [objdet.data.LotSerNbr]);
            if (objLot) {
                if (Qty > objLot.Qty) {
                    HQ.message.show(35, '', '');
                    objdet.set('Qty', 0);
                    return false;
                }
            } else {
                HQ.message.show(35, '', '');
                objdet.set('Qty', 0);
                return false;
            }
            var qty = 0;
            App.stoLotTrans.data.each(function (item) {
                if (!Ext.isEmpty(item.data.LotSerNbr)) {
                    if (item.data.LotSerNbr != e.record.data.LotSerNbr && item.data.SiteID == recordTran.SiteID && item.data.InvtID == recordTran.InvtID && item.data.OMLineRef == recordTran.LineRef) {
                        qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                    }
                }

            });
            if (qty + e.value > App.winLot.record.data.LineQty) {

                HQ.message.show('20150818', [qty + e.value, App.winLot.record.data.LineQty], '', true);
                return false;
            }
        }
    },
    cboLotTrans_Change: function (sender) {
        App.lblLotQtyAvail.setText('');
        var objLot = HQ.store.findInStore(App.cboLotSerNbr.getStore(), ['LotSerNbr'], [sender.value]);
        if (!Ext.isEmpty(objLot)) {
            var obj = App.smlLot.selected.items[0].data;
            var Qty = obj.UnitMultDiv == "M" ? Math.floor(objLot.Qty / obj.CnvFact) : objLot.Qty * obj.CnvFact;
            App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + Qty + " " + obj.UnitDesc);
            App.smlLot.selected.items[0].set("ExpDate", objLot.ExpDate);
        }
        else {
            App.lblLotQtyAvail.setText(_invtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
        }

    },
    deleteLot: function (item) {
        if (item == 'yes') {
            App.grdLot.deleteSelected();
        }
    },
    addNewLot: function (record, lotSerNbr) {
        var newRow = Ext.create('App.mdlLotTrans');
        newRow.data.LotSerNbr = !Ext.isEmpty(lotSerNbr) ? lotSerNbr.LotSerNbr : '';
        newRow.data.BranchID = App.winLot.record.data.BranchID;
        newRow.data.OrderNbr = App.winLot.record.data.OrderNbr;

        newRow.data.OMLineRef = record.LineRef;
        newRow.data.UnitDesc = record.SlsUnit;
        newRow.data.InvtID = record.InvtID;
        newRow.data.SiteID = record.SiteID;
        newRow.data.CnvFact = record.UnitRate;
        newRow.data.UnitMultDiv = record.UnitMultDiv;
        newRow.data.InvtMult = -1;
        newRow.data.UnitPrice = record.SlsPrice;
        newRow.data.ExpDate = !Ext.isEmpty(lotSerNbr) ? lotSerNbr.ExpDate : '';

        HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);
    },

    filterStore: function (store, field, value) {
        store.filterBy(function (record) {
            if (record) {
                var flat = true;
                for (var i = 0; i < field.length; i++) {
                    if (record.data[field[i]].toString().toLowerCase() != (HQ.util.passNull(value[i]).toLowerCase())) {
                        flat = false;
                        break;
                    }
                }
                if (flat) return record;

            }
        });
    },
    renderRowNumberLot: function (value, meta, record) {
        return App.stoLotTrans.data.indexOf(record) + 1;
    }

}
var PopupWinClose = {
    chkSelectHeaderCloseOrder_change: function (chk, newValue, oldValue, eOpts) {
        App.stoCloseOrder.data.each(function (record) {
                record.data.Selected = chk.value;
        });
        App.grdCloseOrder.view.refresh();
    },
    btnCloseOK_Click: function (btn, e) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('Processing') + '...',
            url: 'OM20500/ClosePO',
            timeout: 1000000,
            clientValidation: false,
            params: {
                lstOrderChange: HQ.store.getData(App.grdCloseOrder.store,["Selected"],[true])
            },
            success: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '', true);
                }
                else {
                    HQ.message.show(201405071);
                }
                App.winCloseOrder.hide();
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
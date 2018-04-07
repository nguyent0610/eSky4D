HQ.recentRecord = null;
HQ.focus = 'batch';
HQ.objBatch = null;
HQ.isTransfer = false;
var _lotQ = 'Q';
HQ.currentDet;
//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


var stoTrans_BeforeLoad = function () {
    //App.grdTrans.view.loadMask.disable();
};
var stoHandle_Load = function () {
    App.Handle.setValue('N');
};
var stoTrans_Load = function () {
    bindTran();
};
var stoDetail_Load = function () {
    checkSourceDetail();
}
var store_Load = function () {
    HQ.numSource++;
    checkSetDefault();
};
var stoBatch_Load = function () {
    
    var record = App.stoBatch.getById(App.BatNbr.getValue());
    if (record) {
        bindBatch(record);
        //if (record.data.Status == 'H') {
        //    HQ.common.lockItem(App.frmMain, false);
        //}
        //else {
        //    HQ.common.lockItem(App.frmMain, true);
        //}

        //if (!HQ.isInsert && HQ.isNew) {
        //    App.BatNbr.forceSelection = true;
        //    HQ.common.lockItem(App.frmMain, true);
        //}
        //else if (!HQ.isUpdate && !HQ.isNew) {
        //    HQ.common.lockItem(App.frmMain, true);
        //}
    }
    
};
var stoBatch_BeforeLoad = function (store, operation, eOpts) {
    //if (Ext.isEmpty(operation.params.query)) {
    //   // operation.params.query = App.BatNbr.getValue();
    //}

};

var checkSelect = function (records, options, success) {
    HQ.numSelectTrans++;
    if (HQ.numSelectTrans == HQ.maxSelectTrans) {
        HQ.common.showBusy(false);
        getQtyAvail(options.row);
    }
};
var checkSetDefault = function () {
    if (HQ.numSource == HQ.maxSource) {
        defaultOnNew();
    }
};
var checkSourceEdit = function (records, options, success) {
    HQ.numTrans++;
    if (HQ.numTrans == HQ.maxTrans) {
        checkExitEdit(options.row);
    }
};
var checkSourceDetail = function (records, options, success) {
    HQ.numDetail++;
    if (HQ.numDetail == HQ.maxDetail) {
        bindTran();
    }
}

var checkSourceEditLot = function (records, options, success) {
    HQ.numLot++;
    if (HQ.numLot == HQ.maxLot) {
        checkExitEditLot(options.row);
    }
};
var checkSourceTransfer = function (records, options, success) {
    HQ.numLotTransfer++;
    if (HQ.numLotTransfer == HQ.maxLotTransfer) {
        bindTransfer();
    }
};
var checkSelectLot = function (records, options, success) {
    HQ.numSelectLot++;
    if (HQ.numSelectLot == HQ.maxSelectLot) {
        HQ.common.showBusy(false);
        getLotQtyAvail(options.row);
    }
};
var checkExitEditLot = function (row) {
    var key = row.field;
    var record = row.record;
    var lot = row.record.data;
    if (key == "Qty") {
        if (Ext.isEmpty(record.data.LotSerNbr)) {
            if (record.data.Qty > 0) {
                HQ.common.showBusy(true, '', App.winLot);
                App.direct.IN10100Number(
                    lot.InvtID, Ext.Date.format(lot.ExpDate, 'Y-m-d'), 'LotNbr',
                    {
                        success: function (result) {
                            lot.LotSerNbr = result[0];
                            record.commit();
                            HQ.common.showBusy(false, '', App.winLot);

                            if (!Ext.isEmpty(result)) {
                                var newRow = Ext.create('App.mdlLotTrans');
                                newRow.data.INTranLineRef = lot.INTranLineRef;
                                newRow.data.UnitDesc = lot.UnitDesc;
                                newRow.data.UnitPrice = lot.UnitPrice;
                                newRow.data.CnvFact = lot.CnvFact;
                                newRow.data.UnitMultDiv = lot.UnitMultDiv;
                                newRow.data.ExpDate = App.DateEnt.getValue();
                                HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);
                            }
                            getLotQtyAvail(record);
                        },
                        failure: function (result) {
                            HQ.common.showBusy(false, '', App.winLot);
                        }
                    });

            } else {
                getLotQtyAvail(record);
            }
        }
        else {
            getLotQtyAvail(record);
        }

    } else if (key == "LotSerNbr") {
        var flat = false;
        App.stoLotTrans.data.each(function (item) {
            if (item.data.LotSerNbr == lot.LotSerNbr && item.id != record.id) {
                flat = true;
                return false;
            }
        });
        if (flat) {
            HQ.message.show(219, "", "", true);
            lot.LotSerNbr = "";
            HQ.common.showBusy(false);
            record.commit();
            return;
        }
        lot.UnitDesc = App.winLot.record.data.UnitDesc;
        lot.UnitPrice = lot.UnitCost = App.winLot.record.data.UnitPrice;
        lot.UnitMultDiv = App.winLot.record.data.UnitMultDiv;
        lot.CnvFact = App.winLot.record.data.CnvFact;
        var objCboLot = HQ.store.findRecord(App.stoCalcLot, ['InvtID', 'SiteID', 'LotSerNbr'], [lot.InvtID, lot.SiteID, lot.LotSerNbr]);
        if (objCboLot) {
            lot.PackageID = objCboLot.data.PackageID;
            lot.tstamp = '1';
        }

        var itemLot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', 'LotSerNbr'], [lot.InvtID, lot.SiteID, lot.LotSerNbr]);
        if (!Ext.isEmpty(itemLot)) {
            lot.ExpDate = itemLot.ExpDate;
        }

        if (!Ext.isEmpty(lot.LotSerNbr)) {
            var newRow = Ext.create('App.mdlLotTrans');
            newRow.data.INTranLineRef = lot.INTranLineRef;
            newRow.data.UnitDesc = lot.UnitDesc;
            newRow.data.UnitPrice = lot.UnitPrice;
            newRow.data.CnvFact = lot.CnvFact;
            newRow.data.ExpDate = App.DateEnt.getValue();
            newRow.data.UnitMultDiv = lot.UnitMultDiv;
           
            HQ.store.insertRecord(App.stoLotTrans, key, newRow, true);
        }
        if (lot.ExpDate == null || lot.ExpDate == undefined || lot.ExpDate == '') {
            lot.ExpDate = App.DateEnt.getValue();//HQ.businessDate;
        }
    }
    getLotQtyAvail(row.record);
    record.commit();
    HQ.common.showBusy(false);
};
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////



//// Event ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var frmMain_BoxReady = function () {
    HQ.util.checkAccessRight();
    HQ.numDetail = 0;
    HQ.maxDetail = 2;
    HQ.numSource = 0;
    HQ.maxSource = 8;
    HQ.numTrans = 0;
    HQ.maxTrans = 3;
    HQ.numSelectTrans = 0;
    HQ.maxSelectTrans = 2;
    App.BatNbr.key = true;
    App.BranchID.setValue(HQ.cpnyID);
    App.Handle.getStore().addListener('load', stoHandle_Load);
    App.Status.getStore().addListener('load', store_Load);
    App.cboTransInvtID.getStore().addListener('load', store_Load);
    App.FromToSiteID.getStore().addListener('load', store_Load);
    App.SlsperID.getStore().addListener('load', store_Load);
    App.SiteID.getStore().addListener('load', store_Load);
    App.ReasonCD.getStore().addListener('load', store_Load);
    App.cboPosmID.getStore().addListener('load', store_Load);
    App.stoSetup.addListener('load', store_Load);

    App.cboTransInvtID.store.reload();
    App.stoInvt = App.cboTransInvtID.getStore();

    App.stoSetup.load();

    HQ.common.showBusy(true, HQ.waitMsg);
    if (HQ.showQtyOnhand) {
        App.colQtyOnHand.show();
        if (App.colLotQtyOnHand) {
            App.colLotQtyOnHand.show();
        }
    }
};
var frmMain_FieldChange = function (item, field, newValue, oldValue) {
    if (field.key != undefined) {
        return;
    }
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (Object.keys(App.stoBatch.getChangedData()).length > 0 || App.grdTrans.isChange == true) {
        setChange(true);
    } else {
        setChange(false);
    }
};
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'batch') {
                if (HQ.isChange || App.grdTrans.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    App.frmMain.loadRecord(App.stoBatch.first());
                }
            } else if (HQ.focus == 'trans') {
                HQ.grid.first(App.grdTrans);
            }
            break;
        case "next":
            if (HQ.focus == 'batch') {
                if (HQ.isChange || App.grdTrans.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    var index = App.stoBatch.indexOf(App.stoBatch.getById(App.BatNbr.getValue()));
                    App.BatNbr.setValue(App.stoBatch.getAt(index + 1).get('BatNbr'));
                }
            } else if (HQ.focus == 'trans') {
                HQ.grid.next(App.grdTrans);
            }
            break;
        case "prev":
            if (HQ.focus == 'batch') {
                if (HQ.isChange || App.grdTrans.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    var index = App.stoBatch.indexOf(App.stoBatch.getById(App.BatNbr.getValue()));
                    App.BatNbr.setValue(App.stoBatch.getAt(index - 1).get('BatNbr'));
                }
            } else if (HQ.focus == 'trans') {
                HQ.grid.prev(App.grdTrans);
            }
            break;
        case "last":
            if (HQ.focus == 'batch') {
                if (HQ.isChange || App.grdTrans.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    App.frmMain.loadRecord(App.stoBatch.last());
                }
            } else if (HQ.focus == 'trans') {
                HQ.grid.last(App.grdTrans);
            }
            break;
        case "save":
            if (HQ.isBusy == false) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    save();
                }
            }
            break;
        case "delete":
            if (HQ.focus == 'batch') {
                if (App.BatNbr.value) {
                    if (HQ.isDelete) {
                        if (App.Status.getValue() != 'H') {
                            HQ.message.show(2015020805, [App.BatNbr.value], '', true);
                        } else {
                            HQ.message.show(11, '', 'deleteHeader');
                        }
                    } else {
                        HQ.message.show(728, '', '', true);
                    }
                } else {
                    menuClick('new');
                }
            } else if (HQ.focus == 'trans') {
                if ((App.BatNbr.value && HQ.isUpdate) || (!App.BatNbr.value && HQ.isInsert)) {
                    if (App.Status.getValue() != "H") {
                        HQ.message.show(2015020805, [App.BatNbr.value], '', true);
                        return;
                    }
                    if (App.smlTrans.selected.items.length != 0) {
                        if (!Ext.isEmpty(App.smlTrans.selected.items[0].data.InvtID)) {
                            HQ.message.show(2015020806, [App.smlTrans.selected.items[0].data.InvtID], 'deleteTrans', true);
                        }
                    }
                }
            }
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (HQ.isChange || App.grdTrans.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
        case "new":
            if (HQ.focus == 'batch') {
                if (HQ.isChange || App.grdTrans.isChange ) {
                    HQ.message.show(150, '', '');
                }
                else {
                    defaultOnNew();
                }
            } else if (HQ.focus == 'trans') {
                eventNew(App.stoTrans, App.grdTrans, 'InvtID');
            }

            //if (HQ.focus == 'batch') {
            //    if ((HQ.isChange || App.grdTrans.isChange) && !Ext.isEmpty(App.BatNbr.getValue())) {
            //        HQ.message.show(2015030201, '', "askNew", true);
            //    } else {
            //        defaultOnNew();
            //    }
            //} else if (HQ.focus == 'trans') {
            //    eventNew(App.stoTrans, App.grdTrans, 'InvtID');
            //    //HQ.store.insertRecord(App.stoTrans, 'InvtID', Ext.create('App.mdlTrans'), true);
            //}
            break;
        case "refresh":
            if ((HQ.isChange || App.grdTrans.isChange)) {
                HQ.message.show(20150303, '', "askRefresh", true);
            } else {
                if (!Ext.isEmpty(App.BatNbr.getValue())) {
                    App.stoBatch.reload();
                } else {
                    defaultOnNew();
                }
            }
            break;
        case "print":
            if (!Ext.isEmpty(App.BatNbr.getValue()) && App.Status.value != "H") {
                report();
            }
            break;
        default:
    }
};

var eventNew = function (sto, grd, keys) {
    if (HQ.isInsert) {
        HQ.store.insertBlank(sto, keys);
        HQ.grid.last(grd);
        if (grd.editingPlugin) {
            grd.editingPlugin.startEditByPosition({
                row: sto.getCount() - 1,
                column: 1
            });
        } else {
            grd.lockedGrid.editingPlugin.startEditByPosition({
                row: sto.getCount() - 1,
                column: 1
            });
        }
    }
};

var btnLot_Click = function () {
    if (Ext.isEmpty(this.record.invt)) {
        App.cboTransInvtID.store.clearFilter();
        this.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [this.record.data.InvtID]);
    }
    if (!Ext.isEmpty(this.record.invt)){ 
        if(this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.UnitDesc)) {
            showLot(this.record, true);
        }
    }
};
var btnImport_Click = function (c, e) {
    if (Ext.isEmpty(App.SiteID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('siteid')], '', true);
        App.btnImport.reset();
        return;
    }

    var fileName = c.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            clientValidation: false,
            method: 'POST',
            url: 'IN10100/Import',
            timeout: 1000000,
            params: {
                lineRef: lastLineRef()
            },
            success: function (msg, data) {
                if (this.result.data.lstTrans != undefined) {
                    App.stoTrans.suspendEvents();
                    this.result.data.lstTrans.forEach(function (item) {
                        var objTrans = HQ.store.findRecord(App.stoTrans, ['InvtID'], [item.InvtID]);
                        if (!objTrans) {
                            HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);
                            var newTrans = App.stoTrans.data.items[App.stoTrans.getCount() - 1];
                            newTrans.set('JrnlType', item.JrnlType);
                            newTrans.set('ReasonCD', item.ReasonCD);
                            newTrans.set('TranAmt', item.TranAmt);
                            newTrans.set('BranchID', HQ.cpnyID);
                            newTrans.set('CnvFact', item.CnvFact);
                            newTrans.set('ExtCost', item.TranAmt);
                            newTrans.set('InvtID', item.InvtID);
                            newTrans.set('InvtMult', item.InvtMult);
                            newTrans.set('LineRef', item.LineRef);
                            newTrans.set('Qty', item.Qty);
                            newTrans.set('QtyOnHand', item.QtyOnHand);
                            newTrans.set('SiteID', item.SiteID);
                            newTrans.set('TranDate', App.DateEnt.getValue());
                            newTrans.set('TranDesc', item.TranDesc);
                            newTrans.set('TranType', item.TranType);
                            newTrans.set('UnitCost', item.UnitCost);
                            newTrans.set('UnitDesc', item.UnitDesc);
                            newTrans.set('UnitMultDiv', item.UnitMultDiv);
                            newTrans.set('UnitPrice', item.UnitPrice);
                        }
                        else {
                            objTrans.set('Qty', (objTrans.data.Qty + item.Qty));
                        }
                    });
                    App.stoTrans.resumeEvents();
                    App.grdTrans.view.refresh();

                    App.stoLotTrans.clearFilter();
                    App.stoLotTrans.suspendEvents();
                    this.result.data.lstLot.forEach(function (item) {
                        var objLot = HQ.store.findRecord(App.stoLotTrans, ['InvtID', 'LotSerNbr'], [item.InvtID, item.LotSerNbr]);
                        var objTrans = HQ.store.findRecord(App.stoTrans, ['InvtID'], [item.InvtID]);
                        if (!objLot) {
                            HQ.store.insertRecord(App.stoLotTrans, ["InvtID", "LotSerNbr"], Ext.create('App.mdlLotTrans'), true);
                            var newLot = App.stoLotTrans.data.items[App.stoLotTrans.getCount() - 1];
                            newLot.set('LotSerNbr',item.LotSerNbr);
                            newLot.set('INTranLineRef',objTrans.data.LineRef);
                            newLot.set('ExpDate',new Date(parseInt(item.ExpDate.substr(6))));
                            newLot.set('InvtID',item.InvtID);
                            newLot.set('InvtMult',item.InvtMult);
                            newLot.set('MfgrLotSerNbr',item.MfgrLotSerNbr);
                            newLot.set('Qty', item.Qty);
                            newLot.set('QtyOnHand', item.QtyOnHand);
                            newLot.set('SiteID',item.SiteID);
                            newLot.set('SlsperID',item.SlsperID);
                            newLot.set('TranDate',App.DateEnt.getValue());
                            newLot.set('WarrantyDate',new Date(parseInt(item.WarrantyDate.substr(6))));
                            newLot.set('TranType','RC');
                            newLot.set('UnitCost',item.UnitCost);
                            newLot.set('UnitDesc',item.UnitDesc);
                            newLot.set('UnitMultDiv',item.UnitMultDiv);
                            newLot.set('UnitPrice',item.UnitPrice);
                            newLot.set('CnvFact',item.CnvFact);
                            newLot.set('MfgrLotSerNbr','');
                        }
                        else {
                            objLot.set('Qty',(objLot.data.Qty + item.Qty)); 
                        }
                    });
                    App.stoLotTrans.resumeEvents();
                    calculate();
                    checkTransAdd();

                    if (!Ext.isEmpty(this.result.data.message)) {
                       var result = "<div style ='overflow: auto !important; min-width:400px !important; max-height:400px !important'> " + this.result.data.message + " </div>";
                        HQ.message.show('2013103001', [result], '', true);
                      //  HQ.message.show('2013103001', [this.result.data.message], '', true);
                    } else {
                        HQ.message.process(msg, data, true);
                    }
                } else {
                    HQ.message.process(msg, data, true);
                }
                App.btnImport.reset();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.btnImport.reset();
            }
        });
    } else {
        HQ.message.show('2014070701', [ext], '', true);
        App.btnImport.reset();
    }
};
var btnLotOK_Click = function () {
    if (!App.grdLot.isLock) {
        var det = App.winLot.record.data;
        var flat = null;
        App.stoLotTrans.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                if (item.data.Qty == 0) {
                    HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
                    flat = item;
                    return false;
                }

                if (Ext.isEmpty(item.data.UnitDesc)) {
                    HQ.message.show(1000, [HQ.common.getLang('unit')], '', true);
                    flat = item;
                    return false;
                }

                if (Ext.isEmpty(item.data.UnitMultDiv)) {
                    HQ.message.show(2525, [invtID], '', true);
                    flat = item;
                    return false;
                }
            }
        });
        if (!Ext.isEmpty(flat)) {
            App.smlLot.select(App.stoLotTrans.indexOf(flat));
            return;
        }

        var qty = 0.0;
        App.stoLotTrans.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                if (item.data.SiteID == det.SiteID && item.data.InvtID == det.InvtID && item.data.INTranLineRef == det.LineRef) {
                    qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                }
            }
        });

        var lineQty = (det.UnitMultDiv == "M" ? qty / det.CnvFact : det.Qty * det.CnvFact)
        if (HQ.currentDet.data.LotSerTrack == _lotQ) {
            App.winLot.record.data.Qty = lineQty;
        } else {
            App.winLot.record.data.Qty = Math.round(lineQty);
        }
        
        App.winLot.record.data.TranAmt = App.winLot.record.data.Qty * App.winLot.record.data.UnitPrice;
        App.winLot.record.commit();

        App.grdTrans.view.refresh();

        calculate();

        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr)) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
    }


    App.winLot.hide();
};
var btnLotDel_Click = function () {
    if ((App.BatNbr.value && HQ.isUpdate) || (!App.BatNbr.value && HQ.isInsert)) {
        if (App.Status.getValue() != "H") {
            HQ.message.show(2015020805, [App.BatNbr.value], '', true);
            return;
        }
        if (App.smlLot.selected.items.length != 0) {
            if (!Ext.isEmpty(App.smlLot.selected.items[0].data.LotSerNbr)) {
                HQ.message.show(2015020806, [App.smlLot.selected.items[0].data.InvtID + ' ' + App.smlLot.selected.items[0].data.LotSerNbr], 'deleteLot', true);
            }
        }
    }
};
var btnExport_Click = function () {
    //if (Ext.isEmpty(App.BatNbr.getValue())) {
    //    HQ.message.show('1000', [HQ.common.getLang('batnbr')], '', true);
    //    return;
    //}
    //var form = Ext.DomHelper.append(document.body, {
    //    tag: 'form',
    //    method: 'post',
    //    action: 'IN10100/Export'
    //});

    //document.body.appendChild(form);

    App.frmMain.submit({
        url: 'IN10100/Export',
        timeout: 1000000,
        clientValidation: false,
        success: function (msg, data) {
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });


};
var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'IN10100?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
};

var cboBatNbr_Change = function (item, newValue, oldValue) {
    var record = App.stoBatch.getById(newValue);
    if (record) {
        //showMask();
        HQ.isNew = false;
        bindBatch(record);
    } else {
        if (HQ.recentRecord != record) {
            //console.log('cboOrderNbr_Change new');
            //showMask();
            //defaultOnNew();
            //setOrderTypeContrainst();

        }
        else {
        }
    }
    HQ.recentRecord = record;
}
var cboStatus_Change = function (item, newValue, oldValue) {
    App.Handle.getStore().reload();
};
var cboFromToSiteID_Change = function () {
    if (App.FromToSiteID.getValue() == App.SiteID.getValue()) {
        App.FromToSiteID.setValue('');
    }
};
var cboSiteID_Change = function () {
    if (App.FromToSiteID.getValue() == App.SiteID.getValue()) {
        App.SiteID.setValue('');
    }
};
var cboTrnsferNbr_Change = function () {
    if (Ext.isEmpty(App.BatNbr.getValue())) {
        if (!Ext.isEmpty(App.TrnsferNbr.getValue())) {
            App.btnImport.setDisabled(true);
            HQ.numLotTransfer = 0;
            HQ.maxLotTransfer = 2;
            App.stoTransfer.load({
                params: { branchID: App.BranchID.getValue(), tranDate: App.DateEnt.getValue(), trnsfrDocNbr: App.TrnsferNbr.getValue() },
                callback: checkSourceTransfer
            });
            App.stoLotTransfer.load({
                params: { branchID: App.BranchID.getValue(), tranDate: App.DateEnt.getValue(), trnsfrDocNbr: App.TrnsferNbr.getValue() },
                callback: checkSourceTransfer
            });

        } else {
            if (Ext.isEmpty(App.BatNbr.getValue())) {
                App.btnImport.setDisabled(false);
            }
            HQ.isTransfer = false;
            App.stoTrans.removeAll();
            HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);
            checkTransAdd();
            calculate();
        }
    }
};

var grdTrans_BeforeEdit = function (item, e) {
    if (!HQ.grid.checkBeforeEdit(e, ['InvtID'])) return false;
    if (App.Status.getValue() != 'H') {
        return false;
    }
    //if (App.grdTrans.isLock) {
    //    return false;
    //}
    if (e.field == 'Qty' && e.record.data.LotSerTrack == _lotQ) {
        return false;
    }
    if (e.field == 'UnitDesc' && e.record.data.LotSerTrack == _lotQ) {
        return false;
    }
    if (!Ext.isEmpty(HQ.store.findInStore(App.TrnsferNbr.store, ['TrnsfrDocNbr'], [App.TrnsferNbr.getValue()]))) {
        return false;
    }

    if (Ext.isEmpty(App.SiteID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('SiteID')], '', true);
        return false;
    }

    //if (Ext.isEmpty(App.SlsperID.getValue())) {
    //    HQ.message.show(1000, [HQ.common.getLang('SlsperID')], '', true);
    //    return false;
    //}
    var key = e.field;

    if (!HQ.grid.checkInput(e, ['InvtID']) && key != 'InvtID') {
        return false;
    }
    if (!Ext.isEmpty(e.record.data.InvtID) && key == 'InvtID') {
        return false;
    }

    if (Ext.isEmpty(e.record.data.LineRef)) {
        e.record.data.LineRef = lastLineRef();
        e.record.data.InvtMult = 1;
        e.record.data.TranType = 'RC';
        e.record.data.JrnlType = 'IN';
        e.record.data.BranchID = HQ.cpnyID;
        e.record.data.BatNbr = HQ.objBatch.data.BatNbr;
        e.record.data.TranDate = App.DateEnt.getValue();
        e.record.data.SiteID = App.SiteID.getValue();
        e.record.commit();
    }

    if (key == 'UnitPrice') {
        var invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]); //e.row.invt; //
        if (!Ext.isEmpty(invt) && invt.ValMthd == 'T') {
            return false;
        }
    }
    App.cboTransUnitDesc.setValue('');
};
var grdTrans_SelectionChange = function (item, selected) {
    HQ.focus = 'trans';
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectTrans = 0;
            HQ.common.showBusy(true, 'Process...');
            App.stoItemSite.load({
                params: { siteID: App.SiteID.getValue(), invtID: selected[0].data.InvtID },
                callback: checkSelect,
                row: selected[0]
            });
            App.stoUnit.load({
                params: { invtID: selected[0].data.InvtID },
                callback: checkSelect,
                row: selected[0]
            });
        } else {
            App.lblQtyAvail.setText('');
        }
    }
};
var grdTrans_Edit = function (item, e) {
    HQ.focus = 'trans';
    var key = e.field;
    if (Object.keys(e.record.modified).length > 0) {
        App.grdTrans.isChange = true;
        if (e.record.invt == undefined) {
            e.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]);
        }
        var invt = e.record.invt;
        if (!Ext.isEmpty(invt)) {

            if (key == 'InvtID' && Ext.isEmpty(e.record.data.UnitDesc)) {
                if (invt.LotSerTrack != _lotQ) {
                    var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);
                    if (!Ext.isEmpty(cnv)) {
                        e.record.data.UnitDesc = invt.StkUnit;
                        e.record.data.CnvFact = cnv.CnvFact == 0 ? 1 : cnv.CnvFact;
                        e.record.data.UnitMultDiv = cnv.MultDiv;
                        e.record.data.ClassID = invt.ClassID;
                    } else {
                        return;
                    }
                }
                else {
                    var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, 'PAC');
                    if (!Ext.isEmpty(cnv)) {
                        e.record.data.UnitDesc = invt.StkUnit;
                        e.record.data.CnvFact = 1;
                        e.record.data.UnitMultDiv = 'M';
                        e.record.data.ClassID = invt.ClassID;
                    } else {
                        return;
                    }
                }
            }

            if (key == 'InvtID') {
                HQ.common.showBusy(true, 'Process...');
                //var objInvt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]);
                //if (objInvt) {
                //    e.record.set('ClassID',objInvt.ClassID);
                //}
                HQ.numTrans = 0;
                HQ.maxTrans = 3;
                App.stoUnit.load({
                    params: { invtID: e.record.data.InvtID },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoItemSite.load({
                    params: { siteID: App.SiteID.getValue(), invtID: e.record.data.InvtID }, callback: checkSourceEdit, row: e
                });
                App.stoPrice.load({
                    params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.DateEnt.getValue(), valMthd: invt.ValMthd, siteID: App.SiteID.getValue() }, callback: checkSourceEdit, row: e
                });
            } else if (key == 'UnitDesc') {
                HQ.common.showBusy(true, 'Process...');
                HQ.numTrans = 0;
                HQ.maxTrans = 2;
                App.stoItemSite.load({
                    params: { siteID: App.SiteID.getValue(), invtID: e.record.data.InvtID },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoPrice.load({
                    params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.DateEnt.getValue(), valMthd: invt.ValMthd, siteID: App.SiteID.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
            } else {
                checkExitEdit(e);
            }
        }
    }
};
var grdTrans_ValidateEdit = function (item, e) {
};

var grdLot_BeforeEdit = function (item, e) {
    if (App.grdLot.isLock) {
        return false;
    }
    if (e.field == 'Qty' && HQ.currentDet && HQ.currentDet.data.LotSerTrack == _lotQ) {
        return false;
    }
    if (e.field == 'PackageID' && (e.record.data.tstamp != '' || e.record.data.PackageID != '') && HQ.currentDet && HQ.currentDet.data.LotSerTrack == _lotQ) {
        return false;
    }
    var key = e.field;
    var record = e.record;

    if (key == 'LotSerNbr' && !Ext.isEmpty(record.data.LotSerNbr)) return false;
    if (Ext.isEmpty(record.data.InvtID)) {
        record.data.InvtID = App.winLot.record.data.InvtID;
        record.data.SiteID = App.winLot.record.data.SiteID;
        record.data.InvtMult = App.winLot.record.data.InvtMult;
        record.data.TranType = App.winLot.record.data.TranType;
    }

    record.commit();
    if (key == 'ExpDate') {
        if (record.data.tstamp)
            return false;
        else {
            if (e.record.data.LotSerNbr) {
                var objFind = App.stoCalcLot.findRecord(['LotSerNbr'], [e.record.data.LotSerNbr]);
                if (objFind)
                    return false;
                else
                    return true;
            }

        }
    }
};
var grdLot_SelectionChange = function (item, selected) {
    HQ.focus = 'lot';
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectLot = 0;
            HQ.maxSelectLot = 1;
            HQ.common.showBusy(true, 'Process...');
            App.stoItemLot.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.BranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, batNbr: App.DateEnt.getValue() },
                callback: checkSelectLot,
                row: selected[0]
            });
        } else {
            App.lblLotQtyAvail.setText('');
        }
    }
};
var grdLot_Edit = function (item, e) {
    HQ.focus = 'lot';
    var key = e.field;
    var lot = e.record.data;
    var record = e.record;
    if (Object.keys(e.record.modified).length > 0) {
        if (key == "Qty" || key == "UnitDesc") {
            checkExitEditLot(e);
        } else if (key == "LotSerNbr") {
            HQ.common.showBusy(true, 'Process...');
            HQ.numLot = 0;
            HQ.maxLot = 1;

            var invt = HQ.currentDet.invt;
            if (invt && invt.LotSerTrack == _lotQ) {
                var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, 'PAC');
                if (!Ext.isEmpty(cnv)) {
                    e.record.data.Qty = cnv.CnvFact;
                } else {
                    return;
                }
            }

            App.stoItemLot.load({
                params: { siteID: lot.SiteID, invtID: lot.InvtID, branchID: App.BranchID.getValue(), lotSerNbr: lot.LotSerNbr },
                callback: checkSourceEditLot,
                row: e
            });
        }
        //else if (e.field == 'PackageID') {

        //}
    }
};

var renderRowNumber = function (value, meta, record) {
    return App.stoLotTrans.data.indexOf(record) + 1;
};
var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
};
var renderQtyAmt2 = function (value) {
    return Ext.util.Format.number(value, '0,000.00');
};

//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


//// Function ////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

///// data ///////////////////////

var bindTran = function () {
    if (App.stoTrans.data.items.length > 0) {
        var first = App.stoTrans.data.items[0].data;
        App.SiteID.setValue(first.SiteID);
        App.SlsperID.setValue(first.SlsperID);

        App.TrnsferNbr.events['change'].suspend();
        if (first.TranType == "TR" && first.InvtMult == 1) {
            App.TrnsferNbr.forceSelection = false;
            App.TrnsferNbr.setValue(first.RefNbr);
            HQ.isTransfer = true;
        } else {
            HQ.isTransfer = false;
            App.TrnsferNbr.setValue('');
        }
    } else {
        App.TrnsferNbr.setValue('');
        HQ.isTransfer = false;
    }
    App.lblQtyAvail.setText('');
    App.TrnsferNbr.events['change'].resume();

    if (HQ.isInsert)
        HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);

    App.TrnsferNbr.setReadOnly(!Ext.isEmpty(App.BatNbr.getValue()));
    checkTransAdd();
    calculate();
    App.grdTrans.isChange = false;
    HQ.common.showBusy(false, HQ.waitMsg);
    setChange(false);
};
var bindBatch = function (record) {
    HQ.objBatch = record;
    App.BatNbr.events['change'].suspend();
    App.SiteID.events['change'].suspend();
    App.FromToSiteID.events['change'].suspend();
    App.frmMain.events['fieldchange'].suspend();
    App.Status.forceSelection = false;
    App.frmMain.loadRecord(HQ.objBatch);
    App.Status.forceSelection = false;
    App.frmMain.events['fieldchange'].resume();
    App.BatNbr.events['change'].resume();
    App.SiteID.events['change'].resume();
    App.FromToSiteID.events['change'].resume();
    setStatusForm();
    HQ.common.showBusy(true, HQ.waitMsg);
    HQ.numDetail = 0;
    App.TrnsferNbr.store.reload();
    App.stoTrans.reload();
    App.stoLotTrans.reload();
    App.Handle.setValue('N');
};
var bindTransfer = function () {
    HQ.isTransfer = true;
    App.stoTrans.removeAll();

    App.stoTransfer.data.items.forEach(function (item) {
        var newTrans = Ext.create('App.mdlTransfer');
        newTrans.data.JrnlType = item.data.JrnlType;
        newTrans.data.ReasonCD = item.data.ReasonCD;
        newTrans.data.TranAmt = item.data.TranAmt;
        newTrans.data.BatNbr = '';
        newTrans.data.BranchID = App.BranchID.getValue();
        newTrans.data.BarCode = item.data.BarCode;
        newTrans.data.CnvFact = item.data.CnvFact;
        newTrans.data.ExtCost = item.data.ExtCost;
        newTrans.data.InvtID = item.data.InvtID;
        newTrans.data.InvtMult = item.data.InvtMult;
        newTrans.data.LineRef = item.data.LineRef;
        newTrans.data.ObjID = item.data.ObjID;
        newTrans.data.Qty = item.data.Qty;
        newTrans.data.RefNbr = item.data.RefNbr;
        newTrans.data.Rlsed = item.data.Rlsed;
        newTrans.data.ShipperID = item.data.ShipperID;
        newTrans.data.ShipperLineRef = item.data.ShipperLineRef;
        newTrans.data.SiteID = item.data.ToSiteID;
        newTrans.data.SlsperID = item.data.SlsperID;
        newTrans.data.TranDate = item.data.TranDate;
        newTrans.data.TranDesc = item.data.TranDesc;
        newTrans.data.TranFee = item.data.TranFee;
        newTrans.data.TranType = item.data.TranType;
        newTrans.data.UnitCost = item.data.UnitCost;
        newTrans.data.UnitDesc = item.data.UnitDesc;
        newTrans.data.UnitMultDiv = item.data.UnitMultDiv;
        newTrans.data.UnitPrice = item.data.UnitPrice;
        newTrans.commit();
        HQ.store.insertRecord(App.stoTrans, "InvtID", newTrans, true);
    });

    App.stoLotTrans.removeAll();
    App.stoLotTransfer.data.items.forEach(function (item) {
        var newLot = Ext.create('App.mdlLotTrans');
        newLot.data.BranchID = App.BranchID.getValue();
        newLot.data.BatNbr = '';
        newLot.data.RefNbr = '';
        newLot.data.LotSerNbr = item.data.LotSerNbr;
        newLot.data.INTranLineRef = item.data.INTranLineRef;
        newLot.data.ExpDate = item.data.ExpDate;
        newLot.data.InvtID = item.data.InvtID;
        newLot.data.InvtMult = item.data.InvtMult;
        newLot.data.MfgrLotSerNbr = item.data.MfgrLotSerNbr;
        newLot.data.Qty = item.data.Qty;
        newLot.data.SiteID = item.data.ToSiteID;
        newLot.data.SlsperID = item.data.SlsperID;
        newLot.data.TranDate = item.data.TranDate;
        newLot.data.WarrantyDate = item.data.WarrantyDate;
        newLot.data.TranType = 'RC';
        newLot.data.UnitCost = item.data.UnitCost;
        newLot.data.UnitDesc = item.data.UnitDesc;
        newLot.data.UnitMultDiv = item.data.UnitMultDiv;
        newLot.data.UnitPrice = item.data.UnitPrice;
        newLot.data.CnvFact = item.data.CnvFact;
        newLot.commit();
        HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newLot, true);
    });
    if (App.stoTransfer.data.items.length > 0) {
        App.FromToSiteID.setValue(App.stoTransfer.data.items[0].data.SiteID);
        App.SiteID.setValue(App.stoTransfer.data.items[0].data.ToSiteID);
        if (Ext.isEmpty(App.ReasonCD.getValue())) {
            App.ReasonCD.setValue(App.stoTransfer.data.items[0].data.ReasonCD);
        }
        if (Ext.isEmpty(App.SlsperID.value)) {
            App.SlsperID.setValue(App.stoTransfer.data.items[0].data.SlsperID);
        }
    }
    HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);
    checkTransAdd();
    calculate();
};
var save = function () {
    var i = 0;
    var errorMessage = '';
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        i++;
        if(record.data.ClassID == 'POSM')
            if (!record.data.PosmID) {
                errorMessage += i + ', ';
            }
    });
    if (errorMessage) {
        HQ.message.show(2016033001, [errorMessage], '', true);
        return;
    }

    if ((App.BatNbr.value && !HQ.isUpdate) || (Ext.isEmpty(App.BatNbr.value) && !HQ.isInsert)) {
        HQ.message.show(728, '', '', true);
        return;
    }
    if (App.Status.getValue() != "H" && (App.Handle.getValue() == "N" || Ext.isEmpty(App.Handle.getValue()))) {
        HQ.message.show(2015020803, '', '', true);
        return;
    }
    if (App.BatNbr.value && App.JrnlType.getValue() == 'PO') {
        HQ.message.show(2015020801, [App.BatNbr.value], '', true);
        return;
    }
    var store1 = App.stoTrans;
    var allRecords1 = store1.snapshot || store1.allData || store1.data;

    if (allRecords1.items.length <= 1) {
        HQ.message.show(2015020804, [App.BatNbr.value], '', true);
        return;
    }
    var flat = false;
    App.stoLotTrans.clearFilter();
    allRecords1.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
            if (Ext.isEmpty(item.data.UnitDesc)) {
                HQ.message.show(1000, [HQ.common.getLang('Unit')], '', true);
                App.smlTrans.select(App.stoTrans.indexOf(HQ.store.findInStore(App.stoTrans, ['UnitDesc'], [item.data.UnitDesc])));
                flat = true;
                return false;
            }
            if (item.data.Qty == 0) {
                HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
                App.smlTrans.select(App.stoTrans.indexOf(HQ.store.findInStore(App.stoTrans, ['LineRef'], [item.data.LineRef])));
                flat = true;
                return false;
            }
            if (Ext.isEmpty(item.data.SiteID)) {
                HQ.message.show(1000, [HQ.common.getLang('siteid')], '', true);
                App.smlTrans.select(App.stoTrans.indexOf(HQ.store.findInStore(App.stoTrans, ['LineRef'], [item.data.LineRef])));
                flat = true;
                return false;
            }
            if (Ext.isEmpty(item.invt)) {
                item.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [item.data.InvtID]);
            }
            if (!Ext.isEmpty(item.invt) && item.invt.LotSerTrack != "N" && !Ext.isEmpty(item.invt.LotSerTrack)) {
                var lotQty = 0;
                var lotFlat = false;
                App.stoLotTrans.data.each(function (item2) {
                    if (item.data.LineRef == item2.data.INTranLineRef && !Ext.isEmpty(item2.data.LotSerNbr)) {
                        if (item.data.InvtID != item2.data.InvtID) {
                            HQ.message.show(2015040501, [item.data.InvtID], "", true);
                            lotFlat = true;
                            return false;
                        }

                        if (item.data.SiteID != item2.data.SiteID) {
                            HQ.message.show(2015040501, [item.data.InvtID], "", true);
                            lotFlat = true;
                            return false;
                        }

                        lotQty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty / item2.data.CnvFact;
                    }
                });
                if (lotFlat) {
                    flat = item;
                    return false;
                }

                if (item.invt.LotSerTrack == _lotQ) {
                    var detQty = item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                } else {
                    var detQty = Math.round(item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact);
                }
               
                if (detQty != lotQty) {
                    HQ.message.show(2015040502, [item.data.InvtID], "", true);
                    flat = item;
                    return false;
                }
            }

        }
    });
    if (flat) {
        return;
    }
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            method: 'POST',
            url: 'IN10100/Save',
            timeout: 180000,
            params: {
                lstTrans: Ext.encode(App.stoTrans.getRecordsValues()),
                lstLot: Ext.encode(App.stoLotTrans.getRecordsValues()),
                isTransfer: HQ.isTransfer
            },
            success: function (msg, data) {
                if (HQ.isTransfer) {
                    App.TrnsferNbr.store.reload();
                }
                var batNbr = '';

                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    var batNbr = this.result.data.batNbr
                }
                if (!Ext.isEmpty(batNbr)) {
                    App.BatNbr.forceSelection = false
                    App.BatNbr.events['change'].suspend();
                    App.BatNbr.setValue(batNbr);
                    App.BatNbr.events['change'].resume();
                    if (Ext.isEmpty(HQ.recentRecord)) {
                        HQ.recentRecord = batNbr;
                    }
                }


                HQ.message.process(msg, data, true);

                if (!Ext.isEmpty(App.BatNbr.getValue())) {
                    App.stoBatch.reload();
                } else {
                    defaultOnNew();
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteHeader = function (item) {
    if (item == 'yes') {
        if (Ext.isEmpty(App.BatNbr.getValue())) {
            menuClick('new');
        } else {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'IN10100/Delete',
                timeout: 180000,
                success: function (msg, data) {
                    var record = App.stoBatch.getById(App.BatNbr.getValue());
                    if (!Ext.isEmpty(record)) {
                        App.stoBatch.remove(record);
                    }
                    setChange(false);
                    HQ.message.process(msg, data, true);
                    menuClick('new');
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};
var deleteTrans = function (item) {
    if (item == 'yes') {
        if (App.BatNbr.value) {
            App.stoLotTrans.clearFilter();
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'IN10100/DeleteTrans',
                timeout: 180000,
                params: {
                    lineRef: App.smlTrans.selected.items[0].data.LineRef
                },
                success: function (msg, data) {
                    if (!Ext.isEmpty(data.result.data.tstamp)) {
                        App.tstamp.setValue(data.result.data.tstamp);
                    }
                    App.grdTrans.deleteSelected();
                    calculate();
                    HQ.message.process(msg, data, true);
                    HQ.isChange = false;
                    HQ.common.changeData(HQ.isChange, 'IN10100');
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
        else {
            App.grdTrans.deleteSelected();
            calculate();
        }
    }
};
var deleteLot = function (item) {
    if (item == 'yes') {
        App.grdLot.deleteSelected();
    }
};
var report = function () {
    App.frmMain.submit({
        waitMsg: HQ.waitMsg,
        clientValidation: false,
        method: 'POST',
        url: 'IN10100/Report',
        timeout: 180000,
        success: function (msg, data) {
            if (this.result.reportID != null) {
                window.open('Report?ReportName=' + this.result.reportName + '&_RPTID=' + this.result.reportID, '_blank');
            }
            processMessage(msg, data, true);
        },
        failure: function (msg, data) {
            processMessage(msg, data, true);
        }
    });
};
var calcLot = function (record, show) {
    if (!Ext.isEmpty(record.invt) && !Ext.isEmpty(record.invt.LotSerTrack) && record.invt.LotSerTrack != 'N' && !Ext.isEmpty(record.data.UnitDesc)) {
        App.stoLotTrans.clearFilter();
        App.stoLotTrans.data.each(function (item) {
            if (item.data.INTranLineRef == record.data.LineRef) {
                item.data.UnitDesc = record.data.UnitDesc;
                item.data.CnvFact = record.data.CnvFact;
                item.data.UnitMultDiv = record.data.UnitMultDiv;
                item.data.QtyOnHand = record.data.QtyOnHand;
                item.data.UnitCost = item.data.UnitPrice = record.data.UnitPrice;
                item.commit();
            }
        });
        if (show == true) {
            showLot(record, true);
        }

    }
};
var showLot = function (record, loadCombo) {
    HQ.currentDet = record;
    var lock = !((App.BatNbr.value && HQ.isUpdate) || (!App.BatNbr.value && HQ.isInsert)) || App.Status.getValue() != "H";
    App.grdLot.isLock = lock;
    if (loadCombo) {
        App.stoCalcLot.load({
            params: {
                siteID: record.data.SiteID,
                invtID: record.data.InvtID,
                branchID: App.BranchID.getValue(),
                batNbr: App.BatNbr.getValue(),
            }
        });
    }


    App.stoLotTrans.clearFilter();
    App.stoLotTrans.filter('INTranLineRef', record.data.LineRef);

    var newRow = Ext.create('App.mdlLotTrans');
    newRow.data.INTranLineRef = record.data.LineRef;
    newRow.data.UnitDesc = record.data.UnitDesc;
    newRow.data.ExpDate = App.DateEnt.getValue();
    newRow.data.UnitPrice = record.data.UnitPrice;
    newRow.data.CnvFact = record.data.CnvFact;
    newRow.data.UnitMultDiv = record.data.UnitMultDiv;
    HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);

    App.winLot.record = record;
    App.grdLot.view.refresh();
    App.winLot.setTitle(record.data.InvtID + ' ' + (record.data.UnitMultDiv == "M" ? record.data.Qty * record.data.CnvFact : record.data.Qty / record.data.CnvFact) + ' ' + record.invt.StkUnit);
    HQ.focus = '';
    App.winLot.show();
    setTimeout(function () {
        App.winLot.toFront();
        if (HQ.showQtyOnhand) {
            if (App.colLotQtyOnHand) {
                App.colLotQtyOnHand.show();
            }
            
        }
        var discAmtCol = App.grdLot.down('[dataIndex=Qty]');
        var discAmtField = discAmtCol.getEditor().field;
        if (HQ.currentDet.data.LotSerTrack == _lotQ) {
            App.colPackageID.show();
            discAmtCol.format = "0,000.00";
        } else {
            discAmtCol.format = "0,000";
            App.colPackageID.hide();
        }
        
    }, 50);
};
//////////////////////////////////
var calculate = function () {
    var totAmt = 0;
    var totQty = 0;
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;

    allRecords.each(function (item) {
        totAmt += item.data.TranAmt;
        totQty += item.data.Qty;
    });

    App.TotAmt.setValue(totAmt);
    App.TotQty.setValue(totQty);

};

var defaultOnNew = function () {
    HQ.isNew = true;
    var record = Ext.create('App.mdlBatch');
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'H';
    record.data.DateEnt = HQ.businessDate;
    App.SiteID.setValue(HQ.inSite);
    App.SlsperID.setValue('');
    App.TrnsferNbr.forceSelection = true;

    App.frmMain.validate();

    bindBatch(record);
};

var lastLineRef = function () {
    var num = 0;
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;

    allRecords.each(function (item) {
        if (!Ext.isEmpty(item.data.LineRef) && parseInt(item.data.LineRef) > num) {
            num = parseInt(item.data.LineRef);
        }
    });

    num++;
    var lineRef = num.toString();
    var len = lineRef.length;
    for (var i = 0; i < 5 - len; i++) {
        lineRef = "0" + lineRef;
    }
    return lineRef;
};

var setUOM = function (invtID, classID, stkUnit, fromUnit) {
    if (!Ext.isEmpty(fromUnit)) {
        var data = HQ.store.findInStore(App.stoUnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["3", "*", invtID, fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }

        data = HQ.store.findInStore(App.stoUnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["2", classID, "*", fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }

        data = HQ.store.findInStore(App.stoUnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["1", "*", "*", fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }
        HQ.message.show(2525, [invtID], '', true);
        return null;
    }
    return null;
};

var rdrTrans_QtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
};

var rendererQty = function (value, meta, record) {
    if (record.data.LotSerTrack == _lotQ) {
        return Ext.util.Format.number(value, '0,000.00');
    } else {
        return Ext.util.Format.number(value, '0,000');
    }
}

var rendererLotQty = function (value, meta, record) {
    if (HQ.currentDet && HQ.currentDet.data.LotSerTrack == _lotQ) {
        return Ext.util.Format.number(value, '0,000.00');
    } else {
        return Ext.util.Format.number(value, '0,000');
    }
}

var setStatusForm = function () {
    var lock = true;
    var flag = true;
    if (!Ext.isEmpty(HQ.objBatch.data.BatNbr)) {
        if (HQ.objBatch.data.JrnlType == 'PO') {
            lock = true;
        } else if (HQ.objBatch.data.Status == 'H') {
            lock = false;
        }
        
    } else {
        lock = !HQ.isInsert;
        App.btnImport.setDisabled(false);
    }

    App.btnImport.setDisabled(App.Status.getValue() != 'H');

    App.BatNbr.setReadOnly(false);
    App.Handle.setReadOnly(false);
    App.RvdBatNbr.setReadOnly(true);
    App.Status.setReadOnly(true);
    App.RefNbr.setReadOnly(true);
    App.TotQty.setReadOnly(true);
    App.TotAmt.setReadOnly(true);
    App.BranchID.setReadOnly(true);

    if (!HQ.isInsert && HQ.isNew) {
        lock = true;
        flag = false;
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        lock = true;
        flag = false;
        App.btnImport.setDisabled(true);
    }
    
    HQ.common.lockItem(App.frmMain, lock);
    App.grdTrans.isLock = lock;
    if (flag == true && App.Status.getValue() != 'H') {
        App.Handle.setReadOnly(false);
    }
};

var checkExitEdit = function (row) {
    var flagPosm = false;
    if (row.record.data.ClassID == "POSM")
        flagPosm = true;
    else
        flagPosm = false;
    var key = row.field;
    var trans = row.record.data;
    if (key == 'InvtID' || key == 'BarCode') {

        trans.ReasonCD = App.ReasonCD.getValue();
        trans.SiteID = App.SiteID.getValue();

        var invt = row.record.invt;
        if (invt.LotSerTrack != _lotQ) {
            var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);

            if (Ext.isEmpty(cnv)) {
                trans.UnitMultDiv = '';
                trans.UnitPrice = 0;
                trans.TranAmt = 0;
                row.record.commit();
                HQ.common.showBusy(false);
                return;
            }
            trans.UnitDesc = invt.StkUnit;
            trans.CnvFact = cnv.CnvFact == 0 ? 1 : cnv.CnvFact;
            trans.UnitMultDiv = cnv.MultDiv;
            
        } else {
            var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, 'PAC');

            if (Ext.isEmpty(cnv)) {
                trans.UnitMultDiv = '';
                trans.UnitPrice = 0;
                trans.TranAmt = 0;
                row.record.commit();
                HQ.common.showBusy(false);
                return;
            }
           
            trans.CnvFact = 1;
            trans.UnitMultDiv = 'M';
        }
        trans.LotSerTrack = invt.LotSerTrack;
        trans.UnitDesc = invt.StkUnit;
        trans.TranDesc = invt.Descr;
        trans.BarCode = invt.BarCode;
       

        var site = HQ.store.findInStore(App.stoItemSite, ['SiteID', 'InvtID'], [trans.SiteID, trans.InvtID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
        }
        //if (invt.ValMthd == "A" || invt.ValMthd == "E") {
        //    trans.UnitPrice = Math.round(site.AvgCost, 0);
        //} else {
            trans.UnitPrice = App.stoPrice.data.items[0].data.Price;
        //}
        if(flagPosm == false)
            trans.TranAmt = trans.Qty * trans.UnitPrice;
        trans.ClassID = invt.ClassID;
        getQtyAvail(row.record);

    } else if (key == 'UnitDesc') {

        var invt = row.record.invt;

        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, trans.UnitDesc);

        if (Ext.isEmpty(cnv)) {
            trans.UnitMultDiv = '';
            trans.UnitPrice = 0;
            trans.TranAmt = 0;
            row.record.commit();
            HQ.common.showBusy(false);
            return;
        }

        var site = HQ.store.findInStore(App.stoItemSite, ['SiteID', 'InvtID'], [trans.SiteID, trans.InvtID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
        }

        trans.CnvFact = cnv.CnvFact;
        trans.UnitMultDiv = cnv.MultDiv;

        //if (invt.ValMthd == "A" || invt.ValMthd == "E") {
        //    trans.UnitPrice = Math.round(trans.UnitMultDiv == 'M' ? site.AvgCost * trans.CnvFact : site.AvgCost / trans.CnvFact, 0);
        //} else {
            trans.UnitPrice = App.stoPrice.data.items[0].data.Price;
        //}
        if(flagPosm == false)
            trans.TranAmt = trans.Qty * trans.UnitPrice;
        calcLot(row.record, false);
    }

    else if (key == "Qty") {
        if (flagPosm == false)
            trans.TranAmt = trans.Qty * trans.UnitPrice;
        calcLot(row.record, true);
    }

    else if (key == "UnitPrice") {
        if (flagPosm == false)
            trans.TranAmt = trans.Qty * trans.UnitPrice;
        calcLot(row.record, false);
    }

    trans.ExtCost = trans.TranAmt;
    trans.UnitCost = trans.UnitPrice;
    row.record.commit();

    if (key == 'InvtID' && !Ext.isEmpty(trans.InvtID)) {
        HQ.store.insertRecord(App.stoTrans, key, Ext.create('App.mdlTrans'), true);
    }

    calculate();

    checkTransAdd();

    HQ.common.showBusy(false);
};

var checkTransAdd = function () {
    var flat = false;
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;

    allRecords.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
            flat = true;
            return false;
        }
    });
    if (!HQ.isInsert && HQ.isNew) {
        flat = true;
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        flat = true;
    }
    App.SlsperID.setReadOnly(App.Status.getValue() != 'H');
    App.FromToSiteID.setReadOnly(HQ.isTransfer || App.Status.getValue() != 'H');
    App.SiteID.setReadOnly(flat);
    App.FromToSiteID.setReadOnly(flat);
};

var getQtyAvail = function (row) {
    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [row.data.InvtID, row.data.SiteID]);
    var qtyOnhand = 0;
    if (!Ext.isEmpty(site)) {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + site.QtyAvail);
        qtyOnhand = site.QtyOnHand;
    }
    else {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
    }
    row.set('QtyOnHand', qtyOnhand);
};
var getLotQtyAvail = function (row) {
    var lot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', ['LotSerNbr']], [row.data.InvtID, row.data.SiteID, row.data.LotSerNbr]);
    var qtyOnhand = 0;
    if (!Ext.isEmpty(lot)) {
        App.lblLotQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + lot.QtyAvail);
        qtyOnhand = lot.QtyOnHand;
    }
    else {
        App.lblLotQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + 0);
    }
    row.set('QtyOnHand', qtyOnhand);
};

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
var askNew = function (item) {
    if (item == "yes" || item == "ok") {
        defaultOnNew();
    }
};
var askRefresh = function (item) {
    if (item == "yes" || item == "ok") {
        if (!Ext.isEmpty(App.BatNbr.getValue())) {
            App.stoBatch.reload();
        } else {
            defaultOnNew();
        }
    }
}; 
var setChange = function (isChange) {   
    HQ.isChange = isChange;
    if (isChange) {
        App.BatNbr.setReadOnly(true);
    } else {
        App.grdTrans.isChange = false;
        App.BatNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'IN10100');
};

var tickToDate = function (ticks) {
    //ticks are in nanotime; convert to microtime
    var ticksToMicrotime = ticks / 10000;

    //ticks are recorded from 1/1/1; get microtime difference from 1/1/1/ to 1/1/1970
    var epochMicrotimeDiff = 2208988800000;

    //new date is ticks, converted to microtime, minus difference from epoch microtime
    return new Date(ticksToMicrotime - epochMicrotimeDiff);
};
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
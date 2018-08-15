HQ.recentRecord = null;
HQ.focus = 'batch';
HQ.objBatch = null;
HQ.isTransfer = false;
HQ.objUser = null;
var _siteID = '';
//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var stoUserDefault_Load = function () {
    if (App.stoUserDefault.data.items.length == 0) {
        HQ.objUser = Ext.create('App.mdlUserDefault').data;
    } else {
        HQ.objUser = App.stoUserDefault.data.items[0].data;
    }
    HQ.numSource++;
    checkSetDefault();
}

var stoSetup_Load = function () {
    if (App.stoSetup.data.items.length == 0) {
        HQ.objSetup = Ext.create('App.mdlSetup').data;
    } else {
        HQ.objSetup = App.stoSetup.data.items[0].data;
    }
    HQ.numSource++;
    checkSetDefault();
}
var stoTrans_BeforeLoad = function () {
}
var stoHandle_Load = function () {
    App.cboHandle.setValue('N');
}
var stoDetail_Load = function () {
    checkSourceDetail();
}
var store_Load = function () {
    HQ.numSource++;
    checkSetDefault();
}
var stoBatch_Load = function () {
    
    var record = App.stoBatch.getById(App.cboBatNbr.getValue());
    if (record) {
        bindBatch(record);
    }
}
var stoBatch_BeforeLoad = function (store, operation, eOpts) {
    //if (Ext.isEmpty(operation.params.query)) {
    //   // operation.params.query = App.cboBatNbr.getValue();
    //}

}

var checkSelect = function (records, options, success) {
    HQ.numSelectTrans++;
    if (HQ.numSelectTrans == HQ.maxSelectTrans) {
        HQ.common.showBusy(false);
        getQtyAvail(options.row);
    }
}
var checkSetDefault = function () {
    if (HQ.numSource == HQ.maxSource) {
        defaultOnNew();
    }
}
var checkSourceEdit = function (records, options, success) {
    HQ.numEditTrans++;
    if (HQ.numEditTrans == HQ.maxEditTrans) {
        checkExitEdit(options.row);
    }
}
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
}
var checkSelectLot = function (records, options, success) {
    HQ.numSelectLot++;
    if (HQ.numSelectLot == HQ.maxSelectLot) {
        HQ.common.showBusy(false);
        getLotQtyAvail(options.row);
    }
}
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////



//// Event ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var frmMain_BoxReady = function () {
    App.cboSlsperID.allowBlank = !HQ.allowSlsper;
    HQ.util.checkAccessRight();
    if (showFromSite) {
        App.cboFromToSiteID.show();
        App.hideLabel.hide();
    }
    else {
        App.cboFromToSiteID.hide();
        App.hideLabel.show();
    }

    
    HQ.numDetail = 0;
    HQ.maxDetail = 2;
    HQ.numSource = 0;
    HQ.maxSource = 8;
    HQ.numEditTrans = 0;
    HQ.maxEditTrans = 4;
    HQ.numSelectTrans = 0;
    HQ.maxSelectTrans = 3;
    App.cboBatNbr.key = true;
    App.txtBranchID.setValue(HQ.cpnyID);
    App.cboHandle.getStore().addListener('load', stoHandle_Load);
    App.cboStatus.getStore().addListener('load', store_Load);
    App.cboTransInvtID.getStore().addListener('load', store_Load);
    App.cboFromToSiteID.getStore().addListener('load', store_Load);
    App.cboSlsperID.getStore().addListener('load', store_Load);
    App.cboSiteID.getStore().addListener('load', store_Load);
    App.cboReasonCD.getStore().addListener('load', store_Load);
    App.stoUserDefault.addListener('load', stoUserDefault_Load);
    App.stoSetup.addListener('load', stoSetup_Load);
    App.stoUnitConversion.addListener('load', store_Load);
    App.cboPerPost.store.addListener('load', store_Load);

    App.stoSetup.load();
    App.stoUserDefault.load();
    App.stoUnitConversion.load();
    App.cboTransInvtID.store.reload();
    App.stoInvt = App.cboTransInvtID.getStore();

    
    //App.cboPerPost.store.reload();

    App.smlTrans.tab = false;
    if (HQ.showWhseLoc == 0) {
        App.cboWhseLoc.setVisible(false);
        App.cboWhseLoc.allowBlank = true;
    }
    else {
        if (HQ.showWhseLoc == 1) {
            App.cboWhseLoc.setVisible(true);
            App.cboWhseLoc.allowBlank = true;
        }
        else {
            App.cboWhseLoc.setVisible(true);
            App.cboWhseLoc.allowBlank = false;
        }
    }
    App.cboWhseLoc.isValid();
    App.smlTrans.onEditorTab = function (field, e) {
        App.smlTrans.tab = true;
        if (field.activeColumn.dataIndex == 'Qty' || field.activeColumn.dataIndex == 'UnitDesc' || field.activeColumn.dataIndex == 'InvtID') {
            field.activeEditor.completeEdit();
        } else {
            origEditorTab(field,e);
        }
      
    }
    App.btnImport.setVisible(HQ.showImport);
    App.btnExport.setVisible(HQ.showExport);
    HQ.common.showBusy(true, HQ.waitMsg);
    if (HQ.showQtyOnhand) {
        App.colQtyOnHand.show();
        if (App.colLotQtyOnHand) {
            App.colLotQtyOnHand.show();
        }
    }
}


var origEditorTab = function(instance, self){
    var evt = this;
    var view = instance.context.view;
    var cell = instance.getActiveRecord();
    var options = instance.getActiveColumn();
    var content = view.getPosition(cell, options);
    /** @type {string} */
    var presence_h_tag = self.shiftKey ? "left" : "right";
    do {
        content = view.walkCells(content, presence_h_tag, self, evt.preventWrap);
    } while (content && (!content.columnHeader.getEditor(cell) || !instance.startEditByPosition(content)));
}
var frmMain_FieldChange = function (item, field, newValue, oldValue) {
    if (field.key != undefined) {
        return;
    }
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (Object.keys(App.stoBatch.getChangedData()).length > 0 || Ext.isEmpty(App.cboBatNbr.getValue()) || App.grdTrans.isChange == true) {
        setChange(true);
    } else {
        setChange(false);
    }

}
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'batch') {
                if (HQ.isChange) {
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
                if (HQ.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    var index = App.stoBatch.indexOf(App.stoBatch.getById(App.cboBatNbr.getValue()));
                    App.cboBatNbr.setValue(App.stoBatch.getAt(index + 1).get('BatNbr'));
                }
            } else if (HQ.focus == 'trans') {
                HQ.grid.next(App.grdTrans);
            }
            break;
        case "prev":
            if (HQ.focus == 'batch') {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    var index = App.stoBatch.indexOf(App.stoBatch.getById(App.cboBatNbr.getValue()));
                    App.cboBatNbr.setValue(App.stoBatch.getAt(index - 1).get('BatNbr'));
                }
            } else if (HQ.focus == 'trans') {
                HQ.grid.prev(App.grdTrans);
            }
            break;
        case "last":
            if (HQ.focus == 'batch') {
                if (HQ.isChange) {
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
                    var checkPerPost = true;
                    if (HQ.checkPerPost) {
                        var objPerPost = HQ.store.findRecord(App.cboPerPost.store, ['CycleNbr'], [App.cboPerPost.getValue()]);
                        if (objPerPost != undefined) {
                            var tam = App.txtDateEnt.getValue();
                            if (tam > objPerPost.data.EndDate || tam < objPerPost.data.StartDate) {
                                checkPerPost = false;
                            }
                        }
                        else {
                            checkPerPost = false;
                        }
                    }
                    if (HQ.checkPerPost && !checkPerPost) {
                        HQ.message.show(2018081511, '', 'checkSave');
                    }
                    else {
                        save();
                    }                    
                }
            }
            break;
        case "delete":
            if (HQ.focus == 'batch') {
                if (App.cboBatNbr.value) {
                    if (HQ.isDelete) {
                        if (App.cboStatus.getValue() != 'H') {
                            HQ.message.show(2015020805, [App.cboBatNbr.value], '', true);
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
                if ((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != "H") {
                        HQ.message.show(2015020805, [App.cboBatNbr.value], '', true);
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
            //if (HQ.isChange) {
            //    HQ.message.show(2015030201, '', "askNew", true);
            //} else {
            //    defaultOnNew();
            //}

            if (HQ.focus == 'batch') {
                if (HQ.isChange || App.grdTrans.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    defaultOnNew();
                }
            } else if (HQ.focus == 'trans') {
                eventNew(App.stoTrans, App.grdTrans, 'InvtID');
            }
            break;
        case "refresh":
            if ((HQ.isChange || App.grdTrans.isChange)) {
                HQ.message.show(20150303, '', "askRefresh", true);
            } else {
                if (!Ext.isEmpty(App.cboBatNbr.getValue())) {
                    App.stoBatch.reload();
                } else {
                    defaultOnNew();
                }
            }
            break;
        case "print":
            if (!Ext.isEmpty(App.cboBatNbr.getValue()) && App.cboStatus.value != "H") {
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

var cboBatNbr_Change = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.cboReasonCD.store.reload();
    }
    var record = App.stoBatch.getById(newValue);
    if (record) {
        HQ.isNew = false;
        bindBatch(record);
    } else {
        if (HQ.recentRecord != record) {
            App.cboBatNbr.store.reload();            
            //App.cboPerPost.setValue(HQ.perpost);
        }
    }
    HQ.recentRecord = record;
};


var cboStatus_Change = function (item, newValue, oldValue) {
    App.cboHandle.getStore().reload();
};

var cboFromToSiteID_Change = function (item, newValue, oldValue) {
    if (App.cboFromToSiteID.getValue() == App.cboSiteID.getValue()) {
        App.cboFromToSiteID.setValue('');
    }    
};

var cboSiteID_Change = function (item, newValue, oldValue) {
    App.cboWhseLoc.store.reload();
    if (App.cboFromToSiteID.getValue() == App.cboSiteID.getValue()) {
        App.cboSiteID.setValue('');
    }
    if (item.hasFocus) {
        if (newValue != oldValue) {
            App.cboWhseLoc.setValue("");
            App.cboWhseLoc.store.reload();
        }
    }    
};

var btnLot_Click = function () {
    if (Ext.isEmpty(this.record.invt)) {
        App.cboTransInvtID.store.clearFilter();
        this.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [this.record.data.InvtID]);
    }
    if (!Ext.isEmpty(this.record.invt)) {
        if (this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.UnitDesc)) {
            showLot(this.record, true);
        }
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

        var qty = 0;
        App.stoLotTrans.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                if (item.data.SiteID == det.SiteID && item.data.InvtID == det.InvtID && item.data.INTranLineRef == det.LineRef) {
                    qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                }
            }

        });

        var lineQty = (det.UnitMultDiv == "M" ? qty / det.CnvFact : det.Qty * det.CnvFact)
        if (lineQty % 1 > 0) {
            App.winLot.record.data.Qty = qty;
            App.winLot.record.data.UnitDesc = App.winLot.record.invt.StkUnit;
            App.winLot.record.data.UnitRate = 1;
            App.winLot.record.data.UnitMultDiv = "M";
            if (App.winLot.record.invt.ValMthd == "A" || App.winLot.record.invt.ValMthd == "E") {
                var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [det.InvtID, det.SiteID]);
                price = site.AvgCost;
                App.winLot.record.data.UnitPrice = price;

            } else {
                App.winLot.record.data.UnitPrice = 0;
            }
        } else {
            App.winLot.record.data.Qty = Math.round(lineQty);
        }
        App.winLot.record.data.TranAmt = App.winLot.record.data.ExtCost = App.winLot.record.data.Qty * App.winLot.record.data.UnitPrice;
        App.winLot.record.commit();

        App.grdTrans.view.refresh();


        calculate();
        App.stoLotTrans.clearFilter();
        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr)) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
        if (lineQty != det.StkQty)
            setChange(true);
    }   
    App.winLot.hide();
};

var btnLotDel_Click = function () {
    if ((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) {
        if (App.cboStatus.getValue() != "H") {
            HQ.message.show(2015020805, [App.cboBatNbr.value], '', true);
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
    //if (Ext.isEmpty(App.cboBatNbr.getValue())) {
    //    HQ.message.show('1000', [HQ.common.getLang('batnbr')], '', true);
    //    return;
    //}
    
    App.frmMain.submit({
        url: 'IN10200/Export',
        timeout: 1000000,
        type: 'POST',
        clientValidation: false,
        params: {
            branchID: App.txtBranchID.getValue()

        },
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
        window.location.href = 'IN10200?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
};

var btnCopyCancel_Click = function () {
    App.winCopy.hide();
};

var btnCopyOK_Click = function () {

    var lst = App.txtCopy.value.split('\n');

    if (lst.length > 0) {
        App.winCopy.mask();
        App.frmMain.submit({
            url: 'IN10200/Import',
            timeout: 1000000,
            type: 'POST',
            clientValidation: false,
            params: {
                lineRef: lastLineRef(),
                importData: App.txtCopy.value
            },
            success: function (msg, data) {
                if (this.result.data.lstTrans != undefined) {

                    this.result.data.lstTrans.forEach(function (item) {
                        var newTrans = Ext.create('App.mdlTrans');
                        newTrans.data.JrnlType = 'IN';
                        newTrans.data.ReasonCD = App.cboReasonCD.getValue();
                        newTrans.data.TranAmt = newTrans.data.ExtCost = item.TranAmt;
                        newTrans.data.BranchID = HQ.cpnyID;
                        newTrans.data.CnvFact = item.CnvFact;
                        newTrans.data.ExtCost = item.TranAmt;
                        newTrans.data.InvtID = item.InvtID;
                        newTrans.data.InvtMult = -1;
                        newTrans.data.LineRef = item.LineRef;
                        newTrans.data.Qty = item.Qty;
                        newTrans.data.QtyOnHand = item.QtyOnHand;
                        newTrans.data.SiteID = item.SiteID;
                        newTrans.data.TranDate = App.txtDateEnt.getValue();
                        newTrans.data.TranDesc = item.TranDesc;
                        newTrans.data.TranType = 'II';
                        newTrans.data.UnitDesc = item.UnitDesc;
                        newTrans.data.UnitMultDiv = item.UnitMultDiv;
                        newTrans.data.UnitPrice = newTrans.data.UnitCost = item.UnitPrice;
                        newTrans.commit();
                        App.stoTrans.insert(App.stoTrans.getCount() - 1, newTrans);
                    });

                    App.stoLotTrans.clearFilter();
                    App.stoLotTrans.suspendEvents();
                    this.result.data.lstLot.forEach(function (item) {
                        var newLot = Ext.create('App.mdlLotTrans');
                        newLot.data.LotSerNbr = item.LotSerNbr;
                        newLot.data.BranchID = HQ.cpnyID;
                        newLot.data.INTranLineRef = item.INTranLineRef;
                        newLot.data.ExpDate = new Date(parseInt(item.ExpDate.substr(6)));
                        newLot.data.InvtID = item.InvtID;
                        newLot.data.InvtMult = -1;
                        newLot.data.MfgrLotSerNbr = '';
                        newLot.data.Qty = item.Qty;
                        newLot.data.QtyOnHand = item.QtyOnHand;
                        newLot.data.SiteID = item.SiteID;
                        newLot.data.SlsperID = '';
                        newLot.data.TranDate = App.txtDateEnt.getValue();
                        newLot.data.WarrantyDate = new Date(parseInt(item.WarrantyDate.substr(6)));
                        newLot.data.TranType = 'II';
                        newLot.data.UnitDesc = item.UnitDesc;
                        newLot.data.UnitMultDiv = item.UnitMultDiv;
                        newLot.data.UnitPrice = newLot.data.UnitCost = item.UnitPrice;
                        newLot.data.CnvFact = item.CnvFact;
                        newLot.commit();
                        App.stoLotTrans.insert(App.stoLotTrans.getCount() - 1, newLot);
                    });
                    App.stoLotTrans.resumeEvents();
                    calculate();
                    checkTransAdd();

                    if (!Ext.isEmpty(this.result.data.message)) {
                        HQ.message.show('2013103001', [this.result.data.message], '', true);
                    } else {
                        HQ.message.process(msg, data, true);
                    }
                } else {
                    HQ.message.process(msg, data, true);
                }

                App.winCopy.unmask();
                App.winCopy.hide();

            },
            failure: function (msg, data) {
                App.winCopy.unmask();
                App.winCopy.hide();
                HQ.message.process(msg, data, true);
            }
        });
        //App.stoImport.load({
        //    params: { branchID: App.BranchID.getValue(), effDate: App.DateEnt.getValue(), brandID: App.BrandID.getValue(), siteID: App.SiteID.getValue(), barcode: App.txtCopy.value },
        //    callback: function () {

        //        Ext.each(App.stoImport.data.items, function (item, index) {
        //            if (!Ext.isEmpty(item.data.InvtID)) {
        //                var newItem = Ext.create('App.mdlTrans');

        //                newItem.data.LineRef = lastLineRef();
        //                newItem.data.InvtMult = 1;
        //                newItem.data.TranType = 'RC';
        //                newItem.data.JrnlType = 'IN';
        //                newItem.data.BranchID = HQ.cpnyID;
        //                newItem.data.BatNbr = HQ.objBatch.data.BatNbr;
        //                newItem.data.TranDate = App.DateEnt.getValue();
        //                newItem.data.SiteID = App.SiteID.getValue();
        //                newItem.data.ReasonCD = App.ReasonCD.getValue();
        //                newItem.data.UnitDesc = item.data.StkUnit;
        //                newItem.data.CnvFact = 1;
        //                newItem.data.UnitMultDiv = 'M'

        //                newItem.data.Qty = lstImportQty[index];

        //                if (invt.data.ValMthd == "A" || invt.data.ValMthd == "E") {
        //                    newItem.data.UnitPrice = item.data.Price;
        //                }

        //                newItem.data.POPrice = item.data.POPrice;
        //                newItem.data.OMPrice = item.data.OMPrice;
        //                newItem.data.TranDesc = item.data.Descr;
        //                newItem.data.BarCode = item.data.BarCode;
        //                newItem.data.InvtID = item.data.InvtID;
        //                newItem.data.TranAmt = newItem.data.Qty * newItem.data.UnitPrice;
        //                newItem.commit();

        //                App.stoTrans.insert(App.stoTrans.getCount() - 1, newItem);
        //            }
        //        });
        //        App.grdTrans.view.refresh();
        //        if (Ext.isEmpty(errorInvtID) && Ext.isEmpty(errorFormat)) {
        //            App.txtCopy.setValue('');
        //            App.winCopy.hide();
        //            calculate();
        //        }
        //        else {
        //            errorInvtID = (errorInvtID == '' ? '' : '</br>' + HQ.common.getLang('ErrorInvtID') + ' ' + errorInvtID.substring(0, errorInvtID.length - 1)) + (errorFormat == '' ? '' : '</br>' + HQ.common.getLang('ErrorFormat') + ' ' + errorFormat.substring(0, errorFormat.length - 1))
        //            HQ.message.show(201405072, [errorInvtID], '', true);

        //        }
        //        App.winCopy.unmask();
        //        HQ.common.showBusy(false);
        //    }
        //});
    }
};

var btnImport_Click = function (c, e) {
    if (App.cboStatus.getValue() != 'H') {
        return;
    }


    if (Ext.isEmpty(App.cboSiteID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('siteid')], '', true);
        App.btnImport.reset();
        return;
    }
    if (HQ.isInsert || HQ.isUpdate) {
        var files = c.fileInputEl.dom.files;
        var fileName = c.getValue();
        var ext = fileName.split(".").pop().toLowerCase();
        if (ext == "csv" || ext == "xls" || ext == "xlsx") {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                url: 'IN10200/Import',
                timeout: 1800000,
                type: 'POST',
                clientValidation: false,
                params: {
                    lineRef: lastLineRef(),
                    branchID: App.txtBranchID.getValue(),
                    cboWhseLoc: App.cboWhseLoc.getValue()
                },
                success: function (msg, data) {
                    if (this.result.data.lstTrans != undefined) {
                        var errorInvtID = [];
                        this.result.data.lstTrans.forEach(function (item) {
                            var obj = HQ.store.findRecord(App.stoTrans, ["InvtID"], [item.InvtID]);
                            if (!obj) {
                                var newTrans = Ext.create('App.mdlTrans');
                                newTrans.data.JrnlType = 'IN';
                                newTrans.data.ReasonCD = App.cboReasonCD.getValue();
                                newTrans.data.TranAmt = newTrans.data.ExtCost = item.TranAmt;
                                newTrans.data.BranchID = HQ.cpnyID;
                                newTrans.data.CnvFact = item.CnvFact;
                                newTrans.data.ExtCost = item.TranAmt;
                                newTrans.data.InvtID = item.InvtID;
                                newTrans.data.InvtMult = -1;
                                newTrans.data.LineRef = item.LineRef;
                                newTrans.data.Qty = item.Qty;
                                newTrans.data.QtyOnHand = item.QtyOnHand;
                                newTrans.data.SiteID = item.SiteID;
                                newTrans.data.WhseLoc = item.WhseLoc;
                                newTrans.data.TranDate = App.txtDateEnt.getValue();
                                newTrans.data.TranDesc = item.TranDesc;
                                newTrans.data.TranType = 'II';
                                newTrans.data.UnitDesc = item.UnitDesc;
                                newTrans.data.UnitMultDiv = item.UnitMultDiv;
                                newTrans.data.UnitPrice = newTrans.data.UnitCost = item.UnitPrice;
                                newTrans.commit();
                                App.stoTrans.insert(App.stoTrans.getCount() - 1, newTrans);
                            }
                            else {
                                errorInvtID.push(item.InvtID);
                                //if (item.CnvFact == obj.data.CnvFact) {
                                //    obj.set("Qty", obj.data.Qty + item.Qty);
                                //    obj.set("TranAmt", obj.data.UnitPrice * obj.data.Qty);
                                //    obj.set("ExtCost", obj.data.TranAmt);
                                //}
                                //else if (item.CnvFact == 1) {

                                //    obj.set("Qty", obj.data.Qty * obj.data.CnvFact + item.Qty);
                                //    obj.set("CnvFact", 1);
                                //    obj.set("UnitDesc", item.UnitDesc);
                                //    obj.set("UnitPrice", item.UnitPrice);
                                //    obj.set("UnitCost", item.UnitPrice);
                                //    obj.set("TranAmt", item.UnitPrice * obj.data.Qty);
                                //    obj.set("ExtCost", obj.data.TranAmt);

                                //}
                                //else if (obj.data.CnvFact == 1) {
                                //    obj.set("Qty", obj.data.Qty + item.Qty * item.CnvFact);
                                //    obj.set("TranAmt", obj.data.UnitPrice * obj.data.Qty);
                                //    obj.set("ExtCost", obj.data.TranAmt);
                                //}
                            }
                        });

                        App.stoLotTrans.clearFilter();
                        App.stoLotTrans.suspendEvents();
                        this.result.data.lstLot.forEach(function (item) {
                            var objLot = HQ.store.findRecord(App.stoLotTrans, ["InvtID", "LotSerNbr"], [item.InvtID, item.LotSerNbr]);
                            if (!objLot) {
                                var newLot = Ext.create('App.mdlLotTrans');
                                newLot.data.LotSerNbr = item.LotSerNbr;
                                newLot.data.BranchID = HQ.cpnyID;
                                newLot.data.INTranLineRef = item.INTranLineRef;
                                newLot.data.ExpDate = new Date(parseInt(item.ExpDate.substr(6)));
                                newLot.data.InvtID = item.InvtID;
                                newLot.data.InvtMult = -1;
                                newLot.data.MfgrLotSerNbr = '';
                                newLot.data.Qty = item.Qty;
                                newLot.data.QtyOnHand = item.QtyOnHand;
                                newLot.data.SiteID = item.SiteID;
                                newLot.data.WhseLoc = item.WhseLoc;
                                newLot.data.SlsperID = '';
                                newLot.data.TranDate = App.txtDateEnt.getValue();
                                newLot.data.WarrantyDate = new Date(parseInt(item.WarrantyDate.substr(6)));
                                newLot.data.TranType = 'II';
                                newLot.data.UnitDesc = item.UnitDesc;
                                newLot.data.UnitMultDiv = item.UnitMultDiv;
                                newLot.data.UnitPrice = newLot.data.UnitCost = item.UnitPrice;
                                newLot.data.CnvFact = item.CnvFact;
                                newLot.commit();
                                App.stoLotTrans.insert(App.stoLotTrans.getCount() - 1, newLot);
                            }
                            else {
                                //if (item.CnvFact == objLot.data.CnvFact) {
                                //    objLot.set("Qty", objLot.data.Qty + item.Qty);
                                //}
                                //else if (item.CnvFact == 1) {

                                //    objLot.set("Qty", objLot.data.Qty * objLot.data.CnvFact + item.Qty);
                                //    objLot.set("CnvFact", 1);
                                //    objLot.set("UnitDesc", item.UnitDesc);
                                //    objLot.set("UnitPrice", item.UnitPrice);
                                //    objLot.set("UnitCost", item.UnitPrice);

                                //}
                                //else if (objLot.data.CnvFact == 1) {
                                //    objLot.set("Qty", objLot.data.Qty + item.Qty * item.CnvFact);
                                //}
                            }
                        });
                        App.stoLotTrans.resumeEvents();
                        calculate();
                        checkTransAdd();
                        if (errorInvtID.length > 0) {
                            HQ.message.show('201512241', [errorInvtID.join(',')], '', true);
                        }
                        else if (!Ext.isEmpty(this.result.data.message)) {
                            HQ.message.show('2013103001', [this.result.data.message], '', true);
                        } else {
                            HQ.message.process(msg, data, true);
                        }
                    } else {
                        HQ.message.process(msg, data, true);
                    }

                    //App.winCopy.unmask();
                    //App.winCopy.hide();

                },
                failure: function (msg, data) {
                    //App.winCopy.unmask();
                    //App.winCopy.hide();
                    HQ.message.process(msg, data, true);
                }
            });

        } else {
            HQ.message.show('2014070701', [ext], '', true);
            App.btnImport.reset();
        }
    }
    //thay the import text thanh import excel
    //App.txtCopy.setValue('');
    //App.winCopy.show();
};

var grdTrans_BeforeEdit = function (item, e) {
    if (!HQ.grid.checkBeforeEdit(e, ['InvtID'])) return false;
    if (!HQ.form.checkRequirePass(App.frmMain)) return false;
    if (App.cboStatus.getValue() != 'H') {
        return false;
    }
    //if (App.grdTrans.isLock) {
    //    return false;
    //}
    if (e.field === 'WhseLoc') {
        _siteID = e.record.data.SiteID;
        App.cboWhseLocTrans.store.reload();
    }
    if (Ext.isEmpty(App.cboSiteID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('SiteID')], '', true);
        return false;
    }

    var key = e.field;

    if (!HQ.grid.checkInput(e, ['InvtID']) && key != 'InvtID') {
        return false;
    }
    if (!Ext.isEmpty(e.record.data.InvtID) && key == 'InvtID') {
        return false;
    }

    if (Ext.isEmpty(e.record.data.LineRef)) {
        e.record.data.LineRef = lastLineRef();
        e.record.data.InvtMult = -1;
        e.record.data.TranType = 'II';
        e.record.data.JrnlType = 'IN';
        e.record.data.BranchID = HQ.cpnyID;
        e.record.data.BatNbr = HQ.objBatch.data.BatNbr;
        e.record.data.TranDate = App.txtDateEnt.getValue();
        e.record.data.SiteID = App.cboSiteID.getValue();
        e.record.commit();
    }

    if (key == 'UnitPrice') {
        var invt = e.row.invt;
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
                params: { siteID: App.cboSiteID.getValue(), invtID: selected[0].data.InvtID,whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSelect,
                row: selected[0]
            });
            App.stoUnit.load({
                params: { invtID: selected[0].data.InvtID },
                callback: checkSelect,
                row: selected[0]
            });
            App.stoOldTrans.load({
                params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue() },
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
        if (e.field === 'SiteID') {
            e.record.set('WhseLoc', '');
        }
        var invt = e.record.invt;
        if (!Ext.isEmpty(invt)) {

            if (key == 'InvtID' && Ext.isEmpty(e.record.data.UnitDesc)) {
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

            if (key == 'InvtID' || key == 'Qty') {
                HQ.common.showBusy(true, 'Process...');
                HQ.numEditTrans = 0;
                HQ.maxEditTrans = 4;
                App.stoUnit.load({
                    params: { invtID: e.record.data.InvtID },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoItemSite.load({
                    params: { siteID: App.cboSiteID.getValue(), invtID: e.record.data.InvtID, whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoOldTrans.load({
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoPrice.load({
                    params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.txtDateEnt.getValue(), valMthd: invt.ValMthd, siteID: App.cboSiteID.getValue() }, callback: checkSourceEdit, row: e
                });

            } else if (key == 'UnitDesc') {
                HQ.common.showBusy(true, 'Process...');
                HQ.numEditTrans = 0;
                HQ.maxEditTrans = 3;
                App.stoItemSite.load({
                    params: { siteID: App.cboSiteID.getValue(), invtID: e.record.data.InvtID, whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoOldTrans.load({
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoPrice.load({
                    params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.txtDateEnt.getValue(), valMthd: invt.ValMthd, siteID: App.cboSiteID.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
            } else {
                checkExitEdit(e);
            }

        } else {

        }
    } else {
        if (key != 'InvtID') handleTab(key);
    }
};

var grdTrans_ValidateEdit = function (item, e) {
    if (e.field === "SiteID" && e.value === e.record.data.SiteID) {
        return false;
    }
};

var grdLot_BeforeEdit = function (item, e) {
    if (App.grdLot.isLock) {
        return false;
    }
    if ((e.field === 'WhseLoc' || e.field === 'SiteID') && !HQ.IsChangeSite) {
        return false;
    }

    if (e.field === 'WhseLoc' && !HQ.showWhseLocColumn) {
        return false;
    }

    if (e.field === 'SiteID' && !HQ.showSiteColumn) {
        return false;
    }
    var key = e.field;
    var record = e.record;
    if (key != 'LotSerNbr' && Ext.isEmpty(e.record.data.LotSerNbr)) return false;
    if (key == 'LotSerNbr' && !Ext.isEmpty(record.data.LotSerNbr)) return false;


    if (Ext.isEmpty(record.data.InvtID)) {
        record.data.InvtID = App.winLot.record.data.InvtID;
        record.data.SiteID = App.winLot.record.data.SiteID;
    }

    record.commit();
    //App.cboLotUnitDesc.setValue('');
};

var grdLot_SelectionChange = function (item, selected) {
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectLot = 0;
            HQ.maxSelectLot = 1;
            HQ.common.showBusy(true, 'Process...');
            App.stoItemLot.load({
                //params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, batNbr: App.cboBatNbr.getValue(), whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc, cnvFact: selected[0].data.CnvFact },
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, batNbr: App.cboBatNbr.getValue(), whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSelectLot,
                row: selected[0]
            });
        } else {
            App.lblLotQtyAvail.setText('');
        }
    }
};

var grdLot_Edit = function (item, e) {
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
            App.stoItemLot.load({
                //params: { siteID: lot.SiteID, invtID: lot.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: lot.LotSerNbr, whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc, cnvFact: selected[0].data.CnvFact },
                params: { siteID: lot.SiteID, invtID: lot.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: lot.LotSerNbr, whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSourceEditLot,
                row: e
            });
        }
    }
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

        App.cboSiteID.store.reload();
        App.cboWhseLocTrans.store.reload();
        App.cboReasonTrans.store.reload();

        var objFirst = App.stoTrans.data.items[0].data;
       // App.cboSiteID.setValue(objFirst.SiteID);
        App.cboSlsperID.setValue(objFirst.SlsperID);
        //App.cboWhseLoc.setValue(objFirst.WhseLoc);
    }
    App.lblQtyAvail.setText('');

    if(HQ.isInsert)
        HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);

    checkTransAdd();

    calculate();
    App.grdTrans.isChange = false;
    HQ.common.showBusy(false, HQ.waitMsg);
    setChange(false);
};

var bindBatch = function (record) {
    HQ.objBatch = record;
    App.cboBatNbr.events['change'].suspend();
    App.cboSiteID.events['change'].suspend();
    App.cboFromToSiteID.events['change'].suspend();
    App.cboPerPost.events['change'].suspend();
    App.frmMain.events['fieldchange'].suspend();
    App.cboStatus.forceSelection = false;
    App.frmMain.loadRecord(HQ.objBatch);
    App.cboStatus.forceSelection = false;
    //if (HQ.perpost && !HQ.objBatch.data.PerPost) {
    //    HQ.objBatch.data.PerPost = HQ.perpost;
    //}
    App.frmMain.events['fieldchange'].resume();
    App.cboBatNbr.events['change'].resume();
    App.cboSiteID.events['change'].resume();
    App.cboPerPost.events['change'].resume();
    App.cboFromToSiteID.events['change'].resume();
    setStatusForm();


    HQ.common.showBusy(true, HQ.waitMsg);
    HQ.numDetail = 0;
    App.stoTrans.reload();
    App.stoLotTrans.reload();

    App.cboHandle.setValue('N');
};

var save = function () {
    var i = 0;
    var errorMessage = '';
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        i++;
        if (record.data.ClassID == 'POSM')
            if (!record.data.PosmID) {
                errorMessage += i + ', ';
            }
    });
    if (errorMessage) {
        HQ.message.show(2016033001, [errorMessage], '', true);
        return;
    }    

    if ((App.cboBatNbr.value && !HQ.isUpdate) || (Ext.isEmpty(App.cboBatNbr.value) && !HQ.isInsert)) {
        HQ.message.show(728, '', '', true);
        return;
    }
    if (App.cboStatus.getValue() != "H" && (App.cboHandle.getValue() == "N" || Ext.isEmpty(App.cboHandle.getValue()))) {
        HQ.message.show(2015020803, '', '', true);
        return;
    }
    var store1 = App.stoTrans;
    var allRecords1 = store1.snapshot || store1.allData || store1.data;

    if (allRecords1.items.length <= 1) {
        HQ.message.show(2015020804, [App.cboBatNbr.value], '', true);
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

            if (HQ.showSiteColumn) {
                if (!item.data.SiteID) {
                    HQ.message.show(1000, 'SiteID');
                    return false;
                }
            }

            if (HQ.showWhseLocColumn) {
                if (!item.data.WhseLoc) {
                    HQ.message.show(1000, 'WhseLoc');
                    return false;
                }
            }
            if (item.data.Qty == 0) {
                HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
                App.smlTrans.select(App.stoTrans.indexOf(HQ.store.findInStore(App.stoTrans, ['LineRef'], [item.data.LineRef])));
                flat = true;
                return false;
            }
            if (item.data.UnitMultDiv == '') {
                HQ.message.show(2525, [item.data.InvtID], '', true);
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
            if (item.invt.LotSerTrack != "N" && !Ext.isEmpty(item.invt.LotSerTrack)) {
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

                var detQty = Math.round(item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact);
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
            url: 'IN10200/Save',
            timeout: 180000,
            params: {
                lstTrans: Ext.encode(App.stoTrans.getRecordsValues()),
                lstLot: Ext.encode(App.stoLotTrans.getRecordsValues()),
                PerPost: App.cboPerPost.getValue()                
            },
            success: function (msg, data) {

                var batNbr = '';

                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    var batNbr = this.result.data.batNbr;
                }
                if (!Ext.isEmpty(batNbr)) {
                    App.cboBatNbr.forceSelection = false
                    App.cboBatNbr.events['change'].suspend();
                    App.cboBatNbr.setValue(batNbr);
                    App.cboBatNbr.events['change'].resume();
                    if (Ext.isEmpty(HQ.recentRecord)) {
                        HQ.recentRecord = batNbr;
                    }
                   
                }
                App.stoBatch.reload();
                setChange(false);                
                HQ.message.process(msg, data, true);
                
              
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteHeader = function (item) {
    if (item == 'yes') {
        if (Ext.isEmpty(App.cboBatNbr.getValue())) {
            menuClick('new');
        } else {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'IN10200/Delete',
                timeout: 180000,
                success: function (msg, data) {
                    var record = App.stoBatch.getById(App.cboBatNbr.getValue());
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
        var count = 0;
        var det = App.smlTrans.selected.items[0].data;
        App.stoLotTrans.clearFilter();
        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (det.LineRef == App.stoLotTrans.data.items[i].data.INTranLineRef) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
        for (var i = 0; i < App.stoTrans.data.length; i++)
        {
            if (App.stoTrans.data.items[i].data.InvtID != '')
                count++;
        }
        if (count == 1)
        {
            HQ.message.show(2018062501);
            return false;
        }
        if (App.cboBatNbr.value) {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'IN10200/DeleteTrans',
                timeout: 180000,
                params: {
                    lineRef: App.smlTrans.selected.items[0].data.LineRef,
                },
                success: function (msg, data) {
                    if (!Ext.isEmpty(data.result.data.tstamp)) {
                        App.tstamp.setValue(data.result.data.tstamp);
                    }
                    App.grdTrans.deleteSelected();
                    calculate();
                    HQ.message.process(msg, data, true);
                    HQ.isChange = false;
                    HQ.common.changeData(HQ.isChange, 'IN10200');
                    App.stoBatch.reload();
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
        url: 'IN10200/Report',
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

//////////////////////////////////
var calcLot = function (record) {
    if (!Ext.isEmpty(record.invt) && !Ext.isEmpty(record.invt.LotSerTrack) && record.invt.LotSerTrack != 'N' && !Ext.isEmpty(record.data.UnitDesc)) {
        var flat = false;
        var det = record.data;

        App.stoLotTrans.clearFilter();
        App.stoLotTrans.data.each(function (item) {
            if (item.data.INTranLineRef == det.LineRef && !Ext.isEmpty(item.data.LotSerNbr)) {
                flat = true;
            }
        });

        if (!flat) {
            HQ.common.showBusy(true, HQ.waitMsg);
            App.stoCalcLot.load({
                params: {
                    siteID: det.SiteID,
                    invtID: det.InvtID,
                    branchID: App.txtBranchID.getValue(),
                    batNbr: App.cboBatNbr.getValue(),
                    whseLoc: App.cboWhseLoc.getValue(),
                    showWhseLoc: HQ.showWhseLoc,
                    CnvFact: det.CnvFact
                },
                det: record.data,
                row: record,
                callback: function (records, options, success) {

                    var det = options.det;
                    var record = options.row;
                    //var needQty = Math.round(det.UnitMultDiv == "M" ? det.Qty * det.CnvFact : det.Qty / det.CnvFact);
                    var needQty = Math.round(det.UnitMultDiv == "M" ? det.Qty : det.Qty);
                    App.stoLotTrans.clearFilter();
                    App.stoCalcLot.data.each(function (item) {
                        var newQty = 0;
                        var curQty = 0;

                        App.stoLotTrans.data.each(function (item2) {
                            if (item2.data.LotSerNbr == item.data.LotSerNbr && item2.data.InvtID == item.data.InvtID && item2.data.SiteID == item.data.SiteID) {
                                curQty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
                            }
                        });

                        if (Math.round(item.data.QtyAvail - curQty) == 0) return true;

                        if ((item.data.QtyAvail - curQty) >= needQty) {
                            newQty = needQty;
                            needQty = 0;
                        }
                        else {
                            newQty = (item.data.QtyAvail - curQty);
                            needQty -= (item.data.QtyAvail - curQty);
                            item.data.QtyAvail = 0;
                        }

                        if (newQty != 0) {
                            var newLot = Ext.create('App.mdlLotTrans');
                            newLot.data.BranchID = App.txtBranchID.getValue();
                            newLot.data.BatNbr = HQ.util.passNull(App.cboBatNbr.getValue());
                            newLot.data.LotSerNbr = item.data.LotSerNbr;
                            newLot.data.ExpDate = item.data.ExpDate;
                            newLot.data.WarrantyDate = item.data.WarrantyDate;
                            newLot.data.PercentExpDate = item.data.PercentExpDate;
                            newLot.data.INTranLineRef = det.LineRef;
                            newLot.data.SiteID = det.SiteID;
                            newLot.data.InvtID = det.InvtID;
                            newLot.data.InvtMult = -1;
                            if ((det.UnitMultDiv == "M" ? newQty / det.CnvFact : newQty * det.CnvFact) % 1 > 0) {
                                newLot.data.CnvFact = det.CnvFact;//1;
                                newLot.data.UnitMultDiv = 'M';
                                newLot.data.Qty = newQty;
                                newLot.data.UnitDesc = det.UnitDesc;//options.row.invt.StkUnit;

                                if (record.invt.ValMthd == "A" || record.invt.ValMthd == "E") {
                                    newLot.data.UnitPrice = newLot.data.UnitCost = Math.round(det.UnitMultDiv == "M" ? det.UnitPrice / det.CnvFact : det.UnitPrice * det.CnvFact);
                                } else {
                                    newLot.data.UnitPrice = newLot.data.UnitCost = 0;
                                }
                            } else {
                                newLot.data.Qty = Math.round(det.UnitMultDiv == "M" ? newQty / det.CnvFact : newQty * det.CnvFact);
                                newLot.data.CnvFact = det.CnvFact;
                                newLot.data.UnitMultDiv = det.UnitMultDiv;
                                newLot.data.UnitPrice = det.UnitPrice;
                                newLot.data.UnitCost = det.UnitPrice;
                                newLot.data.UnitDesc = det.UnitDesc;
                            }                            
                            newLot.data.QtyOnHand = item.data.QtyOnHand;
                            newLot.commit();
                            App.stoLotTrans.insert(App.stoLotTrans.getCount(), newLot);
                        }



                        if (needQty == 0) return false;
                    });
                    App.stoLotTrans.commitChanges();
                    HQ.common.showBusy(false);
                    showLot(options.row, false);
                    
                }
            });
        } else {
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
            showLot(record, true);
        }
    }
};

var showLot = function (record, loadCombo) {

    var lock = !((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) || App.cboStatus.getValue() != "H";
    App.grdLot.isLock = lock;
    if (loadCombo) {

        App.stoCalcLot.load({
            params: {
                siteID: record.data.SiteID,
                invtID: record.data.InvtID,
                branchID: App.txtBranchID.getValue(),
                batNbr: App.cboBatNbr.getValue(),
                whseLoc: App.cboWhseLoc.getValue(),
                showWhseLoc: HQ.showWhseLoc,
                CnvFact: record.data.CnvFact
            }
        });
    }


    App.stoLotTrans.clearFilter();
    App.stoLotTrans.filter('INTranLineRef', record.data.LineRef);

    var newRow = Ext.create('App.mdlLotTrans');
    newRow.data.INTranLineRef = record.data.LineRef;
    newRow.data.UnitDesc = record.data.UnitDesc;
    newRow.data.InvtMult = record.data.InvtMult;
    newRow.data.TranType = record.data.TranType;
    newRow.data.PercentExpDate = record.data.PercentExpDate;
    newRow.data.WarrantyDate = record.data.WarrantyDate;
    HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);
    HQ.focus = '';
    App.winLot.record = record;
    App.grdLot.view.refresh();
    App.winLot.setTitle(record.data.InvtID + ' ' + (record.data.UnitMultDiv == "M" ? record.data.Qty * record.data.CnvFact : record.data.Qty / record.data.CnvFact) + ' ' + record.invt.StkUnit);
    App.winLot.show();
    setTimeout(function () {
        //App.winLot.toFront();
        if (HQ.showQtyOnhand) {
            if (App.colLotQtyOnHand) {
                App.colLotQtyOnHand.show();
            }
        }
    }, 50);
};

var calculate = function () {
    var totAmt = 0;
    var totQty = 0;
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (item) {
        totAmt += item.data.TranAmt;
        totQty += item.data.Qty;
    });

    App.txtTotAmt.setValue(totAmt);
    App.txtTotQty.setValue(totQty);
};

var defaultOnNew = function () {
    HQ.isNew = true;
    var record = Ext.create('App.mdlBatch');
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'H';
    record.data.PerPost = HQ.perpost;
    record.data.DateEnt = HQ.businessDate;
    App.cboSiteID.setValue(HQ.inSite);
    if (HQ.showWhseLoc == 0) {
        App.cboWhseLoc.setValue("");
    }
    else {
        App.cboWhseLoc.setValue(HQ.WhseLoc);
    }
    //if (HQ.perpost) {
    //    App.cboPerPost.setValue(HQ.perpost);
    //}
    App.cboSlsperID.setValue('');
    App.frmMain.validate();


    if (HQ.showSiteColumn) {
        App.colSiteID.show();
    }
    if (HQ.showWhseLocColumn) {
        App.colWhseLoc.show();
    }
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

var setStatusForm = function () {
    var lock = true;
    var flag = true;
    if (!Ext.isEmpty(HQ.objBatch.data.BatNbr)) {
        if (HQ.objBatch.data.Status == 'H') {
            lock = false;
        }

    } else {
        lock = !HQ.isInsert;
        App.btnImport.setDisabled(false);
    }

    App.btnImport.setDisabled(App.cboStatus.getValue() != 'H');

    HQ.common.lockItem(App.frmMain, lock);
    App.grdTrans.isLock = lock;
    App.cboBatNbr.setReadOnly(false);
    App.cboHandle.setReadOnly(false);
    App.txtRvdBatNbr.setReadOnly(true);
    App.cboStatus.setReadOnly(true);
    App.txtRefNbr.setReadOnly(true);
    App.txtTotQty.setReadOnly(true);
    App.txtTotAmt.setReadOnly(true);

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
    if (flag == true && App.cboStatus.getValue() != 'H') {
        App.cboHandle.setReadOnly(false);
    }
    //var lock = true;
    //HQ.common.lockItem(App.frmMain, true);
    //if (!Ext.isEmpty(HQ.objBatch.data.BatNbr)) {
    //    if (HQ.objBatch.data.Status == 'H') {
    //        lock = false;
    //    }

    //} else {
    //    lock = !HQ.isInsert;
    //}

    //HQ.common.lockItem(App.frmMain, lock);
    //App.grdTrans.isLock = lock;
    //App.cboBatNbr.setReadOnly(false);
    //App.cboHandle.setReadOnly(false);
    //App.txtRvdBatNbr.setReadOnly(true);
    //App.cboStatus.setReadOnly(true);
    //App.txtRefNbr.setReadOnly(true);
    //App.txtTotQty.setReadOnly(true);
    //App.txtTotAmt.setReadOnly(true);
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

        trans.ReasonCD = App.cboReasonCD.getValue();
        trans.SiteID = App.cboSiteID.getValue();
        trans.WhseLoc = App.cboWhseLoc.getValue();
        var invt = row.record.invt;
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
        if (flagPosm == false)
            trans.TranAmt = trans.Qty * trans.UnitPrice;
        trans.ClassID = invt.ClassID;
        getQtyAvail(row.record);

    } else if (key == 'UnitDesc') {

        var invt = row.record.invt;

        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, trans.UnitDesc);

        if (Ext.isEmpty(cnv)) {
            trans.UnitMultDiv = '';
            trans.UnitPrice = trans.UnitCost = 0;
            trans.Qty = 0;
            trans.TranAmt = 0;
            row.record.commit();
            HQ.common.showBusy(false);
            return;
        }

        var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [trans.InvtID, trans.SiteID]);

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
        if (flagPosm == false)
            trans.TranAmt = trans.Qty * trans.UnitPrice;
        getQtyAvail(row.record);
        if (trans.Qty > 0) {
            calcLot(row.record);
        }
    } else if (key == "Qty") {

        var invt = row.record.invt;
        var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [trans.InvtID, trans.SiteID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
        }

        if (invt.StkItem == 1) {
            var editQty = 0;
            var totQty = 0;

            if (trans.UnitMultDiv == "M") {
                editQty = trans.Qty * (trans.CnvFact == 0 ? 1 : trans.CnvFact);
            }
            else {
                editQty = trans.Qty / (trans.CnvFact == 0 ? 1 : trans.CnvFact);
            }

            if (!HQ.objSetup.NegQty && trans.TranType != "RI") {
                totQty = editQty + calculateInvtTotal(trans.InvtID, trans.SiteID, trans.LineRef);
                if (totQty > site.QtyAvail) {
                    trans.Qty = 0;
                    row.record.commit();
                    HQ.message.show(35, '', '', true);
                    HQ.common.showBusy(false);
                    return;
                }
            }
        }
        if (flagPosm == false)
            trans.TranAmt = trans.Qty * trans.UnitPrice;
        getQtyAvail(row.record);

        calcLot(row.record);
    }
    else if (key == "PosmID" && App.grdTrans.isChange == true) {
        setChange(true);
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

    handleTab(key);
};

var handleTab = function (key) {
    if (App.smlTrans.tab) {
        setTimeout(function () {
            if (key == 'InvtID') {
                App.grdTrans.editingPlugin.startEdit(App.smlTrans.getCurrentPosition().row, 3)
                App.cboTransUnitDesc.selectText();
            } else if (key == 'UnitDesc') {
                App.grdTrans.editingPlugin.startEdit(App.smlTrans.getCurrentPosition().row, 4)
                App.txtTransQty.selectText()
            } else if (key == 'Qty') {
                App.grdTrans.editingPlugin.startEdit(App.smlTrans.getCurrentPosition().row + 1, 4)
            }
        }, 100);
    }
    App.smlTrans.tab = false;
};

var checkExitEditLot = function (row) {
    var key = row.field;
    var record = row.record;
    var lot = row.record.data;
    if (key == "Qty") {
       // getLotQtyAvail(record);
    } else if (key == "UnitDesc") {
        var price = 0;
        var cnvFact = 0;
        var unitMultDiv = "";
        var cnv = setUOM(App.winLot.record.invt.InvtID, App.winLot.record.invt.ClassID, App.winLot.record.invt.StkUnit, lot.UnitDesc);
        if (!Ext.isEmpty(cnv)) {
            cnvFact = cnv.CnvFact;
            unitMultDiv = cnv.MultDiv;
            lot.CnvFact = cnvFact;
            lot.UnitMultDiv = unitMultDiv;
        } else {
            lot.CnvFact = 1;
            lot.UnitMultDiv = '';
            lot.UnitPrice = 0;
            lot.UnitDesc = '';
            
            record.commit();
            HQ.common.showBusy(false);
            return;
        }

        if (App.winLot.record.invt.ValMthd == "A" || App.winLot.record.invt.ValMthd == "E") {
            var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [lot.InvtID, lot.SiteID]);
            price = site.AvgCost * lot.CnvFact;
            lot.UnitPrice = lot.UnitCost = price;

        } else {
            lot.UnitPrice = lot.UnitCost = 0
        }

       // getLotQtyAvail(record);
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
        if (HQ.showWhseLoc==2 ||(HQ.showWhseLoc==1 && !Ext.isEmpty(App.cboWhseLoc.getValue())) ) {
            var itemLot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', 'LotSerNbr', 'WhseLoc'], [lot.InvtID, lot.SiteID, lot.LotSerNbr, App.cboWhseLoc.getValue()]);
        }
        else {
            var itemLot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', 'LotSerNbr'], [lot.InvtID, lot.SiteID, lot.LotSerNbr]);
        }
        
        if (!Ext.isEmpty(itemLot)) {
            lot.ExpDate = itemLot.ExpDate;
            lot.WarrantyDate = itemLot.WarrantyDate;
            lot.PercentExpDate = itemLot.PercentExpDate;
        }

        if (!Ext.isEmpty(lot.LotSerNbr)) {
            var newRow = Ext.create('App.mdlLotTrans');
            newRow.data.INTranLineRef = lot.INTranLineRef;
            HQ.store.insertRecord(App.stoLotTrans, key, newRow, true);
        }
        getLotQtyAvail(record);
    }
    getLotQtyAvail(row.record);
    record.commit();
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

    //App.cboSlsperID.setReadOnly(App.cboStatus.getValue() != 'H');
    App.cboSlsperID.setReadOnly(flat);
    App.cboFromToSiteID.setReadOnly(App.cboStatus.getValue() != 'H');
    App.cboSiteID.setReadOnly(flat);
    App.cboFromToSiteID.setReadOnly(flat);
    App.cboWhseLoc.setReadOnly(flat);
    
};

var getQtyAvail = function (row) {
    var qtyOnhand = 0;
    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [row.data.InvtID, row.data.SiteID]);

    var qty = 0;
    if (!Ext.isEmpty(site)) {
        qty = site.QtyAvail - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
    }
    else {
        qty = 0 - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
    }
    qty = HQ.util.mathRound(row.data.UnitMultDiv == "M" ? qty / row.data.CnvFact : qty * row.data.CnvFact, 2);

    //if (!Ext.isEmpty(site)) {
    //    App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + (site.QtyAvail - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "")));
    //    qtyOnhand = site.QtyOnHand;
    //}
    //else {
    //    App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + (0 - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "")));
    //}
    App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ": " + qty + " " + row.data.UnitDesc);
    row.set('QtyOnHand', qtyOnhand);
};



var getLotQtyAvail = function (row) {
    if (HQ.showWhseLoc == 2 || (HQ.showWhseLoc == 1 && !Ext.isEmpty(App.cboWhseLoc.getValue()))) {
        var lot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', ['LotSerNbr'], 'WhseLoc'], [row.data.InvtID, row.data.SiteID, row.data.LotSerNbr, App.cboWhseLoc.getValue()]);
    }
    else {
        var lot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', ['LotSerNbr']], [row.data.InvtID, row.data.SiteID, row.data.LotSerNbr]);
    }
    var qty = 0;
    var qtyAvail = 0;
    var qtyOnhand = 0;
    App.stoLotTrans.snapshot.each(function (item2) {
        if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID) {
            qty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
        }
    });
    var det = App.winLot.record;

    if (!Ext.isEmpty(lot)) {
        qtyOnhand =HQ.util.mathRound( row.data.UnitMultDiv == "M" ? lot.QtyOnHand/row.data.CnvFact:lot.QtyOnHand*row.data.CnvFact ,2);
        qtyAvail = HQ.util.mathRound( row.data.UnitMultDiv == "M" ? (lot.QtyAvail - qty)/row.data.CnvFact:(lot.QtyAvail - qty)*row.data.CnvFact,2);
        if (qtyAvail < 0) {
            HQ.message.show(1043, [row.data.InvtID + " " + row.data.LotSerNbr, row.data.SiteID], "", true);
            qty = 0;
            row.data.Qty = 0;
            row.commit();
            App.stoLotTrans.snapshot.each(function (item2) {
                if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID) {
                    qty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
                }
            });
            qtyAvail = lot.QtyAvail - qty;

        }
    }
    else {
        qtyAvail = 0 - qty;
        if (qtyAvail < 0) {
            HQ.message.show(1043, [row.data.InvtID + " " + row.data.LotSerNbr, row.data.SiteID], "", true);
            qty = 0;
            row.data.Qty = 0;
            row.commit();
            App.stoLotTrans.snapshot.each(function (item2) {
                if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID) {
                    qty += HQ.util.mathRound(item2.data.UnitMultDiv == "M" ? item2.data.Qty / item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact,2);
                }
            });
            qtyAvail = 0 - qty;
        }
    }
    App.lblLotQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + qtyAvail + " " + row.data.UnitDesc);
    row.set('QtyOnHand', qtyOnhand);
};

var calculateInvtTotal = function (invtID, siteID, lineRef) {
    var qty = 0;
    var qtyOld = 0;
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID && (lineRef == "" || (lineRef != "" && lineRef != item.data.LineRef))) {
            qty +=HQ.util.mathRound( item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact,2);
        }
    });
    App.stoOldTrans.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID) {
            qtyOld +=HQ.util.mathRound( item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact,2);
        }
    });
    return qty - qtyOld;
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
        if (!Ext.isEmpty(App.cboBatNbr.getValue())) {
            App.stoBatch.reload();
        } else {
            defaultOnNew();
        }
    }
};

var setChange = function (isChange) {
    
    HQ.isChange = isChange;
    if (isChange) {
        App.cboBatNbr.setReadOnly(true);
    } else {
        App.grdTrans.isChange = false;
        App.cboBatNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'IN10200');
};

var renderRowNumber = function (value, meta, record) {
    return App.stoLotTrans.data.indexOf(record) + 1;
};

var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
};
var rendererReason = function (val) {
    var record = HQ.store.findRecord(App.ReasonCD.store, ["ReasonCD"], [val]);
    return (record) ? record.data.Descr : val;
}

var rendererWhseLoc = function (val) {
    var record = HQ.store.findRecord(App.cboWhseLoc.store, ["WhseLoc"], [val]);
    return (record) ? record.data.Descr : val;
}

function checkSave(item) {
    if (item == 'yes') {
        save();
    }
};

var rendererWarrantyDate = function (value, meta, record) {
    var date = new Date(1900, 0, 1);
    if (record.data.WarrantyDate != null) {
        if (record.data.WarrantyDate.toDateString() == date.toDateString() && record.data.LotSerNbr != "") {
            return '';
        } else {
            return Ext.util.Format.date(value, 'd-m-Y');
        }
    }
}
//var renderLotSerNbr = function (value) {
//    var lot = HQ.store.findInStore(App.cb, ['InvtID', 'SiteID', ['LotSerNbr'], 'WhseLoc'], [row.data.InvtID, row.data.SiteID, row.data.LotSerNbr, App.cboWhseLoc.getValue()]);
    
//};
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
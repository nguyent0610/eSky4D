﻿HQ.recentRecord = null;
HQ.focus = 'batch';
HQ.objBatch = null;
HQ.isTransfer = false;
HQ.objUser = null;
var _beginStatus = 'H';
var keyLoadData = true;
var _branchID = '';
var _advanceType = '';
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
    //App.grdTrans.view.loadMask.disable();
    //HQ.common.showBusy(false);
}
var stoHandle_Load = function () {
    App.cboHandle.setValue('N');
}
var stoTrans_Load = function () {
    bindTran();
}
var stoDetail_Load = function () {
    checkSourceDetail();
    if (App.grdTrans.store.data.items.length > 1) {
        App.cboAdvanceType.setReadOnly(true);
    }
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
var stoAllInvtINSite_Load = function (sto) {

    HQ.common.showBusy(false, HQ.waitMsg);
    App.grdTrans.store.suspendEvents();
    App.grdTrans.store.removeAll();
    var allInvtINSite = App.stoAllInvtINSite.data.items;
    if (allInvtINSite && allInvtINSite.length > 0) {
            for (var i = 0; i < allInvtINSite.length; i++) {

                App.stoTrans.insert(App.stoTrans.getCount(), allInvtINSite[i]);
            }

            
    }
    eventNew(App.stoTrans, App.grdTrans, 'InvtID');
    App.grdTrans.store.resumeEvents();
    App.grdTrans.view.refresh();
    var datagrd = App.stoTrans.data.items;
    var k = datagrd.length > 1;
    App.grdLot.store.removeAll();
    if (datagrd.length > 0) {
        for (var i = 0; i < datagrd.length; i++) {
            if (datagrd[i].data.InvtID != null && datagrd[i].data.InvtID != "") {
            //    k = true
                datagrd[i].data.LineRef = lastLineRef();
                var datagrdItemLot=App.stoLotTransVanSale.data.items;
                for (var j = 0; j < datagrdItemLot.length; j++) {
                    if (datagrdItemLot[j].data.InvtID == datagrd[i].data.InvtID) {
                        datagrdItemLot[j].data.INTranLineRef = datagrd[i].data.LineRef;
                        App.stoLotTrans.insert(App.stoLotTrans.getCount(), datagrdItemLot[j]);
                            //HQ.store.insertBlank(App.stoLotTransVanSale, ['LotSerNbr']);
                            //record = App.stoLotTransVanSale.getAt(App.grdLot.store.getCount() - 1);
                            //record.set('INTranLineRef', datagrdItemLot[j].data.INTranLineRef);
                            //record.set('BranchID', datagrdItemLot[j].data.BranchID);
                            //record.set('ExpDate', datagrdItemLot[j].data.ExpDate);
                            //record.set('SiteID', datagrdItemLot[j].data.SiteID);
                            //record.set('InvtID', datagrdItemLot[j].data.InvtID);
                            //record.set('LotSerNbr', datagrdItemLot[j].data.LotSerNbr);
                            //record.set('CnvFact', datagrdItemLot[j].data.CnvFact);
                            //record.set('InvtMult', datagrdItemLot[j].data.InvtMult);
                            //record.set('UnitMultDiv', datagrdItemLot[j].data.UnitMultDiv);
                            //record.set('TranType', datagrdItemLot[j].data.TranType);
                            //record.set('Qty', datagrdItemLot[j].data.Qty);
                            //record.set('UnitDesc', datagrdItemLot[j].data.UnitDesc);
                            //record.set('UnitPrice', datagrdItemLot[j].data.UnitPrice);
                            //record.set('UnitCost', datagrdItemLot[j].data.UnitCost);
                            //App.stoLotTransVanSale.insert(App.stoTrans.getCount(), datagrdItemLot[i]);
                            
                    }
                }
            }
        }        
    }
    App.cboSiteID.setReadOnly(k);
    App.cboToSiteID.setReadOnly(k);
    App.cboAdvanceType.setReadOnly(k);
    App.cboWhseLoc.setReadOnly(k);
    App.cboToWhseLoc.setReadOnly(k);
}
var stoAllInvtINSite_BeforeLoad = function () {
    HQ.common.showBusy(true, HQ.waitMsg);
}

var checkSelect = function (records, options, success) {
    HQ.numSelectTrans++;
    if (HQ.numSelectTrans == HQ.maxSelectTrans) {
        //App.grdTrans.view.loadMask.hide();
        //App.grdTrans.view.loadMask.setDisabled(false)
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
        //App.grdLot.view.loadMask.hide();
        //App.grdLot.view.loadMask.setDisabled(false)
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
    HQ.util.checkAccessRight();
    HQ.numSource = 0;
    HQ.maxSource = 12;
    HQ.numEditTrans = 0;
    HQ.maxEditTrans = 4;
    HQ.numDetail = 0;
    HQ.maxDetail = 2;
    HQ.numSelectTrans = 0;
    HQ.maxSelectTrans = 3;
    App.cboBatNbr.key = true;
    App.txtBranchID.setValue(HQ.cpnyID);
    App.cboHandle.getStore().addListener('load', stoHandle_Load);
    App.cboStatus.getStore().addListener('load', store_Load);
    App.cboTransInvtID.getStore().addListener('load', store_Load);
    App.cboToSiteID.getStore().addListener('load', store_Load);
    App.cboSiteID.getStore().addListener('load', store_Load);
    App.cboToCpnyID.getStore().addListener('load', store_Load);
    App.cboReasonCD.getStore().addListener('load', store_Load);
    App.cboShipViaID.getStore().addListener('load', store_Load);
    App.cboTransferStatus.getStore().addListener('load', store_Load);
    App.cboTransferType.getStore().addListener('load', store_Load);
    App.stoUserDefault.addListener('load', stoUserDefault_Load);
    App.stoSetup.addListener('load', stoSetup_Load);
    App.stoUnitConversion.addListener('load', store_Load);
    App.stoSetup.load();
    App.stoUserDefault.load();
    App.stoUnitConversion.load();
    App.stoInvt = App.cboTransInvtID.getStore();
    App.stoToSite = App.cboToSiteID.getStore();
    App.RptExpDate.setVisible(!HQ.hideRptExpDate);
    App.cboAdvanceType.setVisible(false);
    App.btnLoad.setVisible(false);
    App.txtExpectedDate.setVisible(!HQ.hideExpectedDateRcptDate);
    App.txtRcptDate.setVisible(!HQ.hideExpectedDateRcptDate);
    App.txtDescr.allowBlank = !HQ.allowDescrBlank;
    App.txtComment.allowBlank = !HQ.allowNoteBlank;
    App.btnImport.setVisible(HQ.showImprtExprt);
    App.btnExport.setVisible(HQ.showImprtExprt);
    if (HQ.showWhseLoc == 0) {
        App.cboWhseLoc.allowBlank = true;
        App.cboWhseLoc.setVisible(false);
        App.cboToWhseLoc.allowBlank = true;
        App.cboToWhseLoc.setVisible(false);
    }
    else {
        App.cboWhseLoc.setVisible(true);
        App.cboToWhseLoc.setVisible(true);
        if (HQ.showWhseLoc == 2) {
            App.cboWhseLoc.allowBlank = false;
            App.cboToWhseLoc.allowBlank = false;
        }
        else {
            App.cboWhseLoc.allowBlank = true;
            App.cboToWhseLoc.allowBlank = true;
        }
    }
    //App.cboWhseLoc.isValid();
    //App.cboToWhseLoc.isValid();
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.waitMsg);
}
var frmMain_FieldChange = function (item, field, newValue, oldValue) {
    if (field.key != undefined) {
        return;
    }
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (Object.keys(App.stoBatch.getChangedData()).length > 0 || Ext.isEmpty(App.cboBatNbr.getValue())) {
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
            //if (!App.grdTrans.view.loadMask.isHidden()) {
            //    return;
            //}
            if (HQ.isBusy == false) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    save();
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
            if (HQ.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
        case "new":
            if (HQ.focus == 'batch') {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                } else {
                    App.cboAdvanceType.setValue('');
                    defaultOnNew();
                }
            } else if (HQ.focus == 'trans') {
                eventNew(App.stoTrans, App.grdTrans, 'InvtID');
            }
            break;
        case "refresh":
            if (HQ.isChange) {
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
}

var eventNew = function (sto, grd, keys) {
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

var btnLot_Click = function () {
    //if (!Ext.isEmpty(this.record.invt)) {
        if (Ext.isEmpty(this.record.invt)) {
            this.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [this.record.data.InvtID]);
        }

        if (!Ext.isEmpty(this.record.invt.LotSerTrack) && this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.UnitDesc)) {
            showLot(this.record, true);
        }
    //}
}
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
            //App.winLot.record.data.UnitDesc = App.winLot.record.invt.StkUnit;
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
        App.winLot.record.data.TranAmt = App.winLot.record.data.Qty * App.winLot.record.data.UnitPrice;
        App.winLot.record.commit();

        App.grdTrans.view.refresh();


        calculate();
        App.stoLotTrans.clearFilter();
        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr)) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
    }
    App.winLot.hide();
}
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
}
var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        _branchID = App.cboPopupCpny.getValue();
        
        App.winPopup.hide();

        window.location.href = 'IN10300?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
}

var btnLoad_Click = function () {
    var keyLoad = true;
    if ((App.txtDescr.getValue() == "" || App.txtDescr.getValue() == null) && HQ.allowDescrBlank==true) {
        keyLoad = false;
        HQ.message.show(1000, [HQ.common.getLang('Descr')], '', true);
    }
    else {
        if (App.cboAdvanceType.getValue() == "" || App.cboAdvanceType.getValue() == null) {
            keyLoad = false;
            HQ.message.show(1000, [HQ.common.getLang('AdvanceType')], '', true);
        }
        else {
            if (App.cboSiteID.getValue() == "" || App.cboSiteID.getValue() == null) {
                keyLoad = false;
                HQ.message.show(1000, [HQ.common.getLang('Site')], '', true);
            }
            else {
                if (App.cboToCpnyID.getValue() == "" || App.cboToCpnyID.getValue() == null) {
                    keyLoad = false;
                    HQ.message.show(1000, [HQ.common.getLang('ToBranch')], '', true);
                }
                else {
                    if (App.cboToSiteID.getValue() == "" || App.cboToSiteID.getValue() == null) {
                        keyLoad = false;
                        HQ.message.show(1000, [HQ.common.getLang('ToSite')], '', true);
                    }
                    else {
                        if (App.cboTransferType.getValue() == "" || App.cboTransferType.getValue() == null) {
                            keyLoad = false;
                            HQ.message.show(1000, [HQ.common.getLang('TrsferType')], '', true);
                        }
                        else {
                            if (App.cboShipViaID.getValue() == "" || App.cboShipViaID.getValue() == null) {
                                keyLoad = false;
                                HQ.message.show(1000, [HQ.common.getLang('ShipVia')], '', true);
                            }
                            else {
                                if ((App.txtComment.getValue() == "" || App.txtComment.getValue() == null) && HQ.allowNoteBlank==true) {
                                    keyLoad = false;
                                    HQ.message.show(1000, [HQ.common.getLang('Comment')], '', true);
                                }
                                else {
                                    if (App.cboAdvanceType.getValue() == "" || App.cboAdvanceType.getValue() == null) {
                                        keyLoad = false;
                                        HQ.message.show(1000, [HQ.common.getLang('AdvanceType')], '', true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    if (keyLoad == true) {
        App.stoLotTransVanSale.reload();
        App.stoAllInvtINSite.reload();
        
    }  
}

var cboBatNbr_Change = function (item, newValue, oldValue) {
    var record = App.stoBatch.getById(newValue);
    App.cboReasonCD.store.reload();
    if (record) {
        HQ.isNew = false;
        bindBatch(record);
    } else {
        if (HQ.recentRecord != record) {

        }
        else {
        }
    }
    if (newValue != null && newValue != "") {
        keyLoadData = false;
    } else {
        keyLoadData = true;
    }
    HQ.recentRecord = record;
}
var cboStatus_Change = function (item, newValue, oldValue) {
    App.cboHandle.getStore().reload();
}

var cboReasonCD_Change = function (item, newValue, oldValue) {
    if (newValue == "TU" && !HQ.hideWarehouss) {
        App.cboAdvanceType.setVisible(true);
        App.btnLoad.setVisible(true);
        App.cboAdvanceType.allowBlank = false;
        App.btnLoad.disable();
        App.frmMain.isValid();
    }
    else {
        App.cboAdvanceType.setVisible(false);
        App.btnLoad.setVisible(false);
        App.cboAdvanceType.allowBlank = true;
        App.frmMain.isValid();
    }
    App.cboAdvanceType.isValid();
   // App.frmMain.isValid();

}

var cboAdvanceType_Change = function (item, newValue, oldValue) {
    _advanceType = App.cboAdvanceType.getValue();
    if (newValue == 'TH') {
        App.btnLoad.enable();
    }
    else {
        App.btnLoad.disable();
    }
    if (App.cboBatNbr.getValue() == null || App.cboBatNbr.getValue() == '') {
        if ((newValue != oldValue)) {
            App.cboSiteID.setValue('');
            App.cboToSiteID.setValue('');
            App.cboSiteID.store.reload();
            App.cboToSiteID.store.reload();
        }
    }
    if(App.cboBatNbr.getValue() != null && App.cboBatNbr.getValue() != '')
    {
        _advanceType = '';
        App.cboSiteID.store.reload();
        App.cboToSiteID.store.reload();
    }
}

var cboToSiteID_Change = function (sender) {
    //if (App.cboToSiteID.getValue() == App.cboSiteID.getValue()) {
    //    App.cboToSiteID.setValue('');
    //}
    if (sender.hasFocus) {
        if (HQ.showWhseLoc == 0 && App.cboSiteID.getValue() == App.cboToSiteID.getValue()) {
            App.cboToSiteID.setValue('');
        }
        App.cboToWhseLoc.setValue('');
    }
    App.cboToWhseLoc.store.reload();
    App.cboTransInvtID.store.reload();
}
var cboSiteID_Change = function (sender, e) {
    //if (App.cboToSiteID.getValue() == App.cboSiteID.getValue()) {
    //    App.cboSiteID.setValue('');
    //}
    if (sender.valueModels != undefined && sender.valueModels.length > 0) {
        //if (sender.valueModels[0].data.SiteType == true) {
        //    showHideControll("yes");
        //}
        //else {
        //    showHideControll("no")
        //}
    }
    if (sender.hasFocus) {
        if (HQ.showWhseLoc == 0 && App.cboSiteID.getValue()==App.cboToSiteID.getValue()) {
            App.cboSiteID.setValue('');
        }
        App.cboWhseLoc.setValue('');
    }
    App.cboToSiteID.store.reload();
    App.cboWhseLoc.store.reload();
    App.cboTransInvtID.store.reload();
}

var cboWhseLoc_Change = function (sender, e) {
    if (App.cboToSiteID.getValue() == App.cboSiteID.getValue() && App.cboWhseLoc.getValue() == App.cboToWhseLoc.getValue()) {
        if (App.cboToSiteID.getValue() != '' && App.cboSiteID.getValue() != '' && App.cboWhseLoc.getValue() != '' && App.cboToWhseLoc.getValue() != '') {
            HQ.message.show(2018062201, [App.cboSiteID.getValue(), App.cboWhseLoc.getValue(), App.cboToSiteID.getValue(), App.cboToWhseLoc.getValue()], '', true);
        }
        App.cboWhseLoc.setValue('');
    }
    App.cboToWhseLoc.store.reload();
    App.cboTransInvtID.store.reload();
}
var cboToWhseLoc_Change = function () {
    if (App.cboToSiteID.getValue() == App.cboSiteID.getValue() && App.cboWhseLoc.getValue() == App.cboToWhseLoc.getValue()) {
        if (App.cboToSiteID.getValue() != '' && App.cboSiteID.getValue() != '' && App.cboWhseLoc.getValue() != '' && App.cboToWhseLoc.getValue() != '') {
            HQ.message.show(2018062201, [App.cboSiteID.getValue(), App.cboWhseLoc.getValue(), App.cboToSiteID.getValue(), App.cboToWhseLoc.getValue()], '', true);
        }
        App.cboToWhseLoc.setValue('');
    }
    App.cboToWhseLoc.store.reload();
    App.cboTransInvtID.store.reload();
}

var cboTrnsferNbr_Change = function () {
    if (Ext.isEmpty(App.cboBatNbr.getValue())) {
        if (!Ext.isEmpty(App.TrnsferNbr.getValue())) {
            App.btnImport.setDisabled(true);
            App.stoTransfer.load({
                params: { branchID: App.BranchID.getValue(), tranDate: App.txtTranDate.getValue(), trnsfrDocNbr: App.TrnsferNbr.getValue() },
                callback: function () {
                    HQ.isTransfer = true;
                    App.stoTrans.removeAll();
                    App.stoTransfer.data.items.forEach(function (item) {
                        var newTrans = Ext.create('App.mdlTransfer');
                        newTrans.data.JrnlType = item.data.JrnlType;
                        newTrans.data.ReasonCD = item.data.ReasonCD;
                        newTrans.data.TranAmt = item.data.TranAmt;
                        newTrans.data.BatNbr = item.data.BatNbr;
                        newTrans.data.BranchID = item.data.BranchID;
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
                        newTrans.data.WhseLoc = item.data.WhseLoc;
                        newTrans.data.ToWhseLoc = item.data.ToWhseLoc;
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
                }
            });

        } else {
            if (Ext.isEmpty(App.cboBatNbr.getValue())) {
                App.btnImport.setDisabled(false);
            }
            HQ.isTransfer = false;
            App.stoTrans.removeAll();
            HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);
            checkTransAdd();
            calculate();
        }
    }
}

var grdTrans_BeforeEdit = function (item, e) {
    if (!HQ.grid.checkBeforeEdit(e, ['InvtID'])) return false;
    if (App.grdTrans.isLock) {
        return false;
    }
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (Ext.isEmpty(App.cboSiteID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('SiteID')], '', true);
        return false;
    }
    if (HQ.showWhseLoc == 2) {
        if (Ext.isEmpty(App.cboWhseLoc.getValue())) {
            HQ.message.show(1000, [HQ.common.getLang('WhseLoc')], '', true);
            return false;
        }
        if (Ext.isEmpty(App.cboToWhseLoc.getValue())) {
            HQ.message.show(1000, [HQ.common.getLang('ToWhseLoc')], '', true);
            return false;
        }
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
        e.record.data.TranType = 'TR';
        e.record.data.JrnlType = 'IN';
        e.record.data.BranchID = HQ.cpnyID;
        e.record.data.RefNbr = App.txtRefNbr.getValue();
        e.record.data.BatNbr = HQ.objBatch.data.BatNbr;
        e.record.data.TranDate = App.txtTranDate.getValue();
        e.record.data.SiteID = App.cboSiteID.getValue();
        e.record.data.ToSiteID = App.cboToSiteID.getValue();
        e.record.data.WhseLoc = App.cboWhseLoc.getValue();
        e.record.data.ToWhseLoc = App.cboToWhseLoc.getValue();
        e.record.commit();
    }

    if (key == 'UnitPrice') {
        var invt = e.row.invt;
        if (!Ext.isEmpty(invt) && invt.ValMthd == 'T') {
            return false;
        }
    }
    App.cboTransUnitDesc.setValue('');
}
var grdTrans_SelectionChange = function (item, selected) {
    HQ.focus = 'trans';
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectTrans = 0;
            //App.grdTrans.view.loadMask.show();

            HQ.common.showBusy(true,'Process...');
            App.stoItemSite.load({
                params: { siteID: App.cboSiteID.getValue(), invtID: selected[0].data.InvtID, whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSelect,
                row: selected[0]
            });
            App.stoUnit.load({
                params: { invtID: selected[0].data.InvtID },
                callback: checkSelect,
                row: selected[0]
            });
            App.stoOldTrans.load({
                params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue(), refNbr: App.txtRefNbr.getValue() },
                callback: checkSelect,
                row: selected[0]
            });
        } else {
            App.lblQtyAvail.setText('');
        }
    }
}
var grdTrans_Edit = function (item, e) {
    HQ.focus = 'trans';
    var key = e.field;
    if (Object.keys(e.record.modified).length > 0) {
        if (e.record.invt == undefined) {
            e.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]);
        }
        var invt = e.record.invt;
        if (!Ext.isEmpty(invt)) {

            if (key == 'InvtID' && Ext.isEmpty(e.record.data.UnitDesc)) {
                var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);
                if (!Ext.isEmpty(cnv)) {
                    e.record.data.UnitDesc = invt.StkUnit;
                    e.record.data.CnvFact = cnv.CnvFact == 0 ? 1 : cnv.CnvFact;
                    e.record.data.UnitMultDiv = cnv.MultDiv;
                } else {
                    return;
                }
            }

            if (key == 'InvtID') {
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
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue(), refNbr: App.txtRefNbr.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoPrice.load({
                    params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.txtTranDate.getValue(), branchID: e.record.data.BranchID, siteID: e.record.data.SiteID, valMthd: e.record.invt.ValMthd },
                    callback: checkSourceEdit,
                    row: e
                });
            } else if (key == 'UnitDesc') {
                //App.grdTrans.view.loadMask.show();
                HQ.common.showBusy(true, 'Process...');
                HQ.numEditTrans = 0;
                HQ.maxEditTrans = 3;
                App.stoItemSite.load({
                    params: { siteID: App.cboSiteID.getValue(), invtID: e.record.data.InvtID, whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoOldTrans.load({
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue(), refNbr: App.txtRefNbr.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoPrice.load({
                params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.txtTranDate.getValue(), branchID: e.record.data.BranchID, siteID: e.record.data.SiteID, valMthd: e.record.invt.ValMthd },
                    callback: checkSourceEdit,
                    row: e
                });
            } else {
                checkExitEdit(e);
            }

        } else {

        }
    }
}
var grdTrans_ValidateEdit = function (item, e) {
}

var grdLot_BeforeEdit = function (item, e) {
    if (App.grdLot.isLock) {
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
}
var grdLot_SelectionChange = function (item, selected) {
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectLot = 0;
            HQ.maxSelectLot = 1;
            HQ.common.showBusy(true, 'Process...');
            App.stoItemLot.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, batNbr: App.cboBatNbr.getValue(), whseLoc:App.cboWhseLoc.getValue(),showWhseLoc:HQ.showWhseLoc },
                callback: checkSelectLot,
                row: selected[0]
            });
        } else {
            App.lblLotQtyAvail.setText('');
        }
    }
}
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
            params: { siteID: lot.SiteID, invtID: lot.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: lot.LotSerNbr,batNbr: App.cboBatNbr.getValue(), whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSourceEditLot,
                row: e
            });
        }
    }
}
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


//// Function ////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

///// data ///////////////////////

var bindTran = function () {

    App.lblQtyAvail.setText('');

    var newDisplay = Ext.create("App.mdlTrans", {
        InvtID: '',
        RptExpDate: HQ.businessDate,
        AdvanceType: ''
    });

    if (App.cboStatus.getValue() == 'H')//trạng thái đang chờ xử lý thì cho thêm dòng mới, còn xử lý hoàn tất thì không cho thêm('H' là chờ xử lý, còn 'C' là xử lý hoàn tất)
        {
        HQ.store.insertRecord(App.stoTrans, "InvtID", newDisplay);
    }

    checkTransAdd();

    calculate();

    HQ.common.showBusy(false, HQ.waitMsg);
    setChange(false);
}
var bindBatch = function (record) {
    _advanceType = '';
    App.cboToSiteID.store.reload();
    App.cboToSiteID.store.reload();
    HQ.objBatch = record;
    App.cboBatNbr.events['change'].suspend();
    App.cboSiteID.events['change'].suspend();
    App.cboToSiteID.events['change'].suspend();
    App.cboToCpnyID.events['change'].suspend();
    App.frmMain.events['fieldchange'].suspend();
    App.cboStatus.forceSelection = false;
    App.frmMain.loadRecord(HQ.objBatch);
    App.cboStatus.forceSelection = false;
    App.cboWhseLoc.store.reload();
    App.cboToWhseLoc.store.reload();
    //App.cboToSiteID.store.reload();
    App.frmMain.events['fieldchange'].resume();
    App.cboBatNbr.events['change'].resume();
    App.cboSiteID.events['change'].resume();
    App.cboAdvanceType.events['change'].resume();
    App.cboToSiteID.events['change'].resume();
    App.cboToCpnyID.events['change'].resume();
    setStatusForm();
    HQ.common.showBusy(true, HQ.waitMsg);

    HQ.numDetail = 0;
    App.stoLotTrans.reload();
    App.stoTrans.reload();
    if (App.cboBatNbr.getValue() != null && App.cboBatNbr.getValue()!="") {
        keyLoadData = false;
        App.btnLoad.disable();
    }
    if (App.cboBatNbr.getValue() == null || App.cboBatNbr.getValue() == "") {
        if (HQ.dflReasonCD) {
            App.cboReasonCD.store.filter('Dfl', '1');
            var tamReasonCD = '';
            if (App.cboReasonCD.store.data.items.length > 0) {                
                tamReasonCD = App.cboReasonCD.store.data.items[0].data.ReasonCD;
            }
            App.cboReasonCD.setValue(tamReasonCD);
            App.cboReasonCD.store.clearFilter();
        }
    }        
    if (HQ.readOnlyReasonCD) {      
        App.cboReasonCD.setReadOnly(HQ.readOnlyReasonCD);
        
    } 
    if (!HQ.readOnlyReasonCD) {
        //App.cboReasonCD.setValue('');
        App.cboReasonCD.setReadOnly(HQ.readOnlyReasonCD);
    }
    if (App.cboReasonCD.getValue() == 'TU' && !HQ.hideWarehouss) {
        App.cboAdvanceType.allowBlank = false;
    }
    if (App.cboReasonCD.getValue() != 'TU') {
        App.cboAdvanceType.allowBlank = true;
    }

    App.cboHandle.setValue('N');

}

var save = function () {
    var checkPerPost = true;
    if (HQ.CheckperPost) {
        var objPerPost = HQ.store.findRecord(App.cboPerPost.store, ['CycleNbr'], [App.cboPerPost.getValue()]);
        if (objPerPost != undefined) {
            var tam = App.txtTranDate.getValue();
            if (tam > objPerPost.data.EndDate || tam < objPerPost.data.StartDate) {
                checkPerPost = false;
            }
        }
        else {
            checkPerPost = false;
        }
    }
    if ((App.cboBatNbr.value && !HQ.isUpdate) || (Ext.isEmpty(App.cboBatNbr.value) && !HQ.isInsert)) {
        HQ.message.show(728, '', '', true);
        return;
    }
    if (App.cboStatus.getValue() != "H" && (App.cboHandle.getValue() == "N" || Ext.isEmpty(App.cboHandle.getValue()))) {
        HQ.message.show(2015020803, '', '', true);
        return;
    }

    if (App.txtBranchID.getValue() != App.cboToCpnyID.getValue() && App.cboTransferType.getValue() == "1") {
        HQ.message.show(1092, '', '', true);
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
            url: 'IN10300/Save',
            timeout: 180000,
            params: {
                lstTrans: Ext.encode(App.stoTrans.getRecordsValues()),
                lstLot: Ext.encode(App.stoLotTrans.getRecordsValues())
            },
            success: function (msg, data) {

                var batNbr = '';

                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    var batNbr = this.result.data.batNbr
                }
                if (HQ.CheckperPost && !checkPerPost) {
                    HQ.message.show(2018071311, '', '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
                if (!Ext.isEmpty(batNbr)) {
                    App.cboBatNbr.forceSelection = false
                    App.cboBatNbr.events['change'].suspend();
                    App.cboBatNbr.setValue(batNbr);
                    App.cboBatNbr.events['change'].resume();
                    if (Ext.isEmpty(HQ.recentRecord)) {
                        HQ.recentRecord = batNbr;
                    }
                    App.stoBatch.reload();
                }

                setChange(false);
                //HQ.message.process(msg, data, true);
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
                url: 'IN10300/Delete',
                timeout: 180000,
                success: function (msg, data) {
                    var record = App.stoBatch.getById(App.cboBatNbr.getValue());
                    if (!Ext.isEmpty(record)) {
                        App.stoBatch.remove(record);
                    }
                    HQ.message.process(msg, data, true);
                    setChange(false);
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
        var det = App.smlTrans.selected.items[0].data;
        App.stoLotTrans.clearFilter();
        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (det.LineRef == App.stoLotTrans.data.items[i].data.INTranLineRef) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
        if (App.cboBatNbr.value) {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'IN10300/DeleteTrans',
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
                    HQ.common.changeData(HQ.isChange, 'IN10300');
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        } else {
            App.grdTrans.deleteSelected();
            calculate();
        }
    }
};
var deleteLot = function (item) {
    if (item == 'yes') {
        App.grdLot.deleteSelected();
    }
}

var report = function () {
    App.frmMain.submit({
        waitMsg: HQ.waitMsg,
        method: 'POST',
        clientValidation: false,
        url: 'IN10300/Report',
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
}
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
                    cnvFact: det.CnvFact
                },
                det: record.data,
                row: record,
                callback: function (records, options, success) {

                    var det = options.det;
                    var needQty = Math.round(det.UnitMultDiv == "M" ? det.Qty * det.CnvFact : det.Qty / det.CnvFact);

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
                            newLot.data.WhseLoc = det.WhseLoc;
                            newLot.data.ToWhseLoc = det.ToWhseLoc;
                            newLot.data.InvtID = det.InvtID;
                            newLot.data.InvtMult = -1;
                            if ((det.UnitMultDiv == "M" ? newQty / det.CnvFact : newQty * det.CnvFact) % 1 > 0) {
                                newLot.data.CnvFact = 1;
                                newLot.data.UnitMultDiv = 'M';
                                newLot.data.Qty = newQty;
                                newLot.data.UnitDesc = det.UnitDesc;
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
}

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
                cnvFact: record.data.CnvFact
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
    newRow.data.WarrantyDate = record.data.WarrantyDate;
    newRow.data.PercentExpDate = record.data.PercentExpDate;
    newRow.data.WhseLoc = App.cboWhseLoc.getValue();
    newRow.data.ToSiteID = App.cboToSiteID.getValue();
    newRow.data.ToWhseLoc = App.cboToWhseLoc.getValue();
    HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);

    App.winLot.record = record;
    App.grdLot.view.refresh();
    App.winLot.setTitle(record.data.InvtID + ' ' + (record.data.UnitMultDiv == "M" ? record.data.Qty * record.data.CnvFact : record.data.Qty / record.data.CnvFact) + ' ' + record.invt.StkUnit);
    App.winLot.show();
}

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
}
var cboToCpnyID_Change = function (sender,e) {
    App.cboToSiteID.setValue('');
    if (!Ext.isEmpty(e)) {
        App.cboToSiteID.store.reload();
    }
    App.cboTransInvtID.store.reload();
}
var defaultOnNew = function () {
    HQ.isNew = true;
    var record = Ext.create('App.mdlBatch');
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'H';
    record.data.DateEnt = HQ.businessDate;
    record.data.TranDate = HQ.businessDate;
    record.data.ExpectedDate = HQ.businessDate;
    record.data.RcptDate = HQ.businessDate;
    record.data.PerPost = HQ.PerPost;
    //record.data.SiteID = HQ.objUser.INSite;
    if (HQ.isSetDefaultSiteID) {
        record.data.SiteID = HQ.objUser.INSite;
        if (HQ.showWhseLoc != 0) {
            record.data.WhseLoc = HQ.objUser.INWhseLoc;
        }
    }    
    record.data.ToCpnyID = HQ.cpnyID;
    record.data.TransferType = "1";
    record.data.TransferStatus = "P";
    //record.data.ShipViaID = "MD";
    if (HQ.isSetDefaultShipViaID && App.cboShipViaID.store.data.length > 0) {
        record.data.ShipViaID = App.cboShipViaID.store.data.items[0].data.ShipViaID;
    }

    App.frmMain.validate();
    bindBatch(record);
}
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
}
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
}
var renderRowNumber = function (value, meta, record) {
    return App.stoLotTrans.data.indexOf(record) + 1;
}
var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
}
var setStatusForm = function () {
    var flag = true
    var lock = true;
    if (!Ext.isEmpty(HQ.objBatch.data.BatNbr)) {
        if (HQ.objBatch.data.Status == 'H') {
            lock = false;
        }
    } else {
        lock = !HQ.isInsert;
    }

    //HQ.common.lockItem(App.frmMain, lock);
    //App.grdTrans.isLock = lock;
    App.cboBatNbr.setReadOnly(false);
    App.cboHandle.setReadOnly(false);
    App.txtRcptBatNbr.setReadOnly(true);
    App.cboStatus.setReadOnly(true);
    App.txtRefNbr.setReadOnly(true);
    App.txtTotAmt.setReadOnly(true);
    App.txtTrnsfrDocNbr.setReadOnly(true);
    App.cboTransferStatus.setReadOnly(true);
    
    //App.cboSiteID.setReadOnly(true);
    //App.cboToSiteID.setReadOnly(true);
    //App.cboToCpnyID.setReadOnly(true);
    if (!HQ.isInsert && HQ.isNew) {
        lock = true;
        flag = false;
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        lock = true;
        flag = false;
    }

    HQ.common.lockItem(App.frmMain, lock);
    App.grdTrans.isLock = lock;
    if (flag == true && App.cboStatus.getValue() != 'H') {
        App.cboHandle.setReadOnly(false);
    }
}
var checkExitEdit = function (row) {
    var key = row.field;
    var trans = row.record.data;
    if (key == 'InvtID' || key == 'BarCode') {

        trans.ReasonCD = App.cboReasonCD.getValue();
        trans.SiteID = App.cboSiteID.getValue();
        trans.WhseLoc = App.cboWhseLoc.getValue();
        trans.ToWhseLoc = App.cboToWhseLoc.getValue();
        var invt = row.record.invt;
        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);

        if (Ext.isEmpty(cnv)) {
            trans.UnitMultDiv = '';
            trans.UnitPrice = 0;
            row.record.commit();
            //App.grdTrans.view.loadMask.hide();
            //App.grdTrans.view.loadMask.setDisabled(false);
            HQ.common.showBusy(false);
            return;
        }

        trans.UnitDesc = invt.StkUnit;
        trans.CnvFact = cnv.CnvFact == 0 ? 1 : cnv.CnvFact;
        trans.UnitMultDiv = cnv.MultDiv;
        trans.TranDesc = invt.Descr;
        trans.BarCode = invt.BarCode;

        var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [trans.InvtID, trans.SiteID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
        }

        //if (invt.ValMthd == "A" || invt.ValMthd == "E") {
        //    trans.UnitPrice = site.AvgCost;
        //}
        //else {
            trans.UnitPrice = App.stoPrice.data.items[0].data.Price;
        //}
        trans.TranAmt = trans.Qty * trans.UnitPrice;

        getQtyAvail(row.record);

    } else if (key == 'UnitDesc') {

        var invt = row.record.invt;

        var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [trans.InvtID, trans.SiteID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
        }

        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, trans.UnitDesc);

        if (Ext.isEmpty(cnv)) {
            trans.UnitMultDiv = '';
            trans.UnitPrice = trans.UnitCost = 0;
            trans.Qty = 0;
            trans.TranAmt = 0;
            row.record.commit();
            //App.grdTrans.view.loadMask.hide();
            //App.grdTrans.view.loadMask.setDisabled(false);
            HQ.common.showBusy(false);
            return;
        }


        trans.CnvFact = cnv.CnvFact;
        trans.UnitMultDiv = cnv.MultDiv;
        if (invt.ValMthd == "A" || invt.ValMthd == "E") {
            trans.UnitPrice = Math.round((trans.UnitMultDiv == "M" ? site.AvgCost * trans.CnvFact : site.AvgCost / trans.CnvFact));
        }
        else {
            trans.UnitPrice = App.stoPrice.data.items[0].data.Price;
        }
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
                setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);
                totQty = editQty + calculateInvtTotal(trans.InvtID, trans.SiteID, trans.LineRef);
                if (totQty > site.QtyAvail) {
                    trans.Qty = 0;
                    row.record.commit();
                    HQ.message.show(35, '', '', true);
                    //App.grdTrans.view.loadMask.hide();
                    //App.grdTrans.view.loadMask.setDisabled(false);
                    HQ.common.showBusy(false);
                    return;
                }
            }
        }
        trans.TranAmt = trans.Qty * trans.UnitPrice;
        getQtyAvail(row.record);
        calcLot(row.record);
    }
    else if (key == 'RptExpDate') {
        if (App.dteExpDate.value == null || App.dteExpDate.value == "")
        {
            trans.RptExpDate = HQ.businessDate;
        }
    }

    trans.ExtCost = trans.TranAmt;
    trans.UnitCost = trans.UnitPrice;
    row.record.commit();

    if (key == 'InvtID' && !Ext.isEmpty(trans.InvtID)) {
        var rec = Ext.create("App.mdlTrans", {
            InvtID:'',
            RptExpDate:HQ.businessDate
        });
        HQ.store.insertRecord(App.stoTrans, key, rec);
    }

    calculate();

    checkTransAdd();

    //App.grdTrans.view.loadMask.hide();
    //App.grdTrans.view.loadMask.setDisabled(false);
    HQ.common.showBusy(false);
}
var checkExitEditLot = function (row) {
    var key = row.field;
    var record = row.record;
    var lot = row.record.data;
    if (key == "Qty") {
        getLotQtyAvail(record);
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
            //App.grdLot.view.loadMask.hide();
            //App.grdLot.view.loadMask.setDisabled(false)
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

        getLotQtyAvail(record);
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
            //App.grdLot.view.loadMask.hide();
            //App.grdLot.view.loadMask.setDisabled(false)
            HQ.common.showBusy(false);
            record.commit();
            return;
        }
        lot.UnitDesc = App.winLot.record.data.UnitDesc;
        lot.UnitPrice = lot.UnitCost = App.winLot.record.data.UnitPrice;
        lot.UnitMultDiv = App.winLot.record.data.UnitMultDiv;
        lot.CnvFact = App.winLot.record.data.CnvFact;
        var itemLot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', 'LotSerNbr'], [lot.InvtID, lot.SiteID, lot.LotSerNbr]);
        if (!Ext.isEmpty(itemLot)) {
            lot.ExpDate = itemLot.ExpDate;
            lot.WarrantyDate = itemLot.WarrantyDate;
            lot.PercentExpDate = itemLot.PercentExpDate;
            lot.WhseLoc = itemLot.WhseLoc;
        }

        if (!Ext.isEmpty(lot.LotSerNbr)) {
            var newRow = Ext.create('App.mdlLotTrans');
            newRow.data.INTranLineRef = lot.INTranLineRef;
            newRow.data.ToWhseLoc = record.data.ToWhseLoc;
            HQ.store.insertRecord(App.stoLotTrans, key, newRow, true);
        }
    }
    record.commit();
    //App.grdLot.view.loadMask.hide();
    //App.grdLot.view.loadMask.setDisabled(false)
    HQ.common.showBusy(false);
}
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
    App.cboSiteID.setReadOnly(flat);
    if (flat || HQ.readOnlyReasonCD) {
        App.cboReasonCD.setReadOnly(true);
    }
    else {
        App.cboReasonCD.setReadOnly(flat);
    }
    App.cboAdvanceType.setReadOnly(flat);
    App.cboToSiteID.setReadOnly(flat);
    App.cboToCpnyID.setReadOnly(flat);
    App.cboTransferType.setReadOnly(flat);
    App.cboWhseLoc.setReadOnly(flat);
    App.cboToWhseLoc.setReadOnly(flat);
}
var getQtyAvail = function (row) {

    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [row.data.InvtID, row.data.SiteID]);
    var qty = 0;
    if (!Ext.isEmpty(site)) {
        qty = site.QtyAvail - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
    }
    else {
        qty = 0 - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
    }
    qty = HQ.util.mathRound(row.data.UnitMultDiv == "M" ? qty / row.data.CnvFact : qty * row.data.CnvFact, 2);
    App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ": " + qty + " " + row.data.UnitDesc);
}
var getLotQtyAvail = function (row) {
    var lot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', ['LotSerNbr']], [row.data.InvtID, row.data.SiteID, row.data.LotSerNbr]);
    var qty = 0;
    var qtyAvail = 0;

    App.stoLotTrans.snapshot.each(function (item2) {
        if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID) {
            qty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
        }
    });
    var det = App.winLot.record;

    if (!Ext.isEmpty(lot)) {
        qtyAvail = lot.QtyAvail - qty;
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
                    qty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
                }
            });
            qtyAvail = 0 - qty;
        }
    }
    App.lblLotQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + qtyAvail);

}
var calculateInvtTotal = function (invtID, siteID, lineRef) {
    var qty = 0;
    var qtyOld = 0;
    var store = App.stoTrans;
    var allRecords = store.snapshot || store.allData || store.data;

    allRecords.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID && ((lineRef != "" && item.data.LineRef != lineRef) || lineRef == "")) {
            qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
        }
    });
    App.stoOldTrans.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID) {
            qtyOld += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
        }
    });
    return qty - qtyOld;
}
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
}
var askRefresh = function (item) {
    if (item == "yes" || item == "ok") {
        if (!Ext.isEmpty(App.cboBatNbr.getValue())) {
            App.stoBatch.reload();
        } else {
            defaultOnNew();
        }
        App.cboAdvanceType.setValue('');
        App.cboAdvanceType.setReadOnly(false);
    }
}
var askNew = function (item) {
    if (item == "yes" || item == "ok") {
        defaultOnNew();
    }
}
var setChange = function (isChange) {
    HQ.isChange = isChange;
    if (isChange) {
        App.cboBatNbr.setReadOnly(true);
    } else {
        App.cboBatNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'IN10300');
}
var showHideControll = function (key) {
    if (key == "yes") {
        if (keyLoadData) {
            App.btnLoad.enable();
        }        
        App.txtExpectedDate.setReadOnly(true);
        App.txtRcptDate.setReadOnly(true);
        //App.cboReasonCD.setValue('TU');
        //App.cboReasonCD.setReadOnly(HQ.readOnlyReasonCD);
        //App.cboAdvanceType.allowBlank = false;
        App.frmMain.isValid();
    }
    else {
        App.btnLoad.disable();
        //App.cboReasonCD.setReadOnly(false);
        //App.cboReasonCD.setValue('');
        App.txtExpectedDate.setReadOnly(false);
        App.txtRcptDate.setReadOnly(false);
        //App.cboAdvanceType.allowBlank = true;
        App.frmMain.isValid();
    }
    
}

var btnExport_Click = function () { 
   App.frmMain.submit({
       waitMsg: HQ.common.getLang("Exporting"),
       url: 'IN10300/Export',
       type: 'POST',
       timeout: 1000000,
       clientValidation: false,
       params: {
           //lstCustTD: Ext.encode(App.stoDiscBreak.getRecordsValues()),// HQ.store.getData(App.stoAR_CustomerTD)
           //lstCustTD1: Ext.encode(App.cboExport.getValue()),
           //templateExport: App.cboExport.getValue()
       },
       success: function (msg, data) {
           window.location = 'IN10300/DownloadAndDelete?file=' + data.result.fileName;
       },
       failure: function (msg, data) {
           HQ.message.process(msg, data, true);
       }
   });
};

var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'IN10300/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                if (!Ext.isEmpty(this.result.data.message)) {
                    var result = "<div style ='overflow: auto !important; min-width:400px !important; max-height:400px !important'> " + this.result.data.message + " </div>";
                    HQ.message.show(20410, [result], '', true);
                    //HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            },
            failure: function (msg, data) {
                if (data.res) {
                    //HQ.message.show(20410, [message], "", true);
                    var result = "<div style ='overflow: auto !important; min-width:400px !important; max-height:400px !important'> " + data.result.message + " </div>";

                    HQ.message.show(20410, [result], '', true);
                } else {
                    HQ.message.show(8009, [message], "", true);
                }
               // HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701', ext, '');
        sender.reset();
    }
}
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
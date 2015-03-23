HQ.recentRecord = null;
HQ.focus = 'batch';
HQ.objBatch = null;
HQ.isTransfer = false;
HQ.objUser = null;
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
    App.grdTrans.view.loadMask.disable();
}
var stoHandle_Load = function () {
    App.cboHandle.setValue('N');
}
var stoTrans_Load = function () {
    bindTran();
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
        App.grdTrans.view.loadMask.hide();
        App.grdTrans.view.loadMask.setDisabled(false)
        getQtyAvail(options.row);
    }
}
var checkSetDefault = function () {
    if (HQ.numSource == HQ.maxSource) {
        defaultOnNew();
    }
}
var checkSourceEdit = function (records, options, success) {
    HQ.numTrans++;
    if (HQ.numTrans == HQ.maxTrans) {
        checkExitEdit(options.row);
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
    HQ.numSource = 0;
    HQ.maxSource = 12;
    HQ.numTrans = 0;
    HQ.maxTrans = 4;
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

    HQ.common.showBusy(true, HQ.waitMsg);
}
var frmMain_FieldChange = function (item, field, newValue, oldValue) {
    if (field.key != undefined) {
        return;
    }
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (Object.keys(App.stoBatch.getChangedData()).length > 0 || App.grdTrans.isChange) {
        setChange(true);
    } else {
        setChange(false);
    }

}
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
                    var index = App.stoBatch.indexOf(App.stoBatch.getById(App.cboBatNbr.getValue()));
                    App.cboBatNbr.setValue(App.stoBatch.getAt(index + 1).get('BatNbr'));
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
                    var index = App.stoBatch.indexOf(App.stoBatch.getById(App.cboBatNbr.getValue()));
                    App.cboBatNbr.setValue(App.stoBatch.getAt(index - 1).get('BatNbr'));
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
            if (!App.grdTrans.view.loadMask.isHidden()) {
                return;
            }
            save();
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
            if ((HQ.isChange || App.grdTrans.isChange) && !Ext.isEmpty(App.cboBatNbr.getValue())) {
                HQ.message.show(2015030201, '', "askNew", true);
            } else {
                defaultOnNew();
            }
            break;
        case "refresh":
            if (!Ext.isEmpty(App.cboBatNbr.getValue())) {
                App.stoBatch.reload();
            } else {
                defaultOnNew();
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

var cboBatNbr_Change = function (item, newValue, oldValue) {
    var record = App.stoBatch.getById(newValue);
    if (record) {
        bindBatch(record);
    } else {
        if (HQ.recentRecord != record) {

        }
        else {
        }
    }
    HQ.recentRecord = record;
}
var cboStatus_Change = function (item, newValue, oldValue) {
    App.cboHandle.getStore().reload();
}
var cboToSiteID_Change = function () {
    if (App.cboToSiteID.getValue() == App.cboSiteID.getValue()) {
        App.cboToSiteID.setValue('');
    }
}
var cboSiteID_Change = function () {
    if (App.cboToSiteID.getValue() == App.cboSiteID.getValue()) {
        App.cboSiteID.setValue('');
    }
}
var cboToCpnyID_Change = function () {
    App.stoToSite.clearFilter();
    App.cboToSiteID.setValue('');
    if (Ext.isEmpty(App.cboToCpnyID.getValue())) {
        App.stoToSite.filter('CpnyID', "##@##");
    } else {
        App.stoToSite.filter('CpnyID', App.cboToCpnyID.getValue());
    }

}
var cboTrnsferNbr_Change = function () {
    if (Ext.isEmpty(App.cboBatNbr.getValue())) {
        if (!Ext.isEmpty(App.TrnsferNbr.getValue())) {
            App.btnImport.setDisabled(true);
            App.stoTransfer.load({
                params: { branchID: App.BranchID.getValue(), tranDate: App.DateEnt.getValue(), trnsfrDocNbr: App.TrnsferNbr.getValue() },
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

    if (App.grdTrans.isLock) {
        return false;
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
        e.record.data.TranType = 'TR';
        e.record.data.JrnlType = 'IN';
        e.record.data.BranchID = HQ.cpnyID;
        e.record.data.RefNbr = App.txtRefNbr.getValue();
        e.record.data.BatNbr = HQ.objBatch.data.BatNbr;
        e.record.data.TranDate = App.txtTranDate.getValue();
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
}
var grdTrans_SelectionChange = function (item, selected) {
    HQ.focus = 'trans';
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectTrans = 0;
            App.grdTrans.view.loadMask.show();
            App.stoItemSite.load({
                params: { siteID: App.cboSiteID.getValue(), invtID: selected[0].data.InvtID },
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
        App.grdTrans.isChange = true;
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

            if (key == 'InvtID' || key == 'Qty') {
                App.grdTrans.view.loadMask.show();
                HQ.numTrans = 0;
                HQ.maxTrans = 3;
                App.stoUnit.load({
                    params: { invtID: e.record.data.InvtID },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoItemSite.load({
                    params: { siteID: App.cboSiteID.getValue(), invtID: e.record.data.InvtID },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoOldTrans.load({
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue(), refNbr: App.txtRefNbr.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
            } else if (key == 'UnitDesc') {
                App.grdTrans.view.loadMask.show();
                HQ.numTrans = 0;
                HQ.maxTrans = 2;
                App.stoItemSite.load({
                    params: { siteID: App.cboSiteID.getValue(), invtID: e.record.data.InvtID },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoOldTrans.load({
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue(), refNbr: App.txtRefNbr.getValue() },
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

    HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);

    checkTransAdd();

    calculate();

    App.grdTrans.isChange = false;
    HQ.common.showBusy(false, HQ.waitMsg);
    setChange(false);
}
var bindBatch = function (record) {
    HQ.objBatch = record;
    App.cboBatNbr.events['change'].suspend();
    App.cboSiteID.events['change'].suspend();
    App.cboToSiteID.events['change'].suspend();
    App.cboToCpnyID.events['change'].suspend();
    App.frmMain.events['fieldchange'].suspend();
    App.cboStatus.forceSelection = false;
    App.frmMain.loadRecord(HQ.objBatch);
    App.cboStatus.forceSelection = false;
    App.frmMain.events['fieldchange'].resume();
    App.cboBatNbr.events['change'].resume();
    App.cboSiteID.events['change'].resume();
    App.cboToSiteID.events['change'].resume();
    App.cboToCpnyID.events['change'].resume();
    setStatusForm();


    HQ.common.showBusy(true, HQ.waitMsg);
    App.stoTrans.reload();
    App.cboHandle.setValue('N');

}

var save = function () {
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

    if (App.stoTrans.data.items.length <= 1) {
        HQ.message.show(2015020804, [App.cboBatNbr.value], '', true);
        return;
    }
    var flat = false;
    App.stoTrans.data.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
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
                isTransfer: HQ.isTransfer
            },
            success: function (msg, data) {

                var batNbr = '';

                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    var batNbr = this.result.data.batNbr
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


                HQ.message.process(msg, data, true);

                menuClick('refresh');
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
var calculate = function () {
    var totAmt = 0;
    var totQty = 0;

    App.stoTrans.data.each(function (item) {
        totAmt += item.data.TranAmt;
        totQty += item.data.Qty;
    });

    App.txtTotAmt.setValue(totAmt);
}

var defaultOnNew = function () {
    var record = Ext.create('App.mdlBatch');
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'H';
    record.data.DateEnt = HQ.businessDate;
    record.data.TranDate = HQ.businessDate;
    record.data.ExpectedDate = HQ.businessDate;
    record.data.RcptDate = HQ.businessDate;
    record.data.SiteID = HQ.objUser.INSite;
    record.data.ToCpnyID = HQ.cpnyID;
    record.data.TransferType = "1";
    record.data.TransferStatus = "P";
    record.data.ShipViaID = "MD";

    App.stoToSite.clearFilter();
    if (Ext.isEmpty(record.data.ToCpnyID)) {
        App.stoToSite.filter('CpnyID', '##@##');
    } else {
        App.stoToSite.filter('CpnyID', record.data.ToCpnyID);
    }


    App.frmMain.validate();
    bindBatch(record);
}

var lastLineRef = function () {
    var num = 0;

    App.stoTrans.data.each(function (item) {
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

var rdrTrans_QtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
}

var setStatusForm = function () {

    var lock = true;
    if (!Ext.isEmpty(HQ.objBatch.data.BatNbr)) {
        if (HQ.objBatch.data.Status == 'H') {
            lock = false;
        }

    } else {
        lock = !HQ.isInsert;
    }

    HQ.common.lockItem(App.frmMain, lock);
    App.grdTrans.isLock = lock;
    App.cboBatNbr.setReadOnly(false);
    App.cboHandle.setReadOnly(false);
    App.txtRcptBatNbr.setReadOnly(true);
    App.cboStatus.setReadOnly(true);
    App.txtRefNbr.setReadOnly(true);
    App.txtTotAmt.setReadOnly(true);
    App.txtTrnsfrDocNbr.setReadOnly(true);
    App.cboTransferStatus.setReadOnly(true);
}

var checkExitEdit = function (row) {
    var key = row.field;
    var trans = row.record.data;
    if (key == 'InvtID' || key == 'BarCode') {

        trans.ReasonCD = App.cboReasonCD.getValue();
        trans.SiteID = App.cboSiteID.getValue();

        var invt = row.record.invt;
        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);

        if (Ext.isEmpty(cnv)) {
            trans.UnitMultDiv = '';
            trans.UnitPrice = 0;
            row.record.commit();
            App.grdTrans.view.loadMask.hide();
            App.grdTrans.view.loadMask.setDisabled(false)
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

        if (invt.ValMthd == "A" || invt.ValMthd == "E") {
            trans.UnitPrice = site.AvgCost;
        }
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
            App.grdTrans.view.loadMask.hide();
            App.grdTrans.view.loadMask.setDisabled(false)
            return;
        }


        trans.CnvFact = cnv.CnvFact;
        trans.UnitMultDiv = cnv.MultDiv;
        if (invt.ValMthd == "A" || invt.ValMthd == "E") {
            trans.UnitPrice = Math.round((trans.UnitMultDiv == "M" ? site.AvgCost * trans.CnvFact : site.AvgCost / trans.CnvFact));
        }
        trans.TranAmt = trans.Qty * trans.UnitPrice;
        getQtyAvail(row.record);

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
                    App.grdTrans.view.loadMask.hide();
                    App.grdTrans.view.loadMask.setDisabled(false)
                    return;
                }
            }
        }
        trans.TranAmt = trans.Qty * trans.UnitPrice;
        getQtyAvail(row.record);
    }


    trans.ExtCost = trans.TranAmt;
    trans.UnitCost = trans.UnitPrice;
    row.record.commit();

    if (key == 'InvtID' && !Ext.isEmpty(trans.InvtID)) {

        HQ.store.insertRecord(App.stoTrans, key, Ext.create('App.mdlTrans'), true);
    }

    calculate();

    checkTransAdd();

    App.grdTrans.view.loadMask.hide();
    App.grdTrans.view.loadMask.setDisabled(false)
}

var checkTransAdd = function () {
    var flat = false;
    App.stoTrans.data.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
            flat = true;
            return false;
        }
    });

    App.cboSiteID.setReadOnly(flat);
}

var getQtyAvail = function (row) {

    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [row.data.InvtID, row.data.SiteID]);
    if (!Ext.isEmpty(site)) {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + (site.QtyAvail - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "")));
    }
    else {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + (0 - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "")));
    }
}
var calculateInvtTotal = function (invtID, siteID, lineRef) {
    var qty = 0;
    var qtyOld = 0;
    App.stoTrans.each(function (item) {
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
var askNew = function (item) {
    if (item == "yes" || item == "ok") {
        defaultOnNew();
    }
}
var setChange = function (isChange) {
    HQ.isChange = isChange;
    if (isChange) {
        if (!Ext.isEmpty(App.cboBatNbr.getValue())) {
            App.cboBatNbr.setReadOnly(true);
        }

    } else {
        App.grdTrans.isChange = false;
        App.cboBatNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'IN10300');
}
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////






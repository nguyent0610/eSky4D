//// Declare //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

HQ.recentRecord = null;
HQ.focus = 'batch';
HQ.objBatch = null;
HQ.isTransfer = false;
HQ.objUser = null;

var keys = ['InvtID'];

var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;
var _firstLoad = true;

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true
        _Source = 0;
        HQ.common.showBusy(false);
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
            if (HQ.focus == 'batch' ) {
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
            //if (!App.grdTrans.view.loadMask.isHidden()) {
            //    return;
            //}
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
            if (HQ.isChange|| App.grdTrans.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
        case "new":
            if (HQ.focus == 'batch') {
                // if (HQ.isChange || App.grdTrans.isChange) {
                if (HQ.isChange || App.grdTrans.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    defaultOnNew();
                }
            }
            else if (HQ.focus == 'trans') {
                HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true)
            }
            break;
          
        case "refresh":
            if (HQ.isChange || App.grdTrans.isChange) {
                HQ.message.show(20150303, '', "askRefresh", true);
            }
            else {
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



var firstLoad = function () {
    //HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.common.showBusy(true, HQ.waitMsg);
    HQ.numSource = 0;
    HQ.maxSource = 7;
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
    App.cboInventory.getStore().addListener('load', store_Load);
    //App.cboFromToSiteID.getStore().addListener('load', store_Load);
    //App.cboSlsperID.getStore().addListener('load', store_Load);
    App.cboSiteID.getStore().addListener('load', store_Load);
    App.stoUserDefault.addListener('load', stoUserDefault_Load);
    App.stoSetup.addListener('load', stoSetup_Load);
    App.stoUnitConversion.addListener('load', store_Load);
    App.cboReasonCD.getStore().addListener('load', store_Load);


    App.stoSetup.load();
    App.stoUserDefault.load();
    App.stoUnitConversion.load();
    App.stoInvt = App.cboInventory.getStore();
    App.smlTrans.tab = false;

    App.smlTrans.onEditorTab = function (field, e) {
        App.smlTrans.tab = true;
        if (field.activeColumn.dataIndex == 'Qty' || field.activeColumn.dataIndex == 'UnitDesc' || field.activeColumn.dataIndex == 'InvtID') {
            field.activeEditor.completeEdit();
        } else {
            origEditorTab(field, e);
        }

    }

    HQ.util.checkAccessRight();
    
    //HQ.common.showBusy(false)
};

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

};
var origEditorTab = function (instance, self) {
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
};
var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'IN10400?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
};
var grdTrans_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTrans);
    stoChanged(App.stoTrans);
    App.txtTotal.setValue(setValue(App.stoTrans))
};
var grdTrans_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, ['InvtID'])) return false;
    //if (App.grdTrans.isLock) {
    //    return false;
    //}

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
        e.record.data.InvtMult = 1;
        e.record.data.TranType = 'AJ';
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
            // App.grdTrans.view.loadMask.show();
            HQ.common.showBusy(true, 'Process...');
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

            if (key == 'InvtID' || key == 'Qty' || key == 'TranAmt') {
                if (key == "InvtID")
                {
                    if (Ext.isEmpty(e.record.data.InvtID))
                    {
                        return;
                    }
                }
                HQ.common.showBusy(true, 'Process...');
                HQ.numEditTrans = 0;
                HQ.maxEditTrans = 3;
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
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
            } else if (key == 'UnitDesc') {
                HQ.common.showBusy(true, 'Process...');
                HQ.numEditTrans = 0;
                HQ.maxEditTrans = 2;
                App.stoItemSite.load({
                    params: { siteID: App.cboSiteID.getValue(), invtID: e.record.data.InvtID },
                    callback: checkSourceEdit,
                    row: e
                });
                App.stoOldTrans.load({
                    params: { batNbr: App.cboBatNbr.getValue(), branchID: App.txtBranchID.getValue() },
                    callback: checkSourceEdit,
                    row: e
                });
            } else {
                checkExitEdit(e);
            }
            //if (key == 'UnitPrice')
            //{
            //    e.record.set('TranAmt', e.record.data.Qty * e.record.data.UnitCost);
            //    e.commit();
            //    calculate();
            //}

        } else {

        }
    } else {
        if (key != 'InvtID') handleTab(key);
    }
};
var grdTrans_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTrans, e, keys, false);
};

var grdLot_BeforeEdit = function (item, e) {
    if (App.grdLot.isLock) {
        return false;
    }

    var key = e.field;
    var record = e.record;
    if (key != 'LotSerNbr' && Ext.isEmpty(e.record.data.LotSerNbr)) return false;
    if (key == 'LotSerNbr' && !Ext.isEmpty(record.data.LotSerNbr)) return false;
    if (key == "UnitDesc") return false;

    if (Ext.isEmpty(record.data.InvtID)) {
        record.data.InvtID = App.winLot.record.data.InvtID;
        record.data.SiteID = App.winLot.record.data.SiteID;
    }

    record.commit();

    App.cboLotUnitDesc.setValue('');
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
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectLot = 0;
            HQ.maxSelectLot = 1;
            App.grdLot.view.loadMask.show();
            App.stoItemLot.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, batNbr: App.cboBatNbr.getValue() },
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
        // if (key == "Qty" || key == "UnitDesc") {
        if (key == "Qty") {
            checkExitEditLot(e);
        } else if (key == "LotSerNbr") {
            App.grdLot.view.loadMask.show();
            HQ.numLot = 0;
            HQ.maxLot = 1;
            App.stoItemLot.load({
                params: { siteID: lot.SiteID, invtID: lot.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: lot.LotSerNbr },
                callback: checkSourceEditLot,
                row: e
            });
        }
    }
};
/////////////////store/////

var stoBatch_Load = function () {
    var record = App.stoBatch.getById(App.cboBatNbr.getValue());
    if (record) {
        bindBatch(record);
    }
};

var stoDetail_Load = function () {
    checkSourceDetail();
};

var store_Load = function () {
    HQ.numSource++;
    checkSetDefault();
};

var stoBatch_BeforeLoad = function (store, operation, eOpts) {
    //if (Ext.isEmpty(operation.params.query)) {
    //   // operation.params.query = App.cboBatNbr.getValue();
    //}

};

var stoHandle_Load = function () {
    App.cboHandle.setValue('N');
};

var store_Load = function () {
    HQ.numSource++;
    checkSetDefault();
};

var stoSetup_Load = function () {
    if (App.stoSetup.data.items.length == 0) {
        HQ.objSetup = Ext.create('App.mdlSetup').data;
    } else {
        HQ.objSetup = App.stoSetup.data.items[0].data;
    }
    HQ.numSource++;
    checkSetDefault();
};

var stoUserDefault_Load = function () {
    if (App.stoUserDefault.data.items.length == 0) {
        HQ.objUser = Ext.create('App.mdlUserDefault').data;
    } else {
        HQ.objUser = App.stoUserDefault.data.items[0].data;
    }
    HQ.numSource++;
    checkSetDefault();
};

var stoTrans_BeforeLoad = function () {
    HQ.common.showBusy(true, 'Process...');
};
///////////////////
////////////cbo//////////////


var cboBatNbr_Change = function (item, newValue, oldValue) {
    var record = App.stoBatch.getById(newValue);
    if (record) {
        HQ.isNew = false;
        bindBatch(record);
    } else {
        if (HQ.recentRecord != record) {

        }
        else {
        }
    }
    HQ.recentRecord = record;

};
////// data/////

var bindBatch = function (record) {
    HQ.objBatch = record;
    App.cboBatNbr.events['change'].suspend();
    App.cboSiteID.events['change'].suspend();
    // App.cboFromToSiteID.events['change'].suspend();
    App.frmMain.events['fieldchange'].suspend();
    App.cboStatus.forceSelection = false;
    App.frmMain.loadRecord(HQ.objBatch);
    App.cboStatus.forceSelection = false;
    App.frmMain.events['fieldchange'].resume();
    App.cboBatNbr.events['change'].resume();
    App.cboSiteID.events['change'].resume();
    //App.cboFromToSiteID.events['change'].resume();
    setStatusForm();


    HQ.common.showBusy(true, HQ.waitMsg);
    HQ.numDetail = 0;

    App.stoTrans.reload();
    App.stoLotTrans.reload();

    App.cboHandle.setValue('N');

};

var bindTran = function () {
    if (App.stoTrans.data.items.length > 0) {
        var objFirst = App.stoTrans.data.items[0].data;
        App.cboSiteID.setValue(objFirst.SiteID);
        //App.cboSlsperID.setValue(objFirst.SlsperID);
    }
    App.lblQtyAvail.setText('');

    HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);

    checkTransAdd();

    calculate();
    App.grdTrans.isChange = false;
    HQ.common.showBusy(false, HQ.waitMsg);
    setChange(false);
};

var btnLot_Click = function () {
    if (Ext.isEmpty(this.record.invt)) {
        this.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [this.record.data.InvtID]);
    }

    if (!Ext.isEmpty(this.record.invt.LotSerTrack) && this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.UnitDesc)) {
        showLot(this.record, true);
    }
};
var btnLotOK_Click = function () {
    if (!App.grdLot.isLock) {
        var det = App.winLot.record.data;
        var flat = null;
        App.stoLotTrans.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                //if (item.data.Qty == 0) {
                //    HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
                //    flat = item;
                //    return false;
                //}
                //// no check qty Lot = 0
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

        App.winLot.record.data.Qty = Math.round(lineQty);
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
            }
        });
    }


    App.stoLotTrans.clearFilter();
    App.stoLotTrans.filter('INTranLineRef', record.data.LineRef);

    var newRow = Ext.create('App.mdlLotTrans');
    newRow.data.INTranLineRef = record.data.LineRef;
    newRow.data.UnitDesc = record.data.UnitDesc;
    newRow.data.ExpDate = App.txtDateEnt.getValue();
    newRow.data.UnitPrice = record.data.UnitPrice;
    newRow.data.CnvFact = record.data.CnvFact;
    newRow.data.UnitMultDiv = record.data.UnitMultDiv;
    HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);

    App.winLot.record = record;
    App.grdLot.view.refresh();
    App.winLot.setTitle(record.data.InvtID + ' ' + (record.data.UnitMultDiv == "M" ? record.data.Qty * record.data.CnvFact : record.data.Qty / record.data.CnvFact) + ' ' + record.invt.StkUnit);
    HQ.focus = '';
    App.winLot.show();
    setTimeout(function () { App.winLot.toFront(); }, 50);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////

var deleteHeader = function (item) {
    if (item == 'yes') {
        if (Ext.isEmpty(App.cboBatNbr.getValue())) {
            menuClick('new');
        } else {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'IN10400/Delete',
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
                url: 'IN10400/DeleteTrans',
                timeout: 180000,
                params: {
                    lineRef: App.smlTrans.selected.items[0].data.LineRef,
                },
                success: function (msg, data) {
                    if (!Ext.isEmpty(data.result.data.tstamp)) {
                        App.stoBatch.reload();
                    }
                    App.grdTrans.deleteSelected();
                    calculate();
                    HQ.message.process(msg, data, true);
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

var save = function () {
    if ((App.cboBatNbr.value && !HQ.isUpdate) || (Ext.isEmpty(App.cboBatNbr.value) && !HQ.isInsert)) {
        HQ.message.show(728, '', '', true);
        return;
    }
    if (App.cboStatus.getValue() != "H" && (App.cboHandle.getValue() == "N" || Ext.isEmpty(App.cboHandle.getValue()))) {
        HQ.message.show(2015020803, '', '', true);
        return;
    }

    if (App.stoTrans.data.items.length <= 1) {
        HQ.message.show(2015020804, [App.cboBatNbr.value], '', true);
        return;
    }
    if (Ext.isEmpty(App.cboReasonCD.getValue()))
    {
        HQ.message.show(15, [App.cboReasonCD.fieldLabel], '', true);
        return;
    }

    if (Ext.isEmpty(App.txtDescr.getValue())) {
        HQ.message.show(15, [App.txtDescr.fieldLabel], '', true);
        return;
    }
    if (Ext.isEmpty(App.txtDateEnt.getValue())) {
        HQ.message.show(15, [App.txtDateEnt.fieldLabel], '', true);
        return;
    }

    var flat = false;
    App.stoLotTrans.clearFilter();
    App.stoTrans.data.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
            //if (item.data.Qty == 0) {
            //    HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
            //    App.smlTrans.select(App.stoTrans.indexOf(HQ.store.findInStore(App.stoTrans, ['LineRef'], [item.data.LineRef])));
            //    flat = true;
            //    return false;
            //}
            //if (item.data.UnitMultDiv == '') {
            //    HQ.message.show(2525, [item.data.InvtID], '', true);
            //    App.smlTrans.select(App.stoTrans.indexOf(HQ.store.findInStore(App.stoTrans, ['LineRef'], [item.data.LineRef])));
            //    flat = true;
            //    return false;
            //}
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
    if (Ext.isEmpty(App.cboBatNbr.getValue()))
    {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                method: 'POST',
                url: 'IN10400/Save',
                timeout: 180000,
                params: {
                    //lstbatch: Ext.encode(App.stoBatch.getById(App.cboBatNbr.getValue()).data),
                    lstTrans: Ext.encode(App.stoTrans.getRecordsValues()),
                    lstLot: Ext.encode(App.stoLotTrans.getRecordsValues())
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
                        App.stoBatch.reload();
                    }

                    setChange(false);
                    HQ.message.process(msg, data, true);


                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
    else {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                method: 'POST',
                url: 'IN10400/Save',
                timeout: 180000,
                params: {
                    lstbatch: Ext.encode(App.stoBatch.getRecordsValues()),
                    lstTrans: Ext.encode(App.stoTrans.getRecordsValues()),
                    lstLot: Ext.encode(App.stoLotTrans.getRecordsValues())
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
                        App.stoBatch.reload();
                    }

                    setChange(false);
                    HQ.message.process(msg, data, true);
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
    
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ///////////////////////////////////////////////////

var checkSourceDetail = function (records, options, success) {
    HQ.numDetail++;
    if (HQ.numDetail == HQ.maxDetail) {
        bindTran();
    }
};

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
};

var checkSetDefault = function () {
    if (HQ.numSource == HQ.maxSource) {
        //App.cboReasonCD.getStore().reload();
        defaultOnNew();
    }
};

var defaultOnNew = function () {
    HQ.isNew = true;
    var record = Ext.create('App.mdlBatch');
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'H';
    record.data.DateEnt = HQ.businessDate;
    App.cboSiteID.setValue(HQ.inSite);
    //App.cboSlsperID.setValue('');
    // App.cboReasonCD.getStore().reload();
    App.frmMain.validate();

    bindBatch(record);
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
    }

    HQ.common.lockItem(App.frmMain, lock);
    App.grdTrans.isLock = lock;
    App.cboBatNbr.setReadOnly(false);
    //App.cboHandle.setReadOnly(false);
    App.cboStatus.setReadOnly(true);
    App.txtTotQty.setReadOnly(true);
    App.txtTotAmt.setReadOnly(true);
    App.cboSiteID.setReadOnly(true);
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
    //if (App.cboStatus.getValue() != 'H')
    //{
    //    App.cboHandle.setReadOnly(true);
    //}
    //else {
    //    App.cboHandle.setReadOnly(false);
    //}

    ////App.txtRvdBatNbr.setReadOnly(true);
    //App.cboStatus.setReadOnly(true);
    ////App.txtRefNbr.setReadOnly(true);
    //App.txtTotQty.setReadOnly(true);
    //App.txtTotAmt.setReadOnly(true);
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
    App.cboSiteID.setReadOnly(flat);

    //var flat = false;
    //App.stoTrans.data.each(function (item) {
    //    if (!Ext.isEmpty(item.data.InvtID)) {
    //        flat = true;
    //        return false;
    //    }
    //});

    //App.cboSiteID.setReadOnly(flat);

    //App.cboSlsperID.setReadOnly(App.cboStatus.getValue() != 'H');

    // App.cboFromToSiteID.setReadOnly(App.cboStatus.getValue() != 'H');
};

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
            HQ.common.showBusy(false);
            // App.grdTrans.view.loadMask.setDisabled(false)
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
        trans.TranAmt = +(trans.Qty * trans.UnitPrice).toFixed(HQ.decAmt);

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
            // App.grdTrans.view.loadMask.hide();
            HQ.common.showBusy(false);
            //App.grdTrans.view.loadMask.setDisabled(false)
            return;
        }

        trans.CnvFact = cnv.CnvFact;
        trans.UnitMultDiv = cnv.MultDiv;
        if (invt.ValMthd == "A" || invt.ValMthd == "E") {
            trans.UnitPrice = Math.round((trans.UnitMultDiv == "M" ? site.AvgCost * trans.CnvFact : site.AvgCost / trans.CnvFact));
        }
        trans.TranAmt = +(trans.Qty * trans.UnitPrice).toFixed(HQ.decAmt);
        getQtyAvail(row.record);

        calcLot(row.record, false);



    } else if (key == "Qty") {

        var invt = row.record.invt;
        var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [trans.InvtID, trans.SiteID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
            trans.Qty = 0;
            row.record.commit();
            HQ.message.show(2016042101, [trans.InvtID, trans.SiteID], '', true);
            //App.grdTrans.view.loadMask.hide();
            HQ.common.showBusy(false);
            //App.grdTrans.view.loadMask.setDisabled(false)
            return;
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
            if (editQty < 0) {
                editQty = Math.abs(editQty)
                if (!HQ.objSetup.NegQty && trans.TranType != "RI") {
                    totQty = editQty + calculateInvtTotal(trans.InvtID, trans.SiteID, trans.LineRef);
                    if (totQty > site.QtyAvail) {
                        trans.Qty = 0;
                        row.record.commit();
                        HQ.message.show(35, '', '', true);
                        //App.grdTrans.view.loadMask.hide();
                        HQ.common.showBusy(false);
                        // App.grdTrans.view.loadMask.setDisabled(false)
                        return;
                    }
                }

            }

        }

        trans.TranAmt = +(trans.Qty * trans.UnitPrice).toFixed(HQ.decAmt);

        getQtyAvail(row.record);

        calcLot(row.record, true);
    }
    else if (key == 'TranAmt') {

        var invt = row.record.invt;
        var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [trans.InvtID, trans.SiteID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
            trans.TranAmt = 0;
            row.record.commit();
            HQ.message.show(2016042101, [trans.InvtID, trans.SiteID], '', true);
            //App.grdTrans.view.loadMask.hide();
            HQ.common.showBusy(false);
            // App.grdTrans.view.loadMask.setDisabled(false)
            return;
        }
        if (invt.StkItem == 1) {
            var totAmt = trans.TranAmt
            if (totAmt < 0) {
                totAmt = Math.abs(totAmt);
                if (totAmt > site.TotCost) {
                    trans.TranAmt = 0;
                    row.record.commit();
                    HQ.message.show(607, [trans.InvtID, trans.SiteID], '', true);
                    // App.grdTrans.view.loadMask.hide();
                    HQ.common.showBusy(false);
                    //App.grdTrans.view.loadMask.setDisabled(false)
                }
            }

        }
    }

    trans.ExtCost = trans.TranAmt;
    trans.UnitCost = trans.UnitPrice;
    row.record.commit();

    if (key == 'InvtID' && !Ext.isEmpty(trans.InvtID)) {

        HQ.store.insertRecord(App.stoTrans, key, Ext.create('App.mdlTrans'), true);
    }

    calculate();

    checkTransAdd();

    //App.grdTrans.view.loadMask.hide();
    HQ.common.showBusy(false);
    //  App.grdTrans.view.loadMask.setDisabled(false)

    handleTab(key);

};

var checkSelect = function (records, options, success) {
    HQ.numSelectTrans++;
    if (HQ.numSelectTrans == HQ.maxSelectTrans) {
        //App.grdTrans.view.loadMask.hide();
        HQ.common.showBusy(false);
        //App.grdTrans.view.loadMask.setDisabled(false)
        getQtyAvail(options.row);
    }
};

var checkSourceEdit = function (records, options, success) {
    HQ.numEditTrans++;
    if (HQ.numEditTrans == HQ.maxEditTrans) {
        checkExitEdit(options.row);
    }
};

var checkSourceEditLot = function (records, options, success) {
    HQ.numLot++;
    if (HQ.numLot == HQ.maxLot) {
        checkExitEditLot(options.row);
    }
};

var checkSelectLot = function (records, options, success) {
    HQ.numSelectLot++;
    if (HQ.numSelectLot == HQ.maxSelectLot) {
        App.grdLot.view.loadMask.hide();
        App.grdLot.view.loadMask.setDisabled(false)
        getLotQtyAvail(options.row);
    }
};

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
            App.grdLot.view.loadMask.hide();
            App.grdLot.view.loadMask.setDisabled(false)
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
            App.grdLot.view.loadMask.hide();
            App.grdLot.view.loadMask.setDisabled(false)
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
        }

        if (!Ext.isEmpty(lot.LotSerNbr)) {
            var newRow = Ext.create('App.mdlLotTrans');
            newRow.data.INTranLineRef = lot.INTranLineRef;
            HQ.store.insertRecord(App.stoLotTrans, key, newRow, true);
        }
        getLotQtyAvail(record);
    }
    record.commit();
    App.grdLot.view.loadMask.hide();
    App.grdLot.view.loadMask.setDisabled(false)
};

var setChange = function (isChange) {
    HQ.isChange = isChange;
    if (isChange) {
        App.cboBatNbr.setReadOnly(true);
    } else {
        App.grdTrans.isChange = false;
        App.cboBatNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'IN10400');
};

var calculate = function () {
    var totAmt = 0;
    var totQty = 0;

    App.stoTrans.data.each(function (item) {
        totAmt += item.data.TranAmt;
        totQty += item.data.Qty;
    });
    App.txtTotAmt.setValue(totAmt);
    App.txtTotQty.setValue(totQty);

};

var calcLot = function (record, show) {
    if (!Ext.isEmpty(record.invt) && !Ext.isEmpty(record.invt.LotSerTrack) && record.invt.LotSerTrack != 'N' && !Ext.isEmpty(record.data.UnitDesc)) {
        App.stoLotTrans.clearFilter();
        App.stoLotTrans.data.each(function (item) {
            if (item.data.INTranLineRef == record.data.LineRef) {
                item.data.UnitDesc = record.data.UnitDesc;
                item.data.CnvFact = record.data.CnvFact;
                item.data.UnitMultDiv = record.data.UnitMultDiv;
                item.data.UnitCost = item.data.UnitPrice = record.data.UnitPrice;
                item.commit();
            }
        });
        if (show == true) {
            showLot(record, true);
        }

    }
    //showLot(record, true);
    //if (!Ext.isEmpty(record.invt) && !Ext.isEmpty(record.invt.LotSerTrack) && record.invt.LotSerTrack != 'N' && !Ext.isEmpty(record.data.UnitDesc)) {
    //    var flat = false;
    //    var det = record.data;

    //    App.stoLotTrans.clearFilter();
    //    App.stoLotTrans.data.each(function (item) {
    //        if (item.data.INTranLineRef == det.LineRef && !Ext.isEmpty(item.data.LotSerNbr)) {
    //            flat = true;
    //        }
    //    });

    //    if (!flat) {
    //        HQ.common.showBusy(true, HQ.waitMsg);
    //        App.stoCalcLot.load({
    //            params: {
    //                siteID: det.SiteID,
    //                invtID: det.InvtID,
    //                branchID: App.txtBranchID.getValue(),
    //                batNbr: App.cboBatNbr.getValue()
    //            },
    //            det: record.data,
    //            row: record,
    //            callback: function (records, options, success) {

    //                var det = options.det;
    //                var record = options.row;
    //                var needQty = Math.round(det.UnitMultDiv == "M" ? det.Qty * det.CnvFact : det.Qty / det.CnvFact);

    //                App.stoLotTrans.clearFilter();
    //                App.stoCalcLot.data.each(function (item) {
    //                    var newQty = 0;
    //                    var curQty = 0;

    //                    App.stoLotTrans.data.each(function (item2) {
    //                        if (item2.data.LotSerNbr == item.data.LotSerNbr && item2.data.InvtID == item.data.InvtID && item2.data.SiteID == item.data.SiteID) {
    //                            curQty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
    //                        }
    //                    });

    //                    if (Math.round(item.data.QtyAvail - curQty) == 0) return true;

    //                    if ((item.data.QtyAvail - curQty) >= needQty) {
    //                        newQty = needQty;
    //                        needQty = 0;
    //                    }
    //                    else {
    //                        newQty = (item.data.QtyAvail - curQty);
    //                        needQty -= (item.data.QtyAvail - curQty);
    //                        item.data.QtyAvail = 0;
    //                    }

    //                    if (newQty != 0) {
    //                        var newLot = Ext.create('App.mdlLotTrans');
    //                        newLot.data.BranchID = App.txtBranchID.getValue();
    //                        newLot.data.BatNbr = App.cboBatNbr.getValue();
    //                        newLot.data.LotSerNbr = item.data.LotSerNbr;
    //                        newLot.data.ExpDate = item.data.ExpDate;

    //                        newLot.data.INTranLineRef = det.LineRef;
    //                        newLot.data.SiteID = det.SiteID;
    //                        newLot.data.InvtID = det.InvtID;
    //                        newLot.data.InvtMult = -1;
    //                        if ((det.UnitMultDiv == "M" ? newQty / det.CnvFact : newQty * det.CnvFact) % 1 > 0) {
    //                            newLot.data.CnvFact = 1;
    //                            newLot.data.UnitMultDiv = 'M';
    //                            newLot.data.Qty = newQty;
    //                            newLot.data.UnitDesc = options.row.invt.StkUnit;

    //                            if (record.invt.ValMthd == "A" || record.invt.ValMthd == "E") {
    //                                newLot.data.UnitPrice = newLot.data.UnitCost = Math.round(det.UnitMultDiv == "M" ? det.UnitPrice / det.CnvFact : det.UnitPrice * det.CnvFact);
    //                            } else {
    //                                newLot.data.UnitPrice = newLot.data.UnitCost = 0;
    //                            }
    //                        } else {
    //                            newLot.data.Qty = Math.round(det.UnitMultDiv == "M" ? newQty / det.CnvFact : newQty * det.CnvFact);
    //                            newLot.data.CnvFact = det.CnvFact;
    //                            newLot.data.UnitMultDiv = det.UnitMultDiv;
    //                            newLot.data.UnitPrice = det.UnitPrice;
    //                            newLot.data.UnitCost = det.UnitPrice;
    //                            newLot.data.UnitDesc = det.UnitDesc;
    //                        }

    //                        newLot.commit();
    //                        App.stoLotTrans.insert(App.stoLotTrans.getCount(), newLot);
    //                    }



    //                    if (needQty == 0) return false;
    //                });
    //                App.stoLotTrans.commitChanges();
    //                HQ.common.showBusy(false);
    //                showLot(options.row, false);
    //            }
    //        });
    //    } else {
    //        showLot(record, true);
    //    }
    //}
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

var getQtyAvail = function (row) {

    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [row.data.InvtID, row.data.SiteID]);
    if (!Ext.isEmpty(site)) {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + (site.QtyAvail + calculateInvtTotal(row.data.InvtID, row.data.SiteID, "")));
    }
    else {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + (0 - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "")));
    }
};

var calculateInvtTotal = function (invtID, siteID, lineRef) {
    var qty = 0;
    var qtyOld = 0;
    App.stoTrans.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID && (lineRef == "" || (lineRef != "" && lineRef != item.data.LineRef))) {
            qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
        }
    });
    App.stoOldTrans.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID) {
            qtyOld += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
        }
    });
    if (qty >= 0)
        return 0;
    return qty - qtyOld;


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
        if (qty <= 0) {
            qty = Math.abs(qty);
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
                if (qty < 0) {
                    qty = Math.abs(qty);
                }
                qtyAvail = lot.QtyAvail - qty;


            }
        }
        else {
            qtyAvail = lot.QtyAvail;
        }
    }
    else {
        if (qty <= 0) {
            qty = Math.abs(qty)
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
        else {
            qtyAvail = 0;
        }
    }
    App.lblLotQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + qtyAvail);

};

var renderRowNumber = function (value, meta, record) {
    return App.stoLotTrans.data.indexOf(record) + 1;
};

var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
};

var deleteLot = function (item) {
    if (item == 'yes') {
        App.grdLot.deleteSelected();
    }
};
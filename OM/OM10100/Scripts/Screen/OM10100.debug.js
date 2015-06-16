HQ.recentRecord = null;
HQ.focus = 'order';
HQ.objOrder = null;
HQ.objCust = null;
HQ.objType = null;
HQ.objIN = null;
HQ.objOM = null;
HQ.objUser = null;
HQ.false = false;
//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var stoHandle_Load = function () {
    App.cboHandle.setValue('N');
}
var stoData_Load = function () {
    HQ.numSource++;
    checkSetDefault();
}
var stoOrder_Load = function () {
    var record = App.stoOrder.getById(App.cboOrderNbr.getValue());
    if (record) {
        bindOrder(record);
    }
}
var stoDetail_Load = function () {
    checkSourceDetail();
}
var stoOrdDet_BeforeLoad = function () {
    App.grdOrdDet.view.loadMask.disable();
}
var stoTaxTrans_BeforeLoad = function () {
    if (!Ext.isEmpty(App.grdTaxTrans.view.loadMask.disable)) {
        App.grdTaxTrans.view.loadMask.disable();
    }
}
var stoOrdDisc_BeforeLoad = function () {
    if (!Ext.isEmpty(App.grdDisc.view.loadMask.disable)) {
        App.grdDisc.view.loadMask.disable();
    }
}
var stoUserDefault_Load = function () {
    if (App.stoUserDefault.data.items.length == 0) {
        if (HQ.false == false) {
            HQ.false = true;
            HQ.message.show(8006, '', '', true);
            App.frmMain.setDisabled(true);
            HQ.common.showBusy(false);
        }
    } else {
        HQ.objUser = App.stoUserDefault.data.items[0].data;
        HQ.numSource++;
        checkSetDefault();
    }
}
var stoINSetup_Load = function () {
    if (App.stoINSetup.data.items.length == 0) {
        if (HQ.false == false) {
            HQ.false = true;
            HQ.message.show(8006, '', '', true);
            App.frmMain.setDisabled(true);
            HQ.common.showBusy(false);
        }
    } else {
        HQ.objIN = App.stoINSetup.data.items[0].data;
        HQ.numSource++;
        checkSetDefault();
    }
}
var stoOMSetup_Load = function () {
    if (App.stoOMSetup.data.items.length == 0) {
        if (HQ.false == false) {
            HQ.false = true;
            HQ.message.show(8006, '', '', true);
            App.frmMain.setDisabled(true);
            HQ.common.showBusy(false);
        }
    } else {
        HQ.objOM = App.stoOMSetup.data.items[0].data;
        HQ.numSource++;
        checkSetDefault();
    }
}

var loadCust = function (custID, orderDate, shipToID, isSelect) {
    if (isSelect) {
        HQ.numCust = 0;
        HQ.maxCust = 4;

        App.stoPrice.load({
            params: { custID: custID, orderDate: orderDate, branchID: App.txtBranchID.getValue() }, callback: checkSelectCust
        });

        App.stoCustomer.load({
            params: { custID: custID, branchID: App.txtBranchID.getValue() }, callback: checkSelectCust
        });

        App.stoShipToID.load({
            params: { custID: custID, branchID: App.txtBranchID.getValue() }, callback: checkSelectCust
        });

        App.stoSOAddress.load({
            params: { custID: custID, branchID: App.txtBranchID.getValue(), shipToID: shipToID }, callback: checkSelectCust
        });
    } else {
        HQ.numCust = 0;
        HQ.maxCust = 3;

        App.stoPrice.load({
            params: { custID: custID, orderDate: orderDate, branchID: App.txtBranchID.getValue() }, callback: checkSelectCust
        });

        App.stoCustomer.load({
            params: { custID: custID, branchID: App.txtBranchID.getValue() }, callback: checkSelectCust
        });

        App.stoShipToID.load({
            params: { custID: custID, branchID: App.txtBranchID.getValue() }, callback: checkSelectCust
        });
    }

}
var checkSelect = function (records, options, success) {
    HQ.numSelectDet++;
    if (HQ.numSelectDet == HQ.maxSelectDet) {
        App.grdOrdDet.view.loadMask.hide();
        App.grdOrdDet.view.loadMask.setDisabled(false)
        getQtyAvail(options.row);
    }
}
var checkSelectLot = function (records, options, success) {
    HQ.numSelectLot++;
    if (HQ.numSelectLot == HQ.maxSelectLot) {
        App.grdLot.view.loadMask.hide();
        App.grdLot.view.loadMask.setDisabled(false)
        getLotQtyAvail(options.row);
    }
}
var checkSetDefault = function () {
    if (HQ.numSource == HQ.maxSource) {
        defaultOnNew();
    }
}
var checkSourceEdit = function (records, options, success) {
    HQ.numDet++;
    if (HQ.numDet == HQ.maxDet) {
        checkExitEdit(options.row);
    }
}
var checkSourceEditLot = function (records, options, success) {
    HQ.numLot++;
    if (HQ.numLot == HQ.maxLot) {
        checkExitEditLot(options.row);
    }
}
var checkSourceDetail = function (records, options, success) {
    HQ.numDetail++;
    if (HQ.numDetail == HQ.maxDetail) {
        bindDetail();
    }
}
var checkSelectCust = function (records, options, success) {
    HQ.numCust++;
    if (HQ.numCust == HQ.maxCust) {
        bindCust();
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
    HQ.maxSource = 16;

    HQ.numDetail = 0;
    HQ.maxDetail = 5;

    App.cboOrderNbr.key = true;
    App.txtBranchID.setValue(HQ.cpnyID);

    App.cboStatus.getStore().addListener('load', stoData_Load);
    App.cboCustID.getStore().addListener('load', stoData_Load);
    App.cboSlsPerID.getStore().addListener('load', stoData_Load);
    App.cboDeliveryID.getStore().addListener('load', stoData_Load);

    App.cboDetBOType.getStore().addListener('load', stoData_Load);
    App.cboDetSiteID.getStore().addListener('load', stoData_Load);
    App.cboDetBudgetID.getStore().addListener('load', stoData_Load);
    App.cboDetTaxCat.getStore().addListener('load', stoData_Load);

    App.cboOrderType.getStore().addListener('load', function () {
        Ext.each(App.cboOrderType.getStore().getRange(), function (record) {
            var newRecordData = Ext.clone(record.copy().data);
            var model = new App.stoBKOrderType.model(newRecordData, newRecordData.id);
            App.stoBKOrderType.add(model);
        });
        HQ.numSource++;
        checkSetDefault();
    });

    App.cboReasonCode.getStore().addListener('load', function () {
        Ext.each(App.cboReasonCode.getStore().getRange(), function (record) {
            var newRecordData = Ext.clone(record.copy().data);
            var model = new App.stoBKReasonCode.model(newRecordData, newRecordData.id);
            App.stoBKReasonCode.add(model);
        });
        HQ.numSource++;
        checkSetDefault();
    });

    App.stoUnitConversion.addListener('load', function () {
        Ext.each(App.stoUnitConversion.getRange(), function (record) {
            var newRecordData = Ext.clone(record.copy().data);
            var model = new App.stoUnitConversion.model(newRecordData, newRecordData.id);
            App.stoUnit.add(model);
        });
        HQ.numSource++;
        checkSetDefault();
    });

    App.stoInvt.addListener('load', stoData_Load);
    App.stoTax.addListener('load', stoData_Load);

    App.stoUserDefault.addListener('load', stoUserDefault_Load);
    App.stoINSetup.addListener('load', stoINSetup_Load);
    App.stoOMSetup.addListener('load', stoOMSetup_Load);

    App.cboHandle.getStore().addListener('load', stoHandle_Load);

    App.stoOrderType = App.cboOrderType.getStore();
    App.stoBOType = App.cboDetBOType.getStore();
    App.stoDisc = App.cboDetDiscCode.getStore();
    App.stoBudget = App.cboDetBudgetID.getStore();

    App.stoINSetup.load();
    App.stoOMSetup.load();
    App.stoUserDefault.load();
    App.stoUnitConversion.load();
    App.stoInvt.load();
    App.stoTax.load();

    App.cboBKReasonCode.lastQuery = '';
    App.cboBKOrderType.lastQuery = '';
    App.cboDetTaxID.lastQuery = '';
    App.cboDetUnitDesc.lastQuery = '';
    App.cboLotUnitDesc.lastQuery = '';

    HQ.common.showBusy(true, HQ.waitMsg);
}
var frmMain_FieldChange = function (item, field, newValue, oldValue) {
    if (field.key != undefined || !App.cboDetInvtID.submitValue) {
        return;
    }
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (Object.keys(App.stoOrder.getChangedData()).length > 0 || App.grdOrdDet.isChange) {
        setChange(true);
    } else {
        setChange(false);
    }

}
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'order') {
                if (HQ.isChange || App.grdOrdDet.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    App.frmMain.loadRecord(App.stoOrder.first());
                }
            } else if (HQ.focus == 'det') {
                HQ.grid.first(App.grdOrdDet);
            }
            break;
        case "next":
            if (HQ.focus == 'order') {
                if (HQ.isChange || App.grdOrdDet.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    var index = App.stoOrder.indexOf(App.stoOrder.getById(App.cboOrderNbr.getValue()));
                    App.cboOrderNbr.setValue(App.stoOrder.getAt(index + 1).get('OrderNbr'));
                }
            } else if (HQ.focus == 'det') {
                HQ.grid.next(App.grdOrdDet);
            }
            break;
        case "prev":
            if (HQ.focus == 'order') {
                if (HQ.isChange || App.grdOrdDet.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    var index = App.stoOrder.indexOf(App.stoOrder.getById(App.cboOrderNbr.getValue()));
                    App.cboOrderNbr.setValue(App.stoOrder.getAt(index - 1).get('OrderNbr'));
                }
            } else if (HQ.focus == 'det') {
                HQ.grid.prev(App.grdOrdDet);
            }
            break;
        case "last":
            if (HQ.focus == 'order') {
                if (HQ.isChange || App.grdOrdDet.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    App.frmMain.loadRecord(App.stoOrder.last());
                }
            } else if (HQ.focus == 'det') {
                HQ.grid.last(App.grdOrdDet);
            }
            break;
        case "save":
            if (App.grdOrdDet.view.loadMask.isVisible()) {
                return;
            }
            save();
            break;
        case "delete":
            if (HQ.focus == 'order') {
                if (App.cboOrderNbr.value) {
                    if (HQ.isDelete) {
                        if (App.cboStatus.getValue() != 'N') {
                            HQ.message.show(2015020805, [App.cboOrderNbr.value], '', true);
                        } else {
                            HQ.message.show(11, '', 'deleteHeader');
                        }
                    } else {
                        HQ.message.show(728, '', '', true);
                    }
                } else {
                    menuClick('new');
                }
            } else if (HQ.focus == 'det') {
                if ((App.cboOrderNbr.value && HQ.isUpdate) || (!App.cboOrderNbr.value && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != "N") {
                        HQ.message.show(2015020805, [App.cboOrderNbr.value], '', true);
                        return;
                    }
                    if (App.smlOrdDet.selected.items.length != 0) {
                        if (!Ext.isEmpty(App.smlOrdDet.selected.items[0].data.InvtID) && !App.smlOrdDet.selected.items[0].data.FreeItem) {
                            HQ.message.show(2015020806, [App.smlOrdDet.selected.items[0].data.InvtID], 'deleteDet', true);
                        }
                    }
                }
            }
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (HQ.isChange || App.grdOrdDet.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
        case "new":
            if ((HQ.isChange || App.grdOrdDet.isChange) && !Ext.isEmpty(App.cboOrderNbr.getValue())) {
                HQ.message.show(2015030201, '', "askNew", true);
            } else {
                defaultOnNew();
            }
            break;
        case "refresh":
            if (!Ext.isEmpty(App.cboOrderNbr.getValue())) {
                App.stoOrder.reload();
            } else {
                defaultOnNew();
            }

            break;
        case "print":
            if (!Ext.isEmpty(App.cboOrderNbr.getValue()) && App.cboStatus.value != "N") {
                App.winReport.show();
            }
            break;
        default:
    }
}

var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'OM10100?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
}
var btnLot_Click = function () {
    if (Ext.isEmpty(this.record.invt)) {
        this.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [this.record.data.InvtID]);
    }

    if (!Ext.isEmpty(this.record.invt.LotSerTrack) && this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.SlsUnit)) {
        showLot(this.record, true);
    }
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
                if (item.data.SiteID == det.SiteID && item.data.InvtID == det.InvtID && item.data.OMLineRef == det.LineRef) {
                    qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                }
            }

        });

        var lineQty = (det.UnitMultDiv == "M" ? qty / det.UnitRate : det.LineQty * det.UnitRate)
        if (lineQty % 1 > 0) {
            App.winLot.record.data.LineQty = qty;
            App.winLot.record.data.SlsUnit = App.winLot.record.invt.StkUnit;
            App.winLot.record.data.UnitRate = 1;
            App.winLot.record.data.UnitMultDiv = "M";
            if (HQ.objOM.DfltSalesPrice == "I") {
                price = Math.round(unitMultDiv == "M" ? App.winLot.record.invt.SOPrice * cnvFact : App.winLot.record.invt.SOPrice / cnvFact);
                App.winLot.record.data.SlsPrice = price;
            } else {
                var price = HQ.store.findInStore(App.stoPrice, ['InvtID', 'Unit'], [App.winLot.record.data.InvtID, App.winLot.record.data.SlsUnit]);
                if (!Ext.isEmpty(price)) price = price.Price;
                else price = 0;
                App.winLot.record.data.SlsPrice = price;
            }
        } else {
            App.winLot.record.data.LineQty = Math.round(lineQty);
        }

        App.winLot.record.commit();

        App.grdOrdDet.view.refresh();

        checkSubDisc(App.winLot.record);
        checkTaxInGrid("LineQty", App.winLot.record);

        calcDet();

        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr)) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
    }


    App.winLot.hide();
}
var btnLotDel_Click = function () {
    if ((App.cboOrderNbr.value && HQ.isUpdate) || (!App.cboOrderNbr.value && HQ.isInsert)) {
        if (App.cboStatus.getValue() != "N") {
            HQ.message.show(2015020805, [App.cboOrderNbr.value], '', true);
            return;
        }
        if (App.smlLot.selected.items.length != 0) {
            if (!Ext.isEmpty(App.smlLot.selected.items[0].data.LotSerNbr)) {
                HQ.message.show(2015020806, [App.smlLot.selected.items[0].data.InvtID + ' ' + App.smlLot.selected.items[0].data.LotSerNbr], 'deleteLot', true);
            }
        }
    }
}
var btnBackOrder_Click = function () {
    if ((HQ.objType.ARDocType == "CM" || HQ.objType.ARDocType == "CC" || HQ.objType.INDocType == "CM") && !Ext.isEmpty(App.cboCustID.getValue()) && Ext.isEmpty(App.cboOrderNbr.getValue())) {
        App.cboBKOrderType.setValue('');
        App.cboBKOrderNbr.setValue('');
        App.cboBKReasonCode.setValue('');
        App.cboBKOrderNbr.getStore().clearData();

        App.winBackOrder.show();
    }
}
var btnShowReport_Click = function () {

    App.frmMain.submit({
        waitMsg: HQ.waitMsg,
        method: 'POST',
        url: 'OM10100/Report',
        timeout: 180000,
        params: {
            type: App.cboReport.value
        },
        success: function (msg, data) {
            if (this.result.reportID != null) {
                window.open('Report?ReportName=' + this.result.reportName + '&_RPTID=' + this.result.reportID, '_blank');
            }
            App.winReport.close();
            HQ.message.process(msg, data, true);
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });

}
var btnBKOk_Click = function () {
    if (Ext.isEmpty(App.cboBKOrderNbr.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('OrderNbr')], "", true);
        return;
    }
    if (Ext.isEmpty(App.cboBKReasonCode.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('ReasonCD')], "", true);
        return;
    }
    App.cboReasonCode.setValue(App.cboBKReasonCode.getValue());

    HQ.common.showBusy(true, HQ.waitMsg);
    App.stoOldOrdDet.load({
        params: { orderNbr: App.cboBKOrderNbr.getValue(), branchID: App.txtBranchID.getValue() },
        callback: function () {
            App.stoOldLotTrans.load({
                params: { orderNbr: App.cboBKOrderNbr.getValue(), branchID: App.txtBranchID.getValue() },
                callback: function () {
                    App.stoOrdDet.clearData();
                    App.stoTaxTrans.clearData();
                    App.stoOrdDisc.clearData();
                    App.stoTaxDoc.clearData();

                    App.stoOldOrdDet.data.each(function (item) {
                        var newDet = Ext.create('App.mdlOrdDet');
                        newDet.data.BranchID = item.data.BranchID;
                        newDet.data.LineRef = item.data.LineRef;
                        newDet.data.Descr = item.data.Descr;
                        newDet.data.DiscPct = item.data.DiscPct;
                        newDet.data.TaxCat = item.data.TaxCat;
                        newDet.data.TaxID = item.data.TaxID;
                        newDet.data.BOCustID = item.data.BOCustID;
                        newDet.data.BOType = item.data.BOType;
                        newDet.data.BarCode = item.data.BarCode;
                        newDet.data.BudgetID1 = item.data.BudgetID1;
                        newDet.data.BudgetID2 = item.data.BudgetID2;
                        newDet.data.CostID = item.data.CostID;
                        newDet.data.DiscAmt = item.data.DiscAmt;
                        newDet.data.DiscAmt1 = item.data.DiscAmt1;
                        newDet.data.DiscAmt2 = item.data.DiscAmt2;
                        newDet.data.DiscCode = item.data.DiscCode;
                        newDet.data.DiscID1 = item.data.DiscID1;
                        newDet.data.DiscID2 = item.data.DiscID2;
                        newDet.data.DiscPct1 = item.data.DiscPct1;
                        newDet.data.DiscPct2 = item.data.DiscPct2;
                        newDet.data.DiscSeq1 = item.data.DiscSeq1;
                        newDet.data.DiscSeq2 = item.data.DiscSeq2;
                        newDet.data.DocDiscAmt = item.data.DocDiscAmt;
                        newDet.data.FreeItem = item.data.FreeItem;
                        newDet.data.FreeItemQty1 = item.data.FreeItemQty1;
                        newDet.data.FreeItemQty2 = item.data.FreeItemQty2;
                        newDet.data.GroupDiscAmt1 = item.data.GroupDiscAmt1;
                        newDet.data.GroupDiscAmt2 = item.data.GroupDiscAmt2;
                        newDet.data.GroupDiscID1 = item.data.GroupDiscID1;
                        newDet.data.GroupDiscID2 = item.data.GroupDiscID2;
                        newDet.data.GroupDiscPct1 = item.data.GroupDiscPct1;
                        newDet.data.GroupDiscPct2 = item.data.GroupDiscPct2;
                        newDet.data.GroupDiscSeq1 = item.data.GroupDiscSeq1;
                        newDet.data.GroupDiscSeq2 = item.data.GroupDiscSeq2;
                        newDet.data.InvtID = item.data.InvtID;
                        newDet.data.ItemPriceClass = item.data.ItemPriceClass;
                        newDet.data.LineAmt = item.data.LineAmt;
                        newDet.data.LineQty = item.data.LineQty;
                        newDet.data.ManuDiscAmt = item.data.ManuDiscAmt;
                        newDet.data.OrderType = App.cboOrderType.getValue();
                        newDet.data.OrigOrderNbr = item.data.OrigOrderNbr;
                        newDet.data.QtyBO = item.data.QtyBO;
                        newDet.data.QtyInvc = item.data.QtyInvc;
                        newDet.data.QtyOpenShip = item.data.QtyOpenShip;
                        newDet.data.QtyShip = item.data.QtyShip;
                        newDet.data.SOFee = item.data.SOFee;
                        newDet.data.ShipStatus = item.data.ShipStatus;
                        newDet.data.SiteID = item.data.SiteID;
                        newDet.data.SlsPrice = item.data.SlsPrice;
                        newDet.data.SlsUnit = item.data.SlsUnit;
                        newDet.data.StkQty = item.data.StkQty;
                        newDet.data.TaxAmt00 = item.data.TaxAmt00;
                        newDet.data.TaxAmt01 = item.data.TaxAmt01;
                        newDet.data.TaxAmt02 = item.data.TaxAmt02;
                        newDet.data.TaxAmt03 = item.data.TaxAmt03;
                        newDet.data.TaxId00 = item.data.TaxId00;
                        newDet.data.TaxId01 = item.data.TaxId01;
                        newDet.data.TaxId02 = item.data.TaxId02;
                        newDet.data.TaxId03 = item.data.TaxId03;
                        newDet.data.TxblAmt00 = item.data.TxblAmt00;
                        newDet.data.TxblAmt01 = item.data.TxblAmt01;
                        newDet.data.TxblAmt02 = item.data.TxblAmt02;
                        newDet.data.TxblAmt03 = item.data.TxblAmt03;
                        newDet.data.UnitMultDiv = item.data.UnitMultDiv;
                        newDet.data.UnitRate = item.data.UnitRate;
                        newDet.data.UnitWeight = item.data.UnitWeight;
                        newDet.data.DumyLineQty = item.data.DumyLineQty;
                        newDet.commit();
                        App.stoOrdDet.data.add(newDet);
                    });


                    App.stoOldLotTrans.data.each(function (item) {
                        var newLot = Ext.create('App.mdlLotTrans');
                        newLot.data.BranchID = App.txtBranchID.getValue();
                        newLot.data.LotSerNbr = item.data.LotSerNbr;
                        newLot.data.ExpDate = item.data.ExpDate;

                        newLot.data.OMLineRef = item.data.OMLineRef;
                        newLot.data.SiteID = item.data.SiteID;
                        newLot.data.InvtID = item.data.InvtID;
                        newLot.data.InvtMult = 1;
                        newLot.data.TranDate = item.data.TranDate;
                        newLot.data.WarantyDate = item.data.WarrantyDate;
                        newLot.data.CnvFact = item.data.CnvFact;
                        newLot.data.UnitMultDiv = item.data.UnitMultDiv;
                        newLot.data.Qty = item.data.Qty;
                        newLot.data.UnitDesc = item.data.UnitDesc;
                        newLot.data.UnitPrice = item.data.UnitPrice;
                        newLot.data.UnitCost = item.data.UnitCost;

                        newLot.commit();
                        App.stoLotTrans.insert(App.stoLotTrans.getCount(), newLot);
                    });

                    App.stoLotTrans.commitChanges();

                    var newRow = Ext.create('App.mdlOrdDet');
                    newRow.data.BOType = 'S';
                    HQ.store.insertRecord(App.stoOrdDet, "InvtID", newRow, true);

                    App.stoOrdDet.commitChanges();
                    App.grdOrdDet.view.refresh();

                    App.txtOrigOrderNbr.setValue(App.cboBKOrderNbr.getValue());

                    for (i = 0; i < App.stoOrdDet.data.length; i++) {
                        calcTax(App.stoOrdDet.data.items[i]);
                    }
                    calcTaxTotal();
                    calcDet();

                    var oldOrd = HQ.store.findInStore(App.cboBKOrderNbr.getStore(), ['OrderNbr'], [App.cboBKOrderNbr.getValue()]);

                    App.cboSlsPerID.setValue(oldOrd.SlsPerID);
                    App.cboDeliveryID.setValue(oldOrd.DeliveryID);

                    App.txtVolDiscPct.setValue(oldOrd.VolDiscAmt);
                    App.txtFreightAmt.setValue(oldOrd.FreightAmt);
                    App.txtOrdDiscAmt.setValue(oldOrd.OrdDiscAmt);

                    calcDet();

                    HQ.common.showBusy(false);

                }
            });
        }
    });
    App.winBackOrder.hide();


}
var cboOrderNbr_Change = function (item, newValue, oldValue) {
    var record = App.stoOrder.getById(newValue);
    if (record) {
        //showMask();
        bindOrder(record);
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
var cboOrderType_Change = function (item, newValue, oldValue) {
    if (item.valueModels.length > 0) {
        HQ.objType = item.valueModels[0].data;
    } else {
        HQ.objType = App.create('App.mdlOrderType').data;
    }
    defaultOnNew();
}
var cboStatus_Change = function (item, newValue, oldValue) {
    App.cboHandle.store.reload();
}

var cboCustID_Change = function (item, newValue, oldValue) {

    if (item.valueModels.length > 0) {
        HQ.common.showBusy(true, HQ.waitMsg);
        loadCust(App.cboCustID.getValue(), App.txtOrderDate.getValue(), item.valueModels[0].data.DfltShipToID, true);
    } else {
        App.stoCustomer.clearData();
        App.stoSOAddress.clearData();
        App.stoShipToID.loadData([], false);
        App.cboShiptoID.setValue('');
        HQ.objCust = Ext.create('App.mdlCustomer');
        bindAddress();
    }

}
var cboShiptoID_Change = function (item, newValue, oldValue) {
    if (item.valueModels.length > 0) {
        App.stoSOAddress.load({
            params: { custID: App.cboCustID.getValue(), branchID: App.txtBranchID.getValue(), shipToID: App.cboShiptoID.getValue() }, callback: function () {
                bindAddress();
            }
        });
    }
    else {
        App.stoSOAddress.clearData();
        bindAddress();
    }

}
var cboSlsPerID_Change = function (item, newValue, oldValue) {
    if (item.valueModels.length > 0) {
        App.cboDeliveryID.setValue(item.valueModels[0].data.DeliveryID);
    }
    else {
        App.cboDeliveryID.setValue('');
    }
}
var cboBKOrderType_Change = function (item, newvalue, oldValue) {
    if (!Ext.isEmpty(App.cboBKOrderType.getValue())) {
        App.cboBKOrderNbr.getStore().reload();
    }
    else {
        App.cboBKOrderNbr.getStore().clearData();
    }
}
var txtOrderDate_Change = function () {

    if (App.txtOrderDate.isValid()) {
        HQ.common.showBusy(true, HQ.waitMsg);

        App.txtARDocDate.setValue(App.txtOrderDate.getValue());
        App.txtShipDate.setValue(App.txtOrderDate.getValue());
        App.stoPrice.load({
            params: { custID: App.cboCustID.getValue(), orderDate: App.txtOrderDate.getValue(), branchID: App.txtBranchID.getValue() }, callback: function () {
                HQ.common.showBusy(false);
            }
        });
    } else {
        App.stoPrice.clearData();
    }

}
var txtFreightAmt_Change = function () {
    calcDet();
}
var txtMiscAmt_Change = function () {
    calcDet();
}
var txtVolDiscPct_Change = function () {
    if (!Ext.isEmpty(HQ.objType)) {
        updateDistPctAmt();
        calcDet();
    }
}
var txtOrdDiscAmt_Change = function () {
    App.txtVolDiscPct.events['change'].suspend();
    if ((App.txtCuryLineAmt.getValue() - App.txtVolDiscAmt.getValue()) != 0) {
        if (HQ.objOM.InlcSOFeeDisc) {
            if (App.txtVolDiscAmt.getValue() == 0) {
                if (HQ.objType.TaxFee)
                    App.txtVolDiscPct.setValue(+((App.txtOrdDiscAmt.getValue() / (App.txtCuryLineAmt.getValue() + App.txtSOFeeTot.getValue() * 1.1 - App.txtVolDiscAmt.getValue())) * 100).toFixed(2));
                else
                    App.txtVolDiscPct.setValue(+((App.txtOrdDiscAmt.getValue() / (App.txtCuryLineAmt.getValue() + App.txtSOFeeTot.getValue() - App.txtVolDiscAmt.getValue())) * 100).toFixed(2));
            } else {
                App.txtVolDiscPct.setValue(+((App.txtOrdDiscAmt.getValue() / (App.txtCuryLineAmt.getValue() - App.txtVolDiscAmt.getValue())) * 100).toFixed(2));
            }
        } else
            App.txtVolDiscPct.setValue(+((App.txtOrdDiscAmt.getValue() / (App.txtCuryLineAmt.getValue() - App.txtVolDiscAmt.getValue())) * 100).toFixed(2));
    } else {
        App.txtVolDiscPct.setValue(0);
    }
    calcDet();
    App.txtVolDiscPct.events['change'].resume();

}

var grdOrdDet_BeforeEdit = function (item, e) {
    if (App.grdOrdDet.isLock) {
        return false;
    }

    if (Ext.isEmpty(App.cboCustID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('CustID')], '', true);
        return false;
    }

    if (Ext.isEmpty(App.cboSlsPerID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('SlsperID')], '', true);
        return false;
    }

    if (!App.txtOrderDate.validate()) {
        HQ.message.show(1000, [HQ.common.getLang('OrderDate')], '', true);
        return false;
    }

    var key = e.field;
    var record = e.record;

    if (key == 'BOType' && !Ext.isEmpty(record.data.BOType)) return false;

    if (key == 'InvtID' && !Ext.isEmpty(record.data.InvtID)) return false;

    if (key != 'InvtID' && key != 'BOType' && (Ext.isEmpty(e.record.data.BOType) || Ext.isEmpty(e.record.data.InvtID))) return false;

    if (key == "FreeItem" && HQ.objType.SalesType == "PRO") return false;

    if (key == "QtyBO" && record.data.BOType != "S" && record.data.BOType != "0") return false;

    if (key == "BOCustID" && record.data.BOType != "B" && record.data.BOType != "0") return false;

    if ((record.data.FreeItem || record.data.BOType == "R") && (key == "SlsPrice" || key == "DiscAmt" || key == "DiscPct")) {
        if (key == "SlsPrice") {
            if (!HQ.objOM.EditableSlsPrice) {
                return false;
            }
        } else {
            return false;
        }
    }
    else {
        if ((key == "SlsPrice" || key == "LineAmt") && !HQ.objOM.EditableSlsPrice) return false;

        if (HQ.objType.ARDocType != "NA" && HQ.objType.ARDocType != "CM" && HQ.objType.ARDocType != "CC" && !Ext.isEmpty(record.data.DiscID1) && (key == "DiscAmt" || key == "DiscPct" || key == "DiscCode")) {
            return false;
        }
    }

    if (key == "DiscCode") {
        App.stoDisc.load({
            params: { orderNbr: App.cboOrderNbr.getValue(), orderDate: App.txtOrderDate.getValue(), branchID: App.txtBranchID.getValue() }
        });
    }
    if (key == "SlsUnit") {
        App.stoUnit.clearFilter();
        if (e.record.invt == undefined) {
            e.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]);
        }
        App.stoUnit.filterBy(function (item) {
            if (item.data.ToUnit == record.invt.StkUnit && (item.data.InvtID == "*" || item.data.InvtID == record.invt.InvtID) && (item.data.ClassID == "*" || item.data.ClassID == record.invt.ClassID)) {
                return item;
            }
        });
    }
    if (Ext.isEmpty(record.data.TaxID)) {
        record.data.TaxID = '*';
    }

    if (Ext.isEmpty(record.data.LineRef)) {
        record.data.LineRef = lastLineRef();
        record.data.BranchID = App.txtBranchID.getValue();
        record.data.OrderNbr = Ext.isEmpty(App.cboOrderNbr.getValue()) ? '' : App.cboOrderNbr.getValue();
    }

    record.commit();

    App.cboDetUnitDesc.setValue('');
}
var grdOrdDet_SelectionChange = function (item, selected) {
    HQ.focus = 'det';
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectDet = 0;
            HQ.maxSelectDet = 2;
            App.grdOrdDet.view.loadMask.show();
            App.stoItemSite.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.txtBranchID.getValue() },
                callback: checkSelect,
                row: selected[0]
            });
            App.stoOldOrdDet.load({
                params: { orderNbr: App.cboOrderNbr.getValue(), branchID: App.txtBranchID.getValue() },
                callback: checkSelect,
                row: selected[0]
            });
        } else {
            App.lblQtyAvail.setText('');
        }
    }
}
var grdOrdDet_Edit = function (item, e) {
    HQ.focus = 'det';
    var key = e.field;
    if (Object.keys(e.record.modified).length > 0) {
        App.grdOrdDet.isChange = true;
        if (e.record.invt == undefined) {
            e.record.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]);
        }
        var invt = e.record.invt;
        if (!Ext.isEmpty(invt)) {

            if ((key == 'InvtID' || key == 'BarCode') && Ext.isEmpty(e.record.data.SlsUnit)) {
                var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);
                if (!Ext.isEmpty(cnv)) {
                    e.record.data.SlsUnit = invt.StkUnit;
                    e.record.data.UnitRate = cnv.CnvFact == 0 ? 1 : cnv.CnvFact;
                    e.record.data.UnitMultDiv = cnv.MultDiv;
                } else {
                    return;
                }
            }

            if (key == 'InvtID' || key == 'BarCode' || key == 'SiteID' || key == 'SlsUnit') {
                App.grdOrdDet.view.loadMask.show();
                HQ.numDet = 0;
                HQ.maxDet = 1;
                App.stoItemSite.load({
                    params: { siteID: '', invtID: e.record.data.InvtID, branchID: App.txtBranchID.getValue() }, callback: checkSourceEdit, row: e
                });
            } else {
                checkExitEdit(e);
            }
        }
    }
}
var grdOrdDet_ValidateEdit = function (item, e) {
}

var grdLot_BeforeEdit = function (item, e) {
    if (App.grdLot.isLock) {
        return false;
    }


    var key = e.field;
    var record = e.record;
    if (key != 'LotSerNbr' && Ext.isEmpty(e.record.data.LotSerNbr)) return false;
    if (key == 'LotSerNbr' && !Ext.isEmpty(record.data.LotSerNbr)) return false;

    if (key == "UnitDesc") {
        App.stoUnit.clearFilter();
        App.stoUnit.filterBy(function (item) {
            if (item.data.ToUnit == App.winLot.record.invt.StkUnit && (item.data.InvtID == "*" || item.data.InvtID == App.winLot.record.invt.InvtID) && (item.data.ClassID == "*" || item.data.ClassID == App.winLot.record.invt.ClassID)) {
                return item;
            }
        });
    }


    if (Ext.isEmpty(record.data.InvtID)) {
        record.data.InvtID = App.winLot.record.data.InvtID;
        record.data.SiteID = App.winLot.record.data.SiteID;
    }

    record.commit();

    App.cboLotUnitDesc.setValue('');
}
var grdLot_SelectionChange = function (item, selected) {
    HQ.focus = 'lot';
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectLot = 0;
            HQ.maxSelectLot = 1;
            App.grdLot.view.loadMask.show();
            App.stoItemLot.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.txtBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, orderNbr: App.cboOrderNbr.getValue() },
                callback: checkSelectLot,
                row: selected[0]
            });
        } else {
            App.lblLotQtyAvail.setText('');
        }
    }
}
var grdLot_Edit = function (item, e) {
    HQ.focus = 'lot';
    var key = e.field;
    var lot = e.record.data;
    var record = e.record;
    if (Object.keys(e.record.modified).length > 0) {
        if (key == "Qty" || key == "UnitDesc") {
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

    HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);

    App.TrnsferNbr.setReadOnly(!Ext.isEmpty(App.cboOrderNbr.getValue()));
    checkTransAdd();
    calculate();
    App.grdOrdDet.isChange = false;
    HQ.common.showBusy(false, HQ.waitMsg);
    setChange(false);
}
var bindOrder = function (record) {
    HQ.objOrder = record;

    App.cboOrderType.events['change'].suspend();
    App.cboOrderNbr.events['change'].suspend();
    App.cboCustID.events['change'].suspend();
    App.cboSlsPerID.events['change'].suspend();
    App.txtOrderDate.events['change'].suspend();
    App.txtOrdDiscAmt.events['change'].suspend();
    App.txtVolDiscPct.events['change'].suspend();
    App.cboShiptoID.events['change'].suspend();

    App.frmMain.loadRecord(record);

    App.cboShiptoID.events['change'].resume();
    App.cboOrderType.events['change'].resume();
    App.cboOrderNbr.events['change'].resume();
    App.cboCustID.events['change'].resume();
    App.cboSlsPerID.events['change'].resume();
    App.txtOrderDate.events['change'].resume();
    App.txtOrdDiscAmt.events['change'].resume();
    App.txtVolDiscPct.events['change'].resume();

    setStatusForm();

    HQ.common.showBusy(true, HQ.waitMsg);

    if (!Ext.isEmpty(App.cboOrderNbr.getValue())) {
        HQ.numDetail = 0;
        loadCust(record.data.CustID, record.data.OrderDate, '', false);
    } else {
        HQ.objCust = null;
        App.stoLotTrans.clearFilter();
        App.stoOrdDet.clearData();
        App.stoLotTrans.clearData();
        App.stoTaxTrans.clearData();
        App.stoOrdDisc.clearData();
        App.stoOrdAddr.clearData();
        App.grdOrdDet.view.refresh();
        App.grdTaxTrans.view.refresh();
        App.grdDisc.view.refresh();
        bindDetail();
    }

    App.cboHandle.setValue('N');
}
var bindDetail = function () {

    if (Ext.isEmpty(HQ.objCust)) {
        HQ.objCust = Ext.create('App.mdlCustomer')
    }

    var addr = App.stoOrdAddr.first();
    if (addr == undefined) {
        addr = Ext.create('App.mdlOrdAddr');
    }

    App.cboShiptoID.events['change'].suspend();
    App.cboShiptoID.setValue(Ext.isEmpty(addr.data.ShiptoID) ? HQ.objCust.data.DfltShipToId : addr.data.ShiptoID);
    App.cboShiptoID.events['change'].resume();


    if (Ext.isEmpty(HQ.objCust.data.BillAddr1)) {
        App.txtCustAddr.setValue(HQ.objCust.data.BillAddr2)
    } else {
        App.txtCustAddr.setValue(HQ.objCust.data.BillAddr1 + (Ext.isEmpty(HQ.objCust.data.BillAddr2) ? "" : " - " + HQ.objCust.data.BillAddr2));
    }

    App.txtCustName.setValue(HQ.objCust.data.BillName);

    App.txtBillName.setValue(addr.data.BillName);
    App.txtBillAttn.setValue(addr.data.BillAttn);
    App.txtBillAddrLine1.setValue(addr.data.BillAddrLine1);
    App.txtBillAddrLine2.setValue(addr.data.BillAddrLine2);
    App.txtBillZip.setValue(addr.data.BillZip);
    App.txtBillPhone.setValue(addr.data.BillPhone);
    App.txtBillFax.setValue(addr.data.BillFax);
    App.txtBillCity.setValue(addr.data.BillCity);
    App.txtBillStateID.setValue(addr.data.BillStateID);
    App.txtBillCntryID.setValue(addr.data.BillCntryID);
    App.txtTaxRegNbr.setValue(addr.data.TaxRegNbr);

    App.txtShipName.setValue(addr.data.ShipName);
    App.txtShipAttn.setValue(addr.data.ShipAttn);
    App.txtShipAddrLine1.setValue(addr.data.ShipAddrLine1);
    App.txtShipAddrLine2.setValue(addr.data.ShipAddrLine2);
    App.txtShipFax.setValue(addr.data.ShipFax);
    App.txtShipPhone.setValue(addr.data.ShipPhone);
    App.txtShipZip.setValue(addr.data.ShipZip);
    App.txtShipCity.setValue(addr.data.ShipCity);
    App.txtShipStateID.setValue(addr.data.ShipStateID);
    App.txtShipCntryID.setValue(addr.data.ShipCntryID);

    var newRow = Ext.create('App.mdlOrdDet');
    newRow.data.BOType = 'S';
    HQ.store.insertRecord(App.stoOrdDet, "InvtID", newRow, true);

    calcDet();
    calcTaxTotal();

    checkDetAdd();

    setChange(false);

    HQ.common.showBusy(false);
    App.grdOrdDet.view.loadMask.setDisabled(false);
}
var bindCust = function () {

    HQ.objCust = App.stoCustomer.first();
    if (Ext.isEmpty(HQ.objCust)) {
        HQ.objCust = Ext.create('App.mdlCustomer')
    }
    App.stoTax.clearFilter();
    App.stoTax.filterBy(function (record) {
        if (record.data.TaxID == HQ.objCust.data.TaxID00 || record.data.TaxID == HQ.objCust.data.TaxID01 || record.data.TaxID == HQ.objCust.data.TaxID02 || record.data.TaxID == HQ.objCust.data.TaxID03) {
            return record;
        }
    });

    if (Ext.isEmpty(App.cboOrderNbr.getValue())) {

        App.cboTerms.setValue(HQ.objCust.data.Terms);
        App.cboSlsPerID.setValue(HQ.objCust.data.SlsperId);
        App.txtTaxRegNbr.setValue(HQ.objCust.data.TaxRegNbr);

        App.cboShiptoID.events['change'].suspend();
        App.cboShiptoID.setValue(HQ.objCust.data.DfltShipToId);
        App.cboShiptoID.events['change'].resume();

        bindAddress();

        HQ.common.showBusy(false);
    } else {
        App.stoOrdDet.reload();
        App.stoLotTrans.reload();
        App.stoTaxTrans.reload();
        App.stoOrdDisc.reload();
        App.stoOrdAddr.reload();
    }
}
var bindAddress = function () {

    var addr = App.stoSOAddress.first();

    if (Ext.isEmpty(addr)) {
        addr = Ext.create('App.mdlSOAddress');
    }

    if (Ext.isEmpty(HQ.objCust.data.BillAddr1)) {
        App.txtCustAddr.setValue(HQ.objCust.data.BillAddr2)
    } else {
        App.txtCustAddr.setValue(HQ.objCust.data.BillAddr1 + (Ext.isEmpty(HQ.objCust.data.BillAddr2) ? "" : " - " + HQ.objCust.data.BillAddr2));
    }

    App.txtCustName.setValue(HQ.objCust.data.BillName);

    App.txtShipName.setValue(addr.data.SOName);
    App.txtShipAttn.setValue(addr.data.Attn);
    App.txtShipAddrLine1.setValue(addr.data.Addr1);
    App.txtShipAddrLine2.setValue(addr.data.Addr2);
    App.txtShipCity.setValue(addr.data.City);
    App.txtShipStateID.setValue(addr.data.State);
    App.txtShipCntryID.setValue(addr.data.Country);
    App.txtShipZip.setValue(addr.data.Zip);
    App.txtShipPhone.setValue(addr.data.Phone);
    App.txtShipFax.setValue(addr.data.Fax);

    App.txtBillAddrLine1.setValue(HQ.objCust.data.BillAddr1);
    App.txtBillAddrLine2.setValue(HQ.objCust.data.BillAddr2);
    App.txtBillAttn.setValue(HQ.objCust.data.BillAttn);
    App.txtBillCity.setValue(HQ.objCust.data.BillCity);
    App.txtBillCntryID.setValue(HQ.objCust.data.BillCountry);
    App.txtBillFax.setValue(HQ.objCust.data.BillFax);
    App.txtBillName.setValue(HQ.objCust.data.BillName);
    App.txtBillPhone.setValue(HQ.objCust.data.BillPhone);
    App.txtBillStateID.setValue(HQ.objCust.data.BillState);
    App.txtBillZip.setValue(HQ.objCust.data.BillZip);
    App.txtShipPriority.setValue("A");
}

var save = function () {

    if ((App.cboOrderNbr.getValue() && !HQ.isUpdate) || (Ext.isEmpty(App.cboOrderNbr.getValue()) && !HQ.isInsert)) {
        HQ.message.show(728, '', '', true);
        return;
    }
    if (App.cboStatus.getValue() != "N" && (App.cboHandle.getValue() == "N" || Ext.isEmpty(App.cboHandle.getValue()))) {
        HQ.message.show(2015020803, '', '', true);
        return;
    }

    if (App.stoOrdDet.data.items.length <= 1) {
        HQ.message.show(2015020804, [App.cboOrderNbr.getValue()], '', true);
        return;
    }
    if (Ext.isEmpty(App.cboShiptoID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('shiptoid')], '', true);
        return;
    }
    var flat = null;
    App.stoLotTrans.clearFilter();
    App.stoOrdDet.data.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
            if (item.data.LineQty == 0) {
                HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
                flat = item;
                return false;
            }

            if (Ext.isEmpty(item.data.SiteID)) {
                HQ.message.show(1000, [HQ.common.getLang('siteid')], '', true);
                flat = item;
                return false;
            }

            if (Ext.isEmpty(item.data.SlsUnit)) {
                HQ.message.show(1000, [HQ.common.getLang('unit')], '', true);
                flat = item;
                return false;
            }

            if (Ext.isEmpty(item.data.UnitMultDiv)) {
                HQ.message.show(2525, [invtID], '', true);
                flat = item;
                return false;
            }


            if (item.data.FreeItem && item.data.LineAmt != 0) {
                HQ.message.show(703, '', '', true);
                flat = item;
                return false;
            }
            if (!item.data.FreeItem && item.data.BOType != "R" && item.data.LineAmt == 0 && item.data.QtyBO == 0) {
                HQ.message.show(703, '', '', true);
                flat = item;
                return false;
            }
            if (HQ.objType.BO) {
                if (item.data.BOType != "O" && item.data.LineQty == 0 && item.data.QtyBO == 0) {
                    HQ.message.show(233, '', '', true);
                    flat = item;
                    return false;
                }
            }
            else {
                if (item.data.LineQty == 0 && item.data.QtyBO == 0) {
                    HQ.message.show(233, '', '', true);
                    flat = item;
                    return false;
                }
            }
            if (item.data.SlsPrice == 0 && !item.data.FreeItem) {
                HQ.message.show(726, '', '', true);
                flat = item;
                return false;
            }
            if (HQ.objOM.ReqDiscID && Ext.isEmpty(item.data.DiscCode) && Ext.isEmpty(item.data.DiscID1) && item.data.FreeItem) {
                HQ.message.show(746, '', '', true);
                flat = item;
                return false;
            }

            if (item.data.BOType == "B" && Ext.isEmpty(item.data.BOCustID)) {
                HQ.message.show(734, '', '', true);
                flat = item;
                return false;
            }
            if (Ext.isEmpty(item.invt)) {
                item.invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [item.data.InvtID]);
            }
            if (item.invt.LotSerTrack != "N" && !Ext.isEmpty(item.invt.LotSerTrack)) {
                var lotQty = 0;
                var lotFlat = false;
                App.stoLotTrans.data.each(function (item2) {
                    if (item.data.LineRef == item2.data.OMLineRef && !Ext.isEmpty(item2.data.LotSerNbr)) {
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

                var detQty = Math.round(item.data.UnitMultDiv == "M" ? item.data.LineQty * item.data.UnitRate : item.data.LineQty / item.data.UnitRate);
                if (detQty != lotQty) {
                    HQ.message.show(2015040502, [item.data.InvtID], "", true);
                    flat = item;
                    return false;
                }
            }

        }
    });
    if (!Ext.isEmpty(flat)) {
        App.smlOrdDet.select(App.stoOrdDet.indexOf(flat));
        return;
    }
    
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            method: 'POST',
            url: 'OM10100/Save',
            timeout: 180000,
            params: {
                lstOrdDet: Ext.encode(App.stoOrdDet.getRecordsValues()),
                lstLot: Ext.encode(App.stoLotTrans.getRecordsValues()),
                lstTax: Ext.encode(App.stoTaxTrans.getRecordsValues()),
                lstDisc: Ext.encode(App.stoOrdDisc.getRecordsValues())
            },
            success: function (msg, data) {

                var orderNbr = '';

                if (this.result.data != undefined && this.result.data.orderNbr != null) {
                    orderNbr = this.result.data.orderNbr
                }
                if (!Ext.isEmpty(orderNbr)) {
                    App.cboOrderNbr.forceSelection = false
                    App.cboOrderNbr.events['change'].suspend();
                    App.cboOrderNbr.setValue(orderNbr);
                    App.cboOrderNbr.events['change'].resume();
                    if (Ext.isEmpty(HQ.recentRecord)) {
                        HQ.recentRecord = orderNbr;
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

var calcLot = function (record) {
    if (!Ext.isEmpty(record.invt) && !Ext.isEmpty(record.invt.LotSerTrack) && record.invt.LotSerTrack != 'N' && !Ext.isEmpty(record.data.SlsUnit)) {
        var flat = false;
        var det = record.data;

        App.stoLotTrans.clearFilter();
        App.stoLotTrans.data.each(function (item) {
            if (item.data.OMLineRef == det.LineRef && !Ext.isEmpty(item.data.LotSerNbr)) {
                flat = true;
            }
        });
        var back = !(record.data.BOType != "B" && HQ.objType.INDocType != "CM" && HQ.objType.INDocType != "DM" && HQ.objType.INDocType != "NA" && HQ.objType.INDocType != "RC");
        if (!flat && !back) {
            HQ.common.showBusy(true, HQ.waitMsg);
            App.stoCalcLot.load({
                params: {
                    siteID: det.SiteID,
                    invtID: det.InvtID,
                    branchID: App.txtBranchID.getValue(),
                    orderNbr: App.cboOrderNbr.getValue(),
                    all: back
                },
                det: record.data,
                row: record,
                callback: function (records, options, success) {

                    var det = options.det;
                    var needQty = Math.round(det.UnitMultDiv == "M" ? det.LineQty * det.UnitRate : det.LineQty / det.UnitRate);

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
                            newLot.data.OrderNbr = App.cboOrderNbr.getValue();
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
                    showLot(options.row, false);
                }
            });
        } else {
            showLot(record, true);
        }
    }
}

var showLot = function (record, loadCombo) {

    var lock = !((App.cboOrderNbr.value && HQ.isUpdate) || (!App.cboOrderNbr.value && HQ.isInsert)) || App.cboStatus.getValue() != "N" || record.data.FreeItem;
    App.grdLot.isLock = lock;
    if (loadCombo) {
        var back = !(record.data.BOType != "B" && HQ.objType.INDocType != "CM" && HQ.objType.INDocType != "DM" && HQ.objType.INDocType != "NA" && HQ.objType.INDocType != "RC");
        App.stoCalcLot.load({
            params: {
                siteID: record.data.SiteID,
                invtID: record.data.InvtID,
                branchID: App.txtBranchID.getValue(),
                orderNbr: App.cboOrderNbr.getValue(),
                all: back
            }
        });
    }


    App.stoLotTrans.clearFilter();
    App.stoLotTrans.filter('OMLineRef', record.data.LineRef);

    var newRow = Ext.create('App.mdlLotTrans');
    newRow.data.OMLineRef = record.data.LineRef;
    HQ.store.insertRecord(App.stoLotTrans, "LotSerNbr", newRow, true);

    App.winLot.record = record;
    App.grdLot.view.refresh();
    App.winLot.setTitle(record.data.InvtID + ' ' + (record.data.UnitMultDiv == "M" ? record.data.LineQty * record.data.UnitRate : record.data.LineQty / record.data.UnitRate) + ' ' + record.invt.StkUnit);
    App.winLot.show();
}
var deleteHeader = function (item) {
    if (item == 'yes') {
        if (Ext.isEmpty(App.cboOrderNbr.getValue())) {
            menuClick('new');
        } else {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                method: 'POST',
                url: 'OM10100/Delete',
                timeout: 180000,
                params: {
                    lstOrdDet: Ext.encode(App.stoOrdDet.getRecordsValues()),
                    lstTax: Ext.encode(App.stoTaxTrans.getRecordsValues()),
                    lstDisc: Ext.encode(App.stoOrdDisc.getRecordsValues())
                },
                success: function (msg, data) {
                    var record = App.stoOrder.getById(App.cboOrderNbr.getValue());
                    if (!Ext.isEmpty(record)) {
                        App.stoOrder.remove(record);
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
}

var deleteDet = function (item) {
    if (item == 'yes') {
        if (Ext.isEmpty(App.smlOrdDet.selected.items[0].data.tstamp)) {
            HQ.message.show(2015032101, "", "", true);
            delTax(App.smlOrdDet.selected.items[0]);
            calcTaxTotal();

            var det = App.smlOrdDet.selected.items[0].data;
            App.stoLotTrans.clearFilter();
            for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
                if (det.LineRef == App.stoLotTrans.data.items[i].data.OMLineRef) {
                    App.stoLotTrans.data.removeAt(i);
                }
            }

            App.grdOrdDet.deleteSelected();
            App.grdTaxTrans.view.refresh();
            calcDet();
        } else {
            App.stoLotTrans.clearFilter();
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                method: 'POST',
                url: 'OM10100/DeleteDet',
                timeout: 180000,
                params: {
                    lstOrdDet: Ext.encode(App.stoOrdDet.getRecordsValues()),
                    lstLot: Ext.encode(App.stoLotTrans.getRecordsValues()),
                    lineRef: App.grdOrdDet.getSelectionModel().selected.items[0].data.LineRef,
                    lstTax: Ext.encode(App.stoTaxTrans.getRecordsValues()),
                    lstDisc: Ext.encode(App.stoOrdDisc.getRecordsValues())
                },
                success: function (msg, data) {

                    HQ.message.process(msg, data, true);
                    App.grdOrdDet.deleteSelected();
                    App.stoOrder.load();

                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
}
var deleteLot = function (item) {
    if (item == 'yes') {
        App.grdLot.deleteSelected();
    }
}
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
}

//////////////////////////////////
var checkExitEdit = function (row) {
    var key = row.field;
    var det = row.record.data;
    var invt = row.record.invt;
    var record = row.record;
    var qtyTot = 0, editQty = 0, stkQty = 0, lineQty = 0;

    if (key == "LineQty" || key == "UnitRate") {
        det.StkQty = det.UnitMultDiv == "D" ? det.LineQty / det.UnitRate : det.LineQty * det.UnitRate;
        det.DumyLineQty = det.LineQty;
    }

    if (key == "LineQty" || key == "QtyBO" || key == "SlsPrice" || key == "DiscAmt" || key == "DiscPct" || key == "ManuDiscAmt") {

        if (key == "LineQty") {
            var budgetID = "";
            var discCode = "";
            var discID = "";
            var discSeg = "";
            var mannualDisc = false;
            var firstCal = true;

            if (!Ext.isEmpty(det.DiscCode) && det.FreeItem) {
                var objDisc = HQ.store.findInStore(App.stoDisc, ['DiscCode'], [det.DiscCode]);
                if (!Ext.isEmpty(objDisc)) {
                    budgetID = objDisc.BudgetID;
                    discCode = objDisc.DiscCode;
                    discID = discCode;
                    discSeg = "";
                    mannualDisc = true;
                    firstCal = true;
                }
            }
            else if (!Ext.isEmpty(det.BudgetID1) && det.FreeItem) {
                budgetID = det.BudgetID1;
                discID = det.DiscID1;
                discSeg = det.DiscSeq1;
                firstCal = true;
            }
            else if (!Ext.isEmpty(det.BudgetID2) && det.FreeItem) {
                budgetID = det.BudgetID2;
                discID = det.DiscID2;
                discSeg = det.DiscSeq2;
                firstCal = false;
            }

            if (HQ.objOM.DfltSalesPrice == "I") {
                det.SlsPrice = det.UnitMultDiv == "M" ? invt.SOPrice * det.UnitRate : invt.SOPrice / det.UnitRate;
            }
            else {

                if (HQ.objType.INDocType == "II" || HQ.objType.INDocType == "RC") {
                    var itemSite = HQ.store.findInStore(App.stoItemSite, ['SiteID', 'InvtID'], [det.SiteID, det.InvtID]);
                    if (det.UnitMultDiv == "M")
                        det.SlsPrice = itemSite.AvgCost * itemSite.UnitRate;
                    else
                        det.SlsPrice = itemSite.AvgCost / (det.UnitRate == 0 ? 1 : det.UnitRate);
                }
                else {

                    var price = HQ.store.findInStore(App.stoPrice, ['InvtID', 'Unit'], [det.InvtID, det.SlsUnit]);
                    if (!Ext.isEmpty(price)) det.SlsPrice = price.Price;
                    else det.SlsPrice = 0;

                    if (det.UnitMultDiv == "M")
                        stkQty = det.LineQty * (det.UnitRate == 0 ? 1 : det.UnitRate);
                    else
                        stkQty = det.LineQty / (det.UnitRate == 0 ? 1 : det.UnitRate);

                    if (det.FreeItem) {
                        if (HQ.objOM.InlcSOFeeProm) {
                            det.SOFee = Math.round(invt.SOFee * stkQty);
                        }
                        else
                            det.SOFee = 0;
                    }
                    else
                        det.SOFee = Math.round(invt.SOFee * stkQty);
                }
            }


        } else if (key == "SlsPrice") {
            if (!det.FreeItem && det.BOType != "R") {

                var soFee = 0;

                if (HQ.objOM.InlcSOFeeDisc)
                    soFee = det.SOFee;
                else
                    soFee = 0;

                if (det.DiscAmt != 0) det.ManuDiscAmt = 0;

                if (det.BOType == "O" && det.DiscAmt1 == 0 && det.DiscAmt2 == 0) {
                    det.DiscAmt2 = Math.round((soFee + det.LineQty * det.SlsPrice) * (det.DiscPct / 100));
                    det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }
                else if (det.DiscAmt1 == 0 && det.DiscAmt2 == 0) {
                    det.DiscAmt2 = Math.round((soFee + det.LineQty * det.SlsPrice) * (det.DiscPct / 100));
                    det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }

            }
        } else if (key == "QtyBO") {
            if (det.DiscAmt != 0) det.ManuDiscAmt = 0;
            if (det.BOType == "O" && det.DiscAmt1 == 0 && det.DiscAmt2 == 0) {
                det.DiscAmt2 = Math.round(det.LineQty * det.SlsPrice * (det.DiscPct / 100));
                det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
            }
        } else if (key == "DiscAmt" && det.LineQty != 0 && det.SlsPrice != 0) {
            if (!det.FreeItem && det.BOType != "R") {
                var soFee = 0;
                if (HQ.objOM.InlcSOFeeDisc)
                    soFee = det.SOFee;
                else
                    soFee = 0;

                det.DiscAmt1 = det.DiscAmt;

                if (det.BOType == "O") {
                    det.DiscPct = +((det.DiscAmt * 100) / ((det.LineQty) * det.SlsPrice + soFee - det.ManuDiscAmt)).toFixed(2);
                    det.LineAmt = Math.round((det.LineQty) * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }
                else {
                    det.DiscPct = +((det.DiscAmt * 100) / (det.LineQty * det.SlsPrice + soFee - det.ManuDiscAmt)).toFixed(2);
                    det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }
            }
        } else if (key == "DiscPct") {
            if (!det.FreeItem && det.BOType != "R") {
                var soFee = 0;
                if (HQ.objOM.InlcSOFeeDisc)
                    soFee = det.SOFee;
                else
                    soFee = 0;

                det.DiscAmt1 = det.DiscAmt;

                if (det.BOType == "O") {
                    det.DiscAmt = Math.round((soFee - det.ManuDiscAmt + (det.LineQty) * det.SlsPrice) * (det.DiscPct / 100));
                    det.LineAmt = Math.round((det.LineQty) * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }
                else {
                    det.DiscAmt = Math.round((soFee - det.ManuDiscAmt + det.LineQty * det.SlsPrice) * (det.DiscPct / 100));
                    det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }
            }
        } else if (key == "ManuDiscAmt") {
            if (!det.FreeItem && det.BOType != "R") {
                var soFee = 0;
                if (HQ.objOM.InlcSOFeeDisc)
                    soFee = det.SOFee;
                else
                    soFee = 0;

                if (det.BOType == "O") {
                    if (det.ManuDiscAmt == 0) {
                        //det.DiscAmt = Math.Round((soFee + (det.LineQty + det.QtyBO) * det.SlsPrice) * (det.DiscPct / 100), 0);
                        det.DiscAmt = Math.round((soFee + (det.LineQty) * det.SlsPrice) * (det.DiscPct / 100));
                    }
                    //det.LineAmt = Math.Round((det.LineQty + det.QtyBO) * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt, 0);
                    det.LineAmt = Math.round((det.LineQty) * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }
                else {
                    if (det.ManuDiscAmt == 0)
                        det.DiscAmt = Math.round((soFee + det.LineQty * det.SlsPrice) * (det.DiscPct / 100));
                    det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
                }
            }
        }
        record.commit();
        getQtyAvail(row.record);
        if (key == "LineQty") {
            calcLot(record);
        }
        checkSubDisc(record);
        checkTaxInGrid(key, record);
        calcDet();


    } else if (key == "LineAmt") {
        var soFee = 0;
        if (HQ.objOM.InlcSOFeeDisc)
            soFee = det.SOFee;
        else
            soFee = 0;

        det.ManuDiscAmt = 0;

        if (det.BOType == "O") {
            det.SlsPrice = Math.round((det.DiscAmt + det.LineAmt) / (det.LineQty));
            det.DiscPct = +((det.DiscAmt * 100) / ((det.LineQty) * det.SlsPrice + soFee)).toFixed(2);
        }
        else {
            det.SlsPrice = Math.round((det.DiscAmt + det.LineAmt) / det.LineQty);
            det.DiscPct = +((det.DiscAmt * 100) / (det.LineQty * det.SlsPrice + soFee)).toFixed(2);
        }
        record.commit();

    } else if (key == "SiteID") {
        var price = 0;

        if (HQ.objOM.DfltSalesPrice == "I") {
            price = Math.round(det.UnitMultDiv == "M" ? invt.SOPrice * det.UnitRate : invt.SOPrice / det.UnitRate);
            det.SlsPrice = price;
        }
        else {

            var price = HQ.store.findInStore(App.stoPrice, ['InvtID', 'Unit'], [det.InvtID, det.SlsUnit]);

            if (!Ext.isEmpty(price)) det.SlsPrice = price.Price;
            else det.SlsPrice = 0;
        }

        det.LineQty = 0;
        det.QtyBO = 0;

        App.stoLotTrans.clearFilter();
        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (App.stoLotTrans.data.items[i].data.OMLineRef == det.LineRef) {
                App.stoLotTrans.data.removeAt(i);
            }
        }

        calcDet();
        record.commit();
        getQtyAvail(row.record);
        checkTaxInGrid(key, record);

    } else if (key == "SlsUnit") {
        var price = 0;
        var cnvFact = 0;
        var unitMultDiv = "";

        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, det.SlsUnit);
        if (!Ext.isEmpty(cnv)) {
            cnvFact = cnv.CnvFact;
            unitMultDiv = cnv.MultDiv;
            det.UnitRate = cnvFact;
            det.UnitMultDiv = unitMultDiv;
        } else {
            det.UnitMultDiv = '';
            det.UnitPrice = 0;
            det.SlsUnit = '';
            record.commit();
            App.grdOrdDet.view.loadMask.hide();
            App.grdOrdDet.view.loadMask.setDisabled(false);
            return;
        }

        if (HQ.objOM.DfltSalesPrice == "I") {
            price = Math.round(unitMultDiv == "M" ? invt.SOPrice * cnvFact : invt.SOPrice / cnvFact);
            det.SlsPrice = price;
        }
        else {

            var price = HQ.store.findInStore(App.stoPrice, ['InvtID', 'Unit'], [det.InvtID, det.SlsUnit]);

            if (!Ext.isEmpty(price)) det.SlsPrice = price.Price;
            else det.SlsPrice = 0;
        }
        det.LineQty = 0;
        det.QtyBO = 0;
        det.SOFee = 0;
        det.LineAmt = 0;
        record.commit();

        calcDet();
        getQtyAvail(row.record);
        calcLot(row.record);
        checkTaxInGrid(key, record);

    } else if (key == "FreeItem") {
        if (det.FreeItem) {

            det.DiscPct = 0;
            det.DiscAmt = 0;
            det.DocDiscAmt = 0;
            det.LineAmt = 0;
            det.ManuDiscAmt = 0;

            if (HQ.objOM.InlcSOFeeProm) det.SOFee = Math.round(invt.SOFee * det.StkQty);
            else det.SOFee = 0;
        }
        else
            det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);

        record.commit();

    } else if (key == "CostID" && invt.ValMthd == "S") {

    } else if (key == "SOFee") {
        var soFee = 0;
        if (HQ.objOM.InlcSOFeeDisc) soFee = det.SOFee;
        else soFee = 0;

        if (det.BOType != "R" && !det.FreeItem) {
            if (det.DiscAmt != 0)
                record.data.ManuDiscAmt = 0;

            record.data.DiscAmt = Math.round((soFee + det.LineQty * det.SlsPrice) * (det.DiscPct / 100));
            record.data.DiscPct = +((det.DiscAmt * 100) / (det.LineQty * det.SlsPrice + soFee)).toFixed(2);
            record.data.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
        }

        record.commit();
        calcDet();

    } else if (key == "DiscCode") {
        var budgetID = "";
        var discCode = "";
        var tmp = "";
        var tmplineQty = det.LineQty;

        var objDisc = HQ.store.findInStore(App.stoDisc, ['DiscCode'], [det.DiscCode]);

        if (!Ext.isEmpty(objDisc)) det.BudgetID1 = objDisc.BudgetID;
        else det.BudgetID1 = "";

        record.commit();

    } else if (key == "InvtID" || key == "BarCode") {
        var invt = row.record.invt;
        if (key == "BarCode") {
            det.InvtID = invt.InvtID;
        }

        if (key == "InvtID") {
            det.BarCode = invt.BarCode;
        }

        var site = HQ.store.findInStore(App.stoItemSite, ['SiteID', 'InvtID'], [HQ.objUser.OMSite, det.InvtID]);

        if (!Ext.isEmpty(site)) {
            det.SiteID = HQ.objUser.OMSite;
        }
        else {
            site = HQ.store.findInStore(App.stoItemSite, ['SiteID', 'InvtID'], [invt.DfltSite, det.InvtID]);
            if (Ext.isEmpty(site)) {
                site = Ext.create('App.mdlItemSite');
            }
        }

        var cnvFact = 0;
        var unitMultDiv = "";

        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.DfltSOUnit);

        if (!Ext.isEmpty(cnv)) {
            cnvFact = cnv.CnvFact;
            unitMultDiv = cnv.MultDiv;
            det.SlsUnit = invt.DfltSOUnit;
            det.UnitRate = cnvFact;
            det.UnitMultDiv = unitMultDiv;
            det.SiteID = site.SiteID;
            det.Descr = invt.Descr;
            det.TaxCat = invt.TaxCat;
            det.ItemPriceClass = invt.PriceClassID;
        }
        else {
            det.UnitMultDiv = '';
            det.UnitPrice = 0;
            det.SlsUnit = '';
            det.InvtID = '';
            det.BarCode = '';
            record.commit();
            return;
        }

        var price = 0;
        if (HQ.objOM.DfltSalesPrice == "I") {
            price = unitMultDiv == "M" ? invt.SOPrice * cnvFact : invt.SOPrice / cnvFact;
            record.data.SlsPrice = price;
        }
        else {

            var price = HQ.store.findInStore(App.stoPrice, ['InvtID', 'Unit'], [det.InvtID, det.SlsUnit]);

            if (!Ext.isEmpty(price)) det.SlsPrice = price.Price;
            else det.SlsPrice = 0;
        }

        det.LineQty = 0;
        det.QtyBO = 0;
        det.SOFee = 0;
        det.LineAmt = 0;
        det.DiscPct = 0;
        det.DiscAmt = 0;
        det.ManuDiscAmt = 0;

        record.commit();

        checkDetAdd();

        calcDet();
        getQtyAvail(row.record);
        checkTaxInGrid(key, record);


        if ((key == 'InvtID' && !Ext.isEmpty(det.InvtID)) || (key == 'BarCode' && !Ext.isEmpty(det.BarCode))) {
            var newRow = Ext.create('App.mdlOrdDet');
            newRow.data.BOType = 'S';
            HQ.store.insertRecord(App.stoOrdDet, key, newRow, true);
        }


    }
    App.grdOrdDet.view.loadMask.hide();
    App.grdOrdDet.view.loadMask.setDisabled(false)
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
            lot.SlsUnit = '';
            record.commit();
            App.grdLot.view.loadMask.hide();
            App.grdLot.view.loadMask.setDisabled(false)
            return;
        }

        if (HQ.objOM.DfltSalesPrice == "I") {
            price = Math.round(unitMultDiv == "M" ? App.winLot.record.invt.SOPrice * cnvFact : App.winLot.record.invt.SOPrice / cnvFact);
            lot.UnitPrice = lot.UnitCost = price;
        }
        else {

            var price = HQ.store.findInStore(App.stoPrice, ['InvtID', 'Unit'], [App.winLot.record.invt.InvtID, lot.UnitDesc]);

            if (!Ext.isEmpty(price)) lot.UnitPrice = lot.UnitCost = price.Price;
            else lot.UnitPrice = lot.UnitCost = 0;
        }

        getLotQtyAvail(record);
    } else if (key == "LotSerNbr") {
        var flat = false;
        //App.stoLotTrans.data.each(function (item) {
        //    if (item.data.LotSerNbr == lot.LotSerNbr && item.id != record.id) {
        //        flat = true;
        //        return false;
        //    }
        //});
        if (flat) {
            HQ.message.show(219, "", "", true);
            lot.LotSerNbr = "";
            App.grdLot.view.loadMask.hide();
            App.grdLot.view.loadMask.setDisabled(false)
            record.commit();
            return;
        }
        lot.UnitDesc = App.winLot.record.data.SlsUnit;
        lot.UnitPrice = lot.UnitCost = App.winLot.record.data.SlsPrice;
        lot.UnitMultDiv = App.winLot.record.data.UnitMultDiv;
        lot.CnvFact = App.winLot.record.data.UnitRate;
        var itemLot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', 'LotSerNbr'], [lot.InvtID, lot.SiteID, lot.LotSerNbr]);
        if (!Ext.isEmpty(itemLot)) {
            lot.ExpDate = itemLot.ExpDate;
        }

        if (!Ext.isEmpty(lot.LotSerNbr)) {
            var newRow = Ext.create('App.mdlLotTrans');
            newRow.data.OMLineRef = lot.OMLineRef;
            HQ.store.insertRecord(App.stoLotTrans, key, newRow, true);
        }
    }
    record.commit();
    App.grdLot.view.loadMask.hide();
    App.grdLot.view.loadMask.setDisabled(false)
}
var checkSubDisc = function (record) {

    var det = record.data;
    if (!det.FreeItem && det.BOType != "R") {
        if (det.DiscAmt != 0) det.ManuDiscAmt = 0;
        if (det.BOType == "O") {
            det.DiscAmt = Math.round(det.LineQty * det.SlsPrice * (det.DiscPct / 100));
            det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
        }
        else {
            det.DiscAmt = Math.round(det.LineQty * det.SlsPrice * (det.DiscPct / 100));
            det.LineAmt = Math.round(det.LineQty * det.SlsPrice - det.DiscAmt - det.ManuDiscAmt);
        }
    }
    App.stoOrdDisc.data.each(function (item) {
        var disc = item.data;
        if (det.FreeItem && !Ext.isEmpty(det.DiscID1) && disc.FreeItemID == det.InvtID && disc.SOLineRef == det.LineRef) {
            disc.FreeItemQty = det.LineQty;
            disc.UserOperationLog = "User Changed Free Item Qty";
        }
    });

    App.stoOrdDisc.commitChanges();
    App.grdDisc.view.refresh();
}

var calcDet = function () {
    if (Ext.isEmpty(HQ.objType)) return;

    var taxAmt00 = 0;
    var taxAmt01 = 0;
    var taxAmt02 = 0;
    var taxAmt03 = 0;

    var soFee = 0;
    var curyLineDiscAmt = 0;
    var ordQty = 0;


    var curyLineAmt = 0;

    App.stoOrdDet.data.each(function (det) {
        taxAmt00 += det.data.TaxAmt00;
        taxAmt01 += det.data.TaxAmt01;
        taxAmt02 += det.data.TaxAmt02;
        taxAmt03 += det.data.TaxAmt03;
        soFee += det.data.SOFee;
        curyLineAmt += det.data.LineAmt;
        curyLineDiscAmt += det.data.DiscAmt + det.data.ManuDiscAmt;
        ordQty += det.data.LineQty;
    });




    App.txtSOFeeTot.setValue(Math.round(soFee));
    App.txtCuryTaxAmt.setValue(Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03));
    App.txtCuryLineDiscAmt.setValue(Math.round(curyLineDiscAmt));
    App.txtCuryLineAmt.setValue(Math.round(curyLineAmt));


    if (HQ.objType.DiscType == "B")
        App.txtTxblAmt.setValue(curyLineAmt);
    else {
        if (HQ.objType.TaxFee)
            App.txtTxblAmt.setValue(curyLineAmt - App.txtCuryTaxAmt.getValue() +
                               App.txtSOFeeTot.getValue() * 0.1);
        else
            App.txtTxblAmt.setValue(curyLineAmt - App.txtCuryTaxAmt.getValue());
    }


    App.txtCuryOrdAmt.setValue(Math.round(App.txtTxblAmt.getValue() + App.txtFreightAmt.getValue() +
                                        App.txtMiscAmt.getValue() + App.txtCuryTaxAmt.getValue() +
                                        App.txtSOFeeTot.getValue() - App.txtVolDiscAmt.getValue() -
                                        App.txtOrdDiscAmt.getValue()))
    App.txtOrdQty.setValue(Math.round(ordQty));
}

var updateDistPctAmt = function () {
    App.txtOrdDiscAmt.events['change'].suspend();
    if (HQ.objOM.InlcSOFeeDisc) {
        if (App.txtVolDiscAmt.getValue() == 0) {
            if (HQ.objType.TaxFee)
                App.txtOrdDiscAmt.setValue(Math.round((App.txtVolDiscPct.getValue() * (App.txtCuryLineAmt.getValue() + App.txtSOFeeTot.getValue() * 1.1 - App.txtVolDiscAmt.getValue())) / 100));
            else
                App.txtOrdDiscAmt.setValue(Math.round((App.txtVolDiscPct.getValue() * (App.txtCuryLineAmt.getValue() + App.txtSOFeeTot.getValue() - App.txtVolDiscAmt.getValue())) / 100));
        }
        else
            App.txtOrdDiscAmt.setValue(Math.round((App.txtVolDiscPct.getValue() * (App.txtCuryLineAmt.getValue() - App.txtVolDiscAmt.getValue())) / 100));
    } else {
        App.txtOrdDiscAmt.setValue(Math.round((App.txtVolDiscPct.getValue() * (App.txtCuryLineAmt.getValue() - App.txtVolDiscAmt.getValue())) / 100));
    }
    App.txtOrdDiscAmt.events['change'].resume();
}

var defaultOnNew = function () {
    var record = Ext.create('App.mdlOrder');

    if (Ext.isEmpty(App.cboOrderType.getValue())) {
        App.cboOrderType.events['change'].suspend();
        App.cboOrderType.setValue('IN');
        App.cboOrderType.events['change'].resume();
        record.data.OrderType = 'IN';
    } else {
        record.data.OrderType = App.cboOrderType.getValue();
    }

    orderTypeContrainst();
    formatGrid();

    record.data.DoNotCalDisc = App.chkDoNotCalDisc.getValue();
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'N';
    record.data.OrderDate = HQ.businessDate;
    record.data.ARDocDate = HQ.businessDate;
    record.data.ShipDate = HQ.businessDate;
    record.data.ExpiryDate = HQ.businessDate;




    App.frmMain.validate();

    bindOrder(record);
}

var orderTypeContrainst = function () {

    if (Ext.isEmpty(HQ.objType)) {
        HQ.objType = App.cboOrderType.displayTplData[0];
    }
    if (Ext.isEmpty(HQ.objType)) {
        HQ.objType = Ext.create(App.stoOrderType.model.modelName).data;
    }
    if (HQ.objType.SalesType == "PET" || HQ.objType.SalesType == "PEX" || HQ.objType.SalesType == "POS" || HQ.objType.SalesType == "INS") {
        App.chkDoNotCalDisc.setReadOnly(false);
        App.chkDoNotCalDisc.setValue(false);
    }
    else {
        App.chkDoNotCalDisc.setReadOnly(false);
        App.chkDoNotCalDisc.setValue(false);
    }

    if (HQ.objType.AutoPromotion != 1)
        App.chkDoNotCalDisc.setValue(true);
    else
        App.chkDoNotCalDisc.setValue(false);
}

var formatGrid = function () {

    if (HQ.objType.BO) {
        App.colBOType.show();
        App.colQtyBO.show();
        App.colBOCustID.show();
        App.colOrigOrderNbr.show();
    }
    else {
        App.colBOType.hide();
        App.colQtyBO.hide();
        App.colBOCustID.hide();
        App.colOrigOrderNbr.hide();
    }
    if (HQ.objOM.UseBarCode == 1) {
        App.colBarCode.show();
        App.colInvtID.hide();
    }
    else {
        App.colBarCode.hide();
        App.colInvtID.show();
    }
    if (HQ.objIN.CnvFactEditable == true) {
        App.colUnitRate.show();
    }
    else {
        App.colUnitRate.hide();
    }

    App.colFreeItem.editable = HQ.objType.ManualDisc;
}

var lastLineRef = function () {
    var num = 0;
    App.stoOrdDet.data.each(function (item) {
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

var calcTaxTotal = function () {
    var flat = false;
    App.stoTaxDoc.clearData();
    App.stoTaxTrans.data.each(function (tax) {
        flat = true;
        App.stoTaxDoc.data.each(function (taxDoc) {
            if (tax.data.OrderNbr == taxDoc.data.OrderNbr && tax.data.TaxID == taxDoc.data.TaxID) {
                taxDoc.data.TxblAmt += tax.data.TxblAmt;
                taxDoc.data.TaxAmt += tax.data.TaxAmt;
                flat = false;
                return false;
            }
        });
        if (flat) {
            var newTaxDoc = Ext.create('App.mdlTaxDoc');
            newTaxDoc.data.BranchID = tax.data.BranchID;
            newTaxDoc.data.OrderNbr = tax.data.OrderNbr;
            newTaxDoc.data.TaxID = tax.data.TaxID;
            newTaxDoc.data.TaxAmt = tax.data.TaxAmt;
            newTaxDoc.data.TaxRate = tax.data.TaxRate;
            newTaxDoc.data.TxblAmt = tax.data.TxblAmt;
            App.stoTaxDoc.data.add(newTaxDoc);
        }
    });
    App.grdTaxDoc.view.refresh();
}

var checkTaxInGrid = function (key, record) {
    var det = record.data;
    if (key == "TaxID" || key == "TaxCat" || key == "InvtID" || key == "SiteID" || key == "LineQty" || key == "SlsUnit" || key == "SlsPrice" || key == "DiscPct" || key == "DiscAmt" || key == "SOFee" || key == "ManuDiscAmt" || key == "LineAmt" || key == "FreeItem") {
        delTax(record);
        if (!calcTax(record)) {
            det.SlsUnit = '';
            det.SiteID = '';
            det.SlsPrice = 0;
            det.TxblAmt00 = 0;
            det.TaxAmt00 = 0;
            det.LineQty = 0;
            det.QtyBO = 0;
            det.DiscPct = 0;
            det.DiscAmt = 0;
            det.ManuDiscAmt = 0;
            det.LineAmt = 0;
            record.commit();
        }
        calcTaxTotal();
    }
}

var delTax = function (record) {
    if (App.cboStatus.getValue() == "C" || App.cboStatus.getValue() == "L" || App.cboStatus.getValue() == "I") return false;
    var lineRef = record.data.LineRef;
    for (var j = App.stoTaxTrans.data.length - 1; j >= 0; j--) {
        if (App.stoTaxTrans.data.items[j].data.LineRef == lineRef)
            App.stoTaxTrans.data.removeAt(j);
    }
    clearTax(record);
    calcTaxTotal();
    calcDet();
    return true;
}

var clearTax = function (record) {
    record.data.TaxId00 = '';
    record.data.TaxAmt00 = 0;
    record.data.TxblAmt00 = 0;

    record.data.TaxId01 = '';
    record.data.TaxAmt01 = 0;
    record.data.TxblAmt01 = 0;

    record.data.TaxId02 = '';
    record.data.TaxAmt02 = 0;
    record.data.TxblAmt02 = 0;

    record.data.TaxId03 = '';
    record.data.TaxAmt03 = 0;
    record.data.TxblAmt03 = 0;

    record.commit();
}

var calcTax = function (record) {
    var det = record.data;
    if (App.cboStatus.getValue() == "C" || App.cboStatus.getValue() == "L" || App.cboStatus.getValue() == "I") return false;

    var groupDocDistAmt = det.DocDiscAmt + det.GroupDiscAmt1 + det.GroupDiscAmt2;

    var dt = [];
    if (det.TaxID == "*") {
        App.stoTax.data.each(function (item) {
            dt.push(item.data);
        });
    }
    else {
        var strTax = det.TaxID.split(',');
        if (strTax.length > 0) {
            for (var k = 0; k < strTax.length; k++) {
                for (var j = 0; j < App.stoTax.data.length; j++) {
                    if (strTax[k] == App.stoTax.data.items[j].data.TaxID) {
                        dt.push(App.stoTax.data.items[j].data);
                        break;
                    }
                }
            }
        }
        else {
            if (Ext.isEmpty(det.TaxID) || Ext.isEmpty(det.TaxCat)) App.stoOrdDet.data.items[i].set('TxblAmt00', det.LineAmt - groupDocDistAmt);
            return false;
        }
    }

    var taxCat = det.TaxCat;
    var prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
    for (var j = 0; j < dt.length; j++) {
        var objTax = HQ.store.findInStore(App.stoTax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if ((HQ.objType.DiscType == "A" && objTax.PrcTaxIncl == "0") ||
                    (HQ.objType.DiscType == "B" && objTax.PrcTaxIncl != "0")) {
                    HQ.message.show(730, '', '', true);
                    return false;
                }
                if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                    prcTaxInclRate = prcTaxInclRate + objTax.TaxRate;
                }
            }
        }
    }

    if (HQ.objType.SalesType == "PET" && !det.FreeItem) {
        txblAmtL1 = Math.round(det.SlsPrice / (1 + prcTaxInclRate / 100)) * det.LineQty - det.DiscAmt - det.ManuDiscAmt;
    }
    else {
        if (prcTaxInclRate == 0)
            txblAmtL1 = Math.round(det.LineAmt - groupDocDistAmt);
        else
            txblAmtL1 = Math.round((det.LineAmt - groupDocDistAmt) / (1 + prcTaxInclRate / 100));
    }

    det.TxblAmt00 = txblAmtL1;
    record.commit();
    for (var j = 0; j < dt.length; j++) {

        var taxID = "", lineRef = "";
        var taxRate = 0, taxAmtL1 = 0;
        var objTax = HQ.store.findInStore(App.stoTax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "1") {
                    taxID = dt[j].TaxID;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    taxAmtL1 = Math.round(txblAmtL1 * objTax.TaxRate / 100);

                    if (objTax.Lvl2Exmpt == 0) txblAmtAddL2 += txblAmtL1;

                    if (objTax.PrcTaxIncl != "0" && HQ.objType.SalesType != "PET") {
                        var chk = false;
                        if (j < dt.length - 1) {
                            for (var k = j + 1; k < dt.length; k++) {
                                objTax = dt[k];
                                if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
                                    if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat &&
                                                            objTax.CatExcept01 != taxCat && objTax.CatExcept02 != taxCat &&
                                                            objTax.CatExcept03 != taxCat && objTax.CatExcept04 != taxCat &&
                                                            objTax.CatExcept05 != taxCat)
                                                      || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                                                    objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                                                    objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                                        if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                                            chk = false;
                                            break;
                                        }
                                    }
                                }
                                chk = true;
                            }
                        }
                        else {
                            chk = true;
                        }

                        if (chk) {
                            if (HQ.objType.TaxFee) {
                                if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 - (det.SOFee * taxRate / 100) != det.LineAmt)
                                    taxAmtL1 = Math.round(det.LineAmt + (det.SOFee * taxRate / 100) - groupDocDistAmt - (totPrcTaxInclAmt + txblAmtL1));
                            }
                            else {
                                if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 != det.LineAmt) taxAmtL1 = Math.round(det.LineAmt - groupDocDistAmt - (totPrcTaxInclAmt + txblAmtL1));
                            }
                        }
                        else
                            totPrcTaxInclAmt += totPrcTaxInclAmt + taxAmtL1;
                    }

                    if (HQ.objType.TaxFee)
                        insertUpdateTax(taxID, lineRef, taxRate, taxAmtL1, txblAmtL1 + det.SOFee,
                                        1);
                    else
                        insertUpdateTax(taxID, lineRef, taxRate, taxAmtL1, txblAmtL1, 1);

                }
            }
        }
    }

    for (var j = 0; j < dt.Count; j++) {
        var taxID = "", lineRef = "";
        var taxRate = 0, txblAmtL2 = 0, taxAmtL2 = 0;
        var objTax = HQ.store.findInStore(App.stoTax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "2") {
                    taxID = dt[j].TaxID;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    txblAmtL2 = Math.round(txblAmtAddL2 + txblAmtL1);
                    taxAmtL2 = Math.round(txblAmtAddL2 * objTax.TaxRate / 100);
                    insertUpdateTax(taxID, lineRef, taxRate, taxAmtL2, txblAmtL2, 2);
                }
            }
        }
    }
    updateTax(record);
    calcDet();
    return true;
}

var insertUpdateTax = function (taxID, lineRef, taxRate, taxAmt, txblAmt, taxLevel) {
    var flat = false;
    for (var i = 0; i < App.stoTaxTrans.data.length; i++) {
        var tax = App.stoTaxTrans.data.items[i];
        if (tax.data.TaxID == taxID && tax.data.LineRef == lineRef) {
            tax.data.OrderNbr = Ext.isEmpty(App.cboOrderNbr.getValue()) ? '' : App.cboOrderNbr.getValue();
            tax.data.BranchID = App.txtBranchID.getValue();
            tax.data.TaxID = taxID;
            tax.data.LineRef = lineRef;
            tax.data.TaxRate = taxRate;
            tax.data.TaxLevel = taxLevel.toString();
            tax.data.TaxAmt = taxAmtl
            tax.data.TxblAmt = txblAmt;
            tax.commit();
            flat = true;
            break;
        }
    }
    if (!flat) {
        var newTax = Ext.create('App.mdlTaxTrans');
        newTax.data.BranchID = App.txtBranchID.getValue();
        newTax.data.OrderNbr = Ext.isEmpty(App.cboOrderNbr.getValue()) ? '' : App.cboOrderNbr.getValue();
        newTax.data.TaxID = taxID;
        newTax.data.LineRef = lineRef;
        newTax.data.TaxRate = taxRate;
        newTax.data.TaxLevel = taxLevel.toString();
        newTax.data.TaxAmt = taxAmt;
        newTax.data.TxblAmt = txblAmt;

        App.stoTaxTrans.data.add(newTax);
    }
    App.stoTaxTrans.sort('LineRef', "ASC");
    calcDet();

}

var updateTax = function (record) {
    var j = 0;
    var det = record.data;
    App.stoTaxTrans.data.each(function (item) {
        if (item.data.LineRef == det.LineRef) {
            if (j == 0) {
                det.TaxId00 = item.data.TaxID;
                det.TxblAmt00 = item.data.TxblAmt;
                det.TaxAmt00 = item.data.TaxAmt;
            }
            else if (j == 1) {
                det.TaxId01 = item.data.TaxID;
                det.TxblAmt01 = item.data.TxblAmt;
                det.TaxAmt01 = item.data.TaxAmt;
            }
            else if (j == 2) {
                det.TaxId02 = item.data.TaxID;
                det.TxblAmt02 = item.data.TxblAmt;
                det.TaxAmt02 = item.data.TaxAmt;
            }
            else if (j == 3) {
                det.TaxId03 = item.data.TaxID;
                det.TxblAmt03 = item.data.TxblAmt;
                det.TaxAmt03 = item.data.TaxAmt;
            }
            record.commit();
            j++;
        }
        if (j != 0 && item.data.LineRef != det.LineRef)
            return false;
    });

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

var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
}
var renderQtyAmt2 = function (value) {
    return Ext.util.Format.number(value, '0,000.00');
}

var setStatusForm = function () {

    var lock = true;

    if (!Ext.isEmpty(HQ.objOrder.data.OrderNbr)) {
        if (HQ.objOrder.data.Status == 'N') {
            lock = false;
        }
    } else {
        lock = !HQ.isInsert;
    }

    HQ.common.lockItem(App.frmMain, lock);

    App.grdOrdDet.isLock = lock;

    App.cboOrderNbr.setReadOnly(false);
    App.cboHandle.setReadOnly(false);
    App.cboCustID.setReadOnly(false);
    App.cboOrderType.setReadOnly(false);
    App.cboStatus.setReadOnly(true);
    App.txtCustName.setReadOnly(true);
    App.txtCustAddr.setReadOnly(true);
    App.txtVolDiscAmt.setReadOnly(true);
    App.txtCuryLineDiscAmt.setReadOnly(true);
    App.txtOrdQty.setReadOnly(true);
    App.txtTxblAmt.setReadOnly(true);
    App.txtCuryTaxAmt.setReadOnly(true);
    App.txtOrigOrderNbr.setReadOnly(true);
    App.txtARRefNbr.setReadOnly(true);
    App.txtLastInvcNbr.setReadOnly(true);
    App.txtSOFeeTot.setReadOnly(true);
    App.txtCuryOrdAmt.setReadOnly(true);

    if (!Ext.isEmpty(HQ.objOrder.data.OrderNbr)) {
        App.cboSlsPerID.setReadOnly(true);
        App.cboCustID.setReadOnly(true);
    }
}

var checkDetAdd = function () {
    var flat = false;
    App.stoOrdDet.data.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
            flat = true;
            return false;
        }
    });
    App.cboCustID.setReadOnly(true);//fix loi combo CustID khong the chon du set readonly = false
    App.cboCustID.setReadOnly(App.cboStatus.getValue() != 'N' || flat);
    App.cboSlsPerID.setReadOnly(App.cboStatus.getValue() != 'N' || flat);
    App.txtOrderDate.setReadOnly(App.cboStatus.getValue() != 'N' || flat);
}

var calculateInvtTotal = function (invtID, siteID, lineRef) {
    var oldQty = 0;
    App.stoOldOrdDet.data.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID) {
            oldQty += item.data.UnitMultDiv == "M" ? item.data.LineQty * item.data.UnitRate : item.data.LineQty / item.data.UnitRate;
        }
    });

    var qty = 0;
    App.stoOrdDet.data.each(function (item) {
        if (item.data.InvtID == invtID && item.data.SiteID == siteID) {
            qty += item.data.UnitMultDiv == "M" ? item.data.LineQty * item.data.UnitRate : item.data.LineQty / item.data.UnitRate;
        }
    });
    return qty - oldQty;
}

var getQtyAvail = function (row) {

    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [row.data.InvtID, row.data.SiteID]);
    var qty = 0;
    if (row.data.BOType != "B" && HQ.objType.INDocType != "CM" && HQ.objType.INDocType != "DM" && HQ.objType.INDocType != "NA" && HQ.objType.INDocType != "RC") {
        if (!Ext.isEmpty(site)) {
            qty = site.QtyAvail - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
            if (qty < 0) {
                HQ.message.show("1043", [row.data.InvtID, row.data.SiteID], "", true);
                row.data.LineQty = 0;
                row.commit();
                qty = site.QtyAvail - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
            }
        }
        else {
            qty = 0 - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
            if (qty < 0) {
                HQ.message.show("1043", [row.data.InvtID, row.data.SiteID], "", true);
                row.data.LineQty = 0;
                row.commit();
                qty = 0 - calculateInvtTotal(row.data.InvtID, row.data.SiteID, "");
            }

        }
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ": " + qty);
    } else {
        if (!Ext.isEmpty(site)) {
            qty = site.QtyAvail;
        } else {
            qty = 0;
        }
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ": " + qty);
    }



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

    if (det.data.BOType != "B" && HQ.objType.INDocType != "CM" && HQ.objType.INDocType != "DM" && HQ.objType.INDocType != "NA" && HQ.objType.INDocType != "RC") {
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
    } else {
        if (!Ext.isEmpty(lot)) {
            qtyAvail = lot.QtyAvail;
        } else {
            qtyAvail = 0;
        }
        App.lblLotQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + qtyAvail);
    }

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
        if (!Ext.isEmpty(App.cboOrderNbr.getValue())) {
            App.cboOrderType.setReadOnly(true);
            App.cboOrderNbr.setReadOnly(true);
        }

    } else {
        App.grdOrdDet.isChange = false;
        App.cboOrderType.setReadOnly(false);
        App.cboOrderNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'OM10100');
}

var renderBOType = function (value) {
    var r = HQ.store.findInStore(App.stoBOType, ['Code'], [value]);
    if (Ext.isEmpty(r)) {
        return "";
    }
    return r.Descr;
}
var renderRowNumber = function (value, meta, record) {
    return App.stoLotTrans.data.indexOf(record) + 1;
}
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////






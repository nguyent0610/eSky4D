HQ.recentRecord = null;
HQ.focus = 'batch';
HQ.objBatch = null;
HQ.isTransfer = false;

//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


var stoTrans_BeforeLoad = function () {
    App.grdTrans.view.loadMask.disable();
}
var stoHandle_Load = function () {
    App.Handle.setValue('N');
}
var stoTrans_Load = function () {
    bindTran();
}
var store_Load = function () {
    HQ.numSource++;
    checkSetDefault();
}
var stoBatch_Load = function () {
    var record = App.stoBatch.getById(App.BatNbr.getValue());
    if (record) {
        bindBatch(record);
    }
}
var stoBatch_BeforeLoad = function (store, operation, eOpts) {
    //if (Ext.isEmpty(operation.params.query)) {
    //   // operation.params.query = App.BatNbr.getValue();
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
    HQ.maxSource = 6;
    HQ.numTrans = 0;
    HQ.maxTrans = 3;
    HQ.numSelectTrans = 0;
    HQ.maxSelectTrans = 1;
    App.BatNbr.key = true;
    App.BranchID.setValue(HQ.cpnyID);
    App.Handle.getStore().addListener('load', stoHandle_Load);
    App.Status.getStore().addListener('load', store_Load);
    App.cboTransInvtID.getStore().addListener('load', store_Load);
    App.FromToSiteID.getStore().addListener('load', store_Load);
    App.SlsperID.getStore().addListener('load', store_Load);
    App.SiteID.getStore().addListener('load', store_Load);
    App.ReasonCD.getStore().addListener('load', store_Load);
    App.stoInvt = App.cboTransInvtID.getStore();

    HQ.common.showBusy(true, HQ.waitMsg);
}
var frmMain_FieldChange = function  ( item, field, newValue, oldValue ) {
    if (field.key != undefined) {
        return;
    }
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (Object.keys(App.stoBatch.getChangedData()).length > 0 || grdTrans.isChange) {
        setChange(true);
    } else {
        setChange(false);
    }
   
}
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'batch') {
                if (HQ.isChange || grdTrans.isChange) {
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
                if (HQ.isChange || grdTrans.isChange) {
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
                if (HQ.isChange || grdTrans.isChange) {
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
                if (HQ.isChange || grdTrans.isChange) {
                    HQ.message.show(150, '', '', true);
                } else {
                    App.frmMain.loadRecord(App.stoBatch.last());
                }
            } else if (HQ.focus == 'det') {
                HQ.grid.last(App.grdTrans);
            }
            break;
        case "save":
            save();
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
                            if (App.BatNbr.value)
                                HQ.message.show(2015020806, [App.smlTrans.selected.items[0].data.InvtID], 'deleteTrans', true);
                            else {
                                App.grdTrans.deleteSelected();
                                calculate();
                            }
                        }
                    }
                }
            }
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (HQ.isChange || grdTrans.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
        case "new":
            if ((HQ.isChange || grdTrans.isChange) && !Ext.isEmpty(App.BatNbr.getValue())) {
                HQ.message.show(2015030201, '',  "askNew",true);
            } else {
                defaultOnNew();
            }
            break;
        case "refresh":
            if(!Ext.isEmpty(App.BatNbr.getValue())){
                App.TrnsferNbr.store.reload();
                App.stoBatch.reload();
            } else {
                defaultOnNew();
            }
           
            break;
        case "print":
            if (!Ext.isEmpty(App.BatNbr.getValue()) && App.Status.value != "H") {
                report();
            }
            break;
        default:
    }
}

var btnLot_Click = function () {
    if (!Ext.isEmpty(this.record.data.Lot)) {
       
    } else {
      
    }
}
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
            success: function (msg, data) {
                if (this.result.data.lstTrans != undefined) {
                    App.stoTrans.removeAll();
                    this.result.data.lstTrans.forEach(function (item) {
                        var newTrans = Ext.create('App.mdlTrans');
                        newTrans.data.JrnlType = item.JrnlType;
                        newTrans.data.ReasonCD = item.ReasonCD;
                        newTrans.data.TranAmt = item.TranAmt;
                        newTrans.data.BranchID = HQ.cpnyID;
                        newTrans.data.CnvFact = item.CnvFact;
                        newTrans.data.ExtCost = item.TranAmt;
                        newTrans.data.InvtID = item.InvtID;
                        newTrans.data.InvtMult = item.InvtMult;
                        newTrans.data.LineRef = item.LineRef;
                        newTrans.data.Qty = item.Qty;
                        newTrans.data.SiteID = item.SiteID;
                        newTrans.data.TranDate = App.DateEnt.getValue();
                        newTrans.data.TranDesc = item.TranDesc;
                        newTrans.data.TranType = item.TranType;
                        newTrans.data.UnitCost = item.UnitCost;
                        newTrans.data.UnitDesc = item.UnitDesc;
                        newTrans.data.UnitMultDiv = item.UnitMultDiv;
                        newTrans.data.UnitPrice = item.UnitPrice;
                        newTrans.data.UnitCost = item.UnitCost;
                        newTrans.commit();
                        HQ.store.insertRecord(App.stoTrans, 'InvtID', newTrans, true);
                    });
                    HQ.store.insertRecord(App.stoTrans, "InvtID", Ext.create('App.mdlTrans'), true);
                    calculate();
                    checkTransAdd();

                    if (!Ext.isEmpty(this.result.data.message)) {
                        HQ.message.show('2013103001', [this.result.data.message], '', true);
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
}

var btnExport_Click = function () {
    if (Ext.isEmpty(App.BatNbr.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('batnbr')], '', true);
        return;
    }
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
var cboBatNbr_Change = function (item, newValue, oldValue) {
    var record = App.stoBatch.getById(newValue);
    if (record) {
        //showMask();
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
}
var cboFromToSiteID_Change = function () {
    if (App.FromToSiteID.getValue() == App.SiteID.getValue()) {
        App.FromToSiteID.setValue('');
    }
}
var cboSiteID_Change = function () {
    if (App.FromToSiteID.getValue() == App.SiteID.getValue()) {
        App.SiteID.setValue('');
    }
}
var cboTrnsferNbr_Change = function () {
    if (Ext.isEmpty(App.BatNbr.getValue())) {
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
                        if(Ext.isEmpty(App.ReasonCD.getValue())){
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
}

var grdTrans_BeforeEdit = function (item, e) {

    if (App.grdTrans.isLock) {
        return false;
    }

    if (!Ext.isEmpty(HQ.store.findInStore(App.TrnsferNbr.store, ['TrnsfrDocNbr'], [App.TrnsferNbr.getValue()]))) {
        return false;
    }


    if (Ext.isEmpty(App.SiteID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('SiteID')], '', true);
        return false;
    }

    if (Ext.isEmpty(App.SlsperID.getValue())) {
        HQ.message.show(1000, [HQ.common.getLang('SlsperID')], '', true);
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
        e.record.data.TranType = 'RC';
        e.record.data.JrnlType = 'IN';
        e.record.data.BranchID = HQ.cpnyID;
        e.record.data.BatNbr = HQ.objBatch.data.BatNbr;
        e.record.data.TranDate = App.DateEnt.getValue();
        e.record.data.SiteID = App.SiteID.getValue();
        e.record.commit();
    }

    if (key == 'UnitPrice') {
        var invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]);
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
                params: { siteID: App.SiteID.getValue(), invtID: selected[0].data.InvtID },
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
        grdTrans.isChange = true;
        var invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [e.record.data.InvtID]);
        if (!Ext.isEmpty(invt)) {
            if (invt.ValMthd == 'A' || invt.ValMthd == 'E') {
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
                    App.grdTrans.view.loadMask.show();
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
                        params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.DateEnt.getValue() }, callback: checkSourceEdit, row: e
                    });
                } else if (key == 'UnitDesc') {
                    App.grdTrans.view.loadMask.show();
                    HQ.numTrans = 0;
                    HQ.maxTrans = 1;
                    App.stoPrice.load({
                        params: { uom: e.record.data.UnitDesc, invtID: e.record.data.InvtID, effDate: App.DateEnt.getValue() }, callback: checkSourceEdit, row: e
                    });
                } else {
                    checkExitEdit(e);
                }
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

    App.TrnsferNbr.setReadOnly(!Ext.isEmpty(App.BatNbr.getValue()));
    checkTransAdd();
    calculate();
    App.grdTrans.isChange = false;
    HQ.common.showBusy(false, HQ.waitMsg);
    setChange(false);
}
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
    App.stoTrans.reload();
    App.Handle.setValue('N');
}

var save = function () {
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
    if (App.stoTrans.data.items.length <= 1) {
        HQ.message.show(2015020804, [App.BatNbr.value], '', true);
        return;
    }
    var flat = false;
    App.stoTrans.data.each(function (item) {
        if (!Ext.isEmpty(item.data.InvtID)) {
            if (item.data.Qty == 0) {
                HQ.message.show(1000, [HQ.common.getLang('qty')], '', true);
                App.smlTrans.select(App.stoTrans.indexOf(HQ.store.findInStore(App.stoTrans,['LineRef'],[item.data.LineRef])));
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
            url: 'IN10100/Save',
            timeout: 180000,
            params: {
                lstTrans: Ext.encode(App.stoTrans.getRecordsValues()),
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
                    App.stoBatch.remove(App.stoBatch.getById(App.BatNbr.getValue()));
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
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            clientValidation: false,
            method: 'POST',
            url: 'IN10100/DeleteTrans',
            timeout: 180000,
            params: {
                lineRef: App.smlTrans.selected.items[0].data.LineRef,
            },
            success: function (msg, data) {
                App.grdTrans.deleteSelected();
                calculate();
                HQ.message.process(msg, data, true);
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var report = function () {
    App.frmMain.submit({
        waitMsg: HQ.waitMsg,
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
var calculate = function () {
    var totAmt = 0;
    var totQty = 0;

    App.stoTrans.data.each(function (item) {
        totAmt += item.data.TranAmt;
        totQty += item.data.Qty;
    });

    App.TotAmt.setValue(totAmt);
    App.TotQty.setValue(totQty);

}

var defaultOnNew = function () {
    var record = Ext.create('App.mdlBatch');
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'H';
    record.data.DateEnt = HQ.businessDate;
    App.SiteID.setValue('');
    App.SlsperID.setValue('');
    App.TrnsferNbr.forceSelection = true;
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
        if (HQ.objBatch.data.JrnlType == 'PO') {
            lock = true;
        } else if (HQ.objBatch.data.Status == 'H') {
            lock = false;
        }
        App.btnImport.setDisabled(true);
    } else {
        lock = !HQ.isInsert;
        App.btnImport.setDisabled(false);
    }

    HQ.common.lockItem(App.frmMain, lock);
    App.grdTrans.isLock = lock;
    App.BatNbr.setReadOnly(false);
    App.Handle.setReadOnly(false);
    App.RvdBatNbr.setReadOnly(true);
    App.Status.setReadOnly(true);
    App.RefNbr.setReadOnly(true);
    App.TotQty.setReadOnly(true);
    App.TotAmt.setReadOnly(true);
}

var checkExitEdit = function (row) {
    var key = row.field;
    var trans = row.record.data;
    if (key == 'InvtID' || key == 'BarCode'){
        
        trans.ReasonCD = App.ReasonCD.getValue();
        trans.SiteID = App.SiteID.getValue();

        var invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [trans.InvtID]);
        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);
        if (Ext.isEmpty(cnv)){
            trans.UnitMultDiv = '';
            trans.UnitPrice = 0;
            return;
        }

        trans.UnitDesc = invt.StkUnit;
        trans.CnvFact = cnv.CnvFact == 0 ? 1 : cnv.CnvFact;
        trans.UnitMultDiv = cnv.MultDiv;
        trans.TranDesc = invt.Descr;
        trans.BarCode = invt.BarCode;
        trans.UnitPrice = App.stoPrice.data.items[0].data.Price;
        trans.TranAmt = trans.Qty * trans.UnitPrice;

        getQtyAvail(row.record);

    } else if (key == 'UnitDesc') {

        var invt = HQ.store.findInStore(App.stoInvt, ['InvtID'], [trans.InvtID]);

        var cnv = setUOM(invt.InvtID, invt.ClassID, invt.StkUnit, invt.StkUnit);

        if (Ext.isEmpty(cnv)){
            trans.UnitMultDiv = '';
            trans.UnitPrice = 0;
            return;
        }
       
        trans.CnvFact = cnv.CnvFact;
        trans.UnitMultDiv = cnv.MultDiv;
        trans.UnitPrice = App.stoPrice.data.items[0].data.Price;
        trans.TranAmt = trans.Qty * trans.UnitPrice;
    }

    if (key == "Qty" || key == "UnitPrice") {
        trans.TranAmt = trans.Qty * trans.UnitPrice;
    }
    
    trans.ExtCost = trans.TranAmt;
    trans.UnitCost = trans.UnitPrice;
    row.record.commit();

    if (key == 'InvtID' && !Ext.isEmpty(trans.InvtID)) {
        HQ.store.insertRecord(App.stoTrans, key, Ext.create('App.mdlTrans'),true);
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

    App.SiteID.setReadOnly(flat);

    App.SlsperID.setReadOnly(App.Status.getValue() != 'H');

    App.FromToSiteID.setReadOnly(HQ.isTransfer || App.Status.getValue() != 'H');
}

var getQtyAvail = function(row) {
   
    var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [row.data.InvtID, row.data.SiteID]);
    if (!Ext.isEmpty(site)) {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + site.QtyAvail);
    }
    else {
        App.lblQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ":" + 0);
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
        if (!Ext.isEmpty(App.BatNbr.getValue())) {
            App.BatNbr.setReadOnly(true);
        }
        
    } else {
        App.BatNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'IN10100');
}
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////






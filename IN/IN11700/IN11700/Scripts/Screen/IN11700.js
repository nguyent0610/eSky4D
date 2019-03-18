var keys = ['InvtID'];
var fieldsCheckRequire = [];
var fieldsLangCheckRequire = [];
var keys1 = ['ComponentID'];
var fieldsCheckRequire1 = [];
var fieldsLangCheckRequire1 = [];
var _Source = 0;
var _maxSource = 8;
var _isLoadMaster = false;
var _detSource = 0;
var _selBranchID = '';
var _isRejectHeader = false;
var _beginStatus = "H";
var _fileName;
var _ext;
var idTempjs = '';
var _fromDate = '';
var _toDate = '';
HQ.objBatch = null;
var _componentID = '';
var _lineRef = '';
var _batNbr = '';
var _invtID = '';
var _kitID = "";
var _QtySite='';
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {        
        _Source = 0;
        _detSource = 0;
        _lstDel = [];
        App.stoLotTrans4Save.clearData();
        App.stoComponent4Save.clearData();
        App.stoComponent.reload();
        App.stoSite.reload();
        App.stoLotTrans.reload();
        App.stoBatch.reload();
        HQ.common.showBusy(false);
    }
};
var checkStoreLoad = function () {
    HQ.isFirstLoad = false; //sto load cuoi se su dung   
    frmChange();
    _detSource++;
    if (_detSource == 2 || _isLoadMaster) {
        //HQ.common.showBusy(false);
        _isLoadMaster = true;
        if (App.grdSite.selModel.selected.length == 0) {
            //App.slmHeader.select(0);
        }
    }
    
};

var menuClick = function (command) {
    var grd = App.grdSite;
    var keyGrid = keys;
    if (HQ.focus == 'grdComponent') {
        grd = App.grdComponent;
        keyGrid = keys1;
    }
    switch (command) {
        case "first":
            HQ.grid.first(grd);
            break;
        case "prev":
            HQ.grid.prev(grd);
            break;
        case "next":
            HQ.grid.next(grd);
            break;
        case "last":
            HQ.grid.last(grd);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header') {
                    if (HQ.isChange || App.grdSite.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        addnew();
                    }
                }
                else
                {
                    HQ.grid.insert(grd, keyGrid);
                }                  
            }
            break;
        case "delete":
            if (HQ.focus == 'header') {
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
            } else if (HQ.focus == 'grdSite') {
                if ((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != "H") {
                        HQ.message.show(2015020805, [App.cboBatNbr.value], '', true);
                        return;
                    }
                    if (App.slmHeader.selected.items.length != 0) {
                        if (!Ext.isEmpty(App.slmHeader.selected.items[0].data.InvtID)) {
                            HQ.message.show(2015020806, [App.slmHeader.selected.items[0].data.InvtID], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSite, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    var lstLot = App.stoLotTrans4Save.snapshot || App.stoLotTrans4Save.allData || App.stoLotTrans4Save.data;
                    var lstComponent = App.stoComponent4Save.snapshot|| App.stoComponent4Save.allData||  App.stoComponent4Save.data;
                    if (lstComponent != undefined) {
                        for (var i = 0; i < lstComponent.length; i++) {
                            var QtyLoc = 0;
                            for (var j = 0; j < lstLot.length; j++) {
                                if (lstComponent.items[i].data.LineRef == lstLot.items[j].data.INTranLineRef && lstComponent.items[i].data.ComponentID && lstLot.items[j].data.ComponentID) {
                                    QtyLoc += lstLot.items[j].data.Qty;
                                }                              
                            }
                            if (QtyLoc != lstComponent.items[i].data.ComponentQty && lstComponent.items[i].data.LotSerTrack != 'N') {
                                HQ.message.show(2018082711, [lstComponent.items[i].data.ComponentID, HQ.common.getLang("IN11700Component")], '', true);
                                return;
                            }
                        }
                    }
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);
            break;
    }

};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.numDetail = 0;
    HQ.maxDetail = 2;
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    App.cboBranchID.getStore().addListener('load', checkLoad());
    App.cboStatus.getStore().addListener('load', checkLoad());
    App.cboHandle.getStore().addListener('load', checkLoad());
    App.cboSite.getStore().addListener('load', checkLoad());
    App.cboSiteLocation.getStore().addListener('load', checkLoad());
    App.cboSiteTP.getStore().addListener('load', checkLoad());
    App.cboSiteTPLocation.getStore().addListener('load', checkLoad());
    App.cboReason.getStore().addListener('load', checkLoad());
    checkLoad(); // Mới
    App.cboStatus.setValue('H');
    App.txtDateEnd.setValue(HQ.bussinessDate);
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    App.stoKitID = App.cboKitID.getStore();
    App.stoCompo = App.cboComponent.getStore();
    App.cboHandle.setValue('N');
    App.txtDateEnd.setValue(HQ.bussinessDate);
    App.cboBranchID.setValue(HQ.cpnyID);
}

var frmChange = function () {
    HQ.isChange = isChange(App.stoSite) || HQ.store.isChange(App.stoComponent);
    if (HQ.isChange) {
        App.cboBranchID.setReadOnly(true);
        App.cboBatNbr.setReadOnly(true);
    } else {
        App.cboBranchID.setReadOnly(false);
        App.cboBatNbr.setReadOnly(false);
    }
    HQ.common.changeData(HQ.isChange, 'IN11700');//co thay doi du lieu gan * tren tab title header
};
var frmMain_FieldChange = function (item, field, newValue, oldValue) {
    if (field.key != undefined) {
        return;
    }
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (Object.keys(App.stoBatch.getChangedData()).length > 0 || isChange(App.stoSite) || HQ.store.isChange(App.stoComponent)) {
        setChange(true);
    } else {
        setChange(false);
    }
};
var stoBatch_Load = function () {
    var record = App.stoBatch.getById(App.cboBatNbr.getValue());
    if (record) {
        bindBatch(record);
    }
    else
    {
        App.cboSite.store.reload();
        App.cboSiteTP.store.reload();
    }
};
var stoSite_Load = function (sto, records, successful, eOpts) {
    if (App.cboStatus.getValue() != "C")
    {
        if (HQ.isInsert) {
            var record = HQ.store.findRecord(sto, keys, ['', '']);
            if (!record) {
                HQ.store.insertBlank(sto, keys);
            }
        }
    }   
    checkStoreLoad();
    checkSourceDetail();
    App.stoComponent.reload();
};

var stoComponent_Load = function (sto, records, successful, eOpts) {
    if (records != undefined) {
        for (var i = 0; i < sto.data.length; i++) {
            var objComponent = HQ.store.findRecord(App.stoComponent4Save, ['InvtID', 'ComponentID'], [records[i].data.InvtID, records[i].data.ComponentID]);
            if (objComponent == undefined) {
                if (sto.data.items[i].data.LineRef == "") {
                    sto.data.items[i].data.LineRef = lastLineRef();
                }
                sto.data.items[i].data.InvtMult = 1;
                sto.data.items[i].data.JrnlType = 'IN';
                App.stoComponent4Save.add(sto.data.items[i]);
            }

        }
    }    
    //if (App.cboStatus.getValue() != "C")
    //{
    //    if (HQ.isInsert) {
    //        var record = HQ.store.findRecord(sto, keys1, ['']);
    //        if (!record) {
    //            HQ.store.insertBlank(sto, keys1);
    //        }
    //    }
    //}
    HQ.common.showBusy(false);
    //checkStoreLoad();
};
var stoDetail_Load = function (sto, records, successful, eOpts) {
    for (var i = 0; i < sto.data.length; i++) {
        var objLot = HQ.store.findRecord(App.stoLotTrans4Save, ['INTranLineRef', 'ComponentID'], [records[i].data.INTranLineRef, records[i].data.ComponentID]);
        if(objLot==undefined)
        {
            App.stoLotTrans4Save.add(sto.data.items[i]);
        }
        
    }
};
//trước khi load trang busy la dang load data
var sto_BeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var smlComponent_select = function (slm, selRec, idx, eOpts) {

};
var save = function () {
    //p.frmMain.updateRecord();
    App.grdComponent.store.clearFilter();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            method: 'POST',
            url: 'IN11700/Save',
            timeout: 1800000,
            params: {
                lstTrans: Ext.encode(App.stoSite.getRecordsValues()),
                lstLot: Ext.encode(App.stoLotTrans4Save.getRecordsValues()),
                lstComponent: Ext.encode(App.stoComponent4Save.getRecordsValues()),
                lstBatch: Ext.encode(App.cboBatNbr.store.getRecordsValues()),
                lstLotDPBB: Ext.encode(App.stoLotTransDPBB.getRecordsValues())
            },
            success: function (msg, data) {
                var batNbr = '';

                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    var batNbr = this.result.data.batNbr;
                }
                if (!Ext.isEmpty(batNbr)) {
                    App.cboBatNbr.forceSelection = false;
                    //App.cboBatNbr.events['change'].suspend();
                    App.cboBatNbr.getStore().load({
                        callback: function () {
                            if (Ext.isEmpty(App.cboBatNbr.getValue())) {
                                App.cboBatNbr.setValue(batNbr);
                            }                           
                        }
                    });
                    //App.cboBatNbr.events['change'].resume();
                    if (Ext.isEmpty(HQ.recentRecord)) {
                        HQ.recentRecord = batNbr;
                    }
                    App.stoComponent4Save.clearData();
                    App.stoLotTrans4Save.clearData();
                    App.stoLotTrans.reload();
                    refresh('yes');
                }
                App.lblKitQtyAvail.setText("");
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
                url: 'IN11700/Delete',
                timeout: 180000,
                success: function (msg, data) {
                    var record = App.stoBatch.getById(App.cboBatNbr.getValue());
                    if (!Ext.isEmpty(record)) {
                        App.stoBatch.remove(record);
                    }
                    setChange(false);
                    HQ.message.process(msg, data, true);
                    App.stoComponent.clearData();
                    App.grdComponent.view.refresh();
                    App.stoComponent4Save.clearData();
                    App.stoLotTrans4Save.clearData();
                    menuClick('new');                    
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};
var deleteData = function (item) {
    if (item == 'yes') {
        var count = 0;
        var det = App.slmHeader.selected.items[0].data;
        App.stoLotTrans.clearFilter();
        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (det.LineRef == App.stoLotTrans.data.items[i].data.INTranLineRef) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
        for (var i = 0; i < App.stoSite.data.length; i++) {
            if (App.stoSite.data.items[i].data.InvtID != '')
                count++;
        }
        if (count == 1) {
            HQ.message.show(2018062501);
            return false;
        }
        if (App.cboBatNbr.value) {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                clientValidation: false,
                method: 'POST',
                url: 'IN11700/DeleteKitID',
                timeout: 180000,
                params: {
                    lineRef: App.slmHeader.selected.items[0].data.LineRef,
                },
                success: function (msg, data) {
                    if (!Ext.isEmpty(data.result.data.tstamp)) {
                        App.tstamp.setValue(data.result.data.tstamp);
                    }
                    App.grdSite.deleteSelected();
                    HQ.message.process(msg, data, true);
                    HQ.isChange = false;
                    HQ.common.changeData(HQ.isChange, 'IN11700');
                    App.stoBatch.reload();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
        else {
            App.grdSite.deleteSelected();
            App.grdComponent.store.clearData();
            App.grdComponent.view.refresh();
        }
    }
};

var grdSite_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() == "C")
    {
        return false;
    }
    _invtID = e.record.data.InvtID;
    _lineRef = e.record.data.LineRef;
    _batNbr = e.record.data.BatNbr;
    _QtySite = e.record.data.Qty;
    if (!App.frmMain.isValid()) 
    {
        showFieldInvalid(App.frmMain);
        return false;
    }
    if (Ext.isEmpty(e.record.data.LineRef)) {
        e.record.data.LineRef = lastLineRef();
        e.record.data.BranchID = App.cboBranchID.getValue();
        e.record.data.InvtMult = -1;
        e.record.data.TranType = 'AJ';
        e.record.data.JrnlType = 'IN';
        if (HQ.objBatch!=null)
        {
            e.record.data.BatNbr = HQ.objBatch.data.BatNbr;
        }
        e.record.data.TranDate = App.txtDateEnd.getValue();
        e.record.data.SiteID = App.cboSite.getValue();
        e.record.commit();
    }
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};
var grdSite_Edit = function (item, e) {    
    if (e.field == 'InvtID') {
        
        if (App.cboKitID.valueModels != undefined && App.cboKitID.valueModels.length > 0) {
            var objKitID = HQ.store.findInStore(App.cboKitID.store, ['KitID'], [e.value]);
            var cnv = setUOM(e.value, objKitID.ClassID, objKitID.StkUnit, objKitID.StkUnit);
            if (objKitID != undefined) {
                e.record.set('LotSerTrack', objKitID.LotSerTrack);
            }
            if (!Ext.isEmpty(cnv)) {
                e.record.data.CnvFact = cnv.CnvFact == 0 ? 1 : cnv.CnvFact;
                e.record.data.UnitMultDiv = cnv.MultDiv;
                e.record.data.ClassID = objKitID.ClassID;
            } else {
                return;
            }
            e.record.set('BranchID', App.cboBranchID.getValue());
            e.record.set('TranDesc', App.cboKitID.valueModels[0].data.Descr);
            e.record.set('UnitDesc', App.cboKitID.valueModels[0].data.StkUnit);
        } else {
            e.record.set('BranchID', '');
            e.record.set('TranDesc', '');
            e.record.set('UnitDesc', '');
        }
        _selBranchID = e.record.data.InvtID;
        _kitID = e.record.data.InvtID;
        App.stoItemSiteKit.load({
            params: { siteID: App.cboSite.getValue(), invtID: e.record.data.InvtID, whseLoc: App.cboSiteLocation.getValue(), showWhseLoc: HQ.showWhseLoc },
            callback: checkSelect,
            row: e.record
        });
        App.grdComponent.store.clearData();
        if (!e.record.data.IsSelected && (e.record.data.InvtID != "" && e.record.data.InvtID != null)) {
            e.record.set('IsSelected', true);
            App.grdComponent.store.reload();
        } else {
            App.stoComponent.suspendEvents();
            App.stoComponent4Save.data.each(function (item) {
                if (item.data.InvtID == _invtID) {
                    App.stoComponent.data.add(item);
                }
            });
            App.stoComponent.resumeEvents();
            App.grdComponent.view.refresh();
        }
    }
    var grid = App.grdComponent.getSelectionModel();
    var record = grid.getStore().getRange();
    var lstComponent = App.grdComponent.store.snapshot || App.grdComponent.store.allData|| App.grdComponent.store.data;
    if (e.field == 'Qty')
    {
        App.stoComponent.suspendEvents();
        if (e.value > 0) {
            calcLot(e.record);
        }
        for (var i = 0; i < lstComponent.length; i++)
        {
            if (e.record.data.InvtID == lstComponent.items[i].data.InvtID)
            {
                if (e.record.data.Qty > 0) {
                    lstComponent.items[i].set("ComponentQty", (lstComponent.items[i].data.QtyTemp * e.record.data.Qty));
                }
                else {
                    lstComponent.items[i].set("ComponentQty", lstComponent.items[i].data.QtyTemp);
                }
                            
            }     
        }
        
        App.stoComponent.resumeEvents();
        App.grdComponent.view.refresh();
    }
    if (e.field == "Qty")
    {
        
        if(e.record.data.Qty!=_QtySite)
        {
            var lstCompo4Save = App.stoComponent4Save.snapshot || App.stoComponent4Save.allData || App.stoComponent4Save.data;
            var lstLot = App.stoLotTrans4Save.snapshot || App.stoLotTrans4Save.allData || App.stoLotTrans4Save.data;

            if (lstCompo4Save != undefined) {
                for (var i = 0; i < lstCompo4Save.length; i++) {
                    if (lstCompo4Save.items[i].data.InvtID == e.record.data.InvtID) {
                        lstCompo4Save.items[i].set("ComponentQty", lstCompo4Save.items[i].data.QtyTemp * e.record.data.Qty);
                        if (lstLot != undefined) {                           
                            for (var j = lstLot.length; j > 0; j--) {
                                if (lstLot.items[j - 1].data.INTranLineRef == lstCompo4Save.items[i].data.LineRef && lstLot.items[j - 1].data.ComponentID == lstCompo4Save.items[i].data.ComponentID) {
                                    App.stoLotTrans4Save.remove(lstLot.items[j-1]);
                                }                                
                            }
                        }
                    }  
                }
            }           
        }
    }
    
    getQtyAvail(e.record);

    HQ.grid.checkInsertKey(App.grdSite, e, keys);
    checkDPBBAdd();
    frmChange();
};
var grdSite_ValidateEdit = function (item, e) {
    if (e.field == "Qty") {
        var obj = HQ.store.findRecord(App.stoItemSiteKit, ['InvtID', 'SiteID', 'WhseLoc'], [e.record.data.InvtID, App.cboSite.getValue(), App.cboSiteLocation.getValue()]);
        if (obj != undefined) {
            if (obj.data.QtyAvail < e.value) {                
                HQ.message.show(2018082811, [e.record.data.InvtID], '', true);
                return false;
            }
        }
        else {
            HQ.message.show(2018082811, [e.record.data.InvtID], '', true);
            return false;
        }
    }
    return checkValidateEdit(App.grdSite, e, keys);
};
var grdSite_Reject = function (record) {
    HQ.common.showBusy(true, HQ.waitMsg);
    _isRejectHeader = true;
    var storeAll = App.grdComponent.store.snapshot || App.grdComponent.store.allData || App.grdComponent.store.data;
    for (var i = 0; i < storeAll.length; i++) {
        if (storeAll.items[i].data.InvtID == _invtID) {
            if (App.grdComponent.store.snapshot.items[i].data.ComponentQty != App.grdComponent.store.snapshot.items[i].data.QtyTemp)
            {
                App.grdComponent.store.snapshot.items[0].data.ComponentQty = App.grdComponent.store.snapshot.items[i].data.QtyTemp;
            }
        }
    }
    App.grdComponent.view.refresh();
    HQ.common.showBusy(false, HQ.waitMsg);
    HQ.grid.checkReject(record, App.grdSite);
    frmChange();

};
var grdComponent_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() == "C") {
        return false;
    }
    _invtID = e.record.data.InvtID;
    _lineRef = e.record.data.LineRef;
    _batNbr = e.record.data.BatNbr;
    if (!App.frmMain.isValid()) {
        showFieldInvalid(App.frmMain);
        return false;
    }
    if (!HQ.grid.checkBeforeEdit(e, keys1)) return false;
};
var grdComponent_Edit = function (item, e) {
    if (e.field == 'ComponentID') {
        if (App.cboKitID.valueModels != undefined && App.cboKitID.valueModels.length > 0) {
            e.record.set('BranchID', App.cboBranchID.getValue());
            e.record.set('TranDesc', App.cboKitID.valueModels[0].data.Descr);
            e.record.set('UnitDesc', App.cboKitID.valueModels[0].data.StkUnit);
        } else {
            e.record.set('BranchID', '');
            e.record.set('TranDesc', '');
            e.record.set('UnitDesc', '');
        }
        _selBranchID = e.record.data.InvtID;
    }
    HQ.grid.checkInsertKey(App.grdComponent, e, keys1);
    frmChange();
};
var grdComponent_ValidateEdit = function (item, e) {
    return checkValidateEdit(App.grdComponent, e, keys1);
};
var grdComponent_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdComponent);
    frmChange();
};
var grdLotDPBB_BeforeEdit = function (item, e) {
    if (App.grdLotDPBB.isLock) {
        return false;
    }

    var key = e.field;
    var record = e.record;
    if (key != 'LotSerNbr' && Ext.isEmpty(e.record.data.LotSerNbr)) return false;
    if (key == 'LotSerNbr' && !Ext.isEmpty(record.data.LotSerNbr)) return false;


    if (Ext.isEmpty(record.data.InvtID)) {
        record.data.InvtID = App.winLotDPBB.record.data.InvtID;
        record.data.SiteID = App.winLotDPBB.record.data.SiteID;
    }

    record.commit();
    HQ.common.showBusy(false);
};
var grdLotDPBB_Edit = function (item, e) {
    var key = e.field;
    var lot = e.record.data;
    var record = e.record;
    if (Object.keys(e.record.modified).length > 0) {
        if (key == "Qty" || key == "UnitDesc") {
            checkExitEditLotDPBB(e);
        } else if (key == "LotSerNbr") {
            App.grdLotDPBB.view.loadMask.show();
            HQ.numLot = 0;
            HQ.maxLot = 1;
            App.stoItemLotDPBB.load({
                params: { siteID: lot.SiteID, invtID: lot.InvtID, branchID: App.cboBranchID.getValue(), lotSerNbr: lot.LotSerNbr, whseLoc: App.cboSiteLocation.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSourceEditLotDPBB,
                row: e
            });
        }
    }
};
var grdLotDPBB_SelectionChange = function (item, selected) {
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectLot = 0;
            HQ.maxSelectLot = 1;
            HQ.common.showBusy(true, 'Process...');
            App.stoItemLotDPBB.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.cboBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, batNbr: App.cboBatNbr.getValue(), whseLoc: App.cboSiteLocation.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSelectLotDPBB,
                row: selected[0]
            });
        } else {
            App.lblLotDPBBQtyAvail.setText('');
        }
    }
    HQ.common.showBusy(false);
};
var grdLot_Edit = function (item, e) {
    App.grdLot.view.refresh();
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
                params: { siteID: App.cboSiteTP.getValue(), invtID: _componentID, branchID: App.cboBranchID.getValue(), lotSerNbr: lot.LotSerNbr, batNbr: App.cboBatNbr.getValue(), whseLoc: App.cboSiteTPLocation.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSourceEditLot,
                row: e
            });
        }
    }
    if (e.field == 'LotSerNbr') {
        if (App.cboLotSerNbr.valueModels != undefined && App.cboLotSerNbr.valueModels.length > 0) {
            e.record.set('ExpDate', App.cboLotSerNbr.valueModels[0].data.ExpDate);
            e.record.set('ComponentID', App.winLot.record.data.ComponentID);
            e.record.set('INTranLineRef', App.winLot.record.data.LineRef);
        } else {
            if (e.value == "") {
                e.record.set('ExpDate', '');
                e.record.set('ComponentID', '');
                e.record.set('INTranLineRef', '');
            }
        }
        _selBranchID = e.record.data.InvtID;
    }
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
                params: { siteID: App.cboSiteTP.getValue(), invtID: selected[0].data.ComponentID, branchID: App.cboBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, batNbr: App.cboBatNbr.getValue(), whseLoc: App.cboSiteTPLocation.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSelectLot,
                row: selected[0]
            });
        } else {
            App.lblLotQtyAvail.setText('');
        }
    }
};
var btnLotOK_Click = function () {
    if (!App.grdLot.isLock) {
        var det = App.winLot.record.data;
        var flat = null;
        App.stoLotTrans.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                if (Ext.isEmpty(item.data.UnitDesc)) {
                    HQ.message.show(1000, [HQ.common.getLang('unit')], '', true);
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

        var lineQty = (det.UnitMultDiv == "M" ? qty / det.CnvFact : det.ComponentQty * det.CnvFact)

        App.winLot.record.data.Qty = Math.round(lineQty);
        App.winLot.record.data.TranAmt = App.winLot.record.data.Qty * App.winLot.record.data.UnitPrice;
        App.winLot.record.commit();

        App.grdComponent.view.refresh();
        for (i = App.stoLotTrans.data.items.length - 1; i >= 0; i--) {
            if (Ext.isEmpty(App.stoLotTrans.data.items[i].data.LotSerNbr)) {
                App.stoLotTrans.data.removeAt(i);
            }
        }
        for (var i = 0; i < App.stoLotTrans.data.length; i++) {
            var objFind=App.stoLotTrans4Save.findRecord(["LotSerNbr", "ComponentID", "LineRef"], [App.stoLotTrans.data.items[i].data.LotSerNbr, App.stoLotTrans.data.items[i].data.ComponentID, App.stoLotTrans.data.items[i].data.LineRef]);
            if (objFind==undefined)
            {
                App.stoLotTrans4Save.add(App.stoLotTrans.data.items[i]);
            }
            else
            {
                objFind = App.stoLotTrans.data.items[i].data.Qty;
            }
        }
        
    }

    var lstLot = App.grdLot.store.data;
    if(lstLot.items[0]!=undefined)
    {
        var QtyLoc = 0;
        var checkQtyLot = false;
        for(var i=0;i<lstLot.length;i++)
        {
            QtyLoc += lstLot.items[i].data.Qty;
        }
        var objRecordComponent = HQ.store.findRecord(App.stoComponent4Save, ["LineRef", "ComponentID"], [lstLot.items[0].data.INTranLineRef, lstLot.items[0].data.ComponentID]);
        if(objRecordComponent.data.ComponentQty!=QtyLoc)
        {
            HQ.message.show(2018060760, [HQ.common.getLang("IN11700Component")], '', true);
            checkQtyLot = true;
        }
        if (checkQtyLot == false) {
            App.winLot.hide();
        }
    }
    else {
        HQ.message.show(2018060760, [HQ.common.getLang("IN11700Component")], '', true);
    }
};
var btnLotDPBBDel_Click = function () {
    if ((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) {
        if (App.cboStatus.getValue() != "H") {
            HQ.message.show(2015020805, [App.cboBatNbr.value], '', true);
            return;
        }
        if (App.smlLotDPBB.selected.items.length != 0) {
            if (!Ext.isEmpty(App.smlLotDPBB.selected.items[0].data.LotSerNbr)) {
                HQ.message.show(2015020806, [App.smlLotDPBB.selected.items[0].data.InvtID + ' ' + App.smlLotDPBB.selected.items[0].data.LotSerNbr], 'deleteLotDPBB', true);
            }
        }
    }
};
var btnLotDPBBOK_Click = function () {
    if (!App.grdLotDPBB.isLock) {
        var det = App.winLotDPBB.record.data;
        var flat = null;
        App.stoLotTransDPBB.data.each(function (item) {
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
            App.smlLotDPBB.select(App.stoLotTransDPBB.indexOf(flat));
            return;
        }

        var qty = 0;
        App.stoLotTransDPBB.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                if (item.data.SiteID == det.SiteID && item.data.InvtID == det.InvtID && item.data.INTranLineRef == det.LineRef) {
                    qty += item.data.UnitMultDiv == "M" ? item.data.Qty * item.data.CnvFact : item.data.Qty / item.data.CnvFact;
                }
            }

        });

        var lineQty = (det.UnitMultDiv == "M" ? qty / det.CnvFact : det.Qty * det.CnvFact)
        if (lineQty % 1 > 0) {
            App.winLotDPBB.record.data.Qty = qty;
            App.winLotDPBB.record.data.UnitDesc = App.winLotDPBB.record.invt.StkUnit;
            App.winLotDPBB.record.data.UnitRate = 1;
            App.winLotDPBB.record.data.UnitMultDiv = "M";
            if (App.winLotDPBB.record.invt.ValMthd == "A" || App.winLotDPBB.record.invt.ValMthd == "E") {
                var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [det.InvtID, det.SiteID]);
                price = site.AvgCost;
                App.winLotDPBB.record.data.UnitPrice = price;

            } else {
                App.winLotDPBB.record.data.UnitPrice = 0;
            }
        } else {
            App.winLotDPBB.record.data.Qty = Math.round(lineQty);
        }
        App.winLotDPBB.record.data.TranAmt = App.winLotDPBB.record.data.ExtCost = App.winLotDPBB.record.data.Qty * App.winLotDPBB.record.data.UnitPrice;
        App.winLotDPBB.record.commit();

        App.grdSite.view.refresh();


        //calculate();
        App.stoLotTransDPBB.clearFilter();
        for (i = App.stoLotTransDPBB.data.items.length - 1; i >= 0; i--) {
            if (Ext.isEmpty(App.stoLotTransDPBB.data.items[i].data.LotSerNbr)) {
                App.stoLotTransDPBB.data.removeAt(i);
            }
        }
        var grid = App.grdComponent.getSelectionModel();
        var record = grid.getStore().getRange();
        var lstComponent = App.grdComponent.store.data;
        for (var i = 0; i < lstComponent.length; i++) {
            if (det.InvtID == lstComponent.items[i].data.InvtID) {
                record[i].set("ComponentQty", record[i].data.QtyTemp * lineQty);
                App.stoComponent.resumeEvents();
                App.grdComponent.view.refresh();
            }
        }
    }
    App.winLotDPBB.hide();
};
var btnLotDel_Click = function () {
    if ((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) {
        if (App.cboStatus.getValue() != "H") {
            HQ.message.show(2015020805, [App.cboBatNbr.value], '', true);
            return;
        }
        if (App.smlLot.selected.items.length != 0) {
            if (!Ext.isEmpty(App.smlLot.selected.items[0].data.LotSerNbr)) {
                HQ.message.show(2015020806, [App.smlLot.selected.items[0].data.ComponentID + ' ' + App.smlLot.selected.items[0].data.LotSerNbr], 'deleteLot', true);
            }
        }
    }
};
var lastLineRef = function () {
    var numSite = 0;
    var numComponent = 0;
    var store = App.stoComponent;
    var storeSite = App.stoSite;
    var allRecordsSite = storeSite.snapshot || storeSite.allData || storeSite.data;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (item) {
        if (!Ext.isEmpty(item.data.LineRef) && parseInt(item.data.LineRef) > numComponent) {
            numComponent = parseInt(item.data.LineRef);
        }
    });
    allRecordsSite.each(function (item) {
        if (!Ext.isEmpty(item.data.LineRef) && parseInt(item.data.LineRef) > numSite) {
            numSite = parseInt(item.data.LineRef);
        }
    });
    if (numSite > numComponent)
    {
        numSite++;
        var lineRef = numSite.toString();
        var len = lineRef.length;
        for (var i = 0; i < 5 - len; i++) {
            lineRef = "0" + lineRef;
        }
    }
    else
    {
        numComponent++;
        var lineRef = numComponent.toString();
        var len = lineRef.length;
        for (var i = 0; i < 5 - len; i++) {
            lineRef = "0" + lineRef;
        }
    }
    return lineRef;
};

var bindBatch = function (record) {
    //Dung bindTrans IN10100 lay duoc vi tri kho
    HQ.objBatch = record;
    App.cboBatNbr.events['change'].suspend();
    //App.frmMain.events['fieldchange'].suspend();
    App.cboStatus.forceSelection = false;
    App.frmMain.loadRecord(HQ.objBatch);
    //App.frmMain.events['fieldchange'].resume();
    App.cboBatNbr.events['change'].resume();
    if (Ext.isEmpty(record.data.BatNbr)) {
        App.cboSite.setValue("");
        App.cboSite.setReadOnly(false);
        App.cboSiteLocation.setValue("");
        App.cboSiteLocation.setReadOnly(false);
        App.cboSiteTP.setValue("");
        App.cboSiteTP.setReadOnly(false);
        App.cboSiteTPLocation.setValue("");
        App.cboSiteTPLocation.setReadOnly(false);
        App.stoSite.clearData();
    }
    App.stoSite.reload();
    App.stoLotTransDPBB.reload();
    App.stoLotTrans.reload();
    App.stoComponent.reload();
    setStatusForm();
    checkSourceDetail();
    HQ.common.showBusy(true, HQ.waitMsg);
    HQ.numDetail = 0;
    
   App.cboHandle.setValue('N');
};
var cboBatNbr_Change = function (item, newValue, oldValue) {
        var record = App.stoBatch.getById(newValue);
        if (record) {
            HQ.isNew = false;
            bindBatch(record);
        }
        //else {
        //    if (HQ.recentRecord != record) {
        //        App.cboBatNbr.store.reload();
        //    }
        //    else {
        //    }
        //}
        if (App.cboBatNbr.getValue() != null) {
            App.cboReason.setReadOnly(true);
            App.txtDescr.setReadOnly(true);
            App.txtDateEnd.setReadOnly(true);
        }
        else {
            App.cboReason.setReadOnly(false);
            App.txtDescr.setReadOnly(false);
            App.txtDateEnd.setReadOnly(false);
        }
        HQ.recentRecord = record;
        App.stoLotTrans4Save.clearData();
        App.stoComponent4Save.clearData();
        App.stoKitID.reload();
        App.stoComponent.reload();
        App.stoLotTrans.reload();
    
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        if (!Ext.isEmpty(App.cboBatNbr.getValue())) {
            App.stoBatch.reload();
        } else {
            defaultOnNew();
        }
        App.stoLotTrans4Save.clearData();
        App.stoComponent4Save.clearData();
    }
};
function addnew() {
    HQ.isNew = true;
    App.cboBatNbr.setValue('');
    var record = Ext.create('App.mdlBatch');
    App.stoLotTrans4Save.loadData([], false);
    App.stoComponent4Save.loadData([], false);
    record.data.BranchID = HQ.cpnyID;
    record.data.Status = 'H';
    record.data.DateEnt = HQ.bussinessDate;
    if (HQ.showWhseLoc == 0) {
        App.cboWhseLoc.setValue("");
    }
    else {
        App.cboSiteLocation.setValue(HQ.WhseLoc);
    }
    App.frmMain.validate();
    App.grdComponent.store.loadData([], false)
    bindBatch(record);
}
var checkSourceDetail = function (records, options, success) {
    HQ.numDetail++;
    if (HQ.numDetail == HQ.maxDetail) {
        HQ.numDetail = 0;
        bindTran();
    }
};
var renderRowNumberDPBB = function (value, meta, record) {
    return App.stoLotTransDPBB.data.indexOf(record) + 1;
};
var renderRowNumber = function (value, meta, record) {
    return App.stoLotTrans.data.indexOf(record) + 1;
};
var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
};
var setChange = function (isChange) {
    HQ.isChange = isChange;
    if (isChange) {
        App.cboBranchID.setReadOnly(true);
        App.cboBatNbr.setReadOnly(true);
    } else {
        App.cboBranchID.setReadOnly(false);
        App.cboBatNbr.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'IN11700');
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
    App.grdSite.isLock = lock;
    App.cboBatNbr.setReadOnly(false);
   
    if (!HQ.isInsert && HQ.isNew) {
        lock = true;
        flag = false;
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        lock = true;
        flag = false;
    }

    HQ.common.lockItem(App.frmMain, lock);
    //App.grdTrans.isLock = lock;
    if (flag == true && App.cboStatus.getValue() != 'H') {
        App.cboHandle.setReadOnly(false);
    }
};
var bindTran = function () {
    if (App.stoSite.data.items.length > 0) {
        var first = App.stoSite.data.items[0].data;
        App.cboSite.setValue(first.SiteID);
        App.cboSiteLocation.setValue(first.WhseLoc);
        App.cboSiteTP.setValue(first.ToSiteID)
        App.cboSiteTPLocation.setValue(first.ToWhseLoc)
    }
    if (HQ.isInsert)
        HQ.store.insertRecord(App.stoSite, "InvtID", Ext.create('App.IN11700_pgSite'), true);

    HQ.common.showBusy(false, HQ.waitMsg);
    checkDPBBAdd();
    setChange(false);
};
var checkSourceEditLotDPBB = function (records, options, success) {
    HQ.numLot++;
    if (HQ.numLot == HQ.maxLot) {
        checkExitEditLotDPBB(options.row);
    }
};
var checkSourceEditLot = function (records, options, success) {
    HQ.numLot++;
    if (HQ.numLot == HQ.maxLot) {
        checkExitEditLot(options.row);
    }
};
var checkExitEditLotDPBB = function (row) {
    var key = row.field;
    var record = row.record;
    var lot = row.record.data;
    if (key == "Qty") {
        getLotQtyAvailDPBB(record);
    } else if (key == "UnitDesc") {
        var price = 0;
        var cnvFact = 0;
        var unitMultDiv = "";
        var cnv = setUOM(App.winLotDPBB.record.invt.InvtID, App.winLot.record.invt.ClassID, App.winLot.record.invt.StkUnit, lot.UnitDesc);
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
            App.grdLotDPBB.view.loadMask.hide();
            App.grdLotDPBB.view.loadMask.setDisabled(false)
            return;
        }

        if (App.winLotDPBB.record.invt.ValMthd == "A" || App.winLot.record.invt.ValMthd == "E") {
            var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [lot.InvtID, lot.SiteID]);
            price = site.AvgCost * lot.CnvFact;
            lot.UnitPrice = lot.UnitCost = price;

        } else {
            lot.UnitPrice = lot.UnitCost = 0
        }

        getLotQtyAvailDPBB(record);
    } else if (key == "LotSerNbr") {
        var flat = false;
        App.stoLotTransDPBB.data.each(function (item) {
            if (item.data.LotSerNbr == lot.LotSerNbr && item.id != record.id) {
                flat = true;
                return false;
            }
        });
        if (flat) {
            HQ.message.show(219, "", "", true);
            lot.LotSerNbr = "";
            App.grdLotDPBB.view.loadMask.hide();
            App.grdLotDPBB.view.loadMask.setDisabled(false)
            record.commit();
            return;
        }
        lot.UnitDesc = App.winLotDPBB.record.data.UnitDesc;
        lot.UnitPrice = lot.UnitCost = App.winLotDPBB.record.data.UnitPrice;
        lot.UnitMultDiv = App.winLotDPBB.record.data.UnitMultDiv;
        lot.CnvFact = App.winLotDPBB.record.data.CnvFact;
        var itemLot = HQ.store.findInStore(App.stoItemLotDPBB, ['InvtID', 'SiteID', 'LotSerNbr', 'WhseLoc'], [App.winLotDPBB.record.data.InvtID, lot.SiteID, lot.LotSerNbr, App.cboSiteLocation.getValue()]);
        if (!Ext.isEmpty(itemLot)) {
            lot.ExpDate = itemLot.ExpDate;
            lot.WarrantyDate = itemLot.WarrantyDate;
            lot.PercentExpDate = itemLot.PercentExpDate;
        }

        if (!Ext.isEmpty(lot.LotSerNbr)) {
            var newRow = Ext.create('App.mdlLotTrans');
            newRow.data.INTranLineRef = lot.INTranLineRef;
            HQ.store.insertRecord(App.stoLotTransDPBB, key, newRow, true);
        }
        getLotQtyAvailDPBB(record);
    }
    record.commit();
    App.grdLotDPBB.view.loadMask.hide();
    App.grdLotDPBB.view.loadMask.setDisabled(false);
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
        lot.UnitDesc = App.winLot.record.data.Unit;
        lot.UnitPrice = lot.UnitCost = App.winLot.record.data.UnitPrice;
        lot.UnitMultDiv = App.winLot.record.data.UnitMultDiv;
        lot.CnvFact = App.winLot.record.data.CnvFact;
        var itemLot = HQ.store.findInStore(App.stoItemLot, ['ComponentID', 'SiteID', 'LotSerNbr', 'WhseLoc'], [_componentID, lot.SiteID, lot.LotSerNbr, App.cboSiteTPLocation.getValue()]);
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
    record.commit();
    App.grdLot.view.loadMask.hide();
    App.grdLot.view.loadMask.setDisabled(false)
};
var getLotQtyAvailDPBB = function (row) {
    var lot = HQ.store.findInStore(App.stoItemLotDPBB, ['InvtID', 'SiteID', ['LotSerNbr'], 'WhseLoc'], [row.data.InvtID, App.cboSiteTP.getValue(), row.data.LotSerNbr, App.cboSiteLocation.getValue()]);
    var qty = 0;
    var qtyAvail = 0;

    App.stoLotTransDPBB.data.each(function (item2) {
        if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
            qty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
        }
    });
    var det = App.winLotDPBB.record;

    if (!Ext.isEmpty(lot)) {
        if (qty <= 0) {
            qty = Math.abs(qty);
            qtyAvail = HQ.util.mathRound(row.data.UnitMultDiv == "M" ? (lot.QtyAvail - qty) / row.data.CnvFact : (lot.QtyAvail - qty) * row.data.CnvFact, 2);
            if (qtyAvail < 0) {
                HQ.message.show(1043, [row.data.InvtID + " " + row.data.LotSerNbr, row.data.SiteID], "", true);
                qty = 0;
                row.data.Qty = 0;
                row.commit();
                App.stoLotTransDPBB.snapshot.each(function (item2) {
                    if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
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
                App.stoLotTransDPBB.snapshot.each(function (item2) {
                    if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
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
    App.lblLotDPBBQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + qtyAvail + " " + row.data.UnitDesc);

};
var getLotQtyAvail = function (row) {
    var lot = HQ.store.findInStore(App.stoItemLot, ['ComponentID', 'SiteID', ['LotSerNbr'], 'WhseLoc'], [_componentID, App.cboSiteTP.getValue(), row.data.LotSerNbr, App.cboSiteTPLocation.getValue()]);
    var qty = 0;
    var qtyAvail = 0;

    //App.stoLotTrans.snapshot.each(function (item2) {
    //    if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
    //        qty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
    //    }
    //});
    App.stoLotTrans.data.each(function (item2) {
        if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
            qty += item2.data.UnitMultDiv == "M" ? item2.data.Qty * item2.data.CnvFact : item2.data.Qty * item2.data.CnvFact;
        }
    });
    var det = App.winLot.record;

    if (!Ext.isEmpty(lot)) {
        if (qty <= 0) {
            qty = Math.abs(qty);
            qtyAvail = HQ.util.mathRound(row.data.UnitMultDiv == "M" ? (lot.QtyAvail - qty) / row.data.CnvFact : (lot.QtyAvail - qty) * row.data.CnvFact, 2);
            if (qtyAvail < 0) {
                HQ.message.show(1043, [row.data.InvtID + " " + row.data.LotSerNbr, row.data.SiteID], "", true);
                qty = 0;
                row.data.Qty = 0;
                row.commit();
                App.stoLotTrans.snapshot.each(function (item2) {
                    if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
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
                    if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
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
    App.lblLotQtyAvail.setText("Lot " + row.data.LotSerNbr + " - " + HQ.common.getLang('qtyavail') + ": " + qtyAvail + " " + row.data.UnitDesc);

};
var checkSelectLotDPBB = function (records, options, success) {
    HQ.numSelectLot++;
    if (HQ.numSelectLot == HQ.maxSelectLot) {
        App.grdLotDPBB.view.loadMask.hide();
        App.grdLotDPBB.view.loadMask.setDisabled(false)
        getLotQtyAvailDPBB(options.row);
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
var deleteLotDPBB = function (item) {
    if (item == 'yes') {
        App.stoLotTrans4Save.remove(App.grdLotDPBB.selModel.selected.items[0]);
        App.grdLotDPBB.deleteSelected();
    }
};
var deleteLot = function (item) {
    if (item == 'yes') {
        App.stoLotTrans4Save.remove(App.grdLot.selModel.selected.items[0]);
        App.grdLot.deleteSelected();
        var lst = App.grdLot.store.data;
    }
};
var defaultOnNew = function () {
    HQ.isNew = true;
    _kitID = '';
    App.stoLotTrans4Save.loadData([], false);
    var record = Ext.create('App.mdlBatch');
    record.data.Status = 'H';
    record.data.BranchID = HQ.cpnyID;
    record.data.DateEnt = HQ.bussinessDate;
    App.cboSite.setValue('');
    App.cboSiteLocation.setValue('');
    App.cboSiteTP.setValue('');
    App.cboSiteTPLocation.setValue('');
    App.txtDescr.setValue('');
    App.cboReason.setValue('');   
   
    App.frmMain.validate();

    bindBatch(record);
};
///////////////////////////////////
var btnLotDPBB_Click = function () {
    if (Ext.isEmpty(this.record.invt)) {
        var rcInvt = HQ.store.findRecord(App.stoSite, ['InvtID'], [this.record.data.InvtID]);
        this.record.invt = rcInvt.data;
    }
    if (!Ext.isEmpty(this.record.invt.LotSerTrack) && this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.UnitDesc)) {
        showLotDPBB(this.record, true);
    }
};
var btnLot_Click = function () {
    if (Ext.isEmpty(this.record.InvtID)) {
        var rcInvt = HQ.store.findRecord(App.stoCompo, ['ComponentID'], [this.record.data.ComponentID]);
        this.record.invt = rcInvt.data;
    }
    if (!Ext.isEmpty(this.record.invt.LotSerTrack) && this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.Unit)) {
        showLot(this.record, true);
    }
};
var renderStatus = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboStatus.store, ['Code'], [record.data.Status]);
    if (Ext.isEmpty(r))
        return value;
    else
        return r.data.Descr;
};
var renderTerritory = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboTerritory.store, ['Territory'], [record.data.Territory]);
    if (Ext.isEmpty(r))
        return value;
    else
        return r.data.Descr;
};
var stringFilterStatus = function (record) {
    if (this.dataIndex == 'Status') {
        App.cboStatus.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboStatus.store, "Code", "Descr");
    }

    return HQ.grid.filterString(record, this);
}
var calcLot = function (record) {
    if (!Ext.isEmpty(record.data) && !Ext.isEmpty(record.data.LotSerTrack) && record.data.LotSerTrack != 'N' && !Ext.isEmpty(record.data.UnitDesc)) {
        var flat = false;
        var det = record.data;

        App.stoLotTransDPBB.clearFilter();
        App.stoLotTransDPBB.data.each(function (item) {
            if (item.data.INTranLineRef == det.LineRef && !Ext.isEmpty(item.data.LotSerNbr)) {
                flat = true;
            }
        });

        if (!flat) {
            HQ.common.showBusy(true, HQ.waitMsg);
            App.stoCalcLotDPBB.load({
                params: {
                    siteID: det.SiteID,
                    invtID: det.InvtID,
                    branchID: App.cboBranchID.getValue(),
                    batNbr: App.cboBatNbr.getValue(),
                    whseLoc: App.cboSiteLocation.getValue(),
                    showWhseLoc: HQ.showWhseLoc,
                    CnvFact: record.data.CnvFact
                },
                det: record.data,
                row: record,
                callback: function (records, options, success) {

                    var det = options.det;
                    var record = options.row;
                    var needQty = Math.round(det.UnitMultDiv == "M" ? det.Qty : det.Qty);
                    App.stoLotTransDPBB.clearFilter();
                    App.stoCalcLotDPBB.data.each(function (item) {
                        var newQty = 0;
                        var curQty = 0;

                        App.stoLotTransDPBB.data.each(function (item2) {
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
                            var newLot = Ext.create('App.mdlLotTransDPBB');
                            newLot.data.BranchID = App.cboBranchID.getValue();
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
                            App.stoLotTransDPBB.insert(App.stoLotTransDPBB.getCount(), newLot);
                        }



                        if (needQty == 0) return false;
                    });
                    App.stoLotTransDPBB.commitChanges();
                    HQ.common.showBusy(false);
                    showLotDPBB(options.row, false);

                }
            });
        } else {
            App.stoLotTransDPBB.clearFilter();
            App.stoLotTransDPBB.data.each(function (item) {
                if (item.data.INTranLineRef == record.data.LineRef) {
                    item.data.UnitDesc = record.data.UnitDesc;
                    item.data.CnvFact = record.data.CnvFact;
                    item.data.UnitMultDiv = record.data.UnitMultDiv;
                    item.data.QtyOnHand = record.data.QtyOnHand;
                    item.data.UnitCost = item.data.UnitPrice = record.data.UnitPrice;
                    item.commit();
                }
            });
            showLotDPBB(record, true);
        }
    }
};
var showLotDPBB = function (record, loadCombo) {
    var lock = !((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) || App.cboStatus.getValue() != "H";
    App.grdLot.isLock = lock;
    if (loadCombo) {
        App.stoCalcLotDPBB.load({
            params: {
                siteID: App.cboSiteTP.getValue(),
                invtID: record.data.InvtID,
                branchID: App.cboBranchID.getValue(),
                batNbr: App.cboBatNbr.getValue(),
                whseLoc: App.cboSiteLocation.getValue(),
                showWhseLoc: HQ.showWhseLoc,
                CnvFact: record.data.CnvFact
            }
        });
    }

    App.stoLotTransDPBB.clearFilter();
    App.stoLotTransDPBB.filter('INTranLineRef', record.data.LineRef);
    var newRow = Ext.create('App.mdlLotTransDPBB');
    newRow.data.INTranLineRef = record.data.LineRef;
    newRow.data.UnitDesc = record.data.UnitDesc;
    newRow.data.ExpDate = App.txtDateEnd.getValue();
    newRow.data.UnitPrice = record.data.UnitPrice;
    newRow.data.CnvFact = record.data.CnvFact;
    newRow.data.UnitMultDiv = record.data.UnitMultDiv;
    newRow.data.WhseLoc = record.data.WhseLoc;
    newRow.data.WarrantyDate = HQ.businessDate;
    newRow.data.PctExpDate = 0;
    HQ.store.insertRecord(App.stoLotTransDPBB, "LotSerNbr", newRow, true);
    App.winLotDPBB.record = record;
    App.grdLotDPBB.view.refresh();
    App.winLotDPBB.setTitle(record.data.InvtID + ' ' + (record.data.UnitMultDiv == "M" ? record.data.Qty * record.data.CnvFact : record.data.Qty / record.data.CnvFact) + ' ' + record.data.UnitDesc);
    HQ.focus = '';
    App.winLotDPBB.show();
    App.grdLotDPBB.isLock = lock;
    setTimeout(function () { App.winLotDPBB.toFront(); }, 50);
};
var showLot = function (record, loadCombo) {
    var lock = !((App.cboBatNbr.value && HQ.isUpdate) || (!App.cboBatNbr.value && HQ.isInsert)) || App.cboStatus.getValue() != "H";
    App.grdLot.isLock = lock;
    if (loadCombo) {
        App.stoCalcLot.load({
            params: {
                siteID: App.cboSiteTP.getValue(),
                component: record.data.ComponentID,
                branchID: App.cboBranchID.getValue(),
                batNbr: App.cboBatNbr.getValue(),
                whseLoc: App.cboSiteTPLocation.getValue(),
                showWhseLoc: HQ.showWhseLoc
            }
        });
    }

    App.stoLotTrans.clearData();
    _componentID = record.data.ComponentID;
    App.stoLotTrans.suspendEvents();
    App.stoLotTrans4Save.data.each(function (item) {
        if (item.data.ComponentID == _componentID && item.data.INTranLineRef == record.data.LineRef) {
            // App.stoLotTrans.data.add(item);
            App.stoLotTrans.data.add(item);
        }
    });
    if (HQ.isInsert) {
        var objComponentID = HQ.store.findInStore(App.stoComponent4Save, ['InvtID', 'ComponentID'], [record.data.InvtID, record.data.ComponentID]);
        var newRecord = Ext.create('App.mdlLotTrans');
        newRecord.data.INTranLineRef = objComponentID.LineRef;
            HQ.store.insertBlank(App.stoLotTrans, ['LotSerNbr'], newRecord, true);
    }
    App.stoLotTrans.resumeEvents();
    App.grdLot.view.refresh();


    //App.grdLot.store.clearData();
    //if (!record.data.IsSelected) {
    //    record.set('IsSelected', true);
    //    App.grdLot.store.reload();
    //} else {
    //    App.stoLotTrans.suspendEvents();
    //    App.stoLotTrans4Save.data.each(function (item) {
    //        if (item.data.ComponentID == _componentID) {
    //            App.stoLotTrans.data.add(item);
    //        }
    //    });
    //    if (HQ.isInsert) {
    //        var newRecord = HQ.store.findRecord(App.stoLotTrans, ['LotSerNbr'], ['']);
    //        if (!newRecord) {
    //            HQ.store.insertBlank(App.stoLotTrans, ['LotSerNbr']);
    //        }
    //    }
    //    App.stoLotTrans.resumeEvents();
    //    App.grdLot.view.refresh();
    //}


    App.winLot.record = record;
    App.grdLot.view.refresh();
    App.winLot.setTitle(record.data.ComponentID + ' ' + (record.data.UnitMultDiv == "M" ? record.data.ComponentQty * record.data.CnvFact : record.data.ComponentQty / record.data.CnvFact) + ' ' + record.invt.Unit);
    HQ.focus = '';
    App.winLot.show();
    setTimeout(function () { App.winLot.toFront(); }, 50);
};


var isChange = function (store) {
    if ((store.getChangedData().Created != undefined && store.getChangedData().Created.length > 1)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined
       ) {
        return true;
    } else {
        if (store.getChangedData().Created != undefined && store.getChangedData().Created.length == 1) {
            for (var i = 0; i < keys.length; i++) {
                if (store.getChangedData().Created[0][keys[i]] != '' && store.getChangedData().Created[0][keys[i]] != null) {
                    return true;
                }
            }
        }
        return false;
    }

}

var checkValidateEdit = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (e.field != 'BranchID' && e.field != 'EquipmentName') {
            var regex = /^(\w*(\d|[a-zA-Z]|[\_@()+-.]))*$/;
            if (e.value) {
                if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
                    HQ.message.show(2017120509, e.column.text);
                    return false;
                }
            }
        }
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
    }
}

var slmHeader_select = function (slm, selRec, idx, eOpts) {
    _invtID = selRec.data.InvtID;
    _kitID = selRec.data.InvtID;
    App.grdComponent.store.clearData();
    App.stoItemSiteKit.load({
        params: { siteID: App.cboSite.getValue(), invtID: selRec.data.InvtID, whseLoc: App.cboSiteLocation.getValue(), showWhseLoc: HQ.showWhseLoc },
        callback: checkSelect,
        row: selRec
    });
    if (!selRec.data.IsSelected && (selRec.data.InvtID != "" && selRec.data.InvtID != null)) {
        selRec.set('IsSelected', true);
        App.grdComponent.store.reload();
    } else {
        App.stoComponent.suspendEvents();
        App.stoComponent4Save.data.each(function (item) {
            if (item.data.InvtID == _invtID) {
                App.stoComponent.data.add(item);
            }
        });
        App.stoComponent.resumeEvents();
        App.grdComponent.view.refresh();
    }
    if (!Ext.isEmpty(selRec.data.InvtID)) {

    }
    else {
        App.lblKitQtyAvail.setText('');
    }
};

var getQtyAvail = function (row) {
    var qtyOnhand = 0;
    if (!Ext.isEmpty(App.cboSite.getValue()) && !Ext.isEmpty(App.cboSiteLocation.getValue())) {
        var site = HQ.store.findInStore(App.stoItemSiteKit, ['InvtID', 'SiteID', 'WhseLoc'], [row.data.InvtID, App.cboSite.getValue(), App.cboSiteLocation.getValue()]);

        var qty = 0;
        if (!Ext.isEmpty(site)) {
            qty = site.QtyAvail - row.data.Qty; //calculateInvtTotal(row.data.InvtID, row.data.SiteID, row.data.WhseLoc, "");
        }
        else {
            qty = 0 - row.data.Qty;
        }
        qty = qty;//HQ.util.mathRound(row.data.UnitMultDiv == "M" ? qty / row.data.CnvFact : qty * row.data.CnvFact, 2);
        App.lblKitQtyAvail.setText(row.data.InvtID + " - " + HQ.common.getLang('qtyavail') + ": " + qty + " " + row.data.UnitDesc);
    }
    else {
        App.lblKitQtyAvail.setText("");
    }
};

var getQtyAvailTp = function (row) {
    var qtyOnhand = 0;
    var site = HQ.store.findInStore(App.stoInvtComponent, ['InvtID', 'SiteID', 'WhseLoc'], [row.data.ComponentID, App.cboSiteTP.getValue(), App.cboSiteTPLocation.getValue()]);

    var qty = 0;
    if (!Ext.isEmpty(site)) {
        qty = site.QtyAvail ;
    }
    else {
        qty = row.data.ComponentQty;
    }
    qty = qty;
    App.lblKitQtyAvail.setText(row.data.ComponentID + " - " + HQ.common.getLang('qtyavail') + ": " + qty + " " + row.data.Unit);
};

var checkSelect = function (records, options, success) {   
    HQ.common.showBusy(false);
    getQtyAvail(options.row);
};

var checkSelectTP = function (records, options, success) {
    HQ.common.showBusy(false);
    getQtyAvailTp(options.row);
};

var slmDetData_select = function (slm, selRec, idx, eOpts) {
    _componentID = selRec.data.ComponentID;
    App.stoInvtComponent.load({
        params: { siteID: App.cboSiteTP.getValue(), invtID: selRec.data.ComponentID, whseLoc: App.cboSiteTPLocation.getValue(), showWhseLoc: HQ.showWhseLoc },
        callback: checkSelectTP,
        row: selRec
    });

    App.grdLot.store.clearData();
    if (!selRec.data.IsSelected) {
        selRec.set('IsSelected', true);
        App.grdLot.store.reload();
    } else {
        App.stoLotTrans.suspendEvents();
        App.stoLotTrans4Save.data.each(function (item) {
            if (item.data.ComponentID == _invtID && item.data.INTranLineRef == selRec.data.LineRef) {
                App.stoLotTrans.data.add(item);
            }
        });
        App.stoLotTrans.resumeEvents();
        App.grdLot.view.refresh();
    }
    if (Ext.isEmpty(selRec.data.LineRef)) {
        selRec.data.BranchID = App.cboBranchID.getValue();
        selRec.data.InvtMult = 1;
        selRec.data.TranType = 'AJ';
        selRec.data.JrnlType = 'IN';
        if (HQ.objBatch != null) {
            selRec.data.BatNbr = HQ.objBatch.data.BatNbr;
        }
        selRec.data.TranDate = App.txtDateEnd.getValue();
        selRec.data.SiteID = App.cboSite.getValue();
        selRec.commit();
    }
};

var getAllData = function (store) {
    var lstData = [];
    var allData = store.snapshot || store.allData || store.data;
    allData.each(function (item) {
        if (item.data.IMEI != '') {
            lstData.push(item.data);
        }
    });
    return Ext.encode(lstData);    
}

var cboSite_Focus = function () {
    App.cboSite.store.clearFilter();
    var code = "@@@@@";
    if (App.cboBranchID.getValue() != "" && App.cboBranchID.getValue() != null)
    {
        code = App.cboBranchID.getValue();
    }
    App.cboSite.store.filter("CpnyID", code);
    App.cboSite.forceSelection = true;
}
var cboSite_Change = function (item) {
    App.cboSiteLocation.store.clearFilter();
    if (item.hasFocus) {
        if (App.cboSite.valueModels && App.cboSite.valueModels.length > 0) {
            App.cboSiteLocation.setValue(App.cboSite.valueModels[0].data.WhseLoc);
        }
        else {
            App.cboSiteLocation.setValue("");
        }        
    }    
    var code = "@@@@@";
    if (App.cboSite.getValue() != "" && App.cboSite.getValue() != null) {
        code = App.cboSite.getValue();
    }
    App.cboSiteLocation.store.filter("SiteID", new RegExp('^' + Ext.escapeRe(code) + '$'));
}
var cboSiteTP_Focus = function () {
    App.cboSiteTP.store.clearFilter();
    var code = "@@@@@";
    if (App.cboBranchID.getValue() != "" && App.cboBranchID.getValue() != null) {
        code = App.cboBranchID.getValue();
    }
    App.cboSiteTP.store.filter("CpnyID", code);
}
var cboSiteLocation_Focus = function () {
    App.cboSiteLocation.store.clearFilter();
    var code = "@@@@@";
    if (App.cboSite.getValue() != "" && App.cboSite.getValue() != null) {
        code = App.cboSite.getValue();
    }
    App.cboSiteLocation.store.filter("SiteID", new RegExp('^' + Ext.escapeRe(code) + '$'));
}

var cboSiteTP_Change = function (item) {
    if (item.hasFocus) {
        if (App.cboSiteTP.valueModels && App.cboSiteTP.valueModels.length>0) {
            App.cboSiteTPLocation.setValue(App.cboSiteTP.valueModels[0].data.WhseLoc);
        }
        else {
            App.cboSiteTPLocation.setValue("");
        }        
    }
    App.cboSiteTPLocation.store.clearFilter();
    var code = "@@@@@";
    if (App.cboSiteTP.getValue() != "" && App.cboSiteTP.getValue() != null) {
        code = App.cboSiteTP.getValue();
    }
    App.cboSiteTPLocation.store.filter("SiteID", new RegExp('^' + Ext.escapeRe(code) + '$'));
}


var cboSiteLocation_Change = function () {
    App.stoItemSiteKit.load({
        params: { siteID: App.cboSite.getValue(), invtID: '', whseLoc: App.cboSiteLocation.getValue(), showWhseLoc: HQ.showWhseLoc }
    });
}


var cboSiteTPLocation_Focus = function () {
    App.cboSiteTPLocation.store.clearFilter();
    var code = "@@@@@";
    if (App.cboSiteTP.getValue() != "" && App.cboSiteTP.getValue() != null) {
        code = App.cboSiteTP.getValue();
    }
    App.cboSiteTPLocation.store.filter("SiteID", new RegExp('^' + Ext.escapeRe(code) + '$'));
}
var cboBranchID_Change = function () {
    App.stoBatch.reload();
}
var focusOnInvalidField = function (item) {
    if (item == "ok") {
        App.frmMain.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                field.focus();
                return false;
            }
        });
    }
};
var showFieldInvalid = function (form) {
    var done = 1;
    form.getForm().getFields().each(function (field) {
        if (!field.isValid()) {
            HQ.message.show(15, field.fieldLabel, 'focusOnInvalidField');
            done = 0;
            return false;
        }
    });
    return done;
};

var checkSpecialChar = function (value) {
    //var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/;
    //if (!HQ.util.passNull(value.toString()).match(regex))
    //    return false;
    //for (var i = 0, n = value.length; i < n; i++) {
    //    if (value.charCodeAt(i) > 127) {
    //        return false;
    //    }
    //}
    //return true;
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        //var regex = /^(\w*(\d|[a-zA-Z]))[_ \)\(\@-]*$/
        if (value)
            if (!HQ.util.passNull(value) == '' && !HQ.util.passNull(value.toString()).match(regex)) {
                HQ.message.show(20140811, App.cboBudgetID.fieldLabel);
                return false;
            }

        //if (HQ.grid.checkDuplicate(grd, e, keys)) {
        //    HQ.message.show(1112, value);
        //    return false;
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
var checkDPBBAdd = function () {
    debugger
    var flat = false;
    var store = App.stoSite;
    var allRecords = store.allData || store.data;

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

    App.cboSite.setReadOnly(flat);
    App.cboSiteLocation.setReadOnly(flat);
    App.cboSiteTP.setReadOnly(flat);
    App.cboSiteTPLocation.setReadOnly(flat);
};
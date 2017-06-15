HQ.focus = 'header';
var curInvtID = '';
var curUnitDesc = '';
HQ.isChange = false;
var roles = '@ViewBag.Roles'.split(',');
var recentRecord = null;
var forceLoad = false;

// Events // 
var stoBudget_Load = function () {
    if (App.stoBudget.forceLoad) {
        var record = App.stoBudget.getById(App.cboBudgetID.getValue());
        if (record) {
            bindBudget(record);
        } else {
            if (recentRecord != record) {
                defaultOnNew();
            }
        }
    }
    recentRecord = record;
};

var stoHandle_Load = function () {
    App.cboHandle.setValue("N");
};

var stoBudgetFree_Load = function () {
    HQ.numDetailLoad++;
    if (HQ.numDetailLoad == HQ.maxDetailLoad) {
        bindFree();
    }
};

var stoBudgetCust_Load = function () {
    HQ.numDetailLoad++;
    if (HQ.numDetailLoad == HQ.maxDetailLoad) {
        bindFree();
    }
};

var stoBudgetCompany_Load = function () {
    HQ.numDetailLoad++;
    if (HQ.numDetailLoad == HQ.maxDetailLoad) {
        bindFree();
    }
};

var stoData_Load = function () {
    checkSource();
};

var checkSource = function () {
    HQ.numSource++;
    if (HQ.numSource == HQ.maxSource) {
        defaultOnNew();
    }
};

var checkBrandSource = function () {
    HQ.numBrandSource++;
    if (HQ.numBrandSource == HQ.maxBrandSource) {
        HQ.common.showBusy(false);
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                App.frmMain.loadRecord(App.stoBudget.first());
            } else if (HQ.focus == 'company') {
                HQ.grid.first(App.grdBudgetFree);
            } else if (HQ.focus == 'inventory') {
                HQ.grid.first(App.grdBudgetFree);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                var index = App.stoBudget.indexOf(App.stoBudget.getById(App.cboBudgetID.getValue()));
                App.cboBudgetID.setValue(App.stoBudget.getAt(index + 1).get('BudgetID'));
            } else if (HQ.focus == 'company') {
                HQ.grid.next(App.grdBudgetCompany);
            } else if (HQ.focus == 'inventory') {
                HQ.grid.next(App.grdBudgetFree);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                var index = App.stoBudget.indexOf(App.stoBudget.getById(App.PriceID.getValue()));
                App.cboBudgetID.setValue(App.stoBudget.getAt(index - 1).get('BudgetID'));
            } else if (HQ.focus == 'company') {
                HQ.grid.prev(App.grdBudgetCompany);
            } else if (HQ.focus == 'inventory') {
                HQ.grid.prev(App.grdBudgetFree);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                App.frmMain.loadRecord(App.stoBudget.last());
            } else if (HQ.focus == 'company') {
                HQ.grid.last(App.grdBudgetCompany);  
            } else if (HQ.focus == 'inventory') {
                HQ.grid.ast(App.grdBudgetFree);
            }
            break;
        case "save":
            if (HQ.form.checkRequirePass(App.frmMain))
                saveHeader();
            break;
        case "delete":
            if (HQ.focus == 'header') {
                if (!isNewBudget()) {
                    if (HQ.isDelete) {
                        if (App.cboStatus.getValue() != HQ.beginStatus) {
                            return;
                        } else {
                            HQ.message.show(11, '', 'deleteHeader',true);
                        }
                    } else {
                        HQ.message.show(728, '', '', true);
                    }
                } else {
                    menuClick('new');
                }
            } else if (HQ.focus == 'company') {
                if ((!isNewBudget() && HQ.isUpdate) || (isNewBudget() && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != HQ.beginStatus) return;

                    if (App.smlBudgetCompany.selected.items.length != 0) {
                        if (!Ext.isEmpty(App.smlBudgetCompany.selected.items[0].data.CpnyID)) {

                            for (j = App.stoBudgetCust.snapshot.items.length - 1; j >= 0; j--) {
                                if ( App.stoBudgetCust.snapshot.items[j].data.FreeItemID == App.smlBudgetCompany.selected.items[0].data.FreeItemID &&
                                     App.stoBudgetCust.snapshot.items[j].data.CpnyID == App.smlBudgetCompany.selected.items[0].data.CpnyID ) {
                                    App.stoBudgetCust.snapshot.removeAt(j);
                                }
                            }

                            App.grdBudgetCompany.deleteSelected();
                            if (App.cboApplyTo.getValue() != 'F') {
                                var totalAlloc = 0;
                                App.stoBudgetCompany.data.each(function (item) {
                                    totalAlloc += item.data.QtyAmtAlloc;
                                });
                                App.txtQtyAmtAlloc.setValue(totalAlloc);
                                App.txtQtyAmtFree.setValue(App.txtQtyAmtTotal.value - totalAlloc);
                            }
                        }
                    }

                   
                    
                }
            } else if (HQ.focus == 'inventory') {
                if ((!isNewBudget() && HQ.isUpdate) || (isNewBudget() && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != HQ.beginStatus) return;

                    if (App.smlBudgetInvt.selected.items.length != 0) {
                      
                        if (!Ext.isEmpty(App.smlBudgetInvt.selected.items[0].data.FreeItemID)) {
                            for (i = App.stoBudgetCompany.snapshot.items.length - 1; i >= 0; i--) {
                                if (App.stoBudgetCompany.snapshot.items[i].data.FreeItemID == App.smlBudgetInvt.selected.items[0].data.FreeItemID) {
                                    App.stoBudgetCompany.snapshot.removeAt(i);
                                }
                            }

                            for (j = App.stoBudgetCust.snapshot.items.length - 1; j >= 0; j--) {
                                if (App.stoBudgetCust.snapshot.items[j].data.FreeItemID == App.smlBudgetInvt.selected.items[0].data.FreeItemID) {
                                    App.stoBudgetCust.snapshot.removeAt(j);
                                }
                            }

                            App.grdBudgetFree.deleteSelected();
                            var totalAlloc = 0;
                            App.stoBudgetFree.data.each(function (item) {
                                totalAlloc += item.data.QtyAmtAlloc;
                            });
                            App.txtQtyAmtAlloc.setValue(totalAlloc);
                            App.txtQtyAmtFree.setValue(App.txtQtyAmtTotal.value - totalAlloc);
                        }
                    }
                }
            } else if (HQ.focus == 'cust') {
                if ((!isNewBudget() && HQ.isUpdate) || (isNewBudget() && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != HQ.beginStatus) return;

                    if (App.smlCust.selected.items.length != 0) {
                        if (!Ext.isEmpty(App.smlCust.selected.items[0].data.ObjID)) {

                            App.grdCust.deleteSelected();

                        }
                    }
                }
            }
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (Object.keys(App.stoBudget.getChangedData()).length > 0 || grdBudgetFree_IsChange() || grdBudgetCompany_IsChange()) {
                HQ.message.show(5, '', 'closeScreen',true);
            } else {
                this.parentAutoLoadControl.close()
            }
            break;
        case "new":
            if (HQ.isChange) {
                HQ.message.show(2015030201, '', "askNew", true);
            } else {
                //App.cboBudgetID.setValue('');
                defaultOnNew();
            }
            break;
        case "refresh":
            if (HQ.isChange && !App.stoBudget.forceLoad) {
                HQ.message.show(20150303, '', "askRefresh", true);
            } else {
                if (!isNewBudget() || App.stoBudget.forceLoad) {
                    App.stoBudget.forceLoad = true;
                    App.stoBudget.reload();
                } else {
                    defaultOnNew();
                }
            }
            break;
        default:
    }
};

var frmMain_BoxReady = function () {
    HQ.maxDetailLoad = 3;
    HQ.numDetailLoad = 6;

    HQ.numSource = 0;
    HQ.maxSource = 4;

    HQ.numBrandSource = 0;
    HQ.maxBrandSource = 3;
    HQ.common.showBusy(true, HQ.waitMsg);

    App.cboHandle.store.addListener('load', stoHandle_Load);
    App.cboApplyTo.store.addListener('load', stoData_Load);
    App.cboAllocType.store.addListener('load', stoData_Load);
    App.cboStatus.store.addListener('load', stoData_Load);
    App.cboInvtID.store.addListener('load', stoData_Load);
    
    App.cboBudgetID.key = true;

    //App.stoBudgetFree.addListener('beforeload', function () {
    //    if (App.grdBudgetFree.view.loadMask.disable) {
    //        App.grdBudgetFree.view.loadMask.disable();
    //    }
    //});

    //App.stoBudgetCompany.addListener('beforeload', function () {
    //    if (App.grdBudgetCompany.view.loadMask.disable) {
    //        App.grdBudgetCompany.view.loadMask.disable();
    //    }
    //});
    //App.stoBudgetCust.addListener('beforeload', function () {
    //    if (App.grdCust.view.loadMask.disable) {
    //        App.grdCust.view.loadMask.disable();
    //    }
    //});
    App.cboCustID.lastQuery = '';
    App.cboCustID.addListener('beforequery', function () {
        App.cboCustID.lastQuery = '';
        var branchID = App.smlBudgetCompany.selected.items.length > 0 ? App.smlBudgetCompany.selected.items[0].data.CpnyID : "";
        filterComboxEx(
            App.cboCustID,
            'CustID,Name',
            function (record) {
                return record.data.BranchID == branchID
            }
        );
    });

    setLevel(App.pnlHeader, 'header');
};

var frmMain_FieldChange = function (item, field, newValue, oldValue) {
    if (field.key != undefined || !field.submitValue) {
        return;
    }
    if (App.frmMain.getRecord() != null) App.frmMain.updateRecord();

    if (App.frmMain.getRecord().dirty != undefined && App.frmMain.getRecord().dirty == true) {
        setChange(true);
    } else {
        setChange(false);
    }
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoBudgetFree) == false ? (HQ.store.isChange(App.stoBudgetCompany)
                                                    == false ? (HQ.store.isChange(App.stoBudgetCust)) : true) : true;
    HQ.common.changeData(HQ.isChange, 'OM20300');
    if (App.cboBudgetID.valueModels == null || isNewBudget())
        App.cboBudgetID.setReadOnly(false);
    else
        App.cboBudgetID.setReadOnly(HQ.isChange);
};

var txtQtyAmtTotal_Change = function () {
    updateQty();
};

var txtQtyAmtAlloc_Change = function () {
    updateQty();
};

var cboCompany_Change = function (item, newValue, oldValue) {
    if (App.smlBudgetCompany.getSelection().length > 0) {
        var r = '';
        var flat = App.cboCompany.store.data.each(function (record) {
            if (record.get('BranchID') == newValue) {
                r = record;
                return false;
            }
        });
        if (Ext.isEmpty(r)) {
            App.smlBudgetCompany.getSelection()[0].set('CpnyName', "");
            App.smlBudgetCompany.getSelection()[0].set('CpnyID', "");
        }
        else {
            App.smlBudgetCompany.getSelection()[0].set('CpnyName', r.data.BranchName);
            App.smlBudgetCompany.getSelection()[0].set('CpnyID', r.data.BranchID);
        }
    }
};

var cboInvtID_Change = function (item, newValue, oldValue) {
    if (App.smlBudgetInvt.getSelection().length > 0) {
        curInvtID = newValue;
        var r =HQ.store.findInStore(App.cboInvtID.store,['InvtID'],[newValue]);
        if (Ext.isEmpty(r)) {
            App.smlBudgetInvt.getSelection()[0].set('Descr', "");
        }
        else {
            App.smlBudgetInvt.getSelection()[0].set('Descr', r.Descr);
        }
    }
};

var cboBudgetID_Change = function (item, newValue, oldValue) {
    var record = item.getStore().getById(newValue);
    if (record) {
        bindBudget(record);
    } else {
        if (recentRecord != record) {
            defaultOnNew();
        }
    }
    
    recentRecord = record;
};

var cboBudgetID_Select = function (item, newValue, oldValue) {

};

var cboBudgetID_Blur = function (item, newValue, oldValue) {
    
};

var cboAllocType_Change = function (item, newValue, oldValue) {
    if (newValue == '0') {
        App.pnlCust.hide();
        //App.splitCompany2.hide();
    } else {
        App.pnlCust.show();
        //App.splitCompany2.show();
    }
};

var cboApplyTo_Change = function (item, newValue, oldValue) {
    if (newValue == "F") {
        
        //App.splitCompany.setVisible(true);
        //App.splitCompany.updateLayout();
        App.pnlInventory.setVisible(true);
        App.pnlInventory.updateLayout();
    } else {
        //App.splitCompany.setVisible(false);
        //App.splitCompany.updateLayout();
        App.pnlInventory.setVisible(false);
        App.pnlInventory.updateLayout();

        bindCompany();
    }
};

var cboCustID_Change = function (item, newValue, oldValue) {
    if (App.smlCust.selected.items.length > 0) {
        var r = null;
        var flat = App.cboCustID.store.data.each(function (record) {
            if (record.get('CustID') == newValue) {
                r = record;
                return false;
            }
        });
        if (!r) {
            App.smlCust.selected.items[0].set('Descr', "");
            App.smlCust.selected.items[0].set('ObjID', "");
        }
        else {
            App.smlCust.selected.items[0].set('ObjID', r.data.CustID);
            App.smlCust.selected.items[0].set('Descr', r.data.Name);
        }
    }
};

function grdBudgetCompany_IsChange() {
    if (App.grdBudgetCompany.store.getChangedData().Created.length > 1 || App.grdBudgetCompany.store.getChangedData().Updated != undefined || App.grdBudgetCompany.store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};

function grdBudgetFree_IsChange() {
    if (App.grdBudgetFree.store.getChangedData().Created.length > 1 || App.grdBudgetFree.store.getChangedData().Updated != undefined || App.grdBudgetFree.store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};

var grdBudgetCompany_BeforeEdit = function (item, e) {
    HQ.focus = 'company';
    if (App.cboApplyTo.getValue() == 'F' && (App.smlBudgetInvt.getSelection().length == 0 || App.smlBudgetInvt.getSelection()[0].data.FreeItemID == '')) {
        return false;
    }
   
    if (App.cboStatus.getValue() != HQ.beginStatus || (isNewBudget() && !HQ.isInsert) || (!isNewBudget()&& !HQ.isUpdate)) {
        return false;
    }
    if (e.field == 'CpnyID' && !Ext.isEmpty(e.record.data.CpnyID)) {
        return false;
    }
    if (e.field != 'CpnyID' && Ext.isEmpty(e.record.data.CpnyID)) {
        return false;
    }
};

var grdBudgetCompany_ValidateEdit = function (item, e) {
    if (e.field == 'CpnyID') {
        if (e.value) {
            var flat = App.stoBudgetCompany.findBy(function (record, id) {
                if (record.get('CpnyID') == e.record.data.CpnyID && record.id != e.record.id) {
                    return true;
                }
                return false;
            });

            if (flat != -1) {
                e.record.set('CpnyID', "");
                e.record.set('CpnyName', "");
                HQ.message.show(219, [e.value], '',true);
                return false;
            }
        }
    }
    if (e.field == 'QtyAmtAlloc') {
        var FreeItems = App.smlBudgetInvt.selected.items[0] == undefined ? '' : App.smlBudgetInvt.selected.items[0].data.FreeItemID;
        var Company = App.smlBudgetCompany.selected.items[0] == undefined ? '' : App.smlBudgetCompany.selected.items[0].data.CpnyID;

        if (e.record.data.QtyAmtSpent > e.value)
            return false;
        if (App.cboApplyTo.getValue() == 'F') {

            var totalAlloc = 0;
            App.stoBudgetCompany.suspendEvents();
            App.stoBudgetCompany.data.each(function (item) {
                if (item.data.FreeItemID == FreeItems)
                    totalAlloc += item.data.QtyAmtAlloc;
            });
            App.stoBudgetCompany.resumeEvents();

            var totalAllocCust = 0;
            App.stoBudgetCust.suspendEvents();
            App.stoBudgetCust.data.each(function (item) {
                if (item.data.CpnyID == Company)
                    totalAllocCust += item.data.QtyAmtAlloc;
            });
            App.stoBudgetCust.resumeEvents();

            totalAlloc = totalAlloc - e.originalValue + e.value;
            var totalAllocItem = App.smlBudgetInvt.getSelection().length > 0 ? App.smlBudgetInvt.getSelection()[0].data.QtyAmtAlloc : 0;

            if (totalAlloc > totalAllocItem || e.value < totalAllocCust) {
                return false;
            }
        } else {
            var totalAlloc = 0;
            App.stoBudgetCompany.suspendEvents();
            App.stoBudgetCompany.data.each(function (item) {
                if (item.data.FreeItemID == FreeItems)
                    totalAlloc += item.data.QtyAmtAlloc;
            });
            App.stoBudgetCompany.resumeEvents();

            totalAlloc = totalAlloc - e.originalValue + e.value;

            if (totalAlloc > App.txtQtyAmtTotal.getValue()) {
                return false;
            } else {
                if (App.cboApplyTo.getValue() != 'F') {
                    App.txtQtyAmtAlloc.setValue(totalAlloc);
                    App.txtQtyAmtFree.setValue(App.txtQtyAmtTotal.value - totalAlloc);
                }
            }
        }
    }
};

var grdBudgetCompany_Edit = function (item, e) {
    var key = e.field;
    var record = e.record;
    if (key == 'CpnyID') {
        if (e.value) {
            var newRow = Ext.create('App.mdlBudgetCompany');
            HQ.store.insertRecord(App.stoBudgetCompany, key, newRow, true);
            bindCust();
        }
        record.set('FreeItemID', curInvtID);
        record.set('UnitDesc', curUnitDesc);
    }
    
   
    App.cboAllocType.setReadOnly(true);
    App.cboApplyTo.setReadOnly(true);
    frmChange();
};

var grdBudgetCompany_SelectionChange = function (item, selected) {

    if (selected.length > 0) {
        App.cboCustID.store.clearFilter();
        App.cboCustID.store.filter('BranchID', selected[0].data.CpnyID);
    }

    bindCust();
    HQ.focus = 'company';
};

var grdBudgetCust_BeforeEdit = function (item, e) {
    HQ.focus = 'cust';
    if (App.smlBudgetCompany.getSelection().length == 0 || App.smlBudgetCompany.getSelection()[0].data.CpnyID == '') {
        return false;
    }
   
    if (App.cboStatus.getValue() != HQ.beginStatus || (isNewBudget() && !HQ.isInsert) || (!isNewBudget() && !HQ.isUpdate)) {
        return false;
    }
    if (e.field == 'ObjID' && !Ext.isEmpty(e.record.data.ObjID)) {
        return false;
    }
    if (e.field != 'ObjID' && Ext.isEmpty(e.record.data.ObjID)) {
        return false;
    }
};

var grdBudgetCust_ValidateEdit = function (item, e) {
    if (e.field == 'ObjID') {
        if (e.value) {
            var flat = App.stoBudgetCust.findBy(function (record, id) {
                if (record.get('ObjID') == e.record.data.ObjID && record.id != e.record.id) {
                    return true;
                }
                return false;
            });

            if (flat != -1) {
                e.record.set('ObjID', '');
                e.record.set('Descr', '');
                HQ.message.show(219, [e.value], '', true);
                return false;
            }
        }
    }
    if (e.field == 'QtyAmtAlloc') {
        var Company = App.smlBudgetCompany.selected.items[0] == undefined ? '' : App.smlBudgetCompany.selected.items[0].data.CpnyID;
        if (e.record.data.QtyAmtSpent > e.value) {
            return false;
        }
        
        var totalAlloc = 0;
        App.stoBudgetCust.suspendEvents();
        App.stoBudgetCust.data.each(function (item) {
            if (item.data.CpnyID == Company)
                totalAlloc += item.data.QtyAmtAlloc;
        });
        App.stoBudgetCust.resumeEvents();

        totalAlloc = totalAlloc - e.originalValue + e.value;

        var totalAllocItem = App.smlBudgetCompany.getSelection().length > 0 ? App.smlBudgetCompany.getSelection()[0].data.QtyAmtAlloc : 0;

        if (totalAlloc > totalAllocItem) {
            return false;
        } 
    }
};

var grdBudgetCust_Edit = function (item, e) {
    var key = e.field;
    var record = e.record;
    if (key == 'ObjID') {
        if (e.value) {
            var newRow = Ext.create('App.mdlBudgetCust');
            HQ.store.insertRecord(App.stoBudgetCust, key, newRow, true);

            var cpnyID = App.smlBudgetCompany.selected.items[0].data.CpnyID;
            var unitDesc = App.smlBudgetCompany.selected.items[0].data.UnitDesc;
            var freeItemID = App.smlBudgetCompany.selected.items[0].data.FreeItemID;

            record.set('FreeItemID', freeItemID);
            record.set('UnitDesc', unitDesc);
            record.set('CpnyID', cpnyID);
            record.set('QtyAmtAlloc', 0);
        }
    }
    frmChange();
};

var grdBudgetCust_SelectionChange = function (item, selected) {
    HQ.focus = 'cust';
};

var grdBudgetFree_SelectionChange = function (item, selected) {
    if (selected.length > 0) {
        curInvtID = selected[0].data.FreeItemID;
        curUnitDesc = selected[0].data.UnitDesc;
    } else {
        curInvtID = '';
        curUnitDesc = '';
    }
    //if (App.grdBudgetCompany.view.loadMask.enable) {
    //    App.grdBudgetCompany.view.loadMask.enable();
    //}
    bindCompany();
    HQ.focus = 'inventory';
};

var grdBudgetFree_BeforeEdit = function (item, e) {
    HQ.focus = 'inventory';
  
    var key = e.field;
    if (App.cboStatus.getValue() != HQ.beginStatus || (isNewBudget() && !HQ.isInsert) || (!isNewBudget() && !HQ.isUpdate)) {
        return false;
    }
    if (key == 'FreeItemID' && !Ext.isEmpty(e.record.data.FreeItemID)) {
        return false;
    }
    if (key != 'FreeItemID' && Ext.isEmpty(e.record.data.FreeItemID)) {
        return false;
    }
};

var grdBudgetFree_ValidateEdit = function (item, e) {
    var key = e.field;
    if (key == 'FreeItemID') {
        if (e.value) {
            var flat = App.stoBudgetFree.findBy(function (record, id) {
                if (record.get('FreeItemID') == e.value && record.id != e.record.id) {
                    return true;
                }
                return false;
            });

            if (flat != -1) {
                e.record.set('FreeItemID', "");
                e.record.set('Descr', "");
                HQ.message.show(219, [e.value], '' , true);
                return false;
            }
        }
    }
    if (key == 'QtyAmtAlloc') {
        var FreeItems = App.smlBudgetInvt.selected.items[0] == undefined ? '' : App.smlBudgetInvt.selected.items[0].data.FreeItemID;
        if (e.record.data.QtyAmtSpent > e.value)
            return false;

        var totalAlloc = 0;
        App.stoBudgetFree.data.each(function (item) {
            totalAlloc += item.data.QtyAmtAlloc;
        });
        totalAlloc = totalAlloc - e.originalValue + e.value;

        var totalAllocCpny = 0;
        App.stoBudgetCompany.suspendEvents();
        App.stoBudgetCompany.data.each(function (item) {
            if (item.data.FreeItemID == FreeItems)
                totalAllocCpny += item.data.QtyAmtAlloc;
        });
        App.stoBudgetCompany.resumeEvents();

        if (totalAlloc > App.txtQtyAmtTotal.value) {
            return false;
        } else if (totalAllocCpny > e.value) {
            return false;
        } else {
            App.txtQtyAmtAlloc.setValue(totalAlloc);
            App.txtQtyAmtFree.setValue(App.txtQtyAmtTotal.value - totalAlloc);
        }
    }
};

var grdBudgetFree_Edit = function (item, e) {
    var key = e.field;
    var record = e.record;
    if (key == 'FreeItemID') {
        if (e.value) {
            var newRow = Ext.create('App.mdlBudgetFree');
            HQ.store.insertRecord(App.stoBudgetFree, key, newRow, true);

            var objInvt = HQ.store.findInStore(App.cboInvtID.store,['InvtID'],[e.value]);
            if(objInvt){
                record.set('UnitDesc', objInvt.StkUnit);
            }

            curInvtID = e.value;
            curUnitDesc = objInvt ? objInvt.StkUnit : '';
            bindCompany();
        }
    }
    App.cboAllocType.setReadOnly(true);
    App.cboApplyTo.setReadOnly(true);
    frmChange();
};

// Data 
function saveHeader() {
    if ((!isNewBudget() && !HQ.isUpdate) || (isNewBudget() && !HQ.isInsert)) {
        HQ.message.show(728, '', '', true);
        return;
    }
    if (App.cboStatus.getValue() != HQ.beginStatus && (App.cboHandle.getValue() == 'N' || App.cboHandle.getValue()=='')) {
        return;
    }
    var totalAlloc = 0;

    App.stoBudgetFree.data.each(function (item) {
        totalAlloc += item.data.QtyAmtAlloc;
    });

    if (totalAlloc > App.txtQtyAmtTotal.getValue()) {
        HQ.message.show(2014040101, '', '', true);
        return;
    }

    if (App.cboApplyTo.getValue() == 'F') {
        var flat = false;
        App.stoBudgetFree.data.each(function (item) {
            if (item.data.UnitDesc == '' && item.data.FreeItemID != '') {
                flat = true;
                return false;
            }
        });
        if (flat) {
            HQ.message.show(2014040102, '', '', true);
            return;
        }
    }
    App.stoBudgetCompany.suspendEvents();
    App.stoBudgetCompany.clearFilter();
    App.stoBudgetCompany.resumeEvents();

    App.stoBudgetCust.suspendEvents();
    App.stoBudgetCust.clearFilter();
    App.stoBudgetCust.resumeEvents();

    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            method: 'POST',
            timeout: 300000,
            url: 'OM20300/Save',
            params: {
                lstInventory: Ext.encode(App.stoBudgetFree.getRecordsValues()),
                lstCompany: Ext.encode(App.stoBudgetCompany.getRecordsValues()),
                lstAlloc: Ext.encode(App.stoBudgetCust.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.stoBudget.forceLoad = true;
                menuClick('refresh');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

function deleteHeader(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            method: 'POST',
            url: 'OM20300/Delete',
            timeout: 180000,
            clientValidation: false,
            success: function (msg, data) {
                var record = App.stoBudget.getById(App.cboBudgetID.getValue());
                if (!Ext.isEmpty(record)) {
                    App.stoBudget.remove(record);
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
};

var defaultOnNew = function () {
    var record = recentRecord = Ext.create('App.mdlBudget');
    record.data.AllocType = '0';
    record.data.ApplyTo = 'F';
    record.data.Active = true;
    record.data.Status = HQ.beginStatus;
    record.data.RvsdDate = HQ.bussinessDate;
    App.frmMain.validate();
    bindBudget(record);
};

var bindBudget = function (record) {
    HQ.focus = 'header';
    App.stoBudget.forceLoad = false;
   
    App.frmMain.loadRecord(record);
 
    setStatusForm();


    HQ.common.showBusy(true, HQ.waitMsg);
    if (isNewBudget()) {
        App.stoBudgetFree.clearData();
        App.stoBudgetCompany.clearFilter();
        App.stoBudgetCompany.clearData();

        App.stoBudgetCust.clearFilter();
        App.stoBudgetCust.clearData();

        App.grdBudgetFree.view.refresh();

        bindFree();
        HQ.common.showBusy(false);
    } else {
        HQ.numDetailLoad = 0;
        App.stoBudgetFree.reload();
        App.stoBudgetCompany.reload();
        App.stoBudgetCust.reload();

       
        App.cboCompany.store.load({
            params: {
                cpnyID: HQ.cpnyID,
                userID: HQ.userName,
              
            }, callback: function () {
                checkSource();
            }
        });
    }
    App.cboHandle.store.reload();
    setChange(false);
};

var cboBudgetID_Focus = function () {
    if (isNewBudget())
        App.cboBudgetID.forceSelection = false;
    else
        App.cboBudgetID.forceSelection = true;
    
    if (!HQ.isInsert && isNewBudget())
        App.cboBudgetID.forceSelection = true;
};

var bindFree = function () {
    App.stoBudgetFree.loadPage(1);
    HQ.store.insertBlank(App.stoBudgetFree, 'FreeItemID');
    //HQ.store.insertRecord(App.stoBudgetFree, ['FreeItemID'], { BudgetID: App.cboBudgetID.getValue() });
    //App.stoBudgetFree.insert(App.stoBudgetFree.getCount(), Ext.data.Record());
    bindCompany();
};

var bindCompany = function () {
    App.stoBudgetCompany.suspendEvents();
    App.stoBudgetCompany.clearFilter();

    if (App.cboApplyTo.getValue() == 'F') {
        if (curInvtID != '') {
            App.stoBudgetCompany.filter('FreeItemID', curInvtID);
            var recordNew = HQ.store.findInStore(App.stoBudgetCompany, ['FreeItemID', 'CpnyID'], [curInvtID, '']);
            if (!recordNew) {
                //var row = Ext.create('App.mdlBudgetCompany');
                //row.data.FreeItemID = curInvtID;
                //row.data.UnitDesc = curUnitDesc;
                //App.stoBudgetCompany.insert(App.stoBudgetCompany.getCount(), row);
                var newRow = Ext.create('App.mdlBudgetCompany');
                HQ.store.insertRecord(App.stoBudgetCompany, 'CpnyID', newRow, true);
            }
        }
        else {
            App.stoBudgetCompany.filter('FreeItemID', '@#$@');
        }
    } else {
        App.stoBudgetCompany.clearFilter();
        var recordNew = HQ.store.findInStore(App.stoBudgetCompany, ['CpnyID'], ['']);
        if (!recordNew) {
            //var row = Ext.create('App.mdlBudgetCompany');
            //row.data.FreeItemID = '.';
            //App.stoBudgetCompany.insert(App.stoBudgetCompany.getCount(), row);
            var newRow = Ext.create('App.mdlBudgetCompany');
            HQ.store.insertRecord(App.stoBudgetCompany, 'CpnyID', newRow, true);
        }
    }

   
    App.stoBudgetCompany.resumeEvents();
    App.grdBudgetCompany.view.refresh();
    App.stoBudgetCompany.loadPage(1);
    bindCust();
};

var bindCust = function () {
    App.stoBudgetCust.suspendEvents();
    App.stoBudgetCust.clearFilter();

    if (App.cboApplyTo.getValue() == 'F') {
        if (App.smlBudgetCompany.selected.length > 0 && App.smlBudgetCompany.selected.items[0].data.CpnyID) {
            var freeItemID = App.smlBudgetCompany.selected.items[0].data.FreeItemID;
            var cpnyID = App.smlBudgetCompany.selected.items[0].data.CpnyID;
            var unitDesc = App.smlBudgetCompany.selected.items[0].data.UnitDesc;
            //App.stoBudgetCust.filter('FreeItemID', freeItemID);
            //App.stoBudgetCust.filter('CpnyID', cpnyID);

            App.stoBudgetCust.filter([
                { property: "FreeItemID", value: freeItemID },
                { property: "CpnyID", value: cpnyID }
            ]);


            var recordNew = HQ.store.findInStore(App.stoBudgetCust, ['CpnyID', 'FreeItemID', 'ObjID'], [cpnyID, freeItemID, '']);
            if (!recordNew) {
                //var row = Ext.create('App.mdlBudgetCust');
                //row.set('FreeItemID',freeItemID);
                //row.set('CpnyID', cpnyID);
                //row.set('UnitDesc', unitDesc);
                //App.stoBudgetCust.insert(App.stoBudgetCust.getCount(), row);
                var newRow = Ext.create('App.mdlBudgetCust');
                HQ.store.insertRecord(App.stoBudgetCust, 'ObjID', newRow, true);
            }
        }
        else {
            App.stoBudgetCust.filter('FreeItemID', '@#$@');
            App.stoBudgetCust.filter('CpnyID', '@#$@');
        }
    } else {
        App.stoBudgetCust.clearFilter();


        if (App.smlBudgetCompany.selected.items.length > 0 && App.smlBudgetCompany.selected.items[0].data.CpnyID) {
            var cpnyID = App.smlBudgetCompany.selected.items[0].data.CpnyID;
            App.stoBudgetCust.filter('FreeItemID', '');
            App.stoBudgetCust.filter('CpnyID', cpnyID);
            var recordNew = HQ.store.findInStore(App.stoBudgetCust, ['CpnyID', 'ObjID'], [cpnyID, '']);
            if (!recordNew) {
                //var row = Ext.create('App.mdlBudgetCust');
                //row.data.CpnyID = cpnyID;
                //App.stoBudgetCust.insert(App.stoBudgetCust.getCount(), row);
                var newRow = Ext.create('App.mdlBudgetCust');
                HQ.store.insertRecord(App.stoBudgetCust, 'ObjID', newRow, true);
            }
        } else {
            App.stoBudgetCust.filter('CpnyID', '@#$@');
        }

    }
    App.stoBudgetCust.resumeEvents();
    App.grdCust.view.refresh();
    App.stoBudgetCust.loadPage(1);
    HQ.common.showBusy(false);
};

// Functions
var setStatusForm = function () {

    HQ.common.lockItem(App.frmMain, true);

    var lock = true;

    if (isNewBudget())
        lock = !HQ.isInsert;
    else
        lock = !HQ.isUpdate;

    if (App.cboStatus.getValue() != HQ.beginStatus)
        lock = true;

    HQ.common.lockItem(App.frmMain, lock)

    App.cboBudgetID.setReadOnly(false);
    App.cboHandle.setReadOnly(false);
    App.txtQtyAmtAlloc.setReadOnly(true);
    App.txtQtyAmtFree.setReadOnly(true);
    App.cboStatus.setReadOnly(true);

    if (!isNewBudget()) {
        App.cboAllocType.setReadOnly(true);
        App.cboApplyTo.setReadOnly(true);
    }
};

var updateQty = function () {
    App.txtQtyAmtFree.setValue(App.txtQtyAmtTotal.getValue() - App.txtQtyAmtAlloc.getValue());
};

var existsCompany = function () {
    if (App.stoBudgetCompany.data.items.length > 1) {
        return true;
    }
    return false;
};

var isNewBudget = function () {
    return Ext.isEmpty(App.cboBudgetID.displayTplData) || App.cboBudgetID.displayTplData[0].BudgetID == '' ? true : false;
};

var askDelete = function (item) {
    if (item == "yes") {
        if (HQ.focus = 'header') {
            deleteHeader();
        } else if (HQ.focus = 'inventory') {
            App.grdBudgetFree.deleteSelected();
        } else if (HQ.focus = 'company') {
            App.grdBudgetCompany.deleteSelected();
        }
    }
};

var askNew = function (item) {
    if (item == "yes" || item == "ok") {
        defaultOnNew();
    }
};

var askRefresh = function (item) {
    if (item == "yes" || item == "ok") {
        if (!isNewBudget()) {
            App.stoBudget.forceLoad = true;
            App.stoBudget.reload();
        } else {
            defaultOnNew();
        }
    }
};

var setChange = function (isChange) {
    HQ.isChange = isChange;
    if (isChange) {
        if (!isNewBudget()) {
            App.cboBudgetID.setReadOnly(true);
        }

    } else {
        App.cboBudgetID.setReadOnly(false);
    }
    HQ.common.changeData(isChange, 'OM20300');
};

var closeScreen = function (item) {
    if (item == "no") {
        this.parentAutoLoadControl.close()
    }
};

var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
};

function lockForm(isNew) {
    var lock = true;
    if (isNew)
        lock = !HQ.isInsert;
    else
        lock = !HQ.isUpdate;

    if (App.cboStatus.getValue() != beginStatus)
        lock = true;
    HQ.common.lockItem(App.frmMain, lock)
    App.cboBudgetID.setReadOnly(false);
    App.cboHandle.setReadOnly(false);
    if (!isNew) {
        App.cboAllocType.setReadOnly(true);
        App.cboApplyTo.setReadOnly(true);
    }
    App.txtQtyAmtAlloc.setReadOnly(true);
    App.txtQtyAmtFree.setReadOnly(true);
    App.cboStatus.setReadOnly(true);
};

var setfocus = function () {
    HQ.focus = this.level;
};

var setLevel = function (ctr, level) {
    if (typeof (ctr.items) != "undefined") {
        ctr.items.each(function (itm) {
            itm.level = level;
            itm.addListener('focus', setfocus);

            setLevel(itm, level);
        });
    }
};

var filterComboxEx = function (control, stkeyFilter, func) {
    if (control) {
        var store = control.getStore();
        var value = HQ.util.passNull(control.getValue()).toString();
        if (value.split(',').length > 1) value = '';//value.split(',')[value.split(',').length-1];
        if (value.split(';').length > 1) value = '';//value.split(';')[value.split(',').length - 1];
        if (store) {

            store.clearFilter();
            if (control.valueModels == null || control.valueModels.length == 0) {
                store.filterBy(function (record) {
                    if (record) {
                        var isMap = false;
                        stkeyFilter.split(',').forEach(function (key) {
                            if (key) {
                                if ((typeof HQ.util.passNull(value)) == "string") {
                                    if (record.data[key]) {
                                        var fieldData = record.data[key].toString().toLowerCase().indexOf(HQ.util.passNull(value).toLowerCase());
                                        if (fieldData > -1) {
                                            isMap = true;
                                            return record;
                                        }
                                    }
                                }
                            }
                        });
                        if (isMap == true && func(record)) return record
                    }
                });
            }

        }
    }
};
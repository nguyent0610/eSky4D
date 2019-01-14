HQ.focus = 'header';
var curInvtID = '';
var curUnitDesc = '';
HQ.isChange = false;
var roles = '@ViewBag.Roles'.split(',');
var recentRecord = null;
var forceLoad = false;
var _branchID = '';
var checkUnicode = 0;
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
        var record = HQ.store.findInStore(App.stoBudgetFree, ['FreeItemID'], ['']);
        if (!record) {
            HQ.store.insertBlank(App.stoBudgetFree, ['FreeItemID']);
        }
};

var stoBudgetCust_Load = function () {
    var record = HQ.store.findInStore(App.stoBudgetCust, ['CustID'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoBudgetCust, ['CustID']);
    }
};

var stoBudgetCompany_Load = function () {
    var record = HQ.store.findInStore(App.stoBudgetCompany, ['CpnyID'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoBudgetCompany, ['CpnyID']);
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
                            HQ.message.show(2015020806, HQ.common.getLang('OM20300Company') + " " + App.smlBudgetCompany.selected.items[0].data.CpnyID, 'deleteCompany');
                           
                        }
                    }

                   
                    
                }
            } else if (HQ.focus == 'inventory') {
                if ((!isNewBudget() && HQ.isUpdate) || (isNewBudget() && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != HQ.beginStatus) return;

                    if (App.smlBudgetInvt.selected.items.length != 0) {
                        if (!Ext.isEmpty(App.smlBudgetInvt.selected.items[0].data.FreeItemID)) {
                            HQ.message.show(2015020806, HQ.common.getLang('OM20300Inventory') + " " + App.smlBudgetInvt.selected.items[0].data.FreeItemID, 'deleteFreeItem');
                        }
                    }
                }
            } else if (HQ.focus == 'cust') {
                if ((!isNewBudget() && HQ.isUpdate) || (isNewBudget() && HQ.isInsert)) {
                    if (App.cboStatus.getValue() != HQ.beginStatus) return;

                    if (App.smlCust.selected.items.length != 0) {
                        if (!Ext.isEmpty(App.smlCust.selected.items[0].data.ObjID)) {
                            if (App.cboAllocType.getValue() == 2) {
                                HQ.message.show(2015020806, HQ.common.getLang('OM20300Customer') + " " + App.smlCust.selected.items[0].data.ObjID, 'deleteCust');
                               
                            }
                            else if (App.cboAllocType.getValue() == 1) {
                                HQ.message.show(2015020806, HQ.common.getLang('OM20300SalesPerson') + " " + App.smlCust.selected.items[0].data.ObjID, 'deleteCust');
                               
                            }
                           

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

    //App.cboCustID.lastQuery = '';
    //App.cboCustID.addListener('beforequery', function () {
    //    App.cboCustID.lastQuery = '';
    //    var branchID = App.smlBudgetCompany.selected.items.length > 0 ? App.smlBudgetCompany.selected.items[0].data.CpnyID : "";
    //    filterComboxEx(
    //        App.cboCustID,
    //        'CustID,Name',
    //        function (record) {
    //            return record.data.BranchID == branchID
    //        }
    //    );
    //});
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
    HQ.isChange = HQ.store.isChange(App.stoBudgetFree)|| HQ.store.isChange(App.stoBudgetCompany) || HQ.store.isChange(App.stoBudgetCust);
    HQ.common.changeData(HQ.isChange, 'OM20300');
    //if (App.cboBudgetID.valueModels == null || isNewBudget())
    //    App.cboBudgetID.setReadOnly(false);
    //else
        App.cboBudgetID.setReadOnly(HQ.isChange);
};

var txtQtyAmtTotal_Change = function () {
    updateQty();
};

var txtQtyAmtAlloc_Change = function () {
    updateQty();
};
var cboTerritory_Change = function (item, newValue, oldValue) {
    if (App.cboTerritory.valueModels && !App.cboTerritory.loading) {
        App.cboCompany.store.reload();
    }
};
var cboCompany_Expand = function () {
    App.cboCompany.store.reload();
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
    checkUnicode = 0;
    checkSpecialChar(App.cboBudgetID.getValue());
    if (checkUnicode == 1) {
        App.cboBudgetID.setValue('');
        return;
    }
};

var cboAllocType_Change = function (item, newValue, oldValue) {
    if (newValue == '0') {
        App.pnlCust.hide();
    } else {
        App.pnlCust.show();
    }
    if (App.cboAllocType.getValue() == "2") {
        App.pnlCust.setTitle(HQ.common.getLang("OM20300Customer"));
        App.colObjID.setText(HQ.common.getLang("CustID"));
        App.colDescr.setText(HQ.common.getLang("CustName"));
    }
    else if (App.cboAllocType.getValue() == "1") {
        App.pnlCust.setTitle(HQ.common.getLang("OM20300SalesPerson"));
        App.colObjID.setText(HQ.common.getLang("SlsperID"));
        App.colDescr.setText(HQ.common.getLang("OM20300NameSales"));
    }
};

var cboApplyTo_Change = function (item, newValue, oldValue) {
    if (newValue == "F") {
        
        App.pnlInventory.setVisible(true);
        App.pnlInventory.updateLayout();
    } else {
        App.pnlInventory.setVisible(false);
        App.pnlInventory.updateLayout();
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
    _branchID = e.record.data.CpnyID;
    HQ.focus = 'company';
    if (!App.frmMain.isValid()) {
        showFieldInvalid(App.frmMain);
        return false;
    }
    if (App.cboApplyTo.getValue() == 'F' && (App.smlBudgetInvt.getSelection().length == 0 || App.smlBudgetInvt.getSelection()[0].data.FreeItemID == '')) {
        return false;
    }
   
    if (App.cboStatus.getValue() != HQ.beginStatus || (isNewBudget() && !HQ.isInsert) || (!isNewBudget()&& !HQ.isUpdate)) {
        return false;
    }
    if (e.field == 'CpnyID' && !Ext.isEmpty(e.record.data.CpnyID)) {
        return false;
    }
    if (e.field == 'Territory' && !Ext.isEmpty(e.record.data.CpnyID)) {
        return false;
    }
    if (e.field != 'CpnyID' && e.field != 'Territory' && Ext.isEmpty(e.record.data.CpnyID)) {
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
            //else {
            //    App.smlBudgetInvt.getSelection()[0].data.QtyAmtSpent = totalAlloc;
            //    App.grdBudgetFree.view.refresh();
            //}
        } else {
            var totalAlloc = 0;
            App.stoBudgetCompany.suspendEvents();
            App.stoBudgetCompany.data.each(function (item) {              
                    totalAlloc += item.data.QtyAmtAlloc;
            });
            App.stoBudgetCompany.resumeEvents();

            totalAlloc = totalAlloc - e.originalValue + e.value;

            if (totalAlloc > App.txtQtyAmtTotal.getValue()) {
                HQ.message.show(2018102261);
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
            HQ.store.insertRecord(App.stoBudgetCompany, key, newRow, false);
        }
        record.set('FreeItemID', curInvtID);
        record.set('UnitDesc', curUnitDesc);
    }
    else if (key == 'QtyAmtAlloc') {
            e.record.set('QtyAmtAvail', e.value - e.record.data.QtyAmtSpent)        
    }
   
    App.cboAllocType.setReadOnly(true);
    App.cboApplyTo.setReadOnly(true);
    frmChange();
};

var grdBudgetCompany_SelectionChange = function (item, selected) {

    //if (selected.length > 0) {
    //    App.cboCustID.store.clearFilter();
    //    App.cboCustID.store.filter('BranchID', selected[0].data.CpnyID);
    //}

    HQ.focus = 'company';
};

var grdBudgetCust_BeforeEdit = function (item, e) {
    App.cboCustID.store.reload();
    HQ.focus = 'cust';
    if (!App.frmMain.isValid()) {
        showFieldInvalid(App.frmMain);
        return false;
    }
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
        //else
        //{
        //    App.smlBudgetCompany.getSelection()[0].data.QtyAmtSpent = totalAlloc;
        //    App.grdBudgetCompany.view.refresh();
        //}
    }
};

var grdBudgetCust_Edit = function (item, e) {
    var key = e.field;
    var record = e.record;
    if (key == 'ObjID') {
        if (e.value) {
            var newRow = Ext.create('App.mdlBudgetCust');
            HQ.store.insertRecord(App.stoBudgetCust, key, newRow, false);

            var cpnyID = App.smlBudgetCompany.selected.items[0].data.CpnyID;
            var unitDesc = App.smlBudgetCompany.selected.items[0].data.UnitDesc;
            var freeItemID = App.smlBudgetCompany.selected.items[0].data.FreeItemID;

            record.set('FreeItemID', freeItemID);
            record.set('UnitDesc', unitDesc);
            record.set('CpnyID', cpnyID);
            record.set('QtyAmtAlloc', 0);
        }
    } else if (key == 'QtyAmtAlloc') {
        e.record.set('QtyAmtAvail', e.value - e.record.data.QtyAmtSpent)

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
    HQ.focus = 'inventory';
};

var grdBudgetFree_BeforeEdit = function (item, e) {
    HQ.focus = 'inventory';
    if (!App.frmMain.isValid()) {
        showFieldInvalid(App.frmMain);
        return false;
    }
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
            HQ.message.show(2018102261);
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
            HQ.store.insertRecord(App.stoBudgetFree, key, newRow, false);

            var objInvt = HQ.store.findInStore(App.cboInvtID.store,['InvtID'],[e.value]);
            if(objInvt){
                record.set('UnitDesc', objInvt.StkUnit);
            }

            curInvtID = e.value;
            curUnitDesc = objInvt ? objInvt.StkUnit : '';
        }
    }
    else if (key == 'QtyAmtAlloc') {
        e.record.set('QtyAmtAvail', e.value - e.record.data.QtyAmtSpent)
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

    if (App.cboApplyTo.getValue() == "F") {
        var count = 0;
        var lstFreeItem = App.grdBudgetFree.store.snapshot || App.grdBudgetFree.store.allData || App.grdBudgetFree.store.data;
        if (lstFreeItem != undefined) {
            for (var i = 0; i < lstFreeItem.length; i++) {
                if (lstFreeItem.items[i].data.FreeItemID != "") {
                    count++;
                }
            }
        }
        if (count == 0) {
            HQ.message.show(2018102260, [HQ.common.getLang("OM20300Inventory")], '', true);
            return;
        }
    }
    else {
        var count = 0;
        var lstCompany = App.grdBudgetCompany.store.snapshot || App.grdBudgetCompany.store.allData || App.grdBudgetCompany.store.data;
        if (lstCompany != undefined) {
            for (var i = 0; i < lstCompany.length; i++) {
                if (lstCompany.items[i].data.CpnyID != "") {
                    count++;
                }
            }
        }
        if (count == 0) {
            HQ.message.show(2018102260, [HQ.common.getLang("OM20300Company")], '', true);
            return;
        }
    }
    

    if (App.cboAllocType.getValue() == "1" || App.cboAllocType.getValue() == "2") {

        var lstFreeItem = App.grdBudgetFree.store.snapshot || App.grdBudgetFree.store.allData || App.grdBudgetFree.store.data;
        if (lstFreeItem != undefined) {
            for (var i = 0; i < lstFreeItem.length; i++) {
                if (lstFreeItem.items[i].data.FreeItemID != "") {
                    var record = HQ.store.findInStore(App.grdCust.store, ["FreeItemID"], [lstFreeItem.items[i].data.FreeItemID]);
                    if (record == undefined) {
                        HQ.message.show(2018101860, [App.colFreeItemID.text, lstFreeItem.items[i].data.FreeItemID], '', true);
                        return;
                    }
                }
            }
        }


        var lstCompany = App.grdBudgetCompany.store.snapshot || App.grdBudgetCompany.store.allData || App.grdBudgetCompany.store.data;
        if (lstCompany != undefined) {
            for (var i = 0; i < lstCompany.length; i++) {
                if (lstCompany.items[i].data.CpnyID != "") {
                    var record = HQ.store.findInStore(App.grdCust.store, ["CpnyID"], [lstCompany.items[i].data.CpnyID]);
                    if (record == undefined) {
                        HQ.message.show(2018101860, [App.colCpnyID.text, lstCompany.items[i].data.CpnyID], '', true);
                        return;
                    }
                }
            }
        }
    }

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
    //record.data.ApplyTo = 'F';
    record.data.ApplyTo = 'A';
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

        App.stoBudgetFree.reload();
        App.stoBudgetCompany.reload();
        App.stoBudgetCust.reload();
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
        if (App.cboApplyTo.getValue() == "A") {
            App.grdBudgetCompany.selModel.select(0);
        }
        else {
            App.grdBudgetFree.selModel.select(0);
        }
        
        HQ.common.showBusy(false);
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
var btnImport_Click = function (c, e) {
    if (HQ.isInsert || HQ.isUpdate) {
        var files = c.fileInputEl.dom.files;
        var fileName = c.getValue();
        var ext = fileName.split(".").pop().toLowerCase();
        if (ext == "xls" || ext == "xlsx") {
            App.frmMain.submit({
                waitMsg: HQ.waitMsg,
                url: 'OM20300/Import',
                timeout: 1800000,
                clientValidation: false,
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                        App.cboBudgetID.store.reload();
                    }
                    else {
                        HQ.message.process(msg, data, true);
                        App.cboBudgetID.store.reload();
                    }
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
};
var btnExport_Click = function () {
    App.winDetailFileNameExport.show();
};
var btnFileNameExport_Click = function () {
    if (App.chkAmount.getValue()) {
        App.frmMain.submit({
            url: 'OM20300/ExportAmount',
            type: 'POST',
            timeout: 1000000,
            clientValidation: false,
            params: {

            },
            success: function (msg, data) {
                alert('sus');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    if (App.chkQuantity.getValue()) {
        App.frmMain.submit({
            url: 'OM20300/ExportQuantity',
            type: 'POST',
            timeout: 1000000,
            clientValidation: false,
            params: {

            },
            success: function (msg, data) {
                alert('sus');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var btnFileNameCancel_Click = function () {
    App.winDetailFileNameExport.hide();
};
var renderTerritory = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboTerritory.store, ['Territory'], [record.data.Territory])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.Descr;
};
var smlBudgetCompany_Select = function (slm, selRec, idx, eOpts) {
    HQ.focus = 'company';
    _branchID = selRec.data.CpnyID;
    if (App.cboAllocType.getValue() == '1' || App.cboAllocType.getValue() == '2') {
        if (App.cboApplyTo.getValue() == 'A') {
            App.grdCust.getFilterPlugin().clearFilters();
            App.grdCust.getFilterPlugin().getFilter('CpnyID').setValue([selRec.data.CpnyID, '']);
            App.grdCust.getFilterPlugin().getFilter('CpnyID').setActive(true);
            
            if (HQ.isInsert) {
                var record = HQ.store.findRecord(App.stoBudgetCust, ['CpnyID'], ['']);
                if (!record) {
                    HQ.store.insertBlank(App.stoBudgetCust, ['CpnyID']);
                }
            }
        }
        else {
            App.grdCust.getFilterPlugin().clearFilters();
            App.grdCust.getFilterPlugin().getFilter('CpnyID').setValue([selRec.data.CpnyID, '']);
            App.grdCust.getFilterPlugin().getFilter('CpnyID').setActive(true);
            App.grdCust.getFilterPlugin().getFilter('FreeItemID').setValue([selRec.data.FreeItemID, '']);
            App.grdCust.getFilterPlugin().getFilter('FreeItemID').setActive(true);
           
            if (HQ.isInsert) {
                var record = HQ.store.findRecord(App.stoBudgetCust, ['CpnyID','FreeItemID', 'CpnyID'], ['','','']);
                if (!record) {
                    HQ.store.insertBlank(App.stoBudgetCust, ['CpnyID', 'FreeeItemID', 'CpnyID']);
                }
            }
        }
       
    }
    
}
var smlBudgetInvt_Select = function (slm, selRec, idx, eOpts) {
    HQ.focus = 'inventory';
    App.grdBudgetCompany.getFilterPlugin().clearFilters();
    App.grdBudgetCompany.getFilterPlugin().getFilter('FreeItemID').setValue([selRec.data.FreeItemID, '']);
    App.grdBudgetCompany.getFilterPlugin().getFilter('FreeItemID').setActive(true);

    App.grdCust.getFilterPlugin().clearFilters();
    App.grdCust.getFilterPlugin().getFilter('CpnyID').setValue([selRec.data.CpnyID, '']);
    App.grdCust.getFilterPlugin().getFilter('CpnyID').setActive(true);
    App.grdCust.getFilterPlugin().getFilter('FreeItemID').setValue([selRec.data.FreeItemID, '']);
    App.grdCust.getFilterPlugin().getFilter('FreeItemID').setActive(true);

    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoBudgetCompany, ['CpnyID', 'FreeItemID'], ['','']);
        if (!record) {
            HQ.store.insertBlank(App.stoBudgetCompany, ['CpnyID', 'FreeeItemID']);
        }
    }
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
    var regex = /^(\w*(\d|[a-zA-Z]|[\_@()!#$%^&()~`+-=]))*$/;
    if (value)
        if (!HQ.util.passNull(value) == '' && !HQ.util.passNull(value.toString()).match(regex)) {
            HQ.message.show(20140811, App.cboBudgetID.fieldLabel);
            checkUnicode = 1;
            return false;
        }
};
var deleteCust = function (item) {
    if (item == 'yes') {
        App.grdCust.deleteSelected();
    }
};
var deleteFreeItem = function (item) {
    if (item == 'yes') {
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
};
var deleteCompany = function (item) {
    if (item == 'yes') {
        for (j = App.stoBudgetCust.snapshot.items.length - 1; j >= 0; j--) {
            if (App.stoBudgetCust.snapshot.items[j].data.FreeItemID == App.smlBudgetCompany.selected.items[0].data.FreeItemID &&
                 App.stoBudgetCust.snapshot.items[j].data.CpnyID == App.smlBudgetCompany.selected.items[0].data.CpnyID) {
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
var chkAmount_Change = function () {
    if (App.chkAmount.getValue() == true) {
        App.chkQuantity.setValue(false)
    }
}
var chkQuantity_Change = function () {
    if (App.chkQuantity.getValue() == true) {
        App.chkAmount.setValue(false)
    }
}
var txtRvsdDate_Change = function (data, newValue, oldValue) {

    //if (App.txtRvsdDate.getValue() < HQ.bussinessDate) {
    //    HQ.message.show(2018102560);
    //    App.txtRvsdDate.setValue(oldValue);
    //}
}
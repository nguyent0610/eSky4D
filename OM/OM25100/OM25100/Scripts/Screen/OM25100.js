////////////////////////////////////////////////////////////////////////

var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID", "Target"];
var fieldsLangCheckRequire = ["BranchID", "Target"];

var keys1 = ['BranchID', 'ClassID'];
var fieldsCheckRequire1 = ["BranchID", "ClassID", "Target"];
var fieldsLangCheckRequire1 = ["BranchID", "ClassID", "Target"];

var keys2 = ['BranchID', 'InvtID'];
var fieldsCheckRequire2 = ["BranchID", "InvtID", "Target"];
var fieldsLangCheckRequire2 = ["BranchID", "InvtID", "Target"];

var keys3 = ['BranchID', 'SlsperId'];
var fieldsCheckRequire3 = ["BranchID", "SlsperId", "Target"];
var fieldsLangCheckRequire3 = ["BranchID", "SlsperId", "Target"];

var keys4 = ['BranchID', 'SlsperId', 'ClassID'];
var fieldsCheckRequire4 = ["BranchID", "SlsperId", "ClassID", "Target"];
var fieldsLangCheckRequire4 = ["BranchID", "SlsperId", "ClassID", "Target"];

var keys5 = ['BranchID', 'SlsperId', 'InvtID'];
var fieldsCheckRequire5 = ["BranchID", "SlsperId", "InvtID", "Target"];
var fieldsLangCheckRequire5 = ["BranchID", "SlsperId", "InvtID", "Target"];

var keys6 = ['BranchID', 'CustID']; // 'SlsperId', 
var fieldsCheckRequire6 = ["BranchID", 'CustID', "Target"];
var fieldsLangCheckRequire6 = ["BranchID", 'CustID', "Target"];

var keys7 = ['BranchID', 'CustID', 'ClassID']; // "SlsperId",
var fieldsCheckRequire7 = ["BranchID", 'CustID', "ClassID", "Target"];
var fieldsLangCheckRequire7 = ["BranchID", 'CustID', "ClassID", "Target"];

var keys8 = ['BranchID', 'CustID', 'InvtID']; //'SlsperId', 
var fieldsCheckRequire8 = ["BranchID", "CustID", "InvtID", "Target"];
var fieldsLangCheckRequire8 = ["BranchID", "CustID", "InvtID", "Target"];

var keys9 = ['LineRef'];
var fieldsCheckRequire9 = ["LineRef", "Descr"];
var fieldsLangCheckRequire9 = ["LineRef", "Descr"];


var keys10 = ['ClassID']; // grid Class detail
var fieldsCheckRequire10 = ["ClassID"];
var fieldsLangCheckRequire10 = ["ListProdType"];
//// Declare ///////////////////////////////////////////////////////////
var _beginStatus = 'H';

var _Source = 0;
var _maxSource = 8;
var _isLoadMaster = false;
var CycleNbr = '';
var _branchID = '';

var _stoSource = 0;
var _st0MaxSoucre = 11;
/////////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        if (HQ.ShowTabCondition == 0)
            App.tabInfo.child('#itmCondition').tab.hide();
        _isLoadMaster = true;
        _Source = 0;
        App.stoCycleNbr.reload();
        HQ.common.showBusy(true);
    }
};

var checkStoLoad = function () {
    _stoSource += 1;
    if (_stoSource == _st0MaxSoucre) {
        _stoSource = 0;
        HQ.isFirstLoad = false;
    }
};
////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboCycleNbr.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboZone.getStore().addListener('load', checkLoad);
    App.cboKPI.getStore().addListener('load', checkLoad);
    App.cboTerritory.getStore().addListener('load', checkLoad);
    App.cboClassID.getStore().addListener('load', checkLoad);
    App.cboInvtID.getStore().addListener('load', checkLoad);
    App.cboConditionType.getStore().addListener('load', checkLoad);
};

/////////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus=='pnlHeader') {
                HQ.combo.first(App.cboCycleNbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdOM_KPICpny_All') {
                HQ.grid.first(App.grdOM_KPICpny_All);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Class') {
                HQ.grid.first(App.grdOM_KPICpny_Class);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Invt') {
                HQ.grid.first(App.grdOM_KPICpny_Invt);
            }
            else if (HQ.focus == 'grdOM_KPISales_All') {
                HQ.grid.first(App.grdOM_KPISales_All);
            }
            else if (HQ.focus == 'grdOM_KPISales_Class') {
                HQ.grid.first(App.grdOM_KPISales_Class);
            }
            else if (HQ.focus == 'grdOM_KPISales_Invt') {
                HQ.grid.first(App.grdOM_KPISales_Invt);
            }
            else if (HQ.focus == 'grdOM_KPISales_Invt') {
                HQ.grid.first(App.grdOM_KPISales_Invt);
            }
            else if (HQ.focus == 'grdOM_KPICondition') {
                HQ.grid.first(App.grdOM_KPICondition);
            }
            break;
        case "prev":
            if (HQ.focus=='pnlHeader') {
                HQ.combo.prev(App.cboCycleNbr, HQ.isChange);
            }
            else if (HQ.focus=='grdOM_KPICpny_All') {
                HQ.grid.prev(App.grdOM_KPICpny_All);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Class') {
                HQ.grid.prev(App.grdOM_KPICpny_Class);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Invt') {
                HQ.grid.prev(App.grdOM_KPICpny_Invt);
            }
            else if (HQ.focus == 'grdOM_KPISales_All') {
                HQ.grid.prev(App.grdOM_KPISales_All);
            }
            else if (HQ.focus == 'grdOM_KPISales_Class') {
                HQ.grid.prev(App.grdOM_KPISales_Class);
            }
            else if (HQ.focus == 'grdOM_KPISales_Invt') {
                HQ.grid.prev(App.grdOM_KPISales_Invt);
            }
            else if (HQ.focus == 'grdOM_KPICondition') {
                HQ.grid.prev(App.grdOM_KPICondition);
            }
            break;
        case "next":
            if (HQ.focus=='pnlHeader') {
                HQ.combo.next(App.cboCycleNbr, HQ.isChange);
            }
            else if (HQ.focus=='grdOM_KPICpny_All') {
                HQ.grid.next(App.grdOM_KPICpny_All);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Class') {
                HQ.grid.next(App.grdOM_KPICpny_Class);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Invt') {
                HQ.grid.next(App.grdOM_KPICpny_Invt);
            }
            else if (HQ.focus == 'grdOM_KPISales_All') {
                HQ.grid.next(App.grdOM_KPISales_All);
            }
            else if (HQ.focus == 'grdOM_KPISales_Class') {
                HQ.grid.next(App.grdOM_KPISales_Class);
            }
            else if (HQ.focus == 'grdOM_KPISales_Invt') {
                HQ.grid.next(App.grdOM_KPISales_Invt);
            }
            else if (HQ.focus == 'grdOM_KPICondition') {
                HQ.grid.next(App.grdOM_KPICondition);
            }
            break;
        case "last":
            if (HQ.focus=='pnlHeader') {
                HQ.combo.last(App.cboCycleNbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdOM_KPICpny_All') {
                HQ.grid.last(App.grdOM_KPICpny_All);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Class') {
                HQ.grid.last(App.grdOM_KPICpny_Class);
            }
            else if (HQ.focus == 'grdOM_KPICpny_Invt') {
                HQ.grid.last(App.grdOM_KPICpny_Invt);
            }
            else if (HQ.focus == 'grdOM_KPISales_All') {
                HQ.grid.last(App.grdOM_KPISales_All);
            }
            else if (HQ.focus == 'grdOM_KPISales_Class') {
                HQ.grid.last(App.grdOM_KPISales_Class);
            }
            else if (HQ.focus == 'grdOM_KPISales_Invt') {
                HQ.grid.last(App.grdOM_KPISales_Invt);
            }
            else if (HQ.focus == 'grdOM_KPICondition') {
                HQ.grid.last(App.grdOM_KPICondition);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange && HQ.focus == 'pnlHeader') {
                    HQ.message.show(150, '', 'refresh');
                } else {
                    if (HQ.focus == 'pnlHeader') {
                        App.cboCycleNbr.setValue('');
                        App.cboKPI.setValue('');
                        App.dtefromday.setValue('');
                        App.dtetoday.setValue('');
                        App.cboApplyFor.setValue('');
                        App.cboApplyTo.setValue('');
                        App.cboType.setValue('');
                        App.stoCycleNbr.reload();
                    }
                    else if (HQ.focus == 'grdOM_KPICpny_All') {
                        HQ.grid.insert(App.grdOM_KPICpny_All, keys);
                    }
                    else if (HQ.focus == 'grdOM_KPICpny_Class') {
                        HQ.grid.insert(App.grdOM_KPICpny_Class, keys1);
                    }
                    else if (HQ.focus == 'grdOM_KPICpny_Invt') {
                        HQ.grid.insert(App.grdOM_KPICpny_Invt, keys2);
                    }
                    else if (HQ.focus == 'grdOM_KPISales_All') {
                        HQ.grid.insert(App.grdOM_KPISales_All, keys3);
                    }
                    else if (HQ.focus == 'grdOM_KPISales_Class') {
                        HQ.grid.insert(App.grdOM_KPISales_Class, keys4);
                    }
                    else if (HQ.focus == 'grdOM_KPISales_Invt') {
                        HQ.grid.insert(App.grdOM_KPISales_Invt, keys5);
                    }

                    else if (HQ.focus == 'grdOM_KPICustomer_All') {
                        HQ.grid.insert(App.grdOM_KPICustomer_All, keys6);
                    }
                    else if (HQ.focus == 'grdOM_KPICustomer_Class') {
                        HQ.grid.insert(App.grdOM_KPICustomer_Class, keys7);
                    }
                    else if (HQ.focus == 'grdOM_KPICustomer_Invt') {
                        HQ.grid.insert(App.grdOM_KPICustomer_Invt, keys8);
                    }


                    else if (HQ.focus == 'grdOM_KPICondition') {
                        HQ.grid.insert(App.grdOM_KPICondition, keys9);
                    }
                }
            }
            break;
        case "delete":
            if (HQ.isDelete && App.cboStatus.getValue() =="H")
             {
                if (HQ.focus=='pnlHeader') {
                    if (App.cboCycleNbr.getValue()) {
                        HQ.message.show(11, '', 'deleteData');
                    } else {
                        menuClick('new');
                    }
                }
                else if (HQ.focus=='grdOM_KPICpny_All')
                {
                    if (App.slmData.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPICpny_All);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPICpny_All), ''], 'deleteData_Grid', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPICpny_Class') {
                    if (App.slmgrdOMKPICpnyClass.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPICpny_Class);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPICpny_Class), ''], 'deleteData_Grid1', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPICpny_Invt') {
                    if (App.slmgrdOMKPICpnyInvt.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPICpny_Invt);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPICpny_Invt), ''], 'deleteData_Grid2', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPISales_All') {
                    if (App.slmgrdOMKPISales_All.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPISales_All);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPISales_All), ''], 'deleteData_Grid3', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPISales_Class') {
                    if (App.slmgrdOMKPISales_Class.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPISales_Class);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPISales_Class), ''], 'deleteData_Grid4', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPISales_Invt') {
                    if (App.slmgrdOMKPISales_Invt.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPISales_Invt);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPISales_Invt), ''], 'deleteData_Grid5', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPICustomer_All') {
                    if (App.slmgrdOMKPICustomer_All.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPICustomer_All);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPICustomer_All), ''], 'deleteData_Grid6', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPICustomer_Class') {
                    if (App.slmgrdOMKPICustomer_Class.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPICustomer_Class);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPICustomer_Class), ''], 'deleteData_Grid7', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPICustomer_Invt') {
                    if (App.slmgrdOMKPICustomer_Invt.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_KPICustomer_Invt);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_KPICustomer_Invt), ''], 'deleteData_Grid8', true)
                    }
                }
                else if (HQ.focus == 'grdOM_KPICondition') {
                    if (App.slmgrdOM_KPICondition.selected.items[0] != undefined) {
                        var rowindex = '';
                        if (!Ext.isEmpty(App.slmgrdOM_KPICondition.selected.items[0].data.Type)) {
                            for (var i = 0; i < App.slmgrdOM_KPICondition.selected.length; i++) {
                                rowindex += (App.stoOM_KPICondition.indexOf(App.slmgrdOM_KPICondition.selected.items[i]) + 1) + ',';
                            }
                            rowindex = rowindex.substring(0, rowindex.length - 1);
                            HQ.message.show(2015020807, [rowindex, ''], 'deleteData_GridCondition', true);
                        }
                    }
                } else if (HQ.focus == 'grdSalesClassDetail') {
                    if (App.slmSalesClassDetail.selected.items[0] != undefined) {
                        var rowindex = '';
                        if (!Ext.isEmpty(App.slmSalesClassDetail.selected.items[0].data.ClassID)) {
                            for (var i = 0; i < App.slmSalesClassDetail.selected.length; i++) {
                                rowindex += (App.stoSalesClassDetail.indexOf(App.slmSalesClassDetail.selected.items[i]) + 1) + ',';
                            }
                            rowindex = rowindex.substring(0, rowindex.length - 1);
                            HQ.message.show(2015020807, [rowindex, ''], 'deleteData_GridSalesClassDetail', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && App.cboCycleNbr.getStore().findRecord("CycleNbr", App.cboCycleNbr.getValue()) != null)
                {
                    var issave = false;
                    var isError = true;
                    if (App.grdOM_KPICpny_All.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPICpny_All, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {                            
                            if (App.stoOM_KPICpny_All.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPICpny_All.data.items[0].data.BranchID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPICpny_Class.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPICpny_Class, keys1, fieldsCheckRequire1, fieldsLangCheckRequire1)) {
                            if (App.stoOM_KPICpny_Class.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPICpny_Class.data.items[0].data.BranchID)
                                    && !Ext.isEmpty(App.stoOM_KPICpny_Class.data.items[0].data.ClassID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPICpny_Invt.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPICpny_Invt, keys2, fieldsCheckRequire2, fieldsLangCheckRequire2)) {
                            if (App.stoOM_KPICpny_Invt.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPICpny_Invt.data.items[0].data.BranchID)
                                    && !Ext.isEmpty(App.stoOM_KPICpny_Invt.data.items[0].data.InvtID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPISales_All.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPISales_All, keys3, fieldsCheckRequire3, fieldsLangCheckRequire3)) {
                            if (App.stoOM_KPISales_All.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPISales_All.data.items[0].data.BranchID)
                                    && !Ext.isEmpty(App.stoOM_KPISales_All.data.items[0].data.SlsperId)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPISales_Class.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPISales_Class, keys4, fieldsCheckRequire4, fieldsLangCheckRequire4)) {
                            if (App.stoOM_KPISales_Class.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPISales_Class.data.items[0].data.BranchID)
                                    && !Ext.isEmpty(App.stoOM_KPISales_Class.data.items[0].data.SlsperId)
                                    && !Ext.isEmpty(App.stoOM_KPISales_Class.data.items[0].data.ClassID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPISales_Invt.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPISales_Invt, keys5, fieldsCheckRequire5, fieldsLangCheckRequire5)) {
                            if (App.stoOM_KPISales_Invt.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPISales_Invt.data.items[0].data.BranchID)
                                     && !Ext.isEmpty(App.stoOM_KPISales_Invt.data.items[0].data.SlsperId)
                                    && !Ext.isEmpty(App.stoOM_KPISales_Invt.data.items[0].data.InvtID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPICustomer_All.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPICustomer_All, keys6, fieldsCheckRequire6, fieldsLangCheckRequire6)) {
                            if (App.stoOM_KPICustomer_All.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPICustomer_All.data.items[0].data.BranchID)
                                   //&& !Ext.isEmpty(App.stoOM_KPICustomer_All.data.items[0].data.SlsperId)
                                    && !Ext.isEmpty(App.stoOM_KPICustomer_All.data.items[0].data.CustID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPICustomer_Class.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPICustomer_Class, keys7, fieldsCheckRequire7, fieldsLangCheckRequire7)) {
                            if (App.stoOM_KPICustomer_Class.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPICustomer_Class.data.items[0].data.BranchID)
                                    //&& !Ext.isEmpty(App.stoOM_KPICustomer_Class.data.items[0].data.SlsperId)
                                    && !Ext.isEmpty(App.stoOM_KPICustomer_Class.data.items[0].data.CustID)
                                    && !Ext.isEmpty(App.stoOM_KPICustomer_Class.data.items[0].data.ClassID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    else if (App.grdOM_KPICustomer_Invt.isHidden() == false) {
                        if (HQ.store.checkRequirePass(App.stoOM_KPICustomer_Invt, keys8, fieldsCheckRequire8, fieldsLangCheckRequire8)) {
                            if (App.stoOM_KPICustomer_Invt.data.length > 0) {
                                if (!Ext.isEmpty(App.stoOM_KPICustomer_Invt.data.items[0].data.BranchID)
                                    //&& !Ext.isEmpty(App.stoOM_KPICustomer_Invt.data.items[0].data.SlsperId)
                                    && !Ext.isEmpty(App.stoOM_KPICustomer_Invt.data.items[0].data.CustID)
                                    && !Ext.isEmpty(App.stoOM_KPICustomer_Invt.data.items[0].data.InvtID)) {
                                    isError = false;
                                }
                            }
                            issave = true;
                        } else {
                            return;
                        }
                    }
                    
                    if (issave && isError) {
                        HQ.message.show(2017020802, [App.itmDetail.title], '', true);
                        return;
                    }
                    if (App.grdOM_KPICondition.isHidden() == false)
                        if (HQ.store.checkRequirePass(App.stoOM_KPICondition, keys9, fieldsCheckRequire9, fieldsLangCheckRequire9))
                            save();
                        else {
                            App.tabInfo.setActiveTab(1);
                            return;
                        }
                    else 
                        save();
                }               
            }
            break;
        case "print":
            break;
        case "close":
            break;
    }
};

////////////////////////////////////////////////////////////////////////
var frmChange = function () {
    if (App.stoCycleNbr.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoCycleNbr);
    HQ.common.changeData(HQ.isChange, 'OM25100');

    if (App.cboCycleNbr.valueModels == null || HQ.isNew == true) {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.btnLoad.enable(true);      
        App.cboHandle.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.cboSlsperIdHeader.setReadOnly(false);

    }

    else
    {
        App.cboCycleNbr.setReadOnly(HQ.isChange);
        App.cboKPI.setReadOnly(HQ.isChange);
    }
        
};
////////////////////////////////////////////////////////////////////////
var stoCycleNbr_Load = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    App.cboCycleNbr.forceSelection = false;
    App.cboKPI.forceSelection = false;
    App.cboStatus.forceSelection = false;
    
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "KPI");
        record = sto.getAt(0);
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        
        //App.dtefromday.setValue(new Date());
        //App.dtetoday.setValue(new Date());
        record.set('Status', _beginStatus);
        App.cboHandle.setValue('N');
        App.dtefromday.setReadOnly(true);
        App.dtetoday.setReadOnly(true);
        App.cboApplyFor.setReadOnly(true);
        App.cboApplyTo.setReadOnly(true);
        App.cboType.setReadOnly(true);
        App.cboStatus.setReadOnly(true);

      
        App.cboCycleNbr.forceSelection = false;
        App.cboKPI.forceSelection = false;////////////////////////////
        
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboCycleNbr.focus(true); //focus ma khi tao moi
        sto.commitChanges();

        //App.stoOM_KPICpny_All.reload();
        //App.stoOM_KPICpny_Class.reload();
        //App.stoOM_KPICpny_Invt.reload();

        //App.stoOM_KPISales_All.reload();
        //App.stoOM_KPISales_Class.reload();
        //App.stoOM_KPISales_Invt.reload();

        //App.stoOM_KPICustomer_All.reload();
        //App.stoOM_KPICustomer_Class.reload();
        //App.stoOM_KPICustomer_Invt.reload();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.cboStatus.setValue(record.data.Status);
    if (!HQ.isInsert && HQ.isNew) {
        App.cboCycleNbr.forceSelection = false;
        HQ.common.lockItem(App.frmMain, true);
        HQ.store.insertBlank(sto, keys);
        HQ.store.insertBlank(sto, keys1);
        HQ.store.insertBlank(sto, keys2);
        HQ.store.insertBlank(sto, keys3);
        HQ.store.insertBlank(sto, keys4);
        HQ.store.insertBlank(sto, keys5);
        HQ.store.insertBlank(sto, keys6);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    HQ.common.showBusy(false);
    frmChange();
    if (App.cboApplyFor.value == 'C' && App.cboApplyTo.value == 'A') {
        App.stoOM_KPICpny_All.reload();
    }
    else {
        if (App.cboApplyFor.value == 'C' && App.cboApplyTo.value == 'G') {
            App.stoOM_KPICpny_Class.reload();
        }
        else {
            if (App.cboApplyFor.value == 'C' && App.cboApplyTo.value == 'I') {
                App.stoOM_KPICpny_Invt.reload();
            }
            else {
                if (App.cboApplyFor.value == 'S' && App.cboApplyTo.value == 'A') {
                    App.stoOM_KPISales_All.reload();
                }
                else {
                    if (App.cboApplyFor.value == 'CUS' && App.cboApplyTo.value == 'A') {
                        App.stoOM_KPICustomer_All.reload();
                    }
                    else if (App.cboApplyFor.value == 'CUS') {
                        if (App.cboApplyFor.value == 'CUS' && App.cboApplyTo.value == 'G') {
                            App.stoOM_KPICustomer_Class.reload();
                        }
                        else {
                            App.stoOM_KPICustomer_Invt.reload();
                        }
                    }
                    if (App.cboApplyFor.value == 'S' && App.cboApplyTo.value == 'G') {
                        App.stoOM_KPISales_Class.reload();
                    }
                    else if (App.cboApplyFor.value == 'S') {
                        App.stoOM_KPISales_Invt.reload();
                    }
                }
            }
        }
    }
    if (!Ext.isEmpty(App.cboTerritory.getValue()) && !Ext.isEmpty(App.cboZone.getValue())) {
        App.grdOM_KPICondition.store.reload();
        App.grdSalesClassDetail.store.reload();
    } else {
        App.grdOM_KPICondition.store.clearData();
        App.grdOM_KPICondition.view.refresh();
        stoKPICondition_Load(App.grdOM_KPICondition.store);

        App.grdSalesClassDetail.store.clearData();
        App.grdSalesClassDetail.view.refresh();
        stoSalesClassDetail_Load(App.grdSalesClassDetail.store);
    }
   
};

/////Store///////////////////////////////////////////////////////////////////
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
var stoKPICpnyAllLoad = function (sto) {   
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto,keys);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }       
    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.disable(true);
            }
            if(value == 'H')
            {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }        
    }   
};
var stoKPICpnyClassLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys1);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys1);
        }
    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }   
};

var stoKPICpnyInvtLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys2);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys2);
        }
    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }   
};

var stoKPISalesAllLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys3);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys3);
        }
    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }    
};

var stoKPICustomerAllLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys6);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys6);
        }

    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }

};

var stoKPICustomerClassLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys7);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys7);
        }

    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }
};

var stoKPICustomerInvtLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys8);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys8);
        }
    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }
};

var stoKPISalesClassLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys4);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys4);
        }
    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }
};

var stoKPISalesInvtLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys5);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys5);
        }
    }
    var value = App.cboStatus.getValue();
    if (!HQ.isNew) {
        if (value != null) {
            if (value == 'C' || value == 'W' || value == 'D')
            {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
            }
            if (value == 'H') {
                App.dtefromday.setReadOnly(true);
                App.dtetoday.setReadOnly(true);
                App.cboApplyFor.setReadOnly(true);
                App.cboApplyTo.setReadOnly(true);
                App.cboType.setReadOnly(true);
                App.cboStatus.setReadOnly(true);
                App.menuClickbtnDelete.enable(true);
            }
        }
    }
};

var stoKPICondition_Load = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys9);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(sto, keys9, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys9);
        }
    }
}

var stoSalesClassDetail_Load = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isGridChange(sto, keys10);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(sto, keys10, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys10);
        }
    }
}

var stoKPICpnyAllChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto,keys);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange == true) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};
var stoKPICpnyClassChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys1);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};
var stoKPICpnyInvtChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys2);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};
var stoKPISalesAllChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys3);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};

var stoKPICustomerAllChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys3);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};

var stoKPICustomerClassChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys7);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};

var stoKPISalesClassChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys4);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};

var stoKPICustomerInvtChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys8);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};

var stoKPISalesInvtChanged = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys5);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};

var stoOM_KPICondition_Changed = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys9);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};

var stoSalesClassDetail_Changed = function (sto) {
    HQ.isChange = HQ.store.isGridChange(sto, keys10);
    HQ.common.changeData(HQ.isChange, 'OM25100');
    if (HQ.isChange) {
        App.cboCycleNbr.setReadOnly(true);
        App.cboKPI.setReadOnly(true);
        App.cboZone.setReadOnly(true);
        App.cboTerritory.setReadOnly(true);
        App.btnLoad.disable(true);
    }
    else {
        App.cboCycleNbr.setReadOnly(false);
        App.cboKPI.setReadOnly(false);
        App.cboZone.setReadOnly(false);
        App.cboTerritory.setReadOnly(false);
        App.btnLoad.disable(false);
    }
};
///////////////////////////////////////////////////
var cboCycleNbr_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoCycleNbr.loading) {
        var obj = App.cboCycleNbr.getStore().findRecord("CycleNbr", App.cboCycleNbr.getValue());
        if (obj != null) {
            App.dtefromday.setValue(obj.data.StartDate);
            App.dtetoday.setValue(obj.data.EndDate);
        }
        App.stoCycleNbr.reload();
    }
};

var cboCycleNbr_Blur = function (sender, value) {
    HQ.isFirstLoad = true;
    //if (sender.valueModels != null && !App.stoCycleNbr.loading) {
    var obj = App.cboCycleNbr.getStore().findRecord("CycleNbr", App.cboCycleNbr.getValue());
    if (obj== null) {
        App.cboCycleNbr.setValue('');
    }
};

var cboCycleNbr_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoCycleNbr.loading) {
        var obj = App.cboCycleNbr.getStore().findRecord("CycleNbr", App.cboCycleNbr.getValue());
        if (obj != null) {
            App.dtefromday.setValue(obj.data.StartDate);
            App.dtetoday.setValue(obj.data.EndDate);
        }
        App.stoCycleNbr.reload();     
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboCycleNbr_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboCycleNbr.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboCycleNbr_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboCycleNbr.setValue('');
        App.dtefromday.setValue('');
        App.dtetoday.setValue('');
        App.cboKPI.setValue('');
        App.cboApplyFor.setValue('');
        App.cboApplyTo.setValue('');
        App.cboType.setValue('');
       
        App.stoOM_KPICpny_Class.reload();
    }
};
var cboKPI_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboKPI.setValue('');
        App.cboApplyFor.setValue('');
        App.cboApplyTo.setValue('');
        App.cboType.setValue('');
        App.stoOM_KPICpny_Class.reload();
    }
};
var cboKPI_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoCycleNbr.loading) {
        var obj = App.cboKPI.getStore().findRecord("KPI", App.cboKPI.getValue());
        var allowBlank = true;
        if (obj != null) {
            App.cboApplyFor.setValue(obj.data.ApplyFor);
            App.cboApplyTo.setValue(obj.data.ApplyTo);
            App.cboType.setValue(obj.data.Type);
            if (obj.data.ApplyFor == 'C'
                && obj.data.ApplyTo == 'A') {
                App.grdOM_KPICpny_All.show();
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPISales_All.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.hide();
                
                
            }else if (obj.data.ApplyFor == 'C'
                && obj.data.ApplyTo == 'G') {
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPISales_All.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.hide();
                App.grdOM_KPICpny_Class.show();
            }
            else if (obj.data.ApplyFor == 'C'
                && obj.data.ApplyTo == 'I') {
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPISales_All.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.hide();
                App.grdOM_KPICpny_Invt.show();
            }
            else if (obj.data.ApplyFor == 'S'
                && obj.data.ApplyTo == 'A') {
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.hide();
                App.grdOM_KPISales_All.show();
                allowBlank = false;
            }
            else if (obj.data.ApplyFor == 'S'
                && obj.data.ApplyTo == 'G') {
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_All.hide();
                App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.hide();
                App.grdOM_KPISales_Class.show();
                allowBlank = false;
            }
            else if (obj.data.ApplyFor == 'S'
                && obj.data.ApplyTo == 'I') {
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPISales_All.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.hide();
                App.grdOM_KPISales_Invt.show();
                allowBlank = false;
            }
            else if (obj.data.ApplyFor == 'CUS'
               && obj.data.ApplyTo == 'A') {
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPISales_All.hide();
                App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.show();
            }
            else if (obj.data.ApplyFor == 'CUS'
                && obj.data.ApplyTo == 'G') {
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPISales_All.hide();
                App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPICustomer_All.hide();
                App.grdOM_KPICustomer_Invt.hide();
                App.grdOM_KPICustomer_Class.show();
            }
            else if (obj.data.ApplyFor == 'CUS'
                && obj.data.ApplyTo == 'I') {
                App.grdOM_KPICpny_Class.hide();
                App.grdOM_KPICpny_All.hide();
                App.grdOM_KPICpny_Invt.hide();
                App.grdOM_KPISales_Class.hide();
                App.grdOM_KPISales_All.hide();
               App.grdOM_KPISales_Invt.hide();
                App.grdOM_KPICustomer_Class.hide();
                App.grdOM_KPICustomer_All.hide();
                App.grdOM_KPICustomer_Invt.show();
            }
            if (obj.data.ApplyFor == 'S'
                && obj.data.ApplyTo == 'G') {
                if (HQ.showTabSalesClassDetail == 0)
                    App.tabInfo.child('#pnlSalesClassDetail').tab.hide();
                else {
                    App.tabInfo.child('#pnlSalesClassDetail').tab.show();
                }
            } else {                
                App.tabInfo.child('#pnlSalesClassDetail').tab.hide();
            }
        } else {

        }
        App.cboSlsperIdHeader.allowBlank = allowBlank;
        App.cboSlsperIdHeader.validate();
        App.stoCycleNbr.reload();
        App.stoOM_KPICpny_All.addListener('load', checkStoLoad);
        App.stoOM_KPICpny_Class.addListener('load', checkStoLoad);
        App.stoOM_KPICpny_Invt.addListener('load', checkStoLoad);
        App.stoOM_KPISales_All.addListener('load', checkStoLoad);
        App.stoOM_KPISales_Class.addListener('load', checkStoLoad);
        App.stoOM_KPISales_Invt.addListener('load', checkStoLoad);
        App.stoOM_KPICustomer_All.addListener('load', checkStoLoad);
        App.stoOM_KPICustomer_Class.addListener('load', checkStoLoad);
        App.stoOM_KPICustomer_Invt.addListener('load', checkStoLoad);

        App.stoOM_KPICondition.addListener('load', checkStoLoad);
        App.stoSalesClassDetail.addListener('load', checkStoLoad);
    }
};

var cboStatus_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null && !App.cboStatus.loading) {
        App.cboStatus.setValue(App.cboStatus.valueModels[0] ? App.cboStatus.valueModels[0].data.Code : '');
        var value = App.cboStatus.getValue();
        if (value != null) {
            App.cboHandle.getStore().reload();           
        }
    }
};

var cboZone_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null && !App.cboTerritory.loading) {
        var value = App.cboZone.getValue().join(',');
        if (value != null) {
            App.cboTerritory.setValue('');
            App.cboTerritory.getStore().reload();
        }
    }
};
var cboTerritory_Expand = function (sender, newValue, oldValue) {
    App.cboTerritory.setValue('');
    App.cboTerritory.store.reload();
};
var cboBranchIDAll_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _branchID = value;
       // App.cboSlsperId.getStore().reload();
    } else {
        _branchID = '';
       // App.cboSlsperId.getStore().reload();
    }
};

var cboBranchIDAll_Select = function (sender, value) {
    if (!App.cboSlsperId.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _branchID = sender.valueModels[0].data.BranchID;
        } else {
            _branchID = '';
        }
       // App.cboSlsperId.getStore().reload();
    }
};

var cboBranchIDCustAll_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _branchID = value;
        //App.cboSlsperIdCust.getStore().reload();
        //App.cboCustId.getStore().reload();
    } else {
        _branchID = '';
        //App.cboSlsperIdCust.getStore().reload();
        //App.cboCustId.getStore().reload();
    }
};

var cboBranchIDCustAll_Select = function (sender, value) {
    if (!App.cboSlsperIdCust.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _branchID = sender.valueModels[0].data.BranchID;
        } else {
            _branchID = '';
        }
        //App.cboSlsperIdCust.getStore().reload();
        //App.cboCustId.getStore().reload();
    }
};

var cboBranchIDClass_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _branchID = value;
        //App.cboSlsperIdClass.getStore().reload();
    } else {
        _branchID = '';
       // App.cboSlsperIdClass.getStore().reload();
    }
};

var cboBranchIDClass_Select = function (sender, value) {
    if (!App.cboSlsperIdClass.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _branchID = sender.valueModels[0].data.BranchID;
        } else {
            _branchID = '';
        }
        //App.cboSlsperIdClass.getStore().reload();
    }
};

var cboBranchIDClassCust_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _branchID = value;
        //App.cboSlsperIdClassCust.getStore().reload();
        //App.cboCustIdClass.getStore().reload();
    } else {
        _branchID = '';
        //App.cboSlsperIdClassCust.getStore().reload();
        //App.cboCustIdClass.getStore().reload();
    }
};

var cboBranchIDClassCust_Select = function (sender, value) {
    if (!App.cboSlsperIdClassCust.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _branchID = sender.valueModels[0].data.BranchID;
        } else {
            _branchID = '';
        }
        //App.cboSlsperIdClassCust.getStore().reload();
        //App.cboCustIdClass.getStore().reload();
    }
};

var cboBranchIDInvtCust_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _branchID = value;
        //App.cboSlsperIdInvtCust.getStore().reload();
        //App.cboCustIdInvt.getStore().reload();
    } else {
        _branchID = '';
        //App.cboSlsperIdInvtCust.getStore().reload();
        //App.cboCustIdInvt.getStore().reload();
    }
};

var cboBranchIDInvtCust_Select = function (sender, value) {
    if (!App.cboSlsperIdInvtCust.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _branchID = sender.valueModels[0].data.BranchID;
        } else {
            _branchID = '';
        }
        //App.cboSlsperIdInvtCust.getStore().reload();
        //App.cboCustIdInvt.getStore().reload();
    }
};

var cboBranchID_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _branchID = value;
        //App.cboSlsperIdInvt.getStore().reload();
    } else {
        _branchID = '';
        //App.cboSlsperIdInvt.getStore().reload();
    }
};

var cboBranchID_Select = function (sender, value) {
    if (!App.cboSlsperIdInvt.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _branchID = sender.valueModels[0].data.BranchID;
        } else {
            _branchID = '';
        }
        //App.cboSlsperIdInvt.getStore().reload();
    }
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM25100/Save',
            params: {
                lstMCCode: Ext.encode(App.stoCycleNbr.getRecordsValues()),
                stoOM_KPICpny_All: HQ.store.getData(App.stoOM_KPICpny_All),
                stoOM_KPICpny_Class: HQ.store.getData(App.stoOM_KPICpny_Class),
                stoOM_KPICpny_Invt: HQ.store.getData(App.stoOM_KPICpny_Invt),
                stoOM_KPISales_All: HQ.store.getData(App.stoOM_KPISales_All),
                stoOM_KPISales_Class: HQ.store.getData(App.stoOM_KPISales_Class),
                stoOM_KPISales_Invt: HQ.store.getData(App.stoOM_KPISales_Invt),
                stoOM_KPICustomer_All: HQ.store.getData(App.stoOM_KPICustomer_All),
                stoOM_KPICustomer_Class: HQ.store.getData(App.stoOM_KPICustomer_Class),
                stoOM_KPICustomer_Invt: HQ.store.getData(App.stoOM_KPICustomer_Invt),
                stoOM_KPICondition: HQ.store.getData(App.stoOM_KPICondition),
                stoSalesClassDetail: HQ.store.getData(App.stoSalesClassDetail)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                CycleNbr = data.result.CycleNbr;
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.cboCycleNbr.getStore().load({
                    callback: function () {
                        if (Ext.isEmpty(App.cboCycleNbr.getValue())) {
                            App.cboCycleNbr.setValue(CycleNbr);
                            App.stoCycleNbr.reload();
                        }
                        else {
                            App.cboCycleNbr.setValue(CycleNbr);
                            App.stoCycleNbr.reload();
                        }
                    }
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

// Submit the deleted data into server side
var deleteData = function (item) {    
    if (item == "yes") {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("DeletingData"),
                url: 'OM25100/DeleteAll',
                timeout: 7200,
                success: function (msg, data) {
                    App.cboCycleNbr.getStore().load();
                    menuClick("new");
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};
    
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew) {
            App.cboCycleNbr.setValue('');
            App.cboKPI.setValue('');
        }
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoCycleNbr.reload();
    }
};

function lastLineRef(store) {
    var num = 0;
    for (var j = 0; j < store.data.length; j++) {
        var item = store.data.items[j];

        if (!Ext.isEmpty(item.data.LineRef) && parseInt(item.data.LineRef) > num) {
            num = parseInt(item.data.LineRef);
        }
    };
    num++;
    var lineRef = num.toString();
    var len = lineRef.length;
    for (var i = 0; i < 10 - len; i++) {
        lineRef = "0" + lineRef;
    }
    return lineRef;
};

var btnLoad_click = function (btn, e, eOpts) {

    HQ.isChange = false;
    HQ.isFirstLoad = true;
    App.stoCycleNbr.reload();

    App.cboCpnyID.store.reload();
    App.cboCpnyIDClass.store.reload();
    App.cboCpnyIDInvt.store.reload();
    App.cboCpnyIDSalesAll.store.reload();
    App.cboCpnyIDSalesClass.store.reload();
    App.cboCpnyIDSalesinvt.store.reload();
    App.cboCpnyIDCustomerAll.store.reload();
    App.cboCpnyIDCustomerClass.store.reload();
    App.cboCpnyIDCustomerinvt.store.reload();

    if (App.cboStatus.value == 'H') {
        App.stoOM_KPICpny_All.addListener('load', checkStoLoad);
        App.stoOM_KPICpny_Class.addListener('load', checkStoLoad);
        App.stoOM_KPICpny_Invt.addListener('load', checkStoLoad);
        App.stoOM_KPISales_All.addListener('load', checkStoLoad);
        App.stoOM_KPISales_Class.addListener('load', checkStoLoad);
        App.stoOM_KPISales_Invt.addListener('load', checkStoLoad);
        App.stoOM_KPICustomer_All.addListener('load', checkStoLoad);
        App.stoOM_KPICustomer_Class.addListener('load', checkStoLoad);
        App.stoOM_KPICustomer_Invt.addListener('load', checkStoLoad);
        App.stoOM_KPICondition.addListener('load', checkStoLoad);
    }
};

var renderClassName = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboClassID.store, ['ClassID'], [record.data.ClassID])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.Descr;
};

var renderInvtName = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboInvtID.store, ['InvtID'], [record.data.InvtID])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.Descr;
};

var renderConditionType = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboConditionType.store, ['Code'], [record.data.Type])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.Descr;
};

var checkCancelEdit = function () {
    return (App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E')
    //return App.cboStatus.getValue() == _beginStatus;
};
///////////////////////////////////

var deleteData_Grid = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPICpny_All.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICpny_All.store, keys);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid1 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPICpny_Class.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICpny_Class.store, keys1);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid2 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPICpny_Invt.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICpny_Invt.store, keys2);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid3 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPISales_All.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPISales_All.store, keys3);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid4 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPISales_Class.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPISales_Class.store, keys4);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid5 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPISales_Invt.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPISales_Invt.store, keys5);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid6 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPICustomer_All.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICustomer_All.store, keys6);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid7 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPICustomer_Class.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICustomer_Class.store, keys7);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_Grid8 = function (item) {
    if (item == "yes") {
        //HQ.isChange = true;
        App.grdOM_KPICustomer_Invt.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICustomer_Invt.store, keys8);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_GridCondition = function (item) {
    if (item == "yes") {
        App.grdOM_KPICondition.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICondition.store, keys9);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

var deleteData_GridSalesClassDetail = function (item) {
    if (item == "yes") {
        App.grdSalesClassDetail.deleteSelected();
        HQ.isChange = HQ.store.isGridChange(App.grdSalesClassDetail.store, keys10);
        HQ.common.changeData(HQ.isChange, 'OM25100');
    }
};

//grd OM_KPICpny_All/////////////////////////////////////
var grdKPICpnyAll_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    if (checkCancelEdit())
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        return HQ.grid.checkBeforeEdit(e, keys);
    }
};

var grdKPICpnyAll_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboCpnyIDOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPICpny_All, e, keys);
    //Khi add new=> tren tab ten man hinh phai co dau *
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICpny_All.store, keys);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPICpnyAll_ValidateEdit = function (item, e) {
    if (!Ext.isEmpty(e.value)) {
        return HQ.grid.checkValidateEdit(App.grdOM_KPICpny_All, e, keys, false);
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPICpny_All, e, keys);
};

var grdKPICpnyAll_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPICpny_All);
    
};
//grd OM_KPICpny_Class/////////////////////////////////////
var grdKPICpnyClass_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys1);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E')
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        return HQ.grid.checkBeforeEdit(e, keys1);
    }
    //return HQ.grid.checkBeforeEdit(e, keys1);
    //return true;
};

var grdKPICpnyClass_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboCpnyIDClassOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPICpny_Class, e, keys1);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICpny_Class.store, keys1);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPICpnyClass_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        return HQ.grid.checkValidateEdit(App.grdOM_KPICpny_Class, e, keys1, false);
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPICpny_Class, e, keys1);
};

var grdKPICpnyClass_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPICpny_Class);
};
//grd OM_KPICpny_Invt/////////////////////////////////////
var grdKPICpnyInvt_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys2);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E')
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        return HQ.grid.checkBeforeEdit(e, keys2);
    }
    //return HQ.grid.checkBeforeEdit(e, keys2);
  
};

var grdKPICpnyInvt_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboCpnyIDInvtOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPICpny_Invt, e, keys2);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICpny_Invt.store, keys2);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPICpnyInvt_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        return HQ.grid.checkValidateEdit(App.grdOM_KPICpny_Invt, e, keys2, false);
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPICpny_Invt, e, keys2);
};

var grdKPICpnyInvt_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPICpny_Invt);
};
//grd OM_KPISales_All/////////////////////////////////////
var grdKPISalesAll_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys3);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E')
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        if (e.field == 'SlsperId') {
            _branchID = e.record.data.BranchID;
            App.cboSlsperId.store.reload();
        }
        return HQ.grid.checkBeforeEdit(e, keys3);
    }
    //return HQ.grid.checkBeforeEdit(e, keys3);
   
};

var grdKPISalesAll_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboCpnyIDOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        }
    }
    if (e.field == 'SlsperId') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboSlsperIdOM25100_pcSlsperId.findRecord('SlsperId', e.value);
        if (obj) {
            e.record.set('SlsperName', obj.data.Name);
            e.record.set('Position', obj.data.Position);
            e.record.set('PosDesc', obj.data.PosDesc);
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPISales_All, e, keys3);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPISales_All.store, keys3);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPISalesAll_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        return HQ.grid.checkValidateEdit(App.grdOM_KPISales_All, e, keys3, false);
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPISales_All, e, keys3);
};

var grdKPISalesAll_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPISales_All);
};

//var grdKPISalesAll_BeforeEdit = function (editor, e) {
//    if (!HQ.form.checkRequirePass(App.frmMain)) {
//        return false;
//    }
//    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
//        return false;
//    }
//    //else {
//    //    return HQ.grid.checkBeforeEdit(e, keys3);
//    //}
//    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E') 
//    {
//        return false;
//    }
//    if (HQ.isUpdate == false && HQ.isInsert == false) {
//        return false;
//    }
//    else {
//        return HQ.grid.checkBeforeEdit(e, keys3);
//    }
//    //return HQ.grid.checkBeforeEdit(e, keys3);

//};

var grdOM_KPICustomer_All_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboCpnyIDCustomerAll.store, ["CpnyID"], [e.value]);  //  App.cboCpnyIDOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        } else {
            e.record.set('CpnyName', '');
        }
    }
    if (e.field == 'SlsperId') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboSlsperIdCust.store, ["SlsperId"], [e.value]);  //App.cboSlsperIdOM25100_pcSlsperId.findRecord('SlsperId', e.value);
        if (obj) {
            e.record.set('SlsperName', obj.data.Name);
        } else {
            e.record.set('SlsperName', '');
        }
    }
    if (e.field == 'CustID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboCustId.store, ["CustId"], [e.value]);  //App.cboCustIdOM25100_pcCustId.findRecord('CustId', e.value);
        if (obj) {
            e.record.set('CustName', obj.data.CustName);
        } else {
            e.record.set('CustName', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPICustomer_All, e, keys6);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICustomer_All.store, keys6);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdOM_KPICustomer_All_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        if (e.field == "CustID" && e.value != "") {
            var obj = HQ.store.findInStore(App.stoOM_KPICustomer_All, ['BranchID', 'SlsperId', 'CustID'], [e.record.data.BranchID, e.record.data.SlsperId, e.value]);
            if(obj != undefined)
                return HQ.grid.checkValidateEdit(App.grdOM_KPICustomer_All, e, keys6, false);
        }
    }
    
};


var grdOM_KPICustomer_All_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys6);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E') 
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        _branchID = e.record.data.BranchID;
        if (e.field == 'SlsperId') {
            if (e.record.data.BranchID != '' && e.record.data.CustID == '') {
                App.cboSlsperIdCust.store.reload();
                return true;
            }
            if (e.record.data.BranchID == '' || e.record.data.BranchID != '' && e.record.data.CustID != '') {
                return false;
            }
            App.cboSlsperIdCust.store.reload();
        }
        if (e.field == 'CustID') {
            App.cboCustId.store.reload();
        }
        return HQ.grid.checkBeforeEdit(e, keys6);
    }
   

};
var grdKPICustomerAll_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPICustomer_All);
};
//grd OM_KPISales_Class/////////////////////////////////////
var grdKPISalesClass_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys4);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E')
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        if (HQ.showTabSalesClassDetail == 1) {
            if (e.record.data.ClassID == '') {
                e.record.set('ClassID', '*');
                e.record.commit();
            }
            if (e.field == 'ClassID') {
                return false;
            }
        }
        if (e.field == 'SlsperId') {
            _branchID = e.record.data.BranchID;
            App.cboSlsperIdClass.store.reload();
        }
        return HQ.grid.checkBeforeEdit(e, keys4);
    }

   
    //return HQ.grid.checkBeforeEdit(e, keys4);
    //return true;
};

var grdKPISalesClass_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboCpnyIDOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        }
    }
    if (e.field == 'SlsperId') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboSlsperIdClassOM25100_pcSlsperId.findRecord('SlsperId', e.value);
        if (obj) {
            e.record.set('SlsperName', obj.data.Name);
            e.record.set('Position', obj.data.Position);
            e.record.set('PosDesc', obj.data.PosDesc);
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPISales_Class, e, keys4);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPISales_Class.store, keys4);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPISalesClass_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        return HQ.grid.checkValidateEdit(App.grdOM_KPISales_Class, e, keys4,false);
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPISales_Class, e, keys4);
};

//grd OM_KPICustomer_Class/////////////////////////////////////
var grdKPICustomerClass_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys7);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E') 
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        _branchID = e.record.data.BranchID;
        if (e.field == 'SlsperId') {
            if (e.record.data.BranchID != '' && e.record.data.CustID == '') {
                App.cboSlsperIdClassCust.store.reload();
                return true;
            }
            if (e.record.data.BranchID == '' || e.record.data.BranchID != '' && e.record.data.CustID != '') {
                return false;
            }            
            App.cboSlsperIdClassCust.store.reload();
        }
        if (e.field == 'CustID') {
            App.cboCustIdClass.store.reload();
        }
        return HQ.grid.checkBeforeEdit(e, keys7);
    }
};

var grdKPICustomerClass_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboCpnyIDCustomerClass.store, ["CpnyID"], [e.value]);  //  App.cboCpnyIDOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        } else {
            e.record.set('CpnyName', '');
        }
    }
    if (e.field == 'SlsperId') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboSlsperIdClassCust.store, ["SlsperId"], [e.value]);  //App.cboSlsperIdOM25100_pcSlsperId.findRecord('SlsperId', e.value);
        if (obj) {
            e.record.set('SlsperName', obj.data.Name);
        } else {
            e.record.set('SlsperName', '');
        }
    }
    if (e.field == 'CustID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboCustIdClass.store, ["CustId"], [e.value]);  //App.cboCustIdOM25100_pcCustId.findRecord('CustId', e.value);
        if (obj) {
            e.record.set('CustName', obj.data.CustName);
        } else {
            e.record.set('CustName', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPICustomer_Class, e, keys7);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICustomer_Class.store, keys7);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPICustomerClass_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        if (e.record.data.CustID != "" && e.field == "ClassID") {
            var obj = HQ.store.findInStore(App.stoOM_KPICustomer_Class, ['BranchID', 'SlsperId', 'CustID', 'ClassID'], [e.record.data.BranchID, e.record.data.SlsperId, e.record.data.CustID, e.value]);
            if (obj != undefined)
                return HQ.grid.checkValidateEdit(App.grdOM_KPICustomer_Class, e, keys7, false);
        }
        if (e.record.data.ClassID != "" && e.field == "CustID") {
            var obj = HQ.store.findInStore(App.stoOM_KPICustomer_Class, ['BranchID', 'SlsperId', 'CustID', 'ClassID'], [e.record.data.BranchID, e.record.data.SlsperId, e.value, e.record.data.ClassID]);
            if (obj != undefined)
                return HQ.grid.checkValidateEdit(App.grdOM_KPICustomer_Class, e, keys7, false);
        }
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPICustomer_Class, e, keys7);
};

var grdKPISalesClass_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPISales_Class);
};

var grdKPICustomerClass_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPICustomer_Class);
};

//grd OM_KPICustomer_Invt/////////////////////////////////////
var grdKPICustomerInvt_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys8);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E') 
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        _branchID = e.record.data.BranchID;
        if (e.field == 'SlsperId') {
            if (e.record.data.BranchID != '' && e.record.data.CustID == '') {
                App.cboSlsperIdInvtCust.store.reload();
                return true;
            }
            if (e.record.data.BranchID == '' || e.record.data.BranchID != '' && e.record.data.CustID != '') {
                return false;
            }
            App.cboSlsperIdInvtCust.store.reload();
        } 
        if (e.field == 'CustID') {
            App.cboCustIdInvt.store.reload();
        }
        return HQ.grid.checkBeforeEdit(e, keys8);
    }
    
    //return HQ.grid.checkBeforeEdit(e, keys5);

};

var grdKPICustomerInvt_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboCpnyIDCustomerinvt.store, ["CpnyID"], [e.value]);  //  App.cboCpnyIDOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        } else {
            e.record.set('CpnyName', '');
        }
    }
    if (e.field == 'SlsperId') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboSlsperIdInvtCust.store, ["SlsperId"], [e.value]);  //App.cboSlsperIdOM25100_pcSlsperId.findRecord('SlsperId', e.value);
        if (obj) {
            e.record.set('SlsperName', obj.data.Name);
        } else {
            e.record.set('SlsperName', '');
        }
    }
    if (e.field == 'CustID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = HQ.store.findRecord(App.cboCustIdInvt.store, ["CustId"], [e.value]);  //App.cboCustIdOM25100_pcCustId.findRecord('CustId', e.value);
        if (obj) {
            e.record.set('CustName', obj.data.CustName);
        } else {
            e.record.set('CustName', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPICustomer_Invt, e, keys8);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICustomer_Invt.store, keys8);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPICustomerinvt_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        if (e.record.data.CustID != "" && e.field == "InvtID") {
            var obj = HQ.store.findInStore(App.stoOM_KPICustomer_Invt, ['BranchID', 'SlsperId', 'CustID', 'InvtID'], [e.record.data.BranchID, e.record.data.SlsperId, e.record.data.CustID, e.value]);
            if (obj != undefined)
                return HQ.grid.checkValidateEdit(App.grdOM_KPICustomer_Invt, e, keys8, false);
        }
        if (e.record.data.InvtID != "" && e.field == "CustID") {
            var obj = HQ.store.findInStore(App.stoOM_KPICustomer_Invt, ['BranchID', 'SlsperId', 'CustID', 'InvtID'], [e.record.data.BranchID, e.record.data.SlsperId, e.value, e.record.data.InvtID]);
            if (obj != undefined)
                return HQ.grid.checkValidateEdit(App.grdOM_KPICustomer_Invt, e, keys8, false);
        }
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPICustomer_Invt, e, keys5);
};

var grdKPICustomerInvt_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPICustomer_Invt);
};

//grd OM_KPISales_Invt/////////////////////////////////////
var grdKPISalesInvt_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
        return false;
    }
    //else {
    //    return HQ.grid.checkBeforeEdit(e, keys5);
    //}
    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E')
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        if (e.field == 'SlsperId') {
            _branchID = e.record.data.BranchID;
            App.cboSlsperIdInvt.store.reload();
        }
        return HQ.grid.checkBeforeEdit(e, keys5);
    }
    //return HQ.grid.checkBeforeEdit(e, keys5);
   
};

var grdKPISalesInvt_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboCpnyIDOM25100_pcBranchID.findRecord('CpnyID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);
        }
    }
    if (e.field == 'SlsperId') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
        var obj = App.cboSlsperIdInvtOM25100_pcSlsperId.findRecord('SlsperId', e.value);
        if (obj) {
            e.record.set('SlsperName', obj.data.Name);
            e.record.set('Position', obj.data.Position);
            e.record.set('PosDesc', obj.data.PosDesc);
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPISales_Invt, e, keys5);
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPISales_Invt.store, keys5);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdKPISalesinvt_ValidateEdit = function (item, e) {
    if (e.record.data.BranchID && e.record.data.BranchID != "") {
        return HQ.grid.checkValidateEdit(App.grdOM_KPISales_Invt, e, keys5, false);
    }
    //return HQ.grid.checkValidateEdit(App.grdOM_KPISales_Invt, e, keys5);
};

var grdKPISalesInvt_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPISales_Invt);
};

var grdOM_KPICondition_BeforeEdit = function (editor, e) {    
    //if (App.cboZone.value.join(',') == '' && App.cboTerritory.value.join(',') == '') {
    //    return false;
    //}
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }

    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E') 
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    if (e.field != 'Type' && e.record.data.Type == '') {
        return false;
    }    
};

var grdOM_KPICondition_Edit = function (item, e) {
    HQ.isChange = true;
    if (e.field == 'Type' && !Ext.isEmpty(e.record.data.Type)) {
        if (Ext.isEmpty(e.record.data.LineRef)) {
            e.record.set('LineRef', lastLineRef(App.grdOM_KPICondition.store));
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_KPICondition, e, 'Type');
    HQ.isChange = HQ.store.isGridChange(App.grdOM_KPICondition.store, keys9);
    HQ.common.changeData(HQ.isChange, 'OM25100');
};

var grdOM_KPICondition_ValidateEdit = function (item, e) {
    if (e.record.data.Type && e.record.data.Type != "") {
        return HQ.grid.checkValidateEdit(App.grdOM_KPICondition, e, keys9, false);
    }
};

var grdOM_KPICondition_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_KPICondition);
};


////////////////////////////
var grdSalesClassDetail_BeforeEdit = function (editor, e) {
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }

    if (checkCancelEdit()) //(App.cboStatus.getValue() == 'C' || App.cboStatus.getValue() == 'E') 
    {
        return false;
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    return HQ.grid.checkBeforeEdit(e, keys10);
};

var grdSalesClassDetail_Edit = function (item, e) {
    HQ.isChange = true;
    HQ.grid.checkInsertKey(App.grdSalesClassDetail, e, 'ClassID');
    HQ.isChange = HQ.store.isGridChange(App.grdSalesClassDetail.store, keys10);
    HQ.common.changeData(HQ.isChange, 'OM25100');

    if (e.field == 'ClassID') {
        var descr = '';
        if (!Ext.isEmpty(e.record.data.ClassID && App.cboGroupClassID.valueModels.length > 0)) {
            descr = App.cboGroupClassID.valueModels[0].data.Descr;
        }
        e.record.set('Descr', descr);
    }
};

var grdSalesClassDetail_ValidateEdit = function (item, e) {
   // if (e.record.data.ClassID && e.record.data.ClassID != "") {
        return HQ.grid.checkValidateEdit(App.grdSalesClassDetail, e, keys10, false);
    //}
};

var grdSalesClassDetail_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSalesClassDetail);
};

var stringFilter = function (record) {
    if (HQ.focus == 'grdOM_KPICondition') {
        if (this.dataIndex == 'Type') {
            App.cboConditionType.store.clearFilter();
            return HQ.grid.filterComboDescr(record, this, App.cboConditionType.store, "Code", "Descr");
        }
    }
    return HQ.grid.filterString(record, this);
};

var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing...",
            url: 'OM25100/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {

            },
            success: function (msg, data) {
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                    btnLoad_click();
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701', '', '');
        sender.reset();
    }
};

var cboApplyFor_Change = function (sender, e) {
    if (e == "C")
        App.cboSlsperIdHeader.allowBlank = true;
    else
        App.cboSlsperIdHeader.allowBlank = false;
}

var joinParams = function(multiCombo) {
    var returnValue = "";
    if (multiCombo.value && multiCombo.value.length) {
        returnValue = multiCombo.value.join();
    }
    else {
        if (multiCombo.getValue()) {
            returnValue = multiCombo.rawValue;
        }
    }

    return returnValue;
}

var cboSlsperIdHeader_Change = function (sender, e) {
    App.cboSlsperId.getStore().reload();
}

var cboTerritory_Change = function (sender, e) {
    App.cboCpnyID.store.reload();
    App.cboCpnyIDClass.store.reload();
    App.cboCpnyIDInvt.store.reload();
    App.cboCpnyIDSalesAll.store.reload();
    App.cboCpnyIDSalesClass.store.reload();
    App.cboCpnyIDSalesinvt.store.reload();
    App.cboCpnyIDCustomerAll.store.reload();
    App.cboCpnyIDCustomerClass.store.reload();
    App.cboCpnyIDCustomerinvt.store.reload();
}
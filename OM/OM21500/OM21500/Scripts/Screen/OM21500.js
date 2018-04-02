//// Declare //////////////////////////////////////////////////////////

var keys = ['DiscCode'];
var fieldsCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate", "PromoType", "ObjApply", "DiscType"];
var fieldsLangCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate", "OM21500PromoType", "OM21500ObjApply", "OM21500DiscType"];
var _crrDiscCode = '';
var _cpnyTitle = '';
var _invtTitle = '';
var _beginStatus = 'H';
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 5;
var _isLoadMaster = false;
var loadPage = false;
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_DiscDescr.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    var grd = App.grdOM_DiscDescr;
    if (HQ.focus == 'cpny') {
        grd = App.grdCompany;
    } else if (HQ.focus == 'invt') {
        grd = App.grdInvt;
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
            if (HQ.isInsert && HQ.focus == 'header') {
                HQ.grid.insert(App.grdOM_DiscDescr, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (HQ.focus == 'header') {
                    if (App.slmOM_DiscDescr.selected.items[0] != undefined) {
                        if (App.slmOM_DiscDescr.selected.items[0].data.RoleID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_DiscDescr)], 'deleteData', true);
                        }
                    }
                } else if (HQ.focus == 'cpny') {
                    var selRecs = App.grdCompany.selModel.selected.items;
                    if (selRecs.length > 0) {
                        var params = [];
                        selRecs.forEach(function (record) {
                            params.push(record.data.CpnyID);
                        });
                        HQ.message.show(2015020806,
                            params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                            'deleteSelectedCompanies');
                    }
                } else if (HQ.focus == 'invt') {
                    var selRecs = App.grdInvt.selModel.selected.items;
                    if (selRecs.length > 0) {
                        var params = [];
                        selRecs.forEach(function (record) {
                            params.push(record.data.InvtID);
                        });
                        HQ.message.show(2015020806,
                            params.join(" & ") + ",",
                            'deleteSelectedInvt');
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_DiscDescr, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            //HQ.common.close(this);
            break;
    }

};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    loadPage = false;
    App.cboPromoType.getStore().addListener('load', checkLoad);
    App.cboObjApply.getStore().addListener('load', checkLoad);
    App.cboCpnyID.getStore().addListener('load', checkLoad);
    App.cboInvtType.getStore().addListener('load', checkLoad);
    App.cboDiscType.getStore().addListener('load', checkLoad);
    App.cboApplyFor.getStore().addListener('load', checkLoad);
    App.cboPromoType.getStore().reload();
    App.cboObjApply.getStore().reload();
    App.cboCpnyID.getStore().reload();
    App.cboInvtType.getStore().reload();
    App.cboDiscType.getStore().reload();
    App.cboApplyFor.getStore().reload();
    _cpnyTitle = App.pnlAppComp.title;
    _invtTitle = App.tabInvt.title;
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
//var stoChanged = function (sto) {
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'OM21500');
//};
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoOM_DiscDescr) || HQ.store.isChange(App.stoDescCpny);
    HQ.common.changeData(HQ.isChange, 'OM21500');//co thay doi du lieu gan * tren tab title header
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    if (!HQ.isInsert && HQ.isNew) {        
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    App.stoInvt.reload();
};
var stoInvt_load = function (sto) {
    frmChange();
    if (_isLoadMaster) {
        App.stoDescCpny.reload();
    }
    filterGrdInvtByDiscCode();
}

var stoDescCpny_Load = function (sto) {    
    frmChange();
    if (_isLoadMaster) {        
        HQ.common.showBusy(false);
    }
    filterGrdCpnyByDiscCode();
}
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdOM_DiscDescr_BeforeEdit = function (editor, e) {
    if (e.field == 'OMTime' && e.record.data.ApplyFor == "PO") {
        return false;
    }
    else if (e.field == 'POTime' && e.record.data.ApplyFor == "OM") {
        return false;
    }
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_DiscDescr_Edit = function (item, e) {
    if (e.field == 'ToDate') {
        if (e.record.data.FromDate != null) {
            if (e.value < e.record.data.FromDate) {
                HQ.message.show(2015061501, '', '');
                e.record.set('ToDate', e.record.data.FromDate);
                return false;
            }
        }
    } else if (e.field == 'FromDate') {
        if (e.record.data.ToDate != null) {
            if (e.value > e.record.data.ToDate) {
                HQ.message.show(2015061501, '', '');
                e.record.set('FromDate', e.record.data.ToDate);
                return false;
            }
        }
    } else if (e.field == 'OMTime') {
        if (e.record.data.POTime != null) {
            if (e.value < e.record.data.POTime) {
                HQ.message.show(2015061501, '', '');
                e.record.set('OMTime', e.record.data.POTime);
                return false;
            }
        }
    } else if (e.field == 'POTime') {
        if (e.record.data.OMTime != null) {
            if (e.value > e.record.data.OMTime) {
                HQ.message.show(2015061501, '', '');
                e.record.set('POTime', e.record.data.OMTime);
                return false;
            }
        }
    }
    else if (e.field == 'DiscCode') {
        _crrDiscCode = e.record.data.DiscCode;
        setTitle();
        if (e.value && !e.record.data.DiscType) {
            e.record.set('DiscType','M');
        }
    }
    //if (e.record.data.OMTime == null && e.record.data.POTime == null && e.record.data.ApplayFor == "PO-OM")
    //{
    //    HQ.message.show(15, App.grdOM_DiscDescr.columns[10].text);
    //    return false;
    //}
    if (e.field == "OMTime" || e.field == "POTime") {
        if (e.record.data.ApplyFor == "PO-OM") {
            if (e.field == "OMTime")
            {
                if (e.record.data.OMTime == null || e.record.data.OMTime == "") {
                    HQ.message.show(15, e.column.text);
                    return false;
                }
            }
            if (e.field == "POTime")
            {
                if (e.record.data.POTime == null || e.record.data.POTime == "") {
                    HQ.message.show(15, e.column.text);
                    return false;
                }
            }
        }
        else if (e.record.data.ApplyFor == "PO")
        {
            if (e.field == "POTime") {
                if (e.record.data.POTime == null || e.record.data.POTime == "") {
                    HQ.message.show(15, e.column.text);
                    return false;
                }
            }
        }
        else if(e.record.data.ApplyFor == "OM")
        {
            if (e.field == "OMTime") {
                if (e.record.data.POTime == null || e.record.data.POTime == "") {
                    HQ.message.show(15, e.column.text);
                    return false;
                }
            }
        }
    }

    HQ.grid.checkInsertKey(App.grdOM_DiscDescr, e, keys);

};
var grdOM_DiscDescr_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_DiscDescr, e, keys);
};
var grdOM_DiscDescr_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_DiscDescr);
    frmChange();
};
var slmOM_DiscDescr_Select = function (sender, e) {
    _crrDiscCode = e.data.DiscCode;
    filterGrdCpnyByDiscCode();
    filterGrdInvtByDiscCode();
    frmChange();
    setTitle();
};
// Grid Inventory before edit
var grdInvt_beforeEdit = function (editor, e) {
    if (e.field == 'InvtType') {
        if (e.record.data.IsLock) {
            return false;
        }
    }
    var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            return HQ.grid.checkBeforeEdit(e, keys);
        }
    }
    else {
        return false;
    }
};
// Grid Inventory validate edit
var grdInvt_ValidateEdit = function (item, e) {
    var keys = ['DiscCode' ,'InvtID', 'InvtType'];
    return HQ.grid.checkValidateEdit(App.grdInvt, e, keys);
};

// Grid Inventory reject
var grdInvt_reject = function (record) {
    HQ.grid.checkReject(record, App.grdInvt);
    frmChange();
};

// Grid Company
var grdCompany_reject = function (record) {
    HQ.grid.checkReject(record, App.grdCompany);
    frmChange();
};

var grdCompany_BeforeEdit = function (editor, e) {
    return false;
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    var lstCpny = App.stoDescCpny;
    lstCpny.clearFilter();
    var lstItem = App.stoInvt;
    lstItem.clearFilter();
    var isBreak = false;
    for (var i = 0; i < App.stoOM_DiscDescr.data.length; i++) {
        var objH = App.stoOM_DiscDescr.data.items[i].data;
        if (objH.DiscCode != '') {
            var obj = HQ.store.findRecord(lstCpny, ['DiscCode'], [objH.DiscCode]);
            if (!obj) {
                HQ.message.show(2016090901, [objH.DiscCode], '', true);
                isBreak = true;
                break;
            }            
        }
        if (objH.ApplyFor == 'PO-OM') {
            if (objH.POTime == null) {
                HQ.message.show(15, App.grdOM_DiscDescr.columns[10].text);
                isBreak = true;
                break;
            }
            if (objH.OMTime == null) {
                HQ.message.show(15, App.grdOM_DiscDescr.columns[11].text);
                isBreak = true;
                break;
            }
        }
        else if (objH.ApplyFor == 'PO') {
            if (objH.POTime == null) {
                HQ.message.show(15, App.grdOM_DiscDescr.columns[10].text);
                isBreak = true;
                break;
            }
        }
        else if (objH.ApplyFor == 'OM') {
            {
                if (objH.OMTime == null) {
                    HQ.message.show(15, App.grdOM_DiscDescr.columns[11].text);
                    isBreak = true;
                    break;
                }
            }
        }

    }
    if (isBreak) {
        filterGrdCpnyByDiscCode();
        filterGrdInvtByDiscCode();
        return false;
    }    
    if (App.frmMain.isValid()) {
        App.tabDetail.setActiveTab(0);
        HQ.focus = 'header';
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21500/Save',
            params: {
                lstOM_DiscDescr: HQ.store.getData(App.stoOM_DiscDescr),
                lstCpnyDel: Ext.encode(App.stoDescCpny.getChangedData().Deleted),// HQ.store.getData(App.stoDescCpny),
                lstCpny: Ext.encode(lstCpny.getRecordsValues()),
                lstItemDel: Ext.encode(App.stoInvt.getChangedData().Deleted),
                lstItem: Ext.encode(lstItem.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;                
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_DiscDescr.deleteSelected();
        App.grdOM_DiscDescr.view.refresh();
        App.grdCompany.store.removeAll();
        App.grdInvt.store.removeAll();
        frmChange();
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        if (HQ.focus == 'header') {
            App.stoOM_DiscDescr.reload();
        } else if (HQ.focus == 'invt') {
            App.stoInvt.reload();
        } else if (HQ.focus == 'cpny') {
            App.stoDescCpny.reload();
        }
    }
};

var PromoType_render = function (value) {
    var record = HQ.store.findRecord(App.cboPromoType.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var ObjApply_render = function (value) {
    var record = HQ.store.findRecord(App.cboObjApply.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var DiscType_render = function (value) {
    var record = HQ.store.findRecord(App.cboDiscType.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var CpnyName_render = function (value) {
    var record = HQ.store.findRecord(App.cboCpnyID.store, ["CpnyID"], [value]);
    return (record) ? record.data.CpnyName : value;
};

var InvtType_render = function (value) {
    var record = HQ.store.findRecord(App.cboInvtType.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var ApplyFor_render = function (value) {
    var record = HQ.store.findRecord(App.cboApplyFor.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var setTitle = function () {
    var title = _cpnyTitle;
    var invtTitle = _invtTitle;
    if (_crrDiscCode != '') {
        title = _cpnyTitle + ' (' + App.grdOM_DiscDescr.columns[1].text + ':' + _crrDiscCode + ')';
        invtTitle = _invtTitle + ' (' + App.grdOM_DiscDescr.columns[1].text + ':' + _crrDiscCode + ')';
    }
    App.pnlAppComp.setTitle(title);
    App.tabInvt.setTitle(invtTitle);
}
var stringFilter = function (record) {
    if (HQ.focus == 'header') {
        if (this.dataIndex == 'PromoType') {
            App.cboPromoType.store.clearFilter();
            return HQ.grid.filterComboDescr(record, this, App.cboPromoType.store, "Code", "Descr");
        } else if (this.dataIndex == 'ObjApply') {
            App.cboObjApply.store.clearFilter();
            return HQ.grid.filterComboDescr(record, this, App.cboObjApply.store, "Code", "Descr");
        }
    } else if (HQ.focus == 'invt') {
        if (this.dataIndex == 'InvtType') {
            App.cboInvtType.store.clearFilter();
            return HQ.grid.filterComboDescr(record, this, App.cboInvtType.store, "Code", "Descr");
        }
    }
    return HQ.grid.filterString(record, this);
}

var filterGrdCpnyByDiscCode = function () {
    App.stoDescCpny.suspendEvents();
    App.grdCompany.getFilterPlugin().clearFilters();
    App.grdCompany.getFilterPlugin().getFilter('DiscCode').setValue([_crrDiscCode, '']);
    App.grdCompany.getFilterPlugin().getFilter('DiscCode').setActive(true);
    sortGrdCompany();
    App.stoDescCpny.resumeEvents();
    App.grdCompany.view.refresh();
}

var filterGrdInvtByDiscCode = function () {
    App.stoInvt.suspendEvents();
    App.grdInvt.getFilterPlugin().clearFilters();
    App.grdInvt.getFilterPlugin().getFilter('DiscCode').setValue([_crrDiscCode, '']);
    App.grdInvt.getFilterPlugin().getFilter('DiscCode').setActive(true);
    sortGrdInvt();
    App.stoInvt.resumeEvents();
    App.grdInvt.view.refresh();
};

var checkFromDate = function () {
};
var checkToDate = function () {
};

var sortGrdCompany = function () {
    if (App.grdCompany.store.data.length > 0) {
        App.grdCompany.store.sort('CpnyID', 'ASC');
    }    
}
var sortGrdInvt = function () {
    if (App.grdInvt.store.data.length > 0) {
        App.grdInvt.store.sort('InvtID', 'ASC');
    }    
}

var showFieldInvalid = function (form) {
    var done = 1;
    form.getForm().getFields().each(function (field) {
        if (!field.isValid()) {
            HQ.message.show(15, field.fieldLabel, '');
            done = 0;
            return false;
        }
    });
    return done;
}
///////////////////////////////////

// Tree Event /////////////////////////////////////////////////////////////////////////////
var btnCpnyAddAll_click = function (btn, e, eOpts) {
    if (_crrDiscCode == '') {
        App.tabDetail.setActiveTab(0);
        HQ.message.show(2016090911);
        return false;
    }
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var allNodes = getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
            if (allNodes && allNodes.length > 0) {
                App.stoDescCpny.suspendEvents();
                allNodes.forEach(function (node) {
                    if (node.data.Type == "Company") {
                        var idx = App.grdCompany.store.getCount();
                        var record = HQ.store.findInStore(App.grdCompany.store,
                            ['DiscCode', 'CpnyID'],
                            [_crrDiscCode, node.data.RecID]);
                        if (!record) {
                            App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                DiscCode: _crrDiscCode,
                                CpnyID: node.data.RecID,
                                CpnyName: node.data.text
                            }));
                        }
                    }
                });
                App.stoDescCpny.resumeEvents();
                App.stoDescCpny.loadPage(1);
                App.grdCompany.view.refresh();
                frmChange();
                App.treePanelBranch.clearChecked();
            }
        }
        else {
            showFieldInvalid(App.frmMain);            
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnCpnyAdd_click = function (btn, e, eOpts) {
    if (_crrDiscCode == '') {
        App.tabDetail.setActiveTab(0);
        HQ.message.show(2016090911);
        return false;
    }
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var allNodes = App.treePanelBranch.getCheckedNodes();
            if (allNodes && allNodes.length > 0) {
                App.stoDescCpny.suspendEvents();
                allNodes.forEach(function (node) {
                    if (node.attributes.Type == "Company") {
                        var idx = App.grdCompany.store.getCount();
                        var record = HQ.store.findInStore(App.grdCompany.store,
                            ['DiscCode', 'CpnyID'],
                            [_crrDiscCode, node.attributes.RecID]);
                        if (!record) {
                            App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                DiscCode: _crrDiscCode,
                                CpnyID: node.attributes.RecID,
                                CpnyName: node.text
                            }));
                        }
                    }
                });
                App.stoDescCpny.resumeEvents();
                App.stoDescCpny.loadPage(1);
                App.grdCompany.view.refresh();
                frmChange();
                App.treePanelBranch.clearChecked();
            }            
        }
        else {
            showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnCpnyDel_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var selRecs = App.grdCompany.selModel.selected.items;
            if (selRecs.length > 0) {
                var params = [];
                selRecs.forEach(function (record) {
                    params.push(record.data.CpnyID);
                });
                HQ.message.show(2015020806,
                    params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                    'deleteSelectedCompanies');
            }
        }
        else {
            showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnCpnyDelAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid() && App.grdCompany.store.data.length > 0) {
            HQ.message.show(11, '', 'deleteAllCompanies');
        }
        else {
            showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var getDeepAllLeafNodes = function (node, onlyLeaf) {
    var allNodes = new Array();
    if (!Ext.value(node, false)) {
        return [];
    }
    if (node.isLeaf()) {
        return node;
    } else {
        node.eachChild(
         function (Mynode) {
             allNodes = allNodes.concat(Mynode.childNodes);
         }
        );
    }
    return allNodes;
}

var deleteSelectedCompanies = function (item) {
    if (item == "yes") {
        App.grdCompany.deleteSelected();
        sortGrdCompany();
        frmChange();
    }
}

var deleteAllCompanies = function (item) {
    if (item == "yes") {
        App.grdCompany.store.removeAll();
        sortGrdCompany();
        frmChange();
    }
}

var treePanelBranch_checkChange = function (node, checked, eOpts) {
    node.childNodes.forEach(function (childNode) {
        childNode.set("checked", checked);
    });
}
///////////////////////////////////////////////////////////////

///////////////////////////////////////// TREE INVT VIEW ////////////////////////////////////
var beforenodedrop = function (node, data, overModel, dropPosition, dropFn) {
    if (Ext.isArray(data.records)) {
        var records = data.records;
        data.records = [];

        App.stoInvt.suspendEvents();
        addNode(records[0]);
        App.stoInvt.resumeEvents();
        //App.stoInvt.loadPage(1);
        App.grdInvt.view.refresh();
    }
};

var treePanelInvt_checkChange = function (node, checked) {
    if (node.hasChildNodes()) {
        node.eachChild(function (childNode) {
            childNode.set('checked', checked);
            treePanelInvt_checkChange(childNode, checked);
        });
    }

};

var btnInvtExpand_click = function (btn, e, eOpts) {
    App.treePanelInvt.getStore().suspendEvents();
    App.treePanelInvt.expandAll();
    App.treePanelInvt.getStore().resumeEvents();
};

var btnInvtCollapse_click = function (btn, e, eOpts) {
    App.treePanelInvt.collapseAll();
};

var addNode = function (node) {
    if (node.data.Type == "Invt") {
        var record = HQ.store.findInStore(App.grdInvt.store, ['InvtID'], [node.data.InvtID]);
        if (!record) {
            HQ.store.insertBlank(App.stoOM21700, 'InvtID');
            record = App.stoOM21700.getAt(App.grdOM21700.store.getCount() - 1);
            record.set('InvtID', node.data.InvtID);
            record.set('Descr', node.data.Descr);
            record.set('Mark', node.data.Mark);
        }
        var record = App.stoInvt.getAt(App.stoInvt.getCount() - 1);
    }
    else if (node.childNodes) {
        node.childNodes.forEach(function (itm) {
            addNode(itm);
        });
    }
    frmChange();
};

var btnAddAllInvt_click = function (btn, e, eOpts) {
    if (_crrDiscCode == '') {
        App.tabDetail.setActiveTab(0);
        HQ.message.show(2016090911);
        return false;
    }
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            if (HQ.isUpdate) {
                var allNodes = getLeafNodes(App.treePanelInvt.getRootNode());
                if (allNodes && allNodes.length > 0) {
                    App.stoInvt.suspendEvents();
                    allNodes.forEach(function (node) {
                        if (node.data.Type == "Invt") {
                            var record = HQ.store.findRecord(App.stoInvt, ['DiscCode', 'InvtID'], [_crrDiscCode, node.data.InvtID]); // App.grdOM21700.store
                            if (!record) {
                                HQ.store.insertBlank(App.stoInvt, ['InvtID']);
                                record = App.stoInvt.getAt(App.grdInvt.store.getCount() - 1);
                                record.set('DiscCode', _crrDiscCode);
                                record.set('InvtID', node.data.InvtID);
                                record.set('InvtName', node.data.Descr);
                                record.set('InvtType', node.data.InvtType);
                                record.set('Mark', node.data.Mark);
                            } else {
                                var newInvtType = record.data.InvtType == 'I' ? 'P' : 'I';
                                var newRecord = HQ.store.findRecord(App.stoInvt, ['DiscCode', 'InvtID', 'InvtType'], [_crrDiscCode, node.data.InvtID, newInvtType]);
                                if (!newRecord) {
                                    HQ.store.insertBlank(App.stoInvt, ['InvtID']);
                                    newRecord = App.stoInvt.getAt(App.grdInvt.store.getCount() - 1);
                                    newRecord.set('DiscCode', _crrDiscCode);
                                    newRecord.set('InvtID', node.data.InvtID);
                                    newRecord.set('InvtName', node.data.Descr);
                                    newRecord.set('InvtType', newInvtType);
                                    newRecord.set('Mark', node.data.Mark);
                                }
                            }
                        }
                    });
                    App.stoInvt.resumeEvents();
                    App.stoInvt.loadPage(1);
                    App.grdInvt.view.refresh();

                    var record = App.stoInvt.getAt(App.stoInvt.getCount() - 1);
                    App.treePanelInvt.clearChecked();
                    frmChange();
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        }
        else {
            showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnAddInvt_click = function (btn, e, eOpts) {
    if (_crrDiscCode == '') {
        App.tabDetail.setActiveTab(0);
        HQ.message.show(2016090911);
        return false;
    }
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
                var allNodes = App.treePanelInvt.getCheckedNodes();
                if (allNodes && allNodes.length > 0) {
                    App.stoInvt.suspendEvents();
                    allNodes.forEach(function (node) {
                        if (node.attributes.Type == "Invt") {
                            var record = HQ.store.findRecord(App.stoInvt, ['DiscCode', 'InvtID'], [_crrDiscCode, node.attributes.InvtID]);
                            if (!record) {
                                HQ.store.insertBlank(App.stoInvt, ['InvtID']);
                                record = App.stoInvt.getAt(App.grdInvt.store.getCount() - 1);
                                record.set('DiscCode', _crrDiscCode);
                                record.set('InvtID', node.attributes.InvtID);
                                record.set('InvtName', node.attributes.Descr);
                                record.set('InvtType', node.attributes.InvtType);
                                record.set('Mark', node.attributes.Mark);
                            } else {
                                var newInvtType = record.data.InvtType == 'I' ? 'P' : 'I';
                                var newRecord = HQ.store.findRecord(App.stoInvt, ['DiscCode', 'InvtID', 'InvtType'], [_crrDiscCode, node.attributes.InvtID, newInvtType]);
                                if (!newRecord) {
                                    HQ.store.insertBlank(App.stoInvt, ['InvtID']);
                                    newRecord = App.stoInvt.getAt(App.grdInvt.store.getCount() - 1);
                                    newRecord.set('DiscCode', _crrDiscCode);
                                    newRecord.set('InvtID', node.attributes.InvtID);
                                    newRecord.set('InvtName', node.attributes.Descr);
                                    newRecord.set('InvtType', newInvtType);
                                    newRecord.set('Mark', node.attributes.Mark);
                                }
                            }
                        }
                    });
                    App.stoInvt.resumeEvents();
                    //  App.stoInvt.loadPage(1);
                    App.grdInvt.view.refresh();

                    App.treePanelInvt.clearChecked();
                }
                frmChange();
        }
        else {
            showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDelInvt_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var selRecs = App.grdInvt.selModel.selected.items;
            if (selRecs.length > 0) {
                var params = [];
                selRecs.forEach(function (record) {
                    params.push(record.data.InvtID);
                });
                HQ.message.show(2015020806,
                    params.join(" & ") + ",",
                    'deleteSelectedInvt');
            }            
        }
        else {
            showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDelAllInvt_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            HQ.message.show(2015020806, '', 'deleteAllInvts');            
        }
        else {
            showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var getDeepAllLeafNodes = function (node, onlyLeaf) {
    var allNodes = new Array();
    if (!Ext.value(node, false)) {
        return [];
    }
    if (node.isLeaf()) {
        return node;
    } else {
        node.eachChild(
         function (Mynode) {
             allNodes = allNodes.concat(Mynode.childNodes);
         }
        );
    }
    return allNodes;
};

var getLeafNodes = function (node) {
    var childNodes = [];
    node.eachChild(function (child) {
        if (child.isLeaf()) {
            childNodes.push(child);
        }
        else {
            var children = getLeafNodes(child);
            if (children.length) {
                children.forEach(function (nill) {
                    childNodes.push(nill);
                });
            }
        }
    });
    return childNodes;
};

var deleteSelectedLoc = function (item) {
    if (item == "yes") {
        App.grdLoc.deleteSelected();
        frmChange();
    }
};

var deleteSelectedInvt = function (item) {
    if (item == "yes") {
        App.grdInvt.deleteSelected();
        frmChange();
    }
};

var deleteAllInvts = function (item) {
    if (item == "yes") {
        App.stoInvt.removeAll();
        frmChange();
    }
};
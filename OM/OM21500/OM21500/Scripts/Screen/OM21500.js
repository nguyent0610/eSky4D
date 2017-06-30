//// Declare //////////////////////////////////////////////////////////

var keys = ['DiscCode'];
var fieldsCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate", "PromoType"];//, "ObjApply"
var fieldsLangCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate", "OM21500PromoType"];//, "OM21500ObjApply"
var _crrDiscCode = '';
var _cpnyTitle = '';
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 2;
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
                } else {
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
            HQ.common.close(this);
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
    //App.cboObjApply.getStore().addListener('load', checkLoad);
    App.cboCpnyID.getStore().addListener('load', checkLoad);

    App.cboPromoType.getStore().reload();
    //App.cboObjApply.getStore().reload();
    App.cboCpnyID.getStore().reload();
    _cpnyTitle = App.pnlAppComp.title;

    if (HQ.isInsert == false) {
      //  App.menuClickbtnNew.disable();
        App.btnAdd.disable();
        App.btnAddAll.disable();
    }
    if (HQ.isDelete == false) {
        App.btnDel.disable();
        App.btnDelAll.disable();
    }
    //if (HQ.isInsert == false && HQ.isDelete == false && HQ.isUpdate == false)
    //    App.menuClickbtnSave.disable();
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
    _crrDiscCode = "";
    setTitle();
    //if (!HQ.isInsert && HQ.isNew) {        
    //    HQ.common.lockItem(App.frmMain, true);
    //}
    //else if (!HQ.isUpdate && !HQ.isNew) {
    //    HQ.common.lockItem(App.frmMain, true);
    //}

    App.stoDescCpny.reload();
};
var stoDescCpny_Load = function (sto) {    
    frmChange();
    if (_isLoadMaster) {        
        HQ.common.showBusy(false);
    }
    filterGridByDiscCode();
}
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdOM_DiscDescr_BeforeEdit = function (editor, e) {
    
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
    }
    else if (e.field == 'DiscCode') {
        _crrDiscCode = e.record.data.DiscCode;
        setTitle();
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
    filterGridByDiscCode();
    frmChange();
    setTitle();
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
    }
    if (isBreak) {
        filterGridByDiscCode();
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
                lstCpny: Ext.encode(lstCpny.getRecordsValues())
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
        _crrDiscCode = "";
        setTitle();
        App.grdCompany.store.removeAll();
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
        } else {
            App.stoDescCpny.reload();
        }
        setTitle();
    }
};

var PromoType_render = function (value) {
    var record = HQ.store.findRecord(App.cboPromoType.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

//var ObjApply_render = function (value) {
//    var record = HQ.store.findRecord(App.cboObjApply.store, ["Code"], [value]);
//    return (record) ? record.data.Descr : value;
//};

var CpnyName_render = function (value) {
    var record = HQ.store.findRecord(App.cboCpnyID.store, ["CpnyID"], [value]);
    return (record) ? record.data.CpnyName : value;
};

var setTitle = function () {
    var title = _cpnyTitle;
    if (_crrDiscCode != '') {
        title = _cpnyTitle + ' (' + App.grdOM_DiscDescr.columns[1].text + ':' + _crrDiscCode + ')';
    }
    else
        title = _cpnyTitle;
    App.pnlAppComp.setTitle(title);
}
var stringFilter = function (record) {
    if (HQ.focus == 'header') {
        if (this.dataIndex == 'PromoType') {
            App.cboPromoType.store.clearFilter();
            return HQ.grid.filterComboDescr(record, this, App.cboPromoType.store, "Code", "Descr");
        }
        //else if (this.dataIndex == 'ObjApply') {
        //    App.cboObjApply.store.clearFilter();
        //    return HQ.grid.filterComboDescr(record, this, App.cboObjApply.store, "Code", "Descr");
        //}
    }
    //else if (this.dataIndex == 'Zone') {
    //    App.cboZone.store.clearFilter();
    //    return HQ.grid.filterComboDescr(record, this, App.cboZone.store, "Code", "Descr");
    //}

    //else
    return HQ.grid.filterString(record, this);
}

var filterGridByDiscCode = function () {
    App.stoDescCpny.suspendEvents();
    App.grdCompany.getFilterPlugin().clearFilters();
    App.grdCompany.getFilterPlugin().getFilter('DiscCode').setValue([_crrDiscCode, '']);
    App.grdCompany.getFilterPlugin().getFilter('DiscCode').setActive(true);
    sortGrdCompany();
    App.stoDescCpny.resumeEvents();
    App.grdCompany.view.refresh();
}
var checkFromDate = function () {
};
var checkToDate = function () {
};

var sortGrdCompany = function () {
    App.grdCompany.store.sort('CpnyID', 'ASC');
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
var btnAddAll_click = function (btn, e, eOpts) {
    if (_crrDiscCode == '') {
        App.tabDetail.setActiveTab(0);
        HQ.message.show(2016090911);
        return false;
    }
    if (HQ.isInsert) {
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

var btnAdd_click = function (btn, e, eOpts) {
    if (_crrDiscCode == '') {
        App.tabDetail.setActiveTab(0);
        HQ.message.show(2016090911);
        return false;
    }
    if (HQ.isInsert) {
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

var btnDel_click = function (btn, e, eOpts) {
    if (HQ.isDelete) {
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

var btnDelAll_click = function (btn, e, eOpts) {
    if (HQ.isDelete) {
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

//// Declare //////////////////////////////////////////////////////////

var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID","WrkOpenDate", "WrkAdjDate"];
var fieldsLangCheckRequire = ["BranchID"];
var _territory = '';
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_CloseDateSetUp);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_CloseDateSetUp);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_CloseDateSetUp);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_CloseDateSetUp);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_CloseDateSetUp.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_CloseDateSetUp, keys);
            }
            break;
        case "delete":
           
                if (App.slmSYS_CloseDateSetUp.selected.items[0] != undefined) {
                    //if (HQ.isDelete) {
                    //    HQ.message.show(11, '', 'deleteData');
                    //}
                    if (HQ.isDelete) {
                        var selRecs = App.grdSYS_CloseDateSetUp.selModel.selected.items;
                        if (selRecs.length > 0) {
                            var params = [];
                            selRecs.forEach(function (record) {
                                params.push(record.data.BranchID);
                            });
                            HQ.message.show(2015020806,
                                params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                                'deleteSelectedCompanies');
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                }
            
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_CloseDateSetUp, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var btnUpdate_Click = function (sender, e) {
    if (App.frmMain.isValid()) {
        var store = App.stoSYS_CloseDateSetUp;
        var allRecords = store.snapshot || store.allData || store.data;
        store.suspendEvents();
        allRecords.each(function (record) {
            if (!Ext.isEmpty(record.data.BranchID))
               // record.set('WrkDateChk', value);
            record.set("WrkDateChk", App.chkWrkDateChk.checked);
            record.set("WrkUpperDays", App.lblWrkUpperDays.getValue());
            record.set("WrkLowerDays", App.lblWrkLowerDays.getValue());
            record.set("WrkOpenDate", App.lblWrkOpenDate.getValue());
            record.set("WrkAdjDate", App.lblWrkAdjDate.getValue());
        });
        store.resumeEvents();
        App.grdSYS_CloseDateSetUp.view.refresh();

        //App.grdSYS_CloseDateSetUp.getStore().each(function (item) {
        //    item.set("WrkDateChk", App.chkWrkDateChk.checked);
        //    item.set("WrkUpperDays", App.lblWrkUpperDays.getValue());
        //    item.set("WrkLowerDays", App.lblWrkLowerDays.getValue());
        //    item.set("WrkOpenDate", App.lblWrkOpenDate.getValue());
        //    item.set("WrkAdjDate", App.lblWrkAdjDate.getValue());
        //});
    }
    else if (!App.lblWrkAdjDate.isValid()) {

        HQ.message.show(15, HQ.common.getLang("WRKADJDATE"), '');
    }
    else if (!App.lblWrkOpenDate.isValid()) {

        HQ.message.show(15, HQ.common.getLang("WRKOPENDATE"), '');
    };
};

var beforenodedrop = function (node, data, overModel, dropPosition, dropFn) {
    if (Ext.isArray(data.records)) {
        var records = data.records;

        data.records = [];
        HQ.store.insertBlank(App.stoSYS_CloseDateSetUp, keys);
        addNode(records[0]);
    }
};

var treePanelBranch_checkChange = function (node, checked, eOpts) {
    node.childNodes.forEach(function (childNode) {
        childNode.set("checked", checked);
    });
};

var btnExpand_click = function (btn, e, eOpts) {
    App.treePanelBranch.expandAll();
};

var btnCollapse_click = function (btn, e, eOpts) {
    App.treePanelBranch.collapseAll();
};

var btnAddAll_click = function (btn, e, eOpts) {
    if (HQ.isInsert) {
        var allNodes = getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
        if (allNodes && allNodes.length > 0) {
            allNodes.forEach(function (node) {
                HQ.store.insertBlank(App.stoSYS_CloseDateSetUp, keys);
                if (node.data.Type == "Company") {
                    var record = HQ.store.findInStore(App.grdSYS_CloseDateSetUp.store,
                        ['BranchID'],
                        [node.data.RecID]);
                    if (!record) {
                        record = App.stoSYS_CloseDateSetUp.getAt(App.grdSYS_CloseDateSetUp.store.getCount() - 1);
                        record.set('BranchID', node.data.RecID);
                        record.set('WrkAdjDate', new Date(_dateServer));
                        record.set('WrkOpenDate', new Date(_dateServer));
                    }
                }
            });
            HQ.store.insertBlank(App.stoSYS_CloseDateSetUp, keys);
            var record = App.stoSYS_CloseDateSetUp.getAt(App.stoSYS_CloseDateSetUp.getCount() - 1);
            record.set('WrkAdjDate', new Date(_dateServer));
            record.set('WrkOpenDate', new Date(_dateServer));
            App.treePanelBranch.clearChecked();
        }

    }
    else {
        HQ.message.show(4, '', '');
    }
};
var addNode = function (node) {
    if (node.data.Type == "Company") {
        var record = HQ.store.findInStore(App.grdSYS_CloseDateSetUp.store,
            ['BranchID'],
            [node.data.RecID]);
        if (!record) {
            record = App.stoSYS_CloseDateSetUp.getAt(App.grdSYS_CloseDateSetUp.store.getCount() - 1);
            record.set('BranchID', node.data.RecID);
            record.set('WrkAdjDate', new Date(_dateServer));
            record.set('WrkOpenDate', new Date(_dateServer));

        }
        HQ.store.insertBlank(App.stoSYS_CloseDateSetUp, keys);
        var record = App.stoSYS_CloseDateSetUp.getAt(App.stoSYS_CloseDateSetUp.getCount() - 1);
        record.set('WrkAdjDate', new Date(_dateServer));
        record.set('WrkOpenDate', new Date(_dateServer));
    }
    else if (node.childNodes) {
        node.childNodes.forEach(function (itm) {
            addNode(itm);
        });
    }
}
var btnAdd_click = function (btn, e, eOpts) {
    if (HQ.isInsert) {
        var allNodes = App.treePanelBranch.getCheckedNodes();
        if (allNodes && allNodes.length > 0) {
            allNodes.forEach(function (node) {
                HQ.store.insertBlank(App.stoSYS_CloseDateSetUp, keys);
                if (node.attributes.Type == "Company") {
                    var record = HQ.store.findInStore(App.grdSYS_CloseDateSetUp.store,
                        ['BranchID'],
                        [node.attributes.RecID]);
                    if (!record) {
                        record = App.stoSYS_CloseDateSetUp.getAt(App.grdSYS_CloseDateSetUp.store.getCount() - 1);
                        record.set('BranchID', node.attributes.RecID);
                        record.set('WrkAdjDate', new Date(_dateServer));
                        record.set('WrkOpenDate', new Date(_dateServer));
                    }
                }
            });
            HQ.store.insertBlank(App.stoSYS_CloseDateSetUp, keys);
            var record = App.stoSYS_CloseDateSetUp.getAt(App.stoSYS_CloseDateSetUp.getCount() - 1);
            record.set('WrkAdjDate', new Date(_dateServer));
            record.set('WrkOpenDate', new Date(_dateServer));
            App.treePanelBranch.clearChecked();
        }

    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDel_click = function (btn, e, eOpts) {
    if (HQ.isDelete) {
        var selRecs = App.grdSYS_CloseDateSetUp.selModel.selected.items;
        if (selRecs.length > 0) {
            var params = [];
            selRecs.forEach(function (record) {
                params.push(record.data.BranchID);
            });
            HQ.message.show(2015020806,
                params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                'deleteSelectedCompanies');
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDelAll_click = function (btn, e, eOpts) {
    if (HQ.isDelete) {
        HQ.message.show(20160310, '', 'deleteAllCompanies');
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

var deleteSelectedCompanies = function (item) {
    if (item == "yes") {
        App.grdSYS_CloseDateSetUp.deleteSelected();
    }
};

var deleteAllCompanies = function (item) {
    if (item == "yes") {
        App.grdSYS_CloseDateSetUp.store.removeAll();
    }
};

var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchIDSA40000_pcCompany.findRecord("CpnyID", rec.data.BranchID);
    if (record) {
        return record.data.CpnyName;
    }
    else {
        return value;
    }
};

var renderTerritory = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchIDSA40000_pcCompany.findRecord("CpnyID", rec.data.BranchID);
    if (record) {
        return record.data.Territory;
    }
    else {
        return value;
    }
};

var renderAddress = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchIDSA40000_pcCompany.findRecord("CpnyID", rec.data.BranchID);
    if (record) {
        return record.data.Address;
    }
    else {
        return value;
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    HQ.util.checkAccessRight();
    if (!HQ.isUpdate) {
        App.btnUpdate.disable();
    }
    App.stoSYS_CloseDateSetUp.reload();
    //App.cboBranchID.store.load(function () {
    //    //App.stoSYS_CloseDateSetUp.reload();

    //});
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA40000');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA40000');
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoSYS_CloseDateSetUp, keys, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    //if (HQ.isFirstLoad) {
    //    if (HQ.isInsert) {
    //        HQ.store.insertRecord(sto, keys, { WrkAdjDate: new Date(_dateServer), WrkOpenDate: new Date(_dateServer) });
    //    }
    //    HQ.isFirstLoad = false;
    //}
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdSYS_CloseDateSetUp_BeforeEdit = function (editor, e) {
    if (e.field == "BranchID") {
        _territory = e.record.data.Territory;
        App.cboBranchID.store.reload();
    }
    else {
        _territory = '';
    }

    if (e.field == "Territory")
    {
        return true;
    }
    else {
        return HQ.grid.checkBeforeEdit(e, keys);
    }
    
};
var grdSYS_CloseDateSetUp_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_CloseDateSetUp, e, keys);
    var record = App.stoSYS_CloseDateSetUp.getAt(App.stoSYS_CloseDateSetUp.getCount() - 1);
    if (Ext.isEmpty(record.data.BranchID)) {
        record.set('WrkAdjDate', new Date(_dateServer));
        record.set('WrkOpenDate', new Date(_dateServer));
    }

    //if (e.field == "BranchID") {
    //    var selectedRecord = App.cboBranchID.store.findRecord('CpnyID', e.value);
    //    if (selectedRecord) {
    //        e.record.set("Territory", selectedRecord.data.Territory);
    //        e.record.set("BranchName", selectedRecord.data.CpnyName);
    //        e.record.set("Address", selectedRecord.data.Address);
    //    }
    //    else {
    //        e.record.set("Territory", "");
    //        e.record.set("BranchName", "");
    //        e.record.set("Address", "");
    //    }
    //}
};
var grdSYS_CloseDateSetUp_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_CloseDateSetUp, e, keys);
};
var grdSYS_CloseDateSetUp_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_CloseDateSetUp);
    stoChanged(App.stoSYS_CloseDateSetUp);
};


/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA40000/Save',
            params: {
                lstSYS_CloseDateSetUp: HQ.store.getData(App.stoSYS_CloseDateSetUp)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSYS_CloseDateSetUp.deleteSelected();
        stoChanged(App.stoSYS_CloseDateSetUp);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_CloseDateSetUp.reload();
    }
};

//Filter dung trong truong hop co treeview
var myValidateRecord = function (filter, record, columnName) {
    var filterValue = filter.getValue();
    if (filterValue.length == 0) {
        return true;
    }
    var values = filterValue.split(",");
    for (var i = 0; i < values.length; i++) {

        if (record.get(columnName).indexOf(values[i].trim()) > -1) {
            return true;
        }
    }
    return false;
};
/////////////////////////////////////////////////////////////////////////

var chkSelectHeaderkWrkDateChk_change = function (sender, value, oldValue) {
    if (HQ.isUpdate)  {
        var store = App.stoSYS_CloseDateSetUp;
        var allRecords = store.snapshot || store.allData || store.data;
        store.suspendEvents();
        allRecords.each(function (record) {
            if (!Ext.isEmpty(record.data.BranchID))
                record.set('WrkDateChk', value);
        });
        store.resumeEvents();
        App.grdSYS_CloseDateSetUp.view.refresh();
    }
    else
        App.chkSelectHeaderkWrkDateChk.setValue(false);
};
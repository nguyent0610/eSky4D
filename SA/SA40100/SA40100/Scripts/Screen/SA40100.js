//// Declare ///////////////////////////////////////////////////////////
var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID"];
var fieldsLangCheckRequire = ["BranchID"];
////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_CloseDateHistDetail);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_CloseDateHistDetail);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_CloseDateHistDetail);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_CloseDateHistDetail);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_CloseDateHistDetail.reload();
                App.cboHistID.store.reload();
            }
            break;
        case "new":
            App.pnlLeft.show();
            App.cboTask.setDisabled(false);
            App.lblDate.setDisabled(false);
            App.btnProcess.setDisabled(false);
            App.pnlDetail.hide();
            App.cboHistID.setValue('');
            App.lblDate.setValue(_dateServer);
            break;
        case "delete":
            if (App.slmSYS_CloseDateHistDetail.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if(App.stoSYS_CloseDateHistDetail.getCount() == 0){//if (HQ.store.getAllData(App.stoSYS_CloseDateHistDetail).length == 2) {
                    HQ.message.show(20405);
                }
                else {
                    if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoSYS_CloseDateHistDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                        save();
                    }
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
var loadData = function () {
    if (App.stoSYS_CloseDateHistHeader.getCount() == 0) {
        // If has no record then create a new
        App.stoSYS_CloseDateHistHeader.insert(0, Ext.data.Record());
    }
    var record = App.stoSYS_CloseDateHistHeader.getAt(0);
    App.frmMain.getForm().loadRecord(record);
};

var slmSYS_CloseDateHistDetail_SelectionChange = function (mdl, selected, eOpts) {
    if (selected.length) {
        App.tplDetail.overwrite(App.pnlDetail.body, selected[0].data);
    }
};

var cboHistID_Select = function (sender, e) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoSYS_CloseDateHistDetail.loading) {
        if (e) {
            App.pnlLeft.hide();
            App.cboTask.setDisabled(true);
            App.lblDate.setDisabled(true);
            App.btnProcess.setDisabled(true);
            App.pnlDetail.show();
            App.tplDetail.overwrite(App.pnlDetail.body, '');
            App.grdSYS_CloseDateHistDetail.getSelectionModel().clearSelections();
            App.stoSYS_CloseDateHistDetail.reload();
        }

    }
}
var cboHistID_Change = function (sender, e) {
    if (!e) {
        App.pnlLeft.show();
        App.cboTask.setDisabled(false);
        App.lblDate.setDisabled(false);
        App.btnProcess.setDisabled(false);
        App.pnlDetail.hide();
        App.tplDetail.overwrite(App.pnlDetail.body, '');
        App.grdSYS_CloseDateHistDetail.getSelectionModel().clearSelections();
        App.stoSYS_CloseDateHistDetail.reload();
    }

};
var btnProcess_Click = function (sender, e) {
    menuClick('save');
    //if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
    //    if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoSYS_CloseDateHistDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
    //        save();
    //    }
    //}
};

/////////////////// TREE /////////////////////

var beforenodedrop = function (node, data, overModel, dropPosition, dropFn) {
    if (Ext.isArray(data.records)) {
        var records = data.records;
        data.records = [];
        //HQ.store.insertBlank(App.stoSYS_CloseDateHistDetail, keys);
        App.stoSYS_CloseDateHistDetail.suspendEvents();
        addNode(records[0]);
        App.stoSYS_CloseDateHistDetail.resumeEvents();
        App.grdSYS_CloseDateHistDetail.view.refresh();
        App.stoSYS_CloseDateHistDetail.loadPage(1);
    }
};

var treePanelBranch_checkChange = function (node, checked, eOpts) {
    //node.childNodes.forEach(function (childNode) {
    //    childNode.set("checked", checked);
    //});
    if (node.hasChildNodes()) {
        node.eachChild(function (childNode) {
            childNode.set('checked', checked);
            treePanelBranch_checkChange(childNode, checked);
        });
    }
};

var btnExpand_click = function (btn, e, eOpts) {
    App.treePanelBranch.expandAll();
};

var btnCollapse_click = function (btn, e, eOpts) {
    App.treePanelBranch.collapseAll();
};

var btnAddAll_click = function (btn, e, eOpts) {
    if (HQ.isInsert) {
        //var allNodes = getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
        var allNodes = getLeafNodes(App.treePanelBranch.getRootNode());
        if (allNodes && allNodes.length > 0) {
            App.stoSYS_CloseDateHistDetail.suspendEvents();
            allNodes.forEach(function (node) {
                if (node.data.Type == "Company") {
                    var record = HQ.store.findRecord(App.grdSYS_CloseDateHistDetail.store,
                        ['BranchID'],
                        [node.data.RecID]);
                    if (!record) {
                        var oldDate = HQ.store.findRecord(App.stoGetDayCloseDateSetUp, ['BranchID'], [node.data.RecID]);
                        if (oldDate != null) {
                            HQ.store.insertBlank(App.stoSYS_CloseDateHistDetail, keys);
                            record = App.stoSYS_CloseDateHistDetail.getAt(App.grdSYS_CloseDateHistDetail.store.getCount() - 1);
                            record.set('BranchID', node.data.RecID);
                            record.set('WrkDateChk', oldDate.data.WrkDateChk);
                            record.set('WrkAdjDateBefore', oldDate.data.WrkAdjDate);
                            record.set('WrkOpenDateBefore', oldDate.data.WrkOpenDate);
                            record.set('WrkAdjDateAfter', new Date(App.lblDate.getValue()));
                            record.set('WrkOpenDateAfter', new Date(App.lblDate.getValue()));
                            record.set('WrkLowerDays', oldDate.data.WrkLowerDays);
                            record.set('WrkUpperDays', oldDate.data.WrkUpperDays);
                        }
                    }
                }
            });
            App.stoSYS_CloseDateHistDetail.resumeEvents();
            App.grdSYS_CloseDateHistDetail.view.refresh();
            App.stoSYS_CloseDateHistDetail.loadPage(1);
            App.treePanelBranch.clearChecked();
        }

    }
    else {
        HQ.message.show(4, '', '');
    }
};

var addNode = function (node) {
    if (HQ.isInsert) {
        if (node.data.Type == "Company") {
            var record = HQ.store.findRecord(App.grdSYS_CloseDateHistDetail.store,
                ['BranchID'],
                [node.data.RecID]);
            if (!record) {
                var oldDate = HQ.store.findRecord(App.stoGetDayCloseDateSetUp, ['BranchID'], [node.data.RecID]);
                if (oldDate != null) {
                    HQ.store.insertBlank(App.stoSYS_CloseDateHistDetail, keys);
                    record = App.stoSYS_CloseDateHistDetail.getAt(App.grdSYS_CloseDateHistDetail.store.getCount() - 1);
                    record.set('BranchID', node.data.RecID);
                    record.set('WrkDateChk', oldDate.data.WrkDateChk);
                    record.set('WrkAdjDateBefore', oldDate.data.WrkAdjDate);
                    record.set('WrkOpenDateBefore', oldDate.data.WrkOpenDate);
                    record.set('WrkAdjDateAfter', new Date(App.lblDate.getValue()));
                    record.set('WrkOpenDateAfter', new Date(App.lblDate.getValue()));
                    record.set('WrkLowerDays', oldDate.data.WrkLowerDays);
                    record.set('WrkUpperDays', oldDate.data.WrkUpperDays);
                }
            }
        }
        else if (node.childNodes) {
            node.childNodes.forEach(function (itm) {
                addNode(itm);
            });
        }
    }
};

var btnAdd_click = function (btn, e, eOpts) {
    if (HQ.isInsert) {
        var allNodes = App.treePanelBranch.getCheckedNodes();
        if (allNodes && allNodes.length > 0) {
            App.stoSYS_CloseDateHistDetail.suspendEvents();
            allNodes.forEach(function (node) {
                if (node.attributes.Type == "Company") {
                    var record = HQ.store.findRecord(App.grdSYS_CloseDateHistDetail.store,
                        ['BranchID'],
                        [node.attributes.RecID]);
                    if (!record) {
                        var oldDate = HQ.store.findRecord(App.stoGetDayCloseDateSetUp, ['BranchID'], [node.attributes.RecID]);
                        if (oldDate != null) {
                            HQ.store.insertBlank(App.stoSYS_CloseDateHistDetail, keys);
                            record = App.stoSYS_CloseDateHistDetail.getAt(App.grdSYS_CloseDateHistDetail.store.getCount() - 1);
                            record.set('BranchID', node.attributes.RecID);
                            record.set('WrkDateChk', oldDate.data.WrkDateChk);
                            record.set('WrkAdjDateBefore', oldDate.data.WrkAdjDate);
                            record.set('WrkOpenDateBefore', oldDate.data.WrkOpenDate);
                            record.set('WrkAdjDateAfter', new Date(App.lblDate.getValue()));
                            record.set('WrkOpenDateAfter', new Date(App.lblDate.getValue()));
                            record.set('WrkLowerDays', oldDate.data.WrkLowerDays);
                            record.set('WrkUpperDays', oldDate.data.WrkUpperDays);
                        }
                    }
                }
            });
            App.stoSYS_CloseDateHistDetail.resumeEvents();
            App.grdSYS_CloseDateHistDetail.view.refresh();
            App.stoSYS_CloseDateHistDetail.loadPage(1);
            App.treePanelBranch.clearChecked();
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDel_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        var selRecs = App.grdSYS_CloseDateHistDetail.selModel.selected.items;
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
        HQ.message.show(4, '', '');
    }
};

var btnDelAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        HQ.message.show(11, '', 'deleteAllCompanies');
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
var deleteSelectedCompanies = function (item) {
    if (item == "yes") {
        App.grdSYS_CloseDateHistDetail.deleteSelected();
    }
};

var deleteAllCompanies = function (item) {
    if (item == "yes") {
        HQ.isChange = false;
        App.grdSYS_CloseDateHistDetail.store.removeAll();
    }
};

var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchIDSA40100_pcCompany.findRecord("CpnyID", rec.data.BranchID);
    if (record) {
        return record.data.CpnyName;
    }
    else {
        return value;
    }
};

var renderStatus = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboStatusSA40100_pcStatus.findRecord("Code", rec.data.Status);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.cboBranchID.store.load(function () {
        App.stoSYS_CloseDateHistDetail.reload();
    });
};
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA40100');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA40100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertRecord(sto, keys, { WrkAdjDate: new Date(_dateServer), WrkOpenDate: new Date(_dateServer) });
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdSYS_CloseDateHistDetail_BeforeEdit = function (editor, e) {
    //return HQ.grid.checkBeforeEdit(e, keys);
    return false;
};
var grdSYS_CloseDateHistDetail_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_CloseDateHistDetail, e, keys);
};
var grdSYS_CloseDateHistDetail_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_CloseDateHistDetail, e, keys);
};
var grdSYS_CloseDateHistDetail_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_CloseDateHistDetail);
    stoChanged(App.stoSYS_CloseDateHistDetail);
};


/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA40100/Save',
            params: {
                lstSYS_CloseDateHistHeader: HQ.store.getData(App.frmMain.getRecord().store),
                lstSYS_CloseDateHistDetail: HQ.store.getData(App.stoSYS_CloseDateHistDetail)
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                var HistID = data.result.HistID;
                App.cboHistID.getStore().load(function () {
                    App.cboHistID.setValue(HistID);
                    App.stoSYS_CloseDateHistDetail.reload();
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSYS_CloseDateHistDetail.deleteSelected();
        stoChanged(App.stoSYS_CloseDateHistDetail);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_CloseDateHistDetail.reload();
        App.cboHistID.store.reload();
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

var getRowClass = function (record, index) {
    if (record.data.Status == 'H') {
        return "row-red";// + record.data.color;
    }
    else if (record.data.tstamp == "") {
        return "row-yellow";// + record.data.color;
    }
    else return "row-none";
};
/////////////////////////////////////////////////////////////////////////
var dates = {
    convert: function (d) {
        return (
            d.constructor === Date ? d :
            d.constructor === Array ? new Date(d[0], d[1], d[2]) :
            d.constructor === Number ? new Date(d) :
            d.constructor === String ? new Date(d) :
            typeof d === "object" ? new Date(d.year, d.month, d.date) :
            NaN
        );
    }
};

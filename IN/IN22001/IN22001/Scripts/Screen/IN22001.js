
var Process = {
    saveData: function () {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            url: 'IN22001/SaveData',
            waitMsg: HQ.common.getLang('Submiting') + "...",
            timeout: 1800000,
            params: {
                lstPosm: Ext.encode(App.stoPOSM.getRecordsValues()),
                lstDetChange: HQ.store.getData(App.grdDet.store),
                isNew: HQ.isNew
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                HQ.isChange = false;
                App.cboPosmID.store.load(function (records, operation, success) {
                    App.stoPOSM.reload();
                });
            },
            failure: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            }
        });
    },

    deletePosm: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'IN22001/DeletePosm',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    posmID: App.cboPosmID.getValue()//,
                    //isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboPosmID.store.load();
                    App.cboPosmID.clearValue();
                },
                failure: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
    },

    deleteBranch: function (item) {
        if (item == "yes") {
            App.grdDet.deleteSelected();
        }
    },

    showFieldInvalid: function (form) {
        var done = 1;
        form.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                HQ.message.show(15, field.fieldLabel, '');
                done = 0;
                return false;
            }
        });
        return done;
    },

    //kiem tra key da nhap du chua
    isAllValidKey: function (items, keys) {
        if (items != undefined) {
            for (var i = 0; i < items.length; i++) {
                for (var j = 0; j < keys.length; j++) {
                    if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
                        return false;
                }
            }
            return true;
        } else {
            return true;
        }
    },

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            App.stoDet.reload();
            Event.Form.menuClick("refresh");
        }
    },
};

var Store = {
    stoPOSM_load: function (sto, records, successful, eOpts) {
        HQ.isNew = false;
        if (sto.getCount() == 0) {
            var newPosm = Ext.create("App.mdlPOSM", {
                Active: true,
                FromDate: HQ.dateNow,
                ToDate: HQ.dateNow
            });
            sto.insert(0, newPosm);
            HQ.isNew = true;
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);

        App.grdDet.store.reload();

        Event.Form.frmMain_fieldChange();
    },

    stoDet_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var frmRec = App.frmMain.getRecord();
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

            if (frmRec && frmRec.data.Active) {
                if (successful) {
                    var record = HQ.store.findRecord(App.grdDet.store, ['BranchID'], ['']);
                    if (!record) {
                        HQ.store.insertBlank(sto, keys);
                    }
                }
            }
        }
    }
};

var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            HQ.common.setRequire(frm);
            App.stoPOSM.reload();
        },

        frmMain_fieldChange: function () {
            if (App.stoPOSM.getCount() > 0) {
                App.frmMain.updateRecord();
                if (!HQ.store.isChange(App.stoPOSM)) {
                    HQ.isChange = HQ.store.isChange(App.grdDet.store);
                }
                else {
                    HQ.isChange = true;
                }

                HQ.common.changeData(HQ.isChange, 'IN22001');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                App.cboPosmID.setReadOnly(HQ.isChange);
            }
        },

        btnImport_click: function (btn, e) {
            Ext.Msg.alert("Import", "Coming soon!");
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        cboPosmID_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoPOSM.reload();
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(dtp.value);
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'posm') {
                        HQ.combo.first(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grdDet') {
                        HQ.grid.first(App.grdDet);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'posm') {
                        HQ.combo.next(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grdDet') {
                        HQ.grid.next(App.grdDet);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'posm') {
                        HQ.combo.prev(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grdDet') {
                        HQ.grid.prev(App.grdDet);
                    }
                    break;
                case "last":
                    if (HQ.focus == 'posm') {
                        HQ.combo.last(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grdDet') {
                        HQ.grid.last(App.grdDet);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'posm') {
                            App.cboPosmID.store.load(function (records, operation, success) {
                                App.stoPOSM.reload();
                            });
                        }
                        else if (HQ.focus == 'grdDet') {
                            App.grdDet.store.reload();
                        }
                    }
                    break;
                case "new":
                    if (HQ.isInsert) {
                        if (HQ.isChange) {
                            HQ.message.show(150, '', '');
                        }
                        else {
                            App.cboPosmID.clearValue();
                        }
                    }
                    break;
                case "save":
                    if (HQ.isUpdate || HQ.isCreate) {
                        if (App.frmMain.isValid()) {
                            var keys = App.grdDet.store.HQFieldKeys ? App.grdDet.store.HQFieldKeys : "";
                            if (HQ.store.checkRequirePass(App.grdDet.store, keys,["PosmID","BranchID"])) {
                                Process.saveData();
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
                case "delete":
                    if (HQ.isDelete) {
                        if (HQ.focus == 'posm') {
                            if (App.cboPosmID.getValue()) {
                                HQ.message.show(11, '', 'Process.deletePosm');
                            }
                        }
                        else if (HQ.focus == 'grdDet') {
                            if (App.cboPosmID.getValue() && App.slmDet.getCount()) {
                                HQ.message.show(2015020806, App.slmDet.selected.items[0].data.BranchID,'Process.deleteBranch');
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
            }
        }
    },

    Grid: {
        grdDet_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var frmRec = App.frmMain.getRecord();
                    if (frmRec && frmRec.data.Active) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
                        }
                       if (e.field == "BranchID") {
                           App.cboBranchID.store.reload();
                        }

                        return HQ.grid.checkInput(e, keys);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                    return false;
                }
            }
            else {
                return false;
            }
        },

        grd_reject: function (col, record) {
            var grd = col.up('grid');
            if (!record.data.tstamp) {
                grd.getStore().remove(record, grd);
                grd.getView().focusRow(grd.getStore().getCount() - 1);
                grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            } else {
                record.reject();
            }
        },

        grdDet_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    HQ.store.insertBlank(e.store, keys);
                }
            }
            if (e.field == 'BranchID') {               
                if (App.cboBranchID.valueModels != undefined && App.cboBranchID.valueModels.length > 0) {
                    e.record.set('PosmID', App.cboPosmID.getValue());
                    e.record.set('BranchID', App.cboBranchID.valueModels[0].data.CpnyID)
                    e.record.set('CpnyName', App.cboBranchID.valueModels[0].data.CpnyName);
                    e.record.set('CpnyType', App.cboBranchID.valueModels[0].data.CpnyType)
                    }
                }
        
            //Event.Form.frmMain_fieldChange();
        },

        grdDet_validateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                if (e.value && !e.value.match(regex)) {
                    HQ.message.show(20140811, e.column.text);
                    return false;
                }
                if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                    HQ.message.show(1112, e.value);
                    return false;
                }

            }
        }
    },
};
//-----TREE------

var tree_ItemCollapse = function (a, b) {
    collapseNode(a);
}//ham nay controller goi
var tree_AfterRender = function (id) {
    HQ.common.showBusy(true, HQ.waitMsg);
    App.direct.IN22001GetTreeBranch((id), {
        success: function (result) {
            App.treePanelCompany.getRootNode().expand();
            HQ.common.showBusy(false, HQ.waitMsg);
        }, failure: function (result) {
            HQ.common.showBusy(false, HQ.waitMsg);

        }
    });
}
getLeafNodes = function (node) {
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
}
var updateTreeView = function (tree, fn) {
    var view = tree.getView();
    view.getStore().loadRecords(fn(tree.getRootNode()));
    view.refresh();
};

/// ///////// Expand /////////////////////////////////////////////////////////////
var expandAll = function (tree) {
    this.updateTreeView(tree, function (root) {
        var nodes = [];
        root.cascadeBy(function (node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = true;
                nodes.push(node);
            }
        });
        return nodes;
    });
}
var expandNode = function (node) {
    if (node != undefined) {
        node.expand();
        if (node.childNodes.length > 0) {
            for (var i = 0; i < node.childNodes.length; i++) {
                expandNode(node.childNodes[i]);
            }
        }
    }
}
/// ///////// Collapse /////////////////////////////////////////////////////////////
var collapseAll = function (tree) {
    this.updateTreeView(tree, function (root) {
        root.cascadeBy(function (node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = false;
            }
        });
        return tree.rootVisible ? [root] : root.childNodes;
    });
}
var collapseNode = function (node) {
    if (node != undefined) {
        node.collapse();
        if (node.childNodes.length > 0) {
            for (var i = 0; i < node.childNodes.length; i++) {
                collapseNode(node.childNodes[i]);
            }
        }
    }
}
var treePanelCompany_checkChange = function (node, checked) { 
        if (node.hasChildNodes()) {
            node.eachChild(function (childNode) {
                childNode.set('checked', checked);
                treePanelCompany_checkChange(childNode, checked);
            });
    }
}
btnExpand_Click = function (btn, e, eOpts) {
    Ext.suspendLayouts();
    expandAll(App.treePanelCompany);
    Ext.resumeLayouts(true);
}
btnCollapse_Click = function (btn, e, eOpts) {
    Ext.suspendLayouts();
    collapseAll(App.treePanelCompany);
    Ext.resumeLayouts(true);
}
//---Button Add Tree----
var btnAdd_click = function (btn, e, eOpts) {   
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var isError = false;
            var allNodes = App.treePanelCompany.getCheckedNodes();
            if (allNodes && allNodes.length > 0) {
                App.grdDet.store.suspendEvents();
                var invtBlank = HQ.store.findRecord(App.grdDet.store, ['BranchID'], ['']);
                if (invtBlank) {
                    App.grdDet.store.remove(invtBlank);
                }
                var _posm = App.cboPosmID.getValue();
                allNodes.forEach(function (node) {
                    if (node.attributes.Type == "CpnyID") {
                        var idx = App.grdDet.store.getCount();                 
                        var record = HQ.store.findInStore(App.grdDet.store,['BranchID'],[node.attributes.CpnyID]);
                        if (!record) {
                            var det = App.stoDet.snapshot || App.stoDet.allData || App.stoDet.data;
                            for (var j = 0; j < det.length; j++) {
                                var _posmrecord = det.items[j].data.PosmID;
                                var _branchrecord = det.items[j].data.BranchID;
                                if (_posm == _posmrecord && _branchrecord == node.attributes.CpnyID) {
                                    return false;
                                    isError = true;
                                } 
                            }
                        if(!isError) {
                            HQ.store.insertBlank(App.stoDet, ['BranchID']);
                            var record = App.stoDet.getAt(App.grdDet.store.getCount() - 1);
                            record.set('PosmID', _posm);
                            record.set('BranchID', node.attributes.CpnyID);
                            var branchName = node.attributes.CpnyName.split(" - ");
                            record.set('CpnyName', branchName[1]);
                            record.set('CpnyType', node.attributes.CpnyType);
                            } 
                        }
                    }
                });
                invtBlank = HQ.store.findRecord(App.grdDet.store, ['BranchID'], ['']);
                if (!invtBlank) {
                    var idx = App.grdDet.store.data.length > 0 ? App.grdDet.store.data.length : 0;
                    App.grdDet.store.insert(idx, Ext.create("App.mdlCpny", {
                        BranchID: ''
                    }));
                }
                App.grdDet.store.resumeEvents();
                App.stoDet.loadPage(1);
                App.stoDet.applyPaging();
                App.treePanelCompany.clearChecked();
                App.grdDet.view.refresh();
            }

        }

    }
    else {
        HQ.message.show(4, '', '');
    }
}
var btnAddAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var allNodes = getLeafNodes(App.treePanelCompany.getRootNode(), true);
            if (allNodes && allNodes.length > 0) {
                App.grdDet.store.suspendEvents();
                var invtBlank = HQ.store.findRecord(App.grdDet.store, ['BranchID'], ['']);
                if (invtBlank) {
                    App.grdDet.store.remove(invtBlank);
                }
                var isError = false;
                var lstnode = [];
                var _posm = App.cboPosmID.getValue();
                allNodes.forEach(function (node, index) {
                    if (node.data.Type == "CpnyID") {
                        var idx = App.grdDet.store.getCount();
                        if (idx > 0)
                            idx = idx - 1;
                        var record = HQ.store.findInStore(App.grdDet.store,['BranchID'],[node.data.CpnyID]);
                        if (!record) {                          
                            var det = App.stoDet.snapshot || App.stoDet.allData || App.stoDet.data;
                            for (var j = 0; j < det.length; j++) {
                                var _posmrecord = det.items[j].data.PosmID;
                                var _branchrecord = det.items[j].data.BranchID;
                                if (_posm == _posmrecord && _branchrecord == node.data.CpnyID) {
                                    return false;
                                    isError = true;
                                }
                            }
                            if (!isError) {
                                var record = Ext.create('App.mdlCpny');
                                record.set('PosmID', _posm);
                                record.set('BranchID', node.data.CpnyID);
                                var branchName = node.data.CpnyName.split(" - ");
                                record.set('CpnyName', branchName[1]);
                                record.set('CpnyType', node.data.CpnyType);
                                lstnode.push(record);
                            }
                        }
                    }
                });               
                App.stoDet.add(lstnode);
                App.grdDet.store.resumeEvents();
                App.stoDet.loadPage(1);
                App.stoDet.applyPaging();
                App.treePanelCompany.clearChecked();
                App.grdDet.view.refresh();

            }

        }
        else {
                Main.Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
}
var btnDel_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            Event.Form.menuClick('delete');
        }
        else {
            if (Main.Process.showFieldInvalid(App.frmMain)) {
                Main.Process.showFieldInvalid(App.frmMain);
            }
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
}

var btnDelAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            HQ.message.show(11, '', 'deleteAllBranch');
        }
        else {
            if (Main.Process.showFieldInvalid(App.frmMain)) {
                Main.Process.showFieldInvalid(App.frmMain);
            }
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
}
deleteAllBranch = function (item) {
    if (item == "yes") {
        App.grdDet.store.removeAll();
        var record = HQ.store.findRecord(App.grdDet.store, ['BranchID'], ['']);
        if (!record) {
            HQ.store.insertBlank(App.stoDet, ['BranchID'],['']);
        }
    }
}
//// Declare ///////////////////////////////////////////////////////////
var keys = ['ComponentID'];//khoa của lưới
var values = [''];
var fieldsCheckRequire = ["ComponentID", "ComponentQty", "Unit"];
var fieldsLangCheckRequire = ["ComponentID", "Qty", "StkUnit"];
var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;
var KitID = '';
var _InvtID = '';
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoKitID.reload();
        HQ.common.showBusy(true);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
//load lần đầu khi mở
var firstLoad = function () {
    App.chkPack.setVisible(HQ.Pack);
    App.Price.setVisible(HQ.Price);
    App.direct.ReloadTreeIN20800();
    App.txtKitName.setReadOnly(true);
    App.frmMain.isValid();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboKitID.getStore().addListener('load', checkLoad);
    App.cboInvtID.getStore().addListener('load', checkLoad);

    if (HQ.isInsert == false) {
        App.menuClickbtnNew.disable();
    }
    if (HQ.isDelete == false) {
        App.menuClickbtnDelete.disable();
    }
    if (HQ.isUpdate == false && HQ.isInsert == false && HQ.isDelete == false) {
        App.menuClickbtnSave.disable();
    }
    //App.txtKitName.setReadOnly(true);
    //HQ.common.showBusy(false);
};
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'pnlHeader') {
                HQ.isFirstLoad = true;
                HQ.combo.first(App.cboKitID, HQ.isChange);
            }
            else {
                HQ.grid.first(App.grdIN_Component);
            }
            break;
        case "prev":
            if (HQ.focus == 'pnlHeader') {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboKitID, HQ.isChange);
            }
            else {
                HQ.grid.prev(App.grdIN_Component);
            }
            break;
        case "next":
            if (HQ.focus == 'pnlHeader') {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboKitID, HQ.isChange);
            }
            else {
                HQ.grid.next(App.grdIN_Component);
            }
            break;
        case "last":
            if (HQ.focus == 'pnlHeader') {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboKitID, HQ.isChange);
            }
            else {
                HQ.grid.last(App.grdIN_Component);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isFirstLoad = true;
                if (HQ.focus == 'pnlHeader') {
                    App.cboKitID.store.load(function () {
                        App.stoKitID.reload();
                    });
                }
                else
                    App.grdIN_Component.store.reload();
            }
            break;
        case "new":

            if (HQ.isInsert) {
                if (HQ.isChange && HQ.focus == 'pnlHeader') {
                    HQ.message.show(150, '', '');
                } else {
                    if (HQ.focus == 'pnlHeader') {
                        App.cboKitID.setValue('');
                        App.stoKitID.reload();
                    }
                    else {
                        HQ.grid.insert(App.grdIN_Component, keys, values);
                    }
                }
            }
            break;
        case "delete":
            var a = App.cboInvt.store.data;
            var check = false;
            for (var i = 0; i < a.length; i++) {
                if (App.cboKitID.getValue() == a.items[i].data.InvtID) {
                    check = true;
                }
            }
            if (check == false)
            {
                if (HQ.isDelete) {
                    if (HQ.focus == 'pnlHeader') {
                        if (App.cboKitID.getValue()) {
                            HQ.message.show(11, '', 'deleteData');
                        } else {
                            menuClick('new');
                        }
                    }
                    else
                        if (HQ.focus == 'grdIN_Component') {
                            var rowindex = HQ.grid.indexSelect(App.grdIN_Component);
                            if (rowindex != '')
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdIN_Component), ''], 'deleteData_Grid', true);
                        }
                }
            }
            else
                HQ.message.show(2018051461, '', '');
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoIN_Component, keys, fieldsCheckRequire, fieldsLangCheckRequire))
                    if (HQ.util.checkSpecialChar(App.cboKitID.getValue()) == true) {
                        save();
                    }
                    else {
                        HQ.message.show(20140811, App.cboKitID.fieldLabel);
                        App.cboKitID.focus();
                        App.cboKitID.selectText();
                    }
                    HQ.isFirstLoad = true;
                }
            
            break;
        case "print":
            break;
        case "close":
            break;
    }
};

var frmChange = function () {
    if (App.stoKitID.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoKitID) == false ? (HQ.store.isChange(App.stoIN_Component)) : true;
    HQ.common.changeData(HQ.isChange, 'IN20800');
    if (App.cboKitID.valueModels == null || HQ.isNew == true) {
        App.cboKitID.setReadOnly(false);
    }
    else {
        App.cboKitID.setReadOnly(HQ.isChange);
    }

    //HQ.isChange = HQ.store.isChange(App.stoKitID);
    //HQ.common.changeData(HQ.isChange, 'IN20800');
    //if (App.cboKitID.valueModels == null || HQ.isNew == true) {
    //    App.cboKitID.setReadOnly(false);
    //}
    //else {
    //    if (HQ.focus == "pnlHeader") {
    //        HQ.isChange = HQ.store.isChange(App.stoKitID);
    //        App.cboKitID.setReadOnly(HQ.isChange);
    //    }
    //    else {
    //        if (HQ.focus == "grdIN_Component") {
    //            HQ.isChange = HQ.store.isChange(App.stoIN_Component);
    //            App.cboKitID.setReadOnly(HQ.isChange);
    //        }
    //    }
    //}
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoChanged = function () {

    HQ.common.changeData(HQ.isChange, 'IN20800');
};
var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    
    //App.cboKitID.forceSelection = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "KitID");
        record = sto.getAt(0);
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        //App.cboKitID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        //App.cboKitID.focus(true); //focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    
    if (!HQ.isInsert && HQ.isNew) {
        //App.cboKitID.forceSelection = false;
        HQ.common.lockItem(App.frmMain, true);
        HQ.store.insertBlank(sto, keys);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
    App.stoIN_Component.reload();
    nodeID = App.cboKitID.getValue();
    if (App.slmtreeKitID.selected.items.length == 0 || (App.slmtreeKitID.selected.items.length > 0 && App.slmtreeKitID.selected.items[0].data.id != nodeID))
        searchNode(nodeID);

};
var stoLoad_IN_Component = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20800');
    if (HQ.isInsert) {
        var record1 = HQ.store.findRecord(App.stoIN_Component, ["KitID"], ['']);
        if (!record1) {
            HQ.store.insertBlank(sto, "KitID");
        }
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
};
var stoIN_ComponentChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20800');
};

var cboKitID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
        App.cboKitID.store.reload();
    }
};
var cboKitID_change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoKitID.loading) {
        //KitID = value;
        App.stoKitID.reload();
    }
    var record = HQ.store.findRecord(App.cboKitID.store, ["KitID"], [App.cboKitID.getValue()]);
    if(record!=undefined)
    {
        App.txtKitName.setValue(record.data.Descr);
    }
    else
    {
        App.txtKitName.setValue('');
    }
   
};
var cboKitID_select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoKitID.loading) {
        //KitID = value;
        App.stoKitID.reload();
    }
};
var cboInvtID_change = function (sender, value) {
    App.cboUnit.store.reload();
};
var grdIN_Component_BeforeEdit = function (editor, e) {
    var a = App.cboInvt.store.data;
    for(var i=0;i<a.length;i++)
    {
        if (App.cboKitID.getValue() == a.items[i].data.InvtID)
        {
            if (e.field == "ComponentID" || e.field == "ComponentQty" || e.field == "Unit" || e.field == "Price")
                return false;
        }
    }
    _InvtID = e.record.data.ComponentID;
    App.cboUnit.store.reload();
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdIN_Component_Edit = function (item, e) {
    if (e.field == 'ComponentID')
    {
        e.record.set('ComponentQty', 1);
    }
    var record = App.cboInvtID.findRecord("InvtID", e.record.data.ComponentID);
    if (record){
        e.record.set("Descr", record.data.Descr);
        e.record.set("Unit", record.data.StkUnit);
    }
    else {
        e.record.set("Descr", '');
        e.record.set("ComponentQty", 0);
        e.record.set("Unit", '');
    }
    //checkInsertKey(App.grdIN_Component, e, keys);
    frmChange();
};

var checkInsertKey = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && HQ.isInsert)
            HQ.store.insertBlank(grd.getStore(), keys);
    }
};

var grdIN_Component_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdIN_Component, e, keys,false);
};
var grdIN_Component_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN_Component);
    //App.cboKitID.setReadOnly(false);
    //HQ.isChange = false;
    frmChange();
    stoChanged(App.stoIN_BundleItem);
};
var save = function () {
    if (HQ.isInsert) {
        if (App.stoIN_Component.getCount() == 0) {
            HQ.message.show(2018051460, '', '');
            App.grdIN_Component.focus();
            return;
        }
    }
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'IN20800/Save',
            params: {
                lstIN_Kit: Ext.encode(App.stoKitID.getRecordsValues()),
                lstIN_Component: HQ.store.getData(App.stoIN_Component),
                lstIN_ComponentCreate: Ext.encode(App.grdIN_Component.store.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.stoKitID.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
deleteData = function (item) {
    if (item == "yes") {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("DeletingData"),
                url: 'IN20800/Delete',
                timeout: 7200,
                success: function (msg, data) {
                    App.cboKitID.getStore().load();
                    menuClick("new");
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};
var deleteData_Grid = function (item) {
    if (item == "yes") {
        App.grdIN_Component.deleteSelected();
        frmChange();
        stoIN_ComponentChanged(App.stoIN_BundleItem);
    }
};
function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew) {
            App.cboKitID.setValue('');
        }
        HQ.isChange = false;
        HQ.isFirstLoad = true;

        App.stoKitID.reload();
    }
};

//Tree
var searchNode = function (nodeID) {
    App.frmMain.suspendLayouts();
    var nodeKitID = App.treeKitID.getRootNode().findChild('id', nodeID, true);
    if (nodeKitID) {
        App.treeKitID.getSelectionModel().deselectAll();
        App.treeKitID.getRootNode().expand();
        expandParentNode(nodeKitID);
        App.treeKitID.getSelectionModel().select(nodeKitID, true);
    }
    App.frmMain.resumeLayouts(true);
};
var node_expand = function (item) {
    App.treeKitID.view.focusRow(App.slmtreeKitID.getSelection()[0]);
}
var expandParentNode = function (node) {
    var parentNode = node.parentNode;
    if (parentNode) {

        expandParentNode(parentNode);
        parentNode.expand();
    }
};

var ReloadTree = function (type, valueKitID) {
    try {
        _treeExpandAll = false;
        App.direct.ReloadTreeIN20800({
            success: function (data) {
                if (type == 'save') {
                    App.cboKitID.store.reload();
                }
                else if (type == 'delete') {
                    App.cboKitID.getStore().reload();
                }
            },
            failure: function () {
                alert("fail");
            },
            //eventMask: { msg: 'loadingTree', showMask: true }
        });
    } catch (ex) {
        alert(ex.message);
    }
};
var nodeSelected_Change = function (store, operation, options) {
    if (HQ.isChange) {
        HQ.message.show(150);
        return false;
    }
    Ext.suspendLayouts();
    if (operation.internalId != 'root') {
        _root = 'false';
        HQ.KitID = operation.raw.KitID;
    }
    if (HQ.KitID) {
        if (App.cboKitID.getValue() != HQ.KitID) {
            App.cboKitID.setValue(HQ.KitID);
        }
    }

    Ext.resumeLayouts(true);
};
var collapseAll = function (tree) {
    this.updateTreeView(tree, function (root) {
        root.cascadeBy(function (node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = false;
            }
        });
        return tree.rootVisible ? [root] : root.childNodes;
    });
};
var updateTreeView = function (tree, fn) {
    var view = tree.getView();
    view.getStore().loadRecords(fn(tree.getRootNode()));
    view.refresh();
};
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
};
var btnExpandKitID_click = function (btn, e, eOpts) {
    Ext.suspendLayouts();
    _treeExpandAll = true;
    expandAll(App.treeKitID);
    Ext.resumeLayouts(true);
};

var btnCollapseKitID_click = function (btn, e, eOpts) {
    collapseAll(App.treeKitID);
};


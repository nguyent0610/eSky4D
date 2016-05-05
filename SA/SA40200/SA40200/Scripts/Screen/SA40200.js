//// Declare //////////////////////////////////////////////////////////
var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID"];
var fieldsLangCheckRequire = ["BranchID"];
var _flagDelete = false;
var ID = '';
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlgrid') {
            _focusNo = 1;
        }
        else {
            _focusNo = 0;
        }
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.combo.first(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.first(App.grdSYS_CloseDateBranchAuto);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.prev(App.grdSYS_CloseDateBranchAuto);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.next(App.grdSYS_CloseDateBranchAuto);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.last(App.grdSYS_CloseDateBranchAuto);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboID.valueModels == null) App.cboID.setValue('');
                App.cboID.getStore().load(function () {
                    App.stoSYS_CloseDateAuto.reload();
                });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        ID = '';
                        App.cboID.clearValue();
                        App.stoSYS_CloseDateAuto.reload();
                    }
                }
                else {
                    HQ.grid.insert(App.grdSYS_CloseDateBranchAuto, keys);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (_focusNo == 0) {
                    if (App.cboID.getValue()) {
                        HQ.message.show(11, '', 'deleteData');
                    } else {
                        menuClick('new');
                    }
                }
                else {
                    if (App.slmSYS_CloseDateBranchAuto.selected.items[0] != undefined) {
                        if (App.slmSYS_CloseDateBranchAuto.selected.items[0].data.BranchID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_CloseDateBranchAuto)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoSYS_CloseDateAuto, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    if (App.stoSYS_CloseDateBranchAuto.getCount() == 1)
                        HQ.message.show(2015020804, '', '');
                    else
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



var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchIDSA40200_pcCompany.findRecord("CpnyID", rec.data.BranchID);
    if (record) {
        return record.data.CpnyName;
    }
    else {
        return value;
    }
};

// Event when cboOrderType_Main is changed or selected item 
var cboID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSYS_CloseDateAuto.loading) {
        ID = value;
        App.grdSYS_CloseDateBranchAuto.getSelectionModel().clearSelections();
        App.stoSYS_CloseDateAuto.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }
};

var cboID_KeyPress = function (sender, e) {
    if(e.keyCode <48 || e.keyCode > 57) {
        e.stopEvent();
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.cboBranchID.store.load(function () {
        App.stoSYS_CloseDateAuto.reload();
    });
};

var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = (HQ.store.isChange(App.stoSYS_CloseDateAuto) == false ? HQ.store.isChange(App.stoSYS_CloseDateBranchAuto) : true);
    if (_flagDelete == true)
        HQ.isChange = true;
    HQ.common.changeData(HQ.isChange, 'SA40200');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboID.valueModels == null || HQ.isNew == true)
        App.cboID.setReadOnly(false);
    else App.cboID.setReadOnly(HQ.isChange);
};

var grdSYS_CloseDateBranchAuto_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSYS_CloseDateBranchAuto_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_CloseDateBranchAuto, e, keys);
    frmChange();
};

var grdSYS_CloseDateBranchAuto_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_CloseDateBranchAuto, e, keys);
};

var grdSYS_CloseDateBranchAuto_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_CloseDateBranchAuto);
    //stoChanged(App.stoSYS_CloseDateBranchAuto);
    frmChange();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA40200');
};

var stoLoadHeader = function (sto) {
    HQ.isFirstLoad = true;
    HQ.isNew = false;
    //App.cboID.forceSelection = true;
    //App.cboARDOCTYPE.forceSelection = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "ID");
        record = sto.getAt(0);

        HQ.isNew = true;//record la new    
        //App.cboID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.stoSYS_CloseDateBranchAuto.reload();

    if (HQ.isDelete) {
        App.btnDel.enable();
        App.btnDelAll.enable();
    }
    else {
        App.btnDel.disable();
        App.btnDelAll.disable();
    }

    if (!HQ.isInsert && HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
        _flagDelete = false;
    }
    frmChange();
    HQ.common.showBusy(false);
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 18000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA40200/Save',
            params: {
                lstSYS_CloseDateAuto: Ext.encode(App.stoSYS_CloseDateAuto.getChangedData({ skipIdForPhantomRecords: false })),
                lstSYS_CloseDateBranchAuto: HQ.store.getAllData(App.stoSYS_CloseDateBranchAuto)
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                ID = data.result.ID;
                App.cboID.getStore().load({
                    callback: function () {
                        if (Ext.isEmpty(App.cboID.getValue())) {
                            App.cboID.setValue(ID);
                            App.stoSYS_CloseDateAuto.reload();
                        }
                        else {
                            App.cboID.setValue(ID);
                            App.stoSYS_CloseDateAuto.reload();
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
function deleteData(item) {
    if (item == 'yes') {
        if (_focusNo == 0) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                url: 'SA40200/DeleteAll',
                success: function (action, data) {
                    App.cboID.setValue("");
                    App.cboID.getStore().load(function () { cboID_Change(App.cboID); });
                },
                failure: function (action, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                }
            });
        }
        else {
            App.grdSYS_CloseDateBranchAuto.deleteSelected();
        }
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        if (HQ.isNew)
            App.cboID.clearValue();
        App.stoSYS_CloseDateAuto.reload();
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
////////////////////////////////////// TREE ///////////////////////////////////

var beforenodedrop = function (node, data, overModel, dropPosition, dropFn) {
    if (Ext.isArray(data.records)) {
        var records = data.records;

        data.records = [];
        HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
        App.stoSYS_CloseDateBranchAuto.suspendEvents();
        addNode(records[0]);
        App.stoSYS_CloseDateBranchAuto.resumeEvents();
        App.grdSYS_CloseDateBranchAuto.view.refresh();
        App.stoSYS_CloseDateBranchAuto.loadPage(1);
    }
};

var treePanelBranch_checkChange = function (node, checked, eOpts) {
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
        var allNodes = getLeafNodes(App.treePanelBranch.getRootNode());
        if (allNodes && allNodes.length > 0) {
            App.stoSYS_CloseDateBranchAuto.suspendEvents();
            allNodes.forEach(function (node) {
                if (node.data.Type == "Company") {
                    var record = HQ.store.findRecord(App.grdSYS_CloseDateBranchAuto.store,
                        ['BranchID'],
                        [node.data.RecID]);
                    if (!record) {
                        record = App.stoSYS_CloseDateBranchAuto.getAt(App.grdSYS_CloseDateBranchAuto.store.getCount() - 1);
                        record.set('BranchID', node.data.RecID);
                        HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
                    }
                }
            });
            //HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
            App.stoSYS_CloseDateBranchAuto.resumeEvents();
            App.grdSYS_CloseDateBranchAuto.view.refresh();
            App.stoSYS_CloseDateBranchAuto.loadPage(1);
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
            var record = HQ.store.findRecord(App.grdSYS_CloseDateBranchAuto.store,
                ['BranchID'],
                [node.data.RecID]);
            if (!record) {
                record = App.stoSYS_CloseDateBranchAuto.getAt(App.grdSYS_CloseDateBranchAuto.store.getCount() - 1);
                record.set('BranchID', node.data.RecID);
                HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
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
            App.stoSYS_CloseDateBranchAuto.suspendEvents();
            allNodes.forEach(function (node) {
                if (node.attributes.Type == "Company") {
                    var record = HQ.store.findInStore(App.grdSYS_CloseDateBranchAuto.store,
                        ['BranchID'],
                        [node.attributes.RecID]);
                    if (!record) {
                        //HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
                        record = App.stoSYS_CloseDateBranchAuto.getAt(App.grdSYS_CloseDateBranchAuto.store.getCount() - 1);
                        record.set('BranchID', node.attributes.RecID);
                        HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
                    }
                }
            });
            
            App.stoSYS_CloseDateBranchAuto.resumeEvents();
            App.grdSYS_CloseDateBranchAuto.view.refresh();
            App.stoSYS_CloseDateBranchAuto.loadPage(1);
            App.treePanelBranch.clearChecked();
        }

    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDel_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        var selRecs = App.grdSYS_CloseDateBranchAuto.selModel.selected.items;
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
        App.grdSYS_CloseDateBranchAuto.deleteSelected();
    }
};

var deleteAllCompanies = function (item) {
    if (item == "yes") {
        var store = App.stoSYS_CloseDateBranchAuto;
        var allRecords = store.snapshot || store.allData || store.data;

        store.suspendEvents();
        allRecords.each(function (record) {
            record.phantom = true;
            store.remove(record);
        });
        store.resumeEvents();
        store.loadPage(1);
        _flagDelete = true;
        HQ.isChange = true;
        //HQ.isFirstLoad = true;
        HQ.common.changeData(HQ.isChange, 'OM42001')
        if (App.cboID.valueModels == null || HQ.isNew == true) {
            App.cboID.setReadOnly(false);
        }
        else {
            App.cboID.setReadOnly(HQ.isChange);
        }
        HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
    }
};
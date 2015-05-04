//// Declare //////////////////////////////////////////////////////////
var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID"];
var fieldsLangCheckRequire = ["BranchID"];
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
                        App.cboID.setValue('');
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
                    var curRecord = App.frmMain.getRecord();
                    if (curRecord) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
                else {
                    if (App.slmSYS_CloseDateBranchAuto.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdSYS_CloseDateBranchAuto);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSYS_CloseDateBranchAuto), ''], 'deleteData', true)
                    }
                }

            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoSYS_CloseDateAuto, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

function ReloadTree() {
    try {
        App.direct.ReloadTreeSI21600(App.cboType.getValue(),{
            success: function (data) {
            
            },
            failure: function () {
                alert("fail");
            },
            eventMask: { msg: 'Loading Tree', showMask: true }
        });
    } catch (ex) {
        alert(ex.message);
    }
};


var cboType_Change = function (sender, value) {
    ReloadTree();
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboType_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboType.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboType_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }
};

var beforenodedrop = function (node, data, overModel, dropPosition, dropFn) {
    if (Ext.isArray(data.records)) {
        var records = data.records;

        data.records = [];
        HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
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
    if (HQ.isUpdate) {
        var allNodes = getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
        if (allNodes && allNodes.length > 0) {
            allNodes.forEach(function (node) {
                HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
                if (node.data.Type == "Company") {
                    var record = HQ.store.findInStore(App.grdSYS_CloseDateBranchAuto.store,
                        ['BranchID'],
                        [node.data.RecID]);
                    if (!record) {
                        record = App.stoSYS_CloseDateBranchAuto.getAt(App.grdSYS_CloseDateBranchAuto.store.getCount() - 1);
                        record.set('BranchID', node.data.RecID);
                    }
                }
            });
            HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
            App.treePanelBranch.clearChecked();
        }

    }
    else {
        HQ.message.show(4, '', '');
    }
};

var addNode = function (node) {
    if (node.data.Type == "Company") {
        var record = HQ.store.findInStore(App.grdSYS_CloseDateBranchAuto.store,
            ['BranchID'],
            [node.data.RecID]);
        if (!record) {
            record = App.stoSYS_CloseDateBranchAuto.getAt(App.grdSYS_CloseDateBranchAuto.store.getCount() - 1);
            record.set('BranchID', node.data.RecID);
        }
        HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
    }
    else if (node.childNodes) {
        node.childNodes.forEach(function (itm) {
            addNode(itm);
        });
    }
};

var btnAdd_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        var allNodes = App.treePanelBranch.getCheckedNodes();
        if (allNodes && allNodes.length > 0) {
            allNodes.forEach(function (node) {
                HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
                if (node.attributes.Type == "Company") {
                    var record = HQ.store.findInStore(App.grdSYS_CloseDateBranchAuto.store,
                        ['BranchID'],
                        [node.attributes.RecID]);
                    if (!record) {
                        record = App.stoSYS_CloseDateBranchAuto.getAt(App.grdSYS_CloseDateBranchAuto.store.getCount() - 1);
                        record.set('BranchID', node.attributes.RecID);
                    }
                }
            });
            HQ.store.insertBlank(App.stoSYS_CloseDateBranchAuto, keys);
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

var deleteSelectedCompanies = function (item) {
    if (item == "yes") {
        App.grdSYS_CloseDateBranchAuto.deleteSelected();
    }
};

var deleteAllCompanies = function (item) {
    if (item == "yes") {
        App.grdSYS_CloseDateBranchAuto.store.removeAll();
    }
};

var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchIDSI21600_pcCompany.findRecord("CpnyID", rec.data.BranchID);
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
    if (sender.valueModels != null) {
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
    HQ.isFirstLoad = true;
    ReloadTree();
};

var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = (HQ.store.isChange(App.stoSYS_CloseDateAuto) == false ? HQ.store.isChange(App.stoSYS_CloseDateBranchAuto) : true);
    HQ.common.changeData(HQ.isChange, 'SI21600');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboID.valueModels == null || HQ.isNew == true)
        App.cboID.setReadOnly(false);
    else App.cboID.setReadOnly(HQ.isChange);
};

var grdSYS_CloseDateBranchAuto_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
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
    HQ.common.changeData(HQ.isChange, 'SI21600');
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
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
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
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI21600/Save',
            params: {
                lstSYS_CloseDateAuto: Ext.encode(App.stoSYS_CloseDateAuto.getChangedData({ skipIdForPhantomRecords: false })),
                lstSYS_CloseDateBranchAuto: HQ.store.getData(App.stoSYS_CloseDateBranchAuto)
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var ID = data.result.ID;
                App.cboID.getStore().load(function () {
                    App.cboID.setValue(ID);
                    App.stoSYS_CloseDateAuto.reload();
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
                url: 'SI21600/DeleteAll',
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
        var ID = '';
        if (App.cboID.valueModels != null) ID = App.cboID.getValue();
        App.cboID.getStore().load(function () {
            App.cboID.setValue(ID);
            App.stoSYS_CloseDateAuto.reload();
        });
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
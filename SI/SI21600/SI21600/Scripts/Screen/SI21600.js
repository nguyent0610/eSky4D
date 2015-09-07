//// Declare //////////////////////////////////////////////////////////
var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID"];
var fieldsLangCheckRequire = ["BranchID"];

var _nodeID;
var _nodeLevel;
var _parentRecordID;
var parentRecordIDAll = "";
var parentRecordID = "";
var _recordID = "";
var selectNode;
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboNodeID, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboNodeID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboNodeID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboNodeID, HQ.isChange);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboNodeID.valueModels == null) App.cboNodeID.setValue('');
                App.cboNodeID.getStore().load(function () {
                    App.stoSI_Hierarchy.reload();
                });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    App.cboNodeID.setValue('');
                    App.txtDescr.setValue('');
                    if (_nodeID != 'root') {
                        var obj = App.IDTree.getSelectedNodes().length > 0 ? App.IDTree.getSelectedNodes()[0].nodeID.split('-') : ['', 0, 0, 0];
                        App.txtNodeLevel.setValue(parseInt(obj[1], "10") + 1);
                        App.txtParentRecordID.setValue(obj[3]);
                    }
                    else {
                        App.txtNodeLevel.setValue(1);
                        App.txtParentRecordID.setValue(0);
                    }
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                var curRecord = App.frmMain.getRecord();
                if (curRecord) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    if (App.txtNodeLevel.getValue() == "0") {
                        HQ.message.show(2015090701, '', '');
                    } else {
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

function ReloadTree() {
    try {
        App.direct.ReloadTreeSI21600(App.cboType.getValue(), {
            success: function (data) {
                App.cboNodeID.getStore().load(function () {
                    //App.cboNodeID.setValue(NodeID);
                    App.stoSI_Hierarchy.reload();
                });
                // App.stoSI_Hierarchy.reload();
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
    App.cboNodeID.setValue('');
    App.txtDescr.setValue('');
    App.txtNodeLevel.setValue(0);
    App.txtParentRecordID.setValue(0);

    ReloadTree();
};

var NodeSelected_Change = function (store, operation, options) {
    if (HQ.isNew == false || Ext.isEmpty(App.cboNodeID.getValue())) {
        parentRecordIDAll = operation.data.id.split("-");
        _nodeID = parentRecordIDAll[0];
        _nodeLevel = parentRecordIDAll[1];
        _parentRecordID = parentRecordIDAll[2];
        _recordID = parentRecordIDAll[3];

        App.cboNodeID.setValue(_nodeID);
        if (_nodeID == 'root') {
            App.cboNodeID.setValue('');
            App.txtNodeLevel.setValue(1);
            App.txtParentRecordID.setValue(0);
        }
        else {
            App.txtNodeLevel.setValue(_nodeLevel);
            App.txtParentRecordID.setValue(_parentRecordID);
        }
    }
    else {
        App.cboNodeID.setValue("");
        App.txtDescr.setValue("");
        parentRecordIDAll = operation.data.id.split("-");
        App.txtNodeLevel.setValue(parentRecordIDAll[1]);
        App.txtParentRecordID.setValue(parentRecordIDAll[2]);
    }

};

var cboNodeID_Change = function (sender, value) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoSI_Hierarchy.loading) {
        _recordID = Ext.isEmpty(sender.valueModels[0]) ? '' : sender.valueModels[0].data.RecordID;
        App.stoSI_Hierarchy.reload();
    }
};

var cboNodeID_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoSI_Hierarchy.loading) {
        _recordID = Ext.isEmpty(sender.valueModels[0]) ? '' : sender.valueModels[0].data.RecordID;


        App.stoSI_Hierarchy.reload();
    }

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

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    App.cboType.getStore().load(function () {
        App.cboType.setValue('I');
        HQ.isFirstLoad = true;
        HQ.isNew = true;
    });
};

var frmChange = function () {
    if (App.stoSI_Hierarchy.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoSI_Hierarchy);
        HQ.common.changeData(HQ.isChange, 'SI21600');
    }
};

var stoLoadHeader = function (sto) {
    HQ.common.showBusy(false);
    HQ.isFirstLoad = true;
    HQ.isNew = false;
    App.cboNodeID.forceSelection = true;
    if (sto.data.length == 0) {
        App.cboNodeID.forceSelection = false;
        HQ.store.insertBlank(sto, "NodeID");
        record = sto.getAt(0);
        HQ.isNew = true;//record la new    
        App.cboNodeID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    HQ.common.setRequire(App.frmMain);  //to do cac o la require  
    frmChange();
    if (!HQ.isNew) searchNode();
};

//trước khi load trang busy la dang load data
var storeBeforeLoad = function (sto) {
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
                lstSI_Hierarchy: Ext.encode(App.stoSI_Hierarchy.getChangedData({ skipIdForPhantomRecords: false })),
                NodeID: App.cboNodeID.getValue(),
                NodeLevel: App.txtNodeLevel.getValue(),
                ParentRecordID: App.txtParentRecordID.getValue(),
                isNew: HQ.isNew

            },
            success: function (result, data) {
                HQ.isNew = false;
                HQ.message.show(201405071, '', '');

                var NodeID = data.result.NodeID;
                _recordID = data.result.RecordID;
                ReloadTree();
                App.cboNodeID.setValue(NodeID);
                //App.cboNodeID.getStore().load(function () {


                //    //App.stoSI_Hierarchy.reload();
                //});

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
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'SI21600/DeleteAll',
            success: function (action, data) {
                App.cboNodeID.setValue("");
                App.cboNodeID.getStore().load(function () { cboNodeID_Change(App.cboNodeID); });
                ReloadTree();
            },
            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });

    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.stoSI_Hierarchy.reload();
    }
};

var searchNode = function () {
    HQ.common.showBusy(true, HQ.common.getLang('searching node....'), App.frmMain);
    //tu dong bat node tree khi chon 1 cai tu cboInvtID
    var inactiveHierachy = App.stoSI_Hierarchy.getAt(0).data;

    var invtIDDescr = inactiveHierachy.NodeID + "-" + inactiveHierachy.NodeLevel + "-" + inactiveHierachy.ParentRecordID + "-" + inactiveHierachy.RecordID;
    var record = App.IDTree.getStore().getNodeById(invtIDDescr);
    if (App.IDTree.getStore().getNodeById(invtIDDescr)) {
        var depth = App.IDTree.getStore().getNodeById(invtIDDescr).data.depth;

        expandTree(depth, App.IDTree.getStore().getNodeById(invtIDDescr));
        var findNode = App.IDTree.items.items[0].store.data.items;
        for (var i = 0; i < findNode.length; i++) {
            if (findNode[i].data.id == invtIDDescr) {
                App.slmTree.select(i);
                tmpSelectedNode = App.slmTree.selected.items[0].data.id;

                if (tmpSelectedNode.split('-').length == 4) {
                    _nodeID = tmpSelectedNode.split('-')[0];
                    _nodeLevel = tmpSelectedNode.split('-')[1];
                    _parentRecordID = tmpSelectedNode.split('-')[2];
                    _recordID = tmpSelectedNode.split('-')[3];

                    App.txtNodeLevel.setValue(_nodeLevel);
                    App.txtParentRecordID.setValue(_parentRecordID);
                }
                break;
            }
        }
    }
    HQ.common.showBusy(false);
};

var expandTree = function (depth, tree) {
    if (depth > 0) {
        var treechil = tree.parentNode;
        expandTree(depth - 1, treechil);
        tree.parentNode.expand();
    }
};
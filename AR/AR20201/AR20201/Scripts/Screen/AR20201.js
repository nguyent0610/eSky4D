// Declare
var _beginStatus = "H";
var _pgIDLoad = "";


// Processing Function
var Process = {

    saveData: function () {
        if (App.frmMain.isValid()) {
            var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);

            App.frmMain.updateRecord();

            App.frmMain.submit({
                url: 'AR20201/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstPG: Ext.encode(App.stoPG.getRecordsValues()),
                    lstPGCpnyAddr: HQ.store.getData(App.grdPGCpnyAddr.store),
                    channel: selRec ? selRec.Channel : "",
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboPGID.store.load(function () {
                        App.stoPG.reload();
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
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    },

    deleteData: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'AR20201/Delete',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    branchID: App.cboBranchID.getValue(),
                    pgID: App.cboPGID.getValue(),
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboPGID.store.load();
                    App.cboPGID.clearValue();
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

    deletePGCpnyAddr: function (item) {
        if (item == "yes") {
            App.grdPGCpnyAddr.deleteSelected();
        }
    },

    deleteAllPGAddrs: function (item) {
        if (item == "yes") {
            App.grdPGCpnyAddr.store.removeAll();
        }
    },

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.Form.menuClick("refresh");

        }
    },

    showFieldInvalid: function (form) {
        var done = 1;
        form.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                HQ.message.show(15, field.fieldLabel, 'Process.focusOnInvalidField');
                done = 0;
                return false;
            }
        });
        return done;
    },

    focusOnInvalidField: function (item) {
        if (item == "ok") {
            App.frmMain.getForm().getFields().each(function (field) {
                if (!field.isValid()) {
                    field.focus();
                    return false;
                }
            });
        }
    },

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

    getDeepAllLeafNodes: function (node, onlyLeaf) {
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
    },

    isChange: function (store) {
        if ((store.getChangedData().Created != undefined && store.getChangedData().Created.length > 0)
            || store.getChangedData().Updated != undefined
            || store.getChangedData().Deleted != undefined) {
            return true;
        } else {
            return false;
        }
    },

    getChannel: function () {
        var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);

        if (selRec && selRec.Channel) {
            return selRec.Channel
        }
        else {
            return "";
        }
    }
};

// Store Event
var Store = {
    stoPG_load: function (sto, records, successful, eOpts) {
        HQ.isNew = false;
        if (sto.getCount() == 0) {
            var newSlsper = Ext.create("App.mdlAR_PG", {
                PGID: App.cboPGID.getValue(),
                BranchID: App.cboBranchID.getValue()
            });
            sto.insert(0, newSlsper);
            HQ.isNew = true;
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);
        _pgIDLoad = frmRecord.data.PGID;

        App.grdPGCpnyAddr.store.reload();

        var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);

        if (selRec && selRec.Channel == "MT") {
            //App.pnlPGCpnyAddr.show();
        }
        else {
            //App.pnlPGCpnyAddr.hide();
            //App.grdPGCpnyAddr.store.removeAll();
        }
        HQ.isChange = false;
        //Event.Form.frmMain_fieldChange();
    },

    stoPGCpnyAddr_load: function (sto, records, successful, eOpts) {
        //if (HQ.isUpdate) {
        //    var branchID = App.cboBranchID.getValue();
        //    var slperID = App.cboPGID.getValue();
        //    var status = App.cboStatus.getValue();
        //    var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

        //    if (branchID && slperID && status == _beginStatus) {
        //        if (successful) {
        //            var newData = {
        //                SlsperID: slperID,
        //                BranchID: branchID
        //            };

        //            var newRec = Ext.create(sto.model.modelName, newData);
        //            HQ.store.insertRecord(sto, keys, newRec, false);
        //        }
        //    }
        //}
    }
};

// Form Event
var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            HQ.common.setRequire(frm);
            App.cboBranchID.store.load(function (records, operation, success) {
                App.cboBranchID.setValue(HQ.cpnyID);
            });
        },

        frmMain_fieldChange: function () {
            if (App.stoPG.getCount() > 0) {
                App.frmMain.updateRecord();
                if (!HQ.store.isChange(App.stoPG)) {
                    HQ.isChange = Process.isChange(App.grdPGCpnyAddr.store);
                }
                else {
                    HQ.isChange = true;
                }

                HQ.common.changeData(HQ.isChange, 'AR20201');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                App.cboBranchID.setReadOnly(HQ.isChange);
                App.cboPGID.setReadOnly(HQ.isChange);
            }
        },

        cboBranchID_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboPGLeader.store.reload();
            //App.cboPGID.clearValue();
            App.cboPGID.store.load(function (records, operation, success) {
                if (records.length > 0) {
                    if (records[0].data.PGID != App.cboPGID.getValue()) {
                        App.cboPGID.setValue(records[0].data.PGID);
                    }
                    else {
                        App.cboPGID.setValueAndFireSelect(records[0].data.PGID);
                    }
                }
                else {
                    App.cboPGID.clearValue();
                }
            });

            //App.treeCpnyAddr.store.reload();
        },

        cboPGID_change: function (cbo, newValue, oldValue, eOpts) {
            if (_pgIDLoad != HQ.util.passNull(cbo.getValue()) && !cbo.hasFocus) {
                App.stoPG.reload();
            }
        },

        cboPGID_select: function (cbo, newValue, oldValue, eOpts) {
            App.stoPG.reload();
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'header') {
                        HQ.combo.first(App.cboPGID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.first(App.grdPGCpnyAddr);
                    }
                    break;

                case "next":
                    if (HQ.focus == 'header') {
                        HQ.combo.next(App.cboPGID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.next(App.grdPGCpnyAddr);
                    }
                    break;

                case "prev":
                    if (HQ.focus == 'header') {
                        HQ.combo.prev(App.cboPGID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.prev(App.grdPGCpnyAddr);
                    }
                    break;

                case "last":
                    if (HQ.focus == 'header') {
                        HQ.combo.last(App.cboPGID, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.last(App.grdPGCpnyAddr);
                    }
                    break;

                case "save":
                    if (HQ.isInsert || HQ.isUpdate) {
                        Process.saveData();
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;

                case "delete":
                    if (HQ.isDelete) {
                        if (HQ.focus == 'header') {
                            if (App.cboPGID.getValue()) {
                                HQ.message.show(11, '', 'Process.deleteData');
                            }
                            else {
                                HQ.message.show(20140306, '', '');
                            }
                        }
                        else if (HQ.focus == 'grid') {
                            var selected = App.grdPGCpnyAddr.getSelectionModel().selected.items;
                            if (selected.length > 0) {
                                if (selected[0].index != undefined) {
                                    var params = selected[0].index + 1 + ',' + App.pnlPGCpnyAddr.title;
                                    HQ.message.show(2015020807, params, 'Process.deletePGCpnyAddr');
                                }
                                else {
                                    HQ.message.show(11, '', 'Process.deletePGCpnyAddr');
                                }
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;

                case "close":
                    HQ.common.close(this);
                    break;

                case "new":
                    if (HQ.isInsert) {
                        if (HQ.isChange) {
                            HQ.message.show(150, '', '');
                        }
                        else {
                            App.cboPGID.clearValue();
                        }
                    }
                    break;

                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'header') {
                            App.cboPGID.store.load(function () {
                                App.stoPG.reload();
                            });
                        }
                        else if (HQ.focus == 'grid') {
                            App.grdPGCpnyAddr.store.reload();
                        }
                    }
                    break;
                default:
            }
        }
    },

    Grid: {
        grdPGCpnyAddr_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var keys = ["AddrID"];

                    if (keys.indexOf(e.field) != -1) {
                        if (e.record.data.tstamp)
                            return false;
                    }
                    return HQ.grid.checkInput(e, keys);
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

        grdPGCpnyAddr_edit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    var branchID = App.cboBranchID.getValue();
                    var pgID = App.cboPGID.getValue();
                    var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                    if (branchID && pgID) {
                        var newData = {
                            PGID: pgID,
                            BranchID: branchID
                        };

                        var newRec = Ext.create(e.store.model.modelName, newData);
                        HQ.store.insertRecord(e.store, keys, newRec, false);
                    }
                }
            }
        },

        grdPGCpnyAddr_validateEdit: function (editor, e) {
            var keys = ['AddrID'];

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
        },

        grdPGCpnyAddr_reject: function (col, record) {
            var store = record.store;

            if (record.data.tstamp == '') {
                store.remove(record);
                col.grid.getView().focusRow(store.getCount() - 1);
                col.grid.getSelectionModel().select(store.getCount() - 1);
            } else {
                record.reject();
            }
        }
    },

    Tree: {
        treePanelCpnyAddr_checkChange: function (node, checked, eOpts) {
            node.childNodes.forEach(function (childNode) {
                childNode.set("checked", checked);
            });
        },

        btnExpand_click: function (btn, e, eOpts) {
            App.treePanelCpnyAddr.expandAll();
        },

        btnCollapse_click: function (btn, e, eOpts) {
            App.treePanelCpnyAddr.collapseAll();
        },

        btnAddAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var branchID = App.cboBranchID.getValue();
                    var pgID = App.cboPGID.getValue();

                    if (branchID && pgID) {
                        var allNodes = Process.getDeepAllLeafNodes(App.treePanelCpnyAddr.getRootNode(), true);
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.raw.Type == "Addr") {
                                    var idx = App.grdPGCpnyAddr.store.getCount();
                                    var record = HQ.store.findInStore(App.grdPGCpnyAddr.store,
                                        ['AddrID'],
                                        [node.raw.RecID]);
                                    if (!record) {
                                        App.grdPGCpnyAddr.store.insert(idx, Ext.create("App.mdlPGCpnyAddr", {
                                            AddrID: node.raw.RecID,
                                            Addr1: node.raw.Addr1,
                                            Name: node.raw.AddrName,
                                            //WorkingTime: ''
                                        }));
                                    }
                                }
                            });
                            App.treePanelCpnyAddr.clearChecked();
                        }
                    }
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAdd_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var branchID = App.cboBranchID.getValue();
                    var slperID = App.cboPGID.getValue();

                    if (branchID && slperID) {
                        var allNodes = App.treePanelCpnyAddr.getChecked();
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.raw.Type == "Addr") {
                                    var idx = App.grdPGCpnyAddr.store.getCount();
                                    var record = HQ.store.findInStore(App.grdPGCpnyAddr.store,
                                        ['AddrID'],
                                        [node.raw.RecID]);
                                    if (!record) {
                                        App.grdPGCpnyAddr.store.insert(idx, Ext.create("App.mdlPGCpnyAddr", {
                                            AddrID: node.raw.RecID,
                                            Addr1: node.raw.Addr1,
                                            Name: node.raw.AddrName,
                                            //WorkingTime: ''
                                        }));
                                    }
                                }
                            });
                            App.treePanelCpnyAddr.clearChecked();
                        }
                    }
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDel_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var selRecs = App.grdPGCpnyAddr.selModel.selected.items;
                    if (selRecs.length > 0) {
                        var params = [];
                        selRecs.forEach(function (record) {
                            params.push(record.data.AddrID);
                        });
                        HQ.message.show(2015020806,
                            params.join(" & ") + "," + HQ.common.getLang("PGCpnyAddr"),
                            'Process.deletePGCpnyAddr');
                    }
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    HQ.message.show(11, '', 'Process.deleteAllPGAddrs');
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        }
    }
};
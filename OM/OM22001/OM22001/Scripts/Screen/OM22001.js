var _beginStatus = "H";

var Process = {
    renderCpnyName: function (value) {
        var record = App.cboGCpnyID.store.findRecord("CpnyID", value);
        if (record) {
            return record.data.CpnyName;
        }
        else {
            return value;
        }
    },

    renderInvtDescr: function (value) {
        var record = App.cboColInvtID.store.findRecord("Code", value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },

    renderLocDescr: function (value) {
        var record = App.cboColLocID.store.findRecord("Code", value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },

    renderInvtCategory: function(value){
        var record = App.cboColInvtID.store.findRecord("Code", value);
        if (record) {
            return record.data.Category;
        }
        else {
            return value;
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

    deleteSelectedCompanies: function (item) {
        if (item == "yes") {
            App.grdCompany.deleteSelected();
        }
    },

    deleteAllCompanies: function (item) {
        if (item == "yes") {
            App.grdCompany.store.removeAll();
        }
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
            Event.Form.menuClick("refresh");
        }
    },

    saveData: function () {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();

            App.frmMain.submit({
                url: 'OM22001/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstDisplay: Ext.encode(App.stoDisplay.getRecordsValues()),
                    lstCpny: Ext.encode(App.grdCompany.store.getRecordsValues()),
                    lstCpnyChange: HQ.store.getData(App.grdCompany.store),
                    lstLevelChange: HQ.store.getData(App.grdLevel.store),
                    //lstInvtChange: HQ.store.getData(App.grdInventory.store),
                    lstLocChange: HQ.store.getData(App.grdLocation.store),
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    HQ.isChange = false;
                    App.cboDisplayID.store.load(function (records, operation, success) {
                        App.stoDisplay.reload();
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
    },

    deleteDisplay: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM22001/DeleteDisplay',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    displayID: App.cboDisplayID.getValue()//,
                    //isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboDisplayID.store.load();
                    App.cboDisplayID.clearValue();
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

    deleteCpny: function (item) {
        if (item == "yes") {
            App.grdCompany.deleteSelected();
        }
    },

    deleteLevel: function (item) {
        if (item == "yes") {
            App.grdLevel.deleteSelected();
        }
    },

    deleteInvt: function (item) {
        if (item == "yes") {
            App.grdInventory.deleteSelected();
        }
    },

    deleteLoc: function (item) {
        if (item == "yes") {
            App.grdLocation.deleteSelected();
        }
    }
};

var Store = {
    stoDisplay_load: function (sto, records, successful, eOpts) {
        HQ.isNew = false;
        if (sto.getCount() == 0) {
            var newDisplay = Ext.create("App.mdlOM_TDisplay", {
                FromDate: HQ.dateNow,
                ToDate: HQ.dateNow,
                Status: _beginStatus
            });
            HQ.store.insertRecord(sto, [], newDisplay, true);
            HQ.isNew = true;
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);

        App.grdCompany.store.reload();
        App.grdLevel.store.reload();
        //App.grdInventory.store.reload();
        App.grdLocation.store.reload();

        App.cboApplyFor.setReadOnly(frmRecord.data.Status != _beginStatus);
        App.cboApplyType.setReadOnly(frmRecord.data.Status != _beginStatus);

        Event.Form.frmMain_fieldChange();
    },

    stoGrid_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var status = App.cboStatus.getValue();
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

            if (status == _beginStatus) {
                if (successful) {
                    HQ.store.insertBlank(sto, keys);
                }
            }
        }
    },

    stoInventory_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var status = App.cboStatus.getValue();
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

            if (status == _beginStatus) {
                if (successful) {
                    if (sto.getCount() == 0) {
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
            //App.cboGCpnyID.store.reload();
            //App.cboColNhan.store.reload();
            //App.cboColInvtID.store.reload();
            HQ.common.setRequire(frm);
            App.stoDisplay.reload();
        },

        frmMain_fieldChange: function (frm, field, newValue, oldValue, eOpts) {
            if (App.stoDisplay.getCount() > 0) {
                App.frmMain.updateRecord();
                if (!HQ.store.isChange(App.stoDisplay)) {
                    if (!HQ.store.isChange(App.grdCompany.store)) {
                        if (!HQ.store.isChange(App.grdLevel.store)) {
                            HQ.isChange = HQ.store.isChange(App.grdInventory.store);
                        }
                        else {
                            HQ.isChange = true;
                        }
                    }
                    else {
                        HQ.isChange = true;
                    }
                }
                else {
                    HQ.isChange = true;
                }
                HQ.common.changeData(HQ.isChange, 'OM22001');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                App.cboDisplayID.setReadOnly(HQ.isChange);
            }
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        cboDisplayID_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoDisplay.reload();
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(newValue);
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'display') {
                        HQ.combo.first(App.cboDisplayID, HQ.isChange);
                    }
                    else if (HQ.focus == 'cpny') {
                        HQ.grid.first(App.grdCompany);
                    }
                    else if (HQ.focus == 'level') {
                        HQ.grid.first(App.grdLevel);
                    }
                    else if (HQ.focus == 'invt') {
                        HQ.grid.first(App.grdInventory);
                    }
                    else if (HQ.focus == 'loc') {
                        HQ.grid.first(App.grdLocation);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'display') {
                        HQ.combo.next(App.cboDisplayID, HQ.isChange);
                    }
                    else if (HQ.focus == 'cpny') {
                        HQ.grid.next(App.grdCompany);
                    }
                    else if (HQ.focus == 'level') {
                        HQ.grid.next(App.grdLevel);
                    }
                    else if (HQ.focus == 'invt') {
                        HQ.grid.next(App.grdInventory);
                    }
                    else if (HQ.focus == 'loc') {
                        HQ.grid.next(App.grdLocation);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'display') {
                        HQ.combo.prev(App.cboDisplayID, HQ.isChange);
                    }
                    else if (HQ.focus == 'cpny') {
                        HQ.grid.prev(App.grdCompany);
                    }
                    else if (HQ.focus == 'level') {
                        HQ.grid.prev(App.grdLevel);
                    }
                    else if (HQ.focus == 'invt') {
                        HQ.grid.prev(App.grdInventory);
                    }
                    else if (HQ.focus == 'loc') {
                        HQ.grid.prev(App.grdLocation);
                    }
                    break;

                case "last":
                    if (HQ.focus == 'display') {
                        HQ.combo.last(App.cboDisplayID, HQ.isChange);
                    }
                    else if (HQ.focus == 'cpny') {
                        HQ.grid.last(App.grdCompany);
                    }
                    else if (HQ.focus == 'level') {
                        HQ.grid.last(App.grdLevel);
                    }
                    else if (HQ.focus == 'invt') {
                        HQ.grid.last(App.grdInventory);
                    }
                    else if (HQ.focus == 'loc') {
                        HQ.grid.last(App.grdLocation);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'display') {
                            App.cboDisplayID.store.load(function (records, operation, success) {
                                App.stoDisplay.reload();
                            });
                        }
                        else if (HQ.focus == 'cpny') {
                            App.grdCompany.store.reload();
                        }
                        else if (HQ.focus == 'level') {
                            App.grdLevel.store.reload();
                        }
                        else if (HQ.focus == 'invt') {
                            App.grdInventory.store.reload();
                        }
                        else if (HQ.focus == 'loc') {
                            App.grdLocation.store.reload();
                        }
                    }
                    break;
                case "new":
                    if (HQ.isInsert) {
                        if (HQ.isChange) {
                            HQ.message.show(150, '', '');
                        }
                        else {
                            App.cboDisplayID.clearValue();
                        }
                    }
                    break;
                case "delete":
                    if (HQ.isDelete) {
                        if (App.cboStatus.getValue() == _beginStatus) {
                            if (HQ.focus == 'display') {
                                if (App.cboDisplayID.getValue()) {
                                    HQ.message.show(11, '', 'Process.deleteDisplay');
                                }
                            }
                            else if (HQ.focus == 'cpny') {
                                if (App.cboDisplayID.getValue() && App.slmCompany.getCount()) {
                                    HQ.message.show(2015020806,
                                        HQ.common.getLang('CpnyID') + " " + App.slmCompany.selected.items[0].data.CpnyID,
                                        'Process.deleteCpny');
                                }
                            }
                            else if (HQ.focus == 'level') {
                                if (App.cboDisplayID.getValue() && App.slmLevel.getCount()) {
                                    HQ.message.show(2015020806,
                                        HQ.common.getLang('LevelID') + " " + App.slmLevel.selected.items[0].data.LevelID,
                                        'Process.deleteLevel');
                                }
                            }
                            else if (HQ.focus == 'invt') {
                                if (App.cboDisplayID.getValue() && App.slmInventory.getCount()) {
                                    HQ.message.show(2015020806,
                                        HQ.common.getLang('InvtID') + " " + App.slmInventory.selected.items[0].data.InvtID,
                                        'Process.deleteInvt');
                                }
                            }
                            else if (HQ.focus == 'loc') {
                                if (App.cboDisplayID.getValue() && App.slmLocation.getCount()) {
                                    HQ.message.show(2015020806,
                                        HQ.common.getLang('LocID') + " " + App.slmLocation.selected.items[0].data.LocID,
                                        'Process.deleteLoc');
                                }
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
                case "save":
                    if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                        if (App.frmMain.isValid()) {
                            Process.saveData();
                        }
                    }
                    break;
                case "print":
                    break;
                case "close":
                    HQ.common.close(this);
                    break;
            }
        }
    },

    Grid: {
        grd_reject: function (col, record) {
            var grd = col.up('grid');
            if (!record.data.tstamp) {
                grd.getStore().remove(record, grd);
                grd.getView().focusRow(grd.getStore().getCount() - 1);
                grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            } else {
                record.reject();
            }
            //Event.Form.frmMain_fieldChange();
        },

        grd_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var status = App.cboStatus.getValue();
                    if (status == _beginStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
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

        grd_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    HQ.store.insertBlank(e.store, keys);   
                }
            }

            //Event.Form.frmMain_fieldChange();
        },

        grd_validateEdit: function (item, e) {
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

    Tree: {
        treePanelBranch_checkChange: function (node, checked, eOpts) {
            node.childNodes.forEach(function (childNode) {
                childNode.set("checked", checked);
            });
        },

        btnExpand_click: function (btn, e, eOpts) {
            App.treePanelBranch.expandAll();
        },

        btnCollapse_click: function (btn, e, eOpts) {
            App.treePanelBranch.collapseAll();
        },

        btnAddAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var displayID = App.cboDisplayID.getValue();
                    var status = App.cboStatus.value;

                    if (displayID && status == _beginStatus) {
                        var allNodes = Process.getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.data.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['CpnyID'],
                                        [node.data.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            CpnyID: node.data.RecID
                                        }));
                                    }
                                }
                            });
                            App.treePanelBranch.clearChecked();
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
                    var displayID = App.cboDisplayID.getValue();
                    var status = App.cboStatus.value;

                    if (displayID && status == _beginStatus) {
                        var allNodes = App.treePanelBranch.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['CpnyID'],
                                        [node.attributes.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            CpnyID: node.attributes.RecID
                                        }));
                                    }
                                }
                            });
                            App.treePanelBranch.clearChecked();
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
                    var status = App.cboStatus.value;

                    if (status == _beginStatus) {
                        var selRecs = App.grdCompany.selModel.selected.items;
                        if (selRecs.length > 0) {
                            var params = [];
                            selRecs.forEach(function (record) {
                                params.push(record.data.CpnyID);
                            });
                            HQ.message.show(2015020806,
                                params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                                'Process.deleteSelectedCompanies');
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

        btnDelAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _beginStatus) {
                        HQ.message.show(11, '', 'Process.deleteAllCompanies');
                    }
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
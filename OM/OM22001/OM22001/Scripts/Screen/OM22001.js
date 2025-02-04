var _beginStatus = "H";
var _displayType = {
    Level: "L",
    Signboard: "S",
    LCD: "M"
};

var _applyType = {
    Amount: "A",
    Qty: "Q"
};

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

    renderLocDescr: function (value) {
        var record = App.cboColLocID.store.findRecord("Code", value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },

    renderInvtInfo: function (value, metaData, record, rowIndex, colIndex, store) {
        var rec = App.cboColInvtID.store.findRecord("Code", record.data.InvtID);
        var returnValue = value;
        if (rec) {
            if (metaData.column.dataIndex == "Descr") {
                returnValue = rec.data.Descr;
            }
            else if (metaData.column.dataIndex == "StkUnit") {
                returnValue = rec.data.StkUnit;
            }
        }

        return returnValue;
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

    //isSomeValidKey: function (items, keys) {
    //    if (items && items.length > 0) {
    //        for (var i = 0; i < items.length; i++) {
    //            for (var j = 0; j < keys.length; j++) {
    //                if (items[i][keys[j]]) {
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    } else {
    //        return true;
    //    }
    //},

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
                    lstInvtChange: Process.getChangedFilteredData(App.grdInvt.store),
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
        else {
            Process.showFieldInvalid(App.frmMain);
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
            App.grdInvt.store.removeAll();
            App.grdLevel.deleteSelected();
        }
    },

    deleteInvt: function (item) {
        if (item == "yes") {
            App.grdInvt.deleteSelected();
        }
    },

    lastNbr: function (store) {
        var num = 0;
        for (var j = 0; j < store.data.length; j++) {
            var item = store.data.items[j];

            if (!Ext.isEmpty(item.data.LevelID) && parseInt(item.data.LevelID) > num) {
                num = parseInt(item.data.LevelID);
            }
        };
        num++;
        return num.toString();
    },

    showColumns: function (grid, dataIndexColumns, isShow) {
        grid.columns.forEach(function (col) {
            if (dataIndexColumns.indexOf(col.dataIndex) > -1) {
                if (isShow) {
                    col.show();
                    col.hideable = true;
                }
                else {
                    col.hide();
                    col.hideable = false;
                }
            }
        });
    },

    getChangedFilteredData: function (store) {
        var data = store.data,
            changedData

        store.data = store.snapshot; // does the trick
        changedData = store.getChangedData();
        store.data = data; // to revert the changes back
        return Ext.encode(changedData);
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
        App.grdLevel.store.load(function () {
            App.grdInvt.store.load(function () {
                App.grdInvt.store.filterBy(function (record) { });
            });
        });

        if (frmRecord.data.tstamp) {
            App.cboApplyFor.setReadOnly(true);
            App.cboApplyType.setReadOnly(true);
            App.cboDisplayType.setReadOnly(true);
            App.cboApplyTime.setReadOnly(true);

            if (frmRecord.data.Status == _beginStatus) {
                App.dtpFromDate.setReadOnly(false);
                App.dtpToDate.setReadOnly(false);
                App.txtDescr.setReadOnly(false);
            }
            else {
                App.dtpFromDate.setReadOnly(true);
                App.dtpToDate.setReadOnly(true);
                App.txtDescr.setReadOnly(true);
            }
        }
        else {
            App.cboApplyFor.setReadOnly(false);
            App.cboApplyType.setReadOnly(false);
            App.cboDisplayType.setReadOnly(false);
            App.cboApplyTime.setReadOnly(false);

            App.dtpFromDate.setReadOnly(false);
            App.dtpToDate.setReadOnly(false);
            App.txtDescr.setReadOnly(false);
        }

        Event.Form.frmMain_fieldChange();
    },

    stoGrid_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var status = App.cboStatus.getValue();
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

            if (status == _beginStatus) {
                if (successful) {
                    if (keys.indexOf('LevelID') > -1) {
                        var rec = Ext.create(sto.model.modelName, {
                            LevelID: Process.lastNbr(sto)
                        });
                        HQ.store.insertRecord(sto, keys, rec);
                    }
                    else {
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
                        HQ.isChange = HQ.store.isChange(App.grdLevel.store);
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
                App.cboDisplayID.setReadOnly(HQ.isChange && App.cboDisplayID.getValue());

                var frmRecord = App.frmMain.getRecord();
                if (!frmRecord.data.tstamp) {
                    if (HQ.isChange && App.cboDisplayType.getValue()
                        && (App.grdCompany.store.getCount() > 0 || App.grdLevel.store.getCount() > 1)) {
                        App.cboDisplayType.setReadOnly(true);
                    }
                    else {
                        App.cboDisplayType.setReadOnly(false);
                    }
                }
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

        cboApplyType_change: function (cbo, newValue, oldValue, eOpts) {
            if (cbo.getValue()) {
                if (cbo.getValue() == _applyType.Amount) {
                    App.pnlInvt.hide();
                    Process.showColumns(App.grdLevel, ["Bonus"], true);
                }
                else if (cbo.getValue() == _applyType.Qty) {
                    App.pnlInvt.show();
                    Process.showColumns(App.grdLevel, ["Bonus"], false);
                }
            }
            else {
                App.pnlInvt.hide();
                Process.showColumns(App.grdLevel, ["Bonus"], false);
            }
        },

        cboDisplayType_change: function (cbo, newValue, oldValue, eOpts) {
            if (cbo.getValue()) {
                if (cbo.getValue() == _displayType.Level) {
                    Process.showColumns(App.grdLevel, ["SoMatTB", "ChiPhiDauLon"], true);
                }
                else if (cbo.getValue() == _displayType.Signboard) {
                    Process.showColumns(App.grdLevel, ["SoMatTB", "ChiPhiDauLon"], false);
                }
                else if (cbo.getValue() == _displayType.LCD) {
                    Process.showColumns(App.grdLevel, ["SoMatTB", "ChiPhiDauLon"], false);
                }
            }
            else {
                Process.showColumns(App.grdLevel, ["SoMatTB", "ChiPhiDauLon"], false);
            }
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
                        HQ.grid.first(App.grdInvt);
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
                        HQ.grid.next(App.grdInvt);
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
                        HQ.grid.prev(App.grdInvt);
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
                        HQ.grid.last(App.grdInvt);
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
                                if (App.cboDisplayID.getValue() && App.slmInvt.getCount()) {
                                    HQ.message.show(2015020806,
                                        HQ.common.getLang('InvtID') + " " + App.slmInvt.selected.items[0].data.InvtID,
                                        'Process.deleteInvt');
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
                        Process.saveData();
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
        slmLevel_selectChange: function (grid, selected, eOpts) {
            var store = App.grdInvt.store;
            var keys = store.HQFieldKeys ? store.HQFieldKeys : "";
            var displayID = App.cboDisplayID.getValue();
            var displayType = App.cboDisplayType.getValue();

            store.clearFilter();
            if (selected.length > 0) {
                store.filterBy(function (record) {
                    if (record.data.LevelID == selected[0].data.LevelID) {
                        return record;
                    }
                });

                if (selected[0].data.LevelID && App.cboStatus.getValue() == _beginStatus) {
                    if (Process.isAllValidKey(store.getRecordsValues(), keys)) {
                        var newData = {
                            DisplayID: displayID,
                            DisplayType: displayType,
                            LevelID: selected[0].data.LevelID
                        };

                        var newRec = Ext.create(store.model.modelName, newData);
                        HQ.store.insertRecord(store, keys, newRec, false);
                    }
                }
            }
            else {
                store.filterBy(function (record) {
                    // no data
                });
            }
        },

        grd_reject: function (col, record) {
            var grd = col.up('grid');
            var keys = grd.store.HQFieldKeys ? grd.store.HQFieldKeys : "";
            if (!record.data.tstamp && keys && keys.indexOf('LevelID')<0) {
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
                    if (keys.indexOf('LevelID') > -1) {
                        var rec = Ext.create(e.store.model.modelName, {
                            LevelID: Process.lastNbr(e.store)
                        });
                        HQ.store.insertRecord(e.store, keys, rec);
                    }
                    else {
                        if (e.record.data.LevelID) {
                            var rec = Ext.create(e.store.model.modelName, {
                                LevelID: e.record.data.LevelID
                            });
                            HQ.store.insertRecord(e.store, keys, rec);
                        }
                        else {
                            HQ.store.insertBlank(e.store, keys);
                        }
                    }
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
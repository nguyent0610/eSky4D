var _holdStatus = "H";
var _focusID = "";
var _gridForDel;

var Main = {

    Process: {
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

        //kiem tra nhung field yeu cau bat buoc nhap
        checkRequire: function (title, items, checkedFields, keys) {
            if (items != undefined) {
                for (var i = 0; i < items.length; i++) {
                    if (HQ.grid.checkRequirePass(items[i], keys)) continue;

                    for (var j = 0; j < checkedFields.length; j++) {
                        if (items[i][checkedFields[j]].trim() == "") {
                            HQ.message.show(2015020808, HQ.common.getLang(checkedFields[j]) + "," + title);
                            return false;
                        }
                    }
                }
                return true;
            } else {
                return true;
            }
        },

        deleteSelectedInGrid: function (item) {
            if (item == "yes") {
                if (_gridForDel) {
                    _gridForDel.deleteSelected();
                }
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

        getParamForGridCombo: function (grid, fieldName) {
            if (grid.selModel.selected.items.length > 0) {
                var record = grid.selModel.selected.items[0];
                return record.data[fieldName];
            }
            else {
                return "";
            }
        },

        saveData: function () {
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if(App.frmMain.isValid()){ //if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    //var discId = App.cboDiscID.getValue();
                    //var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        if (Main.Process.checkRequire(App.grdDiscBreak.title, App.grdDiscBreak.store.getChangedData().Created, [], [])
                    && Main.Process.checkRequire(App.grdDiscBreak.title, App.grdDiscBreak.store.getChangedData().Updated, [], [])

                    && Main.Process.checkRequire(App.grdFreeItem.title, App.grdFreeItem.store.getChangedData().Updated, ["FreeItemID"], ["FreeItemID"])
                    && Main.Process.checkRequire(App.grdFreeItem.title, App.grdFreeItem.store.getChangedData().Updated, ["FreeItemID"], ["FreeItemID"])

                    && Main.Process.checkRequire(App.pnlAppComp.title, App.grdCompany.store.getChangedData().Updated, ["CpnyID"], ["CpnyID"])
                    && Main.Process.checkRequire(App.pnlAppComp.title, App.grdCompany.store.getChangedData().Updated, ["CpnyID"], ["CpnyID"])

                    && Main.Process.checkRequire(App.pnlDPII.title, App.grdDiscItem.store.getChangedData().Updated, ["InvtID"], ["InvtID"])
                    && Main.Process.checkRequire(App.pnlDPII.title, App.grdDiscItem.store.getChangedData().Updated, ["InvtID"], ["InvtID"])

                    && Main.Process.checkRequire(App.pnlDPBB.title, App.grdBundle.store.getChangedData().Updated, ["InvtID"], ["InvtID"])
                    && Main.Process.checkRequire(App.pnlDPBB.title, App.grdBundle.store.getChangedData().Updated, ["InvtID"], ["InvtID"])

                    && Main.Process.checkRequire(App.pnlDPTT.title, App.grdDiscCustClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])
                    && Main.Process.checkRequire(App.pnlDPTT.title, App.grdDiscCustClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])

                    && Main.Process.checkRequire(App.pnlDPCC.title, App.grdDiscCust.store.getChangedData().Updated, ["CustID"], ["CustID"])
                    && Main.Process.checkRequire(App.pnlDPCC.title, App.grdDiscCust.store.getChangedData().Updated, ["CustID"], ["CustID"])

                    && Main.Process.checkRequire(App.pnlDPPP.title, App.grdDiscItemClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])
                    && Main.Process.checkRequire(App.pnlDPPP.title, App.grdDiscItemClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])) {

                            App.frmMain.submit({
                                waitMsg: HQ.common.getLang("SavingData"),
                                url: 'OM21100/SaveData',
                                params: {
                                    lstDiscSeqInfo: HQ.store.getData(App.stoDiscSeqInfo),
                                    lstDiscBreak: HQ.store.getData(App.grdDiscBreak.store),
                                    lstFreeItem: HQ.store.getData(App.grdFreeItem.store),
                                    lstCompany: HQ.store.getData(App.grdCompany.store),
                                    lstDiscItem: HQ.store.getData(App.grdDiscItem.store),
                                    lstBundle: HQ.store.getData(App.grdBundle.store),
                                    lstDiscCustClass: HQ.store.getData(App.grdDiscCustClass.store),
                                    lstDiscCust: HQ.store.getData(App.grdDiscCust.store),
                                    lstDiscItemClass: HQ.store.getData(App.grdDiscItemClass.store)
                                },
                                success: function (msg, data) {
                                    if (data.result.msgCode) {
                                        HQ.message.show(data.result.msgCode);
                                    }
                                    else {
                                        HQ.message.show(201405071);
                                    }
                                    menuClick("refresh");
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
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
        }
    },

    Event: {
        control_render: function (cmd) {
            cmd.getEl().on('mousedown', function () {
                _focusID = cmd.id;
            });
        },

        frmMain_boxReady: function (frm, width, height, eOpts) {
            DiscDefintion.Event.cboDiscID_change(App.cboDiscID, "", "");
        },

        sto_load: function (sto, records, successful, eOpts) {
            if (HQ.isUpdate) {
                var discId = App.cboDiscID.getValue();
                var discSeq = App.cboDiscSeq.getValue();
                var status = App.cboStatus.getValue();
                var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";
                var idxLref = keys.indexOf("LineRef")

                if (discId && discSeq && status == _holdStatus) {
                    if (successful) {
                        var newData = {
                            DiscID: discId,
                            DiscSeq: discSeq
                        };

                        if (idxLref != -1) {
                            newData.LineRef = HQ.store.lastLineRef(sto);
                            keys.splice(idxLref, 1);
                        }

                        HQ.store.insertRecord(sto, keys, newData, false);
                    }
                }
            }
        },

        grd_beforeEdit: function (editor, e) {
            
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
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
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
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
                    && Main.Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Main.Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var idxLref = keys.indexOf("LineRef")

                    var newData = {
                        DiscID: discId,
                        DiscSeq: discSeq
                    };

                    if (idxLref != -1) {
                        newData.LineRef = HQ.store.lastLineRef(e.store);
                        keys.splice(idxLref, 1);
                    }

                    HQ.store.insertRecord(e.store, keys, newData, false);

                    if (!App.cboDiscClass.disabled) {
                        App.cboDiscClass.disable();
                    }
                }
            }
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
        },

        grd_reject: function (col, record) {
            var store = record.store;

            if (record.data.tstamp == '') {
                store.remove(record);
                col.grid.getView().focusRow(store.getCount() - 1);
                col.grid.getSelectionModel().select(store.getCount() - 1);
            } else {
                record.reject();
            }
        },

        menuClick: function (command) {
            var focusGrid;
            var title = "Data";

            if (App[_focusID]) {
                if (App[_focusID].xtype == "grid") {
                    focusGrid = App[_focusID];
                    title = focusGrid.title;
                }
                else {
                    var tmpGrds = App[_focusID].down("gridpanel");
                    if (tmpGrds) {
                        if (!tmpGrds.length) {
                            focusGrid = tmpGrds;
                            title = App[_focusID].title;
                        }
                    }
                }
            }

            switch (command) {
                case "first":
                    if (focusGrid) {
                        HQ.grid.first(focusGrid);
                    }
                    break;
                case "prev":
                    if (focusGrid) {
                        HQ.grid.prev(focusGrid);
                    }
                    break;
                case "next":
                    if (focusGrid) {
                        HQ.grid.next(focusGrid);
                    }
                    break;
                case "last":
                    if (focusGrid) {
                        HQ.grid.last(focusGrid);
                    }
                    break;
                case "refresh":
                    if (focusGrid) {
                        focusGrid.store.reload();
                        HQ.grid.first(focusGrid);
                    }
                    break;
                case "new":
                    if (HQ.isInsert) {
                        App.cboDiscID.setValue("");
                    }
                    break;
                case "delete":
                    if (focusGrid) {
                        if (HQ.isUpdate) {
                            var selected = focusGrid.getSelectionModel().selected.items;
                            if (selected.length > 0) {
                                _gridForDel = focusGrid;
                                if (selected[0].index != undefined) {
                                    var params = focusGrid.getSelectionModel().selected.items[0].index + 1 + ',' + title;
                                    HQ.message.show(2015020807, params, 'Main.Process.deleteSelectedInGrid');
                                }
                                else {
                                    HQ.message.show(11, '', 'Main.Process.deleteSelectedInGrid');
                                }
                            }
                        }
                        else {
                            HQ.message.show(4, '', '');
                        }
                    }
                    break;
                case "save":
                    Main.Process.saveData();
                    break;
                case "print":
                    break;
                case "close":
                    //if (HQ.store.isChange(App.stoLanguage)) {
                    //    HQ.message.show(5, '', 'askClose');
                    //} else {
                    //    HQ.common.close(this);
                    //}
                    break;
            }
        }
    }
};

var DiscDefintion = {
    Process: {
        enableATabInList: function (tabNames) {
            var listTabs = ["pnlDPII", "pnlDPBB", "pnlDPTT", "pnlDPCC", "pnlDPPP"];

            for (var j = 0; j < listTabs.length; j++) {
                App[listTabs[j]].disable();
            }

            for (var i = 0; i < tabNames.length; i++) {
                App[tabNames[i]].enable();
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

        renderCpnyName: function (value) {
            var record = App.cboGCpnyID.store.findRecord("CpnyID", value);
            if (record) {
                return record.data.CpnyName;
            }
            else {
                return value;
            }
        },

        renderFreeItemName: function (value) {
            var record = App.cboFreeItemID.store.findRecord("InvtID", value);
            if (record) {
                return record.data.Descr;
            }
            else {
                return value;
            }
        },

        renderDpiiInvtName: function (value) {
            var record = App.cboDpiiInvtID.store.findRecord("InvtID", value);
            if (record) {
                return record.data.Descr;
            }
            else {
                return value;
            }
        },

        renderGInvtName: function (value) {
            var record = App.cboGInvtID.store.findRecord("InvtID", value);
            if (record) {
                return record.data.Descr;
            }
            else {
                return value;
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
        }
    },

    Event: {
        stoDiscSeqInfo_load: function (sto, records, successful, eOpts) {
            if (records.length > 0) {
                App.frmDiscSeqInfo.loadRecord(records[0]);
            }
            else {
                var discSeqRec = Ext.create("App.mdlDiscSeqInfo", {
                    POStartDate: _dateNow,
                    POEndDate: _dateNow,
                    StartDate: _dateNow,
                    EndDate: _dateNow,
                    Status: _holdStatus
                });
                App.frmDiscSeqInfo.loadRecord(discSeqRec);
            }
        },

        cboDiscID_change: function (cbo, newValue, oldValue, eOpts) {
            var discData = HQ.store.findInStore(cbo.store, ['DiscID'], [newValue]);
            if (!discData) {
                App.cboDiscSeq.setValue("");
                if (App.cboDiscClass.disabled) {
                    App.cboDiscClass.enable();
                }
            }
            else {
                if (!App.cboDiscClass.disabled) {
                    App.cboDiscClass.disable();
                }
            }

            var discRecord = Ext.create(App.cboDiscID.store.model.getName(), discData);
            App.frmDiscDefintionTop.loadRecord(discRecord);
            App.cboDiscSeq.getStore().load();
        },

        cboDiscClass_change: function (cbo, newValue, oldValue, eOpts) {
            // 0: {Code: "BB", Descr: "Item Bundle"}
            if (cbo.value == "BB") {
                DiscDefintion.Process.enableATabInList(["pnlDPBB"]);
            }
                // 1: {Code: "CB", Descr: "Customer and Item Bundle"}
            else if (cbo.value == "CB") {
                DiscDefintion.Process.enableATabInList(["pnlDPBB", "pnlDPCC"]);
            }
                // 2: {Code: "CC", Descr: "Customer"}
            else if (cbo.value == "CC") {
                DiscDefintion.Process.enableATabInList(["pnlDPCC"]);
            }
                // 3: {Code: "CI", Descr: "Customer and Invt. Item"}
            else if (cbo.value == "CI") {
                DiscDefintion.Process.enableATabInList(["pnlDPCC", "pnlDPII"]);
            }
                // 4: {Code: "II", Descr: "Inventory Item"}
            else if (cbo.value == "II") {
                DiscDefintion.Process.enableATabInList(["pnlDPII"]);
            }
                // 5: {Code: "PP", Descr: "Product Group"}
            else if (cbo.value == "PP") {
                DiscDefintion.Process.enableATabInList(["pnlDPPP"]);
            }
                // 6: {Code: "TB", Descr: "Shop Type and Item Bundle"}
            else if (cbo.value == "TB") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT", "pnlDPBB"]);
            }
                // 7: {Code: "TI", Descr: "Shop Type and Invt. Item"}
            else if (cbo.value == "TI") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT", "pnlDPII"]);
            }
                // 8: {Code: "TP", Descr: "Prod. Group and Shop Type"}
            else if (cbo.value == "TP") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT", "pnlDPPP"]);
            }
                // 9: {Code: "TT", Descr: "Shop Type"}
            else if (cbo.value == "TT") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT"]);
            }
            else {
                DiscDefintion.Process.enableATabInList([]);
            }
        },

        cboDiscSeq_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoDiscSeqInfo.reload();

            App.grdDiscBreak.store.reload();
            App.grdFreeItem.store.reload();
            App.grdCompany.store.reload();

            App.grdDiscItem.store.reload();
            App.grdBundle.store.reload();
            App.grdDiscCustClass.store.reload();
            App.grdDiscCust.store.reload();
            App.grdDiscItemClass.store.reload();
        },

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
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = DiscDefintion.Process.getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.data.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['DiscID', 'DiscSeq', 'CpnyID'],
                                        [discId, discSeq, node.data.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CpnyID: node.data.RecID
                                        }));
                                    }
                                }
                            });
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAdd_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelBranch.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['DiscID', 'DiscSeq', 'CpnyID'],
                                        [discId, discSeq, node.attributes.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CpnyID: node.attributes.RecID
                                        }));
                                    }
                                }
                            });
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDel_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var selRecs = App.grdCompany.selModel.selected.items;
                        if (selRecs.length > 0) {
                            var params = [];
                            selRecs.forEach(function (record) {
                                params.push(record.data.CpnyID);
                            });
                            HQ.message.show(2015020806,
                                params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                                'DiscDefintion.Process.deleteSelectedCompanies');
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
                        HQ.message.show(11, '', 'DiscDefintion.Process.deleteAllCompanies');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        }
    }
};



var _holdStatus = "H";
var _gridForDel;
var _isNewDisc = false;
var _isNewSeq = false;

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

        isSomeValidKey: function (items, keys) {
            if (items && items.length > 0) {
                var flag = 0;
                for (var i = 0; i < items.length; i++) {
                    for (var j = 0; j < keys.length; j++) {
                        if (items[i][keys[j]])
                            flag = i;
                    }
                }
                if (flag == items.length - 1) {
                    return true;
                }
                else {
                    return false;
                }
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

        reloadAllData: function () {
            App.cboDiscID.store.reload();
            App.cboDiscSeq.store.reload();
            App.stoDiscSeqInfo.reload();
            App.grdDiscBreak.store.load(function () {
                App.grdFreeItem.store.load(function () {
                    App.slmDiscBreak.select(0);
                });
            });
            App.grdCompany.store.reload();
            App.grdDiscItem.store.reload();
            App.grdBundle.store.reload();
            App.grdDiscCustClass.store.reload();
            App.grdDiscCust.store.reload();
            App.grdDiscItemClass.store.reload();
        },

        saveData: function () {
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (App.frmMain.isValid()) { //if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
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

                            App.frmDiscSeqInfo.updateRecord();

                            App.frmMain.submit({
                                waitMsg: HQ.common.getLang("SavingData"),
                                url: 'OM21100/SaveData',
                                timeout: 10000000,
                                params: {
                                    isNewDiscID: _isNewDisc,
                                    isNewDiscSeq: _isNewSeq,

                                    //lstDiscSeqInfoChange: HQ.store.getData(App.stoDiscSeqInfo),
                                    //lstDiscBreakChange: HQ.store.getData(App.grdDiscBreak.store),
                                    lstFreeItemChange: HQ.store.getData(App.grdFreeItem.store),
                                    //lstCompanyChange: HQ.store.getData(App.grdCompany.store),
                                    //lstDiscItemChange: HQ.store.getData(App.grdDiscItem.store),
                                    //lstBundleChange: HQ.store.getData(App.grdBundle.store),
                                    //lstDiscCustClassChange: HQ.store.getData(App.grdDiscCustClass.store),
                                    //lstDiscCustChange: HQ.store.getData(App.grdDiscCust.store),
                                    //lstDiscItemClassChange: HQ.store.getData(App.grdDiscItemClass.store),

                                    lstDiscSeqInfo: (function () {
                                        App.stoDiscSeqInfo.each(function (item) {
                                            item.data.Active = App.chkActive.value ? 1 : 0;
                                            item.data.Promo = App.chkDiscTerm.value ? 1 : 0;
                                        });
                                        return Ext.encode(App.stoDiscSeqInfo.getRecordsValues());
                                    })(),
                                    lstDiscBreak: Ext.encode(App.grdDiscBreak.store.getRecordsValues()), // data
                                    lstFreeItem: Ext.encode(Main.Process.getRecordValues(App.grdFreeItem.store.snapshot.getRange())), // record.data
                                    lstCompany: Ext.encode(App.grdCompany.store.getRecordsValues()),
                                    lstDiscItem: Ext.encode(App.grdDiscItem.store.getRecordsValues()),
                                    lstBundle: Ext.encode(App.grdBundle.store.getRecordsValues()),
                                    lstDiscCustClass: Ext.encode(App.grdDiscCustClass.store.getRecordsValues()),
                                    lstDiscCust: Ext.encode(App.grdDiscCust.store.getRecordsValues()),
                                    lstDiscItemClass: Ext.encode(App.grdDiscItemClass.store.getRecordsValues()),
                                },
                                success: function (msg, data) {
                                    if (data.result.msgCode) {
                                        HQ.message.show(data.result.msgCode);
                                    }
                                    else {
                                        HQ.message.show(201405071);
                                    }
                                    if (data.result.tstamp) {
                                        App.tstamp.setValue(data.result.tstamp);
                                    }
                                    Main.Process.reloadAllData();
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
        },

        getRecordValues: function (recordRange) {
            var values = [];
            recordRange.forEach(function (record) {
                values.push(record.data);
            });
            return values;
        },

        refresh: function (item) {
            if (item == 'yes') {
                HQ.isChange = false;
                Main.Event.menuClick("refresh");
            }
        }
    },

    Event: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            DiscDefintion.Event.cboDiscID_change(App.cboDiscID, "", "");
        },

        frmMain_fieldChange: function (frm, field, newValue, oldValue, eOpts) {
            var disc = App.frmDiscDefintionTop.getRecord();
            if (disc) {
                App.frmDiscDefintionTop.updateRecord();
                HQ.isChange0 = disc.dirty;

                App.frmDiscSeqInfo.updateRecord();
                if (!HQ.store.isChange(App.stoDiscSeqInfo)) {
                    if (!HQ.store.isChange(App.grdDiscBreak.store)) {
                        if (!HQ.store.isChange(App.grdFreeItem.store)) {
                            if (!HQ.store.isChange(App.grdCompany.store)) {
                                if (!HQ.store.isChange(App.grdDiscItem.store)) {
                                    if (!HQ.store.isChange(App.grdBundle.store)) {
                                        if (!HQ.store.isChange(App.grdDiscCustClass.store)) {
                                            if (!HQ.store.isChange(App.grdDiscCust.store)) {
                                                HQ.isChange = HQ.store.isChange(App.grdDiscItemClass.store);
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
                    }
                    else {
                        HQ.isChange = true;
                    }
                }
                else {
                    HQ.isChange = true;
                }

                if (HQ.isChange || HQ.isChange0) {
                    HQ.common.changeData(true, 'OM21100');//co thay doi du lieu gan * tren tab title header
                    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                }
                else {
                    HQ.common.changeData(false, 'OM21100');
                }
                App.cboDiscID.setReadOnly(HQ.isChange0);
                App.cboDiscSeq.setReadOnly(HQ.isChange);
                //App.btnSearch.setReadOnly(HQ.isChange);
            }
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

                    if (!App.cboDiscClass.readOnly) {
                        App.cboDiscClass.setReadOnly(true);
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

            if (App[HQ.focus]) {
                if (App[HQ.focus].xtype == "grid") {
                    focusGrid = App[HQ.focus];
                    title = focusGrid.title;
                }
                else {
                    var tmpGrds = App[HQ.focus].down("gridpanel");
                    if (tmpGrds) {
                        if (!tmpGrds.length) {
                            focusGrid = tmpGrds;
                            title = App[HQ.focus].title;
                        }
                    }
                }
            }

            switch (command) {
                case "first":
                    if (HQ.focus == 'frmDiscDefintionTop')
                    {
                        HQ.combo.first(App.cboDiscID, HQ.isChange);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.first(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.first(focusGrid);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'frmDiscDefintionTop') {
                        HQ.combo.prev(App.cboDiscID, HQ.isChange);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.prev(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.prev(focusGrid);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'frmDiscDefintionTop') {
                        HQ.combo.next(App.cboDiscID, HQ.isChange);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.next(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.next(focusGrid);
                    }
                    break;
                case "last":
                    if (HQ.focus == 'frmDiscDefintionTop') {
                        HQ.combo.last(App.cboDiscID, HQ.isChange);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.last(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.last(focusGrid);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Main.Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'frmDiscDefintionTop') {
                            App.cboDiscID.store.reload();
                        }
                        else if (HQ.focus == 'frmDiscSeqInfo') {
                            App.cboDiscSeq.store.load(function (records, operation, success) {
                                App.stoDiscSeqInfo.reload();
                            });
                        }
                        else if (focusGrid) {
                            focusGrid.store.reload();
                            HQ.grid.first(focusGrid);
                        }
                    }
                    
                    break;
                case "new":
                    if (HQ.isInsert) {
                        App.cboDiscID.setValue("");
                    }
                    break;
                case "delete":
                    
                    if (HQ.isUpdate) {
                        if (HQ.focus == 'frmDiscDefintionTop') {
                            // Xoa discount
                        }
                        else if (HQ.focus == 'frmDiscSeqInfo') {
                            // Xoa disc seq
                        }
                        else if (focusGrid) {
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
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
                case "save":
                    Main.Process.saveData();
                    break;
                case "print":
                    break;
                case "close":
                    if (App["parentAutoLoadControl"] != undefined) {
                        App["parentAutoLoadControl"].close();
                    }
                    else {
                        parentAutoLoadControl.close();
                    }
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
            if (sto.getCount() > 0) {
                _isNewSeq = false;
            }
            else {
                _isNewSeq = true;
                var discSeqRec = Ext.create("App.mdlDiscSeqInfo", {
                    DiscID: App.cboDiscID.getValue(),
                    DiscSeq: App.cboDiscSeq.getValue(),
                    POStartDate: _dateNow,
                    POEndDate: _dateNow,
                    StartDate: _dateNow,
                    EndDate: _dateNow,
                    Status: _holdStatus,
                    POUse: false,
                    Active: 0,
                    Promo: 0,
                    AutoFreeItem: false,
                    AllowEditDisc: false
                });
                sto.insert(0, discSeqRec);
            }
            App.frmDiscSeqInfo.loadRecord(sto.getAt(0));
        },

        cboDiscID_change: function (cbo, newValue, oldValue, eOpts) {
            var discData = HQ.store.findInStore(cbo.store, ['DiscID'], [newValue]);
            if (!discData) {
                _isNewDisc = true;
                App.cboDiscSeq.setValue("");
                if (App.cboDiscClass.readOnly) {
                    App.cboDiscClass.setReadOnly(false);
                }
            }
            else {
                _isNewDisc = false;
                if (!App.cboDiscClass.readOnly) {
                    App.cboDiscClass.setReadOnly(true);
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
            App.grdDiscBreak.store.load(function () {
                App.grdFreeItem.store.load(function () {
                    App.slmDiscBreak.select(0);
                    cbo.focus();
                });
            });
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
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        slmDiscBreak_selectChange: function (grid, selected, eOpts) {
            var store = App.grdFreeItem.store;
            var keys = store.HQFieldKeys ? store.HQFieldKeys : "";
            var discID = App.cboDiscID.getValue();
            var discSeq = App.cboDiscSeq.getValue();

            store.clearFilter();
            if (selected.length > 0) {
                store.filterBy(function (record) {
                    if (record.data.LineRef == selected[0].data.LineRef) {
                        return record;
                    }
                });

                if (selected[0].data.LineRef && App.cboStatus.getValue() == _holdStatus) {
                    if (Main.Process.isSomeValidKey(store.getRecordsValues(), keys)) {
                        var newData = {
                            DiscID: discID,
                            DiscSeq: discSeq,
                            LineRef: selected[0].data.LineRef
                        };

                        HQ.store.insertRecord(store, keys, newData, false);
                    }
                }
            }
            else {
                store.filterBy(function (record) {
                    //if (record.data.InvtIDGroup == selected[0].data.InvtIDGroup) {
                    //    return record;
                    //}
                });
            }
        },

        grdDiscBreak_edit: function (editor, e) {
            var keys = ["BreakQty", "BreakAmt", "DiscAmt"];
            if (Main.Process.isSomeValidKey(e.store.getRecordsValues(), keys)) {
                var discId = App.cboDiscID.getValue();
                var discSeq = App.cboDiscSeq.getValue();

                var newData = {
                    DiscID: discId,
                    DiscSeq: discSeq
                };
                newData.LineRef = HQ.store.lastLineRef(e.store);

                HQ.store.insertRecord(e.store, keys, newData, false);

                if (!App.cboDiscClass.readOnly) {
                    App.cboDiscClass.setReadOnly(true);
                }
            }
        },

        grdFreeItem_edit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                var recordValues = e.store.getRecordsValues();
                if (e.value != ''
                    && Main.Process.isAllValidKey(recordValues, keys)) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var selRecs = App.grdDiscBreak.selModel.selected;
                    if (selRecs.length > 0) {
                        var newData = {
                            DiscID: discId,
                            DiscSeq: discSeq,
                            LineRef: selRecs.items[0].data.LineRef
                        };

                        HQ.store.insertRecord(e.store, keys, newData, false);

                        if (!App.cboDiscClass.readOnly) {
                            App.cboDiscClass.setReadOnly(true);
                        }
                    }
                }
            }
        },
    }
};
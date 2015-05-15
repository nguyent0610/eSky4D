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
                for (var i = 0; i < items.length; i++) {
                    for (var j = 0; j < keys.length; j++) {
                        if (items[i][keys[j]]) {
                            return true;
                        }
                    }
                }
                return false;
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
            App.stoDiscInfo.reload();
            App.stoDiscSeqInfo.reload();
        },

        checkEntireRequire: function () {
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
                return true;
            }
            else {
                return false;
            }
        },

        checkHasData: function () {
            var hasData = false;
            if (App.cboStatus.getValue() == _holdStatus) {
                if (App.grdDiscBreak.store.getCount() > 1 && App.grdCompany.store.getCount() > 0) {
                        // 0: {Code: "BB", Descr: "Item Bundle"}
                        if (App.cboDiscClass.value == "BB") {
                            if (App.grdBundle.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 1: {Code: "CB", Descr: "Customer and Item Bundle"}
                        else if (App.cboDiscClass.value == "CB") {
                            if (App.grdBundle.store.getCount() > 1 && App.grdDiscCust.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 2: {Code: "CC", Descr: "Customer"}
                        else if (App.cboDiscClass.value == "CC") {
                            if (App.grdDiscCust.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 3: {Code: "CI", Descr: "Customer and Invt. Item"}
                        else if (App.cboDiscClass.value == "CI") {
                            if (App.grdDiscCust.store.getCount() > 1 && App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 4: {Code: "II", Descr: "Inventory Item"}
                        else if (App.cboDiscClass.value == "II") {
                            if (App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 5: {Code: "PP", Descr: "Product Group"}
                        else if (App.cboDiscClass.value == "PP") {
                            if (App.grdDiscItemClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 6: {Code: "TB", Descr: "Shop Type and Item Bundle"}
                        else if (App.cboDiscClass.value == "TB") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdBundle.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 7: {Code: "TI", Descr: "Shop Type and Invt. Item"}
                        else if (App.cboDiscClass.value == "TI") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 8: {Code: "TP", Descr: "Prod. Group and Shop Type"}
                        else if (App.cboDiscClass.value == "TP") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdDiscItemClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        // 9: {Code: "TT", Descr: "Shop Type"}
                        else if (App.cboDiscClass.value == "TT") {
                            if (App.grdDiscCustClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                hasData = false;
                            }
                        }
                        else {
                            hasData = false;
                        }
                }
                else {
                    hasData = false;
                }
            }
            else {
                hasData = true;
            }

            return hasData;
        },

        saveData: function () {
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (App.frmMain.isValid()) { //if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    //var discId = App.cboDiscID.getValue();
                    //var discSeq = App.cboDiscSeq.getValue();
                    //var status = App.cboStatus.value;

                    //if (status == _holdStatus) {
                    if (Main.Process.checkHasData()) {
                        if (Main.Process.checkEntireRequire()) {
                            App.frmDiscDefintionTop.updateRecord();
                            App.frmDiscSeqInfo.updateRecord();

                            App.frmMain.submit({
                                waitMsg: HQ.common.getLang("SavingData"),
                                url: 'OM21100/SaveData',
                                timeout: 10000000,
                                params: {
                                    isNewDiscID: _isNewDisc,
                                    isNewDiscSeq: _isNewSeq,

                                    lstDiscInfo: Ext.encode([App.frmDiscDefintionTop.getRecord().data]),
                                    lstDiscBreakChange: HQ.store.getData(App.grdDiscBreak.store),
                                    lstFreeItemChange: HQ.store.getData(App.grdFreeItem.store),
                                    lstCompanyChange: HQ.store.getData(App.grdCompany.store),
                                    lstDiscItemChange: HQ.store.getData(App.grdDiscItem.store),
                                    lstBundleChange: HQ.store.getData(App.grdBundle.store),
                                    lstDiscCustClassChange: HQ.store.getData(App.grdDiscCustClass.store),
                                    lstDiscCustChange: HQ.store.getData(App.grdDiscCust.store),
                                    lstDiscItemClassChange: HQ.store.getData(App.grdDiscItemClass.store),

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
                    else {
                        HQ.message.show(744, '', '');
                    }
                    //}
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
                HQ.isChange0 = false;
                Main.Event.menuClick("refresh");
            }
        },

        deleteDisc: function () {
            if (HQ.isDelete) {
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'OM21100/DeleteDisc',
                    clientValidation: false,
                    timeout: 10000000,
                    params: {
                        discID: App.cboDiscID.getValue()
                    },
                    success: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        else {
                            HQ.message.show(201405071);
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
            else {
                HQ.message.show(4, '', '');
            }
        },

        deleteDiscSeq: function () {
            if (HQ.isDelete) {
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'OM21100/DeleteDiscSeq',
                    clientValidation: false,
                    timeout: 10000000,
                    params: {
                        discID: App.cboDiscID.getValue(),
                        discSeq: App.cboDiscSeq.getValue()
                    },
                    success: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        else {
                            HQ.message.show(201405071);
                        }
                        App.cboDiscSeq.store.reload();
                        App.stoDiscSeqInfo.reload();
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
                HQ.message.show(4, '', '');
            }
        }
    },

    Event: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            App.stoDiscInfo.reload();
            App.stoDiscSeqInfo.reload();

            HQ.common.setRequire(App.frmMain);
        },

        frmMain_fieldChange: function (frm, field, newValue, oldValue, eOpts) {
            var disc = App.frmDiscDefintionTop.getRecord();
            if (disc) {
                App.frmDiscDefintionTop.updateRecord();
                HQ.isChange0 = disc.dirty;
                
                var discSeq = App.frmDiscSeqInfo.getRecord();
                if (discSeq) {
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

                        var newRec = Ext.create(sto.model.modelName, newData);
                        HQ.store.insertRecord(sto, keys, newRec, false);
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

                    var newRec = Ext.create(e.store.model.modelName, newData);
                    HQ.store.insertRecord(e.store, keys, newRec, false);

                    if (!App.cboDiscClass.readOnly) {
                        App.cboDiscClass.setReadOnly(true);
                        App.cboDiscType.setReadOnly(true);
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
                        HQ.combo.first(App.cboDiscID, HQ.isChange || HQ.isChange0);
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
                        HQ.combo.prev(App.cboDiscID, HQ.isChange || HQ.isChange0);
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
                        HQ.combo.next(App.cboDiscID, HQ.isChange || HQ.isChange0);
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
                        HQ.combo.last(App.cboDiscID, HQ.isChange || HQ.isChange0);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.last(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.last(focusGrid);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange || HQ.isChange0) {
                        HQ.message.show(20150303, '', 'Main.Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'frmDiscDefintionTop') {
                            App.cboDiscID.store.load(function (records, operation, success) {
                                App.stoDiscInfo.reload();
                            });
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
                        if (HQ.isChange || HQ.isChange0) {
                            HQ.message.show(20150303, '', 'Main.Process.refresh');
                        }
                        else {
                            if (HQ.focus == 'frmDiscDefintionTop') {
                                App.cboDiscID.clearValue();
                            }
                            else if (HQ.focus == 'frmDiscSeqInfo') {
                                App.cboDiscSeq.clearValue();
                            }
                        }
                    }
                    break;
                case "delete":
                    
                    if (HQ.isUpdate) {
                        if (HQ.focus == 'frmDiscDefintionTop') {
                            // Xoa discount
                            Main.Process.deleteDisc();
                        }
                        else if (HQ.focus == 'frmDiscSeqInfo') {
                            // Xoa disc seq
                            Main.Process.deleteDiscSeq();
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

        renderFreeItemName: function (value, metaData, record, rowIndex, colIndex, store) {
            var rec = App.cboFreeItemID.store.findRecord("InvtID", record.data.FreeItemID);
            var returnValue = value;
            if (rec) {
                if (metaData.column.dataIndex == "Descr" && !record.data.Descr) {
                    returnValue = rec.data.Descr;
                }
                else if(metaData.column.dataIndex == "UnitDescr" && !record.data.UnitDescr){
                    returnValue = rec.data.StkUnit;
                }
            }

            return returnValue;
        },

        renderDpiiInvtName: function (value, metaData, record, rowIndex, colIndex, store) {
            var rec = App.cboDpiiInvtID.store.findRecord("InvtID", record.data.InvtID);
            var returnValue = value;
            if (rec) {
                if (metaData.column.dataIndex == "Descr" && !record.data.Descr) {
                    returnValue = rec.data.Descr;
                }
                else if (metaData.column.dataIndex == "UnitDesc" && !record.data.UnitDesc) {
                    returnValue = rec.data.StkUnit;
                }
            }

            return returnValue;
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
        stoDiscInfo_load: function (sto, records, successful, eOpts) {
            if (sto.getCount() > 0) {
                _isNewDisc = false;
            }
            else {
                _isNewDisc = true;
                App.cboDiscSeq.setValue("");
                var discRec = Ext.create("App.mdlDiscInfo", {
                    DiscID: App.cboDiscID.getValue()
                });
                sto.insert(0, discRec);
            }
            var frmRec = sto.getAt(0);
            App.frmDiscDefintionTop.loadRecord(frmRec);

            if (frmRec.data.tstamp) {
                App.cboDiscClass.setReadOnly(true);
                App.cboDiscType.setReadOnly(true);
            }
            else {
                App.cboDiscClass.setReadOnly(false);
                App.cboDiscType.setReadOnly(false);
            }

            Main.Event.frmMain_fieldChange();
        },

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
                    Active: false,
                    Promo: false,
                    AutoFreeItem: false,
                    AllowEditDisc: false
                });
                sto.insert(0, discSeqRec);
            }

            var discSeqRec = sto.getAt(0);
            App.frmDiscSeqInfo.loadRecord(discSeqRec);

            if(discSeqRec.data.tstamp){
                App.cboBreakBy.setReadOnly(true);
                App.cboDiscFor.setReadOnly(true);
            }
            else{
                App.cboBreakBy.setReadOnly(false);
                App.cboDiscFor.setReadOnly(false);
            }

            App.grdDiscBreak.store.load(function () {
                App.grdFreeItem.store.load(function () {
                    App.grdFreeItem.store.filterBy(function (record) { });
                });
            });
            App.grdCompany.store.reload();
            App.grdDiscItem.store.reload();
            App.grdBundle.store.reload();
            App.grdDiscCustClass.store.reload();
            App.grdDiscCust.store.reload();
            App.grdDiscItemClass.store.reload();

            Main.Event.frmMain_fieldChange();
        },

        cboDiscID_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoDiscInfo.reload();
            App.cboDiscSeq.store.load(function () {
                if (App.cboDiscSeq.store.getCount()) {
                    App.cboDiscSeq.setValue(App.cboDiscSeq.store.getAt(0).data.DiscSeq);
                }
                else {
                    App.cboDiscSeq.clearValue();
                }
            });
        },

        cboDiscType_change: function (cbo, newValue, oldValue, eOpts) {
            var frmRec = App.frmDiscDefintionTop.getRecord();
            if (!frmRec.data.tstamp) {
                if (cbo.value) {
                    if (oldValue) {
                        if (cbo.value
                            || App.grdDiscBreak.store.getCount() > 1
                            || App.grdBundle.store.getCount() > 1
                            || App.grdDiscCust.store.getCount() > 1
                            || App.grdDiscCustClass.store.getCount() > 1
                            || App.grdDiscItem.store.getCount() > 1
                            || App.grdDiscItemClass.store.getCount() > 1) {
                            HQ.message.show(43, "", "");

                            cbo.suspendEvents(false);
                            cbo.setValue(oldValue);
                            cbo.resumeEvents();
                            return;
                        }

                        if (App.cboDiscFor.value == "X" && cbo.value != "L") {
                            HQ.message.show(53, "", "");

                            cbo.suspendEvents(false);
                            cbo.setValue(oldValue);
                            cbo.resumeEvents();
                            return;
                        }
                    }


                    if (cbo.value == "L" || cbo.value == "G") {
                        App.cboDiscClass.setValue("II");

                    }
                    else {
                        App.cboDiscClass.setValue("CC");
                        App.cboBreakBy.setValue("A");
                    }
                }
            }
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
        },

        cboDiscFor_change: function (cbo, newValue, oldValue, eOpts) {
            var frmRec = App.frmDiscSeqInfo.getRecord();
            if (!frmRec.data.tstamp) {
                if (cbo.value && oldValue &&
                    (App.grdDiscBreak.store.getCount() > 1
                    || App.grdBundle.store.getCount() > 1
                    || App.grdDiscItem.store.getCount() > 1
                    || App.grdDiscCustClass.store.getCount() > 1
                    || App.grdDiscItemClass.store.getCount() > 1)) {
                    HQ.message.show(43, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                    return;
                }
                if (cbo.value == "X" && App.cboDiscType.value != "L") {
                    HQ.message.show(53, "","");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                    return;
                }
            }
        },

        cboBreakBy_change: function (cbo, newValue, oldValue, eOpts) {
            var discSeqRec = App.frmDiscSeqInfo.getRecord();
            if (!discSeqRec.data.tstamp) {
                if (App.cboDiscSeq.getValue() && oldValue && cbo.value
                    && (App.grdDiscBreak.store.getCount()>1 
                    || App.grdBundle.store.getCount()>1 
                    || App.grdDiscItem.store.getCount()>1 
                    || App.grdDiscItemClass.store.getCount()>1 
                    || App.grdDiscCust.store.getCount()>1 
                    || App.grdDiscCustClass.store.getCount()>1))
                {
                    HQ.message.show(43,"","");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                }
                else if((App.cboDiscClass.value=="CC" 
                    || App.cboDiscClass.value=="TT" 
                    || App.cboDiscType.value=="D") 
                    && cbo.value=="Q")
                {
                    HQ.message.show(53,"","");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                }
                else if(App.grdDiscBreak.store.getCount()>1 && 
                    (App.grdDiscBreak.store.getAt(0).BreakAmt>0 || App.grdDiscBreak.store.getAt(0).BreakQty>0 ))
                {
                    HQ.message.show(53,"","");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                } 
            }
            if (cbo.value == "W") {
                App.grdDiscBreak.down('[dataIndex=BreakQty]').setText(HQ.common.getLang("weight"));
            }
            else
                App.grdDiscBreak.down('[dataIndex=BreakQty]').setText(HQ.common.getLang("breakqty"));
        },

        cboProAplForItem_change: function (cbo, newValue, oldValue, eOpts) {
            var discSeqRec = App.frmDiscSeqInfo.getRecord();
            if (!discSeqRec.data.tstamp) {
                if (cbo.value == "M"
                    && App.cboDiscClass.value
                    && App.cboDiscClass.value.substring(1, 1) != "I") {

                    cbo.suspendEvents(false);
                    cbo.setValue("A");
                    cbo.resumeEvents();
                }
            }
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
                            App.treePanelBranch.clearChecked();
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
                            App.treePanelBranch.clearChecked();
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

                        var newRec = Ext.create(store.model.modelName, newData);
                        HQ.store.insertRecord(store, keys, newRec, false);
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

        grdDiscBreak_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
                        }

                        if (App.cboBreakBy.value == "A" && e.field == "BreakQty") {
                            return false;
                        }
                        else if (App.cboBreakBy.value == "Q" && App.cboDiscClass.value != "BB"
                            && App.cboDiscClass.value != "CB" && App.cboDiscClass.value != "TB"
                            && e.field == "BreakAmt") {
                            return false;
                        }
                        else if ((App.cboDiscClass.value == "BB" || App.cboDiscClass.value == "CB"
                            || App.cboDiscClass.value == "TB") && e.field == "BreakQty") {
                            return false;
                        }
                        else if (App.grdFreeItem.store.getCount() > 1 && e.field == "DiscAmt") {
                            return false
                        }
                        else if (e.rowIdx > 0 && e.store.getAt(e.rowIdx - 1).data.DiscAmt == 0 && e.field == "DiscAmt") {
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

        grdFreeItem_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
                        }

                        var selRec = App.slmDiscBreak.getSelection();
                        if (selRec) {
                            if (selRec[0].data.DiscAmt) {
                                return false;
                            }
                        }
                        else {
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

        grdDiscBreak_edit: function (editor, e) {
            var totalKeys = ["BreakQty", "BreakAmt", "DiscAmt"];

            var keys = [];
            if (totalKeys.indexOf(e.field)) {
                keys.push(e.field);
            }

            if (Main.Process.isSomeValidKey(e.store.getRecordsValues(), keys)) {
                var discId = App.cboDiscID.getValue();
                var discSeq = App.cboDiscSeq.getValue();

                var newData = {
                    DiscID: discId,
                    DiscSeq: discSeq,
                    LineRef: HQ.store.lastLineRef(e.store)
                };

                var newRec = Ext.create(e.store.model.modelName, newData);
                HQ.store.insertRecord(e.store, (keys.length? keys : totalKeys), newRec, false);

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

                        var newRec = Ext.create(e.store.model.modelName, newData);
                        HQ.store.insertRecord(e.store, keys, newRec, false);

                        if (!App.cboDiscClass.readOnly) {
                            App.cboDiscClass.setReadOnly(true);
                        }
                    }
                }
            }
        },
    }
};
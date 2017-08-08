var _holdStatus = "H";
var _gridForDel;
var _isNewDisc = false;
var _isNewSeq = false;
var _discLoad = "";
var _seqLoad = "";
var _selBranchID = '';
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
            App.cboDiscID.store.load();
            App.cboDiscSeq.store.load();
            App.stoDiscInfo.reload();
            App.stoDiscSeqInfo.reload();
        },

        checkEntireRequire: function () {
            if (Main.Process.checkRequire(App.grdDiscBreak.title, App.grdDiscBreak.store.getChangedData().Created, [], [])
                    && Main.Process.checkRequire(App.grdDiscBreak.title, App.grdDiscBreak.store.getChangedData().Updated, [], [])

                    && Main.Process.checkRequire(App.pnlFreeItem.title, App.grdFreeItem.store.getChangedData().Updated, ["FreeItemID"], ["FreeItemID"])
                    && Main.Process.checkRequire(App.pnlFreeItem.title, App.grdFreeItem.store.getChangedData().Updated, ["FreeItemID"], ["FreeItemID"])

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

                if (App.stoBundle.data.length > 0) {
                    var flat = null;
                    var breakByVal = App.cboBreakBy.getValue();
                    var disClassVal = App.cboDiscClass.getValue();
                    // Check Grid 
                    App.stoBundle.data.each(function (item) {
                        if (!Ext.isEmpty(item.data.InvtID) && (item.data.BundleOrItem == 'B' || disClassVal == 'BB' || disClassVal == "CB" || disClassVal == "TB")) {

                            if (item.data.BundleQty == 0 && breakByVal == 'Q') {
                                HQ.message.show(1111, [App.pnlDPBB.title, HQ.common.getLang('BreakQty')], '', true);
                                flat = item;
                                return false;
                            }
                            if (item.data.BundleAmt == 0 && breakByVal == 'A') {
                                HQ.message.show(1111, [App.pnlDPBB.title, HQ.common.getLang('BreakAmt')], '', true);
                                flat = item;
                                return false;
                            }
                        }
                    });
                    if (!Ext.isEmpty(flat)) {
                        App.slmBundle.select(App.stoBundle.indexOf(flat));
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    return true;
                }
            }
            else {

                return false;
            }
        },

        checkHasData: function () {
            var hasData = false;
            if (App.cboStatus.getValue() == _holdStatus) {
                if (App.grdDiscBreak.store.getCount() > 1 ||
                    (App.grdDiscBreak.store.getCount() == 1 &&
                        (App.grdDiscBreak.store.data.items[0].data.BreakQty > 0 ||
                            App.grdDiscBreak.store.data.items[0].data.BreakAmt > 0)
                    )) {
                    var flat = null;
                    var discClassVal = App.cboDiscClass.getValue();
                    var breakByVal = App.cboBreakBy.getValue();
                    if (discClassVal == "BB" || discClassVal == "CB" || discClassVal == "TB") {
                        var allData0 = App.grdFreeItem.store.snapshot || App.grdFreeItem.store.allData || App.grdFreeItem.store.data;
                        var isValid = false;
                        var showError = true;
                        App.grdDiscBreak.store.data.each(function (item) {
                            if (item.data.BreakAmt == 0) {
                                if (item.data.BreakQty != 0 || item.data.DiscAmt != 0 || item.data.Descr != '') {
                                    HQ.message.show(1111, [App.grdDiscBreak.title, App.grdDiscBreak.columns[2].text], '', true);
                                    flat = item;
                                    hasData = false;
                                    return false;
                                }
                            } else {
                                isValid = false;
                                if (item.data.DiscAmt == 0) {

                                    for (var i = 0; i < allData0.length; i++) {
                                        if (!Ext.isEmpty(allData0.items[i].data.FreeItemID) && allData0.items[i].data.LineRef == item.data.LineRef) {
                                            if (allData0.items[i].data.FreeItemQty == 0) {
                                                flat = item;
                                                showError = false;
                                                isValid = false;
                                                HQ.message.show(1111, [App.pnlFreeItem.title, App.grdFreeItem.columns[3].text], '', true);
                                                break;
                                            }
                                            isValid = true;
                                        }
                                    }
                                } else {
                                    isValid = true;
                                }
                                hasData = true;
                            }
                        });
                        if (!Ext.isEmpty(flat)) {
                            App.slmDiscBreak.select(App.grdDiscBreak.store.indexOf(flat));
                            return false;
                        }
                        if (!isValid && showError) {
                            HQ.message.show(1798);
                            return false;
                        }
                    } else {
                        App.grdDiscBreak.store.data.each(function (item) {
                            if (breakByVal == 'A') {
                                if (item.data.BreakAmt == 0) {
                                    if (item.data.BreakQty != 0 || item.data.DiscAmt != 0 || item.data.Descr != '') {
                                        HQ.message.show(1111, [App.grdDiscBreak.title, App.grdDiscBreak.columns[2].text], '', true);
                                        flat = item;
                                        hasData = false;
                                        return false;
                                    }
                                } else {
                                    hasData = true;
                                }
                            } else if (breakByVal == 'Q') {
                                if (item.data.BreakQty == 0) {
                                    if (item.data.BreakAmt != 0 || item.data.DiscAmt != 0 || item.data.Descr != '') {
                                        HQ.message.show(1111, [App.grdDiscBreak.title, App.grdDiscBreak.columns[1].text], '', true);
                                        flat = item;
                                        hasData = false;
                                        return false;
                                    }
                                } else {
                                    hasData = true;
                                }
                            }

                        });
                        if (!Ext.isEmpty(flat)) {
                            App.slmDiscBreak.select(App.grdDiscBreak.store.indexOf(flat));
                            return false;
                        }
                    }

                    if (!hasData) {
                        HQ.message.show(1000, HQ.common.getLang('DiscBreak'), '');
                        hasData = false;
                        return hasData;
                    }
                    if (App.grdCompany.store.getCount() > 0) {
                        // 0: {Code: "BB", Descr: "Item Bundle"}
                        if (App.cboDiscClass.value == "BB") {
                            if (App.grdBundle.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPBB'), '');
                                hasData = false;
                            }
                        }
                            // 1: {Code: "CB", Descr: "Customer and Item Bundle"}
                        else if (App.cboDiscClass.value == "CB") {
                            if (App.grdBundle.store.getCount() > 1 && App.grdDiscCust.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPBB') + ' / ' + HQ.common.getLang('DPCC'), '');
                                hasData = false;
                            }
                        }
                            // 2: {Code: "CC", Descr: "Customer"}
                        else if (App.cboDiscClass.value == "CC") {
                            if (App.grdDiscCust.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCC'), '');
                                hasData = false;
                            }
                        }
                            // 3: {Code: "CI", Descr: "Customer and Invt. Item"}
                        else if (App.cboDiscClass.value == "CI") {
                            if (App.grdDiscCust.store.getCount() > 1 && App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCC') + ' / ' + HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }
                            // 4: {Code: "II", Descr: "Inventory Item"}
                        else if (App.cboDiscClass.value == "II") {
                            if (App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }
                            // 5: {Code: "PP", Descr: "Product Group"}
                        else if (App.cboDiscClass.value == "PP") {
                            if (App.grdDiscItemClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPPP'), '');
                                hasData = false;
                            }
                        }
                            // 6: {Code: "TB", Descr: "Shop Type and Item Bundle"}
                        else if (App.cboDiscClass.value == "TB") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdBundle.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT') + ' / ' + HQ.common.getLang('DPBB'), '');
                                hasData = false;
                            }
                        }
                            // 7: {Code: "TI", Descr: "Shop Type and Invt. Item"}
                        else if (App.cboDiscClass.value == "TI") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT') + ' / ' + HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }
                            // 8: {Code: "TP", Descr: "Prod. Group and Shop Type"}
                        else if (App.cboDiscClass.value == "TP") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdDiscItemClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT') + ' / ' + HQ.common.getLang('DPPP'), '');
                                hasData = false;
                            }
                        }
                            // 9: {Code: "TT", Descr: "Shop Type"}
                        else if (App.cboDiscClass.value == "TT") {
                            if (App.grdDiscCustClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT'), '');
                                hasData = false;
                            }
                        }
                        else {
                            hasData = false;
                        }
                    }
                    else {
                        HQ.message.show(1000, HQ.common.getLang('AppComp'), '');
                        hasData = false;
                    }
                }
                else {
                    HQ.message.show(1000, HQ.common.getLang('DiscBreak'), '');
                    hasData = false;
                }
            }
            else {
                hasData = true;
            }

            return hasData;
        },

        getChangedFilteredData: function (store) {
            var data = store.data,
                changedData

            store.data = store.snapshot; // does the trick
            changedData = store.getChangedData();
            store.data = data; // to revert the changes back
            return Ext.encode(changedData);
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
                                    lstFreeItemChange: Main.Process.getChangedFilteredData(App.grdFreeItem.store),
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
                //App.stoDiscSeqInfo.reload();
                Main.Event.menuClick("refresh");
            }
        },

        deleteDisc: function (item) {
            if (item == 'yes') {
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
            }
        },

        deleteDiscSeq: function (item) {
            if (item == 'yes') {
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
                                App.cboDiscSeq.setValue('');
                                HQ.message.show(201405071);
                            }
                            App.cboDiscSeq.store.load();
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

        checkAddFreeItem: function () {
            var isAllowInsert = true;
            //for (var i = 0; i < App.grdDiscBreak.store.data.length; i++) {
                if (App.grdDiscBreak.selModel.selected.items[0].data.DiscAmt) {
                    isAllowInsert = false;
                  //  break;
                }
           // }
            if (!isAllowInsert || App.grdDiscBreak.store.data.length == 0) {
                return false;
            }
            return true;
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
                    if (!discSeq.dirty) {
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
                var idxLref = keys.indexOf("LineRef");

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
                        if (sto.storeId == 'stoDiscCust' || sto.storeId == 'stoDiscItem' || sto.storeId == 'stoBundle') {
                            if (sto.data.length < sto.pageSize ||
                                sto.currentPage == (sto.totalCount / sto.pageSize)
                                ) {
                                var obj = HQ.store.findRecord(sto, keys, [discId, discSeq, '', '', '', '', '']);
                                if (!obj) {
                                    var newRec = Ext.create(sto.model.modelName, newData);
                                    HQ.store.insertRecord(sto, keys, newRec, false);
                                }
                            }
                        } else {
                            var obj = HQ.store.findRecord(sto, keys, [discId, discSeq, '', '', '', '', '']);
                            if (!obj) {
                                var newRec = Ext.create(sto.model.modelName, newData);
                                HQ.store.insertRecord(sto, keys, newRec, false);
                            }
                        }

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
                        if (e.store.storeId == 'stoBundle') {
                            if ((e.field == 'BundleQty' && App.cboBreakBy.getValue() != 'Q')
                                || (e.field == 'BundleAmt' && App.cboBreakBy.getValue() != 'A')) {
                                return false;
                            }
                        }
                        if (e.field == 'UnitDesc') {
                            if (e.store.storeId == 'stoDiscItem') {
                                App.cboGItemUnitDescr.store.reload();
                            }
                            else if (e.store.storeId == 'stoBundle') {
                                App.cboGInvtUnitDescr.store.reload();
                            }
                            else if (e.store.storeId == 'stoFreeItem') {
                                App.cboGUnitDescr.store.reload();
                            }
                            else if (e.store.storeId == 'stoDiscItemClass') {
                                App.cboGClassUnitDescr.store.reload();
                            }
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
                //var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                //if (e.value && !e.value.match(regex)) {
                //    HQ.message.show(20140811, e.column.text);
                //    return false;
                //}
                if (e.store.storeId == 'stoFreeItem') {
                    keys = ['DiscID', 'DiscSeq', 'LineRef', 'FreeItemID'];
                }
                if (e.store.storeId == 'stoDiscCust') {
                    e.record.set('BranchID', _selBranchID);
                    if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                        e.record.set('BranchID', '');
                        HQ.message.show(1112, e.value);
                        return false;
                    }
                } else {
                    if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                        HQ.message.show(1112, e.value);
                        return false;
                    }
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
                    if (HQ.focus == 'frmDiscDefintionTop') {
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
                        else {//if (HQ.focus == 'frmDiscSeqInfo') {
                            App.cboDiscSeq.store.load(function (records, operation, success) {
                                App.stoDiscSeqInfo.reload();
                            });
                        }
                        if (focusGrid) {
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
                            HQ.message.show(11, '', 'Main.Process.deleteDisc', true);
                            //Main.Process.deleteDisc();
                        }
                        else if (HQ.focus == 'frmDiscSeqInfo') {
                            // Xoa disc seq
                            HQ.message.show(11, '', 'Main.Process.deleteDiscSeq', true);
                            // Main.Process.deleteDiscSeq();
                        }
                        else if (focusGrid) {
                            var selected = focusGrid.getSelectionModel().selected.items;
                            if (selected.length > 0) {
                                _gridForDel = focusGrid;

                                if (HQ.focus == 'grdFreeItem') {
                                    HQ.message.show(2015020807, DiscDefintion.Process.indexSelect(App.grdFreeItem), 'Main.Process.deleteSelectedInGrid');
                                } else {
                                    if (selected[0].index != undefined) {
                                        var rowIdx = '';
                                        for (var i = 0; i < selected.length; i++) {
                                            rowIdx += (focusGrid.getSelectionModel().selected.items[i].index + 1) + ' & ';
                                        }
                                        var params = rowIdx.length > 3 ? rowIdx.substring(0, rowIdx.length - 3) : '';// + title;
                                        HQ.message.show(2015020807, params, 'Main.Process.deleteSelectedInGrid');
                                    }
                                    else {
                                        HQ.message.show(11, '', 'Main.Process.deleteSelectedInGrid');
                                    }
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
                 });
            }
            return allNodes;
        },

        getLeafNodes: function (node) {
            var childNodes = [];
            node.eachChild(function (child) {
                if (child.isLeaf()) {
                    childNodes.push(child);
                }
                else {
                    var children = DiscDefintion.Process.getLeafNodes(child);
                    if (children.length) {
                        children.forEach(function (nill) {
                            childNodes.push(nill);
                        });
                    }
                }
            });
            return childNodes;
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
                else if (metaData.column.dataIndex == "UnitDescr" && !record.data.UnitDescr) {
                    returnValue = rec.data.StkUnit;
                    //record.set("UnitDescr", rec.data.StkUnit);
                    record.data.UnitDescr = returnValue;
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
                    //record.set("UnitDesc", rec.data.StkUnit);
                    record.data.UnitDesc = returnValue;
                }
            }

            return returnValue;
        },

        renderGInvtName: function (value, metaData, record, rowIndex, colIndex, store) {
            var rec = App.cboGInvtID.store.findRecord("InvtID", record.data.InvtID);
            var returnValue = value;
            if (rec) {
                if (metaData.column.dataIndex == "Descr" && !record.data.Descr) {
                    returnValue = rec.data.Descr;
                }
                else if (metaData.column.dataIndex == "UnitDesc" && !record.data.UnitDesc) {
                    returnValue = rec.data.StkUnit;
                    //record.set("UnitDesc", rec.data.StkUnit);
                    record.data.UnitDesc = returnValue;
                }
            }

            return returnValue;
        },

        indexSelect: function (grd) {
            var index = '';
            var allData = grd.store.data;
            var arr = grd.getSelectionModel().selected.items;
            arr.forEach(function (itm) {
                index += (allData.indexOfKey(itm.internalId) + 1) + ' & ';
            });
            return index.substring(0, index.length - 3);
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

        deleteAllFreeItem: function (item) {
            if (item == "yes") {
                App.grdFreeItem.store.suspendEvents();
                while (App.grdFreeItem.store.data.length > 0) {
                    App.grdFreeItem.store.removeAt(0);
                }
                App.grdFreeItem.store.resumeEvents();
                App.grdFreeItem.view.refresh();
                var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                if (!invtBlank) {
                    App.grdFreeItem.store.insert(0, Ext.create("App.mdlFreeItem", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        FreeItemID: '',
                        LineRef: lineRef
                    }));
                }
            }
        },

        deleteAllInvts: function (item) {
            if (item == "yes") {


                //App.grdDiscItem.store.suspendEvents();
                //var store = App.grdDiscItem.store.snapshot || App.grdDiscItem.store.allData || App.grdDiscItem.store.data;
                //var index = 0;
                //while (store.length > 0) {
                //    App.grdDiscItem.getSelectionModel().select(store.items[0]);
                //    App.grdDiscItem.deleteSelected();
                //    index++;
                //    if (index >= App.grdDiscItem.store.pageSize) {
                //        App.grdDiscItem.view.refresh();
                //        App.grdDiscItem.store.loadPage(1);
                //        index = 0;
                //    }
                //}
                //App.grdDiscItem.store.resumeEvents();

                App.grdDiscItem.store.removeAll();
                // App.grdDiscItem.store.submitData();
                App.grdDiscItem.view.refresh();
                App.grdDiscItem.store.loadPage(1);
                var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                if (!invtBlank) {
                    App.grdDiscItem.store.insert(0, Ext.create("App.mdlDiscItem", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        InvtID: '',
                        UnitDesc: '',
                        Descr: ''
                    }));
                }
            }
        },

        deleteAllBundle: function (item) {
            if (item == "yes") {
                App.grdBundle.store.removeAll();
                App.grdBundle.store.submitData();
                App.grdBundle.view.refresh();
                App.grdBundle.store.loadPage(1);
                var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                if (!invtBlank) {
                    App.grdBundle.store.insert(0, Ext.create("App.mdlBundle", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        InvtID: '',
                        UnitDesc: '',
                        Descr: '',
                        BundleQty: 0,
                        BundleAmt: 0
                    }));
                }
            }
        },

        deleteAllCust: function (item) {
            if (item == "yes") {
                App.grdDiscCust.store.removeAll();
                App.grdDiscCust.store.submitData();
                App.grdDiscCust.view.refresh();
                App.grdDiscCust.store.loadPage(1);
                var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID'], ['']);
                if (!invtBlank) {
                    App.grdDiscCust.store.insert(0, Ext.create("App.mdlDiscCust", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        CustID: ''
                    }));
                }

            }
        },
    },

    Event: {
        storeGridLoaded : function(sto) {
            HQ.gridSource++;
            if (HQ.gridSource == 8) {
                App.frmMain.unmask();
                App.grdFreeItem.store.filterBy(function (record) { });
                HQ.common.showBusy(false);
                //HQ.gridSource = 0;
            }
        }

        , stoDiscInfo_load: function (sto, records, successful, eOpts) {
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
            _discLoad = frmRec.data.DiscID;

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
            _seqLoad = discSeqRec.data.DiscSeq;

            if (discSeqRec.data.tstamp) {
                App.cboBreakBy.setReadOnly(true);
                App.cboDiscFor.setReadOnly(true);
            }
            else {
                App.cboBreakBy.setReadOnly(false);
                App.cboDiscFor.setReadOnly(false);
            }

            if (discSeqRec.data.Status == _holdStatus) {
                App.cboProAplForItem.setReadOnly(false);
                App.dteStartDate.setReadOnly(false);
                App.dteEndDate.setReadOnly(false);
                App.chkDiscTerm.setReadOnly(false);
                App.chkAutoFreeItem.setReadOnly(false);
                App.cboBudgetID.setReadOnly(false);
                App.txtSeqDescr.setReadOnly(false);
            }
            else {
                App.cboProAplForItem.setReadOnly(true);
                App.dteStartDate.setReadOnly(true);
                App.dteEndDate.setReadOnly(true);
                App.chkDiscTerm.setReadOnly(true);
                App.chkAutoFreeItem.setReadOnly(true);
                App.cboBudgetID.setReadOnly(true);
                App.txtSeqDescr.setReadOnly(true);
            }

            HQ.gridSource = 0;

            App.grdDiscBreak.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdFreeItem.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdCompany.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscItem.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdBundle.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscCustClass.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscCust.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscItemClass.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.frmMain.mask();
            App.grdDiscBreak.store.reload();
            App.grdFreeItem.store.reload();
            App.grdCompany.store.reload();
            App.grdDiscItem.store.reload();
            App.grdBundle.store.reload();
            App.grdDiscCustClass.store.reload();
            App.grdDiscCust.store.reload();
            App.grdDiscItemClass.store.reload();

            Main.Event.frmMain_fieldChange();
        },

        cboDiscID_change: function (cbo, newValue, oldValue, eOpts) {
            //var selRec = HQ.store.findInStore(cbo.store, ["DiscID"], [cbo.getValue()]);
            //if (selRec || !_isNewDisc) {
            if (HQ.util.passNull(cbo.getValue()) != _discLoad && !cbo.hasFocus) {
                App.stoDiscInfo.reload();
                App.cboDiscSeq.store.load(function () {
                    if (App.cboDiscSeq.store.getCount()) {
                        var discSeqValue = App.cboDiscSeq.store.getAt(0).data.DiscSeq;
                        if (discSeqValue == App.cboDiscSeq.getValue()) {
                            App.cboDiscSeq.setValue(discSeqValue);
                            App.stoDiscSeqInfo.reload();
                        }
                        else {
                            App.cboDiscSeq.setValue(discSeqValue);
                        }
                    }
                    else {
                        App.cboDiscSeq.clearValue();
                    }
                });
            }
            //}
        },

        cboDiscID_select: function (cbo, newValue, oldValue, eOpts) {
            App.stoDiscInfo.reload();
            App.cboDiscSeq.store.load(function () {
                if (App.cboDiscSeq.store.getCount()) {
                    var discSeqValue = App.cboDiscSeq.store.getAt(0).data.DiscSeq;
                    if (discSeqValue == App.cboDiscSeq.getValue()) {
                        App.cboDiscSeq.setValue(discSeqValue);
                        App.stoDiscSeqInfo.reload();
                    }
                    else {
                        App.cboDiscSeq.setValue(discSeqValue);
                    }
                }
                else {
                    App.cboDiscSeq.clearValue();
                }
            });
        },

        cboDiscType_change: function (cbo, newValue, oldValue, eOpts) {
            var frmRec = App.frmDiscDefintionTop.getRecord();
            //if (!frmRec.data.tstamp) {
            //    if (cbo.value) {
            //        if (oldValue) {
            //            if (cbo.value
            //                || App.grdDiscBreak.store.getCount() > 1
            //                || App.grdBundle.store.getCount() > 1
            //                || App.grdDiscCust.store.getCount() > 1
            //                || App.grdDiscCustClass.store.getCount() > 1
            //                || App.grdDiscItem.store.getCount() > 1
            //                || App.grdDiscItemClass.store.getCount() > 1) {
            //                HQ.message.show(43, "", "");

            //                cbo.suspendEvents(false);
            //                cbo.setValue(oldValue);
            //                cbo.resumeEvents();
            //                return;
            //            }

            //            if (App.cboDiscFor.value == "X" && cbo.value != "L") {
            //                HQ.message.show(53, "", "");

            //                cbo.suspendEvents(false);
            //                cbo.setValue(oldValue);
            //                cbo.resumeEvents();
            //                return;
            //            }
            //        }


            //        if (cbo.value == "L" || cbo.value == "G") {
            //            App.cboDiscClass.setValue("II");

            //        }
            //        else {
            //            App.cboDiscClass.setValue("CC");
            //            App.cboBreakBy.setValue("A");
            //        }
            //    }
            //}
            App.cboDiscClass.clearValue();
            App.cboDiscClass.store.load(function () {
                if (frmRec.data.tstamp) {
                    App.cboDiscClass.setValue(frmRec.raw.DiscClass);
                }
            });
        },

        cboDiscClass_change: function (cbo, newValue, oldValue, eOpts) {
            // 0: {Code: "BB", Descr: "Item Bundle"}
            var isbb = false;
            if (cbo.value == "BB") {
                DiscDefintion.Process.enableATabInList(["pnlDPBB"]);
                isbb = true;
            }
                // 1: {Code: "CB", Descr: "Customer and Item Bundle"}
            else if (cbo.value == "CB") {
                DiscDefintion.Process.enableATabInList(["pnlDPBB", "pnlDPCC"]);
                isbb = true;
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
                isbb = true;
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

            if (App.cboProAplForItem.value == "M"
                    && cbo.value
                    && cbo.value.substring(1) != "I") {
                App.cboProAplForItem.setValue("A");
            }
            if (isbb) {
                App.grdDiscBreak.columns[2].setText(HQ.common.getLang('NoBundle'));
            } else {
                App.grdDiscBreak.columns[2].setText(HQ.common.getLang('BreakAmt'));
            }
        },

        cboDiscSeq_change: function (cbo, newValue, oldValue, eOpts) {
            //var selRec = HQ.store.findInStore(cbo.store, ["DiscSeq"], [cbo.getValue()]);
            //if (selRec || !_isNewSeq) {
            if (HQ.util.passNull(cbo.getValue()) != _seqLoad && !cbo.hasFocus) {
                App.stoDiscSeqInfo.reload();
            }
            //}
        },

        cboDiscSeq_select: function (cbo, newValue, oldValue, eOpts) {
            if (!App.stoDiscSeqInfo.loading) {
                App.stoDiscSeqInfo.reload();
            }
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
                    HQ.message.show(53, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                    return;
                }
            }

            var discAmtCol = App.grdDiscBreak.down('[dataIndex=DiscAmt]');
            var discAmtField = discAmtCol.getEditor().field;

            if (App.cboDiscFor.getValue() == "A") {
                discAmtCol.format = "0,000";
                discAmtField.decimalPrecision = 0;
            }
            else if (App.cboDiscFor.getValue() == "P") {
                discAmtCol.format = "0,000.00";
                discAmtField.decimalPrecision = 2;
            }
        },

        cboBreakBy_change: function (cbo, newValue, oldValue, eOpts) {
            var discSeqRec = App.frmDiscSeqInfo.getRecord();
            if (!discSeqRec.data.tstamp) {
                if (App.cboDiscSeq.getValue() && oldValue && cbo.value
                    && (App.grdDiscBreak.store.getCount() > 1
                    || App.grdBundle.store.getCount() > 1
                    || App.grdDiscItem.store.getCount() > 1
                    || App.grdDiscItemClass.store.getCount() > 1
                    || App.grdDiscCust.store.getCount() > 1
                    || App.grdDiscCustClass.store.getCount() > 1)) {
                    HQ.message.show(43, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                }
                else if ((App.cboDiscClass.value == "CC"
                    || App.cboDiscClass.value == "TT"
                    || App.cboDiscType.value == "D")
                    && cbo.value == "Q") {
                    HQ.message.show(53, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                }
                else if (App.grdDiscBreak.store.getCount() > 1 &&
                    (App.grdDiscBreak.store.getAt(0).BreakAmt > 0 || App.grdDiscBreak.store.getAt(0).BreakQty > 0)) {
                    HQ.message.show(53, "", "");

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
                    && App.cboDiscClass.value.substring(1) != "I") {

                    cbo.suspendEvents(false);
                    cbo.setValue("A");
                    cbo.resumeEvents();
                }
                else {
                    
                }

                if (cbo.value == "A") {
                    App.chkAutoFreeItem.setDisabled(true);
                    App.chkAutoFreeItem.setValue(false);
                }
                else if (cbo.value == "M") {
                    App.chkAutoFreeItem.setDisabled(true);
                    App.chkAutoFreeItem.setValue(true);
                }
                else {
                    App.chkAutoFreeItem.setDisabled(false);
                    App.chkAutoFreeItem.setValue(false);
                }
            }
        },


        cboGCustID_Change: function (item, newValue, oldValue, eOpts) {
            _selBranchID = '';
            if (item.valueModels != undefined && item.valueModels[0]) {
                _selBranchID = item.valueModels[0].data.BranchID;
            }
        },

        treePanelBranch_checkChange: function (node, checked, eOpts) {
            if (App.cboStatus.getValue() == _holdStatus) {
                node.childNodes.forEach(function (childNode) {
                    childNode.set("checked", checked);
                });
            } else {
                App.treePanelBranch.clearChecked();
            }
        },

        treePanelFreeItem_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelFreeItem_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelFreeItem.clearChecked();
            }
        },

        treePanelInvt_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelInvt_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelInvt.clearChecked();
            }
        },

        treePanelBundle_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelBundle_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelBundle.clearChecked();
            }
        },

        treePanelCustomer_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelCustomer_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelCustomer.clearChecked();
            }
        },

        btnExpand_click: function (btn, e, eOpts) {
            App.treePanelBranch.expandAll();
        },

        btnCollapse_click: function (btn, e, eOpts) {
            App.treePanelBranch.collapseAll();
        },

        btnFreeItemExpand_click: function (btn, e, eOpts) {
            App.treePanelFreeItem.getStore().suspendEvents();
            App.treePanelFreeItem.expandAll();
            App.treePanelFreeItem.getStore().resumeEvents();
        },

        btnFreeItemCollapse_click: function (btn, e, eOpts) {
            App.treePanelFreeItem.collapseAll();
        },

        btnInvtExpand_click: function (btn, e, eOpts) {
            App.treePanelInvt.getStore().suspendEvents();
            App.treePanelInvt.expandAll();
            App.treePanelInvt.getStore().resumeEvents();
        },

        btnInvtCollapse_click: function (btn, e, eOpts) {
            App.treePanelInvt.collapseAll();
        },

        btnBundleExpand_click: function (btn, e, eOpts) {
            App.treePanelBundle.getStore().suspendEvents();
            App.treePanelBundle.expandAll();
            App.treePanelBundle.getStore().resumeEvents();
        },

        btnBundleCollapse_click: function (btn, e, eOpts) {
            App.treePanelBundle.collapseAll();
        },

        btnCustomerExpand_click: function (btn, e, eOpts) {
            App.treePanelCustomer.getStore().suspendEvents();
            App.treePanelCustomer.expandAll();
            App.treePanelCustomer.getStore().resumeEvents();
        },

        btnCustomerCollapse_click: function (btn, e, eOpts) {
            App.treePanelCustomer.collapseAll();
        },

        // Company
        btnAddAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = DiscDefintion.Process.getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
                        if (allNodes && allNodes.length > 0) {
                            App.grdCompany.store.suspendEvents();
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
                            App.grdCompany.store.resumeEvents();
                            App.grdCompany.view.refresh();
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
                            App.grdCompany.store.suspendEvents();
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
                            App.grdCompany.store.resumeEvents();
                            App.grdCompany.view.refresh();
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
                        Main.Event.menuClick('delete');
                        //var selRecs = App.grdCompany.selModel.selected.items;
                        //if (selRecs.length > 0) {
                        //    var params = [];
                        //    selRecs.forEach(function (record) {
                        //        params.push(record.data.CpnyID);
                        //    });
                        //    HQ.message.show(2015020806,
                        //        params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                        //        'DiscDefintion.Process.deleteSelectedCompanies');
                        //}
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

        // Free Item

        btnAddAllFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                        var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelFreeItem.getRootNode());
                        if (allNodes && allNodes.length > 0) {
                            App.grdFreeItem.store.suspendEvents();

                            var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                            if (!invtBlank) {
                                var idx = App.grdFreeItem.store.data.length > 0 ? App.grdFreeItem.store.data.length - 1 : 0;
                                App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    FreeItemID: '',
                                    LineRef: lineRef
                                }));
                            }
                            var allNodeLength = allNodes.length;
                            var idx = App.grdFreeItem.store.getCount() - 1;
                            for (var i = 0; i < allNodeLength; i++) {
                                if (allNodes[i].data.Type == "Invt") {
                                    var record = HQ.store.findInStore(App.grdFreeItem.store,
                                        ['DiscID', 'DiscSeq', 'FreeItemID', 'LineRef'],
                                        [discId, discSeq, allNodes[i].data.InvtID, lineRef]);
                                    if (!record) {
                                        App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            FreeItemID: allNodes[i].data.InvtID,
                                            LineRef: lineRef
                                        }));
                                        idx++;
                                    }
                                }
                            }

                            App.treePanelFreeItem.clearChecked();
                            App.grdFreeItem.store.resumeEvents();
                            App.grdFreeItem.view.refresh();
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

        btnAddFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {


                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                        var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                        var allNodes = App.treePanelFreeItem.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.grdFreeItem.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                            if (!invtBlank) {
                                //HQ.store.insertBlank(App.grdDiscItem.store, ['InvtID']);
                                var idx = App.grdFreeItem.store.data.length > 0 ? App.grdFreeItem.store.data.length - 1 : 0;
                                App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    FreeItemID: '',
                                    LineRef: lineRef
                                }));
                            }
                            var idx = App.grdFreeItem.store.getCount() - 1;
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "Invt") {

                                    var record = HQ.store.findInStore(App.grdFreeItem.store,
                                        ['DiscID', 'DiscSeq', 'FreeItemID', 'LineRef'],
                                        [discId, discSeq, node.attributes.InvtID, lineRef]);
                                    if (!record) {
                                        App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            FreeItemID: node.attributes.InvtID,
                                            LineRef: lineRef
                                        }));
                                    }
                                }
                            });

                            App.treePanelFreeItem.clearChecked();
                            App.grdFreeItem.store.resumeEvents();
                            App.grdFreeItem.view.refresh();

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

        btnDelFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        //HQ.message.show(2015020807, DiscDefintion.Process.indexSelect(App.grdFreeItem), 'Main.Process.deleteSelectedInGrid');//
                        Main.Event.menuClick('delete');
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

        btnDelAllFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllFreeItem');
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

        // Item
        btnAddAllInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelInvt.getRootNode());
                        if (allNodes && allNodes.length > 0) {
                            App.grdDiscItem.store.suspendEvents();

                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdDiscItem.store.remove(invtBlank);
                            }

                            var allNodeLength = allNodes.length;
                            for (var i = 0; i < allNodeLength; i++) {
                                if (allNodes[i].data.Type == "Invt") {
                                    var record = HQ.store.findInStore(App.grdDiscItem.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, allNodes[i].data.InvtID]);
                                    if (!record) {
                                        App.grdDiscItem.store.insert(0, Ext.create("App.mdlDiscItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: allNodes[i].data.InvtID,
                                            UnitDesc: allNodes[i].data.Unit,
                                            Descr: allNodes[i].data.Descr
                                        }));
                                        // idx++;
                                    }
                                }
                            }
                            App.treePanelInvt.clearChecked();
                            App.grdDiscItem.store.resumeEvents();
                            App.grdDiscItem.view.refresh();
                            App.grdDiscItem.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdDiscItem.store.data.length > 0 ? App.grdDiscItem.store.data.length - 1 : 0;
                                App.grdDiscItem.store.insert(idx, Ext.create("App.mdlDiscItem", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: ''
                                }));
                            }

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

        btnAddInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelInvt.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.grdDiscItem.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdDiscItem.store.remove(invtBlank);
                            }
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "Invt") {
                                    var idx = App.grdDiscItem.store.getCount() - 1;
                                    var record = HQ.store.findInStore(App.grdDiscItem.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, node.attributes.InvtID]);
                                    if (!record) {
                                        App.grdDiscItem.store.insert(idx, Ext.create("App.mdlDiscItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: node.attributes.InvtID,
                                            UnitDesc: node.attributes.Unit,
                                            Descr: node.attributes.Descr
                                        }));
                                    }
                                }
                            });

                            App.treePanelInvt.clearChecked();
                            App.grdDiscItem.store.resumeEvents();
                            App.grdDiscItem.view.refresh();
                            App.grdDiscItem.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdDiscItem.store.data.length > 0 ? App.grdDiscItem.store.data.length - 1 : 0;
                                App.grdDiscItem.store.insert(idx, Ext.create("App.mdlDiscItem", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: ''
                                }));
                            }
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

        btnDelInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        Main.Event.menuClick('delete');
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

        btnDelAllInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllInvts');
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

        // Bundle
        btnAddAllBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelBundle.getRootNode());
                        if (allNodes && allNodes.length > 0) {
                            App.grdBundle.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdBundle.store.remove(invtBlank);
                            }
                            allNodes.forEach(function (node) {
                                if (node.data.Type == "Invt") {
                                    var idx = App.grdBundle.store.getCount() - 1;
                                    var record = HQ.store.findInStore(App.grdBundle.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, node.data.InvtID]);
                                    if (!record) {
                                        App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: node.data.InvtID,
                                            UnitDesc: node.data.Unit,
                                            Descr: node.data.Descr,
                                            BundleQty: 0,
                                            BundleAmt: 0
                                        }));
                                    }
                                }
                            });

                            App.treePanelBundle.clearChecked();
                            App.grdBundle.store.resumeEvents();
                            App.grdBundle.view.refresh();
                            App.grdBundle.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdBundle.store.data.length > 0 ? App.grdBundle.store.data.length - 1 : 0;
                                App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: '',
                                    BundleQty: 0,
                                    BundleAmt: 0
                                }));
                            }
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

        btnAddBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelBundle.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.grdBundle.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdBundle.store.remove(invtBlank);
                            }
                            var idx = App.grdBundle.store.getCount() - 1;
                            var allNodeLength = allNodes.length;
                            for (var i = 0; i < allNodeLength; i++) {
                                if (allNodes[i].attributes.Type == "Invt") {

                                    var record = HQ.store.findInStore(App.grdBundle.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, allNodes[i].attributes.InvtID]);
                                    if (!record) {
                                        App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: allNodes[i].attributes.InvtID,
                                            UnitDesc: allNodes[i].attributes.Unit,
                                            Descr: allNodes[i].attributes.Descr,
                                            BundleQty: 0,
                                            BundleAmt: 0
                                        }));
                                        idx++;
                                    }
                                }
                            }
                            App.treePanelBundle.clearChecked();
                            App.grdBundle.store.resumeEvents();
                            App.grdBundle.view.refresh();
                            App.grdBundle.store.loadPage(1);
                            if (!invtBlank) {
                                var idx = App.grdBundle.store.data.length > 0 ? App.grdBundle.store.data.length - 1 : 0;
                                App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: '',
                                    BundleQty: 0,
                                    BundleAmt: 0
                                }));
                            }
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

        btnDelBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        Main.Event.menuClick('delete');
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

        btnDelAllBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllBundle');
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

        // Customer
        btnAddAllCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        // App.frmMain.body.mask();
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelCustomer.getRootNode());
                        if (allNodes && allNodes.length > 0) {
                            App.grdDiscCust.store.suspendEvents();
                            //while (App.grdDiscCust.store.data.length > 0) {
                            //    App.grdDiscCust.store.removeAt(0);
                            //}
                            //App.grdDiscCust.store.removeAll();
                            //App.grdDiscCust.store.submitData();
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID'], ['']);
                            if (invtBlank) {
                                App.grdDiscCust.store.remove(invtBlank);
                            }
                            var allNodeLength = allNodes.length;
                            var idx = App.grdDiscCust.store.getCount() - 1;
                            for (var i = 0; i < allNodeLength; i++) {
                                if (allNodes[i].data.Type == "CustID") {
                                    var record = HQ.store.findInStore(App.grdDiscCust.store,
                                        ['DiscID', 'DiscSeq', 'CustID', 'BranchID'],
                                        [discId, discSeq, allNodes[i].data.RecID, allNodes[i].data.BranchID]);
                                    if (!record) {
                                        App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CustID: allNodes[i].data.RecID,
                                            BranchID: allNodes[i].data.BranchID,
                                            TerritoryName: allNodes[i].data.Territory
                                        }));
                                        idx++;
                                    }
                                }
                            }
                            App.treePanelCustomer.clearChecked();
                            App.grdDiscCust.store.resumeEvents();
                            App.grdDiscCust.view.refresh();
                            App.grdDiscCust.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdDiscCust.store.data.length > 0 ? App.grdDiscCust.store.data.length - 1 : 0;
                                App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    CustID: '',
                                    BranchID: '',
                                    TerritoryName: ''
                                }));
                            }
                            //App.frmMain.body.unmask();
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

        btnAddCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelCustomer.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.grdDiscCust.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID', 'BranchID'], ['', '']);
                            if (invtBlank) {
                                App.grdDiscCust.store.remove(invtBlank);
                            }

                            var idx = App.grdDiscCust.store.data.length > 0 ? App.grdDiscCust.store.data.length - 1 : 0;
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "CustID") {
                                    var record = HQ.store.findInStore(App.grdDiscCust.store,
                                        ['DiscID', 'DiscSeq', 'CustID', 'BranchID'],
                                        [discId, discSeq, node.attributes.RecID, node.attributes.BranchID]);
                                    if (!record) {
                                        App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CustID: node.attributes.RecID,
                                            BranchID: node.attributes.BranchID,
                                            TerritoryName: node.attributes.Territory
                                        }));
                                    }
                                }
                            });
                            App.treePanelCustomer.clearChecked();
                            App.grdDiscCust.store.resumeEvents();
                            App.grdDiscCust.view.refresh();
                            App.grdDiscCust.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID', 'BranchID'], ['', '']);
                            if (!invtBlank) {
                                var idx = App.grdDiscCust.store.data.length > 0 ? App.grdDiscCust.store.data.length - 1 : 0;
                                App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    CustID: '',
                                    BranchID: '',
                                    TerritoryName: ''
                                }));
                            }
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

        btnDelCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        Main.Event.menuClick('delete');
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

        btnDelAllCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllCust');
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
            if (ctr.id == 'cboDiscSeq') {
                App.cboDiscSeq.store.clearFilter();
            }
            ctr.clearValue();
        },

        slmDiscBreak_selectChange: function (grid, selected, eOpts) {
            var store = App.grdFreeItem.store;
            var keys = store.HQFieldKeys ? store.HQFieldKeys : "";
            var discID = App.cboDiscID.getValue();
            var discSeq = App.cboDiscSeq.getValue();

            store.clearFilter();
            if (selected.length > 0) {
                var objFree = HQ.store.findRecord(store, ['DiscID', 'DiscSeq', 'LineRef', 'FreeItemID'], [discID, discSeq, selected[0].data.LineRef, '']);
                if (objFree) {
                    store.remove(objFree);
                }
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
                        } else if (e.field == "DiscAmt") {
                            if (App.grdFreeItem.store.getCount() > 1 
                                && App.grdFreeItem.store.data.items[0].data.LineRef == e.record.data.LineRef 
                                && !Ext.isEmpty(App.grdFreeItem.store.data.items[0].data.FreeItemID)) {
                                //console.log(App.grdFreeItem.store.data.items[0].data.FreeItemID);
                                return false;
                            } else {
                                var allFreeItem = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data; // 
                                var hasFreeItem = false;
                                if (!HQ.addSameKind) {
                                    for (var i = 0; i < allFreeItem.length; i++) {
                                        if (allFreeItem.items[i].data.LineRef == e.record.data.LineRef && !Ext.isEmpty(allFreeItem.items[i].data.FreeItemID)) {
                                            hasFreeItem = true;
                                            break;
                                        }
                                    }
                                } else {
                                    for (var i = 0; i < allFreeItem.length; i++) {
                                        if (!Ext.isEmpty(allFreeItem.items[i].data.FreeItemID)) {
                                            hasFreeItem = true;
                                            break;
                                        }
                                    }
                                }
                                if (hasFreeItem) {
                                    return false;
                                }
                            }                            
                        }  
                        //else if (e.rowIdx > 0 && e.store.getAt(e.rowIdx - 1).data.DiscAmt == 0 && e.field == "DiscAmt") {

                        //    //return false;
                        //}
                        
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
                        // Dòng đã KM tiền thì ko tặng hàng
                        var selRec = App.slmDiscBreak.getSelection();
                        if (selRec) {
                            if (selRec[0].data.DiscAmt) {
                                return false;
                            }
                        }
                        else {
                            return false;
                        }
                        // Cùng 1 DiscSeq có thể KM Tiền ở mức này và KM tặng hàng ở mức khác
                        if (HQ.addSameKind) {
                            var isAllowInsert = true;
                            for (var i = 0; i < App.grdDiscBreak.store.data.length; i++) {
                                if (App.grdDiscBreak.store.data.items[i].data.DiscAmt) {
                                    isAllowInsert = false;
                                    break;
                                }
                            }
                            if (!isAllowInsert) {
                                return false;
                            }
                        }
                        if (e.store.storeId == 'stoFreeItem') {
                            App.cboGUnitDescr.store.reload();
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
            if (Ext.isEmpty(e.record.data.LineRef)) {
                e.record.set('LineRef', HQ.store.lastLineRef(e.store));
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
                HQ.store.insertRecord(e.store, (keys.length ? keys : totalKeys), newRec, false);

                if (!App.cboDiscClass.readOnly) {
                    App.cboDiscClass.setReadOnly(true);
                }
            }

        },

        grdFreeItem_edit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            var selRecs = App.grdDiscBreak.selModel.selected;
            if (Ext.isEmpty(e.record.data.LineRef) && selRecs.length > 0) {
                e.record.set('LineRef', selRecs.items[0].data.LineRef);
            }
            if (keys.indexOf(e.field) != -1) {
                var recordValues = e.store.getRecordsValues();
                if (e.value != ''
                    && Main.Process.isAllValidKey(recordValues, keys)) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();

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

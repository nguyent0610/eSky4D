var _ka = "KA";
var _rs = "RS";
var _in = "IN";
var _pc = "PC";

var Process = {
    saveData: function () {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            url: 'OM23400/SaveData',
            waitMsg: HQ.common.getLang('Submiting') + "...",
            timeout: 1800000,
            params: {
                lstBonus: Ext.encode(App.stoBonus.getRecordsValues()),

                lstBonusRSChange: HQ.store.getData(App.grdBonusRS.store),
                lstProductChange: HQ.store.getData(App.grdProduct.store),

                lstMonthChange: HQ.store.getData(App.grdMonth.store),
                lstQuarterChange: HQ.store.getData(App.grdQuarter.store),
                lstYearChange: HQ.store.getData(App.grdYear.store),
                isNew: HQ.isNew
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                HQ.isChange = false;
                App.cboBonusID.store.load(function (records, operation, success) {
                    App.stoBonus.reload();
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
    },

    deleteBonus: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM23400/DeleteBonus',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    bonusID: App.cboBonusID.getValue()//,
                    //isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboBonusID.store.load();
                    App.cboBonusID.clearValue();
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

    deleteBonusRS: function (item) {
        if (item == "yes"){
            App.grdBonusRS.deleteSelected();
        }
    },

    deleteProduct: function (item) {
        if (item == "yes"){
            App.grdProduct.deleteSelected();
        }
    },

    deleteMonth: function (item) {
        if (item == "yes") {
            App.grdMonth.deleteSelected();
        }
    },

    deleteQuarter: function (item) {
        if (item == "yes") {
            App.grdQuarter.deleteSelected();
        }
    },

    deleteYear: function (item) {
        if (item == "yes") {
            App.grdYear.deleteSelected();
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

    focusOnInvalidField: function(item){
        if (item == "ok") {
            App.frmMain.getForm().getFields().each(function (field) {
                if (!field.isValid()) {
                    field.focus();
                    return false;
                }
            });
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

    renderProductDescr: function (value, metaData, rec, rowIndex, colIndex, store) {
        var record = App.cboColProductID.store.findRecord("Code", rec.data.ProductID);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },

    lastNbr: function (store) {
        var num = 0;
        for (var j = 0; j < store.data.length; j++) {
            var item = store.data.items[j];

            if (!Ext.isEmpty(item.data.LevelNbr) && parseInt(item.data.LevelNbr) > num) {
                num = parseInt(item.data.LevelNbr);
            }
        };
        num++;
        return num.toString();
    },

    monthDiff: function (dateFrom, dateTo) {
        return (dateTo.getMonth() - dateFrom.getMonth() +
            (12 * (dateTo.getFullYear() - dateFrom.getFullYear())));
    },

    cloneMonthToQuarter: function (item) {
        if (item == "yes") {
            if (App.grdQuarter.store.getCount()) {
                App.grdQuarter.store.removeAll();
            }

            var keys = App.grdQuarter.store.HQFieldKeys ? App.grdQuarter.store.HQFieldKeys : [];
            var monthCount = 3;
            App.grdMonth.store.each(function (record) {
                if (record.data.ClassID) {
                    var rec = Ext.create(App.grdQuarter.store.model.modelName, record.data);
                    rec.set("SlsAmt", record.data.SlsAmt * monthCount);
                    HQ.store.insertRecord(App.grdQuarter.store, keys, rec);
                }
            });

            var newRec = Ext.create(App.grdQuarter.store.model.modelName, {
                LevelNbr: Process.lastNbr(App.grdQuarter.store)
            });
            HQ.store.insertRecord(App.grdQuarter.store, keys, newRec);
        }
    },

    cloneMonthToYear: function (item) {
        if (item == "yes") {
            if (App.grdYear.store.getCount()) {
                App.grdYear.store.removeAll();
            }

            var keys = App.grdYear.store.HQFieldKeys ? App.grdYear.store.HQFieldKeys : [];

            var monthCount = Process.monthDiff(App.dtpFromDate.value, App.dtpToDate.value);
            App.grdMonth.store.each(function (record) {
                if (record.data.ClassID) {
                    var rec = Ext.create(App.grdYear.store.model.modelName, record.data);
                    rec.set("SlsAmt", record.data.SlsAmt * monthCount);
                    HQ.store.insertRecord(App.grdYear.store, keys, rec);
                }
            });

            var newRec = Ext.create(App.grdYear.store.model.modelName, {
                LevelNbr: Process.lastNbr(App.grdYear.store)
            });
            HQ.store.insertRecord(App.grdYear.store, keys, newRec);
        }
    },
};

var Store = {
    stoBonus_load: function (sto, records, successful, eOpts) {
        HQ.isNew = false;
        if (sto.getCount() == 0) {
            var newPosm = Ext.create("App.mdlBonus", {
                FromDate: HQ.dateNow,
                ToDate: HQ.dateNow
            });
            sto.insert(0, newPosm);
            HQ.isNew = true;
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);

        App.cboChannel.setReadOnly(frmRecord.data.tstamp);
        App.cboRSApplyType.setReadOnly(frmRecord.data.tstamp);

        // load RS
        App.grdBonusRS.store.reload();
        App.grdProduct.store.reload();

        // load KA
        App.grdMonth.store.reload();
        App.grdQuarter.store.reload();
        App.grdYear.store.reload();

        Event.Form.frmMain_fieldChange();
    },

    stoBonusRS_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : [];
            if (successful) {
                if (keys.indexOf('LevelNbr') > -1) {
                    var rec = Ext.create(sto.model.modelName, {
                        LevelNbr: Process.lastNbr(sto)
                    });
                    HQ.store.insertRecord(sto, keys, rec);
                }
                else {
                    HQ.store.insertBlank(sto, keys);
                }
            }
        }
    }
};

var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            HQ.common.setRequire(frm);
            App.stoBonus.reload();
        },

        frmMain_fieldChange: function () {
            if (App.stoBonus.getCount() > 0) {
                App.frmMain.updateRecord();
                if (!HQ.store.isChange(App.stoBonus)) {
                    if (App.cboChannel.value == _rs) {
                        if (!HQ.store.isChange(App.grdBonusRS.store)) {
                            HQ.isChange = HQ.store.isChange(App.grdProduct.store);
                        }
                        else {
                            HQ.isChange = true;
                        }
                    }
                    else if (App.cboChannel.value == _ka) {
                        if (!HQ.store.isChange(App.grdMonth.store)) {
                            if (!HQ.store.isChange(App.grdQuarter.store)) {
                                HQ.isChange = HQ.store.isChange(App.grdYear.store);
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
                        HQ.isChange = false;
                    }
                }
                else {
                    HQ.isChange = true;
                }

                HQ.common.changeData(HQ.isChange, 'OM23400');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                App.cboBonusID.setReadOnly(HQ.isChange && App.cboBonusID.getValue());

                var frmRecord = App.frmMain.getRecord();
                if (!frmRecord.data.tstamp) {
                    if (HQ.isChange && (App.grdBonusRS.store.getCount()
                                        || App.grdProduct.store.getCount()
                                        || App.grdMonth.store.getCount()
                                        || App.grdYear.store.getCount())) {
                        App.cboChannel.setReadOnly(true);
                        App.cboRSApplyType.setReadOnly(true);
                    }
                    else {
                        App.cboChannel.setReadOnly(false);
                        App.cboRSApplyType.setReadOnly(false);
                    }
                }
            }
        },

        btnImport_click: function (btn, e) {
            Ext.Msg.alert("Import", "Coming soon!");
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        cboBonusID_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoBonus.reload();
        },

        cboChannel_change: function (cbo, newValue, oldValue, eOpts) {
            if (cbo.value) {
                if (cbo.value == _rs) {
                    App.cboRSApplyType.show();
                    App.cboRSApplyType.allowBlank = false;

                    App.tabBonusRS.show();
                    App.tabBonusKA.hide();
                }
                else if (cbo.value == _ka) {
                    App.cboRSApplyType.hide();
                    App.cboRSApplyType.allowBlank = true;

                    App.tabBonusRS.hide();
                    App.tabBonusKA.show();
                }
            }
            else {
                App.cboRSApplyType.hide();
                App.cboRSApplyType.allowBlank = true;

                App.tabBonusRS.hide();
                App.tabBonusKA.hide();
            }
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(dtp.value);
        },

        cboRSApplyType_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboColProductID.store.reload();

            var column = App.grdProduct.down('[dataIndex=ProductID]');
            if (column) {
                if (newValue == _pc) {
                    column.setText(HQ.common.getLang("ProductClass"));
                }
                else {
                    column.setText(HQ.common.getLang("ProductID"));
                }
            }
        },

        btnQuarterClone_click: function (btn, eOpts) {
            if (App.frmMain.isValid()) {
                if (App.grdMonth.store.getCount()) {
                    if (App.grdQuarter.store.getCount()) {
                        HQ.message.show(87, '', 'Process.cloneMonthToQuarter');
                    }
                    else {
                        Process.cloneMonthToQuarter("yes");
                    }
                }
            }
            else {
                Process.showFieldInvalid(App.frmMain);
            }
        },

        btnYearClone_click: function (btn, eOpts) {
            if (App.frmMain.isValid()) {
                if (App.grdMonth.store.getCount()) {
                    if (App.grdYear.store.getCount()) {
                        HQ.message.show(87, '', 'Process.cloneMonthToYear');
                    }
                    else {
                        Process.cloneMonthToYear("yes");
                    }
                }
            }
            else {
                Process.showFieldInvalid(App.frmMain);
            }
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'bonus') {
                        HQ.combo.first(App.cboBonusID, HQ.isChange);
                    }
                    else if (HQ.focus == 'BonusRS') {
                        HQ.grid.first(App.grdBonusRS);
                    }
                    else if (HQ.focus == 'Product') {
                        HQ.grid.first(App.grdProduct);
                    }
                    else if (HQ.focus == 'month') {
                        HQ.grid.first(App.grdMonth);
                    }
                    else if (HQ.focus == 'quarter') {
                        HQ.grid.first(App.grdQuarter);
                    }
                    else if (HQ.focus == 'year') {
                        HQ.grid.first(App.grdYear);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'bonus') {
                        HQ.combo.next(App.cboBonusID, HQ.isChange);
                    }
                    else if (HQ.focus == 'BonusRS') {
                        HQ.grid.next(App.grdBonusRS);
                    }
                    else if (HQ.focus == 'Product') {
                        HQ.grid.next(App.grdProduct);
                    }
                    else if (HQ.focus == 'month') {
                        HQ.grid.next(App.grdMonth);
                    }
                    else if (HQ.focus == 'quarter') {
                        HQ.grid.next(App.grdQuarter);
                    }
                    else if (HQ.focus == 'year') {
                        HQ.grid.next(App.grdYear);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'bonus') {
                        HQ.combo.prev(App.cboBonusID, HQ.isChange);
                    }
                    else if (HQ.focus == 'BonusRS') {
                        HQ.grid.prev(App.grdBonusRS);
                    }
                    else if (HQ.focus == 'Product') {
                        HQ.grid.prev(App.grdProduct);
                    }
                    else if (HQ.focus == 'month') {
                        HQ.grid.prev(App.grdMonth);
                    }
                    else if (HQ.focus == 'quarter') {
                        HQ.grid.prev(App.grdQuarter);
                    }
                    else if (HQ.focus == 'year') {
                        HQ.grid.prev(App.grdYear);
                    }
                    break;
                case "last":
                    if (HQ.focus == 'bonus') {
                        HQ.combo.last(App.cboBonusID, HQ.isChange);
                    }
                    else if (HQ.focus == 'BonusRS') {
                        HQ.grid.last(App.grdBonusRS);
                    }
                    else if (HQ.focus == 'Product') {
                        HQ.grid.last(App.grdProduct);
                    }
                    else if (HQ.focus == 'month') {
                        HQ.grid.last(App.grdMonth);
                    }
                    else if (HQ.focus == 'quarter') {
                        HQ.grid.last(App.grdQuarter);
                    }
                    else if (HQ.focus == 'year') {
                        HQ.grid.last(App.grdYear);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'bonus') {
                            App.cboBonusID.store.load(function (records, operation, success) {
                                App.stoBonus.reload();
                            });
                        }
                        else if (HQ.focus == 'BonusRS') {
                            App.grdBonusRS.store.reload();
                        }
                        else if (HQ.focus == 'Product') {
                            App.grdProduct.store.reload();
                        }
                        else if (HQ.focus == 'month') {
                            App.grdMonth.store.reload();
                        }
                        else if (HQ.focus == 'quarter') {
                            App.grdQuarter.store.reload();
                        }
                        else if (HQ.focus == 'year') {
                            App.grdYear.store.reload();
                        }
                    }
                    break;
                case "new":
                    if (HQ.isInsert) {
                        if (HQ.isChange) {
                            HQ.message.show(150, '', '');
                        }
                        else {
                            App.cboBonusID.clearValue();
                        }
                    }
                    break;
                case "save":
                    if (HQ.isUpdate || HQ.isCreate) {
                        if (App.frmMain.isValid()) {
                            if (App.cboChannel.value == _rs) {
                                var keysBonusRS = App.grdBonusRS.store.HQFieldKeys ? App.grdBonusRS.store.HQFieldKeys : [];
                                var keysProduct = App.grdProduct.store.HQFieldKeys ? App.grdBonusRS.store.HQFieldKeys : [];

                                if (HQ.store.checkRequirePass(App.grdBonusRS.store, keysBonusRS, keysBonusRS,
                                    ["Level", "FromLevel", "ToLevel", "Bonus"])) {
                                    if (HQ.store.checkRequirePass(App.grdProduct.store, keysProduct, keysProduct, 
                                        [App.cboRSApplyType.value == _in ? "InvtID" : (App.cboRSApplyType.value == _pc ? "ProductClass" : "ProductID")])) {
                                        Process.saveData();
                                    }
                                }
                            }
                            else if (App.cboChannel.value == _ka) {
                                //var keysMonth = App.grdMonth.store.HQFieldKeys ? App.grdMonth.store.HQFieldKeys : "";
                                //var keysQuarter = App.grdQuarter.store.HQFieldKeys ? App.grdQuarter.store.HQFieldKeys : "";
                                //var keysYear = App.grdYear.store.HQFieldKeys ? App.grdYear.store.HQFieldKeys : "";

                                //if (HQ.store.checkRequirePass(App.grdMonth.store, keysMonth, keysMonth,
                                //    ["Level", "ProductClass", "AmtTot", "FromLevel", "ToLevel", "Bonus"])) {
                                //    if (HQ.store.checkRequirePass(App.grdQuarter.store, keysQuarter, keysQuarter,
                                //        ["Level", "ProductClass", "AmtTot", "FromLevel", "ToLevel", "Bonus"])) {
                                //        if (HQ.store.checkRequirePass(App.grdYear.store, keysYear, keysYear,
                                //            ["Level", "ProductClass", "AmtTot", "FromLevel", "ToLevel", "Bonus"])) {
                                //            Process.saveData();
                                //        }
                                //    }
                                //}
                                Process.saveData();
                            }
                        }
                        else {
                            Process.showFieldInvalid(App.frmMain);
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
                case "delete":
                    if (HQ.isDelete) {
                        if (HQ.focus == 'bonus') {
                            if (App.cboBonusID.getValue()) {
                                HQ.message.show(11, '', 'Process.deleteBonus');
                            }
                        }
                        else if (HQ.focus == 'BonusRS') {
                            if (App.cboBonusID.getValue() && App.slmBonusRS.getCount()) {
                                HQ.message.show(2015020806,
                                    HQ.common.getLang('Level') + " " + App.slmBonusRS.selected.items[0].data.LevelNbr,
                                    'Process.deleteBonusRS');
                            }
                        }
                        else if (HQ.focus == 'Product') {
                            if (App.cboBonusID.getValue() && App.slmProduct.getCount()) {
                                HQ.message.show(2015020806,
                                    HQ.common.getLang('ProductID') + " " + App.slmProduct.selected.items[0].data.ProductID,
                                    'Process.deleteProduct');
                            }
                        }
                        else if (HQ.focus == 'month') {
                            if (App.cboBonusID.getValue() && App.slmMonth.getCount()) {
                                HQ.message.show(2015020806,
                                    HQ.common.getLang('ProductClass') + " " + App.slmMonth.selected.items[0].data.ClassID,
                                    'Process.deleteMonth');
                            }
                        }
                        else if (HQ.focus == 'quarter') {
                            if (App.cboBonusID.getValue() && App.slmQuarter.getCount()) {
                                HQ.message.show(2015020806,
                                    HQ.common.getLang('ProductClass') + " " + App.slmQuater.selected.items[0].data.ClassID,
                                    'Process.deleteQuarter');
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
            }
        }
    },

    Grid: {
        grdBonusRS_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : [];

                    if (keys.indexOf(e.field) != -1) {
                        if (e.record.data.tstamp) {
                            if (keys.indexOf('LevelNbr') > -1 && keys.length == 1) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        }
                    }

                    //if (e.field == "ProductID") {
                    //    App.cboColProductID.store.reload();
                    //}
                    if (keys.indexOf('LevelNbr') > -1 && keys.length == 1) {
                        return true;
                    }
                    else {
                        return HQ.grid.checkInput(e, keys);
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

        grd_reject: function (col, record) {
            var grd = col.up('grid');
            if (!record.data.tstamp && !record.fields.containsKey('LevelNbr')) {
                grd.getStore().remove(record, grd);
                grd.getView().focusRow(grd.getStore().getCount() - 1);
                grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            } else {
                record.reject();
            }
            //HQ.grid.checkReject(record, grd);
            Event.Form.frmMain_fieldChange();
        },

        grdBonusRS_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : [];

            if (keys.indexOf('LevelNbr') > -1 && keys.length == 1) {
                keys = ['LevelNbr', e.field];
            }

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    //if (keys.indexOf('LevelNbr') > -1) {
                    if(e.record.fields.containsKey('LevelNbr')){
                        var BonusRSRec = Ext.create(e.store.model.modelName, {
                            LevelNbr: Process.lastNbr(e.store)
                        });
                        HQ.store.insertRecord(e.store, keys, BonusRSRec);
                    }
                    else {
                        HQ.store.insertBlank(e.store, keys);
                    }
                }
            }

            //Event.Form.frmMain_fieldChange();
        },

        grdBonusRS_validateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : [];

            if (keys.indexOf(e.field) != -1) {
                var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                if (isNaN(e.value)) {
                    if (e.value && !e.value.toString().match(regex)) {
                        HQ.message.show(20140811, e.column.text);
                        return false;
                    }
                }
                if (keys.indexOf('LevelNbr') > -1 && keys.length == 1 && !e.value) {
                    HQ.message.show(1235);
                    return false;
                }
                else if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                    HQ.message.show(1112, e.value);
                    return false;
                }

            }
        }
    },
};
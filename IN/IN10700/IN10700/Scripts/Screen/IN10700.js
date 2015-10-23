var _LOT = "LOT";
var _allowUpdate = false;

var Process = {
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

    renderDescr: function (value, metaData, record, rowIndex, colIndex, store) {
        var returnValue = value;

        if (metaData.column.dataIndex == "ReasonID") {
            var dataRec = HQ.store.findInStore(App.cboReason.store, ["Code"], [record.data.ReasonID]);
            if (dataRec) {
                returnValue = dataRec.Descr;
            }
        }
        else if (metaData.column.dataIndex == "Descr" && !record.data.Descr) {
            var dataRec = HQ.store.findInStore(App.cboInvtID.store, ["InvtID"], [record.data.InvtID]);
            if (dataRec) {
                returnValue = dataRec.Descr;
            }
        }

        return returnValue;
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

    checkInput: function (row, keys) {
        if (keys.indexOf(row.field) == -1) {

            for (var jkey = 0; jkey < keys.length; jkey++) {
                if (row.record.data[keys[jkey]] == "") {
                    return false;
                }
            }
        }
        if (keys.indexOf(row.field) != -1) {
            for (var jkey = 0; jkey < keys.length; jkey++) {
                if (!row.record.data[keys[jkey]]) return true;
            }
            return false;
        }
        return true;
    },

    checkDuplicate: function (grd, row, keys) {
        var found = false;
        var store = grd.getStore();
        if (keys == undefined) keys = row.record.idProperty.split(',');
        if (store.data) {
            for (var i = 0; i < store.data.items.length; i++) {
                var record = store.data.items[i];
                var data = '';
                var rowdata = '';
                for (var jkey = 0; jkey < keys.length; jkey++) {
                    if (record.data[keys[jkey]]) {
                        data += record.data[keys[jkey]].toString().toLowerCase() + ',';
                        if (row.field == keys[jkey])
                            rowdata += (row.value == null ? "" : row.value.toString().toLowerCase()) + ',';
                        else
                            rowdata += (!row.record.data[keys[jkey]]? '' : row.record.data[keys[jkey]].toString().toLowerCase()) + ',';
                    }
                }
                if (found = (data == rowdata && record.id != row.record.id) ? true : false) {
                    break;
                };
            }
        }
        else {
            for (var i = 0; i < store.allData.items.length; i++) {
                var record = store.allData.items[i];
                var data = '';
                var rowdata = '';
                for (var jkey = 0; jkey < keys.length; jkey++) {
                    if (record.data[keys[jkey]]) {
                        data += record.data[keys[jkey]].toString().toLowerCase() + ',';
                        if (row.field == keys[jkey])
                            rowdata += (row.value == null ? "" : row.value.toString().toLowerCase()) + ',';
                        else
                            rowdata += (!row.record.data[keys[jkey]] ? '' : row.record.data[keys[jkey]].toString().toLowerCase()) + ',';
                    }
                }
                if (found = (data == rowdata && record.id != row.record.id) ? true : false) {
                    break;
                };
            }
        }
        return found;
    },

    checkStkOutNbrFromPDA: function (stkOutNbr) {
        if (stkOutNbr && !isNaN(stkOutNbr)) {
            return true;
        }
        return false;
    },

    saveData: function () {
        if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
            if (HQ.form.checkRequirePass(App.frmMain)) {
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("SavingData"),
                    url: 'IN10700/SaveData',
                    timeout: 10000000,
                    params: {
                        lstStockOutlet: Ext.encode(App.stoStockOutlet.getRecordsValues()),
                        lstStockOutletDetChange: HQ.store.getData(App.grdStockOutletDet.store),
                        isNew: HQ.isNew
                    },
                    success: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        else {
                            HQ.message.show(201405071);
                        }
                        App.stoStockOutlet.reload();
                        //App.grdStockOutletDet.store.reload();
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
            HQ.message.show(4, '', '');
        }
    },

    deleteSelectedInGrid: function (item) {
        if (item == 'yes') {
            App.grdStockOutletDet.deleteSelected();
        }
    }
};

var Store = {
    stoStockOutlet_load: function (sto, records, successful, eOpts) {
        HQ.isNew = false;
        if (!sto.getCount()) {
            var newRec = Ext.create(sto.model.modelName, {
                BranchID: App.cboBranchID.getValue(),
                SlsPerID: App.cboSlsperID.getValue(),
                CustID: App.cboCustID.getValue(),
                StockType: App.cboStockType.getValue(),
                StkOutDate: App.dtpStkOutDate.getValue(),
                StkOutNbr: ''
            });
            sto.insert(0, newRec);
            HQ.isNew = true;
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);

        App.stoCheckForUpdate.reload();
        //App.grdStockOutletDet.store.reload();
    },

    stoStockOutletDet_beforeLoad: function () {
        var type = App.cboStockType.getValue();
        if (type == _LOT) {
            this.HQFieldKeys = ['ExpDate','InvtID'];
        }
        else {//"LOT"
            this.HQFieldKeys = ['InvtID'];
        }
    },

    stoStockOutletDet_load: function (sto, records, successful, eOpts) {
        if (!Process.checkStkOutNbrFromPDA(App.txtStkOutNbr.value) && _allowUpdate) {
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";
            var newData = {
                BranchID: App.cboBranchID.getValue(),
                SlsperID: App.cboSlsperID.getValue()//,
                //ExpDate: HQ.dateNow
            };

            var newRec = Ext.create(sto.model.modelName, newData);
            HQ.store.insertRecord(sto, keys, newRec, false);
        }
        Event.Form.frmMain_fieldChange();
    },

    stoCheckForUpdate_load: function (sto, records, successful, eOpts) {
        var chkRec = sto.getAt(0);
        if (chkRec) {
            _allowUpdate = chkRec.data.Result;
        }
        else {
            _allowUpdate = false;
        }
        App.grdStockOutletDet.store.reload();
    }
};

var Event = {
    Form: {
        frmMain_boxReady: function () {
            HQ.common.setRequire(App.frmMain);
            App.cboBranchID.store.load(function (records, operation, success) {
                App.cboBranchID.setValue(HQ.cpnyID);
            });
        },

        frmMain_fieldChange: function () {
            HQ.isChange = HQ.store.isChange(App.grdStockOutletDet.store);

            HQ.common.changeData(HQ.isChange, 'IN10700');
            App.cboBranchID.setReadOnly(HQ.isChange);
            App.cboSlsperID.setReadOnly(HQ.isChange);
            App.cboCustID.setReadOnly(HQ.isChange);
            App.cboStockType.setReadOnly(HQ.isChange);
            App.dtpStkOutDate.setReadOnly(HQ.isChange);
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        cboBranchID_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboSlsperID.store.reload();
            App.cboCustID.store.reload();
            App.cboInvtID.store.reload();

            if (App.frmMain.isValid()) {
                App.stoStockOutlet.reload();
            }
        },

        cboSlsperID_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboCustID.store.reload();

            if (App.frmMain.isValid()) {
                App.stoStockOutlet.reload();
            }
        },

        cboCustID_change: function (cbo, newValue, oldValue, eOpts) {
            if (App.frmMain.isValid()) {
                App.stoStockOutlet.reload();
            }
        },

        cboStockType_change: function (cbo, newValue, oldValue, eOpts) {
            if (newValue == _LOT) {
                Process.showColumns(App.grdStockOutletDet, ["ExpDate"], true);
            }
            else {
                Process.showColumns(App.grdStockOutletDet, ["ExpDate"], false);
            }
            if (App.frmMain.isValid()) {
                App.stoStockOutlet.reload();
            }
        },

        dtpStkOutDate_change: function (dtp, newValue, oldValue, eOpts) {
            if (App.frmMain.isValid()) {
                App.stoStockOutlet.reload();
            }
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    HQ.grid.first(App.grdStockOutletDet);
                    break;
                case "next":
                    HQ.grid.next(App.grdStockOutletDet);
                    break;
                case "prev":
                    HQ.grid.prev(App.grdStockOutletDet);
                    break;
                case "last":
                    HQ.grid.last(App.grdStockOutletDet);
                    break;
                case "refresh":
                    App.grdStockOutletDet.store.reload();
                    break;
                case "save":
                    Process.saveData();
                    break;
                case "delete":
                    if (HQ.isUpdate) {
                        if (_allowUpdate) {
                            HQ.message.show(11, '', 'Process.deleteSelectedInGrid');
                        }
                        else {
                            HQ.message.show(2015012701, '', '');
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
        grd_reject: function (col, record) {
            var grd = col.up('grid');
            if (!record.data.tstamp) {
                grd.getStore().remove(record, grd);
                grd.getView().focusRow(grd.getStore().getCount() - 1);
                grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            } else {
                record.reject();
            }
        },

        grdStockOutletDet_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                    if (e.record.data.tstamp) {
                        if (e.field == "StkQty" || e.field == "ReasonID") {
                            if (_allowUpdate) {
                                return true;
                            }
                            else {
                                HQ.message.show(2015012701, '', '');
                                return false;
                            }
                        }
                        else {
                            return false;
                        }
                    }
                    return Process.checkInput(e, keys);
                }
                else {
                    return false;
                }
            }
            else {
                return false;
            }
        },

        grdStockOutletDet_edit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            if (!Process.checkStkOutNbrFromPDA(App.txtStkOutNbr.value)) {
                if (keys.indexOf(e.field) != -1) {
                    if (e.value != ''
                        && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                        && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                        var newData = {
                            BranchID: App.cboBranchID.getValue(),
                            SlsperID: App.cboSlsperID.getValue()//,
                            //ExpDate: HQ.dateNow
                        };

                        var newRec = Ext.create(e.store.model.modelName, newData);
                        HQ.store.insertRecord(e.store, keys, newRec, false);
                    }
                }
            }
        },

        grdStockOutletDet_validateEdit: function(editor, e){
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                if (e.value && e.column.xtype != "datecolumn" && !e.value.match(regex)) {
                    HQ.message.show(20140811, e.column.text);
                    return false;
                }
                if (Process.checkDuplicate(e.grid, e, keys)) {
                    HQ.message.show(1112, e.value);
                    return false;
                }

            }
        }
    }
};
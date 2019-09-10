var _LOT = "LOT";
var _allowUpdate = false;
var _POSM = 'POSM';
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
                            rowdata += (!row.record.data[keys[jkey]] ? '' : row.record.data[keys[jkey]].toString().toLowerCase()) + ',';
                    }
                }
                if (found = (data == rowdata && record.id != row.record.id && rowdata) ? true : false) {
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
                if (found = (data == rowdata && record.id != row.record.id && rowdata) ? true : false) {
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
                if (HQ.store.checkRequirePass(App.stoStockOutletDet, ["InvtID", "ExpDate"], ["InvtID", "ExpDate"], ["InvtID", "ExpDate"])) {
                    var i = 0;
                    var errorMessage = '';
                    var store = App.stoStockOutletDet;
                    //var allRecords = store.snapshot || store.allData || store.data;
                    //allRecords.each(function (record) {
                    //    i++;
                    //    if (record.data.ClassID == 'POSM')
                    //        if (!record.data.PosmID ) {
                    //            errorMessage += i + ', ';
                    //        }
                    //});
                    if (errorMessage) {
                        HQ.message.show(2016033001, [errorMessage], '', true);
                        return;
                    }
                    App.stoPOSM.clearFilter();
                    App.frmMain.submit({
                        waitMsg: HQ.common.getLang("SavingData"),
                        url: 'IN10700/SaveData',
                        timeout: 10000000,
                        params: {
                            lstStockOutlet: Ext.encode(App.stoStockOutlet.getRecordsValues()),
                            lstStockOutletDetChange: HQ.store.getData(App.grdStockOutletDet.store),
                            lstStockOutletPOSM: HQ.store.getData(App.stoPOSM),
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
        }
        else {
            HQ.message.show(4, '', '');
        }
    },

    deleteSelectedInGrid: function (item) {
        if (item == 'yes') {
            App.stoPOSM.clearFilter();
            var det = App.slmStockOutletDet.selected.items[0].data;
            for (i = App.stoPOSM.data.items.length - 1; i >= 0; i--) {
                if (App.stoPOSM.data.items[i].data.InvtID == det.InvtID) {
                    App.stoPOSM.data.removeAt(i);
                }
            }
            App.grdStockOutletDet.deleteSelected();
        }
    },
    deleteHeader: function (item) {
        if (item == 'yes') {
            if (App.frmMain.isValid()) {
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang('DeletingData'),
                    method: 'POST',
                    url: 'IN10700/DeleteHeader',
                    timeout: 180000,
                    params: {
                        isPOSM: App.cboInvtType.getValue() == _POSM,
                        lstStockOutlet: Ext.encode(App.stoStockOutlet.getRecordsValues())
                    },
                    success: function (msg, data) {
                        if (App.frmMain.isValid()) {
                            App.stoStockOutlet.reload();
                        }
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
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
            this.HQFieldKeys = ['ExpDate', 'InvtID'];
        }
        else {//"LOT"
            this.HQFieldKeys = ['InvtID'];
        }
    },

    stoStockOutletDet_load: function (sto, records, successful, eOpts) {
        //if (!Process.checkStkOutNbrFromPDA(App.txtStkOutNbr.value) && _allowUpdate) {
        var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";
        var newData = {
            BranchID: App.cboBranchID.getValue(),
            SlsperID: App.cboSlsperID.getValue()//,
            //ExpDate: HQ.bussinessDate
        };

        var newRec = Ext.create(sto.model.modelName, newData);
        HQ.store.insertRecord(sto, keys, newRec, false);
        //}
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
        App.stoPOSM.reload();
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
            App.cboInvtType.setReadOnly(HQ.isChange);
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
                Process.showColumns(App.grdPOSM, ["ExpDate"], true);
            }
            else {
                Process.showColumns(App.grdStockOutletDet, ["ExpDate"], false);
                Process.showColumns(App.grdPOSM, ["ExpDate"], false);
            }
            if (App.frmMain.isValid()) {
                App.stoStockOutlet.reload();

            }
        },

        cboInvtType_change: function (cbo, newValue, oldValue, eOpts) {
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
                    App.stoPOSM.reload();

                    break;
                case "new":
                    if (HQ.isUpdate) {
                        if (_allowUpdate) {
                            HQ.grid.insert(App.grdStockOutletDet, App.grdStockOutletDet.store.HQFieldKeys);
                        }
                    }
                    break;
                case "save":
                    if (HQ.isUpdate && _allowUpdate) {
                        Process.saveData();
                    }
                    else {
                        HQ.message.show(2015012701, '', '');
                    }
                    break;
                case "delete":
                    if (HQ.isUpdate) {
                        if (_allowUpdate) {
                            if (HQ.focus == 'grdStockOutletDet') {
                                var rowindex = HQ.grid.indexSelect(App.grdStockOutletDet);
                                if (rowindex != '')
                                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdStockOutletDet), ''], 'Process.deleteSelectedInGrid', true);
                            } else {
                                HQ.message.show(11, '', 'Process.deleteHeader');
                            }
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
        },

        cboInvtID_Expand: function (combo) {
            App.cboInvtID.store.clearFilter();
            var store = App.cboInvtID.store;
            // Filter data -- Main Site
            store.filterBy(function (record) {
                if (record) {
                    if (App.cboInvtType.getValue() == _POSM) {
                        if (record.data['ClassID'].toString() == _POSM) {
                            return record;
                        }
                    }
                    else {
                        if (record.data['ClassID'].toString() != _POSM) {
                            return record;
                        }
                    }
                }
            });
        },
        cboInvtID_Collapse: function (combo) {
            App.cboInvtID.store.clearFilter();
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
                if (_allowUpdate) {
                    if (HQ.form.checkRequirePass(App.frmMain)) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
                        if (e.record.data.tstamp) {
                            if (e.field == "StkQty" || e.field == "ReasonID" || e.field == "PosmID") {
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
                    HQ.message.show(2015012701, '', '');
                    return false;
                }
            }
            else {
                return false;
            }
        },

        grdStockOutletDet_edit: function (editor, e) {
            if (e.field == 'InvtID') {
                var objInvt = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [e.record.data.InvtID]);
                if (objInvt) {
                    e.record.set('ClassID', objInvt.ClassID);
                    if ((!e.record.data.ExpDate && objInvt.ClassID == "POSM") || (App.cboStockType.getValue() != 'LOT'))
                        e.record.set('ExpDate', new Date(1900, 0, 1));
                }

            }
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            HQ.grid.checkInsertKey(App.grdStockOutletDet, e, keys);

            //if (!Process.checkStkOutNbrFromPDA(App.txtStkOutNbr.value)) {
            //if (keys.indexOf(e.field) != -1) {
            //    if (e.value != ''
            //        && Process.isAllValidKey(e.store.getChangedData().Created, keys)
            //        && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
            //        var newData = {
            //            BranchID: App.cboBranchID.getValue(),
            //            SlsperID: App.cboSlsperID.getValue()//,
            //            //ExpDate: HQ.bussinessDate
            //        };

            //        var newRec = Ext.create(e.store.model.modelName, newData);
            //        HQ.store.insertRecord(e.store, keys, newRec, false);
            //    }
            //}
            //}
        },

        grdStockOutletDet_validateEdit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            var objInvt = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], e.field == "InvtID" ? [e.value] : [e.record.data.InvtID]);
            if (objInvt && objInvt.ClassID == "POSM") {
                keys = ["InvtID"];
            }
            if (keys.indexOf(e.field) != -1) {
                var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                if (e.value && e.column.xtype != "datecolumn" && !e.value.match(regex)) {
                    HQ.message.show(20140811, e.column.text);
                    return false;
                }
                if (Process.checkDuplicate(e.grid, e, keys)) {
                    HQ.message.show(1112, e.field == "ExpDate" ? e.value.getFromFormat(HQ.formatDate) : e.value);
                    return false;
                }
            }
        },

        showPOSM: function (record) {
            App.winPOSM.invt = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [record.data.InvtID]);
            if (!Ext.isEmpty(record.data.ClassID) && !Ext.isEmpty(record.data.InvtID) && App.winPOSM.invt.ClassID == 'POSM') {
                PopupwinPOSM.showPOSM(record);
            }
        },
        txtQty_Change: function (sender) {
            var record = App.slmStockOutletDet.selected.items[0];
            Event.Grid.showPOSM(record);
        }
    }
};

var PopupwinPOSM = {
    showPOSM: function (record) {

        var lock = !((App.txtStkOutNbr.value && HQ.isUpdate) || (!App.txtStkOutNbr.value && HQ.isInsert));
        App.grdPOSM.isLock = lock;
        App.stoPOSM.clearFilter();
        App.stoPOSM.filter('InvtID', record.data.InvtID);
        App.winPOSM.record = record;
        App.grdPOSM.view.refresh();

        App.winPOSM.setTitle(record.data.InvtID);

        App.cboPosmID.getStore().reload();

        PopupwinPOSM.addNewPOSM(record.data);

        App.winPOSM.show();
    },
    btnPOSMOK_Click: function () {
        var qty = 0;
        if (PopupwinPOSM.checkRequirePass(App.stoPOSM, ["PosmID", "ExpDate"], ["PosmID", "ExpDate"], ["PosmID", "ExpDate"])) {
            setTimeout(function () {
                App.stoPOSM.data.each(function (item) {
                    qty += item.data.StkQty;
                    App.winPOSM.record.set("StkQty", qty);
                    App.winPOSM.hide();
                });
            }, 300);
        }
    },
    btnPOSMDel_Click: function () {
        if (App.smlPOSM.selected.items.length != 0) {
            if (!Ext.isEmpty(App.smlPOSM.selected.items[0].data.PosmID)) {
                HQ.message.show(2015020806, [App.smlPOSM.selected.items[0].data.PosmID], 'PopupwinPOSM.deletePOSM', true);
            }
        }
    },
    grdPOSM_BeforeEdit: function (item, e) {
        var obj = e.record.data;

        if (App.grdPOSM.isLock) {
            return false;
        }

        //if (e.field == 'PosmID')
        return HQ.grid.checkBeforeEdit(e, ["PosmID", "ExpDate"]);
    },
    grdPOSM_SelectionChange: function (item, selected) {

    },
    grdPOSM_Edit: function (item, e) {
        if (e.field == "PosmID" && App.cboStockType.getValue() != 'LOT') {
            e.record.set('ExpDate', new Date(1900, 0, 1));
        }
        var record = App.winPOSM.record.data;
        PopupwinPOSM.addNewPOSM(record);

    },
    grdPOSM_ValidateEdit: function (item, e) {
        if (App.cboStockType.getValue() == 'LOT') {
            if (Process.checkDuplicate(App.grdPOSM, e, ["PosmID", "ExpDate"])) {
                HQ.message.show(1112, e.field == "ExpDate" ? e.value.getFromFormat(HQ.formatDate) : e.value);
                return false;
            }
        } else {
            if (Process.checkDuplicate(App.grdPOSM, e, ["PosmID"])) {
                HQ.message.show(1112, e.field == "ExpDate" ? e.value.getFromFormat(HQ.formatDate) : e.value);
                return false;
            }
        }

    },
    deletePOSM: function (item) {
        if (item == 'yes') {
            App.grdPOSM.deleteSelected();
        }
    },
    addNewPOSM: function (record, PosmID) {
        var newRow = Ext.create('App.mdlStockOutletPOSM');
        newRow.data.PosmID = !Ext.isEmpty(PosmID) ? PosmID : '';
        newRow.data.InvtID = record.InvtID;
        newRow.data.InvtID = record.InvtID;
        if (App.cboStockType.getValue() == 'LOT')
            HQ.store.insertRecord(App.stoPOSM, ["InvtID", "PosmID", "ExpDate"], newRow, false);
        else
            HQ.store.insertRecord(App.stoPOSM, ["InvtID", "PosmID"], newRow, false);
    },
    checkRequirePass: function (store, keys, fieldsCheck, fieldsLang) {
        items = store.data.items;
        if (items != undefined) {
            for (var i = 0; i < items.length; i++) {
                for (var jkey = 0; jkey < keys.length; jkey++) {
                    if (items[i].data[keys[jkey]]) {
                        for (var k = 0; k < fieldsCheck.length; k++) {
                            if (HQ.util.passNull(items[i].data[fieldsCheck[k]]).toString().trim() == "") {
                                HQ.message.show(15, HQ.common.getLang(fieldsLang == undefined ? fieldsCheck[k] : fieldsLang[k]));
                                return false;
                            }
                        }
                    }
                }
            }
        }
        return true;
    }
}


/////////////excel
var btnExport_Click = function () {
    App.frmMain.submit({
        url: 'IN10700/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {

        },
        success: function (msg, data) {
            var filePath = data.result.filePath;
            if (filePath) {
                window.location = "IN10700/Download?filePath=" + filePath + "&fileName=data_";
            }
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });

}
var btnImport_Click = function () {
    App.frmMain.submit({
        waitMsg: HQ.common.getLang("WaitMsg"),
        url: 'IN10700/Import',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        success: function (msg, data) {
            if (this.result.data.message) {
                HQ.message.show('2013103001', [this.result.data.message], '', true);
            }
            else {
                HQ.message.process(msg, data, true);
            }
            refresh("yes");
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
            App.btnImport.reset();
        }
    });
}

var refresh = function (value) {
    if (value == "yes") {
        if (App.dtpStkOutDate.getValue() != null) {
            App.stoStockOutlet.reload();
        }
        
    }
}

   
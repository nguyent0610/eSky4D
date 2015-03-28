var _hold = "H";
var _keys = ["InvcNbr", "DocDate"];

var Process = {
    deleteBatRef: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                url: 'AR10200/DeleteBatch',
                timeout: 1800000,
                params: {
                    lstBatNbr: Ext.encode(App.stoBatNbr.getRecordsValues())
                },
                success: function (action, data) {
                    App.cboBatNbr.store.load(function (records, operation, success) {
                        App.cboBatNbr.clearValue();
                    });
                },

                failure: function (errorMsg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, '', '');
                    }
                    else {
                        HQ.message.process(errorMsg, data, true);
                    }
                }
            });
        }
    },

    deleteAdjust: function (item) {
        if (item == "yes") {
            App.grdAdjust.deleteSelected();

            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                url: 'AR10200/DeleteAdjust',
                timeout: 1800000,
                params: {
                    lstAdjust: Ext.encode(App.grdAdjust.store.getChangedData({
                        skipIdForPhantomRecords: false
                    }))
                },
                success: function (data) {
                    App.grdAdjust.store.reload();
                },

                failure: function (errorMsg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, '', '');
                    }
                    else {
                        HQ.message.process(errorMsg, data, true);
                    }
                }
            });
        }
    },

    releaseAdjdRefNbr: function (strAdjdRefNbr) {
        App.frmMain.submit({
            clientValidation: false,
            waitMsg: HQ.common.getLang('Submiting') + "...",
            url: 'AR10200/ReleaseAdjdRef',
            timeout: 1800000,
            params: {
                strAdjdRefNbr: strAdjdRefNbr
            },
            success: function (msg, data) {
                var batNbr = '';

                if (this.result.data != undefined && this.result.data.batNbr != null) {
                    batNbr = this.result.data.batNbr;
                }
                HQ.message.process(msg, data, true);
                App.winRef.close();
                App.cboBatNbr.getStore().load(function () {
                    App.cboBatNbr.setValue(batNbr);
                    App.stoBatNbr.reload();
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }           
        });
    },

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.Form.menuClick("refresh");
        }
    },

    saveBatch: function () {
        App.frmMain.submit({
            clientValidation: false,
            waitMsg: HQ.common.getLang('Submiting') + "...",
            url: 'AR10200/SaveBatch',
            timeout: 1800000,
            params: {
                lstBatNbr: Ext.encode(App.stoBatNbr.getRecordsValues()),
                lstRefNbr: Ext.encode(App.stoRefNbr.getRecordsValues()),
                lstAdjust: Ext.encode(App.grdAdjust.store.getRecordsValues())
            },
            success: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, '', '');
                }
                App.cboBatNbr.store.load(function (records, operation, success) {
                    if (data.result.batNbr) {
                        App.cboBatNbr.setValue(data.result.batNbr);
                    }
                    App.stoBatNbr.reload();
                    App.cboRefNbr.store.load(function (records0, operation0, success0) {
                        if (data.result.refNbr) {
                            App.cboRefNbr.setValue(data.result.refNbr);
                        }
                        App.stoRefNbr.reload();
                    });
                });
            },

            failure: function (errorMsg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, '', '');
                }
                else {
                    HQ.message.process(errorMsg, data, true);
                }
            }
        });
    }
};

var Store = {
    stoBatNbr_load: function (sto, records, successful, eOpts) {
        if (!sto.getCount()) {
            var batRec = Ext.create("App.mdlBatNbr", {
                BranchID: HQ.cpnyID,
                Status: _hold
            });
            //sto.insert(0, batRec);
            //batRec.commit();
            HQ.store.insertRecord(sto, [], batRec, true);
        }
        var frmRecord = sto.getAt(0);
        App.pnlBatch.loadRecord(frmRecord);

        App.cboBankAcct.setValue(frmRecord.data.ReasonCD);
        App.txtAutoPayment.setValue(0);
        App.txtOdd.setValue(0);
    },

    stoRefNbr_load: function (sto, records, successful, eOpts) {
        if (!sto.getCount()) {
            var batRec = Ext.create("App.mdlRefNbr", {
                BranchID: HQ.cpnyID,
                DocDate: HQ.currentDate
            });
            //sto.insert(0, batRec);
            //batRec.commit();
            HQ.store.insertRecord(sto, [], batRec, true);
        }
        var docRec = sto.getAt(0);
        App.pnlDocument.loadRecord(docRec);
        if (docRec.data.RefNbr) {
            App.grdAdjust.store.reload();
        }
        else {
            App.grdAdjust.store.removeAll();
        }
    },

    stoAdjust_load: function (sto, records, successful, eOpts) {
        App.chkSelectHeader.setValue(false);
        var status = App.cboStatus.value;

        if (status == _hold) {
            var adjustRec = Ext.create("App.mdlAdjust", {
                BranchID: HQ.cpnyID,
                DocDate: App.dteDocDate.value ? App.dteDocDate.value : HQ.currentDate
            });
            HQ.store.insertRecord(sto, _keys, adjustRec);
        }
    },

    stoAdjust_dataChanged: function (sto, eOpts) {
        var paymentTotal = 0;
        var unpaymentTotal = 0;

        sto.each(function (record) {
            paymentTotal += record.data.Payment;
            unpaymentTotal += record.data.DocBal;
        });

        App.txtTotApply.setValue(paymentTotal);
        App.txtUnTotApply.setValue(unpaymentTotal);
    }
};

var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            App.cboBatNbr.store.load(function (records, operation, success) {
                App.stoBatNbr.reload();
                App.cboHandle.store.reload();
                App.cboRefNbr.store.load(function (records0, operation0, success0) {
                    App.stoRefNbr.reload();
                });
            });
        },

        frmMain_fieldChange: function (frm, field, newValue, oldValue, eOpts) {
            var batRec = App.pnlBatch.getRecord();
            if (batRec) {
                App.pnlBatch.updateRecord();
            }
            if (!HQ.store.isChange(App.stoBatNbr)) {
                var refRec = App.pnlDocument.getRecord();
                if (refRec) {
                    App.pnlDocument.updateRecord();
                }
                if (!HQ.store.isChange(App.stoRefNbr)) {
                    HQ.isChange = HQ.store.isChange(App.grdAdjust.store);
                }
                else {
                    HQ.isChange = true;
                }
            }
            else {
                HQ.isChange = true;
            }

            HQ.common.changeData(HQ.isChange, 'AR10200');//co thay doi du lieu gan * tren tab title header
            //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu

            App.cboBatNbr.setReadOnly(HQ.isChange);
            App.cboRefNbr.setReadOnly(HQ.isChange);
            //App.btnSearch.setReadOnly(HQ.isChange);
        },

        cboBatNbr_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboRefNbr.store.load(function (records, operation, success) {
                if (records.length) {
                    App.cboRefNbr.setValue(records[0].data.RefNbr);
                }
                App.stoBatNbr.reload();
            });
        },

        cboStatus_Change: function (cbo, newValue, oldValue, eOpts) {
            App.cboHandle.store.reload();
        },

        chkAllowSort_change: function (chk, newValue, oldValue, eOpts) {
            App.grdAdjust.sortableColumns = chk.checked;
            App.grdAdjust.update();
        },

        btnSearch_click: function (btn, e) {
            App.grdAdjust.store.reload();
        },

        cboRefNbr_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoRefNbr.reload();
        },

        btnAutoPayment_click: function (btn, e) {
            var autoPayment = App.txtAutoPayment.getValue() ? App.txtAutoPayment.getValue() : 0;
            var store = App.grdAdjust.store;
            var sumPayment = 0;

            if (autoPayment && autoPayment > 0) {
                store.each(function (record) {
                    record.set("Payment", 0);
                    record.set("Selected", false);

                    if (autoPayment >= record.data.OrigDocBal) {
                        autoPayment -= record.data.OrigDocBal;

                        record.set("Selected", true);
                        record.set("Payment", record.data.OrigDocBal);
                        record.set("DocBal", 0);
                    }
                    else if (autoPayment > 0) {
                        record.set("Selected", false);
                        record.set("Payment", autoPayment);
                        record.set("DocBal", record.data.OrigDocBal - autoPayment);
                        autoPayment = 0;
                    }
                    else {
                        record.set("Selected", false);
                        record.set("Payment", autoPayment);
                        record.set("DocBal", record.data.OrigDocBal - autoPayment);
                    }

                    sumPayment += record.data.Payment;
                });

                // Calc Odd
                var odd = App.txtAutoPayment.getValue() - sumPayment;
                App.txtOdd.setValue(odd);
                App.txtAutoPayment.setValue(0);
            }
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'batNbr') {
                        HQ.combo.first(App.cboBatNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'refNbr') {
                        HQ.combo.first(App.cboRefNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'adjust') {
                        HQ.grid.first(App.grdAdjust);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'batNbr') {
                        HQ.combo.next(App.cboBatNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'refNbr') {
                        HQ.combo.next(App.cboRefNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'adjust') {
                        HQ.grid.next(App.grdAdjust);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'batNbr') {
                        HQ.combo.prev(App.cboBatNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'refNbr') {
                        HQ.combo.prev(App.cboRefNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'adjust') {
                        HQ.grid.prev(App.grdAdjust);
                    }
                    break;

                case "last":
                    if (HQ.focus == 'batNbr') {
                        HQ.combo.last(App.cboBatNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'refNbr') {
                        HQ.combo.last(App.cboRefNbr, HQ.isChange);
                    }
                    else if (HQ.focus == 'adjust') {
                        HQ.grid.last(App.grdAdjust);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'batNbr') {
                            App.cboBatNbr.store.load(function (records, operation, success) {
                                App.stoBatNbr.reload();
                                App.cboRefNbr.store.load(function (records0, operation0, success0) {
                                    App.stoRefNbr.reload();
                                    App.grdAdjust.store.reload();
                                });
                            });
                        }
                        else if (HQ.focus == 'refNbr') {
                            App.cboRefNbr.store.load(function (records, operation, success) {
                                App.stoRefNbr.reload();
                                App.grdAdjust.store.reload();
                            });
                        }
                        else if (HQ.focus == 'adjust') {
                            App.grdAdjust.store.reload();
                        }
                    }
                    break;
                case "new":
                    if (HQ.isInsert) {
                        if (HQ.focus == 'batNbr') {
                            App.cboBatNbr.clearValue();
                        }
                        else if (HQ.focus == 'refNbr') {
                            App.cboRefNbr.clearValue();
                        }
                    }
                    break;
                case "delete":
                    if (HQ.isDelete) {
                        if (HQ.focus == 'batNbr' || HQ.focus == 'refNbr') {
                            if (App.cboBatNbr.value) {
                                HQ.message.show(11, '', 'Process.deleteBatRef');
                            }
                        }
                        else if (HQ.focus == 'adjust') {
                            if (App.cboBatNbr.value
                                && App.cboRefNbr.value
                                && App.slmAdjust.getCount()) {
                                HQ.message.show(2015020806,
                                    HQ.common.getLang('InvcNbr') + " " + App.slmAdjust.selected.items[0].data.InvcNbr,
                                    'Process.deleteAdjust');
                            }
                        }
                    }
                    break;
                case "save":
                    if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                        if (App.frmMain.isValid()) {
                            var handle = App.cboHandle.value;
                            var status = App.cboStatus.value;

                            if ((status == "U" || status == "C" || status == "H")
                                && (handle == "C" || handle == "V" || handle == "R")) {

                                if (handle == "R" || handle == "V" || handle == "C") {

                                    if (handle == "R" && !HQ.isRelease) {
                                        HQMessageBox.Show(737, hqSys.LangID, null, null);
                                    }
                                    else if ((handle == "V" || handle == "C") && !HQ.isRelease) {
                                        HQMessageBox.Show(725, hqSys.LangID, null, null);
                                    }
                                    else {
                                        if (handle == "V" || handle == "C") {
                                            if (handle == "V") {
                                                App.winRef.show();
                                                App.grdRef.store.reload();
                                            }
                                        }
                                        else if (handle == "R") {
                                            Process.saveBatch();
                                        }
                                    }
                                }

                                return;
                            }
                            else {
                                Process.saveBatch();
                            }
                        }
                    }
                    break;
                case "print":
                    break;
                case "close":
                    HQ.common.close(this);
                    break;
            }
        },

        chkSelectHeaderRef_change: function (chk, newValue, oldValue, eOpts) {
            App.grdRef.store.each(function (record) {
                if (record.data.Selected != chk.value) {
                    record.set("Selected", chk.value);
                }
            });
        },

        btnOK_click: function (btn, e) {
            var strAdjdRefNbr = "";
            var arrAdjdRefNbr = [];
            App.grdRef.store.each(function(record){
                if(record.data.Selected){
                    arrAdjdRefNbr.push(record.data.AdjdRefNbr);
                }
            });
            if (App.grdRef.store.getCount()) {
                if (arrAdjdRefNbr.length == App.grdRef.store.getCount()) {
                    strAdjdRefNbr = "%";
                }
                else {
                    strAdjdRefNbr = arrAdjdRefNbr.join(",");
                }
            }
            else {
                strAdjdRefNbr = "";
            }

            Process.releaseAdjdRefNbr(strAdjdRefNbr);
        },

        btnCancel_click: function (btn, e) {
            btn.up("window").close();
        }
    },

    Grid: {
        chkSelectHeader_change: function (chk, newValue, oldValue, eOpts) {
            if (App.cboStatus.value == _hold) {
                var store = chk.up("grid").store;
                store.each(function (record) {
                    if (record.data.InvcNbr) {
                        if (chk.checked) {
                            record.set("Selected", true);
                            record.set("DocBal", 0);
                            record.set("Payment", record.data.OrigDocBal);
                        }
                        else {
                            record.set("Selected", false);
                            record.set("DocBal", record.data.OrigDocBal);
                            record.set("Payment", 0);
                        }
                    }
                });
            }
            Event.Form.frmMain_fieldChange();
        },

        grdAdjust_beforeEdit: function (editor, e) {
            if (!HQ.grid.checkBeforeEdit(e, _keys)) {
                return false;
            }

            var record = e.record;
            var value = e.value;

            // Prevent edit if hold status
            if (App.cboStatus.getValue() != _hold) {
                return false;
            }

            // Prepare for editing in Selected check column
            if (e.field == 'Selected') { // value is true/false
                if (record.data.InvcNbr) {
                    if (value) {
                        record.set("DocBal", 0);
                        record.set("Payment", record.data.OrigDocBal ? record.data.OrigDocBal : 0);
                        record.set("Selected", true);
                    }
                    else {
                        record.set("DocBal", record.data.OrigDocBal ? record.data.OrigDocBal : 0);
                        record.set("Payment", 0);
                        record.set("Selected", false);
                    }
                }
            }

            // Prepare for editing in Payment column
            if (e.field == 'Payment') { // value is payment
                if (value >= record.data.OrigDocBal) {
                    record.set("DocBal", 0);
                    record.set("Payment", record.data.OrigDocBal);
                    record.set("Selected", true);
                    return true;
                }
                else {
                    record.set("Payment", value);
                    record.set("DocBal", record.data.OrigDocBal - value);
                    record.set("Selected", false);
                }
            }

            if (e.field == 'InvcNbr') {
                App.cboInvcNbr.store.reload();
            }
        },

        grdAdjust_edit: function (editor, e) {
            var record = e.record;
            var value = e.value;

            // Edited in Selected check column
            if (e.field == 'Selected') { // value is true/false
                if (value) {
                    record.set("DocBal", 0);
                    record.set("Payment", record.data.OrigDocBal);
                    record.set("Selected", true);
                }
                else {
                    record.set("DocBal", record.data.OrigDocBal);
                    record.set("Payment", 0);
                    record.set("Selected", false);
                }
            }
            else {
                var batNbr = App.cboBatNbr.value;
                var refNbr = App.cboRefNbr.value;
                var status = App.cboStatus.value;

                if (status == _hold) {
                    var adjustRec = Ext.create("App.mdlAdjust", {
                        BranchID: HQ.cpnyID,
                        DocDate: App.dteDocDate.value ? App.dteDocDate.value : HQ.currentDate
                    });
                    HQ.store.insertRecord(e.store, _keys, adjustRec);
                }
            }

            // Edited in Payment column
            if (e.field == 'Payment') { // value is payment
                if (value >= e.record.data.OrigDocBal) {
                    record.set("DocBal", 0);
                    record.set("Payment", record.data.OrigDocBal);
                    record.set("Selected", true);
                }
                else {
                    record.set("Payment", value);
                    record.set("DocBal", record.data.OrigDocBal - value);
                    record.set("Selected", false);
                }
            }

            if (e.field == 'InvcNbr') {
                var invcRec = App.cboInvcNbr.store.findRecord("InvcNbr", e.value);
                if (invcRec) {
                    e.record.set("BatNbr", invcRec.data.BatNbr);
                    e.record.set("BranchID", invcRec.data.BranchID);
                    e.record.set("CustId", invcRec.data.CustId);
                    e.record.set("CustName", invcRec.data.CustName);
                    e.record.set("Descr", invcRec.data.Descr);
                    e.record.set("DocBal", invcRec.data.DocBal);

                    e.record.set("DocDate", invcRec.data.DocDate);
                    e.record.set("DocType", invcRec.data.DocType);
                    e.record.set("InvcNbr", invcRec.data.InvcNbr);
                    e.record.set("IsChanged", invcRec.data.IsChanged);
                    e.record.set("OrigDocBal", invcRec.data.OrigDocBal);
                    e.record.set("Payment", invcRec.data.Payment);

                    e.record.set("RefNbr", invcRec.data.RefNbr);
                    e.record.set("Selected", invcRec.data.Selected);
                    e.record.set("SlsperID", invcRec.data.SlsperID);
                }
            }

            Event.Form.frmMain_fieldChange();
        },

        grdAdjust_validateEdit: function (editor, e) {

            return HQ.grid.checkValidateEdit(e.grid, e, _keys);
        },

        grdAdjust_reject: function (record, grid) {
            HQ.grid.checkReject(record, grid);
            Event.Form.frmMain_fieldChange();
        }
    }
};
var _beginStatus = "H";

var Process = {
    saveData: function () {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            url: 'IN22000/SaveData',
            waitMsg: HQ.common.getLang('Submiting') + "...",
            timeout: 1800000,
            params: {
                lstDetChange: HQ.store.getData(App.grdDet.store)
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                HQ.isChange = false;
                App.cboPosmID.store.load(function (records, operation, success) {
                    App.stoPOSM.reload();
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

    deletePosm: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'IN22000/DeletePosm',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    posmID: App.cboPosmID.getValue()//,
                    //isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.grdDet.store.reload();
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

    deleteBranch: function (item) {
        if (item == "yes"){
            App.grdDet.deleteSelected();
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

    renderDescrByCode: function (value, metaData, rec, rowIndex, colIndex, store) {
        if (metaData.column.dataIndex == "SlsName") {
            var record = App.cboColSlsperID.store.findRecord("SlsperId", rec.data.SlsperID);
            if (record) {
                value = record.data.Name;
            }
        }
        else if (metaData.column.dataIndex == "CustName") {
            var record = App.cboColCustID.store.findRecord("CustId", rec.data.CustID);
            if (record) {
                value = record.data.CustName;
            }
        }
        else if (metaData.column.dataIndex == "PosmName") {
            var record = App.cboColPosmCode.store.findRecord("InvtID", rec.data.PosmCode);
            if (record) {
                value = record.data.Descr;
            }
        }
        else if (metaData.column.dataIndex == "Status"){
            var record = App.stoStatus.findRecord("Code", rec.data.Status);
            if (record) {
                value = record.data.Descr;
            }
        }

        return value;
    }
};

var Store = {
    stoPOSM_load: function (sto, records, successful, eOpts) {
        HQ.isNew = false;
        if (sto.getCount() == 0) {
            var newPosm = Ext.create("App.mdlPOSM", {
                Active: true,
                FromDate: HQ.dateNow,
                ToDate: HQ.dateNow
            });
            sto.insert(0, newPosm);
            HQ.isNew = true;
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);

        App.grdDet.store.reload();

        Event.Form.frmMain_fieldChange();
    },

    stoDet_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var frmRec = App.frmMain.getRecord();
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

            if (frmRec && frmRec.data.Active) {
                if (successful) {
                    HQ.store.insertRecord(sto, keys, Ext.create("App.mdlDet", { Status: _beginStatus }));
                }
            }
        }
    }
};

var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            HQ.common.setRequire(frm);
            App.stoPOSM.reload();
        },

        frmMain_fieldChange: function () {
            if (App.stoPOSM.getCount() > 0) {
                App.frmMain.updateRecord();
                if (!HQ.store.isChange(App.stoPOSM)) {
                    HQ.isChange = HQ.store.isChange(App.grdDet.store);
                }
                else {
                    HQ.isChange = true;
                }

                HQ.common.changeData(HQ.isChange, 'IN22000');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                App.cboPosmID.setReadOnly(HQ.isChange);
            }
        },

        btnImport_click: function (btn, e) {
            Ext.Msg.alert("Import", "Coming soon!");
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        cboPosmID_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoPOSM.reload();
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(dtp.value);
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'posm') {
                        HQ.combo.first(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'branch') {
                        HQ.grid.first(App.grdDet);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'posm') {
                        HQ.combo.next(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'branch') {
                        HQ.grid.next(App.grdDet);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'posm') {
                        HQ.combo.prev(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'branch') {
                        HQ.grid.prev(App.grdDet);
                    }
                    break;
                case "last":
                    if (HQ.focus == 'posm') {
                        HQ.combo.last(App.cboPosmID, HQ.isChange);
                    }
                    else if (HQ.focus == 'branch') {
                        HQ.grid.last(App.grdDet);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'posm') {
                            App.cboPosmID.store.load(function (records, operation, success) {
                                App.stoPOSM.reload();
                            });
                        }
                        else if (HQ.focus == 'branch') {
                            App.grdDet.store.reload();
                        }
                    }
                    break;
                case "new":
                    if (HQ.isInsert) {
                        if (HQ.isChange) {
                            HQ.message.show(150, '', '');
                        }
                        else {
                            App.cboPosmID.clearValue();
                        }
                    }
                    break;
                case "save":
                    if (HQ.isUpdate || HQ.isCreate) {
                        if (App.frmMain.isValid()) {
                            var keys = App.grdDet.store.HQFieldKeys ? App.grdDet.store.HQFieldKeys : "";

                            if (HQ.store.checkRequirePass(App.grdDet.store, keys,
                                ['BranchID', 'CustID', 'PosmCode', 'SlsperID'],
                                ['BranchID', 'CustID', 'PosmCode', 'SlsperID'])) {
                                Process.saveData();
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
                case "delete":
                    if (HQ.isDelete) {
                        if (HQ.focus == 'posm') {
                            if (App.cboPosmID.getValue()) {
                                HQ.message.show(11, '', 'Process.deletePosm');
                            }
                        }
                        else if (HQ.focus == 'branch') {
                            if (App.cboPosmID.getValue() && App.slmDet.getCount()) {
                                if (App.slmDet.selected.items[0].data.Status == _beginStatus) {
                                    HQ.message.show(2015020806,
                                        HQ.common.getLang('CustID') + " " + App.slmDet.selected.items[0].data.CustID,
                                        'Process.deleteBranch');
                                }
                                else {
                                    HQ.message.show(20140306,'','');
                                }
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
        grdDet_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    if (e.record.data.Status == _beginStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
                        }

                        if (e.field == "BranchID") {
                            App.cboColCpny.store.reload();
                        }
                        else if (e.field == "SlsperID") {
                            App.cboColSlsperID.store.reload();
                        }
                        else if (e.field == "CustID") {
                            App.cboColCustID.store.reload();
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

        grdDet_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    HQ.store.insertRecord(e.store, keys, Ext.create("App.mdlDet", { Status: _beginStatus }));
                }
            }

            if (e.field == "BranchID") {
                if (e.value != e.originalValue) {
                    e.record.set("SlsperID", "");
                    e.record.set("CustID", "");
                }
            }
            else if (e.field == "SlsperID") {
                if (e.value != e.originalValue) {
                    e.record.set("CustID", "");
                }
            }

            //Event.Form.frmMain_fieldChange();
        },

        grdDet_validateEdit: function (item, e) {
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
};
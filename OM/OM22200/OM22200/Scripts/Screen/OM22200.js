var _beginStatus = "H";

var Store = {
    stoPJP_load: function (sto, records, successful, eOpts ) {
        if (sto.getCount() == 0) {
            var newPJP = Ext.create("App.mdlPJP", {
                PJPID: App.cboPJPID.getValue(),
                BranchID: App.cboBranchID.getValue(),
                StatusHandle: _beginStatus
            });
            sto.insert(0, newPJP);
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);
    }
};

var Process = {
    loadSaleRouteMaster: function (store) {
        if (App.cboPJPID.getValue() && App.cboBranchID.getValue()
            && App.cboSlsperId.getValue() && App.cboSalesRouteID.getValue()) {
                store.reload();
        }
        else {
            if (store.getCount()) {
                store.reload();
            }
        }
    },

    refreshGrid: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.menuClick("refresh");
        }
    },

    renderCustName: function (value) {
        //var store = App.cboColCustID.store
        return value;
    },

    getExistCustID: function (grid) {
        var lstCustID = [];
        grid.store.each(function (record) {
            lstCustID.push(record.data.CustID);
        });
        if (lstCustID.length) {
            return lstCustID.join(",");
        }
        else {
            return "";
        }
    },

    isValidSel: function () {
        var iVisit = 0;
        if (App.chkMon.value)
        {
            iVisit = iVisit + 1;
        }
        if (App.chkSat.value)
        {
            iVisit = iVisit + 1;
        }
        if (App.chkSun.value)
        {
            iVisit = iVisit + 1;
        }
        if (App.chkFri.value)
        {
            iVisit = iVisit + 1;
        }
        if (App.chkThu.value)
        {
            iVisit = iVisit + 1;
        }
        if (App.chkTue.value)
        {
            iVisit = iVisit + 1;
        }
        if (App.chkWed.value)
        {
            iVisit = iVisit + 1;
        }
        if (App.cboSlsFreq.getValue())
        {
            switch (App.cboSlsFreq.getValue())
            {
                case "F1":
                case "F2":
                case "F4":
                    if (iVisit != 1)
                    {
                        return false;
                    }
                    break;
                case "F8":
                    if (iVisit != 2)
                    {
                        return false;
                    }
                    break;
                case "F12":
                    if (iVisit != 3)
                    {
                        return false;
                    }
                    break;
                case "A":
                    if (iVisit == 0)
                    {
                        return false;
                    }
                    break;
            }
        }
        return true;
    },

    saveData: function () {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();

            App.frmMain.submit({
                url: 'OM22200/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstPJP: Ext.encode(App.stoPJP.getRecordsValues()),
                    lstSaleRouteMaster: HQ.store.getData(App.grdSalesRouteMaster.store)
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    HQ.isChange = false;
                    Event.menuClick("refresh");
                    //App.cboPJPID.store.load();
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

    deletePJP: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM22200/DeletePJP',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    branchId: App.cboBranchID.getValue(),
                    pjpID: App.cboPJPID.getValue()
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboPJPID.store.load();
                    App.cboPJPID.clearValue();
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

    deleteCust: function (item) {
        if (item == "yes") {
            App.grdSalesRouteMaster.deleteSelected();
        }
    },

    importExcel: function () {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'OM22200/OM22200Import',
            timeout:1800,
            clientValidation: false,
            method: 'POST',
            params: {
                BranchID: App.cboBranchID.getValue(),
                PJPID: App.cboPJPID.getValue()
            },
            success: function (msg, data) {
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }

            },
            failure: function (msg, data) {           
                HQ.message.process(msg, data, true);
            }
        });
    },

    exportExcel: function () {
        App.frmMain.submit({
            //waitMsg: HQ.common.getLang("Exporting")+"...",
            url: 'OM22200/Export',
            type: 'POST',
            timeout: 1000000,
            params: {
                BranchID: App.cboBranchID.getValue(),
                PJPID: App.cboPJPID.getValue(),
                BranchName: App.cboBranchID.getDisplayValue(),
                SlsPerID: App.cboSlsperId.getValue(),
                RouteID: App.cboSalesRouteID.getValue()
            },
            success: function (msg, data) {
                //processMessage(msg, data, true);
                //menuClick('refresh');
                var filePath = data.result.filePath;
                if (filePath) {
                    window.location = "OM22200/Download?filePath=" + filePath + "&fileName=MCP_" + App.cboBranchID.getValue();
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var Event = {
    frmMain_boxReady: function (frm, width, height, eOpts) {
        App.cboBranchID.store.load(function (records, operation, success) {
            App.cboBranchID.setValue(HQ.cpnyID);
        });
    },

    frmMain_fieldChange: function () {
        if (App.stoSaleRouteMaster.getCount() > 0) {
            App.frmMain.updateRecord();
            HQ.isChange = !HQ.store.isChange(App.stoSaleRouteMaster) ?
                HQ.store.isChange(App.stoPJP) : true;
            HQ.common.changeData(HQ.isChange, 'AR20200');//co thay doi du lieu gan * tren tab title header
            //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
            App.cboPJPID.setReadOnly(HQ.isChange);
            App.cboBranchID.setReadOnly(HQ.isChange);
            App.cboSalesRouteID.setReadOnly(HQ.isChange);
            App.cboSlsperId.setReadOnly(HQ.isChange);
        }
    },

    cboPJPID_change: function (cbo, newValue, oldValue, eOpts) {
        App.stoPJP.reload();
        Process.loadSaleRouteMaster(App.stoSaleRouteMaster);
    },

    cboBranchID_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboSalesRouteID.store.reload();
        App.cboSlsperId.store.reload();

        App.stoPJP.reload();
        Process.loadSaleRouteMaster(App.stoSaleRouteMaster);
    },

    cboSalesRouteID_change: function (cbo, newValue, oldValue, eOpts) {
        Process.loadSaleRouteMaster(App.stoSaleRouteMaster);
    },

    cboSlsperId_change: function (cbo, newValue, oldValue, eOpts) {
        Process.loadSaleRouteMaster(App.stoSaleRouteMaster);
    },

    cboSlsFreq_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboWeekofVisit.store.reload();
        App.ctnDayOfWeek.items.each(function (checkBox) {
            if (checkBox.value) {
                checkBox.setValue(false);
            }
        });
    },

    cboStatus_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboHandle.store.reload();
    },

    chk_change: function (chk, newValue, oldValue, eOpts) {
        var slsFreq = App.cboSlsFreq.value;
        var weekOfVisit = App.cboWeekofVisit.value;

        if (slsFreq && weekOfVisit) {
            var iVisit = 0;

            if (App.chkMon.value) {
                iVisit = iVisit + 1;
            }
            if (App.chkSat.value) {
                iVisit = iVisit + 1;
            }
            if (App.chkSun.value) {
                iVisit = iVisit + 1;
            }
            if (App.chkFri.value) {
                iVisit = iVisit + 1;
            }
            if (App.chkThu.value) {
                iVisit = iVisit + 1;
            }
            if (App.chkTue.value) {
                iVisit = iVisit + 1;
            }
            if (App.chkWed.value) {
                iVisit = iVisit + 1;
            }
            switch (slsFreq) {
                case "F1":
                case "F2":
                case "F4":
                    if (iVisit > 1) {
                        chk.setValue(false);
                    }
                    break;
                case "F8":
                    if (iVisit > 2) {
                        chk.setValue(false);
                    }
                    break;
                case "F12":
                    if (iVisit > 3) {
                        chk.setValue(false);
                    }
                    break;
            }
        }
        else {
            chk.setValue(false);
        }
    },

    fupImport_change: function (fup, newValue, oldValue, eOpts) {
        if (App.frmMain.isValid()) {
            var fileName = fup.getValue();
            var ext = fileName.split(".").pop().toLowerCase();
            if (ext == "xls" || ext == "xlsx") {
                Process.importExcel();

            } else {
                alert("Please choose a Media! (.xls, .xlsx)");
                sender.reset();
            }
        }
    },

    btnExport_click: function (btn, e, eOpts) {
        Process.exportExcel();
    },

    btnAddCust_click: function (btn, e, eOpts) {
        if (HQ.isInsert) {
            if (App.frmMain.isValid()) {
                if (App.cboStatus.getValue() == _beginStatus) {
                    if (Process.isValidSel()) {
                        App.stoCustomer.reload();
                        App.winCustomer.show();
                    }
                    else {
                        HQ.message.show(53, '', '');
                    }
                }
            }
        }
        else {
            HQ.message.show(4, '', '');
        }
    },

    btnUpdate_click: function (btn, e, eOpts) {
        if (HQ.isUpdate) {
            if (App.frmMain.isValid()) {
                if (App.cboStatus.getValue() == _beginStatus) {
                    if (Process.isValidSel()) {
                        var store = App.grdSalesRouteMaster.store;
                        store.each(function (record) {
                            if (record.data.Selected) {
                                record.set("SlsFreq", App.cboSlsFreq.getValue());
                                record.set("WeekofVisit", App.cboWeekofVisit.getValue());

                                App.ctnDayOfWeek.items.each(function (item) {
                                    record.set(item.tagHiddenName, item.value);
                                });

                                record.set("Selected", false);
                            }
                        });
                    }
                    else {
                        HQ.message.show(53, '', '');
                    }
                }
            }
        }
        else {
            HQ.message.show(4, '', '');
        }
    },

    btnGenerate_click: function (btn, e, eOpts) {
        if (HQ.isChange) {
            HQ.message.show(150, '', '');
        }
        else {
            
        }
    },

    chkSelectHeader_change: function (chk, newValue, oldValue, eOpts) {
        var store = chk.up('grid').store;

        store.each(function (record) {
            record.set("Selected", chk.value);
        });
    },

    grdSalesRouteMaster_Reject: function (record) {
        var grd = App.grdSalesRouteMaster;
        HQ.grid.checkReject(record, grd);

        if (record.data.tstamp == '') {
            grd.getStore().remove(record);
            grd.getView().focusRow(grd.getStore().getCount() - 1);
            grd.getSelectionModel().select(grd.getStore().getCount() - 1);
        } else {
            record.reject();
        }
        Event.frmMain_fieldChange();
    },

    grdSalesRouteMaster_beforeEdit: function (editor, e) {
        if (e.field == "WeekofVisit") {
            App.cboColWeekofVisit.store.reload();
        }
        else if (e.field == "CustID") {
            App.cboColCustID.store.reload();
        }
    },

    grdSalesRouteMaster_edit: function (editor, e) {
        Event.frmMain_fieldChange();
    },

    menuClick: function (command) {
        switch (command) {
            case "first":
                HQ.grid.first(App.grdSalesRouteMaster);
                break;
            case "next":
                HQ.grid.next(App.grdSalesRouteMaster);
                break;
            case "prev":
                HQ.grid.prev(App.grdSalesRouteMaster);
                break;
            case "last":
                HQ.grid.last(App.grdSalesRouteMaster);
                break;
            case "refresh":
                if (HQ.isChange) {
                    HQ.message.show(20150303, '', 'Process.refreshGrid');
                }
                else {
                    App.cboPJPID.getStore().load(function () {
                        App.stoPJP.reload();
                        App.grdSalesRouteMaster.store.reload();
                    });

                }
                break;
            case "new":
                if (HQ.isInsert) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboPJPID.clearValue();
                        //cboMailID_Change(App.cboMailID);
                    }
                }
                break;
            case "delete":
                if (HQ.isDelete) {
                    if (HQ.focus == "header") {
                        if (App.cboBranchID.getValue() && App.cboPJPID.getValue()) {
                            if (App.cboStatus.getValue() == _beginStatus) {
                                HQ.message.show(11, '', 'Process.deletePJP');
                            }
                            else {
                                HQ.message.show(20140306, '', '');
                            }
                        }
                    }
                    else if (HQ.focus == "grid") {
                        if (App.cboBranchID.getValue() && App.cboPJPID.getValue()) {
                            HQ.message.show(2015020806,
                                    HQ.common.getLang('CustID') + " " + App.slmSalesRouteMaster.selected.items[0].data.CustID,
                                    'Process.deleteCust');
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
    },

    // CUSTOMER
    chkSelectHeaderCust_change: function (chk, newValue, oldValue, eOpts) {
        var grid = chk.up("grid");

        if (newValue) {
            grid.store.each(function (record) {
                record.set("Selected", true);
            });
        }
        else {
            grid.store.each(function (record) {
                record.set("Selected", false);
            });
        }    
    },

    btnCustOK_click: function (btn, eOpts) {
        if (HQ.isInsert) {
            App.grdCustomer.store.each(function (record) {
                if (record.data.Selected) {
                    var newRec = Ext.create("App.mdlSaleRouteMaster", {
                        CustID: record.data.CustId,
                        BranchID: App.cboBranchID.getValue(),
                        CustName: record.data.CustName,
                        PJPID: App.cboPJPID.getValue(),
                        SalesRouteID: App.cboSalesRouteID.getValue(),
                        SlsPerID: App.cboSlsperId.getValue(),
                        SlsFreq: App.cboSlsFreq.getValue(),
                        WeekofVisit: App.cboWeekofVisit.getValue(),
                        Sun: App.chkSun.value,
                        Mon: App.chkMon.value,
                        Tue: App.chkTue.value,
                        Wed: App.chkWed.value,
                        Thu: App.chkThu.value,
                        Fri: App.chkFri.value,
                        Sat: App.chkSat.value
                    });
                    HQ.store.insertRecord(App.grdSalesRouteMaster.store, ["CustID"], newRec);
                }
            });

            App.winCustomer.close();
        }
        else {
            HQ.message.show(4, '', '');
        }
    }
};
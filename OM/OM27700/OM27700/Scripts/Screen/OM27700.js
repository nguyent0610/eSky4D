var _beginStatus = "H";
var _custBranches = [];
var _salesBranches = [];
var _displayType = {
    Level: "L",
    Signboard: "S",
    LCD: "M"
};

var _applyType = {
    Amount: "A",
    Qty: "Q"
};
var _source = 0;

var Process = {
    renderCpnyName: function (value) {
        var record = App.cboGCpnyID.store.findRecord("CpnyID", value);
        if (record) {
            return record.data.CpnyName;
        }
        else {
            return value;
        }
    },

    renderLocDescr: function (value) {
        var record = App.cboColLocID.store.findRecord("Code", value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },

    renderInvtInfo: function (value, metaData, record, rowIndex, colIndex, store) {
        var rec = App.cboColInvtID.store.findRecord("Code", record.data.InvtID);
        var returnValue = value;
        if (rec) {
            if (metaData.column.dataIndex == "Descr") {
                returnValue = rec.data.Descr;
            }
            else if (metaData.column.dataIndex == "StkUnit") {
                returnValue = rec.data.StkUnit;
            }
        }

        return returnValue;
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

    focusOnInvalidField: function (item) {
        if (item == "ok") {
            App.frmMain.getForm().getFields().each(function (field) {
                if (!field.isValid()) {
                    field.focus();
                    return false;
                }
            });
        }
    },

    deleteSelectedCompanies: function (item) {
        if (item == "yes") {
            var selRecs = App.grdCompany.selModel.selected.items;
            if (selRecs.length > 0) {
                if (App.cboObjApply.getValue() == 'C') {
                    App.stoCustomer.suspendEvents();
                    var allData = App.stoCustomer.allData || App.stoCustomer.snapshot || App.stoCustomer.data;
                    selRecs.forEach(function (record) {
                        for (var i = allData.items.length - 1; i >= 0; i--) {
                            if (allData.items[i].data.CpnyID == record.data.CpnyID) {
                                App.grdCustomer.getStore().remove(allData.items[i], App.grdCustomer);
                                App.grdCustomer.getView().focusRow(App.grdCustomer.getStore().getCount() - 1);
                                App.grdCustomer.getSelectionModel().select(App.grdCustomer.getStore().getCount() - 1);
                            }
                        }
                    });
                    App.stoCustomer.resumeEvents();
                    App.grdCustomer.view.refresh();
                    App.stoCustomer.loadPage(1);
                } else if (App.cboObjApply.getValue() == 'S') {
                    App.stoSales.suspendEvents();
                    var allData = App.stoSales.allData || App.stoSales.snapshot || App.stoSales.data;
                    selRecs.forEach(function (record) {
                        for (var i = allData.items.length - 1; i >= 0; i--) {
                            if (allData.items[i].data.CpnyID == record.data.CpnyID) {
                                App.grdSales.getStore().remove(allData.items[i], App.grdSales);
                                App.grdSales.getView().focusRow(App.grdSales.getStore().getCount() - 1);
                                App.grdSales.getSelectionModel().select(App.grdSales.getStore().getCount() - 1);
                            }
                        }
                    });
                    App.stoSales.resumeEvents();
                    App.grdSales.view.refresh();
                    App.stoSales.loadPage(1);
                }                    
                

                App.grdCompany.deleteSelected();
                Event.Form.frmMain_fieldChange();
            }
        }
    },

    deleteAllCompanies: function (item) {
        if (item == "yes") {
            App.stoCompany.suspendEvents();
            var allData = App.stoCompany.snapshot || App.stoCompany.allData || App.stoCompany.data;
            var selRecs = allData.items;
            for (var i = selRecs.length - 1; i >= 0; i--) {
                App.grdCompany.getStore().remove(allData.items[i], App.grdCompany);
                App.grdCompany.getView().focusRow(App.grdCompany.getStore().getCount() - 1);
                App.grdCompany.getSelectionModel().select(App.grdCompany.getStore().getCount() - 1);
            }
            App.stoCompany.resumeEvents();
            App.grdCompany.view.refresh();
            //App.grdCompany.store.removeAll();

            deleteAllSales(item);
            deleteAllCustomer(item);
            Event.Form.frmMain_fieldChange();
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

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.Form.menuClick("refresh");
        }
    },

    checkDataBeforeSave: function() {
        var validData = true;
        if (App.stoCompany.data.length == 0 || App.stoCompany.data.length == 1 && App.stoCompany.data.items[0].data.CpnyID == '' ) {
            HQ.message.show(2017060601, [App.tabCompany.title], '', true);
            App.tabInfo.setActiveTab(0);
            validData = false;
            return false;
        }

        if (App.stoLevel.data.length == 0 ||
                (
                    App.stoLevel.data.length == 1
                    && App.stoLevel.data.items[0].data.LevelDescr == ''
                    && App.stoLevel.data.items[0].data.LevelFrom == 0
                    && App.stoLevel.data.items[0].data.LevelTo == 0
                    && App.stoLevel.data.items[0].data.PercentBonus == 0
                    && App.stoLevel.data.items[0].data.LevelType == ''
                )
            )
        {
            validData = false;
            App.tabInfo.setActiveTab(1);
            HQ.message.show(2017060601, [App.tabLevel.title], '', true);
            return false;
        } else {
            var flat = null;
            App.stoLevel.data.each(function (item) {
                if (!Ext.isEmpty(item.data.LevelDescr) 
                    || item.data.LevelFrom != 0
                    || item.data.LevelTo != 0
                    || item.data.PercentBonus != 0
                    || !Ext.isEmpty(item.data.LevelType)
                    
                ) {
                    if (Ext.isEmpty(item.data.LevelDescr)) {
                        HQ.message.show(2017060602, [Process.findColumnNameByIndex(App.grdLevel, 'LevelDescr'), App.tabLevel.title], '', true);
                        flat = item;
                        return false;
                    }

                    if (item.data.LevelFrom > item.data.LevelTo) {
                        HQ.message.show(2016070401, [Process.findColumnNameByIndex(App.grdLevel, 'LevelFrom'), Process.findColumnNameByIndex(App.grdLevel, 'LevelTo')], '', true);
                        flat = item;
                        return false;
                    }

                    if (Ext.isEmpty(item.data.LevelType)) {
                        HQ.message.show(2017060602, [Process.findColumnNameByIndex(App.grdLevel, 'LevelType'), App.tabLevel.title], '', true);
                        flat = item;
                        return false;
                    }
                }
            });

            if (!Ext.isEmpty(flat)) {
                validData = false;
                App.tabInfo.setActiveTab(1);
                App.slmLevel.select(App.stoLevel.indexOf(flat));
                return;
            }
        }
        if (App.cboObjApply.getValue() == 'C') { // Customer
            if (App.stoCustomer.data.length == 0 || App.stoCustomer.data.length == 1 && App.stoCustomer.data.items[0].data.CustID == '') {
                validData = false;
                App.tabInfo.setActiveTab(2);
                HQ.message.show(2017060601, [App.tabCustomer.title], '', true);
            }
        } else if (App.cboObjApply.getValue() == 'S') { // Sales
            if (App.stoSales.data.length == 0 || App.stoSales.data.length == 1 && App.stoSales.data.items[0].data.SlsperID == '') {
                validData = false;
                App.tabInfo.setActiveTab(3);
                HQ.message.show(2017060601, [App.tabSales.title], '', true);
            }
        }
        return validData;
    }

    , saveData: function () {        
        if (Process.checkDataBeforeSave()) {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();

                App.frmMain.submit({
                    url: 'OM27700/SaveData',
                    waitMsg: HQ.common.getLang('Submiting') + "...",
                    timeout: 1800000,
                    params: {
                        lstAccumulate: Ext.encode(App.stoDisplay.getRecordsValues()),
                        lstCpny: Ext.encode(App.grdCompany.store.getRecordsValues()),
                        lstCpnyChange: HQ.store.getData(App.grdCompany.store),
                        lstLevelChange: HQ.store.getData(App.grdLevel.store),
                        lstInvtChange: HQ.store.getData(App.grdInvt.store),
                        lstCustomer: Ext.encode(App.grdCustomer.store.getRecordsValues()),
                        lstSales: Ext.encode(App.grdSales.store.getRecordsValues()),
                        isNew: HQ.isNew
                    },
                    success: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        HQ.isChange = false;
                        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
                        App.cboAccumulateID.store.load(function (records, operation, success) {
                            App.stoDisplay.reload();
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
            }
            else {
                Process.showFieldInvalid(App.frmMain);
            }
        }
    },

    deleteDisplay: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM27700/DeleteAccumulate',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    accumulateID: App.cboAccumulateID.getValue()
                    //isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboAccumulateID.store.load();
                    App.cboAccumulateID.clearValue();
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

    deleteLevel: function (item) {
        if (item == "yes") {
            App.grdInvt.store.removeAll();
            App.grdLevel.deleteSelected();
            Event.Form.frmMain_fieldChange();
        }
    },

    deleteInvt: function (item) {
        if (item == "yes") {
            App.grdInvt.deleteSelected();
        }
        Event.Form.frmMain_fieldChange();
    },

    lastNbr: function (store) {
        var num = 0;
        for (var j = 0; j < store.data.length; j++) {
            var item = store.data.items[j];

            if (!Ext.isEmpty(item.data.LevelID) && parseInt(item.data.LevelID) > num) {
                num = parseInt(item.data.LevelID);
            }
        };
        num++;
        return num.toString();
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

    getChangedFilteredData: function (store) {
        var data = store.data,
            changedData

        store.data = store.snapshot; // does the trick
        changedData = store.getChangedData();
        store.data = data; // to revert the changes back
        return Ext.encode(changedData);
    }

    , loadTreeCustomer: function (lstCpny) {
        HQ.common.showBusy(true, HQ.waitMsg);
        App.direct.OM27700GetTreeCustomer('treeCustomer', lstCpny, {
            success: function (result) {
                App.treePanelCustomer.getRootNode().expand();
                HQ.common.showBusy(false);
            }
        });        
    }

    , loadTreeSales: function (lstCpny) {
        HQ.common.showBusy(true, HQ.waitMsg);
        App.direct.OM27700GetTreeSales('treeSales', lstCpny, {
            success: function (result) {
                App.treePanelSales.getRootNode().expand();
                HQ.common.showBusy(false);
            }
        });
    }

    , findColumnNameByIndex: function (grd, dataIndex) {
        var index = HQ.grid.findColumnIndex(grd.columns, dataIndex);
        return index != -1 ? grd.columns[index].text : dataIndex;
    },
};

var Store = {
    stoDisplaynoData_load: function (sto) {
    //    HQ.isNew = false;
    //    if (sto.getCount() == 0) {
    //        var newDisplay = Ext.create("App.mdlOM_TDisplay", {
    //            FromDate: HQ.dateNow,
    //            ToDate: HQ.dateNow,
    //            Status: _beginStatus,
    //            DisplayType: 'L'
    //        });
    //        HQ.store.insertRecord(sto, [], newDisplay, true);
    //        HQ.isNew = true;
    //    }
    //    var frmRecord = sto.getAt(0);
    //    App.frmMain.loadRecord(frmRecord);

    //    App.grdCompany.store.clearData();

    //    App.grdLevel.store.clearData();
    //    App.grdInvt.store.clearData();
    //    App.grdSetup.store.clearData();
    //    App.grdLoc.store.clearData();
    //    HQ.store.insertBlank(App.stoLocation, ['LocationID']);
    //    // HQ.store.insertBlank(App.stoLevel, ['LevelID']);
    //    var rec = Ext.create(App.stoLevel.model.modelName, {
    //        LevelID: Process.lastNbr(App.stoLevel)
    //    });
    //    HQ.store.insertRecord(App.stoLevel, ['LevelID'], rec);

    //    HQ.store.insertBlank(App.stoInvt, ['InvtID']);
    //    App.grdCompany.view.refresh();
    //    App.grdLevel.view.refresh();
    //    App.grdInvt.view.refresh();
    //    App.grdLoc.view.refresh();
    //    App.grdSetup.view.refresh();

    //    _source = 0;
    //    if (frmRecord.data.tstamp) {
    //        App.cboApplyFor.setReadOnly(true);
    //        App.cboApplyType.setReadOnly(true);

    //        if (frmRecord.data.Status == _beginStatus) {
    //            App.dtpFromDate.setReadOnly(false);
    //            App.dtpToDate.setReadOnly(false);
    //            App.txtDescr.setReadOnly(false);
    //        }
    //        else {
    //            App.dtpFromDate.setReadOnly(true);
    //            App.dtpToDate.setReadOnly(true);
    //            App.txtDescr.setReadOnly(true);
    //        }
    //    }
    //    else {
    //        App.cboApplyFor.setReadOnly(false);
    //        App.cboApplyType.setReadOnly(false);

    //        App.dtpFromDate.setReadOnly(false);
    //        App.dtpToDate.setReadOnly(false);
    //        App.txtDescr.setReadOnly(false);
    //    }

    //    Event.Form.frmMain_fieldChange();
    },

    stoDisplay_load: function (sto, records, successful, eOpts) {
        HQ.isNew = false;
        if (sto.getCount() == 0) {
            var newDisplay = Ext.create("App.mdlOM_Accumulated", {
                FromDate: HQ.dateNow,
                ToDate: HQ.dateNow,
                Status: _beginStatus,
                DisplayType: 'L'
            });
            HQ.store.insertRecord(sto, [], newDisplay, true);
            HQ.isNew = true;
        }
        
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);
        _source = 0;

        //HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));

        if (HQ.isNew) {
            App.grdCompany.store.clearData();
            App.grdLevel.store.clearData();
            App.grdInvt.store.clearData();
            App.grdCustomer.store.clearData();

            // HQ.store.insertBlank(App.stoLevel, ['LevelID']);
            var rec = Ext.create(App.stoLevel.model.modelName, {
                LevelID: Process.lastNbr(App.stoLevel)
            });
            HQ.store.insertRecord(App.stoLevel, ['LevelID'], rec);

            HQ.store.insertBlank(App.stoInvt, ['InvtID']);
            App.grdCompany.view.refresh();
            App.grdLevel.view.refresh();
            App.grdInvt.view.refresh();        
            HQ.common.showBusy(false);
        } else {
            HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
            App.grdCompany.store.reload();
            App.grdLevel.store.reload();
            App.grdInvt.store.reload();
            App.grdCustomer.store.reload();
            App.grdSales.store.reload();
        }        
        if (frmRecord.data.tstamp) {
            App.cboApplyFor.setReadOnly(true);
            App.cboApplyType.setReadOnly(true);
            App.cboObjApply.setReadOnly(true);
            if (frmRecord.data.Status == _beginStatus) {
                App.dtpFromDate.setReadOnly(false);
                App.dtpToDate.setReadOnly(false);
                App.txtDescr.setReadOnly(false);
            }
            else {
                App.dtpFromDate.setReadOnly(true);
                App.dtpToDate.setReadOnly(true);
                App.txtDescr.setReadOnly(true);
            }
        }
        else {
            App.cboApplyFor.setReadOnly(false);
            App.cboApplyType.setReadOnly(false);
            App.cboObjApply.setReadOnly(false);
            App.dtpFromDate.setReadOnly(false);
            App.dtpToDate.setReadOnly(false);
            App.txtDescr.setReadOnly(false);
        }

        Event.Form.frmMain_fieldChange();
    },

    stoGrid_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var status = App.cboStatus.getValue();
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

            if (status == _beginStatus) {
                if (successful) {
                    if (keys.indexOf('LevelID') > -1) {
                        var rec = Ext.create(sto.model.modelName, {
                            LevelID: Process.lastNbr(sto)
                        });
                        HQ.store.insertRecord(sto, keys, rec);
                    }
                    else {
                        //if (sto.storeId != 'stoSetup') {
                        //    HQ.store.insertBlank(sto, keys);
                        //}
                    }
                }
            }
        }
    }

    , //trước khi load trang busy la dang load data
     sto_BeforeLoad : function () {
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    }
};

var Event = {
    Form: {
        checkLoad: function (sto) {
            _source += 1;
            if (_source == 5) {
                _source = 0;
                HQ.isNew = false;
                App.grdInvt.store.filterBy(function (record) { });
                HQ.common.showBusy(false);
            }
        },

        checkStoreMasterLoad: function (sto) {
            HQ.masterSource++;
            if (HQ.masterSource == HQ.maxMasterSource) {
                HQ.masterSource = 0;
               // HQ.common.showBusy(false);
                App.stoDisplay.reload();
            }           
        },

        frmMain_boxReady: function (frm, width, height, eOpts) {
            HQ.masterSource = 0;
            HQ.maxMasterSource = 9;
            //Store.sto_BeforeLoad();

            App.cboAccumulateID.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            App.cboApplyFor.getStore().addListener('load', Event.Form.checkStoreMasterLoad);

            App.cboApplyType.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            App.cboObjApply.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            App.cboStatus.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            App.cboGCpnyID.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            App.cboCustID.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            App.cboSlsperID.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            App.cboLevelType.getStore().addListener('load', Event.Form.checkStoreMasterLoad);
            

            App.grdCompany.store.addListener('load', Event.Form.checkLoad);
            App.grdLevel.store.addListener('load', Event.Form.checkLoad);
            App.grdInvt.store.addListener('load', Event.Form.checkLoad);
            App.grdCustomer.store.addListener('load', Event.Form.checkLoad);
            App.grdSales.store.addListener('load', Event.Form.checkLoad);

            App["tabCustomer"].disable();
            App["tabSales"].disable();
            HQ.common.setRequire(frm);            
        },

        frmMain_fieldChange: function (frm, field, newValue, oldValue, eOpts) {
            if (App.stoDisplay.getCount() > 0) {
                App.frmMain.updateRecord();

                HQ.isChange = HQ.store.isChange(App.stoDisplay) ||
                        HQ.store.isChange(App.grdCompany.store) ||
                        HQ.store.isChange(App.grdLevel.store) ||
                        HQ.store.isChange(App.grdInvt.store) ||
                        HQ.store.isChange(App.grdCustomer.store)
                        || HQ.store.isChange(App.grdSales.store)
                HQ.common.changeData(HQ.isChange, 'OM27700');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                App.cboAccumulateID.setReadOnly(HQ.isChange && App.cboAccumulateID.getValue());

                var frmRecord = App.frmMain.getRecord();
                if (!frmRecord.data.tstamp) {
                    if (HQ.isChange
                        && (App.grdCompany.store.getCount() > 0 || App.grdLevel.store.getCount() > 1)) {
                        //App.cboDisplayType.setReadOnly(true);
                    }
                    else {
                        //App.cboDisplayType.setReadOnly(false);
                    }
                }
            }
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        cboAccumulateID_change: function (cbo, newValue, oldValue, eOpts) {
            //var data;
            //App.cboAccumulateID.store.data.each(function (item) {
            //    var tmp1 = item.get('DisplayID');
            //    var tmp2 = newValue;
            //    var val1 = (tmp1 == undefined || tmp1 == null) ? '' : tmp1.toString().toLowerCase();
            //    var val2 = (tmp2 == undefined || tmp2 == null) ? '' : tmp2.toString().toLowerCase();
            //    if (val1 == val2) {
            //        data = item;
            //        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
            //        App.cboAccumulateID.setValue('');
            //        App.cboAccumulateID.setValue(item.data.DisplayID);
            //        App.stoDisplay.reload();
            //        return false;
            //    } else {
            //        App.stoDisplay.clearData();
            //        Store.stoDisplaynoData_load(App.stoDisplay);
            //    }
            //});

            App.stoDisplay.reload();
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(newValue);            
            App.dtpToDate.validate();          
        },

        cboApplyType_change: function (cbo, newValue, oldValue, eOpts) {
            if (cbo.getValue()) {
                if (cbo.getValue() == _applyType.Amount) {
                    App.pnlInvt.hide();
                    App.stoLevel.reload();
                }
                else if (cbo.getValue() == _applyType.Qty) {
                    App.pnlInvt.show();
                }
            }
            else {
                App.pnlInvt.hide();
            }
        },

        cboApplyType_Expand: function (s, e) {
            if (App.stoLevel.data.length >= 1) {
                var firstRecord = App.stoLevel.data.items[0].data;
                if (firstRecord.LevelDescr != '' ||
                    firstRecord.LevelFrom != 0 ||
                    firstRecord.LevelTo != 0 ||
                    firstRecord.LevelType != '') {
                    App.cboApplyType.collapse();
                }
            }
        },

        cboApplyTypeTrigger_click: function (ctr) {
            if (App.stoLevel.data.length >= 1) {
                var firstRecord = App.stoLevel.data.items[0].data;
                if (firstRecord.LevelDescr != '' ||
                    firstRecord.LevelFrom != 0 ||
                    firstRecord.LevelTo != 0 ||
                    firstRecord.LevelType != '') {
                    App.cboApplyType.collapse();
                }
                else {
                    ctr.clearValue();
                }
            } else {
                ctr.clearValue();
            }

        },

        cboObjApply_change: function (cbo, newValue, oldValue, eOpts) {
           // var listTabs = ["tabCompany", "tabCustomer", "tabSales", "tabLevel"]; 
           
            App["tabCustomer"].disable();
            App["tabSales"].disable();
            if (!Ext.isEmpty(App.cboObjApply.getValue())) {               
                if (App.cboObjApply.getValue() == 'S') {
                    App["tabSales"].enable();
                    if (App.tabInfo.activeTab.id == 'tabCustomer') {
                        App.tabInfo.setActiveTab(0);
                    }
                } else if (App.cboObjApply.getValue() == 'C') {
                    App["tabCustomer"].enable();
                    if (App.tabInfo.activeTab.id == 'tabSales') {
                        App.tabInfo.setActiveTab(0);
                    }
                } else {
                    App.tabInfo.setActiveTab(0);
                }
            } else {
                App.tabInfo.setActiveTab(0);
            }
        },

        tabInfo_Change: function (tabPanel, newCard, oldCard, eOpts) {
            if (newCard.id == 'tabCustomer' && App.cboObjApply.getValue() == 'C') {
                var loadTree = false;
                var branches = [];
                var store = App.stoCompany.snapshot || App.stoCompany.allData || App.stoCompany.data;
                var stoCpnyLength = store.length;
                if (_custBranches.length == 0 || _custBranches.length != stoCpnyLength) {     // First Load               
                    for (var i = 0; i < stoCpnyLength; i++) {
                        branches.push(store.items[i].data.CpnyID);
                    }
                    _custBranches = branches;
                    loadTree = true;
                } else {
                    for (var i = 0; i < stoCpnyLength; i++) {
                        var flag = false;
                        for (var j = 0; j < _custBranches.length; j++) {
                            if (store.items[i].data.CpnyID == _custBranches[j]) {                                
                                flag = true;
                                break;
                            }
                        }
                        if (flag == false) {
                            loadTree = true;
                            break;
                        }                        
                    }
                    for (var i = 0; i < stoCpnyLength; i++) {
                        branches.push(store.items[i].data.CpnyID);
                    }
                    _custBranches = branches;
                }
                
                if (loadTree) {
                    Process.loadTreeCustomer(_custBranches.join(','));
                }
                
            } else if (newCard.id == 'tabSales' && App.cboObjApply.getValue() == 'S') {
                var loadTree = false;
                var branches = [];
                var store = App.stoCompany.snapshot || App.stoCompany.allData || App.stoCompany.data;
                var stoCpnyLength = store.length;
                if (_salesBranches.length == 0 || _salesBranches.length != stoCpnyLength) {     // First Load               
                    for (var i = 0; i < stoCpnyLength; i++) {
                        branches.push(store.items[i].data.CpnyID);
                    }
                    _salesBranches = branches;
                    loadTree = true;
                } else {
                    for (var i = 0; i < stoCpnyLength; i++) {
                        var flag = false;
                        for (var j = 0; j < _salesBranches.length; j++) {
                            if (store.items[i].data.CpnyID == _salesBranches[j]) {
                                flag = true;
                                break;
                            }
                        }
                        if (flag == false) {
                            loadTree = true;
                            break;
                        }
                    }
                    for (var i = 0; i < stoCpnyLength; i++) {
                        branches.push(store.items[i].data.CpnyID);
                    }
                    _salesBranches = branches;
                }

                if (loadTree) {
                    Process.loadTreeSales(_salesBranches.join(','));
                }
            }
        },

        menuClick: function (command) {
            var grd = null;
            if (HQ.focus == 'cpny') {
                grd = App.grdCompany;
            } else if (HQ.focus == 'level') {
                grd = App.grdLevel;
            } else if (HQ.focus == 'invt') {
                grd = App.grdInvt;
            } else if (HQ.focus == 'customer') {
                grd = App.grdCustomer;
            } else if (HQ.focus == 'sales') {
                grd = App.grdSales;
            }

            switch (command) {
                case "first":
                    if (HQ.focus == 'accumulate') {
                        HQ.combo.first(App.cboAccumulateID, HQ.isChange);
                    } else if (grd) {
                        HQ.grid.first(grd);
                    } 
                    break;
                case "next":
                    if (HQ.focus == 'accumulate') {
                        HQ.combo.next(App.cboAccumulateID, HQ.isChange);
                    } else if (grd) {
                        HQ.grid.next(grd);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'accumulate') {
                        HQ.combo.prev(App.cboAccumulateID, HQ.isChange);
                    } else if (grd) {
                        HQ.grid.prev(grd);
                    }
                    break;

                case "last":
                    if (HQ.focus == 'accumulate') {
                        HQ.combo.last(App.cboAccumulateID, HQ.isChange);
                    } else if (grd) {
                        HQ.grid.last(grd);
                    } 
                    break;
                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'accumulate') {
                            App.cboAccumulateID.store.load(function (records, operation, success) {
                                App.cboAccumulateID.clearValue();
                                App.stoDisplay.reload();
                            });
                        }
                        else {
                            HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
                            _source = 0;
                            App.grdCompany.store.reload();                        
                            App.grdLevel.store.reload();                                                
                            App.grdInvt.store.reload();
                            App.grdCustomer.store.reload();
                            App.grdSales.store.reload();
                        }
                    }
                    break;
                case "new":
                    if (HQ.isInsert) {                        
                        if (HQ.focus == 'accumulate') {
                            if (HQ.isChange) {
                                HQ.message.show(150, '', '');
                            }
                            else {
                                App.cboAccumulateID.clearValue();
                            }
                        }
                        //else if (HQ.focus == 'cpny') {
                        //    HQ.grid.insert(App.grdCompany, ['CpnyID']);
                        //}
                    }
                    break;
                case "delete":
                    if (HQ.isDelete) {
                        if (App.cboStatus.getValue() == _beginStatus) {
                            if (HQ.focus == 'accumulate') {
                                if (App.cboAccumulateID.getValue()) {
                                    HQ.message.show(11, '', 'Process.deleteDisplay');
                                }
                            }
                            else if (HQ.focus == 'cpny') {
                                var selRecs = App.grdCompany.selModel.selected.items;
                                if (selRecs.length > 0) {
                                    var params = [];
                                    selRecs.forEach(function (record) {
                                        params.push(record.data.CpnyID);
                                    });
                                    if (App.cboObjApply.getValue() == 'B') {
                                        HQ.message.show(2015020806,
                                        Process.findColumnNameByIndex(App.grdCompany, 'CpnyID') + " " + params.join(" & "),
                                        'Process.deleteSelectedCompanies', '', true);
                                    } else if (App.cboObjApply.getValue() == 'C') {
                                        HQ.message.show(2017060603,
                                        [params.join(" & "), App.tabCustomer.title],
                                        'Process.deleteSelectedCompanies', true);
                                    } else if (App.cboObjApply.getValue() == 'S') {
                                        HQ.message.show(2017060603,
                                        [params.join(" & "), App.tabSales.title],
                                        'Process.deleteSelectedCompanies', true);
                                    }
                                    
                                }
                            }
                            else if (HQ.focus == 'level') {
                                var rowindex = HQ.grid.indexSelect(App.grdLevel);
                                if (rowindex != '') {
                                    HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdLevel), ''], 'Process.deleteLevel', true);
                                }
                            }
                            else if (HQ.focus == 'invt') {
                                if (App.cboAccumulateID.getValue() && App.slmInvt.getCount()) {
                                    HQ.message.show(2015020806,
                                        HQ.common.getLang('InvtID') + " " + App.slmInvt.selected.items[0].data.InvtID,
                                        'Process.deleteInvt');
                                }
                            } else if (HQ.focus == 'customer') {
                                if (App.cboAccumulateID.getValue() && App.slmCustomer.getCount()) {
                                    var selRecs = App.grdCustomer.selModel.selected.items;
                                    if (selRecs.length > 0) {
                                        var params = [];
                                        var langCust = Process.findColumnNameByIndex(App.grdCustomer, 'CustID') + " - "
                                                    + Process.findColumnNameByIndex(App.grdCustomer, 'CpnyID') + "</br>";
                                        selRecs.forEach(function (record) {
                                            params.push(record.data.CustID + " - " + record.data.CpnyID);
                                        });
                                        HQ.message.show(2015020806,
                                            langCust + params.join(" & "),
                                            'deleteSelectedCustomer');
                                    }
                                }
                            } else if (HQ.focus == 'sales') {
                                if (App.cboAccumulateID.getValue() && App.slmSales.getCount()) {
                                    var selRecs = App.grdSales.selModel.selected.items;
                                    if (selRecs.length > 0) {
                                        var params = [];
                                        var langSales = Process.findColumnNameByIndex(App.grdSales, 'SlsperID') + " - "
                                                        + Process.findColumnNameByIndex(App.grdSales, 'CpnyID') + "</br>";
                                        selRecs.forEach(function (record) {
                                            params.push(record.data.SlsperID + " - " + record.data.CpnyID);
                                        });
                                        HQ.message.show(2015020806,
                                            langSales + params.join(" & "),
                                            'deleteSelectedSales');
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
        }
    },

    Grid: {
        slmLevel_selectChange: function (grid, selected, eOpts) {
            var store = App.grdInvt.store;
            var keys = store.HQFieldKeys ? store.HQFieldKeys : "";
            var accumulateID = App.cboAccumulateID.getValue();

            store.clearFilter();
            if (selected.length > 0) {
                store.filterBy(function (record) {
                    if (record.data.LevelID == selected[0].data.LevelID) {
                        return record;
                    }
                });

                if (selected[0].data.LevelID && App.cboStatus.getValue() == _beginStatus) {
                    if (Process.isAllValidKey(store.getRecordsValues(), keys)) {
                        var newData = {
                            AccumulateID: accumulateID,
                            LevelID: selected[0].data.LevelID
                        };

                        var newRec = Ext.create(store.model.modelName, newData);
                        HQ.store.insertRecord(store, keys, newRec, false);
                    }
                }
            }
            else {
                store.filterBy(function (record) {
                    // no data
                });
            }
        },

        grd_reject: function (col, record) {
            var grd = col.up('grid');
            //var keys = grd.store.HQFieldKeys ? grd.store.HQFieldKeys : "";
            //if (!record.data.tstamp && keys && keys.indexOf('LevelID')<0) {
            //    grd.getStore().remove(record, grd);
            //    grd.getView().focusRow(grd.getStore().getCount() - 1);
            //    grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            //} else {
            HQ.grid.checkReject(record, grd);
            //record.reject();
            Event.Form.frmMain_fieldChange();
        },
        grdCpnyID_beforeEdit: function (editor, e) {
            return false;

            if (HQ.isUpdate) {                
                if (App.frmMain.isValid()) {
                    var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
                    var status = App.cboStatus.getValue();
                    if (status == _beginStatus) {
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
                    Process.showFieldInvalid(App.frmMain);
                    return false;
                }
            }
            else {
                return false;
            }
        },
        grd_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var status = App.cboStatus.getValue();
                    if (status == _beginStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
                        return HQ.grid.checkBeforeEdit(e, keys);
                        //var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";                        
                        //if (keys.indexOf(e.field) != -1) {
                        //    if (e.record.data.tstamp)
                        //        return false;
                        //}
                        //return HQ.grid.checkInput(e, keys);
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
        grdCpnyID_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            var record = App.cboGCpnyID.findRecord("CpnyID", e.record.data.CpnyID);
            if (record)
                e.record.set("CpnyName", record.data.CpnyName);
            else
                e.record.set("CpnyName", '');
            HQ.grid.checkInsertKey(App.grdCompany, e, keys);
        },
        grdCustID_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            HQ.grid.checkInsertKey(App.grdCustomer, e, keys);
        },
        grdCustID_ValidateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            return HQ.grid.checkValidateEdit(App.grdCustomer, e, keys);
        },
        grd_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            if (e.store.storeId == 'stoLevel') {
                if (e.field == 'LevelDescr') {
                    var rec = Ext.create(e.store.model.modelName, {
                        LevelID: Process.lastNbr(e.store)
                    });
                    var record = HQ.store.findRecord(App.stoLevel, ['LevelDescr'], ['']);
                    if (!record) {
                        HQ.store.insertRecord(e.store, 'LevelID', rec, false);
                    }
                }
            } else {
                if (keys.indexOf(e.field) != -1) {

                    if (e.value != ''
                        && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                        && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                        if (keys.indexOf('LevelID') > -1) {
                            var rec = Ext.create(e.store.model.modelName, {
                                LevelID: Process.lastNbr(e.store)
                            });
                            HQ.store.insertRecord(e.store, keys, rec);
                        }
                        else {
                            if (e.record.data.LevelID) {
                                var rec = Ext.create(e.store.model.modelName, {
                                    LevelID: e.record.data.LevelID
                                });
                                HQ.store.insertRecord(e.store, keys, rec);
                            }
                            else {
                                HQ.store.insertBlank(e.store, keys);
                            }
                        }
                    }
                }
            }


            Event.Form.frmMain_fieldChange();
        },
        grdCpnyID_validateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            return HQ.grid.checkValidateEdit(App.grdCompany, e, keys);
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
        }
        ,
        grdLoc_reject: function (col, record) {
            HQ.grid.checkReject(record, App.grdLoc);
            Event.Form.frmMain_fieldChange();
        },
        grdLoc_beforeEdit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var status = App.cboStatus.getValue();
                    if (status == _beginStatus) {
                        return HQ.grid.checkBeforeEdit(e, keys);
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

        grdLoc_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            HQ.grid.checkInsertKey(App.grdLoc, e, keys);
            Event.Form.frmMain_fieldChange();
        },

        grdLoc_validateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            return HQ.grid.checkValidateEdit(App.grdLoc, e, keys);
        },

        grdSetup_reject: function (col, record) {
            HQ.grid.checkReject(record, App.grdSetup);
            Event.Form.frmMain_fieldChange();
        },
        grdSetup_beforeEdit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var status = App.cboStatus.getValue();
                    if (status == _beginStatus) {
                        return HQ.grid.checkBeforeEdit(e, keys);
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

        grdSetup_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            HQ.grid.checkInsertKey(App.grdSetup, e, keys);
            Event.Form.frmMain_fieldChange();
        },

        grdSetup_validateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            return HQ.grid.checkValidateEdit(App.grdSetup, e, keys);
        }

        , grdCustomer_BeforeEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            if (HQ.grid.checkBeforeEdit(e, keys))
                return false;
            if(e.field == "CustID")
                if(e.value != '')
                    return false;
        }

        , grdSales_BeforeEdit: function (item, e) {
            return false;
        }

        , rendererLevelType: function (value) {
            var r = HQ.store.findInStore(App.cboLevelType.store, ['Code'], [value]);
            if (Ext.isEmpty(r)) {
                return value;
            }
            return r.Descr;
        }
    },

    Tree: {
        treePanelBranch_checkChange: function (node, checked, eOpts) {
            if (App.cboStatus.getValue() == _beginStatus) {
                node.childNodes.forEach(function (childNode) {
                    childNode.set("checked", checked);
                });
            } else {
                App.treePanelBranch.clearChecked();
            }
        },

        btnAddAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var accumulateID = App.cboAccumulateID.getValue();
                    var status = App.cboStatus.value;

                    if (accumulateID && status == _beginStatus) {
                        var allNodes = Process.getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
                        if (allNodes && allNodes.length > 0) {
                            App.stoCompany.suspendEvents();
                            allNodes.forEach(function (node) {
                                if (node.data.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['CpnyID'],
                                        [node.data.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            CpnyID: node.data.RecID,
                                            CpnyName: node.data.text
                                        }));
                                    }
                                }
                            });
                            App.stoCompany.resumeEvents();
                            App.grdCompany.view.refresh();
                            App.grdCompany.store.loadPage(1);
                            App.treePanelBranch.clearChecked();

                            Event.Form.frmMain_fieldChange();
                        }
                    }
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAdd_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var accumulateID = App.cboAccumulateID.getValue();
                    var status = App.cboStatus.value;

                    if (accumulateID && status == _beginStatus) {
                        var allNodes = App.treePanelBranch.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.stoCompany.suspendEvents();
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['CpnyID'],
                                        [node.attributes.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            CpnyID: node.attributes.RecID,
                                            CpnyName: node.text
                                        }));
                                    }
                                }
                            });
                            App.stoCompany.resumeEvents();
                            App.grdCompany.view.refresh();
                            App.grdCompany.store.loadPage(1);
                            App.treePanelBranch.clearChecked();
                            Event.Form.frmMain_fieldChange();
                        }
                    }
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDel_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    HQ.focus = 'cpny';
                    Event.Form.menuClick('delete');
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _beginStatus) {
                        HQ.message.show(11, '', 'Process.deleteAllCompanies');
                    }
                }
                else {
                    Process.showFieldInvalid(App.frmMain);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        }        
    }
};


///////////////////////////////////////// TREE INVT VIEW ////////////////////////////////////
var beforenodedrop = function (node, data, overModel, dropPosition, dropFn) {
    if (Ext.isArray(data.records)) {
        var records = data.records;
        data.records = [];

        App.stoSetup.suspendEvents();
        addNode(records[0]);
        App.stoSetup.resumeEvents();
        //App.stoSetup.loadPage(1);
        App.grdSetup.view.refresh();
    }
};

var treePanelInvt_checkChange = function (node, checked) {
    if (App.cboStatus.getValue() == _beginStatus) {
        if (node.hasChildNodes()) {
            node.eachChild(function (childNode) {
                childNode.set('checked', checked);
                treePanelInvt_checkChange(childNode, checked);
            });
        }
    } else {
        App.treePanelInvt.clearChecked();
    }
};

var btnInvtExpand_click = function (btn, e, eOpts) {
    App.treePanelInvt.getStore().suspendEvents();
    App.treePanelInvt.expandAll();
    App.treePanelInvt.getStore().resumeEvents();
};

var btnInvtCollapse_click = function (btn, e, eOpts) {
    App.treePanelInvt.collapseAll();
};

var addNode = function (node) {
    if (node.data.Type == "Invt") {
        var record = HQ.store.findInStore(App.grdSetup.store, ['InvtID'], [node.data.InvtID]);
        if (!record) {
            HQ.store.insertBlank(App.stoOM21700, 'InvtID');
            record = App.stoOM21700.getAt(App.grdOM21700.store.getCount() - 1);
            record.set('InvtID', node.data.InvtID);
            record.set('Descr', node.data.Descr);
            record.set('Qty', node.data.CnvFact);
        }
        var record = App.stoSetup.getAt(App.stoSetup.getCount() - 1);
    }
    else if (node.childNodes) {
        node.childNodes.forEach(function (itm) {
            addNode(itm);
        });
    }
    Event.Form.frmMain_fieldChange();//frmChange();
};

var btnAddAllInvt_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var accumulateID = App.cboAccumulateID.getValue();
            var status = App.cboStatus.value;

            if (accumulateID && status == _beginStatus) {
                if (HQ.isUpdate) {
                    var allNodes = getLeafNodes(App.treePanelInvt.getRootNode());
                    if (allNodes && allNodes.length > 0) {
                        App.stoSetup.suspendEvents();
                        allNodes.forEach(function (node) {
                            if (node.data.Type == "Invt") {
                                var record = HQ.store.findInStore(App.stoSetup, ['InvtID'], [node.data.InvtID]); // App.grdOM21700.store
                                if (!record) {
                                    HQ.store.insertBlank(App.stoSetup, ['InvtID']);
                                    record = App.stoSetup.getAt(App.grdSetup.store.getCount() - 1);
                                    record.set('InvtID', node.data.InvtID);
                                    record.set('Descr', node.data.Descr);
                                    record.set('Qty', node.data.CnvFact);
                                }
                            }
                        });
                        App.stoSetup.resumeEvents();
                        // App.stoSetup.loadPage(1);
                        App.grdSetup.view.refresh();

                        var record = App.stoSetup.getAt(App.stoSetup.getCount() - 1);
                        App.treePanelInvt.clearChecked();
                        Event.Form.frmMain_fieldChange();
                    }
                }
                else {
                    HQ.message.show(4, '', '');
                }
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnAddInvt_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var accumulateID = App.cboAccumulateID.getValue();
            var status = App.cboStatus.value;

            if (accumulateID && status == _beginStatus) {
                var allNodes = App.treePanelInvt.getCheckedNodes();
                if (allNodes && allNodes.length > 0) {
                    App.stoSetup.suspendEvents();
                    allNodes.forEach(function (node) {
                        if (node.attributes.Type == "Invt") {
                            var record = HQ.store.findInStore(App.stoSetup, ['InvtID'], [node.attributes.InvtID]);
                            if (!record) {
                                HQ.store.insertBlank(App.stoSetup, ['InvtID']);
                                record = App.stoSetup.getAt(App.grdSetup.store.getCount() - 1);
                                record.set('InvtID', node.attributes.InvtID);
                                record.set('Descr', node.attributes.Descr);
                                record.set('Qty', node.attributes.CnvFact);
                            }
                        }
                    });
                    App.stoSetup.resumeEvents();
                    //  App.stoSetup.loadPage(1);
                    App.grdSetup.view.refresh();

                    App.treePanelInvt.clearChecked();
                }
                Event.Form.frmMain_fieldChange();
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDelInvt_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var status = App.cboStatus.value;

            if (status == _beginStatus) {
                var selRecs = App.grdSetup.selModel.selected.items;
                if (selRecs.length > 0) {
                    var params = [];
                    selRecs.forEach(function (record) {
                        params.push(record.data.InvtID);
                    });
                    HQ.message.show(2015020806,
                        params.join(" & ") + ",",
                        'deleteSelectedInvt');
                }
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDelAllInvt_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var status = App.cboStatus.value;
            if (status == _beginStatus) {
                HQ.message.show(2015020806, '', 'deleteAllInvts');
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var getDeepAllLeafNodes = function (node, onlyLeaf) {
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
};

var getLeafNodes = function (node) {
    var childNodes = [];
    node.eachChild(function (child) {
        if (child.isLeaf()) {
            childNodes.push(child);
        }
        else {
            var children = getLeafNodes(child);
            if (children.length) {
                children.forEach(function (nill) {
                    childNodes.push(nill);
                });
            }
        }
    });
    return childNodes;
};

var deleteSelectedLoc = function (item) {
    if (item == "yes") {
        App.grdLoc.deleteSelected();
        Event.Form.frmMain_fieldChange();
    }
};

var deleteSelectedInvt = function (item) {
    if (item == "yes") {
        App.grdSetup.deleteSelected();
        Event.Form.frmMain_fieldChange();
    }
};

var deleteAllInvts = function (item) {
    if (item == "yes") {
        App.stoSetup.removeAll();
        Event.Form.frmMain_fieldChange();
    }
};

/// Tree Customer
var treePanelCustomer_checkChange = function (node, checked) {
    if (App.cboStatus.getValue() == _beginStatus) {
        if (node.hasChildNodes()) {
            node.eachChild(function (childNode) {
                childNode.set('checked', checked);
                treePanelCustomer_checkChange(childNode, checked);
            });
        }
    } else {
        App.treePanelCustomer.clearChecked();
    }
}

var btnCustomerAddAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var accumulateID = App.cboAccumulateID.getValue();

            if (accumulateID && App.cboStatus.value == _beginStatus) {
                if (HQ.isUpdate) {
                    HQ.common.showBusy(true);
                    var allNodes = getLeafNodes(App.treePanelCustomer.getRootNode());
                    if (allNodes && allNodes.length > 0) {                        
                        App.stoCustomer.suspendEvents();
                        allNodes.forEach(function (node) {
                            if (node.data.Type == "customer") {
                                var record = HQ.store.findInStore(App.stoCustomer, ['CustID', 'CpnyID'], [node.data.CustID, node.data.CpnyID]); // App.grdOM21700.store
                                if (!record) {
                                    HQ.store.insertBlank(App.stoCustomer, ['CustID', 'CpnyID']);
                                    record = App.stoCustomer.getAt(App.stoCustomer.getCount() - 1);
                                    record.set('CustID', node.data.CustID);
                                    record.set('CustName', node.data.CustName);
                                    record.set('CpnyID', node.data.CpnyID);
                                }
                            }
                        });
                        App.treePanelCustomer.clearChecked();
                        App.stoCustomer.resumeEvents();
                        App.grdCustomer.view.refresh();
                        App.grdCustomer.store.loadPage(1);
                       // var record = App.stoCustomer.getAt(App.stoCustomer.getCount() - 1);
                        
                        Event.Form.frmMain_fieldChange();
                    }
                    HQ.common.showBusy(false);
                }
                else {
                    HQ.message.show(4, '', '');
                }
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnCustomerAdd_click = function () {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var accumulateID = App.cboAccumulateID.getValue();
            var status = App.cboStatus.value;

            if (accumulateID && status == _beginStatus) {
                var allNodes = App.treePanelCustomer.getCheckedNodes();
                if (allNodes && allNodes.length > 0) {
                    App.stoCustomer.suspendEvents();
                    allNodes.forEach(function (node) {
                        if (node.attributes.Type == "customer") {
                            var record = HQ.store.findInStore(App.stoCustomer, ['CustID', 'CpnyID'], [node.attributes.CustID, node.attributes.CpnyID]);
                            if (!record) {
                                HQ.store.insertBlank(App.stoCustomer, ['CustID', 'CpnyID']);
                                record = App.stoCustomer.getAt(App.stoCustomer.getCount() - 1);
                                record.set('CustID', node.attributes.CustID);
                                record.set('CustName', node.attributes.CustName);
                                record.set('CpnyID', node.attributes.CpnyID);
                            }
                        }
                    });
                    App.treePanelCustomer.clearChecked();
                    App.stoCustomer.resumeEvents();                    
                    App.grdCustomer.view.refresh();
                    App.stoCustomer.loadPage(1);
                }
                Event.Form.frmMain_fieldChange();
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
}

var btnCustomerDel_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {            
            HQ.focus = 'customer';
            Event.Form.menuClick('delete');
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnCustomerDelAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var status = App.cboStatus.value;
            if (status == _beginStatus) {
                HQ.message.show(2015020806, '', 'deleteAllCustomer');
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var deleteSelectedCustomer = function (item) {
    if (item == "yes") {
        App.grdCustomer.deleteSelected();
        Event.Form.frmMain_fieldChange();
    }
};

var deleteAllCustomer = function (item) {
    if (item == "yes") {
        App.stoCustomer.loadData([], false);
        App.stoCustomer.submitData();
        App.grdCustomer.view.refresh();
        App.stoCustomer.loadPage(1);

        Event.Form.frmMain_fieldChange();
    }
};

/// Tree Sales
var treePanelSales_checkChange = function (node, checked) {
    if (App.cboStatus.getValue() == _beginStatus) {
        if (node.hasChildNodes()) {
            node.eachChild(function (childNode) {
                childNode.set('checked', checked);
                treePanelSales_checkChange(childNode, checked);
            });
        }
    } else {
        App.treePanelSales.clearChecked();
    }
}

var btnSalesAddAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var accumulateID = App.cboAccumulateID.getValue();
            var status = App.cboStatus.value;

            if (accumulateID && status == _beginStatus) {
                if (HQ.isUpdate) {
                    var allNodes = getLeafNodes(App.treePanelSales.getRootNode());
                    if (allNodes && allNodes.length > 0) {
                        App.stoSales.suspendEvents();
                        allNodes.forEach(function (node) {
                            if (node.data.Type == "sales") {
                                var record = HQ.store.findInStore(App.stoSales, ['SlsperID', 'CpnyID'], [node.data.SlsperID, node.data.CpnyID]); // App.grdOM21700.store
                                if (!record) {
                                    HQ.store.insertBlank(App.stoSales, ['SlsperID', 'CpnyID']);
                                    record = App.stoSales.getAt(App.stoSales.getCount() - 1);
                                    record.set('SlsperID', node.data.SlsperID);
                                    record.set('SlsName', node.data.SlsName);
                                    record.set('CpnyID', node.data.CpnyID);
                                }
                            }
                        });
                        App.treePanelSales.clearChecked();
                        App.stoSales.resumeEvents();
                        App.grdSales.view.refresh();
                        App.stoSales.loadPage(1);
                        // var record = App.stoCustomer.getAt(App.stoCustomer.getCount() - 1);

                        Event.Form.frmMain_fieldChange();
                    }
                }
                else {
                    HQ.message.show(4, '', '');
                }
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnSalesDel_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            HQ.focus = 'sales';
            Event.Form.menuClick('delete');
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnSalesAdd_click = function () {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var accumulateID = App.cboAccumulateID.getValue();
            var status = App.cboStatus.value;

            if (accumulateID && status == _beginStatus) {
                var allNodes = App.treePanelSales.getCheckedNodes();
                if (allNodes && allNodes.length > 0) {
                    App.stoSales.suspendEvents();
                    allNodes.forEach(function (node) {
                        if (node.attributes.Type == "sales") {
                            var record = HQ.store.findInStore(App.stoSales, ['SlsperID', 'CpnyID'], [node.attributes.SlsperID, node.attributes.CpnyID]);
                            if (!record) {
                                HQ.store.insertBlank(App.stoSales, ['SlsperID', 'CpnyID']);
                                record = App.stoSales.getAt(App.stoSales.getCount() - 1);
                                record.set('SlsperID', node.attributes.SlsperID);
                                record.set('SlsName', node.attributes.SlsName);
                                record.set('CpnyID', node.attributes.CpnyID);
                            }
                        }
                    });
                    App.treePanelSales.clearChecked();
                    App.stoSales.resumeEvents();
                    App.grdSales.view.refresh();
                    App.stoSales.loadPage(1);
                }
                Event.Form.frmMain_fieldChange();
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
}

var btnSalesDelAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var status = App.cboStatus.value;
            if (status == _beginStatus) {
                HQ.message.show(2015020806, '', 'deleteAllSales');
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var deleteSelectedSales = function (item) {
    if (item == "yes") {
        App.grdSales.deleteSelected();
        Event.Form.frmMain_fieldChange();
    }
};

var deleteAllSales = function (item) {
    if (item == "yes") {
        App.stoSales.loadData([], false);
        App.stoSales.submitData();
        App.grdSales.view.refresh();
        App.stoSales.loadPage(1);
        Event.Form.frmMain_fieldChange();
    }
};
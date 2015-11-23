// Declare
var _beginStatus = "H";


// Processing Function
var Process = {
    displayImage: function (imgControl, fileName) {
        Ext.Ajax.request({
            url: 'AR20200/ImageToBin',
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            params: JSON.stringify({
                fileName: fileName
            }),
            success: function (result) {
                var jsonData = Ext.decode(result.responseText);
                if (jsonData.imgSrc) {
                    imgControl.setImageUrl(jsonData.imgSrc);
                }
                else {
                    imgControl.setImageUrl("");
                }
            },
            failure: function (errorMsg, data) {
                HQ.message.process(errorMsg, data, true);
            }
        });
    },

    readImage: function (fup, imgControl) {
        var files = fup.fileInputEl.dom.files;
        if (files && files[0]) {
            var FR = new FileReader();
            FR.onload = function (e) {
                imgControl.setImageUrl(e.target.result);
            };
            FR.readAsDataURL(files[0]);
        }
    },

    saveData: function () {
        if (App.frmMain.isValid()) {
            var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);
            App.frmMain.updateRecord();

            App.frmMain.submit({
                url: 'AR20200/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstSalesPerson: Ext.encode(App.stoSalesPerson.getRecordsValues()),
                    lstSlsperCpnyAddr: HQ.store.getData(App.grdSlsperCpnyAddr.store),
                    channel: selRec ? selRec.Channel : "",
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboSlsperid.store.load(function () {
                        App.cboSlsperid.setValue(data.result.slsperID);
                        App.stoSalesPerson.reload();

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
    },

    deleteData: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'AR20200/Delete',
                clientValidation: false,
                waitMsg: HQ.common.getLang('Deleting') + "...",
                timeout: 1800000,
                params: {
                    branchID: App.cboBranchID.getValue(),
                    slsperID: App.cboSlsperid.getValue(),
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboSlsperid.store.load();
                    App.cboSlsperid.clearValue();
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

    deleteSlsperCpnyAddr: function (item) {
        if (item == "yes") {
            App.grdSlsperCpnyAddr.deleteSelected();
        }
    },

    deleteAllCpnyAddrs: function (item) {
        if (item == "yes") {
            App.grdSlsperCpnyAddr.store.removeAll();
        }
    },

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.Form.menuClick("refresh");

        }
    },

    renderAddr: function (value, metaData, record, rowIndex, colIndex, store) {
        var rec = App.cboCpnyAddrID.store.findRecord("AddrID", record.data.CpnyAddrID);
        var returnValue = value;
        if (rec) {
            if (metaData.column.dataIndex == "Addr1" && !record.data.Addr1) {
                returnValue = rec.data.Addr1;
            }
            else if (metaData.column.dataIndex == "Addr2" && !record.data.Addr2) {
                returnValue = rec.data.Addr2;
            }
        }

        return returnValue;
    },

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
                if (field.id == "txtEMailAddr") {
                    HQ.message.show(999, '', 'Process.focusOnInvalidField');
                }
                else {
                    HQ.message.show(15, field.fieldLabel, 'Process.focusOnInvalidField');
                }
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
    }
};

// Store Event
var Store = {
    stoSalesPerson_load: function (sto, records, successful, eOpts) {
        //App.cboState.forceSelection = false;
        //App.cboDistrict.forceSelection = false;
        HQ.common.setForceSelection(App.frmMain, false, "cboBranchID,cboSlsperid")

        HQ.isNew = false;
        if (sto.getCount() == 0) {
            var newSlsper = Ext.create("App.mdlAR_Salesperson", {
                SlsperId: App.cboSlsperid.getValue(),
                BranchID: App.cboBranchID.getValue(),
                Active: true,
                Status: _beginStatus
            });
            sto.insert(0, newSlsper);
            sto.commitChanges();
            HQ.isNew = true;
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);

        // display image
        App.fupImages.reset();
        if (frmRecord.data.Images) {
            Process.displayImage(App.imgImages, frmRecord.data.Images);
        }
        else {
            App.imgImages.setImageUrl("");
        }

        var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);

        if (selRec && selRec.Channel == "MT") {
            App.pnlSlsperCpnyAddr.show();
            App.grdSlsperCpnyAddr.store.reload();
        }
        else {
            App.pnlSlsperCpnyAddr.hide();
            App.grdSlsperCpnyAddr.store.removeAll();
        }
        var isLock=frmRecord.data.Status != _beginStatus ? true : false;
        HQ.common.lockItem(App.frmMain, isLock);
        if (isLock) {
            App.btnClearImage.disable();
            App.fupImages.disable()
        } else {
            App.btnClearImage.enable();
            App.fupImages.enable()
        }
        
     
        App.fupImages.setReadOnly(isLock);
        Event.Form.frmMain_fieldChange();
    },

    stoSlsperCpnyAddr_load: function (sto, records, successful, eOpts) {
        //if (HQ.isUpdate) {
        //    var branchID = App.cboBranchID.getValue();
        //    var slperID = App.cboSlsperid.getValue();
        //    var status = App.cboStatus.getValue();
        //    var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

        //    if (branchID && slperID && status == _beginStatus) {
        //        if (successful) {
        //            var newData = {
        //                SlsperID: slperID,
        //                BranchID: branchID
        //            };

        //            var newRec = Ext.create(sto.model.modelName, newData);
        //            HQ.store.insertRecord(sto, keys, newRec, false);
        //        }
        //    }
        //}
    },
    stoAR20200_pdCheckAutoSales_load: function (sto, records, successful, eOpts) {
        if (sto.data.items[0].data.Result == "1") {
            App.cboSlsperid.allowBlank = true;
            App.cboSlsperid.forceSelection = true;
        }
        else {
            App.cboSlsperid.allowBlank = false;
            App.cboSlsperid.forceSelection = false;
        }
    }
};

// Form Event
var Event = {
    Form: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            HQ.common.setRequire(frm);
            App.cboBranchID.store.load(function (records, operation, success) {
                App.cboBranchID.setValue(HQ.cpnyID);
            });
        },

        frmMain_fieldChange: function () {
            if (App.stoSalesPerson.getCount() > 0) {
                App.frmMain.updateRecord();
                if (!HQ.store.isChange(App.stoSalesPerson)) {
                    HQ.isChange = HQ.store.isChange(App.grdSlsperCpnyAddr.store);
                }
                else {
                    HQ.isChange = true;
                }

                HQ.common.changeData(HQ.isChange, 'AR20200');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                App.cboBranchID.setReadOnly(HQ.isChange);
                App.cboSlsperid.setReadOnly(HQ.isChange);
            }
        },

        cboBranchID_change: function (cbo, newValue, oldValue, eOpts) {
            //if (!newValue) {
            App.cboSlsperid.clearValue();
            //}
            App.cboSlsperid.store.load(function (records, operation, success) {
                if (records.length > 0) {
                    App.cboSlsperid.setValue(records[0].data.SlsperId);
                }
                //else {
                //    App.cboSlsperid.clearValue();
                //}
            });
            App.cboDeliveryMan.store.reload();
            App.cboSupID.store.reload();
            App.cboVendID.store.reload();
            App.stoAR20200_pdCheckAutoSales.reload();
            //App.cboCpnyAddrID.store.reload();
        },

        cboSlsperid_change: function (cbo, newValue, oldValue, eOpts) {
            App.stoSalesPerson.reload();
        },

        cboStatus_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboHandle.store.reload();
        },

        cboCountryId_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboState.store.load(function (records, operation, success) {
                var formRecord = App.frmMain.getRecord();
                if (formRecord) {
                    App.cboState.setValue(formRecord.data.State);
                }
            });
        },

        cboState_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboDistrict.store.load(function (records, operation, success) {
                var formRecord = App.frmMain.getRecord();
                if (formRecord) {
                    App.cboDistrict.setValue(formRecord.data.District);
                }
            });
        },

        fupImages_change: function (fup, newValue, oldValue, eOpts) {
            if (fup.value) {
                var ext = fup.value.split(".").pop().toLowerCase();
                if (ext == "jpg" || ext == "png" || ext == "gif") {
                    App.hdnImages.setValue(fup.value);
                    Process.readImage(fup, App.imgImages);
                }
                else {
                    HQ.message.show(148, '', '');
                }
            }
        },

        btnClearImage_click: function (btn, eOpts) {
            App.fupImages.reset();
            App.imgImages.setImageUrl("");
            App.hdnImages.setValue("");
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'header') {
                        HQ.combo.first(App.cboSlsperid, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.first(App.grdSlsperCpnyAddr);
                    }
                    break;

                case "next":         
                    if (HQ.focus == 'header') {
                        HQ.combo.next(App.cboSlsperid, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.next(App.grdSlsperCpnyAddr);
                    }
                    break;

                case "prev":
                    if (HQ.focus == 'header') {
                        HQ.combo.prev(App.cboSlsperid, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.prev(App.grdSlsperCpnyAddr);
                    }
                    break;

                case "last":
                    if (HQ.focus == 'header') {
                        HQ.combo.last(App.cboSlsperid, HQ.isChange);
                    }
                    else if (HQ.focus == 'grid') {
                        HQ.grid.last(App.grdSlsperCpnyAddr);
                    }
                    break;

                case "save":
                    if (HQ.isInsert || HQ.isUpdate) {
                        Process.saveData();
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;

                case "delete":
                    if (HQ.isDelete) {
                        if (HQ.focus == 'header') {
                            if (App.cboSlsperid.getValue() && App.cboStatus.getValue() == _beginStatus) {
                                HQ.message.show(11, '', 'Process.deleteData');
                            }
                            else {
                                HQ.message.show(20140306, '', '');
                            }
                        }
                        else if (HQ.focus == 'grid') {
                            var selected = App.grdSlsperCpnyAddr.getSelectionModel().selected.items;
                            if (selected.length > 0) {
                                if (selected[0].index != undefined) {
                                    var params = selected[0].index + 1 + ',' + App.pnlSlsperCpnyAddr.title;
                                    HQ.message.show(2015020807, params, 'Process.deleteSlsperCpnyAddr');
                                }
                                else {
                                    HQ.message.show(11, '', 'Process.deleteSlsperCpnyAddr');
                                }
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;

                case "close":
                    HQ.common.close(this);
                    break;

                case "new":
                    if (HQ.isInsert) {
                        if (HQ.isChange) {
                            HQ.message.show(150, '', '');
                        }
                        else {    
                            App.cboSlsperid.clearValue();
                        }
                    }
                    break;

                case "refresh":
                    if (HQ.isChange) {
                        HQ.message.show(20150303, '', 'Process.refresh');
                    }
                    else {
                        if (HQ.focus == 'header') {
                            App.cboSlsperid.store.load(function () {
                                App.stoSalesPerson.reload();
                            });
                        }
                        else if (HQ.focus == 'grid') {
                            App.grdSlsperCpnyAddr.store.reload();
                        }
                    }
                    break;
                default:
            }
        }
    },

    Grid: {
        grdSlsperCpnyAddr_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _beginStatus) {
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
                    Process.showFieldInvalid(App.frmMain);
                    return false;
                }
            }
            else {
                return false;
            }
        },

        grdSlsperCpnyAddr_edit: function (editor, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    var branchID = App.cboBranchID.getValue();
                    var slperID = App.cboSlsperid.getValue();
                    var status = App.cboStatus.getValue();
                    var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                    if (branchID && slperID && status == _beginStatus) {
                        var newData = {
                            SlsperID: slperID,
                            BranchID: branchID
                        };

                        var newRec = Ext.create(e.store.model.modelName, newData);
                        HQ.store.insertRecord(e.store, keys, newRec, false);
                    }
                }
            }
        },

        grdSlsperCpnyAddr_validateEdit: function (editor, e) {
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

        grdSlsperCpnyAddr_reject: function (col, record) {
            var store = record.store;

            if (record.data.tstamp == '') {
                store.remove(record);
                col.grid.getView().focusRow(store.getCount() - 1);
                col.grid.getSelectionModel().select(store.getCount() - 1);
            } else {
                record.reject();
            }
        }
    },

    Tree: {
        treePanelCpnyAddr_checkChange: function (node, checked, eOpts) {
            node.childNodes.forEach(function (childNode) {
                childNode.set("checked", checked);
            });
        },

        btnExpand_click: function (btn, e, eOpts) {
            App.treePanelCpnyAddr.expandAll();
        },

        btnCollapse_click: function (btn, e, eOpts) {
            App.treePanelCpnyAddr.collapseAll();
        },

        btnAddAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmMain.isValid()) {
                    var branchID = App.cboBranchID.getValue();
                    var slperID = App.cboSlsperid.getValue();
                    //var status = App.cboStatus.getValue();

                    if (branchID && slperID) {
                        var allNodes = Process.getDeepAllLeafNodes(App.treePanelCpnyAddr.getRootNode(), true);
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.raw.Type == "Addr") {
                                    var idx = App.grdSlsperCpnyAddr.store.getCount();
                                    var record = HQ.store.findInStore(App.grdSlsperCpnyAddr.store,
                                        ['CpnyAddrID'],
                                        [node.raw.RecID]);
                                    if (!record) {
                                        App.grdSlsperCpnyAddr.store.insert(idx, Ext.create("App.mdlSlsperCpnyAddr", {
                                            CpnyAddrID: node.raw.RecID,
                                            Addr1: node.raw.Addr1,
                                            Name: node.raw.AddrName
                                        }));
                                    }
                                }
                            });
                            App.treePanelCpnyAddr.clearChecked();
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
                    var branchID = App.cboBranchID.getValue();
                    var slperID = App.cboSlsperid.getValue();
                    //var status = App.cboStatus.getValue();

                    if (branchID && slperID) {
                        var allNodes = App.treePanelCpnyAddr.getChecked();
                        if (allNodes && allNodes.length > 0) {
                            allNodes.forEach(function (node) {
                                if (node.raw.Type == "Addr") {
                                    var idx = App.grdSlsperCpnyAddr.store.getCount();
                                    var record = HQ.store.findInStore(App.grdSlsperCpnyAddr.store,
                                        ['CpnyAddrID'],
                                        [node.raw.RecID]);
                                    if (!record) {
                                        App.grdSlsperCpnyAddr.store.insert(idx, Ext.create("App.mdlSlsperCpnyAddr", {
                                            CpnyAddrID: node.raw.RecID,
                                            Addr1: node.raw.Addr1,
                                            Name: node.raw.AddrName
                                        }));
                                    }
                                }
                            });
                            App.treePanelCpnyAddr.clearChecked();
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
                    //var status = App.cboStatus.value;

                    //if (status == _beginStatus) {
                        var selRecs = App.grdSlsperCpnyAddr.selModel.selected.items;
                        if (selRecs.length > 0) {
                            var params = [];
                            selRecs.forEach(function (record) {
                                params.push(record.data.CpnyAddrID);
                            });
                            HQ.message.show(2015020806,
                                params.join(" & ") + "," + HQ.common.getLang("SlsperCpnyAddr"),
                                'Process.deleteSlsperCpnyAddr');
                        }
                    //}
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
                    //var status = App.cboStatus.value;
                    //if (status == _beginStatus) {
                        HQ.message.show(11, '', 'Process.deleteAllCpnyAddrs');
                    //}
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
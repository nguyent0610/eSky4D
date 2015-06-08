// Declare
var _beginStatus = "H";


// Processing Function
var Process = {
    displayImage: function (imgControl, fileName) {
        Ext.Ajax.request({
            url: 'OM00000/ImageToBin',
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
            App.frmMain.updateRecord();

            App.frmMain.submit({
                url: 'OM00000/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstSalesPerson: Ext.encode(App.stoSalesPerson.getRecordsValues()),
                    lstSlsperCpnyAddr: HQ.store.getData(App.grdSlsperCpnyAddr.store),
                    channel: App.cboBranchID.valueModels.length > 0 ? App.cboBranchID.valueModels[0].data.Channel : "",
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.cboSlsperid.store.load(function () {
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
    },

    deleteData: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM00000/Delete',
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

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.menuClick("refresh");

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

        if (App.cboBranchID.valueModels.length > 0) {
            var selRec = App.cboBranchID.valueModels[0];
            if (selRec.data.Channel == "MT") {
                App.pnlSlsperCpnyAddr.show();
                App.grdSlsperCpnyAddr.store.reload();
            }
            else {
                App.pnlSlsperCpnyAddr.hide();
                App.grdSlsperCpnyAddr.store.removeAll();
            }
        }

        Event.frmMain_fieldChange();
    },

    stoSlsperCpnyAddr_load: function (sto, records, successful, eOpts) {
        if (HQ.isUpdate) {
            var branchID = App.cboBranchID.getValue();
            var slperID = App.cboSlsperid.getValue();
            var status = App.cboStatus.getValue();
            var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";

            if (branchID && slperID && status == _beginStatus) {
                if (successful) {
                    var newData = {
                        SlsperID: slperID,
                        BranchID: branchID
                    };

                    var newRec = Ext.create(sto.model.modelName, newData);
                    HQ.store.insertRecord(sto, keys, newRec, false);
                }
            }
        }
    }
};

// Form Event
var Event = {
    frmMain_boxReady: function (frm, width, height, eOpts) {
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

            HQ.common.changeData(HQ.isChange, 'OM00000');//co thay doi du lieu gan * tren tab title header
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
        App.cboCpnyAddrID.store.reload();
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
    },

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
};
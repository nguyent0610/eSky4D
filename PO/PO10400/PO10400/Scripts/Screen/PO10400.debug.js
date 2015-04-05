// Declare
var _beginStatus = "H";


// Processing Function
var Process = {
    displayImage: function (imgControl, fileName) {
        Ext.Ajax.request({
            url: 'PO10400/ImageToBin',
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
                url: 'PO10400/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    branchID: App.cboBranchID.getValue(),
                    slsperID: App.cboSlsperid.getValue(),
                    lstSalesPerson: Ext.encode(App.stoSalesPerson.getRecordsValues()),
                    isNew: HQ.isNew
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    Process.refresh("yes");
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
                url: 'PO10400/Delete',
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

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.menuClick("refresh");

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

        Event.frmMain_fieldChange();
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
            HQ.isChange = HQ.store.isChange(App.stoSalesPerson);
            HQ.common.changeData(HQ.isChange, 'PO10400');//co thay doi du lieu gan * tren tab title header
            //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
            App.cboBranchID.setReadOnly(HQ.isChange);
            App.cboSlsperid.setReadOnly(HQ.isChange);
        }
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
                HQ.combo.first(App.cboSlsperid, HQ.isChange);
                break;

            case "next":
                HQ.combo.next(App.cboSlsperid, HQ.isChange);
                break;

            case "prev":
                HQ.combo.prev(App.cboSlsperid, HQ.isChange);
                break;

            case "last":
                HQ.combo.last(App.cboSlsperid, HQ.isChange);
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
                    if (App.cboSlsperid.getValue() && App.cboStatus.getValue() == _beginStatus) {
                        HQ.message.show(11, '', 'Process.deleteData');
                    }
                    else {
                        HQ.message.show(20140306, '', '');
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
                    App.cboSlsperid.store.load(function () {
                        App.stoSalesPerson.reload();
                    });
                }

                break;
            default:
        }
    }
};
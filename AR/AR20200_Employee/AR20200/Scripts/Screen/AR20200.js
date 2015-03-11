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
            App.frmMain.updateRecord();

            App.frmMain.submit({
                url: 'AR20200/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    branchID: App.cboBranchID.getValue(),
                    slsperID: App.cboSlsperid.getValue(),
                    lstSalesPerson: Ext.encode(App.stoSalesPerson.getRecordsValues())
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    Event.menuClick("refresh");
                    App.cboSlsperid.store.load();
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

    deleteData: function () {
        App.frmMain.submit({
            url: 'AR20200/Delete',
            clientValidation: false,
            waitMsg: HQ.common.getLang('Deleting') + "...",
            timeout: 1800000,
            params: {
                branchID: App.cboBranchID.getValue(),
                slsperID: App.cboSlsperid.getValue()
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
    },

    // Check the store of data is change or not
    storeIsChange: function (store, isCreate) {
        if (isCreate == undefined) isCreate = true;
        if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
            || store.getChangedData().Updated != undefined
            || store.getChangedData().Deleted != undefined) {
            return true;
        }
        return false;
    },

    closeScreen: function (item) {
        if (item == "no") {
            this.parentAutoLoadControl.close()
        }
    }
};

// Store Event
var Store = {
    stoSalesPerson_load: function (sto, records, successful, eOpts) {
        if (sto.getCount() == 0) {
            var newSlsper = Ext.create("App.mdlAR_Salesperson", {
                SlsperId: App.cboSlsperid.getValue(),
                BranchID: App.cboBranchID.getValue(),
                Active: true,
                Status: _beginStatus
            });
            sto.insert(0, newSlsper);
        }
        var frmRecord = sto.getAt(0);
        App.frmMain.loadRecord(frmRecord);

        // display image
        if (frmRecord.data.Images) {
            Process.displayImage(App.imgImages, frmRecord.data.Images);
        }
        else {
            App.imgImages.setImageUrl("");
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

    cboBranchID_change: function (cbo, newValue, oldValue, eOpts) {
        if (!newValue) {
            App.cboSlsperid.clearValue();
        }
        App.cboSlsperid.store.load(function (records, operation, success) {
            if (records.length > 0) {
                App.cboSlsperid.setValue(records[0].data.SlsperId);
            }
            else {
                App.cboSlsperid.clearValue();
            }
        });
        App.cboDeliveryMan.store.reload();
        App.cboSupID.store.reload();
        App.cboVendID.store.reload();
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
        var curRecord = HQ.store.findRecord(App.cboSlsperid.store, ["SlsperId"], [App.cboSlsperid.getValue()]);

        switch (command) {
            case "first":
                var firstRec = App.cboSlsperid.store.getAt(0);
                if (firstRec) {
                    App.cboSlsperid.setValue(firstRec.data.SlsperId);
                }
                break;

            case "next":
                if (curRecord) {
                    var nextIdx = curRecord.index + 1;
                    var nextRec = App.cboSlsperid.store.getAt(nextIdx);
                    if (nextRec) {
                        App.cboSlsperid.setValue(nextRec.data.SlsperId);
                    }
                }
                break;

            case "prev":
                if (curRecord) {
                    var prevIdx = curRecord.index - 1;
                    var prevRec = App.cboSlsperid.store.getAt(prevIdx);
                    if (prevRec) {
                        App.cboSlsperid.setValue(prevRec.data.SlsperId);
                    }
                }
                break;

            case "last":
                var lastRec = App.cboSlsperid.store.getAt(App.cboSlsperid.store.getCount() - 1);
                if (lastRec) {
                    App.cboSlsperid.setValue(lastRec.data.SlsperId);
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
                if (App.frmMain.getRecord() != undefined) {
                    App.frmMain.updateRecord()
                };
                if (Process.storeIsChange(App.stoSalesPerson, false)) {
                    HQ.message.show(7, '', 'Process.closeScreen');
                } else {
                    this.parentAutoLoadControl.close()
                }
                break;

            case "new":
                if (HQ.isInsert) {
                    App.cboSlsperid.clearValue();
                }
                break;

            case "refresh":
                App.stoSalesPerson.reload();
                break;
            default:
        }
    }
};
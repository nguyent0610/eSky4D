// Declare
var _beginStatus = "H";


// Processing Function
var Process = {
    displayImage: function (imgControl, fileName) {
        Ext.Ajax.request({
            url: 'AR20201/ImageToBin',
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
                url: 'AR20201/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    branchID: App.cboBranchID.getValue(),
                    slsperID: App.cboPGID.getValue(),
                    lstPG: Ext.encode(App.stoPG.getRecordsValues())
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    Event.menuClick("refresh");
                    App.cboPGID.store.load();
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
            url: 'AR20201/Delete',
            clientValidation: false,
            waitMsg: HQ.common.getLang('Deleting') + "...",
            timeout: 1800000,
            params: {
                branchID: App.cboBranchID.getValue(),
                slsperID: App.cboPGID.getValue()
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                App.cboPGID.store.load();
                App.cboPGID.clearValue();
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
    stoPG_load: function (sto, records, successful, eOpts) {
        if (sto.getCount() == 0) {
            var newSlsper = Ext.create("App.mdlAR_PG", {
                SlsperId: App.cboPGID.getValue(),
                BranchID: App.cboBranchID.getValue(),
                Active: true,
                Status: _beginStatus
            });
            sto.insert(0, newSlsper);
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
    }
};

// Form Event
var Event = {
    frmMain_boxReady: function (frm, width, height, eOpts) {
        App.cboBranchID.store.load(function (records, operation, success) {
            App.cboBranchID.setValue(HQ.cpnyID);
        });
    },

    frmMain_fieldChange: function (frm) {
        frm.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoPG);
        HQ.common.changeData(HQ.isChange, 'AR20201');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        App.cboBranchID.setReadOnly(HQ.isChange);
        App.cboPGID.setReadOnly(HQ.isChange);
    },

    cboBranchID_change: function (cbo, newValue, oldValue, eOpts) {
        if (!newValue) {
            App.cboPGID.clearValue();
        }
        App.cboPGID.store.load(function (records, operation, success) {
            if (records.length > 0) {
                App.cboPGID.setValue(records[0].data.SlsperId);
            }
            else {
                App.cboPGID.clearValue();
            }
        });
        App.cboDeliveryMan.store.reload();
        App.cboSupID.store.reload();
        App.cboVendID.store.reload();
    },

    cboPGID_change: function (cbo, newValue, oldValue, eOpts) {
        App.stoPG.reload();
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
                HQ.combo.first(App.cboPGID, HQ.isChange);
                break;

            case "next":
                HQ.combo.next(App.cboPGID, HQ.isChange);
                break;

            case "prev":
                HQ.combo.prev(App.cboPGID, HQ.isChange);
                break;

            case "last":
                HQ.combo.last(App.cboPGID, HQ.isChange);
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
                    if (App.cboPGID.getValue() && App.cboStatus.getValue() == _beginStatus) {
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
                if (Process.storeIsChange(App.stoPG, false)) {
                    HQ.message.show(7, '', 'Process.closeScreen');
                } else {
                    this.parentAutoLoadControl.close()
                }
                break;

            case "new":
                if (HQ.isInsert) {
                    App.cboPGID.clearValue();
                }
                break;

            case "refresh":
                if (HQ.isChange) {
                    HQ.message.show(20150303, '', 'refresh');
                }
                else {
                    App.cboPGID.store.load(function () {
                        App.stoPG.reload();
                    });
                }
                
                break;
            default:
        }
    }
};
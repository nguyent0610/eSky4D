var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
//ATTENTION: isUpdate, isInsert, isDelete  -- from index.cshtml

// Submit the changed data (created, updated) into server side
function Save() {
    var curRecord = App.dataForm.getRecord();
    curRecord.data.Images = App.imgPPCStorePicReq.imageUrl;

    App.dataForm.getForm().updateRecord();
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'AR20200/Save',
            params: {
                lstARSalesPersonHeader: Ext.encode(App.storeARSalesPersonHeader.getChangedData({ skipIdForPhantomRecords: false })),
            },
            success: function (data) {
                menuClick('refresh');                
                App.cboSlsperid.getStore().reload();
                App.cboHandle.setValue("");
            },

            failure: function () {
            }
        });
    }
};

// Submit the deleted data into server side
function Delete(item) {
    if (item == 'yes') {
        try{
            App.direct.Delete(App.cboSlsperid.getValue(), App.cboBranchID.getValue(), {
                success: function (data) {
                    menuClick('refresh');
                    App.cboSlsperid.getStore().load();
                },
                failure: function () {
                    //
                },
                eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
            });
        } catch (ex) {
            alert(ex.message);
        }
    }
};

// Upload the picture file to server
function UploadImage() {
    App.dataForm.submit({
        waitMsg: 'Uploading your file...',
        url: 'AR20200/Upload',

        success: function (result) {
            //
        },
        failure: function (error) {
            //
        }
    });
};

// Load and show binding data to the form
var loadDataHeader = function () {
    if (App.storeARSalesPersonHeader.getCount() == 0) {
        App.storeARSalesPersonHeader.insert(0, Ext.data.Record());
    }
    var record = App.storeARSalesPersonHeader.getById(_curSlsperid);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
        if (record.data.Status == _hold) {
            enableControls(true);
        }
        else {
            enableControls(false);
        }
    } else {
        enableControls(true);
        App.dataForm.getForm().loadRecord(App.storeARSalesPersonHeader.getAt(0));
    }
};

// Event when cboBranchID is changed or selected item
var cboBranchID_Change = function (sender, e) {
    App.cboSlsperid.getStore().load();
    App.cboDeliveryMan.getStore().load();

    App.storeARSalesPersonHeader.reload();
    selectedRecord = 0;
};

// Event when cboSlsperid is changed or selected item
var cboSlsperid_Change = function (sender, e) {
    _curSlsperid = e;
    var record = App.storeARSalesPersonHeader.getById(e);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
        if (record.data.Images) {
            App.direct.GetImages(record.data.Images);
        } else {
            App.imgPPCStorePicReq.setImageUrl("");
        }

        if (record.data.Status == _hold) {
            enableControls(true);
        }
        else {
            enableControls(false);
        }
    } else {
        App.imgPPCStorePicReq.setImageUrl("");
        enableControls(true);
    }
};

// Event when cboCountryId is changed or selected item
var cboCountryId_Change = function (sender, e) {
    App.cboState.getStore().load();
};

// Event when cboState is changed or selected item
var cboState_Change = function (sender, e) {
    App.cboDistrict.getStore().load();
};

// Event when cboStatus is changed or selected item
var cboStatus_Change = function (sender, e) {
    App.cboHandle.getStore().reload();
};

// Event when cboHandle is changed or selected item
var cboHandle_Change = function (sender, e) {
    var curRecord = App.dataForm.getRecord();
    curRecord.setDirty();
};

// Event when uplPPCStorePicReq is change a file
var NamePPCStorePicReq_Change = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "jpg" || ext == "png" || ext == "gif") {
        UploadImage();
        var curRecord = App.dataForm.getRecord();
        curRecord.data.Images = App.imgPPCStorePicReq.imageUrl;
        curRecord.setDirty();
    } else {
        alert("Please choose a picture! (.jpg, .png, .gif)");
        sender.reset();
    }
};

// Click to clear image of sales person
var btnClearImage_Click = function (sender, e) {
    App.imgPPCStorePicReq.setImageUrl("");
    var curRecord = App.dataForm.getRecord();
    curRecord.data.Images = "";
    curRecord.setDirty();
};

// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            selectedIndex = 0;
            App.dataForm.getForm().loadRecord(App.storeARSalesPersonHeader.first());
            break;
        case "next":
            if (selectedIndex < (App.storeARSalesPersonHeader.getCount() - 1))
                selectedIndex += 1;
            App.dataForm.getForm().loadRecord(App.storeARSalesPersonHeader.getAt(selectedIndex));
            break;
        case "prev":
            if (selectedIndex > 0)
                selectedIndex -= 1;
            App.dataForm.getForm().loadRecord(App.storeARSalesPersonHeader.getAt(selectedIndex));
            break;
        case "last":
            selectedIndex = App.storeARSalesPersonHeader.getCount() - 1;
            App.dataForm.getForm().loadRecord(App.storeARSalesPersonHeader.last());
            break;
        case "save":
            Save();
            break;
        case "delete":
            var curRecord = App.dataForm.getRecord();
            if (curRecord && curRecord.data.Status == _hold) {
                if (isDelete) {
                    callMessage(11, '', 'Delete');
                }
            }
            break;
        case "close":
            if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
            if (storeIsChange(App.storeARSalesPersonHeader, false)) {
                callMessage(5, '', 'closeScreen');
            } else {
                this.parentAutoLoadControl.close()
            }
            break;
        case "new":
            if (App.dataForm.isValid()) {
                selectedIndex = 0;
                App.storeARSalesPersonHeader.insert(0, Ext.data.Record());
                App.dataForm.getForm().loadRecord(App.storeARSalesPersonHeader.first());
                App.cboStatus.setValue(_hold);
            }
            break;
        case "refresh":
            App.storeARSalesPersonHeader.load();
            break;
        default:
    }
};

// When anwser the confirmed closing
var closeScreen = function (item) {
    if (item == "no") {
        this.parentAutoLoadControl.close()
    }
    else if (item == "yes") {
        Save();
    }
};

// Check the store of data is change or not
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};

function enableControls(enable) {
    if (enable) {
        App.txtName.enable();
        App.cboPosition.enable();
        App.txtAddr1.enable();
        App.txtAddr2.enable();
        App.txtEMailAddr.enable();
        App.cboDeliveryMan.enable();
        App.cboSupID.enable();
        App.txtCrLmt.enable();
        App.cboCountryId.enable();
        App.cboState.enable();
        App.cboDistrict.enable();
        App.txtPhone.enable();
        App.txtFax.enable();
        App.txtCmmnPct.enable();
        App.chkPPCStorePicReq.enable();
        App.chkPPCAdmin.enable();
    } else {
        App.txtName.disable();
        App.cboPosition.disable();
        App.txtAddr1.disable();
        App.txtAddr2.disable();
        App.txtEMailAddr.disable();
        App.cboDeliveryMan.disable();
        App.cboSupID.disable();
        App.txtCrLmt.disable();
        App.cboCountryId.disable();
        App.cboState.disable();
        App.cboDistrict.disable();
        App.txtPhone.disable();
        App.txtFax.disable();
        App.txtCmmnPct.disable();
        App.chkPPCStorePicReq.disable();
        App.chkPPCAdmin.disable();
    }
};
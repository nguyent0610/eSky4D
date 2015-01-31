var selectedIndex = 0;
var _hold = "H";
//ATTENTION: isUpdate, isInsert, isDelete  -- from index.cshtml

// Submit the changed data (created, updated) into server side
function Save() {
    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        curRecord.data.Images = App.imgPPCStorePicReq.imageUrl;

        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AR20200/Save',
                params: {
                    lstARSalesPersonHeader: Ext.encode(App.stoSalesPerson.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (data) {
                    App.cboSlsperid.getStore().reload();
                    menuClick('refresh');
                },

                failure: function () {
                }
            });
        }
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
                eventMask: { msg: HQ.common.getLang('DeletingData'), showMask: true }
            });
        } catch (ex) {
            alert(ex.message);
        }
    }
};

// Upload the picture file to server
function UploadImage() {
    App.frmMain.submit({
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
var stoSalesPerson_load = function () {
    var record = App.stoSalesPerson.getAt(0);
    if (record) {
        // Edit record
        App.frmMain.getForm().loadRecord(record);

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
        // If has no record then create a new
        var insertedRecord = Ext.create("App.mdlAR_Salesperson", {
            BranchID: App.cboBranchID.value,
            SlsperId: "",
            Status: _hold
        })
        App.stoSalesPerson.insert(0, insertedRecord);
        App.frmMain.getForm().loadRecord(insertedRecord);
        App.imgPPCStorePicReq.setImageUrl("");

        enableControls(true);
    }
};

// Event when cboBranchID is changed or selected item
var cboBranchID_Change = function (sender, e) {
    App.cboDeliveryMan.getStore().reload();

    setTimeout(function () {
        App.cboSlsperid.getStore().reload();
    }, 500);
};

// Event when cboSlsperid is changed or selected item
var cboSlsperid_Change = function (sender, e) {
    App.stoSalesPerson.reload();
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
    var curRecord = App.frmMain.getRecord();
    curRecord.setDirty();
};

// Event when uplPPCStorePicReq is change a file
var NamePPCStorePicReq_Change = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "jpg" || ext == "png" || ext == "gif") {
        UploadImage();
        var curRecord = App.frmMain.getRecord();
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
    var curRecord = App.frmMain.getRecord();
    curRecord.data.Images = "";
    curRecord.setDirty();
};

// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            selectedIndex = 0;
            var slsper = App.cboSlsperid.store.getAt(selectedIndex);
            if (slsper) {
                App.cboSlsperid.setValue(slsper.data.SlsperId);
            }
            break;
        case "next":
            if (selectedIndex < (App.cboSlsperid.store.getCount() - 1))
                selectedIndex += 1;
            var slsper = App.cboSlsperid.store.getAt(selectedIndex);
            if (slsper) {
                App.cboSlsperid.setValue(slsper.data.SlsperId);
            }
            break;
        case "prev":
            if (selectedIndex > 0)
                selectedIndex -= 1;
            var slsper = App.cboSlsperid.store.getAt(selectedIndex);
            if (slsper) {
                App.cboSlsperid.setValue(slsper.data.SlsperId);
            }
            break;
        case "last":
            selectedIndex = App.cboSlsperid.store.getCount() - 1;
            var slsper = App.cboSlsperid.store.getAt(selectedIndex);
            if (slsper) {
                App.cboSlsperid.setValue(slsper.data.SlsperId);
            }
            break;
        case "save":
            Save();
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (curRecord && curRecord.data.Status == _hold) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'Delete');
                }
            }
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) {
                App.frmMain.updateRecord()
            };
            if (storeIsChange(App.stoSalesPerson, false)) {
                HQ.message.show(5, '', 'closeScreen');
            } else {
                this.parentAutoLoadControl.close()
            }
            break;
        case "new":
            if (HQ.isInsert) {
                selectedIndex = 0;
                App.cboSlsperid.setValue("");
                cboSlsperid_Change(App.cboSlsperid);
            }
            else {
                HQ.message.show(4, '', '');
            }
            break;
        case "refresh":
            if (App.frmMain.isValid()) {
                App.stoSalesPerson.reload();
                App.cboHandle.setValue("");
            }
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
    //App.cboBranchID.setReadOnly(enable);
    App.chkActive.setReadOnly(!enable);
    App.txtName.setReadOnly(!enable);
    App.cboPosition.setReadOnly(!enable);
    App.txtAddr1.setReadOnly(!enable);
    App.txtAddr2.setReadOnly(!enable);
    App.txtEMailAddr.setReadOnly(!enable);
    App.cboDeliveryMan.setReadOnly(!enable);
    App.cboSupID.setReadOnly(!enable);
    App.txtCrLmt.setReadOnly(!enable);
    App.cboCountryId.setReadOnly(!enable);
    App.cboState.setReadOnly(!enable);
    App.cboDistrict.setReadOnly(!enable);
    App.txtPhone.setReadOnly(!enable);
    App.txtFax.setReadOnly(!enable);
    App.txtCmmnPct.setReadOnly(!enable);
    App.chkPPCStorePicReq.setReadOnly(!enable);
    App.chkPPCAdmin.setReadOnly(!enable);
};
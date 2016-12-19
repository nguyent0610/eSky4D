//// Declare //////////////////////////////////////////////////////////
var keys = ['ClassID','AdverID'];
var fieldsCheckRequire = ["ClassID", "AdverID", "Descr"];
var fieldsLangCheckRequire = ["ClassID", "AdverID", "Descr"];
var _flagPosition = 0;
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;

var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_Advertise.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboClassID.getStore().addListener('load', checkLoad);
};
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_Advertise);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_Advertise);
            break;
        case "next":
            HQ.grid.next(App.grdOM_Advertise);
            break;
        case "last":
            HQ.grid.last(App.grdOM_Advertise);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_Advertise, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmOM_Advertise.selected.items[0] != undefined) {
                    if (App.slmOM_Advertise.selected.items[0].data.ClassID != ""
                        && App.slmOM_Advertise.selected.items[0].data.AdverID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_Advertise)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_Advertise, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":        
            break;
    }

};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23000');
};

var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23000');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var grdOM_Advertise_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdOM_Advertise_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_Advertise, e, keys);
};

var grdOM_Advertise_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_Advertise, e, keys, true);
};

var grdOM_Advertise_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_Advertise);
    stoChanged(App.stoOM_Advertise);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.stoOM_Advertise.clearFilter();

        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM23000/Save',
            params: {
                lstOM_Advertise: Ext.encode(App.stoOM_Advertise.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_Advertise.deleteSelected();
        stoChanged(App.stoOM_Advertise);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_Advertise.reload();
    }
};

var renderClassID = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboClassIDOM23000_pcClassID.findRecord("ClassID", rec.data.ClassID);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var stringFilter = function (record) {
    if (this.dataIndex == 'ClassID') {
        App.cboClassID.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboClassID.store, "ClassID", "Descr");
    }
    else return HQ.grid.filterString(record, this);
};

var btnImage1_Click = function (record) {
    _row = record;
    _flagPosition = 1;
    var uploadField = Ext.getCmp('FileUpLoad');
    uploadField.fileInputEl.dom.click(record);
};

var btnImage2_Click = function (record) {
    _row = record;
    _flagPosition = 2;
    var uploadField = Ext.getCmp('FileUpLoad');
    uploadField.fileInputEl.dom.click(record);
};

var fupPPCStorePicReq_Change = function (fup, newValue, oldValue, eOpts, record) {
    HQ.common.showBusy(true, HQ.common.getLang('Uploading...'), App.frmMain);
    if (fup.value) {
        var ext = fup.value.split(".").pop().toLowerCase();

        App.frmMain.submit({
            timeout: 1800000,
            clientValidation: false,
            type: 'POST',
            url: 'OM23000/UploadImage',
            params: ({
                fileOldName: _flagPosition == 1 ? _row.data.Video : _row.data.Profile,
                fileName: fup.value,
                ClassID: _row.data.ClassID,
                AdverID: _row.data.AdverID,
                flagPosition: _flagPosition,
            }),
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                if (data.result.Exist) {
                    if(_flagPosition == 1)
                    {
                        _row.set('Video', data.result.Exist);
                        _row.set('tstamp', data.result.newTstamp);
                    }
                        
                    else if (_flagPosition == 2) {
                        _row.set('Profile', data.result.Exist);
                        _row.set('tstamp', data.result.newTstamp);
                    }
                }
            },
            failure: function (errorMsg, data) {
                HQ.message.process(errorMsg, data, true);
            }
        });
    }
    HQ.common.showBusy(false);
};


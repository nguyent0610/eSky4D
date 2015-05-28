var keys = ['Branch', 'Area', 'SKU'];
var fieldsCheckRequire = ["Branch", "Area", "SKU"];
var fieldsLangCheckRequire = ["Branch", "Area", "SKU"];

var _focusNo = 0;

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboID.getStore().load(function () {
        App.cboStatus.getStore().load(function () {
            App.cboHandle.getStore().load(function () {
                App.stoOM_HOKPI.reload();
            })
        })
    });
};

var loadComboGrid = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboBranch.getStore().load(function () {
        App.cboTerritory.getStore().load(function () {
            App.cboInvt.getStore().load(function () {
                App.stoOM_HOKPIDetail.reload();
                HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
            })
        })
    });
};

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlGrid') {
            _focusNo = 1;
        }
        else {//pnlHeader
            _focusNo = 0;
        }
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.isFirstLoad = true;
                HQ.combo.first(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.first(App.grdOM_HOKPIDetail);
            }

            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.prev(App.grdOM_HOKPIDetail);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.next(App.grdOM_HOKPIDetail);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboID, HQ.isChange);
            }
            else {
                HQ.grid.last(App.grdOM_HOKPIDetail);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboID.valueModels == null) App.cboID.setValue('');
                App.cboID.getStore().load(function () { App.stoOM_HOKPI.reload(); });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboID.setValue('');
                    }
                }
                else {
                    HQ.grid.insert(App.grdOM_HOKPIDetail, keys);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (_focusNo == 0) {
                    var curRecord = App.frmMain.getRecord();
                    if (curRecord) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
                else {
                    if (App.slmOM_HOKPIDetail.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_HOKPIDetail);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_HOKPIDetail), ''], 'deleteData', true)
                    }
                }

            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoOM_HOKPIDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);
            break;
    }
};

//load lần đầu khi mở
var firstLoad = function () {
    HQ.isFirstLoad = true;
    loadSourceCombo();
};

////////////Kiem tra combo chinh CpnyID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = (HQ.store.isChange(App.stoOM_HOKPI) == false ? HQ.store.isChange(App.stoOM_HOKPIDetail) : true);
    HQ.common.changeData(HQ.isChange, 'OM21900');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboID.valueModels == null || HQ.isNew == true)
        App.cboID.setReadOnly(false);
    else App.cboID.setReadOnly(HQ.isChange);
};

//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdOM_HOKPIDetail_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdOM_HOKPIDetail_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_HOKPIDetail, e, keys);
    frmChange();
};

var grdOM_HOKPIDetail_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_HOKPIDetail, e, keys);
};

var grdOM_HOKPIDetail_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_HOKPIDetail);
    //stoChanged(App.stoOM_HOKPIDetail);
    frmChange();
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21900');
};

//load store khi co su thay doi CpnyID
var stoLoad = function (sto) {

    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboID.forceSelection = false;
    //App.cboHandle.setValue('N');
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "ID");
        record = sto.getAt(0);
        record.data.Status = 'H';
        record.data.FromDate = HQ.bussinessDate;
        record.data.EndDate = HQ.bussinessDate;
        HQ.isNew = true;//record la new    

        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    loadComboGrid();
};

var stoLoadOM_HOKPIDetail = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        //HQ.isFirstLoad = false;
    }
    frmChange();
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

// Event when cboVendID is changed or selected item 
var cboID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoOM_HOKPI.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }
};

function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'OM21900/Save',
            params: {
                lstOM_HOKPI: Ext.encode(App.stoOM_HOKPI.getRecordsValues()),
                lstOM_HOKPIDetail: Ext.encode(App.stoOM_HOKPIDetail.getChangedData({ skipIdForPhantomRecords: false })),
                isNew: HQ.isNew
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var ID = data.result.ID;
                App.cboID.getStore().load(function () {
                    App.cboID.setValue(ID);
                    App.stoOM_HOKPI.reload();
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

// Submit the deleted data into server side
function deleteData(item) {
    if (item == 'yes') {
        if (_focusNo == 0) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                url: 'OM21900/DeleteAll',
                success: function (action, data) {
                    App.cboID.setValue("");
                    App.cboID.getStore().load(function () { cboID_Change(App.cboID); });

                },
                failure: function (action, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                }
            });
        }
        else {
            App.grdOM_HOKPIDetail.deleteSelected();
            frmChange();
        }

    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        var ID = '';
        if (App.cboID.valueModels != null) ID = App.cboID.getValue();
        App.cboID.getStore().load(function () { App.cboID.setValue(ID); App.stoOM_HOKPI.reload(); });

    }
};
///////////////////////////////////
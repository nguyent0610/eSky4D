var keys = [''];
var fieldsCheckRequire = [""];
var fieldsLangCheckRequire = [""];

var _focusNo = 0;

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboTagID.getStore().load(function () {
        App.cboReasonCD.getStore().load(function () {
            App.cboStatus.getStore().load(function () {
                App.cboHandle.getStore().load(function () {
                    App.stoIN_TagHeader.reload();
                })
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
                HQ.combo.first(App.cboTagID, HQ.isChange);
            }
            else {
                HQ.grid.first(App.grdIN_TagDetail);
            }

            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboTagID, HQ.isChange);
            }
            else {
                HQ.grid.prev(App.grdIN_TagDetail);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboTagID, HQ.isChange);
            }
            else {
                HQ.grid.next(App.grdIN_TagDetail);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboTagID, HQ.isChange);
            }
            else {
                HQ.grid.last(App.grdIN_TagDetail);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboTagID.valueModels == null) App.cboTagID.setValue('');
                App.cboTagID.getStore().load(function () { App.stoIN_TagHeader.reload(); });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboTagID.setValue('');
                    }
                }
                else {
                    HQ.grid.insert(App.grdIN_TagDetail, keys);
                }
            }
            break;
        case "delete":
            if (App.cboStatus.getValue() == 'H') {
                if (HQ.isDelete) {
                    if (_focusNo == 0) {
                        var curRecord = App.frmMain.getRecord();
                        if (curRecord) {
                            HQ.message.show(11, '', 'deleteData');
                        }
                    }
                    else {
                        //if (App.slmgrdInvt.selected.items[0] != undefined) {
                        //    var s = HQ.grid.indexSelect(App.grdInvt);
                        //    if (s != '')
                        //        HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdInvt), ''], 'deleteData', true)
                        //}
                    }

                }
            }
            else {
                HQ.message.show(2015061202, '', '');
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoIN_TagDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    HQ.isChange = (HQ.store.isChange(App.stoIN_TagHeader) == false ? HQ.store.isChange(App.stoIN_TagDetail) : true);
    HQ.common.changeData(HQ.isChange, 'IN10500');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboTagID.valueModels == null || HQ.isNew == true)
        App.cboTagID.setReadOnly(false);
    else App.cboTagID.setReadOnly(HQ.isChange);
};

//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdIN_TagDetail_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() == "C") {
        return false;
    }
    else {
        return HQ.grid.checkBeforeEdit(e, keys);
    }
};

var grdIN_TagDetail_Edit = function (item, e) {
    if (e.field == "ActualCaseQty") {
        e.record.set("OffetCaseQty", e.record.data.ActualCaseQty - e.record.data.BookCaseQty);
    }
    if (e.field == "ActualEAQty") {
        e.record.set("OffsetEAQty", e.record.data.ActualEAQty - e.record.data.BookEAQty);
    }
    HQ.grid.checkInsertKey(App.grdIN_TagDetail, e, keys);
    frmChange();
};

var grdIN_TagDetail_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdIN_TagDetail, e, keys);
};

var grdIN_TagDetail_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN_TagDetail);
    //stoChanged(App.stoIN_TagDetail);
    frmChange();
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN10500');
};

//load store khi co su thay doi CpnyID
var stoLoad = function (sto) {

    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    //App.cboTagID.forceSelection = false;
    //App.cboHandle.setValue('N');
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "TagID");
        record = sto.getAt(0);
        record.data.Status = 'H';
        HQ.isNew = true;//record la new    

        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboTagID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    if (record.data.Status == "C") {
        lockControl(true);
    } else {
        lockControl(false);
    }

    App.frmMain.getForm().loadRecord(record);

    App.stoIN_TagDetail.reload();
};

var lockControl = function (value) {
    setTimeout(function () {
        App.cboReasonCD.setReadOnly(value);
        App.dtpTranDate.setReadOnly(value);
        App.cboHandle.setReadOnly(value);
        App.txtDescr.setReadOnly(value);
    }, 300);
};

var stoLoadIN_TagDetail = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keys);
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
var cboTagID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoIN_TagHeader.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboTagID_Expand = function (sender, value) {
    //if (HQ.isChange) {
    //    App.cboTagID.collapse();
    //}
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboTagID_TriggerClick = function (sender, value) {
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
            timeOut: 7200,
            url: 'IN10500/Save',
            params: {
                lstIN_TagHeader: Ext.encode(App.stoIN_TagHeader.getRecordsValues()),
                lstIN_TagDetail: Ext.encode(App.stoIN_TagDetail.getChangedData({ skipIdForPhantomRecords: false })),
                SiteID: App.cboTagID.valueModels[0] == undefined ? HQ.cpnyID : App.cboTagID.valueModels[0].data.SiteID
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var ID = data.result.TagID;
                App.cboTagID.getStore().load(function () {
                    App.cboTagID.setValue(ID);
                    App.stoIN_TagHeader.reload();
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
                url: 'IN10500/DeleteAll',
                params: {
                    SiteID: App.cboTagID.valueModels[0] == undefined ? HQ.cpnyID : App.cboTagID.valueModels[0].data.SiteID,
                },
                success: function (action, data) {
                    App.cboTagID.setValue("");
                    App.cboTagID.getStore().load(function () { cboTagID_Change(App.cboTagID); });

                },
                failure: function (action, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                }
            });
        }
        else {
            //App.grdIN_TagDetail.deleteSelected();
            //frmChange();
        }
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        var ID = '';
        if (App.cboTagID.valueModels != null) ID = App.cboTagID.getValue();
        App.cboTagID.getStore().load(function () { App.cboTagID.setValue(ID); App.stoIN_TagHeader.reload(); });

    }
};

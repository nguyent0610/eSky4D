var keys = ['InvtID'];
var fieldsCheckRequire = ["InvtID"];
var fieldsLangCheckRequire = ["InvtID"];
HQ.barCode = '';
var _focusNo = 0;
HQ.isChange = false;
HQ.isSaving = false;
HQ.NumberSoure = 0;
var _beginStatus = 'H';
var _isFirstTime = true;
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboStatus.getStore().load(function () {
        HQ.common.showBusy(false, HQ.common.getLang("loadingData"));      
    });
    App.cboBranchID.setValue(HQ.cpnyID);
    App.cboTagID.store.reload();
    App.stoIN_TagHeader.reload();
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
                App.cboTagID.getStore().load(function () {
                    if (App.frmMain.isValid()) {

                        App.stoIN_TagHeader.reload();
                    }                    
                });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    HQ.isChange = false;                    
                    App.cboTagID.setValue('');                    
                    App.cboSiteID.setValue('');
                    App.stoIN_TagHeader.reload();                    
                }                                
            }
            break;
        case "delete":
            if (App.cboStatus.getValue() == _beginStatus) {
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
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoIN_TagDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'IN10500?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
}

var cboBranchID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    App.cboTagID.setValue('');
    if (sender.valueModels != null) {
        App.cboTagID.store.reload();
    }
};

var cboBranchID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    App.cboTagID.setValue('');
    if (sender.valueModels != null && !App.stoIN_TagHeader.loading) {
        App.cboTagID.store.reload();
    }
};

var cboTagID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoIN_TagHeader.reload();       
    }
};

var cboTagID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_TagHeader.loading) {
        App.stoIN_TagHeader.reload();      
    }
};
var cboReasonCD_Change = function (sender, value) {
    App.stoIN_TagDetail.suspendEvents();
    for (var i = 0; i < App.stoIN_TagDetail.data.length ; i++) {
        App.stoIN_TagDetail.data.items[i].set('ReasonCD', value);
    }
    App.stoIN_TagDetail.resumeEvents();
    App.grdIN_TagDetail.view.refresh();
};
var cboSiteID_Change = function (sender, value) {
    if (App.frmMain.isValid()) {
        App.stoIN_TagDetail.reload();
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
    var isSetSite = false;
    if (App.stoIN_TagHeader.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = (HQ.store.isChange(App.stoIN_TagHeader) == false ? HQ.store.isChange(App.stoIN_TagDetail) : true);
        HQ.common.changeData(HQ.isChange, 'IN10500');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboTagID.valueModels == null || HQ.isNew == true) {
            App.cboTagID.setReadOnly(false);
            App.cboSiteID.setReadOnly(HQ.store.isChange(App.stoIN_TagDetail));
            isSetSite = true;
        }
        else {
            App.cboTagID.setReadOnly(HQ.isChange);
            //App.cboSiteID.setReadOnly(HQ.isChange);
        }
    }
    if (!isSetSite) {
        App.cboSiteID.setReadOnly(!HQ.isNew);
    }    
    App.dtpTranDate.setReadOnly(!HQ.isNew);
};

//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdIN_TagDetail_BeforeEdit = function (editor, e) {

    if (App.cboStatus.getValue() != _beginStatus) {
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
    // Calc Total Actual Qty
    calcTotQty();
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

//load store khi co su thay doi TagID
var stoIN_TagHeader_Load = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    HQ.isFirstLoad = true;  
    HQ.isNew = false;
    //App.cboTagID.forceSelection = false;
    //App.cboHandle.setValue('N');
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "TagID");
        record = sto.getAt(0);
        record.data.Status = _beginStatus;
        record.data.TranDate = HQ.bussinessDate;
        HQ.isNew = true;//record la new    

        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboTagID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    if (record.data.Status != _beginStatus) {
        lockControl(true);
    } else {
        lockControl(false);
    }
    App.frmMain.getForm().loadRecord(record);
    HQ.NumberSoure = 0;
    if (!_isFirstTime) {        
        App.stoIN_TagDetail.reload();
    } else {
        _isFirstTime = false;
        HQ.common.showBusy(false);
    }
    App.cboSiteID.setReadOnly(!HQ.isNew);
    //App.stoInvt.reload();
};
stoInvt_Load = function (sto) {
    HQ.NumberSoure += 1;
    if (HQ.NumberSoure == 2)
        HQ.common.showBusy(false);
};
var lockControl = function (value) {
    setTimeout(function () {
        App.cboReasonCD.setReadOnly(value);
       // App.dtpTranDate.setReadOnly(value);
       // App.cboHandle.setReadOnly(value);
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
    // Calc Total Actual Qty
    calcTotQty();
    HQ.NumberSoure += 1;
    if (HQ.NumberSoure == 1) HQ.common.showBusy(false);
    frmChange();
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {

};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboTagID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        this.clearValue();
        menuClick('new');
    }
};

// Load data
var btnLoad_Click = function () {
    if (App.frmMain.isValid()) {
        App.stoIN_TagDetail.reload();
    }
};
function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        HQ.isSaving = true;
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            timeOut: 7200,
            url: 'IN10500/Save',
            params: {
                lstIN_TagHeader: Ext.encode(App.stoIN_TagHeader.getRecordsValues()),
                lstIN_TagDetail: Ext.encode(App.stoIN_TagDetail.getRecordsValues()),//Ext.encode(App.stoIN_TagDetail.getChangedData({ skipIdForPhantomRecords: false })),
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var ID = data.result.TagID;
                App.cboTagID.getStore().load(function () {
                    App.cboTagID.setValue(ID);
                    App.stoIN_TagHeader.reload();
                });
                HQ.isSaving = false;
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                HQ.isSaving = false;
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
// Render Rownumber
var renderRowNumber= function (value, meta, record) {
    return App.stoIN_TagDetail.data.indexOf(record) + 1;
}
// Render ReasonCD
var rendererReasonCD = function (value) {
    var r = HQ.store.findInStore(App.cboReasonCD.store, ['ReasonCD'], [value]);
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.Descr;
}
// Filter ReasonCD
var stringFilter = function (record) {
    App.cboReason.store.clearFilter();
    return HQ.grid.filterComboDescr(record, this, App.cboReason.store, "ReasonCD", "Descr");
}

// Calc Total Actual Qty
var calcTotQty = function () {
    var totQty = 0;
    for (var i = 0; i < App.stoIN_TagDetail.data.length; i++) {
        totQty += App.stoIN_TagDetail.data.items[i].data.ActualCaseQty;
    }
    App.txtTotQty.setValue(totQty);
};

///////////////////////////////////
var keys = ['InvtID'];
var fieldsCheckRequire = ["InvtID", 'Notes'];
var fieldsLangCheckRequire = ["InvtID", 'Notes'];
HQ.barCode = '';
var _focusNo = 0;
HQ.isChange = false;
HQ.isSaving = false;
var _beginStatus = 'H';
var _isFirstTime = true;
var _isLoadStoreTag = false;
var cboTagStoreLoad = function (sto) {
    App.cboTagID.setValue(HQ.TagID);
    _isLoadStoreTag = HQ.TagID == '' ? false : true;
}
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboStatus.getStore().load(function () {
        HQ.common.showBusy(false);      
    });
    App.cboBranchID.setValue(HQ.branchID);
    App.cboTagID.store.reload();
    if (HQ.TagID == '') {
        App.stoIN_TagHeader.reload();
    }
//  
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
                    if (HQ.form.checkRequirePass(App.frmMain)) {

                        App.stoIN_TagHeader.reload();
                    }                    
                });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        HQ.isChange = false;
                        App.cboTagID.setValue('');
                        App.cboSiteID.setValue('');
                        App.stoIN_TagHeader.reload();
                    }
                } else {
                    if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus) {
                        var obj = HQ.store.findInStore(App.stoIN_TagDetail, [keys], ['']);
                        if (!obj) {
                            HQ.store.insertBlank(App.stoIN_TagDetail, keys);
                        }                        
                    }
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
                        if (App.slmIN_TagDetail.selected.items[0] != undefined) {
                            if (HQ.grid.indexSelect(App.grdIN_TagDetail) != '')
                                HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdIN_TagDetail), ''], 'deleteData', true)
                        }
                    }
                }
            }
            else {
                HQ.message.show(1002, '', '');
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && checkRequirePass(App.stoIN_TagDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    if (App.stoIN_TagDetail.data.length > 0) {
                        if (App.stoIN_TagDetail.data.items[0].data.SiteID != App.cboSiteID.getValue()) {
                            HQ.message.show(2016102101);
                            return false;
                        }
                    }
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

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN10500');
};

//load store khi co su thay doi TagID
var stoIN_TagHeader_Load = function (sto) {
    HQ.isFirstLoad = true;
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "TagID");
        record = sto.getAt(0);
        record.data.Status = _beginStatus;
        record.data.TranDate = HQ.bussinessDate;
        record.data.SiteID = HQ.dftSiteID;
        HQ.isNew = true;//record la new    
        App.cboSiteID.store.clearFilter();
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
    if (_isLoadStoreTag) {
        App.stoIN_TagDetail.reload();
        _isFirstTime = false;
        _isLoadStoreTag = false;
    }
    else if (!_isFirstTime) {
        if (App.frmMain.isValid()) {
            if (!App.cboTagID.getValue()) {
                App.direct.IN10500_pdCheckCreateIN_Tag(
                App.cboBranchID.getValue(), App.cboSiteID.getValue(),
                {
                    success: function (result) {
                        if (result == '') {
                            App.stoIN_TagDetail.reload();
                        }
                        else {
                            result = "<div style ='overflow: auto !important; max-height:400px !important'> " + result + " </div>";
                            HQ.message.show(2015070801, result, '', false);
                            HQ.common.showBusy(false);
                        }
                    },
                    failure: function (result) {
                        HQ.common.showBusy(false);
                    }
                });
            } else {
                App.stoIN_TagDetail.reload();
            }
        } else {
            App.stoIN_TagDetail.removeAll();
            App.grdIN_TagDetail.view.refresh();
            HQ.common.showBusy(false);
        }
       
    } else {
        _isFirstTime = false;
        HQ.common.showBusy(false);
    }
    App.cboSiteID.setReadOnly(!HQ.isNew);
};

var lockControl = function (value) {
    setTimeout(function () {
        App.cboReasonCD.setReadOnly(value);
        App.dtpTranDate.setReadOnly(value);
        App.txtDescr.setReadOnly(value);
    }, 300);
};

var stoLoadIN_TagDetail = function (sto) {

    if (HQ.isFirstLoad) {
        //if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus) {
        //    HQ.store.insertBlank(sto, keys);
        //}
        HQ.isFirstLoad = false;
    }
    if (App.cboStatus.getValue() == _beginStatus) {
        var obj = HQ.store.findInStore(App.stoIN_TagDetail, [keys], ['']);
        if (!obj) {
            HQ.store.insertBlank(App.stoIN_TagDetail, keys);
        }
    }
    // Calc Total Actual Qty
    calcTotQty();
    HQ.common.showBusy(false);
    frmChange();
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'IN10500?BranchID=' + App.cboPopupCpny.getValue();
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
        var siteID = sender.valueModels[0] ? sender.valueModels[0].data.SiteID : '';
        App.cboSiteID.setValue(siteID);
        App.stoIN_TagHeader.reload();       
    } 
};

var cboTagID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_TagHeader.loading) {
        var siteID = sender.valueModels[0] ? sender.valueModels[0].data.SiteID : '';
        App.cboSiteID.setValue(siteID);
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
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
   // App.cboInvtID.store.reload();    
    App.cboInvtID.getStore().load(function () {
        HQ.common.showBusy(false);
    });
};

//load lần đầu khi mở
var firstLoad = function () {   
    HQ.isFirstLoad = true;
    HQ.util.checkAccessRight();
    App.cboTagID.getStore().addListener('load', cboTagStoreLoad);
  //  App.stoIN_TagHeader.getStore().addListener('load', cboTagStoreLoad);
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
            App.cboSiteID.setReadOnly(App.stoIN_TagDetail.data.length > 0 && HQ.store.isChange(App.stoIN_TagDetail));
            isSetSite = true;
        }
        else {
            App.cboTagID.setReadOnly(HQ.isChange);
        }
    }
    if (!isSetSite) {
        App.cboSiteID.setReadOnly(!HQ.isNew);
    }    
};

//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdIN_TagDetail_BeforeEdit = function (editor, e) {

    if (App.cboStatus.getValue() != _beginStatus) {
        return false;
    }
    else {
        if (App.stoIN_TagDetail.data.length > 1) {
            if (App.stoIN_TagDetail.data.items[0].data.SiteID != App.cboSiteID.getValue()) {
                HQ.message.show(2016102101);
                return false;
            }
        } else {
            if (e.record.data.InvtID != '' && e.record.data.SiteID != App.cboSiteID.getValue()) {
                HQ.message.show(2016102101);
                return false;
            }
        }
        
        return HQ.grid.checkBeforeEdit(e, keys);
    }
};

var grdIN_TagDetail_Edit = function (item, e) {
    if (e.field == "ActualEAQty") {
        e.record.set("OffsetEAQty", e.record.data.ActualEAQty - e.record.data.BookEAQty);
    }
    if (e.field == 'InvtID') {
        var record = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [e.value]);
        if (record) {
            e.record.set('InvtName', record.InvtName);
            e.record.set('SiteID', App.cboSiteID.getValue());
            e.record.set('EAUnit', record.EAUnit);
            e.record.set('BookEAQty', record.BookEAQty);
            e.record.set("OffsetEAQty", e.record.data.ActualEAQty - e.record.data.BookEAQty);
            e.record.set('ReasonCD', App.cboReasonCD.getValue());
            e.record.set('StkItem', record.StkItem);
        }
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
    calcTotQty();
    //stoChanged(App.stoIN_TagDetail);
    frmChange();
};


//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboTagID_TriggerClick = function (sender, value) {
    this.clearValue();
    //menuClick('new');
};

// Load data
var btnLoad_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        var isChange = HQ.store.isChange(App.stoIN_TagDetail);
        if (isChange && App.stoIN_TagDetail.data.length > 0) {
            HQ.message.show(20150303, '', 'refreshDet');
        }
        else {
            HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
            if (!App.cboTagID.getValue()) {
                App.direct.IN10500_pdCheckCreateIN_Tag(
                    App.cboBranchID.getValue(), App.cboSiteID.getValue(),
                    {
                        success: function (result) {
                            if (result == '') {
                                App.stoIN_TagDetail.reload();
                            }
                            else {
                                result = "<div style ='overflow: auto !important; max-height:400px !important'> " + result + " </div>";                                
                                HQ.message.show(2015070801, result, '', false);
                                HQ.common.showBusy(false);
                            }
                        },
                        failure: function (result) {
                            HQ.common.showBusy(false);
                        }
                    });
            } else {
                App.stoIN_TagDetail.reload();
            }
        }
    }
};
function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    var isSave = !(App.cboStatus.getValue() == 'C' && (!App.cboHandle.getValue() || App.cboHandle.getValue() == '' || App.cboHandle.getValue() == 'N'));
    if (HQ.form.checkRequirePass(App.frmMain) && isSave) {
        HQ.isSaving = true;
        var siteName = '';
        var objSite = HQ.store.findInStore(App.cboSiteID.store, ['SiteID'], [App.cboSiteID.getValue()]);
        if (objSite) {
            siteName = objSite.Name;
        }
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            timeOut: 180000,
            url: 'IN10500/Save',
            params: {
                lstIN_TagHeader: Ext.encode(App.stoIN_TagHeader.getRecordsValues()),
                lstIN_TagDetail: Ext.encode(App.stoIN_TagDetail.getRecordsValues()),//Ext.encode(App.stoIN_TagDetail.getChangedData({ skipIdForPhantomRecords: false })), 
                lstDel: Ext.encode(App.stoIN_TagDetail.getChangedData().Deleted),
                siteName: siteName
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var ID = data.result.TagID;
                HQ.TagID = ID;
                HQ.SiteID = App.cboSiteID.getValue();
                App.cboTagID.getStore().load(function () {
                    App.cboTagID.setValue(ID);
                    App.stoIN_TagHeader.reload();
                });
                App.cboHandle.setValue('N');
                HQ.isSaving = false;
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                HQ.isSaving = false;
                App.cboHandle.setValue('N');
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
                    SiteID: App.cboTagID.valueModels[0] == undefined ? HQ.branchID : App.cboTagID.valueModels[0].data.SiteID,
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
            App.grdIN_TagDetail.deleteSelected();
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
        if (App.cboTagID.valueModels != null) ID = App.cboTagID.getValue();
        App.cboTagID.getStore().load(function () { App.cboTagID.setValue(ID); App.stoIN_TagHeader.reload(); });
    }
};
function refreshDet(item) {
    if (item == 'yes') {
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.stoIN_TagDetail.reload();
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
        totQty += App.stoIN_TagDetail.data.items[i].data.ActualEAQty;
    }
    App.txtTotQty.setValue(totQty);
};

var checkRequirePass = function (store, keys, fieldsCheck, fieldsLang) {    
    for (var i = 0; i < store.data.length; i++) {
        if (store.data.items[i].data.InvtID != ''
            && store.data.items[i].data.ReasonCD == 'OT'
            && HQ.util.passNull(store.data.items[i].data.Notes).toString().trim() == "") {
            var param = i + 1;
            HQ.message.show(2016111101, param, '', false);
            return false;                            
        }
    }
    return true;
}
///////////////////////////////////
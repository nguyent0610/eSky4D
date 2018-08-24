var keys = ['LineRef','InvtID'];
var fieldsCheckRequire = ["InvtID","SiteID", 'Notes'];
var fieldsLangCheckRequire = ["InvtID","SiteID", 'Notes'];
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
                var ID = '';
                if (App.cboTagID.valueModels == null || App.cboTagID.valueModels[0] == null) {
                    App.cboTagID.setValue(ID);
                    App.stoLot.reload();
                }
                //App.cboTagID.getStore().load(function () {
                    if (HQ.form.checkRequirePass(App.frmMain)) {

                        App.stoIN_TagHeader.reload();
                        App.stoLot.reload();
                    }                    
                //});
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
                        App.cboWhseLoc.setValue('');
                        App.stoIN_TagHeader.reload();
                        App.stoLot.reload();
                    }
                } else {
                    if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus && HQ.allowAddNewInvtID) {
                        if (HQ.showColWhseLoc == 2) {
                            var obj = HQ.store.findInStore(App.stoIN_TagDetail, keys, ["", "", "", ""]);
                            if (!obj) {
                                HQ.store.insertBlank(App.stoIN_TagDetail, keys);
                            }
                        }
                        else {
                            var obj = HQ.store.findInStore(App.stoIN_TagDetail, keys, ["", "", ""]);
                            if (!obj) {
                                HQ.store.insertBlank(App.stoIN_TagDetail, keys);
                            }
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
                        //if (App.stoIN_TagDetail.data.items[0].data.SiteID != App.cboSiteID.getValue()) {
                        //    HQ.message.show(2016102101);
                        //    return false;
                        //}
                    }

                    var checkPerPost = true;
                    if (HQ.CheckperPost) {
                        var objPerPost = HQ.store.findRecord(App.cboPerPost.store, ['Code'], [App.cboPerPost.getValue()]);
                        if (objPerPost != undefined) {
                            var tam = App.dtpTranDate.getValue();
                            if (tam > objPerPost.data.EndDate || tam < objPerPost.data.StartDate) {
                                checkPerPost = false;
                            }
                        }
                        else {
                            checkPerPost = false;
                        }
                    }

                    if (HQ.CheckperPost && !checkPerPost) {
                        HQ.message.show(2018081311, '', 'checkSave');
                       
                    }
                    else {
                        save();
                    }
                }
            }
            break;
        case "print":                       
            if (!Ext.isEmpty(App.cboTagID.getValue())) {// && App.Status.value == 'C') {
                if (App.cboReport.store.data.items.length == 1) {
                    btnShowReport_Click();
                }
                else  App.winReport.show();
            }
            break;
        case "close":
            HQ.common.close(this);
            break;
    }
};

function checkSave(item) {
    if (item == 'yes') {
        save();
    }
};

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN10500');
};

//load store khi co su thay doi TagID
var stoIN_TagHeader_Load = function (sto) {
    App.cboWhseLoc.store.clearFilter();
    App.txtTotQty.setValue(0);
    HQ.isFirstLoad = true;
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "TagID");
        record = sto.getAt(0);
        record.data.Status = _beginStatus;
        record.data.TranDate = HQ.bussinessDate;
        record.data.SiteID = HQ.dftSiteID;
        record.data.PerPost = HQ.perPost;
        if (HQ.showWhseLoc != 0) {
            record.data.WhseLoc = HQ.dftWhseLoc;
        }
        HQ.isNew = true;//record la new    
        App.cboSiteID.store.clearFilter();
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboTagID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
        App.cboType.setReadOnly(false);
    }
    else {
        App.cboType.setReadOnly(true);
    }
    var record = sto.getAt(0);
    if (record.data.Status != _beginStatus) {
        lockControl(true);
    } else {
        lockControl(false);
    }
    if (App.cboTagID.getValue() != null) {
        App.cboClassID.setReadOnly(true);
        App.btnLoad.disable();        
    }
    else {
        App.cboClassID.setReadOnly(false);
        App.btnLoad.enable();
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
                App.cboBranchID.getValue(), record.data.SiteID, record.data.WhseLoc, record.data.WhseLoc,
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
    App.cboWhseLoc.setReadOnly(!HQ.isNew);
};

var lockControl = function (value) {
    setTimeout(function () {
        App.cboReasonCD.setReadOnly(value);
        App.dtpTranDate.setReadOnly(value);
        App.txtDescr.setReadOnly(value);
    }, 300);
};

var stoLoadIN_TagDetail = function (sto) {
    App.txtTotQty.setValue(0);
    if (HQ.isFirstLoad) {
        //if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus) {
        //    HQ.store.insertBlank(sto, keys);
        //}
        HQ.isFirstLoad = false;
    }
    if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus && HQ.allowAddNewInvtID) {
        if (HQ.showColWhseLoc == 2) {
            var obj = HQ.store.findInStore(App.stoIN_TagDetail, keys, ["", "", "", ""]);
            if (!obj) {
                HQ.store.insertBlank(App.stoIN_TagDetail, keys);
            }
        }
        else {
            var obj = HQ.store.findInStore(App.stoIN_TagDetail, keys, ["", "", ""]);
            if (!obj) {
                HQ.store.insertBlank(App.stoIN_TagDetail, keys);
            }
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
    App.cboInvtCheck.store.reload();
    App.cboInvtID.store.record();
    App.cboInvtoryLot.store.reload();
    App.cboClassID.store.reload();
};

var cboBranchID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    App.cboTagID.setValue('');
    if (sender.valueModels != null && !App.stoIN_TagHeader.loading) {
        App.cboTagID.store.reload();
    }
};

var cboTagID_Change = function (sender, value) {
    App.cboWhseLoc.store.clearFilter();
    HQ.isFirstLoad = true;
    if (sender.hasFocus) {
        App.cboReason.store.reload();
        App.cboReasonCD.store.reload();
    }
    if (sender.valueModels != null) {
        var siteID = sender.valueModels[0] ? sender.valueModels[0].data.SiteID : '';
        var whseLoc = sender.valueModels[0] ? sender.valueModels[0].data.WhseLoc : '';
        App.cboSiteID.setValue(siteID);
        if (HQ.showWhseLoc != 0) {
            App.cboWhseLoc.setValue(whseLoc);
        }
        App.stoIN_TagHeader.reload();
    }
    App.stoLot.reload();
    
};


var cboWhseLoc_Focus = function (sender, value) {
    App.cboWhseLoc.store.clearFilter();
    App.cboWhseLoc.store.filter('SiteID', new RegExp('^' + Ext.escapeRe(App.cboSiteID.getValue()) + '$'));
};

var cboTagID_Select = function (sender, value) {
    App.cboWhseLoc.store.clearFilter();
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_TagHeader.loading) {
        var siteID = sender.valueModels[0] ? sender.valueModels[0].data.SiteID : '';
        var whseLoc = sender.valueModels[0] ? sender.valueModels[0].data.WhseLoc : '';
        App.cboSiteID.setValue(siteID);
        App.cboWhseLoc.store.reload();
        if (HQ.showWhseLoc != 0) {
            App.cboWhseLoc.setValue(whseLoc);
        }
        App.stoIN_TagHeader.reload();      
    }
};
var cboReasonCD_Change = function (sender, value) {
    if (sender.hasFocus) {
        App.stoIN_TagDetail.suspendEvents();
        for (var i = 0; i < App.stoIN_TagDetail.data.length ; i++) {
            App.stoIN_TagDetail.data.items[i].set('ReasonCD', value);
        }
        App.stoIN_TagDetail.resumeEvents();
        App.grdIN_TagDetail.view.refresh();
    }    
};

var cboSiteID_Change = function (sender, value) {
    if (sender.hasFocus) {
        App.cboWhseLoc.setValue('');
    }
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    // App.cboInvtID.store.reload();    
    App.cboWhseLoc.store.clearFilter();
    App.cboInvtID.getStore().load(function () {
        
        HQ.common.showBusy(false);
    });
};


var cboWhseLoc_Change = function (sender, value) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));  
    App.cboInvtID.getStore().load(function () {        
        HQ.common.showBusy(false);
    });
};

//load lần đầu khi mở
var firstLoad = function () {   
    HQ.isFirstLoad = true;    
    App.cboTagID.getStore().addListener('load', cboTagStoreLoad);
  //  App.stoIN_TagHeader.getStore().addListener('load', cboTagStoreLoad);
    loadSourceCombo();
    HQ.util.checkAccessRight();
    if (HQ.showType) {
        App.cboType.setVisible(true);
    }
    else {
        App.cboType.setVisible(false);
    }
    if (HQ.showWhseLoc == 0) {
        App.cboWhseLoc.setVisible(false);
        App.cboWhseLoc.allowBlank = true;
        App.colWhseLoc.hide();
    }
    else {
        App.colWhseLoc.show();
        App.cboWhseLoc.setVisible(true);
        if (HQ.showWhseLoc == 2) {
            App.cboWhseLoc.allowBlank = false;
        }
        else {
            App.cboWhseLoc.allowBlank = true;
        }        
    }
    if (HQ.showColWhseLoc == 2) {
        keys = ['LineRef', 'InvtID'];
    }

    if (HQ.showColSiteID) {
        App.colSiteID.show();
    }
    else {
        App.colSiteID.hide();
    }
    if (HQ.showColWhseLoc == 0) {
        App.colWhseLoc.hide();
    }
    else {
        App.colWhseLoc.show();        
    }


    App.cboWhseLoc.isValid();
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
            //App.cboSiteID.setReadOnly(App.stoIN_TagDetail.data.length > 0 && HQ.store.isChange(App.stoIN_TagDetail));

            isSetSite = true;
        }
        else {
            App.cboTagID.setReadOnly(HQ.isChange);
        }
    }
    if (!isSetSite) {
        App.cboSiteID.setReadOnly(!HQ.isNew);
        App.cboWhseLoc.setReadOnly(!HQ.isNew);
    }

    var lstData = App.stoIN_TagDetail.snapshot || App.stoIN_TagDetail.allData || App.stoIN_TagDetail.data;
    if (lstData != undefined) {
        var key=false;
        for (var i = 0; i < lstData.length; i++) {
            if (!Ext.isEmpty(lstData.items[i].data.InvtID)) {
                key = true;
                break;
            }
        }

        App.cboSiteID.setReadOnly(key);
        App.cboWhseLoc.setReadOnly(key);
        App.cboClassID.setReadOnly(key);
    }
};

//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdIN_TagDetail_BeforeEdit = function (editor, e) {
    if (e.field == "WhseLoc") {
        if (!HQ.editSiteWhseLoc) {
            return false;
        }
    }
    if (e.field == "SiteID") {
        if (!HQ.editSiteWhseLoc) {
            return false;
        }
    }

    if (e.field == "WhseLoc") {
        App.cboWhseLoc1.store.clearFilter();
        App.cboWhseLoc1.store.filter('SiteID', new RegExp('^' +e.record.data.SiteID + '$'));
    }

    if (App.cboStatus.getValue() != _beginStatus) {
        return false;
    }
    else {
        //if (App.stoIN_TagDetail.data.length > 1) {
        //    if (App.stoIN_TagDetail.data.items[0].data.SiteID != App.cboSiteID.getValue()) {
        //        HQ.message.show(2016102101);
        //        return false;
        //    }
        //} else {
        //    if (e.record.data.InvtID != '' && e.record.data.SiteID != App.cboSiteID.getValue()) {
        //        HQ.message.show(2016102101);
        //        return false;
        //    }
        //}
        
        return HQ.grid.checkBeforeEdit(e, keys);
    }
};

var grdIN_TagDetail_Edit = function (item, e) {

    if (e.field == "ActualEAQty") {
        var record1 = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [e.record.data.InvtID]);
        if (record1.LotSerTrack != "N") {
            checkExitEdit(e);
        }        
        e.record.set("OffsetEAQty", e.record.data.ActualEAQty - e.record.data.BookEAQty);
    }
    if (e.field == 'InvtID') {
        var record = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [e.value]);
        var record12 = HQ.store.findInStore(App.cboInvtCheck.store, ['InvtID','SiteID','WhseLoc'], [e.value,e.record.data.SiteID,e.record.data.WhseLoc]);
        if (record) {
            if (e.record.data.LineRef == "" || e.record.data.LineRef == null) {
                e.record.set('LineRef', HQ.store.lastLineRef(App.grdIN_TagDetail.store));
            }            
            e.record.set('InvtName', record.InvtName);
            e.record.set('SiteID', '');
            if (HQ.showWhseLoc != 0) {
                e.record.set('WhseLoc', '');
            }            
            e.record.set('EAUnit', record.EAUnit);
            if (record12 != undefined) {
                e.record.set('BookEAQty', record12.BookEAQty);
            }            
            e.record.set("OffsetEAQty", e.record.data.ActualEAQty - e.record.data.BookEAQty);
            e.record.set('ReasonCD', App.cboReasonCD.getValue());
            e.record.set('StkItem', record.StkItem);
        }
    }

    if (e.field == 'SiteID') {
        var record12 = HQ.store.findInStore(App.cboInvtCheck.store, ['InvtID', 'SiteID', 'WhseLoc'], [e.record.data.InvtID, e.record.data.SiteID, e.record.data.WhseLoc]);
        if (record12 != undefined) {
            e.record.set('BookEAQty', record12.BookEAQty);
        }
        e.record.set('WhseLoc', '');
        App.grdLot.store.clearFilter();        
        var lstLot = App.grdLot.store.snapshot || App.grdLot.store.data;
        if (lstLot != undefined) {
            for (var j = lstLot.length - 1; j >= 0; j--) {
                if (lstLot.items[j].data.InvtID == e.record.data.InvtID) {
                    lstLot.items[j].data.WhseLoc = e.record.data.WhseLoc;
                    lstLot.items[j].data.SiteID = e.record.data.SiteID;
                }
            }
        }        
    }
    if (e.field == 'WhseLoc') {
        var record12 = HQ.store.findInStore(App.cboInvtCheck.store, ['InvtID', 'SiteID', 'WhseLoc'], [e.record.data.InvtID, e.record.data.SiteID, e.record.data.WhseLoc]);
        if (record12 != undefined) {
            e.record.set('BookEAQty', record12.BookEAQty);
        }
        App.grdLot.store.clearFilter();
        var lstLot = App.grdLot.store.snapshot || App.grdLot.store.data;
        if (lstLot != undefined) {
            for (var j = lstLot.length - 1; j >= 0; j--) {
                if (lstLot.items[j].data.InvtID == e.record.data.InvtID) {
                    lstLot.items[j].data.WhseLoc = e.record.data.WhseLoc;
                    lstLot.items[j].data.SiteID = e.record.data.SiteID;
                }
            }
        }
    }
    // Calc Total Actual Qty
    calcTotQty();
    sumTotal();
    HQ.grid.checkInsertKey(App.grdIN_TagDetail, e, keys);
    HQ.isNew = false;
    frmChange();
};

var grdIN_TagDetail_ValidateEdit = function (item, e) {
    return checkValidateEdit(App.grdIN_TagDetail, e, ["InvtID", "SiteID", "WhseLoc"]);
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
                    App.cboBranchID.getValue(), App.cboSiteID.getValue(),App.cboClassID.getValue(),App.cboWhseLoc.getValue(),
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
                App.stoLot.reload();
            }
        }
    }
};
function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    var lstData = App.grdIN_TagDetail.store.snapshot || App.grdIN_TagDetail.store.allData || App.grdIN_TagDetail.store.data;
    var checkSiteID = false;
    var checkWhseLoc = false;
    if (lstData != undefined) {
        for (var i = 0; i < lstData.length; i++) {
            if (lstData.items[i].data.InvtID != "" && lstData.items[i].data.SiteID == "" && HQ.editSiteWhseLoc && HQ.showColSiteID) {
                checkSiteID = true;
            }
            if (lstData.items[i].data.InvtID != "" && lstData.items[i].data.WhseLoc == "" && HQ.editSiteWhseLoc && HQ.showColWhseLoc == 2) {
                checkWhseLoc = true;
            }
        }
    }
    if (checkSiteID) {
        HQ.message.show(1000, HQ.common.getLang('SiteID'), '', false);
        return false;
    }
    if (checkWhseLoc) {
        HQ.message.show(1000, HQ.common.getLang('WhseLoc'), '', false);
        return false;
    }

    var isSave = !(App.cboStatus.getValue() == 'C' && (!App.cboHandle.getValue() || App.cboHandle.getValue() == '' || App.cboHandle.getValue() == 'N'));
    if (HQ.form.checkRequirePass(App.frmMain) && isSave) {
        HQ.isSaving = true;
        var siteName = '';
        var objSite = HQ.store.findInStore(App.cboSiteID.store, ['SiteID'], [App.cboSiteID.getValue()]);
        if (objSite) {
            siteName = objSite.Name;
        }
        App.grdLot.store.clearFilter();
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            timeOut: 180000,
            url: 'IN10500/Save',
            params: {
                lstIN_TagHeader: Ext.encode(App.stoIN_TagHeader.getRecordsValues()),
                lstIN_TagDetail: Ext.encode(App.stoIN_TagDetail.getRecordsValues()),//Ext.encode(App.stoIN_TagDetail.getChangedData({ skipIdForPhantomRecords: false })), 
                lstDel: Ext.encode(App.stoIN_TagDetail.getChangedData().Deleted),
                lstIN_TagLot: Ext.encode(App.stoLot.getRecordsValues()),
                lstTagLotDel: Ext.encode(App.stoLot.getRecordsValues().Deleted),
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
                    App.stoLot.reload();
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
            var lstDataDel = App.grdIN_TagDetail.selModel.selected.items;
            App.grdLot.store.clearFilter();
            if (lstDataDel != undefined) {
                for (var i = 0; i < lstDataDel.length; i++) {
                    var lstLot = App.grdLot.store.snapshot || App.grdLot.store.data;
                    if (lstLot != undefined) {
                        for (var j = lstLot.length-1; j >= 0; j--) {
                            if (lstLot.items[j].data.InvtID == lstDataDel[i].data.InvtID && lstLot.items[j].data.LineRef == lstDataDel[i].data.LineRef) {
                                App.stoLot.remove(lstLot.items[j]);
                            }                            
                        }
                    }
                }
            }
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
        if (App.cboTagID.valueModels == null || App.cboTagID.valueModels[0] == null)
        {
            App.cboTagID.setValue(ID);
        }
        //App.cboTagID.getStore().load(function () {
          //  App.cboTagID.setValue(ID);
        App.stoIN_TagHeader.reload();
        App.stoLot.reload();
       // });
    }
};
function refreshDet(item) {
    if (item == 'yes') {
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.stoIN_TagDetail.reload();
        App.stoLot.reload();
    }
};

// Render Rownumber
var renderRowNumber= function (value, meta, record) {
    return App.stoIN_TagDetail.data.indexOf(record) + 1;
}

// Render Rownumber
var renderLotRowNumber = function (value, meta, record) {
    //return App.stoIN_TagDetail.data.indexOf(record) + 1;
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

var btnShowReport_Click = function () {
    if (App.cboReport.validate()) {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            method: 'POST',
            url: 'IN10500/Report',
            timeout: 180000,
            params: {
                lstIN_TagHeader: Ext.encode(App.stoIN_TagHeader.getRecordsValues()),
                reportNbr: App.cboReport.valueModels[0].data.ReportNbr,
                reportName: App.cboReport.valueModels[0].data.ReportName
            },
            success: function (msg, data) {
                if (this.result.reportID != null) {
                    window.open('Report?ReportName=' + this.result.reportName + '&_RPTID=' + this.result.reportID, '_blank');
                }
                App.winReport.close();
                HQ.message.process(msg, data, true);
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }

}
///////////////////////////////////

var btnImport_Click = function (c, e) {
    if (Ext.isEmpty(App.cboSiteID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('siteid')], '', true);
        App.btnImport.reset();
        return;
    }
    if (Ext.isEmpty(App.cboBranchID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('BranchID')], '', true);
        App.btnImport.reset();
        return;
    }
    if (!App.cboTagID.getValue()) {
        App.direct.IN10500_pdCheckCreateIN_Tag(
            App.cboBranchID.getValue(), App.cboSiteID.getValue(), App.cboClassID.getValue(),App.cboWhseLoc.getValue(),
            {
                success: function (result) {
                    if (result == '') {
                        importData(c);
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
        importData(c);
    }
};
function importData(c)
{
    var fileName = c.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            clientValidation: false,
            method: 'POST',
            url: 'IN10500/Import',
            timeout: 1000000,
            params: {
            },
            success: function (msg, data) {
                if (this.result.data.lstData != undefined) {

                    this.result.data.lstData.forEach(function (item) {
                        var objTrans = HQ.store.findRecord(App.stoIN_TagDetail, ['InvtID', 'SiteID', 'WhseLoc'], [item.InvtID, item.SiteID, item.WhseLoc]);
                        var record = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [item.InvtID]);
                        var recordInvt = HQ.store.findInStore(App.cboInvtCheck.store, ['InvtID', 'SiteID', 'WhseLoc'], [item.InvtID, item.SiteID, item.WhseLoc]);
                        if (!objTrans) {
                            if (record) {
                                HQ.store.insertRecord(App.stoIN_TagDetail, "InvtID", Ext.create('App.mdlIN10500_pgLoadGrid'), true);
                                var newTrans = App.stoIN_TagDetail.data.items[App.stoIN_TagDetail.getCount() - 1];
                                newTrans.set('BranchID', App.cboBranchID.getValue());
                                newTrans.set('InvtID', record.InvtID);
                                newTrans.set('InvtName', record.InvtName);
                                newTrans.set('SiteID', item.SiteID);
                                newTrans.set('WhseLoc', item.WhseLoc);
                                newTrans.set('EAUnit', record.EAUnit);
                                if (recordInvt != undefined) {
                                    newTrans.set('BookEAQty', recordInvt.BookEAQty);
                                    newTrans.set("OffsetEAQty", item.ActualEAQty - recordInvt.BookEAQty);
                                }
                                else {
                                    newTrans.set('BookEAQty', record.QtyAvail);
                                    newTrans.set("OffsetEAQty", item.ActualEAQty );
                                }
                                newTrans.set('ReasonCD', App.cboReasonCD.getValue());
                                newTrans.set('ActualEAQty', item.ActualEAQty);
                                newTrans.set('StkItem', record.StkItem);
                                newTrans.set('LineRef', HQ.store.lastLineRef(App.stoIN_TagDetail));
                            }
                        }
                        else {
                            objTrans.set('ActualEAQty', item.ActualEAQty);
                            objTrans.set('OffsetEAQty', item.ActualEAQty - objTrans.data.BookEAQty);
                        }
                    });
                    if (!Ext.isEmpty(this.result.data.message)) {
                        HQ.message.show('2013103001', [this.result.data.message], '', true);
                    } else {
                        HQ.message.process(msg, data, true);
                    }
                } else {
                    HQ.message.process(msg, data, true);
                }

                if (this.result.data.lstDataLot != undefined) {

                    this.result.data.lstDataLot.forEach(function (item) {
                        var objLot = HQ.store.findRecord(App.stoLot, ['InvtID', 'SiteID', 'WhseLoc', 'LotSerNbr'], [item.InvtID, item.SiteID, item.WhseLoc, item.LotSerNbr]);
                        var objDetail = HQ.store.findRecord(App.stoIN_TagDetail, ['InvtID', 'SiteID', 'WhseLoc'], [item.InvtID, item.SiteID, item.WhseLoc]);
                        var record = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [item.InvtID]);
                        var recordLot = HQ.store.findInStore(App.cboInvtoryLot.store, ['InvtID', 'SiteID', 'WhseLoc', 'LotSerNbr'], [item.InvtID, item.SiteID, item.WhseLoc, item.LotSerNbr]);
                        if (!objLot) {
                            if (record) {
                                HQ.store.insertRecord(App.stoLot, "LotSerNbr", Ext.create('App.mdlLot'), true);
                                var newLot = App.stoLot.data.items[App.stoLot.getCount() - 1];
                                newLot.set('BranchID', App.cboBranchID.getValue());
                                newLot.set('InvtID', record.InvtID);
                                newLot.set('SiteID', item.SiteID);
                                newLot.set('WhseLoc', item.WhseLoc);
                                newLot.set('UnitDesc', record.EAUnit);
                                if (recordLot != undefined) {
                                    newLot.set('BookEAQty', recordLot.QtyAvail);
                                    newLot.set("OffsetEAQty", item.ActualEAQty - recordLot.QtyAvail);
                                }
                                else {
                                    newLot.set('BookEAQty', 0);
                                    newLot.set("OffsetEAQty", item.ActualEAQty);
                                }
                                
                                
                                newLot.set('ActualEAQty', item.ActualEAQty);
                                newLot.set('StkItem', record.StkItem);
                                newLot.set('LotSerNbr', item.LotSerNbr);
                                newLot.set('LineRef', objDetail.data.LineRef)
                            }
                        }
                        else {
                            objLot.set('ActualEAQty', item.ActualEAQty);
                            objLot.set('OffsetEAQty', item.ActualEAQty - objLot.data.BookEAQty);
                        }
                    });
                    if (!Ext.isEmpty(this.result.data.message)) {
                        HQ.message.show('2013103001', [this.result.data.message], '', true);
                    } else {
                        HQ.message.process(msg, data, true);
                    }
                }

                App.btnImport.reset();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.btnImport.reset();
            }
        });
    } else {
        HQ.message.show('2014070701', [ext], '', true);
        App.btnImport.reset();
    }
}
var btnExport_Click = function () {
    //App.frmMain.submit({
    //    url: 'IN10500/Export',
    //    timeout: 1000000,
    //    clientValidation: false,
    //    success: function (msg, data) {
    //    },
    //    failure: function (msg, data) {
    //        HQ.message.process(msg, data, true);
    //    }
    //});
    App.frmMain.submit({
        waitMsg: HQ.common.getLang("Exporting"),
        timeout: 1000000,
        url: 'IN10500/ExportFileName',
        clientValidation: false,
        success: function (returnValue, data) {
            if (!Ext.isEmpty(data.result.id)) {
                exportExcelFile(data.result.id, data.result.name);
            }
        },
        failure: function (msg, data) {

            HQ.message.process(msg, data, true);
        }
    });
};
function exportExcelFile(idfile, name) {
    App.frmMain.submit({
        waitMsg: HQ.common.getLang("Exporting"),
        url: 'IN10500/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
            id: idfile,
            name: name
        },
        success: function (returnValue, data) {
            window.location = 'IN10500/DownloadAndDelete?name=' + data.result.name + '&id=' + data.result.id;
        },
        failure: function (msg, data) {
            if (data.failureType == "connect")
                downloadFile(idfile, name);
            else HQ.message.process(msg, data, true);
        }
    });
}
function downloadFile(idfile, name) {

    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("CheckingFile"),
            method: 'POST',
            timeout: 18000000,
            url: 'IN10500/CheckFile',
            params: {
                name: name,
                id: idfile
            },
            success: function (msg, data) {
                if (!Ext.isEmpty(data.result.name)) {
                    window.location = 'RPT/DownloadAndDelete?name=' + data.result.name + '&id=' + data.result.id;
                }
            },
            failure: function (msg, data) {
                if (data.failureType == "connect")
                    downloadFile(idfile, name);
                else HQ.message.process(msg, data, true);
            }
        });
    }
}


function btnLotDel_Click() {
    if ((App.cboTagID.value && HQ.isUpdate) || (!App.cboTagID.value && HQ.isInsert)) {
        if (App.cboStatus.getValue() != "H") {
            HQ.message.show(2015020805, [App.cboTagID.getValue()], '', true);
            return;
        }
        if (App.smlLot.selected.items.length != 0) {
            if (!Ext.isEmpty(App.smlLot.selected.items[0].data.LotSerNbr)) {
                HQ.message.show(2015020806, [App.smlLot.selected.items[0].data.InvtID + ' ' + App.smlLot.selected.items[0].data.LotSerNbr], 'deleteLot', true);
            }
        }
    }
};

var btnLotOK_Click = function () {
    if (!App.grdLot.isLock) {
        var det = App.winLot.record.data;
        var flat = null;
        App.stoLot.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                //if (item.data.ActualEAQty == 0) {
                //    HQ.message.show(1000, [HQ.common.getLang('ActualEAQty')], '', true);
                //    flat = item;
                //    return false;
                //}
                //// no check qty Lot = 0
                if (Ext.isEmpty(item.data.UnitDesc)) {
                    HQ.message.show(1000, [HQ.common.getLang('unit')], '', true);
                    flat = item;
                    return false;
                }

                //if (Ext.isEmpty(item.data.UnitMultDiv)) {
                //    HQ.message.show(2525, [invtID], '', true);
                //    flat = item;
                //    return false;
                //}
            }
        });
        if (!Ext.isEmpty(flat)) {
            App.smlLot.select(App.stoLot.indexOf(flat));
            return;
        }

        var actualEAQty = 0;
        App.stoLot.data.each(function (item) {
            if (!Ext.isEmpty(item.data.LotSerNbr)) {
                if (item.data.SiteID == det.SiteID && item.data.InvtID == det.InvtID && item.data.TAGID == det.TAGID && det.LineRef==item.data.LineRef && det.SiteID==item.data.SiteID) {
                    actualEAQty += item.data.ActualEAQty;
                }
            }
        });

        var lineQty = actualEAQty;

        App.winLot.record.data.ActualEAQty = Math.round(lineQty);
        App.winLot.record.data.OffsetEAQty = Math.round(lineQty) - App.winLot.record.data.BookEAQty;
        //App.winLot.record.commit();

        App.grdIN_TagDetail.view.refresh();

        calculate();

        for (i = App.stoLot.data.items.length - 1; i >= 0; i--) {
            if (Ext.isEmpty(App.stoLot.data.items[i].data.LotSerNbr)) {
                App.stoLot.data.removeAt(i);
            }
        }
    }
    App.winLot.hide();
    sumTotal();
};

function renderQtyAmt(value) {
    return Ext.util.Format.number(value, '0,000');
};


var btnLot_Click = function () {
    if (Ext.isEmpty(this.record.invt)) {
        App.cboInvtID.store.clearFilter();
        var rcInvt = HQ.store.findRecord(App.cboInvtID.store, ['InvtID'], [this.record.data.InvtID]);
        this.record.invt = rcInvt.data;
    }

    if (!Ext.isEmpty(this.record.invt.LotSerTrack) && this.record.invt.LotSerTrack != 'N' && !Ext.isEmpty(this.record.data.EAUnit)) {
        showLot(this.record, true);
    }
};

var sumTotal = function () {
    var lstData = App.grdIN_TagDetail.store.snapshot || App.grdIN_TagDetail.store.allData || App.grdIN_TagDetail.store.data;
    var qty = 0;
    if (lstData != undefined) {
        for (var i = 0; i < lstData.length; i++) {
            qty = qty + lstData.items[i].data.ActualEAQty;
        }
    }
    App.txtTotQty.setValue(qty);
};



var showLot = function (record, loadCombo) {
    var lock = !((App.cboTagID.value && HQ.isUpdate) || (!App.cboTagID.value && HQ.isInsert)) || App.cboStatus.getValue() != "H";
    App.grdLot.isLock = lock;
    if (loadCombo) {
        App.stoCalcLot.load({
            params: {
                siteID: record.data.SiteID,
                invtID: record.data.InvtID,
                branchID: App.cboBranchID.getValue(),
                tagID: App.cboTagID.getValue(),
                whseLoc: App.cboWhseLoc.getValue(),
                showWhseLoc: HQ.showWhseLoc
            }
        });
    }
    App.stoLot.clearFilter();
    App.stoLot.filter('InvtID', record.data.InvtID);
    App.stoLot.filter('LineRef', record.data.LineRef);
    var newRow = Ext.create('App.mdlLot');
    newRow.data.SiteID = record.data.SiteID;
    newRow.data.SiteID = record.data.WhseLoc;
    newRow.data.UnitDesc = record.data.EAUnit;
    newRow.data.ExpDate = '';
    newRow.data.UnitPrice = 0;
    newRow.data.CnvFact = 1;
    newRow.data.UnitMultDiv = 'M';
    newRow.data.TranDate = App.dtpTranDate.getValue();
    HQ.store.insertRecord(App.stoLot, "LotSerNbr", newRow, true);
    App.winLot.record = record;
    App.grdLot.view.refresh();
    HQ.focus = '';
    App.winLot.show();
    setTimeout(function () { App.winLot.toFront(); }, 50);
};


var grdLot_BeforeEdit = function (item, e) {
    if (App.cboStatus.getValue() == "R") {
        return false;
    }

    if (App.grdLot.isLock) {
        return false;
    }
    var key = e.field;
    var record = e.record;
    if ((key != 'LotSerNbr' && key != 'ActualEAQty') && Ext.isEmpty(e.record.data.LotSerNbr)) return false;
    if (key == 'LotSerNbr' && !Ext.isEmpty(record.data.LotSerNbr)) return false;
    if (key == "UnitDesc") return false;

    if (Ext.isEmpty(record.data.InvtID)) {
        record.data.InvtID = App.winLot.record.data.InvtID;
        record.data.SiteID = App.winLot.record.data.SiteID;
    }
    record.commit();
    App.cboLotUnitDesc.setValue('');
    if (key == 'ExpDate') {
        if (record.data.tstamp)
            return false;
        else {
            if (e.record.data.LotSerNbr) {
                var objFind = App.stoCalcLot.findRecord(['LotSerNbr'], [e.record.data.LotSerNbr]);
                if (objFind)
                    return false;
                else
                    return true;
            }
        }
    }
};

var checkSelectLot = function (records, options, success) {
    HQ.numSelectLot++;
    if (HQ.numSelectLot == HQ.maxSelectLot) {
        App.grdLot.view.loadMask.hide();
        App.grdLot.view.loadMask.setDisabled(false)
        getLotQtyAvail(options.row);
    }
};

var grdLot_SelectionChange = function (item, selected) {
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectLot = 0;
            HQ.maxSelectLot = 1;
            App.grdLot.view.loadMask.show();
            App.stoItemLot.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, branchID: App.cboBranchID.getValue(), lotSerNbr: selected[0].data.LotSerNbr, tagID: App.cboTagID.getValue(), whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSelectLot,
                row: selected[0]
            });
        } else {
            App.lblLotQtyAvail.setText('');
        }
    }
};
var grdLot_Edit = function (item, e) {
    var key = e.field;
    var lot = e.record.data;
    var record = e.record;
    e.record.set('WhseLoc', App.winLot.record.data.WhseLoc);
    e.record.set('LineRef', App.winLot.record.data.LineRef);
    e.record.set('SiteID', App.winLot.record.data.SiteID);
    e.record.set('TAGID', App.cboTagID.getValue());
    e.record.set('BranchID', App.cboBranchID.getValue());
    if (Object.keys(e.record.modified).length > 0) {
        // if (key == "Qty" || key == "UnitDesc") {
        if (key == "ActualEAQty" && !Ext.isEmpty(e.record.data.LotSerNbr)) {
            checkExitEditLot(e);
        } else if (key == "LotSerNbr") {
            var objLotSerNbr = HQ.store.findInStore(App.stoCalcLot, ["LotSerNbr", "SiteID", "WhseLoc"], [e.value, App.winLot.record.data.SiteID, App.winLot.record.data.WhseLoc]);
            if (objLotSerNbr != null) {
                e.record.set("TranDate", App.dtpTranDate.getValue());
                e.record.set("ExpDate", objLotSerNbr.ExpDate);
                e.record.set("BookEAQty", objLotSerNbr.BookEAQty);
                e.record.set("OffsetEAQty", objLotSerNbr.OffsetEAQty);
            }
            App.grdLot.view.loadMask.show();
            HQ.numLot = 0;
            HQ.maxLot = 1;
            App.stoItemLot.load({
                params: { siteID: lot.SiteID, invtID: lot.InvtID, branchID: App.cboBranchID.getValue(), lotSerNbr: lot.LotSerNbr, tagID: App.cboTagID.getValue(), whseLoc: App.cboWhseLoc.getValue(), showWhseLoc: HQ.showWhseLoc },
                callback: checkSourceEditLot,
                row: e
            });
        }
    }
    if (e.field == "ActualEAQty" && e.value > 0 && Ext.isEmpty(e.record.data.LotSerNbr)) {
        var objIN_Inventory = HQ.store.findInStore(App.stoIN10500_pdIN_Inventory, ["InvtID"], [App.winLot.record.data.InvtID]);
        objIN_Inventory = objIN_Inventory == null ? "" : objIN_Inventory;

        if (App.cboStatus.getValue() != "C") {
            if (objIN_Inventory.LotSerRcptAuto || Ext.isEmpty(e.record.data.LotSerNbr)) {
                if (Ext.isEmpty(e.record.data.LotSerNbr)) {
                    HQ.common.showBusy(true, '', App.winLot);
                    App.direct.INNumberingLot(
                        App.winLot.record.data.InvtID, Ext.Date.format(App.dtpTranDate.getValue(), 'Y-m-d'), 'LotNbr',
                        {
                            success: function (result) {
                                e.record.set("LotSerNbr", result);
                                e.record.set('WhseLoc', App.winLot.record.data.WhseLoc);
                                e.record.set('LineRef', App.winLot.record.data.LineRef);
                                e.record.set('SiteID', App.winLot.record.data.SiteID);
                                e.record.set('WarrantyDate', App.dtpTranDate.getValue());
                                e.record.set('ExpDate', App.dtpTranDate.getValue());
                                e.record.set('UnitDesc', App.winLot.record.data.EAUnit);
                                e.record.set('CnvFact', 1);
                                e.record.set('UnitMultDiv', 'M');
                                e.record.set('TranDate', App.dtpTranDate.getValue());
                                e.record.set('BookEAQty', 0);
                                e.record.set('OffsetEAQty', e.record.data.ActualEAQty-e.record.data.BookEAQty);
                                HQ.common.showBusy(false, '', App.winLot);
                                if (!Ext.isEmpty(e.record.data.LotSerNbr)) {
                                    addNewLot(record);
                                }

                            },
                            failure: function (result) {
                                HQ.common.showBusy(false, '', App.winLot);
                            }
                        });

                }
            }
            else if (!Ext.isEmpty(objDetail.LotSerNbr)) {
                addNewLot(record);
            }
        }

        //else if (!Ext.isEmpty(objDetail.LotSerNbr)) {
        //    PopupWinLot.addNewLot(recordTran);
        //}
    }
    //else if (!Ext.isEmpty(objDetail.LotSerNbr)) {
    //    PopupWinLot.addNewLot(recordTran);
    //}
    if (e.field == "ActualEAQty") {
        e.record.set("OffsetEAQty",e.record.data.ActualEAQty-e.record.data.BookEAQty)
    }
};

var checkSourceEditLot = function (records, options, success) {
    HQ.numLot++;
    if (HQ.numLot == HQ.maxLot) {
        checkExitEditLot(options.row);
    }
};

var checkExitEditLot = function (row) {
    var key = row.field;
    var record = row.record;
    var lot = row.record.data;
    if (key == "ActualEAQty") {
        getLotQtyAvail(record);
    } else if (key == "UnitDesc") {
        var price = 0;
        var cnvFact = 0;
        var unitMultDiv = "M";
        var cnv = setUOM(App.winLot.record.invt.InvtID, App.winLot.record.invt.ClassID, App.winLot.record.invt.StkUnit, lot.UnitDesc);
        if (!Ext.isEmpty(cnv)) {
            cnvFact = cnv.CnvFact;
            unitMultDiv = cnv.MultDiv;
            lot.CnvFact = cnvFact;
            lot.UnitMultDiv = 'M';
        } else {
            lot.CnvFact = 1;
            lot.UnitMultDiv = 'M';
            lot.UnitPrice = 0;
            lot.UnitDesc = '';
            record.commit();
            App.grdLot.view.loadMask.hide();
            App.grdLot.view.loadMask.setDisabled(false)
            return;
        }

        if (App.winLot.record.invt.ValMthd == "A" || App.winLot.record.invt.ValMthd == "E") {
            var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [lot.InvtID, lot.SiteID]);
            price = site.AvgCost * lot.CnvFact;
            lot.UnitPrice = lot.UnitCost = price;

        } else {
            lot.UnitPrice = lot.UnitCost = 0
        }

        getLotQtyAvail(record);
    } else if (key == "LotSerNbr") {
        var flat = false;
        App.stoLot.data.each(function (item) {
            if (item.data.LotSerNbr == lot.LotSerNbr && item.id != record.id) {
                flat = true;
                return false;
            }
        });
        if (flat) {
            HQ.message.show(219, "", "", true);
            lot.LotSerNbr = "";
            App.grdLot.view.loadMask.hide();
            App.grdLot.view.loadMask.setDisabled(false)
            record.commit();
            return;
        }
        lot.UnitDesc = App.winLot.record.data.EAUnit;
        lot.UnitPrice = lot.UnitCost = 0;
        lot.UnitMultDiv = App.winLot.record.data.UnitMultDiv;
        lot.CnvFact = App.winLot.record.data.CnvFact;
        var itemLot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', 'LotSerNbr', 'WhseLoc'], [lot.InvtID, lot.SiteID, lot.LotSerNbr, App.cboWhseLoc.getValue()]);
        if (!Ext.isEmpty(itemLot)) {
            lot.ExpDate = itemLot.ExpDate;
        }

        if (!Ext.isEmpty(lot.LotSerNbr)) {
            var newRow = Ext.create('App.mdlLot');
            newRow.data.LotSerNbr = "";
            HQ.store.insertRecord(App.stoLot, key, newRow, true);
        }
        getLotQtyAvail(record);
    }
    record.commit();
    App.grdLot.view.loadMask.hide();
    App.grdLot.view.loadMask.setDisabled(false)
};


var getLotQtyAvail = function (row) {
    var lot = HQ.store.findInStore(App.stoItemLot, ['InvtID', 'SiteID', ['LotSerNbr'], 'WhseLoc'], [row.data.InvtID, row.data.SiteID, row.data.LotSerNbr, App.cboWhseLoc.getValue()]);
    var actualEAQty = 0;
    var qtyAvail = 0;

    App.stoLot.snapshot.each(function (item2) {
        if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
            actualEAQty += item2.data.ActualEAQty;
        }
    });
    var det = App.winLot.record;

    //if (lot != undefined) {
    //    if ((lot.QtyAvail - qty) < 0) {
    //        HQ.message.show(1043, [row.data.InvtID + " " + row.data.LotSerNbr, row.data.SiteID], "", true);
    //        return false;
    //    }
    //}

    if (!Ext.isEmpty(lot)) {
        if (actualEAQty <= 0) {
            actualEAQty = Math.abs(actualEAQty);
            qtyAvail = lot.QtyAvail - actualEAQty;
            if (qtyAvail < 0) {
                HQ.message.show(1043, [row.data.InvtID + " " + row.data.LotSerNbr, row.data.SiteID], "", true);
                actualEAQty = 0;
                row.data.ActualEAQty = 0;
                row.commit();
                App.stoLot.snapshot.each(function (item2) {
                    if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
                        actualEAQty += item2.data.ActualEAQty;
                    }
                });
                if (actualEAQty < 0) {
                    actualEAQty = Math.abs(actualEAQty);
                }
                qtyAvail = lot.QtyAvail - actualEAQty;
            }
        }
        else {
            qtyAvail = lot.QtyAvail;
        }
    }
    else {
        if (actualEAQty <= 0) {
            actualEAQty = Math.abs(actualEAQty)
            qtyAvail = 0 - actualEAQty;
            if (qtyAvail < 0) {
                HQ.message.show(1043, [row.data.InvtID + " " + row.data.LotSerNbr, row.data.SiteID], "", true);
                actualEAQty = 0;
                row.data.ActualEAQty = 0;
                row.commit();
                App.stoLot.snapshot.each(function (item2) {
                    if (item2.data.LotSerNbr == row.data.LotSerNbr && item2.data.InvtID == row.data.InvtID && item2.data.SiteID == row.data.SiteID && item2.data.WhseLoc == row.data.WhseLoc) {
                        actualEAQty += item2.data.ActualEAQty;
                    }
                });
                qtyAvail = 0 - actualEAQty;
            }
        }
        else {
            qtyAvail = 0;
        }
    }    
};

var calculate = function () {
    //var totAmt = 0;
    //var totQty = 0;

    //App.stoIN_TagDetail.data.each(function (item) {
    //    totAmt += item.data.TranAmt;
    //    totQty += item.data.Qty;
    //});
    //App.txtTotAmt.setValue(totAmt);
    //App.txtTotQty.setValue(totQty);

};




var calcLot = function (record, show) {
    var record1 = HQ.store.findInStore(App.cboInvtID.store, ['InvtID'], [record.data.InvtID]);
    if (!Ext.isEmpty(record1) && !Ext.isEmpty(record1.LotSerTrack) && record1.LotSerTrack != 'N' && !Ext.isEmpty(record.data.EAUnit)) {
        App.stoLot.clearFilter();
        App.stoLot.data.each(function (item) {
            if (item.data.TAGID == App.cboTagID.getValue()) {
                item.data.UnitDesc = record.data.EAUnit;
                item.data.CnvFact = 1;
                item.data.UnitMultDiv ="M";
                item.commit();
            }
        });
        if (show == true) {
            showLot(record, true);
        }

    }
};


var checkExitEdit = function (row) {
    var key = row.field;
    var trans = row.record.data;
    if (key == "ActualEAQty") {
        var invt = row.record.invt;
        var site = HQ.store.findInStore(App.stoItemSite, ['InvtID', 'SiteID'], [trans.InvtID, trans.SiteID]);

        if (Ext.isEmpty(site)) {
            site = Ext.create('App.mdlItemSite').data;
            site.SiteID = trans.SiteID;
            site.InvtID = trans.InvtID;
            trans.Qty = 0;
            row.record.commit();

            //HQ.message.show(2016042101, [trans.InvtID, trans.SiteID], '', true);

            //App.grdTrans.view.loadMask.hide();
            HQ.common.showBusy(false);
            //App.grdTrans.view.loadMask.setDisabled(false)
            return;
        }

        calcLot(row.record, true);
    }
    row.record.commit();


    //App.grdTrans.view.loadMask.hide();
    HQ.common.showBusy(false);
    //  App.grdTrans.view.loadMask.setDisabled(false)

};

var checkValidateEdit= function (grd, e, keys, isCheckSpecialChar) {
    if (keys.indexOf(e.field) != -1) {        
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            if (e.column.xtype == "datecolumn")
                HQ.message.show(1112, Ext.Date.format(e.value, e.column.format));
            else HQ.message.show(1112, e.value);
            return false;
        }

    }
}



var grdIN_TagDetail_SelectionChange = function (item, selected) {
    HQ.focus = 'trans';
    if (selected.length > 0) {
        if (!Ext.isEmpty(selected[0].data.InvtID)) {
            HQ.numSelectTrans = 0;
            // App.grdTrans.view.loadMask.show();
            HQ.common.showBusy(true, 'Process...');
            App.stoItemSite.load({
                params: { siteID: selected[0].data.SiteID, invtID: selected[0].data.InvtID, whseLoc: selected[0].data.WhseLoc, showWhseLoc: HQ.showWhseLoc },
                callback: checkSelect,
                row: selected[0]
            });
        } 
    }
};

var checkSelect = function (records, options, success) {
    //HQ.numSelectTrans++;
    //if (HQ.numSelectTrans == HQ.maxSelectTrans) {
        HQ.common.showBusy(false);
    //}
};


var addNewLot = function (record, lotSerNbr) {
    var newRow = Ext.create('App.mdlLot');

    newRow.data.LotSerNbr = !Ext.isEmpty(lotSerNbr) ? lotSerNbr : '';
    newRow.data.InvtID = record.data.LineRef;
    newRow.data.UnitDesc = App.winLot.record.data.EAUnit;
    newRow.data.InvtID = record.data.InvtID;
    newRow.data.SiteID = record.data.SiteID;
    newRow.data.CnvFact = record.data.RcptConvFact;
    newRow.data.UnitMultDiv = record.data.RcptMultDiv;
    newRow.data.TranType = newRow.data.TranType;
    newRow.data.InvtMult = newRow.data.TranType == "X" ? -1 : 1;
    newRow.data.WhseLoc = record.data.WhseLoc;
    newRow.data.UnitCost = record.data.UnitCost;
    newRow.data.MfcDate = HQ.bussinessDate;
    newRow.data.WarrantyDate = App.dtpTranDate.getValue();
    newRow.data.TranDate = App.dtpTranDate.getValue();
    newRow.data.ActualEAQty = 0;
    newRow.data.BookEAQty = 0;
    newRow.data.OffsetEAQty = 0;
    newRow.data.ExpDate = App.dtpTranDate.getValue();;
    HQ.store.insertRecord(App.stoLot, "LotSerNbr", newRow, true);
};

var deleteLot = function (item) {
    if (item == 'yes') {
        App.grdLot.deleteSelected();
    }
};
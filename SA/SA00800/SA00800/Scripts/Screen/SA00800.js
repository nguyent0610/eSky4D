////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var keys = ['ReportFormat'];
var fieldsCheckRequire = ["ReportFormat"];
var fieldsLangCheckRequire = ["ReportFormat"];

var _focusNo = 0;
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var VendID = '';

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_ReportControl.reload();

        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    HQ.util.checkAccessRight(); // Kiem tra quyen Insert Update Delete de disable button tren top bar
    HQ.isFirstLoad = true;
    App.frmMain.isValid(); // Require cac field yeu cau tren from

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboReportNbr.getStore().addListener('load', checkLoad);
    
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.combo.first(App.cboReportNbr, HQ.isChange);
            }
            else {
                HQ.grid.first(App.grdSYS_ReportParm);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboReportNbr, HQ.isChange);
            }
            else {
                HQ.grid.prev(App.grdSYS_ReportParm);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboReportNbr, HQ.isChange);
            }
            else {
                HQ.grid.next(App.grdSYS_ReportParm);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboReportNbr, HQ.isChange);
            }
            else {
                HQ.grid.last(App.grdSYS_ReportParm);
            }
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
                if (_focusNo == 0) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', 'refresh');
                    }
                    else {
                        App.cboReportNbr.setValue('');
                    }
                }
                else {
                    HQ.grid.insert(App.grdSYS_ReportParm, keys);
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
                    if (App.slmSYS_ReportParm.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdSYS_ReportParm);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSYS_ReportParm), ''], 'deleteData', true)
                    }
                }

            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoSYS_ReportControl, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            //HQ.common.close(this);
            break;
    }
};

var frmChange = function () {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = (HQ.store.isChange(App.stoSYS_ReportControl) == false ? HQ.store.isChange(App.stoSYS_ReportParm) : true);
        HQ.common.changeData(HQ.isChange, 'SA00800');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboReportNbr.valueModels == null || HQ.isNew == true)
            App.cboReportNbr.setReadOnly(false);
        else App.cboReportNbr.setReadOnly(HQ.isChange);
};

var stoLoad = function (sto) {
    HQ.isFirstLoad = true;

    HQ.isNew = false;
    App.cboReportNbr.forceSelection = true;
    //App.cboARDOCTYPE.forceSelection = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, keys);
        //record = sto.getAt(0);

        HQ.isNew = true;//record la new    
        App.cboReportNbr.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboReportNbr.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    
    App.cboReportFormat.store.reload();
    App.stoSYS_ReportParm.reload();

    if (!HQ.isInsert && HQ.isNew) {
        App.cboVendID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
};

//Truoc khi load store se hien Busy
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoadSYS_ReportParm = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
    HQ.common.showBusy(false);
};

var grdSYS_ReportParm_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_ReportParm_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_ReportParm, e, keys);
    frmChange();
};
var grdSYS_ReportParm_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_ReportParm, e, keys);
};
var grdSYS_ReportParm_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_ReportParm);
    //stoChanged(App.stoSys_CompanyAddr);
    frmChange();
};

var cboReportNbr_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSYS_ReportControl.store.loading) {
        App.stoSYS_ReportControl.reload();
        App.stoSYS_ReportParm.reload();
    }

    //App.cboReportNbr.setValue("");
};


var cboReportNbr_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoSYS_ReportControl.reload();
    }

    //App.cboReportNbr.setValue("");
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboReportNbr_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboReportNbr.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboReportNbr_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboReportNbr.setValue('');
    }
};

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlSYS_ReportParm') {
            _focusNo = 1;
        }
        else {
            _focusNo = 0;
        }
    });
};



/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'SA00800/Save',
            params: {
                lstSYS_ReportControl: Ext.encode(App.stoSYS_ReportControl.getChangedData({ skipIdForPhantomRecords: false })),
                lstSYS_ReportParm: Ext.encode(App.stoSYS_ReportParm.getChangedData({ skipIdForPhantomRecords: false })),
                isNew: HQ.isNew
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var ReportNbr = data.result.ReportNbr;
                App.cboReportNbr.getStore().load(function () {
                    App.cboReportNbr.setValue(ReportNbr);
                    App.stoSYS_ReportControl.reload();
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
                url: 'SA00800/DeleteAll',
                success: function (action, data) {
                    App.cboReportNbr.setValue("");
                    App.cboReportNbr.getStore().load(function () { cboReportNbr_Change(App.cboReportNbr); });
                },
                failure: function (action, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                }
            });
        }
        else {
            App.grdSYS_ReportParm.deleteSelected();
        }
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        if (App.cboReportNbr.valueModels == null) App.cboReportNbr.setValue('');
        App.cboReportNbr.getStore().load(function () {
            App.stoSYS_ReportControl.reload();
        });
        HQ.isFirstLoad = true;
        //App.stoSYS_ReportControl.reload();
    }
};
///////////////////////////////////
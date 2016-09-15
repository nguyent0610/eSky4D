////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 4;
var _isLoadMaster = false;
var TermsID = '';

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSI_Terms.reload();
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

    App.cboTermsID.getStore().addListener('load', checkLoad);
    App.ApplyTo.getStore().addListener('load', checkLoad);
    App.DiscType.getStore().addListener('load', checkLoad);
    App.DueType.getStore().addListener('load', checkLoad);
    
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboTermsID, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboTermsID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboTermsID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboTermsID, HQ.isChange);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', 'refresh');
                } else {
                    App.cboTermsID.setValue('');
                    App.stoSI_Terms.reload();
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboTermsID.getValue()) {
                    HQ.message.show(11, '', 'deleteData');
                } else {
                    menuClick('new');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
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

var frmChange = function () {
    if (App.stoSI_Terms.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoSI_Terms);
    HQ.common.changeData(HQ.isChange, 'SI21100');

    if (App.cboTermsID.valueModels == null || HQ.isNew == true)
        App.cboTermsID.setReadOnly(false);
    else
        App.cboTermsID.setReadOnly(HQ.isChange);
};

var stoLoad = function (sto) {
    HQ.isNew = false;

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "TermsID");
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        App.cboTermsID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboTermsID.focus(true); //focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    if (!HQ.isInsert && HQ.isNew) {
        App.cboTermsID.forceSelection = true;
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

var cboTermsID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSI_Terms.loading) {
        VendID = value;
        App.stoSI_Terms.reload();
    }
};

var cboTermsID_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoSI_Terms.loading) {
        TermsID = value;
        App.stoSI_Terms.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboTermsID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboTermsID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboTermsID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboTermsID.setValue('');
    }
};




/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        if (HQ.util.checkSpecialChar(App.cboTermsID.getValue()) == true) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("WaitMsg"),
                url: 'SI21100/Save',
                params: {
                    lstSI_Terms: Ext.encode(App.stoSI_Terms.getRecordsValues())
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    TermsID = data.result.TermsID;
                    HQ.isChange = false;
                    HQ.isFirstLoad = true;
                    App.cboTermsID.getStore().load({
                        callback: function () {
                            if (Ext.isEmpty(App.cboTermsID.getValue())) {
                                App.cboTermsID.setValue(TermsID);
                                App.stoSI_Terms.reload();
                            }
                            else {
                                App.cboTermsID.setValue(TermsID);
                                App.stoSI_Terms.reload();
                            }
                        }
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
        else {
            HQ.message.show(20140811, App.cboTermsID.fieldLabel);
            App.cboTermsID.focus();
            App.cboTermsID.selectText();
        }
    }
};

// Submit the deleted data into server side
var deleteData = function (item) {
    if (item == "yes") {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("DeletingData"),
                url: 'SI21100/DeleteAll',
                timeout: 7200,
                success: function (msg, data) {
                    App.cboTermsID.getStore().load();
                    menuClick("new");
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew)
            App.cboTermsID.setValue('');
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSI_Terms.reload();
    }
};

//var checkNumberDueIntrv = function () {
//    if (App.DueIntrv.value < 0) {
//        App.DueIntrv.setValue(0)
//    }
//};

//var checkNumberDiscPct = function () {
//    if (App.DiscPct.value < 0) {
//        App.DiscPct.setValue(0)
//    }
//};

//var checkNumberDiscIntrv = function () {
//    if (App.DiscIntrv.value < 0) {
//        App.DiscIntrv.setValue(0)
//    }
//};

///////////////////////////////////

var event_KeyDown = function (sender, e) {
    if ((e.ctrlKey == true && e.keyCode == 86) ||
            (((e.keyCode < 48 || e.keyCode > 57) && (e.keyCode < 96 || e.keyCode > 105) && (e.keyCode < 37 || e.keyCode > 40))
            && (e.keyCode != 8 && e.keyCode != 9 && e.keyCode != 35 && e.keyCode != 36 && e.keyCode != 46 && e.keyCode != 45 && e.keyCode != 110 && e.keyCode != 190 && e.ctrlKey == false)
            )
           ) {
        e.stopEvent();
    }
};
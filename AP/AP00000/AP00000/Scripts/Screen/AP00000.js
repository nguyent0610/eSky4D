﻿////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 4;
var _isLoadMaster = false;

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoAP00000Header.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    if (HQ.isInsert == false && HQ.isDelete == false && HQ.isUpdate == false)
        App.menuClickbtnSave.disable();
    App.frmMain.isValid(); //Require các field yêu cầu trên man hình
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isFirstLoad = true;
    App.cboClassID.getStore().addListener('load', checkLoad);
    App.cboTermsID.getStore().addListener('load', checkLoad);
    App.cboTranDescDef.getStore().addListener('load', checkLoad);
    App.cboBankAcct.getStore().addListener('load', checkLoad);
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            break;
        case "next":
            break;
        case "prev":
            break;
        case "last":
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    save();
                }
            }
            break;

        case "close":
            break;
        case "new":
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;

        default:
    }
};

var frmChange = function () {
    if (App.stoAP00000Header.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoAP00000Header);
    HQ.common.changeData(HQ.isChange, 'AP00000');
    
};

var stoLoad = function (sto) {
  
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "");
        record = sto.getAt(0);
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require 
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    if (!HQ.isInsert && HQ.isNew) {
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

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AP00000/Save',
            params: {
                lstSetup: Ext.encode(App.stoAP00000Header.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

// Submit the deleted data into server side
//var deleteData = function (item) {
//    if (item == "yes") {
//        if (App.frmMain.isValid()) {
//            App.frmMain.updateRecord();
//            App.frmMain.submit({
//                waitMsg: HQ.common.getLang("DeletingData"),
//                url: 'AP00000/DeleteAll',
//                timeout: 7200,
//                success: function (msg, data) {
//                    App.cboVendID.getStore().load();
//                    menuClick("new");
//                },
//                failure: function (msg, data) {
//                    HQ.message.process(msg, data, true);
//                }
//            });
//        }
//    }
//};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        //if (HQ.isNew)
            //App.cboVendID.setValue('');
        HQ.isChange = false;
        App.stoAP00000Header.reload();
    }
};
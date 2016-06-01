//var Declare = {
//    OM: "OM"
//};

//var Event = {
//    Form: {
//        frmMain_boxReady: function (frm, width, height, eOpts) {
//            App.stoDetDiscGxC1.load(function () {
//                App.stoPriceSeqxx.load(function () {
//                    App.stoGroupDiscGxC1.load(function () {
//                        App.stoDocDiscGxCx.load(function () {
//                            App.stoSetup.reload();
//                        });
//                    });
//                });
//            });
//        },

//        frmMain_fieldChange: function () {
//            if (App.stoSetup.getCount() > 0) {
//                App.frmMain.updateRecord();

//                HQ.isChange = HQ.store.isChange(App.stoSetup);

//                HQ.common.changeData(HQ.isChange, 'OM00000');//co thay doi du lieu gan * tren tab title header
//            }
//        },

//        menuClick: function (command) {
//            switch (command) {
//                case "save":
//                    if (HQ.isInsert || HQ.isUpdate) {
//                        Process.saveData();
//                    }
//                    else {
//                        HQ.message.show(4, '', '');
//                    }
//                    break;

//                case "delete":
//                    if (HQ.isDelete) {
                        
//                    }
//                    else {
//                        HQ.message.show(4, '', '');
//                    }
//                    break;

//                case "close":
//                    HQ.common.close(this);
//                    break;

//                case "new":
//                    if (HQ.isInsert) {
//                        if (HQ.isChange) {
//                            HQ.message.show(150, '', '');
//                        }
//                        else {
                            
//                        }
//                    }
//                    break;

//                case "refresh":
//                    if (HQ.isChange) {
//                        HQ.message.show(20150303, '', 'Process.refresh');
//                    }
//                    else {
//                        Process.refresh("yes");
//                    }
//                    break;
//                default:
//            }
//        }
//    },

//    Store: {
//        stoSetup_load: function (sto, records, successful, eOpts) {
//            HQ.isNew = false;
//            if (sto.getCount() == 0) {
//                var newSlsper = Ext.create("App.mdlOM_Setup", {
//                    SetupID: Declare.OM
//                });
//                sto.insert(0, newSlsper);
//                HQ.isNew = true;
//            }
//            var frmRecord = sto.getAt(0);
//            App.frmMain.loadRecord(frmRecord);

//            HQ.isChange = false;
//            HQ.common.changeData(HQ.isChange, 'OM00000');
//        }
//    }
//};

//var Process = {
//    saveData: function () {
//        if (App.frmMain.isValid()) {
//            App.frmMain.updateRecord();

//            App.frmMain.submit({
//                url: 'OM00000/SaveSetup',
//                waitMsg: HQ.common.getLang('Submiting') + "...",
//                timeout: 1800000,
//                params: {
//                    lstSetup: Ext.encode(App.stoSetup.getRecordsValues()),
//                    isNew: HQ.isNew
//                },
//                success: function (msg, data) {
//                    if (data.result.msgCode) {
//                        HQ.message.show(data.result.msgCode);
//                    }
//                    App.stoSetup.reload();
//                },
//                failure: function (msg, data) {
//                    if (data.result.msgCode) {
//                        HQ.message.show(data.result.msgCode);
//                    }
//                    else {
//                        HQ.message.process(msg, data, true);
//                    }
//                }
//            });
//        }
//    },

//    showFieldInvalid: function (form) {
//        var done = 1;
//        form.getForm().getFields().each(function (field) {
//            if (!field.isValid()) {
//                HQ.message.show(15, field.fieldLabel, 'Process.focusOnInvalidField');
//                done = 0;
//                return false;
//            }
//        });
//        return done;
//    },

//    focusOnInvalidField: function (item) {
//        if (item == "ok") {
//            App.frmMain.getForm().getFields().each(function (field) {
//                if (!field.isValid()) {
//                    field.focus();
//                    return false;
//                }
//            });
//        }
//    },

//    refresh: function (item) {
//        if (item == "yes") {
//            App.stoSetup.reload();
//        }
//    }
//};

////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 58;
var _isLoadMaster = false;

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSetup.reload();
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
    App.cboPOSPrinter.getStore().addListener('load', checkLoad);
    App.cboOrderType.getStore().addListener('load', checkLoad);
    App.cboSalesPrice.getStore().addListener('load', checkLoad);
    App.cboCheckCreditRule.getStore().addListener('load', checkLoad);
    //App.stoDetDiscGxC1.addListener('load', checkLoad);
    //App.stoPriceSeqxx.addListener('load', checkLoad);
    //App.stoGroupDiscGxC1.addListener('load', checkLoad);
    //App.stoDocDiscGxCx.addListener('load', checkLoad);
    App.cboDetDiscG1C1.getStore().addListener('load', checkLoad);
    App.cboDetDiscG2C1.getStore().addListener('load', checkLoad);
    App.cboDetDiscG3C1.getStore().addListener('load', checkLoad);
    App.cboDetDiscG4C1.getStore().addListener('load', checkLoad);
    App.cboDetDiscG5C1.getStore().addListener('load', checkLoad);
    App.cboDetDiscG6C1.getStore().addListener('load', checkLoad);
    //
    App.cboDetDiscG1C2.getStore().addListener('load', checkLoad);
    App.cboDetDiscG2C2.getStore().addListener('load', checkLoad);
    App.cboDetDiscG3C2.getStore().addListener('load', checkLoad);
    App.cboDetDiscG4C2.getStore().addListener('load', checkLoad);
    App.cboDetDiscG5C2.getStore().addListener('load', checkLoad);
    App.cboDetDiscG6C2.getStore().addListener('load', checkLoad);
    //
    //
    //
    App.cboPriceSeq00.getStore().addListener('load', checkLoad);
    App.cboPriceSeq01.getStore().addListener('load', checkLoad);
    App.cboPriceSeq02.getStore().addListener('load', checkLoad);
    App.cboPriceSeq03.getStore().addListener('load', checkLoad);
    App.cboPriceSeq04.getStore().addListener('load', checkLoad);
    App.cboPriceSeq05.getStore().addListener('load', checkLoad);
    //
    //
    //
    App.cboGroupDiscG1C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG2C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG3C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG4C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG5C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG6C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG7C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG8C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG9C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG10C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG11C1.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG12C1.getStore().addListener('load', checkLoad);
    //
    App.cboGroupDiscG1C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG2C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG3C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG4C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG5C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG6C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG7C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG8C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG9C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG10C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG11C2.getStore().addListener('load', checkLoad);
    App.cboGroupDiscG12C2.getStore().addListener('load', checkLoad);
    //
    //
    //
    App.cboDocDiscG1C1.getStore().addListener('load', checkLoad);
    App.cboDocDiscG2C1.getStore().addListener('load', checkLoad);
    App.cboDocDiscG3C1.getStore().addListener('load', checkLoad);
    App.cboDocDiscG4C1.getStore().addListener('load', checkLoad);
    App.cboDocDiscG5C1.getStore().addListener('load', checkLoad);
    App.cboDocDiscG6C1.getStore().addListener('load', checkLoad);
    //
    App.cboDocDiscG1C2.getStore().addListener('load', checkLoad);
    App.cboDocDiscG2C2.getStore().addListener('load', checkLoad);
    App.cboDocDiscG3C2.getStore().addListener('load', checkLoad);
    App.cboDocDiscG4C2.getStore().addListener('load', checkLoad);
    App.cboDocDiscG5C2.getStore().addListener('load', checkLoad);
    App.cboDocDiscG6C2.getStore().addListener('load', checkLoad);


}

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
    if (App.stoSetup.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoSetup);
    HQ.common.changeData(HQ.isChange, 'OM00000');

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
            url: 'OM00000/SaveSetup',
            params: {
                lstSetup: Ext.encode(App.stoSetup.getRecordsValues()),
                isNew: HQ.isNew
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




/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        //if (HQ.isNew)
        //App.cboVendID.setValue('');
        HQ.isChange = false;
        App.stoSetup.reload();
    }
};
///////////////////////////////////
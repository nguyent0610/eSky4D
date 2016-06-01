////////////////////////////////////////////////////////////////////////
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
        App.stoAR00000Header.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    //HQ.util.checkAccessRight(); //Kiểm tra quyền Insert Update Delete để disable các button trên topbar(Bắt buộc)
    if (HQ.isInsert == false && HQ.isDelete == false && HQ.isUpdate == false)
        App.menuClickbtnSave.disable();
    App.frmMain.isValid(); //Require các field yêu cầu trên man hình
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isFirstLoad = true;
    App.cboTranDescDflt.getStore().addListener('load', checkLoad);
    App.cboDfltShipViaID.getStore().addListener('load', checkLoad);
    App.cboAddressLevel.getStore().addListener('load', checkLoad);
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
        case "delete":
            
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
    if (App.stoAR00000Header.getCount() > 0)
        App.frmMain.getForm().updateRecord();
    
    HQ.isChange = HQ.store.isChange(App.stoAR00000Header);
    HQ.common.changeData(HQ.isChange, 'AR00000');
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

function save() {
    var curRecord = App.frmMain.getRecord();


    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'AR00000/Save',
            params: {
                lstAR00000Header: Ext.encode(App.stoAR00000Header.getRecordsValues())
            },
            success: function (data) {
                HQ.message.show(201405071);
                refresh("yes");
            },

            failure: function (errorMsg, data) {
                HQ.message.process(errorMsg, data, true);
            }
        });
    }
};

// Load and show binding data to the form
//var loadDataHeader = function () {
//    if (App.stoAR00000Header.getCount() == 0) {
//        App.stoAR00000Header.insert(0, Ext.data.Record());
//    }

//    App.frmMain.getForm().loadRecord(App.stoAR00000Header.getAt(0));

//};




/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////



function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.stoAR00000Header.reload();
    }
};



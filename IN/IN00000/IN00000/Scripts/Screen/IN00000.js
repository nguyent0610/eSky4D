////////////////////////////////////////////////////////////////////////
//// Declare ///////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoIN00000.reload();
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
    App.cboDfltSite.getStore().addListener('load', checkLoad);
    App.cboDfltValMthd.getStore().addListener('load', checkLoad);
    
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
    if (App.stoIN00000.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoIN00000);
    HQ.common.changeData(HQ.isChange, 'IN00000');
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
            url: 'IN00000/Save',
            params: {
                lstIN00000: Ext.encode(App.stoIN00000.getRecordsValues())
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





/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////



function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.stoIN00000.reload();
    }
};



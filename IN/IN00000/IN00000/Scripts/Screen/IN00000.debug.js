//var loadSourceCombo = function () {
//    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
//    App.cboDfltSite.getStore().load(function () {
//        App.cboDfltValMthd.getStore().load(function () {

//        })
//    });
//};
// Submit the changed data (created, updated) into server side
function save() {

    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        //curRecord.data.Name = App.txtName.getValue();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'IN00000/Save',
                params: {
                    lstIN00000: Ext.encode(App.stoIN00000.getChangedData({ skipIdForPhantomRecords: false })),
                    isNew: HQ.isNew
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
                    App.stoIN00000.reload();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }

};


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
                if (HQ.form.checkRequirePass(App.frmMain))//check require truoc khi save
                    save();
            }
            break;
        case "close":
            HQ.common.close(this);
            break;
        case "new":
            break;
        case "delete":
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.stoIN00000.reload();
            }
            break;
        default:
    }
};


var firstLoad = function () {
    //loadSourceCombo();
};

//load store khi co su thay doi
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BranchID");
        record = sto.getAt(0);
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
    }
    App.frmMain.getForm().loadRecord(App.stoIN00000.getAt(0));
    frmChange();
};

//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoIN00000);
    HQ.common.changeData(HQ.isChange, 'IN00000');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.stoIN00000.reload();
    }
};
///////////////////////////////////



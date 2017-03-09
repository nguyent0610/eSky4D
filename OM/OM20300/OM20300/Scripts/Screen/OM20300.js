var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
//ATTENTION: isUpdate, isInsert, isDelete  -- from index.cshtml

// Submit the changed data (created, updated) into server side
function save() {
    var curRecord = App.frmMain.getRecord();
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'OM20300/Save',
            params: {
                lstOM20300Header: Ext.encode(App.stoOM20300Header.getChangedData({ skipIdForPhantomRecords: false })),
            },
            success: function (data) {
                HQ.message.show(201405071);
                menuClick('refresh');
            },

            failure: function (errorMsg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
// Load and show binding data to the form
var loadDataHeader = function () {
    if (App.stoOM20300Header.getCount() == 0) {
        App.stoOM20300Header.insert(0, Ext.data.Record());
    }

    App.frmMain.getForm().loadRecord(App.stoOM20300Header.getAt(0));

};

// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            selectedIndex = 0;
            App.frmMain.getForm().loadRecord(App.stoOM20300Header.first());
            break;
        case "next":
            if (selectedIndex < (App.stoOM20300Header.getCount() - 1))
                selectedIndex += 1;
            App.frmMain.getForm().loadRecord(App.stoOM20300Header.getAt(selectedIndex));
            break;
        case "prev":
            if (selectedIndex > 0)
                selectedIndex -= 1;
            App.frmMain.getForm().loadRecord(App.stoOM20300Header.getAt(selectedIndex));
            break;
        case "last":
            selectedIndex = App.stoOM20300Header.getCount() - 1;
            App.frmMain.getForm().loadRecord(App.stoOM20300Header.last());
            break;
        case "save":
            save();
            break;

        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (storeIsChange(App.stoOM20300Header, false)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(App);
            }
            break;
        case "new":

            break;
        case "refresh":
            App.stoOM20300Header.load();
            break;
        default:
    }
};

// When anwser the confirmed closing
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(App);
    }
};

// Check the store of data is change or not
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};



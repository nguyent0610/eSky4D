//// Declare //////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            break;
        case "prev":
            break;
        case "next":
            break;
        case "last":
            break;
        case "refresh":
            App.storeCA_Setup.reload();
            break;
        case "new":
            break;
        case "delete":
            break;
        case "save":
            save();
            break;
        case "print":
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (storeIsChange(App.storeCA_Setup, false)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(App);
            }
            break;
    }

};

/////////////////////////////////////////////////////////////////////////




//// Process Data ///////////////////////////////////////////////////////
var loadData = function () {
    if (App.storeCA_Setup.getCount() == 0) {
        App.storeCA_Setup.insert(0, Ext.data.Record());
    }
    App.frmMain.getForm().loadRecord(App.storeCA_Setup.getAt(0));

};
var frmloadAfterRender = function (obj) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingdata"));
    App.storeCA_Setup.load();
};
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'CA00000/Save',
            params: {
                lstCA_Setup: HQ.store.getData(App.storeCA_Setup)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
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
/////////////////////////////////////////////////////////////////////////



//// Other Functions ////////////////////////////////////////////////////

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(App);
    }
};


/////////////////////////////////////////////////////////////////////////









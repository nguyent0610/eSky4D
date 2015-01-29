var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
//ATTENTION: isUpdate, isInsert, isDelete  -- from index.cshtml

// Submit the changed data (created, updated) into server side
function Save() {
    var curRecord = App.dataForm.getRecord();
   

    App.dataForm.getForm().updateRecord();
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'AR00000/Save',
            params: {
                lstAR00000Header: Ext.encode(App.storeAR00000Header.getChangedData({ skipIdForPhantomRecords: false })),
            },
            success: function (data) {
                menuClick('refresh');                
                App.storeAR00000Header.load();
                callMessage(201405071, '', null);
            },

            failure: function (errorMsg, data) {
                //processMessage(errorMsg, data);
                var dt = Ext.decode(data.response.responseText)
                callMessage(dt.code, '', null);
            }
        });
    }
};





// Load and show binding data to the form
var loadDataHeader = function () {
    if (App.storeAR00000Header.getCount() == 0) {
        App.storeAR00000Header.insert(0, Ext.data.Record());
    }
    
        App.dataForm.getForm().loadRecord(App.storeAR00000Header.getAt(0));
    
};












// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            selectedIndex = 0;
            App.dataForm.getForm().loadRecord(App.storeAR00000Header.first());
            break;
        case "next":
            if (selectedIndex < (App.storeAR00000Header.getCount() - 1))
                selectedIndex += 1;
            App.dataForm.getForm().loadRecord(App.storeAR00000Header.getAt(selectedIndex));
            break;
        case "prev":
            if (selectedIndex > 0)
                selectedIndex -= 1;
            App.dataForm.getForm().loadRecord(App.storeAR00000Header.getAt(selectedIndex));
            break;
        case "last":
            selectedIndex = App.storeAR00000Header.getCount() - 1;
            App.dataForm.getForm().loadRecord(App.storeAR00000Header.last());
            break;
        case "save":
            Save();
            break;
        
        case "close":
            if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
            if (storeIsChange(App.storeAR00000Header, false)) {
                callMessage(5, '', 'closeScreen');
            } else {
                this.parentAutoLoadControl.close()
            }
            break;
        case "new":
            
            break;
        case "refresh":
            App.storeAR00000Header.load();
            break;
        default:
    }
};

// When anwser the confirmed closing
var closeScreen = function (item) {
    if (item == "no") {
        this.parentAutoLoadControl.close()
    }
    else if (item == "yes") {
        Save();
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



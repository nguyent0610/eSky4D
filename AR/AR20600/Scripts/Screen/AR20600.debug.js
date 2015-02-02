var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 0;
var beforeedit = '';
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var chooseGrid = "";
var click = 0;
var tmpImageDelete = 0;
var tmpMediaDelete = 0;
var tmpSelectedNode = "";
var tmpCopyForm = "0";
var tmpCopyFormSave = "0";
var tmpOldFileName = "";
var tmpShipToIdloadStoreOrForm = "0";
var menuClick = function (command) {
    switch (command) {
        case "first":
            var combobox = App.cboCustId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboCustId.setValue(combobox.store.getAt(0).data.CustId);
            break;
        case "prev":
            var combobox = App.cboCustId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboCustId.setValue(combobox.store.getAt(index - 1).data.CustId);
            break;
        case "next":
            var combobox = App.cboCustId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboCustId.setValue(combobox.store.getAt(index + 1).data.CustId);
            break;
        case "last":
            var combobox = App.cboCustId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboCustId.setValue(App.cboCustId.store.getAt(App.cboCustId.store.getTotalCount() - 1).data.CustId);
            break;
        case "refresh":
            App.storeForm.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                App.cboShipToId.setValue('');
                App.storeForm.reload();
                //var index = App.ApproveStatusAll.indexOf(App.ApproveStatusAll.findRecord("Code", "H"));                    
            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {
                if (App.cboShipToId.value != "") {                    
                    //callMessage(11, '', 'deleteRecordForm');
                    HQ.message.show(11, '', 'deleteRecordForm');
                }
            }
            break;
        case "save":
            Save();
            break;
        case "print":
            alert(command);
            break;
        case "close":
            Close();
            break;
    }

};
var refreshTree = function (i) {
    App.Tree.select(i);
}
var refreshTree2 = function (dt) {
    App.cboInvtID.setValue(dt);
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == node) {
            //click = 1;
            App.Tree.select(i);
            //setTimeout(function () { refreshTree(i); }, 7000);
            break;
        }
    }
}

var waitFormReload = function (sender, e) {
    App.cboShipToId.setValue(sender);
    App.storeForm.reload();
}

function Save() {
    var curRecord = App.frmMain.getRecord();
    curRecord.data.ShipToId = App.cboShipToId.getValue();

    App.frmMain.getForm().updateRecord();
    //App.frmMain.updateRecord(curRecord);
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'AR20600/Save',
            params: {
                lstheader: Ext.encode(App.storeForm.getChangedData({ skipIdForPhantomRecords: false })),//,
                custId: App.cboCustId.value
            },
            success: function (result, data) {
                if (data.result.ShipToId != "") {
                    //App.storeForm.reload();
                    App.cboShipToId.getStore().reload();
                    //callMessage(201405071, '', null);
                    HQ.message.show(201405071, '', null);
                    //App.cboShipToId.setValue('');
                    menuClick("refresh");
                    setTimeout(function () { waitFormReload(data.result.ShipToId); }, 2500);
                    //App.cboShipToId.setValue(data.result.ShipToId);
                    // App.storeForm.reload();
                } else {

                }
            }
            , failure: function (errorMsg, data) {

                //var dt = Ext.decode(data.response.responseText);
                //callMessage(dt.code, dt.colName + ',' + dt.value, null);
                if (data.result.code) {
                    //callMessage(data.result.code, '', '');
                    HQ.message.show(data.result.code, '', '');
                }
                else {
                    processMessage(errorMsg, data);
                }
            }
        });
    }
    else {
        var fields = App.frmMain.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        alert(item);
                    }
                }
            );
    }
}


// Xem lai
function Close() {
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (storeIsChange(App.storeForm, false)) {
       // callMessage(5, '', 'closeScreen');
        HQ.message.show(5, '', 'closeScreen');

    } else {
        parent.App.tabAR20600.close();
    }
}
// Xem lai
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;

    //if (isCreate === '') {//true, !='', != null, !=0, != underfide, 
    //}
};
var closeScreen = function (item) {
    if (item == "yes") {

        Save();
    }
    else {
        if (parent.App.tabAR20600 != null)
            parent.App.tabAR20600.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        try {
            App.direct.AR20600Delete(App.cboCustId.getValue(), App.cboShipToId.getValue(), {
                success: function (data) {

                    App.cboShipToId.getStore().reload();
                    menuClick('refresh');
                    App.cboShipToId.setValue('');
                },
                failure: function () {
                    //
                },
                eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
            });
        } catch (ex) {
            alert(ex.message);
        }
    }
};

var deleteRecordGrid = function (item) {
    if (item == "yes") {
        App.grd.deleteSelected();
    }
};

function selectRecord(grid, record) {
    var record = grid.store.getById(id);
    grid.store.loadPage(grid.store.findPage(record), {
        callback: function () {
            grid.getSelectionModel().select(record);
        }
    });
};

var loadDataAutoHeader = function () {

    if (App.storeForm.getCount() == 0) {
        App.storeForm.insert(0, Ext.data.Record());
    }
    var record = App.storeForm.getAt(0);
    if (record) {
        App.frmMain.getForm().loadRecord(record);
        //App.cboCustId.getStore().reload();
    }
};

var waitCopyShipToIdLoad = function () {
    
    if (App.cboShipToId.getStore().data.items[0] == undefined) {
        App.cboShipToId.setValue("");
    } else {
        //App.cboShipToId.setValue("");
        App.cboShipToId.setValue(App.cboShipToId.getStore().data.items[0].data.ShipToId);
    }
}

var cboCustID_Change = function (sender, e) {
    App.cboShipToId.setValue("");
    App.cboShipToId.getStore().reload();
    setTimeout(function () { waitCopyShipToIdLoad(); }, 1500);
    //App.storeForm.reload;
}

var cboShipToId_Change = function (sender, e) {
    tmpShipToIdloadStoreOrForm = "1";
    App.storeForm.reload();
}

//var waitcboCountryLoad = function () {
//    App.cboState.getStore().reload();

//}

//var waitcboStateLoad = function () {
//    App.cboCity.getStore().reload();
//    App.cboDistrict.getStore().reload();

//}
var cboCountry_Change = function (sender, newValue, oldValue) {
    if (tmpShipToIdloadStoreOrForm == "1") {
        App.cboState.getStore().reload();
        App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
    } else {
        App.cboState.getStore().reload();
    }
}

var cboState_Change = function (sender, newValue, oldValue) {
    if (tmpShipToIdloadStoreOrForm == "1") {
        App.cboCity.getStore().reload();
        App.cboDistrict.getStore().reload();
        App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
        tmpShipToIdloadStoreOrForm = "0";
    } else {
        App.cboCity.getStore().reload();
        App.cboDistrict.getStore().reload();
    }
}
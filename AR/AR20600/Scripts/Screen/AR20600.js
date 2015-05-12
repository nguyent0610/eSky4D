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
            tmpShipToIdloadStoreOrForm = "1";
            App.storeForm.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                App.cboShipToId.setValue('');
                App.storeForm.reload();                 
            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {
                if (App.cboShipToId.value != "") {
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
};

var refreshTree2 = function (dt) {
    App.cboInvtID.setValue(dt);
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == node) {
            App.Tree.select(i);
            break;
        }
    }
};

var waitFormReload = function (sender, e) {
    tmpShipToIdloadStoreOrForm = "1";
    App.cboShipToId.setValue(sender);
    App.storeForm.reload();
};

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
                lstheader: Ext.encode(App.storeForm.getChangedData({ skipIdForPhantomRecords: false })),
                custId: App.cboCustId.value
            },
            success: function (result, data) {
                if (data.result.ShipToId != "") {
                    App.cboShipToId.getStore().reload();
                    HQ.message.show(201405071, '', null);
                    menuClick("refresh");
                    setTimeout(function () { waitFormReload(data.result.ShipToId); }, 2500);
                } else {
                }
            }
            , failure: function (errorMsg, data) {
                if (data.result.code) {
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
};

// Xem lai
function Close() {
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (storeIsChange(App.storeForm, false)) {
        HQ.message.show(5, '', 'closeScreen');

    } else {
        parent.App.tabAR20600.close();
    }
};

// Xem lai
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
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
                eventMask: { msg: HQ.common.getLang("DeletingData"), showMask: true }
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
    }
};

var waitCopyShipToIdLoad = function () {

    if (App.cboShipToId.getStore().data.items[0] == undefined) {
        App.cboShipToId.setValue("");
    } else {
        App.cboShipToId.setValue(App.cboShipToId.getStore().data.items[0].data.ShipToId);
    }
};

var cboCustID_Change = function (sender, e) {
    App.cboShipToId.setValue("");
    App.cboShipToId.getStore().reload();
    setTimeout(function () { waitCopyShipToIdLoad(); }, 1500);
};

var cboShipToId_Change = function (sender, e) {
    tmpShipToIdloadStoreOrForm = "1";
    App.storeForm.reload();
};

var cboCountry_Change = function (sender, newValue, oldValue) {
    setTimeout(function () {
        if (tmpShipToIdloadStoreOrForm == 1) {
            App.cboState.getStore().load({
                scope: this,
                callback: function () {
                    var record = App.frmMain.getRecord();
                    App.cboState.setValue(record.data.State);
                }
            });
        } else {
            App.cboState.getStore().load();
        }
    }, 1500);
};

var cboState_Change = function (sender, newValue, oldValue) {
    setTimeout(function () { waitCityDistrictLoad(); }, 1500);
};

var waitCityDistrictLoad = function () {
    if (tmpShipToIdloadStoreOrForm == 1) {
        var record = App.frmMain.getRecord();
        App.cboCity.getStore().load({
            scope: this,
            callback: function () {
                App.cboCity.setValue(record.data.City);
            }
        });

        App.cboDistrict.getStore().load({
            scope: this,
            callback: function () {
                App.cboDistrict.setValue(record.data.District);
            }
        });
        //App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
        tmpShipToIdloadStoreOrForm = 0;
    } else {
        App.cboCity.getStore().load();
        App.cboDistrict.getStore().load();
    }
};

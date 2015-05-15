
var isChanged = 0;
var menuClick = function (command) {
    switch (command) {
        case "first":
            var combobox = App.cboClassId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboClassId.setValue(combobox.store.getAt(0).data.ClassId);
            break;
        case "prev":
            var combobox = App.cboClassId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboClassId.setValue(combobox.store.getAt(index - 1).data.ClassId);
            break;
        case "next":
            var combobox = App.cboClassId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboClassId.setValue(combobox.store.getAt(index + 1).data.ClassId);
            break;
        case "last":
            var combobox = App.cboClassId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboClassId.setValue(App.cboClassId.store.getAt(App.cboClassId.store.getTotalCount() - 1).data.ClassId);
            break;
        case "refresh":
            isChanged = 1;
            App.stoForm.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                App.cboClassId.setValue('');                
                App.stoForm.reload();                
                setTimeout(function () { setDfltTaxLoad(); }, 1200);
            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {
                if (App.cboClassId.value != "") {
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
    isChanged = 1;
    App.cboClassId.setValue(sender);
    App.stoForm.load();
}

function Save() {
    var curRecord = App.frmMain.getRecord();
    curRecord.data.classId = App.cboClassId.getValue();    
    App.frmMain.getForm().updateRecord();
    //App.frmMain.updateRecord(curRecord);
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'AR20100/Save',
            params: {
                lstheader: Ext.encode(App.stoForm.getChangedData({ skipIdForPhantomRecords: false })),
                classId: App.cboClassId.value
            },
            success: function (result, data) {
                if (data.result.ClassId != "") {
                    App.cboClassId.getStore().reload();
                    HQ.message.show(201405071, '', null);
                    menuClick("refresh");
                    setTimeout(function () { waitFormReload(data.result.ClassId); }, 2500);
                }                
            }
            , failure: function (errorMsg, data) {
                if (data.result.msgCode) {
                    //callMessage(data.result.code, '', '');
                    HQ.message.show(data.result.msgCode, '', '');
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

var setDfltTaxLoad = function () {
    
    if (App.cboTaxDflt.getStore().data.items[0] != undefined) {       
        App.cboTaxDflt.setValue(App.cboTaxDflt.getStore().data.items[0].data.Code);
    }
}

// Xem lai
function Close() {
    if (App.frmMain.getRecord() != undefined)
        App.frmMain.updateRecord();
    if (storeIsChange(App.stoForm, false)) {
       // callMessage(5, '', 'closeScreen');
        HQ.message.show(5, '', 'closeScreen');

    } else {
        parent.App.tabAR20100.close();
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
    if (item == "yes")
    {
        Save();
    }
    else {
        if (parent.App.tabAR20100 != null)
            parent.App.tabAR20100.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        try {
            App.direct.AR20100Delete(App.cboClassId.getValue(), {
                success: function (data) {

                    App.cboClassId.getStore().reload();
                    menuClick('refresh');
                    App.cboClassId.setValue('');
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

    if (App.stoForm.getCount() == 0) {
        App.stoForm.insert(0, Ext.data.Record());
    }
    var record = App.stoForm.getAt(0);
    if (record) {
        App.frmMain.getForm().loadRecord(record);
    }
};

var cboClassId_Changed = function (sender, e) {
    isChanged = 1;
    App.cboTerritory.setValue("");
    App.cboTerritory.getStore().reload();   
    App.stoForm.reload();
    var record = App.frmMain.getRecord();
    if (record == undefined || record.data.ClassId == "")
    {
        setTimeout(function () { setDfltTaxLoad(); }, 1000);
    }
}

var cboCountry_Changed = function (sender, newValue, oldValue) {
    setTimeout(function () {
        if (isChanged == 1) {
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
}

var cboState_Changed = function (sender, newValue, oldValue) {
    setTimeout(function () { waitCityDistrictLoad(); }, 1500);
}

var waitCityDistrictLoad = function () {
    if (isChanged == 1) {
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
        isChanged = 0;
    } else {
        App.cboCity.getStore().load();
        App.cboDistrict.getStore().load();
    }
};
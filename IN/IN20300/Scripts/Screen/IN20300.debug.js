var pattern = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
var fieldId = "";
var focus = 0;
var isChanged = 0;
var isSaveError = 0;
var menuClick = function (command) {
    switch (command) {
        case "first":
            var combobox = App.cboSiteId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboSiteId.setValue(combobox.store.getAt(0).data.SiteId);
            break;
        case "prev":
            var combobox = App.cboSiteId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboSiteId.setValue(combobox.store.getAt(index - 1).data.SiteId);
            break;
        case "next":
            var combobox = App.cboSiteId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboSiteId.setValue(combobox.store.getAt(index + 1).data.SiteId);
            break;
        case "last":
            var combobox = App.cboSiteId;
            var v = combobox.getValue();
            var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            var index = combobox.store.indexOf(record);
            App.cboSiteId.setValue(App.cboSiteId.store.getAt(App.cboSiteId.store.getTotalCount() - 1).data.SiteId);
            break;
        case "refresh":
            isSaveError = 0;
            isChanged = 1; 
            App.stoForm.load();
            //App.stoCompany.load();
            break;
        case "new":
            if (HQ.isInsert) {
                App.cboSiteId.setValue('');                
                App.stoForm.reload();
                //App.stoCompany.load();
            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {                
                if (App.cboSiteId.value != "") {
                    HQ.message.show(11, '', 'deleteRecordForm');
                        //if (focus == "0") {                        
                            
                        //}
                        //else if (focus == "1"){
                        //    if (App.cboSiteId.value != "") {
                        //        HQ.message.show(11, '', 'deleteRecordGrid');
                        //    }
                        //}
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
            App.frmMain.updateRecord();
            if (HQ.store.isChange(App.stoForm)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};

var askClose = function (item) {
    if (item == "yes") {
        Save();
    } else {
        HQ.common.close(this);
    }
};


var waitFormReload = function (sender, e) {
    isChanged = 1;
    App.cboSiteId.setValue(sender);
    App.stoForm.load();
    //App.stoCompany.load();
}

function Save() {
    var curRecord = App.frmMain.getRecord();
    curRecord.data.SiteId = App.cboSiteId.getValue();    
    App.frmMain.getForm().updateRecord();
    //App.frmMain.updateRecord(curRecord);
    if (App.frmMain.isValid()) {
        if (App.txtEmailAddr.getValue() != '' && !App.txtEmailAddr.getValue().match(pattern)) {
            HQ.message.show(9112014, '', '');
            return;
        } else {
            App.frmMain.submit({
                waitMsg: 'Submiting...',
                url: 'IN20300/Save',
                params: {
                    lstheader: Ext.encode(App.stoForm.getChangedData({ skipIdForPhantomRecords: false })),
                    siteId: App.cboSiteId.value,
                    //lstCpny: Ext.encode(App.stoCompany.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (result, data) {
                    if (data.result.SiteId != "") {
                        App.cboSiteId.getStore().reload();
                        HQ.message.show(201405071, '', null);
                        menuClick("refresh");
                        setTimeout(function () { waitFormReload(data.result.SiteId); }, 2500);
                    }
                    isSaveError = 0;
                }
                , failure: function (errorMsg, data) {
                    if (data.result.msgCode) {
                        isSaveError = 1;
                        //callMessage(data.result.code, '', '');
                        HQ.message.show(data.result.msgCode, '', '');
                    }
                    else {
                        processMessage(errorMsg, data);
                    }
                }
            });
        }
    }
    else {
        App.frmMain.getForm().getFields().each(                
            function (item) {
                if (!item.isValid()) {
                    fieldId = item.id;
                    HQ.message.show(1000, item.fieldLabel, 'focusAfterBox');
                    return false;
                }
            }
        );
    }
}



var focusAfterBox = function (item) {
    //if (item == "ok") {
        if (App[fieldId]) {
            App[fieldId].focus();
        }
    //}
};


// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        try {
            App.direct.IN20300Delete(App.cboSiteId.getValue(), {
                success: function (data) {

                    App.cboSiteId.getStore().reload();
                    menuClick('refresh');
                    App.cboSiteId.setValue('');
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
        App.grdCompany.deleteSelected();
    }
};

var loadDataAutoHeader = function () {
    if (App.stoForm.getCount() == 0) {
        App.stoForm.insert(0, Ext.data.Record());
    }
    var record = App.stoForm.getAt(0);
    if (record) {
        App.frmMain.getForm().loadRecord(record);
    }
    App.cboCountry.getStore().load({
        scope: this,
        callback: function () {
            var record = App.frmMain.getRecord();
            App.cboState.setValue(record.data.State);
        }
    });
};

// Check visible Company Tab
function chkAutoChange() {
    var activeTab = App.tabIN_Site.items.findIndex('id', App.tabIN_Site.getActiveTab().id);
    if (true == App.chkPublic.checked) {        
        App.tabIN_Site.closeTab(App.tabCpny);
    } else {
        App.tabIN_Site.addTab(App.tabCpny);
        App.tabIN_Site.setActiveTab(activeTab);
    }
    App.tabCpny.setDisabled(App.chkPublic.checked);
}

var frmloadAfterRender = function (obj) {
    App.stoForm.load();    
    //App.stoCompany.load();
};

var tabIN_Site_AfterRender = function (obj) {
    //if (this.parentAutoLoadControl != null)
    //    obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    //else {
    //    obj.setHeight(Ext.getBody().getViewSize().height - 100);
    //}
};

var cboCpnyID_Changed = function (item, newValue, oldValue) {
    var r = App.CpnyID.valueModels[0];
    if (r == null) {
        App.SelectModelIN20300_pcCpnybyUsername.getSelection()[0].set('CpnyName', "");
    }
    else {
        App.SelectModelIN20300_pcCpnybyUsername.getSelection()[0].set('CpnyName', r.data.CpnyName);
    }

};

var cboSiteId_Changed = function (item, newValue, oldValue) {
    if (isSaveError == 1)
    {
        return;
    }
    isChanged = 1;         
    App.cboSiteId.setValue(newValue);
    //if ((item.valueModels.length == 0 || item.valueModels == null) && App.storeUser.data.items.length != 0) {
    //    App.stoForm.reload();
    //    App.stoCompany.load();
    //}
    //else if (item.valueModels.length != 0) {
        oldValue = null;
        App.stoForm.load();
        //App.stoCompany.load();
    //}
    //chkAutoChange();
}

var cboCountry_Changed = function (sender, newValue, oldValue) {
    //setTimeout(function () {
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
    //}, 1500);
}

var cboState_Changed = function (sender, newValue, oldValue) {
    setTimeout(function () { waitCityDistrictLoad(); }, 1000);
}
// Reload City and District
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
        isChanged = 0;
    } else {
        App.cboCity.getStore().load();
        App.cboDistrict.getStore().load();
    }
};

// Validate email address
var Email_Changed = function (sender, e) {    
    if (App.txtEmailAddr.getValue() != '' && !App.txtEmailAddr.getValue().match(pattern)) {
        HQ.message.show(9112014, '', null);       
        return false;
    }
}

var grdCpny_focus = function (sender, e) {
    focus = "1";
}

var level0_Focus = function (sender, e) {
    focus = "0";
}

var grdCompany_BeforeEdit = function (editor, e) {
    if (App.cboSiteId.getValue() == null || !HQ.isUpdate)
        return false;
    var strkey = e.record.idProperty.split(',');
    if (e.record.phantom == false && strkey.indexOf(e.column.dataIndex) != -1)
        return false;
};
var grdCompany_ValidateEdit = function (item, e) {
    if (e.field == 'CpnyID') {
        if (e.value && e.record.phantom) {
            var flat = App.stoCompany.findBy(function (record, id) {
                if (record.get('CpnyID') == e.value && record.id != e.record.id) {
                    return true;
                }
                return false;
            });
            if (flat != -1) {
                item.CpnyID = "";
                HQ.message.show(219, e.value, '');                
                return false;
            }
        }
    }
};

var grdCompany_Edit = function (item, e) {
    if (e.field == 'CpnyID') {
        if (e.value) {
            var flat = App.stoCompany.findBy(function (record, id) {
                if (!record.get('CpnyID')) {
                    return true;
                }
                return false;
            });
            if (flat == -1) {
                App.stoCompany.insert(App.stoCompany.getCount(), Ext.data.Record());
            }
        }
    }
};

var renderCpnyName = function (value) {
    var record = App.cboCpnyID.findRecord("CpnyID", value);
    if (record)
    {
        return record.data.CpnyName;
    } else {
        return value;
    }
};
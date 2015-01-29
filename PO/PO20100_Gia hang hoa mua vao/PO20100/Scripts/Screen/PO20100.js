function Save() {
    App.dataForm.getForm().updateRecord();
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'PO20100/Save',
            params: {

                lstPOPriceHeader: Ext.encode(App.storePOPriceHeader.getChangedData({ skipIdForPhantomRecords: false })),
                lstPOPrice: Ext.encode(App.storePOPrice.getChangedData({ skipIdForPhantomRecords: false })),
                lstPOPriceCpny: Ext.encode(App.storePOPriceCpny.getChangedData({ skipIdForPhantomRecords: false }))
            },
            success: function (data) {
                var PriceID = App.cboPriceID.getValue();
                App.cboPriceID.getStore().load();
                App.cboPriceID.setValue(PriceID);
                menuClick("refresh");
            },

            failure: function () {
            }
        });
    }
}
function Delete(item) {
    if (item == 'yes') {
        App.direct.Delete(App.cboPriceID.getValue(), {
            success: function () {
                App.cboPriceID.getStore().load();
                App.cboPriceID.setValue('');
                menuClick('new');
            },
            eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
        });
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (focus == 'POPriceHeader') {
                App.cboPriceID.setValue(App.cboPriceID.getStore().first().get('PriceID'));
            }
            else if (focus == 'POPrice') {
                App.SelectModelPOPrice.select(0);
            } else if (focus == 'POPriceCpny') {
                App.SelectModelPOPriceCpny.select(0);

            }
            break;
        case "next":
            if (focus == 'POPriceHeader') {
                var index = App.cboPriceID.getStore().indexOf(App.cboPriceID.getStore().getById(App.cboPriceID.getValue()));
                App.cboPriceID.setValue(App.cboPriceID.getStore().getAt(index + 1).get('PriceID'));
            }
            else if (focus == 'POPrice') {
                App.SelectModelPOPrice.selectNext();
            } else if (focus == 'POPriceCpny') {
                App.SelectModelPOPriceCpny.selectNext();
            }
            break;
        case "prev":
            if (focus == 'POPriceHeader') {
                var index = App.cboPriceID.getStore().indexOf(App.cboPriceID.getStore().getById(App.cboPriceID.getValue()));
                App.cboPriceID.setValue(App.cboPriceID.getStore().getAt(index - 1).get('CpnyID'));
            }
            else if (focus == 'POPrice') {
                App.SelectModelPOPrice.selectPrevious();
            } else if (focus == 'POPriceCpny') {
                App.SelectModelPOPriceCpny.selectPrevious();
            }
            break;
        case "last":
            if (focus == 'POPriceHeader') {
                App.cboPriceID.setValue(App.cboPriceID.getStore().last().get('PriceID'));
            }
            else if (focus == 'POPrice') {
                App.SelectModelPOPrice.select(App.storePOPrice.data.items.length - 1);
            } else if (focus == 'POPriceCpny') {
                App.SelectModelPOPriceCpny.select(App.storePOPriceCpny.data.items.length - 1);
            }
            break;
        case "save":
            Save();
            break;
        case "delete":
            if (focus == 'POPriceHeader') {
                if (App.cboPriceID.value) {
                    if (isDelete) {
                        callMessage(11, '', 'Delete');
                    }
                } else {
                    menuClick('new');
                }
            } else if (focus == 'POPrice') {
                if ((App.cboPriceID.value && isUpdate && isDelete) || (!App.cboPriceID.value && isInsert)) {
                    App.grdPOPrice.deleteSelected();
                }
            } else if (focus == 'POPriceCpny') {
                if ((App.cboPriceID.value && isUpdate && isDelete) || (!App.cboPriceID.value && isInsert)) {
                    App.grdPOPriceCpny.deleteSelected();
                }
            }
            break;
        case "close":
            if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
            if (storeIsChange(App.storePOPriceHeader, false) || storeIsChange(App.storePOPrice) || storeIsChange(App.storePOPriceCpny)) {
                callMessage(5, '', 'closeScreen');
            } else {
                this.parentAutoLoadControl.close()
            }
            break;
        case "new":
            App.cboPriceID.setValue(null);
            break;
        case "refresh":
            App.storePOPriceHeader.load();
            App.storePOPrice.load();
            App.storePOPriceCpny.load();
            break;
        default:
    }
};
var loadData = function () {
    if (App.storePOPriceHeader.getCount() == 0) {
        App.storePOPriceHeader.insert(0, Ext.data.Record());
    }
    App.dataForm.getForm().loadRecord(App.storePOPriceHeader.getAt(0));
};
var closeScreen = function (item) {
    if (item == "no") {
        this.parentAutoLoadControl.close()
    }
    else if (item == "yes") {
        Save();
    }
};
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false) || store.getChangedData().Updated != undefined || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};
var grdPOPrice_BeforeEdit = function (editor, e) {
    if (App.cboPriceID.getValue() == null || !isUpdate) return false;
    var strkey = e.record.idProperty.split(',');
    paramInvtID = e.record.data.InvtID;
    App.UOM.getStore().load();

    if (e.record.phantom == false && strkey.indexOf(e.column.dataIndex) != -1)
        return false;

};
var grdPOPrice_ValidateEdit = function (item, e) {
    if (e.field == 'InvtID') {
        if (e.value && e.record.phantom) {
            var flat = App.storePOPrice.findBy(function (record, id) {
                if (record.get('InvtID') == e.value && record.id != e.record.id) {

                    return true;
                }
                return false;
            });

            if (flat != -1) {
                callMessage(219, e.value, '');
                return false;
            }
        }
    }
};
var grdPOPrice_Edit = function (item, e) {
    if (e.field == 'InvtID') {
        if (e.value) {
            var flat = App.storePOPrice.findBy(function (record, id) {
                if (!record.get('InvtID')) {
                    return true;
                }
                return false;
            });
            if (flat == -1) {
                App.storePOPrice.insert(App.storePOPrice.getCount(), Ext.data.Record());
            }
        }
    }
};
var grdPOPriceCpny_BeforeEdit = function (editor, e) {
    if (App.cboPriceID.getValue() == null || !isUpdate) return false;
    var strkey = e.record.idProperty.split(',');
    if (e.record.phantom == false && strkey.indexOf(e.column.dataIndex) != -1)
        return false;

};
var grdPOPriceCpny_ValidateEdit = function (item, e) {
    if (e.field == 'CpnyID') {
        if (e.value && e.record.phantom) {
            var flat = App.storePOPriceCpny.findBy(function (record, id) {
                if (record.get('CpnyID') == e.value && record.id != e.record.id) {

                    return true;
                }
                return false;
            });

            if (flat != -1) {
                callMessage(219, e.value, '');
                return false;
            }
        }
    }
};
var grdPOPriceCpny_Edit = function (item, e) {
    if (e.field == 'CpnyID') {
        if (e.value) {
            var flat = App.storePOPriceCpny.findBy(function (record, id) {
                if (!record.get('CpnyID')) {
                    return true;
                }
                return false;
            });
            if (flat == -1) {
                App.storePOPriceCpny.insert(App.storePOPriceCpny.getCount(), Ext.data.Record());
            }
        }
    }
};
var cboPriceID_Change = function (item, newValue, oldValue) {
    App.cboPriceID.setValue(newValue);
    if ((item.valueModels.length == 0 || item.valueModels == null) && App.storePOPriceHeader.data.items.length != 0) {
        App.storePOPriceHeader.load();
        App.storePOPrice.load();
        App.storePOPriceCpny.load();
        lockcontrol();
    }
    else if (item.valueModels.length != 0) {
        oldValue = null;
        App.storePOPriceHeader.load();
        App.storePOPrice.load();
        App.storePOPriceCpny.load();
        lockcontrol();
    }

};
var cboInvtID_Change = function (item, newValue, oldValue) {
    var r = App.InvtID.valueModels[0];
    if (r == null) {
        App.SelectModelPOPrice.getSelection()[0].set('Descr', "");
    }
    else {
        App.SelectModelPOPrice.getSelection()[0].set('Descr', r.data.Descr);
    }

};
var cboCpnyID_Change = function (item, newValue, oldValue) {
    var r = App.CpnyID.valueModels[0];
    if (r == null) {
        App.SelectModelPOPriceCpny.getSelection()[0].set('CpnyName', "");
    }
    else {
        App.SelectModelPOPriceCpny.getSelection()[0].set('CpnyName', r.data.BranchName);
    }
    // App.SelectModelPOPrice.getSelection()[0].set('Descr', App.InvtID.displayTplData[0].Descr);        
};
var chkPublic_Change = function (checkbox, checked) {

    if (checked) {
        App.tabPO20100.closeTab(App.tabCompany)

    }
    else {
        App.tabPO20100.addTab(App.tabCompany)

    }
    // App.SelectModelPOPrice.getSelection()[0].set('Descr', App.InvtID.displayTplData[0].Descr);        
};
function btnFill_Click() {
    App.storePOPrice.each(function (item, index, totalItems) {
        item.set('Disc', App.txtFill.getValue());

    });

};
var tabPO_Setup_AfterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 100);
    }
};
var frmloadAfterRender = function (obj) {
    App.storePOPriceHeader.load();
    App.storePOPrice.load();
    App.storePOPriceCpny.load();
    lockcontrol();
};
function lockcontrol() {
    if (App.cboPriceID.valueModels.length == 0) {
        App.Descr.setReadOnly(!isInsert);
        App.Public.setReadOnly(!isInsert);
        App.Status.setReadOnly(!isInsert);
        App.txtFill.setReadOnly(!isInsert);
        App.btnFill.setDisabled(!isInsert);
        App.EffDate.setReadOnly(!isInsert);
    }
    else {
        App.Descr.setReadOnly(!isUpdate);
        App.Public.setReadOnly(!isUpdate);
        App.Status.setReadOnly(!isUpdate);
        App.txtFill.setReadOnly(!isUpdate);
        App.btnFill.setDisabled(!isUpdate);
        App.EffDate.setReadOnly(!isUpdate);
    }
}
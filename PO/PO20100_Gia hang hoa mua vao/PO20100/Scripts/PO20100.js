  
var paramInvtID = "";
function Save() {
    App.dataForm.getForm().updateRecord();
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'PO20100/Save',
            params: {

                lststorePO_PriceHeader: Ext.encode(App.storePO_PriceHeader.getChangedData({ skipIdForPhantomRecords: false }))
            },
            success: function (data) {
                if (data != "askdelete") {
                    App.dataForm.reset();
                }
            },

            failure: function () {
            }
        });
    }
}
var menuClick = function (command) {
    if (command == 'refresh') {
        storePO_PriceHeader.load();
    } else if (command == 'new') {
        App.storePO_Price.insert(0, Ext.data.Record())
    } else if (command == 'delete') {
        Delete();
    } else if (command == 'save') {
        Save();
    } else if (command == 'print') {
        //alert(command);
    } else if (command == 'close') {
        Close();
    } else {
        alert(command);
    }
};
var loadData = function () {
    if (App.storePO_PriceHeader.getCount() == 0) {
        App.storePO_PriceHeader.insert(0, Ext.data.Record());
    }
    App.dataForm.getForm().loadRecord(App.storePO_PriceHeader.getAt(0));
};    
var grdPO_Price_BeforeEdit = function (editor, e) {
    var strkey = e.record.idProperty.split(',');
    paramInvtID=e.record.data.InvtID;
    App.UOM.getStore().load();
    if (e.record.phantom == false && strkey.indexOf(e.column.dataIndex) != -1)
        return false;

};    
var grdPO_Price_AfterEdit = function (editor, e) {     
    var strkey = e.record.idProperty.split(',');
    if (strkey.indexOf(e.field) != -1) {
        if (e.record.modified[e.field] == null && e.originalValue != "")
            return false;
    }

};
var grdPO_Price_ValidateEdit = function (item, e) {
    if (e.field == 'InvtID') {
        if (e.value && e.record.phantom) {
            var flat = App.storePO_Price.findBy(function (record, id) {
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
var cboInvtID_Change = function (item, newValue, oldValue) {
    var r = App.InvtID.displayTplData[0];
    if (r == undefined) {
        App.SelectModelPO_Price.getSelection()[0].set('Descr', "");
    }
    else {
        App.SelectModelPO_Price.getSelection()[0].set('Descr', r.Descr);
    }
    // App.SelectModelPO_Price.getSelection()[0].set('Descr', App.InvtID.displayTplData[0].Descr);        
};

function Close() {
    App.dataForm.getForm().updateRecord();
    if (App.storePO_PriceHeader.getChangedData() == null && parent.App.tabPO20100 != null)
        parent.App.tabPO20100.close();
    else if (App.storePO_PriceHeader.getChangedData().Updated != undefined || App.storePO_PriceHeader.getChangedData().Created != undefined || App.storePO_PriceHeader.getChangedData().Deleted != undefined) {
        App.direct.AskClose({
            success: function (result) {

            }
        });
    }
    else alert("Da dong");


}
var askClose = function (item) {
    if (item == "yes") {
        Save();
    }
    else {
        if (parent.App.tabPO20100 != null)
            parent.App.tabPO20100.close();
        else alert("Khong tim thay parent tab");
    }
};

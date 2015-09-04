//// Declare //////////////////////////////////////////////////////////
var stoDet_Load = function (store, records, success) {
    store.loaded = true;
    HQ.common.showBusy(false);
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(store);
}
var stoDet_BeforeLoad  = function (store, records, success) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
}
var menuClick = function (command) {
    switch (command) {
        case "refresh":           
            HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
            App.grdDet.getStore().reload();
            break;

    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;  
};
var cboType_Change = function (sender, newValue, oldValue) {   
    if (sender.valueModels != null ) {
        //App.txtBranchName.setValue(App.cboBranchID.valueModels[0] ? App.cboBranchID.valueModels[0].data.BranchName : '');
        //App.cboSlsperID.setValue('');
        //App.cboSlsperID.getStore().reload();
        App.stoDet.reload();
     
    }

};
var cboType_Select = function (sender) {
    if (sender.valueModels != null) {
        //App.txtBranchName.setValue(App.cboBranchID.valueModels[0] ? App.cboBranchID.valueModels[0].data.BranchName : '');
        //App.cboSlsperID.setValue('');
        //App.cboSlsperID.getStore().reload();
        App.stoDet.reload();
    }
};
var btnImport_Click = function (c, e) {
    
};
var btnExport_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
       
        App.frmMain.submit({
            //waitMsg: HQ.common.getLang("Exporting"),
            url: 'IF30100/Export',
            type: 'POST',
            timeout: 1000000,
            clientValidation: false,
            params: {
                lstDet: Ext.encode(App.stoDet.getRecordsValues()),
                view: App.cboType.getValue(),
                name: App.cboType.valueModels[0].data.ReportName
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);  
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var colCheck_Header_Change = function (value) {
    if (value) {
        App.stoDet.suspendEvents();
        App.stoDet.each(function (item) {              
            item.set("Checked", value.checked);           
        });
        App.stoDet.resumeEvents();
        App.grdDet.view.refresh();
    }
}

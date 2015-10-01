var HQ_OrdNbrBranch = '';
var _cpnyID = HQ.cpnyID;
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdHeader);
            break;
        case "prev":
            HQ.grid.prev(App.grdHeader);
            break;
        case "next":
            HQ.grid.next(App.grdHeader);
            break;
        case "last":
            HQ.grid.last(App.grdHeader);
            break;
        case "refresh":
            App.stoHeader.reload();
            break;
        case "new":
            break;
        case "delete":
            break;
        case "save":
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);
            break;
    }
};

var Ctrl_Expand = function (a, item) {
    HQ_OrdNbrBranch = item.data.OrdNbrBranch;
    App.stoDetail.reload();
};

var cboBranchID_Change = function (item, newValue, oldValue) {
    if (item.valueModels != null && !Ext.isEmpty(App.cboBranchID.getValue()) && !item.hasFocus) {//truong hop co chon branchid
        App.txtBranchName.setValue(App.cboBranchID.valueModels[0].data.BranchName);
        _cpnyID = App.cboBranchID.valueModels[0].data.BranchID;
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.cboCustID.setValue('');
        App.cboInvtID.setValue('');
        App.cboSiteID.setValue('');
        App.cboCustID.getStore().reload();
        App.cboInvtID.getStore().reload();
        App.cboSiteID.getStore().reload();
        
        HQ.common.showBusy(false);
    }
    else {
        if (Ext.isEmpty(App.cboBranchID.getValue())) {
            App.txtBranchName.setValue('');
            _cpnyID = '';
        }
    }

};
var cboBranchID_Select = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.txtBranchName.setValue(App.cboBranchID.valueModels[0].data.BranchName);
        _cpnyID = App.cboBranchID.valueModels[0].data.BranchID;
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.cboCustID.setValue('');
        App.cboInvtID.setValue('');
        App.cboSiteID.setValue('');
        App.cboCustID.getStore().reload();
        App.cboInvtID.getStore().reload();
        App.cboSiteID.getStore().reload();
        HQ.common.showBusy(false);

    }
}
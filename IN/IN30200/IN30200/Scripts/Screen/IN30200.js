var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdDetail);
            break;
        case "prev":
            HQ.grid.prev(App.grdDetail);
            break;
        case "next":
            HQ.grid.next(App.grdDetail);
            break;
        case "last":
            HQ.grid.last(App.grdDetail);
            break;
        case "refresh":
            App.stoDetail.reload();
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

var cboInvtID_Change = function () {
    App.stoHeader.reload();
};

var cboBranchID_Change = function () {
    App.cboInvtID.store.reload();
    App.stoDetail.reload();
};

var chkBranchID_Change = function (ctr) {
    if (App.chkBranchID.checked) {
        HQ.combo.selectAll(App.cboBranchID);
    }
    else {
        App.cboBranchID.setValue('');
    }
};

var stoHeader_Load = function (sto) {
    if (sto.data.items.length == 0) {
        sto.insert(sto.getCount(), Ext.data.Record());
    }
    var record = sto.data.items[0];
    App.frmMain.getForm().loadRecord(record);
    App.stoDetail.reload();
};

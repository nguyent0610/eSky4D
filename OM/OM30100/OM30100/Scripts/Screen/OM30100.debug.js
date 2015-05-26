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

var cboInvtID_Change = function () {
    App.stoHeader.reload();
};

var cboBranchID_Change = function () {
    App.cboInvtID.store.reload();
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
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
};

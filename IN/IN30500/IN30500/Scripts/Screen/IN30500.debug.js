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

var cboBranchID_Change = function () {
    App.cboInvtID.getStore().load(function () {
        App.cboSite.store.reload();
    });
};

var chkBranchID_Change = function (ctr) {
    if (App.chkBranchID.checked) {
        HQ.combo.selectAll(App.cboBranchID);
    }
    else {
        App.cboBranchID.setValue('');
    }
};

var chkInvtID_Change = function (ctr) {
    if (App.chkInvtID.checked) {
        HQ.combo.selectAll(App.cboInvtID);
    }
    else {
        App.cboInvtID.setValue('');
    }
};

var chkSite_Change = function (ctr) {
    if (App.chkSite.checked) {
        HQ.combo.selectAll(App.cboSite);
    }
    else {
        App.cboSite.setValue('');
    }
};

var stoLoad = function (sto) {

};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        App.stoDetail.reload();
    }
};


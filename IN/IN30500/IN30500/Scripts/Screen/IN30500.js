var HQ_MatHang = '';
var HQ_BranchID = '';

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
            refresh('yes');
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
    HQ_MatHang = item.data.MaHang;
    HQ_BranchID = item.data.BranchID;
    App.stoDetail.reload();
};

var grdHeader_Change = function () {
    App.stoDetail.reload();
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
    HQ.common.showBusy(false, HQ.waitMsg);
};
var sto_BeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.waitMsg);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {      
        App.stoHeader.reload();
    }
};



var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Access);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Access);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Access);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Access);
            break;
        case "refresh":
            App.stoMailHeader.reload();
            HQ.grid.first(App.grdSYS_Access);
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
            if (HQ.store.isChange(App.stoSYS_BuildLog)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};

/////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};

var loadDataAutoHeader = function () {
    var record = App.stoMailHeader.getAt(0);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
    }
};

var cboACCESSDATE_Change = function (sender, e) {
    if (sender.isValid() == true) {
        App.stoMailHeader.reload();
    }
};
var cboACCESSDATE_Edit = function (value) {
    //if (value == "") {
    //    App.cboACCESSDATE.setValue(new Date(Date.now()));
    //}
};
var renderFromcbodateTogrid = function () {
    App.stoMailHeader.reload();
};

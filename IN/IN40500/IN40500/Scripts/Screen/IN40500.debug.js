var menuClick = function (command) {
    switch (command) {
        case "first":

            break;
        case "next":
            break;
        case "prev":
            break;
        case "last":
            break;
        case "save":

            break;
        case "close":
            HQ.common.close(this);
            break;
        case "new":
            break;
        case "delete":
            break;
        case "refresh":
            break;
        default:
    }
};

var btnCreat_Click = function () {
    if (!App.txtDescr.isValid())
        HQ.message.show(1000, App.txtDescr.fieldLabel, '');
    else if (!App.cboSiteID.isValid())
        HQ.message.show(1000, App.cboSiteID.fieldLabel, '');
    else
        save();
};

var firstLoad = function () {
    //loadSourceCombo();
    App.txtBranchID.setValue(HQ.cpnyID);
    App.txtDescr.isValid();
    App.cboSiteID.isValid();
};

function save() {
    if (HQ.isInsert || HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'IN40500/Save',
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};



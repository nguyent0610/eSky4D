
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
            break;
        case "new":
            HQ.common.lockItem(App.frmMain, false);
            App.txtTAGID.hide();
            App.txtTAGID.setValue('');
            App.txtDescr.setValue('');
            App.cboSiteID.clearValue();
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
    App.dtpTranDate.setValue(HQ.bussinessDate);
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
                timeout: 1800000,
                success: function (msg, data) {
                    if (data.result.TAGIG != "") {
                        App.txtTAGID.show();
                        App.txtTAGID.setValue(data.result.TAGID);
                        HQ.message.show(20403, [data.result.TAGID], '', true);
                        HQ.common.lockItem(App.frmMain, true);
                    }
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};



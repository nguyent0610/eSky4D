
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
            App.cboBranchID.select(App.cboBranchID.getStore().getAt(0));
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

var cboBranchID_Change = function (sender, value) {
    if (sender.valueModels != null) {
        App.cboSiteID.clearValue();
        App.cboSiteID.store.reload();
    }
};

var cboBranchID_Select = function (sender, value) {
    if (sender.valueModels != null) {
        App.cboSiteID.clearValue();
        App.cboSiteID.store.reload();
    }
};

var btnCreat_Click = function () {
    if(!App.cboBranchID.isValid())
        HQ.message.show(1000, App.cboBranchID.fieldLabel, '');
    else if (!App.txtDescr.isValid())
        HQ.message.show(1000, App.txtDescr.fieldLabel, '');
    else if (!App.cboSiteID.isValid())
        HQ.message.show(1000, App.cboSiteID.fieldLabel, '');
    else
        save();
};

var firstLoad = function () {
    App.dtpTranDate.setValue(HQ.bussinessDate);
    App.cboBranchID.isValid();
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



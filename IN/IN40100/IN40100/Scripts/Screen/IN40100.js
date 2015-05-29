var _Change = false;
var key = [''];
var _firstLoad = true;
var _firstLoad1 = true;
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
};

var firstLoad = function () {
    App.txtPerPost.setValue(HQ.bussinessDate);
    App.cboSiteID.isValid();
    App.cboOption.isValid();
};

var ColCheck_Header_Change = function (value, rowIndex, checked) {
    if (value) {
        App.stoIN40100.each(function (item) {
            item.set("Sel", value.checked);
        });
    }
};

var btnLoad_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        App.stoIN40100.reload();
    }
};

var btnProcess_Click = function () {
    if (!App.cboOption.getValue()) {
        HQ.message.show(1000, App.cboOption.fieldLabel);
    }
    else {
        var flat = false;
        App.stoIN40100.data.each(function (item) {
            if (item.data.Sel) {
                flat = true;
                return false;
            }
        });
        if (flat && !Ext.isEmpty(App.cboOption.getValue())) {
            save(false,false);
        }
    }
};
var save = function (b76,b81) {
    App.frmMain.submit({
        clientValidation: false,
        waitMsg: HQ.common.getLang("Handle"),
        method: 'POST',
        url: 'IN40100/Process',
        timeout: 180000,
        params: {
            lstPPC_StockRecovery: Ext.encode(App.grdIN40100.store.getRecordsValues()),
            mess76: b76
        },
        success: function (msg, data) {
            HQ.message.show(201405071);
            App.stoIN40100.reload();
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};

var process76 = function (item) {
    if (item == 'yes') {
        save(true,false);
    }
};

var process81 = function (item) {
    if (item == 'yes') {
        save(false,true);
    }
};
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdIN40100);
            break;
        case "prev":
            HQ.grid.prev(App.grdIN40100);
            break;
        case "next":
            HQ.grid.next(App.grdIN40100);
            break;
        case "last":
            HQ.grid.last(App.grdIN40100);
            break;
        case "refresh":
            if (_Change) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                App.stoIN40100.reload();
            }
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

var grdIN40100_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdIN40100, e, keys);
};

var grdIN40100_BeforeEdit = function (editor, e) {
    if (e.field == 'Sel')
        return true;
    else
        return false;
};

var grdIN40100_Edit = function (item, e) {
    //HQ.grid.checkInsertKey(App.grdOM_WeekOfVisit, e, keys);
};

var grdIN40100_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN40100);
    stoChanged(App.stoIN40100);
};

var stoChanged = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(_Change, 'IN40100');
    App.cboOption.setReadOnly(_Change);
    App.cboSiteID.setReadOnly(_Change);
    App.btnLoad.setDisabled(_Change);

};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN40100');
    HQ.common.showBusy(false);
    stoChanged(App.stoIN40100);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.stoIN40100.reload();
    }
};
///////////////////////////////////////////////////////////////////////
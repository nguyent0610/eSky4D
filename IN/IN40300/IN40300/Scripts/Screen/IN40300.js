var _Change = false;
var keys = [''];
var _firstLoad = true;
var _firstLoad1 = true;

var _Source = 0;
var _maxSource = 3;

var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _Source = 0;
        HQ.common.showBusy(false);
        
    }
};


var cboStatus_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        //App.grdIN40300.store.removeAll();
        App.stoIN40300.each(function (item) {
            item.set("ColCheck", false);
        });
        App.ColCheck_Header.setValue(false);
        App.cboHandle.store.reload();
    }
};

var btnLoad_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        App.stoIN40100.reload();
    }
};


var firstLoad = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboInvtID.getStore().addListener('load', checkLoad);
    App.cboSiteID.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.txtBranchID.setValue(HQ.cpnyID);
    HQ.util.checkAccessRight();
}

var ColCheck_Header_Change = function (value, rowIndex, checked) {
    if (value) {
        App.stoIN40300.each(function (item) {
            if (item.data.Status == App.cboStatus.getValue()) {
                item.set("ColCheck", value.checked);
            }
        });
    }
};
var btnProccess_Click = function (type) {
    if (Ext.isEmpty(App.cboSiteID.getValue())) {
        HQ.message.show(1000, App.cboSiteID.fieldLabel);
        return;
    }
    App.frmMain.submit({
        clientValidation: false,
        waitMsg: HQ.common.getLang("WaitMsg"),
        method: 'POST',
        url: 'IN40300/Process',
        timeout: 180000,
        params: {
            chkINValidates: App.chkINValidate.getValue(),
            chkCalQtys: App.chkCalQty.getValue(),
            chkCalSOQtys: App.chkCalSOQty.getValue(),
            type: type,
        },
        success: function (msg, data) {
            if (data.result.Msg)
            {
                App.txtRes.setValue(data.result.Msg);
            }
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};

//var btnProcess_Click = function () {
//    if (!App.cboHandle.getValue()) {
//        HQ.message.show(1000, App.cboHandle.fieldLabel);
//    }
//    else {
//        var d = Ext.Date.parse("01/01/1990", "m/d/Y");
//        if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
//        var flat = false;
//        App.stoIN40300.data.each(function (item) {
//            if (item.data.ColCheck) {
//                flat = true;
//                return false;
//            }
//        });
//        if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
//            App.frmMain.submit({
//                clientValidation: false,
//                waitMsg: HQ.common.getLang("Handle"),
//                method: 'POST',
//                url: 'IN40300/Process',
//                timeout: 180000,
//                params: {
//                    lstPPC_StockRecovery: Ext.encode(App.grdIN40300.store.getRecordsValues())
//                },
//                success: function (msg, data) {
//                    HQ.message.show(201405071);
//                    App.stoIN40300.reload();
//                },
//                failure: function (msg, data) {
//                    HQ.message.process(msg, data, true);
//                }
//            });
//        }
//    }
//};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdIN40300);
            break;
        case "prev":
            HQ.grid.prev(App.grdIN40300);
            break;
        case "next":
            HQ.grid.next(App.grdIN40300);
            break;
        case "last":
            HQ.grid.last(App.grdIN40300);
            break;
        case "refresh":
            if (_Change) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                App.stoIN40300.reload();
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

var grdIN40300_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdIN40300, e, keys);
};

var grdIN40300_BeforeEdit = function (editor, e) {
    if (e.field == 'ColCheck' && e.record.data.Status == App.cboStatus.getValue()) {
        return true;
    }

    if (e.record.data.isEdit == '1' && e.field != 'ColCheck') {
        return true;
    }
    return false;
};

var grdIN40300_Edit = function (item, e) {
    if (e.record.data.StkQty < e.record.data.ApproveQty || e.record.data.ApproveQty < 0) {
        e.record.set("ApproveQty", e.record.data.StkQty);
    }
};

var grdIN40300_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN40300);
    stoChanged(App.stoIN40300);
};

var stoChanged = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(_Change, 'IN40300');
    App.cboStatus.setReadOnly(_Change);
    App.btnLoad.setDisabled(_Change);
    App.cboBranchID.setDisabled(_Change);
    App.FromDate.setDisabled(_Change);
    App.ToDate.setDisabled(_Change);
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN40300');
    HQ.common.showBusy(false);
    //record = sto.getAt(0)
    //if (record.data.Status = 'H') {
    //    record.data.ApproveQty = record.data.StkQty;
    //}
    stoChanged(App.stoIN40300);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.stoIN40300.reload();
    }
};

var renderStatus = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboStatusIN40300_pcStatus.findRecord("Code", rec.data.Status);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
///////////////////////////////////////////////////////////////////////
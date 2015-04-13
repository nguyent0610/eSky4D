var _Change = false;
var keys = [''];
var _firstLoad = true;
var _firstLoad1 = true;
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboStatus.getStore().load(function () {
        if (_firstLoad1) {
            App.cboStatus.setValue("H");
            //App.cboHandle.setValue("N");
            _firstLoad1 = false;
        }
        App.cboHandle.getStore().load(function () {
            HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
            if (_firstLoad) {
                //App.cboStatus.setValue("H");
                App.cboHandle.setValue("N");
                _firstLoad = false;
            }
        })
    })
};

var cboStatus_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        //App.grdPPC_StockRecovery.store.removeAll();
        App.stoPPC_StockRecovery.each(function (item) {
            item.set("ColCheck", false);
        });
        App.ColCheck_Header.setValue(false);
        App.cboHandle.store.reload();
    }
};

var btnLoad_Click = function () {
    App.stoPPC_StockRecovery.reload();
};

var ColCheck_Header_Change = function (value, rowIndex, checked) {
    if (value) {
        App.stoPPC_StockRecovery.each(function (item) {
            if (item.data.Status == App.cboStatus.getValue() && item.data.Status == 'H') {
                item.set("ColCheck", value.checked);
            }
        });
    }
};

var btnProcess_Click = function () {
    if (App.cboHandle.getValue()) {
        var d = Ext.Date.parse("01/01/1990", "m/d/Y");
        if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
        var flat = false;
        App.stoPPC_StockRecovery.data.each(function (item) {
            if (item.data.ColCheck) {
                flat = true;
                return false;
            }
        });
        if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang("Handle"),
                method: 'POST',
                url: 'IN22002/Process',
                timeout: 180000,
                params: {
                    lstPPC_StockRecovery: Ext.encode(App.grdPPC_StockRecovery.store.getRecordsValues())
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    App.stoPPC_StockRecovery.reload();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdPPC_StockRecovery);
            break;
        case "prev":
            HQ.grid.prev(App.grdPPC_StockRecovery);
            break;
        case "next":
            HQ.grid.next(App.grdPPC_StockRecovery);
            break;
        case "last":
            HQ.grid.last(App.grdPPC_StockRecovery);
            break;
        case "refresh":
            if (_Change) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                App.stoPPC_StockRecovery.reload();
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

var grdPPC_StockRecovery_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdPPC_StockRecovery, e, keys);
};
var grdPPC_StockRecovery_BeforeEdit = function (editor, e) {
    if (e.record.data.Status == 'H') {
        return true;
    }
    else {
        return false;
    }
};
var grdPPC_StockRecovery_Edit = function (item, e) {
    if (e.record.data.StkQty < e.record.data.ApproveQty || e.record.data.ApproveQty < 0) {
        e.record.set("ApproveQty", 0);
    }
};
var grdPPC_StockRecovery_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdPPC_StockRecovery);
    stoChanged(App.stoPPC_StockRecovery);
};
var stoChanged = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(_Change, 'IN22002');
    App.cboStatus.setReadOnly(_Change);
    App.btnLoad.setDisabled(_Change);
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN22002');
    HQ.common.showBusy(false);
    stoChanged(App.stoPPC_StockRecovery);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.stoPPC_StockRecovery.reload();
    }
};

var renderStatus = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboStatusIN22002_pcStatus.findRecord("Code", rec.data.Status);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
///////////////////////////////////////////////////////////////////////
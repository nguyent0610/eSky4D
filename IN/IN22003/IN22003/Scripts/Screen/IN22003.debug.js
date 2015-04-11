var _Change = false;
var keys = ['ID'];
var _firstLoad = true;
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboCpnyID.getStore().load(function () {
        App.cboSlsperId.getStore().load(function () {
            App.cboStatus.getStore().load(function () {
                App.cboHandle.getStore().load(function () {
                    HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
                    if (_firstLoad) {
                        App.cboStatus.setValue("H");
                        App.cboHandle.setValue("N");
                        _firstLoad = false;
                    }
                })
            })
        })
    })
};
var cboCpnyID_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        App.grdCust.store.removeAll();
        App.cboSlsperId.store.reload();
        //App.grdCust.removeAll();
    }
};
var cboStatus_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        App.grdCust.store.removeAll();
        App.cboHandle.store.reload();
        //App.grdCust.removeAll();
    }
};

var btnLoad_Click = function () {
    App.stoCust.reload();
};

var ColCheck_Header_Change = function (value) {
    if (value) {
        App.stoCust.each(function (item) {
            item.set("ColCheck", value.checked);
        });
    }
};

var btnProcess_Click = function () {
    if (App.cboHandle.getValue()) {
        var d = Ext.Date.parse("01/01/1990", "m/d/Y");
        if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
        var flat = false;
        App.stoCust.data.each(function (item) {
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
                url: 'IN22003/Process',
                timeout: 180000,
                params: {
                    lstCust: Ext.encode(App.grdCust.store.getRecordsValues())
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    App.stoCust.reload();
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
            HQ.grid.first(App.grdCust);
            break;
        case "prev":
            HQ.grid.prev(App.grdCust);
            break;
        case "next":
            HQ.grid.next(App.grdCust);
            break;
        case "last":
            HQ.grid.last(App.grdCust);
            break;
        case "refresh":
            if (_Change) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                App.stoCust.reload();
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

var grdCust_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdCust, e, keys);
};
var grdCust_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCust);
    stoChanged(App.stoCust);
};
var stoChanged = function (sto) {

    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(_Change, 'IN22003');
    App.cboStatus.setReadOnly(_Change);
    App.btnLoad.setDisabled(_Change);
    App.cboCpnyID.setReadOnly(_Change);
    App.cboSlsperId.setReadOnly(_Change);
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN22003');

    HQ.common.showBusy(false);
    stoChanged(App.stoCust);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.stoCust.reload();
    }
};
///////////////////////////////////////////////////////////////////////
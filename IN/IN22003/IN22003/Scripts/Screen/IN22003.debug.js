var _Change = false;
var keys = [''];
var _firstLoad = true;
var _firstLoad1 = true;

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboZone.getStore().load(function () {
        App.cboTerritory.getStore().load(function () {

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
        })
    })
};

var dateKPI_expand = function (dte, eOpts) {
    dte.picker.setWidth(100);
    dte.picker.monthEl.hide();
    dte.picker.monthEl.setWidth(0);
};

var dateKPI_Select = function (sender, e) {
    //App.cboCycle.store.reload();
};

var cboZone_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        //App.grdOM_FCSBranch.store.removeAll();
        //App.grdOM_FCSBranch.hide();
        App.cboTerritory.store.load();
    }
};

var cboTerritory_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        //App.grdOM_FCSBranch.store.removeAll();
        //App.grdOM_FCSBranch.hide();
        App.cboBranchID.store.reload();
    }
};

var cboStatus_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        //App.grdIN_StockRecoveryDet.store.removeAll();
        App.stoIN_StockRecoveryDet.each(function (item) {
            item.set("ColCheck", false);
        });
        App.ColCheck_Header.setValue(false);
        App.cboHandle.store.reload();
    }
};

var btnLoad_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        App.stoIN_StockRecoveryDet.reload();
    }
};

var ColCheck_Header_Change = function (value, rowIndex, checked) {
    if (value) {
        App.stoIN_StockRecoveryDet.each(function (item) {
            if (item.data.Status == App.cboStatus.getValue()) {
                item.set("ColCheck", value.checked);
            }
        });
    }
};

var btnProcess_Click = function () {
    if (!App.cboHandle.getValue()) {
        HQ.message.show(1000, App.cboHandle.fieldLabel);
    }
    else if (!App.NewDateExp.getValue()) {
        HQ.message.show(1000, App.NewDateExp.fieldLabel);
    }
    else if (App.NewDateExp.validate() == false) {
        HQ.message.show(1555);
    }
    else {
        var flat = false;
        App.stoIN_StockRecoveryDet.data.each(function (item) {
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
                    lstIN_StockRecoveryDet: Ext.encode(App.grdIN_StockRecoveryDet.store.getRecordsValues())
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    App.stoIN_StockRecoveryDet.reload();
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
            HQ.grid.first(App.grdIN_StockRecoveryDet);
            break;
        case "prev":
            HQ.grid.prev(App.grdIN_StockRecoveryDet);
            break;
        case "next":
            HQ.grid.next(App.grdIN_StockRecoveryDet);
            break;
        case "last":
            HQ.grid.last(App.grdIN_StockRecoveryDet);
            break;
        case "refresh":
            if (_Change) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                App.stoIN_StockRecoveryDet.reload();
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

var grdIN_StockRecoveryDet_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdIN_StockRecoveryDet, e, keys);
};

var grdIN_StockRecoveryDet_BeforeEdit = function (editor, e) {
    if (e.field == 'ColCheck' && e.record.data.Status == App.cboStatus.getValue()) {
        return true;
    }

    if (e.record.data.isEdit == '1' && e.field != 'ColCheck') {
        return true;
    }
    return false;
};

var grdIN_StockRecoveryDet_Edit = function (item, e) {
    if (e.field == "ApproveStkQty") {
        if (e.record.data.StkQty < e.record.data.ApproveStkQty || e.record.data.ApproveStkQty < 0) {
            e.record.set("ApproveStkQty", e.record.data.StkQty);
            e.record.set("ApprovePriceStkQty", e.record.data.StkQty * e.record.data.Price)
        } else {
            e.record.set("ApprovePriceStkQty", e.record.data.ApproveStkQty * e.record.data.Price)
        }
    }
};

var grdIN_StockRecoveryDet_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN_StockRecoveryDet);
    stoChanged(App.stoIN_StockRecoveryDet);
};

var stoChanged = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(_Change, 'IN22003');
    App.cboStatus.setReadOnly(_Change);
    App.btnLoad.setDisabled(_Change);

    App.cboTerritory.setReadOnly(_Change);
    App.cboZone.setReadOnly(_Change);
    App.cboBranchID.setReadOnly(_Change);
    App.dateKPI.setReadOnly(_Change);
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN22003');
    HQ.common.showBusy(false);
    stoChanged(App.stoIN_StockRecoveryDet);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.stoIN_StockRecoveryDet.reload();
    }
};

var renderStatus = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboStatusIN22003_pcStatus.findRecord("Code", rec.data.Status);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
///////////////////////////////////////////////////////////////////////
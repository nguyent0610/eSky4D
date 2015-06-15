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
        App.stoPopUp.reload();
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
    else {
        var flat = false;
        App.stoIN_StockRecoveryDet.data.each(function (item) {
            if (item.data.ColCheck) {
                flat = true;
                return false;
            }
        });
        if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
            App.stoPopUp.clearFilter();
            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang("Handle"),
                method: 'POST',
                url: 'IN22003/Process',
                timeout: 180000,
                params: {
                    lstPopUp: Ext.encode(App.stoPopUp.getRecordsValues()),
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
        App.stoPopUp.reload();
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

var PopupWin = {

    showDetail: function (record) {
        var lock = record.data.Status != "H";
        App.grdPopUp.isLock = lock;

        App.stoPopUp.clearFilter();
        App.stoPopUp.filter('StkRecNbr', record.data.StkRecNbr);
        App.stoPopUp.filter('ExpDate', record.data.ExpDate);
        App.stoPopUp.filter('InvtID', record.data.InvtID);

        App.winDetail.record = record;

        if (record.data.Status == "H") {
            PopupWin.addPopUp(record.data);
        }

        App.grdPopUp.view.refresh();
        App.winDetail.show();
    },

    btnOK_Click: function () {
        var recordTran = App.winDetail.record.data;
        if (recordTran.Status == 'H' || recordTran.isEdit == '1') {
            setTimeout(function () {
                HQ.common.showBusy(false);


                var flat = null;
                var date;
                //App.stoPopUp.clearFilter();


                App.stoPopUp.data.each(function (item) {
                    if (recordTran.Status == 'H') {

                        if (item.data.NewExpDate == date && item.data.ApproveStkQty > 0) {
                            HQ.message.show(2015061203, '', '');
                            flat = item;
                            return false;
                        }
                        date = item.data.NewExpDate;

                        if (Ext.isEmpty(item.data.NewExpDate) && item.data.ApproveStkQty > 0) {
                            HQ.message.show(1000, [HQ.common.getLang('NewExpDate')], '', true);
                            flat = item;
                            return false;
                        }
                        if (Ext.isEmpty(item.data.ApproveStkQty)) {
                            HQ.message.show(1000, [HQ.common.getLang('ApproveStkQty')], '', true);
                            flat = item;
                            return false;
                        }
                        if (item.data.ApproveStkQty == 0) {
                            App.slmPopUp.select(App.stoPopUp.indexOf(item));
                            App.grdPopUp.deleteSelected();
                        }

                    }

                });

                if (!Ext.isEmpty(flat)) {
                    App.slmPopUp.select(App.stoPopUp.indexOf(flat));
                    return;
                }

                var ApproveStkQty = 0;
                var Price = 0;
                App.stoPopUp.data.each(function (item) {
                    ApproveStkQty += item.data.ApproveStkQty;
                    Price = item.data.Price;
                });

                if (ApproveStkQty > recordTran.StkQty) {
                    HQ.message.show(2015061201, '', '');
                    return false;
                }
                else {
                    App.winDetail.record.set('ApproveStkQty', ApproveStkQty);
                    App.winDetail.record.set('ApprovePriceStkQty', ApproveStkQty * Price);
                }

                App.winDetail.hide();
            }, 300);
        }
        else {
            App.winDetail.hide();
        }
    },

    btnDel_Click: function () {
        if (HQ.isUpdate || HQ.isInsert) {
            if (App.slmPopUp.selected.items.length != 0) {
                HQ.message.show(11, '', 'PopupWin.deletePopUp');
            }
        }
    },

    addPopUp: function (record, lotSerNbr) {
        var newRow = Ext.create('App.mdlPopUp');

        newRow.data.BranchID = record.BranchID;
        newRow.data.StkRecNbr = record.StkRecNbr;
        newRow.data.ExpDate = record.ExpDate;
        newRow.data.InvtID = record.InvtID;
        newRow.data.NewExpDate = '';
        newRow.data.StkQty = record.StkQty;
        newRow.data.Price = record.Price;
        newRow.data.ApproveStkQty = 0;
        newRow.data.Status = record.Status;

        HQ.store.insertRecord(App.stoPopUp, "NewExpDate", newRow, true);
    },

    deletePopUp: function (item) {
        if (item == 'yes') {
            App.grdPopUp.deleteSelected();
        }
    },


    grdPopUp_Edit: function (item, e) {
        HQ.common.showBusy(true);
        var objDetail = e.record.data;
        var recordTran = App.winDetail.record.data;
        if (e.field == 'NewExpDate' && !e.value) {
            HQ.message.show(1000, e.field);
        }
            //else if (e.field == 'NewExpDate' && e.validate == false) {
            //    HQ.message.show(1555);
            //}
        else if (e.field == 'ApproveStkQty' && e.value > 0) {
            PopupWin.addPopUp(recordTran);
        }
    },

    checkValidate: function (grd, e, keys) {
        if (keys.indexOf(e.field) != -1) {
            if (HQ.grid.checkDuplicate(grd, e, keys)) {
                HQ.message.show(1112, e.value);
                return false;
            }

        }
    },

    grdPopUp_ValidateEdit: function (item, e) {
        return PopupWin.checkValidate(App.grdPopUp, e, ['NewExpDate']);
    },

    grdPopUp_BeforeEdit: function (item, e) {
        if (App.winDetail.record.data.Status == 'H')
            return true;
        else
            if (App.winDetail.record.data.isEdit == '1')
                return true;
        return false;
    }
};



///////////////////////////////////////////////////////////////////////
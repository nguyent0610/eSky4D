var _beginStatus = "H";
var _tradeType = {
    bonus: "B",
    display: "D"
};

var Process = {
    checkAllValid: function (store) {
        var flag = true;
        store.each(function (record) {
            if (record.data.Selected && !record.data.LevelID) {
                flag = false;
                HQ.message.show(15, HQ.common.getLang("LevelID"));
                return false;
            }
        });
        return flag;
    },

    confirmWarning: function (store) {
        var flag = false;
        store.each(function (record) {
            if (record.data.Selected && !record.data.Registered) {
                flag = true;
                return false;
            }
        });
        return flag;
    },

    saveData: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM22002/SaveData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstCustChange: HQ.store.getData(App.grdDet.store),
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.grdDet.store.reload();
                },
                failure: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
    },

    saveDataBonus: function (item) {
        if (item == "yes") {
            App.frmMain.submit({
                url: 'OM22002/SaveDataBonus',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    lstCustChange: HQ.store.getData(App.grdBonus.store),
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.btnLoadBonus.fireEvent("click");
                },
                failure: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
    },

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isChange = false;
            Event.Form.menuClick("refresh");
        }
    }
};

var Store = {

};

var Event = {
    Form: {
        frmMain_fieldChange: function (frm, field, newValue, oldValue, eOpts) {
            if (App.grdDet.store.getCount() > 0) {

                HQ.isChange = HQ.store.isChange(App.grdDet.store);

                HQ.common.changeData(HQ.isChange, 'OM22002');//co thay doi du lieu gan * tren tab title header
                //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
            }
        },

        btnLoad_click: function (btn, e) {
            if (App.frmMain.isValid()) {
                App.grdDet.store.reload();
            }
        },

        btnImport_click: function (btn, e) {
            Ext.Msg.alert("Import", "Coming soon!");
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        cboCpny_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboObjectID.store.reload();
        },

        cboTradeType_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboObjectID.store.reload();
            if (cbo.value == _tradeType.bonus) {
                App.grdDet.hide();
                App.btnLoad.hide();
                App.btnLoadBonus.show();
                if (App.grdBonus) {
                    App.grdBonus.show();
                }
            }
            else {
                App.grdDet.show();
                App.btnLoad.show();
                App.btnLoadBonus.hide();
                if (App.grdBonus) {
                    App.grdBonus.hide();
                }
            }
        },

        cboObjectID_change: function (cbo, newValue, oldValue, eOpts) {
            //App.cboLevelID.store.reload();
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    if (App.cboTradeType.value == _tradeType.display) {
                        HQ.grid.first(App.grdDet, HQ.isChange);
                    }
                    else {
                        HQ.grid.first(App.grdBonus, HQ.isChange);
                    }
                    break;
                case "next":
                    if (App.cboTradeType.value == _tradeType.display) {
                        HQ.grid.next(App.grdDet, HQ.isChange);
                    }
                    else {
                        HQ.grid.next(App.grdBonus, HQ.isChange);
                    }
                    break;
                case "prev":
                    if (App.cboTradeType.value == _tradeType.display) {
                        HQ.grid.prev(App.grdDet, HQ.isChange);
                    }
                    else {
                        HQ.grid.prev(App.grdBonus, HQ.isChange);
                    }
                    break;
                case "last":
                    if (App.cboTradeType.value == _tradeType.display) {
                        HQ.grid.last(App.grdDet, HQ.isChange);
                    }
                    else {
                        HQ.grid.last(App.grdBonus, HQ.isChange);
                    }
                    break;
                case "refresh":
                    if (App.frmMain.isValid()) {
                        if (HQ.isChange) {
                            HQ.message.show(20150303, '', 'Process.refresh');
                        }
                        else {
                            if (App.cboTradeType.value == _tradeType.display) {
                                App.grdDet.store.reload();
                            }
                            else {
                                App.btnLoadBonus.fireEvent("click");
                            }
                        }
                    }
                    break;
                case "save":
                    if (HQ.isUpdate) {
                        if (App.frmMain.isValid()) {
                            if (App.cboTradeType.value == _tradeType.display) {
                                if (Process.checkAllValid(App.grdDet.store)) {
                                    HQ.message.show(20150407, '', 'Process.saveData');
                                }
                            }
                            else {
                                if (App.grdBonus && Process.checkAllValid(App.grdBonus.store)) {
                                    if (Process.confirmWarning(App.grdBonus.store)) {
                                        HQ.message.show(20150407, '', 'Process.saveDataBonus');
                                    }
                                    else {
                                        Process.saveDataBonus("yes");
                                    }
                                }
                            }
                        }
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
            }
        }
    },

    Grid: {
        grdDet_beforeEdit: function (editor, e) {
            //if (!e.record.data.Registered) {
                if (e.field == "LevelID") {
                    App.cboColLevelID.store.reload();
                }
            //}
            else {
                return false;
            }
        },

        grdBonus_beforeEdit: function (editor, e) {
            if (!e.record.data.Registered) {
                if (e.field == "LevelID") {
                    App.cboColLevelIDBonus.store.reload();
                }
            }
            else {
                return false;
            }
        },

        grd_reject: function (col, record) {
            //var grd = col.up('grid');
            //if (!record.data.tstamp) {
            //    grd.getStore().remove(record, grd);
            //    grd.getView().focusRow(grd.getStore().getCount() - 1);
            //    grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            //} else {
                record.reject();
            //}
        }
    },
};
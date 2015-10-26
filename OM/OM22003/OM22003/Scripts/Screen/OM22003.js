var _beginStatus = "H";
var _Pass = { Dat: "Dat", KhongDat: "KhongDat" };

var Process = {
    renderStatus: function (value) {
        var record = App.cboStatus.store.findRecord("Code", value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },

    renderAppraise: function (value, metaData, record, rowIndex, colIndex, store) {
        if (metaData.column.dataIndex == "Pass") {
            if (value == _Pass.KhongDat) {
                metaData.tdAttr = 'style="color: #FF0000"';
            }
            return HQ.common.getLang(value);
        }
        else if (metaData.column.dataIndex == "SoMatTB") {
            var rec = HQ.store.findInStore(App.cboSoMatTB.store, ["Code"], [value]);
            if (rec) {
                return rec.Descr;
            }
        }
        else if (metaData.column.dataIndex == "LocID") {
            var rec = HQ.store.findInStore(App.cboLocID.store, ["Code"], [value]);
            if (rec) {
                return rec.Descr;
            }
        }
        else if (metaData.column.dataIndex == "DisplayType") {
            var rec = HQ.store.findInStore(App.cboDisplayType.store, ["Code"], [value]);
            if (rec) {
                return rec.Descr;
            }
        }

        return value;
    },

    prepareData: function (data) {
        data.CreateDate = Ext.Date.format(data.CreateDate, "Y-m-d G:i:s");
        return data;
    },

    saveAppraise: function (item) {
        if (item == "yes") {
            var frmRecord = App.frmImage.getRecord();

            App.frmImage.updateRecord();
            App.frmImage.submit({
                url: 'OM22003/SaveAppraise',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    record: Ext.encode([App.frmImage.getRecord().data]),
                    pass: App.radDat.value
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    App.grdDet.store.reload();
                    App.winImgAppraise.close();
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

    showFieldInvalid: function (form) {
        var done = 1;
        form.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                HQ.message.show(15, field.fieldLabel, 'Process.focusOnInvalidField');
                done = 0;
                return false;
            }
        });
        return done;
    },

    focusOnInvalidField: function (item) {
        if (item == "ok") {
            App.frmMain.getForm().getFields().each(function (field) {
                if (!field.isValid()) {
                    field.focus();
                    return false;
                }
            });
        }
    },

    joinParams: function (multiCombo) {
        var returnValue = "";
        if (multiCombo.value && multiCombo.value.length) {
            returnValue = multiCombo.value.join();
        }
        else {
            if (multiCombo.getValue()) {
                returnValue = multiCombo.rawValue;
            }
        }

        return returnValue;
    }
};

var Store = {
    stoImage_load: function (sto, records, successful, eOpts) {
        if (successful) {
            App.dspPicAmt.setValue(sto.getCount());
        }
    }
};

var Event = {
    Form: {
        frmMain_boxReady: function () {
            App.dtpFromDate.setValue(HQ.dateNow);
            App.dtpToDate.setValue(HQ.dateNow);
        },

        cboZone_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboTerritory.store.reload();
        },

        cboTerritory_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboCpny.store.reload();
        },

        cboCpny_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboDisplayID.store.reload();
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(newValue);
            App.dtpToDate.validate();
        },

        btnLoad_click: function (btn, e) {
            if (App.frmMain.isValid()) {
                App.grdDet.store.reload();
            }
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
        },

        btnConfirm_click: function (btn, e) {
            if (HQ.isUpdate) {
                if (App.frmImage.isValid()) {
                    HQ.message.show(20150407, '', 'Process.saveAppraise');
                }
                else {
                    Process.showFieldInvalid(App.frmImage);
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnCancel_click: function (btn, e) {
            App.winImgAppraise.close();
        },

        cboImageSize_change: function (cbo, newValue, oldValue, eOpts) {
            App.grdImage.store.reload();
        },

        radKhongDat_change: function (rad, newValue, oldValue, eOpts) {
            if (rad.value) {
                App.txtRemark.allowBlank = false;
            }
            else {
                App.txtRemark.allowBlank = true;
            }
        },

        menuClick: function (command) {
            switch (command) {
                case "first":
                    HQ.grid.first(App.grdDet);
                    break;
                case "next":
                    HQ.grid.next(App.grdDet);
                    break;
                case "prev":
                    HQ.grid.prev(App.grdDet);
                    break;
                case "last":
                    HQ.grid.last(App.grdDet);
                    break;
                case "refresh":
                    App.grdDet.store.reload();
                    break;
                case "save":

                    break;
            }
        }
    },

    Grid: {
        grd_reject: function (col, record) {
            //var grd = col.up('grid');
            //if (!record.data.tstamp) {
            //    grd.getStore().remove(record, grd);
            //    grd.getView().focusRow(grd.getStore().getCount() - 1);
            //    grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            //} else {
            record.reject();
            //}
        },

        colBtnView_click: function (command, record) {
            if (command == "View") {
                App.frmImage.loadRecord(record);

                App.grdImage.store.reload();
                App.winImgAppraise.show();

                if (record.data.Pass) {
                    App.btnConfirm.disable();
                    if (record.data.Pass == _Pass.Dat) {
                        App.radDat.setValue(true);
                    }
                    else {
                        App.radKhongDat.setValue(true);
                    }
                }
                else {
                    App.btnConfirm.enable();
                    App.radDat.setValue(false);
                    App.radKhongDat.setValue(false);
                }
            }
        }
    },
};
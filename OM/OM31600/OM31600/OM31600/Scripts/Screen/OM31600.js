

var frmImage;
var button1;
var dataview1;
var _branchID = '';
var _dayVisit = '';
var _slsperID = '';
var _branchID1 = '';
var _custID1 = '';
var _dayVisit1 = '';
var _slsperID1 = '';


var _cboPeriodID = null;
var _levelID = '';
var keys = ['CustID'];
var fieldsCheckRequire = [""];
var fieldsLangCheckRequire = [""];

var Process = {
    
    prepareData: function (data) {
        data.CreateDate = Ext.Date.format(data.CreateDate, "Y-m-d G:i:s");
        return data;
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
    stoDisplayPeriod_load: function (sto, records, successful, eOpts) {
        if (successful) {
            HQ.common.showBusy(false);
            frmImage.getForm().loadRecord(records[0]);
            HQ.recordForm = records[0];
            HQ.common.lockItem(frmImage, !HQ.recordForm.data.isEdit);
        }
    },

    
};

var Event = {
    Form: {
        frmMain_boxReady: function () {
            //HQ.util.checkAccessRight();
            App.dtpFromDate.setValue(HQ.dateNow);
            //App.dtpToDate.setValue(HQ.dateNow);
        },

        cboTerritory_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboState.clearValue();
            App.cboBranchID.clearValue();
            App.cboState.store.reload();
        },
        cboState_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboBranchID.clearValue();
            App.cboBranchID.store.reload();
        },
        cboBranchID_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboDisplayID.store.reload();
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(newValue);
            //App.dtpToDate.validate();
        },

        btnLoad_click: function (btn, e) {
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                if (App.frmMain.isValid()) {
                    App.grdDet.store.reload();
                }
            }
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
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

var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {

        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};



var stoChanged = function () {
    HQ.isChange = false;
    //HQ.common.changeData(HQ.isChange, 'OM31600');
};


var cboTerritory_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (App.cboTerritory.valueModels) {
        App.cboState.store.reload();
    }
};

var cboState_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (App.cboState.valueModels) {
        App.cboBranchID.store.reload();
    }
};


var stoReportCheckingLevel1_Load = function (sto) {

};
var stoReportCheckingLevel2_Load = function (sto) {
};
var grdReportCheckingLevel1_AfterRender = function (obj, item, tmp) {
    _branchID = obj.record.data.CpnyID;
    _dayVisit = obj.record.data.DayVisit;
    _slsperID = obj.record.data.SlsperId;
    obj.store.reload();
}
var grdReportCheckingLevel2_AfterRender = function (obj, item, tmp) {
    _branchID1 = obj.record.data.CpnyID;
    _custID1 = obj.record.data.CustId;
    _dayVisit1 = obj.record.data.DayVisit;
    _slsperID1 = obj.record.data.SlsperId;

    obj.store.reload();
}

var renderColor = function (value, metaData, record, rowIndex, colIndex, store) {
    var newvalue;
    if (record.data.Color != '') {
        newvalue = "<span style='color:" + record.data.Color + "'>" + value + "</span>";
    } else {
        newvalue = value;
    }
    return newvalue;
}

var renderColorStartDate = function (value, metaData, record, rowIndex, colIndex, store) {
    var newvalue;
    if (record.data.Color != '') {
        newvalue = "<span style='color:" + record.data.Color + "'>" + Ext.Date.format(value, HQ.formatDateJS) + "</span>";
    } else {
        newvalue = Ext.Date.format(value, HQ.formatDateJS);
    }
    return newvalue;
}


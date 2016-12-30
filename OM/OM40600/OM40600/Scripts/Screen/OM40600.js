
var chkSelectHeader_change = function (chk, newValue, oldValue, eOpts) {
    App.stoDetail.suspendEvents();
    var allData = App.stoDetail.snapshot || App.stoDetail.allData || App.stoDetail.data;
    allData.each(function (record) {
        record.set('Selected',chk.value);
    });
    App.stoDetail.resumeEvents();
    App.grdDetail.view.refresh();
}
var chkSelectHeaderBranchID_change = function (chk, newValue, oldValue, eOpts) {
    App.stoDetailBranchID.suspendEvents();
    var allData = App.stoDetailBranchID.snapshot || App.stoDetailBranchID.allData || App.stoDetailBranchID.data;
    allData.each(function (record) {
        record.set('Selected', chk.value);
    });
    App.stoDetailBranchID.resumeEvents();
    App.grdDetailBranchID.view.refresh();
}
var chkSelectAllTerritory_change = function (chk, newValue, oldValue, eOpts) {
    HQ.combo.selectAll(App.cboTerritory);
}
var dtpFromDate_change = function (dtp, newValue, oldValue, eOpts) {
    App.dtpToDate.setMinValue(newValue);
    App.dtpToDate.validate();
}
var dtpFromDateBranchID_change = function (dtp, newValue, oldValue, eOpts) {
    App.dtpToDateBranchID.setMinValue(newValue);
    App.dtpToDateBranchID.validate();
}
var cboBranchID_Change = function (item, newValue, oldValue) {
    App.cboPJPID.setValue('');
    App.cboRoute.setValue('');
    App.cboSlsperID.setValue('');
    App.cboCustID.setValue('');
    if (item.valueModels != null && App.cboBranchID.getValue() != null && !item.hasFocus) {//truong hop co chon branchid
        App.cboPJPID.getStore().reload();
        App.cboSlsperID.getStore().reload();
        App.cboCustID.getStore().reload();
    }
};
var cboBranchID_Select = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.cboPJPID.getStore().reload();
        App.cboSlsperID.getStore().reload();
        App.cboCustID.getStore().reload();
    }
}
var cboPJPID_Change = function (item, newValue, oldValue) {
    App.cboRoute.setValue('');
    if (item.valueModels != null && App.cboBranchID.getValue() != null && !item.hasFocus) {//truong hop co chon branchid
        App.cboRoute.getStore().reload();
    }
};
var cboPJPID_Select = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.cboRoute.getStore().reload();
    }
}
var btnGenerate_click = function () {
    if (HQ.isInsert || HQ.isUpdate) {
        var recordOrder = HQ.store.findRecord(App.stoDetail, ["Selected"], [true]);
        if (App.dtpFromDate.validate() && App.dtpToDate.validate() && recordOrder != undefined) {
            //if (App.dtpFromDate.lastValue.getFullYear() != App.dtpToDate.lastValue.getFullYear()) {
            //    HQ.message.show(201506111);
            //    return;
            //}

            App.frmMain.submit({
                waitMsg: HQ.common.getLang('SavingData'),
                method: 'POST',
                url: 'OM40600/Save',
                timeout: 1800000,
                params: {
                    lstDet: HQ.store.getAllData(App.stoDetail, ["Selected"], [true]),
                    fromDate: App.dtpFromDate.getValue(),
                    toDate: App.dtpToDate.getValue()
                },
                success: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    btnLoad_click();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
    else {
        HQ.message.show(728);
    }
}
var btnGenerateBranchID_click = function () {
    if (HQ.isInsert || HQ.isUpdate) {
        if (!App.dtpFromDate.validate()) {
            App.dtpFromDate.setValue(App.dtpFromDateBranchID.getValue());
        }
        if (!App.dtpToDate.validate()) {
            App.dtpToDate.setValue(App.dtpToDateBranchID.getValue());
        }
        var recordBranchID = HQ.store.findRecord(App.stoDetailBranchID, ["Selected"], [true]);
        if (App.dtpFromDateBranchID.validate() && App.dtpToDateBranchID.validate() && recordBranchID != undefined) {
            var record = HQ.store.findRecord(App.stoDetailBranchID, ["Selected"], [true]);
            if(record)
                saveBranch(record);
        }
    }
    else {
        HQ.message.show(728);
    }
}
var saveBranch = function (record) {  
    if (record) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('SavingData'),
            method: 'POST',
            url: 'OM40600/SaveBranchID',
            timeout: 18000000,
            params: {
                BranchID: record.data.BranchID,
                PJPID: record.data.PJPID,
                SalesRouteID: record.data.SalesRouteID,
                fromDateBranchID: App.dtpFromDateBranchID.getValue(),
                toDateBranchID: App.dtpToDateBranchID.getValue()
            },
            success: function (msg, data) {
                record.data.Selected = false;
                var record1= HQ.store.findRecord(App.stoDetailBranchID, ["Selected"], [true]);
                if (record1)
                    saveBranch(record1);
                else {
                    HQ.message.process(msg, data, true);
                    btnLoadBranchID_click();
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    } 
}
var btnLoad_click = function () {
    HQ.branchID = '';
    HQ.slsperID = '';
    HQ.custID = '';
    HQ.pJPID = '';
    HQ.routeID = '';
    if (App.cboBranchID.getValue().join(',').length == 0) {
        App.cboBranchID.getStore().each(function (item) {
            HQ.branchID += item.data.BranchID + ',';
        });
    }
    else HQ.branchID = App.cboBranchID.getValue().join(',');

    if (App.cboPJPID.getValue().join(',').length == 0) {
        HQ.pJPID = '%';
    }
    else HQ.pJPID = App.cboPJPID.getValue().join(',');

    if (App.cboRoute.getValue().join(',').length == 0) {
        HQ.routeID = '%';
    }
    else HQ.routeID = App.cboRoute.getValue().join(',');

    if (App.cboSlsperID.getValue().join(',').length == 0) {
        HQ.slsperID = '%';
    }
    else HQ.slsperID = App.cboSlsperID.getValue().join(',');

    if (App.cboCustID.getValue().join(',').length == 0) {
        HQ.custID = '%';
    }
    else HQ.custID = App.cboCustID.getValue().join(',');

    App.stoDetail.reload();
}
var btnLoadBranchID_click = function () {
    App.stoDetailBranchID.reload();
}
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.dtpFromDate.setValue(HQ.bussinessDate);
    App.dtpToDate.setValue(HQ.bussinessDate);
    App.dtpFromDate.setMinValue(HQ.bussinessDate);

    App.dtpFromDateBranchID.setValue(HQ.bussinessDate);
    App.dtpToDateBranchID.setValue(HQ.bussinessDate);
        
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM40600');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM40600');

};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

/////////////////////////////////////////////////////////////////////////
var joinParams = function (multiCombo) {
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
};








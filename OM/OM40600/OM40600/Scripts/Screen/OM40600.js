//// Declare //////////////////////////////////////////////////////////

var keys = ['CountryID'];
var fieldsCheckRequire = ["CountryID"];
var fieldsLangCheckRequire = ["CountryID"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":

            HQ.grid.first(App.grdDetail);
            break;
        case "prev":
            HQ.grid.prev(App.grdDetail);
            break;
        case "next":
            HQ.grid.next(App.grdDetail);
            break;
        case "last":
            HQ.grid.last(App.grdDetail);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoCountry.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdDetail, keys);
            }
            break;
        case "delete":
            if (App.slmCountry.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoCountry, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};

var chkSelectHeader_change= function (chk, newValue, oldValue, eOpts) {
    App.stoDetail.data.each(function (record) {                    
        record.data.Selected = chk.value;           
    });
    App.grdDetail.view.refresh();
}
var dtpFromDate_change= function (dtp, newValue, oldValue, eOpts) {
    App.dtpToDate.setMinValue(newValue);
    App.dtpToDate.validate();
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
    var recordOrder = HQ.store.findRecord(App.stoDetail, ["Selected"], [true]);
    if (App.dtpFromDate.validate() && App.dtpToDate.validate() && recordOrder != undefined) {
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
var btnLoad_click = function () {
    HQ.branchID = '';
    HQ.slsperID = '';
    HQ.custID = '';
    HQ.pJPID = '';
    HQ.routeID = '';
    if (App.cboBranchID.getValue().join(',').length == 0) {
        App.cboBranchID.getStore().each(function (item) {
            HQ.branchID += item.data.BranchID+',';
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
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, 'OM40600');//khi dong roi gan lai cho change la false
        HQ.common.close(this);
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.dtpFromDate.setValue(HQ.bussinessDate);
    App.dtpToDate.setValue(HQ.bussinessDate);
   
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
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

/////////////////////////////////////////////////////////////////////////









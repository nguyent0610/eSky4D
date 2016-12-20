//// Store /////////////////////////////////////////////////////////////
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var sto_Load = function (sto) {
    HQ.common.showBusy(false);   
};
//trước khi load trang busy la dang load data
var sto_BeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
//// Event ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var firstLoad = function () {
    HQ.util.checkAccessRight();//disable cac nut ko có quyền
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    App.dteFromDate.setValue(HQ.bussinessDate);
    App.dteToDate.setValue(HQ.bussinessDate);
}
var frmChange = function () {
    if (App.cboType) {
        if (App.cboType.getValue() == '1') {
            HQ.isChange = HQ.store.isChange(App.stoPDA);
        } else if (App.cboType.getValue() == '2') {
            HQ.isChange = HQ.store.isChange(App.stoOrder);
        }
        HQ.common.changeData(HQ.isChange, 'OM42700');//co thay doi du lieu gan * tren tab title header
        HQ.common.lockItem(App.frmMain, HQ.isChange);
    }
};

var cboType_Change = function (item, newValue, oldValue) {
    if (newValue == '1') {
        App.grdPDA.show();
        App.grdOrder.hide();
    }
    else if (newValue == '2') {
        App.grdPDA.hide();
        App.grdOrder.show();
    }
    else {
        App.grdPDA.hide();
        App.grdOrder.hide();
    }
}
var ColCheck_Header_Change = function (value) {
    if (value) {
        if (App.cboType.getValue() == '1') {
            App.stoPDA.suspendEvents();
            var allData = App.stoPDA.snapshot || App.stoPDA.allData || App.stoPDA.data;
            allData.each(function (item) {
                item.set("Selected", value.checked);
            });
            App.stoPDA.resumeEvents();
        }
        else if (App.cboType.getValue() == '2') {
            App.stoOrder.suspendEvents();
            var allData = App.stoOrder.snapshot || App.stoOrder.allData || App.stoOrder.data;
            allData.each(function (item) {
                item.set("Selected", value.checked);
            });
            App.stoOrder.resumeEvents();
        }
    }
};
var ColAddShop_Header_Change = function (value) {
    if (value) {
        App.stoOrder.suspendEvents();
        var allData = App.stoOrder.snapshot || App.stoOrder.allData || App.stoOrder.data;
        allData.each(function (item) {
            item.set("IsAddStock", value.checked);
        });
        App.stoOrder.resumeEvents();
    }
}

var grdPDA_BeforeEdit = function (editor, e) {

};
var grdPDA_Edit = function (item, e) {
 
};
var grdPDA_ValidateEdit = function (item, e) {
 
};


var menuClick = function (command) {
    switch (command) {
        case "first":
            if (App.cboType.getValue() == '1') {
                HQ.grid.first(App.grdPDA);
            }
            else if (App.cboType.getValue() == '2') {
                HQ.grid.first(App.grdOrder);
            }
            break;
        case "next":
            if (App.cboType.getValue() == '1') {
                HQ.grid.next(App.grdPDA);
            }
            else if (App.cboType.getValue() == '2') {
                HQ.grid.next(App.grdOrder);
            }
            break;
        case "prev":
            if (App.cboType.getValue() == '1') {
                HQ.grid.prev(App.grdPDA);
            }
            else if (App.cboType.getValue() == '2') {
                HQ.grid.prev(App.grdOrder);
            }
            break;
        case "last":
            if (App.cboType.getValue() == '1') {
                HQ.grid.last(App.grdPDA);
            }
            else if (App.cboType.getValue() == '2') {
                HQ.grid.last(App.grdOrder);
            }
            break;
        case "save":
            if (HQ.isBusy == false) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    save();
                }
            }
            break;
       
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh', true);
            } else {
                refresh('yes');
            }
            break;       
        default:
    }
};
//// Process Data Menu click function///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA02800/Save',
            params: {
                lstSYS_Role: HQ.store.getData(App.stoSYS_Role)
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var refresh = function (item) {
    if (item == 'yes') {
        if (App.cboType.getValue() == '1') {
            App.grdPDA.store.reload();
        } else if (App.cboType.getValue() == '2') {
            App.grdOrder.store.reload();
        }
        HQ.common.lockItem(App.frmMain, false);
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////

///////////////////////////////////

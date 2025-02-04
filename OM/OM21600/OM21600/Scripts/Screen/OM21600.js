﻿//// Declare //////////////////////////////////////////////////////////
var keys = ['BranchRouteID', 'SalesRouteID'];

var fieldsCheckRequire = ["BranchRouteID", "SalesRouteID"];
var fieldsLangCheckRequire = ["BranchRouteID", "SalesRouteID"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSalesRoute.reload();
        HQ.common.showBusy(false);
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboCpnyID, HQ.isChange);
            }
            else {
                HQ.grid.first(App.grdSalesRoute);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboCpnyID, HQ.isChange);
            }
            else {
                HQ.grid.prev(App.grdSalesRoute);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboCpnyID, HQ.isChange);
            }
            else {
                HQ.grid.next(App.grdSalesRoute);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboCpnyID, HQ.isChange);
            }
            else {
                HQ.grid.last(App.grdSalesRoute);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', 'refresh');
                } else {
                    if (HQ.focus == 'header') {
                        App.cboCpnyID.setValue('');
                        
                    }
                    else {
                        HQ.grid.insert(App.grdSalesRoute, keys);
                    }
                    
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (HQ.focus == 'header') {
                    HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSalesRoute)], 'deleteData', true);
                }
                else {
                    if (App.slmSalesRoute.selected.items[0] != undefined) {
                        if (App.slmSalesRoute.selected.items[0].data.SalesRouteID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSalesRoute)], 'deleteData', true);
                        }
                    }
                }
            }
            //if (App.slmSalesRoute.selected.items[0] != undefined) {
            //    var rowindex = HQ.grid.indexSelect(App.grdSalesRoute);
            //    if (rowindex != '')
            //        HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSalesRoute), ''], 'deleteData', true)
            //}
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSalesRoute, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            //HQ.common.close(this);
            break;
    }

};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboCpnyID.getStore().addListener('load', checkLoad);
    if (HQ.requiredRouteType && HQ.allowRouteType) {
        fieldsCheckRequire = ["SalesRouteID", "RouteType"];
        fieldsLangCheckRequire = ["SalesRouteID", "RouteType"];
    }
};

var cboCpnyID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSalesRoute.loading) {
        App.stoSalesRoute.reload();
        

    }
};

var cboCpnyID_Blur = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSalesRoute.loading) {
        App.stoSalesRoute.reload();
    }
};
var cboCpnyID_Select = function (sender, value) {
    //HQ.isFirstLoad = true;
    //if (sender.valueModels != null && !App.stoSalesRoute.loading) {
    //    App.stoSalesRoute.reload();
    //}
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboCpnyID_Expand = function (sender, value) {
    //if (HQ.isChange) {
    //    App.cboCpnyID.collapse();
    //}
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboCpnyID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboCpnyID.setValue('');
    }

};
var cboTerritory_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {

        App.cboTerritory.setValue('');
        //App.cboCpnyID.setValue('');
        //App.cboCpnyID.store.clearFilter();
        //App.cboCpnyID.store.filter("Territory", '@@@@@@@@@');
        //App.cboCpnyID.store.reload();
        App.cboCpnyID.store.reload();
        App.grdSalesRoute.store.reload();

    }
};
var cboTerritory_Collapse = function (sender, value) {
    App.cboCpnyID.store.reload();
    //code = "@@@@@@@"
    //if (App.cboTerritory.getValue() != '') {
    //    code = App.cboTerritory.getValue();
    //}
    //App.cboCpnyID.setValue('');
    //App.cboCpnyID.store.clearFilter();
    //App.cboCpnyID.store.filter("Territory", code);
    App.cboBranchRouteID.store.reload();
};
var cboCpnyID_Collapse = function (sender, value) {
    App.cboBranchRouteID.store.reload();
};


//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
//var stoChanged = function (sto) {
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'OM21600');
//};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSalesRoute);
    HQ.common.changeData(HQ.isChange, 'OM21600');//co thay doi du lieu gan * tren tab title header

    if (App.cboCpnyID.valueModels == null || HQ.isNew == true)
        App.cboCpnyID.setReadOnly(false);
    else
        App.cboCpnyID.setReadOnly(HQ.isChange);

};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    //App.cboCpnyID.forceSelection = false;
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    if (!HQ.isInsert && HQ.isNew) {
        App.cboCpnyID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    App.grdSalesRoute.columns[5].setVisible(HQ.allowRouteType);
};

var grdSalesRoute_BeforeEdit = function (editor, e) {
    
    //if (App.grdSalesRoute.selModel.selected.items[0].data.tstamp != null && App.grdSalesRoute.selModel.selected.items[0].data.tstamp != "") {
    //    App.cboRouteType.setReadOnly(true);
    //}
    //else {
    //    App.cboRouteType.setReadOnly(false);
    //}

    if (e != undefined && e.field == "RouteType") {
        if (App.grdSalesRoute.selModel.selected.items[0].data.tstamp != null && App.grdSalesRoute.selModel.selected.items[0].data.tstamp != "") {
            return false;
        }
        
    }
    else
        return HQ.grid.checkBeforeEdit(e, keys);
};

var grdSalesRoute_Edit = function (item, e) {
    if (e.field == "BranchRouteID") {
        var a = HQ.store.findRecord(App.cboBranchRouteID.store, ['BranchRouteID'], [e.value]);
        if (e.value != "" && e.value != null) {
            e.record.set('DescrBranch', a.data.Descr);
        }
        else {
            e.record.set('DescrBranch', "");
        }
    }
    HQ.grid.checkInsertKey(App.grdSalesRoute, e, keys);
    frmChange();
};

var grdSalesRoute_ValidateEdit = function (item, e) {
   
    return checkValidateEdit(App.grdSalesRoute, e, keys);
};

var grdSalesRoute_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSalesRoute);
    frmChange();
};
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
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21600/Save',
            params: {
                lstOM_SalesRoute: HQ.store.getData(App.stoSalesRoute)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        var key = 0;
        if (HQ.focus == "header") {
            var indexcolum = '';
            var check = '';
            var lstDelete = App.grdSalesRoute.store.allData;
            for (var i = 0; i < lstDelete.length; i++) {
                indexcolum = indexcolum + (lstDelete.items[i].index + 1) + ",";
                check = check + lstDelete.items[i].data.SalesRouteID + ",";
            }
            checkDeleteData(indexcolum, check, key);
            
        }
        else {

            var indexcolum = '';
            var check = '';
            var lstSelete = App.grdSalesRoute.selModel.selected;
            for (var i = 0; i < lstSelete.length; i++) {
                indexcolum = indexcolum + (lstSelete.items[i].index + 1) + ",";
                check = check + lstSelete.items[i].data.SalesRouteID + ",";
            }
            key = 1;
            checkDeleteData(indexcolum, check,key);

            //App.grdSalesRoute.deleteSelected();
            //frmChange();
        }
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSalesRoute.reload();
    }
};
///////////////////////////////////
var renderRouteType = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboRouteType.findRecord("Code", rec.data.RouteType);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};


var stringFilterRouteType = function (record) {

    if (this.dataIndex == 'RouteType') {
        App.cboRouteType.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboRouteType.store, "Code", "Descr");
    }

    return HQ.grid.filterString(record, this);
}



var checkValidateEdit = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {

        var regex = /^(\w*(\d|[a-zA-Z]|[\_@()+-.]))*$/;
        //var regex = /\@^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (e.value) {
            if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
                HQ.message.show(2017120509, e.column.text);
                return false;
            }
        }
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }

    }
}

var checkDeleteData = function (indexColum, check,key) {

    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21600/CheckDelete',
            params: {
                lstIndexColum: indexColum,
                lstCheck: check
            },
            success: function (msg, data) {
                if (key == 1) {
                    App.grdSalesRoute.deleteSelected();
                }
                else {
                    App.grdSalesRoute.getStore().removeAll();
                    HQ.grid.insert(App.grdSalesRoute, keys);
                }
               
                frmChange();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};


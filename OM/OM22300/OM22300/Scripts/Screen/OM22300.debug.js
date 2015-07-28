//// Declare //////////////////////////////////////////////////////////
var keys = [''];
var fieldsCheckRequire = [""];
var fieldsLangCheckRequire = [""];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboBranchID.getStore().load(function () {
        App.cboEmployee.getStore().load(function () {
            App.cboReasonCD.getStore().load(function () {
                App.cboStatus.getStore().load(function () {
                    App.cboEmployee_Grid.getStore().load(function () {
                        App.cboReasonCD_Grid.getStore().load(function () {
                            App.cboCoaching.getStore().load(function () {
                                HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
                                App.stoOM_SalesRouteDet.reload();
                            });
                        });
                    });
                });
            });
        });
    });
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_SalesRouteDet);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_SalesRouteDet);
            break;
        case "next":
            HQ.grid.next(App.grdOM_SalesRouteDet);
            break;
        case "last":
            HQ.grid.last(App.grdOM_SalesRouteDet);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_SalesRouteDet.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_SalesRouteDet, keys);
            }
            break;
        case "delete":
            if (App.slmOM_SalesRouteDet.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_SalesRouteDet, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);
            break;
    }
};

var btnLoad_Click = function (sender, e) {
    App.grdOM_SalesRouteDet.getStore().reload();
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    //loadSourceCombo();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22300');
    //lockControl(HQ.isChange);
    //App.cboBrandID.setReadOnly(HQ.isChange);
    //App.cboSUP.setReadOnly(HQ.isChange);
    //App.cboCycle.setReadOnly(HQ.isChange);
};

//var lockControl = function (value) {
//    setTimeout(function () {
//        //App.btnAddDetail.setDisabled(value);
//        App.Period.setReadOnly(value);
//        App.cboBranchID.setReadOnly(value);
//        App.cboEmployee.setReadOnly(value);
//        App.cboReasonCD.setReadOnly(value);
//        App.cboStatus.setReadOnly(value);
//    }, 300);
//};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isFirstLoad = true;
    //var ReasonCD = [];
    //var Status = [];
    //App.cboReasonCD.getStore().data.each(function (item) {
    //    if (ReasonCD.indexOf(item.data.Code) == -1) {
    //        ReasonCD.push([item.data.Code, item.data.Descr]);
    //    }
    //});
    //App.cboStatus.getStore().data.each(function (item) {
    //    if (Status.indexOf(item.data.Code) == -1) {
    //        Status.push([item.data.Code, item.data.Descr]);
    //    }
    //});
    //filterFeature = App.grdOM_SalesRouteDet.filters;
    //colAFilter = filterFeature.getFilter('ReasonCD');
    //colAFilter.menu = colAFilter.createMenu({
    //    options: ReasonCD
    //});

    //colAFilter = filterFeature.getFilter('IsApprove');
    //colAFilter.menu = colAFilter.createMenu({
    //    options: Status
    //});

    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22300');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keys);
            //HQ.store.insertRecord(sto, keys, { DateWorking: HQ.bussinessDate, IsApprove: 'W' });
        }
        HQ.isFirstLoad = false;
    }
    stoChanged(sto);
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var grdOM_SalesRouteDet_BeforeEdit = function (editor, e) {
    if (e.field == "VisitSort")
        return true;
    else
        return false;
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdOM_SalesRouteDet_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_SalesRouteDet, e, keys);
};

//var checkValidate = function (grd, e, keys) {
//    if (keys.indexOf(e.field) != -1) {
//        if (HQ.grid.checkDuplicate(grd, e, keys)) {
//            HQ.message.show(1112, e.value);
//            return false;
//        }
//    }
//};

var grdOM_SalesRouteDet_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_SalesRouteDet, e, keys);
    //return checkValidate(App.grdOM_SalesRouteDet, e, keys);
};

var grdOM_SalesRouteDet_Reject = function (record) {
    //record.reject();
    HQ.grid.checkReject(record, App.grdOM_SalesRouteDet);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM22300/Save',
            params: {
                lstOM_SalesRouteDet: HQ.store.getData(App.stoOM_SalesRouteDet)
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
        App.grdOM_SalesRouteDet.deleteSelected();
        stoChanged(App.stoOM_SalesRouteDet);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_SalesRouteDet.reload();
    }
};

///////////////////////////////////
var renderVisitOfWeek = function (value, metaData, rec, rowIndex, colIndex, store) {
    value = value == "NA" ? "None" :
    value == "OW" ? "OddWeek" :
    value == "EW" ? "EvenWeek" :
    value == "W159" ? "Weeks" :
    value == "W2610" ? "Weeks" :
    value == "W3711" ? "Weeks" :
    value == "W4812" ? "Weeks" : value;

    var sufstr = '';
    if (value == "W159")
        sufstr = " 1,5,9,...";
    else if (value == "W2610")
        sufstr = " 2,6,10,...";
    else if (value == "W3711")
        sufstr = " 3,7,11,...";
    else if (value == "W4812")
        sufstr = " 4,8,12,...";

    return (HQ.common.getLang(value) + sufstr);
};

var renderSlsFreq = function (value, metaData, rec, rowIndex, colIndex, store) {
    value = value == "A" ? "Arbitrary" : value;
    return HQ.common.getLang(value);
};

var renderSlsFreqType = function (value, metaData, rec, rowIndex, colIndex, store) {
    value = value == "A" ? "AdHoc" :
        value == "R" ? "Recurrent" : value;
    return HQ.common.getLang(value);
};

var renderDayOfWeek = function (value, metaData, rec, rowIndex, colIndex, store) {
    value = value == "Sun" ? "Sun" :
    value == "Sat" ? "Sat" :
    value == "Fri" ? "Fri" :
    value == "Thu" ? "Thu" :
    value == "Wed" ? "Wed" :
    value == "Tue" ? "Tue" :
    value == "Mon" ? "Mon" : value;
    return HQ.common.getLang(value);
};

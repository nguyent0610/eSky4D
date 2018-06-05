//// Declare //////////////////////////////////////////////////////////

var keys = ['CompID'];
var fieldsCheckRequire = ["CompID", "CompName"];
var fieldsLangCheckRequire = ["CompID", "CompName"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_CompetitorVendor.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_CompetitorVendor);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_CompetitorVendor);
            break;
        case "next":
            HQ.grid.next(App.grdOM_CompetitorVendor);
            break;
        case "last":
            HQ.grid.last(App.grdOM_CompetitorVendor);
            break;
        case "refresh":
	
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_CompetitorVendor.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_CompetitorVendor, keys);
            }
            break;
        case "delete":
            if (App.slmOM_CompetitorVendor.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmOM_CompetitorVendor.selected.items[0] != undefined) {
                        if (App.slmOM_CompetitorVendor.selected.items[0].data.Code != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_CompetitorVendor)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_CompetitorVendor, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":         
            break;
    }

};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad(); // Mới
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoOM_CompetitorVendor);
    HQ.common.changeData(HQ.isChange, 'OM20070');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_CompetitorVendor.reload();
    }
};


//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdOM_CompetitorVendor_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_CompetitorVendor_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_CompetitorVendor, e, keys);
};
var grdOM_CompetitorVendor_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_CompetitorVendor, e, keys);
};
var grdOM_CompetitorVendor_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_CompetitorVendor);
    frmChange();
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM20070/Save',
            params: {
                lstOM_CompetitorVendor: HQ.store.getData(App.stoOM_CompetitorVendor)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_CompetitorVendor.deleteSelected();
        frmChange();
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_CompetitorVendor.reload();
    }
};
///////////////////////////////////









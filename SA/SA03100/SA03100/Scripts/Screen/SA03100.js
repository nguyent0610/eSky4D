//// Declare //////////////////////////////////////////////////////////

var keys = ['GroupID'];
var fieldsCheckRequire = ["GroupID", "Descr", "ListCpny"];
var fieldsLangCheckRequire = ["GroupID", "Descr", "ListCpny"];
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
        App.stoSYS_CompanyGroup.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_CompanyGroup);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_CompanyGroup);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_CompanyGroup);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_CompanyGroup);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_CompanyGroup.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_CompanyGroup, keys);
            }
            break;
        case "delete":
            if (App.slmSYS_CompanyGroup.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmSYS_CompanyGroup.selected.items[0] != undefined) {
                        if (App.slmSYS_CompanyGroup.selected.items[0].data.GroupID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_CompanyGroup)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_CompanyGroup, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var grdSYS_CompanyGroup_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_CompanyGroup_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_CompanyGroup, e, keys);
};
var grdSYS_CompanyGroup_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_CompanyGroup, e, keys);
};
var grdSYS_CompanyGroup_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_CompanyGroup);
    frmChange();
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA03100/Save',
            params: {
                lstSYS_CompanyGroup: HQ.store.getData(App.stoSYS_CompanyGroup)
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
        App.grdSYS_CompanyGroup.deleteSelected();
        frmChange();
    }
};

/////////////////////////////////////////////////////////////////////////
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad(); // Mới
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSYS_CompanyGroup);
    HQ.common.changeData(HQ.isChange, 'SA03100');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_CompanyGroup.reload();
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


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_CompanyGroup.reload();
    }
};
///////////////////////////////////

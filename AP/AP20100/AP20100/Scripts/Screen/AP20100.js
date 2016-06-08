//// Declare //////////////////////////////////////////////////////////
var keys = ['ClassID'];
var fieldsCheckRequire = ["ClassID", "Descr", "Terms"];
var fieldsLangCheckRequire = ["ClassID", "Descr", "Terms"];
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
        App.stoAP_VendClass.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdAP_VendClass);
            break;
        case "prev":
            HQ.grid.prev(App.grdAP_VendClass);
            break;
        case "next":
            HQ.grid.next(App.grdAP_VendClass);
            break;
        case "last":
            HQ.grid.last(App.grdAP_VendClass);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoAP_VendClass.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdAP_VendClass, keys);
            }
            break;
        case "delete":
            if (App.slmAP_VendClass.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdAP_VendClass);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdAP_VendClass), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoAP_VendClass, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboTerms.getStore().addListener('load', checkLoad);
};


//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var frmChange = function (sto) {
    HQ.isChange = HQ.store.isChange(App.stoAP_VendClass);
    HQ.common.changeData(HQ.isChange, 'AP20100');
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

var grdAP_VendClass_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdAP_VendClass_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdAP_VendClass, e, keys);
};

var grdAP_VendClass_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAP_VendClass, e, keys);
};

var grdAP_VendClass_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAP_VendClass);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'AP20100/Save',
            params: {
                lstAP_VendClass: HQ.store.getData(App.stoAP_VendClass)
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
        App.grdAP_VendClass.deleteSelected();
        stoChanged(App.stoAP_VendClass);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoAP_VendClass.reload();
    }
};
///////////////////////////////////
//// Declare //////////////////////////////////////////////////////////
var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Descr"];
var fieldsLangCheckRequire = ["Code", "Descr"];

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
        App.stoChannel.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdChannel);
            break;
        case "prev":
            HQ.grid.prev(App.grdChannel);
            break;
        case "next":
            HQ.grid.next(App.grdChannel);
            break;
        case "last":
            HQ.grid.last(App.grdChannel);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoChannel.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdChannel, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmChannel.selected.items[0] != undefined) {
                    if (App.slmChannel.selected.items[0].data.Code != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdChannel)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoChannel, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoChannel);
    HQ.common.changeData(HQ.isChange, 'AR21100');//co thay doi du lieu gan * tren tab title header
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

var grdChannel_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdChannel_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdChannel, e, keys);
    frmChange();
};

var grdChannel_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdChannel, e, keys);
};

var grdChannel_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdChannel);
    frmChange();
};


/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'AR21100/Save',
            params: {
                lstChannel: HQ.store.getData(App.stoChannel)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdChannel.deleteSelected();
        frmChange();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoChannel.reload();
    }
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

/////////////////////////////////////////////////////////////////////////









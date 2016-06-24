//// Declare //////////////////////////////////////////////////////////

var keys = ['DiscCode'];
var fieldsCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate"];
var fieldsLangCheckRequire = ["DiscCode", "Descr", "FromDate", "ToDate"];

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
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
        App.stoOM_DiscDescr.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_DiscDescr);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_DiscDescr);
            break;
        case "next":
            HQ.grid.next(App.grdOM_DiscDescr);
            break;
        case "last":
            HQ.grid.last(App.grdOM_DiscDescr);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_DiscDescr.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_DiscDescr, keys);
            }
            break;
        case "delete":
            //if (App.slmOM_DiscDescr.selected.items[0] != undefined) {
            //    if (HQ.isDelete) {
            //        HQ.message.show(11, '', 'deleteData');
            //    }
            //}
            if (HQ.isDelete) {
                if (App.slmOM_DiscDescr.selected.items[0] != undefined) {
                    if (App.slmOM_DiscDescr.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_DiscDescr)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_DiscDescr, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    checkLoad();
    
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
//var stoChanged = function (sto) {
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'OM21500');
//};
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoOM_DiscDescr);
    HQ.common.changeData(HQ.isChange, 'OM21500');//co thay doi du lieu gan * tren tab title header
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }


    if (!HQ.isInsert && HQ.isNew) {        
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
};
var grdOM_DiscDescr_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_DiscDescr_Edit = function (item, e) {
    if (e.field == 'ToDate') {
        if (e.record.data.FromDate != null) {
            if (e.value < e.record.data.FromDate) {
                HQ.message.show(2015061501, '', '');
                e.record.set('ToDate', e.record.data.FromDate);
                return false;
            }
        }
    } else if (e.field == 'FromDate') {
        if (e.record.data.ToDate != null) {
            if (e.value > e.record.data.ToDate) {
                HQ.message.show(2015061501, '', '');
                e.record.set('FromDate', e.record.data.ToDate);
                return false;
            }
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_DiscDescr, e, keys);

};
var grdOM_DiscDescr_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_DiscDescr, e, keys);
};
var grdOM_DiscDescr_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_DiscDescr);
    frmChange();
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21500/Save',
            params: {
                lstOM_DiscDescr: HQ.store.getData(App.stoOM_DiscDescr)
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
        App.grdOM_DiscDescr.deleteSelected();
        frmChange();
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_DiscDescr.reload();
    }
};

var checkFromDate = function () {
};
var checkToDate = function () {
};
///////////////////////////////////
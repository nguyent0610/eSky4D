//// Declare //////////////////////////////////////////////////////////

var keys = ['EntryID'];
///////////////////////////////////////////////////////////////////////
var fieldsCheckRequire = ['EntryID', 'descr', 'RcptDisbFlg'];
var fieldsLangCheckRequire = ["EntryID", "Descr", "RcptDisbFlg"];

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
        App.stoEntryType.reload();
        HQ.common.showBusy(false);
    }
};

//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdEntryType);
            break;
        case "prev":
            HQ.grid.prev(App.grdEntryType);
            break;
        case "next":
            HQ.grid.next(App.grdEntryType);
            break;
        case "last":
            HQ.grid.last(App.grdEntryType);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoEntryType.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdEntryType);
            }
            break;
        case "delete":
            if (App.slmEntryType.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmEntryType.selected.items[0] != undefined) {
                        if (App.slmEntryType.selected.items[0].data.CostID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdEntryType)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoEntryType, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    // HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    checkLoad(); // Mới
};


var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoEntryType);
    HQ.common.changeData(HQ.isChange, 'CA20100');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    stoLoad
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoEntryType.reload();
    }
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
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
var grdEntryType_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdEntryType_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdEntryType, e, keys);
    frmChange();
};
var grdEntryType_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdEntryType, e, keys);
};

var grd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdEntryType);
    frmChange();
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'CA20100/Save',
            params: {
                lstEntryType: HQ.store.getData(App.stoEntryType)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
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
        App.grdEntryType.deleteSelected();
        frmChange();
    }
};

//kiem tra key da nhap du chua
var isAllValidKey = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var j = 0; j < keys.length; j++) {
                if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
                    return false;
            }
        }
        return true;
    } else {
        return true;
    }
};
//kiem tra nhung field yeu cau bat buoc nhap
//var checkRequire = function (items) {
//    if (items != undefined) {
//        for (var i = 0; i < items.length; i++) {
//            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
//            if (items[i]["EntryID"].trim() == "") {
//                HQ.message.show(15,HQ.common.getLang("EntryID"));
//                return false;
//            }

//            if (items[i]["descr"].trim() == "") {
//                HQ.message.show(15, HQ.common.getLang("Descr"));
//                return false;
//            }
//            if (items[i]["RcptDisbFlg"].trim() == "") {
//                HQ.message.show(15, HQ.common.getLang("RcptDisbFlg"));
//                return false;
//            }

//        }
//        return true;
//    } else {
//        return true;
//    }
//};
/////////////////////////////////////////////////////////////////////////



//// Other Functions ////////////////////////////////////////////////////

var ActiveCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdEntryType.getStore().each(function (item) {

            item.set("active", value.checked);
        });
    }
};

var change = function (value) {
    var record = App.stoMasterType.findRecord("Code", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
    //var descr = "";
    //for (var i = 0; i < App.stoMasterType.getCount() ; i++) {
    //    if (App.stoMasterType.data.items[i].data.Code == value) {
    //        descr = App.stoMasterType.data.items[i].data.Descr;
    //        break;
    //    }
    //}
    //if (descr) {
    //    return descr;
    //}
    //else {
    //    return value;
    //}
}
/////////////////////////////////////////////////////////////////////////









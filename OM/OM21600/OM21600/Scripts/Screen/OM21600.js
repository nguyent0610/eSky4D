//// Declare //////////////////////////////////////////////////////////

var keys = ['SalesRouteID', 'Descr'];
var fieldsCheckRequire = ["SalesRouteID", "Descr"];
var fieldsLangCheckRequire = ["SalesRouteID", "Descr"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":

            HQ.grid.first(App.grdSalesRoute);
            break;
        case "prev":
            HQ.grid.prev(App.grdSalesRoute);
            break;
        case "next":
            HQ.grid.next(App.grdSalesRoute);
            break;
        case "last":
            HQ.grid.last(App.grdSalesRoute);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoSalesRoute.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSalesRoute, keys);
            }
            break;
        case "delete":
            if (App.slmSalesRoute.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
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
            if (HQ.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSalesRoute_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSalesRoute_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSalesRoute, e, keys);
};
var grdSalesRoute_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSalesRoute, e, keys);
};
var grdSalesRoute_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSalesRoute);
    stoChanged(App.stoSalesRoute);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM21600/Save',
            params: {
                lstSalesRoute: HQ.store.getData(App.stoSalesRoute)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
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
        App.grdSalesRoute.deleteSelected();
        stoChanged(App.stoSalesRoute);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, 'OM21600');//khi dong roi gan lai cho change la false
        HQ.common.close(this);
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSalesRoute.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21600');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM21600');
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









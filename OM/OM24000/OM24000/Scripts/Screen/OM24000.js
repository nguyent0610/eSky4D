//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
var fieldsCheckRequire = ["Code", "Lang00", "Lang01"];
var fieldsLangCheckRequire = ["Code", "Lang00", "Lang01"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdPPC_DiscConsumers);
            break;
        case "prev":
            HQ.grid.prev(App.grdPPC_DiscConsumers);
            break;
        case "next":
            HQ.grid.next(App.grdPPC_DiscConsumers);
            break;
        case "last":
            HQ.grid.last(App.grdPPC_DiscConsumers);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoPPC_DiscConsumers.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                //HQ.grid.insert(App.grdPPC_DiscConsumers, keys);
            }
            break;
        case "delete":
            if (App.slmPPC_DiscConsumers.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoPPC_DiscConsumers, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var firstLoad = function () {
    HQ.isFirstLoad = true;
    //App.stoPPC_DiscConsumers.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM24000');
    App.cboBranchID.setReadOnly(HQ.isChange);
    App.cboSlsperID.setReadOnly(HQ.isChange);
    App.cboCustID.setReadOnly(HQ.isChange);
    App.FromDate.setReadOnly(HQ.isChange);
    App.ToDate.setReadOnly(HQ.isChange);
    App.btnLoad.setDisabled(HQ.isChange);
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM24000');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    stoChanged(sto);
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdPPC_DiscConsumers_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdPPC_DiscConsumers_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdPPC_DiscConsumers, e, keys);

};
var grdPPC_DiscConsumers_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdPPC_DiscConsumers, e, keys);
};
var grdPPC_DiscConsumers_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdPPC_DiscConsumers);
    stoChanged(App.stoPPC_DiscConsumers);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM24000/Save',
            params: {
                lstPPC_DiscConsumers: HQ.store.getData(App.stoPPC_DiscConsumers)
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
        App.grdPPC_DiscConsumers.deleteSelected();
        stoChanged(App.stoPPC_DiscConsumers);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoPPC_DiscConsumers.reload();
    }
};
///////////////////////////////////



var btnLoad_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        App.stoPPC_DiscConsumers.reload();
    }
};
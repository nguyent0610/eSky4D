//// Declare //////////////////////////////////////////////////////////
var keys = ['Country', 'AR_Det'];
var fieldsCheckRequire = ["Country", "AR_Det"];
var fieldsLangCheckRequire = ["Country", "AR_Det"];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdAR_Det);
            break;
        case "prev":
            HQ.grid.prev(App.grdAR_Det);
            break;
        case "next":
            HQ.grid.next(App.grdAR_Det);
            break;
        case "last":
            HQ.grid.last(App.grdAR_Det);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoAR_Det.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdAR_Det, keys);
            }
            break;
        case "delete":
            if (App.slmAR_Det.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdAR_Det);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdAR_Det), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoAR_Det, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoAR_Det.reload();
};

var cboBranchID_Change = function (sender, newValue, oldValue) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoAR_Det.loading) {     
        App.stoAR_Det.reload();
    }

};
var cboBranchID_Select = function (sender) {
    if (sender.valueModels != null && !App.stoAR_Det.loading) {
        App.stoAR_Det.reload();
    }
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IF30100');
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IF30100');
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
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IF30100/Save',
            params: {
                lstAR_Det: HQ.store.getData(App.stoAR_Det)
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
        App.grdAR_Det.deleteSelected();
        stoChanged(App.stoAR_Det);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoAR_Det.reload();
    }
};
///////////////////////////////////





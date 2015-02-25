//// Declare //////////////////////////////////////////////////////////
var screenNbr = 'SI20500';
var keys = ['Country', 'State', 'City'];
var fieldsCheckRequire = ["Country", "State", "City", "Name"];
var fieldsLangCheckRequire = ["Country", "State", "City", "Name"];
//// Event //////////////////////////////////////////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_City);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_City);
            break;
        case "next":
            HQ.grid.next(App.grdSI_City);
            break;
        case "last":
            HQ.grid.last(App.grdSI_City);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoSI_City.reload();
         
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_City,keys);
            }
            break;
        case "delete":
            if (App.slmSI_City.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_City, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }                    
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSI_City)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var cboCountry_Change = function () {
    App.slmSI_City.getSelection()[0].set('State', '');
};
var grdSI_City_BeforeEdit = function (editor, e) {
    //HQ.grid.checkBeforeEdit(e, keys) kiem tra cho phep nhap lieu hay ko xem them trong HQ.grid
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
    if (e.field == 'State' ) {     
        App.cboState.store.load();
    }
};
var grdSI_City_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSI_City, e, keys);
  
};
var grdSI_City_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSI_City, e, keys);    
};
var grdSI_City_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSI_City);
};
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI20500/Save',
            params: {
                lstSI_City: HQ.store.getData(App.stoSI_City)
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
        App.grdSI_City.deleteSelected();
    }
};
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, screenNbr);
        HQ.common.close(this);
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSI_City.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, screenNbr);
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, screenNbr);
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
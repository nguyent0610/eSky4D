//// Declare //////////////////////////////////////////////////////////
var screenNbr = 'SI21700';
var keys = ['Country', 'State', 'District'];
var fieldsCheckRequire = ["Country", "State", "District", "Name"];
var fieldsLangCheckRequire = ["Country", "State", "District", "Name"];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSI_District);
            break;
        case "prev":
            HQ.grid.prev(App.grdSI_District);
            break;
        case "next":
            HQ.grid.next(App.grdSI_District);
            break;
        case "last":
            HQ.grid.last(App.grdSI_District);
            break;
        case "refresh":
            HQ.isFirstLoad = true;
            App.stoSI_District.reload();         
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSI_District);
            }
            break;
        case "delete":
            if (App.slmSI_District.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSI_District, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSI_District)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
//neu cboCountry thay đổi thì reload lại store của cboState và set giá trị rỗng cho cboState
var cboCountry_Change = function () {
    App.slmSI_District.selected.items[0].set('State', '');
};

var grdSI_District_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;  
    //nếu cell đang click vào là State thì reload store của cboState
    if (e.field == "State") {
        App.cboState.getStore().reload();
    }   
};

var grdSI_District_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSI_District, e, keys);
};
var grdSI_District_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSI_District, e, keys);
};

var grd_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSI_District);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21700/Save',
            params: {
                lstSI_District: HQ.store.getData(App.stoSI_District)
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
        App.grdSI_District.deleteSelected();
        stoChanged(App.stoSI_District);
    }
};

/////////////////////////////////////////////////////////////////////////
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
    App.stoSI_District.reload();
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
        menuClick('new');
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
/////////////////////////////////////////////////////////////////////////









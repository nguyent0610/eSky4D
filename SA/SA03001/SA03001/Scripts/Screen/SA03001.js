//// Declare //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////
var keys = ['UserName'];
var _CustID = '';
var _tstamp = '';
var _selBranch = [];
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdUser);
            break;
        case "prev":
            HQ.grid.prev(App.grdUser);
            break;
        case "next":
            HQ.grid.next(App.grdUser);
            break;
        case "last":
            HQ.grid.last(App.grdUser);
            break;
        case "refresh":
            App.stoUser.reload();
            break;
        case "new":
            if (HQ.isInsert) {
                App.winLocation.setTitle("New")
                App.winLocation.show();
                App.txtUserName.setReadOnly(false);
                App.txtUserName.setValue("");
                App.txtFirstName.setValue("");
                App.txtPassWord.setValue("");
                App.txtCpnyID.setValue("");
                App.txtEmail.setValue("");
                App.cboUserTypes.setValue("");
                App.dteBlockedTime.setValue(new Date());
                App.dteLastLoggedIn.setValue("");
                App.cboManager.setValue("");
                App.txtFailedLoginCount.setValue(0);
                App.cboUserGroup.setValue("");
                HQ.isNew = true;

            }
            break;
        //case "delete":
        //    if (App.slmPO_CostPurchasePrice.selected.items[0] != undefined) {
        //        if (HQ.isDelete) {
        //            HQ.message.show(2015122301, '', 'deleteData');
        //        }
        //    }
        //    break;
        //case "save":
        //    if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
        //        if (HQ.store.checkRequirePass(App.stoPO_CostPurchasePrice, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
        //            save();
        //        }
        //    }
        //    break;
        //case "print":
        //    break;
        case "close":
            HQ.common.close(this);            
            break;
    }

};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau

var firstLoad = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
   
    App.stoUser.reload();
    HQ.util.checkAccessRight();
};

//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid

var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03001');
};


//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA03001');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    HQ.common.showBusy(false);// su dung khi cuoi cung

   
};

var stoBranch_Load = function (sto) {
    sto.suspendEvents();
    for (var i = 0; i < _selBranch.length; i++) {
        var record = HQ.store.findRecord(sto, ['BranchID'], [_selBranch[i]]);
        if (record) {
            record.data.Check = true;
        }
    }
    sto.resumeEvents();
    App.grdBranch.view.refresh();
    //App.chkActive_All.setValue(a);
    HQ.common.showBusy(false)
};

var stoForm_load = function (sto) {
    var record = sto.getAt(0);
    App.txtPassWord.setValue(record.data.Password);
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    
};

var grdPO_CostPurchasePrice_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdPO_CostPurchasePrice_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdPO_CostPurchasePrice, e, keys);
};

var grdPO_CostPurchasePrice_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdPO_CostPurchasePrice, e, keys);
};

var grdPO_CostPurchasePrice_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdPO_CostPurchasePrice);
    stoChanged(App.stoPO_CostPurchasePrice);
};
var btnEdit_Click = function (record) {
    _tstamp = record.data.tstamp;
    App.frmDetail.loadRecord(record);
    App.txtCpnyID.setValue(record.data.CpnyID.split(','));
    App.txtUserID.setValue(record.data.UserName);
    App.txtUserName.setReadOnly(true);
    App.winLocation.setTitle("Edit");
    HQ.isNew = false;
    //App.cboUserTypes.forceSelection = false;
    //App.cboUserGroup.forceSelection = false;
    App.winLocation.show();
    App.stoForm.reload();
};
var btnLocationCancel_Click = function () {
    App.winLocation.hide();
};

var btnAddCustomer_Click = function () {
    App.chkActive_All.setValue(false);
    App.stoBranch.reload();
    _selBranch = App.txtCpnyID.value;

    App.winBranch.show();
};
var chkActiveAll_Change = function (sender, value, oldValue) {
    if (sender.hasFocus) {
        var store = App.stoBranch;
        var allRecords = store.allData;
        store.suspendEvents();
        allRecords.each(function (record) {
            record.set('Check', value);
        });
        store.resumeEvents();
        App.grdBranch.view.refresh();
        if (value == false) {
            _selBranch = [];
        }
    }
};
var btnBranchOK_Click = function () {
    var res = "";
    var store = App.stoBranch;
    var allRecords = store.snapshot || store.allData || store.data;
    store.suspendEvents();
    allRecords.each(function (record) {
        if (record.data.Check == true) {
            res += record.data.BranchID + ',';
        }
    });
    store.resumeEvents();

    App.txtCpnyID.setValue(res.split(','));
    App.winBranch.hide();
};
var btBranchCancel_Click = function () {
    App.winBranch.hide();
};
var btnLocationOK_Click = function () {
    if (HQ.isUpdate == false && HQ.isInsert) {
        return;
    }
    if (App.txtUserName.getValue() == null)
    {
        HQ.message.show(15, App.txtUserName.fieldLabel);
        return;
    }
    if (App.txtFirstName.getValue() == null)
    {
        HQ.message.show(15, App.txtFirstName.fieldLabel);
        return;
    }
    if (App.txtPassWord.getValue() == null)
    {
        HQ.message.show(15, App.txtPassWord.fieldLabel);
        return;
    }
    if (App.txtEmail.getValue() == null)
    {
        HQ.message.show(15, App.txtPassWord.fieldLabel);
        return;
    }
    if (App.txtCpnyID.getValue() == null)
    {
        HQ.message.show(15, App.txtPassWord.fieldLabel);
        return;
    }
    if (App.cboUserTypes.getValue() == null)
    {
        HQ.message.show(15, App.cboUserTypes.fieldLabel);
        return;
    }
    save();
};


/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {

    var user = App.txtUserName.getValue();
    if (!HQ.util.checkSpecialChar(user.trim()))
    {
        HQ.message.show(20140811, App.txtUserName.fieldLabe);
        return;

    }
    if (Ext.isEmpty(App.txtCpnyID.getValue())) {
        HQ.message.show(15, App.txtCpnyID.fieldLabel);
        return;
    }
    if (!HQ.util.checkEmail(App.txtEmail.getValue()))
    {
        return;
    }
    
    App.frmDetail.submit({
        timeout: 1800000,
        waitMsg: HQ.common.getLang("SavingData"),
        url: 'SA03001/Save',
        params: {
            lstUser: Ext.encode(App.stoForm.getRecordsValues()),
            valueBloked: App.ckbBloked.getValue(),
            lstCpnyID : App.txtCpnyID.getValue().join(','),
            isNewUser: HQ.isNew,
            //valueStartDate: App.dtpStartDate.getValue().toDateString(),
            //valueEndDate: App.dtpEndDate.getValue().toDateString(),
            valueTstamp: _tstamp
        },
        success: function (msg, data) {
            HQ.message.show(201405071);
            App.stoUser.reload();
            App.winLocation.hide();
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////

var checkValidateEdit = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
    }
};
//// Renderer
var rendererPassword = function (value) {
    var r = value.length;
    var a = "";
    for (var i = 0; i < r; i++) {
        a += "*";
    }
    return a;
};


var expand = function (cbo, delimiter) {
    cbo.store.clearFilter();
    if (cbo.getValue())
        cbo.setValue(cbo.getValue().toString().replace(new RegExp(' ', 'g'), '').replace(new RegExp(delimiter, 'g'), ',').split(','));
};
//// Declare //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////
var keys = ['UserName'];
var _CustID = '';
var _tstamp = '';
var _selBranch = [];
var _userName = '';
var dsr = [];
var check = false;
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
                if (HQ.IsShowUserTypes) {

                    App.cboUserTypes.hide();
                    //App.conHide.show();
                    if (check == false) {
                        App.cboStatus.setMargin("5 0 0 0");
                        App.dtpStartWork.setMargin("5 0 0 0");
                        App.dtpEndWork.setMargin("5 0 0 0");
                        check = true;
                    }
                    App.cboUserTypes.allowBlank = true;
                    App.cboUserTypes.isValid();
                }
                else {
                    App.cboUserTypes.show();
                    //App.conHide.hide();
                    App.cboUserTypes.allowBlank = false;
                    App.cboUserTypes.isValid();
                }
                if (HQ.IsCheckFirstLogin) {
                    App.ckbCheckFirstLogin.hide();
                }
                else {
                    App.ckbCheckFirstLogin.show();
                }
                if (HQ.IsAddress) {
                    App.txtAddress.hide();
                }
                else {
                    App.txtAddress.show();
                }
                if (HQ.IsTel) {
                    App.txtTel.hide();
                }
                else {
                    App.txtTel.show();
                }
                if (HQ.IsChannel) {
                    App.cboChannel.hide();
                }
                else {
                    App.cboChannel.show();
                }
                if (HQ.IsMultiLogin) {
                    App.ckbMultiLogin.hide();
                }
                else {
                    App.ckbMultiLogin.show();
                }
                if (HQ.IsRequiredCpny) {
                    App.txtCpnyID.allowBlank = false;
                }
                else {
                    App.txtCpnyID.allowBlank = true;
                }
                if (HQ.IsBrandID) {
                    App.cboBrandID.hide();
                    App.cboBrandID.allowBlank = true;
                }
                else {
                    App.cboBrandID.show();
                    App.cboBrandID.allowBlank = false;
                }
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
                App.dtpBeginDay.setValue(new Date());
                App.dtpStartWork.setValue(new Date());
                App.dtpEndWork.setValue(new Date());
                App.txtExpireDay.setValue(0);
                App.cboStatus.setValue('AC');
                App.ckbCheckFirstLogin.setValue(0);
                App.txtAddress.setValue('');
                App.ckbMultiLogin.setValue(0);
                App.cboChannel.setValue('');
                App.txtTel.setValue('');
                App.txtUserName.isValid();
                App.txtFirstName.isValid();
                App.txtPassWord.isValid();
                App.txtEmail.isValid();
                App.txtCpnyID.isValid();
                App.cboStatus.isValid();
                App.chkAuto.setValue(0);
                App.cboBrandID.setValue('');
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
    if (HQ.IsShowUserTypes) {
        App.UserTypes.hide();
    }
    else
    {
        App.UserTypes.show();
    }
    if (HQ.IsCheckFirstLogin)
    {
        App.CheckFirstLogin.hide();
    }
    else
    {
        App.CheckFirstLogin.show();
    }
    if (HQ.IsAddress)
    {
        App.Address.hide();
    }
    else {
        App.Address.show();
    }
    if (HQ.IsTel) {
        App.Tel.hide();
    }
    else {
        App.Tel.show();
    }
    if (HQ.IsChannel) {
        App.Channel.hide();
    }
    else {
        App.Channel.show();
    }
    if (HQ.IsMultiLogin) {
        App.MultiLogin.hide();
    }
    else {
        App.MultiLogin.show();
    }
    if (HQ.IsBrandID) {
        App.BrandID.hide();
    }
    else {
        App.BrandID.show();
    }
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
    HQ.grid.hide(App.grdUser, App.stoUser.data.items[0].data.HideColumn.split(','));
   
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
    if (HQ.IsShowUserTypes) {
        
        App.cboUserTypes.hide();
        //App.conHide.show();
        if (check == false)
        {
            App.cboStatus.setMargin("5 0 0 0");
            App.dtpStartWork.setMargin("5 0 0 0");
            App.dtpEndWork.setMargin("5 0 0 0");
            check = true;
        }
        
    }
    else {
        App.cboUserTypes.show();
        //App.conHide.hide();
    }
    if (HQ.IsCheckFirstLogin)
    {
        App.ckbCheckFirstLogin.hide();
    }
    else {
        App.ckbCheckFirstLogin.show();
    }
    if (HQ.IsAddress)
    {
        App.txtAddress.hide();
    }
    else {
        App.txtAddress.show();
    }
    if (HQ.IsTel) {
        App.txtTel.hide();
    }
    else {
        App.txtTel.show();
    }
    if (HQ.IsChannel) {
        App.cboChannel.hide();
    }
    else {
        App.cboChannel.show();

    }
    if (HQ.IsMultiLogin) {
        App.ckbMultiLogin.hide();
    }
    else {
        App.ckbMultiLogin.show();
    }
    if (HQ.IsBrandID) {
        App.cboBrandID.hide();
        App.cboBrandID.allowBlank = true;
    }
    else {
        App.cboBrandID.show();
        App.cboBrandID.allowBlank = false;
       
    }
    if (HQ.IsMultiChannel) {
        App.cboChannel.multiSelect = true;
    }
    _tstamp = record.data.tstamp;
    App.frmDetail.loadRecord(record);
    App.txtCpnyID.setValue(record.data.CpnyID.split(','));
    App.txtUserID.setValue(record.data.UserName);
    App.txtUserName.setReadOnly(true);
    App.winLocation.setTitle("Edit");
    HQ.isNew = false;
    //App.ckbCheckFirstLogin.setValue(0);
    //App.cboUserTypes.forceSelection = false;
    //App.cboUserGroup.forceSelection = false;
    
    App.winLocation.show();
    App.stoForm.reload();
    HQ.combo.expand(App.cboBrandID, ',');
    HQ.combo.expand(App.cboChannel, ',')
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
var btnReplace_Click = function () {
    if (App.slmUser.selected.items[0] != undefined) {
        _userName = App.slmUser.selected.items[0].data.UserName;
        App.txtUserNameOld.setValue(_userName);
        App.cboUserBy.store.reload();
        treeAVC_AfterRender('pnlTreeAVC', _userName);
        App.winReplace.show();
    }
};
var btnUpdate_Click = function () {
    getSelected();
    if (Ext.isEmpty(App.cboUserBy.getValue()))
    {
        HQ.message.show(15, App.cboUserBy.fieldLabel);
        return;
    }
    if (dsr.join(',') == "")
    {
        HQ.message.show(2018052801);
        return;
    }
    App.direct.UpdateSalesForce(App.txtUserNameOld.getValue(), App.cboUserBy.getValue(), dsr.join(','), {
        success: function (result) {
            treeAVC_AfterRender('pnlTreeAVC', App.txtUserNameOld.getValue());
            treeAVC_AfterRenderUserReplace('pnlTreeAVCUserReplace', App.cboUserBy.getValue());
        }
    });
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
var dtpStartWork_Change = function (dtp, newValue, oldValue, eOpts) {
    App.dtpEndWork.setMinValue(App.dtpStartWork.getValue());
    if (App.dtpEndWork.getValue() < App.dtpStartWork.getValue()) {
        App.dtpEndWork.setValue(App.dtpStartWork.getValue());
    }
};
var cboUserBy_Change = function (sender, value, oldValue) {
    if (sender.valueModels != null) {
        treeAVC_AfterRenderUserReplace('pnlTreeAVCUserReplace', value);
    }
};
var cboUserBy_Select = function (sender, value, oldValue) {
    if (sender.valueModels != null) {
        treeAVC_AfterRenderUserReplace('pnlTreeAVCUserReplace', value[0].data.UserName);
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
        HQ.message.show(15, App.txtEmail.fieldLabel);
        return;
    }
    if (HQ.IsRequiredCpny) {
        if (App.txtCpnyID.getValue() == null) {
            HQ.message.show(15, App.txtCpnyID.fieldLabel);
            return;
        }
    }
   
    if (App.cboUserTypes.getValue() == null)
    {
        HQ.message.show(15, App.cboUserTypes.fieldLabel);
        return;
    }
    Check();
    //save();
};


/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {

    var user = App.txtUserName.getValue();
    if (App.chkAuto.getValue() == false)
    {
        if (!HQ.util.checkSpecialChar(user.trim())) {
            HQ.message.show(20140811, App.txtUserName.fieldLabe);
            return;

        }
    }
    if (HQ.IsRequiredCpny) {
        if (Ext.isEmpty(App.txtCpnyID.getValue())) {
            HQ.message.show(15, App.txtCpnyID.fieldLabel);
            return;
        }
    }
    if (Ext.isEmpty(App.cboStatus.getValue())) {
        HQ.message.show(15, App.cboStatus.fieldLabel);
        return;
    }
    if (!HQ.IsShowUserTypes) {
        if (Ext.isEmpty(App.cboUserTypes.value[0])) {
            HQ.message.show(15, App.cboUserTypes.fieldLabel);
            return;
        }
    }
    if (Ext.isEmpty(App.dtpStartWork.getValue())) {
        HQ.message.show(15, App.dtpStartWork.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.dtpEndWork.getValue())) {
        HQ.message.show(15, App.dtpEndWork.fieldLabel);
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
            valueTstamp: _tstamp,
            valueCheckFirstLogin: App.ckbCheckFirstLogin.getValue(),
            Auto: App.chkAuto.getValue(),
            valueMultiLogin: App.ckbMultiLogin.getValue()
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

var treeAVC_AfterRender = function (id, userNameOld) {
    App.direct.SA03001_GetTreeData(id, userNameOld, {
        success: function (result) {

        }
    });
};

var treeAVC_AfterRenderUserReplace = function (id, userNameNew) {
    App.direct.SA03001_GetTreeDataUserReplace(id, userNameNew, {
        success: function (result) {

        }
    });
};
var treePanelAVC_checkChange = function (node, checked) {
    if (node.hasChildNodes()) {
        node.eachChild(function (childNode) {
            childNode.set('checked', checked);
            treePanelAVC_checkChange(childNode, checked);
        });
    }    
};
var getDeepAllLeafNodes = function (node, onlyLeaf) {
    var allNodes = new Array();
    if (!Ext.value(node, false)) {
        return [];
    }
    if (node.isLeaf()) {
        return node;
    } else {
        allNodes = allNodes.concat(node.childNodes);
    }
    return allNodes;
};
var getSelected = function () {
    dsr = [];
    var allNodes = getDeepAllLeafNodes(App.treeAVC.getRootNode(), true);
        allNodes.forEach(function (node) {
            if (node.data.checked) {
                if (node.data.Type != 'B') {
                    var index = dsr.indexOf(node.data.Data);
                    if (index == -1) {
                        dsr.push(node.data.Data);
                    }
                }

                if (node.childNodes.length > 0) {
                    getChildSelected(node);
                }
            }
        });
}

var getChildSelected = function (node) {
    for (var i = 0; i < node.childNodes.length; i++) {
        if (node.childNodes[i].data.checked) {
            if (node.childNodes[i].data.Type != 'B') {
                var index = dsr.indexOf(node.childNodes[i].data.Data);
                if (index == -1) {
                    dsr.push(node.childNodes[i].data.Data);
                }
            }
        }

        if (node.childNodes[i].childNodes.length) {
            getChildSelected(node.childNodes[i]);
        }
    }
}
var btnExpand_click = function (btn, e, eOpts) {
    App.treeAVC.expandAll();
};

var btnCollapse_click = function (btn, e, eOpts) {
    App.treeAVC.collapseAll();
};
var btnUserReplaceExpand_click = function (btn, e, eOpts) {
    App.treeAVCUserReplace.expandAll();
};

var btnUserReplaceCollapse_click = function (btn, e, eOpts) {
    App.treeAVCUserReplace.collapseAll();
};
function Check() {
    
    App.frmMain.isValid();
    App.txtUserName.isValid();
    if (HQ.form.checkRequirePass(App.frmDetail))
    {
        if (HQ.GroupAdmin == "1" && HQ.TextValAdmin != '0') {
            var decimal = new RegExp("^(?=.*\\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\\s).{" + HQ.TextValAdmin + ",}$", "");

            if (!App.txtPassWord.value.match(decimal)) {
                HQ.message.show(20180111, [HQ.TextValAdmin], null, true);
                App.txtPassWord.focus();
                return;
            }
        }
        else if (HQ.GroupAdmin == "0" && HQ.TextVal != '0') {
            //var decimal = /^(?=.*\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\s).{6,}$/;
            var decimal = new RegExp("^(?=.*\\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\\s).{" + HQ.TextVal + ",}$", "");

            if (!App.txtPassWord.value.match(decimal)) {
                HQ.message.show(998, [HQ.TextVal], null, true);
                App.txtPassWord.focus();
                return;
            }
        }
        save();
    }
};
var focusOnInvalidField = function (item) {
    if (item == "ok") {
        App.frmMain.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                field.focus();
                return false;
            }
        });
    }
};
var showFieldInvalid = function (form) {
    var done = 1;
    form.getForm().getFields().each(function (field) {
        if (!field.isValid()) {
            HQ.message.show(15, field.fieldLabel, 'focusOnInvalidField');
            done = 0;
            return false;
        }
    });
    return done;
};
function chkAutoChange() {
    if (App.chkAuto.checked) {
        App.txtUserName.setValue('');
        App.txtUserName.allowBlank = true
        App.txtUserName.setDisabled(true);
        App.txtUserName.isValid();
    }
    else {
        App.txtUserName.setDisabled(false);
    }
};
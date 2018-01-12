var keys = ['GroupID'];
var fieldsCheckRequireUserGroup = ["GroupID"];
var fieldsLangCheckRequireUserGroup = ["GroupID"];

var keys1 = ['GroupID'];
var fieldsCheckRequireUserCompany = ["GroupID"];
var fieldsLangCheckRequireUserCompany = ["GroupID"];

var _focusNo = 0;

// Declare
var _beginStatus = "H";
var _Source = 0;
var _maxSource = 9;
var _isLoadMaster = false;
var UserID = '';

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoUser.reload();
        HQ.common.showBusy(false);
        //App.cboBranchID.setValue(HQ.cpnyID);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    App.fupImages.button.btnEl.setWidth(90);
    App.fupImages.button.setWidth(90);

    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboUserID.getStore().addListener('load', checkLoad);
    App.CpnyIDHand.getStore().addListener('load', checkLoad);
    App.UserTypes.getStore().addListener('load', checkLoad);
    App.Channel.getStore().addListener('load', checkLoad);
    App.Manager.getStore().addListener('load', checkLoad);
    App.Department.getStore().addListener('load', checkLoad);
    App.HomeScreenNbr.getStore().addListener('load', checkLoad);
    App.cboGroupIDGroup.getStore().addListener('load', checkLoad);
    App.cboGroupIDCompany.getStore().addListener('load', checkLoad);
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    if (App.stoUser.isLoading()) return;
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.combo.first(App.cboUserID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.first(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.first(App.grdSYS_UserCompany);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboUserID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.prev(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.prev(App.grdSYS_UserCompany);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboUserID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.next(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.next(App.grdSYS_UserCompany);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboUserID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.last(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.last(App.grdSYS_UserCompany);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "new":
            if (_focusNo == 0) {
                if (!HQ.isChange) {
                    UserID = '';
                    App.cboUserID.setValue('');
                    App.cboUserID.focus();
                }
                else {
                    HQ.message.show(150, '', 'refresh');
                }
            }
            else if (_focusNo == 1) {
                if (HQ.isInsert) {
                    HQ.grid.insert(App.grdSYS_UserGroup);
                }
            }
            else if (_focusNo == 2) {
                if (HQ.isInsert) {
                    HQ.grid.insert(App.grdSYS_UserCompany);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (_focusNo == 0) {
                    if (App.cboUserID.value) {
                        HQ.message.show(11, '', 'deleteData');
                    } else {
                        menuClick('new');
                    }
                }
                else if (_focusNo == 1) {
                    if (App.slmSYS_UserGroup.selected.items[0] != undefined) {
                        if (App.slmSYS_UserGroup.selected.items[0].data.GroupID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_UserGroup)], 'deleteData', true);
                        }
                    }
                }
                else if (_focusNo == 2) {
                    if (App.slmSYS_UserCompany.selected.items[0] != undefined) {
                        if (App.slmSYS_UserCompany.selected.items[0].data.GroupID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_UserCompany)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.findInStore(App.grdSYS_UserGroup.store, ['GroupID'], ['Admin'])!=undefined && HQ.TextValAdmin != '0') {
                    var decimal = new RegExp("^(?=.*\\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\\s).{" + HQ.TextValAdmin + ",}$", "");

                    if (!App.Password.value.match(decimal)) {
                        HQ.message.show(20180111, [HQ.TextValAdmin],null,true);
                        App.Password.focus();
                        break;
                    }
                }
                else if (HQ.TextVal != '0') {
                    //var decimal = /^(?=.*\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\s).{6,}$/;
                    var decimal = new RegExp("^(?=.*\\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\\s).{" + HQ.TextVal + ",}$", "");

                    if (!App.Password.value.match(decimal)) {
                        HQ.message.show(998, [HQ.TextVal], null, true);
                        App.Password.focus();
                        break;
                    }
                }
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.util.checkEmail(App.Email.value) && HQ.store.checkRequirePass(App.stoSYS_UserGroup, keys, fieldsCheckRequireUserGroup, fieldsLangCheckRequireUserGroup)
                        && HQ.store.checkRequirePass(App.stoSYS_UserCompany, keys, fieldsCheckRequireUserCompany, fieldsLangCheckRequireUserCompany)) {
                    if (App.chkAuto.getValue() == true)
                        save();
                    else {
                        if (HQ.util.checkSpecialChar(App.cboUserID.getValue()) == true) {
                            save();
                        }
                        else {
                            HQ.message.show(20140811, App.cboUserID.fieldLabel);
                            App.cboUserID.focus();
                            App.cboUserID.selectText();
                        }
                    }
                }
            }
            break;
        case "print":
            break;
        case "close":
            break;
    }
};

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlUserGroup') {
            _focusNo = 1;
        }
        else if (cmd.id == 'pnlUserCompany') {
            _focusNo = 2;
        }
        else {//pnlHeader
            _focusNo = 0;
        }
    });
};

var CpnyIDHand_Change = function (sender, value) {
    //if (sender.valueModels.length > 0) {
    //App.CpnyID.setValue(value);
    if (sender.hasFocus)
        GetAllCompany();
    //}
};
var cboUserID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoUser.loading) {
        UserID = value;
        App.stoUser.reload();
    }
};

var cboUserID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoUser.loading) {
        UserID = value;
        App.stoUser.reload();
    }
};

var cboUserID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboUserID.collapse();
    }
};

var cboUserID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }

};

function chkAutoChange() {
    if (App.chkAuto.checked) {
        App.cboUserID.setValue('');
        App.cboUserID.setDisabled(true);
    }
    else {
        App.cboUserID.setDisabled(false);
    }
};

var chkPublic_Change = function (checkbox, checked) {
    if (checked) {
        App.tabDetail.closeTab(App.pnlPO_PriceCpny);
    }
    else {
        App.tabDetail.addTab(App.pnlPO_PriceCpny);
    }
};

/////////////////////////////////
//Image
var btnClearImage_click = function (btn, eOpts) {
    App.fupImages.reset();
    App.imgImages.setImageUrl("");
    App.hdnImages.setValue("");
};

var fupImages_change = function (fup, newValue, oldValue, eOpts) {
    if (fup.value) {
        var ext = fup.value.split(".").pop().toLowerCase();
        if (ext == "jpg" || ext == "png" || ext == "gif") {
            App.hdnImages.setValue(fup.value);
            readImage(fup, App.imgImages);
        }
        else {
            HQ.message.show(148, '', '');
        }
    }
};

var frmChange = function () {
    if (App.stoUser.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoUser) == false ? (HQ.store.isChange(App.stoSYS_UserGroup)
                                                 == false ? (HQ.store.isChange(App.stoSYS_UserCompany)) : true) : true;
    HQ.common.changeData(HQ.isChange, 'SA00300');
    if (App.cboUserID.valueModels == null || HQ.isNew == true)
        App.cboUserID.setReadOnly(false);
    else App.cboUserID.setReadOnly(HQ.isChange);
};

//Truoc khi load store se hien Busy
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
    HQ.common.lockItem(App.frmMain, false);
    HQ.common.setForceSelection(App.frmMain, false, "cboUserID");
    HQ.isNew = false;
    App.cboUserID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "UserID");
        record = sto.getAt(0);
        HQ.isNew = true;
        App.cboUserID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);           
        App.cboUserID.focus(true);
        sto.commitChanges();
    }

    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.stoSYS_UserGroup.reload();
    App.fupImages.reset();
    if (record.data.Images) {
        displayImage(App.imgImages, record.data.Images);
    }
    else {
        App.imgImages.setImageUrl("");
    }

    if (!HQ.isInsert && HQ.isNew) {
        App.cboUserID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
        App.fupImages.setDisabled(true);
    }
};

//grd SYS_UserGroup
var stoSYS_UserGroup_Load = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
    }
    App.stoSYS_UserCompany.reload();
};

var grdSYS_UserGroup_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSYS_UserGroup_Edit = function (item, e) {
    if (e.field == "GroupID") {
        var selectedRecord = App.cboGroupIDGroup.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("Descr", "");
        }
    }
    HQ.grid.checkInsertKey(App.grdSYS_UserGroup, e, keys);
    frmChange();
};

var grdSYS_UserGroup_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_UserGroup, e, keys);
};

var grdSYS_UserGroup_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_UserGroup);
    frmChange();
};


//grd SYS_UserCompany
var stoSYS_UserCompany_Load = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys1);
        }
        HQ.isFirstLoad = false;
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
};

var grdSYS_UserCompany_BeforeEdit = function (editor, e) {
    //HQ.grid.checkInsertKey(App.grdSYS_UserCompany, e, keys1);
    if (!HQ.grid.checkBeforeEdit(e, keys1)) return false;
};

var grdSYS_UserCompany_Edit = function (item, e) {
    if (e.field == "GroupID") {
        var selectedRecord = App.cboGroupIDCompany.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("ListCpny", selectedRecord.data.ListCpny);
            e.record.set("Descr", selectedRecord.data.Descr);
            GetAllCompany();
        }
        else {
            e.record.set("ListCpny", "");
            e.record.set("Descr", "");
        }

    }
    HQ.grid.checkInsertKey(App.grdSYS_UserCompany, e, keys1);
    frmChange();
};

var GetAllCompany = function () {
    var strCpny = App.CpnyIDHand.getRawValue().split(', ');
    if (strCpny.length == 1 && Ext.isEmpty(strCpny[0]))
        strCpny = [];
    var strTemp = '';
    var store = App.stoSYS_UserCompany;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (!Ext.isEmpty(record.data.GroupID)) {
            var strGrid = record.data.ListCpny.split(',');
            strGrid.forEach(function (item) {
                if (strCpny.indexOf(item) == -1) {
                    strCpny.push(item);
                }
            });
        }
    });
    App.CpnyID.setValue(strCpny);
};

var grdSYS_UserCompany_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_UserCompany, e, keys1);
};

var grdSYS_UserCompany_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_UserCompany);
    frmChange();
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var checkGrid = function (store, field) {
    var count = 0;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data[field]) {
            count++;
            return false;
        }
    });
    if (count > 0)
        return true;
    else
        return false;
};


var save = function () {

    if (checkGrid(App.stoSYS_UserCompany, 'GroupID') == false && Ext.isEmpty(App.CpnyIDHand.getValue())) {
        HQ.message.show(2016062701);
        return;
    }
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA00300/Save',
            params: {
                lstUser: Ext.encode(App.stoUser.getRecordsValues()),
                lstSYS_UserCompany: HQ.store.getData(App.stoSYS_UserCompany),
                lstSYS_UserGroup: HQ.store.getData(App.stoSYS_UserGroup),
                Auto: App.chkAuto.getValue()
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                HQ.message.show(201405071);
                App.chkAuto.setValue(false);
                UserID = data.result.UserID;
                App.cboUserID.getStore().load({
                    callback: function () {
                        if (Ext.isEmpty(App.cboUserID.getValue())) {
                            App.cboUserID.setValue(UserID);
                            App.stoUser.reload();
                        }
                        else {
                            App.cboUserID.setValue(UserID);
                            App.stoUser.reload();
                        }
                    }
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.fupImages.reset();
                App.imgImages.setImageUrl("");
                App.hdnImages.setValue("");
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        if (_focusNo == 0) {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'SA00300/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        App.cboUserID.getStore().load();
                        menuClick("new");
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (_focusNo == 1) {
            App.grdSYS_UserGroup.deleteSelected();
        }
        else if (_focusNo == 2) {
            App.grdSYS_UserCompany.deleteSelected();
            GetAllCompany();
        }
    }
};

var displayImage = function (imgControl, fileName) {
    Ext.Ajax.request({
        url: 'SA00300/ImageToBin',
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        params: JSON.stringify({
            fileName: fileName
        }),
        success: function (result) {
            var jsonData = Ext.decode(result.responseText);
            if (jsonData.imgSrc) {
                imgControl.setImageUrl(jsonData.imgSrc);
            }
            else {
                imgControl.setImageUrl("");
            }
        },
        failure: function (errorMsg, data) {
            HQ.message.process(errorMsg, data, true);
        }
    });
};

var readImage = function (fup, imgControl) {
    var files = fup.fileInputEl.dom.files;
    if (files && files[0]) {
        var FR = new FileReader();
        FR.onload = function (e) {
            imgControl.setImageUrl(e.target.result);
        };
        FR.readAsDataURL(files[0]);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoUser.reload();
    }
};

var checkExpireDay = function () {
    if (App.ExpireDay.value < 1) {
        App.ExpireDay.setValue(1)
    }
};

var checkCrtLmt = function () {
    if (App.CrtLmt.value < 0) {
        App.CrtLmt.setValue(0)
    }
};

var checkFailedLoginCount = function () {
    if (App.FailedLoginCount.value < 0) {
        App.FailedLoginCount.setValue(0)
    }
};
var checkCrtLmtInvoice = function () {
    if (App.CrtLmtInvoice.value < 0) {
        App.CrtLmtInvoice.setValue(0)
    }
};
///////////////////////////////////
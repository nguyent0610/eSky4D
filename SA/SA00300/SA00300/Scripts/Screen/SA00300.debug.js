
var keys = ['GroupID'];
var fieldsCheckRequireUserGroup = ["GroupID"];
var fieldsLangCheckRequireUserGroup = ["GroupID"];


var keys1 = ['GroupID'];
var fieldsCheckRequireUserCompany = ["GroupID"];
var fieldsLangCheckRequireUserCompany = ["GroupID"];

var _focusNo = 0;

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboUserID.getStore().load(function () {
        App.stoUser.reload();
    });
};

var loadComboGrid = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboGroupIDGroup.getStore().load(function () {
        App.cboGroupIDCompany.getStore().load(function () {
            App.stoSYS_UserGroup.reload();
            //App.stoSYS_UserCompany.reload();
            HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
        })
    });
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

var menuClick = function (command) {
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
                App.cboUserID.setValue("");
                App.stoSYS_UserGroup.reload();
                App.stoSYS_UserCompany.reload();
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
            if (_focusNo == 0) {
                if (App.cboUserID.value) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                } else {
                    menuClick('new');
                }
            }
            else if (_focusNo == 1) {
                if (App.slmSYS_UserGroup.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }
            else if (_focusNo == 2) {
                if (App.slmSYS_UserCompany.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.TextVal == '1') {
                    var decimal = /^(?=.*\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\s).{6,}$/;
                    if (!App.Password.value.match(decimal)) {
                        HQ.message.show(998, '', null);
                        App.Password.focus();
                        break;
                    }
                }
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.util.checkEmail(App.Email.value) && HQ.store.checkRequirePass(App.stoSYS_UserGroup, keys, fieldsCheckRequireUserGroup, fieldsLangCheckRequireUserGroup)
                        && HQ.store.checkRequirePass(App.stoSYS_UserCompany, keys, fieldsCheckRequireUserCompany, fieldsLangCheckRequireUserCompany)) {
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

var focusChecking = function (item) {
    if (App[invalidField]) {
        App[invalidField].focus();
    }
};

var tabDetail_AfterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 100);
    }
};

var cboUserID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoUser.reload();
        chkAutoChange();
    }
};
var cboUserID_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoUser.loading) {
        App.stoUser.reload();
    }
};
//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboUserID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboUserID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboUserID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }

};

function chkAutoChange() {
    //if (App.chkAuto.checked) {
    //    if (App.cboUserID.getValue() == null || App.cboUserID.getValue() == '') {
    //        App.cboUserID.setDisabled(true);
    //    }
    //    else {
    //        App.cboUserID.setDisabled(false);
    //        App.chkAuto.checked
    //    }
    //}
    //else {
    //    App.cboUserID.setDisabled(false);
    //}
    if (App.chkAuto.checked) {
        App.cboUserID.setValue('');
        App.cboUserID.setDisabled(true);
    }
    else {
        App.cboUserID.setDisabled(false);
    }
};

var firstLoad = function () {
    HQ.isFirstLoad = true;
    loadSourceCombo();
};

////////////Kiem tra combo chinh CpnyID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = (HQ.store.isChange(App.stoUser) == false ? HQ.store.isChange(App.stoSYS_UserGroup) : true) || (HQ.store.isChange(App.stoUser) == false ? HQ.store.isChange(App.stoSYS_UserCompany) : true);
    HQ.common.changeData(HQ.isChange, 'SA00300');
    if (App.cboUserID.valueModels == null || HQ.isNew == true)
        App.cboUserID.setReadOnly(false);
    else App.cboUserID.setReadOnly(HQ.isChange);
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var stoLoad = function (sto) {
    //HQ.isFirstLoad = true;
    HQ.isNew = false;
    HQ.common.showBusy(false);

    App.cboUserID.forceSelection = false;
    App.CpnyID.forceSelection = false;
    App.UserTypes.forceSelection = false;
    App.Channel.forceSelection = false;
    App.Manager.forceSelection = false;
    App.Department.forceSelection = false;
    App.HomeScreenNbr.forceSelection = false;

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "UserID");
        record = sto.getAt(0);

        HQ.isNew = true;//record la new    
        App.cboUserID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboUserID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    loadComboGrid();
    // display image
    if (record.data.Images) {
        displayImage(App.imgImages, record.data.Images);
    }
    else {
        App.imgImages.setImageUrl("");
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

var cboInvtID_Change = function (item, newValue, oldValue) {

};

//grd SYS_UserGroup
var stoSYS_UserGroup_Load = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        //HQ.isFirstLoad = false;
    }
    App.stoSYS_UserCompany.reload();
    frmChange();
    HQ.common.showBusy(false);
};

var grdSYS_UserGroup_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSYS_UserGroup_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_UserGroup, e, keys);
    frmChange();
    if (e.field == "GroupID") {
        var selectedRecord = App.cboGroupIDGroup.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("Descr", "");
        }
    }

};

var grdSYS_UserGroup_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSYS_UserGroup, e, keys);
};

var grdSYS_UserGroup_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
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
    frmChange();
    HQ.common.showBusy(false);
};

var grdSYS_UserCompany_BeforeEdit = function (editor, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_UserCompany, e, keys1);

};

var grdSYS_UserCompany_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_UserCompany, e, keys1);
    frmChange();
    if (e.field == "GroupID") {
        var selectedRecord = App.cboGroupIDCompany.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("ListCpny", selectedRecord.data.ListCpny);
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("ListCpny", "");
            e.record.set("Descr", "");
        }
    }

};

var grdSYS_UserCompany_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSYS_UserCompany, e, keys1);
};

var grdSYS_UserCompany_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSYS_UserCompany);
    frmChange();
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA00300/Save',
            params: {
                lstUser: HQ.store.getData(App.frmMain.getRecord().store),
                lstSYS_UserCompany: HQ.store.getData(App.stoSYS_UserCompany),
                lstSYS_UserGroup: HQ.store.getData(App.stoSYS_UserGroup),
                Auto: App.chkAuto.getValue()
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.chkAuto.setValue(false);
                //refresh("yes");
                var UserID = data.result.UserID;
                App.cboUserID.getStore().load(function () {
                    App.cboUserID.setValue(UserID);
                    App.stoUser.reload();
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

var btnClearImage_click = function (btn, eOpts) {
    App.fupImages.reset();
    App.imgImages.setImageUrl("");
    App.hdnImages.setValue("");
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        //var userid = '';
        //userid = App.cboUserID.getValue();
        //App.cboUserID.getStore().load(function () {
        //    App.cboUserID.setValue(userid);

        //});
        App.stoUser.reload();
    }
};
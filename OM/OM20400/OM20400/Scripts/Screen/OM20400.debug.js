var keys = ['GroupID'];
var keysHeader = ['UserName'];

var fieldsCheckRequireHeader = ["FirstName"];//, "LastName", "Address", "Name", "Email", "Tel", "Password", "PasswordQuestion", "PasswordAnswer", "CpnyID", "UserTypes", "Channel", "ExpireDay"];
var fieldsLangCheckRequireHeader = ["FirstName"];//, "LastName", "Address", "Name", "Email", "Tel", "Password", "PasswordQuestion", "PasswordAnswer", "CpnyID", "UserTypes", "Channel", "ExpireDay"];

var fieldsCheckRequireUserGroup = ["GroupID"];
var fieldsLangCheckRequireUserGroup = ["GroupID"];

var fieldsCheckRequireUserCompany = ["GroupID"];
var fieldsLangCheckRequireUserCompany = ["GroupID"];

var _focusNo = 0;

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
                App.cboUserID.setValue(App.cboUserID.getStore().first().get('UserName'));
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
                var combobox = App.cboUserID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboUserID.setValue(combobox.store.getAt(index - 1).data.UserName);
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
                var combobox = App.cboUserID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboUserID.setValue(combobox.store.getAt(index + 1).data.UserName);
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
                App.cboUserID.setValue(App.cboUserID.getStore().last().get('UserName'));
            }
            else if (_focusNo == 1) {
                HQ.grid.last(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.last(App.grdSYS_UserCompany);
            }
            break;
        case "refresh":
            App.stoUser.load();
            App.stoSYS_UserGroup.reload();
            App.stoSYS_UserCompany.reload();
            break;
        case "new":
            if (_focusNo == 0) {
                App.cboUserID.setValue("");
                App.stoSYS_UserGroup.reload();
                App.stoSYS_UserCompany.reload()
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
                //if (checkRequire(App.stoSYS_UserGroup.getChangedData().Created) && checkRequire(App.stoSYS_UserGroup.getChangedData().Updated)
                //    || checkRequire(App.stoSYS_UserCompany.getChangedData().Created) && checkRequire(App.stoSYS_UserCompany.getChangedData().Updated)
                //    ) {
                //if (App.frmMain.isValid()) {
                //App.frmMain.updateRecord();

                if (HQ.form.checkRequirePass(App.frmMain) && HQ.util.checkEmail(App.Email.value) && HQ.store.checkRequirePass(App.stoSYS_UserGroup, keys, fieldsCheckRequireUserGroup, fieldsLangCheckRequireUserGroup)
                        && HQ.store.checkRequirePass(App.stoSYS_UserCompany, keys, fieldsCheckRequireUserCompany, fieldsLangCheckRequireUserCompany)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (HQ.store.isChange(App.stoUser) || HQ.store.isChange(App.stoSYS_UserCompany) || HQ.store.isChange(App.stoSYS_UserGroup)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }
};

var focusChecking = function (item) {
    if (App[invalidField]) {
        App[invalidField].focus();
    }
};

var tabOM20400_AfterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 100);
    }
};

var cboCpnyID_Change = function (item, newValue, oldValue) {
    App.stoSYS_Company.load();
    //App.grdSYS_UserGroup.store.reload();
    //App.grdSYS_UserCompany.store.reload();
};


var frmloadAfterRender = function (obj) {
    App.stoUser.load();
    App.stoSYS_UserGroup.load();
    App.stoSYS_UserCompany.load();
};

var loadData = function () {
    var record = App.stoUser.getAt(0);
    if (record) {
        // Edit record
        App.frmMain.getForm().loadRecord(record);

        if (record.data.Images) {
            App.direct.OM20400GetImages(record.data.Images);
        } else {
            App.imgPPCStorePicReq.setImageUrl("");
        }

    } else {
        // If has no record then create a new
        App.stoUser.insert(0, Ext.data.Record());
        App.frmMain.getForm().loadRecord(App.stoUser.getAt(0));
        App.imgPPCStorePicReq.setImageUrl("");
    }


    //if (App.stoUser.getCount() == 0) {
    //    App.stoUser.insert(0, Ext.data.Record());
    //}
    //App.frmMain.getForm().loadRecord(App.stoUser.getAt(0));
    //App.direct.OM20400GetImages(App.stoUser.getAt(0).data.Images);
};

function UploadImage() {
    App.frmMain.submit({
        waitMsg: 'Uploading your file...',
        url: 'OM20400/OM20400Upload',
        success: function (result) {
            //
        },
        failure: function (error) {
            //
        }
    });
};

var NamePPCStorePicReq_Change = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "jpg" || ext == "png" || ext == "gif") {
        UploadImage();
        var curRecord = App.frmMain.getRecord();
        curRecord.data.Images = App.imgPPCStorePicReq.imageUrl;
        curRecord.setDirty();
    } else {
        alert("Please choose a picture! (.jpg, .png, .gif)");
        sender.reset();
    }
};

var btnClearImage_Click = function (sender, e) {
    App.imgPPCStorePicReq.setImageUrl("");
    var curRecord = App.frmMain.getRecord();
    curRecord.data.Images = "";
    curRecord.setDirty();
};

var chkPublic_Change = function (checkbox, checked) {
    if (checked) {
        App.tabOM20400.closeTab(App.pnlPO_PriceCpny);
    }
    else {
        App.tabOM20400.addTab(App.pnlPO_PriceCpny);
    }
};

var cboInvtID_Change = function (item, newValue, oldValue) {

};

//grd SYS_UserGroup
var grdSYS_UserGroup_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSYS_UserGroup_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_UserGroup, e, keys);

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
};

//grd SYS_UserCompany
var grdSYS_UserCompany_BeforeEdit = function (editor, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_UserCompany, e, keys);
};

var grdSYS_UserCompany_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSYS_UserCompany, e, keys);

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
    return HQ.grid.checkValidateEdit(App.grdSYS_UserCompany, e, keys);
};

var grdSYS_UserCompany_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSYS_UserCompany);
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM20400/Save',
            params: {
                lstUser: HQ.store.getData(App.frmMain.getRecord().store),
                lstSYS_UserCompany: HQ.store.getData(App.stoSYS_UserCompany),
                lstSYS_UserGroup: HQ.store.getData(App.stoSYS_UserGroup)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.cboUserID.getStore().reload();
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
        if (_focusNo == 0) {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'OM20400/DeleteAll',
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

//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};